(function($) {

    $(document).ready(function() {
        //KendoDropDownList
        $('.kendoDropDown').kendoDropDownList();

        //KendoDatePicker
        $('.kendoDatePicker').kendoDatePicker();

        $('#CountryDepartment').change(function () {
            var value = $(this).val();
            var url = $('#CountryDepartmentValues').data('route') + '/' + value;
            $('#CountryDepartmentValues').load(url, absoluteHeight);
        });

        $('#SectorDepartment').change(function () {
            var value = $(this).val();
            var url = $('#SectorDepartmentValues').data('route') + '/' + value;
            $('#SectorDepartmentValues').load(url, absoluteHeight);
        });
        
        setTimeout(absoluteHeight, 100);
    });

    function absoluteHeight() {
        $('.relative.font075em').each(function () {
            var holder = 0;

            $(this).find('.grayBorder').each(function () {
                if (holder < $(this).height())
                    holder = $(this).height();
            });
            $(this).find('.grayBorder').height(holder);
        });
    }

})(window.jQuery);