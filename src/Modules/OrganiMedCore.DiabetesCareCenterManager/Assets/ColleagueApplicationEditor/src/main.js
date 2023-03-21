import Vue from 'vue'
import App from './App.vue'

var element = document.getElementById('colleague-application-editor');

Vue.prototype.$http = require('axios');

new Vue({
  el: '#colleague-application-editor',
  render: h => h(App, {
    props: {
      occupationCanBeChanged: element.dataset.occupationCanBeChanged.toLowerCase() == 'true',
      occupation: /\d/.test(element.dataset.occupation) ? +element.dataset.occupation : null,
      apiUrl: element.dataset.apiUrl,
      backButtonUrl: element.dataset.backButtonUrl
    }
  })
});