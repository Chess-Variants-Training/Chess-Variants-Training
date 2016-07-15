function start() {
    window.ended = false;
    document.getElementById("start-training").removeEventListener("onclick", start);
    document.getElementById("start-training").classList.remove("start-link");
    jsonXhr("/Puzzle/Train-Timed/Mate-In-One/Start", "POST", null, function(req, jsonResponse) {
        window.sessionId = jsonResponse["sessionId"];
        document.getElementById("start-training").textContent = jsonResponse["seconds"].toString();
        window.secondsLeft = jsonResponse["seconds"];
        window.interval = setInterval(function() {
            window.secondsLeft--;
            document.getElementById("start-training").textContent = window.secondsLeft.toString();
            if (window.secondsLeft === 0) {
                end();
            }
        }, 1000);
        showPosition(jsonResponse["fen"], jsonResponse["color"], jsonResponse["dests"]);
    }, function(req, err) {
        alert(err);
    });
}

function end() {
    if (window.ended) return;
    clearInterval(window.interval);
    window.ended = true;
    document.getElementById("start-training").textContent = "0";
    window.ground.stop();
    jsonXhr("/Puzzle/Train-Timed/Mate-In-One/AcknowledgeEnd", "POST", "sessionId=" + window.sessionId, function (req, jsonResponse) {
        document.getElementById("start-training").textContent = "Score: " + jsonResponse["score"];
    }, function (req, err) {
        alert(err);
    });
}

function showPosition(fen, color, dests) {
    window.ground.set({
        fen: fen,
        orientation: color,
        turnColor: color,
        lastMove: null,
        selected: null,
        movable: {
            dests: dests
        }
    });
}

function verifyAndGetNext(origin, destination, metadata) {
    if (window.ended) return;
    jsonXhr("/Puzzle/Train-Timed/Mate-In-One/VerifyAndGetNext", "POST",
        "sessionId=" + window.sessionId + "&origin=" + origin + "&destination=" + destination, function (req, jsonResponse) {
            if (jsonResponse["ended"] && !window.ended) {
                end();
                return;
            }
            var li = document.createElement("li");
            var text = document.createTextNode(jsonResponse["correct"] ? "Correct" : "Incorrect");
            li.appendChild(text);
            document.getElementById("previous").appendChild(li);
            showPosition(jsonResponse["fen"], jsonResponse["color"], jsonResponse["dests"]);
        }, function (req, err) {
            alert(err);
        })
}

window.addEventListener("load", function () {
    window.ground = Chessground(document.getElementById("chessground"), {
        coordinates: false,
        movable: {
            free: false,
            dropOff: "revert",
            showDests: false,
            events: {
                after: verifyAndGetNext
            }
        }
    });
    document.getElementById("start-training").addEventListener("click", start);
});