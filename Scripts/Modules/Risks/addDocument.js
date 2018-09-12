function addDocumentRisk(listDocuments, action) {

    var data = {
        table: $("#docTableUIRI001_edit tbody"),
        hidden: "class=\"Hidden\"",
        element: "class=\"Element\"",
        valign: "valign=\"middle\"",
        align: "align=\"center\"",
        td: function (attr, content) {
            return "<td " + attr + ">" + content + "</td>";
        },
        button: function (type, id, cls, onclick, title, dataContent, dataTarget) {
            return "<button type=\"" + type + "\" name=\"" + id + "\" id=\"" + id + "\" class=\"" + cls +
                   "\" onclick=\"" + onclick + "\" title=\"" + title + "\" data-content=\"" + dataContent +
                   "\" data-target=\"" + dataTarget + "\"><span class=\"icon\"></span><span></span></button>";
        },
        inputHidden: function (id, index, value) {
            return "<input type=\"hidden\" id=\"DocumentsRecord_" + index + "__" + id +
                   "\" name=\"DocumentsRecord[" + index + "]." + id + "\" value=\"" + value + "\"/>";
        },
        sizeTable: $("table#docTableUIRI001_edit tbody tr").size(),
        count: 0,
        selected: action === "selected" ? true : false,
        urlReplace: $("#replaceDocumentAsync").val()
    };

    var dt = new Date(),
        date = dt.getDate() + " " + dt.getMonthName() + " " + dt.getFullYear();

    var tabValue = function (author, created) {
        var typeUndefined = "undefined";
        var contextUser = $("input#UserName").val();
        var aut = (author !== "" && typeof (author) != typeUndefined) ? author : contextUser,
            crt = (created !== "" && typeof (created) != typeUndefined) ? created : contextUser;
        return data.selected ? aut : crt;
    };

    for (var i = 0; i < listDocuments.length; i++) {

        data.count = data.sizeTable + i;
        var docLink = listDocuments[i].Link;
        $("<tr data-id=\"\">" +
            data.td(data.hidden,
                    data.inputHidden("DocumentId", data.count, listDocuments[i].DocumentId) +
                    listDocuments[i].DocumentId) +
            data.td(data.hidden,
                    data.inputHidden("IsPublished", data.count, (listDocuments[i].DiscloseDate != null)) +
                    (listDocuments[i].DiscloseDate != null)) +
            data.td(data.hidden,
                    data.inputHidden("Link", data.count, listDocuments[i].Url) +
                    docLink) +
            data.td('class="Element DocumentNumber"',
                data.inputHidden("DocumentNumber", data.count, listDocuments[i].DocumentNumber) +
                "<a href=\"" + docLink + "\" target=\"_blank\" rel=\"noopener\">" +
                listDocuments[i].DocumentNumber + "</a>") +
            data.td('class="Element DocumentName"',
                    data.inputHidden("DocumentName", data.count, listDocuments[i].DocumentName) +
                    listDocuments[i].DocumentName) +
            data.td(data.element,
                    data.inputHidden("Author", data.count, tabValue(listDocuments[i].AuthorId,
            listDocuments[i].CreatedBy)) +
                    tabValue(listDocuments[i].AuthorId, listDocuments[i].CreatedBy)) +
            data.td(data.element,
                    data.inputHidden("CreationDate", data.count, tabValue(listDocuments[i].UsDateParser,
            date)) +
                    tabValue(listDocuments[i].UsDateParser, date)) +
            data.td(data.valign + " " + data.align,
                data.button("button", "btnReplaceDocument", "buttonDmReplace", "doReplaceDocument(event)",
                            "", listDocuments[i].DocumentNumber, data.urlReplace) +
                data.button("button", "btnRemoveDocumentReference", "buttonDmRemoveLink",
                            "window.deleteDocumentRisk('" + listDocuments[i].DocumentNumber + "')", "", listDocuments[i].DocumentNumber,
                            null)) +
        "</td>").appendTo(data.table);
    }

    $("#docTableUIRI001_edit").sortableConfluence();
    $("#docTableUIRI001_edit").selectableConfluence();

}

function deleteDocumentRisk(documentNumber) {

    $("#modalDeleteDocumentRiskEdit").modal("show");
    $("input#deleteDocumentNumber").val(documentNumber);
    $("#actionConfirmed").off();
    $("#actionConfirmed").on("click", function () {
        var dataForm = $("#DetailsOpeRisk").serializeArray();
        var url = $("#deleteDocumentRisk").val();
        var ajaxObject = {
            url: url,
            type: "POST",
            data: dataForm,
            async: false,
            beforeSend: function () {
                $("#modalDeleteDocumentRiskEdit").modal("hide");
                window.showLoaderOptional();
            },
            success: function (data) {
                if (data !== null && data !== undefined) {
                    $("#tableDocumentEdit").html(data);
                }
            },
            error: function (jqXhr) {
                window.errorBar(jqXhr.status + " " + jqXhr.statusText, 60, true);
            },
            complete: function () {
                window.hideLoaderOptional();
            }
        };

        $.ajax(ajaxObject);
    });
}