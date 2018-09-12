var ReformulationTabs = new Object();

ReformulationTabs.TabsContainer = $('.tabs-reformulation');
ReformulationTabs.Buttons = ReformulationTabs.TabsContainer.find('.btn');
ReformulationTabs.BlocksContainer = $('.reformulation-group-content');
ReformulationTabs.HideButtons = $('button[data-action=edit]').
    add('[data-action=tryToEditApproval]').
    add('[data-id=ApprovalOperationDocuments-add]').
    add('[name=CheckFundAvailability]').
    add('[name=ChkFundAvailability]').
    add('[data-action=editDocuments]').
    add('[data-action=editRemarks]').
    not('[data-idb-fieldname=btnEditRspb]');

ReformulationTabs.updateSelected = function (selector) {

    ReformulationTabs.TabsContainer.find('.btn.active').removeClass('active');
    ReformulationTabs.TabsContainer.find('[dd-tab=' + selector + ']').addClass('active');

    ReformulationTabs.BlocksContainer.children('.tab-pane').hide();
    ReformulationTabs.BlocksContainer.children(selector).show();

    if (ReformulationTabs.TabsContainer.length > 0) {

        ReformulationTabs.HideButtons.css({
            'visibility': ReformulationTabs.isEditable() ? 'visible' : 'hidden'
        });

        if ($('.fdp-page-content').length > 0) {

            $('.fdp-page-content').removeAttr('id');
            $('.financial-data-preparation-content:visible').
                        find('.fdp-page-content').
                        attr('id', 'PageContent');
            refreshValues();
            grafico();
            coFinancingTableOperations.collapsesCall();
        }
    }
};

ReformulationTabs.isCurrent = function () {

    if (ReformulationTabs.Buttons.length > 0) {
        return ReformulationTabs.TabsContainer.find('.btn.active').is('.is-current');
    }

    return true;
};

ReformulationTabs.isEditable = function () {

    if (ReformulationTabs.Buttons.length > 0) {
        return ReformulationTabs.TabsContainer.find('.btn.active').is('.is-editable');
    }

    return true;

};

ReformulationTabs.GetActivityPlanId = function () {

    if (ReformulationTabs.Buttons.length > 0) {
        var selector = ReformulationTabs.TabsContainer.find('.btn.active').attr('dd-tab');
        var container = $(selector);
        return container.attr('data-activity-plan-id');
    }

    return null;

};

ReformulationTabs.GetCurrentContainer = function () {

    if (ReformulationTabs.Buttons.length > 0) {
        var selector = ReformulationTabs.TabsContainer.find('.btn.active').attr('dd-tab');
        return $(selector);
    }

    return $('[data-activity-plan-id]');
};

ReformulationTabs.DisableButtons = function () {
    ReformulationTabs.Buttons.not('.active').
        addClass('disabled').
        prop('disabled', true);
};

ReformulationTabs.EnableButtons = function () {
    ReformulationTabs.Buttons.not('.active').
        removeClass('disabled').
        prop('disabled', false);
};

ReformulationTabs.Buttons.on('click', function () {
    ReformulationTabs.updateSelected($(this).attr('dd-tab'));
});

ReformulationTabs.updateSelected(
    ReformulationTabs.TabsContainer.find('.is-current').attr('dd-tab')
);

$(document).ready(function () {

    var maxWidth = 0;

    ReformulationTabs.Buttons.each(function (index, item) {

        var width = $(item).width();

        if (width > maxWidth) {
            maxWidth = width;
        }
    });

    ReformulationTabs.Buttons.width(maxWidth);

});