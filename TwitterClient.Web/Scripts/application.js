var map;
function fitToUKBounds(map) {
    var topLeft = new google.maps.LatLng(49.955269, -8.164723);
    var bottomRight = new google.maps.LatLng(60.6311, 1.7425);
    var bounds = new google.maps.LatLngBounds(topLeft, bottomRight);
    map.fitBounds(bounds);
}
function initialize() {
    var mapOptions = {
        zoom: 5
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);
    fitToUKBounds(map);
}
// document ready shorthand
google.maps.event.addDomListener(window, 'load', initialize);

$(function () {
    // obtain reference to the hub proxy and hub itself
    var theHub = $.connection.twitterHub;

    // this is the function that the server will call to broadcast new tweets
    theHub.client.broadcast = function (tweet) {

        var c = tweet.coordinates;

        if (!$.isEmptyObject(c)) {
            console.log(c);
            // To add the marker to the map, use the 'map' property
            var marker = new google.maps.Marker({
                position: { lat: c.coordinates[1], lng: c.coordinates[0] },
                map: map,
                animation: google.maps.Animation.DROP,
                title: tweet.text
            });

            var html = tweet.text;

            var infowindow = new google.maps.InfoWindow({
                content: html
            });
            google.maps.event.addListener(marker, 'click', function () {
                infowindow.open(map, marker);
            });

        }
        
    };

    // this is a function that indicates that connection to the hub has been successful
    $.connection.hub.start().done(function () {
        console.log("connected");
    });
});
