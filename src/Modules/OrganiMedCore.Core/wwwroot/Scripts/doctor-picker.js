new Vue({
    el: '#doctor-picker-app',
    data: {
        organizationUnitId: DoctorPickerInitData.OrganizationUnitId,
        doctorApiUrlBase: DoctorPickerInitData.DoctorApiUrlBase,
        doctors: []
    },
    created: function () {
        this.getDoctors();
    },
    methods: {
        getDoctors: function () {
            if (this.organizationUnitId) {
                axios
                    .get(this.doctorApiUrlBase, {
                        params: {
                            organizationUnitId: this.organizationUnitId
                        }
                    })
                    .then((response) => this.doctors = response.data)
                    .catch((error) => this.doctors = []);
            }
        }
    },
    watch: {
        "organizationUnitId": function () {
            this.getDoctors();
        }
    }
})
