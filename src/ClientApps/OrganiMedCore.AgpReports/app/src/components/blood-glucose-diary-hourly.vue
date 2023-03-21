<template>
    <div class="row">
        <div class="col-12">
          <div class="diary-table diary-table-hourly" v-if="!isSmallSize">
            <!-- header -->
            <div class="row diary-table-row row-first sticky-top print-not-break">
              <div class="col diary-table-column column-first"></div>
              <div class="col diary-table-column column-value" v-for="hour in hours" v-bind:key="hour">
                {{ hour.toString().padStart(2, '0') }}
              </div>
            </div>

            <!-- body -->
            <div v-for="day in daysLoaded"
                 v-bind:key="day.getTime()"
                 class="row diary-table-row print-not-break"
                 :class="{'weekend': day.getDay() == 0 || day.getDay() == 6 }">
              <div class="col diary-table-column column-first">
                <div>
                  {{ day | dateFormat(dateFormatterFirstLine) }}
                </div>
                <div>
                  {{ day | dateFormat(dateFormatterSecondLine) }}
                </div> 
              </div>

              <div class="col diary-table-column column-value print-not-break" v-for="hour in hours" v-bind:key="hour">
                <blood-glucose-diary-value :cell-values="getValuesInHours(day, hour)"
                                           :insulin-types="insulinTypes"
                                           :original-unit="originalUnit"
                                           :bg-low-text="bgLowText"
                                           :bg-high-text="bgHighText" />
                                           
              </div>
            </div>

            <div class="row d-print-none">
              <div class="col-12 text-center pt-3">
                <button type="button"
                        class="btn btn-sm btn-outline-primary"
                        @click="loadNextDays()"
                        v-show="hasUnloadedDays">
                  <i class="icon-refresh"></i>
                  {{ moreButtonText }}
                </button>
              </div>
            </div>
          </div>

          <div class="row diary-sm diary-sm-hourly" v-if="isSmallSize">
            <blood-glucose-diary-pager />

            <div class="col-12 diary-day">
                <div class="diary-rows">
                  <div class="row diary-row" v-for="hour in hours" v-bind:key="hour">                          
                    <div class="col-2 diary-column diary-column-hours">
                      <span>
                        {{ hour.toString().padStart(2, '0') }}
                      </span>
                    </div>
                    
                    <div class="col-10 diary-column diary-column-values align-self-center">
                      <blood-glucose-diary-value :cell-values="getValuesInHours(diarySelectedDay, hour)"
                                                 :insulin-types="insulinTypes"
                                                 :original-unit="originalUnit"
                                                 :bg-low-text="bgLowText"
                                                 :bg-high-text="bgHighText" />
                                                
                    </div>
                  </div>
                </div>
            </div>
          </div>
      </div>
  </div>
</template>

<script>
import moment from "moment";
import bloodGlucoseDiaryPager from "./blood-glucose-diary-pager.vue";
import bloodGlucoseDiaryValue from "./blood-glucose-diary-value.vue";
import utils from "../services/utils";
import { EventBus } from "../services/event-bus";

const dateAndHourEquals = (date, hour, compareDate) => {
  return (
    compareDate.getFullYear() == date.getFullYear() &&
    compareDate.getMonth() == date.getMonth() &&
    compareDate.getDate() == date.getDate() &&
    compareDate.getHours() == hour
  );
};

export default {
  data() {
    return {
      beginDate: null,
      isSmallSize: false
    };
  },
  components: {
    bloodGlucoseDiaryPager,
    bloodGlucoseDiaryValue
  },
  methods: {
    getValuesInHours(date, hour) {
      if (!this.glucoseValues || !this.insulinValues) {
        return [];
      }

      return this.glucoseValues
        .filter(x => dateAndHourEquals(date, hour, x.date))
        .concat(
          this.insulinValues.filter(x => dateAndHourEquals(date, hour, x.date))
        )
        .sort(utils.sortByDate);
    },

    isSmallWindow() {
      return window.innerWidth < 1024;
    },

    getInitialBeginDate() {
      return moment(new Date(this.selectedMaxDate))
        .subtract(3, "weeks")
        .endOf("week")
        .add(1, "days")
        .toDate();
    },

    loadNextDays() {
      this.beginDate = moment(this.beginDate)
        .subtract(3, "weeks")
        .toDate();
    },

    raiseSmDateChanged() {
      EventBus.$emit("diary-sm-date-changed");
    },

    onVResized() {
      this.isSmallSize = this.isSmallWindow();
    },

    onRangeChanged() {
      this.beginDate = this.getInitialBeginDate();

      this.$store.commit(
        "setDiaryDate",
        moment(this.selectedMaxDate)
          .startOf("day")
          .toDate()
      );
      this.raiseSmDateChanged();
    }
  },
  computed: {
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings
        .currentLanguage;
    },

    lang() {
      return this.$i18n.t("bloodGlucoseDiary", this.currentLanguage);
    },

    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },

    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },

    diarySelectedDay() {
      return this.$store.state.mainModule.diarySelectedDay;
    },

    bgLowText() {
      return this.$i18n.t("bgLowText", this.currentLanguage);
    },

    bgHighText() {
      return this.$i18n.t("bgHighText", this.currentLanguage);
    },

    moreButtonText() {
      return this.$i18n.t("moreButtonText", this.currentLanguage);
    },

    dateFormatterFirstLine() {
      return this.lang.dateFormatterFirstLine;
    },

    dateFormatterSecondLine() {
      return this.lang.dateFormatterSecondLine;
    },

    datepickerFormat() {
      return this.$i18n.t("datepickerFormat", this.currentLanguage);
    },

    hours() {
      return Array.from(Array(24).keys());
    },

    days() {
      let result = [];

      for (
        let currentDate = moment(this.selectedMaxDate)
          .startOf("day")
          .toDate();
        moment(currentDate).isSameOrAfter(this.selectedMinDate);
        currentDate = utils.addDays(currentDate, -1)
      ) {
        result.push(currentDate);
      }

      return result;
    },

    daysLoaded() {
      return this.days.filter(day => utils.dayLoaded(this.beginDate, day));
    },

    hasUnloadedDays() {
      return moment(this.beginDate).isAfter(this.selectedMinDate);
    }
  },
  filters: {
    dateFormat(date, format) {
      return moment(date).format(format);
    }
  },
  mounted() {
    this.beginDate = this.getInitialBeginDate();
    this.isSmallSize = this.isSmallWindow();
    if (!this.diarySelectedDay) {
      this.$store.commit(
        "setDiaryDate",
        moment(this.selectedMaxDate)
          .startOf("day")
          .toDate()
      );
      this.raiseSmDateChanged();
    }

    EventBus.$on("vResized", this.onVResized);
    EventBus.$on("rangeChanged", this.onRangeChanged);
  },
  beforeDestroy() {
    EventBus.$off("vResized", this.onVResized);
    EventBus.$off("rangeChanged", this.onRangeChanged);
  },
  updated() {
    utils.triggerInvalidatedEvent("diary");
  },
  props: ["originalUnit", "glucoseValues", "insulinValues", "insulinTypes"]
};
</script>
