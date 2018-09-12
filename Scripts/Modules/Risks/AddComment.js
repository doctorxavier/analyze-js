
$(document).on("ready", function ()
{

    Date.prototype.getMonthName = function () {
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                      "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
        return monthNames[this.getMonth()];
    }

    $('#newCommentRisks').click(function () {

        var month_Name = new Date().getMonthName();
        var date = new Date()
        var month_Name = date.getMonthName();
        var day = date.getDate();
        var year = date.getUTCFullYear().toString().substring(0, 4);
        var CurrentUser = $("#UserName").attr("value"); 
        var htmlContent = '';

        var ope = $('table#commentTableUIRI001_edit tbody tr:last').index() + 1;
        htmlContent += '<tr class="new">';
        htmlContent += '<td>';
        htmlContent += '<div class="padding10 leftAlign">';
        htmlContent += '<div class="verticalMargin10 w100 relative h50px" style="height:0%;">';
        htmlContent += '<input type="button" value="Delete" class="deleteTextButton verticalAlignTop absolute absRight">';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100">';
        htmlContent += '<div class="verticalMargin10 inline-block">' + day + " " + month_Name + " " + year + '</div>';
        htmlContent += '<div class="verticalMargin10 inline-block bold marginLeft3">' + CurrentUser + '</div>';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100 textAreaContainer">';
        htmlContent += '<input data-val="true" data-val-user-required="The UserCommentId field is required." id="UserComment_' + ope + '__UserCommentId" name="UserComment[' + ope + '].UserCommentId" type="hidden" value="-1">';
        htmlContent += '<input id="UserComment_' + ope + '__Text" name="UserComment[' + ope + '].Text" type="hidden" data-val="true" data-val-user-required="The Text field is required." value="", maxlength="500" >';
        htmlContent += '<textarea class="txtDescriptionComent input-validation-error" id="UserComment_' + ope + '__Text" name="UserComment[' + ope + '].Text" data-val="true" data-val-required="The Text field is required." , maxlength="500"></textarea>';

        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</td>';
        htmlContent += '</tr>';

        $('#commentTableUIRI001_edit').append(htmlContent);
        var ddd = htmlContent;

        $('#commentTableUIRI001_edit').find("textarea").last().focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        resizeIframeCloud();
    });







});
