$(document).ready(function () {

    
    $(".kendoDropDown ").kendoDropDownList();
    $(".k-dropdown").css("font-size", "16px");
    $(".k-dropdown-wrap").css("font-size", "16px");
    $(".k-input").css("font-size", "16px");
    $(".k-select").css("font-size", "16px");


    $("#VisitsPanel").find('input[type="text"][id$="__Actual"]').removeAttr("data-val");
    $("#VisitsPanel").find('input[type="text"][id$="__Actual"]').removeAttr("data-val-required");

    var saveWarningOnTabChange = function () {
        event.preventDefault();
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = $("#IsInEdit").data("windowTitle");
        var modal = $(".dinamicModal").kendoWindow({
            width: "800px",
            position: { top: 100 },
            title: title,
            draggable: false,
            resizable: false,
            pinned: false,
            content: $("#IsInEdit").data("routeSaveWarning"),
            actions: ["Close"],
            modal: true,
            visible: false,
            activate: function () {
                $("#ConfirmSaveWarning").click(functionCancel);
            },
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        modal.open();
    };

    $("#saveVisit").click(function (e) {
        
        var form = $("#save-form").removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);

        var PlannedFields = $("#VisitsPanel").find('input[type="text"][id$="__Planned"]');
        var emptyPlannedFieldsCount = 0;
        $.each(PlannedFields, function (index, plannedField) {
            if ($(plannedField).val().length <= 0 || parseInt($(plannedField).val()) <= 0) {
                emptyPlannedFieldsCount++;
                return false;
            }
        });

        if (emptyPlannedFieldsCount <= 0) {
            idbg.lockUi(null, true);
            $(this).submit();
        }
    });


    $('.yearSelector').on('click', function () {
        event.preventDefault();

        $('.plan-row').removeClass('selected');
        $(this).closest('.plan-row').addClass('selected');

        var planId = $(this).closest('tr').data('planId');
        var actualYear = $('#main-form').find('#Year');
        var year = $(this).closest('tr').data('year');
        $('#main-form').find('#Year').val(year);
        $('#save-form').find('#Year').val(year);
        if (actualYear != year) {
            var isInEdit = $("#IsInEdit").val().toLowerCase() === 'true';
            if (!isInEdit) {
                $('#SelectedPlanId').val(planId);
                $('#TitleYear').text(year);
                var buttonsRoute = $(this).closest('tr').data('buttonsRoute');

                $.get(buttonsRoute, function (data) {
                    $('.buttonsPanel').html(data);
                });
            }
            $(".inline-block.tabItem.active").click();
        }
    });

    $(document).on('click', "input[type=button].deleteVisitBtn", deleteVisit);

    $(document).on('click', ".ac-EditPlan", function () {

        event.preventDefault();
        $(".view-selector").val($(".inline-block.tabItem.active").data("id"));
        redirectPage($(this).data("route"));
    });
    /*
    $(".inline-block.tabItem").click(function () {
        
        var isInEdit = $("#IsInEdit").val().toLowerCase() === 'true';
        var planId = $('#SelectedPlanId').val();
        if (isInEdit) {
            saveWarningOnTabChange();
        }
        else if (planId != -1) {
            $(".inline-block.tabItem").removeClass("active");
            $(this).addClass("active");
            var ajaxContainer = $("#loadingContainerSP");
            var panel = $("#SupervisionPlanPanel");
            panel.addClass("loading-overlay");
            ajaxContainer.css("display", "");
            kendo.ui.progress(ajaxContainer, true);

            var url = $(this).data("route") + '?id=' + planId;


            $('#VisitPlanPanel').load(url, function () {
                kendo.ui.progress(ajaxContainer, false);
                panel.removeClass("loading-overlay");
                ajaxContainer.css("display", "none");
                resizeIframeCloud();
            });

        }
    });
    */
    $(document).on("click", ".download-doc", function () {
        event.preventDefault();
        var url = $(this).data("route");
        var form = $("#main-form");
        if (url != "")
            form.attr("action", url);
        deletemodal__()
        form.submit();
    });

    $('.integer-edit-field').keypress(function (event) {

        if (!keyCodeIsNumber(event.which) && !isSpecialKey(event.which)) {
            event.preventDefault();
            return;
        }
    });

    $(document).on("click", ".remove-doc", function () {
        event.preventDefault();
        //TODO remove document and recount
        var tr = $(this).closest("tr");
        var docsTable = $(this).closest("tbody");
        tr.remove();
        reCountDocuments(docsTable);
    });

    $(document).on('click', "#newVisit", function () {
       
        event.preventDefault();
        var totalVisits = $("#VisitsPanel").find(".visitComponent").length;
        var visitIndex = totalVisits > 0 ? (totalVisits - 1 + 1) : 0;
        var viewVisitIndex = visitIndex + 1;
        var templateData = new Array();
        templateData.push({ "visitIndex": visitIndex, "viewVisitIndex": viewVisitIndex });

        // Load persons grid template
        var fuente = $('#newVisit-template').html();
        // Compile template
        var plantilla = Handlebars.compile(fuente);
        // Get filled template
        var html = plantilla(templateData);

        //$("#VisitsPanel").prepend(html);
        $("#VisitsPanel").append(html);
        // $(".kendoDropDown ").kendoDropDownList();
        $("#Visits_" + visitIndex + "__TypeVisitId").kendoDropDownList();
        //$(html).find("select.kendoDropDown").kendoDropDownList();

        $(".k-dropdown").css("font-size", "16px");
        $(".k-dropdown-wrap").css("font-size", "16px");
        $(".k-input").css("font-size", "16px");
        $(".k-select").css("font-size", "16px");

        setVisitsCount();
        
    });

    $(document).on('click', ".ac-CancelEdit", function () {
        event.preventDefault();
        $(".view-selector").val($(".inline-block.tabItem.active").data("id"));
        redirectPage($(this).data("route"));
    });

    $(document).on('click', ".ac-SavePlan", function () {


        event.preventDefault();

        var form = $("#save-form")
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation"); /* added by the jquery unobtrusive plugin */
        $.validator.unobtrusive.parse(form);

        prepareSave();
        $("#ReturnToEdit").val("false");
        if ($("#save-form").valid()) {
            idbg.lockUi(null, true);
            $("#save-form").submit();
        } else {
            $("#save-form .input-validation-error").first().focus();
        }
    });

    $(document).on('click', ".ac-DeletePlan", function () {

        event.preventDefault();
        var planId = $(this).data('version-id');
        var title = $(this).data('windowTitle');
        var warningRoute = $(this).data('routeDeleteWarning');
        var actionRoute = $(this).data("route") + "?planVersionId=" + planId;
        deleteWarning(planId, title, warningRoute, actionRoute);
    });

    $(document).on('click', ".ac-PlanYear", function () {

        var operation = $(this).data('version-id');
        var year = $(this).data('year');
        var title = $(this).data('windowTitle');
        var selectRoute = $(this).data('routeSelectYear');
        var actionRoute = $(this).data("route") + "?operationNumber=" + operation + "?years=" + year;

        selectYear(operation, title, selectRoute, actionRoute);
    });

    deletemodal__();

    if ($("#hdnIsApprovedPlan").val() == "True") {
        $("#budgetInformationContainer").show();
    }
});

function reloadkeypress() {
    var form = $("#save-form")
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation"); /* added by the jquery unobtrusive plugin */
    $.validator.unobtrusive.parse(form);
    $('.integer-edit-field').keypress(function (event) {
        if (!keyCodeIsNumber(event.which) && !isSpecialKey(event.which)) {
            event.preventDefault();
            return;
        }
    });
}

function keyCodeIsNumber(keyCode) {
    return (keyCode >= 48 && keyCode <= 57);
}

function keyCodeIsDecimalDelimiter(keyCode) {
    return (keyCode == 46 || keyCode == 44);
}

function isSpecialKey(keyCode) {
    return (keyCode == 0 || keyCode == 9 || keyCode == 8);
}

function deletemodal__() {
    try {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
        $(".k-window-titlebar").remove();
    } catch (Exception) { }
}

var selectYear = function (operation, title, selectRoute, actionRoute) {

    event.preventDefault();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: selectRoute,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        activate: function () {
            $("#CancelDelete").click(functionCancel);
            $("#ConfirmDelete").click(function () {
                event.preventDefault();
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
            });
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $("body").find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    dialog.center();
    dialog.open();
};

var deleteWarning = function (versionId, title, warningRoute, actionRoute) {

    event.preventDefault();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var modal = $(".dinamicModal").kendoWindow({
        width: "800px",
        position: { top: 100 },
        title: title,
        draggable: false,
        resizable: false,
        pinned: false,
        content: warningRoute,
        actions: ["Close"],
        modal: true,
        visible: false,
        activate: function () {
            $("#CancelDelete").click(functionCancel);
            $("#ConfirmDelete").click(function () {
                event.preventDefault();
                $('#main-form').find('#Year').val(year);
                $('#main-form').find('#PlanVersionId').val(versionId);
                redirectPage(actionRoute);
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
            });
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $("body").find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    modal.open();

};

function redirectPage(url) {

    var form = $("#main-form");
    if (url != "")
        form.attr("action", url);
    idbg.lockUi(null, true);
    deletemodal__()
    form.submit();
};

var functionCancel = function () {
    event.preventDefault();
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();
};

var elementtodeletevisits = null;

function deleteVisit(element, VisitId) {

    var elementdelete = $(element).parent();
    var validator = true;
    var cont = 0;
    while (validator) {
        elementdelete = $(elementdelete).parent();
        if ($(elementdelete).attr("class") == "verticalMargin40 visitComponent") { validator = false; }
        if (cont >= 10) { validator = false; }
        cont++;
    }
    elementtodeletevisits = elementdelete;
    var positionRelative = $(element).offset();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    var messageDelete = $("#messageDelete").val();
    var titleDeleteVisit = $("#titleDeleteVisit").val();
    var year = $("#Year").val();
    var operationNumber = $("#OperationNumber").val();
    var visit = $(this).closest(".visitComponent");
    $('body').append('<div class="ui-widget-overlay ui-front"></div>');
    if (VisitId != undefined) {
        var route = $("#rutaDeleteVisit").val() + '?visitId=' + VisitId + '&operationNumber=' + operationNumber + '&year=' + year;

        $("body").append(
                              '<div class="dinamicModal"><div style="padding: 20px;">' + messageDelete + '</div><div class="pie pieReassign"><div class="botones"> ' +
                                 '<a title="Cancel" class="button blueButton" id="CancelWarningDialog" onclick="removemodal();" ' +
                                'href="javascript:void(0)">Cancel</a><label for="cancel">' +
                                '<a title="Delete" class="button blueButton" id="DeleteVisit"' +
                                'href="' + route + '" onclick="javascript:idbg.lockUi(null, true);deletemodal__()">Delete</a><label for="delete"></div></div></div>');
    }
    else {

        $("body").append(
                             '<div class="dinamicModal"><div style="padding: 20px;">' + "This action cannot be undone, are you sure you wish to continue?" + '</div><div class="pie pieReassign"><div class="botones"> ' +
                                '<a title="Cancel" class="button blueButton" id="CancelWarningDialog" onclick="removemodal();" ' +
                               'href="javascript:void(0)">Cancel</a><label for="cancel">' +
                               '<a title="Delete" class="button blueButton" id="DeleteComponent"' +
                               'onclick="RemoveComponent();" href="javascript:void(0)" onclick="javascript:idbg.lockUi(null, true);deletemodal__()">Delete</a><label for="delete"></div></div></div>');
    }

    titleDeleteVisit = "Delete Visit";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: titleDeleteVisit,
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        activate: function () {
            $("#CancelDelete").click(functionCancel);
            $("#DeleteComponent").click(function () {
                redirectPage(actionRoute);
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
            });
        },
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
    
};

var RemoveComponent = function (elementdelete) {

    event.preventDefault();

    var visit = $(this).closest(".visitComponent");
    visit.remove();
    $(elementtodeletevisits).remove();
    setVisitsCount();
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();

};

var setVisitsCount = function () {

    var visits = $(".visit-counter");
    for (var i = 0; i < visits.length; i++) {
        $(visits[i]).text(i + 1);
    }
    var posicion = $("#footer_").position();
    $(document).scrollTop(posicion.top);
    reloadkeypress();
};

var prepareSave = function () {

    var viewId = $(".inline-block.tabItem.active").data("id");
    reCountFields("#VisitsPanel", "activity");
    $(".view-selector").val(viewId);
}

var reCountFields = function (container, view) {

    var rows = $(container);
    for (var i = 0; i < rows.children().length; i++) {
        var element = $(rows.children()[i]);
        var elems = element.find('[data-view="' + view + '"]');
        for (var j = 0; j < elems.length; j++) {
            var name = $(elems[j]).attr("name");
            name = name.replace(/\[\d+\]/i, "[" + i + "]");
            $(elems[j]).attr("name", name);
        }
    }
    reloadkeypress();
};

var reCountDocuments = function (table) {

    for (var i = 0; i < table.children().length; i++) {
        var element = $(table.children()[i]);
        var elems = element.find('[data-view="document"]');
        for (var j = 0; j < elems.length; j++) {
            var name = $(elems[j]).attr("name");
            name = name.replace(/\Documents\[\d+\]/i, "Documents[" + i + "]");
            $(elems[j]).attr("name", name);
        }
    }
    reloadkeypress();
};

function deleteDocument(element, DocumentId) {
    var positionRelative = $(element).offset();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    var messageDelete = $("#messageDelete").val();

    $('body').append('<div class="ui-widget-overlay ui-front"></div>');
    var route = $("#rutaVisit").val() + '?dicId=' + DocumentId;

    $("body").append(
                          '<div class="dinamicModal"><div style="padding: 20px;">' + messageDelete + '</div><div class="pie pieReassign"><div class="botones"> ' +
                             '<a title="Cancel" class="cancel" id="CancelWarningDialog" onclick="removemodal();" ' +
                            'href="javascript:void(0)">Cancel</a><label for="cancel">' +
                            '<a title="Delete" class="button blueButton" id="DeleteExecutorContact"' +
                            'href="' + route + '" onclick="javascript:idbg.lockUi(null, true);deletemodal__()">Delete</a><label for="delete"></div></div></div>');


    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: "Delete Document",
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
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

function DeleteDocumentByVisit(DocumentId) {
    idbg.lockUi(null, true);
    deletemodal__()
    $("#formVisit").submit();
};

function removemodal() {

    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
};

function save1() {
    idbg.lockUi(null, true);
    deletemodal__()
    $("#save-form").submit();
};

function showHideVisitDetail(btnSlideVisitDetail) {

    if ($(btnSlideVisitDetail).hasClass('plus')) {
        $(btnSlideVisitDetail).removeClass('plus');
        $(btnSlideVisitDetail).parent('div').parent('div').siblings().slideUp(200);
    } else {
        $(btnSlideVisitDetail).addClass('plus');
        $(btnSlideVisitDetail).parent('div').parent('div').siblings().slideDown(200);
    }

}