var MARKER_URL = "img/modulos/map/map-marker.png";

var PUNTOS_WIDGET_VO = "Geo VO Multipoint";
var LINEAS_WIDGET_VO = "Geo VO Polyline";
var POLIGONOS_WIDGET_VO = "Geo VO Polygon";

var PUNTOS_WIDGET = "Geo Project Multipoint";
var LINEAS_WIDGET = "Geo Project Polyline";
var POLIGONOS_WIDGET = "Geo Project Polygon";

var FEATURE_PROJECT_POINT_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvVisualProject/FeatureServer/0";
var FEATURE_PROJECT_POLYGON_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvVisualProject/FeatureServer/1";
var FEATURE_PROJECT_LINE_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvVisualProject/FeatureServer/2";

var FEATURE_POINT_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvOutputProject/FeatureServer/0";
var FEATURE_POLYGON_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvOutputProject/FeatureServer/1";
var FEATURE_LINE_URL = hostEsriServicesUrl + "arcgis/rest/services/" + folderEsriServiceGeolocation + "/srvOutputProject/FeatureServer/2";



//var GEOMETRY_SERVICE = hostEsriServicesUrl + "arcgis/rest/services/Utilities/Geometry/GeometryServer";
var GEOMETRY_SERVICE = "https://tasks.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer";

var urlPoint = "";
var urlLine = "";
var urlPolygon = "";



var ESRI_ADMINISTRATIVE_LEVEL_SERVICE = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

/* Send a VISUAL_OUTPUT_ID 
 * returns the Country description
 */
//var GET_DESCRIPTION_COUNTRY = hostServicesUrl + "GetDescriptionCountryResponse";
var GET_DESCRIPTION_COUNTRY = hostServicesUrl + "VisualOutputGetCountryName";


/* Send VISUAL_OUTPUT_ID
 * returns LEVEL1, LEVEL2, LEVEL3 and VISUAL_OUTPUT_VERSION_ID
 */
var GET_VISUAL_OUTPUT_VERSION = hostServicesUrl + "VisualOutputVersionGet";

/*Send VISUAL_OUTPUT_ID, LEVEL1, LEVEL2, LEVEL3
 * returns VISUAL_OUTPUT_VERSION_ID
 */
var UPDATE_VISUAL_OUTPUT_VERSION = hostServicesUrl + "VisualOutputUpdateLevel";



/* Send a VISUAL_PROJECT_ID 
 * returns the Country description
 */
var GET_DESCRIPTION_COUNTRY_VISUAL_PROJECT = hostServicesUrl + "VisualProjectGetCountryName";

/* Send VISUAL_PROJECT_ID
 * returns LEVEL1, LEVEL2, LEVEL3 and VISUAL_PROJECT_VERSION_ID
 */
var GET_VISUAL_PROJECT_VERSION = hostServicesUrl + "VisualProjectVersionGet";

/*Send VISUAL_PROJECT_ID, LEVEL1, LEVEL2, LEVEL3
* returns VISUAL_PROJECT_VERSION_ID
*/
var UPDATE_VISUAL_PROJECT_VERSION = hostServicesUrl + "VisualProjectUpdateLevel";


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
//var language="en";
//var typeLang="";
var objectType="";

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

        for (var i = 0; i < featuresArray.length ; i++) {
            var featureItem = featuresArray[i];
            var geometryType = featureItem.geometryType;

            switch (geometryType) {
                case "esriGeometryPoint":
                    getNumObjectsFeature(pointServiceLayer, whereStatement, 2, center);
                    break;
                case "esriGeometryMultipoint":
                    getNumObjectsFeature(pointServiceLayer, whereStatement, 2, center);
                    break;
                case "esriGeometryPolyline":
                    getNumObjectsFeature(LineServiceLayer, whereStatement, 0, center);
                    break;
                case "esriGeometryPolygon":
                    getNumObjectsFeature(PolygonServiceLayer, whereStatement, 1, center);
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

function isFakeMultipoint(geometry) {
    var puntos = [];
    for (var i = 0; i < geometry.points.length; i++) {
        if (inArrayPunto(geometry.points[i], puntos) == -1) {
            puntos.push(geometry.points[i]);
        }
    }
    return puntos.length == 1;
}



function getNumObjectsFeature(featureService, whereStatement, varToStore, center) {
    console.log("entramos en getNumObjectsFeature");
    var query = new esri.tasks.Query();
    query.outSpatialReference = map.spatialReference;
    query.where = whereStatement;
    query.outFields = ["*"];

    featureService.queryFeatures(query, function (featureSet) {
        if (featureSet.features.length > 0) {
            var graphicsLayer = "";
            for (var i = 0; i < featureSet.features.length; i++) {
                if (varToStore == 2) {
                    puntosArrayGraphics.push(featureSet.features[i]);
                } else if (varToStore == 0) {
                    lineasArrayGraphics.push(featureSet.features[i]);
                } else if (varToStore == 1) {
                    poligonosArrayGraphics.push(featureSet.features[i]);
                }
            }

            //Center the map at a specific location 
            if (center == true) {
                var textPoints = "";

                if (puntosArrayGraphics.length > 0) {
                    var punto = puntosArrayGraphics[0];
                    if (punto.geometry != null) {
                        if (isFakeMultipoint(punto.geometry)) {
                            var point = new esri.geometry.Point(punto.geometry.points[0][0], punto.geometry.points[0][1], map.spatialReference);
                            map.centerAndZoom(point, 10);
                        } else {
                            map.setExtent(punto._extent.expand(1.5));
                        }
                    }

                } else if (lineasArrayGraphics.length > 0) {
                    var line = new esri.geometry.Polyline(map.spatialReference);
                    if (lineasArrayGraphics[0].geometry != null) {
                        line.addPath(lineasArrayGraphics[0].geometry.paths);
                        line._extent = lineasArrayGraphics[0].geometry._extent;
                        map.setExtent(line.getExtent().expand(1.5));


                    }

                } else if (poligonosArrayGraphics.length > 0) {
                    var poligon = new esri.geometry.Polygon(map.spatialReference);
                    if (poligonosArrayGraphics[0].geometry != null) {
                        poligon.addRing(poligonosArrayGraphics[0].geometry.rings); //Adds the ring to the polygon
                        poligon._extent = poligonosArrayGraphics[0].geometry._extent;
                        map.setExtent(poligon.getExtent().expand(1.5));
                    }
                }
            }

            unBlock();
            if (edit == "1" && $("#Edit").is(":visible")) {
                if (objectType == "vp") {
                    $("#lineaButtonContainer").hide();
                    $("#poligonoButton").hide();
                }
                $("#Edit").click();
                $("#cancelEdit").hide();
                $("#Add").hide();
            }
        }
    }, function (error) {
        console.log("error=" + error);
        unBlock();
        if (edit == "1" && $("#Edit").is(":visible")) {
            if (objectType == "vp") {
                $("#lineaButtonContainer").hide();
                $("#poligonoButton").hide();
            }
            $("#Edit").click();
            $("#cancelEdit").hide();
            $("#Add").hide();
        }
    });

}



// puntosLeidos = puntoLeido[]
// tipoObjeto = 3 -> punto, 1 -> linea, 2-> poligono
function pintarObjeto(puntosLeidos, tipoObjeto, comprobar, borrarGraphics, anadirArrays) {
    var result = true;
    if (comprobar)
        result = controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, null);
    if (result) {
        //Remove all graphics si Accept is selected
        if (borrarGraphics) {
            if (anadirArrays) {
                deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
            }
            RemoveAllGraphicsfromView();
        }

        switch (tipoObjeto) {
            case 3:
                if (puntosLeidos.length > 0) {
                    map.on("load", putMarkerLine(puntosLeidos));
                }
                break;
            case 1:
                if (puntosLeidos.length > 0) {
                    map.on("load", putMarkerPoint(puntosLeidos));
                }
                break;
            case 2:
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
    var mp = new esri.geometry.Multipoint(map.spatialReference);
    mp.addPoint(point);

    addGraphicToMap(mp, symbolPuntos, "esriGeometryMultipoint");
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
        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(geometryGraphic.y) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(geometryGraphic.x);
    } else if (geometry == "multipoint") {
        for (i = 0; i < geometryGraphic.points.length; i++) {
            textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(geometryGraphic.points[i][1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(geometryGraphic.points[i][0]);
            if (i < geometryGraphic.points.length)
                textPoints = textPoints + "<br><br>";
        }
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

                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[0]);


                    if (j < ruta.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }

            } else {
                p = esri.geometry.xyToLngLat(path[j][0], path[j][1]);
                textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[0]);

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

                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[0]);

                    if (j < ruta.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }
            } else {
                p = esri.geometry.xyToLngLat(ring[j][0], ring[j][1]);

                textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(p[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(p[0]);

                if (j < ring.length - 1) {
                    textPoints = textPoints + "<br><br>";
                }
            }


        }
    }

    return textPoints;
}


function puntosInArray(punto, puntos) {
    for (var i = 0; i < puntos.length; i++) {
        if (puntos[i][0] == punto[0] && puntos[i][1] == punto[1])
            return i;
    }
    return -1;
}


function formatCoordinates(geometryGraphic) {
    var textPoints = "";
    if (geometryGraphic != null && geometryGraphic != undefined) {
        var geometry = geometryGraphic.type;
        if (geometry == "point") {

            textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(geometryGraphic.y) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(geometryGraphic.x);

        } else if (geometry == "multipoint") {
            var puntosYaAnadidos = [];
            for (i = 0; i < geometryGraphic.points.length; i++) {
                if (puntosInArray(geometryGraphic.points[i], puntosYaAnadidos) == -1) {
                    puntosYaAnadidos.push(geometryGraphic.points[i]);
                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(geometryGraphic.points[i][1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(geometryGraphic.points[i][0]);
                    if (i < geometryGraphic.points.length)
                        textPoints = textPoints + "<br><br>";
                }
            }

        } else if (geometry == "polyline") {
            var path = geometryGraphic.paths[0];

            for (var j = 0; j < path.length; j++) {
                var p = "";
                if (path[0][j] instanceof Array) {
                    var ruta = path[0];
                    var puntoPath = "";
                    for (k = 0; k < ruta.length; k++) {
                        puntoPath = ruta[k];
                        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(puntoPath[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(puntoPath[0]);
                        if (j < ruta.length - 1) {
                            textPoints = textPoints + "<br><br>";
                        }
                    }

                } else {
                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(path[j][1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(path[j][0]);
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

                        textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(puntoPath[1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(puntoPath[0]);

                        if (j < ruta.length - 1) {
                            textPoints = textPoints + "<br><br>";
                        }
                    }
                } else {
                    textPoints = textPoints + typeLang.Latitude + "=" + formatearCoordenada(ring[j][1]) + "<br> " + typeLang.Longitude + "=" + formatearCoordenada(ring[j][0]);

                    if (j < ring.length - 1) {
                        textPoints = textPoints + "<br><br>";
                    }
                }


            }
        }
    }

    return textPoints;
}



