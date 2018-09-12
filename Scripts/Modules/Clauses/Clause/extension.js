$(document).ready(function () {

    EditExtensionCalculoFecha();
    //inputsDatepicker();
    $(".datepicker").kendoDatePicker({
        format: "MM/dd/yyyy"
    });

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
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: $(element).data("route"),
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    dialog.center();
    dialog.open();
    resizeIframeCloud();
}