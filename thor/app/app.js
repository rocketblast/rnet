'use strict';

//var module = angular.module('Thor', ['ui.router', 'Thor.Chat', 'RealtimeService']);
//var module = angular.module('thorApp', ['ui.router', 'ngRoute', '$rootScope']);
var module = angular.module('thorApp', ['ngRoute']);

module.config(function ($routeProvider) {
    //$urlRouterProvider.otherwise('/home');

    $routeProvider.when('/', {
        templateUrl: 'app/components/home/home.html',
        controller: 'homeController'
    });
});

//module.run(function ($rootScope) {
//    $rootScope.$on('$stateChangeError', console.error.bind(console));

//    $rootScope.$on('$stateChangeStart', function (event, toState) {
//        if (toState.redirectTo) {
//            if (angular.isFunction(toState.redirectTo)) {
//                toState.redirectTo.apply(toState, [].slice.call(arguments));
//            } else {
//                event.preventDefault();
//                $state.go(toState.redirectTo);
//            }
//        }
//    });
//});