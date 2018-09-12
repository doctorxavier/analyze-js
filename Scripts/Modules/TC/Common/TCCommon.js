
function retrySendEmail() {
    var response = $.Deferred();
    var modalContent = $('.new-modal-content');
    var taskStatusContainer = modalContent.find('#tcTaskStatus');
    var emailTitle = modalContent.find('#emailTitle').text();
    var emailButton = modalContent.find('.button01');
    var emailMsg = modalContent.find('#emailErrorMsg');
    var emailURL = taskStatusContainer.find('.new-modal-buttons-container').data('url');

    taskStatusContainer.attr('data-url', emailURL);
    taskStatusContainer.attr('data-parsley-validate', 'data - parsley - validate');
    taskStatusContainer.attr('data-parsley-excluded', '[disabled]');
    taskStatusContainer.attr('data-ignore-nullable-values', 'true');
    taskStatusContainer.attr('data-pagemode', 'edit');

    responseSend = saveContainer(
        taskStatusContainer,
        null,
        false,
        null,
        null,
        false,
        false,
        modalContent);

    if (responseSend !== false) {
        responseSend.done(function (data) {
            if (data.IsValid) {
                emailStatus = data.ValidMessage;
                emailButton.remove();
            }
            else {
                emailStatus = data.ErrorMessage;
            }
            emailMsg.text(emailTitle + " " + emailStatus);
        });
    }

    return response;
}
