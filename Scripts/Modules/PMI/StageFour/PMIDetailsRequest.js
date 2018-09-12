
$(document).ready(function () {
    
    $(".ButtonLink").on("click", function () {
        var route = $(this).attr("data-route");
        location.href = route;
    });

    $(".ButtonLinkPMISendRequest").on("click", function () {
        idbg.lockUi(null, true);
        var route = $(this).attr("data-route");
        route += '&additionalValidator=' + $('#validator_list_additional_list').val();
        location.href = route;
    });
});

