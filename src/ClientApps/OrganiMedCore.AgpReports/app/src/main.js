import 'babel-polyfill';
import Vue from 'vue';
import Vuex from 'vuex';
import moment from 'moment';
import App from './App';
import Unsupported from './Unsupported';
import 'bootstrap';
import utils from './services/utils';
import VueI18n from 'vue-i18n';
import messages from './lang.json';
import mainModule from './stores/main-vuex';
import globalSettingsModule from './stores/global-settings-vuex';
import BootstrapVue from 'bootstrap-vue';

// // Opera 8.0+
// var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;

// // Firefox 1.0+
// var isFirefox = typeof InstallTrigger !== 'undefined';

// // Safari 3.0+ "[object HTMLElementConstructor]"
// var isSafari =
//     /constructor/i.test(window.HTMLElement) ||
//     (function(p) {
//         return p.toString() === '[object SafariRemoteNotification]';
//     })(!window['safari'] || safari.pushNotification);

// Internet Explorer 6-11
var isIE = /*@cc_on!@*/ false || !!document.documentMode;

// Edge 20+
var isEdge = !isIE && !!window.StyleMedia;

// // Chrome 1+
// var isChrome = !!window.chrome && !!window.chrome.webstore;

// Blink engine detection
//var isBlink = (isChrome || isOpera) && !!window.CSS;

var supportedBrowser = !(isIE || isEdge);
if (supportedBrowser) {
    Vue.use(VueI18n);
    Vue.use(BootstrapVue);

    Vue.config.productionTip = false;

    Vue.filter('round', function (value, decimals) {
        return utils.round(value, decimals).toFixed(decimals);
    });

    Vue.filter('formatDate', function (value) {
        if (value) {
            return moment(value).format('YYYY. MMMM DD.');
        }
    });

    Vue.filter('shortDate', function (value) {
        if (value) {
            return moment(value).format('YYYY.MM.DD.');
        }
    });

    Vue.filter('shortTime', function (value) {
        return utils.shortTime(value);
    });

    Vue.use(Vuex);
    const store = new Vuex.Store({
        modules: {
            mainModule: mainModule,
            globalSettingsModule: globalSettingsModule
        }
    });

    const i18n = new VueI18n({
        locale: 'hu', // set locale
        messages // set locale messages
    });

    var container = 'charts-main-container';
    var element = document.getElementById(container);

    window.addEventListener("load", function (event) {
        new Vue({
            i18n,
            store,
            el: '#' + container,
            render: h =>
                h(App, {
                    props: {
                        baseUrl: element.dataset.baseUrl,
                        patientProfileRawUrl: element.dataset.patientProfileUrl
                    }
                })
        });
    });
} else {
    new Vue({
        el: '#' + container,
        render: h => h(Unsupported)
    });
}
