var State = null;
$(document).on("ready", function () {

    $("#deletedClausesOnly").on("click", function () {

        if (State == null) {
            $("#LabelFordeletedClausesOnly").css("background-position", "0 -20px");
            State = "Checket";
        }
        else {
            $("#LabelFordeletedClausesOnly").css("background-position", "0 0px");
            State = null;
        }
    });

    $("#FormClauseReportCreate").on("submit", function (e) {
        e.preventDefault();

        $("#AjaxLoadForClauseReport").show();

        var route = $('#FormClauseReportCreate').attr('action');

        $.ajax({
            url: route,
            data: $("#FormClauseReportCreate").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0) {

                    $("#frameReportClauses").attr("src", resp);
                    $("#frameReportClauses").show();
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
                $("#AjaxLoadForClauseReport").hide();
                
            }
        });
    });

    $("#Btn_ExportExcelClauseReport").on("click", function()
    {   
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportClauseReport();
    });

    $("#Btn_ExportdPDFClauseReport").on("click", function ()
    {
        $("#ExportType").attr("value", "PDF");
        ExportClauseReport();
    });

    $("#Btn_Clear").on("click", function () {
        
        $(".MultiSelectCustomCR option").each(function (index, element) {
            $(element).removeAttr("selected");
        });

        $("#ClauseNumber").val("");
        $("#CurrentExpirationDateFrom").val("");
        $("#CurrentExpirationDateFrom").val("");

        $("#LabelFordeletedClausesOnly").css("background-position", "0 0px");
        State = null;

        $("#deletedClausesOnly").prop("checked", false);


    });

});

function ExportClauseReport()
{   
    var Route = $("#LinkExportClauseReport").attr("data-route");

    $("#AjaxLoadForClauseReportExport").show();

    $.ajax({
        url: Route,
        data: $("#FormClauseReportCreate").serialize(),
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0) {
                parent.location.href = resp;
            }
            else
            {
                alert(resp);
            }
        },
        error: function (e, err, erro) {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        },
        complete: function () {
            $("#AjaxLoadForClauseReportExport").hide();
        }
    });

}
