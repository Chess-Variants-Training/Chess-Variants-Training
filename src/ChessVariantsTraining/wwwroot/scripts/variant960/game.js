function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests, wsUrl) {
    if (myColor === "") myColor = null;
    var ground;
    var ws;

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
                break;
        }
    }

    function pieceMoved(orig, dest, metadata) {
        ws.send(JSON.stringify({ "t": "move", "d": orig + '-' + dest }));
    }
}