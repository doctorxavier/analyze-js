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
    //Top menu 
    $('.navMenuPrincipal .level_2').hide();
   // $('.navMenuPrincipal .submenu').hide();
    $('.navMenuPrincipal .sel .level_2').show();

    $('.navMenuPrincipal .item_level_1').mouseenter(function () {
        if ($('.menu_max').is('.max')) {
            if ($(this).is('.sel') || !$('.navMenuPrincipal .submenu').is(":hidden")) {
                $('.navMenuPrincipal .level_2').hide();
                $('.navMenuPrincipal .submenu').show();
                $(this).find('.level_2').show();
            }
        } else {
            $('.navMenuPrincipal .level_2').hide();
            $('.navMenuPrincipal .submenu').show();
            $(this).find('.level_2').show();
        }
    });
    $('.navMenuPrincipal').mouseleave(function () {
        if ($('.menu_max').is('.max')) {
            $('.navMenuPrincipal .submenu').hide();
            $('.navMenuPrincipal .level_2').hide();
            $('.izquierda').hide();
            $('.layoutDosColsIzda .central').css('display', 'block');
        } else {
            $('.navMenuPrincipal .level_2').hide();
            $('.navMenuPrincipal .sel .level_2').show();
        }
    });

    //Maximized behaviour
    $("#item_op_details").mouseenter(function () {
        if ($('.menu_max').is('.max')) {
            $('.izquierda').show();
            $('.layoutDosColsIzda .central').css('display', 'table-cell');
        }
    });
    $(".izquierda").mouseleave(function () {
        if ($('.menu_max').is('.max')) {
            $('.izquierda').hide();
            $('.layoutDosColsIzda .central').css('display', 'block');
        }
    });
    
    $(".menu_max").click(function () {
        if ($('.menu_max').is('.max')) {
            //$('.navMenuPrincipal').slideDown();
            $('.navMenuPrincipal .submenu').show();
            $('.navMenuPrincipal .sel .level_2').show();
            $('.opMin').show();
            $('.opMax').hide();
            $('.menu_max').removeClass('max');
            $('.mod_contenido_header').slideDown();
            $('.izquierda').show();
            $('.layoutDosColsIzda .central').css('display', 'table-cell');
        } else {
            //  $('.navMenuPrincipal').slideUp();
            $('.navMenuPrincipal .submenu').hide();
            $('.navMenuPrincipal .level_2').hide();
            $('.opMin').hide();
            $('.opMax').show();
            $('.menu_max').addClass('max');
            $('.mod_contenido_header').slideUp();
            $('.izquierda').hide();
            $('.layoutDosColsIzda .central').css('display', 'block');
        }
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
