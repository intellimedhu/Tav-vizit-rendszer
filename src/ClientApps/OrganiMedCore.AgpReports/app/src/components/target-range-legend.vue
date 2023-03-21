<template>
    <li :class="wrapperClasses" :style="wrapperTopComputed">
        <div class="col col-percent p-0">
            <strong>{{ glucosePercentage }}%</strong>
        </div>

        <div class="col col-count p-0">
            ({{ measurementsCount }} {{ countUnit }})
        </div>

        <div class="col col-range-name p-0">
            <strong>{{ rangeCaption }}</strong>
            <span class="d-none d-sm-inline range-labels">
                ({{ referenceMin }} {{ unit }} - {{ referenceMax }} {{ unit }})
            </span>
        </div>

        <div class="col col-reference d-sm-none p-0">
            ({{ referenceMin }} {{ unit }} - {{ referenceMax }} {{ unit }})
        </div>

        <div class="col col-hours p-0">
            <span class="d-none d-md-inline">{{ hourScale }}</span>
            <span class="d-md-none">{{ hourScaleSm }}</span>
        </div>
    </li>
</template>
<script>
export default {
  computed: {
    unit() {
      return this.$store.state.globalSettingsModule.globalSettings.unit;
    },
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings.currentLanguage;
    },
    countUnit() {
      return this.$i18n.t("countUnit", this.currentLanguage);
    },
    wrapperTopComputed() {
      if (!this.wrapperTop) {
        return;
      }

      // 12.5: half of the list item's height
      return `top:calc(${this.wrapperTop}% - 12.5px)`;
    }
  },
  props: [
    "wrapperClasses",
    "wrapperTop",
    "glucosePercentage",
    "measurementsCount",
    "rangeCaption",
    "referenceMin",
    "referenceMax",
    "hourScale",
    "hourScaleSm"
  ]
};
</script>
