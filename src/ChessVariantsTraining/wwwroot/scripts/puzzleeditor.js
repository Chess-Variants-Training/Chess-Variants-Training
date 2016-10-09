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
    if (selected.classList.contains("selected")) {
        return;
    }
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

function applyFen(e) {
    e = e || window.event;
    e.preventDefault();

    var fen = document.getElementById("fenInput").value;
    var parts = fen.trim().split(" ");
    var position = parts[0];
    var whoseTurn = parts.length >= 2 ? parts[1] : null;
    var castlingRights = parts.length >= 3 ? parts[2] : null;
    window.ground.set({ fen: position });
    if (whoseTurn) {
        document.getElementById(whoseTurn === "w" ? "whitetomove" : "blacktomove").checked = true;
        document.getElementById(whoseTurn === "b" ? "whitetomove" : "blacktomove").checked = false;
    }
    if (castlingRights) {
        document.getElementById("whitecastlekingside").checked = castlingRights.indexOf("K") !== -1;
        document.getElementById("whitecastlequeenside").checked = castlingRights.indexOf("Q") !== -1;
        document.getElementById("blackcastlekingside").checked = castlingRights.indexOf("k") !== -1;
        document.getElementById("blackcastlequeenside").checked = castlingRights.indexOf("q") !== -1;
    }
    updateFen();
}

function goToStep2(e) {
    e = e || window.event;
    e.preventDefault();
    window.variant = document.getElementById("variantSelector").value;
    if (variant === "none") {
        alert("Please select a variant.");
        return;
    }
    clearSelection(e);

    jsonXhr("/Puzzle/Editor/RegisterPuzzleForEditing", "POST",
        "fen=" + encodeURIComponent(document.getElementById("fen").innerHTML) + "&variant=" + window.variant, function (req, jsonResponse) {
            window.currentVariation = 0;
            var step1Elements = document.getElementsByClassName("step1");
            for (var i = 0; i < step1Elements.length; i++) {
                step1Elements[i].setAttribute("class", "step1 hidden");
            }
            var step2Elements = document.getElementsByClassName("step2");
            for (i = 0; i < step2Elements.length; i++) {
                step2Elements[i].setAttribute("class", "step2");
            }

            window.puzzleId = jsonResponse["id"];
            var whoseTurn = document.getElementById("fen").innerHTML.split(" ")[1] === "w" ? "white" : "black";
            var orientation = whoseTurn === "white" || window.variant === "RacingKings" ? "white" : "black";
            window.ground.set({
                orientation: orientation,
                movable: {
                    free: false,
                    showDests: false,
                    events: {
                        after: submitMove
                    },
                    dropOff: "revert"
                }
            });
            updateChessGroundValidMoves();
        }, function (req, err) {
            displayError(err);
        });
}

function submitMove(orig, dest, metadata) {
    if (ChessgroundExtensions.needsPromotion(window.ground, dest)) {
        ChessgroundExtensions.drawPromotionDialog(orig, dest, document.getElementById("chessground"), doSubmitMoveRequest, window.ground, window.variant === "Antichess");
    } else {
        doSubmitMoveRequest(orig, dest, null);
    }
}

function doSubmitMoveRequest(orig, dest, promotion) {
    document.getElementById("variations").children[window.currentVariation].innerHTML += " " + orig + "-" + dest;
    if (promotion) {
        document.getElementById("variations").children[window.currentVariation].innerHTML += "=" + (promotion !== "knight" ? promotion.charAt(0).toUpperCase() : "N");
    }
    jsonXhr("/Puzzle/Editor/SubmitMove", "POST", "id=" + window.puzzleId + "&origin=" + orig + "&destination=" + dest + (promotion ? "&promotion=" + promotion : ""), function (req, jsonResponse) {
        window.ground.set({
            fen: jsonResponse["fen"]
        });
        updateChessGroundValidMoves();
    }, function (req, err) {
        displayError(err);
    });
}

function updateChessGroundValidMoves() {
    jsonXhr("/Puzzle/Editor/GetValidMoves/" + window.puzzleId, "GET", null, function (req, jsonResponse) {
        window.ground.set({
            turnColor: jsonResponse["whoseturn"],
            movable: {
                dests: jsonResponse["dests"],
                color: jsonResponse["whoseturn"]
            }
        });
    }, function (req, err) {
        displayError(err);
    });
}

function submitPuzzle(e) {
    e = e || window.event;
    e.preventDefault();
    var solutions = [];
    for (var i = 0; i <= window.currentVariation; i++) {
        solutions.push(document.getElementById("variations").children[i].textContent.trim());
    }
    var solution = solutions.join(';');
    jsonXhr("/Puzzle/Editor/Submit", "POST", "id=" + window.puzzleId + "&solution=" + encodeURIComponent(solution.trim()) +
        "&explanation=" + encodeURIComponent(document.getElementById("puzzleExplanation").value), function (req, jsonResponse) {
            window.location.href = jsonResponse["link"];
        }, function (req, err) {
            displayError(err);
        });
}

function addAnotherVariation(e) {
    e = e || window.event;
    e.preventDefault();

    jsonXhr("/Puzzle/Editor/NewVariation", "POST", "id=" + window.puzzleId, function (req, jsonResponse) {
        window.currentVariation++;
        var li = document.createElement("li");
        document.getElementById("variations").appendChild(li);
        window.ground.set({
            fen: jsonResponse["fen"]
        });
        updateChessGroundValidMoves();
    }, function (req, err) {
        displayError(err);
    });
}

function deleteVariation(e) {
    e = e || window.event;
    e.preventDefault();

    var num = parseInt(document.getElementById("variationToDelete").value, 10);
    if (isNaN(num)) {
        return;
    }

    var id = num - 1;
    document.getElementById("variations").children[id].remove();

    if (window.currentVariation === id) {
        window.currentVariation--;
        addAnotherVariation({ preventDefault: function () { } });
    } else {
        window.currentVariation--;
    }
}

function clearBoard(e) {
    e = e || window.event;
    e.preventDefault();
    window.ground.set({ fen: "8/8/8/8/8/8/8/8 w - -" });
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
    document.getElementById("clearboard").addEventListener("click", clearBoard);

    var whoseTurnRadioButtons = document.querySelectorAll("[name=whoseturn]");
    for (i = 0; i < whoseTurnRadioButtons.length; i++) {
        whoseTurnRadioButtons[i].addEventListener("click", updateFen);
    }

    var castlingCheckboxes = document.querySelectorAll("[name=castling]");
    for (i = 0; i < castlingCheckboxes.length; i++) {
        castlingCheckboxes[i].addEventListener("click", updateFen);
    }

    document.getElementById("applyFenInput").addEventListener("click", applyFen);

    document.getElementById("gotostep2").addEventListener("click", goToStep2);
    document.getElementById("submitpuzzle").addEventListener("click", submitPuzzle);
    document.getElementById("addVariation").addEventListener("click", addAnotherVariation);
    document.getElementById("deleteVariation").addEventListener("click", deleteVariation);
});