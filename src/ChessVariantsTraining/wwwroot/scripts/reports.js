window.addEventListener("load", function () {
    var judgementLinks = document.querySelectorAll("a[data-judgement]");
    for (var i = 0; i < judgementLinks.length; i++) {
        judgementLinks[i].addEventListener("click", handle);
    }
});

function handle(e) {
    e = e || window.event;
    e.preventDefault();
    var reportId = e.target.dataset.reportid;
    var judgement = e.target.dataset.judgement;
    jsonXhr("/Report/Handle", "POST", "judgement=" + judgement + "&id=" + encodeURIComponent(reportId),
        function (req, jsonResponse) {
            e.target.parentElement.parentElement.remove();
        },
        function (req, err) { displayError(err); }
    );
}