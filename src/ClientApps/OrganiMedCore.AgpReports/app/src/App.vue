<template>
  <div class="container-fluid main-container">
    <div v-if="initialized && globalSettings.unit && originalUnit">
      <settings-bar v-if="rangePickerValues && rangePickerValues.length"
                    :values="rangePickerValues"
                    :glucose-values="bloodGlucoseValues"
                    :insulin-values="insulinValuesInBulk"
                    :insulin-types="insulinTypes" />
   
      <div class="row">
        <div class="col-12">
          <div class="card card-diagram-block">
            <div class="card-body py-lg-2 px-lg-1">
              <span class="badge badge-primary badge-information">
                <span class="icon-info"></span>
                <div>
                  <ul class="list-unstyled">
                    <li>
                      <h4>Oszlop diagram</h4>
                      <ul>
                        <li>
                          A kiválsztott dátumtartományon belüli összes mérések megoszlása az adott céltartományhoz tekintve.
                          A céltartomány a hivatalos <a href="http://www.agpreport.org/agp/agpreports" target="_blank">AGP szerinti</a>.
                        </li>
                      </ul>
                    </li>
                    <li>
                      <h4>Vércukor statisztikák</h4>
                      <ul>
                        <li>
                          <strong>Mérési dátumtartomány: </strong> -tól -ig dátum.
                        </li>
                        <li>
                          <strong>Átlagos napi mérésszám: </strong>A mérési eredmények összege osztva a mérési tartományonban lévő napok számával.
                        </li>
                        <li>
                          <strong>Vércukor átlag: </strong>A mérési eredmények összege osztva a mérések darabszámával.
                        </li>
                        <li>
                          <strong>Glucose Management Indicator (GMI): </strong> A vércukor átlagból számítható; A1c becsült értéke.
                        </li>
                        <li>
                          <strong>Variációs együttható: </strong>: A mérési eredmények egymástól való távolságát írja le, ami ideális esetben egy alacsony szám.
                        </li>
                        <li>
                          <strong>Szórás: </strong> A mérések átlagtól való eltérése, ami ideális esetben egy alacsony szám
                        </li>
                      </ul>
                    </li>

                    <li>
                      <h4>Ambuláns glükózprofil</h4>
                      <ul>
                        <li>A kiválsztott dátumtartományon belüli összes mérések száma egy napra vetítve. (0-24 óra). Tartalmazza az alábbiakat:
                          <ul>
                            <li><strong style="color: gray">Szürke sáv</strong>: céltartomány</li>
                            <li><strong style="color: #99c26b">Zöld vonalak</strong>: Összes mérés alsó és felső 10%-a. (ideális esetben)</li>
                            <li><strong style="color: #50a9d8">Kék vonalak</strong>: A mérések 50%-a. (ideális esetben)</li>
                            <li><strong style="color: #f68b1e">Narancssárga</strong>: középérték vagy átlag</li>
                          </ul>
                        </li>
                      </ul>
                    </li>

                    <li>
                      <h4>Napi glükózprofil</h4>
                      <ul>
                        <li>A kiválsztott dátumtartományon belüli összes mérések naponként</li>
                      </ul>
                    </li>
                  </ul>
                </div>
              </span>
              
              <patient :patient-profile-raw-url="patientProfileRawUrl" />

              <div class="row statistic-row glucose-statistic-row print-content-diagrams" :class="{'mb-lg-4': isDcmView}">
                <glucose-statistics-agp :statistical-data="statisticalData" />
                <target-range-chart :statistical-data="statisticalData" />
              </div>

              <insulin-statistics v-show="filteredInsulinValuesInBulk.length && isDcmView"
                                  :insulin-brands="insulinBrands"
                                  :insulin-types="insulinTypes"
                                  :insulin-values="filteredInsulinValuesByBrands" />

              <agp-highcharts :filtered-blood-glucose-values="filteredBloodGlucoseValues"
                              :filtered-insulin-values-by-brands="filteredInsulinValuesByBrands"
                              :insulin-types="insulinTypes"
                              :original-unit="originalUnit"
                              :daily-charts-v-max="statisticalData.maxValue" />

              <dgp-charts :glucose-values="filteredBloodGlucoseValues"
                          :insulin-values="filteredInsulinValuesInBulk"
                          :insulin-types="insulinTypes"
                          :daily-charts-v-max="statisticalData.maxValue"
                          :original-unit="originalUnit" />
            </div>
          </div>
        </div>
      </div>

      <div class="row print-content-diary" :class="{'diary-print-margin': globalSettings.printOptions.charts}" v-if="isDcmView">
        <div class="col-12">
          <div class="card card-diagram-block">
            <div class="card-header">
              <h5 class="text-center block-title">{{ lang['bloodGlucoseDiaryTitle'] }}</h5>
            </div>
            <div class="card-body py-lg-2 px-lg-1">
              <span class="badge badge-primary badge-information d-print-none">
                <span class="icon-info"></span>
                <div v-if="globalSettings.diaryType == 'mealtime'">
                  A kiválsztott dátumtartományon belüli összes mérések és inzulinok megjelenítése a <u>vércukornapló</u>ban,
                  figyelembe véve az étkezés előtti/utáni céltartományokat és étkezési időpontokat.
                  <br />
                  <br />
                  <ul class="list-unstyled">
                    <li>
                      <strong>Étkezés előtti időszak</strong>: étkezési időszak kezdete <u>-{{ globalSettings.preMealTimeInMinutes }} perc</u>, és étkezési időszak kezdete közötti idő
                    </li>
                    <li>
                      <strong>Étkezés utáni időszak</strong>: étkezési időszak vége, és étkezési időszak vége <u>+{{ globalSettings.postMealTimeInMinutes }} perc</u> közötti idő
                    </li>
                    <li>
                      <br />
                      A táblázat fejléce igazodik ezen időszakokhoz.
                    </li>
                    <li>
                      <br />
                      Mérések szín szerint:
                      <br />
                      <ul class="list-inline">
                        <li class="list-inline-item">
                          <strong class="text-shigh">Nagyon magas</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-high">Magas</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-target">Normál</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-low">Alacsony</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-slow">Nagyon alacsony</strong>
                        </li>
                      </ul>

                      <br />
                      Inzulinok szín szerint:
                      <ul class="list-inline">
                        <li class="list-inline-item" v-for="type in insulinTypes" v-bind:key="type.type">
                          <strong class="badge badge-pill text-white" :style="`background-color:${type.color}`">
                            {{ type.type }}
                          </strong>
                        </li>
                      </ul>
                      <br />
                    </li>
                    <li>
                      Az értékek fölé mozgatva az egeret megjelenik a pontos időpont.
                    </li>
                  </ul>
                </div>
                <div v-if="globalSettings.diaryType == 'hourly'">
                  <p>
                  A kiválsztott dátumtartományon belüli összes mérések és inzulinok megjelenítése <u>vércukornapló</u>ban,
                  figyelembe véve, hogy a páciens milyen jelölést használt a mérés során,
                  valamint az étkezés előtti/utáni céltartományokat. A páciens által az alábbi jelölők használhatók:
                  </p>

                  <ul>
                    <li>Étkezés előtti</li>
                    <li>Étkezés utáni</li>
                    <li>Normál (nincs jelölés)</li>
                    <li>Egyéb</li>
                  </ul>

                  <br>
                  Mérések szín szerint:
                      <br />
                      <ul class="list-inline">
                        <li class="list-inline-item">
                          <strong class="text-shigh">Nagyon magas</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-high">Magas</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-target">Normál</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-low">Alacsony</strong>
                        </li>
                        <li class="list-inline-item">
                          <strong class="text-slow">Nagyon alacsony</strong>
                        </li>
                      </ul>

                      <br />
                      Inzulinok szín szerint:
                      <ul class="list-inline">
                        <li class="list-inline-item" v-for="type in insulinTypes" v-bind:key="type.type">
                          <strong class="badge badge-pill text-white" :style="`background-color:${type.color}`">
                            {{ type.type }}
                          </strong>
                        </li>
                      </ul>
                      <br />

                    <p>
                      Az értékek fölé mozgatva az egeret megjelenik a pontos időpont.
                    </p>
                </div>
              </span>

              <div class="row">
                <div class="col-lg-4 mb-2 text-center text-lg-left d-print-none pl-lg-4">
                    <button type="button"
                            class="btn btn-sm mb-2"
                            :class="{'btn-primary': globalSettings.diaryType == 'hourly', 'btn-outline-primary': globalSettings.diaryType != 'hourly'}"
                            @click="setDiaryType('hourly')">
                      <i :class="{'icon-check' : globalSettings.diaryType == 'hourly', 'icon-check-empty' : globalSettings.diaryType != 'hourly'}"></i>
                      {{ lang['diaryTypeHourlyText'] }}
                    </button>

                    <button type="button" 
                            class="btn btn-sm mb-2"
                            :class="{'btn-primary': globalSettings.diaryType == 'mealtime', 'btn-outline-primary': globalSettings.diaryType != 'mealtime'}"
                            @click="setDiaryType('mealtime')">
                      <i :class="{'icon-check' : globalSettings.diaryType == 'mealtime', 'icon-check-empty' : globalSettings.diaryType != 'mealtime'}"></i>
                      {{ lang['diaryTypeMealtimeText'] }}
                    </button>
                </div>

                <div class="col-lg-8 mb-2 pr-lg-0" v-if="globalSettings.diaryType == 'mealtime'">
                  <div class="text-center text-lg-right">
                    <div class="d-inline d-lg-block d-xl-inline">
                      <button class="btn btn-sm bg-info text-light mb-2" @click="editMealtime('breakfast')">
                        <strong>{{ breakfastLabel }}:</strong> {{ mealtimeFormat.breakfast }}
                      </button>

                      <button class="btn btn-sm bg-info text-light mb-2" @click="editMealtime('elevenses')">
                        <strong>{{ elevensesLabel }}:</strong> {{ mealtimeFormat.elevenses }}
                      </button>

                      <button class="btn btn-sm bg-info text-light mb-2" @click="editMealtime('lunch')">
                          <strong>{{ lunchLabel }}:</strong> {{ mealtimeFormat.lunch }}
                      </button>
                    </div>

                    <div class="d-inline d-lg-block d-xl-inline">
                      <button class="btn btn-sm bg-info text-light mb-2" @click="editMealtime('afternoonsnack')">
                        <strong>{{ afternoonsnackLabel }}:</strong> {{ mealtimeFormat.afternoonsnack }}
                      </button>

                      <button class="btn btn-sm bg-info text-light mb-2" @click="editMealtime('dinner')">
                          <strong>{{ dinnerLabel }}:</strong> {{ mealtimeFormat.dinner }}
                      </button>
                    </div>
                  </div>
                  <mealtime-editor />
                </div>                
              </div>

              <blood-glucose-diary-mealtime v-if="globalSettings.diaryType == 'mealtime'"
                                            :glucose-values="filteredBloodGlucoseValues"
                                            :insulin-values="filteredInsulinValuesInBulk"
                                            :original-unit="originalUnit"
                                            :insulin-types="insulinTypes" />

              <blood-glucose-diary-hourly v-if="globalSettings.diaryType == 'hourly'"
                                          :original-unit="originalUnit"
                                          :glucose-values="filteredBloodGlucoseValues"
                                          :insulin-values="filteredInsulinValuesInBulk"
                                          :insulin-types="insulinTypes" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-show="!initialized" class="text-center text-primary">
      <div class="spinner-border" role="status">
        <span class="sr-only"></span>
      </div>
      <br />
      {{ loadingText }}
    </div>
  </div>
</template>

<script>
import axios from "axios";
import moment from "moment";

import agpHighcharts from "./components/agp-highcharts";
import bloodGlucoseDiaryHourly from "./components/blood-glucose-diary-hourly.vue";
import bloodGlucoseDiaryMealtime from "./components/blood-glucose-diary-mealtime.vue";
import dgpCharts from "./components/dgp-charts.vue";
import glucoseStatisticsAgp from "./components/glucose-statistics-agp.vue";
import insulinStatistics from "./components/insulin-statistics.vue";
import mealtimeEditor from "./components/mealtime-editor.vue";
import patient from "./components/patient.vue";
import settingsBar from "./components/settings-bar.vue";
import targetRangeChart from "./components/target-range-chart.vue";
import utils, {
  agpConstants,
  betweenDateRange,
  groupByDate,
  countByDate,
  InsulinTypes
} from "./services/utils";
import { EventBus } from "./services/event-bus";
import webStorage from "./services/web-storage";

import "./styles/site.scss";

export default {
  name: "App",
  components: {
    agpHighcharts,
    bloodGlucoseDiaryHourly,
    bloodGlucoseDiaryMealtime,
    dgpCharts,
    glucoseStatisticsAgp,
    insulinStatistics,
    mealtimeEditor,
    patient,
    settingsBar,
    targetRangeChart
  },
  data() {
    return {
      bloodGlucoseValues: [],
      insulinValues: [],
      insulinTypes: [],
      originalUnit: "",
      rangePickerValues: [],
      clientWidth: window.innerWidth,
      initialized: null
    };
  },
  methods: {
    initialize() {
      this.initialized = false;
      axios.get(this.baseUrl).then(result => {
        let data = result.data;

        this.$store.commit("initializePatient", data.parameters.patient);

        this.originalUnit = data.parameters.unit.toLowerCase().trim();
        // Unit may comes from local storage.
        if (!this.globalSettings.unit) {
          this.$store.commit("setGlobalSettingsProperty", {
            key: "unit",
            value: this.originalUnit
          });
        }

        this.insulinTypes = data.parameters.insulintypes.map(x => {
          return {
            type: x.type.toLowerCase().trim(),
            color: x.color.toLowerCase().trim()
          };
        });

        // Overriding colors if stored in LS.
        this.updateInsulinTypeColor(InsulinTypes.basal, "colorBasal");
        this.updateInsulinTypeColor(InsulinTypes.bolus, "colorBolus");
        this.updateInsulinTypeColor(InsulinTypes.mix, "colorMix");

        let veryLowLimitAGP = agpConstants[this.globalSettings.unit].sLowLimit;
        let lowLimitAgp = agpConstants[this.globalSettings.unit].lowLimit;
        let highLimitAgp = agpConstants[this.globalSettings.unit].highLimit;
        let veryHighLimitAGP =
          agpConstants[this.globalSettings.unit].sHighLimit;

        this.bloodGlucoseValues = data.bgms
          .map(bgm => {
            return bgm.values.map(x => {
              let measureType = +x.measuretype;
              let value = +x.glucose;
              let valueInUnit = utils.changeUnit(
                value,
                this.originalUnit,
                this.globalSettings.unit
              );

              let agpIsVeryLow = valueInUnit <= veryLowLimitAGP;
              let agpIsLow = !agpIsVeryLow && valueInUnit <= lowLimitAgp;
              let agpIsVeryHigh = valueInUnit > veryHighLimitAGP;
              let agpIsHigh = !agpIsVeryHigh && valueInUnit > highLimitAgp;

              return {
                bgmrangelow: bgm.bgmrangelow,
                bgmrangehigh: bgm.bgmrangehigh,
                date: new Date(x.measuredate),
                value: value,
                type: measureType,
                measurable:
                  value >= bgm.bgmrangelow && value <= bgm.bgmrangehigh,
                agpIsVeryLow: agpIsVeryLow,
                agpIsLow: agpIsLow,
                agpIsInTarget:
                  !agpIsVeryLow && !agpIsLow && !agpIsHigh && !agpIsVeryHigh,
                agpIsHigh: agpIsHigh,
                agpIsVeryHigh: agpIsVeryHigh
              };
            });
          })
          .reduce((accumulator, currentValue) => {
            return accumulator.concat(currentValue);
          })
          .sort(utils.sortByDate);

        this.insulinValues = data.insulins.map(x => {
          return {
            type: x.type,
            name: x.name,
            values: x.values.map(y => {
              return {
                date: new Date(y.injdate),
                unit: +y.injunit,
                duration: +y.injtime
              };
            })
          };
        });

        setTimeout(() => {
          this.initializeRangePickerValues();
          this.initialized = true;
        }, 2);
      });
    },

    getCountOfGlucoseForRange(property) {
      return this.filteredBloodGlucoseValues.filter(x => x[property] === true)
        .length;
    },

    updateInsulinTypeColor(type, settingName) {
      if (!this.globalSettings[settingName]) {
        return;
      }

      this.insulinTypes.forEach(x => {
        if (x.type == type) {
          x.color = this.globalSettings[settingName];
        }
      });
    },

    initializeRangePickerValues() {
      let countGlucoseValues = group => {
        return Object.keys(group).map(key => {
          return {
            date: key,
            bloodGlocoseCount: group[key],
            insulinCount: 0
          };
        });
      };

      let countInsulinValues = group => {
        return Object.keys(group).map(key => {
          return {
            date: key,
            bloodGlocoseCount: 0,
            insulinCount: group[key]
          };
        });
      };

      let groupToCumulated = group => {
        return Object.keys(group).map(key => {
          let v1 = group[key][0];
          let v2 = group[key][1];

          let bg = v1.bloodGlocoseCount;
          let ins = v1.insulinCount;

          if (v2) {
            bg += v2.bloodGlocoseCount;
            ins += v2.insulinCount;
          }

          return {
            date: new Date(key).getTime(),
            bloodGlocoseCount: bg,
            insulinCount: ins
          };
        });
      };

      let glocuseValues = countGlucoseValues(
        this.bloodGlucoseValues.reduce(countByDate, {})
      );
      let insulinValues = countInsulinValues(
        this.ungroupInsulins().reduce(countByDate, {})
      );

      this.rangePickerValues = groupToCumulated(
        glocuseValues.concat(insulinValues).reduce(groupByDate, {})
      ).sort(utils.sortByDate);

      countGlucoseValues = null;
      countInsulinValues = null;
      groupToCumulated = null;
      glocuseValues = null;
      insulinValues = null;

      let selectedMaxDate = moment()
        .endOf("day")
        .toDate();

      this.$store.commit("initializeRange", {
        allowedMinDate: moment(this.rangePickerValues[0].date)
          .startOf("day")
          .toDate(),
        allowedMaxDate: moment()
          .endOf("day")
          .toDate(),
        selectedMinDate: utils.subtract3Months(selectedMaxDate),
        selectedMaxDate: selectedMaxDate
      });

      this.rangePickerValues.sort(utils.sortByDate);
    },

    ungroupInsulins() {
      return this.insulinValues
        .map(x => {
          let insulinType = this.insulinTypes.filter(y => y.type == x.type)[0];
          return x.values.map(i => {
            return Object.assign(
              {
                name: x.name,
                type: insulinType.type
              },
              i
            );
          });
        })
        .reduce((accumulator, currentValue) => {
          return accumulator.concat(currentValue);
        });
    },

    setDiaryType(diaryType) {
      this.$store.commit("setGlobalSettingsProperty", {
        key: "diaryType",
        value: diaryType
      });
    },

    editMealtime(type) {
      EventBus.$emit("open-mealtime-editor", { type: type });
    },

    onGlobalSettingsUpdated(x) {
      if (x.difference.colorBasal) {
        this.updateInsulinTypeColor(InsulinTypes.basal, "colorBasal");
      }

      if (x.difference.colorBolus) {
        this.updateInsulinTypeColor(InsulinTypes.bolus, "colorBolus");
      }

      if (x.difference.colorMix) {
        this.updateInsulinTypeColor(InsulinTypes.mix, "colorMix");
      }
    },

    onWindowResized() {
      if (this.clientWidth != window.innerWidth) {
        this.clientWidth = window.innerWidth;

        EventBus.$emit("vResized" /*, this.clientWidth*/);
      }
    }
  },
  computed: {
    loadingText() {
      return this.$i18n.t("loadingText", this.currentLanguage);
    },

    globalSettings() {
      return this.$store.state.globalSettingsModule.globalSettings;
    },

    selectedMinDate() {
      return this.$store.state.mainModule.range.selectedMinDate;
    },

    selectedMaxDate() {
      return this.$store.state.mainModule.range.selectedMaxDate;
    },

    totalDays() {
      return this.$store.getters.totalDays;
    },

    mealtimeFormat() {
      let mealtime = this.$store.getters.patientMealtimes;
      let result = {};
      Object.keys(mealtime).forEach(key => {
        result[key] = mealtime[key] ? utils.shortTime(mealtime[key]) : "-";
      });

      return result;
    },

    filteredBloodGlucoseValues() {
      if (
        !this.bloodGlucoseValues ||
        !this.selectedMinDate ||
        !this.selectedMaxDate
      ) {
        return [];
      }

      return this.bloodGlucoseValues.filter(x => {
        if (!this.globalSettings.displayNonMeasurableValues && !x.measurable) {
          return false;
        }

        return x.date >= this.selectedMinDate && x.date < this.selectedMaxDate;
      });
    },

    filteredInsulinValuesByBrands() {
      if (
        !this.insulinValues ||
        !this.selectedMinDate ||
        !this.selectedMaxDate
      ) {
        return [];
      }

      return this.insulinValues.map(x => {
        return {
          insulinType: x.type,
          name: x.name,
          values: x.values.filter(x =>
            betweenDateRange(x.date, this.selectedMinDate, this.selectedMaxDate)
          )
        };
      });
    },

    insulinValuesInBulk() {
      if (
        !this.insulinValues ||
        !this.selectedMinDate ||
        !this.selectedMaxDate
      ) {
        return [];
      }

      return this.ungroupInsulins();
    },

    filteredInsulinValuesInBulk() {
      return this.insulinValuesInBulk.filter(x =>
        betweenDateRange(x.date, this.selectedMinDate, this.selectedMaxDate)
      );
    },

    statisticalData() {
      let statistics = {
        avg: 0,
        gmi: null,
        //hbA1c: null,
        sLow: 0,
        low: 0,
        inTarget: 0,
        high: 0,
        sHigh: 0,
        sum: 0,
        count: this.filteredBloodGlucoseValues.length,
        sd: 0,
        cv: 0,
        atpd: 0,
        maxValue: 0
      };

      if (statistics.count) {
        statistics.sLow = this.getCountOfGlucoseForRange("agpIsVeryLow");
        statistics.low = this.getCountOfGlucoseForRange("agpIsLow");
        statistics.high = this.getCountOfGlucoseForRange("agpIsHigh");
        statistics.sHigh = this.getCountOfGlucoseForRange("agpIsVeryHigh");
        statistics.inTarget = this.getCountOfGlucoseForRange("agpIsInTarget");

        statistics.maxValue = this.filteredBloodGlucoseValues.reduce(
          (accumulator, item) => {
            let currentValue = utils.changeUnit(
              item.value,
              this.originalUnit,
              this.globalSettings.unit
            );

            if (currentValue > accumulator) {
              accumulator = currentValue;
            }

            statistics.sum += currentValue;

            currentValue = null;

            return accumulator;
          },
          0
        );

        statistics.avg = statistics.sum / statistics.count;

        statistics.gmi =
          agpConstants.gmiConstant +
          agpConstants.gmiMultiplier *
            (this.globalSettings.unit == "mmol/l"
              ? 18 * statistics.avg
              : statistics.avg);

        // if (statistics.count >= this.globalSettings.minimumMeasurementsOfHbA1c) {
        //   statistics.hbA1c =
        //     (agpConstants[this.globalSettings.unit].a1cConstants +
        //       statistics.avg) /
        //     agpConstants[this.globalSettings.unit].a1cDivider;
        // }

        statistics.sd = Math.sqrt(
          this.filteredBloodGlucoseValues.reduce((accumulator, item) => {
            let currentValue = utils.changeUnit(
              item.value,
              this.originalUnit,
              this.globalSettings.unit
            );

            return (
              accumulator + Math.pow(Math.abs(currentValue - statistics.avg), 2)
            );
          }, 0) / statistics.count
        );

        statistics.cv = (statistics.sd / statistics.avg) * 100;

        let selectedMinDateTime = this.selectedMinDate.getTime();
        let selectedMaxDateTime = this.selectedMaxDate.getTime();

        this.rangePickerValues
          .filter(
            x => selectedMinDateTime <= x.date && x.date < selectedMaxDateTime
          )
          .forEach(x => {
            statistics.atpd += x.bloodGlocoseCount;
          });

        statistics.atpd /= this.totalDays;
      }

      return statistics;
    },

    isDcmView() {
      return this.globalSettings.viewMode == "DCMView";
    },

    insulinBrands() {
      return this.filteredInsulinValuesByBrands
        .map(x => {
          let type = this.insulinTypes.filter(y => y.type == x.insulinType)[0];
          return {
            name: x.name,
            type: type.type
          };
        })
        .filter(x => {
          return this.filteredInsulinValuesInBulk.some(y => y.type == x.type);
        });
    },

    lang() {
      return this.$i18n.t("app", this.globalSettings.currentLanguage);
    },

    breakfastLabel() {
      return this.$i18n.t("breakfast", this.globalSettings.currentLanguage);
    },

    elevensesLabel() {
      return this.$i18n.t("elevenses", this.globalSettings.currentLanguage);
    },

    lunchLabel() {
      return this.$i18n.t("lunch", this.globalSettings.currentLanguage);
    },

    afternoonsnackLabel() {
      return this.$i18n.t(
        "afternoonsnack",
        this.globalSettings.currentLanguage
      );
    },

    dinnerLabel() {
      return this.$i18n.t("dinner", this.globalSettings.currentLanguage);
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("globalSettingsUpdated", this.onGlobalSettingsUpdated);

    window.addEventListener("resize", this.onWindowResized);
    window.addEventListener("afterprint", this.onAfterPrint);
  },
  beforeDestroy() {
    EventBus.$off("globalSettingsUpdated", this.onGlobalSettingsUpdated);

    window.removeEventListener("resize", this.onWindowResized);
    window.removeEventListener("afterprint", this.onAfterPrint);
  },
  props: ["baseUrl", "patientProfileRawUrl"]
};
</script>
