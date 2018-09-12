MessageTranslation._texts['changeJustification'] = null;
MessageTranslation._texts['addCommentRejectTask'] = null;
MessageTranslation._texts['removeMessage'] = null;
MessageTranslation._texts['actionNotValid'] = null;
MessageTranslation._texts['accept'] = null;

$(document).ready(function () {

    formatHeader();

    $('#ConvergencePartialForm').on('submit', function () {
        var form = $(this);

        if (!form.valid()) {
            form.validate().focusInvalid();

            return;
        }
    });

    jQuery.validator.addMethod("txtDescriptionComent", function (value, element) {
        if (value.toString() != "") {
            if ($(element).val().toString().trim().length == 0)
                return false;
        }
        else
            return false;

        return true;
    });

    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });

    
});

var formatHeader = function () {
    var header = $('#fullTaskContainer');
    buttons = header.find('.editingButtonsEdit');
    buttons.css('min-width', 0).css('margin-right', 0);
    buttons.find('input.btn-primary').removeClass('btn-primary').addClass('buttonBlue');
    buttons.find('input.btn-secondary').removeClass('btn-secondary').addClass('buttonWhite');
    buttons.find('a.verify').addClass('buttonLink');
    header.find('.operationData.dataDetails.marginTop2Bottom1').css('font-size', 'initial').css('width', '90%');
}

function showMessageValidation(messageCode) {
    if (!$(".txtDescriptionComent").valid())
        return;

    switch (messageCode) {
        case 1:
        case 2: customMessage = MessageTranslation._texts['changeJustification']; break;
        case 3:
        case 5: customMessage = MessageTranslation._texts['addCommentRejectTask']; break;
        case 4: customMessage = MessageTranslation._texts['removeMessage']; break;
        default: customMessage = MessageTranslation._texts['actionNotValid']; break;
    }

    confirmActionCustom(customMessage, null, MessageTranslation._texts['accept']);
}