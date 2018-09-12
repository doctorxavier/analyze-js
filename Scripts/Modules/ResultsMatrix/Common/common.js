var $blockCRF = null;

$(document).ready(function () {

    $("#btnCRFInformation").on('click', function () {
        $blockCRF = $blockCRF === null ? $('.blockCRF') : $blockCRF;
        var $button = $(this);
        if ($button.hasClass('ui-state-active')) {
            $blockCRF.hide();
            $button.removeClass('ui-state-active');

        }
        else {
            $blockCRF.show();
            $button.addClass('ui-state-active');
        };
    });

    $("#showDisaggregation").on('click', function () {
        var $button = $(this);
        if ($button.hasClass('ui-state-active')) {
            $(".minimizeTable .child").slideDown();
            $button.removeClass('ui-state-active');
        }
        else {
            $(".minimizeTable .child").slideUp();
            $button.addClass('ui-state-active');
        };
    });

    $("#showInactive").button().click(function (event) {
        var valueShowInactive = $(this).is(':checked');
        var urlRoute = $("#urlRouteIndex").val();
       
        document.location.href = urlRoute + '&showInactive=' + valueShowInactive;
        //redirectPage(urlRoute + '&showInactive=' + valueShowInactive);
    });

    $('.toolTipDisaggregation').tooltip();

    $('#autoCalEop').on('change', 'input.numberInput', function () {

        if (!$(this).closest("table").closest("td").hasClass("totalOutputYearPlan") &&
            $(this).closest("table").closest("td").closest("tr").find("> td.totalOutputYearPlan").length > 0) {

            totalClassName = "totalOutputYearPlan";
        }
        else if (!$(this).closest("table").closest("td").hasClass("totalMilestoneYearPlan") &&
            $(this).closest("table").closest("td").closest("tr").find("> td.totalMilestoneYearPlan").length > 0) {

            totalClassName = "totalMilestoneYearPlan";
        }

        if (typeof (totalClassName) !== 'undefined') {
            var inputsLabelReferenceYears = $(this).closest("table").closest("td").closest("tr")
                .find('td[data-rowcolumnrelated] > table > tbody > tr.rm_pa_row > td > label > input').map(function () {
                    if (parseInt($(this).val()) >= 0) {
                        return parseInt($(this).closest("table").closest("td").find('> input.hiddenYear').val())
                    }
                });

            var lastYearWithData = inputsLabelReferenceYears.length > 0 ?
                Math.max.apply(Math, inputsLabelReferenceYears) :
                $('[name=CurrentServerYear]').val();

            var pmrYear = $(this).closest("table").closest("td").closest("tr")
                .find('td[data-rowcolumnrelated] > table > tbody > tr > td > label > input[data-pmryear]')
                .attr('data-pmryear');

            var currentReferenceYear = lastYearWithData < pmrYear ?
                lastYearWithData : pmrYear;

            autoCalculateYearPlans(
                $(this).closest("tr").attr("class"),
                $(this),
                totalClassName,
                currentReferenceYear);

            autoCalculatedDisaggregation($(this));
        }
    });

    $('.downloadReport').on('click', function () {
        downloadReport(this);
    });
});

var autoCalculatedDisaggregation = function (element) {
    var firstValue, lastValue, result;
    var referent = element.closest('tr').data('autocalculatedrelated');
    var table = element.closest("table").closest('tbody')
        .find("> tr[data-milestoneindex] > td[data-rowcolumnrelated]")
        .find("[data-autocalculatedrelated=" + referent + "]");
    var parentRow = $(element).parents('tr').eq(0);
    var types = ['calc_p_row', 'calc_pa_row', 'calc_a_row'];

    $.each(table, function (index) {
        switch (index) {
            case 0:
                firstValue = parseFloat($(this).find("> td input:last").autoNumeric('get'));
                break;
            case 1:
                lastValue = parseFloat($(this).find("> td input:last").autoNumeric('get'));
                break;
        }
    });

    if (!isNaN(firstValue) && !isNaN(lastValue)) {
        result = parseFloat((firstValue / lastValue) * 100);
        var yearColumn = element.data('yearcolumn');
        var indicatorInput = element.closest('.nivel02').prev('.nivel01')
            .find('> td[data-yearcolumnrelated=' + yearColumn + ']')
            .find('[data-section=' + getSectionType(parentRow, types) + ']');

        setInput(indicatorInput, (isInfinity(result)) ? '' : result);
    }

    $.each(types, function (index, type) {
        var inputs = element.closest('table').closest('tbody').find('[data-section=' + type + ']');
 
        try {
            firstValue = parseFloat(inputs.eq(0).autoNumeric('get'));
            lastValue = parseFloat(inputs.eq(1).autoNumeric('get'));
        }
        catch (Exception) {
            firstValue = 0;
            lastValue = 0;
        }

        if (!isNaN(firstValue) && !isNaN(lastValue)) {
            result = (firstValue / lastValue) * 100;
            var inputResult = element.closest('.nivel02').prev('.nivel01')
                .find('.totalOutputYearPlan').find('[data-section=' + type + ']');

            setInput(inputResult, (isInfinity(result)) ? '' : result);
        }
    });
}

var setInput = function (input, result) {
    var config = {
        aSign: '',
        pSign: ''
    };

    if(Number.isInteger(result))
        config.mDec = countDecimals(result);

    input.autoNumeric('update', config);
    input.autoNumeric('set', result);
}

var countDecimals = function (value) {
    if (Math.floor(value) === value)
        return 0;
    var length = value.toString().split('.')[1].length || 0;
    return length < 3 ? length : 3;
}

var isInfinity = function (value) {
    return value === Number.POSITIVE_INFINITY || value === Number.NEGATIVE_INFINITY;
}

var getSectionType = function (parentRow, types) {
    if (parentRow.hasClass('rm_ac_row')) {
        return types[2];
    }

    if (parentRow.hasClass('rm_pa_row')) {
        return types[1];
    }

    return types[0];
}

var autoCalculateYearPlans = function (dataType, inputElement, totalClassName, referenceYear) {
    var rowParent = $(inputElement).closest("table").closest("td").closest("tr");
    var currentYear = parseInt($("#CurrentServerYear").val());

    if (referenceYear == undefined) {
        referenceYear = $(inputElement).closest("[data-rowcolumnrelated]").siblings("[data-rowcolumnrelated]").last()
            .find("td>label>input[data-referenceyear]").first().attr('data-referenceyear');
    }

    if (dataType == "rm_p_row") {//update original
        if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_p_row input[readonly]").length > 0) {
            var originalPlanAuto = 0;

            $(rowParent).find("> td[data-rowcolumnrelated]").each(function (index) {
                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_p_row input.numberInput").autoNumeric('get')))) {
                    originalPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_p_row input.numberInput").autoNumeric('get'));
                }
            });

            $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_p_row input:not(:hidden)").autoNumeric('set', originalPlanAuto);
        }
    }
    else if (dataType == "rm_ac_row") {//update actual

        if ($(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_ac_row input[readonly]").length > 0) {
            var actualPlanAuto = 0;

            $(rowParent).find("> td[data-rowcolumnrelated]").each(function (index) {
                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get')))) {
                    actualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_ac_row input").autoNumeric('get'));
                }
            });

            $(rowParent).find("> td." + totalClassName + " > table > tbody > tr.rm_ac_row input").autoNumeric('set', actualPlanAuto);

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
                        $(rowParent).find("> td[data-rowcolumnrelated]").each(function (index) {
                            var columnYear = parseInt($(this).find("> input.hiddenYear").val());

                            if (columnYear < referenceYear) {
                                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input.numberInput").autoNumeric('get')))) {
                                    annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_ac_row input.numberInput").autoNumeric('get'));
                                }
                            }
                            else {
                                if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_pa_row input.numberInput").autoNumeric('get')))) {
                                    annualPlanAuto += parseFloat($(this).find("> table > tbody > tr.rm_pa_row input.numberInput").autoNumeric('get'));
                                }
                            }
                        });
                    }

                    $(rowParent).find("> td." + totalClassName + "> table > tbody > tr.rm_pa_row input").autoNumeric('set', annualPlanAuto);
                }
            }
        }
    }
    else if (dataType == "rm_pa_row") {//update annual

        if ($(rowParent).find("> td." + totalClassName + "> table > tbody > tr.rm_pa_row input[readonly]").length > 0) {
            var annualPlanAuto = 0;
            var hasAnyAnnualValues = false;

            $(rowParent).find("> td:not(." + totalClassName + ")> table > tbody > tr.rm_pa_row input").each(function (index, item) {

                if (!isNaN(parseFloat($(item).autoNumeric('get')))) {
                    hasAnyAnnualValues = true;
                    return false;
                }
            });

            if (hasAnyAnnualValues) {
                $(rowParent).find("> td[data-rowcolumnrelated]").each(function (index) {
                    var columnYear = parseInt($(this).find("> input.hiddenYear").val());

                    if (columnYear < referenceYear) {
                        if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_ac_row input.numberInput").autoNumeric('get')))) {
                            annualPlanAuto = annualPlanAuto + parseFloat($(this).find("> table > tbody > tr.rm_ac_row input.numberInput").autoNumeric('get'));
                        }
                    }
                    else {
                        if (!isNaN(parseFloat($(this).find("> table > tbody > tr.rm_pa_row input.numberInput").autoNumeric('get')))) {
                            annualPlanAuto = annualPlanAuto + parseFloat($(this).find("> table > tbody > tr.rm_pa_row input.numberInput").autoNumeric('get'));
                        }
                    }
                });
            }

            $(rowParent).find("> td." + totalClassName + "> table > tbody > tr.rm_pa_row input").autoNumeric('set', annualPlanAuto);
        }
    }
}

function addNewYearPlanColumGlobal(gridBody, rowHead, rowFollowingYear, impacctIndicatorId) {
    var impactTables = $(".mod_tabla_impacts");

    $.each(impactTables, function (index, value) {
        var gridBodyId = $(value).find('[id^="bodyTable_"]');
        var rowHeadId = $(value).find('[id^="yearPlans_"]');
        var followingId = $(value).find('[id^="followingYear_"]');
        var rowHeadText = $(rowHeadId).attr('id');

        if (rowHeadText != null && rowHeadText != "") {
            var indicatorId = rowHeadText.substring(rowHeadText.indexOf("_") + 1);
            addNewYearPlanColum(gridBodyId, rowHeadId, followingId, indicatorId, 3);
        }
    });

    $('.auto').autoNumeric('init');
    fixDropdonwYears();
}

function addNewYearPlanColum(gridBody, rowHead, rowFollowingYear, impactIndex, impactIndicatorId, year) {

    var table = $(gridBody).parent();
    var rowHeadx = null;
    rowHeadx = $(table).find("thead .rowYearPlan");
    var thead = $(table).find("thead");
    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var displayModeStartUpPlan = false;
    var displayModeAnnualPlan = false;
    var displayModeActualPlan = false;
    var currentColSpan = parseInt($(rowFollowingYear).attr("colspan"));

    currentColSpan += 1;

    var currentYearsPlanRows = currentColSpan - 2;
    var lastSelectedOption = new Date().getFullYear();
    var lastOption = $(rowHeadx).find('[id="lastCellHead"]').prev().find("select option:selected").text();
    if (lastOption.length > 0) {
        lastSelectedOption = parseInt(lastOption) + 1;
    }

    if (year != null && year != lastSelectedOption) {
        lastSelectedOption = year;
    }

    $(rowFollowingYear).attr("colspan", currentColSpan);
    var dropDownYears = DropDownYearsPlan($(rowHeadx).find('[id="lastCellHead"]'), impactIndex, year);
    var insertedTh = $('<th class="btn_azul_oscuro selects years-impact">' + dropDownYears + '</th>');
    var prevColumn = $(rowHeadx).find('[id="lastCellHead"]');
    $(insertedTh).insertBefore(prevColumn);

    var insertedSelect = $(insertedTh).find("select");

    $(insertedSelect).kendoDropDownList();

    var isValidated = isValidatedYearPlan($(insertedSelect).find("option:selected").text());

    if (isValidated) {
        $(insertedTh).attr("title", $("#hdnInactiveYearPlanToolTip").val());
    }

    var impactIndicatorRows = $(gridBody).find('[id^="impactIndicatorRow_"]');
    var tHeadLixtBox = $(table).find("thead select");

    $.each(impactIndicatorRows, function (index, value) {
        lastSelectedListBox = $(rowHeadx).find("select option:selected").text();

        var cellIndex = parseInt($(this).find("input:text").length);

        lastSelectedListBox = $(rowHeadx).find("select").length;

        var nextIndex = lastSelectedListBox - 1;
        var impactIndicatorId = $("#Impacts_" + impactIndex + "__ImpactIndicators_" + index + "__ImpactIndicatorId").val();

        displayModeStartUpPlan = false; displayModeAnnualPlan = false; displayModeActualPlan = false;

        displayModeStartUpPlan = BlockFieldStartUpPlan(interval);
        displayModeAnnualPlan = BlockFieldAnnualPlan(interval, lastSelectedOption);
        displayModeActualPlan = BlockFieldActualPlan(interval, lastSelectedOption);

        var type = $('#typeOfModule').val();
        var referenceYear = parseInt($('[name="referenceYear"]').val());
        var isAutoCalculated = Boolean($('[name="isSpecialAutoCalculated"]').val());
        var hiddenFields = getHiddenFields(impactIndex, index, impactIndicatorId, nextIndex, -1, lastSelectedOption);

        var content = "";
        content += '<td data-rowcolumnrelated=' + lastSelectedOption + ' class="dato07" data-yearcolumnrelated=' + lastSelectedOption + '>';
        content += '<input class="hiddenYear" id="{type}s_' + impactIndex + '__{type}Indicators_' + index + '__{type}IndicatorYearPlans_' + nextIndex + '__Year" name="{type}s[' + impactIndex + '].{type}Indicators[' + index + '].{type}IndicatorYearPlans[' + nextIndex + '].Year" type="hidden" value=' + lastSelectedOption + '>';
        content += '<table class="ouputTable"> ';
        content += '<tbody style="border-width:0px"> ';
        content += '<tr class="rm_p_row"> ';
        content += '<td class="rm_edit_rh"> ';

        content += '<label class="editLabel" for=""> ' + hiddenFields;

        content += '<input class="input min_input numberInput ' + (displayModeStartUpPlan ? "bcGray" : "") + ' auto" data-section="calc_p_row" data-referenceyear=' + referenceYear +
            ' id="{type}s_' + impactIndex + '__{type}Indicators_' + index + '__{type}IndicatorYearPlans_' + nextIndex + '__OriginalPlan" name="{type}s[' + impactIndex + '].{type}Indicators[' + index +
            '].{type}IndicatorYearPlans[' + nextIndex + '].OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" ' +
            (displayModeStartUpPlan ? "readonly='readonly'" : "") + '> ';

        content += '</label> ';
        content += '</td> ';
        content += '</tr> ';
        content += '<tr class="rm_pa_row"> ';
        content += '<td class="rm_edit_rh"> ';
        content += '<label class="editLabel" for=""> ';

        content += '<input class="input min_input numberInput ' + (displayModeAnnualPlan ? "bcGray" : "") + ' auto" data-section="calc_pa_row" data-referenceyear=' + referenceYear +
            ' id="{type}s_' + impactIndex + '__{type}Indicators_' + index + '__{type}IndicatorYearPlans_' + nextIndex + '__AnnualPlan" name="{type}s[' + impactIndex + '].{type}Indicators[' + index +
            '].{type}IndicatorYearPlans[' + nextIndex + '].AnnualPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" ' +
            (displayModeAnnualPlan ? "readonly='readonly'" : "") + '> ';

        content += '</label> ';
        content += '</td> ';
        content += '</tr> ';
        content += '<tr class="rm_ac_row"> ';
        content += '<td class="rm_edit_rh"> ';
        content += '<label class="editLabel" for=""> ';

        content += '<input class="input min_input numberInput ' + (displayModeActualPlan ? "bcGray" : "") + ' auto" data-section="calc_a_row" data-referenceyear=' + referenceYear +
            ' id="{type}s_' + impactIndex + '__{type}Indicators_' + index + '__{type}IndicatorYearPlans_' + nextIndex + '__ActualValue" name="{type}s[' + impactIndex + '].{type}Indicators[' + index +
            '].{type}IndicatorYearPlans[' + nextIndex + '].ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" ' +
            (displayModeActualPlan ? "readonly='readonly'" : "") + '> ';

        content += '</label> ';
        content += '</td> ';
        content += '</tr> ';
        content += '</tbody> ';
        content += '</table> ';
        content += '</td> ';

        content = content.replaceAll("{type}", type);
        content.trim();

        $(content).insertBefore($(this).find('[id="lastCellContent"]').prev());

    });

    resizeDissagregationTable(gridBody);
    addNewYearPlanDissagregation(gridBody, lastSelectedOption, impactIndex);
}

function downloadReport(source) {
    //redirectPage($(source).data('href'));
    document.location.href = $(source).data('href');
    return false;
}

function fixDropdonwYears(outputs) {

    outputs = arguments.length == 0 ? false : true;

    var $dropdownYears = $('.years-impact .k-input');
    var $dropdownBaselineYears = $('.dato02 .k-input');

    $dropdownBaselineYears.css('cssText',
        'font-size: xx-small !important; text-align: center; padding-top: 10%');

    if (outputs) {
        $dropdownYears.css('cssText',
            'font-size: xx-small !important; text-align: center; padding-top: 10%; ' +
            'margin-top: 3% !important; margin-right: 4px');
    } else {
        $dropdownYears.css('cssText',
            'font-size: xx-small !important; text-align: center; padding-top: 10%;');
    }
}