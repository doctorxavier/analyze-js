var nivelAsignado = false;
var elementosGuardados = 0;
var output = new Array();
var algoQueGuardar = false;
var posicionCheck;
var totalLLamadas = 0;
var geometryService = null;
var Guardar = true;
var isEmpty = false;
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
        scriptPathArcgis + "ArcgisMap.js",

        "dijit/layout/BorderContainer", "dijit/layout/ContentPane",
        "dijit/form/Button", "dojo/domReady!"
], function (
        maps, Geocoder, Draw, Edit, Graphic, Point, Multipoint, webMercatorUtils, esriConfig,
        FeatureLayer, Query, QueryTask,
        SimpleMarkerSymbol, SimpleLineSymbol, SimpleFillSymbol,
        TemplatePicker,
        arrayUtils, event, lang, Color, parser, registry, ArcgisMap,
        BorderContainer, ContentPane, Button        
      ) {
     
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
    geometryService = new esri.tasks.GeometryService(GEOMETRY_SERVICE);
    esriConfig.defaults.geometryService = geometryService;
     
    
    elementosAGuardar = new Array();
    ActionsEnum = ["Anadir", "Modificar", "Eliminar"];
    ActionsEnum["Anadir"] = 1;
    ActionsEnum["Modificar"] = 2;
    ActionsEnum["Eliminar"] = 3;


    map = new maps("map", {
        basemap: "streets",
        center: [-83.244, 42.581],
        zoom: 3
    });

    initMap(ArcgisMap);
    dojo.connect(map, 'onLoad');
    
    /*******************************************
     * 1. Busqueda de Calle, ciudad, pais...
     *******************************************/
    geocoder = new Geocoder({
        map: map
    }, "searchDireccion");

    geocoder.startup();
    geocoder.hide(); //Hiding the widget. It is called through other input.

    $("#address").keydown(function () { setupAutoComplete(); });

    /**
    * FIND BUTTON ADDRESS
    */
    $("#FindAddress").click(function () {
        inputAddressOnChange();
        typeSelect = $("input[name=selectType]").val().concat();
        
    });

    //Saves changes in the bbdd
    $("#Add").click(add_click);
    
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
                nivelAsignado = true;
                firstLevel = $("#firstAddress").text();
                secondLevel = $("#secondAddress").text();
                thirdLevel = $("#thirdAddress").text();
            });
        } else {
            new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        }


    }

    function geocodeResults(places, zoomValue) {
        places = places.results;
        if (places.length > 0) {
            zoomToPlaces(places, zoomValue);
        } else {
            new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        }
    }


    function zoomToPlaces(places, zoomValue) {
        var point = new Point(places[0].feature.geometry);
        map.centerAndZoom(point, zoomValue);

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

    /***********************************************
     *         COORDINATE
     ***********************************************/

    $("#Find").on("click", function () { 
        output = new Array();
        if ($("#coordinates").val() != "" && $("#coordinates").val() != undefined) {
            deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
            soloEliminarElementosAGuardar();
            RemoveAllGraphicsfromView();

            isEmpty = true;
            algoQueGuardar = false;

            $("#AddressIDDiv").hide();
            $("#address").val("");

            $("#divCarrusel").hide();
            $("#BrowseInput").val("");
            $("#txtfileName").html("No file selected");
            $("#geoLocationAddress").html("");


            $("#principal").hide();
            var coordString = readCoordinates($("#coordinates").val());

            typeSelect = $("input[name=selectType]").val().concat();
        } else {
            $("#divCarrusel").attr('style', "visibility:hidden;");
            $("#btnCarPrevious").attr('style', "visibility:hidden;");
            $("#btnCarNext").attr('style', "visibility:hidden;");

            new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
        }
    });

    //Reading input coordinates
    function readCoordinates(entrada) {

        var valorInput = entrada;
        var coordenadas = valorInput.split(";");

        var coordString = "";

        for (var i = 0 ; i < coordenadas.length; i++) {

            var coordenada = coordenadas[i];
            var coordenadaArray = coordenada.split(",");
            if (coordenadaArray != undefined) {

                if (coordenadaArray[0] != undefined && coordenadaArray[1] != undefined) {
                    var coordX = coordenadaArray[1];
                    var coordY = coordenadaArray[0];

                    if (coordX != undefined && coordY != undefined && !isNaN(coordX) && !isNaN(coordY) && coordX != "" && coordY != "") {
                        var point = new puntoLeido();
                        point.x = coordX;
                        point.y = coordY;

                        output.push(point);

                        coordString += typeLang.Latitude + "=" + formatearCoordenada(coordY) + "; " + typeLang.Longitude + "=" + formatearCoordenada(coordX) + "<br/>"; 

                    } else {
                        new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                        output = new Array();
                        break;
                    }
                } else {
                    new Messi(typeLang.ALERT_COORDINATES_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
                    output = new Array();
                    break;
                }
            } else {
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
            RemoveAllGraphicsfromView();
            $("#divCarrusel").attr('style', "visibility:hidden;");
            $("#btnCarPrevious").attr('style', "visibility:hidden;");
            $("#btnCarNext").attr('style', "visibility:hidden;");

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
        nivelAsignado = false;
        firstLevel = "";
        secondLevel = "";
        thirdLevel = "";
        var tipoObjeto = getTypeObjeto(arrayCoordinates);
        widget = PUNTOS_WIDGET;
        algoQueGuardar = true;
        if (objectType == "vp" && arrayCoordinates.length > 1) {
            new Messi(typeLang.ALERT_6, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
            $("#ulFile").addClass("hide");
            $("#liFile1").attr('style', "display:block;");
            $("#liFile2").attr('style', "display:block;");
            $("#divCarrusel").attr('style', "visibility:hidden;");
        }else{
            pintarObjeto(arrayCoordinates, tipoObjeto, false, true, true);
            showCarrusel(output);
            
        }
        

    }

    /*Center a map over a specific country*/
    function centrarMapa(country) {
        geocoder.value = country;
        var def = geocoder.find();
        def.then(function (res) {
            geocodeResults(res, 5);
        });
    }


    /*
     * Functionality for drawing points, lines and polygons - Feature Services
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
    function initMap(ArcgisMap) {
        console.log("INFO: initmap ejecutado");
        new ArcgisMap($(".mapWindow"), { node_id_map: "map" });
        block();
        if (edit != "1" ) {
            $("#Edit").hide();
        }

        if (objectType.toLowerCase() == "vo") {
            getOutputVersionID(GET_VISUAL_OUTPUT_VERSION);
        } else {
            getOutputVersionID(GET_VISUAL_PROJECT_VERSION);
        }
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
            var data = "VisualOutputId=" + visual_output_id + "&IsValidate=" + false;
            ajaxCall(data, urlService, outputVersionIDResponse);
        } else {
            var data = "VisualProjectId=" + visual_output_id + "&IsValidate=" + false;
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
                campoVisual = "FK_GEO_VISUAL_OUTPUT_ID"; //This field corresponding to visual version id                 
                  
            } else {
                 campoVisual = "FK_GEO_VISUAL_PROJECT_ID";   //This field corresponding to visual project id
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
                                        var data = "VisualOutputId=" + visual_output_id + "&IsValidate=" + true;
                                        ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, outputVersionIDTrueResponse);
                                    } else {
                                        var data = "VisualProjectId=" + visual_output_id + "&IsValidate=" + true;
                                        ajaxCall(data, GET_VISUAL_PROJECT_VERSION, outputVersionIDTrueResponse);
                                    }
                                }
                            }, function (error) {
                                console.log("error=" + error);
                            });
                        }
                    }, function (error) {
                        console.log("error=" + error);
                    });
                }
            },function (error){
        		console.log("error="+error);
            });

        } else {
            console.log("There is no response in function outputVersionIDResponse");

            if (objectType.toLowerCase() == "vo") {
                var data = "VisualOutputId=" + visual_output_id + "&IsValidate=" + true;
                ajaxCall(data, GET_VISUAL_OUTPUT_VERSION, outputVersionIDTrueResponse);
            } else {
                var data = "VisualProjectId=" + visual_output_id + "&IsValidate=" + true;
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
                
                
            } else {
                campoVisual = "FK_GEO_VISUAL_PROJECT_ID";     
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
                            },function (error){
                                console.log("error=" + error);
                                unBlock();
                            });

                        }
                    },function (error){
                        console.log("error=" + error);
                        unBlock();
                    });
                }
            },function (error){
                console.log("error=" + error);
                unBlock();
              }
            );

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
        console.log("entramos en getCountryVisualOutputID");
        if (visual_output_id > 0 ){
            if (objectType.toLowerCase() == "vo") {
                var data = "VisualOutputId=" + visual_output_id;
                ajaxCall(data, GET_DESCRIPTION_COUNTRY, countryVisualOutputResponse);
            } else {
                var data = "VisualProjectId=" + visual_output_id;
                ajaxCall(data, GET_DESCRIPTION_COUNTRY_VISUAL_PROJECT, countryVisualOutputResponse);
            }
        } else {
            var response = {};
            response.Geolocation = country;
            countryVisualOutputResponse(response)
        }
    }


    /**
     * Center de map
     */ 
    function countryVisualOutputResponse(response) {
        if (response.Geolocation != "" && response.Geolocation != null ) {
            country = response.Geolocation;
            centrarMapa(country);
        } else {
            if (country!=""){
                centrarMapa(country);                
            } else {
                console.log("INFO: el mapa no se centro con el pais");
                var point = new esri.geometry.Point({ 
                    latitude: -12.81,
                    longitude: -57.66
                });
                map.centerAndZoom(point, 4);
            }
        }

        var campoVisual = "";

        if (objectType.toLowerCase() == "vo") {
           campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";
        } else {
            campoVisual = "FK_GEO_VISUAL_PROJECT_ID";
        }

        pointServiceLayer.setDefinitionExpression(campoVisual + " = -1");
        LineServiceLayer.setDefinitionExpression(campoVisual + " = -1");
        PolygonServiceLayer.setDefinitionExpression(campoVisual + " = -1");

        map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
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
                    } else {
                        campoVisual = "FK_GEO_VISUAL_PROJECT_ID";
                    }
                    pointServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                    LineServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                    PolygonServiceLayer.setDefinitionExpression(campoVisual + " = -1");
                    map.addLayers([pointServiceLayer, LineServiceLayer, PolygonServiceLayer]);
                    unBlock();
                    if (edit == "1" && $("#Edit").is(":visible")) {
                        $("#Edit").click();
                        $("#cancelEdit").hide();
                        $("#Add").hide();
                    }
                    

                } else if (urlService == GET_VISUAL_OUTPUT_VERSION || urlService == GET_VISUAL_PROJECT_VERSION) {
                    getCountryVisualOutputID();
                }
            }
        });
    }
    
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
        	case "multipoint":
        		var points = object.geometry.points;
        		for (var i = 0; i < points.length ; i++) {
        			var point = new puntoLeido();
        			var p = esri.geometry.xyToLngLat(points[i][0], points[i][1]);        			        			
        			point.x = p[0];
        			point.y = p[1];
        			output.push(point);        			        			        			
        		}    			
    		break;
            case "point":
                var point = new puntoLeido();
                var p = esri.geometry.xyToLngLat(object.geometry.x, object.geometry.y);
                point.x = p[0];
                point.y = p[1];

                output.push(point);
                break;
            case "polyline":
            	for (var j=0; j<object.geometry.paths.length; j++){
                var path = object.geometry.paths[j];
	                for (var i = 0; i < path.length ; i++) {
	                    var point = new puntoLeido();
	                    var p = esri.geometry.xyToLngLat(path[i][0], path[i][1]);
	
	                    point.x = p[0];
	                    point.y = p[1];
	
	                    output.push(point);
	                }
            	}
                break;
            case "polygon":
            	for (var j=0; j<object.geometry.rings.length; j++){
	                var ring = object.geometry.rings[j];
	
	                for (var i = 0; i < ring.length ; i++) {
	                    var point = new puntoLeido();
	                    var p = esri.geometry.xyToLngLat(ring[i][0], ring[i][1]);
	
	                    point.x = p[0];
	                    point.y = p[1];
	
	                    output.push(point);
	                }
            	}
                break;
        }

        showCarrusel(output);

        $("#ulFile").removeClass("hide");
        $("#liFile1").attr('style', "display:none;");
        $("#liFile2").attr('style', "display:none;");
        $("#divCarrusel").attr('style', "visibility:visible;");

    }

    function anadirGeometriaElementosAGuardar(indice,geometry){
    	var geometrias = [];
    	geometriaEnArray = elementosAGuardar[indice].graphic.geometry;
    	geometrias.push(geometriaEnArray);
    	if (geometry.type != geometriaEnArray.type){
    		var points = { 
    	        "points": [[geometry.x,geometry.y]], 
    	        "spatialReference": ({ "wkid": geometry.spatialReference.wkid })
    	    };
    		var mp = new esri.geometry.Multipoint(points);
    		geometrias.push(mp);
    	}else{
    		geometrias.push(geometry);	
    	}
    	
    	geometryService.union(geometrias,  actualizarNivelesAdmin, null);
    }

    function actualizarNivelesAdmin(geometry){
    	for (var i =0; i< elementosAGuardar.length; i++){
    		if (elementosAGuardar[i]["action"] == ActionsEnum["Anadir"]){
    			elementosAGuardar[i]["graphic"].geometry = geometry;
    			var graphic = elementosAGuardar[i]["graphic"]; 
    			getAdministrativeLevelsObject(graphic);
    			    			    			
    			var newAttributes = lang.mixin({}, selectedTemplate.template.prototype.attributes);
	            var newGraphic2 = new Graphic(geometry, null, newAttributes);
	            
    			selectedTemplate.featureLayer.remove(graphic);    			
    			selectedTemplate.featureLayer.add(newGraphic2);
    			
    			getCoordinatesEditButton();
    			nivelAsignado = false;
    			firstLevel = "";
    			secondLevel = "";
    			thirdLevel = "";		
    		}		
    	}		
    }
    

    //This function is executed when Feature layers are loaded
    function initEditing(evt)
    {
        window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
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
                case "esriGeometryMultipoint":
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

        drawToolbar.on("draw-end", function (evt)
        {
            algoQueGuardar = true;
            window.parent.$(".dinamicModal").height($('.pie.edit').height() + $('.central').height() + 30);
            indice = yaExisteAlgunoParaAnadir();
            var geometria;
            if (evt.geometry.type == "point")
            {
                geometria = trasformarPuntoaMultipunto(evt.geometry);
            } else
            {
                geometria = evt.geometry;
            }

            if (indice < 0)
            {
                var newAttributes = lang.mixin({}, selectedTemplate.template.prototype.attributes);
                var newGraphic = new Graphic(geometria, null, newAttributes);

                anadirAccion(selectedTemplate.featureLayer, newGraphic, ActionsEnum["Anadir"]);

                getAdministrativeLevelsObject(newGraphic);
                selectedTemplate.featureLayer.add(newGraphic);
                getCoordinatesEditButton();
                nivelAsignado = false;
                firstLevel = "";
                secondLevel = "";
                thirdLevel = "";
            }
            else
            {
                if (objectType == "vo")
                {
                    anadirGeometriaElementosAGuardar(indice, geometria);
                }
            }
        });
    }

    $("#puntoButton").click(function () { clickPuntoEditButton(); });
    $("#lineaButton").click(function () { clickLineaEditButton(); });
    $("#poligonoButton").click(function () { clickPoligonoEditButton(); });

    function clickPuntoEditButton() {
        if (objectType == "vp") {
            indice = yaExisteAlgunoParaAnadir();
            if(indice < 0){
                controlDrawGraphics(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics, PUNTOS_WIDGET);
            }
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

    function clickarCarPrevious() {
        var position = parseInt($("#carPos").html(), 10) - 1;
        var punto = output[position - 1];
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.y) + " " + typeLang.Longitude + "=" + formatearCoordenada(punto.x));
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
                    }
                }
            }

        } else {
            if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                var adm = listAdm[position - 1];
                drawLevels(adm.level1, adm.level2, adm.level3);
            }
        }
    };

    function clickarCarNext() {

        var position = parseInt($("#carPos").html(), 10) + 1;
        var punto = output[position - 1];
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.y) + " "+typeLang.Longitude +"=" + formatearCoordenada(punto.x));
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
                    }
                }
            }

        } else {
            if (listAdm[position - 1] != undefined && listAdm[position - 1] != null) {
                var adm = listAdm[position - 1];
                drawLevels(adm.level1, adm.level2, adm.level3);
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
                
                deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
                soloEliminarElementosAGuardar();
                RemoveAllGraphicsfromView();

                isEmpty = true;
                algoQueGuardar = false;


                $("#coordinates").val("");

                $("#AddressIDDiv").hide();
                $("#address").val("");

                $("#divCarrusel").hide();
                $("#BrowseInput").val("");
                $("#txtfileName").html("No file selected");
                $("#geoLocationAddress").html("");
                
                $("#principal").hide();
                $("#secondPrincipal").hide();
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
                campoVisual = "FK_GEO_VISUAL_OUTPUT_ID";
            } else {
                campoVisual = "FK_GEO_VISUAL_PROJECT_ID";              
            }


            var query = new Query();
            query.where = campoVisual + " =" + visual_output_version_id;
            query.returnGeometry = true;
            query.outFields = ["*"];
            var queryTask = new QueryTask(urlPoint);

            queryTask.execute(query, function (result) {
                if (result.features.length > 0) {
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
                                    var graphic = new esri.Graphic(result.features[0].geometry, null, result.features[0].attributes);

                                    PolygonServiceLayer.applyEdits(null, null, [graphic]);
                                }	    	
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
        nivelAsignado = true;
        firstLevel = $("#firstCountry").text().concat();
        secondLevel = $("#secondDistrict").text().concat();
        thirdLevel = $("#thirdMunicipality").text().concat();
    });

    function initCancelButton() {
        $("#cancelEdit").on("click", function () {
            hideEditInfo();
            EditionMode = false;
            window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
        });
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

        $("#selectTypeCoordinates").attr('checked', 'checked');
        
        $("#txtfileName").html(typeLang.NoFileSelected);

        if (objectType.toLowerCase() == "vo") {
            getOutputVersionID(GET_VISUAL_OUTPUT_VERSION);
        } else {
            getOutputVersionID(GET_VISUAL_PROJECT_VERSION);
        }

        $("#principal").show();

        elementosAGuardar = new Array();
        comproborBotonAnadir();
        nivelAsignado = false;
        firstLevel = "";
        secondLevel = "";
        thirdLevel = "";
        typeSelect = "";
    }
});

/**
 *  PUBLIC FUNCTIONS
 */

/*
 * Handle file reading 
 */
function manejadorDeArchivos(files) {
            deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
            soloEliminarElementosAGuardar();
            RemoveAllGraphicsfromView();

    	    isEmpty = true;
            algoQueGuardar = false;

            $("#coordinates").val("");

            $("#AddressIDDiv").hide();
            $("#address").val("");

            $("#divCarrusel").hide();            
            $("#geoLocationAddress").html("");

            output = new Array();            

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
//devuelve cadena vacia si no todos los tipos del geojson son del mismo tipo , sino devuelve el tipo
function tipoGeometriaGeoJson(geoJson){ 
	tipo = geoJson.features[0].geometry.type ;
	for (var i=1; i<geoJson.features.length; i++){		
		if (geoJson.features[i].geometry.type != tipo){
			return "";			
		}
	}
	return tipo;
}

function tipoGeometriaLinea(linea){
	if (linea.length==1){
		return "Point";
	}else{
		if (linea[0].x == linea[linea.length-1].x && linea[0].y == linea[linea.length-1].y){
			return "Polygon";
		}else{
			return "LineString";
		}   
	}
}

//devuelve cadena vacia si no todos los tipos del texto son del mismo tipo , sino devuelve el tipo, -1 si hay error en el texto
function tipoGeometriaTexto(lineas){	
	for (var i = 0; i < lineas.length; i++) {
		linea = lineas[i];
		puntos = linea.split(';');
		var lineaPuntos = [];
	    for (var j = 0; j < puntos.length; j++) {
	    	var cadenaPunto = puntos[j];	 
	    	var cadenaPuntoSplit = cadenaPunto.split(",");
	    	if (cadenaPuntoSplit.length == 2) {
	    		var coorX = cadenaPuntoSplit[0];
	    		var coorY = cadenaPuntoSplit[1];
	    		var punto = new puntoLeido();
	    		if (coorX == "" || coorY == "" || isNaN(coorX) || isNaN(coorY)) {
	    			return "-1";
	    		}else{
	    			var punto = new puntoLeido();		    	
					punto.x = coorX;
					punto.y = coorY;
					lineaPuntos.push(punto);		
	    		}
	    	}else{
	    		return "-1";
	    	}
	    }
	    if (lineaPuntos.length>0){
	    	if (i==0){
	    		tipo = tipoGeometriaLinea(lineaPuntos);
	    	}else{
	    		if (tipoGeometriaLinea(lineaPuntos) != tipo){
	    			return "";
	    		}
	    	}
	    }else{
	    	return "-1";
	    }
	}
	return tipo;
}

function soloEliminarElementosAGuardar(){
	for (var i = 0; i < elementosAGuardar.length; i++) {
        switch (elementosAGuardar[i]["action"]) {
            case ActionsEnum["Anadir"]:
                elementosAGuardar.splice(i, 1);
            	i--;
                break;
            case ActionsEnum["Modificar"]:
                elementosAGuardar.splice(i, 1);
            	i--;
                break;
        }
    }
}

function tratarTexto(texto){
	resultado = texto;
	lineas = resultado.split("\r\n"); 
	tipo = tipoGeometriaTexto(lineas);
	if (tipo=="" ){
		new Messi(typeLang.ALERT_3 , { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
		resetCarrusel();
		output = new Array();
		return;
	}else if (tipo=="-1"){
		new Messi(typeLang.ALERT_FILE_INCORRECT, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
		resetCarrusel();
		output = new Array();
		return;
	}else{
		deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
		soloEliminarElementosAGuardar();            
        RemoveAllGraphicsfromView();
		if (tipo == "LineString"){
			var line = new esri.geometry.Polyline(map.spatialReference);
			for (var i = 0; i < lineas.length; i++) {
				linea = lineas[i];
				puntos = linea.split(';');
				var path = new Array();
				for (var j = 0; j < puntos.length; j++) {
					var cadenaPunto = puntos[j];		 
					var cadenaPuntoSplit = cadenaPunto.split(",");					
					var coorX = cadenaPuntoSplit[0];
					var coorY = cadenaPuntoSplit[1];
					var punto = punto_esri(coorX,coorY);
					var puntot = new puntoLeido();					
					puntot.x = coorX;
					puntot.y = coorY;
					output.push(puntot);
      			  	path.push(punto);		    			    		  					
				}
				line.addPath(path);				
			}
			addGraphicToMap(line, symbolLineas, "esriGeometryPolyline");
			algoQueGuardar = true;
			map.setExtent(line.getExtent());
		}else if (tipo=="Polygon"){
			var poligon = new esri.geometry.Polygon(map.spatialReference);			
			for (var i = 0; i < lineas.length; i++) {
				linea = lineas[i];
				puntos = linea.split(';');
				var ring = new Array();
				for (var j = 0; j < puntos.length; j++) {
					var cadenaPunto = puntos[j];
					var cadenaPuntoSplit = cadenaPunto.split(",");					
					var coorX = cadenaPuntoSplit[0];
					var coorY = cadenaPuntoSplit[1];
					var punto = punto_esri(coorX,coorY);
					if (j != puntos.length-1){
						var puntot = new puntoLeido();
						puntot.x = coorX;
						puntot.y = coorY;
						output.push(puntot);
					}
					ring.push(punto);      
				}
				poligon.addRing(ring);				
			}
			addGraphicToMap(poligon, symbolPoligonos, "esriGeometryPolygon");
			algoQueGuardar = true;
			map.setExtent(poligon.getExtent());
			
		}else if (tipo == "Point"){
			var multipoint = new esri.geometry.Multipoint(map.spatialReference);
			for (var i = 0; i < lineas.length; i++) {
				linea = lineas[i];
				var cadenaPuntoSplit = linea.split(",");
				var coorX = cadenaPuntoSplit[0];
				var coorY = cadenaPuntoSplit[1];
				var punto = punto_esri(coorX,coorY);
				var puntot = new puntoLeido();
				puntot.x = coorX;
				puntot.y = coorY;
				output.push(puntot);
				multipoint.addPoint(punto);
				
			}
			addGraphicToMap(multipoint, symbolPuntos, "esriGeometryMultipoint");
			algoQueGuardar = true;
			map.setExtent(multipoint.getExtent());
		}
		if (output.length>0){
			$("#divCarrusel").attr('style', "visibility:visible;");
			showCarrusel(output);
		}
	}
}

function trasformarPuntoaMultipunto(punto) {
    var multipoint = new esri.geometry.Multipoint(map.spatialReference);
    multipoint.addPoint(punto);
    
    return multipoint;
}

function tratarGeoJson(geoJson){
	if (geoJson.features.length>0){
		tipoGeometria = tipoGeometriaGeoJson(geoJson);
		if ( tipoGeometria == "" ){
			new Messi(typeLang.ALERT_3 , { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
		}else{
			deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
			soloEliminarElementosAGuardar();            
            RemoveAllGraphicsfromView();
            
			if (tipoGeometria == "LineString"){				
				var line = new esri.geometry.Polyline(map.spatialReference);
				for (var i=0; i<geoJson.features.length; i++){
				    var feature = geoJson.features[i];
				    var path = new Array();
					for (var j=0; j<feature.geometry.coordinates.length; j++){
						var punto = punto_esri(feature.geometry.coordinates[j][0],feature.geometry.coordinates[j][1]);
						var puntot = new puntoLeido();
						puntot.x =  feature.geometry.coordinates[j][0];
						puntot.y = feature.geometry.coordinates[j][1];
	      			  	output.push(puntot);
	      			  	path.push(punto);
					}
					line.addPath(path);
					map.setExtent(line.getExtent());
				}
				algoQueGuardar = true;
				addGraphicToMap(line, symbolLineas, "esriGeometryPolyline");				
			}else if (tipoGeometria == "Polygon"){				
				var poligon = new esri.geometry.Polygon(map.spatialReference);																
				for (var i=0; i<geoJson.features.length; i++){
					var feature =  geoJson.features[i];					
					var ring = new Array();
                    if (feature.geometry.coordinates.length > 0){
						var coordenada = feature.geometry.coordinates[0];
						if (Array.isArray(coordenada)){
							for (var k=0; k<coordenada.length; k++){	
								var punto = punto_esri(coordenada[k][0],coordenada[k][1]);
								var puntot = new puntoLeido();
								puntot.x = coordenada[k][0];
								puntot.y = coordenada[k][1];
			      			  	output.push(puntot);
			      			  	ring.push(punto);			      			  	
			      			  	if (k == coordenada.length-1){
			      			  		var punto = punto_esri(coordenada[0][0],coordenada[0][1]);
			      			  		ring.push(punto);								    
			      			  	}                      
							}																																
							poligon.addRing(ring);
						}else{							
							var punto = punto_esri(feature.geometry.coordinates[0],feature.geometry.coordinates[1]);
							var puntot = new puntoLeido();
							puntot.x = feature.geometry.coordinates[0];
							puntot.y = feature.geometry.coordinates[1];
		      			  	output.push(puntot);
							ring.push(punto);
						}
					}  											
				}
				algoQueGuardar = true;
				addGraphicToMap(poligon, symbolPoligonos, "esriGeometryPolygon");
				map.setExtent(poligon.getExtent());
			}else if (tipoGeometria == "Point"){
				var multipoint = new esri.geometry.Multipoint(map.spatialReference);
				for (var i=0; i<geoJson.features.length; i++){
					var feature =  geoJson.features[i];										
                    if (feature.geometry.coordinates.length > 0){
						var coordenada = feature.geometry.coordinates[0];
						if (Array.isArray(coordenada)){
							for (var k=0; k<coordenada.length; k++){	
								var punto = punto_esri(coordenada[k][0],coordenada[k][1]);
								var puntot = new puntoLeido();
								puntot.x = coordenada[k][0];
								puntot.y = coordenada[k][1];
			      			  	output.push(puntot);
			      			  	multipoint.addPoint(punto);
			      			  	
							}		
						}else{
							var punto = punto_esri(feature.geometry.coordinates[0],feature.geometry.coordinates[1]);
							var puntot = new puntoLeido();
							puntot.x = feature.geometry.coordinates[0];
							puntot.y = feature.geometry.coordinates[1];
		      			  	output.push(puntot);
		      			  	multipoint.addPoint(punto);
						}
					}  											
				}
				algoQueGuardar = true;
				addGraphicToMap(multipoint, symbolPuntos, "esriGeometryMultipoint");
				map.setExtent(multipoint.getExtent());
			}
			if (output.length>0){
				$("#divCarrusel").attr('style', "visibility:visible;");
				showCarrusel(output);
			}
		}
	}else{
		new Messi(typeLang.ALERT_4, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
	}
}

function punto_esri(x,y){
	
		point = new esri.geometry.Point(x,y,map.spatialReference);
		return esri.geometry.geographicToWebMercator(point);
	 
}

function resetCarrusel(){
	$("#divCarrusel").attr('style', "visibility:hidden;");
    $("#btnCarPrevious").attr('style', "visibility:hidden;");
    $("#btnCarNext").attr('style', "visibility:hidden;");
}

function tratarOutput(output,borarGraphs){
	if (output.length > 0) {
  	  var tipoObjeto = getTypeObjeto(output);  	  
  	  $("#divCarrusel").attr('style', "visibility:visible;");
  	  if (borarGraphs)
  		  deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
  	  pintarObjeto(output, tipoObjeto,false,borarGraphs,false);
    } else {
  	  RemoveAllGraphicsfromView();
  	  resetCarrusel();
  	  
  	  for (var i = 0; i < elementosAGuardar.length; i++) {
  		  var element = elementosAGuardar[i];
  		  if (elementosAGuardar[i].action == 1) {
  			  elementosAGuardar.splice(i, 1);
  		  }
  	  }
  	  $("#Add").prop("disabled", true);                    
    }
}

function getTypeObjeto(array) {
    var tipoObjeto = 0;
    if (array.length == 1) {
        //it is a point(1)
        tipoObjeto = 1;
    } else {
        punto = array[0];
        ultimoPunto = array[array.length - 1];
        if (punto.x != ultimoPunto.x || punto.y != ultimoPunto.y) {
            //it is a line (3)	
            tipoObjeto = 3;
        } else {
            //it is a polygon (2)
            tipoObjeto = 2;
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
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.y) + " " + typeLang.Longitude +"=" + formatearCoordenada(punto.x));
        $("#radio_administrationLavelName").prop('checked', true);
        $("#radio_administrationLavelName").prop('disabled', true);
        $("#radio_administrationLavelName").click();

        $("#btnCarPrevious").attr('style', "visibility:hidden;");
        $("#btnCarNext").attr('style', "visibility:hidden;");
        posicionCheck = 1;
        getAdministrativeLevelsEsri(punto,0);
    } else {
        $("#carPos").html("1");
        $("#carTam").html(array.length);
        punto = array[0];
        $("#carCoordinate").html(typeLang.Latitude + "=" + formatearCoordenada(punto.y) + " " + typeLang.Longitude + "=" + formatearCoordenada(punto.x));
        $("#btnCarPrevious").attr('style', "visibility:hidden;");
        $("#btnCarNext").attr('style', "visibility:visible;");
        $("#radio_administrationLavelName").prop('checked', false);
        $("#radio_administrationLavelName").prop('disabled', false);

        posicionCheck = 0;
        totalLLamadas = array.length;
        for (var i = 0; i<array.length; i++){
        	getAdministrativeLevelsEsri(array[i],i);
        }
    }
}


function todosLosNivelesIguales(){
	if (listAdm.length==0)
		return true;
	
	var nivel1 = listAdm[0].level1;
	var nivel2 = listAdm[0].level2;
	var nivel3 = listAdm[0].level3;
		
	for (var i=1; i<listAdm.length; i++){
		if (nivel1!=listAdm[i].level1 || nivel2!=listAdm[i].level2 || nivel3!=listAdm[i].level3)
			return false;
	}		
	return true;
}

/**
 * This function retrieves the administrative levels using the Esri Service Geocode (commented code)
 * Using Bing API
 * @param punto
 */
function getAdministrativeLevelsEsri(punto,orden) {    
	peticionBing(punto.y, punto.x,orden);
}

function drawLevels(first, second, third) {
	if (first!= undefined )
		$("#firstCountry").html(first);
	else
		$("#firstCountry").html("");
	
	if (second!= undefined )
		$("#secondDistrict").html(second);
	else
		$("#secondDistrict").html("");
	
	if (third!= undefined )
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

function clickarWidget(elemento,borrarAllGrahics) {
    $("#selectTypeCoordinates").attr('checked', 'checked');
    $("input[name=selectType]").change();
    if (borrarAllGrahics)
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
    var total=0;
	
	if (widget == PUNTOS_WIDGET || widget == PUNTOS_WIDGET_VO)
		total =  LineasArray.length + poligonosArray.length;	
	if (widget == POLIGONOS_WIDGET || widget == POLIGONOS_WIDGET_VO)
		total = puntosArray.length + LineasArray.length ;
	if (widget == LINEAS_WIDGET || widget == LINEAS_WIDGET_VO)
		total = puntosArray.length + poligonosArray.length;

	if (elementosAGuardar.length == 0 && (total > 0)) {
        Messi.ask(typeLang.ALERT_5, function (val) {
            if (val == "Y") {
                deleteGraphicsLayer(puntosArray, LineasArray, poligonosArray); //Delete the Graphics  			  
                RemoveAllGraphicsfromView();
                isEmpty = true;
                algoQueGuardar = false;

                if (widget != null) clickarWidget(widget,true);

                return true;
            } else {
                $("#divCarrusel").hide();
                $("#txtfileName").html("No file selected");
                $("#BrowseInput").val("");

                return false;
            }
        });    
    } else if (elementosAGuardar.length > 0) {
    	    	       	
    	if (!hayelementosAnadirOModificarDistintoTipoDelWidget(widget)) {
            Messi.ask(typeLang.ALERT_5, function (val) {            	
                if (val == "Y") {
	        	isEmpty = true;
	                algoQueGuardar = false;
                	deleteGraphicsLayer(puntosArray, LineasArray, poligonosArray); //Delete the Graphics
                	soloEliminarElementosAGuardar();                    
	                RemoveAllGraphicsfromView();

                    if (widget != null) clickarWidget(widget,true);
                    return true;

                } else {
                    $("#divCarrusel").hide();

                    $("#txtfileName").html("No file selected.");
                    $("#BrowseInput").val("");

                    return false;
                }

            });         
        } else {
        	deleteGraphicsLayer(puntosArray, LineasArray, poligonosArray); //Delete the Graphics
            if (widget != null) clickarWidget(widget,false);
            return true;
        }
    } else {    	
        if (widget != null) clickarWidget(widget,true);
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
        var data = "VisualOutputVersionId=" + visual_output_version_id + "&Level1=" + level1 + "&Level2=" + level2 + "&Level3=" + level3;
        ajaxCallSave(data, UPDATE_VISUAL_OUTPUT_VERSION, updateAdministrativeLevelsResponse);
    } else {
        var data = "VisualProjectVersionId=" + visual_output_version_id + "&Level1=" + level1 + "&Level2=" + level2 + "&Level3=" + level3;
        ajaxCallSave(data, UPDATE_VISUAL_PROJECT_VERSION, updateAdministrativeLevelsResponse);
    }
}


/**
 * Response function of updating adiminstrative levels and calls guardarMapa function passing the new versionID,  in order to save map changes.
 * @param response
 */
function updateAdministrativeLevelsResponse(response) {

    if (response == "ok") {
        guardarMapa(visual_output_version_id, elementosAGuardar.length - 1);
        getNumberGraphicsFeatures(false, visual_output_version_id);

        window.parent.$("#geoLocationAddress").html($('#geoLocationAddress'));
        window.parent.$("#level1Name").text($("#firstCountry").text().concat()); 
        window.parent.$("#level2Name").text($("#secondDistrict").text().concat());
        window.parent.$("#level3Name").text($("#thirdMunicipality").text().concat());
        window.parent.$("#inputValueLevel1").val($("#firstCountry").text().concat());
        window.parent.$("#inputValueLevel2").val($("#secondDistrict").text().concat());
        window.parent.$("#inputValueLevel3").val($("#thirdMunicipality").text().concat());
        var win = window.parent.$(".dinamicModal").data("kendoWindow");
    } else {
        new Messi(typeLang.ALERT_UPDATE_INFORMATION, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
    }
}

function fakeMultipoint(geometria) {
    if (geometria.type == "multipoint") {
        if (geometria.points.length == 1) {
            geometria.addPoint(geometria.points[0]);
        }
    }
    return geometria;
}

/** 
 * Save the features updating the value of VISUAL_OUTPUT_VERSION_ID or VISUAL_PROJECT_VERSION_ID retrieved of the service.
 */
function guardarMapa(newVersionID, vuelta) {
    if (vuelta>-1){
        elementosGuardados = elementosAGuardar.length;
        var alertAdmLevels = false;
        var elementosAGuardarAgrupadas = [];
        if (elementosAGuardar.length > 0) {
            var element = elementosAGuardar[vuelta]["graphic"];

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
            var oldGeometry = element.geometry;
            element.geometry = fakeMultipoint(element.geometry);
            var updateGraphic = new esri.Graphic(element.geometry, null, attr, null);
            var graph = elementosAGuardar[vuelta];
            var accion = elementosAGuardar[vuelta]["action"];

            if (ActionsEnum["Anadir"] == accion) {

                elementosAGuardar[vuelta]["feature"].applyEdits([updateGraphic], null, null, function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                }, function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                });

                var textPoints = "";

                textPoints = convert_To_LongLat(oldGeometry);

                $("#geoLocationAddress").html(jQuery.trim(textPoints));
            } else if (ActionsEnum["Modificar"] == accion) {
                elementosAGuardar[vuelta]["feature"].applyEdits(null, [updateGraphic], null, function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                }, function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                });

            } else if (ActionsEnum["Eliminar"] == accion) {
                elementosAGuardar[vuelta]["feature"].applyEdits(null, null, [updateGraphic], function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                }, function (error) {
                    guardarMapa(newVersionID, vuelta - 1);
                });
            }
        } else {

            if (!alertAdmLevels) {
                elementosAGuardar = new Array();
                comproborBotonAnadir();
                nivelAsignado = false;
                firstLevel = "";
                secondLevel = "";
                thirdLevel = "";
                typeSelect = "";
            }
            window.parent.saveMap_callback();
        }
    } else {
        window.parent.saveMap_callback();
    }

}

function elementoGuardado() {
    elementosGuardados = elementosGuardados - 1;
    if (elementosGuardados == 0)
        window.parent.saveMap_callback();
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
    	case "multipoint": 
    		var points = updateGraphic.geometry.points;
            for (var i = 0; i < points.length ; i++) {    		    		
            	var x = points[i][0];
                var y = points[i][1];
                var p = esri.geometry.xyToLngLat(x, y);
                puntos[i] = { "x": p[0], "y": p[1] };
                GeolocationSpatialTypeName = 'MULTIPOINT';
            }            
            break;
        case "point":
            var p = esri.geometry.xyToLngLat(updateGraphic.geometry.x, updateGraphic.geometry.y);
            puntos[0] = { "x": p[0], "y": p[1] };
            GeolocationSpatialTypeName = 'POINT';
            break;
        case "polyline":
            var path = updateGraphic.geometry.paths[0];
            for (var i = 0; i < path.length ; i++) {
                var x = path[i][0];
                var y = path[i][1];
                var p = esri.geometry.xyToLngLat(x, y);
                puntos[i] = { "x": p[0], "y": p[1] };
                GeolocationSpatialTypeName = 'LINE';
            }
            break;
        case "polygon": 

            var ring = updateGraphic.geometry.rings[0];

            for (var i = 0; i < ring.length ; i++) {
                var x = ring[i][0];
                var y = ring[i][1];
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
            window.parent.$("#GeolocationSpatialTypeName").text(GeolocationSpatialTypeName);
        },
        error: function (msg) {
            console.log(typeLang.ERROR_SERVER + ": " + UPDATE_GEOLOCATION + "  msg:" + msg.textStatus);
        }
    });
}

function inArrayPunto(p, aray) {
    for (var i=0; i< aray.length; i++){
        if (aray[i][0]==p[0] && aray[i][1]==p[1] ){
            return i;
        }
    }
    return -1;
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
        case "multipoint":
            var points = point[0].geometry.points;
            var insertedPoints = [];
            for (var i = 0; i < points.length ; i++) {
                var p = esri.geometry.xyToLngLat(points[i][0], points[i][1]);
                if (inArrayPunto(p, insertedPoints) == -1) {
                    insertedPoints.push(p);
                    coordinateString = formatearCoordenada(p[1]) + "," + formatearCoordenada(p[0])+";";                                                
                }
    		}
    		coordinateString = coordinateString.substr(0, coordinateString.length - 1);
        break;
        case "point":
            var p = esri.geometry.xyToLngLat(point[0].geometry.y, point[0].geometry.x);
            coordinateString = formatearCoordenada(p[1]) + "," + formatearCoordenada(p[0]);
            break;
        case "polyline":
            var path = line[0].geometry.paths[0];

            for (var i = 0; i < path.length ; i++) {
                var x = path[i][0];
                var y = path[i][1];
                var p = esri.geometry.xyToLngLat(x, y);
                coordinateString = coordinateString + formatearCoordenada(p[1]) + "," + formatearCoordenada(p[0]);

                if (i != path.length - 1) {
                    coordinateString = coordinateString + ";";
                }
            }
            break;
        case "polygon":

            var ring = polygon[0].geometry.rings[0];

            for (var i = 0; i < ring.length ; i++) {
                var x = ring[i][0];
                var y = ring[i][1];
                var p = esri.geometry.xyToLngLat(x, y);

                coordinateString = coordinateString + formatearCoordenada(p[1]) + "," + formatearCoordenada(p[0]);

                if (i != ring.length - 1) {
                    coordinateString = coordinateString + ";";
                }
            }
            break;
    }

    $("#coordinates").val(coordinateString);
}

function setAppLanguage(language) {
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

function ajaxCallSave(data, urlService, successFunction) {
    console.log(data);
    console.log(urlService);    
    $.ajax({
        type: "GET",
        url: urlService,
        async: false,
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

function peticionBing(lat, long,orden) {	
	 $.getJSON('https://dev.virtualearth.net/REST/v1/Locations/'+ lat + ','+ long + '?key='+BingMapsKey+'&jsonp=?', function (result) {
		 MapResults(result,orden);         
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
                });
    } else {
        new Messi(typeLang.ALERT_ADDRESS, { title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }] });
    }
}

//For Address searching 
function setupAutoComplete()
{
    $("#address").autocomplete(
    {
        source: function (request, response) { inputAddressAutocomplete(response); },
        minLength: 4,
        open: function (event, ui)
        {
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
        select: function (event, ui)
        {
            deleteGraphicsLayer(puntosArrayGraphics, lineasArrayGraphics, poligonosArrayGraphics); //Delete the Graphics
            soloEliminarElementosAGuardar();
            RemoveAllGraphicsfromView();
            isEmpty = true;
            algoQueGuardar = false;

            $("#coordinates").val("");

            $("#AddressIDDiv").hide();
            $("#address").val("");

            $("#divCarrusel").hide();
            $("#BrowseInput").val("");
            $("#txtfileName").html("No file selected");
            $("#geoLocationAddress").html("");

            output = new Array();
            var output2 = new Array();

            var point = new puntoLeido();
            point.x = ui.item.extent.coordinates[1];
            point.y = ui.item.extent.coordinates[0];
            algoQueGuardar = true;
            output.push(point);

            var tipoObjeto = getTypeObjeto(output);
            widget = PUNTOS_WIDGET;
            pintarObjeto(output, tipoObjeto, false, true, true);

            showCarrusel(output);

            $("#ulFile").removeClass("hide");
            $("#liFile1").attr('style', "display:none;");
            $("#liFile2").attr('style', "display:none;");
            $("#divCarrusel").attr('style', "visibility:visible;");    
        }
    })
    .data("ui-autocomplete")._renderItem = function (ul, item)
    {
        return $("<li></li>")
          .data("item.autocomplete", item)
          .append("<a><b>" + item.label + "</b></a><br><br>")
          .appendTo(ul);
    };
}

//END ADDRESS AUTOCOMPLETE BING
function MapResults(response,orden) {
	totalLLamadas = totalLLamadas-1; 
    if (response.resourceSets[0].resources.length > 0) {
        var results = response.resourceSets[0].resources;
        var position = parseInt($("#carPos").html(), 10)-1;
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
            listAdm[orden]= admLevel;

        } else {
        	if (orden == position)
        		drawLevels("", "", "");
            window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
            var admLevel = new admLevels();
            admLevel.level1 = "";
            admLevel.level2 = "";
            admLevel.level3 = "";
            listAdm[orden]= admLevel;
            
        }

    }
    else {
    	if (orden == position)
    		drawLevels("", "", "");
        window.parent.$(".dinamicModal").height($('.pie').height() + $('.central').height() + 30);
        var admLevel = new admLevels();
        admLevel.level1 = "";
        admLevel.level2 = "";
        admLevel.level3 = "";
        listAdm[orden]= admLevel;
    }
    if (totalLLamadas==0){
    	if (todosLosNivelesIguales()){
    	    $("#radio_administrationLavelName").prop('checked', true);
    	    posicionCheck = 1;
    	    nivelAsignado = true;
    	    firstLevel = $("#firstCountry").text().concat();
    	    secondLevel = $("#secondDistrict").text().concat();
    	    thirdLevel = $("#thirdMunicipality").text().concat();
      }
    }
}

function deleteGraphicsLayersAttributeNull()
{
    if (pointServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < pointServiceLayer.graphics.length ; i++) {
            var graph = pointServiceLayer.graphics[i];
            if (graph.attributes == null) {
                pointServiceLayer.remove(graph);
                i = 0;
            }
        }
    }

    if (LineServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < LineServiceLayer.graphics.length ; i++) {
            var graph = LineServiceLayer.graphics[i];
            if (graph.attributes == null) {
                LineServiceLayer.remove(graph);
                i = 0;
            }
        }
    }

    if (PolygonServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < PolygonServiceLayer.graphics.length ; i++) {
            var graph = PolygonServiceLayer.graphics[i];
            if (graph.attributes == null) {
                PolygonServiceLayer.remove(graph);
                i = 0;
            }
        }
    }
}

function admLevels()
{
    this.level1;
    this.level2;
    this.level3;
}

function block()
{
    $.blockUI();
}

function unBlock()
{
    $.unblockUI();
}

//devuelve -1 si no hay ningun elemento a añadir, si lo hay devuelve su indice 
function yaExisteAlgunoParaAnadir()
{
    for (var i = 0; i < elementosAGuardar.length; i++)
    {
		if (elementosAGuardar[i].action == ActionsEnum["Anadir"])
			return i;
	}
	return -1;
}

function getGraphicsFromMaps2Array()
{
    if (pointServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < pointServiceLayer.graphics.length ; i++)
        {
            anadirAccion(pointServiceLayer, pointServiceLayer.graphics[i], ActionsEnum["Anadir"]);        
        }
    }

    if (LineServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < LineServiceLayer.graphics.length ; i++)
        {
            anadirAccion(LineServiceLayer, LineServiceLayer.graphics[i], ActionsEnum["Anadir"]);
        }
    }

    if (PolygonServiceLayer.graphics.length > 0)
    {
        for (var i = 0; i < PolygonServiceLayer.graphics.length ; i++)
        {
            anadirAccion(PolygonServiceLayer, PolygonServiceLayer.graphics[i], ActionsEnum["Anadir"]);
        }
    }
}


function hayelementosAnadirOModificarDistintoTipoDelWidget(widget)
{
    for (var i = 0; i < elementosAGuardar.length; i++)
    {
        if (elementosAGuardar[i].action == ActionsEnum["Anadir"] || elementosAGuardar[i].action == ActionsEnum["Modificar"])
        {
			if (elementosAGuardar[i].graphic.geometry.type == "polygon" && !(widget == POLIGONOS_WIDGET_VO || widget == POLIGONOS_WIDGET))
				return false;
			if (elementosAGuardar[i].graphic.geometry.type == "multipoint" && !(widget == PUNTOS_WIDGET_VO || widget == PUNTOS_WIDGET))
			    return false;
			if (elementosAGuardar[i].graphic.geometry.type == "point" && !(widget == PUNTOS_WIDGET_VO || widget == PUNTOS_WIDGET))
			    return false;
			if (elementosAGuardar[i].graphic.geometry.type == "polyline" && !(widget == LINEAS_WIDGET_VO || widget == LINEAS_WIDGET))
			    return false;
		}
	}
	return true;
}

function validateForm()
{
    if (algoQueGuardar)
    {
        if ($("#radio_administrationLavelName").is(":disabled"))
        {
            $("#radio_administrationLavelName").click();
        }

        if (!nivelAsignado)
        {
            new Messi(typeLang.ALERT_ADM_LEVELS,{
                    title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }]
            });
            alertAdmLevels = true;
            return false; 
        } else {
            return true;
        }
    } else {
        if (visual_output_id <= 0 || isEmpty) {
            new Messi(typeLang.ALERT_NOTHIG_TO_SAVE, {
                title: '', titleClass: 'info', buttons: [{ id: 0, label: 'Close', val: 'X' }]
            });
            return false;
        } else {
            Guardar = false;
            return true;
        }
    }
}

function add_click() {
    if (Guardar){
        updateAdministrativeLevels(firstLevel, secondLevel, thirdLevel);
    } else {
        window.parent.saveMap_callback();
    }        
}


