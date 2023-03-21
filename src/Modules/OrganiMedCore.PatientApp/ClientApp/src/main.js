import Vue from 'vue'
import store from './store';
import App from './App.vue'
import BootstrapVue from 'bootstrap-vue'

Vue.use(BootstrapVue);

new Vue({
  store,
  el: '#app',
  render: h => h(App)
});