var VOMDownload = {};

VOMDownload.data = {};
VOMDownload.data['FilterInformationViewModel'] = {};
VOMDownload.departmentSectorDefaultValue = "Depart";
VOMDownload.departmentSectorDefaultValueLength = 3;

VOMDownload.departmentSectorChange = function () {
    $("[name=SearchVisualOutputsDepartmentSector]").change(function () {
        var departmentSectorId = $(this).GetValue();

        var divisionsDropDownList =
            $('ul[aria-labelledby=id-SearchVisualOutputsMappingDivision]');
        var divisionsDropDownListLiElements = divisionsDropDownList.find('li');

        divisionsDropDownListLiElements.
            find('a[dd-value]').
            removeClass('hide');

        if (departmentSectorId !== null && departmentSectorId !== "")
        {
            divisionsDropDownListLiElements.
            find('a[dd-value]').
            addClass('hide');

            $('ul[aria-labelledby=id-SearchVisualOutputsMappingDivision] li a[dd-group-name='
                + departmentSectorId + ']')
                .removeClass('hide');

            $('ul[aria-labelledby=id-SearchVisualOutputsMappingDivision] li a[dd-value=""]')
                .removeClass('hide');
        }

        divisionsDropDownList.FirstorDefault();
    });
};

VOMDownload.defaultButton = function () {
    $("[name=visuaslOutputsMappingContent]").keypress(function (e) {
        if (e.keyCode === '13' || e.keyCode === 13) {
            $(this).find("[name=downloadSectionApplyFilter]").click();
        }
    });
};

VOMDownload.applyFilter = function () {
    $("[name=downloadSectionApplyFilter]").click(function () {
        VOMDownload.loadFilterDataModel();
        showLoader();
        window.setTimeout(function () {
            $.ajax({
                'url': VOMDownload.applyFilterURL,
                'type': 'post',
                'data': VOMDownload.data['FilterInformationViewModel'],
                'success': function (data) {
                    if (!data) {
                        showMessage("Something went wrong.");
                        return;
                    }
                    else if (!VOMDownload.checkBool(data['IsValid'])) {
                        showMessage(data['Message']);
                        return;
                    }

                    $(".downloadTableResultsContainer").html(data["TabView"]);

                    bindHandlers();
                    VOMDownload.loadOnDocumentReadyFunction(true);
                    VOMDownload.insertRowWithEmptyResultsMessage();

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

VOMDownload.loadFilterDataModel = function () {
    VOMDownload.data['FilterInformationViewModel']['OperationNumber'] =
        VOMDownload.loadOperationNumberFilterValue();

    VOMDownload.data['FilterInformationViewModel']['OperationName'] =
        VOMDownload.loadOperationNameFilterValue();

    VOMDownload.data['FilterInformationViewModel']['ApprovalNumber'] =
        VOMDownload.loadApprovalNumberFilterValue();

    VOMDownload.data['FilterInformationViewModel']['CountryCode'] =
        $("[name=SearchVisualOutputsMappingCountry]").val();

    VOMDownload.data['FilterInformationViewModel']['DepartmentSector'] =
        VOMDownload.loadDepartmentSectorValue();

    VOMDownload.data['FilterInformationViewModel']['Division'] =
        $("[name=SearchVisualOutputsMappingDivision]").val();

    VOMDownload.data['FilterInformationViewModel']['ApprovalYearsList'] =
        VOMDownload.loadYearsFilterValues("SearchVisualOutputsMappingApprovalYear");

    VOMDownload.data['FilterInformationViewModel']['ElegibilityYearsList'] =
        VOMDownload.loadYearsFilterValues("SearchVisualOutputsMappingElegibilityYear");

    VOMDownload.data['FilterInformationViewModel']['OperationType'] =
        $("[name=SearchVisualOutputsMappingOperationType]").val();

    VOMDownload.data['FilterInformationViewModel']['Active'] =
        $("[name=showActiveFilter]").prop("checked");

    VOMDownload.data['FilterInformationViewModel']['ShowInactive'] =
        $("[name=showInactiveFilter]").prop("checked");

    VOMDownload.data['FilterInformationViewModel']['ShowExited'] =
        $("[name=showExitedFilter]").prop("checked");

    VOMDownload.data['FilterInformationViewModel']['MyOperations'] =
        $("[name=showMyOperationsFilter]").prop("checked");
};

VOMDownload.checkBool = function (value) {
    if (value === true || String(value).toLowerCase() === 'true') {
        return true;
    }
    return false;
};

VOMDownload.loadOperationNumberFilterValue = function () {
    var operationNumber = "";
    var isOperationNumber = $("[name=txtSearch]").val().match(/(df\-)?\w{2}\-?\w\d{4}/);

    if (isOperationNumber) {
        operationNumber = $("[name=txtSearch]").val();
    }

    return operationNumber;
};

VOMDownload.loadApprovalNumberFilterValue = function () {
    var approvalNumber = "";
    var isApprovalNumber = $("[name=txtSearch]").val().match(/\w{0,4}\/[a-bA-Z]{2}\-\w{2,4}/);

    if (isApprovalNumber) {
        approvalNumber = $("[name=txtSearch]").val().toUpperCase();
    }

    return approvalNumber;
};

VOMDownload.loadOperationNameFilterValue = function () {
    var operationNumber = VOMDownload.loadOperationNumberFilterValue();
    var approvalNumber = VOMDownload.loadApprovalNumberFilterValue();
    var operationName = "";

    if (operationNumber === "" && approvalNumber === "") {
        operationName = $("[name=txtSearch]").val().toUpperCase();
    }

    return operationName;
};

VOMDownload.loadYearsFilterValues = function (elementName) {
    var element = $("[name=" + elementName + "]");
    var yearsToBeReturned = "";

    if (element.val() !== null) {
        yearsToBeReturned = element.val().join(",");
    }

    return yearsToBeReturned;
};

VOMDownload.loadOnDocumentReadyFunction = function (applyFilter) {

    applyFilter = typeof applyFilter !== 'undefined' ? applyFilter : false;

    VOMDownload.departmentSectorChange();
    VOMTableOperations.addSortAndPagination(
        $("#visualOutputDownloadFilterResultsTable"), 10);
    VOMTableOperations.handlePaginationWhenActiveTabChanges();
    VOMTableOperations.recordsDisplayedSelectionChange();
    VOMTableOperations.singleCheckBoxChange();
    VOMTableOperations.selectUnselectCheckBoxes();
    if (!applyFilter) {
        VOMDownload.defaultButton();
        VOMDownload.applyFilter();
    }

    VOMDownload.clearFilter();
    VOMDownload.downloadExcelFile();
    VOMDownload.showTooltipNotSelectedOperation();
    VOMDownload.downloadLogsButtonClick();
    VOMDownload.downloadValidationLogs();
    VOMDownload.downloadMapsLayers();
    VOMDownload.showPopUpDownloadMapsLayers();
    VOMDownload.cancelDownloadValidationLogs();
    VOMDownload.selectAllVO();
    VOMDownload.singleCheckBoxVO();
    VOMDownload.showTooltipNotSelectedOperationMapsLayers();
};

VOMDownload.clearFilter = function () {
    $("[name=clearAppliedFilter]").click(function () {
        $("[name=txtSearch]").val("");

        var dropDowns =
            ["SearchVisualOutputsMappingCountry",
             "SearchVisualOutputsDepartmentSector",
             "SearchVisualOutputsMappingDivision",
             "SearchVisualOutputsMappingOperationType"];
        VOMDownload.clearDropdowns(dropDowns);

        var multiDropdowns = ["SearchVisualOutputsMappingApprovalYear",
                              "SearchVisualOutputsMappingElegibilityYear"];
        VOMDownload.clearMultiDropdowns(multiDropdowns);

        var checkBoxes = ["showActiveFilter", "showInactiveFilter", "showExitedFilter"];
        VOMDownload.clearCheckBoxes(checkBoxes);
        $("[name=showActiveFilter]").prop("checked", true);
        $("[name=showInactiveFilter]").prop("checked", false);
        $("[name=showExitedFilter]").prop("checked", false);
    });
};

VOMDownload.clearDropdowns = function (selectorList) {
    $.each(selectorList, function () {
        $("[name=" + this + " ]").FirstorDefault();
    });
};

VOMDownload.clearCheckBoxes = function (selectorList) {
    $.each(selectorList, function () {
        $("[name=" + this + " ]").prop("checked", false);
    });
};

VOMDownload.clearMultiDropdowns = function (selectorList) {
    $.each(selectorList, function () {
        $("[name=" + this + " ]").Clean();
    });
};

VOMDownload.showFilterInput = function () {
    $('.returnToFilterSection').click(function () {
        var button = $(this);
        if (button.data('element')) {
            $(button.data('element')).toggle();
        }
    });
};

VOMDownload.insertRowWithEmptyResultsMessage = function () {
    if ($(".downloadTableResultsContainer").find("table").find("tbody").find("tr").length === 0) {
        $(".downloadTableResultsContainer")
            .find("table")
            .find("tbody")
            .append("<tr data-id=''><td>" + VOMDownload.emptyResultsMessage + "</td></tr>");
    }
};

VOMDownload.downloadExcelFile = function () {
    $("[name=downloadExcelFile]").click(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");

        var opSelcted = activeTab.find("[name=selectedOperation]:checked").length > 0;

        if (opSelcted) {
            Confirm.ShowWarning(VOMDownload.popUpBody, VOMDownload.popUpTitle,
                VOMDownload.continueText).done(function (answer) {
                    if (answer) {
                        setTimeout(function () {
                            var operationsIds = VOMTableOperations.loadSelectedOperationsIds();

                            window.open(VOMDownload.downloadExcel +
                                            "?operationsIds=" +
                                            operationsIds, "_blank");
                        }, 10);
                    }
                });
        }
    });
};

VOMDownload.showTooltipNotSelectedOperation = function () {
    $("[name=downloadExcelFile]").mouseover(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");
        var opSelcted = activeTab.find('[name=selectedOperation]:checked').length > 0;

        if (!opSelcted) {
            VOMDownload.showCheckedItemsValidationTooltip();
        } else {
            VOMDownload.hideCheckedItemsValidationTooltip();
        }
    });
};

VOMDownload.showCheckedItemsValidationTooltip = function () {
    $("[name=downloadExcelFile]").tooltip("enable");
    $("[name=downloadExcelFile]").attr("title", VOMDownload.notSelectedOperation);
};

VOMDownload.hideCheckedItemsValidationTooltip = function () {
    $("[name=downloadExcelFile]").tooltip("enable");
    $("[name=downloadExcelFile]").attr("title", "");
};

VOMDownload.loadDepartmentSectorValue = function () {
    var departmentSectorValue = $("[name=SearchVisualOutputsDepartmentSector]").GetText();

    if (departmentSectorValue.length > VOMDownload.departmentSectorDefaultValueLength
        && departmentSectorValue.indexOf(VOMDownload.departmentSectorDefaultValue) >= 0) {
        departmentSectorValue = "";
    }

    return departmentSectorValue;
};

VOMDownload.downloadLogsButtonClick = function () {
    $('[name=downloadChangesLogButton]').click(function () {
        var currentTab = $("div.tab-pane.active").
                            find(".tabContent");
        var opSelcted = currentTab.find('[name=selectedOperation]:checked').length > 0;

        if (opSelcted) {
            $('#downloadLogModal').modal();
        }
    });
};

VOMDownload.downloadLoadLogsButtonClick = function () {
    $('[name=downloadLoadChangesLogButton]').click(function () {
        var currentTab = $("div.tab-pane.active").
                            find(".tabContent");
        var opSelcted = currentTab.find('[name=selectedOperation]:checked').length > 0;

        if (opSelcted) {
            var operationsIds = VOMLoad.loadSelectedOperationsIds();
            var saveData = $("#dataToSave").val();
            var url = VOMDownload.downloadLoadLogsURL + "?operationsIds=" + operationsIds;
            var form = $('<form method="POST" action="' + url + '">');
            form.append($("<input></input>")
                .attr('type', 'hidden')
                .attr('name', 'vOExcelViewModel')
                .attr('value', saveData)); //JSON.stringify(saveData)
            //form.append($('<input type="hidden" name="vOExcelViewModel" value='
            //    + saveData + '>'));

            $('body').append(form);
            form.submit();
        }
    });
};

VOMDownload.downloadValidationLogs = function () {
    $("[name=downloadValidationLogs]").click(function () {
        var currentTab = $("div.tab-pane.active").find(".tabContent");
        var operationNumbers = VOMTableOperations.loadSelectedOperationsNumbers();
        var fromDownloadDate = currentTab.find("[name=fromDownloadDate]").val();
        var toDownloadDate = currentTab.find("[name=toDownloadDate]").val();

        var validationDateNull = currentTab.find("[name=validationDateNull]");
        var toDateMajorFromDate = currentTab.find("[name=toDateMajorFromDate]");
        var toDateMajorCurrentDate = currentTab.find("[name=toDateMajorCurrentDate]");

        VOMDownload.validationDateMessageHide();

        if (fromDownloadDate === "" || toDownloadDate === "") {
            validationDateNull.show();
            return;
        }

        var fromDate = new Date(fromDownloadDate);
        var toDate = new Date(toDownloadDate);

        if (fromDate > toDate) {
            toDateMajorFromDate.show();
            return;
        }

        if (toDate > Date.now()) {
            toDateMajorCurrentDate.show();
            return;
        }

        window.open(
            VOMDownload.downloadLogsExcelUrl +
            "?operationNumbers=" + operationNumbers +
            "&fromDownloadDate=" + fromDownloadDate +
            "&toDownloadDate=" + toDownloadDate,
            "_blank");

        $('#downloadLogModal').modal('hide');
    });
};

VOMDownload.showPopUpDownloadMapsLayers = function () {
    $("[name=InLayersButton]").click(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");

        var opSelcted = activeTab.find("[name=selectedOperation]:checked").length > 0;

        if (opSelcted) {
            $('#exportMapLayers').modal();
        }
    });
};

VOMDownload.showTooltipNotSelectedOperationMapsLayers = function () {
    $("[name=InLayersButton]").mouseover(function () {
        var activeTab = $("div.tab-pane.active").find(".tabContent");
        var opSelcted = activeTab.find('[name=selectedOperation]:checked').length > 0;

        if (!opSelcted) {
            $("[name=InLayersButton]").tooltip("enable");
            $("[name=InLayersButton]").attr("title", VOMDownload.notSelectedOperation);
        } else {
            $("[name=InLayersButton]").tooltip("enable");
            $("[name=InLayersButton]").attr("title", "");
        }
    });
};

VOMDownload.cancelDownloadValidationLogs = function () {
    $("[name=cancelDownloadValidationLogs]").click(function () {
        var currentTab = $("div.tab-pane.active").find(".tabContent");
        var date = new Date();
        var dateNow = date.getDate() + " "
            + VOMDownload.arrayMonths[date.getMonth()] + " "
            + date.getFullYear();

        currentTab.find("[name=fromDownloadDate]").val(dateNow);
        currentTab.find("[name=toDownloadDate]").val(dateNow);
        VOMDownload.validationDateMessageHide();
    });
};

VOMDownload.validationDateMessageHide = function () {
    var currentTab = $("div.tab-pane.active").find(".tabContent");
    var validationDateNull = currentTab.find("[name=validationDateNull]");
    var toDateMajorFromDate = currentTab.find("[name=toDateMajorFromDate]");
    var toDateMajorCurrentDate = currentTab.find("[name=toDateMajorCurrentDate]");

    validationDateNull.hide();
    toDateMajorFromDate.hide();
    toDateMajorCurrentDate.hide();
};

VOMDownload.downloadMapsLayers = function () {
    $("[name=exportMapLayers]").click(function () {

        var selectedVO = $('[name=selectedVO]:checked').length > 0;
        if (selectedVO) {
            var filterVo = "";
            if ($('#VOExternalMap').is(":checked")) {
                filterVo = filterVo + "E";
            }
            if ($('#VOInternalMap').is(":checked")) {
                filterVo = filterVo + "I";
            }
            if ($('#VODraftStatus').is(":checked")) {
                filterVo = filterVo + "D";
            }
            if ($('#AllVO').is(":checked")) {
                filterVo = "A";
            }

            if ($('#VOLayer').is(":checked")) {
                setTimeout(function () {
                    var operationsIds = VOMTableOperations.loadSelectedOperationsIds();

                    window.open(VOMDownload.downloadMapLayers +
                                    "?operationsIds=" + operationsIds
                                    + "&layer=VOLayer&filterVO=" + filterVo, "_blank");
                }, 10);
            }

            if ($('#ServicesLayer').is(":checked")) {
                setTimeout(function () {
                    var operationsIds = VOMTableOperations.loadSelectedOperationsIds();

                    window.open(VOMDownload.downloadMapLayers +
                                    "?operationsIds=" + operationsIds
                                    + "&layer=ServicesLayer&filterVO=" + filterVo, "_blank");
                }, 20);
            }

            if ($('#MilestoneLayer').is(":checked")) {
                setTimeout(function () {
                    var operationsIds = VOMTableOperations.loadSelectedOperationsIds();

                    window.open(VOMDownload.downloadMapLayers +
                                    "?operationsIds=" + operationsIds
                                    + "&layer=MilestoneLayer&filterVO=" + filterVo, "_blank");
                }, 30);
            }
            $('#exportMapLayers').modal('hide');
        }
    });
};

VOMDownload.selectAllVO = function () {
    $("[name=selectAllVO]").change(function () {
        var status = this.checked;

        $('[name=selectedVO]').each(function () {
            this.checked = status;

            if (status) {
                $(this).attr("checked", "checked");
            }

            if (!status) {
                $(this).removeAttr("checked");
            }
        });
    });
};

VOMDownload.singleCheckBoxVO = function () {
    $('[name=selectedVO]').change(function () {

        var selectAllVO = $("[name=selectAllVO]")[0];
        var $currentElement = $(this);

        if (!this.checked) {
            selectAllVO.checked = false;
            $currentElement.removeAttr("checked");
            $(selectAllVO).removeAttr("checked");
        }

        if (this.checked) {
            $currentElement.attr("checked", "checked");
        }

        if ($('[name=selectedVO]:checked').length ===
            $('[name=selectedVO]').length) {
            selectAllVO.checked = true;
            $(selectAllVO).attr("checked", "checked");
        }
    });
};
