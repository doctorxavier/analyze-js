/**
 * Finding Recomendations Module
 * Thu, 14 November 2013
 * eSmart Group
 */

//remap jQuery to $
(function ($) {
    $(document).ready(function () {
        
        $('#Clauses').click(function () {
            location.href = $(this).data("route");
        });

        $('#Matrixchanges').click(function ()
        {
            console.log($(this).data("route")); 
            location.href = $(this).data("route");
            console.log($(this).data("route"));
        });

        $('#Delays').click(function () {
            location.href = $(this).data("route");
        });

        $('#Risk').click(function () {
            location.href = $(this).data("route");
        });

        $('#Additional').click(function () {
            location.href = $(this).data("route");
        });

        //Table Sortable(ui_fi_001)
        $('#tableSortUIFI001, #tableSortUIFI001edit').tablesorter({
            headers: {
                7: {
                    sorter: false
                }
            }
        });

        //Table Sortable(ui_fi_005)
        $('#tableSortUIFI005').tablesorter({
            headers: {
                7: {
                    sorter: false
                }
            }
        });

        //Table Sortable(ui_fi_006)
        $('#tableSortUIFI006').tablesorter({
            headers: {
                0: {
                    sorter: false
                }
            }
        });

        $(".kendoDropDown").kendoDropDownList({
            enabled: true
        });

        toggleTr();
        //trClosest();
        collapseButtonToggle();
        //addRowTableUIFI007Edit();
        //addRowTableSortUIFI004_1();
        //addRowTableSortUIFI004_2();

        $('.optionSelect').kendoDropDownList();
        $(".datepicker").kendoDatePicker({
            format: "dd MMM yyyy"
        });




        //$(".editFandR").click(function (e) {
        //        showNotification("warning", $("#hdnNotificationPath").val());
        //        e.preventDefault();
           
        //});

    });

    //Expand or collapse tr containing ralated information(ui_fi_005).
    function toggleTr() {
        $(document).on('click', '.arrow', function () {
            if ($(this).closest('tr').hasClass('expanded')) {
                $(this).closest('tr').removeClass('expanded');
                $(this).closest('tr').addClass('collapsed');
                $(this).closest('tr').next('tr').toggle(0);
            } else {
                $(this).closest('tr').addClass('expanded');
                $(this).closest('tr').removeClass('collapsed');
                $(this).closest('tr').next('tr').toggle(0);
            }
        });
    }

    //Expand or collapse all tr containing ralated information(ui_fi_001).
    function collapseButtonToggle() {
        $('#collapseControlButton').click(function () {
            if (!$(this).hasClass('allExpanded')) {
                $('table.grid tbody').children('tr').each(function () {
                    if ($(this).find('tr').hasClass('expanded')) {
                        $(this).find('.arrow').click();
                    }
                });
                $('.innerGrid .arrow').click();
                $(this).val('Collapse All');
                $(this).addClass('allExpanded');
                $(this).closest('.collapseControl').addClass('greenBackground').addClass('right');
            } else {
                $('table.grid tbody').children('tr:first-child').each(function () {
                    if ($(this).hasClass('collapsed')) {
                        $(this).find('.arrow').click();
                    }
                });
                $('.innerGrid .arrow').click();
                $(this).val('Expand All');
                $(this).removeClass('allExpanded');
                $(this).closest('.collapseControl').removeClass('greenBackground').removeClass('right');
            }
        });
    }


    // Delete Html tr
    function trClosest() {
        $(document).on('mousedown', '.trClosest', function () {
            $(this).closest('tr').parent().closest('tr').remove();
        });
    }

})(window.jQuery);

//Aregar combo Stages en fila de vista edit:additional
function DropDownStages(index) {

    var StagesList = $("#listStages").val();

    var htmlContent = "";

    htmlContent += '<fieldset>';
    htmlContent += '<legend class="hide">Dropdown con opciones</legend>';
    htmlContent += '<ul class="optionList">';
    htmlContent += '<li class="small">';

    htmlContent += '<span class="k-widget k-dropdown k-header kendoDropDown" unselectable="on" role="listbox" aria-haspopup="true" aria-expanded="false" tabindex="0" aria-owns="" aria-disabled="false" aria-readonly="false" aria-busy="false" aria-activedescendant="" style="">';
    htmlContent += '<span unselectable="on" class="k-dropdown-wrap k-state-default k-state-hover">';
    htmlContent += '<span unselectable="on" class="k-input"></span>';
    htmlContent += '<span unselectable="on" class="k-select">';
    htmlContent += '<span unselectable="on" class="k-icon k-i-arrow-s">select</span>';
    htmlContent += '</span>';
    htmlContent += '</span>';
    htmlContent += '<select class="StageskendoDropDown" data-val="true" data-val-required="The StageId field is required." id="FindingRecommendations_' + index + '__StageId" name="FindingRecommendations[' + index + '].StageId" data-role="dropdownlist" style="display: none;">';

    var count = 0;
    $.each($.parseJSON(StagesList), function (index, Stages) {
        if (count == 0) {
            htmlContent += "<option value='" + Stages.Item1 + "' selected = 'selected'>" + Stages.Item2 + "</option>";
        } else {
            htmlContent += "<option value='" + Stages.Item1 + "'>" + Stages.Item2 + "</option>";
        }
        count++;
    });

    htmlContent += '</select>';
    htmlContent += '</span>';
    htmlContent += '</li>';
    htmlContent += '</ul>';
    htmlContent += '</fieldset>';

    return htmlContent;
}

//Agregar combo Dimensions en fila de vista edit:additional
function DropDownDimensions(index) {
    var DimensionsList = $("#listDimensions").val();

    var htmlContent = "";

    htmlContent += '<fieldset>';
    htmlContent += '<legend class="hide">Dropdown con opciones</legend>';
    htmlContent += '<ul class="optionList">';
    htmlContent += '<li class="small">';

    htmlContent += '<span class="k-widget k-dropdown k-header kendoDropDown" unselectable="on" role="listbox" aria-haspopup="true" aria-expanded="false" tabindex="0" aria-owns="" aria-disabled="false" aria-readonly="false" aria-busy="false" aria-activedescendant="" style="">';
    htmlContent += '<span unselectable="on" class="k-dropdown-wrap k-state-default k-state-hover">';
    htmlContent += '<span unselectable="on" class="k-input"></span>';
    htmlContent += '<span unselectable="on" class="k-select">';
    htmlContent += '<span unselectable="on" class="k-icon k-i-arrow-s">select</span>';
    htmlContent += '</span>';
    htmlContent += '</span>';
    htmlContent += '<select class="DimensionskendoDropDown" data-val="true" data-val-required="The DimensionId field is required." id="FindingRecommendations_' + index + '__DimensionId" name="FindingRecommendations[' + index + '].DimensionId" data-role="dropdownlist" style="display: none;">';

    var count = 0;
    $.each($.parseJSON(DimensionsList), function (index, Dimensions) {
        if (count == 0) {
            htmlContent += "<option value='" + Dimensions.Item1 + "' selected = 'selected'>" + Dimensions.Item2 + "</option>";
        } else {
            htmlContent += "<option value='" + Dimensions.Item1 + "'>" + Dimensions.Item2 + "</option>";
        }
        count++;
    });

    htmlContent += '</select>';
    htmlContent += '</span>';
    htmlContent += '</li>';
    htmlContent += '</ul>';
    htmlContent += '</fieldset>';

    return htmlContent;
}

//Reordenamiento de Id en grilla de vista edit:additional
function ReordenarComentarios(Commenttable) {

    var ope = $('table#' + Commenttable + ' tbody tr:last').index();
    var Comments = $('table#' + Commenttable + ' tbody');
    var rowCount = 0;

    $(Comments).find('tr').each(function (index, tr) {

        var CommentsId = $(this).find('[id$="__UserCommentId"]');
        var Modifiedby = $(this).find('[id$="__ModifiedBy"]');
        var Modified = $(this).find('[id$="__Modified"]');
        var Text = $(this).find('[id$="__Text"]');

        if ($(CommentsId).val() != null) {
            $(CommentsId).attr("id", "UserComment_" + rowCount + "__UserCommentId");
            $(CommentsId).attr("name", "UserComment[" + rowCount + "].UserCommentId");
        }

        if ($(Modifiedby).val() != null) {
            $(Modifiedby).attr("id", "UserComment_" + rowCount + "__ModifiedBy");
            $(Modifiedby).attr("name", "UserComment[" + rowCount + "].ModifiedBy");
        }

        if ($(Modified).val() != null) {
            $(Modified).attr("id", "UserComment_" + rowCount + "__Modified");
            $(Modified).attr("name", "UserComment[" + rowCount + "].Modified");
        }

        if ($(Text).val() != null) {
            $(Text).attr("id", "UserComment_" + rowCount + "__Text");
            $(Text).attr("name", "UserComment[" + rowCount + "].Text");
        }

        rowCount++;
    });

}

//function showNotification(type, path) {

//    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
//    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
//    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading ...</div></div>');
//    var title = "Warning";
//    var dialog = $(".dinamicModal").kendoWindow({
//        width: "800px",
//        title: title,
//        draggable: false,
//        resizable: false,
//        content: path,
//        pinned: true,
//        actions: [
//            "Close"
//        ],
//        modal: true,
//        visible: false,
//        close: function () {
//            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
//            $(".ui-widget-overlay").remove();
//            $(".k-window").remove();
//        }
//    }).data("kendoWindow");

//    $(".k-window-titlebar").addClass("warning");
//    $(".k-window-title").addClass("ico_warning");
//    dialog.open();

//}