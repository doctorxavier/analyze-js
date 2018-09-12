var Localization = {
    GetText: {
        'WarningCloseMilestone': null,
        'WarningTLtoESG': null,
        'WarningESGToTL': null,
        'WarningCompleteRequired': null,
        'WarningCompleteActivity': null,
        'WarningCancel': null,
        'ErrorMessage': null,
        'RootPath': null
    },

    _SetText: function (key, value) {
        this.GetText[key] = value;
    }
};

$(window).on('load', function () {
    closeNotification();
});

function addDocumentInTable(documentList, sourceType, fileNames) {
    if (documentList.length <= 0)
        return;


    if (sourceType === 'selected') {
        docNumber = documentList[0].DocumentNumber;
        fileName = documentList[0].DocumentName;
        url = documentList[0].Url;
    }
    else {
        docNumber = documentList[0];
        fileName = fileNames[0];
        url = '';
    }

    var object = {
        documentNumber: docNumber,
        documentName: fileName,
        url: url
    };

    var route = $('#DisclosureDocuments-modal-add').data('route');

    $.ajax({
        url: route,
        type: 'POST',
        data: JSON.stringify(object),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid)
                return false;

            angular.element(document.getElementById('disclosureContainer')).scope().UpdateDocument(response.document);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            showMessage(Localization.GetText['ErrorMessage']);
        }
    });
}