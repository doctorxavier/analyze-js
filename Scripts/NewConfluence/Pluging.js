(function ($) {
    $.fn.Clean = function () {
        if ($(this).is('.chosen')) {
            $(this).val('').trigger('chosen:updated');
        }

        if ($(this).is('.inputText')) {
            $(this).val('');
        }

        return $(this);
    }

    $.fn.Collapsable = function (execute) {
        execute = execute === true;

        if ($(this).is('.btnCollapseAll')) {
            $(this).addClass('collapse');
            $(this).find('label').attr('dd-value', $(this).attr('dd-exp')).text($(this).attr('dd-coll'));

            if (execute) {
                $(this).click();
            }

            return $(this);
        }

        if ($(this).is('.btnCollapseOne') && !$(this).is('.btnCollapseOneContainer')) {
            $(this).addClass('collapse');
            $(this).find('label').attr('dd-value', $(this).attr('dd-exp')).text($(this).attr('dd-coll'));

            if (execute) {
                $(this).click();
            }

            return $(this);
        }

        if ($(this).is('.btnCollapseOneContainer')) {
            $(this).addClass('collapse');
            $(this).find("span.icon").html("-");

            if (execute) {
                $(this).click();
            }

            return $(this);
        }
        return $(this);
    }

    $.fn.Expandable = function (execute) {
        execute = execute === true;

        if ($(this).is('.btnCollapseAll')) {
            $(this).removeClass('collapse');
            $(this).find('label').attr('dd-value', $(this).attr('dd-coll')).text($(this).attr('dd-exp'));

            if (execute) {
                $(this).click();
            }

            return $(this);
        }

        if ($(this).is('.btnCollapseOne') && !$(this).is('.btnCollapseOneContainer')) {
            $(this).removeClass('collapse');
            $(this).find('label').attr('dd-value', $(this).attr('dd-coll')).text($(this).attr('dd-exp'));

            if (execute) {
                $(this).click();
            }

            return $(this);
        }

        if ($(this).is('.btnCollapseOneContainer')) {
            $(this).removeClass('collapse');
            $(this).find("span.icon").html("+");

            if (execute) {
                $(this).click();
            }

            return $(this);
        }

        return $(this);
    }

    $.fn.sortableConfluense = function (order, index) {
        if ($(this).is('table')) {
            sortableMiTable($(this), order, index);
        }
        return $(this);
    }

    $.fn.sortableConfluence = function (order, index) {
        return $(this).sortableConfluense(order, index);
    }

    $.fn.objectSortableConfluence = function () {
        var result = new Object();
        var table = $(this);
        if (table.is('table')) {
            var header = table.find('.sort').not('.sortTing');

            var page = 0;
            if (table.hasClass('tablePagination')) {
                page = $('.Pagination[dd-table="' + table.attr('id') + '"]').find('.Pagination_Active').text().trim();
            }

            if (header.length === 1) {
                result = {
                    order: header.hasClass('sortAsc'),
                    column: header.index(),
                    pagination: page
                };
            }            
        }
        return result;
    }

    $.fn.selectableConfluence = function (multiselect) {
        if ($(this).is('table')) {
            $(this).addClass('tableSelectable');
            $(this).find('tbody tr[data-id] td').not('.not-selectabled').off('click');
            $(this).find('tbody tr[data-id] td').not('.not-selectabled').click(function () {
                var tr = $(this).closest('tr');

                if (multiselect === undefined || multiselect !== true) {
                    tr.closest('tbody').find('tr.row-selected').removeClass('row-selected');
                }

                if (tr.is('.row-selected')) {
                    tr.removeClass('row-selected');
                } else {
                    tr.addClass('row-selected');
                }

                if (typeof afterSelect !== 'undefined' && $.isFunction(afterSelect)) {
                    afterSelect($(this).closest('tr'), $(this));
                }
            });
        }
        return $(this);
    }

    $.fn.paginationConfluense = function (rows, prePagination, width) {
        if ($(this).is('table')) {
            $(this).addClass('tablePagination');
            $('div.Pagination[dd-table="' + $(this).attr("id") + '"]').remove();
            $(this).after('<div class="Pagination" dd-table="' + $(this).attr("id") + '" dd-row="' + rows + '"></div>');
            $(this).find('tbody tr[data-id]').removeClass('hide');
            var totalRows = Math.ceil($(this).find('tbody tr[data-id]').length / rows);

            if (totalRows > 0) {
                var paginator = $(document).find('div.Pagination[dd-table="' + $(this).attr("id") + '"]');
                if (width !== undefined) {
                    $(paginator).css("min-width", width);
                    $(paginator).css("width", width);
                    $(paginator).css("max-width", width);
                } else {
                    $(paginator).css("min-width", $(this).width());
                    $(paginator).css("width", $(this).width());
                    $(paginator).css("max-width", $(this).width());
                }

                paginator.append('<div dd-num="1" onclick="PaginationMove(this)" class="Pagination_Text Pagination_Prev">' + Translate.Get('GLOBAL.PAGINATION.PREV') + '</div>');

                if (totalRows > 5) {
                    paginator.append('<div dd-num="1" class="Pagination_Text Pagination_Points hide">...</div>');
                }
                for (var i = 1; i < totalRows + 1 ; i++) {
                    if (i === 1) {
                        paginator.append('<div dd-num="' + i + '" class="Pagination_Number Pagination_Active">' + i + '</div>');
                    }
                    else {
                        if (i > 5) {
                            paginator.append('<div dd-num="' + i + '"  class="Pagination_Number hide">' + i + '</div>');
                        } else {
                            paginator.append('<div dd-num="' + i + '"  class="Pagination_Number">' + i + '</div>');
                        }

                    }
                }

                if (totalRows > 5) {
                    paginator.append('<div dd-num="' + totalRows + '" class="Pagination_Text Pagination_Points">...</div>');
                }

                paginator.append('<div onclick="PaginationMove(this)" dd-num="' + totalRows + '" class="Pagination_Text Pagination_Next">' + Translate.Get('GLOBAL.PAGINATION.NEXT') + '</div>');

                $(this).find("tbody tr[data-id].tableNormalBorder").removeClass('tableNormalBorder');
                $(this).find("tbody tr[data-id]:gt(" + (rows - 1) + ")").addClass("hide");
                $(this).find("tbody tr[data-row-parent-id]:gt(" + (rows - 1) + ")").addClass("hide");
                $(this).find("tbody tr.showChildRow.hide").removeClass("showChildRow");
                $(this).find("tbody tr[data-id]").not('.hide').last().addClass('tableNormalBorder');

                if (prePagination !== undefined) {
                    $(paginator).find('[dd-num="' + prePagination + '"]').click();
                }
            }

            $(".Pagination_Number").click(function () {
                if (!$(this).is(".Pagination_Active")) {
                    $(this).closest(".Pagination").find("div").removeClass("Pagination_Active");
                    $(this).addClass("Pagination_Active");
                    var table = "#" + $(this).closest(".Pagination").attr("dd-table");
                    var row = $(this).closest(".Pagination").attr("dd-row");
                    var cantidad = ($(this).html() * 1) - 1;
                    $(table).find("tbody tr[data-id]").addClass("hide");
                    $(table).find("tbody tr[data-row-parent-id]").addClass("hide").removeClass('showChildRow ');

                    $(table).find("tbody tr[data-id]:gt(" + (row * cantidad) + "):lt(" + (row - 1) + ")").removeClass("hide")
                        .each(function () {
                            if ($(this).find('.buttonShowRow.active').length > 0) {
                                $(this).closest('tbody').find('[data-row-parent-id=' + $(this).attr('data-id') + ']')
                                .removeClass('hide').addClass('showChildRow ');
                            }
                        });

                    $(table).find("tbody tr[data-id]:eq(" + (row * cantidad) + ")").removeClass("hide")
                    .each(function () {
                        if ($(this).find('.buttonShowRow.active').length > 0) {
                            $(this).closest('tbody').find('[data-row-parent-id=' + $(this).attr('data-id') + ']')
                            .removeClass('hide').addClass('showChildRow ');
                        }
                    });

                    var totalIndexes = $(this).closest(".Pagination").find('.Pagination_Number').length;

                    if (totalIndexes > 5) {
                        $(this).closest(".Pagination").find('.Pagination_Number[dd-num]').addClass('hide');
                        $(this).closest(".Pagination").find('.Pagination_Points').removeClass('hide');
                        var indexInterno = $(this).attr('dd-num') * 1;
                        if ($(this).text() * 1 < 3) {
                            $(this).closest(".Pagination").find('.Pagination_Points').first().addClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="1"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="2"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="3"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="4"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="5"]').removeClass('hide');
                        } else if ($(this).text() * 1 > ($(this).closest(".Pagination").find('.Pagination_Number').length - 4)) {
                            $(this).closest(".Pagination").find('.Pagination_Points').last().addClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 1) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 2) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 3) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 4) + '"]').removeClass('hide');
                        } else {
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno - 1) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + indexInterno + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 1) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 2) + '"]').removeClass('hide');
                            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 3) + '"]').removeClass('hide');
                        }
                    }

                    paginationEndTable(table);
                }
            });

            $(".Pagination_Text").click(function () {
                if ($(this).is('.Pagination_Points')) {
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + $(this).attr('dd-num') + '"]').click();
                    var table = "#" + $(this).closest(".Pagination").attr("dd-table");

                    paginationEndTable(table);
                }
            });

            $(".Pagination_Text").hover(function () {
                var table = "#" + $(this).closest(".Pagination").attr("dd-table");
                if ($(this).attr('dd-num') === $(this).closest('.Pagination').find('.Pagination_Active').attr('dd-num')) {
                    $(this).addClass('NoActive');
                } else {
                    $(this).removeClass('NoActive');
                }

                paginationEndTable(table);
            });


            if (totalRows > 0) {
                if (prePagination !== undefined) {
                    $(paginator).find('[dd-num="' + prePagination + '"]').click();
                }
            }
        }

        return $(this);
    };

    $.fn.FixLastRowLine = function () {
        var $element = $(this);

        if ($element.is('table')) {
            $element.find('.tree.last').each(function (index) {
                $this = $(this).removeClass('last');

                var rowId = $this.closest('tr').attr('data-id');

                if (rowId != undefined && rowId.length > 0) {
                    $this.closest('table').find('[data-row-parent-id="' + rowId + '"]').each(function () {
                        var tree = $(this).find('td:first > span').closest('td');
                        if (tree.index() === 0) {
                            tree.addClass('tree');
                        }
                    });
                }
            });

            $element.find('tbody').each(function (index) {
                $this = $(this);

                var $tree = $this.find('tr[data-id] td.tree').not($this.find('tbody tr[data-id] td.tree')).last();
                $tree.addClass('last');

                var rowId = $tree.closest('tr').attr('data-id');
                if (rowId != undefined && rowId.length > 0) {
                    $tree.closest('table').find('[data-row-parent-id="' + rowId + '"]').each(function () {
                        $(this).find('td.tree').first().removeClass('tree');
                    });
                }
            });
        }

        return $element;
    }

    $.fn.paginationConfluence = function (rows, width) {
        return $(this).paginationConfluense(rows, width);
    }

    $.fn.FirstorDefault = function () {
        if ($(this).closest('.dropdown').length > 0) {
            var contenedor = $(this).closest('.dropdown');
            var elemento = contenedor.find('ul.dropdown-menu li').first().find('a');
            contenedor.find('input').val(elemento.attr('dd-value'));
            contenedor.find('input').attr('value', elemento.attr('dd-value'));
            contenedor.find('input').trigger('change');
            contenedor.find('button').find('span.valueText').text(elemento.text());
            contenedor.find('ul.dropdown-menu li').find('[dd-selected]').removeAttr('dd-selected');
            elemento.attr('dd-selected', '');

            if (contenedor.find('input').val().length === 0) {
                contenedor.addClass('placeholder');
            }
        }
        return $(this);
    }

    $.fn.SelectIndex = function (index) {
        if ($(this).closest('.dropdown').length > 0) {
            var contenedor = $(this).closest('.dropdown');
            var elementos = contenedor.find('ul.dropdown-menu li');

            if (elementos.length <= index) {
                $(this).FirstorDefault();
                return $(this);
            }

            var elemento = $(elementos[index]).find('a');
            contenedor.find('input').attr('value', elemento.attr('dd-value'));
            contenedor.find('input').val(elemento.attr('dd-value'));
            contenedor.find('input').trigger('change');
            contenedor.find('button').find('span.valueText').text(elemento.text());
            contenedor.find('ul.dropdown-menu li').find('[dd-selected]').removeAttr('dd-selected');
            elemento.attr('dd-selected', '');

            if (contenedor.find('input').val().length === 0) {
                contenedor.addClass('placeholder');
            } else {
                contenedor.removeClass('placeholder');
            }
        }
        return $(this);
    }

    $.fn.GetText = function () {
        if ($(this).closest('.dropdown').length > 0) {
            return $(this).closest('.dropdown').find('.valueText').text();
        }

        if ($(this).closest('.ctlAsyncr').length > 0) {
            return $(this).closest('.ctlAsyncr').find('input[type="text"]').first().val();
        }

        if ($(this).closest('.ctlComplete').length > 0) {
            return $(this).closest('.ctlComplete').find('input[type="text"]').first().val();
        }

        return null;
    }

    $.fn.GetValue = function () {
        if ($(this).closest('.dropdown').length > 0) {
            return $(this).closest('.dropdown').find('input.hide').val();
        }

        if ($(this).closest('.ctlAsyncr').length > 0) {
            return $(this).closest('.ctlAsyncr').find('input.hidden').last().val();
        }

        if ($(this).closest('.ctlComplete').length > 0) {
            return $(this).closest('.ctlComplete').find('input.hidden').val();
        }

        return null;
    }

    $.fn.SetValue = function (value, text) {
        if ($(this).closest('.dropdown').length > 0) {
            $(this).closest('.dropdown').find('input.hide').attr('value', value);
            $(this).closest('.dropdown').find('input.hide').val(value);
            $(this).closest('.dropdown').find('input.hide').trigger('change');
            $(this).closest('.dropdown').find('.valueText').text(text);
        }

        if ($(this).closest('.ctlAsyncr').length > 0) {
            $(this).closest('.ctlAsyncr').find('input.hidden').last().attr('value', value);
            $(this).closest('.ctlAsyncr').find('input.hidden').last().val(value);
            $(this).closest('.ctlAsyncr').find('input.hidden').last().trigger('change');
            $(this).closest('.ctlAsyncr').find('input[type="text"]').first().val(text);
        }

        if ($(this).closest('.ctlComplete').length > 0) {
            $(this).closest('.ctlComplete').find('input.hidden').attr('value', value);
            $(this).closest('.ctlComplete').find('input.hidden').val(value);
            $(this).closest('.ctlComplete').find('input.hidden').trigger('change');
            $(this).closest('.ctlComplete').find('input[type="text"]').first().val(text);
        }

        return $(this);
    }

    $.fn.ChosenMultiDropdown = function () {
        $(this).each(function () {
            $(this).chosen();

            var width = $(this).attr('data-width');
            var height = $(this).attr('data-maxHeight');

            var contenedor = $(this).closest('div.chosenMultiSelect');

            if (width && width.length > 0) {
                $(contenedor).find('.chosen-container').css('width', width);
            }
            if (height && height.length > 0) {
                $(contenedor).find('.chosen-choices').css('max-height', height);
            }
        });
        return $(this);
    }

    $.fn.TableNumber = function () {
        $(this).each(function () {
            if ($(this).find('td.tree div.number').length > 0) {
                $(this).find('.treeNumber').remove();
                $(this).find('.tree').each(function () {
                    if (!$(this).is('.noCreateNumber')) {
                        $(this).before('<td class="treeNumber">' + $(this).find('.number').text() + '</td>');
                    }
                });
            }
        });
        return $(this);
    }
    
}(jQuery));

String.prototype.replaceAll = function (find, replace) {
    var str = this;
    return str.replace(new RegExp(find.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), replace);
};

if (typeof $.fn.modal != "undefined") {
    $.fn.modal.prototype.constructor.Constructor.DEFAULTS.backdrop = 'static';
    $.fn.modal.prototype.constructor.Constructor.DEFAULTS.keyboard = false;
}

(function ($) {
    if ($.fn.style) {
        return;
    }

    var escape = function (text) {
        return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
    };
    
    var isStyleFuncSupported = !!CSSStyleDeclaration.prototype.getPropertyValue;
    if (!isStyleFuncSupported) {
        CSSStyleDeclaration.prototype.getPropertyValue = function (a) {
            return this.getAttribute(a);
        };
        CSSStyleDeclaration.prototype.setProperty = function (styleName, value, priority) {
            this.setAttribute(styleName, value);
            var priority = typeof priority != 'undefined' ? priority : '';
            if (priority != '') {
                var rule = new RegExp(escape(styleName) + '\\s*:\\s*' + escape(value) +
                    '(\\s*;)?', 'gmi');
                this.cssText =
                    this.cssText.replace(rule, styleName + ': ' + value + ' !' + priority + ';');
            }
        };
        CSSStyleDeclaration.prototype.removeProperty = function (a) {
            return this.removeAttribute(a);
        };
        CSSStyleDeclaration.prototype.getPropertyPriority = function (styleName) {
            var rule = new RegExp(escape(styleName) + '\\s*:\\s*[^\\s]*\\s*!important(\\s*;)?',
                'gmi');
            return rule.test(this.cssText) ? 'important' : '';
        }
    }

    $.fn.style = function (styleName, value, priority) {
        var node = this.get(0);
        if (typeof node == 'undefined') {
            return this;
        }
        var style = this.get(0).style;
        if (typeof styleName != 'undefined') {
            if (typeof value != 'undefined') {
                priority = typeof priority != 'undefined' ? priority : '';
                style.setProperty(styleName, value, priority);
                return this;
            } else {
                return style.getPropertyValue(styleName);
            }
        } else {
            return style;
        }
    };
})(jQuery);