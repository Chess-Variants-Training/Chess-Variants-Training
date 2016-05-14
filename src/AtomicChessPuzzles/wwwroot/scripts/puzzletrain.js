function startWithRandomPuzzle() {
    jsonXhr("/Puzzle/Train/GetOneRandomly", "GET", null, function (req, jsonResponse) {
        setup(jsonResponse["id"]);
    }, function (req, err) {
        alert(err);
    });
}

function setup(puzzleId) {
    document.getElementById("result").setAttribute("class", "");
    window.puzzleId = puzzleId;
    jsonXhr("/Puzzle/Train/Setup", "POST", "id=" + window.puzzleId + (window.trainingSessionId ? "&trainingSessionId=" + window.trainingSessionId : ""), function (req, jsonResponse) {
        window.ground.set({
            fen: jsonResponse["fen"],
            orientation: jsonResponse["whoseTurn"],
            turnColor: jsonResponse["whoseTurn"],
            movable: {
                free: false,
                dests: jsonResponse["dests"]
            }
        });
        clearExplanation();
        clearComments();
        loadComments();
        document.getElementById("author").textContent = jsonResponse["author"];
        window.trainingSessionId = jsonResponse["trainingSessionId"];
    }, function (req, err) {
        alert(err);
    });
}

function showExplanation(expl) {
    document.getElementById("explanation").innerHTML = expl;
}

function clearExplanation() {
    document.getElementById("explanation").innerHTML = "";
}

function submitPuzzleMove(origin, destination, metadata) {
    jsonXhr("/Puzzle/Train/SubmitMove", "POST", "id=" + window.puzzleId + "&trainingSessionId=" + window.trainingSessionId + "&origin=" + origin + "&destination=" + destination, function (req, jsonResponse) {
        window.ground.set({
            fen: jsonResponse["fen"]
        });
        if (jsonResponse["play"]) {
            var parts = jsonResponse["play"].split("-");
            window.ground.move(parts[0], parts[1]);
            window.ground.set({
                fen: jsonResponse["fenAfterPlay"]
            });
        }
        switch (jsonResponse["correct"]) {
            case 0:
                break;
            case 1:
                with (document.getElementById("result")) {
                    textContent = "Success!";
                    setAttribute("class", "green");
                };
                break;
            case -1:
                with (document.getElementById("result")) {
                    textContent = "Sorry, that's not correct. This was correct: " + jsonResponse["solution"];
                    setAttribute("class", "red");
                }
        }
        if (jsonResponse["dests"]) {
            window.ground.set({
                movable: {
                    dests: jsonResponse["dests"]
                }
            });
        }
        if (jsonResponse["explanation"]) {
            showExplanation(jsonResponse["explanation"]);
        }
    }, function (req, err) {
        alert(err);
    });
}

function submitComment(e) {
    e = e || window.event;
    e.preventDefault();
    jsonXhr("/Puzzle/Comment/PostComment", "POST", "commentBody=" + encodeURIComponent(document.getElementById("commentBody").value) + "&puzzleId=" + window.puzzleId, function (req, jsonResponse) {
        appendComment(getUsernameFromTopbar(), jsonResponse["bodySanitized"]);
    },
    function (req, err) {
        alert(err);
    });
}

function clearComments() {
    document.getElementById("commentContainer").innerHTML = "";
}

function appendComment(author, bodySan) {
    var p = document.createElement("p");
    p.innerHTML = "<em>" + author + ":</em> " + bodySan;
    var cmtContainer = document.getElementById("commentContainer");
    cmtContainer.insertBefore(p, cmtContainer.firstElementChild);
}

function loadComments() {
    jsonXhr("/Puzzle/Comment/GetComments?puzzleId=" + window.puzzleId, "GET", null, function (req, jsonResponse) {
        var commentCount = jsonResponse["count"];
        if (commentCount === 0) {
            document.getElementById("commentContainer").innerHTML = "There are no comments on this puzzle.";
            return;
        }
        var commentsFromResponse = jsonResponse["comments"];
        for (var i = 0; i < commentsFromResponse.length; i++) {
            var curr = commentsFromResponse[i];
            appendComment(curr["author"], curr["body"]);
        }
    },
    function (req, err) {
        alert(err);
    })
}

window.addEventListener("load", function () {
    window.ground = Chessground(document.getElementById("chessground"), {
        coordinates: false,
        movable: {
            free: false,
            dropOff: "revert",
            showDests: false,
            events: {
                after: submitPuzzleMove
            }
        }
    });
    document.getElementById("submitCommentLink").addEventListener("click", submitComment);
    startWithRandomPuzzle();
});