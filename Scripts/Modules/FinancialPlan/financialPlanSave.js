var FPage = new Object();

FPage.data = {};
FPage.data['model'] = {};
FPage.emptyString = "";
FPage.defaultValue = "0.00";
FPage.validationOk = true;
FPage.errorMessageValidation = FPage.emptyString;
FPage.monthFilterEmpty = 'MMM';
FPage.yearFilterEmpty = 'YYYY';
FPage.loadTypeTAB = 'TAB';
FPage.loadTypeFILTER = 'FILTER';

FPage.saveData = function (loadType) {

    FPage.saveURL = $('input#financialPlanSave').val();
    FPage.data['model']['OperationNumber'] = $('input#operationNumber').val();
    FPage.data['model']['ContractNumber'] = $(".SearchFinancialContractDropdown").GetValue();
    FPage.data['model']['PlanningPeriodBudgetQuota'] = FPage.convertToFloat($.findActiveTab('[name=InputPlanningPeriodBudgetQuota]').attr("data-usdValue"));
    FPage.data['model']['NotJustifiedExpenses'] = FPage.convertToFloat($.findActiveTab('[name=IdNotJustifiedExpenses]').attr('data-usdValue'));
    FPage.data['model']['FinancialTaskList'] = FPage.loadExecuteAmount();
    FPage.data['model']['FinancialDisbursementTypes'] = FPage.loadDisbursementTable();
    FPage.data['model']['Mode'] = FPPage.operationMode;
    FPage.validationFilterEmpty();

    if (FPage.validationOk) {
        if (loadType === FPage.loadTypeTAB) {
            Confirm.ShowInfo(FPPage.SaveMessages.message).done(function (pressOk) {
                if (pressOk) {
                    FPage.saveAction(loadType);
                }
            });
        }
        else {
            FPage.saveAction(loadType);
        }

    }
    else {
        Confirm.ShowInfo(FPage.errorMessageValidation);
    }
};

FPage.checkBool = function (value) {
    if (value === true || String(value).toLowerCase() === 'true') {
        return true;
    }
    return false;
};

FPage.loadMonths = function (monthTotals) {
    var months = [];
    monthTotals.each(function () {
        months.push({ "MonthIndex": $(this).data("month"), "Year": $(this).data("year"), "MonthTotal": $(this).attr('data-usdValue').trim().replace(/,/g, "") });;
    });

    return months;
}


FPage.loadExecuteAmount = function () {
    var rowsToBeProcessed = $.findActiveTab("#tableFinancialPrincipal").find("tbody").find("tr").not(':last');
    var taskListArr = [];
   
    $.each(rowsToBeProcessed, function () {
        var item = $(this);
        var taskId = item.data("task-id");
        taskList = {};
        idbData = {};
        localData = {};
        coData = {};
        var financialTaskSourceArr = [];
        var idb = item.find("[name='inputDecimalExecuteMonthIDB']");
        var local = item.find("[name='inputDecimalExecuteMonthLOCAL']");
        var co = item.find("[name='inputDecimalExecuteMonthCO']");
        var idbMonths = FPage.loadMonths(item.find("[name='idbMonthTotal']"));
        var localMonths = FPage.loadMonths(item.find("[name='localMonthTotal']"));
        var coMonths = FPage.loadMonths(item.find("[name='cofinancingMonthTotal']"));
        var restOfYearIdb = item.find("[name='restOfYearIDB']");
        var restOfYearLocal = item.find("[name='restOfYearLOCAL']");
        var restOfYearCo = item.find("[name='restOfYearCO']");

        idbData = FPage.loadSourceData(idb, "idb", idbMonths, restOfYearIdb);
        financialTaskSourceArr.push(idbData);
        localData = FPage.loadSourceData(local, "local", localMonths, restOfYearLocal);
        financialTaskSourceArr.push(localData);
        coData = FPage.loadSourceData(co, "co", coMonths, restOfYearCo);
        financialTaskSourceArr.push(coData);

        taskList = { "Id": taskId, "FinancialTaskSources": financialTaskSourceArr };
        taskListArr.push(taskList);
    });

    return taskListArr;
}

FPage.convertToFloat = function (stringVal)
{
    return parseFloat(stringVal.trim().replace(/,/g, ""));
}

FPage.loadDisbursementTable = function ()
{
    var rowsToBeProcessed = $.findActiveTab("#tableDisbursementExtra").find("tbody").find("tr.totalDisbursementProjections");
    var disbursementType = [];

    $.each(rowsToBeProcessed, function () {
        disbursement = {};
        monthData = [];


        var currentRow = $(this);
        var monthsToBeProcessed = currentRow.find("[data-pagemode='edit']").find("ul");
        var sourceType = currentRow.data("sourcetype");
        $.each(monthsToBeProcessed, function () {
            var currentMonth = $(this);
            month = {};

            var monthName = currentMonth.data("monthname");
            var monthIndex = currentMonth.data("monthindex");
            var monthAmount = currentMonth.find("li input").attr('data-usdValue').replace(/,/g, "");
            var monthYear = currentMonth.data("monthyear");

            month = { "Year" : monthYear, "MonthIndex" : monthIndex, "MonthName" : monthName, "ProjectionAmount" : monthAmount };
            monthData.push(month);
        });

        disbursement = { "DisbursementType": sourceType, "FinancialCashFlows": monthData }
        disbursementType.push(disbursement);
    });

    return disbursementType;
}

FPage.addFormatToString = function (element) {
    var result = element.attr('data-usdValue').trim().replace(/,/g, "");
    if (result === FPage.emptyString) {
        result = FPage.defaultValue;
    }
    return result;
}

FPage.loadSourceData = function (element, sourceType, months, restOfYear) {
    var data = {};
    data = {
        "ExecuteAmount": FPage.addFormatToString(element),
        "SourceType": element.data("source" + sourceType),
        "RestOfYear": FPage.addFormatToString(restOfYear),
        "FinancialMonth": months
    };
    return data;
}

FPage.setActiveTab = function () {
    var tabs = $("ul.tabs");
    var activeTab = tabs.find('li.active').attr('dd-tab');
    $("[dd-tab=" + activeTab + "]").click();
}

FPage.totalsValidation = function () {
    var totalAvailableFunds = parseFloat($.findActiveTab('#TotalAvailableFunds').find('span label').attr('data-usdvalue'));
    var totalDisbursement = parseFloat($.findActiveTab('.disbursementProjections-Total').find('li').attr('data-usdvalue'));
    var projectedLiquidityNeeds = parseFloat($.findActiveTab('.projectedLiquidityNeedsTotal').find('li').attr('data-usdvalue'));
    var restOfYearTotalIDB = parseFloat($.findActiveTab('ul.restOfYearTotal').find('li.idb').attr('data-usdvalue'));
    var filterElements = $.findActiveTab('.filter-data-container');
    var startMonthFilter = filterElements.find('#id-monthFilterFrom').find('span.valueText').text();
    var startYearFilter = filterElements.find('#id-yearFilterFrom').find('span.valueText').text();
    var endMonthFilter = filterElements.find('#id-monthFilterTo').find('span.valueText').text();
    var endYearFilter = filterElements.find('#id-yearFilterTo').find('span.valueText').text();

    FPage.validationOk = true;

    if (projectedLiquidityNeeds > totalDisbursement) {
        FPage.validationOk = false;
        FPage.errorMessageValidation = FPPage.SaveMessages.errorMessageNotEnoughDisbursements;
    }

    if (FPage.validationOk && totalAvailableFunds < totalDisbursement) {
        FPage.validationOk = false;
        FPage.errorMessageValidation = FPPage.SaveMessages.errorMessageTooManyDisbursements;
    }

    if (FPage.validationOk && totalAvailableFunds <= restOfYearTotalIDB) {
        FPage.validationOk = false;
        FPage.errorMessageValidation = FPPage.SaveMessages.errorMessageRestOfYearTotalIDBBiggerThanTotalAvailableFunds;
    }

    if (FPage.validationOk && (startMonthFilter == FPage.monthFilterEmpty ||
        endMonthFilter == FPage.monthFilterEmpty ||
        startYearFilter == FPage.yearFilterEmpty ||
        endYearFilter == FPage.yearFilterEmpty)) {
        FPage.validationOk = false;
        FPage.errorMessageValidation = FPPage.SaveMessages.errorMessageProjectionFilterEmpty;
    }
}

FPage.validationFilterEmpty = function () {
    var filterElements = $.findActiveTab('.filter-data-container');
    var startMonthFilter = filterElements.find('#id-monthFilterFrom').find('span.valueText').text();
    var startYearFilter = filterElements.find('#id-yearFilterFrom').find('span.valueText').text();
    var endMonthFilter = filterElements.find('#id-monthFilterTo').find('span.valueText').text();
    var endYearFilter = filterElements.find('#id-yearFilterTo').find('span.valueText').text();
    FPage.validationOk = true;

    if (FPage.validationOk && (startMonthFilter == FPage.monthFilterEmpty ||
        endMonthFilter == FPage.monthFilterEmpty ||
        startYearFilter == FPage.yearFilterEmpty ||
        endYearFilter == FPage.yearFilterEmpty)) {
        FPage.validationOk = false;
        FPage.errorMessageValidation = FPPage.SaveMessages.errorMessageProjectionFilterEmpty;
    }
}

FPage.saveAction = function (loadType) {
    FPage.totalsValidation();
    if (FPage.validationOk) {
        showLoader();
        window.setTimeout(function () {
            $.ajax({
                'url': FPage.saveURL,
                'type': 'post',
                'data': FPage.data['model'],
                'success': function (data) {
                    if (!data) {
                        showMessage(FPPage.SaveMessages.errorMessage);
                        return;
                    }
                    else if (data && !FPage.checkBool(data['isValid'])) {
                        showMessage(data['message']);
                        return;
                    }

                    hideLoader();
                    if (loadType === FPage.loadTypeTAB) {
                        FPage.setActiveTab();
                    }
                    else {
                        periodFilter.periodFilterAction();
                    }
                },
                'error': function () {
                    hideLoader();
                    showMessage(FPPage.SaveMessages.errorMessage);
                }
            });
        }, 5)
    }
    else {
        Confirm.ShowInfo(FPage.errorMessageValidation);
    }
    return;
}