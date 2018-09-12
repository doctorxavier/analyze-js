function validatioUnits() {
    var d = new Date();
    var year = d.getFullYear();
    var inputTextYear = $('#OutputYearPlanId option:selected').text();
    var inputYear = parseInt(inputTextYear);
    if (year > inputYear)
    {
        if ($("#DeliveryStatusId option:contains('In progress')").length > 0) {
            optionInProgress = $("#DeliveryStatusId option:contains('In progress')").remove();
        }
        if ($("#DeliveryStatusId option:contains('Planned')").length > 0) {
            optionPlanned = $("#DeliveryStatusId option:contains('Planned')").remove();
        }
        if (!$("#DeliveryStatusId option:contains('Delivered')").length > 0) {
            $('#DeliveryStatusId').append(optionDelivered);
        }
        $('#DeliveryStatusId').kendoDropDownList();
        var txtActualValue = $('#actualValue').text();
        if (txtActualValue != undefined && txtActualValue != null && txtActualValue != '' && txtActualValue != '-')
        {
            var txtActualMapped = $('#actualMapped').text();
            if ((parseInt(txtActualValue) == parseInt(txtActualMapped))
                || (parseInt(txtActualValue) == 0)
                || (parseInt(txtActualValue) < parseInt(txtActualMapped)))
            {  
                $("#OutputUnits").rules("remove", "max");
                $("#OutputUnits").prop('disabled', true);
                $("#OutputUnits").removeClass("invalid");
                $("#OutputUnits").rules("add", {
                    required: false
                });
            }
            else
            {
                var maximo = parseInt(txtActualValue) - parseInt(txtActualMapped);
                $("#OutputUnits").rules("remove", "max");
                $("#OutputUnits").removeClass("invalid");
                $("#OutputUnits").rules("add", {
                    max: maximo,
                    required: true
                });
            }

            if ((txtActualMapped != undefined
                || txtActualMapped != null
                || txtActualMapped != '')
                || txtActualMapped != '-')
            {                
                if (parseInt(txtActualValue) < parseInt(txtActualMapped)) {
                    $('#actualMapped').css({ "font-weight": "bold", "color": "#FF0000" });
                }
            }

        }
        else
        {
            $("#OutputUnits").rules("remove", "max");
            $("#OutputUnits").prop('disabled', true);
            $("#OutputUnits").removeClass("invalid");
            $("#OutputUnits").rules("add", {
                required: false
            });
        }

    }
    else
    {
        if (year < inputYear)
        {
            if ($("#DeliveryStatusId option:contains('Delivered')").length > 0) {
                optionDelivered = $("#DeliveryStatusId option:contains('Delivered')").remove();
            }
            if (!$("#DeliveryStatusId option:contains('In progress')").length > 0) {
                $('#DeliveryStatusId').append(optionInProgress);
            }
            if (!$("#DeliveryStatusId option:contains('Planned')").length > 0) {
                $('#DeliveryStatusId').append(optionPlanned);
            }

        }
        else
        {
            if (!$("#DeliveryStatusId option:contains('In progress')").length > 0) {
                $('#DeliveryStatusId').prepend(optionInProgress);
            }
            if (!$("#DeliveryStatusId option:contains('Planned')").length > 0) {
                $('#DeliveryStatusId').prepend(optionPlanned);
            }
            if (!$("#DeliveryStatusId option:contains('Delivered')").length > 0) {
                $('#DeliveryStatusId').append(optionDelivered);
            }
            $("#DeliveryStatusId option:contains('In progress')").prop('selected', 'true');

        }
        
        if (inputYear >= year )
        {
            var actualPlanned = parseInt($('#planned').text());
            var txtActualValue = $('#actualValue').text();
            var actualValue = parseInt(txtActualValue);
            var txtActualMapped = $('#actualMapped').text();
            var actualMapped = parseInt(txtActualMapped);
            
            if (txtActualValue == undefined
                || txtActualValue == null
                || txtActualValue == '-'
                || txtActualValue == '')
            {
                actualValue = 0;
            }
            if (txtActualMapped == undefined
                || txtActualMapped == null
                || txtActualMapped == '-'
                || txtActualMapped == '')
            {
                actualMapped = 0;
            }
            
            var biggerOfBoth = Math.max(actualPlanned, actualValue);
            if (biggerOfBoth > actualMapped) {
                 $('#actualMapped').css({ "font-weight": "bold", "color": "#FF0000" });
            }
            
        }
                
        $('#DeliveryStatusId').kendoDropDownList();
        var txtActualPlanned1 = $('#planned').text();
        var txtActualValue1 = $('#actualValue').text();

        var txtActualPlanned = parseInt(txtActualPlanned1);
        var txtActualValue = parseInt(txtActualValue1);

        if (!isNaN(txtActualPlanned)) {
            if (isNaN(txtActualValue)) {
                var intActualPlanned = parseInt(txtActualPlanned);
                var maxValue = intActualPlanned;
            } else {
                var intActualPlanned = parseInt(txtActualPlanned);
                var intActualValue = parseInt(txtActualValue);
                var maxValue = Math.max(intActualPlanned, intActualValue);
            }
            var txtActualMapped = $('#actualMapped').text();
            if ((maxValue == parseInt(txtActualMapped)) || (maxValue == 0) ||
                (maxValue < parseInt(txtActualMapped))) {
                $("#OutputUnits").rules("remove", "max");
                $("#OutputUnits").prop('disabled', true);
                $("#OutputUnits").removeClass("invalid");
                $("#OutputUnits").rules("add", {
                    required: false
                });
            }
            else {
                var maximo = maxValue - parseInt(txtActualMapped);
                $("#OutputUnits").rules("remove", "max");
                $("#OutputUnits").removeClass("invalid");
                $("#OutputUnits").rules("add", {
                    max: maximo,
                    required: true
                });
            }
        }
        else {
            if (!isNaN(txtActualValue)) {
                var intActualValue = parseInt(txtActualValue);
                var maxValue = intActualValue;
                var txtActualMapped = $('#actualMapped').text();
                if ((maxValue == parseInt(txtActualMapped)) || (maxValue == 0) ||
                    (maxValue < parseInt(txtActualMapped))) {
                    $("#OutputUnits").rules("remove", "max");
                    $("#OutputUnits").prop('disabled', true);
                    $("#OutputUnits").removeClass("invalid");
                    $("#OutputUnits").rules("add", {
                        required: false
                    });
                }
                else {
                    var maximo = maxValue - parseInt(txtActualMapped);
                    $("#OutputUnits").rules("remove", "max");
                    $("#OutputUnits").removeClass("invalid");
                    $("#OutputUnits").rules("add", {
                        max: maximo,
                        required: true
                    });
                }
            }
            else {
                $("#OutputUnits").rules("remove", "max");
                $("#OutputUnits").prop('disabled', true);
                $("#OutputUnits").removeClass("invalid");
                $("#OutputUnits").rules("add", {
                    required: false
                });

            }
        }

    }

}