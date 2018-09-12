$(document).ready(function () {
        $("#Save").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('#Save').attr("action");
        miVar = setTimeout("route", 7000);
        redirectPage(miVar);
    });

});

$(document).ready(function () {
    $("#Save2").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('#Save2').attr("action");
        miVar = setTimeout("route", 7000);
        redirectPage(miVar);
    });

});