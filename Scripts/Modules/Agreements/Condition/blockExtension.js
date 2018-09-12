$(document).ready(function () {
    $("#Save-Extension").on('click', function () {
        if (sumbitaction()) {
            idbg.lockUi(null, true);
            var route = $('#Save-Extension').attr("action");
            miVar = setTimeout("route", 7000);
            redirectPage(miVar);
        }
    });

});

$(document).ready(function () {

    $("#Save-Extension1").on('click', function () {
        if (sumbitaction()) {
            idbg.lockUi(null, true);
            var route = $('#Save-Extension1').attr("action");
            miVar = setTimeout("route", 7000);
            redirectPage(miVar);
        }
    });
});

$(document).ready(function () {
    $(".central").submit(function () {
        return sumbitaction();
    });
});

var extensionContractPopup = function (title, content) {
    var target = $("#target");
    $(window.parent.document).find("#target").append('<div class="ui-widget-overlay ui-front"></div>');
    target.append('<div class="ui-widget-overlay ui-front"></div>');
    target.append('<div class="dinamicModal"></div>');
    var title = title;
    var dialog = $(".dinamicModal").kendoWindow({
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
            this.content(content);
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
    }).data("kendoWindow");
    var kWindowTitle = $(".k-window-title");
    $(".k-window-titlebar").addClass("warning");
    kWindowTitle.addClass("ico_warning");
    kWindowTitle.css("overflow", 'visible', 'important');
    dialog.open();
    var popup = $(".k-widget.k-window");
    $('body, html', parent.document).animate(
        {
            scrollTop: popup.offset().top,
            scrollLeft: popup.offset().left - 120
        }, 1000);
}

var isSpecialEvent = function (targetUrl) {
    var windowParentDocumentForm = $(window.parent.document).find("#target");
    windowParentDocumentForm.find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
    windowParentDocumentForm.find(".ui-widget-overlay.ui-front").remove();
    $('#idbRequest, #executorRequestedMonths').prop('disabled', false);
    idbg.lockUi(null, true);
    $("#target").attr('action', targetUrl);
    $("#target").submit();
}