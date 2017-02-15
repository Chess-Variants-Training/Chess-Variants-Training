function main(fen) {
    var ground;
    window.addEventListener("load", function () {
        ground = Chessground(document.getElementById("chessground"), {
            fen: fen,
            coordinates: true,
            movable: {
                free: false,
                dropOff: "revert",
                showDests: false
            },
            drawable: {
                enabled: true
            }
        });
    });
}