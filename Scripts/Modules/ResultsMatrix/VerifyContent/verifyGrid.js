
$(document).ready(function () {

    $("#lnkCloseVerifyContent").click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
    });

    /**
    * Jira ID: CON-1060
    * Fix Description: Add Pagination and behavior of kendo grid 
    * Attended by: Jhon Astaiza 
    * Date: 11/10/2014
    * Begin Fix
    **/
    var grid = $("#verify-results-grid").kendoGrid({
        dataSource: {
            schema: {
                model: {
                    fields: {
                        Description: { type: "string" },
                        Compliance: { type: "string" }
                    }
                }
            },
            pageSize: 10
        },
        scrollable: false,
        sortable: false,
        selectable: true,
        filterable: false,
        pageable: true,
        info: false,
        previousNext: false,
        resizable: true,
        messages: {
            display: "",
            first: "",
            previous: "",
            next: "",
            last: "",
            refresh: ""
        },
        columns: [
            {
                field: "Description",
                title: 'Description'
            },
            {
                field: "Compliance",
                title: 'Compliance'
            }
        ]
    }).data("kendoGrid");
    /**
    * End Fix
    **/
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
