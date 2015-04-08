(function () {
    'use strict';

    angular
        .module('app')
        .factory('agregarAnimalesDto', agregarAnimalesDto);

    function agregarAnimalesDto() {
        
        return {
            animal: '',
            cantidad: '',
            periodo: ''
        };
    }
})();