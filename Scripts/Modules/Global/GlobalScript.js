$(document).on("ready", function ()
{   
    var grid1 = new GridComponent(".grid1", 20, false, true);
    var grid2 = new GridComponent(".grid2", 20, false, true);
    
    loadRouteGenerateLetter();



    $("#Btn_GenerateLetter").on("click", function () {
        idbg.lockUi(null, true);
    });


    $("#ListLanguages").on("change", function () {

        loadRouteGenerateLetter();

    });


    $("#Btn_GenerateLetter").on("click", function ()
    {
        //alert($("#Btn_GenerateLetter").attr('data-route'));
        loadRouteGenerateLetter();
        location.href = $("#Btn_GenerateLetter").attr('data-route');
    })

});


function loadRouteGenerateLetter()
{
    var route = $("#PostLetter").attr("data-route");
    var lang = $("#LanguageLetter option:selected").attr("value");
    var Validator = $("#ValidatorWF option:selected").attr("value");
    route += "&language=" + lang;
    route += "&signer=" + Validator;
    $("#Btn_GenerateLetter").attr('data-route', route);
}