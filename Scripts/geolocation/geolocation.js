$(document).ready(function () {
    $(".optionSelect").kendoDropDownList();
});

$(document).ready(function () {
    var thumbnail = $(".thumbnail");
    //var icon = $(thumbnail).find(".ico_check_green");
    if ($(thumbnail).length > 0) {
        $(thumbnail).click(function () {
            if ($(this).hasClass("active")) {
                /* deactivate thumbnail */
                $(this).removeClass("active");
            } else {
                /* activate thumbnail */
                $(this).addClass("active");
            }
        });
    }
});

/* bloquea el scroll en la ventana principal al abrir una ventana modal */
var keys = [37, 38, 39, 40];

$(document).ready(function () {
    var enlace = $(".lnkModal"); /* enlace de la ventana modal */
    /* se comprueba la accion sobre el enlace que abre la ventana modal */
    enlace.click(function () {
        $("body").css("overflow", "hidden");
        var cerrar = $(".k-window-action");
        cerrar.click(function () {
            $(".k-window").hover(
                function () {
                    document.onmousewheel = document.onmousedown = wheel;
                    document.onkeydown = keydown;
                },
                function () {
                    document.onmousewheel = document.onmousedown = document.onkeydown = null;
                }
            );
            $("body").css("overflow", "");

        });
    });
});

function preventDefault(e) {
    e = e || window.event;
    if (e.preventDefault)
        e.preventDefault();
    e.returnValue = false;
}

function keydown(e) {
    for (var i = keys.length; i--;) {
        if (e.keyCode === keys[i]) {
            preventDefault(e);
            return;
        }
    }
}

function wheel(e) {
    preventDefault(e);
}

function disable_scroll() {
    if (window.addEventListener) {
        window.addEventListener('DOMMouseScroll', wheel, false);
    }
    $(".k-window").hover(
        function () {
            document.onmousewheel = document.onmousedown = document.onkeydown = null;
        },
        function () {
            document.onmousewheel = document.onmousedown = wheel;
            document.onkeydown = keydown;
        }
    );
}

function enable_scroll() {
    if (window.removeEventListener) {
        window.removeEventListener('DOMMouseScroll', wheel, false);
    }
    document.onmousewheel = document.onmousedown = document.onkeydown = null;
}

var languages = {
    spanish: {
        Latitude: "Latitud",
        Longitude: "Longitud",
        Save: "Guardar",
        Close: "Cerrar",
        Add: "A\u00F1adir",
        AddGeolocation: "A?adir geolocalizaci\u00F3n",
        View_Geolocation: "Tipo Mapa",
        Street: "Calles",
        Satellites: "Sat\u00E9lite",
        BaseMap: "BaseMap",
        Hybrid: "H\u00EDbrido",
        Topographic: "Topogr\u00E1fico",
        Gray: "Gris",
        Oceans: "Oc\u00E9anos",
        NationalGeographic: "National-Geographic",
        Osm: "OSM",
        ClearMap: "Limpiar Mapa",
        Type: "Tipo",
        Select: "Seleccionar",
        Coordinates: "Coordenadas",
        File: "Fichero",
        Find: "Buscar",
        EnterAddress: "Escriba una direcci\u00F3n",
        FileUpload: "Subida de ficheros",
        NoFileSelected: "Fichero no seleccionado",
        Showing: "Mostrando",
        Of: "de",
        Geolocation: "Geolocalizaci\u00F3n",
        AdministrativeLevel: "Niveles administrativos",
        First: "Primero (Pa\u00EDs)",
        Second: "Segundo (Distrito/Departamento/Estado)",
        Third: "Tercero (Municipalidad/Distrito)",
        Address: "Direcci\u00F3n",
        AddressShow: "Direcci\u00F3n a mostrar",
        Edit: "Editar",
        Cancel: "Cancelar",
        Browse: "Seleccionar",
        puntoButtonTitle: "Dibujar un punto",
        lineaButtonTitle: "Dibujar una l\u00EDnea",
        poligonoButtonTitle: "Dibujar un pol\u00EDgono",
        PlaceHolderAddress: "Introduzca una direcci\u00F3n",
        ALERT_ADDRESS: "La direcci\u00F3n no puede ser localizada",
        ALERT_COORDINATES_INCORRECT: "Por favor, revise el formato de las coordenadas",
        ALERT_3: "Formato incorrecto. S\u00F3lo se permite una l\u00EDnea",
        ALERT_4: "El tipo del fichero no es correcto y no puede ser procesado",
        ALERT_5: "Existe un elemento. Si Acepta ser\u00E1 eliminado",
        ALERT_FILE_NEEDED: "Por favor, seleccione un fichero",
        ALERT_FILE_INCORRECT: "El formato del fichero no es correcto y no puede ser procesado",
        ALERT_LOCATION_NEEDED: "Por favor, seleccione una localizaci\u00F3n",
        ALERT_ADM_LEVELS: "Por favor, seleccione un nivel administrativo",
        ALERT_SAVE: "Cambios guardados",
        ALERT_SERVER_INFORMATION: "No es posible recuperar informaci\u00F3n",
        ALERT_UPDATE_INFORMATION: "No es posible actualizar informaci\u00F3n",
        ALERT_GRAPHICS_REMOVE: "Se eliminar\u00E1n los gr\u00E1ficos si presiona Sí",
        ERROR_SERVER: "Error de comunicaci\u00F3n con el servidor",
        ALERT_FORMAT_COORD_INCORRECT: "Las coordenadas no son num\u00E9ricas"
    },

    english: {
        Latitude: "Latitude",
        Longitude: "Longitude",
        Save: "Save",
        Close: "Close",
        Add: "Add",
        AddGeolocation: "Add Geolocation",
        View_Geolocation: "View-Geolocation",
        Street: "Streets",
        Satellites: "Satellites",
        BaseMap: "BaseMap",
        Hybrid: "Hybrid",
        Topographic: "Topographic",
        Gray: "Gray",
        Oceans: "Oceans",
        NationalGeographic: "National-Geographic",
        Osm: "OSM",
        ClearMap: "Clear Map",
        Type: "Type",
        Select: "Select",
        Coordinates: "Coordinates",
        File: "File",
        Address: "Address",
        Find: "Find",
        EnterAddress: "Enter an address",
        FileUpload: "File Upload",
        NoFileSelected: "No file selected",
        Showing: "Showing",
        Of: "of",
        Geolocation: "Geo-location",
        AdministrativeLevel: "Administrative Level",
        First: "First (Country)",
        Second: "Second (Distric/Departament/State)",
        Third: "Third (Municipality/District)",
        AddressShow: "Address to show",
        Edit: "Edit",
        Cancel: "Cancel",
        Browse: "Browse",
        puntoButtonTitle: "Draw a point",
        lineaButtonTitle: "Draw a line",
        poligonoButtonTitle: "Draw a polygon",
        PlaceHolderAddress: "Enter an address",
        ALERT_ADDRESS: "The address that was entered cannot be located",
        ALERT_COORDINATES_INCORRECT: "Please, review the format of the coordinates",
        ALERT_3: "Format incorrect. Only a line is allowed",
        ALERT_4: "The type of the file is not correct and it cannot be processed",
        ALERT_5: "An element exists. It will be deleted if yes is selected",
        ALERT_FILE_NEEDED: "Please, select a file to be uploaded",
        ALERT_FILE_INCORRECT: "The format of the file is not right and it cannot be processed",
        ALERT_LOCATION_NEEDED: "Please, select the location to be added",
        ALERT_ADM_LEVELS: "Please, select the administrative levels to be added",
        ALERT_SAVE: "Changes are saved",
        ALERT_SERVER_INFORMATION: "Impossible to get information",
        ALERT_UPDATE_INFORMATION: "Impossible to update information",
        ALERT_GRAPHICS_REMOVE: "All graphics will be removed if you press \"yes\"",
        ERROR_SERVER: "Server communication error",
        ALERT_FORMAT_COORD_INCORRECT: "Coordinates are no numeric."
    },

    french: {
        Latitude: "Latitude",
        Longitude: "Longitude",
        Save: "Save",
        Close: "Close",
        Add: "Add",
        AddGeolocation: "Add Geolocation",
        View_Geolocation: "View-Geolocation",
        Street: "Streets",
        Satellites: "Satellites",
        BaseMap: "BaseMap",
        Hybrid: "Hybrid",
        Topographic: "Topographic",
        Gray: "Gray",
        Oceans: "Oceans",
        NationalGeographic: "National-Geographic",
        Osm: "OSM",
        ClearMap: "Clear Map",
        Type: "Type",
        Select: "Select",
        Coordinates: "Coordinates",
        File: "File",
        Address: "Address",
        Find: "Find",
        EnterAddress: "Enter an address",
        FileUpload: "File Upload",
        NoFileSelected: "No file selected",
        Showing: "Showing",
        Of: "of",
        Geolocation: "Geo-location",
        AdministrativeLevel: "Administrative Level",
        First: "First (Country)",
        Second: "Second (Distric/Departament/State)",
        Third: "Third (Municipality/District)",
        AddressShow: "Address to show",
        Edit: "Edit",
        Cancel: "Cancel",
        Browse: "Browse",
        puntoButtonTitle: "Draw a point",
        lineaButtonTitle: "Draw a line",
        poligonoButtonTitle: "Draw a polygon",
        PlaceHolderAddress: "Enter an address",
        ALERT_ADDRESS: "The address that was entered cannot be located",
        ALERT_COORDINATES_INCORRECT: "Please, review the format of the coordinates",
        ALERT_3: "Format incorrect. Only a line is allowed",
        ALERT_4: "The type of the file is not correct and it cannot be processed",
        ALERT_5: "An element exists. It will be deleted if yes is selected",
        ALERT_FILE_NEEDED: "Please, select a file to be uploaded",
        ALERT_FILE_INCORRECT: "The format of the file is not right and it cannot be processed",
        ALERT_LOCATION_NEEDED: "Please, select the location to be added",
        ALERT_ADM_LEVELS: "Please, select the administrative levels to be added",
        ALERT_SAVE: "Changes are saved",
        ALERT_SERVER_INFORMATION: "Impossible to get information",
        ALERT_UPDATE_INFORMATION: "Impossible to update information",
        ALERT_GRAPHICS_REMOVE: "All graphics will be removed if you press \"yes\"",
        ERROR_SERVER: "Server communication error",
        ALERT_FORMAT_COORD_INCORRECT: "Coordinates are no numeric."
    },

    portuguse: {
        Latitude: "Latitude",
        Longitude: "Longitude",
        Save: "Save",
        Close: "Close",
        Add: "Add",
        AddGeolocation: "Add Geolocation",
        View_Geolocation: "View-Geolocation",
        Street: "Streets",
        Satellites: "Satellites",
        BaseMap: "BaseMap",
        Hybrid: "Hybrid",
        Topographic: "Topographic",
        Gray: "Gray",
        Oceans: "Oceans",
        NationalGeographic: "National-Geographic",
        Osm: "OSM",
        ClearMap: "Clear Map",
        Type: "Type",
        Select: "Select",
        Coordinates: "Coordinates",
        File: "File",
        Address: "Address",
        Find: "Find",
        EnterAddress: "Enter an address",
        FileUpload: "File Upload",
        NoFileSelected: "No file selected",
        Showing: "Showing",
        Of: "of",
        Geolocation: "Geo-location",
        AdministrativeLevel: "Administrative Level",
        First: "First (Country)",
        Second: "Second (Distric/Departament/State)",
        Third: "Third (Municipality/District)",
        AddressShow: "Address to show",
        Edit: "Edit",
        Cancel: "Cancel",
        Browse: "Browse",
        puntoButtonTitle: "Draw a point",
        lineaButtonTitle: "Draw a line",
        poligonoButtonTitle: "Draw a polygon",
        PlaceHolderAddress: "Enter an address",
        ALERT_ADDRESS: "The address that was entered cannot be located",
        ALERT_COORDINATES_INCORRECT: "Please, review the format of the coordinates",
        ALERT_3: "Format incorrect. Only a line is allowed",
        ALERT_4: "The type of the file is not correct and it cannot be processed",
        ALERT_5: "An element exists. It will be deleted if yes is selected",
        ALERT_FILE_NEEDED: "Please, select a file to be uploaded",
        ALERT_FILE_INCORRECT: "The format of the file is not right and it cannot be processed",
        ALERT_LOCATION_NEEDED: "Please, select the location to be added",
        ALERT_ADM_LEVELS: "Please, select the administrative levels to be added",
        ALERT_SAVE: "Changes are saved",
        ALERT_SERVER_INFORMATION: "Impossible to get information",
        ALERT_UPDATE_INFORMATION: "Impossible to update information",
        ALERT_GRAPHICS_REMOVE: "All graphics will be removed if you press \"yes\"",
        ERROR_SERVER: "Server communication error",
        ALERT_FORMAT_COORD_INCORRECT: "Coordinates are no numeric."
    }
};


/**
 * This function sets up the proper language texts
 * @param typeLang
 * @returns
 */
function setLangTexts(typeLang) {
    $("#window1_wnd_title").text(typeLang.Geolocation);
    $("#closeButton").text(typeLang.Close);
    $("#window1_wnd_title_add").text(typeLang.AddGeolocation);
    $("#closeButtonGeolocation").text(typeLang.Close);

    $("#viewGeolocationP").text(typeLang.View_Geolocation);


    var selectMapView = '<select name="view" id="view" class="mapLayerType AMAP_mapTypeSelect">';
    $("#SelectMapType").html(selectMapView);
    $("#view").html("");

    $("#view").append('<option value="">' + typeLang.BaseMap + '</option>');
    $("#view").append('<option value="streets">' + typeLang.Street + '</option>');
    $('#view').append("<option value='satellite'>" + typeLang.Satellites + "</option>");
    $('#view').append("<option value='hybrid'>" + typeLang.Hybrid + "</option>");
    $('#view').append("<option value='topo'>" + typeLang.Topographic + "</option>");
    $('#view').append("<option value='gray'>" + typeLang.Gray + "</option>");
    $('#view').append("<option value='oceans'>" + typeLang.Oceans + "</option>");
    $('#view').append("<option value='national-geographic'>" + typeLang.NationalGeographic + "</option>");
    $('#view').append("<option value='osm'>" + typeLang.Osm + "</option>");

    $("#view").kendoDropDownList({ height: "auto" });
    $("#view").on("change", function () { onSelectMapLayer($("#view").val()); });

    this.onSelectMapLayer = function (value) {
        var mode = value.toLowerCase();
        if (mode != "")
            map.setBasemap(mode);
    };



    $("#typeSelect").text(typeLang.Type);

    var selectType = '<input type="radio" name="selectType" id="selectTypeCoordinates" value="Coordinates" checked="checked">' + typeLang.Coordinates + '<br><input type="radio" name="selectType" id="selectTypeFile" value="File">' + typeLang.File +
        '<br><input type="radio" name="selectType" id="selectTypeAddress" value="Address">' + typeLang.Address;
    $("#SelectTypeDiv").html(selectType);


    ///var selectType = '<select name="select" id ="selectType" class="AMAP_addTypeSelect">';

    $("#SelectTypeDiv").html(selectType);
	/*$("#selectType").html("");
	$("#selectType").append('<option id = "Select" value="Select">'+ typeLang.Select +'</option>');
	$('#selectType').append('<option id="Coordinates" value="Coordinates" selected="selected">'+ typeLang.Coordinates +'</option>');
	$('#selectType').append('<option id="File" value="File">'+ typeLang.File +'</option>');
	$('#selectType').append('<option id ="Address" value="Address">'+ typeLang.Address +'</option>'); 
	$("#selectType").kendoDropDownList({width:"150px"});
	
	$("#selectType").on("change",function(){onSelectTypeLayer($("#selectType").val());})*/

    $("input[name=selectType]").on("change", function () { onSelectTypeLayer($("input[name=selectType]:checked").val()); })

    this.onSelectTypeLayer = function (value) {
        var valueT = value.toLowerCase();
        if (valueT === "coordinates") {

            $(".operationData .data_address").addClass("hide");
            $(".operationData .data").removeClass("hide");
            $(".coordinates").removeClass("hide");
            $(".address_info").addClass("hide");
            $(".file").addClass("hide");

            $("#principal").hide();
            $("#coordinates").val("");


        } else if (valueT === "file") {
            $(".operationData .data").addClass("hide");
            $(".coordinates").addClass("hide");
            $(".address_info").addClass("hide");
            $(".file").removeClass("hide");
            $(".operationData .data_address").addClass("hide");

            $("#principal").hide();
            $("#coordinates").val("");
            $("#divCarrusel").hide();

            $("#liFile1").attr('style', "display:block;");
            $("#liFile2").attr('style', "display:block;");

            $("#txtfileName").html(typeLang.NoFileSelected);
            $("#BrowseInput").val("");

        } else if (valueT === "select") {
            $(".operationData .data").addClass("hide");
            $(".coordinates").addClass("hide");
            $(".address_info").addClass("hide");
            $(".file").addClass("hide");
            $(".operationData .data_address").addClass("hide");

            $("#principal").hide();
            $("#coordinates").val("");
            $("#divCarrusel").hide();

        } if (valueT === "address") {
            $(".operationData .data").addClass("hide");
            $(".operationData .data_address").removeClass("hide");
            $(".coordinates").addClass("hide");
            $(".address_info").removeClass("hide");
            $(".file").addClass("hide");
            $("#AddressIDDiv").hide();
            $("#address").val("");
        }
    }


    $("#clearMapId").text(typeLang.ClearMap);

    $("#Find").val(typeLang.Find);
    $("#FindAddress").val(typeLang.Find);

    $("#fileUploadp").text(typeLang.FileUpload);
    $("#txtfileName").html(typeLang.NoFileSelected);
    $("#idShowing").html(typeLang.Showing + '<span id="carPos" class="dest">1</span><span id="ofId"> of </span> <span id="carTam" class="dest">7</span>');
    $("#ofId").text(typeLang.Of);

    $("#geolocationCarrusel").text(typeLang.Geolocation);
    $("#admLevelId").text(typeLang.AdministrativeLevel);
    $("#firstCarrusel").text(typeLang.First);
    $("#secondCarrusel").text(typeLang.Second);
    $("#thirdCarrusel").text(typeLang.Third);

    $("#geolocationPrincipal").text(typeLang.Geolocation);
    $("#admPrincipal").text(typeLang.AdministrativeLevel);
    $("#firstPrincipal").text(typeLang.First);
    $("#secondPrincipal").text(typeLang.Second);
    $("#thirdPrincipal").text(typeLang.Third);


    $("#addressId").text(typeLang.Address);
    $("#address").attr("placeholder", typeLang.PlaceHolderAddress);
    $("#coordinates").attr("placeholder", typeLang.Latitude + "," + typeLang.Longitude + "(19.12,-11.81)");

    $("#admAddress").text(typeLang.AdministrativeLevel);
    $("#firstAddress").text(typeLang.First);
    $("#secondAddress").text(typeLang.Second);
    $("#thirdAddress").text(typeLang.Third);

    $("#Edit").val(typeLang.Edit);
    $("#cancelEdit").text(typeLang.Cancel);
    $("#Add").val(typeLang.Add);
    $("#Browse").val(typeLang.Browse);
    $('#puntoButton').prop('title', typeLang.puntoButtonTitle);
    $('#lineaButton').prop('title', typeLang.lineaButtonTitle);
    $('#poligonoButton').prop('title', typeLang.poligonoButtonTitle);

}


//Constans
var hostServicesUrl = "http://optimamvc-s.iadb.org:8080/IDB.Presentation.MVC4/Visualization/Geolocation/";
var hostEsriServicesUrl = "http://10.64.49.105:6080/";
var folderEsriService = "geolocation";

var BingMapsKey = "AonYcj_tr_kwmga7Pto3uKErDSFyyTf-BFv045eryPzZpnLLAwGwKPlhi6u0zLVe";

var MARKER_URL = "img/modulos/map/map-marker.png";

var PUNTOS_WIDGET_VO = "Geo VO Point";
var LINEAS_WIDGET_VO = "Geo VO Polyline";
var POLIGONOS_WIDGET_VO = "Geo VO Polygon";

var PUNTOS_WIDGET = "Geo Project Point";
var LINEAS_WIDGET = "Geo Project Polyline";
var POLIGONOS_WIDGET = "Geo Project Polygon";

var FEATURE_PROJECT_POINT_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvVisualProject/FeatureServer/0";
var FEATURE_PROJECT_LINE_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvVisualProject/FeatureServer/1";
var FEATURE_PROJECT_POLYGON_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvVisualProject/FeatureServer/2";

var FEATURE_POINT_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvOutputProject/FeatureServer/0";
var FEATURE_LINE_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvOutputProject/FeatureServer/1";
var FEATURE_POLYGON_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriService + "/srvOutputProject/FeatureServer/2";

var GEOMETRY_SERVICE = hostEsriServicesUrl + "arcgis/rest/services/Utilities/Geometry/GeometryServer";

var urlPoint = "";
var urlLine = "";
var urlPolygon = "";



var ESRI_ADMINISTRATIVE_LEVEL_SERVICE = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

/* Send a VISUAL_OUTPUT_ID 
 * returns the Country description
 */
var GET_DESCRIPTION_COUNTRY = hostServicesUrl + "GetDescriptionCountryResponse";

/* Send VISUAL_OUTPUT_ID
 * returns LEVEL1, LEVEL2, LEVEL3 and VISUAL_OUTPUT_VERSION_ID
 */
var GET_VISUAL_OUTPUT_VERSION = hostServicesUrl + "GetVisualOutputVersionResponse";

/*Send VISUAL_OUTPUT_ID, LEVEL1, LEVEL2, LEVEL3
 * returns VISUAL_OUTPUT_VERSION_ID
 */
var UPDATE_VISUAL_OUTPUT_VERSION = hostServicesUrl + "UpdateVisualOutputVersionResponse";



/* Send a VISUAL_PROJECT_ID 
 * returns the Country description
 */
var GET_DESCRIPTION_COUNTRY_VISUAL_PROJECT = hostServicesUrl + "GetDescriptionCountryVisualProjectResponse";

/* Send VISUAL_PROJECT_ID
 * returns LEVEL1, LEVEL2, LEVEL3 and VISUAL_PROJECT_VERSION_ID
 */
var GET_VISUAL_PROJECT_VERSION = hostServicesUrl + "GetVisualProjectVersionResponse";

/*Send VISUAL_PROJECT_ID, LEVEL1, LEVEL2, LEVEL3
* returns VISUAL_PROJECT_VERSION_ID
*/
var UPDATE_VISUAL_PROJECT_VERSION = hostServicesUrl + "UpdateVisualProjectVersionResponse";


/**
 * This service update the Georeference field
 */
var UPDATE_GEOLOCATION = hostServicesUrl + "UpdateObjectLocationResponse";


var countryCodes = [];
countryCodes["BRA"] = "Brazil";
countryCodes["CHL"] = "Chile";
countryCodes["CRI"] = "Costa Rica";
countryCodes["GUF"] = "Guyana";
countryCodes["MEX"] = "Mexico";
countryCodes["PER"] = "Peru";
countryCodes["ARG"] = "Argentina";
countryCodes["BHS"] = "Bahamas";
countryCodes["PAN"] = "Panama";
countryCodes["VEN"] = "Venezuela";
countryCodes["COL"] = "Colombia";
countryCodes["GTM"] = "Guatemala";
countryCodes["URY"] = "Uruguay";


//global variables
var map;
var idTemplateSelected;
var templatePicker;
var geocoder;
var editToolbar;

var pointServiceLayer;
var LineServiceLayer;
var PolygonServiceLayer;

var drawToolbar;

var symbolPuntos;
var symbolLineas;
var symbolPoligonos;

var puntosArrayGraphics = new Array();
var lineasArrayGraphics = new Array();
var poligonosArrayGraphics = new Array();

var selectedTemplate = "";


var idProject; //This var contains the id of the convergence project 

var firstLevel;
var secondLevel;
var thirdLevel;
var typeSelect;

var arrayAdministrativeLevels = new Array();

var EditionMode = false;
var language = "en";
var typeLang = "";
var objectType = "";

var listAdm = new Array();


//Funciones
//Receive a read point
function AdministrativeLevel(punto) {

    // Call the service using puntoLeido and get the information here

    this.First_Country = "Country";
    this.Second_District = "District";
    this.Third_Municipality = "Municipality";

    this.get_First_Country = function () {
        return this.First_Country;
    };

    this.get_Second_District = function () {
        return this.Second_District;
    };

    this.get_Third_Municipality = function () {
        return this.Third_Municipality;
    };
}

function puntoLeido() {
    this.x;
    this.y;
}


function readSymbol(pick) {
    if (objectType == "vp") {
        if (pick.label == PUNTOS_WIDGET) {
            symbolPuntos = pick.symbol;
        } else if (pick.label == LINEAS_WIDGET) {
            symbolLineas = pick.symbol;
        } else if (pick.label == POLIGONOS_WIDGET) {
            symbolPoligonos = pick.symbol;
        }
    } else {
        if (pick.label == PUNTOS_WIDGET_VO) {
            symbolPuntos = pick.symbol;
        } else if (pick.label == LINEAS_WIDGET_VO) {
            symbolLineas = pick.symbol;
        } else if (pick.label == POLIGONOS_WIDGET_VO) {
            symbolPoligonos = pick.symbol;
        }
    }

}

function getAllSymbolsFeatures() {
    // Get the symbols
    var widgets = templatePicker._itemWidgets;

    for (var key in widgets) {
        if (key === 'length' || !widgets.hasOwnProperty(key)) continue;
        readSymbol(widgets[key]);
    }
}


//Get the number of graphics of each layer
//If center == true, the map has to be center in a point, polyline or polygon
function getNumberGraphicsFeatures(center, visual_version) {

    // Reset variables
    puntosArrayGraphics = new Array();
    lineasArrayGraphics = new Array();
    poligonosArrayGraphics = new Array();

    var featuresArray = new Array(); //templatePicker.featureLayers;
    featuresArray.push(pointServiceLayer);
    featuresArray.push(LineServiceLayer);
    featuresArray.push(PolygonServiceLayer);

    var campoVisual = "";

    if (objectType.toLowerCase() == "vo") {
        campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";    //This field corresponding to visual project id                     
    } else {
        campoVisual = "FK_GEO_VISUAL_PROJECT_ID";  //This field corresponding to visual version id        
    }


    if (visual_version != -1 && visual_version != undefined) {
        var whereStatement = campoVisual + " = " + visual_version;

        for (var i = 0; i < featuresArray.length; i++) {
            var featureItem = featuresArray[i];
            var geometryType = featureItem.geometryType;

            switch (geometryType) {
                case "esriGeometryPoint":
                    getNumObjectsFeature(pointServiceLayer, whereStatement, 0, center);
                    break;
                case "esriGeometryPolyline":
                    getNumObjectsFeature(LineServiceLayer, whereStatement, 1, center);
                    break;
                case "esriGeometryPolygon":
                    getNumObjectsFeature(PolygonServiceLayer, whereStatement, 2, center);
                    break;
            }
        }
    } else {
        puntosArrayGraphics = new Array();
        lineasArrayGraphics = new Array();
        poligonosArrayGraphics = new Array();
    }
}


//function getNumObjectsFeature(featureService,whereStatement,varToStore, center){
//
//  var query = new esri.tasks.Query();
//  query.where = whereStatement;
//  query.outFields = ["*"];
//  
//  featureService.queryFeatures(query, function (featureSet) {
//		if(featureSet.features.length > 0){
//			var graphicsLayer ="";
//			for(var i = 0; i< featureSet.features.length;i++){
//				if(featureSet.features[i]._graphicsLayer != null){
//					graphicsLayer = featureSet.features[i]._graphicsLayer;
//					break;
//				}
//			}
//			
//			 if(varToStore == 0){ 				 				 
//				 puntosArrayGraphics = graphicsLayer.graphics.concat();
//				 
//				 if(puntosArrayGraphics == undefined) puntosArrayGraphics=new Array();
//				 
//			  }else if(varToStore == 1){ 				  
//				  lineasArrayGraphics= graphicsLayer.graphics.concat();
//				  if(lineasArrayGraphics == undefined) lineasArrayGraphics=new Array();
//				  
//			  }else if(varToStore == 2){ 				  
//				  poligonosArrayGraphics = graphicsLayer.graphics.concat();
//				  if(poligonosArrayGraphics == undefined) poligonosArrayGraphics=new Array();
//			  }	 
//		}
//		
//		
//		//Center the map at a specific location 
//		if(center == true){
//			var textPoints="";
//			
//			if(puntosArrayGraphics.length > 0){
//				var punto = puntosArrayGraphics[0];
//				var point = new esri.geometry.Point(parseFloat(punto.geometry.x), parseFloat(punto.geometry.y),map.spatialReference); 
//
//			     map.centerAndZoom(point,13);			     
//			     textPoints = convert_To_LongLat(point);			    			     
//				
//			}else if (lineasArrayGraphics.length > 0){
//				var line = new esri.geometry.Polyline(map.spatialReference);
//				line.addPath(lineasArrayGraphics[0].geometry.paths);
//				line._extent = lineasArrayGraphics[0].geometry._extent;
//				
//				map.setExtent(line.getExtent().expand(1.5));
//				
//				textPoints = convert_To_LongLat(line);
//								
//			}else if (poligonosArrayGraphics.length > 0){
//				var poligon = new esri.geometry.Polygon(map.spatialReference);
//				poligon.addRing(poligonosArrayGraphics[0].geometry.rings); //Adds the ring to the polygon
//				poligon._extent = poligonosArrayGraphics[0].geometry._extent;
//				
//				map.setExtent(poligon.getExtent().expand(1.5));
//				
//				textPoints = convert_To_LongLat(poligon);				
//			}
//			
//			$("#geoLocationAddress").html(textPoints);
//		}
//	
//   }); 
//  
//}



function getNumObjectsFeature(featureService, whereStatement, varToStore, center) {

    var query = new esri.tasks.Query();
    query.where = whereStatement;
    query.outFields = ["*"];

    featureService.queryFeatures(query, function (featureSet) {
        if (featureSet.features.length > 0) {
            var graphicsLayer = "";
            for (var i = 0; i < featureSet.features.length; i++) {
                if (varToStore == 0) {
                    puntosArrayGraphics.push(featureSet.features[i]);
                } else if (varToStore == 1) {
                    lineasArrayGraphics.push(featureSet.features[i]);
                } else if (varToStore == 2) {
                    poligonosArrayGraphics.push(featureSet.features[i]);
                }
            }
        }


        //Center the map at a specific location 
        if (center == true) {
            var textPoints = "";

            if (puntosArrayGraphics.length > 0) {
                var punto = puntosArrayGraphics[0];
                if (punto.geometry != null) {
                    var point = new esri.geometry.Point(parseFloat(punto.geometry.x), parseFloat(punto.geometry.y), map.spatialReference);

                    drawGraphic(point, symbolPuntos, "esriGeometryPoint");
                    map.centerAndZoom(point, 13);
                    textPoints = convert_To_LongLat(point);
                }

            } else if (lineasArrayGraphics.length > 0) {
                var line = new esri.geometry.Polyline(map.spatialReference);
                if (lineasArrayGraphics[0].geometry != null) {
                    line.addPath(lineasArrayGraphics[0].geometry.paths);
                    line._extent = lineasArrayGraphics[0].geometry._extent;

                    drawGraphic(line, symbolLineas, "esriGeometryPolyline");
                    map.setExtent(line.getExtent().expand(1.5));

                    textPoints = convert_To_LongLat(line);
                }

            } else if (poligonosArrayGraphics.length > 0) {
                var poligon = new esri.geometry.Polygon(map.spatialReference);
                if (poligonosArrayGraphics[0].geometry != null) {
                    poligon.addRing(poligonosArrayGraphics[0].geometry.rings); //Adds the ring to the polygon
                    poligon._extent = poligonosArrayGraphics[0].geometry._extent;

                    drawGraphic(poligon, symbolPoligonos, "esriGeometryPolygon");
                    map.setExtent(poligon.getExtent().expand(1.5));

                    textPoints = convert_To_LongLat(poligon);
                }
            }

            $("#geoLocationAddress").html(textPoints);
        }

        unBlock();
    });

}



// puntosLeidos = puntoLeido[]
// tipoObjeto = 1 -> punto, 2 -> linea, 3 -> poligono
function pintarObjeto(puntosLeidos, tipoObjeto, comprobar) {
    var result = true;
    if (comprobar)
        result = controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, null);
    if (result) {
        //Remove all graphics si Accept is selected
        RemoveAllGraphicsfromView();

        switch (tipoObjeto) {
            case 1:
                if (puntosLeidos.length > 0) {
                    map.on("load", putMarkerPoint(puntosLeidos));
                }
                break;
            case 2:
                if (puntosLeidos.length > 0) {
                    map.on("load", putMarkerLine(puntosLeidos));
                }
                break;
            case 3:
                if (puntosLeidos.length > 0) {
                    map.on("load", putMarkerPolygon(puntosLeidos));
                }
                break;
        }
    }
}

//Draw a point
function putMarkerPoint(puntosLeidos) {
    var p = puntosLeidos[0];
    var point = new esri.geometry.Point(parseFloat(p.x), parseFloat(p.y), map.spatialReference);

    point = esri.geometry.geographicToWebMercator(point);

    addGraphicToMap(point, symbolPuntos, "esriGeometryPoint");
    map.centerAndZoom(point, 13);
}


//Draw a line
function putMarkerLine(puntosLeidos) {

    var path = new Array();

    for (var i = 0; i < puntosLeidos.length; i++) {
        var p = puntosLeidos[i];
        var point = "";
        if (p instanceof Array) {
            point = new esri.geometry.Point(parseFloat(p[0]), parseFloat(p[1]), map.spatialReference);
        } else {
            point = new esri.geometry.Point(parseFloat(p.x), parseFloat(p.y), map.spatialReference);
        }

        point = esri.geometry.geographicToWebMercator(point);

        path.push(point); //Add the point to the path of the line	
    }

    var line = new esri.geometry.Polyline(map.spatialReference);
    line.addPath(path); //Adds the path to the line

    var graphic = addGraphicToMap(line, symbolLineas, "esriGeometryPolyline");

    map.setExtent(graphic.geometry.getExtent().expand(1.5));
}


//Draw a polygon
function putMarkerPolygon(puntosLeidos) {

    var ring = new Array();

    for (var i = 0; i < puntosLeidos.length; i++) {
        var p = puntosLeidos[i];
        var point = "";
        if (p instanceof Array) {
            point = new esri.geometry.Point(parseFloat(p[0]), parseFloat(p[1]), map.spatialReference);
        } else {
            point = new esri.geometry.Point(parseFloat(p.x), parseFloat(p.y), map.spatialReference);

        }
        point = esri.geometry.geographicToWebMercator(point);
        ring.push(point); //Add the point to the path of the line	
    }

    var poligon = new esri.geometry.Polygon(map.spatialReference);
    poligon.addRing(ring); //Adds the path to the line

    var graphic = addGraphicToMap(poligon, symbolPoligonos, "esriGeometryPolygon");

    map.setExtent(graphic.geometry.getExtent().expand(1.5));
}


//This function adds an object (point, line or polygon to the map)
function addGraphicToMap(object, symbol, type) {
    var newGraphic = new esri.Graphic(object, symbol, null);

    var temptSelected = new templateSelected();

    for (var i = 0; i < templatePicker.featureLayers.length; i++) {
        var featur = templatePicker.featureLayers[i];

        if (featur.geometryType == type) {
            //map.graphics.add(newGraphic);

            temptSelected.featureLayer = featur;
            temptSelected.template = featur.templates[0];
            temptSelected.featureLayer.add(newGraphic);
        }
    }

    anadirAccion(temptSelected.featureLayer, newGraphic, ActionsEnum["Anadir"]);
    return newGraphic;

}

//This function draw an object (point, line or polygon to the map)
function drawGraphic(object, symbol, type) {
    var newGraphic = new esri.Graphic(object, symbol, null);

    var temptSelected = new templateSelected();

    for (var i = 0; i < templatePicker.featureLayers.length; i++) {
        var featur = templatePicker.featureLayers[i];

        if (featur.geometryType == type) {
            //map.graphics.add(newGraphic);

            temptSelected.featureLayer = featur;
            temptSelected.template = featur.templates[0];
            temptSelected.featureLayer.add(newGraphic);
        }
    }
    return newGraphic;
}


function templateSelected() {
    this.featureLayer;
    this.item = undefined;
    this.simbolInfo = undefined;
    this.template;
    this.type = undefined;
}


function formatearCoordenada(punto) {
    return parseFloat(punto).toFixed(4);
}

/**
 * This functions receives a geometry and converts x and y to Long Lat
 * @param geometryGraphic
 * @returns {String} An string with x, y converted to long lat for showing in the screen
 */
function convert_To_LongLat(geometryGraphic) {
    var textPoints = "";

    var geometry = geometryGraphic.type;

    if (geometry == "point") {
        var p = esri.geometry.xyToLngLat(geometryGraphic.x, geometryGraphic.y);
        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[1]);

    } else if (geometry == "polyline") {
        var path = geometryGraphic.paths[0];

        for (var j = 0; j < path.length; j++) {
            var p = "";
            if (path[0][j] instanceof Array) {
                var ruta = path[0];
                var puntoPath = "";
                for (k = 0; k < ruta.length; k++) {
                    puntoPath = ruta[k];
                    p = esri.geometry.xyToLngLat(puntoPath[0], puntoPath[1]);

                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[1]);


                    if (j < ruta.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }

            } else {
                p = esri.geometry.xyToLngLat(path[j][0], path[j][1]);
                textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[1]);

                if (j < path.length - 1) {
                    textPoints = textPoints + "<br><br>";
                }
            }
        }

    } else if (geometry == "polygon") {

        var ring = geometryGraphic.rings[0];
        for (var j = 0; j < ring.length; j++) {
            var p = "";
            if (ring[0][j] instanceof Array) {

                var ruta = ring[0];
                var puntoPath = "";
                for (k = 0; k < ruta.length; k++) {
                    puntoPath = ruta[k];
                    p = esri.geometry.xyToLngLat(puntoPath[0], puntoPath[1]);

                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[1]);

                    if (j < ruta.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }
            } else {
                p = esri.geometry.xyToLngLat(ring[j][0], ring[j][1]);

                textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[1]);

                if (j < ring.length - 1) {
                    textPoints = textPoints + "<br><br>";
                }
            }


        }
    }

    return textPoints;
}



function formatCoordinates(geometryGraphic) {
    var textPoints = "";
    if (geometryGraphic != null && geometryGraphic != undefined) {
        var geometry = geometryGraphic.type;

        if (geometry == "point") {
            textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(geometryGraphic.x) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(geometryGraphic.y);

        } else if (geometry == "polyline") {
            var path = geometryGraphic.paths[0];

            for (var j = 0; j < path.length; j++) {
                var p = "";
                if (path[0][j] instanceof Array) {
                    var ruta = path[0];
                    var puntoPath = "";
                    for (k = 0; k < ruta.length; k++) {
                        puntoPath = ruta[k];
                        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(puntoPath[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(puntoPath[1]);
                        if (j < ruta.length - 1) {
                            textPoints = textPoints + "<br><br>";
                        }
                    }

                } else {
                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(path[j][0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(path[j][1]);
                    if (j < path.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }
            }

        } else if (geometry == "polygon") {

            var ring = geometryGraphic.rings[0];
            for (var j = 0; j < ring.length; j++) {
                var p = "";
                if (ring[0][j] instanceof Array) {

                    var ruta = ring[0];
                    var puntoPath = "";
                    for (k = 0; k < ruta.length; k++) {
                        puntoPath = ruta[k];

                        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(puntoPath[0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(puntoPath[1]);

                        if (j < ruta.length - 1) {
                            textPoints = textPoints + "<br><br>";
                        }
                    }
                } else {
                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(ring[j][0]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(ring[j][1]);

                    if (j < ring.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }


            }
        }
    }

    return textPoints;
}



var output = new Array();

var posicionCheck;
var totalLLamadas = 0;

require([
    "esri/map",
    "esri/dijit/Geocoder",
    "esri/toolbars/draw",
    "esri/toolbars/edit",
    "esri/graphic",
    "esri/geometry/Point",
    "esri/geometry/Multipoint",
    "esri/geometry/webMercatorUtils",
    "esri/config",

    "esri/layers/FeatureLayer",
    "esri/tasks/query",
    "esri/tasks/QueryTask",

    "esri/symbols/SimpleMarkerSymbol",
    "esri/symbols/SimpleLineSymbol",
    "esri/symbols/SimpleFillSymbol",

    "esri/dijit/editing/TemplatePicker",

    "dojo/_base/array",
    "dojo/_base/event",
    "dojo/_base/lang",
    "dojo/_base/Color",
    "dojo/parser",
    "dijit/registry",

    "dijit/layout/BorderContainer", "dijit/layout/ContentPane",
    "dijit/form/Button", "dojo/domReady!"
], function (
    maps, Geocoder, Draw, Edit, Graphic, Point, Multipoint, webMercatorUtils, esriConfig,
    FeatureLayer, Query, QueryTask,
    SimpleMarkerSymbol, SimpleLineSymbol, SimpleFillSymbol,
    TemplatePicker,
    arrayUtils, event, lang, Color, parser, registry
) {


        //Setting variables for testing
        //language = "en";
        //visual_output_id = 1;
        //objectType = "vp";

        //Setting variables for testing
        //language = "en";
        //visual_output_id = "3";
        //objectType = "vo";

        language = $('#language').val();
        visual_output_id = $('#visualObjectId').val();
        objectType = $('#objectType').val();


        /*************************/

        if (objectType.toLocaleLowerCase() == "vo") {
            urlPoint = FEATURE_POINT_URL;
            urlLine = FEATURE_LINE_URL;
            urlPolygon = FEATURE_POLYGON_URL;
        } else {
            urlPoint = FEATURE_PROJECT_POINT_URL;
            urlLine = FEATURE_PROJECT_LINE_URL;
            urlPolygon = FEATURE_PROJECT_POLYGON_URL;
        }

        //Set the proper language
        setAppLanguage(language);

        parser.parse();

        // refer to "Using the Proxy Page" for more information:  https://developers.arcgis.com/en/javascript/jshelp/ags_proxy.html
        esriConfig.defaults.io.proxyUrl = "/proxy";

        // This service is for development and testing purposes only. We recommend that you create your own geometry service for use within your applications.
        esriConfig.defaults.geometryService = new esri.tasks.GeometryService(GEOMETRY_SERVICE);

        elementosAGuardar = new Array();
        //ActionsEnum = Object.freeze({"Anadir":1, "Modificar":2, "Eliminar":3}); 
        ActionsEnum = ["Anadir", "Modificar", "Eliminar"];
        ActionsEnum["Anadir"] = 1;
        ActionsEnum["Modificar"] = 2;
        ActionsEnum["Eliminar"] = 3;


        map = new maps("map", {
            basemap: "streets",
            center: [-83.244, 42.581],
            zoom: 3
        });


        /*******************************************
         * 1. Busqueda de Calle, ciudad, pais...
         * 
         * 				 ADDRESS
         *******************************************/
        geocoder = new Geocoder({
            map: map
        }, "searchDireccion");

        geocoder.startup();
        geocoder.hide(); //Hiding the widget. It is called through other input.

        //$("#address").click(function() { $("#address").val(""); });
        //$("#address").change(function() { inputAddressOnChange();});
        $("#address").keydown(function () { setupAutoComplete(); });



        /**
        * FIND BUTTON ADDRESS
        */
        $("#FindAddress").click(function () {
            inputAddressOnChange();

            //Set the type of the select
            //typeSelect = $("#selectType").val().concat();
            typeSelect = $("input[name=selectType]").val().concat();

        });


        //Saves changes in the bbdd
        $("#Add").click(function () {

            if ($("#radio_administrationLavelName").is(":disabled")) {
                $("#radio_administrationLavelName").click();
            }

            if (firstLevel == undefined || firstLevel == "") {
                //alert(typeLang.ALERT_ADM_LEVELS);
                new Messi(typeLang.ALERT_ADM_LEVELS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                alertAdmLevels = true;
            } else {
                updateAdministrativeLevels(firstLevel, secondLevel, thirdLevel);
            }
        });


        comproborBotonAnadir();

        function inputAddressOnChange() {
            var valorInput = $("#address").val();
            if (valorInput != "" && valorInput != undefined) {

                $("#AddressIDDiv").show();

                geocoder.value = valorInput;

                var def = geocoder.find();
                def.then(function (res) {
                    geocodeResults(res, 12);
                    writeAddress();

                    firstLevel = $("#firstAddress").text();
                    secondLevel = $("#secondAddress").text();
                    thirdLevel = $("#thirdAddress").text();
                });
            } else {
                //alert(typeLang.ALERT_ADDRESS);
                new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
            }


        }

        function geocodeResults(places, zoomValue) {
            // Translate features to places...
            places = places.results;
            if (places.length > 0) {
                // Zoom to results
                zoomToPlaces(places, zoomValue);
            } else {
                // alert(typeLang.ALERT_ADDRESS);
                new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
            }
        }


        function zoomToPlaces(places, zoomValue) {
            var point = new Point(places[0].feature.geometry);
            map.centerAndZoom(point, zoomValue);

            //TODO call the service in order to get the administrative levels
            var pointL = new puntoLeido();
            var admLevels = new AdministrativeLevel(pointL);

            $("#firstAddress").html(admLevels.get_First_Country());
            $("#secondAddress").html(admLevels.get_Second_District());
            $("#thirdAddress").html(admLevels.get_Third_Municipality());
        }


        //Writing the Address in the modal window
        function writeAddress() {
            var valorInput = $("#address").val();
            var AddressP = $(".operationData .data_address .data1").first().find("p").last();
            AddressP.html(valorInput);
        }




        //AUTOCOMPLETE functionality ESRI

        //      function inputAddressAutocomplete(response){
        //       	  var valorInput = $("#address").val();
        //       	  if(valorInput != "" && valorInput != undefined){
        //       		  
        //       		     // $("#AddressIDDiv").show();
        //       		 
        // 		      	  geocoder.value = valorInput;
        // 		      	  
        // 		      	  var def = geocoder.find();
        // 		          def.then(function(res){
        // 		        	  var result =[];
        // 		        	  
        // 		        	  for(var i = 0; i< res.results.length; i++){
        // 		        		  var item = res.results[i];
        // 		        		  result.push({
        // 		        			  value:item.name,
        // 		        			  feature:item.feature,
        // 		        			  extent:item.extent		        			  
        // 		        		  });  
        // 		        	  }
        // 		        	  
        // 		        	  response(result);
        //
        // 		          });
        //       	  }else{
        //       		  alert(typeLang.ALERT_ADDRESS);
        //       	  }
        //       }
        //       
        //       
        //      //For Address searching 
        //      function setupAutoComplete() {
        //          //Create the autocomplete
        //          $("#address").autocomplete({
        //                source: function(request, response) {inputAddressAutocomplete(response);},
        //              minLength: 4,
        //              open: function(event, ui){           	  
        //                  $(this).autocomplete('widget').css('z-index', 2000);
        //                                    
        //                  $(this).autocomplete('widget').css({
        //                	  width: $("#idDivAddress").width(),                	  
        //                	  'background-color': 'white',
        //                	  'font-size': '11px',
        //                	  'font-weigth':'bold'              	  
        //                  });
        //    
        //                  return false;
        //              },
        //              appendTo: "#addressSuggestion",
        //              select: function(event, ui) {
        //            	 output = new Array();
        //            	 
        //            	 var point = new puntoLeido();
        //            	 var p = esri.geometry.xyToLngLat(ui.item.feature.geometry.x,ui.item.feature.geometry.y);
        //                 point.x = p[0];
        //                 point.y = p[1];
        //                 	                   
        //                 output.push(point);
        //                 
        //                 var tipoObjeto = getTypeObjeto(output);
        //          	     pintarObjeto(output, tipoObjeto);
        //                 
        //            	 showCarrusel(output);
        //            	 
        //          	   $("#ulFile").removeClass("hide");
        //          	   $("#liFile1").attr('style',"display:none;");
        //          	   $("#liFile2").attr('style',"display:none;");
        //          	   $("#divCarrusel").attr('style',"visibility:visible;");            	   
        //            	 
        //                 var point = new Point(ui.item.feature.geometry);
        //                 map.centerAndZoom(point,18);
        //              }
        //          })
        //          .data("ui-autocomplete")._renderItem = function( ul, item ) {
        //  			  return $( "<li></li>" )
        //  				.data("item.autocomplete", item)
        //  				.append("<a><b>"+ item.label + "</b></a><br><br>")
        //  				.appendTo(ul);
        //  		};
        //     }



        /***********************************************
         *         COORDINATE
         ***********************************************/

        $("#Find").on("click", function () {
            output = new Array();
            if ($("#coordinates").val() != "" && $("#coordinates").val() != undefined) {
                $("#principal").hide();
                var coordString = readCoordinates($("#coordinates").val());

                //Set the type of the select
                //typeSelect = $("#selectType").val().concat();
                typeSelect = $("input[name=selectType]").val().concat();

            } else {
                $("#divCarrusel").attr('style', "visibility:hidden;");
                $("#btnCarPrevious").attr('style', "visibility:hidden;");
                $("#btnCarNext").attr('style', "visibility:hidden;");

                new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                //alert(typeLang.ALERT_COORDINATES_INCORRECT);
            }
        });


        //Reading input coordinates
        function readCoordinates(entrada) {

            var valorInput = entrada;
            var coordenadas = valorInput.split(";");

            var coordString = "";

            for (var i = 0; i < coordenadas.length; i++) {

                var coordenada = coordenadas[i];
                var coordenadaArray = coordenada.split(",");
                if (coordenadaArray != undefined) {

                    if (coordenadaArray[0] != undefined && coordenadaArray[1] != undefined) {
                        var coordX = coordenadaArray[0];
                        var coordY = coordenadaArray[1];

                        if (coordX != undefined && coordY != undefined && !isNaN(coordX) && !isNaN(coordY) && coordX != "" && coordY != "") {
                            var point = new puntoLeido();
                            point.x = coordX;
                            point.y = coordY;

                            output.push(point);

                            coordString += typeLang.Latitude + "=" + formatearCoordenada(coordX) + "; " + typeLang.Longitude + "=" + formatearCoordenada(coordY) + "<br/>";

                        } else {
                            // alert(typeLang.ALERT_COORDINATES_INCORRECT);
                            new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                            output = new Array();
                            break;
                        }
                    } else {
                        new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                        //alert(typeLang.ALERT_COORDINATES_INCORRECT);
                        output = new Array();
                        break;
                    }
                } else {
                    //alert(typeLang.ALERT_COORDINATES_INCORRECT);
                    new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                    output = new Array();
                    break;
                }
            }

            if (output.length > 0) {

                $("#ulFile").removeClass("hide");
                $("#liFile1").attr('style', "display:none;");
                $("#liFile2").attr('style', "display:none;");
                $("#divCarrusel").attr('style', "visibility:visible;");

                locateCoordinates(output);
                return coordString;
            } else {
                //Remove graphics of the map and carrusel
                RemoveAllGraphicsfromView();
                $("#divCarrusel").attr('style', "visibility:hidden;");
                $("#btnCarPrevious").attr('style', "visibility:hidden;");
                $("#btnCarNext").attr('style', "visibility:hidden;");

                //Remove the element to be stored from memory

                for (var i = 0; i < elementosAGuardar.length; i++) {
                    var element = elementosAGuardar[i];
                    if (elementosAGuardar[i].action == 1) {
                        elementosAGuardar.splice(i, 1);
                    }
                }

                $("#Add").prop("disabled", true);

            }

            return "";
        }



        function locateCoordinates(arrayCoordinates) {
            firstLevel = "";
            secondLevel = "";
            thirdLevel = "";
            var tipoObjeto = getTypeObjeto(arrayCoordinates);
            pintarObjeto(arrayCoordinates, tipoObjeto, true);

            showCarrusel(output);

        }



        /*Center a map over a specific country
         * */

        function centrarMapa(country) {
            geocoder.value = country;
            var def = geocoder.find();
            def.then(function (res) {
                geocodeResults(res, 5);
            });
        }


        /*
         * Functionality for drawing points, lines and polygons
         * 
         * Feature Services
         */
        map.on("layers-add-result", initEditing);

        pointServiceLayer = new FeatureLayer(urlPoint, {
            mode: FeatureLayer.MODE_SNAPSHOT,
            outFields: ["*"]
        });

        LineServiceLayer = new FeatureLayer(urlLine, {
            mode: FeatureLayer.MODE_SNAPSHOT,
            outFields: ["*"]
        });


        PolygonServiceLayer = new FeatureLayer(urlPolygon, {
            mode: FeatureLayer.MODE_SNAPSHOT,
            outFields: ["*"]
        });



        /******************************
         *  FILTRADO INICIAL
         ******************************/

        block();

        if (objectType.toLowerCase() == "vo") {
            getOutputVersionID(GET_VISUAL_OUTPUT_VERSION);
        } else {
            getOutputVersionID(GET_VISUAL_PROJECT_VERSION);
        }


        /**
        * This functions retrieves the information of a specific FK_VISUAL_OUTPUT_ID in order to center a map when accessing for the first time
        * 
        * This service retrieves:
        *   VISUAL_OUTPUT_VERSION_ID
        *   LEVEL1
        *   LEVEL2
        *   LEVEL3
        * 
        * params urlService:  Url of the service to be called
        */
        function getOutputVersionID(urlService) {
            //Call  service in order to retrieve VISUAL_OUTPUT_VERSION_ID field.

            if (objectType.toLowerCase() == "vo") {
                var data = "VisualOutputId=" + visual_output_id + "&Isvalidate=" + false;
                ajaxCall(data, urlService, outputVersionIDResponse);
            } else {
                var data = "VisualProjectId=" + visual_output_id + "&Isvalidate=" + false;
                ajaxCall(data, urlService, outputVersionIDResponse);
            }
        }


        /**
         * This function checks if there are data for Isvalidate=false, if not, performs a call with Isvalidate=true
         */
        function outputVersionIDResponse(response) {

            if (response != undefined && response != "" && response != null && response != "{}") {
                var campoVisual = "";

                if (objectType.toLowerCase() == "vo") {
                    campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";
                    //This field corresponding to visual version id 
                    visual_output_version_id = response.VisualoutputversionId;
                } else {
                    campoVisual = "FK_GEO_VISUAL_PROJECT_ID";   //This field corresponding to visual project id
                    visual_output_version_id = response.VisualprojectversionId;
                }


                var level1 = response.Level1;
                var level2 = response.Level2;
                var level3 = response.Level3;


                var query = new Query();


                query.where = campoVisual + " =" + visual_output_version_id;
                query.returnGeometry = true;
                query.outFields = ["*"];
                var queryTask = new QueryTask(urlPoint);

                queryTask.execute(query, function (result) {
                    if (result.features.length > 0) {
                        drawLevelsPrincipal(level1, level2, level3);
                        var text = formatCoordinates(result.features[0].geometry);
                        $("#geoLocationAddress").html(jQuery.trim(text));

                        if (symbolPuntos != undefined) {
                            putMarkerPoint(result.features[0].geometry);
                        }

                        pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                        LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                        getNumberGraphicsFeatures(true, visual_output_version_id);


                    } else {
                        var query = new Query();
                        query.where = campoVisual + " =" + visual_output_version_id;
                        query.returnGeometry = true;
                        query.outFields = ["*"];
                        var queryTask = new QueryTask(urlLine);


                        queryTask.execute(query, function (result) {
                            if (result.features.length > 0) {
                                drawLevelsPrincipal(level1, level2, level3);
                                var text = formatCoordinates(result.features[0].geometry);
                                $("#geoLocationAddress").html(jQuery.trim(text));

                                if (symbolLineas != undefined) {
                                    putMarkerLine(result.features[0].geometry.paths[0]);
                                }

                                pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                                map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                                getNumberGraphicsFeatures(true, visual_output_version_id);

                            } else {
                                var query = new Query();
                                query.where = campoVisual + " =" + visual_output_version_id;
                                query.returnGeometry = true;
                                query.outFields = ["*"];
                                var queryTask = new QueryTask(urlPolygon);

                                queryTask.execute(query, function (result) {
                                    if (result.features.length > 0) {
                                        drawLevelsPrincipal(level1, level2, level3);
                                        var text = formatCoordinates(result.features[0].geometry);
                                        $("#geoLocationAddress").html(jQuery.trim(text));

                                        if (symbolPoligonos != undefined) {
                                            putMarkerPolygon(result.features[0].geometry.rings[0]);
                                        }


                                        pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                        LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                                        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                                        getNumberGraphicsFeatures(true, visual_output_version_id);

                                    } else {

                                        if (objectType.toLowerCase() == "vo") {
                                            var data = "VisualOutputId=" + visual_output_id + "&Isvalidate=" + true;
                                            ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, outputVersionIDTrueResponse);
                                        } else {
                                            var data = "VisualProjectId=" + visual_output_id + "&Isvalidate=" + true;
                                            ajaxCall(data, GET_VISUAL_PROJECT_VERSION, outputVersionIDTrueResponse);
                                        }
                                    }
                                });
                            }
                        });
                    }
                });

            } else {
                console.log("There is no response in function outputVersionIDResponse");

                if (objectType.toLowerCase() == "vo") {
                    var data = "VisualOutputId=" + visual_output_id + "&Isvalidate=" + true;
                    ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, outputVersionIDTrueResponse);
                } else {
                    var data = "VisualProjectId=" + visual_output_id + "&Isvalidate=" + true;
                    ajaxCall(data, GET_VISUAL_PROJECT_VERSION, outputVersionIDTrueResponse);
                }
            }
        }


        /**
         * This function checks if there are data for  Isvalidate=true, if not, center the map in the country
         */
        function outputVersionIDTrueResponse(response) {

            if (response != undefined && response != "" && response != null && response != "{}") {
                var campoVisual = "";

                if (objectType.toLowerCase() == "vo") {
                    campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";
                    //This field corresponding to visual version id 
                    visual_output_version_id = response.VisualoutputversionId;
                } else {
                    campoVisual = "FK_GEO_VISUAL_PROJECT_ID";     //This field corresponding to visual project id
                    visual_output_version_id = response.VisualprojectversionId;
                }

                var level1 = response.Level1;
                var level2 = response.Level2;
                var level3 = response.Level3;


                var query = new Query();
                query.where = campoVisual + " =" + visual_output_version_id;
                query.returnGeometry = true;
                query.outFields = ["*"];
                var queryTask = new QueryTask(urlPoint);

                queryTask.execute(query, function (result) {
                    if (result.features.length > 0) {
                        drawLevelsPrincipal(level1, level2, level3);
                        var text = formatCoordinates(result.features[0].geometry);
                        $("#geoLocationAddress").html(jQuery.trim(text));

                        pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                        LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                        getNumberGraphicsFeatures(true, visual_output_version_id);

                        if (symbolPuntos != undefined) {
                            putMarkerPoint(result.features[0].geometry);
                        }


                    } else {
                        var query = new Query();
                        query.where = campoVisual + " = ' " + visual_output_version_id + "'";
                        query.returnGeometry = true;
                        query.outFields = ["*"];
                        var queryTask = new QueryTask(urlLine);

                        queryTask.execute(query, function (result) {
                            if (result.features.length > 0) {
                                drawLevelsPrincipal(level1, level2, level3);
                                var text = formatCoordinates(result.features[0].geometry);
                                $("#geoLocationAddress").html(jQuery.trim(text));

                                pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                                map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                                getNumberGraphicsFeatures(true, visual_output_version_id);


                                if (symbolLineas != undefined) {
                                    putMarkerLine(result.features[0].geometry.paths[0]);
                                }

                            } else {

                                var query = new Query();
                                query.where = campoVisual + " = ' " + visual_output_version_id + "'";
                                query.returnGeometry = true;
                                query.outFields = ["*"];
                                var queryTask = new QueryTask(urlPolygon);

                                queryTask.execute(query, function (result) {
                                    if (result.features.length > 0) {
                                        drawLevelsPrincipal(level1, level2, level3);
                                        var text = formatCoordinates(result.features[0].geometry);
                                        $("#geoLocationAddress").html(jQuery.trim(text));

                                        pointServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                        LineServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);
                                        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = " + visual_output_version_id);

                                        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                                        getNumberGraphicsFeatures(true, visual_output_version_id);

                                        if (symbolPoligonos != undefined) {
                                            putMarkerPolygon(result.features[0].geometry.rings[0]);
                                        }
                                    } else {
                                        getCountryVisualOutputID();
                                    }
                                });

                            }
                        });
                    }
                });

            } else {
                console.log("There is no data for IsValidate = true");
                getCountryVisualOutputID();
            }
        }







        /**
        * This function retrieves the country of a given visual_output_id in order to center a map
        * 
        * Parameter data: VisualOutputId
        */
        function getCountryVisualOutputID() {

            if (objectType.toLowerCase() == "vo") {
                var data = "VisualOutputId=" + visual_output_id;
                ajaxCall(data, GET_DESCRIPTION_COUNTRY, countryVisualOutputResponse);
            } else {
                var data = "VisualProjectId=" + visual_output_id;
                ajaxCall(data, GET_DESCRIPTION_COUNTRY_VISUAL_PROJECT, countryVisualOutputResponse);
            }
        }


        /**
         * Center de map
         */
        function countryVisualOutputResponse(response) {
            if (response != "" && response != null && response != "") {
                country = response;
                centrarMapa(country);
            }

            var campoVisual = "";

            if (objectType.toLowerCase() == "vo") {
                campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";  //This field corresponding to visual version id 
            } else {
                campoVisual = "FK_GEO_VISUAL_PROJECT_ID";
                //This field corresponding to visual project id
            }

            pointServiceLayer.setDefinitionExpression(campoVisual + " = -1");
            LineServiceLayer.setDefinitionExpression(campoVisual + " = -1");
            PolygonServiceLayer.setDefinitionExpression(campoVisual + " = -1");

            map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
            unBlock();
            $("#Edit").click(); //There is no data to show, so it enters to the Edition mode directly.
        }

        function ajaxCall(data, urlService, successFunction) {
            console.log(data);
            console.log(urlService);

            $.ajax({
                type: "GET",
                url: urlService,
                data: data,
                dataType: "json",
                success: function (response) {
                    successFunction(response);

                },
                error: function (msg) {
                    console.log(typeLang.ERROR_SERVER + ": " + urlService);
                    if (urlService == GET_DESCRIPTION_COUNTRY || urlService == GET_DESCRIPTION_COUNTRY_VISUAL_PROJECT) {

                        var campoVisual = "";

                        if (objectType.toLowerCase() == "vo") {
                            campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";
                            //This field corresponding to visual version id 
                        } else {
                            campoVisual = "FK_GEO_VISUAL_PROJECT_ID";
                            //This field corresponding to visual project id
                        }
                        pointServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                        LineServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                        unBlock();

                    } else if (urlService == GET_VISUAL_OUTPUT_VERSION || urlService == GET_VISUAL_PROJECT_VERSION) {
                        console.log("There is no data");
                        getCountryVisualOutputID();
                    }
                }
            });
        }


        /******************************
         * FIN  FILTRADO INICIAL
         ******************************/


        /* 
         * This functions retrieves the administrativeLevels of a new drawn object (Bing service)
         * The object could be a point, polyline or polygon
         * 
         * params object: the feature (a point, a polyline or a polygon)
         */
        function getAdministrativeLevelsObject(object) {

            var geometryType = object.geometry.type;

            output = new Array();

            switch (geometryType) {
                case "point":
                    var point = new puntoLeido();
                    var p = esri.geometry.xyToLngLat(object.geometry.x, object.geometry.y);
                    point.x = p[0];
                    point.y = p[1];

                    output.push(point);
                    break;
                case "polyline":
                    var path = object.geometry.paths[0];

                    for (var i = 0; i < path.length; i++) {
                        var point = new puntoLeido();
                        var p = esri.geometry.xyToLngLat(path[i][0], path[i][1]);

                        point.x = p[0];
                        point.y = p[1];

                        output.push(point);
                    }

                    break;
                case "polygon":

                    var ring = object.geometry.rings[0];

                    for (var i = 0; i < ring.length; i++) {
                        var point = new puntoLeido();
                        var p = esri.geometry.xyToLngLat(ring[i][0], ring[i][1]);

                        point.x = p[0];
                        point.y = p[1];

                        output.push(point);
                    }
                    break;
            }

            showCarrusel(output);

            $("#ulFile").removeClass("hide");
            $("#liFile1").attr('style', "display:none;");
            $("#liFile2").attr('style', "display:none;");
            $("#divCarrusel").attr('style', "visibility:visible;");

        }


        //This function is executed when Feature layers are loaded
        function initEditing(evt) {
            //window.parent.$(".dinamicModal").height(485);
            window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
            //Set the proper language
            if (language == "en") {
                typeLang = languages.english;
                setLangTexts(typeLang);
            } else if (language == "es") {
                typeLang = languages.spanish;
                setLangTexts(typeLang);
            } else if (language == "fr") {
                typeLang = languages.french;
                setLangTexts(typeLang);
            } else if (language == "pt") {
                typeLang = languages.portuguese;
                setLangTexts(typeLang);
            }


            initCancelButton();


            var currentLayer = null;
            var layers = arrayUtils.map(evt.layers, function (result) {
                return result.layer;
            });

            editToolbar = new Edit(map);
            editToolbar.on("deactivate", function (evt) {
                anadirAccion(currentLayer, evt.graphic, ActionsEnum["Modificar"]);
            });

            arrayUtils.forEach(layers, function (layer) {
                var editingEnabled = false;
                layer.on("dbl-click", function (evt) {
                    event.stop(evt);
                    if (EditionMode) {
                        if (editingEnabled === false) {
                            editingEnabled = true;
                            editToolbar.activate(Edit.EDIT_VERTICES, evt.graphic);
                        } else {
                            currentLayer = this;
                            editToolbar.deactivate();
                            editingEnabled = false;
                        }
                    }
                });

                layer.on("click", function (evt) {
                    event.stop(evt);
                    if (EditionMode) {
                        if (evt.ctrlKey === true || evt.metaKey === true) {  //delete feature if ctrl key is depressed

                            anadirAccion(layer, evt.graphic, ActionsEnum["Eliminar"]);

                            layer.remove(evt.graphic);
                            currentLayer = this;
                            editToolbar.deactivate();
                            editingEnabled = false;
                        }
                    }
                });
            });

            templatePicker = new TemplatePicker({
                featureLayers: layers,
                rows: "auto",
                columns: 4,
                grouping: true,
                style: "height: auto; overflow: auto; display:none"
            }, "templatePickerDiv");

            templatePicker.startup();


            drawToolbar = new Draw(map);


            templatePicker.on("selection-change", function () {
                if (templatePicker.getSelected()) {
                    selectedTemplate = templatePicker.getSelected();
                }

                switch (selectedTemplate.featureLayer.geometryType) {
                    case "esriGeometryPoint":
                        drawToolbar.activate(Draw.POINT);
                        break;
                    case "esriGeometryPolyline":
                        drawToolbar.activate(Draw.POLYLINE);
                        break;
                    case "esriGeometryPolygon":
                        drawToolbar.activate(Draw.POLYGON);
                        break;
                }
            });

            drawToolbar.on("draw-end", function (evt) {
                //TODO
                //window.parent.$(".dinamicModal").height(625);
                window.parent.$(".dinamicModal").height($('.pie.edit').height() + $('.central').height() + 30);
                drawToolbar.deactivate();
                editToolbar.deactivate();
                var newAttributes = lang.mixin({}, selectedTemplate.template.prototype.attributes);
                var newGraphic = new Graphic(evt.geometry, null, newAttributes);
                anadirAccion(selectedTemplate.featureLayer, newGraphic, ActionsEnum["Anadir"]);

                getAdministrativeLevelsObject(newGraphic);
                selectedTemplate.featureLayer.add(newGraphic);
                getCoordinatesEditButton();

                firstLevel = "";
                secondLevel = "";
                thirdLevel = "";
            });



        }



        $("#puntoButton").click(function () { clickPuntoEditButton(); });
        $("#lineaButton").click(function () { clickLineaEditButton(); });
        $("#poligonoButton").click(function () { clickPoligonoEditButton(); });


        function clickPuntoEditButton() {
            if (objectType == "vp") {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, PUNTOS_WIDGET);
            } else {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, PUNTOS_WIDGET_VO);
            }
        }

        function clickLineaEditButton() {
            if (objectType == "vp") {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, LINEAS_WIDGET);
            } else {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, LINEAS_WIDGET_VO);
            }
        }

        function clickPoligonoEditButton() {
            if (objectType == "vp") {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, POLIGONOS_WIDGET);
            } else {
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, POLIGONOS_WIDGET_VO);
            }
        }


        $("#btnCarPrevious").click(function () {
            clickarCarPrevious();
        });
        $("#btnCarNext").click(function () {
            clickarCarNext();
        });

        $("#radio_administrationLavelName").change(function () {
            if ($(this).is(':checked')) {
                posicionCheck = parseInt($("#carPos").html(), 10);
            }
        });




        //TODO functions button previous - next
        function clickarCarPrevious() {
            var position = parseInt($("#carPos").html(), 10) - 1;
            var punto = output[position - 1];
            $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.x) + " " + typeLang.Longitude + "=" + formatearCoordenada(punto.y));
            $("#carPos").html(position);
            if (position == posicionCheck) {
                $("#radio_administrationLavelName").prop('checked', true);
            } else {
                $("#radio_administrationLavelName").prop('checked', false);
            }

            if (position == 1) {
                $("#btnCarPrevious").attr('style', "visibility:hidden;");
            }
            $("#btnCarNext").attr('style', "visibility:visible;");


            if (arrayAdministrativeLevels.length > 0) {

                for (var i = 0; i < arrayAdministrativeLevels.length; i++) {
                    var admLevel = arrayAdministrativeLevels[i];

                    if (punto.x == admLevel.x && punto.y == admLevel.y) {
                        if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                            var adm = listAdm[position - 1];
                            drawLevels(adm.level1, adm.level2, adm.level3);
                        } else {
                            //getAdministrativeLevelsEsri(punto);
                        }

                    }
                }

            } else {
                if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                    var adm = listAdm[position - 1];
                    drawLevels(adm.level1, adm.level2, adm.level3);
                } else {
                    //getAdministrativeLevelsEsri(punto);
                }
            }


        };

        function clickarCarNext() {

            var position = parseInt($("#carPos").html(), 10) + 1;
            var punto = output[position - 1];
            $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.x) + " " + typeLang.Longitude + "=" + formatearCoordenada(punto.y));
            $("#carPos").html(position);
            if (position == posicionCheck) {
                $("#radio_administrationLavelName").prop('checked', true);
            } else {
                $("#radio_administrationLavelName").prop('checked', false);
            }

            if (position == output.length) {
                $("#btnCarNext").attr('style', "visibility:hidden;");
            }
            $("#btnCarPrevious").attr('style', "visibility:visible;");


            if (arrayAdministrativeLevels.length > 0) {

                for (var i = 0; i < arrayAdministrativeLevels.length; i++) {
                    var admLevel = arrayAdministrativeLevels[i];

                    if (punto.x == admLevel.x && punto.y == admLevel.y) {
                        if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                            var adm = listAdm[position - 1];
                            drawLevels(adm.level1, adm.level2, adm.level3);
                        } else {
                            //getAdministrativeLevelsEsri(punto);
                        }
                    }
                }

            } else {
                if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                    var adm = listAdm[position - 1];
                    drawLevels(adm.level1, adm.level2, adm.level3);
                } else {
                    //getAdministrativeLevelsEsri(punto);
                }
            }

        };


        /**
        * CLEAR MAP FUNCTIONALITY
        * 
        *  Clear all changes that are no validated from tables of point, polylines and polygons 
        *  and center the map in the point validated in case that it exists, otherwise the map will be centered in the country
        *  
        */
        $("#clearMapId").click(function () {
            Messi.ask(typeLang.ALERT_GRAPHICS_REMOVE, function (val) {
                if (val == "Y") {
                    //Remove data from memory
                    elementosAGuardar = new Array();
                    comproborBotonAnadir();

                    puntosArrayGraphics = new Array();
                    lineasArrayGraphics = new Array();
                    poligonosArrayGraphics = new Array();

                    firstLevel = "";
                    secondLevel = "";
                    thirdLevel = "";
                    typeSelect = "";

                    $("#coordinates").val("");

                    $("#AddressIDDiv").hide();
                    $("#address").val("");

                    $("#divCarrusel").hide();
                    $("#BrowseInput").val("");
                    $("#txtfileName").html("No file selected");
                    $("#geoLocationAddress").html("");

                    drawLevelsPrincipal("", "", "");
                    drawLevels("", "", "");
                    window.parent.$(".dinamicModal").height($('.pie.edit').height() + $('.central').height() + 30);
                    if (objectType.toLowerCase() == "vo") {
                        var data = "VisualOutputId=" + visual_output_id + "&Isvalidate=" + false;
                        ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, removeFalseGraphics);
                    } else {
                        var data = "VisualProjectId=" + visual_output_id + "&Isvalidate=" + false;
                        ajaxCall(data, GET_VISUAL_PROJECT_VERSION, removeFalseGraphics);
                    }
                }

            });
        });




        /**
           * This function checks if there are data for Isvalidate=false and delete them
           */
        function removeFalseGraphics(response) {
            RemoveAllGraphicsfromView();
            if (response != undefined && response != "" && response != null) {
                var campoVisual = "";

                if (objectType.toLowerCase() == "vo") {
                    campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";  //This field corresponding to visual version id 
                    visual_output_version_id = response.VisualoutputversionId;
                } else {
                    campoVisual = "FK_GEO_VISUAL_PROJECT_ID";
                    //This field corresponding to visual project id
                    visual_output_version_id = response.VisualprojectversionId;
                }


                var query = new Query();
                query.where = campoVisual + " =" + visual_output_version_id;
                query.returnGeometry = true;
                query.outFields = ["*"];
                var queryTask = new QueryTask(urlPoint);

                queryTask.execute(query, function (result) {
                    if (result.features.length > 0) {
                        //Delete features
                        var attr = result.features[0].attributes;
                        var graphic1 = new esri.Graphic(result.features[0].geometry, null, attr);

                        pointServiceLayer.applyEdits(null, null, [graphic1]);


                    } else {
                        var query = new Query();
                        query.where = campoVisual + " =" + visual_output_version_id;
                        query.returnGeometry = true;
                        query.outFields = ["*"];
                        var queryTask = new QueryTask(urlLine);

                        queryTask.execute(query, function (result) {
                            if (result.features.length > 0) {
                                //Delete features
                                var attr = result.features[0].attributes;
                                var graphic1 = new esri.Graphic(result.features[0].geometry, null, attr);

                                LineServiceLayer.applyEdits(null, null, [graphic1]);

                            } else {
                                var query = new Query();
                                query.where = campoVisual + " =" + visual_output_version_id;
                                query.returnGeometry = true;
                                query.outFields = ["*"];
                                var queryTask = new QueryTask(urlPolygon);

                                queryTask.execute(query, function (result) {
                                    if (result.features.length > 0) {
                                        //Delete features
                                        var graphic = new esri.Graphic(result.features[0].geometry, null, result.features[0].attributes);

                                        PolygonServiceLayer.applyEdits(null, null, [graphic]);
                                    }
                                    //						  	    			    	else{
                                    //						  	    	            		RemoveAllGraphicsfromView();
                                    //						 	    	            		//clear all changes that are no validated and center the map in the point validated
                                    //						  	    	            		 if(objectType.toLowerCase() == "vo"){
                                    //							  		    	       		     	 var data = "VisualOutputId="+visual_output_id + "&Isvalidate="+true;	        
                                    //							 		    	       		         ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, outputVersionIDTrueResponse); 
                                    //											              }else{    		  
                                    //							  		    	       	             var data = "VisualProjectId="+visual_output_id + "&Isvalidate="+true;	         
                                    //							 		    	       		         ajaxCall(data, GET_VISUAL_PROJECT_VERSION, outputVersionIDTrueResponse); 
                                    //							  	    	       		      }   			    	            		 	            	  
                                    //						  	    	            	}  			    	
                                });
                            }
                        });
                    }
                });

            } else {
                console.log("There is no response in function outputVersionIDResponse");
            }
        }



        //This event stores the administrative levels to be sent to the service
        $("#radio_administrationLavelName").click(function () {
            firstLevel = $("#firstCountry").text().concat();
            secondLevel = $("#secondDistrict").text().concat();
            thirdLevel = $("#thirdMunicipality").text().concat();
        });





        function initCancelButton() {
            //TODO

            $("#cancelEdit").on("click", function () {
                hideEditInfo();
                EditionMode = false;
                window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
            });

            //$("#cancelEdit").on("click", function () { hideEditInfo(); EditionMode = false; window.parent.$(".dinamicModal").height(485) });
            //$("#cancelEdit").on("click", function () { hideEditInfo(); EditionMode = false; });
        };

        function hideEditInfo() {

            RemoveAllGraphicsfromView();

            $(".pie").removeClass("hide");
            $(".edit").addClass("hide");
            $(".add").addClass("hide");
            $(".operationData .data_address").addClass("hide");
            $(".coordinates").addClass("hide");
            $(".address_info").addClass("hide");
            $(".file").addClass("hide");
            $(".operationData .data").removeClass("hide");
            //TODO
            //var select_addType = $("#selectType").data("kendoDropDownList");
            //select_addType.select(1);
            $("#selectTypeCoordinates").attr('checked', 'checked');


            $("#txtfileName").html(typeLang.NoFileSelected);


            if (objectType.toLowerCase() == "vo") {
                getOutputVersionID(GET_VISUAL_OUTPUT_VERSION);
            } else {
                getOutputVersionID(GET_VISUAL_PROJECT_VERSION);
            }


            $("#principal").show();


            //Remove data from memory
            elementosAGuardar = new Array();
            comproborBotonAnadir();

            firstLevel = "";
            secondLevel = "";
            thirdLevel = "";
            typeSelect = "";
        }
    });



/**
 *  PUBLIC FUNCTIONS
 * 
 */


/*
 * Handle file reading 
 */
function manejadorDeArchivos(files) {
    output = new Array();

    // files is a FileList of File objects. List some properties.

    for (var i = 0, f; f = files[i]; i++) {
        var ext = f.name.toLowerCase().split('.').pop();
        if ($.inArray(ext, ['txt', 'shp', 'kml']) >= 0) {

            fr = new FileReader();
            if (f.type == "text/plain" || ext == "txt") {
                fr.readAsText(f);
                fr.onloadend = function () {
                    tratarTexto(this.result);
                };

            } else if (f.type == "application/vnd.google-earth.kml+xml" || ext == "kml") {
                fr.readAsText(f);
                fr.onloadend = function () {
                    var parser = new DOMParser();
                    var doc = parser.parseFromString(this.result, "text/xml");
                    tratarGeoJson(toGeoJSON.kml(doc));
                };

            } else if (f.type == "" || ext == "shp") {
                fr.readAsBinaryString(f);
                fr.onloadend = function () {
                    try {
                        shapefile = new Shapefile(this.result, function (data) {
                            tratarGeoJson(data.geojson);
                        }
                        );
                    } catch (e) {
                        new Messi(typeLang.ALERT_4, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                        resetCarrusel();
                    }
                };
            } else {
                new Messi(typeLang.ALERT_4, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                resetCarrusel();
            }
        } else {
            new Messi(typeLang.ALERT_4, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
            resetCarrusel();
        }
    }
}

function tratarGeoJson(geoJson) {
    //	if (geoJson.features.length > 1) {
    //		new Messi(typeLang.ALERT_3, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
    //	      resetCarrusel();
    //	}else{
    //for (var i=0; i<geoJson.features.length; i++){
    if (geoJson.features.length > 0) {
        deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics);
        for (var i = 0; i < 1; i++) {
            var feature = geoJson.features[i];
            if (feature.geometry.type == "LineString") {
                var feature = geoJson.features[i];
                for (var j = 0; j < feature.geometry.coordinates.length; j++) {
                    var punto = new puntoLeido();
                    punto.x = feature.geometry.coordinates[j][0];
                    punto.y = feature.geometry.coordinates[j][1];
                    output.push(punto);
                }
            } else {
                if (feature.geometry.coordinates.length > 0) {
                    var coordenada = feature.geometry.coordinates[0];
                    if (Array.isArray(coordenada)) {
                        for (var k = 0; k < coordenada.length; k++) {
                            var punto = new puntoLeido();
                            punto.x = coordenada[k][0];
                            punto.y = coordenada[k][1];
                            output.push(punto);      
                        }
                        break;
                    } else {
                        var punto = new puntoLeido();
                        punto.x = feature.geometry.coordinates[0];
                        punto.y = feature.geometry.coordinates[1];
                        output.push(punto);
                        break;
                    }
                }
            }
        }
        //	}
        tratarOutput(output);
    } else {
        new Messi(typeLang.ALERT_4, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
    }
}

function tratarTexto(texto) {
    resultado = texto;
    lineas = resultado.split("\r\n");
    if (lineas.length > 1) {
        new Messi(typeLang.ALERT_3, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        resetCarrusel();
    } else {
        for (var i = 0; i < lineas.length; i++) {
            linea = lineas[i];

            puntos = linea.split(';');
            for (var j = 0; j < puntos.length; j++) {
                var cadenaPunto = puntos[j];

                var cadenaPuntoSplit = cadenaPunto.split(",");
                if (cadenaPuntoSplit.length == 2) {
                    var coorX = cadenaPuntoSplit[0];
                    var coorY = cadenaPuntoSplit[1];
                    var punto = new puntoLeido();
                    if (coorX == "" || coorY == "" || isNaN(coorX) || isNaN(coorY)) {
                        new Messi(typeLang.ALERT_FILE_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                        resetCarrusel();
                        output = new Array();
                        break;
                    } else {
                        punto.x = coorX;
                        punto.y = coorY;
                        output.push(punto);
                    }

                } else {
                    new Messi(typeLang.ALERT_FILE_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                    resetCarrusel();
                    output = new Array();
                    break;
                }
            }
        }

        tratarOutput(output);
    }
}


function resetCarrusel() {
    $("#divCarrusel").attr('style', "visibility:hidden;");
    $("#btnCarPrevious").attr('style', "visibility:hidden;");
    $("#btnCarNext").attr('style', "visibility:hidden;");
}

function tratarOutput(output) {
    if (output.length > 0) {
        var tipoObjeto = getTypeObjeto(output);
        showCarrusel(output);
        $("#divCarrusel").attr('style', "visibility:visible;");
        deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
        pintarObjeto(output, tipoObjeto, false);
    } else {
        //	Remove graphics of the map and carrusel
        RemoveAllGraphicsfromView();
        resetCarrusel();

        //	Remove the element to be stored from memory
        for (var i = 0; i < elementosAGuardar.length; i++) {
            var element = elementosAGuardar[i];
            if (elementosAGuardar[i].action == 1) {
                elementosAGuardar.splice(i, 1);
            }
        }
        $("#Add").prop("disabled", true);
    }
}

//output : array <Punto>
function getTypeObjeto(array) {
    var tipoObjeto = 0;

    if (array.length == 1) {

        //it is a point(1)
        tipoObjeto = 1;
    } else {
        punto = array[0];

        ultimoPunto = array[array.length - 1];
        if (punto.x != ultimoPunto.x || punto.y != ultimoPunto.y) {
            //it is a line (2)	
            tipoObjeto = 2;
        } else {
            //it is a polygon (3)
            tipoObjeto = 3;
        }
    }
    return tipoObjeto;
}



function showCarrusel(array) {
    listAdm = new Array();
    var punto = "";
    if (array.length == 1) {
        punto = array[0];
        $("#carPos").html("1");
        $("#carTam").html("1");
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.x) + " " + typeLang.Longitude + "=" + formatearCoordenada(punto.y));
        $("#radio_administrationLavelName").prop('checked', true);
        $("#radio_administrationLavelName").prop('disabled', true);
        $("#radio_administrationLavelName").click();

        $("#btnCarPrevious").attr('style', "visibility:hidden;");
        $("#btnCarNext").attr('style', "visibility:hidden;");
        posicionCheck = 1;
        getAdministrativeLevelsEsri(punto, 0);
    } else {
        $("#carPos").html("1");
        $("#carTam").html(array.length);
        punto = array[0];
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.x) + " " + typeLang.Longitudede + "=" + formatearCoordenada(punto.y));
        $("#btnCarPrevious").attr('style', "visibility:hidden;");
        $("#btnCarNext").attr('style', "visibility:visible;");
        $("#radio_administrationLavelName").prop('checked', false);
        $("#radio_administrationLavelName").prop('disabled', false);

        posicionCheck = 0;
        totalLLamadas = array.length;
        for (var i = 0; i < array.length; i++) {
            getAdministrativeLevelsEsri(array[i], i);
        }
    }
}


function todosLosNivelesIguales() {
    if (listAdm.length == 0)
        return true;

    var nivel1 = listAdm[0].level1;
    var nivel2 = listAdm[0].level2;
    var nivel3 = listAdm[0].level3;

    for (var i = 1; i < listAdm.length; i++) {
        if (nivel1 != listAdm[i].level1 || nivel2 != listAdm[i].level2 || nivel3 != listAdm[i].level3)
            return false;
    }
    return true;
}


/**
 * This function retrieves the administrative levels using the Esri Service Geocode (commented code)
 * Using Bing API
 * @param punto
 */
function getAdministrativeLevelsEsri(punto, orden) {

    //	var processResult = function(result){
    //		var datos = result.address.address;
    //		
    //		var country = datos.CountryCode;
    //		var state = datos.Subregion;
    //		var county = datos.Region;		
    //		var city = datos.City;
    //		var neighborhood = datos.Neighborhood;				
    //		
    //		
    //		var first = countryCodes[country];
    //		if(first == undefined){
    //			first = country;
    //		}
    //		
    //		var second = county;
    //		if (state != null && state != undefined){
    //			second = second + " / " + state;
    //		}
    //		
    //		var third = city;
    //		if(neighborhood != null && neighborhood != undefined && city != neighborhood){
    //			third = third + " / " + neighborhood;
    //		}
    //		
    //		if(numPuntos == 1){
    //			firstLevel = first;
    //			secondLevel = second;
    //			thirdLevel = third;
    //		}else{
    //			firstLevel = "";
    //			secondLevel = "";
    //			thirdLevel = "";
    //		}
    //		
    //		drawLevels(first, second , third);
    //			
    //	};
    //	
    //	
    //	var onError = function(){
    //		drawLevels("No data available", "No data available" , "No data available");
    //	};
    //	
    //	 try{
    //		var locator = new esri.tasks.Locator(ESRI_ADMINISTRATIVE_LEVEL_SERVICE);
    //		
    //		locator.outSpatialReference = map.spatialReference;
    //	    var point = new esri.geometry.Point(punto.x,punto.y);
    //	   
    //	    locator.locationToAddress(point,30000, null, onError);    
    //	    locator.on("location-to-address-complete", processResult);
    //	 }catch(e){
    //		 drawLevels("No data available", "No data available" , "No data available");
    //	 }

    /*The delay of 500ms is needed because Bing service doesn't return data sometimes when many requests are done */
    window.setTimeout(function () { peticionBing(punto.y, punto.x, orden); }, 500);
}



function drawLevels(first, second, third) {
    if (first != undefined)
        $("#firstCountry").html(first);
    else
        $("#firstCountry").html("");

    if (second != undefined)
        $("#secondDistrict").html(second);
    else
        $("#secondDistrict").html("");

    if (third != undefined)
        $("#thirdMunicipality").html(third);
    else
        $("#thirdMunicipality").html("");

};


function drawLevelsPrincipal(first, second, third) {
    $("#firstData").html(first);
    $("#secondData").html(second);
    $("#thirdData").html(third);

};

//add to the array  elementosAGuardar a new element
function anadirAccion(feature, Graphic, Action) {
    var elementoaGuardar = new Array();
    elementoaGuardar["feature"] = feature;
    elementoaGuardar["graphic"] = Graphic;
    elementoaGuardar["action"] = Action;
    elementosAGuardar.push(elementoaGuardar);
    comproborBotonAnadir();
}

//set up or prevent the add button depending on if the array elementosAGuardar has elements 
function comproborBotonAnadir() {
    if (elementosAGuardar.length > 0) {
        $("#Add").prop("disabled", false);
    } else {
        $("#Add").prop("disabled", true);
    }
}



/* 1. It is retrieved the elements of templatePicker in elementosTemplate variable
 * 2. We get the id, which is going to be invoked in click event..
 */

function clickarWidget(elemento) {
    //$("#selectType option[value='Coordinates']").attr('selected', 'selected'); //Reset type Select
    //document.getElementById("selectType").selected=true;
    //$("selectType").val("Coordinates");
    $("#selectTypeCoordinates").attr('checked', 'checked');
    //$("#selectType").change();
    $("input[name=selectType]").change();




    RemoveAllGraphicsfromView();

    var elementosTemplate = $("div[widgetid^='tpick-surface-']");
    for (var i = 0; i < elementosTemplate.length; i++) {
        var id = elementosTemplate[i].id;
        var div = $("#" + id).children()[1];
        if (div.textContent.toLowerCase().indexOf(elemento.toLowerCase()) >= 0) {
            $("#" + id).click();
        }
    }
}


function controlDrawGraphics(puntosArray, LineasArray, poligonosArray, widget) {

    if (elementosAGuardar.length == 0 && (puntosArray.length + LineasArray.length + poligonosArray.length > 0)) {
        //There is no elements in memory but yes in layers

        Messi.ask(typeLang.ALERT_5, function (val) {
            if (val == "Y") {
                deleteGraphicsLayer(puntosArray, LineasArray, poligonosArray); //Delete the Graphics  			  
                RemoveAllGraphicsfromView();
                if (widget != null) clickarWidget(widget);

                return true;
            } else {
                $("#divCarrusel").hide();
                $("#txtfileName").html("No file selected");
                $("#BrowseInput").val("");

                return false;
            }
        });
    } else if (elementosAGuardar.length > 0 /*&& (puntosArray.length + LineasArray.length + poligonosArray.length) == 0*/) {

        var count = 0;
        for (var i = 0; i < elementosAGuardar.length; i++) {
            if (elementosAGuardar[i].action == ActionsEnum["Anadir"] || elementosAGuardar[i].action == ActionsEnum["Modificar"]) {
                count++;
                break;
            }
        }

        if (count > 0) {
            Messi.ask(typeLang.ALERT_5, function (val) {
                if (val == "Y") {
                    for (var i = 0; i < elementosAGuardar.length; i++) {

                        switch (elementosAGuardar[i]["action"]) {
                            case ActionsEnum["Anadir"]:
                                elementosAGuardar.splice(i, 1);
                                break;
                            case ActionsEnum["Modificar"]:
                                elementosAGuardar.splice(i, 1);
                                break;
                        }
                    }

                    RemoveAllGraphicsfromView();

                    if (widget != null) clickarWidget(widget);
                    return true;

                } else {
                    $("#divCarrusel").hide();

                    $("#txtfileName").html("No file selected.");
                    $("#BrowseInput").val("");

                    return false;
                }

            });
        } else {
            if (widget != null) clickarWidget(widget);
            return true;
        }
    } else {
        if (widget != null) clickarWidget(widget);
        return true;
    }

}

// Remove elements that was added before from memory and from the map view in all layers
function deleteGraphicsLayer(puntosArray, LineasArray, poligonosArray) {

    for (var i = 0; i < puntosArray.length; i++) {
        anadirAccion(pointServiceLayer, puntosArray[i], ActionsEnum["Eliminar"]);
    }

    for (var i = 0; i < LineasArray.length; i++) {
        anadirAccion(LineServiceLayer, LineasArray[i], ActionsEnum["Eliminar"]);
    }


    for (var i = 0; i < poligonosArray.length; i++) {
        anadirAccion(PolygonServiceLayer, poligonosArray[i], ActionsEnum["Eliminar"]);
    }
}


function RemoveAllGraphicsfromView() {
    pointServiceLayer.clear();
    LineServiceLayer.clear();
    PolygonServiceLayer.clear();
}

/**
 * This service update the administrative level to IADB tables
 * @param level1
 * @param level2
 * @param level3
 */
function updateAdministrativeLevels(level1, level2, level3) {
    if (objectType.toLowerCase() == "vo") {
        var data = "VisualOutputId=" + visual_output_id + "&Level1=" + level1 + "&Level2=" + level2 + "&Level3=" + level3;
        ajaxCall(data, UPDATE_VISUAL_OUTPUT_VERSION, updateAdministrativeLevelsResponse);
    } else {
        var data = "VisualProjectId=" + visual_output_id + "&Level1=" + level1 + "&Level2=" + level2 + "&Level3=" + level3;
        ajaxCall(data, UPDATE_VISUAL_PROJECT_VERSION, updateAdministrativeLevelsResponse);
    }
}


/**
 * Response function of updating adiminstrative levels and calls guardarMapa function passing the new versionID,  in order to save map changes.
 * @param response
 */
function updateAdministrativeLevelsResponse(response) {

    if (response != undefined && response != "" && response != null) {
        visual_output_version_id = response;

        guardarMapa(visual_output_version_id);
        getNumberGraphicsFeatures(false, visual_output_version_id);

        /* TODO */
        window.parent.$("#geoLocationAddress").html($('#geoLocationAddress'));
        window.parent.$("#level1Name").text($("#firstCountry").text().concat());
        window.parent.$("#level2Name").text($("#secondDistrict").text().concat());
        window.parent.$("#level3Name").text($("#thirdMunicipality").text().concat());
        window.parent.$("#inputValueLevel1").val($("#firstCountry").text().concat());
        window.parent.$("#inputValueLevel2").val($("#secondDistrict").text().concat());
        window.parent.$("#inputValueLevel3").val($("#thirdMunicipality").text().concat());
        var win = window.parent.$(".dinamicModal").data("kendoWindow");
        setTimeout(function () {
            win.close();
        }, 1000);
        /* */
    } else {
        new Messi(typeLang.ALERT_UPDATE_INFORMATION, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        //alert(typeLang.ALERT_UPDATE_INFORMATION);
    }
}



/** 
 * Save the features updating the value of VISUAL_OUTPUT_VERSION_ID or VISUAL_PROJECT_VERSION_ID retrieved of the service.
 */
function guardarMapa(newVersionID) {

    var alertAdmLevels = false;

    if (elementosAGuardar.length > 0) {

        for (var i = 0; i < elementosAGuardar.length; i++) {

            //Update the value VISUAL_OUTPUT_VERSION_ID
            var element = elementosAGuardar[i]["graphic"];

            if (element.attributes != null) {
                if (objectType == "vp") {
                    element.attributes.FK_GEO_VISUAL_PROJECT_ID = newVersionID;
                } else {
                    element.attributes.FK_GEO_VISUAL_OUTPUT_ID = newVersionID;
                }
            } else {
                element.attributes = pointServiceLayer.templates[0].prototype.attributes;
                if (objectType == "vp") {
                    element.attributes.FK_GEO_VISUAL_PROJECT_ID = newVersionID;
                } else {
                    element.attributes.FK_GEO_VISUAL_OUTPUT_ID = newVersionID;
                }
            }


            var attr = element.attributes;
            var updateGraphic = new esri.Graphic(element.geometry, null, attr, null);

            var graph = elementosAGuardar[i];
            var accion = elementosAGuardar[i]["action"];

            if (ActionsEnum["Anadir"] == accion) {

                elementosAGuardar[i]["feature"].applyEdits([updateGraphic], null, null, updateGeoreferenceField(updateGraphic), function (error) {
                    new Messi("Error: " + error.message, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                    //alert("Error: "+ error.message); 
                });

                var textPoints = "";

                textPoints = convert_To_LongLat(graph.graphic.geometry);

                $("#geoLocationAddress").html(jQuery.trim(textPoints));


            } else if (ActionsEnum["Modificar"] == accion) {

                elementosAGuardar[i]["feature"].applyEdits(null, [updateGraphic], null);


            } else if (ActionsEnum["Eliminar"] == accion) {
                elementosAGuardar[i]["feature"].applyEdits(null, null, [updateGraphic]);
            }


        }


        if (!alertAdmLevels) {
            elementosAGuardar = new Array();
            comproborBotonAnadir();

            firstLevel = "";
            secondLevel = "";
            thirdLevel = "";
            typeSelect = "";

            new Messi(typeLang.ALERT_SAVE, {
                title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }],
                callback: function () { closeWindow() }
            });

        }


    } else {
        new Messi(typeLang.ALERT_LOCATION_NEEDED, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        //alert(typeLang.ALERT_LOCATION_NEEDED);
    }

}


/**
 * This function update the georeference field
 * @param updateGraphic
 */
function updateGeoreferenceField(updateGraphic) {
    var point = pointServiceLayer.graphics;
    var line = LineServiceLayer.graphics;
    var polygon = PolygonServiceLayer.graphics;

    var geometryType = "";

    if (point.length > 0) {
        geometryType = point[0].geometry.type;
    } else if (line.length > 0) {
        geometryType = line[0].geometry.type;
    } else if (polygon.length > 0) {
        geometryType = polygon[0].geometry.type;
    }
    var GeolocationSpatialTypeName = '';
    var puntos = [];
    switch (geometryType) {
        case "point":
            var p = esri.geometry.xyToLngLat(updateGraphic.geometry.x, updateGraphic.geometry.y);
            puntos[0] = { "x": p[0], "y": p[1] };
            GeolocationSpatialTypeName = 'POINT';
            break;
        case "polyline":
            var path = updateGraphic.geometry.paths[0];
            for (var i = 0; i < path.length; i++) {
                var x = path[i][0];
                var y = path[i][1];

                //Converts to long lat
                var p = esri.geometry.xyToLngLat(x, y);

                puntos[i] = { "x": p[0], "y": p[1] };
                GeolocationSpatialTypeName = 'LINE';
            }
            break;
        case "polygon":

            var ring = updateGraphic.geometry.rings[0];

            for (var i = 0; i < ring.length; i++) {
                var x = ring[i][0];
                var y = ring[i][1];

                //Converts to long lat
                var p = esri.geometry.xyToLngLat(x, y);

                puntos[i] = { "x": p[0], "y": p[1] };

            }
            GeolocationSpatialTypeName = 'POLYGON';
            break;
    }


    var data = {
        objectId: visual_output_id,
        objectType: objectType,
        points: puntos
    };

    $.ajax({
        type: "POST",
        url: UPDATE_GEOLOCATION,
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            console.log("Georeference: " + response);
            //TODO
            window.parent.$("#GeolocationSpatialTypeName").text(GeolocationSpatialTypeName);
        },
        error: function (msg) {
            console.log(typeLang.ERROR_SERVER + ": " + UPDATE_GEOLOCATION + "  msg:" + msg.textStatus);
        }
    });


}



/*This function get the coordinates of a point, polyline or polygon to write them into the coordinate field
 *  when edit button is pressed*/
function getCoordinatesEditButton() {

    var point = pointServiceLayer.graphics;
    var line = LineServiceLayer.graphics;
    var polygon = PolygonServiceLayer.graphics;

    var geometryType = "";

    if (point.length > 0) {
        if (point[0].geometry != null) { geometryType = point[0].geometry.type; }
    } else if (line.length > 0) {
        if (line[0].geometry != null) { geometryType = line[0].geometry.type; }
    } else if (polygon.length > 0) {
        if (polygon[0].geometry != null) { geometryType = polygon[0].geometry.type; }
    }

    var coordinateString = "";

    switch (geometryType) {
        case "point":
            var p = esri.geometry.xyToLngLat(point[0].geometry.x, point[0].geometry.y);
            coordinateString = formatearCoordenada(p[0]) + "," + formatearCoordenada(p[1]);
            break;
        case "polyline":
            var path = line[0].geometry.paths[0];

            for (var i = 0; i < path.length; i++) {

                var x = path[i][0];
                var y = path[i][1];

                var p = esri.geometry.xyToLngLat(x, y);

                coordinateString = coordinateString + formatearCoordenada(p[0]) + "," + formatearCoordenada(p[1]);

                if (i != path.length - 1) {
                    coordinateString = coordinateString + ";";
                }
            }

            break;
        case "polygon":

            var ring = polygon[0].geometry.rings[0];

            for (var i = 0; i < ring.length; i++) {
                var x = ring[i][0];
                var y = ring[i][1];

                //Converts to long lat
                var p = esri.geometry.xyToLngLat(x, y);

                coordinateString = coordinateString + formatearCoordenada(p[0]) + "," + formatearCoordenada(p[1]);

                if (i != ring.length - 1) {
                    coordinateString = coordinateString + ";";
                }
            }
            break;
    }

    $("#coordinates").val(coordinateString);
}







function setAppLanguage(language) {
    //Set the proper language
    if (language == "en") {
        typeLang = languages.english;
        setLangTexts(typeLang);
    } else if (language == "es") {
        typeLang = languages.spanish;
        setLangTexts(typeLang);
    } else if (language == "fr") {
        typeLang = languages.french;
        setLangTexts(typeLang);
    } else if (language == "pt") {
        typeLang = languages.portuguese;
        setLangTexts(typeLang);
    }
}


function ajaxCall(data, urlService, successFunction) {
    console.log(data);
    console.log(urlService);
    $.ajax({
        type: "GET",
        url: urlService,
        data: data,
        dataType: "json",
        success: function (response) {
            successFunction(response);

        },
        error: function (msg) {
            console.log(typeLang.ERROR_SERVER + ": " + urlService + "  msg:" + msg.textStatus);
        }
    });
}





//BING Reverse Geocoder

//Makes a service request for the Bing Map REST Services and Bing 2.0 Search Service
function MakeServiceRequest(request) {
    var script = document.createElement("script");
    script.setAttribute("type", "text/javascript");
    script.setAttribute("src", request);
    document.getElementsByTagName('head')[0].appendChild(script);
}

function peticionBing(lat, long, orden) {

    //    var requestStr = "http://dev.virtualearth.net/REST/v1/Locations/"
    //    + lat + "," + long + "?"
    //    + "jsonp=MapResults&key=" + BingMapsKey;
    //
    //    //Make URL service request
    //    MakeServiceRequest(requestStr);


    $.getJSON('https://dev.virtualearth.net/REST/v1/Locations/' + lat + ',' + long + '?key=' + BingMapsKey + '&jsonp=?', function (result) {
        MapResults(result, orden);
    });

}



/* ********************************************
 * AUTOCOMPLETE ADDRESS BING
 * ********************************************/

function getGeocoderInformation(res, responseAutocomplete) {
    var result = [];
    var resultados = res.resourceSets[0];
    for (var i = 0; i < resultados.resources.length; i++) {
        var item = resultados.resources[i];
        var name = item.address.formattedAddress + ", " + item.address.countryRegion;
        result.push({
            value: name,
            feature: item.address,
            extent: item.point
        });
    }

    responseAutocomplete(result);

}


function inputAddressAutocomplete(responseAutocomplete) {
    var valorInput = $("#address").val();
    if (valorInput != "" && valorInput != undefined) {

        var url = "https://dev.virtualearth.net/REST/v1/Locations?q=";	

        url = url + valorInput + "&key=" + BingMapsKey;
        url += '&jsonp=?';
        $.getJSON(url, function (res) {
            getGeocoderInformation(res, responseAutocomplete);
            //getGeocoderInformation(res + "&callback=?", responseAutocomplete);
        });
    } else {
        new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
    }
}

//For Address searching 
function setupAutoComplete() {
    //Create the autocomplete
    $("#address").autocomplete({
        source: function (request, response) { inputAddressAutocomplete(response); },
        minLength: 4,
        open: function (event, ui) {
            $(this).autocomplete('widget').css('z-index', 2000);

            $(this).autocomplete('widget').css({
                width: $("#idDivAddress").width(),
                'background-color': 'white',
                'font-size': '11px',
                'font-weigth': 'bold'
            });

            return false;
        },
        appendTo: "#addressSuggestion",
        select: function (event, ui) {
            output = new Array();
            var output2 = new Array();

            var point = new puntoLeido();

            //var p = esri.geometry.xyToLngLat(ui.item.extent.coordinates[0],ui.item.extent.coordinates[1]);
            point.x = ui.item.extent.coordinates[1];
            point.y = ui.item.extent.coordinates[0];
            output.push(point);

            var tipoObjeto = getTypeObjeto(output);
            pintarObjeto(output, tipoObjeto, true);

            showCarrusel(output);

            $("#ulFile").removeClass("hide");
            $("#liFile1").attr('style', "display:none;");
            $("#liFile2").attr('style', "display:none;");
            $("#divCarrusel").attr('style', "visibility:visible;");

        }
    })
        .data("ui-autocomplete")._renderItem = function (ul, item) {
            return $("<li></li>")
                .data("item.autocomplete", item)
                .append("<a><b>" + item.label + "</b></a><br><br>")
                .appendTo(ul);
        };
}

//END ADDRESS AUTOCOMPLETE BING

function MapResults(response, orden) {
    totalLLamadas = totalLLamadas - 1;
    if (response.resourceSets[0].resources.length > 0) {
        //Get results
        var results = response.resourceSets[0].resources;
        var position = parseInt($("#carPos").html(), 10) - 1;
        if (results.length > 0) {
            var level1 = results[0].address.countryRegion;
            var level2 = results[0].address.adminDistrict;
            var level3 = results[0].address.locality;
            if (orden == position)
                drawLevels(level1, level2, level3);
            window.parent.$(".dinamicModal").height($('.pie.edit').height() + $('.central').height() + 30);
            var admLevel = new admLevels();
            admLevel.level1 = level1;
            admLevel.level2 = level2;
            admLevel.level3 = level3;
            listAdm[orden] = admLevel;

        } else {
            if (orden == position)
                drawLevels("", "", "");
            window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
            var admLevel = new admLevels();
            admLevel.level1 = "";
            admLevel.level2 = "";
            admLevel.level3 = "";
            listAdm[orden] = admLevel;

        }

    }
    else { //No results were returned
        if (orden == position)
            drawLevels("", "", "");
        window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
        var admLevel = new admLevels();
        admLevel.level1 = "";
        admLevel.level2 = "";
        admLevel.level3 = "";
        listAdm[orden] = admLevel;
    }
    if (totalLLamadas == 0) {
        if (todosLosNivelesIguales()) {
            $("#radio_administrationLavelName").prop('checked', true);
            posicionCheck = 1;
        }
    }
}


function deleteGraphicsLayersAttributeNull() {
    if (pointServiceLayer.graphics.length > 0) {
        for (var i = 0; i < pointServiceLayer.graphics.length; i++) {
            var graph = pointServiceLayer.graphics[i];
            if (graph.attributes == null) {
                pointServiceLayer.remove(graph);
                i = 0;
            }
        }
    }

    if (LineServiceLayer.graphics.length > 0) {
        for (var i = 0; i < LineServiceLayer.graphics.length; i++) {
            var graph = LineServiceLayer.graphics[i];
            if (graph.attributes == null) {
                LineServiceLayer.remove(graph);
                i = 0;
            }
        }
    }

    if (PolygonServiceLayer.graphics.length > 0) {
        for (var i = 0; i < PolygonServiceLayer.graphics.length; i++) {
            var graph = PolygonServiceLayer.graphics[i];
            if (graph.attributes == null) {
                PolygonServiceLayer.remove(graph);
                i = 0;
            }
        }
    }
}


function admLevels() {
    this.level1;
    this.level2;
    this.level3;
}



function block() {
    $.blockUI();
}

function unBlock() {
    $.unblockUI();
}

/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */


require(["../../../scripts/geolocation/ArcgisMap.js"], function (ArcgisMap) {
    //    var map /*,geoLocate*/;

    //    var optionsMap = {
    //        basemap: "streets",
    //        center: [-120.435, 46.159], // lon, lat
    //        zoom: 3,
    //        logo: false,
    //        nav:true        
    //    };
    //    
    //    
    //      
    //         var map = new Map("map",optionsMap);

    new ArcgisMap($(".mapWindow"), { node_id_map: "map" });
    /* geoLocate = new LocateButton({
         map: map
       }, "LocateButton");    
     
     geoLocate.startup();*/
});



(function (window, geolocalitation) {

    if (window.document && window.Worker) {
        var worker = null;

        var Shapefile = function (o, callback) {
            var
                t = this,
                o = typeof o == "string" ? { shp: o } : o;


            if (!worker) {
                var path = "js/geolocation/shapefile.js";
                var w = worker = this.worker = new Worker(path);
            } else {
                var w = worker;
            }


            w.onerror = function (event) {
                throw new Error(event.message + " (" + event.filename + ":" + event.lineno + ")");
            };

            w.onmessage = function (e) {
                t.data = e.date
                if (callback) callback(e.data)
            }

            w.postMessage(["Load", o])

            if (o.dbf) this.dbf = new DBF(o.dbf, function (data) {
                w.postMessage(["Add DBF Attributes", data])
            })
        }

        window["Shapefile"] = Shapefile
        return
    }

    var IN_WORKER = !window.document
    if (IN_WORKER) {
        importScripts('stream.js')
        onmessage = function (e) {
            switch (e.data[0]) {
                case "Load":
                    window.shapefile = new Shapefile(e.data[1])
                    break
                case "Add DBF Attributes":
                    window.shapefile.addDBFDataToGeoJSON(e.data[1])
                    window.shapefile._postMessage()
                    break
                default:
            }
        };
    }

    var SHAPE_TYPES = {
        "0": "Null Shape",
        "1": "Point", // standard shapes
        "3": "PolyLine",
        "5": "Polygon",
        "8": "MultiPoint",
        "11": "PointZ", // 3d shapes
        "13": "PolyLineZ",
        "15": "PolygonZ",
        "18": "MultiPointZ",
        "21": "PointM", // user-defined measurement shapes
        "23": "PolyLineM",
        "25": "PolygonM",
        "28": "MultiPointM",
        "31": "MultiPatch"
    }

    var Shapefile = function (o, callback) {
        /*
                var xhr = new XMLHttpRequest(),
                    that = this,
                    o = typeof o == "string" ? {shp: o} : o
        
                xhr.open("GET", o.shp, false)
                xhr.overrideMimeType("text/plain; charset=x-user-defined")
                xhr.send()
        
                if(200 != xhr.status)
                    throw "Unable to load " + o.shp + " status: " + xhr.status
        */

        //this.url = o.shp
        //this.stream = new Gordon.Stream(xhr.responseText)
        this.stream = new Gordon.Stream(o.shp);
        //throw o.shp;

        this.callback = callback;


        this.readFileHeader();
        this.readRecords();
        this.formatIntoGeoJson();

        if (o.dbf) this.dbf = IN_WORKER ?
            null :
            new DBF(o.dbf, function (data) {
                that.addDBFDataToGeoJSON(data);
                that._postMessage();
            })
        else this._postMessage()
    }


    Shapefile.prototype = {
        constructor: Shapefile,
        _postMessage: function () {
            var data = {
                header: this.header,
                records: this.records,
                dbf: this.dbf,
                geojson: this.geojson
            }
            if (IN_WORKER) postMessage(data)
            else if (this.callback) this.callback(data)
        },
        readFileHeader: function () {
            var s = this.stream,
                header = this.header = {}

            // The main file header is fixed at 100 bytes in length
            if (s < 100) throw "Invalid Header Length"

            // File code (always hex value 0x0000270a)
            header.fileCode = s.readSI32(true)

            if (header.fileCode != parseInt(0x0000270a))
                throw "Invalid File Code"

            // Unused; five uint32
            s.offset += 4 * 5

            // File length (in 16-bit words, including the header)
            header.fileLength = s.readSI32(true) * 2

            header.version = s.readSI32()

            header.shapeType = SHAPE_TYPES[s.readSI32()]

            // Minimum bounding rectangle (MBR) of all shapes contained within the shapefile; four doubles in the following order: min X, min Y, max X, max Y
            this._readBounds(header)

            // Z axis range
            header.rangeZ = {
                min: s.readDouble(),
                max: s.readDouble()
            }

            // User defined measurement range
            header.rangeM = {
                min: s.readDouble(),
                max: s.readDouble()
            }

        },
        readRecords: function () {
            var s = this.stream,
                records = this.records = [],
                record

            do {
                record = {}

                // Record number (1-based)
                record.id = s.readSI32(true)

                if (record.id == 0) break //no more records

                // Record length (in 16-bit words)
                record.length = s.readSI32(true) * 2

                record.shapeType = SHAPE_TYPES[s.readSI32()]

                // Read specific shape
                this["_read" + record.shapeType](record);

                records.push(record);

            } while (true);

        },
        _readBounds: function (object) {
            var s = this.stream

            object.bounds = {
                left: s.readDouble(),
                bottom: s.readDouble(),
                right: s.readDouble(),
                top: s.readDouble()
            }

            return object
        },
        _readParts: function (record) {
            var s = this.stream,
                nparts,
                parts = []

            nparts = record.numParts = s.readSI32()

            // since number of points always proceeds number of parts, capture it now
            record.numPoints = s.readSI32()

            // parts array indicates at which index the next part starts at
            while (nparts--) parts.push(s.readSI32())

            record.parts = parts

            return record
        },
        _readPoint: function (record) {
            var s = this.stream

            record.x = s.readDouble()
            record.y = s.readDouble()

            return record
        },
        _readPoints: function (record) {
            var s = this.stream,
                points = [],
                npoints = record.numPoints || (record.numPoints = s.readSI32())

            while (npoints--)
                points.push({
                    x: s.readDouble(),
                    y: s.readDouble()
                })

            record.points = points

            return record
        },
        _readMultiPoint: function (record) {
            var s = this.stream

            this._readBounds(record)
            this._readPoints(record)

            return record
        },
        _readPolygon: function (record) {
            var s = this.stream

            this._readBounds(record)
            this._readParts(record)
            this._readPoints(record)

            return record
        },
        _readPolyLine: function (record) {
            return this._readPolygon(record);
        },
        formatIntoGeoJson: function () {
            var bounds = this.header.bounds,
                records = this.records,
                features = [],
                feature, geometry, points, fbounds, gcoords, parts, point,
                geojson = {}

            geojson.type = "FeatureCollection"
            geojson.bbox = [
                bounds.left,
                bounds.bottom,
                bounds.right,
                bounds.top
            ]
            geojson.features = features

            for (var r = 0, record; record = records[r]; r++) {
                feature = {}, fbounds = record.bounds, points = record.points, parts = record.parts
                feature.type = "Feature"
                if (record.shapeType !== 'Point') {
                    feature.bbox = [
                        fbounds.left,
                        fbounds.bottom,
                        fbounds.right,
                        fbounds.top
                    ]
                }
                geometry = feature.geometry = {}

                switch (record.shapeType) {
                    case "Point":
                        geometry.type = "Point"
                        geometry.coordinates = [
                            record.x,
                            record.y]
                        break
                    case "MultiPoint":
                    case "PolyLine":
                        geometry.type = (record.shapeType == "PolyLine" ? "LineString" : "MultiPoint")
                        gcoords = geometry.coordinates = []

                        for (var p = 0; p < points.length; p++) {
                            var point = points[p]
                            gcoords.push([point.x, point.y])
                        }
                        break
                    case "Polygon":
                        geometry.type = "Polygon"
                        gcoords = geometry.coordinates = []

                        for (var pt = 0; pt < parts.length; pt++) {
                            var partIndex = parts[pt],
                                part = [],
                                point

                            // partIndex 0 == main poly, partIndex > 0 == holes in poly
                            for (var p = partIndex; p < (parts[pt + 1] || points.length); p++) {
                                point = points[p]
                                part.push([point.x, point.y])
                            }
                            gcoords.push(part)
                        }
                        break
                    default:
                }
                features.push(feature)
            }
            this.geojson = geojson

            if (this._addDataAfterLoad) this.addDBFDataToGeoJSON(this._addDataAfterLoad);
        },
        addDBFDataToGeoJSON: function (dbfData) {
            if (!this.geojson) return (this._addDataAfterLoad = dbfData)

            this.dbf = dbfData

            var features = this.geojson.features,
                len = features.length,
                records = dbfData.records

            while (len--) features[len].properties = records[len]
        }
    }

    window["Shapefile"] = Shapefile;
})(self);


toGeoJSON = (function () {
    'use strict';

    var removeSpace = (/\s*/g),
        trimSpace = (/^\s*|\s*$/g),
        splitSpace = (/\s+/);
    // generate a short, numeric hash of a string
    function okhash(x) {
        if (!x || !x.length) return 0;
        for (var i = 0, h = 0; i < x.length; i++) {
            h = ((h << 5) - h) + x.charCodeAt(i) | 0;
        } return h;
    }
    // all Y children of X
    function get(x, y) { return x.getElementsByTagName(y); }
    function attr(x, y) { return x.getAttribute(y); }
    function attrf(x, y) { return parseFloat(attr(x, y)); }
    // one Y child of X, if any, otherwise null
    function get1(x, y) { var n = get(x, y); return n.length ? n[0] : null; }
    // https://developer.mozilla.org/en-US/docs/Web/API/Node.normalize
    function norm(el) { if (el.normalize) { el.normalize(); } return el; }
    // cast array x into numbers
    function numarray(x) {
        for (var j = 0, o = []; j < x.length; j++) o[j] = parseFloat(x[j]);
        return o;
    }
    function clean(x) {
        var o = {};
        for (var i in x) if (x[i]) o[i] = x[i];
        return o;
    }
    // get the content of a text node, if any
    function nodeVal(x) {
        if (x) { norm(x); }
        return (x && x.firstChild && x.firstChild.nodeValue) || '';
    }
    // get one coordinate from a coordinate array, if any
    function coord1(v) { return numarray(v.replace(removeSpace, '').split(',')); }
    // get all coordinates from a coordinate array as [[],[]]
    function coord(v) {
        var coords = v.replace(trimSpace, '').split(splitSpace),
            o = [];
        for (var i = 0; i < coords.length; i++) {
            o.push(coord1(coords[i]));
        }
        return o;
    }
    function coordPair(x) {
        var ll = [attrf(x, 'lon'), attrf(x, 'lat')],
            ele = get1(x, 'ele');
        if (ele) ll.push(parseFloat(nodeVal(ele)));
        return ll;
    }

    // create a new feature collection parent object
    function fc() {
        return {
            type: 'FeatureCollection',
            features: []
        };
    }

    var serializer;
    if (typeof XMLSerializer !== 'undefined') {
        serializer = new XMLSerializer();
        // only require xmldom in a node environment
    } else if (typeof exports === 'object' && typeof process === 'object' && !process.browser) {
        serializer = new (require('xmldom').XMLSerializer)();
    }
    function xml2str(str) { return serializer.serializeToString(str); }

    var t = {
        kml: function (doc) {

            var gj = fc(),
                // styleindex keeps track of hashed styles in order to match features
                styleIndex = {},
                // atomic geospatial types supported by KML - MultiGeometry is
                // handled separately
                geotypes = ['Polygon', 'LineString', 'Point', 'Track', 'gx:Track'],
                // all root placemarks in the file
                placemarks = get(doc, 'Placemark'),
                styles = get(doc, 'Style');

            for (var k = 0; k < styles.length; k++) {
                styleIndex['#' + attr(styles[k], 'id')] = okhash(xml2str(styles[k])).toString(16);
            }
            for (var j = 0; j < placemarks.length; j++) {
                gj.features = gj.features.concat(getPlacemark(placemarks[j]));
            }
            function kmlColor(v) {
                var color, opacity;
                v = v || "";
                if (v.substr(0, 1) === "#") v = v.substr(1);
                if (v.length === 6 || v.length === 3) color = v;
                if (v.length === 8) {
                    opacity = parseInt(v.substr(0, 2), 16) / 255;
                    color = v.substr(2);
                }
                return [color, isNaN(opacity) ? undefined : opacity];
            }
            function gxCoord(v) { return numarray(v.split(' ')); }
            function gxCoords(root) {
                var elems = get(root, 'coord'), coords = [];
                if (elems.length === 0) elems = get(root, 'gx:coord');
                for (var i = 0; i < elems.length; i++) coords.push(gxCoord(nodeVal(elems[i])));
                return coords;
            }
            function getGeometry(root) {
                var geomNode, geomNodes, i, j, k, geoms = [];
                if (get1(root, 'MultiGeometry')) return getGeometry(get1(root, 'MultiGeometry'));
                if (get1(root, 'MultiTrack')) return getGeometry(get1(root, 'MultiTrack'));
                if (get1(root, 'gx:MultiTrack')) return getGeometry(get1(root, 'gx:MultiTrack'));
                for (i = 0; i < geotypes.length; i++) {
                    geomNodes = get(root, geotypes[i]);
                    if (geomNodes) {
                        for (j = 0; j < geomNodes.length; j++) {
                            geomNode = geomNodes[j];
                            if (geotypes[i] == 'Point') {
                                geoms.push({
                                    type: 'Point',
                                    coordinates: coord1(nodeVal(get1(geomNode, 'coordinates')))
                                });
                            } else if (geotypes[i] == 'LineString') {
                                geoms.push({
                                    type: 'LineString',
                                    coordinates: coord(nodeVal(get1(geomNode, 'coordinates')))
                                });
                            } else if (geotypes[i] == 'Polygon') {
                                var rings = get(geomNode, 'LinearRing'),
                                    coords = [];
                                for (k = 0; k < rings.length; k++) {
                                    coords.push(coord(nodeVal(get1(rings[k], 'coordinates'))));
                                }
                                geoms.push({
                                    type: 'Polygon',
                                    coordinates: coords
                                });
                            } else if (geotypes[i] == 'Track' ||
                                geotypes[i] == 'gx:Track') {
                                geoms.push({
                                    type: 'LineString',
                                    coordinates: gxCoords(geomNode)
                                });
                            }
                        }
                    }
                }
                return geoms;
            }
            function getPlacemark(root) {
                var geoms = getGeometry(root), i, properties = {},
                    name = nodeVal(get1(root, 'name')),
                    styleUrl = nodeVal(get1(root, 'styleUrl')),
                    description = nodeVal(get1(root, 'description')),
                    timeSpan = get1(root, 'TimeSpan'),
                    extendedData = get1(root, 'ExtendedData'),
                    lineStyle = get1(root, 'LineStyle'),
                    polyStyle = get1(root, 'PolyStyle');

                if (!geoms.length) return [];
                if (name) properties.name = name;
                if (styleUrl && styleIndex[styleUrl]) {
                    properties.styleUrl = styleUrl;
                    properties.styleHash = styleIndex[styleUrl];
                }
                if (description) properties.description = description;
                if (timeSpan) {
                    var begin = nodeVal(get1(timeSpan, 'begin'));
                    var end = nodeVal(get1(timeSpan, 'end'));
                    properties.timespan = { begin: begin, end: end };
                }
                if (lineStyle) {
                    var linestyles = kmlColor(nodeVal(get1(lineStyle, 'color'))),
                        color = linestyles[0],
                        opacity = linestyles[1],
                        width = parseFloat(nodeVal(get1(lineStyle, 'width')));
                    if (color) properties.stroke = color;
                    if (!isNaN(opacity)) properties['stroke-opacity'] = opacity;
                    if (!isNaN(width)) properties['stroke-width'] = width;
                }
                if (polyStyle) {
                    var polystyles = kmlColor(nodeVal(get1(polyStyle, 'color'))),
                        pcolor = polystyles[0],
                        popacity = polystyles[1],
                        fill = nodeVal(get1(polyStyle, 'fill')),
                        outline = nodeVal(get1(polyStyle, 'outline'));
                    if (pcolor) properties.fill = pcolor;
                    if (!isNaN(popacity)) properties['fill-opacity'] = popacity;
                    if (fill) properties['fill-opacity'] = fill === "1" ? 1 : 0;
                    if (outline) properties['stroke-opacity'] = outline === "1" ? 1 : 0;
                }
                if (extendedData) {
                    var datas = get(extendedData, 'Data'),
                        simpleDatas = get(extendedData, 'SimpleData');

                    for (i = 0; i < datas.length; i++) {
                        properties[datas[i].getAttribute('name')] = nodeVal(get1(datas[i], 'value'));
                    }
                    for (i = 0; i < simpleDatas.length; i++) {
                        properties[simpleDatas[i].getAttribute('name')] = nodeVal(simpleDatas[i]);
                    }
                }
                return [{
                    type: 'Feature',
                    geometry: (geoms.length === 1) ? geoms[0] : {
                        type: 'GeometryCollection',
                        geometries: geoms
                    },
                    properties: properties
                }];
            }
            return gj;
        },
        gpx: function (doc) {
            var i,
                tracks = get(doc, 'trk'),
                routes = get(doc, 'rte'),
                waypoints = get(doc, 'wpt'),
                // a feature collection
                gj = fc(),
                feature;
            for (i = 0; i < tracks.length; i++) {
                feature = getTrack(tracks[i]);
                if (feature) gj.features.push(feature);
            }
            for (i = 0; i < routes.length; i++) {
                feature = getRoute(routes[i]);
                if (feature) gj.features.push(feature);
            }
            for (i = 0; i < waypoints.length; i++) {
                gj.features.push(getPoint(waypoints[i]));
            }
            function getPoints(node, pointname) {
                var pts = get(node, pointname), line = [],
                    l = pts.length;
                if (l < 2) return;  // Invalid line in GeoJSON
                for (var i = 0; i < l; i++) {
                    line.push(coordPair(pts[i]));
                }
                return line;
            }
            function getTrack(node) {
                var segments = get(node, 'trkseg'), track = [], line;
                for (var i = 0; i < segments.length; i++) {
                    line = getPoints(segments[i], 'trkpt');
                    if (line) track.push(line);
                }
                if (track.length === 0) return;
                return {
                    type: 'Feature',
                    properties: getProperties(node),
                    geometry: {
                        type: track.length === 1 ? 'LineString' : 'MultiLineString',
                        coordinates: track.length === 1 ? track[0] : track
                    }
                };
            }
            function getRoute(node) {
                var line = getPoints(node, 'rtept');
                if (!line) return;
                return {
                    type: 'Feature',
                    properties: getProperties(node),
                    geometry: {
                        type: 'LineString',
                        coordinates: line
                    }
                };
            }
            function getPoint(node) {
                var prop = getProperties(node);
                prop.sym = nodeVal(get1(node, 'sym'));
                return {
                    type: 'Feature',
                    properties: prop,
                    geometry: {
                        type: 'Point',
                        coordinates: coordPair(node)
                    }
                };
            }
            function getProperties(node) {
                var meta = ['name', 'desc', 'author', 'copyright', 'link',
                    'time', 'keywords'],
                    prop = {},
                    k;
                for (k = 0; k < meta.length; k++) {
                    prop[meta[k]] = nodeVal(get1(node, meta[k]));
                }
                return clean(prop);
            }
            return gj;
        }
    };
    return t;
})();

if (typeof module !== 'undefined') module.exports = toGeoJSON;
