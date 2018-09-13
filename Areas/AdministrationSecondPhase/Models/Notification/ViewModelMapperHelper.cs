using System.Collections.Generic;

using IDB.MW.Application.AdministrationModule.ViewModels.Notification;
using IDB.MW.Application.Core.Services.CatalogService;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Notification
{
    public class ViewModelMapperHelper
    {
        private readonly dynamic _viewBag;
        private readonly ICatalogService _catalogService;

        public ViewModelMapperHelper(dynamic viewBag, ICatalogService catalogService)
        {
            _viewBag = viewBag;
            _catalogService = catalogService;
        }

        public NotificationSearchViewModel GetListNotificationsAll()
        {
            var response = new NotificationSearchViewModel();

            var list = new List<TableNotificationViewModel>();

            var item1 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-001",
                Name = "Validation of modification of revolving fund percentage",
                Description = "Approval of the change for the revolving fund percentage according to the OA-420 indications.",
                ExpirationDate = "24 Mar 2016"
            };
            list.Add(item1);

            var item2 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-002",
                Name = "VEligibility Date Approval",
                Description = "Approval of the eligibility date according to the OA-420 indications.",
                ExpirationDate = "24 Mar 2016"
            };
            list.Add(item2);

            var item3 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-003",
                Name = "Clause Extension Approval",
                Description = "Approval of an extension of a clause according to the OA-420 indications.",
                ExpirationDate = "25 Mar 2016"
            };
            list.Add(item3);

            var item4 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-004",
                Name = "Clause Final Status Validation",
                Description = "Approval of the final status of a clause according to the OA-420 indications.",
                ExpirationDate = "25 Mar 2016"
            };
            list.Add(item4);
            var item5 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-005",
                Name = "Validation of supervision plan    ",
                Description = "Approval of the supervision plan for current year.",
                ExpirationDate = string.Empty
            };
            list.Add(item5);

            var item6 = new TableNotificationViewModel
            {
                Id = 1,
                Code = "WF-CL-006",
                Name = "Validation",
                Description = "Approval ",
                ExpirationDate = string.Empty
            };
            list.Add(item6);

            response.TableNotifications = list;
            return response;
        }

        public NotificationEditViewModel GetNotificationEditable(bool isNew)
        {
            var notification = new NotificationEditViewModel
            {
                NewNotification = isNew
            };
            return notification;
        }
    }
}