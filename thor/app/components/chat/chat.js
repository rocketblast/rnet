'use strict';

angular.module('Thor.Chat', ['ui.router', 'ui.bootstrap']).config(function ($stateProvider) {
    $stateProvider.state('chat', {
        url: '/chat',
        templateUrl: 'app/components/chat/Chat.html',
        controller: 'ChatCtrl'
    }).
    state('chat.server', {
        url: '/chat/:serverName',
        templateUrl: 'app/components/chat/server/Server.Html',
        controller: 'ChatServerCtrl'
    });
});