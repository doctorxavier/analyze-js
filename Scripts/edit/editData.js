


	var hideAndShowContainer = function(){



		if($(".modEditingBasicData").length > 0){

			$('.editingBox').hide();
			$('.pie.bottom').hide();
			$('.buscadorConvergence').hide();
			$('.table_search_item').hide();
			$('.editingButtons').hide();
			$('.cancelConfirm').hide();
			$('.pie.top').hide();

			$('.edit').click(function(){
				$('.editableBox').hide();
				$('.editableButtons').hide();
				$('.editingBox').show();
				$('.editingButtons').show();
				$('.editingButtonsEdit').hide();
			});

			$('.cancel').click(function(){
				$('.editingBox').hide();
				$('.editingButtons').hide();
				$('.editableBox').show();
				$('.editableButtons').show();
				$('.editingButtonsEdit').show();
			});

			$('.save').click(function(){
				$('.editingBox').hide();
				$('.editingButtons').hide();
				$('.editableBox').show();
				$('.editableButtons').show();
				$('.editingButtonsEdit').show();
			});

		    //---------------------------------------------
			$('.newIndicator').click(function () {
			    $('.table_search_item').hide();
			    $('.buscadorConvergence').show();
			})

			$('.botones .searchItem').click(function () {
			    $('.table_search_item').show();
			    $('.pie.bottom').show();
			})

			$('.k-window-actions').click(function () {
			    $('.buscadorConvergence').hide();
			    $('.table_search_item').hide();
			    $('.pie.bottom').hide();
			    $('.pie.top').hide();
			    $('.cancelConfirm').hide();
			    $('.mod_tabla.mod_tabla_agrup').show();
			})

			$('.closeSearch').click(function () {
			    $('.pie.bottom').hide();
			    $('.table_search_item').hide();
			    $('.buscadorConvergence').hide();
			    $('.pie.top').hide();
			});

			$('.btn_delete').click(function () {
			    $('.mod_tabla.mod_tabla_agrup').hide();
			    $('.pie.bottom').hide();
			    $('.table_search_item').hide();
			    $('.buscadorConvergence').hide();
			    $('.cancelConfirm').show();
			    $('.pie.top').show();
			});

			$('.closeConfirm').click(function () {
			    $('.cancelConfirm').hide();
			    $('.pie.top').hide();
			    $('.mod_tabla.mod_tabla_agrup').show();
			    $('.pie.bottom').show();
			    $('.table_search_item').show();
			    $('.buscadorConvergence').show();
			});
		}
	};







