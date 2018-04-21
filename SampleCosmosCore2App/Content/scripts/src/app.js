/* License statement */

import $ from 'jquery';
import Vue from 'vue';
import SampleAppViewModel from './sampleApp/sampleAppViewModel';

console.log(Vue);

$(() => {
    if($('#app').length > 0){
        const vm = new SampleAppViewModel();
        vm.initialize();

        new Vue({
            el: '#app',
            data: vm,
            computed: vm.computed
        });
    }
});
