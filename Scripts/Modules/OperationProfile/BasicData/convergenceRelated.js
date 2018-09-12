$(document).ready(function () {

    var grid = new GridComponent(".grid2", 20, false);

    $(".datepicker1").kendoDatePicker({
        value: new Date(Date.now()),
        format: "dd MMM yyyy",
        defaultValue: new Date(Date.now())
    });

    $("#RelationDate").on("focusout", function () {
        value = $('#RelationDate').val();
        ValidarFecha(value, $("#RelationDate"));
    });

    //init config  

    $('.buscadorConvergence').hide();
    $('.table_search_item').hide();
    $('.pie.top').hide();
    $('.cancelConfirm').hide();
    $('.pie.bottom').hide();
    $('.RelationDateClass').hide();

    //events handlers

    $('.newIndicator').click(function () {
        $('.table_search_item').hide();
        $('.RelationDateClass').hide();
        $('.buscadorConvergence').show();
    });

    $('.botones .searchItem').click(function () {

        if ($('#input7').val().length > 0) {

            $.ajaxSetup({
                // Disable caching of AJAX responses
                cache: false
            });
            $("#resultContent").empty();
            $.get($("#searchRelatedOperation").val(), { OperationId: $('#hdnOperationID').val(), searchKeyword: $('#input7').val() })
              .done(function (data) {
                  $("#resultContent").html(data);
                  $('.table_search_item').show();
                  $('.RelationDateClass').show();
                  $('.pie.bottom').show();
                  $.ajaxSetup({
                      // Disable caching of AJAX responses
                      cache: true
                  });
              });
        }
    });

    $('.closeSearch').click(function () {
        $('.pie.bottom').hide();
        $('.table_search_item').hide();
        $('.RelationDateClass').hide();
        $('.buscadorConvergence').hide();
        $('.pie.top').hide();
    });

    $('.cancel').click(function () {
        $('.mod_tabla.mod_tabla_agrup').show();
        $('.buscadorConvergence').hide();
        $('.table_search_item').hide();
        $('.RelationDateClass').hide();
        $('.pie.top').hide();
        $('.cancelConfirm').hide();
        $('.pie.bottom').hide();
    });

    $('.btn_delete').click(function () {
        $('#hdnOperationToDelete').val($(this).data('operationid'));
        $('.mod_tabla.mod_tabla_agrup').hide();
        $('.pie.bottom').hide();
        $('.table_search_item').hide();
        $('.RelationDateClass').hide();
        $('.buscadorConvergence').hide();
        $('.cancelConfirm').show();
        $('.pie.top').show();
    });

    $('.closeConfirm').click(function () {
        $.post($("#deleteRelatedOperation").val(),
            { OperationId: $('#hdnOperationToDelete').val() }, function (data) {
                var dialog = $(".dinamicModal").data("kendoWindow");
                $.ajaxSetup({
                    // Disable caching of AJAX responses
                    cache: false
                });

                $.get($("#indexRelatedOperation").val(), { operationID: $('#hdnOperationID').val() })
                .done(function (data) {
                    dialog.content(data);
                    $.ajaxSetup({
                        // Disable caching of AJAX responses
                        cache: false
                    });
                });

            });
    });

    $('.k-window-actions').click(function () {
        $('.buscadorConvergence').hide();
        $('.table_search_item').hide();
        $('.RelationDateClass').hide();
        $('.pie.bottom').hide();
        $('.pie.top').hide();
        $('.cancelConfirm').hide();
        $('.mod_tabla.mod_tabla_agrup').show();
    });

    $('#btnAdd').click(function () {

        var newIdOperation = $('input:radio[name=Operation]:checked').val();

        if (newIdOperation != null) {

            $.post($("#createRelatedOperation").val(),
                {
                    OperationId: $('#hdnOperationID').val(), NewOperationIdChild: newIdOperation, RelationDate: $('#RelationDate').val()
                }, function (data) {
                    if (data != null) {
                        var Mensaje = data.Item1;
                        var OperationIdChild = data.Item2;

                        if (Mensaje == "Save") {
                            $("#resultContent").empty();
                            $('.cancelConfirm').hide();
                            $('.pie.bottom').hide();
                            $('.RelationDateClass').hide();

                            var dialog = $(".dinamicModal").data("kendoWindow");
                            $.ajaxSetup({
                                // Disable caching of AJAX responses
                                cache: false
                            });

                            $.get($("#indexRelatedOperation").val(), { operationID: $('#hdnOperationID').val() })
                            .done(function (data) {
                                dialog.content(data);
                                $.ajaxSetup({
                                    // Disable caching of AJAX responses
                                    cache: false
                                });
                            })
                        }
                        else {
                            var AltoTabla = $('.table_search_item').height();
                            $("input[value='" + OperationIdChild + "']").attr('Title', Mensaje);
                            $("input[value='" + OperationIdChild + "']").tooltip({ show: { effect: "blind", duration: 800 } });
                            $("input[value='" + OperationIdChild + "']").tooltip("option", "position", { my: "left+40% bottom+" + AltoTabla });
                            $("input[value='" + OperationIdChild + "']").focus();
                        }
                    }
                });
        }
    });
})

//Validacion de Fechas: Fecha valida 
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
                $("#" + $(element).attr("id")).kendoDatePicker({
                    value: new Date(MesActual + "/" + DiaActual + "/" + AnioActual),
                    format: "dd MMM yyyy",
                    defaultValue: FechaActualSplit[0]
                });
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

    return;
}
