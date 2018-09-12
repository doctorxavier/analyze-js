function approveRejectAllVOItems(origin) {
    switch ($(origin)[0].selectedIndex) {
        case 1:
            $('.mod_contenido_central.impacts').find('[name^=valSendToMap_][value=Approved]').prop('checked', true);
            break;
        case 2:
            $('.mod_contenido_central.impacts').find('[name^=valSendToMap_][value=Rejected]').prop('checked', true);
            break;
    }

    origin.value = "0";
}

var VOsCooDialogs = new Object();

VOsCooDialogs.NotAllRadioButtonsSelectedDialog = function (popupTitle) {
    if (VOsCooDialogs.NotAllRadioButtonsSelectedDialog.Dialog == null) {
		var overlayDiv = '<div class="ui-widget-overlay ui-front"></div>';

        $(window.parent.document).find('body').append(overlayDiv);
        $(window).find('body').append(overlayDiv);
        $(window).find('body').append(overlayDiv);

        VOsCooDialogs.NotAllRadioButtonsSelectedDialog.Dialog = $("#notAllRadioButtonsSelectedDialog").kendoWindow({
            width: "800px",
            title: popupTitle,
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

    VOsCooDialogs.NotAllRadioButtonsSelectedDialog.Dialog.center().open();
};

VOsCooDialogs.NotAllRadioButtonsSelectedDialog.Dialog = null;
