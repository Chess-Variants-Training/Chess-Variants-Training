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
        var message = JSON.parse(e.data);
        var type = message.t;
        var data = message.d;
        switch (type) {
            case "add":
                var seekTableRow = document.createElement("div");
                seekTableRow.setAttribute("id", "seek-" + data.i);
                var playerDiv = document.createElement("div");
                playerDiv.innerHTML = data.o;
                playerDiv.classList.add("seek-player");
                seekTableRow.appendChild(playerDiv);
                var timeDiv = document.createElement("div");
                timeDiv.innerHTML = data.c;
                timeDiv.classList.add("seek-time");
                seekTableRow.appendChild(timeDiv);
                var variantDiv = document.createElement("div");
                variantDiv.innerHTML = data.v + " (" + data.s + ")";
                variantDiv.classList.add("seek-variant");
                seekTableRow.appendChild(variantDiv);
                document.getElementById("seek-table").appendChild(seekTableRow);
                break;
            case "remove":
                if (document.getElementById("seek-" + data)) {
                    document.getElementById("seek-" + data).remove();
                }
                break;
        }
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