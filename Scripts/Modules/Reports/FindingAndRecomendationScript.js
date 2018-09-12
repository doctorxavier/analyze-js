var State = null;
$(document).on("ready", function () {

    $("#IsIssueSolved").on("click", function () {
        if (State == null) {
            $("#LabelForIsIssueSolved").css("background-position", "0 -20px");
            State = "Checket";
        }
        else {
            $("#LabelForIsIssueSolved").css("background-position", "0 0px");
            State = null;
        }
    });

    $("#CountryDepartment").on("change", function () {
        var route = $('#LinkListCountryFilterFR').attr('data-route');
        $.ajax({
            url: route,
            data: $("#FormFRAggregate").serialize(),
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
        var route = $('#LinkListDivisionFilterFR').attr('data-route');
        $.ajax({
            url: route,
            data: $("#FormFRAggregate").serialize(),
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

    $("#Dimension").on("change", function () {

        var route = $('#LinkListDimensionFilterFR').attr('data-route');
        $.ajax({
            url: route,
            data: $("#FormFRAggregate").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                var LoadOptions = "";
                $("#Categories option").remove();
                for (var i = 0; i < resp.length; i++) {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
                $("#Categories").append(LoadOptions);
                ClearDivision();
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });

    });

    $("#Btn_ExportExcelFRReport").on("click", function () {
        var idSelect = $("#cmbusernames option:selected").attr("id")
        $("#OperationNumber").val(idSelect).val();

        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportClauseReport();
    });

    $("#Btn_ExportdPDFFRRReport").on("click", function () {
        var idSelect = $("#cmbusernames option:selected").attr("id")
        $("#OperationNumber").val(idSelect).val();

        $("#ExportType").attr("value", "PDF");
        ExportClauseReport();
    });


    function ExportClauseReport() {

        var Route = $("#LinkExportFRReport").attr("data-route");

        $(".AjaxLoadForFRReportExport").show();

        $.ajax({
            url: Route,
            data: $("#FormFRAggregate").serialize(),
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
                $(".AjaxLoadForFRReportExport").hide();
            }
        });

    }

    $("#FormFRAggregate").on("submit", function (e) {
        e.preventDefault();        
        var idSelect = $("#cmbusernames option:selected").attr("id")
        $("#OperationNumber").val(idSelect).val();

        $(".AjaxLoadForFRReportExport").show();

        var route = $('#FormFRAggregate').attr('action');

        $.ajax({
            url: route,
            data: $("#FormFRAggregate").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0) {

                    $("#frameFandRecomendations").attr("src", resp);
                    $("#frameFandRecomendations").show();
                }
                else {
                    alert(resp);
                }
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function () {
                $(".AjaxLoadForFRReportExport").hide();
                
            }
        });
    });

    $("#cmbusernames").change(function () {
        var selectedText = $("#cmbusernames option:selected").first().val();
        if (selectedText) {
            $("#txtOperationNumber").val(selectedText);
        }
    });

    $("#Btn_Clear").on("click", function () {

        $(".MultiSelectCustomCR option").each(function (index, element) {
            $(element).removeAttr("selected");
        });

        $("#txtOperationNumber").val("");
        $("#cmbusernames").empty("");
        $("#cmbusernames").addClass("hide");
        $("#cmbusernames").addClass("hidden-field");
    });

    // Fin Ready
});

//Llena texto lista cuando se consulta por una operación
function fillCombo() {
    $("#operationnumbernotfound").css("display", "none");
    $(".AjaxLoadForFRReportExport").show();
    $.getJSON("../FindingRecomendationsReport/GetSearchResults" + "?numberOperation=" + $("#txtOperationNumber").val(), null,

            function (data) {
                $("#cmbusernames").empty();
                $(".messages").remove();

                var cont = 0;
                $.each(data, function (i, item) {


                    $("#divcbn").fadeIn(1000);

                    $("#cmbusernames").append("<option id='" + item.OperationCode + "'>" + item.OperationCode.toUpperCase() + "</option>").css('height', '80px');
                    cont++;
                });
                if (cont == 0) {

                    $("#cmbusernames").addClass("hide");
                    $("#cmbusernames").addClass("hidden-field");
                    $("#operationnumbernotfound").css("display", "inline-block");

                }
                else {
                    $("#cmbusernames").removeClass("hide");
                    $("#cmbusernames").removeClass("hidden-field");
                    $("#operationnumbernotfound").css("display", "none");

                }
                $(".AjaxLoadForFRReportExport").hide();
            }).fail(function (data) {
                $("#message-warning-1").css("visibility", "visible");
                $(".AjaxLoadForFRReportExport").hide();
            })
              .success(function (data) {
                  $("#message-warning-1").css("visibility", "hidden");
              })

};