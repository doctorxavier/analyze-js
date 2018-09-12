var MessageTranslation = new Object();

MessageTranslation._texts = {};
MessageTranslation._texts['fulfillmentDateIsDifferentToTheExpirationDate'] = null;
MessageTranslation._texts['includeStatementPresentation'] = null;
MessageTranslation._texts['okButton'] = null;
MessageTranslation._texts['cancelButton'] = null;

$(document).ready(function () {
    $("input[type='checkbox'],input[type='radio']").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });

    $(".optionSelect").kendoDropDownList();

    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });

    $("#fulfilmentDate").on("focusout", function () {
        value = $('#fulfilmentDate').val();
        ValidarFecha(value, $("#fulfilmentDate"));
    });

    $(".save").on("click", function () {
        var form = $('#target');

        if (!form.valid()) {
            form.validate().focusInvalid();

            return;
        }

        var currentExpirationDate = $('#CurrentExpirationdate').val();
        var fulfilmentDate = $('#fulfilmentDate').val();

        if (currentExpirationDate != null && fulfilmentDate != null) {
            var currentDate = new Date(currentExpirationDate).toLocaleDateString("pt-BR");
            var fullfillmentDate = new Date(fulfilmentDate).toLocaleDateString("pt-BR");

            if (currentDate != fullfillmentDate) {
                confirmAction(MessageTranslation._texts['fulfillmentDateIsDifferentToTheExpirationDate'])
                    .done(function (value) {
                        if (value) {
                            CheckIncludeFinancialStatementAndSubmit();
                        }
                    });
            }
            else {
                CheckIncludeFinancialStatementAndSubmit();
            }
        }
        else {
            CheckIncludeFinancialStatementAndSubmit();
        }
    });

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $('.k-dropdown-wrap.k-state-default').addClass('btn');
});

var CheckIncludeFinancialStatementAndSubmit = function () {
    confirmActionInformation(
        MessageTranslation._texts['includeStatementPresentation'],
        MessageTranslation._texts['okButton'],
        MessageTranslation._texts['cancelButton'])
    .done(function (value) {
        if (value) {
            $('#IncludeFinancialStatements').val('true');
        }

        idbg.lockUi(null, true);
        $("#target").submit();
    });
}

MessageTranslation.text = function (key, value) {
    MessageTranslation._texts[key] = value;
};

function ValidarFecha(value, element) {
    var validator = this;
    var datePat = /^(\d{1,2})(\/|-)(\d{1,2})(\/|-)(\d{4})$/;

    var FechaActual = new Date().toLocaleString("pt-BR");
    var FechaActualSplit = FechaActual.toString().split(" ");
    var FechaActualvalue = FechaActualSplit[0].match(datePat);

    var DiaActual = FechaActualvalue[1];
    var MesActual = FechaActualvalue[3];
    var AnioActual = FechaActualvalue[5];

    var fechaCompleta = value.match(datePat);

    if (fechaCompleta == null) {

        var Fecha = value.toString().split(" ");

        if (Fecha.length == 3) {
            fechaCompleta = (new Date(value)).toLocaleDateString("pt-BR").match(datePat);

            if (fechaCompleta == null) {

                $("#" + $(element).attr("id")).kendoDatePicker({
                    value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
                    format: "dd MMM yyyy",
                    defaultValue: FechaActualSplit[0]
                });

                return;
            }
            else {
                dia = fechaCompleta[1];
                mes = fechaCompleta[3];
                anio = fechaCompleta[5];
            }
        }
        else {
            if (Fecha[0] == "") {
                return;
            }

            $("#" + $(element).attr("id")).kendoDatePicker({
                value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
                format: "dd MMM yyyy",
                defaultValue: FechaActualSplit[0]
            });
            return;
        }
    }
    else {
        dia = fechaCompleta[1];
        mes = fechaCompleta[3];
        anio = fechaCompleta[5];
    }

    //El valor del día debe estar comprendido entre 1 y 31.
    if (dia < 1 || dia > 31) {
        $("#" + $(element).attr("id")).kendoDatePicker({
            value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
            format: "dd MMM yyyy",
            defaultValue: FechaActualSplit[0]
        });
        return;
    }

    //El valor del mes debe estar comprendido entre 1 y 12.
    if (mes < 1 || mes > 12) {
        $("#" + $(element).attr("id")).kendoDatePicker({
            value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
            format: "dd MMM yyyy",
            defaultValue: FechaActualSplit[0]
        });
        return;
    }

    //El mes debe tener 31 días
    if ((mes == 4 || mes == 6 || mes == 9 || mes == 11) && dia == 31) {
        $("#" + $(element).attr("id")).kendoDatePicker({
            value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
            format: "dd MMM yyyy",
            defaultValue: FechaActualSplit[0]
        });
        return;
    }

    //El mes debe tener 28 dias
    if (mes == 2) {
        // bisiesto
        var bisiesto = (anio % 4 == 0 && (anio % 100 != 0 || anio % 400 == 0));
        if (dia > 29 || (dia == 29 && !bisiesto)) {
            $("#" + $(element).attr("id")).kendoDatePicker({
                value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
                format: "dd MMM yyyy",
                defaultValue: FechaActualSplit[0]
            });
            return;
        }
    }

    $("#" + $(element).attr("id")).kendoDatePicker({
        value: new Date(mes + "/" + dia + "/" + anio),
        format: "dd MMM yyyy",
        defaultValue: value
    });
}
