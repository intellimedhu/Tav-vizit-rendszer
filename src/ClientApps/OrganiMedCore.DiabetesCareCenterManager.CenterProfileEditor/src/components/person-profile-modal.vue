<template>
    <div>
        <b-modal :ref="modalId"
                 :hide-header="true"
                 :hide-footer="true"
                 :no-close-on-backdrop="true">
            <div class="row">
                <div class="col-12">
                    <h4>{{personProfile.fullName}} adatlapja</h4>
                    <hr>
                </div>
                <div class="col-12">
                  <div class="card card-center-profile-editor card-center-profile-editor-primary"
                       v-if="personProfile.qualifications.length || personProfile.graduationIssuedBy || personProfile.otherQualification">
                    <div class="card-header">
                      <h4 class="mb-0">Képesítések</h4>
                    </div>
                    <div class="card-body">
                      <ul class="list-unstyled mb-0" v-if="personProfile.qualifications.length">
                          <li v-for="(q, index) in personProfile.qualifications" v-bind:key="index">
                              <h5 class="m-0">{{q.qualification}}</h5>
                              <span class="d-block" v-if="q.number">Száma: {{q.number}}</span>
                              <span class="d-block" v-if="q.year">Év: {{q.year}}</span>
                              <span class="d-block" v-if="qualificationStateCaptions[q.state]">Állapot: {{qualificationStateCaptions[q.state]}}</span>
                              <hr v-if="index < personProfile.qualifications.length - 1">
                          </li>
                      </ul>

                      <div v-if="personProfile.graduationIssuedBy">
                          <hr>
                          <h5>Érettségi</h5>
                          <ul class="list-unstyled mb-0" v-if="personProfile.graduationIssuedBy">
                              <li>{{personProfile.graduationIssuedBy}}<span v-if="personProfile.graduationYear">, {{personProfile.graduationYear}}</span></li>
                          </ul>
                      </div>

                      <div v-if="personProfile.otherQualification">
                          <hr>
                          <h5>Egyéb</h5>
                          <ul class="list-unstyled mb-0" v-if="personProfile.otherQualification">
                              <li>{{personProfile.otherQualification}}</li>
                          </ul>
                      </div>
                    </div>
                  </div>

                  <div class="card card-center-profile-editor card-center-profile-editor-primary mt-3">
                    <div class="card-header">
                      <h4 class="mb-0">Személyes adatok</h4>
                    </div>
                    <div class="card-body">
                      <div>     
                        <h5>Telefonszám</h5>
                        <ul class="list-unstyled mb-0">
                          <li>
                            <a v-if="personProfile.privatePhone" :href="cleanPhone">{{personProfile.privatePhone}}</a>
                            <span v-else>-</span>
                          </li>
                        </ul>
                      </div>
                      <hr>
                      <div>
                        <h5>Tagsági információ</h5>
                        <ul>
                          <li class="row">
                            <span class="col-5">Tagsági információ:</span>
                            <span class="col text-success" v-if="personProfile.hasMembership">
                              <i class="fas fa-check"></i>
                              <strong>MDT Tag</strong>
                            </span>
                            <span class="col text-danger" v-else>
                              <i class="fas fa-ban"></i>
                              <strong>Nem MDT Tag</strong>
                            </span>
                          </li>
                          <li class="row" v-if="personProfile.hasMembership">
                            <span class="col-5">Tagdíj:</span>
                            <span class="col text-success" v-if="personProfile.isMembershipFeePaid">
                              <i class="fas fa-check"></i>
                              <strong>Rendezett</strong>
                            </span>
                            <span class="col text-danger" v-else>
                              <i class="fas fa-ban"></i>
                              <strong>Nem rendezett</strong>
                            </span>
                          </li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-12 text-right">
                    <hr>
                    <button type="button" class="btn btn-outline-secondary" @click="close()">
                        Bezár
                    </button>
                </div>
            </div>
        </b-modal>
    </div>
</template>

<script>
import { EventBus } from "../event-bus";

export default {
  data() {
    return {
      modalId: "colleague-profile-modal",
      personProfile: this.getEmptyProfile()
    };
  },
  methods: {
    getEmptyProfile() {
      return {
        fullName: null,
        qualifications: [],
        graduationIssuedBy: null,
        graduationYear: null,
        otherQualification: null,
        privatePhone: null,
        hasMembership: null,
        isMembershipFeePaid: null
      };
    },
    onShow(e) {
      Object.assign(this.personProfile, e.personProfile);
      this.$refs[this.modalId].show();
    },
    close() {
      this.$refs[this.modalId].hide();
    }
  },
  computed: {
    qualificationStateCaptions() {
      return this.$store.getters.qualificationStateCaptions;
    },
    cleanPhone() {
      if (this.personProfile.privatePhone) {
        return `tel:${this.personProfile.privatePhone.replace(/[^+\d]/g, "")}`;
      }
    }
  },
  mounted() {
    EventBus.$on("show-person-profile", this.onShow);

    this.$root.$on("bv::modal::hide", () => {
      this.personProfile = this.getEmptyProfile();
    });
  },
  beforeDestroy() {
    EventBus.$off("show-person-profile", this.onShow);
  }
};
</script>
