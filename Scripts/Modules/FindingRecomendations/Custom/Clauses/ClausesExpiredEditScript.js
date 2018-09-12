$(document).on("ready", function ()
{
    $('#tableSortUIFI001edit').find('.arrow').click();
    

    $(".linkCancel").on("click", function () {
        var route = $(this).attr("data-route");
        location.href = route; 
    }); 

});