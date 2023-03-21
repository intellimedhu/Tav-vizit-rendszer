import webStorage from '../services/web-storage';

const globalSettingsModule = {
    state: {
        globalSettings: webStorage.getGlobalSettings()
    },
    mutations: {
        setGlobalSettings(state, updatedSettings) {
            Object.assign(state.globalSettings, updatedSettings);
            webStorage.updateGlobalSettings(updatedSettings);
            
        },

        setGlobalSettingsProperty(state, property) {
            state.globalSettings[property.key] = property.value;
            webStorage.updateGlobalSettings(state.globalSettings);
        },

        setPrintOptions(state, property) {
            state.globalSettings.printOptions[property.key] = property.value;
            webStorage.updateGlobalSettings(state.globalSettings);
        }
    }
}

export default globalSettingsModule;