$(document).ready(function () {
    if (typeof window.parent.resizeIframeTasks === 'function') {
        var partialHeight = $(document).height();
        window.parent.resizeIframeTasks(partialHeight);
    }
});