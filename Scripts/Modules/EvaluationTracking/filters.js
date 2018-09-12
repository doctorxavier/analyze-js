(function ($) {

    function reloadStageAjax() {
        var route = $('#MethodologyCurrentFilter').attr('data-route');
        $.ajax({
            url: route,
            data: $("#FormEvaluationTracking").serialize(),
            type: 'POST',
            dataType: "JSON",
            success: function (resp) {
                var combobox = $("#Stage").data("kendoDropDownList");
                var updatedOptions = new Array();
                $.each(resp, function (index, type) {
                    updatedOptions.push({ text: type.Name, value: type.ConvergenceMasterDataId });
                });
                combobox.dataSource.data(updatedOptions);
                combobox.dataSource.query();
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });
    }

})(window.jQuery);