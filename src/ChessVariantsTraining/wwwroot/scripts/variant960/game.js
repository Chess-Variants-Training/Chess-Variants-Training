function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests, lastMove, check, wsUrl, shortVariant, replayFensInitial, replayMovesInitial, replayChecksInitial, pocket, replayPocketInitial) {
    var isAnti = shortVariant === "Antichess";
    var isRacingKings = shortVariant === "RacingKings";
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
    var closing = false;
    var replayFens = replayFensInitial.slice();
    var replayMoves = replayMovesInitial.slice();
    var replayChecks = replayChecksInitial.slice();
    var replayPocket = replayPocketInitial ? replayPocketInitial.slice() : null;
    var latestDests = dests;
    var currentReplayItem = replayFens.length - 1;
    var needsReplayWarning = false;
    var isZh = shortVariant === "Crazyhouse";


    var soundTurnedOn = localStorage.getItem("sound") === "yes";

    var soundBasePath = "/sound/sfx/";
    var sounds = {
        genericNotify: new Howl({ src: [soundBasePath + "GenericNotify.mp3", soundBasePath + "GenericNotify.ogg"] }),
        move: new Howl({ src: [soundBasePath + "Move.mp3", soundBasePath + "Move.ogg"] }),
        capture: new Howl({ src: [soundBasePath + "Capture.mp3", soundBasePath + "Capture.ogg"] }),
        explosion: new Howl({ src: [soundBasePath + "Explosion.mp3", soundBasePath + "Explosion.ogg"] })
    };
    sounds.genericNotify.once('load', function () {
        if (soundTurnedOn && isPlayer && !isFinished && replayFens.length < 3) {
            sounds.genericNotify.play();
        }
    });
    function atLastReplayItem() {
        return currentReplayItem + 1 === replayFens.length;
    }

    window.addEventListener("load", function () {
        function renderSoundText() {
            if (soundTurnedOn) {
                document.getElementById("sound-toggle-text").innerHTML = "&#x1f50a;";
            } else {
                document.getElementById("sound-toggle-text").innerHTML = "&#x1f507;";
            }
        }
        renderSoundText();
        document.getElementById("sound-toggle").addEventListener("click", function () {
            soundTurnedOn = !soundTurnedOn;
            localStorage.setItem("sound", soundTurnedOn ? "yes" : "no");
            renderSoundText();
        });
        ground = Chessground(document.getElementById("chessground"), {
            fen: fen,
            coordinates: true,
            orientation: isRacingKings ? "white" : myColor || "white",
            turnColor: whoseTurn,
            lastMove: lastMove,
            viewOnly: !isPlayer || isFinished,
            autoCastle: false,
            movable: {
                free: false,
                dropOff: "revert",
                showDests: false,
                dests: dests,
                color: myColor,
                rookCastle: false,
                events: {
                    after: pieceMoved,
                    afterNewPiece: pieceDropped
                }
            },
            drawable: {
                enabled: true
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
        ChessgroundExtensions.setCheck(ground, check);

        window.addEventListener("beforeunload", function () {
            closing = true;
        });

        document.getElementById("chat-input").addEventListener("keydown", chatKeyDown);
        if (document.getElementById("switch-to-players")) {
            document.getElementById("switch-to-players").addEventListener("click", switchToPlayersChat);
            document.getElementById("switch-to-spectators").addEventListener("click", switchToSpectatorsChat);
        }
        document.getElementById("flip-board").addEventListener("click", flipBoard);

        if (isPlayer) {
            if (document.getElementById("abort-link")) {
                document.getElementById("abort-link").addEventListener("click", abort);
            }
            if (!isFinished) {
                document.getElementById("draw-offer-link").addEventListener("click", offerDraw);
                document.getElementById("draw-accept").addEventListener("click", acceptDraw);
                document.getElementById("draw-decline").addEventListener("click", declineDraw);
                document.getElementById("resign-link").addEventListener("click", resign);


                var pocketPieces = document.querySelectorAll(".pocket-piece");
                for (i = 0; i < pocketPieces.length; i++) {
                    pocketPieces[i].classList.add("draggable");
                    pocketPieces[i].addEventListener("mousedown", startDragNewPiece);
                    pocketPieces[i].addEventListener("touchstart", startDragNewPiece);
                }
            }
            document.getElementById("rematch-offer-link").addEventListener("click", offerRematch);
            document.getElementById("rematch-accept").addEventListener("click", acceptRematch);
            document.getElementById("rematch-decline").addEventListener("click", declineRematch);
            document.getElementById("rematch-cancel").addEventListener("click", cancelRematch);
        }
        document.getElementById("controls-begin").addEventListener("click", replayControlClickedBegin);
        document.getElementById("controls-prev").addEventListener("click", replayControlClickedPrev);
        document.getElementById("controls-next").addEventListener("click", replayControlClickedNext);
        document.getElementById("controls-end").addEventListener("click", replayControlClickedEnd);

        jsonXhr("/Variant960/Game/StoreAnonymousIdentifier", "POST", null, function (req, jsonResponse) {
            ws = new WebSocket(wsUrl);
            ws.addEventListener("open", wsOpened);
            ws.addEventListener("message", wsMessageReceived);
            ws.addEventListener("close", wsClosed);
        },
            function (req, err) {
                displayError(err);
            });

        updatePocketCounters();
    });

    function wsOpened() {
        ws.send(JSON.stringify({ "t": "syncClock" }));
        ws.send(JSON.stringify({ "t": "syncChat" }));
        setInterval(function () {
            ws.send(JSON.stringify({ "t": "keepAlive" }));
        }, 20 * 1000);
    }

    function wsClosed() {
        if (!closing) {
            displayError("WebSocket closed! Please reload the page (there is no auto-reconnect yet)");
        }
    }

    function wsMessageReceived(e) {
        var message = JSON.parse(e.data);
        switch (message.t) {
            case "invalidDrop":
                var pieceSet = {};
                pieceSet[pos] = null;
                ground.setPieces(pieceSet);

                ground.set({ turnColor: ground.state.turnColor === "white" ? "black" : "white" });
                break;
            case "moved":
                if (atLastReplayItem()) {
                    if (soundTurnedOn) {
                        if (message.isCapture) {
                            if (shortVariant === "Atomic" || shortVariant.startsWith("Atomar")) {
                                sounds.explosion.play();
                            } else {
                                sounds.capture.play();
                            }
                        } else {
                            sounds.move.play();
                        }
                    }
                    currentReplayItem++;
                    ground.set({
                        fen: message.fen,
                        lastMove: message.lastMove,
                        turnColor: message.turnColor,
                        movable: {
                            dests: message.dests
                        }
                    });
                }
                replayFens.push(message.fen);
                replayChecks.push(message.check);
                replayMoves.push(message.lastMove[0] + "-" + message.lastMove[1]);
                if (message.pocket) {
                    replayPocket.push(message.pocket);
                }
                latestDests = message.dests;
                needsReplayWarning = isPlayer && myColor === message.turnColor;
                if (!atLastReplayItem() && needsReplayWarning) {
                    document.getElementById("controls-end").classList.add("orange-bg");
                }
                ChessgroundExtensions.setCheck(ground, message.check);
                if (message.outcome) {
                    gotOutcome(message.outcome, message.termination);
                }

                if (isPlayer && myColor === message.turnColor && premove && !message.outcome) {
                    ws.send(JSON.stringify({ "t": "premove", "d": premove.origin + '-' + premove.destination }));
                    premoveUnset();
                    ground.cancelPremove();
                }
                updateClockValue("white", message.clock.white);
                updateClockValue("black", message.clock.black);
                if (message.plies > 1) {
                    if (document.getElementById("abort-link")) {
                        document.getElementById("abort-link").classList.add("nodisplay");
                    }
                    clockInfo.which = message.turnColor;
                    if (!clocksStarted) {
                        clocksStarted = true;
                        requestAnimationFrame(clockTick);
                    }
                }
                if (message.additional) {
                    document.getElementById("additional-info").textContent = message.additional;
                }
                if (message.pocket) {
                    pocket = message.pocket;
                    updatePocketCounters();
                }
                document.getElementById("pgn-moves").textContent = message.pgn;
                break;
            case "chat":
                chats[message.channel].push(message.msg);
                if (message.channel === currentChatChannel) {
                    var msgDiv = document.createElement("div");
                    msgDiv.innerHTML = message.msg;
                    var chatContent = document.getElementById("chat-content");
                    chatContent.appendChild(msgDiv);
                    chatContent.scrollTop = chatContent.scrollHeight;
                }
                break;
            case "clock":
                updateClockValue("white", message.white);
                updateClockValue("black", message.black);
                if (!clocksStarted && message.run) {
                    clockInfo.which = message.whoseTurn;
                    clocksStarted = true;
                    requestAnimationFrame(clockTick);
                }
                break;
            case "error":
                displayError(message.d);
                break;
            case "outcome":
                gotOutcome(message.outcome, message.termination);
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
            case "rematch":
                var loc = "/Variant960/Game/" + message.d;
                document.getElementById("view-rematch").classList.remove("nodisplay");
                document.getElementById("view-rematch").setAttribute("href", loc);
                if (isPlayer) {
                    closing = true;
                    ws.close();
                    window.location.assign(loc);
                }
                break;
            case "rematch-offer":
                document.getElementById("rematch-offer").classList.add("nodisplay");
                document.getElementById("rematch-offer-received").classList.remove("nodisplay");
                break;
            case "rematch-decline":
                document.getElementById("rematch-offer").classList.remove("nodisplay");
                document.getElementById("rematch-offer-sent").classList.add("nodisplay");
                document.getElementById("rematch-offer-received").classList.add("nodisplay");
                break;
            case "draw-offer":
                document.getElementById("draw-offer").classList.add("nodisplay");
                document.getElementById("draw-offer-received").classList.remove("nodisplay");
                break;
            case "draw-decline":
                document.getElementById("draw-offer").classList.remove("nodisplay");
                document.getElementById("draw-offer-sent").classList.add("nodisplay");
        }
    }

    function gotOutcome(outcome, termination) {
        needsReplayWarning = false;
        document.getElementById("game-result").textContent = outcome;
        document.getElementById("game-termination").textContent = termination;
        stopClockTicking();
        if (isPlayer) {
            document.getElementById("chat-header").innerHTML = '<a href="#" id="switch-to-players" class="selected-chat">Players\' chat</a> | <a href="#" id="switch-to-spectators">Spectators\' chat</a>';
            document.getElementById("switch-to-players").addEventListener("click", switchToPlayersChat);
            document.getElementById("switch-to-spectators").addEventListener("click", switchToSpectatorsChat);
            document.getElementById("rematch-offer").classList.remove("nodisplay");
            document.getElementById("resign-link").classList.add("nodisplay");
            if (document.getElementById("abort-link")) {
                document.getElementById("abort-link").classList.add("nodisplay");
            }
            document.getElementById("draw-offer").classList.add("nodisplay");
            document.getElementById("draw-offer-sent").classList.add("nodisplay");
            document.getElementById("draw-offer-received").classList.add("nodisplay");
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

    function pieceDropped(role, pos) {
        ws.send(JSON.stringify({ "t": "move", "d": (role === "knight" ? "N" : role[0].toUpperCase()) + "@" + pos }));
    }

    function premoveSet(orig, dest) {
        premove = { origin: orig, destination: dest };
    }

    function premoveUnset() {
        premove = null;
    }

    function updatePocketCounters() {
        if (!pocket) return;

        var keys = Object.keys(pocket);

        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            var counterElement = document.querySelector("span[data-counter-for='" + key + "']");
            counterElement.textContent = pocket[key].toString();

            var role = key.split('-')[1];
            var color = key.split('-')[0];
            if (pocket[key] === 0) {
                document.querySelector(".pocket-piece[data-role=" + role + "][data-color=" + color + "]").classList.add("unavailable");
            } else {
                document.querySelector(".pocket-piece[data-role=" + role + "][data-color=" + color + "]").classList.remove("unavailable");
            }
        }
    }

    function startDragNewPiece(e) {
        e = e || window.event;
        var role = e.target.dataset.role;
        var color = e.target.dataset.color;

        if (isZh && isPlayer && atLastReplayItem() && myColor === ground.state.turnColor && color === myColor && pocket[color + "-" + role] > 0) {
            ground.dragNewPiece({ color: color, role: role }, e);
        }
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
    // opponent's color
    function opponent(which) {
        if (which == "white") 
            return "black";
        else return "white";
    }
    function updateClockElement(which) {
        document.getElementById(which + "-clock").textContent = clockDisplay(clockInfo[which + "Value"]);
        document.getElementById(opponent(which) + "-clock").className = "clocks";
        document.getElementById(which + "-clock").className = "clocks turn";
        if (clockInfo[which + "Value"] < 20) {
            document.getElementById(which + "-clock").style.color = "red";
        } else {
            document.getElementById(which + "-clock").style.color = "";
        }
    }

    function clockDisplay(time) {
        time = Math.max(0, time);
        var minutes = Math.floor(time / 60);
        var seconds = Math.floor(time % 60 * 10) / 10;
        return minutes + ":" + (seconds < 10 ? "0" + seconds.toFixed(1) : seconds.toFixed(1));
    }

    function clockTick() {
        if (stopClocks) return;
        var which = clockInfo.which;
        var latestUpdate = new Date();
        clockInfo[which + "Value"] = clockInfo[which + "Value"] - (latestUpdate - clockInfo[which + "Update"]) / 1000;
        clockInfo[which + "Update"] = latestUpdate;
        if (clockInfo[which + "Value"] <= 0 && new Date() - latestFlagRequest > 500) {
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

    function offerRematch(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "rematch-offer" }));
        document.getElementById("rematch-offer").classList.add("nodisplay");
        document.getElementById("rematch-offer-sent").classList.remove("nodisplay");
    }

    function acceptRematch(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "rematch-yes" }));
    }

    function declineRematch(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "rematch-no" }));
        document.getElementById("rematch-offer-received").classList.add("nodisplay");
        document.getElementById("rematch-offer").classList.remove("nodisplay");
    }

    function cancelRematch(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "rematch-no" }));
        document.getElementById("rematch-offer-sent").classList.add("nodisplay");
        document.getElementById("rematch-offer").classList.remove("nodisplay");
    }

    function resign(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "resign" }));
    }

    function abort(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "abort" }));
    }

    function offerDraw(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "draw-offer" }));
        document.getElementById("draw-offer").classList.add("nodisplay");
        document.getElementById("draw-offer-sent").classList.remove("nodisplay");
    }

    function acceptDraw(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "draw-yes" }));
    }

    function declineDraw(e) {
        e = e || window.event;
        e.preventDefault();

        ws.send(JSON.stringify({ "t": "draw-no" }));
        document.getElementById("draw-offer").classList.remove("nodisplay");
        document.getElementById("draw-offer-received").classList.add("nodisplay");
    }

    function replayControlClickedBegin(e) {
        currentReplayItem = 0;
        updateBoardAfterReplayStateChanged();
    }

    function replayControlClickedPrev(e) {
        if (currentReplayItem === 0) return;

        currentReplayItem--;
        updateBoardAfterReplayStateChanged();
    }

    function replayControlClickedNext(e) {
        if (atLastReplayItem()) return;

        currentReplayItem++;
        updateBoardAfterReplayStateChanged();
    }

    function replayControlClickedEnd(e) {
        if (atLastReplayItem()) return;

        currentReplayItem = replayFens.length - 1;
        updateBoardAfterReplayStateChanged();
    }

    function updateBoardAfterReplayStateChanged() {
        var destsToUse = atLastReplayItem() ? latestDests : {};
        ground.set({
            fen: replayFens[currentReplayItem],
            turnColor: replayFens.length % 2 === 0 ? 'black' : 'white',
            lastMove: replayMoves[currentReplayItem] ? replayMoves[currentReplayItem].split("-") : null,
            movable: {
                dests: destsToUse
            }
        });
        ChessgroundExtensions.setCheck(ground, replayChecks[currentReplayItem]);

        if (atLastReplayItem()) {
            document.getElementById("controls-end").classList.remove("orange-bg");
        } else if (needsReplayWarning) {
            document.getElementById("controls-end").classList.add("orange-bg");
        }

        if (replayPocket) {
            pocket = replayPocket[currentReplayItem];
            updatePocketCounters();
        }
    }

    function flipBoard(e) {
        e.preventDefault();

        ground.toggleOrientation();
    }
}