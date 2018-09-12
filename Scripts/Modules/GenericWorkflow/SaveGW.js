var SaveGW = {};

SaveGW.data = {};
SaveGW.data['model'] = {};

SaveGW.saveData = function (url) {
    SaveGW.data['model']["DocumentNumbers"] = SaveGW.GetDocuments();
    SaveGW.data['model']["TaskId"] = $('#TaskId').val();
    SaveGW.data['model']["ChangeStatus"] = $("[name=changeStatus]").val();
    SaveGW.data['model']['Comments'] = SaveGW.GetComments();
    SaveGW.data['model']['DeleteComments'] = SaveGW.GetDeleteComments();
    SaveGW.data['model']['AdditionalValidators'] = SaveGW.GetAdditionalValidators();
    SaveGW.data['model']['ValidatorViewModel'] = SaveGW.GetHideValidators();
    SaveGW.data['model']['WorkflowInstanceId'] = $('#workflowInstanceId').val();

    showLoader();
    window.setTimeout(function () {
        $.ajax({
            'url': ValueObjGW.urlSaveGW,
            'type': 'post',
            'data': SaveGW.data['model'],
            'success': function (data) {
                if (!data.IsValid) {
                    showMessage(data.ErrorMessage);
                    return;
                }
                else
                {
                    window.location.href = url;
                }

                hideLoader();
            },
            'error': function () {
                hideLoader();
                showMessage("Error!");
            }
        });
    }, 5);
};


SaveGW.GetDocuments = function () {
    var documents = $('#tblWFDocuments').find('tbody tr td.docNumber label');
    var documentsNumber = [];

    $.each(documents, function () {
        var currentDocNumber = $(this).text();

        documentsNumber.push(currentDocNumber);
    });

    return documentsNumber;
};


SaveGW.GetComments = function () {
    var newComments = $('[name=newComment]');
    var comments = [];

    $.each(newComments, function () {
        var comment =
        {
            "CommentId": 0,
            "Text": $(this).text(),
            "WorkflowTaskId": ValueObjGW.WorkflowTaskId
        };
        comments.push(comment);
    });

    return comments;
};

SaveGW.GetDeleteComments = function () {
    var comments = $('[name=deleteComments]');
    if (typeof comments !== "undefined" && typeof comments.val() !== "undefined") {
        return comments.val().split('|').map(function (item) {
            var num = parseInt(item, 10);
            return isNaN(num) ? 0 : num;
        });
    }
    return [];
};

SaveGW.GetAdditionalValidators = function () {
    var userProfiles = $('[name=newUserProfile]');
    var additionalValidators = [];

    $.each(userProfiles, function () {
        var profile = $(this).val();
        additionalValidators.push(profile);
    });
    return additionalValidators;
};

SaveGW.HideValidator = function (obj) {
    obj.parent().parent().addClass('hide');
};

SaveGW.ShowValidator = function () {
    var validators = $('#tableWorkflowValidators').find('tr').not(':first');

    $.each(validators, function () {
        $(this).removeClass('hide');
    });
};

SaveGW.GetHideValidators = function () {
    var validators = $('#tableWorkflowValidators').find('tr.hide');
    var validatorsModel = [];

    $.each(validators, function () {
        var currentValidator = $(this);
        var validatorData = {};

        var taskName = currentValidator.find('td.taskName').text();
        var role = currentValidator.find('td.role').text();
        var type = currentValidator.find('td.type').data('type');

        validatorData =
        {
            "TaskName": taskName,
            "Role": role,
            "Mandatory": type
        };

        validatorsModel.push(validatorData);
    });

    return validatorsModel;
}
