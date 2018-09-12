var loadingPanelNew = $('#layoutLoadingDiv');
var loadingPanelNewOptinal = $('#layoutLoadingOptionalDiv');

$(document).ready(function () {
    loadingPanelNew = $('#layoutLoadingDiv');
    loadingPanelNewOptinal = $('#layoutLoadingOptionalDiv');
    bindHandlers();
});

function bindHandlers(container) {
    if (container === null || container === undefined) {
        container = $('body');
    }

    if ($(container).find('table.tableNormal').find('.tree').length > 0) {
        CollapseTables();
    }

    imgFlag();

    $(container).find('div.btn-group').each(function () {
        var graphId = $(this).find('button.graphButton').attr('data-id');
        var valuesId = $(this).find('button.valuesButton').attr('data-id');

        if ($('div[data-id="' + graphId + '"]').is('.hide') || $('div[data-id="' + graphId + '"]').is('.hidden')) {
            $(this).find('[data-id="' + graphId + '"]').removeClass('active');
        } else {
            $(this).find('[data-id="' + graphId + '"]').addClass('active');
        }

        if ($('div[data-id="' + valuesId + '"]').is('.hide') || $('div[data-id="' + valuesId + '"]').is('.hidden')) {
            $(this).find('[data-id="' + valuesId + '"]').removeClass('active');
        } else {
            $(this).find('[data-id="' + valuesId + '"]').addClass('active');
        }
    });

    var tabActive = $(container).find('ul.tabs li.active');
    if (tabActive.length > 0) {
        var tabPane = $('div.tab-pane');
        if (tabPane.length > 0) {
            tabPane.removeClass('active');
            $(tabActive.attr('dd-tab')).addClass('active');
        }
    }

    $(container).find('[data-action]').off('click');
    $(container).find('[data-action]').click(function () {
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

    $(container).find('[data-navigate]').off('click');
    $(container).find('[data-navigate]').click(function () {
        location.href = $(this).attr('data-navigate');
    });

    alternateRowColor(container);

    $(container).find('.inputText').change(function () {
        $(this).attr("value", $(this).val());
    });

    $(container).find('.inputTextarea').blur(function () {
        $(this).text($(this).val());
    });

    $(container).find('.hasDatepicker').removeClass('hasDatepicker');
    container.find('.datepicker-default').datepicker({
        selectOtherMonths: true,
        showOtherMonths: true,
        dateFormat: 'dd M yy',
        changeMonth: true,
        changeYear: true
    });

    container.find('div.dropdown').each(function () {
        if ($(this).GetValue() === "") {
            $(this).addClass('placeholder');
        }
    });

    container.find('.chosen').ChosenMultiDropdown();

    container.find('.radiobutton-default-text').off('click');
    container.find('.radiobutton-default-text').click(function () {
        $(this).closest('.radiobutton-default').find('input').click();
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

    container.find('[data-a-dec], [data-a-sep], [data-v-min], [data-v-max], [data-m-dec]').not('[type="number"]').autoNumeric('init');

    Validation.Init(container);

    hideLoader();
}

function showMessage(message, title) {
    Alert.ShowInfo(message, title);
}

function resizeLoader() {
    if ($('#mainLayoutOperationContent').length > 0) {
        loadingPanelNew.appendTo($('#body.container-fluid'));
        loadingPanelNewOptinal.appendTo($('#body.container-fluid'));

        var position = $('#mainLayoutOperationContent').position();

        loadingPanelNew.css('top', position.top)
            .css('left', position.left)
            .css('position', 'absolute')
            .css('height', $('#mainLayoutOperationContent').height())
            .css('width', $('#mainLayoutOperationContent').width());

        loadingPanelNewOptinal.css('top', position.top)
            .css('left', position.left)
            .css('position', 'absolute')
            .css('height', $('#mainLayoutOperationContent').height())
            .css('width', $('#mainLayoutOperationContent').width());

        $(document)
            .scroll(function () {
                var position = $('#mainLayoutOperationContent').position();

                loadingPanelNew.css('top', position.top)
                    .css('left', position.left)
                    .css('position', 'absolute')
                    .css('height', $('#mainLayoutOperationContent').height())
                    .css('width', $('#mainLayoutOperationContent').width());

                loadingPanelNewOptinal.css('top', position.top)
                    .css('left', position.left)
                    .css('position', 'absolute')
                    .css('height', $('#mainLayoutOperationContent').height())
                    .css('width', $('#mainLayoutOperationContent').width());
            });
    }
}

function showLoader(text) {
    if (loadingPanelNew.length > 0) {
        resizeLoader();
        loadingPanelNew.show();
        if (text !== undefined) {
            var textZone = loadingPanelNew.find('.confluence-loader-text');
            if (textZone.length > 0) {
                textZone.text(text);
            }
        }
    }
}

function showLoaderOptional(text) {
    if (loadingPanelNewOptinal.length > 0) {
        resizeLoader();

        if (loadingPanelNew !== null) {
            loadingPanelNew.hide();
        }

        loadingPanelNewOptinal.show();
        if (text !== undefined) {
            var textZone = loadingPanelNewOptinal.find('.confluence-loader-text');
            if (textZone.length > 0) {
                textZone.text(text);
            }
        }
    }
}

function hideLoader() {
    if (loadingPanelNew !== null) {
        loadingPanelNew.fadeOut('slow');
    }
}

function hideLoaderOptional() {
    if (loadingPanelNewOptinal.length > 0) {
        loadingPanelNewOptinal.fadeOut('slow');
    }
}

function hideLoaderOptionalMain() {
        if (loadingPanelNewOptinal.length > 0) {
            loadingPanelNewOptinal.hide();
        }
}

function triggerAction(source, target) {
    window[target](source);
}