(function () {
    'use strict';

    angular
        .module('app')
        .factory('comanding', comanding);

    comanding.$inject = ['$http'];

    function comanding($http) {
        var service = {
            agregarAnimales: agregarAnimales
        };

        return service;

        function agregarAnimales(dto) {
            return $http.post('api/admin/agregarAnimales', dto);
        }
    }
})();