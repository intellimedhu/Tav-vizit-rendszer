import Vue from 'vue';
import App from './App.vue';
import BootstrapVue from 'bootstrap-vue';
import 'bootstrap-vue/dist/bootstrap-vue.css';

Vue.use(BootstrapVue);

new Vue({
  el: '#organimedcore-diabetes-care-center-manager-map-view',
  render: h =>
    h(App, {
      props: {
        mapViewOptions: mapViewOptions
      }
    })
});
