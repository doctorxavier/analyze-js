var State = null;
$(document).on("ready", function () {

    $("#CountryDepartments").on("change", function () {
        $("#dvCountriesLoading").show();
        reloadCountriesAjax();
    });

    $("#SectorDepartments").on("change", function () {
        $("#dvDivisionsLoading").show();
        reloadDivisionsAjax();
    });

    $("#clearButtonVisualization").on("click", function () {
        var combo = $("#CountryDepartments option").attr("selected", false);
        combo = $("#SectorDepartments option").attr("selected", false);
        combo = $("#Countries option").attr("selected", false);
        combo = $("#Divisions option").attr("selected", false);
        $("#PublicationStatus").data('kendoDropDownList').value(-1);
        $("#ReportType").data('kendoDropDownList').value(-1);
        $("#OperationNumber").val('');
        reloadCountriesAjax();
        reloadDivisionsAjax();
    });

    function reloadCountriesAjax() {
        var route = $('#CountryDepartmentFilter').attr('data-route');
        $.ajax({
            url: route,
            data: $("#FormVisualization").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                $('#Countries').empty();
                $.each(resp, function (index, type) {
                    $('#Countries').append(new Option(type.Name, type.ConvergenceMasterDataId));
                });

            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function (resp) {
                $("#dvCountriesLoading").hide();
            }
        });

    }

    function reloadDivisionsAjax() {
        var route = $('#SectorDepartmentFilter').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormVisualization").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                $('#Divisions').empty();
                $.each(resp, function (index, type) {
                    $('#Divisions').append(new Option(type.Code, type.ConvergenceMasterDataId));
                });

            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function (resp) {
                $("#dvDivisionsLoading").hide();
            }
        });

    }
});