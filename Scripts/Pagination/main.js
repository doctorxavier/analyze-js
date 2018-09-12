Handlebars.registerHelper('paintPagingFooter', function (BaseURL, CurrentPage, HasNextPage, HasPrevPage, QryStrID, QryStrType, TotalPages, TotalRecords) {
    var htmlContent = '';

    htmlContent+= '<table border="0" style="width:100%"><tbody><tr><td style="text-align:left">';

    htmlContent += '<div class="pagingFooterLeft">';
    htmlContent += TotalRecords;
    htmlContent += (TotalRecords > 1) ? ' Records in ' : ' Record in ';
    htmlContent += TotalPages;
    htmlContent += (TotalPages > 1) ? ' pages' : ' page';
    htmlContent += '</div>';

    htmlContent += '</td>';

    htmlContent += '<td style="text-align: right">';

    htmlContent += '<div class="pagingFooterRight">';

    for (var i = 1; i <= TotalPages; i++) {
        if (CurrentPage == i) {
            htmlContent += '<span class="page-number currentPage">' + i + '</span>';
        } else {
            htmlContent += '<a class="gridPageNumber" title="' + i + '" href="' + BaseURL + '">';
            htmlContent += '<span class="page-number">'+i+'</span>';
            htmlContent += '</a>';
        }
    }

    htmlContent += '</div>';

    htmlContent += '</td></tr></tbody></table>';

    return htmlContent;
});

Handlebars.registerHelper('compare', function (lvalue, rvalue, options) {

    if (arguments.length < 3)
        throw new Error("Handlerbars Helper 'compare' needs 2 parameters");

    operator = options.hash.operator || "==";

    var operators = {
        '==': function (l, r) { return l == r; },
        '===': function (l, r) { return l === r; },
        '!=': function (l, r) { return l != r; },
        '<': function (l, r) { return l < r; },
        '>': function (l, r) { return l > r; },
        '<=': function (l, r) { return l <= r; },
        '>=': function (l, r) { return l >= r; },
        'typeof': function (l, r) { return typeof l == r; }
    }

    if (!operators[operator])
        throw new Error("Handlerbars Helper 'compare' doesn't know the operator " + operator);

    var result = operators[operator](lvalue, rvalue);

    if (result) {
        return options.fn(this);
    } else {
        return options.inverse(this);
    }

});

Handlebars.registerHelper('cutString', function (string, limit) {
    if (string != null) {
        if (string.length >= limit) {
            var tempString = string.substring(0, limit) + "<div class='commentsIcon' style='cursor:pointer' title='" + string + "'></div>";
            return tempString;
        }
    }
    return string;
});



function updateGridRowsContent(templateJSId, gridContentSelector, updatedData)
{
    // Load persons grid template
    var fuente = $('#' + templateJSId).html();
    // Compile template
    var plantilla = Handlebars.compile(fuente);
    // Get filled template
    var html = plantilla(updatedData);

    $("#" + gridContentSelector).find("tr").remove();

    $('#' + gridContentSelector).append(html);
}

function updatePagingFooter(pageSettings)
{
    // Load pagingfooter template
    var fuente = $('#pagingFooter-template').html();
    // Compile template
    var plantilla = Handlebars.compile(fuente);
    // Get filled template content
    var html = plantilla(pageSettings);
    // Update Paging Content
    $("#customPaginator").html(html);
}

