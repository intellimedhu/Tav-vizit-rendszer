<template>
  <div id="app">
        <div class="card mb-3 border-primary">
          <div class="card-header bg-primary text-light">
            <h4 class="m-0">
              Személyes adatok
            </h4>
          </div>
          <div class="card-body">
            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Tagsági információ
              </label>

              <div class="col-md-9">
                <p class="text-success form-control m-0">
                  <i class="fas fa-check"></i>
                  <strong>MDT Tag</strong>
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Tagdíj
              </label>
              <div class="col-md-9">
                <p class="text-success form-control m-0">
                  <i class="fas fa-check"></i>
                  <strong>Rendezett</strong>
                </p>
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Név
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.fullName" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Leánykori név
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.maidenName" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Születési hely
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.bornPlace" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Születési idő
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.bornDate" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Email cím
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.emails[0]" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Pecsétszám
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.stampNumber" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Diabetológia licensz
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.diabetLicenceNumber" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Tudományos fokozat
              </label>
              <div class="col-md-9">
                <input type="text" class="form-control" readonly :value="dokiNetMember.scientificDegree" />
              </div>
            </div>
          </div>
        </div>

    <div class="row">
      <div class="col-12">
        <div class="card mb-3 border-primary">
          <div class="card-header bg-primary text-light">
            <h4 class="m-0">
              Szakképesítések
            </h4>
          </div>
          <div class="card-body rounded" style="background-color: #eee;">
            <div class="row">
              <div class="col-12">
                <div class="row" v-for="(profileQualification, index) in profileQualifications.qualifications" v-bind:key="index">
                  <div class="col-12">
                    <div class="p-3 bg-white mb-3 card card-center-profiles card-center-profiles-primary">
                      <div class="row form-group">
                        <label class="col-md-3 text-md-right col-form-label" :for="`profile-qualification-qualification-${index}`">
                          <strong>Szakképesítés {{ index + 1 }}</strong>:
                        </label>
                        <div class="col-md-9 col-xl-8">
                          <div :class="{'input-group' : occupationsPerQualifications[profileQualification.id]}">
                            <select class="form-control"
                                    :id="`profile-qualification-qualification-${index}`"
                                    v-model="profileQualification.id"
                                    name="DiabetesUserProfilePart.Qualifications[0].QualificationId">
                              <option :value="null">Kérem, válasszon!</option>
                              <option v-for="qualification in unusedQualificationsExcept(qualifications, profileQualifications.qualifications, profileQualification.id)"
                                      v-bind:key="qualification.id" :value="qualification.id">{{qualification.name}}</option>
                            </select>
                            <div class="input-group-append">
                              <b-button v-b-tooltip.hover
                                        :title="`Ezzel a szakképesítéssel az alábbi munkakörökre tud jelentkezni: ${occupationsPerQualifications[profileQualification.id]}`"
                                        v-if="occupationsPerQualifications[profileQualification.id]"
                                        tabindex="-1">
                                <i class="fas fa-info"></i>
                              </b-button>
                            </div>
                          </div>
                        </div>
                      </div>

                      <div class="row form-group">
                        <label class="col-md-3 text-md-right col-form-label" :for="`profile-qualification-number-${index}`">
                          Száma:
                        </label>
                        <div class="col-md-5">
                          <input type="text"
                                :id="`profile-qualification-number-${index}`"
                                class="form-control"
                                v-model="profileQualification.number" />
                        </div>
                      </div>

                      <div class="row form-group">
                        <label class="col-md-3 text-md-right col-form-label" :for="`profile-qualification-year-${index}`">
                          Év:
                        </label>
                        <div class="col-md-5">
                          <input type="number"
                                :id="`profile-qualification-year-${index}`"
                                class="form-control"
                                min="1900"
                                :max="maxYear"
                                v-model="profileQualification.year" />
                        </div>
                      </div>

                      <div class="row">
                        <label class="col-md-3 text-md-right col-form-label">
                          Státusz:
                        </label>
                        <div class="col-md-5">
                          <div class="custom-control custom-radio mt-2 mb-2" v-for="state in qualificationStates" v-bind:key="state.value">
                              <input type="radio" class="custom-control-input" v-model="profileQualification.state" :value="state.value"
                                    :id="`qualification-states-${index}-${state.value}`" />
                              <label class="custom-control-label" :for="`qualification-states-${index}-${state.value}`">
                                  {{ state.text }}
                              </label>
                          </div>
                        </div>
                        <div class="col-md-4 col-xl-3 text-center text-md-right align-self-end mt-3">
                            <button type="button" class="btn btn-danger" @click="deleteQualification(index)">
                                <i class="fas fa-trash"></i>
                                Szakképesítés törlése
                            </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <p v-if="!profileQualifications.qualifications.length" class="text-muted text-center">
                  <strong>
                  Nincsenek szakképesítések rögzítve.
                  </strong>
                </p>

                <div class="text-center">
                  <button type="button" class="btn btn-primary" @click="addQualification()" :disabled="!this.anyUnusedQualifications">
                    <i class="fa fa-plus"></i>
                    Szakképesítés hozzáadása
                  </button>
                </div>
              </div>
            </div>

            <div class="row mt-3" v-if="submitted && hasQualificationErrors">
              <div class="col">
                <ul class="alert alert-danger m-0 pl-5">
                  <li v-if="validationErrors.qualificationId">Nem minden szakképesítés lett megadva.</li>
                  <li v-if="validationErrors.qualificationNumber">Minden szakképesítéshez kötelező megadni a számát.</li>
                  <li v-if="validationErrors.qualificationYear">A szakképesítés éve helytelen.</li>
                  <li v-if="validationErrors.qualificationState">Minden szakképesítéshez kötelező megadni a státuszt.</li>
                </ul>
              </div>
            </div>
          </div>
        </div>

        <div class="card mb-3 border-primary">
          <div class="card-header bg-primary text-light">
            <h4 class="m-0">
              További képesítések
            </h4>
          </div>
          <div class="card-body rounded" style="background-color:#eee">
            <div class="p-3 bg-white card card-center-profiles card-center-profiles-primary">
            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Érettségi:
              </label>
              <div class="col-md-7 col-xl-6">
                <input type="text" class="form-control" placeholder="helye" v-model="profileQualifications.graduationIssuedBy" />
              </div>
              <div class="col-md-2 mt-3 mt-md-0">
                <input type="number" class="form-control" min="1900" :max="maxYear" placeholder="éve" v-model="profileQualifications.graduationYear" />
              </div>
            </div>

            <div class="row form-group">
              <label class="col-form-label col-md-3 text-md-right">
                Egyéb képzettség:
              </label>
              <div class="col-md-9 col-xl-8">
                <input type="text" class="form-control"  v-model="profileQualifications.otherQualification" />
              </div>
            </div>

            <div class="row" v-if="submitted && hasGraduationErrors">
              <div class="col">
                <ul class="alert alert-danger m-0 pl-5">
                  <li v-if="validationErrors.graduationIssuedBy">Az érettségi helye nincs megadva.</li>
                  <li v-if="validationErrors.graduationYear1">Az érettségi éve nincs megadva.</li>
                  <li v-if="validationErrors.graduationYear2">Az érettségi éve helytelen.</li>
                </ul>
              </div>
            </div>
            </div>
          </div>
        </div>

        <div class="row mt-3" v-if="submitted && serverErrors.length">
              <div class="col">
                <ul class="alert alert-danger m-0 pl-5">
                  <li v-for="(error, index) in serverErrors" v-bind:key="index">{{error}}</li>
                </ul>
              </div>
            </div>

        <div class="row mt-3">
              <div class="col text-right">
                <button type="button" class="btn btn-lg btn-success" @click="save()">
                  <i class="fas fa-save"></i>
                  Mentés
                </button>

                <a :href="backButtonUrl" class="btn btn-lg btn-secondary">
                  <i class="fas fa-chevron-left"></i>
                  Vissza
                </a>
              </div>
            </div>
      </div>

      <!-- <div class="col-lg-5 col-xl-4 mt-3 mt-md-0">
        <div class="position-sticky" style="top:70px">
        <div class="card mb-3">
          <div class="card-header">
            <h5 class="m-0">Információ</h5>
          </div>
          <div class="card-body">
            <p>Szakellátóhelyre történő jelentkezés esetén az egyes munkakörökhöz az alábbi képesítések szükségesek:</p>
            <ul class="list-unstyled m-0">
              <li v-for="(occupation, i) in occupations" v-bind:key="occupation.value" :class="{'mb-3': i < occupations.length - 1}">
                <div v-if="qualificationsPerOccupations[occupation.value]">
                  <strong>{{ occupation.text }}</strong>
                  <ul>
                    <li v-for="(req, index) in qualificationsPerOccupations[occupation.value]" v-bind:key="index">
                      {{ req.name }}
                    </li>
                    <li v-if="graduationRequiredOccupations.indexOf(occupation.value) > -1">Érettségi</li>
                  </ul>
                </div>
              </li>
            </ul>
          </div>
        </div>
        </div>
      </div> -->
    </div>
    
  </div>
</template>

<script>
export default {
  name: "app",
  data() {
    return {
      dokiNetMember: {
        bornDate: null,
        bornPlace: null,
        diabetLicenceNumber: null,
        emails: [],
        firstName: null,
        fullName: null,
        lastName: null,
        maidenName: null,
        memberId: null,
        memberRightId: null,
        prefix: null,
        scientificDegree: null,
        stampNumber: null,
        userName: null,
        webId: null
      },
      maxYear: new Date().getFullYear(),
      qualificationStates: [],
      profileQualifications: {
        graduationIssuedBy: null,
        graduationYear: null,
        otherQualification: null,
        qualifications: []
      },
      settings: {
        qualificationsPerOccupations: []
      },
      graduationRequiredOccupations: [3, 4, 5, 6],
      occupations: [],
      qualifications: [],
      serverErrors: {},
      submitted: false,
      hasQualificationErrors: false,
      hasGraduationErrors: false
    };
  },
  methods: {
    addQualification() {
      if (!this.anyUnusedQualifications) {
        return;
      }

      this.profileQualifications.qualifications.push({
        id: null,
        year: null,
        number: null,
        state: null
      });
    },
    deleteQualification(index) {
      confirmationModal({
        title: '<i class="fas fa-trash"></i> Törlés megerősítése',
        message: "Biztos, hogy törölni szeretné a szakképesítést?",
        callback: confirmed => {
          if (confirmed) {
            this.profileQualifications.qualifications.splice(index, 1);
          }
        }
      });
    },
    unusedQualificationsExcept(qualifications, profileQualifications, id) {
      return qualifications
        .filter(
          q => !profileQualifications.some(x => x.id == q.id) || q.id == id
        )
        .sort((a, b) => {
          return a.name.localeCompare(b.name);
        });
    },
    save() {
      this.serverErrors = [];
      this.submitted = true;

      if (this.hasValidationErrors) {
        return;
      }

      this.$http
        .post(this.apiPostUrl, this.profileQualifications)
        .then(() => {
          this.submitted = false;

          let forwardUrl = this.forwardUrl;
          alertModal({
            message: "Sikeres mentés.",
            callback: () => {
              if (forwardUrl) {
                window.location.href = forwardUrl;
              }
            }
          });
        })
        .catch(e => {
          if (e.response.status == 400) {
            Object.keys(e.response.data).forEach(key => {
              this.serverErrors.push(e.response.data[key].join());
            });
          } else {
            console.log(JSON.parse(JSON.stringify(e)));
            alertModal({
              message: "Hiba történt a mentés során!"
            });
          }
        });
    },
    mapKeyValuePair(x) {
      return {
        value: x.key,
        text: x.value
      };
    }
  },
  computed: {
    anyUnusedQualifications() {
      return (
        this.profileQualifications.qualifications.length <
        this.qualifications.length
      );
    },
    qualificationsPerOccupations() {
      let result = {};

      this.occupations
        .map(occupation => {
          return Object.assign({}, occupation, {
            requirements: this.qualifications.filter(q =>
              this.settings.qualificationsPerOccupations.some(
                x =>
                  x.qualificationId == q.id &&
                  x.occupation == occupation.value &&
                  x.isSelected
              )
            )
          });
        })
        .forEach(x => {
          result[x.value] = x.requirements;
        });

      return result;
    },
    occupationsPerQualifications() {
      let result = {};

      this.qualifications
        .map(qualification => {
          return Object.assign({}, qualification, {
            occupations: this.occupations
              .filter(occupation =>
                this.settings.qualificationsPerOccupations.some(
                  x =>
                    x.qualificationId == qualification.id &&
                    x.occupation == occupation.value &&
                    x.isSelected
                )
              )
              .map(x => x.text)
              .join(", ")
          });
        })
        .filter(x => {
          return x.occupations;
        })
        .forEach(x => {
          result[x.id] = x.occupations;
        });

      return result;
    },
    validationErrors() {
      let errors = {};

      let currentYear = new Date().getFullYear();
      let yearMin = 1900;
      this.hasQualificationErrors = false;
      this.hasGraduationErrors = false;

      if (this.profileQualifications.qualifications.some(x => !x.id)) {
        errors.qualificationId = 1;
        this.hasQualificationErrors = true;
      }

      if (this.profileQualifications.qualifications.some(x => !x.number)) {
        errors.qualificationNumber = 1;
        this.hasQualificationErrors = true;
      }

      if (
        this.profileQualifications.qualifications.some(
          x => !x.year || x.year < yearMin || x.year > currentYear
        )
      ) {
        errors.qualificationYear = 1;
        this.hasQualificationErrors = true;
      }

      if (
        this.profileQualifications.qualifications.some(
          x => !this.qualificationStates.some(y => y.value == x.state)
        )
      ) {
        errors.qualificationState = 1;
        this.hasQualificationErrors = true;
      }

      if (
        !this.profileQualifications.graduationIssuedBy &&
        this.profileQualifications.graduationYear
      ) {
        errors.graduationYear1 = 1;
        this.hasGraduationErrors = true;
      }

      if (
        this.profileQualifications.graduationIssuedBy &&
        !this.profileQualifications.graduationYear
      ) {
        errors.graduationIssuedBy = 1;
        this.hasGraduationErrors = true;
      }

      if (
        this.profileQualifications.graduationYear &&
        (this.profileQualifications.graduationYear < yearMin ||
          this.profileQualifications.graduationYear > currentYear)
      ) {
        errors.graduationYear2 = 1;
        this.hasGraduationErrors = true;
      }

      return errors;
    },
    hasValidationErrors() {
      return !!(
        Object.keys(this.validationErrors).length &&
        this.validationErrors.constructor === Object
      );
    }
  },
  mounted() {
    this.$http.get(this.apiGetUrl).then(response => {
      Object.assign(this.dokiNetMember, response.data.dokiNetMember, {
        bornDate: response.data.dokiNetMember.bornDate
          ? new Date(response.data.dokiNetMember.bornDate).toLocaleDateString(
              "hu-HU"
            )
          : null,
        fullName: [
          response.data.dokiNetMember.prefix,
          response.data.dokiNetMember.lastName,
          response.data.dokiNetMember.firstName
        ]
          .filter(x => !!x)
          .join(" ")
      });
      this.settings.qualificationsPerOccupations =
        response.data.settings.qualificationsPerOccupations;
      this.qualifications = response.data.qualifications;
      this.qualificationStates = response.data.qualificationStates.map(x =>
        this.mapKeyValuePair(x)
      );
      this.occupations = response.data.occupations.map(x =>
        this.mapKeyValuePair(x)
      );
      Object.assign(this.profileQualifications, response.data.viewModel);
    });
  },
  props: ["apiGetUrl", "apiPostUrl", "forwardUrl", "backButtonUrl"]
};
</script>
