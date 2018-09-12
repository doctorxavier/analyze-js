$(document).on("ready", function () {

    $("#FormPorfolioCountryStatus").on("submit", function (e) {
        e.preventDefault();

        var route = $("#FormPorfolioCountryStatus").attr("action");

        $("#AjaxLoadForCountryStatusReport").show();
        $.ajax({
            url: route,
            data: $("#FormPorfolioCountryStatus").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0) {

                    $("#frameReportCountryStatus").attr("src", resp);
                    $("#frameReportCountryStatus").show();

                    //parent.location.href = resp;
                }
                else {
                    alert(resp);
                }
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function () {
                $("#AjaxLoadForCountryStatusReport").hide();
            }
        });

    });

    $("#Btn_ExportdPDFCountryStatusReport").on("click", function () {

        $("#ExportType").attr("value", "PDF");
        ExportCountryStatus();
    });

    $("#CountryDepartment").on("change", function () {

        var route = $('#LinkCountryDepartmentFilterForCountryStatus').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPorfolioCountryStatus").serialize(),
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

        var route = $('#LinkSectorDepartmentFilterForCountryStatus').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPorfolioCountryStatus").serialize(),
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

function ExportCountryStatus()
{
    var route = $("#LinkExportCountryStatusReport").attr("data-route");

    $("#AjaxLoadForCountryStatusExport").show();
    $.ajax({
        url: route,
        data: $("#FormPorfolioCountryStatus").serialize(),
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0) {

                $("#frameReportCountryStatus").hide();
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
            $("#AjaxLoadForCountryStatusExport").hide();
        }
    });

}

