function xhr(url, method, data, success, error) {
    var req = new XMLHttpRequest();
    req.onreadystatechange = function () {
        if (req.readyState === 4 && req.status === 200) {
            success(req);
        } else if (req.readyState === 4) {
            error(req, "Status code: " + req.status + ": " + req.statusText);
        }
    };
    req.open(method, url);
    if (method === "POST") {
        req.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        req.send(data);
    } else {
        req.send();
    }
}

function jsonXhr(url, method, data, success, error) {
    xhr(url, method, data, function (req) {
        var jsonResponse = JSON.parse(req.responseText);
        if (jsonResponse["success"] === true) {
            success(req, jsonResponse);
        } else {
            error(req, "Error from response: " + jsonResponse["error"]);
        }
    },
    function (req, err) {
        error(req, err);
    });
}

function displayError(err) {
    document.getElementById("error-overlay-inner").textContent = err;
    document.getElementById("error-overlay").classList.remove("nodisplay");
}

function hideError(e) {
    e = e || window.event;
    e.preventDefault();
    document.getElementById("error-overlay-inner").textContent = "";
    document.getElementById("error-overlay").classList.add("nodisplay");
}

window.addEventListener("load", function () {
    document.getElementById("error-overlay").addEventListener("click", hideError);
});