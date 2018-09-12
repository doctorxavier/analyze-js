/// <reference path="main.js" />
$(document).ready(function () {
   $("#showInactiveOutputs").button().click(function (event) {
      var valueShowInactiveOutputs = $(this).is(':checked');
      var valueshowMilestones = $("#showMilestones").is(':checked');
      var valueshowDisaggregation = $("#showDisaggregation").is(':checked');;
      if (valueshowMilestones) {
         valueshowDisaggregation = false;
      }
      var urlRoute = $("#urlRoute").val();
       //redirectPage(urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation);
      document.location.href = urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation;
   });
   $("#showMilestones").click(function (event) {
      var valueShowInactiveOutputs = $("#showInactiveOutputs").is(':checked');
      var valueshowMilestones = $(this).is(':checked');
      var valueshowDisaggregation = $("#showDisaggregation").is(':checked');
      if (valueshowMilestones) {
         valueshowDisaggregation = false;
      }
      var urlRoute = $("#urlRoute").val();
      if ($("#showVisualizations").length != 0) {
         var valueShowVisualizations = false;
          //redirectPage(urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showVisualizations=' + valueShowVisualizations);
         document.location.href = urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showVisualizations=' + valueShowVisualizations;
      }
      else {
          //redirectPage(urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation);
          document.location.href = urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation;
      }

   });
   $("#showDisaggregation").click(function (event) {
      var valueShowInactiveOutputs = $("#showInactiveOutputs").is(':checked');
      var valueshowMilestones = $("#showMilestones").is(':checked');
      var valueshowDisaggregation = $(this).is(':checked');
      if (valueshowDisaggregation) {
         valueshowMilestones = false;
      }
      var urlRoute = $("#urlRoute").val();
       //redirectPage(urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation);
      document.location.href = urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showDisaggregation=' + valueshowDisaggregation;

   });

   $("#showVisualizations").click(function (event) {
      var valueShowVisualizations = $(this).is(':checked');
      var valueShowInactiveOutputs = $("#showInactiveOutputs").is(':checked');
      var valueshowMilestones = $("#showMilestones").is(':checked');
      if (valueShowVisualizations) {
         valueshowMilestones = false;
      }
      var urlRoute = $("#urlRoute").val();
       //redirectPage(urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showVisualizations=' + valueShowVisualizations);
      document.location.href = urlRoute + '&showInactiveOutputs=' + valueShowInactiveOutputs + '&showMilestones=' + valueshowMilestones + '&showVisualizations=' + valueShowVisualizations;

   });

   $(".editOutput").click(function (event) {
       //redirectPage($(this).data("route"));
       document.location.href = $(this).data("route");
      return false;
   });

   $(".outputDetail").click(function (event) {
       //redirectPage($(this).data("route"));
       document.location.href = $(this).data("route");
      return false;
   });

   $(".editPhysicalProgress").click(function (event) {
      var valueShowMilestones = $("#showMilestones").is(':checked');
      var valueShowDisaggregation = $("#showDisaggregation").is(':checked');
      if (valueShowMilestones || (!valueShowMilestones && !valueShowDisaggregation)) {
          //redirectPage($(this).data("route") + '&editMilestones=true&editDisaggregation=false');
          document.location.href = $(this).data("route") + '&editMilestones=true&editDisaggregation=false';
      }
      else {
          //redirectPage($(this).data("route") + '&editMilestones=false&editDisaggregation=true');
          document.location.href = $(this).data("route") + '&editMilestones=false&editDisaggregation=true';
      }
      return false;
   });

   $(".editFinancialProgress").click(function (event) {
      var valueshowMilestones = $("#showMilestones").is(':checked');
      if (valueshowMilestones) {
          //redirectPage($(this).data("route") + '&editMilestones=' + true);
          document.location.href = $(this).data("route") + '&editMilestones=' + true;
      }
      else {
          //redirectPage($(this).data("route"));
          document.location.href = $(this).data("route");
      }
      return false;
   });

   $("#PMRCycleAdmin").click(function (event) {
       //redirectPage($(this).data("route"));
       document.location.href = $(this).data("route");
   });

});