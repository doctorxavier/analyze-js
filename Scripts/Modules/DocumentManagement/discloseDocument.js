
function validateDisclosableDocument(documentNumber, url) {
    if ((url == null) || (documentNumber == null)) {
        return;
    }

    var dataForm = {
        documentNumber: documentNumber
    };

    var ajaxObject = {
        type: "POST",
        url: url,
        async: true,
        data: dataForm,
        beforeSend: function () {
            showLoaderOptional();
        },
        success: function (data) {
            if (data.IsValid) {
                Validation.Destroy();
                mapDomFileds(data)
                $("#publishDocumentData").modal();
                $("#btnPublishDocument").off('click').on('click', function () {
                    sendDiscloseDocument();
                });
                getDisclosureActivities();
            } else {
                showDiscloseError(data.ErrorMessage);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showDiscloseError(errorThrown);
        },
        complete: function () {
            hideLoader();
            hideLoaderOptional();
        }
    };

    $.ajax(ajaxObject);
}

function sendDiscloseDocument() {
    event.preventDefault();
    var url = $('#frmPublishValidDocument').attr('data-target');
    if ((typeof url === "undefined") || (url == null)) {
        return;
    }

    $('#StageDescription').val($("[name=StageCode]").GetText());
    var documentNumber = $('#DocumentNumber').val();
    var postData = $('#frmPublishValidDocument');

    var ajaxObject = {
        type: "POST",
        url: url,
        async: true,
        data: postData.serializeArray(),
        beforeSend: function () {
            showLoaderOptional();
        },
        success: function (data) {
            if (data.IsValid) {
                showDiscloseSuccess(data.ErrorMessage);
                $('#btnDiscloseDocument[data-content=' + documentNumber + ']').prop('disabled', true);
                $('#btnReplaceDocument[data-content=' + documentNumber + ']').prop('disabled', true);
                $("#publishDocumentData").modal('hide');
            } else {
                showDiscloseError(data.ErrorMessage);
            }
        },
        error: function () {
            showDiscloseError(data.ErrorMessage);
        },
        complete: function () {
            hideLoaderOptional();
            cleanDomFieds();
        }
    };

    if (Validation.Container(postData)) {
        $.ajax(ajaxObject);
    }
}

function getDisclosureActivities() {
    var url = $("#id-StageCode").attr("data-targetUri");
    if ((typeof url === "undefined") || (url == null))
    {
        return;
    }

    var valuesArea = $("#id-StageCode").siblings('ul.dropdown-menu');

    var ajaxObject = {
        type: "GET",
        url: url,
        cache: true,
        async: true,
        beforeSend: function () {
            showLoaderOptional();
        },
        success: function (data) {
            if (data != null) {
                for (var i = 0; i < data.ListResponse.length; i++) {
                    var aElem = $('<a />')
                        .attr('dd-value', data.ListResponse[i].Value)
                        .attr('dd-parent-id', '');
                    aElem.text(data.ListResponse[i].Text);
                    $('<li/>').append(aElem).appendTo(valuesArea)
                }
            }
        },
        error: function (xhr) {
        },
        complete: function () {
            hideLoaderOptional();
        }
    };

    $.ajax(ajaxObject);
}

function mapDomFileds(data) {
    $("#OperationNumber").val(data.OperationNumber);
    $("#DocumentNumber").val(data.DocumentNumber);
    $("#DocumentName").val(data.DocumentName);
    $("#AuthorId").val(data.AuthorId);
    $("#lblFileExtension").text(data.FileExtension);
    $("#FileExtension").val(data.FileExtension);
    $('#publishDocumentData ul.dropdown-menu')
        .find('[dd-selected]')
        .removeAttr('dd-selected');
}

function cleanDomFieds() {
    $("#DocumentNumber").val('');
    $("#DocumentName").val('');
    $("#AuthorId").val('');
    $("#lblFileExtension").text('');
    $("#FileExtension").val('');
}

function showDiscloseError(msg) {
    errorBar(msg, 60, true);
}

function showDiscloseSuccess(msg) {
    successBar(msg, 60, true);
}
