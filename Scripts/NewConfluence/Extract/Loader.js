var loadingPanelNew = null;
var loadingPanelNewOptinal = null;

$(document).ready(function () {
    loadingPanelNew = $('#layoutLoadingDiv');
    loadingPanelNewOptinal = $('#layoutLoadingOptionalDiv');
});

function showLoader(text) {
    setTimeout(function () {
        if (loadingPanelNew.length > 0) {
            loadingPanelNew.show();
            if (text !== undefined) {
                var textZone = loadingPanelNew.find('.confluence-loader-text');
                if (textZone.length > 0) {
                    textZone.text(text);
                }
            }
        }
    }, 100);
}

function showLoaderOptional(text) {
    setTimeout(function () {
        if (loadingPanelNewOptinal.length > 0) {

            if (loadingPanelNew !== null) {
                loadingPanelNew.hide();
            }

            loadingPanelNewOptinal.show();
            if (text !== undefined) {
                var textZone = loadingPanelNewOptinal.find('.confluence-loader-text');
                if (textZone.length > 0) {
                    textZone.text(text);
                }
            }
        }
    }, 100);
}

function hideLoader() {
    setTimeout(function () {
        if (loadingPanelNew !== null) {
            loadingPanelNew.fadeOut('slow');
        }
    }, 100);
}

function hideLoaderOptional() {
    setTimeout(function () {
        if (loadingPanelNewOptinal.length > 0) {
            loadingPanelNewOptinal.fadeOut('slow');
        }
    }, 100);
}