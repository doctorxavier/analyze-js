var State = null; 
$(document).on("ready", function () {

    $("#FormVisualizationPorfolioReport").on("submit", function (e) {
        e.preventDefault(); 
        
        var StartDate = $("#StartYear option:selected").attr("value");
        var EndDate = $("#EndYear option:selected").attr("value");

        if (Date.parse(StartDate) > Date.parse(EndDate))
        {
            alert("Start date cannot be after End Date");
        }
        else
        {

            $("#AjaxLoadForVisualizationPorfolio").show();

            var route = $("#FormVisualizationPorfolioReport").attr("action"); 

            $.ajax({
                url: route,
                data: $("#FormVisualizationPorfolioReport").serialize(),
                type: 'Post',
                success: function (resp) {
                    if (resp.indexOf("http") >= 0)
                    {
                        $("#frameReportVisualizationPorfolio").attr("src", resp);
                        $("#frameReportVisualizationPorfolio").show();
                    }
                    else {
                        alert(resp);
                    }
                },
                error: function (e, err, erro) {
                    alert('Error: ' + e + ' - ' + err + ' - ' + erro);
                },
                complete: function () {
                    $("#AjaxLoadForVisualizationPorfolio").hide();
                }
            });
        }

    }); 

    $("#ValidatedResultMatrix").on("click", function () {

        if (State == null) {
            $("#LabelForValidatedResultMatrix").css("background-position", "0 -20px");
            State = "Checket";
        }
        else {
            $("#LabelForValidatedResultMatrix").css("background-position", "0 0px");
            State = null;
        }
    });
    
    $("#Btn_ExportPDFVisualizationPorfolioReport").on("click", function () {
        $("#ExportType").attr("value", "PDF");
        ExportPorfolioReport();
    });

    $("#Btn_ExportExcelVisualizationPorfolioReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportPorfolioReport(); 
    });

    $("#CountryDepartment").on("change", function () {

        var route = $('#LinkFilterCountryPorfolioVisualization').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormVisualizationPorfolioReport").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                var LoadOptions = "";
                $("#Country option").remove();
                for (var i = 0; i < resp.length; i++) {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
                $("#Country").append(LoadOptions);
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });

    });

    $("#SectorDepartment").on("change", function () {

        var route = $('#LinkFilterDivisionVisualizationPorfolio').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormVisualizationPorfolioReport").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                var LoadOptions = "";
                $("#Division option").remove();
                for (var i = 0; i < resp.length; i++) {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Code + "</option>";
                }
                $("#Division").append(LoadOptions);
                ClearDivision();
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });

    });

    $("#Btn_Clear").on("click", function () {
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });
    });

}); 

function ExportPorfolioReport()
{
    var Route = $("#LinkExportVisualizationPorfolioReport").attr("data-route");

    $("#AjaxLoadForVisualizationPorfolioExport").show();

    $.ajax({
        url: Route,
        data: $("#FormVisualizationPorfolioReport").serialize(),
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
            $("#AjaxLoadForVisualizationPorfolioExport").hide();
        }
    });
}




