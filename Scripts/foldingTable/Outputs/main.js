/* Function that creates the necessary TableFold objects. */
$(document).ready(function() {
    var collapseBtnLoans = "Collapse Loans"; /* Default text in the HTML file of the collapse loans button */
    var collapseBtnImpacts = "Collapse Impacts"; /* Default text in the HTML file of the collapse impacts button */
    var collapseBtnOutputs = "Collapse Components";
    var collapseBtnInactiveText;
    var foldingElements = [];
    /* Check the existance of minimizeTable elements. */
    var tables = []; /* Array containing all the TableFold objects. */
    if($(".minimizeTable").length > 0){
        /* Create a TableFold object for each element. */
        var minimizeTable = $(".minimizeTable");
        for(var i=0; i<$(minimizeTable).length;i++){
            var table = tables.push(new TableFold(minimizeTable[i]));
        }
    }
    
    /* Check the existance of the mass-collapse button. */
    if($(".btn-action-group").length > 0){
        var collapseBtn = new BotonActivo($(".btn-action-group")); /* BotonActivo object from the mass-collapse button. */
        /* Check which button is beeing created. */
        var collapseBtnInputText = collapseBtn.getBtnInputText();
        if(collapseBtnInputText.toLowerCase() === collapseBtnLoans.toLowerCase()){
            /* Collapse loans button. */
            collapseBtn.setActiveText(collapseBtnLoans);
            collapseBtnInactiveText = "Expand Loans";
            collapseBtn.setInactiveText(collapseBtnInactiveText);
        }else if(collapseBtnInputText.toLowerCase() === collapseBtnImpacts.toLowerCase()){
            /* Collapse impacts button */
            collapseBtn.setActiveText(collapseBtnImpacts);
            collapseBtnInactiveText = "Expand Impacts";
            collapseBtn.setInactiveText(collapseBtnInactiveText);
        }else if(collapseBtnInputText.toLowerCase() === collapseBtnOutputs.toLowerCase()){
            /* Collapse impacts button */
            collapseBtn.setActiveText(collapseBtnOutputs);
            collapseBtnInactiveText = "Expand Components";
            collapseBtn.setInactiveText(collapseBtnInactiveText);
        }
        try{
            var filter = new Filter($(".filter-clauses"),$(".filter")); /* Filter Object. */
        }catch(e){}
        /* Event on the BotonActivo object. */
        try{
            collapseBtn.btnInput.click(function(){ massCollapse(collapseBtn,{tables:tables,filter:filter}); });      
        }catch(e){
            collapseBtn.btnInput.click(function(){ massCollapse(collapseBtn,{tables:tables}); });
        }

        /* Event on the Filter's visibility switch button. */
        try{
            filter.filterBtn.click(function(){
                /* Change collapsebtn state if needed. */
                setTimeout(function(){
                    if(changeCollapseBtn(collapseBtn,{tables:tables,filter:filter})){ collapseBtn.switchState(collapseBtn); }
                },500);
            });
        }catch(e){}

        /* Event on each of the TableFold objects. */
        for(var i=0;i<tables.length;i++){
            tables[i].foldBtn.click(function(){
                /* Change collapsebtn state if needed. */
                setTimeout(function(){
                    try{
                        if(changeCollapseBtn(collapseBtn,{tables:tables,filter:filter})){ collapseBtn.switchState(collapseBtn); }
                    }catch(e){
                        if(changeCollapseBtn(collapseBtn,{tables:tables})){ collapseBtn.switchState(collapseBtn); }
                    }
                },500);
            });
        }
    }

    /**
     * Function that manages the collapse behaviour of all the elements in the page.
     * @param {BotonActivo} collapseBtn
     * @param {[FoldingTable]} tables
     * @param {Filter} filter
     */
    function massCollapse(collapseBtn,options){
        var options = options || {};
        var tables = options.tables || null;
        var filter = options.filter || null;
        if(collapseBtn.isActive()){
            /* Un-collapse. */
            if(filter !== null){ filter.unfold(); }
            if(tables !== null){ for(var i=0;i<tables.length;i++){ tables[i].unfold(); } }
        }else{
            /* Collapse. */
            if(filter !== null){ filter.fold(); }
            if(tables !== null){ for(var i=0;i<tables.length;i++){ tables[i].fold(); } }
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
    function changeCollapseBtn(collapseBtn,options){
        var options = options || {};
        var filter = options.filter || null;
        var tables = options.tables || null;

        if(filter !== null && tables !== null){
            if(collapseBtn.isActive()){
                /* collapseBtn is active. */
                if(!filter.isVisible()){
                    /* filter is not visible. */
                    for(var i=0;i<tables.length;i++){
                        if(tables[i].isVisible()){
                            /* table[i] is visible. */
                            return false;
                        }
                    }
                    /* All tables hidden. */
                    return true;
                }else{
                    /* filter visible. */
                    return false;
                }
            }else{
                /* collapseBtn is inactive */
                if(!filter.isVisible()){
                    /* filter is not visible. */
                    for(var i=0;i<tables.length;i++){
                        if(tables[i].isVisible()){
                            /* table[i] is visible. */
                            return true;
                        }
                    }
                    return false;
                }else{
                    return true;
                }
            }
        }else if(tables !== null){
            if(collapseBtn.isActive()){
                /* collapseBtn is active. */
                for(var i=0;i<tables.length;i++){
                    if(tables[i].isVisible()){
                       /* table[i] is visible. */
                       return false;
                    }
                }
                /* All tables hidden. */
                return true;
            }else{
                /* collapseBtn is inactive */
                for(var i=0;i<tables.length;i++){
                    if(tables[i].isVisible()){
                        /* table[i] is visible. */
                        return true;
                    }
                }
                return false;
            }
        }else if(filter !== null){
            if(collapseBtn.isActive()){
                /* collapseBtn is active. */
                if(!filter.isVisible()){
                    /* filter is not visible. */
                    return true;
                }else{
                    /* filter visible. */
                    return false;
                }
            }else{
                /* collapseBtn is inactive */
                if(!filter.isVisible()){
                    /* filter is not visible. */
                    return false;
                }else{
                    return true;
                }
            }
        }

    }

});