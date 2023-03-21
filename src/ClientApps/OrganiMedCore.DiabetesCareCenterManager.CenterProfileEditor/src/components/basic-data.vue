<template>
    <div class="row">
      <div class="col-12">
        <div class="card card-center-profile-editor card-center-profile-editor-primary mt-3">
          <div class="card-header">
            <h4 class="m-0">
              Szakellátóhely vezető személyes adatai
            </h4>
          </div>

          <div class="card-body">
            <div class="table-responsive table-striped">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Név</th>
                    <th>Email</th>
                    <th class="text-right">Adatlap megtekintése</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="additionalData.leader">
                    <td>{{ additionalData.leader.fullName }}</td>
                    <td>
                      <a v-if="additionalData.leader.email" :href="`mailto:${additionalData.leader.email}`">
                        {{ additionalData.leader.email }}
                      </a>
                    </td>
                    <td class="text-right">
                      <button type="button"
                              class="btn btn-sm btn-info"
                              :title="'Szakellátóhely vezető adatlapja'"
                              @click="viewLeaderProfile()">
                        <i class="fas fa-info"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
          <div class="card-header">
            <h4>
              Szakellátóhely adatok
            </h4>
          </div>

          <div class="card-body">
            <div class="row form-group">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Szakellátóhely neve</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <input type="text"
                       class="form-control"
                       v-model="centerData.basicData.centerName"
                       autofocus />
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.centerName">
                  <i class="fas fa-exclamation-triangle"></i>
                  A szakellátóhely nevének megadása kötelező
                </p>
              </div>
            </div>

            <div class="row form-group mt-5">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Irányítószám</span>
              </label>
              <div class="col-sm-3 col-md-2">
                <input type="number"
                       min="1000"
                       max="9999"
                       class="form-control"
                       placeholder="----"
                       v-model="centerData.basicData.centerZipCode"
                       @input="getSettlements()" />
              </div>
              <div class="col-sm-8 offset-sm-4 col-md-9 offset-md-3">
                <p class="center-profile-validation-error" v-show="wrongZipCode">
                  <i class="fas fa-exclamation-triangle"></i>
                  A megadott irányítószám nem létezik a rendszerben
                </p>
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.centerZipCode">
                  <i class="fas fa-exclamation-triangle"></i>
                  Az irányítószám megadása kötelező
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Település</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <select
                  class="form-control"
                  v-model="centerData.basicData.centerSettlement"
                  @change="centerSettlementChanged()">
                  <option
                    v-for="x in settlementsToRapporteurs"
                    v-bind:key="x.zipCode + x.settlement"
                    :value="x.settlement">
                    {{ x.settlement }}
                  </option>
                </select>
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.centerSettlement">
                  <i class="fas fa-exclamation-triangle"></i>
                  A település megadása kötelező
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Cím</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <input
                  type="text"
                  class="form-control"
                  placeholder="Közterület, közterület jellege, házszám"
                  :readonly="!settlementsToRapporteurs.length"
                  v-model="centerData.basicData.centerAddress"
                  @blur="geocode()" />
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.centerAddress">
                  <i class="fas fa-exclamation-triangle"></i>
                  A cím megadása kötelező
                </p>
              </div>
            </div>

            <div class="row form-group" v-show="showMap">
              <div class="col-12 col-lg-9 offset-lg-3">
                <div id="map" class="border rounded" style="height:280px;"></div>
              </div>
            </div>

            <div class="row form-group mt-5">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Telefonszám</span>
              </label>
              <div class="col-sm-5 col-md-4 col-lg-3 col-xl-3">
                <input type="text" class="form-control" v-model="centerData.basicData.phone" placeholder="Csak számot írjon be" />
              </div>
              <div class="col-sm-8 offset-sm-4 col-md-9 offset-md-3">
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.phone">
                  <i class="fas fa-exclamation-triangle"></i>
                  Helytelen telefonszám: csak 7-11 karakter hosszú számot adjon meg
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-sm-4 col-md-3 text-sm-right">
                <span>Fax</span>
              </label>
              <div class="col-sm-5 col-md-4 col-lg-3 col-xl-3">
                <input type="text" class="form-control" v-model="centerData.basicData.fax" placeholder="Csak számot írjon be" />
              </div>
              <div class="col-sm-8 offset-sm-4 col-md-9 offset-md-3" v-show="submitted && validationErrors.fax">
                <p class="center-profile-validation-error">
                  <i class="fas fa-exclamation-triangle"></i>
                  Helytelen fax: csak 7-11 karakter hosszú számot adjon meg
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Email</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <input type="email" class="form-control"
                       v-model="centerData.basicData.email" />
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.email">
                  <i class="fas fa-exclamation-triangle"></i>
                  Helytelen email
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-sm-4 col-md-3 text-sm-right">
                <span>Weboldal</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <input type="url" class="form-control" v-model="centerData.basicData.web" />
                <p class="center-profile-validation-error" v-show="submitted && validationErrors.web">
                  <i class="fas fa-exclamation-triangle"></i>
                  Helytelen weboldal
                </p>
              </div>
            </div>

            <div class="row form-group mt-5">
              <label class="col-form-label label-required col-sm-4 col-md-3 text-sm-right">
                <span>Profil</span>
              </label>
              <div class="col-sm-8 col-md-9">
                <ul class="list-unstyled">
                  <li>
                    <div class="custom-control custom-radio">
                      <input type="radio"
                             v-model="centerTypesComplex.majorType"
                             :value="0"
                             class="custom-control-input"
                             id="center-type-radio-0"
                             name="center-type-major"
                             @change="centerTypesComplexMajorChanged" />
                      <label class="custom-control-label" for="center-type-radio-0">
                        Felnőtt szakellátóhely
                      </label>
                    </div>
                  </li>

                  <li class="ml-3">
                    <div class="custom-control custom-switch">
                      <input type="checkbox"
                             v-model="centerTypesComplex.isGestational"
                             class="custom-control-input"
                             id="center-type-checkbox-is-gestational"
                             :disabled="centerTypesComplex.majorType != 0"
                             @change="centerTypesComplexMinorChanged(2)" />
                      <label class="custom-control-label" for="center-type-checkbox-is-gestational">
                        Gesztációs szakellátóhely is egyben
                      </label>
                    </div>
                  </li>

                  <li class="ml-3">
                    <div class="custom-control custom-switch">
                      <input type="checkbox" 
                             v-model="centerTypesComplex.isContinuousInsulinDelivery0"
                             class="custom-control-input"
                             id="center-type-checkbox-0-is-cid"
                             :disabled="centerTypesComplex.majorType != 0"
                             @change="centerTypesComplexMinorChanged(3)" />
                      <label class="custom-control-label" for="center-type-checkbox-0-is-cid">
                        Folyamatos inzulinadagoló szakellátóhely is egyben
                      </label>
                    </div>
                  </li>

                  <li class="mt-3">
                    <div class="custom-control custom-radio">
                      <input type="radio"
                             v-model="centerTypesComplex.majorType"
                             :value="1"
                             class="custom-control-input"
                             id="center-type-radio-1"
                             name="center-type-major"
                             @change="centerTypesComplexMajorChanged" />
                      <label class="custom-control-label" for="center-type-radio-1">
                        Gyermek szakellátóhely
                      </label>
                    </div>
                  </li>

                  <li class="ml-3">
                    <div class="custom-control custom-switch">
                      <input type="checkbox"
                             v-model="centerTypesComplex.isContinuousInsulinDelivery1"
                             class="custom-control-input"
                             id="center-type-checkbox-1-is-cid"
                             :disabled="centerTypesComplex.majorType != 1"
                             @change="centerTypesComplexMinorChanged(3)" />
                      <label class="custom-control-label" for="center-type-checkbox-1-is-cid">
                        Folyamatos inzulinadagoló szakellátóhely is egyben
                      </label>
                    </div>
                  </li>
                </ul>

                <p class="center-profile-validation-error" v-show="submitted && validationErrors.centerTypes">
                  <i class="fas fa-exclamation-triangle"></i>
                  Legalább egyet kötelező választani
                </p>
              </div>
            </div>

            <div class="row form-group mt-5" v-show="territorialRapporteur">
              <label class="col-form-label col-sm-4 col-md-3 text-sm-right">
                Területi referens
              </label>
              <div class="col-sm-8 col-md-9">
                <input type="text" readonly class="form-control" :value="territorialRapporteur" />
              </div>
            </div>

            <div class="row" v-show="geocodeFailed">
              <div class="col">
                <span class="text-muted">
                  <i class="fas fa-exclamation-triangle"></i>
                  A térképes megjelenítés nem sikerült
                </span>
              </div>
            </div>
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
import creationRequestService from "../services/creation-request-service";
import { EventBus } from "../event-bus";
import utils from "../utils";

export default {
  data() {
    return {
      additionalData: {
        leader: {
          memberRightId: null,
          fullName: null
        }
      },
      centerData: {
        basicData: {
          centerName: null,
          centerZipCode: null,
          centerSettlement: null,
          centerAddress: null,
          centerLatitude: null,
          centerLongitude: null,
          centerTypes: [],
          prenatalCare: null,
          insulinPump: null,
          email: null,
          fax: null,
          phone: null,
          web: null,
          diabeticPregnancyCare: false,
          insulinPumpTreatment: false
        },
        colleagues: []
      },
      centerTypesComplex: {
        majorType: null,
        isGestational: false,
        isContinuousInsulinDelivery0: false,
        isContinuousInsulinDelivery1: false
      },
      centerDataIsDirty: false,
      territorialRapporteur: null,
      wrongZipCode: false,
      settlementsToRapporteurs: [],
      geocoder: null,
      map: null,
      showMap: false,
      geocodeFailed: false,
      submitted: false,
      serverValidationErrors: []
    };
  },
  methods: {
    initialize() {
      creationRequestService
        .getCenterProfile(this.urls.apiUrl, {
          s: "BasicData"
        })
        .then(response => {
          Object.assign(this.additionalData, response.data.additionalData);
          Object.assign(this.centerData.basicData, response.data.viewModel);

          if (this.centerData.basicData.centerTypes.indexOf(0) > -1) {
            this.centerTypesComplex.majorType = 0;

            if (this.centerData.basicData.centerTypes.indexOf(2) > -1) {
              this.centerTypesComplex.isGestational = true;
            }
            if (this.centerData.basicData.centerTypes.indexOf(3) > -1) {
              this.centerTypesComplex.isContinuousInsulinDelivery0 = true;
            }
          } else if (this.centerData.basicData.centerTypes.indexOf(1) > -1) {
            this.centerTypesComplex.majorType = 1;

            if (this.centerData.basicData.centerTypes.indexOf(3) > -1) {
              this.centerTypesComplex.isContinuousInsulinDelivery1 = true;
            }
          }

          if (!this.googleMapLoaded) {
            utils
              .loadScript(
                `https://maps.googleapis.com/maps/api/js?key=${this.options.googleMapsApiKey}`
              )
              .then(() => {
                EventBus.$emit("google-maps-loaded");
                if (
                  this.centerData.basicData.centerZipCode ||
                  this.centerData.centerSettlement
                ) {
                  this.searchByZipCode(this.setCenterDataPristine);
                } else {
                  this.setCenterDataPristine();
                }
              });
          } else {
            this.searchByZipCode(this.setCenterDataPristine);
          }
        })
        .catch(e => {
          console.log(e);
          utils.loadingFailed();
          this.setCenterDataPristine();
        });
    },

    setCenterDataPristine() {
      this.$nextTick(() => {
        this.centerDataIsDirty = false;
      });
    },

    getSettlements() {
      this.settlementsToRapporteurs = [];
      this.territorialRapporteur = null;
      this.wrongZipCode = false;
      this.centerData.basicData.centerSettlement = null;

      if (!this.validCenterZipCode) {
        this.showMap = false;
        this.removeMarker();
        return;
      }

      this.searchByZipCode();
    },

    searchByZipCode(callback) {
      creationRequestService
        .searchByZipCode(this.urls.searchByZipCodeUrl, this.validCenterZipCode)
        .then(result => {
          if (!result.data || !result.data.length) {
            this.wrongZipCode = true;
            if (callback) {
              callback();
            }

            return;
          }

          this.settlementsToRapporteurs = result.data;

          this.centerData.basicData.centerSettlement = this.settlementsToRapporteurs[0].settlement;
          this.territorialRapporteur = this.settlementsToRapporteurs[0].territorialRapporteur;

          this.geocode(callback);
        });
    },

    centerSettlementChanged() {
      var item = this.settlementsToRapporteurs.find(x => {
        return (
          x.zipCode == this.validCenterZipCode &&
          x.settlement == this.centerData.basicData.centerSettlement
        );
      });

      if (item) {
        this.territorialRapporteur = item.territorialRapporteur;
      } else {
        this.territorialRapporteur = null;
      }

      this.geocode();
    },

    centerTypesComplexMajorChanged() {
      this.$nextTick(() => {
        if (this.centerTypesComplex.majorType == 0) {
          this.centerTypesComplex.isContinuousInsulinDelivery1 = false;
          this.centerData.basicData.centerTypes = [0];
        } else {
          this.centerTypesComplex.isGestational = false;
          this.centerTypesComplex.isContinuousInsulinDelivery0 = false;
          this.centerData.basicData.centerTypes = [1];
        }
      });
    },

    centerTypesComplexMinorChanged(minorType) {
      this.$nextTick(() => {
        if (this.centerData.basicData.centerTypes.indexOf(minorType) > -1) {
          this.centerData.basicData.centerTypes = this.centerData.basicData.centerTypes.filter(
            x => x !== minorType
          );
        } else {
          this.centerData.basicData.centerTypes.push(minorType);
        }
      });
    },

    geocode(callback) {
      this.$nextTick(() => {
        this.geocodeFailed = false;

        if (
          !this.validCenterZipCode ||
          !this.centerData.basicData.centerSettlement
        ) {
          this.showMap = false;
          this.removeMarker();
          if (callback) {
            callback();
          }
          return;
        }

        this.showMap = true;
        var address =
          this.validCenterZipCode +
          " " +
          this.centerData.basicData.centerSettlement;
        if (this.centerData.basicData.centerAddress) {
          address += ", " + this.centerData.basicData.centerAddress;
        }

        if (!this.geocoder) {
          this.geocoder = new google.maps.Geocoder();
        }

        if (!this.map) {
          this.map = new google.maps.Map(document.getElementById("map"), {
            zoom: 15
          });
        }

        this.removeMarker();

        this.geocoder.geocode(
          {
            address: address
          },
          (results, status) => {
            if (status == "OK") {
              this.map.setCenter(results[0].geometry.location);
              this.marker = new google.maps.Marker({
                map: this.map,
                position: results[0].geometry.location
              });

              this.centerData.basicData.centerLatitude = +results[0].geometry.location.lat();
              this.centerData.basicData.centerLongitude = +results[0].geometry.location.lng();
            } else {
              this.showMap = false;
              this.geocodeFailed =
                "Geocode was not successful for the following reason: " +
                status;
              console.warn(this.geocodeFailed);
            }

            if (callback) {
              callback();
            }
          }
        );
      });
    },

    removeMarker() {
      if (this.map && this.marker) {
        this.marker.setMap(null);
        this.marker = null;
      }
    },

    viewLeaderProfile() {
      utils.viewLeaderProfile(
        this.options.urls.viewLeaderUrl,
        this.additionalData.leader.memberRightId,
        this.additionalData.leader.fullName
      );
    },

    onSave() {
      this.submitted = true;
      if (this.hasValidationErrors) {
        this.alertOnContinue();
        return;
      }

      creationRequestService
        .submit(this.urls.basicDataUrl, this.centerData.basicData)
        .then(response => {
          if (response.status == 200) {
            this.setCenterDataPristine();

            if (this.isNew) {
              let redirectUrl =
                response.data.redirectUrl.trim() + "#/additional";
              this.$nextTick(() => {
                window.location.replace(redirectUrl);
              });

              return;
            }

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

          console.log(this.serverValidationErrors);
        });
    },

    onBeforeUnload(e) {
      utils.preventPageLeaveIfFormIsDirty(e, this.centerDataIsDirty);
    },

    alertOnContinue() {
      utils.alertModal(
        "A folytatás előtt kérjük adja meg a helyes alapadatokat."
      );
    },

    onSummaryValidationErrors(e) {
      this.submitted = true;
      this.serverValidationErrors = e.errors;
      this.alertOnContinue();
    }
  },
  computed: {
    validCenterZipCode() {
      if (!/^[1-9]\d{3}$/.test(this.centerData.basicData.centerZipCode)) {
        return;
      }

      return +this.centerData.basicData.centerZipCode;
    },

    hasValidationErrors() {
      return !!(
        Object.keys(this.validationErrors).length &&
        this.validationErrors.constructor === Object
      );
    },

    validationErrors() {
      var result = {};

      if (!this.centerData.basicData.centerName) {
        result.centerName = 1;
      }

      if (!/^\d{4}$/.test(this.centerData.basicData.centerZipCode)) {
        result.centerZipCode = 1;
      }

      if (!this.centerData.basicData.centerSettlement) {
        result.centerSettlement = 1;
      }

      if (!this.centerData.basicData.centerAddress) {
        result.centerAddress = 1;
      }

      if (!/^\d{7,11}$/.test(this.centerData.basicData.phone)) {
        result.phone = 1;
      }

      if (
        this.centerData.basicData.fax &&
        !/^\d{7,11}$/.test(this.centerData.basicData.fax)
      ) {
        result.fax = 1;
      }

      if (
        !/^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(
          this.centerData.basicData.email
        )
      ) {
        result.email = 1;
      }

      if (
        this.centerData.basicData.web &&
        !/^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/gm.test(
          this.centerData.basicData.web
        )
      ) {
        result.web = 1;
      }

      if (!this.centerData.basicData.centerTypes.length) {
        result.centerTypes = 1;
      }

      return result;
    },

    isNew() {
      return this.options.isNew;
    },

    options() {
      return this.$store.state.options;
    },

    urls() {
      return this.options.urls;
    },

    centerTypes() {
      return this.$store.getters.centerTypes;
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
    if (this.hasValidationErrors) {
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
      handler(newVal, oldVal) {
        this.centerDataIsDirty = true;
      },
      deep: true
    }
  },
  props: ["googleMapLoaded"]
};
</script>
