var periodFilter = new Object();
periodFilter.validationMinimumValue = 1;
periodFilter.validationMaximumValue = 12;
periodFilter.validationBaseValue = 0;
periodFilter.filterUrl = '';
periodFilter.contractNumber = '';
periodFilter.operationNumber = '';

periodFilter.filterDatesValidation = function () {
    var yearFrom = $.findActiveTab('#id-yearFilterFrom').GetValue();
    var monthFrom = $.findActiveTab('#id-monthFilterFrom').GetValue();
    var yearTo = $.findActiveTab('#id-yearFilterTo').GetValue();
    var monthTo = $.findActiveTab('#id-monthFilterTo').GetValue();
    var dateFrom = new Date(yearFrom,monthFrom);
    var dateTo = new Date(yearTo,monthTo);

    var monthsDifference = dateTo.getMonth() - dateFrom.getMonth() + (12 * (dateTo.getFullYear() - dateFrom.getFullYear()));

    return monthsDifference;
}

periodFilter.isThereNoneEmptyFilterValue = function () {
    var yearFrom = $.findActiveTab('#id-yearFilterFrom').GetValue();
    var monthFrom = $.findActiveTab('#id-monthFilterFrom').GetValue();
    var yearTo = $.findActiveTab('#id-yearFilterTo').GetValue();
    var monthTo = $.findActiveTab('#id-monthFilterTo').GetValue();

    return (periodFilter.isEmptyOrDefault(yearFrom) ||
            periodFilter.isEmptyOrDefault(monthFrom) ||
            periodFilter.isEmptyOrDefault(yearTo) ||
            periodFilter.isEmptyOrDefault(monthTo));
}

periodFilter.isEmptyOrDefault = function (fieldToBeCheckedValue) {
    var emptyString = "";
    var defaultValue = "(select option)";

    return (fieldToBeCheckedValue === emptyString || fieldToBeCheckedValue === defaultValue);
}

periodFilter.periodFilterCall = function (filterUrl, contractNumber, operationNumber) {
    var filterCanNotBeApplied = $.findActiveTab("#filterCanNotBeApplied").val();
    var filterEmptyCanNotBeApplied = $.findActiveTab("#filterEmptyCanNotBeApplied").val();
    var startDateBiggerThanEndDateMessage = $.findActiveTab("#startDateBiggerThanEndDateMessage").val();

    if (!periodFilter.isThereNoneEmptyFilterValue()) {
        var filterValidation = periodFilter.filterDatesValidation();
        var baseValidation = filterValidation >= periodFilter.validationBaseValue;
        var maximumValidation = filterValidation < periodFilter.validationMaximumValue;
        var isValid = baseValidation && maximumValidation;

        if (isValid) {
            periodFilter.filterUrl = filterUrl;
            periodFilter.contractNumber = contractNumber;
            periodFilter.operationNumber = operationNumber;
            periodFilter.applyFilterLogic();

        } else {
            var message = baseValidation ? filterCanNotBeApplied : startDateBiggerThanEndDateMessage;
            showMessage(message);
        }
    }
    else {
        showMessage(filterEmptyCanNotBeApplied);
    }
}

periodFilter.applyFilterLogic = function () {
    var isEditMode = FPPage.isEditMode();
    if (isEditMode) {
        Confirm.ShowInfo(FPPage.SaveMessages.messageConfirmApplyFilter, '',
        FPPage.SaveMessages.OptionYes,
        FPPage.SaveMessages.OptionNo,
        false)
        .done(function (pressOk) {
            if (pressOk) {
                FPage.saveData(FPage.loadTypeFILTER);
            }
            else {
                periodFilter.periodFilterAction();
            }
        });
    } else {
        periodFilter.periodFilterAction();
    }
}

periodFilter.periodFilterAction = function () {
    var yearFrom = $.findActiveTab('#id-yearFilterFrom').GetValue();
    var monthFrom = $.findActiveTab('#id-monthFilterFrom').GetValue();
    var yearTo = $.findActiveTab('#id-yearFilterTo').GetValue();
    var monthTo = $.findActiveTab('#id-monthFilterTo').GetValue();
    var curretTabActive = $('ul.tabs .active').attr('dd-tab').substring(1);
    var isEditMode = FPPage.isEditMode();
    var totalMonths;
    var fpMode;

    showLoader();
    window.setTimeout(function () {
        var response = postUrlWithOptions(periodFilter.filterUrl, { async: false },
                {
                    'contractNumber': periodFilter.contractNumber,
                    'operationNumber': periodFilter.operationNumber,
                    'startMonthStr': monthFrom, 'startYearStr': yearFrom,
                    'endMonthStr': monthTo, 'endYearStr': yearTo,
                    'currentTab': curretTabActive
                }).done(function (data) {
                    $("#" + curretTabActive).html(data.TabView);
                    totalMonths = data.Months;
                    fpMode = data.fpMode;
                });

        periodFilter.showFilteredTable();
        FPPage.loadInitialDataAndFunctions(totalMonths);
        hideLoader();
        if (!isEditMode) {
            exitEditMode(false, $.findActiveTab('section'), false, false);
            exitEditMode(false, $.findActiveTab('section'), false, false);
        }
        else {
            FPPage.editModeFunctions();
        }

        if (fpMode === FPPage.preparationMode) {
            $.findActiveTab('.submitReviewButton').addClass('hide');
        }

        $.findActiveTab("[name=SelectPlanningPeriod]").click();
        FPPage.checkStateOperation(fpMode);
        bindHandlers();
    }, 5);
}

periodFilter.showFilteredTable = function () {
    var table = $.findActiveTab("#tableFinancialPrincipal");
    var selectMonths = table.find(".financialMonth");
    var selectTotals = table.find(".financial-totals");
    var yearCurrentFilter = $.findActiveTab("#yearsCurrent");
    var yearNextFilter = $.findActiveTab("#yearsNext");
    var isEditMode = FPPage.isEditMode();
    var currentTab = $('ul.tabs .active').attr('dd-tab').substring(1);

    selectMonths.css("display", "table-cell");
    selectTotals.css("display", "none");
    yearCurrentFilter.css("display", "none");
    yearNextFilter.css("display", "none");

    if (isEditMode) {
        enterEditMode(false, $('#' + currentTab), true);
    } else {
        exitEditMode(false, $('#' + currentTab), true);
    }

    periodFilter.updateCurrencyForNotFilteredElements();
    mode = isEditMode ? FPPage.EDIT_MODE : FPPage.READ_MODE;
    FPPage.disbursmentCalculation(mode);
    periodFilter.calculateTotals();
    FPPage.loadUsdValueForMonthsTotal();
    FPPage.tableFinancialPrincipalTotalsUsdValueLoad();
}

periodFilter.calculateTotals = function () {
    FPPage.calculateTotalsForEOPEAC();
    FPPage.calculateTotalsForMonthsVertically();
    FPPage.calculateTotalForMonthsHorizontally();
    FPPage.calculateTotalBySourceTotal();
    FPPage.restOfYearCalculation();
    FPPage.calculateRemainingBalanceTotal();
    FPPage.calculateRemainingToProject();
    periodFilter.disbursementTableCalculations();
    FPPage.updateProjectionAmountTotalsWhenEnteringEditMode();
}

periodFilter.disbursementTableCalculations = function () {
    FPPage.calculateProjectedLiquidityNeeds();
    FPPage.disbursementTypeColumnsCalculation();
    FPPage.calculateIdbCashFlowWithoutDisbursmentMonthlyTotals();
    FPPage.calculateHorizontalTotalForDisbursementProjections();
}

periodFilter.updateCurrencyForNotFilteredElements = function () {
    var currencyDropdown = $.findActiveTab('.widthDropdownCurrency');
    currencyDropdown.closest('.dropdown').find('input.hide').attr('value', FPPage.defaultCurrencyValue);
    currencyDropdown.closest('.dropdown').find('input.hide').val(FPPage.defaultCurrencyValue);
    currencyDropdown.closest('.dropdown').find('.valueText').text(FPPage.defaultCurrency);
    FPPage.currentSelectedCurrency(parseFloat(FPPage.defaultCurrencyValue), FPPage.defaultCurrency, currencyDropdown.attr("previouslySelectedCurrencyValue"), $.findActiveTab('#tableDisbursement').find('[data-usdValue]'));
    FPPage.currentSelectedCurrency(parseFloat(FPPage.defaultCurrencyValue), FPPage.defaultCurrency, currencyDropdown.attr("previouslySelectedCurrencyValue"), $.findActiveTab('div.headerInformationBlock').find('[data-usdValue]'));
    currencyDropdown.attr("previouslySelectedCurrencyValue", FPPage.defaultCurrencyValue);
}