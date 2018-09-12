
$(document).ready(function () {

    $("#lnkCloseVerifyContent").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
    });

    var grid = $(".gridVerifyContent").kendoGrid({
        dataSource: {
            pageSize: 10
        },
        scrollable: false,
        sortable: true,
        selectable: false,
        filterable: false,
        pageable: true,
        resizable: true,
    }).data("kendoGrid");

});



//#region Global Functions

/// <summary>Description of A function</summary>
/// <param name="intParemeter" type="int">Is an integer parameter value</param>
/// <returns type="int" />
function functionA(intParemeter) {
    // TO DO
}

/// <summary>Description of B function</summary>
/// <param name="intParemeter" type="int">Is an integer parameter value</param>
/// <returns type="int" />
function functionB() {
    // TO DO
}

/// <summary>Description of C function</summary>
/// <param name="intParemeter" type="int">Is an integer parameter value</param>
/// <returns type="int" />
function functionC() {
    // TO DO
}

//#endregion
