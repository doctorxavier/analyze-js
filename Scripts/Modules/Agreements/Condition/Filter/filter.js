var Filter = function (filterBtn, element) {
    this.filterBtn = filterBtn;
    this.element = element;
    this._createFilter();
    var container = $('div.filter');
    var clauseType = container.find('button[id=id-ConditionType]').GetValue();
    var clauseStatus = container.find('button[id=id-FilterConditionStatus]').GetValue();
    var clauseCategory = container.find('button[id=id-ConditionCategory]').GetValue();
    var clauseNumber = container.find('#conditionNumberFilter').val();
    var expirationDateFrom = container.find('input[name="dateExpirationFrom"]').val();
    var expirationDateTo = container.find('input[name="dateExpirationTo"]').val();

    if ((clauseType != "") || (clauseStatus != "") || (clauseCategory != "") || (clauseNumber != "") ||
        (expirationDateFrom != "") || (expirationDateTo != "")) {
        $('.filter').slideDown();
    }
};

Filter.prototype._createFilter = function () {
    var instance = this;
    this.filterBtn.click(function () { instance.switchVisibility(instance.element); });
};

Filter.prototype.switchVisibility = function () {
    var element = this.element;
    var heightFrame = $('.mod_contenido_central').height();

    if ($(element).is(":visible")) {
        $(element).slideUp();
        $(window.parent.document).find('iframe').height(heightFrame + "px");
    }
    else {
        $(element).slideDown();
        var newHeightFrame = heightFrame + 500;
        $(window.parent.document).find('iframe').height(newHeightFrame + "px");
    }
};

Filter.prototype.isVisible = function () {
    return $(this.element).is(":visible");
};

Filter.prototype.show = function () {
    $(this.element).slideDown();
};

Filter.prototype.hide = function () {
    $(this.element).slideUp();
};