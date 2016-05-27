﻿(function (angular) {
    angular
        .module('equizModule')
        .controller('UserGroupController', UserGroupController);

    UserGroupController.$inject = ['$scope', 'userGroupService', '$location', '$timeout'];

    function UserGroupController($scope, userGroupService, $location, $timeout) {

        var vm = this;
        vm.users = [];
        
        vm.predicate = 'LastName';
        vm.reverse = false;
        vm.errorMessageVisible = false;
        vm.successMessageVisible = false;
        vm.loadingVisible = false;

        vm.sortBy = sortBy;
        vm.showOrderArrow = showOrderArrow;
        vm.deleteUser = deleteUser;
        vm.save = save;
        vm.showSuccess = showSuccess;
        vm.showError = showError;
        vm.showLoading = showLoading;
        vm.hideLoading = hideLoading;

        activate();

        function activate() {
            if ($location.search().id) {
                vm.showLoading();
                userGroupService.getGroup($location.search().id).then(function (data) {
                    vm.group = data.data.group;
                    vm.users = data.data.users;
                    vm.hideLoading();
                });
            }
        };


        function sortBy(predicate) {
            vm.reverse = (vm.predicate === predicate) ? !vm.reverse : false;
            vm.predicate = predicate;            
        };

        function showOrderArrow(predicate) {
            if (vm.predicate === predicate) {
                return vm.reverse ? '▲' : '▼';
            }
            return '';
        };

        function deleteUser(user) {
            var userIndex = vm.users.indexOf(user);
            vm.users.splice(userIndex, 1);
        };

        function save() {
            vm.showLoading();
            userGroupService.save({ userGroup: vm.group, users: vm.users }).then(function (data) {
                vm.group = data.data.group;
                vm.users = data.data.users;
                vm.hideLoading();
                vm.showSuccess();
            }, function (data) {
                vm.hideLoading();
                vm.showError();
            });
        };

        function showSuccess() {
            vm.successMessageVisible = true;
            $timeout(function () {
                vm.successMessageVisible = false;
                window.location.href = '/moderator/usergroup/index';
            }, 2000);
        }
        function showError() {
            vm.errorMessageVisible = true;
            $timeout(function () {
                vm.errorMessageVisible = false;
            }, 4000);
        }
        function showLoading() {
            vm.loadingVisible = true;
        }
        function hideLoading() {
            vm.loadingVisible = false;
        }
    };

})(angular)