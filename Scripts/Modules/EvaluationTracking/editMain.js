var safeName = function (str) {
    str = str.replace(/\<span.*\>.*\<\/span\>/ig, '');
    str = str.replace(/^\s*|\s*$/ig, '');
    str = str.replace(/\&\#\d+\;/g, '');
    str = str.replace(/áéíúóÁÉÍÓÚñÑ/i, '');
    return str.replace(/\W+/g, '');
};



var showValidationWarning = function (msg) {
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal1"><div class="evaluation-tracking-msg">' + msg + '</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal1").kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
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
    dialog.center();
    dialog.open();
};

var currentMedia;

(function ($) {

    var editUrl = null;
    var collapseBtn = null;

    var trimString = function (str) {
        if (str == null) {
            return '';
        }
        return str.replace(/^\s*|\s*$/g, '');
    };

    var doSave = function (clientData) {

        $.ajax({
            type: 'POST',
            url: editUrl,
            data: JSON.stringify(clientData),
            contentType: "application/json; charset=utf-8",
            traditional: true,
            success: function (data) {
                window.location.href = data;
            },
            error: function (e) {
                console.log("Error when edit Evaluation Tracking from JavaScript: ", e);
            },
            complete: function () {
                idbg.lockUi(null, false);
            }
        });
    };

    var getClientData = function () {
        var clientData = [];
        $("[id^=CountET]").not('.template').each(function () {
            container = $(this);
            var fieldData = {};
            container
                .find('input[type="hidden"], input:not([name*=Funding]), select:not([name*=Funding]), textarea')
                .each(function () {
                    var field = $(this);

                    if (field.attr('name')) {
                        var fieldName = field.attr('name');
                        var position = fieldName.lastIndexOf('.');

                        if (fieldName.indexOf('TopicModel') > -1)
                            position = fieldName.indexOf('.');

                        fieldName = fieldName.substring(position + 1);
                        fieldData[fieldName] = field.val();
                    }
                });

            var allFunding = [];

            if (container.find('[id*=Funding-]').length > 0) {
                var totalFunding = container.find('[id*=Funding-]:last')
                    .attr('id').match(/[\d]+$/);

                for (var idFundingClone = 0; idFundingClone <= totalFunding; idFundingClone++) {
                    var actualFunding = {};
                    container.find('#dvFunding-' + idFundingClone)
                        .find('input[type="text"], input[type="hidden"], select')
                        .each(function () {
                            var field = $(this);
                            var fieldName = field.attr('name');
                            var position = fieldName.lastIndexOf('.');
                            fieldName = fieldName.substring(position + 1);
                            actualFunding[fieldName] = field.val();
                        });

                    if (!$.isEmptyObject(actualFunding))
                        allFunding.push(actualFunding);
                }
            }

            var listFunding = {};
            var deletedFundingId = container.find('.deletedFundings');
            listFunding[deletedFundingId.attr('name')] = deletedFundingId.val();
            listFunding["listFunding"] = allFunding;
            fieldData["FundingModel"] = listFunding;

            var allDocuments = [];

            var documents = container.find(".documentsEvaluation");

            if (documents.length > 0) {
                var dataSource = documents.data("kendoGrid").dataSource;
                var data = dataSource.data();
                var changedModel = {};

                for (var i = 0; i < data.length; i++) {
                    dataDocument = data[i];
                    dataDocument["NameOfDocument"] = "";
                    var idDocument = dataDocument["DocumentEvaluationTrackingId"];
                    var initialPosition = idDocument.lastIndexOf("value=") + 7;
                    var finalPosition = idDocument.lastIndexOf(">") - 1;
                    idDocument = idDocument.substring(initialPosition, finalPosition);
                    dataDocument["DocumentEvaluationTrackingId"] = idDocument;
                    dateDocument = dataDocument["DocumentCreationDate"];
                    dataDocument["DocumentCreationDate"] = dateDocument;
                    allDocuments.push(dataDocument);
                }
            }

            listDocument = {};
            listDocument["listDocuments"] = allDocuments;
            fieldData["DocumentModel"] = listDocument;

            clientData.push(fieldData);
        });
        return clientData;
    }

    $(document).ready(function () {

        var validateSave = function () {
            var isValid = true;

            $('.evaluation-tracking-block').not('.template').each(function () {

                var $this = $(this);

                isValid = validateFields($this);

                if (!isValid) {
                    return false;
                }

                $this.find('.allFundings .displayBlock').each(function () {

                    var source = trimString($(this).find('.SourceFunding')
                        .find('option:selected').text());
                    var code = trimString($(this).find('.CodeFunding').val());

                    source = safeName(source);
                    var invalidFunding = code === '' &&
                        (source === safeName(fundingLoan) ||
                            source === safeName(fundingTC) ||
                            source === safeName(fundingESW));

                    if (invalidFunding) {
                        showValidationWarning(ErrorsMessages.FundingError);
                        isValid = false;
                        return false;
                    }
                });

                return isValid;
            });
            return isValid;
        };

        initializeCollapse();
        CreateDeletes_Funding("");
        CreateElementEvents_Funding("");

        $(".bSave").click(function () {

            if (!validateSave()) {
                return;
            }

            var clientData = getClientData();

            editUrl = $(this).attr('data-route');
            idbg.lockUi(null, true);

            var verifyContentParams = getVerifyContentString();

            $.ajax({
                type: 'POST',
                url: $(this).attr('data-route-check'),
                data: { 'resultsMatrixId': $("#hdnResultsMatrixId").val(), 'verifyContent': verifyContentParams },
                success: function (data) {

                    if (data.StageValidation == 'OK') {
                        doSave(clientData);
                    }
                    else {
                        idbg.lockUi(null, false);
                        $('.verifySave').trigger('click');
                    }
                },
                error: function (e) {
                    console.log("Error when edit Evaluation Tracking from JavaScript: ", e);
                }
            });

        });

        initialKendoDropDownState();

        $(document).on('change', '.MethodologyCurrent', function () {
            selectMethodologyCurrent(this, 0);
        });

        $(".MethodologyDem").change(function () {
            if ($(this).val() != $(this).parents("div.tableGrid")
                .find("[id*=MethodologyCurrentId]").val()) {
                $(this).parents("div.tableGrid").find("[id^=dChangedMethodology]").show();
                
                return;
            }
            $(this).parents("div.tableGrid").find("[id^=dChangedMethodology]").hide();
        });

        $(document).on('change', '.StageId', function () {
            if ($(this).find("option:selected").text() === cancelledName) {

                $(this).parents('.evaluation-tracking-block').find('.cancelJustification').show();

                $(this).parents("div.tableGrid").parents("div.minimizeTable").find("[name*=Name]")
                    .prop("disabled", true);
                $(this).parents("div.tableGrid").find("input, textarea, select")
                    .not('#modelItem_CancelJustification')
                    .prop("disabled", true);
                $(this).parents("div.tableGrid").find("input[type='button']").addClass('disabled');
                $(this).parents("div.tableGrid").find("select.kendoDropDown:not([id*=Stage])")
                    .each(function () {
                        $(this).data("kendoDropDownList").enable(false);
                    });

                $(this).parents('.evaluation-tracking-block').find('.dateSelect .k-select')
                    .css({ 'visibility': 'hidden' });
                $(this).parents('.evaluation-tracking-block').find('.deleteDocumentButton')
                    .css({ 'visibility': 'hidden' });

                $(this).parents('.evaluation-tracking-block').addClass('Cancelled');
                return;
            }

            $(this).parents('.evaluation-tracking-block').removeClass('Cancelled');

            $(this).parents('.evaluation-tracking-block').find('.cancelJustification').hide();
            $(this).parents('.evaluation-tracking-block').find('.deleteDocumentButton')
                .css({ 'visibility': 'visible' });
            $(this).parents('.evaluation-tracking-block').find('.dateSelect .k-select')
                .css({ 'visibility': 'visible' });

            $(this).parents("div.tableGrid").parents("div.minimizeTable").find("[name*=Name]")
                    .prop("disabled", false);
            $(this).parents("div.tableGrid").find("input, textarea, select")
                .not('#modelItem_CancelJustification')
                .prop('disabled', false);
            $(this).parents("div.tableGrid").find("input[type='button']").removeClass('disabled');

            $(this).parents("div.tableGrid").find("select.kendoDropDown").each(function () {
                if ($(this).attr('id') != 'modelItem_MethodologyPerDiemId') {
                    $(this).data("kendoDropDownList").enable();
                }
            });

            $(".ResponsibleId").trigger('change');
        });

        $(document).on('change', '.ResponsibleId', function () {
            $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]");
            var nameProject= $(this).find("option:selected").text();

            if (nameProject === projectTeam || nameProject ==="") {
                $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]")
                    .prop('disabled', true).val("");
                $(this).parents("div.tableGrid").find('.responsableInfoBlock')
                    .css({ 'visibility': 'hidden' });
                return;
            }

            $(this).parents("div.tableGrid").find("[id^=ResponsibleInfo]")
                .prop('disabled', false);
            $(this).parents("div.tableGrid").find('.responsableInfoBlock')
                .css({ 'visibility': 'visible' });

        });

        $(".datepicker").kendoDatePicker(
        {
            format: "dd MMM yyyy"
        });

        $(".kendoDropDown").kendoDropDownList();

        buildTable(null);

        $(".datePicker0").on("focusout", function () {
            value = $(this).val();
            ValidateDate(value, this);
        });

        $('#completeDocument').change(function () {
           
            var idDoc = currentMedia.attr("data-routeDoc");

            currentMedia.load(idDoc, function () {
                buildTable(currentMedia)
                hideLoader();
            });
            
        })

        $(".StageId").trigger('change');
        $(".ResponsibleId").trigger('change');

        setTimeout(function () {
            $(".StageId").trigger('change');
            $(".ResponsibleId").trigger('change');
        }, 500);

        $('#lnkShowDisaggregation').bind('click', function () {

            $('.evaluation-tracking-block.Cancelled').show();
            $('#lnkShowDisaggregation').hide();
            $('#lnkHideDisaggregation').removeClass('hidden');
            
        });

        $('#lnkHideDisaggregation').bind('click', function () {

            $('.evaluation-tracking-block.Cancelled').hide();
            $('#lnkShowDisaggregation').show();
            $('#lnkHideDisaggregation').addClass('hidden');
            
        });

        $('.currencyCharacthers').autoNumeric('init', { aDec: '.', aSep: '', vMin: 0, wEmpty: 'zero' });

        $('.displayBlock.Cancelled').hide();

        initComboboxes();

        $('.inline-block').has('.documentsEvaluation').css("width", "95%");

        $('#createNewEvaluation').on('click', createNewEvaluation);
    });

    function documentAddWindow(url) {

        //$(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');

        idbKendoWindow(
        {
            content: url,
            title: 'Add document',
            resizable: false,
            close: function () {
                //$(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
            }
        });
    }

    function documentAddWindowParent(event) {
        var clientData = getClientData();
        var row = $(event.target).parents('tr');
        var rowIndex = $(row).index();

        idbg.lockUi(null, true);

        $.ajax({
            type: 'POST',
            url: $('.bSave').attr('data-route-save-documents'),
            data: JSON.stringify(clientData),
            contentType: "application/json; charset=utf-8",
            traditional: true,
            success: function (data) {

                var documentEvaluationTrackingIdParts = data['DocumentEvaluationTrackingId'].split(',');
                var clickecDocumentEvaluationTrackingId = documentEvaluationTrackingIdParts[rowIndex];

                var tableDocuments = $(row).parents('.evaluation-tracking-block').find('.documentsEvaluation');

                tableDocuments.find('tbody tr').each(function () {

                    if ($(this).find('input[name="modelDoc.DocumentEvaluationTrackingId"]').length == 0) {
                        $(this).children().last().append($("<input type='hidden' />")
                            .attr({
                                'name': 'modelDoc.DocumentEvaluationTrackingId',
                                'id': 'modelDoc_DocumentEvaluationTrackingId'
                            }).val(documentEvaluationTrackingIdParts.shift()));
                    }
                });

                currentMedia = $(row).parents('.gridDiv');
                $("#currentMedia").val(currentMedia);

                dataSource = $(row).parents('.documentsEvaluation').data('kendoGrid').dataSource;
                dataDoc = dataSource.data();

                var url = currentMedia.attr('data-routeurl')
                    .replace("documentevaluationtrackingid", clickecDocumentEvaluationTrackingId)
                    .replace("stageid", dataDoc[rowIndex].StageDocumentId)
                    .replace("typeofdocumentid", dataDoc[rowIndex].TypeOfDocumentId)
                    .replace("documentpermissionsid", dataDoc[rowIndex].DocumentPermissionsId)
                    .replace("documentdate", toDate(dataDoc[rowIndex].DocumentCreationDate))
                    .replace("description", dataDoc[rowIndex].Description);
                documentAddWindow(url);
            },
            error: function (e) {
                console.log("Error when edit Evaluation Tracking from JavaScript: ", e);
            },
            complete: function () {
                idbg.lockUi(null, false);
            }
        });
    }

    function initialKendoDropDownState() {
        var seletedId = 0;
        $(".tableGrid").each(function (index) {
            var selectedId = $(this).find("#StageId").val();
            selectMethodologyCurrent($(this).find(".MethodologyCurrent"), selectedId);
        });
    }

    function reloadStageAjax(dclone, seletedId) {
        var route = dclone.find('#MethodologyCurrentFilter').attr('data-route');
        dclone.find("#dvStageLoading").show();

        var optionDefaultMetodology = dclone.find('.MethodologyCurrent').val();
        var optionMetodology = dclone.find('.MethodologyCurrent select').val();

        $.ajax({
            url: route,
            data: {
                "MethodologyCurrentId": optionDefaultMetodology == "" ?
                    optionMetodology : optionDefaultMetodology
            },
            type: 'POST',
            dataType: "JSON",
            success: function (resp) {
                var combobox = dclone.find("#StageId").data("kendoDropDownList");
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
            },
            complete: function (resp) {
                dclone.find("#dvStageLoading").hide();
            }
        });
    }

    function selectMethodologyCurrent(element, selectedId) {
        reloadStageAjax($(element).parents("div.tableGrid"), selectedId);

        var textCurrent = $(element).parents("div.tableGrid").find(".MethodologyCurrent")
            .find("option:selected").text();
        var textDem = $(element).parents("div.tableGrid").find("[id*=MethodologyPerDiemId]")
            .find("option:selected").text();
        var blockQuasiExOther = $(element).parents("div.tableGrid").find("[id^=QuasiExperimentalOther]")
        blockQuasiExOther.hide();
        $(element).parents("div.tableGrid").find("[id^=dChangedMethodology]").hide();
        if ($("#CodeNotChangedReason").val() != "ETMETHB2009") {

            if (textDem == textCurrent && textCurrent == quasiExperimentalOther) {
                blockQuasiExOther.show();
            }
            else {
                blockQuasiExOther.hide();
            }

            if ($(element).val() != $(element).parents("div.tableGrid")
                .find("[id*=MethodologyPerDiemId]").val()) {
                $(element).parents("div.tableGrid").find("[id^=dChangedMethodology]").show();
                return;
            }
            $(element).parents("div.tableGrid").find("[id^=dChangedMethodology]").hide();
        }
    };

    function CreateDeletes_Funding(dclone) {
        var bdelete = (dclone == "") ? $(".deleteFunding") : dclone.find(".deleteFunding");
        bdelete.click(function () {
            if ($(this).siblings().val()) {
                var selectFunding = $(this).parents().find('.deletedFundings:first');
                selectFunding.append('<option val="' + $(this).siblings().val() +
                    '">' + $(this).siblings().val() + '</option>');
                selectFunding.find('option:last').attr("selected", "selected");
            }

            $(this).parent().parent().remove();

            resizeIframeCloud();
        });
    }

    function CreateElementEvents_Funding(dclone) {
        $(dclone + ".addFunding").off('click');
        $(dclone + ".addFunding").click(function () {
            var idFClone = 0;
            var newClone = $("#dvFunding").clone();

            if ($(this).parent().siblings('#dvAllFundings').find("[id^=dvFunding-]").length > 0) {
                idFClone = $(this).parent().siblings('#dvAllFundings')
                    .find("[id^=dvFunding-]:last").attr("id").match(/[\d]+$/);
                idFClone++;
            }

            newClone.attr("id", "dvFunding-" + idFClone);
            newClone.removeClass("displayNone");
            newClone.addClass("displayBlock");
            newClone.appendTo($(this).parent().siblings('#dvAllFundings'));

            RemoveKendoSpan(newClone);
            CreateDeletes_Funding(newClone);

            newClone.find(".kendoDropDown").kendoDropDownList();
            $('.currencyCharacthers').autoNumeric('init', { aDec: '.', aSep: '', vMin: 0, wEmpty: 'zero' });

            resizeIframeCloud();
        });

    }

    function RemoveKendoSpan(newClone) {
        newClone.find("select.kendoDropDown, select.k-icon").unwrap();
        newClone.find("input.datepicker").unwrap().unwrap();
        newClone.find("span.k-select, span.k-input, span.k-dropdown-wrap").remove();
    }

    function initializeCollapse() {
        var tables = [];

        if ($(".minimizeTable").length > 0) {
            var minimizeTable = $(".minimizeTable");
            collapseBtn = new BotonActivo($(".btn-action-group"));

            for (var i = 0; i < $(minimizeTable).length; i++) {
                tables.push(new TableFold(minimizeTable[i]));
            }
        }

        for (var i = 0; i < tables.length; i++) {
            collapseFold(tables[i], tables);
        }

        if (collapseBtn !== undefined) {
            var collapseBtnActiveText = "Collapse All";
            var collapseBtnInactiveText;
            var collapseBtnInputText = collapseBtn.getBtnInputText();

            if (collapseBtnInputText.toLowerCase() === collapseBtnActiveText.toLowerCase()) {
                collapseBtn.setActiveText(collapseBtnActiveText);
                collapseBtnInactiveText = "Expand All";
                collapseBtn.setInactiveText(collapseBtnInactiveText);
            }
            collapseBtn.btnInput.click(function () {
                massCollapse(collapseBtn, tables);
            });
        }
    }

    function collapseFold(table, tables) {
        table.foldBtn.click(function () {
            setTimeout(function () {
                if (changeCollapseBtn(collapseBtn, { tables: tables })) {
                    collapseBtn.switchState(collapseBtn);
                }
            }, 500);
        });
    }

    function selectToJson(gridD, busca) {
        var values = []
        $(gridD).parents("div.tableGrid").find("#" + busca + " option")
            .each(function () {
                values.push({ text: $(this).text(), value: $(this).attr("value") })
            });

        return values;
    }

    function buildTable(divGrid) {
        var allGrids = (divGrid) ? divGrid.find(".documentsEvaluation") : $(".documentsEvaluation");
        var defaultValues =
            {
                Stage: $("#StageDocumentId option:first").attr("value"),
                TypeofDocument: $("#TypeofDocumentId option:first").attr("value"),
                DocumentCreationDate: $.now(),
                DocumentPermission: $("#DocumentPermissionId option:first").attr("value")
            };

        allGrids.each(function () {
            stages = selectToJson(this, "StageDocumentId");
            types = selectToJson(this, "TypeOfDocumentId");
            permissions = selectToJson(this, "DocumentPermissionsId");

            var isCancelled = $(this).parents('.evaluation-tracking-block').hasClass(cancelledName);

            var grid = $(this).kendoGrid(
                {
                    toolbar: [
                        {
                            name: "create",
                            text: "Add Document",
                            template: kendo.template($("#template").html())
                        }
                    ],

                    dataSource:
                        {
                            schema:
                                {
                                    model:
                                        {
                                            fields:
                                                {
                                                    StageDocumentId:
                                                        {

                                                        },
                                                    IDBDocNumber:
                                                        {
                                                            editable: false,
                                                        },
                                                    TypeOfDocumentId:
                                                        {
                                                        },
                                                    NameOfDocument:
                                                        {
                                                            editable: false
                                                        },
                                                    Description:
                                                        {
                                                        },
                                                    DocumentCreationDate:
                                                        {
                                                        },
                                                    DocumentPermissionsId:
                                                        {
                                                            defaultValue: defaultValues["DocumentPermission"]
                                                        },
                                                    DocumentEvaluationTrackingId:
                                                        {
                                                            editable: false
                                                        },
                                                }
                                        }
                                }
                        },

                    columns: [
                        {
                            field: "StageDocumentId",
                            values: stages,
                            editor: function (container, options) {
                                var input = $("<input data-text-field='text' data-value-field='value' data-bind='value:"
                                + options.field + "' />");
                                input.appendTo(container);


                                input.kendoDropDownList(
                                {
                                    optionLabel: "Select Item...",
                                    dataSource: selectToJson(grid.wrapper, "StageId"),
                                    dataTextField: "text",
                                    dataValueField: "value",

                                    change: function (e) {
                                        var row = this.element.closest("tr");
                                        var rowIdx = $("tr", grid.tbody).index(row);

                                        $.ajax({
                                            type: "POST",
                                            url: grid.wrapper.find("table")
                                                .attr("data-requestfilter")
                                                + "?IdStage=" + this.value(),
                                            contentType: "application/json; charset=utf-8",
                                            traditional: true,
                                            success: function (data) {
                                                var newValue = grid.dataSource.at(rowIdx);
                                                newValue.set("TypeOfDocumentId", data[0].value);

                                                if (newValue.get("DocumentPermissionsId") != null
                                                    && newValue.get("DocumentPermissionsId") != ""
                                                    && newValue.get("NameOfDocument") == "") {
                                                    row.find(".k-grid-delete").hide();
                                                    row.find(".k-grid-newDocument").show();
                                                }
                                            }
                                        });
                                    }

                                }).appendTo(container);

                                if (isCancelled)
                                    input.data('kendoDropDownList').readonly();
                            }
                        },
                        {
                            field: "TypeOfDocumentId",
                            values: types,
                            editor: function (container, options) {
                                var row = container.closest("tr");
                                var rowIdx = $("tr", grid.tbody).index(row);
                                var newValue = grid.dataSource.at(rowIdx);
                                var input = $("<input data-text-field='text' data-value-field='value' data-bind='value:"
                                    + options.field + "' />");

                                input.appendTo(container);

                                input.kendoDropDownList(
                                {
                                    optionLabel: "Select Item...",
                                    dataSource:
                                        {
                                            transport:
                                                {
                                                    read:
                                                        {
                                                            url: grid.wrapper.find("table")
                                                                .attr("data-requestfilter")
                                                                + "?IdStage="
                                                                + newValue.StageDocumentId,
                                                            dataType: "json",
                                                            type: "POST"
                                                        }
                                                }
                                        },
                                    dataTextField: "text",
                                    dataValueField: "value",
                                    change: function (e) {
                                        if (newValue.get("DocumentPermissionsId") != null
                                            && newValue.get("DocumentPermissionsId") != ""
                                            && newValue.get("NameOfDocument") == "") {
                                            row.find(".k-grid-delete").hide();
                                            row.find(".k-grid-newDocument").show();

                                            newValue.get("DocumentPermissionsId")
                                        }
                                    }
                                }).appendTo(container);

                                if (isCancelled)
                                    input.data('kendoDropDownList').readonly();
                            }
                        },
                        {
                            field: "NameOfDocument"
                        },
                        {
                            field: "Description"
                        },
                        {
                            field: "DocumentCreationDate",
                            format: "{0:dd MMM yyyy}",
                            editor: dateEditor
                        },
                        {
                            field: "IDBDocNumber",
                        },
                        {
                            field: "DocumentPermissionsId",
                            values: permissions,
                            editor: function (container, options) {
                                var row = container.closest("tr");
                                var rowIdx = $("tr", grid.tbody).index(row);
                                var newValue = grid.dataSource.at(rowIdx);

                                var input = $("<input data-text-field='text' data-value-field='value' data-bind='value:"
                                    + options.field + "' />");

                                input.appendTo(container);

                                input.kendoDropDownList(
                                {
                                    optionLabel: "Select Item...",
                                    dataSource: permissions,
                                    dataTextField: "text",
                                    dataValueField: "value",
                                    change: function (e) {
                                        if (newValue.get("StageDocumentId") != null
                                            && newValue.get("StageDocumentId") != ""
                                            && newValue.get("NameOfDocument") == "") {
                                            row.find(".k-grid-delete").hide();
                                            row.find(".k-grid-newDocument").show();
                                        }
                                    }
                                }).appendTo(container);

                                if (newValue.get("NameOfDocument") != "")
                                    input.data("kendoDropDownList").readonly();

                                container.find('.k-dropdown-wrap').css({ 'background-color': '#EEE' });
                            }
                        },
                        {
                            field: "DocumentEvaluationTrackingId",
                        },
                        {
                            command: [
                                {
                                    name: "newDocument",
                                    text: "",
                                    className: "operationButton addOne deleteDocument",

                                    click: function (e) {
                                        documentAddWindowParent(e);
                                    },
                                },
                                {
                                    template: '<input type="button" class="operationButton removeIcon deleteFunding deleteDocumentButton" id="DeleteFunding" onclick="GridDeleteDocumentClick(this)">',
                                    text: " ",
                                    width: "180px"
                                }
                            ]
                        }

                    ],

                    scrollable: false,
                    pageable: false,
                    editable: true,
                    dataBound: function (e) {
                        $(".k-pager-nav").hide();
                        onDataBoundCommands(e);
                    }
                }).data("kendoGrid");
            
        });
    }

    function onDataBoundCommands(e) {

        var gridData = e.sender.dataSource.view();

        for (var i = 0; i < gridData.length; i++) {
            var currentUid = gridData[i].uid;
            var currentRow = e.sender.table.find("tr[data-uid='" + currentUid + "']");

            if (gridData[i].StageDocumentId != null && gridData[i].StageDocumentId != ""
                && gridData[i].TypeOfDocumentId != null && gridData[i].TypeOfDocumentId != ""
                && gridData[i].DocumentPermissionsId != null
                && gridData[i].DocumentPermissionsId != "") {
                var button = $(currentRow).find(".k-grid-delete");

                if (gridData[i].NameOfDocument != null && gridData[i].NameOfDocument != "")
                    button = $(currentRow).find(".k-grid-newDocument");
                button.hide();
            }
            else
                $(currentRow).find(".k-grid-newDocument").hide();
            var cells = currentRow.children();
            cells.eq(0).addClass('editable');
            cells.eq(1).addClass('editable');
            cells.eq(3).addClass('editable');
            cells.eq(4).addClass('editable long');
            cells.eq(6).addClass('editable');
        }

        resizeIframeCloud();
    }

    function dateEditor(container, options) {
        $("<input data-text-field='" + options.field + "' data-value-field='" + options.field +
            "' data-bind='value:" + options.field + "' data-format='" + options.format + "'/>")
        .appendTo(container)
        .kendoDatePicker(
        {
            dateFormat: 'dd MMM yyyy'
        });
    }

    function massCollapse(collapseBtn, tables) {
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

        resizeiframeCloudTimeOut();
    }

    function changeCollapseBtn(collapseBtn, tables) {
        resizeIframeCloud();
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

    Date.prototype.ddmmmyyyy = function () {
        var yyyy = this.getFullYear().toString();
        var mm = (this.getMonth() + 1).toString();
        var dd = this.getDate().toString();

        return (dd[1] ? dd : "0" + dd[0]) + '-' + (mm[1] ? mm : "0" + mm[0]) + '-' + yyyy;
    };

    function toDate(val) {
        var d = new Date(val);

        if (!isNaN(d.valueOf()))
            return d.ddmmmyyyy();

        return val;
    }

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
        })

        var hiddenMetodologyCurrents = $('.metodology-current-id');
        var selectMetodologyCurrents = $('.MethodologyCurrent select');

        for (var i = 0; i < hiddenMetodologyCurrents.length; i++) {

            var $hiddenMetodologyCurrents = $(hiddenMetodologyCurrents[i]).val();
            var $selectMetodologyCurrents = $(selectMetodologyCurrents[i]);

            if ($hiddenMetodologyCurrents === "") {
                $selectMetodologyCurrents.data('kendoDropDownList').select(0);
                var tableGrid = $(".tableGrid").first();
                reloadStageAjax(tableGrid, 0);
            }
        }
        $('.MultiSelectCustom').each(function () {
            $(this).find('option').first().remove();
        });
    }

    function createNewEvaluation(source) {
        var newClone = $('.evaluation-tracking-block.template').clone();
        
        var identifier = $('.evaluation-tracking-block.template')
            .find('#modelItem_EvaluationTrackingId');

        identifier.val(parseInt(identifier.val()) - 1);

        newClone.removeClass("displayNone").removeClass('template');
        newClone.addClass("displayBlock");

        newClone.appendTo($('.container'));
        $('.container').append('<div class="operationData"></div>');

        RemoveKendoSpan(newClone);
        
        newClone.find(".kendoDropDown").kendoDropDownList();
        newClone.find('select.MethodologyDem').data("kendoDropDownList").enable(false);
        newClone.find('#ResponsibleId').data('kendoDropDownList').select(0);
        newClone.find('#StageId').data('kendoDropDownList').select(0);
        newClone.find('#MethodologyCurrent').data('kendoDropDownList').select(0);

        newClone.find('.datepicker').kendoDatePicker(
        {
            format: "dd MMM yyyy"
        });


        var rowId = newClone.find('#modelItem_EvaluationTrackingId').val() * - 1;
        var placeholder = newClone.find('#modelItem_Name').attr('placeholder') + ' ' + rowId;

        newClone.find('#modelItem_Name').attr('placeholder', placeholder);

        var fold = new TableFold(newClone.find($(".minimizeTable")));
        var tables = [fold];

        collapseFold(fold, tables);

        collapseBtn.btnInput.click(function () {
            massCollapse(collapseBtn, tables);
        });

        newClone.find('.k-grid.k-widget.k-secondary').remove();

        CreateElementEvents_Funding("");

        resizeIframeCloud();
    }

    function validateFields(form) {

        var name = trimString(form.find('.modelItemName').val());
        if (name === '') {
            showValidationWarning(ErrorsMessages.NameError);
            return false;
        }

        var reasons = trimString(form.find('.ExplainChangeReasons').val());
        var invalidReason = reasons === '' &&
            $(this).find('.ExplainChangeReasons').is(':visible');

        if (invalidReason) {
            showValidationWarning(ErrorsMessages.CancelJustificationError);
            return false;
        }

        var date = trimString(form.find('input.datepicker').val());
        if (date === '') {
            showValidationWarning(ErrorsMessages.DateError);
            return false;
        }

        var responsibleInfo = trimString(form.find('.ResponsibleInfo').val());
        var responsibleId = trimString(form.find('.ResponsibleId')
            .find("option:selected").text());

        var invalidResponsible = responsibleInfo === '' && responsibleId !== projectTeam;

        if (invalidResponsible) {
            showValidationWarning(ErrorsMessages.ResponsibleError);
            return false;
        }

        var cancelJustification = trimString(form.find('.cancelJustification textarea').val());
        var stageName = trimString(form.find('.StageId')
            .find("option:selected").text());
        var invalidStage = stageName === cancelledName && cancelJustification === '';

        if (invalidStage) {
            showValidationWarning(ErrorsMessages.CancelError);
            return false;
        }

        var metodologyCurrent = trimString(form.find('.MethodologyCurrent')
            .find("option:selected").text());

        if (metodologyCurrent === '') {
            showValidationWarning(ErrorsMessages.MetodologyCurrentError);
            return false;
        }

        var selectedEvaluation = form.find('#boxEvaluation').find("option:selected")
            .text();

        if (selectedEvaluation === '') {
            showValidationWarning(ErrorsMessages.EvaluationError);
            return false;
        }

        var selectedIntervention = form.find('#boxIntervention').find("option:selected")
            .text();

        if (selectedIntervention === '') {
            showValidationWarning(ErrorsMessages.InterventionError);
            return false;
        }

        var selectedStage = form.find('#StageId').find("option:selected")
            .text();

        if (selectedStage === '') {
            showValidationWarning(ErrorsMessages.StageError);
            return false;
        }

        return true;
    }

})(window.jQuery);

function GridDeleteDocumentClick(node) {
    GridDeleteDocumentClick.node = node;
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    var dialogModal = $('<div />')
        .html('<div class="evaluation-tracking-msg">' + $('#confirmWindow').html() + '</div>');
    $("body").append(dialogModal);
    var title = "Warning";
    var dialog = dialogModal.kendoWindow({
        width: "800px",
        title: title,
        draggable: false,
        resizable: false,
        content: $('#confirmWindow').html(),
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
    dialog.center();
    dialog.open();
}

var GridDeleteDocument = function () {

    idbg.lockUi(null, true);
    var node = $(GridDeleteDocumentClick.node);
    var tr = node.parents('tr');
    var trIndex = tr.index();
    var table = node.parents('table');

    var entityGrid = table.data("kendoGrid");
    var data = entityGrid.dataSource.data()[trIndex];
    GridDeleteDocumentCloseDialog();

    if (data['DocumentEvaluationTrackingId']) {

        var idMatch = data['DocumentEvaluationTrackingId'].match(/value\=\"(.+?)\"/);

        if (idMatch) {
            var documentEvaluationTrackingId = idMatch[1];

            $.ajax({
                type: 'POST',
                url: table.attr('data-delete-documents'),
                data: { 'documentEvaluationTrackingId': documentEvaluationTrackingId },
                success: function (data) {
                    idbg.lockUi(null, false);
                    if (data.responseDeleteDoc == true) {
                        currentMedia = $(node).parents('.gridDiv');
                        $('#completeDocument').trigger('change');
                    }
                }
            });
        }
        return;
    }
    else {
        idbg.lockUi(null, false);
        currentMedia = $(node).parents('.gridDiv');
        $('#completeDocument').trigger('change');
    }
};

var GridDeleteDocumentCloseDialog = function () {

    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();

    if ($(window.parent.document).find('body').find(".ui-widget-overlay")) {
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    }
    if ($(".ui-widget-overlay").length > 0) {
        $(".ui-widget-overlay").remove();
    }
    if ($(".k-window").length > 0) {
        $(".k-window").remove();
    }
    if ($(".k-window-titlebar").length > 0) {
        $(".k-window-titlebar").remove();
    }
};

GridDeleteDocumentClick.node = null;

$('body').on("click mousedown keydown keypress keyup mouseclick",
    '.evaluation-tracking-block.Cancelled .documentsEvaluation', function (event) {
        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();
        $('#lnkShowDisaggregation').focus();
        return false;
    });