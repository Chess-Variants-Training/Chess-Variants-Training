(function main() {
    var ground;
    window.addEventListener("load", function () {
        ground = Chessground(document.getElementById("chessground"), {
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
})();