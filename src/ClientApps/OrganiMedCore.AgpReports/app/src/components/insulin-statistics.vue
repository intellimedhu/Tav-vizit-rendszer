<template>
  <div class="row statistic-row insulin-statistic-row print-content-diagrams">
    <statistics-template :title="lang['mainTitle']" :blocks="blocks">
      <template :slot="`block-header-${data.brandIndex}`" v-for="data in statisticsValuable">
        <span class="stat-block-header" v-bind:key="data.brandIndex" :style="`color:${data.color}`">
          {{ data.name }} - {{ data.type }}
        </span>
      </template>
    </statistics-template>

    <div class="col-lg my-4 my-md-5 my-lg-0 loading-container">
      <div class="loading-spinner d-print-none text-primary" v-show="loading">
        <div class="spinner-border" role="status">
          <span class="sr-only">{{ loadingText }}</span>
        </div>
      </div>
      <div class="row">
        <div class="col col-insulin-chart mx-auto mr-sm-0 m-lg-0">
          <div :id="chartContainerId" v-show="!noData"></div>
        </div>
        <div class="col col-insulin-legend mx-auto ml-sm-0" v-show="!loading">
          <div class="insulin-legend">
            <div v-for="data in statisticsValuable" v-bind:key="data.brandIndex" class="legend-item my-1">
              <i class="sign mr-1" :style="`background-color:${data.color}`"></i>
              <strong class="percent mx-1">{{ percentages[data.brandIndex] }}%</strong>
              <span class="unit mx-1">
                ({{ data.sumUnit | round(1) }} <span v-show="data.sumUnit">{{ insulinUnitShort }}</span>)
              </span>
              <strong :style="`color:${data.color}`">{{ data.name }} - {{ data.type }}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import utils, { countByDate } from "../services/utils";
import { EventBus } from "../services/event-bus";
import statisticsTemplate from "./statistics-template";
import highcharts from "highcharts";
require("highcharts/highcharts-more")(highcharts);

export default {
  components: {
    statisticsTemplate
  },
  data() {
    return {
      chartContainerId: "insulin-chart-area",
      chart: null,
      loading: null
    };
  },
  methods: {
    isValuable(statisticalData) {
      return !!(
        statisticalData.avgDuration ||
        statisticalData.sumUnit ||
        statisticalData.avgUnit ||
        statisticalData.avgCountPerDay
      );
    },

    draw() {
      this.destroy();

      if (this.noData) {
        utils.triggerInvalidatedEvent("insulinStatistics");
        return;
      }

      this.loading = true;
      setTimeout(() => {
        let _this = this;
        this.chart = highcharts.chart(this.chartContainerId, {
          title: "",
          chart: {
            type: "pie",
            height: 153,
            width: 148,
            margin: 0,
            events: {
              load() {
                _this.loading = false;
                utils.triggerInvalidatedEvent("insulinStatistics");
              }
            }
          },
          legend: { enabled: false },
          credits: { enabled: false },
          exporting: { enabled: false },
          tooltip: { enabled: false },

          plotOptions: {
            series: {
              shadow: false
            },
            pie: {
              animation: false,
              dataLabels: {
                enabled: false
              }
            }
          },
          series: this.series
        });
      }, 3);
    },

    invalidate() {
      this.$nextTick(this.draw);
    },

    destroy() {
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
      }
    },

    onGlobalSettingsUpdated(x) {
      if (
        x.difference.colorBasal ||
        x.difference.colorBolus ||
        x.difference.colorMix
      ) {
        this.invalidate();
      }
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    lang() {
      return this.$i18n.t(
        "insulinStatistics",
        this.globalSettings.currentLanguage
      );
    },
    loadingText() {
      return this.$i18n.t("loadingText", this.globalSettings.currentLanguage);
    },
    displayedBrands() {
      return this.$store.state.insulinVuexModule.displayedBrands;
    },
    insulinUnitShort() {
      return this.$i18n.t(
        "insulinUnitShort",
        this.globalSettings.currentLanguage
      );
    },
    countUnit() {
      return this.$i18n.t("countUnit", this.globalSettings.currentLanguage);
    },
    secUnit() {
      return this.$i18n.t("secUnit", this.globalSettings.currentLanguage);
    },
    statisticsValuable() {
      let brandIndex = 0;

      return this.insulinValues
        .filter(brand => brand.values.length)
        .map(brand => {
          let insulinType = this.insulinTypes.find(
            x => x.type == brand.insulinType
          );

          let brandValues = {
            brandIndex: brandIndex++,
            name: brand.name,
            type: brand.insulinType,
            avgDuration: null,
            avgUnit: null,
            avgCountPerDay: null,
            avgInjectionPerDay: null,
            sumUnit: null,
            valuable: false,
            color: insulinType.color,
            count: brand.values.length
          };

          if (brand.values.length) {
            let avgDuration =
              brand.values.reduce((a, b) => a + b.duration, 0) /
              brand.values.length;
            brandValues.avgDuration = avgDuration
              ? (avgDuration / 1000.0).toFixed(1)
              : null;

            avgDuration = null;

            brandValues.sumUnit = brand.values.reduce((a, b) => a + b.unit, 0);
            brandValues.avgUnit = (
              brandValues.sumUnit / brand.values.length
            ).toFixed(1);

            let dailyGroups = brand.values.reduce(countByDate, {});
            let sum = 0;
            let length = 0;
            for (let day in dailyGroups) {
              length++;
              sum += dailyGroups[day];
            }

            brandValues.avgCountPerDay = length
              ? (sum / length).toFixed(1)
              : null;

            if (brandValues.avgUnit && brandValues.avgCountPerDay) {
              brandValues.avgInjectionPerDay = (
                brandValues.avgUnit * brandValues.avgCountPerDay
              ).toFixed(1);
            }

            brandValues.valuable = this.isValuable(brandValues);

            dailyGroups = null;
            sum = null;
            length = null;
          }

          return brandValues;
        })
        .filter(data => this.isValuable(data));
    },
    percentages() {
      let result = {};
      this.statisticsValuable.forEach(data => {
        result[data.brandIndex] = utils.getPercentage(
          data.sumUnit,
          this.totalUnit
        );
      });

      return result;
    },
    totalUnit() {
      return this.statisticsValuable.reduce((a, b) => {
        return a + b.sumUnit;
      }, 0);
    },
    noData() {
      return !this.totalUnit;
    },
    series() {
      if (!this.noData) {
        return [
          {
            data: this.statisticsValuable.map(data => {
              let insulinType = this.insulinTypes.find(
                x => x.type == data.type
              );

              return {
                name: data.type,
                y: data.sumUnit,
                color: insulinType ? insulinType.color : null
              };
            })
          }
        ];
      }
    },
    blocks() {
      return this.statisticsValuable.map(data => {
        return {
          id: data.brandIndex,
          rows: [
            {
              label: this.lang["dailyAverages"],
              dataText: `${data.avgInjectionPerDay || "-"} ${
                data.avgInjectionPerDay ? this.insulinUnitShort : ""
              } / ${data.avgCountPerDay || "-"} ${
                data.avgCountPerDay ? this.countUnit : ""
              }`
            },
            {
              label: this.lang["injectionUnitAndDuration"],
              dataText: `${data.avgUnit || "-"} ${
                data.avgUnit ? this.insulinUnitShort : ""
              } / ${data.avgDuration || "-"} ${this.secUnit}`
            }
          ]
        };
      });
    }
  },
  mounted() {
    this.invalidate();

    EventBus.$on("rangeChanged", this.invalidate);
    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
  },
  beforeDestroy() {
    this.destroy();

    EventBus.$off("rangeChanged", this.invalidate);
    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);
  },
  props: ["insulinValues", "insulinTypes"]
};
</script>
