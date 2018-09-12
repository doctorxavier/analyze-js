

function LeftMenuItemClick(e) {

}

$(document).ready(function () {

    updateCounters();
    
    /*to use when load a floting task*/    
    if (hideHeader == true) {   
        $('#collapseBtn').css("background-image", "url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADImlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS4wLWMwNjEgNjQuMTQwOTQ5LCAyMDEwLzEyLzA3LTEwOjU3OjAxICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M1LjEgV2luZG93cyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyOTEwMURFRjg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyOTEwMURGMDg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI5MTAxREVEODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI5MTAxREVFODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+5UlMOgAAAYxJREFUOE/tVL1Kw2AUPflpkyAUR/E5nKu4FMTF1UUc/HkC61ClFBz0CURcXPQFRBCdugiidBXnUoqDaKWkSb4m3vMldSjNZDc9cNObc889yU1uYyQCTBFm9js1/FVDFQVyjBGECkqykKTA9wPwjUVCMpiTI6ihlj3sTT0yQ9u29MkQhgTw9plaeq4DI1EomJEO5uQIaqhlDzHyyEZOEIcRnKKFfgTsHzTw0vnCQCrRgLcW62BOjjVqqGUPe9P7HxlyHgEpJZd9eGxhbaeG++c2Ys+VcfhYZDDJybFGDbU/S5x5pIaWDdMugBdyXGBxaRmv7wm2qkc4v7qDwdElmJNjjRpq2cNeehC5/5TG2Q1OLy4R2iWUK6uaa95eo6h62N1Yx+H2iubGkWvYk2g+dbBXP0b3o6+5udkZnNSrKC/Mo6SZCaDhJEQSvkSrHSSVzZoO5uRYy0P+x0HJXtmO3jef+yHwZDOKTLLaJOQbDn15rbJjtoskXTURy0HJ4piSWF5KjuH/8/V7TNkQ+AZsxAOgjwuUhQAAAABJRU5ErkJggg==')");

        $(".mainContent")
            .removeClass("col-md-10")
            .addClass("col-md-12");
        $("#mainOperationDetails").hide();
        $("#sidebar").hide();

        $('#topHeaderMainMenu').addClass('rowMargin');
    }

    /* Quick Operations search Select2  */
    $(".srch-term").select2({
        
        ajax: {
            dataType: "json",
            delay: 500,
            data: function (params) {               
                
                /*save search string to use it if there is no valid result*/
                $('#headerSearchBtn').attr('searchvalue', params.term);

                return {
                    q: params.term, // search term
                    page: params.page
                };
               
            },
            cache: true
        },
        theme: "bootstrap",
        escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
        minimumInputLength: 4,
        templateResult: function(repo) {
            if (repo.loading) return repo.text;
            var markup = repo.id + " - " + repo.text;
            return markup;
        }, 
        templateSelection: function(repo) {
            return repo.id === "" ? repo.text : repo.id + " - " + repo.text;
            }
    })
    .on("select2:select", function (e) {
        window.open(e.params.data.actionUrl, "_self");
    });

    $("#headerSearchBtn").click(function () {
        var searchOperationNumber = $("#headerSearchBtn").attr("searchvalue");
        $("#headerSearchBtn").attr("href", UrlManager.Operation + searchOperationNumber);
    });

    /* Operation section Collapse button */

    $('#collapseBtn').click(function () {
        
        toggleBars();
    });

    function toggleBars() {
        if ($('.collap').hasClass('collapse in')) {
            $('#collapseBtn').css("background-image", "url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADImlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS4wLWMwNjEgNjQuMTQwOTQ5LCAyMDEwLzEyLzA3LTEwOjU3OjAxICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M1LjEgV2luZG93cyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyOTEwMURFRjg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyOTEwMURGMDg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI5MTAxREVEODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI5MTAxREVFODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+5UlMOgAAAYxJREFUOE/tVL1Kw2AUPflpkyAUR/E5nKu4FMTF1UUc/HkC61ClFBz0CURcXPQFRBCdugiidBXnUoqDaKWkSb4m3vMldSjNZDc9cNObc889yU1uYyQCTBFm9js1/FVDFQVyjBGECkqykKTA9wPwjUVCMpiTI6ihlj3sTT0yQ9u29MkQhgTw9plaeq4DI1EomJEO5uQIaqhlDzHyyEZOEIcRnKKFfgTsHzTw0vnCQCrRgLcW62BOjjVqqGUPe9P7HxlyHgEpJZd9eGxhbaeG++c2Ys+VcfhYZDDJybFGDbU/S5x5pIaWDdMugBdyXGBxaRmv7wm2qkc4v7qDwdElmJNjjRpq2cNeehC5/5TG2Q1OLy4R2iWUK6uaa95eo6h62N1Yx+H2iubGkWvYk2g+dbBXP0b3o6+5udkZnNSrKC/Mo6SZCaDhJEQSvkSrHSSVzZoO5uRYy0P+x0HJXtmO3jef+yHwZDOKTLLaJOQbDn15rbJjtoskXTURy0HJ4piSWF5KjuH/8/V7TNkQ+AZsxAOgjwuUhQAAAABJRU5ErkJggg==')");

            $(".mainContent")
                .removeClass("col-md-10")
                .addClass("col-md-12");
            $("#mainOperationDetails").hide();
            $("#sidebar").hide();

            $('#topHeaderMainMenu').addClass('rowMargin');

        } else if ($('.collap').hasClass('collapse')) {
            $('#collapseBtn').css("background-image", "url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADImlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS4wLWMwNjEgNjQuMTQwOTQ5LCAyMDEwLzEyLzA3LTEwOjU3OjAxICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ1M1LjEgV2luZG93cyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyOTEwMURFRjg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyOTEwMURGMDg2MTUxMUUzOEY5NDgzQjlEOTkyRDVCRSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjI5MTAxREVEODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjI5MTAxREVFODYxNTExRTM4Rjk0ODNCOUQ5OTJENUJFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+5UlMOgAAAYVJREFUOE/tVD1LA0EQncuXghCtJFha+QMEwSKFjRDSC1qIhZImlqYQJdgplhZiZ5M/oAhaptVaf0BI0EI4Di73tZdx3lwEIWxlyjyY27k3771b2OUcFtAUkRuvU8Ms8A/SgCgJieTIcGp6cniAw8wCeyDLqDhPsUPkpVmhB6czC6wTU5gj2Qu992PaOTzTQg8OMxus99CT6r4N6KR9SZ+ur1xlaYGu2i2qrq9QWZlJWAMv7p7o9r5DcaFM1e26ct3nRyoZjxr7u3R+VFNuAghkEzOnKfuyuPJ60Lrm3GaTK1sNvum8sIFECj04zKCBFh54NUOQBUZDTqOQE2m/AubVjRqv1Y/54bXHoXBJIDMp9OAwgwZaeOBFBpAFjmIlsYtv+dBe85Q/+h4HI+Z4KC4jBin04DCDBlp4NFAygHGgfof9yOguem6kNItZZ6mIUejBCaCBFh54swzOLpQxcskEebm5eVmXF0v6HoQRsVOgZFTUQg8OgAZaeIDfjNnv6/+YciDRD9hBeXzTy2ZwAAAAAElFTkSuQmCC')");

            $(".mainContent")
                .removeClass("col-md-12")
                .addClass("col-md-10");
            $("#mainOperationDetails").show();
            $("#sidebar").show();

            $('#topHeaderMainMenu').removeClass('rowMargin');

        }
    }

    /* Navigation Tabs and submenus */
    $(".topMenuLevelOne").mouseenter(function () {
        var menuId = $(this).data("menu"); //Hide the others

        $("[data-parentmenu]").addClass("hidden");
        $("[data-parentmenu=" + menuId + "]").removeClass("hidden");

    })
    .click(function () {

        var menuId = $(this).data("menu");
        $("#topMenuTwo li a").not(".hidden").first(".IsDefaultPage").trigger("click");

    });

    $("#TopMenuNavigationSection").mouseleave(function () {
        $(".topMenuItemSelected .topMenuLevelOne").trigger("mouseenter");
    });

    $(".topMenuItem").click(function () {
        $(".topMenuItem ").removeClass('topMenuItemSelected');

        if ($(this).hasClass("topMenuItemSelected")) {
            $("#topMenuTwo li a").removeClass("subNavBarTopSelected");
        }
        $(this).addClass("topMenuItemSelected");
    });

    $("#topMenuTwo li a").click(function () {
        $("#topMenuTwo li a").removeClass("subNavBarTopSelected");
        $(this).addClass("subNavBarTopSelected");

        if ($(this).data("parentmenu") !== $(".topMenuItemSelected a").data("menu")) {
            $(".topMenuItem ").removeClass('topMenuItemSelected');
            $("#" + $(this).data("parentmenu") + "").addClass('topMenuItemSelected');
        }
    });
    
    $(".topMenuItemSelected a").trigger("mouseenter"); 

    /* User settings */
    $(".popoverLink").popover(
   {
       html: true,
       placement: "bottom",
       content: function () {
           return $(this).next(".popover-content").html();
       }
   });

    $("#userPreferences").parent().on("click", "#UserSettings_Cancel", function () {
        $("#headerUserLink").popover("hide");
    });

    $("#userPreferences").parent().on("click", "#UserSettings_ApplyChanges", function () {
        $.ajax({
            url: UrlManager.SaveUserSettings,
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            cache: false,
            data: JSON.stringify({
                userSetting:
                            {
                                UserSettingId: $("#UserSettingId").val(),
                                PreferredLanguage: $("#userPreferences, input[name=userPreferenceLanguage]:checked").val(),
                                Settings: {
                                    EmailPreferences: {
                                        DoNotSendEmailNotificationsFrom: $("#UserSettings_DoNotSendEmailNotificationsFrom").is(":checked"),
                                        NotificationsPMR: $("#UserSettings_NotificationsPMR").is(":checked"),
                                        NotificationsInstitutions: $("#UserSettings_NotificationsInstitutions").is(":checked"),
                                        NotificationsLifeCycle: $("#UserSettings_NotificationsLifeCycle").is(":checked"),
                                        NotificationsClauses: $("#UserSettings_NotificationsClauses").is(":checked"),
                                        NotificationsSupervisionPlan: $("#UserSettings_NotificationsSupervisionPlan").is(":checked"),
                                        NotificationsPublications: $("#UserSettings_NotificationsPublications").is(":checked")
                                    },
                                    AnnouncementsPreferences: {
                                        DoNotShowAnnouncements: $("#UserSettings_DoNotShowAnnouncements").is(":checked")
                                    }
                                }
                            },
                currentConnectionId: $.connection.hub.id //For SignalR notification
            })
        })
        .done(function (response) {
            if (response.IsValid) {
                if (response.IsReloadRequired) {
                    if ($("#UserSettings_DoNotShowAnnouncements").is(":checked")) {
                        var notifier = $.connection.notificationsHub;
                        notifier.server.closeAllAnnouncements();
                    }
                    //TODO: User confirmation required before reloading the page?
                    location.reload();
                } else {
                    $("#headerUserLink").popover("hide");
                }
            }
            else {
                console.log(response.ErrorMessage);
            }

        })
        .fail(function (jqXhr, textStatus, errorThrown) {
            console.log("Error occurred:" + errorThrown);//TODO: Replace alert for proper showMessage
        });
    });

    $("#userPreferences").parent().on("change", "input#UserSettings_DoNotSendEmailNotificationsFrom", function () {
        var isChecked = $(this).prop('checked');
        $("#emailPreferences input").prop('checked', isChecked);
    });


});


//function LoadUrlIframe(url, iframeId) {
//    //$("#" + iframeId).data("postloadsrc", url);
//    $("#" + iframeId).attr('src', url);
//    return false;
//}

function updateCounters() {
    $.ajax({
        url: UrlManager.UpdateCounter,
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        cache: true
    })
        .done(function (response) {
            if (response.IsValid) {
                updateCounter(response);
            }
        });
}

function updateCounter(response) {
    //Update counters
    $("#globalUserTaskCounter").html(response.TasksInfo.PendingTaskCount);

    if (response.TasksInfo.PendingTaskCount > 0) {
        $(".headerCounterTask").removeClass("CounterBlue")
                               .addClass("CounterRed");
    } else {
        $(".headerCounterTask").removeClass("CounterRed")
                                .addClass("CounterBlue");
    }
        
    $("#globalUserNotificationsCounter").html(response.NotificationsInfo.UnreadNotificationsCount);
    if (response.NotificationsInfo.UnreadNotificationsCount > 0) {
        $(".headerCounterAlarm").removeClass("CounterBlue")
                                .addClass('CounterRed');
    } else {
        $(".headerCounterAlarm").removeClass("CounterRed")
                                .addClass("CounterBlue");
    }

    //Fill Tasks
    $("#taskDivPopover").html("");
    $.each(response.TasksInfo.Tasks, function(index,task) {
        var itemClass = "poptaskDivPar";
        if (index % 2 === 1) {
            itemClass = "poptaskDivImpar";
        }

        $("#taskDivPopover").append(
        "<div class='" + itemClass +"'>" +
            "<a href='" + window.UrlManager.TaskDetail
                        + "?idOperation=" + task.OperationNumber
                        + "&idTask=" + task.WorkflowInstanceTaskId + "'>" +
                "<div class='divEllipsis'>" + task.TaskName + "</div>" +
                "<div class='divEllipsis'>" + task.OperationNumber + " - " + task.OperationName + "</div>" 
                //+ "<div class='divEllipsis'>" + task.WorkflowName + "</div>" +
            +"</a>" 
       + "</div>");
    });
    $(".seeAllTaskCount").html(response.TasksInfo.PendingTaskCount);

    //Fill Notifications
    $("#notificationsDivPopover").html("");
    $.each(response.NotificationsInfo.Notifications, function (index, notification) {
        var itemClass = "poptaskDivPar";
        if (index % 2 === 1) {
            itemClass = "poptaskDivImpar";
        }

        $("#notificationsDivPopover").append(
        "<div class='" + itemClass + "'>" +
            "<a title='" + notification.OperationNumber + ": " + notification.OperationName+ "' " +
                "href='" + window.UrlManager.Operation + notification.OperationNumber + "'>" +
                "<div class='divEllipsis'>" + notification.OperationNumber + "</div>" +
                "<div class='divEllipsis'>" + notification.OperationName + "</div>" +
                //notification.Link,
                //"<div class='divEllipsis'>" + notification.Body + "</div>" +
            "</a>" +
        "</div>");
    });
    $(".seeAllNotificationsCount").html(response.NotificationsInfo.UnreadNotificationsCount);
}