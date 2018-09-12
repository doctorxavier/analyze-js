$(document).ready(function () {
    paciReportEventHandler();
});


function paciReportEventHandler() {
    hideLoaderOptional();

    $(".movableCellContent").mouseenter(function (element) {
        if ($(this).find("span").length > 0) {
            $(this).find("span").css("display", "block");
        }
    });
        
    $(".movableCellContent").mouseleave(function (element) {
        if ($(this).find("span").length > 0) {
            $(this).find("span").css("display", "none");
        }
    });

    $('#btnComplete').click(function () {
        var url = basePath + "/paci/report/CompletePaci?operationNumber=" + operationNumber;
        showLoaderOptional();
        var dataForm = $("#frmPaciReportEditMode");
        $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
            if (jqXHR.status == 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }
            $("#btnPaciReportSave").click();
        }).fail(function (jqXHR, textStatus) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
        });
    });

    bindHandlers();
    handleReportContainerColapse();
}

function handleReportContainerColapse() {
    var containerStrList = '';
    $('div.reportmodulecontainer').each(function (element) {        
        var containerId = this.id;        
        containerStrList += containerId + ',';
    });
    $('.btnCollapseAll').attr("dd-contenedores", containerStrList);
}

function completeReport() {
    $("#modalReportComplete").modal();
}

function reviewReport() {
    var url = basePath + "/paci/report/ReviewPaci?operationNumber=" + operationNumber;
    showLoaderOptional();
    var dataForm = $("#frmPaciReportEditMode");
    $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
        if (jqXHR.status == 204) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
            return;
        }        
        $("#btnPaciReportCancel").click();
    }).fail(function (jqXHR, textStatus) {
        hideLoaderOptional();
        errorBar(jqXHR.statusText, 60, true);
    });
}

function evaluateModule(moduleId, isComplete) {
    var url = basePath + "/paci/report/EvaluateModule?operationNumber=" + operationNumber + "&moduleId=" + moduleId;
    showLoaderOptional();
    var dataForm = $("#frmPaciReportEditMode");
    $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
        if (jqXHR.status == 204) {
            hideLoaderOptional();
            $("#modalEvaluateModule").modal();
            return;
        }
        var tableName = "table_assessment_" + moduleId;
        var body = $("#" + tableName).find('tbody').replaceWith(data);
        if (isComplete) {
            if (isAllEvaluated) {
                $("button#btnPaciComplete").show();
            }
        } else {
            if (isAllEvaluated) {
                $("button#btnPaciReview").show();
            }
        }
        paciReportEventHandler();
    }).fail(function (jqXHR, textStatus) {
        hideLoaderOptional();
        $("#modalEvaluateModule").modal();
    });
}

function moveAssessmentRight(element) {    
    var newType = $(element).attr('data-newType');
    var currentRow = $(element).closest('tr');
    var moduleId = $(currentRow).attr('data-module-id');
    var assessmentId = $(currentRow).attr('data-assessment-id');
    var url = basePath + "/paci/report/ChangeAssessmentType?operationNumber=" + operationNumber + "&moduleId=" + moduleId + "&newtype=" + newType + "&assessmentId=" + assessmentId;
    var dataForm = $("#frmPaciReportEditMode");
    $.post(url, dataForm.serializeArray(), function (data) {
        $(currentRow).replaceWith(data);
        paciReportEventHandler();
    });
}

function moveAssessmentLeft(element) {   
    var newType = $(element).attr('data-newType');
    var currentRow = $(element).closest('tr');
    var moduleId = $(currentRow).attr('data-module-id');
    var assessmentId = $(currentRow).attr('data-assessment-id');   
    var url = basePath + "/paci/report/ChangeAssessmentType?operationNumber=" + operationNumber + "&moduleId=" + moduleId + "&newtype=" + newType + "&assessmentId=" + assessmentId;
    var dataForm = $("#frmPaciReportEditMode");
    $.post(url, dataForm.serializeArray(), function (data) {
        $(currentRow).replaceWith(data);        
        paciReportEventHandler();
    });
}

function ExportFilePaciReport(source)
{
    var formatDoc = source.attr('data-filetype');
    var paciId = source.attr('data-id');
    var urlReportFile = basePath + "/paci/report/DownloadPaciReportFile?paciId=" + paciId + "&formatType=" + formatDoc + "&operationNumber=" + operationNumber;
    window.open(urlReportFile, "_blank");
}

    function paciAjaxOnBegin() {
        showLoaderOptional();
    }

    function paciAjaxOnComplete() {
        paciReportEventHandler();
    }

    function reportAjaxOnFailure(xhr) {
        hideLoaderOptional();
        showError(xhr.statusText)
    };