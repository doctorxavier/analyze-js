var dataInPageHasChanged = false;
var Images = new Object();

Images.carrouselWidth = 800;
Images.carrouselInc = 150;
Images.visibleImages = 4;
Images.imagesWidth = 170;
Images.imagesMargin = 15;

Images.scrollRight = function (button) {
    Images.scroll(button, 1);
};

Images.scrollLeft = function (button) {
    Images.scroll(button, -1);
};

Images.scroll = function (button, inc) {
    var $button = $(button);
    var carrousel = $button.parents('.carrousel');
    var carrouselPos = parseInt(carrousel.attr('data-pos'));
    var images = carrousel.find('img');
    var imagesLength = images.length;
    var newPos = carrouselPos + inc;

    if (newPos <= 0) {
        newPos = 0;
    }

    if (newPos > imagesLength - Images.visibleImages) {
        newPos = imagesLength - Images.visibleImages;
    }

    carrousel.attr('data-pos', newPos);
    var newContainerLeft = (Images.imagesWidth + (Images.imagesMargin * 2) + 220) * newPos;
    carrousel.find('.images-container').animate({ 'left': '-' + String(newContainerLeft) + 'px' });

    Images.loadImage(images.eq(Images.visibleImages + newPos - 1));
};

Images.loadImage = function (img) {
    $(img).each(function () {
        var $img = $(this);
        var imageUrl = $img.attr('data-src');
        $img.attr('src', imageUrl);
    });
};

Images.initCarrousel = function () {
    $('.carrousel').each(function () {
        $(this).find('.images').width(Images.imagesWidth * Images.visibleImages);
        var images = $(this).find('img');
        images.width(Images.imagesWidth);

        images.css({ 'margin': '0 ' + String(Images.imagesMargin) + 'px' });
        Images.loadImage(images.slice(0, Images.visibleImages));

        var $this = $(this);
        setTimeout(function () {
            $this.find('.paginator input').height($this.find('.images').outerHeight());
            $this.find('.images').css({ 'top': -$this.find('.paginator input').height() });
            $this.find('.paginator').width($this.find('.images').outerWidth());
            $this.parents('.tableGrid').first().height($this.parents('.tableGrid').height() - $this.find('.paginator input').height());
        }, 50);
    }).removeClass('hide');
};

Images.openModalDialog = function (url, name, years, source, comments, documenturl, document,status) {
        $("#media-url").attr("src", url);
        $("#media-name").html(name);
        $("#media-years").html(years);
        $("#media-source").html(source);
        $("#media-comments").html(comments);
        $("#media-doc-url").attr("href",documenturl);
        $("#media-document").html(document);
        $("#media-status").html(status);
        Images.openModalDialog.Dialog = $("#SendToMapDialog").kendoWindow({
            maxWidth: "440px",
            title: visualOutputsPopupTitlePhotoDetails != undefined ?
                visualOutputsPopupTitlePhotoDetails : "Photo Details",
            draggable: false,
            resizable: false,
            pinned: false,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
            }
        }).data("kendoWindow");
        $(".k-window-title").addClass("ico_warning");
    Images.openModalDialog.Dialog.center();
    Images.openModalDialog.Dialog.open();
}

function massCollapse(collapseBtn, tables) {
    if (collapseBtn.isActive()) {
        if (tables !== null) {
            for (var i = 0; i < tables.length; i++) {
                tables[i].unfold();
            }
        }
    }
    else {
        if (tables !== null) {
            for (var i = 0; i < tables.length; i++) {
                tables[i].fold();
            }
        }
    }
}

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

function changeCollapseBtn(collapseBtn, tables) {
    if (tables !== null) {
        if (collapseBtn.isActive()) {
            for (var i = 0; i < tables.length; i++) {
                if (tables[i].isVisible()) {
                    return false;
                }
            }
            return true;
        }
        else {
            for (var i = 0; i < tables.length; i++) {
                if (tables[i].isVisible()) {
                    return true;
                }
            }
            return false;
        }
    }
}

var ConfirmDialog = new Object();

ConfirmDialog.ChangeTab = function (redirectToUrl) {
    if (dataInPageHasChanged) {
        if (ConfirmDialog.ChangeTab.Dialog == null) {
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $(window).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $(window).find('body').append('<div class="ui-widget-overlay ui-front"></div>');

            ConfirmDialog.ChangeTab.Dialog = $("#changeTabDialog").kendoWindow({
                width: "800px",
                title: visualOutputsPopupTitleChangeTab != undefined ?
                    visualOutputsPopupTitleChangeTab : "Change tab",
                draggable: false,
                resizable: false,
                pinned: true,
                actions: ["Close"],
                modal: true,
                visible: false,
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                }
            }).data("kendoWindow");

            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
        }

        ConfirmDialog.ChangeTab.Dialog.center().open();
    }
    else {
        window.location = redirectToUrl;
    }
};

ConfirmDialog.ChangeTab.Dialog = null;

function ChangeTab(redirectToUrl) {
    ConfirmDialog.ChangeTab.Dialog.close();
    window.location = redirectToUrl;
}

$(document).ready(function () {
    Images.initCarrousel();

    var tables = [];

    if ($(".minimizeTable").length > 0) {
        var minimizeTable = $(".minimizeTable");
        var collapseBtn = new BotonActivo($(".btn-action-group"));

        for (var i = 0; i < $(minimizeTable).length; i++) {
            tables.push(new TableFold(minimizeTable[i]));
        }
    }

    for (var i = 0; i < tables.length; i++) {
        tables[i].foldBtn.click(function () {
            setTimeout(function () {
                if (changeCollapseBtn(collapseBtn, { tables: tables })) {
                    collapseBtn.switchState(collapseBtn);
                }
            }, 500);
        });
    }

    if (collapseBtn !== undefined) {
        var collapseBtnActiveText =
            collapseBtnActiveTextVIS != undefined ? collapseBtnActiveTextVIS : "Collapse All";
        var collapseBtnInactiveText =
            collapseBtnInactiveTextVIS != undefined ? collapseBtnInactiveTextVIS : "Expand All";
        var collapseBtnInputText = collapseBtn.getBtnInputText();

        if (collapseBtnInputText.toLowerCase() === collapseBtnActiveText.toLowerCase()) {
            collapseBtn.setActiveText(collapseBtnActiveText);
            collapseBtn.setInactiveText(collapseBtnInactiveText);
        }

        collapseBtn.btnInput.click(function () {
            massCollapse(collapseBtn, tables);
        });
    }

    $('#lnkShowDisaggregation').bind('click', function () {

        $('.evaluation-tracking-block.Cancelled').show();
        $('#lnkShowDisaggregation').hide();
        $('#lnkHideDisaggregation').removeClass('hidden');
        

    });

    $('#lnkHideDisaggregation').bind('click', function () {
        $('.evaluation-tracking-block.Cancelled').hide();
        $('#lnkShowDisaggregation').show();
        $('#lnkHideDisaggregation').addClass('hidden');
        
    });
    $('.evaluation-tracking-block.Cancelled').hide();

   
});
