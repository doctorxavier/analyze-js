/**
 * @constructor
 * @description Componente para la vista de contenido dentro de una ventana modal
 *
 * </br>Las funciones precedidas por un  guión bajo no deben ser invocadas de manera
 * manual
 * <pre>=============================================================================
 * Dependencias:
 * jquery.1.7 >
 * Compatibile:
 * IE: 7 >
 * Firefox
 * Chrome
 * =============================================================================
 * 1.1
 * - Añadido parámetro opcional en constructor. Permite agregar un conjunto de
 * items en su instanciación.
 * =============================================================================</pre>
 * @version 1.0
 * <pre>Changelog
 * =============================================================================
 * 0.1
 * - Primera versión estable
 * =============================================================================</pre>
 *
 *
 * @autor Centro DFront, everis spain
 * @param {contentWindow} parametro donde indica el contendor a mostar en ventana modal
 * @param {lnkOpenWindow} enlace que va a lanzar la ventana modal.
 *
 * <pre id="api_constructor">
 *
 * </pre>
 * @example new DataTable (".window",".lnkModal");
 *
 */

var Modal = function (contentWindow, lnkOpenWindow, lnkCloseWindow) {

    /**
    * @param {contentWindow} Nodo que envuelve el contenido a mostar en ventana modal
    * @private
    */
    this.contentWindow = $(contentWindow);


    /**
    * @param {lnkOpenWindow} Elemento que ejecuta las ventanas modales
    * @private
    */
    this.lnkOpenWindow = $(lnkOpenWindow);


    /**
    * @param {lnkOpenWindow} Elemento que cierra las ventanas modales
    * @private
    */
    this.lnkCloseWindow = $(lnkCloseWindow);

    //this._createModal();

    this.on(this.lnkOpenWindow, "click", { instance: this }, this.assignEventOpenWindow);
    this.on(this.lnkCloseWindow, "click", { instance: this }, this.assignEventCloseWindow);
};

/**
 * @param {String}  Constante que almacena el valor correspondiente a cerrar una ventana *
 * @static
 * @constant
 * @default CLASS_WINDOW_CLOSE
 */
Modal.prototype.CLASS_WINDOW_CLOSE = ".k-link";



/**
 * @param {String}  Constante que almacena el valor correspondiente a una ventana abierta *
 * @static
 * @constant
 * @default CLASS_WINDOW_OPEN
 */
Modal.prototype.CLASS_WINDOW_OPEN = "windowOpen";

/**
 * @param {String}  Constante que almacena el valor correspondiente a la clase que utiliza el overlay *
 * @static
 * @constant
 * @default CLASS_OVERLAY
 */
Modal.prototype.CLASS_OVERLAY = "ui-widget-overlay";


/**
 * @param {String}  Constante que almacena el valor correspondiente a la clase de seleccion para el overlay *
 * @static
 * @constant
 * @default SELECTED_CLASS_OVERLAY
 */
Modal.prototype.SELECTED_CLASS_OVERLAY = ".ui-widget-overlay";


/**
 * @param {String}  Constante que almacena el valor correspondiente a la clase que oculta elementos *
 * @static
 * @constant
 * @default CLASS_HIDE
 */
Modal.prototype.CLASS_HIDE = "hide";



/**
 * @param {String}  Constante que almacena el valor correspondiente a la nomenclatura de llamada del componente
 * @static
 * @constant
 * @default NAME_COMPONENT_FRAMEWORK
 */
Modal.prototype.NAME_COMPONENT_FRAMEWORK = "kendoWindow";



/**
 * @param {String}  Constante que almacena el valor correspondiente a la ventana modal una vez creada
 * @static
 * @constant
 * @default CLASS_WINDOW_COMPONENT
 */
Modal.prototype.CLASS_WINDOW_COMPONENT = ".k-window";


Modal.prototype._createModal = function () {

    var instance = this;

    this.contentWindow.each(function () {

        $(this).kendoWindow({
            width: "800px",
            title: $(this).data("title"),
            draggable: false,
            resizable: false,
            content: $(this).data("route"),
            pinned: true,
            actions: [
                "Close"
            ],
            close: function () { instance._hideOverlay(instance); }
        });

    });

    $(this.CLASS_WINDOW_COMPONENT).hide();

    this._createOverlay();

};

Modal.prototype.openModalWindow = function (openWindow, instance) {
    
    this._createModal();
    this.on($(this.CLASS_WINDOW_CLOSE), "click", { instance: this }, this.assignEventCloseWindowButton);


    var newWindow = $(openWindow);

    newWindow.data(instance.NAME_COMPONENT_FRAMEWORK).open();

    newWindow.addClass(instance.CLASS_WINDOW_OPEN);

    instance._showOverlay(instance.SELECTED_CLASS_OVERLAY, instance.CLASS_HIDE);

    instance.centerModalWindow(newWindow, instance.NAME_COMPONENT_FRAMEWORK);

    instance.newWindow = newWindow;

    instance.on($(window), "resize", { instance: instance }, instance.assignEventCenterWindow);
};


Modal.prototype.closeModalWindow = function (closeWindow, instance) {

    closeWindow = $(closeWindow);
    closeWindow.removeClass(instance.CLASS_WINDOW_OPEN);

    closeWindow.data(instance.NAME_COMPONENT_FRAMEWORK).close();

};


Modal.prototype.assignEventOpenWindow = function (e) {

    var instance = e.data.instance;

    var openWindow = $(this).attr("href");
    instance.openModalWindow(openWindow, instance);
};

Modal.prototype.assignEventCloseWindow = function (e) {

    var instance = e.data.instance;

    var closeWindow = $(this).attr("href");
    instance.closeModalWindow(closeWindow, instance);
};

Modal.prototype.assignEventCloseWindowButton = function (e) {
    var instance = e.data.instance;
    var closeWindow = $(this).parents(instance.CLASS_WINDOW_COMPONENT).find("." + instance.CLASS_WINDOW_OPEN)
    closeWindow.removeClass(instance.CLASS_WINDOW_OPEN);
};

Modal.prototype.assignEventCenterWindow = function (e) {

    var instance = e.data.instance;

    instance.centerModalWindow(instance.newWindow, instance.NAME_COMPONENT_FRAMEWORK);
};

Modal.prototype.centerModalWindow = function (element, component) {

    $(element).data(component).center();
};

Modal.prototype._createOverlay = function () {

    $("body").append('<div class="hide ' + this.CLASS_OVERLAY + ' ui-front"></div>');

};

Modal.prototype._showOverlay = function (element, classHide) {

    $(element).removeClass(classHide);

};

Modal.prototype._hideOverlay = function(instance) {

    var anyContainerWindowOpen = $(instance.CLASS_WINDOW_COMPONENT + " ." + instance.CLASS_WINDOW_OPEN);


    if (anyContainerWindowOpen.length == 0) {
        $(instance.SELECTED_CLASS_OVERLAY).addClass(instance.CLASS_HIDE);
    }
};
Modal.prototype.on = function (element, event, data, func) {

    element.on(event, data, func);
};

Modal.prototype.off = function (element, event, func) {
    element.off(event, func);
};


