$(document).ready(function(){
    alert("aqui esta");
    
    ocultartexto();


});


function ocultartexto() {

    var longitud = 12;

        var texto = $('.texto-cortado').text();
        var indiceUltimoEspacio = texto.lastIndexOf(' ');
        texto = texto.substring(0, indiceUltimoEspacio) + '<span class="puntos">...</span>';

        var primeraParte = '<span class="texto-mostrado">' + texto + '</span>';
        var segundaParte = '<span class="texto-ocultado">' + $(this).text().substring(indiceUltimoEspacio, $(this).text().length - 1) + '</span>';

        $(this).html(primeraParte + segundaParte);
        $(this).after('<span class="boton_mas_info">Ver más</span>');

    };

 