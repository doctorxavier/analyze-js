var resizeFrameDelay = 500;

$(document).ready(function () {

    loadingPanelNew = $('#layoutLoadingDiv');
    loadingPanelNewOptinal = $('#layoutLoadingOptionalDiv');

    $('.kendoDropDown').kendoDropDownList();

    $(".kendoDatePicker").kendoDatePicker({
        open: function () {
            var calendar = this.dateView.calendar;
            calendar.wrapper.width(this.wrapper.width());
        }
    });

    $("#btnSearch").click(function (e) {
        var createdDate = dateFormat($("#dateCreated").data("kendoDatePicker").value());
        var modifiedDate = dateFormat($("#dateModified").data("kendoDatePicker").value());

        getData(JSON.stringify({
            OperationNumber: $("#hdnOperationNumber").val(),
            UserName: $("#hdnUserName").val(),
            KeyWord: $("#txtkeyWord").val(),
            Created: createdDate,
            Modified: modifiedDate,
            Status: $("#ddlStatus").data("kendoDropDownList").value(),
            WorkflowType: $("#ddlWorkflowType").data("kendoDropDownList").value()
        }), true);
    });

    $("#btnClear").click(function (e) {
        if ($("#txtkeyWord").length) { $("#txtkeyWord").val(""); }
        $("#dateCreated").val("");
        $("#dateModified").val("");
        $("#ddlStatus").data("kendoDropDownList").value("");
        $("#ddlWorkflowType").data("kendoDropDownList").value("");
    });

    getData(JSON.stringify({
        OperationNumber: $("#hdnOperationNumber").val(),
        UserName: $("#hdnUserName").val(),
        KeyWord: null,
        Created: null,
        Modified: null,
        Status: 'started',
        WorkflowType: 0
    }), false);

    setTimeout(resizeIframeCloud, resizeFrameDelay);
});

$(document).on('mousedown', 'a.k-icon', function () {
    setTimeout(resizeIframeCloud, resizeFrameDelay);
});

function getData(filter, isSearch) {
    $.ajax({
        url: $("#hdnSearchPath").val(),
        data: filter,
        dataType: "json",
        contentType: "application/json",
        type: 'post',
        timeout: 360000,
        success: function (workflows) {
            initGrid(workflows, isSearch);
        },
        error: function (xmlhttprequest, textstatus, message) {
        }
    });
}

function initGrid(workflows, isSearch) {

    if ($("#main-workflows-grid").data('kendoGrid') == null) {

        var grid = $("#main-workflows-grid").kendoGrid({
            dataSource: {
                data: [],
                schema: {
                    model: {
                        fields: {
                            OperationId: { type: "number" },
                            StartDate: { type: "date" },
                            EndDate: { type: "date" },
                            UserId: { type: "number" },
                            NotificationDate: { type: "date" },
                            NotificationName: { type: "string" },
                            NotificationWorkFlow: { type: "string" },
                            NotificationOperation: { type: "string" },
                            NotificationLoan: { type: "string" },
                            NotificationRead: { type: "string" },
                            NotificationNew: { type: "string" },
                            Created: { type: "date" },
                            CreatedBy: { type: "string" },
                            Modified: { type: "date" },
                            ModifiedBy: { type: "string" },
                            Submiter: { type: "string" }
                        }
                    }
                },
                pageSize: 10
            },
            scrollable: false,
            sortable: false,
            selectable: false,
            filterable: false,
            pageable: true,
            info: false,
            previousNext: false,
            resizable: true,
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
                    field: "FolioId",
                    title: $("#hdnWorkflowIdTitle").val(),

                },
                {
                    field: "OperationNumber",
                    title: $("#hdnOperationTitle").val(),
                    template: kendo.template($("#operation-column").html())
                },
                {
                    field: "WorkflowName",
                    title: $("#hdnWorkflowTitle").val(),
                },
                {
                    field: "WorkflowStatus",
                    title: $("#hdnStatusTitle").val()
                },
                {
                    field: "Submiter",
                    title: $("#hdnSubmiterTitle").val()
                },
                {
                    field: "Users",
                    title: $("hdnAssignedToTitle").val(),
                    template: '#= FormatUsers(Users) #',
                    hidden: true
                },
                {
                    field: "StartDate",
                    title: $("#hdnStartDateTitle").val(),
                    format: "{dd MMM yy}",
                    template: '#= FormatJsonDate(StartDate) #'
                },
                {
                    field: "EndDate",
                    title: $("#hdnEndDateTitle").val(),
                    format: "{dd MMM yy}",
                    template: '#= FormatJsonDate(EndDate)#'
                },
                {
                    field: "Loans",
                    title: $("#hdnLoansTitle").val(),
                    template: '#= FormatLoans(Loans) #'
                }
            ],
            detailTemplate: kendo.template($("#template").html()),
            detailInit: detailInit,
            dataBound: function (e) {

                updateWorkflowsCounter($("#main-workflows-grid").data("kendoGrid").dataSource.total());

                $("a.k-icon").click(function (e) {
                    var background = $(this).css("background");
                    if (background.indexOf("flecha_arriba_azul_table") > 0) {
                        $(this).removeClass("k-iconUp");
                    } else {
                        $(this).addClass("k-iconUp");
                    }
                });


            }
        }).data("kendoGrid");
    }
    else {
        $("#main-workflows-grid").data('kendoGrid').dataSource.data([]);
    }

    if (workflows != null && workflows.length > 0 && isSearch == false) {

        $.each(workflows, function (index, workflow) {
            workflow.StartDate = kendo.parseDate(workflow.StartDate);
            workflow.EndDate = kendo.parseDate(workflow.EndDate);
        });

        $("#main-workflows-grid").data('kendoGrid').dataSource.data(workflows);

        $("#main-workflows-grid").data('kendoGrid').refresh();

        $("#main-workflows-grid").data('kendoGrid').dataSource.page(1);

    } else {

        if (workflows != null && workflows.length > 0 && isSearch == true) {

            $.each(workflows, function (index, workflow) {
                workflow.StartDate = kendo.parseDate(workflow.StartDate);
                workflow.EndDate = kendo.parseDate(workflow.EndDate);
            });

            $("#main-workflows-grid").data('kendoGrid').dataSource.data(workflows);

            $("#main-workflows-grid").data('kendoGrid').refresh();

            $("#main-workflows-grid").data('kendoGrid').dataSource.page(1);

            updateWorkflowsCounter($("#main-workflows-grid").data("kendoGrid").dataSource.total());
        }
    }

//    setTimeout(correctIframeHeight, resizeFrameDelay);
}

function detailInit(e) {

    var detailRow = e.detailRow;

    detailRow.find(".tabstrip").append(
    "<div id='tabStrip" + e.data.WorkflowId + "'>" +
         "<ul id='header" + e.data.WorkflowId + "'>" +
             "<li style='font-weight:bold; z-index: 400;'>" + $("#hdnTaskTitle").val() + "</li>" +
             "<li style='font-weight:bold' class='k-state-active'>" + $("#hdnCommentTitle").val() + "</li>");

    if (e.data.IsWorkflowClause === true) {
        detailRow.find("#header" + e.data.WorkflowId).append(
           "<li style='font-weight:bold' class='k-state-active'>" + $('#hdnClauseTitle').val() + "</li>");
    }

    detailRow.find("#tabStrip" + e.data.WorkflowId).append(
          "</ul>" +
          "<div>" +
          "<table style='width:100%'>" +
              "<tr>" +
                  "<td style='width:25%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnRoleTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:13%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnUserTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:13%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnUserIdTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:12%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnCreationDateTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:12%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnModificationDateTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:10%'>" +
                      "<h5 style='font-weight: bold'>" + $("#hdnActionTitle").val() + "</h5>" +
                  "</td>" +
                  "<td style='width:15%'>" +
                      "<h5 style='font-weight: bold'></h5>" +
                  "</td>" +
              "</tr></table>" +
          "<div id='DivTask" + e.data.WorkflowId + "'/></div>" +
          "" +
          "<div><div id='DivComment" + e.data.WorkflowId + "'/>" +
          "<div id='pager" + e.data.WorkflowId + "' class='k-pager-wrap'/></div>");

    if (e.data.IsWorkflowClause === true) {

        detailRow.find("#tabStrip" + e.data.WorkflowId).append(
        "<div>" +
        "<table style='width:100%'>" +
            "<tr>" +
                "<td style='width:20%'>" +
                    "<h5 style='font-weight: bold'>" + $("#hdnClauseNumber").val() + "</h5>" +
                "</td>" +
                "<td style='width:100%'>" +
                    "<h5 style='font-weight: bold'>" + $("#hdnClauseDescription").val() + "</h5>" +
                "</td>" +
            "</tr></table>" +
        "<div id='DivClause" + e.data.WorkflowId + "'/></div>" +
        "</div>");
    }

    $("#tabStrip" + e.data.WorkflowId).kendoTabStrip({
        animation: {
            open: {
                effects: "fadeIn"
            },
        },
        select: (function (tse) {
            if ($(tse.item).find("> .k-link").text() == $("#hdnTaskTitle").val()) {

                $("#DivTask" + e.data.WorkflowId).kendoListView({
                    dataSource: {
                        transport: {
                            read: {
                                url: $("#hdnTasksPath").val(),
                                dataType: "json",
                                data: { workflowId: e.data.WorkflowId },
                                type: "POST"
                            }
                        },
                        schema: {
                            model: {
                                fields: {
                                    WorkflowInstanceTaskId: { type: "int" },
                                    TaskNameEn: { type: "string" },
                                    FullName: { type: "string" },
                                    Username: { type: "string" },
                                    StartDate: { type: "date" },
                                    Due: { type: "date" },
                                    Status: { type: "string" },
                                    ActivateChangeGroup: { type: "string" }
                                }
                            }

                        }
                    },
                    template: kendo.template($("#templateTask").html()),
                    pageable: true
                });
            }

            if ($(tse.item).find("> .k-link").text() == $("#hdnCommentTitle").val()) {

                $("#DivComment" + e.data.WorkflowId).kendoListView({
                    dataSource: {
                        transport: {
                            read: {
                                url: $("#hdnCommentsPath").val(),
                                dataType: "json",
                                data: { workflowId: e.data.WorkflowId },
                                type: "POST"
                            }
                        },
                        schema: {
                            model: {
                                fields: {
                                    UserCommentId: { type: "int" },
                                    Modified: { type: "date" },
                                    ModifiedBy: { type: "string" },
                                    Text: { type: "string" }
                                }
                            }

                        },
                        pageSize: 5
                    },
                    template: kendo.template($("#templateComment").html())
                });

                $("#pager" + e.data.WorkflowId).kendoPager({
                    dataSource: $("#DivComment" + e.data.WorkflowId).data("kendoListView").dataSource
                });
            }

            if (e.data.IsWorkflowClause === true) {

                if ($(tse.item).find("> .k-link").text() == $("#hdnClauseTitle").val()) {

                    $("#DivClause" + e.data.WorkflowId).kendoListView({
                        dataSource: {
                            transport: {
                                read: {
                                    url: $("#hdnClauseInformation").val(),
                                    dataType: "json",
                                    data: { workflowId: e.data.WorkflowId },
                                    type: "POST"
                                }
                            },
                            schema: {
                                model: {
                                    fields: {
                                        ClauseNumber: { type: "string" },
                                        ClauseDescription: { type: "string" }
                                    }
                                }
                            }
                        },
                        template: kendo.template($("#templateClause").html()),
                    });
                }
            }
        })
    });

    var tabStrip = $("#tabStrip" + e.data.WorkflowId).getKendoTabStrip();
    tabStrip.select(0);

}

function FolioWorkFlowId(wfId) {
    var url = "<a href='" + $("#hdnK2DetailPath").val() + wfId.substring(wfId.lastIndexOf(".") + 1) + "' target='_blank'>" + wfId + "</a>";

    return url;
}

function FormatWorkflowName(wfName, wfId) {
    var url = "<a href='" + $("#hdnK2DetailPath").val() + wfId.substring(wfId.lastIndexOf(".") + 1) + "' target='_blank'>" + wfName + "</a>";

    return url;
}

function FormatJsonDate(jsonDt) {
    var theDate = kendo.toString(jsonDt, "dd MMM yy");

    if (theDate == null || theDate == "null")
        return "";
    else
        return theDate;
}

function FormatLoans(loanList) {
    var loansHTML = "<div>";

    $.each(loanList, function (index, value) {
        loansHTML = loansHTML + "<div>" + value + "</div>";
    });
    return loansHTML + "</div>";
}

function FormatUsers(users) {
    var usersHTML = "<div>";
    $.each(users, function (index, value) {
        usersHTML = usersHTML + "<div class='WFUsrRole'>" + value.UserRole + "</div>" + "<div class='WFUsrName'>" + value.UserName + "</div>";
    });
    return usersHTML + "</div>";
}

function updateWorkflowsCounter(currentCounter) {
    $("#notificationQty").html(currentCounter);
}

function initLoading() {
    $(".k-loading-mask").show();
}

function finishLoading() {
    $(".k-loading-mask").hide();
}

function dateFormat(date) {
    var currDate = null;
    if (date != null) {
        currDate = date.toLocaleDateString("en-US");
    }

    return currDate;
}