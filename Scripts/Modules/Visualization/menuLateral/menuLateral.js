/* *************************************************************************** */
/* Proyecto: BID	                                                    	   */
/* T�tulo: navegacion.js	                     							   */
/* Desripci�n: Javascript para controlar el men� principal de navegaci�n	   */
/* *************************************************************************** */

var CLASE_DESPLIEGUE_MENU = ".lista_desplegable_b .level2"

$(document).ready(function () {				
    //Oculto todas las listas con clase level2
	hide($(CLASE_DESPLIEGUE_MENU));
	//Evento click sobre el enlace que despliega
    $(".itemSlider").click(function(){
        //LLamo a la funcio que despliega
        gestorDespliegue($(this))
        putSelected();
    });
});
//Funcion que despliega el elemento
function despliegue(elemento){	
	//Muestro el elemento
    elemento.slideDown();
}
//Funcion que pliega el elemento
function pliegue(elemento){
	//Oculto el elemento
	elemento.slideUp();
}
//Funcion que gestiona si se pliega o no un elemento
function gestorDespliegue(elemento){
    //Variable que recoge el siguiente nodo hermano con clase level2
    var listaDesplegable = elemento.next(CLASE_DESPLIEGUE_MENU);
	//Si el elemento tiene la clase activo
    if(elemento.hasClass("activo")){
        //elimino la clase y llamo a la funcion de plegado
        elemento.removeClass("activo");
        pliegue(listaDesplegable);
    }else{
        //Agregamos la clase activo al elemento y llamo a la funcion de desplegado
        elemento.addClass("activo");
        despliegue(listaDesplegable);
    }
}
//Funcion para ocultar
function hide(elemento){
    //Oculto el elemento
    elemento.hide();
}

/* Function that manages the active state indicator */
function putSelected(){
	var list = $(".lista_desplegable_b").find(".level2"); /* Level2 list. */
	var links = $(list).find("a"); /* Links of the level2 list. */
	/* Click event on link. */
	$(links).click(function(){
		/* Remove "active" class from all links. */
		if(!$(this).hasClass("active")){			
			for(var i=0;i<links.length;i++){
				if($(links[i]).hasClass("active")){
					$(links[i]).removeClass("active");
				}
			}
			/* Add "active" class to this link. */
			$(this).addClass("active");
		}
	});	
}
