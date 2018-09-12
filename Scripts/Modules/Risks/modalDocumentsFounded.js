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
            } else {
                /* activate thumbnail */
                $(this).addClass("active");
                var testData = $(this).data("docinfo");
                var loan = testData.contractNumber.split(";")[0];
                var operationNumber = testData.contractNumber.split(";")[1];
                var routeToDownload = $('#downloadBtn').data('route') + '?docNum=' + testData.idbNumber;
                $('#downloadBtn').attr('href', routeToDownload);
                $("#IDBDocNumber").val(testData.idbNumber);
                $('#docName').text(testData.docName);
                $('#idbNumber').text(testData.idbNumber);
                $('#date').text(testData.date);
                $('#docID').text(testData.docID);
                $('#docDesc').text(testData.docDesc);
                $('#docAuthor').text(testData.docAuthor);
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
                dialog.content(data);
                $('#undefined-1').hide();
                $('#undefined-2').show();
                SelectedTabAction("SearchDocClauses");
            }).fail(function () {
                alert("Http not found!");
            });
    })

    $('#AddDoc').click(function () {
        $.post($(this).data("route"),
            {
                OperationId: $("#operationId").val(),
                mainOperationNumber: $("#operationNumber").val(),
                IDBDocNumber: $('#idbNumber').text(),
                DocumentName: $('#docName').text(),
                DocumentID: $('#docID').text(),
                DocumentContent: $('#docDesc').text(),
                entityRegisterId: $("#entityRegisterId").val(),
                parentEntityId: $("#parentEntityId").val(),
                subParentEntityId: $("#subParentEntityId").val()
            }).done(function () {
                var route = $(docAdded).data("route");
                var dialog = $(".dinamicModal").data("kendoWindow");
                dialog.close();
                redirectPage(route);
            }).fail(function () {
                alert("Document Can not be added to Clause!");
            });
    });
});