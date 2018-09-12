/* Function that creates the necessary TableFold objects. */
$(window).load(function () {
    /* Check the existance of minimizeTable elements. */
    var tables = []; /* Array containing all the TableFold objects. */
    
    if($(".minimizeTable").length > 0){
        /* Create a TableFold object for each element. */
        var minimizeTable = $(".minimizeTable");
        for(var i=0; i<$(minimizeTable).length;i++){
            tables.push(new TableFold(minimizeTable[i]));
        }
    }
    
    /* Check the existance of the mass-collapse button. */
    if($(".btn-action-group").length > 0){
        var collapseBtn = new BotonActivo($(".btn-action-group")); /* BotonActivo object from the mass-collapse button. */
        var filter = new Filter($(".filter-clauses"),$(".filter")); /* Filter Object. */
        /* Event on the BotonActivo object. */
        collapseBtn.btnInput.click(function(){ massCollapse(collapseBtn,tables); });

        /* Event on the Filter's visibility switch button. */
        //filter.filterBtn.click(function(){
        //    if (!filter.isVisible()) {
        //        filter.hide();
        //    } else {
        //        filter.show();
        //    }
        //});

        /* Event on each of the TableFold objects. */
        for(var i=0;i<tables.length;i++){
            tables[i].foldBtn.click(function(){
                /* Change collapsebtn state if needed. */
                //setTimeout(function(){
                    if(changeCollapseBtn(collapseBtn,tables,filter)){ collapseBtn.switchState(collapseBtn); }
                //},500);
            });
        }
    }

/**
 * Function that manages the collapse behaviour of all the elements in the page.
 * @param {BotonActivo} collapseBtn
 * @param {[FoldingTable]} tables
 * @param {Filter} filter
 */
    function massCollapse(collapseBtn, tables) {
        
    if(collapseBtn.isActive()){
        /* Un-collapse. */
        for(var i=0;i<tables.length;i++){ tables[i].unfold(); }
    }else{
        /* Collapse. */
        for(var i=0;i<tables.length;i++){ tables[i].fold(); }
    }
}

/**
 * Function that manages the botonActivo state deending on the visibility of the
 * other elements of the page.
 * @param {BotonActivo} collapseBtn
 * @param {[FoldingTable]} tables
 * @param {Filter} filter
 * @returns {Boolean} true if the BotonActivo's state needs to be changed.
 */
function changeCollapseBtn(collapseBtn,tables,filter){
    if(collapseBtn.isActive()){
        /* collapseBtn is active. */
        for (var i = 0; i < tables.length; i++) {
            if (tables[i].isVisible()) {
                /* table[i] is visible. */
                return false;
            }
        }
    }else{
        /* collapseBtn is inactive */
        for (var i = 0; i < tables.length; i++) {
            if (tables[i].isVisible()) {
                /* table[i] is visible. */
                return true;
            }
        }
    }
    

    }

});