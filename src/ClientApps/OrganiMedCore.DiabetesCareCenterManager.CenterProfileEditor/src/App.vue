<template>
  <div id="app" class="center-profile-editor-app">
    <div class="row">
      <div class="col-12">
        <div class="row center-profile-editor-steps m-0 p-0">
          <div class="col-12 center-profile-editor-step-item" 
              :class="{
                'active bg-gradient-success': activeTab == 1, 'visited': activeTab > 1,
                'col-md-3' : isNew, 'col-md-20pc' : !isNew
              }">
            <router-link to="/" class="center-profile-editor-route-link">
              <span class="text">
                <span class="center-profile-editor-step-number">1.</span>
                Alapadatok
              </span>
            </router-link>
          </div>
          <div class="col-12 center-profile-editor-step-item"
              :class="{
                'active bg-gradient-success': activeTab == 2, 'visited': activeTab > 2,
                'col-md-3' : isNew, 'col-md-20pc' : !isNew
              }">
            <router-link to="/additional" class="center-profile-editor-route-link">
              <span class="text">
                <span class="center-profile-editor-step-number">2.</span>
                Kiegészítő adatok
              </span>
            </router-link>
          </div>
          <div class="col-12 center-profile-editor-step-item"
              :class="{
                'active bg-gradient-success': activeTab == 3, 'visited': activeTab > 3,
                'col-md-3' : isNew, 'col-md-20pc' : !isNew
              }">
            <router-link to="/equipments" class="center-profile-editor-route-link">
              <span class="text">
                <span class="center-profile-editor-step-number">3.</span>
                Tárgyi eszközök
              </span>
            </router-link>
          </div>
          <div v-if="!isNew" class="col-12 col-md-20pc center-profile-editor-step-item"
              :class="{'active bg-gradient-success': activeTab == 4, 'visited': activeTab > 4}">
            <router-link to="/colleagues" class="center-profile-editor-route-link">
              <span class="text">
                <span class="center-profile-editor-step-number">4.</span>
                Munkatársak
              </span>
            </router-link>
          </div>
          <div class="col-12 center-profile-editor-step-item"
              :class="{
                'active bg-gradient-success': activeTab == 5, 
                'col-md-3' : isNew, 'col-md' : !isNew
              }">
            <router-link to="/summary" class="center-profile-editor-route-link">
              <span class="text">
                <span class="center-profile-editor-step-number">{{ isNew ? 4 : 5 }}.</span>
                Összegzés
              </span>
            </router-link>
          </div>
        </div>
      </div>

      <accreditation-status v-if="!isNew && initialized" />

      <div class="col-12" v-if="initialized">
        <router-view :google-map-loaded="googleMapLoaded"></router-view>
      </div>

      <div class="col-12 text-right mt-5">
        <button type="button" class="btn btn-lg btn-secondary mb-2 mb-md-0 float-md-left" v-show="activeTab > 1" @click="goBack()">
          <i class="fas fa-chevron-left"></i>
          Vissza
        </button>

        <a :href="options.urls.returnUrl" class="btn btn-lg btn-secondary mb-2 mb-md-0 float-md-left" v-show="activeTab == 1">
          <i class="fas fa-chevron-left"></i>
          Vissza
        </a>

        <button type="button"
                class="btn btn-lg mb-2 mb-md-0"
                :class="{'btn-primary': activeTab > 1, 'btn-outline-primary': activeTab == 1}"
                :disabled="activeTab == 1"
                @click="save(true)">
          <i class="fas fa-clock"></i>
          Mentés, folytatás később
        </button>

        <button type="button" class="btn btn-lg btn-success mb-2 mb-md-0" @click="save(false)"
                v-if="activeTab < 5">
          <i class="fas fa-save"></i>
          Mentés és tovább
        </button>

        <button type="button" class="btn btn-lg btn-success mb-2 mb-md-0" @click="save(false)"
                v-if="activeTab == 5 && isNew">
          <i class="fas fa-save"></i>
          <span>Szakellátóhely iránti kérelem benyújtása</span>
        </button>

        <div class="text-right mt-2" v-if="activeTab == 5 && !isNew">
          <button type="button" class="btn btn-lg btn-success mb-2 mb-md-0" @click="save(false)">
            <i class="fas fa-save"></i>
            <span>Szakellátóhely adatlapjának elküldése területi referensi véleményezésre</span>
          </button>
        </div>
      </div>
    </div>

    <person-profile-modal></person-profile-modal>
  </div>
</template>

<script>
import axios from "axios";
import { EventBus } from "./event-bus";
import personProfileModal from "./components/person-profile-modal.vue";
import accreditationStatus from "./components/accreditation-status.vue";
import infoBlockService from "./services/info-block-service";
import utils from './utils';

export default {
  name: "app",
  components: {
    personProfileModal,
    accreditationStatus
  },
  data() {
    return {
      activeTab: 1,
      saveAndExit: null,
      googleMapLoaded: false,
      initialized: null
    };
  },
  methods: {
    routeChanged(newPath) {
      switch (newPath) {
        case "/":
          this.activeTab = 1;
          break;

        case "/additional":
          this.activeTab = 2;
          break;

        case "/equipments":
          this.activeTab = 3;
          break;

        case "/colleagues":
          if (!this.isNew) {
            this.activeTab = 4;
          } else {
            this.$router.replace("/summary");
          }
          break;

        case "/summary":
          this.activeTab = 5;
          break;
      }

      infoBlockService.hideAllExpect(this.activeTab);
    },

    goBack() {
      if (this.activeTab == 1) {
        return;
      }

      switch (this.activeTab) {
        case 3:
          this.$router.replace("/additional");
          break;

        case 4:
          this.$router.replace("/equipments");
          break;

        case 5:
          this.$router.replace(this.isNew ? "/equipments" : "/colleagues");
          break;

        case 2:
        default:
          this.$router.replace("/");
          break;
      }
    },

    save(saveAndExit) {
      this.saveAndExit = saveAndExit;
      EventBus.$emit("save", {
        saveAndExit: saveAndExit
      });
    },

    onSaved() {
      if (this.saveAndExit) {
        window.location.href = this.options.urls.returnUrl;
        return;
      }

      switch (this.activeTab) {
        case 1:
          this.$router.replace("/additional");
          break;
        case 2:
          this.$router.replace("/equipments");
          break;
        case 3:
          this.$router.replace(this.isNew ? "/summary" : "/colleagues");
          break;
        case 4:
          this.$router.replace("/summary");
          break;
        case 5:
          window.location.href = this.options.urls.returnUrl;
          break;
      }

      EventBus.$emit("get-accreditation-status");
    },

    initialize() {
      axios
        .get(this.options.urls.initUrl)
        .then(response => {
          this.$store.commit("initAdditionalData", response.data);
          this.initialized = true;
        })
        .catch(e => console.log(JSON.parse(JSON.stringify(e))));
    },

    failedToSave() {
      utils.alertModal("Az adatok mentése nem sikerült");
    },

    onGoogleMapsLoaded() {
      this.googleMapLoaded = true;
    }
  },
  computed: {
    isNew() {
      return this.options.isNew;
    },
    options() {
      return this.$store.state.options;
    }
  },
  watch: {
    $route(to) {
      this.routeChanged(to.path);
    }
  },
  mounted() {
    this.initialized = false;
    this.initialize();
    infoBlockService.initialize();
    infoBlockService.hideAll();

    this.routeChanged(this.$route.path);

    EventBus.$on("saved", this.onSaved);
    EventBus.$on("failed-to-save", this.failedToSave);
    EventBus.$on("google-maps-loaded", this.onGoogleMapsLoaded);
  },
  beforeDestroy() {
    EventBus.$off("saved", this.onSaved);
    EventBus.$off("failed-to-save", this.failedToSave);
    EventBus.$off("google-maps-loaded", this.onGoogleMapsLoaded);
  }
};
</script>