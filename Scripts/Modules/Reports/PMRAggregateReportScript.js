var State = null;
$(document).on("ready", function () {
    
    $("#CountryDepartment").on("change", function () {
        
        var route = $('#LinkListCountryFilterPMR').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPMRAggregate").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                var LoadOptions = ""; 
                $("#Country option").remove();
                for (var i = 0; i < resp.length; i++) 
                {
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

        var route = $('#LinkListDivisionFilterPMR').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPMRAggregate").serialize(),
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

    $("#esgTrackingFilterReports").on("change", function () {
        var valComboFinancial = $("#esgTrackingFilterReports option:selected").attr("value");
        var IsFinancialDataAggregated = null;
        if (valComboFinancial == "true") {
            IsFinancialDataAggregated = true;
        }
        else if (valComboFinancial == "false") {
            IsFinancialDataAggregated = false;
        }

        $("#ESGTracking").attr("value", IsFinancialDataAggregated);

    });

    $("#ESGTracking").attr("value", true);

    $('#LabelIsRegional').on('click', function ()
    {
        if (State == null) {
            $("#LabelIsRegional").css("background-position", "0 -20px");
            State = "Checket";
        }
        else {
            $("#LabelIsRegional").css("background-position", "0 0px");
            State = null;
        }
    });

    $("#Btn_Clear").on("click", function () {
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });
    });

   
    $("#Btn_ExportExcelPMRPorfolioReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportPMRPorfolioHistoryReport();
    });

    $("#Btn_ExportdPDFPMRPorfolioReport").on("click", function () {
        $("#ExportType").attr("value", "PDF");
        ExportPMRPorfolioHistoryReport();
    });


});


function ExportPMRPorfolioHistoryReport()
{
    var Route = $("#LinkExportPMRPorfolioReport").attr("data-route");

    $("#AjaxLoadForPMRDashReport").show();

    $.ajax({
        url: Route,
        data: $("#FormPMRAggregate").serialize(),
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
            $("#AjaxLoadForPMRDashReport").hide();
        }
    });

}

function ConvertStringInt(a,b) {
   
    for (var i = 0; i < a.length; i++) {
        b[i] = parseInt(a[i], 10);
    }
    return b;
}


function ControlReport() {
   
    var CountryDepartment = $('#CountryDepartment').val();
    var SectorDepartment = $('#SectorDepartment').val();
    var Country = $('#Country').val();
    var Division = $('#Division').val();
    var HiddenPMRAggregate = $('#HiddenPMRAggregate').val();
    var PMRCycleId = $("#PMRCycleId").data("kendoDropDownList").value();
       var ESGTracking = $('#ESGTracking').data("kendoDropDownList").value();
    
    var CountryDepartment1 = [];
    if (CountryDepartment === null)
    {
         CountryDepartment1[0] = -1;
        
     }
     else
     {
       ConvertStringInt(CountryDepartment, CountryDepartment1);
    }

    var Country1 = [];
    if (Country === null) {
         Country1[0] = -1;
    }
    else
    {
        ConvertStringInt(Country, Country1);
    }
   
    var SectorDepartment1 =[];

    if (SectorDepartment === null) {
        SectorDepartment1[0] = -1;

    }
    else
    {
        ConvertStringInt(SectorDepartment, SectorDepartment1);
    }
    

    var Division1 =[];

    if (Division === null) {
        Division1[0] = -1;

    }
    else
    {
        Division1 = ConvertStringInt(Division,Division1);

    }
  
    var ESGTracking1 = Boolean(ESGTracking);
    var PMRCycleId1 = parseInt(PMRCycleId);
   

    $.ajax({
        url: HiddenPMRAggregate,
        data: JSON.stringify({ 'countryDepartment': CountryDepartment1, 'paises': Country1,'department': SectorDepartment1, 'division': Division1, 'pmr_cycle_id': PMRCycleId1, 'esg_tracking': ESGTracking1 }),
        type: "POST",
        dataType: "json",
        traditional: true,
        contentType:"application/json",
        success: function (resp) {
            $('#notFoundMessage').hide();
            if (resp > 0)
            {
                    var route = $('#FormPMRAggregate').attr('action');

                    $("#AjaxLoadForPMRDashReport").show();
                
                    $.ajax({
                        url: route,
                        data: $("#FormPMRAggregate").serialize(),
                        type: 'Post',
                        
                        success: function (resp) {
                            if (resp.indexOf("http") >= 0) {

                                $("#framePMRPorfolioReport").attr("src", resp);
                                $("#framePMRPorfolioReport").show();
                            }
                            else {
                                alert(resp);
                            }
                        },
                        error: function (e, err, erro) {
                            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
                        },
                        complete: function () {
                            $("#AjaxLoadForPMRDashReport").hide();
                            
                        }
                    });
                
            }
            else
            {
                $('#framePMRPorfolioReport').hide();
                $('#notFoundMessage').show();
             }
        },
        error: function (e) {
            alert('Error: ');
        },
        complete: function () {
           
        }
    });


}