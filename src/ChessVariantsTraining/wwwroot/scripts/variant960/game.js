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
                dests: dests
            },
            drawable: {
                enabled: true
            }
        });

        ws = new WebSocket(wsUrl);
        ws.addEventListener("message", wsMessageReceived);
    });

    function wsMessageReceived(e) {
        var message = JSON.parse(e.data);
    }
}