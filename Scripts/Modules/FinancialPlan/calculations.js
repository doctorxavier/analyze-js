var calculations = new Object();

calculations.calculateSum = function (columnElement, totalElement, mode) {
    var sum = 0;
    mode = FPPage.checkModeToBeUsed(mode);
    sum += calculations.sumElements(columnElement, mode);
    totalElement.text(calculations.numberWithCommas(sum.toFixed(2).toString()));
}

calculations.calculateTwoColumnsSum = function (columnElementOne, ColumnElementTwo, totalElement) {
    var sum = calculations.sumElements(columnElementOne);
    sum += calculations.sumElements(ColumnElementTwo);
    totalElement.text(calculations.numberWithCommas(sum.toFixed(2).toString()));
}

calculations.calculateTwoColumnsSubstraction = function (columnElementOne, ColumnElementTwo, totalElement) {
    var substraction = calculations.sumElements(columnElementOne);
    substraction -= calculations.sumElements(ColumnElementTwo);
    totalElement.text(calculations.numberWithCommas(substraction.toFixed(2).toString()));
}

calculations.sumElements = function (columnElement, mode) {
    var sum = 0;
    mode = FPPage.checkModeToBeUsed(mode);
    var value = "";

    columnElement.each(function () {
        var currentColumnElement = $(this);

        if (FPPage.isCurrentModeRead(mode)) {
            value = currentColumnElement.text().trim().replace(/,/g, "");
        } else {
            value = currentColumnElement.val().trim().replace(/,/g, "");
        }

        if (!isNaN(value) && value.length != 0) {
            sum += parseFloat(value);
        }
    });

    return sum;
}

calculations.substractElements = function (substracter, substractee) {
    var substraction = 0;
    var substracterText = substracter.text().replace(/,/g, "");
    var substracteeText = substractee.text().replace(/,/g, "");
    substraction = parseFloat(substracteeText) - parseFloat(substracterText);

    if (isNaN(substraction)) {
        substraction = 0;
    }

    return substraction;
}

calculations.calculateSumElementsAndRefreshTotal = function (elementsForSum, totalTarget, mode) {
    mode = FPPage.checkModeToBeUsed(mode);
    calculations.calculateSum(elementsForSum.find('li.idb'), totalTarget.find('li.idb'), mode);
    calculations.calculateSum(elementsForSum.find('li.localCounterPart'), totalTarget.find('li.localCounterPart'), mode);
    calculations.calculateSum(elementsForSum.find('li.cofinancing'), totalTarget.find('li.cofinancing'), mode);
}

calculations.numberWithCommas = function(numberToBeTransformed) {
    return numberToBeTransformed.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

calculations.AddNumberWithCommasToElements = function (selectedElement) {
    selectedElement.each(function () {
        var value = $(this).text();
        $(this).text(numberWithCommas(value));
    });
}

calculations.removeCommas = function (elementValue) {
    return elementValue.replace(/,/g, "")
}

calculations.convertDecimalToFixedString = function (element) {
    return calculations.numberWithCommas(element.toFixed(2));
}

calculations.previousCurrencyUpdateCalculation = function (currentValueToBeUpdated, previousCurrency) {
    var result = 0;
    result = (currentValueToBeUpdated / previousCurrency);
    return result;
}

calculations.transformToDecimal = function (element){
    return parseFloat(calculations.removeCommas(element.text()));
}
