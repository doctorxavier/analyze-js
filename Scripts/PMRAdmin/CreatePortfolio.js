var PMRPortfolioGrid = new Object();

PMRPortfolioGrid._table = null;
PMRPortfolioGrid._pageLength = null;
PMRPortfolioGrid._displayedPmrCycleId = null;
PMRPortfolioGrid._downloadPortfolioUrl = null;
PMRPortfolioGrid._resetPortfolioUrl = null;
PMRPortfolioGrid._savePortfolioUrl = null;
PMRPortfolioGrid._isExceptionCheckboxes = null;
PMRPortfolioGrid._commentIconButtons = null;
PMRPortfolioGrid._blueButtons = null;
PMRPortfolioGrid._excelButton = null;
PMRPortfolioGrid._justificationLabel = null;
PMRPortfolioGrid._valueRequiredMessage = null;
PMRPortfolioGrid._saveToDownloadMessage = null;
PMRPortfolioGrid._stringifiedOriginalTable = null;

PMRPortfolioGrid._setupPagination = function () {
    PMRPortfolioGrid._table.paginationConfluense(PMRPortfolioGrid._pageLength);
};

PMRPortfolioGrid._validateObservationForException = function (isFirstLoad) {
    var justifContainer = $('.justificationContainer');

    if (justifContainer.children('#justificationPmr').val().trim() == '') {
        $('input.btn.btn-warning.vex-dialog-button.vex-first').prop('disabled', true);

        if (!isFirstLoad) {
            justifContainer.children('ul.parsley-errors-list').addClass('filled');
            justifContainer.children('#justificationPmr').addClass('parsley-error');
        }
    } else {
        PMRPortfolioGrid._cleanObservationsValidation();
    }
};

PMRPortfolioGrid._cleanObservationsValidation = function () {
    var justifContainer = $('.justificationContainer');

    $('input.btn.btn-warning.vex-dialog-button.vex-first').prop('disabled', false);
    justifContainer.children('ul.parsley-errors-list').removeClass('filled');
    justifContainer.children('#justificationPmr').removeClass('parsley-error');
};

PMRPortfolioGrid._initEventHandlers = function () {
    PMRPortfolioGrid._table.find('button.commentIcon').on('click', function() {
        var observations = $(this).find('span.generalCommentsContainer').text().trim();

        showMessage(observations.replace(new RegExp('\r?\n', 'g'), '<br />'));
    });

    PMRPortfolioGrid._table.find('[name=isException]').on('click', function () {
        var $this = $(this);
        var isChecked = $(this).is(':checked');

        $this.attr('checked', isChecked);

        var parent = $this.closest('tr');
        var generalComments = parent.find('.generalCommentsContainer');
        var currentCommentText = generalComments.text().trim();

        var container = $('<div />').attr({ 'class': 'justificationContainer' });
        var label = $('<label />').attr({ 'for': 'comment' }).
            text(PMRPortfolioGrid._justificationLabel).appendTo(container);

        if (isChecked) {
            var asterisk = $('<span />').attr({ 'class': 'asteriskColor' }).
                text('*').appendTo(label);
            var textArea = $('<textarea />').
                attr({
                    'class': 'form-control',
                    'rows': '5',
                    'id': 'justificationPmr',
                    'onkeyup': 'PMRPortfolioGrid._validateObservationForException(false)',
                    'focus': 'PMRPortfolioGrid._cleanObservationsValidation()'
                }).
                text(currentCommentText).appendTo(container);
            var parsleyUl = $('<ul />').attr({ 'class': 'parsley-errors-list' }).
                appendTo(container);
            var parsleyLi = $('<li />').attr({ 'class': 'parsley-required' }).
                text(PMRPortfolioGrid._valueRequiredMessage).appendTo(parsleyUl);

            confirmAction(container[0].outerHTML)
                .notify()
                .progress(function () { PMRPortfolioGrid._validateObservationForException(true); })
                .done(function (ok) {
                    if (ok) {
                        generalComments.html($('#justificationPmr').val().trim());
                        parent.find('.commentIcon').removeClass('hide');
                    } else {
                        $this.prop('checked', !isChecked);
                    }
                });
        }
        else {
            var textArea = $('<textarea />').
                attr({
                    'class': 'form-control',
                    'rows': '5',
                    'id': 'justificationPmr'
                }).
                text(currentCommentText).appendTo(container);

            confirmAction(container[0].outerHTML)
                .done(function (ok) {
                    if (ok) {
                        var updatedObservation = $('#justificationPmr').val().trim();

                        generalComments.html(updatedObservation);

                        if (updatedObservation == '') {
                            parent.find('.commentIcon').addClass('hide');
                        }
                    } else {
                        $this.prop('checked', !isChecked);
                    }
                });
        }
    });

    PMRPortfolioGrid._table.find('th.sort').off('click', PMRPortfolioGrid._initEventHandlers);
    PMRPortfolioGrid._table.find('th.sort').on('click', PMRPortfolioGrid._initEventHandlers);
};

PMRPortfolioGrid._disableExceptionAndCommentIcons = function () {
    PMRPortfolioGrid._isExceptionCheckboxes.prop("disabled", true);
    PMRPortfolioGrid._commentIconButtons.prop("disabled", true);
};

PMRPortfolioGrid._enableExceptionAndCommentIcons = function () {
    PMRPortfolioGrid._isExceptionCheckboxes.prop("disabled", false);
    PMRPortfolioGrid._commentIconButtons.prop("disabled", false);
};

PMRPortfolioGrid._disableButtons = function () {
    PMRPortfolioGrid._blueButtons.prop("disabled", true);
    PMRPortfolioGrid._excelButton.prop("disabled", true);
};

PMRPortfolioGrid._enableButtons = function () {
    PMRPortfolioGrid._blueButtons.prop("disabled", false);
    PMRPortfolioGrid._excelButton.prop("disabled", false);
};

PMRPortfolioGrid._successCallback = function (data) {
    if (data.IsValid) {
        showNotification({
            message: data.Message,
            type: "success",
            autoClose: true,
            duration: 5
        });

        window.setTimeout(function () {
            PMRPortfolioGrid._disableButtons();
            showLoader();
            window.location.reload();
        }, 400);
    }
    else {
        showNotification({
            message: data.ErrorMessage,
            type: "error",
            autoClose: false,
            duration: 2
        });

        PMRPortfolioGrid._enableExceptionAndCommentIcons();
        PMRPortfolioGrid._enableButtons();
    }
};

PMRPortfolioGrid.Init = function (table, pageLength, displayedPmrCycleId, justifLabel, valueRequiredMsg, saveToDownloadMsg) {
    PMRPortfolioGrid._table = $(table);
    PMRPortfolioGrid._table.sortableConfluence();
    PMRPortfolioGrid._pageLength = pageLength;
    PMRPortfolioGrid._displayedPmrCycleId = displayedPmrCycleId;
    PMRPortfolioGrid._isExceptionCheckboxes = $('[name="isException"]');
    PMRPortfolioGrid._commentIconButtons = $(".commentIcon");
    PMRPortfolioGrid._blueButtons = $('button.buttonBlue');
    PMRPortfolioGrid._excelButton = $('button.buttonExcel');
    PMRPortfolioGrid._justificationLabel = justifLabel;
    PMRPortfolioGrid._valueRequiredMessage = valueRequiredMsg;
    PMRPortfolioGrid._saveToDownloadMessage = saveToDownloadMsg;
    PMRPortfolioGrid._stringifiedOriginalTable =
        JSON.stringify($.map(PMRPortfolioGrid._table.find('input[type=checkbox]')
        .toArray(), function (item, index) { return $(item).is(':checked'); }));

    PMRPortfolioGrid._setupPagination();
    PMRPortfolioGrid._initEventHandlers();
    PMRPortfolioGrid._table.find('th.sort').bind('click', PMRPortfolioGrid._setupPagination);
};

PMRPortfolioGrid.DownloadPortfolio = function () {
    var stringifiedCurrentTable =
        JSON.stringify($.map(PMRPortfolioGrid._table.find('input[type=checkbox]')
        .toArray(), function (item, index) { return $(item).is(':checked'); }));

    if (stringifiedCurrentTable != PMRPortfolioGrid._stringifiedOriginalTable) {
        showMessage(PMRPortfolioGrid._saveToDownloadMessage);
    }
    else {
        window.open(PMRPortfolioGrid._downloadPortfolioUrl, '_blank');
    }
};

PMRPortfolioGrid.ResetPortfolio = function () {
    PMRPortfolioGrid._disableExceptionAndCommentIcons();
    PMRPortfolioGrid._disableButtons();

    $.ajax({
        type: "POST",
        url: PMRPortfolioGrid._resetPortfolioUrl,
        data: JSON.stringify(PMRPortfolioGrid._displayedPmrCycleId),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            PMRPortfolioGrid._successCallback(data);
        }
    });
};

PMRPortfolioGrid.SavePortfolio = function (isSaveAsDraft) {
    PMRPortfolioGrid._disableExceptionAndCommentIcons();
    PMRPortfolioGrid._disableButtons();

    var createPortfolioEntryViewModels = [];

    PMRPortfolioGrid._table.find('tbody > tr').each(function (i, obj) {
        var $tr = $(obj);
        var $columns = $tr.children();
        var roleCode = $columns.find('[data-name=rolePmrRelation]').attr('data-role-code');

        var createPortfolioEntryObject = new Object();
        createPortfolioEntryObject.CreatePortfolioEntryId = parseInt($tr.attr('data-id'));
        createPortfolioEntryObject.OperationNumber = $columns.find('[data-name=operationNumber]').text().trim();
        createPortfolioEntryObject.IsPCRActive = $columns.find('[name=pcrActive]').is(':checked');
        createPortfolioEntryObject.IsPMRRequired = $columns.find('[name=pmrRequired]').is(':checked');
        createPortfolioEntryObject.IsMarchClassificationRequired = $columns.find('[name=marchClassificationRequired]').is(':checked');
        createPortfolioEntryObject.IsPMRRelated = $columns.find('[name=pmrRelated]').is(':checked');
        createPortfolioEntryObject.RolePMRRelationCode = roleCode === undefined ? '' : roleCode;
        createPortfolioEntryObject.OperationsRelated = $columns.find('[data-name=operationsRelated]').text().trim();
        createPortfolioEntryObject.IsException = $columns.find('[name=isException]').is(':checked');
        createPortfolioEntryObject.Observations = $columns.find('.generalCommentsContainer').text().trim();

        createPortfolioEntryViewModels.push(createPortfolioEntryObject);
    });

    var objectToPost = new Object();
    objectToPost.isSaveAsDraft = isSaveAsDraft;
    objectToPost.displayedPmrCycleId = PMRPortfolioGrid._displayedPmrCycleId;
    objectToPost.createPortfolioEntryViewModels = createPortfolioEntryViewModels;

    $.ajax({
        type: "POST",
        url: PMRPortfolioGrid._savePortfolioUrl,
        data: JSON.stringify(objectToPost),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            PMRPortfolioGrid._successCallback(data);
        }
    });
};

PMRPortfolioGrid.RemoveCreatePortfolioLoader = function (portfolioLoaderDataProp, portfolioLoaderDataValue) {
    $('[data-' + portfolioLoaderDataProp + '="' + portfolioLoaderDataValue + '"]').remove();
};
