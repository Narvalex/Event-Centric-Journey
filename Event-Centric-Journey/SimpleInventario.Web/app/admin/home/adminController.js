(function () {
    'use strict';

    angular
        .module('app')
        .controller('adminController', adminController);

    adminController.$inject = ['$scope']; 

    function adminController($scope) {
        $scope.title = 'adminController';

        activate();

        function activate() { }
    }
})();
