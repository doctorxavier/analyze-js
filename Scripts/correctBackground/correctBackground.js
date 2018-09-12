// corrige el fondo del contenedor general al modificar el tamaño del menu lateral.
var correctBackground = function(){
	
	var $seccionContenido = $("#contenidoID");
	var $listaDesplegable = $(".izquierda");
	var menuHeight = $listaDesplegable.height();
	var contenidoHeight = $seccionContenido.height();
		// si el tamaño del menu lateral es superior al del contenido central se añade una clase al central que modifica el background.
		// en caso contrario se elimina esa clase añadida.
		menuHeight = $listaDesplegable.height();
		contenidoHeight = $seccionContenido.height();
		if(!(menuHeight < contenidoHeight)){
			if(!$seccionContenido.hasClass(".contenidoSmall")){
				$seccionContenido.addClass("contenidoSmall");
			}
		}else{
				$seccionContenido.removeClass("contenidoSmall");
		}

}	
