
var resource_messages = {};
var _mvcajax = {};
var __block_elements = [];
var financialData = null;

var current_graph_data, current_graph_data2,
    current_graph_data3;


_mvcajax.process_response = function (response) {
    var fn;
    switch (response.action) {
        case 'show_error':
            _mvcajax.set_message(response.error_text, 'error');
            break;
        case 'call_function':
            fn = window[response.call_function_fn];
            fn.apply(null);
            break;
        case 'call_function_parametered':
            fn = window[response.call_function_fn];
            var paramters = $.parseJSON(response.call_function_parameters);
            fn.apply(null, paramters);
            break;

        case 'reload':
            document.location.reload();
            break;
    }
};

_mvcajax.request_defaults = {
    waiting_element: undefined,
    type: 'json'
};

//Make a new ajax request and procees the result depending of 
//response parameters. 
_mvcajax.request = function (parameters) {
    parameters = jQuery.extend({}, _mvcajax.request_defaults, parameters);

    if (parameters.waiting_element) {
        __block_elements.push(parameters.waiting_element);
    }
    if (parameters.type == 'post') {
        $.ajax({
            type: "POST",
            url: parameters.url,
            data: parameters.data,
            async: true,
            global: false,
            success: function (response) {
                __block_elements = [];
                _mvcajax.process_response(response, true);
            },
            dataType: 'json'
        })
    } else {
        jQuery.get(parameters.url,
            parameters.data,
            function (response) {
                __block_elements = [];
                _mvcajax.process_response(response, true);
            }, parameters.type);
    }
};

_mvcajax.add_message_page_notification = function (message, type, id) {
    _mvcajax.set_message_page_notification(message, type, false, id);
};

_mvcajax.remove_message_page_notification = function (id) {
    $('.messages').find('li[data-id="' + id + '"] .removeMsg').click();
};

_mvcajax.set_message_page_notification = function (mesage, type, isSaving, id) {
    if (mesage.length == 0) {
        return;
    }

    if (isSaving == null) {
        isSaving = false;
    }

    if (id == undefined) {
        id = "";
    }

    if (type == 'warning') {
        var field = $('.messages')
            .removeClass('message-')
            .removeClass('message-error')
            .removeClass('message-warning')
            .removeClass('message-notification');

        if (type == 'error') {
            field.addClass('message-error');
        } else if (type == 'warning') {
            field.addClass('message-warning');
        } else if (type == 'notification') {
            field.addClass('message-notification');
        }

        field.find('ul').append('<li data-id="' + id + '"><span class="glyphicon glyphicon-remove mr15 pointer removeMsg"></span>' + mesage + '</li>');
    }
    else {
        showNotification({ "message": mesage, "type": type, "autoClose": true, "duration": 2000 });
    }
};

_mvcajax.set_message_global_notification = function (message, type, isSaving) {
    if (isSaving != null) {
        _mvcajax.set_message_page_notification(message, type, isSaving);
    }
    else {
        _mvcajax.set_message_page_notification(message, type);
    }
};

_mvcajax.set_message = function (message, type) {
    if (message instanceof Array) {
        _mvcajax.set_message_global_notification(message, type, true);
    } else {
        _mvcajax.set_message_page_notification(message, type);
    }
};

_mvcajax.clear_message = function () {
    _mvcajax.set_message('');
};

$(document).ready(function () {

    $('.click-action').click(function () {
        var element = $(this);

        if (element.attr('data-target-url')) {
            document.location.href = element.attr('data-target-url');
        }
    });

    $("#btnAddFinancial").click(function () {
        $("#grid_projections_edit").html(financialData);
        initializeDPEvents();
        bindHandlers();
        financialData = null;
    });

    $("#btnCancelAddFinancial").click(function () {
        financialData = null;
    });

    $("#btnYesCurr").click(function () {
        showLoaderOptional();
        setTimeout(function ()
        {           
            retriveFinnancialLogic($("button[name='btnRetriveFinnancial']"));
        }, 2000);        
    });
});

disbursement_cancel_edit_form = function () {
    showLoaderOptional();
    var url = document.location.href;
    url = url.replace('&ed=1', '');
    document.location.href = url;
};

function disbursement_redirect_save() {
    var url = document.location.href;
    url = url.replace('&ed=1', '');
    url = url.replace('&IsFromSharePoint=false', '');
    document.location.href = url;
}


function disbursement_send_matrix(level, operationNumber, group) {
    showLoaderOptional();
    var call_parameters = {};
    var show_confirmation = false;

    $('.disbursement-grid-value').each(function () {
        var field = $(this);
        var id = field.attr('id') + '-value';
        call_parameters[id] = field.val().replace(/,/g, '');
        var value = parseFloat(field.val().replace(/,/g, ''));
        if (value > 0 && field.hasClass('disbursement-after-expiration')) {
            show_confirmation = true;
        }
    });

    if (show_confirmation) {
        hideLoaderOptional();
        confirmAction(idbg._(disbursmentLiterals.msgConfirmExpirationDateAfter)).done(function (pressOk) {
           
            if (pressOk) {
                showLoaderOptional();
                call_parameters['loanNumber'] = $('#loan_number').val();
                call_parameters['fundCode'] = $('#fund_code').val();
                call_parameters['loanTcdNum'] = $('#loan_tcdnum').val();
                call_parameters['loanProjectNumber'] = $('#loan_projectnumber').val();

                call_parameters['actualDisbursementProjection'] = $("#actualDisbursementProjection").val();
                call_parameters['operationNumber'] = operationNumber;
                call_parameters['group'] = group;
                call_parameters['disbursementCancellations'] = $('#disbursement_cancellations').val();

                call_parameters['disbursementBalance'] = $('#disbursement_balance').val();
                call_parameters['currCode'] = $('#CurrCode').val();
                call_parameters['originalExpirationDate'] = $('#OriginalExpirationDate').val();
                call_parameters['currentDisbursementExpirationDate'] = $('#CurrentDisbursementExpirationDate').val();

                _mvcajax.request({
                    url: disbursement_edit_path,
                    data: call_parameters,
                    type: 'post'
                });
            } else {
                hideLoaderOptional();
            }
        });
    } else {
        call_parameters['loanNumber'] = $('#loan_number').val();
        call_parameters['fundCode'] = $('#fund_code').val();
        call_parameters['loanTcdNum'] = $('#loan_tcdnum').val();
        call_parameters['loanProjectNumber'] = $('#loan_projectnumber').val();
        call_parameters['actualDisbursementProjection'] = $("#actualDisbursementProjection").val();
        call_parameters['operationNumber'] = operationNumber;
        call_parameters['group'] = group;
        call_parameters['disbursementCancellations'] = $('#disbursement_cancellations').val().replace(/,/g, '');
        call_parameters['disbursementBalance'] = $('#disbursement_balance').val();
        call_parameters['currCode'] = $('#CurrCode').val();
        call_parameters['originalExpirationDate'] = $('#OriginalExpirationDate').val();
        call_parameters['currentDisbursementExpirationDate'] = $('#CurrentDisbursementExpirationDate').val();

        _mvcajax.request({
            url: disbursement_edit_path,
            data: call_parameters,
            type: 'post'
        });
    }

    return false;
}

function disbursement_validate() {
    var negativeCount = 0;
    var years = {};
    var projected_total = 0;
    var balance = parseFloat($('#disbursement_balance').val());
    var cancellations_original = $('#disbursement_cancellations').val();
    var save_button = $('.disbursement-save-button');
    var field = $(this);

    var currentMonthDisbursement = $("#actualDisbursementProjection").val();
    if (currentMonthDisbursement == 'undefined') {
        currentMonthDisbursement = 0;
    } else {
        currentMonthDisbursement = parseFloat(currentMonthDisbursement.replaceAll(',', ''));
    }

    if (cancellations_original == '') {
        cancellations_original = '0';
        $('#disbursement_cancellations').val(cancellations_original);
    }
    cancellations = parseFloat(cancellations_original.replaceAll(',', ''));
    if (cancellations < 0)
    {
        $('#disbursement_cancellations').addClass('validation-fail');
        negativeCount++;
    }
    else {
        $('#disbursement_cancellations').removeClass('validation-fail');
    }

    save_button.attr('disabled', 'disabled');
    save_button.addClass('disabled');

    //sum each field value
	$('.disbursement-grid-value').each(function () {
        var element = $(this);
        var year = element.attr('id').split('_')[1];

        if (!(year in years)) {
            years[year] = 0.0;
        }
        var value = element.val().replace(/,/g, '');
        if (value == '') {
            value = '0';
            element.val(value);
        }

        var val = parseFloat(value);
		years[year] += val;
        projected_total += val;
		
		projected_total = Math.round(projected_total*100)/100;
		years[year] = Math.round(years[year]*100)/100;

        projected_total = Math.round(projected_total * 100) / 100;
        years[year] = Math.round(years[year] * 100) / 100;

        if (value < 0) {
            $(element).addClass('validation-fail');
            negativeCount++;
        }
        else {
            $(element).removeClass('validation-fail');
        }
    });


    var remaining_balance = parseFloat(balance - (projected_total - currentMonthDisbursement + cancellations));
    var remaining = remaining_balance.toLocaleString('en-US', { style: 'currency', currency: 'USD', currencyDisplay: 'symbol' }).replace(/[$)]/g, '').replace('(','-');
    $('.disbursement_remaining_balance').text(remaining);
    var totalKeys = $('#currentProjectionTable').find('[data-type="total"]');

    var value = 0;
    $('#currentProjectionTable').find('[data-type="value"] [disabled]:not([ignore-value])').each(function () {

        if ($(this).length > 0) {
            value += $(this).val().replaceAll(',', '') * 1;
        }
    });

    var balanceValue = balance + value;
    totalKeys.each(function () {
        var totalYear = parseFloat($(this).text().replaceAll(',', ''));
        var percentage = totalYear / (balanceValue > 0 ? balanceValue : 1)  * 100;
        $(this).closest('tr').find('[data-type="percent"] label').text(percentage.toFixed(2).replace(/./g, function (c, i, a) {
            return i && c !== "." && (a.length - i) % 3 === 0 ? ',' + c : c;
        }) + "%");
    });

    //set years sum
    for (key in years) {
        $('#total_' + key).text(years[key]);
    }

    if (remaining_balance != 0) {      
        _mvcajax.remove_message_page_notification("sumBalance");
        _mvcajax.add_message_page_notification(disbursmentLiterals.msgRemainingBalanceWarning, 'warning', "sumBalance");
    } else {     
        _mvcajax.remove_message_page_notification("sumBalance");
    }

    if (negativeCount > 0)
    {
        _mvcajax.remove_message_page_notification("negNumber");
        _mvcajax.add_message_page_notification(disbursmentLiterals.msgNegativeNumbers, 'warning', "negNumber");
    }
    else
    {
        _mvcajax.remove_message_page_notification("negNumber");
    }

    if (remaining_balance == 0 && negativeCount == 0)
    {
        save_button.attr('disabled', false);
        save_button.removeClass('disabled');
    }
    else
    {
        save_button.attr('disabled', 'disabled');
        save_button.addClass('disabled');
    }
}

function disbursement_show_edit_form() {
    showLoader();
    document.location.href = document.location.href + '&ed=1';
}

function disbursement_show_graph(graph_data) {
    current_graph_data = graph_data;
    var currExpirationDateVisibility = graph_data.currentExpirationDate[0][0] != 0;
    var currDateVisibility = graph_data.currentDate[0][0] != 0;
    var plotLines = [];

    if (currDateVisibility)
    {
        plotLines.push({
            lineWidth: 1,
            strokeStyle: 'gray',
            value: graph_data.currentDate[0][0],
            zIndex: 2,
            title: {
                position: 'outside',
                text: disbursmentLiterals.GraphProjectLifeCurrentDate,
                vAlign: 'top',
                fillStyle: 'gray'
            },
            strokeDashArray: [10, 10]
        });
    }
   
    if (currExpirationDateVisibility)
    {
        plotLines.push({
            lineWidth: 1,
            strokeStyle: 'gray',
            value: graph_data.currentExpirationDate[0][0],
            zIndex: 2,
            title: {
                position: 'outside',
                text: disbursmentLiterals.GraphProjectLifeCurrentExpirationDate,
                vAlign: 'top',
                fillStyle: 'gray'
            },
            strokeDashArray: [10, 10]
        })
    }

    $('#projected_graph').jqChart({
        title: {
            text: disbursmentLiterals.titleGraph1,
            font: '18px OpenSans-Bold'
        },
        tooltips: { type: 'shared' },
        border: {
        visible: false
        },
        axes: [
            {
                name: 'y',
                location: 'left',
                type: 'linear',
                maximum: 1.1,
                title: {
                    text: disbursmentLiterals.GraphProjectLifeAxisY,
                    fillStyle: 'gray'
                },
            },
            {
                name: 'x',
                location: 'bottom',
                type: 'linear',
                interval: 12,
                plotLines: plotLines,
                title: {
                    text: disbursmentLiterals.GraphProjectLifeAxisX,
                    fillStyle: 'gray'
                },
            }
        ],
        series: [
            {
                axisX: 'x',
                type: "spline",
                title: disbursmentLiterals.GraphProjectLifeDisbursed,
                markers: {
                    size: 0.9
                },
                data: graph_data.disbursed
            }, {
                axisX: 'x',
                type: "spline",
                title: disbursmentLiterals.GraphProjectLifeProjectedDisbursements,
                markers: {
                    size: 0.9
                },
                data: graph_data.projected_disbursement
            }, {
                axisX: 'x',
                type: "spline",
                title: disbursmentLiterals.GraphProjectLifeCountryProfile,
                markers: {
                    size: 0.9
                },
                data: graph_data.country_profile
            }, {
                axisX: 'x',
                type: "spline",
                title: disbursmentLiterals.GraphProjectLifeCountryUpperLimit,
                markers: {
                    size: 0.9
                },
                data: graph_data.country_upper
            }, {
                axisX: 'x',
                type: "spline",
                title: disbursmentLiterals.GraphProjectLifeCountryLowerLimit,
                markers: {
                    size: 0.9
                },
                data: graph_data.country_lower
            }
        ]
    });
}

function disbursement_show_yearlygraph(graph_data) {
    current_graph_data2 = graph_data;
    $('#earlyAcumulated_graph').jqChart({
        title: {
            text: disbursmentLiterals.titleGraph2,
            font: '18px OpenSans-Bold'
        },
        tooltips: { type: 'shared' },
        border: {
            visible: false
        },
        axes: [
            {
                name: 'y',
                location: 'left',
                type: 'linear',
                title: {
                    text: disbursmentLiterals.GraphEarlyAccumulatedAxisY,
                    fillStyle: 'gray'
                },
            },
            {
                name: 'x',
                location: 'bottom',
                type: 'category',
                categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                title: {
                    text: disbursmentLiterals.GraphEarlyAccumulatedAxisX,
                    fillStyle: 'gray'
                }
            }
        ],
        series: [
            {
                axisX: 'x',
                type: "line",
                title: disbursmentLiterals.GraphEarlyAccumulatedAgreedAccumProj,
                markers: {
                    size: 0.9
                },
                data: graph_data.actual_acumulated
            }, {
                axisX: 'x',
                type: "line",
                title: disbursmentLiterals.GraphEarlyAccumulatedCurrentAccumProj,
                markers: {
                    size: 0.9
                },
                data: graph_data.current_acumulated
            }, {
                axisX: 'x',
                type: "line",
                title: disbursmentLiterals.GraphEarlyAccumulatedActualAccumDisbToDate,
                markers: {
                    size: 0.9
                },
                data: graph_data.accumulated_projection
            }
        ]
    }).bind('tooltipFormat', function (e, data) {
        var tooltip = '<div>' + data[0].x + '</div>';

        for (var i = 0; i < data.length; i++) {
            var value = data[i].dataItem.toFixed(2).replace(/./g, function (c, i, a) {
                return i && c !== "." && (a.length - i) % 3 === 0 ? ',' + c : c;
            });

            var color = 'color:' + data[i].series.fillStyle;
            var title = data[i].series.title;
            tooltip += '<div style="' + color + '">' + title + ': $<b>' + value + '</b></div>';
        }

        return tooltip;
    }).bind('axisLabelCreating', function (event, data) {
        if (data.context.axis.location === 'left' && data.text >= 0) {
            data.text = '$' + (data.text * 1).toFixed(2).replace(/./g, function (c, i, a) {
                return i && c !== "." && (a.length - i) % 3 === 0 ? ',' + c : c;
            });
        }
    });

}
function disbursement_show_monthlygraph(graph_data) {
    current_graph_data3 = graph_data;
    $('#monthlyAcumulated_graph').jqChart({
        title: {
            text: disbursmentLiterals.titleGraph3,
            font: '18px OpenSans-Bold'
        },
        tooltips: { type: 'shared' },
        border: {
            visible: false
        },
        axes: [
            {
                name: 'y',
                location: 'left',
                type: 'linear',
                title: {
                    text: disbursmentLiterals.GraphMonthlyAccumulatedAxisY,
                    fillStyle: 'gray'
                }
            },
            {
                name: 'x',
                location: 'bottom',
                type: 'category',
                categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                title: {
                    text: disbursmentLiterals.GraphMonthlyAccumulatedAxisX,
                    fillStyle: 'gray'
                }
            }
        ],
        series: [
            {
                axisX: 'x',
                type: "column",
                title: disbursmentLiterals.GraphMonthlyAccumulatedAgreedProjs,
                markers: {
                    size: 0.9
                },
                data: graph_data.agreedProjections
            }, {
                axisX: 'x',
                type: "column",
                title: disbursmentLiterals.GraphMonthlyAccumulatedCurrentProjs,
                markers: {
                    size: 0.9
                },
                data: graph_data.currentProjections
            }, {
                axisX: 'x',
                type: "column",
                title: disbursmentLiterals.GraphMonthlyAccumulatedActualProjs,
                markers: {
                    size: 0.9
                },
                data: graph_data.actualDisbursements
            }
        ]
    }).bind('tooltipFormat', function (e, data) {

        var tooltip = '<div style="color:' + data.series.fillStyle + '">' + data.series.title + '</div>';

        var value = data.dataItem.toFixed(2).replace(/./g, function (c, i, a) {
            return i && c !== "." && (a.length - i) % 3 === 0 ? ',' + c : c;
        });
        tooltip += '$<b>' + value + '</b>';

        return tooltip;
    }).bind('axisLabelCreating', function (event, data) {
        if (data.context.axis.location === 'left' && data.text >= 0) {
            data.text = '$' + (data.text * 1).toFixed(2).replace(/./g, function (c, i, a) {
                return i && c !== "." && (a.length - i) % 3 === 0 ? ',' + c : c;
            });
        }
    });
}

function convertCurrency(elem, curr) {
    var url = $(elem).attr('data-url');
    var hidModel = $("#hidModel");
    var json = hidModel.attr("data-json");
    var urlGroupingSection = hidModel.attr("data-url-1");
    var urlRefInformation = hidModel.attr("data-url-2");
    var urlGridProjections = hidModel.attr("data-url-3");
    var selectedYear = $("#dropdownYearSelection").val();
    var params = $.parseJSON(json);
    var graphModel = $.parseJSON($("#hidGraphModel").attr('data-json'));

    HasOperationLevelProjection = params.CurrCode == null;

    var newModel = {
        HasOperationLevelProjection: HasOperationLevelProjection,
        Balance: params.Balance,
        RemainingBalance: params.RemainingBalance,
        IsExecution: params.IsExecution,
        EditMode: params.EditMode,
        Cancellations: params.Cancellations,
        CurrCode: HasOperationLevelProjection ? params.Currency : params.CurrCode,
        PartialEligibilityDate: params.PartialEligibilityDate,
        TotalEligibilityDate: params.TotalEligibilityDate,
        OriginalExpirationDate: params.OriginalExpirationDate,
        CurrentDisbursementExpirationDate: params.CurrentDisbursementExpirationDate,
        CummulativeExtensions: params.CummulativeExtensions,
        OperationNumber: params.OperationNumber,
        LoanNumber: params.LoanNumber,
        ProjectedYears: params.ProjectedYears,
        ProjectedMonths: params.ProjectedMonths,
        SelectedYear: selectedYear,
        ActualAccumDisbCurrentAccumProj: params.Projects[disbursmentLiterals.Actual],
        AccumProjAgreedAccumProjPerc: params.Projects[disbursmentLiterals.ActualAgree],
        AccumProjAgreedAccumProj: params.Projects[disbursmentLiterals.ActualAgreeAccum],
        AgreedAccumProj: params.Projects[disbursmentLiterals.AgreedAccum],
        AgreedProjection: params.Projects[disbursmentLiterals.AgreedProjection],
        ExchangeRate: 1,
        Total: params.Total,
        AnnualProjectedInitial: params.AnnualProjectedInitial,
        MinimumAmountPending: params.MinimumAmountPending,
        GovermentBudget:params.GovermentBudget,
        OtherFinancingSources: params.OtherFinancingSources,
        CurrentFinancialPlanningPeriod: params.CurrentFinancialPlanningPeriod,
        CurrentProjectionMonth: params.CurrentProjectionMonth,
    };
    
    showLoaderOptional();
    setTimeout(function () {
        if (hidModel.val() > 0) {
            newModel.ExchangeRate = parseFloat(hidModel.val());           
            headerSection(newModel, urlGroupingSection);
            referenceInformation(newModel, urlRefInformation);

            newModel.OldProjection = params.OldProjection;
            newModel.ProjectedYears = params.ProjectedYears;
            gridProjectionsReload(newModel, urlGridProjections);
            graphModel.ExchangeRate = newModel.ExchangeRate;
            var graphData = graphModel;
            GraphMonthlyProyections(graphData);
            GraphYearlyProyections(graphData);

            $(elem).addClass("hidden");
            imput2 = $("#back-disbursement-convert");
            $(imput2).removeClass("hidden");
            $("[data-id=expressedCurrency]").text("USD");

            hideLoaderOptional();
        }
        else {
            $.ajax({
                url: url,
                type: "POST",
                data: { currency: newModel.CurrCode },
                success: function (data) {
                    if (data.IsValid) {
                        var exchangeRate = data.Amount;
                        if (exchangeRate != null) {
                            newModel.ExchangeRate = parseFloat(exchangeRate);
                            $('#hidModel').val(newModel.ExchangeRate);
                            $(elem).addClass("hidden");
                            imput2 = $("#back-disbursement-convert");
                            $(imput2).removeClass("hidden");

                            headerSection(newModel, urlGroupingSection);
                            referenceInformation(newModel, urlRefInformation);

                            newModel.OldProjection = params.OldProjection;
                            newModel.ProjectedYears = params.ProjectedYears;
                            gridProjectionsReload(newModel, urlGridProjections);
                            graphModel.ExchangeRate = newModel.ExchangeRate;
                            var graphData = graphModel;
                            GraphMonthlyProyections(graphData);
                            GraphYearlyProyections(graphData);
                            $("[data-id=expressedCurrency]").text("USD");
                            hideLoaderOptional();
                        }
                    }
                    else {                       
                        var errorMsg = disbursmentLiterals.msgErrorExchangeRate;
                        if (data.ErrorMessage != null && data.ErrorMessage.length > 0) {
                            errorMsg = data.ErrorMessage;                          
                        }
                        hideLoaderOptional();
                        showNotification({ "message": errorMsg, "type": 'error', "autoClose": true, "duration": 2000 })
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    hideLoaderOptional();
                    if (XMLHttpRequest.status == 500)
                    {
                        showNotification({ "message": XMLHttpRequest.responseText, "type": 'error', "autoClose": true, "duration": 2000 })
                    }
                                                    
                }
            });
        }
    }, 500);
}

function backToCurrency(elem) {
    var hidModel = $("#hidModel");
    var json = hidModel.attr("data-json");
    var graphModel = $("#hidGraphModel").attr('data-json');
    var urlGroupingSection = hidModel.attr("data-url-1");
    var urlRefInformation = hidModel.attr("data-url-2");
    var urlGridProjections = hidModel.attr("data-url-3");
    var selectedYear = $("#dropdownYearSelection").val();
    var params = $.parseJSON(json);

    showLoaderOptional();
    HasOperationLevelProjection = params.CurrCode == null;

    var oldModel = {
        Balance: params.Balance,
        RemainingBalance: params.RemainingBalance,
        IsExecution: params.IsExecution,
        EditMode: params.EditMode,
        Cancellations: params.Cancellations,
        CurrCode: HasOperationLevelProjection ? params.Currency : params.CurrCode,
        PartialEligibilityDate: params.PartialEligibilityDate,
        TotalEligibilityDate: params.TotalEligibilityDate,
        OriginalExpirationDate: params.OriginalExpirationDate,
        CurrentDisbursementExpirationDate: params.CurrentDisbursementExpirationDate,
        CummulativeExtensions: params.CummulativeExtensions,
        OperationNumber: params.OperationNumber,
        LoanNumber: params.LoanNumber,
        ProjectedYears: params.ProjectedYears,
        ProjectedMonths: params.ProjectedMonths,
        SelectedYear: selectedYear,

        ActualAccumDisbCurrentAccumProj: params.Projects[disbursmentLiterals.Actual],
        AccumProjAgreedAccumProjPerc: params.Projects[disbursmentLiterals.ActualAgree],
        AccumProjAgreedAccumProj: params.Projects[disbursmentLiterals.ActualAgreeAccum],
        AgreedAccumProj: params.Projects[disbursmentLiterals.AgreedAccum],
        AgreedProjection: params.Projects[disbursmentLiterals.AgreedProjection],

        ExchangeRate: 1,
        Total: params.Total,
        CurrentProjectionMonth: params.CurrentProjectionMonth,
        AnnualProjectedInitial: params.AnnualProjectedInitial,
        MinimumAmountPending: params.MinimumAmountPending,
        GovermentBudget: params.GovermentBudget,
        OtherFinancingSources: params.OtherFinancingSources,
        CurrentFinancialPlanningPeriod: params.CurrentFinancialPlanningPeriod
    };
    $("[data-id=expressedCurrency]").text(oldModel.CurrCode);

    setTimeout(function () {
        headerSection(oldModel, urlGroupingSection);

        var inputYear = $("#dropdownYearSelection").val();
        inputYear = inputYear != null && inputYear.trim() != "" && !isNaN(inputYear)
            ? parseInt(inputYear) : null
        if (inputYear != null && inputYear != (new Date).getFullYear())
        {        
            changeInformation(inputYear, oldModel.ExchangeRate);
        }
        else
        {
            referenceInformation(oldModel, urlRefInformation);
        }
       
        oldModel.OldProjection = params.OldProjection;
        oldModel.ProjectedYears = params.ProjectedYears;

        gridProjectionsReload(oldModel, urlGridProjections);
        GraphMonthlyProyections($.parseJSON(graphModel));
        GraphYearlyProyections($.parseJSON(graphModel));

        $(elem).addClass("hidden");
        $("#disbursement-convert").removeClass("hidden");
        

        hideLoaderOptional();
    },500);
}

function headerSection(newModel, urlGroupingSection) {
    appendPartial(newModel, urlGroupingSection, $("#grouping-section"));
}

function referenceInformation(newModel, urlRefInformation) {
    appendPartial(newModel, urlRefInformation, $("#reference-information-table"));
}

function gridProjectionsReload(newModel, urlGridProjections) {
    appendPartial(newModel, urlGridProjections, $("#grid_projections"))
}

function appendPartial(params, url, content) {
    $.ajax({
        url: url,
        type: "POST",
        data: params,
        success: function (result) {
            $(content).html(result);
            bindHandlers();          
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            bindHandlers();
        }
    });
}

$(document).ready(function () {
    $('.graphButton').click(function () {
        setTimeout(function () {
            $('#projected_graph').fadeIn("slow");
            $('#monthlyAcumulated_graph').fadeIn("slow");
            $('#earlyAcumulated_graph').fadeIn("slow");
        }, 500);
    });

    $('.valuesButton').click(function () {
        $('#projected_graph').hide();
        $('#monthlyAcumulated_graph').hide();
        $('#earlyAcumulated_graph').hide();
    }); 
});

$(document).on("click", ".removeMsg", function () {
    var $this = $(this);
    var ul = $this.closest('ul');
    $this.closest('li').remove();

    if (ul.find('li').length == 0) {
        $('.messages')
            .removeClass('message-')
            .removeClass('message-error')
            .removeClass('message-warning')
            .removeClass('message-notification');
    }
});

$(document).on("change", '#dropdownYearSelection', function () {
    var inputValue = $(this).val();
    if (inputValue != null && inputValue.trim() != "")
    {
        showLoaderOptional();
        setTimeout(function (year) {
            changeInformation(year, $("#hidModel").val());
            hideLoaderOptional();
        }, 500, $(this).val());
    }    
});

function changeInformation(year, exchange) {
    var hidModel = $("#hidModel");
    var useRating = $("#disbursement-convert").hasClass("hidden");
    var json = hidModel.attr("data-json");
    var urlRefInformation = hidModel.attr("data-url-2");
    var selectedYear = $("#dropdownYearSelection").val();
    var params = $.parseJSON(json);

    HasOperationLevelProjection = params.CurrCode == null;

    var newModel = {
        HasOperationLevelProjection: HasOperationLevelProjection,
        Balance: params.Balance,
        RemainingBalance: params.RemainingBalance,
        IsExecution: params.IsExecution,
        EditMode: params.EditMode,
        Cancellations: params.Cancellations,
        CurrCode: HasOperationLevelProjection ? params.Currency : params.CurrCode,
        PartialEligibilityDate: params.PartialEligibilityDate,
        TotalEligibilityDate: params.TotalEligibilityDate,
        OriginalExpirationDate: params.OriginalExpirationDate,
        CurrentDisbursementExpirationDate: params.CurrentDisbursementExpirationDate,
        CummulativeExtensions: params.CummulativeExtensions,
        OperationNumber: params.OperationNumber,
        LoanNumber: params.LoanNumber,
        ProjectedYears: params.ProjectedYears,
        ProjectedMonths: params.ProjectedMonths,
        SelectedYear: selectedYear,

        ActualAccumDisbCurrentAccumProj: params.Projects[disbursmentLiterals.Actual],
        AccumProjAgreedAccumProjPerc: params.Projects[disbursmentLiterals.ActualAgree],
        AccumProjAgreedAccumProj: params.Projects[disbursmentLiterals.ActualAgreeAccum],
        AgreedAccumProj: params.Projects[disbursmentLiterals.AgreedAccum],
        AgreedProjection: params.Projects[disbursmentLiterals.AgreedProjection],
        CurrentProjectionMonth: params.CurrentProjectionMonth,
        ExchangeRate: exchange != null && !isNaN(exchange) && exchange > 0 && useRating == true
            ? parseFloat(exchange) : 1,
    };

    if (params.ExchangeRates != null)
    {
        newModel.ExchangeRates = params.ExchangeRates;
    }

    referenceInformation(newModel, urlRefInformation);
}

function retriveFinnancial(elem) {
    var hidModel = $("#hidModel");
    var json = hidModel.attr("data-json");
    var params = $.parseJSON(json);

    if (params.CurrCode != undefined && params.CurrCode != "USD")
    {
        $("#modalDistinctCurrency").modal();
    }
    else
    {
        retriveFinnancialLogic(elem);
    }
}

function retriveFinnancialLogic(elem)
{
    var hidModel = $("#hidModel");
    var json = hidModel.attr("data-json");
    var params = $.parseJSON(json);

    var currentProjectionRequest = new Array();
    $('.disbursement-grid-value').each(function () {
        var field = $(this);
        var original_id = field.attr('id');
        var id = original_id + '-value';
        var value = field.val();
        var date = original_id.split("_");
        currentProjectionRequest.push({ fieldId: original_id, amount: value, month: date[0], year: date[1], isOutofRange: false });
    });

    $('.disabledProjectedValue').each(function () {
        var field = $(this);
        var original_id = field.attr('id');
        var value = field.val();
        var date = original_id.split("_");
        currentProjectionRequest.push({ fieldId: original_id, amount: value, month: date[0], year: date[1], isOutofRange: true });
    });

    params["currentProjectionRequest"] = currentProjectionRequest;
    var url = $(elem).attr("data-url");

    $.post(url, params)
        .success(function (data) {
            if (data.IsValid) {
                switch (data.MessageType) {
                    case 0:
                        $("#grid_projections_edit").html(data.FinancialData);
                        initializeDPEvents();
                        bindHandlers();
                        break;
                    case 2:
                        financialData = data.FinancialData;
                        $("#modalAddFinancialDifference").modal();
                        break;
                    default:
                        break;
                }
            }
            else {
                $("#modalNoProjections").modal();
            }

            hideLoader();
            hideLoaderOptional();
        })
        .error(function (xhr, textStatus, error) {
            if (xhr.statusCode === 204) {
                $("#modalNoProjections").modal();
            }

            hideLoader();
            hideLoaderOptional();
        });
}