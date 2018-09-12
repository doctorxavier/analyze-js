function disableValidations() {
    $('input[data-optional-required]').attr('data-optional-required', false);
    $('input[data-parsley-required]').attr('data-parsley-required', false);
    $('input[data-force-parsley-validation]').attr('data-force-parsley-validation', false);
    $('textarea[data-optional-required]').attr('data-optional-required', false);
    $('textarea[data-parsley-required]').attr('data-parsley-required', false);
    $('textarea[data-force-parsley-validation]').attr('data-force-parsley-validation', false);
    $('select[data-optional-required]').attr('data-optional-required', false);
    $('select[data-parsley-required]').attr('data-parsley-required', false);
    $('select[data-force-parsley-validation]').attr('data-force-parsley-validation', false);
    $('input[required]').removeAttr('required');
    $('select[required]').removeAttr('required');
    $('textarea[required]').removeAttr('required');
    $('input[data-parsley-nonzero]').removeAttr('data-parsley-nonzero');
    $('input[data-parsley-arithmeticSum]').removeAttr('data-parsley-arithmeticSum');
}

function selectOperations(source) {
    var container = source.closest('div.tcSupportedOperation');
    changeLabelCheck(container, source.checked);
    selectAllOperation(container, source.checked);
}

function changeLabelCheck(container, isCheck) {
    if (isCheck) {
        $(container).find("[name='selectAll']").addClass("hide");
        $(container).find("[name='unselectAll']").removeClass("hide");
    }
    else {
        $(container).find("[name='selectAll']").removeClass("hide");
        $(container).find("[name='unselectAll']").addClass("hide");
    }
}

function selectAllOperation(container, checkAll) {
    var operations = $(container).find("[name='checkRelatedOperation']");
    if (operations.length > 0) {
        operations.each(function (index, element) {
            element.checked = checkAll;
        });
    }
}

function allCheckOperations(container, source) {
    var operations = $(container).find("[name='checkRelatedOperation']");
    if (operations.length > 0) {
        var allCheck = true;
        operations.each(function (index, element) {
            if ($(element).data('id') !== $(source).data('id') && !element.checked)
                allCheck = false;
        });
        return allCheck;
    }
    return false;
}

function checkOperation(source) {
    var container = source.closest('div.tcSupportedOperation');
    var checkGeneral = $(container).find("[name='checkOperationSupport']");
    var allCheck = allCheckOperations(container, source);
    if (source.checked && !checkGeneral.prop('checked') && allCheck) {
        checkGeneral.prop('checked', true);
        changeLabelCheck(container, true);
    }
    if (!source.checked && checkGeneral.prop('checked')) {
        checkGeneral.prop('checked', false);
        changeLabelCheck(container, false);
    }
}

function firstCounterpartFinancingValidation() {
    var compsCounterpart = $('input[name=counterpartFinancingComponent]');
    if (compsCounterpart.length > 0) {
        compsCounterpart.each(function (index, element) {
            var row = element.closest('tr');
            element.attr ?
                element.attr('onchange', 'counterpartFinancingChanged(this)') :
                element.setAttribute('onchange', 'counterpartFinancingChanged(this)');
            manageFinancingFieldsValidation(
                element.value,
                $(row).find('input[name=financingComponent]'));
        });
    }
}

function counterpartFinancingNewChanged(source) {
    var inputCounterpart = $(source);
    var row = inputCounterpart.closest('tr');
    var inputFinancing = row.find('input[name=financingComponent-new]');
    manageFinancingFieldsValidation(inputCounterpart.val(), inputFinancing);
}

function counterpartFinancingChanged(source) {
    var inputCounterpart = $(source);
    var row = inputCounterpart.closest('tr');
    var inputFinancing = row.find('input[name=financingComponent]');
    manageFinancingFieldsValidation(inputCounterpart.val(), inputFinancing);
}

function manageFinancingFieldsValidation(counterpartFinancingValue, financingComponent) {
    if (counterpartFinancingValue == "0.00") {
        financingComponent.attr ?
            financingComponent.attr('data-parsley-nonzero', 0) :
            financingComponent.setAttribute('data-parsley-nonzero', 0);
    }
    else {
        financingComponent.removeAttr ?
            financingComponent.removeAttr('data-parsley-nonzero') :
            financingComponent.removeAttribute('data-parsley-nonzero');
        cleanParsley(financingComponent.closest('span'));
        cleanParsley($('input[name=financing]').closest('div'));
    }
}

function cleanParsley(source) {
    if (source === undefined)
        return;

    var sectionParsley = source.find('ul.parsley-errors-list');

    if (sectionParsley.length > 0) {
        sectionParsley.removeClass("filled");
        sectionParsley.empty();
    }
}