var map;
function initialize() {
    var mapOptions = {
        center: { lat: -34.397, lng: 150.644 },
        zoom: 8
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);
}
// document ready shorthand
google.maps.event.addDomListener(window, 'load', initialize);

$(function () {
    // obtain reference to the hub proxy and hub itself
    var theHub = $.connection.twitterHub;

    // this is the function that the server will call to broadcast new tweets
    theHub.client.broadcast = function (tweet) {

        var c = tweet.coordinates.coordinates;

        if (!$.isEmptyObject(tweet.coordinates)) {
            //console.log(c);
            // To add the marker to the map, use the 'map' property
            var marker = new google.maps.Marker({
                position: { lat: c[1], lng: c[0] },
                map: map,
                title: "Hello World!"
            });
        }
        
    };

    // this is a function that indicates that connection to the hub has been successful
    $.connection.hub.start().done(function () {
        console.log("connected");
    });
});
