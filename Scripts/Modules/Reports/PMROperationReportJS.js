$(document).on("ready", function () {

   $("#FormPMROperationReport").on("submit", function (e) {
   });

   $("#SelectorFinancialDataAggregatedList").on("change", function () {
      var valComboFinancial = $("#SelectorFinancialDataAggregatedList option:selected").attr("value");
      var IsFinancialDataAggregated = null;
      if (valComboFinancial == "true") {
         IsFinancialDataAggregated = true;
      }
      else if (valComboFinancial == "false") {
         IsFinancialDataAggregated = false;
      }

      $("#FinancialDataAggregation").attr("value", IsFinancialDataAggregated);

   });

   $("#SelectorTypeOfReportList").on("change", function () {
      var TypeReport = $("#SelectorTypeOfReportList option:selected").val();

      if (TypeReport == "Custom") {
         $(".ItemTableReportOperation").prop("checked", false);
         $(".ItemTableReportOperation").removeAttr("disabled");

         $("#ListPMRCycleID").removeAttr("disabled");
         $("#ListPMRCycleID option:selected").removeAttr("selected");

         $('#tableUIRT011 thead tr label').css("background-position", "0 0px");
         $('#tableUIRT011 tbody tr label').css("background-position", "0 0px");


         $('.specialCheckCustom').css("background-position", "0 0px");
         $(".specialCheckCustomReal").prop("checked", false);
         $(".specialCheckCustomReal").removeAttr("disabled");
        

      }
      else if (TypeReport == "Complete") {
         $(".ItemTableReportOperation").prop("checked", true);
         $(".ItemTableReportOperation").attr("disabled", "disabled");

         $("#ListPMRCycleID").removeAttr("disabled");
         $("#ListPMRCycleID option:selected").removeAttr("selected");
              

         $('#tableUIRT011 thead tr label').css("background-position", "0 -20px");
         $('#tableUIRT011 tbody tr label').css("background-position", "0 -20px");

         $('.specialCheckCustom').css("background-position", "0 0px");
         $(".specialCheckCustomReal").prop("checked", false);
         $(".specialCheckCustomReal").attr("disabled", "disabled");
         

      }
      else if (TypeReport == "Results Matrix") {
         $('#tableUIRT011 thead tr label').css("background-position", "0 0px");
         $('#tableUIRT011 tbody tr label').css("background-position", "0 0px");
         $(".ItemTableReportOperation").prop("checked", false);

         $('.LabelItemReportResultMatrix').css("background-position", "0 -20px");
         $(".ItemReportResultMatrix").prop("checked", true);
         $(".ItemTableReportOperation").attr("disabled", "disabled");

         $('.specialCheckCustom').css("background-position", "0 0px");
         $(".specialCheckCustomReal").prop("checked", false);
         $(".specialCheckCustomReal").attr("disabled", "disabled");

          /**
            * Jira ID: CON-498
            * Fix Description: Report  de “results matrix”. 
            * Attended by: Angelica Singh
            * Date: 09/02/2015
            * Begin Fix
            **/                  
               
         $("#ListPMRCycleID").find('option').removeAttr("selected");
         $("#ListPMRCycleID").attr("disabled", "disabled");
         $("#ListPMRCycleID option:first").attr("selected", "selected");
          /* End Fix */

      }

   });

   $("#ButtonDownloadReportPMROperation").on("click", function () {
       $(".ItemTableReportOperation").removeAttr("disabled");
       
       var typeReport = $("#SelectorTypeOfReportList").val();
      
       if (typeReport == "Results Matrix") {
           $("#ListPMRCycleID").removeAttr("disabled");
           $('#FormPMROperationReport').submit();
           $("#ListPMRCycleID").attr("disabled", "disabled");
       }
       else
       {
           $('#FormPMROperationReport').submit();
       }

   });

   $(".ItemTableReportOperation").prop("checked", true);
   $('#tableUIRT011 thead tr label').css("background-position", "0 -20px");
   $('#tableUIRT011 tbody tr label').css("background-position", "0 -20px");

   $('.specialCheckCustom').css("background-position", "0 0px");


   $("#LabelBasicDataCheck").on('click', function () {
      var state = $("#BasicDataCheck").attr('disabled');

      if (state == null) {
         if ($('#BasicDataCheck').is(":checked")) {
            $('.LabelItemBasicDataCheck').css("background-position", "0 0px");
         }
         else {
            $('.LabelItemBasicDataCheck').css("background-position", "0 -20px");
         }
      }

   });

   $("#LabelresultMatrixCheck").on('click', function () {

      var state = $("#resultMatrixCheck").attr('disabled');

      if (state == null) {
         if ($('#resultMatrixCheck').is(":checked")) {
            $('.LabelItemResultMatrixCheck').css("background-position", "0 0px");
         }
         else {
            $('.LabelItemResultMatrixCheck').css("background-position", "0 -20px");
         }
      }
   });

   $("#LabelReportParts_FindingRecommendation_").on('click', function () {

      var state = $("#ReportParts_FindingRecommendation_").attr('disabled');

      if (state == null) {

         if ($('#ReportParts_FindingRecommendation_').is(":checked")) {
            $('.LabelItemFindingCheck').css("background-position", "0 0px");
         }
         else {
            $('.LabelItemFindingCheck').css("background-position", "0 -20px");
         }
      }
   });

   $("#Btn_Clear").on("click", function () {
      $(".MultiSelectCustom option").each(function (index, element) {
         $(element).removeAttr("selected");
      });
   });

});

function ReestablecerValores() {
   $(".ItemTableReportOperation").attr("disabled", "disabled");
}

function changeCheckboxSelected(element, check) {

   var state = $(element).attr('disabled');

   if (state == null) {
      if ($(element).is(":checked")) {
         $(check).css("background-position", "0 0px");
      }
      else {
         $(check).css("background-position", "0 -20px");
      }
   }

}

function changeRadioSelected(original, target) {

   $(original).css("background-image", "url('../../Images/controls/radioButtonInactive.png')");
   $(target).css("background-image", "url('../../Images/controls/radioButtonActive.png')");
}




