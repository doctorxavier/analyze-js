(function (a) {
    a.fn.validateText = function (b) {
        a(this).on({
            keypress: function (a) {
                var c = a.which,
                    d = a.keyCode,
                    e = String.fromCharCode(c).toLowerCase(),
                    f = b;

                (-1 != f.indexOf(e)
                || 9 == d
                || 37 != c && 37 == d
                || 39 == d && 39 != c
                || 8 == d
                || 46 == d && 46 != c) && 161 != c
                || (event.preventDefault
                    ? event.preventDefault()
                    : event.returnValue = false)
            }
        })
    }
})(jQuery);

$(document).on("ready", function () {
    if ($(".numberOnly").validateText != null) {
        $(".numberOnly").validateText("0123456789");

        $(".numberOnlyAll").validateText("-0123456789");

        $(".textOnly").validateText("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        $(".decimalCharacthers").validateText("0123456789,.");

        $(".typeDatepicker").validateText("");
    }
});
