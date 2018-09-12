(function(idbKendoDefaults, $, param) {
    idbKendoDefaults.DataSource = function(readUrl) {
        return {
            transport: {
                read: {
                    url: readUrl,
                    dataType: "json",
                    type: "POST"
                }
            },
            schema: {
                data: "Data",
                total: "Total"

            },
            serverSorting: true,
            serverPaging: true
        };
    };
}(window.idbKendoDefaults = window.idbKendoDefaults || {}, jQuery));