$(document).on("ready", function ()
{
    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();

    $(document).on('click', '.searchDelegateUser', function () {
        //event.preventDefault();
		if(event.preventDefault){
			event.preventDefault();
		}else{
		event.returnValue = false; 
		};
        var form = $(this).closest('form');
        if ($('#User').val() == ''
            && $('#OperationNumber').val() == ''
            && $('#SharepointSelectedGroupName').val() == ''
            && $('#Country').val() == ''
            && $('#Division').val() == '') {
            $('.error-validation').removeClass('hide');
            return;
        }
        idbg.lockUi($("#DelegatingTaskWindow"), true);
        $("#DelegatingTaskWindow").load($(form).attr('action'), $(form).serialize(), function callback() {
            idbg.lockUi($("#DelegatingTaskWindow"), false);
        });
    });

    $(document).on('click', '.clearFilter', function () {
        //event.preventDefault();
		if(event.preventDefault){
			event.preventDefault();
		}else{
		event.returnValue = false; 
		};
        $("#DelegatingTaskWindow").load($(this).data('searchUrl'), function () {
            $('.kendoDropDown').kendoDropDownList();
        });
    });

    var SelectedUserName = "";
    $(document).on('click', '.membersContainer .memberContainer', function () {
        if ($(this).hasClass('not-selectable'))
            return;
        if ($(this).find(".popUpSelectedPicture") > -1) {
            $(this).removeClass('active');
            $(this).find('.popUpSelectedPicture').removeClass('popUpSelectedPicture');
        } else {
            $(".teamMember").siblings(".absolute.absRight").removeClass('popUpSelectedPicture');
            $('.memberContainer').removeClass('active');
            $(this).addClass('active');
            $(this).find(".teamMember").siblings(".absolute.absRight").addClass('popUpSelectedPicture');
        }
        SelectedUserName = $(this).attr('id');
    });

    $(document).on('click', '.saveDelegateUser', function ()
    {
        var selectedUser = $('.active');
        var taskId = $('#TaskId').val();
        if (selectedUser.length == 0 || taskId < 0)
        {
            return;
        }

        location.href = $(this).data('saveRoute') +
            '?operationNumber=' + $("#hdnOperationNumber").val() +
            '&taskId=' + taskId +
            '&username=' + SelectedUserName + 
            '&currentUserName=' + $("#CurrentUser").attr("value");

    });

    $(document).on('click', '.search-replace', function () {
        $("#DelegatingTaskWindow").load($(this).data('searchRoute'));
    });

    $(document).on('click', '.remove-delegation', function () {
        //event.preventDefault();
		if(event.preventDefault){
			event.preventDefault();
		}else{
		event.returnValue = false; 
		};
        $('#username').val('');
        $('.memberContainer > div.relative').remove();
    });

    $(document).on('click', '.cancelButton', functionClose);
});


function showSearchWindow(btn) {

    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();

    // Display modal window to remove impact from the server side
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    var title = "Warning";
    var dialog = $(".dinamicModal").kendoWindow({
        width: "650px",
        title: "Delegated Task",
        draggable: false,
        resizable: false,
        content: $(btn).data('searchRoute'),
        pinned: true,
        actions: [
            "Close"
        ],
        modal: true,
        visible: false,
        close: function () {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
        }
    }).data("kendoWindow");
    //$(".k-window-titlebar").addClass("warning");
    //$(".k-window-title").addClass("ico_warning");
    //dialog.center();
    dialog.open();
}

function showDelegatingTaskWindow(url) {

    $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
    $("body").find(".ui-widget-overlay").remove();
    $(".ui-widget-overlay").remove();
    $(".k-window").remove();

    //event.preventDefault();
	if(event.preventDefault){
		event.preventDefault();
	}else{
		event.returnValue = false; 
	};
    $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="ui-widget-overlay ui-front"></div>');
    $("body").append('<div class="dinamicModal"><div class="loadingdatawarning"><br>Loading...</div></div>');
    $(".dinamicModal").load(url, function () {
        $('.kendoDropDown').kendoDropDownList();
        var title = $("#DelegatingTaskWindow").data("windowTitle");
        var modal = $(".dinamicModal").kendoWindow({
            width: "800px",
            position: { top: 100 },
            title: title,
            draggable: false,
            resizable: false,
            pinned: false,
            actions: ["Close"],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("popUpHeaderBackground padding20 relative");
        modal.open();
    });
};

var functionClose = function () {
    //event.preventDefault();
	if(event.preventDefault){
		event.preventDefault();
	}else{
		event.returnValue = false; 
	};
    var modal = $(".dinamicModal").data("kendoWindow");
    modal.close();
    $(".ui-widget-overlay").remove();
};