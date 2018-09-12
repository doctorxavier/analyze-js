var PopupBuilderWarning = function (title, jQueryDomData) {
    var target = $("#target");
    $(window.parent.document).find("#target").append('<div class="ui-widget-overlay ui-front"></div>');
    target.append('<div class="ui-widget-overlay ui-front"></div>');
    target.append('<div class="dinamicModal"></div>');
    var title = title;
    var dialog = jQueryDomData.kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        open: function (e) {
            this.content("");
        },
        close: function () {
            var windowParentDocumentForm = $(window.parent.document).find("#target");
            $('#editingSpecial').hide();
            windowParentDocumentForm.find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
            windowParentDocumentForm.find(".ui-widget-overlay.ui-front").remove();
            var editingSpecial2 = $("editingSpecial2")
            if (editingSpecial2.length > 0) {
                editingSpecial2.hide();
            }
        }
    })
    var kWindowTitle = $(".k-window-title");
    $(".k-window-titlebar").addClass("warning");
    kWindowTitle.addClass("ico_warning");
    kWindowTitle.css("overflow", 'visible', 'important');
    dialog.data("kendoWindow").open();
}

var noActionPopUp = function () {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").hide();
    $('.loading-container').remove();
};