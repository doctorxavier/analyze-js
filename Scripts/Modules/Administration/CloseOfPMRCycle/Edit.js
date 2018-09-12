/**
 * Administration Module
 */
/**
 * Searches Module
 * Thu, 17 december 2013
 * eSmart Group
 */

//remap jQuery to $




$(document).ready(function () {

   $("#btnCloseCycle").click(function () {

      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      var title = "Warning";
      var dialog = $("#closeWarning").kendoWindow({
         width: "800px",
         title: title,
         draggable: false,
         resizable: false,
         pinned: true,
         actions: [
             "Close"
         ],
         modal: true,
         visible: false,
         close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
         }
      }).data("kendoWindow");
      $(".k-window-titlebar").addClass("warning");
      $(".k-window-title").addClass("ico_warning");
      dialog.center();
      dialog.open();
   });

   $("#btnOpenCycle").click(function () {

      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      var title = "Warning";
      var dialog = $("#openWarning").kendoWindow({
         width: "800px",
         title: title,
         draggable: false,
         resizable: false,
         pinned: true,
         actions: [
             "Close"
         ],
         modal: true,
         visible: false,
         close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
         }
      }).data("kendoWindow");
      $(".k-window-titlebar").addClass("warning");
      $(".k-window-title").addClass("ico_warning");
      dialog.center();
      dialog.open();
   });

   $("#lnkCancelCloseCycle").click(function () {
      $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
      $(".ui-widget-overlay").remove();
      $("#closeWarning").data("kendoWindow").close();
   });

   $("#lnkCancelOpenCycle").click(function () {
      $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
      $(".ui-widget-overlay").remove();
      $("#openWarning").data("kendoWindow").close();
   });

   $("#btnConfirmCloseCycle").click(function () {

      $(this).prop('disabled', true);
      $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
      $(".ui-widget-overlay").remove();
      $("#CloseCycleForm:first").submit();

   });

   $("#btnConfirmOpenCycle").click(function () {
      $(this).prop('disabled', true);
      $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
      $(".ui-widget-overlay").remove();
      $("#openWarning").data("kendoWindow").close();
      idbg.lockUi($("#mainPMRContent"), true);

      setTimeout(function () {
         $("#OpenCycleForm:first").submit();
      }, 2000);

   });


   $.ajax({
      url: $("#SearchUrl").data("search-path"),
      data: JSON.stringify({ 'Page': 1 }),
      dataType: "json",
      contentType: "application/json",
      type: 'POST',
      success: function (data) {
         // Update PagingFooter Content
         updateGridRowsContent("PMRCycleCloseEdit-template", "grdPmrCycle", data.PmrCycles);

         // Update PagingFooter Content
         updatePagingFooter(data.PageSettings);

         inputsDatepickerSetup();
      }
   });

});


$(document).on("click", "a.gridPageNumber", function (e) {
   e.preventDefault();
   var filter = {
      'Page': $(this).find("span").text()
   };

   $.ajax({
      url: $(this).attr("href"),
      data: JSON.stringify(filter),
      dataType: "json",
      contentType: "application/json",
      type: 'post',
      success: function (data) {

         // Update PagingFooter Content
         updateGridRowsContent("PMRCycleCloseEdit-template", "grdPmrCycle", data.PmrCycles);

         // Update PagingFooter Content
         updatePagingFooter(data.PageSettings);
         inputsDatepickerSetup();
      }
   });

});

function inputsDatepickerSetup() {
   $("#CycleClosingDateVisual").kendoDatePicker({
      change: function () {
         var value = new Date(this.value());
         $("#CycleClosingDate").val(value.toJSON());

      }
   });

   var datepicker = $("#CycleClosingDateVisual").data("kendoDatePicker");
   if (datepicker) {
      datepicker.value(new Date($("#CycleClosingDate").val()));
   }

}


