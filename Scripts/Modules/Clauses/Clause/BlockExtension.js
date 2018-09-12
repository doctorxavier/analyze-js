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

var isSpecialClausePopupBuilder = function (
    popupNoSpecialExtension,
    message,
    messagePopupNoSpecial,
    targetUrl) {

    var $target = $("#target");
    var isChecked = $('#Is_Special_Extension_Id').is(':checked');

    if (!$target.valid()) {
        $target.validate().focusInvalid();

        return;
    }

    if (isChecked) {

        $('#editingSpecial').show();
        var editingSpecial = $('#editingSpecial');
        var content = "";
        content += "<div class='margin5'><span>"
            + message
            + "</span> </div>";
        content += "<div class='editingButtonsEdit'>" + editingSpecial.html() + "</div>";
        var title = "Warning";
        extensionContractPopup(title, content);
        return;
    }

    $('#IsSpecialExtensionCheck').val(false);
    $('#Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__ExecutorRequest').remove();
    $('#Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__IdbRequest').remove();
    $target.attr('action', targetUrl);

    showLoaderOptional();

    $.ajax({
        type: 'POST',
        url: $("#determine-validator-udr").val(),
        data: $target.serialize(),
        async: false,
        success: function (response) {

            $target.find('#id-mandatory-validator').val(response.Data.Validators);
            $target
                .find('input[name="Contracts[0].Clauses[0]' +
                    '.ClauseIndividuals[0].ClauseIndividualId"]')
                .val(response.Data.ClauseIndividualId);
            $target.find('input[name="Contracts[0].Clauses[0].ClauseId"]')
                .val(response.Data.ClauseId);
            $target.find('input[name="Contracts[0].ContractId"]').val(response.Data.ContractId);
            $target
                .find('input[name="Contracts[0].Clauses[0].ClauseIndividuals[0]' +
                    '.ClauseExtension[0].ClauseExtensionId"]')
                .val(response.Data.ClauseExtensionId);

            if (!response.IsValid) {
                hideLoaderOptional();
                showMessage(response.ErrorMessage);
                return;
            }

            if (response.IsValidPopup) {
                $target.submit();
                return;
            }

            confirmAction(response.PopupMsg).done(function (isPressedOk) {
                if (isPressedOk) {
                    showLoaderOptional();
                    $target.submit();
                    return;
                }
            });

            hideLoaderOptional();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            hideLoaderOptional();
            ShowNotification('error', "An error ocurred");
        }
    });
};

function removeLoading() {
    $(".loading-container").remove();
}

function ShowNotification(type, message) {
    showNotification({
        'message': message,
        'type': type,
        'autoClose': 'true',
        'duration': '30'
    });
}

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
    $target = $("#target");
    $target.attr('action', targetUrl);

    showLoaderOptional();

    $.ajax({
        type: 'POST',
        url: $("#determine-validator-udr").val(),
        data: $target.serialize(),
        async: false,
        success: function (response) {

            if (response.IsValid) {
                $target.submit();
                return;
            }

            hideLoaderOptional();
            showMessage(response.ErrorMessage);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            hideLoaderOptional();
            ShowNotification('error', "An error ocurred");
        }
    });


}