var EXECUTE_SUCCESS = 1;
var EXECUTE_NOT_HAS_PERMISSION = 0;

var GenericField = function (node) {

    var _node = $(node).closest('div.LabelsGroup');

    var _isChecked = function () {
        return _node.find('input[type="checkbox"]').prop("checked");
    };

    var _convertToSQLdate = function (date, defaultDate) {
        date = converToDate(date);

        if (!date && defaultDate)
            return _convertToSQLdate(defaultDate);

        return $.datepicker.formatDate('yy-mm-dd', date);
    };

    this.getValue = function () {
        var $input = _node.find('input[type="text"]');
        var value = $.trim($input.val());

        var isChecked = _isChecked();
        var isDatePicker = $input.hasClass('hasDatepicker');
        var isInputText = $input.hasClass('inputText');

        if ((isDatePicker || isInputText) && isChecked && value === "")
            return null;

        if (isDatePicker) {
            return _convertToSQLdate(value, $input.attr('dd-min-date'));
        }

        if (isInputText) {
            return value === "" ? " " : value;
        }

        value = parseInt($input.GetValue()) || $input.GetValue();

        value = value === "" ? 0 : value;

        if (isChecked && value === 0)
            return null;

        return value;
    };
};

var GenericCase = new Object();

GenericCase._adjustComponent = 37;
GenericCase._adjustDropdown = 24;
GenericCase.executeURL = "";

GenericCase._Fields = new Object();
GenericCase._AdittionalData = new Object();

GenericCase["_ReadAdditionalData"] = function () {
    $('div#aditional-data').find('input[data-field-name], textarea[data-field-name]')
        .each(function () {
            var $this = $(this);
            var key = $this.data('field-name');
            var value = $this.GetValue() || $this.val();

            if (value === "on")
                value = $this.prop("checked");

            GenericCase._AdittionalData[key] = value;
        });
}

GenericCase["_ReadFields"] = function () {
    $('div#generic-case-fields').find('[data-field-name]:visible').each(function () {
        var element = new GenericField(this);
        var fieldName = $(this).data('field-name');

        GenericCase._Fields[fieldName] = element.getValue();
    });
}

GenericCase["GetJson"] = function () {
    this._ReadAdditionalData();
    this._ReadFields();

    return $.extend({}, this._AdittionalData, this._Fields)
}

$.fn.adjustWidth = function () {
    $(this).each(function (key, value) {
        var adjust = GenericCase._adjustComponent;

        var $this = $(this);
        var $checkbox = $this.find('[name="checkbox"]');
        var $adjustElement = $this.find('input[name]:visible');

        if ($adjustElement.length === 0) {
            adjust = GenericCase._adjustDropdown;
            $adjustElement = $this.find('div.gc-adjust-display');
        }

        if ($adjustElement.length > 0 && $checkbox.length > 0) {
            var avalWidth = $this.width();
            var checkboxWidth = $checkbox.width();

            $adjustElement.width(avalWidth - checkboxWidth - adjust);
        }
    })

    return this;
}

function execute(button) {
    var validateAditionalData = true;

    $('#aditional-data').each(function (index, element) {
        validateAditionalData = validateContainer($(this));

        return validateAditionalData;
    });

    var validateFields = validateContainer($('#generic-case-fields'));

    if (!validateAditionalData || !validateFields)
        return;

    $.ajax({
        url: $('button[name="execute"]').data("url"),
        cache: false,
        data: GenericCase.GetJson(),
        async: true,
        type: 'POST'
    }).done(function (response) {
        if (!response.IsValid) {
            showMessage(response.ErrorMessage);

            return;
        }

        showMessage(response.Model.Message);

        if (response.Model.ExitCode === EXECUTE_NOT_HAS_PERMISSION ||
            response.Model.ExitCode === EXECUTE_SUCCESS)
            clearAll();
    });
}

function toggleEnabled(element) {
    var $content = $(element).closest('div');
    var $element = $content.find('input[type="text"]:visible');

    if ($element.length === 0)
        $element = $content.find('button:visible');

    $element.prop('disabled', function (index, status) {
        $element.val("");
        $element.FirstorDefault();

        if (status) {
            $element.closest('div.dropdown').removeClass('gc-disabled');
        }
        else {
            $element.closest('div.dropdown').addClass('gc-disabled');
            $content.find('ul.filled').removeClass('filled');
        }

        return !status;
    });
}

function clearFields(contents) {
    $(contents).each(function (index, content) {
        var $content = $(content);

        $content.find('input[name][type="checkbox"]').each(function (index, element) {
            var $this = $(this);
            var $content = $this.closest('div.lgCont');

            $this.prop("checked", false);

            $content.find('input[type="text"]:visible').prop("disabled", true);
            $content.find('button:visible').prop("disabled", true);
            $('div.gc-adjust-display').addClass('gc-disabled');
        });

        $content.find('input[data-keep="false"]')
            .siblings('[aria-labelledby]')
            .find('li:not(:eq(0))')
            .remove();
    });

    $('div.dropdown').FirstorDefault();
    $('input[name][type="text"], textarea[name]').val("");

    $('.filled').removeClass('filled');
    $('.parsley-error').removeClass('parsley-error');
}

function ToSelectListItems(data) {
    var response = new Object();

    if (data == null)
        return response;

    $.each(data, function (key, value) {
        response[key] = value.Number
    });

    return response;
}

function addDataToDropdown(collectionData, dropdownDataOptions) {
    dropdownDataOptions.FirstorDefault();

    dropdownDataOptions.find('li:not(:eq(0))').remove();

    if (collectionData !== null)
        $.each(collectionData, function (key, value) {
            var $listItem = $('<li />').appendTo(dropdownDataOptions);

            $('<a />').attr('dd-value', key).html(value).appendTo($listItem);
        });
};

$(document).ready(function () {
    $('input[name|="update"]').on('click', function (event) {
        var $this = $(event.target);
        var $parent = $this.closest('div.lgCont');

        $this.prop('checked', function (index, status) {
            $parent.find('[data-required]').attr('data-parsley-required', status)
        })

    });

    $('input[name="transaction-type"]').on('change', function (event) {
        changeTransactionType(event.target);
    })
});