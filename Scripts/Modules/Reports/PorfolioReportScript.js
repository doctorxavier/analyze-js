
function LoadAllCountry() {

   var route = $('#LinkAllCountrys').attr('data-route');

   var newhtml = "";
   $("#CountryDepartment option").each(function (index, element) {
      var valueSelected = $(element).val();
      newhtml += "<input type='hidden' class='indexCountry' name='CountryDepartmentsId' value='" + valueSelected + "' />";
   });

   $("#FormDashPorfolio").append(newhtml);

   $.ajax({
      url: route,
      data: $("#FormDashPorfolio").serialize(),
      type: 'Post',
      dataType: "json",
      success: function (resp) {
         var LoadOptions = "";
         $("#Country option").remove();
         for (var i = 0; i < resp.length; i++) {
            LoadOptions += "<option value='" + resp[i].Code + "'>" + resp[i].Name + "</option>";
         }
         $("#Country").append(LoadOptions);
      },
      error: function (e, err, erro) {
         alert('Error: ' + e + ' - ' + err + ' - ' + erro);
      },
      complete: function () {
         $(".indexCountry").remove();
      }
   });

}

function LoadAllDivision() {
   var route = $('#LinkAllDivisions').attr('data-route');

   var newhtml = "";
   $("#DepartmentSector option").each(function (index, element) {
      var valueSelected = $(element).val();
      newhtml += "<input type='hidden' class='indexDepertments' name='Departments' value='" + valueSelected + "' />";
   });

   $("#FormDashPorfolio").append(newhtml);

   $.ajax({
      url: route,
      data: $("#FormDashPorfolio").serialize(),
      type: 'Post',
      dataType: "json",
      success: function (resp) {
         var LoadOptions = "";
         $("#Division option").remove();
         for (var i = 0; i < resp.length; i++) {
            LoadOptions += "<option value='" + resp[i].Code + "'>" + resp[i].Code + "</option>";
         }
         $("#Division").append(LoadOptions);
         ClearDivision();
      },
      error: function (e, err, erro) {
         alert('Error: ' + e + ' - ' + err + ' - ' + erro);
      },
      complete: function () {
         $(".indexDepertments").remove();
      }
   });
}

$(document).on("ready", function () {
   $('#searchBoxContainerReportsDS002').toggle();

   $("#Btn_ExportExcel").on("click", function () {
      $("#Format").attr("value", "EXCELOPENXML");
      GenerateReport();
      $("#Format").attr("value", "");
   });

   $("#Btn_ExportdPDF").on("click", function () {
      $("#Format").attr("value", "PDF");
      GenerateReport();
      $("#Format").attr("value", "");
   });

   $("#CountryDepartment").on("change", function () {
      if ($("#CountryDepartment option:selected").length == 0) {
         LoadAllCountry();
      }
      else {
          var route = $('#LinkFilterCountry').attr('data-route');
          $.ajax({
            url: route,
            data: $("#FormDashPorfolio").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
               var LoadOptions = "";
               $("#Country option").remove();
               for (var i = 0; i < resp.length; i++) {
                  LoadOptions += "<option value='" + resp[i].Code + "'>" + resp[i].Name + "</option>";
               }
               $("#Country").append(LoadOptions);
            },
            error: function (e, err, erro) {
               alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
         });
      }
   });

   $("#DepartmentSector").on("change", function () {

      if ($("#DepartmentSector option:selected").length == 0) {
         LoadAllDivision();
      }
      else {
         var route = $('#LinkFilterDepartment').attr('data-route');

         $.ajax({
            url: route,
            data: $("#FormDashPorfolio").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp) {
               var LoadOptions = "";
               $("#Division option").remove();
               for (var i = 0; i < resp.length; i++) {
                  LoadOptions += "<option value='" + resp[i].Code + "'>" + resp[i].Name + "</option>";
               }
               $("#Division").append(LoadOptions);
            },
            error: function (e, err, erro) {
               alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            }
         });
      }

   });

   $("#FormDashPorfolio").on("submit", function (e) {
      e.preventDefault();

      GenerateReport();

   });

   $("#Btn_Clear").on("click", function () {
      $(".MultiSelectCustom option").each(function (index, element) {
         $(element).removeAttr("selected");
      });
      LoadAllCountry();
      LoadAllDivision();
   });

   $("#Btn_Sumit").submit();

});

function GenerateReport() {
   var route = $("#FormDashPorfolio").attr("action");
   $("#AjaxLoadForDashReport").show();

   $.ajax({
      url: route,
      data: $("#FormDashPorfolio").serialize(),
      type: 'Post',
      success: function (resp) {
         if (resp.indexOf("http") >= 0) {

            $("#framePorfolioReport").attr("src", resp);
            $("#framePorfolioReport").show();
            $("#framePorfolioReport").addClass("ReportShow");

         }
         else {
            alert(resp);
         }
      },
      error: function (e, err, erro) {
         alert('Error: ' + e + ' - ' + err + ' - ' + erro);
      },
      complete: function () {
         $("#AjaxLoadForDashReport").hide();
         
      }
   });

}

function RadioSelected(Selected) {
   if ($("#framePorfolioReport").hasClass("ReportShow")) {
      //GenerateReport();
   }
}
