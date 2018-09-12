$.fn.clickTreeButton = function() {
    return this.each(function() {
        var html = this.innerHTML;

        if(html.trim() == "-") {
            this.click();
        }
    })
}

function fixedTreeTableRelations() {
    var $firstTreeIcon = $('.tree span.icon:first');
    var $lastTreeIcon;
    var heightFix;

    if ($('table.tableNormal.relation-table').length > 0) {
        $lastTreeIcon = $('.tree span.icon:last');
        heightFix = 28;
    } else {
        $lastTreeIcon = $('td.tree.son > span.selected:last-child');
        heightFix = 38;
    }

    var firstTreeIconTop = $firstTreeIcon.offset().top;
    var lastTreeIconTop = $lastTreeIcon.offset().top;
    var theadHeight = $firstTreeIcon.closest('thead').height();

    var heightTreeIcon = lastTreeIconTop - firstTreeIconTop - theadHeight - heightFix;

    $('.tableNormal > tr:last-child td.tree span').first().height(heightTreeIcon);
    $('th.tree.icon:not(:first) > span:not(.icon)').removeClass('hide');
}

function collapseOperationRelatedTable(element) {
    var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(element).closest('table').attr('id') + '"]');
    var label = $(tempCollapseAll).find('label[dd-value]');
    var tempTexp = $(tempCollapseAll).text().trim();
    $(element).closest('table').find('tbody > tr > td > div > button.buttonShowRow.active:visible').click();

    if ($(element).html() === "-") {
        $(element).closest('table').find('tbody tr:not(.template)').not('.notCollapse').addClass("hide");
        $(element).closest('table').find('tbody tr.notCollapse td.tree span').addClass("hide");
        $(element).closest('th').find('span').not('.icon').addClass("hide");
        $(element).html('+');

        if ($(element).closest('table').find('thead tr').length > 0) {
            $(element).closest('table').find('thead tr').not(':first').not('.notCollapse').addClass("hide");
        }

        $('.notCollapse.customRowEven.selfOperation').find('td:first-child span').removeClass('span-selected span-selected-tree hide');
        $('table:not(.main-table)').find('span.collapse-button').clickTreeButton();

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
            $(element).closest('table').find('thead tr').not(':first').not('.notCollapse').removeClass("hide");
        }

        if ($(tempCollapseAll).length > 0) {
            if (!$(tempCollapseAll).is('.collapse')) {
                $(tempCollapseAll).addClass('collapse');
                $(label).text($(label).attr('dd-value'));
                $(label).attr('dd-value', tempTexp);
            }
        }

        if ($(element).closest('table').is(':not(.main-table)')) {
            $(element).closest('table').find('> tbody > tr:last-child > td:first-child').removeClass('hide');
            $(element).closest('table').find('tbody > tr span.selected:first').addClass('span-selected');
            $(element).closest('table').find('tbody > tr > td > span:first').addClass('span-selected-tree');
        } else {
            $('#relationsTable.tableNormal.child-table.relation-table > tbody > tr:last-child > td:first-child').addClass('hide');
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
    fixedTreeTableRelations();
}

function CollapseTables() {
    $('table.tableNormal th.tree.icon span.icon').not(':hidden').each(function () {
        if ($(this).html() === "-") {
            $(this).closest('table').find('tbody tr:not(.template)').removeClass("hide");
            $(this).closest('th').find('span').last().removeClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').not('.notCollapse').removeClass("hide");
            }
        } else {
            $(this).closest('table').find('tbody tr:not(.template)').not('.notCollapse').addClass("hide");
            $(this).closest('th').find('span').last().addClass("hide");
            if ($(this).closest('table').find('thead tr').length > 0) {
                $(this).closest('table').find('thead tr').not(':first').not('.notCollapse').addClass("hide");
            }
        }
    });
}