$(document).ready(function () {

    $(".optionSelectTypeIndicators").kendoDropDownList();

    $(".optionSelectPriorityAreas").kendoDropDownList();

    $("#btnSearchIndicator").click(function (e) {
        e.preventDefault();
        $.ajax({
            url: $(this).data("search-path"),
            data: serializeFilters(this),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {
                // Update PagingFooter Content
                updateGridRowsContent("predefinedIndicator-template", "grdPredefinedIndicator", data.Indicators);

                // Update PagingFooter Content
                updatePagingFooter(data.PageSettings);
            }
        });
    });

});

$(document).on("click", "a.gridPageNumber", function (e) {
    event.preventDefault();

    var filter = {
        'IndicatorType': $("#typeIndicatorId").data("kendoDropDownList").value(),
        'AreaId': $("#priorityArea").data("kendoDropDownList").value(),
        'IndicatorNumber': $("#indicatorNumber").val(),
        'IndicatorName': $("#indicatorName").val(),
        'UnitOfMeasure': $("#unitOfMeasure").val(),
        'Page': $(this).find("span").text()
    };

    $.ajax({
        url: $(this).attr("href"),
        data: JSON.stringify(filter),
        dataType: "json",
        contentType: "application/json",
        type: 'post',
        success: function (data) {

            // Update PagingFooter Content
            updateGridRowsContent("predefinedIndicator-template", "grdPredefinedIndicator", data.Indicators);

            // Update PagingFooter Content
            updatePagingFooter(data.PageSettings);
        }
    });

});

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

function showDetail(predefinedIndicatorId)
{
    $.getJSON($("#hdnGetDetailIndicatorUrl").val(), { predefinedIndicatorId: predefinedIndicatorId }, function (data) {
        console.log(data);
        // Update PagingFooter Content
        updateDetailContent("indicatorDetail-template", "detailAreaWrapper", data.Indicators);
    });
}

function backToSearch() {

}


function linkIndicator() {
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
            url: $("#hdnLinkToCurrentPredfIndUrl").val(),
            type: 'POST',
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(
                linkModel
            ),
            success: function (data) {
                location.href = data.backUrl
            }
        });

    }
}
