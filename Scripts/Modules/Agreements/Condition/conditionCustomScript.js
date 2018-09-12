var SelectedRequest = "Month";

$(document).on("ready", function () {

    loadRequested();

    $('.kendoDatePicker').kendoDatePicker({
        format: "dd MMM yyyy"
    }).data("kendoDatePicker");

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            if ($(this).attr('data-val-required'))
                return $(this).attr('data-val-required');
            if ($(this).attr('data-val-date'))
                return $(this).attr('data-val-date');
            if ($(this).attr('data-val-number'))
                return $(this).attr('data-val-number');
            if ($(this).attr('data-val-range'))
                return $(this).attr('data-val-range');
        }
    });

    $("#target").on("submit", function (e) {
        showLoader();
        var executorRequestedMonths = $('#executorRequestedMonths');
        var idbRequest = $('#idbRequest');
        var requestedExtensionDate = $('#RequestedExtensionDate');

        idbRequest.prop('disabled', false);

        if (SelectedRequest == "Month") {
            if (idbRequest.val() != null && idbRequest.val() != "" &&
                executorRequestedMonths.val() != null && executorRequestedMonths.val() != "") {
                requestedExtensionDate.val();
            }
        }
        else if (SelectedRequest == "Date") {
            if (requestedExtensionDate.val() != null && requestedExtensionDate != "") {
                idbRequest.val();
                executorRequestedMonths.val();
            }
        }
        else {
            e.preventDefault();
        }

        return sumbitaction();
    });

    $("#TypeRequest_1Label").on("click", function () {
        if ($('#TypeRequest_1').is(':checked')) {
            return;
        }

        loadRequestByMonths();
    });

    $("#TypeRequest_2Label").on("click", function () {
        if ($('#TypeRequest_2').is(':checked')) {
            return;
        }

        loadRequestByDate();
    });
});

function loadRequestByMonths() {
    $("#dateRequestSelected").hide();
    $("#monthsSelected").show();

    $("#idbRequest").val($("#ExistIdbRequest").val());
    $("#executorRequestedMonths").val($("#ExistExecutorRequest").val())
    $("#RequestedExtensionDate").val($("#ExistRequestExtensionDate").val());

    SelectedRequest = "Month";
}

function loadRequestByDate() {
    $("#monthsSelected").hide();
    $("#dateRequestSelected").show();

    $("#RequestedExtensionDate").val($("#ExistRequestExtensionDate").val());
    $("#idbRequest").val($("#ExistIdbRequest").val());
    $("#executorRequestedMonths").val($("#ExistExecutorRequest").val())

    SelectedRequest = "Date";
}

function loadRequested() {
    if ($("#RequestedExtensionDate").val()) {
        loadRequestByDate();
    } else {
        loadRequestByMonths();
    }
}

var sumbitaction = function () {
    if (SelectedRequest == "Month") {
        return true;
    } else {
        var reqExtDateItem = $("#RequestedExtensionDate");
        var requestExtensionDateStringFormat = $(reqExtDateItem).val();
        var requestExtensionDate = new Date(requestExtensionDateStringFormat);
        var today = new Date();

        if (requestExtensionDate <= today) {
            $(reqExtDateItem).attr('data-val-required', MessageTranslation._texts['requestDateMessageToSave']);
            $(reqExtDateItem).addClass("input-validation-error");
            $(reqExtDateItem).focus();
            hideLoader();
            showMessage(MessageTranslation._texts['requestDateMessageToSave'], false);

            return false;
        } else {
            return true;
        }
    }
};