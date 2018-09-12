$(document).ready(function () {
   $("#radio2").click(function () {

      var showMilestone = false;
      var positionRelative = $(this).offset();
      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

      var dialog = $(".dinamicModal").kendoWindow({
         width: "800px",
         title: $(this).data("title"),
         draggable: false,
         resizable: false,
         content: $(this).data("route") + "?showMilestone=" + showMilestone,
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
            $('#radio2').next().removeClass('ui-state-active');
            $('#radio1').next().addClass('ui-state-active');
         }
      }).data("kendoWindow");
      dialog.open();
   });
});

function deleteMilestone(element) {
   if ($(element).closest("tr.hoverMilestone").find("> td:eq(0) > input.MilestoneId").length == 0) {
      var milestonesElement = $(element).closest("table.milestoneTable").first();
      $(element).closest("tr.hoverMilestone").remove();
      
   } else {
      var positionRelative = $(element).offset();
      var operationNumber = $("#Operation_OperationNumber").val();
      var milestoneId = $(element).closest("tr.hoverMilestone").find("> td:eq(0) > input").val();

      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

      var dialog = $(".dinamicModal").kendoWindow({
         width: "800px",
         title: $("#IndexDeleteMilestoneWarning").data("title"),
         draggable: false,
         resizable: false,
         content: $("#IndexDeleteMilestoneWarning").data("route") + "?MilestoneId=" + milestoneId + "&OperationNumber=" + operationNumber,
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
}