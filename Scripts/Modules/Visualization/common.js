$(document).ready(function () {
    $('.minMaxButton ').click(function () {
        var container = $(this)
            .parents('table')
        .first();
        $(this)
            .parent()
            .children('.expansion-line')
            .toggle();
        var td =
            container
            .children('tbody')
            .children('tr')
            .children('td')
            .get(1);
        $(td)
            .children('.level-container')
            .toggle();
        $(this).toggleClass('minus');
        $(this).toggleClass('plus');

        setTimeout(function () { resizeIframeCloud(); }, 1000);
    });
    $('.show-more').click(function () {
        var element = $(this);
        element.prev().show();
        element.hide();
        setTimeout(function () { resizeIframeCloud(); }, 1000);
    });
    if ($("form").validate) {
        $(".idb-form-validate").validate({
            focusInvalid: true,
            errorClass: 'invalid',
            success: function () {
                
            },
            errorPlacement: function (error, element) {
                error = $(error);
                errorText = error.text();
                if (errorText.indexOf('{name}') != -1 && $(element).data('val-name')) {
                    errorText = error.text().replace('{name}', $(element).data('val-name'));
                }
                $(element).data('val-message', errorText);
                if ($(element).tooltip) {
                    $(document).tooltip({
                        items: ".invalid",
                        content: function () {
                            return $(this).data('val-message');
                        }
                    });
                }
            },
        });
        jQuery.extend(jQuery.validator.messages, {
            required: 'Field "{name}" is required.'
        });
    }
    if ($('input.numeric').numeric) {
        $('input.numeric').numeric();
    }
    $('.collapse-control').click(function () {
        var control = $(this);
        control.toggleClass('collapsed');
        var controls = $('.level-container');
        controls.each(function (index, element) {
            element = $(element);
            if (element.find('.level-container').length > 0 &&
                element.parents('.level-container').length == 0) {
                element.find('.minMaxButton').first().trigger('click');
            }
        });
        setTimeout(function () { resizeIframeCloud(); }, 1000);
    });
    checkDocumentHeight(initContainers);

    setTimeout(function () {
        var minHeight = $(window.parent.document.getElementsByName('someFrame'))
            .contents()
            .height();

        resizeIframeCloud(1, minHeight);
    }, 1000);
});

function urlThumbImages(file) {
    pos = file.lastIndexOf(".");
    var ext = file.substr(pos);
    var res = ext.replace(".", "_");
    file = file.substr(0, pos);
    file = file + res + ".jpg";
    var posBarra = file.lastIndexOf("/");
    file = file.substr(0, posBarra + 1) + "_t" + file.substr(posBarra);
    return file;
}

function checkDocumentHeight(callback) {
    var lastHeight = document.body.clientHeight, newHeight, timer;

    (function run() {
        newHeight = $(".renderHeight").prop("scrollHeight");
        if (lastHeight != newHeight)
            callback();
        lastHeight = newHeight;
        timer = setTimeout(run, 500);
    })();
}
function initContainers() {
    $('.level-container').each(function (index, element) {
        var container = $(element);
        var column = container.find('.first-column').first();
        var tr = $(container
            .children('tbody')
            .children('tr')
            .get(0));
        var td = $(tr.children('td').get(1));
        var childrens = td.children('.level-container:visible');

        if (!childrens.length != 0) {
            return;
        }
        column.children('.minMaxButton').show();
        column.children('.expansion-line').show();
        var offsetFirst = childrens
            .last()
            .position()
            .top;
        var expansionLine = column
            .children('.expansion-line')
            .first();
        offsetFirst -= expansionLine.position().top;
        offsetFirst += 24;
        column
            .children('.expansion-line')
            .css('height', offsetFirst + 'px');
    });
}