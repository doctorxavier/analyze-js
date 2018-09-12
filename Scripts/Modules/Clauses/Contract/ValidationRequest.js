ClauseExtension = new Object();

ClauseExtension.Methods = {
    BlockUI: function () {
        if (typeof showLoaderOptional !== 'undefined' && $.isFunction(showLoaderOptional)) {
            showLoaderOptional();
        }
    },
    DisableButtons: function () {
        ClauseExtension.ActionButtons.addClass('disabled').prop('disabled', true);
    },
    EnableButtons: function () {
        ClauseExtension.ActionButtons.removeClass('disabled').prop('disabled', false);
    }
}

$(document).ajaxError(function () {
    ClauseExtension.Methods.EnableButtons();
});

$(document).ready(function () {

    ClauseExtension.ActionButtons = $('.action-buttons').find('input');

    $(".Request").on("click", function () {
        var titleWarning = $(".Request").attr('data-title');
        if ($('#TLValidators').val() == "False") {
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
            var title = titleWarning;
            var dialog = $(".dinamicModal").kendoWindow({
                width: "800px",
                title: title,
                draggable: false,
                resizable: false,
                content: $("#TLSendRequestUrl").val(),
                pinned: true,
                actions: [
                    "Close"
                ],
                modal: false,
                visible: false,
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
            dialog.center();
            dialog.open();
        }
        else {
            warningDraw();
        }
    });

    $(".RequestEdit").on("click", function () {
        var categoryClause = $(".RequestEdit").attr('data-category');
        var titleWarning = $(".RequestEdit").attr('data-title');
        if (categoryClause == "LASTD") {
            if ($('#TLValidators').val() == "False") {
                $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
                var title = titleWarning;
                var dialog = $(".dinamicModal").kendoWindow({
                    width: "800px",
                    title: title,
                    draggable: false,
                    resizable: false,
                    content: $("#TLSendRequestUrl").val(),
                    pinned: true,
                    actions: [
                        "Close"
                    ],
                    modal: false,
                    visible: false,
                    close: function () {
                        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                        $(".ui-widget-overlay").remove();
                        $(".k-window").remove();
                    }
                }).data("kendoWindow");
                $(".k-window-titlebar").addClass("warning");
                $(".k-window-title").addClass("ico_warning");
                dialog.center();
                dialog.open();
            }
            else {
                warningDraw();
            }
        }
        else {
            if ($('#TLValidators').val() == "False") {
                $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="ui-widget-overlay ui-front"></div>');
                $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
                var title = titleWarning;
                var dialog = $(".dinamicModal").kendoWindow({
                    width: "800px",
                    title: title,
                    draggable: false,
                    resizable: false,
                    content: $("#TLSendRequestUrl").val(),
                    pinned: true,
                    actions: [
                        "Close"
                    ],
                    modal: false,
                    visible: false,
                    close: function () {
                        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                        $(".ui-widget-overlay").remove();
                        $(".k-window").remove();
                    }
                }).data("kendoWindow");
                $(".k-window-titlebar").addClass("warning");
                $(".k-window-title").addClass("ico_warning");
                dialog.center();
                dialog.open();
            }
            else {
                sendRequest();
            }
        }
    });

    function warningDraw() {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"style="overflow-y: hidden;"></div>');
        $("#warningSendRequest").appendTo(".dinamicModal").removeClass("hide");
        var title = $("#warningSendRequest").data("title");
        var modal = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: false,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("#warningSendRequest").appendTo("#ui_sp_001").addClass("hide");
                $("body").find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        modal.center();
        modal.open();
    }

});

function warningPopupSendRequest(urlToSendRequest) {
    sendRequest();
}

function warningPopupSendRequestWorkflow(urlToSendRequest) {
    window.location = urlToSendRequest;
}

function warningPopupSendRequestWorkflowCL2(urlToSendRequest) {
    window.location = urlToSendRequest + "&additionalValidators=" + $('#validator_list_additional_list').val();
}

function warningPopupClose(sender) {
    $(sender).closest('.dinamicModal.k-window-content.k-content').data("kendoWindow").close();
}

var functionCancel = function () {
    event.preventDefault ? event.preventDefault() : event.returnValue = false;
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();
};

function sendRequest() {
    ClauseExtension.Methods.BlockUI();
    ClauseExtension.Methods.DisableButtons();
    idbg.lockUi(null, true);
    var form = document.getElementById('target');
    form.action = $('.RequestEdit').attr('name');
    form.submit();
}

function InhabilitarBotones(ui_element, waiting) {
    //Bloqueo de links y botones tipo submit y button, 
    $(document).find('input[type="button"],input[type="submit"],a').attr('disabled', 'disabled');

    //Bloqueo de texto de un textarea,
    $(document).find('textarea').attr('readonly', 'true');

    //Bloqueo de botones de chequeo
    $(document).find('input[type="text"],input[type="checkbox"]').attr('readonly', 'readonly');

    //Bloqueo DropDown
    $(".kendoDropDown").kendoDropDownList({
        enabled: false
    });

    //$('.k-widget.k-dropdown.k-header.kendoDropDown').attr('aria-disabled', 'true');
    //$('.k-widget.k-dropdown.k-header.kendoDropDown').removeAttr('tabindex');
    //$('.k-dropdown-wrap.k-state-default').attr('class', 'k-dropdown-wrap k-state-disabled');
    //$('select.kendoDropDown').attr('disabled', 'disabled');

    //Bloqueo Data Picker
    $(".datepicker").kendoDatePicker({
        enabled: false
    });

    //Bloqueo Radio Button
    $(document).find('input[type="radio"]').parent().attr('enabled', 'true');
    $(document).find('input[type="radio"]').attr('disabled', 'disabled');
    $('.custom.radio').attr('disabled', 'disabled');
}

