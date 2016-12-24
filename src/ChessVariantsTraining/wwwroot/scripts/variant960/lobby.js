(function main() {
    var ws;
    var clientId;

    function initialTimeChanged() {
        var value = parseInt(document.getElementById("time-range").value, 10);
        var text = "";
        switch (value)
        {
            case 0:
                text = "0 minutes";
                break;
            case 1:
                text = "30 seconds";
                break;
            case 2:
                text = "45 seconds";
                break;
            case 3:
                text = "1 minute";
                break;
            case 4:
                text = "90 seconds";
                break;
            default:
                text = (value - 3).toString(10) + " minutes";
                break;
        }
        document.getElementById("initial-time").innerHTML = text;
    }

    function incrementChanged() {
        var value = document.getElementById("inc-range").value;
        document.getElementById("increment").innerHTML = value === "1" ? "1 second" : value.toString() + " seconds";
    }

    function makeSeek() {
        var initialTimeValue = parseInt(document.getElementById("time-range").value, 10);
        switch (initialTimeValue) {
            case 0:
                var initialTimeSeconds = "0";
                break;
            case 1:
                var initialTimeSeconds = "30";
                break;
            case 2:
                var initialTimeSeconds = "45";
                break;
            case 3:
                var initialTimeSeconds = "60";
                break;
            case 4:
                var initialTimeSeconds = "90";
                break;
            default:
                var initialTimeSeconds = ((initialTimeValue - 3) * 60).toString(10);
                break;
        }
        var inc = document.getElementById("inc-range").value;
        var variant = document.getElementById("variant-selector").value;
        var sym = document.getElementById("symmetry-selector").value === "symmetrical" ? "true" : "false";
        var message = { "t": "create", "d": initialTimeSeconds + ";" + inc + ";" + variant + ";" + sym };
        ws.send(JSON.stringify(message));
    }

    function wsMessageReceived(e) {
        console.log(e);
    }

    window.addEventListener("load", function () {
        initialTimeChanged();
        incrementChanged();
        document.getElementById("time-range").addEventListener("input", initialTimeChanged);
        document.getElementById("time-range").addEventListener("change", initialTimeChanged);
        document.getElementById("inc-range").addEventListener("input", incrementChanged);
        document.getElementById("inc-range").addEventListener("change", incrementChanged);
        document.getElementById("create-game").addEventListener("click", makeSeek);

        clientId = Math.random().toString(36).substring(2);
        while (clientId.length < 6) { clientId += clientId; }
        ws = new WebSocket(window.wsUrl + "?clientId=" + clientId);
        ws.addEventListener("message", wsMessageReceived);
    });
})();