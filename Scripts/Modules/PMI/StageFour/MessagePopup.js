function popUpByCycle(localizationtText, titleText) {
    $(document).on("click", ".lnkModal", function () {
        event.returnValue = false;
        var stringhtml = '<div style="margin:20px;"> ' + localizationtText + ' </div>';

        var b = $(document).find('body');
        $(document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        b.append(
            '<div class="window k-window-content k-content windowOpen dinamicModal" '+
            'style="z-index:4000;" id="window1" data-role="window" tabindex="0" role="dialog"'+
            'aria-labelledby="window1_wnd_title" style="overflow: visible;"><div class="loadingdatawarning">'+
            '<br>Loading...</div></div>' +
            '</div>');
        var dialog = b.find('.dinamicModal').kendoWindow({
            title: titleText,
            draggable: false,
            resizable: false,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            deactivate: function () {
                $(".contenedor_general").remove();
            },
            open: function () {
                var leftP = (screen.width / 2);
                var topP = (screen.height / 2) + (200 / 2);

                var left = "left:" + leftP + "px !important;";
                var top = "top:" + topP + "px !important;";
                var pos = top + left + "width:800px;";
                $('.dinamicModal').parent().css('cssText', pos);
                $('.dinamicModal').parent().detach().appendTo($(document).find('body'));
            },
            close: function () {
                $(document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(document).find('body').find(".k-window").remove();
                $(document).find('body').find(".k-window-titlebar").remove();
                //location.reload(true);
            }
        }).data("kendoWindow");
        dialog.open();
        dialog.content($.parseHTML(stringhtml)),
        $(document.getElementsByName($(this).attr("name"))[0]).addClass($(this).attr("name"));

        $("body").css("overflow", "hidden");
        var cerrar = $(".k-window-action");
        cerrar.click(function () {
            $(".k-window").hover(
                function () {
                    document.onmousewheel = document.onmousedown = wheel;
                    document.onkeydown = keydown;
                },
                function () {
                    document.onmousewheel = document.onmousedown = document.onkeydown = null;
                }
            );
            $("body").css("overflow", "");
            
        });

        modalTabstrips();
    });
}