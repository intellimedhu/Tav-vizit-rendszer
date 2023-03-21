import Vue from 'vue';
import Vuex from 'vuex';
import utils from '../utils';

Vue.use(Vuex);

const store = new Vuex.Store({
    state: {
        additionalData: {
            accreditationStatuses: [],
            centerTypes: [],
            colleagueStatusZones: {
                active: [],
                pending: [],
                removed: []
            },
            colleagueStatuses: [],
            days: [],
            laboratory: [],
            occupations: [],
            qualificationStateCaptions: [],
            tools: []
        },
        options: {
            googleMapsApiKey: null,
            isNew: null,
            urls: {}
        }
    },
    mutations: {
        initAdditionalData(state, additionalData) {
            Object.assign(state.additionalData, additionalData);
        },
        initOptions(state, options) {
            Object.assign(state.options, options);
        }
    },
    getters: {
        accreditationStatuses: state => {
            return utils.mapKeyValuePairs(state.additionalData.accreditationStatuses);
        },
        centerTypes: state => {
            return utils.mapKeyValuePairs(state.additionalData.centerTypes);
        },
        colleagueStatuses: state => {
            return utils.mapKeyValuePairs(state.additionalData.colleagueStatuses);
        },
        days: state => {
            return utils.mapDays(state.additionalData.days);
        },
        dayNames: state => {
            return utils.mapKeyValuePairs(state.additionalData.days);
        },
        occupations: state => {
            return utils.mapKeyValuePairs(state.additionalData.occupations);
        },
        qualificationStateCaptions: state => {
            return utils.mapKeyValuePairs(state.additionalData.qualificationStateCaptions);
        },
        tools: state => {
            return state.additionalData.tools;
        },
        laboratory: state => {
            return state.additionalData.laboratory;
        },
        colleagueStatusZones: state => {
            return state.additionalData.colleagueStatusZones;
        }
    }
});

export default store;