function updateFen() {
    var fenStr = window.ground.getFen();
    if (document.getElementById("whitetomove").checked) {
        fenStr += " w ";
    } else {
        fenStr += " b ";
    }
    var castlingStr = "";
    if (document.getElementById("whitecastlekingside").checked) castlingStr += "K";
    if (document.getElementById("whitecastlequeenside").checked) castlingStr += "Q";
    if (document.getElementById("blackcastlekingside").checked) castlingStr += "k";
    if (document.getElementById("blackcastlequeenside").checked) castlingStr += "q";
    if (castlingStr === "") castlingStr = "-";
    fenStr += castlingStr;
    document.getElementById("fen").innerHTML = fenStr;
}

function dropPiece(dest) {
    if (!window.selectedPiece) return;
    var piecesChange = {};
    piecesChange[dest] = window.selectedPiece;
    window.ground.setPieces(piecesChange);
    updateFen();
}

function pieceSelected(e) {
    e = e || window.event;
    var selected = e.target;
    selected.setAttribute("class", "selectable selected");
    if (window.selectedPieceElement) {
        window.selectedPieceElement.setAttribute("class", "selectable");
    }
    var color = selected.getAttribute("data-color");
    var role = selected.getAttribute("data-role");
    window.selectedPiece = { color: color, role: role };
    window.selectedPieceElement = selected;
}

function clearSelection(e) {
    e = e || window.event;
    e.preventDefault();
    if (!window.selectedPieceElement) return;
    window.selectedPieceElement.setAttribute("class", "selectable");
    delete window.selectedPiece;
}

window.addEventListener("load", function () {
    window.ground = Chessground(document.getElementById("chessground"), {
        coordinates: false,
        disableContextMenu: true,
        movable: {
            free: true,
            color: "both",
            dropOff: "trash"
        },
        animation: {
            enabled: false
        },
        premovable: {
            enabled: false
        },
        highlight: {
            lastMove: false
        },
        drawable: {
            enabled: true
        },
        events: {
            change: updateFen,
            select: dropPiece
        }
    });
    updateFen();
    var selectable = document.getElementsByClassName("selectable");
    for (var i = 0; i < selectable.length; i++) {
        selectable[i].addEventListener("click", pieceSelected);
    }
    document.getElementById("clearselection").addEventListener("click", clearSelection);

    var whoseTurnRadioButtons = document.querySelectorAll("[name=whoseturn]");
    for (var i = 0; i < whoseTurnRadioButtons.length; i++) {
        whoseTurnRadioButtons[i].addEventListener("click", updateFen);
    }

    var castlingCheckboxes = document.querySelectorAll("[name=castling]");
    for (var i = 0; i < castlingCheckboxes.length; i++) {
        castlingCheckboxes[i].addEventListener("click", updateFen);
    }
});