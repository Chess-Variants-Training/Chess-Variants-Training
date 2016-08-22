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

function goToStep2(e) {
    e = e || window.event;
    clearSelection(e);
    var step1Elements = document.getElementsByClassName("step1");
    for (var i = 0; i < step1Elements.length; i++) {
        step1Elements[i].setAttribute("class", "step1 hidden");
    }
    var step2Elements = document.getElementsByClassName("step2");
    for (var i = 0; i < step2Elements.length; i++) {
        step2Elements[i].setAttribute("class", "step2");
    }
    jsonXhr("/Puzzle/Editor/RegisterPuzzleForEditing", "POST",
        "fen=" + encodeURIComponent(document.getElementById("fen").innerHTML + " - 0 1") + "&variant=" + document.getElementById("variantSelector").value, function (req, jsonResponse) {
            window.puzzleId = jsonResponse["id"];
            var whoseTurn = document.getElementById("fen").innerHTML.split(" ")[1] === "w" ? "white" : "black";
            window.ground.set({
                orientation: whoseTurn,
                movable: {
                    free: false,
                    showDests: false,
                    events: {
                        after: submitMove
                    }
                }
            });
            updateChessGroundValidMoves();
        }, function (req, err) {
            alert(err);
        });
}

function submitMove(orig, dest, metadata) {
    if (ChessgroundExtensions.needsPromotion(window.ground, dest)) {
        ChessgroundExtensions.drawPromotionDialog(orig, dest, document.getElementById("chessground"), doSubmitMoveRequest, window.ground);
    } else {
        doSubmitMoveRequest(orig, dest, null);
    }
}

function doSubmitMoveRequest(orig, dest, promotion) {
    document.getElementById("movelist").innerHTML += " " + orig + "-" + dest;
    if (promotion) {
        document.getElementById("movelist").innerHTML += "=" + (promotion !== "knight" ? promotion.charAt(0).toUpperCase() : "N");
    }
    jsonXhr("/Puzzle/Editor/SubmitMove", "POST", "id=" + window.puzzleId + "&origin=" + orig + "&destination=" + dest + (promotion ? "&promotion=" + promotion : ""), function (req, jsonResponse) {
        window.ground.set({
            fen: jsonResponse["fen"]
        });
        updateChessGroundValidMoves();
    }, function (req, err) {
        alert(err);
    });
}

function updateChessGroundValidMoves() {
    jsonXhr("/Puzzle/Editor/GetValidMoves/" + window.puzzleId, "GET", null, function (req, jsonResponse) {
        window.ground.set({
            turnColor: jsonResponse["whoseturn"],
            movable: {
                dests: jsonResponse["dests"],
                color: jsonResponse["whoseturn"],
            }
        });
    }, function (req, err) {
        alert(err);
    });
}

function submitPuzzle(e) {
    e = e || window.event;
    e.preventDefault();
    var solution = document.getElementById("movelist").innerHTML;
    jsonXhr("/Puzzle/Editor/Submit", "POST", "id=" + window.puzzleId + "&solution=" + encodeURIComponent(solution.trim()) +
        "&explanation=" + encodeURIComponent(document.getElementById("puzzleExplanation").value), function (req, jsonResponse) {
            alert("Success!");
        }, function (req, err) {
            alert(err);
        });
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

    document.getElementById("gotostep2").addEventListener("click", goToStep2);
    document.getElementById("submitpuzzle").addEventListener("click", submitPuzzle);
});