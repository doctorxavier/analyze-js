function filterBegin() {
    showLoaderOptional();
}

function filterFail(xhr) {
    showError(xhr.statusText);
}

function filterSuccess() {
    hideLoaderOptional();
}

function filterComplete() {
    $("#localDocumentsTable").paginationConfluense(pageSize);
}

function clearAllFilters() {
    $("#TakeAll").remove();
    if ($("#contentForm input[type=checkbox]").val() === 'true') {
        $("#contentForm .checkbox-default").trigger('click');
    }

    $("#contentForm input[type=text]").val('');
    $("#contentForm input[type=checkbox]").val(false);

    $("#contentForm").submit();
}

function toggleFilterSection() {
    $('#searchBoxContainerGlobal004').toggle();
}

function addDocument(documentList, sourceType, description) {
    var data = {
        OperationNumber: $('#operationNumber').val(),
        Documents: documentList
    };

    var url = $('#documentSection').attr("data-target");

    var ajaxOptions = {
        method: "POST",
        url: url,
        data: data,
        async: true,
        success: function (data) {
            if (data.IsValid) {
                window.location.reload();
            } else {
                showError(data.ErrorMessage);
                hideLoaderOptional();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showError(errorThrown);
            hideLoaderOptional();
        }
    };

    $.ajax(ajaxOptions);
}