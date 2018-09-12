var materializedDelete;
var materializedContainerName;
var materializedStageCategory;
var checkBoxElementSelected;
var materializedCausesDelay;
var matDel;

$(document).ready(function () {
    riskMaterializedEventHandler();
});

function riskMaterializedEventHandler() {
    hideLoaderOptional();    
    
    $("#tblIssues").sortableConfluence();
    bindHandlers();
}

function riskMaterializedAjaxOnBegin() {
    if (!Validation.Container()) {
        return false;
    } else {
        showLoaderOptional();
    }
}

function riskMaterializedAjaxOnBeginNoValidate() {
    showLoaderOptional();
}

function riskMaterializedAjaxOnComplete() {
    riskMaterializedEventHandler();
}

function riskMaterializedAjaxOnFailure(xhr) {
    hideLoaderOptional();
    if (xhr.statusCode().status == 400) {
        var modalContent = $("#errorSaveMaterialized div.modal-body");
        modalContent.empty();
        modalContent.html("<p>" + xhr.statusText + "</p>");
        riskMatrixEventHandler();
        $("#errorSaveMaterialized").modal()
    } else {
        showError(xhr.statusText)
    }
}

$(document).off('change', '.riskMaterializedOptionsSectionSelector input[type="checkbox"]');
$(document).on('change', '.riskMaterializedOptionsSectionSelector input[type="checkbox"]', function () {
    var element = $(this);
    var riskId = element.attr("data-issueId");
    materializedContainerName = "#riskMaterializedOptionsSection_" + riskId;    
    materializedCausesDelay = "#riskMaterializedDelay_" + riskId;
    if (element.prop('checked') === false) {
        checkBoxElementSelected = element;
        $("#deletefrModal").modal();
    }
    else{       
        $(materializedContainerName).show();
        $(materializedCausesDelay).show();
        materializedContainerName = null;
        materializedCausesDelay = null;
    }
    
});

function newIssue(element) {
    showLoaderOptional();
    var id = $(element).attr('dd-table');
    var url = basePath + "/riskmatrix/Materialized/NewIssue?operationNumber=" + operationNumber;
    var dataForm = $("#frmriskMaterializedEditMode");
    $.post(url,
        dataForm.serializeArray(),
        function (data, textStatus, jqXHR) {
            if (jqXHR.status === 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }
            $("#tblIssues").find("tbody").append(data);
            riskMaterializedEventHandler();
        });
}

function deleteMaterialized(element) {
    var materializedId = $(element).closest('tr').attr('data-id');
    if (materializedId > 0) {
        materializedDelete = materializedId;
        matDel = element;
        $("#deleteWarning").modal();
    } else {
        var trBack = $(element).closest('tr');
        $(element).closest('tbody').find('[data-row-parent-id="' + materializedId + '"]').remove();
        $(element).closest('tr').remove();
        fixAttributesIdAndNameMaterialized(trBack);
    }
}

function ExportFileMaterializedReport(source) {
    var formatDoc = source.attr('data-filetype');
    var urlReportFile = basePath + "/RiskMatrix/Materialized/DownloadRiskMaterializedExportFile?operationNumber=" + operationNumber + "&formatType=" + formatDoc;
    window.open(urlReportFile, "_blank");
}

$('#btnDeleteRowMaterializedYes').on("click", function () {
    showLoaderOptional();
    var url = basePath + "/riskmatrix/Materialized/DeleteMaterialized?operationNumber=" + operationNumber + "&materializedId=" + materializedDelete;
    var dataForm = $("#frmPaciEditMode");
    $.post(url,
        dataForm.serializeArray(),
        function (data) {
            if (data.IsValid) {
                var rows = $('#tblIssues tbody');
                var trBack = $(matDel).closest('tr');
                
                rows.find('[data-row-parent-id="' + materializedDelete + '"]').remove();
                rows.find('[data-id="' + materializedDelete + '"]').remove();           
                fixAttributesIdAndNameMaterialized(trBack);

                hideLoaderOptional();
            } else {
                showError(data.ErrorMessage);
            }
            materializedDelete = 0;
            matDel = null;
        });
});

$('#btnDeleteRowMaterializedNo').on("click", function () {
    materializedDelete = 0;
});

$(document).off('change', '#frCategories input[type=checkbox]');
$(document).on('change', '#frCategories input[type=checkbox]', function () {   
    var element = $(this);    
    if (element.prop('checked') === true)
    {
        var parent = element.parents('#frCategories');
        var allElements = parent.find('input[type=checkbox]');
        var count = 0;
        allElements.each(function (index) {
            var currentItem = $(this);
            if (currentItem.prop('checked') == true)
            {
                count++;
            }

            if (count > 3) {
                element.prop('checked', false);
                return;
            }
        });
    }
});

$('#btnDeleteFrYes').on("click", function () {

    Validation.Destroy(materializedContainerName);
    $(materializedContainerName).hide();
    materializedContainerName = null;
    $(materializedCausesDelay).hide();
    materializedCausesDelay = null;
});

$('#btnDeleteFrNo').on("click", function () {
    $(checkBoxElementSelected).prop("checked", true);
    $(checkBoxElementSelected).val(true);
    checkBoxElementSelected = null;
    
});

function saveMaterializedWithValidate() {
    if (Validation.Container()) {
        $("#frmriskMaterializedEditMode").submit();
    }
}

function fixAttributesIdAndNameMaterialized(tr) {

    var idMatDel = $(tr.find('td')[0]).find('input').attr('id');
    var nameMatDel = $(tr.find('td')[0]).find('input').attr('id');
    var indexId = idMatDel.match(/(\d+)/g)[0];
    var indexName = nameMatDel.match(/(\d+)/g)[0];

    indexId++;
    var countIdMat = $('[id*="MaterializedRisks_' + indexId + '__"').length;

    while (countIdMat > 0) {
        var othersFields = $('[id*="MaterializedRisks_' + indexId + '__"');

        $.each(othersFields, function (index, value) {
            var actualId = $(value).attr('id');
            var indexMat = actualId.match(/(\d+)/g)[0];
            var newIndex = indexMat - 1;
            var newId = actualId.replace('MaterializedRisks_' + indexMat + '__', 'MaterializedRisks_' + newIndex + '__');
            $(value).attr('id', newId);
        });

        indexId++
        countIdMat = $('[id*="MaterializedRisks_' + indexId + '__"').length;
    }

    indexName++;
    var countNameMat = $('[name*="MaterializedRisks[' + indexName + ']"').length;

    while (countNameMat > 0) {
        var othersFields = $('[name*="MaterializedRisks[' + indexName + ']"');

        $.each(othersFields, function (index, value) {
            var actualName = $(value).attr('name');
            var indexMat = actualName.match(/(\d+)/g)[0];
            var newIndex = indexMat - 1;
            var newName = actualName.replace('MaterializedRisks[' + indexMat + ']', 'MaterializedRisks[' + newIndex + ']');
            $(value).attr('name', newName);
        });

        indexName++
        countNameMat = $('[name*="MaterializedRisks[' + indexName + ']"').length;
    }

    Validation.Destroy();
    Validation.Init();
}