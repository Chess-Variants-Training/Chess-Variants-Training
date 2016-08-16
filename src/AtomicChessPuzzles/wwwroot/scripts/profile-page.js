window.addEventListener("load", function () {
    jsonXhr("/User/RatingChartData/" + document.getElementsByTagName("h1")[0].textContent, "GET", null, function (req, jsonResponse) {
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
    })
});