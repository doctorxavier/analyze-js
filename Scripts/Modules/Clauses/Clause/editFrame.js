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
    navigators($frame, minHeigth, adequacy);
}

function navigators($frame, minHeigth, adequacy) {

    navigator.sayswho = (function () {
        var ua = navigator.userAgent, tem,
        M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
        if (/trident/i.test(M[1])) {
            tem = /\brv[ :]+(\d+)/g.exec(ua) || [];
            return 'IE ' + (tem[1] || '');
        }
        if (M[1] === 'Chrome') {
            tem = ua.match(/\b(OPR|Edge)\/(\d+)/);
            if (tem !== null) return tem.slice(1).join(' ').replace('OPR', 'Opera');
        }
        M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];
        if ((tem = ua.match(/version\/(\d+)/i)) !== null) M.splice(1, 1, tem[1]);
        return M.join(' ');
    })();

    var browser = navigator.sayswho;
    var heigth = $frame.contents().find('html').height();
    var ie11 = 'IE 11';
    if (heigth === 0)
    {
        heigth = Math.max($(document).height(), $(window).height());
    }

    var value = heigth + adequacy;

    if (browser === ie11)
    {
        if (value < minHeigth) {
            value = minHeigth + minHeigth;
        }
    }
    else
    {
        if (value < minHeigth) {
            value = minHeigth;
        }
    
    }

    $frame.css('height',value + 'px')
        .attr('height', value )
       .attr("scrolling", "yes");
}

function resizeiframeCloudTimeOut(time) {
    if (isNaN(time * 1)) {
        time = 500;
    }
    setTimeout(function () { resizeIframeCloud(); }, time);
}