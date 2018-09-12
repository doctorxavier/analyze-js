$(document).on("ready", function ()
{
    $("#filterComment").on("click", function ()
    {
        $("#contentForm").append("<input type='hidden' id='TakeAll' name='TakeAll' value='True'/>");
    });

})