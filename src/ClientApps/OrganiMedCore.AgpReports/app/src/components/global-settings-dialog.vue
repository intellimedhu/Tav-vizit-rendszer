<template>
    <div>
        <div class="modal-body settings-modal-body">
            <div class="row settings-row">
                <div class="col-md-3 col-lg-2 settings-nav">
                    <div class="nav flex-column nav-pills" id="v-pills-tab" role="tablist" aria-orientation="vertical">
                        <a class="nav-link settings-nav-link"
                            :class="{'active': settings.activeSettingsTab == 'v-pills-view-model-tab' }"
                            id="v-pills-view-model-tab"
                            data-toggle="pill"
                            href
                            @click="setActiveTab('v-pills-view-model-tab')"
                            role="tab"
                            aria-controls="v-pills-glucose">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationViewMode'] }}
                        </a>

                        <a class="nav-link settings-nav-link"
                            :class="{'active': settings.activeSettingsTab == 'v-pills-language-tab' }"
                            id="v-pills-language-tab"
                            data-toggle="pill"
                            href
                            @click="setActiveTab('v-pills-language-tab')"
                            role="tab"
                            aria-controls="v-pills-language"
                            aria-selected="false">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationLanguage'] }}
                        </a>

                        <a class="nav-link settings-nav-link"
                            :class="{'active': settings.activeSettingsTab == 'v-pills-unit-tab' }"
                            id="v-pills-unit-tab"
                            data-toggle="pill"
                            href
                            @click="setActiveTab('v-pills-unit-tab')"
                            role="tab"
                            aria-controls="v-pills-unit"
                            aria-selected="false">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationUnit'] }}
                        </a>

                        <a class="nav-link settings-nav-link"
                           v-if="settings.viewMode == 'DCMView'"
                           :class="{'active': settings.activeSettingsTab == 'v-pills-glucose-tab' }"
                           id="v-pills-glucose-tab"
                           data-toggle="pill"
                           href
                           @click="setActiveTab('v-pills-glucose-tab')"
                           role="tab"
                           aria-controls="v-pills-glucose">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationBloodGlucose'] }}
                        </a>
                        
                        <a class="nav-link settings-nav-link"
                           v-if="settings.viewMode == 'DCMView'"
                           :class="{'active': settings.activeSettingsTab == 'v-pills-insulin-tab' }"
                           id="v-pills-insulin-tab"
                           data-toggle="pill"
                           href
                           @click="setActiveTab('v-pills-insulin-tab')"
                           role="tab"
                           aria-controls="v-pills-insulin">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationInsulin'] }}
                        </a>

                        <a class="nav-link settings-nav-link"
                           v-if="settings.viewMode == 'DCMView'"
                           :class="{'active': settings.activeSettingsTab == 'v-pills-diary-tab' }"
                           id="v-pills-diary-tab"
                           data-toggle="pill"
                           href
                           @click="setActiveTab('v-pills-diary-tab')"
                           role="tab"
                           aria-controls="v-pills-diary"
                           aria-selected="false">
                            <i class="icon-chevron-right nav-icon"></i>
                            {{ lang['navigationDiary'] }}
                        </a>
                    </div>
                </div>
                
                <div class="col-md-9 col-lg-10 pl-sm-3 settings-body" id="global-settings-settings-body">
                    <div class="tab-content" :class="{'was-validated': submitted}" id="v-pills-tabContent">
                        <div class="tab-pane fade" id="v-pills-view-model-tab" role="tabpanel" aria-labelledby="v-pills-view-model-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-view-model-tab' }">
                            <div class="form-group row">
                                <div class="col-sm-6 offset-sm-3 text-center">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.viewMode == 'OfficialAGP',
                                                    'btn-outline-primary': settings.viewMode != 'OfficialAGP'
                                                }"
                                                @click="setViewMode('OfficialAGP')">
                                            {{ lang.tabViewModel['AGP'] }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.viewMode == 'DCMView',
                                                    'btn-outline-primary': settings.viewMode != 'DCMView'
                                                }"
                                                @click="setViewMode('DCMView')">
                                            {{ lang.tabViewModel['DCM'] }}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="tab-pane fade" id="v-pills-language" role="tabpanel" aria-labelledby="v-pills-language-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-language-tab' }">
                            <div class="form-group row">
                                <div class="col-sm-6 offset-sm-3 text-center">
                                    <div class="btn-group" role="group">
                                        <button class="btn btn-sm btn-locale btn-locale-hu"
                                                type="button"
                                                :class="{'btn-primary': settings.currentLanguage == 'hu', 'btn-outline-primary': settings.currentLanguage != 'hu'}"
                                                @click="settings.currentLanguage = 'hu'"
                                                title="magyar">
                                            <i class="flag"></i>
                                            Magyar
                                        </button>

                                        <button class="btn btn-sm btn-locale btn-locale-en"
                                                type="button"
                                                :class="{'btn-primary': settings.currentLanguage == 'en', 'btn-outline-primary': settings.currentLanguage != 'en'}"
                                                @click="settings.currentLanguage = 'en'"
                                                title="english">
                                            <i class="flag"></i>
                                            English
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="tab-pane fade" id="v-pills-unit" role="tabpanel" aria-labelledby="v-pills-unit-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-unit-tab' }">
                            <div class="form-group row">
                                <div class="col-sm-6 offset-sm-3 text-center">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.unit == 'mmol/l',
                                                    'btn-outline-primary': settings.unit != 'mmol/l'
                                                }"
                                                @click="settings.unit = 'mmol/l'">
                                            mmol/l
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.unit == 'mg/dl',
                                                    'btn-outline-primary': settings.unit != 'mg/dl'
                                                }"
                                                @click="settings.unit = 'mg/dl'">
                                            mg/dl
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="tab-pane fade" id="v-pills-glucose" role="tabpanel" aria-labelledby="v-pills-glucose-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-glucose-tab' }">
                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabBloodGlucose['midlineTypeLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.midlineType == 'avg',
                                                    'btn-outline-primary': settings.midlineType != 'avg'
                                                }"
                                                @click="settings.midlineType = 'avg'">
                                            {{ lang.tabBloodGlucose['midlineTypeAvg'] }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.midlineType == 'median',
                                                    'btn-outline-primary': settings.midlineType != 'median'
                                                }"
                                                @click="settings.midlineType = 'median'">
                                            {{ lang.tabBloodGlucose['midlineTypeMedian'] }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabBloodGlucose['displayNonMeasurableValuesLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.displayNonMeasurableValues,
                                                    'btn-outline-primary': !settings.displayNonMeasurableValues
                                                }"
                                                @click="settings.displayNonMeasurableValues = true">
                                            {{ yesText }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': !settings.displayNonMeasurableValues,
                                                    'btn-outline-primary': settings.displayNonMeasurableValues
                                                }"
                                                @click="settings.displayNonMeasurableValues = false">
                                            {{ noText }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabBloodGlucose['averageLinesLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.displayAverageLines,
                                                    'btn-outline-primary': !settings.displayAverageLines
                                                }"
                                                @click="settings.displayAverageLines = true">
                                            {{ yesText }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': !settings.displayAverageLines,
                                                    'btn-outline-primary': settings.displayAverageLines
                                                }"
                                                @click="settings.displayAverageLines = false">
                                            {{ noText }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabBloodGlucose['roundedLineChartsLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.curveType == 'function',
                                                    'btn-outline-primary': settings.curveType != 'function'
                                                }"
                                                @click="settings.curveType = 'function'">
                                            {{ yesText }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.curveType == 'none',
                                                    'btn-outline-primary': settings.curveType != 'none'
                                                }"
                                                @click="settings.curveType = 'none'">
                                            {{ noText }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabBloodGlucose['dgpTypeLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.dgpChartType == 'line',
                                                    'btn-outline-primary': settings.dgpChartType != 'line'
                                                }"
                                                @click="settings.dgpChartType = 'line'">
                                            {{ lang.tabBloodGlucose['dgpTypeLine'] }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.dgpChartType == 'scatter',
                                                    'btn-outline-primary': settings.dgpChartType != 'scatter'
                                                }"
                                                @click="settings.dgpChartType = 'scatter'">
                                            {{ lang.tabBloodGlucose['dgpTypeScatter'] }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <!-- <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm" for="minimumMeasurementsOfHbA1c">
                                    {{ lang.tabBloodGlucose['minimumMeasurementsOfHbA1cLabel'] }}
                                </label>

                                <div class="col-sm-3 col-md-2 col-xl-1">
                                        <input type="number" min="50" step="10"
                                            id="minimumMeasurementsOfHbA1c"
                                            class="form-control"
                                            v-model="settings.minimumMeasurementsOfHbA1c" />
                                </div>
                            </div> -->
                        </div>

                        <div class="tab-pane tab-panel-insulin fade" id="v-pills-insulin" role="tabpanel" aria-labelledby="v-pills-insulin-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-insulin-tab' }">
                            <div class="form-group row">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ lang.tabInsulin['displayInsulinsInAgpLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <div class="btn-group" role="group">
                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': settings.displayInsulinInAGP,
                                                    'btn-outline-primary': !settings.displayInsulinInAGP
                                                }"
                                                @click="settings.displayInsulinInAGP = true">
                                            {{ yesText }}
                                        </button>

                                        <button type="button"
                                                class="btn btn-sm"
                                                :class="{
                                                    'btn-primary': !settings.displayInsulinInAGP,
                                                    'btn-outline-primary': settings.displayInsulinInAGP
                                                }"
                                                @click="settings.displayInsulinInAGP = false">
                                            {{ noText }}
                                        </button>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row" v-for="insulinType in insulinSettings" v-bind:key="insulinType.type">
                                <label class="col-sm-6 col-form-label col-form-label-sm">
                                    {{ insulinType.type }} {{ lang.tabInsulin['typeColorLabel'] }}
                                </label>

                                <div class="col-sm-6">
                                    <input type="color" v-model="insulinType.color" />
                                </div>
                            </div>
                        </div>

                        <div class="tab-pane fade" id="v-pills-diary" role="tabpanel" aria-labelledby="v-pills-diary-tab"
                             :class="{ 'show active': settings.activeSettingsTab == 'v-pills-diary-tab' }">
                            <div class="row">
                                <div class="col">
                                    <h5>
                                        {{ lang.tabDiary['typeMealtimeTitle'] }}
                                    </h5>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-4 col-form-label col-form-label-sm" for="settingsPreMealTime">
                                    {{ lang.tabDiary['preMealtimeLabel'] }}
                                </label>

                                <div class="col-sm-3 col-lg-2">
                                    <div class="input-group input-group-sm">
                                        <input type="number" 
                                               required="required"
                                               :min="mealtimeMin"
                                               :max="mealtimeMax"
                                               :step="mealtimeStep"
                                               id="settingsPreMealTime"
                                               class="form-control form-control-sm"
                                               v-model="settings.preMealTimeInMinutes" />
                                        <div class="input-group-append">
                                            <span class="input-group-text" id="basic-addon2">
                                                {{ lang.tabDiary['minutes'] }}
                                            </span>
                                        </div>
                                        <div v-if="validationErrors.preMealTimeInMinutes" class="invalid-feedback">{{numberRequiredBetween}}</div>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group row">
                                <label class="col-sm-4 col-form-label col-form-label-sm" for="settingsPostMealTime">
                                    {{ lang.tabDiary['postMealtimeLabel'] }}
                                </label>

                                <div class="col-sm-3 col-lg-2">
                                    <div class="input-group input-group-sm">
                                        <input type="number"
                                               required="required"
                                               :min="mealtimeMin"
                                               :max="mealtimeMax"
                                               :step="mealtimeStep"
                                               id="settingsPostMealTime"
                                               class="form-control form-control-sm"
                                               v-model="settings.postMealTimeInMinutes" />
                                        <div class="input-group-append">
                                            <span class="input-group-text" id="basic-addon2">
                                                {{ lang.tabDiary['minutes'] }}
                                            </span>
                                        </div>
                                        <div v-if="validationErrors.postMealTimeInMinutes" class="invalid-feedback">{{numberRequiredBetween}}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="modal-footer">
            <button type="button" class="btn btn-sm btn-primary" @click="saveSettings()" :disabled="submitted && hasValidationErrors">
                <i class="icon-ok"></i> {{ okText }}
            </button>
        </div>
    </div>
</template>

<script>
import { EventBus } from "../services/event-bus";
import { InsulinTypes } from "../services/utils";

export default {
  data() {
    return {
      settings: {
        activeSettingsTab: null,
        averageModeDaysLimit: null,
        colorBasal: null,
        colorBolus: null,
        colorMix: null,
        currentLanguage: null,
        curveType: null,
        dgpChartType: null,
        dgpDialogMode: null,
        diaryType: null,
        displayAverageLines: null,
        displayInsulinInAGP: null,
        displayNonMeasurableValues: null,
        glucoseStatisticsMode: null,
        midlineType: null,
        //minimumMeasurementsOfHbA1c: null,
        postMealTimeInMinutes: null,
        preMealTimeInMinutes: null,
        printOptions: {
          charts: null,
          diary: null
        },
        timeScaleType: null,
        unit: null,
        viewMode: null
      },
      insulinSettings: [],
      mealtimeStep: 5,
      mealtimeMin: 5,
      mealtimeMax: 150,
      submitted: false
    };
  },
  methods: {
    setActiveTab(tab) {
      this.settings.activeSettingsTab = tab;
    },

    setViewMode(mode) {
      this.settings.viewMode = mode;

      if (mode == "OfficialAGP") {
        this.settings.glucoseStatisticsMode = "agp";
        this.settings.midlineType = "median";
        this.settings.displayNonMeasurableValues = true;
        this.settings.displayAverageLines = true;
        this.settings.curveType = "none";
        this.settings.dgpChartType = "scatter";
        this.settings.displayInsulinInAGP = false;
        this.settings.timeScaleType = "glucose";
      } else {
        this.settings.glucoseStatisticsMode = "custom";
        this.settings.midlineType = "avg";
        this.settings.displayAverageLines = false;
        this.settings.curveType = "function";
        this.settings.dgpChartType = "line";
        this.settings.displayInsulinInAGP = true;
      }
    },

    saveSettings() {
      this.submitted = true;
      if (this.hasValidationErrors) {
        return;
      }

      this.insulinSettings.forEach(insulinType => {
        switch (insulinType.type) {
          case InsulinTypes.basal:
            this.settings.colorBasal = insulinType.color;
            break;
          case InsulinTypes.bolus:
            this.settings.colorBolus = insulinType.color;
            break;
          case InsulinTypes.mix:
            this.settings.colorMix = insulinType.color;
            break;
        }
      });

      EventBus.$emit("settings-updated", this.settings);
    },

    onOpening() {
      this.submitted = false;
      Object.assign(this.settings, this.globalSettings);
      this.insulinSettings = this.insulinTypes.map(x => Object.assign({}, x));

      this.settings.activeSettingsTab =
        this.globalSettings.activeSettingsTab || "v-pills-view-model-tab";
    }
  },
  computed: {
    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },

    lang() {
      return this.$i18n.t(
        "globalSettingsDialog",
        this.globalSettings.currentLanguage
      );
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

    hasValidationErrors() {
      return Object.keys(this.validationErrors).length > 0;
    },

    validationErrors() {
      var result = {};
      if (
        !this.settings.postMealTimeInMinutes ||
        this.settings.postMealTimeInMinutes < 5 ||
        this.settings.postMealTimeInMinutes > 150
      ) {
        result["postMealTimeInMinutes"] = 1;
      }

      if (
        !this.settings.preMealTimeInMinutes ||
        this.settings.preMealTimeInMinutes < 5 ||
        this.settings.preMealTimeInMinutes > 150
      ) {
        result["preMealTimeInMinutes"] = 1;
      }

      return result;
    },

    numberRequiredBetween() {
      return this.lang.tabDiary.numberRequiredBetween
        .replace("{0}", this.mealtimeMin)
        .replace("{1}", this.mealtimeMax);
    }
  },
  mounted() {
    EventBus.$on("open-global-settings-modal", this.onOpening);
  },
  beforeDestroy() {
    EventBus.$off("open-global-settings-modal", this.onOpening);
  },
  props: ["insulinTypes"]
};
</script>
