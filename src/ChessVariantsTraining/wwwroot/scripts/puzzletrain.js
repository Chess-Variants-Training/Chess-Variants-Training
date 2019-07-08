function startWithRandomPuzzle() {
    jsonXhr("/Puzzle/Train/GetOneRandomly/" + window.variant + (window.trainingSessionId ? "?trainingSessionId=" + window.trainingSessionId : ""), "GET", null, function (req, jsonResponse) {
        if (!jsonResponse["allDone"]) {
            setup(jsonResponse.id);
        } else {
            document.getElementById("bodycontainer").innerHTML = "There are no more puzzles for you in this variant. But don't worry! Registered users can submit puzzles, so if you come back here later, there might be new puzzles!";
        }
    }, function (req, err) {
        displayError(err);
    });
}

function setup(puzzleId) {
    jsonXhr("/Puzzle/Train/Setup", "POST", "id=" + puzzleId + (window.trainingSessionId ? "&trainingSessionId=" + window.trainingSessionId : ""), function (req, jsonResponse) {
        window.puzzleId = puzzleId;
        window.replay = null;
        window.currentVariant = jsonResponse.variant;
        window.pocket = jsonResponse.pocket;
        window.yourColor = jsonResponse.whoseTurn;
        updatePocketCounters();
        if (window.currentVariant === "Crazyhouse") {
            document.getElementById("pocket-zh").classList.remove("nodisplay");
        } else {
            document.getElementById("pocket-zh").classList.add("nodisplay");
        }
        window.ground.set({
            fen: jsonResponse.fen,
            check: null,
            orientation: jsonResponse.whoseTurn === "white" || window.currentVariant === "RacingKings" ? "white" : "black",
            turnColor: jsonResponse.whoseTurn,
            lastMove: null,
            selected: null,
            movable: {
                free: false,
                dests: jsonResponse.dests
            }
        });
        if (jsonResponse.check) {
            ChessgroundExtensions.setCheck(window.ground, jsonResponse.check);
        }
        clearExplanation();
        clearPuzzleRating();
        clearRatingChange();
        clearComments();
        document.getElementById("puzzleLinkContainer").setAttribute("class", "nodisplay");
        if (document.getElementById("reportLinkContainer")) {
            document.getElementById("reportLinkContainer").setAttribute("class", "nodisplay");
        }
        if (jsonResponse.additionalInfo) {
            document.getElementById("additionalInfo").textContent = jsonResponse.additionalInfo;
        } else {
            document.getElementById("additionalInfo").textContent = "";
        }
        document.getElementById("result").setAttribute("class", "blue");
        document.getElementById("result").innerHTML = "Find the best move!";
        document.getElementById("author").textContent = jsonResponse.author;
        document.getElementById("author").setAttribute("href", jsonResponse.authorUrl);
        document.getElementById("variantName").textContent = jsonResponse.variant;
        document.getElementById("controls").classList.add("nodisplay");
        document.getElementById("colorToPlay").textContent = jsonResponse.whoseTurn;
        document.getElementById("permalink").setAttribute("href", "/Puzzle/" + window.puzzleId);
        document.getElementById("analysis-board-p").classList.add("nodisplay");
        hideTags();
        setTags(jsonResponse.tags);
        window.trainingSessionId = jsonResponse.trainingSessionId;
        if (window.immediatelyShowComments) {
            loadComments();
        }
    }, function (req, err) {
        displayError(err);
    });
}

function showPuzzleRating(r) {
    document.getElementById("puzzleRating").innerHTML = "Puzzle rating: " + r;
}

function clearPuzzleRating() {
    document.getElementById("puzzleRating").innerHTML = "";
}

function showRatingChange(r) {
    document.getElementById("ratingChange").innerHTML = "Rating change: " + (r >= 0 ? "&plus;" : "&minus;") + Math.abs(r).toString();
}

function clearRatingChange() {
    document.getElementById("ratingChange").innerHTML = "";
}


function addExplanation(expl) {
    document.getElementById("explanation").classList.remove("nodisplay");
    document.getElementById("explanationInner").innerHTML = expl;
}

function toggleExplanation(e) {
    e = e || window.event;
    e.preventDefault();
    if (document.getElementById("toggleExplanationLink").textContent === "[hide]") {
        hideExplanation();
    } else {
        showExplanation();
    }
}

function showExplanation() {
    document.getElementById("explanationInner").classList.remove("nodisplay");
    document.getElementById("toggleExplanationLink").textContent = "[hide]";
}

function hideExplanation() {
    document.getElementById("explanationInner").classList.add("nodisplay");
    document.getElementById("toggleExplanationLink").textContent = "[show]";
}

function clearExplanation() {
    document.getElementById("explanation").classList.add("nodisplay");
    document.getElementById("explanationInner").innerHTML = "";
}

function setTags(tags) {
    if (tags.length === 0) {
        document.getElementById("puzzle-tags").innerHTML = "&lt;none&gt;";
    } else {
        var tagHtmls = [];
        for (var i = 0; i < tags.length; i++) {
            tagHtmls.push("<a href='/Puzzle/Tags/-variant-/-tag-'>-tag-</a>".replace("-variant-", window.variant).replace(/-tag-/g, tags[i]));
        }
        document.getElementById("puzzle-tags").innerHTML = tagHtmls.join(", ");
    }
}

function showTags() {
    document.getElementById("tag-container").classList.remove("nodisplay");
}

function hideTags() {
    document.getElementById("tag-container").classList.add("nodisplay");
}

function processPuzzleMove(origin, destination, metadata) {
    if (ChessgroundExtensions.needsPromotion(window.ground, destination)) {
        ChessgroundExtensions.drawPromotionDialog(origin, destination, document.getElementById("chessground"), submitPuzzleMove, window.ground, window.currentVariant === "Antichess");
    } else {
        submitPuzzleMove(origin, destination, null);
    }
}

function processPuzzleDrop(role, pos) {
    jsonXhr("/Puzzle/Train/SubmitDrop", "POST", "id=" + window.puzzleId + "&trainingSessionId=" + window.trainingSessionId + "&role=" + role + "&pos=" + pos, processResponseAfterMoveOrDrop, function (req, err) {
        displayError(err);
    });
}

function submitPuzzleMove(origin, destination, promotion) {
    jsonXhr("/Puzzle/Train/SubmitMove", "POST", "id=" + window.puzzleId + "&trainingSessionId=" + window.trainingSessionId + "&origin=" + origin + "&destination=" + destination + (promotion ? "&promotion=" + promotion : ""), processResponseAfterMoveOrDrop, function (req, err) {
        displayError(err);
    });
}

function processResponseAfterMoveOrDrop(req, jsonResponse) {
    if (jsonResponse.invalidDrop) {
        var pieceSet = {};
        pieceSet[jsonResponse.pos] = null;
        window.ground.setPieces(pieceSet);

        window.ground.set({ turnColor: window.ground.state.turnColor === "white" ? "black" : "white" });
        return;
    }
    if (jsonResponse.fen) {
        window.ground.set({
            fen: jsonResponse.fen
        });
    }
    if (jsonResponse.check) {
        ChessgroundExtensions.setCheck(window.ground, jsonResponse.check);
    } else {
        ChessgroundExtensions.setCheck(window.ground, null);
    }
    if (jsonResponse.pocket) {
        window.pocket = jsonResponse.pocket;
        updatePocketCounters();
    }
    if (jsonResponse.play) {
        window.ground.set({
            fen: jsonResponse.fenAfterPlay,
            lastMove: jsonResponse.play.indexOf("@") === -1 ? jsonResponse.play.substr(0, 5).split("-") : [ jsonResponse.play.split("@")[1], jsonResponse.play.split("@")[1] ]
        });
        if (jsonResponse.checkAfterAutoMove) {
            ChessgroundExtensions.setCheck(window.ground, jsonResponse.checkAfterAutoMove);
        } else {
            ChessgroundExtensions.setCheck(window.ground, null);
        }
        if (jsonResponse.pocketAfterAutoMove) {
            window.pocket = jsonResponse.pocketAfterAutoMove;
            updatePocketCounters();
        }
    }
    switch (jsonResponse.correct) {
        case 0:
            break;
        case 1:
            document.getElementById("puzzleLinkContainer").classList.remove("nodisplay");
            if (document.getElementById("reportLinkContainer")) {
                document.getElementById("reportLinkContainer").classList.remove("nodisplay");
            }
            document.getElementById("result").textContent = "Success!";
            document.getElementById("result").setAttribute("class", "green");
            window.ground.set({ movable: { dests: [] } });
            loadComments();
            showTags();
            break;
        case -1:
            document.getElementById("puzzleLinkContainer").classList.remove("nodisplay");
            if (document.getElementById("reportLinkContainer")) {
                document.getElementById("reportLinkContainer").classList.remove("nodisplay");
            }
            window.ground.set({ lastMove: null });
            document.getElementById("result").innerHTML = "<div>Puzzle failed.</div><div><small>But you can keep making moves to solve it.</small></div><div><small>Or, if you want to see the solution instead, click the arrows below.</small></div>";
            document.getElementById("result").setAttribute("class", "red");
            loadComments();
            showTags();
    }
    if (jsonResponse.analysisUrl) {
        document.getElementById("analysis-board-p").classList.remove("nodisplay");
        document.getElementById("analysis-board-link").setAttribute("href", jsonResponse.analysisUrl);
    }
    if (jsonResponse.dests && jsonResponse.correct !== 1) {
        window.ground.set({
            movable: {
                dests: jsonResponse.dests
            }
        });
    }
    if (jsonResponse.explanation) {
        addExplanation(jsonResponse.explanation);
    }
    if (jsonResponse.rating) {
        showPuzzleRating(jsonResponse.rating);
    }
    if (jsonResponse.ratingChange) {
        showRatingChange(jsonResponse.ratingChange);
    }
    if (jsonResponse.replayFens) {
        window.replay = {};
        window.replay.fens = jsonResponse.replayFens;
        window.replay.current = window.replay.fens.indexOf(jsonResponse.fen || window.ground.getFen());
        window.replay.checks = jsonResponse.replayChecks;
        window.replay.moves = jsonResponse.replayMoves;
        window.replay.pockets = jsonResponse.replayPockets;
        document.getElementById("controls").classList.remove("nodisplay");
    }
}

function submitComment(e) {
    e = e || window.event;
    e.preventDefault();
    jsonXhr("/Comment/PostComment", "POST", "commentBody=" + encodeURIComponent(document.getElementById("commentBody").value) + "&puzzleId=" + window.puzzleId, function (req, jsonResponse) {
        document.getElementById("commentBody").value = "";
        clearComments();
        loadComments();
    },
    function (req, err) {
        displayError(err);
    });
}

function clearComments() {
    var commentContainer = document.getElementById("commentContainer");
    while (commentContainer.firstChild) {
        commentContainer.removeChild(commentContainer.firstChild);
    }
}

function loadComments() {
    xhr("/Comment/ViewComments/" + window.puzzleId, "GET", null, function (req) {
        document.getElementById("commentContainer").innerHTML = req.responseText;
        var comments = document.getElementById("commentContainer").querySelectorAll(".comment");
        for (var i = 0; i < comments.length; i++) {
            comments[i].style.marginLeft = parseInt(comments[i].dataset.indentlevel, 10) * 1.5 + "%";
        }
        var voteLinks = document.getElementById("commentContainer").querySelectorAll("a[data-vote]");
        for (i = 0; i < voteLinks.length; i++) {
            voteLinks[i].addEventListener("click", voteClicked);
        }
        var replyLinks = document.getElementById("commentContainer").querySelectorAll("a[data-to]");
        for (i = 0; i < replyLinks.length; i++) {
            replyLinks[i].addEventListener("click", replyLinkClicked);
        }
        var sendLinks = document.getElementById("commentContainer").getElementsByClassName("send-reply");
        for (i = 0; i < sendLinks.length; i++) {
            sendLinks[i].addEventListener("click", sendLinkClicked);
        }
        var cancelLinks = document.getElementById("commentContainer").getElementsByClassName("cancel-reply");
        for (i = 0; i < cancelLinks.length; i++) {
            cancelLinks[i].addEventListener("click", cancelLinkClicked);
        }
        var reportLinks = document.getElementById("commentContainer").getElementsByClassName("report-link");
        for (i = 0; i < reportLinks.length; i++) {
            reportLinks[i].addEventListener("click", reportCommentLinkClicked);
        }
        var modLinks = document.getElementById("commentContainer").getElementsByClassName("mod-link");
        for (i = 0; i < modLinks.length; i++) {
            modLinks[i].addEventListener("click", modLinkClicked);
        }
        if (window.location.search !== "") {
            var matches = /[?&]comment=[0-9a-zA-Z_-]+/.exec(window.location.search);
            if (matches) {
                var id = matches[0].slice(9);
                var highlighted = document.getElementById("cmt-" + id);
                if (highlighted) {
                    highlighted.scrollIntoView(true);
                    highlighted.style.backgroundColor = "#feb15a";
                }
            }
        }
    }, function (req, err) {
        displayError(err);
    });
}

function upvoteComment(commentId) {
    jsonXhr("/Comment/Upvote", "POST", "commentId=" + commentId, function (req, jsonResponse) {
        clearComments();
        loadComments();
    }, function (req, err) {
        displayError(err);
    });
}

function downvoteComment(commentId) {
    jsonXhr("/Comment/Downvote", "POST", "commentId=" + commentId, function (req, jsonResponse) {
        clearComments();
        loadComments();
    }, function (req, err) {
        displayError(err);
    });
}

function undoVote(commentId) {
    jsonXhr("/Comment/UndoVote", "POST", "commentId=" + commentId, function (req, jsonResponse) {
        clearComments();
        loadComments();
    }, function (req, err) {
        displayError(err);
    });
}

function sendReply(to, body) {
    jsonXhr("/Comment/Reply", "POST", "to=" + to + "&body=" + encodeURIComponent(body) + "&puzzleId=" + window.puzzleId, function (req, jsonResponse) {
        clearComments();
        loadComments();
    },
    function (req, err) {
        displayError(err);
    });
}

function voteClicked(e) {
    e = e || window.event;
    e.preventDefault();
    var target = e.target;
    if (target.dataset.vote === "up") {
        if (target.classList.contains("upvote-highlighted")) {
            undoVote(target.dataset.commentid);
        } else {
            upvoteComment(target.dataset.commentid);
        }
    } else {
        if (target.classList.contains("downvote-highlighted")) {
            undoVote(target.dataset.commentid);
        } else {
            downvoteComment(target.dataset.commentid);
        }
    }
}

function replyLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    (document.getElementById("to-" + e.target.dataset.to) || { style: {} }).style.display = "block";
}

function sendLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    var to = e.target.parentElement.id.slice(3);
    var body = e.target.parentElement.firstElementChild.value;
    sendReply(to, body);
}

function cancelLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    e.target.parentElement.style.display = "none";
}

function reportCommentLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    var itemToReport = "cmt-" + e.target.dataset.item;
    if (!window.reportCommentDialogHtml) {
        xhr("/Report/Dialog/Comment", "GET", null, function (req) {
            window.reportCommentDialogHtml = req.responseText;
            showReportDialog(itemToReport);
        }, function (req, err) {
            displayError(err);
        });
    }
    else {
        showReportDialog(itemToReport);
    }
}

function showReportDialog(itemToReport) {
    var itemToReportElement = document.getElementById(itemToReport);
    itemToReportElement.getElementsByClassName("comment-content")[0].style.display = "none";
    itemToReportElement.insertAdjacentHTML("beforeend", window.reportCommentDialogHtml);
    itemToReportElement.getElementsByClassName("report-dialog")[0].lastElementChild.addEventListener("click", reportLinkInDialogClicked);
}

function removeReportDialog(itemReported) {
    var itemReportedElement = document.getElementById(itemReported);
    itemReportedElement.getElementsByClassName("comment-content")[0].style.display = "flex";
    itemReportedElement.removeChild(itemReportedElement.getElementsByClassName("report-dialog")[0]);
}

function reportLinkInDialogClicked(e) {
    e = e || window.event;
    e.preventDefault();
    var parent = e.target.parentElement;
    jsonXhr("/Report/Submit/Comment", "POST", "item=" + parent.parentElement.dataset.commentid + "&reason=" + parent.getElementsByTagName("select")[0].value + "&reasonExplanation=" + encodeURIComponent(parent.getElementsByTagName("textarea")[0].value),
        function (req, jsonResponse) {
            removeReportDialog(parent.parentElement.id);
        },
        function (req, err) {
            displayError(err);
        });
}

function modLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    var action = e.target.dataset.action;
    var commentId = e.target.dataset.commentid;
    jsonXhr("/Comment/Mod/" + action, "POST", "commentId=" + commentId, function (req, jsonResponse) {
        clearComments();
        loadComments();
    }, function (req, err) {
        displayError(err);
    });
}

function nextPuzzle(e) {
    e = e || window.event;
    if (e.target.getAttribute("href") !== "#") return true;
    e.preventDefault();
    startWithRandomPuzzle();
}

function retryPuzzle(e) {
    e.preventDefault();
    setup(window.puzzleId);
}

function reportPuzzleLinkClicked(e) {
    e = e || window.event;
    e.preventDefault();
    if (!window.reportPuzzleHtml) {
        xhr("/Report/Dialog/Puzzle", "GET", null, function(req) {
            document.getElementById("reportDialogContainer").innerHTML = req.responseText;
            document.getElementById("submitPuzzleReportLink").addEventListener("click", submitPuzzleReport);
            showPuzzleReportDialog();
        }, function(req, err) {
            displayError(err);
        });
    } else {
        showPuzzleReportDialog();
    }
}

function submitPuzzleReport(e) {
    e = e || window.event;
    e.preventDefault();
    var explanation = document.getElementById("puzzleReportExplanation").value;
    var reason = document.getElementById("puzzleReportReason").value;
    jsonXhr("/Report/Submit/Puzzle", "POST", "item=" + puzzleId + "&reason=" + encodeURIComponent(reason) + "&reasonExplanation=" + encodeURIComponent(explanation),
            function(req, jsonResponse) {
                document.getElementById("puzzleReportExplanation").value = "";
                hidePuzzleReportDialog();
            }, function(req, err) { displayError(err); });
}

function showPuzzleReportDialog() {
    document.getElementById("next-to-ground-inner").classList.add("nodisplay");
    document.getElementById("reportDialogContainer").classList.remove("nodisplay");
}

function hidePuzzleReportDialog() {
    document.getElementById("next-to-ground-inner").classList.remove("nodisplay");
    document.getElementById("reportDialogContainer").classList.add("nodisplay");
}

function replayControlClicked(e) {
    if (!window.replay) return;
    if (e.target.id === "controls-begin") {
        window.replay.current = 0;
    } else if (e.target.id === "controls-prev" && window.replay.current !== 0) {
        window.replay.current--;
    } else if (e.target.id === "controls-next" && window.replay.current !== window.replay.fens.length - 1) {
        window.replay.current++;
    } else if (e.target.id === "controls-end") {
        window.replay.current = window.replay.fens.length - 1;
    }
    var lastMove = window.replay.moves[window.replay.current];
    if (lastMove) {
        if (lastMove.indexOf("@") !== -1) lastMove = [lastMove.split("@")[1], lastMove.split("@")[1]];
        else lastMove = lastMove.substr(0, 5).split("-");
    } else {
        lastMove = null;
    }
    window.ground.set({
        fen: window.replay.fens[window.replay.current],
        lastMove: lastMove
    });
    var currentCheck = window.replay.checks[window.replay.current];
    if (currentCheck) {
        ChessgroundExtensions.setCheck(window.ground, currentCheck);
    } else {
        ChessgroundExtensions.setCheck(window.ground, null);
    }
    window.pocket = window.replay.pockets[window.replay.current];
    updatePocketCounters();
}

function startDragNewPiece(e) {
    e = e || window.event;

    var color = e.target.dataset.color;
    if (color !== window.yourColor) return;

    var role = e.target.dataset.role;
    if (!window.pocket[color + "-" + role] || window.pocket[color + "-" + role] < 1) return;

    ground.dragNewPiece({ color: color, role: role }, e);
}

function updatePocketCounters() {
    if (!window.pocket) return;

    var keys = Object.keys(window.pocket);

    for (var i = 0; i < keys.length; i++) {
        var key = keys[i];
        var counterElement = document.querySelector("span[data-counter-for='" + key + "']");
        counterElement.textContent = window.pocket[key].toString();

        var role = key.split('-')[1];
        var color = key.split('-')[0];
        if (window.pocket[key] === 0) {
            document.querySelector(".pocket-piece[data-role=" + role + "][data-color=" + color + "]").classList.add("unavailable");
        } else {
            document.querySelector(".pocket-piece[data-role=" + role + "][data-color=" + color + "]").classList.remove("unavailable");
        }
    }
}

function retagKeydown(e) {
    if (/[^0-9a-zA-Z,-]/.test(e.key)) {
        e.preventDefault();
    }

    if (e.key === "Enter") {
        e.preventDefault();
        var tags = document.getElementById("retag").value;
        jsonXhr("/Puzzle/Retag/" + window.selectedPuzzle.toString(), "POST", "tags=" + encodeURIComponent(tags), function (req, jsonResponse) {
            showTags(tags.split(","));
            document.getElementById("retag").value = "";
        }, function (req, err) {
            displayError(err);
        });
    }
}

window.addEventListener("load", function () {
    window.ground = Chessground(document.getElementById("chessground"), {
        coordinates: true,
        movable: {
            free: false,
            dropOff: "revert",
            showDests: false,
            events: {
                after: processPuzzleMove,
                afterNewPiece: processPuzzleDrop
            }
        },
        drawable: {
            enabled: true
        }
    });
    var submitCommentLink = document.getElementById("submitCommentLink");
    if (submitCommentLink) {
        submitCommentLink.addEventListener("click", submitComment);
    }
    document.getElementById("nextPuzzleLink").addEventListener("click", nextPuzzle);
    document.getElementById("retryPuzzleLink").addEventListener("click", retryPuzzle);
    document.getElementById("toggleExplanationLink").addEventListener("click", toggleExplanation);
    if (document.getElementById("reportPuzzleLink")) {
        document.getElementById("reportPuzzleLink").addEventListener("click", reportPuzzleLinkClicked);
    }
    if (!window.selectedPuzzle) {
        startWithRandomPuzzle();
    } else {
        setup(window.selectedPuzzle);
    }
    var controlIds = ["controls-begin", "controls-prev", "controls-next", "controls-end"];
    for (var i = 0; i < controlIds.length; i++) {
        document.getElementById(controlIds[i]).addEventListener("click", replayControlClicked);
    }

    var pocketPieces = document.querySelectorAll(".pocket-piece");
    for (i = 0; i < pocketPieces.length; i++) {
        pocketPieces[i].classList.add("draggable");
        pocketPieces[i].addEventListener("mousedown", startDragNewPiece);
        pocketPieces[i].addEventListener("touchstart", startDragNewPiece);
    }

    if (document.getElementById("retag")) {
        document.getElementById("retag").addEventListener("keydown", retagKeydown);
    }
});
