$(document).ready(function () {

    $(document).on('click', 'button.addNewCondition', function () {
        showLoader();
        redirectPage($(this).data('route'));
    });

    $(document).on('click', 'ul.dropdown-menu[aria-labelledby="id-ConditionStatus"] a', function (event) {
        var valueRoute = $(this).GetValue();
        var valueAction = valueRoute.split(';');
        var object = {};

        if (valueAction[0] === "NotAction") {
            return;
        }

        if (valueAction[0] === "Delete") {

            confirmAction(MessageTranslation._texts['deleteConditionMessage'])
                .done(function (value) {
                    if (value) {
                        AjaxCall(object, valueAction[1]);
                        return false;
                    }
                });

            return false;
        }

        if (valueAction[0] === "CreateExtension") {
            showLoader();
            redirectPage(valueAction[1]);

            return false;
        }

        AjaxCall(object, valueRoute);

        return false;
    });

    $(document).on('click', 'ul.dropdown-menu[aria-labelledby="id-AgreementStatus"] a', function (event) {
        var valueRoute = $(this).GetValue();
        var object = {};

        if (valueRoute === "NotAction") {
            return;
        }

        confirmAction(MessageTranslation._texts['deleteConditionMessage'])
            .done(function (value) {
                if (value) {
                    AjaxCall(object, valueRoute);
                    return false;
                }
            });

        return;
    });

    var grid = $(".grid2").kendoGrid({
        dataSource: {
            pageSize: 25,
            schema: {
                model: {
                    fields: {
                        ClauseNumber: { type: "string" },
                        ConditionDescription: { type: "string" },
                        ClauseDependency: { type: "string" },
                        CategoryName: { type: "string" },
                        TypeName: { type: "string" },
                        ExpirationDate: { type: "string" },
                        SubmitDate: { type: "string" },
                        ValidateDate: { type: "string" },
                        ValidationStageName: { type: "string" },
                    }
                }
            }
        },
        scrollable: false,
        sortable: false,
        selectable: false,
        filterable: false,
        pageable: {
            pageSize: 25
        },
        info: false,
        previousNext: false,
        resizable: true,
        allowUnsort: false,
        messages: {
            display: "",
            first: "",
            previous: "",
            next: "",
            last: "",
            refresh: ""
        },
        columns:
        [
            {
                field: "ClauseNumber",
                sortable: {
                    compare: function (a, b) {
                        if (a['ClauseNumber']) {
                            var str = a['ClauseNumber'];

                            var strA = a['ClauseNumber'].replace(/.*\<\/div\>/, '').trim();
                            var strB = b['ClauseNumber'].replace(/.*\<\/div\>/, '').trim();

                            var compareStringA = strA.match(/(?=\>).*?(?=\<)/)[0].replace(/^\>/g, '');
                            var compareStringB = strB.match(/(?=\>).*?(?=\<)/)[0].replace(/^\>/g, '');

                            return compareStringB.localeCompare(compareStringA);
                        }
                        return 0;

                    }
                }
            },
            {
                field: "ConditionDescription"
            },
            {
                field: "ClauseDependency"
            },
            {
                field: "CategoryName"
            },
            {
                field: "TypeName"
            },
            {
                field: "ExpirationDate",
                sortable: {
                    compare: function (a, b) {

                        var dateOne = null;
                        var dateTwo = null;

                        if (a.ExpirationDate.length > 0) {
                            dateOne = changeDateFormat(a.ExpirationDate);
                        } else {
                            dateOne = changeDateFormat("01 Jan 1900");
                        }

                        if (b.ExpirationDate.length > 0) {
                            dateTwo = changeDateFormat(b.ExpirationDate);
                        } else {
                            dateTwo = changeDateFormat("01 Jan 1900");
                        }

                        return dateOne.getTime() == dateTwo.getTime() ? 0 : (dateOne.getTime() > dateTwo.getTime()) ? 1 : -1;
                    }
                }
            },
            {
                field: "SubmitDate",
                sortable: {
                    compare: function (a, b) {

                        var dateOne = null;
                        var dateTwo = null;

                        if (a.SubmitDate.length > 0) {
                            dateOne = changeDateFormat(a.SubmitDate);
                        } else {
                            dateOne = changeDateFormat("01 Jan 1900");
                        }

                        if (b.SubmitDate.length > 0) {
                            dateTwo = changeDateFormat(b.SubmitDate);
                        } else {
                            dateTwo = changeDateFormat("01 Jan 1900");
                        }

                        return dateOne.getTime() == dateTwo.getTime() ? 0 : (dateOne.getTime() > dateTwo.getTime()) ? 1 : -1;
                    }
                }
            },
            {
                field: "ValidateDate",
                sortable: {
                    compare: function (a, b) {

                        var dateOne = null;
                        var dateTwo = null;

                        if (a.ValidateDate.length > 0) {
                            dateOne = changeDateFormat(a.ValidateDate);
                        } else {
                            dateOne = changeDateFormat("01 Jan 1900");
                        }

                        if (b.ValidateDate.length > 0) {
                            dateTwo = changeDateFormat(b.ValidateDate);
                        } else {
                            dateTwo = changeDateFormat("01 Jan 1900");
                        }

                        return dateOne.getTime() == dateTwo.getTime() ? 0 : (dateOne.getTime() > dateTwo.getTime()) ? 1 : -1;
                    }
                }
            },
            {
                field: "ValidationStageName",
                attributes: { "class": "fixedLimitColumnSatus" }
            }
        ],
        dataBound: function (e) {
            $(".radio").click(function () {
                var rButtons = $(".table_search_item").find("table").find("tbody").find(".radio");
                for (var i = 0; i < rButtons.length; i++) {
                    $(rButtons[i]).removeClass("checked");
                }
                $(this).addClass("checked");
                for (var i = 0; i < rButtons.length; i++) {
                    if ($(rButtons[i]).hasClass("checked")) {
                        $(rButtons[i]).closest("tr").addClass("k-state-selected");
                    } else {
                        $(rButtons[i]).closest("tr").removeClass("k-state-selected");
                    }
                }
            });

            $(".grid2").find(".btn_delete").click(function () {
                $('.mod_tabla.mod_tabla_agrup').hide();
                $('.pie.bottom').hide();
                $('.table_search_item').hide();
                $('.buscadorConvergence').hide();
                $('.cancelConfirm').show();
                $('.pie.top').show();
            });

            $(".k-pager-nav").hide();
            $(".k-pager-info").hide();
            $(".k-pager-first").hide();
            $(".k-pager-last").hide();
        }
    }).data("kendoGrid");

    $('button[name="clearBtn"]').on('click', function (event) {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        var container = $('div.filter');

        container.find('button[id=id-ConditionType]').FirstorDefault();
        container.find('button[id=id-FilterConditionStatus]').FirstorDefault();
        container.find('button[id=id-ConditionCategory]').FirstorDefault();
        container.find('#conditionNumberFilter').val(null);
        container.find('input[name="specialclause2"]').removeAttr('checked');
        container.find('input[name="dateExpirationFrom"]').val(null);
        container.find('input[name="dateExpirationTo"]').val(null);
    });

    $('button[name="filterBtn"]').on('click', function (event) {
        event.preventDefault ? event.preventDefault() : event.returnValue = false;
        var url = $(this).data("route");
        var values = new Object();
        var container = $('div.filter');

        values._data = {};
        values._data['conditionTypeId'] = container.find('button[id=id-ConditionType]').GetValue();
        values._data['conditionStatusId'] = container.find('button[id=id-FilterConditionStatus]').GetValue();
        values._data['conditionCategoryId'] = container.find('button[id=id-ConditionCategory]').GetValue();
        values._data['conditionNumber'] = container.find('#conditionNumberFilter').val();
        values._data['expirationDateFrom'] = container.find('input[name="dateExpirationFrom"]').val();
        values._data['expirationDateTo'] = container.find('input[name="dateExpirationTo"]').val();
        values._data['isFilter'] = false;

        if ((values._data['conditionTypeId'] != "") || (values._data['conditionStatusId'] != "") ||
            (values._data['conditionCategoryId'] != "") || (values._data['conditionNumber'] != "") ||
            (values._data['expirationDateFrom'] != "") || (values._data['expirationDateTo'] != "")) {
            values._data['isFilter'] = true;
        }

        test = $(this).data("route") +
            '&FilterTypeId=' + values._data['conditionTypeId'] +
            '&FilterStatusId=' + values._data['conditionStatusId'] +
            '&FilterCategoryId=' + values._data['conditionCategoryId'] +
            '&FilterNumber=' + values._data['conditionNumber'] +
            '&FilterExpirationDateFrom=' + values._data['expirationDateFrom'] +
            '&FilterExpirationDateTo=' + values._data['expirationDateTo'] +
            '&isFilter=' + values._data['isFilter'];

        redirectPage(test);
    });

    selectedOperation();
    
});

function loansToSee(selectList) {
    if (selectList.value == "All") {
        // Show All Loans
        $(".loanTables").show();
        
    } else {
        // Mostrar
        $(".loanTables").hide();
        $("#" + selectList.value).show();
    }
}

function changeDateFormat(strDate) {
    var splittedDate = strDate.split(" ");
    var convertedDate = "";
    var strMonth = splittedDate[1];
    var numericMonth = -1;

    switch (strMonth) {
        case "Jan": numericMonth = "01"; break;
        case "Feb": numericMonth = "02"; break;
        case "Mar": numericMonth = "03"; break;
        case "Apr": numericMonth = "04"; break;
        case "May": numericMonth = "05"; break;
        case "Jun": numericMonth = "06"; break;
        case "Jul": numericMonth = "07"; break;
        case "Aug": numericMonth = "08"; break;
        case "Sep": numericMonth = "09"; break;
        case "Oct": numericMonth = "10"; break;
        case "Nov": numericMonth = "11"; break;
        case "Dec": numericMonth = "12"; break;
    }

    convertDate = splittedDate[0] + "/" + numericMonth + "/" + splittedDate[2];

    return kendo.parseDate(convertDate, "dd/MM/yyyy");
}

function selectedOperation() {
    var cssClass = "ui-state-active";
    var mainOperation = $("#radio").data("mainoperation");
    if ($("#labelForRadio1").hasClass(cssClass)) {
        $(".loanTables").hide();
        $("." + mainOperation).show();
    } else {
        $(".loanTables").show();
    }

    
}

function GroupOfOperations() {
    $(".loanTables").show();
}

function DeleteClause(option) {
    var OptionSelected = $("option:selected", option).text();
    var OptionSelectedValue = $(option).find("option:selected").val();

    if (OptionSelected == $("#ConstantDelete").val()) {
        idbg.lockUi(null, true);
        deleteItem(OptionSelectedValue);
    }
    else if (OptionSelected == $("#ConstantSentToTrack").val() ||
        OptionSelected == $("#ConstantSentToTrackWidthDraw").val() ||
        OptionSelected == $("#ConstantCreateClause").val()) {
        idbg.lockUi(null, true);

        var obj = option.parentNode;
        while (obj.nodeName != "TABLE") { obj = obj.parentNode; }
        location.href = OptionSelectedValue + "&locatetop=" + document.getElementById('locatetop').innerHTML + "|" + obj.id + "|" + $("#" + obj.id).data("kendoGrid").dataSource.page();
    }
    else {
        idbg.lockUi(null, false);
    }
}

function deleteItem(element) {
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: element,
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
            idbg.lockUi(null, false);
        }
    }).data("kendoWindow");
    $(".k-window-titlebar").addClass("warning");
    $(".k-window-title").addClass("ico_warning");
    dialog.center();
    dialog.open();
}

