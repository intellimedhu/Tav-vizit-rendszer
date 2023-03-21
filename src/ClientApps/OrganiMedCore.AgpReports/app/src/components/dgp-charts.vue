<template>
    <div class="d-print-none">
        <div class="dgp-sticky sticky-top rounded" v-if="isDcmView" v-show="selectedDays.length">
          <div class="row">
            <div class="col-12">              
              <div class="dgp-toolbar text-center border border-primary p-1 rounded text-primary align-self-center">
                <div class="inner" @click="showDialog()">
                  <i class="dgp-icon icon-bar-chart icon-2x"></i>
                  {{ selectedDays.length }} {{ daysSelectedText }}
                </div>
                <i class="dgp-icon dgp-icon-remove icon-remove-circle" @click="clearSelection()" :title="lang['clearSelection']"></i>
              </div>
            </div>
          </div>
        </div>

        <div class="dgp-container">
          <div class="row dgp-main">
            <span class="dgp-title">
              {{ lang.chartTitle }}
            </span>

            <div class="col dgp-content col-sm-12">
              <div class="row dgp-row dgp-row-header">
                <div class="col dgp-column dgp-column-week"></div>
                <div class="col dgp-column-week-wrapper">
                  <div class="row">
                    <div class="col dgp-column dgp-column-week-values" v-for="weekDay in weekDays" v-bind:key="weekDay">
                      <span :class="{'clickable': isDcmView}"
                            @click="selectDaysRepeatedly(weekDay)">
                        {{ lang.days[weekDay] }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            
              <div v-for="week in weeks" v-bind:key="week[0].getTime()">
                <div class="row dgp-row dgp-row-body">
                  <div class="col dgp-column dgp-column-week"
                       :class="{'clickable': isDcmView}"
                       :title="week[0] | weekTooltip(datepickerFormat)"
                       @click="selectWeek(week)">
                    <div>
                      <span class="year">{{ week[0].getFullYear() }}</span>
                      <span class="space">_</span>
                      <span class="month" v-if="isDcmView">{{ week[0] | weekCaptionDcm }}.</span>
                      <span class="month" v-else>{{ week[0] | weekCaptionAgp }}.</span>
                    </div>
                  </div>

                  <div class="col dgp-column-week-wrapper">
                    <div class="row dgp-row-for-columns">
                      <div :class="`col dgp-column dgp-column-week-values dgp-column-${ getDayColumnOrder(day.getDay())} ${!isBetween(day) ? 'out-of-range': 'in-range'}`"
                           v-for="day in week" v-bind:key="day.getTime()">
                        <div class="dgp-chart-wrapper"
                             v-if="isBetween(day)">
                          <span class="day-name">
                            {{ lang.days[day.getDay()] }}
                          </span>

                          <span class="date">                            
                            {{ day.getDate() }}
                          </span>
                          
                          <button v-if="isDcmView"
                                class="btn btn-sm btn-dgp-day-selector"
                                :class="{'btn-outline-dgp unchecked': !isDaySelected(day), 'btn-dgp': isDaySelected(day) }"
                                @click="selectDay(day)">
                            <i class="icon icon-ok"></i>
                          </button>

                          <div class="dgp-chart-area">
                            <dgp-chart :glucose-values="valuesForDay(true, day)"
                                       :insulin-values="valuesForDay(false, day)"
                                       :insulin-types="insulinTypes"
                                       :original-unit="originalUnit"
                                       :y-axis-max-value="dailyChartsVMax" />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="row" v-show="hasUnloadedWeeks">
            <div class="col-12 text-center mt-2">
              <button type="button" class="btn btn-sm btn-outline-dgp" @click="loadNextPeriod()">
                <i class="icon-refresh"></i>
                {{ moreButtonText }}
              </button>
            </div>
          </div>
        </div>

        <b-modal ref="custom-dgp-dialog-modal"
                 size="fs"
                 scrollable
                 :no-close-on-esc="true"
                 :no-close-on-backdrop="true"
                 :hide-footer="true"
                 :hide-header="true"
                 v-if="isDcmView">
            <template slot="default">
              <div ref="print-content" class="p-3">
                <button type="button"
                            class="btn btn-sm btn-outline-secondary float-right"
                            data-dismiss="modal"
                            aria-label="Close"
                            @click="closeCustomDgpModal()">
                        <i class="icon-remove"></i>
                    </button>

                    <button type="button"
                            :disabled="!filteredGlucoseValuesBySelection.length && !filteredInsulinValuesBySelection.length"
                            class="btn btn-sm btn-primary float-right mr-1"
                            data-dismiss="modal"
                            aria-label="Print"
                            @click="printContent()">
                        <i class="icon-print"></i>
                    </button>

                <dgp-custom-chart :glucose-values="filteredGlucoseValuesBySelection"
                                  :insulin-values="filteredInsulinValuesBySelection"
                                  :insulin-brands="insulinBrandsForSelectedDays"
                                  :insulin-types="insulinTypes"
                                  :selected-days="selectedDays"
                                  :original-unit="originalUnit"
                                  :chartTitle="modalTitle" />
              </div>
            </template>
        </b-modal>
    </div>
</template>

<script>
import moment from "moment";
import dgpChart from "./dgp-chart.vue";
import dgpCustomChart from "./dgp-custom-chart.vue";
import utils, {
  betweenDateRange,
  groupByDate,
  groupToArray
} from "../services/utils";
import { EventBus } from "../services/event-bus";

const filterForSelectedDays = (x, selectedDays) => {
  return selectedDays.filter(y => {
    return (
      moment(y).isBefore(x.date) &&
      moment(y)
        .add(1, "days")
        .isAfter(x.date)
    );
  }).length;
};

export default {
  data() {
    return {
      chartsBeginDate: null,
      selectableDaysCount: 31,
      selectedDays: [],
      resetOnResize: true,
      weekStartDayIndex: 0,
      weeks: [],
      invalidatedDgpCharts: 0
    };
  },
  components: {
    dgpChart,
    dgpCustomChart
  },
  methods: {
    getInitialChartsBeginDate() {
      let startDate = moment(this.selectedMaxDate)
        .subtract(window.innerWidth < 768 ? 1 : 3, "weeks")
        .startOf("week");

      if (startDate.isBefore(this.selectedMinDate)) {
        startDate = moment(this.selectedMinDate);
      }

      return startDate.toDate();
    },

    calculateWeekStartDayIndex() {
      // It may differ between cultures.
      this.weekStartDayIndex = moment()
        .startOf("week")
        .day();
    },

    calculateWeeks() {
      let endDate = moment(this.selectedMaxDate)
        .endOf("week")
        .startOf("day");

      let startDate = moment(this.chartsBeginDate).startOf("week");

      this.weeks = [];
      for (
        let currentWeek = endDate.startOf("week").toDate();
        moment(startDate).isSameOrBefore(currentWeek);
        currentWeek = moment(currentWeek)
          .subtract(1, "weeks")
          .toDate()
      ) {
        let week = [];
        for (let i = 0; i <= 6; i++) {
          week.push(
            moment(currentWeek)
              .add(i, "days")
              .toDate()
          );
        }

        this.weeks.push(week);
      }
    },

    showDialog() {
      this.$refs["custom-dgp-dialog-modal"].show();
    },

    selectDay(day) {
      this.onDaySelection(day, this.warnDaySelection);
    },

    onDaySelection(day, onReachedSelectableDaysCount) {
      if (!this.isDcmView) {
        return;
      }

      if (this.isDaySelected(day)) {
        this.selectedDays = this.selectedDays.filter(
          x => !utils.dateEquals(x, day)
        );
      } else if (this.selectedDays.length < this.selectableDaysCount) {
        this.selectedDays.push(day);
      } else {
        onReachedSelectableDaysCount();
      }
    },

    selectGivenDays(daysToSelect) {
      if (!this.isDcmView) {
        return;
      }

      let alertSelectableDays = true;
      daysToSelect.forEach(x => {
        this.onDaySelection(x, () => {
          if (alertSelectableDays) {
            this.$nextTick(() => {
              this.warnDaySelection();
            });
            alertSelectableDays = false;
          }
        });
      });
    },

    warnDaySelection() {
      alert(
        this.lang["daySelectionWarnMessage"].replace(
          "{0}",
          this.selectableDaysCount
        )
      );
    },

    selectWeek(week) {
      if (!this.isDcmView) {
        return;
      }

      let days = week.filter(x =>
        betweenDateRange(x, this.selectedMinDate, this.selectedMaxDate)
      );

      // Unselected days on the week.
      let unselectedDays = days.filter(x => !this.isDaySelected(x));

      this.selectGivenDays(
        unselectedDays.length
          ? unselectedDays.filter(x =>
              betweenDateRange(x, this.selectedMinDate, this.selectedMaxDate)
            )
          : days
      );
    },

    selectDaysRepeatedly(dayOfWeek) {
      if (!this.isDcmView) {
        return;
      }

      let startWeek = moment(this.selectedMinDate)
        .startOf("week")
        .toDate();
      let dayDiff = dayOfWeek - startWeek.getDay();

      // sunday
      if (dayDiff == -1) {
        dayDiff = 6;
      }

      let startDay = moment(startWeek).add(dayDiff, "days");
      if (startDay.isBefore(this.selectedMinDate)) {
        startDay = startDay.add(1, "weeks");
      }

      // Is is not allowed to select those days that are before the 'rendered' days.
      while (startDay.isBefore(this.chartsBeginDate)) {
        startDay = startDay.add(1, "weeks");
      }

      let days = [];
      for (
        let day = startDay;
        day.isSameOrBefore(this.selectedMaxDate);
        day = day.add(1, "weeks")
      ) {
        days.push(day.toDate());
      }

      startDay = null;

      let unselectedDays = days.filter(x => !this.isDaySelected(x));
      this.selectGivenDays(unselectedDays.length ? unselectedDays : days);
    },

    isDaySelected(day) {
      return this.selectedDays.filter(x => moment(x).isSame(day)).length;
    },

    clearSelection() {
      this.selectedDays = [];
    },

    loadNextPeriod() {
      this.chartsBeginDate = moment(this.chartsBeginDate)
        .subtract(window.innerWidth < 768 ? 1 : 3, "weeks")
        .toDate();

      if (moment(this.selectedMinDate).isAfter(this.chartsBeginDate)) {
        this.chartsBeginDate = new Date(this.selectedMinDate);
      }

      this.calculateWeeks();
    },

    isBetween(weekDay) {
      return (
        moment(weekDay).isSameOrAfter(this.chartsBeginDate) &&
        moment(weekDay).isSameOrBefore(this.selectedMaxDate)
      );
    },

    valuesForDay(isGlucose, day) {
      let firstMatch = (isGlucose
        ? this.valuesByDays.glucoseValues
        : this.valuesByDays.insulinValues
      ).filter(x => utils.dateEquals(day, new Date(x.date)))[0];

      if (!firstMatch) {
        return [];
      }

      return firstMatch.values;
    },

    resetSettings() {
      this.chartsBeginDate = this.getInitialChartsBeginDate();
      this.calculateWeekStartDayIndex();
      this.clearSelection();
      this.calculateWeeks();
    },

    getDayColumnOrder(dayIndex) {
      let order = dayIndex - this.weekStartDayIndex;

      return 7 - (order == -1 ? 6 : order);
    },

    closeCustomDgpModal() {
      if (this.isDcmView) {
        this.$refs["custom-dgp-dialog-modal"].hide();
      }
      EventBus.$emit("print-dgp-dialog", { open: false });
    },

    onRangeChanged() {
      // By emptying the array, we achieve that all the charts will be redrawn.
      this.weeks = [];
      this.invalidatedDgpCharts = 0;

      this.$nextTick(() => {
        this.resetSettings();
        this.calculateWeeks();
      });
    },

    onGlobalSettingsUpdated(x) {
      let resetting = false;
      if (
        x.difference.dgpChartType ||
        x.difference.displayNonMeasurableValues ||
        x.difference.currentLanguage
      ) {
        resetting = true;
      }

      if (x.difference.currentLanguage) {
        this.clearSelection();
        this.calculateWeekStartDayIndex();
        this.calculateWeeks();
      }

      if (!resetting) {
        for (let propertyName in x.difference) {
          if (propertyName.indexOf("insulinType-") > -1) {
            resetting = true;
          }
        }
      }

      if (resetting) {
        this.resetSettings();
      }
    },

    modalShown(e, modalId) {
      EventBus.$emit("dgp-custom-chart-show", this.invalidate);
    },

    printContent() {
      EventBus.$emit("print-dgp-dialog", { open: true });
    },

    dgpChartInvalidated() {
      if (++this.invalidatedDgpCharts == this.displayedDays.length) {
        utils.triggerInvalidatedEvent("dgp");
      }
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },

    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },

    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },

    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },

    filteredGlucoseValuesBySelection() {
      if (!this.isDcmView) {
        return;
      }
      return this.glucoseValues.filter(x =>
        filterForSelectedDays(x, this.selectedDays)
      );
    },

    filteredInsulinValuesBySelection() {
      if (!this.isDcmView) {
        return;
      }
      return this.insulinValues.filter(x =>
        filterForSelectedDays(x, this.selectedDays)
      );
    },

    lang() {
      return this.$i18n.t("dpgCharts", this.globalSettings.currentLanguage);
    },

    displayedDays() {
      return this.weeks
        .reduce((a, b) => {
          return a.concat(b);
        }, [])
        .filter(x => this.isBetween(x));
    },

    modalTitle() {
      return this.lang["modalTitle"].replace(
        "{0}",
        this.$store.getters.getPatientName(this.globalSettings.currentLanguage)
      );
    },

    moreButtonText() {
      return this.$i18n.t(
        "moreButtonText",
        this.globalSettings.currentLanguage
      );
    },

    daysSelectedText() {
      return this.$i18n.t("daysSelected", this.globalSettings.currentLanguage);
    },

    weekDays() {
      let result = [];
      for (
        let i = this.weekStartDayIndex;
        i < this.weekStartDayIndex + 7;
        i++
      ) {
        result.push(i % 7);
      }

      return result;
    },

    datepickerFormat() {
      return this.$i18n.t(
        "datepickerFormat",
        this.globalSettings.currentLanguage
      );
    },

    hasUnloadedWeeks() {
      return moment(this.chartsBeginDate).isAfter(this.selectedMinDate);
    },

    valuesByDays() {
      return {
        glucoseValues: groupToArray(this.glucoseValues.reduce(groupByDate, {})),
        insulinValues: groupToArray(this.insulinValues.reduce(groupByDate, {}))
      };
    },

    insulinBrandsForSelectedDays() {
      if (!this.isDcmView) {
        return;
      }

      if (!this.selectedDays.length) {
        return [];
      }

      return this.insulinValues
        .filter(item => {
          return this.selectedDays.some(day =>
            utils.dateEquals(day, item.date)
          );
        })
        .reduce((accumulator, item) => {
          let brand = accumulator.find(x => x.name == item.name);
          if (!brand) {
            brand = {
              type: item.type,
              name: item.name
            };

            accumulator.push(brand);
          }

          return accumulator;
        }, []);
    }
  },
  filters: {
    weekTooltip(week, formatter, startDate, endDate) {
      if (!week) {
        return;
      }

      let weekStart = moment(week);

      return `${weekStart.format(formatter)} - ${weekStart
        .add(6, "days")
        .format(formatter)}`;
    },

    weekCaptionDcm(weekStart) {
      let weekEnd = utils.addDays(weekStart, 6);
      let weekStartMonth = weekStart.getMonth() + 1;
      let result = weekStartMonth.toString().padStart(2, "0");
      let weekEndMonth = weekEnd.getMonth() + 1;
      if (weekEndMonth != weekStartMonth) {
        result += "/" + weekEndMonth.toString().padStart(2, "0");
      }

      return result;
    },

    weekCaptionAgp(weekStart) {
      return moment(weekStart).week();
    }
  },
  mounted() {
    this.chartsBeginDate = this.getInitialChartsBeginDate();
    this.calculateWeekStartDayIndex();
    this.calculateWeeks();

    EventBus.$on("rangeChanged", this.onRangeChanged);
    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$on("dgp-chart-invalidate-finished", this.dgpChartInvalidated);

    // Cannot unsubscribe from this event.
    this.$root.$on("bv::modal::shown", this.modalShown);
  },
  beforeDestroy() {
    EventBus.$off("rangeChanged", this.onRangeChanged);
    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);
    EventBus.$off("dgp-chart-invalidate-finished", this.dgpChartInvalidated);
  },
  props: [
    "glucoseValues",
    "insulinValues",
    "insulinTypes",
    "dailyChartsVMax",
    "originalUnit"
  ]
};
</script>
