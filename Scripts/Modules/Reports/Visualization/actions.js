$(document).on("ready", function () {

    $("#pdfButtonVisualization").on("click", function () {
        $("#reportParametersPDF").val(buildReportParameters());
        var reportType = $("#ReportType").data("kendoDropDownList").text();
        $("#reportParametersReportTypePDF").val(reportType);
        $("#ExportVisualizationPDF").submit();
    });

    $("#excelButtonVisualization").on("click", function () {
        $("#reportParametersExcel").val(buildReportParameters());
        var reportType = $("#ReportType").data("kendoDropDownList").text();
        $("#reportParametersReportTypeExcel").val(reportType);
        $("#ExportVisualizationExcel").submit();
    });

    $("#submitButtonVisualization").on("click", function () {
        idbg.lockUi(null, true);
        var reportParameterEmbedded = buildReportParameters();
        var reportParameterType = $("#ReportType").data("kendoDropDownList").text();
        var urlAction = $('#submitButtonVisualization').data('url');
        $.ajax({
            url: urlAction,
            data: JSON.stringify(
                {
                    'reportParametersEmbedded': reportParameterEmbedded,
                    'reportParametersReportTypeEmbedded': reportParameterType
                }),
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            success: function (resp) {
                $('#notFoundMessage').hide();
                if (resp.data.indexOf("http") >= 0) {
                    $("#frameVisualizationSReport").attr("src", resp.data);
                    $("#frameVisualizationSReport").height(1500);
                    $("#frameVisualizationSReport").show();
                    initReziseCloud();
                }
                else {
                    $('#frameVisualizationSReport').hide();
                    initReziseCloud();
                    $('#notFoundMessage').show();
                }
            },
            error: function (e) {
                alert('ERROR: Something went wrong during the operation.' +
                    'Please, refer to the log for further detail.');
            },
            complete: function () {
                
                idbg.lockUi(null, false);
            }
        });
    });

    function buildReportParameters() {
        var dropdownPublicationStatus = $("#PublicationStatus").data("kendoDropDownList");
        var parameters = '';
        var pubStatus = ''
        var countryDepts = '';
        var sectorDepts = '';
        var countries = '';
        var divisions = '';

        if (dropdownPublicationStatus.text() != "All") {
            pubStatus = dropdownPublicationStatus.value();
        }
        parameters = "&PUBLICATION_STATUS=" + pubStatus;

        $("#CountryDepartments option:selected").each(function () {
            countryDepts += $(this).val() + ',';
        });
        if (countryDepts != '')
            countryDepts = countryDepts.substr(0, countryDepts.length - 1);
        parameters += "&COUNTRY_DEPARTMENT=" + countryDepts;

        $("#SectorDepartments option:selected").each(function () {
            sectorDepts += $(this).val() + ',';
        });
        if (sectorDepts != '')
            sectorDepts = sectorDepts.substr(0, sectorDepts.length - 1);
        parameters += "&DEPARTMENT_SECTOR=" + sectorDepts;

        $("#Countries option:selected").each(function () {
            countries += $(this).val() + ',';
        });
        if (countries != '')
            countries = countries.substr(0, countries.length - 1);
        parameters += "&COUNTRY=" + countries;

        $("#Divisions option:selected").each(function () {
            divisions += $(this).val() + ',';
        });
        if (divisions != '')
            divisions = divisions.substr(0, divisions.length - 1);
        parameters += "&DIVISION=" + divisions;

        var opNumber = $("#OperationNumber").val().toUpperCase();
        parameters += "&OPERATION_NUMBER=" + opNumber;

        return parameters;
    }

});