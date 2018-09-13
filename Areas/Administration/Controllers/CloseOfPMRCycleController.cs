using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.PMR.Contracts;
using IDB.MW.Application.PMR.Models;
using IDB.MW.Application.SupervisionPlanModule.Services;
using IDB.MW.Domain.Contracts.ModelRepositories.Administration;
using IDB.MW.Domain.Models.Administration;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Paging;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Pagination;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.Administration.Controllers
{
    public partial class CloseOfPMRCycleController : BaseController
    {
        #region wcf services repositories

        public virtual IAdministrationModelRepository Close_PMR_Cycle { get; set; }

        #endregion

        private readonly IPMRService _pmrCreatePortfolioService;

        public CloseOfPMRCycleController(IPMRService pmrCreatePortfolioService)
        {
            _pmrCreatePortfolioService = pmrCreatePortfolioService;
        }

        // GET: /Administration/CloseOfPMRCycle/
        public virtual ActionResult Index(MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                return RedirectToRoute("Default", new { controller = "Mistakes", action = "Mistake403" });
            }

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            return View();
        }

        public virtual ActionResult Edit(MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                return RedirectToRoute("Default", new { controller = "Mistakes", action = "Mistake403" });
            }

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            ViewBag.HasOpenCycle = Close_PMR_Cycle.HasOpenCycle();

            return View();
        }

        [HttpPost]
        public virtual ActionResult CloseCycle(DateTime? CycleClosingDate)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                return RedirectToRoute("Default", new { controller = "Mistakes", action = "Mistake403" });
            }

            var isOk = true;
            try
            {
                var _spNotificationService = Globals.Resolve<ISpNotificationService>();
                isOk = _spNotificationService.CloseCycle(CycleClosingDate);
            }
            catch (Exception)
            {
                return RedirectToAction(
                    "Index",
                    "CloseOfPMRCycle",
                    new
                    {
                        area = "Administration",
                        messageStatus = MessageNotificationCodes.SaveDataFail
                    });
            }

            if (isOk)
            {
                return RedirectToAction(
                    "Index",
                    "CloseOfPMRCycle",
                    new
                    {
                        area = "Administration",
                        messageStatus = MessageNotificationCodes.SaveDataSucessfully
                    });
            }
            else
            {
                return RedirectToAction(
                    "Index",
                    "CloseOfPMRCycle",
                    new
                    {
                        area = "Administration",
                        messageStatus = MessageNotificationCodes.SaveDataFail
                    });
            }
        }

        [HttpPost]
        public virtual ActionResult OpenCycle()
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                return RedirectToRoute("Default", new { controller = "Mistakes", action = "Mistake403" });
            }

            var isOk = true;
            try
            {
                isOk = Close_PMR_Cycle.OpenCycle();
            }
            catch (Exception)
            {
                return RedirectToAction(
                    "Index",
                    "CloseOfPMRCycle",
                    new
                    {
                        area = "Administration",
                        messageStatus = MessageNotificationCodes.SaveDataFail
                    });
            }

            if (isOk)
            {
                return RedirectToAction("Index", "CloseOfPMRCycle", new { area = "Administration", messageStatus = MessageNotificationCodes.SaveDataSucessfully });
            }
            else
            {
                return RedirectToAction("Index", "CloseOfPMRCycle", new { area = "Administration", messageStatus = MessageNotificationCodes.SaveDataFail });
            }
        }

        [HttpPost]
        public virtual JsonResult Search(PmrCycleFilter filter)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                return Json(new { });
            }

            // Set page and pagesize values according to the needs of every view
            PagingHelper pageHelper = new PagingHelper();
            QuerySettingsPage querySettingsPage = pageHelper.GetQueryPageConfiguration(
                filter.Page,
                PageDefaultSetUp.DefaultPagePmrCycle,
                filter.Size,
                PageDefaultSetUp.DefaultPageSizePmrCycle);

            // Get paged data
            var pmrCycles = this.Close_PMR_Cycle.Search(ref querySettingsPage, filter);

            // Set Paging information to send to the view
            PageSettings pageSettings = pageHelper.GetPagingInformation(querySettingsPage);

            pageSettings.BaseURL = Url.Action("Search", "CloseOfPMRCycle");

            pmrCycles.ForEach(x =>
               {
                   x.CycleClosingDateString = string.Format("{0:dd MMM yyy}", x.CycleClosingDate);
                   x.CycleClosingDate = null;
               });

            return Json(new { PmrCycles = pmrCycles, PageSettings = pageSettings });
        }

        [HttpGet]
        [HasPermission(Permissions = Permission.PMR_ADMIN)]
        public virtual ActionResult CreatePortfolioIndex()
        {
            var model = _pmrCreatePortfolioService.GetPortfolio(Localization.CurrentLanguage);
            return View(model);
        }

        [HttpGet]
        [HasPermission(Permissions = Permission.PMR_ADMIN)]
        public virtual FileResult DownloadPortfolio()
        {
            var response = _pmrCreatePortfolioService
                .GetPortfolioReport(Localization.CurrentLanguage);

            var fileFormat = GetApplicationAndNameReport(
                OutputFormatEnum.Excel, PMRGlobalValues.PMR_PORTFOLIO_NAME);

            return File(response.File, fileFormat.ElementAt(0), fileFormat.ElementAt(1));
        }

        [HttpPost]
        [HasPermission(Permissions = Permission.PMR_ADMIN)]
        public virtual ActionResult ResetPortfolio(int displayedPmrCycleId)
        {
            var response = _pmrCreatePortfolioService.ResetPortfolio(displayedPmrCycleId);

            if (response.IsValid)
                return Json(
                    new
                    {
                        IsValid = response.IsValid,
                        Message = Localization.GetText("AP.PMR.Portfolio.Reset.Sucess")
                    }); 

            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage });
        }

        [HttpPost]
        [HasPermission(Permissions = Permission.PMR_ADMIN)]
        public virtual ActionResult SavePortfolio(
            bool isSaveAsDraft,
            int displayedPmrCycleId,
            IList<CreatePortfolioEntryViewModel> createPortfolioEntryViewModels)
        {
            var response = _pmrCreatePortfolioService
                .SavePortfolio(isSaveAsDraft, displayedPmrCycleId, createPortfolioEntryViewModels);

            if (response.IsValid)
                return Json(
                    new
                    {
                        IsValid = response.IsValid,
                        Message = Localization.GetText("AP.PMR.Portfolio.Save.Success")
                    });

            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage });
        }

        private List<string> GetApplicationAndNameReport(
            OutputFormatEnum outputFormat,
            string nameReport)
        {
            var response = new List<string>();
            response.Add(PMRGlobalValues.APPLICATION + PMRGlobalValues.XLS);
            response.Add(nameReport += PMRGlobalValues.XLS);

            return response;
        }
    }
}
