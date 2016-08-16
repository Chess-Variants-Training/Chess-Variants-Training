window.addEventListener("load", function () {
    var username = document.getElementsByTagName("h1")[0].textContent;
    updateChartData(username, "all");
    document.getElementById("ratingChartDateRangeSelector").addEventListener("change", function (e) {
        e = e || window.event;
        updateChartData(username, e.target.options[e.target.selectedIndex].value);
    });
});

function updateChartData(user, range) {
    jsonXhr("/User/RatingChartData/" + user + "?range=" + range, "GET", null, function (req, jsonResponse) {
        var ctx = document.getElementById("ratingChart").getContext("2d");
        var data = {
            labels: jsonResponse["labels"],
            datasets: [
                {
                    label: "Puzzle rating",
                    lineTension: 0,
                    fill: false,
                    borderColor: "red",
                    data: jsonResponse["ratings"],
                    borderWidth: 1,
                    pointRadius: 2,
                    pointHoverRadius: 3,
                    pointBackgroundColor: "red"
                }
            ]
        };
        Chart.Line(ctx, {
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    xAxes: [{ display: false }]
                }
            }
        });
    }, function (req, err) {
        alert(err);
    });
}