'use strict';

var module = angular.module('thorApp', []);

module.service('RealtimeService', function () {
    var skynet = $.connection.Skynet;

    this.Connect = function () {
        $.connection.hub.start().done(function () {
            console.log("Connected!");

            this.skynet.server.joingroup("webclients");
        })
    }
});