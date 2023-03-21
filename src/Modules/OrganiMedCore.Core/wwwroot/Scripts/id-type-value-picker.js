Vue.component('id-type-value-picker', {
    data: function () {
        return {
            selectedIdentifierType: {},
            identifierTypes: this.identifierTypesInit,
            selectedIdentifierTypeValue: this.selectedIdentifierTypeValueInit,
            identifierValue: this.identifierValueInit
        };
    },
    created: function () {
        for (var i = 0; i < this.identifierTypes.length; i++) {
            if (this.identifierTypes[i].value == this.selectedIdentifierTypeValue) {
                this.selectedIdentifierType = this.identifierTypes[i];
            }
        }
    },
    methods: {
        selectIdentifierType: function (identifierType) {
            this.selectedIdentifierType = identifierType;
        }
    },
    watch: {
        "selectedIdentifierType.value": function () {
            if (this.selectedIdentifierType.value == 0) {
                this.identifierValue = "";
            }
        }
    },
    computed: {
        nextButtonText: function () {
            return this.selectedIdentifierType.value == 0 ? "Tovább" : "Ellenőrzés";
        }
    },
    props: {
        identifierTypesInit: {
            type: Array,
            default: () => ([]),
        },
        selectedIdentifierTypeValueInit: {
            type: Number,
            default: () => (0),
        },
        identifierValueInit: {
            type: String,
            default: () => (''),
        }
    }
});