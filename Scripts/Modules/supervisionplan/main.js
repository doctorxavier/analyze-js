(function ($) {

    $(document).ready(function () {

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

        
        $('.kendoDropDown').kendoDropDownList();

        
        $('.kendoDatePicker').kendoDatePicker();

        $('.decimal-edit-field').keypress(function (event) {
            if ((!keyCodeIsNumber(event.which)
                && !keyCodeIsDecimalDelimiter(event.which)
                && !isSpecialKey(event.which))
                || keyCodeIsDecimalDelimiter(event.which)
                && $(this).val().toLowerCase().indexOf(".") > -1) {
                event.preventDefault();
                return;
            }

            if (event.which == 44) {
                $(this).val($(this).val() + ".");
                event.preventDefault();
            }
        });

        $('.yearSelector').on('click', function (event) {
            event.preventDefault();

            $('.plan-row').removeClass('selected');
            $(this).closest('.plan-row').addClass('selected');

            var planId = $(this).closest('tr').data('planId');
            var actualYear = $('#main-form').find('#Year');
            var year = $(this).closest('tr').data('year');
            $('#main-form').find('#Year').val(year);
            $('#save-form').find('#Year').val(year);
            if (actualYear != year) {
                var isInEdit = $("#IsInEdit").val().toLowerCase() === 'true';
                if (!isInEdit) {
                    $('#SelectedPlanId').val(planId);
                    $('#TitleYear').text(" " + year);
                    var buttonsRoute = $(this).closest('tr').data('buttonsRoute');

                    $.get(buttonsRoute, function (data) {
                        $('.buttonsPanel').html(data);
                    });
                }
                $(".inline-block.tabItem.active").click();
            }
        });

        $('.newIndicator').on('click', function (event) {
            event.preventDefault();
            var isInEdit = $("#IsInEdit").val().toLowerCase() === 'true';
            if (isInEdit) {
                saveWarningOnTabChange();
                return;
            }
            $('#main-form').find('#Year').val($(this).data('yearToCreate'));
            $('#main-form').find('#CreateNew').val(true);
            redirectPage($(this).data('route'));
        });

        $('#tableUISP003 > tbody > tr > td :input.decimal-edit-field').on("blur", function () {
            var categoryId = $(this).data("categoryId");
            var sourceId = $(this).data("sourceId");
            var totalRow = 0.0, totalCol = 0.0, total = 0.0;
            var tBody = $("#tableUISP003 > tbody");
            var colElements = tBody.find('.decimal-edit-field[data-source-id="' + sourceId + '"]');
            var rowElements = tBody.find('.decimal-edit-field[data-category-id="' + categoryId + '"]');

            colElements.each(function () {
                totalCol += parseFloat($(this).val());
            });

            rowElements.each(function () {
                totalRow += parseFloat($(this).val());
            });
            $(".decimal-edit-field").each(function () {
                total += parseFloat($(this).val());
            });

            $('#tableUISP003 > tbody > tr > td.total-row[data-category-id="' + categoryId + '"]')
                .text(totalRow.toFixed(2));
            $('#tableUISP003 > tbody > tr > td.total-col[data-source-id="' + sourceId + '"]')
                .text(totalCol.toFixed(2));
            $("#total-budget").text(total.toFixed(2));
        });

        $('.integer-edit-field').keypress(function (event) {
            if (!keyCodeIsNumber(event.which) && !isSpecialKey(event.which)) {
                event.preventDefault();
                return;
            }
        });

        function redirectPage(url) {
            var form = $("#main-form");
            if (url != "")
                form.attr("action", url);
            form.submit();
        };

        $(".inline-block.tabItem").click(function () {
            var isInEdit = $("#IsInEdit").val().toLowerCase() === 'true';
            var planId = $('#SelectedPlanId').val();
            if (isInEdit) {
                saveWarningOnTabChange();
            }
            else if (planId != -1) {
                $(".inline-block.tabItem").removeClass("active");
                $(this).addClass("active");
                var ajaxContainer = $("#loadingContainerSP");
                var panel = $("#SupervisionPlanPanel");
                panel.addClass("loading-overlay");
                ajaxContainer.css("display", "");
                kendo.ui.progress(ajaxContainer, true);

                var url = $(this).data("route") + '?id=' + planId;
                

                $('#SupervisionPlanPanel').load(url, function () {
                    kendo.ui.progress(ajaxContainer, false);
                    panel.removeClass("loading-overlay");
                    ajaxContainer.css("display", "none");
                    
                    $('html, body').stop().animate({
                        scrollTop: $('#TitleYear').offset().top
                    }, 'slow');
                });

            }
        });

        $(".prior-version-btn").on('click', function (event) {
            event.preventDefault();
            var buttonsRoute = $(this).data('buttonsRoute');                      
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
            $("#modalWindow").appendTo(".dinamicModal").removeClass("hide");
            var title = $(this).data("windowtitle");
            var modal = $(".dinamicModal").kendoWindow({
                width: "800px",
                title: title,
                draggable: false,
                resizable: false,
                pinned: true,
                actions: [
                    "Close"
                ],
                modal: true,
                visible: false,
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $("#modalWindow").appendTo("#ui_sp_001").addClass("hide");
                    $("body").find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("popUpHeaderBackground padding20 relative");
            $.get(buttonsRoute, function (data) {
                modal.content(data);
            });
            modal.refresh();
            modal.center();
            modal.open();
        });

        var saveWarningOnTabChange = function (event) {
            //event.preventDefault();
            if (typeof event != 'undefined') {
                event.preventDefault();
            }
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
            var title = $("#IsInEdit").data("windowTitle");
            var modal = $(".dinamicModal").kendoWindow({
                width: "800px",
                position: { top: 100 },
                title: title,
                draggable: false,
                resizable: false,
                pinned: false,
                content: $("#IsInEdit").data("routeSaveWarning"),
                actions: ["Close"],
                modal: true,
                visible: false,
                activate: function () {
                    $("#ConfirmSaveWarning").click(functionCancel);
                },
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $("body").find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
            modal.open();
        };

        $(document).on('click', ".ac-EditPlan", function (event) {
            event.preventDefault();
            $(".view-selector").val($(".inline-block.tabItem.active").data("id"));
            redirectPage($(this).data("route"));
        });

        $(document).on('click', ".ac-SavePlan", function (event) {
            event.preventDefault();

            var form = $("#save-form")
                .removeData("validator") 
                .removeData("unobtrusiveValidation"); 
            $.validator.unobtrusive.parse(form);

            prepareSave();
            $("#ReturnToEdit").val("false");
            if ($("#save-form").valid()) {
                $("#save-form").submit();
            } else {
                $("#save-form .input-validation-error").first().focus();
            }
        });

        $(document).on('click', ".ac-SaveRequestPlan", function (event) {
            event.preventDefault();

            var form = $("#save-form")
           .removeData("validator") 
           .removeData("unobtrusiveValidation");  
            $.validator.unobtrusive.parse(form);

            prepareSave();
            $("#ReturnToEdit").val("false");
            $("#RequestAfterSave").val("true");
            if ($("#save-form").valid()) {
                $("#save-form").submit();
            }
            else {
                $("#save-form .input-validation-error").first().focus();
            }
        });

        $(document).on('click', '.ac-RequestPlan', function (event) {
            event.preventDefault();
            $('#main-form').find('#PlanVersionId').val($(this).data('idPlanVersion'));
            redirectPage($(this).data("route"));
        });

        $(document).on('click', ".ac-CancelEdit", function (event) {
            event.preventDefault();
            $(".view-selector").val($(".inline-block.tabItem.active").data("id"));
            redirectPage($(this).data("route"));
        });

        $(document).on('click', ".ac-DeletePlan", function (event) {
            event.preventDefault();
            var year = $(this).closest('tr').data("year");
            var versionId = $(this).data('versionId');
            var title = $(this).data('windowTitle');
            var warningRoute = $(this).data('routeDeleteWarning');
            var actionRoute = $(this).data("route");

            deleteWarning(year, versionId, title, warningRoute, actionRoute);
        });

        var deleteWarning = function (year, versionId, title, warningRoute, actionRoute) {
            $('#main-form').find('#Year').val(year);
            $('#main-form').find('#PlanVersionId').val(versionId);
            $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="ui-widget-overlay ui-front"></div>');
            $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
            var modal = $(".dinamicModal").kendoWindow({
                width: "800px",
                position: { top: 100 },
                title: title,
                draggable: false,
                resizable: false,
                pinned: false,
                content: warningRoute,
                actions: ["Close"],
                modal: true,
                visible: false,
                activate: function () {
                    $("#CancelDelete").click(functionCancel);
                    $("#ConfirmDelete").click(function () {
                        $('#main-form').find('#Year').val(year);
                        $('#main-form').find('#PlanVersionId').val(versionId);
                        redirectPage(actionRoute);
                        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                        $("body").find(".ui-widget-overlay").remove();
                    });
                },
                close: function () {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $("body").find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();
                }
            }).data("kendoWindow");
            $(".k-window-titlebar").addClass("warning");
            $(".k-window-title").addClass("ico_warning");
            modal.open();
        };

        $(document).on('click', ".ac-ModifyPlan", function (event) {
            event.preventDefault();
            $('#Year').val($(this).closest('tr').data("year"));
            $(".view-selector").val($(".inline-block.tabItem.active").data("id"));
            $("#ReturnToEdit").val("true");
            $('#PlanVersionId').val($(this).data('planVersionId'));
            redirectPage($(this).data("route"));
        });

        var functionCancel = function (event) {
            event.preventDefault();
            var modal = $(".dinamicModal").data("kendoWindow");
            modal.close();
            $(".ui-widget-overlay").remove();
        };

        var deleteRowProduct = function (event) {
            event.preventDefault();
            var row = $(this).parent().parent();
            row.remove();
        };

        var deleteActivity = function (event) {
            event.preventDefault();
            var activity = $(this).closest(".activityComponent");
            activity.remove();
            reCountFields("#ActivitiesPanel", "activity");
        };

        var deleteComment = function (event) {
            event.preventDefault();
            var comment = $(this).closest(".commentComponent");
            comment.remove();
        };

        var prepareSave = function () {
            var viewId = $(".inline-block.tabItem.active").data("id"); 
            switch (viewId) {
                case "CriticalProducts":
                    reCountFields("#CriticalProductEditableRows", "criticalproduct");
                    activateProductFields();
                    break;
                case "Activities":
                    reCountFields("#ActivitiesPanel", "activity");
                    break;
                case "Budget":
                    activateBudgetFields();
                    break;
                case "Comments":
                    reCountFields("#CommentsPanel", "comment");
            }
            $(".view-selector").val(viewId);
        };

        var reCountFields = function (container, view) {
            var rows = $(container);

            for (var i = 0; i < rows.children().length; i++) {
                var element = $(rows.children()[i]);

                var elems = element.find('[data-view="' + view + '"], [data-view="document"]');
                for (var j = 0; j < elems.length; j++) {
                    var name = $(elems[j]).attr("name");
                    if (name) {
                    name = name.replace(/\[\d+\]/i, "[" + i + "]");
                    $(elems[j]).attr("name", name);
                }
            }
            }
        };

        var reCountDocuments = function (table) {
            for (var i = 0; i < table.children().length; i++) {
                var element = $(table.children()[i]);
                var elems = element.find('[data-view="document"]');
                for (var j = 0; j < elems.length; j++) {
                    var name = $(elems[j]).attr("name");
                    name = name.replace(/\ActivityDocuments\[\d+\]/i, "ActivityDocuments[" + i + "]");
                    $(elems[j]).attr("name", name);
                }
            }
        };

        var activateProductFields = function () {
            $('[data-view="criticalproduct"]').removeAttr("disabled");
        };

        var activateBudgetFields = function () {
            $('#BudgetPanel').find('input[type=text]').removeAttr("disabled");
        };

        var setActivitiesCount = function () {
            var activities = $(".activity-counter");
            for (var i = 0; i < activities.length; i++) {
                $(activities[i]).text(i + 1);
            }
        };

        $(document).on('click', "#newProduct", function (event) {
            event.preventDefault();
            var template = $("#CriticalProductEditTemplate").children(".row").clone();
            var rows = $("#CriticalProductEditableRows");
            template.html(template.html().replace(/#rowCount#/g, 0));
            $(template).find('select').addClass('kendoDropDown');
            $(template).find('.kendoDropDown').kendoDropDownList();
            $(template).find('.desc-critical-product1')
                .attr("data-val-required", "Please, complete the mandatory fields");
            $(template).find('.desc-critical-product1')
                .attr("data-val", "true");
            $(template).find('.desc-critical-product1')
                .attr("class", "desc-critical-product input-validation-error");
            rows.append(template);

            var form = $("#save-form")
           .removeData("validator") 
           .removeData("unobtrusiveValidation");  
            $.validator.unobtrusive.parse(form);
            setActivitiesCount();
            
        });

        $(document).on('click', "#newComment", function (event) {
            event.preventDefault();
            var template = $("#CommentTemplate").children(".commentComponent").clone();
            var content = $("#CommentsPanel");
            template.html(template.html().replace(/#rowCount#/g, 0));
            $(template).find('.txtDescriptionComent')
                .attr("data-val-required", "Please, complete the mandatory fields");
            $(template).find('.txtDescriptionComent')
                .attr("data-val", "true");
            $(template).find('.txtDescriptionComent')
                .attr("class", "desc-comment input-validation-error w100");
            content.append(template);

            var form = $("#save-form")
                .removeData("validator") 
                .removeData("unobtrusiveValidation");  
            $.validator.unobtrusive.parse(form);

            
        });

        $(document).on('click', "#newActivity", function (event) {
            event.preventDefault();
            var template = $("#ActivityEditTemplate").children(".activityComponent").clone();
            var content = $("#ActivitiesPanel");
            template.html(template.html().replace(/#rowCount#/g, 0));
            $(template).find('select').addClass('kendoDropDown');
            $(template).find('.kendoDropDown').kendoDropDownList();
            content.append(template);
            setActivitiesCount();

            
        });

        $(document).on("click", ".remove-doc", function (event) {
            event.preventDefault();
            var tr = $(this).closest("tr");
            var docsTable = $(this).closest("tbody");
            tr.remove();
            reCountDocuments(docsTable);
        });

        $(document).on("click", ".download-doc", function (event) {
            event.preventDefault();
            window.open($(this).data("route"));
        });

        $(document).on('click', "input[type=button].deleteCommentBtn", deleteComment);
        $(document).on('click', "input[type=button].deleteProductBtn", deleteRowProduct);
        $(document).on('click', "input[type=button].deleteActivityBtn", deleteActivity);

        toggleActivities();
        $(".desc-critical-product").validate();
        
    });

    function toggleActivities() {
        $(document).on('mousedown', '.minMaxButton', function () {
            if ($(this).hasClass('plus')) {
                $(this).removeClass('plus');
                $(this).parent('div').parent('div').siblings().slideUp(100);
            } else {
                $(this).addClass('plus');
                $(this).parent('div').parent('div').siblings().slideDown(100);
            }
        });
    }

    function keyCodeIsNumber(keyCode) {
        return (keyCode >= 48 && keyCode <= 57);
    }

    function keyCodeIsDecimalDelimiter(keyCode) {
        return (keyCode == 46 || keyCode == 44);
    }

    function isSpecialKey(keyCode) {
        return (keyCode == 0 || keyCode == 9 || keyCode == 8);
    }


})(window.jQuery);

$(document).on("click", ".pdfDownloadIcon, .excelDownloadIcon", function (event) {
    event.preventDefault();
    $.ajax({
        url: $(this).data("route"),        
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0) {
                parent.location.href = resp;
            }
            else {
                alert(resp);
            }
        },
        error: function (e, err, erro) {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        }
    });
});

$(document).ready(function () {
    var yearSelection = function () {
        $(".yearSelector").click(function (e) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: $('#TitleYear').offset().top
            }, 1000);
        });
    };

    yearSelection();
});

function closePendingChanges() {
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $("body").find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();
}

