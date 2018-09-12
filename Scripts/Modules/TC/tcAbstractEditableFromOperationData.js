var tcAbstractEditableData = new Object();

tcAbstractEditableData._IsTcAbstractEditable = null;
tcAbstractEditableData._ExecutingAgencyRoleId = null;
tcAbstractEditableData._ResponsibleRoleId = null;
tcAbstractEditableData._DisbursRespUnitRoleId = null;

tcAbstractEditableData.Init = function (
    isEditableBasedOnTcAbstractState,
    executingAgencyRoleId,
    responsibleRoleId,
    disbursRespUnitRoleId) {

    tcAbstractEditableData._IsTcAbstractEditable = isEditableBasedOnTcAbstractState;
    tcAbstractEditableData._ExecutingAgencyRoleId = executingAgencyRoleId;
    tcAbstractEditableData._ResponsibleRoleId = responsibleRoleId;
    tcAbstractEditableData._DisbursRespUnitRoleId = disbursRespUnitRoleId;
}

tcAbstractEditableData.PrepareFieldsAccordingTCAbstractState = function () {
    tcAbstractEditableData._ToggleTcAbstractEditionInformationMessages();
    tcAbstractEditableData._ToggleTcAbstractEditionFieldsAssociatedInstitutions();
    tcAbstractEditableData.ToggleTcAbstractEditionFieldsResponsibilityUnits(false);
    tcAbstractEditableData.ToggleTcAbstractEditionFieldsCountryDepartment();
}

tcAbstractEditableData._ToggleTcAbstractEditionInformationMessages = function () {
    if(tcAbstractEditableData._IsTcAbstractEditable) {
        $('span.tc-abstract-not-editable-message.pull-left').addClass('hide');

        return;
    }

    $('span.tc-abstract-not-editable-message.pull-left').removeClass('hide');
}

tcAbstractEditableData._ToggleTcAbstractEditionFieldsAssociatedInstitutions = function () {
    $('#associatedInstitutionsTable tbody tr').each(function (index, elem) {
        var isExecutingAgencyRole = tcAbstractEditableData._ExecutingAgencyRoleId ==
            $(elem).find('[name="associatedInstitutionsRole"]').val();

        if (!tcAbstractEditableData._IsTcAbstractEditable && isExecutingAgencyRole) {
            $(elem).find('button.buttonTrash').addClass('hidden');
        } else {
            $(elem).find('button.buttonTrash').removeClass('hidden');
        }
    })
}

tcAbstractEditableData.ToggleTcAbstractEditionFieldsResponsibilityUnits = function (isNewRow) {
    var $responsibleUnitsRows = $('#responsibleUnitsTable tbody tr');

    $responsibleUnitsRows.each(function (index, elem) {
        var selectedRole = $(elem).find('input[name="responsibleUnitsRole"]').val();
        var isRoleProtected = tcAbstractEditableData._IsRoleProtectedByTcAbstractEditableStatus(selectedRole);

        if (!isNewRow && isRoleProtected) {
            $(elem).find('button').not('.buttonTrash')
                .prop('disabled', !tcAbstractEditableData._IsTcAbstractEditable);
            $(elem).find('input').not('[name="responsibleUnitsEffortInDays"]')
                .prop('disabled', !tcAbstractEditableData._IsTcAbstractEditable);

            if(tcAbstractEditableData._IsTcAbstractEditable) {
                $(elem).find('button.buttonTrash').removeClass('hidden');
            } else {
                $(elem).find('button.buttonTrash').addClass('hidden');
            }
        }

        if (isNewRow && index === ($responsibleUnitsRows.length - 1)) {
            tcAbstractEditableData.ToggleTcAbstractEditionFieldsOptionElements(
                $(this).find('[aria-labelledby="id-responsibleUnitsRole"] li'));
        }
    });
}

tcAbstractEditableData.ToggleTcAbstractEditionFieldsOptionElements = function (liElements) {
    liElements.each(function(index, elem2) {
        var $option = $(elem2).find('a');

        if (tcAbstractEditableData._IsRoleProtectedByTcAbstractEditableStatus($option.attr('dd-value')) &&
            !$option.is('[dd-selected]')) {

            if(tcAbstractEditableData._IsTcAbstractEditable) {
                $(elem2).removeClass('hidden');
            } else {
                $(elem2).addClass('hidden');
            }
        }
    });
}

tcAbstractEditableData._IsRoleProtectedByTcAbstractEditableStatus = function (roleId) {
    var rolesToCompare = [
        tcAbstractEditableData._ResponsibleRoleId,
        tcAbstractEditableData._DisbursRespUnitRoleId,
        tcAbstractEditableData._ExecutingAgencyRoleId];

    return rolesToCompare.indexOf(roleId) >= 0;
}

tcAbstractEditableData.ToggleTcAbstractEditionFieldsCountryDepartment = function () {
    var $countryDepartment = $('[name="countryDepartment"]');
    var $countryDepartmentButton = $countryDepartment.find('button');
    var $countryDepartmentContainer = $countryDepartment.parent();

    $countryDepartment.find('input').prop('disabled', !tcAbstractEditableData._IsTcAbstractEditable);
    $countryDepartmentButton.prop('disabled', !tcAbstractEditableData._IsTcAbstractEditable);

    if (!$countryDepartmentButton.is(':visible')) {
        $countryDepartmentContainer.find('.tc-abstract-not-editable-message').addClass('hide');
    }

    if (tcAbstractEditableData._IsTcAbstractEditable) {
        $countryDepartmentContainer.find('label').removeClass('opdata-cell-like');
    } else {
        $countryDepartmentContainer.find('label').addClass('opdata-cell-like');
    }
}