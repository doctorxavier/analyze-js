//modal windows for basic data web page
var lastDocument = 0;
$(document).ready(function () {
    modalTabstrips();
    $("#browserDoc").kendoUpload();

    $('#SearchButton').click(function () {
        $.post($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                IDBDocNumber: $("#idbdocsNumber").val(),
                DocumentName: $("#documentName").val(),
                DocumentID: $("#documentID").val(),
                DocumentContent: $("#documentContent").val(),
                OperationNumber: $("#operationNumber").val(),
                ContractNumber: $("#contractNumber").val(),
                InputCorrespondenceNumber: $("#documentNumberInput").val(),
                IDBAuthorUserId: $("#idbAuthorUserId").val(),
                InputCorrespondenceYear: $("#inputCorrespondenceYear option:selected").val(),
                InputCorrespondenceAssignedTo: $("#AsignedtoInput").val(),
                OutputCorrespondenceNumber: $("#ReferencesNumberOutput").val(),
                OutputCorrespondenceYear: $("#outputCorrespondenceYear option:selected").val(),
                Reference: $("#Reference").val()
            },
            function (data) {
                var dialog = $(".dinamicModal").data("kendoWindow");
                dialog.content(data);
            })
    });

    function closeBtn() {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
        $(".k-window-titlebar").remove();
    }
    $("#muestratodoOutput").hide();
    $("#muestratodoInput").hide();

    SelectedTabAction("AddDocClauses");

    $('#UploadButton').click(function () {
        $.post($(this).data("route"))
    });

    $('#cancel').click(function () {
        closeBtn();
    });

    function checkFile() {
        var fileElement = document.getElementById("file");
        var fileExtension = "";
        if (fileElement.value.lastIndexOf(".") > 0) {
            fileExtension = fileElement.value.substring(fileElement.value.lastIndexOf(".") + 1, fileElement.value.length);
        }
        if (fileExtension == "txt") {
            return true;
        }
        else {
            alert("You must select a txt file for upload");
            return false;
        }
    }
});

//Muestra todo Output
function getOptionValueOutput(radio)
{
    if ($("#outputCorrespondence").is(':checked'))
    {
        $("#muestratodoOutput").hide();
    }
    else
    {
        $("#muestratodoOutput").show();
    }
}

//Muestra todo Input
function getOptionValueInput(radio)
{
    if ($("#inputCorrespondence").is(':checked'))
    {
        $("#muestratodoInput").hide();
    }
    else
    {
        $("#muestratodoInput").show();
    }
}

//Pestaña seleccionada
function SelectedTabAction(Tab)
{
    switch (Tab)
    {
        case "AddDocClauses":
            $('#UploadButton').show();
            $('#SearchButton').hide();
            break;
        case "SearchDocClauses":
            $('#SearchButton').show();
            $('#UploadButton').hide();
            break;
    }
}
