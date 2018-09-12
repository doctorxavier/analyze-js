$(document).ready(function () {
    // Update css class radio button
    redefineClassRadioButton($("#hdndisplayAsGroup").val());

    // Get radiobutton option
    $("input[name = 'rdbtn_display']").click(function () {

        // Set displayAsGroup option (true or false)
        var displayAsGroup = ($(this).val() == 0) ? false : true;
        
        url = idbg.getPath("OperationProfile/FinancingData/Index?OperationNumber=" + $("#hdnOperationNumber").val() + "&displayAsGroup=" + displayAsGroup);
        // Redirect to Financing Data
        $(location).attr("href",url);
    });

});

// Change style to selected radio buttton
function redefineClassRadioButton(displayAsGroup)
{
    if (displayAsGroup == "true") {
        $("#lbl_group").addClass("ui-state-active");
        $("#lbl_selected").removeClass("ui-state-active");
    } else {
        $("#lbl_group").removeClass("ui-state-active");
        $("#lbl_selected").addClass("ui-state-active");
    }
}