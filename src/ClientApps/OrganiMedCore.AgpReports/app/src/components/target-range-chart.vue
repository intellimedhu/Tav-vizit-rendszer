<template>
  <div class="col-lg target-range-chart my-4 my-md-5 my-lg-0 loading-container" :style="`height:${chartHeight}px`">
    <div class="row" v-show="!noData">
      <div class="col loading-spinner d-print-none" v-show="loading">
        <div class="spinner-border mb-3" role="status">
          <span class="sr-only">{{ loadingText }}</span>
        </div>
      </div>

      <div class="col targe-range-limits p-0" v-show="!loading">
        <ul class="list-unstyled range-limit-list">
          <li class="high" :style="`top:calc(${targetRangeStacked.high}% - 4px)`">{{targetRangeStackedValues.sHigh}}</li>
          <li class="in-target" :style="`top:calc(${targetRangeStacked.inTarget}% - 4px)`">{{targetRangeStackedValues.high}}</li>
          <li class="low" :style="`top:calc(${targetRangeStacked.low}% - ${showTargetRangeStackedSlow?'4':'0'}px)`">{{targetRangeStackedValues.low}}</li>
          <li v-show="showTargetRangeStackedSlow"
              class="slow"
              :style="`top:calc(${targetRangeStacked.sLow}% - 4px)`">{{targetRangeStackedValues.sLow}}</li>
        </ul>
      </div>
      <div class="col col-target-range-chart text-right">
        <div class="chart-inner">
          <div :id="chartContainerId"></div>
        </div>
      </div>
      <div class="col target-range-column-percent pr-0 mr-lg-n1 mr-xl-0" v-show="!loading">
        <ul class="list-unstyled percent-list">
          <target-range-legend wrapper-classes="shigh row align-items-end"
                                      :glucose-percentage="glucosePercentages.sHigh"
                                      :measurements-count="statisticalData.sHigh"
                                      :range-caption="lang['veryHighCaption']"
                                      :reference-min="percentageReferences.sHigh[0]"
                                      :reference-max="percentageReferences.sHigh[1]"
                                      :hour-scale="hourScales.sHigh"
                                      :hour-scale-sm="hourScalesSm.sHigh" />

          <target-range-legend wrapper-classes="high row align-items-center"
                                      :wrapper-top="percentageStacked.high"
                                      :glucose-percentage="glucosePercentages.high"
                                      :measurements-count="statisticalData.high"
                                      :range-caption="lang['highCaption']"
                                      :reference-min="percentageReferences.high[0]"
                                      :reference-max="percentageReferences.high[1]"
                                      :hour-scale="hourScales.high"
                                      :hour-scale-sm="hourScalesSm.high"  />

          <target-range-legend wrapper-classes="in-target row align-items-center"
                                      :wrapper-top="percentageStacked.inTarget"
                                      :glucose-percentage="glucosePercentages.inTarget"
                                      :measurements-count="statisticalData.inTarget"
                                      :range-caption="lang['inTargetCaption']"
                                      :reference-min="percentageReferences.inTarget[0]"
                                      :reference-max="percentageReferences.inTarget[1]"
                                      :hour-scale="hourScales.inTarget"
                                      :hour-scale-sm="hourScalesSm.inTarget"  />

          <target-range-legend wrapper-classes="low row align-items-center"
                                      :wrapper-top="percentageStacked.low"
                                      :glucose-percentage="glucosePercentages.low"
                                      :measurements-count="statisticalData.low"
                                      :range-caption="lang['lowCaption']"
                                      :reference-min="percentageReferences.low[0]"
                                      :reference-max="percentageReferences.low[1]"
                                      :hour-scale="hourScales.low"
                                      :hour-scale-sm="hourScalesSm.low"  />

          <target-range-legend wrapper-classes="slow row align-items-start"
                                      :glucose-percentage="glucosePercentages.sLow"
                                      :measurements-count="statisticalData.sLow"
                                      :range-caption="lang['veryLowCaption']"
                                      :reference-min="percentageReferences.sLow[0]"
                                      :reference-max="percentageReferences.sLow[1]"
                                      :hour-scale="hourScales.sLow"
                                      :hour-scale-sm="hourScalesSm.sLow"  />
        </ul>
      </div>
    </div>
    <p class="alert alert-warning" v-if="noData">
      <i class="icon-info-sign"></i>
      {{ lang['noData'] }}
    </p>
  </div>
</template>

<script>
import moment from "moment";
import targetRangeLegend from "./target-range-legend";
import { EventBus } from "../services/event-bus";
import utils, { agpConstants, smbgColors } from "../services/utils";
import highcharts from "highcharts";
import boost from "highcharts/modules/boost";
require("highcharts/highcharts-more")(highcharts);
require("highcharts-border-radius");
const borderRadius = require("highcharts-border-radius");
borderRadius(highcharts);
boost(highcharts);

export default {
  components: {
    targetRangeLegend
  },
  data() {
    return {
      chartContainerId: "target-range-chart-area",
      chartHeight: 153,
      chart: null,
      loading: null
    };
  },
  methods: {
    draw() {
      this.destroy();

      if (this.noData) {
        utils.triggerInvalidatedEvent("targetRange");
        return;
      }

      this.loading = true;
      let _this = this;
      setTimeout(() => {
        this.chart = highcharts.chart(this.chartContainerId, {
          title: "",
          chart: {
            type: "column",
            width: 42,
            height: 153,
            margin: 0,
            boost: {
              enabled: true,
              useGPUTranslations: true
            },
            events: {
              load() {
                _this.loading = false;
                utils.triggerInvalidatedEvent("targetRange");
              }
            }
          },
          legend: { enabled: false },
          credits: { enabled: false },
          exporting: { enabled: false },
          tooltip: { enabled: false },
          plotOptions: {
            column: {
              stacking: "percent",
              pointWidth: 42,
              animation: false
            }
          },

          yAxis: {
            min: 0,
            gridLineWidth: 0
          },

          series: [
            // very high
            {
              data: [this.glucosePercentages.sHigh],
              color: smbgColors.shigh,
              borderRadiusTopLeft: this.glucosePercentages.sHigh > 0 ? 5 : null,
              borderRadiusTopRight:
                this.glucosePercentages.sHigh > 0 ? 5 : null,
              borderRadiusBottomLeft:
                this.glucosePercentages.sLow +
                  this.glucosePercentages.low +
                  this.glucosePercentages.inTarget +
                  this.glucosePercentages.high ==
                  0 && this.glucosePercentages.sHigh > 0
                  ? 5
                  : null,
              borderRadiusBottomRight:
                this.glucosePercentages.sLow +
                  this.glucosePercentages.low +
                  this.glucosePercentages.inTarget +
                  this.glucosePercentages.high ==
                  0 && this.glucosePercentages.sHigh > 0
                  ? 5
                  : null
            },
            // high
            {
              data: [this.glucosePercentages.high],
              color: smbgColors.high,
              borderRadiusTopLeft:
                this.glucosePercentages.sHigh == 0 &&
                this.glucosePercentages.high > 0
                  ? 5
                  : null,
              borderRadiusTopRight:
                this.glucosePercentages.sHigh == 0 &&
                this.glucosePercentages.high > 0
                  ? 5
                  : null,
              borderRadiusBottomLeft:
                this.glucosePercentages.sLow +
                  this.glucosePercentages.low +
                  this.glucosePercentages.inTarget ==
                  0 && this.glucosePercentages.high > 0
                  ? 5
                  : null,
              borderRadiusBottomRight:
                this.glucosePercentages.sLow +
                  this.glucosePercentages.low +
                  this.glucosePercentages.inTarget ==
                  0 && this.glucosePercentages.high > 0
                  ? 5
                  : null
            },
            // target range
            {
              data: [this.glucosePercentages.inTarget],
              color: smbgColors.target,
              borderRadiusTopLeft:
                this.glucosePercentages.sHigh + this.glucosePercentages.high ==
                  0 && this.glucosePercentages.inTarget > 0
                  ? 5
                  : null,
              borderRadiusTopRight:
                this.glucosePercentages.sHigh + this.glucosePercentages.high ==
                  0 && this.glucosePercentages.inTarget > 0
                  ? 5
                  : null,
              borderRadiusBottomLeft:
                this.glucosePercentages.sLow + this.glucosePercentages.low ==
                  0 && this.glucosePercentages.inTarget > 0
                  ? 5
                  : null,
              borderRadiusBottomRight:
                this.glucosePercentages.sLow + this.glucosePercentages.low ==
                  0 && this.glucosePercentages.inTarget > 0
                  ? 5
                  : null
            },
            // low
            {
              data: [this.glucosePercentages.low],
              color: smbgColors.low,
              borderRadiusTopLeft:
                this.glucosePercentages.sHigh +
                  this.glucosePercentages.high +
                  this.glucosePercentages.inTarget ==
                  0 && this.glucosePercentages.low > 0
                  ? 5
                  : null,
              borderRadiusTopRight:
                this.glucosePercentages.sHigh +
                  this.glucosePercentages.high +
                  this.glucosePercentages.inTarget ==
                  0 && this.glucosePercentages.low > 0
                  ? 5
                  : null,
              borderRadiusBottomLeft:
                this.glucosePercentages.sLow == 0 &&
                this.glucosePercentages.low > 0
                  ? 5
                  : null,
              borderRadiusBottomRight:
                this.glucosePercentages.sLow == 0 &&
                this.glucosePercentages.low > 0
                  ? 5
                  : null
            },
            // very low
            {
              data: [this.glucosePercentages.sLow],
              color: smbgColors.slow,
              borderRadiusTopLeft:
                this.glucosePercentages.sHigh +
                  this.glucosePercentages.high +
                  this.glucosePercentages.inTarget +
                  this.glucosePercentages.low ==
                  0 && this.glucosePercentages.sLow > 0
                  ? 5
                  : null,
              borderRadiusTopRight:
                this.glucosePercentages.sHigh +
                  this.glucosePercentages.high +
                  this.glucosePercentages.inTarget +
                  this.glucosePercentages.low ==
                  0 && this.glucosePercentages.sLow > 0
                  ? 5
                  : null,
              borderRadiusBottomLeft:
                this.glucosePercentages.sLow > 0 ? 5 : null,
              borderRadiusBottomRight:
                this.glucosePercentages.sLow > 0 ? 5 : null
            }
          ]
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
      if (x.difference.currentLanguage || x.difference.unit) {
        this.invalidate();
      }
    },

    getHourScalePercent(value) {
      return 1440 * (value / 100);
    },

    floorHours(value) {
      return Math.floor(value / 60);
    },

    modMinutes(value) {
      return utils.round(value % 60);
    },

    getHourScaleFormatted(type, small) {
      let time = moment(this.dateBase).add({
        hours: this.hourScaledValues.hours[type],
        minutes: this.hourScaledValues.minutes[type]
      });

      return time.days()
        ? `24 ${small ? this.hourSm : this.hour} 0 ${
            small ? this.minuteSm : this.minute
          }`
        : time.format(small ? this.hourMinuteFormatSm : this.hourMinuteFormat);
    }
  },
  computed: {
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings
        .currentLanguage;
    },
    unit() {
      return this.$store.state.globalSettingsModule.globalSettings.unit;
    },
    lang() {
      return this.$i18n.t("targetRangeChart", this.currentLanguage);
    },
    loadingText() {
      return this.$i18n.t("loadingText", this.currentLanguage);
    },
    glucosePercentages() {
      let result = {
        sLow: utils.getPercentage(
          this.statisticalData.sLow,
          this.statisticalData.count
        ),
        low: utils.getPercentage(
          this.statisticalData.low,
          this.statisticalData.count
        ),
        inTarget: 0,
        high: utils.getPercentage(
          this.statisticalData.high,
          this.statisticalData.count
        ),
        sHigh: utils.getPercentage(
          this.statisticalData.sHigh,
          this.statisticalData.count
        )
      };

      if (this.statisticalData.inTarget) {
        result.inTarget = utils.round(
          100.0 - (result.sLow + result.low + result.high + result.sHigh),
          1
        );
      }

      return result;
    },
    noData() {
      return (
        this.glucosePercentages.sLow +
          this.glucosePercentages.low +
          this.glucosePercentages.inTarget +
          this.glucosePercentages.high +
          this.glucosePercentages.sHigh ==
        0
      );
    },
    percentageStacked() {
      var result = {
        low: utils.round(
          this.glucosePercentages.sLow + this.glucosePercentages.low,
          1
        )
      };

      result.inTarget = utils.round(
        result.low + this.glucosePercentages.inTarget,
        1
      );
      result.high = utils.round(
        result.inTarget + this.glucosePercentages.high,
        1
      );

      result.high = utils.round(
        100 - ((result.high - result.inTarget) / 2 + result.inTarget),
        1
      );

      result.inTarget = utils.round(
        100 - ((result.inTarget - result.low) / 2 + result.low),
        1
      );

      result.low = utils.round(
        100 -
          ((result.low - this.glucosePercentages.sLow) / 2 +
            this.glucosePercentages.sLow),
        1
      );

      if (result.low > 91) {
        result.low = 91;
      }

      if (result.inTarget > result.low - 16) {
        result.inTarget = result.low - 16;
      }

      if (result.high > result.inTarget - 16) {
        result.high = result.inTarget - 16;
      }

      if (result.high < 7) {
        result.high = 7;
      }

      if (result.inTarget < result.high + 16) {
        result.inTarget = result.high + 16;
      }

      if (result.low < result.inTarget + 16) {
        result.low = result.inTarget + 16;
      }

      return result;
    },
    percentageReferences() {
      return {
        sLow: [
          agpConstants[this.unit].min.toFixed(1),
          agpConstants[this.unit].sLowLimit.toFixed(1)
        ],
        low: [
          (agpConstants[this.unit].sLowLimit + 0.1).toFixed(1),
          agpConstants[this.unit].lowLimit.toFixed(1)
        ],
        inTarget: [
          (agpConstants[this.unit].lowLimit + 0.1).toFixed(1),
          agpConstants[this.unit].highLimit.toFixed(1)
        ],
        high: [
          (agpConstants[this.unit].highLimit + 0.1).toFixed(1),
          agpConstants[this.unit].sHighLimit.toFixed(1)
        ],
        sHigh: [
          (agpConstants[this.unit].sHighLimit + 0.1).toFixed(1),
          agpConstants[this.unit].max.toFixed(1)
        ]
      };
    },
    targetRangeStacked() {
      let result = {
        sLow: this.glucosePercentages.sLow,
        low: this.glucosePercentages.low,
        inTarget: this.glucosePercentages.inTarget,
        high: this.glucosePercentages.high
      };

      result.low = utils.round(result.low + result.sLow, 1);
      result.inTarget = utils.round(result.inTarget + result.low, 1);
      result.high = utils.round(result.high + result.inTarget, 1);

      if (result.sLow > 85) {
        result.sLow = 85;
      }

      if (result.low < result.sLow + 5) {
        result.low = result.sLow + 5;
      } else if (result.low > result.inTarget - 5) {
        result.low = result.inTarget - 5;
      }

      if (result.inTarget < result.low + 5) {
        result.inTarget = result.low + 5;
      } else if (result.inTarget > result.high - 5) {
        result.inTarget = result.high - 5;
      }

      if (result.high < result.inTarget + 5) {
        result.high = result.inTarget + 5;
      }

      result.sLow = utils.round(100 - result.sLow, 1);
      result.low = utils.round(100 - result.low, 1);
      result.inTarget = utils.round(100 - result.inTarget, 1);
      result.high = utils.round(100 - result.high, 1);

      return result;
    },
    targetRangeStackedValues() {
      return {
        sHigh: utils.round(agpConstants[this.unit].sHighLimit, 1).toFixed(1),
        high: utils.round(agpConstants[this.unit].highLimit, 1).toFixed(1),
        low: utils.round(agpConstants[this.unit].lowLimit, 1).toFixed(1),
        sLow: utils.round(agpConstants[this.unit].sLowLimit, 1).toFixed(1)
      };
    },
    showTargetRangeStackedSlow() {
      return (
        this.glucosePercentages.sLow > 0 && this.glucosePercentages.low > 0
      );
    },
    hourMinuteFormat() {
      return `H [${this.hour}] m [${this.minute}]`;
    },
    hourMinuteFormatSm() {
      return `H[${this.hourSm}] m[${this.minuteSm}]`;
    },
    dateBase() {
      return new Date(0, 0, 0);
    },
    hourScaledValues() {
      let sHigh = this.getHourScalePercent(this.glucosePercentages.sHigh);
      let high = this.getHourScalePercent(this.glucosePercentages.high);
      let low = this.getHourScalePercent(this.glucosePercentages.low);
      let sLow = this.getHourScalePercent(this.glucosePercentages.sLow);

      let result = {
        hours: {
          sHigh: this.floorHours(sHigh),
          high: this.floorHours(high),
          inTarget: 0,
          low: this.floorHours(low),
          sLow: this.floorHours(sLow)
        },
        minutes: {
          sHigh: this.modMinutes(sHigh),
          high: this.modMinutes(high),
          inTarget: 0,
          low: this.modMinutes(low),
          sLow: this.modMinutes(sLow)
        }
      };

      let inTargetMinutes =
        1440 -
        (60 *
          (result.hours.sHigh +
            result.hours.high +
            result.hours.low +
            result.hours.sLow) +
          (result.minutes.sHigh +
            result.minutes.high +
            result.minutes.low +
            result.minutes.sLow));

      result.hours.inTarget = this.floorHours(inTargetMinutes);
      result.minutes.inTarget = this.modMinutes(inTargetMinutes);

      return result;
    },
    hourScalesSm() {
      return {
        sHigh: this.getHourScaleFormatted("sHigh", true),
        high: this.getHourScaleFormatted("high", true),
        inTarget: this.getHourScaleFormatted("inTarget", true),
        low: this.getHourScaleFormatted("low", true),
        sLow: this.getHourScaleFormatted("sLow", true)
      };
    },
    hourScales() {
      return {
        sHigh: this.getHourScaleFormatted("sHigh"),
        high: this.getHourScaleFormatted("high"),
        inTarget: this.getHourScaleFormatted("inTarget"),
        low: this.getHourScaleFormatted("low"),
        sLow: this.getHourScaleFormatted("sLow")
      };
    },
    hour() {
      return this.$i18n.t("hour", this.currentLanguage);
    },
    hourSm() {
      return this.$i18n.t("hourSm", this.currentLanguage);
    },
    minute() {
      return this.$i18n.t("minute", this.currentLanguage);
    },
    minuteSm() {
      return this.$i18n.t("minuteSm", this.currentLanguage);
    }
  },
  mounted() {
    this.invalidate();

    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$on("rangeChanged", this.invalidate);
  },
  beforeDestroy() {
    this.destroy();

    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$off("rangeChanged", this.invalidate);
  },
  props: ["statisticalData"]
};
</script>
