(function () {
    'use strict';

    angular.module('app').controller('portalController', portalController);

    portalController.$inject = ['$scope', '$http'];

    function portalController($scope, $http) {
        var vm = $scope;

        vm.start = start;
        vm.stop = stop;
        vm.rebuildReadModel = rebuildReadModel;
        vm.rebuildEventStore = rebuildEventStore;

        vm.portalHub = null;

        // indica si se detuvo el worker o si esta corriendo
        vm.isWorking = true;

        // para esconder todo el muenu de comandos, mientras esta ejecutando un proceso de reconstruccion
        vm.hideAll = false;

        activate();

        function activate() {

            var Model = function () {
                var self = this;

                self.notifications = ko.observableArray();
                self.messages = ko.observableArray();
            };

            Model.prototype = {
                addNotification: function (notification) {
                    var self = this;

                    var entry = ko.utils.arrayFirst(self.notifications(),
                        function (receivedNotification) {
                            return receivedNotification.id == notification.id;
                        });

                    if (!entry) {
                        self.notifications.push(notification);
                        self.messages.push(notification.id + '. ' + notification.message);

                        scrollToBottom();

                    }
                }
            };

            var model = new Model();

            vm.portalHub = $.connection.portalHub;

            vm.portalHub.client.notify = function (notification) {
                model.addNotification(notification);
            };

            vm.portalHub.client.newMessage = function (message) {
                //toastr.info(message);
            };

            $.connection.hub.logging = true;
            $.connection.hub.start().done(function () {
                // SingalR is connected...
                vm.portalHub.server.sendMessage('Client connected');
                getStatus();
            });

            $(function () {
                ko.applyBindings(model);
            });
        }

        function start() {
            //toastr.info("Starting Engine...")
            vm.portalHub.server.sendMessage('===> Starting worker...');
            return $http.get('/api/portal/start')
                        .then(function (response) {
                            //toastr.success("Engine is running!")
                            vm.isWorking = response.data;
                        });
        }

        function stop() {
            //toastr.warning("Stopping Engine...")
            vm.portalHub.server.sendMessage('===> Stopping worker...');
            return $http.get('/api/portal/stop')
                        .then(function (response) {
                            //toastr.error("Engine stopped!")
                            vm.isWorking = response.data;
                        });
        }

        function rebuildReadModel() {
            vm.hideAll = true;
            // stopping...

            toastr.warning("Stopping Engine for read model rebuild...")
            vm.portalHub.server.sendMessage('===> Stopping worker and starting read model rebuilding process...');
            return $http.get('/api/portal/rebuildReadModel')
                        .then(function (response) {
                            vm.isWorking = response.data;
                            vm.hideAll = false;
                            toastr.info('Engine is now working again');
                        });

        }

        function rebuildEventStore() {
            vm.hideAll = true;
            // stopping...

            toastr.warning("Stopping Engine for event store rebuild...")
            vm.portalHub.server.sendMessage('===> Stopping worker and starting event store rebuilding process...');
            return $http.get('/api/portal/rebuildEventStore')
                        .then(function (response) {
                            vm.isWorking = response.data;
                            vm.hideAll = false;
                            toastr.info('Engine is now working again');
                        });

        }

        function getStatus() {
            vm.portalHub.server.sendMessage("Checking worker status...");
            return $http.get('/api/portal/status')
                        .then(function (response) {

                            vm.isWorking = response.data;

                            if (vm.isWorking) {
                                vm.portalHub.server.sendMessage("Worker is online");
                            }
                            else {
                                vm.portalHub.server.sendMessage("Worker is down");
                            }
                        });
        }
    }

    function scrollToBottom() {
        $('html, body').animate({ scrollTop: $(document).height() }, 10);
        //$('html, body').animate({ scrollTop: $(document).height() }, 500);
        //window.scrollTo(0, document.body.scrollHeight);
    }

})();

