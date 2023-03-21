import basicData from './components/basic-data.vue';
import additionalData from './components/additional-data.vue';
import equipments from './components/equipments.vue';
import colleagues from './components/colleagues.vue';
import summary from './components/summary.vue';

const routes = [
    { path: '/', component: basicData, name: "basicData" },
    { path: '/additional', component: additionalData },
    { path: '/equipments', component: equipments },
    { path: '/colleagues', component: colleagues },
    { path: '/summary', component: summary },
];

export default routes;