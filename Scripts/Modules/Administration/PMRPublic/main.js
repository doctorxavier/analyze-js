(function ($) {

    $(document).ready(function () {
        $('.kendoDropDown').kendoDropDownList();

        buildTable();

        $('#filterButtonPMRPublic').on('click', function () {

            var url = $(this).attr("data-url");

            model = $('#FormPMRPublic').serialize();

            callFilterRequest(url, model, model);
        });
    });

})(window.jQuery);

function callFilterRequest(route, data, model) {
    idbg.lockUi(null, true);
    $.ajax({
        url: route,
        type: "POST",
        cache: false,
        async: true,
        data: data,
        success: function (result) {
            idbg.lockUi(null, false);
            $('#idpartial').html(result);
            $('#idpartial').data('requestFilter', model);

            buildTable();
            
        },
        error: function (e, err, erro) {
            idbg.lockUi(null, false);
            alert('ERROR: Something went wrong during the operation.' +
                'Please, refer to the log for further detail.');
        }
    });
}

function buildTable() {
    var grid = $("#operationsResults").kendoGrid({
        dataSource: {
            pageSize: 50
        },
        scrollable: false,
        sortable: {
            allowUnsort: false
        },
        pageable: true,
        dataBound: function (e) {
            $(".k-pager-nav").hide();
        }
    }).data("kendoGrid");
}