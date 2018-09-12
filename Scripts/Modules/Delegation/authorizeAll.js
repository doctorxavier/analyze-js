$(document).ready(function () {
    if (parseInt(globalVariables.delegationId) == 0) {
        edit();
    }
    else {
        $("#PageContent").removeClass("hide");
    }
});

function edit() {
    if (parseInt(globalVariables.delegationId) > 0) {
        postUrlWithOptions(globalVariables.urlLockRegister,
            { async: false },
            { operationNumber: globalVariables.delegationId, url: globalVariables.urlDelegation })
            .done(function (data) {
                if (data.IsValid === false) {
                    showMessage(data.ErrorMessage);
                }
                else {
                    enterEditMode(false, $('#PageContent'), false);
                    enterEditMode(false, $('[data-id=headerButtons]'), false);
                }
            });
    }
    else {
        enterEditMode(false, $('#PageContent'), false);
        enterEditMode(false, $('[data-id=headerButtons]'), false);
    }
}

function cancel() {
    var msg = globalVariables.cancelMsg;

    confirmAction(msg).done(function (pressOk) {
        if (pressOk) {
            exitEditMode(false, $('#PageContent'), false, true);
            exitEditMode(false, $('[data-id=headerButtons]'), false, true);
            $('.NotYetDelegatedOperation').find('input:first:checked').click();
            $('.NotYetDelegatedOperation').find('input:first').attr("disabled", "disabled");
        }
    });
}

function cancelNew() {
    var msg = globalVariables.cancelMsg;
    confirmAction(msg).done(function (pressOk) {
        if (pressOk) {
            showLoader();
            window.location.href = globalVariables.urlCancelButton;
            separateColumnIntoSeveralDivs();
            operationTypeClick();
        }
    });
}

function save() {
    if (!validateSameAuthorizerAssigned()) {
        return false;
    }

    if ($('#AuthorizationTable tbody tr').length === 0) {
        showMessage(globalVariables.msgNoRole);
        return false;
    }

    var hasValidatedOtherReasonComment = validateOtherReasonComment();
    errorDate($('[name="startDate"]'));
    errorDate($('[name="endDate"]'));

    var delegatorUser = $("input[name='User']").val();
    if (delegatorUser != null && delegatorUser.trim() != '')
    {
        delegatorUser = delegatorUser.toUpperCase();
    }

    if (hasValidatedOtherReasonComment && Validation.Container()
        && $("span.validation-element:not(.hide)").length == 0) {
        var existDocument = $('#DocumentsTable > tbody > tr:eq(0)').length > 0;
        if (globalVariables.currentUser.toUpperCase() != delegatorUser && existDocument === false)
        {
            Confirm.ShowInfo(globalVariables.msgDocumentAdmin, "", "", "", true, { cancel: false });
            return false;
        }
        else {
            var comments = $("textarea[name = 'commentComment']:not([disabled = 'disabled'])");
            var commentsArray = [];
            $.each(comments, function (key, value) {
                commentsArray.push($(this).text());
            });

            var commentsId = $("input[name = commentCommentId]");
            var commentsIdArray = [];
            $.each(commentsId, function (key, value) {
                commentsIdArray.push($(this).val());
            });

            var commentUsers = $("input[name = commentUser]");
            var commentUsersArray = [];
            $.each(commentUsers, function (key, value) {
                commentUsersArray.push($(this).val());
            });
            console.log(converToDate($('[name="startDate"]').val()));
            console.log(converToDate($('[name="startDate"]').val()).toUTCString());
            var userToAssign = {
                AssignedUserName: $('[name="SearchDelegateUser"]').val(),
                StartDate: $.datepicker.formatDate("yy-mm-dd", converToDate($('[name="startDate"]').val())),
                EndDate: $.datepicker.formatDate("yy-mm-dd", converToDate($('[name="endDate"]').val())),
                Reason: $('[name="delegationReason"]').val(),
                Type: $('[name="delegationType"]').val()
            }

            var documents = [];
            fillDocuments(documents);
            var delegatorUserName = "";
            if ($('[name="User"]').length > 0 && globalVariables.isAdminUser === 'True') {
                delegatorUserName = $('[name="User"]').val();
            }
            else {
                delegatorUserName = globalVariables.currentUser;
            }

            var commentsObj = [];
            for (i = 0; i < comments.length; i++) {
                commentsObj.push({
                    CommentId: commentsIdArray[i],
                    User: commentUsersArray[i],
                    Comment: commentsArray[i],
                    Date: Date()
                });
            }

            var model = {
                UserName: delegatorUserName,
                UserToAssign: userToAssign,
                CommentsDelegation: commentsObj,
                TableDocument: documents,
                DelegationId: globalVariables.delegationId
            }

            $.post(globalVariables.urlSave, { model: model }, function (response) {
                if (response.IsValid) {
                    window.setTimeout(function () {
                        showLoaderOptional();
                        window.location.href =
                            globalVariables.urlAllAuthorization +
                            '?delegationId=' +
                            response.DelegationId;
                    },
                                500);
                } else {
                    Alert.ShowError(response.ErrorMessage);
                }
            }).fail(function () {
                alert("error");
            });
        }
    }
}

function newComment() {
    postUrlWithOptions(
            globalVariables.newCommentUrl,
            { async: false })
        .done(function (data) {
            var container = $('#UserCommentFields');
            $(container).append(data);
            bindHandlers();
            enterEditMode(false, container, false);
            Validation.Init($('#UserCommentFields'));
        });
}

function removeRowComment(source) {
    $(source).closest('.rowComment').remove();
}

function validateOtherReasonComment() {
    var reasonId = $("#delegationReason").val();
    var reasonText = $("a[dd-value = '" + reasonId + "']").text();
    if (reasonText == globalVariables.otherReasonId) {
        var commentNumber = $("textarea[name='commentComment']").length;
        if (commentNumber <= 0) {
            Confirm.ShowInfo(globalVariables.otherReasonDocument, "", "", "", true, { cancel: false });
            return false;
        }
    } else {
        var comments = $("textarea[name='commentComment']");
        recursiveCommentRemove(comments)
    }
    return true;
}

function recursiveCommentRemove(comments) {
    if (comments.length > 0) {
        for (var i = 0; i < comments.length; i++) {
            if ($("textarea[name='commentComment']")[i].textContent.length == 0) {
                removeRowComment(comments[i]);
                comments = $("textarea[name='commentComment']");
                recursiveCommentRemove(comments);
                break;
            }
        }
    }
}

function validateSameAuthorizerAssigned() {
    var AuthorizerName = $("input[name='User']").val();
    var AssignedName = $("input[name='SearchDelegateUser']").val();

    if (AuthorizerName.length === 0) {
        return Validation.Container($('#authorizeAllFilter'));
    }

    if (AuthorizerName === AssignedName) {
        cleanDropDownAjaxAll($("input[name='SearchDelegateUser']"));
        showMessage(globalVariables.msgSameAuthorizer);
        return false;
    }
    return true;
}

function fillDocuments(documentArray) {
    var documentTable = $('#DocumentsTable > tbody > tr');
    documentTable.each(function (index, element) {
        var documentAllAuth = {
            DocumentId: $(element).find("[name=DocumentId]").val(),
            User: globalVariables.currentUser,
            DocNumber: $(element).find("[name=DocNumber]").val(),
            Description: $(element).find("[name=Description]").val()
        }
        documentArray.push(documentAllAuth);
    });
}

function removeRowDocument(source) {
    var row = source.closest("tr");
    row.remove();
    var value = $('[name="ddlDocumentsPagination"]').val();
    if (value === "All") {
        value = $("#DocumentsTable tbody tr").length;
    }
    $("#DocumentsTable").paginationConfluense(parseInt(value));
}

function ToJavaScriptDate(value) {
    var pattern = /Date\(([^)]+)\)/;
    var results = pattern.exec(value);
    return new Date(parseFloat(results[1]));
}