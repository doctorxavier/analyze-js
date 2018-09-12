function initializeApp() {
    $.ajaxSetup({ async: false });

    $(document).ajaxStart(function () {
        if (typeof showLoaderOptional !== 'undefined' && $.isFunction(showLoaderOptional)) {
            showLoaderOptional();
        }
    });

    $(document).ajaxComplete(function () {
        if (typeof hideLoaderOptional !== 'undefined' && $.isFunction(hideLoaderOptional)) {
            hideLoaderOptional();
        }
    });

    //Set focus on vex modal if try focus out
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

        if ((validateForm === "true") && !validateContainer($("[data-parsley-validate]"))) {
            return;
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

                //modalContent.parent().addClass('hide');
                parentToRemove.remove();

                if (fnCustomModalOnClose !== "") {
                    var fnCustomModalOnCloseDelegate = window[fnCustomModalOnClose];
                    if (!(fnCustomModalOnCloseDelegate == undefined)) {
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

    $(document).off('click', '[data-action]');
    $(document).on('click', '[data-action]', function () {
        var $this = $(this);
        $this.blur();
        $this.attr('disabled', 'disabled');
        

        window.setTimeout(function () {
            var hooks = $this.attr('data-action').split(',');

            for (var i = 0; i < hooks.length; i++) {
                triggerAction($this, hooks[i]);
            }
            $this.attr('disabled', null);
            hideLoader();
        }, 10);
    });
}

function triggerAction(source, target) {
    window[target](source);
}

function setPositionModal(modal) {
    var scrollHeader = 0;

    //Check if we are in a IFRAME
    var $frame = $(window.parent.document.getElementById('iframe_id'));
    var modalHeight = 0;
    if (modal !== undefined) {
        modalHeight = modal.height();
    }

    var frameHeight = null;

    var internalScrollTop = 0;
    var frameScrollTop = 0;

    var internalPositionTop = 0;

    if (($frame != null) && ($frame.length > 0)) {
        scrollHeader = $frame.offset().top;
        frameHeight = $("html").height();
        frameScrollTop = window.parent.document.body.scrollTop || window.parent.document.documentElement.scrollTop || window.parent.pageYOffset || 0;
        internalScrollTop = frameScrollTop - scrollHeader;


        internalPositionTop = internalScrollTop + 150;

        if (internalPositionTop < 150) {
            internalPositionTop = 150;
        }

        if ((frameHeight > 0) && (modalHeight + internalPositionTop > frameHeight)) {
            internalPositionTop = frameHeight - modalHeight;
        }

        if (modal !== undefined) {
            modal.css("top", internalPositionTop + "px");
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
    ShowMessageFunctions.GenericFunction(message, reload, reloadUrl, title);
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

function confirmActionInformation(message, okButton, cancelButton) {
    var promise = $.Deferred();
    var modal = vex.dialog.confirm({
        message: message,
        afterOpen: function ($vexContent) {
            $vexContent.prepend($('<h3 class="modalTitle h3-padding new-modal-title"><span class="icon-information new-modal-text" style="#FFF">Information</span></h3>'));
            $vexContent.find('.vex-dialog-form').css({ 'padding': '20px 20px 0 20px' });
            $vexContent.parents('.vex-theme-warning').removeClass('vex-theme-warning');
            $vexContent.find('.vex-dialog-button.vex-last').css('border', 'solid 1px darkgray');
        },
        deleteContent: true,
        callback: function (value) {
            window.setTimeout(function () {
                promise.resolve(value);
            }, 0);
        },
        buttons: [
            $.extend({}, vex.dialog.buttons.YES, { className: 'btn-primary vex-dialog-button vex-first', text: okButton }),
            $.extend({}, vex.dialog.buttons.NO, { className: 'btn-secondary vex-dialog-button vex-last', text: cancelButton })
        ]
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
            $.extend({}, vex.dialog.buttons.NO, {
                className: cancelButton === null ? 'hidden' : 'btn btn-link vex-dialog-button vex-last',
                text: cancelButton
            })
        ]
    });
    setPositionModal(modal);
    return promise;
}