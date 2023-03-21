<template>
  <div class="loading-container">
    <small class="text-center d-block" style="color:#e3e3e3;line-height:60px;" v-if="!loading && noData">
      {{ lang['noData'] }}
    </small>
    <div v-show="loading" class="loading-spinner">
      <div class="spinner-border" role="status">
        <span class="sr-only">{{ loadingText }}</span>
      </div>      
    </div>
    <div class="chart" v-show="!printView"></div>
  </div>
</template>

<script>
import moment from "moment";
import utils, { agpConstants } from "../services/utils";
import { EventBus } from "../services/event-bus";
import highcharts from "highcharts";
import boost from "highcharts/modules/boost";
require("highcharts/highcharts-more")(highcharts);
boost(highcharts);

export default {
  data() {
    return {
      glucoseTruncateValue: 20,
      chart: null,
      loading: null,
      printView: false
    };
  },
  methods: {
    draw() {
      this.destroy();

      if (this.noData) {
        this.triggerDgpInvalidatedEvent();
        return;
      }

      this.loading = true;
      setTimeout(() => {
        let _this = this;
        this.chart = new highcharts.Chart({
          title: "",
          chart: {
            renderTo: _this.$el.getElementsByClassName("chart")[0],
            height: 80,
            margin: [10, 0, 24, 0],
            spacing: 0,
            events: {
              load() {
                _this.loading = false;
                _this.triggerDgpInvalidatedEvent();
              }
            },
            boost: {
              enabled: true,
              useGPUTranslations: true
            }
          },
          legend: { enabled: false },
          credits: { enabled: false },
          exporting: { enabled: false },
          tooltip: {
            borderWidth: 0,
            shadow: false,
            backgroundColor: "transparent",
            animation: false,
            outside: true,
            useHTML: true,
            headerFormat:
              '<span style="top:0;left:-60px;position:absolute;z-index:20;border:1px solid gray;padding:3px;background:white;border-radius:5px;">',
            footerFormat: "</span>",
            hideDelay: 20
          },
          plotOptions: {
            series: {
              animation: false,
              stickyTracking: false
            },
            arearange: {
              enableMouseTracking: false
            },
            scatter: {
              tooltip: _this.getTooltipGlucose(_this)
            },
            spline: {
              tooltip: _this.getTooltipGlucose(_this)
            },
            line: {
              tooltip: _this.getTooltipGlucose(_this)
            },
            column: {
              tooltip: {
                pointFormatter() {
                  let time = moment()
                    .startOf("day")
                    .add(this.x, "hours")
                    .format("HH:mm");

                  return `<small>${time}, ${this.y.toFixed(0)} ${
                    _this.insulinUnitShort
                  } </small>`;
                }
              }
            }
          },
          yAxis: [
            {
              visible: false,
              min: 0,
              max: this.isDcmView
                ? utils.changeUnit(
                    this.glucoseTruncateValue + 0.1,
                    "mmol/l",
                    this.globalSettings.unit
                  )
                : this.yAxisMaxValue
            },
            {
              visible: false,
              min: 0,
              max: 25
            }
          ],
          xAxis: {
            min: 0,
            max: 24,
            tickPositions: [4, 8, 12, 16, 20],
            labels: {
              style: {
                color: "#333",
                fontSize: "8px"
              }
            }
          },
          series: this.series
        });
      }, Math.floor(Math.random() * 201) + 1);
    },
    triggerDgpInvalidatedEvent() {
      EventBus.$emit("dgp-chart-invalidate-finished");
    },
    getTooltipGlucose(_this) {
      return {
        pointFormatter() {
          let time = moment()
            .startOf("day")
            .add(this.x, "hours")
            .format("HH:mm");

          return `<small>${time}, ${this.originalValue.toFixed(1)} ${
            _this.globalSettings.unit
          } </small>`;
        }
      };
    },
    destroy() {
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
      }
    },
    onGlobalSettingsUpdated(x) {
      let redraw = false;
      if (
        x.difference.dgpChartType ||
        x.difference.displayNonMeasurableValues ||
        x.difference.unit ||
        x.difference.colorBasal ||
        x.difference.colorBolus ||
        x.difference.colorMix ||
        x.difference.viewMode ||
        x.difference.curveType
      ) {
        redraw = true;
      }

      if (!redraw) {
        for (let propertyName in x.difference) {
          if (propertyName.indexOf("insulinType-") > -1) {
            redraw = true;
          }
        }
      }

      if (redraw) {
        this.draw();
      }

      redraw = null;
    },
    onPrint(e) {
      this.printView = e == "open";
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    insulinUnitShort() {
      return this.$i18n.t(
        "insulinUnitShort",
        this.globalSettings.currentLanguage
      );
    },
    series() {
      var series = [
        // target range
        {
          name: "target range",
          type: "arearange",
          data: [
            [
              0,
              agpConstants[this.globalSettings.unit].lowLimit,
              agpConstants[this.globalSettings.unit].highLimit
            ],
            [
              24,
              agpConstants[this.globalSettings.unit].lowLimit,
              agpConstants[this.globalSettings.unit].highLimit
            ]
          ],
          lineWidth: 2,
          fillColor: "rgba(230, 254, 229, 0.75)",
          color: "lightgreen",
          marker: {
            enabled: false
          },
          yAxis: 0,
          connectNulls: true
        }
      ];

      if (this.isDcmView) {
        this.insulinTypes.forEach((insulinType, i) => {
          series.push({
            color: insulinType.color,
            name: insulinType.type,
            type: "column",
            pointWidth: 2,
            yAxis: 1,
            // Adding points at 0 and 24, so the chart width will always be 100%
            data: [[0, 0]]
              .concat(
                this.insulinValues
                  .filter(insulin => insulin.type == insulinType.type)
                  .map(insulin => {
                    return [
                      insulin.date.getHours() + insulin.date.getMinutes() / 60,
                      insulin.unit
                    ];
                  })
              )
              .concat([[24, 0]])
              .sort((a, b) => {
                return a[0] < b[0] ? -1 : 1;
              }),
            connectNulls: true
          });
        });
      }

      series.push(
        // glucose values
        {
          color: "#555",
          data: this.glucoseValues.map(x => {
            let value = x.value;
            if (
              this.isDcmView &&
              utils.changeUnit(value, this.originalUnit, "mmol/l") >
                this.glucoseTruncateValue
            ) {
              value = this.glucoseTruncateValue;
            }

            return {
              x: x.date.getHours() + x.date.getMinutes() / 60,
              y: utils.changeUnit(value, "mmol/l", this.globalSettings.unit),
              originalValue: utils.changeUnit(
                x.value,
                this.originalUnit,
                this.globalSettings.unit
              )
            };
          }),
          type:
            this.globalSettings.dgpChartType == "scatter"
              ? "scatter"
              : this.globalSettings.curveType == "function"
              ? "spline"
              : "line",
          marker: {
            enabled: true,
            symbol:
              this.globalSettings.dgpChartType == "scatter"
                ? "diamond"
                : "circle",
            radius: 3,
            yAxis: 0
          },
          connectNulls: true
        }
      );

      return series;
    },
    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },
    noData() {
      return (
        !this.glucoseValues.length &&
        ((this.isDcmView && !this.insulinValues.length) || !this.isDcmView)
      );
    },
    lang() {
      return this.$i18n.t("dpgCharts", this.globalSettings.currentLanguage);
    },
    loadingText() {
      return this.$i18n.t("loadingText", this.globalSettings.currentLanguage);
    }
  },
  mounted() {
    this.draw();

    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$on("print", this.onPrint);
  },
  beforeDestroy() {
    this.destroy();

    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$off("print", this.onPrint);
  },
  props: [
    "glucoseValues",
    "insulinValues",
    "insulinTypes",
    "originalUnit",
    "yAxisMaxValue"
  ]
};
</script>
