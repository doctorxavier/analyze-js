var idtable = 0;
var activityId = 0;
var riskId = 0;
var riskIdTemp = 0;
var documentRow = null;
var riskdDelete;
var activityDelete;
var RiskactivityDelete;
var commentSelector = null;
var riskIdMaterialized = 0;
var idRiskIssue = 0;
var previousValRiskStatus = null;
var previousTextRiskStatus = null;
var idTableResponsePlan = null;
var activityDel = null;
var elDelRisk = null;
var idRiskValidate = 0;

$(document).ready(function () {
    history.pushState({}, '', basePath + 'Operation/' + operationNumber + '/');
    riskMatrixEventHandler();
    loadEvents();
    $('div.icoInformation').attr('data-toggle', 'tooltip');
    $('div.icoInformation').attr('title', textCriticalPath);

    if (message !="") {
        showErrorMessage(message)
    }

});

function loadEvents() {
    loadChangeStatusEvent();
    //loadClickStatusEvent();
    loadChangeOutputEvent();
}

function riskMatrixEventHandler(container) {
    hideLoaderOptional();
    ////$("#tblRisk").sortableConfluence();
    bindHandlers(container);
}

function riskMatrixAjaxOnBegin() {
    if (!Validation.Container()) {
        showElementsNotValid();        
        return false;
    } else {
        showLoaderOptional();
    }    
}

function riskMatrixAjaxOnComplete() {
    riskMatrixEventHandler();
}

function riskMatrixAjaxOnFailure(xhr) {
    hideLoaderOptional();

    if (xhr.statusCode().status == 406) {
        var modalContent = $("#errorSaveRiskMatrix div.modal-body");
        modalContent.empty();
        modalContent.html("<p>" + xhr.statusText + "</p>");
        riskMatrixEventHandler();
        $("#errorSaveRiskMatrix").modal()
    } else {
        showError(xhr.statusText)
    }    
}

function riskMatrixAjaxOnBeginNoValidate() {
    showLoaderOptional();
}

function loadEventFundingSource() {
    $("input[data-name=ddlFundingSource]").on('change', function () {
        $("input[data-name=ddlFundingSource]").off('change');
        var activityId = $(this).attr('data-id');
        var valueElement = this.value;
        var textElement;

        if (valueElement == null || valueElement == "") {
            textElement = "(select option)";
        } else {
            textElement = $('a[dd-value=' + valueElement + ']')[0].text;
        }

        var elements = $('button[data-id=' + activityId + ']');

        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $(item).SelectIndex(1)
                $(item).SetValue(valueElement, textElement);
                $('input[data-id=' + activityId + ']').val(valueElement);
                if (textElement == "(select option)") {
                    $(item.children[0]).addClass("placeholder");
                } else {
                    $(item.children[0]).removeClass("placeholder");
                }
            });
        }

        loadEventFundingSource();
    });
}

function loadEventsDropdownActivityStat() {

    $("input[data-name=ddlActivityStatus]").on('change', function () {
        $("input[data-name=ddlActivityStatus]").off('change');
        var activityId = $(this).attr('data-id');
        var valueElement = this.value;
        var textElement;

        if (valueElement == null || valueElement == "") {
            textElement = "(select option)";
        } else {
            textElement = $('a[dd-value=' + valueElement + ']')[0].text;
        }

        var elements = $('button[data-id=' + activityId + ']');

        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $(item).SelectIndex(1)
                $(item).SetValue(valueElement, textElement);
                $('input[data-id=' + activityId + ']').val(valueElement);
                if (textElement == "(select option)") {
                    $(item.children[0]).addClass("placeholder");
                } else {
                    $(item.children[0]).removeClass("placeholder");
                }
                
            });
        }

        if (this.value == idCompletedActv) {
            var idRisk = activityId.split("_")[1];
            var dates = $("input[data-id='completionDateActv_" + idRisk + "']");
            var toDay = $.datepicker.formatDate("dd M yy", new Date());

            if (dates.length >= 1) {
                $.each(dates, function (index, item) {
                    $(item).val(toDay);
                });
            }
        }

        loadEventsDropdownActivityStat();
    });

}

var prev_val;
function loadEventDropdownManagementStrategy() {
    $('select[data-name="mddlManagementStrategy"]').off('change').change(function (ev, val) {
        var $this = $(this);
        var $dataParentID = $this.closest('tr').attr('data-row-parent-id');

        if (val.selected != undefined) {
            if (val.selected == idAcceptStatus) {
                prev_val = idAcceptStatus;
            } else {
                if ($this.val().length < 3) {
                    prev_val = $this.val();
                    var index = prev_val.indexOf(idAcceptStatus);
                    if (index > -1) {
                        prev_val.splice(index, 1);
                    }
                }
            }
        } else {
            prev_val = $this.val();
        }
        var dataId = $this.attr('data-id');

        $('[data-id="' + dataId + '"]').each(function () {
            var $element = $(this);
            var dataParentID = $element.closest('tr').attr('data-row-parent-id');
            var dataId = $element.attr("data-id").split("_")[1];

            $element.val(prev_val).trigger("chosen:updated");
            if (prev_val != null) {
                var index = prev_val.indexOf(idAcceptStatus);
                if (index > -1) {
                    $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).show();
                    $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).children('textarea').attr('data-parsley-required', 'true');
                    Validation.Init();
                } else {
                    $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).hide();
                    $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).children('textarea').removeAttr('data-parsley-required');
                    Validation.Destroy();
                    Validation.Init();
                }
            } else {
                $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).hide();
                $element.closest('tbody').find("#riskJustificationOutputs_" + dataId).children('textarea').removeAttr('data-parsley-required');
                Validation.Destroy();
                Validation.Init();
            }
        });

    }).chosen().on('chosen:showing_dropdown', function () {
        prev_val = $(this).val();
    });
}

function loadChangeOutputEvent() {

    loadEventsDropdownActivityStat();
    loadEventDropdownManagementStrategy();
    loadEventFundingSource();

    $("textarea[data-name=activityDescription]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('textarea[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $(item).val(description);
            });
        }
    });

    $("textarea[data-name=justificationActv]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('textarea[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('textarea[data-id=' + activityId + ']').val(description);
            });
        }
    });

    $("input[data-name=institutioneActv]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('input[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('input[data-id=' + activityId + ']').val(description);
            });
        }
    });

    $("input[data-name=responsibleActivity]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('input[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('input[data-id=' + activityId + ']').val(description);
            });
        }
    });

    $("input[data-name=budgetActivity]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('input[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('input[data-id=' + activityId + ']').val(description);
            });
        }
    });

    $("input[data-name=expectedActDate]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('input[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('input[data-id=' + activityId + ']').val(description);
            });
        }
    });

    $("input[data-name=completionDateActv]").on('change', function () {
        var description = this.value;
        var activityId = $(this).attr('data-id');
        var elements = $('input[data-id=' + activityId + ']');
        if (elements.length > 1) {
            $.each(elements, function (index, item) {
                $('input[data-id=' + activityId + ']').val(description);
            });
        }
    });


}

function loadChangeStatusEvent() {

    $('div[id=materializedRisk]').find('button.close').on("click", function () {
        resetValuesRiskStatus();
    });


    $("input[data-name=ddlistStatus]").on('change', function () {
        var idRisk = this.parentElement.parentElement.parentNode.dataset.id;
        idRiskIssue = idRisk;
        if (this.value == idMaterialized && (previousValRiskStatus != this.value)) {
            $('#descriptionMaterialized').attr('data-parsley-required', 'true');
            Validation.Init($('#descriptionMaterialized').parent());
            $("#materializedRisk").modal();
        }

        if (this.value == idInactiveStatus) {
            $('#riskJustification_' + idRisk).show();
            $('#riskJustification_' + idRisk).children('textarea').attr('data-parsley-required', 'true');
            Validation.Init('#riskJustification_' + idRisk);
        } else {
            $('#riskJustification_' + idRisk).hide();
            $('#riskJustification_' + idRisk).children('textarea').removeAttr('data-parsley-required');
            Validation.Destroy('#riskJustification_' + idRisk);
            Validation.Init('#riskJustification_' + idRisk);
        }
    });

    $("button[data-name=ddlistStatus]").on('click', function () {
        previousValRiskStatus = $(this).GetValue();
        previousTextRiskStatus = $(this).GetText();

    });

    $("input[data-name=ddlistProbability]").on('change', function () {
        var valProbability = this.value;
        var thisTemp = $($(this).closest("tr")).find("button[data-name='ddlistImpact']");
        var valImpactText = $($(this).closest("tr")).find("button[data-name='ddlistImpact']").text().trim();
        var valImpact = thisTemp.parent().find("ul a:contains('" + valImpactText + "')").filter(function () {
            return $(this).text() === valImpactText;
        }).attr('dd-value');

        recalculateRiskLevel(this, valProbability, valImpact);

    });

    $("input[data-name=ddlistImpact]").on('change', function () {
        var valImpact = this.value;
        var thisTemp = $($(this).closest("tr")).find("button[data-name='ddlistProbability']");
        var valProbabilityText = $($(this).closest("tr")).find("button[data-name='ddlistProbability']").text().trim();
        var valProbability = thisTemp.parent().find("ul a:contains('" + valProbabilityText + "')").filter(function () {
            return $(this).text() === valProbabilityText;
        }).attr('dd-value');

        recalculateRiskLevel(this, valProbability, valImpact);

    });
}

function recalculateRiskLevel(it, valProbability, valImpact) {
    if ((valProbability != "" && valProbability != undefined) && (valImpact != "" && valImpact != undefined)) {

        riskIdTemp = $($(it).closest("tr")).attr('data-id');
        var url = basePath + "/RiskMatrix/RiskMatrix/RecalculateRiskLevel?operationNumber=" + operationNumber + "&valProbability=" + valProbability + "&valImpact=" + valImpact;
        $.post(url, function (data, textStatus, jqXHR) {
            if (jqXHR.status === 204) {
                hideLoaderOptional();
                showError(jqXHR.statusText)
                return;
            }
            $("label[data-name=" + riskIdTemp + "_RiskLevel]").text(data.RiskLevel);
            changeColorRiskLevel(data.RiskLevelCode, riskIdTemp);
        }).fail(function () {
            hideLoaderOptional();
        });
    }
}

$(document).off('click', ".subnivel .tableNormal thead tr th.tree.icon:contains('+')");
$(document).on('click', ".subnivel .tableNormal thead tr th.tree.icon:contains('+')", function () {
    var buttonClick = $(this);
    var tableColapse = buttonClick.closest('table').find('tbody tr[data-id] .buttonShowRow');
    if (tableColapse.hasClass('active')) {
        tableColapse.click();
    }
});

$(document).off('click', "#tblRisk thead tr th.tree.icon:contains('+')");
$(document).on('click', "#tblRisk thead tr th.tree.icon:contains('+')", function () {
    var buttonCollapse = $(this);
    var tableColapse = buttonCollapse.closest('table').find('tbody tr[data-id] .buttonShowRow');
    if (tableColapse.hasClass('active')) {
        buttonCollapse.closest('table').find('tbody tr[data-id] .buttonShowRow.active').click();
        }
});

$(document).off('click', 'ul.dropdown-menu[aria-labelledby="id-ddlActivities"] a');
$(document).on('click', 'ul.dropdown-menu[aria-labelledby="id-ddlActivities"] a', function () {
    activityId = $("[name='ddlActivities']").val();
})

function addNewRowActivity(me) {
    idtable = me.context.getAttribute("dd-table");
    riskId = idtable.split("_")[1];
    var url = basePath + "/RiskMatrix/RiskMatrix/GetActivitiesByRisk?operationNumber=" + operationNumber + "&riskId=" + riskId;
    var dataForm = $("#frmRiskMatrixEditMode");
    $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
        if (jqXHR.status === 204) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
            return;
        }
        var modalContent = $("#addActivityModal div.modal-body");
        modalContent.empty();
        modalContent.html(data);
        riskMatrixEventHandler(modalContent);
        var rowResponsePlan = $('tr[data-row-response-plan="' + riskId + '"]');
        rowResponsePlan.removeClass('hide');
        $.each(rowResponsePlan.find('tr'), function (index, value) {
            $(value).removeClass('hide');
        });

        rowResponsePlan.find('table th span').removeClass('hide');
        rowResponsePlan.find('table th.tree.icon span.icon').html('-');

        $("#addActivityModal").modal();
    }).fail(function () {
        hideLoaderOptional();
    });
}

function addNewRowRisk() {
    showLoaderOptional();
    var url = basePath + "/RiskMatrix/RISKMATRIX/AddNewRowRisk?operationNumber=" + operationNumber;
    var dataForm = $("#frmRiskMatrixEditMode");
    $.post(url,
        dataForm.serializeArray(),
        function (data, textStatus, jqXHR) {
            if (jqXHR.status === 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }

            var id = $(data).attr('id');
            modifyVisualElementsAddnewRisk();
            var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="tblRisk"]');

            $($('#tblRisk').find('tbody')[0]).append(data);
            var length = $('span.icon.lineRisksImportant').length;
            var lastIcon = $('span.icon.lineRisksImportant')[length - 1];

            if (tempCollapseAll.hasClass('collapse')) {
                $(lastIcon).text('-');
            } else {
                $(lastIcon).text('+');
            }

            riskMatrixEventHandler($('[id="newRisk"]').last());
            riskMatrixEventHandler($('[data-id="secondLineNewRisk"]').last());
            riskMatrixEventHandler($('[data-risk-line="thirdLineNewRisk"]').find('table > thead').last());
            riskMatrixEventHandler($('[data-risk-line="thirdLineNewRisk"] #buttonAdNewAcitivty').last());
            if (tempCollapseAll.hasClass('collapse')) {
                $('tr[data-risk-line="thirdLineNewRisk"]').last().removeClass('hide');
            }

            loadChangeStatusEvent();
        })
        .fail(function () {
            hideLoaderOptional();
        })    
}

function addDocument() {
    $('#dmDocumentModal').modal();
}

function addComment() {
    showLoaderOptional();    
    var url = basePath + "/riskmatrix/riskmatrix/NewNote?operationNumber=" + operationNumber;
    var dataForm = $("#frmRiskMatrixEditMode");
    $.post(url,
        dataForm.serializeArray(),
        function (data, textStatus, jqXHR) {
            if (jqXHR.status === 204) {
                hideLoaderOptional();
                errorBar(jqXHR.statusText, 60, true);
                return;
            }
            $("#sectionRiskMatrixNewComment").append(data);
            $("[name=btnRiskAddComment]").attr("disabled", true);
            riskMatrixEventHandler($("#sectionRiskMatrixNewComment"));
        })
        .fail(function () {
            hideLoaderOptional();
        });
}

function deleteNote(element) {
    commentSelector = $(element).closest("div#riskmatrixUserCommentSection");
    $("#deleteCommentWarning").modal();
}

function validateFields() {
    if (Validation.Container()) {
        $("#frmRiskMatrixEditMode").submit();
    }
}

function renderizeRow(documentNumber, typeTab, documentName) {
    showLoaderOptional();
    if (documentNumber.length > 0) {
        var documentData = getDocumentNumberAndName(documentNumber, typeTab, documentName);
        var docNumber = documentData["documentNumber"];
        var docName = documentData["documentName"];
        if (docNumber != "" && docName != "") {

            var dataForm = $("#frmRiskMatrixEditMode");

            var url = encodeURI(basePath + "/riskmatrix/riskmatrix/AddDocument?operationNumber=" + operationNumber + "&documentNumber=" + docNumber);

            $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
                if (jqXHR.status == 204) {
                    hideLoaderOptional();
                    showError(jqXHR.statusText)
                    return;
                }

                var table = $('#tblRiskMatrixDocument');
                table.find("tbody").append(data);
                riskMatrixEventHandler($('#tblRiskMatrixDocument tbody'));
            }).fail(function (jqXHR, textStatus) {
                hideLoaderOptional();
                showError(jqXHR.statusText);
            });
        } else {
            hideLoaderOptional();
        }
    }
}

function getDocumentNumberAndName(object, typeTab, documentName) {
    var documentData = { documentName: "", documentNumber: "" };

    switch (typeTab) {
        case "added":
            documentData["documentNumber"] = object[0];
            documentData["documentName"] = documentName;
            break;
        case "selected":
            documentData["documentName"] = object[0].DocumentName;
            documentData["documentNumber"] = object[0].DocumentNumber;
            break;
    }
    
    return documentData;
}

function deleteDocument(element) {
    documentRow = $(element).closest("tr");    
    $("#deleteDocumentWarning").modal();
}

function deleteRisk(element) {
    var riskId = $(element).closest('tr').attr('data-id');
    elDelRisk = element;
    if (riskId > 0) {
        riskdDelete = riskId;
        $("#deleteWarning").modal();
    } else {
        $(element).closest('tbody').find('[data-row-parent-id="' + riskId + '"]').remove();
        var tr = $('[data-id="' + riskId + '"]').closest('tr');
        $(element).closest('tbody').find('[data-risk-id="' + riskId + '"]').remove();
        var last = $(element).closest('tbody').find('[data-description="firtTdResponsePlan"]').length;
        fixAttributesIdAndNameRisks(tr);
        $($(element).closest('tbody').find('[data-description="firtTdResponsePlan"]')[last - 1]).removeClass("tree");
        $($(element).closest('tbody').find('[data-description="firtTdResponsePlan"]')[last - 1]).find("span").remove();
        $(element).closest('tr').remove();
        modifyVisualElementsAfterDelRisk();
        
    }
}

function ExportFileRiskReport(source) {
    var formatDoc = source.attr('data-filetype');
    var urlReportFile = basePath + "/RiskMatrix/RiskMatrix/DownloadRiskMatrixExportFile?operationNumber=" + operationNumber + "&formatType=" + formatDoc;
    window.open(urlReportFile, "_blank");
}

$("#btnDeleteYes").on("click", function () {
    showLoaderOptional();
    var documentId = documentRow.attr("data-Id");
    if (documentId === "undefined" || documentId === "0") {
        $(documentRow).remove();
        documentRow = null;
        hideLoaderOptional();
    } else {
        var url = encodeURI(basePath + "/riskmatrix/riskmatrix/DeleteDocument?operationNumber=" + operationNumber + "&documentId=" + documentId);
        var dataForm = $("#frmRiskMatrixEditMode");
        $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
            if (jqXHR.status == 204) {
                hideLoaderOptional();
                showError(jqXHR.statusText);
                documentRow = null;
                return;
            }

            if (!data.IsValid) {
                hideLoaderOptional();
                showError(jqXHR.statusText);
                documentRow = null;
                return;
            }

            $(documentRow).remove();
            documentRow = null;
        })
    }
})

$('#btnDeleteRowRiskYes').on("click", function () {
    showLoaderOptional();
    var url = basePath + "/riskmatrix/riskmatrix/DeleteRisk?operationNumber=" + operationNumber + "&riskId=" + riskdDelete;
    $.post(url,        
        function (data) {
            if (data.IsValid) {
                var rows = $('#tblRisk tbody');
                rows.find('[data-row-parent-id="' + riskdDelete + '"]').remove();
                var tr = $('[data-id="' + riskdDelete + '"]');    
                $('[data-id="' + riskdDelete + '"]').find('td')[1]
                rows.find('[data-id="' + riskdDelete + '"]').remove();
                rows.find('[data-risk-id-resp="' + riskdDelete + '"]').remove();
                fixAttributesIdAndNameRisks(tr);

                var last = $('#tblRisk tbody').find('[data-description="firtTdResponsePlan"]').length;
                $($('#tblRisk tbody').find('[data-description="firtTdResponsePlan"]')[last - 1]).removeClass("tree");
                $($('#tblRisk tbody').find('[data-description="firtTdResponsePlan"]')[last - 1]).find("span").remove();

                modifyVisualElementsAfterDelRisk();
                hideLoaderOptional();
            } else {
                showError(data.ErrorMessage);
                hideLoaderOptional();
            }

            riskdDelete = 0;

        }).fail(function () {
        hideLoaderOptional();
    });
});

$('#btnDeleteRowRiskNo').on("click", function () {
    riskdDelete = 0;
});

$('#btnAddActivityYes').on("click", function () {

    showLoaderOptional();
    var url = basePath + "/RiskMatrix/RiskMatrix/AddNewRowActivity?operationNumber=" + operationNumber + "&RiskMatrixId=" + riskId + "&ActivityId= " + activityId;
    var dataForm = $("#frmRiskMatrixEditMode");
    $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
        if (jqXHR.status === 204) {
            hideLoaderOptional();
            errorBar(jqXHR.statusText, 60, true);
            return;
        }
        var isNewTable = unexistTableActivity(riskId);
        var table = $('#' + idtable);
        if (isNewTable) {
            $('#divResponsePlan_' + riskId).removeClass('notShowResponsePlan');
        }
        modifyVisualElementosAddActivity(idtable);
        table.find('tbody').append(data);
        riskMatrixEventHandler(table);
        var rowResponsePlan = $('tr[data-row-response-plan="' + riskId + '"]');
        rowResponsePlan.removeClass('hide');
        $.each(rowResponsePlan.find('tr'), function (index, value) {
            $(value).removeClass('hide');
        });

        loadChangeOutputEvent();
        riskId = 0;
        activityId = 0;

    })
    .fail(function () {
        hideLoaderOptional();
    });
});

$("#btnDeleteCommentYes").on("click", function () {
    showLoaderOptional();
    var commentId = commentSelector.attr("data-Id");
    if (commentId === "undefined" || commentId === "0") {
        $(commentSelector).remove();
        $("[name=btnRiskAddComment]").removeAttr("disabled");
        commentSelector = null;
        hideLoaderOptional();
    } else {
        var url = encodeURI(basePath + "/riskmatrix/riskmatrix/DeleteUserComment?operationNumber=" + operationNumber + "&userCommentId=" + commentId);
        var dataForm = $("#frmRiskMatrixEditMode");
        $.post(url, dataForm.serializeArray(), function (data, textStatus, jqXHR) {
            if (jqXHR.status == 204) {
                hideLoaderOptional();
                showError(jqXHR.statusText);
                documentRow = null;
                return;
            }

            if (!data.IsValid) {
                hideLoaderOptional();
                showError(jqXHR.statusText);
                documentRow = null;
                return;
            }

            $(commentSelector).remove();
            commentSelector = null;
        })
    }
});

function deleteActivity(element) {

    var ActivityId = $(element).closest('tr').attr('data-id');
    var riskId = $(element).closest('tr').attr('data-risk-id');
    var idTable = $(element).closest('table').attr("id");
    idTableResponsePlan = idTable;
    if (ActivityId > 0) {
        activityDelete = ActivityId;
        activityDel = element;
        RiskactivityDelete = riskId;
        $("#deleteWarningActivity").modal();
    } else {
        var tr = $(element).closest('tr');
        var tbody = tr.parent();
        $(element).closest('tr').remove();               
       
        var otherRowsToDel = tbody.find('tr[data-row-parent-id="' + ActivityId + '"]');

        modifyVisualElementosDelActivity(idTable);
        if (otherRowsToDel.length > 0) {
            $.each(otherRowsToDel, function (index, item) {
                $(item).remove();
            });
        }
        hideDivResponsePlan(riskId);
        fixAttributesIdAndNameActivities(tr);
    }
}

$('#btnDeleteRowActivityYes').on("click", function () {
    showLoaderOptional();
    var url = basePath + "/riskmatrix/riskmatrix/DeleteActivity?operationNumber=" + operationNumber + "&ActivityId=" + activityDelete + "&RiskId=" + RiskactivityDelete;
    $.post(url,
        function (data) {
            if (data.IsValid) {
                var trBack = $(activityDel).closest('tr');
                var tbody = trBack.parent();
                var tr = $(activityDel).closest('tr');
                tr.remove();
                modifyVisualElementosDelActivity(idTableResponsePlan);
                idTableResponsePlan = null;
                var otherRowsToDel = tbody.find('tr[data-row-parent-id="' + activityDelete + '"]');

                if (otherRowsToDel.length > 0) {
                    $.each(otherRowsToDel, function (index, item) {
                        $(item).remove();
                    });
                }

                hideDivResponsePlan(RiskactivityDelete);

                fixAttributesIdAndNameActivities(trBack);
                hideLoaderOptional();
                activityDel = null;
            } else{
                showError(data.ErrorMessage);
                hideLoaderOptional();
                activityDel = null;
            }
            activityDelete = 0;
            activityDel = null;

        }).fail(function () {
            hideLoaderOptional();
        });
});

$('#btnDeleteRowActivitykNo').on("click", function () {
    activityDelete = 0;
    idTableResponsePlan = null;
    activityDel = null;

});


$("#btnMaterializedRiskYes").on("click", function () {  
    var parentEl = $('#descriptionMaterialized').parent();
    if (Validation.Container(parentEl)) {
        showLoaderOptional();
        var description = $('input[id=descriptionMaterialized]').val();
        $('input[data-description-issue=' + idRiskIssue + ']').val(description);
        $('input[id=descriptionMaterialized]').val("");
        $('#descriptionMaterialized').removeAttr('data-parsley-required');
        Validation.Destroy(parentEl);
        Validation.Init(parentEl);
        $("#materializedRisk").modal("hide");
    }    
});

$("#btnMaterializedRiskNo").on("click", function () {
    resetValuesRiskStatus();
    $("#materializedRisk").modal("hide");
});

function resetValuesRiskStatus() {
    var parentEl = $('#descriptionMaterialized').parent();
    if (previousValRiskStatus != null && previousTextRiskStatus != null) {
        $("input[data-name=ddlistStatus]").off('change');
        $('button[data-id="ddlistStatus_' + idRiskIssue + '"]').SetValue(previousValRiskStatus, previousTextRiskStatus);
        previousValRiskStatus = null;
        previousTextRiskStatus = null;
        $('input[id=descriptionMaterialized]').val("");
        loadChangeStatusEvent();
        $('#descriptionMaterialized').removeAttr('data-parsley-required');
        Validation.Destroy(parentEl);
        Validation.Init(parentEl);
    }
}

function changeColorRiskLevel(RiskCode, riskIdTemp) {
    switch(RiskCode) {
        case CodeStatusHigh:
            var tdParent = $("label[data-name=" + riskIdTemp + "_RiskLevel]").parent();
            var divCircle = tdParent.find('div.circle-icon');

            if (divCircle.length > 0) {
                divCircle.remove();
            }

            var html = tdParent.html();
            tdParent.html('<div class="circle-icon circle-red"></div>' + html);
            break;
        case CodeStatusMediumHigh:
            var tdParent = $("label[data-name=" + riskIdTemp + "_RiskLevel]").parent();
            var divCircle = tdParent.find('div.circle-icon');

            if (divCircle.length > 0) {
                divCircle.remove();
            }

            var html = tdParent.html();
            tdParent.html('<div class="circle-icon circle-orange"></div>' + html);
            break;
        case CodeStatusMediumLow:
            var tdParent = $("label[data-name=" + riskIdTemp + "_RiskLevel]").parent();
            var divCircle = tdParent.find('div.circle-icon');

            if (divCircle.length > 0) {
                divCircle.remove();
            }

            var html = tdParent.html();
            tdParent.html('<div class="circle-icon circle-yellow"></div>' + html);
            break;
        case CodeStatusLow:
            var tdParent = $("label[data-name=" + riskIdTemp + "_RiskLevel]").parent();
            var divCircle = tdParent.find('div.circle-icon');

            if (divCircle.length > 0) {
                divCircle.remove();
            }

            var html = tdParent.html();
            tdParent.html('<div class="circle-icon circle-green"></div>' + html);
            break;
        case CodeStatusMedium:
            var tdParent = $("label[data-name=" + riskIdTemp + "_RiskLevel]").parent();
            var divCircle = tdParent.find('div.circle-icon');

            if (divCircle.length > 0) {
                divCircle.remove();
            }

            var html = tdParent.html();
            tdParent.html('<div class="circle-icon circle-yellow"></div>' + html);
            break;
    }
}

function modifyVisualElementsAfterDelRisk() {
    var rowLastRisk = $('tr[data-desc="firstRowRisk"]').length;
    var $firstTd = $($('tr[data-desc="firstRowRisk"]')[rowLastRisk - 1]).find("td")[0];
    var $firtSpan = $($firstTd).find("span")[0];
    var $secondSpan = $($firstTd).find("span")[1];
    $($secondSpan).addClass("selected topZero highZero");

    var countSecondRowRisk = $('tr[data-desc="secondRowRisk"]').length;
    var lastSecondRowRisk = $('tr[data-desc="secondRowRisk"]')[countSecondRowRisk - 1];
    $(lastSecondRowRisk).find("span").remove();
}

function modifyVisualElementsAddnewRisk() {
    $('#lastTdRisk').addClass("tree");
    $('#lastTdRisk').append("<span></span>");
    $('#lastTdRisk').removeAttr("id");

    $($('#lastRowRisk').find('span')[0]).removeClass("high50");
    $($('#lastRowRisk').find('span')[1]).removeClass("topZero");
    $('#lastRowRisk').removeAttr("id");

    $('#lastRowRiskSubLevel').addClass("tree");
    $('#lastRowRiskSubLevel').append("<span></span>");
    $('#lastRowRiskSubLevel').removeAttr("id");

    var countTdResponsePlan = $('td[data-description="firtTdResponsePlan"]').length;

    if (countTdResponsePlan > 0) {
        var lastTdResponsePlan = $('td[data-description="firtTdResponsePlan"]')[countTdResponsePlan - 1];

        if (lastTdResponsePlan.innerHTML == "") {
            $(lastTdResponsePlan).addClass("tree");
            $(lastTdResponsePlan).append("<span></span>");
        }
    }    

    var countfirstRowRisk = $('tr[data-desc="firstRowRisk"]').length;

    if (countfirstRowRisk > 0) {
        var lastFirstRowRisk = $('tr[data-desc="firstRowRisk"]')[countfirstRowRisk - 1];
        $($(lastFirstRowRisk).find('span')[0]).removeClass("high50");
        $($(lastFirstRowRisk).find('span')[1]).removeClass("topZero");
        $($(lastFirstRowRisk).find('span')[1]).removeClass("highZero");
        $($(lastFirstRowRisk).find('span')[1]).removeClass("selected");
    }
    

    var countSecondRowRisk = $('tr[data-desc="secondRowRisk"]').length;

    if (countSecondRowRisk > 0) {
        var lastSecondRowRisk = $('tr[data-desc="secondRowRisk"]')[countSecondRowRisk - 1];

        if (lastSecondRowRisk.firstElementChild.innerHTML == "") {
            $(lastSecondRowRisk.firstElementChild).append("<span></span>");
        }
    }
    
}

function modifyVisualElementosAddActivity(idtable) {
    var countFirstRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="firstRowResponsePlan"]').length;

    if (countFirstRowResponsePlan > 0) {
        var lastFirstRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="firstRowResponsePlan"]')[countFirstRowResponsePlan - 1];
        var firstTd = $(lastFirstRowResponsePlan).first();
        var firstSpan = $(firstTd).find('span')[0];
        var secondSpan = $(firstTd).find('span')[1];
        $(firstSpan).removeClass("high50");
        $(secondSpan).removeClass("topZero");
    }
    

    var countSecondRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="secondRowResponsePlan"]').length;

    if (countSecondRowResponsePlan > 0) {
        var lastSecondRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="secondRowResponsePlan"]')[countSecondRowResponsePlan - 1];
        $(lastSecondRowResponsePlan.firstElementChild).append("<span></span><span></span>");
    }
    
    var countThirdRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="thirdRowResponsePlan"]').length;

    if (countThirdRowResponsePlan) {
        var lastThirdRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="thirdRowResponsePlan"]')[countThirdRowResponsePlan - 1];
        $(lastThirdRowResponsePlan.firstElementChild).append("<span></span><span></span>");
    }

    var countQuarterRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="quarterRowResponsePlan"]').length;
    if (countQuarterRowResponsePlan > 0) {
        var lastQuarterRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="quarterRowResponsePlan"]')[countQuarterRowResponsePlan - 1];
        $(lastQuarterRowResponsePlan.firstElementChild).append("<span></span>");
    }  
    
}

function modifyVisualElementosDelActivity(idtable) {
    var countFirstRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="firstRowResponsePlan"]').length;
    var lastFirstRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="firstRowResponsePlan"]')[countFirstRowResponsePlan - 1];
    var firstTd = $(lastFirstRowResponsePlan).first();
    var firstSpan = $(firstTd).find('span')[0];
    var secondSpan = $(firstTd).find('span')[1];
    $(firstSpan).addClass("high50");
    $(secondSpan).addClass("topZero");

    var countSecondRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="secondRowResponsePlan"]').length;
    var lastSecondRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="secondRowResponsePlan"]')[countSecondRowResponsePlan - 1];
    var firstTd = $(lastSecondRowResponsePlan).first();
    $($(firstTd).find('span')[0]).remove();
    $($(firstTd).find('span')[0]).remove();

    var countThirdRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="thirdRowResponsePlan"]').length;
    var lastThirdRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="thirdRowResponsePlan"]')[countThirdRowResponsePlan - 1];
    var firstTd = $(lastThirdRowResponsePlan).first();
    $($(firstTd).find('span')[0]).remove();
    $($(firstTd).find('span')[0]).remove();

    var countQuarterRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="quarterRowResponsePlan"]').length;
    var lastQuarterRowResponsePlan = $('table[id=' + idtable + '] tr[data-description="quarterRowResponsePlan"]')[countQuarterRowResponsePlan - 1];
    var firstTd = $(lastQuarterRowResponsePlan).first();
    $($(firstTd).find('span')[0]).remove();
    $($(firstTd).find('span')[0]).remove();
}

function fixAttributesIdAndNameActivities(tr) {

    var id = $($($(tr).find('td')[3]).find('input')[0]).attr('id');
    var name = $($($(tr).find('td')[3]).find('input')[0]).attr('name');
    var otherId = $($($(tr).find('td')[2]).find('textarea')).attr('id');

    var indexIdRisk = id.match(/(\d+)/g)[0];
    var indexId = id.match(/(\d+)/g)[1];

    var indexRiskName = name.match(/(\d+)/g)[0];
    var indexName = name.match(/(\d+)/g)[1];

    var indexRiskOtherId = otherId.match(/(\d+)/g)[0];
    var indexOtherId = otherId.match(/(\d+)/g)[1];

    indexOtherId++;
    var countIdOthers = $('[id*="Risks_' + indexRiskOtherId + '__ResponsePlanActivity_' + indexOtherId + '__"]').length;

    while (countIdOthers > 0) {
        var othersFields = $('[id*="Risks_' + indexRiskOtherId + '__ResponsePlanActivity_' + indexOtherId + '__"]');
        $.each(othersFields, function (index, value) {
            var actualId = $(value).attr('id');
            var indexActivity = actualId.match(/(\d+)/g)[1];
            var newIndex = indexActivity - 1;
            var newId = actualId.replace('_' + indexActivity + '__', '_' + newIndex + '__');
            $(value).attr('id', newId);
        });

        indexOtherId++
        countIdOthers = $('[id*="Risks_' + indexRiskOtherId + '__ResponsePlanActivity_' + indexOtherId + '__"]').length;
    }


    indexId++;
    var countOthers = $('[id*="Risks[' + indexIdRisk + '].ResponsePlanActivity[' + (indexId) + ']"]').length;

    while (countOthers > 0) {
        var othersFields = $('[id*="Risks[' + indexIdRisk + '].ResponsePlanActivity[' + (indexId) + ']"]');
        $.each(othersFields, function (index, value) {
            var actualId = $(value).attr('id');
            var indexActivity = actualId.match(/(\d+)/g)[1];
            var newIndex = indexActivity - 1;
            var newId = actualId.replace('[' + indexActivity + ']', '[' + newIndex + ']');
            $(value).attr('id', newId);
        });

        indexId++
        countOthers = $('[id*="Risks[' + indexIdRisk + '].ResponsePlanActivity[' + (indexId) + ']"]').length;
    }

    indexName++;
    var countOthersName = $('[name*="Risks[' + indexRiskName + '].ResponsePlanActivity[' + (indexName) + ']"]').length;

    while (countOthersName > 0) {
        var othersFields = $('[name*="Risks[' + indexRiskName + '].ResponsePlanActivity[' + (indexName) + ']"]');
        $.each(othersFields, function (index, value) {
            var actualName = $(value).attr('name');
            var indexActivity = actualName.match(/(\d+)/g)[1];
            var newIndex = indexActivity - 1;
            var newName = actualName.replace('[' + indexActivity + ']', '[' + newIndex + ']');
            $(value).attr('name', newName);

        });

        indexName++;
        countOthersName = $('[name*="Risks[' + indexRiskName + '].ResponsePlanActivity[' + (indexName) + ']"]').length;

    }

    Validation.Destroy();
    Validation.Init();

}

function fixAttributesIdAndNameRisks(tr) {
    var IdRisk = $($($(tr).find('td')[3])).find('input').attr('id');
    var otherIdRisk = $($($(tr).find('td')[4])).find('input').attr('id');
    var nameRisk = $($($(tr).find('td')[3])).find('input').attr('name');
    var indexIdRisk = IdRisk.match(/(\d+)/g)[0];
    var indexNameRisk = nameRisk.match(/(\d+)/g)[0];
    var indexOtherIsRisk = otherIdRisk.match(/(\d+)/g)[0];

    indexOtherIsRisk++;
    var countOtherIdRisks = $('[id*="Risks_' + indexOtherIsRisk + '__').length;

    while (countOtherIdRisks > 0) {
        var othersFields = $('[id*="Risks_' + indexOtherIsRisk + '__');

        $.each(othersFields, function (index, value) {
            var actualId = $(value).attr('id');
            var indexRisk = actualId.match(/(\d+)/g)[0];
            var newIndex = indexRisk - 1;
            var newId = actualId.replace('Risks_' + indexRisk + '__', 'Risks_' + newIndex + '__');
            $(value).attr('id', newId);
        });

        indexOtherIsRisk++
        countOtherIdRisks = $('[id*="Risks_' + indexOtherIsRisk + '__').length;
    }

    indexIdRisk++;
    var countIdRisks = $('[id*="Risks[' + indexIdRisk + ']."]').length;

    while (countIdRisks > 0) {
        var othersFields = $('[id*="Risks[' + indexIdRisk + ']."]');

        $.each(othersFields, function (index, value) {
            var actualId = $(value).attr('id');
            var indexRisk = actualId.match(/(\d+)/g)[0];
            var newIndex = indexRisk - 1;
            var newId = actualId.replace('Risks[' + indexRisk + ']', 'Risks[' + newIndex + ']');
            $(value).attr('id', newId);
        });

        indexIdRisk++
        countIdRisks = $('[id*="Risks[' + indexIdRisk + ']."]').length;
    }

    indexNameRisk++;

    var countNameRisks = $('[name*="Risks[' + indexNameRisk + ']."]').length;

    while (countNameRisks > 0) {
        var othersFields = $('[name*="Risks[' + indexNameRisk + ']."]');

        $.each(othersFields, function (index, value) {
            var actualName = $(value).attr('name');
            var indexRisk = actualName.match(/(\d+)/g)[0];
            var newIndex = indexRisk - 1;
            var newName = actualName.replace('Risks[' + indexNameRisk + ']', 'Risks[' + newIndex + ']');
            $(value).attr('name', newName);
        });

        indexNameRisk++
        countNameRisks = $('[name*="Risks[' + indexNameRisk + ']."]').length;
    }

    Validation.Destroy();
    Validation.Init();
}
function showErrorMessage(message) {
    var modalContent = $("#errorSaveRiskMatrix div.modal-body");
    modalContent.empty();
    modalContent.html("<p>" + message + "</p>");
    $("#errorSaveRiskMatrix").modal()
}

function showElementsNotValid() {
    var elements = $('span.validation-element:not(.hide)');
    $.each(elements, function (index, value) {
        var firsttr = $(value).closest('tr[data-row-response-plan]');
        firsttr.removeClass('hide');
        idRiskValidate = firsttr.attr('data-row-response-plan');
        $.each(firsttr.find('.hide').not('span.validation-element').not('input.hide'), function (index, value) {
            $(value).removeClass('hide');
            $(value).closest('tr').find('th.tree.icon span.icon').text('-');
            var tr = $('tr[data-id="' + idRiskValidate + '"]');
            var btnCollapse = tr.find('td.tree.icon span.icon');
            btnCollapse.text('-');
            var tempCollapseAll = $('div.btnCollapseAll.buttonExpand[dd-contenedores*="' + $(btnCollapse).closest('table').attr('id') + '"]');
            IsAllCollapseRiskMatrix(tempCollapseAll);
        });
    });
}

function unexistTableActivity(riskId) {
    var divResponsePlan = $('#divResponsePlan_' + riskId);
    if (divResponsePlan.hasClass('notShowResponsePlan')) {
        return true;
    } else {
        return false;
    }
}

function hideDivResponsePlan(RiskactivityDelete) {
    var divResponsePlan = $('#divResponsePlan_' + RiskactivityDelete);
    if (divResponsePlan.find('table tbody').html().trim() == "") {
        divResponsePlan.addClass('notShowResponsePlan');
    }
    
}