
$(document).ready(function () {

    $("#slct1").kendoDropDownList();

    $("#slct2").kendoDropDownList();

    dropDownSelect();

    $(".datepicker").kendoDatePicker({
        format: "MM/dd/yyyy"
    });

    initInputMultiSelect();

    $("#btnSlideOptions").on('click',function () {
        if ($(this).attr("data-state") === "show") {
            $("#blockFilters").slideUp("slow");
            $(this).attr("data-state", "hide");
            $(this).text("Show Filter");
            return;
        }
        $("#blockFilters").slideDown("slow");
        $(this).attr("data-state", "show");
        $(this).text("Show Filter");
    });

    $("#btnFilter").click(function () {
        var inputMultiSelect = $('.chosen.inputMultiSelect');
        var message = $('#resultMatrixMessage').val();
        if (inputMultiSelect.first().find('option:selected').length === 0) {
            notificationPopup('<div class = "messageContentFilter"><span>'+message+'</span></div>');
            return;
        }

        var cycleOptions = [],
            sectionOptions = [],
            typeOfChangeOptions = [],
            subTypeOfChangeOptions = [];

        for (var i = 0;  i< inputMultiSelect.length; i++) {
            var inputSelect = $(inputMultiSelect[i]);
            var nameFilter = inputSelect.attr('name');
            switch (nameFilter) {
                case "Cycles":
                    cycleOptions = loadFilters(inputSelect);
                    break;
                case "Section":
                    sectionOptions = loadFilters(inputSelect);
                    break;
                case "TypeOfChange":
                    typeOfChangeOptions = loadFilters(inputSelect);
                    break;
                case "SubTypeOfChange":
                    subTypeOfChangeOptions = loadFilters(inputSelect);
                    break;
                default:
                    return;
            }

        }

        var isEditPartial = $("#currentView").val() === "Edit";
        var urlRoute = $(this).data("route");
        urlRoute += "&cycleIds=" +
            cycleOptions.toString() +
            "&sectionIds=" + sectionOptions.toString() +
            "&changeIds=" + typeOfChangeOptions.toString() +
            "&subChangeIds=" + subTypeOfChangeOptions.toString()+
            "&editView=" + isEditPartial;
        idbg.lockUi(null, true);
        //redirectPage(urlRoute);
        document.location.href = urlRoute;
    });

    $("#btnClearFilters").click(function () {
        $('[name=Cycles]').val('').trigger('chosen:updated');
        $('[name=Section]').val('').trigger('chosen:updated');
        $('[name=TypeOfChange]').empty().trigger('chosen:updated');
        $('[name=SubTypeOfChange]').empty().trigger('chosen:updated');
    });

    $(".editChangeMatrix").click(function () {
        var urlRoute = $(this).data("route");
        //redirectPage(urlRoute);
        document.location.href = urlRoute;
    });

    $('.chosen.inputMultiSelect').on('change', function () {
        var selectFilter = $(this),
            nameFilter = selectFilter.attr('name'),
            options = [];
        options = loadFilters(selectFilter);

        switch (nameFilter) {
            case "Section":
                loadTypeOfChanges(options);
                break;
            case "TypeOfChange":
                loadSubTypeOfChanges(options);
                break;
            default:
                return;
        }
    });

});

function displayContent(tdCell) {

    var senderObjectType = event.srcElement.nodeName;

    if (senderObjectType == "TD") {
        // Get div to display
        var expandedDiv = $(tdCell).find(".text_expanded");
        // Get display value
        var display = $(expandedDiv).css("display");

        if (display == "none") {
            $(tdCell).css("height", "382px");
            $(expandedDiv).css("display", "block");
            $(expandedDiv).css("margin-top", "69px");
        } else {
            $(tdCell).css("height", "75px");
            $(expandedDiv).css("display", "none");
        }

        initReziseCloud();
    }
}

function deleteChangeMatrixRow(lnkDelete, MatrixChangeId) {
    // Get current row to delete
    var parentRow = $(lnkDelete).parent().parent().parent().parent().parent();

    var url = $("#hdnDeletematrixChangeWarning").val();

    url += "?matrixChangeId=" + MatrixChangeId;

    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: url,
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
    //dialog.center();
    dialog.open();
}

function showNotification(type, path) {

    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading ...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: path,
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
    dialog.open();

}

function notificationPopup(content) {

    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading ...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [
            "Close"
        ],
        open: function () {
            this.content(content);
        },
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
    dialog.open();

}


function loadTypeOfChanges(options) {

    var typeOfChangeFilter = $('[name=TypeOfChange]');
    typeOfChangeFilter.empty().trigger("chosen:updated");
    if (options.length > 0) {
        $.ajax({
            url: $("#hdnChangeTypesPath").val(),
            data: JSON.stringify({ "sectionIds": options }),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    typeOfChangeFilter
                        .append('<option value="0">All ' + typeOfChangeFilter.data('placeholder') + '</option>')
                        .append('<option value="' + data[i].ItemId + '">' + data[i].Name + '</option>')
                }
                typeOfChangeFilter.trigger("chosen:updated");
            }
        });
    } 
}

function loadSubTypeOfChanges(options) {

    var subTypeOfChanges = $('[name=SubTypeOfChange]');
    subTypeOfChanges.empty().trigger("chosen:updated");
    if (options.length > 0) {
        $.ajax({
            url: $("#hdnChangeSubTypesPath").val(),
            data: JSON.stringify({ "changeTypeIds": options }),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    subTypeOfChanges
                        .append('<option value="0">All ' + subTypeOfChanges.data('placeholder') + '</option>')
                        .append('<option value="' + data[i].ItemId + '">' + data[i].Name + '</option>')
                }
                subTypeOfChanges.trigger("chosen:updated");
            }
        });
    }

}

function loadFilters(filter) {
    var isAllOption = false,
        options = [],
        optionSelectFilter = filter.find('option:selected');

    for (var i = 0; i < optionSelectFilter.length; i++) {
        if (optionSelectFilter[i].value === "0") {
            isAllOption = true;
            options = [];
            break;
        }

        options.push(optionSelectFilter[i].value);
    }
    if (isAllOption) {
        var allOption = filter.find('option')
        for (var i = 0; i < allOption.length; i++) {
            options.push(allOption[i].value)
        }
    }

    return options;
}

function initInputMultiSelect() {

    var cycleInputMultiSelect = $('[name=Cycles]');
    var typeOfChangeInputMultiSelect = $('[name=TypeOfChange]');
    var subTypeOfChangeInputMultiSelect = $('[name=SubTypeOfChange]');
    $('[name=Section]').prepend('<option value="0">All Sections</option>');
    if (typeOfChangeInputMultiSelect.find('option').length > 0) {
        typeOfChangeInputMultiSelect.prepend('<option value="0">All TypeOfChange</option>')
    }

    if (subTypeOfChangeInputMultiSelect.find('option').length > 0) {
        subTypeOfChangeInputMultiSelect.prepend('<option value="0">All SubTypeOfChange</option>')
    }

    var cyclesOptions = cycleInputMultiSelect.find('option');
    cyclesOptions.sort(function (a, b) {
        a = a.value;
        b = b.value;
        return b - a;
    });

    $('[name=Cycles]').html(cyclesOptions);
    cycleInputMultiSelect.prepend('<option value="0">All Cycles</option>');
    $('.chosen.inputMultiSelect').chosen().trigger('chosen:updated');
    $('.chosen-container').css("width", "250px")
}