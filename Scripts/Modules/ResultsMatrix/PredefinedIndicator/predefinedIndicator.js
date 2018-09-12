
$(document).ready(function () {

    $(".optionSelectTypeIndicators").kendoDropDownList();

    $(".optionSelectPriorityAreas").kendoDropDownList();

    // Show ajax spinner
    $(document).ajaxStart(function (e) {
        var senderObject = e.delegateTarget.activeElement;
        if ($(senderObject).find("#typeIndicatorId") != null && $(senderObject).is("span")) {
            $("#loadingAreasContainer").fadeIn();
        }

        if ($(senderObject).attr("id") != null && $(senderObject).attr("id") == "btnSearchIndicator") {
            $("#loadingSearchContainer").fadeIn();
        }
    });

    // Hide ajax spinner
    $(document).ajaxStop(function (e) {
        var senderObject = e.delegateTarget.activeElement

        if ($(senderObject).find("#typeIndicatorId") != null && $(senderObject).is("span")) {
            $("#loadingAreasContainer").delay(500).fadeOut();
        }

        if ($(senderObject).attr("id") != null && $(senderObject).attr("id") == "btnSearchIndicator") {
            $("#loadingSearchContainer").delay(500).fadeOut();
        }
    });

    $("#btnSearchIndicator").click(function (e) {
        e.preventDefault();
        $.ajax({
            url: $(this).data("search-path"),
            data: serializeFilters(this),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {
                buildGrid(data.Indicators);
            }
        });
    });

    $(".btnLinkToPredefinedIndicator").click(function (e) {
        var predefinedIndicator = $("input[name=_predefinedToLink]:checked");

        if (predefinedIndicator != null) {

            var linkModel = {
                linkModel: {
                    baseIndicatorId: $("#hdnPredfIndicatorCurrentIndicator").val(),
                    predefinedIndicatorId: $(predefinedIndicator).val(),
                    backUrl: $("#hdnPredefinedIndicatorBackUrl").val(),
                    module: $("#hdnPredfIndicatorCurrentModule").val()
                },
                type: $("#hdnPredfIndicatorCurrentModule").val()
            };

            $.ajax({
                url: $("#hdnPredefIndicatorLinkPath").val(),
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(
                    linkModel
                ),
                success: function (data) {

                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();

                    location.href = data.backUrl
                }
            });

        }
    });

});

function buildGrid(indicators) {

    $("div.k-grid-pager").html("");

    if (indicators.length > 0) {
        var grid = $("#main-results-grid").kendoGrid({
            dataSource: {
                data: indicators,
                schema: {
                    model: {
                        fields: {
                            PredefinedIndicatorId: { type: "string" },
                            IndicatorNumber: { type: "string" },
                            NameEn: { type: "string" },
                            UnitOfMeasureEn: { type: "string" }
                        }
                    }
                },
                pageSize: 5
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
            columns: [
                {
                    field: "PredefinedIndicatorId",
                    title: '',
                    template: kendo.template($("#gridCell_rdBtnPredefinedIndicatorId").html()),
                    width: 10
                },
                {
                    field: "IndicatorNumber",
                    title: 'Indicator Number',
                    width: 10
                },
                {
                    field: "NameEn",
                    title: 'Indicator Name',
                    width: 300
                },
                {
                    field: "UnitOfMeasureEn",
                    title: 'Unit Of Measure',
                    width: 10
                },
                {
                    field: "PredefinedIndicatorId",
                    title: '',
                    template: kendo.template($("#gridCell_detailLink").html()),
                    width: 10
                }
            ]
        }).data("kendoGrid");

        $(".btnLinkToPredefinedIndicator").show();

        $("#countResultsContainer").hide();

        $("#main-results-grid").show();

    } else {
        $("#countResultsContainer").show();
        $("#countResults").text("0");

        if ($('#main-results-grid tr').length > 0) {

            $("#main-results-grid").data('kendoGrid').dataSource.data([]);

            $("#main-results-grid").hide();

            $(".btnLinkToPredefinedIndicator").hide();
        }
    }
}

function serializeFilters(lnkCurrentPage) {

    var filter = {
        'IndicatorType': $("#typeIndicatorId").data("kendoDropDownList").value(),
        'AreaId': $("#priorityArea").data("kendoDropDownList").value(),
        'IndicatorNumber': $("#indicatorNumber").val(),
        'IndicatorName': $("#indicatorName").val(),
        'UnitOfMeasure': $("#unitOfMeasure").val(),
        'Page': $(lnkCurrentPage).find("span").text()
    };

    return JSON.stringify(filter);
}

function loadPriorityAreas(slctTypeIndicators) {

    if ($(slctTypeIndicators).data("kendoDropDownList").value() != "") {

        $.ajax({
            url: $(slctTypeIndicators).data("areas-path"),
            data: JSON.stringify({ "indicatorId": $("#typeIndicatorId").data("kendoDropDownList").value() }),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {


                // Fill areas combo with response data
                var combobox = $("#priorityArea").data("kendoDropDownList");

                var updatedOptions = new Array();

                updatedOptions.push({ text: "All", value: "" });

                $.each(data.areas, function (index, area) {
                    updatedOptions.push({ text: area.NameEn, value: area.Id });
                });

                combobox.dataSource.data(updatedOptions);

                //reevaluate the data
                combobox.dataSource.query();

                //clear combobox selection.
                combobox.value("");

            }
        });

    } else {
        // Fill areas combo with response data
        var combobox = $("#priorityArea").data("kendoDropDownList");

        var updatedOptions = new Array();

        updatedOptions.push({ text: "All", value: "" });

        combobox.dataSource.data(updatedOptions);

        //reevaluate the data
        combobox.dataSource.query();

        //clear combobox selection.
        combobox.value("");
    }

}

function updateDetailContent(templateJSId, containerSelector, updatedData) {
    // Load persons grid template
    var fuente = $('#' + templateJSId).html();
    // Compile template
    var plantilla = Handlebars.compile(fuente);
    // Get filled template
    var html = plantilla(updatedData);

    $("#" + containerSelector).html(html);

    $("#searchAreaWrapper").hide();

    $("#" + containerSelector).show();

}

function showDetail(predefinedIndicatorId) {
    $.getJSON($("#hdnPredefIndicatorDetailPath").val(), { predefinedIndicatorId: predefinedIndicatorId }, function (data) {

        // Update PagingFooter Content
        updateDetailContent("indicatorDetail-template", "detailAreaWrapper", data.indicator);
    });
}

function backToSearch() {
    $("#detailAreaWrapper").hide();
    $("#searchAreaWrapper").show();
}

function showHideDisaggregations(btnSender) {
    if ($(".predefToogleDissagContent").data("visible") == true) {
        $(".predefToogleDissagContent").data("visible", false);
        $(".predefToogleDissagContent").hide();
    } else {
        $(".predefToogleDissagContent").show();
        $(".predefToogleDissagContent").data("visible", true);
    }
}

function showHideTechnicalFields(btnSender) {
    if ($(".predefToogleTechnicalContent").data("visible") == true) {
        $(".predefToogleTechnicalContent").data("visible", false);
        $(".predefToogleTechnicalContent").hide();
    } else {
        $(".predefToogleTechnicalContent").show();
        $(".predefToogleTechnicalContent").data("visible", true);
    }
}

