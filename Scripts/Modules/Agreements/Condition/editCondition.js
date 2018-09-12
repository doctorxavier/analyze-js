function deleteItem(element) {
    var route = $(element).data('route');
    confirmAction(MessageTranslation._texts['deleteConditionMessage'])
        .done(function (value) {
            if (value) {
                var object = {};

                AjaxCall(object, route);

                return false;
            }
        });
}

$(document).ready(function () {

    $(".optionSelect").kendoDropDownList();
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);

    var dialog = null;

    $("#datePicker0").kendoDatePicker({
        format: "dd MMM yyyy"
    });

    

    $("#cancelAction").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        dialog = dialog.close();
        $("#centralSection").append(dialog);
    });

    $("#deleteAction").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        redirectPage($(this).data("route"));
    });

    $('.actionDeleteClause').click(function () {
        var title = "Warning";
        var myWin = $("#confirmWindow");

        if (!myWin.data("kendoWindow")) {
            // window not yet initialized
            myWin.kendoWindow({
                width: "auto",
                height: "auto",
                modal: false,
                resizable: true,
                title: title,
                content: $("#confirmWindow").html()
            });
        } else {
            // reopening window
            myWin.data("kendoWindow")
                .content("Loading...") // add loading message
                .refresh($("#confirmWindow").html()) // request the URL via AJAX
                .open(); // open the window
        }
    });

    if ($(window.parent.document).find('body').find(".ui-widget-overlay")) {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    }
    if ($(".ui-widget-overlay") != null) {
        $(".ui-widget-overlay").remove();
    }
    if ($(".k-window") != null) {
        $(".k-window").remove();
    }
    if ($(".k-window-titlebar") != null) {
        $(".k-window-titlebar").remove();
    }

    $("#datePicker0").on("focusout", function () {
        value = $('#datePicker0').val();
        ValidateDate(value, $("#datePicker0"));
    });

    //Mensaje tooltip de error para validaciones
    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $("#target").validate();

    jQuery.validator.addMethod("txtDescriptionComent", function (value, element) {
        if (value.toString() != "") {
            if ($(element).val().toString().trim().length == 0) {
                return false;
            }
        }
        else {
            return false;
        }
        return true;
    });
});

$("#target").submit(function () {
    var form = $("#target")
   .removeData("validator") /* added by the raw jquery.validate plugin */
   .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */
    $.validator.unobtrusive.parse(form);

    return true;
});

function ValidateDate(value, element) {
    var validator = this;
    var datePat = /([0-9]{2})(.*?)([0-9]{2})(.*?)([0-9]{4})/i;
    var actualDate = new Date().toLocaleString("pt-BR");
    var splitActualDate = actualDate.toString().split(" ");
    var actualDateValue = splitActualDate[0].match(datePat);
    var actualDay = actualDateValue[1];
    var actualMonth = actualDateValue[3];
    var actualYear = actualDateValue[5];

    var actualDateSplit = String(actualDay) + "/" + String(actualMonth) + "/" + String(actualYear);
    var completeDate = value.match(datePat);

    if (completeDate == null) {
        var dateVal = value.toString().split(" ");
        if (dateVal.length == 3) {
            completeDate = (new Date(value)).toLocaleDateString("pt-BR").match(datePat);

            if (completeDate == null) {
                $(element).kendoDatePicker(
                {
                    value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                    format: "dd MMM yyyy",
                    defaultValue: actualDateSplit[0]
                });
                return;
            }
            else {
                day = completeDate[1];
                month = completeDate[3];
                year = completeDate[5];
            }
        }
        else {
            if (dateVal[0] == "") {
                return;
            }

            $(element).kendoDatePicker(
            {
                value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                format: "dd MMM yyyy",
                defaultValue: actualDateSplit[0]
            });
            return;
        }
    }
    else {
        day = completeDate[1];
        month = completeDate[3];
        year = completeDate[5];
    }

    if (day < 1 || day > 31) {
        $(element).kendoDatePicker(
        {
            value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
            format: "dd MMM yyyy",
            defaultValue: actualDateSplit[0]
        });
        return;
    }

    if (month < 1 || month > 12) {
        $(element).kendoDatePicker(
        {
            value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
            format: "dd MMM yyyy",
            defaultValue: actualDateSplit[0]
        });
        return;
    }

    if ((month == 4 || month == 6 || month == 9 || month == 11) && day == 31) {
        $(element).kendoDatePicker(
            {
                value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                format: "dd MMM yyyy",
                defaultValue: actualDateSplit[0]
            });
        return;
    }

    if (month == 2) {
        var bisiesto = (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0));
        if (day > 29 || (day == 29 && !bisiesto)) {
            $(element).kendoDatePicker(
            {
                value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                format: "dd MMM yyyy",
                defaultValue: actualDateSplit[0]
            });
            return;
        }
    }

    $(element).kendoDatePicker(
    {
        value: new Date(month + "/" + day + "/" + year),
        format: "dd MMM yyyy",
        defaultValue: value
    });
}

var functionCancel = function () {
    event.preventDefault ? event.preventDefault() : event.returnValue = false;
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();

};