$(document).on("ready",function ()
{
    

    $(".Btn_Edit").on("click", function () {
        var route = $(this).attr("data-route");
        location.href = route; 
    }); 

});