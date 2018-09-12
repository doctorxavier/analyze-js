// Globals Variables
var elementCheckClone;
var paciQuestionnaireIdToDelete;
var institutionId;
var paciId;

//add sortable function to all tables
$(document).ready(function() {
    paciEventHandler();
});

$(document).off('click', '[name=chkbPaciClone]');
$(document).on('click', '[name=chkbPaciClone]', function () {
    elementCheckClone = $(this);
    institutionId = $(elementCheckClone).closest('table').attr("data-id");
    paciId = $(elementCheckClone).closest('tr').attr("data-id");
    $("#clonePaciWarning").modal();
});

$(document).off('change', 'input[type="checkbox"]');
$(document).on('change', 'input[type="checkbox"]', function () {
    this.checked = false;
});


function paciFeatures() {
    $("#btnClonePaciWarningYes").on("click",
        function () {
            showLoaderOptional();
            var url = basePath +
                "/paci/paci/ClonePaci?institutionId=" +
                institutionId +
                "&PaciQuestionnaireId=" +
                paciId +
                "&operationId=" +
                operationId +
                "&operationNumber=" +
                operationNumber;
            var dataForm = $("#frmPaciEditMode");
            $.post(url,
                dataForm.serializeArray(),
                function (data, textStatus, jqXHR) {
                    if (jqXHR.status === 204) {
                        hideLoaderOptional();
                        errorBar(jqXHR.statusText, 60, true);
                        return;
                    }

                    var table = $('#table_' + institutionId);
                    var checkBoxClones = table.find('label.checkbox-default');
                    checkBoxClones.each(function () {
                        var checkBox = $(this);
                        checkBox.hasClass('displayNone')
                            ? checkBox.addClass('displayNone')
                            : checkBox.hide();
                    });
                    table.find('tbody').append(data);
                    var containerAgency = table.parent();
                    containerAgency.find("[name='btnAddNewPaciRow']").hide();
                    institutionId = 0;
                    paciId = 0;
                    bindHandlers();
                    hideLoaderOptional();
                });
        });

    $('#btnDeleteRowPaciYes').on("click",
        function () {
            showLoaderOptional();
            var url = basePath + "/paci/paci/DeletePaci?operationNumber=" + operationNumber + "&paciQuestionnaireId=" + paciQuestionnaireIdToDelete;
            var dataForm = $("#frmPaciEditMode");
            $.post(url,
                dataForm.serializeArray(),
                function (data) {
                    if (data.IsValid) {
                        var table = $('#table_' + institutionId);
                        table.find('#tr_' + paciQuestionnaireIdToDelete).remove();
                        if (table.find('tbody tr').length === 0) {
                            $('.Pagination[dd-table=table_' + institutionId + ']').remove();
                            $('button[dd-table=' + institutionId + ']').removeClass('displayNone');
                        }
                        var checkBoxClones = table.find('label.checkbox-default');
                        checkBoxClones.each(function () {
                            var checkBox = $(this);
                            checkBox.hasClass('displayNone')
                                ? checkBox.removeClass('displayNone')
                                : checkBox.show();
                        });
                        var containerAgency = table.parent();
                        containerAgency.find("[name='btnAddNewPaciRow']").show();
                        hideLoaderOptional();
                    } else {
                        showError(data.ErrorMessage);
                        hideLoaderOptional();
                    }
                    paciQuestionnaireIdToDelete = 0;
                });
        });

    $('#btnDeleteRowPaciNo').on("click",
        function () {
            paciQuestionnaireIdToDelete = 0;
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
    $('div.pacimodulecontainer').each(function(element) {
        var containerId = this.id;
        containerStrList += containerId + ',';
    });
    $('.btnCollapseAll').attr("dd-contenedores", containerStrList);
}

function handleSortable() {
    $('table').each(function(element) {
        var tableId = this.id;
        $('#' + tableId).sortableConfluence(false, 4);
    });
}

function handlePaging() {
    $('table').each(function(element) {
        $(this).paginationConfluence(10);
    });
}

function newPaciRow(element) {
    showLoaderOptional();
    var id = $(element).attr('dd-table');
    var url = basePath + "/paci/paci/NewPaci?operationNumber=" + operationNumber + "&institutionId=" + id;
    var dataForm = $("#frmPaciEditMode");
    $.post(url,
        dataForm.serializeArray(),
        function (data, textStatus, jqXHR) {
            if (jqXHR.status === 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }

            var table = $('#table_' + id);
            table.find('tbody').append(data);
            var checkBoxClones = table.find('label.checkbox-default');
            checkBoxClones.each(function() {
                var checkBox = $(this);
                checkBox.hasClass('displayNone')
                    ? checkBox.addClass('displayNone')
                    : checkBox.hide();
            });
            var containerAgency = table.parent();
            containerAgency.find("[name='btnAddNewPaciRow']").hide();
            paciEventHandler();
        });
}

function deletePaci(element) {
    var paciQuestionnaireId = $(element).closest('tr').attr('data-id');
    var tableId = $(element).closest('table').attr('data-id');
    if (paciQuestionnaireId !== "0") {
        paciQuestionnaireIdToDelete = paciQuestionnaireId;
        institutionId = tableId;
        $("#deleteWarning").modal();
    } else {
        var table = $('#table_' + tableId);
        var tr = table.find('tbody tr');
        if (tr.length === 1) {
            $('.Pagination[dd-table=table_' + tableId + ']').remove();
        }
        $(element).closest('tr').remove();
        var checkBoxClones = table.find('label.checkbox-default');
        checkBoxClones.each(function() {
            var checkBox = $(this);
            checkBox.hasClass('displayNone')
                ? checkBox.removeClass('displayNone')
                : checkBox.show();
        });
        var containerAgency = table.parent();
        containerAgency.find("[name='btnAddNewPaciRow']").show();
    }
}

function paciAjaxOnBegin() {
    showLoaderOptional();
}

function paciAjaxOnComplete() {
    paciEventHandler();
    paciFeatures();
}