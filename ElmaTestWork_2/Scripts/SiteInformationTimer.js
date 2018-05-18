
//~/Scripts/SiteInformationTimer.js

$(document).ready(function () {
    if (document.getElementById("infoSection") === undefined) {
        return;
    }
    var informationStore = function () {
        newUrl = document.location.protocol + "//" + document.location.host + "/Document/InformationStore";
        $.ajax({
            url: newUrl,
            type: 'POST',
            data: {}
        }).done(function (data) {
            if (data.Result === "OK") {
                if (document.getElementById("fileCount") !== undefined) {
                    document.getElementById("fileCount").innerText = data.Count;
                }
                if (document.getElementById("fileVolume") !== undefined) {
                    document.getElementById("fileVolume").innerText = data.Volume;
                }
            }
            else if (data.Result !== undefined) {
                console.error(data.Result.Message);
            }
        }).fail(function () {
            console.error("There is something wrong. Please try again.");
        });
        setTimeout(informationStore, 1000);
    }
});
