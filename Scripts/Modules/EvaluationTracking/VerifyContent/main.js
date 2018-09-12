function getVerifyContentString() {
    var verifyContentParams = [];

    $('.evaluation-tracking-block').not('.Cancelled').not('.template').each(function () {

        var evaluationTrackingId = $(this).find('.evaluation-tracking-id').val();
        var stageId = $(this).find('.StageId').find('option:selected').attr('value');

        if (!evaluationTrackingId) evaluationTrackingId = '';
        if (!stageId) stageId = '';

        var tableDocuments = $(this).find('.documentsEvaluation');

        var tableStageOptions = $('.stageStringList li');
        var tableTypesOptions = $('.type-of-documents li');
        var idMethodologyCurrent = $(this).find('.MethodologyCurrent option:selected').val();


        tableDocuments.find('tr').each(function (index, item) {

            if (index == 0) return;

            var documentStageId = null;
            var documentTypeId = null;

            var documentStageLabel = safeName($(this).children().eq(0).text());

            tableStageOptions.each(function () {

                var parts = $(this).text().split('@');

                if (safeName(parts[1]) == documentStageLabel && safeName(parts[2]) == idMethodologyCurrent) {
                    documentStageId = safeName(parts[0]);
                    return false;
                }
            });

            var documentTypeLabel = safeName($(this).children().eq(1).text());

            tableTypesOptions.each(function () {

                var parts = $(this).text().split('@');

                if (safeName(parts[1]) == documentTypeLabel && documentStageId == safeName(parts[2])) {
                    documentTypeId = safeName(parts[0]);
                    return false;
                }
            });

            if (documentTypeId && documentStageId) {

                verifyContentParams.push(String(evaluationTrackingId) + ',' + String(stageId)
                    + ',' + String(documentStageId) + ',' + String(documentTypeId));
            }
        });

        if (tableDocuments.find('tr').length == 1) {
            verifyContentParams.push(String(evaluationTrackingId) + ',' + String(stageId) + ',,');
        }
    });


    verifyContentStr = verifyContentParams.join(';');

    return verifyContentStr;
}

$(document).ready(function () {

    $(".verify").click(function (e) {

        var verifyContentParams = getVerifyContentString();

        e.preventDefault();

        $(".tableOperator").css("z-index", "1");

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal4"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = $("#titleVerifyContent").attr("title");
        var dialog = $(".dinamicModal4").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: $("#hdnVerifyContentUrl").val() + "?resultsMatrixId=" + $("#hdnResultsMatrixId").val()
                + '&verifyContent=' + verifyContentParams,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").not('#kendo-dialog-container').remove();
            }
        }).data("kendoWindow");
        dialog.center();
        dialog.open();
    });

    $(".verifySave").click(function (e) {

        var verifyContentParams = getVerifyContentString();

        e.preventDefault();

        $(".tableOperator").css("z-index", "1");

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal4"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = $("#titleVerifyContent").attr("title");
        var dialog = $(".dinamicModal4").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: $("#hdnVerifyContentUrlSave").val() + "?resultsMatrixId=" + $("#hdnResultsMatrixId").val()
                + '&verifyContent=' + verifyContentParams + "&type=s",
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
        dialog.center();
        dialog.open();
        
    });
});