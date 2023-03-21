<template>
  <div class="row">
    <div class="col">
      <div class="card card-center-profile-editor card-center-profile-editor-primary mt-3">
        <div class="card-header">
          <h4 class="m-0">
            Szakellátóhely típusa
          </h4>
        </div>
        <div class="card-body">
          <div class="row form-group">
            <div class="col-12">
              <div class="custom-control custom-switch">
                <input type="checkbox"
                       class="custom-control-input"
                       id="vocationalClinic"
                       v-model="centerData.vocationalClinic" />
                <label class="custom-control-label" for="vocationalClinic">
                  Önálló diabétesz szakrendelés
                </label>
              </div>
            </div>
          </div>

          <div class="row">
            <div class="col-12">
              <div class="custom-control custom-switch">
                <input type="checkbox"
                       class="custom-control-input"
                       id="partOfOtherVocationalClinic"
                       v-model="centerData.partOfOtherVocationalClinic" />
                <label class="custom-control-label" for="partOfOtherVocationalClinic">
                  Más szakrendelés része
                </label>
              </div>
            </div>
            <div class="col-12 mt-2" v-show="centerData.partOfOtherVocationalClinic">
              <textarea class="form-control" placeholder="Kérjük, adja meg mely szakrendelés része" v-model="centerData.otherVocationalClinic"></textarea>
            </div>
          </div>

          <div class="row" v-show="submitted && (validationErrors.vocationalClinics || validationErrors.otherVocationalClinic)">
            <div class="col">
              <p class="center-profile-validation-error" v-show="validationErrors.vocationalClinics">
                  <i class="fas fa-exclamation-triangle"></i>
                  Az egyik kiválasztása kötelező
              </p>

              <p class="center-profile-validation-error" v-show="validationErrors.otherVocationalClinic">
                  <i class="fas fa-exclamation-triangle"></i>
                  Kérem, adja meg, hogy mely szakrendelés része
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
        <div class="card-header">
          <h4 class="m-0">
            NEAK adatok
          </h4>
        </div>
        <div class="card-body">
          <div class="row" :class="{'form-group': centerData.neakContract}">
            <div class="col-12">
              <div class="custom-control custom-switch">
                <input type="checkbox"
                       class="custom-control-input"
                       id="neak-contract"
                       v-model="centerData.neakContract" />
                <label class="custom-control-label" for="neak-contract">
                  Szerződéssel rendelkezik
                </label>
              </div>
            </div>
          </div>

          <div class="row" v-show="centerData.neakContract">
            <div class="col-12" v-for="(neak, index) in centerData.neak" v-bind:key="index">
              <div class="card card-neak mb-3" :class="{ 'card-neak-primary': centerData.neakPrimary == index }">
                <div class="card-body card-body-neak">
                  <div class="neak-toolbar">
                    <div class="row">
                      <div class="col-12 align-self-center">
                        <div class="custom-control custom-radio neak-primary">
                          <input type="radio"
                                class="custom-control-input"
                                name="neak-primary"
                                :id="`neak-primary-${index}`"
                                v-model="centerData.neakPrimary"
                                :value="index" />
                          <label class="custom-control-label" :for="`neak-primary-${index}`">
                            Elsődleges
                          </label>
                        </div>

                        <button type="button"
                                class="btn btn-sm btn-remove-neak float-right"
                                :class="{ 'btn-danger' : centerData.neak.length != 1, 'btn-outline-secondary' : centerData.neak.length == 1 }"
                                :title="centerData.neak.length != 1 ? 'Blokk törlése': 'Egy blokk adatainak kitöltése kötelező, ezért nem törölhető'"
                                :disabled="centerData.neak.length == 1"
                                @click="removeNeak(index)">
                          <i class="fas fa-trash"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <div class="row form-group">
                    <label class="col-form-label col-12 col-sm-9 col-md-10" :for="`neakNumberOfHours-${index}`">
                      Szerződésben foglalt heti óraszám
                    </label>
                    <div class="col-12 col-sm-3 col-md-2">
                      <input type="number"
                            min="0"
                            v-model="neak.numberOfHours"
                            class="form-control"
                            :id="`neakNumberOfHours-${index}`" />
                    </div>
                    <div class="col-12" v-show="submitted && validationErrors.neak && validationErrors.neak[index] && validationErrors.neak[index].numberOfHours">
                      <p class="center-profile-validation-error">
                        <i class="fas fa-exclamation-triangle"></i>
                        Helytelen érték
                      </p>
                    </div>
                  </div>

                  <div class="row form-group">
                    <label class="col-form-label col-12 col-sm-9 col-md-10" :for="`neakNumberOfHoursDiabetes-${index}`">
                      Ebből cukorbetegekre fordított óraszám
                    </label>
                    <div class="col-12 col-sm-3 col-md-2">
                      <input type="number"
                            min="0"
                            v-model="neak.numberOfHoursDiabetes"
                            class="form-control"
                            :id="`neakNumberOfHoursDiabetes-${index}`" />
                    </div>
                    <div class="col-12" v-show="submitted && validationErrors.neak && validationErrors.neak[index] && validationErrors.neak[index].numberOfHoursDiabetes">
                      <p class="center-profile-validation-error">
                        <i class="fas fa-exclamation-triangle"></i>
                        Helytelen érték
                      </p>
                    </div>
                    <div class="col-12" v-show="submitted && validationErrors.neak && validationErrors.neak[index] && validationErrors.neak[index]['noh<nohd']">
                      <p class="center-profile-validation-error">
                        <i class="fas fa-exclamation-triangle"></i>
                        A cukorbetegekre fordított óraszám nem lehet több, mint a szerződésben foglalt heti óraszám
                      </p>
                    </div>
                  </div>

                  <div class="row">
                    <label class="col-form-label col-12 col-sm-6 col-md-9"
                          :for="`neakWorkplaceCode-${index}`">
                      Munkahelyi kód
                    </label>
                    <div class="col-12 col-sm-6 col-md-3">
                      <input type="text"
                            v-model="neak.workplaceCode"
                            class="form-control"
                            :id="`neakWorkplaceCode-${index}`" />
                    </div>
                    <div class="col-12" v-show="submitted && validationErrors.neak && validationErrors.neak[index] && validationErrors.neak[index].workplaceCode">
                      <p class="center-profile-validation-error">
                        <i class="fas fa-exclamation-triangle"></i>
                        Helytelen érték
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="col-12 text-center" @click="addNeak()">
              <button type="button" class="btn btn-success">
                <i class="fas fa-plus"></i>
                Új NEAK adat felvétele
              </button>
            </div>

            <div class="col-12 mt-3">
              <p class="alert alert-info border-info m-0">
                <i class="fas fa-info-circle"></i>
                Kormányhivatal (ÁNTSZ) engedély az elsődlegesen megadott NEAK munkahelyi kódhoz rendelten kerül megadásra.
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
        <div class="card-header">
          <h4 class="m-0">
            Kormányhivatal (ÁNTSZ) engedély
          </h4>
        </div>
        <div class="card-body">
          <div class="row form-group">
            <label for="antszNumber" class="col-form-label col-12 col-sm-6 col-md-9">
              Száma
            </label>
            <div class="col-12 col-sm-6 col-md-3">
              <input type="text"
                     id="antszNumber"
                     class="form-control"
                     v-model="centerData.antsz.number" />
            </div>
            <div class="col-12"  v-show="submitted && validationErrors.antszNumber">
              <p class="center-profile-validation-error">
                <i class="fas fa-exclamation-triangle"></i>
                Helytelen érték
              </p>
            </div>
          </div>

          <div class="row form-group">
            <label for="antszDate" class="col-form-label col-12 col-sm-6 col-md-9">
              Kelte
            </label>
            <div class="col-12 col-sm-6 col-md-3">
              <input type="date" id="antszDate" class="form-control"
                     :value="centerData.antsz.date | inputDate"
                     @input="updateAntszDate($event)" />
            </div>
            <div class="col-12" v-show="submitted && validationErrors.antszDate">
              <p class="center-profile-validation-error">
                <i class="fas fa-exclamation-triangle"></i>
                Helytelen érték
              </p>
            </div>
          </div>

          <div class="row">
            <label for="antszId" class="col-form-label col-12 col-sm-6 col-md-9">
              Azonosító kód
            </label>
            <div class="col-12 col-sm-6 col-md-3">
              <input type="text"
                     id="antszId"
                     class="form-control"
                     v-model="centerData.antsz.id" />
            </div>
            <div class="col-12" v-show="submitted && validationErrors.antszId">
              <p class="center-profile-validation-error">
                <i class="fas fa-exclamation-triangle"></i>
                Helytelen érték
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
        <div class="card-header">
          <h4 class="m-0">
            Rendelési idő
          </h4>
        </div>
        <div class="card-body">
          <div v-for="(day, dayIndex) in officeHoursFullWeek"
            v-bind:key="day.day"
            class="border rounded bg-white p-2"
            :class="{'mb-3': dayIndex < officeHoursFullWeek.length - 1}">
            <div class="row">
              <div class="col-12" :class="{'mb-3': day.hours.length}">
                <label class="col-form-label">
                  <strong>{{ dayNames[day.day] }}</strong>
                </label>

                <button class="btn btn-sm btn-success float-right" @click="addOfficeHours(day.day)"
                  :title="`Új rendelési idő felvétele a ${dayNames[day.day]}i naphoz`">
                  <i class="fas fa-plus"></i>
                  Hozzáad
                </button>
              </div>
              <div class="col-12">
                <div class="row">
                  <div v-for="(hour, hourIndex) in day.hours"
                       v-bind:key="hourIndex"
                       class="col-md-6 col-lg-4"
                       :class="{
                        'mb-3': hourIndex < day.hours.length - 1,
                        'mb-md-0': hourIndex >= day.hours.length - 2 && hourIndex % 2 == 0
                       }">
                    <div class="input-group input-group-sm">
                      <input type="time" v-model="hour.timeFrom" class="form-control form-control-sm" />
                      <div class="input-group-append">
                        <span class="input-group-text pr-2 pl-2 border-right-0">-</span>
                      </div>
                      <input type="time" v-model="hour.timeTo" class="form-control form-control-sm" />
                      <div class="input-group-append">
                        <span class="input-group-text pr-1 pl-1" title="Törlés"
                              @click="removeOfficeHours(day.day, hourIndex)">
                          <i class="fas fa-trash text-danger"></i>
                        </span>
                      </div>
                    </div>

                    <p class="center-profile-validation-error"
                       v-show="submitted && 
                               validationErrors.officeHoursFullWeek &&
                               validationErrors.officeHoursFullWeek[dayIndex] &&
                               validationErrors.officeHoursFullWeek[dayIndex][hourIndex]">
                      <i class="fas fa-exclamation-triangle"></i>
                      Helytelen időpont
                    </p>
                  </div>

                  <div v-show="!day.hours.length" class="col">
                    <div class="text-muted text-center">
                      <small>
                        Nincs megadott rendelési idő.
                      </small>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <p class="alert alert-danger mt-3 mb-0"
             v-show="submitted &&
                     centerData.neakContract &&
                     validationErrors.officeAndNeakHoursNotEqual">
            <i class="fas fa-exclamation-triangle"></i>
            A két óraszámnak meg kell egyeznie:
            <br />
            1. A rendelési idők összesítve: {{ sumOfficeTimeInHours.toFixed(2) }} óra
            <br />
            2. Szerződésben foglalt heti óraszámból cukorbetegekre fordított órák összesítve: {{ sumNeakNumberOfHoursDiabetes.toFixed(2) }} óra
          </p>

          <p class="alert alert-success mt-3 mb-0"
             v-show="centerData.neakContract && 
                     sumOfficeTimeInHours > 0 &&
                     sumOfficeTimeInHours == sumNeakNumberOfHoursDiabetes">
            <i class="fas fa-check-circle"></i>
            A rendelési idő megegyezik a szerződésben foglalt heti óraszámmal.
          </p>
        </div>
      </div>

      <div class="row mt-5" v-show="submitted && hasValidationErrors">
        <div class="col-sm-8 offset-sm-4 col-md-9 offset-md-3">
          <p class="center-profile-validation-error">
            <i class="fas fa-exclamation-triangle"></i>
            Kérjük, javítsa a hibákat!
          </p>
        </div>
      </div>

      <div class="row" v-if="submitted && serverValidationErrors.length">
          <div class="col text-right">
            <ul class="list-unstyled alert alert-danger mt-3 mb-0">
              <li v-for="(error, index) in serverValidationErrors" v-bind:key="index">
                {{ error }}
              </li>
            </ul>
            </div>
        </div>
    </div>
  </div>
</template>

<script>
import moment from "moment";
import creationRequestService from "../services/creation-request-service";
import { EventBus } from "../event-bus";
import utils from "../utils";

export default {
  data() {
    return {
      centerData: {
        vocationalClinic: null,
        partOfOtherVocationalClinic: null,
        otherVocationalClinic: null,
        neakContract: null,
        neakPrimary: 0,
        neak: [],
        antsz: {
          number: null,
          date: null,
          id: null
        },
        officeHours: []
      },
      centerDataIsDirty: false,
      submitted: false,
      serverValidationErrors: []
    };
  },
  methods: {
    setCenterDataPristine() {
      this.$nextTick(() => {
        this.centerDataIsDirty = false;
      });
    },

    getEmptyNeak() {
      return {
        numberOfHours: null,
        numberOfHoursDiabetes: null,
        workplaceCode: null
      };
    },

    addNeak() {
      this.centerData.neak.push(this.getEmptyNeak());
    },

    removeNeak(index) {
      if (this.centerData.neak.length == 1) {
        return;
      }

      confirmationModal({
        message: "Biztos, hogy törölni szeretné ezeket a NEAK adatokat?",
        callback: confirmed => {
          if (confirmed) {
            this.centerData.neak.splice(index, 1);
            if (this.centerData.neakPrimary == index) {
              this.centerData.neakPrimary = 0;
            }

            if (this.centerData.neakPrimary > index) {
              this.centerData.neakPrimary--;
            }
          }
        }
      });
    },

    getDefaultAntsz() {
      return {
        number: null,
        date: null,
        id: null
      };
    },

    initialize() {
      creationRequestService
        .getCenterProfile(this.urls.apiUrl, {
          s: "AdditionalData"
        })
        .then(response => {
          Object.assign(this.centerData, response.data.viewModel, {
            antsz: response.data.viewModel.antsz
              ? Object.assign(response.data.viewModel.antsz, {
                  date: response.data.viewModel.antsz.date
                    ? new Date(response.data.viewModel.antsz.date)
                    : null
                })
              : this.getDefaultAntsz(),
            neakContract: response.data.viewModel.neak.length > 0
          });

          if (!this.centerData.neakContract || !this.centerData.neak.length) {
            this.addNeak();
            this.centerData.neakPrimary = 0;
          } else {
            this.centerData.neak.forEach((neak, index) => {
              if (neak.primary) {
                this.centerData.neakPrimary = index;
              }

              delete neak["primary"];
            });
          }

          this.setCenterDataPristine();
        })
        .catch(e => {
          this.setCenterDataPristine();
          utils.alertModal("Az adatok mentése nem sikerült");
        });
    },

    addOfficeHours(day) {
      let modelDay = this.centerData.officeHours.find(x => x.day == day);
      if (!modelDay) {
        modelDay = {
          day: day,
          hours: []
        };

        this.centerData.officeHours.push(modelDay);
      }

      modelDay.hours.push({
        timeFrom: null,
        timeTo: null
      });
    },

    removeOfficeHours(day, index) {
      let modelDay = this.centerData.officeHours.find(x => x.day == day);
      if (!modelDay) {
        return;
      }

      modelDay.hours.splice(index, 1);
    },

    updateAntszDate(e) {
      this.centerData.antsz.date = e.target.valueAsDate;
    },

    addNeakValidationError(result, index, property) {
      if (!result.neak) {
        result.neak = {};
      }

      if (!result.neak[index]) {
        result.neak[index] = {};
      }

      result.neak[index][property] = 1;
    },

    addOfficeHoursValidationError(result, dayIndex, hourIndex) {
      if (!result.officeHoursFullWeek) {
        result.officeHoursFullWeek = {};
      }

      if (!result.officeHoursFullWeek[dayIndex]) {
        result.officeHoursFullWeek[dayIndex] = {};
      }

      result.officeHoursFullWeek[dayIndex][hourIndex] = 1;
    },

    splitAndGetTimeMoment(timeAsString) {
      var split = timeAsString.split(":");

      return moment(new Date())
        .startOf("day")
        .add(split[0], "hours")
        .add(split[1], "minutes");
    },

    onSave() {
      this.submitted = true;
      if (this.hasValidationErrors) {
        this.alertOnContinue();
        return;
      }

      var data = Object.assign({}, this.centerData, {
        neak: this.centerData.neakContract
          ? this.centerData.neak.map((n, index) => {
              return Object.assign({}, n, {
                primary: index == this.centerData.neakPrimary
              });
            })
          : [],
        officeHours: this.centerData.officeHours.filter(x => x.hours.length)
      });

      delete data["neakPrimary"];
      delete data["neakContract"];

      creationRequestService
        .submit(this.urls.additionalDataUrl, data)
        .then(response => {
          if (response.status == 200) {
            this.setCenterDataPristine();
            this.$nextTick(() => {
              EventBus.$emit("saved");
            });
          } else {
            EventBus.$emit("failed-to-save");
          }
        })
        .catch(e => {
          this.serverValidationErrors = [];
          for (var key in e.response.data) {
            e.response.data[key].forEach(x => {
              this.serverValidationErrors.push(x);
            });
          }
        });
    },

    onBeforeUnload(e) {
      utils.preventPageLeaveIfFormIsDirty(e, this.centerDataIsDirty);
    },

    alertOnContinue() {
      utils.alertModal(
        "A folytatás előtt kérjük adja meg a helyes kiegészítő adatokat."
      );
    },

    onSummaryValidationErrors(e) {
      this.submitted = true;
      this.serverValidationErrors = e.errors;
      this.alertOnContinue();
    }
  },
  computed: {
    urls() {
      return this.$store.state.options.urls;
    },

    nowMaxDate() {
      return moment().format("YYYY-MM-DD");
    },

    officeHoursFullWeek() {
      return utils.mapOfficeHours(
        this.$store.getters.days,
        this.centerData.officeHours
      );
    },

    dayNames() {
      return this.$store.getters.dayNames;
    },

    sumNeakNumberOfHoursDiabetes() {
      return this.centerData.neak
        .filter(neak => neak.numberOfHoursDiabetes)
        .reduce((accumulator, neak) => {
          return accumulator + +neak.numberOfHoursDiabetes;
        }, 0);
    },

    sumOfficeTimeInHours() {
      return (
        this.officeHoursFullWeek.reduce((accumulator, day) => {
          return (
            accumulator +
            day.hours
              .filter(hour => hour.timeFrom && hour.timeTo)
              .reduce((subAccumulator, hour) => {
                var timeFrom = this.splitAndGetTimeMoment(hour.timeFrom);
                var timeTo = this.splitAndGetTimeMoment(hour.timeTo);

                if (timeFrom.isAfter(timeTo)) {
                  return subAccumulator;
                }

                return subAccumulator + timeTo.diff(timeFrom, "minutes");
              }, 0)
          );
        }, 0) / 60
      );
    },

    hasValidationErrors() {
      return !!(
        Object.keys(this.validationErrors).length &&
        this.validationErrors.constructor === Object
      );
    },

    validationErrors() {
      var result = {};

      if (
        !this.centerData.vocationalClinic &&
        !this.centerData.partOfOtherVocationalClinic
      ) {
        result.vocationalClinics = 1;
      }

      if (
        this.centerData.partOfOtherVocationalClinic &&
        !this.centerData.otherVocationalClinic
      ) {
        result.otherVocationalClinic = 1;
      }

      if (this.centerData.neakContract) {
        this.centerData.neak.forEach((neak, index) => {
          if (!neak.numberOfHours || +neak.numberOfHours <= 0) {
            this.addNeakValidationError(result, index, "numberOfHours");
          }

          if (!neak.numberOfHoursDiabetes || +neak.numberOfHoursDiabetes <= 0) {
            this.addNeakValidationError(result, index, "numberOfHoursDiabetes");
          }

          if (
            +neak.numberOfHours > 0 &&
            +neak.numberOfHoursDiabetes > 0 &&
            +neak.numberOfHours < +neak.numberOfHoursDiabetes
          ) {
            this.addNeakValidationError(result, index, "noh<nohd");
          }

          if (!neak.workplaceCode) {
            this.addNeakValidationError(result, index, "workplaceCode");
          }
        });
      }

      if (!this.centerData.antsz.number) {
        result.antszNumber = 1;
      }

      if (!this.centerData.antsz.date) {
        result.antszDate = 1;
      }

      if (!this.centerData.antsz.id) {
        result.antszId = 1;
      }

      this.officeHoursFullWeek.forEach((day, dayIndex) => {
        day.hours.forEach((hour, hourIndex) => {
          if (!hour.timeFrom || !hour.timeTo) {
            this.addOfficeHoursValidationError(result, dayIndex, hourIndex);

            return;
          }

          if (
            this.splitAndGetTimeMoment(hour.timeFrom).isSameOrAfter(
              this.splitAndGetTimeMoment(hour.timeTo)
            )
          ) {
            this.addOfficeHoursValidationError(result, dayIndex, hourIndex);
          }
        });
      });

      if (
        this.centerData.neakContract &&
        this.sumOfficeTimeInHours != this.sumNeakNumberOfHoursDiabetes
      ) {
        result.officeAndNeakHoursNotEqual = 1;
      }

      return result;
    }
  },
  filters: {
    inputDate(date) {
      if (date) {
        return moment(date).format("YYYY-MM-DD");
      }
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("save", this.onSave);
    EventBus.$on("summary-validation-errors", this.onSummaryValidationErrors);

    window.addEventListener("beforeunload", this.onBeforeUnload);
  },
  beforeDestroy() {
    EventBus.$off("save", this.onSave);
    EventBus.$off("summary-validation-errors", this.onSummaryValidationErrors);

    window.removeEventListener("beforeunload", this.onBeforeUnload);
  },
  beforeRouteLeave(to, from, next) {
    // Stepping backward is allowed but forward isn't
    if (this.hasValidationErrors && to.name != "basicData") {
      this.submitted = true;
      this.alertOnContinue();

      next(false);
      return;
    }

    utils.preventRouteLeaveIfFormIsDirty(
      this.centerDataIsDirty,
      "Nem mentett adatok vannak az űrlapon, folytatja?",
      next
    );
  },
  watch: {
    centerData: {
      handler() {
        this.centerDataIsDirty = true;
      },
      deep: true
    }
  }
};
</script>
