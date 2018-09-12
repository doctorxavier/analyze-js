
function showOverlayControl(source, title, content, cssClass, closeOther, removeContentAtOpen) {
    if (closeOther == true) {
        var otherOverlays = $('.overlay .overlayclose');
        otherOverlays.click();
    }

    if (cssClass == null) {
        cssClass = '';
    }

    var overlayStyle = ' ';
    if (window.parent != window) {
        var scrollPositionTop = window.parent.document.body.scrollTop + 50;
        overlayStyle = ' style="top: ' + scrollPositionTop + 'px;" ';
    }
    var template = $('<div class="overlay text-left ' + cssClass + '"> <div class="overlayTitle bg-primary"> <h3></h3></div> <div class="overlayContent"></div> <div class="overlayclose"></div> </div>');

    var overlayTemplate = $('<div class="overlay text-left ' + cssClass + '">');
    var overlayTitle = $('<div class="overlayTitle bg-primary"> <h3></h3><div class="overlayclose"></div></div>');
    var overlayContent = $('<div class="overlayContent"></div>');

    overlayTitle.appendTo(overlayTemplate);
    overlayContent.appendTo(overlayTemplate);

    overlayTemplate.find('h3').append(title);
    var modalContentContainer = overlayTemplate.find('.overlayContent');

    var originalModalContent = content;
    var modalContentClone = originalModalContent.clone(false);

    modalContentClone.removeClass('hide');
    modalContentContainer.append(modalContentClone);

    source.attr("disabled", "disabled");
    overlayTemplate.find('.overlayclose').click(function () {
        source.removeAttr("disabled");
        overlayTemplate.fadeOut("slow");
        overlayTemplate.remove();
    });


    if (removeContentAtOpen == true) {
        originalModalContent.html('');
    }

    $("body").append(overlayTemplate.fadeIn(200));

    setPositionModal(overlayTemplate);
}

function showCustomOverlayControl(source, title, content, type, cssClass, closeOther, removeContentAtOpen) {
    //function showCustomOverlayControl(source, title, content, cssClass, closeOther, removeContentAtOpen) {
    if (closeOther == true) {
        var otherOverlays = $('.overlay .overlay-close');
        otherOverlays.click();
    }

    var overlayTemplate = $('<div>');
    overlayTemplate.addClass('overlay');
    if (type != null) {
        overlayTemplate.addClass(type);
    }
    if (cssClass != null) {
        overlayTemplate.addClass(cssClass);
    }

    var overlayTitle = $('<div class="overlay-title"><div class="overlay-close"></div><h3></h3></div>');
    var overlayContent = $('<div class="overlay-content"></div>');

    overlayTitle.appendTo(overlayTemplate);
    overlayContent.appendTo(overlayTemplate);

    overlayTemplate.find('h3').append(title);

    var modalContentClone = content.clone(false);

    modalContentClone.removeClass('hide');
    overlayContent.append(modalContentClone);

    source.attr("disabled", "disabled");
    overlayTemplate.find('.overlay-close').click(function () {
        source.removeAttr("disabled");
        overlayTemplate.fadeOut("slow");
        overlayTemplate.remove();
    });


    if (removeContentAtOpen == true) {
        content.html('');
    }

    $("body").append(overlayTemplate.fadeIn(200));

    setPositionModal(overlayTemplate);
}

function collapseTables(source) {
    if (source.hasClass('activated')) {
        source.parents('article').find('span.collapsibleContainer button.expanded').click();
    }
    else {
        source.parents('article').find('span.collapsibleContainer button').not('span.collapsibleContainer button.expanded').click();
    }
}

function openModal(modalContent, callback, allowClose, theme) {
    showOverlay();
    vex.open({
        content: modalContent,
        afterOpen: function ($vexContent) {
            $vexContent.css('top', Math.max(getParentElement('body').scrollTop() - $vexContent.height() / 2 + 50, 228));
            if ($('button[data-id=performButton]').length > 0) {
                $('button[data-id=performButton]').removeAttr('disabled');
            }
            return $vexContent;
        },
        afterClose: function () {
            hideOverlay();
            if (callback != null) {
                return callback();
            }
            return true;
        },
        showCloseButton: allowClose,
        escapeButtonCloses: allowClose,
        overlayClosesOnClick: allowClose,
        className: 'vex-theme-default ' + theme
    });
}

function openCustomModal(modalContent, callback, allowClose, title, modalStyle, closeOnOverlayClick, buttons, target) {
    target.removeData('custom-button-last-pressed');

    //create main content
    var template = $('<div data-id="mainContainer"> </div>');

    $(template).append(modalContent);

    $(template).prepend('<h3 class="modalTitle"></h3>');

    //apply title
    if (title == null || title == undefined) {
        $(template).find('h3:eq(0)').text('Title');
    }
    else {
        $(template).find('h3:eq(0)').text(title);
    }

    //create container for buttons
    var buttonsContainer = $('<div data-id="modalButtonsContainer" id="modalButtonsContainer" class="vex-modalFooter"></div>');

    //default "Ok" Button action
    var closeModal = function (source, modalId) {
        var canClose = $(source.target).attr('data-canClose');
        if ((canClose == null) || (canClose == 'true')) {
            vex.closeByID(modalId);
        }
    };

    if (buttons != null || buttons != undefined) {
        $(buttonsContainer).html('');

        $.each(buttons, function (index, value) {
            //create button
            if (value.Legend == '' || value.Legend == null || value.Legend == undefined) {
                var button = $(' <button type="button">Button</button> ');
            }
            else {
                var button = $('<button type="button">' + value.Legend + '</button>');
            }

            //add style to button        
            if (value.style == '' || value.style == null || value.style == undefined) {
                button.addClass('btn btn-primary');
            }
            else {
                button.addClass(value.style);
            }

            if (value.Id != null && value.Id != '' && !(value.Id == undefined)) {
                button.data('custom-modal-data-id', value.Id);
            }

            $(buttonsContainer).append(button);
        });
    }

    if ((buttons != null) && (buttons.length > 0)) {
        $(template).append(buttonsContainer);
    }

    showOverlay();

    if (closeOnOverlayClick == null) {
        closeOnOverlayClick = 'true';
    }

    var parameters = {
        content: template,
        afterOpen: function ($vexContent) {
            //        	$vexContent.css('top', Math.max(getParentElement('body').scrollTop() - $vexContent.height() / 2 + 50, 228));
            if ($('button[data-id=performButton]').length > 0) {
                $('button[data-id=performButton]').removeAttr('disabled');
            }
            return $vexContent;
        },
        afterClose: function () {
            hideOverlay();
            var otherVex = $('.vex.vex-theme-default');
            otherVex.css('display', 'block');

            if (callback != null) {
                return callback();
            }
            return true;
        },
        showCloseButton: allowClose,
        escapeButtonCloses: allowClose,
        overlayClosesOnClick: (closeOnOverlayClick != 'false')
    };

    //apply modal style
    if ((modalStyle != null) && (modalStyle != undefined) && (modalStyle != '')) {
        parameters = $.extend(parameters, { className: 'vex-theme-default ' + modalStyle });
    }

    template.find('ul.tabs li:first').click();

    var vexModal = vex.open(parameters);
    setPositionModal(vexModal);

    var vexModalID = vexModal.data().vex.id;

    if (buttons != null) {
        $.each(buttons, function (index, value) {
            var buttonList = vexModal.find('[data-id=modalButtonsContainer] button');


            for (var i = 0; i < buttonList.length; i++) {
                var vexButton = $(buttonList[i]);

                if (vexButton.html() == value.Legend) {
                    //add behavior to button
                    if (value.callback == null || value.callback == undefined) {
                        vexButton.on('click', function (e) { closeModal(e, vexModalID); });
                    }
                    else {
                        if (value.CloseAtEnd === 'true') {
                            vexButton.on('click', function (e) {
                                var funct = value.callback(e);

                                if (funct !== undefined) {
                                    funct.done(function (data) {
                                        closeModal(e, vexModalID);
                                    });
                                }
                            });
                        }
                        else {
                            vexButton.on('click', value.callback);
                        }
                    }
                }
            }
        });
    }

    vexModal.find('.vex-close').each(function () {
        var button = $(this);
        button.bind('click', function () {
            target.data('custom-button-last-pressed', '');
        });
    });

    vexModal.find('[data-id=modalButtonsContainer] button').each(function () {
        var button = $(this);

        button.bind('click', function () {
            target.data('custom-button-last-pressed', $(this).data('custom-modal-data-id'));
        });
    });
}

function openNewModal(modalContent, callback, allowClose, title, modalType, closeOnOverlayClick, buttons, target, redirectOnCloseTo, useNewRedirect) {
    target.removeAttr("data-new-modal-button-last-pressed");
    var pressedBy = target;
    if (pressedBy.data('pressed-by') != null) {
        pressedBy = pressedBy.data('pressed-by');
    }

    //create main content
    var template = $('<div data-id="mainContainer"><div class="new-modal-content"></div> </div>');

    if (modalContent != null) {
        modalContent.data('shownBy', pressedBy);
        $(template).find(".new-modal-content").append(modalContent);
    }
    var titleContainer = $('<h3 class="new-modal-title"><span class="new-modal-text"></span></h3>');

    $(template).prepend(titleContainer);

    //apply title
    if (title != null) {
        titleContainer.find('.new-modal-text').text(title);
    }

    //create container for buttons
    var buttonsContainer = $('<div data-id="modalButtonsContainer" class="new-modal-buttons-container"></div>');

    //default "Ok" Button action
    var closeModal = function (source, modalId) {
        var canClose = $(source.target).attr("data-canClose");
        if ((canClose == null) || (canClose == "true")) {
            vex.closeByID(modalId);
        }
    };

    if (buttons != null || buttons != undefined) {
        $(buttonsContainer).html('');

        $.each(buttons, function (index, value) {
            //create button
            var button = null;
            if ((value.Legend != null) && (value.Legend != "")) {
                button = $('<button>' + value.Legend + '</button>');
            }
            else {
                button = $(' <button></button> ');
            }

            //add style to button        
            if ((value.Type != null) && (value.Type != "")) {
                button.addClass(value.Type);
            }
            else {
                button.addClass("button01");
            }

            if ((value.Id != null) && (value.Id != "")) {
                button.attr("data-new-modal-button-data-id", value.Id);
            }

            $(buttonsContainer).append(button);

            if (index < buttons.length - 1) {
                $(buttonsContainer).append("&nbsp;&nbsp;");
            }
        });
    }

    if ((buttons != null) && (buttons.length > 0)) {
        $(template).append(buttonsContainer);
    }

    showOverlay();

    if (closeOnOverlayClick == null) {
        closeOnOverlayClick = "true";
    }

    var parameters = {
        content: template,
        afterOpen: function ($vexContent) {
            return $vexContent;
        },
        afterClose: function () {
            hideOverlay();
            var otherVex = $('.vex.vex-theme-default');
            otherVex.css('display', 'block');

            var result = true;
            if (callback != null) {
                result = callback();
            }
            if (redirectOnCloseTo != null) {
                if (useNewRedirect == true) {
                    LocationAssign(redirectOnCloseTo);
                } else {
                    window.location.assign(redirectOnCloseTo);
                }
            }
            return result;
        },
        showCloseButton: allowClose,
        escapeButtonCloses: allowClose,
        overlayClosesOnClick: (closeOnOverlayClick != "false")
    };

    //apply modal style
    if ((modalType != null) && (modalType != "")) {
        parameters = $.extend(parameters, { className: "vex-theme-default " + modalType });
    }

    var vexModal = vex.open(parameters);
    setPositionModal(vexModal);

    var vexModalID = vexModal.data().vex.id;

    if (buttons != null) {
        $.each(buttons, function (index, value) {
            var buttonList = vexModal.find("[data-id=modalButtonsContainer] button");


            for (var i = 0; i < buttonList.length; i++) {
                var vexButton = $(buttonList[i]);

                if (vexButton.html() == value.Legend) {
                    //add behavior to button
                    if (value.Callback == null || value.Callback == undefined) {
                        vexButton.on('click', function (e) { closeModal(e, vexModalID); });
                    }
                    else {
                        if (value.CloseAtEnd === "true") {
                            vexButton.on('click', function (e) {
                                value.Callback(e, pressedBy).done(function (data) {
                                    closeModal(e, vexModalID);
                                });
                            });
                        }
                        else {
                            vexButton.on('click', value.Callback);
                        }
                    }

                }


            }
        });
    }

    vexModal.find(".vex-close").each(function () {
        var button = $(this);
        button.bind("click", function () {
            target.attr("data-new-modal-button-last-pressed", "");
        });
    });

    vexModal.find("[data-id=modalButtonsContainer] button").each(function () {
        var button = $(this);

        button.bind("click", function () {
            target.attr("data-new-modal-button-last-pressed", $(this).attr("data-new-modal-button-data-id"));
        });
    });
    //---------------------Commented because don't allow use showMessage function
    //if (window.parent.openNewModal) {
    //    window.parent.$('body > .vex').remove();
    //}

    return vexModal;
}


function clearComment(source) {
    source.prevAll('textarea').val(null);
}

function updateCommentIcon(source) {
    var container = source.parents('tr').prev().find('[data-PageMode="edit"]').find('[name="' + source.attr('data-relatedField') + '"]').parents('td');

    if (source.val().length > 0) {
        var commentIcon = $('<img/>');
        commentIcon.addClass('commentIcon');

        container.append(commentIcon);
    }
    else {
        container.find('img').remove();
    }
}
