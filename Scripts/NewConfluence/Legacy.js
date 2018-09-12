$(document).ready(function () {
    $($('div[data-pagemode="read"]').first()).attrchange({
        trackValues: true,
        callback: function (event) {
            if (event.newValue !== "display:none") {
                PreCargar();
            }
        }
    });

    $($('div[data-pagemode="edit"]').first()).attrchange({
        trackValues: true,
        callback: function (event) {
            if (event.newValue !== "display:none") {
                PreCargar();
            }
        }
    });
});

function richEvent(element) {
    ShowMessageFunctions.RichFunction($(element));
}

function inputTextModalEvent(element, readonly) {
    ShowMessageFunctions.TextAreaFunction($(element), readonly);
}

function reloadParsley(content) {
    if (content === undefined) {
        content = $(document);
    }

    destroyParsley(content);
    initParsley(content);
    destroyParsley(content);
    initParsley(content);
}

function allowMultiPopUp() {
    $($('div.vex.vex-theme-default')).attrchange({
        trackValues: true,
        callback: function (event) {
            if (event.newValue === "display:none") {
                $(this).removeClass("hide");
                $(this).attr('style', '');
            }
        }
    });
}

$.fn.sortableConfluenseESWCIP = function (order) {
    if ($(this).is('table')) {
        sortableMiTableESWCIP($(this), order);
    }
    return $(this);
}

function sortableMiTableESWCIP(table, order) {
    table.find('thead th').each(function () {
        if (!$(this).is('.sort')) {
            $(this).removeClass('sorting_asc');
            $(this).removeClass('sorting');
            $(this).removeClass('sorting_desc');
            $(this).removeClass('sorting_disabled');
        } else {
            $(this).addClass('sortTing');
            $(this).removeClass('sortAsc');
            $(this).removeClass('sortDes');
        }
    });

    orderTableESW(table, table.find('thead th.sort').index(), order);

    table.find('.sort').off('click');
    table.find('.sort').click(function () {
        var table = $(this).closest('table');

        if ($(this).is('.sortTing')) {
            orderTableESW(table, $(this).index(), true);
        } else {
            if ($(this).is('.sortDes')) {
                orderTableESW(table, $(this).index(), true);
            } else {
                orderTableESW(table, $(this).index(), false);
            }
        }

        var pagination = $('[dd-table="' + table.attr('id') + '"].Pagination');
        if (pagination.length === 1) {
            table.find('tbody tr').addClass('hide');
            table.find('.tableNormalBorder').removeClass('tableNormalBorder');

            var rows = pagination.attr('dd-row') * 1;
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
        bindHandlers();
    });
}

function orderTableESW(element, index, order) {

    element.find('tbody tr span[data-pagemode=edit] input.inputDecimal').each(function () {
        if ($(this).val() !== "") {
            $(this).attr('value', Number($(this).val().replace(/[^0-9\.]+/g, "")));
        } else {
            $(this).attr('value', $(this).val())
        }
    })

    if (!element) {
        return 0;
    }

    if (!index) {
        index = 0;
    }

    if (order === undefined) {
        order = true;
    }

    $(element).find('thead th.sort').removeClass('sortAsc').removeClass('sortDes').addClass('sortTing');
    if (order) {
        $($(element).find('thead th')[index]).removeClass('sortTing').addClass('sortAsc');
    } else {
        $($(element).find('thead th')[index]).removeClass('sortTing').addClass('sortDes');
    }

    var list = new Array();
    var rows = new Array();
    $(element).find('tbody tr').each(function () {
        if ($(this).is('[data-id]')) {
            var sonList = new Array();
            $(this).closest('tbody').find('tr[data-row-parent-id="' + $(this).attr('data-id') + '"]').each(function () {
                sonList.push($(this).index());
            });
            var cell = {
                index: $(this).index(),
                content: $($(this).find('td')[index]).html(),
                son: sonList
            }
            list.push(cell);
        }
        rows.push($(this).get(0).outerHTML);
    });

    list.sort(function (a, b) {
        if (order) {
            if (a.content > b.content) {
                return 1;
            }
            if (a.content < b.content) {
                return -1;
            }
        } else {
            if (a.content < b.content) {
                return 1;
            }
            if (a.content > b.content) {
                return -1;
            }
        }

        return 0;
    });

    $(element).find('tbody').html("");

    for (var i = 0; i < list.length; i++) {
        $(element).find('tbody').append(rows[list[i].index]);
        for (var j = 0; j < list[i].son.length; j++) {
            $(element).find('tbody').append(rows[list[i].son[j]]);
        }
    }

    if ($(element).find('tbody').find('.tableNormalBorder').length > 0) {
        $(element).find('tbody').find('.tableNormalBorder').removeClass('tableNormalBorder');
        $(element).find('tbody').find('tr[data-id]').not('.hide').last().addClass('tableNormalBorder');
    }

    bindHandlers();
    requestProposalClick();
    approvalClick();
    dismissClick();
    studiesCommitteClick();
    evaluateRowCheckBoxesStatus();
    bindCheckboxes();
    enableSubmitButton();
    enableSubmitButtonWhenChange();

    return 0;
}


(function ($) {
    function isDomAttrModifiedSupported() {
        var p = document.createElement('p');
        var flag = false;

        if (p.addEventListener) {
            p.addEventListener('DOMAttrModified', function () {
                flag = true;
            }, false);
        } else if (p.attachEvent) {
            p.attachEvent('onDOMAttrModified', function () {
                flag = true;
            });
        } else { return false; }
        p.setAttribute('id', 'target');
        return flag;
    }

    function checkAttributes(chkAttr, e) {
        if (chkAttr) {
            var attributes = this.data('attr-old-value');

            if (e.attributeName.indexOf('style') >= 0) {
                if (!attributes['style'])
                    attributes['style'] = {};
                var keys = e.attributeName.split('.');
                e.attributeName = keys[0];
                e.oldValue = attributes['style'][keys[1]];
                e.newValue = keys[1] + ':'
						+ this.prop("style")[$.camelCase(keys[1])];
                attributes['style'][keys[1]] = e.newValue;
            } else {
                e.oldValue = attributes[e.attributeName];
                e.newValue = this.attr(e.attributeName);
                attributes[e.attributeName] = e.newValue;
            }

            this.data('attr-old-value', attributes);
        }
    }

    var mutationObserver = window.MutationObserver
			|| window.WebKitMutationObserver;

    $.fn.attrchange = function (a, b) {
        if (typeof a == 'object') {
            var cfg = {
                trackValues: false,
                callback: $.noop
            };
            
            if (cfg.trackValues) {
                this.each(function (i, el) {
                    var attributes = {};
                    for (var attr, i = 0, attrs = el.attributes, l = attrs.length; i < l; i++) {
                        attr = attrs.item(i);
                        attributes[attr.nodeName] = attr.value;
                    }
                    $(this).data('attr-old-value', attributes);
                });
            }

            if (mutationObserver) {
                var mOptions = {
                    subtree: false,
                    attributes: true,
                    attributeOldValue: cfg.trackValues
                };
                var observer = new mutationObserver(function (mutations) {
                    mutations.forEach(function (e) {
                        var element = e.target;

                        if (cfg.trackValues) {
                            e.newValue = $(element).attr(e.attributeName);
                        }
                        if ($(element).data('attrchange-status') === 'connected') {
                            cfg.callback.call(element, e);
                        }
                    });
                });

                return this.data('attrchange-method', 'Mutation Observer').data('attrchange-status', 'connected')
						.data('attrchange-obs', observer).each(function () {
						    observer.observe(this, mOptions);
						});
            } else if (isDomAttrModifiedSupported()) {

                return this.data('attrchange-method', 'DOMAttrModified').data('attrchange-status', 'connected').on('DOMAttrModified', function (event) {
                    if (event.originalEvent) { event = event.originalEvent; }
                    event.attributeName = event.attrName;
                    event.oldValue = event.prevValue;
                    if ($(this).data('attrchange-status') === 'connected') {
                        cfg.callback.call(this, event);
                    }
                });
            } else if ('onpropertychange' in document.body) {
                return this.data('attrchange-method', 'propertychange').data('attrchange-status', 'connected').on('propertychange', function (e) {
                    e.attributeName = window.event.propertyName;

                    checkAttributes.call($(this), cfg.trackValues, e);
                    if ($(this).data('attrchange-status') === 'connected') {
                        cfg.callback.call(this, e);
                    }
                });
            }
            return this;
        } else if (typeof a == 'string' && $.fn.attrchange.hasOwnProperty('extensions') &&
				$.fn.attrchange['extensions'].hasOwnProperty(a)) {
            return $.fn.attrchange['extensions'][a].call(this, b);
        }

        return $(this);
    }

})(jQuery);