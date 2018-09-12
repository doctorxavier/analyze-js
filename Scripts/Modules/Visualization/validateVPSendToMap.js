var Images = new Object();

Images.carrouselWidth = 817;
Images.carrouselInc = 150;
Images.visibleImages = 7;
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
    var newContainerLeft = (Images.imagesWidth + (Images.imagesMargin * 2) + 6) * newPos;
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
        }, 50);
    }).removeClass('hide');
};

Images.openModalDialog = function (url, name, years, source, comments, documenturl, document, status) {
    $("#media-url").attr("src", url);
    $("#media-name").html(name);
    $("#media-years").html(years);
    $("#media-source").html(source);
    $("#media-comments").html(comments);
    $("#media-doc-url").attr("href", documenturl);
    $("#media-document").html(document);
    $("#media-status").html(status);
    Images.openModalDialog.Dialog = $("#SendToMapDialog").kendoWindow({
        maxWidth: "440px",
        title: visualOutputsPopupTitlePhotoDetails != undefined ?
            visualOutputsPopupTitlePhotoDetails : "Photo Details",
        draggable: false,
        resizable: false,
        pinned: false,
        actions: ["Close"],
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

$(document).ready(function () {
    Images.initCarrousel();
});
