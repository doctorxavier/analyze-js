var State = null;

$(document).on('ready', function () {
    $("#ShowDeletedClauses").on("click", function () {

        if (State == null) {
            $("#LabelShowDeletedClauses").css("background-position", "0 -20px");
            State = "Checket";
        }
        else {
            $("#LabelShowDeletedClauses").css("background-position", "0 0px");
            State = null;
        }
    });

    $("#FormClauseExtension").on("submit", function (e) {
        e.preventDefault();

        var route = $('#FormClauseExtension').attr('action');

        $("#AjaxLoadForClauseHistoryReport").show();

        $.ajax({
            url: route,
            data: $("#FormClauseExtension").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0) {

                    $("#frameReportClausesHistory").attr("src", resp);
                    $("#frameReportClausesHistory").show();
                    //parent.location.href = resp;
                }
                else {
                    alert(resp);
                }
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function ()
            {
                $("#AjaxLoadForClauseHistoryReport").hide();
                
            }
        });
    });

    $("#Btn_ExportExcelClauseHistoryReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportClauseHistoryReport();
    });

    $("#Btn_ExportdPDFClauseHistoryReport").on("click", function () {
        $("#ExportType").attr("value", "PDF");
        ExportClauseHistoryReport();
    });

    $("#Btn_Clear").on("click", function()
    {   
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });

        $("#ClauseNumber").val("");

        $("#SubmissionDate").val("");
        $("#LastStatusChangeDate").val("");
        $("#OriginalExpirationDate").val("");
        $("#CurrentExpirationDate").val("");


    });


});

function ExportClauseHistoryReport() {
    var Route = $("#LinkExportClauseHistoryReport").attr("data-route");

    $("#AjaxLoadForClauseHistoryReportExport").show();

    $.ajax({
        url: Route,
        data: $("#FormClauseExtension").serialize(),
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0) {
                parent.location.href = resp;
            }
            else {
                alert(resp);
            }
        },
        error: function (e, err, erro) {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        },
        complete: function () {
            $("#AjaxLoadForClauseHistoryReportExport").hide();
        }
    });

}

