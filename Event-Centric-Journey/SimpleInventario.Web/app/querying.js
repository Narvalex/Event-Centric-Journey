(function () {
    'use strict';

    angular
        .module('app')
        .factory('querying', querying);

    querying.$inject = ['$http'];

    function querying($http) {
        var api = {
            getListaDeAnimales: getListaDeAnimales,
            getListaDeSucursales: getListaDeSucursales,
            getListaPeriodos: getListaPeriodos
        };

        return api;

        function getListaDeAnimales() {
            var lista = 
            [
                "Perro",
                "Gato",
                "Conejo",
                "Tortuga",
                "Vaca",
                "Burro"
            ];

            return lista;
        }

        function getListaDeSucursales() {
            var lista =
                [
                    'Asunción',
                    'San Lorenzo',
                    'New York',
                    'Vallemí'
                ];

            return lista;
        }

        function getListaPeriodos() {
            var lista =
                [
                    '2015',
                    '2014',
                    '2013',
                    '2012'
                ];

            return lista;
        }
    }
})();