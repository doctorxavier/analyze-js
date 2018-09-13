using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Notification;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class NotificationViewController : BaseController
    {
        #region constants
        #endregion

        #region fields
        private readonly ICatalogService _catalogService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        #endregion

        public NotificationViewController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, _catalogService);
        }

        public virtual ActionResult Notification()
        {
            var NotificationList = _viewModelMapperHelper.GetListNotificationsAll();
            var data = new List<System.Web.Mvc.SelectListItem>();
            for (var i = 0; i < 50; i++)
            {
                data.Add(new System.Web.Mvc.SelectListItem()
                {
                    Text = "Code " + i,
                    Value = i.ToString()
                });
            }

            for (var i = 0; i < 50; i++)
            {
                data.Add(new System.Web.Mvc.SelectListItem()
                {
                    Text = "Name " + i,
                    Value = i.ToString()
                });
            }

            ViewBag.ListItem = data;
            return View(NotificationList);
        }

        public virtual ActionResult NotificationEdit()
        {
            return View();
        }

        public virtual ActionResult SearchNotification(string filter)
        {
            var NotificationList = _viewModelMapperHelper.GetListNotificationsAll();
            NotificationList.TableNotifications.RemoveRange(1, 4);
            return PartialView("Partials/NotificationTableSearch", NotificationList);
        }

        public virtual ActionResult NotificationEdit(string notificationId, bool isNew)
        {
            var notificationData = _viewModelMapperHelper.GetNotificationEditable(isNew);
            return View(notificationData);
        }
    }
}