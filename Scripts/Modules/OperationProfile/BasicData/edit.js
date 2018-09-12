/// <reference path="../../../jquery.validate.js" />
/// <reference path="../../../jquery.validate.unobtrusive.js" />


// custom validator 
$.validator.unobtrusive.adapters.addSingleVal("conditional", "otherproperty");

$.validator.addMethod("conditional", function (value, element, otherproperty) {

    if (otherproperty == "IsObjectiveReformulated") {
        var value2 = $('input[name=IsObjectiveReformulated]:checked', '#target').val();
        if (value2 == 'True' && value.length == 0) {
            return false;
        }
    }
    else if (otherproperty == "HasRetroactiveExpenses") {

        var value2 = $('input[name=HasRetroactiveExpenses]:checked', '#target').val();
        if (value2 == 'True' && value.length == 0) {

            return false;
        }
    }
    return true;
});

$.validator.addMethod("Retro", function (value, element) {
    var Element = $(element).attr('id');

    if (Element == "RetroactiveExpensesAmount") {
        var ValueRetroactiveExpensesAmount = value;
        if ($('#HasRetroactiveExpensesYes .checked').length == 1) {
            if (ValueRetroactiveExpensesAmount.trim().length == 0) {
                $(element).attr('data-val-number', "Please, complete the mandatory fields");
                return false
            }
            else {
                var Valor = parseInt(ValueRetroactiveExpensesAmount);

                if (Valor.toString() == "NaN") {
                    $(element).attr('data-val-number', "The field Retroactive expenses amount must be a number.");
                    return false
                }
                else if (Valor < 0) {
                    $(element).attr('data-val-number', "The field ActualValue must be between 0.00 and 9999999999.99.");
                    return false
                }
            }
        }
    }
    return true;
});


jQuery.validator.addMethod("Municipal", function (value, element) {
    if ($(element).attr("id") == "PlannedSupport" || $(element).attr("id") == "ActualSupport") {
        if (value.toString() != "") {
            var ValueMunicipal = parseInt(value);

            if (ValueMunicipal.toString() != "NaN") {
                if (ValueMunicipal >= 0) {
                    if ((value % 1) != 0) {
                        $(element).attr('data-val-range', "Please enter only whole numbers");
                        return false;
                    }
                }
                else {
                    $(element).attr('data-val-range', "Please enter only positive numbers");
                    return false;
                }
            }
            else {
                $(element).attr('data-val-range', "Please enter only positive numbers");
                return false;
            }

        }
    }

    return true;
});


$(window).load(function () {
    $("input[type='checkbox'],input[type='radio']").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });
});

// block the screen whe you save the operation profile info
$(document).ready(function () {

    //inputsDatepicker();
    $(".datepicker").kendoDatePicker({
        format: "MM/dd/yyyy"
    });

    $('form').submit(function (evt) {
        //evt.preventDefault();
        var $form = $(this);
        if ($form.valid()) {
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        }
    });


    $(".cancel").click(function () {
        redirectPage($(this).data("route"));
    });

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            if ($(this).attr('data-val-conditional'))
                return $(this).attr('data-val-conditional');
            if ($(this).attr('data-val-date'))
                return $(this).attr('data-val-date');
            if ($(this).attr('data-val-number'))
                return $(this).attr('data-val-number');
            if ($(this).attr('data-val-range'))
                return $(this).attr('data-val-range');
        }
    });

})

//Validacion Fecha mayor a leggal
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
                return "Letras";
            }
            else {
                dia = fechaCompleta[1];
                mes = fechaCompleta[3];
                anio = fechaCompleta[5];
            }
        }
        else {
            if (value == "") {
                return "Nula";
            }
            else {
                return "Letras";
            }
        }
    }
    else {
        dia = fechaCompleta[1];
        mes = fechaCompleta[3];
        anio = fechaCompleta[5];
    }

    //El valor del día debe estar comprendido entre 1 y 31.
    if (dia < 1 || dia > 31) {
        return "Incorrecta";
    }

    //El valor del mes debe estar comprendido entre 1 y 12.
    if (mes < 1 || mes > 12) {
        return "Incorrecta";
    }

    //El mes debe tener 31 días
    if ((mes == 4 || mes == 6 || mes == 9 || mes == 11) && dia == 31) {
        return "Incorrecta";
    }

    //El mes debe tener 28 dias
    if (mes == 2) {
        // bisiesto
        var bisiesto = (anio % 4 == 0 && (anio % 100 != 0 || anio % 400 == 0));
        if (dia > 29 || (dia == 29 && !bisiesto)) {
            return "Incorrecta";
        }
    }

    return;
}
$(document).ready(function () {


    //modal windows for basic data web page
    $('.lnkModal').click(function () {
        var Id = $(this).attr('id');
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: $(this).data("title"),
            draggable: false,
            resizable: false,
            content: $(this).data("route"),
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
                if (Id == "ConvergencerelatedOperation") {
                    redirectPage($("#IndexBasicDataUrl").val());
                }
            }
        }).data("kendoWindow");
        dialog.center();
        dialog.open();


        $("body").css("overflow", "hidden");
        var cerrar = $(".k-window-action");
        cerrar.click(function () {
            $(".k-window").hover(
                function () {
                    document.onmousewheel = document.onmousedown = wheel;
                    document.onkeydown = keydown;
                },
                function () {
                    document.onmousewheel = document.onmousedown = document.onkeydown = null;
                }
            );
            $("body").css("overflow", "");
            
        });
    });


   
});