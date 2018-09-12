var mifOperationalProgUnit = new Object();

mifOperationalProgUnit._ConOperNsgCategorizations = null;
mifOperationalProgUnit._MifOpuOperNsgCategorizations = null;
mifOperationalProgUnit._MifOpuOperations = null;
mifOperationalProgUnit._LendingTypeNsgId = null;
mifOperationalProgUnit._ConOperationTypeId = null;
mifOperationalProgUnit._ResponsibleRoleId = null;
mifOperationalProgUnit._IsMifOpuOperation = null;
mifOperationalProgUnit._WasMifOpuOperation = null;

mifOperationalProgUnit.Init = function (
    conOperNsgCategorizations,
    mifOpuOperNsgCategorizations,
    mifOpuOperations,
    lendingTypeNsgId,
    conOperationTypeId,
    responsibleRoleId) {

    mifOperationalProgUnit._ConOperNsgCategorizations = conOperNsgCategorizations;
    mifOperationalProgUnit._MifOpuOperNsgCategorizations = mifOpuOperNsgCategorizations;
    mifOperationalProgUnit._MifOpuOperations = mifOpuOperations;
    mifOperationalProgUnit._LendingTypeNsgId = lendingTypeNsgId;
    mifOperationalProgUnit._ConOperationTypeId = conOperationTypeId;
    mifOperationalProgUnit._ResponsibleRoleId = responsibleRoleId;
    mifOperationalProgUnit._IsMifOpuOperation = false;
    mifOperationalProgUnit._WasMifOpuOperation = false;
}

mifOperationalProgUnit.CheckIsCurrentlyMifOpuOperation = function () {

    var operTypeId = $('input[name="operationType"]').val();
    var guaranteedId = operTypeId == mifOperationalProgUnit._ConOperationTypeId ?
        '' : $('[name="LendingType"]').val();
    var nsgCategorizationId = operTypeId == mifOperationalProgUnit._ConOperationTypeId ?
        $('[name="MIFNSGInst"]').val() : $('[name="Intruments"]').val();

    if ((mifOperationalProgUnit._MifOpuOperations.indexOf(operTypeId) >= 0 &&
            guaranteedId == mifOperationalProgUnit._LendingTypeNsgId &&
            mifOperationalProgUnit._MifOpuOperNsgCategorizations.indexOf(nsgCategorizationId) >= 0) ||
        (operTypeId == mifOperationalProgUnit._ConOperationTypeId &&
            mifOperationalProgUnit._ConOperNsgCategorizations.indexOf(nsgCategorizationId) >= 0)) {

        return true;
    }

    return false;
}

mifOperationalProgUnit.SetInitialMifOpuOperationStatus = function () {
    mifOperationalProgUnit._IsMifOpuOperation = mifOperationalProgUnit.CheckIsCurrentlyMifOpuOperation();
    mifOperationalProgUnit._WasMifOpuOperation = mifOperationalProgUnit._IsMifOpuOperation;
}

mifOperationalProgUnit.UpdateMifOpuOperationData = function () {
    mifOperationalProgUnit._WasMifOpuOperation = mifOperationalProgUnit._IsMifOpuOperation;
    mifOperationalProgUnit._IsMifOpuOperation = mifOperationalProgUnit.CheckIsCurrentlyMifOpuOperation();

    if (mifOperationalProgUnit._CheckWasMifOpuOperation()) {
        mifOperationalProgUnit._ClearFundsAndResponsible();
    }
}

mifOperationalProgUnit._CheckWasMifOpuOperation = function () {
    return mifOperationalProgUnit._WasMifOpuOperation && !mifOperationalProgUnit._IsMifOpuOperation;
}

mifOperationalProgUnit._ClearFundsAndResponsible = function () {
    $('#gridExpectedIDB tbody tr').each(function (index, elem) {
        removeRowExpectedIDBWithoutConfirmation($(elem).find('button.buttonTrash'));
    });

    $('#organizationalUnitTable tbody tr').each(function (index, elem) {
        var $elem = $(elem);
        var selectedRoleId = $elem.find('input[name="RoleOrganizationalUnit"]').val();

        if (mifOperationalProgUnit._ResponsibleRoleId == selectedRoleId) {
            $elem.find('input[name="OrganizationalUnit_text"]').click();
        }
    });
}