function getInstitutionsForEfcOperations() {
    var $content = $("#newAscInstitutionModalContent");
    var $associatedInstitutionsInput =
        $content.find("[name='associatedInstitutionsFilter']").val("");
    var $associatedInstitutionsDropdown = $associatedInstitutionsInput.closest('div.dropdown');
    var $associatedInstitutionsDropdownList =
        $associatedInstitutionsDropdown.find('ul.dropdown-menu');
    var placeholder = $associatedInstitutionsDropdown.find('ul.dropdown-menu li:first a').text();

    $associatedInstitutionsDropdown.find('button').removeClass('hide');

    showLoader();

    $.ajax({
        url: $content.attr('data-get-associated-institution-url'),
        type: 'post',
        success: function (data) {

            $associatedInstitutionsDropdownList.empty();

            var $listPlaceHolder = $('<li />').appendTo($associatedInstitutionsDropdownList);
            var $linkPlaceHolder = $('<a />', { 'dd-value': '' })
                .appendTo($listPlaceHolder).html(placeholder);

            if (data.IsValid) {
                data.InstitutionList.forEach(function (item) {
                    var $listItem = $('<li />').appendTo($associatedInstitutionsDropdownList);
                    var $linkItem = $('<a />', { 'dd-value': item.Value })
                        .appendTo($listItem).html(item.Text);
                });
                hideLoader();
            }
        }
    })
}

$(document).on('change', '#associatedInstitutionsRoleFilter', function () {
    var $content = $("#newAscInstitutionModalContent");
    var $filterDiv = $content.find(".row.mt5.mb20.line");
    var $operationTextTag = isCreationForm() ?
        $('#id-operationType span.valueText') :
        $('#cntOpType > label');
    var operationCode = $operationTextTag.text().substring(0, 3);
    var executingAgencyRoleId =
        $("#newAscInstitutionModalContent").attr('data-executing-agency-role-id');

    if (this.value == executingAgencyRoleId && operationCode == "EFC") {
        $filterDiv.hide();
        getInstitutionsForEfcOperations();
        $content.find("[name='associatedInstitutionsFilter']").FirstorDefault();
        $("#id-associatedInstitutionsFilter").attr("disabled", false);
    } else {
        var $associatedInstitutionsDropdown = $("#newAscInstitutionModalContent")
            .find("[name='associatedInstitutionsFilter']").closest('div.dropdown');
        var $associatedInstitutionsDropdownList = $associatedInstitutionsDropdown
            .find('ul.dropdown-menu');
        var placeholder = $associatedInstitutionsDropdown
            .find('ul.dropdown-menu li:first a').text();

        $associatedInstitutionsDropdownList.empty();

        var $listPlaceHolder = $('<li />').appendTo($associatedInstitutionsDropdownList);
        var $linkPlaceHolder = $('<a />', { 'dd-value': '' })
            .appendTo($listPlaceHolder).html(placeholder);

        $filterDiv.show();
    }
});

var cleanerObject = (function (selector) {
    var $container = $(selector);

    var controls = function () {
        $container.find('input[name="LendingType"]').off('change');
        $container.find('input[type!="hidden"]')
            .not('#operationType, #RoleOrganizationalUnit, #operationTeamsRole, [name=userNameTeam_text], disabled')
            .FirstorDefault()
            .val('');
        $container.find('textarea').val('');
        $container.find('input[type!="hidden"]:checkbox').not('.hide').removeAttr('checked');
        $container.find('input[name="LendingType"]').on('change');
    };

    var organizationalUnit = function () {
        $container.find('[name=OrganizationalUnit_text]').click()
    };

    var associatedInstitutions = function () {
        $container.find('#associatedInstitutionsTable tbody tr').remove();
    };

    var relatedOperations = function () {
        $container.find('#relatedOperationsTable tbody tr').remove();
    };

    var financingData = function () {
        $container.find('#gridExpectedIDB tbody tr').remove();
    };

    var clearAll = function () {
        controls();
        organizationalUnit();
        associatedInstitutions();
        relatedOperations();
        financingData();
    };

    return {
        ClearAll: clearAll,
        OrganizationalUnit : organizationalUnit,
        AssociatedInstitutions: associatedInstitutions,
        FinancingData: financingData
    }
})('#PageContent');
