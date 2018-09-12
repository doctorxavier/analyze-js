$(document).ajaxError(function () {
    if (typeof hideLoaderOptional !== 'undefined' && $.isFunction(hideLoaderOptional)) {
        hideLoaderOptional();
    }
});

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

$(document).ready(function () {

    $(".optionSelect").kendoDropDownList();
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);

    var dialog = null;

    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });

    $(document).on('submit', function () {
        showLoaderOptional();
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

function warningMandatoryFulfillment() {
    var dateSubmissionDate = $("#datePicker0").val();
    var values = $('#valuesClauseExpired');

    if (values.data('isotherpcategoryandexpired') === 'True' && !(dateSubmissionDate === null || dateSubmissionDate === '')) {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        confirmActionCustom(values.data('msg'), values.data('txtcancel'), values.data('txtcontinue'))
            .done(function (pressOk) {
                if (pressOk) {
                    redirectPage(values.data('routecreateextension'));
            }
        });

        return false;
    }

    if (dateSubmissionDate === null || dateSubmissionDate === '') {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        confirmActionCustom(values.data('mandatorysubmissiondatemsg'), null, 'OK');
    }
    else {
        var isPBPOrPBLAndOA420Operation = isPBPOrPBLAndOA420OperationTrench;
        if (isPBPOrPBLAndOA420Operation) {
            validationHasTrenchClause()
        } else {
            $('.ClauseFulfilled').removeAttr('onclick').click();
        }
    }
}

function validationHasTrenchClause() {
    var hasMilestonTypeTrenchClause = hasMilestonTypeTrenchClauses;

    if (hasMilestonTypeTrenchClause) {
        WarningHasMilestonTrenchForOperation();
    } else {
        $('.ClauseFulfilled').removeAttr('onclick').click();
    }  
}
var functionCancel = function () {
    event.preventDefault ? event.preventDefault() : event.returnValue = false;
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();

};

function WarningHasMilestonTrenchForOperation() {
    event.preventDefault();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"></div>');
    $("#advMilestonTrenchOperation").appendTo(".dinamicModal").removeClass("hide");
    var title = $("#advMilestonTrenchOperation").data("title");
    var modal = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        activate: function () {
            $("#ConfirmadvMilestonTrenchOperation").click(functionCancelPBPPBL);
            $("#CancelMilestonTrenchOperation").click(functionCancel);
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $("#advMilestonTrenchOperation").appendTo("#ui_sp_001").addClass("hide");
            $("body").find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    modal.center();
    modal.open();

}
var functionCancelPBPPBL = function () {
    event.preventDefault();
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();

    $('.ClauseFulfilled').removeAttr('onclick').click();

};

function warningIsOtherPCategoryAndExpired() {
    var dateSubmissionDate = $("#datePicker0").val();
    var values = $('#valuesClauseExpired');

    if (values.data('isotherpcategoryandexpired') === 'True' &&
        !(dateSubmissionDate === null || dateSubmissionDate === '')) {

        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        confirmActionCustom(values.data('msg'), values.data('txtcancel'), values.data('txtcontinue'))
            .done(function (pressOk) {
                if (pressOk) {
                    redirectPage(values.data('routecreateextension'));
                }
            });

        return false;
    }
}