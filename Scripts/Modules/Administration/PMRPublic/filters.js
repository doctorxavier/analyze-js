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

    $("#clearButtonPMRPublic").on("click", function () {
        var combo = $("#CountryDepartments option").attr("selected",false);
            combo = $("#SectorDepartments option").attr("selected", false);
            combo = $("#PMRCycles").data("kendoDropDownList").select(0);
            combo = $("#PMRValidationStages option").attr("selected", false);
            combo = $("#OperationOverallStages option").attr("selected", false);
            combo = $("#Countries option").attr("selected", false);
            combo = $("#Divisions option").attr("selected", false);
            combo = $("#IsActive").data("kendoDropDownList").select(0);
            combo = $("#IsAuthorize").data("kendoDropDownList").select(0);

            reloadCountriesAjax();
            reloadDivisionsAjax();
    });

    function reloadCountriesAjax() {
        var route = $('#CountryDepartmentFilter').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPMRPublic").serialize(),
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

    function reloadDivisionsAjax(route) {
        var route = $('#SectorDepartmentFilter').attr('data-route');

        $.ajax({
            url: route,
            data: $("#FormPMRPublic").serialize(),
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