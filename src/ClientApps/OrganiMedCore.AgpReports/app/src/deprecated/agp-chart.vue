<template>
  <div>
    <div class="agp-container row print-content-diagrams" :class="{'mx-auto': chart && printView}">
      <div class="col-12 agp-column">
        <div v-if="displayAgp && !noData" class="mb-3">
          <div class="loading-spinner d-print-none text-primary" v-show="!initialized">
            <div class="spinner-border mb-3" role="status">
              <span class="sr-only">{{ loadingText }}</span>
            </div>
            <p>{{ loadingText }}</p>
          </div>
        
          <div class="agp-chart-area-wrapper">
            <div id="agp-chart-area" v-show="!printView"></div>
            <img :src="getChartImageUri()" v-if="chart && printView" class="d-print-block w-100 print-not-break" />
          </div>

          <ul class="axis-captions axis-captions-left d-print-none"
              :class="`axis-captions-left-${insulinBrands.length}`"
              v-if="globalSettings.displayInsulinInAGP"
              v-show="initialized">
            <li>{{ lang['axisHour'] }}</li>
            <li v-if="isDcmView">{{ lang['axisMeasurements'] }} [{{countUnit}}]</li>
            <li v-for="(brand, i) in insulinBrands" :key="i">
              {{ brand.name }} [{{countUnit}}]
            </li>
          </ul>

          <ul class="axis-captions axis-captions-right d-print-none"
              :class="`axis-captions-right-${insulinBrands.length}`" 
              v-if="globalSettings.displayInsulinInAGP"
              v-show="initialized">
            <li>{{ lang['axisHour'] }}</li>
          </ul>
        </div>

        <p class="alert alert-warning mt-3 mb-2" v-show="!displayAgp">
          <i class="icon-info-sign"></i>
          {{ lang['averageModeDaysLimitText'] }}
        </p>

        <p  class="alert alert-warning mt-3 mb-2" v-show="noData">
          <i class="icon-info-sign"></i>
          {{ lang['noDataText'] }}
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import utils, { agpConstants, smbgColors } from "../services/utils";
import { EventBus } from "../services/event-bus";

export default {
  data() {
    return {
      insulinMaxAverage: 0,
      printView: false,
      screenXs: 768,
      initialized: false,
      chart: null,
      windowWidth: window.innerWidth,
      percentageLines: {
        lowest: 0.05,
        low: 0.25,
        high: 0.75,
        highest: 0.95
      }
    };
  },
  methods: {
    async draw() {
      this.initialized = false;
      if (!this.displayAgp || this.noData) {
        return;
      }

      this.dispose();

      this.chart = new google.visualization.ComboChart(
        document.getElementById("agp-chart-area")
      );

      google.visualization.events.addListener(
        this.chart,
        "ready",
        this.chartReady
      );

      this.chart.draw(this.chartView, this.chartOptions);
    },

    chartReady() {
      this.initialized = true;
    },

    getAreaRow(date) {
      let areaRow = [date];

      if (this.isDcmView) {
        areaRow.push(
          null, // very low value
          null, // very low tooltip
          null, // low value
          null, // low tooltip
          null, // normal value
          null, // normal tooltip
          null, // high value
          null, // high tooltip
          null, // very high value
          null // very high tooltip
        );
      } else {
        areaRow.push(
          null, // agp scatter value
          null // agp scatter tooltip
        );
      }

      areaRow.push(
        null, // midline (50%)
        null // midline (50%) tooltip
      );

      if (this.globalSettings.displayAverageLines) {
        areaRow.push(
          null, // line: lowest %
          null, // tooltip: lowest %
          null, // line: low %
          null, // tooltip: low %
          null, // line: high %
          null, // tooltip: high %
          null, // line: highest %
          null // tooltip: highest %)
        );
      }

      // Placeholder for insulin datas
      this.insulinBrands.forEach(() => {
        areaRow.push(null, null); // insulin value and tooltip
      });

      let areaMinFormatted = utils.round(this.targetRangeMin, 1);
      let areaMaxFormatted = utils.round(
        this.targetRangeMax - this.targetRangeMin,
        1
      );
      let areaMaxFormattedValue = utils.round(this.targetRangeMax, 1);

      let strokeColor = this.isDcmView ? "#aafaaa" : "#555";

      let areaStyleMin = `fill-color: white; stroke-color: ${strokeColor}; stroke-width: 2;`;
      let areaStyleMax = `fill-color: ${
        this.isDcmView ? "#aafaaa" : "white"
      }; stroke-color: ${strokeColor}; stroke-width: 2;`;

      areaRow.push(
        areaMinFormatted, // target range min
        areaMinFormatted.toString(), // target range min tooltip
        areaStyleMin,

        areaMaxFormatted, // target range max
        areaMaxFormattedValue.toString(),
        areaStyleMax // area style
      ); // target range max tooltip

      return areaRow;
    },

    getTime(date) {
      if (!this.isDate(date)) {
        throw date + " is not a valid date";
      }

      return new Date(
        0,
        0,
        0,
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
      );
    },

    isDate(parameter) {
      if (!(parameter instanceof Date) || parameter == "Invalid Date") {
        return false;
      }

      return true;
    },

    sortByFirstItem(a, b) {
      return a[0] < b[0] ? -1 : 1;
    },

    sortByHour(a, b) {
      return utils.defaultCompare(a.hour, b.hour);
    },

    getInsulinsAverage(insulins) {
      if (!insulins || !insulins.length) {
        return null;
      }

      return utils.round(
        insulins.reduce((accumulator, currentValue) => {
          return accumulator + currentValue.unit;
        }, 0) / insulins.length,
        1
      );
    },

    initializePercentageValues(percentage) {
      if (
        !this.glucoseValuesByHours ||
        !this.originalUnit ||
        !this.globalSettings.unit
      ) {
        return;
      }

      let result = [];

      this.glucoseValuesByHours.forEach(x => {
        let currentDate = new Date(0, 0, 0, x.hour, 30);

        let newValue = [currentDate];
        if (this.isDcmView) {
          newValue.push(
            null, // very low value
            null, // very low value tooltip
            null, // low value
            null, // low value tooltip
            null, // normal value
            null, // normal value tooltip
            null, // high value
            null, // high value tooltip
            null, // very high value
            null // very high value tooltip
          );
        } else {
          newValue.push(
            null, // agp scatter value
            null // agp scatter tooltip
          );
        }

        newValue.push(
          null, // midline
          null, // midline tooltip
          null, // lowest %
          null, // lowest % tooltip
          null, // low %
          null, // low % tooltip
          null, // high %
          null, // high % tooltip
          null, // highest %
          null // highest % tooltip
        );

        this.insulinBrands.forEach(type => {
          newValue.push(null, null); // insulin & tooltip
        });

        // area
        newValue.push(null, null, null, null, null, null); // target range: min, min tt, min style, max, max tt, max style

        if (x.values.length < 2) {
          return;
        }

        let value = x.values[Math.floor(x.values.length * percentage)];
        let newValueIndex;
        let newValueTooltipIndex;

        switch (percentage) {
          case this.percentageLines.highest:
            newValueIndex = this.isDcmView ? 19 : 11;
            newValueTooltipIndex = this.isDcmView ? 20 : 12;
            break;

          case this.percentageLines.high:
            newValueIndex = this.isDcmView ? 17 : 9;
            newValueTooltipIndex = this.isDcmView ? 18 : 10;

            break;

          case this.percentageLines.low:
            newValueIndex = this.isDcmView ? 15 : 7;
            newValueTooltipIndex = this.isDcmView ? 16 : 8;
            break;

          case this.percentageLines.lowest:
            newValueIndex = this.isDcmView ? 13 : 5;
            newValueTooltipIndex = this.isDcmView ? 14 : 6;

            break;
        }

        let valueInUnit = utils.round(
          utils.changeUnit(
            value.value,
            this.originalUnit,
            this.globalSettings.unit
          ),
          1
        );
        newValue[newValueIndex] = valueInUnit;
        newValue[newValueTooltipIndex] = valueInUnit.toString();

        result.push(newValue);
      });

      result.sort(this.sortByFirstItem);

      return result;
    },

    groupGlucoseValuesByHours(sourceArray, onGroupingFinished) {
      let hourlyGroups = [];

      sourceArray.forEach(currentValue => {
        let currentHour = currentValue.date.getHours();
        let j = 0;
        while (j < hourlyGroups.length && hourlyGroups[j].hour != currentHour) {
          j++;
        }

        if (j < hourlyGroups.length) {
          hourlyGroups[j].values.push(currentValue);
        } else {
          hourlyGroups.push({
            hour: currentHour,
            values: [currentValue]
          });
        }

        hourlyGroups[j].values.sort((a, b) => (a.value < b.value ? -1 : 1));

        j = null;
        currentHour = null;
      });

      this.fillHourLeaks(hourlyGroups);
      hourlyGroups.sort(this.sortByHour);

      onGroupingFinished(hourlyGroups);

      hourlyGroups = null;
    },

    fillHourLeaks(values) {
      // Find leaks
      for (let i = 0; i < 24; i++) {
        let hourLeak = values.filter(x => {
          return x.hour == i;
        })[0];

        if (!hourLeak) {
          values.push({
            hour: i,
            values: []
          });
        }
      }
    },

    dispose() {
      google.visualization.events.removeListener(
        this.chart,
        "ready",
        this.chartReady
      );

      if (this.chart) {
        this.chart.clearChart();
        this.chart = null;
      }
    },

    getChartImageUri() {
      if (!this.initialized || !this.displayAgp || this.noData || !this.chart) {
        return;
      }

      return this.chart.getImageURI();
    },

    invalidate() {
      this.$nextTick(() => {
        google.charts.setOnLoadCallback(this.draw);
      });
    },

    onGlobalSettingsUpdated(x) {
      if (
        x.difference.curveType ||
        x.difference.displayAverageLines ||
        x.difference.displayNonMeasurableValues ||
        x.difference.currentLanguage ||
        x.difference.midlineType ||
        x.difference.unit ||
        x.difference.colorBasal ||
        x.difference.colorBolus ||
        x.difference.colorMix ||
        x.difference.viewMode
      ) {
        this.invalidate();
      }
    },

    onRangeChanged() {
      this.$nextTick(this.invalidate);
    },

    onVResized() {
      this.windowWidth = window.innerWidth;
      this.invalidate();
    },

    onPrintViewSet(printView) {
      this.printView = printView;
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },

    totalDays() {
      return this.$store.getters.totalDays;
    },

    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },

    lang() {
      return this.$i18n.t("agpChart", this.globalSettings.currentLanguage);
    },

    loadingText() {
      return this.$i18n.t("loadingText", this.globalSettings.currentLanguage);
    },

    insulinUnitTitle() {
      return this.$i18n.t(
        "insulinUnitTitle",
        this.globalSettings.currentLanguage
      );
    },

    countUnit() {
      return this.$i18n.t("countUnit", this.globalSettings.currentLanguage);
    },

    isSmallSize() {
      return window.innerWidth < this.screenXs;
    },

    horizontalAxisFontSize() {
      if (this.windowWidth < 768) {
        // sm
        return 6;
      }

      if (this.windowWidth < 1024) {
        // md
        return 7;
      }

      if (this.windowWidth < 1200) {
        // lg
        return 12;
      }

      return 14;
    },

    displayAgp() {
      return this.totalDays <= this.globalSettings.averageModeDaysLimit;
    },

    chartView() {
      if (!this.displayAgp || this.noData) {
        return;
      }

      let data = new google.visualization.DataTable();
      data.addColumn("datetime", this.lang.time);

      if (this.isDcmView) {
        // S0: Scatter - Blood glucose very low values
        data.addColumn("number", this.lang.veryLowValues);
        data.addColumn({ type: "string", role: "tooltip" });

        // S1: Scatter - Blood glucose  low values
        data.addColumn("number", this.lang.lowValues);
        data.addColumn({ type: "string", role: "tooltip" });

        // S2: Scatter - Blood glucose normal values
        data.addColumn("number", this.lang.inTargetValues);
        data.addColumn({ type: "string", role: "tooltip" });

        // S3: Scatter - Blood glucose high values
        data.addColumn("number", this.lang.highValues);
        data.addColumn({ type: "string", role: "tooltip" });

        // S4: Scatter - Blood glucose high values
        data.addColumn("number", this.lang.veryHighValues);
        data.addColumn({ type: "string", role: "tooltip" });
      } else {
        // S4: Scatter - Blood glucose high values
        data.addColumn("number", this.lang.agpScatterValues);
        data.addColumn({ type: "string", role: "tooltip" });
      }

      // S5: Midline
      data.addColumn("number", "50%");
      data.addColumn({ type: "string", role: "tooltip" });

      if (this.globalSettings.displayAverageLines) {
        // S6: lowest percent
        data.addColumn(
          "number",
          `${(this.percentageLines.lowest * 100).toFixed(0)}%`
        );
        data.addColumn({ type: "string", role: "tooltip" });

        // S7: low percent
        data.addColumn(
          "number",
          `${(this.percentageLines.low * 100).toFixed(0)}%`
        );
        data.addColumn({ type: "string", role: "tooltip" });

        // S8: high percent
        data.addColumn(
          "number",
          `${(this.percentageLines.high * 100).toFixed(0)}%`
        );
        data.addColumn({ type: "string", role: "tooltip" });

        // S9: highest percent
        data.addColumn(
          "number",
          `${(this.percentageLines.highest * 100).toFixed(0)}%`
        );
        data.addColumn({ type: "string", role: "tooltip" });
      }

      this.insulinBrands.forEach(brand => {
        // S10+: insulin
        data.addColumn("number", brand.name);
        data.addColumn({ type: "string", role: "tooltip" });
      });

      // S10++: Target range (area)
      data.addColumn("number", "");
      data.addColumn({ type: "string", role: "tooltip" });
      data.addColumn({ type: "string", role: "style" });

      data.addColumn(
        "number",
        this.lang.targetRangeLegend +
          " (" +
          this.targetRangeMin +
          "-" +
          this.targetRangeMax +
          ")"
      );
      data.addColumn({ type: "string", role: "tooltip" });
      data.addColumn({ type: "string", role: "style" });

      // Adding precalculated data.
      if (!this.noData) {
        data.addRows(this.chartData);
      }

      let view = new google.visualization.DataView(data);
      data = null;

      let hiddenColumns = [];
      let insulinColumnIndex = -1;
      if (this.isDcmView) {
        insulinColumnIndex = this.globalSettings.displayAverageLines ? 21 : 13;
      } else {
        insulinColumnIndex = this.globalSettings.displayAverageLines ? 13 : 5;
      }

      if (!this.globalSettings.displayInsulinInAGP) {
        this.insulinBrands.forEach(() => {
          hiddenColumns.push(insulinColumnIndex, insulinColumnIndex + 1);
          insulinColumnIndex += 2;
        });
      }

      if (hiddenColumns.length) {
        view.hideColumns(hiddenColumns);
      }

      return view;
    },

    fullDateFormat() {
      return this.$i18n.t(
        "fullDateFormat",
        this.globalSettings.currentLanguage
      );
    },

    noData() {
      let noData = !this.filteredBloodGlucoseValues.length;
      if (this.globalSettings.displayInsulinInAGP) {
        noData = noData && !this.insulinsCount;
      }

      return noData;
    },

    chartData() {
      if (this.noData) {
        return;
      }

      let result = [
        this.getAreaRow(new Date(0, 0, 0)),
        this.getAreaRow(new Date(0, 0, 0, 23, 59, 59, 59, 999))
      ];

      //Scatter chart's data
      this.filteredBloodGlucoseValues.forEach(bg => {
        let currentValue = utils.changeUnit(
          bg.value,
          this.originalUnit,
          this.globalSettings.unit
        );

        let scatterTooltip = utils.getGlucoseValueTooltip(
          currentValue,
          bg.date,
          this.globalSettings.unit,
          this.fullDateFormat
        );

        let scatterRow = [this.getTime(bg.date)];
        if (this.isDcmView) {
          let veryLowValue = null;
          let veryLowTooltip = null;
          let lowValue = null;
          let lowTooltip = null;
          let normalValue = null;
          let normalTooltip = null;
          let highValue = null;
          let highTooltip = null;
          let veryHighValue = null;
          let veryHighTooltip = null;

          if (bg.agpIsVeryLow) {
            veryLowValue = currentValue;
            veryLowTooltip = scatterTooltip;
          } else if (bg.agpIsLow) {
            lowValue = currentValue;
            lowTooltip = scatterTooltip;
          } else if (bg.agpIsHigh) {
            highValue = currentValue;
            highTooltip = scatterTooltip;
          } else if (bg.agpIsVeryHigh) {
            veryHighValue = currentValue;
            veryHighTooltip = scatterTooltip;
          } else {
            normalValue = currentValue;
            normalTooltip = scatterTooltip;
          }

          scatterRow.push(
            veryLowValue,
            veryLowTooltip,
            lowValue,
            lowTooltip,
            normalValue,
            normalTooltip,
            highValue,
            highTooltip,
            veryHighValue,
            veryHighTooltip
          );
        } else {
          scatterRow.push(currentValue, scatterTooltip);
        }

        // midline and tooltip
        scatterRow.push(null, null);

        if (this.globalSettings.displayAverageLines) {
          scatterRow.push(null, null, null, null, null, null, null, null);
        }

        // Placeholder for insulin datas
        this.insulinBrands.forEach(() => {
          scatterRow.push(null, null); // insulin value ant tooltip
        });

        scatterRow.push(null, null, null, null, null, null); // target range: min, min tt, min style, max, max tt, max style

        result.push(scatterRow);
      });

      // Midline
      let midlineData = null;
      if (this.globalSettings.midlineType == "avg") {
        midlineData = this.avgMidline;
      } else if (this.globalSettings.midlineType == "median") {
        midlineData = this.medianMindline;
      }

      if (midlineData) {
        midlineData.forEach(x => result.push(x));
        midlineData = null;
      }

      // Insulin
      if (this.globalSettings.displayInsulinInAGP) {
        let i = 0;
        let typeMinutes = 0;
        let typeMinuteStep = 60 / this.insulinBrands.length;

        this.insulinMaxAverage = 0;
        this.insulinValuesByHours.forEach(type => {
          type.hourlyValues.forEach(x => {
            let insulinRow = [];
            let k = 0;
            let hasAverage = false;
            this.insulinBrands.forEach(() => {
              if (i == k++) {
                let average = this.getInsulinsAverage(x.insulins);
                if (average) {
                  hasAverage = true;
                  insulinRow.push(average, `${type.insulinType}: ${average}`);

                  if (average > this.insulinMaxAverage) {
                    this.insulinMaxAverage = average;
                  }

                  average = null;

                  return;
                }
              }

              insulinRow.push(null, null);
            });

            if (hasAverage) {
              if (this.globalSettings.displayAverageLines) {
                insulinRow.unshift(
                  null,
                  null,
                  null,
                  null,
                  null,
                  null,
                  null,
                  null
                );
              }

              // midline and tooltip
              insulinRow.unshift(null, null);

              if (this.isDcmView) {
                insulinRow.unshift(
                  null, // very low value
                  null, // very tooltip of low value
                  null, // low value
                  null, // tooltip of low value
                  null, // normal value
                  null, // tooltip of normal value
                  null, // high value
                  null, // tooltip of high value
                  null, // very high value
                  null // very tooltip of high value
                );
              } else {
                insulinRow.unshift(
                  null, // agp scatter value
                  null // agp scatter tooltip
                );
              }

              // time
              insulinRow.unshift(new Date(0, 0, 0, x.hour, typeMinutes, 0));

              insulinRow.push(null, null, null, null, null, null); // target range: min, min tt, min style, max, max tt, max style
              result.push(insulinRow);
            }

            k = null;
            insulinRow = null;
            hasAverage = null;
          });

          i++;
          typeMinutes += typeMinuteStep;
        });

        i = null;
        typeMinutes = null;
        typeMinuteStep = null;
      }

      if (this.globalSettings.displayAverageLines) {
        this.lowestPercentageValues.forEach(x => result.push(x));
        this.lowPercentageValues.forEach(x => result.push(x));
        this.highPercentageValues.forEach(x => result.push(x));
        this.highestPercentageValues.forEach(x => result.push(x));
      }

      return result;
    },

    chartOptions() {
      if (!this.displayAgp || this.noData) {
        return;
      }

      let series = {};
      let seriesIndex = 0;

      if (this.isDcmView) {
        // very low values
        series[seriesIndex++] = {
          color: smbgColors.slow
        };

        // low values
        series[seriesIndex++] = {
          color: smbgColors.low
        };

        // normal values
        series[seriesIndex++] = {
          color: smbgColors.target
        };

        // high values
        series[seriesIndex++] = {
          color: "#f7d61c" // smbgColors.high
        };

        // very high values
        series[seriesIndex++] = {
          color: smbgColors.shigh
        };
      } else {
        // agp scatter values
        series[seriesIndex++] = {
          color: "#6d6e71",
          pointShape: "diamond",
          pointSize: 8
        };
      }

      // midline
      series[seriesIndex++] = {
        type: "line",
        pointSize: 8,
        lineWidth: 3,
        curveType: this.globalSettings.curveType,
        color: "#f68b1e"
      };

      if (this.globalSettings.displayAverageLines) {
        // lowest %
        series[seriesIndex++] = {
          type: "line",
          pointSize: 0,
          lineWidth: 2,
          lineDashStyle: [10, 10],
          curveType: this.globalSettings.curveType,
          color: "#99c26b"
        };

        // low %
        series[seriesIndex++] = {
          type: "line",
          pointSize: 0,
          lineWidth: 2,
          //lineDashStyle: [10, 10],
          curveType: this.globalSettings.curveType,
          color: "#50a9d8"
        };

        // high %
        series[seriesIndex++] = {
          type: "line",
          pointSize: 0,
          lineWidth: 2,
          //lineDashStyle: [10, 10],
          curveType: this.globalSettings.curveType,
          color: "#50a9d8"
        };

        // highest %
        series[seriesIndex++] = {
          type: "line",
          pointSize: 0,
          lineWidth: 2,
          lineDashStyle: [10, 10],
          curveType: this.globalSettings.curveType,
          color: "#99c26b"
        };
      }

      // insulin series
      if (this.globalSettings.displayInsulinInAGP) {
        this.insulinBrands.forEach(brand => {
          let insulinType = this.insulinTypes.find(x => x.type == brand.type);
          series[seriesIndex++] = {
            type: "bars",
            color: insulinType ? insulinType.color : null,
            targetAxisIndex: 1
          };
        });
      }

      // target range start point
      series[seriesIndex++] = {
        type: "area",
        pointSize: 0,
        lineWidth: 1,
        visibleInLegend: false,
        color: "transparent"
      };

      // target range end point
      series[seriesIndex++] = {
        type: "area",
        pointSize: 0,
        lineWidth: 1
      };

      let vAxes = {
        // glucose axis
        0: {
          title: this.isSmallSize ? null : this.lang.chartTitle,
          titleTextStyle: {
            bold: true,
            italic: false,
            fontSize: 12
          },
          gridlines: {
            //count: 7,
            color: "gray"
          },
          ticks: this.horizontalTicksInCurrentUnit,
          format: "0.0"
          //scaleType: "log"
        }
      };

      if (this.globalSettings.displayInsulinInAGP) {
        // insulin axis
        vAxes[1] = {
          title: this.isSmallSize ? null : this.insulinUnitTitle,
          titleTextStyle: {
            bold: true,
            italic: false,
            fontSize: 12
          },
          gridlines: {
            count: 0
          },
          viewWindow: {
            min: 0,
            max: this.insulinMaxAverage
          }
        };
      }

      let options = {
        async: true,
        aggregationTarget: "category",
        height: 500,
        title: this.isSmallSize ? this.lang.chartTitle : null,
        chartArea: {
          left: this.isSmallSize ? null : 100,
          right: this.isSmallSize ? null : 80,
          top: 30,
          bottom: this.isSmallSize ? 60 : 120
        },
        isStacked: true,
        legend: {
          position: "bottom",
          textStyle: {
            fontSize: 14
          }
        },
        hAxis: {
          titleTextStyle: {
            italic: false
          },
          textStyle: {
            fontSize: this.horizontalAxisFontSize
          },
          ticks: utils.get24HoursTicks(
            this.isDcmView,
            this.glucoseValuesByHours,
            this.language,
            this.globalSettings.displayInsulinInAGP && this.insulinBrands.length
              ? this.insulinValuesByHours
              : []
          )
        },
        bar: {
          groupWidth: this.isSmallSize ? "25%" : "5"
        },
        interpolateNulls: true,
        seriesType: "scatter",
        vAxes: vAxes,
        series: series
      };

      series = null;
      vAxes = null;
      seriesIndex = null;

      return options;
    },

    avgMidline() {
      if (this.globalSettings.midlineType != "avg") {
        return;
      }

      let avgData = [];

      this.glucoseValuesByHours.forEach(x => {
        let valuesInTargetRange = x.values.filter(v => {
          let value = utils.changeUnit(
            v.value,
            this.originalUnit,
            this.globalSettings.unit
          );
          return value >= this.targetRangeMin && value <= this.targetRangeMax;
        });

        // no average
        if (!valuesInTargetRange.length) {
          return;
        }

        let average =
          valuesInTargetRange.reduce((a, y) => {
            return a + y.value;
          }, 0) / valuesInTargetRange.length;

        valuesInTargetRange = null;

        let avg = utils.round(
          utils.changeUnit(
            average,
            this.originalUnit,
            this.globalSettings.unit
          ),
          1
        );

        let midlineRow = [new Date(0, 0, 0, x.hour, 30)];
        if (this.isDcmView) {
          midlineRow.push(
            null, // very low value
            null, // very low tooltip
            null, // low value
            null, // low tooltip
            null, // normal value
            null, // normal tooltip
            null, // high value
            null, // high tooltip
            null, // very high value
            null // very high tooltip
          );
        } else {
          midlineRow.push(
            null, // agp scatter value
            null // agp scatter tooltip
          );
        }

        midlineRow.push(
          avg,
          `${avg} ${this.globalSettings.unit}, ${x.hour
            .toString()
            .padStart(2, "0")}`
        );

        if (this.globalSettings.displayAverageLines) {
          midlineRow.push(null, null, null, null, null, null, null, null);
        }

        // Placeholder for insulin datas
        this.insulinBrands.forEach(() => {
          midlineRow.push(null, null); // insulin value, tt
        });

        midlineRow.push(null, null, null, null, null, null); // target range: min, min tt, min style, max, max tt, max style
        avgData.push(midlineRow);
      });

      avgData.sort(this.sortByFirstItem);

      return avgData;
    },

    medianMindline() {
      if (this.globalSettings.midlineType != "median") {
        return;
      }

      let medianData = [];

      this.glucoseValuesByHours.forEach(x => {
        let medianArray = x.values.map(x =>
          utils.changeUnit(x.value, this.originalUnit, this.globalSettings.unit)
        );

        // No median
        if (medianArray.length < 2) {
          return;
        }

        medianArray.sort(utils.defaultCompare);
        let middleIndex = Math.floor(medianArray.length / 2);
        let median = utils.round(
          medianArray.length % 2 == 1
            ? medianArray[middleIndex]
            : (medianArray[Math.floor(middleIndex)] +
                medianArray[Math.floor(middleIndex - 1)]) /
                2,
          1
        );

        middleIndex = null;

        let midline = [new Date(0, 0, 0, x.hour, 30)];
        if (this.isDcmView) {
          midline.push(
            null, // very low value
            null, // very low tooltip
            null, // low value
            null, // low tooltip
            null, // normal value
            null, // normal tooltip
            null, // high value
            null, // high tooltip
            null, // very high value
            null // very high tooltip
          );
        } else {
          midline.push(
            null, // agp scatter value
            null // agp scatter tooltip
          );
        }

        midline.push(
          median,
          `${median} ${this.globalSettings.unit}, ${x.hour
            .toString()
            .padStart(2, "0")}`
        );

        if (this.globalSettings.displayAverageLines) {
          midline.push(null, null, null, null, null, null, null, null);
        }

        // Placeholder for insulin datas
        this.insulinBrands.forEach(() => {
          midline.push(null, null); // insulin value, tt
        });

        midline.push(null, null, null, null, null, null); // target range: min, min tt, min style, max, max tt, max style
        medianData.push(midline);

        midline = null;
      });

      medianData.sort(this.sortByFirstItem);

      return medianData;
    },

    lowestPercentageValues() {
      if (this.globalSettings.displayAverageLines) {
        return this.initializePercentageValues(this.percentageLines.lowest);
      }

      return [];
    },

    lowPercentageValues() {
      if (this.globalSettings.displayAverageLines) {
        return this.initializePercentageValues(this.percentageLines.low);
      }

      return [];
    },

    highPercentageValues() {
      if (this.globalSettings.displayAverageLines) {
        return this.initializePercentageValues(this.percentageLines.high);
      }

      return [];
    },

    highestPercentageValues() {
      if (this.globalSettings.displayAverageLines) {
        return this.initializePercentageValues(this.percentageLines.highest);
      }

      return [];
    },

    glucoseValuesByHours() {
      let result = [];
      this.groupGlucoseValuesByHours(
        this.filteredBloodGlucoseValues,
        valuesByHours => (result = valuesByHours)
      );

      return result;
    },

    horizontalTicksInCurrentUnit() {
      return this.globalSettings.unit == "mmol/l"
        ? [0, 3, 5, 10, 15, 20, 25, this.dailyChartsVMax <= 30 ? 30 : 40]
        : [
            0,
            54,
            90,
            180,
            270,
            360,
            450,
            this.dailyChartsVMax <= 540 ? 540 : 720
          ];
    },

    targetRangeMin() {
      return utils.round(agpConstants[this.globalSettings.unit].lowLimit, 1);
    },

    targetRangeMax() {
      return utils.round(agpConstants[this.globalSettings.unit].highLimit, 1);
    },

    insulinValuesByHours() {
      let result = [];

      this.filteredInsulinValuesByBrands.forEach(type => {
        let hourlyValues = [];

        for (let i = 0; i < 24; i++) {
          hourlyValues.push({
            hour: i,
            insulins: type.values.filter(x => x.date.getHours() == i)
          });
        }

        result.push({
          insulinType: type.insulinType,
          hourlyValues: hourlyValues,
          brandName: type.name
        });

        hourlyValues = null;
      });

      return result;
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.invalidate();
    });

    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$on("rangeChanged", this.onRangeChanged);
    EventBus.$on("vResized", this.onVResized);
    EventBus.$on("setAgpPrintView", this.onPrintViewSet);
  },
  beforeDestroy() {
    this.dispose();

    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$off("rangeChanged", this.onRangeChanged);
    EventBus.$off("vResized", this.onVResized);
    EventBus.$off("setAgpPrintView", this.onPrintViewSet);
  },
  props: [
    "filteredBloodGlucoseValues",
    "insulinsCount",
    "filteredInsulinValuesByBrands",
    "insulinBrands",
    "insulinTypes",
    "originalUnit",
    "dailyChartsVMax"
  ]
};
</script>
