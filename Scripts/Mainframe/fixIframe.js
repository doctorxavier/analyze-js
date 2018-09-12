var suggestHeight = 0;
$(document).ready(function () {
    initReziseCloud();
});

function initReziseCloud() {
    resizeIframeCloud();

    $('.inputSearch button, .dropdown button').click(function () {
        setTimeout(function () {
            var droplist = $('.dropdown-menu:visible');
            if (droplist.length > 0) {
                var posDropList = $('.dropdown-menu:visible')[0].getBoundingClientRect().top + 250;
                if (posDropList > window.innerHeight) {
                    suggestHeight = posDropList - window.innerHeight + 20;
                    $('.dropdown-menu:visible').find('li').click(function () {
                        suggestHeight = 0;
                        resizeIframeCloud();
                    });
                }
            } else {
                suggestHeight = 0;
            }
            resizeIframeCloud();
        }, 1000);
    });

    window.addEventListener('resize', function () {
        resizeIframeCloud();
    });
}

function resizeIframeCloud(adequacy, minHeigth) {
    var $frameBase = window.parent.jQuery('#iframeLoader');

    if ($frameBase.length > 0) {
        $frameBase.css("height", '');
    }

    $frameBase = window.parent.jQuery('#mainLayoutOperationContent');

    if ($frameBase.length > 0) {
        $frameBase.css("height", '');
    }

    var $frame = window.parent.jQuery('iframe:eq(0)')
        .not('[data-viewer="true"]')
        .not('.iframeAutoResize');

    if ($frame.length === 0) {
        $frame = $frameBase;
    }

    if (isNaN(adequacy * 1)) {
        adequacy = 0;
    }

    adequacy += suggestHeight;

    minHeigthTemp = screen.availHeight - ($('header.container-fluid').height() + $('footer').height());

    if (isNaN(minHeigth * 1)) {
        minHeigth = minHeigthTemp;
    } else {
        if (minHeigth < minHeigthTemp) {
            minHeigth = minHeigthTemp;
        }
    }

    var heigth = $frame.contents().find('html').height();

    if (heigth === 0)
    {
        heigth = Math.max($(document).height(), $(window).height());
    }

    var value = heigth + adequacy;

    if (value < minHeigth)
    {
        value = minHeigth;
    }

    $frame.css('height',value + 'px')
        .attr('height', value )
       .attr("scrolling", "no");
}

function resizeiframeCloudTimeOut(time) {
    if (isNaN(time * 1)) {
        time = 500;
    }
    setTimeout(function () { resizeIframeCloud(); }, time);
}
