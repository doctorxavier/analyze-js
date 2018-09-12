//Este string contiene una imagen de 1px*1px color blanco,
//lo dividí en dos líneas debido al espacio disponible
window.imagenVacia = 'data:image/gif;base64,R0lGODlhAQABAI' +
                     'AAAAAAAP///ywAAAAAAQABAAACAUwAOw==';

window.mostrarVistaPrevia = function mostrarVistaPrevia() {

    var Archivos;
    var Lector;

    //Para navegadores antiguos
    if (typeof FileReader !== "function") {
        jQuery('#infoNombre').text('[Vista previa no disponible]');
        jQuery('#infoTamaño').text('(su navegador no soporta vista previa!)');
        return;
    }

    Archivos = jQuery('#upload')[0].files;
    if (Archivos.length > 0) {

        Lector = new FileReader();
        Lector.onloadend = function (e) {
            var origen,
                tipo;

            //Envía la imagen a la pantalla
            origen = e.target; //objeto FileReader

            //Prepara la información sobre la imagen
            tipo = window.obtenerTipoMIME(origen.result.substring(0, 30));
            jQuery('#infoNombre').text(Archivos[0].name + ' (Tipo: ' + tipo + ')');
            jQuery('#infoTamaño').text('Tamaño: ' + e.total + ' bytes' + e.width);
            //Si el tipo de archivo es válido lo muestra, 
            //sino muestra un mensaje 
            if (tipo !== 'image/jpeg' && tipo !== 'image/gif') {
                jQuery('#vistaPrevia').attr('src', window.imagenVacia);
                //alert('El formato de imagen no es válido: debe seleccionar una imagen JPG, PNG o GIF.');
                $('#upload').attr('Title', $('#ErrorExtension').val());
                $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
                $("#upload").tooltip("option", "position", { my: "right-120% top-55%" });
                $('#upload').focus();
            } else {
                jQuery('#vistaPrevia').attr('src', origen.result);
                
                //Toma medidas de la imagen
                 window.obtenerMedidas();
                
                //jQuery('#marcoVistaPrevia').css("visibility", "visible");                                
                jQuery('#lblNewSign').css("visibility", "visible");
                $('#spanfile').css("visibility", "hidden");
                jQuery('#cancelar').css("visibility", "visible");
            }

        };
        Lector.onerror = function (e) {
            console.log(e)
        }
        Lector.readAsDataURL(Archivos[0]);

    } else {
        var objeto = jQuery('#upload');
        objeto.replaceWith(objeto.val('').clone());
        jQuery('#vistaPrevia').attr('src', window.imagenVacia);
        jQuery('#infoNombre').text('[Seleccione una imagen]');
        jQuery('#infoTamaño').text('');
    };


};

//Lee el tipo MIME de la cabecera de la imagen
window.obtenerTipoMIME = function obtenerTipoMIME(cabecera) {
    return cabecera.replace(/data:([^;]+).*/, '\$1');
}

jQuery(document).ready(function () {

    //Cargamos la imagen "vacía" que actuará como Placeholder
    jQuery('#vistaPrevia').attr('src', window.imagenVacia);

    //El input del archivo lo vigilamos con un "delegado"
    jQuery('#botonera').on('change', '#upload', function (e) {
        window.mostrarVistaPrevia();
    });

    //El botón Cancelar lo vigilamos normalmente
    jQuery('#cancelar').on('click', function (e) {
        var objeto = jQuery('#upload');
        objeto.replaceWith(objeto.val('').clone());
        jQuery('#vistaPrevia').attr('src', window.imagenVacia);
        jQuery('#infoNombre').text('[Seleccione una imagen]');
        jQuery('#infoTamaño').text('');
        jQuery('#marcoVistaPrevia').css("visibility", "hidden");
        jQuery('#lblNewSign').css("visibility", "hidden");
        jQuery('#cancelar').css("visibility", "hidden");
        jQuery('#marcoVistaPrevia').css("height", "10px");
    });

});



var ancho = jQuery('#vistaPrevia').width();
var alto = jQuery('#vistaPrevia').height();
//alert('Medidas: ' + ancho + 'x' + alto);

//Obtiene las medidas de la imagen y las agrega a la 
//etiqueta infoTamaño
window.obtenerMedidas = function obtenerMedidas() {
    jQuery('<img/>').bind('load', function (e) {

        var tamaño = jQuery('#infoTamaño').text() +
            '; Medidas: ' + this.width + 'x' + this.height;
                
        var altura=this.height;
        var ancho=this.width;
       
        this.width = 400;
        this.height = 400;  

        jQuery('#marcoVistaPrevia').css("border", "ridge");
        jQuery('#marcoVistaPrevia').css("visibility", "visible");
        
        //tamaño = altura.toString() + ' - ' + ancho.toString() + ' - ' + this.width + ' - ' + this.height;
        jQuery('#marcoVistaPrevia').css("height", jQuery('#vistaPrevia').height() + 'px');
        jQuery('#marcoVistaPrevia').css("width", jQuery('#vistaPrevia').width() + 'px');
        //jQuery('#infoTamaño2').text(tamaño);
      
    }).attr('src', jQuery('#vistaPrevia').attr('src'));
    
}

var Lector = new FileReader();
Lector.onloadend = function (e) {
    var origen,
        tipo;

    //Envía la imagen a la pantalla
    origen = e.target; //objeto FileReader

    //Prepara la información sobre la imagen
    tipo = window.obtenerTipoMIME(origen.result.substring(0, 30));

    jQuery('#infoNombre').text(Archivos[0].name + ' (Tipo: ' + tipo + ')');
    jQuery('#infoTamaño').text('Tamaño: ' + e.total + ' bytes');
    //Si el tipo de archivo es válido lo muestra, 
    //sino muestra un mensaje 
    if (tipo !== 'image/jpeg' && tipo !== 'image/gif') {
        jQuery('#vistaPrevia').attr('src', window.imagenVacia);
        //alert('El formato de imagen no es válido: debe seleccionar una imagen JPG, PNG o GIF.');
        $('#upload').attr('Title', $('#ErrorExtension').val());
        $('#upload').tooltip({ show: { effect: "blind", duration: 800 } });
        $("#upload").tooltip("option", "position", { my: "right-120% top-55%" });
        $('#upload').focus();
        return false;
    } else {
        jQuery('#vistaPrevia').attr('src', origen.result);
        window.obtenerMedidas();
    }
};