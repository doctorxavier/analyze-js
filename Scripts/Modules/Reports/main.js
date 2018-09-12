/**
 * Reports Module
 * Thu, 17 december 2013
 * eSmart Group
 */

//remap jQuery to $
(function ($) {

   $(document).ready(function () {

      $(".datepicker").kendoDatePicker({
         format: "MM/dd/yyyy"
      });
      //KendoDropDownList
      $('.kendoDropDown').kendoDropDownList();

      //KendoDatePicker
      $('.kendoDatePicker').kendoDatePicker();

      //Multiple checkbox selector function
      checkboxSelector();
      checkboxSelectorPortfolio();

      //Filter Box Reports 011
      $('#showFilterButtonReports011').on('mousedown', function () {
         $('#searchBoxContainerReports011').toggle();
      });

      //Filter Box Reports 013
      $('#showFilterButtonReports013').on('mousedown', function () {
         $('#searchBoxContainerReports013').toggle();
      });

      //Filter Box Reports CL_001
      $('#showFilterButtonReportsCL001').on('mousedown', function () {
          $('#searchBoxContainerReportsCL001').toggle();
          if ($("#searchBoxContainerReportsCL001").is(":visible")) {
              $(".ReportContainer").hide();
          } else {
              $(".ReportContainer").show();
          }
      });

      //Filter Box Reports CL_002
      $('#showFilterButtonReportsCL002').on('mousedown', function () {
         $('#searchBoxContainerReportsCL002').toggle();
      });

      //Filter Box Reports CO_001
      $('#showFilterButtonReportsCO001').on('mousedown', function () {
         $('#searchBoxContainerReportsCO001').toggle();
      });

      //Filter Box Reports DS_001
      $('#showFilterButtonReportsDS001').on('mousedown', function () {
         $('#searchBoxContainerReportsDS001').toggle();
      });

      //Filter Box Reports DS_002
      $('#showFilterButtonReportsDS002').on('mousedown', function () {
         $('#searchBoxContainerReportsDS002').toggle();
      });

      //Filter Box Reports DS_002
      $('#showFilterButtonReportsRS001').on('mousedown', function () {
         $('#searchBoxContainerReportsRS001').toggle();
      });

      //Filter Box Reports operation_001
      $('#showFilterButtonReportsOPERATION001').on('mousedown', function () {
         $('#searchBoxContainerReportsOPERATION001').toggle();
      });

      //Filter Box Reports portfolio_001
      $('#showFilterButtonReportsPORTFOLIO001').on('mousedown', function () {
         $('#searchBoxContainerReportsPORTFOLIO001').toggle();
      });
   });

   function checkboxSelector() {

      $("#tableUIRT011 thead input[type='checkbox'], #tablePORTFOLIOUIRT001 thead input[type='checkbox']").click(function () {
         var selector = $(this).closest('th').index() + 1;
         if ($(this).prop("checked") === true) {
            $("#tableUIRT011 tr td:nth-child(" + selector + ") input[type='checkbox']").prop("checked", true);
         } else {
            $("#tableUIRT011 tr td:nth-child(" + selector + ") input[type='checkbox']").prop("checked", false);
         }
      });
   }

   function checkboxSelectorPortfolio() {

      $("#tablePORTFOLIOUIRT001 thead input[type='checkbox']").click(function () {
         var selector = $(this).closest('th').index() + 1;
         if ($(this).prop("checked") === true) {
            $("#tablePORTFOLIOUIRT001 tr td:nth-child(" + selector + ") input[type='checkbox']").prop("checked", true);
         } else {
            $("#tablePORTFOLIOUIRT001 tr td:nth-child(" + selector + ") input[type='checkbox']").prop("checked", false);
         }
      });
   }


})(window.jQuery);