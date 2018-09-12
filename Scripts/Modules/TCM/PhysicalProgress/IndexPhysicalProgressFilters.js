function hideShowOutputDescription() {
    var btnReadShowOutputDescription = $('#btn-read-show-output-description');
    var allOutputDescriptionText = $('[id*=output-description-text]');

    if (!btnReadShowOutputDescription.hasClass("pressed") &&
        !btnReadShowOutputDescription.hasClass("expanded")) {
        btnReadShowOutputDescription.addClass("pressed expanded");

        allOutputDescriptionText.show();

        return;
    }

    btnReadShowOutputDescription.removeClass("pressed expanded");

    allOutputDescriptionText.hide();
};
