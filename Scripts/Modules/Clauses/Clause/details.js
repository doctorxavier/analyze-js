$(document).ready(function () {

    //dropDownSelect();
    $(".optionSelect").kendoDropDownList();
    $(".datepicker").kendoDatePicker({
        format: "dd MMM yyyy"
    });
    var grid = new GridComponent(".grid", 20, false, true);
    buscarFocus();
    $("input[type='checkbox']").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").text(caption);
    });

    $("input[type='checkbox']:checked").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").addClass("unableMarked").text(caption);
    });

    $("input[type='checkbox']:not(:checked)").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").addClass("disabled").text(caption);
    });

    //modal windows for basic data web page
    $('.lnkModal').click(function () {

        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="k-window-titlebar k-header lineOneClauseTemplate">&nbsp;'+
                    '<span class="k-window-title lineTwoClauseTemplate" id="window1_wnd_title">Clause template</span>'+
                    '<div class="k-window-actions">'+
                        '<a class="k-window-action k-link" href="#" role="button">'+
                            '<span class="k-icon k-i-close" role="presentation">Close</span>'+
                        '</a>'+
                    '</div>'+
                '</div>'+
                '<div class="window clauseAddTemplate windowOpen lineThreeClauseTemplate" id="window1" role="dialog" aria-labelledby="window1_wnd_title"></div>');

        var dialog = $(".lineThreeClauseTemplate").kendoWindow({
            width: "800px",
            title: $(this).data("title"),
            draggable: false,
            resizable: false,
            content: $(this).data("route"),
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
        dialog.center();
        dialog.open();


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
    });

    // edit action for basic data
    $('.edit').click(function () {
        window.location.href = $(this).data("route");
        return false;
    });


    function preventDefault(e) {
        e = e || window.event;
        if (e.preventDefault)
            e.preventDefault();
        e.returnValue = false;
    }

    function keydown(e) {
        for (var i = keys.length; i--;) {
            if (e.keyCode === keys[i]) {
                preventDefault(e);
                return;
            }
        }
    }

    function wheel(e) {
        preventDefault(e);
    }

    function disable_scroll() {
        if (window.addEventListener) {
            window.addEventListener('DOMMouseScroll', wheel, false);
        }
        $(".k-window").hover(
            function () {
                document.onmousewheel = document.onmousedown = document.onkeydown = null;
            },
            function () {
                document.onmousewheel = document.onmousedown = wheel;
                document.onkeydown = keydown;
            }
        );
    }

    function enable_scroll() {
        if (window.removeEventListener) {
            window.removeEventListener('DOMMouseScroll', wheel, false);
        }
        document.onmousewheel = document.onmousedown = document.onkeydown = null;
    }

});