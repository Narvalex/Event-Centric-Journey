(function () {
    'use strict';

    angular
        .module('app')
        .controller('agregarAnimalesController', agregarAnimalesController);

    agregarAnimalesController.$inject = ['$scope', 'agregarAnimalesDto', 'dao'];

    function agregarAnimalesController($scope, agregarAnimalesDto, dao) {
        var vm = $scope;

        vm.dto = agregarAnimalesDto;
        vm.animales = [];


        activate();

        function activate() {
            vm.animales = dao.getListaDeAnimales();
        }
    }
})();
