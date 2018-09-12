var Filters = new Object();

Filters.$ = {
    Module: null,
    Description: null,
    ViewCode: null,
    Status: null,
};

Filters.Parameters = {
    ModuleText: "",
    ModuleValue: 0,
    DescriptionText: "",
    DescriptionValue: 0,
    ViewCodeText: "",
    StatusValue: null,

    GetParameters: function () {
        this.ModuleText = Filters.$.Module.GetText();
        this.ModuleValue = Filters.CheckValue(Filters.$.Module.GetValue());
        this.DescriptionText = Filters.$.Description.GetText();
        this.DescriptionValue = Filters.CheckValue(Filters.$.Description.GetValue());
        this.ViewCodeText = Filters.$.ViewCode.val();
        this.StatusValue = Filters.$.Status.GetValue() !== '' ?
            !!+Filters.$.Status.GetValue() :
            null;
    
        return
    }
};

Filters["GetParameters"] = function () {
    return {
        ModuleText: this.$.Module.GetText(),
        ModuleValue: this.CheckValue(this.$.Module.GetValue()),
        DescriptionText: this.$.Description.GetText(),
        DescriptionValue: this.CheckValue(this.$.Description.GetValue()),
        ViewCodeText: this.$.ViewCode.val(),
        StatusValue: this.$.Status.GetValue() !== '' ? !!+this.$.Status.GetValue() : null
    }
}

Filters["CheckValue"] = function (value) {
    return value !== '' ? value : 0;
}

Filters["Clear"] = function () {
    this.$.Module.SetValue();
    this.$.Description.SetValue();
    this.$.ViewCode.val("");
    this.$.Status.FirstorDefault();
}

function searchFilter(button) {
    unbindDocumentAjaxActions();
    showLoader();

    Filters.Parameters.GetParameters();

    $.ajax({
        url: $(button).data('url'),
        cache: false,
        data: Filters.Parameters,
        async: true,
        type: 'GET',
        dataType: 'html'
    }).done(function (response) {
        DataController.Table.$.closest('#tableContent').html(response);

        var total = 0;

        if (response !== null)
            total = parseInt($("label[data-id='results']").text());

        DataController.SetUp("table[id='tableData']", total, true, true);

        DataController.IdRemove.forEach(function (id) {
            DataController.Table.HideRow(id);
        });

        DataController.ToggleDataControls();
        DataController.Pagination();
        DataController.Sort();

        hideLoader();
        bindDocumentAjaxActions();
    });
}

function clearFilter(button) {
    Filters.Clear();

    searchFilter(button);
}

function downloadReport(button) {
    var url = $(button).data('url') + '?' + jQuery.param(Filters.Parameters);

    window.open(url, '_blank');
}

$(document).ready(function () {
    Filters.$.Module = $('input[name="searchModule"]');
    Filters.$.Description = $('input[name="searchDescription"]');
    Filters.$.ViewCode = $('input[name="searchViewCode"]');
    Filters.$.Status = $('input[name="searchStatus"]');

    $(document).on('click', 'button[name="clearAll"]', function () {
        clearFilter(this);
    });

    $(document).on('click', 'button[name="searchFilter"]', function () {
        searchFilter(this);
    });
});