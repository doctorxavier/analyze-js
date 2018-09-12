var PagePaginationAjax = 1;

$.fn.paginationMyDocumentAjax = function (rows, pageSize, width, functionFilter) {

    $(this).after('<div class="Pagination" dd-table="' + $(this).attr("id") + '" dd-row="' + pageSize + '"></div>');
    var totalRows = Math.ceil((rows / pageSize));

    if (totalRows > 0) {
        var paginator = $(document).find('div.Pagination[dd-table="' + $(this).attr("id") + '"]');
        if (width !== undefined) {
            $(paginator).css("min-width", width);
            $(paginator).css("width", width);
            $(paginator).css("max-width", width);
        } else {
            $(paginator).css("min-width", $(this).width());
            $(paginator).css("width", $(this).width());
            $(paginator).css("max-width", $(this).width());
        }

        paginator.append('<div dd-num="1" onclick="PaginationMove(this)" class="Pagination_Text Pagination_Prev">Prev</div>');

        if (totalRows > 5) {
            paginator.append('<div dd-num="1" class="Pagination_Text Pagination_Points hide">...</div>');
        }
        for (var i = 1; i < totalRows + 1 ; i++) {
            if (i === 1) {
                paginator.append('<div dd-num="' + i + '" class="Pagination_Number Pagination_Active">' + i + '</div>');
            }
            else {
                if (i > 5) {
                    paginator.append('<div dd-num="' + i + '"  class="Pagination_Number hide">' + i + '</div>');
                } else {
                    paginator.append('<div dd-num="' + i + '"  class="Pagination_Number">' + i + '</div>');
                }

            }
        }

        if (totalRows > 5) {
            paginator.append('<div dd-num="' + totalRows + '" class="Pagination_Text Pagination_Points">...</div>');
        }

        paginator.append('<div onclick="PaginationMove(this)" dd-num="' + totalRows + '" class="Pagination_Text Pagination_Next">Next</div>');
    }
    /* Paginacion Funcionamiento */

    $(".Pagination_Number").click(function () {
        if (!$(this).is(".Pagination_Active")) {
            $(this).closest(".Pagination").find("div").removeClass("Pagination_Active");
            $(this).addClass("Pagination_Active");
            PagePaginationAjax = ($(this).html() * 1);
            functionFilter();
            var totalIndexes = $(this).closest(".Pagination").find('.Pagination_Number').length;

            if (totalIndexes > 5) {
                $(this).closest(".Pagination").find('.Pagination_Number[dd-num]').addClass('hide');
                $(this).closest(".Pagination").find('.Pagination_Points').removeClass('hide');
                var indexInterno = $(this).attr('dd-num') * 1;
                if ($(this).text() * 1 < 3) {
                    $(this).closest(".Pagination").find('.Pagination_Points').first().addClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="1"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="2"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="3"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="4"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="5"]').removeClass('hide');
                } else if ($(this).text() * 1 > ($(this).closest(".Pagination").find('.Pagination_Number').length - 4)) {
                    $(this).closest(".Pagination").find('.Pagination_Points').last().addClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 1) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 2) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 3) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (totalIndexes - 4) + '"]').removeClass('hide');
                } else {
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno - 1) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + indexInterno + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 1) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 2) + '"]').removeClass('hide');
                    $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + (indexInterno + 3) + '"]').removeClass('hide');
                }
            }
        }
    });

    $(".Pagination_Text").click(function () {
        if ($(this).is('.Pagination_Points')) {
            $(this).closest(".Pagination").find('.Pagination_Number[dd-num="' + $(this).attr('dd-num') + '"]').click();
        }
    });

    $(".Pagination_Text").hover(function () {
        var table = "#" + $(this).closest(".Pagination").attr("dd-table");
        if ($(this).attr('dd-num') === $(this).closest('.Pagination').find('.Pagination_Active').attr('dd-num')) {
            $(this).addClass('NoActive');
        } else {
            $(this).removeClass('NoActive');
        }

    });


    return $(this);
};