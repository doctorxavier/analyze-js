

$(window).load(function () {
   $("input[type='checkbox']").each(function () {
      var caption = $(this).data("caption");
      $(this).closest("label").children("span").text(caption);
      $(this).closest("label").children("span").width("500px");
   });
   if (!$('#IsAutoCalcPhysycalEop').is(":checked")) {
      $("#MileStoneBasedPmiContainer > label > span").addClass("textDeactivated");
      $("#MileStoneBasedPmiContainer > label > span").removeClass("checked");
      $("#MileStoneBasedPmi").attr('checked', false);
      $("#MileStoneBasedPmi").prop('disabled', true);

   }
});

$(document).ready(function () {

   var isOutputDeactivated = $('#IsDeactivated').is(":checked");
   // All editable fields are enabled only in the case when IsDeactivated is “false”.
   if (isOutputDeactivated) {
      $(":input[id!='IsDeactivated'][type!='submit'][type!='hidden'][type!='button']").prop('disabled', true);
   }

   $(".optionSelect").kendoDropDownList(
      {
          enable: !isOutputDeactivated,
      });

   $('#IsAutoCalcPhysycalEop').change(function () {
      if ($(this).is(":checked")) {
         $("#MileStoneBasedPmiContainer > label > span").removeClass("textDeactivated");
         $("#MileStoneBasedPmi").prop('disabled', false);
      }
      else {
         $("#MileStoneBasedPmiContainer > label > span").addClass("textDeactivated");
         $("#MileStoneBasedPmiContainer > label > span").removeClass("checked");
         //$("#MileStoneBasedPmi").attr('checked', 'checked');
         $("#MileStoneBasedPmi").attr('checked', false);
         $("#MileStoneBasedPmi").prop('disabled', true);


      }
   });

   $('#IsProgender').change(function () {
      if ($(this).is(":checked")) {
         $("#ProGenderWarningMsg").removeClass("hidden");
      }
      else {
         $("#ProGenderWarningMsg").addClass("hidden");
      }
   });

   $('#IsProethnicity').change(function () {
      if ($(this).is(":checked")) {
         $("#ProEthnicityWarningMsg").removeClass("hidden");
      }
      else {
         $("#ProEthnicityWarningMsg").addClass("hidden");
      }
   });

   $("#OutputMeansVerification").val($("#MeansOfVerification").val());
   $("#OutputBaseline").val($("#Baseline").val());
   $("#OutputBaselineYear").val($("#BaselineYear").val());

    $(".save").click(function () {
        var $this = $(this);
        var usedForStrategicAlignment = $this.attr('data-sa-checkindicators-usedforsa');
        var placeHolder = $this.attr('data-sa-checkindicators-placeholder');
        var checkSARmIndicatorsUrl = $this.attr('data-sa-checkindicators-url');

        if (usedForStrategicAlignment.toLowerCase() === "true" &&
            placeHolder != null &&
            checkSARmIndicatorsUrl != null &&
            $("input[type=checkbox][id='IsDeactivated']").is(":checked")) {

            checkRmIndicatorRelationsToSAClassifications(placeHolder, checkSARmIndicatorsUrl, function () {
                showWarningOrSave();
            });
        } else {
            showWarningOrSave();
        }
    });

    $('input.numberInput').autoNumeric('init', { vMin: '-9999999999999999.99', vMax: '9999999999999999.99', aSep: ',', aDec: '.' });

    $('#outputsTarget').on('submit', function () {
        var frm = $(this);
        var baselineValue = frm.find('input.numberInput').val();
        frm.find('input.numberInput').val(baselineValue.replace(/,/g, ''));

        frm.submit(function () {
            $.ajax({
                type: 'POST',
                url: frm.attr('action'),
                data: frm.serialize()
            });
        });
    });

    $('.k-dropdown-wrap.k-state-default').addClass('btn');
});

function showWarningOrSave() {
    firstModalContinue = false;

    if (chekIfItHasBeenChanges()) {
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var route = $("#WarningMessageURL").val();
        var title = "Warning";
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: route,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            close: function () {
                if (!firstModalContinue || !checkInactiveVisualOutputs()) {
                    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                    $(".ui-widget-overlay").remove();
                    $(".k-window").remove();
                }
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.center();
        dialog.open();
    }
    else {
        $("#outputsTarget").submit();
    }
}

function chekIfItHasBeenChanges() {

    var interval = new Object();
    interval.IntervalId = parseInt($("#hdnIntervalId").val());
    interval.CycleId = parseInt($("#hdnCycleId").val());

    if ((interval.IntervalId != 1 && interval.IntervalId != 2) ||
        ($("#hdnAccessedByAdministrator").val() == "True" && interval.IntervalId == 3)) {

        var isAutoCalcPhysycalEop = $("input[type=checkbox][id='IsAutoCalcPhysycalEop']").is(":checked");
        var milestoneBasedPmi = $("input[type=checkbox][id='MileStoneBasedPmi']").is(":checked");
        var isDeactivated = $("input[type=checkbox][id='IsDeactivated']").is(":checked");

        if ($("#OutputMeansVerification").val() != $("#MeansOfVerification").val() ||
            $("#OutputBaseline").val() != $("#Baseline").val() ||
            $("#OutputBaselineYear").val() != $("#BaselineYear").val() ||
            isDeactivated.toString() != $("#hdnIsDeactivated").val().toLowerCase() ||
            isAutoCalcPhysycalEop.toString() != $("#hdnIsAutoCalcPhysycalEop").val().toLowerCase() ||
            milestoneBasedPmi.toString() != $("#hdnMileStoneBasedPmi").val().toLowerCase()) {
                return true;
        }
    }

    return false;
}

function checkInactiveVisualOutputs() {

    var inactiveCheckbox = $("input[type=checkbox][id='IsDeactivated']").is(":checked");
    var inactiveOriginal = JSON.parse($("#hdnIsDeactivated").val().toLowerCase());

    return inactiveCheckbox && inactiveCheckbox != inactiveOriginal && hasVisualOutputs;

}

var firstModalContinue = false;