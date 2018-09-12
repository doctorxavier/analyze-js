//modal windows for basic data web page
var lastDocument = 0;
$(document).ready(function () {
    modalTabstrips();

    var thumbnail = $(".thumbnail");
    if ($(thumbnail).length > 0) {
        $(thumbnail).click(function () {
            if ($(this).hasClass("active")) {
                $(this).removeClass("active");
                $('#docName').text('Document name');
                $('#idbNumber').text('IDB Doc Number');
                $('#date').text('DD MMM YY');
                $('#docID').text('docID');
                $('#docDesc').text('Document Content');
                $('#docAuthor').text('Doc Author');
                $('#opNum').text('12345');
                $('#contractNumber').text('CN-12345');
                $('#downloadDoc').enabled = false;
            } else {
                /* activate thumbnail */
                $(this).addClass("active");
                var testData = $(this).data("docinfo");
                //$('#docName').text(testData.docName);
                $('#docName').text('DOCNAME');
                $('#idbNumber').text(testData.idbNumber);
                $('#date').text(testData.date);
                $('#docID').text(testData.docID);
                $('#docDesc').text(testData.docDesc);
                $('#docAuthor').text(testData.docAuthor);
                $('#opNum').text(testData.opNum);
                $('#contractNumber').text(testData.contractNumber);
                $('#downloadDoc').enabled = true;
            }
        });
    }

    $('#SearchButton').click(function () {
        $('.conten_spinner_dialog').css('display', 'block');
        $('.conten_spinner_dialog').css('visibility', 'visible');
        $.post($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val(),
                entityRelated: $("#entityRelated").val(),
                IDBDocNumber: $("#idbdocsNumber").val(),
                DocumentName: $("#documentName").val(),
                DocumentID: $("#documentID").val(),
                DocumentContent: $("#documentContent").val(),
                OperationNumber: $("#operationNumber").val(),
                ContractNumber: $("#contractNumber").val(),
                InputCorrespondenceNumber: $("#documentNumberInput").val(),
                IDBAuthorUserId: $("#idbAuthorUserId").val(),
                //Pendiente aclarar esta parte
                //InputCorrespondenceYear: $("#inputCorrespondenceYear option:selected").val(),
                InputCorrespondenceAssignedTo: $("#AsignedtoInput").val(),
                OutputCorrespondenceNumber: $("#ReferencesNumberOutput").val(),
                //OutputCorrespondenceYear: $("#outputCorrespondenceYear option:selected").val(),
                Reference: $("#Reference").val()
            }).done(function (data) {
                $('.conten_spinner_dialog').css('display', 'none');
                $('.conten_spinner_dialog').css('visibility', 'hidden');
                var dialog = $(".dinamicModal").data("kendoWindow");
                dialog.content(data);
                modalTabstrips();
            }).fail(function () {
                $('.conten_spinner_dialog').css('display', 'none');
                $('.conten_spinner_dialog').css('visibility', 'hidden');
                alert("Data not found!");
            });

    });

    function checkSize(max_file_size) {
        var input = document.getElementById("upload");
        // check for browser support (may need to be modified)
        if (input.files && input.files.length == 1) {
            if (input.files[0].size > max_img_size) {
                alert("The file must be less than " + (max_file_size / 1024) + "KB");
                return false;
            }
        }
        return true;
    }

    function closeBtn() {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
        $(".k-window-titlebar").remove();
    }
    $("#muestratodoOutput").hide();
    $("#muestratodoInput").hide();

    $('#cancel').click(function () {
        closeBtn();
    });

    var tabToSelect = $('#isFirstTime').val();
    if (tabToSelect == "False") {
        SelectedTabAction("SearchDocClauses");

        var classToShowTab = $('#AddDocClauses').attr("class");
        var classToShowTabContent = $('#undefined-1').attr("class");
        var classToHideTab = $('#SearchDocClauses').attr("class");
        var classToHideTabContent = $('#undefined-2').attr("class");
        $('#AddDocClauses').attr("class", classToHideTab);
        $('#undefined-1').attr("class", classToHideTabContent);
        $('#undefined-1').css(["display:none;"]);
        $('#undefined-1').attr("aria-expanded", "false");
        //$('#undefined-1').attr("aria-hidden", "true");
        $('#SearchDocClauses').attr("class", classToShowTab);
        $('#undefined-2').attr("class", classToShowTabContent);
        $('#undefined-2').css(["display: block;"]);
        $('#undefined-2').attr("aria-expanded", "true");
        //$('#undefined-2').removeAtt("aria-hidden");

    }
    else
        SelectedTabAction("AddDocClauses");
});

//Muestra todo Output
function getOptionValueOutput(radio) {
    if ($("#outputCorrespondence").is(':checked')) {
        $("#muestratodoOutput").hide();
    }
    else {
        $("#muestratodoOutput").show();
    }
}

//Muestra todo Input
function getOptionValueInput(radio) {
    if ($("#inputCorrespondence").is(':checked')) {
        $("#muestratodoInput").hide();
    }
    else {
        $("#muestratodoInput").show();
    }
}

//Pestaña seleccionada
function SelectedTabAction(Tab) {
    switch (Tab) {
        case "AddDocClauses":
            $('#AddButton').show();
            $('#SearchButton').hide();
            break;
        case "SearchDocClauses":
            $('#SearchButton').show();
            $('#AddButton').hide();
            break;
    }
}
