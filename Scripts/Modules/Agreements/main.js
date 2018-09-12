$(document).ready(function() {

    $('.saveCondition').on('click', function () {
        var form = $("#target").removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);

        var frm = $('#target');
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
                }
            });

            ev.preventDefault();
        });
    });

});
