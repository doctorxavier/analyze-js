

function resalta(elEvento) {
    var evento = elEvento || window.event;
    switch (evento.type) {
        case 'mouseover':
            this.style.borderColor = 'black';
            break;
        case 'mouseout':
            this.style.borderColor = 'silver';
            break;
    }
}

window.onload = function () {
    document.getElementById("target").onsubmit = resalta;
}

$(document).ready(function () {
});

function BloquearPantalla() {
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front" style="background: transparent;"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front" style="background: transparent;">' +
                        '<div id="loading_spinner_dialog" style="height:100%; width:100%;">' +
                           '<span class="BlockSaveRequest" style="position: absolute; top: 60%; left: 40%; height: 10%;">' +
                            '<p style="text-align: center;">Procesando, por favor espere...</p>' +
                           '</span>' +
                         '</div>' +
                     '</div>');
    $("body").append('<div class="k-window-titlebar k-header lineOneClauseTemplate" style="background: transparent;">&nbsp;' +
                       '<span class="k-window-title lineTwoClauseTemplate" id="window1_wnd_title">Clause template</span>' +
                         '<div class="k-window-actions">' +
                           '<a class="k-window-action k-link" href="#" role="button">' +
                             '<span class="k-icon k-i-close" role="presentation">Close</span>' +
                           '</a>' +
                         '</div>' +
                       '</div>' +
                     '<div class="window clauseAddTemplate windowOpen lineThreeClauseTemplate" id="window1" role="dialog" aria-labelledby="window1_wnd_title" style="background: transparent;"></div>');
}