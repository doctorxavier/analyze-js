/**
 * Filter object constructor.
 * @param {jquery object} filterBtn div containing the button managing the filter visibility.
 * @param {jquery object} element div containing the filter.
 * @returns {Filter}
 */
var Filter = function (filterBtn, element) {
    this.filterBtn = filterBtn;
    this.element = element;
    this._createFilter();
};

/**
 * Filter object creator.
 * Sets the click event on the button to switch the filter visibility.
 */
Filter.prototype._createFilter = function () {
    var instance = this;
    this.filterBtn.click(function () { instance.switchVisibility(instance.element); });
};

/**
 * Function that switches the visibility of the filter element.
 * @param {type} element
 */
Filter.prototype.switchVisibility = function () {
    var element = this.element;
    if ($(element).is(":visible")) { $(element).slideUp(); }
    else { $(element).slideDown(); }
};

/**
 * Function that checks if the Filter's element is visible.
 * @returns {boolean} true if the element is visible.
 */
Filter.prototype.isVisible = function () {
    return $(this.element).is(":visible");
};

/**
 * Function that sets visible the Filter's element.
 */
Filter.prototype.show = function () {
    $(this.element).slideDown();
};

/**
 * Function that sets invisible the Filter's element.
 */
Filter.prototype.hide = function () {
    $(this.element).slideUp();
};