<template>
  <div id="colleague-application-editor">
    <div class="card mb-3 border-primary">
      <div class="card-header bg-primary text-light">
        <h4 class="m-0">Munkakör</h4>
      </div>
      <div class="card-body rounded" style="background-color:#eee">
        <div class="p-3 bg-white card card-center-profiles card-center-profiles-primary">
          <div class="row" :class="{'form-group': currentOccupation == 0}">
            <label class="col-md-3 col-form-label text-md-right" for="occupation">Munkakör</label>
            <div class="col-md-9 col-xl-8">
                <select id="occupation" class="form-control"
                        v-if="occupationCanBeChanged"
                        name="Occupation"
                        v-model="currentOccupation">
                    <option :value="null">Kérem, válasszon!</option>
                    <option v-for="o in occupations" v-bind:key="o.value" :value="o.value">{{o.text}}</option>
                </select>
                <input type="hidden" v-model="currentOccupation" v-if="!occupationCanBeChanged" name="Occupation" />
                <input type="text" :value="currentOccupationCaption" v-if="!occupationCanBeChanged" class="form-control" disabled />
            </div>
          </div>

          <div class="row" v-if="currentOccupation == 0">
              <label class="col-md-3 col-form-label text-md-right" for="mdt-license">Diabetológia licensz</label>
              <div class="col-md-9 col-xl-8">
                  <input type="text" id="mdt-license" class="form-control" v-model="viewModel.diabetLicenceNumber" />
              </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card mb-3 border-primary" v-if="currentOccupation != null && availableQualifications.length">
      <div class="card-header bg-primary text-light">
        <h4 class="m-0">Szakképesítések</h4>
      </div>
      <div class="card-body rounded" style="background-color:#eee">
              <div class="p-3 bg-white mb-3 card card-center-profiles card-center-profiles-primary" v-for="(q, index) in profileQualificationsForOccupation" v-bind:key="index">
                <div class="row form-group">
                  <label class="col-md-3 text-md-right col-form-label" :for="`qualification-${index}`">
                    <strong>Szakképesítés {{ index + 1 }}</strong>:
                  </label>
                  <div class="col-md-9 col-xl-8">
                    <select v-model="q.id" class="form-control" :id="`qualification-${index}`">
                      <option :value="null">Kérem, válasszon!</option>
                      <option v-for="x in getUnusedQualifications(q.id)" v-bind:key="x.id" :value="x.id">{{x.name}}</option>
                    </select>
                  </div>
                </div>

                <div class="row form-group">
                  <label class="col-md-3 text-md-right col-form-label" :for="`qualification-number-${index}`">
                    Száma:
                  </label>
                  <div class="col-md-5">
                    <input type="text"  :id="`qualification-number-${index}`" class="form-control" placeholder="száma" v-model="q.number" />
                  </div>
                </div>

                <div class="row form-group">
                  <label class="col-md-3 text-md-right col-form-label" :for="`qualification-year-${index}`">
                    Év:
                  </label>
                  <div class="col-md-5">
                    <input type="number" :id="`qualification-year-${index}`" class="form-control" :min="minYear" :max="maxYear" placeholder="év" v-model="q.year" />
                  </div>
                </div>

                <div class="row">
                  <label class="col-md-3 text-md-right col-form-label">
                    Státusz:
                  </label>
                  <div class="col-md-5">
                    <div class="custom-control custom-radio mt-2 mb-2" v-for="state in qualificationStates" v-bind:key="state.value">
                      <input type="radio" class="custom-control-input" v-model="q.state" :value="state.value"
                            :id="`qualification-states-${index}-${state.value}`" />
                      <label class="custom-control-label" :for="`qualification-states-${index}-${state.value}`">
                        {{ state.text }}
                      </label>
                    </div>
                  </div>
                  <div class="col-md-4 col-xl-3 text-center text-md-right align-self-end mt-3">
                    <button type="button" class="btn btn-danger" @click="removeQualification(q.uiId)">
                      <i class="fas fa-trash"></i>
                      Szakképesítés törlése
                    </button>
                  </div>
                </div>
              </div>

        <p class="text-muted text-center" v-if="!viewModel.qualifications.length">
          <strong>
            Nincsenek szakképesítések rögzítve.
          </strong>
        </p>

        <div class="row">
          <div class="col-12 text-center">
            <button type="button"
                    class="btn btn-primary"
                    @click="addQualification()"
                    :disabled="!canAddQualification">
              <i class="fas fa-plus"></i>
              Szakképesítés hozzáadása
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card mb-3 border-primary" v-if="graduationRequired || otherQualificationRequired">
      <div class="card-header bg-primary text-light">
        <h4 class="m-0">További képesítések </h4>
      </div>
      <div class="card-body rounded" style="background-color:#eee">
        <div class="p-3 bg-white card card-center-profiles card-center-profiles-primary">
        <div class="row" :class="{'form-group': otherQualificationRequired}" v-if="graduationRequired">
          <label class="col-form-label col-md-3 text-md-right">Érettségi</label>      
          <div class="col-md-7">
              <input type="text" class="form-control" placeholder="helye" v-model="viewModel.graduationIssuedBy" />
          </div>
          <div class="col-md-2">
              <input type="number" class="form-control" :min="minYear" :max="maxYear" placeholder="éve" v-model="viewModel.graduationYear" />
          </div>
        </div>

        <div class="row" v-if="otherQualificationRequired">
          <label class="col-form-label col-md-3 text-md-right">Egyéb képzettség</label>
          <div class="col-md-9">
            <input type="text" class="form-control"  v-model="viewModel.otherQualification" />
          </div>
        </div>
        </div>
      </div>
    </div>

    <div class="row form-group mt-3" v-if="submitted && hasValidationErrors">
      <div class="col">
        <ul class="alert alert-danger m-0 pl-5">
          <li v-if="validationErrors.occupation">A munkakör kiválasztása kötelező.</li>
          <!-- <li v-if="validationErrors.diabetLicenceNumber">A diabetológia licensz megadása kötelező.</li> -->
          <li v-if="validationErrors.qualificationId">Minden szakképesítés kiválasztása kötelező.</li>
          <li v-if="validationErrors.qualificationNumber">Minden szakképesítéshez kötelező megadni a számát.</li>
          <li v-if="validationErrors.qualificationYear">A szakképesítés éve helytelen.</li>
          <li v-if="validationErrors.qualificationState">Minden szakképesítéshez kötelező megadni a státuszt.</li>
          <li v-if="validationErrors.graduationIssuedBy">Az érettségi helye nincs megadva.</li>
          <li v-if="validationErrors.graduationYear">Az érettségi éve helytelen.</li>
          <li v-if="validationErrors.otherQualification">Az egyéb képzettség megadása kötelező.</li>
        </ul>
      </div>
    </div>

    <div class="row mt-3" v-if="submitted && serverErrors.length">
      <div class="col">
        <ul class="alert alert-danger m-0 pl-5">
          <li v-for="(error, index) in serverErrors" v-bind:key="index">{{error}}</li>
        </ul>
      </div>
    </div>

    <div class="text-center text-sm-right mt-3">
        <button type="button" class="btn btn-lg btn-success" @click="save()" :disabled="saveDisabled">
            <i class="fas fa-save"></i>
            Mentés és tovább
        </button>

        <a :href="backButtonUrl" class="btn btn-lg btn-secondary">
            <i class="fas fa-chevron-left"></i>
            Vissza
        </a>
    </div>
  </div>
</template>

<script>
export default {
  name: "app",
  data() {
    return {
      maxYear: new Date().getFullYear(),
      minYear: 1900,
      currentOccupation: null,
      graduationRequiredOccupations: [3, 4, 5, 6],
      settings: {
        qualificationsPerOccupations: []
      },
      occupations: [],
      qualifications: [],
      qualificationStates: [],
      viewModel: {
        diabetLicenceNumber: null,
        graduationIssuedBy: null,
        graduationYear: null,
        otherQualification: null,
        qualifications: []
      },
      nextQualificationId: 1,
      submitted: false,
      saveDisabled: false,
      serverErrors: []
    };
  },
  methods: {
    mapKeyValuePair(x) {
      return {
        value: x.key,
        text: x.value
      };
    },

    getUnusedQualifications(exceptId) {
      return this.availableQualifications.filter(q => {
        return (
          q.id == exceptId ||
          !this.viewModel.qualifications.some(x => x.id == q.id)
        );
      });
    },

    addQualification() {
      if (!this.canAddQualification) {
        return;
      }

      this.viewModel.qualifications.push({
        id: null,
        number: null,
        state: null,
        year: null,
        uiId: this.nextQualificationId++
      });
    },

    removeQualification(uiId) {
      confirmationModal({
        title: '<i class="fas fa-trash"></i> Törlés megerősítése',
        message: "Biztos, hogy törölni szeretné a szakképesítést?",
        okClass: "btn btn-outline-secondary",
        cancelClass: "btn btn-danger",
        callback: confirmed => {
          if (confirmed) {
            this.viewModel.qualifications = this.viewModel.qualifications.filter(
              x => x.uiId != uiId
            );
          }
        }
      });
    },

    save() {
      this.submitted = true;
      this.serverErrors = [];

      if (this.hasValidationErrors) {
        return;
      }

      // Keep qualifications only that are focused for the current occupation.
      let data = Object.assign({}, this.viewModel, {
        qualifications: this.profileQualificationsForOccupation
      });

      this.saveDisabled = true;

      this.$http
        .put(this.apiUrl, data, {
          params: {
            o: this.occupationCanBeChanged
              ? this.currentOccupation
              : this.occupation
          }
        })
        .then(() => {
          this.submitted = false;
          eventBus.$emit("colleague-application-editor");
        })
        .catch(e => {
          this.saveDisabled = false;
          if (e.response.status == 400) {
            Object.keys(e.response.data).forEach(key => {
              this.serverErrors.push(e.response.data[key].join());
            });
          } else {
            console.log(JSON.parse(JSON.stringify(e)));
            alertModal({ message: "Hiba történt a mentés során!" });
          }
        });
    }
  },
  computed: {
    currentOccupationCaption() {
      var occupation = this.occupations.find(
        x => x.value == this.currentOccupation
      );
      if (occupation) {
        return occupation.text;
      }
    },

    graduationRequired() {
      return (
        this.graduationRequiredOccupations.indexOf(this.currentOccupation) > -1
      );
    },

    otherQualificationRequired() {
      return this.currentOccupation == 3;
    },

    availableQualifications() {
      return this.qualifications.filter(x => {
        return this.settings.qualificationsPerOccupations.some(
          y =>
            y.qualificationId == x.id && y.occupation == this.currentOccupation
        );
      });
    },

    profileQualificationsForOccupation() {
      return this.viewModel.qualifications.filter(x => {
        return (
          this.availableQualifications.some(y => y.id == x.id) || x.id == null
        );
      });
    },

    canAddQualification() {
      return (
        this.profileQualificationsForOccupation.length <
        this.availableQualifications.length
      );
    },

    hasValidationErrors() {
      return !!(
        Object.keys(this.validationErrors).length &&
        this.validationErrors.constructor === Object
      );
    },

    validationErrors() {
      let errors = {};

      if (!this.occupations.find(x => x.value == this.currentOccupation)) {
        errors.occupation = 1;

        return errors;
      }

      if (this.profileQualificationsForOccupation.length) {
        if (this.profileQualificationsForOccupation.some(x => !x.id)) {
          errors.qualificationId = 1;
        }

        if (this.profileQualificationsForOccupation.some(x => !x.number)) {
          errors.qualificationNumber = 1;
        }

        if (
          this.profileQualificationsForOccupation.some(
            x => !x.year || x.year < this.minYear || x.year > this.maxYear
          )
        ) {
          errors.qualificationYear = 1;
        }

        if (
          this.profileQualificationsForOccupation.some(
            x => !this.qualificationStates.some(y => y.value === x.state)
          )
        ) {
          errors.qualificationState = 1;
        }
      }

      if (this.graduationRequired) {
        if (!this.viewModel.graduationIssuedBy) {
          errors.graduationIssuedBy = 1;
        }

        if (
          !this.viewModel.graduationYear ||
          this.viewModel.graduationYear < this.minYear ||
          this.viewModel.graduationYear > this.maxYear
        ) {
          errors.graduationYear = 1;
        }
      }

      if (
        this.otherQualificationRequired &&
        !this.viewModel.otherQualification
      ) {
        errors.otherQualification = 1;
      }

      return errors;
    }
  },
  mounted() {
    this.currentOccupation = this.occupation;

    this.$http
      .get(this.apiUrl)
      .then(response => {
        this.settings.qualificationsPerOccupations = response.data.settings.qualificationsPerOccupations
          .filter(x => x.isSelected)
          .map(x => {
            let result = Object.assign({}, x);
            delete result.isSelected;

            return result;
          });
        this.qualifications = response.data.qualifications;
        this.qualificationStates = response.data.qualificationStates.map(x =>
          this.mapKeyValuePair(x)
        );
        this.occupations = response.data.occupations.map(x =>
          this.mapKeyValuePair(x)
        );
        Object.assign(this.viewModel, response.data.viewModel);
        this.viewModel.qualifications.forEach(q => {
          q.uiId = this.nextQualificationId++;
        });
      })
      .catch(e => {
        console.log(JSON.parse(JSON.stringify(e)));
        alertModal({ message: "Hiba történt az adatok betöltése során!" });
      });
  },
  props: ["occupationCanBeChanged", "occupation", "apiUrl", "backButtonUrl"]
};
</script>
