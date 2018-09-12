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

    $("#Btn_ClearFR").on("click", function () {
        $(".MultiSelectCustomCR option").each(function (index, element) {
            $(element).removeAttr("selected");
        });
        $(".TextboxCustomFR").each(function (index, element) {
            $(element).val("");
        });
        $("#LabelForIsIssueSolved").css("background-position", "0 0px");
        State = null;
        $("#IsIssueSolved").prop("checked", false);

        $("#txtOperationNumber").val("");
        $("#cmbusernames").empty("");
        $("#cmbusernames").addClass("hide");
        $("#cmbusernames").addClass("hidden-field");

    });

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

                    $("#frameReportFR").attr("src", resp);
                    $("#frameReportFR").show();
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

    $("#Btn_ExportExcelFRReport").on("click", function () {
        var idSelect = $("#cmbusernames option:selected").attr("id")
        $("#OperationNumber").val(idSelect).val();
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportClauseReport();

    });

    $("#Btn_ExportdPDFFRReport").on("click", function () {
        var idSelect = $("#cmbusernames option:selected").attr("id")
        $("#OperationNumber").val(idSelect).val();
        $("#ExportType").attr("value", "PDF");
        ExportClauseReport();
    });

    $("#cmbusernames").change(function () {
        var selectedText = $("#cmbusernames option:selected").first().val();
        if (selectedText) {
            $("#txtOperationNumber").val(selectedText);
        }
    });

    var iframeContent = document.getElementById('frameReportFR').contentWindow;
    $(iframeContent).on('resize', function () {
        resizeIframeCloud();
    });
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



function fillCombo() {
    $("#operationnumbernotfound").css("display", "none");
    $("#cmbusernames").addClass("hide");
    $("#cmbusernames").addClass("hidden-field");
    $("#loadSearchOp").show();
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
                $("#loadSearchOp").hide();
            }).fail(function (data) {
                $("#message-warning-1").css("visibility", "visible");
                $("#loadSearchOp").hide();
            })
              .success(function (data) {
                  $("#loadSearchOp").hide();
                  $("#message-warning-1").css("visibility", "hidden");
              })

}