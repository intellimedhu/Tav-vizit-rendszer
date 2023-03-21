<template>
    <div>
        <b-modal ref="mealtime-editor-modal"
                 size="lg"
                  scrollable
                 :no-close-on-esc="true"
                 :no-close-on-backdrop="true">
            <template slot="modal-header">
              <div class="row w-100">
                <div class="col">
                  <h4 class="mt-1 mb-1 d-inline-block">
                      <i class="icon-time"></i>
                      {{ lang['modalTitle'] }}
                  </h4>
                  <button type="button"
                          class="btn btn-sm btn-outline-secondary float-right "
                          aria-label="Close"
                          @click="closeModal()">
                      <i class="icon-remove"></i>
                  </button>
                </div>
              </div>
            </template>
            <template slot="default">
              <div class="p-3" id="mealtime-editor-modal-body">
                  <div :class="{'was-validated': submitted}">
                        <div class="row form-group">
                            <label class="col-form-label offset-sm-2 col-sm-4" for="mealtime-edit-breakfast">
                                {{breakfastLabel}}<sup class="text-danger">*</sup>                                
                            </label>
                            <div class="col-sm-4">
                                <input type="time"
                                       id="mealtime-edit-breakfast"
                                       class="form-control"
                                       v-model="mealtime.breakfast"
                                       :max="breakfastTimeMax"
                                       required="required" />
                                <div class="invalid-feedback" v-if="validationErrors['breakfast']">{{lang['timeRequired']}}</div>
                                <div class="invalid-feedback" v-if="validationErrors['breakfast.invalid']">{{lang['timeInvalid']}}</div>
                            </div>
                        </div>

                        <div class="row form-group">
                            <label class="col-form-label offset-sm-2 col-sm-4" for="mealtime-edit-elevenses">
                                {{elevensesLabel}}
                            </label>
                            <div class="col-sm-4">
                                <input type="time"
                                       id="mealtime-edit-elevenses"
                                       class="form-control"
                                       v-model="mealtime.elevenses"
                                       :min="elevensesTimeMin"
                                       :max="elevensesTimeMax" />
                                <div class="invalid-feedback" v-if="validationErrors['elevenses.invalid']">{{lang['timeInvalid']}}</div>
                            </div>
                        </div>

                        <div class="row form-group">
                            <label class="col-form-label offset-sm-2 col-sm-4" for="mealtime-edit-lunch">
                                {{lunchLabel}}<sup class="text-danger">*</sup>
                            </label>
                            <div class="col-sm-4">
                                <input type="time"
                                       id="mealtime-edit-lunch"
                                       class="form-control"
                                       v-model="mealtime.lunch"
                                       :min="lunchTimeMin"
                                       :max="lunchTimeMax"
                                       required="required" />
                                <div class="invalid-feedback" v-if="validationErrors['lunch']">{{lang['timeRequired']}}</div>
                                <div class="invalid-feedback" v-if="validationErrors['lunch.invalid']">{{lang['timeInvalid']}}</div>
                            </div>
                        </div>

                        <div class="row form-group">
                            <label class="col-form-label offset-sm-2 col-sm-4" for="mealtime-edit-afternoonsnack">
                                {{afternoonsnackLabel}}
                            </label>
                            <div class="col-sm-4">
                                <input type="time"
                                       id="mealtime-edit-afternoonsnack"
                                       class="form-control"
                                       v-model="mealtime.afternoonsnack"
                                       :min="afternoonsnackTimeMin"
                                       :max="afternoonsnackTimeMax" />
                                <div class="invalid-feedback" v-if="validationErrors['afternoonsnack.invalid']">{{lang['timeInvalid']}}</div>
                            </div>
                        </div>

                        <div class="row">
                            <label class="col-form-label offset-sm-2 col-sm-4" for="mealtime-edit-dinner">
                                {{dinnerLabel}}<sup class="text-danger">*</sup>
                            </label>
                            <div class="col-sm-4">
                                <input type="time"
                                       id="mealtime-edit-dinner"
                                       class="form-control"
                                       v-model="mealtime.dinner"
                                       :min="dinnerTimeMin" 
                                       required="required" />
                                <div class="invalid-feedback" v-if="validationErrors['dinner']">{{lang['timeRequired']}}</div>
                                <div class="invalid-feedback" v-if="validationErrors['dinner.invalid']">{{lang['timeInvalid']}}</div>
                            </div>
                        </div>

                        <div class="row mt-3" v-show="submitted && validationErrors['timeConflict']">
                          <div class="col-sm-8 offset-sm-2">
                            <p class="alert alert-danger m-0">
                              <small>{{ lang["timeConflict"] }}</small>
                            </p>
                          </div>
                        </div>
                      </div>
              </div>
            </template>
            <template slot="modal-footer">
                <button type="button" class="btn btn-sm btn-primary" @click="save()" :disabled="submitted && hasValidationErrors">
                    <i class="icon-ok"></i>
                    {{okText}}
                </button>
            </template>
        </b-modal>
    </div>
</template>
<script>
import moment from "moment";
import { EventBus } from "../services/event-bus";
import utils from "../services/utils";

export default {
  data() {
    return {
      mealtime: {
        breakfast: null,
        elevenses: null,
        lunch: null,
        afternoonsnack: null,
        dinner: null
      },
      receivedType: null,
      submitted: false,
      modalShown: false
    };
  },
  methods: {
    onShow(e) {
      this.submitted = false;
      this.receivedType = e.type;
      Object.assign(
        this.mealtime,
        this.$store.state.mainModule.patient.settings.mealtime
      );
      this.$refs["mealtime-editor-modal"].show();
    },
    closeModal() {
      this.$refs["mealtime-editor-modal"].hide();
    },
    onModalShown() {
      if (this.receivedType) {
        document.getElementById(`mealtime-edit-${this.receivedType}`).focus();
      }

      this.modalShown = true;
    },
    onModalHidden() {
      this.modalShown = false;
    },
    save() {
      this.submitted = true;
      if (this.hasValidationErrors) {
        return;
      }

      this.$store.commit("updatePatientMealtime", this.mealtime);
      this.closeModal();
    },
    setCustomValidity(type, error = "") {
      if (this.modalShown) {
        document
          .getElementById(`mealtime-edit-${type}`)
          .setCustomValidity(error);
      }
    },
    getMaxTime(nextTime) {
      if (!nextTime) {
        return;
      }

      return moment(utils.convertTimeString(nextTime))
        .subtract(2.5, "hours")
        .format("HH:mm");
    },
    getMinTime(prevTime) {
      if (!prevTime) {
        return;
      }

      return moment(utils.convertTimeString(prevTime))
        .add(2.5, "hours")
        .format("HH:mm");
    }
  },
  computed: {
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings
        .currentLanguage;
    },
    hasValidationErrors() {
      return Object.keys(this.validationErrors).length > 0;
    },
    validationErrors() {
      let result = {};
      let givenTimes = [];

      this.setCustomValidity("breakfast");
      this.setCustomValidity("elevenses");
      this.setCustomValidity("lunch");
      this.setCustomValidity("afternoonsnack");
      this.setCustomValidity("dinner");

      if (!this.mealtime.breakfast) {
        result["breakfast"] = 1;
        this.setCustomValidity("breakfast", "required");
      } else if (!utils.validateTime(this.mealtime.breakfast)) {
        result["breakfast.invlid"] = 1;
        this.setCustomValidity("breakfast", "invalid");
      } else {
        givenTimes.push({
          type: "breakfast",
          time: this.mealtime.breakfast
        });
      }

      if (this.mealtime.elevenses) {
        if (!utils.validateTime(this.mealtime.elevenses)) {
          result["elevenses.invlid"] = 1;
          this.setCustomValidity("elevenses", "invalid");
        } else {
          givenTimes.push({
            type: "elevenses",
            time: this.mealtime.elevenses
          });
        }
      }

      if (!this.mealtime.lunch) {
        result["lunch"] = 1;
        this.setCustomValidity("lunch", "required");
      } else if (!utils.validateTime(this.mealtime.lunch)) {
        result["lunch.invlid"] = 1;
        this.setCustomValidity("lunch", "invalid");
      } else {
        givenTimes.push({
          type: "lunch",
          time: this.mealtime.lunch
        });
      }

      if (this.mealtime.afternoonsnack) {
        if (!utils.validateTime(this.mealtime.afternoonsnack)) {
          result["afternoonsnack.invalid"] = 1;
          this.setCustomValidity("afternoonsnack", "invalid");
        } else {
          givenTimes.push({
            type: "afternoonsnack",
            time: this.mealtime.afternoonsnack
          });
        }
      }

      if (!this.mealtime.dinner) {
        result["dinner"] = 1;
        this.setCustomValidity("dinner", "required");
      } else if (!utils.validateTime(this.mealtime.dinner)) {
        result["dinner.invlid"] = 1;
        this.setCustomValidity("dinner", "invlid");
      } else {
        givenTimes.push({
          type: "dinner",
          time: this.mealtime.dinner
        });
      }

      for (let i = 0; i < givenTimes.length - 1; i++) {
        var time1 = utils.convertTimeString(givenTimes[i].time);
        var time2 = utils.convertTimeString(givenTimes[i + 1].time);

        if (
          moment(time1)
            .add(2.5, "hours")
            .isAfter(time2)
        ) {
          this.setCustomValidity(givenTimes[i].type, "conflict");
          this.setCustomValidity(givenTimes[i + 1].type, "conflict");

          result[`${givenTimes[i].type}.conflict`] = 1;
          result[`${givenTimes[i + 1].type}.conflict`] = 1;

          if (moment(time1).isBefore(time2)) {
            result["timeConflict"] = 1;
          }
        }
      }

      return result;
    },
    breakfastTimeMax() {
      return this.getMaxTime(
        this.mealtime.elevenses ||
          this.mealtime.lunch ||
          this.mealtime.afternoonsnack ||
          this.mealtime.dinner
      );
    },
    elevensesTimeMax() {
      return this.getMaxTime(
        this.mealtime.lunch ||
          this.mealtime.afternoonsnack ||
          this.mealtime.dinner
      );
    },
    lunchTimeMax() {
      return this.getMaxTime(
        this.mealtime.afternoonsnack || this.mealtime.dinner
      );
    },
    afternoonsnackTimeMax() {
      return this.getMaxTime(this.mealtime.dinner);
    },
    elevensesTimeMin() {
      return this.getMinTime(this.mealtime.breakfast);
    },
    lunchTimeMin() {
      return this.getMinTime(
        this.mealtime.elevenses || this.mealtime.breakfast
      );
    },
    afternoonsnackTimeMin() {
      return this.getMinTime(
        this.mealtime.lunch ||
          this.mealtime.elevenses ||
          this.mealtime.breakfast
      );
    },
    dinnerTimeMin() {
      return this.getMinTime(
        this.mealtime.afternoonsnack ||
          this.mealtime.lunch ||
          this.mealtime.elevenses ||
          this.mealtime.breakfast
      );
    },
    lang() {
      return this.$i18n.t("mealtimeEditor", this.currentLanguage);
    },
    okText() {
      return this.$i18n.t("ok", this.currentLanguage);
    },
    breakfastLabel() {
      return this.$i18n.t("breakfast", this.currentLanguage);
    },
    elevensesLabel() {
      return this.$i18n.t("elevenses", this.currentLanguage);
    },
    lunchLabel() {
      return this.$i18n.t("lunch", this.currentLanguage);
    },
    afternoonsnackLabel() {
      return this.$i18n.t("afternoonsnack", this.currentLanguage);
    },
    dinnerLabel() {
      return this.$i18n.t("dinner", this.currentLanguage);
    }
  },
  mounted() {
    EventBus.$on("open-mealtime-editor", this.onShow);

    // Cannot unsubscribe from these event.
    this.$root.$on("bv::modal::shown", this.onModalShown);
    this.$root.$on("bv::modal::hidden", this.onModalHidden);
  },
  beforeDestroy() {
    EventBus.$off("open-mealtime-editor", this.onShow);
  }
};
</script>

