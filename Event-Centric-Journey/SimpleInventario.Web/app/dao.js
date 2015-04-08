(function () {
    'use strict';

    angular
        .module('app')
        .factory('dao', dao);

    dao.$inject = ['$http'];

    function dao($http) {
        var api = {
            getListaDeAnimales: getListaDeAnimales
        };

        return api;

        function getListaDeAnimales() {
            return [
                "Perro",
                "Gato",
                "Conejo",
                "Tortuga",
                "Vaca",
                "Burro"
            ];
        }
    }
})();