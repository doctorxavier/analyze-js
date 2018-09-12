(function($) {
        $(document).ready(function() {
            
            $('.tabSelector').on('click', function () {
                var newSelectedElement = $(this);
                var selectedElement = $('#tabId').val();
                preventEventDefaultBehavior(event);
                if (newSelectedElement.attr('id') === selectedElement) {
                    return;
                }
                
                var selected = newSelectedElement.attr("id");
                $('#tabId').val(selected);
                $("#tabForm").submit();
                showLoaderOptional();
            });

            $('input#Filter\\.Take').on("change", function (element) {
                var pageSize = $('input#Filter\\.Take').val();
                $("#Filter_Take").val(pageSize);
                $("#contentForm").submit();
            });

            $("#localDocumentsTable").paginationConfluense($('input#Filter\\.Take').val());

            resizeIframeCloud();
        });

        function preventEventDefaultBehavior(e) {
            e.preventDefault ? e.preventDefault() : (e.returnValue = false);
        }
    }
)(window.jQuery);