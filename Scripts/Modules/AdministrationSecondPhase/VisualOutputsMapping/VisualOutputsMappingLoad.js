var VOMLoad = {};

VOMLoad.inputFileButtonFileSelection = function () {
    $("[name=browseFile]").change(function (event) {
        showLoader();

        setTimeout(function (event) {
            VOMLoad.submitFileToBeLoaded(event);
        }, 100, event);
    });
};

VOMLoad.submitFileToBeLoaded = function (e) {
    var files = e.target.files;

    if (files.length > 0) {
        var fileUpload = e.target.files[0].name;
        var regex = new RegExp("([a-zA-Z0-9\s_\\.\-:])+(" + e.target.accept.replace(/,/g, "|").replace(/ /g, "") + ")$");
        if (!regex.test(fileUpload.toLowerCase())) {
            hideLoader();
            showMessage("Please upload files having extensions: <b>" + e.target.accept + "</b> only.");
            VOMLoad.cleanInputFileCurrentValues();
            return false;
        }

        if (window.FormData !== undefined) {

            var data = new FormData();

            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }

            window.setTimeout(VOMLoad.importVisualOutputsDataFile(VOMLoad.urlForFileLoad, data)
                , 10);

        } else {
            showMessage("This browser doesn't support HTML5 file uploads!");
        }
    } else { hideLoader(); }
};

VOMLoad.importVisualOutputsDataFile = function (importURL, data) {
    showLoader();
    window.setTimeout(function () {
        $.ajax({
            type: "POST",
            url: importURL,
            contentType: false,
            processData: false,
            data: data,
            success: function (data) {
                if (!data) {
                    showMessage("Something went wrong.");
                    hideLoader();
                    return;
                }

                $(".loadTableResultsContainer").html(data["LogsView"]);
                $('#dataToSave').val(JSON.stringify(data["model"]));
                $('.showOperationLogsLink').click(function () {
                    $('#' + this.dataset.target).modal();
                });

                VOMLoad.saveDataVisualOutputs();
                VOMTableOperations.recordsDisplayedSelectionChange();
                VOMTableOperations.singleCheckBoxChange();
                VOMTableOperations.selectUnselectCheckBoxes();
                VOMDownload.downloadLoadLogsButtonClick();
                VOMDownload.downloadValidationLogs();
                VOMLoad.handlePaginationWhenActiveModal();
                VOMLoad.recordsDisplayedSelectionChange();
                bindHandlers();
                hideLoader();
            },
            error: function () {
                hideLoader();
                showMessage("error");
            }
        });
    }, 5);
};

VOMLoad.cleanInputFileCurrentValues = function () {
    $(".inputFileInput").val("");
    $("[name=browseFile]").val("");
};

VOMLoad.downloadSelectedOperationValidationLogs = function () {
    $(".showOperationLogsLink").click(function () {
        var operationNumber = $(this).data("operationnumber");

        showLoader();
        window.setTimeout(function () {
            $.ajax({
                'url': VOMLoad.downloadSelectedOperationValidationLogsURL + "?operationNumber=" + operationNumber,
                'type': 'post',
                'success': function (data) {
                    if (!data) {
                        showMessage("Something went wrong.");
                        return;
                    }
                    else if (!VOMDownload.checkBool(data['IsValid'])) {
                        showMessage(data['Message']);
                        return;
                    }

                    bindHandlers();
                    hideLoader();
                },
                'error': function () {
                    hideLoader();
                    showMessage("error");
                }
            });
        }, 5);
    });
};

VOMLoad.saveDataVisualOutputs = function () {
    $(".saveConfirmChanges").click(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");

        var opSelcted = activeTab.find("[name=selectedOperation]:checked").length > 0;
        if (opSelcted) {
            var saveData = $("#dataToSave").val();
            showLoader();
            window.setTimeout(function () {
                var operationsIds = VOMLoad.loadSelectedOperationsIds();
                var urlSave = VOMLoad.urlForSaveData + "?operationsIds=" + operationsIds;
                $.ajax({
                    'url': urlSave,
                    'type': 'post',
                    'data': JSON.parse(saveData),
                    'success': function (data) {
                        if (!data) {
                            showMessage("Something went wrong.");
                        }
                        else {
                            showMessage(data['Message']);
                        }
                        VOMLoad.clearLoadForm();
                        bindHandlers();
                        hideLoader();
                        return;
                    },
                    'error': function () {
                        hideLoader();
                        showMessage("error");
                    }
                });
            }, 5);
        }
    });

    $(".CancelSaveData").click(function () {
        showLoader();
        VOMLoad.clearLoadForm();
        hideLoader();
    });
};

VOMLoad.loadSelectedOperationsIds = function () {
    var currentTableRows = $("div.tab-pane.active").
                            find(".tabContent").
                            find("table").
                            find("tbody").
                            find("tr");

    return $.map(currentTableRows, function (element) {
        if ($(element).find("[name=selectedOperation]").prop("checked")) {
            return $(element).attr("data-id");
        }
    }).join(", ");
};

VOMLoad.clearLoadForm = function () {
    VOMLoad.cleanInputFileCurrentValues();
    $("#visualOutputLoadFilterResultsTable tbody tr").remove();
    $("#btn-loadId").html("");
    $("#numElementsLoad").text(0);
};

VOMLoad.handlePaginationWhenActiveModal = function () {
    $('[name=viewLoadSummary]').click(function () {
        var currentTable = $('#visualOutputLoadValidationsLog');
        var tableViewLoadSumamry = currentTable.find('tbody').find('tr').length > 0;

        if (tableViewLoadSumamry) {
            VOMTableOperations.addSortAndPagination(currentTable, 10);
        }
    });
};

VOMLoad.recordsDisplayedSelectionChange = function () {
    $('[name=resultPerPageFilterModal]').change(function () {
        var currentTable = $('#visualOutputLoadValidationsLog');

        VOMTableOperations.addSortAndPagination(currentTable, parseInt($(this).GetValue()));
    });
};
