var richContent;
var prevInnerContent;
$(document).on('focusin', '.cke_dialog_ui_input_text', function () { return false; });

var BIDConfig = {
    CreateValidatorOnEdit: false
};


var CustomChart =
{
    paletteColors: {
        type: 'customColors',
        customColors: ['#2f69b9', '#ffa92d', '#703593', '#a7dc38', '#007283', '#08fcbf', '#002052', '#ffacc0', '#c92b22', '#f6eb3b']
    }
};

var ShowMessageFunctions = {
    CurrentFunction: function (message, reload, reloadUrl, title, promise) {
        if ($(document).attr('suppressMessages') === 'true') {
            return;
        }
        if (message != null && message !== '') {
            if (title === undefined || title === "") {
                title = "Information";
            }
            var modalContent = $('<div data-showMessage="GenericFunction">' + message + '</div>');
            var modal = openNewModal(
                modalContent,
                function (value) {
                    modalContent.parent().parent().remove();
                    if (reload === true) {
                        if (reloadUrl != null && reloadUrl !== '') {
                            location.href = reloadUrl;
                        }
                        else {
                            location.href = location.href;
                        }
                    }
                    window.setTimeout(function () {
                        promise.resolve(value);
                    }, 0);
                },
                true,
                title,
                'information',
                true,
                null,
                $('')
            );
            setPositionModal(modal);
        }
    },
    RichFunction: function (content) {
        richContent = content;
        buildBoxModal(content, true);

        var modalContent = $('.richModal').find(".new-modal-content");

        var contentPagemode = content.find('[data-pagemode]').not('.hide');

        if (contentPagemode.length == 0 || contentPagemode.attr('data-pagemode').toLowerCase() !== "read") {
            CKEDITOR.config.defaultLanguage = 'en';
            CKEDITOR.replace('htmlTextArea', {
                language: 'en'
            });

            CKEDITOR.instances['htmlTextArea'].on("change", function (event) {
                var totalCount = richCount();
                var allowedCount = parseInt($(content).attr('data-max'));
                $('#mycounter1').html(allowedCount - totalCount);
            });

            var totalCount = richCount();
            var allowedCount = parseInt($(content).attr('data-max'));
            $('.richModal').find("#preview").after('<p><b id="mycounter1">' + (allowedCount - totalCount) + '</b> characters remaining</p>');

            $(modalContent).find('.btn-primary').off('click');
            $(modalContent).find('.btn-primary').click(function () {

                if (parseInt($('.richModal').find('#mycounter1').text()) * 1 >= 0) {

                    var dataName = $('.richModal').find('input[name="name"]').val();

                    if ($(document).find('.richContainer[data-name="' + dataName + '"]').length > 1) {
                        richSave(richContent);
                    } else {
                        richSave($('[data-name="' + dataName + '"]'));
                    }
                    $('.richModal').closest('.vex-content').find('.vex-close').click();

                } else {
                    showMessage($('.richModal').find('input[name="msg"]').val());
                }

            })
        } else {
            buildBoxModalReadMode(modalContent, content);
        }
    },
    PCRFunction: function (message, reload, reloadUrl) {
        if ($(document).attr('suppressMessages') === 'true') {
            return;
        }

        if (message != null && message !== '') {
            var modal = vex.dialog.open({
                message: message,
                buttons: [],
                afterOpen: function ($vexContent) {
                    $vexContent.addClass('vex-message');
                },
                deleteContent: true,
                afterClose: function () {
                    $('.vex-dialog-form').remove();
                    if (reload === true) {
                        if (reloadUrl != null && reloadUrl !== '') {
                            location.href = reloadUrl;
                        }
                        else {
                            location.href = location.href;
                        }
                    }
                }
            });
            setPositionModal(modal);
        }
    },
    GenericFunction: function (message, reload, reloadUrl, title, promise) {
        if ($(document).attr('suppressMessages') === 'true') {
            return;
        }
        if (message != null && message !== '') {
            if (title === undefined || title === "") {
                title = "Information";
            }
            var modalContent = $('<div data-showMessage="GenericFunction">' + message + '</div>');
            var modal = openNewModal(
                modalContent,
                function (value) {
                    modalContent.parent().parent().remove();
                    if (reload === true) {
                        if (reloadUrl != null && reloadUrl !== '') {
                            location.href = reloadUrl;
                        }
                        else {
                            location.href = location.href;
                        }
                    }
                    window.setTimeout(function () {
                        promise.resolve(value);
                    }, 0);
                },
                true,
                title,
                'information',
                true,
                null,
                $('')
            );
            setPositionModal(modal);
        }
    },
    FunctionWithReloadParentAbsolute: function (message, reloadUrl) {
        if ($(document).attr('suppressMessages') === 'true') {
            return;
        }
        if (message != null && message !== '') {
            var modalContent = $('<div>' + message + '</div>');
            var modal = openNewModal(
                modalContent,
                function () {
                    modalContent.parent().parent().remove();
                    window.top.location.href = reloadUrl;
                },
                true,
                null,
                'information',
                true,
                null,
                $('')
            );
            setPositionModal(modal);
        }
    },
    SaveActualHTML: function (content) {
        prevInnerContent = content.html();
    },
    LoadPrevHtml: function (content) {
        content.html(prevInnerContent);
    },
    TextAreaFunction: function (content, readonly) {
        var auxContent = content;
        buildBoxModal(content, false);

        var boxEditable = $('.richModal').find('[name="htmlTextArea"]');
        boxEditable.addClass('form-control text-area-box');

        var contentPagemode = content.find('[data-pagemode]').not('.hide');
        var textAreaModal = $('.richModal').find(".new-modal-content");
        var readOnly = content.attr('data-readonly') == 'true';

        if ( contentPagemode.length == 0 ||
            (contentPagemode.attr('data-pagemode').toLowerCase() == "edit" && !readOnly)) {
            buildTextAreaEditMode(textAreaModal, content, auxContent);
        } else {
            buildBoxModalReadMode(textAreaModal, content);
        }
    }
};


function richCount() {
    var innerHTML = CKEDITOR.instances['htmlTextArea'].getData();
    var wrapper = $('<div />').html(innerHTML);
    var wrapperText = wrapper.text();
    return wrapperText.trim().replace(/\s+/g, ' ').length;
}

function richSave(content) {

    ShowMessageFunctions.LoadPrevHtml(content);

    var contents = CKEDITOR.instances['htmlTextArea'].getData();

    if (contents.length > 0) {
        contents = contents.replace(/\\/g, '&#92;');
        contents = contents.replace(/“/g, '"');
        content.attr('data-placehoder-active', 'false');

        modalToBox(content, contents);
    } else {
        content.find('input[type=hidden]').val('');
        content.attr('data-placehoder-active', 'true');
        content.find('div[data-pagemode]').html(content.attr('data-placeholder'));
    }

    ShowMessageFunctions.SaveActualHTML(content);
}

function modalToBox(content, contentText) {

    content.find('input[type=hidden]').val(contentText);
    content.find('div[data-pagemode]').html(contentText);

    if (content.is('.parsley-error')) {
        content.removeClass('parsley-error');
        content.next('ul.errors-list').removeClass('filled').html('');
    }
}

function boxContentSave(content, contentText) {
    ShowMessageFunctions.LoadPrevHtml(content);
    
    modalToBox(content, contentText);

    ShowMessageFunctions.SaveActualHTML(content);
}

function buildBoxModal(content, showModalInfo) {
    var modal = openNewModal(
        null,
        function () {
            $('.richModal').remove();
            ShowMessageFunctions.LoadPrevHtml(content);
        },
        true,
        $(content).attr('data-title'),
        'information',
        true,
        null,
        $(''));

    modal.find('[data-id="mainContainer"]').addClass('richModal');
    setPositionModal(modal);

    var modalContent = $('.richModal').find(".new-modal-content");

    if (showModalInfo) {
        var headerFillContent = $('#fr-header-fillinformation').html();
        modalContent.html(headerFillContent ? headerFillContent : "");
    }

    modalContent.append('<input name="name" type="hidden" value="' + $(content).attr('data-name') + '">');
    modalContent.append('<input name="msg" type="hidden" value="' + $(content).attr('data-error') + '">');
    modalContent.append('<form><textarea name="htmlTextArea" id="htmlTextArea"></textarea></form>');

    modalContent.find('#htmlTextArea').val(content.find('input[type=hidden]').val());

    modalContent.append('<div id="preview"></div>');
    modalContent.append('<div class="vex-modalFooter"></div>');
    modalContent = modalContent.find('.vex-modalFooter');
    modalContent.append('<button type="button" class="btn btn-primary" class="ok-button">' + $(content).attr('data-ok') + '</button>');
    modalContent.append('<button type="button" class="btn btn-link">' + $(content).attr('data-cancel') + '</button>');

    ShowMessageFunctions.SaveActualHTML(content);

    $(modalContent).find('.btn-link').off('click');
    $(modalContent).find('.btn-link').click(function () {
        $('.richModal').closest('.vex-content').find('.vex-close').click();
    });
}

function buildBoxModalReadMode(modalContent, content) {
    $(modalContent).find('.btn-primary').hide();
    $('#htmlTextArea').hide();
    $('#preview').css({ 'overflow-y': 'auto', 'height': 200 });
    $('.richModal').find("#preview")
        .addClass("inputTextarea")
        .html(content.find('input[type=hidden]').val());
    $(modalContent).find('.btn-link').text($(content).attr('data-close'));
    $(content).addClass("noborder");
}

function buildTextAreaEditMode(textAreaModal, content, auxContent) {
    var allowedCount = parseInt($(content).attr('data-max'));

    var htmlTextArea = textAreaModal.find('#htmlTextArea');

    htmlTextArea.bind('input propertychange', function (event) {
        var totalCount = $(this).val().length;
        $('#mycounter1').html(allowedCount - totalCount);
    });

    var remaining = allowedCount - htmlTextArea.val().length;

    textAreaModal.find("#preview")
        .after('<p><b id="mycounter1">' + remaining + '</b> ' + $(content).attr('data-remaining') + '</p>');

    textAreaModal.find('.btn-primary').off('click');
    textAreaModal.find('.btn-primary').click(function () {
        if (parseInt(textAreaModal.find('#mycounter1').text()) * 1 >= 0) {
            var dataName = $('.richModal').find('input[name="name"]').val();

            boxContentSave(auxContent, htmlTextArea.val());
            textAreaModal.closest('.vex-content').find('.vex-close').click();
        } else {
            showMessage(textAreaModal.find('input[name="msg"]').val());
        }
    });
}