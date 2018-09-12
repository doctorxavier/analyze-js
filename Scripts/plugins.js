//Switcherify plugin
(function ($) {
    $.fn.switcherify = function () {
        return this.each(function () {
            var target = $(this);
            var parent = target.parent();
            var container = $('<div/>');
            var switchBackground = $('<label/>');
            var switchForeground = $('<span/>');
            var switchElement = $('<span/>');

            switchForeground.addClass('switchForeground');
            switchForeground.append(switchElement);
            switchBackground.addClass('switchBackground');
            switchBackground.append(switchForeground);
            container.addClass('switchContainer');
            container.append(switchBackground);
            container.append(target);

            if (target.hasClass('activated')) {
                container.addClass('activated');
            }

            parent.append(container);

            switchBackground.click(function () {
                switchFunction(target, container);
            });
        });
    };
}(jQuery));


function switchFunction(target, container) {
    if (target.attr('data-action') != null) {
        triggerAction(target, target.attr('data-action'));
    }

    var alternateText = target.attr('data-switch-text');
    target.attr('data-switch-text', target.text());
    target.text(alternateText);
    target.toggleClass('activated');
    container.toggleClass('activated');
}

//Collapsible region plugin
(function ($) {
    $.fn.collapsible = function () {
        return this.each(function () {
            var target = $(this);
            var targetContainer = target.next(target.attr('data-collapse'));
            var buttonContainer = $('<span/>');
            var buttonElement = $('<button/>');

            var separationDiv = $('<div/>');

            buttonContainer.append(buttonElement);
            buttonContainer.addClass('collapsibleContainer');
            buttonContainer.insertBefore(target);

            separationDiv.insertAfter(targetContainer);

            if (target.hasClass('expanded')) {
                buttonElement.addClass('expanded');
                target.removeClass('expanded');
                separationDiv.addClass('hide');
            }
            else {
                separationDiv.removeClass('hide');
            }
            buttonElement.click(function () {
                switchControl(separationDiv, targetContainer, buttonElement);
            });

        });
    };
}(jQuery));

function switchControl(separationDiv, targetContainer, buttonElement) {
    separationDiv.toggleClass('hide');
    targetContainer.slideToggle();
    buttonElement.toggleClass('expanded');

    var total = targetContainer.parent().find('.collapsible').size();
    var expanded = targetContainer.parent().find('.expanded').size();
    var notExpanded = total - expanded;
    var target = targetContainer.parent().parent().find('button.switch');
    var container = target.parent();

    if (total == expanded) {
        switchFunction(target, container);
    }
    else {
        if (total == notExpanded) {
            switchFunction(target, container);
        }
    }
}

//Togglerify plugin
(function ($) {
    $.fn.togglerify = function () {
        return this.each(function () {
            var target = $(this);
            var containerTarget = target.parents('tr').next('tr.template');

            target.click(function () {
                target.toggleClass('expanded');
                containerTarget.toggleClass('hide');
            });
        });
    };
}(jQuery));

//Tooltip plugin
(function ($) {
    $.fn.dataTooltip = function () {
        return this.each(function () {
            var target = $(this);
            var tooltipContents = target.attr('data-tooltip').split('|');
            var tooltipTitles = target.attr('data-tooltip-title');
            if (tooltipTitles != null) {
                tooltipTitles = tooltipTitles.split('|');
            }
            else {
                tooltipTitles = '';
            }

            var showTitle = target.attr('data-tooltip-showTitle') != 'false';
            var onlyReadTooltip = target.attr('data-tooltip-hideonedit');
            var tooltipContainer = $('#globalTooltipContainer');
            var classDependent = target.attr('data-tooltip-showOnClassPresent');

            if (tooltipContainer.length == 0) {
                tooltipContainer = $('<div/>');
                tooltipContainer.addClass('tooltipContainer').hide();
                tooltipContainer.attr('id', 'globalTooltipContainer');

                $('body').append(tooltipContainer);
            }

            if (onlyReadTooltip == 'true') {
                target = target.find('[data-pagemode="read"]');
            }

            target.hover(function () {
                tooltipContainer.stop().empty();

                if (classDependent != null && !target.hasClass(classDependent)) {
                    return;
                }

                for (var i = 0; i < tooltipContents.length; i++) {
                    var tooltip = tooltipContents[i];
                    tooltip = tooltip.replace(/(\r\n|\n\r|\r|\n)/g, '<br>');

                    var title = tooltipTitles[i];

                    var tooltipTitle = $('<div/>');
                    tooltipTitle.addClass('tooltipTitle');
                    if (title == '') {
                        tooltipTitle.addClass('empty');
                    }
                    tooltipTitle.html(title);
                    tooltipContainer.append(tooltipTitle);

                    var tooltipBody = $('<div/>');
                    tooltipBody.addClass('tooltipBody');
                    tooltipBody.html(tooltip);
                    tooltipContainer.append(tooltipBody);
                }

                tooltipContainer.css('left', target.offset().left - 10);
                tooltipContainer.css('top', target.offset().top + (target.height() + 10));

                tooltipContainer.show();
            }, function () {
                tooltipContainer.hide();
            });
        });
    };
}(jQuery));

//Checkbox plugin
(function ($) {
    $.fn.customCheckbox = function () {
        return this.each(function () {
            var target = $(this);
            var container = $('<label/>');
            var labelElement = $('<span/>');
            var insertion = target.parent();

            labelElement.text(target.attr('data-text'));
            container.addClass('checkbox');
            container.append(target);
            container.append(labelElement);
            insertion.prepend(container);


            if (this.checked) {
                labelElement.addClass('checked');
            }

            labelElement.click(function () {
                var labelParent = $(this);
                var input = this.previousSibling;
                var dataDisabled = $(input).attr("data-disabled");
                if (!input.readOnly && !input.disabled && ((dataDisabled == null) || (dataDisabled === ""))) {
                    labelParent.toggleClass('checked');
                    if (labelParent.hasClass('checked')) {
                        $(input).attr("checked", "checked");
                    }
                    else {
                        $(input).removeAttr("checked");
                    }
                }
            });
        });
    };
}(jQuery));

vex.defaultOptions.className = 'vex-theme-default';

jQuery(function ($) {
    $.datepicker.setDefaults({ dateFormat: 'dd/mm/yy' });
});

String.prototype.padLeft = function (len, chr) {
    var self = Math.abs(this) + '';
    return (this < 0 && '-' || '') + (String(Math.pow(10, (len || 2) - self.length)).slice(1).replace(/0/g, chr || '0') + self);
};

function addNewRepeaterItem(target, position, newId, repeaterItemClass) {

    var btnContainer = target.parents('[data-repeater-buttons]');
    var repeater = btnContainer.parent();
    var template = repeater.find('[data-repeater-template="true"]:eq(0)').children('div');

    var createdElement = template.clone(false)
        .removeAttr('id')
        .removeClass('hide')
        .addClass('repeater-item')
        .addClass(repeaterItemClass)
        .attr('data-repeater-item', "true")
        .attr('data-repeater-item-new-id', newId);


    var extraData = repeater.data('RepeaterData').ExtraData;

    // Set config from inputs from repeater
    //var inputs = createdElement.find('input, select, textarea, button:not(.btnDelete-repeater)');
    var inputs = createdElement.find('input, select, textarea');
    var directInputs = inputs.filter(function () {
        var input = $(this);
        var isInTemplate = input.parents('div[data-repeater-template]').length != 0;
        return !isInTemplate;
    });

    directInputs.removeAttr('disabled');
    directInputs.attr('data-persist-new-id', newId);

    extraData.forEach(function (item) {
        $.each(item, function (name, value) {
            directInputs.attr(name, value);
        });
    });



    // Set config for internal repeaters
    var internalRepeaters = createdElement.find('[data-repeater-main="true"]');
    var internalRepeatersFiltered = internalRepeaters.filter(function () {
        var internalRepeater = $(this);
        var isInTemplate = internalRepeater.parents('[data-repeater-template]').length != 0;
        return !isInTemplate;
    });
    internalRepeatersFiltered.attr('data-repeater-autoincrement-id', 0);
    internalRepeatersFiltered.attr('data-repeater-parent-id', newId);
    internalRepeatersFiltered.attr('data-repeater-parent-id-new', "true");
    initializeRepeaters(internalRepeatersFiltered);

    // Insert item
    if (position === 'top') {
        createdElement.appendTo(repeater);
    }
    else {
        createdElement.insertBefore(btnContainer);
    }

    bindHandlers(createdElement);

    var form = template.parents('[data-parsley-validate]:first');
    destroyParsley(form);
    initParsley(form);
    destroyParsley(form);
    initParsley(form);

    return createdElement;
}

function updateRepeaterFields(container) {

    if (container == null) {
        container = $('body');
    }

    var allRepeater = container.find('[data-repeater-main="true"]');
    var repeaterFiltered = allRepeater.filter(function () {
        var repeater = $(this);
        var isNew = repeater.attr('data-repeater-parent-id-new') === "true";
        return !isNew;
    });

    initializeRepeaters(repeaterFiltered);
}

function initializeRepeaters(repeaters) {
    var fnDelete = function () {
        var $this = $(this);
        var additionalDeleteAction = $this.attr("data-repeater-button-remove-action");
        var canRemove = true;

        if ((additionalDeleteAction != null) && (additionalDeleteAction !== "") && window[additionalDeleteAction] != null) {
            canRemove = window[additionalDeleteAction]($this);
        }

        if (canRemove == true) {
            $this.parents('.repeater-item:eq(0)').remove();
        } else if (typeof (canRemove.done) == "function") {
            canRemove.done(function (result) {
                if (result == true) {
                    $this.parents('.repeater-item:eq(0)').remove();
                }
            });
        }
    };

    var generateRepeaterData = function (repeater) {

        var item = repeater[0];

        var repeaterData = {
            RepeaterItemClass: repeater.attr('data-repeater-item-class'),
            CurrentId: 0,
            ExtraData: []
        };
        if (repeater.attr('data-repeater-autoincrement-id') != null) {
            repeaterData.CurrentId = parseInt(repeater.attr('data-repeater-autoincrement-id'));
        }

        var attributes = item.attributes;
        for (var i = 0; i < attributes.length; i++) {
            var attribute = attributes[i];
            var name = attribute.name;
            var value = attribute.value;
            if (name.startsWith('data-persist-')) {
                var obj = {};
                obj[name] = value;
                repeaterData.ExtraData.push(obj);
            }
        }

        var parentId = repeater.attr('data-repeater-parent-id');
        var parentIdIsNew = repeater.attr('data-repeater-parent-id-new') === "true";
        if ((parentId != null) && parentIdIsNew) {
            repeaterData.ExtraData.push({ "data-persist-new-parent-id": parentId });
        } else if (parentId != null) {
            repeaterData.ExtraData.push({ "data-persist-old-parent-id": parentId });
        }

        return repeaterData;
    };

    var generateAttributesFromRepeaterData = function (repeater) {

        var repeaterData = repeater.data('RepeaterData');

        repeater.attr('data-repeater-autoincrement-id', repeaterData.CurrentId);
        repeater.attr('data-repeater-item-class', repeaterData.RepeaterItemClass);

        // Clean old attributes
        var attributes = repeater[0].attributes;
        for (var i = 0; i < attributes.length; i++) {
            var attribute = attributes[i];
            var name = attribute.name;
            var value = attribute.value;
            if (name.startsWith('data-persist-')) {
                repeater.removeAttr(name);
            }
        }

        repeaterData.ExtraData.forEach(function (item) {
            $.each(item, function (name, value) {
                if (name === "data-persist-old-parent-id") {
                    repeater.attr('data-repeater-parent-id', value);
                    repeater.attr('data-repeater-parent-id-new', "false");
                } else if (name === "data-persist-new-parent-id") {
                    repeater.attr('data-repeater-parent-id', value);
                    repeater.attr('data-repeater-parent-id-new', "true");
                } else {
                    repeater.attr(name, value);
                }
            });
        });
    };

    repeaters.each(function (ix, item) {
        //Set data for repeaters that arent templates
        var repeater = $(item);

        var repeaterData = generateRepeaterData(repeater);

        repeater.data('RepeaterData', repeaterData);

        // Find repeater all inputs in template and set disabled
        repeater.find('div[data-repeater-template="true"]').find('input, select, textarea').attr('disabled', 'disabled');

        // Find inputs in this repeater (direct inputs) and set properties
        repeater.find('div[data-repeater-container="true"]').find('input, select, textarea').each(function () {
            var input = $(this);
            var inputRepeater = input.parents('[data-repeater-main]:first');
            if (inputRepeater.attr('id') !== repeater.attr('id')) {
                return;
            }
            var data = repeater.data('RepeaterData').ExtraData;

            for (var i = 0; i < data.length; i++) {
                input.attr(data[i]);
            }

            var inputContainer = input.parents('div[data-repeater-item]:first');
            var oldId = inputContainer.attr('data-repeater-item-old-id');
            var newid = inputContainer.attr('data-repeater-item-new-id');
            if (oldId != null) {
                input.attr("data-persist-old-id", oldId);
            } else if (newid != null) {
                input.attr("data-persist-new-id", newid);
            }
        });

        // Set funtions to "create" button
        repeater.children('div[data-repeater-buttons="true"]').find('button').click(function () {
            var button = $(this);
            button.blur();

            var repeaterData = repeater.data('RepeaterData');

            var nextId = repeaterData.CurrentId + 1;
            var repeaterItemClass = repeaterData.RepeaterItemClass;
            var repeaterPosition = button.attr('data-repeater-position');
            var createdContent = addNewRepeaterItem(button, repeaterPosition, nextId, repeaterItemClass);

            var aditionalAction = button.attr("data-repeater-button-new-action");

            if ((aditionalAction != null) && (aditionalAction !== "") && window[aditionalAction] != null) {
                window[aditionalAction](button, createdContent);
            }

            repeaterData.CurrentId = nextId;

            repeater.data('RepeaterData', repeaterData);
            generateAttributesFromRepeaterData(repeater);


            createdContent.find('button.btnDelete-repeater').click(fnDelete);
        });

        var deleteButtons = repeater.children('[data-repeater-container]').children().children('.btnDeleteRepeater').children();
        deleteButtons.click(fnDelete);
    });
}

String.format = function (format) {
    var args = Array.prototype.slice.call(arguments, 1);
    return format.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined'
            ? args[number]
            : match
        ;
    });
};

Array.prototype.contains = function (element) {
    return this.indexOf(element) > -1;
};

$.extend($.expr[":"], {
    "containsCaseInsensitive": function (elem, i, match, array) {
        return (elem.textContent || elem.innerText || "").toLowerCase().indexOf((match[3] || "").toLowerCase()) >= 0;
    }
});

//Array.prototype.distinct = function () {
//    var temp = new Array();
//    this.sort();
//    for (i = 0; i < this.length; i++) {
//        if (this[i] == this[i + 1]) { continue }
//        temp[temp.length] = this[i];
//    }
//    return temp;
//}

if (!Array.prototype.filter) {

    Array.prototype.filter = function (fun, scope) {
        var T = this, A = [], i = 0, itm, L = T.length;
        if (typeof fun == 'function') {
            while (i < L) {
                if (i in T) {
                    itm = T[i];
                    if (fun.call(scope, itm, i, T)) A[A.length] = itm;
                }
                ++i;
            }
        }
        return A;
    }
}

//btn-expandable
(function ($) {
    $.fn.btnExpandable = function () {
        return this.each(function () {
            var source = $(this);
            var checkbox = source.children('input');

            var fnSwitch = function () {
                checkbox.prop('checked', !checkbox.is(':checked'));
            };

            var fnExecute = function () {


                var targetRegionsSelector = source.attr("data-btn-expandable");
                var targetRegions = $(targetRegionsSelector);
                var speed = parseInt(source.attr("data-btn-expandable-speed"));
                var isExpanded = !checkbox.is(':checked');
                var markTarget = source.attr("data-btn-expandable-mark-target") == "true";
                var isMasterRow = source.attr("data-btn-expandable-is-master-row") == "true";

                if (isNaN(speed)) {
                    speed = 200;
                }

                if (isExpanded) {
                    targetRegions.slideDown(speed);
                } else {
                    targetRegions.slideUp(speed);
                }

                if (isMasterRow) {
                    source.parents('tr:first').attr('data-showing-detail', isExpanded);
                }

                targetRegions.removeAttr('data-last-detail');
                if (markTarget) {
                    targetRegions.attr('data-is-expanded', isExpanded);
                    targetRegions.last().attr('data-last-detail', 'true');
                }
            };

            var fnLinkCollapseAll = function () {
                var collapseAllButtonsSelector = source.attr("data-btn-expandable-link");
                var collapseAllButtons = $(collapseAllButtonsSelector);

                collapseAllButtons.each(function (ix, item) {
                    var collapsibleAll = $(item);
                    var targetExpandableButtonsSelector = collapsibleAll.attr("data-btn-expandable-all");
                    var targetExpandableButtons = $(targetExpandableButtonsSelector);

                    //var buttonsExpanded = targetExpandableButtons.filter("[data-btn-expandable-is-expanded=true]");
                    //var buttonsCollapsed = targetExpandableButtons.filter("[data-btn-expandable-is-expanded=false]");
                    var buttonsExpanded = targetExpandableButtons.filter(function (index, item) {
                        var target = $(item);
                        var targetCheckbox = target.children('input');

                        return targetCheckbox.is(':checked') == false;
                    });
                    var buttonsCollapsed = targetExpandableButtons.filter(function (index, item) {
                        var target = $(item);
                        var targetCheckbox = target.children('input');

                        return targetCheckbox.is(':checked') == true;
                    });
                    var totalButtonsExpanded = buttonsExpanded.length;
                    var totalButtonsCollapsed = buttonsCollapsed.length;


                    var btnExpandableAll = collapsibleAll.data("btn-expandable-all");

                    if (totalButtonsExpanded == 0) {
                        //Switch to Expand All
                        btnExpandableAll.Switch("expand");
                    }

                    if (totalButtonsCollapsed == 0) {
                        //Switch to Collapse All
                        btnExpandableAll.Switch("collapse");
                    }
                });
            }

            source.data("btn-expandable",
{
    Switch: fnSwitch,
    Execute: fnExecute,
    SwitchAndExecute: function () {
        fnSwitch();
        fnExecute();
    },
    CheckLinkedAndChange: fnLinkCollapseAll
});
        });
    };
}(jQuery));

//btn-expandable-all
(function ($) {
    $.fn.btnExpandableAll = function () {
        return this.each(function () {
            var source = $(this);
            var checkbox = source.children('input');

            //values -> null, expand or collapse
            var fnSwitch = function (specificPosition) {
                if (specificPosition == "expand") {
                    checkbox.prop('checked', false);
                } else if (specificPosition == "collapse") {
                    checkbox.prop('checked', true);
                } else {
                    checkbox.prop('checked', !checkbox.is(':checked'));
                }
            }

            var fnExecute = function () {

                var targetExpandableButtonsSelector = source.attr("data-btn-expandable-all");
                var targetExpandableButtons = $(targetExpandableButtonsSelector);
                var isExpandedAll = checkbox.is(':checked');

                var elementsToChange = null;
                elementsToChange = targetExpandableButtons.filter(function (index, item) {
                    var target = $(item);
                    var targetCheckbox = target.children('input');

                    return targetCheckbox.is(':checked') == isExpandedAll;
                });

                elementsToChange.each(function (ix, item) {
                    var btnExpandable = $(item).data("btn-expandable");
                    btnExpandable.SwitchAndExecute();
                });
            }

            source.data("btn-expandable-all",
    {
        Switch: fnSwitch,
        Execute: fnExecute,
        SwitchAndExecute: function () {
            fnSwitch();
            fnExecute();
        }
    });
        });
    };
}(jQuery));

//Tabs
(function ($) {
    $.fn.idbTabs = function () {
        return this.each(function () {

            var source = $(this);

            var fnSwitch = function (tabId) {
                var containerSelector = source.attr('data-tab');
                var container = $(containerSelector);

                var tab = source.children(String.format('[data-tab-id={0}]', tabId));
                var tabs = source.children('[data-tab-id]');
                tabs.attr('data-tab-active', 'false');
                tabs.removeClass('active');
                tab.attr('data-tab-active', 'true');
                tab.addClass('active');
                var url = tab.attr('data-tab-url');
                if (url != null) {
                    container.load(url);
                }
            };

            source.data("idbTabs",
            {
                Switch: fnSwitch
            });
        });
    };
}(jQuery));
//btn-expandable
(function ($) {
    $.fn.CascadeDropdown = function () {
        return this.each(function () {
            var source = $(this);

            var setOptions = function (tagert, options) {
                tagert.html("");
                var emptyOption = tagert.attr("data-show-empty-option");
                if (emptyOption != null) {
                    addOption(tagert, "", emptyOption);
                }

                options.forEach(function (item, index, list) {
                    addOption(tagert, item.Value, item.Text);
                });
            }

            var addOption = function (target, value, text) {
                var newOption = $("<option>");
                newOption.val(value);
                newOption.html(text);
                newOption.appendTo(target);
            }

            var loadFunction = function () {
                var response = null;

                var parentSelector = source.attr("data-dropdown-cascade");
                var parent = $(parentSelector);

                var functionName = source.attr("data-dropdown-cascade-function");
                var url = source.attr("data-dropdown-cascade-url");

                if ((functionName != null) && (typeof window[functionName] === "function")) {
                    response = window[functionName](parent, parent.val(), source);

                    if (response != null) {
                        if (Array.isArray(response)) {
                            setOptions(source, response);
                        } else if (typeof response.done === "function") {
                            response.done(function (data) {
                                setOptions(source, data);
                            });
                        }
                    }
                }
                else if (url != null) {
                    response = postUrl(url, { parentValue: parent.val() });
                    response.done(function (data) {
                        var options = [];
                        if (data.IsValid) {
                            var conversion = ["Text", "Value"];
                            options = extractFieldsFromComplexArray(data.Data, conversion);
                        }
                        setOptions(source, options);
                    });
                }
            };

            var attachParentChangeFunction = function () {
                var parentSelector = source.attr("data-dropdown-cascade");
                var parent = $(parentSelector);
                parent.change(loadFunction);
            };

            source.data("CascadeDropdown",
            {
                Load: loadFunction,
                AttachParentChangeEvent: attachParentChangeFunction,
            });
        });
    };
}(jQuery));

//Kendo dropdown
(function ($) {
    $.fn.KDropDown = function () {
        var attrValue = "data-kdropdown-value";
        var attrDataSource = "data-kdropdown-source";
        var attrUrl = "data-kdropdown-source-url";
        var attrOptionLabel = "data-kdropdown-optionLabel";
        var attrCascadeFrom = "data-kdropdown-cascadeFrom";
        var attrDataTextField = "data-kdropdown-textField";
        var attrDataValueField = "data-kdropdown-valueField";
        var attrDataCascadeFrom = "data-kdropdown-cascadefrom";
        var attrCascadeFromField = "data-kdropdown-cascadeFromField";


        return this.each(function () {
            var source = $(this);

            var initialStateCSS = function () {

                var kendoSelect = source.data('kendoDropDownList');
                var placeHolder = source.attr('data-kdropdown-optionLabel');
                var textField = source.attr('data-kdropdown-textField');
                var kendeoSelectedView = source.prev();
                kendeoSelectedView.removeClass('k-dropdown-item-placeholder');
                var selectedItem = kendoSelect.dataItem();
                if (selectedItem != null) {
                    var selectedText = selectedItem[textField];

                    if (selectedText === placeHolder) {
                        kendeoSelectedView.addClass('k-dropdown-item-placeholder');
                    }
                }
            };

            var init = function () {
                var options = {
                    value: null,
                    dataTextField: "Text",
                    dataValueField: "Value"
                };
                var dataValue = source.attr(attrValue)
                if (dataValue != null) {
                    options.value = dataValue;
                }
                var dataTextField = source.attr(attrDataTextField)
                if (dataTextField != null) {
                    options.dataTextField = dataTextField;
                }
                var dataValueField = source.attr(attrDataValueField)
                if (dataValueField != null) {
                    options.dataValueField = dataValueField;
                }

                var optionLabel = source.attr(attrOptionLabel)
                if (optionLabel != null) {
                    $.extend(options, { optionLabel: optionLabel });
                }

                var cascadeFrom = source.attr(attrCascadeFrom)
                if (cascadeFrom != null) {
                    $.extend(options, { cascadeFrom: cascadeFrom });
                }

                var cascadeFromField = source.attr(attrCascadeFromField)
                if (cascadeFromField != null) {
                    $.extend(options, { cascadeFromField: cascadeFromField });
                }

                var dataSource = source.attr(attrDataSource);
                var isClientFiltering = dataSource != null;
                var url = source.attr(attrUrl);
                if (isClientFiltering) {
                    $.extend(options, {
                        serverFiltering: false,
                        dataType: "json",
                        type: "GET",
                        dataSource: $.parseJSON(dataSource)
                    });

                }
                else if (url != null) {
                    $.extend(options, {
                        dataSource:
                        {
                            serverFiltering: true,
                            dataType: "json",
                            type: "GET",
                            transport: { read: url }
                        }
                    });
                    var dataCascadeFrom = source.attr(attrDataCascadeFrom);
                    if (dataCascadeFrom != null) {
                        $.extend(options.dataSource.transport, { parameterMap: null });
                        options.dataSource.transport.parameterMap = function (opts, type) { return { parentId: opts.filter.filters[0].value }; };
                        //$.extend(options.dataSource,
                        //    { parameterMap: function (opts, type) { return { parentId: opts.filter.filters[0].value }; } }
                        //);
                    }

                }

                source.kendoDropDownList(options);
                var selectKendo = source.data('kendoDropDownList');
                var items = selectKendo.items();
                if (items.length > 0) {
                    var firstItem = $(items[0]);
                    var text = firstItem.text();
                    if (optionLabel === text) {
                        firstItem.addClass('k-dropdown-item-placeholder')
                    }
                }

                initialStateCSS();
            };

            source.data("KDropDown",
            {
                Init: init
                , SetStateCSS: initialStateCSS
            });
        });
    };
}(jQuery));

//Switcherify plugin
(function ($) {
    $.fn.linetableChildTable = function (classline) {

        $(this).addClass(classline);

        // Cabecera Contenido
        $(this).find('tr').prev().find('td').first().append('<span class="line-fisrt">&nbsp;</span>');
        $(this).find('tr').first().find('th').first().append('<span class="line-fisrt">&nbsp;</span>');
        var tamanoTh = ($(this).find('tr').prev().find('td')[0]).offsetHeight;
        $(this).find('tr').prev().find('span.line-fisrt').css('height', (tamanoTh) + 'px');
        $(this).find('tr').first().find('th').find('span.line-fisrt').css({ 'height': (tamanoTh / 2) + 'px', "margin-left": "-1px" });

        //Columna contenedora de tabla

        var colContenedora = $(this).find('tbody tr').first().next().find('td').first();
        colContenedora.append('<span class="line">&nbsp;</span>');
        var tamano = ($(this).find('tbody tr').first().next().find('td')[0]).offsetHeight;
        colContenedora.find('span.line').css('height', (tamano - tamanoTh - 1) + 'px');
        colContenedora.find('span.line').css('margin-left', '11px');

        //lineas horizontales

        var childTable = $(this).find('table');
        var childRows = childTable.find('tr').not(':first');
        var marginTopRows = tamanoTh / 2 + 5;

        childRows.each(function () {
            marginTopRows += (tamanoTh);
            colContenedora.append('<span class="line-horizontal">&nbsp;</span>')
            colContenedora.find('span.line-horizontal').last().css({ 'margin-top': marginTopRows + 'px', "margin-left": "11px" });
        });



        //// cabecera tabla
        //var cabecera = ChildTable.find('tr').first();
        //cabecera.find('th').first().append('<span class="line">&nbsp;</span>');
        //var tamano = (ChildTable.find('th')[0]).offsetHeight;
        //cabecera.find('span.line').css('height', (tamano) + 'px');


        //// Contenido vertical
        //ChildTable.find('tr').not(':first').not(':last').not(':last').each(function () {
        //    if (!$(this).find('td:first button').is('button')) {
        //        $(this).find('td').first().append('<span class="line">&nbsp;</span>');
        //        var tamano = ($(this).find('td')[0]).offsetHeight;
        //        $(this).find('span.line').css('height', tamano + 'px');
        //        $(this).find('span.line').css('margin-left', '6px');

        //    }
        //    else {
        //        $(this).find('td').first().append('<span class="line-up">&nbsp;</span>');
        //        $(this).find('td').first().append('<span class="line-down">&nbsp;</span>');
        //        var tamano = ($(this).find('td')[0]).offsetHeight / 2;
        //        $(this).find('span.line-up').css('height', tamano + 'px');
        //        $(this).find('span.line-down').css('height', tamano + 'px');
        //    }
        //});

        ////contenido horizontal
        //$(this).find('tr').not(':first').each(function () {
        //    if (!$(this).find('td:first button').is('button')) {
        //        $(this).find('td').first().append('<span class="line-horizontal">&nbsp;</span>');
        //        $(this).find('span.line-horizontal').css('margin-top', (cabecera.height() / 4.5) + 'px');
        //    }
        //});

        ////Final
        //var final = $(this).find('tr').not('.hide').last();
        //final.find('td').first().append('<span class="line">&nbsp;</span>');
        //var tamano = (final.find('td')[0]).offsetHeight / 2 + 2;
        //final.find('span.line').css('height', tamano + 'px');
        //final.find('span.line').css('margin-left', '6px');

        return $(this)
    };
}(jQuery));

//Add startsWith function to string (IE dont have this function)
String.prototype.startsWith = function (start) {
    return this.indexOf(start) == 0;
    //return this.replace(/^\s+|\s+$/g, "");
};

//DropdownSearchBox
(function ($) {

    var proto = $.ui.autocomplete.prototype,
        initSource = proto._initSource;

    function filter(array, term) {
        var matcher = new RegExp($.ui.autocomplete.escapeRegex(term), "i");
        return $.grep(array, function (value) {
            return matcher.test($("<div>").html(value.label || value.value || value).text());
        });
    }

    $.extend(proto, {
        _initSource: function () {
            if (this.options.html && $.isArray(this.options.source)) {
                this.source = function (request, response) {
                    response(filter(this.options.source, request.term));
                };
            } else {
                initSource.call(this);
            }
        },

        _renderItem: function (ul, item) {
            return $("<li></li>")
                .data("item.autocomplete", item)
                .append($("<a></a>")[this.options.html ? "html" : "text"](item.label))
                .appendTo(ul);
        }
    });
})(jQuery);

$(function ($) {
    $.fn.dropDownSeachBox = function () {
        $(this).find("#ddsb_searchInputBox").autocomplete({
            source: function (request, response) {
                $.ajax({
                    dataType: "json",
                    type: 'get',
                    url: this.element.attr("data-search-ur"),
                    data: { filter: request.term },
                    success: function (data) {
                        if (data.length == 0) {
                            showMessage("No data to display");
                            return;
                        }

                        if (!data.IsValid && data.ErrorMessage != null && data.ErrorMessage != '') {
                            showMessage(data.ErrorMessage);
                            return;
                        }

                        response($.map(data.ListResponse, function (item) {
                            var imgUrl = "";
                            var additionalData = "";
                            var row = { label: "", value: "", img: "" };
                            if (!item.Value) {
                                row.label = item.Text;
                            }
                            if (item.PathTemplate != null) {
                                imgUrl = "<img src='" + item.PathTemplate + "' class='img-flag' />";
                            }
                            if (item.AdditionalData != null) {
                                additionalData = "additional-data='" + item.AdditionalData + "'";
                            }

                            var row = {
                                label: "",
                                value: ""
                            };

                            row.label = $('<span ' + additionalData + '> ' + imgUrl + '  ' + item.Text + '</span>');
                            row.value = item.Value;
                            return row;
                        }));
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        showshowMessage(errorThrown);
                    }
                })
            },
            html: true,
            select: function (event, ui) {
                event.preventDefault();
                var element = $(ui.item.label);

                $(this).parent().find("input.hidden").val(ui.item.value);
                $(this).parent().find("input.hidden").attr("value", ui.item.value);
                if ($(this).attr("data-show-photo").toLowerCase() == "true") {
                    $(this).parent().find("img").attr("src", element.find("img").attr("src"));
                    $(this).parent().find("img").removeClass("hide");
                }

                $(this).val(element.text().trim());

                if ($(this).parent().find('ul.parsley-errors-list.filled').length > 0) {
                    $(this).parent().find('ul.parsley-errors-list.filled').find('li').remove();
                    $(this).parent().find('ul.parsley-errors-list.filled').removeClass('filled');
                }
                if ($(this).attr("aditional-function-select") != "") {
                    var data = { item: ui.item, content: $(this).closest('tr') };
                    triggerAction(data, $(this).attr("aditional-function-select"));
                }
            }
        }).on("keyup keypress blur click", function () {
            $(this).autocomplete("close");
            $(this).autocomplete("disable");
        }).on("keyup", function () {
            $(this).closest('.ctrl.selectFilter').find("input.hidden").val("");
            $(this).closest('.ctrl.selectFilter').find("input.hidden").attr("value", "");
            $(this).autocomplete("close");
            $(this).autocomplete("disable");
        });

        //region button
        $(this).find("button").on("click", function () {
            $(this).parent().find("#ddsb_searchInputBox").autocomplete("enable").autocomplete("search");
        }).on("blur", function () {
            $(this).parent().find("#ddsb_searchInputBox").autocomplete("close");
        });
        //endregion
        return $(this);
    }
}(jQuery));

//  Attributes Module front end API
//Usage:
//alert("RAREATTR exists:" + Attributes_Exists('RAREATTR'));
//alert(Attributes_GetElementByAttributeCode("DBOOK").val());

function Attributes_GetElementByAttributeCode(attributeCode) {
    return $("[data-persist-dynamic='true'][data-persist-code='" + attributeCode + "']");
}

function Attributes_GetElementByAttributeId(attributeId) {
    return $("[data-persist-dynamic='true'][data-persist-id='" + attributeId + "']");
}

function Attributes_Exists(attributeCode) {
    return $("[data-persist-dynamic='true'][data-persist-code='" + attributeCode + "']").length > 0;
}
//  End Attributes Module front end API

File.getFileName = function (fullPath) {
    if (fullPath == null) {
        return null;
    }
    return fullPath.replace(/^.*?([^\\\/:]*)$/, '$1');
};

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

//Custom Chosen
(function ($) {
    $.fn.CustomChosen = function () {

        return this.each(function () {
            var source = $(this);

            if ((source.attr('data-role') === 'drop-multiple') &&
                (source.attr('data-bind') === 'true')) {
                var selectableGroups = source.attr('data-selectable-groups') === "true";
                if (selectableGroups) {
                    source.addClass('chosen-group-selectable');
                }

                var sourceChosen = source.chosen(
                {
                    inherit_select_classes: true
                });
                var chosen = source.data("chosen");
                var fnSelect = chosen.result_select;

                chosen.result_select = function (evt) {
                    evt["metaKey"] = true;
                    evt["ctrlKey"] = true;
                    chosen.result_highlight.addClass("result-selected");
                    return fnSelect.call(chosen, evt);
                };

                chosen.container.on('click', '.chosen-results li.group-result', function () {
                    // Get unselected items in this group
                    var unselected = $(this).nextUntil('.group-result').not('.result-selected');
                    if (unselected.length) {
                        // Select all items in this group
                        unselected.trigger('mouseup');
                    } else {
                        $(this).nextUntil('.group-result').each(function () {
                            // Deselect all items in this group
                            $('a.search-choice-close[data-option-array-index="' + $(this).data('option-array-index') + '"]').trigger('click');
                        });
                    }
                });
            }
        });
    };
}(jQuery));


//Autonumeric
(function ($) {
    $.fn.GenerateNewId = function (lastId) {
        var listIds = new Array();
        if (lastId == null) {
            lastId = 0;
        }

        this.each(function () {
            var source = $(this);

            var lastGeneratedId = source.data('lastGeneratedId');
            if (lastGeneratedId == null) {
                lastGeneratedId = lastId;
                source.data('lastGeneratedId', lastGeneratedId);
            }
            var newId = lastGeneratedId + 1;
            source.data('lastGeneratedId', newId);
            listIds.push(newId);
        });

        if (listIds.length == 1) {
            return listIds[0];
        }
        return listIds;
    };

}(jQuery));


//Link Modal
(function ($) {
    $.fn.LinkModalButton = function (options) {

        var _defaultOption = {
            Event: "click",
            LinkModalId: null,
            LinkModalValueId: null
        };


        return this.each(function () {
            var finalOption = $.extend({}, _defaultOption, options);

            var button = $(this);
            var launch = button.attr('data-link-modal-laucher');
            var event = button.attr('data-link-modal-laucher-event');
            if (event != null) {
                finalOption.Event = event;
            }

            if (launch != null) {
                if (launch.indexOf(';') != -1) {
                    finalOption.LinkModalValueId = [];
                    var pair = launch.split(';');
                    for (var i = 0; i < pair.length; i++) {
                        var item = pair[i];
                        var data = item.split(',');
                        finalOption.LinkModalValueId.push({ LinkModalId: data[1], Value: data[0] });
                    }
                } else {
                    finalOption.LinkModalId = launch;
                }
            }


            if (finalOption.LinkModalId != null) {
                button.on(finalOption.Event, function () {
                    var modal = $("#" + finalOption.LinkModalId);
                    var openPopupButton = modal.siblings('[data-name="open-popup-button-container"]').children('button');
                    openPopupButton.data('pressed-by', button);
                    openPopupButton.click();
                });
            } else if (finalOption.LinkModalValueId != null) {
                button.on(finalOption.Event, function () {
                    var value = $(this).val();
                    var modalInfo = finalOption.LinkModalValueId.filter(function (it, ix, list) {
                        return (it.Value == value);
                    });
                    if (modalInfo.length == 1) {
                        var modal = $("#" + modalInfo[0].LinkModalId);
                        var openPopupButton = modal.siblings('[data-name="open-popup-button-container"]').children('button');
                        openPopupButton.data('pressed-by', button);
                        openPopupButton.click();
                    }
                });
            }

        });
    };
}(jQuery));


//Collapse Single Button
(function ($) {
    $.fn.CollapseSingle = function (options) {

        var _defaultOption = {};

        return this.each(function () {

            var button = $(this);
            var finalOption = $.extend({}, _defaultOption, options);

            function getInformation() {

                var info = null;

                var targetRegion = null;
                var duration = 'fast';
                var collapseAllSelector = null;
                var hideClass = null;

                if (button.attr("data-collapse-single-region") != null) {
                    targetRegion = button.attr("data-collapse-single-region");
                } else if ((finalOption != null) && (finalOption.TargetRegion != null)) {
                    targetRegion = finalOption.TargetRegion;
                }

                if (button.attr("data-collapse-single-hide-class") != null) {
                    hideClass = button.attr("data-collapse-single-hide-class");
                } else if ((finalOption != null) && (finalOption.HideClass != null)) {
                    hideClass = finalOption.HideClass;
                }

                if (button.attr("data-collapse-single-duration") != null) {
                    duration = button.attr("data-collapse-single-duration");
                } else if ((finalOption != null) && (finalOption.Duration != null)) {
                    duration = finalOption.Duration;
                }

                if (targetRegion != null) {
                    var region = null;

                    if (targetRegion.indexOf('@') == 0) {
                        targetRegion = targetRegion.substring(2);
                        var rules = targetRegion.split(';');

                        region = button;
                        for (var i = 0; i < rules.length; i++) {
                            var rule = rules[i];
                            var internalSelector;

                            if (rule.indexOf("parent") == 0) {
                                region = region.parent();
                            } else if (rule.indexOf("next") == 0) {
                                region = region.next();
                            } else if (rule.indexOf("closest") == 0) {
                                internalSelector = rule.substring(8, rule.length - 1).trim();
                                if (internalSelector == '') {
                                    throw "Invalid Selector";
                                }
                                region = region.closest(internalSelector);
                            } else if (rule.indexOf("sibling") == 0) {
                                internalSelector = rule.substring(8, rule.length - 1).trim();
                                if (internalSelector == '') {
                                    throw "Invalid Selector";
                                }
                                region = region.siblings(internalSelector);
                            } else if (rule.indexOf("children") == 0) {
                                internalSelector = rule.substring(9, rule.length - 1).trim();
                                if (internalSelector == '') {
                                    throw "Invalid Selector";
                                }
                                region = region.children(internalSelector);
                            } else {
                                throw "Invalid Selector";
                            }
                        }

                    } else {
                        region = $(targetRegion);
                    }

                    if (button.attr("data-collapse-single-collapseall") != null) {
                        collapseAllSelector = button.attr("data-collapse-single-collapseall");
                    } else if ((finalOption != null) && (finalOption.CollapseAllSelector != null)) {
                        collapseAllSelector = finalOption.CollapseAllSelector;
                    }

                    info = {
                        Duration: duration,
                        Region: region,
                        CollapseAll: null,
                        HideClass: null,
                    }

                    if (collapseAllSelector != null) {
                        info.CollapseAll = $(collapseAllSelector);
                    }

                    if ((hideClass != null) && (hideClass.trim() != '')) {
                        info.HideClass = hideClass;
                    }
                }

                return info;
            };

            function slideContent(info) {
                if (info == null) {
                    info = getInformation();
                }

                var mydata = button.data("CollapseSingle");

                if (info != null) {
                    if (mydata.IsCollapse) {
                        expandContent(info);
                    } else {
                        collapseContent(info);
                    }
                }
            };

            function expandContent(info) {
                var mydata = button.data('CollapseSingle');
                var regionContent = button.closest('tr');
                var isTr = false;

                if ($(regionContent).length) {
                    isTr = true;
                }

                if (info == null) {
                    info = getInformation();
                }

                if (info != null && mydata.IsCollapse) {
                    button.removeClass('collapse');

                    if (info.HideClass == null) {
                        var velocity = parseInt(info.Duration);
                        if (isNaN(velocity)) {
                            velocity = info.Duration;
                        }

                        info.Region.slideDown(velocity);
                    } else {
                        info.Region.removeClass(info.HideClass);
                    }

                    if (isTr) {
                        info.Region.addClass('buttonCollapseExpanded');
                        regionContent.addClass('buttonCollapseExpandedTop')
                    }

                    mydata.IsCollapse = false;
                    button.data("CollapseSingle", mydata);
                }
            };

            function collapseContent(info) {
                var mydata = button.data('CollapseSingle');
                var regionContent = button.closest('tr');
                var isTr = false;

                if ($(regionContent).length) {
                    isTr = true;
                }

                if (info == null) {
                    info = getInformation();
                }

                if (info != null && !mydata.IsCollapse) {
                    button.addClass('collapse');

                    if (info.HideClass == null) {
                        var velocity = parseInt(info.Duration);
                        if (isNaN(velocity)) {
                            velocity = info.Duration;
                        }

                        info.Region.slideUp(velocity);
                    } else {
                        info.Region.addClass(info.HideClass);
                    }

                    if (isTr) {
                        info.Region.removeClass('buttonCollapseExpanded');
                        regionContent.removeClass('buttonCollapseExpandedTop')
                    }

                    mydata.IsCollapse = true;
                    button.data("CollapseSingle", mydata);
                }
            };

            function redrawCollapseAll(info) {
                var mydata = button.data('CollapseSingle');
                if (info == null) {
                    info = getInformation();
                }

                if (info == null || info.CollapseAll == null || info.CollapseAll.length == 0) {
                    return;
                }

                var singleSelector = info.CollapseAll.attr('data-collapse-all-collapse-single');
                var singles = $(singleSelector);

                var totalSingles = singles.length;

                var collapsedSingles = singles.filter(function () {
                    var single = $(this);
                    return single.data('CollapseSingle').IsCollapse;
                });
                var totalCollapsedSingles = collapsedSingles.length;

                var collapseAllData = info.CollapseAll.data('CollapseAll');
                if (totalSingles == totalCollapsedSingles) {
                    collapseAllData.DrawCollapsed();
                } else if (totalCollapsedSingles == 0 && totalSingles > 0) {
                    collapseAllData.DrawExpanded();
                }


            };

            var data = {
                IsCollapse: false,
                SlideContent: slideContent,
                CollapseContent: collapseContent,
                RedrawCollapseAll: redrawCollapseAll,
            };

            button.data("CollapseSingle", data);

            button.on('click', function () {
                var info = getInformation();
                data.SlideContent(info);
                data.RedrawCollapseAll(info);
            });
        });
    };
}(jQuery));


//Collapse All Button
(function ($) {
    $.fn.CollapseAll = function (options) {

        var _defaultOption = {};

        return this.each(function () {

            var button = $(this);
            function toggleAllSingle(selector) {
                var collapseSingles = $(selector);
                var isCollapsing = button.hasClass('collapse');
                var newText;
                if (isCollapsing) {
                    drawAsCollapsed();
                } else {
                    drawAsExpanded()
                }
                collapseSingles.each(function () {
                    var single = $(this);
                    var isSingleCollapse = single.hasClass('collapse');
                    if (isCollapsing != isSingleCollapse) {
                        var data = single.data('CollapseSingle');
                        if ((data != null) && (data.SlideContent != null)) {
                            data.SlideContent();
                        }
                    }
                });
            };

            function toggleAllRegion(regions) {
                var isCollapsing = button.hasClass('collapse');

                if (isCollapsing) {
                    button.removeClass('collapse');
                } else {
                    button.addClass('collapse');
                }

                regions.forEach(function (selector) {
                    var region = $(selector);
                    var isRegionCollapsed = !region.is(':visible');

                    if (isCollapsing != isRegionCollapsed) {
                        if (isCollapsing) {
                            region.slideUp("fast");
                        } else {
                            region.slideDown("fast");
                        }
                    }
                });
            };

            function toggleAll() {
                var targetSingleSelector = button.attr('data-collapse-all-collapse-single');
                var targetRegionSelector = button.attr('data-collapse-all-collapse-region');

                if ((targetSingleSelector != null) && (targetSingleSelector.trim() != "")) {
                    toggleAllSingle(targetSingleSelector);
                } else if ((targetRegionSelector != null) && (targetRegionSelector.trim() != "")) {
                    var regions = targetRegionSelector.split(',');
                    toggleAllRegion(regions);
                }
            };

            function drawAsCollapsed() {
                newText = button.attr('data-collapse-all-exp-text');
                button.removeClass('collapse');
                button.find('label').html(newText)
            }
            function drawAsExpanded() {
                newText = button.attr('data-collapse-all-coll-text');
                button.addClass('collapse');
                button.find('label').html(newText);
            }

            var finalOption = $.extend({}, _defaultOption, options);

            var data = {
                ToggleAll: toggleAll,
                DrawCollapsed: drawAsCollapsed,
                DrawExpanded: drawAsExpanded
            };
            button.data("CollapseAll", data);

            button.on('click', function () {
                data.ToggleAll();
            });

        });
    };
}(jQuery));


//Checklist
(function ($) {

    var CheckList,
        AbstractCheckList,
        CheckListValidator,
        CheckListEvaluator,
        __rowId = 0,
        __hasProp = {}.hasOwnProperty,
        __extends = function (child, parent) {
            for (var key in parent) {
                if (__hasProp.call(parent, key)) {
                    child[key] = parent[key];
                }
            }

            function ctor() { this.constructor = child; }

            ctor.prototype = parent.prototype;
            child.prototype = new ctor();
            child.__super__ = parent.prototype;
            return child;
        };


    AbstractCheckList = (function () {
        function AbstractCheckList(form_field, options) {
            this.form_field = form_field;
            this.options = options != null ? options : {};
            this.set_default_options();
            this.setup();
            this.setup_html();
        }

        AbstractCheckList.prototype.set_default_options = function () {
            this.Items = this.options.Items || AbstractCheckList.defaultOptions.Items;
            this.CurrentItems = AbstractCheckList.defaultOptions.CurrentItems;
            this.cssClass = this.options.CssClass || AbstractCheckList.defaultOptions.CssClass;
            this.HeaderItemName = this.options.HeaderItemName || AbstractCheckList.defaultOptions.HeaderItemName;
            this.HeaderMandatory = this.options.HeaderMandatory || AbstractCheckList.defaultOptions.HeaderMandatory;
            this.HeaderCompleted = this.options.HeaderCompleted || AbstractCheckList.defaultOptions.HeaderCompleted;
            this.HeaderDateCompleted = this.options.HeaderDateCompleted || AbstractCheckList.defaultOptions.HeaderDateCompleted;
            this.HeaderUser = this.options.HeaderUser || AbstractCheckList.defaultOptions.HeaderUser;
            this.DrawMode = this.options.DrawMode || AbstractCheckList.defaultOptions.DrawMode;
            this.Editable = this.options.Editable || AbstractCheckList.defaultOptions.Editable;
            this.ValidateOnLoad = this.options.ValidateOnLoad || AbstractCheckList.defaultOptions.ValidateOnLoad;
            this.CustomErrorMessage = this.options.CustomErrorMessage || AbstractCheckList.defaultOptions.CustomErrorMessage;
            this.ValidationMessageFormat = this.options.ValidationMessageFormat || AbstractCheckList.defaultOptions.ValidationMessageFormat;
            this.PermitUncompleteAutoCompleted = this.options.PermitUncompleteAutoCompleted || AbstractCheckList.defaultOptions.PermitUncompleteAutoCompleted;
            this.ScrollToError = this.options.ScrollToError || AbstractCheckList.defaultOptions.ScrollToError;
            this.CurrentUser = this.options.CurrentUser || AbstractCheckList.defaultOptions.CurrentUser;
            this.DateCompletedLessOrEqualToday = this.options.DateCompletedLessOrEqualToday || AbstractCheckList.defaultOptions.DateCompletedLessOrEqualToday;
        };

        AbstractCheckList.ValidationMessageFormat = {
            None: 0,
            ChecklistItemRed: 1
        };

        AbstractCheckList.defaultOptions = {
            Items: [],
            CurrentItems: [],
            CustomErrorMessage: {},
            CssClass: 'tableNormal',
            HeaderItemName: 'Checklist Item',
            HeaderMandatory: 'Mandatory?',
            HeaderCompleted: 'Completed?',
            HeaderDateCompleted: 'Date Completed',
            HeaderUser: 'User',
            DrawMode: 'inside', //inside, next, undefined (if table)
            Editable: false,
            ValidateOnLoad: false,
            ValidationMessageFormat: AbstractCheckList.ValidationMessageFormat.None,
            PermitUncompleteAutoCompleted: false,
            ScrollToError: false,
            CurrentUser: '',
            DateCompletedLessOrEqualToday: false,
        };

        return AbstractCheckList;
    })();

    $.fn.extend({
        checklist: function (options) {
            return this.each(function (input_field) {
                var $this, checklist;
                $this = $(this);
                checklist = $this.data('checklist');
                if (options === 'destroy' && checklist instanceof CheckList) {
                    //checklist.destroy();
                }
                else if (!(checklist instanceof CheckList)) {
                    $this.data('checklist', new CheckList(this, options));
                }
            });
        }
    });

    CheckList = (function (_super) {
        __extends(CheckList, _super);

        function CheckList() {
            _ref = CheckList.__super__.constructor.apply(this, arguments);
            return _ref;
        }

        var __privateFunctions = {


            showValidationResult:
                function (checklist, validationMessageFormat, errorList) {

                    var trs = checklist.form_field_jq.children('tbody').children();

                    if (validationMessageFormat == CheckList.ValidationMessageFormat.ChecklistItemRed) {

                        trs.each(function () {
                            var row = $(this);
                            __privateFunctions.showValidationChecklistItemRed(row, false);
                        });

                        errorList.forEach(function (rowErrorInfo) {
                            __privateFunctions.showValidationChecklistItemRed(rowErrorInfo.Row, !rowErrorInfo.RowValidation.CanBeSaved);
                        });
                    }
                },

            showValidationRowResult:
                function (validationMessageFormat, row, warningList, errorList) {

                    if (validationMessageFormat == CheckList.ValidationMessageFormat.ChecklistItemRed) {

                        __privateFunctions.showValidationChecklistItemRed(row, Array.isArray(errorList) && errorList.length > 0);
                    }
                },

            showValidationChecklistItemRed:
                function (row, hasError) {

                    var td = row.find('[data-checklist-col=itemName]');
                    if (hasError == true) {
                        td.css('color', 'red');
                        td.css('text-decoration', 'underline');
                    } else {
                        td.css('color', '');
                        td.css('text-decoration', '');
                    }
                },

            getRowData:
                function (checklist, row) {
                    var filter = checklist.CurrentItems.filter(function (item) {
                        var rowId = row.attr('data-checklist-row-id');
                        return item.__rowId == rowId;
                    });

                    if (filter.length == 1) {
                        return filter[0];
                    }

                    return null;
                },

        };

        CheckList.prototype.setup = function () {
            this.form_field_jq = $(this.form_field);
            this.RowValidations = {};
            this.RowEvaluations = {};
        };

        CheckList.prototype.setup_html = function () {

            if (this.form_field.tagName != 'TABLE') {
                var table = $('<table>');

                if (DrawMode == 'inside') {
                    table.appendTo(target);
                } else if (DrawMode == 'next') {
                    table.insertBefore(next);
                } else {
                    table.insertBefore(next);
                }
                this.form_field_jq = table;
                this.form_field = table[0];
            }

            this.form_field_jq.attr('data-role', 'checklist');
            this.form_field_jq.addClass(this.cssClass);
            this.form_field_jq.html('');

            var thead = $('<thead>');
            var tr = $('<tr>');

            var th = $('<th>');
            th.attr('data-checklist-col', 'itemName');
            th.html(this.HeaderItemName);
            th.appendTo(tr);

            th = $('<th>');
            th.attr('data-checklist-col', 'mandatory');
            th.html(this.HeaderMandatory);
            th.appendTo(tr);

            th = $('<th>');
            th.attr('data-checklist-col', 'completed');
            th.html(this.HeaderCompleted);
            th.appendTo(tr);

            th = $('<th>');
            th.attr('data-checklist-col', 'dateCompleted');
            th.html(this.HeaderDateCompleted);
            th.appendTo(tr);

            th = $('<th>');
            th.attr('data-checklist-col', 'user');
            th.html(this.HeaderUser);
            th.appendTo(tr);

            tr.appendTo(thead);

            thead.appendTo(this.form_field_jq);

            var tbody = $('<tbody>');
            tbody.appendTo(this.form_field_jq);

            var regItem = this.registerItemObj;

            if (this.Items != null && Array.isArray(this.Items)) {
                for (var i = 0; i < this.Items.length; i++) {
                    var item = this.Items[i];
                    this.registerItemObj(item);
                }
            }

            this.ExecuteEvaluations();

            if (this.ValidateOnLoad) {
                this.Validate(CheckList.ValidationMessageFormat.None, false);
            }

            this.form_field_jq.on('change', '[name="isCompleted"]', function () {
                var source = $(this);
                var row = source.closest('tr');
                var table = row.closest('table');
                var data = table.data('checklist');
                data.ValidateRow(row, null, false);
            })

            this.form_field_jq.on('change', '[name="dateCompleted"]', function () {
                var source = $(this);
                var row = source.closest('tr');
                var table = row.closest('table');
                var data = table.data('checklist');
                data.ValidateRow(row, null, false);
            })

            this.form_field_jq.on('checklist-validate-row', function (event, row) {
                var source = $(this);
                var data = source.data('checklist');
                data.ValidateRow($(row), null, false);
            })

            this.form_field_jq.on('checklist-validate', function () {
                var source = $(this);
                var data = source.data('checklist');
                data.Validate(null, false);
            })

            this.form_field_jq.on('checklist-evaluate-mandatary-row', function (event, row) {
                var source = $(this);
                var data = source.data('checklist');
                data.ExecuteEvaluations();
                data.ValidateRow($(row), null, false);
            })

        };

        CheckList.prototype.registerItemObj = function (item) {
            this.registerItem(item);
        };

        CheckList.prototype.registerItem = function (item) {
            var tr = $('<tr>');
            var currentUser = this.CurrentUser;
            var rowId = __rowId++;

            this.CurrentItems.push(
            {
                __rowId: rowId,
                ChecklistStageModalityItemId: item.ChecklistStageModalityItemId,
                ChecklistItem: item.ChecklistItem,
                ChecklistItemError: item.ChecklistItemError,
                MandatoryType: item.MandatoryType,
                MandatorySelector: item.MandatorySelector,
                IsAuto: item.IsAuto,
                AutoCompleteText: item.AutoCompleteText,
                Completed: item.Completed,
                DateCompleted: item.DateCompleted,
                User: item.User,
                Validations: item.Validations,
                LinkText: item.LinkText,
                LinkHref: item.LinkHref
            });

            tr[item.IsHidden ? 'addClass' : 'removeClass']('hidden');
            tr.attr('data-id', item.ChecklistStageModalityItemId);
            tr.attr('data-checklist-row-id', rowId);
            tr.attr('id', 'cli-' + rowId);

            var td = $('<td>');
            td.attr('data-checklist-col', 'itemName');
            var spanName = $('<span>');
            spanName.html(item.ChecklistItem + ' ');

            if (item.ChecklistItemError != null) {
                spanName.attr('data-toggle', 'tooltip');
                spanName.attr('title', item.ChecklistItemError);
            }

            spanName.appendTo(td);
            if ((item.LinkText != null) && (item.LinkText.trim() != '')) {
                var openSpan = $('<span>');
                openSpan.html('(');
                var closeSpan = $('<span>');
                closeSpan.html(')');

                var linkName = $('<a>');
                if ((item.LinkHref == null) || item.LinkHref.trim() == '') {
                    item.LinkHref = '#';
                }
                linkName.attr('href', item.LinkHref);
                linkName.html(item.LinkText);
                openSpan.appendTo(td);
                linkName.appendTo(td);
                closeSpan.appendTo(td);
            }

            td.appendTo(tr);

            td = $('<td>');
            td.attr('data-checklist-col', 'mandatory');
            td.appendTo(tr);


            td = $('<td>');
            td.attr('data-checklist-col', 'completed');
            var check = $('<input>');
            check.attr('type', 'checkbox');
            check.attr('name', 'isCompleted');
            check.prop('checked', item.Completed == true);
            check.appendTo(td);

            if (this.Editable != true) {
                check.attr('disabled', 'disabled');
            }

            if (item.IsAuto == true) {
                var span = $('<span>');
                span.html(item.AutoCompleteText);
                span.appendTo(td);
                check.addClass('hide');
                check.attr('disabled', 'disabled');
            }
            td.appendTo(tr);


            td = $('<td>');
            td.attr('data-checklist-col', 'dateCompleted');
            var dateString = '';
            if (item.DateCompleted != null) {
                var date = new Date(item.DateCompleted);
                var day = date.getUTCDate()
                var month = $.datepicker._defaults.monthNamesShort[date.getUTCMonth()];
                var year = date.getUTCFullYear();

                dateString = day + ' ' + month + ' ' + year;
            }
            if (this.Editable != true) {

                var datepicker = $('<input>');
                datepicker.addClass('hide');
                datepicker.attr('name', 'dateCompleted');
                datepicker.attr('type', 'text');
                datepicker.attr('value', dateString);
                datepicker.attr('disabled', 'disabled');
                datepicker.appendTo(td);

                var span = $('<span>');
                span.html(dateString);
                span.appendTo(td);

            } else {
                var datepicker = $('<input>');
                datepicker.addClass('inputText');
                datepicker.addClass('datepicker-default');

                datepicker.attr('name', 'dateCompleted');
                datepicker.attr('type', 'text');
                datepicker.attr('value', dateString);
                datepicker.attr('placeholder', 'DD MMM YYYY');

                if (item.Completed != true) {
                    datepicker.attr('value', '');
                    datepicker.attr('disabled', 'disabled');
                }

                datepicker.appendTo(td);
                check.change(function () {
                    if ($(this).is(':checked')) {
                        var date = new Date();
                        var day = date.getDate();
                        var month = $.datepicker._defaults.monthNamesShort[date.getMonth()];
                        var year = date.getFullYear();

                        var valueString = day + ' ' + month + ' ' + year;
                        datepicker.val(valueString);
                        datepicker.removeAttr('disabled');

                        var tdUser = $(this).closest('td').nextAll('[data-checklist-col="user"]');
                        tdUser.html(currentUser);
                    } else {
                        datepicker.val('');
                        datepicker.attr('disabled', 'disabled');
                    }
                });
            }
            td.appendTo(tr);


            td = $('<td>');
            td.attr('data-checklist-col', 'user');
            if ((item.User == null) || (item.User.trim() == '')) {
                td.html(currentUser);
            } else {
                td.html(item.User);
            }
            td.appendTo(tr);


            tr.appendTo(this.form_field_jq.find('tbody'));
            bindHandlers(tr);

            this.registerEvaluation(tr, item.MandatoryType, item.MandatorySelector);
            if (this.Editable == true) {

                if (item.IsAuto && (item.Validations != null) && Array.isArray(item.Validations)) {
                    for (var i = 0; i < item.Validations.length; i++) {
                        var validation = item.Validations[i];
                        this.registerValidation(tr, validation.Type, validation.Selector);
                    }
                }

                this.registerValidation(tr, 'ChecklistItemMandatory', item.MandatorySelector);
                this.registerValidation(tr, 'DateRequiredIfCompleted', null);
                if (this.DateCompletedLessOrEqualToday == true) {
                    this.registerValidation(tr, 'DateLessOrEqualTodayIfCompleted', null);
                }

            }
        };

        CheckList.prototype.registerValidation = function (row, type, selector) {
            if (window.ChecklistValidator.containValidator(type)) {
                var rowId = row.attr('data-checklist-row-id');
                var rowName = "row-" + rowId;

                var validations = this.RowValidations[rowName];
                if (validations == null) {
                    validations = { Row: row, Validations: [] };
                    this.RowValidations[rowName] = validations;
                }

                validations.Validations.push({ Type: type, Selector: selector });
            }
            if (window.ChecklistValidator.containTriggerChangeEvent(type)) {
                var registerEvent = window.ChecklistValidator.TriggerChangeEvent[type];
                registerEvent(row, selector);
            }
        };

        CheckList.prototype.registerEvaluation = function (row, type, selector) {
            if (window.CheckListEvaluator.containEvaluator(type)) {
                var rowId = row.attr('data-checklist-row-id');
                var rowName = "row-" + rowId;

                var evaluations = this.RowEvaluations[rowName];
                if (evaluations == null) {
                    evaluations = { Row: row, Evaluations: [] };
                    this.RowEvaluations[rowName] = evaluations;
                }

                evaluations.Evaluations.push({ Type: type, Selector: selector });
            }
            if (window.CheckListEvaluator.containTriggerChangeEvent(type)) {
                var registerEvent = window.CheckListEvaluator.TriggerChangeEvent[type];
                registerEvent(row, selector);
            }
        };

        CheckList.prototype.ExecuteEvaluations = function () {
            if (this.RowEvaluations != null) {
                for (var name in this.RowEvaluations) {
                    var evaluations = this.RowEvaluations[name];
                    var row = evaluations.Row;
                    for (var i = 0; i < evaluations.Evaluations.length; i++) {
                        var evaluation = evaluations.Evaluations[i];
                        window.CheckListEvaluator.Evaluations[evaluation.Type](row, evaluation.Selector)
                    }
                }
            }
        };

        CheckList.prototype.Validate = function (validationMessageFormat, scrollToError) {
            if (validationMessageFormat == null) {
                validationMessageFormat = this.ValidationMessageFormat;
            }
            if (scrollToError == null) {
                scrollToError = this.ScrollToError;
            }

            var result = {
                HasProblems: false,
                CanBeSaved: true,
                Validations: [],
            };

            var errorList = [];
            if (this.RowValidations != null) {
                var trs = this.form_field_jq.children('tbody').children();
                for (var i = 0; i < trs.length; i++) {
                    var row = $(trs[i]);
                    var rowValiadtion = this.ValidateRow(row, CheckList.ValidationMessageFormat.None, false);

                    result.HasProblems = result.HasProblems || rowValiadtion.HasProblems;
                    result.CanBeSaved = result.CanBeSaved && rowValiadtion.CanBeSaved;

                    errorList.push({ Row: row, RowValidation: rowValiadtion });
                }
            }

            result.Validations = errorList;

            __privateFunctions.showValidationResult(this, validationMessageFormat, errorList);

            if (scrollToError) {
                var onlyErrors = errorList.filter(function (info) {
                    return !info.RowValidation.CanBeSaved;
                });
                if (onlyErrors.length > 0) {
                    var error = onlyErrors[0];
                    var rowError = error.Row;
                    window.location.assign('#' + rowError.attr('id'));
                }
            }

            return result;
        };

        CheckList.prototype.ValidateRow = function (row, validationMessageFormat, scrollToError) {
            var result = {
                HasProblems: false,
                CanBeSaved: true,
                Warnings: [],
                Errors: [],
            };
            if (scrollToError == null) {
                scrollToError = this.ScrollToError;
            }

            var inputCompleted = row.find('td[data-checklist-col="completed"] input[name=isCompleted]');
            var rowId = row.attr('data-checklist-row-id');
            var rowData = __privateFunctions.getRowData(this, row);

            if ((this.PermitUncompleteAutoCompleted != true) && rowData.IsAuto && inputCompleted.is(':checked')) {
                return result;
            }

            if (validationMessageFormat == null) {
                validationMessageFormat = this.ValidationMessageFormat;
            }

            var finalValidationErrorList = [];
            var changeStatusValidationsErrorList = [];

            var rowId = row.attr('data-checklist-row-id');
            var rowInfo = this.RowValidations['row-' + rowId];

            if (rowInfo != null) {

                var validations = rowInfo.Validations;

                var changeStatusValidations = [];
                var finalValidations = [];

                for (var i = 0; i < validations.length; i++) {

                    var validation = validations[i];
                    var validationObj = window.ChecklistValidator.Validations[validation.Type];

                    if (validationObj.ChangeStatus == true) {
                        changeStatusValidations.push({ Function: validationObj.Function, Selector: validation.Selector, Type: validation.Type });
                    } else {
                        finalValidations.push({ Function: validationObj.Function, Selector: validation.Selector, Type: validation.Type });
                    }
                }

                if (changeStatusValidations.length > 0) {

                    var originalIsCompleted = inputCompleted.is(':checked');
                    var newIsCompleted = true;

                    for (var i = 0; i < changeStatusValidations.length; i++) {
                        var obj = changeStatusValidations[i];
                        var func = obj.Function;
                        var validate = func(row, obj.Selector);

                        if (validate != true) {
                            var customMessage = this.CustomErrorMessage[obj.Type]
                            var defaultMessage = window.ChecklistValidator.ValidationsMessage[obj.Type];
                            var message = defaultMessage;
                            if (customMessage != null) {
                                message = customMessage;
                            }
                            changeStatusValidationsErrorList.push({ ValidationType: obj.Type, ValidationMessage: message });
                            newIsCompleted = false;
                        }
                    }

                    inputCompleted.prop('checked', newIsCompleted);
                    if (originalIsCompleted != newIsCompleted) {
                        inputCompleted.change();
                    }
                }

                for (var i = 0; i < finalValidations.length; i++) {
                    var obj = finalValidations[i];
                    var func = obj.Function;
                    var validate = func(row, obj.Selector);

                    if (validate != true) {
                        var customMessage = this.CustomErrorMessage[obj.Type]
                        var defaultMessage = window.ChecklistValidator.ValidationsMessage[obj.Type];
                        var message = defaultMessage;
                        if (customMessage != null) {
                            message = customMessage;
                        }
                        finalValidationErrorList.push({ ValidationType: obj.Type, ValidationMessage: message });
                    }
                }

                __privateFunctions.showValidationRowResult(validationMessageFormat, row, changeStatusValidationsErrorList, finalValidationErrorList);


                result.HasProblems = changeStatusValidationsErrorList.length > 0 || finalValidationErrorList.length > 0;
                result.Warnings = changeStatusValidationsErrorList;
                result.Errors = finalValidationErrorList;
                result.CanBeSaved = result.Errors.length == 0;

                if (scrollToError && !result.CanBeSaved) {
                    window.location.assign('#' + row.attr('id'));
                }

                return result;
            }

            return result;
        };


        return CheckList;
    })(AbstractCheckList);


    CheckListValidator = (function () {


        var __privateFunctions = {
            genericChangeEvent: {

                tableRowRequired: function (row, selector) {
                    var splitted = selector.split(';');
                    var tableSelector = splitted[0];

                    var table = $(tableSelector);
                    var associatedChecklistItem = table.data('checklist-associated-rows');
                    if (associatedChecklistItem == null) {
                        associatedChecklistItem = {};
                    }
                    var checklistRowId = row.attr('data-checklist-row-id');

                    if (associatedChecklistItem[checklistRowId] == null) {
                        associatedChecklistItem[checklistRowId] = row;
                        table.data('checklist-associated-rows', associatedChecklistItem);
                    }
                },

                commonInput: function (row, selector) {
                    var splitted = selector.split(';');
                    var inputSelector = splitted[0];

                    var table = row.closest('table[data-role="checklist"]');
                    $(document).on('change', inputSelector, function () {
                        table.trigger('checklist-validate-row', row);
                    })
                },
            },
        };

        function CheckListValidator() {
            this.AddValidator('ChecklistItemMandatory', this.Validators.itemMandatory, 'La fila debe ser marcada como completada.', false);
            this.AddValidator('DateRequiredIfCompleted', this.Validators.dateRequired, 'La fila debe tener fecha de completado si esta completada.', false);
            this.AddValidator('DateLessOrEqualTodayIfCompleted', this.Validators.dateLessOrEqualToday, 'La fecha de completada debe ser inferior o igual a la de hoy.', false);

            this.AddValidator('TableRowsRequired', this.Validators.tableRequired, 'No hay registros en la tabla.', true, __privateFunctions.genericChangeEvent.tableRowRequired);
            this.AddValidator('AllCheckFieldTrue', this.Validators.allCheckFieldTrue, 'No se han marcado todos los checkbox.', true, __privateFunctions.genericChangeEvent.commonInput);
            this.AddValidator('SomeCheckFieldTrue', this.Validators.someCheckFieldTrue, 'Se debe marcar al menos un checkbox.', true, __privateFunctions.genericChangeEvent.commonInput);
            this.AddValidator('AllFieldRequired', this.Validators.allFieldRequired, 'No se han rellenado todos los campos.', true, __privateFunctions.genericChangeEvent.commonInput);
            this.AddValidator('SomeFieldRequired', this.Validators.someFieldRequired, 'Se debe rellenar al menos un campo', true, __privateFunctions.genericChangeEvent.commonInput);
        }

        CheckListValidator.prototype.AddValidator = function (validatorName, validation, message, changeStatus, triggerFunction) {
            this.Validations[validatorName] = { Function: validation, ChangeStatus: changeStatus };
            this.ValidationsMessage[validatorName] = message;

            if ((triggerFunction != null) && (typeof (triggerFunction) == 'function')) {
                this.TriggerChangeEvent[validatorName] = triggerFunction;
            }
        };

        CheckListValidator.prototype.Validators = {
            tableRequired: function (row, selector) {

                if (selector == null) {
                    return false;
                }
                var splitted = selector.split(';');
                var tableSelector = splitted[0];
                var valueIfNotExist = false;
                var ignoreTrSelector = null;

                var trSelector = '> tbody > tr:not(.template)';

                if (splitted.length == 2) {
                    var second = splitted[1];
                    var secondLower = second.toLowerCase();
                    if (secondLower == 'true') {
                        valueIfNotExist = true;
                    } else if (secondLower == 'false') {
                        valueIfNotExist = false;
                    } else {
                        ignoreTrSelector = second;
                    }
                } else if (splitted.length == 3) {
                    var second = splitted[1].toLowerCase();
                    var third = splitted[2];

                    valueIfNotExist = second == 'true';
                    ignoreTrSelector = third;
                }

                if (ignoreTrSelector != null) {
                    trSelector = trSelector + String.format(':not(0)', ignoreTrSelector);
                }

                var table = $(tableSelector);
                if (table.length == 0) {
                    return valueIfNotExist;
                }
                var allTrs = table.find(trSelector);
                allTrs = allTrs.filter(function () {
                    var tr = $(this);
                    return tr.find('> td.dataTables_empty').length == 0;
                });

                return allTrs.length > 0;
            },
            itemMandatory: function (row, selector) {
                var inputMandatory = row.find('td[data-checklist-col="mandatory"] input[name=isMandatory]');
                if (!inputMandatory.is(':checked')) {
                    return true;
                }
                var inputCompleted = row.find('td[data-checklist-col="completed"] input[name=isCompleted]');
                return inputCompleted.is(':checked');
            },
            dateRequired: function (row, selector) {
                var validate = true;
                var inputCompleted = row.find('td[data-checklist-col="completed"] input[name=isCompleted]');
                var isRequired = inputCompleted.is(':checked');

                if (isRequired) {
                    var inputDateCompleted = row.find('td[data-checklist-col="dateCompleted"] input[name=dateCompleted]');
                    var dateValue = inputDateCompleted.val();
                    var re = eval('/^(((0?[1-9])|([1-2][0-9])|3(0|1)) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) [0-9]{4})$/g');
                    validate = re.test(dateValue);
                }


                return validate;
            },
            dateLessOrEqualToday: function (row, selector) {
                var validate = true;
                var inputDateCompleted = row.find('td[data-checklist-col="dateCompleted"] input[name=dateCompleted]');
                var dateText = inputDateCompleted.val();

                if (dateText != "") {
                    var today = new Date();
                    var dateValue = new Date(dateText);
                    today.setHours(0, 0, 0, 0);
                    dateValue.setHours(0, 0, 0, 0);

                    if (dateValue > today) {
                        validate = false;
                    }
                }

                return validate;
            },

            allCheckFieldTrue: function (row, selector) {
                if (selector == null) {
                    return false;
                }

                var validate = true;
                var splitted = selector.split(';');

                var checkSelector = splitted[0];
                var defaultResult = ((splitted.length >= 2) && (splitted[1].toLowerCase() == 'true'));

                var checkboxList = $(checkSelector);
                if (checkboxList.length == 0) {
                    return defaultResult;
                }

                var selectedCheckbox = checkboxList.filter(function () {
                    var checkbox = $(this);
                    return checkbox.is(':checked');
                });

                return checkboxList.length == selectedCheckbox.length;
            },

            someCheckFieldTrue: function (row, selector) {
                if (selector == null) {
                    return false;
                }

                var validate = true;
                var splitted = selector.split(';');

                var checkSelector = splitted[0];
                var defaultResult = ((splitted.length >= 2) && (splitted[1].toLowerCase() == 'true'));

                var checkboxList = $(checkSelector);
                if (checkboxList.length == 0) {
                    return defaultResult;
                }

                var selectedCheckbox = checkboxList.filter(function () {
                    var checkbox = $(this);
                    return checkbox.is(':checked');
                });

                return selectedCheckbox.length > 0;
            },
            allFieldRequired: function (row, selector) {
                if (selector == null) {
                    return false;
                }

                var validate = true;
                var splitted = selector.split(';');

                var fieldSelector = splitted[0];
                var defaultResult = ((splitted.length >= 2) && (splitted[1].toLowerCase() == 'true'));

                var fieldList = $(fieldSelector);
                if (fieldList.length == 0) {
                    return defaultResult;
                }

                var fillFields = fieldList.filter(function () {
                    var field = $(this);
                    var value = field.val();
                    return (value != null) && (value.trim() != '');
                });

                return fieldList.length == fillFields.length;
            },
            someFieldRequired: function (row, selector) {
                if (selector == null) {
                    return false;
                }

                var validate = true;
                var splitted = selector.split(';');

                var fieldSelector = splitted[0];
                var defaultResult = ((splitted.length >= 2) && (splitted[1].toLowerCase() == 'true'));

                var fieldList = $(fieldSelector);
                if (fieldList.length == 0) {
                    return defaultResult;
                }

                var fillFields = fieldList.filter(function () {
                    var field = $(this);
                    var value = field.val();
                    return (value != null) && (value.trim() != '');
                });

                return fillFields.length > 0;
            },
        };

        CheckListValidator.prototype.containValidator = function (validatorName) {
            return (this.Validations[validatorName] != null) && (typeof (this.Validations[validatorName].Function) == "function");
        }
        CheckListValidator.prototype.containTriggerChangeEvent = function (validatorName) {
            return (this.TriggerChangeEvent[validatorName] != null) && (typeof (this.TriggerChangeEvent[validatorName]) == "function");
        }


        CheckListValidator.prototype.Validations = {};
        CheckListValidator.prototype.ValidationsMessage = {};
        CheckListValidator.prototype.TriggerChangeEvent = {};

        return CheckListValidator;
    })();


    CheckListEvaluator = (function () {

        var __privateFunctions = {
            setMandatoryRow:
                function (row, isMandatory) {
                    var text = '-';
                    if (isMandatory) {
                        text = 'Yes';
                    }

                    var td = row.find('td[data-checklist-col="mandatory"]');
                    td.html('');

                    var input = $('<input>');
                    input.attr('type', 'checkbox');
                    input.attr('name', 'isMandatory');
                    input.prop('checked', isMandatory);
                    input.addClass('hide');
                    input.appendTo(td);

                    var span = $('<span>');
                    span.html(text);
                    span.appendTo(td);
                },

            isMandatoryP:
                function (originalEstimateSelector) {
                    if (originalEstimateSelector == null) {
                        return false;
                    }

                    var originalEstimate = $(originalEstimateSelector);
                    var amount = parseFloat(originalEstimate.val());
                    var isMandatory = ((originalEstimate.length == 1) && !isNaN(amount) && amount > 100000);

                    return isMandatory;
                },
            isMandatoryC:
                function (isConfidentialSelector) {
                    if (isConfidentialSelector == null) {
                        return false;
                    }

                    var isConfidential = $(isConfidentialSelector);
                    var confidential = isConfidential.val() == 'true';
                    var isMandatory = ((isConfidential.length == 1) && !confidential);

                    return isMandatory;
                },
            isMandatoryR:
                function (checkboxUNDBSelector) {
                    if (checkboxUNDBSelector == null) {
                        return false;
                    }

                    var checkedSelector = checkboxUNDBSelector + ':checked';
                    var checkedCheckbox = $(checkedSelector);

                    return checkedCheckbox.length > 0;
                },


            genericChangeEvent: {

                common2Input: function (row, selector) {
                    var splitted = selector.split(';');
                    var inputSelector1 = splitted[0];
                    var inputSelector2 = splitted[0];

                    __privateFunctions.genericChangeEvent.commonInput(row, inputSelector1);
                    __privateFunctions.genericChangeEvent.commonInput(row, inputSelector2);
                },

                commonInput: function (row, selector) {
                    var splitted = selector.split(';');
                    var inputSelector = splitted[0];

                    var table = row.closest('table[data-role="checklist"]');
                    $(document).on('change', inputSelector, function () {
                        table.trigger('checklist-evaluate-mandatary-row', row);
                    })
                },
            },
        };

        function CheckListEvaluator() {
            this.AddEvaluator('ChecklistItemMandatory', this.Evaluators.mandatory);
            this.AddEvaluator('ChecklistItemNoMandatory', this.Evaluators.noMandatory);
            this.AddEvaluator('ChecklistItemMandatoryP', this.Evaluators.mandatoryP, __privateFunctions.genericChangeEvent.commonInput);
            this.AddEvaluator('ChecklistItemMandatoryC', this.Evaluators.mandatoryC, __privateFunctions.genericChangeEvent.commonInput);
            this.AddEvaluator('ChecklistItemMandatoryP|C', this.Evaluators.mandatoryPC, __privateFunctions.genericChangeEvent.common2Input);
            this.AddEvaluator('ChecklistItemMandatoryR', this.Evaluators.mandatoryR, __privateFunctions.genericChangeEvent.commonInput);
            this.AddEvaluator('ChecklistItemMandatoryBAFO', this.Evaluators.mandatoryBAFO, __privateFunctions.genericChangeEvent.commonInput);
        }


        CheckListEvaluator.prototype.AddEvaluator = function (evaluatorName, evaluation, triggerFunction) {
            this.Evaluations[evaluatorName] = evaluation;

            if ((triggerFunction != null) && (typeof (triggerFunction) == 'function')) {
                this.TriggerChangeEvent[evaluatorName] = triggerFunction;
            }
        };

        CheckListEvaluator.prototype.Evaluators = {
            mandatory: function (row, selector) {
                __privateFunctions.setMandatoryRow(row, true);
            },
            noMandatory: function (row, selector) {
                __privateFunctions.setMandatoryRow(row, false);
            },
            mandatoryP: function (row, selector) {
                var isMandatory = __privateFunctions.isMandatoryP(selector);

                __privateFunctions.setMandatoryRow(row, isMandatory);
            },
            mandatoryC: function (row, selector) {
                var isMandatory = __privateFunctions.isMandatoryC(selector);

                __privateFunctions.setMandatoryRow(row, isMandatory);
            },
            mandatoryPC: function (row, selector) {
                var splitted = selector.split(';');
                var originalEstimateSelector = splitted[0];
                var isConfidentialSelector = splitted[1];



                var isMandatory = __privateFunctions.isMandatoryP(originalEstimateSelector)
                               || __privateFunctions.isMandatoryC(isConfidentialSelector);

                __privateFunctions.setMandatoryRow(row, isMandatory);
            },
            mandatoryR: function (row, selector) {
                var isMandatory = __privateFunctions.isMandatoryR(selector);

                __privateFunctions.setMandatoryRow(row, isMandatory);
            },
            mandatoryBAFO: function (row, selector) {
                //jquery acceder al radio button
                //Determinar si esta pulsado yes (true) o no (false) y lo metemos en una variablee isMandatory
                var radioBuffoSelected = $(selector + ':checked');
                var isMandatory = (radioBuffoSelected.length > 0 && radioBuffoSelected.val() == 'True');

                if (!isMandatory) {
                    row.addClass('hide');
                } else {
                    row.removeClass('hide');
                }
                __privateFunctions.setMandatoryRow(row, isMandatory);
            },
        };

        CheckListEvaluator.prototype.Evaluations = {};
        CheckListEvaluator.prototype.TriggerChangeEvent = {};


        CheckListEvaluator.prototype.containEvaluator = function (evaluatorName) {
            return (this.Evaluations[evaluatorName] != null) && (typeof (this.Evaluations[evaluatorName]) == "function");
        }
        CheckListEvaluator.prototype.containTriggerChangeEvent = function (evaluatorName) {
            return (this.TriggerChangeEvent[evaluatorName] != null) && (typeof (this.TriggerChangeEvent[evaluatorName]) == "function");
        }

        return CheckListEvaluator;
    })();

    window.ChecklistValidator = new CheckListValidator();
    window.CheckListEvaluator = new CheckListEvaluator();

}(jQuery));


//Searchable
(function ($) {
    $.fn.Searchable = function () {

        return this.each(function () {

            var div = $(this);
            var inputText = div.find('input[data-type="text"]');
            var inputValue = div.find('[data-type="value"]');
            var searchButton = div.find('button');
            var resultUL = div.find('ul[data-type="result"]');

            if ((div.attr('data-role') != 'searchable') ||
                (inputText.length != 1) ||
                (inputValue.length != 1) ||
                (searchButton.length != 1) ||
                (resultUL.length != 1)) {
                return;
            }

            var data = inputValue.data("Searchable");
            if (data != null) {
                return;
            }

            function addInvalidResult(text) {
                var li = $('<li>');
                li.html(text);
                li.appendTo(resultUL);
            }

            function search() {

                var text = inputText.val();

                var minChar = parseInt(searchButton.attr('data-minchar'));
                var type = searchButton.attr('data-type');
                var loadInfo = searchButton.attr('data-loadinfo');
                var order = searchButton.attr('data-order');

                if (text.length < minChar) {
                    return;
                }

                resultUL.children().remove();
                resultUL.css('min-width', div.css('width'));
                div.addClass('open');

                var list = null;
                if (type == 'list') {
                    list = JSON.parse(loadInfo);
                    list = list.filter(function (item) {
                        return item.Text.toLowerCase().indexOf(text.toLowerCase()) >= 0;
                    });
                } else {
                    var response = postUrl(loadInfo, { filter: text });
                    response.done(function (data) {
                        if (!data.IsValid) {
                            if (data.ErrorMessage == '') {
                                addInvalidResult('Unexpected Error');
                            } else {
                                addInvalidResult(data.ErrorMessage);
                            }
                            return;
                        }
                        list = data.Items;
                    });
                    response.fail(function (res, status, type) {
                        addInvalidResult(type);
                    });
                }

                if (list.length === 0) {
                    addInvalidResult('No Results');
                    return;
                }

                if (order != 'none') {
                    var multiplier = 1;             //order == 'ascending,'
                    if (order == 'descending') {
                        multiplier = -1;
                    }

                     list.sort(function (a, b) {
                        if (a.Text > b.Text) {
                            return 1 * multiplier;
                        } else if (a.Text == b.Text) {
                            return 0
                        }

                        return -1 * multiplier;
                    });
                }

                list.forEach(function (it, ix) {

                    var li = $('<li>');
                    var a = $('<a>');
                    a.attr('dd-value', it.Value)
                    if (it.AdditionalData != null && it.AdditionalData.trim() != '') {
                        a.attr('dd-additional-data', it.AdditionalData);
                    }
                    a.html(it.Text);
                    a.appendTo(li);
                    li.appendTo(resultUL);
                });
            };

            function clickSearch() {
                searchButton.focus();
                searchButton.click()
            }

            var data = {
                Search: clickSearch
            };

            inputValue.data("Searchable", data);

            inputText.off("keydown");
            inputText.on("keydown", function (e) {
                if (e.keyCode === 13) {
                    clickSearch();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    e.preventDefault();
                }
            });

            inputText.off("change");
            inputText.on("change", function (e) {
                inputValue.val('');
                inputValue.trigger('change');
            });

            div.off('click', 'ul[data-type="result"] > li > a');
            div.on('click', 'ul[data-type="result"] > li > a', function (e) {
                var source = $(this);
                markAsSelected(source);
                doSelection();
            });

            searchButton.off("keydown");
            searchButton.on("keydown", function (e) {

                e.stopPropagation();
                e.stopImmediatePropagation();
                e.preventDefault();

                var elements = resultUL.find('a[dd-value]');
                var selected = resultUL.find('a[dd-value][dd-selected]');

                if (elements.length == 0) {
                    return;
                }

                switch (event.keyCode) {
                    case 13:    //Enter
                        doSelection();
                        break;
                    case 40:    //Down
                        if (selected.length == 0) {
                            selected = $(elements[0]);
                        } else {
                            var nextLi = selected.parent().next();
                            if (nextLi.length != 0) {
                                selected = nextLi.find('a');
                            }
                        }
                        markAsSelected(selected);
                        break;
                    case 38:    //Up
                        if (selected.length == 0) {
                            selected = $(elements[elements.length - 1]);
                        } else {
                            var prevLi = selected.parent().prev();
                            if (prevLi.length != 0) {
                                selected = prevLi.find('a');
                            }
                        }
                        markAsSelected(selected);
                        break;
                }
            });

            searchButton.off("click");
            searchButton.on("click", function (e) {
                search();
            });

            $(document).off('click', ':not([data-role="searchable"], [data-role="searchable"] *)', closeResult);
            $(document).on('click', ':not([data-role="searchable"], [data-role="searchable"] *)', closeResult);

            div.off('mouseenter mouseleave', 'ul[data-type="result"] > li > a');
            div.on('mouseenter mouseleave', 'ul[data-type="result"] > li > a', function (e) {
                markAsSelected($(this));
                e.stopPropagation();
                e.stopImmediatePropagation();
                e.preventDefault();
            });

            function markAsSelected(a) {
                div.find('ul[data-type="result"] > li > a').removeAttr('dd-selected');
                a.attr('dd-selected', 'dd-selected');
            }

            function doSelection() {
                var selected = resultUL.find('a[dd-value][dd-selected]');
                if (selected.length == 1) {
                    inputText.val(selected.html().trim());
                    inputValue.val(selected.attr('dd-value'));
                    var additionalData = selected.attr('dd-additional-data');
                    inputValue.trigger('change', additionalData);
                    closeResult();
                }
            }

            function closeResult(e) {
                if (e != null) {
                    var searchablesOpen = $('[data-role="searchable"].open');
                    var numClosed = 0;
                    searchablesOpen.each(function () {
                        var item = $(this);
                        var wasClosed = closeSingle(e, item);
                        if (wasClosed) {
                            numClosed++;
                        }
                    });

                    if ((numClosed != 0) || (searchablesOpen.length > 0)) {
                        e.stopPropagation();
                    }
                } else {
                    closeSingle(null, div);
                }
            }

            function closeSingle(e, searcable) {
                var wasClosed = false;
                if (e != null) {
                    var parent = searcable.parent()[0];
                    var target = e.currentTarget;
                    if (parent != target) {
                        searcable.removeClass('open');
                        wasClosed = true;
                    }
                } else {
                    searcable.removeClass('open');
                    wasClosed = true;
                }

                return wasClosed;
            }

        });

    };
}(jQuery));
