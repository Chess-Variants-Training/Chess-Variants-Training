function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests, wsUrl) {
    if (myColor === "") myColor = null;
    var ground;
    var ws;
    var premove = null;
    var currentChatChannel = isPlayer ? "player" : "spectator";
    var chats = { player: [], spectator: [] };

    window.addEventListener("load", function () {
        ground = Chessground(document.getElementById("chessground"), {
            fen: fen,
            coordinates: true,
            orientation: myColor || "white",
            turnColor: whoseTurn,
            viewOnly: !isPlayer || isFinished,
            movable: {
                free: false,
                dropOff: "revert",
                showDests: false,
                dests: dests,
                color: myColor,
            },
            drawable: {
                enabled: true
            },
            events: {
                move: pieceMoved
            },
            premovable: {
                enabled: true,
                showDests: false,
                events: {
                    set: premoveSet,
                    unset: premoveUnset
                }
            }
        });

        document.getElementById("chat-input").addEventListener("keydown", chatKeyDown);

        jsonXhr("/Variant960/Game/StoreAnonymousIdentifier", "POST", null, function (req, jsonResponse) {
            ws = new WebSocket(wsUrl);
            ws.addEventListener("message", wsMessageReceived);
        },
        function (req, err) {
            displayError(err);
        });

    });

    function wsMessageReceived(e) {
        var message = JSON.parse(e.data);
        switch (message.t) {
            case "moved":
                ground.set({
                    fen: message.fen,
                    lastMove: message.lastMove,
                    turnColor: message.turnColor,
                    movable: {
                        dests: message.dests
                    }
                });
                if (message.outcome) {
                    document.getElementById("game-result").textContent = message.outcome;
                }
                if (isPlayer && myColor == message.turnColor && premove && !message.outcome) {
                    ws.send(JSON.stringify({ "t": "premove", "d": premove.origin + '-' + premove.destination }));
                    premoveUnset();
                    ground.set({
                        premovable: {
                            current: null
                        }
                    });
                }
                document.getElementById("white-clock").textContent = clockDisplay(message.clock.white);
                document.getElementById("black-clock").textContent = clockDisplay(message.clock.black);
                break;
            case "chat":
                chats[message.channel].push(message.msg);
                if (message.channel === currentChatChannel) {
                    var msgDiv = document.createElement("div");
                    msgDiv.innerHTML = message.msg;
                    document.getElementById("chat-content").appendChild(msgDiv);
                }
                break;
            case "error":
                displayError(message.d);
                break;
        }
    }

    function pieceMoved(orig, dest, metadata) {
        ws.send(JSON.stringify({ "t": "move", "d": orig + '-' + dest }));
    }

    function premoveSet(orig, dest) {
        premove = { origin: orig, destination: dest };
    }

    function premoveUnset() {
        premove = null;
    }

    function chatKeyDown(e) {
        e = e || window.event;
        if (e.keyCode !== 13) return;

        var messageToSend = document.getElementById("chat-input").value;
        document.getElementById("chat-input").value = "";
        ws.send(JSON.stringify({ "t": "chat", "d": messageToSend, "channel": currentChatChannel }));
    }

    function clockDisplay(time) {
        var minutes = Math.floor(time / 60);
        var seconds = time % 60;
        if (seconds < 10) {
            var secondsStr = "0" + seconds.toFixed(1);
        } else {
            var secondsStr = seconds.toFixed(1);
        }
        return minutes + ":" + secondsStr;
    }

}