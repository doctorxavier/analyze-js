$(document).ready(function(){
	// elementos a los que va a afectar el evento focus
        var buscarEmpleados = $(".texto_buscar");
        var btnBuscar = $(".botonBuscar").find("input");
	// clases que van a variar con el evento focus
	var claseOriginal = "btn_azul_normal";
	var claseSecundaria = "btn_azul";
        var placeholderOriginal = "texto_buscar";
	var placeholderSecundaria = "activo";
	
	// Comportamiento al recibir el focus el elementoTrigger.
	// se cambia la clase original del elemento objetivo por una clase secundaria
	buscarEmpleados.focus(function(){
		if(!btnBuscar.hasClass("."+claseOriginal)){
			btnBuscar.removeClass(claseOriginal);
			btnBuscar.addClass(claseSecundaria);
		}
	});
        
        buscarEmpleados.focus(function(){
		if(!buscarEmpleados.hasClass("."+placeholderOriginal)){
			buscarEmpleados.addClass("activo");
		}
	});
        
	
	//  Comportamiento al perder el focus el elementoTrigger.
	// se cambia la clase secundaria por la original del elemento objetivo
	buscarEmpleados.focusout(function(){
		if(!btnBuscar.hasClass("."+claseSecundaria)){
			btnBuscar.removeClass(claseSecundaria);
			btnBuscar.addClass(claseOriginal);
		}
	});
        
        buscarEmpleados.focusout(function(){
		if(!buscarEmpleados.hasClass("."+placeholderSecundaria)){
			buscarEmpleados.removeClass(placeholderSecundaria);
		}
	});
	
});