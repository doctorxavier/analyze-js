/**
 * Supervision plan Module
 * Wed, 18 december 2013
 * eSmart Group
 */

//remap jQuery to $
(function($) {

    $(document).ready(function() {
        //KendoDropDownList
        $('.kendoDropDown').kendoDropDownList();

        //KendoDatePicker
        $('.kendoDatePicker').kendoDatePicker();

        //New product ui_sp_001_edit
        $('#newProductUISP001Edit').click(function() {
            $.get('Scripts/modules/supervisionPlan/templates/newProductUISP001.html', function(data) {
                $('#productsUISP001Edit').append(data);
                $('.new').find('.kendoDropDown').kendoDropDownList();
                $('.new').removeClass('new');
            });
        });
        
        //Remove product ui_sp_001_edit
        $(document).on('mousedown', '#ui_sp_001_edit .deleteTextButton', function() {
            $(this).closest('div.row').remove();
        });

        //New activity ui_sp_002_edit
        $('#newActivityUISP002Edit').click(function() {
            $.get('Scripts/modules/supervisionPlan/templates/newActivityUISP002Edit.html', function(data) {
                $('#activitiesUISP002Edit').append(data);
                $('.new').find('.kendoDropDown').kendoDropDownList();
                $('.new').find('.kendoDatePicker').kendoDatePicker();
                $("#activitiesUISP002Edit").find('div.activityComponent').each(function(i) {
                    $(this).find('.componentNumber').text(i + 1);
                });
                $('.new').removeClass('new');
            });
        });

        //Remove rows from tables ui_sp_002_edit
        $(document).on('mousedown', '#ui_sp_002_edit .removeIcon', function() {
            $(this).closest('tr').remove();
        });

        //Remove activity ui_sp_002_edit
        $(document).on('mousedown', '#ui_sp_002_edit .deleteTextButton', function() {
            var parent = $(this).closest('div.activityComponent').parent('div');
            $(this).closest('div.activityComponent').remove();
            $(parent).find('div.activityComponent').each(function(i) {
                $(this).find('.componentNumber').text(i + 1);
            });
        });

        //New activity row ui_sp_002_edit
        $(document).on('mousedown', '#ui_sp_002_edit .addOne', function() {
            var container = $(this).closest('div.addOneWrapper').siblings('table').find('tbody');
            $.get('Scripts/modules/supervisionPlan/templates/newActivityRowUISP002Edit.html', function(data) {
                $(container).append(data);
                $('.new').find('.kendoDatePicker').kendoDatePicker();
                $('.new').removeClass('new');
            });
        });

        //Toggle activities containers ui_sp_002
        toggleActivities();

    });

    function toggleActivities() {
        $(document).on('mousedown', '.minMaxButton', function() {
            if ($(this).hasClass('plus')) {
                $(this).removeClass('plus');
                $(this).parent('div').parent('div').siblings().slideUp(100);
//                if ($(this).parent().siblings('div').length > 0) {
//                    $(this).parent().siblings('div').slideUp(0);
//                }
            } else {
                $(this).addClass('plus');
                $(this).parent('div').parent('div').siblings().slideDown(100);
//                if ($(this).parent().siblings('div').length > 0) {
//                    $(this).parent().siblings('div').slideDown(0);
//                }
            }
        });
    }

})(window.jQuery);