var FPPage = new Object();

FPPage.months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
FPPage.EDIT_MODE = 'edit';
FPPage.READ_MODE = 'read';
FPPage.defaultCurrencyValue = '1.00';
FPPage.defaultCurrency = 'USD';
FPPage.clearInputValue = '0.00';
FPPage.deleteEventKeyValue = 8;
FPPage.undefined = 'undefined';
FPPage.negativeClass = 'negative';
FPPage.IDB = 'idb';
FPPage.LOCALCOUNTERPART = 'localCounterPart';
FPPage.COFINANCING = 'cofinancing';
FPPage.operationMode = $('#operationMode').val();
FPPage.preparationMode = 'preparationMode';
FPPage.executionMode = 'executionMode';
FPPage.COMPONENT = 1;
FPPage.SUBCOMPONENT = 3;
FPPage.OUTPUTS = 5;
FPPage.ENABLE = 'Enable';
FPPage.DISABLE = 'Disable';
FPPage.lengthEqualsOne = 1;
FPPage.lengthEqualsThree = 3;
FPPage.lengthEqualsFive = 5;
FPPage.minusSign = "-";
FPPage.plusSign = "+";
FPPage.types = [FPPage.IDB, FPPage.LOCALCOUNTERPART, FPPage.COFINANCING];
FPPage.twoValue = 2;
FPPage.zeroInputValue = 0.00;
FPPage.zero = 0;
FPPage.oneHundred = 100;
FPPage.FINANCIAL_PLAN = "Financial Plan";
FPPage.classesToBeExcludedFromReport = ".tree, .financialMonth-edit, .financialTdClearExecutionMode, .notJustifiedExpensesEdit, " +
                                       ".td_execute_editMode, .disbursement-edit, .tableNameTitleExcluded, .disbursementExtraTitle," +
                                        " .excludeThis, .remainingYearColumnHeader, .remainingYearValues, .remainingYearsTotalColumn";

FPPage.planningPeriodChange = function (monthsTotal) {
    $("[name=SelectPlanningPeriod]").bind("change", function () {
        var selectFilter = $.findActiveTab(".filter-search");
        var table = $.findActiveTab("#tableFinancialPrincipal");
        var selectMonths = table.find(".financialMonth");
        var selectTotals = table.find(".financial-totals");
        var tdExecutedEditMode = table.find('.td_execute_editMode');
        var yearCurrentFilter = $.findActiveTab("#yearsCurrent");
        var yearNextFilter = $.findActiveTab("#yearsNext");

        if ($(this).prop("checked")) {
            selectFilter.css("visibility", "visible");
            $.findActiveTab('.filterButton').removeClass("hide");
            if (parseInt(monthsTotal) > 0) {
                periodFilter.showFilteredTable();
                FPPage.showDisbursementTables();
                selectMonths.css("display", "");
                tdExecutedEditMode.removeClass("showDataEdit");
            }
            FPPage.updateTopHorizontalScrollBarWidth();
            FPPage.heightTopScrollBar();
        } else {
            selectMonths.css("display", "none");
            selectMonths.removeClass("showDataEdit");
            selectTotals.css("display", "table-cell");
            selectFilter.css("visibility", "hidden");
            yearCurrentFilter.css("display", "");
            yearNextFilter.css("display", "");
            $.findActiveTab('.filterButton').addClass("hide");
            FPPage.removeTopHorizontalScrollBarWidth();
            FPPage.heightTopScrollBar();
        }

        if (FPPage.operationMode === FPPage.preparationMode) {
            FPPage.hideComponentEditPreparation();
        }

    });
}

function edit() {
    var tabEdit = $(".tab-content").find(".active").attr("id");
    enterEditMode(false, $("#"+tabEdit), true);
    FPPage.editModeFunctions();
}

FPPage.editModeFunctions = function () {
    FPPage.sectionCurrencyMock();
    FPPage.disbursmentCalculation(FPPage.EDIT_MODE);
    FPPage.updateProjectionAmountTotalsWhenEnteringEditMode();
    FPPage.calculateDisbursementSubstractions();
    FPPage.clearComponents();
    FPPage.componentClickChange();
    FPPage.projectionAmountChange();
    FPPage.addUsdValueAttribute();
    FPPage.updateUsdValueForInputsWhenChanging();
    FPPage.monthTotalUpdateWhenChange();
    $.findActiveTab('.SearchFinancialContractDropdown button').attr('disabled', 'disabled');
    if (FPPage.operationMode === FPPage.preparationMode) {
        FPPage.hideComponentEditPreparation();
    }
    FPPage.clearAll();
    bindHandlers();
    FPPage.hidelabelInfomation(true);
    FPPage.hideMonthsWhenEnteringEditMode();
    if ($.findActiveTab("[name=SelectPlanningPeriod]").prop("checked")) {
        FPPage.showMonthInputs();
    }
    FPPage.initializeDisableInputs();
    FPPage.updateRemainingBalanceWhenExecutedAmount();
    FPPage.updateRestOfYearTotalAfterValueChanges();
    FPPage.updateTopHorizontalScrollBarWidth();
    FPPage.adjustLastUpdatedField();
}

function cancel() {
    if (FPPage.operationMode === FPPage.preparationMode) {
        var tabs = $("ul.tabs");
        var activeTab = tabs.find('li.active').attr('dd-tab');
        $("[dd-tab=" + activeTab + "]").click();
        return;
    }
    $.findActiveTab(".SearchFinancialContractDropdown li a[dd-selected]").parent()[0].click();
}

function save(source) {
    FPage.saveData(FPage.loadTypeTAB);
}

FPPage.sectionCurrencyMock = function () {
    var section = $.findActiveTab("#js_sectionCurrency");
    var articles = section.find('article');

    if (articles.hasClass("col-md-6")) {
        articles.toggleClass("col-md-4");
    }
}

FPPage.loadOtherContract = function (contractNumber, operationNumber, financialInformationReadUrl) {
    FPPage.changeFinancialPlanMode(contractNumber, operationNumber, financialInformationReadUrl);
    FPPage.componentClickChange();
}


FPPage.changeModeWhenClicking = function (element, currentContractNumber, operationNumber, url) {
    element.click(function () {
        FPPage.changeFinancialPlanMode(currentContractNumber, operationNumber, url);
    });
}

FPPage.changeFinancialPlanMode = function (currentContractNumber, operationNumber, modeURL) {
    showLoader();
    var selectFilter = $.findActiveTab(".filter-search");
    var table = $.findActiveTab("#tableFinancialPrincipal");
    var selectMonths = table.find(".financialMonth");
    var selectTotals = table.find(".financial-totals");
    var yearCurrentFilter = $.findActiveTab("#yearsCurrent");
    var yearNextFilter = $.findActiveTab("#yearsNext");

    window.setTimeout(function () {
        postUrlWithOptions(modeURL, { async: false }, { 'ContractNumber': currentContractNumber, 'OperationNumber': operationNumber })
            .done(function (data) {
                var resultInformationPanel = $('.financial-information');
                resultInformationPanel.html(data);
                FPPage.deleteFirstDropdown();
                FPPage.bindEventDropDown(operationNumber, modeURL);
                FPPage.loadTotals();
                hideLoader();
                exitEditMode(false, $('#contractlevel'), false, false);
                FPPage.showDisbursementTables();
                FPPage.setDefaultCurrencyValue();
                FPPage.loadToolTipElementsLiterals();
                FPPage.addTooltip();
                FPPage.collapseIDBDisbursementsSmallTable();
                FPPage.singleExpandButtonClick();
                FPPage.changeDesignWhenChangeMode();
                filterSearchModule.filterSearchValidations();
                FPPage.submitReviewButton();
                FPPage.convertFieldToNumberAndDots();
                bindHandlers();
            });
    }, 5);
    FPPage.sectionCurrencyMock();

    selectMonths.css("display", "none");
    selectTotals.css("display", "table-cell");
    selectFilter.css("visibility", "hidden");
    yearCurrentFilter.css("display", "");
    yearNextFilter.css("display", "");
    $.findActiveTab('.filterButton').addClass("hide");

    hideLoader();
}

FPPage.bindEventDropDown = function (operationNumber, financialInformationReadUrl) {
    $.findActiveTab(".SearchFinancialContractDropdown li").bind("click", function () {
        FPPage.loadOtherContract($(this).text(), operationNumber, financialInformationReadUrl);
    });
}

FPPage.deleteFirstDropdown = function () {
    $.findActiveTab(".dropdown-menu").find("li:first").css("display", "none");
}

FPPage.calculateTotalsForEOPEAC = function () {
    var ulTotalCostEOP = $.findActiveTab('tr.componentRow ul.totalCostEOP');
    var ulTotalCostEAC = $.findActiveTab('tr.componentRow ul.totalCostEAC');
    var ulTotalsCostEOP = $.findActiveTab('ul.totalCostEOPTotals');
    var ulTotalsCostEAC = $.findActiveTab('ul.totalCostEACTotals');
    calculations.calculateSum(ulTotalCostEOP.find('li.idb'), ulTotalsCostEOP.find('li.idbTotalEOP'));
    calculations.calculateSum(ulTotalCostEOP.find('li.localCounterPart'), ulTotalsCostEOP.find('li.localCounterPartTotalEOP'));
    calculations.calculateSum(ulTotalCostEOP.find('li.cofinancing'), ulTotalsCostEOP.find('li.cofinancingTotalEOP'));
    calculations.calculateSum(ulTotalCostEAC.find('li.idb'), ulTotalsCostEAC.find('li.idbTotalEAC'));
    calculations.calculateSum(ulTotalCostEAC.find('li.localCounterPart'), ulTotalsCostEAC.find('li.localCounterPartTotalEAC'));
    calculations.calculateSum(ulTotalCostEAC.find('li.cofinancing'), ulTotalsCostEAC.find('li.cofinancingTotalEAC'));
}

FPPage.calculateTotalsForTotalAmount = function () {
    var ulTotalAmount = $.findActiveTab('tr.componentRow ul.totalAmount');
    var ulTotalAmountTotals = $.findActiveTab('ul.totalAmountTotals');
    calculations.calculateSum(ulTotalAmount.find('li.idb'), ulTotalAmountTotals.find('li.idb'));
    calculations.calculateSum(ulTotalAmount.find('li.localCounterPart'), ulTotalAmountTotals.find('li.localCounterPart'));
    calculations.calculateSum(ulTotalAmount.find('li.cofinancing'), ulTotalAmountTotals.find('li.cofinancing'));
}

FPPage.calculateTotalsForMonthsVertically = function () {
    var isEditMode = FPPage.isEditMode();
    var mode = isEditMode ? FPPage.EDIT_MODE : FPPage.READ_MODE;

    $.each(FPPage.months, function () {
        var idbElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.' + this), FPPage.IDB);
        var localCounterPartElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.' + this), FPPage.LOCALCOUNTERPART);
        var cofinancingElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.' + this), FPPage.COFINANCING);
        calculations.calculateSum(idbElementsToBeSum, $.findActiveTab('ul.total-' + this + ' li.idb'), mode);
        calculations.calculateSum(localCounterPartElementsToBeSum, $.findActiveTab('ul.total-' + this + ' li.localCounterPart'), mode);
        calculations.calculateSum(cofinancingElementsToBeSum, $.findActiveTab('ul.total-' + this + ' li.cofinancing'), mode);
        var monthlyExpensesRows = $.findActiveTab('tr.monthlyExpenses');
        var currentMonthlyExpense = monthlyExpensesRows.find('ul.' + this).find('li');
        var currentMonthTotal = $.findActiveTab('ul.total-' + this).find('li.idb').first();
        currentMonthlyExpense.text(currentMonthTotal.text());
        calculations.calculateSum(monthlyExpensesRows.find('ul').not('.monthlyExpensesTotal').find('li'),
                     monthlyExpensesRows.find('ul.monthlyExpensesTotal').find('li'));
    });
}

FPPage.calculateTotalForMonthsHorizontally = function () {
    var isEditMode = FPPage.isEditMode();
    var mode = isEditMode ? FPPage.EDIT_MODE : FPPage.READ_MODE;

    $.each($.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom'), function () {
        var currentRow = $(this);
        var columnsToSumTotal = currentRow.find('td.financialMonth').not('.financialTdValues');
        var columnsToSumTotalUl = columnsToSumTotal.find('ul');
        var idbElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, columnsToSumTotalUl, 'idb');
        var localCounterPartElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, columnsToSumTotalUl, 'localCounterPart');
        var cofinancingElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, columnsToSumTotalUl, 'cofinancing');
        var currentRowTotalBySourceUl = currentRow.find('ul.totalBySource');
        
        calculations.calculateSum(idbElementsToBeSum, currentRowTotalBySourceUl.find('li.idb'), mode);
        calculations.calculateSum(localCounterPartElementsToBeSum, currentRowTotalBySourceUl.find('li.localCounterPart'), mode);
        calculations.calculateSum(cofinancingElementsToBeSum, currentRowTotalBySourceUl.find('li.cofinancing'), mode);
    });
}

FPPage.loadMonthElementsToBeSum = function (isEditMode, listOfElements, typeSelector) {
    var elementsToBeSum = isEditMode ? listOfElements.find('li.' + typeSelector).find('input') :
                                                listOfElements.find('li.' + typeSelector);
    return elementsToBeSum;
}

FPPage.validateIfFilterIsLoaded = function (filterUrl, contractNumber, operationNumber) {
    $('.filterButton').click(function () {
        periodFilter.periodFilterCall(filterUrl, contractNumber, operationNumber);
    });
}

FPPage.calculateTotalBySourceTotal = function () {
    var ulTotalBySource = $.findActiveTab('tr.componentRow ul.totalBySource');
    var ulTotalBySourceTotal = $.findActiveTab('ul.totalBySourceTotal');
    calculations.calculateSumElementsAndRefreshTotal(ulTotalBySource, ulTotalBySourceTotal);
}

FPPage.calculateRemainingYears = function () {
    var ulRemainingYears = $.findActiveTab('tr.componentRow ul.remainingYears');
    var ulRemainingYearsTotal = $.findActiveTab('ul.remainingYearsTotal');
    calculations.calculateSumElementsAndRefreshTotal(ulRemainingYears, ulRemainingYearsTotal);
}

FPPage.calculateRemainingBalanceTotal = function () {
    var ulRemainingBalance = $.findActiveTab('tr.componentRow ul.remainingBalance');
    var ulRemainingBalanceTotal = $.findActiveTab('ul.remainingBalanceTotal');
    calculations.calculateSumElementsAndRefreshTotal(ulRemainingBalance, ulRemainingBalanceTotal);
}

FPPage.calculateRemainingToProjectBottomTotal = function () {
    var ulRemainingToProject = $.findActiveTab('tr.componentRow ul.remainingToProject');
    var ulRemainingToProjectTotal = $.findActiveTab('ul.remainingToProjectTotal');
    calculations.calculateSumElementsAndRefreshTotal(ulRemainingToProject, ulRemainingToProjectTotal);
}

FPPage.calculateCurrenYearTotal = function () {
    var ulRemainingToProject = $.findActiveTab('tr.componentRow ul.year1');
    var ulRemainingToProjectTotal = $.findActiveTab('ul.year1Total');
    calculations.calculateSumElementsAndRefreshTotal(ulRemainingToProject, ulRemainingToProjectTotal);
}

FPPage.calculateNextYearTotal = function () {
    var ulRemainingToProject = $.findActiveTab('tr.componentRow ul.year2');
    var ulRemainingToProjectTotal = $.findActiveTab('ul.year2Total');
    calculations.calculateSumElementsAndRefreshTotal(ulRemainingToProject, ulRemainingToProjectTotal);
}

FPPage.calculateRemainingToProject = function () {
    $.each($.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom'), function () {
        var columnsToSumTotalRemainingBalance = $(this).find('td.financialMonth.remainingBalance');
        var columnsToSumTotalBySource = $(this).find('td.financialMonth.totalBySource');
        var columnsToSumTotalRemainingBalanceUl = columnsToSumTotalRemainingBalance.find('ul');
        var columnsToSumTotalBySourceUl = columnsToSumTotalBySource.find('ul');

        calculations.calculateTwoColumnsSubstraction(columnsToSumTotalRemainingBalanceUl.find('li.idb'),
                                columnsToSumTotalBySourceUl.find('li.idb'),
                                $(this).find('ul.remainingToProject li.idb'));

        calculations.calculateTwoColumnsSubstraction(columnsToSumTotalRemainingBalanceUl.find('li.localCounterPart'),
                                columnsToSumTotalBySourceUl.find('li.localCounterPart'),
                                $(this).find('ul.remainingToProject li.localCounterPart'));

        calculations.calculateTwoColumnsSubstraction(columnsToSumTotalRemainingBalanceUl.find('li.cofinancing'),
                                 columnsToSumTotalBySourceUl.find('li.cofinancing'),
                                  $(this).find('ul.remainingToProject li.cofinancing'));
    });

    FPPage.calculateRemainingToProjectBottomTotal();
}

FPPage.currentYearChange = function (operationNumber, getTotalAmountYearUrl) {
    $('[name="YearCurrent"]').on("change", function () {
        showLoader();
        window.setTimeout(function () {
            var contractArray = $.findActiveTab(".SearchFinancialContractDropdown").find('a').not(':first').map(function () {
                return $(this).attr('dd-value');
            }).get().join(",");

            var curretTabActive = $('ul.tabs .active').attr('dd-tab').substring(1);
            var contractNumber = curretTabActive === 'operationlevel' ? contractArray : $('[name="SearchFinancialContract"]').val();
            var yearSelected = $.findActiveTab('[name="YearCurrent"]').val();
            var modelFinancialPlan = $.findActiveTab('#financialPlanModel').val();

            var response = postUrlWithOptions(getTotalAmountYearUrl, { async: false },
                {
                    'contractNumber': contractNumber, 'operationNumber': operationNumber,
                    'year': yearSelected, 'modelFinancial': modelFinancialPlan, 'isCurrentYear': true,
                    'curretTab': curretTabActive
                });

            var data = $.parseJSON(response.responseJSON);

            hideLoader();

            if (data.IsValid) {
                $.each(data.financialTaskTotalYearViewModel, function (i, item) {
                    var currentRow = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom')[i];
                    var currentDataRow = data.financialTaskTotalYearViewModel[i];
                    var year1Column = $(currentRow).find('ul.year1');
                    year1Column.find('li.idb').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[0].year1));
                    year1Column.find('li.localCounterPart').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[1].year1));
                    year1Column.find('li.cofinancing').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[2].year1));
                });

                FPPage.loadTotals();
                FPPage.insertUsdValueForYearElements('year1');
                FPPage.applyCurrencyChangeForYearColumn('year1');
            } else {
                showMessage(data.ErrorMessage);
            }
        }, 5);

    });
}

FPPage.nextYearChange = function (operationNumber, getTotalAmountYearUrl) {
    $('[name="YearNext"]').on("change", function () {
        showLoader();
        window.setTimeout(function () {
            var contractArray = $.findActiveTab(".SearchFinancialContractDropdown").find('a').not(':first').map(function () {
                return $(this).attr('dd-value');
            }).get().join(",");

            var curretTabActive = $('ul.tabs .active').attr('dd-tab').substring(1);
            var contractNumber = curretTabActive === 'operationlevel' ? contractArray : $('[name="SearchFinancialContract"]').val();
            var yearSelected = $.findActiveTab('[name="YearNext"]').val();
            var modelFinancialPlan = $.findActiveTab('#financialPlanModel').val();


            var response = postUrlWithOptions(getTotalAmountYearUrl, { async: false },
                {
                    'contractNumber': contractNumber, 'operationNumber': operationNumber,
                    'year': yearSelected, 'modelFinancial': modelFinancialPlan, 'isCurrentYear': false,
                    'curretTab': curretTabActive
                });

            var data = $.parseJSON(response.responseJSON);

            hideLoader();

            if (data.IsValid) {
                $.each(data.financialTaskTotalYearViewModel, function (i, item) {
                    var currentRow = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom')[i];
                    var currentDataRow = data.financialTaskTotalYearViewModel[i];
                    var year2Column = $(currentRow).find('ul.year2');
                    year2Column.find('li.idb').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[0].year2));
                    year2Column.find('li.localCounterPart').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[1].year2));
                    year2Column.find('li.cofinancing').find('label').text(calculations.convertDecimalToFixedString(data.financialTaskTotalYearViewModel[i].SourceTypes[2].year2));
                });

                FPPage.loadTotals();
                FPPage.insertUsdValueForYearElements('year2');
                FPPage.applyCurrencyChangeForYearColumn('year2');

            } else {
                showMessage(data.ErrorMessage);
            }
        }, 5)
    });
}

FPPage.disbursmentCalculation = function (mode) {
    var result = 0;
    var disbursmentTable = $.findActiveTab('#tableDisbursement');
    mode = FPPage.checkModeToBeUsed(mode);
    var accumulatedAdvances = parseFloat(disbursmentTable.find('td.accumulatedAdvances').text().replace(/,/g, ""));
     
    var notJustifiedExpenses = FPPage.isCurrentModeRead(mode) ? parseFloat(disbursmentTable.find('td.notJustifiedExpenses').find('p[data-pagemode=' + mode + ']').text().replace(/,/g, "")) :
                                               parseFloat(disbursmentTable.find('td.notJustifiedExpenses').find('p[data-pagemode=' + mode + ']').find('input').val().replace(/,/g, ""));
    var pendingJustification = disbursmentTable.find('td.pendingJustification');
    var idbCashFlowTotal = $.findActiveTab('ul.idbCashFlowTotal').find('li');

    result = calculations.convertDecimalToFixedString(accumulatedAdvances - notJustifiedExpenses);
    var resultWithStyle = "<strong>" + result + "</strong>";
    pendingJustification.find('p').html(resultWithStyle);
    idbCashFlowTotal.html(resultWithStyle);

    if (FPPage.isCurrentModeEdit(mode)) {
        periodFilter.disbursementTableCalculations();
        FPPage.updateProjectionAmountTotalsWhenEnteringEditMode();
    }

    FPPage.addUsdValueAttribute();
}

FPPage.calculateIdbCashFlowWithoutDisbursmentMonthlyTotals = function () {
    FPPage.calculateFirstElementOfDisbursement();
    FPPage.calculateRestOfElementsOfDisbursment();
    $.findActiveTab('ul.idbCashFlow-Total').find('li').text($.findActiveTab('ul.idbCashMonth').not('.totals').last().find('li').text());
}

FPPage.calculateFirstElementOfDisbursement = function () {
    var substractee = $.findActiveTab('ul.idbCashFlowTotal:first').find('li');
    var currentMonthlyExpense = $.findActiveTab('tr.monthlyExpenses').find('ul').first().find('li');
    var substraction = calculations.substractElements(currentMonthlyExpense, substractee);
    var totalForIdbNetCashFlow = substraction + calculations.transformToDecimal($.findActiveTab('ul.idbCashMonth.totals').first().find('li'));
    var elementToStoreSubstraction = $.findActiveTab('ul.idbCashMonth').first().find('li');
    var elementToStoreTotalForIdbNetCashFlow = $.findActiveTab('ul.idbNetCashFlow').first().find('li');
    elementToStoreSubstraction.text(calculations.numberWithCommas(substraction.toFixed(2).toString()));
    elementToStoreTotalForIdbNetCashFlow.text(calculations.numberWithCommas(totalForIdbNetCashFlow.toFixed(2).toString()));
}

FPPage.calculateRestOfElementsOfDisbursment = function () {
    var cashMonthColumns = $.findActiveTab('ul.idbCashMonth').not('.totals');
    cashMonthColumns.each(function (index) {
        var nextIndex = ($(this).parent().index() + 1);
        var monthTotalIndex = (index + 1);
        var elementToBeSubstracted = $(this).find('li');
        var rowIdbNetCashFlowWithDisbursements = $.findActiveTab('tr.idbNetCashFlowWithDisbursements');
        var idbCashMonthTotalsUl = $.findActiveTab('ul.idbCashMonth.totals');
        var currentMonthlyExpense = $.findActiveTab('tr.monthlyExpenses').find('td').eq(nextIndex).find('ul').find('li');
        var substraction = calculations.substractElements(currentMonthlyExpense, elementToBeSubstracted);

        var elementToBeSubstractedInIdbNetCashFlow = rowIdbNetCashFlowWithDisbursements.find('.idbNetCashFlow').eq(index).find('li');
        var substractionForIdbNetCashFlow = calculations.substractElements(currentMonthlyExpense, elementToBeSubstractedInIdbNetCashFlow);
        var elementToBeSumForIdbNet = idbCashMonthTotalsUl.eq(monthTotalIndex).find('li');
        var totalForIdbNetCashFlow = substractionForIdbNetCashFlow + calculations.transformToDecimal(elementToBeSumForIdbNet);

        var elementToStoreSubstraction = $(this).parent('td').next().find('ul.idbCashMonth').not('.totals').first().find('li');
        elementToStoreSubstraction.html(calculations.numberWithCommas(substraction.toFixed(2).toString()));

        var elementToStoreIdbNetCashFlowCalculation = rowIdbNetCashFlowWithDisbursements.find('td').eq(nextIndex).find('ul.idbNetCashFlow').find('li');
        elementToStoreIdbNetCashFlowCalculation.html(calculations.numberWithCommas(totalForIdbNetCashFlow.toFixed(2).toString()));
    });

    $.findActiveTab('ul.idbNetCashFlowTotal li').text($.findActiveTab('ul.idbNetCashFlow:last li').text());
    FPPage.markNegativeResultsAsRedForIdbNetCashFlowWithDisbursement();
}

FPPage.showDisbursementTables = function () {
    $.findActiveTab("#tableDisbursementExtra").removeClass("hide");
}

FPPage.hideDisbursementTables = function () {
    $.findActiveTab("#tableDisbursementExtra").addClass("hide");
}

FPPage.loadTotals = function (totalMonths) {
    FPPage.calculateCurrenYearTotal();
    FPPage.calculateNextYearTotal();
    FPPage.calculateTotalsForTotalAmount();
    FPPage.calculateRemainingYears();

    var operationNumber = $('input#operationNumber').val();
    var financialInformationReadUrl = $('input#financialPlanReadUrl').val();
    var getTotalAmountYearUrl = $('input#financialPlanGetTotalAmountYear').val();
    var filterUrl = $('input#financialPlanFilterUrl').val();
    var currentContractNumber = $(".SearchFinancialContractDropdown").GetValue();

    var monthsTotal = $.findActiveTab('input#financialPlanMonthTotal').val();

    $.findActiveTab(".dropdown-menu").find("li:first").css("display", "none");
    FPPage.planningPeriodChange(monthsTotal);
    FPPage.bindEventDropDown(operationNumber, financialInformationReadUrl);
    FPPage.currentYearChange(operationNumber, getTotalAmountYearUrl);
    FPPage.nextYearChange(operationNumber, getTotalAmountYearUrl);
    FPPage.disbursmentCalculation();
    FPPage.validateIfFilterIsLoaded(filterUrl, currentContractNumber, operationNumber);
    if (parseInt(totalMonths) > 0) {
        FPPage.disbursmentCalculation();
        periodFilter.calculateTotals();
    }
    else {
        FPPage.hideDisbursementTables()
    }
}

FPPage.currentSelectedCurrency = function (currencyValue, currency, previouslySelectedValue, elementsToBeUpdated) {
    currencyValue = currencyValue.toFixed(2);
    var isCurrencyDollarSelected = (currency === FPPage.defaultCurrency);
    var previousCurrency = (typeof previouslySelectedValue !== FPPage.undefined) ? previouslySelectedValue : 1;
    previousCurrency = parseFloat(previousCurrency).toFixed(2);

    $.each(elementsToBeUpdated, function () {
        var currentElement = $(this);
        var isElementAnInput = currentElement.is('input');
        var currentValueToBeUpdated = FPPage.getElementValue(currentElement);
        var valueToUpdateElement = 0;

        if (currentValueToBeUpdated !== 0 && isCurrencyDollarSelected) {
            if (isElementAnInput) {
                var valueToUpdateElement = (currentValueToBeUpdated / previousCurrency);
                var valueToUpdateElementAsFormattedString = calculations.convertDecimalToFixedString(valueToUpdateElement);
                currentElement.val(valueToUpdateElementAsFormattedString);
            } else {
                var valueInDollars = calculations.convertDecimalToFixedString(parseFloat(currentElement.attr("data-usdValue")));
                currentElement.text(valueInDollars);
            }
        }
        else if (currentValueToBeUpdated !== 0 && !isCurrencyDollarSelected) {
            var valueToUpdateElement = (currencyValue * (currentValueToBeUpdated / previousCurrency));
            var valueToUpdateElementAsFormattedString = calculations.convertDecimalToFixedString(valueToUpdateElement);
            isElementAnInput ? currentElement.val(valueToUpdateElementAsFormattedString) : currentElement.text(valueToUpdateElementAsFormattedString);
        }
    });
}

FPPage.getElementValue = function (element) {
    var isElementAnInput = element.is('input');
    var elementValue = isElementAnInput ? (element.val() !== "" ? element.val() : FPPage.clearInputValue) : element.text();
    var elementFinalValue = parseFloat(calculations.removeCommas(elementValue));
    return elementFinalValue;
}

FPPage.currencySelection = function () {
    $('.widthDropdownCurrency').change(function () {
        window.setTimeout(function () {
            var currencyElement = $.findActiveTab('.widthDropdownCurrency');
            var currencyLabel = $.findActiveTab('.labelExchangeRateUSD');
            var currencyValue = parseFloat(currencyElement.GetValue());
            var currency = currencyElement.GetText();

            currencyLabel.text(currencyValue.toFixed(2).toString())
            FPPage.currentSelectedCurrency(currencyValue, currency, currencyElement.attr("previouslySelectedCurrencyValue"), $.findActiveTab('[data-usdValue]'));
            currencyElement.attr("previouslySelectedCurrencyValue", currencyValue);

        }, 3);
    });
}

FPPage.calculateProjectedLiquidityNeeds = function () {
    var result = 0;
    var monthlyExpensesTotal = parseFloat($.findActiveTab('ul.monthlyExpensesTotal li').text().replace(/,/g, ""));
    var pendingJustifications = parseFloat($.findActiveTab('td.pendingJustification p').text().replace(/,/g, ""));
    result = calculations.numberWithCommas(monthlyExpensesTotal - pendingJustifications);
    $.findActiveTab('ul.projectedLiquidityNeedsTotal li').text(result);
}

FPPage.disbursementTypeColumnsCalculation = function (mode) {
    mode = FPPage.checkModeToBeUsed(mode);
    var listToSum;

    $.each(FPPage.months, function () {
        listToSum = (FPPage.isCurrentModeRead(mode)) ? $.findActiveTab('ul.disbursementType-' + this + ' li') : $.findActiveTab('ul.disbursementType-' + this + ' li input');
        calculations.calculateSum(listToSum,
                                  $.findActiveTab('ul.disbursementProjections-' + this + ' li'), mode);
    });

    calculations.calculateSum($.findActiveTab('ul.idbCashMonth.totals').find('li'), $.findActiveTab('ul.disbursementProjections-Total li'));
}

FPPage.markNegativeResultsAsRedForIdbNetCashFlowWithDisbursement = function () {
    FPPage.makeNegativeResultsToBeColoredAsRed($.findActiveTab('tr.idbNetCashFlowWithDisbursements ul li'));
}

FPPage.makeNegativeResultsToBeColoredAsRed = function (listOfElementsToBeChecked) {
    listOfElementsToBeChecked.each(function () {
        var currentNumber = parseFloat($(this).text()) || 0;
        if (currentNumber < 0) {
            $(this).addClass(FPPage.negativeClass);
        } else {
            $(this).removeClass(FPPage.negativeClass);
        }
    });
}

FPPage.calculateHorizontalTotalForDisbursementProjections = function (mode) {
    mode = FPPage.checkModeToBeUsed(mode);
    $.findActiveTab('tr.totalDisbursementProjections').each(function () {
        var currentRow = $(this);
        var listToBeSum;

        if (FPPage.isCurrentModeEdit(mode)) {
            listToBeSum = currentRow.find('[data-pagemode=' + mode + ']').find('ul').find('li').find('input');
        } else {
            listToBeSum = currentRow.find('[data-pagemode=' + mode + ']').find('ul').find('li');
        }
        calculations.calculateSum(listToBeSum, currentRow.find('td:last').find('ul.disbursementTypeRowTotal').find('li'), mode);
    });
}

FPPage.projectionAmountChange = function () {
    var mode = FPPage.EDIT_MODE;

    $('input[name="ProjectionAmount"]').change(function () {
        var currentProjectionAmount = $(this);
        FPPage.calculateProjectionAmount(currentProjectionAmount, mode);
    });
}

FPPage.calculateProjectionAmount = function (projectionAmountElement, mode) {
    var currentRow = projectionAmountElement.parents('tr');
    var listToBeSum = currentRow.find('[data-pagemode=' + FPPage.EDIT_MODE + ']').find('ul').find('li').find('input');
    calculations.calculateSum(listToBeSum, currentRow.find('td:last').find('ul.disbursementTypeRowTotal').find('li'), mode);
    FPPage.calculateHorizontalTotalForDisbursementProjections(mode);
    FPPage.disbursementTypeColumnsCalculation(mode);
    FPPage.calculateIdbCashFlowWithoutDisbursmentMonthlyTotals();
    FPPage.addUsdValueAttribute();
}

FPPage.updateProjectionAmountTotalsWhenEnteringEditMode = function () {
    $.findActiveTab('input[name="ProjectionAmount"]').each(function () {
        FPPage.calculateProjectionAmount($(this), FPPage.EDIT_MODE);
    });
}

FPPage.calculateDisbursementSubstractions = function () {
    $('[name="IdNotJustifiedExpenses"]').change(function () {
        FPPage.disbursmentCalculation(FPPage.EDIT_MODE);
    });
}

FPPage.isModeUndefined = function (mode) {
    return (typeof mode !== FPPage.undefined);
}

FPPage.checkModeToBeUsed = function (mode) {
    return (FPPage.isModeUndefined(mode) ? mode : FPPage.READ_MODE);
}

FPPage.isCurrentModeRead = function (mode) {
    return (mode === FPPage.READ_MODE);
}

FPPage.isCurrentModeEdit = function (mode) {
    return (mode === FPPage.EDIT_MODE);
}

FPPage.collapseIDBDisbursementsSmallTable = function () {
    $.findActiveTab("#extraClickActionSend").click(function () {
        $.findActiveTab("#extraClickActionReceive").click();
        $.findActiveTab(".js_buttonShowRowExpandSubTable").html(FPPage.minusSign);
    });
}

FPPage.setDefaultCurrencyValue = function () {
    $.findActiveTab('.widthDropdownCurrency').SetValue(FPPage.defaultCurrencyValue,
                                            FPPage.defaultCurrency);
    $.findActiveTab('.labelExchangeRateUSD').text(FPPage.defaultCurrencyValue);
}

FPPage.loadInitialDataAndFunctions = function (totalMonths, fpMode) {
    FPPage.collapseIDBDisbursementsSmallTable();
    FPPage.loadTotals(totalMonths);
    FPPage.setDefaultCurrencyValue();
    FPPage.currencySelection();
    FPPage.centerClearButton();
    FPPage.changeDesignWhenChangeMode();
    FPPage.loadCurrenTab();
    $("#operationlevel").find('.SearchFinancialContractDropdown').css("display", "none");
    FPPage.checkStateOperation(fpMode);
    if (fpMode === FPPage.preparationMode) {
        $.findActiveTab('.submitReviewButton').addClass('hide');
    }

    FPPage.loadToolTipElementsLiterals();
    FPPage.buttonShowRowExpandSubTable();
    FPPage.addTooltip();
    FPPage.hideInputPastPeriod();
    FPPage.disableInputsTaskInactive();
    FPPage.collapseButtonClick();
    FPPage.excelReportGeneration();
    FPPage.disabledCmbExchangeRate();
    FPPage.stringFormatIDBDisbursements();
    FPPage.updateTopHorizontalScrollBarWidth();
    FPPage.horizontalScrollTopAndBottom();
    FPPage.singleExpandButtonClick();
    FPPage.fixMarginButtonPanel();
    FPPage.adjustLastUpdatedField();
    FPPage.adjustLastUpdateFieldReadMode();
    FPPage.submitReviewButton();
    filterSearchModule.filterSearchValidations();
    FPPage.convertFieldToNumberAndDots();
}

FPPage.clearComponents = function () {
    $('.ButtonErase').click(function () {
        var currentButton = $(this);
        Confirm.ShowInfo(FPPage.WarningMessages.clearTrashWarningMessage).done(function (pressOk) {
            if (pressOk) {
                var currentButtonParentRow = currentButton.parents('tr.componentRow');
                var currentOrderNumber = currentButtonParentRow.find('td.orderNumber').text();
                var financialTableRows = $.findActiveTab("#tableFinancialPrincipal").
                                            find('tbody').
                                            find('tr').
                                            not('.financial-table-results-bottom');

                $.each(financialTableRows, function () {
                    var currentRow = $(this);
                    var currentRowOrderNumber = currentRow.find('td.orderNumber').text().charAt(0);
                    var isComponentChild = currentRowOrderNumber === currentOrderNumber;

                    if (isComponentChild) {
                        var currentRowInputNotBlocked = currentRow.
                                                            find('td').
                                                            not('.js_blockedMonth').
                                                            find('input');
                        currentRow.find('td.js_Percent_Executed li').text('0%');
                        currentRowInputNotBlocked.val(FPPage.clearInputValue);
                        currentRowInputNotBlocked.attr('data-usdValue', FPPage.clearInputValue);
                        currentRowInputNotBlocked.removeAttr('disabled');
                    }
                });

                periodFilter.calculateTotals();
                FPPage.addUsdValueAttribute();
            }
            $('div.modal-backdrop').remove();
        });

    });
}

FPPage.centerClearButton = function () {
    var parentRows = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr.componentRow');
    var notComponentRows = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr.notComponentRow');

    $.each(parentRows, function () {
        var currentRow = $(this);
        var currentOrderNumber = currentRow.find('td.orderNumber').text();
        var amountOfNotComponentRows = [];

        $.each(notComponentRows, function () {
            var notComponentCurrentRow = $(this);
            var notComponentRowOrderNumberFullText = notComponentCurrentRow.find('td.orderNumber').text();
            var notComponentRowOrderNumber = notComponentRowOrderNumberFullText.slice(0, notComponentRowOrderNumberFullText.indexOf("."));
            
            if (currentOrderNumber === notComponentRowOrderNumber) {
                amountOfNotComponentRows.push(notComponentCurrentRow);
                notComponentCurrentRow.find('td.financialTdClear').remove();
            }
        });

        var rowSpan = (amountOfNotComponentRows.length + 1);
        currentRow.find('td.financialTdClear').attr("rowspan", rowSpan.toString());
    });
}

FPPage.addUsdValueAttribute = function () {
    var pendingJustificationParagragh = $.findActiveTab('td.pendingJustification p');
    pendingJustificationParagragh.attr("data-usdValue", calculations.transformToDecimal(pendingJustificationParagragh));

    FPPage.insertUsdValueAttribute($.findActiveTab('.idbCashMonth'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.idbCashFlowTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.monthlyExpenses ul'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.idbNetCashFlow'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.idbCashFlow-Total'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.disbursementProjections-Total'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.disbursementTypeRowTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.idbNetCashFlowTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('.projectedLiquidityNeedsTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.totalAmountTotals'));
    FPPage.loadUsdValueForMonthsTotal();
    FPPage.tableFinancialPrincipalTotalsUsdValueLoad();
}

FPPage.tableFinancialPrincipalTotalsUsdValueLoad = function () {
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.year1Total'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.year2Total'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.remainingYearsTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.remainingBalanceTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.totalBySource'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.remainingToProject'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.remainingToProjectTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.restOfYearTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.totalBySourceTotal'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.totalCostEOPTotals'));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.totalCostEACTotals'));
}

FPPage.insertUsdValueAttribute = function (elementsList) {
    var elementsToAddUsdValue = $(elementsList).find('li');
    FPPage.addUsdValueToListOfElements(elementsToAddUsdValue);
}

FPPage.addUsdValueToListOfElements = function (listOfElements) {
    $.each(listOfElements, function () {
        var currentElement = $(this);
        currentElement.attr("data-usdValue", calculations.transformToDecimal(currentElement));
    });
}

FPPage.loadUsdValueForMonthsTotal = function () {
    $.each(FPPage.months, function () {
        FPPage.insertUsdValueAttribute($.findActiveTab('ul.total-' + this));
    });
}

FPPage.insertUsdValueForYearElements = function (yearSelected) {
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.' + yearSelected));
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.' + yearSelected + 'Total'));
}

FPPage.applyCurrencyChangeForYearColumn = function (yearSelected) {
    var currencyElement = $.findActiveTab('.widthDropdownCurrency');
    var currencyValue = parseFloat(currencyElement.GetValue());
    var currency = currencyElement.GetText();

    if (currency !== FPPage.defaultCurrency) {
        FPPage.currentSelectedCurrency(currencyValue, currency, FPPage.defaultCurrencyValue, $.findActiveTab('ul.' + yearSelected).find('li'));
        FPPage.currentSelectedCurrency(currencyValue, currency, FPPage.defaultCurrencyValue, $.findActiveTab('ul.' + yearSelected + 'Total').find('li'));
    }
}

FPPage.changeDesignWhenChangeMode = function () {
    var itemTable = $("#operationlevel");
    var itemTableContractLevel = $("#contractlevel");
    var col = "col-md-6";
    var margin = "margin-left";
    var marginValue = "15px";
    var display = "display";
    var changeMode = ".js_changeMode";
    var itemChangeMode = itemTable.find(changeMode);
    var approvalNumber = ".approvalNumber";
    var block = "block";
    var none = "none";
    var financialTdClear = ".financialTdClearExecutionMode";
    var financialTdClearTotals = ".financialTdClearExecutionModeTotals";
    
    if (itemTable.hasClass("executionMode")) {
        FPPage.displayElements(itemChangeMode, display, none);
        FPPage.removeClassAndAddStyle(itemTable.find("#TotalAvailableFunds"), col, margin, marginValue);
        FPPage.removeClassAndAddStyle(itemTable.find("#PlanningPeriodCuota"), col, margin, marginValue);
        FPPage.displayElements(itemTable.find(approvalNumber), display, "table-cell");
        FPPage.displayElements(itemTable.find(financialTdClear), display, none);
        FPPage.displayElements(itemTable.find(financialTdClearTotals), display, "table-cell");
   
    } else {
        FPPage.displayElements(itemChangeMode, display, block);
        FPPage.displayElements(itemTable.find(approvalNumber), display, none);
        FPPage.displayElements(itemTable.find(financialTdClear), display, "table-cell");
        FPPage.displayElements(itemTable.find(financialTdClearTotals), display, none);
        FPPage.displayElements(itemTable.find("#MinimumAmountJustified"), display, none);
        FPPage.displayElements(itemTable.find("#PercentToJustify"), display, none);
        itemTable.find("#TotalAvailableFunds").removeClass(col).addClass("col-md-12");
        itemTable.find("#js_informationMiddle").removeClass("col-md-4").addClass("col-md-2");
    }

    FPPage.displayElements(itemTableContractLevel.find(approvalNumber), display, none);
    FPPage.displayElements(itemTableContractLevel.find(financialTdClearTotals), display, none);
}

FPPage.removeClassAndAddStyle = function (element, elementClass, margin, marginValue) {
    element.removeClass(elementClass).css(margin, marginValue);
}

FPPage.displayElements = function (element, display, displayValue) {
    element.css(display, displayValue);
}

FPPage.updateUsdValueForInputsWhenChanging = function () {
    $("input[data-usdvalue]").change(function () {
        FPPage.updateUSDValue($(this));
    });
}

$.findActiveTab = function (selector) {
    var activeTab = $(".tab-content").find(".active");
    return activeTab.find(selector);
};

FPPage.monthTotalUpdateWhenChange = function () {
    $('[name="idbMonthTotal"], [name="localMonthTotal"], [name="cofinancingMonthTotal"]').change(function () {
        periodFilter.calculateTotals();
        var currentElement = $(this);
        FPPage.addUsdValueAttribute();
    });
}

FPPage.loadCurrenTab = function () {
    $('ul.tabs').find('li').click(function () {
        FPPage.activeTab = $(".tab-content").find(".active");
        FPPage.currentActiveTab = $('ul.tabs .active').attr('dd-tab').substring(1);
        FPPage.cleanCollapseAllElementsData();
        var indexUrl = $('input#financialPlanIndexUrl').val();
        var currentLevel = $(".tab-content").find(".active").attr('id');
        var operationNumber = $('input#operationNumber').val();
        var totalMonths;
        var fpMode;

        showLoader();
        window.setTimeout(function () {
            var response = postUrlWithOptions(indexUrl, { async: false },
                    {
                        'currentTabToBeLoaded': currentLevel, 'operationNumber': operationNumber
                    }).done(function (data) {
                        totalMonths = data.Months;
                        fpMode = data.fpMode;
                        $("#" + currentLevel).html(data.TabView);
                    });
            
            FPPage.loadInitialDataAndFunctions(totalMonths, fpMode);
            exitEditMode(false, $("#" + currentLevel), false, false);
            FPPage.checkStateOperation(fpMode);
            if (fpMode === FPPage.preparationMode) {
                $.findActiveTab('.submitReviewButton').addClass('hide');
            }

            hideLoader();
            bindHandlers();
        }, 5);
    });
}

FPPage.hideTabContract = function () {
    $('[dd-tab="#contractlevel"]').addClass('hide');
}

FPPage.showTabContract = function () {
    $('[dd-tab="#contractlevel"]').removeClass('hide');
}

FPPage.checkStateOperation = function (fpMode) {
    if (fpMode === FPPage.preparationMode) {
        FPPage.hideTabContract();
    } else {
        FPPage.showTabContract();
    }
}

FPPage.hideComponentEditPreparation = function () {
    $.findActiveTab('.td_execute_editMode').addClass('hide');
    $.findActiveTab('.td_execute_editMode').removeClass('showDataEdit');
    $.findActiveTab('.td_execute_viewMode').removeClass('hide');

    $.findActiveTab('.notJustifiedExpensesRead').removeClass('hide');
    $.findActiveTab('.notJustifiedExpensesEdit').removeClass('showDataEdit');
    $.findActiveTab('.notJustifiedExpensesEdit').addClass('hide');
}

FPPage.clearAll = function () {
    $('.clearAllButton').click(function () {
        $.findActiveTab('#tableFinancialPrincipal').find('td').not(".js_blockedMonth").find('input[type="text"]')
            .val(FPPage.clearInputValue).attr('data-usdValue', FPPage.clearInputValue)
            .removeAttr('disabled');
        $.findActiveTab('#tableDisbursement').find('input[type="text"]')
            .val(FPPage.clearInputValue).attr('data-usdValue', FPPage.clearInputValue);
        $.findActiveTab('#tableDisbursement').find('.pendingJustification').find("p").find('strong')
            .text(FPPage.clearInputValue).closest('p').attr('data-usdValue', FPPage.clearInputValue);
        $.findActiveTab('#tableDisbursementExtra').find('.idbCashFlowTotal').find("li").find('strong')
            .text(FPPage.clearInputValue).closest('li').attr('data-usdValue', FPPage.clearInputValue);
        $.findActiveTab('#tableDisbursementExtra').find('input[type="text"]').not(':disabled')
            .val(FPPage.clearInputValue).attr('data-usdValue', FPPage.clearInputValue);
        $('input[name="InputPlanningPeriodBudgetQuota"]')
            .val(FPPage.clearInputValue).attr('data-usdValue', FPPage.clearInputValue);
        FPPage.clearDisbursementsInputs();
        periodFilter.calculateTotals();
        FPPage.addUsdValueAttribute();
    });
}

FPPage.percentExecutedCalculation = function () {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var executedAmountInputs;
    var sumExecutedAmountValues;
    var totalAmountInputs;
    var sumTotalAmountValues;

    $.each(findActiveTabAndTable, function () {
        executedAmountInputs = $(this).find('.js_executedAmount li input');
        totalAmountInputs = $(this).find(".totalAmount li");

        sumExecutedAmountValues = FPPage.zero;
        $.each(executedAmountInputs, function () {
            sumExecutedAmountValues += parseFloat($(this).val().replace(/,/g, ""));
        });

        sumTotalAmountValues = FPPage.zero;
        $.each(totalAmountInputs, function () {
            sumTotalAmountValues += parseFloat($(this).text().replace(/,/g, ""));
        });

        var percentExecuted = Math.floor((sumExecutedAmountValues / sumTotalAmountValues) * FPPage.oneHundred);

        if ($.isNumeric(percentExecuted)) {
            $(this).find(".js_Percent_Executed ul li").html(percentExecuted + "%");
        }
    });
}

FPPage.componentClickChange = function () {
    $.findActiveTab('tr ul').not(".js_executedAmount").not(".js_restOfYear").find("input").change(function () {
        var selectedInput = $(this);

        var singleInputElement = selectedInput;
        var singleInputIdentifier = selectedInput.closest("li").attr("class");
        var singleInputVal = selectedInput.val().replace(/,/g, "");
        var componentType = singleInputElement.closest("tr").find(".orderNumber").text();
        var itemsRow = singleInputElement.closest("tr");
        var tdClassIdentifier = selectedInput.closest("ul").attr('class').split(" ")[0];
        var componentTypeToArray = componentType.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);

        FPPage.distributeComponentByType(componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier, dotValue);
    });
}

FPPage.enableAllRowsComponent = function (enableAll, itemsRow, allTdRowsToBeDisabled, tdClassIdentifier) {
    if (enableAll) {
        var allTdEditable = [];
        var collectTdEditable = $(itemsRow).find('td').not(".js_blockedMonth");
        var findExecutedAndMonthsResults = [];

        $.each(collectTdEditable, function () {
            var findExecutedAndMonths = $(this).find("." + tdClassIdentifier);
            if (findExecutedAndMonths.hasClass("js_selectMonth")) {
                var collectInputs = [];
                $(this).find("li input").map(function () {
                    collectInputs.push(parseInt($(this).val().replace(/,/g, "")))
                });
                findExecutedAndMonthsResults.push(FPPage.subcomponentsAndOutputsSumOperations(collectInputs));
            }
        })

        var validateDisable = FPPage.unblockAllRows(findExecutedAndMonthsResults);

        if (validateDisable === false) {
            allTdRowsToBeDisabled.map(function (field) {
                field.prop("disabled", false);
            })
           }
        }
    
}

FPPage.mappingRows = function (componentType, itemsRow, status, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];
    var enableAll;

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);

        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);

        if (currentRowOrderNumber.charAt(0) === componentType.charAt(0) && currentRowOrderNumber.indexOf(componentType, 0) === 0 && (dotValue === 1 || dotValue === 2)) {
            mappingRowsGroup.push(currentRow);
        }
    });

    var allTdRowsToBeDisabled = []

    $.each(mappingRowsGroup, function (i, element) {
        if (status === FPPage.DISABLE) {
            $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").val((0).toFixed(2)).prop("disabled", true);
        }
        if (status === FPPage.ENABLE) {
            var itemForEnable = $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input");
            allTdRowsToBeDisabled.push(itemForEnable);
            enableAll = true;
        }
    });

    FPPage.enableAllRowsComponent(enableAll, itemsRow, allTdRowsToBeDisabled, tdClassIdentifier);
}

FPPage.siblingsVertical = function (findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier) {
    var mappingRowsGroup = [];
    var dotParentValue = FPPage.returnTypeOfComponent(componentType);

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);
        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (dotParentValue === 1) {
            if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 1) {
                mappingRowsGroup.push(currentRow);
            }
        }

        if (dotParentValue === 2) {
            if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 2) {
                mappingRowsGroup.push(currentRow);
            }
        }
    });
    
    var groupOfitemsRowValues = [];
    
    $.each(mappingRowsGroup, function (i, element) {
        element.find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").map(function () {
            groupOfitemsRowValues.push(parseInt($(this).val().replace(/,/g, "")));
        });
    })
    
    var returnValidate = FPPage.unblockAllRows(groupOfitemsRowValues);
    return returnValidate
}

FPPage.siblingsHorizontal = function (findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier) {
    var mappingRowsGroup = [];
    var dotParentValue = FPPage.returnTypeOfComponent(componentType);

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);
        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");
        
        if (dotParentValue === 1) {
            if (componentType === currentRowOrderNumber && firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 1) {
                mappingRowsGroup.push(currentRow);
            }
        }

        if (dotParentValue === 2) {
            if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && firstCurrentOrderNumber[1] === firstComponentType[1] && dotValue === 2) {
                mappingRowsGroup.push(currentRow);
            }
        }
        
    });
    
    var groupOfitemsRowValues = [];
    
    $.each(mappingRowsGroup, function (i, element) {
        element.find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").map(function () {
            groupOfitemsRowValues.push(parseInt($(this).val().replace(/,/g, "")));
        });
    })
    
    var returnValidate = FPPage.unblockAllRows(groupOfitemsRowValues);
    return returnValidate
}

FPPage.validateAllSiblingRows = function (findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier, enableAll, itemsRow, allTdRowsToBeDisabled) {

    var mappingRowsGroup = [];

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);

        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();

        if (componentType === currentRowOrderNumber || currentRowOrderNumber.charAt(0) === componentType.charAt(0) && currentRowOrderNumber.trim().length === componentType.trim().length) {
            mappingRowsGroup.push(currentRow);
        }
    });

    var addIdentifier = 'li input';
    var groupOfitemsRowValues = [];

    $.each(mappingRowsGroup, function (i, element) {
        element.find(addIdentifier).map(function () {
            groupOfitemsRowValues.push(parseInt($(this).val().replace(/,/g, "")));
        });
    })

    var returnValidate = FPPage.unblockAllRows(groupOfitemsRowValues);

    return returnValidate
}



FPPage.mappingRowsSub = function (componentType, itemsRow, status, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];
    var enableAll;

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);
        
        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 0) {
            mappingRowsGroup.push(currentRow);
        }
        if (firstCurrentOrderNumber[0] === firstComponentType[0] && currentRowOrderNumber.indexOf(componentType, 0) === 0 && dotValue === 2) {
            mappingRowsGroup.push(currentRow);
        }
    });

    var allTdRowsToBeDisabled = []

    $.each(mappingRowsGroup, function (i, element) {
        var currentRowSub = $(this);
        var currentRowOrderNumberSub = currentRowSub.find('td.orderNumber').text();
        var componentTypeToArraySub = currentRowOrderNumberSub.split("");
        var dotValueSub = FPPage.returnTypeOfComponent(componentTypeToArraySub);

        if (status === FPPage.DISABLE) {
            if (dotValueSub === 0) {
                $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", true);
            } else {
                $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").val((0).toFixed(2)).prop("disabled", true);
            }
            
        }
        if (status === FPPage.ENABLE) {
            var itemForEnable = $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input");
            allTdRowsToBeDisabled.push(itemForEnable);
            enableAll = true;
        }
    });
    var parentComponentRow = mappingRowsGroup[0]; 
    var childrenSubcomponentRows = mappingRowsGroup.slice(1);
    var validateSiblingsVertical = FPPage.siblingsVertical(findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier);
    if (validateSiblingsVertical === false) {
        $(parentComponentRow).find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", false);
    }
    var validateSiblingsHorizontal = FPPage.siblingsHorizontal(findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier);
    if (validateSiblingsHorizontal === false) {
        $.each(childrenSubcomponentRows, function (index, element) {
            $(element).find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", false);
        })
    }
    
}

FPPage.mappingRowsOut = function (componentType, itemsRow, status, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];
    var enableAll;

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);
        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (firstCurrentOrderNumber[0] === firstComponentType[0] && componentType.indexOf(currentRowOrderNumber, 0) === 0 && (dotValue === 0 || dotValue === 1)) {
            mappingRowsGroup.push(currentRow);

        }
    });

    var allTdRowsToBeDisabled = [];

    $.each(mappingRowsGroup, function (i, element) {
        if (status === FPPage.DISABLE) {
            $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", true);
        }
        if (status === FPPage.ENABLE) { 
            var itemForEnable = $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input");
            allTdRowsToBeDisabled.push(itemForEnable);
            enableAll = true;
        }
    });

    var parentComponentRow = mappingRowsGroup[0]; 
    var childrenSubcomponentRows = mappingRowsGroup.slice(1);
    var validateSiblingsVertical = FPPage.siblingsVertical(findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier);
    if (validateSiblingsVertical === false) {
        $(parentComponentRow).find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", false);
    }
    var validateSiblingsHorizontal = FPPage.siblingsHorizontal(findActiveTabAndTable, orderNumber, componentType, tdClassIdentifier);
    if (validateSiblingsHorizontal === false) {
        $.each(childrenSubcomponentRows, function (index, element) {
            $(element).find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", false);
        })
    }

}

FPPage.subcomponentsAndOutputsSumOperations = function (elementsForSum) {
    var elementsTotal = FPPage.zero;
    for (var i = FPPage.zero; i < elementsForSum.length; i++) {
        elementsTotal += elementsForSum[i] << FPPage.zero;
    }
    return elementsTotal;
}

FPPage.unblockAllRows = function (groupOfitemsRowValues) {
    var unblockTr = groupOfitemsRowValues.some(function (element) {
        if (!isNaN(element) && element > FPPage.zeroInputValue) {
            var isTrue = true;
            return isTrue;
        } else {
            var isFalse = false;
            return isFalse;
        }
    })

    return unblockTr;
}

FPPage.validateBlockAndUnblock = function (orderNumberValGroupValues, lengthEquals, orderNumberVal, orderNumberValGroup, singleInputIdentifier, total, convertSingleInputVal, groupOfitemsRowValues, tdClassIdentifier) {
    if (orderNumberValGroupValues === orderNumberVal) {
        var addIdentifier = "." + tdClassIdentifier;
        var currentRow = $(orderNumberValGroup).closest("tr");
        currentRow.find(addIdentifier).find("." + singleInputIdentifier).find("input").val("").val(calculations.convertDecimalToFixedString(total));

        if (addIdentifier === '.js_executedAmount') {
            var totalCostEAC = parseFloat(currentRow.find("td").find("ul.totalCostEAC").find("." + singleInputIdentifier).text().trim().replace(/,/g, ""));
            var executedAmountValue = parseFloat(currentRow.find(addIdentifier).find("." + singleInputIdentifier).find("input").val().trim().replace(/,/g, ""));
            var updatedRemainingBalance = totalCostEAC - executedAmountValue;
            currentRow.find("td.remainingBalance").find("ul.remainingBalance").find("." + singleInputIdentifier).text(calculations.numberWithCommas(updatedRemainingBalance.toFixed(2).toString()));
            FPPage.calculateRemainingBalanceTotal()
            FPPage.calculateRemainingToProject()
            FPPage.calculateRemainingToProjectBottomTotal();
        }
        FPPage.updateUSDValueForInputs();

    }
}

FPPage.subcomponentsAndOutputsResults = function (containerLevel, dotParentValue, singleInputIdentifier, singleInputVal, total, componentType, tdClassIdentifier) {

    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);
        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (dotParentValue === 2) {
            if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && firstCurrentOrderNumber[1] === firstComponentType[1] && dotValue === 2) {
                mappingRowsGroup.push(currentRow);
            }
        }
        if (dotParentValue === 1) {
            if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 1) {
                mappingRowsGroup.push(currentRow);
            }
        }
    });

    var addIdentifier = '.' + tdClassIdentifier + ' li input';
    var groupOfitemsRowValues = [];

    $.each(mappingRowsGroup, function (i, element) {
        element.find(addIdentifier).map(function () {
            groupOfitemsRowValues.push(parseInt($(this).val().replace(/,/g, "")));
        });
    })

    $.each($(".orderNumber"), function () {
        var orderNumberValGroup = $(this).find("p");
        var orderNumberValGroupValues = orderNumberValGroup.text();
        var orderNumberVal = containerLevel;
        var convertSingleInputVal = parseInt(singleInputVal);

        if (dotParentValue === 2) {
            FPPage.validateBlockAndUnblock(orderNumberValGroupValues, dotParentValue, orderNumberVal, orderNumberValGroup, singleInputIdentifier, total, convertSingleInputVal, groupOfitemsRowValues, tdClassIdentifier);
        }

        if (dotParentValue === 1) {
            FPPage.validateBlockAndUnblock(orderNumberValGroupValues, dotParentValue, orderNumberVal, orderNumberValGroup, singleInputIdentifier, total, convertSingleInputVal, groupOfitemsRowValues, tdClassIdentifier);
        }
    });
}

FPPage.ouputs = function (componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var elementsForSum = [];
    var dotParentValue = FPPage.returnTypeOfComponent(componentType);
    var sliceContainerLevel = componentType.split(".");

    $.each(findActiveTabAndTable, function () {
        var current = $(this);
        var currentRowOrderNumber = current.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (componentType === currentRowOrderNumber || firstCurrentOrderNumber[0] === firstComponentType[0] && firstCurrentOrderNumber[1] === firstComponentType[1] && dotValue === 2) {
            var currentIdentifier = $(this).find("." + tdClassIdentifier).closest("td");
            if (!currentIdentifier.hasClass("js_blockedMonth")) {
                var currentIdentifierFiltered = currentIdentifier.find("." + tdClassIdentifier).find("." + singleInputIdentifier).find("input").val().replace(/,/g, "");
                if (currentIdentifierFiltered != undefined) {
                    elementsForSum.push(currentIdentifierFiltered);
                }
            }
        }
    });

    var total = FPPage.subcomponentsAndOutputsSumOperations(elementsForSum);

    var containerFirstLevel = sliceContainerLevel[0] + "." + sliceContainerLevel[1];

    FPPage.subcomponentsAndOutputsResults(containerFirstLevel, dotParentValue, singleInputIdentifier, singleInputVal, total, componentType, tdClassIdentifier);

    if (dotParentValue === 2) {
        FPPage.subcomponent(componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier);
    }
}

FPPage.subcomponent = function (componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var elementsForSumTwo = [];
    var dotParentValue = FPPage.returnTypeOfComponent(componentType);
    var sliceContainerLevel = componentType.split(".");

    $.each(findActiveTabAndTable, function () {
        var current = $(this);
        var currentRowOrderNumber = current.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 1) {
            var currentIdentifier = $(this).find("." + tdClassIdentifier).closest("td");
            if (!currentIdentifier.hasClass("js_blockedMonth")) {
                var currentIdentifierFiltered = currentIdentifier.find("." + tdClassIdentifier).find("." + singleInputIdentifier).find("input").val().replace(/,/g, "");
                if (currentIdentifierFiltered != undefined) {
                    elementsForSumTwo.push(currentIdentifierFiltered);
                }
            }
        }
    });

    var total = FPPage.subcomponentsAndOutputsSumOperations(elementsForSumTwo);

    var containerFirstLevel = sliceContainerLevel[0];

    FPPage.subcomponentsAndOutputsResults(containerFirstLevel, dotParentValue, singleInputIdentifier, singleInputVal, total, componentType, tdClassIdentifier);
}

FPPage.distributeComponentByType = function (componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier, dotValue) {
    var groupOfitemsRowValues = [];

    var addIdentifier = '.' + tdClassIdentifier + ' li input';
    itemsRow.find(addIdentifier).map(function () {
        groupOfitemsRowValues.push(parseInt($(this).val().replace(/,/g, "")));
    });

    if (dotValue === 0) {
        if (groupOfitemsRowValues[0] > FPPage.zeroInputValue || groupOfitemsRowValues[1] > FPPage.zeroInputValue || groupOfitemsRowValues[2] > FPPage.zeroInputValue) {
            FPPage.mappingRows(componentType, itemsRow, FPPage.DISABLE, tdClassIdentifier);
        } else if ((groupOfitemsRowValues[0] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[0])) && (groupOfitemsRowValues[1] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[1])) && (groupOfitemsRowValues[2] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[2]))) {
            FPPage.mappingRows(componentType, itemsRow, FPPage.ENABLE, tdClassIdentifier);
        }
        FPPage.percentExecutedCalculation();
    }
    if (dotValue === 1) {
        if (groupOfitemsRowValues[0] > FPPage.zeroInputValue || groupOfitemsRowValues[1] > FPPage.zeroInputValue || groupOfitemsRowValues[2] > FPPage.zeroInputValue) {
            FPPage.mappingRowsSub(componentType, itemsRow, FPPage.DISABLE, tdClassIdentifier);
        } else if ((groupOfitemsRowValues[0] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[0])) && (groupOfitemsRowValues[1] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[1])) && (groupOfitemsRowValues[2] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[2]))) {
            FPPage.mappingRowsSub(componentType, itemsRow, FPPage.ENABLE, tdClassIdentifier);
        }
        FPPage.percentExecutedCalculation();
    }
    if (dotValue === 2) {
        if (groupOfitemsRowValues[0] > FPPage.zeroInputValue || groupOfitemsRowValues[1] > FPPage.zeroInputValue || groupOfitemsRowValues[2] > FPPage.zeroInputValue) {
            FPPage.mappingRowsOut(componentType, itemsRow, FPPage.DISABLE, tdClassIdentifier);
        } else if ((groupOfitemsRowValues[0] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[0])) && (groupOfitemsRowValues[1] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[1])) && (groupOfitemsRowValues[2] <= FPPage.zeroInputValue || !$.isNumeric(groupOfitemsRowValues[2]))) {
            FPPage.mappingRowsOut(componentType, itemsRow, FPPage.ENABLE, tdClassIdentifier);
        }
        FPPage.percentExecutedCalculation();
    }
    if (dotValue === 2) {
        FPPage.ouputs(componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier);
        FPPage.percentExecutedCalculation();
    }
    if (dotValue === 1) {
        FPPage.subcomponent(componentType, itemsRow, singleInputVal, singleInputIdentifier, tdClassIdentifier);
        FPPage.percentExecutedCalculation();
    }
}

FPPage.mappingRowsComponentOnload = function (componentType, itemsRow, status, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];
    var enableAll;

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);

        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);

        if (currentRowOrderNumber.charAt(0) === componentType.charAt(0) && currentRowOrderNumber.indexOf(componentType, 0) === 0 && (dotValue === 1 || dotValue === 2)) {
            mappingRowsGroup.push(currentRow);
        }
    });

    var allTdRowsToBeDisabled = []

    $.each(mappingRowsGroup, function (i, element) {
        if (status === FPPage.DISABLE) {
            $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", true);
        }
    });

}

FPPage.mappingRowsSubOnload = function (componentType, itemsRow, status, tdClassIdentifier) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');
    var orderNumber = findActiveTabAndTable.first().find('td.orderNumber').text();

    var mappingRowsGroup = [];
    var enableAll;

    $.each(findActiveTabAndTable, function () {
        var currentRow = $(this);

        var currentRowOrderNumber = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = currentRowOrderNumber.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var firstCurrentOrderNumber = currentRowOrderNumber.split(".");
        var firstComponentType = componentType.split(".");

        if (firstCurrentOrderNumber[0] === firstComponentType[0] && dotValue === 0) {
            mappingRowsGroup.push(currentRow);
        }
        if (firstCurrentOrderNumber[0] === firstComponentType[0] && currentRowOrderNumber.indexOf(componentType, 0) === 0 && dotValue === 2) {
            mappingRowsGroup.push(currentRow);
        }
    });

    var allTdRowsToBeDisabled = []

    $.each(mappingRowsGroup, function (i, element) {
        if (status === FPPage.DISABLE) {
            $(element).closest("tr").find("td").not(".js_blockedMonth").find("." + tdClassIdentifier).find('li').find("input").prop("disabled", true);
        }
    });
}

FPPage.returnTypeOfComponent = function (componentTypeToArray) {
    var dotResult = 0;
    for (i = 0; i <= componentTypeToArray.length; i++) {
        var dotValue = componentTypeToArray[i];
        if (dotValue === ".") {
            var dotResult = dotResult + 1;
        }
    }
    return dotResult
}

FPPage.validateBiggerThanZero = function (componentType, itemsRow, groupOfValues, groupLabel, dotValue, typeofGroup, findActiveTabAndTable) {
    var isBiggerThanZero = groupOfValues.some(function (element) {
        if (element != "0.00") {
            return true
        }
    });

    if (isBiggerThanZero) {
        if (dotValue === 2 && typeofGroup === 2) {
            FPPage.mappingRowsOut(componentType, itemsRow, FPPage.DISABLE, groupLabel);
        }
        if (dotValue === 1 && typeofGroup === 1) {
            FPPage.mappingRowsSubOnload(componentType, itemsRow, FPPage.DISABLE, groupLabel);
        }
        if (dotValue === 0 && typeofGroup === 0) {
            FPPage.mappingRowsComponentOnload(componentType, itemsRow, FPPage.DISABLE, groupLabel);
        }
    }
}

FPPage.initializeDisableInputs = function () {
    FPPage.disableInputsOnEdit(2);
    FPPage.disableInputsOnEdit(1);
    FPPage.disableInputsOnEdit(0);
}

FPPage.disableInputsOnEdit = function (typeofGroup) {
    var findActiveTabAndTable = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').not('.financial-table-results-bottom');

    $.each(findActiveTabAndTable, function (index, row) {
        var currentRow = $(row);
        var itemsRow = currentRow.closest("tr");
        var componentType = currentRow.find('td.orderNumber').text();
        var componentTypeToArray = componentType.split("");
        var dotValue = FPPage.returnTypeOfComponent(componentTypeToArray);
        var tdGroup = $(currentRow).closest("tr").find("td.financialMonth-edit").not(".js_blockedMonth");//.find('li').find('input');
      
        $.each(tdGroup, function () {

            var groupOfInputs = $(this).find('li').find('input:not([disabled])');
            
            var groupLabel = $(this).find("ul").attr('class').split(" ")[0];

            if (groupOfInputs != undefined) {
                var groupOfValues = groupOfInputs.map(function () {
                    return $(this).val();
                }).get();

                FPPage.validateBiggerThanZero(componentType, itemsRow, groupOfValues, groupLabel, dotValue, typeofGroup, findActiveTabAndTable)
            }
            
        })

    })

}

FPPage.hidelabelInfomation = function (flag) {
    if (flag) {
        $.findActiveTab(".financial-information").removeClass('financial-information-hide');
    }
    else {
        $.findActiveTab(".financial-information").addClass('financial-information-hide');
    }
    
}

FPPage.addTooltip = function () {
    $.findActiveTab(".tooltips").hover(function () {
        $(this).find(".financial-tooltip").removeClass("hide");
    }, function () {
        $(this).find(".financial-tooltip").addClass("hide");
    });
}

FPPage.loadToolTipElementsLiterals = function () {
    var arrayOfElementsToAddToolTip = [$.findActiveTab("#CoFinancing"), $.findActiveTab("#LocalCounterpart")];
    
    $.each(arrayOfElementsToAddToolTip, function () {
        FPPage.loadToolTipMessage($(this));
    });
}

FPPage.hideInputPastPeriod = function () {
    var monthsUl = $.findActiveTab('#tableFinancialPrincipal').find('tbody tr').not('.taskInactive').find('.financialMonth-edit ul')
    var date = new Date();
    var currentYear = parseFloat(date.getFullYear());
    var currentMonth = parseFloat(date.getMonth() + 1);

    $.each(monthsUl, function () {
        var currentUl = $(this);
        var yearUl = currentUl.attr('data-year');
        var monthUl = currentUl.attr('data-month');
        if (yearUl <= currentYear && monthUl < currentMonth) {
            currentUl.find('input').attr('disabled', 'disabled');
            currentUl.find('input').closest("td").addClass('js_blockedMonth');
        }
    });

    var disbursementUL = $.findActiveTab('#tableDisbursementExtra').find('tr.totalDisbursementProjections').find('.disbursement-edit').find('ul');

    $.each(disbursementUL, function () {
        var currentUl = $(this);
        var yearUl = currentUl.attr('data-monthyear');
        var monthUl = currentUl.attr('data-monthindex');
        if (yearUl <= currentYear && monthUl < currentMonth) {
            currentUl.find('input').attr('disabled', 'disabled');
            currentUl.find('input').closest("td").addClass('js_blockedMonth');
        }
    });
}

FPPage.disableInputsTaskInactive = function () {
    var listInput = $.findActiveTab('#tableFinancialPrincipal').find('tbody tr').not('.' + FPPage.taskActive).find('td').not('.js_executedAmount').find('input');
    listInput.attr('disabled', 'disabled');
    listInput.closest("td").addClass('js_blockedMonth');
}

FPPage.collapseButtonClick = function () {
    $.findActiveTab('.btnCollapseAll').click(function () {
        var currentCollapseElement = $(this);
        window.setTimeout(function () {
            var collapseText = currentCollapseElement.text().trim();
            var isCollapse = currentCollapseElement.hasClass("collapse");
            var activeTabTables = $.findActiveTab("table");
            var currentCollapseElementLabel = currentCollapseElement.find('label');
            var expandedTables = FPPage.loadTablesAccordingToSign(FPPage.minusSign, activeTabTables);
            var collapsedTables = FPPage.loadTablesAccordingToSign(FPPage.plusSign, activeTabTables);

            if (collapseText === FPPage.expandAllText) {
                FPPage.expandOrCollapseElement(expandedTables);
                FPPage.updateCollapseElementData(currentCollapseElementLabel, FPPage.expandAllText, FPPage.collapseAllText);
            } else {
                FPPage.expandOrCollapseElement(collapsedTables);
                FPPage.updateCollapseElementData(currentCollapseElementLabel, FPPage.collapseAllText, FPPage.expandAllText);
            }

            FPPage.addOrRemoveTopHorizontalScrollBarWhenTableIsCollapsed();
        }, 5);

    });
}

FPPage.expandOrCollapseElement = function (tableElements) {
    tableElements.find(".tree.icon").find('.icon').click();
}

FPPage.updateCollapseElementData = function (element, labelText, labelDDValue) {
    element.text(labelText);
    element.attr("dd-value", labelDDValue);
}

FPPage.loadTablesAccordingToSign = function (sign, tablesToBeFiltered) {
    var tablesAccordingToSign = tablesToBeFiltered.filter(function () {
        if ($(this).find(".tree.icon").find('span').text() === sign) {
            return $(this);
        }
    });

    return tablesAccordingToSign;
}

FPPage.disabledCmbExchangeRate = function () {
    var exchangeElement = $.findActiveTab('.widthDropdownCurrency').find("button");
    var currencyCount = exchangeElement.attr('data-currencycount');

    if (currencyCount <= FPPage.lengthEqualsOne)
    {
        exchangeElement.attr('disabled', 'disabled');
    }
}

FPPage.restOfYearCalculation = function () {
    FPPage.calculateRestOfYearTotal();
}

FPPage.calculateRestOfYearTotal = function () {
    var isEditMode = FPPage.isEditMode;
    var mode = isEditMode ? FPPage.EDIT_MODE : FPPage.READ_MODE;
    var ulRestOfYearTotal = $.findActiveTab('ul.restOfYearTotal');
    var idbElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.restOfYear'), FPPage.IDB);
    var localCounterPartElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.restOfYear'), FPPage.LOCALCOUNTERPART);
    var cofinancingElementsToBeSum = FPPage.loadMonthElementsToBeSum(isEditMode, $.findActiveTab('tr.componentRow ul.restOfYear'), FPPage.COFINANCING);
    calculations.calculateSum(idbElementsToBeSum, ulRestOfYearTotal.find('li.idb'), mode);
    calculations.calculateSum(localCounterPartElementsToBeSum, ulRestOfYearTotal.find('li.localCounterPart'), mode);
    calculations.calculateSum(cofinancingElementsToBeSum, ulRestOfYearTotal.find('li.cofinancing'), mode);
    FPPage.insertUsdValueAttribute($.findActiveTab('ul.restOfYearTotal'));
}

FPPage.buttonShowRowExpandSubTable = function () {
    $.findActiveTab(".js_buttonShowRowExpandSubTable").bind("click", function () {
        var buttonForsubtableTrRows = $(this);
        var subtableTrRows = $(this).closest("tbody").find("tr.totalDisbursementProjections");
        subtableTrRows.toggleClass("hide");

        if (subtableTrRows.hasClass("hide")) {
            buttonForsubtableTrRows.html(FPPage.plusSign)
        } else {
            buttonForsubtableTrRows.html(FPPage.minusSign)
        };
    })
}

FPPage.loadToolTipMessage = function (element) {
    element.find("label.lgTit")
        .addClass("tooltips")
        .append('<span class="financial-tooltip hide">' + element.attr("data-tooltipMessage") + '</span>');
}

FPPage.excelReportGeneration = function () {
    $.findActiveTab('.buttonExcel').click(function () {
        var currentDate = new Date();
        var isDisbursementExtraVisible = $.findActiveTab('#tableDisbursementExtra').hasClass('hide');

        if (isDisbursementExtraVisible) {
            FPPage.exlcludeDisbursementExtraFromReport();
        }

        var tableOldHtml = $.findActiveTab('#tableFinancialPrincipal').html();
        var financialPlanMainTable = $.findActiveTab('#tableFinancialPrincipal');
        var executedAmountColumnIndex = financialPlanMainTable.find('th.executedAmountColumn').index();
        var executedAmountPercentColumnIndex = financialPlanMainTable.find('th.executedAmountPercentColumn').index();
        var totalCostEACColumnIndex = financialPlanMainTable.find('th.totalCostEACColumn').index();
        var oldRemainingBalanceColumnIndex = financialPlanMainTable.find('th.remainingBalanceColumn').index();
        var totalAmountColumn = financialPlanMainTable.find('th.totalAmountColumn').index();
        var yearCurrentColumn = financialPlanMainTable.find('th.yearCurrent').index();
        var yearNextColumn = financialPlanMainTable.find('th.yearNext').index();
        var restOfYearColumn = financialPlanMainTable.find('th.restOfYear').index();
        FPPage.moveColumnAfter(financialPlanMainTable, totalAmountColumn, totalCostEACColumnIndex - 2);
        FPPage.moveColumnAfter(financialPlanMainTable, executedAmountColumnIndex, totalCostEACColumnIndex + 1);
        FPPage.moveColumnAfter(financialPlanMainTable, executedAmountPercentColumnIndex, totalCostEACColumnIndex + 2);
        var newRemainingBalanceColumnIndex = financialPlanMainTable.find('th.remainingBalanceColumn').index();
        FPPage.moveColumnAfter(financialPlanMainTable, yearCurrentColumn, newRemainingBalanceColumnIndex);
        FPPage.moveColumnAfter(financialPlanMainTable, yearNextColumn, newRemainingBalanceColumnIndex + 1);

        $.findActiveTab(".financial-tables").table2excel({
            exclude: FPPage.classesToBeExcludedFromReport,
            name: FPPage.FINANCIAL_PLAN,
            filename: FPPage.FINANCIAL_PLAN + "_" + currentDate.toLocaleDateString(),
            sheetName: FPPage.FINANCIAL_PLAN
        });

        $.findActiveTab('#tableFinancialPrincipal').html(tableOldHtml);
        bindHandlers();

        window.setTimeout(function () {
            if (isDisbursementExtraVisible) {
                FPPage.removeDisbursementExtraFromReportClass();
            }
        }, 3);
	});
}

FPPage.moveColumn = function (table, from, to) {
    var rows = jQuery('tr', table);
    var columns;
    rows.each(function () {
        columns = jQuery(this).children('th, td').not('[data-pagemode=edit]');
        columns.eq(from).detach().insertBefore(columns.eq(to));
    });
}

FPPage.moveColumnAfter = function (table, from, to) {
    var rows = jQuery('tr', table);
    var columns;
    rows.each(function () {
        columns = jQuery(this).children('th, td').not('[data-pagemode=edit]');
        columns.eq(from).detach().insertAfter(columns.eq(to));
    });
}

FPPage.cleanCollapseAllElementsData = function () {
    $.each($('.btnCollapseAll'), function () {
        var currentElement = $(this).find('label');
        var currentElementLabelText = currentElement.text().trim().split(' ');
        var currentElementAttrValue = currentElement.attr("dd-value").trim().split(' ');
        var labelText = currentElementLabelText[0] + " " + currentElementLabelText[1];
        var attrValue = currentElementAttrValue[0] + " " + currentElementAttrValue[1];
        currentElement.text(labelText);
        currentElement.attr('dd-value', attrValue);
    });
}

FPPage.hideMonthsWhenEnteringEditMode = function () {
    var table = $.findActiveTab("#tableFinancialPrincipal");
    var selectMonths = table.find(".financialMonth");
    selectMonths.css("display", "none");
    selectMonths.removeClass("showDataEdit");
}

FPPage.showMonthInputs = function () {
    var selectFilter = $.findActiveTab(".filter-search");
    var table = $.findActiveTab("#tableFinancialPrincipal");
    var selectMonths = table.find(".financialMonth");
    var selectTotals = table.find(".financial-totals");
    var tdExecutedEditMode = table.find('.td_execute_editMode');

    selectFilter.css("visibility", "visible");
    $.findActiveTab('.filterButton').removeClass("hide");
    periodFilter.showFilteredTable();
    FPPage.showDisbursementTables();
    selectMonths.css("display", "");
    tdExecutedEditMode.removeClass("showDataEdit");
}

FPPage.updateUSDValueForInputs = function () {
    $.each($("input[data-usdvalue]"), function () {
        FPPage.updateUSDValue($(this));
    });
}

FPPage.updateUSDValue = function (element) {
    var currencyElement = $.findActiveTab('.widthDropdownCurrency');
    var currency = currencyElement.GetText();
    var currentElement = element;

    if (currency === FPPage.defaultCurrency) {
        var currentValue = calculations.removeCommas(currentElement.val());
        currentElement.attr("data-usdValue", currentValue);
    }

}

FPPage.updateRemainingBalanceWhenExecutedAmount = function () {
    $('.executedAmountInput').change(function () {
        var currentRow = $(this).parents('tr');
        var currentExecutedAmount = $(this);
        var currentExecutedAmountParentType = currentExecutedAmount.parent('li').attr("class");
        var currentExecutedAmountValueAsString = currentExecutedAmount.val().replace(/,/g, "");
        var currentTotalEACValueAsString = currentRow.find('ul.totalCostEAC').
                                            find('li.' + currentExecutedAmountParentType).text().trim().
                                            replace(/,/g, "");
        var remainingBalanceTotal = parseFloat(currentTotalEACValueAsString) - parseFloat(currentExecutedAmountValueAsString);
        var formattedRemainingBalanceTotal = calculations.convertDecimalToFixedString(remainingBalanceTotal);
        var currentRemainingBalance = currentRow.find('ul.remainingBalance').
                                            find('li.' + currentExecutedAmountParentType).
                                            text(formattedRemainingBalanceTotal);

        FPPage.calculateRemainingBalanceTotal();
    });
}

FPPage.updateRestOfYearTotalAfterValueChanges = function () {
    $('.restOfYearInput').change(function () {
        FPPage.calculateRestOfYearTotal();
    });
}

FPPage.getMonthsDifference = function () {
    var yearFrom = $.findActiveTab('#id-yearFilterFrom').GetValue();
    var monthFrom = $.findActiveTab('#id-monthFilterFrom').GetValue();
    var yearTo = $.findActiveTab('#id-yearFilterTo').GetValue();
    var monthTo = $.findActiveTab('#id-monthFilterTo').GetValue();
    var dateFrom = new Date(yearFrom + ',' + monthFrom);
    var dateTo = new Date(yearTo + ',' + monthTo);
    var monthsDifference = periodFilter.validationMinimumValue + dateTo.getMonth() - dateFrom.getMonth();
    return monthsDifference;
}

FPPage.isEditMode = function () {
    var isEditMode = false;
    if ($.findActiveTab('[data-action="cancel"]').length > 0) {
        isEditMode = !$.findActiveTab('[data-action="cancel"]').first().parent('li').hasClass('hide');
    }
    return isEditMode;
}


FPPage.updateUSDValueForInputs = function () {
    $.each($("input[data-usdvalue]"), function () {
        FPPage.updateUSDValue($(this));
    });
}

FPPage.updateUSDValue = function (element) {
    var currencyElement = $.findActiveTab('.widthDropdownCurrency');
    var currency = currencyElement.GetText();
    var currentElement = element;

    if (currency === FPPage.defaultCurrency) {
        var currentValue = calculations.removeCommas(currentElement.val());
        currentElement.attr("data-usdValue", currentValue);
    }

}

FPPage.exlcludeDisbursementExtraFromReport = function () {
    $.findActiveTab('#tableDisbursementExtra').find("tr").addClass("excludeThis");
}

FPPage.removeDisbursementExtraFromReportClass = function () {
    $.findActiveTab('#tableDisbursementExtra').find("tr").removeClass("excludeThis");
}

FPPage.stringFormatIDBDisbursements = function () {
    FPPage.stringFormatP('accumulatedAdvances');
    FPPage.stringFormatP('notJustifiedExpensesRead');
    FPPage.stringFormatP('pendingJustification');
}

FPPage.stringFormatP = function (elementClass) {
    var element = $.findActiveTab('td.' + elementClass).find('p');
    element.attr("data-usdValue", calculations.transformToDecimal(element));
}

FPPage.clearDisbursementsInputs = function () {
    var disbursementUL = $.findActiveTab('#tableDisbursementExtra').find('tr.totalDisbursementProjections').find('.disbursement-edit').find('ul');

    $.each(disbursementUL, function () {
        var currentUl = $(this).not('js_blockedMonth');

        currentUl.find('input').val(FPPage.clearInputValue);
        currentUl.find('input').attr('data-usdValue', FPPage.clearInputValue);
    });                                               
}

FPPage.horizontalScrollTopAndBottom = function () {
    $('.horizontalScrollTopBar').on('scroll', function (e) {
        $.findActiveTab('.table-responsive').scrollLeft($(this).scrollLeft());
    });

    $('.table-responsive').on('scroll', function (e) {
        $.findActiveTab('.horizontalScrollTopBar').scrollLeft($(this).scrollLeft());
    });
}

FPPage.updateTopHorizontalScrollBarWidth = function () {
    $.findActiveTab('.hiddenDivForScrollBar').width($.findActiveTab("#tableFinancialPrincipal").width());
}

FPPage.removeTopHorizontalScrollBarWidth = function () {
    $.findActiveTab('.hiddenDivForScrollBar').width(FPPage.zero);
}

FPPage.addOrRemoveTopHorizontalScrollBarWhenTableIsCollapsed = function () {
    var isPlanningPeriodSelected = $.findActiveTab("[name=SelectPlanningPeriod]").prop("checked");
    var isTableNotVisible = $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr').first().hasClass('hide');

    if (isTableNotVisible) {
        FPPage.removeTopHorizontalScrollBarWidth();
    }

    if (!isTableNotVisible) {
        isPlanningPeriodSelected ? FPPage.updateTopHorizontalScrollBarWidth() : FPPage.removeTopHorizontalScrollBarWidth();
    }
}

FPPage.cleanCollapseAllText = function () {
    var currentCollapseElement = $.findActiveTab('.btnCollapseAll');
    var collapseText = currentCollapseElement.text().trim();
    var isCollapse = currentCollapseElement.hasClass("collapse");
    var activeTabTables = $.findActiveTab("table");
    var currentCollapseElementLabel = currentCollapseElement.find('label');
    var expandedTables = FPPage.loadTablesAccordingToSign(FPPage.minusSign, activeTabTables);
    var collapsedTables = FPPage.loadTablesAccordingToSign(FPPage.plusSign, activeTabTables);

    if (collapseText === FPPage.expandAllText) {
        FPPage.updateCollapseElementData(currentCollapseElementLabel, FPPage.expandAllText, FPPage.collapseAllText);
    } else {
        FPPage.updateCollapseElementData(currentCollapseElementLabel, FPPage.collapseAllText, FPPage.expandAllText);
    }
}

FPPage.singleExpandButtonClick = function () {
    $.findActiveTab(".tree.icon").find('.icon').click(function () {
        FPPage.cleanCollapseAllText();
        if ($(this).closest('table').attr('id') === "tableFinancialPrincipal") {
            FPPage.addOrRemoveTopHorizontalScrollBarWhenTableIsCollapsed();
        }
    });
}

FPPage.fixMarginButtonPanel = function () {
    var executionMode = $("." + FPPage.executionMode);
    var preparationMode = $("." + FPPage.preparationMode);

    executionMode.find(".financial-header-items").addClass("financial-header-items-only-excel");
    executionMode.find(".buttons-panel").addClass("buttons-panel-only-excel");
    preparationMode.find(".financial-header-items").addClass("financial-header-items-only-excel");
    preparationMode.find(".buttons-panel").addClass("buttons-panel-only-excel");
}

FPPage.heightTopScrollBar = function () {
    $.findActiveTab(".horizontalScrollTopBar, .hiddenDivForScrollBar").toggleClass("scroll-top-height");
}

FPPage.adjustLastUpdatedField = function () {
    var isEditMode = FPPage.isEditMode();
    var lastUpdateField = $.findActiveTab('article.lastUpdateField');

    if (isEditMode) {
        if (lastUpdateField.find('label.labelNormal').text() === "") {
            lastUpdateField.find('div.LabelsGroup').addClass('withoutInfoMinHeight');
        } else {
            lastUpdateField.find('div.LabelsGroup').removeClass('withoutInfoMinHeight');
        }
    }
}

FPPage.adjustLastUpdateFieldReadMode = function () {
    var lastUpdateField = $.findActiveTab('article.lastUpdateField');
    lastUpdateField.find('div.LabelsGroup').removeClass('withoutInfoMinHeight');
}

FPPage.convertFieldToNumberAndDots = function () {
    var field = $.findActiveTab("#filter-search-box").find("[name=orderNumber]");
    $(field).keypress(function (evt) {
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode != 46 && charCode > 31
          && (charCode < 48 || charCode > 57))
            return false;

        return true;
    })
}

FPPage.submitReviewButton = function () {
    FPPage.dataSubmit = {};
    FPPage.dataSubmit['workflowRequest'] = {};

    FPPage.dataSubmit['workflowRequest']['WorkflowCode'] = '';
    FPPage.dataSubmit['workflowRequest']['OperationNumber'] = $('input#operationNumber').val();
    FPPage.dataSubmit['workflowRequest']['ModuleName'] = '';
    FPPage.dataSubmit['workflowRequest']['EntityType'] = 'FinancialPlan';
    FPPage.dataSubmit['workflowRequest']['EntityId'] = $('input#financialPlanId').val();
    FPPage.dataSubmit['workflowRequest']['ReturnURL'] = '';


    FPPage.submitURL = $('input#financialPlanSubmit').val();
    $.findActiveTab(".submitReviewButton").click(function () {
        confirmAction(FPPage.warningMessageSubmitted).done(function (pressOk) {
            if (pressOk) {
                showLoader();
                window.setTimeout(function () {
                    $.ajax({
                        'url': FPPage.submitURL,
                        'type': 'post',
                        'data': FPPage.dataSubmit['workflowRequest'],
                        'success': function (data) {

                            if (!data['IsValid']) {
                                showMessage(FPPage.SaveMessages.errorMessage);
                                return;
                            }
                            else {

                                window.location.href = $('input#financialPlanWorkflowURL').val();
                                return;
                            }
                        },
                        'error': function () {
                            hideLoader();
                            showMessage(FPPage.SaveMessages.errorMessage);
                        }
                    });
                }, 5)
            }
        });
    });
    }