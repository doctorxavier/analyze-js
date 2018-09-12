var ModalHelp = new Object();

ModalHelp.URL = "";
ModalHelp.ViewId = "";
ModalHelp.ViewCode = "";


ModalHelp['Initialization'] = function (url, viewCode) {
    ModalHelp.URL = url;
    ModalHelp.ViewCode = viewCode;
    ModalHelp.ViewId = "";
}

ModalHelp["ChangeDescription"] = function (value) {
    this.ViewId = value;

    if (this.ViewId !== "") {
        $('ul.parsley-errors-list').removeClass('filled');
    }
}

ModalHelp["ChangeModule"] = function (selected) {
    var $description = $('#description');
    var $descriptions = $description.siblings('ul');

    $description.FirstorDefault();

    if (selected === "") {
        $descriptions.find('a:hidden').show();

        return;
    }

    $descriptions.find('a[dd-parent-id!="' + selected + '"]').hide();
    $descriptions.find('a[dd-parent-id="' + selected + '"], a:not([dd-parent-id])').show();
}

ModalHelp['Save'] = function () {
    if (this.ViewId === "") {
        $('ul.parsley-errors-list').addClass('filled');

        return;
    }

    $.ajax({
        url: ModalHelp.URL,
        cache: false,
        data: { ViewId: ModalHelp.ViewId, ViewCode: ModalHelp.ViewCode },
        async: true,
        type: 'POST'
    }).done(function (response) {
        if (!response.IsValid) {
            frames['someFrame'].showMessage(response.ErrorMessage);

            return;
        }

        ConvergenceHelp.Add(response.Model.Data[0]);
        ConvergenceHelp.Change(ModalHelp.ViewCode);

        $('#addViewCode').modal('hide');

        frames['someFrame'].showMessage(response.Model.Message);
    });
}