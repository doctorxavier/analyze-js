$(function() {
    $('.btn-action-group').click(function(e) {
        e.preventDefault();
        var $this = $(this);
        if($this.hasClass("activo")){
        $this.removeClass("activo");
    		}else{
        $this.addClass("activo");
    		}
    });
});