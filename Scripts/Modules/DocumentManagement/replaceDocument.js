function selectReplacementDocument(tableRecord) {
    tableRecord.documentName = getDocName(tableRecord);

    $('#replaceDocumentInput')
           .trigger("click")
           .off('change').on('change', function () {
               event.stopPropagation();
               sendReplaceDocument(tableRecord.documentNumber, tableRecord.actionService, tableRecord.selectedRow, tableRecord.documentName);
           });
}

function sendReplaceDocument(documentNumber, targetUri, tableRecord, documentName) {
    if (documentNumber == null || targetUri == null) {
        return;
    }

    restrictFileType(documentName);

    var postData = new FormData();
    postData.append("documentNumber", documentNumber);
    postData.append("documentName", documentName);
    postData.append("files", $('#fileArea')[0].files[0]);
    postData.append("inputFileAccept", $('#replaceDocumentInput').prop('accept'));
    var ajaxOptions = {
        url: targetUri,
        method: 'post',
        async: true,
        data: postData,
        processData: false,
        contentType: false,
        beforeSend: function(){
            window.updateRowStatus(0);
            window.hideLoader();
        },
        success: function (data) {
            if (data.IsValid) {
                window.fileStatus(0, 'success-file', true);
                $(objDoc.modal).find(objDoc.btnAdd).hide();
                $(objDoc.modal).find(objDoc.btnAccept).show();
            }
            else {
                window.fileStatus(0, 'warning-file', false);
                $(objDoc.modal).find(objDoc.btnAdd).hide();
                $(objDoc.modal).find(objDoc.btnAccept).show();
                window.showError(data.ErrorMessage);
            }

            window.hideLoaderOptional();
            window.disabledAddFiles(true);
            window.disabledSearchTab(false);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showReplaceError(errorThrown);
            window.fileStatus(0, 'warning-file', false);
            $(objDoc.modal).find(objDoc.btnAdd).hide();
            $(objDoc.modal).find(objDoc.btnAccept).show();
        },
        complete: function () {
            hideLoaderOptional();
        }
    };

    $.ajax(ajaxOptions);
}

function showReplaceError(msg) {
    errorBar(msg, 60, true);
}

function showReplaceSuccess(msg) {
    successBar(msg, 60, true);
}

function getDocName(tableRecord) {
    var name = '';
    if (tableRecord != null) {
        name = tableRecord.selectedRow.find('[name=DocumentName]').text().trim();
    }

    return name;
}

function restrictFileType(actualDocName) {
    if (actualDocName != null) {
        var allowedExt = actualDocName.substr(actualDocName.lastIndexOf('.'));
        $('#fileArea').prop('accept', allowedExt);
    }
}
