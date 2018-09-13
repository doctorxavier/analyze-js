using System;
using System.Collections.Generic;
using System.Web.Mvc;
using IDB.MW.Application.AdministrationModule.Services.CycleAdminService;
using IDB.MW.Application.AdministrationModule.ViewModels.CycleAdmin;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class CycleAdminController : MVC4.Controllers.ConfluenceController
    {
        #region fields

        private readonly ICycleAdminService _cycleAdminService;
        #endregion

        public CycleAdminController(ICycleAdminService cycleAdminService)
        {
            _cycleAdminService = cycleAdminService;
        }

        public virtual ActionResult Index()
        {
            var model = _cycleAdminService.GetCycleAdmin().GetCycleAdminModel;
            PermissionCycleAdmin();
            return View(model);
        }

        public virtual ActionResult CloseAndOpenCycle()
        {
            var response = _cycleAdminService.CycleAdminModel();
            PermissionCycleAdmin();
            return PartialView("Partial/IndexButtonsView", response);
        }

        private void PermissionCycleAdmin()
        {
            ViewBag.Permission = IDBContext.Current.HasPermission(Permission.TC_REPORTING_PERIOD_ADMIN);
        }
    }
}