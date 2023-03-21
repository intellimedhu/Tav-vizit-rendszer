<template>
    <div class="row">
        <div class="col">
          <p class="alert alert-info border-info mt-3 mb-0">
            <i class="fas fa-info-circle"></i>
              A <strong>vastagon szedett elemek</strong> az akkreditáció megszerzésének feltételei.
          </p>

          <equipment-pane :equipments="centerData.tools"
                          class="mt-5"
                          type="tools"
                          header-text="A szakellátóhely a következő eszközökkel rendelkezik (darab)" />

          <equipment-pane :equipments="centerData.laboratory"
                          class="mt-5"
                          type="laboratory"
                          header-text="Biztosított-e a hosszú távú anyagcsere-vezetés ellenőrzését és a szövődmények korai felismerését szolgáló laboratóriumi háttér?"  />

          <div class="card card-center-profile-editor card-center-profile-editor-primary mt-5">
                <div class="card-header">
                    <h4 class="m-0">
                        Háttér
                    </h4>
                </div>
                <div class="card-body">
                    <div class="row form-group">
                        <div class="col-12">
                            <div class="custom-control custom-switch">
                                <input type="checkbox"
                                    class="custom-control-input"
                                    id="background-concilium"
                                    v-model="centerData.backgroundConcilium" />
                                <label class="custom-control-label font-weight-bold" for="background-concilium">
                                    Biztosított-e a diabetológiában jártas interdiszciplináris konzíliumi háttér?
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-12">
                            <div class="custom-control custom-switch">
                                <input type="checkbox"
                                    class="custom-control-input"
                                    id="background-inpatient"
                                    v-model="centerData.backgroundInpatient" />
                                <label class="custom-control-label font-weight-bold" for="background-inpatient">
                                    Biztosított-e a diabetológiában jártas fekvőbeteg háttér?
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
          </div>
        </div>
    </div>
</template>

<script>
import creationRequestService from "../services/creation-request-service";
import equipmentPane from "./equipment-pane.vue";
import { EventBus } from "../event-bus";
import utils from "../utils";

export default {
  components: {
    equipmentPane
  },
  data() {
    return {
      centerData: {
        tools: [],
        laboratory: [],
        backgroundConcilium: null,
        backgroundInpatient: null
      },
      centerDataIsDirty: false
    };
  },
  methods: {
    setCenterDataPristine() {
      this.$nextTick(() => {
        this.centerDataIsDirty = false;
      });
    },

    initialize() {
      creationRequestService
        .getCenterProfile(this.urls.apiUrl, {
          s: "Equipments"
        })
        .then(response => {
          Object.assign(this.centerData, response.data.viewModel);
          this.mapEquipments(this.tools, "tools");
          this.mapEquipments(this.laboratory, "laboratory");

          this.setCenterDataPristine();
        })
        .catch(e => {
          console.warn(e);
          this.setCenterDataPristine();
          utils.loadingFailed();
        });
    },

    mapEquipments(equipments, type) {
      equipments.forEach(data => {
        let centerTool = this.centerData[type].find(t => t.id == data.id);
        if (!centerTool) {
          centerTool = {
            value: null
          };

          this.centerData[type].push(centerTool);
        }

        Object.assign(centerTool, data);
      });

      this.centerData[type] = this.centerData[type].filter(t => {
        return equipments.some(data => data.id == t.id);
      });

      this.centerData[type].sort((t1, t2) => (t1.order < t2.order ? -1 : 1));
    },

    onSave() {
      let data = Object.assign({}, this.centerData);
      data.tools = data.tools
        .filter(t => +t.value && +t.value > 0)
        .map(t => {
          return {
            id: t.id,
            value: +t.value
          };
        });

      data.laboratory = data.laboratory
        .filter(t => t.value)
        .map(t => {
          return {
            id: t.id,
            value: t.value
          };
        });

      creationRequestService
        .submit(this.urls.equipmentsUrl, data)
        .then(response => {
          if (response.status == 200) {
            this.setCenterDataPristine();
            this.$nextTick(() => {
              EventBus.$emit("saved");
            });
          } else {
            EventBus.$emit("failed-to-save");
          }
        });
    },

    onBeforeUnload(e) {
      utils.preventPageLeaveIfFormIsDirty(e, this.centerDataIsDirty);
    }
  },
  computed: {
    laboratory() {
      return this.$store.getters.laboratory;
    },
    tools() {
      return this.$store.getters.tools;
    },
    urls() {
      return this.$store.state.options.urls;
    }
  },
  mounted() {
    this.initialize();

    EventBus.$on("save", this.onSave);
    window.addEventListener("beforeunload", this.onBeforeUnload);
  },
  beforeDestroy() {
    EventBus.$off("save", this.onSave);
    window.removeEventListener("beforeunload", this.onBeforeUnload);
  },
  beforeRouteLeave(to, from, next) {
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
  }
};
</script>
