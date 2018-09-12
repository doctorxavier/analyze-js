$(document).ready(function () {
    $(".optionSelect").kendoDropDownList();
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);
    
    $(".cancelEditRequest").click(function () {
        redirectPage($(this).data("route"));
    });

    $(".save").on("click", function () {
        $("#FormPostSupervision").attr("action", $(this).attr("data-route"));
        $("#FormPostSupervision").submit();
    });

    $("#Save-Supervision1").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('#Save-Supervision').attr("action");
        $("#RequestAfterSave").attr("value", "false");
        $("#FormPostSupervision").attr("action", route);
        $("#FormPostSupervision").submit();
    });

    $("#Save-Supervision").on('click', function () {
        idbg.lockUi(null, true);
        var route = $('#Save-Supervision').attr("action");
        $("#RequestAfterSave").attr("value", "false");
        $("#FormPostSupervision").attr("action", route);
        $("#FormPostSupervision").submit(); 
   });

   $("#Request-Supervision").on('click', function ()
   {
        idbg.lockUi(null, true);
        var route = $('#Request-Supervision').attr("data-route");
        $("#RequestAfterSave").attr("value", "true");
        $("#FormPostSupervision").attr("action", route);
        $("#FormPostSupervision").submit();
   });


   $("#RequestDown").on('click', function () {
       idbg.lockUi(null, true);
       var route = $('#RequestDown').attr("data-route");
       $("#RequestAfterSave").attr("value", "true");
       $("#FormPostSupervision").attr("action", route);
       $("#FormPostSupervision").submit();
   });



});

