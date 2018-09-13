using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Business.Core.Mainframe.Contracts;
using IDB.MW.Business.Core.Mainframe.DTO.Announcement;
using IDB.MW.Business.Core.Mainframe.Messages;
using IDB.Presentation.MVC4.Areas.Announcements.Models;
using AnnouncementDTO = IDB.MW.Business.Core.Mainframe.DTO.Announcement.AnnouncementDTO;

namespace IDB.Presentation.MVC4.Areas.Announcements.Controllers
{
    public partial class AnnouncementsController : MVC4.Controllers.ConfluenceController
    {
        // GET: Announcements/Announcements
        public virtual ActionResult Index()
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var responseScopeTypes = service.GetScopes();
            if (!responseScopeTypes.IsValid) throw new Exception(responseScopeTypes.ErrorMessage);

            var result = new AnnouncementSearchViewModel
            {
                ScopeTypes = responseScopeTypes.ScopeList,
                SearchResults = new List<AnnouncementSearchItemDTO>()
            };
            return PartialView(result);
        }

        public virtual ActionResult Search(SearchByCriteriaDTO searchFilters, int actualPage)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var response = service.SearchAnnouncement(new SearchAnnouncementRequest
            {
                Criteria = searchFilters,
                Page = actualPage,
                PageSize = 10
            });
            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            var result = new AnnouncementSearchViewModel
            {
                SearchResults = response.Results,
                TotalPages = response.TotalPages,
                ActualPage = actualPage
            };

            return PartialView("~/Areas/Announcements/Views/Partials/SearchResults.cshtml", result);
        }

        public virtual ActionResult Create()
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var scopeTypesResponse = service.GetScopes();
            if (!scopeTypesResponse.IsValid) throw new Exception(scopeTypesResponse.ErrorMessage);
            var globalScope = scopeTypesResponse.ScopeList.SingleOrDefault(o => o.ScopeCode == "GLOBAL");
            if (globalScope == null) throw new Exception("GLOBAL Scope not found");

            var model = new AnnouncementViewModel
            {
                Announcement = new AnnouncementDTO
                {
                    ScopeId = globalScope.ScopeId,
                    ScopeName = globalScope.ScopeText
                },
                ScopeTypes = scopeTypesResponse.ScopeList,
                IsNew = true
            };

            return PartialView("~/Areas/Announcements/Views/Partials/CreateOrEditAnnouncement.cshtml", model);
        }

        [HttpPost]
        public virtual ActionResult Dismiss(int announcementId, bool isDismissed)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var responseAnnouncement = service.UpdateStatus(new UpdateDismissRequest
            {
                AnnouncementId = announcementId,
                IsDismissed = isDismissed
            });
            return Json(responseAnnouncement);
        }

        public virtual ActionResult Edit(int id)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var scopeTypesResponse = service.GetScopes();
            if (!scopeTypesResponse.IsValid) throw new Exception(scopeTypesResponse.ErrorMessage);
            var globalScope = scopeTypesResponse.ScopeList.SingleOrDefault(o => o.ScopeCode == "GLOBAL");
            if (globalScope == null) throw new Exception("GLOBAL Scope not found");

            var responseAnnouncement = service.GetAnnouncement(new GetAnnouncementRequest
            {
                AnnouncementId = id
            });
            var model = new AnnouncementViewModel
            {
                Announcement = responseAnnouncement.Announcement,
                DefaultScopeName = globalScope.ScopeText,
                ScopeTypes = service.GetScopes().ScopeList,
                IsNew = false
            };

            return PartialView("~/Areas/Announcements/Views/Partials/CreateOrEditAnnouncement.cshtml", model);
        }

        public virtual ActionResult Details(int id)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var scopeTypesResponse = service.GetScopes();
            if (!scopeTypesResponse.IsValid) throw new Exception(scopeTypesResponse.ErrorMessage);
            var globalScope = scopeTypesResponse.ScopeList.SingleOrDefault(o => o.ScopeCode == "GLOBAL");
            if (globalScope == null) throw new Exception("GLOBAL Scope not found");

            var responseAnnouncement = service.GetAnnouncement(new GetAnnouncementRequest
            {
                AnnouncementId = id
            });
            var model = new AnnouncementViewModel
            {
                Announcement = responseAnnouncement.Announcement,
                DefaultScopeName = globalScope.ScopeText,
                ScopeTypes = service.GetScopes().ScopeList,
                IsNew = false
            };

            return PartialView("~/Areas/Announcements/Views/Partials/DetailsAnnouncement.cshtml", model);
        }

        public virtual ActionResult Delete(int id)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var scopeTypesResponse = service.GetScopes();
            if (!scopeTypesResponse.IsValid) throw new Exception(scopeTypesResponse.ErrorMessage);
            var globalScope = scopeTypesResponse.ScopeList.SingleOrDefault(o => o.ScopeCode == "GLOBAL");
            if (globalScope == null) throw new Exception("GLOBAL Scope not found");
            var responseAnnouncement = service.GetAnnouncement(new GetAnnouncementRequest
            {
                AnnouncementId = id
            });
            var model = new AnnouncementViewModel
            {
                Announcement = responseAnnouncement.Announcement,
                ScopeTypes = service.GetScopes().ScopeList,
                DefaultScopeName = globalScope.ScopeText,
                IsNew = false
            };
            return PartialView("~/Areas/Announcements/Views/Partials/DeleteAnnouncement.cshtml", model);
        }

        // POST: Announcements/Announcements/Delete/5
        [HttpPost]
        public virtual ActionResult Delete(AnnouncementDTO announcement)
        {
            var service = Globals.Resolve<IAnnouncementService>();
            var responseAnnouncement = service.DeleteAnnouncement(new DeleteAnnouncementRequest
            {
                AnnouncementId = announcement.AnnouncementId
            });

            return Json(responseAnnouncement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Save(AnnouncementDTO announcement)
        {
            if (announcement.ExpirationDate != null &&
                   announcement.StartDate > announcement.ExpirationDate.Value)
            {
                return Json(new
                {
                    IsValid = false,
                    ErrorMessage = "Expiration_Date"
                });
            }

            if (!ModelState.IsValid)
                return Json(new
                {
                    IsValid = false,
                    ErrorMessage = Localization.GetText("Required fields missing")
                });

            var service = Globals.Resolve<IAnnouncementService>();

            if (announcement.AnnouncementId <= 0)
            {
                var responseCreate = service.CreateAnnouncement(new CreateAnnouncementRequest
                {
                    Announcement = announcement
                });

                return Json(responseCreate);
            }

            var responseUpdate = service.UpdateAnnouncement(new UpdateAnnouncementRequest
            {
                Announcement = announcement
            });

            return Json(responseUpdate);
        }
    }
}
