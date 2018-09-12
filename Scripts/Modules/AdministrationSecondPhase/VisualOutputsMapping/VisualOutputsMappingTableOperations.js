var VOMTableOperations = {};

VOMTableOperations.addSortAndPagination = function (tableElement, resultsPerPage) {
    tableElement.paginationConfluense(resultsPerPage).sortableConfluense();
};

VOMTableOperations.selectUnselectCheckBoxes = function () {
    $("[name=selectUnselectVisualOutputs]").change(function () {
        var status = this.checked;
        var activeTab = $("div.tab-pane.active").find(".tabContent");

        activeTab.find('[name=selectedOperation]').each(function () {
            this.checked = status;

            if (status) {
                $(this).attr("checked", "checked");
            }

            if (!status) {
                $(this).removeAttr("checked");
            }
        });
    });
};

VOMTableOperations.singleCheckBoxChange = function () {
    $('[name=selectedOperation]').change(function () {

        var activeTab = $("div.tab-pane.active").find(".tabContent");
        var selectUnselectVisualOutputs =
                activeTab.find("[name=selectUnselectVisualOutputs]")[0];

        var $currentElement = $(this);

        if (!this.checked) {
            selectUnselectVisualOutputs.checked = false;
            $currentElement.removeAttr("checked");
            $(selectUnselectVisualOutputs).removeAttr("checked");
        }

        if (this.checked) {
            $currentElement.attr("checked", "checked");
        }

        if (activeTab.find('[name=selectedOperation]:checked').length ===
            activeTab.find('[name=selectedOperation]').length) {
            selectUnselectVisualOutputs.checked = true;
            $(selectUnselectVisualOutputs).attr("checked", "checked");
        }
    });
};

VOMTableOperations.recordsDisplayedSelectionChange = function () {
    $('[name=resultPerPageFilter]').change(function () {

        var activeTab = $("div.tab-pane.active").find(".tabContent");

        var currentTable = activeTab.find("table");

        VOMTableOperations.addSortAndPagination(currentTable, parseInt($(this).GetValue()));
        VOMTableOperations.singleCheckBoxChange();
        VOMTableOperations.selectUnselectCheckBoxes();
    });
};

VOMTableOperations.handlePaginationWhenActiveTabChanges = function () {
    $('ul.tabs').find('li').click(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");
        var currentTable = $("#" + activeTab.parent().data("tableid"));

        if (currentTable.find('tbody').find('tr').length > 0) {
            VOMTableOperations.addSortAndPagination(currentTable, 10);
            VOMTableOperations.singleCheckBoxChange();
            VOMTableOperations.selectUnselectCheckBoxes();
            VOMLoad.importVisualOutputsDataFile();
        }
    });
};

VOMTableOperations.loadSelectedOperationsIds = function () {
    var currentTableRows = $("div.tab-pane.active").
                            find(".tabContent").
                            find("table").
                            find("tbody").
                            find("tr");

    return $.map(currentTableRows, function (element) {
        if ($(element).find("[name=selectedOperation]").prop("checked")) {
            return $(element).attr("data-id");
        }
    }).join(", ");
};

VOMTableOperations.loadSelectedOperationsNumbers = function () {
    var currentTableRows = $("div.tab-pane.active").
                            find(".tabContent").
                            find("table").
                            find("tbody").
                            find("tr");

    return $.map(currentTableRows, function (element) {
        if ($(element).find("[name=selectedOperation]").prop("checked")) {
            return $(element).find("[name=operationNumber]").val().trim();
        }
    }).join(", ");
};
