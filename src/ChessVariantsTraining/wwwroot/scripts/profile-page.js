window.addEventListener("load", function () {
    updateChartData();
    document.getElementById("ratingChartDateRangeSelector").addEventListener("change", updateChartData);
    document.getElementById("ratingChartShownSelector").addEventListener("change", updateChartData);
});

function updateChartData() {
    if (window.ratingLineChart) {
        window.ratingLineChart.destroy();
    }
    var user = document.getElementsByTagName("h1")[0].textContent;
    var range = document.getElementById("ratingChartDateRangeSelector").value;
    var show = document.getElementById("ratingChartShownSelector").value;
    jsonXhr("/User/RatingChartData/" + user + "?range=" + range + "&show=" + show, "GET", null, function (req, jsonResponse) {
        var ctx = document.getElementById("ratingChart").getContext("2d");
        var data = {
            labels: jsonResponse["labels"],
            datasets: [
                {
                    label: jsonResponse["label"],
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
        window.ratingLineChart = Chart.Line(ctx, {
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