document.addEventListener("DOMContentLoaded", function () {
    function numberThousands(x, sep) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, sep);
    }
    var counter = document.getElementById("counter");
    if (counter) {
        var cnt = 0;
        var interval = setInterval(() => {
            cnt++;
            counter.innerHTML = cnt.toString();
        }, 10);
        fetch("/home/getanalyticsdata")
            .then(response => response.text())
            .then(body => {
            var num = parseInt(body.replace(/"/g, ""));
            if (!isNaN(num)) {
                var sep = document.querySelector("html").lang == "es" ? "." : ",";
                counter.innerHTML = numberThousands(num, sep);
                clearInterval(interval);
            }
            else {
                throw Error("not a number");
            }
        }).catch(() => {
            counter.innerHTML = ">3000";
            clearInterval(interval);
        });
    }
    fetch("antiforgery/get").then(r => r.json()).then(j => {
        var input = document.createElement("input");
        input.setAttribute("name", "__RequestVerificationToken");
        input.setAttribute("type", "hidden");
        input.value = j;
        document.getElementById("selectLanguage").append(input.cloneNode(true));
        document.getElementById("account").append(input);
    });
});
//# sourceMappingURL=layout.js.map