var isHasBeenModifications = false;

var submitActionOutputs = function () {
    $(".btnSavePhysicalOutput").off("click");

    if (!$("#outputsTarget").valid()) {
        $(".btnSavePhysicalOutput").click(submitActionOutputs);
        return;
    }

    if (chekIfItHasBeenChanges()) {
        var positionRelative = $(this).offset();

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var route = $("#WarningMessageURL").val();
        var title = "Warning";
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: route,
            pinned: false,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
                $(".btnSavePhysicalOutput").click(submitActionOutputs);
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();
    }
    else {
        idbg.lockUi(null, true);
        $("input.numberInput").each(function () {

            if ($(this).val().length > 0) {
                $(this).val($(this).autoNumeric('get'));
            }


        });
        $("#outputsTarget").submit();
    }
}

var reassingOutputWindowModal = function (element) {
    var positionRelative = $(element).offset();
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: $(element).data("title"),
        draggable: false,
        resizable: false,
        content: $(element).data("route"),
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        open: function () {
            kendoWindowCenter($(".dinamicModal"));
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    dialog.center();
    dialog.open();

    $("body").css("overflow", "hidden");
    var cerrar = $(".k-window-action");
    cerrar.click(function () {
        $(".k-window").hover(
            function () {
                document.onmousewheel = document.onmousedown = wheel;
                document.onkeydown = keydown;
            },
            function () {
                document.onmousewheel = document.onmousedown = document.onkeydown = null;
            }
        );
        $("body").css("overflow", "");
        
    });
}

$(document).ready(function () {

    $("div.mod_contenido_central").on("mouseenter", ".selectCustom", function () {
        $(this).closest("tr").find(".deleteYear").show();
    });
    $("div.mod_contenido_central").on("mouseleave", ".btn_azul_oscuro", function () {
        $(this).find(".deleteYear").hide();
    });

    $(".btnSavePhysicalOutput").click(submitActionOutputs);

    $(".optionSelect").each(function (index) {
        $(this).kendoDropDownList();

        if ($(this).hasClass("optionSelectOutputYearPlan")) {
            var dropdownlistOutputYearPlan = $(this).data("kendoDropDownList");

            dropdownlistOutputYearPlan.list.width(60);
            dropdownlistOutputYearPlan.bind("change", applyChangedYearInSelects);
            dropdownlistOutputYearPlan.bind("select", verifyChangedYearInSelect);
        }
    });

    $.each($('input.numberInput'), function (index, input) {
        var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

        if (year >= 2014) {
            $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });
        } else {
            $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
        }
    });

    $(".changeValueInput").change(function () {
        isHasBeenModifications = true;
    });

    $(".annualChangeValueInput").change(function () {
        var year = parseInt($(this).closest("td.dato07").find("> input.hiddenYear").val());
        var currentYear = parseInt($("#CurrentServerYear").val());
        if (year >= currentYear) {
            if (!$(this).hasClass("valuechanged")) {
                $(this).addClass("valuechanged");
            }
        }
    });

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            if ($(this).attr('data-val-range'))
                return $(this).attr('data-val-range');
            if ($(this).attr('data-val-required'))
                return $(this).attr('data-val-required');
        }
    });

    $('.reassingOutput').click(function () {
        reassingOutputWindowModal(this);
    });


    $('#outputsTarget').on('mouseenter', 'tr.hoverMilestone', function () {
        $(this).find(".actionbar2").css("display", "block");
    });
    $('#outputsTarget').on('mouseleave', 'tr.hoverMilestone', function () {
        $(this).find(".actionbar2").css("display", "none");
    });

    $('.k-dropdown-wrap.k-state-default').addClass('btn');
});

//--------------------------------------------components---------------------------------------------
function addNewComponent() {
    var maxComponentIndex = -1;
    maxComponentIndex = $("form > div").length;

    var source = $("#component-template").html();
    var template = Handlebars.compile(source);
    var context = { componentIndex: maxComponentIndex };
    var newDinamicContent = template(context);

    $("form").prepend(newDinamicContent);

    rebindingValitadionFieldsForOutputs($("#outputsTarget"));
    

}
function deleteComponent(element) {

    var componentIndex = $(element).closest(".mod_tabla_impacts").attr("data-componentindex");

    if ($(".component" + componentIndex).children("#Components_" + componentIndex + "__ComponentId").length == 0) {
        $(".component" + componentIndex).remove();
    }
    else {
        var positionRelative = $(element).offset();
        var operationNumber = $("#Operation_OperationNumber").val();
        var componentId = $("#Components_" + componentIndex + "__ComponentId").val();
        var positionTop = positionRelative.top - 100;
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: $("#IndexDeleteComponentUrl").data("title"),
            draggable: false,
            resizable: false,
            content: $("#IndexDeleteComponentUrl").data("route") + "?componentId=" + componentId + "&operationNumber=" + operationNumber,
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

        $("body").css("overflow", "hidden");
        var cerrar = $(".k-window-action");
        cerrar.click(function () {
            $(".k-window").hover(
                function () {
                    document.onmousewheel = document.onmousedown = wheel;
                    document.onkeydown = keydown;
                },
                function () {
                    document.onmousewheel = document.onmousedown = document.onkeydown = null;
                }
            );
            $("body").css("overflow", "");
            
        });
    }
    
}

function moveUpComponent(btnMoveUp) {
    ///DOWN
    var componentContainer = $(btnMoveUp).closest(".mod_tabla_impacts");
    var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html();

    ///UP
    var prevComponentContainer = $(componentContainer).prev(".mod_tabla_impacts");
    var previousTrackOrder = $(prevComponentContainer).find(".minimizeBar > .trackOrder").html();

    $("div.outputsContainer > table.grid > tbody > tr.trackOrderRow").each(function (index) {
        var componentIndex = $(this).closest("div.mod_tabla_impacts").attr("data-componentindex");
        var rowIndex = $('div.outputsContainer > table.grid > tbody > tr.trackOrderRow >td').index(this);

        $(this).find("> td").eq(0).text().replace(".", ",");
        orden = $.trim($(this).find("> td").eq(0).text());
        ort = $(this).eq(rowIndex).find("> td").eq(0).text();

        iOrdenUp = parseFloat(orden);

        if (parseFloat(orden) > parseFloat(componentTrackOrder) && parseFloat(orden) < (parseFloat(previousTrackOrder) + 2)) {
            iOrdenUp = iOrdenUp - 1;
            $(this).eq(rowIndex).find("> td").eq(0).text(parseFloat(iOrdenUp).toFixed(1).toString());
        }

        if (parseFloat(orden) > parseFloat(previousTrackOrder) && parseFloat(orden) < (parseFloat(previousTrackOrder) + 1)) {
            iOrdenUp = iOrdenUp + 1;
            $(this).eq(rowIndex).find("> td").eq(0).text(parseFloat(iOrdenUp).toFixed(1).toString());
        }
    });

    var componentContainer = $(btnMoveUp).closest(".mod_tabla_impacts");
    var prevComponentContainer = $(componentContainer).prev(".mod_tabla_impacts");

    if (prevComponentContainer.length > 0) {
        var componentIndex = $(componentContainer).index();
        var prevComponentIndex = $(prevComponentContainer).index();
        var previousTrackOrder = $(prevComponentContainer).find(".minimizeBar > .trackOrder").html();
        var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html();

        $(componentContainer).find(".minimizeBar > .trackOrder").html(previousTrackOrder);
        $(prevComponentContainer).find(".minimizeBar > .trackOrder").html(componentTrackOrder);

        componentContainer.insertBefore(prevComponentContainer);
        $(componentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", prevComponentIndex);
        $(prevComponentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", componentIndex);
    }
}

function moveDownComponent(btnMoveDown) {
    ///UP
    var componentContainer = $(btnMoveDown).closest(".mod_tabla_impacts");
    var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html();

    ///DOWN
    var nextComponentContainer = $(componentContainer).next(".mod_tabla_impacts");
    var nextTrackOrder = $(nextComponentContainer).find(".minimizeBar > .trackOrder").html();

    $("div.outputsContainer > table.grid > tbody > tr.trackOrderRow").each(function (index) {
        var componentIndex = $(this).closest("div.mod_tabla_impacts").attr("data-componentindex");
        var rowIndex = $('div.outputsContainer > table.grid > tbody > tr.trackOrderRow >td').index(this);

        $(this).find("> td").eq(0).text().replace(".", ",");
        orden = $.trim($(this).find("> td").eq(0).text());
        ort = $(this).eq(rowIndex).find("> td").eq(0).text();

        iOrdenDown = parseFloat(orden);

        if (parseFloat(orden) > parseFloat(componentTrackOrder) && parseFloat(orden) < parseFloat(nextTrackOrder)) {
            iOrdenDown = iOrdenDown + 1;
            $(this).eq(rowIndex).find("> td").eq(0).text(parseFloat(iOrdenDown).toFixed(1).toString());
        }

        if (parseFloat(orden) > parseFloat(nextTrackOrder) && parseFloat(orden) < (parseFloat(nextTrackOrder) + 1)) {
            iOrdenDown = iOrdenDown - 1;
            $(this).eq(rowIndex).find("> td").eq(0).text(parseFloat(iOrdenDown).toFixed(1).toString());
        }
    });

    var componentContainer = $(btnMoveDown).closest(".mod_tabla_impacts");
    var nextComponentContainer = $(componentContainer).next(".mod_tabla_impacts");

    if (nextComponentContainer.length > 0) {
        var componentIndex = $(componentContainer).index();
        var nextComponentIndex = $(nextComponentContainer).index();
        var nextTrackOrder = $(nextComponentContainer).find(".minimizeBar > .trackOrder").html();
        var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html();

        $(componentContainer).find(".minimizeBar > .trackOrder").html(nextTrackOrder);
        $(nextComponentContainer).find(".minimizeBar > .trackOrder").html(componentTrackOrder);
        componentContainer.insertAfter(nextComponentContainer);

        $(componentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", nextComponentIndex);
        $(nextComponentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", componentIndex);
    }
}

function showMainActionBar(stamentComponent) {
    $(stamentComponent).closest("label").find(".actionbar.main").show();
}

function hideMainActionBar(stamentComponent) {
    $(stamentComponent).closest("label").find(".actionbar.main").hide();
}

//--------------------------------------------Outputs------------------------------------------------

function componentHasSubComponents(triggerElement, componentIndex)
{
    var componentId = $(".component" + componentIndex)
                        .find("#Components_" + componentIndex + "__ComponentId").val();

    var path = $("#hdn-subcomponents-path").val();

    var request = $.ajax({
        url: path,
        data: JSON.stringify({ componentId: componentId }),
        dataType: "json",
        contentType: "application/json",
        type: 'post'
    });

    var response = {
        isValid: true,
        data: null
    };

    request.done(function (data) {
        response.data = data;
    }).fail(function () {
        response.isValid = false;
    });

    return response;
}

function showOutputCreationWindow(subComponents,
    triggerElement,
    title,
    templateId,
    componentIndex) {

    var componentStatement = $(".component" + componentIndex)
                                .find("#Components_" + componentIndex + "__Statement").val();

    var creationModel = {
        Statement: componentStatement,
        Subcomponents: subComponents
    };

    kendoWindow = createKendoModalWindow
    (
        creationModel,
        triggerElement,
        title,
        $("#" + templateId).html()
    );

    $("#output-pep-creation-container")
        .data("target-element-id", $(triggerElement).attr("id"));

    kendoWindow.open();
}

function createKendoModalWindow(viewModel, targetElement, title, template)
{
    var positionRelative = $(targetElement).offset();
    var positionTop = positionRelative.top - 100;
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

    var kendoWindow = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        pinned: true,
        actions: [ "Close" ],
        modal: true,
        visible: false,
        content: {
            template: template
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");

    kendo.bind(kendoWindow.element, viewModel);

    return kendoWindow;
}

function checkOutputCreation(triggerElement)
{
    var isIntegratedWithPep = $("#hdn-is-integrated-with-pep").val();

    if (isIntegratedWithPep.toLowerCase() == 'true') {
        var componentIndex = $(triggerElement).closest("div.mod_tabla_impacts").attr("data-componentindex");
        var response = componentHasSubComponents(triggerElement, componentIndex);
        var subComponentId = -1;

        if (response.isValid &&
            response.data != null) {

            if (response.data.length > 2) {
                showOutputCreationWindow(response.data,
                    triggerElement,
                    $("#hdn-subcomponents-path").data("modal-title"),
                    'output-pep-creation-template',
                    componentIndex);
            } else {

                if (response.data.length == 2)
                    subComponentId = response.data[1].PepTaskId;

                addNewOutput(triggerElement, subComponentId);
            }
        }

        if (response.isValid == false) {
            showNotification({
                message: "Error try to retrieve subcomponents",
                type: 'error',
                autoClose: false,
                duration: 5
            });

            return;
        }
    } else {
        addNewOutput(triggerElement, -1);
    }
}

function AddOutputWithModalWindow()
{
    var triggerElement = $("#" + $("#output-pep-creation-container").data("target-element-id"));
    var subComponentId = $("#ddl-create-new-output-pep").data("kendoDropDownList").value();

    cancelAddOutputWithModalWindow();
    addNewOutput(triggerElement, subComponentId);
    $(triggerElement).focus();
}

function cancelAddOutputWithModalWindow()
{
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
}

function addNewOutput(element, subComponentId) {

    var componentIndex = $(element).closest("div.mod_tabla_impacts").attr("data-componentindex");

    var interval = new Object();

    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var maxOutputIndex = 0;
    var resultArray = $(element).closest("div.minimizeTable").find("> div.outputsContainer > table > tbody > tr.nivel01[data-outputindex]").
       map(function () { return $(this).attr("data-outputindex"); }).get();

    if (resultArray.length > 0) {
        maxOutputIndex = Math.max.apply(null, resultArray);
        maxOutputIndex++;
    }

    var source, template, context, newDinamicContent;
    var options = new Array();
    var currentYear = parseInt($("#CurrentServerYear").val());

    if ($(".component" + componentIndex).find(".outputsContainer > table").length == 0) {
        source = $("#outputHeader-template").html();
        template = Handlebars.compile(source);
        context = { EndProjectYear: $("#EndProjectYear").val() };
        newDinamicContent = template(context);
        $(".component" + componentIndex).find(".outputsContainer").prepend(newDinamicContent);

        options = getCurrentListOfYears();
        var i = 0;
        for (i = 0; i < options.length; i++) {
            addNewYearPlanOutputHeader(element, options[i]);
        }

        $(".component" + componentIndex).find("div.outputsContainer > table.grid > thead > tr > th[data-outputyearplanindex] div.deleteYear:not(:last)").remove();
    }

    options = new Array();
    $(element).closest("div.minimizeTable").
       find("> div.tableGrid > table > thead > tr:eq(0) > th[data-outputyearplanindex]").
       each(function (index) {
           var isActualValueBlock = false;
           var isOriginalValueBlock = false;
           var isAnnualValueBlock = false;
           var yearValue = 0;
           if ($(this).find("select").length > 0) {
               yearValue = parseInt($(this).find("select").val().trim());
           }
           else {
               yearValue = parseInt($(this).find("li.small").text().trim());
           }

           isOriginalValueBlock = BlockFieldStartUpPlan(interval);
           isAnnualValueBlock = BlockFieldAnnualPlan(interval, yearValue);
           isActualValueBlock = BlockFieldActualPlan(interval, yearValue);
           var newObject = {
               outputYearPlanIndex: (index + 1), year: yearValue, isOriginalValueBlock: isOriginalValueBlock,
               isAnnualValueBlock: isAnnualValueBlock, isActualValueBlock: isActualValueBlock,
               componentIndex: componentIndex, outputIndex: maxOutputIndex
           };
           options.push(newObject);
       });
    
    source = $("#outputBody-template").html();
    template = Handlebars.compile(source);
    context = {
        componentIndex: componentIndex,
        outputIndex: maxOutputIndex,
        parentPepTaskId: subComponentId,
        orderNumber: (maxOutputIndex + 1),
        taskType: typeof subComponentId === 'undefined' || subComponentId == null || subComponentId == '-1' ? 'component' : 'subcomponent',
        options: options,
        EndProjectYear: $("#EndProjectYear").val()
    };
    newDinamicContent = template(context);

    var toBindNumberEvent = $(".component" + componentIndex).find(".outputsContainer > table > tbody").append(newDinamicContent);

    $.each($(toBindNumberEvent).find("tr.nivel01:last input.numberInput"), function (index, input) {
        var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

        if (year >= 2014) {
            $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });
        } else {
            $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
        }
    });

    rebindingValitadionFieldsForOutputs($("#outputsTarget"));
    fixDropdonwYears(true);
}

function deleteOutput(element) {
    var usedForStrategicAlignment = $(element)
        .closest("tr[data-outputindex]")
        .find("> td:eq(0) > input.UsedForStrategicAlignment")
        .val();

    if (usedForStrategicAlignment != undefined && usedForStrategicAlignment.toLowerCase() === "true") {
        var checkSARmIndicatorsUrl = $(element).attr('data-sa-checkindicators-url');
        var placeHolder = $(element).attr('data-sa-checkindicators-placeholder');

        checkRmIndicatorRelationsToSAClassifications(placeHolder, checkSARmIndicatorsUrl, function () {
            deleteOutputExecution(element);
        });
    } else {
        deleteOutputExecution(element);
    }
}

function deleteOutputExecution(element) {
    var componentElement = $(element).closest("div[data-componentindex]");

    if ($(element).closest("tr[data-outputindex]").find("> td:eq(0) > input.OutputId").length == 0) {
        if ($(element).closest("tr[data-outputindex]").next().hasClass("nivel02")) {
            $(element).closest("tr[data-outputindex]").next().remove();
        }
        $(element).closest("tr[data-outputindex]").prev("tr.trackOrderRow").remove();
        $(element).closest("tr[data-outputindex]").remove();
    }
    else {
        var operationNumber = $("#Operation_OperationNumber").val();
        var outputId = $(element).closest("tr[data-outputindex]").find("> td:eq(0) > input:first").val();
        var positionRelative = $(element).offset();

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: $("#IndexDeleteOutputUrl").data("title"),
            draggable: false,
            resizable: false,
            content: $("#IndexDeleteOutputUrl").data("route") + "?OutputId=" + outputId + "&OperationNumber=" + operationNumber,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            open: function () {
                kendoWindowCenter($(".dinamicModal"));
            },
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
                idbg.lockUiRM(null, false);
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();
    }
}

//--------------------------------------------Outputs year plan---------------------------------------
function addNewYearPlanOutput(element) {
    var EOPYear = parseInt($("#EndProjectYear").val());
    var maxYear = getMaxSelectedYearInOutputYearPlans($(element).closest("table.grid").first());

    if ((EOPYear != 0) && (maxYear > (EOPYear + 1) || EOPYear === -1)) {
        return;
    }
    var container = $(element).closest("div.minimizeTable");
    var nroOutputYearPlans = $(element).closest("table.grid").find("thead > tr:first > th[data-outputyearplanindex]").length;
    var onPipeLineYear = parseInt($("#OnPipelineDate_Year").val());

    var currrentYear = $("#CurrentServerYear").val();

    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var options = new Array();
    var maxOutputYearIndex = -1;

    var source = $("#outputYearHeader-template").html();
    var template = Handlebars.compile(source);
    var isFirstOutputYearPlan = false;
    if (nroOutputYearPlans == 0) {
        isFirstOutputYearPlan = true;
        for (var i = onPipeLineYear - 10; i < onPipeLineYear + 30; i++) {
            var newObject = null;
            if (i == maxYear) {
                newObject = { value: i, name: i, isSelected: true };
            }
            else {
                newObject = { value: i, name: i };
            }
            options.push(newObject);
        }
    }

    var year;
    $("div.outputsContainer > table.grid > thead > tr > th[data-outputyearplanindex] div.deleteYear").remove();

    $("div.outputsContainer > table.grid > thead > tr").each(function (index) {
        var componentIndex = $(this).closest("div.mod_tabla_impacts").attr("data-componentindex");
        var maxOutputYearIndex = $(this).find("> th[data-outputyearplanindex]").length + 1;

        var context = {
            outputYearPlanIndex: maxOutputYearIndex, componentIndex: componentIndex,
            options: options, isFirstOutputYearPlan: isFirstOutputYearPlan, currrentYear: maxYear
        };
        var newDinamicContent = template(context);

        var selectContent = null;
        selectContent = $(this).find("th").eq(-2).before(newDinamicContent).prev();

        if (nroOutputYearPlans == 0) {
            var insertedSelect = $(selectContent).find("select");

            $(insertedSelect).kendoDropDownList().prev().addClass('btn');
            var dropdownlistOutputYearPlan = $(insertedSelect).data("kendoDropDownList");
            dropdownlistOutputYearPlan.bind("change", applyChangedYearInSelects);
            dropdownlistOutputYearPlan.bind("select", verifyChangedYearInSelect);
            dropdownlistOutputYearPlan.list.width(60);
            year = insertedSelect.val();
        }
        else {
            var insertedSelect = $(selectContent).find("li.small");
            year = insertedSelect.text();
        }

    });

    source = $("#MilestoneYearHeader-template").html();
    template = Handlebars.compile(source);

    $("div.outputsContainer > table.grid > tbody > tr.nivel02  table.milestoneTable > thead").each(function (index) {

        var colspan = parseInt($(this).closest("tr.nivel02").find("> td:first").attr("colspan"));
        colspan++;
        $(this).closest("tr.nivel02").find("> td:first").attr("colspan", colspan);
        var maxMilestoneYearIndex = $(this).find("> th[data-milestoneyearplanindex]").length + 1;

        var context = {
            milestoneyearplanindex: maxMilestoneYearIndex, year: maxYear
        };
        var newDinamicContent = template(context);

        var selectContent = null;
        selectContent = $(this).find("th").eq(-2).before(newDinamicContent).prev();

    });

    source = $("#outputYearBody-template").html();
    template = Handlebars.compile(source);

    var isActualValueBlock = false;
    var isOriginalValueBlock = false;
    var isAnnualValueBlock = false;

    isOriginalValueBlock = BlockFieldStartUpPlan(interval);
    isAnnualValueBlock = BlockFieldAnnualPlan(interval, year);
    isActualValueBlock = BlockFieldActualPlan(interval, year);


    $("div.outputsContainer > table.grid > tbody").each(function (index) {
        var componentIndex = $(this).closest("div.mod_tabla_impacts").attr("data-componentindex");
        $(this).find(" > tr[data-outputindex]").each(function (index2) {
            var maxOutputYearIndex = $(this).find("> td[data-rowcolumnrelated]").length + 1;

            var outputIndex = $(this).attr("data-outputindex");

            context = {
                outputYearPlanIndex: maxOutputYearIndex,
                componentIndex: componentIndex,
                outputIndex: outputIndex,
                year: year,
                isOriginalValueBlock: isOriginalValueBlock,
                isAnnualValueBlock: isAnnualValueBlock,
                isActualValueBlock: isActualValueBlock
            };

            newDinamicContent = template(context);
            $(newDinamicContent).insertBefore($(this).find("> td:eq(-2)"));

            $.each($(this).find("> td[data-rowcolumnrelated]:last input.numberInput"), function (index, input) {

                var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

                if (year >= 2014) {
                    $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });

                } else {
                    $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
                }
            });
        });

    });

    source = $("#milestoneYearBody-template").html();
    template = Handlebars.compile(source);

    $("div.outputsContainer > table.grid > tbody > tr.nivel02 div.tableGrid > table.milestoneTable > tbody").each(function (index) {
        var componentIndex = $(this).closest("div.mod_tabla_impacts").attr("data-componentindex");
        var outputIndex = $(this).closest("tr.nivel02").prev().attr("data-outputindex");

        $(this).find(" > tr[data-milestoneindex]").each(function (index2) {

            var maxMilestoneYearIndex = $(this).find("> td[data-rowcolumnrelated]").length + 1;

            var milestoneIndex = $(this).attr("data-milestoneindex");

            context = {
                milestoneYearPlanIndex: maxMilestoneYearIndex, componentIndex: componentIndex, outputIndex: outputIndex, milestoneIndex: milestoneIndex,
                year: year, isOriginalValueBlock: isOriginalValueBlock, isAnnualValueBlock: isAnnualValueBlock, isActualValueBlock: isActualValueBlock
            };

            newDinamicContent = template(context);
            $(newDinamicContent).insertBefore($(this).find("> td:eq(-2)"));

            $.each($(this).find("> td[data-rowcolumnrelated]:last input.numberInput"), function (index, input) {

                var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

                if (year >= 2014) {
                    $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });

                } else {
                    $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
                }
            });
        });
    });
}

function addNewYearPlanOutputHeader(element, year) {
    var container = $(element).closest("div.minimizeTable");
    var nroOutputYearPlans = $(container).find("table.grid > thead > tr:first > th[data-outputyearplanindex]").length;
    var onPipeLineYear = parseInt($("#OnPipelineDate_Year").val());
    var componentIndex = $(element).closest("div.mod_tabla_impacts").attr("data-componentindex");

    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var options = new Array();
    var maxOutputYearIndex = -1;

    var source = $("#outputYearHeader-template").html();
    var template = Handlebars.compile(source);
    var isFirstOutputYearPlan = false;
    if (nroOutputYearPlans == 0) {
        isFirstOutputYearPlan = true;
        for (var i = onPipeLineYear - 10; i < onPipeLineYear + 30; i++) {
            var newObject = null;
            if (i == year) {
                newObject = { value: i, name: i, isSelected: true };
            }
            else {
                newObject = { value: i, name: i };
            }
            options.push(newObject);
        }
    }

    $(container).find("div.outputsContainer > table.grid > thead > tr").each(function (index) {

        var maxOutputYearIndex = $(this).find("> th[data-outputyearplanindex]").length;
        maxOutputYearIndex++;

        var context = {
            outputYearPlanIndex: maxOutputYearIndex, componentIndex: componentIndex,
            options: options, isFirstOutputYearPlan: isFirstOutputYearPlan, currrentYear: year
        };
        var newDinamicContent = template(context);

        var selectContent = null;
        selectContent = $(this).find("th").eq(-2).before(newDinamicContent).prev();

        if (nroOutputYearPlans == 0) {
            var insertedSelect = $(selectContent).find("select");
            $(insertedSelect).kendoDropDownList().prev().addClass('btn');
            var dropdownlistOutputYearPlan = $(insertedSelect).data("kendoDropDownList");
            dropdownlistOutputYearPlan.bind("change", applyChangedYearInSelects);
            dropdownlistOutputYearPlan.bind("select", verifyChangedYearInSelect);
            dropdownlistOutputYearPlan.list.width(60);
            year = insertedSelect.val();
            if (interval.IntervalId == 3) {
                dropdownlistOutputYearPlan.enable(false);
            }

        }
        else {
            var insertedSelect = $(selectContent).find("li.small");
            year = insertedSelect.text();
        }
    });
}

function deleteCurrentOutputYear(element) {
    var outputyearplanindex = $(element).parents("th").attr("data-outputyearplanindex");
    var operationNumber = $("#Operation_OperationNumber").val();
    var outputYearPlanId = $("div.outputsContainer  > table > tbody > tr[data-outputindex] > td[data-rowcolumnrelated='" + outputyearplanindex + "'] > input.hiddenId:first").val();

    if (outputYearPlanId != undefined) {

        var isPepAndRmIntegrated = $("#IsIntegratedWithPep").val();
        
        if (isPepAndRmIntegrated == "True") {
            var hasUnconfirmedData = hasPepUnconfirmedData($("#OperationId").val());
            if (hasUnconfirmedData == true) {
                showPepUnconfirmedDataWarning(element);
                return false;
            }
        }

        var positionRelative = $(element).offset();
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: $("#IndexDeleteOutputYearColumnWarning").data("title"),
            draggable: false,
            resizable: false,
            content: $("#IndexDeleteOutputYearColumnWarning").data("route") + "?OutputYearPlanId=" + outputYearPlanId + "&OperationNumber=" + operationNumber,
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
        dialog.center();
        dialog.open();
    }
    else {

        $("div.outputsContainer > table > thead > tr").each(function (index) {
            $(this).find("> th[data-outputyearplanindex]:last").remove();
        });

        $("div.outputsContainer > table > tbody > tr[data-outputindex]").each(function (index) {
            $(this).find("> td[data-rowcolumnrelated]:last").remove();
        });

        $("div.outputsContainer > table > tbody > tr.nivel02").each(function (index) {
            var colspan = parseInt($(this).find("> td:first").attr("colspan"));
            colspan--;
            $(this).find("> td:first").attr("colspan", colspan);
            $(this).find("table.milestoneTable").each(function (index2) {
                $(this).find("> thead > tr > th[data-milestoneyearplanindex]:last").remove();
                $(this).find("> tbody > tr").each(function (index3) {
                    $(this).find("> td[data-rowcolumnrelated]:last").remove();
                });
            });
        });

        $("div.outputsContainer > table.grid").each(function (index) {

            $(this).find(" > thead > tr > th[data-outputyearplanindex]:last table > tbody > tr > td:eq(1)").
               html("<div title='Delete Year' onclick='deleteCurrentOutputYear(this);' class='deleteYear' style='display: none;'>X</div>");
        });
    }
}

function getMaxSelectedYearInOutputYearPlans(elementOutput) {
    var maxOuputYearPlan = -1;
    var isChanged = false;

    $(elementOutput).find("> thead > tr:eq(0) > th[data-outputyearplanindex]").each(function (index) {

        var currentSeletedValue = 0
        if ($(this).find("li.small > span > select").length > 0) {
            currentSeletedValue = parseInt($(this).find("li.small > span > select").val());
        }
        else {
            currentSeletedValue = parseInt($(this).find("li.small").text());
        }
        if (maxOuputYearPlan <= currentSeletedValue) {
            maxOuputYearPlan = currentSeletedValue;
            isChanged = true;
        }
    });
    if (isChanged) {
        maxOuputYearPlan++;
    }
    else {
        maxOuputYearPlan = parseInt($("#EndProjectYear").val());
    }
    return maxOuputYearPlan;
}

function applyChangedYearInSelects(selectElement) {
    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var container = $(selectElement.sender.element).closest("table.grid");
    var valueYear = parseInt(this.value());

    var currentSelectedYear = parseInt($('#currentSelectedYear').val());

    $("div.outputsContainer > table.grid").each(function (index) {
        var kendoDropDownList = $(this).find(" > thead > tr > th[data-outputyearplanindex]:first select:first").data("kendoDropDownList");

        if (kendoDropDownList.selectedIndex != selectElement.sender.selectedIndex) {
            kendoDropDownList.select(selectElement.sender.selectedIndex);
        }

        var currentTable = $(this);

        enabledInputsYearPlan(currentTable, interval, valueYear, 1);

        $(this).find(" > thead > tr > th[data-outputyearplanindex]:gt(0) li.small").each(function (index2) {
            var newYear = valueYear + index2 + 1;
            $(this).text(newYear);
            enabledInputsYearPlan(currentTable, interval, newYear, index2 + 2);
        });

        $(this).find(" > tbody > tr[data-outputindex]").each(function (index2) {

            $(this).find("> td[data-rowcolumnrelated]").each(function (index3) {

                var newSelectedYearValue = valueYear + index3;

                $(this).find("> input.hiddenYear").val(newSelectedYearValue);
            });
        });

        $(this).find(" > tbody > tr.nivel02 table.milestoneTable").each(function (index2) {

            $(this).find(" > thead > tr > th[data-milestoneyearplanindex]").each(function (index3) {
                $(this).text(valueYear + index3);
            });

            $(this).find(" > tbody > tr").each(function (index3) {
                $(this).find(" > td[data-rowcolumnrelated]").each(function (index4) {
                    var newSelectedYearValue = valueYear + index4;
                    if (newSelectedYearValue < currentSelectedYear) {
                        $(this).find("> input.hiddenId").val("0");
                    }
                    $(this).find("> input.hiddenYear:first").val(newSelectedYearValue);
                });
            });
        });
    });
}

function verifyChangedYearInSelect(selectElement) {
    var nroYears = $("div.outputsContainer:first > table > thead > tr > th[data-outputyearplanindex]").length;
    var EOPYear = parseInt($("#EndProjectYear").val());
    var currentSelectedYearValue = parseInt(this.value());
    $('#currentSelectedYear').val(currentSelectedYearValue);
    var newSelectedYearValue = parseInt(selectElement.item.text());

    if (((newSelectedYearValue + nroYears) > (EOPYear + 2) && EOPYear != 0) || EOPYear === -1) {
        selectElement.preventDefault();

        showNotification({
            message: msgVerifyChangedYear,
            type: 'information',
            autoClose: true,
            duration: 5
        });

        return false;
    }

    return true;
}

//--------------------------------------------General functions---------------------------------------
function rebindingValitadionFieldsForOutputs(form) {
    $(form).removeData("validator")
          .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
}
function chekIfItHasBeenChanges() {
    $(".minimizeTable").css("z-index", "0");

    var registerChangeMatrix = false;
    var interval = new Object();

    interval.IntervalId = parseInt($("#hdnIntervalId").val());
    interval.CycleId = parseInt($("#hdnCycleId").val());

    if (interval.IntervalId == 3) {
        var showMilestones = ($("#showMilestones").val().toLowerCase() === "true");

        if (isHasBeenModifications) {
            registerChangeMatrix = true;
            return registerChangeMatrix;
        }

        var query1 = "form > div.mod_tabla_impacts";
        var query2 = "form > div > input[type='hidden'].ComponentId";

        if ($(query1).length != $(query2).length) {
            registerChangeMatrix = true;
            return registerChangeMatrix;
        }

        query1 = "div.outputsContainer > table > tbody > tr.nivel01 > td.verticalShadow > input.OutputId";
        query2 = "div.outputsContainer > table > tbody > tr.nivel01";

        if ($(query1).length != $(query2).length) {
            registerChangeMatrix = true;
            return registerChangeMatrix;
        }

        if (showMilestones) {
            query1 = "div.outputsContainer > table > tbody > tr.nivel02 table.milestoneTable > tbody > tr > td.dato01 > input.MilestoneId";
            query2 = "div.outputsContainer > table > tbody > tr.nivel02 table.milestoneTable > tbody > tr.hoverMilestone";

            if ($(query1).length != $(query2).length) {
                registerChangeMatrix = true;
                return registerChangeMatrix;
            }
        }

        var currentServerYear = parseInt($("#CurrentServerYear").val());

        $("input.ExistingOriginal").each(function () {
            var currentValue = parseFloat($(this).autoNumeric('get'));
            var oldValue = parseFloat($(this).data("originalvalue"));

            if (isNaN(currentValue) && isNaN(oldValue)) {
                return true;
            }

            if (oldValue != currentValue) {
                registerChangeMatrix = true;
                return false;
            }
        });

        if (registerChangeMatrix) {
            return true;
        }

        $("input.ExistingAnnual").each(function () {
            if ($(this).hasClass("isauto")) {
                if ($(this).closest("tr[data-outputindex]").
                   find("td[data-rowcolumnrelated]> table > tbody > tr.rm_pa_row > td > label > input.valuechanged").length > 0) {

                    var currentValue = parseFloat($(this).autoNumeric('get'));
                    var originalValue = parseFloat($(this).data("originalvalue"));
                    var oldValue = parseFloat($(this).data("oldvalue"));
                    if (isNaN(currentValue) && isNaN(originalValue)) {
                        return true;
                    }
                    if (isNaN(currentValue) && isNaN(oldValue)) {
                        return true;
                    }

                    if (currentValue != originalValue && currentValue != oldValue) {
                        registerChangeMatrix = true;
                        return false;
                    }
                }
            }
            else {
                var currentValue = parseFloat($(this).autoNumeric('get'));
                var originalValue = parseFloat($(this).data("originalvalue"));
                var oldValue = parseFloat($(this).data("oldvalue"));
                if (isNaN(currentValue) && isNaN(originalValue)) {
                    return true;
                }
                if (isNaN(currentValue) && isNaN(oldValue)) {
                    return true;
                }
                if (currentValue != originalValue && currentValue != oldValue) {
                    registerChangeMatrix = true;
                    return false;
                }
            }
        });

        if (registerChangeMatrix) {
            return true;
        }

        $("input.ExistingAnnualMilestones").each(function () {

            if ($(this).hasClass("isauto")) {
                if ($(this).closest("tr[data-milestoneindex]").
                   find("td[data-rowcolumnrelated]> table > tbody > tr.rm_pa_row > td > label > input.valuechanged").length > 0) {

                    var currentValue = parseFloat($(this).autoNumeric('get'));
                    var originalValue = parseFloat($(this).data("originalvalue"));
                    var oldValue = parseFloat($(this).data("oldvalue"));

                    if (isNaN(currentValue) && isNaN(originalValue)) {
                        return true;
                    }
                    if (isNaN(currentValue) && isNaN(oldValue)) {
                        return true;
                    }

                    if (currentValue != originalValue && currentValue != oldValue) {
                        registerChangeMatrix = true;
                        return false;
                    }
                }
            }
            else {
                var currentValue = parseFloat($(this).autoNumeric('get'));
                var originalValue = parseFloat($(this).data("originalvalue"));
                var oldValue = parseFloat($(this).data("oldvalue"));

                if (isNaN(currentValue) && isNaN(originalValue)) {
                    return true;
                }
                if (isNaN(currentValue) && isNaN(oldValue)) {
                    return true;
                }

                if (currentValue != originalValue && currentValue != oldValue) {
                    registerChangeMatrix = true;
                    return false;
                }
            }
        });

    }
    return registerChangeMatrix;
}

function getCurrentListOfYears() {
    var existingOptions = new Array();
    var yearPlans = $("form div.outputsContainer > table > thead > tr");

    if (yearPlans.length > 0) {
        var selectedYearPlans = yearPlans[0];
        var numYears = 0;

        $.each(yearPlans, function (index, yearPlan) {
            var years = $(yearPlan).find("> th[data-outputyearplanindex]");
            if (years.length > numYears) {
                selectedYearPlans = yearPlan;
                numYears = years.length;
            }
        });

        $(selectedYearPlans).find("> th[data-outputyearplanindex]").each(function (index) {
            var option = null;
            if ($(this).find("select").length > 0) {
                option = $(this).find("select option:selected").text();

            }
            else {
                option = $(this).find("li.small").text();

            }

            existingOptions.push(option);

        });

    }
    return existingOptions;
}

function moveUpOutputs(element) {
    var currentOutput = $(element).closest("tr.nivel01");
    var currentTrackOrder = $(currentOutput).prev("tr.trackOrderRow");
    var previousOutput;
    var previosTrackOrder;
    if ($(currentTrackOrder).prev("tr.nivel02").length > 0) {
        previousOutput = $(currentTrackOrder).prev("tr.nivel02").prev("tr.nivel01");
        previosTrackOrder = $(previousOutput).prev("tr.trackOrderRow");
    }
    else {
        previousOutput = $(currentTrackOrder).prev("tr.nivel01");
        previosTrackOrder = $(previousOutput).prev("tr.trackOrderRow");

    }

    var currentMilestoneChildren = $(currentOutput).next("tr.nivel02");

    if (previousOutput.length > 0 && previosTrackOrder.length > 0) {
        var currentTrackOrderValue = $(currentTrackOrder).find("td").text();
        var previousTrackOrderValue = $(previosTrackOrder).find("td").text();

        // Start Pep Integration
        var isPepandRmIntegrated = $("#IsIntegratedWithPep").val();

        if (isPepandRmIntegrated == "True") {

            var canBeMoved = outputCanBeMoved(currentOutput, previousOutput);

            if (canBeMoved == false) {
                showMovePepOutputWarning(element);
                $(element).focus();
                return;
            } else {
                var currentOrder = $(currentOutput).find("td:first").find("input.output-order-number").val();
                var previousOrder = $(previousOutput).find("td:first").find("input.output-order-number").val();
                var currentParent = $(currentOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();
                var previousParent = $(previousOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();

                $(currentOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val(previousParent);
                $(previousOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val(currentParent);
                $(currentOutput).find("td:first").find("input.output-order-number").val(previousOrder);
                $(previousOutput).find("td:first").find("input.output-order-number").val(currentOrder);
            }
        }
        // End Pep Integration

        $(currentTrackOrder).find("td").text(previousTrackOrderValue);
        $(previosTrackOrder).find("td").text(currentTrackOrderValue);

        $(currentTrackOrder).insertBefore(previosTrackOrder);
        $(currentOutput).insertBefore(previosTrackOrder);
        if (currentMilestoneChildren.length > 0) {
            $(currentMilestoneChildren).insertBefore(previosTrackOrder);
        }
    }
}

function moveDownOutputs(element) {
    var currentOutput = $(element).closest("tr.nivel01");
    var currentTrackOrder = $(currentOutput).prev("tr.trackOrderRow");
    var nextOutput;
    var nextTrackOrder;
    if ($(currentOutput).next("tr.nivel02").length > 0) {
        nextTrackOrder = $(currentOutput).next("tr.nivel02").next("tr.trackOrderRow");
        nextOutput = $(nextTrackOrder).next("tr.nivel01");
    }
    else {
        nextTrackOrder = $(currentOutput).next("tr.trackOrderRow");
        nextOutput = $(nextTrackOrder).next("tr.nivel01");
    }

    if ($(nextOutput).next("tr.nivel02").length > 0) {
        nextOutput = $(nextOutput).next("tr.nivel02");
    }

    var currentMilestoneChildren = $(currentOutput).next("tr.nivel02");

    if (nextOutput.length > 0 && nextTrackOrder.length > 0) {
        if (currentMilestoneChildren.length > 0) {
            $(currentMilestoneChildren).insertAfter(nextOutput);
        }

        var currentTrackOrderValue = $(currentTrackOrder).find("td").text();
        var nextTrackOrderValue = $(nextTrackOrder).find("td").text();

        // Start Pep Integration
        var isPepandRmIntegrated = $("#IsIntegratedWithPep").val();

        if (isPepandRmIntegrated == "True") {

            var canBeMoved = outputCanBeMoved(currentOutput, nextOutput);

            if (canBeMoved == true) {
                var currentOrder = $(currentOutput).find("td:first").find("input.output-order-number").val();
                var nextOrder = $(nextOutput).find("td:first").find("input.output-order-number").val();
                var currentParent = $(currentOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();
                var nextParent = $(nextOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();

                $(currentOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val(nextParent);
                $(nextOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val(currentParent);
                $(currentOutput).find("td:first").find("input.output-order-number").val(nextOrder);
                $(nextOutput).find("td:first").find("input.output-order-number").val(currentOrder);
            } else {
                showMovePepOutputWarning(element);
                $(element).focus();
                return;
            }
        }
        // End Pep Integration

        $(currentTrackOrder).find("td").text(nextTrackOrderValue);
        $(nextTrackOrder).find("td").text(currentTrackOrderValue);


        $(currentOutput).insertAfter(nextOutput);
        $(currentTrackOrder).insertAfter(nextOutput);
    }
}

function enabledInputsYearPlan(currentTable, interval, year, index) {
    var isBlockedStartUpPlan = BlockFieldStartUpPlan(interval);;
    var isBlockedAnnualPlan = BlockFieldAnnualPlan(interval, year);;
    var isBlockedActualPlan = BlockFieldActualPlan(interval, year);;

    currentTable.find('[data-outputindex]').each(function () {
        var inputP = $(this).find('[data-rowcolumnrelated=' + String(index) + '] tr.rm_p_row input');
        var inputPa = $(this).find('[data-rowcolumnrelated=' + String(index) + '] tr.rm_pa_row input');
        var inputAc = $(this).find('[data-rowcolumnrelated=' + String(index) + '] tr.rm_ac_row input');

        if (isBlockedStartUpPlan) {
            inputP.addClass('bcGray');
            inputP.prop('readonly', 'readonly');
        } else {
            inputP.removeClass('bcGray');
            inputP.removeProp('readonly');
        }

        if (isBlockedAnnualPlan) {
            inputPa.addClass('bcGray');
            inputPa.prop('readonly', 'readonly');
        } else {
            inputPa.removeClass('bcGray');
            inputPa.removeProp('readonly');
        }

        if (isBlockedActualPlan) {
            inputAc.addClass('bcGray');
            inputAc.prop('readonly', 'readonly');
        } else {
            inputAc.removeClass('bcGray');
            inputAc.removeProp('readonly');
        }
    });
}

function outputCanBeMoved(sourceOutput, targetOutput) {

    var newComponent = isNewComponent($(sourceOutput).closest("table").closest(".mod_tabla_impacts"));

    if (newComponent == true)
        return true;

    var sourceOutputId = $(sourceOutput).find("td:first").find("input[id$='__OutputId']");
    var targetOutputId = $(targetOutput).find("td:first").find("input[id$='__OutputId']");
    var sourceParentPepTaskId = $(sourceOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();
    var targetParentPepTaskId = $(targetOutput).find("td:first").find("input[id$='__ParentPeptaskId']").val();

    if (sourceParentPepTaskId != "-1" && sourceOutputId.length > 0 && targetParentPepTaskId != "-1" && targetOutputId.length > 0)
        return true;
    
    var canBeMoved = false;

    var response = existsOutputsRelatedToSubcomponents($(sourceOutput).closest("table").closest(".mod_tabla_impacts"));

    if (response.isValid == true && response.data == false) {

        if (sourceOutputId.length > 0 || targetOutputId.length > 0) {
            return true;
        }
    }

    return canBeMoved;
}

function isNewComponent(componentContainer) {
    var componentId = $(componentContainer).find("input[id$='__ComponentId']");
    return componentId.length == 0;
}

function existsOutputsRelatedToSubcomponents(componentContainer) {

    var componentId = $(componentContainer).find("input[id$='__ComponentId']").val();

    var path = $("#hdn-check-subcomponents-path").val();

    var request = $.ajax({
        url: path,
        data: JSON.stringify({ componentId: componentId }),
        dataType: "json",
        contentType: "application/json",
        type: 'post'
    });

    var response = {
        isValid: true,
        data: null
    };

    request.done(function (data) {
        response.data = data;
    }).fail(function () {
        response.isValid = false;
    });

    return response;
}

function showMovePepOutputWarning(targetElement)
{
    var positionRelative = $(targetElement).offset();
    var positionTop = positionRelative.top - 100;
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

    var kendoWindow = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: "Warning",
        draggable: false,
        resizable: false,
        pinned: true,
        actions: ["Close"],
        modal: true,
        visible: false,
        content: {
            template: $("#output-pep-move-template").html()
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");

    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");

    kendo.bind(kendoWindow.element, null);

    kendoWindow.open();
}

function showPepUnconfirmedDataWarning(targetElement) {
    var positionRelative = $(targetElement).offset();
    var positionTop = positionRelative.top - 100;
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

    var kendoWindow = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: "Warning",
        draggable: false,
        resizable: false,
        pinned: true,
        actions: ["Close"],
        modal: true,
        visible: false,
        content: {
            template: $("#pep-unconfirmed-data-template").html()
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");

    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");

    kendo.bind(kendoWindow.element, null);

    kendoWindow.open();
}

function hasPepUnconfirmedData(operationId) {

    var path = $("#hdn-pep-unconfirmed-data").val();

    var request = $.ajax({
        url: path,
        data: JSON.stringify({ operationId: operationId }),
        dataType: "json",
        contentType: "application/json",
        type: 'post'
    });

    var response;
    
    request.done(function (data) {
        response = data;
    }).fail(function () {
        response = null;
    });

    return response;
}

//--------------------------------------------milestones---------------------------------------------
function addNewMilestone(element) {
    var componentIndex = $(element).closest("div.mod_tabla_impacts").attr("data-componentindex");
    var outputIndex = $(element).closest("tr[data-outputindex]").attr("data-outputindex");
    var referenceYear = $(element).closest("tr[data-outputindex]").find("input[data-referenceyear]")
        .attr('data-referenceyear');

    var interval = new Object();
    interval.IntervalId = $("#hdnIntervalId").val();
    interval.CycleId = $("#hdnCycleId").val();

    var totalYears = getCurrentListOfYears();
    var source, template, context, newDinamicContent;
    var milestoneContainer = null;

    if (!$(element).closest("tr[data-outputindex]").next().hasClass("nivel02")) {

        var options = new Array();
        var newObject;
        for (var i = 0; i < totalYears.length; i++) {
            newObject = { milestoneyearindex: i, year: totalYears[i] };
            options.push(newObject);
        }
        source = $("#milestoneHeader-template").html();
        template = Handlebars.compile(source);
        context = {
            EndProjectYear: $("#EndProjectYear").val(),
            options: options,
            colspanmilestone: (totalYears.length + 5)
        };
        newDinamicContent = template(context);
        $(newDinamicContent).insertAfter($(element).closest("tr[data-outputindex]"));
        milestoneContainer = $(element).closest("tr[data-outputindex]").next();
    }

    milestoneContainer = milestoneContainer === null ?
        $(element).closest("tr[data-outputindex]").next() : milestoneContainer;
    var resultArray = $(milestoneContainer)
        .find("table.milestoneTable:first > tbody > tr[data-milestoneindex] ")
        .map(function () { return $(this).attr("data-milestoneindex"); }).get();
    var maxOutputMilestones = 0;

    if (resultArray.length > 0) {
        maxOutputMilestones = Math.max.apply(null, resultArray);
        maxOutputMilestones++;
    }

    options = new Array();

    $(milestoneContainer).find("table.milestoneTable:first > thead > tr > th[data-milestoneyearplanindex]")
    .each(function (index) {
        index++;
        var isActualValueBlock = false;
        var isOriginalValueBlock = false;
        var isAnnualValueBlock = false;

        isOriginalValueBlock = BlockFieldStartUpPlan(interval);
        isAnnualValueBlock = BlockFieldAnnualPlan(interval, parseInt($(this).text().trim()));
        isActualValueBlock = BlockFieldActualPlan(interval, parseInt($(this).text().trim()));

        var newObject = {
            milestoneYearPlanIndex: index,
            year: $(this).text().trim(),
            isOriginalValueBlock: isOriginalValueBlock,
            isAnnualValueBlock: isAnnualValueBlock,
            isActualValueBlock: isActualValueBlock,
            componentIndex: componentIndex,
            outputIndex: outputIndex,
            milestoneIndex: maxOutputMilestones,
            referenceYear: referenceYear
        };
        options.push(newObject);
    });

    var source = $("#Milestone-template").html();
    var template = Handlebars.compile(source);
    var isAutoCalc = $(element)
        .closest("tr[data-outputindex]").find(".rm_p_row").find("input").last().prop("readonly");

    var context = {
        componentIndex: componentIndex,
        outputIndex: outputIndex,
        milestoneIndex: maxOutputMilestones,
        milestoneYearPlanIndexMax: totalYears.length,
        options: options,
        referenceYear: referenceYear,
        isAutoCalc: isAutoCalc
    };

    var newDinamicContent = template(context);
    var tbodyMilestones = $(milestoneContainer).find(" table.milestoneTable > tbody");
    var toBindNumberEvent = $(tbodyMilestones).append(newDinamicContent);

    $.each($(tbodyMilestones).find("> tr.hoverMilestone:last").find("input.numberInput"), function (index, input) {
        var year = parseInt($(this).closest("table").closest("td.dato07").find('input[type="hidden"][id$="__Year"]').val());

        if (year >= 2014) {
            $(this).autoNumeric('init', { vMin: '0000000000000000.00', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });

        } else {
            $(this).autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' })
        }
    });

    var milestonesTable = $(milestoneContainer).find(" table.milestoneTable");

    rebindingValitadionFieldsForOutputs($("#outputsTarget"));
    
}
