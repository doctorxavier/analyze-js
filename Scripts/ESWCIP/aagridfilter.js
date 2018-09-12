var AAGridFilter = new Object();

AAGridFilter.DROPDOWN_LIST = 'dropdownlist';
AAGridFilter.DROPDOWN_AUTOCOMPLETE = 'autocomplete';
AAGridFilter.DROPDOWN_BOOLEAN = 'dropdownboolean';

AAGridFilter._texts = {};
AAGridFilter._texts['yes'] = null;
AAGridFilter._texts['no'] = null;
AAGridFilter._fieldsData = {};
AAGridFilter._table = null;
AAGridFilter._tbody = null;
AAGridFilter._pageLengthInput = null;

AAGridFilter._getColText = function (row, colIndex) {
    var column = $(row).children('td').eq(colIndex);
    return column.text().trim();
};

AAGridFilter._getColumnTexts = function (colIndex) {

    var columnTexts = [];

    AAGridFilter._tbody.children('tr').each(function () {

        var columnText = AAGridFilter._getColText(this, colIndex);

        if (columnText && columnTexts.indexOf(columnText) === -1) {
            columnTexts.push(columnText);
        }
    });

    columnTexts.sort();
    return columnTexts;
};

AAGridFilter._populateDropdown = function (fieldname) {

    var fieldData = AAGridFilter._fieldsData[fieldname];

    fieldData['list'].children().slice(1).remove();

    $.each(fieldData['options'], function (index, item) {

        var linkItem = $('<a />');
        linkItem.text(item);
        linkItem.attr('dd-value', item);
        linkItem.attr('dd-text', item);

        var listItem = $('<li />');
        listItem.append(linkItem);
        fieldData['list'].append(listItem);

    });
};

AAGridFilter._updatePagination = function () {

    var pageLength = AAGridFilter._pageLengthInput.val();
    AAGridFilter._table.paginationConfluense(pageLength);
};

AAGridFilter.bindDropdown = function (inputSelector, colIndex, dropdownOptions, dropdownType) {

    var dropdownEntry = $(inputSelector);
    var dropdownFieldname = dropdownEntry.attr('data-idb-fieldname');
    var dropdownInput = $('input[data-idb-fieldname=' + dropdownFieldname + ']');
    var dropdownList = $('ul[aria-labelledby=' + dropdownEntry.attr('id') + ']');

    AAGridFilter._fieldsData[dropdownFieldname] = {
        inputEntry: dropdownEntry,
        fieldname: dropdownFieldname,
        input: dropdownInput,
        list: dropdownList,
        options: dropdownOptions,
        colIndex: colIndex,
        type: dropdownType
    };

    AAGridFilter._populateDropdown(dropdownFieldname);
};


AAGridFilter.bindDropdownAutocomplete = function (inputSelector, colIndex, dropdownOptions) {

    var dropdownEntry = $(inputSelector);
    var dropdownContainer = dropdownEntry.parent().css({'width': '100%'});
    var dropdownInput = dropdownContainer.find('input.hidden');
    var dropdownFieldname = dropdownInput.attr('data-idb-fieldname');
    var dropdownList = dropdownContainer.find('ul.dropdown-menu');
    
    AAGridFilter._fieldsData[dropdownFieldname] = {
        inputEntry: dropdownEntry,
        fieldname: dropdownFieldname,
        input: dropdownInput,
        list: dropdownList,
        options: AAGridFilter._getColumnTexts(colIndex),
        colIndex: colIndex,
        type: AAGridFilter.DROPDOWN_AUTOCOMPLETE
    };

    AAGridFilter._populateDropdown(dropdownFieldname);
};

AAGridFilter.bindListDropdown = function (inputSelector, colIndex) {

    AAGridFilter.bindDropdown(inputSelector, colIndex, AAGridFilter._getColumnTexts(colIndex), AAGridFilter.DROPDOWN_LIST);
};

AAGridFilter.bindBooleanDropdown = function (inputSelector, colIndex) {

    AAGridFilter.bindDropdown(inputSelector, colIndex, [AAGridFilter._texts['yes'], AAGridFilter._texts['no']], AAGridFilter.DROPDOWN_BOOLEAN);
};

AAGridFilter.filterUpdate = function () {

    AAGridFilter._tbody.children().removeClass('filtered');

    $.each(AAGridFilter._fieldsData, function (key, fieldData) {
        
        var filterVal = fieldData['input'].val();

        if (filterVal) {

            $.each(AAGridFilter._tbody.children(), function (index, row) {

                var hideRow = false;

                switch (fieldData['type']) {

                    case AAGridFilter.DROPDOWN_LIST:
                    case AAGridFilter.DROPDOWN_AUTOCOMPLETE:

                        var columnText = AAGridFilter._getColText(row, fieldData['colIndex']);
                        hideRow = columnText !== filterVal;

                        break;

                    case AAGridFilter.DROPDOWN_BOOLEAN:

                        var value = $(row).children().eq(fieldData['colIndex']).find('input[type=checkbox]').prop('checked');

                        if (filterVal == AAGridFilter._texts['yes']) {
                            hideRow = value !== true;
                        }
                        else if (filterVal == AAGridFilter._texts['no']) {
                            hideRow = value !== false;
                        }

                        break;
                }

                if (hideRow) {
                    $(row).addClass('filtered');
                }

            });
        }
    });

    AAGridFilter._updatePagination();
};

AAGridFilter.filterClear = function () {

    AAGridFilter._tbody.children().removeClass('filtered');

    $.each(AAGridFilter._fieldsData, function (fieldname, fieldData) {

        switch (fieldData['type']) {

            case AAGridFilter.DROPDOWN_LIST:
            case AAGridFilter.DROPDOWN_BOOLEAN:

                fieldData['list'].find('a:eq(0)').trigger('click');
                break;

            case AAGridFilter.DROPDOWN_AUTOCOMPLETE:

                fieldData['input'].val('');
                fieldData['inputEntry'].val('');

                break;
        }
        
        
    });

};

AAGridFilter.init = function (table, pageLengthInput) {
    AAGridFilter._table = $(table);
    AAGridFilter._tbody = AAGridFilter._table.children('tbody');
    AAGridFilter._pageLengthInput = $(pageLengthInput);

    AAGridFilter._table.find('th.sort').bind('click', function () {
        AAGridFilter._updatePagination();
    });
};

AAGridFilter.text = function (key, value) {
    AAGridFilter._texts[key] = value;
};