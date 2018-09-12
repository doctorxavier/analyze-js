function showError(message) {
    errorBar(message);
}

$(document).off('click', '[dd-exp="Expand All"]');
$(document).on('click', '[dd-exp="Expand All"]', function () {
    if ($('.tableNormal tbody tr[data-id] .buttonShowRow').hasClass('active')) {
        $('.tableNormal tbody tr[data-id] .buttonShowRow.active').click();
    }
});