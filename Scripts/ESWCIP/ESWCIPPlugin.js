//Functions required for ESWCIP module.

//Add a new row to table after clicking on a new element button or when creating new element.
//ponerle una clase a la tabla para poder identificarla al hacer click en el button
function addNewRow(table, newRow) {
    table.append(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&/g, '&').replace("\r\n", "").replace("\n", ""));
}

// This function works with a parent checkBox and children related to them. TODO: Improve it so it's not necessary to use clases and data attributes the way it's done currently.
function checkAllInputCheckBoxes() {
    var childrenCheckAll = $(":checkbox[name=childrenCheckAll]");
    var parentCheckBox = $(".checkAllParentCheckBox :checkbox");

    // Whenever the parent checkbox is checked/unchecked
    parentCheckBox.change(function () {
        // save state of parent
        var c = $(this).is(':checked');
        childrenCheckAll.each(function () {
            // set state of siblings
            if (!($(this).parents('tr').hasClass('hide')))
            {
                $(this).prop('checked', c);
            } else {
                $(this).prop('checked', false);
            }
        });
    });

    // Update parent checkbox based on children
    childrenCheckAll.change(function () {
        if (childrenCheckAll.not(':checked').length < 1) {
            parentCheckBox.prop('checked', true);
        } else {
            parentCheckBox.prop('checked', false);
        }
    });
}

function calculateSum(columnElement, totalElement) {
    var sum = 0;

    columnElement.each(function () {

        var value = $(this).text().replace(/,/g, "");
 
        if (!isNaN(value) && value.length != 0) {
            sum += parseFloat(value);
        }

    });

    totalElement.text(numberWithCommas(sum.toFixed(2).toString()));

}

function calculateInputSum(columnElement, totalElement) {
    var sum = 0;

    columnElement.each(function () {

        var value = $(this).val().replace(/,/g, "");

        if (!isNaN(value) && value.length != 0) {
            sum += parseFloat(value);
        }

    });

    totalElement.text(numberWithCommas(sum.toFixed(2).toString()));
    formatNumberAndDecimalLabelText(totalElement);
}

function totalRequestSum() {
    $('#ESWCIPBudgetsTable').find(".rowContent").each(function () {
        var newTotalRequest = 0;
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.inputConsultation').val().replace(/,/g, ""));
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.inputTravel').val().replace(/,/g, ""));
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.inputOther').val().replace(/,/g, ""));
        $(this).find('.lblTotalRequest').text(numberWithCommas(newTotalRequest.toFixed(2).toString()));
        formatNumberAndDecimalLabelText($(this).find('.lblTotalRequest'));
    });
}

function totalRequestReadSum() {
    $('table#ESWCIPBudgetsTable .rowContent').each(function () {
        var newTotalRequest = 0;
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.lblConsultation').text());
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.lblTravel').text());
        newTotalRequest = sumValue(newTotalRequest, $(this).find('.lblOther').text());
        $(this).find('.lblTotalRequest').text(numberWithCommas(newTotalRequest.toFixed(2).toString()));
    });
}

function sumValue(newTotalRequest, value) {
    if (!isNaN(value.replace(/,/g, "")) && value.length != 0) { newTotalRequest += parseFloat(value.replace(/,/g, "")); }

    return newTotalRequest;
}

function sumInputs() {
    calculateInputSum($('.inputConsultation'), $('.consultationTotal'));
    calculateInputSum($('.inputTravel'), $('.travelTotal'));
    calculateInputSum($('.inputOther'), $('.otherTotal'));
    calculateSum($('label.lblTotalRequest'), $('.totalRequestTotal'));
    calculateInputSum($('.inputOtherFinancing'), $('.otherFinancingTotal'));
    $('#proposedAmmount label:last').text($('.totalRequestTotal').text());
    formatNumberAndDecimalLabelText($('.totalRequestTotal'));
    formatNumberAndDecimalLabelText($('#proposedAmmount label:last'));
    $('[name=RequestedAmount]').val($('.totalRequestTotal').text());
}

function editSummaryRowsCalculation() {
    $('#ESWCIPBudgetsTable td input').change(function () {
        totalRequestSum();
        sumInputs();
    });
}

function readSummaryRowsCalculation() {
    calculateSum($('.lblConsultation'), $('.consultationTotal'));
    calculateSum($('.lblTravel'), $('.travelTotal'));
    calculateSum($('.lblOther'), $('.otherTotal'));
    calculateSum($('.lblTotalRequest'), $('.totalRequestTotal'));
    calculateSum($('.lblOtherFinancing'), $('.otherFinancingTotal'));
    $('#proposedAmmount label:last').text($('.totalRequestTotal').text());
    formatNumberAndDecimalLabelText($('#proposedAmmount label:last'));
    $('[name=RequestedAmount]').val($('.totalRequestTotal').text());
}

function cleanUnnecessaryZeroesFromInputsOnEditMode() {
    $('[name=AllocatedAmount]').each(function () {
        cleanZeroesEdit($(this));
    });

    $('[name=RevisedAmount]').each(function () {
        cleanZeroesEdit($(this));
    });
}

function cleanUnnecessaryZeroesOnReadMode() {
    $('#tblAllocation tbody tr td span[data-pagemode=read]').each(function () {
        cleanZeroes($(this));
    });

    $('td.requestedAmount').each(function () {
        cleanZeroes($(this));
    });
}

function cleanZeroes(element) {
    if (element.html().trim() === "0.00") {
        element.html("");
    }
}

function cleanZeroesEdit(element) {
    if (element.val() === "0.00" || element.val() === "0") {
        element.val("");
    }
}

function checkboxInputClick(sourceCheckBox, destinationCheck)
{
    $('input[name=' + sourceCheckBox + ']').change(function () {
        if ($(this).is(':checked')) {
            $(this).parents('tr').find('span.showDataEdit [name=' + destinationCheck  + ']').attr('disabled', 'disabled');
        }
        else {
            $(this).parents('tr').find('span.showDataEdit [name=' + destinationCheck + ']').removeAttr('disabled');
        }
    });
}

function insertNewRowBeforeLast(table, newRow) {
    table.before(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177').replace("\r\n", "").replace("\n", ""));
}

function separateListIntoSeveralDivs() {
    var divs = $(".countries > div");

    for (var i = 0; i < divs.length; i += 8) {
        divs.slice(i, i + 8).wrapAll("<div class='col-md-12'></div>");
    }
}

function cleanEmptyRows(tableRows) {
    tableRows.each(function () {
        if ($(this).find('span[data-pagemode=read]').length === 0) $(this).remove();
    });
}

function updateResultsPerPage(sourceList, destinationTable) {
    sourceList.click(function () {
        if ($(this).find('a[dd-selected]').length > 0) {
            destinationTable.paginationConfluense(parseInt($(this).find('a').text()));
        }
    });
}

//function to set datepickers minDate

function setDatePickerMinDate(datePickerElements) {
    datePickerElements.datepicker("option", "minDate", 0);
}

function allowOnlyNumbersOnYear() {
    $("[name=relatedOperationYear]").keypress(function (e) {
        if (parseInt(e.keyCode) > 47 && parseInt(e.keyCode) < 58) {
            if (parseInt($(this).val().length) <= 3) {
            }
            else {
                e.preventDefault();
            }
        }
        else {
            e.preventDefault();
        }
    });
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function maskNumbersWithCommas() {
    var reqAmount = $('#requestedAmountGeneralInfo label').first().text();
    $('#requestedAmountGeneralInfo label').first().text(numberWithCommas(reqAmount));
    AddNumberWithCommasToElements($('.lblConsultation'));
    AddNumberWithCommasToElements($('.lblTravel'));
    AddNumberWithCommasToElements($('.lblOther'));
    AddNumberWithCommasToElements($('.lblOtherFinancing'));
}

function AddNumberWithCommasToElements(selectedElement) {
    selectedElement.each(function () {
        var value = $(this).text();
        $(this).text(numberWithCommas(value));
    });
}

function addNumbersBeforeQuestions(tabElementId) {
    $(tabElementId).find('h4').each(function (index) {
        var number = index + 1;
        var question = $(this).html();
        $(this).html(number + ". " + question);
    })
}

function maskAnnualAllocationAmountsOnReadMode() {
    $('td.requestedAmount').each(function () {
        if ($(this).html().trim() !== "0" && $(this).html().trim() !== "0.00" && $(this).html().trim() !== "") {
            var formattedAmount = numberWithCommas(parseFloat($(this).html().replace(/,/g, '')));
            $(this).html(formattedAmount);
        }
    });

    $('td.allocatedAmount,td.revisedAmount').each(function () {
        if ($(this).find('span[data-pagemode=read]').html().trim() !== "0" && $(this).find('span[data-pagemode=read]').html().trim() !== "0.00" && $(this).find('span[data-pagemode=read]').html().trim() !== "") {
            var formattedAmount = numberWithCommas(parseFloat($(this).find('span[data-pagemode=read]').html().replace(/,/g, '')));
            $(this).find('span[data-pagemode=read]').html(formattedAmount);
        }
    });
}

function maskAnnualBudgetsOnReadMode() {
    $('.lblConsultation,.lblTravel,label.lblOther,.lblOther,.lblTotalRequest,.lblOtherFinancing,.consultationTotal,.travelTotal,.otherTotal,.totalRequestTotal,.otherFinancingTotal').
        each(function () {
            formatNumberAndDecimalLabelText($(this));
        });
}

function formatNumberAndDecimalLabelText(numberElement) {
    if (numberElement.html().trim() !== "0" && numberElement.html().trim() !== "0.00" && numberElement.html().trim() !== "") {
        var formattedAmount = numberWithCommas(parseFloat(numberElement.text().replace(/,/g, '')));
        numberElement.html(formattedAmount);
    } else {
        numberElement.html("0");
    }
}

function formatNumberAndDecimalSpanText(numberElement) {
    if (numberElement.find('span[data-pagemode=read]').html().trim() !== "0" && numberElement.find('span[data-pagemode=read]').html().trim() !== "0.00" && numberElement.find('span[data-pagemode=read]').html().trim() !== "") {
        var formattedAmount = numberWithCommas(parseFloat(numberElement.find('span[data-pagemode=read]').text()));
        numberElement.find('span[data-pagemode=read]').html(formattedAmount);
    }
}

function addTooltip(titleElement) {
    titleElement.tooltip();
}

function addToolTipForTitles() {
    $("#tabOERA, #tabRisks").find("h4").each(function () {
        if ($(this).attr('title') !== "") {
            addTooltip($(this))
        }
    });
}

function budgetZerosAddition() {
    $('#ESWCIPBudgetsTable tbody tr td span[data-pagemode=read] label').each(function () {
        if ($(this).html().trim() === "" || $(this).html().trim() === "0.00") {
            $(this).html("0");
        }
    });
}

function centerButtonTrash() {
    $('[name=Delete]').css('margin-top', '0px');
}