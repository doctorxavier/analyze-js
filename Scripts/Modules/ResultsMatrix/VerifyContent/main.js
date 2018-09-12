$(document).ready(function () {


    $(".verify").click(function (e) {

        e.preventDefault();

        // add + icon under the modal
        $(".tableOperator").css("z-index","1");

        // Diploy verify content window
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = $("#titleVerifyContent").attr("title");
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: $("#hdnVerifyContentUrl").val() + "?resultsMatrixId=" + $("#hdnResultsMatrixId").val() + "&intervalId=" + $("#hdnIntervalId").val() + "&cycleId=" + $("#hdnCycleId").val(),
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            open: function () {
                var headerHeight = $(window.parent.document).find('header').height();

                if (window.parent.window.scrollY > headerHeight * 2) {
                    headerHeight = 0;
                }

                kendoWindowSetTop($(".dinamicModal"), getTopKendoWindow() + headerHeight);
            },
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        dialog.center();
        dialog.open();

    });

});