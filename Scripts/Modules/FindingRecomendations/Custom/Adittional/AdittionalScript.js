var FindingCount = 0; 

$(document).on("ready", function ()
{
    $('#tableUIFI007Edit').find('.arrow').click();

    
    CountFinding(); 

    $("#Btn_AgregateNewFinding").on("click", function ()
    {
        FindingCount = FindingCount + 1;
        NewRowTableUIFI007Edit(); 
    });


    $(".linkCancel").on("click", function ()
    {
        var route = $(this).attr("data-route"); 
        location.href = route; 
    });

});

function LoadCategoriesFilter(MyObject)
{   
    var indexSelected = $(MyObject).attr("data-indexdimension");
    var ListLoad = $("select[data-indexCategory|=" + indexSelected + "]").attr("id");
    ListLoad = "#" + ListLoad;
    var route = $("#LinkRouteCategoryFilter").attr("data-route");

    var item = $(MyObject).attr("id");
    item = "#" + item; 
    var FilterValue = $(item).val();

    FilterCategories(FilterValue, ListLoad, route);
}

function IndexComponents()
{

    $(".TrCloseItem").each(function (index, element) {
        $(element).attr("data-indexComponentRowAdittional", index);
    });

    $(".itemCloseFinding").each(function (index, element) {
        $(element).attr("data-index", index);
    });

    $(".itemStage select").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__StageId");
        $(element).attr("name", "FindingRecommendations[" + index + "].StageId");
    });

    $(".itemDimension select").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__DimensionId");
        $(element).attr("name", "FindingRecommendations[" + index + "].DimensionId");
        $(element).attr("data-indexDimension", index);
    });

    $(".itemCategory select").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__CategoryID");
        $(element).attr("name", "FindingRecommendations[" + index + "].CategoryID");
        $(element).attr("data-indexCategory", index);
    });

    $(".itemLastUpdate").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__LastUpdate");
        $(element).attr("name", "FindingRecommendations[" + index + "].LastUpdate");
    });

    $(".itemFindingID").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__FindingRecommendationId");
        $(element).attr("name", "FindingRecommendations[" + index + "].FindingRecommendationId");
        $(element).attr("data-indexMain", index);
    });

    $(".itemFinding").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__Finding");
        $(element).attr("name", "FindingRecommendations[" + index + "].Finding");
    });

    $(".itemRecommendation").each(function (index, element) {
        $(element).attr("id", "FindingRecommendations_" + index + "__Recommendation");
        $(element).attr("name", "FindingRecommendations[" + index + "].Recommendation");
    });

    CountFinding();

}

function CountFinding()
{
    FindingCount = $("#tableUIFI007Edit tbody tr td table").length - 1;
}

function NewRowTableUIFI007Edit()
{
    CountFinding();

    FindingCount = FindingCount + 1;

    var index = FindingCount;

    var CurrentDate = new Date();
    var DateCurrent = $.datepicker.formatDate('dd M yy', CurrentDate); 

    var html = '<tr class="new TrCloseItem" data-indexComponentRowAdittional=' + index + '>';
    html += '<td colspan="8">';
    html += '<table class="w100 innerGrid">';
    html += '<tr>';
    html += '<td style="width: 270px; padding-left: 5px;">';
    html += '<select id="FindingRecommendations_' + index + '__StageId" name="FindingRecommendations[' + index + '].StageId" class="kendoDropDown listBoxCustomWidth itemStage newItemStage">';
    html += '</select>';
    html += '</td>';
    html += '<td style="width: 270px">';
    html += '<select id="FindingRecommendations_' + index + '__DimensionId" name="FindingRecommendations[' + index + '].DimensionId" class="kendoDropDown listBoxCustomWidth itemDimension newItemDimension" data-indexDimension=' + index + ' onchange="LoadCategoriesFilter(this)" >';
    html += '</select>';
    html += '</td>';
    html += '<td style="width: 270px">';
    html += '<select id="FindingRecommendations_' + index + '__CategoryID" name="FindingRecommendations[' + index + '].CategoryID" class="kendoDropDown listBoxCustomWidth itemCategory" data-indexCategory=' + index + '>';
    html += '</select>';
    html += '</td>';
    html += '<td style="width: 190px">' + DateCurrent + '</td>';
    html += '<td style="width: 40px">';
    html += '<input type="button" class="trClosest cursorPointer closeFinding itemCloseFinding" data-index=' + index + ' onclick="deleteRow(this)" />';
    html += '<input type="button" class="noBorder operationButton arrow cursorPointer" />';
    html += '</td>';
    html += '</tr>';
    html += '<tr>';
    html += '<td colspan="8">';
    html += '<div class="padding10">';
    html += '<div class="inline-block w48 leftAlign">';
    html += '<h6 class="boldFont verticalMargin10">Findings</h6>';
    html += '<textarea rows="3" placeholder="Explain finding" class="itemFinding" id="FindingRecommendations_' + index + '__Finding" name="FindingRecommendations[' + index + '].Finding"></textarea>';
    html += '</div>';
    html += '<div class="inline-block w48">';
    html += '<h6 class="boldFont verticalMargin10">' + $('#Recommendationstext').val() + '</h6>';
    html += '<textarea rows="3" placeholder="Propose recomendation" class="itemRecommendation" id="FindingRecommendations_' + index + '__Recommendation"  name="FindingRecommendations[' + index + '].Recommendation"></textarea>';
    html += '</div>';
    html += '</div>';
    html += '</td>';
    html += '</tr>';
    html += '</table>';
    html += '</td>';
    html += '</tr>';

    $("#tableUIFI007Edit").append(html);

    


    if (FindingCount == 0)
    {
        var NewList = '#FindingRecommendations_' + FindingCount + '__StageId';
        var Ruta = $("#LinkRouteStage").attr("data-route");

        $.ajax({
            url: Ruta,
            type: 'Post',
            dataType: "json",
            success: function (resp) {
                var LoadOptions = "";
                for (var i = 0; i < resp.length; i++) {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
                $(NewList).append(LoadOptions);

                LoadDimensions(); 
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });
    }
    else
    {

        var countStage = 0;
        var optionsStage = "";
        $("#FindingRecommendations_0__StageId option").each(function (index, element) {
            if (countStage == 0) {
                optionsStage += "<option value=" + $(element).val() + " selected>" + $(element).text() + "</option>"
                countStage++;
            }
            else {
                optionsStage += "<option value=" + $(element).val() + ">" + $(element).text() + "</option>"
            }

        });
        $(".newItemStage").append(optionsStage);

        var countDimension = 0;
        var optionsDimension = "";
        $(".itemDimension:eq(1) option").each(function (index, element) {
            if (countDimension == 0) {
                optionsDimension += "<option value=" + $(element).val() + " selected>" + $(element).text() + "</option>"
                countDimension = countStage++;
            }
            else {
                optionsDimension += "<option value=" + $(element).val() + ">" + $(element).text() + "</option>"
            }

        });
        $(".newItemDimension").append(optionsDimension);


        $('.new').find('.kendoDropDown').kendoDropDownList();
        $('.new').find('.arrow').click();
        $('.new').removeClass('new');

        LoadCategories();

    }
}

function LoadDimensions()
{

    var NewList = '#FindingRecommendations_' + FindingCount + '__DimensionId';
    var Ruta = $("#LinkRouteDimensions").attr("data-route");


    $.ajax({
        url: Ruta,
        type: 'Post',
        dataType: "json",
        success: function (resp) {
            var LoadOptions = "";
            for (var i = 0; i < resp.length; i++)
            {
                if (i == 0) {
                    LoadOptions += "<option  value='" + resp[i].ConvergenceMasterDataId + "' selected>" + resp[i].Name + "</option>";
                }
                else
                {
                    LoadOptions += "<option  value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
            }
            $(NewList).append(LoadOptions);

            LoadCategories();
        },
        error: function (e, err, erro) {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        }
    });
}

function LoadCategories()
{
    var newItem = $("#FindingRecommendations_" + FindingCount + "__DimensionId");
    
    var indexItemSelected = $(newItem).attr("data-indexDimension");
    var newListCategories = $("select[data-indexCategory|=" + indexItemSelected + "]").attr("id");
    newListCategories = "#" + newListCategories;

    var route = $("#LinkRouteCategoryFilter").attr("data-route");

    var FilterValue = $(newItem).val();

    FilterCategories(FilterValue, newListCategories, route);


    $('.new').find('.kendoDropDown').kendoDropDownList();
    $('.new').find('.arrow').click();
    $('.new').removeClass('new');
    $(".newItemDimension").removeClass("newItemDimension");
    $(".newItemStage").removeClass("newItemStage");

}

function FilterCategories(FilterValue, ListLoad, route)
{
    FilterValue = parseInt(FilterValue);

    $("option", ListLoad).remove();
    $.ajax({
        url: route,
        data: { ValueSelected: FilterValue },
        type: 'Post',
        dataType: "json",
        success: function (resp) {
            var LoadOptions = "";
            for (var i = 0; i < resp.length; i++) {
                LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
            }
            $(ListLoad).append(LoadOptions);
            $(ListLoad).kendoDropDownList();
        },
        error: function (e, err, erro) {
            /**
            * Jira ID: CON-796
            * Fix Description: the alert was commented because it was firing an error alert although the funcionality was ok
            * Attended by: Jhon Astaiza 
            * Date: 06/09/2014
            * Begin Fix
            **/
            // alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            /**
            * End Fix
            **/
        }
    });
}


