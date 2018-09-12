
function BlockFieldStartUpPlan(interval) {
    var currentYear = new Date().getFullYear();
    var editableField = false;

    if (interval.IntervalId == 3) {
        editableField = true;
    }

    return editableField;
}

function BlockFieldAnnualPlan(interval, yearFieldValue) {
    var currentYear = new Date().getFullYear();
    var lastYear = currentYear - 1;
    var editableField = false;

    if (interval.IntervalId == 3) {
        if (interval.CycleId == 2) {
            if (yearFieldValue <= lastYear) {
                editableField = true;
            }
        }
        else if (interval.CycleId == 3) {
            if (yearFieldValue <= currentYear) {
                editableField = true;
            }
        }
    }

    return editableField;
}

function BlockFieldActualPlan(interval, yearFieldValue) {
    var currentYear = new Date().getFullYear();
    var lastYear = currentYear - 1;
    var editableField = false;

    if (interval.IntervalId == 1 || interval.IntervalId == 2) {
        if (yearFieldValue > currentYear) {
            editableField = true;
        }
    }
    else if (interval.IntervalId == 3) {
        if (interval.CycleId == 2) {
            if (yearFieldValue < lastYear || yearFieldValue > currentYear) {
                editableField = true;
            }
        }
        else if (interval.CycleId == 3) {
            if (yearFieldValue <= lastYear || yearFieldValue > currentYear) {
                editableField = true;
            }
        }
    }

    return editableField;
}

function BlockFieldStartUpPlanEOP(interval) {
    var editableField = true;

    if (interval.IntervalId == 2 || interval.IntervalId == 3) {
        editableField = false;
    }

    return editableField;
}

function BlockFieldAnnualPlanEOP(interval, yearPlan) {
    var editableField = true;
    return editableField;
}

function BlockFieldActualPlanEOP(interval, yearPlan) {
    var editableField = true;
    return editableField;
}

function loadDataFromPep(isFinancial) {
    var importUrl = null;

    if (isFinancial) {
        var valueshowMilestones = $("#showMilestones").is(':checked');
        var editMilestones = valueshowMilestones ? true : false;
        importUrl = $("a[name='lnk-import-financial-pep-data']").data("route") + '&editMilestones=' + editMilestones + '&importDataFromPep=true';
    } else {
        var valueShowMilestones = $("#showMilestones").is(':checked');
        var valueShowDisaggregation = $("#showDisaggregation").is(':checked');
        if (valueShowMilestones || (!valueShowMilestones && !valueShowDisaggregation)) {
            importUrl = $("a[name='lnk-import-physical-pep-data']").data("route") + '&editMilestones=true&editDisaggregation=false&importDataFromPep=true';
        }
        else {
            importUrl = $("a[name='lnk-import-physical-pep-data']").data("route") + '&editMilestones=false&editDisaggregation=true&importDataFromPep=true';
        }
    }

    closeKendoWindow();
    $("#layoutLoadingDiv").show();
    redirectPage(importUrl);
}

function checkImportDataFromPep(title, message, cancelTitle, confirmTitle, isFinancial) {
    var confirmContent = null;

    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $('body').append('<div class="ui-widget-overlay ui-front"></div>');

    confirmContent += '<div class="dinamicModal">';
    confirmContent += '	<div style="padding: 20px;">' + message + '</div>';
    confirmContent += '	<div class="pie">';
    confirmContent += '		<div class="botones">';
    confirmContent += '			<a title="Cancel" class="cancel k-i-close" href="javascript:void(0)" onclick="closeKendoWindow()">' + cancelTitle + '</a>';
    confirmContent += '			<label>';
    confirmContent += '				<input type="button" value="' + confirmTitle + '" class="btn-primary" onclick="loadDataFromPep(' + isFinancial + ')">';
    confirmContent += '			</label>';
    confirmContent += '		</div>';
    confirmContent += '	</div>';
    confirmContent += '</div>';

    $('body').append(confirmContent);

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
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    dialog.center();
    dialog.open();

    resizeIframeCloud();
}

function closeKendoWindow() {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
}
