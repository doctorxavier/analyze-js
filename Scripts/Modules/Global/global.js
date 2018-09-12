var global = {}
global.default_kendo_options = {
    pageable: {
        pageSize: 10,
        info: false,
        previousNext: false,
        messages: {
            display: "",
            first: "",
            previous: "",
            next: "",
            last: "",
            refresh: ""
        }
    },
    dataSource: {
        transport: {
            read: {
                dataType: "json",
                type: "POST",
            }
        },
        serverSorting: true,
        serverPaging: true,
    },
}
global.grid = function (selector, data_url, options) {
    var defaults = global.default_kendo_options;
    defaults.dataSource.transport.url = data_url;
    selector = jQuery(selector);
    options = jQuery.extend({}, defaults, options);
    selector.kendoGrid(options);
    return
}
var validateclear = 0;
var bound = false;
function make_filter(gridName) {

    var validator = {
        OperationLoans: false,
        type: false,
        date: false,
    }

    $filter = new Array();
    $filter2 = new Array();

    if ($("#filterValue").val().length > 0) {
        if ($("#filterValue").val().trim() != "") {
            $filter.push({ field: "OperationName", operator: "contains", value: $("#filterValue").val() });
            $filter.push({ field: "OperationNumber", operator: "contains", value: $("#filterValue").val() });
            $filter.push({ field: "Loans", operator: "contains", value: $("#filterValue").val() });
            validator.OperationLoans = true;
        }
    }

    var grid = $(gridName).data("kendoGrid");

    if (($("#filterValueType").length > 0) || ($("#filterValueCreated").length > 0)) {
        if (($("#filterValueType").val().trim() != "") && ($("#filterValueType").val().trim() != "null")) {
            $filter2.push({ field: "WorkflowNameEn", operator: "contains", value: $("#filterValueType").val() });
            validator.type = true;
        }
        if ($("#filterValueCreated").val() != "") {
            $filter2.push({ field: "StartDate", operator: "eq", value: kendo.toString(new Date($("#filterValueCreated").val()), "d")});
            validator.date = true;
        }

        if (initialfilter) {
            $filter = new Array();
            grid.dataSource.filter({ logic: "or", filters: $filter });
        } else {
            if ((validator.date == true) || (validator.type == true)) {
                grid.dataSource.filter({ logic: "and", filters: $filter2 });
            } else {
                grid.dataSource.filter({ logic: "or", filters: $filter });
            }
        }
        validateclear = 0;
    } else {
        grid.dataSource.filter({ logic: "or", filters: $filter });
    }

    var allnot = $("#main-notification-grid tbody tr").length /2;
    var newnot = 0;
    var readnot = 0;

    $("#main-notification-grid tbody tr").each(function (index, element)
    {
        if ($(element).attr("role") != null)
        {
            if ($(element).find("label").hasClass("checked")) {
                readnot = readnot + 1;
            }
            else {
                newnot = newnot + 1;
            }
        }
    });

    $("#CounterNew").text(newnot);
    $("#CounterRead").text(readnot);
    $("#CounterAll").text(newnot);
    //Decomentariar lina siguiente y borrar anterior CON-304
    //$("#CounterAll").text(allnot);


}

function updateTotalOperations(evt) {
    if (!bound) {
        var grid = $("#main-operations-grid").data("kendoGrid");
        $("#notificationQty").html(grid.dataSource.total());
        bound = true;
    }
}

//total displayed for My Operations Grid
function totalDisplayed() {
    var grid = $("#main-operations-grid").data("kendoGrid");
    grid.dataSource.pageSize(parseInt(this.value()));
}

function changeNotificationsDisplay() {
    var grid = $("#main-notification-grid").data("kendoGrid");
    grid.dataSource.pageSize(parseInt(this.value()));
}

function changeTasksDisplay() {
    var grid = $("#main-task-grid").data("kendoGrid");
    grid.dataSource.pageSize(parseInt(this.value()));
}

function changeWorkflowsDisplay() {
    var grid = $("#main-workflows-grid").data("kendoGrid");
    grid.dataSource.pageSize(parseInt(this.value()));
}

function showMyOperationsFilter(gridName)
{
    $("#btnShowFilters").click(function () {
        if ($("#divFilters").css('display') === 'none')
            $("#divFilters").css('display', 'block');
        else {
            $("#divFilters").css('display', 'none');
            $("#filterValue").val("");
            make_filter(gridName);
        }
    });
}

var boundTotalNotifications = false;
function updateTotalNotifications(evt) {
    if (!boundTotalNotifications) {
        var grid = $("#main-notification-grid").data("kendoGrid");
        //$("#notificationQty").html(grid.dataSource.total());


        $("#notificationQty").html(constall);


        

        boundTotalNotifications = true;
    }
}


var boundTotalWorkflows = false;
function updateTotalWorkflows(evt) {
    if (!boundTotalNotifications) {
        var grid = $("#main-workflows-grid").data("kendoGrid");
        $("#notificationQty").html(grid.dataSource.total());
        boundTotalNotifications = true;
    }
}


var boundTotalTasks = false;
function updateTotalTasks(evt) {
    if (!boundTotalTasks) {
        var grid = $("#main-task-grid").data("kendoGrid");
        $("#notificationQty").html(grid.dataSource.total());
        boundTotalTasks = true;
    }
    hideLoader();
    initReziseCloud();
}


function PriorityDropDown(container, options) {
    var data = [{ text: idbg.getPath("/Images/Icons/row_green_down.png"), value: "0" },
                { text: idbg.getPath("/Images/Icons/priority-med.png"), value: "1" },
                { text: idbg.getPath("/Images/Icons/row_red_down.png"), value: "2" }];

    $('<input id="Priority"  data-value-field="value" data-bind="value:' + options.field + '"/>')
        .appendTo(container)
        .kendoDropDownList({
            template: "<img src='#=text#' />",
            valueTemplate: "<img src='#=text#' />",
            autoBind: false,
            dataTextField: "text",
            dataValueField: "value",
            dataSource: data,
            
        });
}

function DueFilter (e)
{
    $filter = new Array();

    $filter.push({ field: "Due", operator: "lte", value: new Date() });

    var grid = $("#main-task-grid").data("kendoGrid");
    grid.dataSource.filter({ logic: "or", filters: $filter });
}


function PriorityFilter() {
    $filter = new Array();

    if (parseInt($("#comboPriority").data("kendoDropDownList").value()) != -1)
        $filter.push({ field: "Priority", operator: "eq", value: $("#comboPriority").data("kendoDropDownList").value() });
    else
        $filter.push({ field: "OperationNumber", operator: "contains", value: "" });

    var grid = $("#main-task-grid").data("kendoGrid");
    grid.dataSource.filter({ logic: "or", filters: $filter });
}

function AllFilter(e) {
    $filter = new Array();
    $filter.push({ field: "OperationNumber", operator: "contains", value: "" });
    var grid = $("#main-task-grid").data("kendoGrid");
    grid.dataSource.filter({ logic: "or", filters: $filter });
}

function globalModalWindows(title,url) {
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append(
        '<div class="contenedor_general layoutDosColsIzda clauses_add_doc"> ' +
        '<div class="k-widget k-window" style="padding-top: 64px; min-width: 90px; min-height: 50px; width: 800px; position: fixed; top: 0px; left: 560px; display: block; opacity: 1; transform: scale(1);">' +
        '<div class="k-window-titlebar k-header" style="margin-top: -64px;">&nbsp;' +
        '<span class="k-window-title" style="right: 60px;" id="window1_wnd_title">'+title+'</span>' +
        '<div class="k-window-actions"><a class="k-window-action k-link" href="#" role="button"><span class="k-icon k-i-close" role="presentation">Close</span></a>' +
        '</div>' +
        '</div>' +
        '<div class="window k-window-content k-content windowOpen dinamicModal" id="window1" data-role="window" tabindex="0" role="dialog" aria-labelledby="window1_wnd_title" style="overflow: visible;"><div class="loadingdatawarning"><br>Loading...</div></div>' +
        '</div>' +
        '</div>');
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: url,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        deactivate: function () {
            $(".contenedor_general").remove();
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
            $(".k-window-titlebar").remove();
            //location.reload(true);
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

    modalTabstrips();
}

function showSaveChangesWindow() {
    // Get url to display modal window
    var urlRoute = $("#hdnSaveChangesUrl").val();


    // Display modal window to remove impact from the server side
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    $(".k-overlay").remove();

    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: urlRoute,
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
    dialog.open();
    $(".k-overlay").remove();
}

function cleanOperations() {

    var $operationStr = $('.js-operationStr'); 
    var str = $operationStr.text() 
    var resultStr = ''; 
    var arr = str.split(','); 

    if (arr.length > 2) {
        resultStr = arr.splice(0, 2).join(', ').concat('...'); 
    }

    $(".js-operationStr").text(resultStr); 
} 


