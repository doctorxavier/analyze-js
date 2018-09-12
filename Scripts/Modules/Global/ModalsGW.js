function bindHandlersModal()
{
    //Add Script Form Modal in GW Tasks
    $('[name=onBehalfOf]').change(function () {
        var value = $("[name=onBehalfOf]");
        var x = value.serializeArray();
        var str = "";
        $.each(x, function (i, field) {
            str += '"' + field.name + '":"' + field.value + '"';
        });
        $("[name=modalTag]").val(str);
    });

    $(".modalOkInfo").bind("click", function () {

        if (validateContainer($('#modalUserRol'))) {
            modalOk($(this));
        } else {
            return false;
        }       
    })
}


