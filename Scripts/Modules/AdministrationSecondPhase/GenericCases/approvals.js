function changeTransactionType() {
    var $approvalDate = $('input[name="approval-date"][type="text"]');
    var $checkbox = $('input[name="update-approval-date"][type="checkbox"]');

    if ($('input#transaction-type-increase').prop("checked")) {
        $checkbox.siblings("span.checkbox-default-icon").removeClass("gc-disabled");
        $checkbox.prop("disabled", false);
    } else {
        $approvalDate.val("");
        $approvalDate.prop("disabled", true);
        $checkbox.siblings("span.checkbox-default-icon").addClass("gc-disabled");
        $checkbox.prop("disabled", true);
        $checkbox.prop("checked", false);
    }
}
function clearAll() {
    $('input[name="transaction-type"]').prop('checked', false);

    clearFields('#generic-case-fields');
}

$(document).ready(function () {
    var $increaseLabel = $('#transaction-type-increase').parent();
    var $transactionTypeError = $increaseLabel.prev('ul');

    if ($transactionTypeError.length > 0)
        $transactionTypeError.detach().insertAfter($increaseLabel);

    $('input[name="operation-number"]').on('change', function (event) {
        var operationId = $(event.target).GetValue();

        if ($.isNumeric(operationId)) {
            $.ajax({
                url: $('input[name="funds-name"]').data("url"),
                cache: false,
                data: { 'operationId': operationId },
                async: true,
                type: 'GET'
            }).done(function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage);

                    return;
                }

                addDataToDropdown(
                    response.DictionaryData, $('[aria-labelledby="id-funds-name"]'))
            });
        }
    });

    $(window).on('resize', function () {
        $('.adjustable-element').adjustWidth();
    });

    $('.adjustable-element').adjustWidth();

});
