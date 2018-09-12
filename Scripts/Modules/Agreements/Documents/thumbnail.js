$(document).ready(function() {
	var thumbnail = $(".thumbnail");
	//var icon = $(thumbnail).find(".ico_check_green");
	if($(thumbnail).length > 0){
		$(thumbnail).click(function(){
			if($(this).hasClass("active")){
				/* deactivate thumbnail */
				$(this).removeClass("active");
			}else{
			    /* activate thumbnail */
			    var test = $(this).val();
			    $('#clauseTemplateId').val($(this).val());
				$(this).addClass("active");
			}
		});
	}
});