<template>
  <div>
    <nav class="navbar fixed-top navbar-expand-md navbar-light bg-light navbar-charts-navigation">
      <button class="navbar-toggler navbar-collapse-button" type="button" data-toggle="collapse"
              data-target="#navbar-navigation" aria-controls="navbar-navigation" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="date-wrapper">
        <div class="input-group input-group-sm">
          <input type="date"
                 class="form-control form-control-sm date-component datepicker"
                 required="required"
                 v-model="dpStartDateFormat"
                 :min="dpStartMinDate"
                 :max="dpStartMaxDate"
                 :disabled="controlDisabled"
                 @change="dpStartDateChanged" />
          <input type="date" class="form-control form-control-sm date-component datepicker"
                 required="required"
                 v-model="dpEndDateFormat"
                 :min="dpEndMinDate"
                 :max="dpEndMaxDate"
                 :disabled="controlDisabled"
                 @change="dpEndDateChanged" />
          <div class="input-group-append">
            <span class="input-group-text date-component total-days">{{ totalDays }} {{ daysText }}</span>
          </div>
        </div>
      </div>

      <button class="btn btn-primary btn-print btn-print-sm collapse-navigation"
              type="button"
              @click="showDialog('print-modal')"
              :disabled="controlDisabled">
        <i class="icon-print"></i>
      </button>

      <button class="btn btn-primary btn-settings btn-settings-sm"
              type="button"
              @click="showDialog('global-settings-modal', 1)"
              :disabled="controlDisabled">
          <i class="icon-cog"></i>
      </button>

      <div class="collapse navbar-collapse navbar-collapse-buttons" id="navbar-navigation">
        <div class="row mt-2 mt-lg-0">
            <div class="col col-left">
              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation d-md-none d-lg-inline-block"
                      :class="{'btn-outline-primary': namedRange == namedRanges.OneDay, 'btn-primary': namedRange != namedRanges.OneDay}"
                      @click="setRange(1, 'days', false, namedRanges.OneDay)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['day1'] }}</span>
                  <span class="d-lg-none">{{ lang['day1Short'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.OneWeek, 'btn-primary': namedRange != namedRanges.OneWeek}"
                      @click="setRange(1, 'weeks', false, namedRanges.OneWeek)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['week1'] }}</span>
                  <span class="d-lg-none">{{ lang['week1Short'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.TwoWeeks, 'btn-primary': namedRange != namedRanges.TwoWeeks}"
                      @click="setRange(2, 'weeks', false, namedRanges.TwoWeeks)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['weeks2'] }}</span>
                  <span class="d-lg-none">{{ lang['weeks2Short'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.OneMonth, 'btn-primary': namedRange != namedRanges.OneMonth}"
                      @click="setRange(1, 'months', false, namedRanges.OneMonth)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['month1'] }}</span>
                  <span class="d-lg-none">{{ lang['month1Short'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.ThreeMonths, 'btn-primary': namedRange != namedRanges.ThreeMonths}"
                      @click="setRange(3, 'months', false, namedRanges.ThreeMonths)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['months3'] }}</span>
                  <span class="d-lg-none">{{ lang['months3Short'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mr-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.LastThreeMonths, 'btn-primary': namedRange != namedRanges.LastThreeMonths}"
                      @click="setRange(3, 'months', true, namedRanges.LastThreeMonths)"
                      :disabled="controlDisabled">
                  <span class="d-none d-lg-inline">{{ lang['months3Last'] }}</span>
                  <span class="d-lg-none">{{ lang['months3LastShort'] }}</span>
              </button>

              <button type="button"
                      class="btn btn-sm mb-1 mb-md-0 btn-named-range collapse-navigation"
                      :class="{'btn-outline-primary': namedRange == namedRanges.FullRange, 'btn-primary': namedRange != namedRanges.FullRange}"
                      @click="setFullRange()"
                      :disabled="controlDisabled">
                  <span class="d-none d-xl-inline">{{ lang[namedRanges.FullRange] }}:</span>
                  {{ allowedMinDate | displayDate(datepickerFormat) }}-{{ allowedMaxDate | displayDate(datepickerFormat) }}
              </button>
              </div>                    
              <div class="col col-right">
                <button class="btn btn-primary btn-settings collapse-navigation"
                        type="button"
                        @click="showDialog('global-settings-modal', 1)"
                        :disabled="controlDisabled">
                  <i class="icon-cog"></i>
                </button>
              </div>
          </div>
        </div>
      </nav>

      <div class="container-fluid date-range-container-fluid mb-3">
        <div class="row row-date-range">
          <div class="col-12 settings-bar-time-scale-type-selector">
            <div class="time-scale-inner">
              <button v-if="isDcmView"
                      type="button"
                      class="btn btn-scale-type"
                      :class="{'btn-outline-primary': timeScaleType == 'default', 'btn-primary': timeScaleType != 'default'}"
                      @click="changeScaleType('default')"
                      :disabled="controlDisabled">
                {{ lang.timeScaleDefault }}
              </button>
              <button v-if="isDcmView"
                      type="button"
                      class="btn btn-scale-type"
                      :class="{'btn-outline-primary': timeScaleType == 'glucose', 'btn-primary': timeScaleType != 'glucose'}"
                      @click="changeScaleType('glucose')"
                      :disabled="controlDisabled">
                {{ lang.timeScaleGlucose }}
              </button>
              <button v-if="isDcmView"
                      type="button"
                      class="btn btn-scale-type"
                      :class="{'btn-outline-primary': timeScaleType == 'insulin', 'btn-primary': timeScaleType != 'insulin'}"
                      @click="changeScaleType('insulin')"
                      :disabled="controlDisabled">
                {{ lang.timeScaleInsulin }}
              </button>
              <!-- <button v-if="isDcmView"
                           type="button"
                           class="btn btn-scale-type"
                           :class="{'btn-outline-primary': timeScaleType == 'steps', 'btn-primary': timeScaleType != 'steps'}"
                           @click="changeScaleType('steps')"
                           :disabled="controlDisabled">
                {{ lang.timeScaleSteps }}
              </button> -->
            </div>
          </div>
          <div class="col-12 p-0">
            <div id="dashboard-date-range-container" class="date-range-dashboard">
              <div class="row row-range-filter">
                <div class="col year-pager year-pager-backward">
                  <button type="button"
                          class="btn btn-primary btn-range-step"
                          :class="{ 'btn-primary': canStepBackward, 'btn-secondary': !canStepBackward }"
                          :disabled="!canStepBackward || controlDisabled"
                          @click="stepBackward()">
                    <i class="icon-double-angle-left"></i>
                  </button>
                </div>
                <div id="dashboard-filter-container" class="col dashboard-filter-container"></div>
                <div class="col year-pager year-pager-forward">
                  <button type="button"
                          class="btn btn-range-step"
                          :class="{ 'btn-primary': canStepForward, 'btn-secondary': !canStepForward }"
                          :disabled="!canStepForward || controlDisabled"
                          @click="stepForward()">
                    <i class="icon-double-angle-right"></i>
                  </button>
                </div>
              </div>
              <div class="date-range-dashboard-spinner loading-spinner text-primary d-print-none" :class="{'d-none': !controlDisabled}">
                <div class="spinner-border" role="status">
                  <span class="sr-only">{{ pleaseWaitText }}</span>
                </div>
                <small class="d-block">{{ pleaseWaitText }}</small>
              </div>
            </div>          
          </div>
        </div>
      </div>

    <b-modal ref="global-settings-modal"
             size="xl"
             scrollable
             :no-close-on-esc="true"
             :no-close-on-backdrop="true"
             :hide-footer="true">
        <template slot="modal-header">
            <div class="row w-100">
                <div class="col">
                    <h4 class="modal-title mt-1 mb-1 d-inline-block">
                        <i class="icon-cog"></i>
                        {{ lang['globalSettingsDialogTitle']}}
                    </h4>
                    <button type="button"
                            class="btn btn-sm btn-outline-secondary float-right"
                            data-dismiss="modal"
                            aria-label="Close"
                            @click="closeDialog('global-settings-modal')">
                        <i class="icon-remove"></i>
                    </button>
                </div>
            </div>
        </template>

        <template slot="default">
            <global-settings-dialog :insulin-types="insulinTypes" />
        </template>
    </b-modal>

    <b-modal ref="print-modal"
             scrollable
             :no-close-on-esc="true"
             :no-close-on-backdrop="true">
        <template slot="modal-header">
            <div class="row w-100">
                <div class="col">
                    <h4 class="modal-title mt-1 mb-1 d-inline-block">
                        <i class="icon-print"></i>
                        {{ lang['printDialogTitle'] }}
                    </h4>
                    <button type="button"
                            class="btn btn-sm btn-outline-secondary float-right"
                            data-dismiss="modal"
                            aria-label="Close"
                            @click="closeDialog('print-modal', 1)">
                        <i class="icon-remove"></i>
                    </button>
                </div>
            </div>
        </template>

        <template slot="default">
            <div v-if="initialized" class="p-3">
                <div class="form-group row">
                  <label class="col-sm-6 col-form-label col-form-label-sm">
                    {{ lang['printGroupCharts'] }}
                  </label>

                  <div class="col-sm-6">
                    <div class="btn-group" role="group">
                      <button type="button" 
                              class="btn btn-sm"
                              :class="{
                                'btn-primary': printOptions.charts,
                                'btn-outline-primary': !printOptions.charts
                              }"
                              @click="setPrintSection('charts', true)">
                        {{ yesText }}
                      </button>

                      <button type="button"
                              class="btn btn-sm"
                              :class="{
                                'btn-primary': !printOptions.charts,
                                'btn-outline-primary': printOptions.charts
                              }"
                              @click="setPrintSection('charts', false)">
                        {{ noText }}
                      </button>
                    </div>
                  </div>
                </div>

                <div class="row" v-show="isDcmView">
                  <label class="col-sm-6 col-form-label col-form-label-sm">
                    {{ lang['printGroupDiary'] }}
                  </label>

                  <div class="col-sm-6">
                    <div class="btn-group" role="group">
                      <button type="button"
                              class="btn btn-sm"
                              :class="{
                                'btn-primary': printOptions.diary,
                                'btn-outline-primary': !printOptions.diary
                              }"
                              @click="setPrintSection('diary', true)">
                        {{ yesText }}
                      </button>

                      <button type="button"
                              class="btn btn-sm"
                              :class="{
                                'btn-primary': !printOptions.diary,
                                'btn-outline-primary': printOptions.diary
                              }"
                              @click="setPrintSection('diary', false)">
                        {{ noText }}
                      </button>
                    </div>
                  </div>
                </div>

                <p v-show="!printOptionsSelected" class="alert alert-warning border-warning mt-3">
                  <i class="icon-warning-sign"></i>
                  {{ lang['printDialogNothingToPrint'] }}
                </p>
              </div>
        </template>

        <template slot="modal-footer">
            <button type="button"
                    class="btn btn-sm"
                    :class="{'btn-primary': printOptionsSelected, 'btn-outline-primary': !printOptionsSelected}"
                    @click="print()"
                    :disabled="!printOptionsSelected">
                {{ okText }}
            </button>
        </template>
    </b-modal>
  </div>
</template>

<script>
import moment from "moment";
import globalSettingsDialog from "./global-settings-dialog.vue";
import { EventBus } from "../services/event-bus";
import utils, { betweenDateRange, smbgColors } from "../services/utils";
import $ from "jquery";
import highcharts from "highcharts/highstock";
import boost from "highcharts/modules/boost";
boost(highcharts);

const NamedRanges = {
  OneDay: "day1",
  OneWeek: "week1",
  TwoWeeks: "weeks2",
  OneMonth: "month1",
  ThreeMonths: "months3",
  LastThreeMonths: "months3Last",
  FullRange: "fullRange",
  Custom: "custom"
};

export default {
  components: {
    globalSettingsDialog
  },
  data() {
    return {
      highstock: null,
      chartRangeSlider: null,
      dpStartDate: null,
      dpStartDateFormat: null,
      dpEndDate: null,
      dpEndDateFormat: null,
      focusStartDate: null,
      focusEndDate: null,
      printOptions: null,
      printDialogResult: "none",
      initialized: false,
      namedRange: NamedRanges.LastThreeMonths,
      invalidatedComponents: {
        agp: false,
        dgp: false,
        diary: false,
        insulinStatistics: false,
        targetRange: false
      }
    };
  },
  methods: {
    initialize() {
      let now = new Date();

      this.dpStartDate = utils.subtract3Months(now);
      this.dpStartDateFormat = moment(this.dpStartDate).format("YYYY-MM-DD");

      this.dpEndDate = moment(now)
        .endOf("day")
        .toDate();
      this.dpEndDateFormat = moment(this.dpEndDate).format("YYYY-MM-DD");

      this.$store.commit("setSelectedRange", {
        start: this.dpStartDate,
        end: this.dpEndDate
      });

      this.focusEndDate = moment(now)
        .endOf("day")
        .toDate();
      this.focusStartDate = moment(this.focusEndDate)
        .subtract(1, "years")
        .toDate();
      if (moment(this.focusStartDate).isBefore(this.allowedMinDate)) {
        this.focusStartDate = new Date(this.allowedMinDate);
      }

      this.focusEndDate = new Date(this.dpEndDate);

      this.printOptions = Object.assign({}, this.globalSettings.printOptions);

      this.initialized = true;
      now = null;
    },

    invalidateIfRangeChanged(startDate, endDate, calledFromNavigator) {
      let changed = false;
      let now = moment();

      if (startDate < this.allowedMinDate) {
        startDate = this.allowedMinDate;
      }

      if (now.isBefore(endDate)) {
        endDate = now.endOf("day").toDate();
      }

      if (endDate < startDate) {
        startDate = moment(endDate)
          .subtract(1, "months")
          .toDate();
      }

      let currentTotalDays = this.totalDays;
      let startDateMovedUpper = this.dpStartDate < startDate;
      if (!utils.dateEquals(this.dpStartDate, startDate)) {
        this.dpStartDate = startDate;
        this.dpStartDateFormat = moment(startDate).format("YYYY-MM-DD");
        changed = true;
      }

      let endDateMovedLower =
        !startDateMovedUpper &&
        moment(this.dpEndDate)
          .startOf("day")
          .isAfter(moment(endDate).startOf("day"));
      if (!utils.dateEquals(this.dpEndDate, endDate)) {
        this.dpEndDate = endDate;
        this.dpEndDateFormat = moment(endDate).format("YYYY-MM-DD");
        changed = true;
      }

      let chartModeChanged = false;
      if (changed) {
        this.disableControl();
        this.setNamedSelectedRange();

        this.$store.commit("setSelectedRange", {
          start: this.dpStartDate,
          end: moment(this.dpEndDate)
            .endOf("day")
            .toDate()
        });

        chartModeChanged =
          (currentTotalDays > this.globalSettings.averageModeDaysLimit &&
            this.totalDays <= this.globalSettings.averageModeDaysLimit) ||
          (this.totalDays > this.globalSettings.averageModeDaysLimit &&
            currentTotalDays <= this.globalSettings.averageModeDaysLimit);
      }

      if (!changed && !chartModeChanged) {
        changed = null;
        now = null;
        return;
      }

      let beyondFocusStartDate = moment(this.focusStartDate).isAfter(
        this.dpStartDate
      );
      let overFocusEndDate = moment(this.focusEndDate).isBefore(this.dpEndDate);

      let reducedToOneYear =
        moment(this.focusEndDate).diff(this.focusStartDate, "days") >
          this.globalSettings.averageModeDaysLimit &&
        this.totalDays <= this.globalSettings.averageModeDaysLimit;

      if (this.totalDays <= this.globalSettings.averageModeDaysLimit) {
        if (reducedToOneYear) {
          if (startDateMovedUpper) {
            this.focusStartDate = moment(this.focusEndDate)
              .subtract(this.globalSettings.averageModeDaysLimit - 1, "days")
              .toDate();
          } else if (endDateMovedLower) {
            this.focusEndDate = moment(this.focusStartDate)
              .add(this.globalSettings.averageModeDaysLimit - 1, "days")
              .toDate();
          }
        } else if (beyondFocusStartDate) {
          this.focusEndDate = this.dpEndDate;
          this.focusStartDate = moment(this.dpEndDate)
            .subtract(1, "years")
            .toDate();

          if (this.focusStartDate < this.allowedMinDate) {
            this.focusStartDate = this.allowedMinDate;
            this.focusEndDate = moment(this.focusStartDate)
              .add(1, "years")
              .toDate();
            if (now.isBefore(this.focusEndDate)) {
              this.focusEndDate = now.endOf("day").toDate();
            }
          }
        } else if (overFocusEndDate) {
          this.focusStartDate = this.dpStartDate;
          this.focusEndDate = moment(this.dpStartDate)
            .add(1, "years")
            .toDate();

          if (now.endOf("day").isBefore(this.focusEndDate)) {
            this.focusEndDate = now.endOf("day").toDate();
            this.focusStartDate = moment(this.focusEndDate)
              .subtract(1, "years")
              .toDate();

            if (this.focusStartDate < this.allowedMinDate) {
              this.focusStartDate = this.allowedMinDate;
            }
          }
        }
      } else {
        this.focusStartDate = this.dpStartDate;
        this.focusEndDate = this.dpEndDate;
      }

      if (beyondFocusStartDate || overFocusEndDate || chartModeChanged) {
        this.draw();
      }

      // Must not set extremes if this method was called from the navigator.
      if (!calledFromNavigator) {
        setTimeout(() => {
          this.highstock.xAxis[0].setExtremes(
            this.dpStartDate.getTime(),
            this.dpEndDate.getTime(),
            true,
            false,
            {
              trigger: "controller",
              calledFromInvalidate: true
            }
          );
        }, 20);
      }

      EventBus.$emit("rangeChanged");
    },

    dpStartDateChanged(e) {
      let value = moment(e.target.value);
      if (
        !value.isValid() ||
        value.isBefore(this.dpStartMinDate) ||
        value.isAfter(this.dpStartMaxDate)
      ) {
        this.dpStartDateFormat = moment(this.dpStartDate).format("YYYY-MM-DD");
        return;
      }

      this.invalidateIfRangeChanged(
        moment(this.dpStartDateFormat)
          .startOf("day")
          .toDate(),
        this.dpEndDate
      );
    },

    dpEndDateChanged(e) {
      let value = moment(e.target.value);
      if (
        !value.isValid() ||
        value.isBefore(this.dpEndMinDate) ||
        value.isAfter(this.dpEndMaxDate)
      ) {
        this.dpEndDateFormat = moment(this.dpEndDate).format("YYYY-MM-DD");
        return;
      }

      this.invalidateIfRangeChanged(
        this.dpStartDate,
        moment(this.dpEndDateFormat)
          .startOf("day")
          .toDate()
      );
    },

    setNamedSelectedRange() {
      if (this.isNamedDateRange(1, "day")) {
        this.namedRange = this.namedRanges.OneDay;
        return;
      }

      if (this.isNamedDateRange(1, "weeks")) {
        this.namedRange = this.namedRanges.OneWeek;
        return;
      }

      if (this.isNamedDateRange(2, "weeks")) {
        this.namedRange = this.namedRanges.TwoWeeks;
        return;
      }

      if (this.isNamedDateRange(1, "months")) {
        this.namedRange = this.namedRanges.OneMonth;
        return;
      }

      if (this.isNamedDateRange(3, "months", true)) {
        this.namedRange = this.namedRanges.LastThreeMonths;
        return;
      }

      if (this.isNamedDateRange(3, "months")) {
        this.namedRange = this.namedRanges.ThreeMonths;
        return;
      }

      if (
        utils.dateEquals(this.allowedMinDate, this.dpStartDate) &&
        utils.dateEquals(this.allowedMaxDate, this.dpEndDate)
      ) {
        this.namedRange = this.namedRanges.FullRange;
        return;
      }

      this.namedRange = this.namedRanges.Custom;
    },

    isNamedDateRange(amout, unit, fromNow) {
      return utils.dateEquals(
        (!fromNow ? moment(this.dpEndDate) : moment().endOf("day"))
          .subtract(amout, unit)
          .add(1, "days")
          .toDate(),
        this.dpStartDate
      );
    },

    setRange(amount, unit, fromNow, namedRange) {
      let now = moment().endOf("day");
      let endDate = (fromNow
        ? now
        : moment(this.dpEndDate).endOf("day")
      ).toDate();
      if (now.isBefore(endDate)) {
        endDate = now.toDate();
      }

      let startDate = moment(endDate)
        .startOf("day")
        .subtract(amount, unit)
        .add(1, "days")
        .toDate();

      if (moment(startDate).isBefore(this.allowedMinDate)) {
        startDate = this.allowedMinDate;
        endDate = moment(startDate)
          .add(amount, unit)
          .toDate();

        if (now.isBefore(endDate)) {
          endDate = now.toDate();
        }
      }

      this.namedRange = namedRange;

      this.invalidateIfRangeChanged(startDate, endDate);
    },

    setFullRange() {
      this.namedRange = this.namedRanges.FullRange;
      this.invalidateIfRangeChanged(this.allowedMinDate, this.allowedMaxDate);
    },

    stepBackward() {
      if (!this.canStepBackward) {
        return;
      }

      let diff = this.dpEndDate - this.dpStartDate;
      let startDate = moment(this.dpStartDate)
        .subtract(3, "months")
        .toDate();
      if (startDate < this.allowedMinDate) {
        startDate = this.allowedMinDate;
      }

      this.invalidateIfRangeChanged(
        startDate,
        new Date(startDate.getTime() + diff)
      );
    },

    stepForward() {
      if (!this.canStepForward) {
        return;
      }

      let diff = this.dpEndDate - this.dpStartDate;
      let endDate = moment(this.dpEndDate)
        .add(3, "months")
        .toDate();

      let today = moment().endOf("day");
      if (today.isBefore(endDate)) {
        endDate = today.toDate();
      }

      this.invalidateIfRangeChanged(
        new Date(endDate.getTime() - diff),
        endDate
      );
    },

    groupMinMax(accumulator, item, groupByMonth, valueProp, part, allParts) {
      let skip = 0;
      if (part && allParts) {
        if (groupByMonth) {
          // 30 days
          skip = part && allParts ? (2592000000 / allParts) * part : 0;
        } else {
          // one day
          skip = part && allParts ? (86400000 / allParts) * part : 0;
        }
      }

      let groupKey = new Date(
        item.date.getFullYear(),
        item.date.getMonth(),
        groupByMonth ? 1 : item.date.getDate()
      );

      let arrayItem = accumulator.find(x => utils.dateEquals(x.date, groupKey));
      if (!arrayItem) {
        arrayItem = {
          date: new Date(groupKey.getTime() + skip),
          count: 0,
          sum: 0,
          min: null,
          max: null
        };

        accumulator.push(arrayItem);
      }

      arrayItem.count++;
      arrayItem.sum += item[valueProp];

      if (!arrayItem.min || arrayItem.min[valueProp] > item[valueProp]) {
        arrayItem.min = item;
      }

      if (!arrayItem.max || arrayItem.max[valueProp] < item[valueProp]) {
        arrayItem.max = item;
      }

      return accumulator;
    },

    draw() {
      this.destroy();
      setTimeout(() => {
        this.highstock = new highcharts.StockChart(this.highstockOptions);
      }, 12);
    },

    destroy() {
      if (this.highstock) {
        this.highstock.destroy();
        this.highstock = null;
      }
    },

    showDialog(name, raiseEvent) {
      this.$refs[name].show();
      if (raiseEvent) {
        EventBus.$emit("open-" + name);
      }
    },

    closeDialog(name, raisePrintViewEvent) {
      this.$refs[name].hide();
      if (raisePrintViewEvent) {
        EventBus.$emit("setAgpPrintView", false);
      }
    },

    updateGlobalSettings(updatedSettings) {
      let difference = {};

      let updated = false;
      for (let propertyName in updatedSettings) {
        if (
          this.globalSettings[propertyName] != updatedSettings[propertyName]
        ) {
          difference[propertyName] = true;
          updated = true;
        }
      }

      this.$store.commit("setGlobalSettings", updatedSettings);

      if (updated) {
        if (difference.currentLanguage) {
          this.draw();
        }

        EventBus.$emit("globalSettingsUpdated", {
          updatedSettings: updatedSettings,
          difference: difference
        });
      }

      difference = null;
      updated = null;
    },

    setPrintSection(property, value) {
      this.printOptions[property] = value;
      this.$store.commit("setPrintOptions", {
        key: property,
        value: value
      });
    },

    print() {
      let html = '<div class="container-fluid">';
      if (this.globalSettings.printOptions.charts) {
        let elements = document.getElementsByClassName(
          "print-content-diagrams"
        );
        for (let i = 0; i < elements.length; i++) {
          html += elements[i].outerHTML;
        }

        EventBus.$emit("setAgpPrintView", true);
      }

      if (this.isDcmView && this.globalSettings.printOptions.diary) {
        html += document.getElementsByClassName("print-content-diary")[0]
          .outerHTML;
      }

      html += "</div>";

      utils.openNewPrintWindow({
        title: "",
        body: html,
        includeCss: true,
        closeAfterPrint: true
      });
    },

    changeScaleType(targetType) {
      if (targetType == this.timeScaleType) {
        return;
      }

      this.$store.commit("setGlobalSettingsProperty", {
        key: "timeScaleType",
        value: targetType
      });

      this.draw();
    },

    onSettingsUpdated(settings) {
      this.updateGlobalSettings(settings);
      this.closeDialog("global-settings-modal");
    },

    disableControl() {
      Object.keys(this.invalidatedComponents).forEach(key => {
        this.invalidatedComponents[key] = false;
      });
    },

    chartInvalidated(e) {
      this.invalidatedComponents[e.name] = true;
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },
    timeScaleType() {
      return this.globalSettings.timeScaleType;
    },
    controlDisabled() {
      return (
        !this.invalidatedComponents.agp ||
        !this.invalidatedComponents.dgp ||
        !this.invalidatedComponents.insulinStatistics ||
        !this.invalidatedComponents.targetRange ||
        (this.isDcmView && !this.invalidatedComponents.diary)
      );
    },
    highchartsDateFormat() {
      return this.$i18n.t(
        "highchartsDateFormat",
        this.globalSettings.currentLanguage
      );
    },
    pleaseWaitText() {
      return this.$i18n.t(
        "pleaseWaitText",
        this.globalSettings.currentLanguage
      );
    },
    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },
    allowedMinDate() {
      return this.$store.state.mainModule.range.allowedMinDate;
    },
    allowedMaxDate() {
      return this.$store.state.mainModule.range.allowedMaxDate;
    },
    namedRanges() {
      return NamedRanges;
    },
    totalDays() {
      return this.$store.getters.totalDays;
    },
    dpStartMinDate() {
      if (!this.allowedMinDate) {
        return;
      }

      return moment(this.allowedMinDate).format("YYYY-MM-DD");
    },
    dpStartMaxDate() {
      if (!this.allowedMaxDate) {
        return;
      }

      let maxDate = null;
      if (!this.dpEndDate) {
        maxDate = this.allowedMaxDate;
      } else {
        maxDate = this.dpEndDate;
      }

      return moment(maxDate).format("YYYY-MM-DD");
    },
    dpEndMinDate() {
      if (!this.allowedMinDate) {
        return;
      }

      let minDate = null;
      if (!this.dpStartDate) {
        minDate = this.allowedMinDate;
      } else {
        minDate = this.dpStartDate;
      }

      return moment(minDate).format("YYYY-MM-DD");
    },
    dpEndMaxDate() {
      return moment().format("YYYY-MM-DD");
    },
    lang() {
      return this.$i18n.t("settingsBar", this.globalSettings.currentLanguage);
    },
    yesText() {
      return this.$i18n.t("yes", this.globalSettings.currentLanguage);
    },
    noText() {
      return this.$i18n.t("no", this.globalSettings.currentLanguage);
    },
    okText() {
      return this.$i18n.t("ok", this.globalSettings.currentLanguage);
    },
    daysText() {
      return this.$i18n.t("days", this.globalSettings.currentLanguage);
    },
    datepickerFormat() {
      return this.$i18n.t(
        "datepickerFormat",
        this.globalSettings.currentLanguage
      );
    },
    isMonthlyTimeScaleView() {
      return this.totalDays > this.globalSettings.averageModeDaysLimit;
    },
    highstockOptions() {
      let _this = this;

      return {
        chart: {
          renderTo: "dashboard-filter-container",
          height: 80,
          spacing: [0, 0, 0, 0],
          boost: {
            enabled: true,
            useGPUTranslations: true
          }
        },
        rangeSelector: {
          enabled: false
        },
        tooltip: {
          enabled: false
        },
        scrollbar: {
          enabled: false
        },
        title: {
          text: ""
        },
        exporting: {
          enabled: false
        },
        credits: {
          enabled: false
        },
        navigator: this.highstockNavigator,
        plotOptions: this.highstockPlotOptions,
        series: this.highstockSeries,
        yAxis: {
          title: null,
          lineWidth: 0,
          gridLineWidth: 0,
          labels: {
            enabled: false
          }
        },
        xAxis: {
          type: "datetime",
          labels: {
            enabled: false
          },
          lineWidth: 0,
          tickLength: 0,
          pointPadding: 0,
          groupPadding: 0,
          min: this.dpStartDate.getTime(),
          max: this.dpEndDate.getTime(),
          events: {
            setExtremes(e) {
              if (e.calledFromInvalidate) {
                return;
              }

              let start = moment(e.min)
                .startOf("day")
                .valueOf();
              let end = moment(e.max)
                .startOf("day")
                .valueOf();

              if (
                start != _this.dpStartDate.getTime() &&
                end != _this.dpEndDate.getTime()
              ) {
                let diffPrevMs = _this.dpEndDate - _this.dpStartDate;
                let newDiff = end - start;

                if (diffPrevMs != newDiff) {
                  let subtractValue;
                  let subtractUnit;

                  switch (_this.namedRange) {
                    case _this.namedRanges.OneDay:
                      subtractValue = 1;
                      subtractUnit = "days";
                      break;
                    case _this.namedRanges.OneWeek:
                      subtractValue = 1;
                      subtractUnit = "weeks";
                      break;
                    case _this.namedRanges.TwoWeeks:
                      subtractValue = 2;
                      subtractUnit = "weeks";
                      break;
                    case _this.namedRanges.OneMonth:
                      subtractValue = 1;
                      subtractUnit = "months";
                      break;
                    case _this.namedRanges.ThreeMonths:
                    case _this.namedRanges.LastThreeMonths:
                      subtractValue = 3;
                      subtractUnit = "months";
                      break;
                    default:
                      subtractValue = newDiff; // + 86400000;
                      subtractUnit = "milliseconds";
                      break;
                  }

                  start = moment(end)
                    .subtract(subtractValue, subtractUnit)
                    .add(1, "days")
                    .valueOf();
                }
              }

              setTimeout(() => {
                _this.invalidateIfRangeChanged(
                  new Date(start),
                  new Date(end),
                  true
                );
              });
            }
          },
          range: moment(this.dpEndDate).diff(this.dpStartDate, "milliseconds")
        }
      };
    },
    highstockNavigator() {
      let navigator = {
        enabled: true,
        maskInside: false,
        maskFill: "rgba(73,115,147,0.4)",
        height: 80,
        xAxis: {
          type: "datetime",
          ordinal: false,
          min: this.focusStartDate.getTime(),
          max: this.focusEndDate.getTime(),
          labels: {
            align: "center",
            format: this.highchartsDateFormat,
            style: {
              color: "#333",
              cursor: "pointer",
              fontSize: "9px",
              fontWeight: "bold"
            }
          }
        },
        yAxis: {
          gridLineColor: "#777",
          gridLineWidth: 1,
          gridZIndex: 4,
          min: -9
        }
      };

      switch (this.timeScaleType) {
        case "default":
        case "insulin":
          navigator.series = {
            type: "column",
            stacking: "normal"
          };
          break;

        case "glucose":
          navigator.series = {
            type: "line",
            color: "transparent",
            marker: {
              enabled: true
            }
          };
          break;
      }

      return navigator;
    },
    highstockPlotOptions() {
      let plotOptions = {
        series: {
          showInNavigator: true
        }
      };

      switch (this.timeScaleType) {
        case "default":
          return Object.assign(plotOptions, {
            column: {
              animation: 0,
              pointWidth: !this.isMonthlyTimeScaleView ? 4 : 25
            }
          });

        case "glucose":
          return Object.assign(plotOptions, {
            line: {
              animation: 0
            }
          });

        case "insulin":
          return Object.assign(plotOptions, {
            column: {
              animation: 0,
              pointWidth: !this.isMonthlyTimeScaleView ? 1 : 8
            }
          });
      }
    },
    highstockSeries() {
      switch (this.timeScaleType) {
        case "default":
          return this.seriesScaleDefault;

        case "glucose":
          return this.seriesScaleGlucose;

        case "insulin":
          return this.seriesScaleInsulin;
      }

      return {};
    },
    seriesScaleDefault() {
      let values = [];
      if (this.isMonthlyTimeScaleView) {
        values = this.values
          .reduce((accumulator, item) => {
            var date = new Date(item.date);
            var groupKey = new Date(date.getFullYear(), date.getMonth(), 1);
            var arrayItem = accumulator.find(x =>
              utils.dateEquals(x.date, groupKey)
            );
            if (!arrayItem) {
              arrayItem = {
                date: groupKey,
                bloodGlocoseCount: 0,
                insulinCount: 0,
                count: 0
              };
              accumulator.push(arrayItem);
            }

            arrayItem.bloodGlocoseCount += item.bloodGlocoseCount;
            arrayItem.insulinCount += item.insulinCount;
            arrayItem.count++;

            return accumulator;
          }, [])
          .map(x => {
            return {
              date: x.date.getTime(),
              bloodGlocoseCount: x.bloodGlocoseCount / x.count,
              insulinCount: x.insulinCount / x.count
            };
          });
      } else {
        values = this.values;
      }

      return [
        {
          type: "column",
          name: "BG",
          color: "#6C96D5",
          data: values
            .filter(x => x.bloodGlocoseCount)
            .map(x => [x.date, x.bloodGlocoseCount])
        },
        {
          type: "column",
          name: "INS",
          color: "#F4832F",
          data: values
            .filter(x => x.insulinCount)
            .map(x => [x.date, -x.insulinCount])
        }
      ];
    },
    seriesScaleGlucose() {
      let tmpData = this.glucoseValues
        .filter(x =>
          betweenDateRange(x.date, this.focusStartDate, this.focusEndDate)
        )
        .reduce(
          (accumulator, item) =>
            this.groupMinMax(
              accumulator,
              item,
              this.isMonthlyTimeScaleView,
              "value"
            ),
          []
        );

      let serieVeryLow = {
        data: [],
        name: "serieVeryLow",
        type: "line",
        marker: {
          enabled: false,
          symbol: "circle",
          radius: 1.5,
          fillColor: smbgColors.slow
        }
      };

      let serieLow = {
        data: [],
        name: "serieLow",
        type: "line",
        marker: {
          enabled: false,
          symbol: "circle",
          radius: 1.5,
          fillColor: smbgColors.low
        }
      };

      let serieAvg = {
        data: [],
        name: "serieAvg",
        type: "line",
        marker: {
          enabled: false,
          symbol: "circle",
          radius: 1.5,
          fillColor: smbgColors.target
        }
      };

      let serieHigh = {
        data: [],
        name: "serieHigh",
        type: "line",
        marker: {
          enabled: false,
          symbol: "circle",
          radius: 1.5,
          fillColor: smbgColors.high
        }
      };

      let serieVeryHigh = {
        data: [],
        name: "serieVeryHigh",
        type: "line",
        marker: {
          enabled: false,
          symbol: "circle",
          radius: 1.5,
          fillColor: smbgColors.shigh
        }
      };

      tmpData.forEach(item => {
        let time = item.date.getTime();

        serieAvg.data.push([time, item.sum / item.count]);

        if (item.count > 1) {
          if (item.min) {
            if (item.min.agpIsVeryLow) {
              serieVeryLow.data.push([time, item.min.value]);
            } else if (item.min.agpIsLow) {
              serieLow.data.push([time, item.min.value]);
            } else if (item.min.agpIsVeryHigh) {
              serieVeryHigh.data.push([time, item.min.value]);
            } else if (item.min.agpIsHigh) {
              serieHigh.data.push([time, item.min.value]);
            } else {
              serieAvg.data.push([time, item.min.value]);
            }
          }

          if (item.max) {
            if (item.max.agpIsVeryLow) {
              serieVeryLow.data.push([time, item.max.value]);
            } else if (item.max.agpIsLow) {
              serieLow.data.push([time, item.max.value]);
            } else if (item.max.agpIsVeryHigh) {
              serieVeryHigh.data.push([time, item.max.value]);
            } else if (item.max.agpIsHigh) {
              serieHigh.data.push([time, item.max.value]);
            } else {
              serieAvg.data.push([time, item.max.value]);
            }
          }
        }
      });

      return [serieVeryLow, serieLow, serieAvg, serieHigh, serieVeryHigh];
    },
    seriesScaleInsulin() {
      return this.insulinTypes.map((insulinType, i) => {
        return {
          name: insulinType.type,
          type: "column",
          color: insulinType.color,
          data: this.insulinValues
            .filter(
              x =>
                x.type == insulinType.type &&
                betweenDateRange(x.date, this.focusStartDate, this.focusEndDate)
            )
            .reduce(
              (accumulator, item) =>
                this.groupMinMax(
                  accumulator,
                  item,
                  this.isMonthlyTimeScaleView,
                  "unit",
                  i,
                  this.insulinTypes.length
                ),
              []
            )
            .map(item => [item.date.getTime(), item.max.unit])
        };
      });
    },
    settings() {
      return Object.assign({}, this.globalSettings);
    },
    printOptionsSelected() {
      for (let propertyName in this.printOptions) {
        if (this.printOptions[propertyName]) {
          return true;
        }
      }
    },
    canStepBackward() {
      return this.dpStartDate > this.allowedMinDate;
    },
    canStepForward() {
      return moment()
        .endOf("day")
        .isAfter(this.dpEndDate);
    }
  },
  filters: {
    displayDate(date, format) {
      return moment(date).format(format);
    }
  },
  mounted() {
    // TODO: show modal event:
    this.initialize();
    this.draw();

    this.$nextTick(() => {
      $(".collapse-navigation").on("click", e => {
        $(".navbar-collapse-buttons").removeClass("show");
      });
    });

    EventBus.$on("settings-updated", this.onSettingsUpdated);
    EventBus.$on("chart-invalidate-finished", this.chartInvalidated);
  },
  beforeDestroy() {
    this.destroy();

    EventBus.$off("settings-updated", this.onSettingsUpdated);
    EventBus.$off("chart-invalidate-finished", this.chartInvalidated);
  },
  props: ["values", "glucoseValues", "insulinValues", "insulinTypes"]
};
</script>
