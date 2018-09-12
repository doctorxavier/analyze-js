$(document).ready(function () {
    //Remove tr ui_ri_003_edit comment table Clause 
    $(document).on('mousedown', '.deleteTextButton', function () {
        var parent = $(this).closest('table');

        $(this).closest('tr').remove();

        var tabla = $(parent).attr('id');
        ReordenarComentarios(tabla);
    });

//Reordenamiento cuando elimina comentarios Clause
    function ReordenarComentarios(Commenttable) {
        var ope = $('table#' + Commenttable + ' tbody tr:last').index();
        var Comments = $('table#' + Commenttable + ' tbody');
        var rowCount = 0;
        
        $(Comments).find('tr').each(function (index, tr) {

            var CommentsId = $(this).find('[id$="__UserCommentId"]');
            var Modifiedby = $(this).find('[id$="__ModifiedBy"]');
            var Modified = $(this).find('[id$="__Modified"]');

            var Created = $(this).find('[id$="__Created"]');
            var CreatedBy = $(this).find('[id$="__CreatedBy"]');

            var Text = $(this).find('[id$="__Text"]');

            if ($(CommentsId).val() != null) {
                $(CommentsId).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__UserCommentId");
                $(CommentsId).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].UserCommentId");
            }

            if ($(Modifiedby).val() != null) {
                $(Modifiedby).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Modifiedby).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Created).val() != null) {
                $(Created).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Created).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(CreatedBy).val() != null) {
                $(CreatedBy).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__ModifiedBy");
                $(CreatedBy).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Modified).val() != null) {
                $(Modified).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__Modified");
                $(Modified).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].Modified");
            }

            if ($(Text).val() != null) {
                $(Text).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + rowCount + "__Text");
                $(Text).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[" + rowCount + "].Text");
            }

            rowCount++;
        });
        resizeIframeCloud();
    }

    //Remove tr ui_ri_003_edit comment table Extension
    $(document).on('mousedown', '.deleteTextButtonExt', function () {
        var parent = $(this).closest('table');
        $(this).closest('tr').remove();

        var tabla = $(parent).attr('id');
        ReordenarComentariosExtension(tabla);
    });

    //Reordenamiento cuando elimina comentarios Extension
    function ReordenarComentariosExtension(Commenttable)
    {
        var ope = $('table#' + Commenttable + ' tbody tr:last').index();
        var Comments = $('table#' + Commenttable + ' tbody');
        var rowCount = 0;

        $(Comments).find('tr').each(function (index, tr) {

            var CommentsId = $(this).find('[id$="__UserCommentId"]');
            var Modifiedby = $(this).find('[id$="__ModifiedBy"]');
            var Modified = $(this).find('[id$="__Modified"]');

            var Created = $(this).find('[id$="__Created"]');
            var CreatedBy = $(this).find('[id$="__CreatedBy"]');

            var Text = $(this).find('[id$="__Text"]');

            if ($(CommentsId).val() != null) {
                $(CommentsId).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__UserCommentId");
                $(CommentsId).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].UserCommentId");
            }

            if ($(Modifiedby).val() != null) {
                $(Modifiedby).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Modifiedby).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Created).val() != null) {
                $(Created).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Created).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(CreatedBy).val() != null) {
                $(CreatedBy).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__ModifiedBy");
                $(CreatedBy).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].ModifiedBy");
            }


            if ($(Modified).val() != null) {
                $(Modified).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__Modified");
                $(Modified).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].Modified");
            }

            if ($(Text).val() != null) {
                $(Text).attr("id", "Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + rowCount + "__Text");
                $(Text).attr("name", "Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[" + rowCount + "].Text");
            }

            rowCount++;
        });

        resizeIframeCloud();
    }

    //Remove tr ui_ri_003_edit comment table Revolving
    $(document).on('mousedown', '.deleteTextButtonRev', function () {
        var parent = $(this).closest('table');
        $(this).closest('tr').remove();

        var tabla = $(parent).attr('id');
        ReordenarComentariosRevolving(tabla);
    });

    //Reordenamiento cuando elimina comentarios Revolving
    function ReordenarComentariosRevolving(Commenttable) {
        var ope = $('table#' + Commenttable + ' tbody tr:last').index();
        var Comments = $('table#' + Commenttable + ' tbody');
        var rowCount = 0;

        $(Comments).find('tr').each(function (index, tr) {

            var CommentsId = $(this).find('[id$="__UserCommentId"]');
            var Modifiedby = $(this).find('[id$="__ModifiedBy"]');
            var Modified = $(this).find('[id$="__Modified"]');

            var Created = $(this).find('[id$="__Created"]');
            var CreatedBy = $(this).find('[id$="__CreatedBy"]');

            var Text = $(this).find('[id$="__Text"]');

            if ($(CommentsId).val() != null) {
                $(CommentsId).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__UserCommentId");
                $(CommentsId).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].UserCommentId");
            }

            if ($(Modifiedby).val() != null) {
                $(Modifiedby).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Modifiedby).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Created).val() != null) {
                $(Created).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Created).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].ModifiedBy");
            }


            if ($(CreatedBy).val() != null) {
                $(CreatedBy).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__ModifiedBy");
                $(CreatedBy).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Modified).val() != null) {
                $(Modified).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__Modified");
                $(Modified).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].Modified");
            }

            if ($(Text).val() != null) {
                $(Text).attr("id", "Contracts_0__RevolvingFund_0__UserComments_" + rowCount + "__Text");
                $(Text).attr("name", "Contracts[0].RevolvingFund[0].UserComments[" + rowCount + "].Text");
            }

            rowCount++;
            resizeIframeCloud();
        });

    }

    //Remove tr ui_ri_003_edit comment table Contracts(Elegibility Date)
    $(document).on('mousedown', '.deleteTextButtonCon', function () {
        var parent = $(this).closest('table');
        $(this).closest('tr').remove();

        var tabla = $(parent).attr('id');
        ReordenarComentariosContractos(tabla);
    });

    //Reordenamiento cuando elimina comentarios eligibility
    function ReordenarComentariosContractos(Commenttable) {
        var ope = $('table#' + Commenttable + ' tbody tr:last').index();
        var Comments = $('table#' + Commenttable + ' tbody');
        var rowCount = 0;        

        $(Comments).find('tr').each(function (index, tr) {

            var CommentsId = $(this).find('[id$="__UserCommentId"]');
            var Modifiedby = $(this).find('[id$="__ModifiedBy"]');
            var Modified = $(this).find('[id$="__Modified"]');

            var Created = $(this).find('[id$="__Created"]');
            var CreatedBy = $(this).find('[id$="__CreatedBy"]');

            var Text = $(this).find('[id$="__Text"]');

            if ($(CommentsId).val() != null) {
                $(CommentsId).attr("id", "Contracts_0__UserComments_" + rowCount + "__UserCommentId");
                $(CommentsId).attr("name", "Contracts[0].UserComments[" + rowCount + "].UserCommentId");
            }

            if ($(Modifiedby).val() != null) {
                $(Modifiedby).attr("id", "Contracts_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Modifiedby).attr("name", "Contracts[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Created).val() != null) {
                $(Created).attr("id", "Contracts_0__UserComments_" + rowCount + "__ModifiedBy");
                $(Created).attr("name", "Contracts[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(CreatedBy).val() != null) {
                $(CreatedBy).attr("id", "Contracts_0__UserComments_" + rowCount + "__ModifiedBy");
                $(CreatedBy).attr("name", "Contracts[0].UserComments[" + rowCount + "].ModifiedBy");
            }

            if ($(Modified).val() != null) {
                $(Modified).attr("id", "Contracts_0__UserComments_" + rowCount + "__Modified");
                $(Modified).attr("name", "Contracts[0].UserComments[" + rowCount + "].Modified");
            }

            if ($(Text).val() != null) {
                $(Text).attr("id", "Contracts_0__UserComments_" + rowCount + "__Text");
                $(Text).attr("name", "Contracts[0].UserComments[" + rowCount + "].Text");
            }

            rowCount++;
        });

        resizeIframeCloud();

    }

    Date.prototype.getMonthName = function () {
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                      "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
        return monthNames[this.getMonth()];
    }

    $(".txtDescriptionComent").focusout(function () {
        var divParent = $(this).parent();
        var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
        $(inputImpactIndicator).attr("value", $(this).val());
    });

    $('#newCommentRisks002').click(function () {
        var UserName = $('#UserName').attr("value");
        var month_Name = new Date().getMonthName();
        var date = new Date()
        var month_Name = date.getMonthName();
        var day = date.getDate();
        var year = date.getUTCFullYear().toString().substring(2, 4);
        var htmlContent = '';
        var ope = $('table#commentTableUIRI002_edit tbody tr:last').index() + 1;
        htmlContent += '<tr class="new">';
        htmlContent += '<td>';
        htmlContent += '<div class="padding10 leftAlign">';
        htmlContent += '<div class="verticalMargin10 w100 relative h50px" style="height:0%;">';
        htmlContent += '<input type="button" value="Delete" class="deleteTextButton verticalAlignTop absolute absRight">';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100">';
        htmlContent += '<div class="verticalMargin10 inline-block" style="margin-right: 1%;">' + day + " " + month_Name + " " + year + '</div>';
        htmlContent += '<div class="verticalMargin10 inline-block bold marginLeft3">' + UserName + '</div>';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100 textAreaContainer">';
        htmlContent += '<div class="padding10">';

        htmlContent += '<input data-val="true" data-val-user-required="The UserCommentId field is required." id="Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments__' + ope + '__UserCommentId" name="Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[' + ope + '].UserCommentId" type="hidden" value="-1">';
        htmlContent += '<input id="Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_' + ope + '__Text" name="Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[' + ope + '].Text" type="hidden" data-val="true" data-val-user-required="Please, complete the mandatory fields" value="">';
        htmlContent += '<textarea class="txtDescriptionComent" id="Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_' + ope + '__Text" name="Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments[' + ope + '].Text" data-val="true" data-val-required="The Text field is required." rows="5" maxlength="1000" ></textarea>';
        
        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</td>';
        htmlContent += '</tr>';
        $('#commentTableUIRI002_edit').append(htmlContent);
        var ddd = htmlContent;

        

        $("#Contracts_0__Clauses_0__ClauseIndividuals_0__UserComments_" + ope + "__Text").focus();

        $('#commentTableUIRI002_edit').find("textarea").last().focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        resizeIframeCloud();

    });

    $('#newCommentClauseExtensionApproval002').click(function () {

        var month_Name = new Date().getMonthName();
        var date = new Date()
        var month_Name = date.getMonthName();
        var day = date.getDate();
        var year = date.getUTCFullYear().toString().substring(2, 4);
        var htmlContent = '';
        var CurrentUser = $("#UserName").attr("value");


        var ope = $('table#commentTableUIRI002_edit tbody tr:last').index() + 1;
        htmlContent += '<tr class="new">';
        htmlContent += '<td>';
        htmlContent += '<div class="padding10 leftAlign">';
        htmlContent += '<div class="verticalMargin10 w100 relative h50px" style="height:0%;">';
        htmlContent += '<input type="button" value="Delete" class="deleteTextButtonExt verticalAlignTop absolute absRight">';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100">';
        htmlContent += '<div class="verticalMargin10 inline-block" style="margin-right: 1%;">' + day + " " + month_Name + " " + year + '</div>';
        htmlContent += '<div class="verticalMargin10 inline-block bold marginLeft3">' + CurrentUser + ' </div>';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100 textAreaContainer">';
        htmlContent += '<input data-val="true" data-val-user-required="The UserCommentId field is required." id="Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments__' + ope + '__UserCommentId" name="Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[' + ope + '].UserCommentId" type="hidden" value="-1">';
        htmlContent += '<input id="Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_' + ope + '__Text" name="Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[' + ope + '].Text" type="hidden" data-val="true" data-val-user-required="Please, complete the mandatory fields" value="">';
        htmlContent += '<textarea class="txtDescriptionComent" id="Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_' + ope + '__Text" name="Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments[' + ope + '].Text" data-val="true" data-val-required="The Text field is required." rows="5"  maxlength="1000"></textarea>';

        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</td>';
        htmlContent += '</tr>';
        $('#commentTableUIRI002_edit').append(htmlContent);
        var ddd = htmlContent;
        

        $("#Contracts_0__Clauses_0__ClauseIndividuals_0__ClauseExtension_0__UserComments_" + ope + "__Text").focus();

        $('#commentTableUIRI002_edit').find("textarea").last().focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        resizeIframeCloud();

    });

    $('#newCommentContractAprprovalRevFund002').click(function () {

        var month_Name = new Date().getMonthName();
        var date = new Date()
        var month_Name = date.getMonthName();
        var day = date.getDate();
        var year = date.getUTCFullYear().toString().substring(0, 4);
        var CurrentUser = $("#UserName").attr("value");
        var htmlContent = '';

        var ope = $('table#commentTableUIRI002_edit tbody tr:last').index() + 1;
        htmlContent += '<tr class="new">';
        htmlContent += '<td>';
        htmlContent += '<div class="padding10 leftAlign">';
        htmlContent += '<div class="verticalMargin10 w100 relative h50px" style="height:0%;">';
        htmlContent += '<input type="button" value="Delete" class="deleteTextButton verticalAlignTop absolute absRight">';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100">';
        htmlContent += '<div class="verticalMargin10 inline-block" style="margin-right: 1%;">' + day + " " + month_Name + " " + year + '</div>';
        htmlContent += '<div class="verticalMargin10 inline-block bold marginLeft3">' + CurrentUser + '</div>';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100 textAreaContainer">';
        htmlContent += '<input data-val="true" data-val-user-required="The UserCommentId field is required." id="Contracts_0__RevolvingFund_0__UserComments__' + ope + '__UserCommentId" name="Contracts[0].RevolvingFund[0].UserComments[' + ope + '].UserCommentId" type="hidden" value="-1">';
        htmlContent += '<input id="Contracts_0__RevolvingFund_0__UserComments_' + ope + '__Text" name="Contracts[0].RevolvingFund[0].UserComments[' + ope + '].Text" type="hidden" data-val="true" data-val-user-required="Please, complete the mandatory fields" value="">';
        htmlContent += '<textarea class="txtDescriptionComent" id="Contracts_0__RevolvingFund_0__UserComments_' + ope + '__Text" name="Contracts[0].RevolvingFund[0].UserComments[' + ope + '].Text" data-val="true" data-val-required="The Text field is required." rows="5" maxlength="1000"></textarea>';

        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</td>';
        htmlContent += '</tr>';
        $('#commentTableUIRI002_edit').append(htmlContent);
        var ddd = htmlContent;
        

        $("#Contracts_0__RevolvingFund_0__UserComments_" + ope + "__Text").focus();

        $('#commentTableUIRI002_edit').find("textarea").last().focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        resizeIframeCloud();

    });

    $('#newCommentContract002').click(function () {

        var month_Name = new Date().getMonthName();
        var date = new Date()
        var month_Name = date.getMonthName();
        var day = date.getDate();
        var year = date.getUTCFullYear().toString().substring(0, 4);
        var CurrentUser = $("#UserName").attr("value");


        var htmlContent = '';
        var ope = $('table#commentTableUIRI002_edit tbody tr:last').index() + 1;
        htmlContent += '<tr class="new">';
        htmlContent += '<td>';
        htmlContent += '<div class="padding10 leftAlign">';
        htmlContent += '<div class="verticalMargin10 w100 relative h50px" style="height:0%;">';
        htmlContent += '<input type="button" value="Delete" class="deleteTextButton verticalAlignTop absolute absRight deleteTextButtonCon">';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100">';
        htmlContent += '<div class="verticalMargin10 inline-block" style="margin-right: 1%;">' + day + " " + month_Name + " " + year + '</div>';
        htmlContent += '<div class="verticalMargin10 inline-block bold marginLeft3">' + CurrentUser + ' </div>';
        htmlContent += '</div>';
        htmlContent += '<div class="verticalMargin10 w100 textAreaContainer">';
        htmlContent += '<input data-val="true" data-val-user-required="The UserCommentId field is required." id="Contracts_0__UserComments__' + ope + '__UserCommentId" name="Contracts[0].UserComments[' + ope + '].UserCommentId" type="hidden" value="-1">';
        htmlContent += '<input id="Contracts_0__UserComments_' + ope + '__Text" name="Contracts[0].UserComments[' + ope + '].Text" type="hidden" data-val="true" data-val-user-required="Please, complete the mandatory fields" value="">';
        htmlContent += '<textarea class="txtDescriptionComent" id="Contracts_0__UserComments_' + ope + '__Text" name="Contracts[0].UserComments[' + ope + '].Text" rows="5" cols="300" data-val="true" data-val-required="The Text field is required." rows="5" maxlength="1000"></textarea>';

        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</td>';
        htmlContent += '</tr>';
        $('#commentTableUIRI002_edit').append(htmlContent);
        var ddd = htmlContent;
        

        $("#Contracts_0__UserComments_" + ope + "__Text").focus();

        $('#commentTableUIRI002_edit').find("textarea").last().focusout(function () {
            var divParent = $(this).parent();
            var inputImpactIndicator = $(divParent).find('[id$="__Text"]');
            $(inputImpactIndicator).attr("value", $(this).val());
        });

        resizeIframeCloud();

    });

});
