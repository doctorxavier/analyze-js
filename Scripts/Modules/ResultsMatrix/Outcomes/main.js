var changesCount = 0;

$(document).ready(function () {

    $('[data-isautocalculated="True"]').removeProp('disabled').prop('readonly', true);
    $('.disagChildTable').on('change', 'input.auto', function () {
        autoCalculatedDisaggregation($(this));
    });

   $(".deleteYear").hide();

   var showText = "Show Disaggregation";
   var Hidetext = "Hide Disaggregation";
   var outcomeErrorFieldMessage = $("#msgOutcomeField").val();

   // Set metadata to active autonumeric format
   setAutonumericFormat($("#impactsContainer"));

   // Activate onchange event to validaate changes in existing elements 
   $(".existingStatement, .existingDefinition, .existingUnitOfMeasure, .existingBaseline, .existingAnnualPlan, .existingOriginalPlan , .existingBaseLineYear").change(function (e) {

      var newValue = "";
      var oldValue = "";

      if ($(e.target).prop("tagName").toUpperCase() == "SELECT") {
         // Get current value
         newValue = $(this).val();
         // Get old value
         oldValue = $(this).parent().parent().parent().next().val();

         // verify if a change has occurred over current element
         if (newValue != oldValue) {
            changesCount++;
         } else {
            //changesCount--;
         }
      } else {

         //special case EOP
         if ($(this).hasClass("existingAnnualPlan")) {

            if ($(this).hasClass("HasAnnualPreviousValue")) {
               // Get current value
               newValue = $(this).val();
               // Get old value
               oldValue = $(this).next().val();
            }
            else {
               return;
            }
         }
         else {
            // Get current value
            newValue = $(this).val();
            // Get old value
            oldValue = $(this).next().val();
         }

         // verify if a change has occurred over current element
         if (newValue != oldValue) {
            changesCount++;
         }
      }
   });

   $("#lnkShowDisaggregation").click(function () {
      event.preventDefault();

      var optionSelected = $(this).text();

      var expandDisaggregation = false;


      switch (optionSelected) {
         case showText: expandDisaggregation = true; break;
         case Hidetext: expandDisaggregation = false; break;
      }

      if (expandDisaggregation == true) {
         $("#filter").val("1");
         $(this).text(Hidetext);

         resizeOperatorBarExpand();
         $(".minimizeTable .child").slideDown();

      }

      if (expandDisaggregation == false) {
         $("#filter").val("0");
         $(this).text(showText);
         resizeOperatorBarCollapse();
         $(".minimizeTable .child").slideUp();
      }
   });

   $("#lnkEditOutcome").click(function () {
      event.preventDefault();
      location.href = $(this).attr("href") + "&filter=" + $("#filter").val();
   });

   $("#lnkEditOutcome2").click(function () {
      event.preventDefault();
      location.href = $(this).attr("href") + "&filter=" + $("#filter").val();
   });

   $('.showComments').click(function () {
      $(this).closest('.plegableHeader').find('.detailsExpanded').slideDown();
      $(this).closest('.detailsMin').hide();

   });

   $('.hideComments').click(function () {
      $(this).closest('.detailsExpanded').slideUp('fast', function () {
         $(this).closest('.plegableHeader').find('.detailsMin').show();
      });

   });

   $("#impactsContainer").on("mouseenter", ".selectCustom", function () {
      $(this).closest("tr").find(".deleteYear").show();
   });

   $("#impactsContainer").on("mouseleave", ".btn_azul_oscuro", function () {
      $(this).find(".deleteYear").hide();
   });

   $("textarea").focusout(function () {
      $(this).text($(this).val());
   });

   // Validate changes pending for storing in the matrix
   $(".btnSave").click(function (e) {

      e.preventDefault();
      validateHiddenDissagregations();

      if (findNewOutcomeOrImpactSection()) {
          if ((parseInt($("#hdnIntervalId").val()) == 3 && changesCount > 0) || ($("#AccessedByAdministrator").val() == "True" && $("#IsThirdInterval").val() == "True" && changesCount > 0)) {
              showSaveChangesWindow();
          } else {
              SaveChanges();
          }
      } else {
          Confirm.ShowWarning(outcomeErrorFieldMessage)
              .done(function(response) {
                  $(".ui-front").filter(":first").end().remove();
                  $(".loading-overlay, .loading-container").remove();
              });
      }
   });
});


/**************************************************************************************/
/***************************** GENERAL FUNCTIONS **************************************/
/**************************************************************************************/
function SaveChanges() {
   validateHiddenDissagregations();
   $(".auto").each(function () {
      $(this).val($(this).val().replace(/,/g, ''));
   });
   showLoaderOptional();
   $("#outcomeEditForm").submit();
}

function setAutonumericFormat(container) {
   // Set metadata to init autonumeric format plugin
   $(container).find('input[type="text"][id$="OriginalPlan"], input[type="text"][id$="AnnualPlan"], input[type="text"][id$="ActualValue"]').each(function (index, element) {
      $(this).attr("data-a-dec", ".");
      $(this).attr("data-a-sep", ",");
      $(this).attr("data-v-min", "-9999999999999999.99");
      $(this).attr("data-v-max", "9999999999999999.99");
      $(this).addClass("auto");
   });

   // Set metadata to init autonumeric format plugin
   $(container).find('input[type="text"][id$="Baseline"]').each(function (index, element) {
      $(this).attr("data-a-dec", ".");
      $(this).attr("data-a-sep", ",");
      $(this).attr("data-v-min", "-9999999999999999.99");
      $(this).attr("data-v-max", "9999999999999999.99");
      $(this).addClass("auto");
   });

   // Init Autonumeric plugin
   $('.auto').autoNumeric('init');

}

function showSaveChangesWindow(changesCount) {
   // Get url to display modal window
   var urlRoute = $("#hdnSaveChangesUrl").val() + "?changesCount=" + changesCount;


   // Display modal window to remove impact from the server side
   $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
   var title = "Warning";
   var dialog = $(".dinamicModal").kendoWindow({
      width: "800px",
      title: title,
      draggable: false,
      resizable: false,
      content: urlRoute,
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
   dialog.open();
}

function validateHiddenDissagregations() {
   $("tbody.disagChild").each(function (index, tbody) {

      var rows = $(this).find("tr.nivel01");
      var fail = false;

      if (tbody != null) {
         $(tbody).find("tr.nivel01").each(function (index, row) {
            var fail = false;
            $(row).find("textarea").each(function (index, textarea) {

               if ($(this).text().length <= 0) {
                  fail = true;
                  return false;
               }
            });

            if (fail) {
               var gridHeight = $(row).closest(".tableGrid").height();
               $(row).closest(".tableGrid").show();
               $(row).closest(".tableGrid").prev().prev().find("div").attr("class", "tableOperator");
               $(row).closest(".tableGrid").prev().height(gridHeight);
            }
         });
      }
   });
}

function showActionBar(disaggregationRow) {
   $(disaggregationRow).find(".actionBarDisagg").first().show();
}

function hideActionBar(disaggregationRow) {
   $(disaggregationRow).find(".actionBarDisagg").first().hide();
}

function deletedYearIsValid(year) {
   var isValid = true;
   $("#impactsContainer").find('input[type="hidden"][id$="__Year"][value="' + year + '"]').each(function (index, item) {
      console.log(item);
      if ($(item).next().val() > 0 && $(item).val() == year) {
         isValid = false;
         return false;
      }
   });

   return isValid;
}

function moveIndicator(btnMoveIndicator) {
   // Get current row
   var indicatorRow = $(btnMoveIndicator).closest("tr.nivel01").first();
   // Get dissagregations of current row
   var disaggRow = $(indicatorRow).nextUntil('tr.nivel01').filter('tr.nivel02');
   // Get dissagregation row childs
   var dissagRowChilds = $(disaggRow).find("tbody:first > tr");
   // Get tbody of current row
   var tbody = $(indicatorRow).closest("tbody.indicatorParent");
   // Get order number field of current indicator
   var orderNumberField = $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]');
   // Get operatorBar of the current table
   var inidcatorTable = $(tbody).parent("table.grid");
   // Check if current row has dissagregations
   if (dissagRowChilds.length > 0 && $(disaggRow).attr("class") == "nivel02") {

      if ($(btnMoveIndicator).is(".moveUp")) {

         // Check if order number of current indicator
         if (parseInt($(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').val()) > 0) {

            // Get current track order
            var currentTrackOrder = $(indicatorRow).prevAll('tr.trackOrderRow').first();
            var previoustrackOrder = $(indicatorRow).prevAll("tr.nivel01").first().prevAll('tr.trackOrderRow').first();
            var currentTrackOrderText = $(currentTrackOrder).find("td").html();
            var previoustrackOrderText = $(previoustrackOrder).find("td").html();

            // Insert current indicator in new position
            $(indicatorRow).insertBefore(indicatorRow.prevAll("tr.nivel01").first());

            $(currentTrackOrder).insertBefore(indicatorRow);
            $(currentTrackOrder).find("td").html(previoustrackOrderText);

            disaggRow.insertAfter(indicatorRow);

            // Update order number field of moved indicator
            var currentIndicatorRows = $(indicatorRow).closest("tbody.indicatorParent").first().find("tr.nivel01.rowIndicator");
            var newCurrentIndicatorPosition = $(currentIndicatorRows).index(indicatorRow);
            $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", newCurrentIndicatorPosition);

            // Update order number of previous indicator
            var nextRow = indicatorRow.nextAll("tr.nivel01").first();

            $(previoustrackOrder).insertBefore(nextRow);
            $(previoustrackOrder).find("td").html(currentTrackOrderText);

            var previousIndicatorPosition = $(currentIndicatorRows).index(nextRow);
            $(nextRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", previousIndicatorPosition);


         }

      } else {
         if (parseInt($(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').val()) != ($(tbody).find("tr.nivel01.rowIndicator").length - 1)) {

            // Get current track order
            var currentTrackOrder = $(indicatorRow).prevAll('tr.trackOrderRow').first();
            var nextTrackOrder = $(indicatorRow).nextAll("tr.nivel01").first().prevAll('tr.trackOrderRow').first();
            var currentTrackOrderText = $(currentTrackOrder).find("td").html();
            var nextTrackOrderText = $(nextTrackOrder).find("td").html();

            var nextRow = indicatorRow.nextAll("tr.nivel01").first();
            var prevRow = nextRow.nextUntil('tr.nivel01').filter('tr.nivel02').last();
            indicatorRow.insertAfter(prevRow);

            $(currentTrackOrder).find("td").html(nextTrackOrderText);
            $(currentTrackOrder).insertBefore(indicatorRow);

            disaggRow.insertAfter(indicatorRow);

            // Update order number field of moved indicator
            var currentIndicatorRows = $(indicatorRow).closest("tbody.indicatorParent").first().find("tr.nivel01.rowIndicator");
            var newCurrentIndicatorPosition = $(currentIndicatorRows).index(indicatorRow);
            $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", newCurrentIndicatorPosition);

            // Update order number of previous indicator
            var nextRow = indicatorRow.prevAll("tr.nivel01").first();

            $(nextTrackOrder).insertBefore(nextRow);
            $(nextTrackOrder).find("td").html(currentTrackOrderText);

            var previousIndicatorPosition = $(currentIndicatorRows).index(nextRow);
            $(nextRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", previousIndicatorPosition);

         }


      }
   } else {
      if ($(btnMoveIndicator).is(".moveUp")) {

         if (parseInt($(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').val()) > 0) {
            // Get current track order
            var currentTrackOrder = $(indicatorRow).prevAll('tr.trackOrderRow').first();
            var previoustrackOrder = $(indicatorRow).prevAll("tr.nivel01").first().prevAll('tr.trackOrderRow').first();
            var currentTrackOrderText = $(currentTrackOrder).find("td").html();
            var previoustrackOrderText = $(previoustrackOrder).find("td").html();

            $(indicatorRow).insertBefore(indicatorRow.prevAll("tr.nivel01").first());
            $(currentTrackOrder).find("td").html(previoustrackOrderText);
            $(currentTrackOrder).insertBefore(indicatorRow);


            // Update order number field of moved indicator
            var currentIndicatorRows = $(indicatorRow).closest("tbody.indicatorParent").first().find("tr.nivel01.rowIndicator");
            var newCurrentIndicatorPosition = $(currentIndicatorRows).index(indicatorRow);
            $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", newCurrentIndicatorPosition);

            // Update order number of previous indicator
            var nextRow = indicatorRow.nextAll("tr.nivel01").first();
            $(previoustrackOrder).insertBefore(nextRow);
            $(previoustrackOrder).find("td").html(currentTrackOrderText);

            var previousIndicatorPosition = $(currentIndicatorRows).index(nextRow);
            $(nextRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", previousIndicatorPosition);

         }
      } else {

         // Find next tr.nivel01
         var rowNivel1 = indicatorRow.nextAll("tr.nivel01").first();

         if (rowNivel1 != null) {
            if (parseInt($(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').val()) != ($(tbody).find("tr.nivel01.rowIndicator").length - 1)) {

               // Check if next tr.nivel01 has dissagregations
               var disaggRow = $(rowNivel1).nextUntil('tr.nivel01').filter('tr.nivel02');

               if (disaggRow != null && disaggRow.length > 0) {

                  // Get current track order
                  var currentTrackOrder = $(indicatorRow).prevAll('tr.trackOrderRow').first();
                  var nextTrackOrder = $(indicatorRow).nextAll("tr.nivel01").first().prevAll('tr.trackOrderRow').first();
                  var currentTrackOrderText = $(currentTrackOrder).find("td").html();
                  var nextTrackOrderText = $(nextTrackOrder).find("td").html();

                  indicatorRow.insertAfter(disaggRow.last());
                  $(currentTrackOrder).find("td").html(nextTrackOrderText);
                  $(currentTrackOrder).insertBefore(indicatorRow);

                  // Update order number field of moved indicator
                  var currentIndicatorRows = $(indicatorRow).closest("tbody.indicatorParent").first().find("tr.nivel01.rowIndicator");
                  var newCurrentIndicatorPosition = $(currentIndicatorRows).index(indicatorRow);
                  $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", newCurrentIndicatorPosition);

                  // Update order number of previous indicator
                  var nextRow = indicatorRow.prevAll("tr.nivel01").first();

                  $(nextTrackOrder).insertBefore(nextRow);
                  $(nextTrackOrder).find("td").html(currentTrackOrderText);

                  var previousIndicatorPosition = $(currentIndicatorRows).index(nextRow);
                  $(nextRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", previousIndicatorPosition);

               } else {

                  indicatorRow.insertAfter(rowNivel1);

                  // Update order number field of moved indicator
                  var currentIndicatorRows = $(indicatorRow).closest("tbody.indicatorParent").first().find("tr.nivel01.rowIndicator");
                  var newCurrentIndicatorPosition = $(currentIndicatorRows).index(indicatorRow);
                  $(indicatorRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", newCurrentIndicatorPosition);

                  // Update order number of previous indicator
                  var nextRow = indicatorRow.prevAll("tr.nivel01").first();
                  var previousIndicatorPosition = $(currentIndicatorRows).index(nextRow);
                  $(nextRow).find('input[type="hidden"][id$="__OrderNumber"]').attr("value", previousIndicatorPosition);

               }

            }
         }
      }
   }


}

/**************************************************************************************/
/***************************** IMPACTS FUNCTIONS **************************************/
/**************************************************************************************/
function addNewImpact(impactContainer) {
   // Get Impacts count
   var impactsCount = $(impactContainer).find(".mod_tabla_impacts").length;
   // Get ResultsMAtrixId
   var resultsMatrixId = $("#ResultsMatrixId").val();
   // Set next Impact index
   var nextImpactIndex = (impactsCount >= 1) ? (impactsCount + 1) : 1;
   var htmlImpactContent = getNewImpactRow(nextImpactIndex, resultsMatrixId);

   // Add new Impact to DOM
   $(impactContainer).prepend(htmlImpactContent);

   // Add matrix change validation
   changesCount++;

   // Remake validation
   var form = $("#outcomeEditForm")
       .removeData("validator") /* added by the raw jquery.validate plugin */
       .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */
   $.validator.unobtrusive.parse(form);
}
function getNewImpactRow(impactIndex, resultsMatrixId) {

   var htmlContent = "";

   htmlContent += "<div class='mod_tabla_impacts editable'>";
   htmlContent += "<input data-val='true' data-val-required='The ResultsMatrixId field is required.' id='Outcomes_" + impactIndex + "__ResultsMatrixId' name='Outcomes[" + impactIndex + "].ResultsMatrixId' type='hidden' value='" + resultsMatrixId + "'>";
   htmlContent += "<input data-val='true' data-val-required='The OutcomeId field is required.' id='Outcomes_" + impactIndex + "__OutcomeId' name='Outcomes[" + impactIndex + "].OutcomeId' type='hidden' value='-1'>";
   htmlContent += "<input id='Outcomes_Index' name='Outcomes.Index' type='hidden' value='" + impactIndex + "'>";
   htmlContent += "<input data-val='true' data-val-required='The OrderNumber field is required.' id='Outcomes_" + impactIndex + "__OrderNumber' name='Outcomes[" + impactIndex + "].OrderNumber' type='hidden' value='" + impactIndex + "'>";
   htmlContent += "<div class='minimizeTable'>";
   htmlContent += "<div class='minimizeBar'>";
   htmlContent += "<div class='trackOrder'></div>";
   htmlContent += "<div class='tableOperator' style='z-index:1;display:none'>Minimize/Maximize</div>";
   htmlContent += "</div>";
   htmlContent += "<div class='operatorBar' id='operatorBar_" + impactIndex + "' style='height: 247.99310302734375px;display:none'></div>";
   htmlContent += "<div class='headingh3_editable'>";

   /**
   * Jira ID: CON-126
   * Fix Description: Methods added to move up and down impacts 
   * Attended by: Jhon Astaiza 
   * Date: 24/06/2014
   * Begin Fix
   **/
   htmlContent += "<label class='editLabel input' for='tituloTabla' onmouseover = 'showMainActionBar(this)' onmouseout='hideMainActionBar(this);'>";
   htmlContent += "<input class='input headingh4' data-val='true' data-val-required='Please enter a statement for the outcome' id='Outcomes_" + impactIndex + "__Statement' name='Outcomes[" + impactIndex + "].Statement' type='text' value=''>";
   htmlContent += "<div class='actionbar main' style='display: none; width: 89%'>";
   htmlContent += "<a class='btn_entypo first moveUp' title='' onclick='moveUpComponent(this)' href='javascript:void(0)'>&#59231;</a>";
   htmlContent += "<a class='btn_entypo first moveDown' title='' onclick='moveDownComponent(this)' href='javascript:void(0)'>&#59228;</a>";
   htmlContent += "</div>";
   htmlContent += "</label>";
   /**
   * End Fix
   **/

   htmlContent += "<a href='javascript:void(0)' class='btn_delete btn_square_min' title='delete' onclick='deleteImpact(this);'>Boton Delete</a>";
   htmlContent += "</div>";
   htmlContent += "<div class='operationData dataDetails dataDetails_editable'>";
   htmlContent += "<div class='data8'>";
   htmlContent += "<label class='editLabel input' for='observaciones'>";
   htmlContent += "<span class='dataTitle'>Observations</span>";
   htmlContent += "<input class='input' id='Outcomes_" + impactIndex + "__Observations' name='Outcomes[" + impactIndex + "].Observations' type='text' value=''>";
   htmlContent += "</label>";
   htmlContent += "</div>";
   htmlContent += "</div>";
   htmlContent += "<div class='tableGrid' id='containerTblGrid__" + impactIndex + "' style=''>";

   htmlContent += "</div>";
   htmlContent += "<div class='k-toolbar'>";
   htmlContent += "<a href='javascript:void(0);' title='New Indicator' class='k-button newIndicator' onclick='addNewImpactIndicatorRow(containerTblGrid__" + impactIndex + ", " + impactIndex + ");'><div class='k-button-'>New Indicator</div></a>";
   htmlContent += "</div>";

   htmlContent += "</div>";
   htmlContent += "</div>";

   return htmlContent;
}
function deleteImpact(btnDeleteImpact, intervalId) {
   var impactContainer = $(btnDeleteImpact).parent().parent().parent();
   var impactId = $(impactContainer).find('[id$="__OutcomeId"]').first().val();

   // If Impact just exits en the client side
   if (impactId <= 0) {
      // Remove impact htnml content
      $(impactContainer).remove();
      // Remove Matrix Change validation
      changesCount--;
   } else {

      // Get url to display modal window
      var urlRoute = $("#hdnDeleteImpactUrl").val() + "?impactId=" + impactId + "&intervalId=" + intervalId + "&accessedByAdmin=" + $("#AccessedByAdministrator").val() + "&isThirInterval=" + $("#IsThirdInterval").val();
      // Display modal window to remove impact from the server side
      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

      var title = $("#hdnTitleDeleteOutcome").val();
      var dialog = $(".dinamicModal").kendoWindow({
         width: "800px",
         title: title,
         draggable: false,
         resizable: false,
         content: urlRoute,
         pinned: true,
         actions: [
             "Close"
         ],
         modal: true,
         visible: false,
         open: function () {
             $('.mod_tabla.no_sortable').css('width', '100%');
         },
         close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
         }
      }).data("kendoWindow");
      $(".k-window-titlebar").addClass("warning");
      $(".k-window-title").addClass("ico_warning");
      dialog.open();
   }

}

function moveUpComponent(btnMoveUp) {
   var componentContainer = $(btnMoveUp).closest(".mod_tabla_impacts");
   var prevComponentContainer = $(componentContainer).prev(".mod_tabla_impacts");
   if (prevComponentContainer.length > 0) {

      var componentIndex = $(componentContainer).index();
      var prevComponentIndex = $(prevComponentContainer).index();

      var previousTrackOrder = $(prevComponentContainer).find(".minimizeBar > .trackOrder").html();
      var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html();
      // Update Previous Order Track Number
      $(componentContainer).find(".minimizeBar > .trackOrder").html(previousTrackOrder);
      // Update Next Order Track Number
      $(prevComponentContainer).find(".minimizeBar > .trackOrder").html(componentTrackOrder);
      // Move up current componet
      componentContainer.insertBefore(prevComponentContainer);

      $(componentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", prevComponentIndex);
      $(prevComponentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", componentIndex);
   }
}

function moveDownComponent(btnMoveDown) {
   var componentContainer = $(btnMoveDown).closest(".mod_tabla_impacts");
   var nextComponentContainer = $(componentContainer).next(".mod_tabla_impacts");
   if (nextComponentContainer.length > 0) {

      var componentIndex = $(componentContainer).index();
      var nextComponentIndex = $(nextComponentContainer).index();

      var nextTrackOrder = $(nextComponentContainer).find(".minimizeBar > .trackOrder").html();
      var componentTrackOrder = $(componentContainer).find(".minimizeBar > .trackOrder").html()
      // Update Next Order Track Number
      $(componentContainer).find(".minimizeBar > .trackOrder").html(nextTrackOrder);
      // Update Previous Order Track Number
      $(nextComponentContainer).find(".minimizeBar > .trackOrder").html(componentTrackOrder);
      // Move down current componet
      componentContainer.insertAfter(nextComponentContainer);

      $(componentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", nextComponentIndex);
      $(nextComponentContainer).find('input[type="hidden"][id$="__OrderNumber"]').first().attr("value", componentIndex);
   }
}

function showMainActionBar(stamentComponent) {
   $(stamentComponent).closest("label").find(".actionbar.main").show();
}

function hideMainActionBar(stamentComponent) {
   $(stamentComponent).closest("label").find(".actionbar.main").hide();
}

/**
* End Fix
**/

/**************************************************************************************/

function resizeOperatorBarExpand() {
   var parentTables = $(".minimizeTable").not(".child");

   $.each(parentTables, function (index, parentTable) {
      var aditionalHeight = 0;
      var childTables = $(this).find(".child");

      $.each(childTables, function (index, childTable) {
         var height = $(this).css("height");
         $(this).find(".operatorBar").css("height", height);

         aditionalHeight += parseInt(height);
      });

      var operatorBarParent = $(this).find(".parent");
      var parentHeight = parseInt($(operatorBarParent).css("height"));
      var newHeight = parentHeight + aditionalHeight;
      $(operatorBarParent).css("height", newHeight + "px");

   });
}

function resizeOperatorBarCollapse() {
   var parentTables = $(".minimizeTable").not(".child");

   $.each(parentTables, function (index, parentTable) {
      var aditionalHeight = 0;
      var childTables = $(this).find(".child");

      $.each(childTables, function (index, childTable) {
         var height = $(this).css("height");
         $(this).find(".operatorBar").css("height", height);
         aditionalHeight += parseInt(height);
      });

      var operatorBarParent = $(this).find(".parent");
      var parentHeight = parseInt($(operatorBarParent).css("height"));
      var newHeight = parentHeight - aditionalHeight;
      $(operatorBarParent).css("height", newHeight + "px");
      //$(parentTable).find("operatorBar:first-child").css("height", aditionalHeight + "px");

   });
}

/********************************************** IMPACTS FUNCTIONS ***********************************************/
function addNewImpactIndicatorRow(impactIndicatorContainer, impactIndex) {
   var tableImpactIndicator = $(impactIndicatorContainer).children();
   var htmlContent = "";

   if (tableImpactIndicator.length < 1) {
       // Add new table to DOM
       htmlContent = getNewImpactIndicatorRowHead(impactIndex);
      $(impactIndicatorContainer).append(htmlContent);

      // Add new row to table
      var resultsMatrixId = $("#ResultsMatrixId").val();
      var rowContent = getNewImpactIndicatorRowContent("nivel01", resultsMatrixId, -1, impactIndex, 1);

      $(impactIndicatorContainer).find(".grid").append(rowContent);
      $(impactIndicatorContainer).find("tr.rowIndicator").last().find("select.optionSelectYearsPlanNew").kendoDropDownList();

      // Add new columns
      var gridBodyId = $(impactIndicatorContainer).find('[id^="bodyTable_' + impactIndex + '"]');
      var rowHeadId = $(impactIndicatorContainer).find('[id^="yearPlans_' + impactIndex + '"]');
      var followingId = $(impactIndicatorContainer).find('[id^="followingYear_' + impactIndex + '"]');
      var years = getCurrentListOfYears();

      for (var i = 0; i < years.length; i++) {
         addNewYearPlanColum(gridBodyId, rowHeadId, followingId, impactIndex, 3, years[i]);
      }

      // Add matrix change validation
      changesCount++;

      var form = $("#outcomeEditForm").removeData("validator").removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse(form);

   } else {
      var indicatorsTable = $(impactIndicatorContainer).find(".grid").first();
      var impactIndicatorIndex = $("#bodyTable_" + impactIndex).find(".rowIndicator").length;
      impactIndicatorIndex++;
      var rowStyle = Math.pow(-1, impactIndicatorIndex) > 0 ? "nivel01" : "nivel01 impar";

      // Add new row to table
      var rowContent = getNewImpactIndicatorRowContent(rowStyle, resultsMatrixId, -1, impactIndex, impactIndicatorIndex);
      $("#bodyTable_" + impactIndex).append(rowContent);

      // Activate dropdownlist
      $(impactIndicatorContainer).find("tr.rowIndicator").last().find("select.optionSelectYearsPlanNew").kendoDropDownList();

      // Add matrix change validation
      changesCount++;

      var form = $("#outcomeEditForm").removeData("validator").removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse(form);
   }

   // Resize table operator
   resizeOperatorBar($(impactIndicatorContainer).children("table.grid").first());
   setAutonumericFormat(tableImpactIndicator);

   $('.toolTipDisaggregation').tooltip();

    fixDropdonwYears();
}

function getNewImpactIndicatorRowHead(impactIndex) {

   var htmlContent = "";
   htmlContent += "<table id='tblGrid_" + impactIndex + "' class='grid'>";
   htmlContent += "<thead>";
   htmlContent += "<tr <tr id='yearPlans_" + impactIndex + "' style='padding:0' class='rowYearPlan'>";
   htmlContent += "<th class='btn_azul_oscuro dato00'>Indicator</th>";
   htmlContent += "<th class='btn_azul_oscuro dato06'>Unit of measure</th>";
   htmlContent += "<th class='btn_azul_oscuro dato06'>Baseline</th>";
   htmlContent += "<th class='btn_azul_oscuro dato06'>Baseline year</th>";
   htmlContent += "<th class='btn_azul_oscuro dato04'></th>";
   htmlContent += "<th id='lastCellHead' class='btn_azul_oscuro selects' style='text-align:center;vertical-align:middle;width: 2em' onclick='addNewYearPlanColumGlobal(bodyTable_" + impactIndex + ", yearPlans_" + impactIndex + ", 0, " + impactIndex + ")'>";
   htmlContent += "<label style='cursor:pointer;text-align:center;' onclick=''><b>+</b></label>";
   htmlContent += "</th>";
   htmlContent += "<th class='btn_azul_oscuro dato05 thEOPImpacts' rowspan='2'>EOP " + $("#hdnEOP").val() + "</th>";
   htmlContent += "</tr>";
   htmlContent += "</thead>";
   htmlContent += "<tbody id='bodyTable_" + impactIndex + "' style='overflow-x: scroll' class='indicatorParent'>";
   htmlContent += "</tbody>";
   htmlContent += "</table>";

   return htmlContent;
}
function getNewImpactIndicatorRowContent(rowStyle, resultsMatrixId, impactId, impactIndex, impactIndicatorIndex) {
   var htmlContent = "";
   // Start Set TrackOrder Row
   var impactOrder = impactIndex <= 1 ? 1 : ((impactIndex + 1)) - 1;
   var indicatorOrder = impactIndicatorIndex <= 1 ? 1 : ((impactIndicatorIndex + 1)) - 1;
   htmlContent += '<tr class="trackOrderRow" id="">';
   htmlContent += '<td style="vertical-align: middle;">' + impactOrder + '.' + indicatorOrder + '</td>';
   htmlContent += '</tr>';
   // End Set TrackOrder Row
   htmlContent += '<tr class="' + rowStyle + ' rowIndicator" id="impactIndicatorRow_' + impactIndex + '">';
   htmlContent += '<td class="verticalShadow">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '_OutcomeId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeId" type="hidden" value="' + impactId + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeId">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorId" type="hidden" value="0" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorId">';

   htmlContent += '<input data-val="true" data-val-required="The OrderNumber field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OrderNumber" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OrderNumber" type="hidden" value="' + impactIndicatorIndex + '">';

   htmlContent += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_Index" name="Outcomes[' + impactIndex + '].OutcomeIndicators.Index" type="hidden" value="' + impactIndicatorIndex + '">';
   htmlContent += '<div class="dato01">';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Indicator</legend>';
   htmlContent += '<textarea class="dato01" cols="55" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__Definition" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].Definition" data-val-required="Please enter the indicator that will be used for measuring progress" data-val="true" rows="2"></textarea>';
   htmlContent += '</fieldset>';
   htmlContent += '</div>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Unit of measure</legend>';
   htmlContent += '<label class="editLabel ici" for="unit01">';
   htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__UnitOfMeasure" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].UnitOfMeasure" type="text" value="">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Baseline</legend>';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__Baseline" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].Baseline" type="text" value="">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td class="dato02">';
   htmlContent += DropDownBaseLineYear(impactIndex, impactIndicatorIndex);
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<table class="ouputTable">';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P(a)';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'A';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '<div class="actionbar" id="actionBar_0">';

   htmlContent += '<a class="btn_entypo first moveUp" title="' + $("#hdnMoveUpIndicatorToolTip").val() + '" onclick="moveIndicator(this)" href="javascript:void(0)">&#59231;</a>';
   htmlContent += '<a class="btn_entypo first moveDown" title="' + $("#hdnMoveDownIndicatorToolTip").val() + '" onclick="moveIndicator(this)" href="javascript:void(0)">&#59228;</a>';

   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnReassignIndicatorToolTip").val() + '" href="#">🔙</a>';
   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnLinkPredefinedIndicatorToolTip").val() + '" href="#">⚑</a>';
   htmlContent += '<a class="btn_entypo first addDisBtn" title="' + $("#hdnAddDissagregationToolTip").val() + '" href="javascript:void(0)" onclick = "addNewDisaggregationDynamic(this,' + impactIndex + ',' + impactIndicatorIndex + ',-1);"></a>';
   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnDeleteIndicatorToolTip").val() + '" href="#" onclick="deleteImpactIndicator(null, null, ' + resultsMatrixId + ', -1, this, null, null)"></a>';
   htmlContent += '</div>';
   htmlContent += '</td>';

   // Get head row of table
   var headRow = $("#bodyTable_" + impactIndex).parent().find("thead").first();

   // Get count of years plan
   var cells = $(headRow).find("select option:selected");

   // Set Interval Object 
   var interval = new Object();
   interval.IntervalId = $("#hdnIntervalId").val();
   interval.CycleId = $("#hdnCycleId").val();

   for (var i = 0; i < cells.length; i++) {

      // to set active or inactive state of controls
      var displayModeStartUpPlan = false; var displayModeAnnualPlan = false; var displayModeActualPlan = false;

      displayModeStartUpPlan = BlockFieldStartUpPlan(interval);
      displayModeAnnualPlan = BlockFieldAnnualPlan(interval, parseInt($(cells[i]).text()));
      displayModeActualPlan = BlockFieldActualPlan(interval, parseInt($(cells[i]).text()));

      htmlContent += '<td>';
      htmlContent += '<table class="ouputTable">';
      htmlContent += '<tbody style="border-width:0px">';
      htmlContent += '<tr class="rm_p_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';
      htmlContent += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_Index" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans.Index" type="hidden" value="' + i + '">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__Year" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].Year" type="hidden" value="' + $(cells[i]).text() + '">';
      htmlContent += '<input id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';

      htmlContent += '<input class="input min_input"' + (displayModeStartUpPlan ? ' bcGray' : '') +
        ' auto" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) +
        '__OutcomeIndicatorYearPlans_' + i + '__OriginalPlan" name="Outcomes[' + (impactIndex) +
        '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i +
        '].OriginalPlan" type="text" data-v-min="-9999999999999999.99" ' +
        'data-v-max="9999999999999999.99" value=""' + (displayModeStartUpPlan ? ' readonly="readonly"' : '') + '>';

      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr class="rm_pa_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';

      htmlContent += '<input class="input min_input' + (displayModeAnnualPlan ? ' bcGray' : '') +
        ' auto" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) +
        '__OutcomeIndicatorYearPlans_' + i + '__AnnualPlan" name="Outcomes[' + (impactIndex) +
        '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i +
        '].AnnualPlan" type="text"  data-v-min="-9999999999999999.99" ' +
        'data-v-max="9999999999999999.99" value=""' + (displayModeAnnualPlan ? ' readonly="readonly"' : '') + '>';

      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr class="rm_ac_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';

      htmlContent += '<input class="input min_input' + (displayModeActualPlan ? ' bcGray' : '') +
        ' auto" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) +
        '__OutcomeIndicatorYearPlans_' + i + '__ActualValue" name="Outcomes[' + (impactIndex) +
        '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i +
        '].ActualValue" type="text"  data-v-min="-9999999999999999.99" ' +
        'data-v-max="9999999999999999.99" value=""' + (displayModeActualPlan ? ' readonly="readonly"' : '') + '>';

      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '</tbody>';
      htmlContent += '</table>';
      htmlContent += '</td>';
   }

   // to set active or inactive state of controls
   var displayModeEOPStartUpPlan = false; var displayModeEOPAnnualPlan = false; var displayModeEOPActualPlan = false;
   displayModeEOPStartUpPlan = BlockFieldStartUpPlanEOP(interval);
   displayModeEOPAnnualPlan = BlockFieldAnnualPlanEOP(interval, -1);
   displayModeEOPActualPlan = BlockFieldActualPlanEOP(interval, -1);

   impactIndex, impactIndicatorIndex

   htmlContent += '<td> </td>';
  
   htmlContent += '<td id="lastCellContent" class="dato07">';
   htmlContent += '<table class="ouputTable">';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr class="rm_p_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OutcomeIndicatorId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OutcomeIndicatorId" type="hidden" value="0">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OutcomeIndicatorYearPlanId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OutcomeIndicatorYearPlanId" type="hidden" value="0">';
   htmlContent += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_Year" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.Year" type="hidden" value="-1">';

   if (displayModeEOPStartUpPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OriginalPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OriginalPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }

   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr class="rm_pa_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';

   if (displayModeEOPAnnualPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_AnnualPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.AnnualPlan" type="text" data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_AnnualPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.AnnualPlan" type="text" data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }

   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr class="rm_ac_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';
   if (displayModeEOPActualPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_ActualValue" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_ActualValue" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '</td>';


   htmlContent += '</tr>';

   // Dissagregation row
   var dissagColspan = (cells.length > 0) ? cells.length : 1;
   htmlContent += "<tr class='nivel02'>";
   htmlContent += "<td colspan='9' class='spanCell'>";
   htmlContent += "<div class='minimizeTable child'>";
   htmlContent += "<div class='minimizeBar' style='display:none'>";
   htmlContent += "<div class='tableOperator' style='top:0px;z-index:1'>Minimize/Maximize</div>";
   htmlContent += "</div>";
   htmlContent += "<div class='operatorBar' style='height: 0px;'></div>";
   htmlContent += "<div class='tableGrid' style='display: block;'>";
   htmlContent += "<table class='grid disagChildTable' style='display:none'>";
   htmlContent += "<thead class='disagChildHead'>";
   htmlContent += "<tr class='headRowDisaggregationYears'>";
   htmlContent += "<th class='btn_azul_oscuro dato00'>";
   htmlContent += "<span>" + $('#hdnDisaggCategLabel').val() + "</span>";
   htmlContent += "<span class='glyphicon glyphicon-info-sign toolTipDisaggregation' ";
   htmlContent += "    title='" + $('#hdnDisaggCategTooltip').val() + "'></span></th>";
   htmlContent += "<th class='btn_azul_oscuro dato04 colIni'></th>";
   htmlContent += "<th class='btn_azul_oscuro dato04' ></th>";
   htmlContent += "<th class='btn_azul_oscuro dato05 thEOPImpacts' id='lastCellHeadChild'>EOP  " + $("#hdnEOP").val() + "</th>";
   htmlContent += "</tr>";
   htmlContent += "</thead>";
   htmlContent += "<tbody id='' class='disagChild'>";
   htmlContent += "</tbody>";
   htmlContent += "</table>";
   htmlContent += "</div>";
   htmlContent += "</div>";
   htmlContent += "</td>";
   htmlContent += "</tr>";

   return htmlContent;
}

/********************************************** IMPACTS FUNCTIONS ***********************************************/
function addNewImpactIndicatorExistingRow(impactIndicatorContainer, impactIndex, impactId) {
   var tableImpactIndicator = $(impactIndicatorContainer).children();
   var htmlContent = "";

   if (tableImpactIndicator.length <= 0) {
      // Add new table to DOM
      htmlContent = getNewImpactIndicatorRowHead(impactIndex);
      $(impactIndicatorContainer).append(htmlContent);

      // Add new row to table
      var resultsMatrixId = $("#ResultsMatrixId").val();
      var rowContent = getNewImpactIndicatorExistingRowContent("nivel01", resultsMatrixId, impactId, impactIndex, 0);
      $(impactIndicatorContainer).find(".grid").append(rowContent);

      // ADD NEW COLUMNS
      var gridBodyId = $(impactIndicatorContainer).find('[id^="bodyTable_' + impactIndex + '"]');
      var rowHeadId = $(impactIndicatorContainer).find('[id^="yearPlans_' + impactIndex + '"]');
      var followingId = $(impactIndicatorContainer).find('[id^="followingYear_' + impactIndex + '"]');
      var years = getCurrentListOfYears();

      for (var i = 0; i < years.length; i++) {
         addNewYearPlanColum(gridBodyId, rowHeadId, followingId, impactIndex, 3, years[i]);
      }

      $(impactIndicatorContainer).find(".optionSelectYearsPlanNew").kendoDropDownList({ enable: true });

      // Add matrix change validation
      changesCount++;

      var form = $("#outcomeEditForm").removeData("validator").removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse(form);

   } else {
      var indicatorsTable = $(impactIndicatorContainer).find(".grid").first();
      var impactIndicatorIndex = $("#bodyTable_" + impactIndex).find(".rowIndicator").length;
      var rowStyle = Math.pow(-1, impactIndicatorIndex) > 0 ? "nivel01" : "nivel01 impar";

       // Add new row to table
      var rowContent = getNewImpactIndicatorExistingRowContent(rowStyle, resultsMatrixId, impactId, impactIndex, impactIndicatorIndex);
      $("#bodyTable_" + impactIndex).append(rowContent);

      // Activate dropdownlist
      $("#bodyTable_" + impactIndex).find(".optionSelectYearsPlanNew").kendoDropDownList({ enable: true });

      // Add matrix change validation
      changesCount++;

      var form = $("#outcomeEditForm").removeData("validator").removeData("unobtrusiveValidation");
      $.validator.unobtrusive.parse(form);
   }
   // Resize table operator
   resizeOperatorBar($(impactIndicatorContainer).children("table.grid").first());
   setAutonumericFormat(tableImpactIndicator);

   $('.toolTipDisaggregation').tooltip();
}

function getNewImpactIndicatorExistingRowContent(rowStyle, resultsMatrixId, impactId, impactIndex, impactIndicatorIndex) {
   var htmlContent = "";
   // Start Set TrackOrder Row
   var impactOrder = impactIndex <= 0 ? 1 : (impactIndex + 1);
   var indicatorOrder = impactIndicatorIndex <= 0 ? 1 : (impactIndicatorIndex + 1);
   htmlContent += '<tr class="trackOrderRow" id="">';
   htmlContent += '<td style="vertical-align: middle;">' + impactOrder + '.' + indicatorOrder + '</td>';
   htmlContent += '</tr>';
   // End Set TrackOrder Row

   htmlContent += '<tr class="' + rowStyle + ' rowIndicator" id="impactIndicatorRow_' + impactIndex + '">';
   htmlContent += '<td class="verticalShadow">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '_OutcomeId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeId" type="hidden" value="' + impactId + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeId">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorId" type="hidden" value="0" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorId">';

   htmlContent += '<input data-val="true" data-val-required="The OrderNumber field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OrderNumber" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OrderNumber" type="hidden" value="' + impactIndicatorIndex + '">';

   htmlContent += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_Index" name="Outcomes[' + impactIndex + '].OutcomeIndicators.Index" type="hidden" value="' + impactIndicatorIndex + '">';
   htmlContent += '<div class="dato01">';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Indicator</legend>';
   htmlContent += '<textarea class="dato01" cols="55" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__Definition" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].Definition" data-val-required="Please enter the indicator that will be used for measuring progress" data-val="true"  rows="2"></textarea>';
   htmlContent += '</fieldset>';
   htmlContent += '</div>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Unit of measure</legend>';
   htmlContent += '<label class="editLabel ici" for="unit01">';
   htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__UnitOfMeasure" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].UnitOfMeasure" type="text" value="">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Baseline</legend>';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__Baseline" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].Baseline" type="text" value="">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td class="dato02">';
   htmlContent += DropDownBaseLineYear(impactIndex, impactIndicatorIndex);
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<table class="ouputTable">';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P(a)';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'A';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '<div class="actionbar" id="actionBar_0">';

   htmlContent += '<a class="btn_entypo first moveUp" title="' + $("#hdnMoveUpIndicatorToolTip").val() + '" onclick="moveIndicator(this)" href="javascript:void(0)">&#59231;</a>';
   htmlContent += '<a class="btn_entypo first moveDown" title="' + $("#hdnMoveDownIndicatorToolTip").val() + '" onclick="moveIndicator(this)" href="javascript:void(0)">&#59228;</a>';

   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnReassignIndicatorToolTip").val() + '" href="javascript:void(0)">🔙</a>';
   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnLinkPredefinedIndicatorToolTip").val() + '" href="javascript:void(0)">⚑</a>';
   htmlContent += '<a class="btn_entypo first addDisBtn" title="' + $("#hdnAddDissagregationToolTip").val() + '" href="javascript:void(0)" onclick = "addNewDisaggregationDynamic(this,' + impactIndex + ',' + impactIndicatorIndex + ',-1);"></a>';
   htmlContent += '<a class="btn_entypo first" title="' + $("#hdnDeleteIndicatorToolTip").val() + '" href="javascript:void(0)" onclick="deleteImpactIndicator(null, null, ' + resultsMatrixId + ', -1, this, null, null)" href="#"></a>';
   htmlContent += '</div>';
   htmlContent += '</td>';

   // Get head row of table
   var headRow = $("#bodyTable_" + impactIndex).parent().find("thead").first();

   // Get count of years plan
   var cells = $(headRow).find("select option:selected");

   // Set Interval Object 
   var interval = new Object();
   interval.IntervalId = $("#hdnIntervalId").val();
   interval.CycleId = $("#hdnCycleId").val();

   for (var i = 0; i < cells.length; i++) {

      // to set active or inactive state of controls
      var displayModeStartUpPlan = false; var displayModeAnnualPlan = false; var displayModeActualPlan = false;

      displayModeStartUpPlan = BlockFieldStartUpPlan(interval);
      displayModeAnnualPlan = BlockFieldAnnualPlan(interval, parseInt($(cells[i]).text()));
      displayModeActualPlan = BlockFieldActualPlan(interval, parseInt($(cells[i]).text()));

      htmlContent += '<td>';
      htmlContent += '<table class="ouputTable">';
      htmlContent += '<tbody style="border-width:0px">';
      htmlContent += '<tr class="rm_p_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';
      htmlContent += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_Index" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans.Index" type="hidden" value="' + i + '">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__Year" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].Year" type="hidden" value="' + $(cells[i]).text() + '">';
      htmlContent += '<input id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';

      if (!displayModeStartUpPlan) {
         htmlContent += '<input class="input min_input" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OriginalPlan" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
      } else {
          htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__OriginalPlan" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
      }


      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr class="rm_pa_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';

      if (!displayModeAnnualPlan) {
         htmlContent += '<input class="input min_input" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__AnnualPlan" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].AnnualPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
      } else {
          htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__AnnualPlan" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].AnnualPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';

      }



      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr class="rm_ac_row">';
      htmlContent += '<td class="rm_edit_rh">';
      htmlContent += '<label class="editLabel" for="">';

      if (!displayModeActualPlan) {
         htmlContent += '<input class="input min_input" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__ActualValue" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
      } else {
          htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + (impactIndex) + '__OutcomeIndicators_' + (impactIndicatorIndex) + '__OutcomeIndicatorYearPlans_' + i + '__ActualValue" name="Outcomes[' + (impactIndex) + '].OutcomeIndicators[' + (impactIndicatorIndex) + '].OutcomeIndicatorYearPlans[' + i + '].ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
      }


      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '</tbody>';
      htmlContent += '</table>';
      htmlContent += '</td>';
   }

   // to set active or inactive state of controls
   var displayModeEOPStartUpPlan = false; var displayModeEOPAnnualPlan = false; var displayModeEOPActualPlan = false;
   displayModeEOPStartUpPlan = BlockFieldStartUpPlanEOP(interval);
   displayModeEOPAnnualPlan = BlockFieldAnnualPlanEOP(interval, -1);
   displayModeEOPActualPlan = BlockFieldActualPlanEOP(interval, -1);

   impactIndex, impactIndicatorIndex

   htmlContent += '<td> </td>';
  
   htmlContent += '<td id="lastCellContent" class="dato07">';
   htmlContent += '<table class="ouputTable">';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr class="rm_p_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OutcomeIndicatorId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OutcomeIndicatorId" type="hidden" value="0">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OutcomeIndicatorYearPlanId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OutcomeIndicatorYearPlanId" type="hidden" value="0">';
   htmlContent += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_Year" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.Year" type="hidden" value="-1">';

   if (displayModeEOPStartUpPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OriginalPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_OriginalPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.OriginalPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }

   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr class="rm_pa_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';

   if (displayModeEOPAnnualPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_AnnualPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.AnnualPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_AnnualPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.AnnualPlan" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }

   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr class="rm_ac_row">';
   htmlContent += '<td class="rm_edit_rh">';
   htmlContent += '<label class="editLabel" for="">';
   if (displayModeEOPActualPlan) {
      htmlContent += '<input class="input min_input" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_ActualValue" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="">';
   } else {
       htmlContent += '<input class="input min_input bcGray" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__EndOfProject_ActualValue" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].EndOfProject.ActualValue" type="text"  data-v-min="-9999999999999999.99" data-v-max="9999999999999999.99" value="" readonly="readonly">';
   }
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '</td>';

   htmlContent += '</tr>';

   // Dissagregation row
   htmlContent += "<tr class='nivel02'>";
   htmlContent += "<td colspan='" + (cells.length + 7) + "' class='spanCell'>";
   htmlContent += "<div class='minimizeTable child'>";
   htmlContent += "<div class='minimizeBar' style='display:none'>";
   htmlContent += "<div class='tableOperator' style='top:0px;z-index:1'>Minimize/Maximize</div>";
   htmlContent += "</div>";
   htmlContent += "<div class='operatorBar' style='height: 0px;'></div>";
   htmlContent += "<div class='tableGrid' style='display: block;'>";
   htmlContent += "<table class='grid disagChildTable' style='display:none'>";
   htmlContent += "<thead class='disagChildHead'>";
   htmlContent += "<tr class='headRowDisaggregationYears'>";
   htmlContent += "<th class='btn_azul_oscuro dato00'>";
   htmlContent += "<span>" + $('#hdnDisaggCategLabel').val() + "</span>";
   htmlContent += "<span class='glyphicon glyphicon-info-sign toolTipDisaggregation' ";
   htmlContent += "    title='" + $('#hdnDisaggCategTooltip').val() + "'></span></th>";
   htmlContent += "<th class='btn_azul_oscuro dato04 colIni' ></th>";
   htmlContent += "<th class='btn_azul_oscuro dato04' ></th>";
   htmlContent += "<th class='btn_azul_oscuro dato05 thEOPImpacts' id='lastCellHeadChild'>EOP  2014</th>";
   htmlContent += "</tr>";
   htmlContent += "</thead>";
   htmlContent += "<tbody id='' class='disagChild'>";
   htmlContent += "</tbody>";
   htmlContent += "</table>";
   htmlContent += "</div>";
   htmlContent += "<div class='k-toolbar disagTool'>";
   htmlContent += "<a href='javascript:void(0);' title='New Disaggregation' style='width: 29em;padding-left:0.4em' class='k-button newIndicator disaggregationButton' onclick='addNewDisaggregation(this," + impactIndex + "," + impactIndicatorIndex + ",-1);'><div class='k-button-'>New Disaggregation</div></a>";
   htmlContent += "</div>";
   htmlContent += "</div>";
   htmlContent += "</td>";
   htmlContent += "</tr>";

   return htmlContent;
}

function resizeOperatorBar(indicatorTable) {
   var modDivTable = $(indicatorTable).parent().parent().parent();
   var operatorBar = $(modDivTable).find(".operatorBar").first();
   var tableOperator = $(modDivTable).find(".tableOperator").first();
   var tableHeight = $(modDivTable).css("height");

   tableHeight = tableHeight.substr(0, tableHeight.length - 2);
   $(operatorBar).css("height", (tableHeight - 26) + "px");
}

function deleteImpactIndicator(order, definition, resultsMatrixId, impactIndicatorId, row, placeHolder, checkSARmIndicatorsUrl) {
    var usedForStrategicAlignment = $(row)
        .closest("tr.nivel01")
        .find("> td:eq(0) > input.UsedForStrategicAlignment")
        .val();

    if (usedForStrategicAlignment != undefined &&
        usedForStrategicAlignment.toLowerCase() === "true" &&
        placeHolder != null &&
        checkSARmIndicatorsUrl != null) {

        checkRmIndicatorRelationsToSAClassifications(placeHolder, checkSARmIndicatorsUrl, function () {
            deleteImpactIndicatorExecution(order, definition, resultsMatrixId, impactIndicatorId, row);
        });
    } else {
        deleteImpactIndicatorExecution(order, definition, resultsMatrixId, impactIndicatorId, row);
    }
}

function deleteImpactIndicatorExecution(order, definition, resultsMatrixId, impactIndicatorId, row) {
    var urlRoute = $("#hdnDeleteIndicator").val() + "?order=" + order + "&definition=" + definition +
        "&resultsMatrixId=" + resultsMatrixId + "&impactIndicatorId=" + impactIndicatorId +
        "&intervalId=" + $("#hdnIntervalId").val();

    if (impactIndicatorId > -1) {
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
        var title = $("#hdnTitleDeleteOutcomeIndicator").val();
        var dialog = $(".dinamicModal").kendoWindow({
            width: "800px",
            title: title,
            draggable: false,
            resizable: false,
            content: urlRoute,
            pinned: true,
            actions: [
                "Close"
            ],
            modal: true,
            visible: false,
            open: function () {
                kendoWindowCenter($(".dinamicModal"));
            },
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        dialog.open();
    } else {
        var rowContent = $(row).closest("tr.nivel01");
        var dissRow = $(rowContent).next("tr.nivel02");
        var indicatorTable = $(rowContent).closest("table");

        $(rowContent).prevAll('tr.trackOrderRow').first().remove();
        $(rowContent).remove();
        $(dissRow).remove();

        changesCount--;
        resizeOperatorBar(indicatorTable);
    }

    return false;
}

function unLinkPredefinedIndicator(resultsMatrixId, impactIndicatorId, module) {
   // Get url to display modal window
   var urlRoute = $("#hdnUnlinkPredefinedIndicatorUrl").val() + "?resultsMatrixId=" + resultsMatrixId + "&indicatorId=" + impactIndicatorId + "&module=" + module;

   // Display modal window to remove impact from the server side
   $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
   var title = "Unlink Predefined Indicator";
   var dialog = $(".dinamicModal").kendoWindow({
      width: "800px",
      title: title,
      draggable: false,
      resizable: false,
      content: urlRoute,
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
   dialog.open();
}
function reorderOutcomeIndicatorIndex(tableBody) {
   var impactTableId = $(tableBody).parent().attr("id");
   var impactIndex = parseInt(impactTableId.substr(impactTableId.length - 1, 1));

   var impactIndicatorRows = $(tableBody).find('[id^="impactIndicatorRow_"]');
   var newImpactIndicatorRowsLength = $(impactIndicatorRows).length;
   var rowCount = 0;

   $.each(impactIndicatorRows, function (index, value) {

      var inputImpactIndicator = $(this).find('[id$="__OutcomeIndicatorId"]');

      if ($(inputImpactIndicator).val() < 0) {
         // Reorder hidden fields
         $(this).find('[id$="__OutcomeId"]').attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeId");
         $(this).find('[id$="__OutcomeId"]').attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeId");

         $(inputImpactIndicator).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorId");
         $(inputImpactIndicator).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorId");

         // Reorder indicators fields
         $(this).find("textarea").attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__Definition");
         $(this).find("textarea").attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].Definition");

         $(this).find('[id$="__UnitOfMeasure"]').attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__UnitOfMeasure");
         $(this).find('[id$="__UnitOfMeasure"]').attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].UnitOfMeasure");

         $(this).find('[id$="__Baseline"]').attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__Baseline");
         $(this).find('[id$="__Baseline"]').attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].Baseline");

         $(this).find('[id$="__BaselineYear"]').attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__BaselineYear");
         $(this).find('[id$="__BaselineYear"]').attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].BaselineYear");



         // Reorder OriginalPlan fields
         var impactYearsPlanOriginalPlan = $(this).find('[id$="__OriginalPlan"]');
         if (impactYearsPlanOriginalPlan.length > 0) {
            $.each(impactYearsPlanOriginalPlan, function (index, yearPlan) {
               // Retrieve siblings input of the current element
               var inputSiblings = $(yearPlan).parent().parent().find("input");

               //<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Impacts_0__ImpactIndicators_1__ImpactIndicatorId" name="Impacts[0].ImpactIndicators[1].ImpactIndicatorId" type="hidden" value="-1">
               var originalPlanImpactIndicatorIdInput = inputSiblings[0];
               // Retrieve Index of every original hidden plan field
               $(originalPlanImpactIndicatorIdInput).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorId");
               $(originalPlanImpactIndicatorIdInput).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorId");


               //<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Impacts_0__ImpactIndicators_2__ImpactIndicatorYearPlans_0__ImpactIndicatorYearPlanId" name="Impacts[0].ImpactIndicators[2].ImpactIndicatorYearPlans[0].ImpactIndicatorYearPlanId" type="hidden" value="-1">
               var originalPlanImpactIndicatorYearPlanIdInput = inputSiblings[1];
               var originalPlanImpactIndicatorYearPlanIdIndex = $(originalPlanImpactIndicatorYearPlanIdInput).attr("name").match(/\[.*?\]/g)[2];
               var originalPlanImpactIndicatorYearPlanIndex = originalPlanImpactIndicatorYearPlanIdIndex.slice(1, (originalPlanImpactIndicatorYearPlanIdIndex.length - 1));
               $(originalPlanImpactIndicatorYearPlanIdInput).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + originalPlanImpactIndicatorYearPlanIndex + "__OutcomeIndicatorYearPlanId");
               $(originalPlanImpactIndicatorYearPlanIdInput).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + originalPlanImpactIndicatorYearPlanIndex + "].OutcomeIndicatorYearPlanId");

               //<input data-val="true" data-val-required="The Year field is required." id="Impacts_0__ImpactIndicators_2__ImpactIndicatorYearPlans_0__Year" name="Impacts[0].ImpactIndicators[2].ImpactIndicatorYearPlans[0].Year" type="hidden" value="2012">
               var originalPlanYearInput = inputSiblings[2];
               var originalPlanYearInputIndex = $(originalPlanYearInput).attr("name").match(/\[.*?\]/g)[2];
               var originalPlanYearIndex = originalPlanYearInputIndex.slice(1, (originalPlanYearInputIndex.length - 1));
               $(originalPlanYearInput).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + originalPlanYearIndex + "__Year");
               $(originalPlanYearInput).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + originalPlanYearIndex + "].Year");

               //<input id="Impacts_0__ImpactIndicators_2__ImpactIndicatorYearPlans_0__ImpactIndicatorYearPlanId" name="Impacts[0].ImpactIndicators[2].ImpactIndicatorYearPlans[0].ImpactIndicatorYearPlanId" type="hidden" value="-1">
               var originalPlanImpactIndicatorYearPlanIdInputHidden = inputSiblings[3];
               var originalPlanImpactIndicatorYearPlanIdHiddenIndex = $(originalPlanImpactIndicatorYearPlanIdInputHidden).attr("name").match(/\[.*?\]/g)[2];
               var originalPlanImpactIndicatorYearPlanHiddenIndex = originalPlanImpactIndicatorYearPlanIdHiddenIndex.slice(1, (originalPlanImpactIndicatorYearPlanIdHiddenIndex.length - 1));
               $(originalPlanImpactIndicatorYearPlanIdInputHidden).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + originalPlanImpactIndicatorYearPlanHiddenIndex + "__OutcomeIndicatorYearPlanId");
               $(originalPlanImpactIndicatorYearPlanIdInputHidden).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + originalPlanImpactIndicatorYearPlanHiddenIndex + "].OutcomeIndicatorYearPlanId");

               // Retrieve Index of every original plan field
               var yearIdIndex = $(yearPlan).attr("name").match(/\[.*?\]/g)[2];
               var yearIndex = yearIdIndex.slice(1, (yearIdIndex.length - 1));
               // Update index of original plan field
               $(yearPlan).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + yearIndex + "__OriginalPlan");
               $(yearPlan).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + yearIndex + "].OriginalPlan");

            });

            // Reorder AnnualPlan fields
            var impactYearsPlanAnnualPlan = $(this).find('[id$="__AnnualPlan"]');
            $.each(impactYearsPlanAnnualPlan, function (index, yearPlan) {

               // Retrieve Index of every original plan field
               var yearIdIndex = $(yearPlan).attr("name").match(/\[.*?\]/g)[2];
               var yearIndex = yearIdIndex.slice(1, (yearIdIndex.length - 1));
               // Update index of AnnualPlan field
               $(yearPlan).attr("id", "Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + yearIndex + "__AnnualPlan");
               $(yearPlan).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + yearIndex + "].AnnualPlan");

            });

            // Reorder AnnualPlan fields
            var impactYearsPlanActualValue = $(this).find('[id$="__ActualValue"]');
            $.each(impactYearsPlanActualValue, function (index, yearPlan) {

               // Retrieve Index of every original plan field
               var yearIdIndex = $(yearPlan).attr("name").match(/\[.*?\]/g)[2];
               var yearIndex = yearIdIndex.slice(1, (yearIdIndex.length - 1));
               // Update index of original plan field
               $(yearPlan).attr("id", " Outcomes_" + impactIndex + "__OutcomeIndicators_" + rowCount + "__OutcomeIndicatorYearPlans_" + yearIndex + "__ActualValue");
               $(yearPlan).attr("name", "Outcomes[" + impactIndex + "].OutcomeIndicators[" + rowCount + "].OutcomeIndicatorYearPlans[" + yearIndex + "].ActualValue");

            });
         }
      }
      rowCount++;
   });
}
function getOrderTrackRowContent(currentOrder) {
   var htmlContent = "";

   htmlContent += '<tr class="trackOrderRow" id="">';
   htmlContent += '<td style="vertical-align: middle;">' + currentOrder + '</td>';
   htmlContent += '</tr>';

   return htmlContent;
}
/****************************************************************************************************************/

function add(gridBody, impactOrder, impactId, resultsMatrixId) {
   // Get number rows of the current tbody grid
   var impactIndicatorRows = $(gridBody).find('tr.trackOrderRow').length;
   // Get number of the impact items
   var impactRows = 3;
   // Increase in one  the impact order
   var impactTrackOrder = impactOrder > 0 ? (impactOrder - 1) : 0;
    // Increase in one the indicator order
   var indicatorTrackOrder = impactIndicatorRows + 1;
   // Create tr trackOrder
   var htmlTrackRow = '<tr class="trackOrderRow" id="impactIndicatorOrderRow_' + (parseInt(impactTrackOrder) + 1) + '"><td>' + impactOrder + '.' + (parseInt(impactIndicatorRows) + 1) + '</td></tr>';
    // Create tr impact Indicator content
   var indicatorName = impactIndicatorRows + '.' + (parseInt(impactIndicatorRows) + 1);
   var rowStyle = "";
   rowStyle = Math.pow(-1, impactIndicatorRows) < 0 ? "nivel01 impar" : "nivel01";
   var htmlContent = "";
   htmlContent += '<tr class="' + rowStyle + '" id="impactIndicatorRow_' + (parseInt(impactTrackOrder) + 1) + '">';
   htmlContent += '<td class="verticalShadow">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeId field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + impactIndicatorRows + '__OutcomeId" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeId" type="hidden" value="' + impactId + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeId">';
   htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + impactIndicatorRows + '__OutcomeIndicatorId" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorId" type="hidden" value="-1" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorId">';
   htmlContent += '<div class="dato01">';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Indicator</legend>';
   htmlContent += '<textarea class="dato01" cols="55" id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__Definition" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].Definition" rows="2"></textarea>';
   htmlContent += '</fieldset>';
   htmlContent += '</div>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Unit of measure</legend>';
   htmlContent += '<label class="editLabel ici" for="unit01">';
   htmlContent += '<input class="input min_input" id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__UnitOfMeasure" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].UnitOfMeasure" type="text" value="">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td>';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Baseline</legend>';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += '<input class="input min_input" data-val="true" data-val-required="The Baseline field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__Baseline" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].Baseline" type="text" value="0">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td class="dato02">';
   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Baseline</legend>';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += '<input class="input min_input" data-val="true" data-val-required="The BaselineYear field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__BaselineYear" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].BaselineYear" type="text" value="0">';
   htmlContent += '</label>';
   htmlContent += '</fieldset>';
   htmlContent += '</td>';
   htmlContent += '<td class="dato07">';
   htmlContent += '<table>';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr>';
   htmlContent += '<td class="">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'P(a)';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td class="">';
   htmlContent += '<label class="editLabel" for="baseline01">';
   htmlContent += 'A';
   htmlContent += '</label>';
   htmlContent += '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '<div class="actionbar" id="actionBar_0">';
   htmlContent += '<a class="btn_awesome first" title="icono mov" href="#"></a>';
   htmlContent += '<a class="btn_entypo first" title="icono back" href="#">🔙</a>';
   htmlContent += '<a class="btn_entypo first" title="icono edit" href="#">⚑</a>';
   var orderName = indicatorName + ', ';
   var nameIndicator = '"Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__Definition",';
   htmlContent += '<a class="btn_entypo first" onclick="deleteImpactIndicator(' + orderName + nameIndicator +
       resultsMatrixId + ', -1,  this, null, null)" title="icono delete" href="#"></a>';
   htmlContent += '<label class="right" for="done">';
   htmlContent += '<input type="button" class="btn-primary small" value="Done" id="done">';
   htmlContent += '</label>';
   htmlContent += '<a class="link right" title="cancel" href="#">Cancel</a>';
   htmlContent += '</div>';
   htmlContent += '</td>';

   // Create the rest of the cells 
   var cells = $("#yearPlans_" + (parseInt(impactTrackOrder) + 1)).find("select option:selected");

   for (var i = 0; i < cells.length; i++) {
      htmlContent += '<td class="dato07">';
      htmlContent += '<table>';
      htmlContent += '<tbody style="border-width:0px">';
      htmlContent += '<tr>';
      htmlContent += '<td>';
      htmlContent += '<label class="editLabel" for="">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorId" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';
      htmlContent += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__Year" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].Year" type="hidden" value="' + $(cells[i]).text() + '">';
      htmlContent += '<input id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1">';
      htmlContent += '<input class="input min_input" data-val="true" data-val-required="The OriginalPlan field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__OriginalPlan" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].OriginalPlan" type="text" value="0">';
      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr>';
      htmlContent += '<td>';
      htmlContent += '<label class="editLabel" for="">';
      htmlContent += '<input class="input min_input" data-val="true" data-val-required="The AnnualPlan field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__AnnualPlan" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].AnnualPlan" type="text" value="0">';
      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '<tr>';
      htmlContent += '<td>';
      htmlContent += '<label class="editLabel" for="">';
      htmlContent += '<input class="input min_input" data-val="true" data-val-required="The ActualValue field is required." id="Outcomes_' + (impactTrackOrder) + '__OutcomeIndicators_' + (impactIndicatorRows) + '__OutcomeIndicatorYearPlans_' + i + '__ActualValue" name="Outcomes[' + (impactTrackOrder) + '].OutcomeIndicators[' + (impactIndicatorRows) + '].OutcomeIndicatorYearPlans[' + i + '].ActualValue" type="text" value="0">';
      htmlContent += '</label>';
      htmlContent += '</td>';
      htmlContent += '</tr>';
      htmlContent += '</tbody>';
      htmlContent += '</table>';
      htmlContent += '</td>';
   }

   htmlContent += '<td id="">';
   htmlContent += '<table>';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr>';
   htmlContent += '<td>&nbsp;</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td>&nbsp;</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td>&nbsp;</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '</td>';
   htmlContent += '<td id="lastCellContent">';
   htmlContent += '<table>';
   htmlContent += '<tbody style="border-width:0px">';
   htmlContent += '<tr>';
   htmlContent += '<td>' + $("#hdnEOP").val() + '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td>' + $("#hdnEOP").val() + '</td>';
   htmlContent += '</tr>';
   htmlContent += '<tr>';
   htmlContent += '<td>' + $("#hdnEOP").val() + '</td>';
   htmlContent += '</tr>';
   htmlContent += '</tbody>';
   htmlContent += '</table>';
   htmlContent += '</td>';

   htmlContent += '</tr>';

   $(gridBody).append(htmlTrackRow + htmlContent);
   var operatorBarLength = $("#operatorBar_" + impactOrder).css("height");
   var operatorBarNewLength = 0;
   operatorBarLength = operatorBarLength.substr(0, operatorBarLength.length - 2);

   operatorBarNewLength = parseInt(operatorBarLength) + 75;
   $("#operatorBar_" + impactOrder).css("height", operatorBarNewLength + "px");


}

function addNewYearPlan(gridBody, rowHead, rowFollowingYear, impacctIndicatorId) {
   // Get index of current impact
   var impactIndex = $(rowFollowingYear).attr("id").substring($(rowFollowingYear).attr("id").length - 1);




   // Get current colspan value of table head
   var currentColSpan = parseInt($(rowFollowingYear).attr("colspan"));
   // Upadte in one the current colspan
   currentColSpan += 1;
   // Update current yera plan rows
   var currentYearsPlanRows = currentColSpan - 2;
   // Get last selected option
   var lastSelectedOption = getLastSelectedOption($(rowHead).find('[id="lastCellHead"]').prev());
   // Update colspan to the thead
   $(rowFollowingYear).attr("colspan", currentColSpan);
   // Add new cell to thead
   var insertedTh = $('<th class="btn_azul_oscuro selects">' + DropDownYearsPlan($(rowHead).find('[id="lastCellHead"]').prev(), impactIndex) + '</th>').insertAfter($(rowHead).find('[id="lastCellHead"]').prev());
   var insertedSelect = $(insertedTh).find("select");
   // Bind kendo clcik event
   $(insertedSelect).kendoDropDownList();
   // Get all rows to the current table
   var impactIndicatorRows = $(gridBody).find('[id^="impactIndicatorRow_"]');
   // Add new Column to the each row of the table
   $.each(impactIndicatorRows, function (index, value) {
      // Get default option to the new select
      lastSelectedListBox = $(rowHead).find("select option:selected").text();
      // Set the current index to cell
      var cellIndex = parseInt($(this).find("input:text").length);

      lastSelectedListBox = $(rowHead).find("select").length;

      var nextIndex = lastSelectedListBox - 1;


      // Se hidden fields
      var hiddenFields = getHiddenFields(impactIndex, index, $("#Outcomes_" + impactIndex + "__OutcomeIndicators_" + index + "__OutcomeIndicatorId").val(), nextIndex, -1, lastSelectedOption);
      // Set content to cell
      var content = "";
      content += '<td class="dato07"> ';
      content += '<table> ';
      content += '<tbody style="border-width:0px"> ';
      content += '<tr> ';
      content += '<td> ' + hiddenFields;
      content += '<label class="editLabel" for=""> ';
      content += '<input class="input min_input" data-referenceyear=' + referenceYear + ' data-val="true" data-val-required="The OriginalPlan field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + index + '__OutcomeIndicatorYearPlans_' + nextIndex + '__OriginalPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + index + '].OutcomeIndicatorYearPlans[' + nextIndex + '].OriginalPlan" type="text" value=""> ';
      content += '</label> ';
      content += '</td> ';
      content += '</tr> ';
      content += '<tr> ';
      content += '<td> ';
      content += '<label class="editLabel" for=""> ';
      content += '<input class="input min_input" data-referenceyear=' + referenceYear + ' data-val="true" data-val-required="The AnnualPlan field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + index + '__OutcomeIndicatorYearPlans_' + nextIndex + '__AnnualPlan" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + index + '].OutcomeIndicatorYearPlans[' + nextIndex + '].AnnualPlan" type="text" value=""> ';
      content += '</label> ';
      content += '</td> ';
      content += '</tr> ';
      content += '<tr> ';
      content += '<td> ';
      content += '<label class="editLabel" for=""> ';
      content += '<input class="input min_input" data-referenceyear=' + referenceYear + ' data-val="true" data-val-required="The ActualValue field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + index + '__OutcomeIndicatorYearPlans_' + nextIndex + '__ActualValue" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + index + '].OutcomeIndicatorYearPlans[' + nextIndex + '].ActualValue" type="text" value=""> ';
      content += '</label> ';
      content += '</td> ';
      content += '</tr> ';
      content += '</tbody> ';
      content += '</table> ';
      content += '</td> ';

      $(content).insertBefore($(this).find('[id="lastCellContent"]').prev());

   });

   // Prueba redimensionar tabla dissagregations
   resizeDissagregationTable(gridBody);
}

function deleteYearColumn(pnlDelete, resultsMatrixId, impactId) {

   // Get current selected option
   var selectedOption = $(pnlDelete).parent("td").prev().find("select option:selected").text();

   if (impactId == null) {

      if (deletedYearIsValid(selectedOption)) {

         var urlRoute = $("#hdnEdit").val();

         // Set Selected postion
         var selectedPosition = 0;

         // Find position of the current select
         $(pnlDelete).closest("tr.rowYearPlan").first().find("select").each(function (index, element) {
            if ($(this).find("option:selected").text() == selectedOption) {
               selectedPosition = index;
               return false;
            }
         });

         var currentSelectArrayLen = $(pnlDelete).closest("tr.rowYearPlan").first().find("select").length;

         // Remove all th indicator
         $("#impactsContainer").find("thead").each(function (index, element) {
            $(this).find("th.selects").eq(selectedPosition).remove();
         });

         // Remove all th dissagregation
         $("#impactsContainer").find(".disagChildHead").each(function (index, element) {
            $(this).find("th.year").eq(selectedPosition).remove();
         });

         // Remove indicator year plan
         $("tr.nivel01").each(function (rowIndex, row) {
            $(row).find('input[type="text"][id$="__OriginalPlan"]').each(function (inputIndex, input) {
               if (inputIndex == selectedPosition) {
                  $(this).closest("td.dato07").first().remove();
               }
            });
         });

         // Remove disaggregation year plan
         $("tr.nivel02 table.disagChildTable tbody.disagChild > tr").each(function (rowIndex, row) {
            $(row).find('input[type="text"][id$="__OriginalPlan"]').each(function (inputIndex, input) {
               if (inputIndex == selectedPosition) {
                  $(this).closest("td.dato07").first().remove();
               }
            });
         });


         // Decrease the year in one
         currentSelectArrayLen -= 1;
      }
   } else {
       var object = {
           'resultsMatrixId': resultsMatrixId,
           'impactId': impactId,
           'year': parseInt(selectedOption)
       };

       confirmAction(deleteYearPlanMessage + titlePendingChanges)
           .done(function (value) {
               if (value) {
                   $.ajax(
                   {
                       url: $("#hdnDeleteYearPost").val(),
                       type: 'GET',
                       data: object,
                       success: function (response) {
                           if (!response.IsValid) {
                               showMessage(errorMessage, false);
                               return false;
                           }

                           idbg.lockUiRM(null, true);
                           location.href = response.Redirect;
                       },
                       error: function () {
                           showMessage(errorMessage, false);
                       }
                   });
               }
           });
   }
}

function DropDownYearsPlan(rowHead, impactIndex, year) {

   var yearsPlanList = $("#listBoxYearsPlan").val();
   var htmlContent = "";
   var htmlSelectContent = "";
   var defaultOption = "";
   var lastSelectedListBox = null;
   var disabledAttr = "";
   var selectedStatus = false;
   var selectedValue = "";

   //Assign selected year
   if (year != "" && year != null) {
      defaultOption = year;
   } else {
      lastSelectedListBox = $(rowHead).prev().find("select option:selected").text();
      defaultOption = (lastSelectedListBox.length > 0) ? (parseInt(lastSelectedListBox) + 1) : new Date().getFullYear();
   }



   $.each($.parseJSON(yearsPlanList), function (index, value) {
      if (value == defaultOption) {

         selectedStatus = true;
         selectedValue = value;
      }

   });


   selectedValue = (selectedValue != null) ? selectedValue : (parseInt(lastSelectedListBox) + 1);


   var isValidated = isValidatedYearPlan(selectedValue);

   if (isValidated) {
      $(rowHead).attr("title", $("#hdnInactiveYearPlanToolTip").val());
   }

   disabledAttr = isValidated ? "disabled='disabled'" : "";




   htmlContent += "<table><tbody style='border-width:0px'><tr><td><div class='filterDropDown selectCustom'>";
   htmlContent += "<ul class='optionList small'>";
   htmlContent += "<li class='small'>";

   htmlSelectContent = "<select class='selectPlanYears" + impactIndex + " optionSelect' onchange = 'validateSelectedOption(" + impactIndex + ", this)' style='display: none;' " + disabledAttr + ">";

   if (selectedStatus) {
      htmlContent += "<span class='k-widget k-dropdown k-header optionSelect selectPlanYears0 mock' unselectable='on' role='listbox' aria-haspopup='true' aria-expanded='false' tabindex='0' aria-owns='' aria-disabled='false' aria-readonly='false' aria-busy='false' style=''>";
      htmlContent += "<span unselectable='on' class='k-dropdown-wrap k-state-default btn'>";
      htmlContent += "<span unselectable='on' class='k-input'>" + selectedValue + "</span>";
      htmlContent += "<span unselectable='on' class='k-select'>";
      htmlContent += "<span unselectable='on' class='k-icon k-i-arrow-s'>select</span>";
      htmlContent += "</span>";
      htmlContent += "</span>";

   } else {
      htmlContent += "<span class='k-widget k-dropdown k-header optionSelect selectPlanYears0 mock' unselectable='on' role='listbox' aria-haspopup='true' aria-expanded='false' tabindex='0' aria-owns='' aria-disabled='false' aria-readonly='false' aria-busy='false' style=''>";
      htmlContent += "<span unselectable='on' class='k-dropdown-wrap k-state-default btn'>";
      htmlContent += "<span unselectable='on' class='k-input'>" + (parseInt(lastSelectedListBox) + 1) + "</span>";
      htmlContent += "<span unselectable='on' class='k-select'>";
      htmlContent += "<span unselectable='on' class='k-icon k-i-arrow-s'>select</span>";
      htmlContent += "</span>";
      htmlContent += "</span>";

   }

   htmlContent += htmlSelectContent;


   $.each($.parseJSON(yearsPlanList), function (index, value) {
      if (value == selectedValue) {
         htmlContent += "<option value='" + value + "' selected = 'selected'>" + value + "</option>";
      }
      else {
         htmlContent += "<option value='" + value + "'>" + value + "</option>";
      }
   });

   htmlContent += "</select>";
   htmlContent += "</span>";
   htmlContent += "</li>";
   htmlContent += "</ul>";
   htmlContent += "</div></td>";
   htmlContent += "<td>";

   if (!isValidated) {
      htmlContent += "<div title='Delete Year' onclick='deleteYearColumn(this,  " + $("#hdnResultsMatrixId").val() + ", null);' class='deleteYear' style='display:none;'>X</div>";
   }

   htmlContent += "</td>";
   htmlContent += "</tr></tbody></table>";

   return htmlContent;
}

function isValidatedYearPlan(year) {
   var isValidated = false;



   if ($("#currentYearsPlan").val() != "null") {

      var currentYearsPlan = $.parseJSON($("#currentYearsPlan").val());
      var yearsFound = $.grep(currentYearsPlan, function (e) { return parseInt(e.Item1) == parseInt(year); });
      isValidated = yearsFound.length > 0 ? yearsFound[0].Item2 : false;
   }

   return isValidated;
}

function DropDownBaseLineYear(impactIndex, impactIndicatorIndex) {
   var yearsPlanList = $("#listBoxYearsPlan").val();

   var htmlContent = "";

   htmlContent += '<fieldset>';
   htmlContent += '<legend class="hide">Dropdown con opciones</legend>';
   htmlContent += '<ul class="optionList">';
   htmlContent += '<li class="small">';

   htmlContent += '<span class="k-widget k-dropdown k-header optionSelect optionSelectYearsPlan" unselectable="on" role="listbox" aria-haspopup="true" aria-expanded="false" tabindex="0" aria-owns="Impacts_' + impactIndex + '__ImpactIndicators_' + impactIndicatorIndex + '__BaselineYear_listbox" aria-disabled="false" aria-readonly="false" aria-busy="false" aria-activedescendant="Impacts_' + impactIndex + '__ImpactIndicators_' + impactIndicatorIndex + '__BaselineYear_option_selected" style="">';
   htmlContent += '<span unselectable="on" class="k-dropdown-wrap k-state-default btn">';
   htmlContent += '<span unselectable="on" class="k-input">' + new Date().getFullYear() + '</span>';
   htmlContent += '<span unselectable="on" class="k-select">';
   htmlContent += '<span unselectable="on" class="k-icon k-i-arrow-s">select</span>';
   htmlContent += '</span>';
   htmlContent += '</span>';
   htmlContent += '<select class="optionSelect optionSelectYearsPlanNew" data-val="true" onchange = "setValue(this);"  data-val-required="The BaselineYear field is required." id="Impacts_' + impactIndex + '__ImpactIndicators_' + impactIndicatorIndex + '__BaselineYear" name="Impacts[' + impactIndex + '].ImpactIndicators[' + impactIndicatorIndex + '].BaselineYear" data-role="dropdownlist" style="display: none;">';

   htmlContent += "<option value='" + new Date().getFullYear() + "' selected = 'selected'>" + new Date().getFullYear() + "</option>";

   $.each($.parseJSON(yearsPlanList), function (index, yearPlan) {
      if (yearPlan !== new Date().getFullYear()) {
         htmlContent += "<option value='" + yearPlan + "'>" + yearPlan + "</option>";
      }
   });

   htmlContent += '</select>';
   htmlContent += '</span>';
   htmlContent += '</li>';
   htmlContent += '</ul>';
   htmlContent += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__BaselineYear" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].BaselineYear" type="hidden" value="' + new Date().getFullYear() + '">';
   htmlContent += '</fieldset>';

   return htmlContent;
}

function setValue(select) {
   $(select).closest("fieldset").find('input[type="hidden"][id$="__BaselineYear"]').val($(select).find("option:selected").text())
}

function isValidYearPlan(select) {
   // Search flag
   var exist = false;

   // Search count
   var itemCount = 0;

   // Get the current row
   var rowHead = $(select).parent().parent().parent().parent().parent().parent().parent().parent().parent().parent();

   // Get all select elements
   var selects = $(rowHead).find("select");

   // Retrieve selected option
   var optionSelected = $(select).find("option:selected").text();

   // Validate every selected option
   $.each(selects, function (index, selectItem) {
      var option = $(selectItem).find("option:selected").text();
      if (option == optionSelected) {
         itemCount++;
      }
   });

   exist = (itemCount > 1) ? false : true;

   return exist;
}

function validateSelectedOption(impactIndex, select) {
   // Get current row
   var rowHead = $(select).closest("tr.rowYearPlan");

   // Get current cell of selected combo
   var thCell = $(select).closest("th.selects");

   // Get position within the header of current select combo (remove 5 to compensate for "following year" removal, now all th are on the same level)
   var currentSelectPos = $(rowHead).find("th").index(thCell) - 5;

   // Get all select combo elements
   var selects = $(rowHead).find("select");

   // Retrieve selected option
   var optionSelected = $(select).find("option:selected").text();

   // Verify if the new selected option already exists in the collection
   var found = false;
   var existingOptions = new Array();
   $.each(selects, function (index, selectItem) {
      if (index != currentSelectPos) {

         var option = $(selectItem).find("option:selected").text();
         //if (option.toLowerCase() == optionSelected.toLowerCase()) {
         if (option == optionSelected) {
            found = true;
         }
         existingOptions.push(parseInt(option));
      }
   });

   var fixedOption = "";
   var previousOption = "";

   // Search all input year and set new option selected
   var indicatorTable = $(rowHead).parents("table");

   // If the year already exists
   if (found) {
      //ROLLBACK to the previous option
      // Get previous option selected
      var yearData = $(indicatorTable).find(".yearData_" + currentSelectPos).first();
      var seq = $(yearData).attr("seq");
      if (seq == null) {
         //If the column does'nt exist in the database we use the index of the cell instead of the sequence.
         seq = currentSelectPos;
      }


      var yearPlanInputs = $(indicatorTable).find("input[id$=" + seq + "__Year]");
      // Set previous option selected
      previousOption = $(yearPlanInputs[0]).val();
      fixedOption = previousOption;//Math.max.apply(Math, existingOptions) + 1;

      // Show the previous selected option 
      $(select).parent().find(".k-input").text(fixedOption);
      var options = $(select).find("option");

      $.each(options, function (index, optionItem) {
         if ($(optionItem).val() == previousOption) {
            $(optionItem).attr("selected", "selected");
         }
      });

      showMessage("The year " + optionSelected + " already exists");

   } else {

      // Set all selects to the selected year
      fixedOption = optionSelected;

      // SET INDICATORS INPUTS TO THE SELECTED YEAR
      var yearData = $("#impactsContainer").find(".yearData_" + currentSelectPos).first();
      var seq = $(yearData).attr("seq");
      if (seq == null) {
         //If the column does'nt exist in the database we use the index of the cell instead of the sequence.
         seq = currentSelectPos;
      }

      $("#impactsContainer").find("input[id$=" + seq + "__Year]").val(fixedOption);

      // SET DISAGGREGATIONS INPUTS TO SELECTED YEAR
      var yearDataDis = $("#impactsContainer").find(".yearDataDis_" + currentSelectPos).first();
      var seqDis = $(yearDataDis).attr("seq");
      if (seqDis == null) {
         //If the column does'nt exist in the database we use the index of the cell instead of the sequence.
         seqDis = currentSelectPos;
      }
      $("#impactsContainer").find("input[id$=OutcomeDisaggregationYearPlans_" + seq + "__Year]").val(fixedOption);

      // CHANGE HEADERS IN COMBOBOXES AND DISAGGREGATIONS
      $.each($(".rowYearPlan"), function (index, optionItem) {

         var text = $(optionItem).find(".k-input").eq(currentSelectPos);
         $(text).text(fixedOption);
         var options = $(text).closest(".k-dropdown").find("option");

         $.each(options, function (index2, optionItem2) {
            if ($(optionItem2).val() == fixedOption) {
               $(optionItem2).attr("selected", "selected");
            }
         });



      });

      $.each($(".headRowDisaggregationYears"), function (index, optionItem) {
         $(optionItem).find("th").eq(currentSelectPos + 2).html(fixedOption)
      });

      // BLOCK FIELDS
      var interval = new Object();
      interval.IntervalId = $("#hdnIntervalId").val();
      interval.CycleId = $("#hdnCycleId").val();

      $.each($(".nivel01"), function (index, optionItem) {

         var td = $(optionItem).find(".dato07").eq(currentSelectPos);

         var isPBlocked = BlockFieldStartUpPlan(interval);
         var isPABlocked = BlockFieldAnnualPlan(interval, fixedOption);
         var isACBlocked = BlockFieldActualPlan(interval, fixedOption);

         var pCell = $(td).find(".rm_p_row").find("input.min_input");
         var paCell = $(td).find(".rm_pa_row").find("input.min_input");
         var acCell = $(td).find(".rm_ac_row").find("input.min_input");

         if (isPBlocked) {
             $(pCell).prop("readonly");
             $(pCell).addClass("bcGray")
         } else {
             $(pCell).removeProp("readonly");
             $(pCell).removeClass("bcGray")
         }
         if (isPABlocked) {
             $(paCell).prop("readonly");
             $(paCell).addClass("bcGray")
         } else {
             $(paCell).removeProp("readonly");
             $(paCell).removeClass("bcGray")
         }
         if (isACBlocked) {
             $(acCell).prop("readonly");
             $(acCell).addClass("bcGray")
         } else {
             $(acCell).removeProp("readonly");
             $(acCell).removeClass("bcGray")
         }
      });
   }
}

function validateSelectedOption2(impactIndex, select) {

   // Get current row
   var rowHead = $(select).parent().parent().parent().parent().parent().parent().parent().parent().parent().parent();

   // Get current cell of selected combo
   var thCell = $(select).parent().parent().parent().parent().parent().parent().parent().parent().parent();

   // Get position of currenbt select
   var currentSelectPos = $(rowHead).find("th.selects").index(thCell);

   // Get all select elements
   var selects = $(rowHead).find("select");

   // Retrieve selected option
   var optionSelected = $(select).find("option:selected").text();

   // Verify if exist previous option selected in the collection
   var found = false;
   var existingOptions = new Array();
   $.each(selects, function (index, selectItem) {
      if (index != currentSelectPos) {

         var option = $(selectItem).find("option:selected").text();
         if (option.toLowerCase() == optionSelected.toLowerCase()) {
            found = true;
         }
         existingOptions.push(parseInt(option));
      }
   });

   var fixedOption = "";
   var previousOption = "";

   // Search all input year and set new option selected
   var indicatorTable = $(rowHead).parents("table");

   // If exist
   if (found) {
      // Get previous option selected
      var yearPlanInputs = $(indicatorTable).find("input[id$=" + currentSelectPos + "__Year]");
      // Set previous option selected
      previousOption = $(yearPlanInputs[0]).val();
      fixedOption = previousOption;//Math.max.apply(Math, existingOptions) + 1;
   } else {
      fixedOption = optionSelected;
   }

   // Set new option selected
   $(indicatorTable).find("input[id$=" + currentSelectPos + "__Year]").val(fixedOption);

   // Change header year of dissagregation
   var dissagregationHeaderYear = $(indicatorTable).find(".disagChildHead tr").eq(1).find("th").eq(currentSelectPos);

   if (dissagregationHeaderYear != null) {
      $(dissagregationHeaderYear).html(optionSelected);
   }

   if (found) {
      // Show the prevous option selected
      $(select).parent().find(".k-input").text(fixedOption);

      var options = $(select).find("option");

      $.each(options, function (index, optionItem) {
         if ($(optionItem).val().toLowerCase() == previousOption.toLowerCase()) {
            $(optionItem).attr("selected", "selected");
         }
         if ($(optionItem).val().toLowerCase() == optionSelected.toLowerCase()) {
            $(optionItem).removeAttr("selected");
         }
      });



      // Set dialog message
      alert("The year " + optionSelected + " really exists");



      //$(".test").fbmodal({
      //    title: "FaceBook Modal Demo",
      //    cancel: "Cancel",
      //    okay: "Okay",
      //    okaybutton: true,
      //    cancelbutton: true,
      //    buttons: true,
      //    opacity: 0.0,
      //    fadeout: true,
      //    overlayclose: true,
      //    modaltop: "30%",
      //    modalwidth: "400"
      //});



   } else {

      // Update dropdown
      $("thead > tr.rowYearPlan").each(function (rowIndex, tRow) {
         $(tRow).find("select.optionSelect").each(function (indexTh, th) {
            if (indexTh == currentSelectPos) {
               $(th).prev().find("span.k-input").first().text(optionSelected);
               $(th).find("option:selected").text(optionSelected);
               $(th).find("option:selected").val(optionSelected);
            }
         });
      });

      // Block or unblock fields in accordance with the selected option
      var interval = new Object();
      interval.IntervalId = $("#hdnIntervalId").val();
      interval.CycleId = $("#hdnCycleId").val();

      // Update year of input hidden
      // Reorder IndicatorYearPlan index
      $("tr.nivel01").each(function (rowIndex, row) {



         // Get all impacta indicator and impact dissgregation original input year plan
         //$(row).find('input[type="text"][id$="__OriginalPlan"]').each(function (indInputIndex, input) {

         //    if (indInputIndex == currentSelectPos) {
         //        // Update indicatoryear index
         //        var year = $(this).closest("td").first().find('input[type="hidden"][id$="__Year"]');
         //        $(year).val(optionSelected);

         //        var isBlocked = BlockFieldStartUpPlan(interval);

         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        } else {
         //            $(this).removeAttr("disabled");
         //        }

         //        //return false;
         //    }
         //});

         var original = $(row).find('input[type="text"][id$="__OriginalPlan"]').eq(currentSelectPos);
         // Update indicatoryear index
         var yearOriginal = $(original).closest("td").first().find('input[type="hidden"][id$="__Year"]');
         $(yearOriginal).val(optionSelected);

         var isBlockedOriginal = BlockFieldStartUpPlan(interval);

         if (isBlockedOriginal) {
            $(original).attr("disabled", "disabled");
         } else {
            $(original).removeAttr("disabled");
         }

         var annual = $(row).find('input[type="text"][id$="__AnnualPlan"]').eq(currentSelectPos);

         var isBlockedAnnual = BlockFieldAnnualPlan(interval, optionSelected);

         if (isBlockedAnnual) {
            $(annual).attr("disabled", "disabled");
         } else {
            $(annual).removeAttr("disabled");
         }


         var actual = $(row).find('input[type="text"][id$="__ActualValue"]').eq(currentSelectPos);

         var isBlockedActual = BlockFieldActualPlan(interval, optionSelected);

         if (isBlockedActual) {
            $(actual).attr("disabled", "disabled");
         } else {
            $(actual).removeAttr("disabled");
         }


         //$(row).find('input[type="text"][id$="__AnnualPlan"]').each(function (dissInputIndex, input) {
         //    if (dissInputIndex == currentSelectPos) {
         //        var isBlocked = BlockFieldAnnualPlan(interval, optionSelected);
         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        } else {
         //            $(this).removeAttr("disabled");
         //        }


         //    }
         //});

         //$(row).find('input[type="text"][id$="__ActualValue"]').each(function (dissInputIndex, input) {
         //    if (dissInputIndex == currentSelectPos) {
         //        var isBlocked = BlockFieldActualPlan(interval, optionSelected);
         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        } else {
         //            $(this).removeAttr("disabled");
         //        }

         //    }
         //});




      });

      $("tr.nivel02 > table.disagChildTable tbody.disagChild > tr").each(function (rowIndex, row) {


         var original = $(row).find('input[type="text"][id$="__OriginalPlan"]').eq(currentSelectPos);
         // Update indicatoryear index
         var yearOriginal = $(original).closest("td").first().find('input[type="hidden"][id$="__Year"]');
         $(yearOriginal).val(optionSelected);

         var isBlockedOriginal = BlockFieldStartUpPlan(interval);

         if (isBlockedOriginal) {
            $(original).attr("disabled", "disabled");
         } else {
            $(original).removeAttr("disabled");
         }

         var annual = $(row).find('input[type="text"][id$="__AnnualPlan"]').eq(currentSelectPos);

         var isBlockedAnnual = BlockFieldAnnualPlan(interval, optionSelected);

         if (isBlockedAnnual) {
            $(annual).attr("disabled", "disabled");
         } else {
            $(annual).removeAttr("disabled");
         }


         var actual = $(row).find('input[type="text"][id$="__ActualValue"]').eq(currentSelectPos);

         var isBlockedActual = BlockFieldActualPlan(interval, optionSelected);

         if (isBlockedActual) {
            $(actual).attr("disabled", "disabled");
         } else {
            $(actual).removeAttr("disabled");
         }


         // Get all impacta indicator and impact dissgregation original input year plan
         //$(row).find('input[type="text"][id$="__OriginalPlan"]').each(function (dissInputIndex, input) {
         //    if (dissInputIndex == currentSelectPos)
         //    {
         //        // Update indicatoryear index
         //        var year = $(this).closest("td").first().find('input[type="hidden"][id$="__Year"]');
         //        $(year).val(optionSelected);

         //        var isBlocked = BlockFieldStartUpPlan(interval);

         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        }


         //    }
         //});

         //$(row).find('input[type="text"][id$="__AnnualPlan"]').each(function (dissInputIndex, input) {
         //    if (dissInputIndex == currentSelectPos) {
         //        var isBlocked = BlockFieldAnnualPlan(interval, optionSelected);
         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        }

         //    }
         //});

         //$(row).find('input[type="text"][id$="__ActualValue"]').each(function (dissInputIndex, input) {
         //    if (dissInputIndex == currentSelectPos) {
         //        var isBlocked = BlockFieldActualPlan(interval, optionSelected);
         //        if (isBlocked) {
         //            $(this).attr("disabled", "disabled");
         //        }

         //    }
         //});

      });
   }

}

function getHiddenFields(impactIndex, impactIndicatorIndex, impactIndicatorId, impactIndicatorYearPlanIndex, impactIndicatorYearPlanId, year) {
   var hiddenFields = "";
   hiddenFields += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_Index" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans.Index" type="hidden" value="' + impactIndicatorYearPlanIndex + '">';
   hiddenFields += '<input data-val="true" data-val-required="The OutcomeIndicatorId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_' + impactIndicatorYearPlanIndex + '__OutcomeIndicatorId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorId" type="hidden" value="' + impactIndicatorId + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorId">';
   hiddenFields += '<input data-val="true" data-val-required="The OutcomeIndicatorYearPlanId field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_' + impactIndicatorYearPlanIndex + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorYearPlanId" type="hidden" value="' + impactIndicatorYearPlanId + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorYearPlanId">';
   hiddenFields += '<input data-val="true" data-val-required="The Year field is required." id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_' + impactIndicatorYearPlanIndex + '__Year" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].Year" type="hidden" value="' + year + '" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].Year">';
   hiddenFields += '<input id="Outcomes_' + impactIndex + '__OutcomeIndicators_' + impactIndicatorIndex + '__OutcomeIndicatorYearPlans_' + impactIndicatorYearPlanIndex + '__OutcomeIndicatorYearPlanId" name="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorYearPlanId" type="hidden" value="-1" style="background-color:#000000;color:#FFFFFF" title="Outcomes[' + impactIndex + '].OutcomeIndicators[' + impactIndicatorIndex + '].OutcomeIndicatorYearPlans[' + impactIndicatorYearPlanIndex + '].OutcomeIndicatorYearPlanId">';
   return hiddenFields;
}

function getLastSelectedOption(rowHead) {
   var lastSelectedListBox = "";
   lastSelectedListBox = $(rowHead).find("select option:selected").text();
   defaultOption = (lastSelectedListBox != null) ? (parseInt(lastSelectedListBox) + 1) : new Date().getFullYear();
   return defaultOption;
}

function reassignIndicator(resultsMatrixId, impactId, impactIndicatorId) {
   // Set route to controller
   var urlRoute = $("#hdnReassignIndicatorUrl").val() + "?resultsMatrixId=" + resultsMatrixId + "&impactId=" + impactId + "&impactIndicatorId=" + impactIndicatorId;

   // Render content view
   $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

   var dialog = $(".dinamicModal").kendoWindow({
      width: "800px",
      title: "Reassing Indicator",
      draggable: false,
      resizable: false,
      content: urlRoute,
      pinned: true,
      actions: [
          "Close"
      ],
      modal: true,
      visible: false,
      open: function () {
          kendoWindowCenter($(".dinamicModal"));
      },
      close: function () {
         $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
         $(".ui-widget-overlay").remove();
         $(".k-window").remove();
      }
   }).data("kendoWindow");
   dialog.center();
   dialog.open();

   $("body").css("overflow", "hidden");
   var cerrar = $(".k-window-action");
   cerrar.click(function () {
      $(".k-window").hover(
          function () {
             document.onmousewheel = document.onmousedown = wheel;
             document.onkeydown = keydown;
          },
          function () {
             document.onmousewheel = document.onmousedown = document.onkeydown = null;
          }
      );
      $("body").css("overflow", "");
      
   });


}

function linkToPredefinedIndicator(resultsMatrixId, impactId, impactIndicatorId) {
   // Set route to controller
   var urlRoute = $("#hdnLinkPredefinedIndicatorUrl").val() + "?resultsMatrixId=" + resultsMatrixId + "&impactId=" + impactId + "&impactIndicatorId=" + impactIndicatorId;

   // Render content view
   $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="ui-widget-overlay ui-front"></div>');
   $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

   var dialog = $(".dinamicModal").kendoWindow({
      width: "800px",
      title: "Search and link pre-defined Indicator",
      draggable: false,
      resizable: false,
      content: urlRoute,
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
   dialog.center();
   dialog.open();
}

/**************************************************************************************************************************************/
/******************************************************* DISAGGREGATION FUNCTIONS *****************************************************/
/**************************************************************************************************************************************/
function resizeDissagregationTable(gridBody) {

   // Get all td cells linked to current tbody/impact/outcome
   var cellDissagregations = $(gridBody).find(".spanCell");

   $.each(cellDissagregations, function (index, cell) {
      // Get current colspan of td
      var currentColspan = parseInt($(cell).attr("colspan"));
      // Set new colspan attribute
      $(cell).attr("colspan", (currentColspan + 1));
   });

}
function addNewYearPlanDissagregation(impactIndicatorBody, year, impactIndex) {

   // Get all disaggregation body
   var disaggregationsTables = $(impactIndicatorBody).find(".disagChild");

   // Get count of disaggregation
   var disaggCount = 0;



   $.each(disaggregationsTables, function (index, disaggregationTable) {
      // Find row head node
      var disaggregationHeadRow = $(disaggregationTable).parent().find(".headRowDisaggregationYears");

      // Remove empty column
      // $(disaggregationHeadRow).find("th:empty").remove();

      // Get Current colspan in one
      // var currentColSpan = parseInt($(disaggregationTable).parent().find("#followingYearDisag").attr("colspan"));



      // Get number of years
      var headYears = $(disaggregationHeadRow).find(".year").length;

      // Increase in one colspan attr
      //$(disaggregationTable).parent().find("#followingYearDisag").attr("colspan", (headYears + 1));

      // Add thead to the table
      if (headYears <= 0) {
         $("<th class='btn_azul_oscuro dato04 year'>" + year + "</th>").insertAfter($(disaggregationTable).parent().find(".headRowDisaggregationYears").find(".colIni").last());
      } else {
         // Insert new head cell after last cell
         $("<th class='btn_azul_oscuro dato04 year'>" + year + "</th>").insertAfter($(disaggregationTable).parent().find(".headRowDisaggregationYears").find(".year").last());
      }


      // Get all rows of the current table
      var disaggregationRows = $(disaggregationTable).find(".nivel01");

      var countDisagRows = 0;


      // Add new colum to every row of every disaggregation table

      $(disaggregationRows).find("td:empty").remove();

      $.each(disaggregationRows, function (index, disaggregationRow) {

         var indicatorId = -1;
         var disaggregationId = -1;

         var disagHiddens = $(disaggregationRow).find("textarea").parent().find("input[type=hidden]");

         if (disagHiddens.length > 1) {
            indicatorId = $(disagHiddens[0]).val();
            disaggregationId = $(disagHiddens[1]).val();
         }

         // Get count of td disaggregation years
         var disaggYearsCount = headYears; //(parseInt($(disaggregationRow).find(".disaggYear").length) > 0) ? (parseInt($(disaggregationRow).find(".disaggYear").length) - 1) : 0;

         var nextDissagIndex = (disaggYearsCount > 0) ? ((disaggYearsCount + 1) - 1) : 0;

         //Add new cell to current row
         var dissagYearCellRowContent = getNewDisagregationYearCellRow(year, impactIndex, disaggCount, countDisagRows, nextDissagIndex, indicatorId, disaggregationId);

         // Remove empty column first time to add new year

         if (headYears < 1) {
            //$($(disaggregationRow).children().eq(2)).remove();
         }

         $(dissagYearCellRowContent).insertBefore($(disaggregationRow).find(".disaggYearEmpty"));

         //$(dissagYearCellRowContent).insertAfter($(disaggregationRow).find(".disaggYear").last());
         //$(dissagYearCellRowContent).insertBefore($(disaggregationRow).children().last());


         countDisagRows++;
      });

      disaggCount++;

   });

}
function getNewDisagregationYearCellHead(year) {
   var htmlContent = "<th class='btn_azul_oscuro dato04'>" + year + "</th>";

   return htmlContent;
}

function getNewDisagregationYearCellRow(year, impactIndex, indicatorIndex, disaggregationtIndex, yearIndex, indicatorId, disaggId) {
   // Set Interval Object 
   var interval = new Object();
   interval.IntervalId = $("#hdnIntervalId").val();
   interval.CycleId = $("#hdnCycleId").val();

   // to set active or inactive state of controls
   var displayModeStartUpPlan = false; var displayModeAnnualPlan = false; var displayModeActualPlan = false;

   displayModeStartUpPlan = BlockFieldStartUpPlan(interval);
   displayModeAnnualPlan = BlockFieldAnnualPlan(interval, year);
   displayModeActualPlan = BlockFieldActualPlan(interval, year);

   var type = $('#typeOfModule').val();
   var referenceYear = parseInt($('[name="referenceYear"]').val());

   var htmlContent = "";
   htmlContent += "<td class='dato07 disaggYear' data-rowcolumnrelated=" + year + ">";
   htmlContent += '<input class="hiddenYear" id="Outcomes_' + impactIndex + '__OutcomeIndicators_' +
       indicatorIndex + '__OutcomeIndicatorYearPlans_' + disaggregationtIndex + '__Year" name="Outcomes[' +
       impactIndex + '].OutcomeIndicators[' + indicatorIndex + '].OutcomeIndicatorYearPlans[' +
       disaggregationtIndex + '].Year" type="hidden" value=' + year + '>';
   htmlContent += '<table class="ouputTable">';
   htmlContent += "<tbody style='border-width:0px'>";
   htmlContent += "<tr class='rm_p_row' data-autocalculatedrelated=rm_p_row_" + year + ">";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for=''>";
   htmlContent += "<input id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_Index' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeDisaggregationYearPlans.Index' type='hidden' value='" + yearIndex + "'>";
   htmlContent += "<input id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeIndicatorId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeIndicatorId' type='hidden' value='" + indicatorId + "'>";
   htmlContent += "<input id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeDisaggregationId' type='hidden' value='" + disaggId + "'>";
   htmlContent += "<input data-val='true' data-val-required='The OutcomeDisaggregationId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" + yearIndex + "__OutcomeDisaggregationId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeDisaggregationYearPlans[" + yearIndex + "].OutcomeDisaggregationId' type='hidden' value='" + disaggId + "'>";
   htmlContent += "<input data-val='true' data-val-required='The OutcomeDisaggregationYearPlanId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" + yearIndex + "__OutcomeDisaggregationYearPlanId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeDisaggregationYearPlans[" + yearIndex + "].OutcomeDisaggregationYearPlanId' type='hidden' value='-1'>";
   htmlContent += "<input data-val='true' data-val-required='The Year field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" + yearIndex + "__Year' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex + "].OutcomeDisaggregationYearPlans[" + yearIndex + "].Year' type='hidden' value='" + year + "'>";

   htmlContent += "<input class='input min_input" + (displayModeStartUpPlan ? " bcGray" : "") +
        " auto' data-yearcolumn=" + year + " data-referenceyear=" + referenceYear +
        " id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex +
        "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" +
        yearIndex + "__OriginalPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" +
        indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex +
        "].OutcomeDisaggregationYearPlans[" + yearIndex + "].OriginalPlan' type='text' " +
        "data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value=''" +
        (displayModeStartUpPlan ? " readonly='readonly'" : "") + ">";

   htmlContent += "</label>";
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "<tr class='rm_pa_row' data-autocalculatedrelated=rm_pa_row_" + year + ">";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for=''>";

   htmlContent += "<input class='input min_input" + (displayModeAnnualPlan ? " bcGray" : "") +
        " auto' data-yearcolumn=" + year + " data-referenceyear=" + referenceYear +
        " id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex +
        "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" +
        yearIndex + "__AnnualPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" +
        indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex +
        "].OutcomeDisaggregationYearPlans[" + yearIndex + "].AnnualPlan' type='text' " +
        "data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value=''" +
        (displayModeAnnualPlan ? " readonly='readonly'" : "") + ">";

   htmlContent += "</label>";
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "<tr class='rm_ac_row' data-autocalculatedrelated=rm_a_row_" + year + ">";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for=''>";

   htmlContent += "<input class='input min_input" + (displayModeActualPlan ? " bcGray" : "") +
       " auto' data-yearcolumn=" + year + " data-referenceyear=" + referenceYear +
       " id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex +
       "__OutcomeDisaggregations_" + disaggregationtIndex + "__OutcomeDisaggregationYearPlans_" +
       yearIndex + "__ActualValue' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" +
       indicatorIndex + "].OutcomeDisaggregations[" + disaggregationtIndex +
       "].OutcomeDisaggregationYearPlans[" + yearIndex + "].ActualValue' type='text' " +
        "data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value=''" +
        (displayModeActualPlan ? " readonly='readonly'" : "") + ">";

   htmlContent += "</label>";
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "</tbody>";
   htmlContent += "</table>";
   htmlContent += "</td>";

   return htmlContent;
}

function addNewDisaggregationDynamic(btnAddDisaggregation, impactIndex, indicatorIndex, indicatorId) {
   addNewDisaggregation(btnAddDisaggregation, impactIndex, indicatorIndex, indicatorId, true, false);
}
function addNewDisaggregation(btnAddDisaggregation, impactIndex, indicatorIndex, indicatorId, isDynamic, isLinked, isAutoCalc) {

   if (!isLinked) {
       //This function assumes that TH headers are already created 
      var disaggregationGridBody = $(btnAddDisaggregation).closest("tr.nivel01").first().next().find("tbody.disagChild");

      // If dissagregation is hidden, show it
      $(disaggregationGridBody).closest("div.child").first().show();
      $(disaggregationGridBody).closest("table.disagChildTable").first().show();
      // If dissagregation is zero, change to display block
      $(disaggregationGridBody).closest("div.child").find("div.minimizeBar").first().show();

      // Get year plan thead from the parent indicator
      var yearsPlanIndicatorHead = $(disaggregationGridBody).parent().parent().parent().parent().parent().parent().parent().find("thead").first();

      // Get all dropdown year selects
      var yearsRowSelect = $(yearsPlanIndicatorHead).find(".rowYearPlan").find("select");

      var years = new Array();

      $.each(yearsRowSelect, function (index, yearPlan) {
         years.push($(yearPlan).find("option:selected").text());
      });

      // SET UP NEW COLUMNS IN THE DOM IF NECESSARY
      // count years
      if (isDynamic == true) {
         var disYears = $(disaggregationGridBody).parent().find(".headRowDisaggregationYears");
         var numYears = $(disYears).find(".dato04").length - 2;
         if (numYears != years.length) {
            $.each(years, function (index, year) {
               if (numYears <= 0) {
                  $("<th class='btn_azul_oscuro dato04 year'>" + year + "</th>").insertAfter($(disYears).find(".colIni").last());
               } else {
                  $("<th class='btn_azul_oscuro dato04 year'>" + year + "</th>").insertAfter($(disYears).find(".year").last());
               }
               numYears = numYears + 1;
            });
         }
      }

      // Get next index to the new disaggregation
      var disaggRows = $(disaggregationGridBody).find(".nivel01").length;
      var nextIndex = (disaggRows > 0) ? ((disaggRows + 1) - 1) : 0;


      // Set zebra row style
      var rowStyle = "";

      if (nextIndex <= 0) rowStyle = "nivel01";

      var rowStyle = (Math.pow(-1, (nextIndex + 1)) > 0) ? "nivel01" : "nivel01 impar"

      $(disaggregationGridBody).append(getNewDisaggregationRow(rowStyle, years, impactIndex, indicatorIndex, nextIndex, indicatorId, -1, isAutoCalc));

      setAutonumericFormat(disaggregationGridBody);

      // Get and set current height of table to operatorbar
      var dissaGregationTableHeight = $(disaggregationGridBody).closest(".tableGrid").first().height();
      $(disaggregationGridBody).closest(".tableGrid").prev().height(dissaGregationTableHeight);

      // Remake validation
      var form = $("#outcomeEditForm")
          .removeData("validator") /* added by the raw jquery.validate plugin */
          .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */
      $.validator.unobtrusive.parse(form);

      //================ Init Autonumeric plugin ==================================================
      $('.auto').autoNumeric('init');
   } else {
      var message = $("#hdnImpactsOutcomesAddNewDisaggregationLinkedMessage").val();
      var position = parseInt($(btnAddDisaggregation).offset().top);

      new $.Zebra_Dialog(message, {
         'title': 'Warning',
         'buttons': true,
         'modal': false,
         'auto_close': false,
         'animation_speed_hide': 0,
         'type': false
      });

      $(".ZebraDialog").css('position', 'absolute');
      $(".ZebraDialog").css('top', (position - 78) + 'px');
   }


}
function getNewDisaggregationRow(rowStyle, years, impactIndex, indicatorIndex, disaggIndex, indicatorId, disaggId, isAutoCalc) {
   // Create content of Basic Data Disaggregation
   var htmlContent = '';
   htmlContent += "<tr class='" + rowStyle + "' id='' onmouseover='showActionBar(this);' onmouseout='hideActionBar(this);' >";
   htmlContent += "<td class='dato01'>";
   htmlContent += "<input id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeIndicatorId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeIndicatorId' type='hidden' value='1'>";
   htmlContent += "<input id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_Index' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations.Index' type='hidden' value='" + disaggIndex + "'>";
   htmlContent += "<div class=''>";
   htmlContent += "<input data-val='true' data-val-required='The OutcomeIndicatorId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__OutcomeIndicatorId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].OutcomeIndicatorId' type='hidden' value='" + indicatorId + "'>";
   htmlContent += "<input data-val='true' data-val-required='The OutcomeDisaggregationId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__OutcomeDisaggregationId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].OutcomeDisaggregationId' type='hidden' value='" + disaggId + "'>";
   htmlContent += "<textarea class='dato01' cols='55' data-val='true' data-val-required='Please enter a title for the type of disaggregation' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__DisaggregationName' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].DisaggregationName' rows='2'></textarea>";
   htmlContent += "</div>";
   htmlContent += "</td>";
   htmlContent += "<td>";
   htmlContent += '<table class="ouputTable">';
   htmlContent += "<tbody style='border-width:0px'>";
   htmlContent += "<tr>";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for='baseline01'>";
   htmlContent += 'P';
   htmlContent += '</label>';
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "<tr>";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for='baseline01'>";
   htmlContent += 'P(a)';
   htmlContent += '</label>';
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "<tr>";
   htmlContent += "<td class='rm_edit_rh'>";
   htmlContent += "<label class='editLabel' for='baseline01'>";
   htmlContent += 'A';
   htmlContent += '</label>';
   htmlContent += "</td>";
   htmlContent += "</tr>";
   htmlContent += "</tbody>";
   htmlContent += "</table>";
   htmlContent += "<div class='actionBarDisagg' id='actionBarDisagg_00' style='display: none;'>";
   htmlContent += "<a class='btn_entypo first' onclick='deleteDissagregation(this, " + $("#hdnResultsMatrixId").val() + ", " + disaggId + ",  null, false)' title='" + $("#hdnDeleteDissagregationToolTip").val() + "' href='#'></a>";
   htmlContent += "</div>";
   htmlContent += "</td>";

   // Create content of yearcolumns
   var yearColumnContent = "";
   for (var i = 0; i < years.length; i++) {
      yearColumnContent += getNewDisagregationYearCellRow(years[i], impactIndex, indicatorIndex, disaggIndex, i, indicatorId, disaggId);
   }

   htmlContent += yearColumnContent;

   // Add the empty column
    htmlContent += '<td></td>';

   // Set Interval Object 
   var interval = new Object();
   interval.IntervalId = $("#hdnIntervalId").val();
   interval.CycleId = $("#hdnCycleId").val();

   // to set active or inactive state of controls
   var displayModeEOPStartUpPlan = false; var displayModeEOPAnnualPlan = false; var displayModeEOPActualPlan = false;
   displayModeEOPStartUpPlan = BlockFieldStartUpPlanEOP(interval);
   displayModeEOPAnnualPlan = BlockFieldAnnualPlanEOP(interval, -1);
   displayModeEOPActualPlan = BlockFieldActualPlanEOP(interval, -1);

   var EOPHtmlContent = "";
   EOPHtmlContent += '<td class="dato07 totalMilestoneYearPlan">';
   EOPHtmlContent += '<table class="ouputTable">';
   EOPHtmlContent += "	<tbody style='border-width:0px'>";
   EOPHtmlContent += "		<tr class='rm_p_row'>";
   EOPHtmlContent += "			<td class='rm_edit_rh'>";
   EOPHtmlContent += "				<label class='editLabel' for=''>";
   EOPHtmlContent += "					<input data-val='true' data-val-required='The OutcomeDisaggregationId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_OutcomeDisaggregationId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.OutcomeDisaggregationId' type='hidden' value='0'>";
   EOPHtmlContent += "					<input data-val='true' data-val-required='The OutcomeDisaggregationYearPlanId field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_OutcomeDisaggregationYearPlanId' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.OutcomeDisaggregationYearPlanId' type='hidden' value='0'>";
   EOPHtmlContent += "					<input data-val='true' data-val-required='The Year field is required.' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_Year' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.Year' type='hidden' value='-1'>";

   if (displayModeEOPStartUpPlan) {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_OriginalPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.OriginalPlan' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   } else {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_OriginalPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.OriginalPlan' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' disabled='disabled' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   }


   EOPHtmlContent += "			   </label>";
   EOPHtmlContent += "			</td>";
   EOPHtmlContent += "		</tr>";
   EOPHtmlContent += "		<tr class='rm_pa_row'>";
   EOPHtmlContent += "			<td class='rm_edit_rh'>";
   EOPHtmlContent += "				<label class='editLabel' for=''>";

   if (displayModeEOPAnnualPlan) {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_AnnualPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.AnnualPlan' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   } else {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_AnnualPlan' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.AnnualPlan' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' disabled='disabled' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   }

   EOPHtmlContent += "				</label>";
   EOPHtmlContent += "			</td>";
   EOPHtmlContent += "		</tr>";
   EOPHtmlContent += "		<tr class='rm_ac_row'>";
   EOPHtmlContent += "			<td class='rm_edit_rh'>";
   EOPHtmlContent += "				<label class='editLabel' for=''>";

   if (displayModeEOPActualPlan) {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_ActualValue' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.ActualValue' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   } else {
       EOPHtmlContent += "					<input class='input min_input numberInput auto " + (isAutoCalc ? "bcGray" : "") + "' id='Outcomes_" + impactIndex + "__OutcomeIndicators_" + indicatorIndex + "__OutcomeDisaggregations_" + disaggIndex + "__EndOfProject_ActualValue' name='Outcomes[" + impactIndex + "].OutcomeIndicators[" + indicatorIndex + "].OutcomeDisaggregations[" + disaggIndex + "].EndOfProject.ActualValue' type='text' data-v-min='-9999999999999999.99' data-v-max='9999999999999999.99' value='' disabled='disabled' " + (isAutoCalc ? "readonly='readonly'" : "") + ">";
   }

   EOPHtmlContent += "				</label>";
   EOPHtmlContent += "			</td>";
   EOPHtmlContent += "		</tr>";
   EOPHtmlContent += "	</tbody>";
   EOPHtmlContent += "</table>";
   EOPHtmlContent += '</td>';



   htmlContent += EOPHtmlContent;

   return htmlContent;

}
function deleteDissagregation(btnAddDisaggregation, resultsMatrixId, impactDisaggregationId, impactDisaggregationRow, isLinked) {

   event.preventDefault();

   if (!isLinked) {

      if (impactDisaggregationId > -1) {
          confirmActionRM(messageDeleteDisaggregation)
              .done(function (value) {
                  if (value) {
                      window.location.href = $("#hdnDeleteDisaggregationPost").val() +
                          "?resultsMatrixId=" + resultsMatrixId + "&impactDisaggregationId=" + impactDisaggregationId;
                  }
              });
      } else {
         var currentRow = $(btnAddDisaggregation).closest("tr.nivel01").first();
         var operatorBar = $(currentRow).closest("div.tableGrid").first().prev();
         var currentBarLength = parseInt($(operatorBar).css("height"));
         var currentHeightRow = parseInt($(currentRow).css("height"));
         $(operatorBar).css("height", (currentBarLength - currentHeightRow) + "px");
         $(currentRow).remove();
      }
   } else {
      var message = $("#hdnImpactsOutcomesDeleteExistingDisaggregationLinkedMessage").val();
      var position = parseInt($(impactDisaggregationRow).offset().top);

      new $.Zebra_Dialog(message, {
         'title': 'Warning',
         'buttons': true,
         'modal': false,
         'auto_close': false,
         'animation_speed_hide': 0,
         'type': false
      });

      $(".ZebraDialog").css('position', 'absolute');
      $(".ZebraDialog").css('top', (position - 78) + 'px');
   }



}

/**************************************************************************************/
/****************************** GENERAL FUNCTIONS *************************************/
/**************************************************************************************/

function preventDefault(e) {
   e = e || window.event;
   if (e.preventDefault)
      e.preventDefault();
   e.returnValue = false;
}

function keydown(e) {
   for (var i = keys.length; i--;) {
      if (e.keyCode === keys[i]) {
         preventDefault(e);
         return;
      }
   }
}

function wheel(e) {
   preventDefault(e);
}

function disable_scroll() {
   if (window.addEventListener) {
      window.addEventListener('DOMMouseScroll', wheel, false);
   }
   $(".k-window").hover(
       function () {
          document.onmousewheel = document.onmousedown = document.onkeydown = null;
       },
       function () {
          document.onmousewheel = document.onmousedown = wheel;
          document.onkeydown = keydown;
       }
   );
}

function enable_scroll() {
   if (window.removeEventListener) {
      window.removeEventListener('DOMMouseScroll', wheel, false);
   }
   document.onmousewheel = document.onmousedown = document.onkeydown = null;
}

function getCurrentListOfYears() {
   //Function that returns the array of years currently displayed by the first impact 
   var existingOptions = new Array();

   var yearPlans = $("#impactsContainer").find('[id^="yearPlans_"]');
   if (yearPlans != null) {

      var selectedYearPlan = yearPlans[0];
      var numYears = 0;

      $.each(yearPlans, function (index, yearPlan) {
         var years = $(yearPlan).find("select");
         if (years.length > numYears)
            selectedYearPlan = yearPlan;
      });
      // Get all select elements from the first row
      var selects = $(selectedYearPlan).find("select");

      $.each(selects, function (index, selectItem) {
         var option = $(selectItem).find("option:selected").text();
         existingOptions.push(parseInt(option));
      });
   }
   return existingOptions;
}
