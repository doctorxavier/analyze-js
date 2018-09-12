function modalOffice365Handler(element) {
    officeElement = element;
    setTimeout(function () {
        $('[name="file"]').off('change');
        $('[name="file"]').change(function () {
            if ($(this).val() !== "") {
                $(this).closest('.uploadContainer').find('.parsley-errors-list').addClass('hide');
                $(this).closest('.uploadContainer').find('[data-name="file-error"]').addClass('hide');
                $(this).closest('.uploadContainer').find('[data-name="file-empty"]').addClass('hide');
            }
        });
    }, 1000);
}

function modalOffice365Close(element) {
    $(element.target).closest('.vex-content').find('.vex-close').click();
}

function modalOffice365Clean() {
    $('#addDocumentOffice365').find('[name="file"]').val('');
    $('#addDocumentOffice365').find('.inputFileInput').val('');
}

function modalOffice365Upload(element) {
    var container = $(element.target).closest('[data-id="mainContainer"]');

    if (container.find('[name="file"]').val() === "") {
        container.find('[data-name="file-empty"]').removeClass('hide');
        container.find('[data-name="file-empty"]').closest('.filled').removeClass('hide');
        return false;
    }

    var nameElement = container.find('[name="file"]').val().split('.');
    var ext = '.' + nameElement[nameElement.length - 1];
    var extAccept = container.find('[name="file"]').attr('accept').split(', ');

    if (extAccept.indexOf(ext) < 0) {
        container.find('[data-name="file-error"]').removeClass('hide');
        container.find('[data-name="file-error"]').closest('.filled').removeClass('hide');
        return false;
    }
    var data = new FormData();
    data.append("file", container.find('[name="file"]').prop('files')[0]);
    data.append("module", container.find('[name="module"]').val());
    data.append("operationNumber", container.find('[name="operation"]').val());
    data.append("folder", container.find('[name="folder"]').val());
    data.append("nameDocument", container.find('[name="documentname"]').val());

    var documentList = $('#documentsTable tbody tr [name=documentNumber]:not([value=""])').closest('tr').find('[name=documentName]');

    if (documentList.length > 0) {
        var documentArray = new Array();
        documentList.each(function() { documentArray.push($(this).val()) });
        data.append("documentList", documentArray);
    }

    $.ajax({
        type: "POST",
        url: container.find('[data-id="addForm"]').attr("action"),
        data: data,
        async: true,
        element: container,
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            if (data.IsValid === true) {
                var action = $(this.element).find('[name="action"]').val();
                showMessage($(this.element).find('[name="msg"]').val());
                $(this.element).closest('.vex-content').find('.vex-close').click();

                if (action !== "") {
                    window[action](data, officeElement);
                }
            } else {
                showMessage(data.ErrorMessage);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showMessage(errorThrown);
        }
    });

    return true;
}