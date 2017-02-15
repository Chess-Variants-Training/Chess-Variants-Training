function main(fen, isPlayer, myColor, whoseTurn, isFinished, dests) {
    if (myColor === "") myColor = null;
    var ground;
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
    });
}