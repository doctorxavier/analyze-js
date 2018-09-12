$(document).ready(function () {   
    var thumbnail = $(".thumbnail");

    if ($(thumbnail).length > 0) {
        
        $(thumbnail).click(function () {
           
            if ($(this).hasClass("active")) {
                $(this).removeClass("active");
                $('#docName').text('');
                $('#idbNumber').text('');
                $('#date').text('');
                $('#docID').text('');
                $('#docDesc').text('');
                $('#docAuthor').text('');
                $('#opNum').text('');
                $('#contractNumber').text('');
                $('#downloadDoc').enabled = false;
                $('#downloadBtn').css('visibility', 'hidden');
                var thumbnailes = $(".thumbnail");
                for (i = 0; i < $(thumbnailes).length; i++) {
                    if (thumbnailes[i].className == "thumbnail active") {
                        $('#downloadBtn').css('visibility', 'hidden');
                        thumbnailes[i].removeClass("active");
                        thumbnailes[i].className = "thumbnail";
                    }
                }
                
            } else {

                var thumbnailes = $(".thumbnail");
                for (i = 0; i < $(thumbnailes).length; i++) {
                    if (thumbnailes[i].className == "thumbnail active") {
                        $('#downloadBtn').css('visibility', 'hidden');
                        thumbnailes[i].className = "thumbnail";
                    }
                }
                
                /* activate thumbnail */
                $(this).addClass("active");
                var docData = $(this).data("docinfo");

                /**
                * Jira ID: CON-470
                * Fix Description: Check if contract number is not null to extract loan number and operation number 
                * Attended by: Jhon Astaiza 
                * Date: 18/07/2014
                * Begin Fix
                **/
                var loan = "";
                var operationNumber = "";
                if (docData.contractNumber != null) {
                    loan = docData.contractNumber.indexOf(";") > -1 ? docData.contractNumber.split(";")[0] : docData.contractNumber;
                    operationNumber = docData.contractNumber.indexOf(";") > -1 ? docData.contractNumber.split(";")[1] : loan;
                }
                /**
                * End Fix
                **/
                $('#downloadBtn').css('visibility','visible');
                var routeToDownload = $('#downloadBtn').data('route') + '?docNum=' + docData.idbNumber;
                $('#downloadBtn').attr('href', routeToDownload);
                $("#IDBDocNumber").val(docData.idbNumber);
                $('#docName').text(docData.docName.replace(/(\r\n|\n|\r)/gm, ""));
                $('#idbNumber').text(docData.idbNumber);
                $('#date').text(docData.date);
                $('#docID').text(docData.docID);
                $('#docDesc').text(docData.docDesc);
                $('#docAuthor').text(docData.docAuthor);
                $('#opNum').text(operationNumber);
                $('#contractNumber').text(loan);
                $('#downloadDoc').enabled = true;
            }
        });
    }

    $('#clearAll').click(function () {
        $.get($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val(),
                subsubParentEntityId:$('#subsubParentEntityId').val(),
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
                var dialog = $(".dinamicModal").data("kendoWindow");
                if (dialog != undefined) {
                    dialog.content(data);
                }
				$('#undefined-1').hide();
				$('#undefined-2').show();
				SelectedTabAction("SearchDocClauses");
            }).fail(function () {
                alert("Http not found!");
            });
    })

    $('#AddDoc').click(function () {
        $.get($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#mainOperationNumber").val(),
                IDBDocNumber: $('#idbNumber').text(),
                DocumentName: $('#docName').text(),
                DocumentID: $('#docID').text(),
                DocumentContent: $('#docDesc').text(),
                entityRelated: $("#entityRelated").val(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val(),
                subsubParentEntityId: $('#subsubParentEntityId').val(),
                DocumentEvaluationTrackingId: $("#DocumentEvaluationTrackingId").val(),
                StageId: $("#Stage").val(),
                TypeOfDocumentId: $("#TypeOfDocument").val(),
                DocumentPermissionsId: $("#DocumentPermissions").val()
            }).done(function (data) {
                var dialog = $(".dinamicModal").data("kendoWindow");
                if (data.indexOf('redirect:') == 0) {
                    data = data.replace('redirect:', '');
                    document.location = data;
                }
                if (dialog != undefined) {
                    dialog.close();
                }
                
            }).fail(function () {
                alert("Document Can not be added");
                var dialog = $(".dinamicModal").data("kendoWindow");
                if (dialog != undefined) {
                    dialog.close();
                }
            });
    });
});