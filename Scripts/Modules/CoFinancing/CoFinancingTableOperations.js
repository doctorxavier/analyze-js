coFinancingTableOperations = {};

coFinancingTableOperations.collapseTablesCofinancing = function() {
    $('table.tableNormal').find('th.tree.icon').
        find('span.icon').not(':hidden').each(function () {
            var currentSpanIcon = $(this);
            var tableElement = currentSpanIcon.closest('table');
            var tableHeaderRows = tableElement.find("thead").find("tr");
            var sourceTable = tableElement.attr("id");

            if (currentSpanIcon.html() === "-") {
                tableElement.find('tbody tr:not(.template)').removeClass("hide");
                currentSpanIcon.closest('th').
                                find('span').last().removeClass("hide");
                if (tableHeaderRows.length > 0) {
                    tableHeaderRows.not(':first').removeClass("hide");
                }

                
                $('#PageContent div.col-md-12.noRecords.' + sourceTable).
                    removeClass("hide");
            } else {
                tableElement.find('tbody tr:not(.template)').
                            not('.notCollapse').addClass("hide");
                currentSpanIcon.closest('th').find('span').
                                last().addClass("hide");
                if (tableHeaderRows.length > 0) {
                    tableHeaderRows.not(':first').addClass("hide");
                }

                $('#PageContent div.col-md-12.noRecords.' + sourceTable).
                    addClass("hide");
            }
    });
}

coFinancingTableOperations.collapseTableCofinancing = function () {
    $("#PageContent span.singleTableCollapse").click(function () {
        var element = this;
        var collapseClasses = $(element).closest('table').
                            attr('class').replace("tableNormal ", "");

        var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' +
                                collapseClasses + '"]:visible');

        var informationSection = $(element).parents("tr").find("div.informationplace");

        var label = $(tempCollapseAll).find('label[dd-value]');
        var tempTexp = $(tempCollapseAll).text().trim();
        var elementTable = $(element).closest('table');
        var elementTableId = elementTable.attr("id");
        var elementTableHeaderRows = elementTable.find('thead').find('tr');
        var elementTableNotTemplateRows = elementTable.
                                            find('tbody tr:not(.template)');
        var elementTableTreeSpanColumn = elementTable.find('tbody').
                                            find('tr.notCollapse').
                                            find('td.tree').find('span');
        var elementTableNotIconSpan = $(element).closest('th').
                                                find('span').
                                                not('.icon');

        if ($(element).html() === "-") {
            elementTableNotTemplateRows.not('.notCollapse').addClass("hide");
            elementTableTreeSpanColumn.addClass("hide");
            elementTableNotIconSpan.addClass("hide");
            $(element).html('+');

            if (elementTableHeaderRows.length > 0) {
                elementTableHeaderRows.not(':first').addClass("hide");
            }

            if ($(tempCollapseAll).length > 0) {
                var elements = $(tempCollapseAll).attr('dd-contenedores');
                elements = elements.split(',');

                elements = jQuery.grep(elements, function (value) {
                    return value.length > 0;
                });

                var isActiveFlag = true;
                var x;
                var id;
                for (x = 0; x < elements.length; x++) {
                    id = "." + elements[x].trim();
                    if ($(id).length === 0 && id !== ".") {
                        showMessage("Element " + elements[x] +
                                    " does not exist in Collapsable All, please verify!");
                        return;
                    }
                }

                for (x = 0; x < elements.length; x++) {
                    id = "." + elements[x].trim();
                    if ($(id).is('table')) {
                        var hiddenBody = $(id).find('tbody tr').is(".hide") ||
                                ($(id).find('tbody').css("display") === "none");
                        if (!(hiddenBody) && id !== ".") {
                            isActiveFlag = false;
                            break;
                        }
                    } else {
                        if ($(id).css("display") !== "none" && id !== ".") {
                            isActiveFlag = false;
                            break;
                        }
                    }
                }

                if (isActiveFlag) {
                    $(tempCollapseAll).removeClass('collapse');
                    label = $(tempCollapseAll).find('label[dd-value]');
                    tempTexp = $(tempCollapseAll).text().trim();
                    $(label).text($(label).attr('dd-value').trim());
                    $(label).attr('dd-value', tempTexp);
                }
            }

            informationSection.addClass("hide");
            var newRowButtonContainer =
                $("#PageContent").find("div.buttonWrapper." + elementTableId);
                
            newRowButtonContainer.addClass('hide');
            newRowButtonContainer.find("button.addNewRow").addClass("hide");
        } else {
            elementTableNotTemplateRows.removeClass("hide");
            elementTableTreeSpanColumn.removeClass("hide");
            elementTableNotIconSpan.removeClass("hide");
            $(element).html('-');

            if (elementTableHeaderRows.length > 1) {
                elementTableHeaderRows.not(':first').removeClass("hide");
            }

            if ($(tempCollapseAll).length > 0) {
                if (!$(tempCollapseAll).is('.collapse')) {
                    $(tempCollapseAll).addClass('collapse');
                    $(label).text($(label).attr('dd-value'));
                    $(label).attr('dd-value', tempTexp);
                }
            }

            informationSection.removeClass("hide");
            var newRowButtonContainer =
                $("#PageContent").find("div.buttonWrapper." + elementTableId);

            newRowButtonContainer.removeClass('hide');
            newRowButtonContainer.find("button.addNewRow").removeClass("hide");
        }

        coFinancingTableOperations.collapseTablesCofinancing();

        $(element).css("display", "");
        $(element).closest('table.tableNormal').
                    find('table.tableNormal').
                    find('th.tree.icon').
                    find('span.icon')
                    .removeClass("hide");

        if ($(element).is('.hideHeader')) {
            var headers = $(element).closest('tr').find('th').not('.tree');
            if ($(element).text() === "+") {
                headers.addClass("hide");
                $(element).css('margin-top', 5);
            } else {
                headers.removeClass("hide");
                $(element).css('margin-top', '50%');
            }
        }
    });
}

coFinancingTableOperations.expandTableElement = function(element, elements) {
    $(element).addClass('collapse');
    var tempCollapseInterno, classes, x;

    for (x = 0; x < elements.length; x++) {
        classes = "." + elements[x].trim() + ':visible';
        tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' +
                                                                elements[x].trim()
                                                                + '"]');

        if ($(tempCollapseInterno).length > 0) {
            if (!$(tempCollapseInterno).is('.collapse')) {
                $(tempCollapseInterno).Collapsable();;
            }
        }

        if ($(classes).is('table')) {
            var tableRows = $(classes).find('tbody').find('tr');
            tableRows.removeClass("hide");
            tableRows.
                find('.notCollapse').
                find('td.tree').
                find('span').
                removeClass("hide");

            var treeIcon = $(classes).find('th.tree.icon');

            if (treeIcon.length > 0) {
                treeIcon.find('span.icon').first().
                    closest('th').find('span').
                        not('.icon').removeClass("hide");
                treeIcon.find('span.icon').first().html('-');

                var headerRows = $(classes).find('thead').find('tr');
                if (headerRows.length > 0) {
                    headerRows.not(':first').removeClass("hide");
                }
            }
        } else {
            $(classes).removeClass("hide");
        }

        var sourceTable = elements[x].trim();
        $('#PageContent div.col-md-12.noRecords.' + sourceTable).removeClass("hide");
    }
}

coFinancingTableOperations.collapseTableElement = function(element, elements) {
    $(element).removeClass('collapse');
    var tempCollapseInterno, classes, x;

    for (x = 0; x < elements.length; x++) {
        classes = "." + elements[x].trim() + ':visible';
        tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' +
                                                        elements[x].trim()
                                                        + '"]');

        if ($(tempCollapseInterno).length > 0) {
            if ($(tempCollapseInterno).is('.collapse')) {
                $(element).Expandable();
            }
        }

        if ($(classes).is('table')) {
            var tableRows = $(classes).find('tbody').
                                find('tr').not('.notCollapse');
            tableRows.addClass("hide");
            tableRows.find('td.tree span').addClass("hide");
            var treeIcon = $(classes).find('th.tree.icon');

            if (treeIcon.length > 0) {
                treeIcon.find('span.icon').first().
                    closest('th').find('span').not('.icon').addClass("hide");
                treeIcon.find('span.icon').first().html('+');

                var headerRows = $(classes).find('thead').find('tr');
                if (headerRows.length > 0) {
                    headerRows.not(':first').addClass("hide");
                }
            }

            var sourceTable = elements[x].trim();
            $('#PageContent div.col-md-12.noRecords.' + sourceTable).addClass("hide");
        } else {
            $(classes).addClass("hide");
        }

    }
}

coFinancingTableOperations.collapsableAllCoFinancingTables = function () {
    $("#PageContent .btnCollapseAll").click(function () {
        var element = this;
        var elements = $(element).attr('dd-contenedores').split(",");
        elements = $.grep(elements, function (n) { return (n) });
        var classes, x;

        for (x = 0; x < elements.length; x++) {
            classes = "." + elements[x].trim();
            if ($(classes).length === 0 && classes !== ".") {
                showMessage("This element " +
                elements[x] + " does not exist, please verify!");
                return;
            }
        }

        var label = $(element).find('label[dd-value]');
        var tempTexp = $(element).text();
        $(label).text($(label).attr('dd-value'));
        $(label).attr('dd-value', tempTexp);
        var newRowButtons = $('#PageContent').find('button.addNewRow');

        if ($(element).is('.collapse')) {
            coFinancingTableOperations.collapseTableElement(element, elements);
            $('#PageContent').find('table').find('th').
                find("div.informationplace").addClass("hide");

            newRowButtons.addClass("hide");
            newRowButtons.parents("div.buttonWrapper").addClass("hide");
        }
        else {
            coFinancingTableOperations.expandTableElement(element, elements);
            $('#PageContent').find('table').find('th').
                find("div.informationplace").removeClass("hide");

            newRowButtons.removeClass("hide");
            newRowButtons.parents("div.buttonWrapper").removeClass("hide");
        }
    });
}

coFinancingTableOperations.collapsesCall = function () {
    var pageContainer = $("#PageContent");

    pageContainer.find("span.singleTableCollapse").unbind("click");
    pageContainer.find(".btnCollapseAll").unbind("click");
    coFinancingTableOperations.collapsableAllCoFinancingTables();
    coFinancingTableOperations.collapseTableCofinancing();
}

