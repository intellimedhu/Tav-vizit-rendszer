<template>
    <div class="row center-profile-summary">
        <div class="col">
            <div class="card card-center-profile-editor card-center-profile-editor-primary mt-3">
                <div class="card-header">
                    <h4 class="m-0">
                        Alapadatok
                    </h4>
                </div>
                <div class="card-body">
                    <div class="summary-group">
                        <h5 class="summary-group-title">Szakellátóhely vezető</h5>
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
                              <tr>
                                <td>{{ centerData.leader.fullName }}</td>
                                <td>
                                  <a v-if="centerData.leader.email" :href="`mailto:${centerData.leader.email}`">
                                    {{ centerData.leader.email }}
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

                    <div class="summary-group">
                        <h5 class="summary-group-title">Szakellátóhely adatok</h5>
                        <div v-if="centerData.basicData">
                            <div class="row">
                                <div class="col-md-5 text-md-right">Szakellátóhely neve</div>
                                <div class="col-md-7">
                                    {{ centerData.basicData.centerName }}
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Szakellátóhely címe</div>
                                <div class="col-md-7">
                                    {{ centerData.basicData.centerZipCode }}
                                    {{ centerData.basicData.centerSettlement }},
                                    {{ centerData.basicData.centerAddress }}
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Telefonszám</div>
                                <div class="col-md-7">{{ centerData.basicData.phone }}</div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Fax</div>
                                <div class="col-md-7">{{ centerData.basicData.fax }}</div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Email</div>
                                <div class="col-md-7">{{ centerData.basicData.email }}</div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Weboldal</div>
                                <div class="col-md-7">
                                    <a :href="centerData.basicData.web" target="_blank">
                                        {{ centerData.basicData.web }}
                                    </a>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-5 text-md-right">Profil</div>
                                <div class="col-md-7">
                                    <ul class="list-unstyled m-0">
                                        <li v-for="type in centerData.basicData.centerTypes" v-bind:key="type">
                                            {{ centerTypes[type] }}
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
                <div class="card-header">
                    <h4 class="m-0">
                        Kiegészítő adatok
                    </h4>
                </div>
                <div class="card-body">
                    <div class="summary-group">
                        <h5 class="summary-group-title">Szakellátóhely típusa</h5>
                        <div class="row" v-if="centerData.additional">
                            <div class="col-md-5 text-md-right">Önálló diabétesz szakrendelés</div>
                            <div class="col-md-7">{{ centerData.additional.vocationalClinic | yesNo }}</div>

                            <div class="col-md-5 text-md-right">Más szakrendelés része</div>
                            <div class="col-md-7">{{ centerData.additional.partOfOtherVocationalClinic | yesNo }}</div>
                            
                            <div class="col-md-5 text-md-right" v-if="centerData.additional.partOfOtherVocationalClinic">Szakrendelések</div>
                            <div class="col-md-7" v-if="centerData.additional.partOfOtherVocationalClinic">
                                {{ centerData.additional.otherVocationalClinic }}
                            </div>
                        </div>
                    </div>

                    <div class="summary-group" v-if="centerData.additional">
                        <h5 class="summary-group-title">NEAK adatok</h5>
                        <div class="row mb-3">
                            <div class="col-md-5 text-md-right">Szerződéssel rendelkezik</div>
                            <div class="col-md-7">{{ centerData.additional.neakContract | yesNo }}</div>
                        </div>
                        <div class="row" v-for="(neak, neakIndex) in centerData.additional.neak" v-bind:key="neakIndex"
                            :class="{'mb-3': neakIndex < centerData.additional.neak.length - 1}">
                            
                            <div class="col-md-5 text-md-right">Elsődleges</div>
                            <div class="col-md-7">{{ neak.primary | yesNo }}</div>

                            <div class="col-md-5 text-md-right">
                                Szerződésben foglalt heti óraszám
                            </div>
                            <div class="col-md-7">
                                {{ neak.numberOfHours }}
                            </div>

                            <div class="col-md-5 text-md-right">
                                Ebből cukorbetegekre fordított óraszám
                            </div>
                            <div class="col-md-7">
                                {{ neak.numberOfHoursDiabetes }}
                            </div>

                            <div class="col-md-5 text-md-right">
                                Munkahelyi kód
                            </div>
                            <div class="col-md-7">
                                {{ neak.workplaceCode }}
                            </div>
                        </div>
                    </div>
                    
                    <div class="summary-group">
                        <h5 class="summary-group-title">Kormányhivatal (ÁNTSZ) engedély</h5>
                        <div class="row" v-if="centerData.additional && centerData.additional.antsz">
                            <div class="col-md-5 text-md-right">Száma</div>
                            <div class="col-md-7">{{ centerData.additional.antsz.number }}</div>

                            <div class="col-md-5 text-md-right">Kelte</div>
                            <div class="col-md-7">{{ antszDate }}</div>

                            <div class="col-md-5 text-md-right">Azonosító kód</div>
                            <div class="col-md-7">{{ centerData.additional.antsz.id }}</div>
                        </div>
                    </div>

                    <div class="summary-group">
                        <h5 class="summary-group-title">Rendelési idő</h5>
                        <ul v-if="centerData.additional && officeHoursOrdered.length" class="list-unstyled">
                            <li v-for="officeHour in officeHoursOrdered" v-bind:key="officeHour.day" class="row">
                                <div class="col-md-5 text-md-right">{{ dayNames[officeHour.day] }}</div>
                                <div class="col-md-7">
                                    <ul class="list-unstyled" v-if="officeHour.hours.length">
                                        <li v-for="(hour, hourIndex) in officeHour.hours" v-bind:key="hourIndex">
                                            {{ hour.timeFrom | shortTime }} - {{ hour.timeTo | shortTime }}
                                        </li>
                                    </ul>
                                    <span v-else>-</span>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

            <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
                <div class="card-header">
                    <h4 class="m-0">
                        Tárgyi eszközök
                    </h4>
                </div>
                <div class="card-body">
                    <div class="summary-group">
                        <h5 class="summary-group-title">A szakellátóhely a következő eszközökkel rendelkezik (darab)</h5>
                        <div class="row" v-for="item in tools" v-bind:key="item.id">
                            <div class="col-md-5 text-md-right">{{ item.caption }}</div>
                            <div class="col-md-7">{{ item.value }}</div>
                        </div>
                    </div>

                    <div class="summary-group">
                        <h5 class="summary-group-title">Biztosított-e a hosszú távú anyagcsere-vezetés ellenőrzését és a szövődmények korai felismerését szolgáló laboratóriumi háttér?</h5>
                        <div class="row" v-for="item in laboratory" v-bind:key="item.id">
                            <div class="col-md-5 text-md-right">{{ item.caption }}</div>
                            <div class="col-md-7">{{ item.value }}</div>
                        </div>
                    </div>

                    <div class="summary-group">
                        <h5 class="summary-group-title">Háttér</h5>
                        <div class="row" v-if="centerData.equipments">
                            <div class="col-md-5 text-md-right">Biztosított-e a diabetológiában jártas konzíliumi háttér?</div>
                            <div class="col-md-7">{{ centerData.equipments.backgroundConcilium | yesNo }}</div>

                            <div class="col-md-5 text-md-right">Biztosított-e a diabetológiában jártas fekvőbeteg háttér?</div>
                            <div class="col-md-7">{{ centerData.equipments.backgroundInpatient | yesNo }}</div>
                        </div>
                    </div>
                </div>
            </div>

            <div v-if="!options.isNew" class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
                <div class="card-header">
                    <h4 class="m-0">
                        Munkatársak
                    </h4>
                </div>
                <div class="card-body">
                    <h5>Szakellátóhely munkatársai, betöltött munkakörök</h5>
                    <div class="table-responsive table-striped" v-if="activeColleagues.length">                    
                        <table class="table table-hover table-sm">
                            <thead>
                                <tr>
                                    <th>Név</th>
                                    <th>Email</th>
                                    <th>Munkakör</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="colleague in activeColleagues" v-bind:key="colleague.id">
                                    <td>{{ colleague.fullName }}</td>
                                    <td>{{ colleague.email }}</td>
                                    <td>{{ occupations[colleague.occupation] }}</td>
                                    <td class="text-right">
                                      <button type="button"
                                              class="btn btn-sm btn-info"
                                              title="Munkatárs adatlapja"
                                              @click="viewColleagueProfile(colleague)">
                                        <i class="fas fa-info"></i>
                                      </button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <p class="alert alert-warning border-warning" v-if="pendingColleaguesCount || removedColleaguesCount">
                        <span v-if="pendingColleaguesCount">
                            Függőben lévő munkatársak: <strong>{{ pendingColleaguesCount }} fő</strong>
                            <br />
                        </span>
                        <span v-if="removedColleaguesCount">
                            Törölt jelentkezések: <strong>{{ removedColleaguesCount }} fő</strong>
                        </span>
                    </p>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import creationRequestService from "../services/creation-request-service";
import utils from "../utils";
import moment from "moment";
import { EventBus } from "../event-bus";
import axios from "axios";

export default {
  data() {
    return {
      centerData: {
        basicData: {
          centerAddress: null,
          centerName: null,
          centerSettlement: null,
          centerTypes: [],
          centerZipCode: null,
          diabeticPregnancyCare: null,
          email: null,
          fax: null,
          insulinPumpTreatment: null,
          phone: null,
          web: null
        },
        leader: {
          fullName: null,
          memberRightId: null
        },
        renewal: null,
        additional: {
          antsz: {
            date: null,
            id: null,
            number: null
          },
          neak: [],
          neakContract: null,
          officeHours: [],
          otherVocationalClinic: null,
          partOfOtherVocationalClinic: null,
          vocationalClinic: null
        },
        equipments: {
          backgroundConcilium: null,
          backgroundInpatient: null,
          laboratory: [],
          tools: []
        },
        colleagues: {
          colleagues: []
        }
      }
    };
  },
  methods: {
    initialize() {
      creationRequestService
        .getCenterProfile(this.options.urls.summaryUrl)
        .then(response => {
          Object.assign(this.centerData, response.data.viewModel);
        })
        .catch(e => {
          utils.loadingFailed();
        });
    },

    getEquipmentValue(equipment, foundItem) {
      if (!foundItem) {
        return "-";
      }

      if (equipment.type == 0) {
        return foundItem.value;
      }

      if (equipment.type == 1) {
        return foundItem.value ? "Igen" : "Nem";
      }

      return value;
    },

    getEquipments(equipments, source) {
      return equipments.map(item => {
        let inListTool = this.centerData.equipments[source].find(
          x => x.id == item.id
        );

        return Object.assign({}, item, {
          selected: !!inListTool,
          value: this.getEquipmentValue(item, inListTool)
        });
      });
    },

    viewColleagueProfile(colleague) {
      utils.viewColleagueProfile(this.options.urls.viewColleagueUrl, colleague);
    },

    viewLeaderProfile() {
      utils.viewLeaderProfile(
        this.options.urls.viewLeaderUrl,
        this.centerData.leader.memberRightId,
        this.centerData.leader.fullName
      );
    },

    onSave(e) {
      if (e.saveAndExit) {
        EventBus.$emit("saved");
        return;
      }

      let confirmMessage = "Biztos benne, hogy elküldi az adatlapot?";
      if (this.pendingColleaguesCount) {
        confirmMessage =
          'A szakellátóhelynél függőben lévő munkatársak vannak.<br />Az adatlap elküldésével a függőben lévő \
          munkatársak <br /> <strong class="text-uppercase">nem kerülnek be</strong> a szakellátóhely munkatársai közé, \
          az elküldött meghívást <strong class="text-uppercase">nem tudják elfogadni</strong>.<br/><br/>' +
          confirmMessage;
      }

      confirmationModal({
        title: '<i class="fas fa-exclamation-triangle"></i> Figyelem!',
        message: confirmMessage,
        okClass: "btn btn-primary",
        callback: confirmed => {
          if (confirmed) {
            axios
              .get(this.options.urls.submitUrl)
              .then(() => {
                setTimeout(() => {
                  alertModal({
                    message: "A bejelentés sikeres volt.",
                    callback: () => {
                      EventBus.$emit("saved");
                    }
                  });
                }, 300);
              })
              .catch(e => {
                console.warn(e);
                if (e.response.status == 409) {
                  setTimeout(() => {
                    utils.alertModal(
                      "Hiba történt a társasági rendszerrel történő kapcsolat során."
                    );
                  }, 300);
                } else if (
                  e.response.status == 400 &&
                  e.response.data &&
                  e.response.data.step
                ) {
                  this.$router.replace(
                    e.response.data.step == "BasicData" ? "/" : "/additional"
                  );
                  this.$nextTick(() => {
                    EventBus.$emit("summary-validation-errors", {
                      errors: e.response.data.errors
                    });
                  });
                } else {
                  setTimeout(() => {
                    utils.alertModal("A bejelentés nem sikerült");
                  }, 300);
                }
              });
          }
        }
      });
    }
  },
  computed: {
    options() {
      return this.$store.state.options;
    },

    days() {
      return this.$store.getters.days;
    },

    dayNames() {
      return this.$store.getters.dayNames;
    },

    officeHoursOrdered() {
      return utils.mapOfficeHours(
        this.days,
        this.centerData.additional.officeHours
      );
    },

    tools() {
      return this.getEquipments(this.$store.getters.tools, "tools");
    },

    laboratory() {
      return this.getEquipments(this.$store.getters.laboratory, "laboratory");
    },

    colleagues() {
      if (this.centerData.colleagues) {
        return utils.mapColleagues(this.centerData.colleagues.colleagues);
      }

      return [];
    },

    colleagueStatusZones() {
      return this.$store.getters.colleagueStatusZones;
    },

    activeColleagues() {
      return this.colleagues.filter(
        colleague =>
          this.colleagueStatusZones["active"].indexOf(
            colleague.statusHistory[0].status
          ) > -1
      );
    },

    pendingColleaguesCount() {
      return this.colleagues.filter(
        colleague =>
          this.colleagueStatusZones["pending"].indexOf(
            colleague.statusHistory[0].status
          ) > -1
      ).length;
    },

    removedColleaguesCount() {
      return this.colleagues.filter(
        colleague =>
          this.colleagueStatusZones["removed"].indexOf(
            colleague.statusHistory[0].status
          ) > -1
      ).length;
    },

    occupations() {
      return this.$store.getters.occupations;
    },

    centerTypes() {
      return this.$store.getters.centerTypes;
    },

    antszDate() {
      if (
        !this.centerData.additional ||
        !this.centerData.additional.antsz.date
      ) {
        return;
      }

      return moment(new Date(this.centerData.additional.antsz.date)).format(
        "YYYY. MM. DD."
      );
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("save", this.onSave);
  },
  beforeDestroy() {
    EventBus.$off("save", this.onSave);
  },
  filters: {
    yesNo(input) {
      return input ? "Igen" : "Nem";
    },
    shortTime(input) {
      // 00:00:00
      if (!/^(\d{2}:){2}\d{2}$/.test(input)) {
        return;
      }

      var split = input.split(":");

      return split[0] + ":" + split[1];
    }
  }
};
</script>
