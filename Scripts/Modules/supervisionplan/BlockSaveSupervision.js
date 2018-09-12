$(document).ready(function () {

    $("#savetest").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('#savetest').attr("action");
        miVar = setTimeout("route", 7000);
        redirectPage(miVar);
    });

});

$(document).ready(function () {

    $("#Requestest").on('click', function () {
        
        idbg.lockUi(null, true);
        var route = $('#Requestest').attr("action");
        miVar = setTimeout("route", 7000);
        redirectPage(miVar);
    });

});