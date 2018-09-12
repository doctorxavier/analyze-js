var RMCDN = new Object();

RMCDN.OperationNumber = null;
RMCDN.OperationIsCND = null;
RMCDN.Texts = new Object();
RMCDN.URLs = new Object();
RMCDN.Data = new Object();
RMCDN.Nodes = new Object();
RMCDN.Beneficiaries = [];
RMCDN.ItemThemes = [];
RMCDN.ItemUnits = [];
RMCDN.ItemOperations = [];
RMCDN.ItemActivityGroups = [];
RMCDN.ItemTypeSupports = [];
RMCDN.ItemMBF = [];

RMCDN.ComponentButtonsInitialized = false;

RMCDN._ComponentsInitialized = false;

RMCDN._ReadOperations = function () {

    $.ajax({
        url: RMCDN.Nodes.$itemModal.attr('data-operations-url'),
        type: "post",
        async: false,
        success: function (data) {
            RMCDN.ItemOperationsOriginal = data['Models'];
            RMCDN.ItemOperations = $.map(data['Models'], function (item) {
                return {
                    'text': item['OperationNumber'] + ' - ' + item['OperationName'],
                    'id': item['OperationId'],
                    'item': item
                }

            });
        }
    });
};

RMCDN._AddSupportType = function (supportType) {

    var exists = false;

    $.each(RMCDN.ItemTypeSupports, function (index, existingSupportType) {
        if (supportType['id'] == existingSupportType['id']) {
            exists = true;
            return false;
        }
    });

    if (!exists) {
        RMCDN.ItemTypeSupports.push(supportType);
    }
};

RMCDN._ReadActivityGroups = function () {

    $.ajax({
        url: RMCDN.Nodes.$itemModal.attr('data-activity-url'),
        type: "post",
        async: false,
        success: function (data) {

            RMCDN.ItemActivityGroupOriginal = [];
            RMCDN.ItemSupportTypesOriginal = [];

            RMCDN.ItemActivityGroups = $.map(data['Models'], function (item) {

                var activityGroup = item['ActivityGroup'];
                var supportTypes = item['SupportTypes'];

                RMCDN.ItemActivityGroupOriginal.push(activityGroup);

                var supportTypesIds = [];

                $.each(supportTypes, function (index, supportTypeItem) {

                    supportTypesIds.push(supportTypeItem['Code']);

                    RMCDN.ItemSupportTypesOriginal.push(supportTypeItem);

                    RMCDN._AddSupportType({
                        'id': supportTypeItem['Code'],
                        'text': supportTypeItem[RMCDN.TextMasterDataKey]
                    });

                });

                return {
                    'text': activityGroup[RMCDN.TextMasterDataKey].substr(0, 90) + '...',
                    'id': activityGroup['Code'],
                    'desc': activityGroup[RMCDN.TextMasterDataKey],
                    'type_supports': supportTypesIds
                }

            });
        }
    });
};

RMCDN._ReadUnits = function () {

    $.ajax({
        url: RMCDN.Nodes.$itemModal.attr('data-units-url'),
        type: "post",
        async: false,
        success: function (data) {
            RMCDN.ItemUnitsOriginal = data['Models'];
            RMCDN.ItemUnits = $.map(data['Models'], function (item) {
                return {
                    'text': item["NameEn"],
                    'id': item['Code']
                }

            });
        }
    });
};

RMCDN._ReadThemes = function () {

    $.ajax({
        url: RMCDN.Nodes.$itemModal.attr('data-themes-url'),
        type: "post",
        async: false,
        success: function (data) {
            RMCDN.ItemThemesOriginal = data['Models'];
            $.each(data['Models'], function (index, item) {

                if (item['NameEn'].toUpperCase() != "Other") {
                    RMCDN.ItemThemes.push({
                        'text': item[RMCDN.TextMasterDataKey],
                        'id': item['Code']
                    });
                }
            });
        }
    });
};

RMCDN._ReadTexts = function (node) {

    var texts = {};

    $.each($(node).get(0).attributes, function (index, attr) {
        var key = String(attr.name);
        var value = String(attr.value);
        var match;
        if (match = key.match(/^data\-text\-/)) {
            texts[key.replace(/^data\-text\-/g, '')] = value;
        }
    });

    return texts;

};

RMCDN._AssignNodes = function () {
    RMCDN.Nodes.$modal = $('#CND-modal');
    RMCDN.Nodes.$itemModal = $('#CND-item-modal');
    RMCDN.Nodes.$form = $('#CND-form');
    RMCDN.Nodes.$itemForm = $('#CND-item-form');
    RMCDN.Nodes.$selectBeneficiaries = $('#CND-beneficiaries');
    RMCDN.Nodes.$blockBeneficiaries = $('#CND-beneficiaries-block');
    RMCDN.Nodes.$selectPointContact = $('#CND-pointContact');
    RMCDN.Nodes.$selectSpecialist = $('#CND-specialist');
    RMCDN.Nodes.$errorBlock = RMCDN.Nodes.$form.find('.errors.alert');
    RMCDN.Nodes.$itemErrorBlock = RMCDN.Nodes.$itemModal.find('.errors.alert');
    RMCDN.Nodes.$selectItemOperations = $('#CND-item-operation');
    RMCDN.Nodes.$selectMBFCode = $('#CND-item-mbf');

    RMCDN.Nodes.$itemModal.on('shown.bs.modal', RMCDN.ResizeItemModal);

};

RMCDN._ReadBeneficiaries = function () {

    $.ajax({
        url: RMCDN.URLs['beneficiaries'],
        type: "post",
        async: false,
        success: function (data) {
            RMCDN.Data.Beneficiaries = data;
        },
        error: function () {
            alert('Corrupted Data Operation');
        }

    });
};

RMCDN._ReadAvailableYearPlans = function () {

    $.ajax({
        url: RMCDN.URLs['available-plan-years'],
        type: "post",
        success: function (data) {
            RMCDN.Data.AvailableYears = data;
        }
    });

};

RMCDN._ReadIsCNDOperation = function () {

    if (RMCDN.OperationIsCND !== null) {
        return;
    }

    $.ajax({
        url: RMCDN.URLs['is-cnd'],
        type: "post",
        async: false,
        success: function (data) {
            RMCDN.OperationIsCND = JSON.parse(data);
        }
    });
};

RMCDN._ReadData = function () {

    RMCDN.OperationNumber = RMCDN.Nodes.$modal.attr('data-operation-number');

    RMCDN.UsersSearchUrl = RMCDN.Nodes.$modal.attr('data-users-search-url');
    RMCDN.UsersSearchParam = RMCDN.Nodes.$modal.attr('data-users-search-param');
    RMCDN.Texts.Delete = RMCDN.Nodes.$modal.attr('data-text-delete');
    RMCDN.Texts.DateValidation = RMCDN.Nodes.$modal.attr('data-text-date-validation');
    RMCDN.Texts.UnkownError = RMCDN.Nodes.$modal.attr('data-text-unknown-error');

    RMCDN.CurrentLanguage = RMCDN.Nodes.$itemModal.attr('data-current-language');

    switch (RMCDN.CurrentLanguage.toUpperCase()) {
        case 'EN':
            RMCDN.TextMasterDataKey = "NameEn";
            break;
        case 'ES':
            RMCDN.TextMasterDataKey = "NameEs";
            break;
        case 'PT':
            RMCDN.TextMasterDataKey = "NamePt";
            break;
        case 'FR':
            RMCDN.TextMasterDataKey = "NameFr";
            break;

    }

    RMCDN.URLs = new Object();
    RMCDN.URLs['beneficiaries'] = RMCDN.Nodes.$modal.attr('data-url-list-beneficiaries');
    RMCDN.URLs['available-plan-years'] = RMCDN.Nodes.$modal.attr('data-url-list-available-plan-years');
    RMCDN.URLs['is-cnd'] = RMCDN.Nodes.$modal.attr('data-url-is-cnd');

    RMCDN._ReadIsCNDOperation();

    if (RMCDN.OperationIsCND) {
        RMCDN._ReadBeneficiaries();
        RMCDN.Texts.Items = RMCDN._ReadTexts(RMCDN.Nodes.$itemModal);
        RMCDN._ReadThemes();
        RMCDN._ReadUnits();
        RMCDN._ReadActivityGroups();
        RMCDN._ReadOperations();
    }
};

RMCDN._UpdateBeneficiariesBlock = function () {

    if (RMCDN.Beneficiaries.length == 0) {

        $('#CND-beneficiaries-block .empty-block').show();
        $('#CND-beneficiaries-table').hide();

    } else {

        $('#CND-beneficiaries-block .empty-block').hide();
        $('#CND-beneficiaries-table').show();
        $('#CND-beneficiaries-tbody').empty();

        $.each(RMCDN.Beneficiaries, function (index, lmsRequestNumber) {

            var item = RMCDN._GetBeneficiaryFromLMSRequestNumber(lmsRequestNumber);
            var $row = $('<tr />').appendTo('#CND-beneficiaries-tbody');
            $row.attr({ 'data-beneficiary-lms-request-number': lmsRequestNumber });

            var $deleteLink = $('<a />').addClass('delete-icon pull-right').attr({ 'href': 'javascript:void(0)', 'title': RMCDN.Texts.Delete });
            $deleteLink.html('<span class="fa  fa-trash-o fa-2x"></span>');
            $deleteLink.on('click', RMCDN._DeleteBeneficiary);

            $row.append($('<td />').html(item['RequestNumber']));
            $row.append($('<td />').html(item['LMSRequestNumber']));
            $row.append($('<td />').html(item['TransactionType']));
            $row.append($('<td />').html('$ ' + item['Amount']));
            $row.append($('<td />').html(item['Name']));
            $row.append($('<td />').append($deleteLink).addClass('text-center'));
        });
    }
};

RMCDN._DeleteBeneficiary = function (evt_id) {

    var id;

    if (evt_id && typeof evt_id == 'object') {

        var $this = $(this);

        if ($this.attr('data-beneficiary-lms-request-number')) {
            id = $this.attr('data-beneficiary-lms-request-number');
        } else {
            id = $this.closest('[data-beneficiary-lms-request-number]').attr('data-beneficiary-lms-request-number');
        }

        return RMCDN._DeleteBeneficiary(id);

    } else {

        id = evt_id;
    }

    RMCDN.Beneficiaries = $.grep(RMCDN.Beneficiaries, function (item) {
        return item != id;
    })


    RMCDN._UpdateBeneficiariesBlock();
    RMCDN._InitBeneficiariesSelect();
};

RMCDN._GetBeneficiaryFromId = function (id) {
    var beneficiary = null;
    $.each(RMCDN.Data.Beneficiaries, function (index, item) {
        if (item['Id'] == id) {
            beneficiary = item;
            return false;
        }
    });

    return beneficiary;
};

RMCDN._GetBeneficiaryFromLMSRequestNumber = function (lmsRequestNumber) {
    var beneficiary = null;
    $.each(RMCDN.Data.Beneficiaries, function (index, item) {
        if (item['LMSRequestNumber'] == lmsRequestNumber) {
            beneficiary = item;
            return false;
        }
    });

    return beneficiary;
};

RMCDN._HasBeneficiary = function (testRequestNumber) {

    var exists = false;

    $.each(RMCDN.Beneficiaries, function (index, LMSRequestNumber) {
        if (testRequestNumber == LMSRequestNumber) {
            exists = true;
            return false;
        }
    });

    return exists;
};

RMCDN._GetBeneficiaryText = function (item) {
    return [item['RequestNumber'], item['LMSRequestNumber'], item['TransactionType'], '$ ' + item['Amount'], item['Name']].join(' | ');
};

RMCDN._ClearBeneficiariesSelect = function () {
    RMCDN.Nodes.$selectBeneficiaries.get(0).selectedIndex = -1;
    RMCDN.Nodes.$selectBeneficiaries.triggerHandler('change.select2');
};

RMCDN._ClearItemOperationsSelect = function () {
    RMCDN.Nodes.$selectItemOperations.get(0).selectedIndex = -1;
    RMCDN.Nodes.$selectItemOperations.triggerHandler('change.select2');
    RMCDN.Nodes.$selectItemOperations.triggerHandler('change');

};

RMCDN._ClearMBFCodeSelect = function () {
    RMCDN.Nodes.$selectMBFCode.empty();
};

RMCDN._InitUserComponent = function (node) {

    node.select2({
        allowClear: true,
        width: '100%',
        minimumInputLength: 3,
        placeholder: $('label[for=' + node.attr('id') + ']').text(),
        escapeMarkup: function (text) { return text; },
        ajax: {
            url: RMCDN.UsersSearchUrl,
            type: 'post',
            data: function (params, page) {
                unbindDocumentAjaxActions();
                var search = {};
                search[RMCDN.UsersSearchParam] = params.term;
                return search;
            },
            processResults: function (data, params) {
                bindDocumentAjaxActions();

                var items = $.map(data['ListResponse'], function (item) {

                    var imgUrl = item['PathTemplate'];

                    var select2Item = {
                        id: item['Value'],
                        text: '<span class="user-item"><img src="' + imgUrl + '" /><span class="text">' + item['Text'] + '</span></span>'
                    };

                    return select2Item;
                });

                return { results: items };
            },
        }
    });
};

RMCDN._InitBeneficiariesSelect = function (destroy) {

    RMCDN.Nodes.$selectBeneficiaries.empty();

    if (destroy) {
        RMCDN.Nodes.$selectBeneficiaries.select2('destroy');
    }

    var beneficiaries = $.grep(RMCDN.Data.Beneficiaries, function (item) {
        return !RMCDN._HasBeneficiary(item['Id']);
    });

    RMCDN.Nodes.$selectBeneficiaries.select2({
        allowClear: true,
        width: '100%',
        placeholder: $('label[for=' + RMCDN.Nodes.$selectBeneficiaries.attr('id') + ']').text(),
        data: $.map(beneficiaries, function (item, index) {
            return {
                id: item['LMSRequestNumber'],
                text: RMCDN._GetBeneficiaryText(item),
            }
        })
    });

    RMCDN.ResizeModal();

    RMCDN._ClearBeneficiariesSelect();
};

RMCDN._InitItemOperationsSelect = function () {

    RMCDN.Nodes.$selectItemOperations.on('change', RMCDN.ChangeOperation);

    RMCDN.Nodes.$selectItemOperations.on('select2:unselecting', RMCDN._ClearMBFCodeSelect);

    RMCDN.Nodes.$selectItemOperations.select2({
        allowClear: true,
        minimumInputLength: 3,
        width: '100%',
        placeholder: $('label[for=' + RMCDN.Nodes.$selectItemOperations.attr('id') + ']').text(),
        data: RMCDN.ItemOperations
    });

    RMCDN.ResizeItemModal();

    RMCDN._ClearItemOperationsSelect();
};

RMCDN._InitComponentButtons = function () {

    if (!RMCDN.ComponentButtonsInitialized) {

        RMCDN.ComponentButtonsInitialized = true;

        if (RMCDN.OperationIsCND) {

            $('#tabPhysicalProgress .row-milestone').prop('readonly', true);
            $('#tabPhysicalProgress .row-disaggregation').prop('readonly', true);
            $('.unit-of-measure').prop('readonly', true);

            window.cancel = function () {
                location.reload();
            }

            $('#tabPhysicalProgress .buttonTrash').each(function () {

                var $this = $(this);
                var title = $this.attr('title');

                if (title && title.match(/(\(\w{2}\)\s*|indicator)$/i)) {
                    $this.hide();
                }
            });

            $('.outputIndicator').text(RMCDN.Nodes.$modal.attr('data-text-item'));
            $(' .outputFinancial').text(RMCDN.Nodes.$modal.attr('data-text-total-amount'));
            $('.financialOutput .outputFinancial').text(RMCDN.Nodes.$modal.attr('data-text-item'));

            $('.outputPPaA').off('click').empty().css('cursor', 'default');
            $('#tabFinancialProgress .addNewRow').hide();

            $('#TCMAddComponentContainer').hide();
            $('#TCMCNDAddComponentContainer').show();

            $('#id-header_YearPlans').prop('disabled', true).css({ 'color': '#FFF', 'border': 0, 'background': 'none !important' });
            $('.spanYear').css('visibility', 'hidden');

            $('.original-value-row input').prop('disabled', true);
            $('.annual-value-row input').prop('disabled', true);
            $('.actual-value-row input').prop('disabled', true);

            $('[dd-tab="#tabMappingProgress"]').hide();
            $('#tabPhysicalProgress .row-milestone').hide();
            $('#tabPhysicalProgress .row-disaggregation').hide();
            $('#btn-read-show-output-description').hide();
            $('#btn-read-show-output-description').hide();
            $('#tabPhysicalProgress .buttonTrash.btn-alternative.hovered.ng-scope').hide();

            var $wOutcomesStatement = $('#tabPhysicalProgress .wOutcomesStatement');
            var $detailTitlesCells = $('[id^=tableTreeRowTitle].tablePrincipal thead .tcmTitleTable');

            $wOutcomesStatement.find('a').remove();
            $wOutcomesStatement.find('input').each(function (index, node) {

                var $this = $(this).hide();
                var val = $this.val();
                var componentId = $this.closest('span[data-component-id]').attr('data-component-id');

                $('<a />').attr({ 'href': 'javascript:void(0)' }).insertAfter($this).html(val).css({ 'line-height': '35px', 'margin-left': '20px' }).on('click', function () {
                    RMCDN.OpenModal(componentId);
                });

                $detailTitlesCells.eq(index).find('span').eq(1).html(
                    $('<a />').
                        attr({ 'href': 'javascript:void(0)' }).
                        insertAfter($this).
                        html(val).
                        css({ 'line-height': '35px', 'margin-left': '20px' }).
                        on('click', function () {
                            RMCDN.OpenModal(componentId, true);
                        })
                );

                if (componentId) {

                    var $itemButton = $('#tabPhysicalProgress .text-center .addNewRow:not(.item)').eq(index).hide();

                    var $newItemButton = $('<button />').insertAfter($itemButton).addClass('addNewRow item');
                    $newItemButton.on('click', function () {
                        RMCDN.OpenItemDialog(componentId);
                    });
                    $newItemButton.html("<span>+</span><span>New Item</span><span>+</span>");
                }

            });

            $('#tabFinancialProgress a[href*=OutputIndicator]:not(.cnd)').each(function (index) {

                var $link = $(this).addClass('cnd');
                var href = $link.attr('href');

                var $table = $link.closest('table');
                var componentIdMatch = $table.attr('id').match(/(\d+)$/i);

                var outputIndicatorIdMatch = href.match(/OutputIndicatorDetail\/(\d+)/i);

                console.log(outputIndicatorIdMatch);

                if (outputIndicatorIdMatch) {

                    var outputIndicatorDetailId = outputIndicatorIdMatch[1];
                    var componentId = componentIdMatch[1];

                    $link.attr({
                        'ng-href': '',
                        'href': 'javascript:void(0)'
                    }).on('click', function (evt) {

                        if ($('[name=btn-save-outputs]').is(':visible')) {
                            RMCDN.OpenItemDialog(componentId, outputIndicatorDetailId);
                        } else {
                            RMCDN.OpenItemDialogReadOnly(componentId, outputIndicatorDetailId);
                        }

                    });
                }
            });

            var $editTextAreas = $('#tabPhysicalProgress #components .inputTextarea.existing.pl30.ng-not-empty:not(.ng-hide)');

            $('#tabPhysicalProgress a[href*=OutputIndicator][ng-href]:not(.cnd)').each(function (index) {

                var $link = $(this).addClass('cnd');
                var href = $link.attr('href');

                var $table = $link.closest('table');
                var componentIdMatch = $table.attr('id').match(/(\d+)$/i);

                var outputIndicatorIdMatch = href.match(/OutputIndicatorDetail\?id\=(\d+)/i);

                if (outputIndicatorIdMatch) {

                    var outputIndicatorDetailId = outputIndicatorIdMatch[1];
                    var componentId = componentIdMatch[1];

                    var $textAreaLink = $link.clone(true);
                    var $textAreaEdit = $editTextAreas.eq(index);
                    $textAreaEdit.hide();
                    $textAreaLink.insertAfter($editTextAreas.eq(index));

                    $link.attr({
                        'ng-href': '',
                        'href': 'javascript:RMCDN.OpenItemDialogReadOnly(' + componentId + ', ' + outputIndicatorDetailId + ')'
                    });

                    $textAreaLink.attr({
                        'ng-href': '',
                        'href': 'javascript:RMCDN.OpenItemDialog(' + componentId + ', ' + outputIndicatorDetailId + ')'
                    }).css({
                        'position': 'relative',
                        'top': '-20px'
                    });
                }
            });

        }

    }
};

RMCDN._InitComponents = function () {

    if (RMCDN._ComponentsInitialized) {
        return;
    }

    RMCDN._ComponentsInitialized = true;

    RMCDN._InitBeneficiariesSelect(false);
    RMCDN._InitItemOperationsSelect(false);

    RMCDN.Nodes.$selectBeneficiaries.on('change', function (evt) {

        var lmsRequestNumber = RMCDN.Nodes.$selectBeneficiaries.val();
        RMCDN._ClearBeneficiariesSelect();
        RMCDN.AddBeneficiary(lmsRequestNumber);
    });

    RMCDN._UpdateBeneficiariesBlock();

    RMCDN._InitUserComponent(RMCDN.Nodes.$selectPointContact);
    RMCDN._InitUserComponent(RMCDN.Nodes.$selectSpecialist);

    $('#CND-plannedEndDate').datepicker("destroy");
    $('#CND-plannedEndDate').datepicker({
        dateFormat: "dd M yy",
        onSelect: function (date) {
            $('#CND-plannedEndDate').data('date', date);
        }
    });

    $('#CND-plannedStartDate').datepicker("destroy");
    $('#CND-plannedStartDate').datepicker({
        dateFormat: "dd M yy",
        onSelect: function (date) {
            $('#CND-plannedStartDate').data('date', date);
        }
    });
};

RMCDN._ReadItemForm = function () {

    var vals = {};

    vals['desc'] = $('#CND-item-desc').val();
    vals['id_activity_group'] = $('[name=activity-group]:checked').val();
    vals['id_support'] = $('#CND-item-support').val();
    vals['id_theme'] = $('#CND-item-theme').val();
    vals['ezreference'] = $('#CND-item-reference').val();
    vals['amount'] = String($('#CND-item-amount').val()).replace(/[^0-9\.]/g, '');
    vals['units'] = $('#CND-item-units').val().replace(/[^0-9]/g, '');
    vals['id_unit'] = $('#CND-item-unit').val();
    vals['id_mbf'] = $('#CND-item-mbf').val();
    vals['id_operation'] = $('#CND-item-operation').val();
    vals['comments'] = $('#CND-item-comment').val();
    vals['id_component'] = $('[name=id_component]').val();
    vals['id_output'] = $('[name=id_output]').val();
    vals['id'] = $('[name=id_cnd]').val();
    vals['disabled'] = $('#CND-item-disabled').is(':checked');

    return vals;
};

RMCDN._TransformItemSaveModel = function (item) {

    var model = new Object();
    model['Id'] = item['id'];
    model['Definition'] = item['desc'];
    model['ComponentId'] = item['id_component'];
    model['OutputId'] = item['id_output'];
    model['MbfCode'] = item['id_mbf'];
    model['NumberOfUnits'] = item['units'];
    model['EzShareReference'] = item['ezreference'];
    model['Amount'] = item['amount'];
    model['Comments'] = item['comments'];
    model['IsDeactivated'] = item['disabled'] ? 'true' : 'false';

    $.each(RMCDN.ItemSupportTypesOriginal, function (index, obj) {
        if (obj['Code'] == item['id_support']) {
            model['SupportType'] = obj;
        }
    });

    $.each(RMCDN.ItemActivityGroupOriginal, function (index, obj) {
        if (obj['Code'] == item['id_activity_group']) {
            model['ActivityGroup'] = obj;
        }
    });

    $.each(RMCDN.ItemThemesOriginal, function (index, obj) {
        if (obj['Code'] == item['id_theme']) {
            model['Theme'] = obj;
        }
    });

    $.each(RMCDN.ItemOperationsOriginal, function (index, obj) {
        if (obj['OperationId'] == item['id_operation']) {
            model['RelatedOperation'] = obj;
        }
    });

    model['UnitOfMeasure'] = $('#CND-item-unit option:selected').text();

    return model;
};

RMCDN._LoadItem = function (model) {

    RMCDN.Nodes.$itemModal.find('[name=id_cnd]').val(model['Id']);
    $('#CND-item-desc').val(model['Definition']);
    $('[name=activity-group][value=' + model['ActivityGroup']['Code'] + ']').prop('checked', true).trigger('click');
    $('#CND-item-theme').val(model['Theme']['Code']);
    $('#CND-item-support').val(model['SupportType']['Code']);
    $('#CND-item-reference').val(model['EzShareReference']);
    $('#CND-item-amount').val(model['Amount']).trigger('blur');
    $('#CND-item-units').val(model['NumberOfUnits']).trigger('blur');
    $('#CND-item-operation').val(model['RelatedOperation']['OperationId']);
    $('#CND-item-operation').triggerHandler('change.select2');
    $('#CND-item-operation').triggerHandler('change');
    $('#CND-item-comment').val(model['Comments']);
    $('#CND-item-disabled').prop('checked', model['IsDeactivated']);
    $('#CND-item-mbf').val(model['MbfCode']);

    $.each(RMCDN.ItemUnitsOriginal, function (index, item) {
        if (item["NameEn"] == model['UnitOfMeasure']) {
            $('#CND-item-unit').val(item['Code']);
            return;
        }
    });
};

RMCDN._DisableComponentDialog = function (disable) {
    if (disable) {
        RMCDN.Nodes.$modal.find('input').prop('disabled', true);
        RMCDN.Nodes.$modal.find('textarea').prop('disabled', true);
        RMCDN.Nodes.$modal.find('select').prop('disabled', true);
        RMCDN.Nodes.$modal.find('.fa.fa-trash-o.fa-2x').css('visibility', 'hidden');
        RMCDN.Nodes.$modal.find('.delete-icon').hide();
        RMCDN.Nodes.$modal.find('.buttons.buttons-enable').hide();
        RMCDN.Nodes.$modal.find('.buttons.buttons-disable').show();
    } else {
        RMCDN.Nodes.$modal.find('select')
        RMCDN.Nodes.$modal.find('input').prop('disabled', false);
        RMCDN.Nodes.$modal.find('textarea').prop('disabled', false);
        RMCDN.Nodes.$modal.find('select').prop('disabled', false);
        RMCDN.Nodes.$modal.find('.fa.fa-trash-o.fa-2x').css('visibility', 'visible');
        RMCDN.Nodes.$modal.find('.delete-icon').show();
        RMCDN.Nodes.$modal.find('.buttons.buttons-enable').show();
        RMCDN.Nodes.$modal.find('.buttons.buttons-disable').hide();
    }
};

RMCDN.ChangeOperation = function () {

    var selectedData = RMCDN.Nodes.$selectItemOperations.select2('data');

    if (selectedData && selectedData.length > 0) {

        var operationDataId = selectedData[0]['item']['OperationDataId'];
        var operationNumber = selectedData[0]['item']['OperationNumber'];

        if (operationDataId) {
            $.ajax({
                url: RMCDN.Nodes.$itemModal.attr('data-mbf-url') + 'operationDataId=' + operationDataId + '&relatedOperationNumber=' + operationNumber,
                type: "post",
                async: false,
                success: function (data) {
                    RMCDN.Nodes.$selectMBFCode.select2({
                        minimumResultsForSearch: -1,
                        width: '94%',
                    });

                    RMCDN._ClearMBFCodeSelect();

                    $.each(data['Models'], function (index, mbf) {
                        RMCDN.Nodes.$selectMBFCode.append($('<option>', {
                            value: mbf.Item1,
                            text: mbf.Item1 + mbf.Item2
                        }));
                    });
                }
            });
        }
    }
};

RMCDN.Init = function () {

    $(document).ready(function () {
        RMCDN.Texts = new Object();
        RMCDN.Data = new Object();
        RMCDN.Nodes = new Object();
        RMCDN.Beneficiaries = [];

        RMCDN._AssignNodes();
        RMCDN._ReadData();
        RMCDN._InitComponentButtons();
    });
};

RMCDN.AddBeneficiary = function (lmsRequestNumber) {

    if (lmsRequestNumber && !RMCDN._HasBeneficiary(lmsRequestNumber)) {
        RMCDN.Beneficiaries.push(lmsRequestNumber);
        RMCDN._UpdateBeneficiariesBlock();
        RMCDN._InitBeneficiariesSelect(true);
        RMCDN.ResizeModal();
    }
};

RMCDN.ClearDialog = function () {

    RMCDN.ComponentId = 0;
    RMCDN.Id = 0;
    RMCDN.Nodes.$form.find('#CND-detail').val('');
    RMCDN.Nodes.$form.find('#CND-expectedResults').val('');
    RMCDN.Nodes.$form.find('#CND-specialist').val('').triggerHandler('change.select2');
    RMCDN.Nodes.$form.find('#CND-pointContact').val('').triggerHandler('change.select2');
    RMCDN.Nodes.$form.find('[name=CND-plannedStartDate]').val('');
    RMCDN.Nodes.$form.find('[name=CND-plannedEndDate]').val('');

    RMCDN.Beneficiaries = [];
    RMCDN._InitBeneficiariesSelect();
    RMCDN._UpdateBeneficiariesBlock();

    $('#CND-total-planned-ammount').html('0');
};

RMCDN.ClearItemDialog = function () {

    if (!RMCDN.Nodes.$selectItemOperations.data('select2')) {
        RMCDN._InitItemOperationsSelect(false);
    }

    RMCDN.ClearItemError();
    RMCDN._ClearItemOperationsSelect();

    $('#CND-item-desc').val('');
    $('#CND-item-amount').val('');
    $('#CND-item-reference').val('');
    $('#CND-item-units').val('');
    $('#CND-item-comment').val('');

    var $selectTheme = $('#CND-item-theme').empty();

    $.each(RMCDN.ItemThemes, function (id, item) {
        $('<option />').attr({ 'value': item['id'] }).html(item['text']).appendTo($selectTheme);
    });

    $selectTheme.get(0).selectedIndex = -1;

    var $selectUnit = $('#CND-item-unit').empty();

    $.each(RMCDN.ItemUnits, function (id, item) {
        $('<option />').attr({ 'value': item['id'] }).html(item['text']).appendTo($selectUnit);
    });

    $selectUnit.get(0).selectedIndex = -1;

    var $selectMBF = $('#CND-item-mbf').empty();

    $.each(RMCDN.ItemMBF, function (id, item) {
        $('<option />').attr({ 'value': item['id'] }).html(item['text']).appendTo($selectMBF);
    });

    $selectMBF.get(0).selectedIndex = -1;

    var $selectTypeSupport = $('#CND-item-support').prop('disabled', true).empty();
    $selectTypeSupport.get(0).selectedIndex = -1;

    var $activityGroupBlockOptions = $('#activity-group-block-options').empty();

    $.each(RMCDN.ItemActivityGroups, function (index, item) {

        var inputId = "activity-id-" + item['id'];

        var $item = $('<li />').appendTo($activityGroupBlockOptions);
        var $input = $('<input type="radio" />').
            attr({
                'name': 'activity-group',
                'id': inputId,
                'value': item['id'],
                'title': item['desc']
            }).
            addClass('mandatory').
            on('click', function () {

                $selectTypeSupport.empty();
                $selectTypeSupport.get(0).selectedIndex = -1;
                $selectTypeSupport.prop('disabled', item['type_supports'].length == 0);

                $.each(item['type_supports'], function (indexSupport, idSupport) {

                    $.each(RMCDN.ItemTypeSupports, function (indexSupportItem, supportItem) {

                        if (supportItem['id'] == idSupport) {

                            $('<option />').
                                attr({ 'value': idSupport }).
                                html(supportItem['text']).
                                appendTo($selectTypeSupport);
                            return false;
                        }
                    });
                });

                $selectTypeSupport.get(0).selectedIndex = -1;
            }).
            appendTo($item);

        var $label = $('<label />').
            attr({
                'for': inputId,
                'title': item['desc']
            }).
            html(item['text']).
            appendTo($item);
    });
};

RMCDN.ClearError = function () {
    RMCDN.Nodes.$errorBlock.hide();
};

RMCDN.ShowError = function (msg) {
    RMCDN.Nodes.$errorBlock.html(msg).show();
};

RMCDN.ClearItemError = function () {
    RMCDN.Nodes.$itemErrorBlock.hide();
};

RMCDN.ShowItemError = function (msg) {
    RMCDN.Nodes.$itemErrorBlock.html(msg).show();
};

RMCDN.Validate = function () {

    RMCDN.ClearError();

    var fieldMissing = null;

    if (!RMCDN.Nodes.$form.find('#CND-detail').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('#CND-detail').attr('placeholder');
    } else if (!RMCDN.Nodes.$form.find('#CND-expectedResults').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('#CND-expectedResults').attr('placeholder');
    } else if (!RMCDN.Nodes.$form.find('#CND-specialist').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('[for=CND-specialist]').text();
    } else if (!RMCDN.Nodes.$form.find('#CND-pointContact').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('[for=CND-pointContact]').text();
    } else if (!RMCDN.Nodes.$form.find('[name=CND-plannedStartDate]').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('[for=CND-plannedStartDate]').text();
    } else if (!RMCDN.Nodes.$form.find('[name=CND-plannedEndDate]').val()) {
        fieldMissing = RMCDN.Nodes.$form.find('[for=CND-plannedEndDate]').text();
    }

    if (!fieldMissing) {
        var startDate = Date.parse($('#CND-plannedStartDate').val());
        var endDate = Date.parse($('#CND-plannedEndDate').val());

        if (startDate > endDate) {
            RMCDN.ShowError(RMCDN.Texts.DateValidation);
            return false;
        }
    }

    if (fieldMissing) {
        var errorMessage = RMCDN.Nodes.$modal.attr('data-text-required-value').replace(/\{0\}/g, fieldMissing);
        RMCDN.ShowError(errorMessage);
        return false;
    }

    return true;
};

RMCDN.GetBeneficiariesArray = function (lmsRequestNumbers) {
    var beneficiaries = [];
    $.each(lmsRequestNumbers, function (index, lmsRequestNumber) {
        beneficiaries.push(RMCDN._GetBeneficiaryFromLMSRequestNumber(lmsRequestNumber));
    });
    return beneficiaries;
}

RMCDN.Save = function () {

    var beneficiaries = RMCDN.GetBeneficiariesArray(RMCDN.Beneficiaries);

    var data = {
        'Statement': RMCDN.Nodes.$form.find('#CND-detail').val(),
        'ExpectedResults': RMCDN.Nodes.$form.find('#CND-expectedResults').val(),
        'Contact': RMCDN.Nodes.$form.find('#CND-pointContact').val(),
        'Specialist': RMCDN.Nodes.$form.find('#CND-specialist').val(),
        'StartDate': RMCDN.Nodes.$form.find('[name=CND-plannedStartDate]').val(),
        'EndDate': RMCDN.Nodes.$form.find('[name=CND-plannedEndDate]').val(),
        'OrderNumber': $('[id^=tableTreeRowTitle]').length + 1,
        'ResultsMatrixId': RMCDN.Nodes.$modal.attr('data-results-matrix-id'),
        'Beneficiaries': beneficiaries,
        'ComponentId': RMCDN.ComponentId,
        'Id': RMCDN.Id
    };

    var saveUrl = RMCDN.Nodes.$modal.attr('data-save-url');

    unbindDocumentAjaxActions();
    showLoader();
    $.ajax({
        'url': saveUrl,
        'type': 'post',
        'data': data,
        'success': function (response) {

            if (response && response.IsValid) {
                location.reload();
            } else if (response && response.ErrorMessage) {
                RMCDN.ShowError(response.ErrorMessage);
                hideLoader();
            } else {
                RMCDN.ShowError(RMCDN.Texts.UnkownError);
                hideLoader();
            }

        },
        'error': function () {
            RMCDN.ShowError(RMCDN.Texts.UnkownError);
            hideLoader();
        }
    })
};

RMCDN.ValidateItem = function (item) {

    RMCDN.ClearItemError();

    var fieldMissing = null;
    var fieldTextMissing = "";

    if (!item['desc']) {
        fieldMissing = 'Description';
        fieldTextMissing = $('[for=CND-item-desc]').text().trim();
    } else if (!item['id_activity_group']) {
        fieldMissing = 'id_activity_group';
        fieldTextMissing = $('label.activity-group-label').text().trim();
    } else if (!item['id_theme']) {
        fieldMissing = 'id_theme';
        fieldTextMissing = $('[for=CND-item-theme]').text().trim();
    } else if (!item['amount']) {
        fieldMissing = 'amount';
        fieldTextMissing = $('[for=CND-item-amount]').text().trim();
    } else if (!item['units']) {
        fieldMissing = 'units';
        fieldTextMissing = $('[for=CND-item-units]').text().trim();
    } else if (!item['id_unit']) {
        fieldMissing = 'id_unit';
        fieldTextMissing = $('[for=CND-item-unit]').text().trim();
    } else if (!item['id_mbf']) {
        fieldMissing = 'id_mbf';
        fieldTextMissing = $('[for=CND-item-mbf]').text().trim();
    } else if (!item['id_operation']) {
        fieldMissing = 'id_operation';
        fieldTextMissing = $('[for=CND-item-operation]').text().trim();
    }

    if (fieldMissing) {
        var errorMessage = RMCDN.Nodes.$modal.attr('data-text-required-value').replace(/\{0\}/g, "<b>" + fieldTextMissing + "</b>");
        RMCDN.ShowItemError(errorMessage);
        return false;
    }

    return true;
};

RMCDN.ResizeModal = function () {
    RMCDN.Nodes.$modal.find('.modal-content').css({ 'height': '' });
};

RMCDN.ResizeItemModal = function () {

    RMCDN.Nodes.$itemModal.find('.modal-content').css({ 'height': '' });

    RMCDN.Nodes.$itemModal.find('label.inline').each(function (index, item) {
        var $label = $(this);
        var $input = $('#' + $label.attr('for'));

        if ($input.is('select') || $input.is('input')) {
            var labelWidth = $label.css({ 'display': 'inline', 'margin-right': '10px' }).width() + 30;
            $input.css({ 'display': 'inline', 'width': 'calc(100% - ' + String(labelWidth) + 'px)', 'min-width': '0' });

        }
    });
};

RMCDN.GetComponent = function (id, fullModel) {

    var url = RMCDN.Nodes.$modal.attr('data-url-get-component') + 'id=' + id;
    var component = null;

    $.ajax({
        url: url,
        type: 'post',
        async: false,
        success: function (response) {
            component = fullModel ? response : response['ComponentModel'];
        }
    });

    return component;
};

RMCDN.LoadData = function (data) {
    RMCDN.Nodes.$form.find('#CND-detail').val(data['Statement']);
    RMCDN.Nodes.$form.find('#CND-expectedResults').val(data['ExpectedResults']);
    RMCDN.Nodes.$form.find('[name=CND-plannedStartDate]').val(data['StartDate']);
    RMCDN.Nodes.$form.find('[name=CND-plannedEndDate]').val(data['EndDate']);

    $.each(data['Beneficiaries'], function (index, item) {
        RMCDN.Beneficiaries.push(item['LMSRequestNumber']);
    });

    RMCDN.ComponentId = data['ComponentId'];
    RMCDN.Id = data['Id'];

    var startDate = data['StartDate'].replace(/\D+/g, '');
    var endDate = data['EndDate'].replace(/\D+/g, '');

    $("#CND-plannedStartDate").datepicker("setDate", new Date(parseInt(startDate)));
    $("#CND-plannedEndDate").datepicker("setDate", new Date(parseInt(endDate)));

    RMCDN._UpdateBeneficiariesBlock();
    RMCDN._InitBeneficiariesSelect();

    $('#CND-pointContact').append(new Option(data['ContactData']['ListResponse'][0]['Text'], data['ContactData']['ListResponse'][0]['Value'], true, true)).trigger('change');
    $('#CND-specialist').append(new Option(data['SpecialistData']['ListResponse'][0]['Text'], data['SpecialistData']['ListResponse'][0]['Value'], true, true)).trigger('change');

    $('#CND-total-planned-ammount').empty();
    $('#CND-total-planned-ammount').html(addCommas(String(data['ComponentRevisedCost'])));
};

RMCDN.OpenModal = function (id, readonly) {


    RMCDN.ClearError();

    RMCDN._ReadData();
    RMCDN.ClearDialog();
    RMCDN._InitComponents();

    RMCDN.Nodes.$modal.css({ 'margin-top': 20 });

    if (id) {
        var url = RMCDN.Nodes.$modal.attr('data-url-get-component') + 'id=' + id;

        $.ajax({
            url: url,
            type: 'post',
            success: function (response) {
                if (response !== null) {
                    var data = response['ComponentModel'];

                    data['ContactData'] = response['ContactData'];
                    data['SpecialistData'] = response['SpecialistData'];

                    RMCDN.LoadData(data);

                    RMCDN.Nodes.$modal.modal('show');
                }
            }
        })
    } else {

        RMCDN.Nodes.$modal.modal('show');
    }

    RMCDN._DisableComponentDialog(readonly);
};

RMCDN.ValidateAndSave = function () {
    if (RMCDN.Validate()) {
        RMCDN.Save();
    }
};

RMCDN.ItemValidateAndSave = function () {

    var item = RMCDN._ReadItemForm();

    if (RMCDN.ValidateItem(item)) {

        var url = RMCDN.Nodes.$itemModal.attr('data-save-url');

        showLoader();
        unbindDocumentAjaxActions();

        $.ajax({
            url: url,
            type: 'post',
            data: RMCDN._TransformItemSaveModel(item),
            'success': function (response) {

                if (response && response.IsValid) {
                    location.reload();
                } else if (response && response.ErrorMessage) {
                    RMCDN.ShowItemError(response.ErrorMessage);
                    hideLoader();
                } else {
                    RMCDN.ShowItemError(RMCDN.Texts.UnkownError);
                    hideLoader();
                }

            },
            'error': function () {
                RMCDN.ShowItemError(RMCDN.Texts.UnkownError);
                hideLoader();
            }
        })
    }
};

RMCDN.DisableItemDialog = function (enable) {

    RMCDN.Nodes.$itemModal.find('input').prop('disabled', !enable);
    RMCDN.Nodes.$itemModal.find('select').prop('disabled', !enable);
    RMCDN.Nodes.$itemModal.find('textarea').prop('disabled', !enable);

    if (enable) {
        RMCDN.Nodes.$itemModal.find('.buttons-enable').show();
        RMCDN.Nodes.$itemModal.find('.buttons-disable').hide();
    } else {
        RMCDN.Nodes.$itemModal.find('.buttons-enable').hide();
        RMCDN.Nodes.$itemModal.find('.buttons-disable').show();
    }
};

RMCDN.OpenItemDialog = function (componentId, itemId) {

    RMCDN.ClearItemDialog();
    RMCDN.Nodes.$itemModal.find('[name=id_component]').val(componentId);
    RMCDN.Nodes.$itemModal.find('[name=id_cnd]').val('0');
    RMCDN.DisableItemDialog(true);

    if (itemId) {

        RMCDN.Nodes.$itemModal.find('.item-disabled-block').show();
        RMCDN.Nodes.$itemModal.find('.modal-title.edit').show();
        RMCDN.Nodes.$itemModal.find('.modal-title.add').hide();
        RMCDN.Nodes.$itemModal.find('[name=id_output]').val(itemId);

        $.ajax({
            url: RMCDN.Nodes.$itemModal.attr('data-get-url') + 'outputId=' + itemId,
            type: 'post',
            'success': function (response) {
                RMCDN._LoadItem(response['Model']);
            },
            'error': function () {
                RMCDN.ShowItemError(RMCDN.Texts.UnkownError);
            }
        })

    } else {

        RMCDN.Nodes.$itemModal.find('.item-disabled-block').hide();
        RMCDN.Nodes.$itemModal.find('#CND-item-disabled').prop('checked', false);

        RMCDN.Nodes.$itemModal.find('.modal-title.add').show();
        RMCDN.Nodes.$itemModal.find('.modal-title.edit').hide();

        RMCDN.Nodes.$itemModal.find('[name=id_output]').val('0');
    }

    RMCDN.Nodes.$itemModal.modal('show');

    RMCDN.Nodes.$itemModal.css({ 'margin-top': 20 });
};


RMCDN.OpenItemDialogReadOnly = function (componentId, itemId) {

    RMCDN.ClearItemDialog();
    RMCDN.Nodes.$itemModal.find('[name=id_component]').val(componentId);
    RMCDN.Nodes.$itemModal.find('[name=id_cnd]').val('0');

    RMCDN.Nodes.$itemModal.find('.item-disabled-block').show();
    RMCDN.Nodes.$itemModal.find('.modal-title.edit').show();
    RMCDN.Nodes.$itemModal.find('.modal-title.add').hide();
    RMCDN.Nodes.$itemModal.find('[name=id_output]').val(itemId);

    $.ajax({
        url: RMCDN.Nodes.$itemModal.attr('data-get-url') + 'outputId=' + itemId,
        type: 'post',
        'success': function (response) {
            RMCDN._LoadItem(response['Model']);
        },
        'error': function () {
            RMCDN.ShowItemError(RMCDN.Texts.UnkownError);
        }
    });

    RMCDN.DisableItemDialog(false);

    var component = RMCDN.GetComponent(componentId, true);

    if (component['IsEditable']) {
        $('#CND-item-disabled').prop('disabled', false);
        RMCDN.Nodes.$itemModal.find('.buttons-enable').show();
        RMCDN.Nodes.$itemModal.find('.buttons-disable').hide();
    }

    RMCDN.Nodes.$itemModal.modal('show');
    RMCDN.Nodes.$itemModal.css({ 'margin-top': 20 });
};


function RMCDNOpenModal() {
    RMCDN.OpenModal();
}

function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}