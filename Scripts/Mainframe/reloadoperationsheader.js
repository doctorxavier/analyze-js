$(document).ready(function () {
    function ReloadOperationsHeader() {
        console.log("method ReloadTheOperationsHeader Init");
        var tabContainer = $('#mainOperationDetailsContainer');
        var url = UrlManager.ReloadOperationDetails;
        $.ajax({
            type: "GET",
            url: url,
            async: true,
            success: function (data) {
                console.log("method ReloadTheOperationsHeader End");
                tabContainer.html(data);
            },
            error: function (res) {
                debugger;
            }
        })
    };
});