var _HasSelectedOperation = false;
var ContractAndClauses = new Object();

ContractAndClauses._Contracts = new Object();

ContractAndClauses["FillData"] = function (data) {
    ContractAndClauses._Contracts = new Object();

    if (data.ContractsWithAllExtensionsApproved !== null) {
        $.each(data.ContractsWithAllExtensionsApproved, function () {
            var clauses = new Object();

            $.each(this.Clauses, function () {
                var extensions = new Object();

                $.each(this.ClauseExtensions, function () {
                    extensions[this.Id] = {
                        Number: this.Number
                    };
                })

                clauses[this.Id] = {
                    Number: this.Number,
                    Extensions: extensions
                };
            });

            ContractAndClauses._Contracts[this.Id] = {
                Number: this.Number,
                Clauses: clauses
            };
        });
    }
}

function clearAll() {
    ContractAndClauses._Contracts = new Object();

    $('input[name="transaction-type"]').prop('checked', false);

    clearFields('#generic-case-fields');
    disabledAllFields();
}

function changeTransactionType(element) {
    if (!_HasSelectedOperation) {
        $('input[name="transaction-type"]').prop('checked', false);

        return;
    }

    disabledAllFields();

    var $element = $(element);
    var $target = $($element.data('target'));

    $target.find('.lgCont').each(function () {
        var $this = $(this);
        var $input = $this.find('button:visible');
        var $required = $this.find('[data-required]');

        $input.prop("disabled", false);

        $required.attr('data-parsley-required', true);
    });
}

function disabledAllFields() {
    var $all = $('#workflow-fields, #elegibility-fields, #clause-extension-fields');

    $all.find('.lgCont').each(function () {
        var $this = $(this);
        var $input = $this.find('[data-field-name]:visible');
        var $required = $this.find('[data-required]');

        $input.prop("disabled", true);
        $input.FirstorDefault();

        $required.attr('data-parsley-required', false);
        $('.filled').removeClass('filled');
    });
}

$(document).ready(function () {
    var $extensionLabel = $('#transaction-type-extension').parent();
    var $transactionTypeError = $extensionLabel.prev('ul');

    if ($transactionTypeError.length > 0)
        $transactionTypeError.detach().insertAfter($extensionLabel);

    $('input[name="operation-number"]').on('change', function (event) {
        var $this = $(event.target);
        var operationId = $this.GetValue();

        _HasSelectedOperation = $.isNumeric(operationId);

        if (_HasSelectedOperation) {
            $.ajax({
                url: $this.siblings('[data-url]').data('url'),
                cache: false,
                data: { 'operationId': operationId },
                async: true,
                type: 'GET'
            }).done(function (response) {
                $('input[name="transaction-type"]').prop('checked', false);

                disabledAllFields();

                if (!response.IsValid) {
                    showMessage(response.ErrorMessage);

                    return;
                }

                addDataToDropdown(
                    ToSelectListItems(response.WorkflowsIntance),
                    $('[aria-labelledby="id-workflow-folio-id"]'));

                collectionData = ToSelectListItems(response.Contracts);

                addDataToDropdown(
                    ToSelectListItems(response.Contracts),
                    $('[aria-labelledby="id-elegibility-contract-number"]'));

                ContractAndClauses.FillData(response);

                collectionData = ToSelectListItems(ContractAndClauses._Contracts);

                addDataToDropdown(
                    ToSelectListItems(ContractAndClauses._Contracts),
                    $('[aria-labelledby="id-extension-contract-number"]'));
            });
        }
    });

    $('input[name="extension-contract-number"]').on('change', function (event) {
        var $this = $(event.target);
        var contractId = $this.GetValue();

        if ($.isNumeric(contractId)) {
            addDataToDropdown(
                ToSelectListItems(ContractAndClauses
                    ._Contracts[contractId]
                    .Clauses),
                $('[aria-labelledby="id-extension-clause-number"]'));

            return;
        }

        addDataToDropdown(null, $('[aria-labelledby="id-extension-clause-number"]'));
        addDataToDropdown(null, $('[aria-labelledby="id-extension-number"]'));
    });

    $('input[name="extension-clause-number"]').on('change', function (event) {
        var $this = $(event.target);
        var clauseId = $this.GetValue();

        if ($.isNumeric(clauseId)) {
            var contractId = $('input[name="extension-contract-number"]').GetValue();

            addDataToDropdown(
                ToSelectListItems(ContractAndClauses
                    ._Contracts[contractId]
                    .Clauses[clauseId]
                    .Extensions),
                $('[aria-labelledby="id-extension-number"]'));

            return;
        }

        addDataToDropdown(null, $('[aria-labelledby="id-extension-number"]'));

    });

    $('input[data-target-reset]').on('change', function (event) {
        var $this = $(event.target);
        var target = $this.data('target-reset');

        $('input[name=' + target + ']').FirstorDefault()
    });

    $(window).on('resize', function () {
        $('.adjustable-element').adjustWidth();
    });

    $('.adjustable-element').adjustWidth();
});