<template>
    <div class="row row-patient pb-4 align-items-center print-content-diagrams">
        <div class="col-12 col-md col-logo text-center text-md-left" v-if="!isDcmView">
            <div class="agp-logo"></div>
        </div>
        <div class="col-12 col-sm-8 col-md col-name align-self-end text-center text-sm-left">
            <strong>{{ lang['patientProfileNameLabel'] }}:</strong>
            {{ patientFullName }}
        </div>
        <div class="col-12 col-sm-4 col-button text-center text-sm-right" :class="{'col-md-3': !isDcmView, 'col-sm': isDcmView }">
            <a :href="patientProfileUrl" class="btn btn-sm btn-primary d-print-none">
            {{ lang['patientProfileLinkLabel'] }}
            </a>
        </div>
    </div>
</template>
<script>
export default {
  computed: {
    currentLanguage() {
      return this.$store.state.globalSettingsModule.globalSettings
        .currentLanguage;
    },
    lang() {
      return this.$i18n.t("patient", this.currentLanguage);
    },
    isDcmView() {
      return (
        this.$store.state.globalSettingsModule.globalSettings.viewMode ==
        "DCMView"
      );
    },
    patientProfileUrl() {
      return this.patientProfileRawUrl
        .replace("{patient_id}", this.$store.state.mainModule.patient.id)
        .replace("{patient_name}", this.$store.getters.getPatientName("hu"));
    },
    patientFullName() {
      return this.$store.getters.getPatientName(this.currentLanguage);
    }
  },
  props: ["patientProfileRawUrl"]
};
</script>
