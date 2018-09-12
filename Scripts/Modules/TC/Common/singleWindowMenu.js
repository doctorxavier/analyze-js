
function addOnClickEvent() {
    var leftMenu = $('.izquierda', window.parent.document);

    var showHideButton = $('.glyphicon.glyphicon-play.glyphicon-hide-menu-pointer');

    showHideButton.on('click', function () {
        if (showHideButton.hasClass('glyphicon-hide-menu-rotate')) {
            showHideButton.removeClass('glyphicon-hide-menu-rotate');
            leftMenu.hide();
            leftMenu.attr('wasHidden', true)
            return;
        }
        showHideButton.addClass('glyphicon-hide-menu-rotate');
        leftMenu.show();
        leftMenu.attr('wasHidden', false);
    });

    analyzeMenuStatus(leftMenu, showHideButton);
}

function analyzeMenuStatus(leftMenu, showHideButton) {
    if (leftMenu.attr('wasHidden')) {
        showHideButton.removeClass('glyphicon-hide-menu-rotate');
        return;
    }
    leftMenu.show();
    showHideButton.addClass('glyphicon-hide-menu-rotate');
}
