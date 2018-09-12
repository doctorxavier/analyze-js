var CurrentAchievementDelay = 0;
var CurrentOtherDelay = 0;

$(document).on("ready", function () {

    $('#tableSortUIFI004-1').find('.arrow').click();
    $('#tableSortUIFI004-2').find('.arrow').click();

    
    NumberOfCurrentAchievementDelay();
    NumberOfCurrentOtherDelay()

    $('.LinkCancel').click(function () {
        redirectPage($(this).data("route"));
    });

    $("#button_ui_fi_004_1_edit").on("click", function () {
        CurrentAchievementDelay = CurrentAchievementDelay = CurrentAchievementDelay + 1;
        addRowTableSortUIFI004_1();
    });

    $("#button_ui_fi_004_2_edit").on("click", function () {
        CurrentOtherDelay = CurrentOtherDelay = CurrentOtherDelay + 1;
        addRowTableSortUIFI004_2();
    });


    jQuery.validator.addMethod("itemDelayReasonOther", function (value, element) {
        if (value.toString() != "") {
            if ($(element).val().toString().trim().length == 0) {
                return false;
            }
        }
        else {
            return false;
        }
        return true;
    });


    jQuery.validator.addMethod("itemDelayNameOther", function (value, element) {
        if (value.toString() != "") {
            if ($(element).val().toString().trim().length == 0) {
                return false;
            }
        }
        else {
            return false;
        }
        return true;
    });

    $(document).tooltip({
        items: ".input-validation-error",
        content: function () {
            if ($(this).attr('data-val-required'))
                return $(this).attr('data-val-required');
            if ($(this).attr('data-val-date'))
                return $(this).attr('data-val-date');
            if ($(this).attr('data-val-number'))
                return $(this).attr('data-val-number');
            if ($(this).attr('data-val-range'))
                return $(this).attr('data-val-range');
        }
    });
    
    LoadEvents()
});


function LoadEvents()
{
    $(".ListTypeDelay select").on("change", function () {
        var ID = $(this).attr("id");
        var indexSelected = $(this).attr("data-typeindex");
        var ListToChange = $("select[data-nameindex|=" + indexSelected + "]").attr("id");
        var route = null;

        
        if ($("option:selected", this).text() == $("#OutputSelected").val())
        {
            route = $('#LinkLoadOutputs').attr('data-route');
        }
        else if ($("option:selected", this).text() == $("#OutcomeSelected").val()) {
            route = $('#LinkLoadOutcomes').attr('data-route');
        }
        else
        {
            route = $('#LinkLoadOutputs').attr('data-route');
        }

        if (route != null) {
            LoadDelayName(route, ListToChange);
            route = null;
        }
        else {
            alert("No master data Code fount")
        }

    });

}

function LoadDelayName(route, ListToChange)
{
    $("option", "#" + ListToChange).remove();
    $("#" + ListToChange).kendoDropDownList();

    $.ajax({
        url: route,
        data: $("#FormDelaysEdit").serialize(),
        type: 'Post',
        dataType: "json",
        success: function (resp)
        {
            var LoadOptions = "";
            for (var i = 0; i < resp.length; i++)
            {
                if (resp[i].Key == 0) {
                    LoadOptions += "<option value='" + resp[i].Key + "'>" + resp[i].Value + "</option>";
                }
                else
                {
                    LoadOptions += "<option value='" + resp[i].Value + "'>" + resp[i].Value + "</option>";
                }
            }
            $("#" + ListToChange).append(LoadOptions);
            $("#" + ListToChange).kendoDropDownList();
        },
        error: function (e, err, erro)
        {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        }
    });
}

function NumberOfCurrentAchievementDelay()
{
    CurrentAchievementDelay = CurrentAchievementDelay = $("#tableSortUIFI004-1 tbody tr td table").length -1;
}

function NumberOfCurrentOtherDelay() {
    CurrentOtherDelay = $("#tableSortUIFI004-2 tbody tr td table").length;
}

function addRowTableSortUIFI004_1()
{
    var CurrentDate = new Date();
    var DateCurrent = $.datepicker.formatDate('dd M yy', CurrentDate);

    var html = '<tr class="new itemtrRowAchivement" data-indexComponentRowAchivement=' + CurrentAchievementDelay + '>';
    html += '<td colspan="4">';
    html += '<table class="w100 innerGrid">';
    html += '<tr>';
    html += '<td><select class="kendoDropDown ListTypeDelay newListTypeDelay itemDelayTypeId" style="width: 140px;" name="AchievementDelays[' + CurrentAchievementDelay + '].DelayTypeId" id="AchievementDelays_' + CurrentAchievementDelay + '__DelayTypeId" data-TypeIndex =' + CurrentAchievementDelay + '></select></td>';
    html += '<td style="padding-left:15px;">';
    html += '<select class="kendoDropDown itemResultElementName" style="width: 400px;" name="AchievementDelays[' + CurrentAchievementDelay + '].ResultElementName" id="AchievementDelays_' + CurrentAchievementDelay + '__ResultElementName" data-NameIndex=' + CurrentAchievementDelay + '></select>';
    html += '</td>';
    html += '<td>';
    html += '<select class="kendoDropDown itemIsSolved" name="AchievementDelays[' + CurrentAchievementDelay + '].IsSolved" id="AchievementDelays_' + CurrentAchievementDelay + '__IsSolved">';
    html += '<option value="True" selected>Yes</option>';
    html += '<option value="False">No</option>';
    html += '</select>';
    html += '</td>';
    html += '<td>';
    html += '<div class="inline-block w82"> ' + DateCurrent + ' </div>';
    html += '</td>';
    html += '<td>';
    html += '<input type="button" class="trClosest inline-block-fix cursorPointer closeAchievementDelays" data-itemClose=' + CurrentAchievementDelay + ' onclick="DeleteAchivement(this)"/>';
    html += '<input type="button" class="noBorder inline-block operationButton arrow cursorPointer" />';
    html += '</td>';
    html += '</tr>';
    html += '<tr>';
    html += '<td colspan="5">';
    html += '<div class="inline-block w30 marginRight3">';
    html += '<h6 class="boldFont verticalMargin10 marginH6Custom">Explain both physical and financial delays</h6>';
    html += '<div class="verticalMargin20"><textarea id="AchievementDelays_' + CurrentAchievementDelay + '__DelayReason" name="AchievementDelays[' + CurrentAchievementDelay + '].DelayReason" class="input normal w94 itemDelayReason"></textarea></div>';
    html += '</div>';
    html += '<div class="inline-block w30 marginRight3">';
    html += '<h6 class="boldFont verticalMargin10">Findings: Effects or implications (negative or positive) of the delays</h6>';
    html += '<div class="verticalMargin20"><textarea id="AchievementDelays_' + CurrentAchievementDelay + '__Finding" name="AchievementDelays[' + CurrentAchievementDelay + '].Finding" class="input normal w94 itemFinding"></textarea></div>';
    html += '</div>';
    html += '<div class="inline-block w33">';
    html += '<h6 class="boldFont verticalMargin10">Recomendations: Actions or decisions recomend to replicate or to void</h6>';
    html += '<div class="verticalMargin20"><textarea id="AchievementDelays_' + CurrentAchievementDelay + '__Recommendation" name="AchievementDelays[' + CurrentAchievementDelay + '].Recommendation" class="input normal w94 itemRecommendation"></textarea></div>';
    html += '</div>';
    html += '</td>';
    html += '</tr>';
    html += '</table>';
    html += '</td>';
    html += '</tr>';
    $('#tableSortUIFI004-1').append(html);

    if (CurrentAchievementDelay == 0)
    {   
        var ListNewList = '#AchievementDelays_' + CurrentAchievementDelay + '__DelayTypeId';
        var ruta = $("#LinkFirsDelayType").attr("data-route"); 


        $.ajax({
            url: ruta,
            data: $("#FormDelaysEdit").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp)
            {   
                var LoadOptions = "";
                for (var i = 0; i < resp.length; i++)
                {
                    LoadOptions += "<option value='" + resp[i].ConvergenceMasterDataId + "'>" + resp[i].Name + "</option>";
                }
                $(ListNewList).append(LoadOptions);

                $('.new').find('.kendoDropDown').kendoDropDownList();
                $('.new').find('.arrow').click();
                $('.new').removeClass('new');
                LoadEvents();

                LoadChangeNameDelay();


                

            },
            error: function (e, err, erro)
            {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });



    }
    else
    {
        var count = 0; 
        var html = "";
        $(".ListTypeDelay:eq(1) option").each(function (index, element)
        {
            if (count == 0)
            {
                html += "<option value=" + $(element).val() + " selected>" + $(element).text() + "</option>"
                count++;
            }
            else
            {
                html += "<option value=" + $(element).val() + ">" + $(element).text() + "</option>"
            }
            
        });

        $(".newListTypeDelay").append(html);

        $('.new').find('.kendoDropDown').kendoDropDownList();
        $('.new').find('.arrow').click();
        $('.new').removeClass('new');
        LoadEvents();

        LoadChangeNameDelay();


        
    }
}

function LoadChangeNameDelay()
{
    var newItem = $(".newListTypeDelay select").attr("id");
    newItem = ("#" + newItem);

    var indexSelected = $(newItem).attr("data-typeindex");

    var ListToChange = $("select[data-nameindex|=" + indexSelected + "]").attr("id");

    var route = null;

    if ($("option:selected", newItem).text() == "Output delay") {
        route = $('#LinkLoadOutputs').attr('data-route');
    }
    else if ($("option:selected", newItem).text() == "Outcome delay") {
        route = $('#LinkLoadOutcomes').attr('data-route');
    }
    else {
        route = $('#LinkLoadOutputs').attr('data-route');
    }

    if (route != null) {
        LoadDelayName(route, ListToChange);
    }
    else {
        alert("rote not fount")
    }

    $('.newListTypeDelay').removeClass('newListTypeDelay');
}

function addRowTableSortUIFI004_2() {
    var CurrentDate = new Date();
    var DateCurrent = $.datepicker.formatDate('dd M yy', CurrentDate);

    NumberOfCurrentOtherDelay();
    
    var html = '<tr class="new itemRowOther" data-indexComponentRowAOther=' + CurrentOtherDelay + ' >';
    html += '<td colspan="4">';
    html += '<table class="w100 innerGrid">';
    html += '<tr>';
    html += '<td>Other Delays</td>';
    html += '<td><input class="grayBorder textboxCustom w97-2 centerAlign itemDelayNameOther" id="OtherDelays_' + CurrentOtherDelay + '__DelayName" name="OtherDelays[' + CurrentOtherDelay + '].DelayName" placeholder="Other Delay Name" type="text"  data-val="true" data-val-required="Please, complete the mandatory fields" ></td>';
    html += '<td>';
    html += '<select class="kendoDropDown itemIsSolvedOther" id="OtherDelays_' + CurrentOtherDelay + '__IsSolved" name="OtherDelays[' + CurrentOtherDelay + '].IsSolved">';
    html += '<option value="True" selected>Yes</option>';
    html += '<option value="False">No</option>';
    html += '</select>';
    html += '</td>';
    html += '<td>';
    html += '<div class="inline-block w82"> ' + DateCurrent + ' </div>';
    html += '</td>';
    html += '<td>';
    html += '<input type="button" class="trClosest inline-block-fix cursorPointer closeOtherDelays" data-itemCloseOther=' + CurrentOtherDelay + ' onclick="DeleteOthers(this)"/>';
    html += '<input type="button" class="noBorder inline-block operationButton arrow cursorPointer" />';
    html += '</td>';
    html += '</tr>';
    html += '<tr>';
    html += '<td colspan="5">';
    html += '<div>';
    html += '<h6 class="boldFont verticalMargin10">Reason</h6>';
    html += '<div class="verticalMargin20"><textarea class="input normal w94 itemDelayReasonOther" cols="55" id="OtherDelays_' + CurrentOtherDelay + '__DelayReason" name="OtherDelays[' + CurrentOtherDelay + '].DelayReason" placeholder="Other Delay Reason" data-val="true" data-val-required="Please, complete the mandatory fields"></textarea></div>';
    html += '</div>';
    html += '</td>';
    html += '</tr>';
    html += '</table>';
    html += '</td>';
    html += '</tr>';
    $('#tableSortUIFI004-2').append(html);
    $('.new').find('.kendoDropDown').kendoDropDownList();
    $('.new').find('.arrow').click();
    $('.new').removeClass('new');





    
}

function DeleteAchivement(MyRow) {

    var valueselected = $(MyRow).attr("data-itemClose");

    if (valueselected != null) {
        var ID_Deleted = $("input[data-valueachievementdelay|=" + valueselected + "]").attr("value");


        if ($(MyRow).hasClass('originalItemClose')) {

            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div id="content" class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div><div style="padding-top: 25px;" >' + $("#messageDelete").attr("value") + '</<div><div style="padding-top: 25px;" ></<div><hr style="width:100%; Color:#406BB5"/><div class="inline-block rightAlign" style="padding: 5px; width:98%; border-radius:5px;"><a id="lnkCancelDeleteIndicator" title="Cancel" class="cancel" id="lnkCancelDeleteIndicator" style="margin:0px; padding-left: 612px; padding-top:10px;" href="javascript:void(0)">' + $("#CancelText").attr("value") + '</a><input id="btnSaveDeleteIndicator" type="submit" class="button blueButton" value=' + $("#DeleteText").attr("value") + '></<div></div>');

            var route = $("#WarningMessageURL").attr("data-route");
            var title = "Warning";
            var dialog = $(".dinamicModal").kendoWindow({
                width: "800px",
                title: title,
                draggable: false,
                resizable: false,
                content: route,
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

            $("#lnkCancelDeleteIndicator").click(function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(document).find('body').find(".k-overlay").remove();
                $(".k-window").remove();
            });

            $("#btnSaveDeleteIndicator").click(function () {
                var state = false;

                $.ajax({
                    url: $("#DeleteAchivementPost").attr("data-route") + "?DeleteAchivementID=" + ID_Deleted + "",
                    type: 'Post',
                    dataType: "json",
                    success: function (resp) {
                        if (resp == "success") {
                            DeleteSuccessAchivement(valueselected);
                        }

                    },
                    error: function (e, err, erro) {
                        alert('Error: ' + e + ' - ' + err + ' - ' + erro);
                    },
                    complete: function () {

                    }
                });

            });
        }
        else
        {
            OnlyReCreateIndexAndDeleteTemp(valueselected);
        }
    }
}

function IndexAchivement() {
    NumberOfCurrentAchievementDelay();

    $(".originalItemClose").each(function (index, element) {
        $(element).attr("id", "closeItem_" + index);
        $(element).attr("data-itemClose", index);
    });

    $(".itemtrRowAchivement").each(function (index, element) {
        $(element).attr("data-indexcomponentrowachivement", index);
    });

    $(".itemAchievementDelayId").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__AchievementDelayId");
        $(element).attr("name", "AchievementDelays[" + index + "].AchievementDelayId");
        $(element).attr("data-valueAchievementDelay", index);
    });

    $(".itemDelayTypeId select").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__DelayTypeId");
        $(element).attr("name", "AchievementDelays[" + index + "].DelayTypeId");
        $(element).attr("data-typeindex", index);

    });

    $(".itemResultElementName select").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__ResultElementName");
        $(element).attr("name", "AchievementDelays[" + index + "].ResultElementName");
        $(element).attr("data-nameindex", index);
    });

    $(".itemIsSolved select").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__IsSolved");
        $(element).attr("name", "AchievementDelays[" + index + "].IsSolved");
    });

    $(".itemLastUpdate").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__LastUpdate");
        $(element).attr("name", "AchievementDelays[" + index + "].LastUpdate");
    });

    $(".itemDelayReason").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__DelayReason");
        $(element).attr("name", "AchievementDelays[" + index + "].DelayReason");
    });

    $(".itemFinding").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__Finding");
        $(element).attr("name", "AchievementDelays[" + index + "].Finding");
    });

    $(".itemRecommendation").each(function (index, element) {
        $(element).attr("id", "AchievementDelays_" + index + "__Recommendation");
        $(element).attr("name", "AchievementDelays[" + index + "].Recommendation");
    });
}

function DeleteSuccessAchivement(valueselected) {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(document).find('body').find(".k-overlay").remove();
    $(".k-window").remove();
    $("tr[data-indexComponentRowAchivement|=" + valueselected + "]").remove();
    IndexAchivement();
}

function OnlyReCreateIndexAndDeleteTemp(valueselected) {
    $("tr[data-indexComponentRowAchivement|=" + valueselected + "]").remove();
    IndexAchivement();
}

function DeleteOthers(MyRow) {
    var valueselected = $(MyRow).attr("data-itemCloseOther");

    if (valueselected != null)
    {
        var ID_Deleted = $("input[data-valueotherdelay|=" + valueselected + "]").attr("value");

          
            if (ID_Deleted) {

                if (!$(this).hasClass("originalOtherDelayclass")) {
                    
                    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
                    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
                    $("body").append('<div id="content" class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div><div style="padding-top: 25px;" >' + $("#messageDelete").attr("value") + '</<div><div style="padding-top: 25px;" ></<div><hr style="width:100%; Color:#406BB5"/><div class="inline-block rightAlign" style="padding: 5px; width:98%; border-radius:5px;"><a id="lnkCancelDeleteIndicator" title="Cancel" class="cancel" id="lnkCancelDeleteIndicator" style="margin:0px; padding-left: 612px; padding-top:10px;" href="javascript:void(0)">' + $("#CancelText").attr("value") + '</a><input id="btnSaveDeleteIndicator" type="submit" class="button blueButton" value=' + $("#DeleteText").attr("value") + '></<div></div>');

                    var route = $("#WarningMessageURL").attr("data-route");
                    var title = "Warning";
                    var dialog = $(".dinamicModal").kendoWindow({
                        width: "800px",
                        title: title,
                        draggable: false,
                        resizable: false,
                        content: route,
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

                    $("#lnkCancelDeleteIndicator").click(function () {
                        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                        $(".ui-widget-overlay").remove();
                        $(document).find('body').find(".k-overlay").remove();
                        $(".k-window").remove();
                    });

                    $("#btnSaveDeleteIndicator").click(function () {
                        var state = false;

                        $.ajax({
                            url: $("#DeleteOtherPost").attr("data-route") + "?DeleteOtherID=" + ID_Deleted + "",
                            type: 'Post',
                            dataType: "json",
                            success: function (resp) {
                                if (resp == "success") {
                                    RemoveWindow();
                                    RemoveRowOther(valueselected);
                                }
                                else {
                                    alert("error");
                                }
                            },
                            error: function (e, err, erro) {
                                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
                            }
                        });

                    });
                }
                

            }
            else
            {
                
                RemoveRowOther(valueselected);
            }

            

    }
}

function RemoveWindow()
{
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(document).find('body').find(".k-overlay").remove();
    $(".k-window").remove();
}

function RemoveRowOther(valueselected)
{
    $("tr[data-indexComponentRowAOther|=" + valueselected + "]").remove();
    IndexOther();
}

function IndexOther()
{

    $(".itemRowOther").each(function (index, element) {
        $(element).attr("data-indexComponentRowAOther", index);
    });

    $(".originalOtherDelay").each(function (index, element) {
        $(element).attr("id", "OriginalOtherDelay_" + index);
        $(element).attr("data-itemCloseOther", index);
    });

    $(".ItemOtherDelayId").each(function (index, element) {
        $(element).attr("id", "OtherDelays_" + index + "__OtherDelayId");
        $(element).attr("name", "OtherDelays[" + index + "].OtherDelayId");
        $(element).attr("data-valueotherdelay", index);
    });

    $(".itemDelayNameOther").each(function (index, element) {
        $(element).attr("id", "OtherDelays_" + index + "__DelayName");
        $(element).attr("name", "OtherDelays[" + index + "].DelayName");
        $(element).attr("data-valueotherdelay", index);
    });

    $(".itemIsSolvedOther").each(function (index, element) {
        $(element).attr("id", "OtherDelays_" + index + "__IsSolved");
        $(element).attr("name", "OtherDelays[" + index + "].IsSolved");
    });

    $(".itemLastUpdateOther").each(function (index, element) {
        $(element).attr("id", "OtherDelays_" + index + "__LastUpdate");
        $(element).attr("name", "OtherDelays[" + index + "].LastUpdate");
    });

    $(".itemDelayReasonOther").each(function (index, element) {
        $(element).attr("id", "OtherDelays_" + index + "__DelayReason");
        $(element).attr("name", "OtherDelays[" + index + "].DelayReason");
    });
}




