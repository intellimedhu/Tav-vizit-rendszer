import utils from '../services/utils';
import webStorage from '../services/web-storage';

const mainModule = {
    state: {
        range: {
            allowedMinDate: null,
            allowedMaxDate: null,
            selectedMinDate: null,
            selectedMaxDate: null
        },
        diarySelectedDay: null,
        patient: {
            id: null,
            firstname: null,
            lastname: null,
            prefix: null,
            settings: {
                mealtime: {
                    breakfast: null,
                    elevenses: null,
                    lunch: null,
                    afternoonsnack: null,
                    dinner: null
                }
            }
        },
    },
    mutations: {
        initializeRange(state, range) {
            Object.assign(state.range, range);
        },
        setSelectedRange(state, range) {
            state.range.selectedMinDate = range.start;
            state.range.selectedMaxDate = range.end;
        },
        setDiaryDate(state, date) {
            state.diarySelectedDay = date;
        },
        initializePatient(state, patient) {
            Object.assign(state.patient, patient);
            Object.assign(
                state.patient.settings,
                webStorage.getPatientSettings(patient.id)
            );
        },
        updatePatientMealtime(state, mealtime) {
            Object.assign(state.patient.settings.mealtime, mealtime);
            webStorage.setPatientSettings(state.patient.id, state.patient.settings);
        }
    },
    getters: {
        totalDays(state) {
            return utils.getTotalDays(state.range.selectedMaxDate, state.range.selectedMinDate);
        },
        patientMealtimes(state) {
            return {
                breakfast: utils.convertTimeString(state.patient.settings.mealtime.breakfast),
                elevenses: utils.convertTimeString(state.patient.settings.mealtime.elevenses),
                lunch: utils.convertTimeString(state.patient.settings.mealtime.lunch),
                afternoonsnack: utils.convertTimeString(state.patient.settings.mealtime.afternoonsnack),
                dinner: utils.convertTimeString(state.patient.settings.mealtime.dinner)
            };
        },
        getPatientName: (state) => (lang) => {
            let names = [];
            if (lang == "hu") {
                names.push(
                    state.patient.prefix,
                    state.patient.lastname,
                    state.patient.firstname
                );
            } else {
                names.push(
                    state.patient.prefix,
                    state.patient.firstname,
                    state.patient.lastname
                );
            }

            return names.filter(x => !!x).join(" ");
        }
    }
}

export default mainModule;