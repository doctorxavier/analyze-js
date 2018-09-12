$(document).ready(function() {
    binHandlersBasicData();
});

function binHandlersBasicData() {
    $('[name="editBasicData"]').click(function () {
        showLoaderOptional();
        var url = aditionalFieldHelper.idConcurrenceBasicData + aditionalFieldHelper.instanceId;

        postUrlWithOptions(aditionalFieldHelper.urlConcurrence, { async: true }, { url: url })
            .done(function (data) {
                if (data.IsValid === true) {
                    var editSuccess = enterEditMode(true, $('#basicData'), false);

                    if (editSuccess) {
                        if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked')) {
                            $('[name="DATE_FACE_TO_FACE"]').prop('data-parsley-required', true);
                            $('[name="TIME_FACE_TO_FACE"]').prop('data-parsley-required', true);
                            $('[name="TIME_FACE_TO_FACE"]').prop('disabled', false);
                            $('[id="id-TIME_FACE_TO_FACE"]').prop('disabled', false);
                            $('[name="DATE_FACE_TO_FACE"]').prop('disabled', false);
                            $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').prop('disabled', false);
                            $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').prop('data-parsley-required', true);
                        } else {
                            $('[name="DATE_FACE_TO_FACE"]').prop('data-parsley-required', false);
                            $('[name="TIME_FACE_TO_FACE"]').prop('data-parsley-required', false);
                            $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').prop('data-parsley-required', false);
                            $('[name="CHECK_FACE_TO_FACE"]').prop('disabled', false);
                        }

                        $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', false);
                        bindHandlers($('#basicData'));
                        binHandlersBasicData();
                    }
                } else {
                    showMessage(data.ErrorMessage);
                }
                hideLoaderOptional();
            });
    });

    $('[name="CHECK_CHANGE_DURATION"]').change(function () {
        if ($(this).prop('checked')) {
            $('[name="END_DATE_PROCESS_FLOW"]').attr('disabled', false);
            $('[name="justificationChangeDuration"]').attr('disabled', false);
            $('[data-parent="CHECK_CHANGE_DURATION"]').removeClass('hide');
            $("[name='END_DATE_PROCESS_FLOW']").attr("data-parsley-required", true);
            $("[name='COMMENT_CHANGE_DURATION']").attr("data-parsley-required", true);
            aditionalFieldHelper.endDateCirculation = $('[name="END_DATE_PROCESS_FLOW"]').val();
            showMessage(aditionalFieldHelper.changeDurationMessage);
            setTimeout('removeModal()', 100);
        } else {
            resetChangeDuration();
            calculateDiff();
        }
    });


    $('[name="END_DATE_PROCESS_FLOW"]').change(function () {
        calculateDiff();
    });


    $('input[name="END_DATE_PROCESS_FLOW"]').datepicker('option', {
        beforeShowDay: function (date) {
            var unavailableDates = jQuery.makeArray($('input[name="END_DATE_PROCESS_FLOW"]').attr('data-holidays'));
            var dataHolidays = $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-holidays');

            if (dataHolidays !== undefined && dataHolidays !== null) {
                unavailableDates = jQuery.makeArray(dataHolidays.split('|'));
            }
            var day = date.getDay();
            var formatDate = ("0" + date.getDate()).slice(-2) + "-" + (("0" + (date.getMonth() + 1)).slice(-2)) + "-" + date.getFullYear();
            if ($.inArray(formatDate, unavailableDates) == -1) {
                return [(day > 0 && day < 6), ""];
            } else {
                return [false, "", ""];
            }
        },
        minDate: converToDate($('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-min-date'))
    });

    $('[name="CHECK_FACE_TO_FACE"]').change(function () {
        if ($(this).prop('checked')) {
            selectFaceToFace();
        } else {
            resetFaceToFace();
        }
    });

    var minDate = new Date();
    var minDateDay = minDate.getDay();
    switch (minDateDay) {
        case 0:
            minDate.setDate(minDate.getDate() + 1);
            break;
        case 6:
            minDate.setDate(minDate.getDate() + 2);
            break;
    }

    $('input[name="DATE_FACE_TO_FACE"]').datepicker('option', {
        beforeShowDay: function (date) {
            var unavailableDates = jQuery.makeArray($('input[name="DATE_FACE_TO_FACE"]').attr('data-holidays'));
            var dataHolidays = $('input[name="DATE_FACE_TO_FACE"]').attr('data-holidays');

            if (dataHolidays !== undefined && dataHolidays !== null) {
                unavailableDates = jQuery.makeArray(dataHolidays.split('|'));
            }
            var day = date.getDay();
            var formatDate = ("0" + date.getDate()).slice(-2) + "-" + (("0" + (date.getMonth() + 1)).slice(-2)) + "-" + date.getFullYear();
            if ($.inArray(formatDate, unavailableDates) == -1) {
                return [(day > 0 && day < 6), ""];
            } else {
                return [false, "", ""];
            }
        },
        minDate: minDate
    });
}

function calculateDiff() {
    var startDate = $('input[name="START_DATE_PROCESS_FLOW"]').val();
    var endDate = $('input[name="END_DATE_PROCESS_FLOW"]').val();
    if (endDate !== "") {
        postUrlWithOptions(aditionalFieldHelper.urlDays, { async: true }, { startDate: startDate, endDate: endDate })
        .done(function (data) {
            if (data.IsValid === true) {
                $('[data-type="daysMeeting"]').text("[" + data.AvailableDays + "]");
            } else {
                showMessage(data.ErrorMessage);
            }
        });
    } else {
        $('[data-type="daysMeeting"]').text("[ ]");
        showMessage(aditionalFieldHelper.processFlowEndDateError);
    }
}

function processSubmitArea() {
    var form = [];

    form.push({ Name: "START_DATE_PROCESS_FLOW", Value: $('[name="START_DATE_PROCESS_FLOW"]').val(), Id: "DateTime" });
    form.push({ Name: "END_DATE_PROCESS_FLOW", Value: $('[name="END_DATE_PROCESS_FLOW"]').val(), Id: "DateTime" });

    if ($('[name=CHECK_CHANGE_DURATION]').prop('checked')) {
        form.push({ Name: "CHECK_CHANGE_DURATION", Value: true, Id: "Bool" });
        form.push({ Name: "COMMENT_CHANGE_DURATION", Value: $('[name="COMMENT_CHANGE_DURATION"]').val(), Id: "String" });
    }

    if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked')) {
        form.push({ Name: "CHECK_FACE_TO_FACE", Value: true, Id: "Bool" });
        var dateTimeFaceToFace = $('[name="DATE_FACE_TO_FACE"]').val() + ' ' + $('[name="TIME_FACE_TO_FACE"]').val();
        form.push({ Name: "DATE_FACE_TO_FACE", Value: dateTimeFaceToFace, Id: "DateTime" });
        form.push({ Name: "COMMENT_CHANGE_MEETING_SUB_TYPE", Value: $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').val(), Id: "String" });
    }

    return form;
}

function validateSubmitArea() {
    var isValid = true;
    if ($('[name=CHECK_CHANGE_DURATION]').prop('checked') == true &&
        aditionalFieldHelper.endDateCirculation == $('[name="END_DATE_PROCESS_FLOW"]').val()) {
        showMessage(aditionalFieldHelper.circulationPeriodFailedMessage);
        setTimeout('removeModal()', 100);
        isValid = false;
    }
    
    return isValid;
}

function resetSubmitArea() {
    if ($('[name=CHECK_CHANGE_DURATION]').prop('checked') == false) {
        resetChangeDuration();
    }

    if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked') == false) {
        resetFaceToFace();
    }
}

function resetChangeDuration() {
    $('[name=CHECK_CHANGE_DURATION]').prop("checked", false);
    $('[data-parent="CHECK_CHANGE_DURATION"]').addClass('hide');
    $('[name="COMMENT_CHANGE_DURATION"]').val("");
    $('[name="COMMENT_CHANGE_DURATION"]').removeAttr('required');
    $("[name='END_DATE_PROCESS_FLOW']").attr("data-parsley-required", false);
    $("[name='END_DATE_PROCESS_FLOW']").val($("[name='END_DATE_PROCESS_FLOW']").attr("data-last-value"));
    $("[name='END_DATE_PROCESS_FLOW']").removeClass("parsley-error");
    $("[name='END_DATE_PROCESS_FLOW']").removeAttr('required');
    $("[name='END_DATE_PROCESS_FLOW']").closest('span').find('ul').removeClass("filled");
    $("[name='COMMENT_CHANGE_DURATION']").attr("data-parsley-required", false);
    $("[name='COMMENT_CHANGE_DURATION']").removeClass("parsley-error");
    $("[name='COMMENT_CHANGE_DURATION']").closest('span').find('ul').removeClass("filled");
    $('[name="END_DATE_PROCESS_FLOW"]').attr('disabled', true);
}

function resetFaceToFace() {
    $('[data-parent="CHECK_FACE_TO_FACE"]').addClass('hide');
    $('[name="CHECK_FACE_TO_FACE"]').prop('checked', false);
    $('[aria-labelledby="id-TIME_FACE_TO_FACE"]').FirstorDefault();
    $("[name='DATE_FACE_TO_FACE']").attr("data-parsley-required", false);
    $("[name='DATE_FACE_TO_FACE']").removeAttr('required');
    $("[name='DATE_FACE_TO_FACE']").removeClass("parsley-error");
    $("[name='DATE_FACE_TO_FACE']").closest('div').find('ul').removeClass("filled");
    $("[name='TIME_FACE_TO_FACE']").attr("data-parsley-required", false);
    $("[name='TIME_FACE_TO_FACE']").removeClass("parsley-error");
    $("[name='TIME_FACE_TO_FACE']").removeAttr('required');
    $("[name='TIME_FACE_TO_FACE']").closest('div').find('ul').removeClass("filled");
    $("[name='COMMENT_CHANGE_MEETING_SUB_TYPE']").attr("data-parsley-required", false);
    $("[name='COMMENT_CHANGE_MEETING_SUB_TYPE']").removeClass("parsley-error");
    $("[name='COMMENT_CHANGE_MEETING_SUB_TYPE']").removeAttr('required');
    $("[name='COMMENT_CHANGE_MEETING_SUB_TYPE']").closest('div').find('ul').removeClass("filled");
}

function selectFaceToFace() {
    if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked')) {
        $('[name="DATE_FACE_TO_FACE"]').datepicker('setDate', 'today');
        $('[data-parent="CHECK_FACE_TO_FACE"]').removeClass('hide');
        $('[name="DATE_FACE_TO_FACE"]').attr('data-parsley-required', true);
        $('[name="TIME_FACE_TO_FACE"]').attr('data-parsley-required', true);
        $('[name="TIME_FACE_TO_FACE"]').attr('disabled', false);
        $('[id="id-TIME_FACE_TO_FACE"]').attr('disabled', false);
        $('[name="DATE_FACE_TO_FACE"]').attr('disabled', false);
        $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').attr('data-parsley-required', true);
        $('[name="COMMENT_CHANGE_MEETING_SUB_TYPE"]').attr('disabled', false);
    }
}

function removeModal() {
    $('.vex-open .information').each(function (i) {
        var element = $(this);
        if (i > 0) {
            element[0].parentNode.removeChild(element[0]);
        }
    });
}