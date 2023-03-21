<template>
    <div class="row">
        <div class="col-12">
            <h3>
                <i class="fa fa-user-plus"></i> 
                <span v-if="invitedColleague">Meghívó szerkesztése</span>
                <span v-else>Munkatárs meghívása</span>
            </h3>
            <hr>
        </div>

        <div class="col">
            <div class="row form-group" v-show="!invitedColleague">
                <div class="col-12">
                    <div class="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="is-member"
                               name="isMember"
                               v-model="colleague.isMember"
                               :value="true"
                               @change="isMemberChanged(true)"
                               class="custom-control-input"
                               :disabled="invitedColleague">
                        <label class="custom-control-label" for="is-member">Tag keresése</label>
                    </div>
                    <div class="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="not-member"
                               name="isMember"
                               v-model="colleague.isMember"
                               @change="isMemberChanged(false)"
                               :value="false"
                               class="custom-control-input"
                               :disabled="invitedColleague">
                        <label class="custom-control-label" for="not-member">Nem tag megadása</label>
                    </div>
                </div>
            </div>

            <div class="row">
              <div class="col-12">
                <p class="alert alert-warning border-warning">
                  <span v-if="colleague.isMember">
                    Kérjük, először ezen a felületen keressen rá a tag nevére és ellenőrizze, hogy a meghívni kívánt személy MDT tagsággal rendelkezik-e.
                  </span>
                  <br />
                  <strong>
                    A Magyar Diabetes Társaság Elnökségének állásfoglalása alapján a Cukorbeteg Szakellátóhelyek MDT akkreditációjának
                    alapfeltétele a szakellátó helyen dolgozó összes kolléga MDT tagsága.
                    </strong>
                    <br />
                    Amennyiben nem MDT
                    <span v-if="colleague.isMember">tagot</span> 
                    <span v-else>felhasználót</span>
                    hív meg, a személynek először az MDT tagjának kell lennie, belépést követően tudja a meghívást elfogadni.                    
                </p>
              </div>

                <div class="col-12" v-show="colleague.isMember">
                    <div class="row form-group">
                        <label class="col-12 col-form-label" for="member-name">Tag neve</label>
                        <div class="col-12" v-show="!memberSelected">
                            <type-ahead :src="`${urls.searchMemberUrl}?name=:keyword`"
                                        :get-response="onGetResponse"
                                        :highlighting="onHighlighting"
                                        :on-hit="onHit"
                                        :delay-time="400"
                                        v-model="memberSearch"></type-ahead>
                        </div>

                        <div class="col-12" v-show="memberSelected">
                            <div class="input-group">
                                <input type="text"
                                    readonly
                                    class="form-control"
                                    :value="colleague.fullName" />
                                <div class="input-group-append">
                                    <button class="btn btn-danger" type="button" @click="clearMember()">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row form-group">
                        <label for="member-email" class="col-12 col-form-label">Email</label>
                        <div class="col-12 mt-2">
                            <select class="form-control"
                                    :disabled="!memberSelected"
                                    v-model="colleague.email"
                                    id="member-email">
                                <option :value="null" disabled>Kérem, válasszon!</option>
                                <option v-for="email in colleague.emails" v-bind:key="email" :value="email">{{email}}</option>
                            </select>
                        </div>
                    </div>
                </div>

                <div class="col-12" v-show="!colleague.isMember">
                    <div class="row form-group">
                        <label class="col-12 col-form-label" for="non-member-name-prefix">Prefix</label>
                        <div class="col-12">
                            <input type="text" id="non-member-name-prefix" class="form-control" v-model="colleague.prefix" />
                        </div>
                    </div>

                    <div class="row form-group">
                        <label class="col-12 col-form-label" for="non-member-name-last-name">Vezetéknév</label>
                        <div class="col-12">
                            <input type="text" id="non-member-name-last-name" class="form-control" v-model="colleague.lastName" />
                        </div>
                    </div>

                    <div class="row form-group">
                        <label class="col-12 col-form-label" for="non-member-name-first-name">Keresztnév</label>
                        <div class="col-12">
                            <input type="text" id="non-member-name-first-name" class="form-control" v-model="colleague.firstName" />
                        </div>
                    </div>

                    <div class="row form-group">
                        <label class="col-12 col-form-label" for="non-member-email">Email</label>
                        <div class="col-12">
                            <input type="email" id="non-member-email" class="form-control" v-model="colleague.email" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="row form-group">
                <label class="col-12 col-form-label" for="non-member-occupation">Munkakör</label>
                <div class="col-12">
                    <select id="non-member-occupation" class="form-control" v-model="colleague.occupation">
                        <option :value="null" selected disabled>Kérem, válasszon!</option>
                        <option v-for="(occupation, index) in occupations" v-bind:key="index" :value="index">
                            {{ occupation }}
                        </option>
                    </select>
                </div>
            </div>
        </div>

        <div class="col-12" v-if="hasError">
            <p class="alert alert-danger">{{ errorMessage }}</p>
        </div>

        <div class="col-12" v-if="submitted && validationErrors.length">
            <ul class="alert alert-danger list-unstyled">
                <li v-for="error in validationErrors" v-bind:key="error">{{ error }}</li>
            </ul>
        </div>

        <div class="col-12 text-right">
            <hr>
            <button type="button" class="btn btn-primary" @click="invite()">
                <i class="fas fa-envelope"></i>
                Meghívó elküldése
            </button>

            <button type="button" class="btn btn-outline-secondary" @click="cancel()">
                Mégsem
            </button>
        </div>
    </div>
</template>

<script>
import axios from "axios";
import { EventBus } from "../event-bus";
import utils from "../utils";
import typeAhead from "vue2-typeahead";

export default {
  components: {
    typeAhead
  },
  data() {
    return {
      memberSearch: null,
      noResults: false,
      memberSelected: false,
      colleague: this.resetMember(),
      hasError: false,
      errorMessage: null,
      submitted: false,
      validationErrors: []
    };
  },
  computed: {
    occupations() {
      return this.$store.getters.occupations;
    },
    urls() {
      return this.$store.state.options.urls;
    }
  },
  methods: {
    onGetResponse(response) {
      return response.data;
    },
    onHighlighting(item, vue) {
      let membership = item.hasMemberShip
        ? '<span class="text-success">MDT tag</span>'
        : '<span class="text-danger">Nem MDT tag</span>';
      let isMembershipFeePaid = "";
      if (item.hasMemberShip) {
        isMembershipFeePaid = "| ";
        if (item.isMembershipFeePaid) {
          isMembershipFeePaid +=
            '<span class="text-success">Tagdíj: Rendezett</span>';
        } else {
          isMembershipFeePaid +=
            '<span class="text-danger">Tagdíj: Nem rendezett</span>';
        }
      }

      return `<div style="border-bottom:1px solid lightgray">
          <strong>${item.fullName}</strong>
          <br>
          <small>
          ${item.emails[0] ? item.emails[0] + " | " : ""}
          Pecsétszám: ${item.stampNumber || "-"} |
          ${membership}
          ${isMembershipFeePaid}
          </small>
        </div>`;
    },
    onHit(member, vue) {
      Object.assign(this.colleague, member);

      this.memberSelected = true;
      this.memberSearch = null;

      this.setDefaultMemberEmail();
    },
    resetMember() {
      return {
        isMember: true,
        id: null,
        email: null,
        emails: [],
        prefix: null,
        firstName: null,
        lastName: null,
        memberRightId: null,
        occupation: null
      };
    },
    clearMember() {
      this.memberSelected = false;
      this.colleague = this.resetMember();
    },
    isMemberChanged(isMember) {
      this.clearMember();
      this.colleague.isMember = isMember;
      this.hasError = false;
      this.errorMessage = null;
      this.validationErrors = [];
    },
    setDefaultMemberEmail() {
      if (this.colleague.emails.length) {
        this.colleague.email = this.colleague.emails[0];
      }
    },
    cancel() {
      EventBus.$emit("add-colleague-form-closing");
    },
    invite() {
      this.submitted = true;
      if (!this.validate()) {
        return;
      }

      EventBus.$emit("add-colleague-form-invite", {
        colleague: Object.assign({}, this.colleague)
      });
    },
    validate() {
      this.validationErrors = [];

      if (this.colleague.isMember) {
        if (!this.memberSelected) {
          this.validationErrors.push("A tag kiválasztása kötelező");
        }
      } else {
        if (!this.colleague.lastName) {
          this.validationErrors.push("A vezetéknév megadása kötelező");
        }

        if (!this.colleague.firstName) {
          this.validationErrors.push("A keresztnév megadása kötelező");
        }
      }

      if (!this.colleague.email) {
        this.validationErrors.push("Az email megadása kötelező");
      }

      if (
        this.colleague.occupation === null ||
        this.colleague.occupation === undefined
      ) {
        this.validationErrors.push("A munkakör kiválasztása kötelező");
      }

      return this.validationErrors.length == 0;
    },
    onActivate() {
      this.colleague = this.resetMember();
      this.hasError = false;
      this.errorMessage = null;
      this.submitted = false;

      if (this.invitedColleague) {
        this.colleague.email = this.invitedColleague.email;
        this.colleague.id = this.invitedColleague.id;
        this.colleague.occupation = this.invitedColleague.occupation;

        if (this.invitedColleague.memberRightId) {
          this.memberSelected = true;

          this.colleague.isMember = true;

          axios
            .get(this.urls.colleagueAsMember, {
              params: {
                id: this.invitedColleague.memberRightId
              }
            })
            .then(response => {
              Object.assign(this.colleague, response.data);
              Object.assign(this.colleague, {
                fullName: utils.getFullName(response.data)
              });
            })
            .catch(e => {
              console.warn(e);
              utils.alertModal("Az adatok mentése nem sikerült");
            });
        } else {
          this.colleague.prefix = this.invitedColleague.prefix;
          this.colleague.lastName = this.invitedColleague.lastName;
          this.colleague.firstName = this.invitedColleague.firstName;
          this.colleague.isMember = false;
        }
      } else {
        this.memberSelected = false;
      }
    },
    handleError(error) {
      this.hasError = true;
      this.errorMessage = error;
    }
  },
  mounted() {
    EventBus.$on("add-colleague-form-activate", this.onActivate);
    EventBus.$on("add-colleague-error", this.handleError);
  },
  beforeDestroy() {
    EventBus.$off("add-colleague-form-activate", this.onActivate);
    EventBus.$off("add-colleague-error", this.handleError);
  },
  props: ["invitedColleague"]
};
</script>
