$(document).ready(function () {
    binHandlersBasicData();
    InitialBasicData();
});

function binHandlersBasicData() {
    var minDate = new Date($('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-min-date'));
    var maxDate = new Date($('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date'));
    var f2fMinDate = new Date($('input[name="DATE_FACE_TO_FACE"]').attr('dd-min-date'));
    var f2fMaxDate = new Date($('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date'));


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
        minDate:
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-min-date') != null &&
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-min-date') != undefined
                ? minDate
                : null,
        maxDate:
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date') != null &&
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date') != undefined
                ? maxDate
                : null
    });


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
        minDate:
            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-min-date') != null &&
            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-min-date') != undefined
                ? f2fMinDate
                : null,
        maxDate:
            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date') != null &&
            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date') != undefined
                ? f2fMaxDate
                : null
    });
}
function InitialBasicData() {

    $('[name="editBasicData"]').click(function () {
        showLoaderOptional();
        var url = aditionalFieldHelper.idConcurrenceBasicData + aditionalFieldHelper.instanceId;

        postUrlWithOptions(aditionalFieldHelper.urlConcurrence, { async: false }, { url: url })
            .done(function (data) {
                if (data.IsValid === true) {
                    var editSuccess = enterEditMode(true, $('#basicData'), false);

                    if (editSuccess) {
                        bindHandlers($('#basicData'));
                        binHandlersBasicData();
                        InitialBasicData();
                        validationEnabledControls();
                    }
                } else {
                    showMessage(data.ErrorMessage);
                }
            });
        hideLoaderOptional();
    });

    $('[name="CHECK_CHANGE_DURATION"]').click(function () {
        if ($(this).prop('checked')) {
            $('[name="END_DATE_PROCESS_FLOW"]').prop('disabled', false);
            $('[name="COMMENT_CHANGE_DURATION"]').prop('disabled', false);
            $('[data-parent="CHECK_CHANGE_DURATION"]').removeClass('hide');
            $("[name='END_DATE_PROCESS_FLOW']").prop('data-parsley-required', true);
            $("[name='COMMENT_CHANGE_DURATION']").prop('data-parsley-required', true);
            $("[name='END_DATE_PROCESS_FLOW']").prop('required', 'required');
            $("[name='COMMENT_CHANGE_DURATION']").prop('required', 'required');
            aditionalFieldHelper.endDateCirculation = $('[name="END_DATE_PROCESS_FLOW"]').val();
            showMessage(aditionalFieldHelper.changeDurationMessage);
            setTimeout('removeModal()', 100);
        } else {
            resetChangeDuration();
            calculateDiff();
            dateF2f();
        }
    });

    $('[name="CHECK_EXTEND_F2F"]').click(function () {
        if ($(this).prop('checked')) {
            $('[data-parent="CHECK_EXTEND_F2F"]').removeClass('hide');
            $("[name='COMMENT_EXTEND_F2F']").prop('required', 'required');

            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date', $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-last-day-of-year'));
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date', $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-max-date-circulation-ext-f2f'));
        } else {
            if ($('[name="CHECK_CHANGE_DURATION"]').prop('checked')) {
                $("[name='END_DATE_PROCESS_FLOW']").val($("[name='END_DATE_PROCESS_FLOW']").attr("data-last-value"));
            }

            $('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date', $('input[name="DATE_FACE_TO_FACE"]').attr('data-max-date-F2F'));
            $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date', $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-max-date-circulation'));
            calculateDiff();
            dateF2f();
            resetExtendF2f();
        }

        binHandlersBasicData();
    });

    $('[name="END_DATE_PROCESS_FLOW"]').change(function () {
        calculateDiff();
        dateF2f();
        binHandlersBasicData();
    });

    $('[name="CHECK_FACE_TO_FACE"]').click(function () {
        if ($(this).prop('checked')) {
            selectFaceToFace();
            resetChangeDuration();
            resetExtendF2f();
            calculateDiff();
        } else {
            resetFaceToFace();
        }
    });

    $('[name="DATE_FACE_TO_FACE"]').change(function () {
        var faceToFace = $('[name="DATE_FACE_TO_FACE"]').val();
        var end = $('[name="END_DATE_PROCESS_FLOW"]').val();

        if (faceToFace != null && end != null) {
            var faceToFaceDate = new Date(faceToFace);
            var endDate = new Date(end);

            var diff = new Date(faceToFaceDate - endDate);
            var days = diff / 1000 / 60 / 60 / 24;

            if (days < 1) {
                $('[data-type="daysMeeting"]').text("[ ]");
                $('[name="END_DATE_PROCESS_FLOW"]').val("");

                if ($('[name="CHECK_CHANGE_DURATION"]').prop('checked') == false) {
                    $('[name=CHECK_CHANGE_DURATION').click();
                    $('[name=CHECK_CHANGE_DURATION').prop('disabled', true);
                }
            }
        }

        $('#parsley-id-' + $('input[name="DATE_FACE_TO_FACE"]').attr('data-parsley-id')).removeClass('filled').find('.parsley-date-max').remove();
    });
}

function validationEnabledControls() {
    var enabledEndDate = $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-enabled-write-mode');
    var enabledDateF2F = $('input[name="DATE_FACE_TO_FACE"]').attr('data-enabled-write-mode');
    var enabledTimeF2F = $('input[name="TIME_FACE_TO_FACE"]').attr('data-enabled-write-mode');

    if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked')) {
        $('[name="CHECK_FACE_TO_FACE"]').prop('disabled', true);
    } else {
        $('[name="CHECK_FACE_TO_FACE"]').prop('disabled', false);

        if (enabledDateF2F == "True") {
            $('[name="DATE_FACE_TO_FACE"]').prop('disabled', false);
        } else {
            $('[name="DATE_FACE_TO_FACE"]').prop('disabled', true);

        }

        if ($('[name="CHECK_EXTEND_F2F"]').prop('checked') || enabledDateF2F == "False") {
            $('[name="CHECK_EXTEND_F2F"]').prop('disabled', true);
        } else {
            $('[name="CHECK_EXTEND_F2F"]').prop('disabled', false);
        }

        if (enabledTimeF2F == "True") {
            $('[name="TIME_FACE_TO_FACE"]').prop('disabled', false);
            $('[id="id-TIME_FACE_TO_FACE"]').prop('disabled', false);
            $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('readonly', false);
            $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('disabled', false);
        } else {
            $('[name="TIME_FACE_TO_FACE"]').prop('disabled', true);
            $('[id="id-TIME_FACE_TO_FACE"]').prop('disabled', true);
            $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('readonly', true);
            $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('disabled', true);
        }

        if (enabledEndDate == "True") {
            $('[name=COMMENT_CHANGE_VIRTUAL_MEETING]').prop('disabled', false);
            $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', false);
            $('[name="CHECK_FACE_TO_FACE"]').prop('disabled', false);
        } else {
            $('[name=COMMENT_CHANGE_VIRTUAL_MEETING]').prop('disabled', true);
            $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', true);
            $('[name="CHECK_FACE_TO_FACE"]').prop('disabled', true);
        }
    }
}


function processSubmitArea() {
    var form = [];
    var existCheckChangeDuration = $('[name=CHECK_CHANGE_DURATION]') != null &&
								    $('[name=CHECK_CHANGE_DURATION]') != undefined &&
								    $('[name=CHECK_CHANGE_DURATION]').prop('checked') != undefined;

    var existCheckExtendF2F = $('[name=CHECK_EXTEND_F2F]') != null &&
                                    $('[name=CHECK_EXTEND_F2F]') != undefined &&
                                    $('[name=CHECK_EXTEND_F2F]').prop('checked') != undefined;

    var existCheckFaceToFace = $('[name=CHECK_FACE_TO_FACE]') != null &&
								$('[name=CHECK_FACE_TO_FACE]') != undefined &&
								$('[name=CHECK_FACE_TO_FACE]').prop('checked') != undefined;

    var existPoaCheckBox = $('[name="POA_REQUIRED"]') != null &&
							$('[name="POA_REQUIRED"]') != undefined &&
							$('[name="POA_REQUIRED"]').prop('checked') != undefined;

    var existEligibilityCheckbox = $('[name="ELIGIBILITY_GRANTED"]') != null &&
									$('[name="ELIGIBILITY_GRANTED"]') != undefined &&
									$('[name="ELIGIBILITY_GRANTED"]').prop('checked') != undefined;

    if (existCheckChangeDuration) {
        form.push({ Name: "START_DATE_PROCESS_FLOW", Value: $('[name="START_DATE_PROCESS_FLOW"]').val(), Id: "DateTime" });
        form.push({ Name: "END_DATE_PROCESS_FLOW", Value: $('[name="END_DATE_PROCESS_FLOW"]').val(), Id: "DateTime" });
        if ($('[name=CHECK_CHANGE_DURATION]').prop('checked')) {
            form.push({ Name: "CHECK_CHANGE_DURATION", Value: true, Id: "Bool" });
            form.push({ Name: "COMMENT_CHANGE_DURATION", Value: $('[name="COMMENT_CHANGE_DURATION"]').val(), Id: "String" });
        }
    }

    if (existCheckExtendF2F) {
        if ($('[name=CHECK_EXTEND_F2F]').prop('checked')) {
            form.push({ Name: "CHECK_EXTEND_F2F", Value: true, Id: "Bool" });
            form.push({ Name: "COMMENT_EXTEND_F2F", Value: $('[name="COMMENT_EXTEND_F2F"]').val(), Id: "String" });
        } else {
            form.push({ Name: "CHECK_EXTEND_F2F", Value: false, Id: "Bool" });
        }
    }

    if (existCheckFaceToFace) {
        if ($('[name="CHECK_FACE_TO_FACE"]').prop('checked')) {
            form.push({ Name: "CHECK_FACE_TO_FACE", Value: $('[name="CHECK_FACE_TO_FACE"]').prop('checked'), Id: "Bool" });
            form.push({ Name: "COMMENT_CHANGE_VIRTUAL_MEETING", Value: $('[name="COMMENT_CHANGE_VIRTUAL_MEETING"]').val(), Id: "String" });
        } else {
            form.push({ Name: "CHECK_FACE_TO_FACE", Value: $('[name="CHECK_FACE_TO_FACE"]').prop('checked'), Id: "Bool" });
            var dateTimeFaceToFace = $('[name="DATE_FACE_TO_FACE"]').val() + ' ' + $('[name="TIME_FACE_TO_FACE"]').val();
            form.push({ Name: "DATE_FACE_TO_FACE", Value: dateTimeFaceToFace, Id: "DateTime" });
            var meetingRoom = $('[name="MEETING_ROOM_FACE_TO_FACE"]').val();
            form.push({ Name: "MEETING_ROOM_FACE_TO_FACE", Value: meetingRoom, Id: "String" });
        }
    }

    if (existPoaCheckBox) {
        form.push({ Name: "POA_REQUIRED", Value: $('[name="POA_REQUIRED"]').prop('checked'), Id: "Bool" });
    }

    if (existEligibilityCheckbox) {
        form.push({ Name: "ELIGIBILITY_GRANTED", Value: $('[name="ELIGIBILITY_GRANTED"]').prop("checked"), Id: "Bool" });
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

function dateF2f() {
    var f2fMinDate = addDaysWithWeekend(1, $('[name="END_DATE_PROCESS_FLOW"]').val());
    var formatMinDate = $.datepicker.formatDate('dd M yy', f2fMinDate);
    $('input[name="DATE_FACE_TO_FACE"]').attr('dd-min-date', formatMinDate);
    $('input[name="DATE_FACE_TO_FACE"]').val($('input[name="DATE_FACE_TO_FACE"]').attr('dd-min-date'));
}

function resetExtendF2f() {
    $('[name=CHECK_EXTEND_F2F]').prop("checked", false);
    $('[data-parent="CHECK_EXTEND_F2F"]').addClass('hide');
    $("[name='COMMENT_EXTEND_F2F']").prop('data-parsley-required', false);
    $("[name='COMMENT_EXTEND_F2F']").removeClass("parsley-error");
    $("[name='COMMENT_EXTEND_F2F']").closest('span').find('ul').removeClass("filled");
    $('[name="COMMENT_EXTEND_F2F"]').val("");
    $('[name="COMMENT_EXTEND_F2F"]').removeAttr('required');
}
function resetChangeDuration() {
    $('[name=CHECK_CHANGE_DURATION]').prop("checked", false);
    $('[data-parent="CHECK_CHANGE_DURATION"]').addClass('hide');
    $('[name="COMMENT_CHANGE_DURATION"]').val("");
    $('[name="COMMENT_CHANGE_DURATION"]').removeAttr('required');
    $("[name='END_DATE_PROCESS_FLOW']").prop('data-parsley-required', false);
    $("[name='END_DATE_PROCESS_FLOW']").val($("[name='END_DATE_PROCESS_FLOW']").attr("data-last-value"));
    $("[name='END_DATE_PROCESS_FLOW']").removeClass("parsley-error");
    $("[name='END_DATE_PROCESS_FLOW']").removeAttr('required');
    $("[name='END_DATE_PROCESS_FLOW']").closest('span').find('ul').removeClass("filled");
    $("[name='COMMENT_CHANGE_DURATION']").prop('data-parsley-required', false);
    $("[name='COMMENT_CHANGE_DURATION']").removeClass("parsley-error");
    $("[name='COMMENT_CHANGE_DURATION']").closest('span').find('ul').removeClass("filled");
    $('[name="END_DATE_PROCESS_FLOW"]').prop('disabled', true);
}

function resetFaceToFace() {
    $("[name='DATE_FACE_TO_FACE']").val($("[name='DATE_FACE_TO_FACE']").attr("data-last-value"));
    $('[aria-labelledby="id-TIME_FACE_TO_FACE"]').FirstorDefault();
    $('[data-parent="CHECK_FACE_TO_FACE"]').addClass('hide');
    $('[name="CHECK_FACE_TO_FACE"]').prop('checked', false);
    $('[name="DATE_FACE_TO_FACE"]').prop('data-parsley-required', true);
    $('[name="TIME_FACE_TO_FACE"]').prop('data-parsley-required', true);
    $('[name="DATE_FACE_TO_FACE"]').attr('data-parsley-required', true);
    $('[name="TIME_FACE_TO_FACE"]').attr('data-parsley-required', true);
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('data-parsley-required', true);
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').attr('data-parsley-required', true);
    $('[name="DATE_FACE_TO_FACE"]').prop('required', 'required');
    $('[name="TIME_FACE_TO_FACE"]').prop('required', 'required');
    $('[name="CHECK_EXTEND_F2F"]').prop('disabled', false);
    $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', false);
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('required', 'required');
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").val("");
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").prop("data-parsley-required", false);
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").attr("data-parsley-required", false);
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").removeAttr('required');
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").removeClass("parsley-error");
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").closest('div').find('ul').removeClass("filled");
    $(".labelFaceToFace").removeClass("hide");
    $(".labelFaceToFace").closest("div.row").find("[data-parent]").removeClass("hide");
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

function addDaysWithWeekend(numDaysAdd, stringDate) {
    var dateMax = new Date(stringDate);
    var endDate = "";
    var finalDays = 0;

    var dataHolidays = $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-holidays');
    var unavailableDates = [];

    if (dataHolidays !== undefined && dataHolidays !== null) {
        unavailableDates = jQuery.makeArray(dataHolidays.split('|'));
    }

    if (numDaysAdd === 10) {
        if (dateMax.getDay() !== 0 || dateMax.getDay() !== 6) {
            numDaysAdd = 9;
        }
    }

    while (finalDays < numDaysAdd) {
        endDate = new Date(dateMax.setDate(dateMax.getDate() + 1));
        var formatDate = ("0" + dateMax.getDate()).slice(-2) + "-" + (("0" + (dateMax.getMonth() + 1)).slice(-2)) + "-" + dateMax.getFullYear();

        if (($.inArray(formatDate, unavailableDates) == -1) === false) {
            endDate = new Date(dateMax.setDate(dateMax.getDate() + 1));
        }

        if (endDate.getDay() !== 0 && endDate.getDay() !== 6) {
            finalDays++;
        }
    }
    return endDate;
}

function selectFaceToFace() {
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").prop('data-parsley-required', true);
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").attr('data-parsley-required', true);
    $("[name='COMMENT_CHANGE_VIRTUAL_MEETING']").prop('required', 'required');
    $('[data-parent="CHECK_FACE_TO_FACE"]').removeClass('hide');
    $('[name=COMMENT_CHANGE_VIRTUAL_MEETING]').val("");

    $('[name=CHECK_EXTEND_F2F]').prop("checked", false);
    $('[name="CHECK_EXTEND_F2F"]').prop('disabled', true);
    $("[name='COMMENT_EXTEND_F2F']").prop('data-parsley-required', false);
    $("[name='COMMENT_EXTEND_F2F']").attr('data-parsley-required', false);
    $("[name='COMMENT_EXTEND_F2F']").removeAttr('required');
    $("[name='COMMENT_EXTEND_F2F']").removeClass("parsley-error");
    $("[name='COMMENT_EXTEND_F2F']").closest('div').find('ul').removeClass("filled");
    $('[data-parent="CHECK_EXTEND_F2F"]').addClass('hide');
    $('[name=COMMENT_EXTEND_F2F]').val("");

    $('[name=CHECK_CHANGE_DURATION]').prop("checked", false);
    $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', true);
    $("[name='COMMENT_CHANGE_DURATION']").prop('data-parsley-required', false);
    $("[name='COMMENT_CHANGE_DURATION']").attr('data-parsley-required', false);
    $("[name='COMMENT_CHANGE_DURATION']").removeAttr('required');
    $("[name='COMMENT_CHANGE_DURATION']").removeClass("parsley-error");
    $("[name='COMMENT_CHANGE_DURATION']").closest('div').find('ul').removeClass("filled");
    $('[data-parent="CHECK_CHANGE_DURATION"]').addClass('hide');
    $('[name=COMMENT_CHANGE_DURATION]').val("");

    $('[aria-labelledby="id-TIME_FACE_TO_FACE"]').FirstorDefault();
    $("[name='DATE_FACE_TO_FACE']").prop('data-parsley-required', false);
    $("[name='DATE_FACE_TO_FACE']").attr('data-parsley-required', false);
    $("[name='DATE_FACE_TO_FACE']").removeAttr('required');
    $("[name='DATE_FACE_TO_FACE']").removeClass("parsley-error");
    $("[name='DATE_FACE_TO_FACE']").closest('div').find('ul').removeClass("filled");
    $("[name='TIME_FACE_TO_FACE']").prop('data-parsley-required', false);
    $("[name='TIME_FACE_TO_FACE']").attr('data-parsley-required', false);
    $("[name='TIME_FACE_TO_FACE']").removeClass("parsley-error");
    $("[name='TIME_FACE_TO_FACE']").removeAttr('required');
    $("[name='TIME_FACE_TO_FACE']").closest('div').find('ul').removeClass("filled");
    $('[name="TIME_FACE_TO_FACE"]').prop('disabled', false);
    $('[id="id-TIME_FACE_TO_FACE"]').prop('disabled', false);
    $('[name="DATE_FACE_TO_FACE"]').prop('disabled', false);
    $(".labelFaceToFace").addClass("hide");
    $(".labelFaceToFace").closest("div.row").find("[data-parent]").addClass("hide");
    $('[name="CHECK_CHANGE_DURATION"]').prop('disabled', true);

    $("[name='MEETING_ROOM_FACE_TO_FACE']").val("");
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').removeAttr('required');
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').removeAttr('disabled');
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').removeAttr('readonly');
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').removeClass("parsley-error");
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').closest('div').find('ul').removeClass("filled");
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').prop('data-parsley-required', false);
    $('[name="MEETING_ROOM_FACE_TO_FACE"]').attr('data-parsley-required', false);

    $('input[name="DATE_FACE_TO_FACE"]').attr('dd-max-date', $('input[name="DATE_FACE_TO_FACE"]').attr('data-max-date-F2F'));
    $('input[name="END_DATE_PROCESS_FLOW"]').attr('dd-max-date', $('input[name="END_DATE_PROCESS_FLOW"]').attr('data-max-date-circulation'));
}

function removeModal() {
    $('.vex-open .information').each(function (i) {
        var element = $(this);
        if (i > 0) {
            element[0].parentNode.removeChild(element[0]);
        }
    });
}
