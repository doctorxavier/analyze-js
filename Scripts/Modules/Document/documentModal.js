//modal windows for basic data web page
var lastDocument = 0;
var operationNumberSearch = "";
$(document).ready(function () {
    modalTabstrips();

    $('#AddDocRelationship').click(function () {
        showLoader();
        var url = $(this).data("route");
        var documentSelected = $('tr.selected').data('json');
        if (documentSelected == null) {
            var errorDocument = "#errorDocumentSelectedSearch";
            $(errorDocument).show();
            hideLoader();
            return;
        }
        $.post($(this).data("route"),
          {
              OperationId: $("#operationId").val(),
              mainOperationNumber: $("#mainOperationNumber").val(),
              IDBDocNumber: documentSelected.DocNum,
              DocumentName: documentSelected.DocName,
              DocumentContent: documentSelected.Content,
              entityRegisterId: $("#entityRegisterId").val(),
              parentEntityId: $("#parentEntityId").val(),
              subParentEntityId: $("#subParentEntityId").val(),
              entityRelated: $("#entityRelated").val(),
              DocumentEvaluationTrackingId: $('#DocumentEvaluationTrackingId').val(),

          }).done(function (data) {


              try {
                  if (data.indexOf('DocumentNewSectionVP') >= 0) {
                      window.parent.addDocumentNewSectionDocumentVP("-1", documentSelected.AuthorId, documentSelected.CreationDate, documentSelected.DocNum, documentSelected.DocName)
                  } else if (data.indexOf('DocumentNewSectionVO') >= 0) {
                      window.parent.addDocumentNewSectionDocumentVO("-1", documentSelected.AuthorId, documentSelected.CreationDate, documentSelected.DocNum, documentSelected.DocName)
                  } else {
                      window.parent.documentAdd(null, documentSelected.DocNum, documentSelected.DocName, true);
                  }
              } catch (e) {
                  location.reload(true);
              }


          }).fail(function () {
              alert("Document Can not be added to Clause!");
              location.reload(true);
          });
    });

    function redirectPage(url) {
        var form = $("#main-form");
        if (url != "") {
            form.attr("action", url);
        }
        form.submit();

    };

    $('#SearchButton').click(function () {
        showLoaderOptional();
        var url = $(this).data("route");
        $.ajax({
            url: url,
            type: 'GET',
            async: true,
            data: {
                OperationNumber: $("#operationNumber").val(),
                ContractNumber: $("#contractNumber").val(),
                IDBDocNumber: $("#idbdocsNumber").val(),
                DocumentName: $("#documentName").val(),
                DocumentID: $("#documentID").val(),
                IDBAuthorUserId: $("#idbAuthorUserId").val(),
                DocumentContent: $("#documentContent").val(),
                InputCorrespondenceNumber: $("#documentNumberInput").val(),
                InputCorrespondenceYear: $("#inputCorrespondenceYear option:selected").val(),
                InputCorrespondenceAssignedTo: $("#AsignedtoInput").val(),
                OutputCorrespondenceNumber: $("#ReferencesNumberOutput").val(),
                OutputCorrespondenceYear: $("#outputCorrespondenceYear option:selected").val(),
                Reference: $("#Reference").val(),
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                entityRelated: $("#entityRelated").val(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val(),
                subsubParentEntityId: $("#subsubParentEntityId").val(),
                DocumentEvaluationTrackingId: $("#DocumentEvaluationTrackingId").val(),
                StageId: $("#Stage").val(),
                TypeOfDocumentId: $("#TypeOfDocument").val(),
                DocumentPermissionsId: $("#DocumentPermissions").val()
            },
            success: function (data, textStatus, jqXHR) {
                $('#dmSearchForm').hide();
                $('#dmSearchResult').html(data);
                $('#dmSearchResult').show();
                $('#AddDocRelationship').show();
                $('#SearchButton').hide();
                hideLoaderOptional();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                hideLoaderOptional();
            }

        });
    });

    function closeBtn() {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
        $(".k-window-titlebar").remove();
        $(".k-overlay", window.parent.document).remove();
    }

    $("#muestratodoOutput").hide();

    $("#muestratodoInput").hide();

    SelectedTabAction("AddDocClauses");
    operationNumberSearch = $("#operationNumber").val();

    $('#upload').click(function (e) {
        $("#errorDocumentSelectedAdd").css('display', 'none');
    });

    $('#UploadButton').click(function (e) {
        e.preventDefault();
        var errorDocumentSelectedAdd = "#errorDocumentSelectedAdd";
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

                        default: ExtensionAceptada = ""; break;
                    }

                    if (ExtensionAceptada !== "") {
                        showLoaderOptional();
                        $("#targetDoc").submit();
                    }
                    else {
                        $(errorDocumentSelectedAdd).show();
                    }
                }
                else {
                    $(errorDocumentSelectedAdd).show();
                }
            }
            else {
                $(errorDocumentSelectedAdd).show();
            }
        }
        else {
            $(errorDocumentSelectedAdd).show();
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
    $('[data-id-selector=tabContainer]').html("");
    $('[data-id-selector=tabContainer2]').html("");
    $('[data-id-selector=tabContainer3]').html("");
    switch (Tab) {
        case "AddDocClauses":
            $('#UploadButton').show();
            $('#SearchButton').hide();
            $('#AddDocRelationship').hide();
            if ($("#errorDocumentSelectedAdd").css('display') == 'block') {
                $("#errorDocumentSelectedAdd").css('display', 'none');
            }
            break;
        case "SearchDocClauses":
            $('#SearchButton').show();
            $('#UploadButton').hide();
            $('#AddDocRelationship').hide();
            $('#dmSearchResult').hide();
            $('#dmSearchForm').show();
            $('#dmSearchResult').html("");
            $("#operationNumber").val(operationNumberSearch);
            if ($("#errorDocumentSelectedSearch").css('display') == 'block') {
                $("#errorDocumentSelectedSearch").css('display', 'none');
            }
            break;
    }
}

//Cargar años anteriores
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

$(document).unload(function () {
    hideLoader();
});