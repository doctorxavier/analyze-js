var DataController = new Object();

DataController.Inputs = {
    $: {
        Module: null,
        DescriptionsEn: null,
        DescriptionsEs: null,
        DescriptionsPt: null,
        DescriptionsFr: null,
        UrlEn: null,
        UrlEs: null,
        UrlPt: null,
        UrlFr: null,
        IsActive: null
    },

    Set: function () {
        this.$.Module = $('input[name="module"]');
        this.$.DescriptionsEn = $('input[name="descriptionEN"]');
        this.$.DescriptionsEs = $('input[name="descriptionES"]');
        this.$.DescriptionsPt = $('input[name="descriptionPT"]');
        this.$.DescriptionsFr = $('input[name="descriptionFR"]');
        this.$.UrlEn = $('textarea[name="urlEN"]');
        this.$.UrlEs = $('textarea[name="urlES"]');
        this.$.UrlPt = $('textarea[name="urlPT"]');
        this.$.UrlFr = $('textarea[name="urlFR"]');
        this.$.IsActive = $('input[name="status"]');
    }
}

DataController.Table = {
    $: null,

    Find: function (selector) {
        return this.$.find(selector);
    },

    SelectRows: function (id) {
        if (id !== undefined) {
            return this.Find("tbody tr[id='" + id + "']");
        }

        return this.Find("tbody tr");
    },

    ShowRow: function (id) {
        var $row = this.SelectRows(id);

        $row.attr("data-id", id);
        $row.removeClass("help-hide");
    },

    HideRow: function (id) {
        var $row = this.SelectRows(id);

        $row.removeAttr("data-id");
        $row.addClass("help-hide");
    }
};

DataController.IdRemove = [];
DataController.DetailsData = {
    Id: "",
    ModuleText: "",
    ModuleId: 0,
    Descriptions: {
        English: "",
        Spanish: "",
        Portuguese: "",
        French: ""
    },
    Urls: {
        English: "",
        Spanish: "",
        Portuguese: "",
        French: ""
    },
    IsActive: true,
    Alias: []
};

DataController.HasPagination = false;
DataController.HasSort = false;
DataController.TotalResults = 0;
DataController.ItemsPerPage = 10;

DataController['SetUp'] = function (table, total, paginator, sort) {
    this.Inputs.Set();
    this.Table.$ = $(table);
    this.HasPagination = paginator;
    this.HasSort = sort;
    this.TotalResults = total;
}

DataController['AddToDelete'] = function (id) {
    this.IdRemove.push(id);
}

DataController['WillBeDeleted'] = function (id) {
    return DataController.IdRemove.contains(parseInt(id));
}

DataController['Results'] = function (value) {
    this.Total = value;
}

DataController['SetDetailsData'] = function (data) {

    if (data === undefined) {
        data = this.GetDetailsData();
    }

    this.DetailsData.ModuleId = data.ModuleId;
    this.DetailsData.ModuleText = data.ModuleText;
    this.DetailsData.Descriptions = data.Descriptions;
    this.DetailsData.Urls = data.Urls;
    this.DetailsData.IsActive = data.IsActive;
    this.DetailsData.Alias = [];

    data.Alias.forEach(function (alias) {
        DataController.DetailsData.Alias.push(
            { Id: alias.Id, ViewCode: alias.ViewCode });
    });
}

DataController['GetDetailsData'] = function () {
    var data = new Object({
        Id: this.DetailsData.Id,
        ModuleId: this.Inputs.$.Module.GetValue(),
        ModuleText: this.Inputs.$.Module.GetText(),
        Descriptions: {
            English: this.Inputs.$.DescriptionsEn.val(),
            Spanish: this.Inputs.$.DescriptionsEs.val(),
            Portuguese: this.Inputs.$.DescriptionsPt.val(),
            French: this.Inputs.$.DescriptionsFr.val()
        },
        Urls: {
            English: this.Inputs.$.UrlEn.val(),
            Spanish: this.Inputs.$.UrlEs.val(),
            Portuguese: this.Inputs.$.UrlPt.val(),
            French: this.Inputs.$.UrlFr.val()
        },
        IsActive: this.Inputs.$.IsActive.is(':checked'),
        Alias: []
    });

    if (DataController.IdRemove.length === 0) {
        return data;
    }

    this.Table.Find('label[data-name="viewCode"]').each(function (index, element) {
        var id = $(element).data('id');

        if (DataController.WillBeDeleted(id))
        {
            data.Alias.push({
                ViewId: data.Id,
                Id: id,
                ViewCode: $(element).text()
            });
        }
    });

    return data;
}

DataController['DeleteRow'] = function (item) {
    var $row = item.closest('tr');

    confirmAction(messages.Delete).done(function (confirm) {
        if (confirm) {
            var id = $row.data('id');

            if (id === 0) {
                $row.remove();
            } else {
                DataController.AddToDelete(id);
                DataController.Table.HideRow(id);
            }
            
            DataController.TotalResults--;
            DataController.ToggleDataControls();

            $("label[data-id='results']").text(DataController.TotalResults);

            DataController.Pagination();
        }
    });
}

DataController['Edit'] = function () {
    enterEditMode(false, $("#pageModeButtons, #pageContent"), false, false);

    $('input[name="status"]').removeAttr("disabled");
}

DataController['Cancel'] = function (isDetails) {
    confirmAction(messages.Cancel).done(function (confirm) {
        if (confirm) {
            if (isDetails && DataController.HasDetailsChanged()) {
                DataController.Inputs.$.Module.SetValue(
                    DataController.DetailsData.ModuleId, DataController.DetailsData.ModuleText);

                DataController.Inputs.$.DescriptionsEn.val(
                    DataController.DetailsData.Descriptions.English);
                DataController.Inputs.$.DescriptionsEs.val(
                    DataController.DetailsData.Descriptions.Spanish);
                DataController.Inputs.$.DescriptionsPt.val(
                    DataController.DetailsData.Descriptions.Portuguese);
                DataController.Inputs.$.DescriptionsFr.val(
                    DataController.DetailsData.Descriptions.French);

                DataController.Inputs.$.UrlEn.val(DataController.DetailsData.Urls.English);
                DataController.Inputs.$.UrlEs.val(DataController.DetailsData.Urls.Spanish);
                DataController.Inputs.$.UrlPt.val(DataController.DetailsData.Urls.Portuguese);
                DataController.Inputs.$.UrlFr.val(DataController.DetailsData.Urls.French);

                DataController.Inputs.$.IsActive.attr(
                    'checked', DataController.DetailsData.IsActive);
            }

            if (!DataController.HasTableChanged()) {
                DataController.ExitEditMode();

                return;
            }

            DataController.IdRemove.forEach(function (id) {
                DataController.Table.ShowRow(id);
            });

            DataController.TotalResults =
                DataController.TotalResults + DataController.IdRemove.length;
            DataController.IdRemove = [];
            DataController.Pagination();

            $("div[id='emptyTable']").addClass("hide");
            $("label[data-id='results']").text(DataController.TotalResults);

            $('tr[data-id="0"]').each(function (index, element) {
                $(element).remove();

                DataController.TotalResults--;
                DataController.ToggleDataControls();

                $("label[data-id='results']").text(DataController.TotalResults);
            })

            DataController.ExitEditMode();
        }
    });
}

DataController['SaveIndex'] = function (button) {
    confirmAction(messages.Save).done(function (confirm) {
        if (confirm) {
            unbindDocumentAjaxActions();
            showLoader();

            $.ajax({
                url: DataController.GetUrlAction(button),
                cache: false,
                data: { ids: DataController.IdRemove },
                async: true,
                type: 'POST'
            }).done(function (response) {

                if (!response.IsValid) {
                    showMessage(response.ErrorMessage);

                    hideLoader();
                    bindDocumentAjaxActions();

                    return;
                }

                $.each(response.Model.Alias, function (key, value) {
                    window.parent.ConvergenceHelp.Remove(value);
                });

                DataController.IdRemove.forEach(function (id) {
                    DataController.Table.SelectRows(id).remove();
                });

                DataController.IdRemove = [];
                DataController.ExitEditMode();

                hideLoader();
                bindDocumentAjaxActions();

                showMessage(response.Model.Message);
            });
        }
    });
}

DataController['SaveDetails'] = function (button) {
    if (DataController.ValidateUrls() && validateContainer($('#pageContent'))) {
        confirmAction(messages.Save).done(function (confirm) {
            if (confirm) {

                showLoader();
                unbindDocumentAjaxActions();

                $.ajax({
                    url: DataController.GetUrlAction(button),
                    cache: false,
                    data: DataController.GetDetailsData(),
                    async: true,
                    type: 'POST'
                }).done(function (response) {

                    if (!response.IsValid) {
                        showMessage(response.ErrorMessage);

                        hideLoader();
                        bindDocumentAjaxActions();

                        return;
                    }

                    var data = DataController.GetDetailsData();
                    var $module = $('div[id="module"]');
                    var $descriptions = $('div[id="descriptions"]');
                    var $urls = $('div[id="urls"]');
                    var $cancel = $('button[name="cancel"]');

                    $cancel.attr('data-action', "cancelDetails");
                    $cancel.removeAttr('data-navigate');
                    
                    $module.find('label:hidden').text(data.ModuleText);

                    $descriptions.find('div[dd-lang="en"] input:hidden:first')
                        .val(data.Descriptions.English);
                    $descriptions.find('div[dd-lang="es"] input:hidden:first')
                        .val(data.Descriptions.Spanish);
                    $descriptions.find('div[dd-lang="pt"] input:hidden:first')
                        .val(data.Descriptions.Portuguese);
                    $descriptions.find('div[dd-lang="fr"] input:hidden:first')
                        .val(data.Descriptions.French);

                    $urls.find('div[dd-lang="en"] textarea:hidden:first')
                        .val(data.Urls.English);
                    $urls.find('div[dd-lang="es"] textarea:hidden:first')
                        .val(data.Urls.Spanish);
                    $urls.find('div[dd-lang="pt"] textarea:hidden:first')
                        .val(data.Urls.Portuguese);
                    $urls.find('div[dd-lang="fr"] textarea:hidden:first')
                        .val(data.Urls.French);

                    DataController.Inputs.$.IsActive.attr('checked', data.IsActive);

                    DataController.Table.Find('tr:hidden').remove();
                    DataController.IdRemove = [];

                    DataController.SetDetailsData();
                    DataController.ExitEditMode();

                    hideLoader();
                    bindDocumentAjaxActions();

                    showMessage(response.Model.Message);
                });
            }
        });
    }
}

DataController['NewAliasRow'] = function (button) {
    $.ajax({
        url: DataController.GetUrlAction(button),
        cache: false,
        async: true,
        type: 'GET',
        dataType: 'html'
    }).done(function (data) {

        if (data !== "") {
            DataController.TotalResults++;
            DataController.ToggleDataControls();

            DataController.Table.$.append(data);
        }
    });
}

DataController['Pagination'] = function () {
    if (this.HasPagination) {
        var paginationData = this.Table.$.objectSortableConfluence();
        var $pagination = $(".Pagination");

        this.Table.$.paginationConfluence(this.ItemsPerPage, paginationData.pagination);

        $pagination.removeAttr("style");
        $pagination.css({ "min-width": "0%", "width": "100%", "max-width": "100%" });

        alternateRowColor(this.Table.$);
    }
}

DataController['Sort'] = function () {
    if (this.HasSort) {
        this.Table.$.sortableConfluence();
    }
}

DataController['ToggleDataControls'] = function () {
    var $emptyMessage = $("div[id='emptyTable']");
    var $tablePagination = $("div[dd-table='DataController']");

    if (this.TotalResults > 0) {
        $emptyMessage.addClass("hide");
        $tablePagination.removeClass("hide");
    } else {
        $emptyMessage.removeClass("hide");
        $tablePagination.addClass("hide");
    }
}

DataController['HasTableChanged'] = function () {
    var hasNewItem = this.Table.Find("tr[data-id='0']").length > 0;

    if (!(DataController.IdRemove.length > 0) && !hasNewItem) {

        return false
    }

    return true
}

DataController['HasDetailsChanged'] = function () {

    if (this.Inputs.$.Module.GetValue !== DataController.DetailsData.ModuleId)
        return true;

    if (this.Inputs.$.DescriptionsEn.val() !== DataController.DetailsData.Descriptions.English)
        return true

    if (this.Inputs.$.DescriptionsEs.val() !== DataController.DetailsData.Descriptions.Spanish)
        return true;

    if (this.Inputs.$.DescriptionsPt.val() !== DataController.DetailsData.Descriptions.Portuguese)
        return true;

    if (this.Inputs.$.DescriptionsFr.val() !== DataController.DetailsData.Descriptions.French)
        return true;

    if (this.Inputs.$.UrlEn.val() !== DataController.DetailsData.Urls.English)
        return true;

    if (this.Inputs.$.UrlEs.val() !== DataController.DetailsData.Urls.Spanish)
        return true;

    if (this.Inputs.$.UrlPt.val() !== DataController.DetailsData.Urls.Portuguese)
        return true;

    if (this.Inputs.$.UrlFr.val() !== DataController.DetailsData.Urls.French)
        return true;

    return false;
}

DataController['GetUrlAction'] = function (button) {
    return $(button).data('url')
}

DataController['ExitEditMode'] = function () {
    $('div[id="descriptions"]').find('li[dd-lang="en"]').click();
    $('div[id="urls"]').find('li[dd-lang="en"]').click();

    $('input[name="status"]').attr("disabled", "disabled");
    $('.error').removeClass('error');
    $('.parsley-errors-list').removeClass('filled');

    var $inputsError = $('.parsley-error');

    $inputsError.addClass('parsley-success');
    $inputsError.removeClass('parsley-error');

    exitEditMode(false, $("#pageModeButtons, #pageContent"), false, false);
}

DataController['ValidateUrls'] = function () {
    var response = true;

    if (!this.IsValidUrl(this.Inputs.$.UrlEn.val())) {
        this.Inputs.$.UrlEn.addClass('parsley-error');
        $('div#urls li[dd-lang="en"]').addClass("error");

        response = false;
    }
    else {
        this.Inputs.$.UrlEn.removeClass('parsley-error');
        $('div#urls li[dd-lang="en"]').removeClass("error");
    }

    if (!this.IsValidUrl(this.Inputs.$.UrlEs.val())) {
        this.Inputs.$.UrlEs.addClass('parsley-error');
        $('div#urls li[dd-lang="es"]').addClass("error");

        response = false;
    }
    else {
        this.Inputs.$.UrlEs.removeClass('parsley-error');
        $('div#urls li[dd-lang="es"]').removeClass("error");
    }

    if (!this.IsValidUrl(this.Inputs.$.UrlPt.val())) {
        this.Inputs.$.UrlPt.addClass('parsley-error');
        $('div#urls li[dd-lang="pt"]').addClass("error");

        response = false;
    }
    else {
        this.Inputs.$.UrlPt.removeClass('parsley-error');
        $('div#urls li[dd-lang="pt"]').removeClass("error");
    }

    if (!this.IsValidUrl(this.Inputs.$.UrlFr.val())) {
        this.Inputs.$.UrlFr.addClass('parsley-error');
        $('div#urls li[dd-lang="fr"]').addClass("error");

        response = false;
    }
    else {
        this.Inputs.$.UrlFr.removeClass('parsley-error');
        $('div#urls li[dd-lang="fr"]').removeClass("error");
    }

    return response;
}

DataController['IsValidUrl'] = function (value) {
    if (value == "")
        return true;

    var pattern = /^https?\:\/\/(?:(?:\w+)\.)?(?:(?:\w|-)+\.)(?:\w+\.)?(?:\w+)(?:\/)?.+/;

    return value.match(pattern);
}

function cancelDetails() {
    DataController.Cancel(true);
}

function cancelIndex() {
    DataController.Cancel(false);
}

function deleteRow(item) {
    DataController.DeleteRow(item);
}

function edit() {
    DataController.Edit();
}

function newAliasRow(button) {
    DataController.NewAliasRow(button);
}

function saveDetails(button) {
    DataController.SaveDetails(button);
}

function saveIndex(button) {
    DataController.SaveIndex(button);
}

$(document).ready(function () {
    DataController.Sort();
    DataController.Pagination();

    if (DataController.TotalResults === 0) {
        $("div [id='emptyTable']").removeClass("hide");
    }
});