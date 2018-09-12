var completeUrl = window.location.href;
var pieces = completeUrl.split("?");
var url = pieces[0];
var query = "n_a";
var intervalID = 0;


if (pieces[1] != null) {
    query = pieces[1];
}
//alert(url + " " + query);

function setPageFree() {
    $('#layoutLoadingDiv').addClass('hide').css("display", "none");
    $.get(idbg.getPath('Api/Concurrence/DeletePath?path=' + url + '&query=' + query),
        function (res, status, xhr) {
            window.clearInterval(intervalID);
        })
        .fail(function (res, status, xhr) {
            if (!(res.responseText.trim() == "" && status.trim() == "error" && xhr.trim() == "")) {
                showMessage("error: " + res.responseText + " " + status + " " + xhr);
                //Go back
                parent.history.back();

                return false;
            }
        }).always(function () {
            $('#layoutLoadingDiv').css("display", "none").removeClass('hide');
        });
}

function checkoutPage() {
    $('#layoutLoadingDiv').addClass('hide').css("display", "none");
    //The page is free or checkout by me, proceed to checkout
    $.post(idbg.getPath('Api/Concurrence/Post?path=' + url + '&query=' + query),
        function (res, status, xhr) {
            //Page reserved	

            if (res != '') {
                //The page is checkout by someone
                showMessage("This page and operation is open by " + res + " and can't be edited until he/she closes it.");
                //Go back
                parent.history.back();
                return false;
            }
        })
        .fail(function (res, status, xhr) {

            if (!(res.responseText.trim() == "" && status.trim() == "error" && xhr.trim() == "")) {
                showMessage("error: " + res.responseText + " " + status + " " + xhr);
                //Go back
                parent.history.back();
                return false;
            }
        }).always(function () {
            $('#layoutLoadingDiv').css("display", "none").removeClass('hide');
        });
}

$(document).ready(function () {
    $('#layoutLoadingDiv').addClass('hide').css("display", "none");
    $.get(idbg.getPath('Api/Concurrence/Get?path=' + url + '&query=' + query),
        function (res, status, xhr) {

            if (res != '') {
                //The page is checkout by someone
                showMessage("This page and operation is open by " + res + " and can't be edited until he/she closes it.");
                //Go back
                parent.history.back();
                return false;
            }
            else {
                checkoutPage();
                intervalID = window.setInterval(checkoutPage, 15000);
            }
        })
        .fail(function (res, status, xhr) {

            if (!(res.responseText.trim() == "" && status.trim() == "error" && xhr.trim() == "")) {
                showMessage("error: " + res.responseText + " " + status + " " + xhr);
                //Go back
                parent.history.back();
                return false;
            }
        }).always(function () {
            $('#layoutLoadingDiv').css("display", "none").removeClass('hide');
        });
});