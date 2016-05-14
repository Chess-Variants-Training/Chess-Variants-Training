function getUsernameFromTopbar() {
    var usernameElement = document.getElementById("username");
    if (!usernameElement) {
        return "Anonymous";
    }
    return usernameElement.textContent;
}

window.addEventListener("load", function () {
    document.getElementById("username").addEventListener("click", function (e) {
        e.preventDefault();
        with (document.getElementById("user-nav-content")) {
            if (getAttribute("class").indexOf("invisible") === -1) {
                setAttribute("class", "invisible");
            }
            else {
                setAttribute("class", "visible");
            }
        }
    });
});