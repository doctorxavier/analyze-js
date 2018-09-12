function checkDisclosableDocument() {
    var frm = $('#frmPublishDocument')
    Validation.Destroy();
    if (!Validation.Container(frm)) {
        return;
    }

    var docNum = $('#DocumentNumber').val();
    var actionPath = frm.prop('action');
    validateDisclosableDocument(docNum, actionPath);
}
