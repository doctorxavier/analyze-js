function openAdditionalPriorClausesModal(title) {
    var $AdditionalPriorClausesModal = $('#AdditionalPriorClausesModal');
    $AdditionalPriorClausesModal.removeClass('hide');

    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');

    var AdditionalPriorClausesModal = $AdditionalPriorClausesModal.kendoWindow({
        width: "750px",
        title: title,
        visible: true,
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
        }
    }).data("kendoWindow");

    AdditionalPriorClausesModal.center();
    AdditionalPriorClausesModal.open();
}

function searchAdditionalPriorClausesModal() {
    var number = $('[name=prior-filter-number]').val()
        .trim().toLowerCase().replace(/(\\W|\.)+/g, "");
    var description = $('[name=prior-filter-descript]').val()
        .trim().toLowerCase().replace(/(\\W|\.)+/g, "");
    var $clausesRows = $('#AdditionalPriorClausesTable')
        .find('tbody tr').removeClass('shown').show();

    if (number) {
        $clausesRows.filter('[data-search-number*="' + number + '"]').addClass('shown');
    }

    if (description) {
        $clausesRows.filter('[data-search-description*="' + description + '"]').addClass('shown');
    }

    if (!number && !description) {
        $clausesRows.addClass('shown');
    }

    $clausesRows.not('.shown').not('.selected').hide();
}

function checkAdditionalPriorClause(node) {
    var $hidden = $('#MultiplePriorClauseIndividualIds');
    var selected = $('#AdditionalPriorClausesTable [type=checkbox]:checked')
        .map(function () { return $(this).val(); }).get().join(',');
    $hidden.val(selected);

    var $span = $('[name="multi-prior-section-text"] span');
    $span.css('color', selected ? '#337ab7' : '');
    $span.css('font-weight', selected ? 'bold' : '');

    var $node = $(node);

    if($node.is(':checked')) {
        $node.closest('tr').addClass('selected');
    }
    else {
        $node.closest('tr').removeClass('selected');
    }
}