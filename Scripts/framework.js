var loadingPanel = null;
var loadingPanelNew = null;
var loadingPanelNewOptinal = null;
var callbacks = new Array();
var lastCallbacks = new Array();
var executingCallbacks = false;

function initializeApp() {
    loadingPanel = $('#loadingPanel');
    loadingPanelNew = $('#layoutLoadingDiv');
    loadingPanelNewOptinal = $('#layoutLoadingOptionalDiv');

    $.ajaxSetup({ async: false });

    bindDocumentAjaxActions();

    $(document).on("focus", "body", function (e) {
        var hasModal = $("body").hasClass("vex-open");
        if (e.target.tagName != 'BODY') {
            var focusOnModal = $(e.target).parents(".vex").length > 0;
            var focusOnDatePicker = $(e.target).parents(".ui-datepicker").length > 0;

            if (hasModal && !(focusOnModal || focusOnDatePicker)) {
                var focusable = $(".vex").find(":focusable:first");
                focusable.focus();
            }
        }
    });

    $(document).on('click', '[data-searchable]:not([disabled]) ~ [data-searchable-button]', function () {
        var source = $(this);
        var searchable = source.prevAll("[data-searchable]");
        searchable.focus();
        searchable.autocomplete("option", "minLength", 0);
        searchable.autocomplete("search");
        searchable.autocomplete("option", "minLength", Number.MAX_VALUE);
    });


    $(document).off('click', '[data-post]');
    $(document).on('click', '[data-post]', function () {
        postUrl($(this).attr('data-post'));
    });

    $(document).off('click', '[data-navigate]');
    $(document).on('click', '[data-navigate]', function () {
        location.href = $(this).attr('data-navigate');
    });

    $(document).off('click', '[data-blank-link]');
    $(document).on('click', '[data-blank-link]', function () {
        var url = $(this).attr('data-blank-link');
        window.open(url);
    });

    $(document).off('click', '[data-modal]');
    $(document).on('click', '[data-modal]', function () {
        $(this).blur();
        var modalContent = $('#' + $(this).attr('data-modal'));
        modalContent.removeClass('hide');
        openModal(modalContent, function () {
            modalContent.addClass('hide');
        }, $(this).attr('data-modal-allowClose'), $(this).attr('data-modal-theme'));
    });

    $(document).off('click', '[data-custom-modal]');
    $(document).on('click', '[data-custom-modal]', function () {
        $(this).blur();
        var validateForm = $(this).attr('data-custom-modal-validate');

        if ((validateForm === 'true') && !validateContainer($('[data-parsley-validate]'))) {
            return;
        }

        var modalContent = $('#' + $(this).attr('data-custom-modal'));
        modalContent.removeClass('hide');

        var filters = modalContent.find(':input[type="file"],input[type="text"]');
        for (var h = 0; h <= filters.length; h++) {
            $(filters[h]).val('');
        }
        var buttonNamesString = $(this).attr('data-custom-buttons');
        var finalButtonList = null;

        if ((buttonNamesString != null) && (buttonNamesString != '')) {
            var buttonNameList = JSON.parse(buttonNamesString);
            finalButtonList = [];
            for (var i = 0; i < buttonNameList.length; i++) {
                var buttonName = buttonNameList[i];
                var text = $(this).attr('data-custom-button-' + buttonName);
                var btnStyle = $(this).attr('data-custom-button-' + buttonName + '-style');
                var functionName = $(this).attr('data-custom-button-' + buttonName + '-callback');
                var closeAtEnd = $(this).attr('data-custom-button-' + buttonName + '-closeAtEnd');
                var dataId = $(this).attr('data-custom-button-' + buttonName + '-id');

                finalButtonList.push({ Legend: text, style: btnStyle, callback: window[functionName], CloseAtEnd: closeAtEnd, Id: dataId });
            }
        }
        var fnCustomModalOnClose = $(this).data('custom-modal-onclose');

        var $this = $(this);

        openCustomModal(
            modalContent,
            function () {
                modalContent.parent().addClass('hide');
                if (fnCustomModalOnClose !== '') {
                    var fnCustomModalOnCloseDelegate = window[fnCustomModalOnClose];
                    if (!(fnCustomModalOnCloseDelegate == undefined)) {
                        fnCustomModalOnCloseDelegate($this.data('custom-button-last-pressed'));
                    }
                }
            },
            $(this).attr('data-modal-allowClose'),
            $(this).attr('data-custom-modal-title'),
            $(this).attr('data-custom-modal-style'),
            $(this).attr('data-custom-modal-closeOnOverlayClick'),
            finalButtonList,
            $this);
    });

    $(document).on('click', '[data-new-modal]', function (e) {
        if (e.isTrigger == null) {
            $(this).removeData('pressed-by');
        }

        $(this).blur();
        var validateForm = $(this).attr('data-new-modal-validate');
        var moveOriginalContent = $(this).attr('data-new-modal-moveOriginalContent') == "true";
        var parentOriginalContent = null;


        if (validateForm === 'true') {
            if (!validateContainer($("[data-parsley-validate]"))) {
                return;
            }
        } else if (validateForm !== 'false') {
            if (!validateContainer($(validateForm))) {
                return;
            }
        }

        var modalContent = $($(this).attr('data-new-modal'));
        var modalToOpen = null;

        var redirectOnCloseToURL = modalContent.attr('data-new-modal-redirectOnCloseToUrl');
        if (redirectOnCloseToURL == null) {
            redirectOnCloseToURL = $(this).attr('data-new-modal-redirectOnCloseToUrl');
        }

        if (moveOriginalContent) {
            modalToOpen = modalContent;
            parentOriginalContent = modalToOpen.parent();
        } else {
            modalToOpen = modalContent.clone(false);
            bindHandlers(modalToOpen);
        }

        modalToOpen.removeClass('hide');

        var buttonNamesString = $(this).attr('data-new-modal-buttons');
        var finalButtonList = null;

        if ((buttonNamesString != null) && (buttonNamesString != undefined) && (buttonNamesString != "")) {
            var buttonNameList = JSON.parse(buttonNamesString);
            finalButtonList = [];
            for (var i = 0; i < buttonNameList.length; i++) {
                var buttonName = buttonNameList[i];
                var text = $(this).attr('data-new-modal-button-' + buttonName);
                var btnType = $(this).attr('data-new-modal-button-' + buttonName + '-type');
                var functionName = $(this).attr('data-new-modal-button-' + buttonName + '-callback');
                var closeAtEnd = $(this).attr('data-new-modal-button-' + buttonName + '-closeAtEnd');
                var dataId = $(this).attr('data-new-modal-button-' + buttonName + "-id");

                finalButtonList.push({ Legend: text, Type: btnType, Callback: window[functionName], CloseAtEnd: closeAtEnd, Id: dataId });
            }
        }
        var fnCustomModalOnClose = $(this).attr("data-new-modal-onclose");

        var $this = $(this);

        openNewModal(
            modalToOpen,
            function () {
                var parentToRemove = modalToOpen.parent().parent();
                if (moveOriginalContent && parentOriginalContent != null) {
                    parentOriginalContent.append(modalToOpen);
                    modalToOpen.addClass('hide');
                }

                parentToRemove.remove();

                if ((fnCustomModalOnClose != null) && (fnCustomModalOnClose !== "")) {
                    var fnCustomModalOnCloseDelegate = window[fnCustomModalOnClose];
                    if (fnCustomModalOnCloseDelegate == undefined) {
                        fnCustomModalOnCloseDelegate = modalToOpen.data(fnCustomModalOnClose)

                        if (fnCustomModalOnCloseDelegate != undefined) {
                            fnCustomModalOnCloseDelegate($this.attr("data-new-modal-button-last-pressed"));
                        }
                    } else {
                        fnCustomModalOnCloseDelegate($this.attr("data-new-modal-button-last-pressed"));
                    }
                }
            },
            $(this).attr('data-new-modal-allowClose'),
            $(this).attr('data-new-modal-title'),
            $(this).attr('data-new-modal-type'),
            $(this).attr('data-new-modal-closeOnOverlayClick'),
            finalButtonList,
            $this,
            redirectOnCloseToURL);
    });

    $(document).on('click', '[data-overlay]', function () {
        var target = $(this);
        showOverlayControl(target, target.attr('data-overlay-title').split('|'), $(target.attr('data-overlay-selector')));
    });

    $(document).on('click', '[data-custom-overlay]', function () {
        var target = $(this);
        showCustomOverlayControl(target, target.attr('data-custom-overlay-title'), $(target.attr('data-custom-overlay')), target.attr('data-custom-overlay-type'));
    });

    $(document).off('click', '[data-action]');
    $(document).on('click', '[data-action]', function () {
        var $this = $(this);
        $this.blur();
        $this.attr('disabled', 'disabled');
        showLoader();

        window.setTimeout(function () {
            var hooks = $this.attr('data-action').split(',');

            for (var i = 0; i < hooks.length; i++) {
                triggerAction($this, hooks[i]);
            }
            $this.attr('disabled', null);
            hideLoader();
        }, 10);
    });

    $(document).on('change', '[data-change]', function () {
        var $this = $(this);

        window.setTimeout(function () {
            var hooks = $this.attr('data-change').split(',');

            for (var i = 0; i < hooks.length; i++) {
                triggerAction($this, hooks[i]);
            }
        }, 10);
    });

    $(document).on('change', '[data-parsley-trigger="change"]', function () {
        if (typeof ($(this).parsley) === 'function') {
            $(this).parsley().validate();
        }
    });

    $(document).on('click', '[data-btn-expandable] input', function () {

        var source = $(this).parent();
        var btnExpandable = source.data("btn-expandable");
        btnExpandable.Execute();
        btnExpandable.CheckLinkedAndChange();
    });


    $(document).on('click', 'td label[data-btn-expandable-all] input', function () {
        var source = $(this).parent();
        var btnExpandableAll = source.data("btn-expandable-all");
        btnExpandableAll.Execute();
    });

    $(document).ajaxError(function (event, response, request, status) {
        console.error(String.format('Unexpected server error: \r\nUrl = {0} \r\nStatus = {1}', request.url, status));
    });

    $(document).on('click', '[data-tab] li[data-tab-id]:not([data-tab-active=true])', function () {
        var tab = $(this);
        var tabUl = tab.parent();
        var tabsObj = tabUl.data("idbTabs");
        tabsObj.Switch(tab.attr('data-tab-id'));
    });

    $(document).on('click', 'table.table-selectable > tbody > tr', function (sender) {
        var $this = $(this);
        var inputCheck = $this.find('input[data-tableselectable ="true"]');
        if (!($(sender.target).is(':radio'))) {
            inputCheck.click();
        }
    });

    $(document).on('change', 'input[data-tableselectable ="true"]', function (sender) {

        var trContainer = $(sender.target).closest('tr');
        var table = trContainer.closest('table');

        if ($(sender.target).is(':radio')) {
            table.find('tr').removeClass("checked");
            trContainer.addClass("checked");
        } else {
            if (trContainer.hasClass('checked')) {
                trContainer.removeClass('checked');
            } else {
                trContainer.addClass('checked');
            }
        }
    });

    $(document).on('change', '[data-role=dropdown]', function (sender) {
        var item = $(this);
        var kDrop = item.data("KDropDown");
        kDrop.SetStateCSS();
    });


    $(document).on('click', 'a[href^="javascript:ShowTerm"]', function (e) {
        e.preventDefault();
    });

    exitEditMode(false, $('body'), false);
    bindHandlers();
    hideLoader();

    $("html").resize(resizeIframe);
    $("html").resize();
}

function showLoader(text) {
        if (loadingPanelNew.length > 0) {
            setPositionModal(loadingPanelNew.children('div'));
            loadingPanelNew.hide();
            loadingPanelNew.show();
            if (text !== undefined && typeof text == 'string') {
                var textZone = loadingPanelNew.find('.confluence-loader-text');
                if (textZone.length > 0) {
                    textZone.text(text);
                }
            }
        } else {
            loadingPanel.css('display', 'block');
            setPositionModal(loadingPanel.find('#circularG'));
            loadingPanel.hide();
            loadingPanel.show();
            positionLoader();
        }
}

function showLoaderOptional(text) {
        if (loadingPanelNew.length > 0) {
            loadingPanelNew.hide();
        } else {
            loadingPanel.hide();
        }

        loadingPanelNewOptinal.hide();
        if (loadingPanelNewOptinal.length > 0) {
            setPositionModal(loadingPanelNewOptinal.children('div'));
            loadingPanelNewOptinal.show();
            if (text !== undefined) {
                var textZone = loadingPanelNewOptinal.find('.confluence-loader-text');
                if (textZone.length > 0) {
                    textZone.text(text);
                }
            }
        }
}


function positionLoader() {
    //Check if we are in a IFRAME
    var $frame = $(window.parent.document).find('iframe[name=someFrame]');

    if (($frame != null) && ($frame.length > 0)) {
        var frameScrollTop = window.parent.document.body.scrollTop || window.parent.document.documentElement.scrollTop || window.parent.pageYOffset || 0;
        var frameScrollLeft = ($(window.parent).width() / 2) - 207;
        if (frameScrollLeft < 0) {
            frameScrollLeft = 0;
        }
        $('#layoutLoadingDiv').find('img').css('top', frameScrollTop + 'px').css('left', frameScrollLeft + 'px');
        $('#layoutLoadingDiv').find('div').css('top', (frameScrollTop + 42) + 'px').css('left', (frameScrollLeft - 3) + 'px');

        $(window.parent).off('scroll');
        $(window.parent).scroll(function () {
            positionLoader();
        });

        $(window.parent).off('resize');
        $(window.parent).resize(function () {
            positionLoader();
        });
    } else {
        $(window).off('resize');
        $(window).resize(function () {
            positionLoader();
        });
    }
}

function hideLoader() {
        if (loadingPanel !== null) {
            loadingPanel.fadeOut('slow');
        }

        if (loadingPanelNew !== null) {
            loadingPanelNew.fadeOut('slow');
        }

        $(window.parent).off('scroll');
        $(window.parent).off('resize');

        $(window).off('resize');
}

function hideLoaderOptional() {
        if (loadingPanelNewOptinal !== null) {
            loadingPanelNewOptinal.fadeOut('slow');
        }

        $(window.parent).off('scroll');
        $(window.parent).off('resize');

        $(window).off('resize');
}

function bindHandlers(container) {
    if (container == null) {
        container = $('body');
    }

    if ($.fn.dropdown) {
        container.find('.dropdown-toggle').dropdown();
    }

    container.find('[data-role="collapse-single"]').CollapseSingle();
    container.find('[data-role="collapse-all"]').CollapseAll();
    container.find('button.switch').switcherify();
    container.find('.collapsible').collapsible();
    container.find('button.toggleRow').togglerify();
    container.find('[data-tooltip]').dataTooltip();
    container.find('.chosen').ChosenMultiDropdown();
    container.find('.tableNormal td.tree div.number').closest('table.tableNormal').TableNumber();
    container.find('[data-parsley-type]').blur(function () {
        if (typeof ($(this).parsley) === 'function') {
            $(this).parsley().validate();
        }
    });
    container.find('.hasDatepicker').removeClass('hasDatepicker');
    container.find('.datepicker').datepicker(
    {
        selectOtherMonths: true,
        dateFormat: 'dd M yy',
        showOn: 'button',
        buttonImage: '',
        buttonImageOnly: true,
        buttonText: ' ',
        changeMonth: true,
        changeYear: true,
        beforeShow: function (input, options) {
            renderDatePicker(input);

            return options;
        },
        onChangeMonthYear: function (year, month, instance) {
            renderDatePicker(this);
        },
        onClose: function () {
            $(this).removeClass('datepickerOpen').nextAll('img').removeClass('datepickerOpen');;
        }
    }).attr('placeholder', 'dd MMM yyyy').attr('pattern', '^(0|1|2|3)\\d\\s(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)\\s\\d{4}$').attr('maxlength', 11);


    container.find('[data-a-dec], [data-a-sep], [data-v-min], [data-v-max], [data-m-dec]').not('[type="number"]').autoNumeric('init');

    container.find('[data-btn-expandable]').btnExpandable();
    container.find('[data-btn-expandable-all]').btnExpandableAll();

    container.find('.datepicker-default').datepicker({
        selectOtherMonths: true,
        showOtherMonths: true,
        dateFormat: 'dd M yy',
        changeMonth: true,
        changeYear: true
    });

    container.find('[data-tab]').idbTabs();

    container.find('[data-dropdown-cascade]').CascadeDropdown();
    container.find('[data-dropdown-cascade]').each(function (ix, element) {
        var item = $(element);
        var cascadeDropdown = item.data("CascadeDropdown");
        cascadeDropdown.AttachParentChangeEvent();
    });

    container.find('[data-searchable]').each(function (ix, element) {
        var item = $(element);
        var functionName = item.attr("data-searchable");
        var fn = window[functionName];
        if ((fn != null) && (typeof fn === "function")) {
            item.autocomplete(
            {
                minLength: Number.MAX_VALUE,
                source: function (request, response) {
                    var results = fn(item, request.term);
                    if (Array.isArray(results)) {
                        response(results);
                    } else if (typeof results.done === "function") {
                        results.done(function (data) {
                            response(data);
                        });
                    }
                }
            });
        }
    });
    container.find('[data-role=dropdown]').KDropDown();
    container.find('[data-role=dropdown]').each(function (index, element) {
        var item = $(element);
        var dropdown = item.data("KDropDown");
        dropdown.Init();
    });
    if (container.find('.buttonShowRow').length > 0) {
        LinesCollapse(container);
    }

    container.find('div.dropdown').each(function () {
        if ($(this).GetValue() === "") {
            $(this).addClass('placeholder');
        }
    });

    container.find('[data-parent-code]').each(function () {
        var parent = $('[data-persist-code="' + $(this).attr('data-parent-code') + '"]');
        var selectedParent = false;
        var selectedValueParent = null;
        var element = $(this).closest('div.dropdown');
        if (parent.is('[type="radio"]')) {
            parent.each(function () {
                if ($(this).prop('checked')) {
                    selectedParent = true;
                    selectedValueParent = $(this).val();
                }
                $(this).closest('.radiobutton-default').click(function () {
                    atributosDependientes($(this).find('input').val(), element);
                });
            });
        } else {
            if (parent.GetValue().length > 0) {
                selectedParent = true;
                selectedValueParent = parent.GetValue();
            }

            $(parent).closest('div.dropdown').find('li').click(function () {
                atributosDependientes($(this).find('a').attr('dd-value'), element);
            });
        }

        if (selectedParent &&
            element.find('a[dd-parent-id="' + selectedValueParent + '"]').length > 0) {
            element.find('a[dd-parent-id]').closest('li').addClass('hide');
            element.find('a[dd-parent-id="' + selectedValueParent + '"]').closest('li').removeClass('hide');

            if (element.find('li').not('.hide').find('a[dd-value="' + element.GetValue() + '"]').length === 0) {
                $(this).FirstorDefault();
            }
        } else {
            $(this).FirstorDefault();
            element.find('button').attr('disabled', 'disabled');
            element.find('[data-parsley-required]').each(function () {
                $(this).attr('mandatory', 'true').removeAttr("data-parsley-required");
            });
            element.find('a[dd-parent-id]').closest('li').addClass('hide');
        }
    });

    container.find('.datepicker-default[dd-min-date]').change(function () {
        if ($(this).val().length > 0) {
            var fechaActual = converToDate($(this).val());
            var fechaTope = converToDate($(this).attr('dd-min-date'));

            if (fechaActual < fechaTope) {
                $('#parsley-id-' + $(this).attr('data-parsley-id')).find('li').remove();

                var message = '<li class="parsley-date-min">Please select a date greater than or equal of today.</li>';
                var customMessage = $(this).data('min-date-message');
                if (typeof(customMessage) !== typeof (undefined)) {
                    message = '<li class="parsley-date-min">' + customMessage + '</li>';
                }
                $('#parsley-id-' + $(this).attr('data-parsley-id')).addClass('filled').append(message);
                $(this).val('');
            } else {
                $('#parsley-id-' + $(this).attr('data-parsley-id')).removeClass('filled').find('.parsley-date-min').remove();
            }
        }
    });

    container.find('.datepicker-default[dd-less-date]').change(function () {
        if ($(this).val().length > 0) {
            var fechatope;
            var puedoEjecutar = true;
            if ($('[name="' + $(this).attr('dd-less-date') + '"').length > 1) {
                fechatope = converToDate($(this).closest('tr').find('[name="' + $(this).attr('dd-less-date') + '"'));
            } else if ($('[name="' + $(this).attr('dd-less-date') + '"').length === 1) {
                fechatope = converToDate($('[name="' + $(this).attr('dd-less-date') + '"').val());
            } else {
                puedoEjecutar = false;
            }
            var fechaActual = converToDate($(this).val());

            if (puedoEjecutar) {
                if (fechatope) {
                    if (fechaActual > fechatope) {
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).find('li').remove();
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).addClass('filled').append('<li class="parsley-date-less">Please select a date less than or equal of parent date field.</li>');
                        $(this).val('');
                    } else {
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).removeClass('filled').find('.parsley-date-less').remove();
                    }
                }
            } else {
                $(this).val('');
                showMessage("Wrong name in less than");
            }
        }
    });

    container.find('.datepicker-default[dd-greater-date]').change(function () {
        if ($(this).val().length > 0) {
            var fechatope;
            var puedoEjecutar = true;
            if ($('[name="' + $(this).attr('dd-greater-date') + '"').length > 1) {
                fechatope = converToDate($(this).closest('tr').find('[name="' + $(this).attr('dd-greater-date') + '"'));
            } else if ($('[name="' + $(this).attr('dd-greater-date') + '"').length === 1) {
                fechatope = converToDate($('[name="' + $(this).attr('dd-greater-date') + '"').val());
            } else {
                puedoEjecutar = false;
            }
            var fechaActual = converToDate($(this).val());

            if (puedoEjecutar) {
                if (fechatope) {
                    if (fechaActual < fechatope) {
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).find('li').remove();
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).addClass('filled').append('<li class="parsley-date-greater">Please select a date greater than or equal of parent date field.</li>');
                        $(this).val('');
                    } else {
                        $('#parsley-id-' + $(this).attr('data-parsley-id')).removeClass('filled').find('.parsley-date-greater').remove();
                    }
                }
            } else {
                $(this).val('');
                showMessage("Wrong name in greater than");
            }
        }
    });

    container.find('.radiobutton-default-text').off('click');
    container.find('.radiobutton-default-text').click(function () {
        $(this).closest('.radiobutton-default').find('input').click();
    });

    container.find('.datepicker-default[dd-max-date]').change(function () {
        if ($(this).val().length > 0) {
            var fechaActual = converToDate($(this).val());
            var fechaTope = converToDate($(this).attr('dd-max-date'));

            if (fechaActual > fechaTope) {
                $('#parsley-id-' + $(this).attr('data-parsley-id')).find('li').remove();
                $('#parsley-id-' + $(this).attr('data-parsley-id')).addClass('filled').append('<li class="parsley-date-max">Please select a date less than or equal of today.</li>');
                $(this).val('');
            } else {
                $('#parsley-id-' + $(this).attr('data-parsley-id')).removeClass('filled').find('.parsley-date-max').remove();
            }
        }
    });

    container.find('.buttonShowRow.active').each(function () {
        var tr = $(this).closest('tr');
        if (!tr.is('.hide')) {
            var dataId = tr.attr('data-id');
            if (container.find('[data-row-parent-id="' + dataId + '"]').is('.hide')) {
                container.find('[data-row-parent-id="' + dataId + '"]').removeClass('hide');
            }
        }
    });

    container.find('input.inputText[type="number"]').keydown(function (e) {
        if (e.key === "e" || e.key === "." || e.key === ",") {
            e.preventDefault();
        }
    });

    container.find('[data-role=drop-multiple][data-bind=true]').CustomChosen();

    updateRepeaterFields(container);
    alternateRowColor(container);

    container.find('.inputText').change(function () {
        $(this).attr("value", $(this).val());
    });

    container.find('.inputTextarea').blur(function () {
        $(this).text($(this).val());
    });

    container.find('[data-role="searchable"]').Searchable();

    container.find('[data-toggle="tooltip"]').tooltip();

    initReziseCloud();

    runCallbacks();
}

function atributosDependientes(value, elemento) {
    elemento.find('button').removeAttr('disabled');
    elemento.find('[mandatory]').each(function () {
        $(this).attr('data-parsley-required', 'true').removeAttr("mandatory");
    });

    elemento.find('a[dd-parent-id]').closest('li').addClass('hide');
    elemento.find('a[dd-parent-id="' + value + '"]').closest('li').removeClass('hide');

    if (elemento.find('[dd-parent-id]').closest('li').not('.hide').length === 0) {
        elemento.find('button').attr('disabled', 'disabled');
        elemento.find('[data-parsley-required]').each(function () {
            $(this).attr('mandatory', 'true').removeAttr("data-parsley-required");
        });
        elemento.find('.filled').removeClass('filled');

        elemento.FirstorDefault();
        resetSonAttribute(elemento);
    }
}

function resetSonAttribute(element) {
    if (element.GetValue().length === 0) {
        var son = $('[data-parent-code="' + $(element).find('[data-persist-code]').attr('data-persist-code') + '"]');
        son.each(function () {
            $(this).FirstorDefault();
            var element = $(this).closest('div.dropdown');
            element.find('button').attr('disabled', 'disabled');
            element.find('[data-parsley-required]').each(function () {
                $(this).attr('mandatory', 'true').removeAttr("data-parsley-required");
            });
            element.find('a[dd-parent-id]').closest('li').addClass('hide');
            element.find('.filled').removeClass('filled');
            resetSonAttribute(element);
        });
    }
}

function renderDatePicker(input) {
    $(input).addClass('datepickerOpen').nextAll('img').addClass('datepickerOpen');

    window.setTimeout(function () {
        var datepickerContainer = $('<div class="datepickerContainer"></div>');
        var datepickerFooter = $('<div class="datepickerFooter"></div>');
        datepickerFooter.text(moment().format('dddd, MMM DD, YYYY'));
        datepickerContainer.append($('.ui-datepicker-header'));
        datepickerContainer.append($('.ui-datepicker-calendar'));
        datepickerContainer.append(datepickerFooter);

        datepickerFooter.click(function () {
            $('.datepicker').datepicker('setDate', new Date());
            $('.datepicker').datepicker('hide');
        });

        $('#ui-datepicker-div').append(datepickerContainer);
    }, 20);
}

function registerCallback(callbackFunction, last) {
    if (last) {
        lastCallbacks.push(callbackFunction);
    }
    else {
        callbacks.push(callbackFunction);
    }
}

function runCallbacks() {
    if (!executingCallbacks) {
        callbacks.reverse();
        lastCallbacks.reverse();
    }
    executingCallbacks = true;
    while (callbacks.length > 0) {
        callbacks.pop()();
    }

    while (lastCallbacks.length > 0) {
        lastCallbacks.pop()();
    }
    executingCallbacks = false;
}

function postUrl(url, data) {
    return postUrlWithOptions(url, null, data);
}

function postUrlWithOptions(url, options, data) {
    return $.ajax(url, $.extend(options, { data: data, type: 'POST' }));
}

function serializeContainer(container, includeDisabled, extraData) {
    var serializedData = container.find('input, select').serializeArray();
    var postData = {};

    $.each(serializedData, function (index, value) {
        if (postData[value.name] != null) {
            postData[value.name] = postData[value.name] + ',' + value.value;
        }
        else {
            postData[value.name] = value.value;
        }
    });

    if (includeDisabled) {
        var controls = container.find('input:disabled, select:disabled');

        $.each(controls, function (index) {
            var control = controls[index];
            if (postData[control.name] != null) {
                postData[control.name] = postData[control.name] + ',' + $(control).val();
            }
            else {
                postData[control.name] = $(control).val();
            }
        });
    }

    if (extraData != null) {
        $.each(extraData, function (name, value) {
            if (postData[name] != null) {
                postData[name] = postData[name] + ',' + value;
            }
            else {
                postData[name] = value;
            }
        });
    }

    return postData;
}

function validateContainer(container) {
    if (container.attr('data-parsley-validate') != null) {
        container.data('is-fired-validation', true);
    }

    var result = container.parsley().validate();

    $(container).find('ul.filled').each(function () {
        if ($(this).find('li.parsley-required').length > 1) {
            $(this).find('li.parsley-required').first().remove();
        }
    });

    $(container).find('.richContainer[data-parsley-required="true"]').each(function () {
        if ($(this).find('input').val().length === 0) {
            result = false;
            $(this).addClass('parsley-error');
            $(this).next('ul.errors-list').addClass('filled parsley-errors-list');
            if ($(this).next('ul.errors-list').find('li.parsley-required').length === 0) {
                $(this).next('ul.errors-list').append('<li class="parsley-required">This value is required.</li>');
            }
        }
    });

    $(container).find('input.inputText[data-min]').each(function () {
        if (!$(this).is('[disabled]')) {
            if ($(this).val() !== "" && $(this).val().replace(/\,/g, '') * 1 < $(this).attr('data-min') * 1) {
                result = false;
                $(this).next('ul.parsley-errors-list').addClass('filled');
                if ($(this).next('ul.parsley-errors-list').find('li.parsley-data-min').length === 0) {
                    $(this).next('ul.parsley-errors-list').append('<li class="parsley-data-min">This value is lower than allowed.</li>');
                }
            }
            $(this).MinValue();
        }
    });

    $(container).find('input.inputText[data-max]').each(function () {
        if (!$(this).is('[disabled]')) {
            if ($(this).val() !== "" && $(this).val().replace(/\,/g, '') * 1 > $(this).attr('data-max') * 1) {
                result = false;
                $(this).next('ul.parsley-errors-list').addClass('filled');
                if ($(this).next('ul.parsley-errors-list').find('li.parsley-data-max').length === 0) {
                    $(this).next('ul.parsley-errors-list').append('<li class="parsley-data-max">This value is greater than allowed.</li>');
                }
            }
            $(this).MaxValue();
        }
    });

    $(container).find('input.datepicker-default').change(function () {
        if (!$(this).is('[disabled]')) {
            if ($(this).val().length > 0 && $(this).val().match($(this).attr('data-parsley-pattern')).length > 0) {
                $(this).next('ul.parsley-errors-list').removeClass('filled');
                $(this).removeClass('parsley-error');
            }
        }
    });

    $(container).find('.parsley-error').each(function () {
        if ($(this).closest('.hide').length > 0) {
            if (!$(this).closest('.hide').is('.parsley-error')) {
                $(this).closest('.hide').removeClass('hide');
            }
        }

        if ($(this).closest('.hidden').length > 0) {
            if (!$(this).closest('.hidden').is('.parsley-error')) {
                $(this).closest('.hidden').removeClass('hidden');
            }
        }

        if ($(this).closest('[style*="display: none;"]').length > 0 && !$(this).is('select')) {
            if (!$(this).closest('[style*="display: none;"]').is('.dont-hide-at-edit')) {
                if ($(this).closest('[style*="display: none;"]').is('[data-row-parent-id]')) {
                    $(this).closest('[style*="display: none;"]').css('display', 'table-row');
                } else {
                    $(this).closest('[style*="display: none;"]').css('display', 'block');
                }

            }
        }

    });

    $(container).find('.boxlanguage').find('.parsley-error').each(function () {
        var lang = $(this).closest('div.contentboxlanguage').attr('dd-lang');
        $(this).closest('div.boxlanguage').find('li[dd-lang="' + lang + '"]').addClass('error');
        $(this).closest('div.boxlanguage').find('div.contentboxlanguage').addClass('hide');
        $(this).closest('div.boxlanguage').find('.glyphicon').removeClass('glyphicon-play');
        $(this).closest('div.boxlanguage').find('li[dd-lang="en"]').find('span.glyphicon').addClass('glyphicon-play');
        $(this).closest('div.boxlanguage').find('div.contentboxlanguage[dd-lang="en"]').removeClass('hide');

        $(this).focusout(function () {
            var lang = $(this).closest('div.contentboxlanguage').attr('dd-lang');
            if ($(this).closest('[dd-lang]').find('.parsley-error').length > 0) {
                $(this).closest('div.boxlanguage').find('li[dd-lang="' + lang + '"]').addClass('error');
            } else {
                $(this).closest('div.boxlanguage').find('li[dd-lang="' + lang + '"]').removeClass('error');
            }
        });
    });

    if ($(container).find('ul.parsley-errors-list.filled[id]').length > 0) {
        var id = $(container).find('ul.parsley-errors-list.filled[id]').first().attr('id');
        id = id.replace('parsley-id-', '');
        $(container).find('[data-parsley-id="' + id + '"]').before('<a name="ParsleyFocus">&nbsp;</a>');
        window.location.href = "#ParsleyFocus";
        $('[name="ParsleyFocus"]').remove();
    }

    customValidations(container, result);

    return result;
}

function customValidations(container, generalResult) {
    if ($(container).attr('data-custom-validate') === 'true' && !generalResult) {
        showPopUpMandatoryFields(container);
    }
}

function showPopUpMandatoryFields(container) {
    var fieldsError = container.find('ul.parsley-errors-list.filled').closest('div').children();
    var mandatoryFieldsError = '';
    var errorMessage = container.find('#modalMandatory').val() + '<br>';

    fieldsError.each(function (index, element) {
        var newItem = element.getAttribute('data-label');
        if (newItem !== null && newItem !== "" && mandatoryFieldsError.search('- ' + newItem) === -1) {
            mandatoryFieldsError += '<br> - ' + newItem;
        }
    });

    var mandatoryFieldsErrorMessage = errorMessage + mandatoryFieldsError;

    confirmAction(mandatoryFieldsErrorMessage);

    $('input.btn.btn-link.vex-dialog-button.vex-last').remove();
}

$(function ($) {
    $.fn.MaxValue = function () {
        $(this).change(function () {
            if ($(this).val() !== "" && $(this).val().replace(/\,/g, '') * 1 > $(this).attr('data-max') * 1) {
                $(this).next('ul.parsley-errors-list').addClass('filled');
                if ($(this).next('ul.parsley-errors-list').find('li.parsley-data-max').length === 0) {
                    $(this).next('ul.parsley-errors-list').append('<li class="parsley-data-max">This value is greater than allowed.</li>');
                }
            } else {
                $(this).next('ul.parsley-errors-list').find('li.parsley-data-max').remove();
                if ($(this).next('ul.parsley-errors-list').find('li').length === 0) {
                    $(this).next('ul.parsley-errors-list').removeClass('filled');
                }
            }
        });
        return $(this);
    }
    $.fn.MinValue = function () {
        $(this).change(function () {
            if ($(this).val() !== "" && $(this).val().replace(/\,/g, '') * 1 < $(this).attr('data-min') * 1) {
                $(this).next('ul.parsley-errors-list').addClass('filled');
                if ($(this).next('ul.parsley-errors-list').find('li.parsley-data-min').length === 0) {
                    $(this).next('ul.parsley-errors-list').append('<li class="parsley-data-min">This value is lower than allowed.</li>');
                }
            } else {
                $(this).next('ul.parsley-errors-list').find('li.parsley-data-min').remove();
                if ($(this).next('ul.parsley-errors-list').find('li').length === 0) {
                    $(this).next('ul.parsley-errors-list').removeClass('filled');
                }
            }
        });
        return $(this);
    }
}(jQuery));

function validateInputList() {
    var result = true;
    if ((arguments != null) && (arguments.length > 0)) {
        $.each(arguments, function () {
            var validateResult = this.parsley().validate();
            result = (validateResult == true) || (validateResult.length == 0);
            return result;
        });
    }
    return result;
}

function triggerAction(source, target) {
    window[target](source);
}

function loadHtml(destination, url, parameters, success) {
    destination.load(url, parameters, function (response, status, xhr) {
        if (typeof (success) === 'function') {
            success(response, status, xhr);
        }
    });
}

function showAjaxMessage(data) {
    if (data.Reload && data.ErrorMessage == '') {
        if (data.ReloadUrl != null && data.ReloadUrl != '') {
            location.href = data.ReloadUrl;
        }
        else {
            location.href = location.href;
        }
    }
    else {
        showMessage(data.ErrorMessage, data.Reload, data.ReloadUrl);
    }
}

function setPositionModal(modal) {
    var paddingTopVex = 0;
    var frameTopPosition = 0;
    var $frame = window.parent.jQuery('iframe:eq(0)')
        .not('[data-viewer="true"]')
        .not('.iframeAutoResize');
    var popupHeight = 0;
    if (modal !== undefined) {
        popupHeight = modal.height();
    }

    var windowHeight = $(window.parent).height();
    var windowHeightForFrame = windowHeight;
    var frameHeight = null;

    var frameScrollTop = 0;
    var windowScrollTop = 0;

    var popupTopPosition = 0;

    if (($frame != null) && ($frame.length > 0)) {
        frameTopPosition = $frame.offset().top;
        frameHeight = $("html").height();
        windowScrollTop = window.parent.document.body.scrollTop || window.parent.document.documentElement.scrollTop || window.parent.pageYOffset || 0;
        frameScrollTop = windowScrollTop - frameTopPosition;

        windowHeightForFrame = windowHeight - frameTopPosition + windowScrollTop;
        if (windowHeightForFrame > windowHeight) {
            windowHeightForFrame = windowHeight;
        }

        if (frameScrollTop + windowHeightForFrame > frameHeight) {
            windowHeightForFrame = windowHeightForFrame - (frameScrollTop + windowHeightForFrame - frameHeight);
        }

        if (frameScrollTop < 0) {
            frameScrollTop = 0;
        }

        popupTopPosition = frameScrollTop + ((windowHeightForFrame - popupHeight) / 2) - paddingTopVex;

        if (popupHeight > windowHeightForFrame) {
            popupTopPosition = frameScrollTop - paddingTopVex;
        }

        if ((frameHeight > 0) && (popupHeight + popupTopPosition > frameHeight)) {
            popupTopPosition = frameHeight - popupHeight - paddingTopVex;
        }


        if (popupTopPosition < -paddingTopVex) {
            popupTopPosition = -paddingTopVex;
        }

        if (modal !== undefined) {
            modal.css("top", popupTopPosition + "px");
        }
    }

    modalMove("h3.h3-padding.modalWarning");
    modalMove("h3.new-modal-title");
}


function modalMove(selector) {
    $(selector).off('mousedown');
    $(selector).mousedown(function (e) {

        var sel = window.getSelection ? window.getSelection() : document.selection;
        if (sel) {
            if (sel.removeAllRanges) {
                sel.removeAllRanges();
            } else if (sel.empty) {
                sel.empty();
            }
        }

        var relativeX = e.offsetX;
        var relativeY = e.offsetY;
        var width = $(this).closest('.vex-content').width();

        $(this).closest('.vex-content')
            .addClass('goMove')
            .width(width)
            .attr('tamX', relativeX)
            .attr('tamY', relativeY);

        $('.vex').off('mousemove');
        $('.vex').mousemove(function (data) {
            var goMove = $(this).find('.goMove');
            var pageX = data.clientX;
            var pageY = data.clientY;
            var intX = goMove.attr('tamX');
            var intY = goMove.attr('tamY');
            if (data.buttons === 1) {
                $(this).find('.goMove').css('left', (pageX - intX) + 'px').css('top', (pageY - intY) + 'px');
            }
        });
    }).mouseup(function () {
        $('.vex').off('mousemove');
    });
}

function showMessage(message, reload, reloadUrl, title) {
    var promise = $.Deferred();
    ShowMessageFunctions.CurrentFunction(message, reload, reloadUrl, title, promise);
    return promise;
}

function showMessageWithReloadParentIframeAbsolute(message, realoadUrl) {
    ShowMessageFunctions.FunctionWithReloadParentAbsolute(message, realoadUrl);
}

function confirmAction(message) {
    var promise = $.Deferred();
    var modal = vex.dialog.confirm({
        message: message,
        afterOpen: function ($vexContent) {
            $vexContent.prepend($('<h3 class="h3-padding modalWarning">Warning</h3>'));
        },
        deleteContent: true,
        callback: function (value) {
            window.setTimeout(function () {
                promise.resolve(value);
            }, 0);
        }
    });
    setPositionModal(modal);
    return promise;
}

function confirmActionCustom(message, cancelButton, okButton) {
    var promise = $.Deferred();
    var modal = vex.dialog.confirm({
        message: message,
        afterOpen: function ($vexContent) {
            $vexContent.prepend($('<h3 class="h3-padding modalWarning">Warning</h3>'));
        },
        deleteContent: true,
        callback: function (value) {
            window.setTimeout(function () {
                promise.resolve(value);
            }, 0);
        },
        buttons: [
            $.extend({}, vex.dialog.buttons.YES, { text: okButton }),
            $.extend({}, vex.dialog.buttons.NO, { text: cancelButton })
        ]
    });
    setPositionModal(modal);
    return promise;
}

function initParsley(container) {
    if (container == null) {
        container = $('[data-parsley-validate]');
    }

    container.each(function () {

        var item = $(this);
        if (item.attr('data-parsley-validate')) {
            item.data('is-fired-validation', false);
            var parsleyObj = item.parsley(
            {
                errorsContainer: function (parsleyField) {
                    var field = parsleyField.$element;
                    var tagName = field[0].tagName;
                    if (tagName == "INPUT") {
                        if (field.attr("type") == "radio") {
                            if (field.attr("data-persist-questionid")) {
                                return field.parent().parent().parent();
                            }
                            return field.parent().parent();
                        }
                        else if (field.attr("type") == "text") {
                            if (field.attr('data-dropdown-type') == 'searchcombo') {
                                return field.parent().parent();
                            } else if (field.attr('data-type') == 'text') {
                                return field.parent().parent();
                            }
                        }
                    } else if (tagName == "SELECT" && field.parent().hasClass('k-dropdown')) {
                        return field.parent().parent();
                    }
                    else if (tagName == "SELECT" && field.attr('multiple')) {
                        return field.parent();
                    }
                },
                excluded: 'input[type=button], input[type=submit], input[type=reset], input[type=hidden], [data-validation-ignored], [disabled]'
            });
        }
    });
}

function destroyParsley(container) {
    if (container == null) {
        container = $('[data-parsley-validate]');
    }

    container.each(function () {
        var item = $(this);
        if (item.attr('data-parsley-validate')) {
            var parsleyObj = item.parsley();
            parsleyObj.destroy();
        }
    });

    container.find('.parsley-errors-list').remove();
}



function initParsleyField(fields) {
    if (fields == null) {
        return;
    }

    fields.each(function () {
        var iField = $(this);
        var tagName = iField.prop('tagName');

        if ((tagName == 'INPUT') || (tagName == 'SELECT') || (tagName == 'TEXTAREA')) {
            var isDisabled = iField.attr('disabled') != null;
            if (isDisabled) {
                iField.removeAttr('disabled');
            }

            var parsleyObj = iField.parsley(
            {
                errorsContainer: function (parsleyField) {
                    var field = parsleyField.$element;
                    var tagName = field[0].tagName;
                    if (tagName == "INPUT") {
                        if (field.attr("type") == "radio") {
                            if (field.attr("data-persist-questionid")) {
                                return field.parent().parent().parent();
                            }
                            return field.parent().parent();
                        }
                        else if (field.attr("type") == "text") {
                            if (field.attr('data-dropdown-type') == 'searchcombo') {
                                return field.parent().parent();
                            } else if (field.attr('data-type') == 'text') {
                                return field.parent().parent();
                            }
                        }
                    } else if (tagName == "SELECT" && field.parent().hasClass('k-dropdown')) {
                        return field.parent().parent();
                    }
                    else if (tagName == "SELECT" && field.attr('multiple')) {
                        return field.parent();
                    }
                },
                excluded: 'input[type=button], input[type=submit], input[type=reset], input[type=hidden], [data-validation-ignored], [disabled]'
            });

            if (isDisabled) {
                iField.attr('disabled', 'disabled');
            }
        }
    });
}

function destroyParsleyField(fields) {
    if (fields == null) {
        return;
    }

    fields.each(function () {
        var field = $(this);
        var tagName = field.prop('tagName');

        if ((tagName == 'INPUT') || (tagName == 'SELECT') || (tagName == 'TEXTAREA')) {
            var parsleyId = field.attr('data-parsley-id');
            
            if (parsleyId != null) {
                var isDisabled = field.attr('disabled') != null;
                if (isDisabled) {
                    field.removeAttr('disabled');
                }

                var parsleyObj = field.parsley();
                parsleyObj.destroy();

                var ulError = $('#parsley-id-' + parsleyId);
                ulError.remove();

                if (isDisabled) {
                    field.attr('disabled', 'disabled');
                }
            }
        }
    });
}



function exitEditModeLoader(reload, container, rebind, cancelEdition) {

    showLoader();
    var mode = 'read';
    if (cancelEdition == true) {
        mode = 'cancel';
    }

    if (reload) {
        var obj = { async: true }
        if (!reloadContainer(container, rebind, mode, obj)) {
            return;
        }
    }

    container.each(function (ix, item) {
        var $item = $(item);

        if ($item.attr('data-PageMode') === "edit") {
            $item.addClass('hide');
        }
        else if ($item.attr('data-PageMode') === "read") {
            $item.removeClass('hide');
        }
    });

    container.find('[data-PageMode="edit"]').not('.hide').addClass('hide').removeClass('showDataEdit');
    container.find('[data-PageMode="read"]').removeClass('hide');
    container.find('button.toggleRow.expanded').removeClass('expanded');
}

function exitEditMode(reload, container, rebind, cancelEdition) {
    var mode = 'read';
    if (cancelEdition == true) {
        mode = 'cancel';
    }

    if (reload) {
        if (!reloadContainer(container, rebind, mode)) {
            return;
        }
    }

    container.each(function (ix, item) {
        var $item = $(item);

        if ($item.attr('data-PageMode') === "edit") {
            $item.addClass('hide');
        }
        else if ($item.attr('data-PageMode') === "read") {
            $item.removeClass('hide');
        }
    });

    container.find('[data-PageMode="edit"]').not('.hide').addClass('hide').removeClass('showDataEdit');
    container.find('[data-PageMode="read"]').removeClass('hide');
    container.find('button.toggleRow.expanded').removeClass('expanded');
    container.find('.richContainer').addClass('noborder');
}

function reloadReadMode() {
    var container = $(document);

    container.each(function (ix, item) {
        var $item = $(item);

        if ($item.attr('data-PageMode') === "edit") {
            $item.addClass('hide');
        }
        else if ($item.attr('data-PageMode') === "read") {
            $item.removeClass('hide');
        }
    });

    container.find('[data-PageMode="edit"]').not('.hide').addClass('hide');
    container.find('[data-PageMode="read"]').removeClass('hide');
    container.find('.richContainer').addClass('noborder');
}

function enterEditMode(reload, container, rebind, aditionalData, resetParsley) {
    if (reload) {
        if (!reloadContainer(container, rebind, 'edit', aditionalData)) {
            return false;
        }
    }

    if (container.attr('data-PageMode') === 'edit') {
        container.removeClass('hide');
    }
    else if (container.attr('data-PageMode') === 'read') {
        container.addClass('hide');
    }

    container.find('[data-PageMode="edit"]').removeClass('hide').addClass('showDataEdit');
    container.find('[data-PageMode="read"]').addClass('hide');
    container.find('.template:not(.dont-hide-at-edit)').addClass('hide');
    container.find('.richContainer').removeClass('noborder');
    if (BIDConfig.CreateValidatorOnEdit && (resetParsley == null || resetParsley === true)) {
        destroyParsley(container);
        initParsley(container);
        destroyParsley(container);
        initParsley(container);
    }

    return true;
}

function isInEditMode(container) {
    return container.find('[data-PageMode="read"]').hasClass('hide');
}

function reloadContainer(container, rebind, accessMode, parameters) {
    if ((accessMode != undefined) && (accessMode != '')) {
        parameters = $.extend(parameters, { accessType: accessMode });
    }

    var reloadOk = true;

    postUrl(container.attr('data-loadUrl'), parameters)
        .done(function (responseText, textStatus, jqXHR) {
            if (responseText.ErrorMessage == undefined) {
                if (responseText.ContentHTML == undefined) {
                    container.html(responseText);
                } else {
                    container.html(responseText.ContentHTML);
                }
                reloadOk = jqXHR.status == 200;
            }
            else {

                showMessage(responseText.ErrorMessage);
                reloadOk = false;
                if (responseText.IsValid != null) {
                    reloadOk = responseText.IsValid && jqXHR.status == 200;
                }
                if (responseText.ContentHTML != undefined) {
                    container.html(responseText.ContentHTML);
                    reloadOk = jqXHR.status == 200;
                }
            }
        })
        .fail(function (jqXHR) {
            reloadOk = jqXHR.status == 200;
        });

    if (reloadOk) {
        if (rebind === true) {
            bindHandlers(container);
        }
    }

    return reloadOk;
}

function replaceCharactersInvalid(input) {
    var re = new RegExp('=', 'g');
    input = input.replace(re, '&#61;');
}

function encodeInvalidCharacters(input) {
    if (input == null || input.length == 0) {
        return input;
    }

    var re = new RegExp('=', 'g');
    input = input.replace(re, '#61;');

    re = new RegExp('&', 'g');
    input = input.replace(re, '#38;');

    re = new RegExp('<', 'g');
    input = input.replace(re, '#187;');

    re = new RegExp('>', 'g');
    input = input.replace(re, '#188;');

    input = input.replace(/\?/g, '#189;');

    input = input.replace(/\+/g, '#43;');

    return input;
}

function saveContainer(
    container,
    hiddenSelector,
    rebind,
    buttonsBar,
    source,
    reload,
    isJsonResponse,
    extraContainer,
    isAsync) {
    if (!validateContainer(container)) {
        return false;
    }
    if (reload == null) {
        reload = true;
    }

    var options = {};
    if ((isJsonResponse == null) || isJsonResponse) {
        options.dataType = 'json';
    }

    if (isAsync === true) {
        options.async = true;
    }

    var clientData = [];
    processContainer(container, clientData);
    if (extraContainer != null) {
        processContainer(extraContainer, clientData);
    }

    var jsonText = JSON.stringify({ ClientFieldData: clientData, SerializedData: container.find(hiddenSelector).val() });
    container.append('<div class="hide" id="serializedDataContainer"></div>');
    $('#serializedDataContainer').text(jsonText);
    var promise = postUrlWithOptions(container.attr('data-url'), options, jsonText);
    $('#serializedDataContainer').remove();

    promise.done(function (data) {
        if (source != null) {
            source.attr('data-canClose', (data.IsValid == null) || data.IsValid);
        }
        if ((data.IsValid == null) || data.IsValid) {
            if (data.UrlRedirect != null) {
                reload = false;
                rebind = false;
            }

            exitEditMode(reload, container, rebind);
            if (buttonsBar != null) {
                exitEditMode(false, buttonsBar, false, false);
            }

            if ((data.UrlRedirect != null) && (data.ContentHTML == null) && (data.ContentObject == null)) {

                if ((data.RedirectMessage != null) && (data.RedirectMessage != '')) {

                    var content = $('<div>');
                    content.addClass('modalBody');
                    content.html(data.RedirectMessage);

                    openNewModal(
                        content,            //modalContent
                        function () {       //callback
                            var parentToRemove = content.parent().parent();
                            parentToRemove.remove();
                        },
                        true,               //allowClose
                        data.RedirectTitle, //title
                        'information',      //modalType
                        true,               //closeOnOverlayClick
                        null,               //buttons
                        $('<button>'),      //target
                        data.UrlRedirect,   //redirectOnCloseTo
                        true);              //useNewRedirect

                    //openNewModal(
                    //    modalToOpen,
                    //    function () {
                    //        var parentToRemove = modalToOpen.parent().parent();
                    //        if (moveOriginalContent && parentOriginalContent != null) {
                    //            parentOriginalContent.append(modalToOpen);
                    //            modalToOpen.addClass('hide');
                    //        }

                    //        //modalContent.parent().addClass('hide');
                    //        parentToRemove.remove();

                    //        if ((fnCustomModalOnClose != null) && (fnCustomModalOnClose !== "")) {
                    //            var fnCustomModalOnCloseDelegate = window[fnCustomModalOnClose];
                    //            if (fnCustomModalOnCloseDelegate == undefined) {
                    //                fnCustomModalOnCloseDelegate = modalToOpen.data(fnCustomModalOnClose)

                    //                if (fnCustomModalOnCloseDelegate != undefined) {
                    //                    fnCustomModalOnCloseDelegate($this.attr("data-new-modal-button-last-pressed"));
                    //                }
                    //            } else {
                    //                fnCustomModalOnCloseDelegate($this.attr("data-new-modal-button-last-pressed"));
                    //            }
                    //        }
                    //    },
                    //    $(this).attr('data-new-modal-allowClose'),
                    //    $(this).attr('data-new-modal-title'),
                    //    $(this).attr('data-new-modal-type'),
                    //    $(this).attr('data-new-modal-closeOnOverlayClick'),
                    //    finalButtonList,
                    //    $this,
                    //    redirectOnCloseToURL);
                } else {
                    window.location.assign(data.UrlRedirect);
                }
            }
        }
    });

    return promise;
}

function processContainer(container, clientData) {
    var attrIgnoreNull = container.attr('data-ignore-nullable-values');
    var ignoreNull = attrIgnoreNull === 'true';
    container.find('[data-PageMode="edit"]').find('input[type="hidden"], input[type="text"], input[type="number"], input[type="email"], input[type="radio"]:checked, input[type="checkbox"], select, textarea').each(function () {
        var field = $(this);
        if ((!ignoreNull || ((field.val() != null) && (field.val() != ''))) && (field.parents('[data-repeater-template]').length == 0) && (field.attr('name') != null)) {
            var fieldData = {};
            fieldData.Name = field.attr('name');
            if (Array.isArray(field.val())) {
                fieldData.Value = field.val().join(',');
            }
            else if (field.is('input[type=checkbox]')) {
                fieldData.Value = field.is(':checked');
            }
            else {
                fieldData.Value = encodeInvalidCharacters(field.val());
            }
            fieldData.Id = field.attr('data-id');

            if (fieldData.Id == null || fieldData.Id == '') {
                fieldData.Id = field.parents('tr').attr('data-id');
            }

            fieldData.ExtraData = {};
            for (var attr, i = 0, attributes = field[0].attributes, length = attributes.length; i < length; i++) {
                attr = attributes[i];
                if (attr.name.substr(0, 12) === 'data-persist') {
                    fieldData.ExtraData[attr.name] = attr.value;
                }
            }
            clientData.push(fieldData);
        }
    });
}

function extractFieldsFromComplexArray(array, fields) {
    function getInternalType(element) {
        var result = null;
        switch (typeof element) {
            case "string":
                result = "field";
                break;
            case "object":
                if ((typeof element.Field === "string") && (typeof element.Rename === "string")) {
                    result = "rename";
                }
                break;
        }
        return result;
    }

    function convertField(field, type, conversion) {
        var result = {};
        if (type === "simple") {
            result[conversion] = field[conversion];
        } else if (type == "simpleRename") {
            result[conversion.Rename] = field[conversion.Field];
        } else {
            conversion.forEach(function (item, index, list) {
                var typeItem = getTypeConversion(item);
                var propertyConversed = convertField(field, typeItem, item);
                result = $.extend(result, propertyConversed);
            });
        }
        return result;
    }

    function getTypeConversion(conversion) {
        var typeConversion = null;
        var type = getInternalType(conversion);
        if (type == null) {

            if (Array.isArray(conversion) && (conversion.length > 0)) {

                var allCorrect = true;
                var i = 0;
                while (i < conversion.length && allCorrect) {
                    var element = conversion[i];
                    var internalType = getInternalType(element);

                    if (internalType == null) {
                        allCorrect = false;
                    }
                    i++;
                }

                if (allCorrect) {
                    typeConversion = "multiple";
                }
            }

        } else if (type === "field") {
            typeConversion = "simple";
        } else if (type === "rename") {
            typeConversion = "simpleRename";
        }
        return typeConversion;
    }

    var result = [];
    var typeConversion = getTypeConversion(fields);


    if (typeConversion == null) {
        return null;
    }


    array.forEach(function (item, index, list) {
        var itemConversed = convertField(item, typeConversion, fields);
        result.push(itemConversed);
    });
    return result;
}

function reloadContainerAsync(container, rebind, accessMode, parameters) {
    if ((accessMode != undefined) && (accessMode != '')) {
        parameters = $.extend(parameters, { accessType: accessMode });
    }

    var reloadOk = true;

    postUrlWithOptions(container.attr('data-loadUrl'), { async: true }, parameters)
        .done(function (responseText, textStatus, jqXHR) {
            if (responseText.ErrorMessage == undefined) {
                if (responseText.ContentHTML == undefined) {
                    container.html(responseText);
                } else {
                    container.html(responseText.ContentHTML);
                }
                reloadOk = jqXHR.status == 200;
            }
            else {

                showMessage(responseText.ErrorMessage);
                reloadOk = false;
                if (responseText.IsValid != null) {
                    reloadOk = responseText.IsValid && jqXHR.status == 200;
                }
                if (responseText.ContentHTML != undefined) {
                    container.html(responseText.ContentHTML);
                    reloadOk = jqXHR.status == 200;
                }
            }
        })
        .fail(function (jqXHR) {
            reloadOk = jqXHR.status == 200;
        });

    if (reloadOk) {
        if (rebind === true) {
            bindHandlers(container);
        }
    }

    return reloadOk;
}

function GetFilterForSearchableTable(originalFilter) {
    var filterToUpload = {};
    for (var name in originalFilter) {
        if (name.startsWith("no-filter-")) {
            var value = originalFilter[name];
            if ((typeof value === "object") && !Array.isArray(value)) {
                filterToUpload = $.extend(filterToUpload, value);
            } else {
                var newName = name.substr(10);
                filterToUpload[newName] = value;
            }
        } else {
            var newName = "custom-filter-" + name;
            var value = originalFilter[name];
            filterToUpload[newName] = value;
        }
    }
    return filterToUpload;
}

function LocationAssign(url) {
    showLoader();
    window.location.assign(url);
}

function confirmActionRM(message) {
    var promise = $.Deferred();
    var modal = vex.dialog.confirm({
        message: message,
        afterOpen: function ($vexContent) {
            $vexContent.prepend($('<h3 class="h3-padding modalWarning">Warning</h3>'));
        },
        deleteContent: true,
        callback: function (value) {
            window.setTimeout(function () {
                promise.resolve(value);
            }, 0)

            if (value) {
                idbg.lockUiRM(null, true);
            }
        }
    });
    setPositionModal(modal);
    return promise;
}

function bindDocumentAjaxActions() {
    $(document).on('ajaxStart', showLoader);
    $(document).on('ajaxComplete', hideLoader);
}

function unbindDocumentAjaxActions() {
    $(document).off('ajaxStart', showLoader);
    $(document).off('ajaxComplete', hideLoader);
}