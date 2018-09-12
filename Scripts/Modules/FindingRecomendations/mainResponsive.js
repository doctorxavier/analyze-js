
var FRPage = new Object();

FRPage.data = {};

FRPage.noDelayId = null;

FRPage.data.selectRow = function (dataKey, key, value, returnKey) {
    var row = null;

    $.each(FRPage.data[dataKey], function (index, item) {
        if (item[key] == value) {
            row = item;
            return false;
        }
    });

    if (returnKey && row != null) {
        return row[returnKey];
    }
    else if (returnKey) {
        return '';
    }
    else {
        return row;
    }

};

FRPage.data.selectRows = function (dataKey, key, value) {
    var rows = [];

    $.each(FRPage.data[dataKey], function (index, item) {
        if (item[key] == value) {
            rows.push(item);
        }
    });

    return rows;
};

FRPage.table = $('#FindingAndRecommendationTable');
FRPage.table.tbody = FRPage.table.children('tbody');
FRPage.page = $('#FRPage');
FRPage.table.colspan = 4;

FRPage.getCategoriesHTML = function (categories, separator) {
    var html = [];
    $.each(categories ? categories : [], function (index, category_id) {

        var name = FRPage.data.selectRow('categories', 'ConvergenceMasterDataId', category_id, 'Name');
        html.push("<span>" + name + "</span>&nbsp;");

    });

    return html.join(separator ? separator : '');
};


FRPage.getDimensionsHTML = function (categories, separator) {

    var html = [];

    var usedDimensions = [];

    $.each(categories ? categories : [], function (index, category_id) {

        var dimension_id = FRPage.data.selectRow('categories', 'ConvergenceMasterDataId', category_id, 'ParentConvergenceMasterDataId');

        if (usedDimensions.indexOf(dimension_id) >= 0) {
            return;
        }

        usedDimensions.push(dimension_id);

        var name = FRPage.data.selectRow('dimensions', 'ConvergenceMasterDataId', dimension_id, 'Name');
        html.push("<span>" + name + "</span>&nbsp;");

    });

    return html.join(separator ? separator : '');

};

Number.random = function (max, min) {

    if (max == null) max = 100000;
    if (min == null) min = 0;

    if (max <= min) return max;

    var numPosibilidades = max - min;
    var aleatorio = Math.round(Math.random() * numPosibilidades);
    return min + aleatorio;
};

FRPage.uniqID = function (prefix, iterations) {

    if (iterations == null) iterations = 2;

    var units_base = Math.pow(10, iterations);
    var randomNumber = Number.random(units_base - 1);
    var date = new Date();

    return (prefix ? prefix : 'id') + String((date.getTime() * units_base) + randomNumber, 16);
};

FRPage.table.addRow = function (item) {
    if (item == null) {
        item = {
            'FindingRecommendationId': '0',
            'StageId': '',
            'CycleId': '',
            'Stage': 'Stage 1',
            'Categories': [],
            'Finding': '',
            'Recommendation': '',
            'LastUpdateFormatted': FRPage.now,
            'IsDeleted': 'false',
            'RegisterUser': 'everis1-idb',
            'DelayAchivements': [],
            'IsEditable': true,
            'IsCurrentCycle': true
        };
    }

    FRPage.data.selectRow('cycles', 'ItemId', item['CycleId'], 'Name')

    var uniqId = FRPage.uniqID();
    var row = $('<tr />').appendTo(FRPage.table.tbody);
    row.attr({ 'data-finding-id': item['FindingRecommendationId'], 'data-uniqid': uniqId });
    row.data('item', item);

    var rowOdd = FRPage.table.tbody.children('[data-finding-id]').length % 2 != 1;

    if (rowOdd) {
        row.addClass('odd');
    }

    var cycleCell = $('<td />').addClass('view-mode').appendTo(row);
    var cycleText = $('<span />').
        appendTo(cycleCell).
        addClass('text view-mode');

    cycleText.text(item['CycleName']);

    var cycleInput = $('<input />').attr({ 'type': 'hidden', 'name': 'cycle_id' }).val(item['CycleId']);
    cycleInput.appendTo(cycleCell);


    var stageCell = $('<td />').addClass('stage-cell').css({ 'border-right': '0' }).appendTo(row);
    var stageText = $('<span />').
        appendTo(stageCell).
        addClass('text view-mode').
        text(FRPage.data.selectRow('stagesTotal', 'ConvergenceMasterDataId', item['StageId'], 'Name'));

    var stageSelect = $('<select />').
        appendTo(stageCell).
        attr('name', 'stage_id').
        addClass('edit-mode');

    $.each(FRPage.data['stages'], function (index, item) {
        stageSelect.append($("<option />").attr({ 'value': item['ConvergenceMasterDataId'] }).html(item['Name']));
    });

    stageSelect.val(item['StageId']);

    var categoriesCell = $('<td />').
        addClass('categories-cell').
        appendTo(row);

    categoriesCell.append("<span class='categories'>" + FRPage.getCategoriesHTML(item['Categories'], ' - ') + "</span> ");

    var categoriesInput = $('<input type="hidden" />').attr({ 'name': 'categories' }).appendTo(categoriesCell).val(item['Categories'].join(','));
    var categoriesButton = $('<button type="button" />').addClass('edit-mode category-button').attr({ 'data-toggle': "modal", 'data-target': "#categoriesModal" }).appendTo(categoriesCell);
    categoriesButton.html('<span class="icon-bar"></span><span class="icon-bar"></span><span class="icon-bar"></span>');

    var dimensionsCell = $('<td />').
        addClass('dimensions-cell').
        appendTo(row);

    dimensionsCell.append("<span class='dimensions'>" + FRPage.getDimensionsHTML(item['Categories'], ' - ') + "</span>");

    categoriesButton.bind('click', function () {

        var categories = $(this).parents('td:eq(0)').find('input').val().split(',');
        var button = $(this);

        $('#categoriesModal').removeClass('for-search');
        $('#categoriesModal').find('input').attr('checked', false);

        FRPage.categoriesDialogShownCallback = function () {

            $('#categoriesModal').find('input').each(function (index1, checkbox) {

                if ((categories.indexOf($(checkbox).val()) != -1 && !$(checkbox).is(':checked')) || (categories.indexOf($(checkbox).val()) == -1 && $(checkbox).is(':checked'))) {
                    $(checkbox).click();
                }
            });

            FRPage.categoriesCallback = function (categories) {

                var categoriesSpan = button.parents('tr:eq(0)').find('.categories');
                var dimensionsSpan = button.parents('tr:eq(0)').find('.dimensions');
                var input = button.parents('td:eq(0)').find('input');

                categoriesSpan.html(FRPage.getCategoriesHTML(categories, ' - '));
                input.val(categories.join(','));
                dimensionsSpan.html(FRPage.getDimensionsHTML(categories, ' - '));
            };
        };

    });

    var lastUpdateCell = $('<td />').
        addClass('last-updated').
        html(item['LastUpdateFormatted']).
        appendTo(row);

    var buttonsCell = $('<td />').addClass('buttons-cell').css({ 'white-space': 'nowrap', 'rowspan': '6', 'width': '90px' }).appendTo(row);


    var removeButton = $("<button type='button' />").
        addClass('edit-mode buttonTrash').
        css({ 'background': '#FFF' }).
        html('<span class="icon">&#59177</span>').
        bind('click', function () {

            var $this = $(this);
            var row = $this.parents('tr:eq(0)');
            var uniqId = row.attr('data-uniqid');
            var childrenRows = $('tr[data-parent-id=' + String(uniqId) + ']');
            FRPage.confirmAction(FRPage.messages['confirmationMessage'], function () {
                row.remove();
                childrenRows.remove();

                var idFinding = row.attr('data-finding-id');

                $.each(FRPage.data['finding_model']['FindingRecommendations'], function (key, finding) {

                    if (finding['FindingRecommendationId'] == idFinding) {
                        finding['IsDeleted'] = true;
                        return false;
                    }

                });
            });


        }).
        appendTo(buttonsCell);

    var expandButton = $("<button type='button' />").
        html('<span class="icon">&#9662;</span>').
        addClass('collapsed expand-button').
        css({ 'background': 'transparent', 'border': '0', 'font-size': '14pt' }).
        bind('click', function () {

            var $this = $(this);
            var row = $this.parents('tr:eq(0)');
            var uniqId = row.attr('data-uniqid');
            var childrenRows = $('tr[data-parent-id=' + String(uniqId) + ']');

            if ($this.hasClass('collapsed')) {
                $this.removeClass('collapsed');
                $this.addClass('expanded');
                childrenRows.show();
                $this.html('<span class="icon">&#9652;</span>');
            }
            else {
                $this.addClass('collapsed');
                $this.removeClass('expanded');
                childrenRows.hide();
                $this.html('<span class="icon">&#9662;</span>');
            }

        }).
        appendTo(buttonsCell);

    var findingRow = $('<tr />').attr({ 'data-parent-finding-id': item['FindingRecommendationId'], 'data-parent-id': uniqId }).hide().appendTo(FRPage.table.tbody);

    if (rowOdd) {
        findingRow.addClass('odd');
    }

    var findingLabelCell = $('<td />').attr('colspan', 1).css({ 'border-right': '0', 'vertical-align': 'top', 'width': '200px' }).appendTo(findingRow);
    var labelFinding = $('<label />').html(FRPage.messages['findings']).appendTo(findingLabelCell);
    findingLabelCell.append('<span class="edit-mode red"> *</span>');

    var findingInputCell = $('<td />').attr('colspan', FRPage.table.colspan).appendTo(findingRow);
    var inputViewFinding = $('<textarea />').addClass('view-mode').attr({ 'readonly': 'readonly' }).val(item['Finding']).css('width', '100%').appendTo(findingInputCell);
    var inputEditFinding = $('<textarea />').addClass('edit-mode').attr({ 'name': 'finding' }).val(item['Finding']).css('width', '100%').appendTo(findingInputCell);

    $('<td />').addClass('view-mode').attr('colspan', 1).appendTo(findingRow);

    var recomendationRow = $('<tr />').attr({ 'data-parent-finding-id': item['FindingRecommendationId'], 'data-parent-id': uniqId }).hide().appendTo(FRPage.table.tbody);

    if (rowOdd) {
        recomendationRow.addClass('odd');
    }

    var recomendationLabelCell = $('<td />').attr('colspan', 1).css({ 'border-right': '0', 'vertical-align': 'top' }).appendTo(recomendationRow);
    var labelRecomendation = $('<label />').html(FRPage.messages['recommendations']).appendTo(recomendationLabelCell);
    recomendationLabelCell.append('<span class="edit-mode red"> *</span>');

    var recomendationInputCell = $('<td />').attr('colspan', FRPage.table.colspan).appendTo(recomendationRow);

    var inputViewRecomendation = $('<textarea />').addClass('view-mode').attr({ 'readonly': 'readonly' }).val(item['Recommendation']).css('width', '100%').appendTo(recomendationInputCell);
    var inputEditRecomendation = $('<textarea />').addClass('edit-mode').attr({ 'name': 'recommendation' }).val(item['Recommendation']).css('width', '100%').appendTo(recomendationInputCell);

    $('<td />').addClass('view-mode').attr('colspan', 1).appendTo(recomendationRow);

    var delaysRow = $('<tr />').hide().attr({ 'data-parent-finding-id': item['FindingRecommendationId'], 'data-parent-id': uniqId }).appendTo(FRPage.table.tbody);

    if (rowOdd) {
        delaysRow.addClass('odd');
    }

    var emptyCell = $('<td />').html('&nbsp;').appendTo(delaysRow);

    var delaysCell = $('<td />').attr('colspan', FRPage.table.colspan).appendTo(delaysRow);

    delaysCell.append($('<label />').html(FRPage.messages['issueCausedDelay']));
    delaysCell.append($('<select />').css({ 'margin-left': '10px', 'width': '100px' }).addClass('edit-mode').html('<option value="">No</option><option value="1">Yes</option>'));
    delaysCell.append($('<span />').addClass('view-mode').css({ 'padding-left': '10px', 'font-weight': 'bold' }).html(item['DelayAchivements'].length ? 'Yes' : 'No'));

    delaysCell.find('select').bind('change', function () {

        if ($(this).val()) {
            delaysCell.find('table.delays').show();
            delaysCell.find('.delays-button').css('visibility', 'visible');
        }
        else {
            delaysCell.find('table.delays').hide();
            delaysCell.find('.delays-button').css('visibility', 'hidden');
        }
    });


    var delaysTable = $('<table />').addClass('delays').css({ 'width': '100%' }).appendTo(delaysCell);
    var delaysThead = $('<thead />').appendTo(delaysTable);
    delaysThead.html("<tr><th style='width: 15%'>Type of Delay</th><th style='width: 65%'>Name</th><th>" + FRPage.messages['issueResolved'] + "</th><th>&nbsp;</th></tr>");
    var delaysTbody = $('<tbody />').addClass('delays').appendTo(delaysTable);

    delaysCell.append('<div class="centerAlign addOneWrapper centerContent verticalMargin20 edit-mode delays-button"><div style="font-size:12px" class="addOneContainer w290 h100"><input type="button" class="noBorder 100 addOne cursorPointer w290 ac-PlanYear" value="New Delay" onclick="FRPage.table.addDelay(this)" /></div></div>');

    delaysCell.find('select').val(item['DelayAchivements'].length > 0 ? '1' : '').trigger('change');

    var buttonDelay = delaysCell.find('.delays-button');

    $('<td />').addClass('view-mode').attr('colspan', 1).appendTo(delaysRow);

    $.each(item['DelayAchivements'], function (index2, delay) {
        FRPage.table.addDelay(buttonDelay, delay);
    });

    FRPage.table.css({ 'border': 'solid 1px #AAA' });
    
    if (!item['IsCurrentCycle']) {
        var $mainRow = $('[data-finding-id=' + item['FindingRecommendationId'] + ']');
        $mainRow.find('select').prop('disabled', true);
        $mainRow.find('button.buttonTrash').hide();
        $mainRow.find('.category-button').hide();

        var $childrenRows = $('[data-parent-finding-id=' + item['FindingRecommendationId'] + ']');
        $childrenRows.find('textarea').prop('disabled', true);
        $childrenRows.find('button.buttonTrash').hide();
        $childrenRows.find('.addOneWrapper').hide();
    }

};

FRPage.table.addDelay = function (button, delay) {
    if (delay == null) {
        delay = {
            'AchievementDelayId': '0',
            'DelayTypeId': '',
            'IsSolved': false,
            'ResultElementName': '',
            'LastUpdateFormatted': FRPage.now
        };
    }

    var delaysTable = $(button).parents('td:eq(0)').find('table.delays tbody');

    var row = $('<tr />').attr({ 'AchievementDelayId': delay['AchievementDelayId'] }).appendTo(delaysTable);

    var typeCell = $('<td />').appendTo(row).css({ 'width': '15%' });

    var typeText = $('<span />').
        appendTo(typeCell).
        text(FRPage.data.selectRow('type_delays', 'ConvergenceMasterDataId', delay['DelayTypeId'], 'Name')).
        addClass('text view-mode');

    var typeSelect = $('<select />').
        appendTo(typeCell).
        attr('name', 'type_id').
        css({ 'width': 200 }).
        addClass('edit-mode');

    $.each(FRPage.data['type_delays'], function (index, item) {

        if (item['Code'] != 'OTHERDELAYNA') {
            typeSelect.append($("<option />").attr({ 'value': item['ConvergenceMasterDataId'] }).html(item['Name']));
        }

    });

    typeSelect.css({ 'width': '100% !important' }).val(delay['DelayTypeId']);

    var nameCell = $('<td />').css({ 'width': '65%' }).addClass('name-cell').appendTo(row);

    var selectedName = '';

    $.each(FRPage.data['type_delays'], function (index3, type_delay) {

        if (type_delay['ConvergenceMasterDataId'] == delay['DelayTypeId']) {
            var names = type_delay['names'];
            $.each(names ? names : [], function (index2, item) {
                if (item['ConvergenceMasterDataId'] == delay['ResultElementName']) {
                    selectedName = item['NameEn'];
                }
            });
        }
    });

    var nameText = $('<span />').
        appendTo(nameCell).
        addClass('text view-mode').
        text(selectedName);

    var otherDelayText = $('<div />').
        appendTo(nameCell).
        addClass('otherdelaytext').
        hide().
        addClass('text view-mode').
        text(FRPage.messages['otherDelayText']);

    var nameSelect = $('<select />').
        appendTo(nameCell).
        attr('name', 'name').
        addClass('edit-mode');


    typeSelect.bind('change', function () {

        var typeId = $(this).val();
        var nameSelect = $(this).parents('tr:eq(0)').find('[name=name]');
        nameSelect.empty();

        nameSelect.append("<option value='(select option)'>&nbsp;</option>");

        $.each(FRPage.data['type_delays'], function (index3, type_delay) {

            if (type_delay['ConvergenceMasterDataId'] == typeId) {

                var names = type_delay['names'];

                if ((type_delay['Code'] == 'OTHERDELAY' || type_delay['Code'] == 'OTHERDELAYNA') || names.length == 0) {
                    nameCell.addClass('otherdelay-cell');

                    if (type_delay['Code'] == 'OTHERDELAY' || type_delay['Code'] == 'OTHERDELAYNA') {
                        otherDelayText.text(FRPage.messages['otherDelayText']);
                    }
                    else if (type_delay['Code'] == 'OUTCDEL') {
                        otherDelayText.text(FRPage.messages['noOutcomesText']);
                    }
                    else if (type_delay['Code'] == 'OUTPDEL') {
                        otherDelayText.text(FRPage.messages['noOutputsText']);
                    }
                }
                else {
                    nameCell.removeClass('otherdelay-cell');
                }

                $.each(names, function (index2, item) {
                    nameSelect.append("<option value='" + String(item['ConvergenceMasterDataId']) + "'>" + String(item['NameEn']) + "</option>");
                });
            }
        });
    });

    typeSelect.val(delay['DelayTypeId']);
    typeSelect.trigger('change');
    nameSelect.val(delay['ResultElementName']);

    var resolvedCell = $('<td />').appendTo(row);

    resolvedCell.append($('<select />').attr({ 'name': 'solved' }).addClass('edit-mode').html('<option value="">No</option><option value="1">Yes</option>'));
    resolvedCell.append($('<span />').addClass('view-mode').css({ 'padding-left': '10px', 'font-weight': 'bold' }).html(FRPage.checkBool(delay['IsSolved']) ? 'Yes' : 'No'));
    resolvedCell.find('select').val(FRPage.checkBool(delay['IsSolved']) ? '1' : '');

    var buttonsCell = $('<td />').appendTo(row);
    var removeButton = $("<button type='button edit-mode' />").
        addClass('edit-mode buttonTrash').
        html('<span class="icon">&#59177</span>').
        bind('click', function () {

            FRPage.confirmAction(FRPage.messages['confirmationMessage'], function () {
                row.remove();
            });

        }).
        appendTo(buttonsCell);
};

FRPage.updateTable = function (useFilters) {

    if (arguments.length == 0) {
        useFilters = true;
    }

    var findings = FRPage.data['finding_model']['FindingRecommendations'];

    if (!findings) {
        FRPage.data['finding_model']['FindingRecommendations'] = [];
    }

    if (useFilters) {
        var newFindings;

        var cycleId = $('#filter-cycle').val();

        if (cycleId && (cycleId.length > 1 || (cycleId.length == 1 && cycleId[0]))) {

            newFindings = [];

            $.each(findings, function (index, finding) {

                if (cycleId.indexOf(String(finding['CycleId'])) >= 0) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }

        var stageId = $('#filter-stage').val();

        if (stageId) {
            newFindings = [];

            $.each(findings, function (index, finding) {

                if (finding['StageId'] == stageId) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }

        var categories = $('#filter-categories').val();

        if (categories) {
            newFindings = [];
            categories = categories.split(',');

            $.each(findings ? findings : [], function (index, finding) {

                var found = false;

                $.each(finding['Categories'], function (index3, category_id) {

                    if (categories.indexOf(String(category_id)) >= 0) {
                        found = true;
                        return false;
                    }
                });

                if (found) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }


        var delayType = $('#filter-delay-type').val();

        if (delayType) {
            newFindings = [];

            $.each(findings ? findings : [], function (index, finding) {

                var found = false;

                if(FRPage.noDelayId == delayType)
                {
                    found = finding['DelayAchivements'] && finding['DelayAchivements'].length == 0;
                }
                else
                {
                    $.each(finding['DelayAchivements'] ? finding['DelayAchivements'] : [], function (index3, delay) {

                        if (delay['DelayTypeId'] == delayType) {
                            found = true;
                            return false;
                        }
                    });    
                }
                

                if (found) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }


        var delayName = $('#filter-delay-name').val();

        if (delayName && (!delayType || delayType != FRPage.noDelayId)) {
            
            newFindings = [];

            $.each(findings ? findings : [], function (index, finding) {

                var found = false;

                $.each(finding['DelayAchivements'], function (index3, delay) {

                    if (delay['ResultElementName'] == delayName) {
                        found = true;
                        return false;
                    }
                });

                if (found) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }


        var solved = $('#filter-solved').val();

        if (solved != '') {
            newFindings = [];

            $.each(findings ? findings : [], function (index, finding) {

                var found = false;

                $.each(finding['DelayAchivements'], function (index3, delay) {

                    if (delay['IsSolved'] != null) {

                        if (solved == '1' && FRPage.checkBool(delay['IsSolved'])) {
                            found = true;
                            return false;
                        }

                        else if (solved == '0' && !FRPage.checkBool(delay['IsSolved'])) {
                            found = true;
                            return false;
                        }

                    }
                });

                if (found) {
                    newFindings.push(finding);
                }
            });

            findings = newFindings;
        }

    }

    FRPage.table.tbody.empty();

    findings = findings ? findings : [];

    $.each(findings, function (index, item) {
        FRPage.table.addRow(item);
    });


};

FRPage.switchEditMode = function () {
    FRPage.page.removeClass('view-mode');
    FRPage.page.addClass('edit-mode');

    $.each(FRPage.data['finding_model']['FindingRecommendations'], function (key, finding) {
        finding['IsDelete'] = false;
    });
};

FRPage.switchViewMode = function (confirm) {

    FRPage.page.removeClass('edit-mode');
    FRPage.page.addClass('view-mode');
};

FRPage.getFindingById = function (id) {

    var finding = null;

    $.each(FRPage.data['finding_model']['FindingRecommendations'], function (index, item) {
        if (item['FindingRecommendationId'] == id) {
            finding = item;
            return false;
        }
    });

    return finding;
};

FRPage.saveData = function () {

    var saveFindings = [];
    var newFindings = [];
    var canSave = true;

    FRPage.table.tbody.find('.input-error').removeClass('input-error');

    FRPage.table.tbody.find('tr[data-finding-id]').each(function (index, rowItem) {

        var $rowItem = $(rowItem);
        var findingRow = $rowItem.next();
        var recommendationRow = findingRow.next();
        var detailsRow = recommendationRow.next();

        var finding = {
            'FindingRecommendationId': $rowItem.attr('data-finding-id'),
            'StageId': $rowItem.find('[name=stage_id]').val(),
            'CycleId': $rowItem.find('[name=cycle_id]').val(),
            'Categories': $rowItem.find('[name=categories]').val() ? $rowItem.find('[name=categories]').val().replace(/\s+/g, '').split(',') : [],
            'Finding': findingRow.find('textarea.edit-mode').val(),
            'IsDeleted': false,
            'LastUpdateFormatted': $rowItem.find('.last-updated').text(),
            'Recommendation': recommendationRow.find('textarea.edit-mode').val(),
            'DelayAchivements': []
        };

        finding['IsCurrentCycle'] = $rowItem.data('item')['IsCurrentCycle'];

        if (!finding['IsCurrentCycle']) {
            return true;
        }

        detailsRow.find('[achievementdelayid]').each(function (index3, row) {

            var $rowDelay = $(this);

            var typeId = $rowDelay.find('[name=type_id]').val();
            var name = $rowDelay.find('[name=name]').val();

            if (typeId && name) {
                finding['DelayAchivements'].push({
                    'AchievementDelayId': $rowDelay.attr('achievementdelayid'),
                    'DelayTypeId': $rowDelay.find('[name=type_id]').val(),
                    'ResultElementName': $rowDelay.find('[name=name]').val(),
                    'IsSolved': $rowDelay.find('[name=solved]').val() == '1' ? true : false
                });
            }
        });

        if (!finding['StageId']) {
            $rowItem.find('[name=stage_id]').addClass('input-error');
        }

        if (!finding['Finding']) {
            findingRow.find('textarea.edit-mode').addClass('input-error');
        }

        canSave = finding['StageId'] && finding['Categories'].length > 0 && finding['Finding'];

        if (!canSave) {
            showMessage(FRPage.messages['validationMessage']);
            return false;
        }

        newFindings.push(finding);
        saveFindings.push(finding);

    });


    if (!canSave) return;

    FRPage.confirmAction(FRPage.messages['confirmationMessage'], function () {

        $.each(FRPage.data['finding_model']['FindingRecommendations'] ? FRPage.data['finding_model']['FindingRecommendations'] : [], function (key1, existingFinding) {

            if ($('tr[data-finding-id=' + existingFinding['FindingRecommendationId'] + ']').length == 0) {

                if (existingFinding['IsDeleted'] && existingFinding['IsCurrentCycle']) {
                    saveFindings.push({
                        'FindingRecommendationId': existingFinding['FindingRecommendationId'],
                        'StageId': existingFinding['StageId'],
                        'CycleId': existingFinding['CycleId'],
                        'Categories': [],
                        'Finding': '-',
                        'IsDeleted': true,
                        'Recommendation': '-',
                        'DelayAchivements': [],
                        'IsCurrentCycle': true
                    });
                }
                else {
                    newFindings.push(existingFinding);
                }

            }

        });

        $.each(saveFindings, function (index, finding) {

            var oldFinding = FRPage.getFindingById(finding['FindingRecommendationId']);

            if (oldFinding && JSON.stringify(oldFinding) != JSON.stringify(finding)) {
                saveFindings[index]['LastUpdateFormatted'] = FRPage.now;
            }
        });

        FRPage.data['finding_model']['FindingRecommendations'] = saveFindings;

        delete FRPage.data['finding_model']['DeleteFindingId'];

        $.ajax({
            'url': FRPage.saveURL,
            'type': 'post',
            'data': FRPage.data['finding_model'],
            'success': function (data) {

                if (!data) {
                    alert('There was an error');
                    return;
                }
                else if (data && !FRPage.checkBool(data['isValid'])) {
                    alert(data['message']);
                    return;
                }

                FRPage.data['finding_model']['FindingRecommendations'] = newFindings;
                FRPage.switchViewMode();
                location.reload();

            },
            'error': function () {
                alert('There was an error');
                location.reload();
            }
        });
    });
};


FRPage.discardData = function () {
    FRPage.confirmAction(FRPage.messages['cancelMessage'], function () {
        FRPage.switchViewMode();
        FRPage.updateTable();
    });
};


FRPage.confirmAction = function (message, callback) {
    var promise = $.Deferred();
    var modal = vex.dialog.confirm({
        message: message,
        afterOpen: function ($vexContent) {
            $vexContent.prepend($('<h3 class="h3-padding modalWarning">Warning</h3>'));
        },
        deleteContent: true,
        callback: function (value) {
            if (value && callback) {
                callback.call(this);
            }
        }
    });
};

FRPage.categoriesDialogShownCallback = function () { };

FRPage.categoriesDialogHiddenCallback = function () { };

FRPage.categoriesCallback = function (categories) { };

FRPage.makeCategoriesDialogHTML = function () {
    var block = $('#categoriesModal .modal-body');
    var row = $('<div />').addClass('row').appendTo(block);
    var ulContainer = $('<div />').addClass('col-md-6').appendTo(row);
    var ul = $('<ul />').appendTo(ulContainer);
    var ulIndex = 0;

    $.each(FRPage.data['dimensions'] ? FRPage.data['dimensions'] : [], function (index, dimension) {

        var categories = FRPage.data.selectRows('categories', 'ParentConvergenceMasterDataId', dimension['ConvergenceMasterDataId']);

        if (!categories || categories.length == 0) {
            return;
        }

        ulIndex++;

        if (ulIndex == 3) {
            ulContainer = $('<div />').addClass('col-md-6').appendTo(row);
            ul = $('<ul />').appendTo(ulContainer);
        }

        var dimensionItem = $('<li />').addClass('dimension').html(dimension['Name']).appendTo(ul);


        $.each(categories, function (index1, category) {

            var categoryItem = $('<li />').appendTo(ul).addClass('category');

            var inputCheckbox = $('<input type="checkbox" />').attr({ 'name': 'category', 'id': 'category_' + String(category['ConvergenceMasterDataId']) }).val(category['ConvergenceMasterDataId']).appendTo(categoryItem);
            var label = $('<label />').attr({ 'for': 'category_' + String(category['ConvergenceMasterDataId']) }).html(category['Name']).css({ 'cursor': 'pointer' }).appendTo(categoryItem);

            inputCheckbox.bind('click', function (evt) {

                var inputs = $('#categoriesModal .modal-body').find('input:checked')

                if (inputs.length > 3 && !$('#categoriesModal').hasClass('for-search')) {
                    evt.preventDefault();
                    evt.stopPropagation();
                    return false;
                }

            });

            $('#categoriesModal').bind('shown.bs.modal', function () {
                var inputs = $('#categoriesModal').find('input:checked').click();
                FRPage.categoriesDialogShownCallback();

            });

            $('#categoriesModal').bind('hidden.bs.modal', function () {
                FRPage.categoriesDialogHiddenCallback();
            });

            $('#categoriesModal').find('[data-dismiss][data-save]').bind('click', function () {

                var categories = [];
                var inputs = $('#categoriesModal').find('input:checked');

                inputs.each(function (index, item) {
                    categories.push($(item).val());
                });

                FRPage.categoriesCallback(categories);
            });

        });

    });

};


FRPage.checkBool = function (value) {
    if (value === true || String(value).toLowerCase() == 'true') {
        return true;
    }

    return false;
};


FRPage.loadFilters = function () {
    var row, col, input;

    var container = $('#fr-search-block .container');

    row = $('<div />').appendTo(container).addClass('row');

    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html('PMR Cycle').appendTo(col);
    col.append('<br />');
    input = $('<select />').attr({ 'name': 'cycle_id', 'id': 'filter-cycle', 'multiple': 'multiple' }).appendTo(col);
    input.append($("<option />").attr({ 'value': '' }).html('All Cycles'));
    var lastCycle = FRPage.data['cycles'][FRPage.data['cycles'].length - 1];
    input.append($("<option />").attr({ 'value': lastCycle['ItemId'] }).html('Current Cycle'));
    $.each(FRPage.data['cycles'], function (index, item) {
        input.append($("<option />").attr({ 'value': item['ItemId'] }).html(item['Name']));
    });


    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html(FRPage.messages['stage']).appendTo(col);
    col.append('<br />');
    input = $('<select />').attr({ 'name': 'stage_id', 'id': 'filter-stage' }).appendTo(col);
    input.append($("<option />").attr({ 'value': '' }).html('(select option)'));
    $.each(FRPage.data['stages'], function (index, item) {
        input.append($("<option />").attr({ 'value': item['ConvergenceMasterDataId'] }).html(item['Name']));
    });

    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html(FRPage.messages['categories']).appendTo(col);
    $('<input type="hidden" />').attr({ 'name': 'categories', 'id': 'filter-categories' }).appendTo(col);
    var categoriesButton = $('<button type="button" />').addClass('category-button').attr({ 'data-toggle': "modal", 'data-target': "#categoriesModal" }).appendTo(col);
    categoriesButton.html('<span class="icon-bar"></span><span class="icon-bar"></span><span class="icon-bar"></span>');
    col.append('<br />');
    $('<div />').addClass('categories-list').attr({ 'id': 'filter-categories-list' }).appendTo(col);

    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html(FRPage.messages['dimensions']).appendTo(col);
    col.append('<br />');
    $('<div />').addClass('dimensions-list').attr({ 'id': 'filter-dimensions-list' }).appendTo(col);


    categoriesButton.bind('click', function () {

        var categories = $(this).prev().val().split(',');
        var button = $(this);

        $('#categoriesModal').addClass('for-search');

        FRPage.categoriesDialogShownCallback = function () {

            $('#categoriesModal').find('input').each(function (index1, checkbox) {

                if ((categories.indexOf($(checkbox).val()) != -1 && !$(checkbox).is(':checked')) || (categories.indexOf($(checkbox).val()) == -1 && $(checkbox).is(':checked'))) {
                    $(checkbox).click();
                }
            });

            FRPage.categoriesCallback = function (categories) {

                $('#filter-categories-list').html(FRPage.getCategoriesHTML(categories));
                $('#filter-categories').val(categories.join(','));
                $('#filter-dimensions-list').html(FRPage.getDimensionsHTML(categories));
            };
        };

    });

    row = $('<div />').appendTo(container).addClass('row').css({ 'padding': '0 15px' });

    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html('Types of Delays').appendTo(col);
    col.append('<br />');

    var typeSelect = $('<select />').
        appendTo(col).
        attr({ 'name': 'type_id', 'id': 'filter-delay-type' });

    typeSelect.append($("<option />").attr({ 'value': '' }).html('(select option)'));
    $.each(FRPage.data['type_delays'], function (index, item) {
        typeSelect.append($("<option />").attr({ 'value': item['ConvergenceMasterDataId'] }).html(item['Name']));
    });


    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html('Names').appendTo(col);
    col.append('<br />');

    var nameSelect = $('<select />').
        appendTo(col).
        attr({ 'name': 'name_id', 'id': 'filter-delay-name' });

    typeSelect.bind('change', function () {

        var typeId = $(this).val();
        nameSelect.empty();

        if (this.selectedIndex > 0) {

            nameSelect.attr('disabled', false);
            nameSelect.append($("<option />").attr({ 'value': '' }).html('(select option)'));

            $.each(FRPage.data['type_delays'], function (index3, type_delay) {

                var names = [];

                if (type_delay['ConvergenceMasterDataId'] == typeId) {

                    $.each(type_delay['names'], function (index2, item) {

                        if (FRPage.page.allowedDelays.indexOf(String(item['ConvergenceMasterDataId'])) >= 0)
                        {
                            names.push(item);
                            nameSelect.append("<option value='" + String(item['ConvergenceMasterDataId']) + "'>" + String(item['NameEn']) + "</option>");
                        }
                    });
                }

                if (names.length == 1 && (type_delay['Code'] == 'OTHERDELAYNA' || type_delay['Code'] == 'OTHERDELAY')) {
                    nameSelect.get(0).selectedIndex = 1;
                }

            });
        }
        else {
            nameSelect.attr('disabled', 'disabled');
        }

    });

    typeSelect.trigger('change');


    col = $('<div />').appendTo(row).addClass('col-md-3');
    $('<label />').html(FRPage.messages['issueResolved']).appendTo(col);
    col.append('<br />');
    col.append($('<select />').attr({ 'name': 'solved', 'id': 'filter-solved' }).html('<option value=""></option><option value="0">No</option><option value="1">Yes</option>'));

    row = $('<div />').appendTo(container).addClass('row');

    row.css({ 'text-align': 'right', 'padding-right': '20px' });

    var clearButton = $('<button type="button" />').addClass('buttonBlue button-clear').html('Clear').appendTo(row);
    clearButton.bind('click', function () {

        $('#fr-search-block select').val('').trigger('change');
        $('#fr-search-block input[type=hidden]').val('').trigger('change');
        $('#fr-search-block .categories-list,#fr-search-block .dimensions-list').html('');
    });

    var searchButton = $('<button type="button" />').addClass('buttonBlue button-search').html('Filter').appendTo(row);
    searchButton.bind('click', function () {
        FRPage.updateTable();
    });
};

FRPage.makeDimensionsDialogHTML = function () {

    //return;
    var modalBody = $('#dimensionsModal .modal-body');
    var table = $('<table />').appendTo(modalBody);
    var thead = $('<thead />').appendTo(table);

    var tr = $('<tr />').appendTo(thead);
    tr.append("<th>Dimensions</th>");
    tr.append("<th>Categories</th>");
    tr.append("<th>Specific factors (examples)</th>");

    var tbody = $('<tbody />').appendTo(table);

    $.each(FRPage.data['dimensions'], function (dindex, dimension) {

        var categories = FRPage.data.selectRows('categories', 'ParentConvergenceMasterDataId', dimension['ConvergenceMasterDataId']);

        if (categories.length == 0) {
            return;
        }

        tr = $('<tr />').appendTo(tbody);

        if (dindex % 2 != 0) {
            tr.addClass('odd');
        }

        $('<td />').addClass('dimension').attr({ 'rowspan': categories.length }).html(dimension['Name']).appendTo(tr);

        $.each(categories, function (cindex, category) {

            if (category['Examples']) {
                $('<td />').addClass('category').html(category['Name']).appendTo(tr);
                $('<td />').addClass('category-examples').html(category['Examples'] ? category['Examples'] : "EXAMPLES").appendTo(tr);
                tr = $('<tr />').appendTo(tbody);
            }
        });

    });
    
    tbody.children().eq(1).addClass('gray');
    tbody.children().eq(5).addClass('gray');
    tbody.children().eq(8).addClass('gray');
    tbody.children().eq(12).addClass('gray');

};


FRPage.updateAllowedDelays = function () {
    FRPage.page.allowedDelays = [];

    $.each(FRPage.data['finding_model']['FindingRecommendations'], function (index, finding) {

        $.each(finding['DelayAchivements'] ? finding['DelayAchivements'] : [], function (index1, delay) {

            if (FRPage.page.allowedDelays.indexOf(delay['ResultElementName']) == -1) {
                FRPage.page.allowedDelays.push(delay['ResultElementName']);
            }

        });

    });
};

FRPage.dataFullLoaded = function () {

    var fullLoaded = true;

    $.each(FRPage.dataKeys, function (index, name) {

        if (FRPage.data[name] == null) {
            fullLoaded = false;
            return false;
        }
    });

    if (fullLoaded) {

        var findings = FRPage.data['finding_model']['FindingRecommendations'];
        FRPage.data['finding_model']['FindingRecommendations'] = [];

        $.each(findings, function(index2, finding) {
            FRPage.data['finding_model']['FindingRecommendations'].push(finding);        
        });
        
        FRPage.updateAllowedDelays();
        FRPage.page.css('visibility', 'visible');

        FRPage.makeCategoriesDialogHTML();

        if (!FRPage.checkEditable()) {
            FRPage.page.find('.button-edit').remove();
        }
        else
        {
         FRPage.page.find('.button-edit').css({'visibility': 'visible'});
        }

        $.each(FRPage.data['type_delays'], function (key, item) {

            if (item['Code'] == 'OUTCDEL') {

                FRPage.data['type_delays'][key]['names'] = [];

                $.each(FRPage.data['outcomes_delays'], function (index2, outcome) {

                    if (outcome && outcome['ConvergenceMasterDataId']) {
                        FRPage.data['type_delays'][key]['names'].push(outcome);
                    }

                });


            }
            else if (item['Code'] == 'OUTPDEL') {

                FRPage.data['type_delays'][key]['names'] = [];

                $.each(FRPage.data['outputs_delays'], function (index2, output) {

                    if (output && output['ConvergenceMasterDataId']) {
                        FRPage.data['type_delays'][key]['names'].push(output);
                    }

                });
            }
            else if (item['Code'] == 'OTHERDELAY' || item['Code'] == 'OTHERDELAYNA') {

                if(item['Code'] == 'OTHERDELAYNA')
                {
                    FRPage.noDelayId = item['ConvergenceMasterDataId'];
                }
                
                FRPage.data['type_delays'][key]['names'] = [
                    {
                        'ConvergenceMasterDataId': '0',
                        'NameEn': 'N/A'
                    }
                ];
            }

        });


        FRPage.makeDimensionsDialogHTML();
        FRPage.loadFilters();

        FRPage.switchViewMode();
        FRPage.updateTable();
    }

};

FRPage.checkEditable = function () {
    return (FRPage.checkBool(FRPage.data['finding_model']['HasRMAdministratorRole']) || FRPage.checkBool(FRPage.data['finding_model']['IsCurrentPcrTaskLessThanFive'])) && FRPage.checkBool(FRPage.data['finding_model']['HasFindingsWritePermission']) && FRPage.checkBool(FRPage.data['finding_model']['IsEditable']);
};


FRPage.dataLoad = function (key, url) {


    $.ajax({
        'url': url,
        'type': 'post',
        'success': function (data) {


            var originalData = data;

            if (key == 'stages' && data) {
                var newData = [];
                $.each(data, function (index, value) {
                    if (value && value['Name'].match(/.+\/.+/)) {
                        newData.push(value);
                    }
                });

                data = newData;

                FRPage.data['stagesTotal'] = originalData;
            }

            if (key == 'categories' && data) {

                var newData = [];

                $.each(data, function (index, value) {
                    if (value) {
                        value['Name'] = value['Name'].replace(/^\w{1,2}\.\s*/, '')
                        newData.push(value);
                    }
                });

                data = newData;
            }

            if (key == 'dimensions' && data) {

                var newData = [];

                $.each(data, function (index, value) {
                    if (value && (value['Code'] == 'FR_DIM_TEC' || value['Code'] == 'FR_DIM_ORG' || value['Code'] == 'FR_DIM_PUB' || value['Code'] == 'FR_DIM_LEG' || value['Code'] == 'FR_DIM_FID' || value['Code'] == 'FR_DIM_REL')) {
                        newData.push(value);
                    }
                });

                data = newData;
            }

            FRPage.data[key] = data ? data : [];


            if (!data['FindingRecommendations']) {
                data['FindingRecommendations'] = [];
            }

            FRPage.dataFullLoaded();

        },
        'error': function () {
            FRPage.data[key] = [];
            FRPage.dataFullLoaded();
        }
    });
};

FRPage.toggleFilters = function (button) {
    var filtersBlock = $('#fr-search-block');
    filtersBlock.toggle();

    if (filtersBlock.is(':visible')) {
        $(button).html('Hide Filter');
    }
    else {
        $(button).html('Show Filter');
    }
};

FRPage.expandAll = function () {
    $('[data-expand-button="expand"]').hide();
    $('[data-expand-button="collapse"]').show();
    $('.expand-button.collapsed').click();
};

FRPage.collapseAll = function () {
    $('[data-expand-button="expand"]').show();
    $('[data-expand-button="collapse"]').hide();
    $('.expand-button.expanded').click();
};

FRPage.dataKeys = ['cycles', 'dimensions', 'categories', 'stages', 'finding_model', 'outputs_delays', 'outcomes_delays', 'type_delays'];

$.each(FRPage.dataKeys, function (index, name) {
    FRPage.data[name] = null;
});

$(document).ready(function () {
    $('.informationplace .icoInformation').css({ 'cursor': 'pointer' }).attr({ 'data-toggle': "modal", 'data-target': "#dimensionsModal" });
});

$(document).on('click', '.buttons-cell', correctIframeHeight);