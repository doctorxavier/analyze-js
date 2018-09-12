var _mvcajax = {}

$(document).ready(function () {
    
    $(".optionSelect").kendoDropDownList();
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);
    
    $(".cancelEditRequest").click(function () {
        redirectPage($(this).data("route"));
    });

    $(".save").on("click", function () {
        $("#target").attr("action", $(this).attr("data-route"));
        $("#target").submit();
    });

    $("#target").submit(function () {
        var Action = $("#target").attr("action");
        var ActionName = Action.toString().split("/");

        if (ActionName[ActionName.length - 1] != 'CommentsSave') {
            event.preventDefault();
            idbg.lockUi(null, true);
        }
    });

});

