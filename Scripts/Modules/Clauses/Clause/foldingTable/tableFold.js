/****************************************************************
Class that manages the folding effects of the folding table.
Effects when folding/un-folding:
	- resize of the operatorBar.
	- folding effect of the grid and associated toolbar.
*****************************************************************/

/** Constructor
 * @param {jQuery object} table minimizeTable element where the folding table is located.
 * @returns {TableFold}
 */

var TableFold = function (table) {
    this.table = table;
    this.grid = $(table).find(".mod_tabla_plegable").find(".k-grid"); /* grid element. */
    this.operatorBar = $(table).find(".operatorBar"); /* operatorBar element */
    this.toolbar = $(table).find(".mod_tabla_plegable").find(".k-toolbar"); /* toolbar element of the table. */
    this.foldBtn = $(table).find(".tableOperator"); /* button that triggers the folding event. */
    this.gridHeight = $(this.grid).height(); /* Height of the grid element. */
    this.foldBtnTopRelPos = $(this.foldBtn).offset().top - $(this.table).offset().top; /* Relative top position of the foldBtn. */
    if ($(this.grid).offset()!=undefined)
        this.gridBottomRelPos = this.gridHeight + $(this.grid).offset().top - $(this.table).offset().top; /* relative bottom position of the grid. */
    this.createFold();
};

/**
 *  TableFold object creator.
 *  Calculates the object's variables on window resize.
 *  Sets the cick event on the fold button.
 */
TableFold.prototype.createFold = function(){   
    var instance = this;
    $(instance.operatorBar).height(instance.gridBottomRelPos);	/* operatorBar intial height. */
    
    /* Re-calculate variable values on window resize. */
    $(window).resize(function(){
        instance._varData();
        if($(instance.foldBtn).hasClass("active")){
        /* folded table */
            $(instance.operatorBar).height(instance.foldBtnTopRelPos);
        }else{
        /* unfolded table */
            $(instance.operatorBar).height(instance.gridBottomRelPos);
        }
    });

     /* Event listener on the folding button. */
     instance.foldBtn.click(function(){
         if($(instance.foldBtn).hasClass("active")){
             /* Unfold */
            instance.unfold();
         }else{
             /* Fold */
            instance.fold();         
         }
     });
}

/* Function that calculates the variables data. */
TableFold.prototype._varData = function(){
    this.gridHeight = $(this.grid).height();
    this.foldBtnTopRelPos = $(this.foldBtn).offset().top - $(this.table).offset().top;
    if ($(this.grid).offset() != undefined)
        this.gridBottomRelPos = this.gridHeight + $(this.grid).offset().top - $(this.table).offset().top;
}

/**
 * Function that manages the folding effect of the object.
 */
TableFold.prototype.fold = function () {
    $(this.toolbar).slideUp();
    $(this.grid).slideUp();
    $(this.operatorBar).height(this.foldBtnTopRelPos);
    $(this.foldBtn).addClass("active");
};

/**
 * Function that manages the unfolding effect of the object.
 */
TableFold.prototype.unfold = function () {
    var instance = this;
    $(this.toolbar).slideDown();
    $(this.grid).slideDown(function(){
    /* Re-calculate variables values in case window has been resized. */
       instance._varData();
       $(instance.operatorBar).height(instance.gridBottomRelPos).slideDown()
    });
    $(this.foldBtn).removeClass("active"); 
};

/**
 * Function that checks the visibility of the object's grid.
 * @returns {boolean} true if visible.
 */
TableFold.prototype.isVisible = function(){
    return $(this.grid).is(":visible");
};