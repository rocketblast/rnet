'use strict';

var module = angular.module('Thor', ['ui.router', 'Thor.Chat']);

module.config(function ($urlRouterProvider) {
    $urlRouterProvider.otherwise('/home');
});

module.run(function ($rootScope) {
    $rootScope.$on('$stateChangeError', console.error.bind(console));

    $rootScope.$on('$stateChangeStart', function (event, toState) {
        if (toState.redirectTo) {
            if (angular.isFunction(toState.redirectTo)) {
                toState.redirectTo.apply(toState, [].slice.call(arguments));
            } else {
                event.preventDefault();
                $state.go(toState.redirectTo);
            }
        }
    });
});