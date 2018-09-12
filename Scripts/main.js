$(window).load(function ()
{
	if ($(document).foundation)
	{
		$(document).foundation();
	}
	if (typeof buscarFocus == 'function')
	{
		buscarFocus();
	}
});

var checkBrowser = function () {
    if (navigator.userAgent.indexOf('MSIE') > 0 && location.hostname != 'optimamvc-d.iadb.org') {
		alert(' Please use Google Chrome to access Convergence');
		parent.history.back();
		return false;
	}
}

var Redirect = function (url) {
    window.parent.location = url;
}

function loadFunctions()
{
	idbg.loadCount = 0;
	idbg.loadInterval = setInterval(correctIframeCallback, 800);
}

var redirectPage = function (url) {
    url = idbg.getPath(url);

    if ($(window.parent.document).find('iframe').length > 0) {
        $(window.parent.document).find('iframe').attr("height", "0px");
        $(window.parent.document).find('iframe').attr('src', url);
    }
    else {
        window.location.href = url;

    }
}
//custom prototype functions
String.prototype.trimStart = function (c)
{
	c = c ? c : ' ';
	var i = 0;
	for (; i < this.length && this.charAt(i) == c; i++);
	return this.substring(i);
}

//iadb global functions
var idbg = {};

idbg.config = {
	defaultLanguage: 'en',
	translationFunction: null,
	baseSharePointAddress: 'https://localhost/sharepoint/',
	domain: '',
	basePath: '/',
	virtualPath: ''
};
idbg.current = {
	language: 'en',
	translation: null
};

idbg._ = function (source)
{
	return source;
}

idbg.getPath = function (url)
{
	url = url.trimStart('/');
	var virtual = idbg.config.virtualPath.toLocaleLowerCase();
	if (virtual != '')
	{
		if (url.toLowerCase().indexOf(virtual) == 0)
		{
			url = url.substring(virtual.length).trimStart('/');
		}
	}
	return idbg.config.basePath + url;
}

idbg.init = function ()
{
    idbg.config.virtualPath = idbg.parseUrl(idbg.config.basePath).virtualPath();
}

//url parser
idbg.urlParser = function (url)
{
	this.url = url.toLowerCase();
	this.parser = document.createElement('a');
	this.parser.href = this.url;
}

idbg.urlParser.prototype.url = '';
idbg.urlParser.prototype.parser = null;

idbg.urlParser.prototype.virtualPath = function () {
    var host = this.parser.protocol + '//' + this.parser.host;
    //IE this.parser.host returns the port even if it is the default one (:80)
    if (host.substring(host.length - 3, host.length) === ":80")
    {
        host = host.substring(0, host.length - 3);
    }
    var virtualPart = this.url.substring(host.length);
    return virtualPart.trimStart('/');
}

idbg.parseUrl = function (url)
{
	return new idbg.urlParser(url);
}
idbg.lockUi = function (ui_element, waiting, message)
{
	var container = jQuery('<div/>');
	var isWindow = false;
	if (ui_element == null)
	{
		ui_element = jQuery(document.body);
		container.css('width', $(document).width());
		container.css('height', $(document).height());
		isWindow = true;
	} else
	{
		container.css('width', ui_element.width());
		container.css('height', ui_element.height());
	}

	ui_element.children('.loading-container').remove();

	if (!waiting)
	{
		return;
	}


	container.css('top', 0);
	container.css('left', 0);
	container.css('z-index', 2);

	container.addClass('loading-container');

	var overlay = jQuery('<div/>');
	overlay.addClass('loading-overlay');
	container.append(overlay);

	var loading_window = jQuery('<div/>');
	loading_window.addClass('loading-window');
	if (!message)
	{
		message = idbg._('Loading...')
	}
	loading_window.text(message);
	loading_window.width(ui_element.width() / 6);
	loading_window.css('left', ((container.width() / 2) - (loading_window.width() / 2)) + 'px');
	if (!isWindow)
	{
		loading_window.css('top', ((container.height() / 2) - (loading_window.height() / 2)) + 'px');

	} else
	{
		loading_window.css('top', (($(window).height() / 2) - (loading_window.height() / 2)) + 'px');
		loading_window.css('position', 'fixed');
	}
	container.append(loading_window);

	ui_element.append(container);
}
idbg.preloadImages = function ()
{
	for (i = 0; i < idbg.preloadImages.arguments.length; i++)
	{
		$('<img/>')[0].src = idbg.preloadImages.arguments[i];
	}
}
$(document).ready(function ()
{
	if ($('.kendo-dropdown').kendoDropDownList)
	{
		$('.kendo-dropdown').kendoDropDownList({
			dataTextField: "text",
			dataValueField: "value",
			width: '250px'
		});
	}
	$('.auto-lock-ui').click(function ()
	{
		idbg.lockUi(null, true);
	});

	if ($('.idb-checkbox').idbCheckbox)
	    $('.idb-checkbox').idbCheckbox();



	$('.click-lock').click(function ()
	{

		var action = $(this);
		if (action.data('lockmessage'))
		{
			idbg.lockUi(null, true, action.data('lockmessage'))
		} else
		{
			idbg.lockUi(null, true);
		}

	});
	$('.click-action').click(function ()
	{
		var button = $(this);
		var route = button.data('route');
		var confirmation = button.data('confirmation');
		if (button.hasClass('disabled'))
		{
			return;
		}
		if (confirmation && route)
		{
			kendoConfirm({
				message: confirmation,
				data: route,
				title: 'Dialog',
				onConfirm: function (data)
				{
					if (data.indexOf('javascript') == 0)
					{
					    var myConstr = "constructor";
					    myConstr[myConstr][myConstr](data.substring(11))();

						if ($(this).data('close-confirm') != false)
						{
							$("#kendo-dialog").data('kendoWindow').close();
						}
					} else
					{
						document.location.href = data;
					}
				},
				closeConfirm: button.data('close-confirm')
			});
			return;
		}
		if (route)
		{
			document.location.href = route;
		}
	});

	imageLoaderStart();

	$('.click-collapse').click(function ()
	{
		var button = $(this);
		if (button.data('element'))
		{
			$(button.data('element')).toggle();
		}
	});

	var container = jQuery('<div/>');
	container.addClass('loading-icon');
	jQuery(document.body).append(container);
	//var url;
	//console.log('cath!!');
	//idbg.preloadImages(idbg.config.basePath +'/Content/kendo/Uniform/convergence-loading-3.gif');
});
function imageLoaderStart()
{
	$('.image-loader').each(function (index, element)
	{
		var img = $('<img/>');
		element = jQuery(element);
		element.empty();
		var url = element.data('url');
		img.attr('src', url);
		element.append(img);
		img.data('original-url', element.data('original-url'));
		img.hide();
		img.load(function ()
		{
			var img = $(this);
			var parent = img.parent();
			var link = $('<a/>');
			var original;
			if (img.data('original-url'))
			{
				original = img.data('original-url');
			} else
			{
				original = img.attr('src');
			}
			link.attr('href', original);
			link.attr('target', '_blank');
			img.detach();
			link.append(img);
			parent.append(link);
			parent.css('background-image', 'none');
			img.css('width', parent.width());
			img.css('height', parent.height());
			img.show();
		});
		img.error(function ()
		{
			var img = $(this);
			var parent = img.parent();
			parent.addClass('not-found');
		});

	});
}
function idbCloseKendoWindow(parent)
{
	if (parent)
	{
		window.parent.$('#kendo-dialog')
            .data('kendoWindow')
            .close();
	} else
	{
		$('#kendo-dialog').data("kendoWindow").close();
	}

}
function idbKendoWindow(options)
{
	options = $.extend({}, {
		content: '',
		iframe: true,
		width: '800px',
		height: '600px',
		modal: true,
		draggable: false,
		visible: false,
		title: 'IDB Window'
	}, options);

	$("#kendo-dialog").kendoWindow(options);

	var kendoOverlay = $("#kendo-dialog").data("kendoWindow");
	var classes = kendoOverlay.wrapper.attr('class');
	kendoOverlay.wrapper.attr('id', 'kendo-dialog-container');
	kendoOverlay.center();
	kendoOverlay.open();

	setInterval(function ()
	{
		var kendoOverlay = $("#kendo-dialog").data("kendoWindow");
		var iframe = $("#kendo-dialog").children('iframe');
		var window = iframe.contents().find('.idb-content-page');

		if (window.length > 0)
		{
			var height = window.prop("scrollHeight");
			kendoOverlay.setOptions({
				height: height
			});
		}

	}, 800);
}

function kendoConfirm(options)
{
	var container = $(document).find('#kendo-dialog');
	options = jQuery.extend(null, {
		onConfirm: function () { $("#kendo-dialog").data("kendoWindow").close(); },
		closeConfirm: true
	},
     options
    );
	if (container.length == 0)
	{
		var container = $('<div id="kendo-dialog"/>');
		container.appendTo('body');
	}
	container.html('');
	var divElement = $('<div class="kendo-dialog-message"/>');
	divElement.text(options.message);
	container.append(divElement);
	divElement = $('<div class="kendo-dialog-buttons"/>');

	var cancelButton = $('<a href="#" onclick="return false">Cancel</a>');
	var sendButton = $('<input type="button" class="button" value="Accept"/>');

	sendButton.attr('data-optional', options.data);
	sendButton.attr('data-close-confirm', options.closeConfirm);
	sendButton.after('<span class="loading-uniform"></span>');
	cancelButton.click(function ()
	{
		$("#kendo-dialog").data("kendoWindow").close();
	});

	sendButton.click(function ()
	{
		$(this).prop('disabled', true);
		$(this).addClass('disabled');
		$(this).after('<span class="loading-uniform"></span>');
		$(this).prev().hide();
		$('.k-window-actions').hide();
		options.onConfirm.call(this, $(this).data('optional'));
	});

	divElement.append(cancelButton);
	divElement.append(sendButton);
	container.append(divElement);
	kendoOverlay({
		title: 'Confirm',
		width: '600px',
		class: 'warning'
	});
}
function kendoOverlay(options)
{
	var options = jQuery.extend({}, {
		modal: true,
		draggable: false,
		visible: false,

	}, options);
	$("#kendo-dialog").kendoWindow(options);
	var kendoOverlay = $("#kendo-dialog").data("kendoWindow");
	var classes = kendoOverlay.wrapper.attr('class');
	if (options.class)
	{
		kendoOverlay.wrapper.addClass(options.class);
	}
	kendoOverlay.wrapper.attr('id', 'kendo-dialog-container');
	kendoOverlay.center();
	kendoOverlay.open();
}

function idbShowError(error, replace)
{
	var ul = $('div#validation-errors').children('ul');
	if (ul.length == 0)
	{
		ul = $('<ul/>');
		$('div#validation-errors').append(ul);
	}
	if (replace)
	{
		ul.empty();
	}
	if (error != null)
	{
		error = '<li>' + error + '</li>';
		ul.append(error);
	} else
	{
		ul.remove();
	}
}
(function ($)
{
	$.fn.idbCheckbox = function ()
	{
		return this.each(function (index, value)
		{
			var element = $(value);
			var disabled = element.attr('disabled') ? true : false;
			var checked = element.attr('checked');
			var check = $("<span/>")
                .addClass("idb-checkbox")
                .data('checked', checked);
			if (checked)
			{
				check.addClass(disabled ? 'checked-disabled' : 'checked');
			} else if (disabled)
			{
				check.addClass('element-disabled');
			}
			element.css('width', '0');
			element.css('height', '0');
			element.css('float', 'left');
			element.after(check);

			if (!disabled)
			{
				check.click(function ()
				{
					var element = $(this);
					var checked = element.hasClass('checked');
					if (checked)
					{
						element.removeClass('checked');
						element.prev().attr('checked', false);
					} else
					{
						element.addClass('checked');
						element.prev().attr('checked', true);
					}
				});
			}
		});

	}

})(jQuery);

//loadFunctions();

$.fn.bindFirst = function (name, fn)
{
	this.on(name, fn);
	this.each(function ()
	{
		var handlers = $._data(this, 'events')[name.split('.')[0]];
		console.log(handlers);
		var handler = handlers.pop();
		handlers.splice(0, 0, handler);
	});
};

var NavigationHelper = new Object();
NavigationHelper.resultsMatrixPattern = new RegExp(/[A-Z]{2}\-?.+?\/ResultsMatrix\//i);
NavigationHelper.resultsMatrixOutputsPattern = new RegExp(/[A-Z]{2}\-?.+?\/ResultsMatrix\/Outputs\//i);
NavigationHelper.resultsMatrixImpactsPattern = new RegExp(/[A-Z]{2}\-?.+?\/ResultsMatrix\/Impacts\//i);
NavigationHelper.visualizationPattern = new RegExp(/[A-Z]{2}\-?.+?\/Visualization\//i);

NavigationHelper.IsResultsMatrix = function () {
    return location.pathname.match(NavigationHelper.resultsMatrixPattern) !== null;
};

NavigationHelper.IsResultsMatrixImpacts= function () {
    return location.pathname.match(NavigationHelper.resultsMatrixImpactsPattern) !== null;
};

NavigationHelper.IsVisualization = function () {
    return location.pathname.match(NavigationHelper.visualizationPattern) !== null;
};

if (!NavigationHelper.IsResultsMatrix() && !NavigationHelper.IsVisualization())
{
    $(document).ready(function () {

        $.fn.kendoWindowWrapper = $.fn.kendoWindow;

        $.fn.kendoWindow = function (options) {
            
            var originalOptions = options;
            var scrollTop = $(window.parent).scrollTop();
            var sharePointHeaderHeight = 160;
			
            var wrapperOptions = $.extend({}, options, {

                'top': scrollTop,
                'modal': false,
                'draggable': false,
                'pinned': true,
                'open': function () {
                    
                        var scrollTop = $('.k-window').position().top + sharePointHeaderHeight;
                        
                        if(scrollTop < 0) {
                            scrollTop = 0;
                        }

                        $(window.parent).scrollTop(scrollTop);

                    if(typeof originalOptions['open'] == 'function') {
                        originalOptions['open'].apply(this, arguments);
                    }
                }
            });
			
            this.kendoWindowWrapper(wrapperOptions);
            var dialog = this.data('kendoWindow');
            dialog.center = function() {};
            this.data('kendoWindow', dialog);
            return this;
        };
    });

}

idbg.lockUiRM = function (ui_element, waiting, message) {
    var container = $('<div/>');
    var isWindow = false;
    if (ui_element == null) {
        ui_element = $(document.body);
        container.css('width', $(document).width());
        container.css('height', $(document).height());
        isWindow = true;
    } else {
        container.css('width', ui_element.width());
        container.css('height', ui_element.height());
    }

    ui_element.children('.loading-container').remove();

    if (!waiting) {
        return;
    }
    
    container.css('top', 0);
    container.css('left', 0);

    container.addClass('loading-container');

    var overlay = $('<div/>');
    overlay.addClass('loading-overlay');
    container.append(overlay);

    var loading_window = $('<div/>');
    loading_window.addClass('loading-window');
    var frame = $(window.parent.document).find('iframe[name=someFrame]');
    var heightFrame = frame.height();
    var positionTextLoading = heightFrame - 100;
    var widthFrame = frame.width();

    loading_window.css({
        'height': heightFrame,
        'width': widthFrame,
        'line-height': positionTextLoading.toString() + 'px'
    });

    loading_window.addClass('loading-windowRM-Custom');

    if (!message) {
        message = idbg._('Loading...')
    }

    loading_window.text(message);
    container.append(loading_window);
    ui_element.append(container);
}

function getTopKendoWindow() {
    var modalHeight = $('.k-window').height();
    var frameTop = $(window.parent.document).find('iframe[name=someFrame]').position().top;
    var scrollTop = window.parent.window.scrollY;
    var pxTopKendoWindows = frameTop + (modalHeight / 2);

    if (scrollTop > (frameTop / 2)) {
        pxTopKendoWindows += scrollTop - (frameTop / 2);
    }

    return pxTopKendoWindows;

}

function kendoWindowSetTop(node, top) {

    var $parent = $(node).parent();

    if ($parent.length > 0) {
        $parent.get(0).style.setProperty('top', String(top) + 'px', 'important');
    }
};

function kendoWindowCenter(node) {
    var topPosition = getTopKendoWindow();
    return kendoWindowSetTop(node, topPosition);
};
