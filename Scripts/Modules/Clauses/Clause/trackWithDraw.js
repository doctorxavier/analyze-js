$(document).ready(function () {
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);
    
    $('.actionDeleteClause').click(function () {
        event.preventDefault();
        var result = confirm("This change will be registered on the Changes to the Contracts and Clauses section and you will be required to explain it. Would you like to proceed?");
        if (result == true) {
            redirectPage($(this).data("route"));
        }
    });

    $("#dropdownForCategory").kendoDropDownList({
        change: function (e) {
            var val = this.value();
            $("#dropdownForType").kendoDropDownList({
                dataTextField: "Text",
                dataValueField: "Value",
                dataSource: {
                    type: "jsonp",
                    transport: {
                        read: $('#divCategory').data('route') + '?categoryId=' + val
                    }
                }
            });
            var dropdownlist = $("#dropdownForType").data("kendoDropDownList");
            dropdownlist.refresh();
        }
    });
    $("#dropdownForType").kendoDropDownList({
        dataTextField: "Text",
        dataValueField: "Value",
        dataSource: {
            type: "jsonp",
            transport: {
                read: $('#divCategory').data('route') + '?categoryId=' + $("#dropdownForCategory").data("kendoDropDownList")._initial
            }
        }
    });

    var dropdownlist = $("#dropdownForType").data("kendoDropDownList");
    dropdownlist.refresh();

    //Mensaje tooltip de error para validaciones
    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            return $(this).attr('data-val-required');
        }
    });

    $('.k-dropdown-wrap.k-state-default').addClass('btn');
});

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
            idbg.lockUi(null, true);

            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    dialog.center();
    dialog.open();
}