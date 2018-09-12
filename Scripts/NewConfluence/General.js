$(document).ready(function () {
    PreCargar();
    var tabActive = $('ul.tabs li.active');
    if (tabActive.length > 0) {
        var tabPane = $('div.tab-pane');
        if (tabPane.length > 0) {
            tabPane.removeClass('active');
            $(tabActive.attr('dd-tab')).addClass('active');
        }
    }

    Translate.Init();
});

$(document).on('shown.bs.modal', '.modal:not(.not-position)', function () {
    var background = $('.modal-backdrop');
    var modal = $(this).find('.modal-content').addClass('hide');
    var top = 0;
    var modalHeight = modal.removeClass('hide').height() + 50;
    var windowScrollTop = 0;

    if ($(window.parent.document).find('[name="someFrame"]').length > 0) {
        var $frame = $(window.parent.document).find('[name="someFrame"]');
        var windowHeight = $(window.parent).height();
        var frameTopPosition = $frame.offset().top;
        var frameHeight = $("html").height();
        windowScrollTop = window.parent.document.body.scrollTop || window.parent.document.documentElement.scrollTop || window.parent.pageYOffset || 0;
        var frameScrollTop = windowScrollTop - frameTopPosition;

        var windowHeightForFrame = windowHeight - frameTopPosition + windowScrollTop;
        if (windowHeightForFrame > windowHeight) {
            windowHeightForFrame = windowHeight;
        }

        if (frameScrollTop + windowHeightForFrame > frameHeight) {
            windowHeightForFrame = windowHeightForFrame - (frameScrollTop + windowHeightForFrame - frameHeight);
        }

        if (frameScrollTop < 0) {
            frameScrollTop = 0;
        }

        var popupTopPosition = frameScrollTop + ((windowHeightForFrame - modalHeight) / 2);

        if (modalHeight > windowHeightForFrame) {
            popupTopPosition = frameScrollTop;
        }

        if ((frameHeight > 0) && (modalHeight + popupTopPosition > frameHeight)) {
            popupTopPosition = frameHeight - modalHeight;
        }

        top = popupTopPosition;
    } else {
        var modalTop = 0;

        if (typeof (window.innerWidth) == 'number') {
            modalTop = window.innerHeight;
        } else if (document.documentElement &&
              document.documentElement.clientHeight) {
            modalTop = document.documentElement.clientHeight;
        } else if (document.body && document.body.clientHeight) {
            modalTop = document.body.clientHeight;
        }

        top = (modalTop - modalHeight) / 2 - 12;
    }

    if (top < 0) {
        top = 0;
    }

    modal.css("top", top + 'px');

    modal.draggable({
        handle: ".modal-header"
    });

    if ($('#mainLayoutOperationContent').length > 0) {
        var position = $('#mainLayoutOperationContent').position();

        background.css('top', position.top)
            .css('left', position.left)
            .css('position', 'absolute')
            .css('height', $('#mainLayoutOperationContent').height());

        var centerLeft = (background.width() - modal.width()) / 2;
        var centerTop = position.top - modal.height() / 2;

        if (windowScrollTop == 0) {
            centerTop = position.top + 10;
        }

        modal.css('left', centerLeft)
            .css('top', centerTop);
    }

    background.style('opacity', 0.5, 'important')
});


$(document).on('shown.bs.modal', '.modal.not-position', function () {
    var background = $('.modal-backdrop');
    var modal = $(this).find('.modal-content').addClass('hide');
    var top = 0;
    var modalHeight = modal.removeClass('hide').height() + 50;
    var windowScrollTop = 0;

    if ($(window.parent.document).find('[name="someFrame"]').length > 0) {
        var $frame = $(window.parent.document).find('[name="someFrame"]');
        var windowHeight = $(window.parent).height();
        var frameTopPosition = $frame.offset().top;
        var frameHeight = $("html").height();
        windowScrollTop = window.parent.document.body.scrollTop || window.parent.document.documentElement.scrollTop || window.parent.pageYOffset || 0;
        var frameScrollTop = windowScrollTop - frameTopPosition;

        var windowHeightForFrame = windowHeight - frameTopPosition + windowScrollTop;
        if (windowHeightForFrame > windowHeight) {
            windowHeightForFrame = windowHeight;
        }

        if (frameScrollTop + windowHeightForFrame > frameHeight) {
            windowHeightForFrame = windowHeightForFrame - (frameScrollTop + windowHeightForFrame - frameHeight);
        }

        if (frameScrollTop < 0) {
            frameScrollTop = 0;
        }

        var popupTopPosition = frameScrollTop + ((windowHeightForFrame - modalHeight) / 2);

        if (modalHeight > windowHeightForFrame) {
            popupTopPosition = frameScrollTop;
        }

        if ((frameHeight > 0) && (modalHeight + popupTopPosition > frameHeight)) {
            popupTopPosition = frameHeight - modalHeight;
        }

        top = popupTopPosition;
    } else {
        var modalTop = 0;

        if (typeof (window.innerWidth) == 'number') {
            modalTop = window.innerHeight;
        } else if (document.documentElement &&
              document.documentElement.clientHeight) {
            modalTop = document.documentElement.clientHeight;
        } else if (document.body && document.body.clientHeight) {
            modalTop = document.body.clientHeight;
        }

        top = (modalTop - modalHeight) / 2 - 12;

        if (top < 0) {
            top = 0;
        }
    }

    modal.draggable({
        handle: ".modal-header"
    });

    if ($('#mainLayoutOperationContent').length > 0) {
        var position = $('#mainLayoutOperationContent').position();

        background.css('top', position.top)
            .css('left', position.left)
            .css('position', 'absolute')
            .css('height', $('#mainLayoutOperationContent').height());

        var centerLeft = (background.width() - modal.width()) / 2;
        var centerTop = position.top - modal.height() / 2;

        if (windowScrollTop == 0) {
            centerTop = position.top + 10;
        }

        modal.css('left', centerLeft)
            .css('top', centerTop);
    }

    background.style('opacity', 0.5, 'important')
});

function PreCargar() {
    if ($('table.tableNormal').find('.tree').length > 0) {
        CollapseTables();
    }

    imgFlag();

    $('div.btn-group').each(function () {
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
}

function converToDate(fecha) {
    if (fecha === undefined) {
        return false;
    }

    var date = fecha.split(" ");

    if (date.length < 3) {
        return false;
    }

    var months = {
        Jan: 1,
        Feb: 2,
        Mar: 3,
        Apr: 4,
        May: 5,
        Jun: 6,
        Jul: 7,
        Aug: 8,
        Sep: 9,
        Oct: 10,
        Nov: 11,
        Dec: 12,
    };

    var day = parseInt(date[0]);
    var year = parseInt(date[2]);

    if (isNaN(day) || months[date[1]] === undefined || isNaN(year)) {
        return false;
    }
    var month = months[date[1]];

    if (day < 1 || year < 1) {
        return false;
    }

    if ((month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) && day > 31) {
        return false;
    }

    if ((month == 4 || month == 6 || month == 9 || month == 11) && day > 30) {
        return false;
    }

    if (month == 2) {
        if (((year % 4) == 0 && (year % 100) != 0) || ((year % 400) == 0 && (year % 100) == 0)) {
            if (day > 29)
                return false;
        } else {
            if (day > 28)
                return false;
        }
    }

    var result = new Date(year, month - 1, day);
    if (!isNaN(result.getTime())) {
        return result;
    } else {
        return false;
    }
}