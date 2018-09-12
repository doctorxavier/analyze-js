$('.deleteTextButton').click(function () {
    var Id = $(this).attr('id');
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

    var dialog = $(".dinamicModal").kendoWindow({
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
