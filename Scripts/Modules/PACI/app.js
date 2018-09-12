function showError(message, duration) {
    errorBar(message, duration);   
}

function confirmAction(message) {
    var promise = $.Deferred();
    showError(message);
    return promise;
}