$(window).load(function () {
   $("input[type='checkbox']").each(function () {
      var caption = $(this).data("caption");
      $(this).closest("label").children("span").text(caption);
      $(this).closest("label").children("span").width("500px");
   });
});


$(document).ready(function () {
   //is it in edit mode
   var IsEditMilestone = ($('#IsEditMilestone').val().toLowerCase() == 'true');

   $(".optionSelect").kendoDropDownList(
      {
         enable: IsEditMilestone
      });

   //check values changes in milestones values 

   $("#MilestoneMeansVerification").val($("#MeansOfVerification").val());
   $("#MilestoneBaseline").val($("#Baseline").val());
   $("#MilestoneBaselineYear").val($("#BaselineYear").val());


   $(".btnEditMilestone").click(function (event) {
      redirectPage($(this).data("route"));
      return false;
   });



   $(".save").click(function () {

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
      else {
         $("#milestonesTarget").submit();
      }
   });

   $('input.numberInput').autoNumeric('init', { vMin: '-999999999.99', vMax: '999999999.99', aSep: ',', aDec: '.' });
});


function chekIfItHasBeenChanges() {
   var registerChangeMatrix = false;

   //Set Interval Object 
   var interval = new Object();
   interval.IntervalId = parseInt($("#hdnIntervalId").val());
   interval.CycleId = parseInt($("#hdnCycleId").val());
   if (interval.IntervalId == 3) {
      if ($("#MilestoneMeansVerification").val() != $("#MeansOfVerification").val() ||
          $("#MilestoneBaseline").val() != $("#Baseline").val() ||
          $("#MilestoneBaselineYear").val() != $("#BaselineYear").val()) {
         registerChangeMatrix = true;
      }
      if ($("#IsAutoCalcPhysycalEop").prop("checked").toString().toLowerCase() != $("#IsAutoCalcPhysycalEop").data("originalvalue").toLowerCase()) {
         registerChangeMatrix = true;
      }

   }
   return registerChangeMatrix;
}