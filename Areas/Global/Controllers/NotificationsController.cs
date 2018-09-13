using System.Collections.Generic;
using System.Web.Mvc;
using IDB.Architecture;
using IDB.MW.Domain.Models.Pagination;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class NotificationsController : BaseController
    {
        public virtual ActionResult Index(string operationNumber = null)
        {
            ViewBag.opNumber = operationNumber;
            ViewBag.K2ViewDetailURL = GlobalCommonLogic.GetK2WorkflowViewDetailURL();

            return View();
        }

        [HttpPost]
        public virtual JsonResult GetData(string operationNumber, DataSourceRequest request)
        {
            var client = new GlobalModelRepository();
            var viewM = client.GetNotificationsByUser(
                IDBContext.Current.UserName,
                operationNumber,
                request,
                GlobalCommonLogic.GetOperationDetailURL(),
                GlobalCommonLogic.GetOperationDraftDetailURL());

            string lang = IDBContext.Current.CurrentLanguage;
            foreach (var item in viewM.Data)
            {
                item.Subject = GlobalCommonLogic.BuildNotification(item.Body, "SUBJECT", lang);
                item.Body = GlobalCommonLogic.BuildNotification(item.Body, "BODY", lang);
            }

            return Json(viewM);
        }

        public virtual JsonResult GetCountNotifications(
            string operationNumber, DataSourceRequest request)
        {
            var client = new GlobalModelRepository();
            var notifications = client.GetNotificationsByUser(
                IDBContext.Current.UserName,
                operationNumber,
                request,
                GlobalCommonLogic.GetOperationDetailURL(),
                GlobalCommonLogic.GetOperationDraftDetailURL());

            notifications.New = 0;
            notifications.Read = 0;
            foreach (var item in notifications.Data)
            {
                if (item.UserRead)
                {
                    notifications.Read++;
                }
                else
                {
                    notifications.New++;
                }
            }

            notifications.Total = notifications.New;

            return Json(notifications);
        }

        public virtual JsonResult MarkAsRead(List<int> ids)
        {
            var client = new GlobalModelRepository();
            var response = client.MarkNotificationsAsRead(ids);
            var serverMessage = "The notifications could not been marked as read";
            if (response)
            {
                serverMessage = "The notifications have been marked as read";
                
                //Refresh counters
                Globals.NotificationsHub.UpdateGlobalUserCounters();
            }

            return Json(new
            {
                Ok = response, Message = serverMessage
            });
        }
    }
}
