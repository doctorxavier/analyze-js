function selectDeselectVOSendItems(origin) {
    var selectedItemValue = $(origin)[0].selectedIndex;
    var allInternalCheckboxes = $(origin).closest('[class=mod_contenido_central impacts]').find('[name=sendtointernalmap]:enabled');
    var allExternalCheckboxes = $(origin).closest('[class=mod_contenido_central impacts]').find('[name=sendtoexternalmap]');

    switch (selectedItemValue) {
        case 1:
            allInternalCheckboxes.prop('checked', true);
            break;
        case 2:
            allInternalCheckboxes.prop('checked', false);
            allInternalCheckboxes.parent().parent().find('[name=sendtoexternalmap]').prop('checked', false);
            break;
        case 3:
            allExternalCheckboxes.prop('checked', true);
            allInternalCheckboxes.prop('checked', true);
            break;
        case 4:
            allExternalCheckboxes.prop('checked', false);
            break;
    }

    origin.value = "0";
}

function internalSendCheckClicked(origin) {
    dataInPageHasChanged = true;
    var selectedItem = $(origin);

    if (!selectedItem.prop('checked')) {
        selectedItem.parent().parent().find('#external-check').prop('checked', false);
    }
}

function externalSendCheckClicked(origin) {
    dataInPageHasChanged = true;
    var selectedItem = $(origin);

    if (selectedItem.prop('checked')) {
        var internalEnabled = selectedItem.parent().parent().find('#internal-check:enabled');

        if (internalEnabled.length > 0) {
            $(internalEnabled).prop('checked', true);
        }
    }
}

function sendItemsToMap(destinationUrl, operationType, operIsSaveAndVal, operationOrigin, returnUrl) {
    if (operIsSaveAndVal || dataInPageHasChanged) {
        var sendToMapDataModels = [];

        $('.checkboxContainer').each(function (i, obj) {
            var currentObj = $(obj);
            var selectedElements = currentObj.find('input');
            var sendToMapDataObject = new Object();

            sendToMapDataObject.vovId = parseInt(currentObj.attr('vov-id'));
            sendToMapDataObject.dest = [];

            selectedElements.each(function (j, obj2) {
                var selectedObj = $(obj2);
                
                if (selectedObj.prop('checked')) {
                    sendToMapDataObject.dest.push(parseInt(selectedObj.val()));
                }
            });

            sendToMapDataModels.push(sendToMapDataObject);
        });

        SendToMap(sendToMapDataModels, operationType, operationOrigin, destinationUrl, returnUrl);
    }

    return false;
}

function SendToMap(sendToMapDataModels, operationType, operationOrigin, destinationUrl, returnUrl) {
    var objectToSend = new Object();

    objectToSend.operationType = operationType;
    objectToSend.operationOrigin = operationOrigin;
    objectToSend.sendToMapDataModels = sendToMapDataModels;

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
