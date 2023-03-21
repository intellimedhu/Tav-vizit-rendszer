import Vue from 'vue'
import BootstrapVue from "bootstrap-vue"
import App from './App.vue'
//import "bootstrap/dist/css/bootstrap.min.css"
import "bootstrap-vue/dist/bootstrap-vue.css"

Vue.use(BootstrapVue)

Vue.prototype.$http = require('axios');

var element = document.getElementById('settlement-editor');

new Vue({
  el: '#settlement-editor',
  render: h => h(App, {
    props: {
      apiUrl: element.dataset.apiUrl
    }
  })
});
