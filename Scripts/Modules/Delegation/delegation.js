$(document).ready(function () {
    if ($("#authorizeAllFilter input[name='User']").length == 0 && globalVariables.delegationId == 0) {
        searchDelegatorFilter();
    }
    else {
        $("#AuthorizationTable").paginationConfluense(5).sortableConfluense();
        $("#tableAuthorizationContent tbody").removeAttr("hidden");
    }

    $('[name="startDate"]').attr("dd-min-date", globalVariables.actualDate);
    $('[name="endDate"]').attr("dd-greater-date", "startDate");

    var myDate = converToDate($('[name="startDate"]').val());
    myDate.setFullYear(myDate.getFullYear() + 1);

    $('[name="endDate"]').attr("dd-greater-date", "startDate")
        .attr("dd-max-date", convertDateToStringDDMMMYYYY(myDate));

    Validation.Init();

    errorDate($('[name="startDate"]'));
    if (parseInt(globalVariables.delegationId) == 0) {
        edit();
    }
    else {
        $("#PageContent").removeClass("hide");
    }
    $("#DocumentsTable").paginationConfluense(5).sortableConfluense();

});

$(document).on('click', 'button[data-navigate]', function () {
    showLoader();
});

$(document).on('change', '[name="startDate"], [name="endDate"]', function (element) {
    var $this = $(this);

    if (element.currentTarget.name == "startDate") {
        if ($this.val().length > 0) {
            var myDate = converToDate($this.val());
            myDate.setFullYear(myDate.getFullYear() + 1);

            var validation = $('[name="endDate"]')
                .attr("dd-greater-date", "startDate")
                .attr("dd-max-date", convertDateToStringDDMMMYYYY(myDate))
                .val("")
                .removeClass('validation-fail')
                .attr('data-validation-id');

            $('[validation-id="' + validation + '"]').text("").addClass('hide');
        }
    }

    errorDate($this);
});

function StartEndDateValidation(element) {
    var $this = $(element);

    if (element.currentTarget.name == "startDate") {
        if ($this.val().length > 0) {
            var myDate = converToDate($this.val());
            myDate.setFullYear(myDate.getFullYear() + 1);

            var validation = $('[name="endDate"]')
                .attr("dd-greater-date", "startDate")
                .attr("dd-max-date", convertDateToStringDDMMMYYYY(myDate))
                .val("")
                .removeClass('validation-fail')
                .attr('data-validation-id');

            $('[validation-id="' + validation + '"]').text("").addClass('hide');
        }
    }
    else {
        if ($("#AuthorizeAllContent").length > 0
            && $("span.validation-element:not(.hide)").length == 0) {
            validateDates();
        }
    }

    errorDate($this);
}

function errorDate($this) {
    if ($this.val().length > 0 &&
        $('[name="unavailableDates"]').val().length > 0 &&
        !$this.is('.validation-fail')) {

        var listDates = jQuery.makeArray($('[name="unavailableDates"]').val().split('|'));
        var date = converToDate($this.val());
        var formatDate = ("0" + date.getDate()).slice(-2) + "-" + (("0" + (date.getMonth() + 1)).slice(-2)) + "-" + date.getFullYear();

        if ($.inArray(formatDate, listDates) >= 0) {
            var validation = $this.addClass('validation-fail').attr('data-validation-id');

            $('[validation-id="' + validation + '"]').text(globalVariables.msgDateError).removeClass('hide');
        }
    }
}

$(document).on('click',
           'ul.dropdown-menu[aria-labelledby="id-ddlPagination"] a',
           function () {
               var value = $(this).attr('dd-value');
               if (value === "All") {
                   value = $("#AuthorizationTable tbody tr").length;
               }
               $("#AuthorizationTable").paginationConfluense(parseInt(value));
           });

function searchDelegatorFilter(clear) {
    var isNormal = $("#authorizeAllFilter input[name='User']").length == 0;
    var searchDelegatorName = clear == true ?
        ""
        : (!isNormal
            ? $("#authorizeAllFilter input[name='User']").val()
            : globalVariables.currentUser);
    var url = globalVariables.urlSearchDelegator;
    if (clear == true || searchDelegatorName !== "") {
        showLoaderOptional()

        $.ajax({
            url: url,
            type: 'POST',
            data: {
                'searchDelegatorName': searchDelegatorName
            },
            async: true,
            success: function (data) {
                var resultSearchTable = $("#tableAuthorizationContent");
                $(resultSearchTable).html(data);
                bindHandlers($(resultSearchTable));

                var value = isNormal ? 5
                    : $("#AuthorizeAllContent [name='ddlPagination']").val()

                if (value === "All") {
                    value = $("#AuthorizationTable tbody tr").length;
                }

                $("#AuthorizationTable").paginationConfluense(parseInt(value)).sortableConfluense();

                if (clear != true) {
                    $('[name="User_text"]').prop('disabled', true);
                    $('input[name="User_text"]')
                        .closest('div').find('button').prop('disabled', true);
                }

                bindHandlers($(resultSearchTable));
                if ($("#AuthorizationTable > tbody > tr").length > 0) {
                    $("#PageContent").removeClass("hide");
                }
                hideLoaderOptional();
                $("#tableAuthorizationContent tbody").removeAttr("hidden");

                Validation.Init();

                if (typeof window.errorDate == "function") {
                    errorDate($('[name="startDate"]'));
                }
            },
            error: function (xhr) {
                hideLoaderOptional();
                $("#tableAuthorizationContent tbody").removeAttr("hidden");
            }
        });
    } else {
        showMessage(globalVariables.msgSelectDelegator);
        $("#tableAuthorizationContent tbody").removeAttr("hidden");
    }
}

function clear() {
    $("#PageContent").addClass("hide");
    searchDelegatorFilter(true);

    var searchDelegatorNameText = $('input[name="User_text"]');
    searchDelegatorNameText.val('');
    $('[name="User"]').val('');
    $('[name="User_text"]').prop('disabled', false);
    searchDelegatorNameText.closest('div').find('button').prop('disabled', false);
    $("#DocumentsTable").find("tbody > tr").remove();
    $("#delegationReason").closest("div").FirstorDefault();
    $("[name=SearchDelegateUser]").closest("div").SetValue("", "");
    $("#UserCommentFields").children().remove();
    Validation.Init();
}

function renderizeRow(documentList, sourceType, fileNames) {
    var docNumber;
    var fileName;
    if (documentList.length <= 0)
        return;
    if (sourceType === "selected") {
        if (documentList.length > 0) {
            docNumber = documentList[0].DocumentNumber;
            fileName = documentList[0].DocumentName;
        }

    } else {
        if (documentList.length > 0) {
            docNumber = documentList[0];
        }
        if (fileNames.length > 0) {
            fileName = fileNames[0];
        }
    }

    var url = $('#tableDocumentContent').attr('data-loadurl');

    $.ajax({
        type: "POST",
        url: url,
        data: {
            documentNumber: docNumber,
            fileName: fileName
        },
        success: function (data) {
            $('#DocumentsTable > tbody').append(data);
            var divDocuments = $('#DocumentsTable').find('tbody');
            if ($(divDocuments).find('td.dataTables_empty').length > 0) {
                $(divDocuments).find('td.dataTables_empty').parent().remove();
            }
            var value = $('[name="ddlDocumentsPagination"]').val();
            if (value === "All") {
                value = $("#DocumentsTable tbody tr").length;
            }
            $("#DocumentsTable").paginationConfluense(parseInt(value));
            closeModal();
            enterEditMode(false, $(divDocuments), false);
        },
        error: function (error) {
            showMessage(error);
        }
    });
}

function downloadDocumentsExport(source) {
    var formatType = source.attr('name');
    var rowSelected = $('#AuthorizationTable tbody tr').has(":checked");
    var userName = globalVariables.currentUser;
    var operationNumber = rowSelected.find("td.OperationNumber").find("span:first").text();
    var delegationId = globalVariables.delegationId;
    var roleDelegetor = rowSelected.find('[name="roleIdDelegator"]').val();
    var country = rowSelected.find("td.country").find("span:first").text();
    var department = rowSelected.find("td.department").find("span:first").text();
    var cDepartment = rowSelected.find("td.cDepartment").find("span:first").text();
    var division = rowSelected.find("td.division").find("span:first").text();
    var operationNumbers = [];
    var countrys = [];
    var departments = [];
    var cDepartments = [];
    var divisions = [];

    $('#AuthorizationTable').find('input:checked').each(function () {
        var row = $(this).closest('tr');
        var operationNumber = $(row).find('td.operationNumber span').text()
        if (operationNumber != "" && !isInArray(operationNumber, operationNumbers)) {
            operationNumbers.push(operationNumber);
        }

        var country = $(row).find('td.country span').text()
        if (country != "" && !isInArray(country, countrys)) {
            countrys.push(country);
        }

        var department = $(row).find('td.department span').text()
        if (department != "" && !isInArray(department, departments)) {
            departments.push(department);
        }

        var cDepartment = $(row).find('td.cDepartment span').text()
        if (cDepartment != "" && !isInArray(cDepartment, cDepartments)) {
            cDepartments.push(cDepartment);
        }

        var division = $(row).find('td.division span').text()
        if (division != "" && !isInArray(division, divisions)) {
            divisions.push(division);
        }
    });
    var url = globalVariables.urlDownloadReport + '?formatType=' +
            formatType +
            '&userName=' +
            userName +
            '&operationNumber=' +
            operationNumbers +
            '&delegationId=' +
            delegationId +
            '&roleId=' +
            roleDelegetor +
            '&country=' +
            countrys +
            '&countryDepartment=' +
            cDepartments +
            '&department=' +
            departments +
            '&division=' +
            divisions;
    window.open(url, '_blank');
}