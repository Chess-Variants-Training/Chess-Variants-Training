function startEndgameTraining(e) {
    e = e || window.event;
    e.preventDefault();
    jsonXhr(window.location.href.trimRight("/") + "/Start", "POST", null, function (req, jsonResponse) {
        window.ground.set({
            lastMove: null,
            fen: jsonResponse["fen"]
        });
        document.getElementById("result").setAttribute("class", "blue");
        document.getElementById("result").innerHTML = "Win the game!";
        window.trainingSessionId = jsonResponse["sessionId"];
        document.getElementById("start-link").classList.add("nodisplay");
        jsonXhr("/Endgames/GetValidMoves/" + window.trainingSessionId, "GET", null, function (req, jsonResponse) {
            window.ground.set({
                movable: { dests: jsonResponse.dests }
            });
        }, function (req, err) {
            alert(err);
        });
    }, function (req, err) { alert(err); });
}

function processMove(origin, destination, metadata) {
    if (ChessgroundExtensions.needsPromotion(window.ground, destination)) {
        ChessgroundExtensions.drawPromotionDialog(origin, destination, document.getElementById("chessground"), submitMove, window.ground);
    } else {
        submitMove(origin, destination, null);
    }
}

function submitMove(origin, destination, promotion) {
    jsonXhr("/Endgames/SubmitMove", "POST", "trainingSessionId=" + window.trainingSessionId + "&origin=" + origin + "&destination=" + destination + (promotion ? "&promotion=" + promotion : ""),
        function (req, jsonResponse) {
            if (jsonResponse.fen) {
                window.ground.set({
                    fen: jsonResponse.fen
                });
            }
            if (jsonResponse.check) {
                window.ground.setCheck(jsonResponse.check);
            } else {
                window.ground.set({ check: null });
            }
            switch (jsonResponse.correct) {
                case 1:
                    document.getElementById("result").setAttribute("class", "green");
                    document.getElementById("result").innerHTML = "You won!";
                    break;
                case -1:
                    document.getElementById("result").setAttribute("class", "red");
                    document.getElementById("result").innerHTML = "50-move rule: it's a draw!";
                    break;
                case -2:
                    document.getElementById("result").setAttribute("class", "red");
                    document.getElementById("result").innerHTML = "It's a stalemate!";
                    break;
                case -3:
                    document.getElementById("result").setAttribute("class", "red");
                    document.getElementById("result").innerHTML = "You lost!";
                    break;
                case 0:
                    window.ground.set({
                        fen: jsonResponse.fenAfterPlay
                    });
                    if (jsonResponse.checkAfterAutoMove) {
                        window.ground.setCheck(jsonResponse.checkAfterAutoMove);
                    } else {
                        window.ground.set({ check: null });
                    }
                    if (jsonResponse.drawAfterAutoMove) {
                        document.getElementById("result").setAttribute("class", "red");
                        document.getElementById("result").innerHTML = "50-move rule: it's a draw!";
                    }
                    if (jsonResponse.winAfterAutoMove) {
                        document.getElementById("result").setAttribute("class", "green");
                        document.getElementById("result").innerHTML = "You won!";
                    }
                    break;
            }
            if (jsonResponse.correct !== 0 || jsonResponse.drawAfterAutoMove || jsonResponse.winAfterAutoMove) {
                document.getElementById("start-link").classList.remove("nodisplay");
                document.getElementById("start-link").childNodes[0].textContent = "Train again";
            }
            if (jsonResponse.dests) {
                window.ground.set({
                    movable: {
                        dests: jsonResponse.dests
                    }
                });
            }
            if (jsonResponse.lastMove) {
                window.ground.set({
                    lastMove: jsonResponse.lastMove
                });
            }
        }, function (req, err) { alert(err); });
}

window.addEventListener('load', function () {
    window.ground = Chessground(document.getElementById("chessground"), {
        coordinates: false,
        movable: {
            free: false,
            dropOff: "revert",
            showDests: false,
            events: {
                after: processMove
            }
        },
        drawable: {
            enabled: true
        }
    });
    document.getElementById("start-link").addEventListener("click", startEndgameTraining);
});