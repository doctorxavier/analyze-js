var MessageTranslation = new Object();

MessageTranslation._texts = {};
MessageTranslation._texts['deleteConditionMessage'] = null;
MessageTranslation._texts['errorMessage'] = null;
MessageTranslation._texts['requestDateMessage'] = null;
MessageTranslation._texts['requestDateMessageToSave'] = null;
MessageTranslation._texts['mandatorySubmissionDate'] = null;
MessageTranslation._texts['expiredMessage'] = null;

$(document).ajaxComplete(function () {
    setTimeout(function () { closeNotification(); }, 2000);
});

MessageTranslation.text = function (key, value) {
    MessageTranslation._texts[key] = value;
};

$(document).ready(function () {
    ShowMessageFunctions.CurrentFunction = ShowMessageFunctions.GenericFunction;
    $('html').off('resize');

    $('.submitFormTarget').on('click', function () {
        var form = $("#target");

        if (!form.valid()) {
            form.validate().focusInvalid();

            return;
        }

        SubmitForm(form);
    });

    $('.changeConditionStatus').on('click', function () {
        var confirmMessage = MessageTranslation._texts['changeConditionStatusMessage'];
        var source = $(this);

        confirmAction(confirmMessage).done(function (value) {
            if (!value)
                return;

            saveAndChangeStatus(changeStatus, source);
        });
    });

    $('.checkDate').on('click', function () {
        var $container = $(this).closest('.headerContainer').find('.dateContainer');
        var route = $(this).data('route');

        $container.find('.datePlain').toggle();
        $container.find('.pickerDate').toggle();

        if ($(this).is(':checked')) {
            var valuePicker = $container.find('.pickerDate').val();
            var datePlain = $container.find('.datePlain');

            if (valuePicker !== '' && datePlain.text() !== valuePicker) {
                $.ajax({
                    type: 'POST',
                    url: route + '&date=' + valuePicker,
                    data: valuePicker,
                    success: function (response) {
                        if (!response.IsValid) {
                            showMessage(response.ErrorMessage, false);
                            return false;
                        }

                        datePlain.text(valuePicker);
                        ShowNotification(response.NotificationType, response.NotificationMessage);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        ShowNotification('error', MessageTranslation._texts['errorMessage']);
                    }
                });
            }
        }
    })
});

var SubmitForm = function (form) {
    var frm = $(form);

    frm.submit(function (ev) {
        $.ajax({
            type: 'POST',
            url: frm.attr('action'),
            data: frm.serialize(),
            success: function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage, false);
                    return false;
                }

                ShowNotification(response.NotificationType, response.NotificationMessage);
                location.href = response.Route;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowNotification('error', MessageTranslation._texts['errorMessage']);
            }
        });

        ev.preventDefault();
    });
}

function AjaxCall(object, routeActionMethod) {
    $.ajax({
        url: routeActionMethod,
        type: 'POST',
        data: JSON.stringify(object),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                showMessage(response.ErrorMessage, false);
                return false;
            }

            ShowNotification(response.NotificationType, response.NotificationMessage);
            vex.close();
            location.href = response.Route;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowNotification('error', MessageTranslation._texts['errorMessage']);
        }
    });
}

function ShowNotification(type, message) {
    showNotification({
        'message': message,
        'type': type,
        'autoClose': 'true',
        'duration': '5'
    });
}

function newComment(source) {
    insertNewComment(source, 'individual');
}

function newCommentExtension(source) {
    insertNewComment(source, 'extension');
}

function insertNewComment(source, type) {
    var model, inputs;
    var date = $(source).data('datetime');
    var userName = $(source).data('username');
    var content = $('#commentslistdinamic');
    var lastComment = content.find('div.userComment').last();
    var i = calcIndex(lastComment);

    if (type === 'extension')
        model = 'ConditionIndividuals[0].ConditionExtensions[0].';
    else
        model = 'ConditionIndividuals[0].';

    inputs = '\
        <input type="hidden" value="0" name="' + model + 'UserComments[' + i + '].UserCommentId" />\
        <input type="hidden" name="' + model + 'UserComments[' + i + '].IsDelete" \
            value="' + model + 'UserComments[' + i + '].IsDelete" class="isDeleteComment" />\
        <textarea name="' + model + 'UserComments[' + i + '].Text" data-force-parsley-validation="true" \
            data-parsley-required="true" class="inputTextarea txtDescriptionComent valid" aria-invalid="false"></textarea>';

    var tableModel = '\
        <table id="commentTableUIRI002_edit">\
            <tbody id="comentContainer">\
                <tr>\
                    <td class="lastRowCommentUserJS hidden">' + userName + '</td>\
                </tr>\
            </tbody>\
        </table>';
    var commentModel = '\
        <div class="userComment ml40 width94p" data-index="' + i + ' ">' + tableModel + '\
        <div class="inputComment"><div class="labels">\
            <label><label class="labelNormal">' + date + '</label></label>\
            <label><label class="labelNormal">' + userName + '</label></label>\
            <label>\
                <button class="buttonTrash" data-action="removeUserComment" data-parsley-required="true" \
                    data-idb-fieldname="btnDeleteComment" data-commentid="0"><span class="icon"></span><span></span>\
                </button>\
            </label>\
        </div>' + inputs + '</div></div>';

    if (lastComment.length === 0)
        content.append(commentModel);
    else
        lastComment.after(commentModel);

    $('#Accept_').val('1');
    $('#Reject_').val('1');
    $('#EditMessage_').val('1');

    
}

function calcIndex(source) {
    var i = parseInt(source.data('index'));

    if (isNaN(i))
        i = 0;
    else
        i++;

    return i;
}

function removeUserComment(source) {
    var content = $(source).closest('.userComment');
    content.find('#commentTableUIRI002_edit').remove();
    content.addClass('hidden');
    content.find('.isDeleteComment').val(true);
}

function addDocumentInTable(documentList, sourceType, fileNames) {
    if (documentList.length <= 0)
        return;

    var docNumber = sourceType === 'selected' ? documentList[0].DocumentNumber : documentList[0];
    var tableContainer = $('#tableDocuments');
    var object = {
        entityRelated: tableContainer.data('entityrelated'),
        documentNumber: docNumber
    };

    $.ajax({
        url: tableContainer.data('getdatadocument'),
        type: 'POST',
        data: JSON.stringify(object),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                ShowNotification(response.NotificationType, response.NotificationMessage);
                return false;
            }

            var container = $('#tableDocuments');
            var entityRelated = container.data('entityrelated');
            var lastRow = container.find('tbody tr').last();
            var i = calcIndex(lastRow);
            var commonModel;

            if (entityRelated === 'ConditionIndividual')
                commonModel = '<input type="hidden" name="ConditionIndividuals[0].UserDocuments[' + i + '].';
            else
                commonModel = '<input type="hidden" \
                    name="ConditionIndividuals[0].ConditionExtensions[0].UserDocuments[' + i + '].';

            var inputs =
                commonModel + 'DocumentId" value="' + response.document.DocumentId + '" />' +
                commonModel + 'EntityRelated" value="' + entityRelated + '" />' +
                commonModel + 'IsNew" value="true" />' +
                commonModel + 'IsDelete" value="false" class="IsDelete" />';

            var newRow =
                '<tr data-index="' + i + '">\
                    <td>' + response.document.UserName + '</td>\
                    <td>' + response.document.DateDocument + '</td>\
                    <td>' + response.document.DocumentReference + '</td>\
                    <td>' + response.document.Description + '</td>\
                    <td>\
                        <button class="buttonTrash" name="deleteDocument" data-action="verifyDeleteDocument"\
                            data-parsley-required="true" data-deletemessage="' + response.DeleteMessage + '"\
                            data-docdescription="' + response.document.Description + '" data-idb-fieldname="btnDeleteDocument"\
                            data-route="' + response.document.DownloadURL + '"><span class="icon"></span><span></span>\
                        </button>\
                    </td>' + inputs + '\
                </tr>';

            container.append(newRow);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowNotification('error', MessageTranslation._texts['errorMessage']);
        }
    });

    vex.close();
    hideLoaderOptional();
}

function verifyDeleteDocument(source) {
    confirmAction(source.data('deletemessage')).done(function (selectionOk) {
        if (!selectionOk)
            return;

        removeDocument(source);
    });
}

function deleteDocument(source) {
    var object = {};
    AjaxCall(object, source.data('route'));
}

function removeDocument(source) {
    var content = $(source).closest('tr');
    content.addClass('hidden');
    content.find('.IsDelete').val(true);
}

function downloadDocument(source) {
    window.open(source.data('route'), '_blank');
}

var validateFulfill = function (source, conditionNumber) {
    var hasDocument = $('#tableDocuments').find('tbody tr').not('.hidden').length > 0;

    if (!hasDocument) {
        showMessage('Please upload the mandatory document needed to verify the fulfilled of the condition.');
        return false;
    }

    var hasMandatory = warningMandatoryFulfillment();

    if (!hasMandatory)
        return false;

    saveAndChangeStatus(conditionFinalStatusValidationConfirm, source, conditionNumber);
}

function warningMandatoryFulfillment() {
    if (!getSubmissionDate()) {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        confirmActionCustom(MessageTranslation._texts['mandatorySubmissionDate'], null, 'OK');

        return false;
    }

    return true;
}

function conditionFinalStatusValidationConfirm(source, conditionNumber) {
    var confirmMessage = MessageTranslation._texts['fulfillmentConfirmationMessage'];
    confirmMessage = confirmMessage.replace('{0}', conditionNumber);

    confirmAction(confirmMessage).done(function (selectionOk) {
        if (!selectionOk)
            return;

        var sourceElem = $(source);

        $.ajax({
            url: sourceElem.data('route') + '&submissionDate=' + getSubmissionDate(),
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage, false);
                    return false;
                }

                ShowNotification(response.NotificationType, response.NotificationMessage);
                location.href = response.Route;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowNotification('error', MessageTranslation._texts['errorMessage']);
            }
        });
    });
}

function getSubmissionDate() {
    var element = $('#datePicker0');

    return element.val() || element.text();
}

function saveAndChangeStatus(callBack, source, conditionNumber) {
    var frm = $("#target");

    if (frm.length) {
        $.ajax({
            type: 'POST',
            url: frm.attr('action'),
            data: frm.serialize(),
            success: function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage, false);
                    return false;
                }

                callBack(source, conditionNumber);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                ShowNotification('error', MessageTranslation._texts['errorMessage']);
            }
        });
    }
    else {
        callBack(source, conditionNumber);
    }
}

function changeStatus(source) {
    $.ajax({
        url: source.data('route'),
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                showMessage(response.ErrorMessage, false);
                return false;
            }

            ShowNotification(response.NotificationType, response.NotificationMessage);
            location.href = response.Route;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            ShowNotification('error', MessageTranslation._texts['errorMessage']);
        }
    });
}