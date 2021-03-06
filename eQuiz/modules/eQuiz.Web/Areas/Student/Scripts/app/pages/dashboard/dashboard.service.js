﻿/// <reference path="E:\MyFork_eQuiz\eQuiz\modules\eQuiz.Web\Scripts/libs/angularjs/angular.js" />
(function (angular) {
    var equizModule = angular.module("equizModule");

    equizModule.factory("dashboardService", ["$http", function ($http) {

        var service = {
            getQuizzes: getQuizzesAjax,
            //getQuestionsById: getQuestionsByIdAjax,
            getUserGroups : getGroups,
            getUserInfo: getUser
        };

        return service;

        function getUser() {
            var promise = $http.get("GetUserInfo");
            return promise;
        };

        function getGroups() {
            var promise = $http.get("GetUserGroups");
            return promise;
        };
        function getQuizzesAjax() {
            var promise = $http.get("GetAllQuizzes");
            return promise;
        };
    }]);
})(angular);