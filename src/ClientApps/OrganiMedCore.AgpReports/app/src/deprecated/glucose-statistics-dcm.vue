<template>
    <div class="col-md-9 col-lg-10 mt-4 mt-md-0 align-self-top statistics-column statistics-column-glucose">
      <div class="statistics-title">
        {{ lang.glucoseStatisticsVTitle }}
      </div>
      
      <div class="row">
        <div class="col statistic-group statistic-group-4">
          <div class="row">
            <div class="col statistic-block col-bordered">
              <div class="statistic-header line-2x">
                {{ lang.averageGlucosTitle }}
                <br />
                {{ globalSettings.unit }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference"></div>
                <div class="statistics-value">
                  {{ statisticalData.avg | round(1) }}
                </div>
                <div class="statistics-reference">
                  {{ referenceAvgMin }} - {{ referenceAvgMax }} *
                </div>
              </div>
            </div>

            <div class="col statistic-block col-bordered">
                <div class="statistic-header line-2x">
                    {{ lang.sdTitle }}
                    <br>
                    {{ globalSettings.unit }}
                </div>

                <div class="statistic-body">
                    <div class="statistics-reference"></div>
                    <div class="statistics-value">
                      {{ statisticalData.sd | round(1) }}
                      <span class="statistics-value-unit">
                        {{ globalSettings.unit }}
                      </span>
                    </div>

                    <div class="statistics-reference">
                      {{ referenceSdMin }} - {{ referenceSdMax }} *
                    </div>
                </div>
            </div>

            <div class="col statistic-block col-bordered">
              <div class="statistic-header line-2x">
                {{ lang.covTitleP1 }}
                <br>
                {{ lang.covTitleP2 }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference"></div>
                <div class="statistics-value">
                  {{ statisticalData.cv | round(1) }}
                </div>

                <div class="statistics-reference">
                  19.25*
                </div>
              </div>
            </div>

            <div class="col statistic-block">
              <div class="statistic-header line-2x">
                  {{ lang.estimatedHbA1cTitleP1 }}
                  <br />
                  {{ lang.hbA1c }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference"></div>
                <div class="statistics-value" v-if="statisticalData.count >= globalSettings.minimumMeasurementsOfHbA1c">
                  {{ statisticalData.hbA1c.toFixed(1) }}
                  <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-hbA1c-warning" v-show="statisticalData.count < globalSettings.minimumMeasurementsOfHbA1c">
                  {{ lang.minRequiredMeasurements.replace('{0}', globalSettings.minimumMeasurementsOfHbA1c) }}
                </div>

                <div class="statistics-reference">
                  &lt;6*
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col statistic-group statistic-group-5">
          <div class="row">
            <div class="col statistic-block statistic-block-slow col-bordered" :class="`pc-${percentAsHtml['sLow']}`">
              <div class="statistic-header line-2x">
                {{ lang.seriousLowTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference">
                  {{ slowBelow }}
                  <br />
                  {{ globalSettings.unit }}
                </div>

                <div class="statistics-value">
                  {{ glucosePercentages.sLow | round(1) }}
                  <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-reference">
                  0*
                </div>
              </div>
            </div>

            <div class="col statistic-block statistic-block-low col-bordered" :class="`pc-${percentAsHtml['low']}`">
              <div class="statistic-header line-2x">
                {{ lang.lowTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference">
                    {{ lowBelow }}
                    <br>
                    {{ globalSettings.unit }}
                </div>

                <div class="statistics-value">
                    {{ glucosePercentages.low | round(1) }}
                    <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-reference">
                    &lt;4*
                </div>
              </div>
            </div>

            <div class="col statistic-block statistic-block-in-target col-bordered" :class="`pc-${percentAsHtml['inTarget']}`">
              <div class="statistic-header line-2x">
                  {{ lang.inTargetRangeTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference">
                  {{ referenceLow | round(1) }} - {{ referenceHigh | round(1) }}
                  <br>
                  {{ globalSettings.unit }}
                </div>

                <div class="statistics-value">
                  {{ glucosePercentages.inTarget | round(1) }}
                  <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-reference">
                  &gt;90*
                  <br>
                  {{ lang.glucoseRangesSubtitle }}
                </div>
              </div>
            </div>

            <div class="col statistic-block statistic-block-high col-bordered" :class="`pc-${percentAsHtml['high']}`">
              <div class="statistic-header line-2x">
                {{ lang.HighTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference">
                  {{ highAbove }}
                  <br>
                  {{ globalSettings.unit }}
                </div>

                <div class="statistics-value">
                  {{ glucosePercentages.high | round(1) }}
                  <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-reference">
                  &lt;6*
                </div>
              </div>
            </div>

            <div class="col statistic-block statistic-block-shigh" :class="`pc-${percentAsHtml['sHigh']}`">
              <div class="statistic-header line-2x">
                {{ lang.seriousHighTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-reference">
                  {{ shighAbove }}
                  <br>
                  {{ globalSettings.unit }}
                </div>

                <div class="statistics-value">
                  {{ glucosePercentages.sHigh | round(1) }}
                  <span class="statistics-value-unit">%</span>
                </div>

                <div class="statistics-reference">
                  0*
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col statistic-group statistic-group-1">
          <div class="row">
            <div class="col statistic-block statistic-block-double-value">
              <div class="statistic-header">
                {{ lang.averageTestsPerDayTitle }}
              </div>

              <div class="statistic-body">
                <div class="statistics-value">
                  {{ statisticalData.atpd | round(1) }}
                </div>

                <div class="statistics-reference">
                  {{ lang.averageTestsPerDayUnit }}
                </div>
              </div>

              <div class="statistic-header">
                {{ lang.measurementsCount }}
              </div>

              <div class="statistic-body">
                <div class="statistics-value">
                  {{ statisticalData.count }}
                </div>
                <div class="statistics-reference" v-show="lang.measurementsCountUnit">
                  {{ lang.measurementsCountUnit }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- <div class="col statistic-group statistic-group-2 statistic-group-with-footer statistic-group-glucose-variability">
          <div class="row">
            <div class="col statistic-block-footer glucose-variabilty">
                {{ lang.glucoseVariabiltySubtitle }}
            </div>
          </div>
        </div> -->
      </div>
    </div>
</template>

<script>
import utils from "./utils";
import { agpConstants } from "./utils";

export default {
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    lang() {
      return this.$i18n.t(
        "glucoseStatisticsDcm",
        this.globalSettings.currentLanguage
      );
    },
    slowBelow() {
      return this.lang["belowValue"].replace(
        "{value}",
        utils.round(this.referenceSLow, 1)
      );
    },
    lowBelow() {
      return this.lang["belowValue"].replace(
        "{value}",
        utils.round(this.referenceLow, 1)
      );
    },
    highAbove() {
      return this.lang["aboveValue"].replace(
        "{value}",
        utils.round(this.referenceHigh, 1)
      );
    },
    shighAbove() {
      return this.lang["aboveValue"].replace(
        "{value}",
        utils.round(this.referenceSHigh, 1)
      );
    },
    referenceAvgMin() {
      return this.getReferenceNumber(4.9, 88);
    },
    referenceAvgMax() {
      return this.getReferenceNumber(6.4, 116);
    },
    referenceSLow() {
      return agpConstants[this.globalSettings.unit].sLowLimit;
    },
    referenceLow() {
      return agpConstants[this.globalSettings.unit].lowLimit;
    },
    referenceHigh() {
      return agpConstants[this.globalSettings.unit].highLimit;
    },
    referenceSHigh() {
      return agpConstants[this.globalSettings.unit].sHighLimit;
    },
    referenceSdMin() {
      return this.getReferenceNumber(0.6, 10);
    },
    referenceSdMax() {
      return this.getReferenceNumber(1.4, 26);
    },
    percentAsHtml() {
      let result = {};
      Object.keys(this.glucosePercentages).forEach(key => {
        result[key] = this.glucosePercentages[key].toFixed(1).replace(".", "-");
      });

      return result;
    }
  },
  methods: {
    getReferenceNumber(valueMmolL, valueMgDl) {
      if (!this.globalSettings.unit) {
        return;
      }

      if (this.globalSettings.unit == "mmol/l") {
        return valueMmolL;
      }

      if (this.globalSettings.unit == "mg/dl") {
        return valueMgDl;
      }
    }
  },
  props: [
    "statisticalData",
    "glucosePercentages",
    "originalUnit"
  ]
};
</script>