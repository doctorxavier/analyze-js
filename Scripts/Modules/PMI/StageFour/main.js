/**
 * PMI Module
 * Wed, 04 december 2013
 * eSmart Group
 */

//remap jQuery to $
var UrlFlag = 0;
var ConfirmationForShow = "";
$(document).ready(function () {

    //================ Confirmation Message =======================
            //var changesCount = 0;
        $("#btnStartUpPlan").click(function (e) {
            UrlFlag = 1;
            ConfirmationForShow = "StartUpPlan";
            showSaveChangesWindow();
        });

        $("#btnTlCompleted").click(function (e) {
            UrlFlag = 2;
            ConfirmationForShow = "TLcompleted";
            showSaveChangesWindow();
        });

        $("#btnCombined").click(function (e) {
            UrlFlag = 3;
            ConfirmationForShow = "Combined";
            showSaveChangesWindow();
        });

    });

function SendReq()
{
    if (UrlFlag == 1) {
        var urlRoute = $("#WebStartUpPlan").val();
    } else if (UrlFlag == 2) {
        var urlRoute = $("#WebTLcompleted").val();
    } else {
        var urlRoute = $("#WebCombined").val();
    }
        
    $(".auto").each(function () {
        $(this).val($(this).val().replace(/,/g, ''));
    });
    window.location.href = urlRoute;
}

function showSaveChangesWindow()
{
    //
    var urlRoute = $("#hdnConfirmationStartWF").attr("data-route"); 
    urlRoute = urlRoute + "&confirmationMessageWF=" + ConfirmationForShow; 

        // Display modal window to confirm start WF
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = "Warning";
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: urlRoute,
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

        dialog.open();

}
