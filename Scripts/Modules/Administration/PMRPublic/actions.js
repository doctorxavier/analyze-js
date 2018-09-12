var State = null;
$(document).on("ready", function () {

    $(document).on('click', '.deleteDocument', function () {
        var documentId = $(this).data("documentnumber");
        var model = $('#idpartial').data('requestFilter');

        if (model === undefined || model === '') {
            model = $('#FormPMRPublic').serialize();
        }
        var data = model + "&DocumentNumber=" + $(this).data("documentnumber");

        callFilterRequest($(this).data("route"), data, model)
    });

    $("#UploadPMRPublic").on("click", function () {
        $("#operations").val(GetOperationsSelectedFromTable());

        if ($("#operations").val() == null ||
            $("#operations").val().length == 0) {
            return;
        }

        $("#pmrCycleId").val($("#PMRCycles").find(":selected").val());
        $("#pmrCycleName").val($("#PMRCycles option:selected").text());

        $("#UploadPMRPublic").addClass('disabled');
        $("#UploadPMRPublic").prop('disabled', true);

        var model = $('#FormPMRPublic').serialize();

        callFilterRequest($(this).data("route"), model, model);

        $("#UploadPMRPublic").removeClass('disabled');
        $("#UploadPMRPublic").prop('disabled', false);
    });

    $("#AuthorizePMRPublic").on("click", function () {
        $("#authorizeDocuments").val(GetDocumentsSelectedFromTable());

        if ($("#authorizeDocuments").val() == null ||
            $("#authorizeDocuments").val().length == 0) {
            return;
        }

        $("#AuthorizePMRPublic").addClass('disabled');
        $("#AuthorizePMRPublic").prop('disabled', true);

        var model = $('#FormPMRPublic').serialize();
        callFilterRequest($(this).data("route"), model, model);

        $("#AuthorizePMRPublic").removeClass('disabled');
        $("#AuthorizePMRPublic").prop('disabled', false);
    });

    $("#CheckAllUpload").on("click", function () {
        $('input:checkbox.uploadcheck').each(function () {
            if ($(this).data("operation") != "no_applicable") {
                $(this).prop('checked', true);
            }
        })
    });

    $("#UncheckAllUpload").on("click", function () {
        $('input:checkbox.uploadcheck').each(function () {
            if ($(this).data("operation") != "no_applicable") {
                $(this).prop('checked', false);
            }
        })
    });

    $("#CheckAllAuthorize").on("click", function () {
        $('input:checkbox.authorizecheck').each(function () {
            if ($(this).data("document") != "no_applicable") {
                $(this).prop('checked', true);
            }
        })
    });

    $("#UncheckAllAuthorize").on("click", function () {
        $('input:checkbox.authorizecheck').each(function () {
            if ($(this).data("document") != "no_applicable") {
                $(this).prop('checked', false);
            }
        })
    });

    function BuildReportParameters() {
        var parameters = "&";
        $('input:checkbox:checked.uploadcheck').each(function () {
            if (!$(this).is(':disabled') && $(this).data("operation") != "no_applicable") {
                var operationInfo = $(this).data("operation");
                parameters += "OPERATION=" + operationInfo.split(";")[1] + "&";
            }
        })

        if (parameters == "&") {
            return;
        }

        parameters += "CYCLE=" + $("#PMRCycles").find(":selected").val();
        return parameters;
    }

    function GetOperationsSelectedFromTable(control) {
        var operations = "";
        $('input:checkbox:checked.uploadcheck').each(function () {
            if (!$(this).disabled && $(this).data("operation") != "no_applicable") {
                operations += $(this).data("operation") + "|";
            }
        });

        if (operations == "")
            return null;

        //Remove the last separator '|'
        return operations.substr(0, operations.length - 1);
    }

    function GetDocumentsSelectedFromTable(control) {
        var documents = "";
        $('input:checkbox:checked.authorizecheck').each(function () {
            if (!$(this).disabled && $(this).data("document") != "no_applicable") {
                documents += $(this).data("document") + "|";
            }
        })

        if (documents == "")
            return null;

        //Remove the last separator '|'
        return documents.substr(0, documents.length - 1);
    }

});

function openLinkInNewWindow(link) {
    var isChrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;
    if (isChrome) {
        var a = document.createElement("a");
        a.href = link;
        var evt = document.createEvent("MouseEvents");
        evt.initMouseEvent("click", true, true, window, 0, 0, 0, 0, 0,
        true, false, false, false, 0, null);
        a.dispatchEvent(evt);
    } else {
        var win = window.open(link, '_blank');
    }
}

function getReportParameters() {
    var parameters = "&";
    $('input:checkbox:checked.uploadcheck').each(function () {
        if (!$(this).is(':disabled') && $(this).data("operation") != "no_applicable") {
            var operationInfo = $(this).data("operation");
            parameters += "OPERATION=" + operationInfo.split(";")[1] + "&";
        }
    })

    if (parameters == "&") {
        return parameters;
    }

    parameters += "CYCLE=" + $("#PMRCycles").find(":selected").val();

    return parameters;
}