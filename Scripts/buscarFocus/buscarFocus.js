var buscarFocus = function(){
    // elementos a los que va a afectar el evento focus
    var elemento2Trigger, elemento2Objetivo = ".texto_buscar";
	var elementoTrigger = "#buscarEmpleados";
	var elementoObjetivo = "#btnBuscar";
	// clases que van a variar con el evento focus
	var claseOriginal = "btn_azul_normal";
	var claseSecundaria = "btn_azul";
	var placeholderOriginal = "texto_buscar";
	var placeholderSecundaria = "activo";

	// Comportamiento al recibir el focus el elementoTrigger.
	// se cambia la clase original del elemento objetivo por una clase secundaria
	$(elementoTrigger).focus(function(){
		var $btnBuscar = $(elementoObjetivo);
		if(!$btnBuscar.hasClass("."+claseOriginal)){
			$btnBuscar.removeClass(claseOriginal);
			$btnBuscar.addClass("btn_azul");
		}
	});

	$(elemento2Trigger).focus(function () {
	    var buscarEmpleados = $(elemento2Objetivo);
	    if (!buscarEmpleados.hasClass("." + placeholderOriginal)) {
	        buscarEmpleados.addClass("activo");
	    }
	});

	//  Comportamiento al perder el focus el elementoTrigger.
	// se cambia la clase secundaria por la original del elemento objetivo
	$(elementoTrigger).focusout(function(){
		var $btnBuscar = $(elementoObjetivo);
		if(!$btnBuscar.hasClass("."+claseSecundaria)){
			$btnBuscar.removeClass(claseSecundaria);
			$btnBuscar.addClass(claseOriginal);
		}
	});
	
	$(elemento2Trigger).focusout(function () {
	    var $buscarEmpleados = $(elemento2Objetivo);
	    if (!$buscarEmpleados.hasClass("." + placeholderSecundaria)) {
	        $buscarEmpleados.removeClass(placeholderSecundaria);
	    }
	});
}