var tableRecord = {
    selectedRow: null,
    actionService: null,
    documentNumber: null,
    documentName: null
};

function doReplaceDocument(event) {
    if (event == null) {
        return;
    }

    event.stopPropagation();

    var $target = $(event.currentTarget);
    var $currentDocumentName = $target.closest("tr").find('[name="DocumentName"]').text();
    var $currentDocumentClass = $target.closest("tr").find('.DocumentName').text();
    var $documentName = ($currentDocumentName !== null && $currentDocumentName !== undefined)
        ? $currentDocumentName : $currentDocumentClass;
    var $modal = "#" + $('[data-id-modal-drag-drop="documentModalDragDrop"]:first').attr('id');

    tableRecord.actionService = $target.attr("data-target");
    tableRecord.documentNumber = $target.attr("data-content");
    tableRecord.selectedRow = $target.closest('tr');

    $('#dmReplaceWarningModal').modal();
    $('button#btnReplaceOk').off('click').on('click', function () {
        $($modal).attr("data-type", "replace");
        $($modal).find("#currentDocument").find(".documentName").text($.trim($documentName));
        $($modal).find("#currentDocument").find(".documentNumber").text($.trim(tableRecord.documentNumber));
        $($modal).find("#currentDocument").find("#urlReplace").val(tableRecord.actionService);
        startNewModalDocuments($modal);
    });
}

function doDiscloseDocument(event) {
    if (event == null) {
        return;
    }

    event.stopPropagation();
    var $target = $(event.currentTarget);

    tableRecord.actionService = $target.attr("data-target");
    tableRecord.documentNumber = $target.attr("data-content");
    tableRecord.selectedRow = $target.closest('tr');

    $('#dmDiscloseWarningModal').modal();
    $('button#btnDiscloseOk').off('click').on('click', function () {
        validateDisclosableDocument(tableRecord.documentNumber, tableRecord.actionService);
    });
}

function doRemoveDocumentReference(event, deleteTargetFunction) {
    if (event == null) {
        return;
    }

    event.stopPropagation();

    var $target = $(event.currentTarget);

    tableRecord.actionService = $target.attr("data-target");
    tableRecord.documentNumber = $target.attr("data-content");
    tableRecord.selectedRow = $target.closest('tr');

    if (deleteTargetFunction !== '') {
        window[deleteTargetFunction](tableRecord.documentNumber, tableRecord.selectedRow);
    }
}
