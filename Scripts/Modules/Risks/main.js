/**
 * Risks Module
 * Tue, 12 december 2013
 * eSmart Group
 */

//remap jQuery to $
(function ($) {
    $(document).ready(function () {
        $("#StatusDescription").hide();

        if ($(window.parent.document).find('body').find(".ui-widget-overlay")) {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        }
        if ($(".ui-widget-overlay").length > 0) {
            $(".ui-widget-overlay").remove();
        }
        if ($(".k-window").length > 0) {
            $(".k-window").remove();
        }
        if ($(".k-window-titlebar").length > 0) {
            $(".k-window-titlebar").remove();
        }
        $(document).tooltip({
            items: ".input-validation-error",
            content: function () {
                return $(this).attr('data-val-required');
            }
        });

        $('.prueba').focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        $('#downloadDoc').click(function () {
            redirectPage($(this).data("route"));
        });

        ////KendoDropDownList
        $('#searchBoxContainerRisks').hide();

        //Show/hide Search Box Visualization
        $('#showFilterButtonRisks').on('mousedown', function () {
            $('#searchBoxContainerRisks').toggle();
            resizeIframeCloud();
        });

        //Table sorter for table001 ui_ri_001, ui_ri_001_edit
        $('#tableSortUIRI001, #tableSortUIRI001_edit').tablesorter({
            headers: {
                5: {
                    sorter: false
                }
            }
        });

        //Table sorter for table001 ui_ri_002
        $('#tableSortUIRI002').tablesorter({
            headers: {
                0: {
                    sorter: false
                }
            }
        });

        //Table sorter for table001 ui_ri_003
        $('#tableSortUIRI003').tablesorter({
            headers: {
                2: {
                    sorter: false
                }
            }
        });

        //Table sorter for table001 ui_ri_003_edit
        $('#tableSortUIRI003_edit').tablesorter({
            headers: {
                2: {
                    sorter: false
                }
            }
        });

        //Desplegar Filtro Riesgos
        if (($('#FilterSelectedRiskType').val() != "") || ($('#FilterDescription').val() != "") ||
           ($('#FilterRiskLevel').val() != "") || ($('#FilterRiskTarget').val() != "") ||
           ($('#FilterRiskStatus').val() != "") || ($('#FilterOperationNumber').val() != "")) {
            $('#searchBoxContainerRisks').toggle();
            
        }

        //Remove tr ui_ri_001_edit comment an doc tables
        $(document).on('mousedown', '#ui_ri_001_edit .removeIcon, #ui_ri_001_edit .deleteTextButton', function () {
            var parent = $(this).closest('table');
            $(this).closest('tr').remove();

            
        });

        //Remove tr ui_ri_002_edit comment table
        $(document).on('mousedown', '#ui_ri_004_edit .deleteTextButton', function () {

            var parent = $(this).closest('table');
            $(this).closest('tr').remove();

            var tabla = $(parent).attr('id');
            ReordenarComentarios(tabla);

            resizeIframeCloud();
        });

        //Remove tr ui_ri_002_edit comment table
        $(document).on('mousedown', '#ui_ri_002_edit .deleteTextButton', function () {
            var parent = $(this).closest('table');
            $(this).closest('tr').remove();

            var tabla = $(parent).attr('id');
            ReordenarComentarios(tabla);

            resizeIframeCloud();
        });

        //Reordenamiento
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

        //Remove tr ui_ri_003_edit comment table
        $(document).on('mousedown', '#ui_ri_003_edit .deleteTextButton', function () {
            var parent = $(this).closest('table');
            $(this).closest('tr').remove();

            
        });

        //New comment ui_ri_001_edit
        //$('#newCommentRisks').click(function () {
        //    $.get('../../Scripts/Modules/Risks/templates/newCommentUIRIEdit.html', function (data)
        //    {   
        //        $('#commentTableUIRI001_edit').append(data);
        //        $('.new').removeClass('new');
        //    });

        //    
        //});

        $('#editBtn1').click(function () {
            redirectPage($(this).data("route"));
        });

        $('#editBtn2').click(function () {
            redirectPage($(this).data("route"));
        });

        ////New comment ui_ri_002_edit
        //$('#newCommentRisks003').click(function() {
        //    $.get('../../Scripts/Modules/Risks/templates/newCommentUIRIEdit.html', function (data) {
        //        $('#commentTableUIRI003_edit').append(data);
        //        $('.new').removeClass('new');
        //    });
        //});

        ////New comment ui_ri_002_edit
        //$('#newMitigationMeasureRisks').click(function () {
        //    $.get('../../Scripts/Modules/Risks/Edit.html', function (data) {
        //        $('#tableSortUIRI002_edit').append(data);
        //        $('.new').find('.kendoDropDown').kendoDropDownList();
        //        $('.new').removeClass('new');
        //    });
        //});

        toggleTr();
        

        $('.kendoDropDown').kendoDropDownList();
        $('.optionSelect').kendoDropDownList();

        $(document).on('mousedown', '#ui_ri_004_edit .remove-doc', function () {
            var Route = $(this).val();
            deleteItem(Route);
        });

    });

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
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();
    }

    //Expand or collapse tr containing ralated information(ui_ri_001).
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

            resizeIframeCloud();
        });
    }

})(window.jQuery);


$(window).load(function () {
    $('.filter span .k-dropdown-wrap').find('span').attr("style", "margin-bottom:4%; margin-top:3%; font-family:OpenSans-Italic !important; font-size:1em !important;")
    $('#DescriptionFilter').attr("style", "font-family:OpenSans-Italic !important; font-size:1em !important;")
})
