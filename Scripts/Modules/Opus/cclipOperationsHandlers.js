var cclipOperationsData = new Object();

cclipOperationsData._IsCreationModule = null;
cclipOperationsData.MultipleSectorId = null;
cclipOperationsData.MultiSectorAttribName = null;
cclipOperationsData._MultipleSubSectorId = null;
cclipOperationsData._OperationNumber = null;
cclipOperationsData._NewOperationNumber = null;
cclipOperationsData._RelationshipTypeCclipSId = null;
cclipOperationsData._RelationshipTypeCclipRId = null;
cclipOperationsData._ExecAgenSectorDeletionWarning = null;
cclipOperationsData._ExecutingAgencyRoleId = null;
cclipOperationsData._FacilityTypeCclipSectorial = null;
cclipOperationsData._FacilityTypeCclipMultisectI = null;
cclipOperationsData._FacilityTypeCclipMultisectII = null;
cclipOperationsData._ConOperationId = null;
cclipOperationsData._FinancingYes = null;
cclipOperationsData._ConModalityCreditLineId = null;
cclipOperationsData._RelatedInstitutions = null;

cclipOperationsData.Init = function (
    isCreationModule,
    multipleSectorId,
    multiSectorAttribName,
    multipleSubSectorId,
    operationNumber,
    newOperationNumber,
    relationshipTypeCclipSId,
    relationshipTypeCclipRId,
    execAgenSectorDeletionWarning,
    executingAgencyRoleId,
    facilityTypeCclipSectorial,
    facilityTypeCclipMultisectI,
    facilityTypeCclipMultisectII,
    conOperationId,
    financingYes,
    conModalityCreditLineId,
    relatedInstitutions) {

    cclipOperationsData._IsCreationModule = isCreationModule;
    cclipOperationsData.MultipleSectorId = multipleSectorId;
    cclipOperationsData.MultiSectorAttribName = multiSectorAttribName;
    cclipOperationsData._MultipleSubSectorId = multipleSubSectorId;
    cclipOperationsData._OperationNumber = operationNumber;
    cclipOperationsData._NewOperationNumber = newOperationNumber;
    cclipOperationsData._RelationshipTypeCclipSId = relationshipTypeCclipSId;
    cclipOperationsData._RelationshipTypeCclipRId = relationshipTypeCclipRId;
    cclipOperationsData._ExecAgenSectorDeletionWarning = execAgenSectorDeletionWarning;
    cclipOperationsData._ExecutingAgencyRoleId = executingAgencyRoleId;
    cclipOperationsData._FacilityTypeCclipSectorial = facilityTypeCclipSectorial;
    cclipOperationsData._FacilityTypeCclipMultisectI = facilityTypeCclipMultisectI;
    cclipOperationsData._FacilityTypeCclipMultisectII = facilityTypeCclipMultisectII;
    cclipOperationsData._ConOperationId = conOperationId;
    cclipOperationsData._FinancingYes = financingYes;
    cclipOperationsData._ConModalityCreditLineId = conModalityCreditLineId;
    cclipOperationsData._RelatedInstitutions = relatedInstitutions;
}

cclipOperationsData._HasOperationAnyCclipRelationship = function () {
    var isNewOperationMode = cclipOperationsData._OperationNumber == cclipOperationsData._NewOperationNumber;
    var $relatedOperations = isNewOperationMode ?
        $('#relatedOperationsTable tbody tr').not('.template') :
        $('table[name=relationsTable] tbody tr');
    var isAnyRelationshipCclipType = false;

    if ($relatedOperations.length <= 0) {
        return isAnyRelationshipCclipType;
    }

    $relatedOperations.each(function(index, elem) {
        var elemRelatedRelationTypeId = $(elem).find('[name="relatedRelationTypeId"]').val();

        if (elemRelatedRelationTypeId == cclipOperationsData._RelationshipTypeCclipSId ||
            elemRelatedRelationTypeId == cclipOperationsData._RelationshipTypeCclipRId) {

            isAnyRelationshipCclipType = true;

            return false;
        }
    });

    return isAnyRelationshipCclipType;
}

cclipOperationsData._ShowCclipDeleteSectorExecAgencyInformationMessage = function () {
    showMessage(cclipOperationsData._ExecAgenSectorDeletionWarning);
}

cclipOperationsData.ValidateCclipExecAgencyGettingRemoved = function (source) {
    var gridAssociatedInstitutionsRoleId = source.parent().find('[name="associatedInstitutionsRole"]').val();
    var isExecAgencyGettingRemoved = gridAssociatedInstitutionsRoleId == cclipOperationsData._ExecutingAgencyRoleId;
    var isAnyRelationshipCclipType = cclipOperationsData._HasOperationAnyCclipRelationship();

    if (isExecAgencyGettingRemoved && isAnyRelationshipCclipType) {
        cclipOperationsData._ShowCclipDeleteSectorExecAgencyInformationMessage();

        return false;
    }

    return true;
}

cclipOperationsData.NeedMandatoryLiaison = function () {
    var facilityTypeId = $('#FacilityType').val();
    var isFacilityTypeCCLIPMultiSectII = cclipOperationsData._FacilityTypeCclipMultisectII == facilityTypeId;

    return cclipOperationsData._IsFinancialConOperation() && isFacilityTypeCCLIPMultiSectII;
}

cclipOperationsData.NeedMandatoryCreditLineExecutingAgency = function () {
    var facilityTypeId = $('#FacilityType').val();
    var needsUniqueExecutingAgency =
        [
            cclipOperationsData._FacilityTypeCclipSectorial,
            cclipOperationsData._FacilityTypeCclipMultisectI
        ].indexOf(facilityTypeId) >= 0;

    return cclipOperationsData._IsFinancialConOperation() && needsUniqueExecutingAgency;
}

cclipOperationsData.NeedMultipleSectors = function () {
    var facilityTypeId = $('#FacilityType').val();
    var isFacilityTypeCCLIPMultiSect =
        [
            cclipOperationsData._FacilityTypeCclipMultisectI,
            cclipOperationsData._FacilityTypeCclipMultisectII
        ].indexOf(facilityTypeId) >= 0;

    return cclipOperationsData._IsFinancialConOperation() && isFacilityTypeCCLIPMultiSect;
}

cclipOperationsData.IsCclipConOperation = function () {
    var facilityTypeId = $('#FacilityType').val();
    var isFacilityTypeCCLIP =
        [
            cclipOperationsData._FacilityTypeCclipSectorial,
            cclipOperationsData._FacilityTypeCclipMultisectI,
            cclipOperationsData._FacilityTypeCclipMultisectII
        ].indexOf(facilityTypeId) >= 0;

    return cclipOperationsData._IsFinancialConOperation() && isFacilityTypeCCLIP;
}

cclipOperationsData.IsRelationshipTypeCclip = function (relationshipTypeId) {
    var isRelationshipTypeCCLIP =
        [
            cclipOperationsData._RelationshipTypeCclipSId,
            cclipOperationsData._RelationshipTypeCclipRId
        ].indexOf(relationshipTypeId) >= 0;

    return isRelationshipTypeCCLIP;
}

cclipOperationsData.GetRelatedInstitutions = function () {
    if (!cclipOperationsData._IsCreationModule) {
        return JSON.parse(cclipOperationsData._RelatedInstitutions);
    }

    var $institutionsRows = $('#associatedInstitutionsTable tbody tr');

    if ($institutionsRows.length == 0) {
        return new Array();
    }

    var institutions = new Array();

    $institutionsRows.each(function () {
        var associatedInstitutionRoleId = $(this).find('[name="associatedInstitutionsRole"]').val();
        var associatedInstitutionId = $(this).find('[name="associatedInstitutions"]').val();
        var relatedInstitutions = {
            AssociatedInstitutionRoleId: associatedInstitutionRoleId,
            AssociatedInstitutionId: associatedInstitutionId };

        institutions.push(relatedInstitutions);
    });

    return institutions;
}

cclipOperationsData.UpdateRelatedInstitutions = function () {
    if ($(".tab-pane.active").attr("id") === 'tabResponsibilityData') {
        var $institutionsRows = $('#associatedInstitutionsTable tbody tr');

        if ($institutionsRows.length == 0) {
            cclipOperationsData._RelatedInstitutions = JSON.stringify(new Array());

            return true;
        }

        var institutions = new Array();

        $institutionsRows.each(function () {
            var associatedInstitutionRoleId = $(this).find('[name="associatedInstitutionsRole"]').val();
            var associatedInstitutionId = $(this).find('[name="associatedInstitutions"]').val();
            var relatedInstitutions = {
                AssociatedInstitutionRoleId: associatedInstitutionRoleId,
                AssociatedInstitutionId: associatedInstitutionId
            };

            institutions.push(relatedInstitutions);
        });

        cclipOperationsData._RelatedInstitutions = JSON.stringify(institutions);

        return true;
    }

    return false;
}

cclipOperationsData._IsFinancialConOperation = function () {
    var isOperationCon = cclipOperationsData._ConOperationId == $('input[name="operationType"]').val();
    var isFinancingYes =  cclipOperationsData._FinancingYes == $('[name="FinancingContainer"]:checked').val();
    var isCreditLineConModality = cclipOperationsData._ConModalityCreditLineId == $('[name="CONModality"]').val();

    return isOperationCon && isFinancingYes && isCreditLineConModality;
}

cclipOperationsData._DisableMultipleSectorsInputs = function () {
    var $multiSectorContainer = $('[name="' + cclipOperationsData.MultiSectorAttribName + '"]');
    $multiSectorContainer.val(null);
    $multiSectorContainer.attr("data-parsley-required", false);

    var $multiSectorsParentContainer = $multiSectorContainer.parent();
    $multiSectorsParentContainer.find('ul.chosen-choices > li.search-choice').remove();
    $multiSectorsParentContainer.find('ul.chosen-choices').addClass('disabled');
    $multiSectorsParentContainer.find('div.chosen-drop').hide();
    $multiSectorsParentContainer.find('input').prop('disabled', true);

    cclipOperationsData._ResetSectorSubSectorValueForCCLIP();

    $('#id-sector').prop('disabled', false);
    $("input[name='sector']").prop('disabled', false);
}

cclipOperationsData._EnableMultipleSectorsInputs = function () {
    var $sectorButton = $('#id-sector');
    var $sectorButtonContainer = $sectorButton.parent();
    $sectorButton.prop('disabled', true);
    $sectorButtonContainer.addClass('placeholder');
    $sectorButtonContainer.find("input[name='sector']").prop('disabled', true);

    var $subSectorButton = cclipOperationsData._IsCreationModule ? $('#id-subSector') : $('#id-subsector');
    var $subSectorButtonContainer = $subSectorButton.parent();
    $subSectorButton.prop('disabled', true);
    $subSectorButtonContainer.addClass('placeholder');

    if (cclipOperationsData._IsCreationModule) {
        $subSectorButtonContainer.find("input[name='subSector']").prop('disabled', true);
    } else {
        $subSectorButtonContainer.find("input[name='subsector']").prop('disabled', true);
    }

    var $multiSectorContainer = $('[name="' + cclipOperationsData.MultiSectorAttribName + '"]');
    var $multiSectorsParentContainer = $multiSectorContainer.parent();
    $multiSectorContainer.attr("data-parsley-required", true);
    $multiSectorsParentContainer.find('ul.chosen-choices').removeClass('disabled').addClass('cclipMultiSectorChosenChoices');
    $multiSectorsParentContainer.find('ul.chosen-results').addClass('cclipMultiSectorAvailableChoices');
    $multiSectorsParentContainer.find('div.chosen-drop').show();
    $multiSectorsParentContainer.find('input').prop('disabled', false);
}

cclipOperationsData._SetSectorValueForCCLIP = function () {
    var $sectorButton = $('#id-sector');
    var $sectorButtonContainer = $sectorButton.parent();
    var $multiSectorLink = $sectorButtonContainer.find('ul.dropdown-menu a[dd-value=' + cclipOperationsData.MultipleSectorId + ']');

    $sectorButtonContainer.find('ul a').removeAttr('dd-selected');
    $multiSectorLink.show();
    $multiSectorLink.attr('dd-selected', 'dd-selected');

    $sectorButtonContainer.find('input').attr('value', cclipOperationsData.MultipleSectorId);
    $sectorButtonContainer.find('input').val(cclipOperationsData.MultipleSectorId);
    $sectorButton.find('.valueText').text($multiSectorLink.text());

    loadSubSectorCombo();
}

cclipOperationsData.SetSubSectorValueForCCLIP = function () {
    var $sectorInput = $("input[name='sector']");

    if($sectorInput.attr('value') == cclipOperationsData.MultipleSectorId) {
        var $subSectorButton = cclipOperationsData._IsCreationModule ? $('#id-subSector') : $('#id-subsector');
        var $subSectorButtonContainer = $subSectorButton.parent();
        var $multiSubSectorLink = $subSectorButtonContainer.find('ul.dropdown-menu a[dd-value=' + cclipOperationsData._MultipleSubSectorId + ']');

        $subSectorButtonContainer.find('ul a').removeAttr('dd-selected');
        $multiSubSectorLink.show();
        $multiSubSectorLink.attr('dd-selected', 'dd-selected');

        $subSectorButtonContainer.find('input').attr('value', cclipOperationsData._MultipleSubSectorId);
        $subSectorButtonContainer.find('input').val(cclipOperationsData._MultipleSubSectorId);
        $subSectorButton.find('.valueText').text($multiSubSectorLink.text());
    }
}

cclipOperationsData._ResetSectorSubSectorValueForCCLIP = function () {
    var $sectorButton = $('#id-sector');
    var $sectorButtonContainer = $sectorButton.parent();
    var $firstOptionLink = $sectorButtonContainer.find('ul.dropdown-menu a:eq(0)');
    var $selectedSectorLink = $sectorButtonContainer.find('ul.dropdown-menu a[dd-selected]');
    var currentSectorValue = $selectedSectorLink.attr('dd-value');

    if(currentSectorValue == cclipOperationsData.MultipleSectorId) {
        $sectorButtonContainer.find('input').attr('value', $firstOptionLink.attr('dd-value'));
        $sectorButtonContainer.find('input').val($firstOptionLink.attr('dd-value'));
        $sectorButtonContainer.find('ul.dropdown-menu a[dd-selected]').removeAttr('dd-selected');
        $sectorButton.find('.valueText').text($firstOptionLink.text());

        cclipOperationsData.HideSectorSubSectorMultipleOption();
    }

    loadSubSectorCombo();
}

cclipOperationsData.UpdateSectorsSubsectorsInputs = function () {
    if(cclipOperationsData.NeedMultipleSectors()) {
        cclipOperationsData._SetSectorValueForCCLIP();
        cclipOperationsData._EnableMultipleSectorsInputs();

        return;
    }

    cclipOperationsData._DisableMultipleSectorsInputs();
}

cclipOperationsData.HideSectorSubSectorMultipleOption = function () {
    var $sectorButton = $('#id-sector');
    var $sectorButtonContainer = $sectorButton.parent();
    var $multiSectorLink = $sectorButtonContainer.find('ul.dropdown-menu a[dd-value=' + cclipOperationsData.MultipleSectorId + ']');

    $multiSectorLink.hide();
}

cclipOperationsData.ValidateMultiSectorModifiable = function () {
    if (cclipOperationsData._HasOperationAnyCclipRelationship()) {
        cclipOperationsData._ShowCclipDeleteSectorExecAgencyInformationMessage();
    }
};

cclipOperationsData.CheckFieldEditableByCclipConditions = function (target) {
    if (cclipOperationsData._HasOperationAnyCclipRelationship()) {
        cclipOperationsData._ShowCclipDeleteSectorExecAgencyInformationMessage();

        if (target !== null && $(target).is('button.dropdown-toggle')) {
            $(target).parent().removeClass('open');
        }

        return false;
    }

    return true;
};