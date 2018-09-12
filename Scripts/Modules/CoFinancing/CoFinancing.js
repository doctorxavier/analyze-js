coFinancing = {};

coFinancing.zeroAmountValue = "0.00";
coFinancing.emptyString = "";
coFinancing.counterPart = "Counterpart";
coFinancing.counterPartAmount = "CounterPartAmount";
coFinancing.coFinancingAmount = "CoFinancingAmount";
coFinancing.onEditMode = "OnEditMode";
coFinancing.onReadMode = "OnReadMode";
coFinancing.inKind = "IN_KIND";
coFinancing.zeroValue = 0;
coFinancing.OTHER = "OTHER";

coFinancing.fundingSourceTextChange = function () {
    $('[name$="FundingSource_text"]').on('keyup click', function () {
        var currentElement = $(this);

        if (currentElement.val() !== "") {
            currentElement.parent().find('ul.dropdown-menu').
                find("[dd-value$=OTHER_PS_PUBLIC]").
                parent().
                removeClass('hidden');

            var autoCompleteDiv = currentElement.closest('div.ctlComplete');

            if (!autoCompleteDiv.hasClass("open")) {
                autoCompleteDiv.addClass('open');
            }
        }

        var errorsList = currentElement.parent().find('ul.parsley-errors-list');

        errorsList.removeClass("filled");
        errorsList.find('li').removeClass('hidden');
    });
};
coFinancing.fillFundingSource = function () {
    var elements = $('[name$=FundingSource_text]');

    $.each(elements, function () {
        var currentElement = $(this).parent();

        var selectedListElement = currentElement.
                                      find('ul.dropdown-menu').
                                      find('li').
                                      find('a[dd-selected]');

        var value = selectedListElement.attr('dd-value');
        var text = selectedListElement.html().trim();

        currentElement.SetValue(value, text);
    });
};

coFinancing.fundingSourceSelection = function () {
    $('[aria-labelledby$="FundingSource"] li a').click(function () {
        var selectedElement = $(this);
        var selectedElementGroup = selectedElement.attr('dd-group-name').split('_').pop();
        var cashInKindDropdown = selectedElement.
                                     parents('tr').
                                     find('[id$=CashOrInKind]');

        if (selectedElementGroup !== "BOTH") {
            selectedElementGroup = selectedElementGroup === CoFinancingValues.KIND ?
                                       CoFinancingValues.IN_KIND : selectedElementGroup;

            var cashInKindDropdownElementToBeSelected = cashInKindDropdown.parent('div.dropdown').
                find('[aria-labelledby$=CashOrInKind]').
                find('li').find('a[dd-value=' + selectedElementGroup + ']');

            cashInKindDropdown.parent('div.dropdown').removeClass("placeholder");
            cashInKindDropdown.SetValue(cashInKindDropdownElementToBeSelected.
                attr('dd-value'),
                cashInKindDropdownElementToBeSelected.text().trim());
        } else {
            cashInKindDropdown.FirstorDefault();
        }
    });
};

coFinancing.makeDescriptionRequired = function (currentValue, currentElement) {
    var inKindValues = CoFinancingValues.InKindValuesArray;

    if (inKindValues.indexOf(currentValue) >= 0) {
        currentElement.parents('tr').
            find("[name$=Description]").
            attr("data-parsley-required", true).
            attr("required", "required");
    } else {
        var currentElementRow = currentElement.parents('tr');

        var fundingSourceValue = currentElementRow.
                                    find('[name$="FundingSource_text"]').
                                    parent().
                                    find('ul.dropdown-menu').
                                    find('li').
                                    find('a[dd-selected]').
                                    attr('dd-value');

        if (fundingSourceValue !== undefined) {
            if (fundingSourceValue.indexOf(coFinancing.OTHER) < 0) {
                currentElementRow.find("[name$=Description]").
                    attr("data-parsley-required", false).
                    attr("required", false).
                    removeClass("parsley-error");
            }
        }
    }
};

coFinancing.modalityClick = function () {
    $("[name=CoFinancingModality]").change(function () {
        var currentElement = $(this);
        var currentValue = currentElement.attr('value');

        window.setTimeout(function () {
            refreshPieChartValues();
            coFinancing.makeDescriptionRequired(currentValue, currentElement);
        }, 5);
    });
};

coFinancing.typeClick = function () {
    $('[name=CounterpartFinancingCashOrInKind], [name="CoFinancingCashOrInKind"]').change(function () {
        var currentElement = $(this);
        var currentValue = currentElement.attr('value');

        window.setTimeout(function () {
            refreshPieChartValues();
            coFinancing.makeDescriptionRequired(currentValue, currentElement);
        }, 5);
    });
};

coFinancing.amountsFocusOut = function () {
    $("input[name=CounterPartAmount], input[name=CoFinancingAmount]").focusout(function () {
        coFinancing.updateGridTotalValuesAndRefreshPieChartData($(this).attr('name'));
    });
};

coFinancing.updateGridTotalValuesAndRefreshPieChartData = function (elementsName) {
    var total = 0;
    var pageModes = [coFinancing.onEditMode, coFinancing.onReadMode];
    var elementsToBeSum = $('#PageContent [name=' + elementsName + ']');
    var elementsToBeSumLength = elementsToBeSum.length;
    elementsToBeSum = elementsToBeSumLength > coFinancing.zeroValue ?
                        elementsToBeSum : coFinancing.loadElementsToBeSum(elementsName, pageModes[1]);

    total = coFinancing.sumAmountElements(elementsToBeSum, CoFinancingValues.CANCELLED);

    var totalName = elementsName.charAt(0).toLowerCase() + elementsName.substr(1);

    pageModes.forEach(function (pageMode) {
        $("#PageContent").find("label." + totalName + pageMode).text(total);
    });

    refreshValues();
    grafico();
}

coFinancing.loadElementsToBeSum = function (elementsName, pageMode) {
    var elementsToBeFound =
        elementsName.toLowerCase() === coFinancing.counterPartAmount.toLocaleLowerCase() ?
            coFinancing.counterPartAmount : elementsName

    var elementsToBeSumPageMode = pageMode.replace("On", coFinancing.emptyString);
    var elementsToBeSum = $('#PageContent [data-pagemode=read] label.' + elementsToBeFound + elementsToBeSumPageMode);
    return elementsToBeSum;
}

coFinancing.sumAmountElements = function (elementsToBeSum, cancelledValue) {
    var total = 0;

    var filteredElementsToBeSum = 
        coFinancing.filterElementsToBeSumByStatus(elementsToBeSum, cancelledValue);

    total = coFinancing.calculateTotalAmountByType(filteredElementsToBeSum);

    return total;
}

coFinancing.filterElementsToBeSumByStatus = function (elementsToBeSum, cancelledValue) {
    var filteredElementsToBeSum = elementsToBeSum.filter(function () {
        var currentElement = $(this);
        var currentElementParentRow = currentElement.parents("tr");
        var statusValue = currentElementParentRow.find("[id$=Status]").GetValue();

        if (statusValue === null) {
            statusValue =
                currentElementParentRow.
                    find("[data-pagemode=read] statusReadMode").attr('data-statusCodeValue');
        }

        if (statusValue !== cancelledValue) {
            return currentElement;
        }
    });

    return filteredElementsToBeSum;
}


coFinancing.totalValuesFocusOut = function () {
    $("input[name='CounterpartFinancingAmount'], input[name='CoFinancingAmmountTotalValue']").
        focusout(function () {
            refreshValues();
            grafico();
        });
};

coFinancing.fundingSourceTextFocusOut = function () {
    $('[name$="FundingSource_text"]').focusout(function () {
        var currentElement = $(this);
        var value = "";
        var text = "";
        var currentElementRow = currentElement.parents('tr');

        if (currentElement.val() !== "") {
            var selectedListElement = currentElement.
                                          parent().
                                          find('ul.dropdown-menu').
                                          find('li').
                                          find('a[dd-selected]');

            var value = selectedListElement.attr('dd-value');
            var text = selectedListElement.html().trim();
            var descriptionElement = currentElementRow.find("[name$=Description]");

            if (value.indexOf(coFinancing.OTHER) >= 0) {
                descriptionElement.attr("data-parsley-required", true);
            }
        } else {
            currentElementRow.
                find('[id$=CashOrInKind]').
                FirstorDefault();
        }

        currentElement.SetValue(value, text);
    });
}

coFinancing.cleanFundingSourceErrorLists = function () {
    $('#PageContent').find('.ctlComplete').
        find('ul.parsley-errors-list.filled').
        find('li.hidden').
        removeClass('hidden');
};

coFinancing.callDescriptionRequired = function (elementRow, elementType) {
    elementRow.find("[aria-labelledby=" + elementType + "] li a[dd-selected]").click();
};

coFinancing.loadEditModeFunctions = function () {
    coFinancing.fundingSourceTextChange();
    coFinancing.amountsFocusOut();
    coFinancing.totalValuesFocusOut
    coFinancing.fundingSourceTextFocusOut();
    coFinancing.modalityClick();
    coFinancing.typeClick();
    coFinancing.fundingSourceSelection();
    coFinancing.statusChange();
}

coFinancing.addNewRowCounterpartFinancing = function() {
    $("#PageContent button[name=CounterpartFinancing]").click(function () {
        coFinancing.addNewRowForCounterPartOrCoFinancing("CounterpartFinancing");
        coFinancing.removeNAElements();
    });
}

coFinancing.addNewRowCoFinancing = function() {
    $("#PageContent button[name=CoFinancing]").click(function () {
        coFinancing.addNewRowForCounterPartOrCoFinancing("CoFinancing");
        coFinancing.removeNAElements();
    });
}

coFinancing.addNewRowForCounterPartOrCoFinancing = function (sourceTable) {
    var container = $('#PageContent').find("table#" + sourceTable).find('tbody');
    var isCofinancing = sourceTable === "CoFinancing";

    var action = isCofinancing ?
                     "GetRowComplementaryResource" : "GetRowCounterPartFinancing";

    $.get("../View/" + action, function (data) {
        var rowsContainer = $(container);

        rowsContainer.append(data);

        rowsContainer.find('tr').
            not('[data-id]').
            remove();

        coFinancing.loadEditModeFunctions();

        if (isCofinancing) {
            coFinancing.reloadCofinancingRemovalFunction();
        } else {
            coFinancing.reloadCounterPartRemovalFunction();
        }

        bindHandlers();
        enterEditMode(false, rowsContainer, false);

        var noRecordsElement =
            $('#PageContent div.col-md-12.noRecords.' + sourceTable).
                find('p.noRecords');

        if (noRecordsElement.length > 0) {
            noRecordsElement.parent().remove();
        }

        var spanExpandCollapseIcon = container.
                                         parent().
                                         find('thead').
                                         find('span.icon');

        if (spanExpandCollapseIcon.text().trim() === '+') {
            spanExpandCollapseIcon.click();
        }
    });
};

coFinancing.loadRowsByType = function (currentRows, elementType, dropdownToGetComparedValue) {
    return (currentRows.filter(function () {
        var currentRow = $(this);
        var cashOrInKindValue =
            currentRow.find('#' + dropdownToGetComparedValue).GetValue();

        if (cashOrInKindValue === null) {
            var dropdownText = dropdownToGetComparedValue;
            var labelClass = dropdownText.substring(dropdownText.indexOf("-") + 1);
            var labelCashOrInKindValue = currentRow.find("label." + labelClass).first();
            cashOrInKindValue = labelCashOrInKindValue.text().trim().toUpperCase();
            cashOrInKindValue = cashOrInKindValue.replace("-", "_");
        }

        if (cashOrInKindValue === elementType) {
            return currentRow;
        }
    }));
}

coFinancing.removeCoFinancing = function() {
    $("button[name=removeCoFinancing]").click(function () {
        var currentElement = $(this);

        confirmAction(CoFinancingMessages.DeleteFundWarning).done(function (pressOk) {
            if (pressOk) {
                currentElementTableBody = currentElement.parents('tbody');
                currentElement.parents("tr[data-id]").remove();
                coFinancing.updateGridTotalValuesAndRefreshPieChartData("CoFinancingAmount");

                $("#PageContent p.noRecords.CoFinancing").parent().remove();

                if (currentElementTableBody.find('tr[data-id]').length === 0) {

                    if (currentElementTableBody.find("tr.no-height").length === 0) {
                        currentElementTableBody.append("<tr class='no-height'></tr>");
                    }

                    currentElementTableBody.
                        parents('table').
                        after("<div class='col-md-12 noRecords CoFinancing'><p class='noRecords CoFinancing text-center mb10'>" +
                        CoFinancingMessages.NoRecords +
                        "</p></div>");
                }
            }
        });
    });
}

coFinancing.removeCounterPart = function() {
    $("button[name=removeCounterPart]").click(function () {
        var currentElement = $(this);

        confirmAction(CoFinancingMessages.DeleteFundWarning).done(function (pressOk) {
            if (pressOk) {
                currentElementTableBody = currentElement.parents('tbody');
                currentElement.parents("tr[data-id]").remove();
                coFinancing.updateGridTotalValuesAndRefreshPieChartData("CounterPartAmount");

                $("#PageContent p.noRecords.CounterpartFinancing").parent().remove();

                if (currentElementTableBody.find('tr[data-id]').length === 0) {

                    if (currentElementTableBody.find("tr.no-height").length === 0) {
                        currentElementTableBody.append("<tr class='no-height'></tr>");
                    }

                    currentElementTableBody.
                        parents('table').
                        after("<div class='col-md-12 noRecords CounterpartFinancing'><p class='noRecords CounterpartFinancing text-center mb10'>" +
                        CoFinancingMessages.NoRecords +
                        "</p></div>");
                }
            }
        });
    });
}

coFinancing.reloadCounterPartRemovalFunction = function () {
    $("button[name=removeCounterPart]").unbind("click");
    coFinancing.removeCounterPart();
}

coFinancing.reloadCofinancingRemovalFunction = function () {
    $("button[name=removeCoFinancing]").unbind("click");
    coFinancing.removeCoFinancing();
}

coFinancing.calculateTotalAmountByType = function(elementsToBeSum) {
    var sum = 0;
    var value = "";

    elementsToBeSum.each(function () {
        var currentElement = $(this);

        if (currentElement.is('input:text')) {
            value = currentElement.val().trim().replace(/,/g, "");
        } else {
            value = currentElement.text().trim().replace(/,/g, "");
        }

        if (!isNaN(value) && value.length != 0) {
            sum += parseFloat(value);
        }
    });

    return sum.toFixed(2).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

coFinancing.totalCofinancingCalculation = function(cofinancingTable, isParallel) {
    var totalCofinancing = 0;
    var totalParallelCofinancing = 0;
    var totalToBeReturned = 0;
    var parallelGrant = CoFinancingParallelValues.PARALLEL_GRANT;
    var parallelLoan = CoFinancingParallelValues.PARALLEL_LOAN;
    var cofinancingPLModalities = [parallelGrant, parallelLoan];
    var filteredRows = cofinancingTable.find("tbody").find("tr").not('.no-height').filter(
        function () {
            return $(this).find("#id-CoFinancingStatus").GetValue() !== CoFinancingParallelValues.CANCELLED;
        });

    filteredRows.each(function () {
        var currentRow = $(this);

        var currentRowSelectedModality =
            currentRow.find('#id-CoFinancingModality').GetValue();

        var isSelectedModalityPL =
            cofinancingPLModalities.indexOf(currentRowSelectedModality) >= 0;

        var currentVal =
            currentRow.find("input:text[name='CoFinancingAmount']").val().replace(/,/g, '') * 1;

        if (!isSelectedModalityPL) {
            totalCofinancing += currentVal;
        } else {
            totalParallelCofinancing += currentVal;
        }
    });

    var totalToBeReturned = isParallel ? totalParallelCofinancing : totalCofinancing;

    return totalToBeReturned;
}

coFinancing.validateAmounts = function (elements) {
    var isValid = true;

    $.each(elements, function (index, element) {
        var elementValue = $(element).val();

        var isEmptyValue = (elementValue === coFinancing.zeroAmountValue) ||
            (elementValue.indexOf("-") >= 0);

        var elementClosestTd = $(element).closest("td");

        if (isEmptyValue) {
            elementClosestTd.find('ul').addClass("filled").find("li").remove();
            elementClosestTd.find('ul')
                .append("<li class='usRequired'>" +
                    coFinancingAmountValidationMessages.RequiredAmountMessage +
                    "</li>");

            isValid = false;
        } else {

            if (elementClosestTd.find('li.usRequired').length == 1 &&
                elementClosestTd.find('li').length == 1) {
                elementClosestTd.find('ul')
                    .removeClass("filled").find("li.usRequired").remove();
            }
            if ((elementClosestTd.find('li.usRequired').length == 1 &&
                elementClosestTd.find('li').length > 1)) {
                elementClosestTd.find('ul').find("li.usRequired").remove();
            }
        }
    });

    return isValid;
}

coFinancing.statusChange = function () {
    $('[name$=Status]').change(function () {
        var elementsName = $(this).attr('name');
        var elementsToUpdateValues =
            elementsName.indexOf(coFinancing.counterPart) >= 0 ?
                coFinancing.counterPartAmount : coFinancing.coFinancingAmount;

        window.setTimeout(function () {
            coFinancing.updateGridTotalValuesAndRefreshPieChartData(elementsToUpdateValues);
        }, 5);
    });
};

coFinancing.hideRowsWithErrors = function () {
    var tables = $("#PageContent").find("table").not('.gridExpectedIDB');

    $.each(tables, function () {
        var currentTable = $(this);
        var isHeaderInvisible =
            currentTable.find("thead").find("tr").last().hasClass("hide");

        if (isHeaderInvisible) {
            currentTable.find("tbody").find("tr[data-id=0]").addClass("hide");
        }
    });
};

coFinancing.refreshTotalAmounts = function () {
    var elementsNames = [coFinancing.counterPartAmount, coFinancing.coFinancingAmount];

    elementsNames.forEach(function (element) {
        coFinancing.updateGridTotalValuesAndRefreshPieChartData(element);
    });
};

coFinancing.removeNAElements = function () {
    var dropdownElements = $('#PageContent').find('.dropdown').not('#id-PipelineCategoryId');
    var autocompleteElements = $('#PageContent').find('.ctlComplete');

    coFinancing.removeNAElementsProcess(dropdownElements);
    coFinancing.removeNAElementsProcess(autocompleteElements);
}

coFinancing.removeNAElementsProcess = function (elements) {
    elements.find("li:contains('N/A')").remove();
}