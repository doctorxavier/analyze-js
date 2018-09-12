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

    idbg.lockUi(null, false);
}

function getvalue(select) {
    var optionSelected = $("#" + $(select).attr("id") + " option:selected").val();
    if (optionSelected == "0" || optionSelected == "Single") {
        // Ocultar
        $("#div_x").hide();
        $(".fromDateDependency").hide();

        if (parseInt($('#ClauseId').val()) != 0) {
            if ($('#ExpirationToDependencyId').val() != "" && $('#ExpirationToDependencyId').val() != "0") {
                $('#isDateDependencyTo').val("false");
                $("#getDependencydown").addClass('checked');
                $('#getdatedown').removeClass('checked');
                $("#muestradatedown").show();
                $("#muestratododown").hide();
            }
            else {
                $('#isDateDependencyTo').val("true");
                $("#getdatedown").addClass('checked');
                $('#getDependencydown').removeClass('checked');
                $("#muestratododown").show();
                $("#muestradatedown").hide();
            }
        }
        else  {
            $('#isDateDependencyTo').val("false");
            $("#getdatedown").addClass('checked');
            $('#getDependencydown').removeClass('checked');
            $("#muestratododown").show();
            $("#muestradatedown").hide();
        }
    } else if (optionSelected == "Multiple") {
        // Mostrar
        $("#div_x").show();
        $(".fromDateDependency").show();

        if (parseInt($('#ClauseId').val()) != 0) {
            if ($('#ExpirationFromDependencyId').val() != "" && $('#ExpirationFromDependencyId').val() != "0") {
                $('#isDateDependencyFrom').val("false");
                $("#getDependency").addClass('checked');
                $('#getdate').removeClass('checked');
                $("#muestratodo").show();
                $("#muestradate").hide();
            }
            else {
                $('#isDateDependencyFrom').val("true");
                $("#getdate").addClass('checked');
                $('#getDependency').removeClass('checked');
                $("#muestradate").show();
                $("#muestratodo").hide();
            }

            if ($('#ExpirationToDependencyId').val() != "" && $('#ExpirationToDependencyId').val() != "0") {
                $('#isDateDependencyTo').val("false");
                $("#getDependencydown").addClass('checked');
                $('#getdatedown').removeClass('checked');
                $("#muestradatedown").show();
                $("#muestratododown").hide();
            }
            else {
                $('#isDateDependencyTo').val("true");
                $("#getdatedown").addClass('checked');
                $('#getDependencydown').removeClass('checked');
                $("#muestratododown").show();
                $("#muestradatedown").hide();
            }
        }
        else
        {
            if ($('#ExpirationFromDependencyId').val() != "" && $('#ExpirationFromDependencyId').val() != "0") {
                $('#isDateDependencyFrom').val("false");
                $("#getDependency").addClass('checked');
                $('#getdate').removeClass('checked');
                $("#muestratodo").show();
                $("#muestradate").hide();
            }
            else {
                $('#isDateDependencyFrom').val("true");
                $("#getdate").addClass('checked');
                $('#getDependency').removeClass('checked');
                $("#muestradate").show();
                $("#muestratodo").hide();
                $("#muestradatedown").hide();
            }
        }
    }

    
}

function getRa(radio5) {

    var cssClass = "custom radio checked";

    $("#getdate").attr("class", cssClass);

    if ($("#getdate").attr("class") == cssClass) {
        $('#isDateDependencyFrom').val("true");
        $('#expirationFromDependencyId').val(null);
        $('#getDependency').removeClass('checked');
        $("#muestradate").show();
        $("#muestratodo").hide();
    }
}


function getRadioValue(radio) {

    var cssClass = "custom radio checked";

    $("#getDependency").attr("class", cssClass);

    if ($("#getDependency").attr("class") == cssClass) {
        $('#isDateDependencyFrom').val("false");
        $('#getdate').removeClass('checked');
        $("#muestratodo").show();
        $("#muestradate").hide();

        ValidateDependency($('#expirationFromDependencyId'), $('#fromDependencyDirection'), $('#monthsDirectionFrom'));
    }
}

function getRe(radiocl) {

    var cssClass = "custom radio checked";

    $("#getdatedown").attr("class", cssClass);

    if ($("#getdatedown").attr("class") == cssClass) {
        $('#isDateDependencyTo').val("true");
        $('#expirationToDependencyId').val(null);
        $('#getDependencydown').removeClass('checked');
        $("#muestratododown").show();
        $("#muestradatedown").hide();
    }
}

function getRadioValue1(radiokl) {

    var cssClass = "custom radio checked";

    $("#getDependencydown").attr("class", cssClass);

    if ($("#getDependencydown").attr("class") == cssClass) {
        $('#isDateDependencyTo').val("false");
        $('#getdatedown').removeClass('checked');
        $("#muestradatedown").show();
        $("#muestratododown").hide();

        ValidateDependency($('#expirationToDependencyId'), $('#toDependencyDirection'), $('#monthsDirectionTo'));
    }
}

function changeFromDirection(FromSelect) {
    var optionSelected = FromSelect.value;
    switch (optionSelected) {
        case 'Before':
            if ($('#monthsDirectionFrom').val() > 0) {
                $('#monthsDirectionFrom').val($('#monthsDirectionFrom').val() * -1)
            }
            break;
        case 'After':
            if ($('#monthsDirectionFrom').val() < 0) {
                $('#monthsDirectionFrom').val($('#monthsDirectionFrom').val() * -1);
            }
            break;
    }
}

function changeToDirection(ToSelect) {
    var optionSelected = ToSelect.value;
    switch (optionSelected) {
        case 'Before':
            if ($('#monthsDirectionTo').val() > 0) {
                $('#monthsDirectionTo').val($('#monthsDirectionTo').val() * -1);
            }
            break;
        case 'After':
            if ($('#monthsDirectionTo').val() < 0) {
                $('#monthsDirectionTo').val($('#monthsDirectionTo').val() * -1);
            }
            break;
    }
}

$(document).ready(function () {
    var test = $('#divCategory').data('route');
    $(".optionSelect").kendoDropDownList();
    $("#dropdownForCategory").kendoDropDownList({
        change: function (e) {
            var val = this.value();
            $("#dropdownForType").kendoDropDownList({
                dataTextField: "Text",
                dataValueField: "Value",
                dataSource: {
                    type: "jsonp",
                    transport: {
                        read: test + '?categoryId=' + val
                    }
                }
            });
            var dropdownlist = $("#dropdownForType").data("kendoDropDownList");
            dropdownlist.refresh();
        }
    });

	disabledElementsForCategoryOtherConditionsFirstDisbursement();	    

    function disabledElementsForCategoryOtherConditionsFirstDisbursement() {
        var datePicker = $('[aria-owns=datePicker10_dateview]');
        var selectedItem = $("#dropdownForCategory").data("kendoDropDownList").value();
        if (window['IsOA421Check'])
		{
			var normative = JSON.parse(IsOA421Check.toLowerCase());

			if (selectedItem === OtherP_421 && normative) {
				disabledFields();
			}
			else {
				activeFields();
			}
		}
    }

    function activeFields() {
        var instancesOfClause = $('[aria-owns=instancesOfClause_listbox] span.k-input');
        instancesOfClause.css({ 'color': '', 'background': '' });
        var dropdownlist = $("#instancesOfClause").data("kendoDropDownList");
        dropdownlist.enable(true);
        var  containerfromTo = $('.custom.operationData.fromTo.clDisburment');
        containerfromTo.css('pointer-events', '');

        $('.custom.operationData.fromTo.clDisburment span').css('color', '');

        containerfromTo.find('.custom.radio.checked').removeClass('disactived');

        $(".operationData.datePicker.fromTo").css('pointer-events', '');

        var datePicker = $('[aria-owns=datePicker10_dateview]');

        datePicker.val(null);

        datePicker.css('background', '');

        datePicker.css('font-weight', '');

        datePicker.css('font-size', '');

        datePicker.css('color', '');

        var isRadioBtnToDependencyChecked = $('#radioBtnToDependency').find('.custom.radio.checked').length;

        if (isRadioBtnToDependencyChecked) {
            $('#muestradatedown').show();
            $('#muestratododown').hide();
        }
        if ($('#labelForOtherDisbursement').is(":visible")) {
            $('#labelForOtherDisbursement').toggle();
        }
    }

    function disabledFields() {
        var dropdownlist = $("#instancesOfClause").data("kendoDropDownList");
        dropdownlist.enable(false);
        var instancesOfClause = $('[aria-owns=instancesOfClause_listbox] span.k-input');
        instancesOfClause.css({ 'color': 'rgba(51, 51, 51, 0.46)', 'background': '#EEE' });

        $('custom operationData fromTo').css('pointer-events', 'none');

        var containerfromTo = $('.custom.operationData.fromTo.clDisburment');
        containerfromTo.css('pointer-events', 'none');

        $('.custom.operationData.fromTo.clDisburment span').css('color', '#CCC');
        
        containerfromTo.find('.custom.radio.checked').addClass('disactived');

        $(".operationData.datePicker.fromTo").css('pointer-events', 'none');

        var dateAutomaticExpiration = $('#dateAutomaticExpiration').val();

        var datePicker = $('[aria-owns=datePicker10_dateview]');

        datePicker.val(dateAutomaticExpiration);

        datePicker.css('background', 'rgba(128, 128, 128, 0.19)');

        datePicker.css('font-weight', 'bold');

        datePicker.css('font-size', '1em');

        datePicker.css('color', 'rgba(51, 51, 51, 0.46)');

        var isRadioBtnToDependencyChecked = $('#radioBtnToDependency').find('.custom.radio.checked').length;

        if (isRadioBtnToDependencyChecked) {
            $('#muestradatedown').hide();
            $('#muestratododown').show();
        }

        if ($('#labelForOtherDisbursement').not(":visible")) {
            $('#labelForOtherDisbursement').toggle();
        }
    }

    $("#dropdownForType").kendoDropDownList({
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: {
            type: "jsonp",
            transport: {
                read: test + '?categoryId=' + $("#dropdownForCategory").data("kendoDropDownList")._initial
            }
        }
    });

    var dropdownlist = $("#dropdownForType").data("kendoDropDownList");
    dropdownlist.refresh();

    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });
    
    $('#datePicker09').on("change", function() {
        setMinDatePickerValue();
    });
    
    
    $('#datePicker10').on("change", function() {
        setMinDatePickerValue();
    });
    
    $("input[type='checkbox']:checked").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });
    $("input[type='checkbox']:not(:checked)").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });
    $("input[type='checkbox'],input[type='radio']").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });

    getvalue($('#instancesOfClause'));

    $('#RelationsWithContracts1').change(function () {
        $('#clauseRelationWithContracts').val($('#RelationsWithContracts1').val());
    });

    

    $('.lnkModal').click(function () {

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="k-window-titlebar k-header lineOneClauseTemplate">&nbsp;' +
                    '<span class="k-window-title lineTwoClauseTemplate" id="window1_wnd_title">Clause template</span>' +
                    '<div class="k-window-actions">' +
                        '<a class="k-window-action k-link" href="#" role="button">' +
                            '<span class="k-icon k-i-close" role="presentation">Close</span>' +
                        '</a>' +
                    '</div>' +
                '</div>' +
                '<div class="window clauseAddTemplate windowOpen lineThreeClauseTemplate" id="window1" role="dialog" aria-labelledby="window1_wnd_title"></div>');

        var dialog = $(".lineThreeClauseTemplate").kendoWindow({
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
                $(".k-window-titlebar").remove();
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

    $('.edit').click(function () {
        window.location.href = $(this).data("route");
        return false;
    });


    function preventDefault(e) {
        e = e || window.event;
        if (e.preventDefault)
            e.preventDefault();
        e.returnValue = false;
    }

    function keydown(e) {
        for (var i = keys.length; i--;) {
            if (e.keyCode === keys[i]) {
                preventDefault(e);
                return;
            }
        }
    }

    function wheel(e) {
        preventDefault(e);
    }

    function disable_scroll() {
        if (window.addEventListener) {
            window.addEventListener('DOMMouseScroll', wheel, false);
        }
        $(".k-window").hover(
            function () {
                document.onmousewheel = document.onmousedown = document.onkeydown = null;
            },
            function () {
                document.onmousewheel = document.onmousedown = wheel;
                document.onkeydown = keydown;
            }
        );
    }

    function enable_scroll() {
        if (window.removeEventListener) {
            window.removeEventListener('DOMMouseScroll', wheel, false);
        }
        document.onmousewheel = document.onmousedown = document.onkeydown = null;
    }

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $("#target").validate();

    jQuery.validator.addMethod("datepicker", function (value, element) {

        if (($("#radioBtnToDate span").attr('class') == "custom radio checked")) {
            if ($(element).attr('id') == "datePicker10") {
                var Resultado = ValidarFechaLeggal(value, element);

                if (Resultado == false) {
                    $(element).attr('data-val-required', "Expiration date of the clauses should be after legal effectiveness date");
                    AsignarClaseDatePicker()
                    return false;
                }
                else if (Resultado == true) {
                    $(element).attr('data-val-required', "Please, complete the mandatory fields");
                    AsignarClaseDatePicker()
                    return false
                }
            }
        }

        if (($("#radioBtnFromDate span").attr('class') == "custom radio checked")) {
            if ($(element).attr('id') == "datePicker09") {
                var Resultado = ValidarFechaLeggal(value, element);

                if (Resultado == false) {
                    $(element).attr('data-val-required', "Expiration date of the clauses should be after legal effectiveness date");
                    AsignarClaseDatePicker()
                    return false;
                }
                else if (Resultado == true) {
                    $(element).attr('data-val-required', "Please, complete the mandatory fields");
                    AsignarClaseDatePicker()
                    return false
                }
            }
        }

        AsignarClaseDatePicker()
        return true;
    });

    jQuery.validator.addMethod("input", function (value, element) {
        if ($('#instancesOfClause').val() == "Single") {
            if (($("#radioBtnToDependency span").attr('class') == "custom radio checked")) {
                if ($(element).attr("id") == "monthsDirectionTo") {
                    var ValueMonths = parseInt(value);

                    if (ValueMonths.toString() == "NaN") {
                        $(element).attr('data-val-required', "Please, fill in the required fields");
                        AsignarClaseDatePicker()
                        return false;
                    }
                    else {
                        if ((value % 1) != 0) {
                            $(element).attr('data-val-required', "Please enter only whole numbers");
                            AsignarClaseDatePicker()
                            return false;
                        }
                    }
                }
            }
        }
        else {
            if ($(element).attr("id") == "eachMonths") {
                var ValueMonths = parseInt(value);

                if (ValueMonths.toString() == "NaN") {
                    $(element).attr('data-val-required', "Please, fill in the required fields");
                    AsignarClaseDatePicker()
                    return false;
                }
                else {
                    if ((value % 1) != 0) {
                        $(element).attr('data-val-required', "Please enter only whole numbers");
                        AsignarClaseDatePicker()
                        return false;
                    }
                }
            }

            if (($("#radioBtnToDependency span").attr('class') == "custom radio checked")) {
                if ($(element).attr("id") == "monthsDirectionTo") {
                    var ValueMonths = parseInt(value);

                    if (ValueMonths.toString() == "NaN") {
                        $(element).attr('data-val-required', "Please, fill in the required fields");
                        AsignarClaseDatePicker()
                        return false;
                    }
                    else {
                        if ((value % 1) != 0) {
                            $(element).attr('data-val-required', "Please enter only whole numbers");
                            AsignarClaseDatePicker()
                            return false;
                        }
                    }
                }
            }

            if (($("#radioBtnFromDependency span").attr('class') == "custom radio checked")) {
                if ($(element).attr("id") == "monthsDirectionFrom") {
                    var ValueMonths = parseInt(value);

                    if (ValueMonths.toString() == "NaN") {
                        $(element).attr('data-val-required', "Please, fill in the required fields");
                        AsignarClaseDatePicker()
                        return false;
                    }
                    else {
                        if ((value % 1) != 0) {
                            $(element).attr('data-val-required', "Please enter only whole numbers");
                            AsignarClaseDatePicker()
                            return false;
                        }
                    }
                }
            }
        }

        AsignarClaseDatePicker()
        return true;
    });

    var functionCancel = function (event) {
        event.preventDefault();
        var modal = $(".dinamicModal").data("kendoWindow");
        modal.close();
        $(".ui-widget-overlay ui-front").remove();
    };

    $('.save').on('click', function (event) {

        var value = $('#Trench').val();
        if (isPBPOrPBLAndOA420Operation && !hasTemplate && cmdNoApplyTrenchClauseId != value) {
            
            event.preventDefault();
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"></div>');
            $("#AdvTemplate").appendTo(".dinamicModal").removeClass("hide");
            var title = $("#AdvTemplate").data("title");
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
                    $("#ConfirmAdvTemplate").click(functionCancel);
                },
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $("#AdvTemplate").appendTo("#ui_sp_001").addClass("hide");
                    $("body").find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
            modal.center();
            modal.open();

        } else {

            event.preventDefault();

            //Limpieza de informacion cuando se selecciona el tipo de instacia de la clausula
            if ($('#instancesOfClause').val() == "Single") {

                //limpieza de informacion dependiendo de si se manejan fechas o dependencias en To
                if ($("#radioBtnToDate span").attr('class') == "custom radio checked") {
                    //Asignacion de valores nulos cuando no es una dependencia
                    $("#expirationToDependencyId").val(null);
                    $("#monthsDirectionTo").val(null);
                    $("#toDependencyDirection").val(null);

                    $("#monthsDirectionTo").attr('data-val', 'false')
                    $("#expirationToDependencyId").attr('data-val', 'false')
                    $("#datePicker10").attr('data-val', 'true')
                }
                else if ($("#radioBtnToDependency span").attr('class') == "custom radio checked") {
                    //Asigancion de valores nulos cuando no es una fecha
                    $("#datePicker10").val(null);

                    $("#monthsDirectionTo").attr('data-val', 'true')
                    $("#expirationToDependencyId").attr('data-val', 'false')
                    $("#datePicker10").attr('data-val', 'false')
                }

                $("#fromDependencyDirection").val(null);

                //Asignacion de valores nulos cuando no se necesita un dato para from (clausula con instacia en single)
                $("#expirationFromDependencyId").val(null);
                $("#monthsDirectionFrom").val(null);
                $("#datePicker09").val(null);

                $("#expirationFromDependencyId").attr('data-val', 'false');
                $("#monthsDirectionFrom").attr('data-val', 'false');
                $("#datePicker09").attr('data-val', 'false');

                $("#eachMonths").val(null);
                $("#eachMonths").attr('data-val', 'false');
            }
            else {
                //limpieza de informacion dependiendo de si se manejan fechas o dependencias en From
                if ($("#radioBtnFromDate span").attr('class') == "custom radio checked") {
                    //Asignacion de valores nulos cuando no es una dependencia
                    $("#expirationFromDependencyId").val(null);
                    $("#monthsDirectionFrom").val(null);
                    $("#fromDependencyDirection").val(null);

                    $("#expirationFromDependencyId").attr('data-val', 'false');
                    $("#monthsDirectionFrom").attr('data-val', 'false');
                    $("#datePicker09").attr('data-val', 'true')
                }
                else if ($("#radioBtnFromDependency span").attr('class') == "custom radio checked") {
                    //Asigancion de valores nulos cuando no es una fecha
                    $("#datePicker09").val(null);

                    $("#expirationFromDependencyId").attr('data-val', 'false');
                    $("#monthsDirectionFrom").attr('data-val', 'true')
                    $("#datePicker09").attr('data-val', 'false')
                }

                //limpieza de informacion dependiendo de si se manejan fechas o dependencias en To
                if ($("#radioBtnToDate span").attr('class') == "custom radio checked") {
                    //Asignacion de valores nulos cuando no es una dependencia
                    $("#expirationToDependencyId").val(null);
                    $("#monthsDirectionTo").val(null);
                    $("#toDependencyDirection").val(null);

                    $("#expirationToDependencyId").attr('data-val', 'false')
                    $("#monthsDirectionTo").attr('data-val', 'false')
                    $("#datePicker10").attr('data-val', 'true')
                }
                else if ($("#radioBtnToDependency span").attr('class') == "custom radio checked") {
                    //Asigancion de valores nulos cuando no es una fecha
                    $("#datePicker10").val(null);

                    $("#expirationToDependencyId").attr('data-val', 'false')
                    $("#monthsDirectionTo").attr('data-val', 'true')
                    $("#datePicker10").attr('data-val', 'false')
                }

                $("#eachMonths").attr('data-val', 'true');
            }

            var form = $("#target").removeData("validator").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);

            AsignarClaseDatePicker();
            $("#target").submit();

            //Asignacion de clase a los datepicker cuando se validan
            AsignarClaseDatePicker();
        }
    });

    $("#datePicker09").on("focusout", function () {
        value = $('#datePicker09').val();
        ValidarFecha(value, $("#datePicker09"));
    });

    $("#datePicker10").on("focusout", function () {
        value = $('#datePicker10').val();
        ValidarFecha(value, $("#datePicker10"));
    });

    $("#monthsDirectionTo").on("focusout", function () {
        ValidationMonths($('#monthsDirectionTo'), $('#toDependencyDirection'));
    });

    $("#monthsDirectionFrom").on("focusout", function () {
        ValidationMonths($('#monthsDirectionFrom'), $('#fromDependencyDirection'));
    });

    $("#expirationToDependencyId").on("focusout", function () {
        ValidateDependency($('#expirationToDependencyId'), $('#toDependencyDirection'), $('#monthsDirectionTo'));
    });

    $("#expirationFromDependencyId").on("focusout", function () {
        ValidateDependency($('#expirationFromDependencyId'), $('#fromDependencyDirection'), $('#monthsDirectionFrom'));
    });

    $("#toDependencyDirection").on("focusout", function () {
        ValidateMonthWithDirection($('#monthsDirectionTo'), $('#toDependencyDirection'));
    });

    $("#fromDependencyDirection").on("focusout", function () {
        ValidateMonthWithDirection($('#monthsDirectionFrom'), $('#fromDependencyDirection'));
    });

    $('.k-dropdown-wrap.k-state-default').addClass('btn');

    if ($('#IncludeFinancialStatements').val() === 'True') {
        var dropdownForCategory = $("#dropdownForCategory").data("kendoDropDownList");
        var dropdownForType = $("#dropdownForType").data("kendoDropDownList");

        dropdownForCategory.readonly();
        dropdownForType.readonly();
    }

});

function setMinDatePickerValue(){
    var datePickerFromDate = $('#datePicker09');
    var datePickerToDate = $('#datePicker10');
    var datePickerTo = datePickerToDate.data("kendoDatePicker");
    var tipedFromDateValue = datePickerFromDate.val();
    var tipedToDateValue = datePickerToDate.val();

    if (tipedFromDateValue){
        startDate = new Date(tipedFromDateValue);
        endDate = dateParse(tipedToDateValue);
        startDate.setDate(startDate.getDate());
        datePickerTo.min(startDate);
        if (isNaN(endDate) || endDate.valueOf() < startDate.valueOf()){
            datePickerTo.value(startDate);
        } 
    }
}

function dateParse(dateString) {
    
    var dateStringNumbers = dateString.replace(/[^\/0-9]+/g, '');
    var dateMatch = dateStringNumbers.match(/(\d+)\/(\d+)\/(\d+)/);
    var date;
    
    if(dateMatch) {
            
            date = new Date();
            var day = parseInt(dateMatch[1]);
            var month = parseInt(dateMatch[2]);
            var year = parseInt(dateMatch[3]);
            
            if(month > 12) {
                day = parseInt(dateMatch[2]);
                month = parseInt(dateMatch[1]);
            }
            
            if(year < 50) {
                year+=2000;
            } else { 
                if (year < 100){
                    year+=1900;
                }
            }
            
            date.setMonth(month - 1);
            date.setDate(day);
            date.setYear(year);
    } else {
        date = new Date(dateString);
    }
    
    return date;
}

function AsignarClaseDatePicker() {
    $("span .datepicker").css('display', 'block');
    $("span .datepicker").attr('class', 'k-widget k-datepicker k-header datepicker');
}

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



//Validacion Fecha mayor a leggal
function ValidarFechaLeggal(value, element) {
    var date = moment(value, "DD MMM YYYY");
    var datef = moment(value).format('YYYY[/]MM[/]DD');
    var legal = moment(new Date($('#legalEffectivenessDate').val())).format('YYYY[/]MM[/]DD');

    if (!date.isValid())
    {
        return true;
    }
    else if (moment(datef).isBefore(legal))
    {
        return false;
    }
    return;
}

//Validacion fecha To mayor a From
function ValidarFechasToFrom(valueFrom, elementFrom, valueTo, elementTo) {

    var FechaValidaFrom = ValidarFechaMayor(valueFrom, elementFrom);
    var FechaValidaTo = ValidarFechaMayor(valueS, elementS);

    if (true) {

    }
    elementId = $(element).attr("id");

}

//Validacion Fecha mayor a leggal
function ValidarFechaMayor(value, element) {
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
                return false;
            }
            else {
                dia = fechaCompleta[1];
                mes = fechaCompleta[3];
                anio = fechaCompleta[5];
            }
        }
        else {
            return false;
        }
    }
    else {
        dia = fechaCompleta[1];
        mes = fechaCompleta[3];
        anio = fechaCompleta[5];
    }

    //El valor del día debe estar comprendido entre 1 y 31.
    if (dia < 1 || dia > 31) {
        return false;
    }

    //El valor del mes debe estar comprendido entre 1 y 12.
    if (mes < 1 || mes > 12) {
        return false;
    }

    //El mes debe tener 31 días
    if ((mes == 4 || mes == 6 || mes == 9 || mes == 11) && dia == 31) {
        return false;
    }

    //El mes debe tener 28 dias
    if (mes == 2) {
        // bisiesto
        var bisiesto = (anio % 4 == 0 && (anio % 100 != 0 || anio % 400 == 0));
        if (dia > 29 || (dia == 29 && !bisiesto)) {
            return false;
        }
    }

    return true;
}

//Validacion de Meses: si el valor=null mes=0, dependiendo de la direccion varia  el valor: after:valor positivo before:valor negativo
function ValidationMonths(element, Direction) {
    var ElementId = $(element).attr("id");
    var Value = $(element).val();

    var ValueDirection = $("#" + Direction.attr("id") + " :selected").val();

    if (ElementId == "monthsDirectionTo" || ElementId == "monthsDirectionFrom") {
        if (Value == "" || Value == null) {
            $(element).val(0);
        }
        else if (Value < 0) {
            if (ValueDirection == "After") {
                Value = $(element).val() * (-1);
                $(element).val(Value);
            }
            else if (ValueDirection == "Before") {
                Value = $(element).val();
                $(element).val(Value);
            }
        }
        else if (Value > 0) {
            if (ValueDirection == "After") {
                Value = $(element).val();
                $(element).val(Value);
            }
            else if (ValueDirection == "Before") {
                Value = $(element).val() * (-1);
                $(element).val(Value);
            }
        }
    }
}

//validacion dependencia: si dependencia es Leggal entonces direccion:after y valor Months:positivo, si es current entonces direccion: after,before y valor Months varias
//dependiendo de la direccion
function ValidateDependency(element, Direction, Months) {

    //Dependency: LEGALEFF or LASTCD
    var ExpFromDep = $("#" + element.attr("id"));
    var ExpFromDepSelected = $("#" + element.attr("id") + " :selected");
    var ExpFromDepSelectedId = $("#" + element.attr("id") + " :selected").val();
    var ListExpFromDep = $("#dateForDependencyListforValidation").val();
    var ListAfterorBefore = $("#dependencyDirectionListValidation").val();

    //Direction: AFTER or BEFORE
    var AfterorBefore = $('#' + Direction.attr("id") + ' :selected').val();
    var optionBeforeText = $("#BeforeDirection").val();
    var optionAfterText = $("#AfterDirection").val();

    $.each($.parseJSON(ListExpFromDep), function (index, ExpFromDependency) {
        if (ExpFromDependency.Value == ExpFromDepSelectedId && ExpFromDependency.Text == "LEGALEFF") {

            var BeforeExist = $("#" + Direction.attr("id") + " option[value='Before']").val();

            if (BeforeExist != null) {

                $("#" + Direction.attr("id") + "option[value='Before']").remove();
                var data = [{ text: optionAfterText, value: optionAfterText, selected: true }];

                $("#" + Direction.attr("id")).kendoDropDownList({
                    dataTextField: "text",
                    dataValueField: "value",
                    dataSource: data,
                    selected: "selected"
                });

                var dropdownlist = $("#" + Direction.attr("id")).data("kendoDropDownList");
                dropdownlist.refresh();
            }

            ValidateMonthWithDirection(Months, Direction);
        }
        else if (ExpFromDependency.Value == ExpFromDepSelectedId && ExpFromDependency.Text == "LASTCD") {
            var BeforeExist = $("#" + Direction.attr("id") + " option[value='Before']").val();

            if (BeforeExist == null) {

                var optionBefore = "'<option value=" + optionBeforeText + ">" + optionBeforeText + "</option>'";
                $("#" + Direction.attr("id")).append(optionBefore);

                var data = [{ text: optionAfterText, value: optionAfterText, selected: true }, { text: optionBeforeText, value: optionBeforeText, selected: false }];

                $("#" + Direction.attr("id")).kendoDropDownList({
                    dataTextField: "text",
                    dataValueField: "value",
                    dataSource: data,
                    selected: "selected"
                });

                var dropdownlist = $("#" + Direction.attr("id")).data("kendoDropDownList");
                dropdownlist.refresh();
            }

            ValidateMonthWithDirection(Months, Direction);
        }
    });
}

//Validacion valor Months segun direccion: si direccion:Before valor Months:Negativo, si direccion:After Valor Months:Positivo
function ValidateMonthWithDirection(element, Direction) {
    var ValueDirection = $("#" + Direction.attr("id") + " :selected").val();
    var ValueElement = $(element).val();

    switch (ValueDirection) {
        case 'Before':
            if (ValueElement > 0) {
                ValueElement = ValueElement * (-1);
                $(element).val(ValueElement);
            }
            break;
        case 'After':
            if (ValueElement < 0) {
                ValueElement = ValueElement * (-1);
                $(element).val(ValueElement);
            }
            break;
    }
}

//Sumar o restar Meses
function addTimeToDate(ValorAdicional, dia, mes, anio) {
    if (ValorAdicional < 0) {
        while (ValorAdicional < -11) {
            ValorAdicional = ValorAdicional + 12;
            anio = anio - 1;
        }

        if (ValorAdicional < 0) {
            mes = mes + ValorAdicional;

            if (mes < 1) {
                anio = anio - 1;
                mes = 12 - mes;
            }
        }
    }
    else if (ValorAdicional >= 0) {
        while (ValorAdicional > 11) {
            ValorAdicional = ValorAdicional - 12;
            anio = anio + 1;
        }

        if (ValorAdicional > 0) {
            mes = mes + ValorAdicional;

            if (mes > 11) {
                anio = anio + 1;
                mes = mes - 12;
            }
        }
    }
    var Fecha = new Date(mes.toString() + "/" + dia.toString() + "/" + anio.toString());
    return Fecha;
}
