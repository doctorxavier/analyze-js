var publicPrivatePartnership = new Object();

publicPrivatePartnership._ConOperationId = null;
publicPrivatePartnership._FinancingYesId = null;
publicPrivatePartnership._PrgOperationId = null;
publicPrivatePartnership._PppUnavailablePrgModalityIds = null;
publicPrivatePartnership._IsPppAvailableOperation = null;

publicPrivatePartnership.Init = function (
    conOperationId,
    financingYesId,
    prgOperationId,
    pppUnavailablePrgModalityIds) {

    publicPrivatePartnership._ConOperationId = conOperationId;
    publicPrivatePartnership._FinancingYesId = financingYesId;
    publicPrivatePartnership._PrgOperationId = prgOperationId;
    publicPrivatePartnership._PppUnavailablePrgModalityIds = pppUnavailablePrgModalityIds;
    publicPrivatePartnership._IsPppAvailableOperation = false;
}

publicPrivatePartnership._TogglePublicPrivatePartnershipAttribute = function () {
    var $publicPrivatePartnershipAttr = $('input[name=PublicPrivatePartnershipAttr]');

    if (!publicPrivatePartnership._IsPppAvailableOperation) {
        $publicPrivatePartnershipAttr
            .closest('.LabelsGroup div[data-pagemode="edit"] input').prop('checked', false);
        $publicPrivatePartnershipAttr.closest('.LabelsGroup').addClass('hide');

        return;
    }

    $publicPrivatePartnershipAttr.closest('.LabelsGroup').removeClass('hide');
}

publicPrivatePartnership._CheckIsCurrentlyPppAvailableOperation = function () {

    if (publicPrivatePartnership._IsConOrPrgOperation()) {
        return publicPrivatePartnership._IsFinancialConOperation() ||
            publicPrivatePartnership._IsPppAvailablePrgOperation();
    }

    return true;
}

publicPrivatePartnership._IsConOrPrgOperation = function () {
    var isOperationCon =
        publicPrivatePartnership._ConOperationId == $('input[name="operationType"]').val();
    var isOperationPrg =
        publicPrivatePartnership._PrgOperationId == $('input[name="operationType"]').val();

    return isOperationCon || isOperationPrg;
}

publicPrivatePartnership._IsFinancialConOperation = function () {
    var isOperationCon =
        publicPrivatePartnership._ConOperationId == $('input[name="operationType"]').val();
    var isFinancingYes =
        publicPrivatePartnership._FinancingYesId == $('[name="FinancingContainer"]:checked').val();

    return isOperationCon && isFinancingYes;
}

publicPrivatePartnership._IsPppAvailablePrgOperation = function () {
    var isOperationPrg =
        publicPrivatePartnership._PrgOperationId == $('input[name="operationType"]').val();
    var prgModality = $('input[name="PRGModality"]').val();

    var isAvailablePrgModality = ($.isNumeric(prgModality) &&
            publicPrivatePartnership._PppUnavailablePrgModalityIds.indexOf(parseInt(prgModality)) < 0) ||
        prgModality == '';

    return isOperationPrg && isAvailablePrgModality;
}

publicPrivatePartnership.SetInitialPublicPrivatePartnershipAttributeVisibility = function () {
    publicPrivatePartnership._IsPppAvailableOperation =
        publicPrivatePartnership._CheckIsCurrentlyPppAvailableOperation();

    publicPrivatePartnership._TogglePublicPrivatePartnershipAttribute();
}

publicPrivatePartnership.UpdatePublicPrivatePartnershipAttributeVisibility = function () {
    publicPrivatePartnership._IsPppAvailableOperation =
        publicPrivatePartnership._CheckIsCurrentlyPppAvailableOperation();

    publicPrivatePartnership._TogglePublicPrivatePartnershipAttribute();
}