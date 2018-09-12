function processJsonDate(date)
{
    return new Date(parseInt(date.substring(6)));
}

function escapeRegExp(string)
{
    return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
}

function replaceAll(string, find, replace)
{
    if (string == null)
    {
        return null;
    }
    return string.replace(new RegExp(escapeRegExp(find), 'g'), replace);
}

function getParentElement(selector)
{
    var windowElement = window.parent;
    if (windowElement == null)
    {
        windowElement = window;
    }

    return windowElement.jQuery(selector);
}

function showOverlay()
{
    getParentElement('.vex').attr('style', null);
}

function hideOverlay()
{
    getParentElement('.vex').attr('style', 'display:none');
}

function moveScrollTop() {
    var targetWindow = window.parent;
    targetWindow.scroll(targetWindow.scrollX, 0);
}

function resizeIframe()
{
    /*
    var $frame = window.parent.jQuery('iframe:eq(0)').not('[data-viewer="true"]').not('.iframeAutoResize');

    var contentBody = $('html');
    var extraSize = 50;
    var testBody = $frame.contents().find('body > section.central > div.renderHeight > section.central > div.mod_contenido_central.generic_tasks');
    if (testBody.length > 0) {
        contentBody = testBody;
        extraSize = 240;
    } else {
        contentBody = $frame.contents();
    }

    var value = $frame.contents().find('body').height() + extraSize;

    //If exist popup (vex) and the calculated iframe size is less than popup size then use popup size.
    var iframeVex = contentBody.find('.vex .vex-content');
    if (iframeVex.length > 0) {
        var vexHeight = 320 + iframeVex.height();

        if (value < vexHeight) {
            value = vexHeight;
        }
    }

    $frame.css('height', value + 'px')
        .attr('height', value)
        .attr("scrolling", "no");

    if (window.parent.jQuery('.vex').length == 0)
    {
        var vexContainer = $('<div/>');
        var vexOverlay = $('<div/>');

        vexContainer.addClass('vex');
        vexOverlay.addClass('vex-overlay');
        vexContainer.hide();

        vexContainer.append(vexOverlay);
        window.parent.jQuery('body').prepend(vexContainer);
    }*/

    resizeIframeCloud();
}

function EditValidationMessages(properties)
{
    if (!Array.isArray(properties) || (properties.length == 0))
    {
        return;
    }

    properties.forEach(function (item, index)
    {
        if (Array.isArray(item) && (item.length == 2) && (item[0] != null) && (item[0] != ""))
        {
            window.ParsleyValidator.catalog.en[item[0]] = item[1];
        }
    });
}

function prepareEmail(to, subject, bodyText)
{
    var form = document.createElement('form');

    //Set the form attributes 
    form.setAttribute('method', 'post');
    form.setAttribute('enctype', 'text/plain');
    form.setAttribute('action', 'mailto:' + encodeURIComponent(to ? to : ' ') + '?Subject=' + encodeURIComponent(subject ? subject : ' ') + '&amp;ampBody=' + encodeURIComponent(bodyText ? bodyText : ' '));
    form.setAttribute('style', 'display:none');

    //Append the form to the body
    document.body.appendChild(form);

    //Submit the form
    form.submit();

    //Clean up
    document.body.removeChild(form);
}

//Deshabilita todos los input, textarea y select del contenedor que esten contenidos dentro de un modo edicion (data-pagemode=edit)
function disableContainer(container, reloadParsley) {
    if (reloadParsley == null) {
        reloadParsley = false;
    }
    var form = container.parents('[data-parsley-validate]:first');
    //if (reloadParsley) {
    //    destroyParsley(form);
    //}

    var originalSelector = '{0}input, {0}select, {0}textarea, {0}.dropdown button';
    var selector = null;

    container.each(function ()
{
        var item = $(this);

        var inputs = null;


        if (item.attr('data-PageMode') == null) {
            selector = String.format(originalSelector, '[data-PageMode=edit] ');
        } else {
            selector = String.format(originalSelector, '');
        }

        var inputs = item.find(selector);
        inputs.attr('disabled', 'disabled');
    });

    //inputs.each(function() {
    //    var validator = $(this).parsley();
    //    if ((validator != null) && (validator.destroy != null)) {
    //        $(this).parsley().destroy();
    //    }
    //});

    if (reloadParsley) {
        destroyParsley(form);
        initParsley(form);
        destroyParsley(form);
        initParsley(form);
    }
}

//Habilita todos los input, textarea y select del contenedor que esten contenidos dentro de un modo edicion (data-pagemode=edit).
//Quedan excluidos de la rehabilitacion aquellos que estan dentro de la plantilla de un repeater y
//aquellos que estan marcados como data-disabled="disabled"
function enableContainer(container, excludeSelector, reloadParsley)
{
    if (reloadParsley == null) {
        reloadParsley = false;
    }

    var form = container.parents('[data-parsley-validate]:first');
    //if (reloadParsley) {
    //    destroyParsley(form);
    //}

    //var selector = '[data-PageMode=edit] input, [data-PageMode=edit] select, [data-PageMode=edit] textarea';
    var originalSelector = '{0}input{1}, {0}select{1}, {0}textarea{1}, {0}.dropdown button{1}';
    var selector = null;

    container.each(function () {
        var item = $(this);


        var inputs = null;
        if (item.attr('data-PageMode') == null) {
            selector = String.format(originalSelector, '[data-PageMode=edit] ');
        } else {
            selector = String.format(originalSelector, '');
        }

        if ((excludeSelector != null) && (excludeSelector !== "")) {
            var tmpExclude = String.format(':not({0})', excludeSelector);
            selector = String.format(selector, null, tmpExclude);
        } else {
            selector = String.format(selector, null, '');
    }
        var inputs = item.find(selector);
        inputs.each(function () {
        var $this = $(this);
        var stayDisabled = $this.attr("data-disabled");
        var templateList = $this.parents('div[data-repeater-template]');

            if ((templateList.length == 0) && (stayDisabled !== "disabled")) {
            $this.removeAttr('disabled');
                //$this.parsley();
        }
    });
    });

    
    if (reloadParsley) {
        destroyParsley(form);
        initParsley(form);
        destroyParsley(form);
        initParsley(form);
    }
}

function parseFloatFormated(value, decimalPrecision)
{
    var correctValue = value.replace(/,/g, '');
    var floatValue = parseFloat(correctValue);
    if (isNaN(floatValue))
    {
        floatValue = 0;
}

    if ((decimalPrecision != null) && (decimalPrecision >= 0))
    {
        floatValue = floatValue.toFixed(decimalPrecision);
    }

    return floatValue;
}

function convertFloatToStringWithFormat(value)
{
    var valueString = value.toString();
    var splitNumber = valueString.split(".");
    var integerPart = splitNumber[0];
    var decimalPart = "00";
    if (splitNumber.length == 2)
    {
        decimalPart = splitNumber[1];
        if (decimalPart.length == 1) {
            decimalPart = decimalPart + '0';
        }
    }

    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(integerPart))
    {
        integerPart = integerPart.replace(rgx, '$1' + ',' + '$2');
    }

    return integerPart + "." + decimalPart;
}

function AddDocumentsGeneric(sourceType, target, documentList)
{
    var newId = target.data('autogenerate-id');
        
    var tbody = target.children("tbody");

    var trTemplate = tbody.children().first();

    var documentNumberList = documentList;
    if (sourceType !== "added")
    {
        documentNumberList = [];
        for (var i = 0; i < documentList.length; i++)
        {
            var document = documentList[i];
            documentNumberList.push(document.DocumentNumber);
        }
    }

    for (var i = 0; i < documentNumberList.length; i++)
    {
        newId = newId +1;
        var docNumber = documentNumberList[i];

        var clonedTr = trTemplate.clone(false);
        clonedTr.removeClass("hide");
        clonedTr.attr('data-persist-new', newId);

        var tdHidden = clonedTr.children("td").first();
        tdHidden.find("input").val(docNumber);

        var tdAction = clonedTr.children("td").last();
        tdAction.find("button[data-id]").attr("data-id", docNumber);

        var date = convertDateToStringDDMMMYYYY(new Date());
        var tdIdbDate = clonedTr.children("td[data-name=idb-doc-date]");
        tdIdbDate.find("span").html(date);

        var tdIdbDocNumber = clonedTr.children("td[data-name=idb-doc-number]");
        tdIdbDocNumber.find("span").html(docNumber);

        clonedTr.find('input').removeAttr('disabled');
        clonedTr.find("input").attr('data-persist-new', newId);

        clonedTr.appendTo(tbody);

        bindHandlers(clonedTr);
    }
    closeModal();
    hideLoaderOptional();

    target.data('autogenerate-id', newId);
}

function closeModal() {
    var vexModal = $(this).parents("[data-id=documentTable]").parents(".vex");
    if (vexModal.length > 0)
        vex.closeByID(vexModal.data().vex.id);
}

function convertDateToStringDDMMMYYYY(date) {
    var day = date.getDate();
    var month = $.datepicker._defaults.monthNamesShort[date.getMonth()];
    var year = date.getFullYear();
    return String.format("{0} {1} {2}", day, month, year);
}

function convertStringToDateDDMMMYYYY(stringDate) {
    if ((stringDate == null) || (stringDate == "")) {
        return null;
    }

    var subStrings = stringDate.split(" ");

    if (subStrings.length != 3) {
        return null;
    }

    var day = parseInt(subStrings[0]);
    var month = $.datepicker._defaults.monthNamesShort.indexOf(subStrings[1]);
    var year = parseInt(subStrings[2]);
    if (isNaN(day) || isNaN(month) || isNaN(year)) {
        return null;
    }

    return new Date(year, month, day, 0, 0, 0);
}

function DownloadDocumentGeneric(source)
{
    var url = source.attr('data-url') + "?documentNumber=" + source.attr('data-id');
    window.open(url, '_blank');
}

function DownloadDocumentMedia(source) {
    var url = source.attr('data-url') + "?documentNumber=" + source.attr('data-id');
    window.open(url, '_blank');
}

function resetStyleTableDocument(table) {
    var groupRow = table.find('tbody tr:not(.hide)');
    groupRow.removeClass("custom-odd");
    groupRow.removeClass("custom-even");
    $(groupRow).each(function (index) {
        var row = $(this);
        if ((index % 2) == 0) {
            row.addClass("custom-even");
        } else {
            row.addClass("custom-odd");
        }
    });
}


function DeleteDocumentGeneric(source)
{
    var beforeDelete = source.attr("data-before-delete");
    var canDelete = true;
    if ((beforeDelete != null) && (beforeDelete !== "")) {
        canDelete = window[beforeDelete]();
    }
    if (canDelete == null || canDelete == true) {
        var tableContainer = source.closest("table");
        source.parents("tr").first().remove();
        resetStyleTableDocument(tableContainer);
    } else if (typeof (canDelete.done) === "function") {
        canDelete.done(function (dataCanDelete) {
            if (dataCanDelete) {
                var tableContainer = source.closest("table");
                source.parents("tr").first().remove();
                resetStyleTableDocument(tableContainer);
            }
        });
    }
}

function redirectToSharepointHelp(caller)
{
    var param = $(caller).attr('caller');
    var partialUrl = $(caller).attr('data-url');
    
    var url = postUrl(partialUrl + '?caller=' + param);
    
    window.open(url.responseText);
}

function selectDropDownItem(inputDropDown, itemValue) {
    if (itemValue == null) {
        itemValue = '';
    }

    var button = inputDropDown.prevAll('button');
    var spanText = button.find('.valueText');
    var aList = inputDropDown.nextAll('ul.dropdown-menu').find('a');
    var div = inputDropDown.closest('div.dropdown');

    aList.removeAttr('dd-selected');

    var newSelected = aList.filter(function () {
        var value = $(this).attr('dd-value');
        if (value == null) {
            value = '';
        }

        return itemValue == value;
    });

    if (newSelected.length == 1) {
        newSelected.attr('dd-selected', 'dd-selected');
        spanText.html(newSelected.html());

        div.removeClass('placeholder');
        if (itemValue == '') {
            div.addClass('placeholder');
        }
        inputDropDown.val(itemValue);
    }
}

function selectDropDownByRelativePosition(inputDropDown, relativePosition) {

    var aList = inputDropDown.nextAll('ul.dropdown-menu').find('a');
    var numberElements = aList.length;

    if ( relativePosition == null || relativePosition < 0 || relativePosition >= numberElements) {
        return;
    }

    var itemValue = null;

    aList.each(function (ix, item) {
        if (ix == relativePosition ) {
            itemValue = $(item).attr('dd-value');
        }
    });

    selectDropDownItem(inputDropDown, itemValue);
}
