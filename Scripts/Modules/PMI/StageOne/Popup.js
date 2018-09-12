$(document).on("click", ".lnkModal", function () {
    event.preventDefault();
    var stringhtml = '<div style="margin:20px;">Please note that to calculate the current Syntethic Indicator and the Classification the system uses different data sources depending on the current PMR Cycle:</div>' +
                       '<p class="headingh3" style="margin:30px 10px 10px 20px;">March PMR Cycle</p>' +
                       '<div style="margin:20px;">The system displays the draft Synthetic Indicator (SI) and the draft classification referring to the period January 1st - December 31st of the <b>previous</b> year </div>' +
                       '<div style="margin:20px; font-family: OpenSans-Italic; font-size: 0.85em;">Note: If the current date is earlier than December 31st, the draft SI and draft Classification refer to the current year (January 1st - Current date)</div>' +
                       '<p class="headingh3" style="margin:30px 10px 10px 20px;">September PMR Cycle</p>' +
                       '<div style="margin:20px;">The system displays the draft Synthetic Indicator (SI) and the draft classification referring to the period January 1st - June 30th of the <b>current</b> year </div>' +
                       '<div style="margin:20px; font-family: OpenSans-Italic; font-size: 0.85em;">Note: If the current date is earlier than December 31st, the draft SI and draft Classification refer to the current year (January 1st - Current date)</div>' +
                       '<div style="display: block; margin:10px; height:30px;"></div>'

    var b = $(window.parent.document).find('body');
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    b.append(
         '<div class="window k-window-content k-content windowOpen dinamicModal" style="z-index:4000;" id="window1" data-role="window" tabindex="0" role="dialog" aria-labelledby="window1_wnd_title" style="overflow: visible;"><div class="loadingdatawarning"><br>Loading...</div></div>' +
         '</div>');
    var dialog = b.find('.dinamicModal').kendoWindow({
        title: $(this).data("title"),
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
            //$(".dinamicModal").parent().css('cssText', 'top:500px !important;');
            var leftP = (screen.width / 2);
            var topP = (screen.height / 2) + (200 / 2);

            var left = "left:" + leftP + "px !important;";
            var top = "top:" + topP + "px !important;";
            var pos = top + left + "width:800px;";
            $('.dinamicModal').parent().css('cssText', pos);
            $('.dinamicModal').parent().detach().appendTo($(window.parent.document).find('body'));
        },
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(window.parent.document).find('body').find(".k-window").remove();
            $(window.parent.document).find('body').find(".k-window-titlebar").remove();
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

