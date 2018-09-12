var safeName = function (str) {
    str = str.replace(/\<span.*\>.*\<\/span\>/ig, '');
    str = str.replace(/^\s*|\s*$/ig, '');
    str = str.replace(/áéíúóÁÉÍÓÚñÑ/i, '');
    str = str.replace(/\&\#\d+\;/g, '');
    return str.replace(/\W+/g, '');
};


(function ($) {

    var idClone = 1;
    var tables = [];
    var collapseBtn = null;

    var trimString = function (str) {
        if (str == null) {
            return '';
        }
        return str.replace(/^\s*|\s*$/g, '');
    };

    var showValidationWarning = function (msg) {
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="evaluation-tracking-msg">' + msg + '</div></div>');
        var title = "Warning";
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: true,
            resizable: false,
            content: "<span class='evaluation-tracking-msg'>" + msg + "</span>",
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        $('.k-widget.k-window').css('margin-top', '100px');
        dialog.open();
    };

    $(document).ready(function () {
        var validateSave = function () {
            var isValid = true;

            $('.evaluation-tracking-block').each(function () {

                var $this = $(this);

                var name = trimString($this.find('.modelItemName').val());

                if (name == '') {
                    showValidationWarning(ErrorsMessages.NameError);
                    isValid = false;
                    return false;
                }

                var reasons = trimString($this.find('.ExplainChangeReasons').val());

                if (reasons == '' && $(this).find('.ExplainChangeReasons').is(':visible')) {
                    showValidationWarning(ErrorsMessages.CancelJustificationError);
                    isValid = false;
                    return false;
                }

                var date = trimString($this.find('input.datepicker').val());

                if (date == '') {
                    showValidationWarning(ErrorsMessages.DateError);
                    isValid = false;
                    return false;
                }

                var responsibleInfo = trimString($this.find('.ResponsibleInfo').val());
                var responsibleId = trimString($this.find('.ResponsibleId')
                    .find("option:selected").text());

                if (responsibleInfo == '' && responsibleId != projectTeam) {
                    showValidationWarning(ErrorsMessages.ResponsibleError);
                    isValid = false;
                    return false;
                }

                var cancelJustification = trimString($this.find('.cancelJustification textarea').val());
                var stageName = trimString($this.find('.StageId')
                    .find("option:selected").text());

                if (stageName == cancelledName && cancelJustification == '') {
                    showValidationWarning(ErrorsMessages.CancelError);
                    isValid = false;
                    return false;
                }

                var metodologyCurrent = trimString($this.find('#MethodologyCurrent')
                    .find("option:selected").text());

                if (metodologyCurrent === '') {
                    showValidationWarning(ErrorsMessages.MetodologyCurrentError);
                    isValid = false;
                    return false;
                }

                var selectedEvaluation = $this.find('#boxEvaluation').find("option:selected")
                    .text();

                if (selectedEvaluation == '') {
                    showValidationWarning(ErrorsMessages.EvaluationError);
                    isValid = false;
                    return false;
                }

                var selectedIntervention = $this.find('#boxIntervention').find("option:selected")
                    .text();

                if (selectedIntervention == '') {
                    showValidationWarning(ErrorsMessages.InterventionError);
                    isValid = false;
                    return false;
                }

                var selectedStage = $this.find('#Stage').find("option:selected")
                    .text();

                if (selectedStage == '') {
                    showValidationWarning(ErrorsMessages.StageError);
                    isValid = false;
                    return false;
                }

                $this.find('.allFundings .displayBlock').each(function () {

                    var source = trimString($(this).find('.SourceFunding')
                        .find('option:selected').text());
                    var code = trimString($(this).find('.CodeFunding').val());

                    source=safeName(source);

                    if ((source == safeName(fundingLoan) || source == safeName(fundingTC) || source == safeName(fundingESW)) && code == '') {
                        showValidationWarning(ErrorsMessages.FundingError);
                        isValid = false;
                        return false;
                    }
                });

                if (!isValid) {
                    return false;
                }
            });

            return isValid;
        };

        CreateElementEvents("#dCloneBlock0 ");

        AddGridCollapse();

        InitiateBotonCollapse();

        $(".bSave").click(function () {
            if (!validateSave()) {
                return;
            }
            var clientData = [];
            for (var idClone = 0; idClone < $("[id^=dCloneBlock]").length ; idClone++) {
                container = $("#dCloneBlock" + idClone);
                var fieldData = {};
                container
                    .find('input[type="hidden"], input:not([name*=Funding]), select:not([name*=Funding]), textarea')
                    .each(function () {
                        var field = $(this);
                        fieldData[field.attr('name')] = field.val();
                    });

                var allFunding = [];
                if (container.find('[id*=Funding-]').length > 0) {
                    var totalFunding = container.find('[id*=Funding-]:last')
                        .attr('id').match(/[\d]+$/);
                    for (var idFundingClone = 0; idFundingClone <= totalFunding; idFundingClone++) {
                        var actualFunding = {};
                        container.find('#dvFunding-' + idFundingClone)
                            .find('input[type="text"], select')
                            .each(function () {
                                var field = $(this);
                                var fieldName = field.attr('name');
                                var position = fieldName.indexOf('.');
                                fieldName = fieldName.substring(position + 1);
                                actualFunding[fieldName] = field.val();
                            });
                        if (!$.isEmptyObject(actualFunding))
                            allFunding.push(actualFunding);
                    }
                }

                var listFunding = {};
                listFunding["listFunding"] = allFunding;
                fieldData["FundingModel"] = listFunding;

                clientData.push(fieldData);
            }

            idbg.lockUi(null, true);
            $.ajax({
                type: 'POST',
                url: $(this).attr('data-route'),
                data: JSON.stringify(clientData),
                contentType: "application/json; charset=utf-8",
                traditional: true,
                success: function (data) {
                    window.location.href = data;
                },
                error: function (e) {
                    console.log("Error when create Evaluation Tracking from JavaScript: ", e);
                },
                complete: function () {
                    idbg.lockUi(null, false);
                }
            });
        });

        $(".StageId").change(function () {

            if ($(this).find("option:selected").text() === cancelledName) {
                $('#dCancelJustification').show();
                $(this).parents("div.tableGrid").parents("div.minimizeTable").find("[name*=Name]")
                    .prop("disabled", true);
                $(this).parents("div.tableGrid").find("input, textarea, select")
                    .prop("disabled", true);
                $(this).parents("div.tableGrid").find("input[type='button']").addClass('disabled');
                $(this).parents("div.tableGrid").find("select.kendoDropDown:not([id*=Stage])")
                    .each(function () {
                        $(this).data("kendoDropDownList").enable(false);
                    });
                return;
            }

            $('#dCancelJustification').hide();
            $(this).parents("div.tableGrid").parents("div.minimizeTable").find("[name*=Name]")
                    .prop("disabled", false);
            $(this).parents("div.tableGrid").find("input, textarea, select")
                .prop('disabled', false);
            $(this).parents("div.tableGrid").find("input[type='button']").removeClass('disabled');
            $(this).parents("div.tableGrid").find("select.kendoDropDown").each(function () {
                if ($(this).attr('id') != 'MethodologyDem') {
                    $(this).data("kendoDropDownList").enable();
                }
            });
            $(".ResponsibleId").trigger('change');
        });

        $("#bNewEvaluation").click(function () {
            var newClone = $("#dCloneBlock0").clone().find("[id^=dvFunding]").remove().end();
            newClone.attr("id", "dCloneBlock" + idClone);
            newClone.appendTo($("#dContentBlocks"));
            newClone.find("input[type='text']").val("");
            newClone.find('#MethodologyDem').prop('disabled', true);
            RemoveKendoSpan("#dCloneBlock", idClone);

            DataElements(idClone);

            CreateElementEvents("#dCloneBlock" + idClone + " ");

            idClone++;

            AddGridCollapse();

            resizeIframeCloud();
        });

        $('.currencyCharacthers').autoNumeric('init', { aDec: '.', aSep: '', vMin: 0, wEmpty: 'zero' });

        setTimeout(function () {
            $(".StageId").trigger('change');
            $(".ResponsibleId").trigger('change');
        }, 500);

        initComboboxes();
        reloadStageAjax("#dCloneBlock0 ");
    });

    function CreateElementEvents(dclone) {
        $(dclone + " select").eq(0).change(function () {
            if ($(this).val() != $(this).parents("div.tableGrid")
                .find(" select").eq(1).val()) {
                $(this).parents("div.tableGrid").find(".ExplainChangeReasons")
                    .parents('.displayBlock').eq(0).show();
                
                return;
            }
            $(this).parents("div.tableGrid").find(".ExplainChangeReasons")
                .parents('.displayBlock').eq(0).hide();
        });

        $(dclone + " select").eq(1).change(function () {
            reloadStageAjax(dclone);
            if ($(this).val() != $(this).parents("div.tableGrid").find(" select").eq(0).val()) {
                $(this).parents("div.tableGrid").find(".ExplainChangeReasons")
                    .parents('.displayBlock').eq(0).show();
                
                return;
            }
            $(this).parents("div.tableGrid").find(".ExplainChangeReasons")
                .parents('.displayBlock').eq(0).hide();
        });

        $(dclone + " .ResponsibleId").change(function () {
            $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]").val("");
            var nameProject = $(this).find("option:selected").text();

            if (nameProject === projectTeam || nameProject === '') {
                $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]")
                    .prop('disabled', true);
                $(this).parents("div.tableGrid").find('.responsableInfoBlock').css({ 'visibility': 'hidden' });
                return;
            }

            $(this).parents("div.tableGrid").find('.responsableInfoBlock').css({ 'visibility': 'visible' });
            $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]").prop('disabled', false);
        });

        $(dclone + "#AddFunding").click(function () {
            var idFClone = 0;
            var newClone = $("#dvFunding").clone();

            newClone.removeClass('hidden');
            if ($(dclone + "[id^=dvFunding-]").length > 0) {
                idFClone = $(dclone + "[id^=dvFunding-]:last").attr("id").match(/[\d]+$/);
                idFClone++;
            }

            newClone.attr("id", "dvFunding-" + idFClone);
            newClone.removeClass("displayNone aStyle");
            newClone.addClass("displayBlock");
            newClone.appendTo($(dclone + "#dvAllFundings"));
            $('.currencyCharacthers').autoNumeric('init', { aDec: '.', aSep: '', vMin: 0, wEmpty: 'zero' });

            RemoveKendoSpan(dclone + "#dvFunding-", idFClone);
            CreateElementEvents_Funding(dclone);

            newClone.find(".kendoDropDown").kendoDropDownList();

            
        });

        initializeData(dclone);
        $(dclone + " .ResponsibleId").trigger('change');
    }

    function initializeData(dclone) {
        $(dclone).find("input[type='text']").val("");
        $(dclone).find("[id^=ResponsibleInfo]").prop('disabled', true);
        $(dclone).find('select').each(function () {
            if ($(this).prop("multiple"))
                $(this).find('option').prop("selected", "");
            else
                $(this).find('option:first').prop("selected", "selected");
        });

        $(dclone + ".kendoDropDown").kendoDropDownList();

        $(dclone + ".kendoDatePicker").kendoDatePicker(
        {
            format: "dd MMM yyyy"
        });

        $(dclone + ".datepicker").kendoDatePicker(
        {
            format: "dd MMM yyyy"
        });

        $(dclone + "#datePicker0").on("focusout", function () {
            value = $(dclone + "#datePicker0").val();
            ValidateDate(value, $(dclone + "#datePicker0"));
        });
    }

    function reloadStageAjax(dclone) {
        var route = $(dclone + '#MethodologyCurrentFilter').attr('data-route');
        $.ajax({
            url: route,
            data: { "MethodologyCurrentId": $(dclone).find('[id=MethodologyCurrent]').val() },
            type: 'POST',
            dataType: "JSON",
            success: function (resp) {
                var combobox = $(dclone + "#Stage").data("kendoDropDownList");
                var updatedOptions = [{ text: "", value: 0 }];

                $.each(resp, function (index, type) {

                    if (type.ConvergenceMasterDataId === 0) {
                        type.Name = "";
                    }
                    updatedOptions.push({ text: type.Name, value: type.ConvergenceMasterDataId });
                });
                combobox.dataSource.data(updatedOptions);
                combobox.dataSource.query();
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
        });
    }

    function CreateElementEvents_Funding(dclone) {
        var bdelete = $(dclone + ".deleteFunding");
        bdelete.click(function () {
            $(this).parent().parent().remove();
            
        });
    }

    function DataElements(idElement) {
        $("#dCloneBlock" + idElement + " input.ReadValueTitle")
            .attr("placeholder", "Name of Evaluation " + (idElement + 1));
    }

    function RemoveKendoSpan(container, idElement) {
        $(container + idElement).find("select.kendoDropDown, select.k-icon").unwrap();
        $(container + idElement).find("input.datepicker").unwrap().unwrap();
        $(container + idElement).find("span.k-select, span.k-input, span.k-dropdown-wrap").remove();
    }

    function CloneElementsNewIds(container, idElement) {
        $(container + idElement).find("*").each(function (index, element) {
            if (element.id)
                if (element.id.length > 0)
                    element.id = element.id + idElement;
            if (element.name)
                if (element.name.length > 0)
                    element.name = element.name + idElement;
        });
    }

    function InitiateBotonCollapse() {
        var collapseBtnActiveText = "Collapse All";
        var collapseBtnInactiveText;
        collapseBtn = new BotonActivo($(".btn-action-group"));
        var collapseBtnInputText = collapseBtn.getBtnInputText();

        if (collapseBtnInputText.toLowerCase() === collapseBtnActiveText.toLowerCase()) {
            collapseBtn.setActiveText(collapseBtnActiveText);
            collapseBtnInactiveText = "Expand All";
            collapseBtn.setInactiveText(collapseBtnInactiveText);
        }
        collapseBtn.btnInput.click(function () {
            massCollapse(collapseBtn);
        });
    }

    function UnbindClickCollapse() {
        var table = tables[tables.length - 1];
        table.foldBtn.unbind("click");
    }

    function AddGridCollapse() {
        var minimizeTable = $(".minimizeTable").last();
        tables.push(new TableFold(minimizeTable));

        var table = tables[tables.length - 1];
        table.foldBtn.click(function () {
            setTimeout(function () {
                if (changeCollapseBtn(collapseBtn)) {
                    collapseBtn.switchState(collapseBtn);
                }
            }, 500);
        });
    }

    function massCollapse(collapseBtn) {
        if (collapseBtn.isActive()) {
            if (tables !== null) {
                for (var i = 0; i < tables.length; i++) {
                    tables[i].unfold();
                }
            }
        }
        else {
            if (tables !== null) {
                for (var i = 0; i < tables.length; i++) {
                    tables[i].fold();
                }
            }
        }
    }

    function changeCollapseBtn(collapseBtn) {
        if (tables !== null) {
            if (collapseBtn.isActive()) {
                for (var i = 0; i < tables.length; i++) {
                    if (tables[i].isVisible()) {
                        return false;
                    }
                }
                return true;
            }
            else {
                for (var i = 0; i < tables.length; i++) {
                    if (tables[i].isVisible()) {
                        return true;
                    }
                }
                return false;
            }
        }
    }

    function ValidateDate(value, element) {
        var validator = this;
        var datePat = /([0-9]{2})(.*?)([0-9]{2})(.*?)([0-9]{4})/i;
        var actualDate = new Date();
        var actualDay = actualDate.getDate();
        var actualMonth = actualDate.getMonth() + 1;
        var actualYear = actualDate.getFullYear();
        var actualDateSplit = String(actualDay) + "/" + String(actualMonth) + "/" + String(actualYear);

        var completeDate = value.match(datePat);

        if (completeDate == null) {
            var dateVal = value.toString().split(" ");
            if (dateVal.length == 3) {
                completeDate = (new Date(value)).toLocaleDateString("pt-BR").match(datePat);

                if (completeDate == null) {
                    $(element).kendoDatePicker(
                    {
                        value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                        format: "dd MMM yyyy",
                        defaultValue: actualDateSplit[0]
                    });
                    return;
                }
                else {
                    day = completeDate[1];
                    month = completeDate[3];
                    year = completeDate[5];
                }
            }
            else {
                if (dateVal[0] == "") {
                    return;
                }

                $(element).kendoDatePicker(
                {
                    value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                    format: "dd MMM yyyy",
                    defaultValue: actualDateSplit[0]
                });
                return;
            }
        }
        else {
            day = completeDate[1];
            month = completeDate[3];
            year = completeDate[5];
        }

        if (day < 1 || day > 31) {
            $(element).kendoDatePicker(
            {
                value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                format: "dd MMM yyyy",
                defaultValue: actualDateSplit[0]
            });
            return;
        }

        if (month < 1 || month > 12) {
            $(element).kendoDatePicker(
            {
                value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                format: "dd MMM yyyy",
                defaultValue: actualDateSplit[0]
            });
            return;
        }

        if ((month == 4 || month == 6 || month == 9 || month == 11) && day == 31) {
            $(element).kendoDatePicker(
                {
                    value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                    format: "dd MMM yyyy",
                    defaultValue: actualDateSplit[0]
                });
            return;
        }

        if (month == 2) {
            var bisiesto = (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0));
            if (day > 29 || (day == 29 && !bisiesto)) {
                $(element).kendoDatePicker(
                {
                    value: new Date(actualMonth + "/" + actualDay + "/" + actualYear),
                    format: "dd MMM yyyy",
                    defaultValue: actualDateSplit[0]
                });
                return;
            }
        }

        $(element).kendoDatePicker(
        {
            value: new Date(month + "/" + day + "/" + year),
            format: "dd MMM yyyy",
            defaultValue: value
        });
    }


})(window.jQuery);

function initComboboxes() {
    var comboboxes = $('#MethodologyCurrent, #ResponsibleId'),
        emptyOption = { text: "", value: "0" };

    comboboxes.each(function () {
        var combobox = $(this),
            comboboxKendo = combobox.data('kendoDropDownList'),
            options = combobox.find('option');
            comboboxOptions = [];

        comboboxOptions.push(emptyOption);
        options.each(function () {
            var option = $(this);
            comboboxOptions.push({ text: option.text(), value: option.val() });
        });

        comboboxKendo.dataSource.data(comboboxOptions);
        comboboxKendo.dataSource.query();
        comboboxKendo.select(0);
    })
    $('.selectCustom select').not('[id=MethodologyDem]').data('kendoDropDownList').select(0);
    $('.MultiSelectCustom').each(function () {
        $(this).find('option').first().remove();
    });
}