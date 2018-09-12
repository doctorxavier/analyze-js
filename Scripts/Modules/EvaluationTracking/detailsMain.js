(function ($) {

    $(document).ready(function () {
        var tables = [];

        if ($(".minimizeTable").length > 0) {
            var minimizeTable = $(".minimizeTable");
            var collapseBtn = new BotonActivo($(".btn-action-group"));

            for (var i = 0; i < $(minimizeTable).length; i++) {
                tables.push(new TableFold(minimizeTable[i]));
            }
        }

        for (var i = 0; i < tables.length; i++) {
            tables[i].foldBtn.click(function () {
                setTimeout(function () {
                    if (changeCollapseBtn(collapseBtn, { tables: tables })) {
                        collapseBtn.switchState(collapseBtn);
                    }
                }, 500);
            });
        }

        if (collapseBtn !== undefined) {
            var collapseBtnActiveText = "Collapse All";
            var collapseBtnInactiveText;
            var collapseBtnInputText = collapseBtn.getBtnInputText();

            if (collapseBtnInputText.toLowerCase() === collapseBtnActiveText.toLowerCase()) {
                collapseBtn.setActiveText(collapseBtnActiveText);
                collapseBtnInactiveText = "Expand All";
                collapseBtn.setInactiveText(collapseBtnInactiveText);
            }

            collapseBtn.btnInput.click(function () {
                massCollapse(collapseBtn, tables);
            });
        }

        $('#lnkShowDisaggregation').bind('click', function () {

            $('.evaluation-tracking-block.Cancelled').show();
            $('#lnkShowDisaggregation').hide();
            $('#lnkHideDisaggregation').removeClass('hidden');
            resizeIframeCloud();

        });

        $('#lnkHideDisaggregation').bind('click', function () {
            $('.evaluation-tracking-block.Cancelled').hide();
            $('#lnkShowDisaggregation').show();
            $('#lnkHideDisaggregation').addClass('hidden');
            resizeIframeCloud();
        });
        $('.evaluation-tracking-block.Cancelled').hide();

        $('.inline-block').has('#documentsEvaluation').css("width", "95%");
    });

    function massCollapse(collapseBtn, tables) {
        if (collapseBtn.isActive()) {
            if (tables !== null) {
                for (var i = 0; i < tables.length; i++) {
                    tables[i].unfold();
                }
            }
        }
        else {
            if (tables !== null) {
                for (var i = 0; i < tables.length; i++) {
                    tables[i].fold();
                }
            }
        }
    }

    function changeCollapseBtn(collapseBtn, tables) {
        if (tables !== null) {
            if (collapseBtn.isActive()) {
                for (var i = 0; i < tables.length; i++) {
                    if (tables[i].isVisible()) {
                        return false;
                    }
                }
                return true;
            }
            else {
                for (var i = 0; i < tables.length; i++) {
                    if (tables[i].isVisible()) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
})(window.jQuery);

