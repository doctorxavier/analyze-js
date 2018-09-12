var State = null;
$(document).on("ready", function () {

   
    $("#CountryDepartment").on("change", function () {
        
        var route = $('#LinkFilterCountryPorfolioSupervision').attr('data-route');
        $("#AjaxLoadForVisualizationPorfolio").show();

        $.ajax({
            url: route,
            data: $("#FormSupervisionPorfolioReport").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {                
                var LoadOptions = "";
                $("#Country option").remove();
                for (var i = 0; i < resp.length; i++) {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
                $("#Country").append(LoadOptions);
                $("#AjaxLoadForVisualizationPorfolio").hide();
            }
        }); 

    });


    $("#SectorDepartment").on("change", function () {
        var route = $('#LinkFilterDivisionSupervisionPorfolio').attr('data-route');        
        $("#AjaxLoadForVisualizationPorfolio").show();

        $.ajax({
            url: route,
            data: $("#FormSupervisionPorfolioReport").serialize(),
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
                $("#AjaxLoadForVisualizationPorfolio").hide();
            }
        });

    });

    $("#Btn_Check").click(function (event) {
        if ($("#Btn_Check").val() == 'Check All') {
            $("#Btn_Check").prop('value','Uncheck All')
            var chk = true;
        } else {
            $("#Btn_Check").prop('value', 'Check All')
            var chk = false;
        }
        $(".checkSelect").prop('checked', chk);
    });

    var grid = $("#tabla").kendoGrid({
        dataSource: {
            pageSize: 25,
            schema: {
                model: {
                    fields: {
                        CountryDepartment: { type: "string" },
                        Country: { type: "string" },
                        SectorDepartment: { type: "string" },
                        OperationNumber: { type: "string" },
                        LoanNumber: { type: "string" },
                        Year: { type: "string" },
                        SpValidationStage: { type: "string" },
                        SpValidationDate: { type: "string" },
                        Select: { type: "string" }
                    }
                }
            },
        },
        scrollable: false,
        sortable: {
            allowUnsort: false
        },
        selectable: false,
        filterable: false,
        pageable: true,
        info: false,
        previousNext: false,
        resizable: true,
        allowUnsort: false,
        messages: {
            display: "",
            first: "",
            previous: "",
            next: "",
            last: "",
            refresh: ""
        },
        columns:
        [
            {
                field: "CountryDepartment"
            },
            {
                field: "Country"
            },
            {
                field: "SectorDepartment"
            },
            {
                field: "OperationNumber"
            },
            {
                field: "LoanNumber"
            },
            {
                field: "Year"
            },
            {
                field: "SpValidationStage"
            },
            {
                field: "SpValidationDate"
            },
            {
               field: "Select"
            }

        ],
        dataBound: function (e) {

            $(".k-pager-nav").hide();
            $(".k-pager-info").hide();
            $(".k-pager-first").hide();
            $(".k-pager-last").hide();
        }
    }).data("kendoGrid");

    $("#Btn_Clear").on("click", function () 
    {
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });

        SearchCountry();
        SearchDivision();

    });

    $("#FormSupervisionPorfolioReport").on("submit", function (e)
    {
        var formData = {};
        $.each($('#FormSupervisionPorfolioReport').serializeArray(), function (_, kv) {
            formData[kv.name] = kv.value;
        });
        if (parseInt(formData['StartYear']) > parseInt(formData['EndYear'])) {
            showWarning("#yearsValidation");
            $("#tabla").show();
            return false;
        }
        var route = $('#FormSupervisionPorfolioReport').attr('action');
        $("#AjaxLoadSupervisionPorfolioExport").show()

        $.ajax({
            url: route,
            data: $("#FormSupervisionPorfolioReport").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0)
                {
                    $("#frameReportSupervisionPorfolio").attr("src", resp);
                    $("#tabla").show();

                }
                else
                {
                    alert(resp);
                }
            },
            complete: function ()
            {
                $("#AjaxLoadSupervisionPorfolioExport").hide();
                
            }
        });
    });

    $("#Btn_ExportExcelSupervisionPorfolioReport").on("click", function () {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportSupervisionReport();
    });

    $("#Btn_ExportdPDFSupervisionPorfolioReport").on("click", function () {        
        $("#ExportType").attr("value", "PDF");
        ExportSupervisionReport();
    });

});



function showWarning(message)
{
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $('body').append('<form id="form" ' +
        '<div class="dinamicModal"><div style="padding: 20px;">' + $("" + message + "").val() + '</div><div class="pie pieReassign"><div class="botones"> ' +
        '<a title="Cancel" class="cancel" id="CancelWarningDialog" onclick="removemodal();" ' +
        'href="javascript:void(0)">' + $("#Cancel").val() + '</a></label> </div></div></div></form>');

    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: 'Warning',
        draggable: false,
        resizable: false,
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


function ExportSupervisionReport()
{
    var cant = 0;
    var operations = '', supervisions = '';

    $(".checkSelect").each(
        function (index, data) {           
            if ($(this).is(':checked')) {
                var id = this.id;
                var res = id.replace("checkBoxId", "");
                operations = operations + $("#OperationId" +res).val() + ',';
                supervisions = supervisions + $("#SupervisionId" + res).val() + ',';
            } else {
            }
    });
  
    if (operations == '' && supervisions == '')
    {
        showWarning("#mensajeSupervision");
        
    }
    else
    {
     var Route = $("#LinkExportSupervisionReport").attr("data-route") + '?Operations=' + operations.substring(0, operations.length - 1) + '&Supervisions=' + supervisions.substring(0, operations.length - 1) + '&Format=' + $("#ExportType").val();
   
     $("#AjaxLoadSupervisionPorfolioExportExport").show();

     $.ajax({
        url: Route,
        data: $("#FormSupervisionPorfolioReport").serialize(),
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
            $("#AjaxLoadSupervisionPorfolioExportExport").hide();
        }
    });
        }
}

function removemodal() {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
}

