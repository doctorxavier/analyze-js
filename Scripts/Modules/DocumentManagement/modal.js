var objDoc = {}

function cleanModalControls() {
    var $origin = $(objDoc.modal);
    var $tableDocument = $origin.find("#tableListDocument").find("tbody");

    $origin.find(objDoc.contentFormAdd).show();
    $origin.find(objDoc.contentFormSearch).hide();
    $origin.find(objDoc.contentResultSearch).hide();
    $origin.find(objDoc.addTab).addClass("active");
    $origin.find(objDoc.SearchTab).removeClass("active");
    $tableDocument.find(".selectRow").html("");
    $tableDocument.find("tr").removeClass("row-selected");
    $tableDocument.find("tr").removeClass("selected");
    objDoc.fileList.empty();
    $origin.find("#fileArea").val("");
    $origin.find(objDoc.contentFormSearch).find("input").val("");
    $origin.find(objDoc.btnSearch).hide();
    $origin.find(objDoc.btnAccept).hide();
    $origin.find(objDoc.btnAdd).show();
    $origin.find(objDoc.loaderSearch).hide();
    $origin.find("#footerDropdown").show();
    $origin.find(".modalDocsTitle").find("#titleReplace").hide();
    $origin.find("#currentDocument").hide();
    resetPositionModal();
    clearDropDownLibraryList();
    enabledButtonClose(true);
    enabledButtonAdd(false);
    disabledSearchTab(false);
    disabledAddFiles(false);
    resetCheckCorrespondence();
    clearCorrespondenceControls("inputCorrespondenceContainer");
    warningAlert();
    objDoc.elements = [];
    objDoc.createTooltip();
}

function resetPositionModal() {
    var $origin = $(objDoc.modal);
    $origin.removeAttr("style");
    $origin.css("top", "0");
    $origin.css("left", "0");
    $origin.css("right", "0");
    $origin.css("width", "");
    $origin.css("height", "");
    $origin.css("overflow", "visible");
}

function enabledButtonClose(bool) {
    var $routeBtnClose = $(objDoc.modal).find(objDoc.btnClose);
    if (bool) {
        $routeBtnClose.removeClass(objDoc.txtBtnWhite);
        $routeBtnClose.addClass(objDoc.txtBtnLink);
        $routeBtnClose.removeClass(objDoc.txtDisabled);
        $routeBtnClose.removeAttr(objDoc.txtDisabled);
    } else {
        $routeBtnClose.removeClass(objDoc.txtBtnLink);
        $routeBtnClose.addClass(objDoc.txtBtnWhite);
        $routeBtnClose.addClass(objDoc.txtDisabled);
        $routeBtnClose.attr(objDoc.txtDisabled, objDoc.txtDisabled);
    }
}

function enabledButtonAdd(bool) {
    var $buttonOrange = "buttonOrange";
    var $routeBtnAdd = $(objDoc.modal).find(objDoc.btnAdd);
    if (bool) {
        $routeBtnAdd.removeClass(objDoc.txtBtnWhite);
        $routeBtnAdd.removeClass(objDoc.txtDisabled);
        $routeBtnAdd.removeAttr(objDoc.txtDisabled);
        $routeBtnAdd.addClass($buttonOrange);
    } else {
        $routeBtnAdd.removeClass($buttonOrange);
        $routeBtnAdd.addClass(objDoc.txtBtnWhite);
        $routeBtnAdd.addClass(objDoc.txtDisabled);
        $routeBtnAdd.attr(objDoc.txtDisabled, objDoc.txtDisabled);
    }
}

function warningAlert(text) {
    var $warning = $(objDoc.modal).find(".warningUpload");
    var $duplicateFile = $warning.find("#duplicateFile");
    var $extensionError = $warning.find("#extensionError");
    switch (text) {
        case "extensionError":
            $warning.show();
            $duplicateFile.hide();
            $extensionError.show();
            break;
        case "duplicateFile":
            $warning.show();
            $extensionError.hide();
            $duplicateFile.show();
            break;
        default:
            $warning.hide();
            $duplicateFile.hide();
            $extensionError.hide();
            break;
    }
}

function disabledAddFiles(bool) {
    var stopEvent = "event.stopPropagation(); event.preventDefault();";
    var $origin = $(objDoc.modal);
    var $formDataDoc = $origin.find("#formDataDocument");
    var $selectFileButton = $origin.find("#selectFileButton");
    var $bgSelectFile = $origin.find(".bgSelectFile");

    if (bool) {
        $formDataDoc.removeAttr("ondrop");
        $formDataDoc.attr("ondrop", stopEvent);
        $selectFileButton.removeClass("buttonBlue");
        $selectFileButton.addClass(objDoc.txtBtnWhite);
        $selectFileButton.addClass("disabled-control");
        $selectFileButton.attr(objDoc.txtDisabled, objDoc.txtDisabled);
        $bgSelectFile.addClass("disabled-control");
    } else {
        $formDataDoc.attr("ondrop", stopEvent + " dodrop(event);");
        $selectFileButton.removeClass(objDoc.txtBtnWhite);
        $selectFileButton.addClass("buttonBlue");
        $selectFileButton.removeClass("disabled-control");
        $selectFileButton.removeAttr(objDoc.txtDisabled);
        $bgSelectFile.removeClass("disabled-control");
    }
}

function disabledSearchTab(bool) {
    var stopEvent = "event.stopPropagation(); event.preventDefault();";
    var $origin = $(objDoc.modal).find(objDoc.SearchTab);

    if (bool) {
        $origin.attr("onclick", stopEvent);
        $origin.addClass("disabled-control");
    } else {
        $origin.removeAttr("onclick");
        $origin.removeClass("disabled-control");
    }
}

function resetCheckCorrespondence() {
    $(objDoc.modal).find('[name="chkInputCorrespondence"]').prop("checked", false);
}

function clearCorrespondenceControls(idSelector) {
    $(objDoc.modal + ' [data-id-selector="' + idSelector + '"]').hide();
    $(objDoc.modal + ' [data-id-selector="' + idSelector + '"] div.dropdown').addClass('placeholder');
    $(objDoc.modal + ' [data-id-selector="' + idSelector + '"] #id-inputYear span.valueText').text('(select option)');
    $(objDoc.modal + ' [data-id-selector="' + idSelector + '"] input[type=text]').val('');
}

function clearDropDownLibraryList() {
    var $routeSelector = $(objDoc.modal).find('[data-id="SelectLibraryContainer"]');
    $routeSelector.find('.dropdown').addClass('placeholder');
    $routeSelector.find('#id-ddLibraryList').find("span.valueText").text('(select option)');
    $routeSelector.find('input[type=text]').val('');
}

function showError(errorMsg) {
    window.errorBar(errorMsg, 60, true);
}

function itemRepeated(value) {
    var items = [];
    var repeated = false;

    objDoc.fileList.find(".nameFile").each(function () {
        items.push($(this).text());
    });

    for (var i = 0; i < items.length; i++) {
        if (items[i] === value) {
            repeated = true;
        }
    }

    return repeated;
}

function warningMessage(message) {
    return $('<div/>', {
        class: 'warningStatusIcon',
        'data-toggle': 'tooltip',
        'data-placement': 'left',
        title: message
    })[0].outerHTML;
}

function addItem(fileName) {
    var $deleteMessage = $(objDoc.modal).find("#deleteFileList").text();
    var $span = $('<span/>', { class: "icon" }).append("")[0].outerHTML;
    var $divBtnTrash = $('<div/>', { class: "buttonTrash" }).append($span)[0].outerHTML;
    var $divBtnRight = $('<div/>', {
            class: 'btnRight',
            'data-toggle': 'tooltip',
            'data-placement': 'top',
            title: $.trim($deleteMessage)
        }).append($divBtnTrash)[0].outerHTML;
    var $divNameFile = $('<div/>', { class: "nameFile" }).append(fileName)[0].outerHTML;

    return $('<li/>').append($divNameFile + $divBtnRight)[0].outerHTML;
}

function fileExtensions() {
    var fileExtensionsIn = objDoc.fileExt.split(",");
    var fileExtensionsOut = [];

    for (var i = 0; i < fileExtensionsIn.length; i++) {
        fileExtensionsOut[i] = $.trim(fileExtensionsIn[i]);
    }
    return fileExtensionsOut;
}

function validExtension(extensionFile) {
    return ($.inArray(extensionFile, fileExtensions()) >= 0);
}

function pendingUploadFiles() {
    if (!(objDoc.fileList.find(".loader-file").size() > 0)) {
        enabledButtonAdd(true);
    }
}

function isMultipleFile(isMultiple) {
    var lengthElements = objDoc.elements.length;
    if (!isMultiple) {
        objDoc.fileList.empty();
        for (var i = 0; i < lengthElements; i++) {
            objDoc.elements.splice(i, lengthElements);
        }
    }
}

function validItem(item, listFiles, isMultiple) {
    if (!itemRepeated(item.name)) {
        isMultipleFile(isMultiple);
        listFiles.append(addItem(item.name));
        objDoc.createTooltip();
        objDoc.elements.push(item);
        pendingUploadFiles();
    } else {
        warningAlert("duplicateFile");
    }
}

function addFiles(files, fileList) {
    var isMultiple = $(objDoc.modal).find("#fileArea").prop("multiple");
    var fileLength = isMultiple ? files.length : 1;

    warningAlert();

    for (var i = 0; i < fileLength; i++) {
        var fileName = files[i].name.lastIndexOf(".");
        var extension = files[i].name.substring(fileName).toLowerCase();

        if (validExtension(extension)) {
            validItem(files[i], fileList, isMultiple);
        } else {
            warningAlert("extensionError");
        }
    }
}

function doSelectFile() {
    var files = $(objDoc.modal).find("#fileArea")[0].files;
    addFiles(files, objDoc.fileList);
}

function dodrop(event) {
    var dt = event.dataTransfer;
    var files = dt.files;
    addFiles(files, objDoc.fileList);
}

function updateRowStatus(index) {
    var $row = objDoc.fileList.find("li").find(".btnRight");
    var $loaderDiv = $('<div/>', { class: 'loader-file' })[0].outerHTML;

    $row.eq(index).removeAttr("data-toggle");
    $row.eq(index).removeAttr("data-placement");
    $row.eq(index).removeAttr("data-original-title");
    $row.eq(index).removeAttr("title");
    $row.eq(index).empty();
    $row.eq(index).append($loaderDiv);
}

function pendingFilesProcess() {
    var pendingUpload = objDoc.modal + " ul#fileList .btnRight .buttonTrash";
    var pendingLoading = objDoc.modal + " ul#fileList .btnRight .loader-file";
    var pending = ($(pendingUpload).size() === 0) && ($(pendingLoading).size() === 0);

    if (pending) {
        $(objDoc.modal + " " + objDoc.btnAdd).hide();
        $(objDoc.modal + " " + objDoc.btnAccept).show();
    } else if ($(pendingUpload).size() > 0) {
        $(objDoc.modal + " " + objDoc.btnAdd).show();
        $(objDoc.modal + " " + objDoc.btnAccept).hide();
        enabledButtonAdd(true);
    }
}

function updateRowStatusResponse(data) {
    var $listElement = objDoc.fileList.find("li");
    var $buttonRight = $listElement.find(".btnRight");
    if (data.IsValid && !reportedParameter(data.RowId)) {
        fileStatus(data.RowId, 'success-file', true);
    } else {
        if (reportedParameter(data.RowId)) {
            $listElement.addClass("warning-file");
            $buttonRight.empty();
            $buttonRight.append(warningMessage(data.ErrorMessage));
        } else {
            $listElement.eq(data.RowId).addClass('warning-file');
            $listElement.find(".btnRight").eq(data.RowId).empty();
            $listElement.find(".btnRight").eq(data.RowId).append(warningMessage(data.ErrorMessage));
        }
        objDoc.createTooltip();
    }
}

function statusModal(target, docNumbers) {
    var bool = $(objDoc.modal).hasClass("in");
    if (bool) {
        $(objDoc.modal).on(objDoc.txtClick, objDoc.btnAccept, function () {
            $(objDoc.modal).modal("hide");
            window[target](docNumbers, "added", null);
        });
    } else {
        window[target](docNumbers, "added", null);
    }
}

function updateResponse(listDoc) {

    var $docsListArray = [];
    var $docsResponseArray = [];

    objDoc.fileList.find("li").each(function () {
        $docsListArray.push($(this).find(".nameFile").text());
    });

    for (var a = 0; a < listDoc.length; a++) {
        $docsResponseArray.push(listDoc[a].DocumentName);
    }

    var $comparisonLists = function (docName) {
        return ($docsResponseArray.indexOf(docName) !== -1);
    };

    for (var e = 0; e < $docsListArray.length; e++) {

        objDoc.fileList.find("li").find(".btnRight").eq(e).empty();

        if ($comparisonLists($docsListArray[e])) {
            fileStatus(e, 'success-file', true);
        } else {
            fileStatus(e, 'warning-file', false);
            objDoc.createTooltip();
        }
    }
}

function fileStatus(index, addClass, status) {
    var $originItem = objDoc.fileList.find("li");
    var $item = $originItem.find(".btnRight");
    var $errorMessage = $(objDoc.modal).find("#errorUploadDoc").val();
    var $result = status ? objDoc.successIcon : warningMessage($errorMessage);

    $originItem.eq(index).addClass(addClass);
    $item.eq(index).removeAttr("data-toggle");
    $item.eq(index).removeAttr("data-placement");
    $item.eq(index).removeAttr("data-original-title");
    $item.eq(index).removeAttr("title");
    $item.eq(index).empty();
    $item.eq(index).append($result);
}

function addFileTable(documentNumbers) {

    var $pendingUpload = objDoc.fileList.find(".buttonTrash");
    var $warning = objDoc.fileList.find("li").find(".warning-file");
    var $success = objDoc.fileList.find("li").find(".success-file");

    if (($warning.size() === 0) && ($success.size() === 0)) {
        updateResponse(documentNumbers);
    }

    if ($pendingUpload.size() > 0) {
        enabledButtonAdd(false);
    } else {
        $(objDoc.modal + " " + objDoc.btnAdd).hide();
        $(objDoc.modal + " " + objDoc.btnAccept).show();
        statusModal(objDoc.targetFunction, documentNumbers);
    }
}

function executeSearch() {
    var url = $(objDoc.modal + " #searchDocumentForm").attr("action");
    var dataForm = $(objDoc.modal + " #searchDocumentForm").serialize();

    var ajaxObject = {
        url: url,
        type: "POST",
        data: dataForm,
        async: true,
        beforeSend: function () {
            $("#layoutLoadingDiv").attr("style", "display:none;");
        },
        success: function (data) {
            if (data.IsValid) {
                fillSearchResultTable(data);
            }
        },
        error: function (jqXhr) {
            showError(jqXhr.status + " " + jqXhr.statusText);
        },
        complete: function () {
            $(objDoc.modal + " " + objDoc.loaderSearch).hide();
            $(objDoc.modal + " " + objDoc.contentResultSearch).show();
            resizeHeadTable();
        }
    };

    $.ajax(ajaxObject);
}

function fillSearchResultTable(jsonData) {
    var tableBody = $(objDoc.modal + " #tblDocumentSearchResult").find("tBody");
    tableBody.html('');
    $('#rowCount').text(jsonData.SearchResult.length);

    jsonData.SearchResult.forEach(function (item) {
            $('<tr>').attr('data-id', '').attr('data-json', JSON.stringify(item)).append(
            $('<td width="5%">').append($('<span>').addClass('dm-selectedRow').html('&nbsp;')),
            $('<td width="20%">').append($('<a>').attr('href', item.Link).attr('target', '_blank').text(item.DocumentNumber)),
            $('<td width="15%">').text(item.SiscorNumber),
            $('<td width="30%">').text(item.DocumentName),
            $('<td width="15%">').text(item.AuthorId),
            $('<td width="15%">').text(item.UsDateParser)
        ).appendTo(tableBody);
    });

    $(objDoc.modal + " #tblDocumentSearchResult").sortableConfluence();
    $(objDoc.modal + " #tblDocumentSearchResult").selectableConfluence();

    hideLoader();
}

function afterSelect(row) {
    var tbody = row.closest('tbody');
    if (objDoc.isMultiple === 'True') {
        row.toggleClass("selected").toggleClass("row-selected");
    } else {
        tbody.find('tr.selected').removeClass("selected").removeClass("row-selected");
        row.addClass("selected").addClass("row-selected");
    }

    enabledButtonAdd(true);
}

function temporalLoader() {
    window.showLoaderOptional();
    setTimeout(function () { window.hideLoaderOptional(); }, 750);
}

function executeSendSelected() {
    var errorMessageSelectingDocument = $(objDoc.modal + " " + objDoc.contentResultSearch + " #errorSelectedDocument").text();
    var selectedDocuments = $("tr.selected");
    var listDocuments = [];

    if (reportedParameter(selectedDocuments.attr("data-json"))) {
        window.hideLoaderOptional();
        showError(errorMessageSelectingDocument);
        $(objDoc.modal).modal("hide");
        return;
    }

    selectedDocuments.each(function (index, element) {
        listDocuments.push($.parseJSON($(element).attr("data-json")));
    });

    temporalLoader();

    setTimeout(function () {
        $(objDoc.modal).modal("hide");
        window[objDoc.targetFunction](listDocuments, "selected", null);
    }, 750);
}

function documentStatusNotify() {
    var documentNotifier = $.connection.notificationsHub;

    documentNotifier.client.notifyDocumentCreated = function (data) {
        updateRowStatusResponse(data);
        pendingFilesProcess();
    }

    $.connection.hub.start({
        transport: ["webSockets", "serverSentEvents", "longPolling"]
    });
}

function executeAdd(urlAction, formElements, array) {
    var ajaxObject = {
        beforeSend: function () {
            window.hideLoader();
        },
        type: "POST",
        url: urlAction,
        async: true,
        data: formElements,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.IsValid && data.Documents.length !== 0) {
                addFileTable(data.Documents);
            }
            else {
                wrongAnswer(data.ErrorMessage);
                showError(data.ErrorMessage);
            }

            window.hideLoaderOptional();
            disabledAddFiles(true);
            disabledSearchTab(false);
        },
        error: function (jqXhr) {
            showError(jqXhr.status + " " + jqXhr.statusText);
            window.hideLoaderOptional();
            wrongAnswer(jqXhr.statusText)
        }
    };

    documentStatusNotify();
    $.ajax(ajaxObject);
    objDoc.elements = [];
}

function wrongAnswer(errorMessage) {

    var $documents = objDoc.fileList.find("li");

    var itemsToNotify = $documents.find('div.btnRight div.loader-file').closest('.btnRight');
    $documents.addClass('warning-file');
    itemsToNotify.each(function (index) {
        var item = this;
        $(item).empty();
        $(item).append(warningMessage(errorMessage));
    });

    objDoc.createTooltip();
    $(objDoc.modal).find(objDoc.btnAdd).hide();
    $(objDoc.modal).find(objDoc.btnAccept).show();
}

function tabAdd() {
    var url = $(objDoc.modal).find("#formDataDocument").attr("action");
    var formData = new FormData();
    var libList = $.trim($(objDoc.modal).find("#ddLibraryList").val());
    var biCod = libList === null || libList === "" ? objDoc.codArea : libList;

    disabledSearchTab(true);
    disabledAddFiles(true);
    enabledButtonAdd(false);
    enabledButtonClose(false);

    formData.append("BusinessAreaCode", biCod);
    formData.append("OperationNumber", objDoc.numOpe);
    formData.append("IsKeyDocument", objDoc.isKeyDocument);
    formData.append("inputFileAccept", objDoc.fileExt);

    for (var i = 0; i < objDoc.elements.length; i++) {
        formData.append("files", objDoc.elements[i]);
        updateRowStatus(i);
    }

    executeAdd(url, formData, objDoc.elements);
}

function tabSearch() {
    executeSendSelected();
}

function startNewModalDocuments(idModal) {
    temporalLoader();
    objDoc.firstModal = "#" + $('[data-id-modal-drag-drop="documentModalDragDrop"]').attr("id");
    objDoc.modal = (!reportedParameter(idModal)) ? idModal : objDoc.firstModal;
    objDoc.contentFormAdd = "#boxDragDropNewModal";
    objDoc.contentFormSearch = "#boxSearchDocsNewModal";
    objDoc.contentResultSearch = "#boxResultsSearchDocsNewModal";
    objDoc.addTab = "#AddDocumentTab";
    objDoc.SearchTab = "#SearchDocumentTab";
    objDoc.btnSearch = "#btn-search";
    objDoc.btnAdd = "#btn-add";
    objDoc.btnClose = "#btn-close";
    objDoc.btnAccept = "#btn-accept";
    objDoc.txtClick = "click";
    objDoc.txtKeyUp = "keyup";
    objDoc.txtDisabled = "disabled";
    objDoc.txtBtnWhite = "buttonWhite";
    objDoc.txtBtnLink = "buttonLink";
    objDoc.loaderSearch = "#loaderSearch";
    objDoc.elements = [];
    objDoc.createTooltip = function () { $('[data-toggle="tooltip"]').tooltip(); };
    objDoc.codArea = $(objDoc.modal).find("#documentModal_BusinessAreaCode").val();
    objDoc.numOpe = $(objDoc.modal).find("#documentModal_OperationNumber").val();
    objDoc.fileExt = $(objDoc.modal).find("#fileExtensions").val();
    objDoc.targetFunction = $(objDoc.modal).find("#documentModal_TargetFunction").val();
    objDoc.isMultiple = $(objDoc.modal).find("#documentModal_MultipleSelection").val();
    objDoc.isKeyDocument = $(objDoc.modal).find("#documentModal_IsKeyDocument").val();
    objDoc.successIcon = $('<div/>', { class: 'successStatusIcon' })[0].outerHTML;
    objDoc.fileList = $(objDoc.modal).find("#fileList");
    buttonLibraryList(false);
    resetLibraryList();
    modalBindHandlers();
    cleanModalControls();
    setTimeout(function () { $(objDoc.modal).modal("show"); }, 750);
    $(objDoc.modal).draggable({ cursor: 'move', handle: ".modal-content" });
    if ($(objDoc.modal).attr("data-type") === "replace") {
        replaceMode(true);
    }
}

function replaceMode(isActive) {
    var $origin = $(objDoc.modal);
    var $fileArea = $origin.find('#fileArea');
    var $multipleSelection = $origin.find('#documentModal_MultipleSelection');
    var $avoidEvents = "event.stopPropagation(); event.preventDefault();";
    var $libraryList = $origin.find("#ddLibraryList");
    var $actualName = $origin.find("#currentDocument").find(".documentName").text();
    var $actualExtension = $actualName.substr($actualName.lastIndexOf('.')).toLowerCase();
    var $defaultExtensions = $origin.find("#fileExtensions").val();

    var $isNotMultiple = function (active) {
        if (active) {
            $fileArea.removeAttr('multiple');
            $multipleSelection.attr('data-val', 'false');
            $multipleSelection.val('false');
        } else {
            $fileArea.attr('multiple', 'multiple');
            $multipleSelection.attr('data-val', 'true');
            $multipleSelection.val('True');
        }
    }

    var $isNotInformation = function (active) {
        if (active) {
            $origin.find("#formDataDocument").attr("style", "width:98.2%;margin-left:15px;");
            $origin.find(".icoInformation").hide();
        } else {
            $origin.find("#formDataDocument").removeAttr("style");
            $origin.find(".icoInformation").show();
        }
    }

    if (isActive) {
        $origin.find("#AddDocumentTab").attr("onclick", $avoidEvents);
        $origin.find("#SearchDocumentTab").hide();
        $origin.find("#footerDropdown").hide();
        $origin.find("#documentModal_TargetFunction").val('sendReplaceDocument');
        $libraryList.removeAttr("data-parsley-required");
        $libraryList.removeAttr("data-force-parsley-validation");
        $libraryList.removeAttr("data-validation-id");
        $(objDoc.modal).find(".validation-element").hide();
        buttonLibraryList(false);
        $isNotMultiple(true);
        $isNotInformation(true);
        $origin.find("#fileArea").attr("accept", $actualExtension);
        $origin.find(".modalDocsTitle").find("#titleDocument").hide();
        $origin.find(".modalDocsTitle").find("#titleReplace").show();
        $origin.find("#currentDocument").show();
    } else {
        $origin.find("#AddDocumentTab").removeAttr("onclick");
        $origin.find("#SearchDocumentTab").show();
        $origin.find("#footerDropdown").show();
        $origin.find("#documentModal_TargetFunction").val('');
        $libraryList.removeAttr("data-parsley-required");
        $libraryList.removeAttr("data-force-parsley-validation");
        $libraryList.removeAttr("data-validation-id");
        $(objDoc.modal).find(".validation-element").show();
        buttonLibraryList(true);
        $isNotMultiple(false);
        $isNotInformation(false);
        $origin.find("#fileArea").attr("accept", $defaultExtensions);
        $origin.find(".modalDocsTitle").find("#titleReplace").hide();
        $origin.find(".modalDocsTitle").find("#titleDocument").show();
        $origin.find("#currentDocument").hide();
    }
}

function resetLibraryList() {
    $(objDoc.modal + " #libraryList ul.dropdown-menu li a").removeAttr("dd-selected");
    $(objDoc.modal + " #libraryList ul.dropdown-menu li").eq(0).find("a").attr("dd-selected", "");
    $(objDoc.modal + " #id-ddLibraryList").removeClass("validation-fail");
    $(objDoc.modal + " input#ddLibraryList").val("");
    $(objDoc.modal + " #id-ddLibraryList span.valueText").text("(select option)");
}

function selectSearch() {
    $(objDoc.modal + " " + objDoc.contentFormSearch).show();
    $(objDoc.modal + " " + objDoc.contentFormAdd).hide();
    $(objDoc.modal + " " + objDoc.contentResultSearch).hide();
    $(objDoc.modal + " " + objDoc.btnSearch).show();
    $(objDoc.modal + " " + objDoc.btnAdd).hide();
    $(objDoc.modal + " #footerDropdown").hide();
    $(objDoc.modal + " " + objDoc.SearchTab).addClass("active");
    $(objDoc.modal + " " + objDoc.addTab).removeClass("active");
    objDoc.fileList.empty();
    $(objDoc.modal + " #searchDocumentForm input[type='text']").val("");
    $(objDoc.modal + " #operationNumber").val(objDoc.numOpe);
    $(objDoc.modal + " " + objDoc.btnAccept).hide();
    $(objDoc.modal + " " + objDoc.btnClose).removeClass(objDoc.txtBtnWhite);
    $(objDoc.modal + " " + objDoc.btnClose).addClass(objDoc.txtBtnLink);
    $(objDoc.modal + " " + objDoc.btnClose).removeClass(objDoc.txtDisabled);
    $(objDoc.modal + " " + objDoc.btnClose).removeAttr(objDoc.txtDisabled);
    resetCheckCorrespondence();
    clearCorrespondenceControls("inputCorrespondenceContainer");
    disabledSearchTab(false);
    disabledAddFiles(false);
    $(objDoc.modal + " #tblDocumentSearchResult").find("tBody").empty();
    $(objDoc.modal + " #rowCount").text(0);
}

function executeAddFile() {
    $(objDoc.modal + " " + objDoc.contentFormAdd).show();
    $(objDoc.modal + " " + objDoc.contentFormSearch).hide();
    $(objDoc.modal + " " + objDoc.contentResultSearch).hide();
    $(objDoc.modal + " " + objDoc.btnSearch).hide();
    $(objDoc.modal + " " + objDoc.btnAdd).show();
    $(objDoc.modal + " #footerDropdown").show();
    clearDropDownLibraryList();
    $(objDoc.modal + " " + objDoc.addTab).addClass("active");
    $(objDoc.modal + " " + objDoc.SearchTab).removeClass("active");
    $(objDoc.modal + " " + objDoc.loaderSearch).hide();
    warningAlert();
    buttonLibraryList(false);
    resetLibraryList();
    $(objDoc.modal + " .documentModalFooter ul.dropdown-menu").attr("style", "min-width: 200px;top: -124px;");
    enabledButtonAdd(false);
}

function executeCorrespondence(chkCorrespondence, inputCorrespondence) {
    if ($(objDoc.modal + ' input[name="' + chkCorrespondence + '"]').prop("checked")) {
        $(objDoc.modal + ' [data-id-selector="' + inputCorrespondence + '"]').show();
        $(objDoc.modal + " .dropdown-menu").css("top", "-260px");
    } else {
        clearCorrespondenceControls(inputCorrespondence);
    }
}

function executeSelectFileButton() {
    var change = "change";

    $(objDoc.modal + " input#fileArea").val("");
    $(objDoc.modal + " input#fileArea").off(objDoc.txtClick);
    $(objDoc.modal + " input#fileArea").trigger(objDoc.txtClick);

    if (navigator.userAgent.indexOf("MSIE ") > -1) {
        var inputElement = document.getElementById("fileArea");
        if (inputElement.addEventListener) {
            inputElement.addEventListener(change, doSelectFile, false);
        } else if (inputElement.attachEvent) {
            inputElement.attachEvent(change, doSelectFile);
        }
    }
}

function executeBtnSearch() {
    $(objDoc.modal + " " + objDoc.contentFormAdd).hide();
    $(objDoc.modal + " " + objDoc.contentFormSearch).hide();
    $(objDoc.modal + " " + objDoc.btnSearch).hide();
    $(objDoc.modal + " " + objDoc.btnAdd).show();
    $(objDoc.modal + " " + objDoc.contentResultSearch + " .contentDocuments").empty();
    $(objDoc.modal + " " + objDoc.loaderSearch).show();
    executeSearch();
}

function buttonLibraryList(isActive) {
    var idLibList = objDoc.modal + " #id-ddLibraryList";
    var disabled = "disabled";

    if (isActive) {
        $(idLibList).addClass(disabled);
        $(idLibList).attr(disabled, disabled);
    } else {
        $(idLibList).removeClass(disabled);
        $(idLibList).removeAttr(disabled);
    }
}

function resizeHeadTable() {
    var $origin = $(objDoc.modal).find("#tblDocumentSearchResult");
    var $thead = $origin.find("thead");
    var $tbody = $origin.find("tbody").find("tr");
    var $highCell = $tbody.height();
    var $numberCells = $tbody.size();
    var $highTotal = ($highCell * $numberCells);

    if ($highTotal > 190) {
        $thead.attr("style", "padding-right:15px;");
        $origin.find("tbody").removeAttr("style");
    } else {
        $thead.removeAttr("style");
        $origin.find("tbody").attr("style", "overflow-y:hidden;");
    }

    if (navigator.userAgent.indexOf("MSIE ") > -1 || navigator.userAgent.indexOf("Trident") > -1) {
        $tbody.removeAttr("style").attr("style", "display:inline-table;");
    }
    else {
        $tbody.removeAttr("style").attr("style", "display:flex;");
    }
}

function updateAcceptExtension(extensionList) {
    var originalToolip = $(objDoc.modal).find(".icoInformation").attr('data-original-title');
    if (!reportedParameter(originalToolip)) {
        var newTooltipText = originalToolip.substring(0, originalToolip.lastIndexOf(':') + 1) + extensionList;
        $(objDoc.modal + ' div.icoInformation').attr('data-original-title', newTooltipText);
    }

    $(objDoc.modal + " #formDataDocument #fileArea").prop('accept', extensionList);
    $(objDoc.modal + ' #formDataDocument #fileExtensions').val(extensionList);
    objDoc.fileExt = extensionList;
}

function reportedParameter(parameter) {
    var types = /^(null|undefined)$/.test(parameter);
    var empty = $.trim(parameter) === "";
    return (types || empty);
}

function modalBindHandlers() {

    $(objDoc.modal).off(objDoc.txtClick, objDoc.btnClose + ", .closeModal");
    $(objDoc.modal).on(objDoc.txtClick, objDoc.btnClose + ", .closeModal", function () {
        $(objDoc.modal).removeAttr("data-type");
        replaceMode(false);
        $(objDoc.modal).modal("hide");
        resetPositionModal();
        cleanModalControls();
    });

    $(objDoc.modal).off(objDoc.txtClick, "#selectFileButton");
    $(objDoc.modal).on(objDoc.txtClick, "#selectFileButton", function () {
        executeSelectFileButton();
    });

    $(objDoc.modal).off("change", "input#fileArea");
    $(objDoc.modal).on("change", "input#fileArea", function () {
        doSelectFile();
    });

    $(objDoc.modal).off(objDoc.txtClick, ".buttonTrash");
    $(objDoc.modal).on(objDoc.txtClick, ".buttonTrash", function (e) {
        var item = $(e.target).closest("li");
        var file = item.find(".nameFile").text();
        var index = 0;

        item.remove();

        for (var i = 0; i < objDoc.elements.length; i++) {
            if (objDoc.elements[i].name === file) {
                objDoc.elements.splice(i, 1);
            }
        }

        objDoc.fileList.find("li").each(function () {
            if (!$(this).hasClass("warning-file")) { index++; }
        });

        if (index === 0) { enabledButtonAdd(false); }
    });

    $(objDoc.modal).off(objDoc.txtClick, ".warningUpload .errorStatusIcon");
    $(objDoc.modal).on(objDoc.txtClick, ".warningUpload .errorStatusIcon", function () {
        warningAlert();
    });

    $(objDoc.modal + " #filedrag").bind("dragover", function () {
        $(this).addClass("filedragHover");
    });

    $(objDoc.modal + " #filedrag").bind("dragleave", function () {
        $(this).removeClass("filedragHover");
    });

    $(objDoc.modal + " #filedrag").bind("drop", function () {
        $(this).removeClass("filedragHover");
    });

    $(objDoc.modal).off(objDoc.txtClick, objDoc.addTab);
    $(objDoc.modal).on(objDoc.txtClick, objDoc.addTab, function () {
        executeAddFile();
    });

    $(objDoc.modal).off(objDoc.txtClick, objDoc.SearchTab);
    $(objDoc.modal).on(objDoc.txtClick, objDoc.SearchTab, function () {
        selectSearch();
    });

    $(objDoc.modal).off(objDoc.txtClick, objDoc.btnSearch);
    $(objDoc.modal).on(objDoc.txtClick, objDoc.btnSearch, function () {
        executeBtnSearch();
    });

    $(objDoc.modal).off(objDoc.txtClick, 'tr[data-id-selector="selectableDocument"]');
    $(objDoc.modal).on(objDoc.txtClick, 'tr[data-id-selector="selectableDocument"]', function () {
            enabledButtonAdd(true);
    });

    $(objDoc.modal).off(objDoc.txtClick, objDoc.btnAdd);
    $(objDoc.modal).on(objDoc.txtClick, objDoc.btnAdd, function () {

        if ($(objDoc.modal).attr("data-type") !== "replace") {

            if ($(objDoc.addTab).hasClass("active")) {
                var ddList = $(objDoc.modal).find("#footerDropdown");
                Validation.Init(ddList);
                if (Validation.Container(ddList)) {
                    tabAdd();
                    buttonLibraryList(true);
                } else {
                    $(objDoc.modal).find(".validation-element").hide();
                    buttonLibraryList(false);
                }
            } else if ($(objDoc.SearchTab).hasClass("active")) {
                tabSearch();
            }

        } else {
            $(objDoc.modal).find(".validation-element").hide();
            buttonLibraryList(false);
            var $origin = $(objDoc.modal).find("#currentDocument");
            var $documentNumber = $origin.find(".documentNumber").text();
            var $actionService = $origin.find("#urlReplace").val();
            var $documentName = $origin.find(".documentName").text();

            disabledAddFiles(true);
            enabledButtonAdd(false);
            enabledButtonClose(false);

            window.sendReplaceDocument($documentNumber, $actionService, null, $documentName);
        }
    });

    $(objDoc.modal).off(objDoc.txtClick, objDoc.btnAccept);
    $(objDoc.modal).on(objDoc.txtClick, objDoc.btnAccept, function () {
        $(objDoc.modal).modal("hide");
        $(objDoc.modal).removeAttr("data-type");
        replaceMode(false);
    });

    $(objDoc.modal).off(objDoc.txtClick, 'input[name="chkInputCorrespondence"]');
    $(objDoc.modal).on(objDoc.txtClick, 'input[name="chkInputCorrespondence"]', function () {
        executeCorrespondence("chkInputCorrespondence", "inputCorrespondenceContainer");
    });

    $(objDoc.modal).off(objDoc.txtKeyUp, objDoc.contentFormSearch);
    $(objDoc.modal).on(objDoc.txtKeyUp, objDoc.contentFormSearch, function (e) {
        if ($(objDoc.SearchTab).hasClass('active')) {
            if (e.which === 13) {
                $(objDoc.btnSearch).click();
            }
        }
    });

    $(objDoc.modal).off(objDoc.txtClick, "table#tblDocumentSearchResult tbody tr");
    $(objDoc.modal).on(objDoc.txtClick, "table#tblDocumentSearchResult tbody tr", function () {
        var selectedCount = $("table#tblDocumentSearchResult tbody tr.selected").size();
        $("span#quantityDocs").text(selectedCount);
        if (selectedCount === 0) { enabledButtonAdd(false); }
    });
}