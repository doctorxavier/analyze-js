var keyDocument = {};

function selectKeyDocument(element) {
    element.attr('data-selected', "uploadKeyBtnSelected");
    $("#selectedRowIdDM").val(element.attr("data-documentid"));
}

function deleteKeyDocument() {
    var element = $(event.currentTarget);
    var url = element.attr('data-url');
    var ajaxOptions = {
        url: url,
        type: 'post',
        async: true,
        success: function (data) {
            processResponse(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            handleDeleteError(errorThrown);
            hideLoaderOptional();
        }
    };

    confirmAction(keyDocumentConfirmMessage).done(function (pressOk) {
        if (pressOk) {
            showLoaderOptional();
            $.ajax(ajaxOptions);
        }});
}

function addKeyDocument(documentList, sourceType) {
    var postData = {
        documentNumber: documentList[0].DocumentNumber,
        documentName: documentList[0].DocumentName,
        author: documentList[0].AuthorId,
        creationDate: documentList[0].UsDateParser,
        selectedRowId: keyDocument.id,
    }

    var ajaxOptions = {
        url: keyDocument.url,
        type: 'post',
        async: true,
        data: postData,
        success: function (data) {
            processResponse(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            handleDeleteError(errorThrown);
            hideLoaderOptional();
        }
    };

    $.ajax(ajaxOptions);

}

function showUploadDocumentModal() {
    var target = $(event.target).parent('button');
    keyDocument = {
        id: target.attr('data-documentid'),
        url: target.attr('data-url')
    }

    startNewModalDocuments();
}

function handleDeleteSuccess() {
    window.location.reload();
    hideLoaderOptional();
}

function handleDeleteError(errorMessage) {
    showError(errorMessage);
    hideLoaderOptional();
}

function processResponse(data){
    if (data.IsValid) {
        handleDeleteSuccess();
    } else {
        handleDeleteError(data.ErrorMessage);
    }
}