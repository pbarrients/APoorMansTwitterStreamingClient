$(function() {
    var theHub = $.connection.twitterHub;

    theHub.client.broadcast = function (tweet) {
        console.log(tweet);
    };
});