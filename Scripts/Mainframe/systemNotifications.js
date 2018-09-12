//notifications Interval
var timeInterval = 5000;
//Time to wait for reconnection
var reconnectInterval = 60000;//1min

$(function () {
    $.connection.hub.qs = "showNotification=" + window.UserSettings.DoNotShowAnnouncements;
    // Reference the auto-generated proxy for the hub.
    var notifier = $.connection.notificationsHub;

    //Shows single announcement
    notifier.client.showAnnouncement = function(announcement) {
        
        if (window.UserSettings.DoNotShowAnnouncements) return;

        //TODO: Replace date format for more formal library such as jquery ui $.datepicker.formatDate or Moment.js or else
        var date = new Date(announcement.StartDate);
        var dd = date.getDate().toString();
        var mm = (date.getMonth() + 1).toString();
        var yyyy = date.getFullYear();
        var toDate = (dd[1] ? dd : "0" + dd[0]) + '-' + (mm[1] ? mm : "0" + mm[0]) + '-' + yyyy;


        var notification = {
            NotificationTypeName: "info",
            Title: announcement.Description,
            Body: announcement.Body
            + "<div class='row signalR_LinksRow'>" +
                "<div class='col-md-6 signalR_AnnoucementNotificationDate'>" +
                    toDate +
                "</div>" +
                "<div class='col-md-6'>" +
                    "<a class='pull-right' href='#' onclick='dismissAnnoucement(" + announcement.AnnouncementId + ",this)'>" + "Do not show again" + "</a>" +
                "</div>" +
              "</div>"
        };
        notifier.client.notify(notification);
    }

    //Shows multiple announcements
    notifier.client.showAnnouncements = function(response) {
        if (!response.IsValid) {
            console.error("showAnnouncements: " + response.ErrorMessage);
            return;
        }
        $.each(response.Results, function (index, announcement) {
            notifier.client.showAnnouncement(announcement);
        });
    }

    notifier.client.closeAllAnnouncements = function() {
        $.notifyClose();
    }

    notifier.client.hubMethodExecutionError = function (message) {
        console.log(message);
    }
    
    //Show Notifications
    notifier.client.notify = function (notification) {
        var icon = "warning-sign";

        switch (notification.NotificationTypeName) {
            case "success": icon = "ok-sign"; break;
            case "info": icon = "info-sign"; break;
            case "warning": icon = "warning-sign"; break;
            case "danger": icon = "exclamation-sign"; break;
        };

       return $.notify({
            // options
            icon: "glyphicon glyphicon-"+icon,
            title: notification.Title + "<br/>",
            message: notification.Body,
            newest_on_top: true
        }, {
            // settings
            type: notification.NotificationTypeName,
            placement: {
                from: "bottom",
                align: "right"
            },
            delay: 0,
            template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
		                '<button type="button" aria-hidden="true" class="close" data-notify="dismiss">×</button>' +
		                '<span data-notify="icon"></span> ' +
		                '<span data-notify="title">{1}</span> ' +
		                '<span data-notify="message">{2}</span>' +
		                //'<div class="progress" data-notify="progressbar">' +
			            //    '<div class="progress-bar progress-bar-{0}" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%;"></div>' +
		                //'</div>' +
		                //'<a href="{3}" target="{4}" data-notify="url"></a>' +
	                '</div>'
        });
    };
    
    notifier.client.welcomeMessage = function (title, imageUrl, message, links) {

        //Thumbnail of 90x90
        imageUrl = window.UrlManager.CachedGetUserPhotoThumbnail + "?userName=" + imageUrl + "&width=90&height=90";

        $.notify({
            // options
            //icon: 'glyphicon glyphicon-warning-sign',
            icon: imageUrl,
            title: title,
            message: message
        }, {
            // settings
            type: 'minimalist',
            placement: {
                from: "bottom",
                align: "right"
            },
            delay: 9000, //20000,
            //timer: 1000,
            icon_type: "image",
            template: '<div id="mainAnnouncement" data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                            '<img data-notify="icon" class="img-circle pull-left" height="60">' +
                            '<span data-notify="title">{1}</span>' +
                            '<span data-notify="message">{2}</span><br>' +
                            "<span data-notify=\"message\">" + links + "</span>" +
                       '</div>'
        });
    };

    
    notifier.startPeriodicNotifications = function () {
        //setTimeout(function () {
        //    //notifier.server.sendNotificationWarning(decodeURI(notifier.client.UserId), notifier.notificationCounter);

        //    notifier.server.updateGlobalUserCounters().done(function(successful) {
        //        if (successful) {
        //            notifier.startPeriodicNotifications();
        //        }

        //        timeInterval = 3600000;
        //    });
        //}, timeInterval);
    }


    //Enable logging
    $.connection.hub.logging = false;

    $.connection.hub.connectionSlow(function () {
        console.log("Connection problem..."); // Your function to notify user.
    });

    $.connection.hub.reconnecting(function () {
        console.log("Reconnecting..."); // Your function to notify user.
    });

    $.connection.hub.disconnected(function () {
        console.log("Disconnected, waiting " + reconnectInterval/1000 + " seconds to reconnect...");
        setTimeout(function () {
            $.connection.hub.start();
        }, reconnectInterval); // Restart connection after 5 seconds.
    });

    // Start the connection.
    $.connection.hub
		.start({
		    transport: ["webSockets", "serverSentEvents"]
		})
		.done(function () {
            notifier.server.showUserAnnouncements();
        });
});
// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $("<div />").text(value).html();
    return encodedValue;
}


function LoadNotificationDetail(operationNumber) {
    window.location = window.UrlManager.NotificationDetail + operationNumber;
}


function dismissAnnoucement(announcementId, sender) {
   
    //Change annoucement state
    $.ajax({
        url: window.UrlManager.AnnouncementDismiss,
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        cache: false,
        data: JSON.stringify({ announcementId: announcementId, isDismissed: true})
    })
       .done(function (response) {

           if (response.IsValid) {
               $(sender).closest("[data-notify=\"container\"]")
                        .find("[data-notify=\"dismiss\"]").trigger("click");
           }
           else {
               console.log(response.ErrorMessage);
           }

       })
       .fail(function (jqXhr, textStatus, errorThrown) {
           console.log("Error occurred:" + errorThrown);
       });
}