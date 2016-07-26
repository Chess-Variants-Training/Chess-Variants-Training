﻿var ChessgroundExtensions = {
    needsPromotion: function (ground, dest) {
        return (dest[1] === "8" || dest[1] === "1") && ground.getPieces()[dest]["role"] === "pawn";
    },
    drawPromotionDialog: function (origin, destination, element, pieceSelected) {
        var promotionChoiceElement = document.createElement("div");
        promotionChoiceElement.id = "promotion-choice";
        element.appendChild(promotionChoiceElement);
        var file = destination.charAt(0);
        var rank = parseInt(destination.charAt(1), 10);
        var color = rank === 1 ? "black" : "white";
        for (var i = 0; i < 4; i++) {
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
            }
            promotionPiece["color"] = color;
            var left = (file.charCodeAt(0) - 97) * 12.5;
            if (color === "black") {
                left = 87.5 - left;
            }
            var top = i * 12.5;
            var square = document.createElement("square");
            square.style.left = left + "%";
            square.style.top = top + "%";
            var piece = document.createElement("piece");
            piece.classList.add(promotionPiece.role);
            piece.classList.add(promotionPiece.color);
            piece.dataset.role = promotionPiece.role;
            piece.addEventListener("click", function (e) {
                e.stopPropagation();
                ChessgroundExtensions.removePromotionDialog(element);
                pieceSelected(origin, destination, e.target.dataset.role);
            });
            square.appendChild(piece);
            promotionChoiceElement.appendChild(square);
        }
    },
    removePromotionDialog: function (element) {
        element.removeChild(document.getElementById("promotion-choice"));
    }
};