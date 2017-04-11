var ChessgroundExtensions = {
    needsPromotion: function (ground, dest) {
        return (dest[1] === "8" || dest[1] === "1") && ground.state.pieces[dest]["role"] === "pawn";
    },
    drawPromotionDialog: function (origin, destination, element, pieceSelected, ground, addKing) {
        var promotionChoiceElement = document.createElement("div");
        promotionChoiceElement.id = "promotion-choice";
        element.querySelector(".cg-board").insertBefore(promotionChoiceElement, element.querySelector(".cg-board").firstChild);
        var file = destination.charAt(0);
        var rank = parseInt(destination.charAt(1), 10);
        var color = rank === 1 ? "black" : "white";
        for (var i = 0; i < 4 + (addKing ? 1 : 0); i++) {
            switch (i) {
                case 0:
                    var promotionPiece = { role: "queen" };
                    break;
                case 1:
                    promotionPiece = { role: "knight" };
                    break;
                case 2:
                    promotionPiece = { role: "rook" };
                    break;
                case 3:
                    promotionPiece = { role: "bishop" };
                    break;
                case 4:
                    promotionPiece = { role: "king" };
                    break;
            }
            promotionPiece["color"] = color;
            var left = (file.charCodeAt(0) - 97) * 12.5;
            if (ground.state.orientation === "black") {
                left = 87.5 - left;
            }
            var top = ground.state.orientation === color ? i * 12.5 : (7 - i) * 12.5;
            var square = document.createElement("square");
            square.style.left = left + "%";
            square.style.top = top + "%";
            square.style.zIndex = 3;
            var piece = document.createElement("piece");
            piece.classList.add(promotionPiece.role);
            piece.classList.add(promotionPiece.color);
            piece.style.width = "100%";
            piece.style.height = "100%";
            piece.dataset.role = promotionPiece.role;
            piece.addEventListener("mousedown", function (e) {
                e.stopPropagation();
                ChessgroundExtensions.removePromotionDialog(element);
                pieceSelected(origin, destination, ChessgroundExtensions.pieceNameToPieceChar(e.target.dataset.role));
            });
            square.appendChild(piece);
            promotionChoiceElement.appendChild(square);
        }
    },
    removePromotionDialog: function (element) {
        element.querySelector(".cg-board").removeChild(document.getElementById("promotion-choice"));
    },
    pieceNameToPieceChar: function (name) {
        switch (name) {
            case "queen":
                return "Q";
            case "rook":
                return "R";
            case "bishop":
                return "B";
            case "knight":
                return "N";
            case "king":
                return "K";
        }
    },
    setCheck: function (ground, color) {
        if (!color) {
            ground.set({ check: null });
        } else {
            for (var square in ground.state.pieces) {
                var piece = ground.state.pieces[square];
                if (piece.role === "king" && piece.color === color) {
                    ground.set({ check: square });
                }
            }
        }
    }
};