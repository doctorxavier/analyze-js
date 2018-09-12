$(document).ready(function () {

    $('button[name="saveTrackWithDraw"]').on('click', function () {
        var form = $("#target");

        if (!form.valid()) {
            form.validate().focusInvalid();

            return;
        }

        SubmitForm(form);
    });

    $('button[name="cancelTrackWithDraw"]').on('click', function () {
        redirectPage($(this).data("route"));
    });

    $('button[name="deleteTrackWithDraw"]').on('click', function () {

        confirmAction(MessageTranslation._texts['deleteConditionMessage'])
            .done(function (value) {
                if (value) {
                    AjaxCall(object, valueAction[1]);
                    return false;
                }
            });
    });

    var grid2 = new GridComponent(".grid2", 20, false, true);

    

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });
});