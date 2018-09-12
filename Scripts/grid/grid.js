



var GridComponent = function (grid, pageSize, selectable, pageable) {
    this.grid = grid;
    this.pageSize = pageSize;
    this.selectable = selectable;
    this.pageable = pageable;
    this._createGrid();
};

GridComponent.prototype._createGrid = function(){    
        
    var instance = this;
    var rButtons = $(".table_search_item").find("table").find("tbody").find(".radio");
    /*
    $(this.grid).kendoGrid({
            dataSource:{pageSize:instance.pageSize},
            selectable: true,
            sortable: true,
            deletable: true,
            reorderable: false,
            scrollable: false,
            pageable: instance.pageable,
            columnMenu: true,
            nullable: true
    }); 
    */
    $(this.grid).kendoGrid({
        dataSource: { pageSize: instance.pageSize },
        selectable: instance.selectable,
        sortable: true,
        deletable: true,
        reorderable: false,
        scrollable: false,
        dataBound: function () {
            $(".radio").click(function () {
                var rButtons = $(".table_search_item").find("table").find("tbody").find(".radio");
                for (var i = 0; i < rButtons.length; i++) {
                    $(rButtons[i]).removeClass("checked");
                }
                $(this).addClass("checked");
                for (var i = 0; i < rButtons.length; i++) {
                    if ($(rButtons[i]).hasClass("checked")) {
                        $(rButtons[i]).closest("tr").addClass("k-state-selected");
                    } else {
                        $(rButtons[i]).closest("tr").removeClass("k-state-selected");
                    }
                }
            });

            $(".grid2").find(".btn_delete").click(function () {
                $('.mod_tabla.mod_tabla_agrup').hide();
                $('.pie.bottom').hide();
                $('.table_search_item').hide();
                $('.buscadorConvergence').hide();
                $('.cancelConfirm').show();
                $('.pie.top').show();
            });

        },
        pageable: instance.pageable,
        columnMenu: true,
        nullable: true
    });

    function ponerSelected() {
        for (var i = 0; i < rButtons.length; i++) {
            if ($(rButtons[i]).hasClass("checked")) {
                $(rButtons[i]).closest("tr").addClass("k-state-selected");
            } else {
                $(rButtons[i]).closest("tr").removeClass("k-state-selected");
            }
        }
    }

    ponerSelected();

    this._formatCells();

};

GridComponent.prototype._formatCells = function(){
    $('.k-header-column-menu').hide();
	$('div.k-grid-header').removeAttr('style');
	$('span.k-pager-info').remove();
	$('div[data-role="pager"] > a[tabindex="-1"]').remove();
};

/*
Creación del objeto grid para la tabla "Padres_Hijos" ordenable por "Padres"
*/

var GridHierarchyComponent = function (grid, pageSize, selectable, pageable) {
    this.grid = grid;
    this.pageSize = pageSize;
    this.pageable = pageable;
    this._createGridHierarchy();
}

// Creación del prototipo Grid
GridHierarchyComponent.prototype._createGridHierarchy = function () {

    var instance = this;
    // Variable que va a tener la información relativa a todos los datos "Padre" de la tabla.
    // Importante en numberId ya que servirá para relacionar los datos "Hijo" a cada dato "Padre".
    // el data es de tipo JSON
    var datosPadre = [{ "numberId": 0, "expensesCategory": "01.00.00.00 - LOREM IPSUM", "currentIDB": "0.000.000,00", "disbursedToDate": "0.000.000,00", "disbursedxCent": "00,00", "balance": "000.000,00" },
			{ "numberId": 1, "expensesCategory": "02.00.00.00 - LOREM IPSUM ", "currentIDB": "00.000.000,00", "disbursedToDate": "00.000.000,00", "disbursedxCent": "00,00", "balance": "000.000,00" },
			{ "numberId": 2, "expensesCategory": "03.00.00.00 - LOREM IPSUM ", "currentIDB": "0,00", "disbursedToDate": "0,00", "disbursedxCent": "0,00", "balance": "0,00" },
                        { "numberId": 3, "expensesCategory": "04.00.00.00 - LOREM IPSUM ", "currentIDB": "0.000,00", "disbursedToDate": "0.000,00", "disbursedxCent": "000,00", "balance": "000.000,00" },
                        { "numberId": 4, "expensesCategory": "05.00.00.00 - LOREM IPSUM ", "currentIDB": "0,00", "disbursedToDate": "0,00", "disbursedxCent": "0,00", "balance": "0,00" }];

    // variable datosHijo que va a tener la información de todos los datos "Hijo"
    // importante el "numberId" que relaciona los datos "Hijo" con los datos "Padre" correspondientes
    // el data es de tipo JSON
    var datosHijo = [{ "hijoId": 0, "numberId": 1, "td1hijo": "01.00.00.00 - LOREM IPSUM", "td2hijo": "0.000.000,00", "td3hijo": "0.000,00", "td4hijo": "00,00", "td5hijo": "000.000,00" },
					{ "hijoId": 1, "numberId": 1, "td1hijo": "02.00.00.00 - LOREM IPSUM", "td2hijo": "0.000,00", "td3hijo": "0.000,00", "td4hijo": "00,00", "td5hijo": "00.000,00" },
					{ "hijoId": 2, "numberId": 3, "td1hijo": "04.00.00.00 - LOREM IPSUM", "td2hijo": "0.000,00", "td3hijo": "0.000,00", "td4hijo": "000,00", "td5hijo": "00.000,00" },
					{ "hijoId": 3, "numberId": 3, "td1hijo": "05.00.00.00 - LOREM IPSUM", "td2hijo": "0.000,00", "td3hijo": "0.000,00", "td4hijo": "000,00", "td5hijo": "00.000,00" }];

    var datosNieto = [{ "hijoId": 3, "td1nieto": "07.00.00.00 - LOREM IPSUM", "td2nieto": "0.000.000,00", "td3nieto": "0.000,00", "td4nieto": "00,00", "td5nieto": "000.000,00" },
                                        { "hijoId": 3, "td1nieto": "08.00.00.00 - LOREM IPSUM", "td2nieto": "0.000.000,00", "td3nieto": "0.000,00", "td4nieto": "00,00", "td5nieto": "000.000,00" }];


    var datosP = new kendo.data.DataSource({
        data: datosPadre
    });
    // creación de la tabla			
    $(this.grid).kendoGrid({
        dataSource: datosP,
        selectable: instance.selectable,
        sortable: true,
        deletable: true,
        reorderable: false,
        scrollable: false,
        pageable: instance.pageable,
        columnMenu: true,
        detailInit: detailInit,
        dataBound: function () {
            this.expandRow(this.tbody.find("tr.k-master-row"));
            determinarNegrita();
        },
        columns: [
			{ field: "expensesCategory", title: "Expenses Category" },
            { field: "currentIDB", title: "Current IDB" },
            { field: "disbursedToDate", title: "Disbursed (to date)" },
            { field: "disbursedxCent", title: "% Disbursed" },
            { field: "balance", title: "Balance" }
        ]
    });


    function detailInit(e) {
        var datosH = new kendo.data.DataSource({
            data: datosHijo,
            filter: { field: "numberId", operator: "eq", value: e.data.numberId }
        });

        $("<div/>").appendTo(e.detailCell).kendoGrid({
            dataSource: datosH,
            detailInit: detailInitNietos,
            dataBound: function () {
                this.expandRow(this.tbody.find("tr.k-master-row"));
            },
            columns: [
				{ field: "td1hijo", title: "Expenses Category" },
				{ field: "td2hijo", title: "Current IDB" },
				{ field: "td3hijo", title: "Disbursed (to date)" },
				{ field: "td4hijo", title: "% Disbursed" },
				{ field: "td5hijo", title: "Balance" }
            ]
        });
    }

    function detailInitNietos(f) {
        var datosN = new kendo.data.DataSource({
            data: datosNieto,
            filter: { field: "hijoId", operator: "eq", value: f.data.hijoId }
        });

        // creación de la tabla			
        $("<div/>").appendTo(f.detailCell).kendoGrid({
            dataSource: datosN,
            columns: [
				{ field: "td1nieto", title: "Expenses Category" },
				{ field: "td2nieto", title: "Current IDB" },
				{ field: "td3nieto", title: "Disbursed (to date)" },
				{ field: "td4nieto", title: "% Disbursed" },
				{ field: "td5nieto", title: "Balance" }
            ]
        });
    }

    function determinarNegrita() {
        var b = $(".grid3").find(".k-detail-row").find(".k-grid").find(".k-grid-content").find("tbody:parent");
        for (var i = 0; i < b.length; i++) {
            $(b[i]).closest(".k-detail-row").prev(".k-master-row").addClass("mod_tabla_negrita");
        }
    }

    this._formatCells();

};

GridHierarchyComponent.prototype._formatCells = function () {
    $('.k-header-column-menu').hide();
    $('div.k-grid-header').removeAttr('style');
    $('span.k-pager-info').remove();
    $('div[data-role="pager"] > a[tabindex="-1"]').remove();
    $('table[role="treegrid"] .k-grid-header-wrap').hide();
};