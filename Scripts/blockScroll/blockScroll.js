/* bloquea el scroll en la ventana principal al abrir una ventana modal */
var keys = [37, 38, 39, 40];

var blockScroll = function () {
    var enlace = $(".lnkModal"); /* enlace de la ventana modal */
    /* se comprueba la accion sobre el enlace que abre la ventana modal */
    enlace.click(function () {
        $("body").css("overflow", "hidden");
        var cerrar = $(".k-window-action");
        cerrar.click(function () {
            $(".k-window").hover(
                function () {
                    document.onmousewheel = document.onmousedown = wheel;
                    document.onkeydown = keydown;
                },
                function () {
                    document.onmousewheel = document.onmousedown = document.onkeydown = null;
                }
            );
            $("body").css("overflow", "");
            
        });
    });
}

function preventDefault(e) {
    e = e || window.event;
    if (e.preventDefault)
        e.preventDefault();
    e.returnValue = false;
}

function keydown(e) {
    for (var i = keys.length; i--;) {
        if (e.keyCode === keys[i]) {
            preventDefault(e);
            return;
        }
    }
}

function wheel(e) {
    preventDefault(e);
}

function disable_scroll() {
    if (window.addEventListener) {
        window.addEventListener('DOMMouseScroll', wheel, false);
    }
    $(".k-window").hover(
		function () {
		    document.onmousewheel = document.onmousedown = document.onkeydown = null;
		},
		function () {
		    document.onmousewheel = document.onmousedown = wheel;
		    document.onkeydown = keydown;
		}
	);
}

function enable_scroll() {
    if (window.removeEventListener) {
        window.removeEventListener('DOMMouseScroll', wheel, false);
    }
    document.onmousewheel = document.onmousedown = document.onkeydown = null;
}