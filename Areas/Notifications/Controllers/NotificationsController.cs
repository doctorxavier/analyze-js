using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using IDB.MW.Business.NotificationsModule.Contracts;
using IDB.MW.Business.NotificationsModule.Messages;
using IDB.MW.Business.NotificationsModule.Services;
using IDB.MW.Domain.Models.Pagination;

namespace IDB.Presentation.MVC4.Areas.Notifications.Controllers
{
    public partial class NotificationsController : MVC4.Controllers.ConfluenceController
    {
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual JsonResult GetCountNotifications(string userName = null)
        {
            var notificationsService = new NotificationsService(); //TODO: Replace by CastleWindsor call

            var response = notificationsService.GetNotificationsByUser(new GetNotificationsByUserRequest
            {
                Username = userName ?? User.Identity.Name,
                OperationNumber = string.Empty,
                Request = new DataSourceRequest
                {
                    Skip = 0,
                    Take = 100
                },
                UrlOperation = string.Empty,
                UrlOperationDraft = string.Empty,
                Notification = false
            });

            return new JsonResult
            {
                Data = response.Notifications
            };
        }
    }
}
