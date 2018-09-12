var changesCount = 0;

$(document).ready(function () {

    $('#IsProgender').change(function () {
        $("#ProGenderWarningMsg").toggleClass("hidden");
    });

    $('#IsProethnicity').change(function () {
        $("#ProEthnicityWarningMsg").toggleClass("hidden");
    });

    $('.titlePrincipal').addClass('tipo1');

    $("#MeansOfVerification").change(function (e) {
        changesCount = 0;
        newValue = $(this).val();
        oldValue = $(this).next().val();

        if (newValue != oldValue)
            changesCount++;
    });

    $('[name="cancelIndicator"]').on('click', function (event) {
        event.preventDefault();
        window.history.back();
    });

    $('[name="saveIndicator"]').on('click', function (e) {
        e.preventDefault();

        var $this = $(this);
        var isTcmModule = $this.attr("data-sa-checkindicators-istcmmodule");
        var usedForStrategicAlignment = $this.attr('data-sa-checkindicators-usedforsa');
        var placeHolder = $this.attr('data-sa-checkindicators-placeholder');
        var checkSARmIndicatorsUrl = $this.attr('data-sa-checkindicators-url');

        if (isTcmModule.toLowerCase() === "false" &&
            usedForStrategicAlignment.toLowerCase() === "true" &&
            placeHolder != null &&
            checkSARmIndicatorsUrl != null &&
            $('[name="impactIndicator.IsDeactivated"]').is(':checked')) {

            checkRmIndicatorRelationsToSAClassifications(placeHolder, checkSARmIndicatorsUrl, function () {
                showWarningOrSave();
            });
        } else {
            showWarningOrSave();
        }
    });
});

function showWarningOrSave() {
    var intervalId = $("#hdnIntervalId").val();
    var accessByAdmin = $("#AccessedByAdministrator").val();
    var isThirdInterval = $("#IsThirdInterval").val();

    if ((intervalId === 3 && changesCount > 0) ||
        (accessByAdmin === 'True' &&
        isThirdInterval === 'True' && changesCount > 0)) {
        showSaveChangesWindow();
    } else {
        SaveChangesIndicator();
    }
}

function showSaveChangesWindow() {
    confirmAction($('#changesMatrixMessage').val())
        .done(function (value) {
            if (value)
                SaveChangesIndicator();
        });
}

function SaveChangesIndicator() {
    if ($('[name="saveIndicator"]').attr('data-sa-checkindicators-usedforsa').toLowerCase() === 'true')
        updateSAClassificationsFromIndicatorForm();

    $('#UpadteIndicatorForm').submit();
}
