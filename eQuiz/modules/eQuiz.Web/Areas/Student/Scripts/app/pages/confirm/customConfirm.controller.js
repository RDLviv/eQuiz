﻿(function (angular) {
    var equizModule = angular.module("equizModule");

    equizModule.controller('confirmCtrl', ["$scope", "$uibModalInstance", function ($scope, $uibModalInstance) {
        $scope.ok = function () {
            $uibModalInstance.close(true);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    }]);
})(angular);