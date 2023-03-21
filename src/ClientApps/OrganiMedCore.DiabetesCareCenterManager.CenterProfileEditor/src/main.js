import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import App from './App.vue';
import VueRouter from 'vue-router';
import routes from './routes';
//import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import store from './stores/store';

Vue.use(BootstrapVue);
Vue.use(VueRouter);

const router = new VueRouter({
  routes,
  scrollBehavior() {
    return { x: 0, y: 0 };
  }
});

store.commit("initOptions", options);

new Vue({
  router,
  store,
  el: '#center-creation-request-editor',
  render: h => h(App)
});
