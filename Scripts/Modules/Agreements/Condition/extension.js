$(document).ready(function () {

    EditExtensionCalculoFecha();

    Date.prototype.getMonthName = function () {
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                      "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
        return monthNames[this.getMonth()];
    }

    $('#requestedExpirationDate').text($('#currentExpirationDate').val());

    $('#idbRequest').on("focusout", function () {
        if ($('#idbRequest').val() > 0) {

            var IdRequest = $('#idbRequest').val();

            var currentExpirationDate = new Date($('#CurrentExpirationDate').val());
            currentExpirationDate.setMonth(currentExpirationDate.getMonth() + parseInt(IdRequest));

            var RequestExpirationDateMonthName = new Date(currentExpirationDate).getMonthName();
            var RequestExpirationDateDay = currentExpirationDate.getDate();
            var RequestExpirationDateYear = currentExpirationDate.getFullYear();

            var NewRequestedDate = RequestExpirationDateDay + " " + RequestExpirationDateMonthName + " " + RequestExpirationDateYear;
            $('#requestedDate').text(NewRequestedDate);
        }
        else if ($('#idbRequest').val() == 0) {
            var RequestExpirationDate = new Date($('#CurrentExpirationDate').val());

            var RequestExpirationDateMonthName = new Date(RequestExpirationDate).getMonthName();
            var RequestExpirationDateDay = RequestExpirationDate.getDate();
            var RequestExpirationDateYear = RequestExpirationDate.getFullYear();

            var NewRequestedDate = RequestExpirationDateDay + " " + RequestExpirationDateMonthName + " " + RequestExpirationDateYear;
            $('#requestedDate').text(NewRequestedDate);
        }

    });

    //Mensaje tooltip de error para validaciones
    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $("#target").validate();

    jQuery.validator.addMethod("input", function (value, element) {
        if ($(element).attr("id") == "eachMonths" || $(element).attr("id") == "monthsDirectionFrom" || $(element).attr("id") == "monthsDirectionTo") {
            var ValueMonths = parseInt(value);

            if (ValueMonths.toString() == "NaN") {
                $(element).attr('data-val-required', "Please, fill in the required fields");
                return false;
            }
            else {
                if ((value % 1) != 0) {
                    $(element).attr('data-val-required', "Please enter only whole numbers");
                    return false;
                }
                else if (value <= 0) {
                    $(element).attr('data-val-required', "Please, the value should be bigger to 0");
                    return false;
                }
            }
        }
        return true;
    });

    $('.cancelEditCondition').on('click', function () {
        redirectPage($(this).data('route'));
    });
});

function EditExtensionCalculoFecha() {
    if ($('#idbRequest').val() > 0) {

        var IdRequest = $('#idbRequest').val();

        var currentExpirationDate = new Date($('#CurrentExpirationDate').val());
        currentExpirationDate.setMonth(currentExpirationDate.getMonth() + parseInt(IdRequest));

        var RequestExpirationDateMonthName = new Date(currentExpirationDate).getMonthNameEdit();
        var RequestExpirationDateDay = currentExpirationDate.getDate();
        var RequestExpirationDateYear = currentExpirationDate.getFullYear();

        var NewRequestedDate = RequestExpirationDateDay + " " + RequestExpirationDateMonthName + " " + RequestExpirationDateYear;
        $('#requestedDate').text(NewRequestedDate);
    }
    else if ($('#idbRequest').val() == 0) {
        var RequestExpirationDate = new Date($('#CurrentExpirationDate').val());

        var RequestExpirationDateMonthName = new Date(RequestExpirationDate).getMonthNameEdit();
        var RequestExpirationDateDay = RequestExpirationDate.getDate();
        var RequestExpirationDateYear = RequestExpirationDate.getFullYear();

        var NewRequestedDate = RequestExpirationDateDay + " " + RequestExpirationDateMonthName + " " + RequestExpirationDateYear;
        $('#requestedDate').text(NewRequestedDate);
    }
}

Date.prototype.getMonthNameEdit = function () {
    var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                  "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
    return monthNames[this.getMonth()];
}

function deleteItem(element) {
    confirmAction(MessageTranslation._texts['deleteConditionMessage'])
        .done(function (value) {
            if (value) {
                var object = {};

                AjaxCall(object, $(element).data('route'));

                return false;
            }
        });
}