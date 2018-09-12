$(document).ready(function () {

    $("#Save1").on('click', function () {
        if(warningIsOtherPCategoryAndExpired()){
            $("#EditClausePost").submit();
        }
    });

});

$(document).ready(function () {

    $("#Save2").on('click', function () {
        if (warningIsOtherPCategoryAndExpired()) {
            $("#EditClausePost").submit();
        }
    });

});