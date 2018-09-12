function updateGridOptionSelect()
{
    $('.grid2 select.optionSelect').each(function() {
        if(!$(this).data('.kendoDropDownList')) {
            $(this).kendoDropDownList();
        }
    });
}

$(document).ready(function () {
    if ($('#lastDisbursment').val()=='True')
    {               
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $('body').append('<div class="ui-widget-overlay ui-front"></div>');

        $('body').append('<form id="formClauses"' +
            '<div class="dinamicModal"><div style="padding: 20px;">' + $("#mensajeLastDisbursment").val() + '</div><div class="pie pieReassign"> <div class="botones"> ' +
            '<a title="Cancel" id="CancelWarningDialog" onclick="removemodal();" ' +
            'href="javascript:void(0)">' + $("#Cancel").val() + '</a>'+
            '</div></div></div></form>');


        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: "Warning",
            draggable: false,
            resizable: false,
            //content:  Url.Action("IndexDeleteComponent", "Outputs", new { area = "ResultsMatrix" }),
            //content: '/SignaturesAndContacts/Signatures/IndexDeleteAsignatureWarning',//?userid=' + 'ueyuey' + '&usersignatureid=' + 2 + '&documentreference=' + 'uriuti',
            pinned: true,
            actions: [
                "Close"
            ],
            //open: function (e) {
            //    this.wrapper.attr("style", "top:" + positionRelative.top + "px !important;margin-top:50px");
            //},
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();

        
    }


    var cssClasse = "custom checkbox";

    $("#chk_1").removeClass("disabled");

    /**
    * Jira ID: CON-735
    * Fix Description: replace build grid method
    * Attended by: Jhon Astaiza 
    * Date: 10/09/2014
    * Begin Fix
    **/
    var grid = $(".grid2").kendoGrid({
        dataSource: {
            pageSize: 25,
            schema: {
                model: {
                    fields: {
                        ClauseNumber: { type: "string" },
                        ClauseDescription: { type: "string" },
                        ClauseDependency: { type: "string" },
                        ClauseCaterogy: { type: "string" },
                        ClauseType: { type: "string" },
                        SpecialClause: { type: "string" },
                        ClauseLocation: { type: "string" },
                        ExpirationDate: { type: "string" },
                        SubmitDate: { type: "string" },
                        ClauseApproval: { type: "string" },
                        ClauseStatus: { type: "string" },
                    }
                }
            }
        },
        scrollable: false,
        sortable: {
            allowUnsort: false
        },
        selectable: false,
        filterable: false,
        pageable: {
          pageSize: 25,
          change: updateGridOptionSelect
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
                field: "ClauseDescription"
            },
            {
                field: "ClauseDependency"
            },
            {
                field: "ClauseCaterogy"
            },
            {
                field: "ClauseType"
            },
            {
                field: "SpecialClause",
                attributes: { "class": "fixedTextCenter"}
            },
            {
                field: "ClauseLocation"
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
                field: "ClauseApproval",
                sortable: {
                    compare: function (a, b) {
                        
                        var dateOne = null;
                        var dateTwo = null;

                        if (a.ClauseApproval.length > 0) {
                            dateOne = changeDateFormat(a.ClauseApproval);
                        } else {
                            dateOne = changeDateFormat("01 Jan 1900");
                        }

                        if (b.ClauseApproval.length > 0) {
                            dateTwo = changeDateFormat(b.ClauseApproval);
                        } else {
                            dateTwo = changeDateFormat("01 Jan 1900");
                        }

                        return dateOne.getTime() == dateTwo.getTime() ? 0 : (dateOne.getTime() > dateTwo.getTime()) ? 1 : -1;
                    }
                }
            },
            {
                field: "ClauseStatus",
                attributes: { "class": "fixedLimitColumnSatus"}
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

    $('.grid2 thead tr th').eq(0).trigger('click');
    $('.grid2 thead tr th').eq(0).trigger('click');

    if (grid != null) {
        grid.dataSource.bind("change", function (e) {

            updateGridOptionSelect();
            $(window.parent.document.body).scrollTop(0);
        });
    };

    if (grid == null) {
        showNotification({
            'type': 'error',
            'message': $('[name="no-contract-data-error-text"]').val()
        });
    }

    /**
    * End Fix
    **/

    updateGridOptionSelect();
    
    setTimeout(function() {
        updateGridOptionSelect();
    }, 1000);
    

    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });

    $("input[type='checkbox'],input[type='radio']").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });

    $('#clearBtn').click(function () {
        var form = $(this).closest('form');
        if (form.length > 0) {
            form.find('select').each(function () { this.selectedIndex = 0 });
            form.find('input[type="text"]').val('');

            var specialCheck = form.find('#chk_1');
            if (specialCheck.length > 0 && specialCheck.hasClass('checked')) {
                specialCheck.removeClass('checked');
                isSpecialClick();
            }
        }
    });

    $('#filterBtn').click(function () {
        test = $(this).data("route") +
            '&clauseTypeId=' + $("#clauseTypeFilter").val() +
            '&clauseStatusId=' + $("#clauseStatusFilter").val() +
            '&clauseCategoryId=' + $("#clauseCategoryFilter").val() +
            '&clauseNumber=' + $("#clauseNumberFilter").val() +
            '&isSpecial=' + $('#isSpecial').val() +
            '&expirationDateFrom=' + $("#datePicker5").val() +
            '&expirationDateTo=' + $("#datePicker6").val();
        redirectPage(test);
    });

    $(".dropDownChangeToTrack").change(function () {
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = "Warning";
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: $(this).data("route"),
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
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();
    });

    $(".dropDownChangeToRecord").change(function () {
        var value = $(this).val();
        if (value != "")
            $(this).data("currentvalue", value);
        if ($(this).data("currentvalue") == "Record") {
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
            var title = "Warning";
            var dialog = $(".dinamicModal").kendoWindow({
                width: "800px",
                title: title,
                draggable: false,
                resizable: false,
                content: $(this).data("route"),
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
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
            dialog.center();
            dialog.open();
        } else if ($(this).data("currentvalue") != undefined && $(this).data("currentvalue") != "Index") {
            location.href = $(this).data("currentvalue");
        }

    });

    selectedOperation();
    

});

$(window).load(function () {
    if ($('#isSpecial').val().toLowerCase() == "true") {
        $('#chk_1').attr("class", "custom checkbox checked");
        $('#chk_1').checked = true;
        isSpecialClick();
    }

    $('.filter span .k-dropdown-wrap').find('span').attr("style", "margin-bottom:4%; margin-top:3%; font-family:OpenSans-Italic; font-size:1em !important;")
    $('#datePicker5').attr("style", "font-family:OpenSans-Italic !important; font-size:1em !important;")
    $('#datePicker6').attr("style", "font-family:OpenSans-Italic !important; font-size:1em !important;")

    if (($('#FilterClauseType').val() != "") || ($('#FilterClauseStatus').val() != "") ||
        ($('#FilterClauseCategory').val() != "") || ($('#FilterClauseNumber').val() != "") ||
        ($('#FilterClauseIsSpecial').val() != "") || ($('#FilterClauseExpirationDateFrom').val() != "") ||
        ($('#FilterClauseExpirationDateTo').val() != "")) {
        var AltoFrame = $('.mod_contenido_central').height() + 300;
        $(window.parent.document).find('iframe').attr("height", AltoFrame);
    }
})

function removemodal() {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
}

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

/**
* Jira ID: CON-735
* Fix Description: replace date format
* Attended by: Jhon Astaiza 
* Date: 10/09/2014
* Begin Fix
**/
function changeDateFormat(strDate)
{
    var splittedDate = strDate.split(" ");
    var convertedDate = "";
    var strMonth =splittedDate[1];
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
/**
* End Fix
**/

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

function isSpecialClick() {
    if ($('#chk_1').hasClass("checked")) {
        $('#isSpecial').val('true');
    } else {
        $('#isSpecial').val(null);
    }
}

function GroupOfOperations() {
    $(".loanTables").show();
}

function DeleteClause(option) {
    var OptionSelected = $("option:selected", option).text();
    var OptionSelectedValue = $(option).find("option:selected").val();

    var optionSelectedClause = OptionSelected.toLowerCase();

    if (OptionSelected == $("#ConstantDelete").val()) {
        deleteItem(OptionSelectedValue);
    }
    else if (optionSelectedClause == $("#ConstantSentToTrack").val().toLowerCase() ||
        optionSelectedClause == $("#ConstantSentToTrackWidthDraw").val().toLowerCase() ||
        optionSelectedClause == $("#ConstantCreateClause").val().toLowerCase()) {
        idbg.lockUi(null, true);

        var obj = option.parentNode;
        while (obj.nodeName != "TABLE") {
            obj = obj.parentNode;
    }
        location.href = option.value + "&locatetop=" +
            document.getElementById('locatetop').innerHTML +
            "|" +
            obj.id +
            "|" +
            $("#" + obj.id).data("kendoGrid").dataSource.page();

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
    resizeIframeCloud();
}
