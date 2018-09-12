$(document).ready(function () {
    $("#total-months").text(monthDiff());
    $('#clause-months-left').text(calculateMonthsLeft());

    $('.FulfilmentLD').on('click', function () {
        confirmAction('This action is cannot be undone, are you sure do you want to continue?')
            .done(function (value) {
                if (value) {
                    redirectPage($('#EditClauseToFulfilment').val());
                }
            });
    });
});

function monthDiff() {
    var originalDate = new Date($('#original-date').text());
    var currentDate = new Date($('#current-date').text());

    var diff = currentDate.getMonth() - originalDate.getMonth() +
        (12 * (currentDate.getFullYear() - originalDate.getFullYear()));

    return diff ? diff : '--';
}

function calculateMonthsLeft() {
    var numberApprovedExt = parseInt($('#clause-approved-extensions').text());
    var approvalDate = new Date($('#contract-approval-date').val());
    var isValidApprovalDate =
        Object.prototype.toString.call(approvalDate) === "[object Date]" && !isNaN(approvalDate.getTime());

    if (!isNaN(numberApprovedExt) && isValidApprovalDate && approvalDate < new Date(2012, 1, 1)) {
        switch (numberApprovedExt) {
            case 0:
                return '30';
            case 1:
                return '24';
            case 2:
                return '12';
            case 3:
                return '0';
        }
    }
    else {
        switch (numberApprovedExt) {
            case 0:
                return '24';
            case 1:
                return '12';
            case 2:
                return '0';
        } 
    }

    return '--';
}
