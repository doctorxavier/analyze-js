$("[name='btn-recalculate-name']").click(function () {

    confirmAction($("[name='translation-eop-recalculation-wrn-popup']").val())
        .done(function (isPressedOk) {

        if (isPressedOk) {
            ajaxRecalculateEOP();
        }
    });
});

function ajaxRecalculateEOP() {
    $.ajax({
        url: $("[name='url-eop-recalculation-async']").val(),
        dataType: "json",
        type: "GET",
        contentType: 'application/json',
        async: true,
        success: function (data) {
            
            if (data.IsValid) {
                showMessage("The EOP update process finished correctly.", 0, null, "Information");
            }
            else {
                showMessage(data.ErrorMessage, 0, null, "Error");
            }

        },
        error: function (xhr) {
            showMessage(xhr.responseText, 0, null, "Error");
        }
    });
}