$(document).ready(function () {


    $('#clearBtn').click(function () {
        var dropdownlist = $("#operationNumberFilterRisks").data("kendoDropDownList");
        dropdownlist.select(0);
        dropdownlist = $("#typeFilterRisks").data("kendoDropDownList");
        dropdownlist.select(0);
        dropdownlist = $("#riskLevelFilterRisks").data("kendoDropDownList");
        dropdownlist.select(0);
        dropdownlist = $("#targetAudienceFilterRisks").data("kendoDropDownList");
        dropdownlist.select(0);
        dropdownlist = $("#statusFilterRisks").data("kendoDropDownList");
        dropdownlist.select(0);
        $("#DescriptionFilter").val(null);

        })
        $("#datePicker5").val(null);
        $("#datePicker6").val(null);
    });

   
