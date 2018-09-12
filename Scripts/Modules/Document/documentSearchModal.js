//modal windows for basic data web page
var lastDocument = 0;
$(document).ready(function () {
    modalTabstrips();

    $('#SearchButton').click(function () {
        var ajaxContainer = $("#loadingContainer");
        var panel = $("#target");
        panel.addClass("loading-overlay");
        ajaxContainer.css("display", "");
        kendo.ui.progress(ajaxContainer, true);
        $.post($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                entityRelated: $("#entityRelated").val(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val(),
                subsubParentEntityId: $("#subsubParentEntityId").val(),
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
                Reference: $("#Reference").val(),
                DocumentEvaluationTrackingId: $("#DocumentEvaluationTrackingId").val(),
                StageId: $("#Stage").val(),
                TypeOfDocumentId: $("#TypeOfDocument").val(),
                DocumentPermissionsId: $("#DocumentPermissions").val()
            },
            function (data) {
                if (typeof iframeMode == 'undefined') {
                    var dialog = $(".dinamicModal").data("kendoWindow");
                    kendo.ui.progress(ajaxContainer, false);
                    panel.removeClass("loading-overlay");
                    ajaxContainer.css("display", "none");
                    if (data == null) {
                        dialog.close();
                    } else {
                        dialog.content(data);
                    }
                } else {
                    $('.idb-content-page').html(data);
                }
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

    SelectedTabAction("SearchDocClauses");

    $(".k-tabstrip-items.k-reset.tabs").find("li > a").css("text-decoration", "none");
    $("#SearchDocClauses").addClass('k-state-active');

    $('#upload').click(function (e) {
        if ($('#upload').attr('aria-describedby') != null) {
            $('#upload').removeAttr('title');
            $("#upload").tooltip("destroy");
        }
    });

    $('#UploadButton').click(function (e) {
        e.preventDefault();

        if ($('#upload').val() != "") {
            var RutaNombreDocumento = $('#upload').val().toString().split('\\');
            if (RutaNombreDocumento.length > 0) {
                var NombreDocumento = RutaNombreDocumento[RutaNombreDocumento.length - 1];
                var NombreExtension = NombreDocumento.toString().split('.');

                if (NombreExtension.length > 0) {
                    var Extension = NombreExtension[NombreExtension.length - 1];
                    var ExtensionAceptada = "";

                    switch (Extension) {
                        case "pdf": ExtensionAceptada = "ACROBAT"; break;
                            //Formato Archivos Excel
                        case "xls": ExtensionAceptada = "MS EXCEL"; break;
                        case "xlsx": ExtensionAceptada = "MS EXCEL"; break;
                            //Formato archivos Outlook
                        case "pst": ExtensionAceptada = "MS OUTLOOK"; break;
                            //Formato archivos PowerPoint
                        case "ppt": ExtensionAceptada = "MS POWERPOINT"; break;
                        case "pptx": ExtensionAceptada = "MS POWERPOINT"; break;
                            //Formato archivos Microsoft Project
                        case "mpp": ExtensionAceptada = "MS PROJECT"; break;
                            //Formato Archivos Microsoft Publisher
                        case "pub": ExtensionAceptada = "MS PUBLISHER"; break;
                            //Formato Archivos Microsoft Visio
                        case "vsd": ExtensionAceptada = "VISIO"; break;
                        case "vss": ExtensionAceptada = "VISIO"; break;
                        case "vst": ExtensionAceptada = "VISIO"; break;
                        case "vdx": ExtensionAceptada = "VISIO"; break;
                        case "vsx": ExtensionAceptada = "VISIO"; break;
                        case "vtx": ExtensionAceptada = "VISIO"; break;
                            //Formato Archivos Microsoft Word
                        case "doc": ExtensionAceptada = "MS WORD"; break;
                        case "docx": ExtensionAceptada = "MS WORD"; break;
                            //Formato Archivos Microsoft Word Perfect
                        case "wpd": ExtensionAceptada = "WORDPERFECT"; break;
                        case "msg": ExtensionAceptada = "MS OUTLOOK"; break;
                        case "dta": ExtensionAceptada = "NOTEPAD"; break;
                        case "do": ExtensionAceptada = "NOTEPAD"; break;
                        case "sps": ExtensionAceptada = "NOTEPAD"; break;
                            //Si el formato no se encuentra dentro de los anteriores casos se tomara por defecto 
                        default: ExtensionAceptada = ""; break;
                    }

                    if (ExtensionAceptada != "") {
                        $("#targetDoc").submit();
                    }
                    else {
                        $('#upload').attr('Title', $('#ErrorExtension').val());
                        $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
                        $("#upload").tooltip("option", "position", { my: "left-50% bottom+240%" });
                        $('#upload').focus();
                    }
                }
                else {
                    $('#upload').attr('Title', $('#ErrorArchivo').val());
                    $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
                    $("#upload").tooltip("option", "position", { my: "left-60% bottom+240%" });
                    $('#upload').focus();
                }
            }
            else {
                $('#upload').attr('Title', $('#ErrorArchivo').val());
                $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
                $("#upload").tooltip("option", "position", { my: "left-60% bottom+240%" });
                $('#upload').focus();
            }
        }
        else {
            $('#upload').attr('Title', $('#ErrorArchivo').val());
            $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
            $("#upload").tooltip("option", "position", { my: "left-60% bottom+240%" });
            $('#upload').focus();
        }
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

    getYears();
});

function getOptionValueOutput(radio) {
    if ($("#outputCorrespondence").is(':checked')) {
        $("#muestratodoOutput").hide();
    }
    else {
        $("#muestratodoOutput").show();
    }
}

function getOptionValueInput(radio) {
    if ($("#inputCorrespondence").is(':checked')) {
        $("#muestratodoInput").hide();
    }
    else {
        $("#muestratodoInput").show();
    }
}

function SelectedTabAction(Tab) {
    $('[data-id-selector=tabContainer]').html("");
    $('[data-id-selector=tabContainer2]').html("");
    $('[data-id-selector=tabContainer3]').html("");

    switch (Tab) {
        case "AddDocClauses":
            $('#UploadButton').show();
            $('#AddDoc').show();
            $('#SearchButton').hide();
            $('#draftModal').hide();
            $('#draftButton').hide();
            $('#CountDocumentSearch').hide();

            $(".tab1").css("display", "block");
            $(".tab2").css("display", "none");
            break;
        case "SearchDocClauses":
            $('#SearchButton').show();
            $('#UploadButton').hide();
            //$('#draftModal').hide();
            $('#draftButton').hide();

            $(".tab1").css("display", "none");
            $(".tab2").css("display", "block");
            break;
    }
}

function getYears() {
    var instanceYears = new Date();
    var year = instanceYears.getFullYear();
    var newhtml = "";

    newhtml += "<option value=''>Year</option>";
    for (var i = year; i >= (year - 10) ; i--) {
        newhtml += "<option value=" + i + ">" + i + "</option>";
    }

    $("#inputCorrespondenceYear").append(newhtml);
    $("#outputCorrespondenceYear").append(newhtml);

}

function downConteinerModal() {
    $($(".k-widget.k-window", window.parent.document)[0]).css("display", "none");
    $(".ui-widget-overlay.ui-front").hide();
}

function tabs(element) {
    $(element).closest('ul.tabs').find('li').removeClass('k-state-active');
    $(element).addClass('k-state-active');
}