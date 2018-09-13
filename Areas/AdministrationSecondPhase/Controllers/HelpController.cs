using System.Web.Mvc;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MVCControls.Confluence.Models;
using IDB.MW.Application.AdministrationModule.Services.HelpService;
using IDB.MW.Application.AdministrationModule.ViewModels.Help;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Help;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class HelpController : BaseController
    {
        private readonly IHelpService _helpService;

        public HelpController(IHelpService helpService)
        {
            _helpService = helpService;
        }

        [HttpPost]
        public virtual ActionResult AddViewCodeModal(string viewCode)
        {
            return PartialView(
                "Partial/_ModalViewCode", _helpService.BuildFieldsForModal(viewCode));
        }

        [HttpPost]
        public virtual JsonResult AddViewCodeToHelpData(AliasItemViewModel alias)
        {
            return Json(_helpService.Save(alias), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual JsonResult DeleteViewsHelpData(int[] ids)
        {
            return Json(_helpService.Delete(ids), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual ActionResult Details(int id)
        {
            var viewModel = new AdministratorViewModel<DetailsViewModel>();
            var response = new SingleModelResponse<DetailsViewModel>
            {
                IsValid = true,
            };

            if (id == 0)
            {
                response = _helpService.SetNewView();

                viewModel.Data = response.Model;
                viewModel.Descriptions = this.FillMultiLanguageBoxSimpleModel(
                    "description", response.Model.Descriptions);
                viewModel.Urls = this.FillMultiLanguageBoxSimpleModel(
                    "url", response.Model.Urls, new bool[] { true, false, false, false });

                viewModel.ButtonConfiguration = new ButtonControlViewModel
                {
                    ActionEdit = "edit",
                    ActionSave = "saveDetails",

                    HasEditButton = IDBContext.Current.HasPermission(
                        Permission.HELP_CONVERGENCE_WRITE),

                    NavigationCancel = Url.Action(
                        controllerName: "Help",
                        actionName: "Index"),
                    UrlSave = Url.Action(
                        controllerName: "Help",
                        actionName: "SaveViewHelpData")
                };

                return View(viewModel);
            }

            response = _helpService.GetViewData(id);

            if (!response.IsValid)
            {
                return RedirectToAction("Index");
            }

            viewModel.Data = response.Model;
            viewModel.Descriptions = this.FillMultiLanguageBoxSimpleModel(
                "description", response.Model.Descriptions);
            viewModel.Urls = this.FillMultiLanguageBoxSimpleModel(
                "url", response.Model.Urls, new bool[] { true, false, false, false });

            viewModel.ButtonConfiguration = new ButtonControlViewModel
            {
                ActionCancel = "cancelDetails",
                ActionEdit = "edit",
                ActionSave = "saveDetails",

                HasEditButton = IDBContext.Current.HasPermission(
                    Permission.HELP_CONVERGENCE_WRITE),

                UrlSave = Url.Action(
                    controllerName: "Help",
                    actionName: "SaveViewHelpData")
            };

            return View(viewModel);
        }

        [HttpGet]
        public virtual ActionResult GetDataByClearFilter()
        {
            return PartialView("Partial/_TableViews", _helpService.ClearFilter());
        }

        [HttpGet]
        public virtual ActionResult GetDataByFilter(FiltersDataViewModel request)
        {
            return PartialView("Partial/_TableViews", _helpService.Filter(request));
        }

        [HttpGet]
        public virtual ActionResult Index()
        {
            var response = _helpService.GetViewsData();

            var viewModel = new AdministratorViewModel<IndexViewModel>
            {
                Data = response.Model,
                ButtonConfiguration = new ButtonControlViewModel
                {
                    ActionCancel = "cancelIndex",
                    ActionEdit = "edit",
                    ActionSave = "saveIndex",

                    HasEditButton = IDBContext.Current.HasPermission(
                        Permission.HELP_CONVERGENCE_WRITE),
                    HasNewButton = IDBContext.Current.HasPermission(
                        Permission.HELP_CONVERGENCE_WRITE),

                    NavigationNew = Url.Action(
                        controllerName: "Help",
                        actionName: "Details",
                        routeValues: new { id = 0 }),
                    UrlSave = Url.Action(
                        controllerName: "Help",
                        actionName: "DeleteViewsHelpData")
                }
            };

            if (!response.IsValid)
            {
                viewModel.ButtonConfiguration.HasEditButton = false;
                viewModel.ButtonConfiguration.HasNewButton = false;
                ViewBag.ErrorMessage = response.ErrorMessage;

                return View(viewModel);
            }

            if (!response.Model.HelpViews.HasAny())
            {
                viewModel.ButtonConfiguration.HasEditButton = false;
                ViewBag.ErrorMessage = response.ErrorMessage;

                return View(viewModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        public virtual JsonResult InitializeJSData()
        {
            return Json(_helpService.GetHelpData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual ActionResult NewAliasRow()
        {
            return PartialView(
                "Partial/_TableAliasNewRow",
                new AliasItemViewModel { Id = 0, ViewCode = string.Empty, ViewId = 0 });
        }

        [HttpPost]
        public virtual JsonResult SaveViewHelpData(DetailsViewModel data)
        {
            return Json(_helpService.Save(data), JsonRequestBehavior.AllowGet);
        }

        public virtual FileResult DownloadReport(FiltersDataViewModel filters)
        {
            var response = _helpService.GetHelpReport(filters);

            if (!response.IsValid)
            {
                response.ErrorMessage = Localization.GetText("AP.HELP.Message.NoRecordsFound");

                return null;
            }

            return File(
                response.File,
                HelpValues.APPLICATION_REPORT,
                HelpValues.FILENAME_REPORT);
        }

        private MultiLanguageBoxSimpleModel FillMultiLanguageBoxSimpleModel(
            string name,
            MultilanguageFieldViewModel data,
            bool[] requireds = null)
        {
            requireds = requireds ?? new bool[] { true, true, false, false };

            return new MultiLanguageBoxSimpleModel
            {
                MaxLength = 256,
                EnAreaName = name + Language.EN.ToUpper(),
                EnAreaValue = data.English ?? string.Empty,
                EnRequired = requireds[0],
                EsAreaName = name + Language.ES.ToUpper(),
                EsAreaValue = data.Spanish ?? string.Empty,
                EsRequired = requireds[1],
                PtAreaName = name + Language.PT.ToUpper(),
                PtAreaValue = data.Portuguese ?? string.Empty,
                PtRequired = requireds[2],
                FrAreaName = name + Language.FR.ToUpper(),
                FrAreaValue = data.French ?? string.Empty,
                FrRequired = requireds[3]
            };
        }
    }
}