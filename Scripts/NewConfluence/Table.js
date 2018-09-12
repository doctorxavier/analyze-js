function alternateRowColor(element) {
    if (element === undefined || element === null) {
        element = $(document).find('[data-row-parent-id]').closest('table');
    } else {
        element = $(element).find('[data-row-parent-id]').closest('table');
    }

    $(element).find('.customRowOdd').removeClass('customRowOdd');
    $(element).find('.customRowEven').removeClass('customRowEven');

    $(element).find('tr[data-id]').each(function (index, row) {
        if (index % 2 !== 0) {
            $(row).addClass('customRowOdd');
        } else {
            $(row).addClass('customRowEven');
        }
    });
}

function paginationEndTable(table) {
    if ($(table).find('tbody').find('.tableNormalBorder').length > 0) {
        $(table).find('tbody').find('.tableNormalBorder').removeClass('tableNormalBorder');
        $(table).find('tbody').find('tr[data-id]').not('.hide').last().addClass('tableNormalBorder');
    }
}

function PaginationMove(element) {
    var active = $(element).closest(".Pagination").find('.Pagination_Active').attr('dd-num') * 1;

    if ($(element).is('.Pagination_Next')) {
        var limite = $(element).closest(".Pagination").find('.Pagination_Points').last().attr('dd-num') * 1;
        if (isNaN(limite)) {
            limite = $(element).closest(".Pagination").find('.Pagination_Number').length;
        }
        if (active + 1 < limite + 1) {
            $(element).closest(".Pagination").find('.Pagination_Number[dd-num="' + (active + 1) + '"]').click();
        }
    }

    if ($(element).is('.Pagination_Prev')) {
        if (active - 1 > 0) {
            $(element).closest(".Pagination").find('.Pagination_Number[dd-num="' + (active - 1) + '"]').click();
        }
    }
}

function colapsableOne(element) {
    var content = "#" + $(element).attr('dd-contenedor');
    var collapseAll = $(".btnCollapseAll");
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).attr('dd-contenedor') + '"]');

    if ($(content).length > 0) {
        var label = $(element).find('label[dd-value]');
        var tempTexp = $(element).text().trim();
        $(label).text($(label).attr('dd-value'));
        $(label).attr('dd-value', tempTexp);
        if ($(element).is('.collapse')) {
            $(element).removeClass('collapse');
            if ($(tempCollapseAll).length > 0) {
                var elements = $(tempCollapseAll).attr('dd-contenedores').replace($(element).attr('dd-contenedor') + ",", "");
                elements = elements.split(',');
                var banderaActivo = true;
                var id;
                var x;
                for (x = 0; x < elements.length; x++) {
                    id = "#" + elements[x].trim();
                    if ($(id).length === 0 && id !== "#") {
                        showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                        return;
                    }
                }

                for (x = 0; x < elements.length; x++) {
                    id = "#" + elements[x].trim();
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }

                if (banderaActivo) {
                    if ($(tempCollapseAll).length > 0) {
                        if ($(tempCollapseAll).is('.collapse')) {
                            $(collapseAll).Expandable();
                        }
                    }
                    $(element).Expandable();
                }
            }

            if ($(content).is('table')) {
                $(content).find('button.buttonShowRow').removeClass('active');
                $(content).find('.lineup').removeClass('lineup');
                $(content).find('.linedown').removeClass('linedown');
                $(content).find('.showChildRow').removeClass('showChildRow');
                $(content).find('.buttonShowRowExpand.icon').removeClass('active').text('+');
            } else {
                $(content).addClass("hide");
            }
        }
        else {
            $(element).addClass('collapse');
            if ($(content).is('table')) {

                $(content).find('button.buttonShowRow').each(function () {
                    var id = $(this).closest('tr').attr("data-id");
                    var row = $(this).closest('tbody').find('[data-row-parent-id="' + id + '"]');
                    $(row).removeClass("hide");
                    $(this).addClass('active');
                    $(this).closest('tr').addClass('lineup');
                    $(row).last().addClass('linedown');
                    if ($(this).closest('tr').css('display') !== "none") {
                        $(row).addClass("showChildRow");
                    }
                });

                $(content).find('.buttonShowRowExpand.icon').each(function () {
                    var id = $(this).closest('tr').attr("data-id");
                    var row = $(this).closest('tbody').find('[data-row-parent-id="' + id + '"]');
                    $(row).removeClass("hide");
                    $(this).addClass('active');
                    $(this).text('-');
                    if ($(this).closest('tr').css('display') !== "none") {
                        $(row).addClass("showChildRow");
                    }
                });

            } else {
                $(content).removeClass("hide");
            }

            if ($(tempCollapseAll).length > 0) {
                if (!$(tempCollapseAll).is('.collapse')) {
                    $(collapseAll).Collapsable();
                }
            }
        }
    }
    else {
        showMessage("This element " + $(element).attr('dd-contenedor') + " not exist, please verify!");
    }
}

function collapsibleOneContainer(element) {
    var content = "#" + $(element).attr('dd-contenedor');
    if ($(content).length > 0) {
        var collapseAll = $(".btnCollapseAll");
        var tempCollapseAll;

        if ($(element).is('.collapse')) {
            $(element).find("span.icon").html("+");
            $(element).removeClass('collapse');
            tempCollapseAll = $('div.btnCollapseAll[dd-contenedores*="' + $(element).attr('dd-contenedor') + '"]');
            if ($(tempCollapseAll).length > 0) {
                var elements = $(tempCollapseAll).attr('dd-contenedores').replace($(element).attr('dd-contenedor') + ",", "");
                elements = elements.split(',');
                var banderaActivo = true;
                var id;
                var x;
                validateElements(true, elements);

                for (x = 0; x < elements.length; x++) {
                    id = "#" + elements[x].trim();
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }

                if (banderaActivo) {
                    if ($(tempCollapseAll).length > 0) {
                        if ($(tempCollapseAll).is('.collapse')) {
                            $(collapseAll).Expandable();
                        }
                    }
                }
            }
            $(content).addClass("hide");
        }
        else {
            $(element).find("span.icon").html("-");
            $(element).addClass('collapse');
            tempCollapseAll = $('div.btnCollapseAll[dd-contenedores*="' + $(element).attr('dd-contenedor') + '"]');
            if ($(tempCollapseAll).length > 0) {
                if (!$(tempCollapseAll).is('.collapse')) {
                    $(collapseAll).Collapsable();
                }
            }

            $(content).removeClass("hide");
        }
    }
    else {
        showMessage("This element " + $(element).attr('dd-contenedor') + " not exist, please verify!");
    }
}

function validateElements(expandable, elements) {
    var id;
    var x;
    if (expandable) {
        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            if ($(id).length === 0 && id !== "#") {
                showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                return;
            }
        }
    } else {
        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            if ($(id).length === 0 && id !== "#") {
                showMessage("This element " + elements[x] + " not exist, please verify!");
                return;
            }
        }
    }
}

function colapsableAllRiskMatrix(element) {
    var elements = $(element).attr('dd-contenedores').split(",");
    elements = $.grep(elements, function (n) { return (n) });
    var id;
    var x;
    for (x = 0; x < elements.length; x++) {
        id = "#" + elements[x].trim();
        if ($(id).length === 0 && id !== "#") {
            showMessage("This element " + elements[x] + " not exist, please verify!");
            return;
        }
    }

    var label = $(element).find('label[dd-value]');
    var tempTexp = $(element).text().trim();
    $(label).text($(label).attr('dd-value'));
    $(label).attr('dd-value', tempTexp);
    var tempCollapseInterno;
    if ($(element).is('.collapse')) {
        $(element).removeClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if ($(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Expandable();
                }
            }

            tempCollapseInterno = $('div.btnCollapseOne.btnCollapseOneContainer[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if ($(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Expandable();
                }
            }

            if ($(id).is('table')) {
                $(id).find('tbody tr').not('.notCollapse').addClass("hide");
                $(id).find('tbody tr.notCollapse td.tree span').addClass("hide");
                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').each(function () {
                        $(this).html('+');
                        $(this).closest('th').find('span').not('.icon').addClass("hide");
                    });

                    $(id).find('td.tree.icon span.icon').each(function () {
                        $(this).html('+');
                    });

                    if ($(id).find('thead tr').length > 0) {
                        $(id).find('thead tr').not(':first').addClass("hide");
                    }
                }
            } else {
                $(id).addClass("hide");
            }
        }
    }
    else {
        $(element).addClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if (!$(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Collapsable();
                }
            }

            tempCollapseInterno = $('div.btnCollapseOne.btnCollapseOneContainer[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if (!$(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Collapsable();
                }
            }

            if ($(id).is('table')) {
                $(id).find('tbody tr').removeClass("hide");
                $(id).find('tbody tr.notCollapse td.tree span').removeClass("hide");
                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').each(function () {
                        $(this).html('-');
                        $(this).closest('th').find('span').not('.icon').removeClass("hide");
                    });

                    $(id).find('td.tree.icon span.icon').each(function () {
                        $(this).html('-');
                    });

                    if ($(id).find('thead tr').length > 0) {
                        $(id).find('thead tr').not(':first').removeClass("hide");
                    }
                }
            } else {
                $(id).removeClass("hide");
            }
        }
    }
}

function colapsableAll(element) {
    var elements = $(element).attr('dd-contenedores').split(",");
    elements = $.grep(elements, function (n) { return (n) });
    var id;
    var x;
    for (x = 0; x < elements.length; x++) {
        id = "#" + elements[x].trim();
        if ($(id).length === 0 && id !== "#") {
            showMessage("This element " + elements[x] + " not exist, please verify!");
            return;
        }
    }

    var label = $(element).find('label[dd-value]');
    var tempTexp = $(element).text().trim();
    $(label).text($(label).attr('dd-value'));
    $(label).attr('dd-value', tempTexp);
    var tempCollapseInterno;
    if ($(element).is('.collapse')) {
        $(element).removeClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if ($(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Expandable();
                }
            }

            tempCollapseInterno = $('div.btnCollapseOne.btnCollapseOneContainer[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if ($(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Expandable();
                }
            }

            if ($(id).is('table')) {
                $(id).find('tbody tr').not('.notCollapse').addClass("hide");
                $(id).find('tbody tr.notCollapse td.tree span').addClass("hide");
                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').each(function () {
                        $(this).html('+');
                        $(this).closest('th').find('span').not('.icon').addClass("hide");
                    });

                    if ($(id).find('thead tr').length > 0) {
                        $(id).find('thead tr').not(':first').addClass("hide");
                    }
                }
            } else {
                $(id).addClass("hide");
            }
        }
    }
    else {
        $(element).addClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();
            tempCollapseInterno = $('div.btnCollapseOne.buttonExpand[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if (!$(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Collapsable();
                }
            }

            tempCollapseInterno = $('div.btnCollapseOne.btnCollapseOneContainer[dd-contenedor="' + elements[x].trim() + '"]');
            if ($(tempCollapseInterno).length > 0) {
                if (!$(tempCollapseInterno).is('.collapse')) {
                    $(tempCollapseInterno).Collapsable();
                }
            }

            if ($(id).is('table')) {
                $(id).find('tbody tr').removeClass("hide");
                $(id).find('tbody tr.notCollapse td.tree span').removeClass("hide");
                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').each(function () {
                        $(this).html('-');
                        $(this).closest('th').find('span').not('.icon').removeClass("hide");
                    });

                    if ($(id).find('thead tr').length > 0) {
                        $(id).find('thead tr').not(':first').removeClass("hide");
                    }
                }
            } else {
                $(id).removeClass("hide");
            }
        }
    }
}

function CollapsableAllSubTables(element) {
    var elements = $(element).attr('dd-contenedores').split(",");
    elements = $.grep(elements, function (n) { return (n) });
    var id;
    var x;
    for (x = 0; x < elements.length; x++) {
        id = "#" + elements[x].trim();
        if ($(id).length === 0 && id !== "#") {
            showMessage("This element " + elements[x] + " not exist, please verify!");
            return;
        }
    }

    var label = $(element).find('label[dd-value]');
    var tempTexp = $(element).text();
    $(label).text($(label).attr('dd-value'));
    $(label).attr('dd-value', tempTexp);

    if ($(element).is('.collapse')) {
        $(element).removeClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();

            if ($(id).is('table')) {
                $(id).find('table.collapsibleNew').addClass("hide");
                $(id).find('tbody td.tree span').addClass("hide");

                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').first().closest('th').find('span').not('.icon').addClass("hide");
                    $(id).find('th.tree.icon span.icon').first().html('+');
                }
            } else {
                $(id).addClass("hide");
            }
        }
    }
    else {
        $(element).addClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();

            if ($(id).is('table')) {
                $(id).find('table.collapsibleNew').removeClass("hide");
                $(id).find('tbody td.tree span').removeClass("hide");

                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').first().closest('th').find('span').not('.icon').removeClass("hide");
                    $(id).find('th.tree.icon span.icon').first().html('-');
                }
            } else {
                $(id).removeClass("hide");
            }
        }
    }
}

function CollapsableAllSubTablesRiskMatrix(element) {
    var elements = $(element).attr('dd-contenedores').split(",");
    elements = $.grep(elements, function (n) { return (n) });
    var id;
    var x;
    for (x = 0; x < elements.length; x++) {
        id = "#" + elements[x].trim();
        if ($(id).length === 0 && id !== "#") {
            showMessage("This element " + elements[x] + " not exist, please verify!");
            return;
        }
    }

    var label = $(element).find('label[dd-value]');
    var tempTexp = $(element).text();
    $(label).text($(label).attr('dd-value'));
    $(label).attr('dd-value', tempTexp);

    if ($(element).is('.collapse')) {
        $(element).removeClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();

            if ($(id).is('table')) {
                $(id).find('table.collapsibleNew').addClass("hide");
                $(id).find('tbody td.tree span').addClass("hide");

                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').first().closest('th').find('span').not('.icon').addClass("hide");
                    $(id).find('th.tree.icon span.icon').first().html('+');
                }
            } else {
                $(id).addClass("hide");
            }
        }
    }
    else {
        $(element).addClass('collapse');

        for (x = 0; x < elements.length; x++) {
            id = "#" + elements[x].trim();

            if ($(id).is('table')) {
                $(id).find('table.collapsibleNew').removeClass("hide");
                $(id).find('tbody td.tree span').removeClass("hide");

                if ($(id).find('th.tree.icon').length > 0) {
                    $(id).find('th.tree.icon span.icon').first().closest('th').find('span').not('.icon').removeClass("hide");
                    $(id).find('th.tree.icon span.icon').first().html('-');
                }
            } else {
                $(id).removeClass("hide");
            }
        }
    }
}

function CollapseTableRiskMatrix(element) {
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).closest('table').attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();
    if ($(element).html() === "-") {
        $(element).closest('table').find('tbody tr:not(.template)').not('.notCollapse').each(function () {
            $this = $(this);
            if ($this.is('.showChildRow')) {
                var idRow = $this.attr('data-row-parent-id');
                $('tr[data-id="' + idRow + '"]').find('.buttonShowRow.active').click();
            }
        }).addClass("hide");

        $(element).closest('table').find('tbody tr.notCollapse td.tree span').addClass("hide");
        $(element).closest('th').find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(element).closest('table').find('thead tr').length > 0) {
            $(element).closest('table').find('thead tr').not(':first').addClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace($(element).closest('table').attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    var trAllHide = true;
                    $(id).find('tbody tr').each(function () {
                        if (!$(this).is(".hide")) {
                            trAllHide = false;
                        }
                    });

                    if (!(trAllHide || $(id).find('tbody').css("display") === "none") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    } else {
        $(element).closest('table').find('tbody tr:not(.template)').removeClass("hide");
        $(element).closest('table').find('tbody tr.notCollapse td.tree span').removeClass("hide");
        $(element).closest('th').find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        if ($(element).closest('table').find('thead tr').length > 1) {
            $(element).closest('table').find('thead tr').not(':first').removeClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    //CollapseTables();
    $(element).css("display", "");
    $(element).closest('table.tableNormal').find('table.tableNormal th.tree.icon span.icon').removeClass("hide");

    if ($(element).is('.hideHeader')) {
        var cabeceras = $(element).closest('tr').find('th').not('.tree');
        if ($(element).text() === "+") {
            cabeceras.addClass("hide");
            $(element).css('margin-top', 5);
        } else {
            cabeceras.removeClass("hide");
            $(element).css('margin-top', '50%');
        }
    }
}

function CollapseTable(element) {
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).closest('table').attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();
    if ($(element).html() === "-") {
        $(element).closest('table').find('tbody tr:not(.template)').not('.notCollapse').each(function () {
            $this = $(this);
            if ($this.is('.showChildRow')) {
                var idRow = $this.attr('data-row-parent-id');
                $('tr[data-id="' + idRow + '"]').find('.buttonShowRow.active').click();
            }
        }).addClass("hide");
        
        $(element).closest('table').find('tbody tr.notCollapse td.tree span').addClass("hide");
        $(element).closest('th').find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(element).closest('table').find('thead tr').length > 0) {
            $(element).closest('table').find('thead tr').not(':first').addClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace($(element).closest('table').attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    var trAllHide = true;
                    $(id).find('tbody tr').each(function () {
                        if (!$(this).is(".hide")) {
                            trAllHide = false;
                        }
                    });

                    if (!(trAllHide || $(id).find('tbody').css("display") === "none") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    } else {
        $(element).closest('table').find('tbody tr:not(.template)').removeClass("hide");
        $(element).closest('table').find('tbody tr.notCollapse td.tree span').removeClass("hide");
        $(element).closest('th').find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        if ($(element).closest('table').find('thead tr').length > 1) {
            $(element).closest('table').find('thead tr').not(':first').removeClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    CollapseTables();
    $(element).css("display", "");
    $(element).closest('table.tableNormal').find('table.tableNormal th.tree.icon span.icon').removeClass("hide");

    if ($(element).is('.hideHeader')) {
        var cabeceras = $(element).closest('tr').find('th').not('.tree');
        if ($(element).text() === "+") {
            cabeceras.addClass("hide");
            $(element).css('margin-top', 5);
        } else {
            cabeceras.removeClass("hide");
            $(element).css('margin-top', '50%');
        }
    }
}

function CollapseAllTableRiskMatrix(element) {
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).closest('table').attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();
    if ($(element).html() === "-") {
        $(element).closest('table').find('tbody tr:not(.template)').not('.notCollapse').each(function () {
            $this = $(this);
            if ($this.is('.showChildRow')) {
                var idRow = $this.attr('data-row-parent-id');
                $('tr[data-id="' + idRow + '"]').find('.buttonShowRow.active').click();
            }
        }).addClass("hide");

        $(element).closest('table').find('tbody tr.notCollapse td.tree span').addClass("hide");
        $(element).closest('th').find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(element).closest('table').find('thead tr').length > 0) {
            $(element).closest('table').find('thead tr').not(':first').addClass("hide");
        }

        $(element).closest('table').find('td.tree.icon span.icon').each(function () {
            $(this).html('+');
        });

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace($(element).closest('table').attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    var trAllHide = true;
                    $(id).find('tbody tr').each(function () {
                        if (!$(this).is(".hide")) {
                            trAllHide = false;
                        }
                    });

                    if (!(trAllHide || $(id).find('tbody').css("display") === "none") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    } else {
        $(element).closest('table').find('tbody tr:not(.template)').removeClass("hide");
        $(element).closest('table').find('tbody tr.notCollapse td.tree span').removeClass("hide");
        $(element).closest('th').find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        $(element).closest('table').find('td.tree.icon span.icon').each(function () {
            $(this).html('-');
        });

        $(element).closest('table').find('th.tree.icon span.icon').each(function () {
            $(this).html('-');
        });

        if ($(element).closest('table').find('thead tr').length > 1) {
            $(element).closest('table').find('thead tr').not(':first').removeClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    CollapseTablesRiskMatrix();
    $(element).css("display", "");
    $(element).closest('table.tableNormal').find('table.tableNormal th.tree.icon span.icon').removeClass("hide");

    if ($(element).is('.hideHeader')) {
        var cabeceras = $(element).closest('tr').find('th').not('.tree');
        if ($(element).text() === "+") {
            cabeceras.addClass("hide");
            $(element).css('margin-top', 5);
        } else {
            cabeceras.removeClass("hide");
            $(element).css('margin-top', '50%');
        }
    }
}

function CollapseTablesRiskMatrix() {
    $('table.tableNormal th.tree.icon span.icon').not(':hidden').each(function () {
        if ($(this).html() === "-") {
            $(this).closest('table').find('tbody tr:not(.template)').removeClass("hide");
            $(this).closest('th').find('span').last().removeClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').removeClass("hide");
            }
        } else {
            $(this).closest('table').find('tbody tr:not(.template)').not('.notCollapse').addClass("hide");
            $(this).closest('th').find('span').last().addClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').addClass("hide");
            }
        }
    });
}

function CollapseTables() {
    $('table.tableNormal th.tree.icon span.icon').not(':hidden').each(function () {
        if ($(this).html() === "-") {
            $(this).closest('table').find('tbody tr:not(.template)').removeClass("hide");
            $(this).closest('th').find('span').last().removeClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').removeClass("hide");
            }
        } else {
            $(this).closest('table').find('tbody tr:not(.template)').not('.notCollapse').addClass("hide");
            $(this).closest('th').find('span').last().addClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').addClass("hide");
            }
        }
    });
}

function CollapseRowTable(element) {
    var id = $(element).closest('tr').attr("data-id");
    var row = $(element).closest('tbody').find('[data-row-parent-id="' + id + '"]').not('.showChildRowPermanent');
    var isButton = $(element).is('.icon');
    var tempTexp;
    var label;
    var colapsable;
    var rowExpandido;

    row.removeClass('hide');
    row.removeClass('hidden');

    if ($(element).is('.buttonShowRowExpand ')) {

        if ($(element).text().trim() === "+") {
            row.addClass('showChildRow');
            $(element).addClass("active");
            $(element).text('-');
            if ($(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]').length > 0) {
                rowExpandido = true;
                $(element).closest('tbody').find('button.buttonShowRow').each(function () {
                    if (!$(this).is('.active')) {
                        rowExpandido = false;
                    }
                });

                if (rowExpandido) {
                    colapsable = $(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]');
                    if (!colapsable.is('.collapse')) {
                        colapsable.addClass('collapse');
                        label = colapsable.find('label[dd-value]');
                        tempTexp = label.text();
                        $(label).text($(label).attr('dd-value'));
                        $(label).attr('dd-value', tempTexp);
                    }
                }
            }
        }
        else {
            row.removeClass('showChildRow');
            $(element).removeClass("active");

            $(element).text('+');

            if ($(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]').length > 0) {
                rowExpandido = false;
                $(element).closest('tbody').find('button.buttonShowRow').each(function () {
                    if ($(this).is('.active')) {
                        rowExpandido = true;
                    }
                });

                if (!rowExpandido) {
                    colapsable = $(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]');
                    if (colapsable.is('.collapse')) {
                        colapsable.removeClass('collapse');
                        label = colapsable.find('label[dd-value]');
                        tempTexp = label.text();
                        $(label).text($(label).attr('dd-value'));
                        $(label).attr('dd-value', tempTexp);
                    }
                }
            }
        }

    } else {
        if (!$(element).is('.active')) {
            row.addClass('showChildRow');
            $(element).addClass("active");

            $(element).closest('tr').addClass('lineup');
            $(row).last().addClass('linedown');

            if ($(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]').length > 0) {
                rowExpandido = true;
                $(element).closest('tbody').find('button.buttonShowRow').each(function () {
                    if (!$(this).is('.active')) {
                        rowExpandido = false;
                    }
                });

                if (rowExpandido) {
                    colapsable = $(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]');
                    if (!colapsable.is('.collapse')) {
                        colapsable.addClass('collapse');
                        label = colapsable.find('label[dd-value]');
                        tempTexp = label.text();
                        $(label).text($(label).attr('dd-value'));
                        $(label).attr('dd-value', tempTexp);
                    }
                }
            }
        } else {
            row.removeClass('showChildRow');
            $(element).removeClass("active");

            $(element).closest('tr').removeClass('lineup');
            $(row).last().removeClass('linedown');

            if ($(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]').length > 0) {
                rowExpandido = false;
                $(element).closest('tbody').find('button.buttonShowRow').each(function () {
                    if ($(this).is('.active')) {
                        rowExpandido = true;
                    }
                });

                if (!rowExpandido) {
                    colapsable = $(document).find('[dd-contenedor="' + $(element).closest('table').attr('id') + '"]');
                    if (colapsable.is('.collapse')) {
                        colapsable.removeClass('collapse');
                        label = colapsable.find('label[dd-value]');
                        tempTexp = label.text();
                        $(label).text($(label).attr('dd-value'));
                        $(label).attr('dd-value', tempTexp);
                    }
                }
            }
        }
    }
}

function CollapseRowTableRiskMatrix(element) {
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).closest('table').attr('id') + '"]');
    var id = $(element).closest('tr').attr("data-id");
    var row = $(element).closest('tbody').find('[data-row-response-plan="' + id + '"]').not('.showChildRowPermanent');
    var isButton = $(element).is('.icon');
    var tempTexp;
    var label;
    var colapsable;
    var rowExpandido;

    row.removeClass('hide');
    row.removeClass('hidden');

        if ($(element).text().trim() === "+") {
            row.addClass('showChildRow');

            $.each(row.find('.hide').not('span.validation-element').not('input.hide'), function (index, value) {
                $(value).removeClass('hide');                
            });

            $(element).text('-');
            row.find('th.tree.icon').find('span.icon').text('-');
            IsAllCollapseRiskMatrix(tempCollapseAll);

        } else {
            row.removeClass('showChildRow');
            row.addClass('hide');
            $.each(row.find('tr'), function (index, value) {
                $(value).addClass('hide');
            });

            $(element).text('+');
            row.find('th.tree.icon').find('span.icon').text('+');
            IsAllCollapseRiskMatrix(tempCollapseAll);

        }    
}

function IsAllCollapseRiskMatrix(tempCollapseAll) {
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim()
    var isMore = true;
    var isLess = true;

    $.each($('td.tree.icon span.icon'), function (index, value) {
        if ($(value).text().trim() === "-") {
            isMore = false;
            return false;
        }
    });

    $.each($('td.tree.icon span.icon'), function (index, value) {
        if ($(value).text().trim() === "+") {
            isLess = false;
            return false;
        }
    });

    if (isMore && !isLess) {
        if (tempCollapseAll.hasClass('collapse')) {
            $(tempCollapseAll).removeClass('collapse');
            label = $(tempCollapseAll).find('label[dd-value]');
            tempTexp = $(tempCollapseAll).text().trim();
            $(label).text($(label).attr('dd-value').trim());
            $(label).attr('dd-value', tempTexp);
        }
    } else if (!isMore && isLess) {
        if (!tempCollapseAll.hasClass('collapse')) {
            $(tempCollapseAll).addClass('collapse');
            $(label).text($(label).attr('dd-value'));
            $(label).attr('dd-value', tempTexp);
        }
    }
}

function LinesCollapse(contenedor) {
    contenedor.find('.buttonShowRow').each(function () {
        var id = $(this).closest('tr').attr("data-id");
        var row = $(this).closest('tbody').find('[data-row-parent-id="' + id + '"]');

        if ($(this).is('.active')) {
            $(this).closest('tr').addClass('lineup');
            $(row).last().addClass('linedown');
        } else {
            $(this).closest('tr').removeClass('lineup');
            $(row).last().removeClass('linedown');
        }
    });
}

function CollapseSubTableRiskMatrix(element) {
    var tableParent = $(element).closest('table');
    var tableCollapsible = $(element).closest('table').find('table.collapsibleNew');
    var th = $(element).closest('th');

    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + tableParent.attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();

    if ($(element).html() === "-") {
        tableCollapsible.addClass("hide");

        tableParent.find('tbody td.tree span').addClass("hide");

        th.find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace(tableParent.attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    if (!$(id).find('tbody td.tree span').is(".hide") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    else {
        tableCollapsible.removeClass("hide");
        tableParent.find('tbody td.tree span').removeClass("hide");
        th.find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
}

function CollapseSubTable(element) {
    var tableParent = $(element).closest('table');
    var tableCollapsible = $(element).closest('table').find('table.collapsibleNew');
    var th = $(element).closest('th');

    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + tableParent.attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();

    if ($(element).html() === "-") {
        tableCollapsible.addClass("hide");

        tableParent.find('tbody td.tree span').addClass("hide");

        th.find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace(tableParent.attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    if (!$(id).find('tbody td.tree span').is(".hide") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    else {
        tableCollapsible.removeClass("hide");
        tableParent.find('tbody td.tree span').removeClass("hide");
        th.find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
}

function CollapseAllSubTableRiskMatrix(element) {
    var tableParent = $(element).closest('table');
    var tableCollapsible = $(element).closest('table').find('table.collapsibleNew');
    var th = $(element).closest('th');

    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + tableParent.attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();

    if ($(element).html() === "-") {
        tableCollapsible.addClass("hide");

        tableParent.find('tbody td.tree span').addClass("hide");

        th.find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(tempCollapseAll).length > 0) {
            var elements = $(tempCollapseAll).attr('dd-contenedores').replace(tableParent.attr('id') + ",", "");
            elements = elements.split(',');

            elements = jQuery.grep(elements, function (value) {
                return value.length > 0;
            });

            var banderaActivo = true;
            var x;
            var id;
            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).length === 0 && id !== "#") {
                    showMessage("Element " + elements[x] + " not exist in Collapsable All, please verify!");
                    return;
                }
            }

            for (x = 0; x < elements.length; x++) {
                id = "#" + elements[x].trim();
                if ($(id).is('table')) {
                    if (!$(id).find('tbody td.tree span').is(".hide") && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                } else {
                    if ($(id).css("display") !== "none" && id !== "#") {
                        banderaActivo = false;
                        break;
                    }
                }
            }

            if (banderaActivo) {
                $(tempCollapseAll).removeClass('collapse');
                label = $(tempCollapseAll).find('label[dd-value]');
                tempTexp = $(tempCollapseAll).text().trim();
                $(label).text($(label).attr('dd-value').trim());
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
    else {
        tableCollapsible.removeClass("hide");
        tableParent.find('tbody td.tree span').removeClass("hide");
        th.find('span').not('.icon').removeClass("hide");
        $(element).html('-');

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }
    }
}

function sortableMiTable(tabla, order, index) {
    tabla.find('thead th').each(function () {
        var th = $(this);
        if (!th.is('.sort')) {
            th.removeClass('sorting_asc')
            .removeClass('sorting')
            .removeClass('sorting_desc')
            .removeClass('sorting_disabled');
        } else {
            th.addClass('sortTing')
            .removeClass('sortAsc')
            .removeClass('sortDes');
        }
    });


    if (index === undefined || index === "") {
        index = tabla.find('thead th.sort').index();
    }

    orderTable(tabla, index, order, tabla.find('thead th.sort').attr('order-type'));

    tabla.find('.sort').off('click');
    tabla.find('.sort').click(function () {
        var th = $(this);
        if (th.is('.disableSort')) {
            return false;
        }

        showLoader();
        var table = th.closest('table');

        if (th.is('.sortTing')) {
            orderTable(table, th.index(), true, th.attr('order-type'));
        } else {
            if (th.is('.sortDes')) {
                orderTable(table, th.index(), true, th.attr('order-type'));
            } else {
                orderTable(table, th.index(), false, th.attr('order-type'));
            }
        }

        var pagination = $('[dd-table="' + table.attr('id') + '"].Pagination');
        if (pagination.length === 1) {
            table.find('tbody tr').addClass('hide');
            table.find('.tableNormalBorder').removeClass('tableNormalBorder');
            var rowsTable = table.find('tbody tr[data-id]').length
            var rows = pagination.attr('dd-row') * 1;
            rows = rowsTable < rows ? rowsTable : rows;
            var active = pagination.find('.Pagination_Active').attr('dd-num') - 1;
            active *= rows;
            var i;
            for (i = 0; i < rows; i++) {
                if (rows - 1 === i) {
                    $(table.find('tbody tr[data-id]')[active + i]).addClass('tableNormalBorder');
                }
                $(table.find('tbody tr[data-id]')[active + i]).removeClass('hide');
            }

            if (table.find('tbody tr[data-row-parent-id]').length > 0) {
                table.find('tbody tr[data-row-parent-id].showChildRow').removeClass('showChildRow');

                rows = table.find('tbody tr[data-id]').not('.hide');
                for (i = 0; i < rows.length; i++) {
                    var element = $(rows[i]);
                    if ($(element).is('.lineup')) {
                        table.find('tbody tr[data-row-parent-id="' + element.attr('data-id') + '"]').removeClass('hide').addClass('showChildRow');
                    }
                };
            }
        }
        alternateRowColor(table);
        hideLoader();

        if ($(table).hasClass('tableSelectable')) {
            $(table).selectableConfluence();
        }

        if (table.find('td.tree').length > 0) {
            table.FixLastRowLine();
        }
        
        return true;
    });
}

function orderTable(element, index, order, type) {
    if (!element) {
        return 0;
    }

    if (!index) {
        index = 0;
    }

    if (order === undefined) {
        order = true;
    }

    if (type === undefined) {
        type = "text";
    }

    $(element).find('thead th.sort').removeClass('sortAsc').removeClass('sortDes').addClass('sortTing');
    if (order) {
        $($(element).find('thead th')[index]).removeClass('sortTing').addClass('sortAsc');
    } else {
        $($(element).find('thead th')[index]).removeClass('sortTing').addClass('sortDes');
    }

    var list = new Array();
    var rows = new Array();

    $(element).find('tbody >  tr').not($(element).find('table tbody tr')).each(function () {
        var tr = $(this);
        if (tr.is('[data-id]')) {
            var sonList = new Array();
            tr.closest('tbody').find('tr[data-row-parent-id="' + tr.attr('data-id') + '"]').each(function () {
                sonList.push($(this).index());
            });
            var cell = {
                index: tr.index(),
                content: "",
                son: sonList
            }

            var valueContent = $(tr.find('td')[index]).find('[data-pagemode]').not('.hide');

            if (valueContent.length > 0) {
                cell.content = valueContent.text().trim();
                if (cell.content.length == 0) {
                    cell.content = valueContent.find('input').val();
                }
            } else {
                cell.content = $(tr.find('td')[index]).text().trim();
            }

            list.push(cell);
        }
        rows.push(tr.get(0).outerHTML);
    });

    list.sort(function (a, b) {
        if (a.content == undefined) {
            a.content = '';
        }

        if (b.content == undefined) {
            b.content = '';
        }

        switch (type) {
            default:
            case "text":
                if (order) {
                    if (a.content.trim() > b.content.trim()) {
                        return 1;
                    }
                    if (a.content.trim() < b.content.trim()) {
                        return -1;
                    }
                } else {
                    if (a.content.trim() < b.content.trim()) {
                        return 1;
                    }
                    if (a.content.trim() > b.content.trim()) {
                        return -1;
                    }
                }
                break;
            case "numeric":
                if (order) {
                    if ((a.content.trim() * 1) > (b.content.trim() * 1)) {
                        return 1;
                    }
                    if ((a.content.trim() * 1) < (b.content.trim() * 1)) {
                        return -1;
                    }
                } else {
                    if ((a.content.trim() * 1) < (b.content.trim() * 1)) {
                        return 1;
                    }
                    if ((a.content.trim() * 1) > (b.content.trim() * 1)) {
                        return -1;
                    }
                }
                break;
            case "date":
                if (order) {
                    if (converToDate(a.content.trim()) === false) {
                        return -1;
                    }

                    if (converToDate(b.content.trim()) === false) {
                        return 1;
                    }

                    if (converToDate(a.content.trim()).getTime() > converToDate(b.content.trim()).getTime()) {
                        return 1;
                    }
                    if (converToDate(a.content.trim()).getTime() < converToDate(b.content.trim()).getTime()) {
                        return -1;
                    }
                } else {
                    if (converToDate(a.content.trim()) === false) {
                        return 1;
                    }

                    if (converToDate(b.content.trim()) === false) {
                        return -1;
                    }
                    if (converToDate(a.content.trim()).getTime() < converToDate(b.content.trim()).getTime()) {
                        return 1;
                    }
                    if (converToDate(a.content.trim()).getTime() > converToDate(b.content.trim()).getTime()) {
                        return -1;
                    }
                }
                break;
        }


        return 0;
    });

    $(element).find('tbody').first().html("");

    for (var i = 0; i < list.length; i++) {
        $(element).find('tbody').first().append(rows[list[i].index]);
        for (var j = 0; j < list[i].son.length; j++) {
            $(element).find('tbody').first().append(rows[list[i].son[j]]);
        }
    }

    if ($(element).find('tbody').first().find('.tableNormalBorder').length > 0) {
        $(element).find('tbody').first().find('.tableNormalBorder').removeClass('tableNormalBorder');
        $(element).find('tbody').first().find('tr[data-id]').not('.hide').last().addClass('tableNormalBorder');
    }
    return 0;
}