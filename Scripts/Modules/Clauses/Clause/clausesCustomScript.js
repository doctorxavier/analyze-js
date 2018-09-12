var SelectedRequest = "Month";

$(document).on("ready", function () {
    $("#AlertDateLess").hide(); 

    $("#target").on("submit", function (e) {
        if (SelectedRequest == "Month") {
            if ($("#idbRequest").val() != null &&
                $("#idbRequest").val() != "" &&
                $("#executorRequestedMonths").val() != null &&
                $("#executorRequestedMonths").val() != "") {

                $("#RequestExtensionDate").val();
            }
        }
        else if (SelectedRequest == "Date") {
            if ($("#RequestExtensionDate").val() != null &&
                $("#RequestExtensionDate").val() != "") {

                $("#idbRequest").val();
                $("#executorRequestedMonths").val();
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

        if ($('#Is_Special_Extension_Id').is(':checked') && !$('#TypeRequest_1').is(':checked')) {
            var specialExtensionMonths = $('#Is_Special_Extension_Id').attr('data-value');
            $('#idbRequest, #executorRequestedMonths').val(specialExtensionMonths)
        }
    });

    $("#TypeRequest_2Label").on("click", function () {
        if ($('#TypeRequest_2').is(':checked')) {
            return;
        }

        loadRequestByDate();
    });
});

(function ($)
{
    $(document).ready(function () 
    {
        $('.kendoDatePicker').kendoDatePicker({
            format: "dd MMM yyyy"
        }).data("kendoDatePicker");

        $(document).tooltip({
            items: ".input-validation-error",
            content: function ()
            {
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
    });
})(window.jQuery);

function loadRequestByMonths() {
    $("#dateRequestSelected").hide();
    $("#monthsSelected").show();

    $("#idbRequest").val($("#ExistIdbRequest").val());
    $("#executorRequestedMonths").val($("#ExistExecutorRequest").val())
    $("#RequestExtensionDate").val($("#ExistRequestExtensionDate").val());

    SelectedRequest = "Month";
}

function loadRequestByDate() {
    $("#monthsSelected").hide();
    $("#dateRequestSelected").show();

    $("#RequestExtensionDate").val($("#ExistRequestExtensionDate").val());
    $("#idbRequest").val($("#ExistIdbRequest").val());
    $("#executorRequestedMonths").val($("#ExistExecutorRequest").val())

    SelectedRequest = "Date";
}

var sumbitaction = function () {
    if (SelectedRequest == "Month") {
        return true;
    } else {
        var reqExtDateItem = $("#RequestExtensionDate");
        var requestExtensionDateStringFormat = $(reqExtDateItem).val();
        var requestExtensionDate = new Date(requestExtensionDateStringFormat);
        var today = new Date();
        if (requestExtensionDate <= today) {
            $(reqExtDateItem).attr('data-val-required', "This date may not be before today");
            $(reqExtDateItem).addClass("input-validation-error");
            $(reqExtDateItem).focus();
            idbg.lockUi(null, false); ///<--- remove in refactor of Clauses frontend; we should call "submitAction" (notice correct name of this function) before locking UI
            return false;
        } else {
            idbg.lockUi(null, true);
            return true;

        }
    }
};