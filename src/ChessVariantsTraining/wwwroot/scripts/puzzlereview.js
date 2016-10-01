window.inReview = true;

function approvePuzzle(e) {
    e = e || window.event;
    e.preventDefault();
    var puzzleId = e.target.parentElement.dataset.object;
    jsonXhr("/Review/Approve/" + puzzleId, "POST", null, function(req, jsonResponse) {
        alert("Approved.");
    }, function(req, err) {
        displayError(err);
    });
}

function rejectPuzzle(e) {
    e = e || window.event;
    e.preventDefault();
    var puzzleId = e.target.parentElement.dataset.object;
    jsonXhr("/Review/Reject/" + puzzleId, "POST", null, function(req, jsonResponse) {
        alert("Rejected.");
    }, function(req, err) {
        displayError(err);
    });
}

window.addEventListener("load", function() {
    document.getElementById("approve-puzzle").addEventListener("click", approvePuzzle);
    document.getElementById("reject-puzzle").addEventListener("click", rejectPuzzle);
});