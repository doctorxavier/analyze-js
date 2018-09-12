function showDMError(message) {
    window.errorBar(message, 30);
}

function closeModalDocument() {
    $("#dmDocumentModal").modal("hide");
    window.hideLoaderOptional();
}

function selectDocumentTab(element) {

    window.showLoaderOptional();
    var selectedTabId = $(element).attr("data-tab-id");

    switch (selectedTabId) {
        case "add":
            loadTabPartial(window.basePath + "DocumentModal/AddIDBDocForm?New=yes&operationNumber=" + modal.operationNumber, selectedTabId);
            break;
        case "search":
            loadTabPartial(window.basePath + "DocumentModal/SearchIDBDocSearchForm?New=yes&operationNumber=" + modal.operationNumber, selectedTabId);
            break;
    }

}

function loadTabPartial(url, selectedTab) {

    $.ajax({
        type: "GET",
        url: url,
        async: true
    })
        .success(function (data) {
            $("#tabPartialContainer").html("").append(data);
            setButtons(selectedTab);
            window.hideLoaderOptional();
            return true;
        })
        .error(function (jqxhr) {

            window.hideLoaderOptional();
            showDMError(jqxhr.statusText);
            return false;
        });
}

function setButtons(tabId) {

    var btnText = $(".modal-footer #performButton").html();

    switch (tabId) {
        case "add":
            btnText = modal.performBtnTextAdd;
            break;
        case "search":
            btnText = modal.performBtnTextSearch;
            break;
    }

    $(".modal-footer #performButton").html(btnText);

}

function addSelectedDocument() {

    window.showLoaderOptional();

    var inputFile = $("input[type=file]")[0];
    var hasNoDocument = $("input[type=file]")[0].files.length === 0;

    if (hasNoDocument) {
        var mensajeNoDocument = $('[data-name="file-empty"] li').text();
        showDMError(mensajeNoDocument);
        window.hideLoaderOptional();
        return false;
    }

    var validExtensions = false;
    var dataForm = new FormData();
    var fileNames = [];
    var extensions = ['.pdf', '.xls', '.xlsx', '.pst', '.ppt', '.pptx', '.mpp', '.pub', '.vsd', '.vss', '.vst', '.vdx', '.vsx', '.vtx', '.doc', '.docx', '.wpd', '.msg'];

    jQuery.each(inputFile.files, function (i, file) {
        var index = file.name.lastIndexOf(".");
        if (index >= 0) {
            var fileName = file.name;
            fileNames.push(fileName);
            var extension = file.name.substr(index).toLowerCase();
            if (jQuery.inArray(extension, extensions) >= 0) {
                validExtensions = true;
                dataForm.append("files", file);
            }
        }
    });

    if (validExtensions) {

        var businessAreaCode = modal.businessAreaCode;

        if (businessAreaCode === "BA_GENERAL_DOCUMENTS") {
            var selectedLib = $('#id-ddLibraryList').GetValue();
            if (selectedLib !== "") {
                businessAreaCode = selectedLib;
            }
        }

        var selectedRowId = $("#selectedRowIdDM");
        var url = window.basePath + "/DocumentModal/IDBDocUploadDocument?operationNumber=" + modal.operationNumber + "&businessAreaCode=" + businessAreaCode + "&selectedRowId=" + selectedRowId.val();

        $.ajax({
            type: 'POST',
            url: url,
            async: true,
            data: dataForm,
            cache: false,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.IsValid) {
                    if (modal.targetFunction !== "") {
                        window[modal.targetFunction](data.DocumentNumbers, "added", fileNames);
                    }
                } else {
                    showDMError(data.ErrorMessage);
                }
                window.hideLoaderOptional();
                closeModalDocument();
            },
            error: function (jqXhr) {
                showDMError(jqXhr.statusText);
            },
            complete: function () {
                window.hideLoaderOptional();
            }
        });

    } else {
        var mensajeErrorExtension = $('[data-name="file-error"] li').text();
        showDMError(mensajeErrorExtension);
        window.hideLoaderOptional();
    }

    $('input[name="file"]').val("");
    $(".inputFileInput").val("");
}

function executeSearch(tabContainer) {

    var form = $("#dmSearchForm");
    var url = window.basePath + "/DocumentModal/SearchIDBDocuments?New=yes";

    $.ajax({
        url: url,
        type: "POST",
        data: form.serializeArray(),
        async: true,
        beforeSend: function () {
            window.showLoaderOptional();
        },
        success: function (data) {
            tabContainer.empty();
            tabContainer.html(data);
            setButtons("add");
            window.bindHandlers();
        },
        error: function (jqXhr) {
            showDMError(jqXhr.statusText);
        },
        complete: function () {
            window.hideLoaderOptional();
        }

    });

}

function executeSendSelected() {

    window.showLoaderOptional();
    var documentList = $("tr.selected");
    var documentsObject = [];

    documentList.each(function (index, element) {
        documentsObject.push($.parseJSON($(element).attr("data-json")));
    });

    if (documentsObject.length > 0) {
        window[modal.targetFunction](documentsObject, "selected");
        window.hideLoaderOptional();
        closeModalDocument();
    } else {
        window.hideLoaderOptional();
        showDMError(modal.errorSelectedDocument);
        return;
    }
}

$("#performButton").on("click", function () {
    var tabContainer = $('[data-id-selector=tabPartialContainer]');
    var isSearchForm = tabContainer.find("[data-id-selector=search-form]").length !== 0;
    var isSearchFormResults = tabContainer.find("[data-id-selector=search-form-results]").length !== 0;
    var isAddDocForm = tabContainer.find("[data-id-selector=add-form]").length !== 0;

    if (isSearchForm) {
        executeSearch(tabContainer);
    }

    if (isSearchFormResults) {
        executeSendSelected();
    }

    if (isAddDocForm) {
        addSelectedDocument();
    }
});

$(document).on("change", "input[type=checkbox][name = chkInputCorrespondence]", function () {
    $(this).closest('[data-id-selector="search-form"]').find('[data-id-selector="inputCorrespondenceContainer"]').toggleClass('hide');
});

$(document).on("change", "input[type=checkbox][name=chkOutputCorrespondence]", function () {
    $(this).closest('[data-id-selector="search-form"]').find('[data-id-selector="outputCorrespondenceContainer"]').toggleClass('hide');
});

$(document).on("click", "button.buttonLink[data-id=clearFilters]", function () {
    $('li[data-tab-id=search]').click();
});

$(".modal").on("hidden.bs.modal", function () {
    $('li[data-tab-id=add]').click();
});

$(".modal-content").attr("style", "margin-bottom:60px;");