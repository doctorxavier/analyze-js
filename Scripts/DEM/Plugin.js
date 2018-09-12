function switchInputTextToLabel(tableId) {
    $('#' + tableId + ' > tbody > tr > td').find('input[type=checkbox]').change(function () {
        var inputText = $(this).closest('tr').find('span.showDataEdit input.inputText');
        var label = $(this).closest('tr').find('span.showDataEdit label.labelHide');

        if ($(this).is(":checked")) {
            inputText.removeClass("hide");
            label.addClass("hide");
        } else {
            var valInputText = inputText.val();
            label.val(valInputText);
            label.removeClass("hide");
            inputText.addClass("hide");
        }
    });
}
$(document).ready(function () {
    $('.DemCommentBox').find('.DemAddComments').click(function () {
        $(this).parent().parent().find('.DemInsert').slideDown('slow');
    });
    $('.buttonTrash').bind({
        click: function () {
            $(this).parent().find('textarea').val('');
            $(this).parent().slideUp('slow').css('display', 'none');
        }
    })
})
