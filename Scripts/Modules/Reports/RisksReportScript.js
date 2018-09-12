$(document).on("ready", function () {

    $("#Btn_Clear").on("click", function () {
        $(".MultiSelectCustomRiskReport option").each(function (index, element) {
            $(element).removeAttr("selected");
        });
    });

    $("#FormRiskReport").on("submit", function (e) {
        e.preventDefault();
        var route = $('#FormRiskReport').attr('action');

        $("#AjaxLoadForRiskReport").show()

        $.ajax({
            url: route,
            data: $("#FormRiskReport").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0) {

                    $("#frameReportRisk").attr("src", resp);
                    $("#frameReportRisk").show();
                    resizeIframeCloud();
                }
                else {
                    alert(resp);
                }
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function () {
                $("#AjaxLoadForRiskReport").hide();
                
            }
        });

    });

    $("#CountryDepartment").on("change", function () {
        

        if ($("#CountryDepartment option:selected").length == 0)
        {

            var route = $('#LinkAllCountrysRisk').attr('data-route');

            var newhtml = "";
            $("#CountryDepartment option").each(function (index, element) {
                var valueSelected = $(element).val();
                newhtml += "<input type='hidden' class='indexCountry' name='CountryDepartmentsId' value='" + valueSelected + "' />";
            });

            $("#FormRiskReport").append(newhtml);

            $.ajax({
                url: route,
                data: $("#FormRiskReport").serialize(),
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
                },
                complete: function () {
                    $(".indexCountry").remove();
                    
                }
            });

        }
        else {
            var route = $('#LinkFilterCountryRisk').attr('data-route');
            $.ajax({
                url: route,
                data: $("#FormRiskReport").serialize(),
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

        }

    });

    $("#Department").on("change", function () {

        if ($("#Department option:selected").length == 0) 
        {
            var route = $('#LinkAllDivisionsRisk').attr('data-route');

            var newhtml = ""; 
            $("#Department option").each(function (index, element) {
                var valueSelected = $(element).val();
                newhtml += "<input type='hidden' class='indexDepertments' name='Departments' value='" + valueSelected + "' />";
               //ArrayDepartments[index] = parseInt(valueSelected);
            }); 

            $("#FormRiskReport").append(newhtml);

            $.ajax({
                url: route,
                data: $("#FormRiskReport").serialize(),
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
                },
                complete: function ()
                {
                    $(".indexDepertments").remove(); 
                }
            });
        }
        else
        {
            var route = $('#LinkFilterDepartmentRisk').attr('data-route');

            $.ajax({
                url: route,
                data: $("#FormRiskReport").serialize(),
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
        }

    });

    $("#ESGTrackingFilterReports").on("change", function () {
        var valComboFinancial = $("#ESGTrackingFilterReports option:selected").attr("value");
        var IsFinancialDataAggregated = null;
        if (valComboFinancial == "true") {
            IsFinancialDataAggregated = true;
        }
        else if (valComboFinancial == "false") {
            IsFinancialDataAggregated = false;
        }

        $("#ESGTracking").attr("value", IsFinancialDataAggregated);

    });

    $("#ESGTracking").attr("value", false);

    $("#Btn_ExportExcelRiskReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportRiskReport();
    });

    $("#Btn_ExportPDFRiskReport").on("click", function () {
        $("#ExportType").attr("value", "PDF");
        ExportRiskReport();
    });
});

function ExportRiskReport()
{
    var Route = $("#LinkExportRiskReport").attr("data-route");

    $("#AjaxLoadForRiskReportExport").show();

    $.ajax({
        url: Route,
        data: $("#FormRiskReport").serialize(),
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
            $("#AjaxLoadForRiskReportExport").hide();
        }
    });
}
