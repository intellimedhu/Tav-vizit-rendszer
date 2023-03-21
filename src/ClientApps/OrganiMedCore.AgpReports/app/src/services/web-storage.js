import moment from 'moment';

let webStorage = {
    constants: {
        globalSettings: 'AGPReports.GlobalSettings',
        patientsSettings: 'AGPReports.PatientsSettings'
    },

    // When updating properties here, update them in global-settings-dialog as well!
    getGlobalSettings() {
        let settings = {
            viewMode: 'OfficialAGP', // 'OfficialAGP'|'DCMView'
            averageModeDaysLimit: 365,
            currentLanguage: 'hu',
            unit: 'mmol/l', // 'mg/dl'|'mmol/l'
            dgpChartType: 'scatter', // 'scatter'|'line'
            midlineType: 'median', // 'avg'|'median',
            curveType: 'none', // 'function'|'none'
            displayAverageLines: true, // true|false, display 90, 75, 25 and 10 percentage lines or not
            diaryType: 'hourly', // 'mealtime'|'hourly'
            preMealTimeInMinutes: 30,
            postMealTimeInMinutes: 120,
            displayInsulinInAGP: false, // true|false
            displayNonMeasurableValues: true,
            //minimumMeasurementsOfHbA1c: 100,
            dgpDialogMode: 'normal', // 'normal'|'sd'
            printOptions: {
                charts: true,
                diary: false
            },
            colorBasal: null,
            colorBolus: null,
            colorMix: null,
            timeScaleType: 'default' // 'default'|'glucose'|'insulin'|'steps'
        };

        if (typeof Storage) {
            let settingsFromStorage = localStorage.getItem(webStorage.constants.globalSettings);
            if (settingsFromStorage) {
                Object.assign(settings, JSON.parse(settingsFromStorage));
            }

            settingsFromStorage = null;
        }

        moment.locale(settings.currentLanguage);

        return settings;
    },

    updateGlobalSettings(globalSettings) {
        moment.locale(globalSettings.currentLanguage);

        if (typeof Storage) {
            localStorage.setItem(webStorage.constants.globalSettings, JSON.stringify(globalSettings));
        }
    },

    getPatientSettings(id) {
        let settings = {
            mealtime: {
                breakfast: "07:00",
                elevenses: null,
                lunch: "12:00",
                afternoonsnack: null,
                dinner: "18:00"
            }
        };

        if (typeof Storage) {
            let currentPatientData = webStorage.getPatientsSettings()[id];
            if (currentPatientData) {
                Object.assign(settings, currentPatientData);
            }
        }

        return settings;
    },

    setPatientSettings(id, data) {
        if (!(typeof Storage)) {
            return;
        }

        let patientsSettings = webStorage.getPatientsSettings();
        if (!patientsSettings[id]) {
            patientsSettings[id] = {};
        }

        Object.assign(patientsSettings[id], data);

        localStorage.setItem(webStorage.constants.patientsSettings, JSON.stringify(patientsSettings));
    },

    getPatientsSettings() {
        let result = {};
        let patientsSettingsAsString = localStorage.getItem(webStorage.constants.patientsSettings);
        if (patientsSettingsAsString) {
            Object.assign(result, JSON.parse(patientsSettingsAsString));
        }

        return result;
    }
};

export default webStorage;