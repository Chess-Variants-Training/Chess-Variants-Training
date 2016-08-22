window.addEventListener("load", function () {
    updateChartData();
    document.getElementById("ratingChartDateRangeSelector").addEventListener("change", updateChartData);
    document.getElementById("ratingChartShownSelector").addEventListener("change", updateChartData);
});

window.borderColors = { "Atomic": "red", "King of the Hill": "orange", "Three-check": "orangered" };

function updateChartData() {
    if (window.ratingLineChart) {
        window.ratingLineChart.destroy();
    }
    var user = document.getElementsByTagName("h1")[0].textContent;
    var range = document.getElementById("ratingChartDateRangeSelector").value;
    var show = document.getElementById("ratingChartShownSelector").value;
    jsonXhr("/User/RatingChartData/" + user + "?range=" + range + "&show=" + show, "GET", null, function (req, jsonResponse) {
        var ctx = document.getElementById("ratingChart").getContext("2d");
        var datasets = [];
        var keys = Object.keys(jsonResponse["ratings"]);
        console.log(jsonResponse);
        for (var i = 0; i < keys.length; i++)
        {
            datasets.push({
                label: keys[i],
                lineTension: 0,
                fill: false,
                borderColor: window.borderColors[keys[i]],
                data: jsonResponse["ratings"][keys[i]],
                borderWidth: 1,
                pointRadius: 2,
                pointHoverRadius: 3,
                pointBackgroundColor: window.borderColors[keys[i]]
            });
        }
        var data = {
            labels: jsonResponse["labels"],
            datasets: datasets
        };
        window.ratingLineChart = Chart.Line(ctx, {
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    xAxes: [{ display: false }]
                },
                spanGaps: true
            }
        });
    }, function (req, err) {
        alert(err);
    });
}