var files = null;
var geocoder;
var templatePicker;
var layers = new Array();
var editToolbar;
var arcGisMap = null;

var country;

function getURLParams() {
    var search = location.search;
    var params = {};

    if (search.indexOf('?') >= 0) {
        var urlGet = search.substr(1).replace(/\&amp\;/ig, '@@@@@');
        $.each(urlGet.split('&'), function (index, item) {
            var parts = item.replace(/\@\@\@\@\@/, '&amp;').split('=');
            if (parts.length > 1) {
                params[decodeURIComponent(parts[0])] = decodeURIComponent(parts[1]);
            }
        });
    }
    return params;
}

define(["esri/map",
        "esri/dijit/Print",
        "esri/dijit/Geocoder",
        "esri/toolbars/draw",
        "esri/toolbars/edit",
        "esri/tasks/locator", 
        "esri/graphic",
        "esri/config",
        "esri/layers/FeatureLayer",
        "esri/InfoTemplate", 
        "esri/symbols/SimpleMarkerSymbol",
        "esri/symbols/SimpleLineSymbol",
        "esri/symbols/SimpleFillSymbol",

        "esri/dijit/editing/TemplatePicker",
        "esri/symbols/Font", 
        "esri/symbols/TextSymbol",
        "esri/geometry/Point",
        "esri/renderers/ClassBreaksRenderer",
        "esri/layers/GraphicsLayer",

        "dojo/_base/array",
        "dojo/_base/event", 
        "dojo/_base/Color",
        "dojo/number", 
        "dojo/parser", 
        "dojo/dom", 
        "dijit/registry",
        
        "dijit/form/Button", 
        "dijit/form/Textarea",
        "dijit/layout/BorderContainer", 
        "dijit/layout/ContentPane",
        "dojo/domReady!"],
        
    function(maps, Printer, Geocoder,Draw, Edit, Locator, Graphic, esriConfig, FeatureLayer,
        InfoTemplate, SimpleMarkerSymbol, SimpleLineSymbol, SimpleFillSymbol,TemplatePicker,
        Font, TextSymbol,
        arrayUtils, event, Color,
        number, parser, dom, registry, Point,
        ClassBreaksRenderer, GraphicsLayer) {

        esriConfig.defaults.io.proxyUrl = arcgisProxy;
            
        var urlParams = getURLParams();
        objectType = urlParams['objectType'];

        var ArcgisMap =  function(node,options){
            this._init(node, options);
        };

        ArcgisMap.prototype.MOD_EDIT = "mode_edit";
        ArcgisMap.prototype.MODE_EDIT_FILE = 0;
        ArcgisMap.prototype.MODE_EDIT_COORDINATES = 1;
        ArcgisMap.prototype.AMAP_MODE_SHOW = "mode_showOnly";
        ArcgisMap.prototype.AMAP_MAP_TYPE_NODE_CLASS = "AMAP_mapTypeSelect";
        ArcgisMap.prototype.AMAP_EDIT_TYPE_NODE_CLASS = "AMAP_editTypeSelect";
        ArcgisMap.prototype.AMAP_TOOLBAR_NODE_CLASS = "AMAP_toolbar";
        ArcgisMap.prototype.AMAP_NAVBAR = "navegationBar";
        ArcgisMap.prototype.AMAP_TOOLBAR_LOCATE_BUTTON = "AMAP_locateButton";
        ArcgisMap.prototype.AMAP_TOOLBAR_CLEAR = "cancel";
        ArcgisMap.prototype.AMAP_EDIT_BUTTON = "AMAP_edit";
        ArcgisMap.prototype.AMAP_MAP_NODE = "AMAP_map";
        ArcgisMap.prototype.AMAP_MAP_MODE = "streets";
        ArcgisMap.prototype.AMAP_COORDINATES_ID = "coordinates";
        ArcgisMap.prototype.AMAP_FIND_BUTTON = "Find";
        ArcgisMap.prototype.AMAP_OPERATION_DATA = "operationData";
        ArcgisMap.prototype.AMAP_OPTION_LIST = "optionList";
        ArcgisMap.prototype.AMAP_OPTION_SELECT = "AMAP_addTypeSelect";
        ArcgisMap.prototype.AMAP_FOOTER = "pie";
        ArcgisMap.prototype.AMAP_EDIT_ID = "Edit";
        ArcgisMap.prototype.AMAP_FOOTER_BUTTONS = "botones";
        ArcgisMap.prototype.AMAP_MARKER_URL = "img/modulos/map/map-marker.png";
        ArcgisMap.prototype.AMAP_BROWSE_BUTTON = "Browse";
        ArcgisMap.prototype.AMAP_BROWSE_INPUT = "BrowseInput";
        ArcgisMap.prototype.AMAP_FILE_TEXT = "fileName";
        ArcgisMap.prototype.AMAP_FILE_INFO = "file";
        ArcgisMap.prototype.AMAP_COORDINATES_INFO = "coordinates";
        ArcgisMap.prototype.AMAP_ADDRESS_INFO = "address_info";
        ArcgisMap.prototype._init = function(node,options){

            this.node = node;
			this.inputMaxLength = options.inputMaxLength != undefined ? options.inputMaxLength : 28;
            this.mode = options.mode === undefined ? this.MODE_SHOW : options.mode;
            this.node_select_mapLayer = options.node_select_mapLayer === undefined ? $("."+this.AMAP_MAP_TYPE_NODE_CLASS) : options.node_select_mapLayer;
            this.node_optionSelect = options.node_optionSelect || $("."+this.AMAP_OPTION_SELECT);
            this.edit_type = options.edit_type === undefined ? -1 : options.edit_type;
            this.node_editButton = options.node_editButton || $("#"+this.AMAP_EDIT_ID);
            this.node_cancelButton = options.node_cancelButton;
            this.node_id_map = options.node_id_map;
            this.node_toolbar = options.node_toolbar === undefined ? $("."+this.AMAP_TOOLBAR_NODE_CLASS) : options.node_toolbar;
            this.node_selectEditType = options.node_selectEditType;
            this.node_coordinates = options.node_coordinates || $("#"+this.AMAP_COORDINATES_ID);
            this.coordinates = [];
            this.node_findButton = options.node_findButton || $("#"+this.AMAP_FIND_BUTTON);
            this.map_locator;
            this.node_operationData = options.node_operationData || $("."+this.AMAP_OPERATION_DATA);
            this.node_footerButtons = options.node_footerButtons || $("."+this.AMAP_FOOTER+" ."+this.AMAP_FOOTER_BUTTONS);
            this.map_modeSelector;
            this.point;
            this.node_clearMap = options.node_clearMap || $("."+this.AMAP_NAVBAR+" ."+this.AMAP_TOOLBAR_CLEAR);
            this.node_browseButton = options.node_browseButton || $("#"+this.AMAP_BROWSE_BUTTON);
            this.node_browseInput = options.node_browseInput || $("#"+this.AMAP_BROWSE_INPUT);
            this.node_fileName = options.node_fileName || $("."+this.AMAP_OPERATION_DATA+" ."+this.AMAP_FILE_TEXT);
            this.nav_bar = options.nav_bar || $("."+this.AMAP_NAVBAR);
            this.node_fileInfo = options.node_fileInfo || $("."+this.AMAP_FILE_INFO);
            this.node_coordinatesInfo = options.node_coordinatesInfo || $("."+this.AMAP_COORDINATES_INFO);
            this.node_addressInfo = options.node_addressInfo || $("."+this.AMAP_ADDRESS_INFO);
            this.map_instance = this._initMap(this.node_id_map);
            this.toolbar = this._initToolbar();
            this._initDataCol();
        };

        ArcgisMap.prototype._onSelectMapLayer = function(value) {
            if(value !== "Map view"){
                this.AMAP_MAP_MODE = value.toLowerCase();
                map.setBasemap(this.AMAP_MAP_MODE);
            }
        };

        ArcgisMap.prototype._initSelectLayer = function(node_select_mapLayer) {
            var instance = this;
            this.select_mapLayer = node_select_mapLayer.kendoDropDownList({height:"auto"});
            this.select_mapLayer.on("change",function(){instance._onSelectMapLayer($(this).val());})
        };

        ArcgisMap.prototype._initSelectAddType= function(node_select_addType) {
            var instance = this;
            this.select_addType = node_select_addType.kendoDropDownList();
            this.select_addType.on("change",function(){instance._onSelectTypeLayer($(this).val());})
        };

        ArcgisMap.prototype._initMap = function(node_id_map) {
             var optionsMap = {
                basemap: this.AMAP_MAP_MODE,
                zoom: 1,
                logo: false,
                autoResize: true
            };
             
            return new maps(node_id_map,optionsMap);
        };

        ArcgisMap.prototype._initToolbar = function() {
           var instance = this;
           var toolbar = {};
           toolbar.locate_button =  $("."+this.AMAP_TOOLBAR_NODE_CLASS+" ."+this.AMAP_TOOLBAR_LOCATE_BUTTON);
           this.map_modeSelector = this._initSelectLayer(this.node_select_mapLayer);
           this.node_clearMap.on("click",function(){ instance._clearMap(); });
    
        };

        ArcgisMap.prototype._initOperationData = function() {
            var instance = this;
            
           if(this.node_browseButton.length > 0){
        	   
				this.node_browseInput.on("change",function() {
				    instance._writeFile(instance.node_browseInput.val());
					manejadorDeArchivos(this.files);
				});
				
                this.node_browseButton.on("click",function(){
				
  	                $("#BrowseInput").val("");
  	               	              
                    instance.node_browseInput.click();
                    
                    typeSelect = $("input[name=selectType]").val().concat();
                    
                    return false;
                });
            }

        };
        
        ArcgisMap.prototype._writeFile = function(fileName) {
			var text;
			if(fileName.length > this.inputMaxLength){
				text = "..."+fileName.slice(-this.inputMaxLength);
			}else{
				text = fileName;
			}
            this.node_fileName.html(text);
            this.node_fileName.attr("title",fileName);
        };

        ArcgisMap.prototype._readCoordinates = function() {
            if(this.node_coordinates.length > 0){
                var coordArray = this.node_coordinates.val().split(";");
                var coordX = coordArray[0].split("=")[1];
                var coordY = coordArray[1].split("=")[1];
                if(coordX === undefined){ coordX = coordArray[0] }
                if(coordY === undefined){ coordY = coordArray[1] }
                this.coordinates = [parseFloat(coordX),parseFloat(coordY)];
            }
        }

        ArcgisMap.prototype._writeCoordinates = function() {
            if(this.coordinates.length > 0){
                var coordString = "x="+this.coordinates[0]+"; y="+this.coordinates[1];
                var coordText = $("."+this.AMAP_OPERATION_DATA+" .data .data1").first().find("p").last();
                coordText.html(coordString);
            }
        };

        ArcgisMap.prototype._sendToCoordinates = function(){
            if(this.coordinates.length > 0){
                var instance = this;
                this.point = new esri.geometry.Point(this.coordinates[0],this.coordinates[1]);
                this.map_instance.centerAndZoom(this.point,10);
                this.map_instance.on("load",instance._putMarker());
            }
        };
        
        ArcgisMap.prototype._putMarker = function() {
            var instance = this;
            if(this.markerLayer !== undefined){ this.markerLayer.hide(); }
            var point = esri.geometry.geographicToWebMercator(instance.point);
            var markerRenderer = new esri.symbol.PictureMarkerSymbol(instance.AMAP_MARKER_URL,35,35);
            var marker = new esri.Graphic(point, markerRenderer);
            this.markerLayer = new esri.layers.GraphicsLayer();
            this.markerLayer.add(marker);
            this.map_instance.addLayer(this.markerLayer);
        };
        
        ArcgisMap.prototype._clearMap = function(){
            if(this.markerLayer !== undefined){
                this.markerLayer.hide();
            }
        };
        
        ArcgisMap.prototype._initEditButton = function() {
            var instance = this;
            this.node_editButton.on("click",function(){ instance._showEditInfo();  EditionMode =true;});
        };

        ArcgisMap.prototype._showEditInfo = function(){
            $("."+this.AMAP_FOOTER).addClass("hide");
            $(".edit").removeClass("hide");
            
            $("#coordinates").val("");

            $("#selectTypeCoordinates").attr('checked', 'checked');
            $("input[name=selectType]").change();
            getCoordinatesEditButton();
            $("#principal").show();
            
    	    $("#FindAddressDiv").hide();
    	    
    	    getAllSymbolsFeatures(); 
    	    getNumberGraphicsFeatures(false, visual_output_version_id);
    	    getGraphicsFromMaps2Array();
        };

        ArcgisMap.prototype._onSelectTypeLayer = function(value) {
            var valueT = value.toLowerCase();
            if(valueT === "coordinates"){
                $("."+this.AMAP_OPERATION_DATA+" .data_address").addClass("hide");
                $("."+this.AMAP_OPERATION_DATA+" .data").removeClass("hide");
                this.node_coordinatesInfo.removeClass("hide");
                this.node_addressInfo.addClass("hide");
                this.node_fileInfo.addClass("hide");

                $("#principal").hide();
                $("#coordinates").val("");
            }else if(valueT === "file"){
                $("."+this.AMAP_OPERATION_DATA+" .data").addClass("hide");
                this.node_coordinatesInfo.addClass("hide");
                this.node_addressInfo.addClass("hide");
                $("."+this.AMAP_OPERATION_DATA+" .data_address").addClass("hide");
                this.node_fileInfo.removeClass("hide");
                
                $("#principal").hide();
                $("#coordinates").val("");
                $("#divCarrusel").hide();

         	   $("#liFile1").attr('style',"display:block;");
         	   $("#liFile2").attr('style',"display:block;");

                this.node_fileName.html("No file selected.");
                $("#BrowseInput").val("");
            }else if(valueT === "select"){
                $("."+this.AMAP_OPERATION_DATA+" .data").addClass("hide");
                this.node_coordinatesInfo.addClass("hide");
                this.node_addressInfo.addClass("hide");
                $("."+this.AMAP_OPERATION_DATA+" .data_address").addClass("hide");
                this.node_fileInfo.addClass("hide");

                $("#principal").hide();
                $("#coordinates").val("");
                $("#divCarrusel").hide();
            }if(valueT === "address"){
                $("."+this.AMAP_OPERATION_DATA+" .data").addClass("hide");
                $("."+this.AMAP_OPERATION_DATA+" .data_address").removeClass("hide");
                this.node_coordinatesInfo.addClass("hide");
                this.node_addressInfo.removeClass("hide");
                this.node_fileInfo.addClass("hide");
                $("#AddressIDDiv").hide();
                $("#address").val("");
            }
        };

        ArcgisMap.prototype._initDataCol = function(){ 
            this._initEditButton();
            this._initOperationData();
            this._initSelectAddType(this.node_optionSelect);
        };

        ArcgisMap.prototype.initPrinter = function () {
            this.printer = new Printer({
                map: this.map_instance,
                url: hostEsriServicesUrl + "arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task"
            }, $("#printerButton").get(0));
            this.printer.startup();
        };

        arcGisMap = ArcgisMap;

        return ArcgisMap;
});