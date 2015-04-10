(function () {
    'use strict';

    angular
        .module('app')
        .controller('agregarAnimalesController', agregarAnimalesController);

    agregarAnimalesController.$inject = ['$scope', 'agregarAnimalesDto', 'querying', 'comanding', 'utils'];

    function agregarAnimalesController($scope, agregarAnimalesDto, querying, comanding, utils) {
        var vm = $scope;

        vm.dto = agregarAnimalesDto;
        vm.animales = [];
        vm.sucursales = [];
        vm.periodos = [];

        // Actions
        vm.agregarAnimales = agregarAnimales;


        activate();

        function activate() {
            vm.animales = querying.getListaDeAnimales();
            vm.sucursales = querying.getListaDeSucursales();
            vm.periodos = querying.getListaPeriodos();
        }

        // Actions
        function agregarAnimales() {
            utils.disableSubmitInput();

            comanding.agregarAnimales(vm.dto)
                .then(function (data) {
                    toastr.success('Se agregaron animales');
                    location = '/#/';
                },
                function (message) {
                    toastr.error('Hubo un error. Verifique que los datos estén correctos por favor')
                    utils.enableSubmitInput();
                });
        }
    }
})();
