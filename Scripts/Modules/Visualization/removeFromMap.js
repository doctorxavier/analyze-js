function selectDeselectVORemoveItems(origin) {
    var selectedItemValue = $(origin)[0].selectedIndex;
    var allInternalCheckboxes = $(origin).closest('[class=mod_contenido_central impacts]').find('[name=sendtointernalmap]:enabled');
    var allExternalCheckboxes = $(origin).closest('[class=mod_contenido_central impacts]').find('[name=sendtoexternalmap]:enabled:visible');

    switch (selectedItemValue) {
        case 1:
            allInternalCheckboxes.prop('checked', true);
            allExternalCheckboxes.prop('checked', true);
            break;
        case 2:
            allInternalCheckboxes.prop('checked', false);
            break;
        case 3:
            allExternalCheckboxes.prop('checked', true);
            break;
        case 4:
            allExternalCheckboxes.prop('checked', false);
            allExternalCheckboxes.parent().parent().find('[name=sendtointernalmap]').prop('checked', false);
            break;
    }

    origin.value = "0";
}

function internalRemoveCheckClicked(origin) {
    dataInPageHasChanged = true;
    var selectedItem = $(origin);

    if (selectedItem.prop('checked')) {
        selectedItem.parent().parent().find('#external-check').prop('checked', true);
    }
}

function externalRemoveCheckClicked(origin) {
    dataInPageHasChanged = true;
    var selectedItem = $(origin);

    if (!selectedItem.prop('checked')) {
        selectedItem.parent().parent().find('#internal-check').prop('checked', false);
    }
}

function removeItemsFromMap(destinationUrl, operationType, operIsSaveAndVal, operationOrigin, returnUrl) {
    if (operIsSaveAndVal || dataInPageHasChanged) {
        var removeFromMapDataModels = [];

        $('.checkboxContainer').each(function (i, obj) {
            var currentObj = $(obj);
            var selectedElements = currentObj.find('input');
            var removeFromMapDataObject = new Object();

            removeFromMapDataObject.vovId = parseInt(currentObj.attr('vov-id'));
            removeFromMapDataObject.dest = [];

            selectedElements.each(function (j, obj2) {
                var selectedObj = $(obj2);

                if (selectedObj.prop('checked')) {
                    removeFromMapDataObject.dest.push(parseInt(selectedObj.val()));
                }
            });

            removeFromMapDataModels.push(removeFromMapDataObject);
        });

        RemoveFromMap(removeFromMapDataModels, operationType, operationOrigin, destinationUrl, returnUrl);
    }

    return false;
}

function RemoveFromMap(removeFromMapDataModels, operationType, operationOrigin, destinationUrl, returnUrl) {
    var objectToSend = new Object();

    objectToSend.operationType = operationType;
    objectToSend.operationOrigin = operationOrigin;
    objectToSend.removeFromMapDataModels = removeFromMapDataModels;

    $.ajax({
        type: "POST",
        url: destinationUrl,
        data: JSON.stringify(objectToSend),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            var params = "?notifStatus=" + data.notifStatus;

            window.location = returnUrl + params;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            window.location = returnUrl + "?notifStatus=ErrorSavingData";
        }
    });
}