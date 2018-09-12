$(document).ready(function () {

   

   jQuery.validator.addMethod("txtDescriptionComent", function (value, element) {
      if (value.toString() != "") {
         if ($(element).val().toString().trim().length == 0) {
            return false;
         }
      }
      else {
         return false;
      }
      return true;
   });

   $(document).tooltip({
      items: ".input-validation-error",
      content: function () {
         if ($(this).attr('data-val-required'))
            return $(this).attr('data-val-required');
         if ($(this).attr('data-val-date'))
            return $(this).attr('data-val-date');
         if ($(this).attr('data-val-number'))
            return $(this).attr('data-val-number');
         if ($(this).attr('data-val-range'))
            return $(this).attr('data-val-range');
      }
   });

   $(".Btn_SubmitAndRequest").on("click", function () {
      var newHtml = '<input type="hidden" name="Request" value="True"/> ';
      $("#FormCommentsPMI").append(newHtml);
      var addValsInput = $('<input type="hidden" name="additional_validators"/>');
      addValsInput.val($('#validator_list_additional_list').val());
      $("#FormCommentsPMI").append(addValsInput);
      idbg.lockUi(null, true);
      $("body").scrollTop(0);
      $(window.parent.document.body).scrollTop(0);
      $("#FormCommentsPMI").submit();
   });

   $(".saveButton").on("click", function () {
      $("body").scrollTop(0);
      $(window.parent.document.body).scrollTop(0);
      $("#FormCommentsPMI").submit();
   });

});


