$(document).ready(function () {

    $("#savetest").on('click', function () {
        //idbg.lockUi(null, true);
        var route = $('#savetest').attr("action");
        miVar = setTimeout("route", 7000);
        idbg.lockUi(null, true);
        redirectPage(miVar);
    });

});
