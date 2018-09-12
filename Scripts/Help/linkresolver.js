var ConvergenceHelp = new Object();

ConvergenceHelp.URLS = {
    English: "",
    Spanish: "",
    Portuguese: "",
    French: ""
};

ConvergenceHelp.Conts = {
    EN: "",
    ES: "",
    PT: "",
    FR: ""
};

ConvergenceHelp.ViewCode = "";
ConvergenceHelp.InitializeURL = "";
ConvergenceHelp.HasPermision = false;

ConvergenceHelp['$'] = function (selector) {

    try {
        if (typeof frames['someFrame'] !== undefined) {
            return frames['someFrame'].window.$(selector);
        }
    }
    catch (ex) {

        return $(selector);
    }
};

ConvergenceHelp['Add'] = function (data) {
    this.URLS[data.ViewCode.toUpperCase()] = {
        English: data.UrlEn,
        Spanish: data.UrlEs,
        Portuguese: data.UrlPt,
        French: data.UrlFr,
    }
};

ConvergenceHelp['Change'] = function (viewCode) {
    var $helpIcon = $('#headerHelp');
    var $helpTitle = this.$('a#mainIconHelp');
    var url = this.Find(viewCode);

    $helpIcon.off('click', openModal);

    if (url === null || url === undefined || url === "") {
        if ($helpTitle !== undefined) {
            $helpTitle.attr('href', 'javascript:void(0)');
            $helpTitle.addClass('hide');
        }

        $helpIcon.removeClass('headerHelpAdd headerHelpDisabled');

        if (this.HasPermision && viewCode !== "" && url !== "") {
            $helpIcon.on('click', openModal);
            $helpIcon.removeAttr('href');
            $helpIcon.addClass('headerHelpAdd');
            $helpIcon.parent().css('cursor', 'pointer');

            return;
        }

        $helpIcon.attr('href', 'javascript:void(0)');
        $helpIcon.addClass('headerHelpDisabled');
        $helpIcon.parent().css('cursor', 'not-allowed');

        return;
    }

    if ($helpTitle.length > 0) {
        $helpTitle.attr('href', url);
        $helpTitle.removeClass('hide');
    }
    else {
        var mainTitle =
            this.$(':header:visible:not([class="new-modal-title"]):first');

        if (mainTitle !== undefined) {
            $helpTitle = this.CreateIconHelp(url).appendTo(mainTitle);
        }
    }

    $helpIcon.attr('href', url)
    $helpIcon.removeClass('headerHelpDisabled headerHelpAdd');
    $helpIcon.parent().css('cursor', 'pointer');

    return;
};

ConvergenceHelp['ChangeByTabs'] = function (viewCode) {
    var $ulTabs = this.$('ul.tabs li.active');
    var $divTabs = this.$('div.tabsContainer div.tabSelector.active');
    var tabViewCode = null;

    if ($ulTabs.length > 0) {
        tabViewCode = viewCode + "/" + $ulTabs.attr('dd-tab').replace("#", "");
    }
    else if ($divTabs.length > 0) {
        tabViewCode = viewCode + "/" + $divTabs.attr('id');
    }

    ConvergenceHelp.Change(tabViewCode);
};

ConvergenceHelp['CreateIconHelp'] = function (url) {

    return $("<a />")
        .attr({
            id: 'mainIconHelp',
            href: url,
            target: '_blank'
        }).css(this.IconStyle);
};

ConvergenceHelp['Find'] = function (key) {
    key = key.toUpperCase();

    if (this.URLS[key] === undefined) {
        return null;
    }

    var match = document.cookie.match(/userLan\=(\w+)/);

    if (match) {
        var language = match[1].toUpperCase();
        var url = "";

        switch (language) {
            case this.Conts.EN:
                return this.URLS[key].English;
            case this.Conts.ES:
                return this.URLS[key].Spanish;
            case this.Conts.PT:
                return this.URLS[key].Portuguese;
            case this.Conts.FR:
                return this.URLS[key].French;
            default:
                return this.URLS[key].English;
        }
    }

    return this.URLS[key].English;
};

ConvergenceHelp['IconStyle'] = {
    'background-image': 'url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABze' +
        'nr0AAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH4QQYDwgzDbQXFAAAA2pJREFUWMPtl0toU2kUx3+59' +
        'yYmaXMT26SVRmMraNtxGPHRInSEDoKgoMXH7ArufG2yUtHFgIthpLuMIg3qwIBuRJnpSBVxUDf11WJx6MxYG' +
        'NFaY8dpLLVJH0nbfLPwmtAk96EVdOGBLG7u+c7/f875zuPCZ/nIYnsX5ViYBqAFaAJqAb/2Kg70A11ARzBC9' +
        'wclEAuzGTgENFu0ewtoC0a4Om8CsTDtwN73jHA0GGHfexGIhVGBjmJeS74QrnWtyIEVICnMjDxhoq8TBu/qR' +
        'aMlGGGs2EvFgFxRcGfDbtTNx5BKA0gON0IIxEwKT9M+Rnou8vq3w7gzc7CaNVvfWI6AXtjttZsI7L+my1gIw' +
        'cifN/i3fSteeRLJZp4OSefCFYDbFpSysPWc8YWy2Sj/ciPpNXsYSxf4tlezbUxAu+0FYg81ongCWU//uXuFM' +
        '63VnN7hoffCD0xPJrO6SzbuYWBMxoptqUidFy012b+cqdGXJF/FGPrrDvd/PkLl9ACN5Ulmrx1l/PHtrG7Zk' +
        'joGE0Wz26xh6F7CFr3wTt6J8vT3KINjMD0LqgIhL/jdIFXU4/YFsrqp5CjjaaFnqgVyjSqfQJNRjstdsNCph' +
        'c4GigRyRR3qzh9xLF6d1Xvw60mcckbPTJNRBGqNCCh5N0ZZvBZ1RwTnspzNvuvn6Lt0nGpVl0CtEQG/1RYnV' +
        '65E3XkSZ8367H8Pr5zl3k8HCbknWebTPeq32oj0y83lo2TDgTngPb+coPf8d4Tso9T4oNRhzVZ+GcYtHfIGc' +
        'a7alX1+8aibvztPEXLkwG36UyZuRKDfEgGnF8VTkauQxAglIklVqSl4AUZ+CrqsjNz08156j9bTPzRBOiORm' +
        'Z6iXMRx+EzB32LoRqDDUuJmU1RV17Ht+A22f99JY2MDNeoMbrul0x2GwygW5qZZFJTKLyjbfx3FVwVA4r8BB' +
        'qLf4h3uRpaMF5VgZO5ULKbeZloFC0qQ1EXZZ0/FUoYJ8Dpl6n2b6TDS1qioYQYSw0w87kKIN+12qL+H+NAg6' +
        'VkQwnA7uvouG5FuKoSAKf9XKF+HmRJ2/rjcDs9uU++HMpe10M9rJXsriRS8HIeZjDYnXIXt2mwlm9dSKgRkR' +
        'G44FSlB06VUMiOgGdiieZK3AYEsvfnlgd8CtpiBfxIfJp/lo8v/9WsSorRSEfgAAAAASUVORK5CYII=")',
    'background-repeat': 'no-repeat',
    'background-position': 'center center',
    'display': 'inline-block',
    'width': '32px',
    'height': '32px',
    'margin-left': '5px',
    'cursor': 'pointer'
};

ConvergenceHelp['GetViewCode'] = function () {
    var $someFrame = $('iframe[name=someFrame]');
    var url = $someFrame.attr('data-postloadsrc');

    if (!url) {
        url = $someFrame.attr('src');
    }

    if ($someFrame.length > 0) {
        try {
            var iframeWindow = $someFrame.get(0).contentWindow;

            if ((!url) || (url !== iframeWindow.location.href)) {
                url = iframeWindow.location.href;
            }
        } catch (e) {
        }
    }
	else {
		var $leftMenuLink = $('.leftMenuItem.parentNode.subMenuSelected[data-ajax][data-ajax-mode="replace"][data-ajax-update="#mainLayoutOperationContent"]');
		
		if($leftMenuLink.length > 0) {
			url = $leftMenuLink.attr('href');
			
			if(!url.match(/^https?\:/i)) {
				url = location.protocol + '//' + location.host + url;
			}
		}
	}

    var viewCode = ConvergenceHelp.ParseSrcCode(url);

    if (this.$('ul.tabs:visible, div.tabsContainer:visible').length > 0) {
        var $ulTabs = this.$('ul.tabs li.active');
        var $divTabs = this.$('div.tabsContainer div.tabSelector.active');
        var tabViewCode = null;

        if ($ulTabs.length > 0) {
            viewCode = viewCode + "/" +
                $ulTabs.attr('dd-tab').replace("#", "").replace(/\-\d*/, "");
        }
        else if ($divTabs.length > 0) {
            viewCode = viewCode + "/" + $divTabs.attr('id');
        }
    }

    return viewCode;
}

ConvergenceHelp['Initialization'] = function () {
    this.URLS = {};

    $.ajax({
        url: this.InitializeURL,
        type: 'GET',
        cache: false,
        async: true
    }).done(function (response) {
        $.each(response.Data, function (key, value) {
            ConvergenceHelp.Add(value);
        });

        ConvergenceHelp.HasPermision = response.HasPermission;
        ConvergenceHelp.Conts = {
            EN: response.English,
            ES: response.Spanish,
            PT: response.Portuguese,
            FR: response.French
        }
    });
};

ConvergenceHelp['ParseSrcCode'] = function (src) {
    //1: Area/Module
    //2: Controller
    //3: View
    if (typeof src == 'undefined') {
        return false;
    }
    var codeMatch = src.match(/(?:https?\:\/+[\w.:]+)(?:\/(?:IDB))?(?:\/(?:\w{2}-?\w\d+))?\/+(\w+)\/(\w+)(?:\/(\w+))?(?:.*)/i);
    var viewCode = "";

    if (codeMatch === null) {
        return viewCode;
    }

    $.each(codeMatch, function (index, value) {

        if (value === undefined) {
            return false;
        }

        if (index > 0) {
            if (viewCode.length > 0) {
                viewCode = viewCode.concat("/");
            }

            viewCode = viewCode.concat(value);
        };
    });

    return viewCode;
};

ConvergenceHelp['Remove'] = function (key) {
    delete this.URLS[key];
}

ConvergenceHelp['WriteLogger'] = function () {
    var viewCode = this.GetViewCode();

    console.log("HELP settings");
    console.log("   - viewCode: " + viewCode);
    console.log("   - url: " + this.Find(viewCode));
}

function changedFrame() {
    ConvergenceHelp.ViewCode = ConvergenceHelp.GetViewCode();

    if (ConvergenceHelp.$('ul.tabs:visible, div.tabsContainer:visible').length > 0) {
        ConvergenceHelp
            .$('ul.tabs li:not("li.active"), div.tabsContainer div.tabSelector:not(".active")')
            .on("click", function () {
                ConvergenceHelp.Change(ConvergenceHelp.ViewCode);
            });
    }

    ConvergenceHelp.Change(ConvergenceHelp.ViewCode);
}

function handleDivLoad() {
    changedFrame();
    handleFrameLoad();
    bindMenuItemListener();
}

function handleFrameLoad() {
    $('iframe[name=someFrame]').on('load', changedFrame);
}

function bindMenuItemListener() {
    $('[data-ajax-mode]').attr({
        'data-ajax-success': 'handleDivLoad'
    });
}

function openModal() {
    $.ajax({
        url: $('#headerHelp').attr('data-help-modal-url'),
        type: 'POST',
        data: { 'viewCode': ConvergenceHelp.ViewCode },
    }).done(function (response) {
        $('#addViewCode').html(response);
        $('#addViewCode').modal({ backdrop: 'static', keyboard: false });
    });
}

function isEmpty(obj) {
    var hasOwnProperty = Object.prototype.hasOwnProperty;

    if (obj === null) return true;

    if (obj.length > 0) return false;

    if (obj.length === 0) return true;

    if (typeof obj !== "object") return true;

    for (var key in obj) {
        if (hasOwnProperty.call(obj, key)) return false;
    }

    return true;
}

$(document).ready(function () {
    bindMenuItemListener();
    handleFrameLoad();
});