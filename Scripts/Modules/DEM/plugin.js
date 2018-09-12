$(document).on('click', '[name="objectivesAligned"]', function (e) {
    var source = $(this);
    var valueSource = source.val();
    var regionAligned = $('div[data-content="aligned"]');
    var regionNotAligned = $('div[data-content="notAligned"]');
    var asteriskAligned = $('span[data-name="asteriskAligned"]');
    var asteriskNotAligned = $('span[data-name="asteriskNotAligned"]');
    var comboAligned = $('[name="objectives"]');
    var comboNotAligned = $('[name="objectivesNotAligned"]');

    var isIncludeOpr = $('input[name=hiddenIndicativePipelines]').val();

    var regionRelevanceOperation = $("div[name=regionRelevanceOperation]");
    var enRelevanceArea = $("textarea[name=enRelevanceArea]");
    var esRelevanceArea = $("textarea[name=esRelevanceArea]");

    if (valueSource == 'aligned') {
        regionNotAligned.addClass('hide');
        regionAligned.removeClass('hide');
        var closeSelected = regionNotAligned.find('a.search-choice-close');
        
        closeSelected.each(function () {
            $(this).click();
        });
        regionNotAligned.find('select').prop('disabled', true);
        asteriskNotAligned.addClass('hide');
        asteriskAligned.removeClass('hide');
        comboAligned.removeAttr('disabled');
        comboAligned.attr('required', 'required');
        comboNotAligned.removeAttr('required');
        var errorList = regionNotAligned.find('#parsley-id-multiple-objectivesNotAligned');
        errorList.removeClass("filled");

        if (isIncludeOpr != undefined) {
            regionRelevanceOperation.addClass('hide');
            enRelevanceArea.attr("data-force-parsley-validation", "false");
            enRelevanceArea.attr("data-parsley-required", "false");
            esRelevanceArea.attr("data-force-parsley-validation", "false");
            esRelevanceArea.attr("data-parsley-required", "false");
        }
    } else {
        regionNotAligned.removeClass('hide');
        regionAligned.addClass('hide');
        regionAligned.find('select').val('');
        var closeSelected = regionAligned.find('a.search-choice-close');
        var errorList = regionAligned.find('#parsley-id-multiple-objectives');
        errorList.removeClass("filled");
        closeSelected.each(function () {
            $(this).click();
        });
        regionAligned.find('select').prop('disabled', true);
        asteriskAligned.addClass('hide');
        asteriskNotAligned.removeClass('hide');
        comboNotAligned.removeAttr('disabled');
        comboAligned.removeAttr('required');
        comboNotAligned.attr('required', 'required');

        var isOperationOpr = $('input[name=hiddenIsOperationOpr]').val();

        if ((isIncludeOpr === "False") && !(isOperationOpr === "False")) {
            regionRelevanceOperation.removeClass('hide');
            enRelevanceArea.attr("data-force-parsley-validation", "true");
            enRelevanceArea.attr("data-parsley-required", "true");
            esRelevanceArea.attr("data-force-parsley-validation", "true");
            esRelevanceArea.attr("data-parsley-required", "true");
        }
    }
});


$(document).on('click', 'span.checkbox-default-icon', function () {
    var activeTab = getTabActive();
    if (activeTab == "tabStrategic") {
        var isChecked = $(this).prev().is(':checked');
        var textarea = $(this).closest('.row').find('textarea');
        textarea.attr('data-parsley-required', !isChecked ? "true" : "false");
        textarea.attr('data-force-parsley-validation', !isChecked ? "true" : "false");
    }
});

function startEditComboState() {
    $("#justificationList .row").each(function () {
        var iconCheck = $(this).find('span.checkbox-default-icon');
        var isChecked = $(this).prev().is(':checked');
        var textarea = $(this).find('textarea');
        textarea.attr('data-parsley-required', "false");
        textarea.attr('data-force-parsley-validation', "false");
    })
}

function strategicAligmentErrorVerification() {
    justificationListErrorVerification();
    countryStrategicErrorVerification();
}

function justificationListErrorVerification() {
    $("#justificationList .row").each(function () {
        $(this).find('ul.parsley-errors-list').detach();
        var textarea = $(this).find('textarea');
        var checkbox = $(this).find('input.indicator-Check');
        if (textarea.attr('data-parsley-required') === "true" && textarea.hasClass('parsley-error')) {
            textarea.after('<ul class="parsley-errors-list filled id="parsley-id-multiple-objectives"><li class="parsley-required parsley-error">This value is required.</li></ul>');
        } else {
            textarea.next('ul.parsley-errors-list').removeClass('filled');
            textarea.removeClass('parsley-error');
            if (textarea.text().trim().length === 0) {
                $(this).children().eq(1).attr('style', "display : none");
            }
        }
    })
}

function countryStrategicErrorVerification() {
    var datacontentDiv = $('#country-strategic').find('div[data-content="aligned"]');
    var otherDiv = $('#country-strategic').find('div[data-content="notAligned"]');
    if (datacontentDiv.prev().find('span.asteriskColor').hasClass('hide')) {
        datacontentDiv = otherDiv;
        otherDiv = $('#country-strategic').find('div[data-content="aligned"]');
    }
    var selectValue = datacontentDiv.find("select").find(':selected').val();
    if (typeof selectValue === 'undefined') {
        if (datacontentDiv.find("ul[id^='parsley-id-multiple-objectives']").length == 0) {
            if (datacontentDiv.attr('data-content') === 'notAligned') {
                htmlError = '<ul class="parsley-errors-list filled" id="parsley-id-multiple-objectivesNotAligned"><li class="parsley-required">This value is required.</li></ul>'
            } else {
                htmlError = '<ul class="parsley-errors-list filled" id="parsley-id-multiple-objectives"><li class="parsley-required">This value is required.</li></ul>'
            }
            datacontentDiv.find('br').after(htmlError);
        } else {
            if (!datacontentDiv.find("#parsley-id-multiple-objectives").hasClass('filled')) {
                datacontentDiv.find("#parsley-id-multiple-objectives").addClass('filled');
            }
        }
    }
    otherDiv.find("#parsley-id-multiple-objectives").removeClass('filled');
}
function switchInputTextToLabel(tableId) {
    $('#' + tableId + ' > tbody > tr > td').find('input[type=checkbox]').change(function () {
        var inputText = $(this).closest('tr').find('span.showDataEdit input.inputText');
        var label = $(this).closest('tr').find('span.showDataEdit label.labelNormal').not(".score");
        var required = ["tableIDBInvolvement", "tableImpactEvaluation", "tableTechnicalAssistance"];

        if ($(this).is(":checked")) {
            inputText.removeClass("hide");
            inputText.removeAttr("required");
            $.map(required, function (n, i) {
                if (n == tableId) {
                    inputText.attr("required", "required");
                }
            });
            label.addClass("hide");
        } else {
            var valInputText = inputText.val();
            label.val(valInputText);
            label.removeClass("hide");
            inputText.addClass("hide");
            for (var i = 0; i < inputText.length; i++) {
                inputText.eq(i).val(label.eq(i).text());
            }
            inputText.removeAttr("required");
            inputText.removeClass("parsley-error");
            inputText.next("ul.parsley-errors-list").removeClass("filled");
        }
    });
}

$(document).ready(function () {

    $('.DemCommentBox').find('.DemAddComments').click(function () {
        $(this).parent().parent().find('.DemInsert').slideDown('slow');
    });
});

function evaluabilityLoadScore(loadScore) {
        function demEvaluabilityInitialLoad(data, childClass, parentClass) {
            var childClass;
            var parentClass;

            var childsTotal = 0;
            $(childClass).find('span[data-pagemode=edit] > label.js_containerData').each(function () {
                childsTotal = childsTotal + parseFloat($(this).text());
            });

            var lastValue = parseFloat(childsTotal).toFixed(2);
            var containerData = $(parentClass).find('.js_containerData');
            containerData.empty();
            containerData.append(lastValue);
        }

        function demEvaluabilityInitialLoadMethod(data, childClass, parentClass) {
            var data;
            var childClass;
            var parentClass;
            $(childClass).find('span[data-pagemode=edit] > label.js_containerData').each(function () {
                data.push($(this).text());
            });

            var calculateMaxMethodNumber = Math.max.apply(Math, data);
            var convertCalculateMaxMethodNumber = calculateMaxMethodNumber.toFixed(2);
            var containerData = $(parentClass).find('.js_containerData');
            containerData.empty();
            containerData.append(convertCalculateMaxMethodNumber);
        }

        function demEvaluabilityInitialLoadProposed(data, childClass, parentClass) {
            var childClass;
            var parentClass;

            var checkEvidence = $(childClass).eq(1).find('.js_buttonCheck > input[type="checkbox"]');
            var checkInformation = $(childClass).eq(2).find('.js_buttonCheck > input[type="checkbox"]');

            if (!checkEvidence.is(':checked') || !checkInformation.is(':checked')) {
                var containerData = $(childClass).eq(2).find('.js_containerData');
                containerData.empty();
                var proposedInitVal = 0.00;
                containerData.append(proposedInitVal.toFixed(2));
            }

            checkEvidence.on('change', function () {
                if (checkInformation.is(':checked')) {
                    var containerData = $(childClass).eq(2).find('.js_containerData');
                    containerData.empty();
                    $(".ProposedInterventions_Child").eq(2)
                        .find('input[type="checkbox"]')
                        .data("scoreid", 0.00);

                    var proposedInitVal = parseFloat(0.40);

                    containerData.append(proposedInitVal.toFixed(2));
                }
                if (checkInformation.is(':checked') && checkEvidence.is(':checked')) {

                    var proposedInterventions = $(".ProposedInterventions").find('.js_containerData');
                    var opTotalVal = proposedInterventions.text();

                    var numbOpTotalVal = parseFloat(opTotalVal);

                    proposedInterventions.empty();

                    proposedInterventions.append((numbOpTotalVal + 0.40)
                        .toFixed(2));
                }
                if (checkInformation.is(':checked') && !checkEvidence.is(':checked')) {

                    var proposedInterventions = $(".ProposedInterventions").find('.js_containerData');

                    var opTotalVal = proposedInterventions.text();

                    var numbOpTotalVal = parseFloat(opTotalVal);

                    proposedInterventions.empty();

                    proposedInterventions.append((numbOpTotalVal - 0.40)
                        .toFixed(2));
                }
                if (!checkEvidence.is(':checked')) {
                    $(childClass).eq(2).find('.js_containerData').empty();

                    $(".ProposedInterventions_Child").eq(2)
                        .find('input[type="checkbox"]')
                        .data("scoreid", 0.00);

                    var proposedInitVal = parseFloat(0.00);

                    $(childClass).eq(2).find('.js_containerData').append(proposedInitVal.toFixed(2));
                }
            });

            checkInformation.on('change', function () {

                var proposedInterventionsChildCheckbox = $(".ProposedInterventions_Child").eq(2).find('input[type="checkbox"]');

                if (checkEvidence.is(':checked') && checkInformation.is(':checked')) {

                    $(childClass).eq(2).find('.js_containerData').empty();

                    proposedInterventionsChildCheckbox.data("scoreid", "0.40");
                    var proposedInitVal = parseFloat(0.00);

                    $(childClass).eq(2).find('.js_containerData').append(proposedInitVal.toFixed(2));
                }
                if (!checkEvidence.is(':checked') && checkInformation.is(':checked')) {

                    $(childClass).eq(2).find('.js_containerData').empty();

                    proposedInterventionsChildCheckbox.data("scoreid", "0.00");
                    var proposedInitVal = parseFloat(0.00);

                    $(childClass).eq(2).find('.js_containerData').append(proposedInitVal.toFixed(2));
                }
            });

            var childsTotal = 0;
            $(childClass).find('span[data-pagemode=edit] > label.js_containerData').each(function () {
                childsTotal = childsTotal + parseFloat($(this).text());
            });

            var lastValue = parseFloat(childsTotal).toFixed(2);

            var containerData = $(parentClass).find('.js_containerData');
            containerData.empty();
            containerData.append(lastValue);
        }

        var verticalLogicChilds = [];
        var outcomeChilds = [];
        var outputsChilds = [];
        var proposedInterventions = [];
        var programDiagnosis = [];
        var costEffectiveness = [];
        var costBenefits = [];
        var generalEconomic = [];
        var monitoring = [];
        var general = [];
        var evaluationAspects = [];
        var methodUsed = [];

        demEvaluabilityInitialLoadProposed(proposedInterventions, '.ProposedInterventions_Child', '.ProposedInterventions');
        demEvaluabilityInitialLoad(verticalLogicChilds, '.VerticalLogic_Child', '.VerticalLogic');
        demEvaluabilityInitialLoad(outcomeChilds, '.Outcome_Child', '.Outcome');
        demEvaluabilityInitialLoad(outputsChilds, '.Outputs_Child', '.Outputs');
        demEvaluabilityInitialLoad(programDiagnosis, '.ProgramDiagnosis_Child', '.ProgramDiagnosis');
        demEvaluabilityInitialLoad(costEffectiveness, '.CostEffectiveness_Child', '.CostEffectiveness');
        demEvaluabilityInitialLoad(generalEconomic, '.GeneralEconomic_Child', '.GeneralEconomic');
        demEvaluabilityInitialLoad(costBenefits, '.CostBenefits_Child', '.CostBenefits');
        demEvaluabilityInitialLoad(monitoring, '.Monitoring_Child', '.Monitoring');
        demEvaluabilityInitialLoad(general, '.General_Child', '.General');
        demEvaluabilityInitialLoadMethod(methodUsed, '.MethodUsed_Child', '.MethodUsed');
        demEvaluabilityInitialLoad(evaluationAspects, '.EvaluationAspects_Child', '.EvaluationAspects');


        function demEvaluabilityClickAction(childClass, parentClass, demTableArea) {
            var childClass;
            var parentClass;
            var demTableArea;

            $(childClass).find('.js_buttonCheck > input[type="checkbox"]').on('change', function () {
                if ($(this).is(':checked')) {
                    dataID = $(this).first('input[type="checkbox"]').data("scoreid");
                    $(this).parent().parent().parent().parent().find('.js_containerData').empty().append(dataID);
                    demEvaluabilityAdd(dataID, parentClass);
                    reloadLevels(demTableArea);
                } else {
                    dataRes = $(this).parent().parent().parent().parent().find('.js_containerData').first().text();
                    $(this).parent().parent().parent().parent().find('.js_containerData').empty().append('0.00');
                    demEvaluabilityRemove(dataRes, parentClass);
                    reloadLevels(demTableArea);
                }
            });
        }

        function demEvaluabilityMethodClickAction(childClass, parentClass, demTableArea) {
            var childClass;
            var parentClass;
            var demTableArea;

            $(childClass).find('.js_buttonCheck > input[type="checkbox"]').on('change', function () {
                if ($(this).is(':checked')) {
                    var dataId = $(this).first('input[type="checkbox"]').data("scoreid");
                    $(this).parent().parent().parent().parent().find('.js_containerData').empty().append(dataId);
                    demEvaluabilityLoadMethod(childClass, parentClass);
                    reloadLevels(demTableArea);
                } else {
                    var dataRes = $(this).parent().parent().parent().parent().find('.js_containerData').first().text();
                    $(this).parent().parent().parent().parent().find('.js_containerData').empty().append('0.00');
                    demEvaluabilityLoadMethod(childClass, parentClass);
                    reloadLevels(demTableArea);
                }
            });
        }

        function demEvaluabilityLoadMethod(childClass, parentClass) {
            var dataM = [];
            var childClass;
            var parentClass;

            $(childClass).find('.js_containerData').each(function () {
                dataM.push($(this).text());
            });

            var calculateMaxMethodNumberN = Math.max.apply(Math, dataM);
            var convertCalculateMaxMethodNumberN = calculateMaxMethodNumberN.toFixed(2);

            var finMethod = $('.MethodUsed').find('.js_containerData').empty();
            $('.MethodUsed').find('.js_containerData').append(convertCalculateMaxMethodNumberN).fadeIn(1000);
            dataM.length = 0;
        }

        function demEvaluabilityAdd(valorO, contexto) {
            valorO = parseFloat(valorO);
            var tomar = $(contexto).find('.js_containerData').first().text();
            tomar = parseFloat(tomar);
            var sumando = (tomar + valorO).toFixed(2);
            $(contexto).find('.js_containerData').empty().append(sumando).hide().fadeIn(1000);
        }

        function demEvaluabilityRemove(valorO, contexto) {
            valorO = parseFloat(valorO);
            var tomar = $(contexto).find('.js_containerData').first().text();
            tomar = parseFloat(tomar);
            var restando = (tomar - valorO).toFixed(2);
            $(contexto).find('.js_containerData').empty().append(restando).hide().fadeIn(1000);
        }

        var outcomeFind = $('.Outcome').find('.js_containerData').first().text();
        outcomeFind = parseFloat(outcomeFind);
        var outputsFind = $('.Outputs').find('.js_containerData').first().text();
        outputsFind = parseFloat(outputsFind);
        var verticalLogicFind = $('.VerticalLogic').find('.js_containerData').first().text();
        verticalLogicFind = parseFloat(verticalLogicFind);
        var valuesSecondLevelA = (outcomeFind + outputsFind + verticalLogicFind).toFixed(2);
        $('.ResultMatrixQuality').find('.js_containerData').empty().append(valuesSecondLevelA).hide().fadeIn(1000);
        var methodUsedFind = $('.MethodUsed').find('.js_containerData').first().text();
        methodUsedFind = parseFloat(methodUsedFind);
        var evaluationAspectsFind = $('.EvaluationAspects').find('.js_containerData').first().text();
        evaluationAspectsFind = parseFloat(evaluationAspectsFind);
        var valuesSecondLevelB = (methodUsedFind + evaluationAspectsFind).toFixed(2);

        $('.MethodologyToMeasure')
            .find('.js_containerData')
            .empty()
            .append(valuesSecondLevelB)
            .hide()
            .fadeIn(1000);

        var generalFind = $('.General').find('.js_containerData').first().text();
        generalFind = parseFloat(generalFind);
        var methodologyFind = $('.MethodologyToMeasure').find('.js_containerData').first().text();
        methodologyFind = parseFloat(methodologyFind);
        var valuesSecondLevelC = (generalFind + methodologyFind).toFixed(2);
        $('.Evaluation').find('.js_containerData').empty().append(valuesSecondLevelC).hide().fadeIn(1000);
        var initFirstLevelA = $('.ProgramDiagnosis').find('.js_containerData').first().text();
        initFirstLevelA = parseFloat(initFirstLevelA);

        var initFirstLevelB = $('.ProposedInterventions')
            .find('.js_containerData')
            .first()
            .text();

        initFirstLevelB = parseFloat(initFirstLevelB);
        var initFirstLevelC = $('.ResultMatrixQuality')
            .find('.js_containerData')
            .first()
            .text();

        initFirstLevelC = parseFloat(initFirstLevelC);
        var initFirstLevelD = $('.CostBenefits')
            .find('.js_containerData')
            .first()
            .text();

        initFirstLevelD = parseFloat(initFirstLevelD);
        var initFirstLevelE = $('.CostEffectiveness').find('.js_containerData').first().text();
        initFirstLevelE = parseFloat(initFirstLevelE);
        var initFirstLevelF = $('.GeneralEconomic').find('.js_containerData').first().text();
        initFirstLevelF = parseFloat(initFirstLevelF);
        var initFirstLevelG = $('.Monitoring').find('.js_containerData').first().text();
        initFirstLevelG = parseFloat(initFirstLevelG);
        var initFirstLevelH = $('.Evaluation').find('.js_containerData').first().text();
        initFirstLevelH = parseFloat(initFirstLevelH);

        var programLogicArea = (initFirstLevelA + initFirstLevelB + initFirstLevelC).toFixed(2);
        $('.ProgramLogic').find('.js_containerData').empty().append(programLogicArea).hide().fadeIn(1000);

        var economicAnalysisGroup = [];
        economicAnalysisGroup.push(initFirstLevelD.toFixed(2));
        economicAnalysisGroup.push(initFirstLevelE.toFixed(2));
        economicAnalysisGroup.push(initFirstLevelF.toFixed(2));
        var calculateMaxnumber = Math.max.apply(Math, economicAnalysisGroup);
        $('.EconomicAnalysis').find('.js_containerData').empty().append(calculateMaxnumber.toFixed(2)).hide().fadeIn(1000);

        var monitoringEvaluationArea = (initFirstLevelG + initFirstLevelH).toFixed(2);

        $('.MonitoringEvaluation')
            .find('.js_containerData')
            .empty()
            .append(monitoringEvaluationArea)
            .hide()
            .fadeIn(1000);

        function reloadLevels(demTableArea) {
            var demTableArea;

            var outcomeFind = $('.Outcome').find('.js_containerData').first().text();
            outcomeFind = parseFloat(outcomeFind);
            var outputsFind = $('.Outputs').find('.js_containerData').first().text();
            outputsFind = parseFloat(outputsFind);

            var verticalLogicFind = $('.VerticalLogic').find('.js_containerData').first().text();

            verticalLogicFind = parseFloat(verticalLogicFind);

            valuesSecondLevelA = (outcomeFind + outputsFind + verticalLogicFind).toFixed(2);

            $('.ResultMatrixQuality').find('.js_containerData')
                .empty()
                .append(valuesSecondLevelA)
                .hide().fadeIn(1000);

            var methodUsedFind = $('.MethodUsed').find('.js_containerData').first().text();

            methodUsedFind = parseFloat(methodUsedFind);

            var evaluationAspectsFind = $('.EvaluationAspects')
                .find('.js_containerData')
                .first()
                .text();
            evaluationAspectsFind = parseFloat(evaluationAspectsFind);

            valuesSecondLevelB = (methodUsedFind + evaluationAspectsFind).toFixed(2);

            $('.MethodologyToMeasure').find('.js_containerData')
                .empty()
                .append(valuesSecondLevelB)
                .hide()
                .fadeIn(1000);

            var generalFind = $('.General').find('.js_containerData').first().text();
            generalFind = parseFloat(generalFind);

            var methodologyFind = $('.MethodologyToMeasure')
                .find('.js_containerData')
                .first()
                .text();
            methodologyFind = parseFloat(methodologyFind);

            valuesSecondLevelC = (generalFind + methodologyFind).toFixed(2);
            $('.Evaluation').find('.js_containerData')
                .empty()
                .append(valuesSecondLevelC)
                .hide()
                .fadeIn(1000);

            var initFirstLevel_A = $('.ProgramDiagnosis')
                .find('.showDataEdit .js_containerData')
                .first()
                .text();

            initFirstLevel_A = parseFloat(initFirstLevel_A);

            var initFirstLevel_B = $('.ProposedInterventions')
                .find('.showDataEdit .js_containerData')
                .first()
                .text();
            initFirstLevel_B = parseFloat(initFirstLevel_B);
            var initFirstLevel_C = $('.ResultMatrixQuality')
                .find('.showDataEdit .js_containerData')
                .first()
                .text();
            initFirstLevel_C = parseFloat(initFirstLevel_C);
            var initFirstLevel_D = $('.CostBenefits')
                .find('.js_containerData')
                .first()
                .text();

            initFirstLevel_D = parseFloat(initFirstLevel_D);

            var initFirstLevel_E = $('.CostEffectiveness')
                .find('.js_containerData')
                .first()
                .text();

            initFirstLevel_E = parseFloat(initFirstLevel_E);
            var initFirstLevel_F = $('.GeneralEconomic')
                .find('.js_containerData')
                .first()
                .text();

            initFirstLevel_F = parseFloat(initFirstLevel_F);
            var initFirstLevel_G = $('.Monitoring')
                .find('.js_containerData')
                .first()
                .text();

            initFirstLevel_G = parseFloat(initFirstLevel_G);
            var initFirstLevel_H = $('.Evaluation')
                .find('.js_containerData')
                .first()
                .text();

            initFirstLevel_H = parseFloat(initFirstLevel_H);

            if (demTableArea == 'programLogicArea') {
                programLogicArea = (initFirstLevel_A + initFirstLevel_B + initFirstLevel_C).toFixed(2);
                $('.ProgramLogic').find('.js_containerData').empty().append(programLogicArea).hide().fadeIn(1000);
                demTableArea = "";
            }

            if (demTableArea == 'economicAnalysisArea') {
                economicAnalysisGroup = [];
                economicAnalysisGroup.push(initFirstLevel_D.toFixed(2));
                economicAnalysisGroup.push(initFirstLevel_E.toFixed(2));
                economicAnalysisGroup.push(initFirstLevel_F.toFixed(2));
                calculateMaxnumber = Math.max.apply(Math, economicAnalysisGroup);;
                $('.EconomicAnalysis').find('.js_containerData').empty().append(calculateMaxnumber.toFixed(2)).hide().fadeIn(1000);
                demTableArea = "";
            }

            if (demTableArea == 'monitoringEvaluationArea') {
                monitoringEvaluationArea = (initFirstLevel_G + initFirstLevel_H).toFixed(2);
                $('.MonitoringEvaluation').find('.js_containerData').empty().append(monitoringEvaluationArea).hide().fadeIn(1000);
                demTableArea = "";
            }
        }

        if (loadScore) {
            demEvaluabilityClickAction('.Outcome_Child', '.Outcome', 'programLogicArea');
            demEvaluabilityClickAction('.Outputs_Child', '.Outputs', 'programLogicArea');
            demEvaluabilityClickAction('.VerticalLogic_Child', '.VerticalLogic', 'programLogicArea');
            demEvaluabilityClickAction('.ProposedInterventions_Child', '.ProposedInterventions', 'programLogicArea');
            demEvaluabilityClickAction('.ProgramDiagnosis_Child', '.ProgramDiagnosis', 'programLogicArea');
            demEvaluabilityClickAction('.CostEffectiveness_Child', '.CostEffectiveness', 'economicAnalysisArea');
            demEvaluabilityClickAction('.GeneralEconomic_Child', '.GeneralEconomic', 'economicAnalysisArea');
            demEvaluabilityClickAction('.CostBenefits_Child', '.CostBenefits', 'economicAnalysisArea');
            demEvaluabilityClickAction('.Monitoring_Child', '.Monitoring', 'monitoringEvaluationArea');
            demEvaluabilityClickAction('.General_Child', '.General', 'monitoringEvaluationArea');
            demEvaluabilityMethodClickAction('.MethodUsed_Child', '.MethodUsed', 'monitoringEvaluationArea');
            demEvaluabilityClickAction('.EvaluationAspects_Child', '.EvaluationAspects', 'monitoringEvaluationArea');
        }
}

function loadingFunctions(loadScore) {
    switchInputTextToLabelForRiskTab();
    switchInputTextToLabelForAdditionalityTab();
    switchInputTextToLabelForEvaluabilityTab();
    setCheckValueOnHeaders(".ChildClass");
    setCheckValueOnHeaders(".DemTableHeaderLevel4");
    setCheckValueOnHeaders(".DemTableHeaderLevel3");
    setCheckValueOnHeaders(".DemTableHeaderLevel2");
    loadRequiredDEM();
    requiredDEM();
    verifyRequiredDEM();
    bindHandlers();
    setRemoveCustomRowOdd();
    initialState();
    evaluabilityLoadScore(loadScore);
    disableEnableTabs(false);

    blueArrow();

    funcHideAddCommentCountry();
    funcHideAddCommentStrategic();
    funcHideAddCommentSectionCountryStrategy();
    funcHideAddCommentSectionCountryProgram();
    treeFilterTablesDEM();
    initialRegionRelevance();
    diplayShowComment();
    alignTrashCan();
}

function loadCurrentTabs() {
    var tabContainerActive = $('#containerTabs').children('.active').attr('id');
    var tabPaneActive = $('#' + tabContainerActive + ' div.active').attr('id');
    var divSave = $('#' + tabPaneActive);
    divSave.find('div').attr('data-url', divSave.attr('data-base-url'));
    divSave.load(location.href + " #" + tabPaneActive + " > *");
    tabs($('[dd-tab=#' + tabPaneActive + ']'));
}

function setCurrentTabAfterCancel(tabName) {
    $('ul.tabs').find('li[dd-tab=#tabSummary]').removeClass("active");

    window.setTimeout(function () {
        $('ul.tabs').find('li[dd-tab=' + tabName + ']').addClass("active");
        $('ul.tabs').find('li[dd-tab=' + tabName + ']').click();
    }, 1);
}

function addNewRow(table, newRow) {
    table.before(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&/g, '&')
        .replace("\r\n", "")
        .replace("\n", ""));
}

function addNewRow(table, newRow) {
    table.before(newRow.replace(/&lt;/g, '<').replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&/g, '&')
        .replace("\r\n", "")
        .replace("\n", ""));
}

function setActiveTab() {
    $('ul.tabs:first li').click(function () {
        activeTab = $('ul.tabs:first li.active').attr('dd-tab');
        localStorage.setItem("activeTab", activeTab);
    });
}

function setRemoveCustomRowOdd() {
    $('tr').removeClass('customRowOdd');
}

function setCheckValueOnHeaders(rowClass) {
    $(' ' + rowClass + ' ').find('span[data-pagemode=edit]').find('[type=checkbox]').change(function () {
        var parentId = $(this).closest('tr').attr('data-levelparent-id');

        var parentRowCheckbox = $('[data-id =' + parentId + ']')
            .find('span[data-pagemode=edit]')
            .find('[type=checkbox]');

        if ($(this).is(':checked')) {
            parentRowCheckbox.prop('checked', 'checked').trigger('change');
        } else {
            var anySiblingsIsChecked = $('[data-levelparent-id=' + parentId + ']')
                .find('td span[data-pagemode=edit ]')
                .find('[type=checkbox]')
                .filter(':checked').length;

            if (!anySiblingsIsChecked) {
                parentRowCheckbox.prop('checked', false).trigger('change');
            }
        }
    });
}

function switchInputTextToLabelForRiskTab() {
    switchInputTextToLabel("tableRiskMatrix");
    switchInputTextToLabel("tableMitigationMeasures");
}

function switchInputTextToLabelForAdditionalityTab() {
    switchInputTextToLabel("tableCountrySystems");
    switchInputTextToLabel("tableIDBInvolvement");
    switchInputTextToLabel("tableTechnicalAssistance");
    switchInputTextToLabel("tableImpactEvaluation");
}

function switchInputTextToLabelForEvaluabilityTab() {
    switchInputTextToLabel("tableEvaluability");
    switchInputTextToLabel("tableEconomicAnalysis");
    switchInputTextToLabel("tableMonitoringAndEvaluation");
}

function gotoStrategicAlignment() {
    var url = "~/OPUS/View/OperationData?OperationNumber=";
    window.open(url, '_blank');
}


function loadRequiredDEM() {
    var tabContent = $(".tab-content").find('.active');
    var tabPaneActive = $(".tab-pane.active").attr("id");
    if (tabPaneActive === "tabSummary") {
        var required = tabContent.find('.requiredDEM [name="txtRequiredDEM"]')
            .is(':checked');

        if (required) {
            tabContent.find('.requiredArea').hide();
            tabContent.find('.lblRequiredDEM').hide();
            tabContent.find('.requiredArea [name="textRequired"]')
                .removeAttr("data-parsley-required");
        }
        else {
            tabContent.find('.requiredArea').show();
            tabContent.find('.lblRequiredDEM').show();
            tabContent.find('.requiredArea [name="textRequired"]')
                .attr("data-parsley-required", "True");
        }
    }
}

function verifyRequiredDEM() {
    var tabPaneActive = $(".tab-pane.active").attr("id");
    var tabContentActive = $(".tab-content").find('.active');

    if (tabPaneActive === "tabSummaryContent" || tabPaneActive === "tabSummary") {

        var required = tabContentActive.find('.requiredDEM [name="txtRequiredDEM"]')
            .is(':checked');

        if (required) {
            tabContentActive.find('.requiredArea').hide();
            tabContentActive.find('.requiredArea [name="textRequired"]')
                .removeAttr("data-parsley-required");
        }
        else {
            tabContentActive.find('.requiredArea').show();

            tabContentActive.find('.requiredArea [name="textRequired"]')
                .attr("data-parsley-required", "True");
        }
    }
}

function deleteCommentRow(source) {
    source.siblings().parent().remove();

    if (source.attr("data-commentId") && source.attr("data-commentId") != "'0'") {
        $("[name=commentDeleteId]").val($("[name=commentDeleteId]").val() + source.attr("data-commentId") + "|");
    }
}

function deleteTableCommentRow(source) {
    source.parents().closest('span.showDataEdit').find('textarea').val("");
}

function deleteTableCommentRiskRow(source) {
    var criterionId = source.attr('data-buttonCriterion');
    var commentId = source.attr("data-commentId");
    $('[data-criterionId =' + criterionId + ']').removeClass('hide');

    if (commentId && commentId != "'0'") {
        source.parents().closest('span.showDataEdit').find('textarea').val("");
        $("[name=commentDeleteId]").val($("[name=commentDeleteId]").val().toString() + commentId + "|");
    }

    if (commentId && commentId == "'0'") {
        source.parents().closest('span.showDataEdit').remove();
    }
}

function closeTextModal(source) {
    var sourceTextArea = source.parents('div#showBiggerText').find('.inputTextarea');
    var destinationTextArea = $('div.textAreaBody').find('textarea#' + sourceTextArea.attr('id'));
    destinationTextArea.each(function () {
        $(this).val(sourceTextArea.val().trim());
    });
    closeModal();
}

function closeModal() {
    var vexModal = $('.modalBody').parents('.vex');
    if (vexModal.length > 0)
        vex.closeByID(vexModal.data().vex.id);
}

function initialCheckState() {
    var alignedCheckBox = $('input[value="aligned"]');
    var notAlignedCheckBox = $('input[value="not-aligned"]');
    var alignedCombo = $('[name="objectives"]');
    var notAlignedCombo = $('[name="objectivesNotAligned"]');
    var asteriskAligned = $('span[data-name="asteriskAligned"]');
    var asteriskNotAligned = $('span[data-name="asteriskNotAligned"]');

    if (alignedCheckBox.is(':checked')) {
        alignedCombo.removeAttr('disabled');
        notAlignedCombo.prop('disabled', true);
        asteriskNotAligned.addClass('hide');
        asteriskAligned.removeClass('hide');
    } else {
        notAlignedCombo.removeAttr('disabled');
        alignedCombo.prop('disabled', true);
        asteriskAligned.addClass('hide');
        asteriskNotAligned.removeClass('hide');
    }
}

function initialState() {
    $('[name=indicator-Check], [name=indicator-Check-dis]').each(function () {
        var source = $(this);
        showOwnData(source);
    });
}

function showOwnDataClick(source) {
    source = $(source);
    var sectionName = source.attr('data-target-section');
    var regionSelector = String.format('[data-section="{0}"]', sectionName);
    var region = $(regionSelector);
    var textarea = region.find('textarea');
    var isValid = region.parent().find('input').val() == 'True';
    if (source.is(':checked')) {
        region.show('blind', 500);
        if (isValid) {
            textarea.removeAttr('disabled');
        }
    } else {
        region.hide('blind', 500);
        window.setTimeout(function () {
            textarea.prop('disabled', true);
        }, 500);
    }
}

function showOwnData(source) {
    source = $(source);
    var sectionName = source.attr('data-target-section');
    var regionSelector = String.format('[data-section="{0}"]', sectionName);
    var region = $(regionSelector);
    var textarea = region.find('textarea');
    var isValid = region.parent().find('input').val() == 'True';
    if (source.is(':checked')) {
        region.show('blind', 500);
        if (isValid) {
            if (textarea.length != 0) {
                textarea.removeAttr('disabled');
            }
        }
    } else {
        region.hide('blind', 500);
        textarea.prop('disabled', true);
    }
}

var resultSubnational = [];
var newResultSubnational = [];
var deleteSubnationalItems = [];

$(document).on("click", 'input[name="subnational"]', function () {
    if ($(this).is(':checked')) {
        $('a#openSubnationalCreation').show();
    } else {
        $('a#openSubnationalCreation').hide();
    }
});

function subnationalLevelSearch() {
    $("#listSubnational").empty();

    var valorInput = $("[name=subnationalSearch_text]").val();
    var url = "https://dev.virtualearth.net/REST/v1/Locations?q=";
    url = url + valorInput + "&key=" + BingMapsKey;
    url += '&jsonp=?';
    $.getJSON(url, function (response) {
        getListSubnationalLevel(response);
    });
    $("#subnationalDropdown").addClass("dropdown open");
}

function getListSubnationalLevel(res) {
    var name = "";
    var resultados = res.resourceSets[0];
    for (var i = 0; i < resultados.resources.length; i++) {
        var item = resultados.resources[i];

        if (item.address.locality == null && item.address.adminDistrict == null) {
            name = item.address.countryRegion;

        } else if (item.address.locality == null) {
            name = item.address.adminDistrict + ", " + item.address.countryRegion;

        } else {
            name = item.address.locality + ", " + item.address.adminDistrict + ", " + item.address.countryRegion;
        }

        $("#listSubnational").append("<li>" + "<a name='itemSubnational'>" + name + "</a></li>");

        resultSubnational.push({
            value: name,
            countryRegion: item.address.countryRegion,
            adminDistrict: item.address.adminDistrict,
            locality: item.address.locality
        });
    }
}

$(document).on("click", "[name=deleteSubnationalItems]", function () {
    var countryRegion = $(this).closest('tr').find("#countryRegion label").text();
    var adminDistrict = $(this).closest('tr').find("#adminDistrict label").text();
    var locality = $(this).closest('tr').find("#locality label").text();

    $(this).closest('tr').remove();

    deleteSubnationalItems.push({
        countryRegion: countryRegion,
        adminDistrict: adminDistrict,
        locality: locality
    });
});

$(document).on("click", "div.vex-close", function () {
    deleteSubnationalItems = [];
    resultSubnational = [];
});

function disableEnableTabs(enableOne) {

    if (enableOne) {
        var idTab = $('.tab-pane.active').attr('data-tab-control-id');
        $("[dd-tab]").not('[dd-tab=' + idTab + ']').addClass('disabled');
    } else {
        $("[dd-tab]").removeClass('disabled');
    }
}


function loadTabs() {
    $('#tabSummaryContent .tabs, #tabSummaryContent .tabs li, #tabChecklist  .tabs, #tabChecklist .tabs li').css('border', 'none');
    tabs($('[dd-tab=#tabSummary]'));
    tabs($('[dd-tab=#tabStrategic]'));
}

function loadTabsAfterSave() {
    loadTabs();
    $('[dd-tab="#tabSummaryContent"]').click();
}

function blueArrow() {

    $('tr').find('.DemListCommentsIconTdContent')
        .parents('tr')
        .prev('tr')
        .find('.caret')
        .css('color', '#0066CC');
}

$("div[data-idb-fieldname='btnCollapse']").attr('onclick', 'collapsaAllCommentRow(), colapsableAll(this)');

$(".tree .icon").attr('onclick', 'collapsaAllCommentRow(), CollapseTable(this)');

function collapsaAllCommentRow() {
    $('.buttonShowRow.active').each(function () {
        CollapseRowTable(this);
    });
}

function deleteCommentIndicatorCountry(source) {
    if (source.attr("data-commentId")) {
        $("[name=deleteCommentsIndicatorCountry]").val($("[name=deleteCommentsIndicatorCountry]").val() + source.attr("data-commentId") + "|");
    }
    var nameColapsable = '#' + source.attr("data-colapsableDem");

    $(nameColapsable)
        .siblings('.showNewCommentButtonAddComment')
        .find('.iconAddNew')
        .removeAttr('disabled', 'disabled');

    source.parents("span[data-pagemode=edit]").remove();
    $(nameColapsable).find('.sectionUpdateComment').remove();

    $('input[name=hiddenCountDeleteComments]').val("1");

    var addCommentIndicatorSection = '#' + source.attr("data-showNewCommentButtonAddComment").replace("\'", '').replace("\'", '');
    $(addCommentIndicatorSection).find('.iconAddNew').show();
}

function funcHideAddCommentCountry() {
    $('.hiddenAddCommentCountry').each(function () {
        var valor = $(this).find('input[name=hiddenOneComment]').val();
        if (valor == "1") {
            $(this).find('.iconAddNew').hide();
        }
    });
}

$(document).on('click', '[name=impactIndicatorCheck], [name=outcomeIndicatorCheck], [name=outputIndicatorCheck]', function (e) {
    var source = $(this);
    var nameHiddenCountIndicatorsRM = "countIndicatorsRM_Indicator" + source.attr('data-id');
    var auxiliarString = 'input[name={0}]';
    var objectHidden = auxiliarString.replace('{0}', nameHiddenCountIndicatorsRM);

    var countUnLink = parseInt($('input[name=hiddenCountIndicatorsRMCountryCurrent]').attr('value'));
    var countCurrent = parseInt($(objectHidden).attr('value'));
    if ($(this).is(':checked')) {
        countCurrent++;
        countUnLink++;
    } else {
        countCurrent--;
        countUnLink--;
    }

    $('input[name=hiddenCountIndicatorsRMCountryCurrent]').val(countUnLink);
    $(objectHidden).val(countCurrent);

    if ($(objectHidden).attr('value') > 0) {
        $(this).parents('.classViewModeCountry')
            .first()
            .find('input[name=indicator-Check-Country]')
            .prop('checked', true);
    } else {
        $(this).parents('.classViewModeCountry')
            .first()
            .find('input[name=indicator-Check-Country]')
            .prop('checked', false);
    }
});

function SaveCountryDevlopmentResults(tabPaneActive) {
    var modelSerialized = '#hidCountryDataViewModel';
    var response = saveContainer($('#' + tabPaneActive), modelSerialized, true, $('#headerButtons'));
    if (response !== false) {
        response.done(function (result) {
            if (!result.Data.IsValid && result.Data.ErrorMessage != null && result.Data.ErrorMessage !== '') {
                errorBar(result.Data.ErrorMessage, 3, true);
                return false;
            } else {
                $("#headerInfoDem").html(result.Data.partialHeader);
                $("#ValidationProcess").html(result.Data.partialValidation);
            }
        });
    }

    if (response !== false) {
        cancelDem();
    }
}

function funcHideAddCommentStrategic() {
    $('.hiddenAddCommentStrategic').each(function () {
        var valor = $(this).find('input[name=hiddenOneCommentStrategic]').val();
        if (valor == "1") {
            $(this).find('.iconAddNew').hide();
        }
    });
}

function deleteCommentIndicator(source) {
    if (source.attr("data-commentId")) {
        $("[name=deleteCommentsIndicator]").val($("[name=deleteCommentsIndicator]").val() +
            source.attr("data-commentId") + "|");
    }
    var nameColapsable = '#' + source.attr("data-colapsableDem");

    $(nameColapsable).siblings('.sectionAddLinkCommentStrategic')
        .find('.iconAddNew').
        removeAttr('disabled', 'disabled');

    source.parents("span[data-pagemode=edit]").remove();
    $(nameColapsable).find('.sectionUpdateComment').remove();

    var addCommentIndicatorSection = '#' + source.attr("data-sectionAddLinkCommentStrategic").replace("\'", '').replace("\'", '');

    $(addCommentIndicatorSection).find('.iconAddNew').show();
}

function funcHideAddCommentSectionCountryStrategy() {
    $('.hiddenAddCommentSectionCountryStrategy').each(function () {
        var valor = $(this).find('input[name=hiddenOneSectionCountryStrategy]').val();
        if (valor == "1") {
            $(this).find('.iconAddNew').hide();
        }
    });
}

function deleteCommentSectionCountryStrategy(source) {
    if (source.attr("data-commentId")) {
        $("[name=deleteCommentsSectionCountryStrategy]").val($("[name=deleteCommentsSectionCountryStrategy]").val() + source.attr("data-commentId") + "|");
    }
    var nameColapsable = '#colapseSectionCountryStrategy';

    $(nameColapsable)
        .siblings('.sectionAddLinkCommentCountryStrategy')
        .find('.iconAddNew')
        .removeAttr('disabled', 'disabled');

    source.parents("span[data-pagemode=edit]").remove();

    $('#sectionAddLinkCommentCountryStrategy').find('.iconAddNew').show();
}


function hideEditButtonOnValidationProcessStatus() {
    $('#tabChecklist .tabs li').click(function () {
        var editDem = $('[name=editDem]');
        if ($('#tabChecklist .tabs li.active').attr("dd-tab") === "#tabValidationProcessStatus") {
            editDem.addClass("hide");
        } else {
            editDem.removeClass("hide");
        }
    });
}



function showEditButtonOnValidationProcessStatus() {
    var editDem = $('[name=editDem]');
    var submitDem = $('[name=submitDem]');

    $('ul.tabs:first li').click(function () {
        activeTab = $('ul.tabs:first li.active').attr('dd-tab');

        switch (activeTab) {
            case "#tabSummaryContent":
                if ($("#tabSummaryContent li.active").attr("dd-tab") === "#tabSummary") {
                    submitDem.removeClass("hide");
                }
                editDem.removeClass("hide");

                break;
            case "#tabChecklist":
                submitDem.addClass("hide");
                if ($('#tabChecklist .tabs li.active').attr("dd-tab") === "#tabValidationProcessStatus") {
                    editDem.addClass("hide");
                } else {
                    editDem.removeClass("hide");
                }
                break;
        }
    });
}




function hideSubmitButtonOnSummaryOrResumen() {
    var submitDem = $('[name=submitDem]');

    $('#tabSummaryContent .tabs li').click(function () {
        var activeTab = $("#tabSummaryContent li.active").attr('dd-tab');
        if (activeTab === "#tabResumen") {
            submitDem.addClass("hide");
        } else {
            submitDem.removeClass("hide");
        }
    });
}

function funcHideAddCommentSectionCountryProgram() {
    $('.hiddenAddCommentSectionCountryProgram').each(function () {
        var valor = $(this).find('input[name=hiddenOneSectionCountryProgram]').val();
        if (valor == "1") {
            $(this).find('.iconAddNew').hide();
        }
    });
}

function deleteCommentSectionCountryProgram(source) {
    if (source.attr("data-commentId")) {
        $("[name=deleteCommentsSectionCountryProgram]").val($("[name=deleteCommentsSectionCountryProgram]").val() + source.attr("data-commentId") + "|");
    }
    var nameColapsable = '#colapseSectionCountryProgram';
    $(nameColapsable)
        .siblings('.sectionAddLinkCommentCountryProgram')
        .find('.iconAddNew')
        .removeAttr('disabled', 'disabled');

    source.parents("span[data-pagemode=edit]").remove();

    $('#sectionAddLinkCommentCountryProgram').find('.iconAddNew').show();
}


function setTableRowsProperColours() {
    $("#tableRiskMatrix tbody tr.DemInputRow:odd").css("background-color", "#e8e8e8");
    $("#tableMitigationMeasures tbody tr.DemInputRow:odd").css("background-color", "#e8e8e8");

    $('div#tabAdditionality table').each(function () {
        $(this).find('tbody tr.rowWithGrey:even')
            .not(".DemTableHeaderLevel1")
            .not("DemTableHeaderLevel2")
            .not(".DemTableHeaderLevel3")
            .css("background-color", "#e8e8e8");
    });
    var childRowsAdditionality = $('div#tabAdditionality table tr.rowWithGrey')
        .not(".DemTableHeaderLevel1")
        .not(".DemTableHeaderLevel2")
        .not(".DemTableHeaderLevel3")
        .not(".DemTableHeaderLevel4").filter(function () {
            if ($(this).find('td.ChildClass')) {
                return $(this);
            }
        });

    childRowsAdditionality.each(function (index) {
        if (index % 2 == 0) {
            $(this).css("background-color", "#ffffff");
        } else {
            $(this).css("background-color", "#e8e8e8");
        }
    });

    var childRows = $('div#tabEvaluability table tr.rowWithGrey')
        .not(".DemTableHeaderLevel1")
        .not(".DemTableHeaderLevel2")
        .not(".DemTableHeaderLevel3")
        .not(".DemTableHeaderLevel4").filter(function () {
            if ($(this).find('td.ChildClass')) {
                return $(this);
            }
        });

    childRows.each(function (index) {
        if (index % 2 == 0) {
            $(this).css("background-color", "#ffffff");
        } else {
            $(this).css("background-color", "#e8e8e8");
        }
    });
}

function setBiggerInfoCommentBox() {
    $('.is-relative').click(function () {
        $(this).children().find(".informationBox").addClass("biggerInformationBox");
    });
}

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

function hideComments(isCommentVisible) {
    if (!isCommentVisible) {
        $('[id*="StrategicAlignmentCommentsIndicator-"]').hide();
        $('#StrategicAlignmentCommentsCountryStrategy').hide();
        $('[id*="CountryDevelopmentResultsCommentsId-"]').hide();
        $(".buttonShowRow").css("visibility", "hidden");
        $('#RiskManagementOverallRiskRateCommentsId').hide();
        $('#RiskManagementEnvironmentalAndSocialCategoryCommentsId').hide();
    }
}

function treeFilterTablesDEM() {
    var marginSmall = "40px";
    var marginMedium = "80px";
    var marginLarge = "120px";
    var backColorGray = "#CCCCCC";
    var backColorWhite = "#FFFFFF";
    var backColorGrayLight = "#f6f7f9";
    var rowEvaluabilityTr = $("#tableEvaluability tbody tr");
    var rowEconomicTr = $("#tableEconomicAnalysis tbody tr");
    var rowMonitoringTr = $("#tableMonitoringAndEvaluation tbody tr");
    var rowCountryTr = $("#tableCountrySystems tbody tr");
    var rowEvaluability = $("#tableEvaluability tbody");
    var rowEconomic = $("#tableEconomicAnalysis tbody");
    var rowMonitoring = $("#tableMonitoringAndEvaluation tbody");
    var rowCountry = $("#tableCountrySystems tbody");
    var rowIDBInvolvement = $("#tableIDBInvolvement tbody");

    jQuery.fn.extend({
        treeFilter: function (row, child, marginElement) {
            var rowElementNumber = this.find("tr")[row];
            var rowElement = $(rowElementNumber).find("td:eq(" + child + ") .labelNormal").css("margin-left", marginElement);
            return rowElement;
        },
        deleteFilter: function (row) {
            var rowENumber = this.find("tr")[row];
            $(rowENumber).hide();
        },
        treeFilterBackColor: function (row, backColorElement) {
            var rowNumber = this.find("tr")[row];
            var changeBackColor = $(rowNumber).find(".tree")[0];
            $(changeBackColor).css("background-color", backColorElement);

            $(changeBackColor).siblings("td").css("background-color", backColorElement);
            if (!$(changeBackColor).attr("style", "background-color") == "#FFFFFF") {
                $(changeBackColor).css("background-color", "#FFFFFF")
            }
        }
    });

    function orderingRows(tableTr, table, col) {
        var parentId;
        var parentId2 = "";
        var parentId3 = "";
        tableTr.each(function (i, e) {
            if (typeof $(this).attr("data-levelparent-id") !== "undefined") {

                if ($(this).attr("data-levelparent-id") == "") {
                    parentId = $(this).attr("data-id");
                } else {

                    if (parentId != $(this).attr("data-levelparent-id")) {

                        if (parentId3 != $(this).attr("data-levelparent-id")) {

                            if (parentId2 != $(this).attr("data-levelparent-id")) {
                                parentId2 = $(this).attr("data-id");
                                table.treeFilter(i, col, marginSmall);
                            } else {
                                table.treeFilter(i, col, marginMedium);
                                parentId3 = $(this).attr("data-id");
                            }
                        } else {
                            table.treeFilter(i, col, marginLarge);
                        }

                    }
                }
            }
        });
    }
    
    orderingRows(rowEvaluabilityTr, rowEvaluability,3);
    orderingRows(rowEconomicTr, rowEconomic,3);
    orderingRows(rowMonitoringTr, rowMonitoring, 3);
    orderingRows(rowCountryTr, rowCountry, 2);

    rowEvaluability.treeFilterBackColor(15, backColorGrayLight);
    rowEvaluability.treeFilterBackColor(17, backColorWhite);
    rowEvaluability.treeFilterBackColor(21, backColorWhite);
    rowEvaluability.treeFilterBackColor(27, backColorWhite);
    rowEvaluability.treeFilterBackColor(41, backColorWhite);
    rowEvaluability.treeFilterBackColor(45, backColorWhite);
    rowEvaluability.treeFilterBackColor(49, backColorWhite);
        
    rowEconomic.treeFilterBackColor(0, backColorGray);
    rowEconomic.treeFilterBackColor(1, backColorWhite);
    rowEconomic.treeFilterBackColor(2, backColorGrayLight);
    rowEconomic.treeFilterBackColor(4, backColorWhite);
    rowEconomic.treeFilterBackColor(6, backColorGrayLight);
    rowEconomic.treeFilterBackColor(8, backColorWhite);
    rowEconomic.treeFilterBackColor(26, backColorWhite);
    rowEconomic.treeFilterBackColor(30, backColorWhite);
    rowEconomic.treeFilterBackColor(34, backColorWhite);
    rowEconomic.treeFilterBackColor(38, backColorWhite);
       

    rowMonitoring.treeFilterBackColor(17, backColorWhite);
    rowMonitoring.treeFilterBackColor(37, backColorWhite);
    rowMonitoring.treeFilterBackColor(41, backColorWhite);
    rowMonitoring.treeFilterBackColor(45, backColorWhite);

    rowCountry.deleteFilter(32);
    
    rowCountry.deleteFilter(35);   

    rowCountry.treeFilterBackColor(1, backColorGray);
    rowCountry.treeFilterBackColor(5, backColorWhite);
    rowCountry.treeFilterBackColor(9, backColorWhite);
    rowCountry.treeFilterBackColor(13, backColorWhite);
    rowCountry.treeFilterBackColor(20, backColorWhite);
    rowCountry.treeFilterBackColor(23, backColorWhite);
    rowCountry.treeFilterBackColor(25, backColorGray);
    rowCountry.treeFilterBackColor(29, backColorWhite);
    rowCountry.treeFilterBackColor(33, backColorWhite);
    rowCountry.treeFilterBackColor(35, backColorWhite);

    rowIDBInvolvement.treeFilterBackColor(0, backColorGray);
}

function commentsDivColor() {

    var currentDivComment = $(".contentComment");
    currentDivComment.each(function () {
        var currentRow = $(this);
        searchLastOldCommentAndMarkIt(currentRow, "label");
        searchLastOldCommentAndMarkIt(currentRow, "textarea");
    })
}

function commentsGroupColor(IdTable) {
    var currentTable = $("#" + IdTable);
    var currentTableRows = currentTable.find("tbody tr");
    currentTableRows.each(function () {
        var currentRow = $(this);
        searchLastOldCommentAndMarkIt(currentRow, "label");
        searchLastOldCommentAndMarkIt(currentRow, "textarea");
    })
}

function searchLastOldCommentAndMarkIt(currentRow, elementType) {
    currentRowOldEditCommentsRead = currentRow.find("span[data-pagemode=read]")
        .find(elementType + '.borderLightBlue');

    currentRowOldEditCommentsEdit = currentRow.find("span[data-pagemode=edit]")
        .find(elementType + '.borderLightBlue');
    if (currentRowOldEditCommentsRead.length > 0) {
        var lastComment = currentRowOldEditCommentsRead.last();
        AddLightBlueClassAndNewBanner(lastComment, currentRow);
    }
    if (currentRowOldEditCommentsEdit.length > 0) {
        var lastComment = currentRowOldEditCommentsEdit.last();
        AddLightBlueClassAndNewBanner(lastComment, currentRow);
    }
}

function AddLightBlueClassAndNewBanner(lastComment, currentRow) {
    var iconNewComment = $('<span></span>', {
        class: 'commentIcon',
    })

    var elementParent = lastComment.parents("tr").prev("tr");
    elementParent.find(".iconDemNew").append($(iconNewComment));
}

function newCommentDesign() {
    commentsGroupColor("tableEvaluability");
    commentsGroupColor("tableEconomicAnalysis");
    commentsGroupColor("tableMonitoringAndEvaluation");
    commentsGroupColor("tableRiskMatrix");
    commentsGroupColor("tableMitigationMeasures");
    commentsGroupColor("tableCountrySystems");
    commentsGroupColor("tableIDBInvolvement");
    commentsGroupColor("tableTechnicalAssistance");
    commentsGroupColor("tableImpactEvaluation");
}

function marginFix() {
    $(".marginFix").next(".tabs").css("margin-bottom", "0px");
}

function hideOrShowRegionRelevanceOperation() {
    var isIncludeOpr = $('input[name=hiddenIndicativePipelines]').val();
    var regionRelevanceOperation = $("div[name=regionRelevanceOperation]");
    var enRelevanceArea = $("textarea[name=enRelevanceArea]");
    var esRelevanceArea = $("textarea[name=esRelevanceArea]");

    if (isIncludeOpr != undefined) {
        var isOperationOpr = $('input[name=hiddenIsOperationOpr]').val();

        if ((isIncludeOpr === "False")
            && !$('input[name=objectivesAligned]').prop("checked")
            && !(isOperationOpr === "False")) {
            regionRelevanceOperation.removeClass('hide');
            enRelevanceArea.attr("data-force-parsley-validation", "true");
            enRelevanceArea.attr("data-parsley-required", "true");
            esRelevanceArea.attr("data-force-parsley-validation", "true");
            esRelevanceArea.attr("data-parsley-required", "true");
        }
        else {
            regionRelevanceOperation.addClass('hide');
            enRelevanceArea.attr("data-force-parsley-validation", "false");
            enRelevanceArea.attr("data-parsley-required", "false");
            esRelevanceArea.attr("data-force-parsley-validation", "false");
            esRelevanceArea.attr("data-parsley-required", "false");
        }
    }
}

function initialRegionRelevance() {
    var isIncludeOpr = $('input[name=hiddenIndicativePipelines]').val();
    var enRelevanceArea = $("textarea[name=enRelevanceArea]");
    var esRelevanceArea = $("textarea[name=esRelevanceArea]");
    if (isIncludeOpr != undefined) {
        if (!$('input[name=objectivesAligned]').prop("checked")) {
            enRelevanceArea.attr("data-force-parsley-validation", "false");
            enRelevanceArea.attr("data-parsley-required", "false");
            esRelevanceArea.attr("data-force-parsley-validation", "false");
            esRelevanceArea.attr("data-parsley-required", "false");
        }
        else {
            enRelevanceArea.attr("data-force-parsley-validation", "true");
            enRelevanceArea.attr("data-parsley-required", "true");
            esRelevanceArea.attr("data-force-parsley-validation", "true");
            esRelevanceArea.attr("data-parsley-required", "true");
        }
    }
}

function diplayShowComment() {
    $('.oldCommentGray').not('.fromTable').parent().addClass('hide');
}

function alignTrashCan() {
    $('.deleteButtonNewComment').removeClass();
}

function getTabActive() {
    var tabContainerActive = $('#containerTabs').children('.active').attr('id');
    return $('#' + tabContainerActive + ' div.active').attr('id');
}