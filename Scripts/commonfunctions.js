/*
focus in a specific object of the page and move the cursor
*/
$.fn.focusx = function () {
	var x = window.scrollX, y = window.scrollY;
	this.focus();
	window.scrollTo(x, y);
	if (!window.parent || !$('iframe', window.parent.document).offset()) {
		return;
	}
	var topFrame = $('iframe', window.parent.document).offset().top;
	$('html, body', window.parent.document).animate({ scrollTop: this.offset().top + topFrame - 100 }, 500);
	return this; //chainability
};

/*
check if a specific element exists
*/
function isDefined(element) {
	if (typeof element == 'undefined' || element==null) {
		return false;
	}
	return true;
}

/* 
check if a variable exists and if this value is true
*/
isTrue = function (element) {
	if (!isDefined(element)) {
		return false;
	}
	return element == true || false; 
};

/*
copy all attributes from object to other
*/
function copyObject(obj) {
	var newObj = {};
	for (var key in obj) {
		//copy all the fields
		newObj[key] = obj[key];
	}

	return newObj;
}

/*
make a slplit of a string but instead of return an array with one
value when the string is empty this function returns a empty array
*/
function splitString(value, separator) {
	value = value.trim();
	if (value.length == 0) {
		return [];
	}
	return value.split(separator);
}

/* 
convert a date value in string if the value is defined, otherwise
return empty
*/
dateToString = function (value) {
	if (!isDefined(value)) {
		return '';
	}
	var yyyy = value.getFullYear().toString();
	var mm = (value.getMonth() + 1).toString();
	var dd = value.getDate().toString();
	return yyyy +'-'+ (mm[1] ? mm : "0" + mm[0]) +'-'+ (dd[1] ? dd : "0" + dd[0]); // padding
};

function contains(a, obj) {
    var i = a.length;
    while (i--) {
       if (a[i] === obj) {
           return true;
       }
    }
    return false;
}

function trimStart(character, string) {
    var startIndex = 0;

    while (string[startIndex] === character) {
        startIndex++;
    }

    return string.substr(startIndex);
}

var radomID = function () {
	// return an numeric ID based on current date
	var current =new Date();
	return -Math.abs(current.getTime() - new Date(current.getFullYear(), current.getMonth(), current.getUTCDate(), 1, 1, 1, 1).getTime());
};
var lastHeight = 0;
function calculateHeight() {
	
    var element = $('.body-content');
    var height = element.prop("scrollHeight");
    height += 100;
    
    if (height < 700) {
        height += 800
    }
    
    
	//don't update height if is the same 
   if (lastHeight != height) {
    	lastHeight = height;
    	$(window.parent.document)
			.find('iframe')
			.css("height", height + "px");
    }
}

$(document).ready(function () {
    setInterval(calculateHeight, 1500);
});

//from: http://stackoverflow.com/questions/18053408/vertically-centering-bootstrap-modal-window
//this method center vertically bootstrap modals
(function ($) {
	"use strict";
	function centerModal() {
		$(this).css('display', 'block');
		var $dialog = $(this).find(".modal-dialog"),
        offset = ($(window).height() - $dialog.height()) / 2,
        bottomMargin = parseInt($dialog.css('marginBottom'), 10);

		// Make sure you don't hide the top part of the modal w/ a negative margin if it's longer than the screen height, and keep the margin equal to the bottom margin of the modal
		if (offset < bottomMargin) offset = bottomMargin;
		$dialog.css("margin-top", offset);
	}

	$(document).on('show.bs.modal', '.modal', centerModal);
	$(window).on("resize", function () {
		$('.modal:visible').each(centerModal);
	});
}(jQuery));

function daydiff(first, second) {
	if (!second) {
		var current = new Date();
		second = new Date(current.getFullYear(), current.getMonth() , current.getDate());
	}
	if (!first)
		return null;
	var timeDiff = first.getTime() - second.getTime();
	var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
	return diffDays;
}

Date.prototype.addDays = function (days) {
	var dat = new Date(this.valueOf());
	dat.setDate(dat.getDate() + days);
	return dat;
}

var getUrlParameter = function getUrlParameter(sParam) {
	var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

	for (i = 0; i < sURLVariables.length; i++) {
		sParameterName = sURLVariables[i].split('=');

		if (sParameterName[0] === sParam) {
			return sParameterName[1] === undefined ? true : sParameterName[1];
		}
	}
};

Date.prototype.getDateOnly = function () {
	var current = new Date();
	return new Date(current.getUTCFullYear(), current.getUTCMonth(), current.getUTCDate(),0,0,0,1);
}


function dayDiffMathRound(startDate, endDate) {

    var msPerDay = 24 * 3600 * 1000;
    startDate.setHours(0, 0, 0, 0);
    endDate.setHours(0, 0, 0, 0);
    var diffDayMs = startDate.getTime() - endDate.getTime();
    var response = Math.round(diffDayMs / msPerDay);
    return response;

}

function createGuid() {
	return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
		var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
		return v.toString(16);
	});
}

function getDatesRange(startDate, stopDate) {
	var dateArray = new Array();
	var currentDate = startDate;
	while (currentDate <= stopDate) {
		dateArray.push(new Date(currentDate))
		currentDate = currentDate.addDays(1);
	}
	return dateArray;
}
(function (window) {

	$type = String;
	$type.__typeName = 'String';
	$type.__class = true;

	$prototype = $type.prototype;
	$prototype.endsWith = function String$endsWith(suffix) {
		/// <summary>Determines whether the end of this instance matches the specified string.</summary>
		/// <param name="suffix" type="String">A string to compare to.</param>
		/// <returns type="Boolean">true if suffix matches the end of this instance; otherwise, false.</returns>
		return (this.substr(this.length - suffix.length) === suffix);
	}

	$prototype.startsWith = function String$startsWith(prefix) {
		/// <summary >Determines whether the beginning of this instance matches the specified string.</summary>
		/// <param name="prefix" type="String">The String to compare.</param>
		/// <returns type="Boolean">true if prefix matches the beginning of this string; otherwise, false.</returns>
		return (this.substr(0, prefix.length) === prefix);
	}

	$prototype.trim = function String$trim() {
		/// <summary >Removes all leading and trailing white-space characters from the current String object.</summary>
		/// <returns type="String">The string that remains after all white-space characters are removed from the start and end of the current String object.</returns>
		return this.replace(/^\s+|\s+$/g, '');
	}

	$prototype.trimEnd = function String$trimEnd() {
		/// <summary >Removes all trailing white spaces from the current String object.</summary>
		/// <returns type="String">The string that remains after all white-space characters are removed from the end of the current String object.</returns>
		return this.replace(/\s+$/, '');
	}

	$prototype.trimStart = function String$trimStart() {
		/// <summary >Removes all leading white spaces from the current String object.</summary>
		/// <returns type="String">The string that remains after all white-space characters are removed from the start of the current String object.</returns>
		return this.replace(/^\s+/, '');
	}

	$type.format = function String$format(format, args) {
		/// <summary>Replaces the format items in a specified String with the text equivalents of the values of   corresponding object instances. The invariant culture will be used to format dates and numbers.</summary>
		/// <param name="format" type="String">A format string.</param>
		/// <param name="args" parameterArray="true" mayBeNull="true">The objects to format.</param>
		/// <returns type="String">A copy of format in which the format items have been replaced by the   string equivalent of the corresponding instances of object arguments.</returns>
		args.unshift(format);
		return String._toFormattedString(false, args);
	}

	$type._toFormattedString = function String$_toFormattedString(useLocale, args) {
		var result = '';
		var format = args[0];

		for (var i = 0; ;) {
			// Find the next opening or closing brace
			var open = format.indexOf('{', i);
			var close = format.indexOf('}', i);
			if ((open < 0) && (close < 0)) {
				// Not found: copy the end of the string and break
				result += format.slice(i);
				break;
			}
			if ((close > 0) && ((close < open) || (open < 0))) {

				if (format.charAt(close + 1) !== '}') {
					throw new Error('format stringFormatBraceMismatch');
				}

				result += format.slice(i, close + 1);
				i = close + 2;
				continue;
			}

			// Copy the string before the brace
			result += format.slice(i, open);
			i = open + 1;

			// Check for double braces (which display as one and are not arguments)
			if (format.charAt(i) === '{') {
				result += '{';
				i++;
				continue;
			}

			if (close < 0) throw new Error('format stringFormatBraceMismatch');


			// Find the closing brace

			// Get the string between the braces, and split it around the ':' (if any)
			var brace = format.substring(i, close);
			var colonIndex = brace.indexOf(':');
			var argNumber = parseInt((colonIndex < 0) ? brace : brace.substring(0, colonIndex), 10) + 1;

			if (isNaN(argNumber)) throw new Error('format stringFormatInvalid');

			var argFormat = (colonIndex < 0) ? '' : brace.substring(colonIndex + 1);

			var arg = args[argNumber];
			if (typeof (arg) === "undefined" || arg === null) {
				arg = '';
			}

			// If it has a toFormattedString method, call it.  Otherwise, call toString()
			if (arg.toFormattedString) {
				result += arg.toFormattedString(argFormat);
			}
			else if (useLocale && arg.localeFormat) {
				result += arg.localeFormat(argFormat);
			}
			else if (arg.format) {
				result += arg.format(argFormat);
			}
			else
				result += arg.toString();

			i = close + 1;
		}

		return result;
	}

})(window);