// document ready shorthand
$(function () {
    // obtain reference to the hub proxy and hub itself
    var theHub = $.connection.twitterHub;


    // this is the function that the server will call to broadcast new tweets
    theHub.client.broadcast = function (tweet) {
        var item = '<li>' + tweet.text + '</li>';
        $('ul.tweets').prepend(item);
    };

    // this is a function that indicates that connection to the hub has been successful
    $.connection.hub.start().done(function () {
        console.log("connected");
    });
});