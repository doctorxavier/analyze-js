var load_ = false;
var initialInterval = false;

$(function () {
    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            if ($(this).attr('data-val-range'))
                return $(this).attr('data-val-range');
            if ($(this).attr('data-val-required'))
                return $(this).attr('data-val-required');
        }
    });
});

function initializeSplitViewState() {

    if ($('#fullTaskContainer').find("button[name='minMaxFloatTask']").size() > 0)
    {
        return;
    }

    var newButton = '<div class="col-md-3 text-right floatRight">';
    newButton += ValueObjGW.LabelMaximizeTask;
    newButton += ValueObjGW.ButtonCloseFloatTask;
    newButton += '</div>';

    var newCommentButton = ValueObjGW.ButtonNewComment;

    var newInstruction = '<div class="col-sm-9 nopadding mt5 mb10" id="instructionFT">';
    newInstruction += '<label class="labelnormal">' + ValueObjGW.instructionsFT +'</label></div>';

    $('#fullTaskContainer').prepend(newButton.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"'));
    $(".box_subtitle").prepend(newInstruction.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"'));

    $('.blueButtonsBlock').append(newCommentButton);

    $('#fullTaskContainer').find('.headingh2').eq(0).width("10%");

    $('body').css('overflow', 'hidden');
    $('body').css('-ms-scroll-limit-y-max', '0');
}

function cleanComment() {
    $("div.inputComment").each(function () {
        if ($(this).find('[name=newComment]').val() === "") {
            $(this).parent().remove();
        }
    });

    if ($('[name=newComment]').text().trim() === '') {
        $('[name=newComment]').siblings("div.inputComment").parent().remove();
    }
}

function setSplitViewState(collapsed) {

    if (collapsed) {
        $('iframe[name="splitFrame"]').show();
        $('.taskBodyContainer').hide();
        $('#fullTaskContainer').addClass("floatBox");
        $('#fullTaskContainer').find('button:contains('+ ValueObjGW.cancelLiteral +')').hide();
        $('#fullTaskContainer').find('button:contains('+ ValueObjGW.editLiteral +')').hide();
        $('#fullTaskContainer').find('button').filter(function () {
            return $(this).text().trim() === ValueObjGW.saveLiteral;
        }).hide();
        $('#instructionFT').show();
        $('[name=minMaxFloatTask]').text(ValueObjGW.maximizeTaskLiteral);
        $('[name=closeFloatTask]').show();
        $('[name=newCommentFloatTask]').show();

        cleanComment();

        $('[name=minMaxFloatTask]').removeClass("plus");
        $('.tMissionCode').hide();

        $('.title_tag').insertAfter($('#fullTaskContainer').find('.headingh2').eq(0)).addClass("pt5").addClass("pl5");
        $('#fullTaskContainer').find('.headingh2').eq(0).hide();
        $('.box_subtitle').addClass('mt10');

        parent.parent.$(".lista_desplegable_b").find("a").attr("target", "splitFrame");
        return;
    }

    $('iframe[name="splitFrame"]').hide();
    $('.taskBodyContainer').show();
    $('#fullTaskContainer').removeClass("floatBox");
    $('#fullTaskContainer').find('button:contains(' + ValueObjGW.cancelLiteral + ')').show();
    $('#fullTaskContainer').find('button:contains(' + ValueObjGW.editLiteral + ')').show();
    $('#fullTaskContainer').find('button').filter(function () {
        return $(this).text().trim() === ValueObjGW.saveLiteral;
    }).show();
    $('#instructionFT').hide();
    $('[name=minMaxFloatTask]').text(ValueObjGW.minimizeTaskLiteral);
    $('[name=closeFloatTask]').hide();
    $('[name=newCommentFloatTask]').hide();

    $('[name=minMaxFloatTask]').addClass("plus");
    $('.tMissionCode').show();

    $('#fullTaskContainer').find('.headingh2').eq(0).show();
    $('.box_subtitle').prepend($('.title_tag').removeClass("pt5").removeClass("pl5")).removeClass("mt10");

    parent.parent.$(".lista_desplegable_b").find("a").attr("target", "someFrame");
}

function newCommentFloatTask(source) {
    minMaxFloatTask(source);
    newComment(source);
    $('[name=newComment]').focus();
}

function closeFloatTask(source) {
    parent.parent.$(".lista_desplegable_b").find("a").attr("target", "someFrame");
    $('#fullTaskContainer').hide();
}

function minMaxFloatTask(source) {
    setSplitViewState(source.hasClass("plus"));
}

$(document).ready(function () {
    {
        initializeSplitViewState();
         $("#layoutLoadingDiv").show();
        setSplitViewState("true");
        $(".operationData").css("padding-left", "0.5%");
    }

    parent.parent.$(".menu_max").click();

    var newUrl = null;

    $("#ConvergencePartialForm").attr("action", newUrl);
    $('.operationData.genericWorkflow').css('width', '100%');
    $('.box_subtitle').css('width', '100%');

    var isEditMode = $('#editModeModel').val();

    if (isEditMode === "True") {
        editRequest();
    }

});

function editRequest() {
    enterEditMode(false, $('#workflowComments'), true);
    enterEditMode(false, $('.blueButtonsBlock'), true);
    enterEditMode(false, $('#workflowValidators'), true);
    enterEditMode(false, $('#workflowDocuments'), true);
    showOrHideValidatorsAddition();
}

function cancelRequestMissions() {
    exitEditMode(false, $('#workflowComments'), true, true);
    exitEditMode(false, $('.blueButtonsBlock'), true, true);
    exitEditMode(false, $('#workflowValidators'), true, true);
    exitEditMode(false, $('#workflowDocuments'), true, true);
}

function cancelTask() {
    exitEditMode(false, $('#workflowComments'), true, true);
    exitEditMode(false, $('.blueButtonsBlock'), true, true);
    exitEditMode(false, $('#workflowValidators'), true, true);
    exitEditMode(false, $('#workflowDocuments'), true, true);
    SaveGW.ShowValidator();
}

function cancelCreateworkflow() {
    //window.location.href = '@Url.Action("MissionCompleteView", "View", new { area = "OpusMissions", missionId = Model.missionId })';
}

function saveAndStartWorkflow() {
    $("[name=changeStatus]").val("StartWorkflow");
    var url = ValueObjGW.urlSaveStartWorkflow;
    processWorkFlow(url);

}

function saveOnly() {
    $("[name=changeStatus]").val("none");
    SaveGW.saveData(ValueObjGW.urlSaveOnly);
}

function setTaskAction(source) {
    var modals = $(source.currentTarget).parents('div[data-id=mainContainer]').find('div.modalBody');
    var action = "";
    if (modals.length > 0) {
        cleanComment();
        action = modals[0].id.substr(5);
        if (modals.data("validcomment") === "True" && $("div.inputComment").length < 1) {
            confirmAction(ValueObjGW.commentMandatoryLiteral);
            return;
        }
    }

    $("[name=changeStatus]").val(action);
    var url = ValueObjGW.urlTaskAction;
    SaveGW.saveData(url);
}

function processWorkFlow(url) {
    showLoader();
    var vexModal = $('.missionsModal').parents('.vex');
    if (vexModal.length > 0) {
        vex.closeByID(vexModal.data().vex.id);
    }

    window.setTimeout(function (url) {
        var response = saveContainer($('#pageContent'), '#hiddenModel', true, null, null, false, true, null, false);
        if (response !== false) {
            response.done(function (data) {
                processWorkflowData(data, url);
            });
        } else {
            hideLoader();
        }
    }, 10, url);
}

function addValidator(source) {
    var newRoleId = $("#id-newRole").GetValue();
    var firstLi = $("[aria-labelledby=id-newRole] li a").first();

    if (newRoleId) {
        var next, lastAdd;

        if ($("#tableWorkflowValidators tr td:contains('Additional')").length > 0) {
            lastAdd = $("#tableWorkflowValidators tr td.status").filter(function () {
                return $(this).html().trim() === "";
            }).last().parent();

            next = parseInt(lastAdd.find("td").first().text()) + 1;
        } else {
            next = 1;
        }

        var a = $('#tableWorkflowValidators tbody tr').filter(function (index) {
            return $(this).find('td.status').html().trim() === "";
        });

        var b = $('#tableWorkflowValidators tbody tr').filter(function (index) {
            return $(this).find('td.status').html().trim() === "Pending" && $(this).find('td').eq(1).html().trim() !== a.find('td').eq(1).html().trim();
        });

        var newRole = $("[aria-labelledby=id-newRole] li a[dd-selected]").text();
        var newRow = '<tr data-id><td>' + next + '</td>';
        var PrincipalTaskName = $("h4.title_tag").html();
        newRow = newRow + "<td >" + PrincipalTaskName + "</td>";
        newRow = newRow + "<td >" + newRole + '<div><span data-pagemode="edit"><input type="hidden" value="' + newRole + '" required=required name="newUserProfile"></span></div>' + "</td>";
        newRow = newRow + "<td class ='status'>Pending</td>" + "<td>Additional Validator</td>" + "<td></td>";
        newRow = newRow + "<td>" + ValueObjGW.ButtonDeleteValidator;
        newRow = newRow + "</td></tr>";

        //b.first().prev().after(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177'));
        //lastAdd = $("#tableWorkflowValidators tbody tr").first();

        if (next > 1)
            $(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177')).insertAfter(lastAdd);
        else {
            $("#tableWorkflowValidators").prepend(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177'));
            lastAdd = $("#tableWorkflowValidators tbody tr").first();
        }

        enumValidators(lastAdd, 1);

        enterEditMode(false, lastAdd, false);
        bindHandlers(lastAdd);

        $("[aria-labelledby=id-newRole] li a[dd-selected]").parent().remove();
        firstLi.attr("dd-selected");
        $("[aria-labelledby=id-newRole]").SetValue(firstLi.val(), firstLi.text().trim());
    }
    else {
        showMessage(ValueObjGW.missionWorkflowValidatorLiteral);
    }
}

function deleteValidator(source) {
    if (source.attr("data-order")) {
        $("[name=deleteValidators]").val($("[name=deleteValidators]").val() + source.attr("data-order") + "|");
    }

    $("[aria-labelledby=id-newRole] li:eq(0)").after("<li><a dd-value=2>" + source.parents("tr").find("td").eq(2).text() + "</a></li>");

    enumValidators(source.parents("tr"), 0);

    source.parents("tr").remove();
}

function enumValidators(trChange, plus) {
    var next = parseInt(trChange.find("td").first().text()) + plus;

    trChange.nextAll("tr").each(function () {
        $(this).find("td").first().text(next);
        next++;
    });
}

function newComment(source) {
    //newComment
    var newBlock = '<div data-pagemode="edit"  class="pb20">' +
        ValueObjGW.inputCommentBox;
$("[name=commentsCount]").val($("[name=commentsCount]").val() + 1);
$("#divWorkflowComments").append(newBlock.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177'));

enterEditMode(false, $("#divWorkflowComments"), false);
bindHandlers($("#divWorkflowComments"));
    }

function deleteComment(source) {
    if (source.attr("data-commentId")) {
        $("[name=deleteComments]").val($("[name=deleteComments]").val() + source.attr("data-commentId") + "|");
    }
    $("[name=commentsCount]").val($("[name=commentsCount]").val() - 1);
    source.siblings("div.inputComment").parent().remove();
}

function downloadDocument(source) {
    var docNumber = source.attr('data-DocNumber');
    var url = ValueObjGW.urlDownloadDocument + '?documentNumber=' + docNumber;
    window.open(url, '_blank');
}

function renderizeRow(documentList, sourceType, fileNames) {
    var docNumber;
    var fileName;
    if (documentList.length <= 0)
        return;
    if (sourceType === "selected") {
        if (documentList.length > 0) {
            docNumber = documentList[0].DocumentNumber;
            fileName = documentList[0].DocumentName;
        }
    }
    else {
        if (documentList.length > 0) {
            docNumber = documentList[0];
        }
        if (fileNames.length > 0) {
            fileName = fileNames[0];
        }
    }

    var urlf = ValueObjGW.urlRenderizeRow;

    var result = postUrl(urlf, { documentNumber: docNumber });
    var newRow = '<tr role="row" data-id="">' +
        '<td>' + result.responseJSON.data.User + '</td>' +
        '<td>' + result.responseJSON.data.Date + '</td>' +
        '<td class="docNumber">' + '<label class="labelNormal">' + result.responseJSON.data.DocumentNumber + '</label>' + '</span> </td>' +
        '<td> ' + fileName +
        '<span data-pagemode="edit">' + ValueObjGW.documentnumber + '</span>' +
        '</td><td>' + ValueObjGW.ButtonDownloadDocument +
        '</td></tr>';

    $('#tblWFDocuments').append(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&amp;#59177/g, '&#59177').replace(/docNumberValue/g, result.responseJSON.data.DocumentNumber).replace(/documentDefaultDescription/g, fileName));
    $('#tblWFDocuments tbody tr td').last().find("[data-docnumber=docNumberValue]").attr("data-docnumber", result.responseJSON.data.DocumentNumber);
}

function showOrHideValidatorsAddition() {
    var validatorsContainer = $("div.validatorsContainer");

    var statusArray = $("#tableWorkflowValidators").
                            find("tbody").
                            find("tr").
                            find("td.status").
                            map(function () { return $(this).text() }).
                            get().join();

    if (statusArray.indexOf("Pending") < 0) {
        validatorsContainer.addClass("hide");
    } else {
        validatorsContainer.removeClass("hide");
    }
}

function removeValidator(obj)
{
    SaveGW.HideValidator(obj);
}

function processWorkflowData(data, url) {
    if (!data.IsValid) {
        if (data.ErrorMessage !== null || data.ErrorMessage !== '')
            showMessage(data.ErrorMessage);
        else
            showMessage(ValueObjGW.errorSavingLiteral);
    }
    else {
        window.location.href = url;
    }
}
