function tabs(element) {
    if (!$(element).is('.disabled')) {
        $(element).closest('ul.tabs').find('li').removeClass('active');
        $(element).addClass('active');
        $(element).closest('ul.tabs').find('li').each(function () {
            $($(this).attr('dd-tab')).removeClass('active');
        });
        $($(element).attr('dd-tab')).addClass('active');
    }
}

function imgFlag() {
    if ($('.img-flag').length > 0) {
        $('.img-flag').hover(function () {
            $(this).addClass('transition');

        }, function () {
            $(this).removeClass('transition');
        });

        $('.img-flag').each(function () {
            if (typeof $(this).attr("dd-width") !== 'undefined') {
                $(this).css("width", $(this).attr("dd-width") + "px");
            }

            if (typeof $(this).attr("dd-height") !== 'undefined') {
                $(this).css("height", $(this).attr("dd-height") + "px");
            }
        });
    }
}

function info(element) {
    var contenedor = $(element).closest('div.informationBox-container');
    contenedor.find('.informationBox-background').addClass("active");
    contenedor.find('.informationBox').addClass("active");

    contenedor.find('.informationBox-background').click(function () {
        contenedor.find('.informationBox-background').removeClass("active");
        contenedor.find('.informationBox').removeClass("active");
    });

    contenedor.find('.informationBox-close').click(function () {
        contenedor.find('.informationBox-background').removeClass("active");
        contenedor.find('.informationBox').removeClass("active");
    });
}

function errorBar(msg, duration, autoClose) {
    if (duration === undefined || isNaN(duration) || duration * 1 < 1) {
        duration = 3;
    } else {
        duration *= 1;
    }

    if (autoClose !== false) {
        autoClose = true;
    }

    showNotification({ message: msg, type: 'error', autoClose: autoClose, duration: duration });
}

function successBar(msg, duration, autoClose) {
    if (duration === undefined || isNaN(duration) || duration * 1 < 1) {
        duration = 3;
    } else {
        duration *= 1;
    }

    if (autoClose !== false) {
        autoClose = true;
    }
    showNotification({ message: msg, type: 'success', autoClose: autoClose, duration: duration });
}

function infoBar(msg, duration, autoClose) {
    if (duration === undefined || isNaN(duration) || duration * 1 < 1) {
        duration = 3;
    } else {
        duration *= 1;
    }

    if (autoClose !== false) {
        autoClose = true;
    }
    showNotification({ message: msg, type: 'information', autoClose: autoClose, duration: duration });
}

function warningBar(msg, duration, autoClose) {
    if (duration === undefined || isNaN(duration) || duration * 1 < 1) {
        duration = 3;
    } else {
        duration *= 1;
    }

    if (autoClose !== false) {
        autoClose = true;
    }
    showNotification({ message: msg, type: 'warning', autoClose: autoClose, duration: duration });
}

function multiLenguague(element) {
    $(element).closest('.boxlanguage').find('.glyphicon-play').removeClass('glyphicon-play');
    $(element).closest('.boxlanguage').find('.contentboxlanguage').addClass('hide');
    var lang = $(element).attr('dd-lang');
    $(element).closest('.boxlanguage').find('div[dd-lang="' + lang + '"]').removeClass('hide');
    $(element).find('.glyphicon').addClass('glyphicon-play');
}

var Alert = new function () {
    Base = function (message, title, showIcon, type) {
        var titleModal = "", typeModal = "", iconContent = "", iconModal = "", useTitle = true;
        if (title === undefined || title === "") {
            useTitle = false;
        }

        switch (type) {
            default:
            case "Info":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.INFORMATION") : title;
                typeModal = "infoModal";
                iconModal = "fa-info-circle";
                break;
            case "Success":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.SUCCESSFUL") : title;
                typeModal = "successModal";
                iconModal = "fa-check";
                break;
            case "Warning":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.WARNING") : title;
                typeModal = "warningModal";
                iconModal = "fa-exclamation-triangle";
                break;
            case "Error":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.ERROR") : title;
                typeModal = "errorModal";
                iconModal = "fa-times";
                break;
        }

        if (showIcon === true) {
            iconContent = '<i class="fa ' + iconModal + '" aria-hidden="true"></i>';
        }

        var tempId = "modalAlert-" + Number(new Date());
        $('body').append(
            '<div class="modal fade popup" id="' + tempId + '" tabindex="-1" role="dialog">' +
                '<div class="modal-dialog ' + typeModal + '">' +
                    '<div class="modal-content hide">' +
                        '<div class="modal-header">' +
                            '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><i class="fa fa-times" aria-hidden="true"></i></button>' +
                            '<h3 class="modal-title">' + iconContent + titleModal + '</h3>' +
                        '</div>' +
                        '<div class="modal-body"> ' + message + '</div>' +
                    '</div>' +
                '</div>' +
            '</div>');

        var modal = $('#' + tempId).modal();
        modal.find('.modal-body').html(message);
        modal.find('.close').click(function () {
            setTimeout(function (modal) {
                $('#' + modal).remove();
            }, 500, tempId);
        });
    };

    ShowTitleIconChildren = function (showIcon) {
        if (showIcon !== false) {
            return true;
        }

        return false;
    };

    this.ShowInfo = function (message, title, showIcon) {
        Base(message, title, ShowTitleIconChildren(showIcon), "Info");
    };

    this.ShowSuccess = function (message, title, showIcon) {
        Base(message, title, ShowTitleIconChildren(showIcon), "Success");
    };

    this.ShowWarning = function (message, title, showIcon) {
        Base(message, title, ShowTitleIconChildren(showIcon), "Warning");
    };

    this.ShowError = function (message, title, showIcon) {
        Base(message, title, ShowTitleIconChildren(showIcon), "Error");
    };
};

var Confirm = new function () {
    Base = function (message, title, showIcon, type, defer, buttonOk, buttonCancel, displayingButtons) {
        var titleModal = "", typeModal = "", iconContent = "", iconModal = "";
        var useTitle = true, modalButtonOk = "", modalButtonCancel = "";
        if (title === undefined || title === "") {
            useTitle = false;
        }

        switch (type) {
            default:
            case "Info":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.INFORMATION") : title;
                typeModal = "infoModal";
                iconModal = "fa-info-circle";
                break;
            case "Success":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.SUCCESSFUL") : title;
                typeModal = "successModal";
                iconModal = "fa-check";
                break;
            case "Warning":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.WARNING") : title;
                typeModal = "warningModal";
                iconModal = "fa-exclamation-triangle";
                break;
            case "Error":
                titleModal = !useTitle ? Translate.Get("GLOBAL.CONTROL.ERROR") : title;
                typeModal = "errorModal";
                iconModal = "fa-times";
                break;
        }

        modalButtonOk = buttonOk === undefined || buttonOk === "" ? Translate.Get("GLOBAL.CONTROL.OK") : buttonOk;
        modalButtonCancel = buttonCancel === undefined || buttonCancel === "" ? Translate.Get("GLOBAL.CONTROL.CANCEL") : buttonCancel;

        if (showIcon === true) {
            iconContent = '<i class="fa ' + iconModal + '" aria-hidden="true"></i>';
        }

        var buttonsHtml = "";
        if (displayingButtons == null)
        {
            buttonsHtml = '<button type="button" data-deferred="cancel" class="buttonLink" data-dismiss="modal">' + modalButtonCancel + '</button>' +
                '<button type="button" data-deferred="true" class="buttonBlue" data-dismiss="modal">' + modalButtonOk + '</button>';
        }
        else
        {
            if (displayingButtons["cancel"] != false) {
                buttonsHtml = '<button type="button" data-deferred="cancel" class="buttonLink" data-dismiss="modal">' + modalButtonCancel + '</button>';
            }

            if (displayingButtons["ok"] != false) {
                buttonsHtml += '<button type="button" data-deferred="true" class="buttonBlue" data-dismiss="modal">' + modalButtonOk + '</button>';
            }
        }

        var tempId = "modalAlert-" + Number(new Date());
        $('body').append(
            '<div class="modal fade popup" id="' + tempId + '" tabindex="-1" role="dialog">' +
                '<div class="modal-dialog ' + typeModal + '">' +
                    '<div class="modal-content hide">' +
                        '<div class="modal-header">' +
                            '<button type="button" class="close" data-deferred="cancel" data-dismiss="modal" aria-label="Close"><i class="fa fa-times" aria-hidden="true"></i></button>' +
                            '<h3 class="modal-title">' + iconContent + titleModal + '</h3>' +
                        '</div>' +
                        '<div class="modal-body"> ' + message + '</div>' +
                        '<div class="modal-footer">' +
                        buttonsHtml +
                        '</div>' +
                    '</div>' +
                '</div>' +
            '</div>');

        var modal = $('#' + tempId).modal();
        modal.find('.modal-body').html(message);
        modal.find('[data-deferred]').click(function () {
            setTimeout(function (modal) {
                $('#' + modal).remove();
            }, 500, tempId);

            if (defer !== undefined) {
                defer.resolve($(this).attr('data-deferred') === "true");

                return defer.promise();
            }

            return;
        });
    };

    ShowTitleIconChildren = function (showIcon) {
        if (showIcon !== false) {
            return true;
        }

        return false;
    };

    this.ShowInfo = function (message, title, buttonOk, buttonCancel, showIcon, displayingButtons) {
        var defer = $.Deferred();
        Base(message, title, ShowTitleIconChildren(showIcon), "Info", defer, buttonOk, buttonCancel, displayingButtons);
        return defer.promise();
    };

    this.ShowSuccess = function (message, title, buttonOk, buttonCancel, showIcon, displayingButtons) {
        var defer = $.Deferred();
        Base(message, title, ShowTitleIconChildren(showIcon), "Success", defer, buttonOk, buttonCancel, displayingButtons);
        return defer.promise();
    };

    this.ShowWarning = function (message, title, buttonOk, buttonCancel, showIcon, displayingButtons) {
        var defer = $.Deferred();
        Base(message, title, ShowTitleIconChildren(showIcon), "Warning", defer, buttonOk, buttonCancel, displayingButtons);
        return defer.promise();
    };

    this.ShowError = function (message, title, buttonOk, buttonCancel, showIcon, displayingButtons) {
        var defer = $.Deferred();
        Base(message, title, ShowTitleIconChildren(showIcon), "Error", defer, buttonOk, buttonCancel, displayingButtons);
        return defer.promise();
    };
};

var Translate = new function () {
    var glosary;

    this.Get = function (literal) {
        if (literal === undefined || literal === "") {
            return "UNDEFINED LITERAL";
        }

        if (glosary === undefined || glosary[literal] === undefined) {
            $.ajax({
                url: GetUrl(),
                async: false,
                data: { literal: literal }
            }).done(function (data) {
                glosary = $.extend(glosary, data);
            });
        }

        return glosary === undefined || glosary[literal] === undefined ? literal : glosary[literal];
    };

    this.Init = function () {
        if (glosary === undefined) {
            $.ajax({
                url: GetUrl(),
                async: false
            }).done(function (data) {
                glosary = $.extend(glosary, data);
            });
        }
    };

    var GetUrl = function () {
        var protocol = location.protocol.toUpperCase() + "//";
        var url = location.href.toUpperCase();
        var newUrl = "";

        url = url.replaceAll(protocol, '')
                .replaceAll('//', '/');
        if (url.indexOf("?") > 0) {
            url = url.substr(0, url.indexOf("?"));
        }

        if (url[url.length - 1] === "/") {
            if (url.substr(0, url.length - 2).split('/').length === 1) {
                newUrl = protocol
                    + url
                    + "/Mainframe/GetLocation";
                return newUrl;
            }
        }

        if (url.indexOf("/OPERATION/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/OPERATION/"))
                    + "/Mainframe/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/OPUS/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/OPUS/"))
                    + "/OPUS/view/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/MISSIONS/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/MISSIONS/"))
                    + "/missions/view/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/VMR/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/VMR/"))
                    + "/VMR/MeetingRoom/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/VER/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/VER/"))
                    + "/VER/VirtualEditingRoom/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/TCM/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/TCM/"))
                    + "/TCM/FindingRecommendation/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/TC/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/TC/"))
                    + "/TC/View/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/SISCOR/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/SISCOR/"))
                    + "/Siscor/CorrespondenceView/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/DOCUMENT") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/DOCUMENT/"))
                    + "/OPUS/view/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/WORKSPACE/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/WORKSPACE/"))
                    + "/Workspace/AdministrationWs/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/DISBURSEMENT/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/DISBURSEMENT/"))
                    + "/Disbursement/OperationLevelProjections/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/OVERVIEW/") > 0) {
            newUrl = protocol
                    + url
                    + "/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/GLOBAL/TASKS/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/GLOBAL/TASKS/"))
                    + "/GLOBAL/TASKS/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/ADMINISTRATIONSECONDPHASE/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/ADMINISTRATIONSECONDPHASE/"))
                    + "/AdministrationSecondPhase/DelegationView/GetLocation";
            return newUrl;
        }

        if (url.indexOf("/FINANCIALPLAN/") > 0) {
            newUrl = protocol
                    + url.substr(0, url.indexOf("/FINANCIALPLAN/"))
                    + "/FinancialPlan/FinancialPlan/GetLocation";
            return newUrl;
        }

        if (url.split('/').length > 3) {
            newUrl = protocol
                    + url.substr(0, url.lastIndexOf("/"))
                    + "/GetLocation";
            return newUrl;
        }

        if (url.split('/').length > 0) {
            newUrl = protocol
                    + url
                    + "/GetLocation";
            return newUrl;
        }

        return url;
    };
};

var Validation = new function () {
    this.Init = function (container, isNotLifeValidation) {
        this.Destroy();
        $('[data-parsley-required="true"]').each(function (index) {
            var $this = $(this).attr('data-validation-id', index);
            var validationHTML = '<span validation-id="' + index + '" class="validation-element hide"></span>';

            if ($this.closest('div.inputSearch').length === 1) {
                $this.closest('div.inputSearch').append(validationHTML);
            } else if ($this.is('.inputPercent')) {
                $this.next().after(validationHTML);
            } else if ($this.is('.inputMultiSelect')) {
                $this.next().after(validationHTML);
            } else {
                $this.after(validationHTML);
            }

        });

        if (isNotLifeValidation !== true) {
            LiveValidation(ValidateContainer(container));
        }
    };

    this.Destroy = function () {
        $('[data-parsley-required="true"]').each(function () {
            $(this).removeAttr('data-validation-id');
        });

        $('[validation-id]').remove();
        $('.validation-fail').removeClass('validation-fail');
    };

    this.Container = function (container) {
        var content = ValidateContainer(container);

        var prevalidation = ValidateErrors();

        if (!prevalidation) {
            return false;
        }

        $(content).find('[data-parsley-required="true"]').each(function () {
            ValidationCore($(this));
            ValidationRegex($(this));
            ValidationDate($(this));
        });

        ForceValidation(content);

        return ValidateErrors();
    };

    ValidateErrors = function (container) {
        var content = ValidateContainer(container);
        if ($(content).find('.validation-fail').length === 0) {
            return true;
        } else {
            $(content).find('.validation-fail').each(function () {
                if ($(this).closest('.hide').length > 0) {
                    $(this).closest('.hide').removeClass('hide');
                }

                if ($(this).closest('[data-row-parent-id]').not('.showChildRow').length > 0) {
                    var idRow = $(this).closest('[data-row-parent-id]').attr('data-row-parent-id');
                    $('[data-id="' + idRow + '"]').find('.buttonShowRow').click();
                }
            }).first().focus();
            return false;
        }
    }

    ValidateContainer = function (container) {
        var response = container;
        if (container === undefined || container === "" || container === null) {
            response = $(document);
        }

        return response;
    };

    LiveValidation = function (container) {
        $(container).find('[data-parsley-required="true"]').bind('blur change keyup', function (event) {
            ValidationCore($(this), event);
        });
    };

    ForceValidation = function (container) {
        $(container).find('[data-parsley-required="true"]').bind('blur change', function (event) {
            ValidationCore($(this), event);
        });
    };

    ValidationCore = function (element, event) {
        var $this = $(element);
        var validationId = $this.attr('data-validation-id');
        if ($this.val() === null || $this.val().length === 0) {
            var msg;
            if ($this.attr('data-validation-req-msg') !== undefined) {
                msg = $this.attr('data-validation-req-msg');
            } else {
                msg = Translate.Get('GLOBAL.VALIDATION.REQUIRED');
            }

            ClassError($this, true);

            PutErrorMsg(validationId, msg);
        }
        else {
            ClassError($this, false);

            if (event !== undefined && event.type === "change") {
                if (!ValidationRegex($this) || !ValidationDate($this)) {
                    return;
                }
            }

            CleanErrorMsg(validationId);
        }
    };

    ValidationDate = function (element) {
        var $this = $(element);
        var validationId = $this.attr('data-validation-id');
        if (($this.val() !== null || $this.val().length !== 0) && $this.is('.datepicker-default')) {

            if (converToDate($this.val()) === false) {
                ClassError($this, true);
                PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATENOTVALID'));
                return false;
            }

            var dateElement = converToDate($this.val());
            var dateTest;

            if ($this.is('[dd-min-date]')) {
                dateTest = converToDate($this.attr('dd-min-date'));

                if (dateTest === false) {
                    ClassError($this, true);
                    $this.val("");
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEPRESELECTNOTVALID'));
                    return false;
                }

                if (dateElement < dateTest) {
                    ClassError($this, true);
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEGREATER') + ' ' + $this.attr('dd-min-date'));
                    return false;
                }
            }

            if ($this.is('[dd-max-date]')) {
                dateTest = converToDate($this.attr('dd-max-date'));

                if (dateTest === false) {
                    ClassError($this, true);
                    $this.val("");
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEPRESELECTNOTVALID'));
                    return false;
                }

                if (dateElement > dateTest) {
                    ClassError($this, true);
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATELESS') + ' ' + $this.attr('dd-max-date'));
                    return false;
                }
            }

            var elementDateCompare;
            var tempName;
            if ($this.is('[dd-less-date]')) {
                tempName = $this.attr('dd-less-date');
                if ($this.closest('tr').find('[name="' + tempName + '"]').length > 0) {
                    elementDateCompare = $this.closest('tr').find('[name="' + tempName + '"]');
                } else {
                    elementDateCompare = $('[name="' + tempName + '"]');
                }

                if (elementDateCompare.length === 0) {
                    ClassError($this, true);
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATENOTFIND') + ' ' + tempName);
                    return false;
                }

                dateTest = converToDate(elementDateCompare.val());

                if (dateTest === false) {
                    ClassError($this, true);
                    $this.val("");
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEPRESELECTNOTVALID'));

                    ClassError(elementDateCompare, true);
                    elementDateCompare.focus();
                    PutErrorMsg(elementDateCompare.attr('data-validation-id'), Translate.Get('GLOBAL.VALIDATION.REQUIRED'));
                    return false;
                }

                if (dateElement > dateTest) {
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATELESS') + ' ' + elementDateCompare.val());
                    return false;
                }
            }

            if ($this.is('[dd-greater-date]')) {
                tempName = $this.attr('dd-greater-date');
                if ($this.closest('tr').find('[name="' + tempName + '"]').length > 0) {
                    elementDateCompare = $this.closest('tr').find('[name="' + tempName + '"]');
                } else {
                    elementDateCompare = $('[name="' + tempName + '"]');
                }

                if (elementDateCompare.length === 0) {
                    ClassError($this, true);
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATENOTFIND') + ' ' + tempName);
                    return false;
                }

                dateTest = converToDate(elementDateCompare.val());

                if (dateTest === false) {
                    ClassError($this, true);
                    $this.val("");
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEPRESELECTNOTVALID'));

                    ClassError(elementDateCompare, true);
                    elementDateCompare.focus();
                    PutErrorMsg(elementDateCompare.attr('data-validation-id'), Translate.Get('GLOBAL.VALIDATION.REQUIRED'));
                    return false;
                }

                if (dateElement < dateTest) {
                    PutErrorMsg(validationId, Translate.Get('GLOBAL.VALIDATION.DATEGREATER') + ' ' + elementDateCompare.val());
                    return false;
                }
            }
        }

        return true;
    };

    ValidationRegex = function (element) {
        var $this = $(element);
        var validationId = $this.attr('data-validation-id');
        if (($this.val() !== null || $this.val().length !== 0) && $this.is('[data-validation-regex]')) {
            var expreg = new RegExp($this.attr('data-validation-regex'));
            var msg;
            if ($this.attr('data-validation-regex-msg') !== undefined) {
                msg = $this.attr('data-validation-regex-msg');
            } else {
                msg = Translate.Get('GLOBAL.VALIDATION.FORMAT');
            }

            if (!expreg.test($this.val())) {
                ClassError($this, true);
                PutErrorMsg(validationId, msg);
                return false;
            }
        }

        return true;
    };

    FindElement = function (element) {
        var $this = $(element);
        var workElement;
        if ($this.closest('div.dropdown').length === 1) {
            workElement = $this.closest('div.dropdown')
                .find('button').first();
        } else if ($this.closest('div.inputSearch').length === 1) {
            workElement = $this.closest('div.inputSearch')
                .find('input').first();
        } else if ($this.closest('div.chosenMultiSelect').length === 1) {
            workElement = $this.closest('div.chosenMultiSelect')
                .find('.chosen-choices').first();
        } else {
            workElement = $this;
        }

        return workElement;
    };

    ClassError = function (element, AddClass) {
        var workElement = FindElement(element);

        if (AddClass) {
            workElement.addClass('validation-fail');
        } else {
            workElement.removeClass('validation-fail');
        }
    };

    PutErrorMsg = function (validationId, msg) {
        $('[validation-id="' + validationId + '"]')
                .removeClass("hide")
                .html(msg);
    };

    CleanErrorMsg = function (validationId) {
        $('[validation-id="' + validationId + '"]')
                .addClass("hide")
                .html("");
    };
};