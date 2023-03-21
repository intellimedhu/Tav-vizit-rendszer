import Vue from 'vue'
import BootstrapVue from "bootstrap-vue"
import App from './App.vue'
// import "bootstrap/dist/css/bootstrap.min.css"
// import "bootstrap-vue/dist/bootstrap-vue.css"

Vue.use(BootstrapVue)

var element = document.getElementById('diabetes-user-profile-editor');

Vue.prototype.$http = require('axios');

new Vue({
  el: '#diabetes-user-profile-editor',
  render: h => h(App, {
    props: {
      apiGetUrl: element.dataset.apiGetUrl,
      apiPostUrl: element.dataset.apiPostUrl,
      forwardUrl: element.dataset.forwardUrl,
      backButtonUrl: element.dataset.backButtonUrl
    }
  })
})
