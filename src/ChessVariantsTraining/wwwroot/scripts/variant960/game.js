function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests, wsUrl) {
    if (myColor === "") myColor = null;
    var ground;
    var ws;
    var premove = null;

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

        ws = new WebSocket(wsUrl);
        ws.addEventListener("message", wsMessageReceived);
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
                if (isPlayer && myColor == message.turnColor && premove) {
                    ws.send(JSON.stringify({ "t": "premove", "d": premove.origin + '-' + premove.destination }));
                    premoveUnset();
                    ground.set({
                        premovable: {
                            current: null
                        }
                    });
                }
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
}