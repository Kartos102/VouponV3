(function (window, document, $) {
    "use strict";

    // This function will contain all our code
    function vodusRewards() {
        var _vodusObject = {};
        
        // Just create a property to our library object.
        _vodusObject.myCustomLog = function (thingToLog) {
            console.log("My-Custom-Log > Type of variable : " + typeof (thingToLog));
            console.log("My-Custom-Log > Is number : " + !isNaN(thingToLog));
            console.log("My-Custom-Log > Length : " + (thingToLog).length);

            return console.log(thingToLog);
        };

        return _vodusObject;
    }

    // We need that our library is globally accesible, then we save in the window
    if (typeof (window.vodusRewards) === 'undefined') {
        window.vodusRewards = vodusRewards();
    }
})(window); // We send the window variable withing our function

// Then we can call our custom function using
//vodusRewards.myCustomLog(["My library", "Rules"]);