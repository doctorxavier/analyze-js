function deleteItem(element) {
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: $(element).data("route"),
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    dialog.center();
    dialog.open();
}

$(document).ready(function () {

    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);

    var dialog = null;

    

    $("#cancelAction").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        dialog = dialog.close();
        $("#centralSection").append(dialog);
    });

    $("#deleteAction").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        redirectPage($(this).data("route"));
    });

    $('.actionDeleteClause').click(function () {
        var title = "Warning";
        var myWin = $("#confirmWindow");

        if (!myWin.data("kendoWindow")) {
            // window not yet initialized
            myWin.kendoWindow({
                width: "auto",
                height: "auto",
                modal: false,
                resizable: true,
                title: title,
                content: $("#confirmWindow").html()
            });
        } else {
            // reopening window
            myWin.data("kendoWindow")
                .content("Loading...") // add loading message
                .refresh($("#confirmWindow").html()) // request the URL via AJAX
                .open(); // open the window
        }
    });

    if ($(window.parent.document).find('body').find(".ui-widget-overlay")) {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    }
    if ($(".ui-widget-overlay").length > 0) {
        $(".ui-widget-overlay").remove();
    }
    if ($(".k-window").length > 0) {
        $(".k-window").remove();
    }
    if ($(".k-window-titlebar").length > 0) {
        $(".k-window-titlebar").remove();
    }

    //Mensaje tooltip de error para validaciones
    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $("#target").validate();

    jQuery.validator.addMethod("txtDescriptionComent", function (value, element) {
        if (value.toString() != "") {
            if ($(element).val().toString().trim().length == 0) {
                return false;
            }
        }
        else {
            return false;
        }
        return true;
    });

});

$("#target").submit(function () {
    var form = $("#target")
   .removeData("validator") /* added by the raw jquery.validate plugin */
   .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */
    $.validator.unobtrusive.parse(form);

    return true;
});
