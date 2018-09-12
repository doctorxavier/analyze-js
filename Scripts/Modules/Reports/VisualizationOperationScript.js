var State = null; 
$(document).on("ready", function ()
{
    $("#FormVisualizationOperationReport").on("submit", function (e) {
        e.preventDefault();

        var route = $('#FormVisualizationOperationReport').attr('action');

        $("#AjaxLoadForVisualizationOperation").show()

        $.ajax({
            url: route,
            data: $("#FormVisualizationOperationReport").serialize(),
            type: 'Post',
            success: function (resp)
            {   
                if (resp.indexOf("http") >= 0)
                {
                    $("#frameReportVisualizationOperation").attr("src", resp);
                    $("#frameReportVisualizationOperation").show();
                    
                }
                else
                {
                    alert(resp);
                }
            },
            error: function (e, err, erro)
            {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function () {
                $("#AjaxLoadForVisualizationOperation").hide();
                
            }
        });

    });

    $("#Btn_ExportPDFVisualizationOperationReport").on("click", function()
    {   
        $("#ExportType").attr("value", "PDF");
        ExportVisualizationOperationReport(); 
    });

    $("#Btn_ExportExcelVisualizationOperationReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportVisualizationOperationReport();
    });

    $("#ValidatedRMStatus").on("click", function ()
    {
        if (State == null) {
            $("#LabelFordeletedClausesOnly").css("background-position", "0 -20px");
            State = "Check";
        }
        else {
            $("#LabelFordeletedClausesOnly").css("background-position", "0 0px");
            State = null;
        }
    })

    $("#Btn_Clear").on("click", function () {
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });


        $("#LabelFordeletedClausesOnly").css("background-position", "0 0px");
        State = null;

    });



});

function ExportVisualizationOperationReport()
{

    var route = $("#LinkExportVisualizationOperationReport").attr("data-route");

    $("#AjaxLoadForVisualizationOperation").show()

    $.ajax({
        url: route,
        data: $("#FormVisualizationOperationReport").serialize(),
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0)
            {
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
            $("#AjaxLoadForVisualizationOperation").hide();
        }
    });

}
