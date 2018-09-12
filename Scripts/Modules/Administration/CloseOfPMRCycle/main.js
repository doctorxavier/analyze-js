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

   $("#btnEditCycle").click(function () {
      var url = $(this).data("route");
      redirectPage(url);

   });



   $.ajax({
      url: $("#SearchUrl").data("search-path"),
      data: JSON.stringify({ 'Page': 1 }),
      dataType: "json",
      contentType: "application/json",
      type: 'POST',
      success: function (data) {
         // Update PagingFooter Content
         updateGridRowsContent("PMRCycle-template", "grdPmrCycle", data.PmrCycles);

         // Update PagingFooter Content
         updatePagingFooter(data.PageSettings);
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
         updateGridRowsContent("PMRCycle-template", "grdPmrCycle", data.PmrCycles);

         // Update PagingFooter Content
         updatePagingFooter(data.PageSettings);
      }
   });

});
