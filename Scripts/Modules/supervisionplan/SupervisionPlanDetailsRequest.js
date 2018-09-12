$(document).ready(function () {

    $(".SendRequestDetails").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('.SendRequestDetails').data("route");
        redirectPage(route);
    });

});