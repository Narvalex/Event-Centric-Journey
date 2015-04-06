﻿(function () {
    'use strict';

    angular.module('app').controller('portalController', portalController);

    portalController.$inject = ['$scope', '$http'];

    function portalController($scope, $http) {
        var vm = $scope;

        vm.start = start;
        vm.stop = stop;
        vm.rebuild = rebuild;

        vm.portalHub = null;

        vm.isWorking = true;

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

        function rebuild() {
            vm.hideAll = true;
            // stopping...

            toastr.warning("Stopping Engine for rebuild...")
            return $http.get('/api/portal/stop')
                        .then(function (response) {
                            toastr.error("Engine stopped! Now start rebuild process")
                            vm.isWorking = response.data;

                            // After stopping, now Reguild
                            $http.get('/api/portal/reproject')
                                .then(function (response) {
                                    vm.hideAll = false;

                                    // After rebuilding start again
                                    start();
                                });

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
