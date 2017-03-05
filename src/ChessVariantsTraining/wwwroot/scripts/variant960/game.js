function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests, wsUrl, isAnti) {
    if (myColor === "") myColor = null;
    var ground;
    var ws;
    var premove = null;
    var currentChatChannel = isPlayer ? "player" : "spectator";
    var chats = { player: [], spectator: [] };
    var clockInfo = { whiteUpdate: NaN, blackUpdate: NaN, whiteValue: NaN, blackValue: NaN, which: null };
    var clocksStarted = false;
    var stopClocks = false;
    var latestFlagRequest = 0;

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
        if (document.getElementById("switch-to-players")) {
            document.getElementById("switch-to-players").addEventListener("click", switchToPlayersChat);
            document.getElementById("switch-to-spectators").addEventListener("click", switchToSpectatorsChat);
        }

        jsonXhr("/Variant960/Game/StoreAnonymousIdentifier", "POST", null, function (req, jsonResponse) {
            ws = new WebSocket(wsUrl);
            ws.addEventListener("open", wsOpened);
            ws.addEventListener("message", wsMessageReceived);
        },
        function (req, err) {
            displayError(err);
        });

    });

    function wsOpened() {
        ws.send(JSON.stringify({ "t": "syncClock" }));
        ws.send(JSON.stringify({ "t": "syncChat" }));
    }

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
                    stopClockTicking();
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
                updateClockValue("white", message.clock.white);
                updateClockValue("black", message.clock.black);
                if (message.plies > 1) {
                    clockInfo.which = message.turnColor;
                    if (!clocksStarted) {
                        clocksStarted = true;
                        requestAnimationFrame(clockTick);
                    }
                }
                break;
            case "chat":
                chats[message.channel].push(message.msg);
                if (message.channel === currentChatChannel) {
                    var msgDiv = document.createElement("div");
                    msgDiv.innerHTML = message.msg;
                    document.getElementById("chat-content").appendChild(msgDiv);
                }
                break;
            case "clock":
                updateClockValue("white", message.white);
                updateClockValue("black", message.black);
                break;
            case "error":
                displayError(message.d);
                break;
            case "outcome":
                document.getElementById("game-result").textContent = message.outcome;
                stopClockTicking();
                break;
            case "chatSync":
                if (message.player) {
                    chats.player = message.player;
                }
                if (message.spectator) {
                    chats.spectator = message.spectator;
                }
                var placeholderEvent = { preventDefault: function () { } };
                if (currentChatChannel === "player") switchToPlayersChat(placeholderEvent);
                else switchToSpectatorsChat(placeholderEvent);
                break;
        }
    }

    function sendMoveMessage(orig, dest, promotion) {
        if (promotion) {
            ws.send(JSON.stringify({ "t": "move", "d": orig + '-' + dest + '-' + promotion.toLowerCase() }));
        } else {
            ws.send(JSON.stringify({ "t": "move", "d": orig + '-' + dest }));
        }
    }

    function pieceMoved(origin, destination, metadata) {
        if (ChessgroundExtensions.needsPromotion(ground, destination)) {
            ChessgroundExtensions.drawPromotionDialog(origin, destination, document.getElementById("chessground"), sendMoveMessage, ground, isAnti);
        } else {
            sendMoveMessage(origin, destination, null);
        }
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

    function updateClockValue(which, seconds) {
        var latestUpdate = new Date();
        clockInfo[which + "Update"] = latestUpdate;
        clockInfo[which + "Value"] = seconds;
        updateClockElement(which);
    }

    function updateClockElement(which) {
        document.getElementById(which + "-clock").textContent = clockDisplay(clockInfo[which + "Value"]);
    }

    function clockDisplay(time) {
        var minutes = Math.floor(time / 60);
        var seconds = Math.floor((time % 60) * 10) / 10;
        return minutes + ":" + (seconds < 10 ? "0" + seconds.toFixed(1) : seconds.toFixed(1));
    }

    function clockTick() {
        if (stopClocks) return;
        var which = clockInfo.which;
        var latestUpdate = new Date();
        clockInfo[which + "Value"] = clockInfo[which + "Value"] - (latestUpdate - clockInfo[which + "Update"]) / 1000;
        clockInfo[which + "Update"] = latestUpdate;
        if (clockInfo[which + "Value"] <= 0 && (new Date() - latestFlagRequest) > 500) {
            latestFlagRequest = new Date();
            ws.send(JSON.stringify({ "t": "flag", "d": which }));
        }
        updateClockElement(which);
        requestAnimationFrame(clockTick);
    }

    function stopClockTicking() {
        stopClocks = true;
    }

    function switchToPlayersChat(e) {
        e = e || window.event;
        e.preventDefault();

        document.getElementById("chat-content").innerHTML = "";
        currentChatChannel = "player";
        var elem;
        for (var i = 0; i < chats.player.length; i++) {
            elem = document.createElement("div");
            elem.innerHTML = chats.player[i];
            document.getElementById("chat-content").appendChild(elem);
        }

        if (document.getElementById("switch-to-players")) {
            document.getElementById("switch-to-players").classList.add("selected-chat");
            document.getElementById("switch-to-spectators").classList.remove("selected-chat");
        }
    }

    function switchToSpectatorsChat(e) {
        e = e || window.event;
        e.preventDefault();

        document.getElementById("chat-content").innerHTML = "";
        currentChatChannel = "spectator";
        var elem;
        for (var i = 0; i < chats.spectator.length; i++) {
            elem = document.createElement("div");
            elem.innerHTML = chats.spectator[i];
            document.getElementById("chat-content").appendChild(elem);
        }

        if (document.getElementById("switch-to-players")) {
            document.getElementById("switch-to-players").classList.remove("selected-chat");
            document.getElementById("switch-to-spectators").classList.add("selected-chat");
        }
    }
}