var filterSearchModule = new Object();

filterSearchModule.filterSearchValidations = function () {
    var filterSearchBox = $.findActiveTab("#filter-search-box");
    var clearFilterSearch = filterSearchBox.find("[name=clearFilterSearch]");
    var sendFilterSearch = filterSearchBox.find("[name=sendFilterSearch]");

    $.findActiveTab(".input-filter-search-dropdown").find("input").addClass("input-filter-search");

    $(sendFilterSearch).on("click", function () {
        var allInputsSearch = filterSearchBox.find(".input-filter-search");
        var allInputsSearchValues = $.map(allInputsSearch, function (element) {
            return $(element).val();
        });
        var validateInputContent = allInputsSearchValues.some(function (element) {

            if (element != "") {
                return true
            }
        })

        if (validateInputContent) {
            $.findActiveTab("#errorEmptyFieldFilter").find("label").addClass("hide");
            var expandValue = $.findActiveTab("#tableFinancialPrincipal").find("thead").find("span.icon").text();
            
            if (expandValue === "-") {
                filterSearchModule.filterSearch(filterSearchBox);
            }
        } else {
            $.findActiveTab("#errorEmptyFieldFilter").find("label").removeClass("hide");

            $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr')
                .not('.financial-table-results-bottom').removeClass("filterTrHide");
        }
    })

    $(clearFilterSearch).on("click", function () {
        var allInputsSearchForClear = filterSearchBox.find(".input-filter-search");
        $(allInputsSearchForClear).val("");
        $.findActiveTab("#errorEmptyFieldFilter").find("label").addClass("hide");
        $.findActiveTab(".input-filter-search-dropdown").FirstorDefault();
        $.findActiveTab("#tableFinancialPrincipal").find('tbody').find('tr')
            .not('.financial-table-results-bottom').removeClass("filterTrHide");
    })
}

filterSearchModule.validateFilterSearchGroupOfValues = function (groupOfValues, Operator, inputValue) {
    var group = $.makeArray(groupOfValues);
    var result = group.some(function (element) {

        switch (Operator) {
            case ">": return (element > inputValue);
            case "<": return (element < inputValue);
            case "=": return (inputValue === element);
            case "=/=": return (inputValue != element);
            default: return false
        }
    })

    return result
}

filterSearchModule.convertToDecimalFilterData = function (element) {
    var removeCommas = element.replace(/,/g, "");
    return parseFloat(removeCommas);
}

filterSearchModule.filterSearch = function (filterSearchBox) {
    var inputId = filterSearchBox.find("[name=id]").val();
    var inputOrderNumber = filterSearchBox.find("[name=orderNumber]").val();
    var inputTaskName = filterSearchBox.find("[name=taskName]").val().toLowerCase();
    var inputTaskType = filterSearchBox.find("[name=taskType]").val();
    var inputTypeTaskValidated = inputTaskType || "";
    var inputTotalCost = filterSearchBox.find("[name=totalCost]").val();
    var inputRemainingBalance = filterSearchBox.find("[name=remainingBalance]").val();
    var inputTotalBySource = filterSearchBox.find("[name=totalBySource]").val();
    var inputRemainingToProject = filterSearchBox.find("[name=remainingToProject]").val();
    var tableTr = $.findActiveTab("#tableFinancialPrincipal").find('tbody')
        .find('tr').not('.financial-table-results-bottom');
    var operatorTotalCost = filterSearchBox.find("[name=totalCostOperator]").val();
    var operatorRemainingBalance = filterSearchBox.find("[name=remainingBalanceOperator]").val();
    var operatorTotalBySource = filterSearchBox.find("[name=totalBySourceOperator]").val();
    var operatorRemainingToProject = filterSearchBox.find("[name=remainingToProjectOperator]").val();

    var trForShow = [];
    var trForHide = [];

    $.each(tableTr, function () {
        var tr = $(this)
        var trId = tr.find(".taskId").find("p").text();
        var trOrderNumber = tr.find(".orderNumber").find("p").text();
        var trTaskName = tr.find(".taskNameBox").find("p").text().toLowerCase();
        var trTaskType = tr.find(".taskTypeBox").find("p").text();
        var trTotalCostEAC = tr.find("ul.totalCostEAC").find("li").map(function () {
            return parseFloat($(this).find("label").text().replace(/,/g, ""))
        });
        var trRemainingBalance = tr.find("ul.remainingBalance").find("li").map(function () {
            return parseFloat($(this).find("label").text().replace(/,/g, ""))
        });
        var trTotalBySource = tr.find("ul.totalBySource").find("li").map(function () {
            return parseFloat($(this).text().replace(/,/g, ""))
        });
        var trRemainingToProject = tr.find("ul.remainingToProject").find("li").map(function () {
            return parseFloat($(this).text().replace(/,/g, ""))
        });
        var isFound = true;

        if (trId.indexOf(inputId) === -1) {
            isFound = false;
        };

        if (isFound === true && trOrderNumber.indexOf(inputOrderNumber) === -1) {
            isFound = false;
        };

        if (isFound === true && trTaskName.indexOf(inputTaskName) === -1) {
            isFound = false;
        };

        if (isFound === true && (inputTypeTaskValidated.length > 0 && trTaskType != inputTypeTaskValidated)) {
            isFound = false;
        }

        if (isFound === true && inputTotalCost != "") {
            var totalCostValidated = filterSearchModule.validateFilterSearchGroupOfValues
                (trTotalCostEAC, operatorTotalCost, filterSearchModule.convertToDecimalFilterData(inputTotalCost));

            if (totalCostValidated === false) {
                isFound = false;
            }
        }

        if (isFound === true && inputRemainingBalance != "") {
            var remainingBalanceValidated = filterSearchModule.validateFilterSearchGroupOfValues
                (trRemainingBalance, operatorRemainingBalance, filterSearchModule.convertToDecimalFilterData(inputRemainingBalance));

            if (remainingBalanceValidated === false) {
                isFound = false;
            }
        }

        if (isFound === true && inputTotalBySource != "") {
            var totalBySourceValidated = filterSearchModule.validateFilterSearchGroupOfValues
                (trTotalBySource, operatorTotalBySource, filterSearchModule.convertToDecimalFilterData(inputTotalBySource));

            if (totalBySourceValidated === false) {
                isFound = false;
            }
        }

        if (isFound === true && inputRemainingToProject != "") {
            var remainingToProjectValidated = filterSearchModule.validateFilterSearchGroupOfValues
                (trRemainingToProject, operatorRemainingToProject, filterSearchModule.convertToDecimalFilterData(inputRemainingToProject));

            if (remainingToProjectValidated === false) {
                isFound = false;
            }
        }

        if (isFound === true) {
            trForShow.push(tr);
        } else {
            trForHide.push(tr);
        }
        return
    })

    $(trForHide).each(function (index, element) {
        $(element).addClass("filterTrHide");
    });

    $(trForShow).each(function (index, element) {
        $(element).removeClass("filterTrHide");
    });
}