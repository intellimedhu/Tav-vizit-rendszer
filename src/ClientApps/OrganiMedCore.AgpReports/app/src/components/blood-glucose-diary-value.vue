<template>
    <div>
        <div v-for="value in cellValues" v-bind:key="value.key" class="diary-value-container print-not-break">
              <span v-if="value.value"
                    class="diary-glucose-badge badge-diary-value print-not-break" 
                    :class="{
                      'text-slow': value.agpIsVeryLow,
                      'text-low': value.agpIsLow,
                      'text-high': value.agpIsHigh,
                      'text-shigh': value.agpIsVeryHigh,
                      'text-target': value.agpIsInTarget
                    }"
                    :title="value.date | shortTime">
                    {{ getValueInUnit(value) }}
                    <i class="icon icon-caret-down" v-if="value.agpIsVeryLow"></i>
                    <i class="icon icon-angle-down" v-if="value.agpIsLow"></i>
                    <i class="icon icon-angle-up" v-if="value.agpIsHigh"></i>
                    <i class="icon icon-caret-up" v-if="value.agpIsVeryHigh"></i>
              </span>

              <span class="badge badge-diary-value badge-pill diary-insulin-badge text-white print-not-break"
                    v-if="value.unit"
                    :title="`${value.unit}, ${shortTime(value.date)}, ${value.duration} ${secUnit}`"
                    :style="`background-color: ${getInsulinBg(value)}`">
                    {{ value.unit }}
              </span>
        </div>
    </div>
</template>
<script>
import utils from "../services/utils";

export default {
  methods: {
    getValueInUnit(bg) {
      let numberValue = utils
        .changeUnit(bg.value, this.originalUnit, this.unit)
        .toFixed(1);
      if (this.unit != "mmol/l") {
        return numberValue;
      }

      if (numberValue < bg.bgmrangelow) {
        return this.bgLowText;
      }

      if (numberValue > bg.bgmrangehigh) {
        return this.bgHighText;
      }

      return numberValue;
    },

    getInsulinBg(insulin) {
      let insulinType = this.insulinTypes.find(x => x.type == insulin.type);
      return insulinType ? insulinType.color : null;
    },

    shortTime(date) {
      return utils.shortTime(date);
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    unit() {
      return this.globalSettings.unit;
    },
    secUnit() {
      return this.$i18n.t("secUnit", this.globalSettings.currentLanguage);
    }
  },
  props: [
    "cellValues",
    "originalUnit",
    "bgLowText",
    "bgHighText",
    "insulinTypes"
  ]
};
</script>
