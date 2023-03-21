<template>
  <statistics-template :title="lang['glucoseStatisticsVTitle']" :blocks="blocks" />
</template>

<script>
import moment from "moment";
import statisticsTemplate from "./statistics-template.vue";
import utils from "../services/utils";

export default {
  components: {
    statisticsTemplate
  },
  computed: {
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings.currentLanguage;
    },
    unit() {
      return this.$store.state.globalSettingsModule.globalSettings.unit;
    },
    lang() {
      return this.$i18n.t("glucoseStatisticsAgp", this.currentLanguage);
    },
    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },
    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },
    startDateFormat() {
      return moment(this.selectedMinDate).format(this.semiLongDateFormat);
    },
    endDateFormat() {
      return moment(this.selectedMaxDate).format(this.semiLongDateFormat);
    },
    semiLongDateFormat() {
      return this.$i18n.t("semiLongDateFormat", this.currentLanguage);
    },
    days() {
      return this.$i18n.t("days", this.currentLanguage);
    },
    countUnit() {
      return this.$i18n.t("countUnit", this.currentLanguage);
    },
    blocks() {
      return [
        {
          id: 0,
          rows: [
            {
              label: `${this.startDateFormat} - ${this.endDateFormat}`,
              dataText: `${this.$store.getters.totalDays} ${this.days}`
            },
            {
              label: `${this.lang["averageTestsPerDayLabel"]} / ${
                this.lang["sumTestsLabel"]
              }`,
              dataText: `${utils
                .round(this.statisticalData.atpd, 1)
                .toFixed(1)} ${this.countUnit} / ${
                this.statisticalData.count
              } ${this.countUnit}`
            }
          ]
        },
        {
          id: 1,
          rows: [
            {
              label: this.lang["averageGlucoseLabel"],
              dataText: `${utils
                .round(this.statisticalData.avg, 1)
                .toFixed(1)} ${this.unit}`
            },
            {
              label: this.lang["gmiLabel"],
              dataText: `${utils
                .round(this.statisticalData.gmi, 1)
                .toFixed(1)} %`
            }
          ]
        },
        {
          id: 2,
          rows: [
            {
              label: this.lang["covLabel"],
              dataText: `${utils
                .round(this.statisticalData.cv, 1)
                .toFixed(1)} %`
            },
            {
              label: this.lang["sdLabel"],
              dataText: `${utils
                .round(this.statisticalData.sd, 1)
                .toFixed(1)} ${this.unit}`
            }
          ]
        }
      ];
    }
  },
  props: {
    statisticalData: Object
  }
};
</script>
