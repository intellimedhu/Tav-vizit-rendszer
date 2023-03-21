<template>
    <div>
      <div id="custom-dialog-chart-area"
           v-show="!noData"
           :style="`height: ${chartMinHeight}px`"></div>

      <p class="text-muted text-center p-3" v-show="noData">
        {{ lang['noDataText'] }}
      </p>
    </div>
</template>

<script>
import moment from "moment";
import utils, { groupByDate, groupToArray } from "../services/utils";
import { EventBus } from "../services/event-bus";
import highcharts from "highcharts";
import exportingInit from "highcharts/modules/exporting";
import boost from "highcharts/modules/boost";
boost(highcharts);
exportingInit(highcharts);

export default {
  data() {
    return {
      chartMinHeight: 500,
      loading: null,
      chart: null,
      colors: [
        "#F012BE",
        "#D2691E",
        "#808000",
        "#FF851B",
        "#3D9970",
        "#FF4136",
        "#6A5ACD",
        "#FFDC00",
        "#40E0D0",
        "#85144b",
        "#0074D9",
        "#001f3f",
        "#111111",
        "#FF00FF",
        "#2ECC40",
        "#FF69B4",
        "#ADFF2F",
        "#696969",
        "#00CED1",
        "#7FDBFF",
        "#39CCCC",
        "#B10DC9",
        "#483D8B",
        "#01FF70",
        "#AAAAAA",
        "#9400D3",
        "#FFD700",
        "#808080",
        "#008080",
        "#3CB371",
        "#00BFFF"
      ]
    };
  },
  methods: {
    draw() {
      this.destroy();

      this.$nextTick(() => {
        setTimeout(() => {
          this.chart = highcharts.chart(
            "custom-dialog-chart-area",
            this.chartOptions
          );
        }, 5);
      });
    },

    destroy() {
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
      }
    },

    onPrint(e) {
      if (e.open) {
        this.chart.print();
      }
    }
  },
  computed: {
    noData() {
      return !this.insulinValues.length && !this.glucoseValues.length;
    },
    hour() {
      return this.$i18n.t("hour", this.globalSettings.currentLanguage);
    },
    insulinUnitTitle() {
      return this.$i18n.t(
        "insulinUnitTitle",
        this.globalSettings.currentLanguage
      );
    },
    datepickerFormat() {
      return this.$i18n.t(
        "datepickerFormat",
        this.globalSettings.currentLanguage
      );
    },
    lang() {
      return this.$i18n.t(
        "dgpCustomChart",
        this.globalSettings.currentLanguage
      );
    },
    ticks25() {
      return Array.from(Array(25), (x, i) => i);
    },
    ticksHour() {
      return this.ticks25.map(hour => `${hour}`.padStart(2, "0"));
    },
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    insulinUnitShort() {
      return this.$i18n.t(
        "insulinUnitShort",
        this.globalSettings.currentLanguage
      );
    },
    countUnit() {
      return (
        "[" +
        this.$i18n.t("countUnit", this.globalSettings.currentLanguage) +
        "]"
      );
    },
    chartOptions() {
      let _this = this;

      return {
        title: {
          text: this.chartTitle,
          style: {
            fontSize: "28px",
            textDecoration: "underline"
          }
        },
        chart: {
          height: _this.chartMinHeight,
          events: {
            load() {
              _this.loading = false;
            }
          },
          boost: {
            enabled: true,
            useGPUTranslations: true
          }
        },
        exporting: {
          buttons: {
            contextButton: {
              enabled: false
            }
          }
        },
        credits: false,
        tooltip: {
          valueDecimals: 1
        },
        plotOptions: {
          series: {
            animation: false,
            events: {
              legendItemClick: () => false
            },
            // Over 50+ series rendering fails. This is a temporarly solution:
            stacking: this.series.length >= 50 ? "normal" : null,
            pointRange: this.series.length >= 50 ? 24 * 3600 * 1000 : null
          },
          spline: {
            tooltip: {
              headerFormat: "",
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
          column: {
            tooltip: {
              headerFormat: "",
              pointFormatter() {
                return `<b>${moment(this.date).format(
                  _this.datepickerFormat
                )}</b>: ${this.y.toFixed(1)} ${_this.insulinUnitShort}`;
              }
            }
          }
        },
        xAxis: this.xAxis,
        yAxis: this.yAxis,
        series: this.series
      };
    },
    xAxis() {
      return {
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
      };
    },
    yAxis() {
      return [
        {
          title: {
            text: this.globalSettings.unit,
            rotation: 270,
            style: {
              fontWeight: "bold"
            }
          },
          min: 0,
          tickPositions: utils.getGlucoseTickPositions(
            this.globalSettings.unit,
            this.glucoseMaxValue
          ),
          gridLineWidth: 1
        },
        {
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
        }
      ];
    },
    series() {
      let seriesColor = [];

      let series = [
        // Insulin series:
        ...this.seriesInsulin,
        // Glucose series:
        ...groupToArray(this.glucoseValues.reduce(groupByDate, {})).map(
          (item, i) => {
            return {
              type: "spline",
              yAxis: 0,
              name: moment(item.values[0].date).format(this.datepickerFormat),
              color: this.colors[i],
              data: item.values
                .map(bg => [
                  bg.date.getHours() + bg.date.getMinutes() / 60,
                  utils.changeUnit(
                    bg.value,
                    this.originalUnit,
                    this.globalSettings.unit
                  )
                ])
                .sort(utils.sortByFirst),
              marker: {
                enabled: true,
                symbol: "circle",
                radius: 4
              }
            };
          }
        )
      ];

      return series;
    },
    seriesInsulin() {
      let result = [];

      let insulinSeriesLink = null;
      this.insulinBrands.forEach((brand, i) => {
        let insulinType = this.insulinTypes.find(x => x.type == brand.type);
        let data = this.insulinValues.filter(item => item.type == brand.type);

        result = result.concat(
          groupToArray(data.reduce(groupByDate, {})).map((dailyData, j) => {
            let serie = {
              type: "column",
              color: insulinType.color,
              yAxis: 1,
              states: {
                hover: {
                  enabled: false
                }
              },
              data: dailyData.values
                .reduce((accumulator, item) => {
                  let groupKey = item.date.getHours();

                  var arrayItem = accumulator.find(x => x.hour == groupKey);

                  if (!arrayItem) {
                    arrayItem = {
                      hour: groupKey,
                      count: 0,
                      sum: 0
                    };
                    accumulator.push(arrayItem);
                  }

                  arrayItem.count++;
                  arrayItem.sum += item.unit;

                  return accumulator;
                }, [])
                .map(item => {
                  let date = moment(new Date(dailyData.date))
                    .add(item.hour, "hours")
                    .toDate();

                  return {
                    date: date,
                    x: date.getHours(),
                    y: item.count == 1 ? item.sum : item.sum / item.count
                  };
                })
                .sort((a, b) => (a.x < b.x ? -1 : 1))
            };

            if (j == 0) {
              serie.id = brand.name;
              serie.name = brand.name;
            } else {
              serie.linkedTo = brand.name;
              serie.showInLegend = false;
            }

            return serie;
          })
        );
      });

      return result;
    },
    glucoseMaxValue() {
      let result = this.glucoseValues.reduce((max, item) => {
        return max < item.value ? item.value : max;
      }, 0);

      return utils.changeUnit(
        result,
        this.originalUnit,
        this.globalSettings.unit
      );
    }
  },
  mounted() {
    EventBus.$on("dgp-custom-chart-show", this.draw);
    EventBus.$on("print-dgp-dialog", this.onPrint);
  },
  beforeDestroy() {
    this.destroy();

    EventBus.$off("dgp-custom-chart-show", this.draw);
    EventBus.$off("print-dgp-dialog", this.onPrint);
  },
  props: [
    "glucoseValues",
    "insulinValues",
    "insulinTypes",
    "insulinBrands",
    "selectedDays",
    "originalUnit",
    "chartTitle"
  ]
};
</script>
