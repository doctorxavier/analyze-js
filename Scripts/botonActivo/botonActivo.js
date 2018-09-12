/**
 * Object that manages a switch-type button.
 * @param btnClass jQuery element class of the button.
 * @returns {BotonActivo}
 */
var BotonActivo = function(btnClass){
    this.btnClass = btnClass;
    this.iText;
    this.aText;
    this.btnInput = btnClass.find(".btn-action"); /* Input inside the button. */
    this._createBotonActivo();
};

/**
 * Creator of the BotonActivo object.
 * Sets the event on the object to switch when clicked.
 */
BotonActivo.prototype._createBotonActivo = function(){
    var instance = this;
    $(this.btnInput).click(function(){ instance.switchState(); });
};

/**
 * Function that switches the state of the object.
 */
BotonActivo.prototype.switchState = function(){
    if($(this.btnClass).hasClass("activo")){
        /* De-activate */
        $(this.btnInput).attr("value",this.iText);
        $(this.btnClass).removeClass("activo");
    }else{
        /* Activate */
        $(this.btnInput).attr("value",this.aText);
        $(this.btnClass).addClass("activo");
    }
};

/**
 * Function that checks if the object is in the activated state.
 * @returns {boolean} true if object has class "activo"
 */
BotonActivo.prototype.isActive = function(){
    return $(this.btnClass).hasClass("activo");
};

/**
 * Function that sets the state of the object to active.
 * Adds the class "activo".
 */
BotonActivo.prototype.setActive = function(){
    if(!$(this.btnClass).hasClass("activo")){ $(this.btnClass).adClass("activo"); }
};

BotonActivo.prototype.setActiveText = function(text){
    this.aText = text;
};

BotonActivo.prototype.setInactiveText = function(text){
    this.iText = text;
};

BotonActivo.prototype.getBtnInputText = function(){
    return this.btnInput.attr("value");
};

function urlThumbImages(file) {
    pos = file.lastIndexOf(".");
    var ext = file.substr(pos);
    var res = ext.replace(".", "_");
    file = file.substr(0, pos);
    file = file + res + ".jpg";
    var posBarra = file.lastIndexOf("/");
    file = file.substr(0, posBarra + 1) + "_t" + file.substr(posBarra);
    return file;
}