$(function() {
    var theHub = $.connection.twitterHub;

    theHub.client.broadcast = function (tweet) {
        var item = '<li>' + tweet.text + '</li>';
        $('ul.tweets').prepend(item);
    };

    $.connection.hub.start().done(function () {
        console.log("connected");
    });
});