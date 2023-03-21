<template>
    <div class="col-12 mt-3 d-none" id="accreditation-status-statusbar" v-show="showAccreditationStatus">
        <div class="alert m-0 row" :class="{
          'alert-success border-success' : accreditationStatus == 0,
          'alert-info border-info' : accreditationStatus == 1,
          'alert-warning border-warning' : accreditationStatus == 2 }">
          <div class="col text-left p-0">
            Várható akkreditációs státusz
          </div>
          <div class="col text-right p-0">
            <i class="fas fa-check text-success" v-if="accreditationStatus == 0"></i>
            <i class="fas fa-check text-primary" v-if="accreditationStatus == 1"></i>
            <i class="fas fa-minus-square text-warning" v-if="accreditationStatus == 2"></i>
            <strong>{{ accreditationStatusText }}</strong>
          </div>

          <div v-if="accreditationStatus > 0" class="col-12 p-0 mt-2">
            <b-button v-b-toggle.collapse-details :variant="accreditationStatus == 1 ? 'info' : 'warning'" size="sm" class="py-0 float-right">
              <span v-show="!detailsOpen">Miért? Kattintson ide! <i class="fas fa-angle-down"></i></span>
              <span v-show="detailsOpen">Részletek elrejtése <i class="fas fa-angle-up"></i></span>
            </b-button>

            <b-collapse id="collapse-details" v-model="detailsOpen" class="mt-5">
              <div class="row mb-3" v-if="accreditationStatusResult.mdtLicence">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Legalább egy orvos rendelkezik Diabetológia licensszel</strong>
                </div>
                <div class="col">
                  Nem
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.membership.length">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Az alábbi személyek nem tagjai az MDT-nek</strong>
                </div>
                <div class="col">
                  <ul class="list-unstyled m-0">
                    <li v-for="(name, i) in accreditationStatusResult.membership" :key="i">
                      {{ name }}
                    </li>
                  </ul>
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.membershipFee.length">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Az alábbi személyek tagdíja nem rendezett</strong>
                </div>
                <div class="col">
                  <ul class="list-unstyled m-0">
                    <li v-for="(name, i) in accreditationStatusResult.membershipFee" :key="i">
                      {{ name }}
                    </li>
                  </ul>
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.tools.length">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Hiányzó eszközök</strong>
                </div>
                <div class="col">
                  <ul class="list-unstyled m-0">
                    <li v-for="(tool, i) in missingTools" :key="i">
                      {{ tool }}
                    </li>
                  </ul>
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.laboratory.length">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Hiányzó laboratóriumi háttér eszközök</strong>
                </div>
                <div class="col">
                  <ul class="list-unstyled m-0">
                    <li v-for="(laboratory, i) in missingLaboratory" :key="i">
                      {{ laboratory }}
                    </li>
                  </ul>
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.backgroundConcilium">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Biztosított-e a diabetológiában jártas interdiszciplináris konzíliumi háttér?</strong>
                </div>
                <div class="col">
                  Nem
                </div>
              </div>

              <div class="row mb-3" v-if="accreditationStatusResult.backgroundInpatient">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Biztosított-e a diabetológiában jártas fekvőbeteg háttér?</strong>
                </div>
                <div class="col">
                  Nem
                </div>
              </div>

              <div class="row" v-if="accreditationStatusResult.personalConditions.length">
                <div class="col-12 col-lg-4 text-lg-right">
                  <strong>Személyi feltételek</strong>
                </div>
                <div class="col">
                  <ul class="list-unstyled m-0">
                    <li v-for="(item, i) in missingPersonalConditions" :key="i" :class="{'mb-3' : i < missingPersonalConditions.length - 1}">
                      <div>
                        Munkakör: {{ item.caption }}
                      </div>
                      <div v-if="item.headCount != null">
                        Minimum szükséges létszám: {{ item.requiredHeadcount }}/{{item.headCount}} fő
                      </div>
                      <div v-if="item.unqualifiedPeople.length">
                        Az alábbi személyeknek nincs rögzítve a munkakör betöltéséhez szükséges szakképesítés:
                        <div v-for="(personName, i) in item.unqualifiedPeople" :key="i">
                          {{ personName }}
                        </div>
                      </div>
                    </li>
                  </ul>
                </div>
              </div>
            </b-collapse>
          </div>
        </div>
      </div>
</template>
<script>
import axios from "axios";
import { EventBus } from "../event-bus";

export default {
  data() {
    return {
      accreditationStatusResult: {
        accreditationStatus: 1,
        backgroundConcilium: true,
        backgroundInpatient: true,
        laboratory: [],
        mdtLicence: null,
        membership: [],
        membershipFee: [],
        personalConditions: [],
        tools: []
      },
      detailsOpen: true
    };
  },
  computed: {
    accreditationStatus() {
      return this.accreditationStatusResult.accreditationStatus;
    },
    showAccreditationStatus() {
      return [0, 1, 2].indexOf(this.accreditationStatus) > -1;
    },
    accreditationStatuses() {
      return this.$store.getters.accreditationStatuses;
    },
    accreditationStatusText() {
      if (this.accreditationStatuses) {
        return this.accreditationStatuses[this.accreditationStatus];
      }
    },
    tools() {
      return this.$store.getters.tools;
    },
    missingTools() {
      return this.accreditationStatusResult.tools.map(toolId => {
        let tool = this.tools.find(x => x.id == toolId);
        if (tool) {
          return tool.caption;
        }
      });
    },
    laboratory() {
      return this.$store.getters.laboratory;
    },
    missingLaboratory() {
      return this.accreditationStatusResult.laboratory.map(laboratoryId => {
        let laboratory = this.laboratory.find(x => x.id == laboratoryId);
        if (laboratory) {
          return laboratory.caption;
        }
      });
    },
    occupations() {
      return this.$store.getters.occupations;
    },
    missingPersonalConditions() {
      return this.accreditationStatusResult.personalConditions.map(pc => {
        let occupation = this.occupations[pc.occupation];
        return Object.assign({}, pc, {
          caption: occupation
        });
      });
    }
  },
  methods: {
    initialize() {
      axios
        .get(this.$store.state.options.urls.getAccreditationStatusUrl)
        .then(response => {
          Object.assign(
            this.accreditationStatusResult,
            response.data.accreditationStatusResult
          );
          this.$nextTick(() => {
            document
              .getElementById("accreditation-status-statusbar")
              .classList.remove("d-none");
          });
        })
        .catch(e => console.log(JSON.parse(JSON.stringify(e))));
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("get-accreditation-status", this.initialize);
    EventBus.$on("green-zone-updated", this.initialize);
  },
  beforeDestroy() {
    EventBus.$off("get-accreditation-status", this.initialize);
    EventBus.$off("green-zone-updated", this.initialize);
  }
};
</script>
