$(window).load(function () {
   $("input[type='checkbox']:checked").each(function () {
      var caption = $(this).data("caption");
      $(this).closest("label").children("span").addClass("unableMarked").text(caption);
   });
   $("input[type='checkbox']:not(:checked)").each(function () {
      var caption = $(this).data("caption");
      $(this).closest("label").children("span").addClass("disabled").text(caption);
   });
});

$(document).ready(function () {


   //modal windows for basic data web page
   $('.lnkModal').click(function () {
      var Id = $(this).attr('id');
      $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="ui-widget-overlay ui-front"></div>');
      $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');

      var dialog = $(".dinamicModal").kendoWindow({
         width: "800px",
         title: $(this).data("title"),
         draggable: false,
         resizable: false,
         content: $(this).data("route"),
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
            if (Id == "ConvergencerelatedOperation") {
               redirectPage($("#IndexBasicDataUrl").val());
            }
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
   });

   // edit action for basic data
   $('.edit').click(function () {
      redirectPage($(this).data("route"));
      return false;
   });


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
});