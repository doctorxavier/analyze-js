$(document).ready(function () {

    $("input[type='checkbox']:checked").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").addClass("unableMarked").text(caption);
    });

    $("input[type='checkbox']:not(:checked)").each(function () {
        var caption = $(this).data("caption");
        $(this).closest("label").children("span").addClass("disabled").text(caption);
    });

    //modal windows for basic data web page
    $(document).on("click", ".lnkModal", function () {
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');

        $("body").append(
            '<div class="contenedor_general layoutDosColsIzda clauses_add_doc"> ' +
            '<div class="k-widget k-window" style="padding-top: 64px; min-width: 90px; min-height: 50px; width: 800px; position: fixed; top: 0px; left: 560px; display: block; opacity: 1; transform: scale(1);">' +
            '<div class="k-window-titlebar k-header" style="margin-top: -64px;">&nbsp;' +
            '<span class="k-window-title" style="right: 60px;" id="window1_wnd_title">' + $(this).data("title") + '</span>' +
            '<div class="k-window-actions"><a class="k-window-action k-link" href="#" role="button"><span class="k-icon k-i-close" role="presentation">Close</span></a>' +
            '</div>' +
            '</div>' +
            '<div class="window k-window-content k-content windowOpen dinamicModal" id="window1" data-role="window" tabindex="0" role="dialog" aria-labelledby="window1_wnd_title" style="overflow: visible;"><div class="loadingdatawarning"><br>Loading...</div></div>' +
            '</div>' +
            '</div>');

        $("body").scrollTop(500);
        $(window.parent.document.body).scrollTop(500);
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
            deactivate: function () {
                $(".contenedor_general").remove();
            },
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
                $(".k-window-titlebar").remove();
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

    $('.cancel').click(function () {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
        $(".k-window-titlebar").remove();
    });

    function preventDefault(e) {
        e = e || window.event;
        if (e.preventDefault) {
            e.preventDefault();
        }
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

    function redirectPage(url) {
        var form = $("#main-form");
        if (url != "")
            form.attr("action", url);
        form.submit();
    };

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

    //modalTabstrips();
    $("#browserDoc").kendoUpload();

    $('#downloadDoc').click(function () {
        var auxiliar = $('#idbNumber').text()
        //$(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        //$(".ui-widget-overlay").remove();
        //$(".k-window").remove();
        //$(".k-window-titlebar").remove();
        redirectPage($(this).data("route") + '?docNum=' + auxiliar);
    });

    var thumbnail = $(".thumbnail");

    if ($(thumbnail).length > 0) {
        $(thumbnail).click(function () {
            if ($(this).hasClass("active")) {
                $(this).removeClass("active");
                $('#docName').text('Document name');
                $('#idbNumber').text('IDB Doc Number');
                $('#date').text('DD MMM YY');
                $('#docID').text('docID');
                $('#docDesc').text('Document Content');
                $('#docAuthor').text('Doc Author');
                $('#opNum').text('12345');
                $('#contractNumber').text('CN-12345');
                $('#downloadDoc').enabled = false;
            } else {
                /* activate thumbnail */
                $(this).addClass("active");
                var testData = $(this).data("docinfo");
                //$('#docName').text(testData.docName);
                $('#docName').text('DOCNAME');
                $('#idbNumber').text(testData.idbNumber);
                $('#date').text(testData.date);
                $('#docID').text(testData.docID);
                $('#docDesc').text(testData.docDesc);
                $('#docAuthor').text(testData.docAuthor);
                $('#opNum').text(testData.opNum);
                $('#contractNumber').text(testData.contractNumber);
                $('#downloadDoc').enabled = true;
            }
        });
    }
});