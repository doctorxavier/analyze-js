// Globals Variables
var statusChackActivate = false;
var elementNewNote = null;
var containerNewNote = null;
var elementButton = null;
var elementDelDocument = null;


function questionnaireAjaxOnFailure(xhr) {
    hideLoaderOptional();
    showError(xhr.statusText)
}

//add sortable function to all tables
$(document).ready(function () {
    paciEventHandler();
});

$(document).off('change', '.chekcSide input[type=checkbox]');
$(document).on('change', '.chekcSide input[type=checkbox]', function () {
        var redioId = $(this).attr("data-id");
        var descriptionId = redioId.replace("IsSelected", "Description");
        if ($("#" + redioId).val() == "True") {
            $("#" + redioId).val("False");
            this.checked = false;
            $("#" + descriptionId).attr("disabled", "disabled");
            $("#" + descriptionId).removeAttr('data-parsley-required');
            $("#" + descriptionId).removeClass("validation-fail");
            $(".validation-element").addClass("hide");
            $("#" + descriptionId).val("");
        } else {
            $("#" + redioId).val("True");
            this.checked = true;
            $("#" + descriptionId).removeAttr("disabled", "disabled");
            $("#" + descriptionId).attr('data-parsley-required', 'true');
        }
        Validation.Destroy($('#' + redioId).parent());
        Validation.Init($('#' + redioId).parent());
});

$(document).off('change', '.checkExportable input[type=checkbox]');
$(document).on('change', '.checkExportable input[type=checkbox]', function () {
    var valueCheckId = $(this).attr("id");
    if ($("#" + valueCheckId).val() == "true") {
        $("#" + valueCheckId).val("false");
        this.checked = false;
    } else {
        $("#" + valueCheckId).val("true");
        this.checked = true;
    }
});

$(document).off('change', 'input[type=checkbox]#DisplayModule_IsInactivate');
$(document).on('change', 'input[type=checkbox]#DisplayModule_IsInactivate', function () {
    var valueCheckId = $(this).attr("id");
    if ($("#" + valueCheckId).val() == "true") {
        $("#" + valueCheckId).val("false");
        this.checked = false;
    } else {
        $("#inactivePaciWarning").modal();
        checkedInactive(this, valueCheckId);
    }
});

$(document).off('change', 'input[type=radio]');
$(document).on('change', 'input[type=radio]', function () {
    var nameCurrentRadioButton = $(this).attr('name');
    var parentLabel = $(this).parent();
    var parentDiv = parentLabel.parent().parent();
    var brotherRadioButtons = parentDiv.find('[name=' + nameCurrentRadioButton + ']');

    brotherRadioButtons.each(function (index) {
        var redioId = $(this).attr("data-id");
        var radioDescription = redioId.replace("IsSelected", "Description");
        $("#" + redioId).val("False");
        this.checked = false;
        $("#" + radioDescription).attr("disabled", "disabled");
        $("#" + radioDescription).removeAttr('data-parsley-required');
        $("#" + radioDescription).removeClass("validation-fail");
        $(".validation-element").addClass("hide");
        $("#" + radioDescription).val("");
    });

    var redioId = $(this).attr("data-id");
    var radioDescription = redioId.replace("IsSelected", "Description");
    $("#" + redioId).val("True");
    this.checked = true;
    $("#" + radioDescription).removeAttr("disabled", "disabled");
    $("#" + radioDescription).attr('data-parsley-required', 'true');
    Validation.Destroy(parentDiv);
    Validation.Init(parentDiv);
})

function questionnaireFeatures() {
    $(".close").on("click", function () {
        closeModalDocument($("#paciDocumentModal"));
    });

    $("[data-id=cancelButton]").on("click", function () {
        closeModalDocument($("#paciDocumentModal"));
    });

    $("#deleteNoteWarning").find("#btnYes").on("click", function () {
        showLoaderOptional();
        var questionId = elementNewNote.closest(".divContainerQuestions").attr("id");
        var commentId = elementNewNote.closest(".internalNote").attr("id");
        var url = basePath + "/paci/questionnaire/DeleteNote?operationNumber=" + operationNumber + "&questionId=" + questionId + "&commentId=" + commentId;
        var dataForm = $("#frmPaciViewMode");

        $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
            if (jqXHR.status == 500) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            } else if (!data.IsValid) {
                hideLoaderOptional();
                errorBar(data.ErrorMessage, 60, true);
                return;
            }

            elementNewNote.closest(".textAreaNote").html("");
            hideLoaderOptional();
        }).fail(function (jqXHR, textStatus) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
        });
    });

    $("#deleteDocumentWarning").find("#btnYes").on("click", function () {
        showLoaderOptional();
        var documentId = elementButton.attr("data-documentId");
        var questionId = elementButton.attr("data-questionId");

        if (documentId != 0) {
            var url = basePath + "/paci/questionnaire/DeleteDocument?operationNumber=" + operationNumber + "&questionId=" + questionId + "&documentId=" + documentId;
            var dataForm = $("#frmPaciViewMode");

            $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
                if (jqXHR.status == 204) {
                    hideLoaderOptional();
                    errorBar(jqXHR.statusText, 60, true);
                    return;
                } else if (!data.IsValid) {
                    hideLoaderOptional();
                    errorBar(data.ErrorMessage, 60, true);
                    return;
                }

                containerNewNote.remove();
                hideLoaderOptional();
            }).fail(function (jqXHR, textStatus) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
            });
        } else {
            containerNewNote.remove();
            hideLoaderOptional();
        }

        elementButton = null;
    });
}

function paciEventHandler() {
    hideLoaderOptional();
    handleColapse();
    handleSortable();
    bindHandlers();
    handlePaging();
}

function handleColapse() {
    var containerStrList = '';
    $('div.questionnairemodulecontainer').each(function (element) {
        var containerId = this.id;
        containerStrList += containerId + ',';
    });
    $('.btnCollapseAll').attr("dd-contenedores", containerStrList);
}

function handleSortable() {
    $('table').each(function (element) {
        var tableId = this.id;
        $('#' + tableId).sortableConfluence(false, 1);
    });
}

function handlePaging() {
    $('table').each(function (element) {
        $(this).paginationConfluence(10);
    });
}

function addDocument(button) {
    elementButton = button;
    $('#dmDocumentModal').modal();
}

function exportFileQuestionnaire(source) {
    var formatDoc = source.attr('data-filetype');
    var paciId = source.attr('data-id');
    var urlReportFile = basePath + "/paci/questionnaire/DownloadQuestionnaireExportFile?paciId=" + paciId + "&formatType=" + formatDoc + "&operationNumber=" + operationNumber;
    window.open(urlReportFile, "_blank");
}

function newNote(element) {
    containerNewNote = $(element).closest("[name=noteFather]").find(".textAreaNewNote");
    if (containerNewNote.html().trim() == "") {
        showLoaderOptional();
        elementNewNote = element;
        var paciConditionId = element.closest(".divContainerCondition").attr("id");
        var questionId = element.closest(".divContainerQuestions").attr("id");
        var url = basePath + "/paci/questionnaire/NewNote?operationNumber=" + operationNumber + "&paciConditionId=" + paciConditionId + "&questionId=" + questionId;
        var dataForm = $("#frmPaciViewMode");

        $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
            if (jqXHR.status == 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }

            elementNewNote.attr("disabled", "disabled");
            containerNewNote.append(data);
            paciEventHandler();
            containerNewNote = null;
        }).fail(function (jqXHR, textStatus) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
        });
    }
}

function deleteNote(element) {
    showLoaderOptional();
    containerNewNote = $(element).closest(".textAreaNote");
    if (containerNewNote.find("div.internalNote").attr("id") != 0) {
        elementNewNote = element;
        $("#deleteNoteWarning").modal();
    } else {
        var buttonNewNote = $(element).closest("[name=noteFather]").find("button#newNote");
        buttonNewNote.removeAttr("disabled");
        element.closest(".textAreaNote").html("");
    }

    hideLoaderOptional();
}

function deleteDocument(element) {
    containerNewNote = $(element).closest("tr");
    var DocumentId = containerNewNote.attr("data-id");
    elementButton = element;
    $("#deleteDocumentWarning").modal();
}

function getDocumentNumberAndName(object, typeTab, documentName) {
    var documentData = { documentName: "", documentNumber: "" };
    if (typeTab == "added") {
        documentData["documentNumber"] = object[0];
        documentData["documentName"] = documentName;
    } else if (typeTab == "selected") {
        documentData["documentName"] = object[0].DocumentName;
        documentData["documentNumber"] = object[0].DocumentNumber;
    }
    return documentData;
}

function renderizeRow(documentNumber, typeTab, documentName) {
    
    if (documentNumber.length > 0) {
        var documentData = getDocumentNumberAndName(documentNumber, typeTab, documentName);
        var docNumber = documentData["documentNumber"];
        var docName = documentData["documentName"];
        if (docNumber != "" && docName != "") {
            var paciConditionId = elementButton.attr("data-paciConditionId");
            var questionId = elementButton.attr("data-questionId");
            
            var dataForm = $("#frmPaciViewMode");

            var url = encodeURI(basePath + "/paci/questionnaire/AddDocument?operationNumber=" + operationNumber + "&paciConditionId=" + paciConditionId + "&questionId=" + questionId + "&documentNumber=" + docNumber + "&documentName=" + docName);

            $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
             
                if (jqXHR.status == 204) {
                    hideLoaderOptional();
                    errorBar(jqXHR.statusText, 60, true);
                    return;
                }

                var table = elementButton.closest("div.divDocument").find("table");
                table.find("tbody").append(data);
                //closeModalDocument($("#paciDocumentModal"));
                elementButton = null;
                paciEventHandler();
            }).fail(function (jqXHR, textStatus) {
                
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
            });
        }
    }
}

function questionnaireAjaxOnBegin() {
    if (!Validation.Container()) {
        return false;
    } else {
        showLoaderOptional();
    }    
}
function questionnaireAjaxOnBeginNoValidate() {
    showLoaderOptional();
}
function questionnaireAjaxOnComplete() {
    paciEventHandler();
    questionnaireFeatures();
}

function checkedInactive(check, inactiveId) {
    $("#btnInactiveYes").on("click",
        function () {
            check.checked = true;
            $("#" + inactiveId).val("true");
        });
}