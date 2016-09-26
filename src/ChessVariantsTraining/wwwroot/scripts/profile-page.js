window.addEventListener("load", function () {
    if (window.accountClosed) return;
    updateRatingChartData();
    document.getElementById("ratingChartDateRangeSelector").addEventListener("change", updateRatingChartData);
    document.getElementById("ratingChartShownSelector").addEventListener("change", updateRatingChartData);

    updateTtsChartData();
    document.getElementById("ttsChartDateRangeSelector").addEventListener("change", updateTtsChartData);
    document.getElementById("ttsChartShownSelector").addEventListener("change", updateTtsChartData);
});

window.borderColors = {
    "Atomic": "red", "King of the Hill": "orange", "Three-check": "blue", "Antichess": "pink", "Horde": "purple", "Racing Kings": "green",
    "Atomic (mate in one)": "red", "King of the Hill (mate in one)": "orange", "Three-check (third check)": "blue", "Antichess (forced capture)": "pink", "Horde (mate in one)": "purple"
};

function updateRatingChartData() {
    if (window.ratingLineChart) {
        window.ratingLineChart.destroy();
    }
    var user = window.userId;
    var range = document.getElementById("ratingChartDateRangeSelector").value;
    var show = document.getElementById("ratingChartShownSelector").value;
    jsonXhr("/User/ChartData/Rating/" + user + "/" + range + "/" + show, "GET", null, function (req, jsonResponse) {
        var ctx = document.getElementById("ratingChart").getContext("2d");
        updateChartData(ctx, jsonResponse["labels"], jsonResponse["ratings"], "ratingLineChart");
    }, function (req, err) {
        alert(err);
    });
}

function updateTtsChartData() {
    if (window.ttsLineChart) {
        window.ttsLineChart.destroy();
    }
    var user = window.userId;
    var range = document.getElementById("ttsChartDateRangeSelector").value;
    var show = document.getElementById("ttsChartShownSelector").value;
    jsonXhr("/User/ChartData/TimedTraining/" + user + "/" + range + "/" + show, "GET", null, function (req, jsonResponse) {
        updateChartData(document.getElementById("ttsChart").getContext("2d"), jsonResponse["labels"], jsonResponse["scores"], "ttsLineChart");
    }, function (req, err) {
        alert(err);
    });
}

function updateChartData(ctx, labels, data, variable) {
    var datasets = [];
    var keys = Object.keys(data);
    for (var i = 0; i < keys.length; i++) {
        datasets.push({
            label: keys[i],
            lineTension: 0,
            fill: false,
            borderColor: window.borderColors[keys[i]],
            data: data[keys[i]],
            borderWidth: 1,
            pointRadius: 2,
            pointHoverRadius: 3,
            pointBackgroundColor: window.borderColors[keys[i]]
        });
    }
    var data = {
        labels: labels,
        datasets: datasets
    };
    window[variable] = Chart.Line(ctx, {
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
}