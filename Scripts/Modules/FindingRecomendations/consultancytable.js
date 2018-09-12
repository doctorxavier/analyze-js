
$(document).ready(function () {

    var ConsultancyTableCtrl = new Object();

    ConsultancyTableCtrl.Table = $('#consultancyTable');
    ConsultancyTableCtrl.TableWrapper = $('#consultancyTable_wrapper');
    ConsultancyTableCtrl.PaginationBlock = ConsultancyTableCtrl.TableWrapper.find('.Pagination');
    ConsultancyTableCtrl.HideAllButton = $('[dd-contenedor=ConsultancyTable][data-expand-button=collapse]');
    ConsultancyTableCtrl.ShowAllButton = $('[dd-contenedor=ConsultancyTable][data-expand-button=expand]');

    ConsultancyTableCtrl.ShowAll = function () {
        ConsultancyTableCtrl.Table.find('tr.hide').addClass('showallhide').removeClass('hide');
        $('#consultancyTable_wrapper .Pagination').hide();
    };

    ConsultancyTableCtrl.HideAll = function () {
        ConsultancyTableCtrl.Table.find('tr.showallhide').addClass('hide').removeClass('showallhide');
        $('#consultancyTable_wrapper .Pagination').show();
    };

    ConsultancyTableCtrl.ShowAllButton.bind('click', function () {

        ConsultancyTableCtrl.ShowAll();
        ConsultancyTableCtrl.HideAllButton.show();
        ConsultancyTableCtrl.ShowAllButton.hide();
    });

    ConsultancyTableCtrl.HideAllButton.bind('click', function () {

        ConsultancyTableCtrl.HideAll();
        ConsultancyTableCtrl.ShowAllButton.show();
        ConsultancyTableCtrl.HideAllButton.hide();
    });

});