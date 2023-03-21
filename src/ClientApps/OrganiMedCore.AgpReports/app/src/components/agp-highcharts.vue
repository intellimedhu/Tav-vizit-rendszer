<template>
  <div class="row print-content-diagrams">
    <div class="col loading-container agp-chart-container mt-3">
        <div :id="chartContainerId" class="agp-chart" v-if="displayAgp"></div>        
        <div class="loading-spinner" :class="{'hidden': !loading}">
          <div class="spinner-border" role="status">
            <span class="sr-only">{{ loadingText }}</span>
          </div>
        </div>

        <p class="alert alert-warning mt-3 mb-2" v-show="!displayAgp">
          <i class="icon-info-sign"></i>
          {{ lang['averageModeDaysLimitText'] }}
        </p>
    </div>
  </div>
</template>

<script>
import moment from "moment";
import highcharts from "highcharts";
import boost from "highcharts/modules/boost";
require("highcharts/highcharts-more")(highcharts);
import utils, { agpConstants, smbgColors } from "../services/utils";
import { EventBus } from "../services/event-bus";
boost(highcharts);

const groupGlucoseByHours = (accumulator, item) => {
  let itemHours = item.date.getHours();
  let hour = accumulator.find(x => x.hour == itemHours);
  if (!hour) {
    hour = {
      hour: itemHours,
      values: []
    };
    accumulator.push(hour);
  }

  hour.values.push({ date: item.date.getTime(), value: item.value });

  return accumulator;
};

export default {
  data() {
    return {
      chart: null,
      loading: null,
      chartContainerId: "agp-highchart-container",
      percentageLines: [
        {
          name: "lowest",
          value: 0.05,
          options: {
            color: "#99c26b",
            dashStyle: "LongDash",
            linecap: "",
            legendIndex: 60
          }
        },
        {
          name: "low",
          value: 0.25,
          options: {
            color: "#50a9d8",
            legendIndex: 70
          }
        },
        {
          name: "high",
          value: 0.75,
          options: {
            color: "#50a9d8",
            legendIndex: 90
          }
        },
        {
          name: "highest",
          value: 0.95,
          options: {
            color: "#99c26b",
            dashStyle: "LongDash",
            linecap: "",
            legendIndex: 100
          }
        }
      ]
    };
  },
  methods: {
    invalidate() {
      this.destroy();

      if (!this.displayAgp) {
        utils.triggerInvalidatedEvent("agp");        
        return;
      }

      this.loading = true;
      setTimeout(() => {
        this.chart = highcharts.chart(this.chartContainerId, this.chartOptions);
      }, 10);
    },
    destroy() {
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
      }
    },
    getScatterData(predicate) {
      return this.filteredBloodGlucoseValues
        .filter(predicate)
        .map(bg => [
          bg.date.getHours() + bg.date.getMinutes() / 60,
          utils.changeUnit(
            bg.value,
            this.originalUnit,
            this.globalSettings.unit
          )
        ])
        .sort(utils.sortByFirst);
    },
    onRangeChanged() {
      this.$nextTick(this.invalidate);
    },
    onGlobalSettingsUpdated(x) {
      if (
        x.difference.curveType ||
        x.difference.displayAverageLines ||
        x.difference.displayNonMeasurableValues ||
        x.difference.currentLanguage ||
        x.difference.displayInsulinInAGP ||
        x.difference.midlineType ||
        x.difference.unit ||
        x.difference.colorBasal ||
        x.difference.colorBolus ||
        x.difference.colorMix ||
        x.difference.viewMode
      ) {
        this.$nextTick(this.invalidate);
      }
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },
    lang() {
      return this.$i18n.t("agpChart", this.globalSettings.currentLanguage);
    },
    loadingText() {
      return this.$i18n.t("loadingText", this.currentLanguage);
    },
    chartOptions() {
      let _this = this;

      return {
        title: "",
        chart: {
          marginLeft: 110,
          height: 500,
          events: {
            load() {
              _this.loading = false;
              utils.triggerInvalidatedEvent("agp");
            }
          },
          boost: {
            enabled: true,
            useGPUTranslations: true
          }
        },
        exporting: { enabled: false },
        credits: false,
        plotOptions: {
          series: {
            animation: false,
            events: {
              legendItemClick: () => false
            }
          },
          scatter: {
            tooltip: {
              headerFormat: "<b>{series.name}</b><br/>",
              pointFormatter() {
                let time = moment()
                  .startOf("day")
                  .add(this.x, "hours")
                  .format("HH:mm");

                return `${time} – ${this.y.toFixed(1)} ${
                  _this.globalSettings.unit
                }`;
              }
            }
          },
          spline: {
            tooltip: {
              headerFormat: "",
              pointFormatter() {
                return `<b>${Math.floor(this.x)} ${
                  _this.hour
                }: </b> ${this.y.toFixed(1)} ${_this.globalSettings.unit}`;
              }
            }
          },
          line: {
            tooltip: {
              headerFormat: "",
              pointFormatter() {
                return `<b>${Math.floor(this.x)} ${
                  _this.hour
                }: </b> ${this.y.toFixed(1)} ${_this.globalSettings.unit}`;
              }
            }
          },
          column: {
            tooltip: {
              headerFormat: "<b>{series.name}</b><br/>",
              pointFormatter() {
                return `<b>${Math.floor(this.x)} ${
                  _this.hour
                }:</b> ${this.y.toFixed(1)} ${_this.insulinUnitShort}`;
              }
            }
          }
        },
        tooltip: {
          valueDecimals: 1
        },
        xAxis: this.xAxis,
        yAxis: this.yAxis,
        series: this.series
      };
    },
    totalDays() {
      return this.$store.getters.totalDays;
    },
    displayAgp() {
      return this.totalDays <= this.globalSettings.averageModeDaysLimit;
    },
    hour() {
      return this.$i18n.t("hour", this.globalSettings.currentLanguage);
    },
    countUnit() {
      return (
        "[" +
        this.$i18n.t("countUnit", this.globalSettings.currentLanguage) +
        "]"
      );
    },
    insulinUnitTitle() {
      return this.$i18n.t(
        "insulinUnitTitle",
        this.globalSettings.currentLanguage
      );
    },
    insulinUnitShort() {
      return this.$i18n.t(
        "insulinUnitShort",
        this.globalSettings.currentLanguage
      );
    },
    ticks25() {
      return Array.from(Array(25), (x, i) => i);
    },
    ticksHour() {
      return this.ticks25.map(hour => `${hour}`.padStart(2, "0"));
    },
    measurementsPerHours() {
      return this.ticks25.map(hour => {
        return this.filteredBloodGlucoseValues.filter(
          x => x.date.getHours() == hour
        ).length;
      });
    },
    xAxis() {
      let xAxis = [
        // glucose
        {
          min: 0,
          max: 24,
          margin: 5,
          title: {
            text: this.lang.axisHour,
            align: "low",
            textAlign: "right",
            margin: -20,
            x: -10,
            y: 5
          },
          gridLineWidth: 1,
          categories: this.ticksHour
        },
        // Count of glucose
        {
          min: 0,
          max: 24,
          margin: 5,
          title: {
            text: this.lang.axisMeasurements + " " + this.countUnit,
            align: "low",
            textAlign: "right",
            margin: -20,
            x: -10,
            y: 5
          },
          categories: this.measurementsPerHours
        }
      ];

      // insulin axis
      if (this.globalSettings.displayInsulinInAGP) {
        xAxis = xAxis.concat(
          this.filteredInsulinValuesByBrands.map(brand => {
            return {
              min: 0,
              max: 24,
              margin: 5,
              title: {
                text: brand.name + " " + this.countUnit,
                align: "low",
                textAlign: "right",
                margin: -20,
                x: -10,
                y: 5
              },
              categories: this.ticks25.map(hour => {
                return brand.values.filter(x => x.date.getHours() == hour)
                  .length;
              })
            };
          })
        );
      }

      return xAxis;
    },
    yAxis() {
      let yAxis = [
        // glucose
        {
          title: {
            text: this.lang.chartTitle,
            style: {
              fontWeight: "bold"
            }
          },
          min: 0,
          tickPositions: this.tickPositionsGlucose,
          gridLineWidth: 1
        }
      ];

      // insulin
      if (this.globalSettings.displayInsulinInAGP) {
        yAxis.push({
          title: {
            text: this.insulinUnitTitle,
            rotation: 270,
            style: {
              fontWeight: "bold"
            }
          },
          min: 0,
          max: 25,
          opposite: true,
          gridLineWidth: 1
        });
      }

      return yAxis;
    },
    tickPositionsGlucose() {
      return utils.getGlucoseTickPositions(
        this.globalSettings.unit,
        this.dailyChartsVMax
      );
    },
    series() {
      let series = [
        // Target range
        {
          name: `${this.lang.targetRangeLegend} (${this.targetRangeMin}-${this.targetRangeMax})`,
          type: "arearange",
          data: [
            [0, this.targetRangeMin, this.targetRangeMax],
            [24, this.targetRangeMin, this.targetRangeMax]
          ],
          lineWidth: 2,
          fillColor: "rgba(230, 254, 229, 0.75)",
          color: "lightgreen",
          marker: {
            enabled: false
          },
          tooltip: {
            headerFormat: "",
            pointFormat: `<strong>${this.lang.targetRangeLegend}: </strong> ${this.targetRangeMin} - ${this.targetRangeMax}`,
            footerFormat: ""
          },
          legendIndex: 200
        }
      ];

      if (this.globalSettings.displayInsulinInAGP) {
        // Insulin averages by hours
        series = series.concat(
          this.filteredInsulinValuesByBrands.map((brand, index) => {
            return {
              name: brand.name + " - " + brand.insulinType,
              type: "column",
              data: brand.values
                .reduce((accumulator, item) => {
                  let itemHours = item.date.getHours();
                  let hour = accumulator.find(x => x.hour == itemHours);
                  if (!hour) {
                    hour = {
                      hour: itemHours,
                      values: []
                    };
                    accumulator.push(hour);
                  }

                  hour.values.push(item.unit);

                  return accumulator;
                }, [])
                .map(hour => [
                  hour.hour,
                  hour.values.reduce((accumulator, unit) => {
                    return (accumulator += unit);
                  }, 0) / hour.values.length
                ])
                .sort(utils.sortByFirst),
              color: this.insulinTypes.find(x => x.type == brand.insulinType)
                .color,
              yAxis: 1,
              xAxis: 2 + index,
              legendIndex: 110 + index * 10
            };
          })
        );
      }

      if (this.isDcmView) {
        series = series.concat([
          // glucose values
          {
            name: this.lang.veryLowValues,
            type: "scatter",
            data: this.getScatterData(bg => bg.agpIsVeryLow),
            marker: {
              symbol: "circle",
              radius: 4
            },
            xAxis: 1,
            color: smbgColors.slow,
            legendIndex: 10
          },
          {
            name: this.lang.lowValues,
            type: "scatter",
            data: this.getScatterData(bg => bg.agpIsLow),
            marker: {
              symbol: "circle",
              radius: 4
            },
            xAxis: 1,
            color: smbgColors.low,
            legendIndex: 20
          },
          {
            name: this.lang.inTargetValues,
            type: "scatter",
            data: this.getScatterData(bg => bg.agpIsInTarget),
            marker: {
              symbol: "circle",
              radius: 4
            },
            xAxis: 1,
            color: smbgColors.target,
            legendIndex: 30
          },
          {
            name: this.lang.highValues,
            type: "scatter",
            data: this.getScatterData(bg => bg.agpIsHigh),
            marker: {
              symbol: "circle",
              radius: 4,
              lineColor: "#e1e70a",
              lineWidth: 1
            },
            xAxis: 1,
            color: smbgColors.high,
            legendIndex: 40
          },
          {
            name: this.lang.veryHighValues,
            type: "scatter",
            data: this.getScatterData(bg => bg.agpIsVeryHigh),
            marker: {
              symbol: "circle",
              radius: 4
            },
            xAxis: 1,
            color: smbgColors.shigh,
            legendIndex: 50
          }
        ]);
      } else {
        series.push({
          name: this.lang.agpScatterValues,
          type: "scatter",
          data: this.getScatterData(bg => true),
          marker: {
            symbol: "diamond",
            radius: 5
          },
          xAxis: 1,
          color: "#6d6e71",
          legendIndex: 10
        });
      }

      // midline
      series.push({
        name: "50%",
        type: this.globalSettings.curveType == "function" ? "spline" : "line",
        data:
          this.globalSettings.midlineType == "avg"
            ? this.averageData
            : this.medianData,
        color: "#f68b1e",
        lineWidth: 3,
        marker: {
          symbol: "circle",
          radius: 4
        },
        xAxis: 1,
        legendIndex: 80
      });

      if (this.globalSettings.displayAverageLines) {
        // percentage values
        series = series.concat(
          this.percentageLines.map(percent => {
            let data = this.glucoseValuesByHours
              .map(x => {
                let valueInUnit = utils.changeUnit(
                  x.values[Math.floor(x.values.length * percent.value)].value,
                  this.originalUnit,
                  this.globalSettings.unit
                );

                return [x.hour + 0.5, valueInUnit];
              })
              .sort(utils.sortByFirst);

            return Object.assign(
              {
                name: `${percent.value * 100} %`,
                type:
                  this.globalSettings.curveType == "function"
                    ? "spline"
                    : "line",
                lineWidth: 2,
                marker: {
                  symbol: "circle",
                  radius: 4
                },
                xAxis: 1,
                data: data
              },
              percent.options
            );
          })
        );
      }

      return series;
    },
    averageData() {
      return this.glucoseValuesByHours.map(hour => [
        hour.hour + 0.5,
        utils.changeUnit(
          hour.values.reduce(
            (accumulator, item) => (accumulator += item.value),
            0
          ) / hour.values.length,
          this.originalUnit,
          this.globalSettings.unit
        )
      ]);
    },
    medianData() {
      return this.glucoseValuesByHours.map(hour => {
        let index = Math.floor(hour.values.length / 2);

        return [
          hour.hour + 0.5,
          utils.changeUnit(
            hour.values.length % 2 == 1
              ? hour.values[index].value
              : (hour.values[Math.floor(index)].value +
                  hour.values[Math.floor(index - 1)].value) /
                  2,
            this.originalUnit,
            this.globalSettings.unit
          )
        ];
      });
    },
    targetRangeMin() {
      return agpConstants[this.globalSettings.unit].lowLimit;
    },
    targetRangeMax() {
      return agpConstants[this.globalSettings.unit].highLimit;
    },
    glucoseValuesByHours() {
      return this.filteredBloodGlucoseValues
        .reduce(groupGlucoseByHours, [])
        .map(hour =>
          Object.assign(hour, {
            values: hour.values.sort((a, b) => (a.value < b.value ? -1 : 1))
          })
        )
        .sort((a, b) => (a.hour < b.hour ? -1 : 1));
    }
  },
  mounted() {
    this.invalidate();

    EventBus.$on("rangeChanged", this.onRangeChanged);
    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
  },
  beforeDestroy() {
    EventBus.$off("rangeChanged", this.onRangeChanged);
    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);

    this.destroy();
  },
  props: [
    "filteredBloodGlucoseValues",
    "filteredInsulinValuesByBrands",
    "insulinTypes",
    "originalUnit",
    "dailyChartsVMax"
  ]
};
</script>
