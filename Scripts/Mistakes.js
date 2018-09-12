function OpenDetails() {
   document.getElementById("fondo").className = "fondoon";
   document.getElementById("poput").className = "poput";
}
function CloseDetails() {
   document.getElementById("fondo").className = "fondooff";
   document.getElementById("poput").className = "fondooff";
}


//common functions
$(window).load(function () {
   $(window.parent.document).find('iframe').attr("height", "1000px");
});

