var autoCalculateYearPlansForFinancial = function (
    dataType, inputElement, totalClassName, referenceYear) {
    var rowParent = $(inputElement).closest("table").closest("td").closest("tr");
    var currentYear = parseInt($("#CurrentServerYear").val());

    if (dataType == "rm_p_row") {//update original
        if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_p_row input[readonly]").length > 0) {
            var originalPlanAuto = 0;
            $(rowParent).find("> td.yearPlanData").each(function (index) {
                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_p_row input").autoNumeric('get')))) {
                    originalPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_p_row input").autoNumeric('get'));
                }
            });
            $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_p_row  input:first").autoNumeric('set', originalPlanAuto);
        }
    }
    else if (dataType == "rm_ac_row") {//update actual
        if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_ac_row input[readonly]").length > 0) {
            var actualPlanAuto = 0;
            $(rowParent).find("> td.yearPlanData").each(function (index) {
                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get')))) {
                    actualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get'));
                }
            });
            $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_ac_row  input:first").autoNumeric('set', actualPlanAuto);
            //update annual too
            var columnActualYear = parseInt($(inputElement).closest("table").closest("td").find("> input.hiddenYear").val());
			
            if (columnActualYear < referenceYear) {
                if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_pa_row input[readonly]").length > 0) {
                    var annualPlanAuto = 0;
                    var hasAnyAnnualValues = false;

                    $(rowParent).find("> td:not(." + totalClassName + ")> table > tbody > tr.rm_pa_row input").each(function (index, item) {
                        if (!isNaN(parseFloat($(item).autoNumeric('get')))) {
                            hasAnyAnnualValues = true;
                            return false;
                        }
                    });

                    if (hasAnyAnnualValues) {
                        $(rowParent).find("> td.yearPlanData").each(function (index) {
                        var columnYear = parseInt($(this).find("> input.hiddenYear").val());

                        if (columnYear < referenceYear) {
                            if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get')))) {
                                annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get'));
                            }
                        }
                        else {
                            if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_pa_row input").autoNumeric('get')))) {
                                annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_pa_row input").autoNumeric('get'));
                            }
                        }
                    });
                    }

                    $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_pa_row  input:first").autoNumeric('set', annualPlanAuto);
                }
            }
        }
    }
    else if (dataType == "rm_pa_row") {//update annual
        if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_pa_row input[readonly]").length > 0) {
            var annualPlanAuto = 0;
            var hasAnyAnnualValues = false;

            $(rowParent).find("> td:not(." + totalClassName + ")> table > tbody > tr.rm_pa_row input").each(function (index, item) {
                if (!isNaN(parseFloat($(item).autoNumeric('get')))) {
                    hasAnyAnnualValues = true;
                    return false;
                }
            });

            if (hasAnyAnnualValues) {
                $(rowParent).find("> td.yearPlanData").each(function (index) {
                var columnYear = parseInt($(this).find("> input.hiddenYear").val());

                if (columnYear < referenceYear) {
                    if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get')))) {
                        annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get'));
                    }
                }
                else {
                    if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_pa_row input").autoNumeric('get')))) {
                        annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_pa_row input").autoNumeric('get'));
                    }
                }
            });
            }

            $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_pa_row  input:first").autoNumeric('set', annualPlanAuto);
        }
    }
}

$(document).ready(function () {
    $.each($('input.numberInput'), function (index, input) {
        var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

        if (year >= 2014) {
            $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });
        } else {
            $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
        }
    });

    $('#outputsFinancialTarget').on('change', 'input.numberInput', function () {
        var totalClassName = "";

        if (!$(this).closest("table").closest("td").hasClass("totalOutputYearPlan") &&
            $(this).closest("table").closest("td").closest("tr").find("> td.totalOutputYearPlan").length > 0) {

            totalClassName = "totalOutputYearPlan";
        }
        else if (!$(this).closest("table").closest("td").hasClass("totalOtherCostYearPlan") &&
            $(this).closest("table").closest("td").closest("tr").find("> td.totalOtherCostYearPlan").length > 0) {

            totalClassName = "totalOtherCostYearPlan";
        }

        if (totalClassName != "") {
                var inputsLabelReferenceYears = $(this).closest("table").closest("td").closest("tr")
                    .find('td.yearPlanData > table > tbody > tr.rm_pa_row > td > label > input').map(function () {
                        if (parseInt($(this).val()) >= 0) {
                            return parseInt($(this).closest("table").closest("td").find('> input.hiddenYear').val())
                        }
                });

            var lastYearWithData = inputsLabelReferenceYears.length > 0 ?
                Math.max.apply(Math, inputsLabelReferenceYears) :
                $('[name=CurrentServerYear]').val();

            var pmryear = $(this).closest("table").closest("td").closest("tr")
                .find('td.yearPlanData > table > tbody > tr > td > label > input[data-pmryear]')
                .attr('data-pmryear');

            var currentReferenceYear = lastYearWithData < pmryear ?
                lastYearWithData : pmryear;

            autoCalculateYearPlansForFinancial(
                $(this).closest("tr").attr("class"),
                $(this),
                totalClassName,
                currentReferenceYear);
        }
    });

    $(".btn-primary_2").click(function () {
        if (!$(this).hasClass("disableAnchor")) {
            if (chekIfItHasBeenChangesFinancial()) {
                var positionRelative = $(this).offset();

                $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
                var route = $("#WarningMessageURL").val();
                var title = "Warning";
                var dialog = $(".dinamicModal").kendoWindow({
                    width: "800px",
                    title: title,
                    draggable: false,
                    resizable: false,
                    content: route,
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
            else {
                $("input.numberInput").each(function () {
                    if ($(this).val().length > 0) {
                        $(this).val($(this).autoNumeric('get'));
                    }
                });
                idbg.lockUi(null, true);
                $(this).addClass("disableAnchor");
                $("#outputsFinancialTarget").submit();
            }

        }
    });

    $(function () {
        $(document).tooltip({
            items: ".input-validation-error",
            content: function () {
                if ($(this).attr('data-val-range'))
                    return $(this).attr('data-val-range');
                if ($(this).attr('data-val-required'))
                    return $(this).attr('data-val-required');
            }
        });
    });

    $(".hoverOtherCost").hover(
    function () {
        $(this).find(".actionbar").css("display", "block");
    },
    function () {
        $(this).find(".actionbar").css("display", "none");
    }
    );

    $(".annualChangeValueInput").change(function () {
        var year = parseInt($(this).closest("td.dato07").find("> input.hiddenYear").val());
        var currentYear = parseInt($("#CurrentServerYear").val());
        if (year >= currentYear) {
            if (!$(this).hasClass("valuechanged")) {
                $(this).addClass("valuechanged");
            }
        }
    });

    $(".import-financial-pep-data").click(function (event) {
        checkImportDataFromPep($("input[name='hdn-import-pep-data-title']").val(),
            $("input[name='hdn-import-pep-data-message']").val(),
            $("input[name='hdn-import-pep-data-cancel']").val(),
            $("input[name='hdn-import-pep-data-confirm']").val(),
            true);
    });
});

function AddOtherCost(element) {
    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();
    var referenceYear = $("input[data-referenceyear]").attr('data-referenceyear');

    var currentYear = parseInt($("#CurrentServerYear").val());
    var maxOtherCostIndex = 0;
    var resultArray = $(element).closest("div.minimizeTable2").find("> div.tableGrid > table > tbody > tr.nivel01").
       map(function () { return $(this).find("> td:eq(0)> input[type=hidden]:not(.OtherCosId)").val(); }).get();

    if (resultArray.length > 0) {
        maxOtherCostIndex = Math.max.apply(null, resultArray);
        maxOtherCostIndex++;
    }

    var maxOtherCostYearPlan = $("#otherCostTable > thead > tr > th[data-othercostyearplanindex]").length;
    var options = new Array();

    $("#otherCostTable > thead > tr > th[data-othercostyearplanindex]").each(function (index) {
        var isActualValueBlock = false;
        var isOriginalValueBlock = false;
        var isAnnualValueBlock = false;

        isOriginalValueBlock = BlockFieldStartUpPlan(interval);
        isAnnualValueBlock = BlockFieldAnnualPlan(interval, parseInt($(this).text().trim()));
        isActualValueBlock = BlockFieldActualPlan(interval, parseInt($(this).text().trim()));

        var newObject = {
            otherCostYearPlansIndex: index,
            year: $(this).text().trim(),
            isActualValueBlock: isActualValueBlock,
            isOriginalValueBlock: isOriginalValueBlock,
            isAnnualValueBlock: isAnnualValueBlock,
            otherCostIndex: maxOtherCostIndex,
            referenceYear: referenceYear
        };
        options.push(newObject);
    });

    var source = $("#OtherCost-template").html();
    var template = Handlebars.compile(source);
    var context = {
        otherCostIndex: maxOtherCostIndex,
        maxOtherCostYearPlan: maxOtherCostYearPlan,
        options: options,
        referenceYear: referenceYear
    };
    var newDinamicContent = template(context);

    $("#otherCostTable > tbody").append(newDinamicContent);

    $.each($("#otherCostTable > tbody > tr.hoverOtherCost:last input.numberInput"), function (index, input) {

        var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

        if (year >= 2014) {
            $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });

        } else {
            $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
        }
    });

    $("#otherCostTable > tbody").find("> tr.hoverOtherCost:eq(0)").hover(
    function () {
        $(this).find(".actionbar").css("display", "block");
    },
    function () {
        $(this).find(".actionbar").css("display", "none");
    }
    );

    rebindingValitadionFieldsForOutputs($("#outputsFinancialTarget"));

    
}

function deleteOtherCost(element) {
    var otherCostTableTbody = $(element).closest("tbody");

    if ($(element).closest("tr").find("> td:eq(0) > input.OtherCosId[type=hidden]").length == 0) {
        $(element).closest("tr").remove();
    }
    else {

        var operationNumber = $("#Operation_OperationNumber").val();
        var otherCostId = $(element).closest("tr").find(" > td:eq(0) > input.OtherCosId[type=hidden]").val();
        var positionRelative = $(element).offset();

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: $("#IndexDeleteOtherCostWarning").data("title"),
            draggable: false,
            resizable: false,
            content: $("#IndexDeleteOtherCostWarning").data("route") + "?otherCostId=" + otherCostId + "&operationNumber=" + operationNumber,
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
        dialog.center();
        dialog.open();

    }

}

//--------------------------------------------General functions---------------------------------------
function rebindingValitadionFieldsForOutputs(form) {
    $(form).removeData("validator")
          .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
}

function moveUpOtherCost(element) {

    var currentOtherCost = $(element).closest("tr.nivel01");
    var previousOtherCost = $(currentOtherCost).prev("tr.nivel01");

    if (previousOtherCost.length > 0) {

        $(currentOtherCost).insertBefore(previousOtherCost);

    }
}

function moveDownOtherCost(element) {
    var currentOtherCost = $(element).closest("tr.nivel01");

    var nextOtherCost = $(currentOtherCost).next("tr.nivel01");;


    if (nextOtherCost.length > 0) {
        $(currentOtherCost).insertAfter(nextOtherCost);
    }
}

function chekIfItHasBeenChangesFinancial() {
    var registerChangeMatrix = false;

    var newValue = "";
    var oldValue = "";

    var registerChangeMatrix = false;

    $(".minimizeTable").css("z-index", "0");
    var registerChangeMatrix = false;

    var interval = new Object();
    interval.IntervalId = parseInt($("#hdnIntervalId").val());
    interval.CycleId = parseInt($("#hdnCycleId").val());

    if (interval.IntervalId == 3) {

        $("input.ExistingOriginal").each(function (index) {
            var currentValue = parseFloat($(this).autoNumeric('get'));
            var originalValue = parseFloat($(this).data("originalvalue"));

            if (isNaN(currentValue) && isNaN(originalValue)) {
                return true;
            }

            if (currentValue != originalValue) {
                registerChangeMatrix = true;
                return false;
            }
        });

        $("input.ExistingAnnual").each(function () {
            if ($(this).closest("tr.nivel01").
               find("> td.yearPlanData > table > tbody > tr.rm_pa_row > td > label > input.valuechanged").length > 0) {

                var currentValue = parseFloat($(this).autoNumeric('get'));
                var originalValue = parseFloat($(this).data("originalvalue"));
                var oldValue = parseFloat($(this).data("oldvalue"));

                if (isNaN(currentValue) && isNaN(originalValue)) {
                    return true;
                }
                if (isNaN(currentValue) && isNaN(oldValue)) {
                    return true;
                }


                if (currentValue != originalValue && currentValue != oldValue) {
                    registerChangeMatrix = true;
                    return false;
                }
            }
        });

        if (registerChangeMatrix) {
            return true;
        }

        $("input.ExistingAnnualMilestones").each(function () {
            var currentValue = parseFloat($(this).autoNumeric('get'));
            var originalValue = parseFloat($(this).data("originalvalue"));
            var oldValue = parseFloat($(this).data("oldvalue"));

            if (isNaN(currentValue) && isNaN(originalValue)) {
                return true;
            }
            if (isNaN(currentValue) && isNaN(oldValue)) {
                return true;
            }

            if (currentValue != originalValue && currentValue != oldValue) {
                registerChangeMatrix = true;
                return false;
            }
        });
    }

    return registerChangeMatrix;
}
