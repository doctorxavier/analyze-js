var Control = {
    texts: {
        'errorMessage': null,
        'yesCode': null,
        'noCode': null,
        'notRequired': null,
        'oldStatus': null,
        'cancelled': null
    },

    permissions: {
        'isSPDCoordinator': false,
        'isCanceledWritePermission': false,
        'isRequiredWritePermission': false
    },

    _SetText: function (key, value) {
        this.texts[key] = value;
    },

    _SetPermissions: function (key, value) {
        this.permissions[key] = value;
    },

    _Redirect: function (route) {
        location.href = route;
    },

    _AddDisabled: function (element) {
        element.prop('disabled', true);
    },

    _RemoveDisabled: function (element) {
        element.removeAttr('disabled');
    },

    _SetRequired: function (element, value) {
        element.attr('data-parsley-required', value);
    },

    _LoadContainer: function () {
        this.Container = $('#ControlContainer');
        this.RequiredValue = this.Container
            .find('ul.dropdown-menu[aria-labelledby="id-RequiredOptionId"] a[dd-selected]')
            .GetText();
        this.CanceledValue = this.Container
            .find('ul.dropdown-menu[aria-labelledby="id-CanceledOptionId"] a[dd-selected]')
            .GetText();
        this.RequiredDrop = this.Container.find('button[id="id-RequiredOptionId"]');
        this.CanceledDrop = this.Container.find('button[id="id-CanceledOptionId"]');
        this.PCRJustificationContainer = this.Container.find('#PCRJustificacionContainer');
        this.PCRJustification = this.Container.find('textarea[name="PCRJustification"]');
        this.PCRClassification = this.Container.find('button[id="id-PCRClassificationId"]');
        this.IsPCRRequired = this.Container.find('input:hidden[name="IsPCRRequired"]');
        this.IsPCRCanceled = this.Container.find('input:hidden[name="IsPCRCanceled"]');
        this.PCRClassificationId = this.Container.find('input:hidden[name="PCRClassificationId"]');
        this.TitleStatus = $('p.titleStatus');
    },

    _LoadButtons: function () {
        var buttons = $('#ButtonsContainer');
        this.ButtonSave = buttons.find('[data-button="save"]');
        this.ButtonEdit = buttons.find('[data-button="edit"]');
    },

    _ValidationControl: function () {
        this._LoadContainer();
        this._LoadButtons();

        if (this.RequiredValue === this.texts['noCode']) {
            this.IsPCRRequired.val(false);
            this.IsPCRCanceled.val(false);
            this.TitleStatus.text(Control.texts['notRequired']);

            if (this.permissions['isCanceledWritePermission'])
                this._AddDisabled(this.CanceledDrop);

            this._AddDisabled(this.PCRClassification);
            this.PCRJustificationContainer.removeClass('hidden');
            this._RemoveDisabled(this.PCRJustification);
            this._SetRequired(this.PCRJustification, true);
        }
        else if (this.CanceledValue === this.texts['noCode']) {
            this.IsPCRRequired.val(true);
            this.IsPCRCanceled.val(false);
            this.TitleStatus.text(Control.texts['oldStatus']);

            if (this.permissions['isRequiredWritePermission'])
                this._RemoveDisabled(this.RequiredDrop);

            if (this.permissions['isCanceledWritePermission'])
                this._RemoveDisabled(this.CanceledDrop);

            this.PCRJustificationContainer.addClass('hidden');
            this._AddDisabled(this.PCRJustification);
            this._SetRequired(this.PCRJustification, false);
            this._RemoveDisabled(this.PCRClassification);            
        }
        else if (this.CanceledValue === this.texts['yesCode']) {
            this.IsPCRRequired.val(true);
            this.IsPCRCanceled.val(true);
            this.TitleStatus.text(Control.texts['cancelled']);

            if (this.permissions['isCanceledWritePermission'])
                this._RemoveDisabled(this.CanceledDrop);

            this.PCRJustificationContainer.removeClass('hidden');
            this._RemoveDisabled(this.PCRJustification);

            if (this.permissions['isRequiredWritePermission'])
                this._AddDisabled(this.RequiredDrop);

            this._SetRequired(this.PCRJustification, 'true');
            this._AddDisabled(this.PCRClassification);
        }
    },

    _AjaxCall: function (routeActionMethod) {
        showLoader();
        $.ajax({
            url: routeActionMethod,
            type: 'POST',
            cache: false,
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage, false);
                    return false;
                }

                Control.Container.html(response.Data);
                Control.ButtonSave.removeClass('hidden');
                Control.ButtonEdit.addClass('hidden');
                Control._ValidationControl();
                hideLoader();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                showMessage('error', Control.texts['errorMessage']);
                hideLoader();
            }
        });
    },

    _SubmitForm: function (frm) {
        showLoader();

        $.ajax({
            type: 'POST',
            url: frm.attr('action'),
            data: frm.serialize(),
            success: function (response) {
                if (!response.IsValid) {
                    showMessage(response.ErrorMessage, false);
                    return;
                }

                Control._Redirect(response.Route);
                hideLoader();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                showMessage('error', Control.texts['errorMessage']);
                hideLoader();
            }
        });
    }
}

$(document).ready(function () {

    Control._ValidationControl();

    $('button[name="saveOldMethodology"]').on('click', function () {
        var form = $("#formOldMethodology");

        if (!form.parsley().validate()) {
            return;
        }

        Control._SubmitForm(form)
    });

    $(document).on('click', 'ul.dropdown-menu[aria-labelledby*="id-"] a', function () {
        Control._ValidationControl();
    });
});

function cancelOldMethodology(source) {
    Control._Redirect(source.data('route'));
}

function editOldMethodology(source) {
    Control._AjaxCall(source.data('route'));
}