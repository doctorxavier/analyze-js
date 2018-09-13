using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.Messages.SupervisionReport;
using IDB.MW.Application.MrBlueModule.Services.SupervisionReport;
using IDB.MW.Application.MrBlueModule.ViewModels.SupervisionReport;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class SupervisionReportController : BaseController
    {
        #region Constants

        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_SUPERVISION_REPORT_STEP1 = "/MrBlue/SupervisionReportStep1";
        private const string URL_SUPERVISION_REPORT_STEP2 = "/MrBlue/SupervisionReportStep2";

        #endregion

        #region Fields

        private readonly ISupervisionReportService _supervisionReportService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Constructors

        public SupervisionReportController(
            ISupervisionReportService supervisionReportService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService,
            ICatalogService catalogService)
            : base(authorizationService, enumMappingService)
        {
            _supervisionReportService = supervisionReportService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _catalogService = catalogService;
        }

        #endregion

        #region Supervision Report
        public virtual ActionResult SupervisionReportDashboard(string operationNumber)
        {
            var model = GetSupervisionReportDashboard(operationNumber);
            SetViewBagRoles(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult SupervisionReportRedirect(string operationNumber, SupervisionReportStepEnum fromStep, StepNavigatorEnum navigator, int versionId = 0)
        {
            base.SetViewBagErrorByTempData();

            SetViewBagRoles(operationNumber);

            if (!ViewBag.WriteRole)
            {
                TempData["ErrorMessage"] = Localization.GetText(NO_WRITE_PERMISSION);
                return RedirectToAction(SupervisionReportStepEnum.Dashboard.GetEnumDescription(), new { operationNumber });
            }

            var responseNextStep = _supervisionReportService.GetSupervisonReportNextStep(operationNumber, versionId, fromStep, navigator);

            return RedirectToAction(responseNextStep.NextStep.GetEnumDescription(), new { operationNumber = operationNumber, versionId = versionId, isEditable = true });
        }

        public virtual ActionResult SupervisionReportStep1(string operationNumber, bool isEditable, int versionId = 0, bool isNew = false)
        {
            base.SetViewBagRoles(operationNumber);

            var jsonResult = base.TryAccessToResources(URL_SUPERVISION_REPORT_STEP1, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SupervisionReportDashboard", new { operationNumber });
            }

            var model = GetSupervisionReportPage1(operationNumber, isEditable, versionId);
            model.IsNew = isNew;

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SupervisionReportDashboard", new { operationNumber });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult SupervisionReportStep2(string operationNumber, bool isNew, int versionId = 0, int parentId = 0)
        {
            SetViewBagRoles(operationNumber);

            if (!ViewBag.WriteRole)
            {
                return Json(new { ErrorMessage = Localization.GetText(NO_WRITE_PERMISSION) });
            }

            var jsonResult = TryAccessToResources(URL_SUPERVISION_REPORT_STEP2, operationNumber, versionId);
            if (jsonResult != null)
            {
                return jsonResult;
            }

            var model = GetSupervisionReportPage2(operationNumber, isNew, versionId, parentId);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.UserName = IDBContext.Current.UserName;

            return View(model);
        }

        public virtual RedirectResult SupervisionReportDownload(string operationNumber, int versionId, string fecha, string typeDoc)
        {
            var response = _supervisionReportService.ExportSupervisionReportToFile(operationNumber, versionId);

            if (!response.IsValid || !Uri.IsWellFormedUriString(response.FileName,
               UriKind.RelativeOrAbsolute))
            {
                return null;
            }

            return Redirect(response.FileName);
        }

        public virtual JsonResult FindUserAD(string filter)
        {
            var adMode = ConfigurationServiceFactory.Current.GetApplicationSettings().IsADMode;
            FindUserADResponse response = new FindUserADResponse();
            
            response = _supervisionReportService.FindUserAD(filter, IDBContext.Current.Operation);
            
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion

        #region Save Supervision Report

        public virtual JsonResult SaveSupervisionReport1(string operationNumber)
        {
            SaveSupervisionReportStep1Response response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SupervisionReportStep1ViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSupervisionReportStep1(jsonDataRequest.ClientFieldData, _enumMappingService);

            response = _supervisionReportService.SaveSupervisionReportStep1(operationNumber, viewModel);

            return Json(response);
        }

        public virtual JsonResult SupervisionReportStep2Save(string operationNumber)
        {
            SaveSupervisionReportStep2Response response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SupervisionReportStep2ViewModel>(jsonDataRequest.SerializedData);

            var givenAnswers = viewModel.SupervisionReportDynamicQuestionnaire.ProcessAnswers(jsonDataRequest.ClientFieldData);

            viewModel.UpdateSupervisionReportStep2(jsonDataRequest.ClientFieldData, _enumMappingService);

            var value = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == "isFinalize");

            bool isFinalize = value != null ? bool.Parse(value.Value) : false;
            response = _supervisionReportService.SaveSupervisionReportStep2(operationNumber, viewModel, givenAnswers, isFinalize);

            return Json(response);
        }

        public virtual JsonResult SupervisionReportDeleteReport(string operationNumber, int versionId)
        {
            var response = _supervisionReportService.DeleteSupervisionReportReport(operationNumber, versionId);

            SetViewBagErrorMessageInvalidResponse(response);
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            return Json(response);
        }

        public virtual JsonResult UploadFileSupervisionReport(string operationNumber, SupervisionReportUploadViewModel supervisionReportUploadViewModel, HttpPostedFileBase FileDocuemnt)
        {
            var response = new UploadSupervisionReportResponse();

            using (var reader = new BinaryReader(FileDocuemnt.InputStream))
            {
                supervisionReportUploadViewModel.File = reader.ReadBytes(FileDocuemnt.ContentLength);
            }

            response = _supervisionReportService.UploadSupervisionReportDocument(operationNumber, supervisionReportUploadViewModel);

            return Json(response);
        }

        #endregion

        #region Private Methods

        private SupervisionReportDashboardViewModel GetSupervisionReportDashboard(string operationNumber)
        {
            var response = _supervisionReportService.GetSupervisionReportDashboard(operationNumber);
            var viewModel = response.SupervisionReportDashboard ?? ViewModelInitializerFactory.InitializeSupervisionReportDashboardViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSupervisionReportDashboard(operationNumber);

            return viewModel;
        }

        private void SetViewBagSupervisionReportDashboard(string operationNumber)
        {
            SetViewBaSupervisionReportDashBoard();
            SetViewBagEnumMapping<ESGDocumentTypeEnum>();
        }

        private SupervisionReportStep1ViewModel GetSupervisionReportPage1(string operationNumber, bool isEditable, int versionId = 0)
        {
            var response = _supervisionReportService.GetSupervisionReportPage1(operationNumber, versionId);
            var viewModel = response.SupervisionReportStep1 ?? ViewModelInitializerFactory.InitializeSupervisionReportPage1ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSupervisionReportPage1(operationNumber);

            return viewModel;
        }

        private void SetViewBagSupervisionReportPage1(string operationNumber)
        {
        }

        private SupervisionReportStep2ViewModel GetSupervisionReportPage2(string operationNumber, bool isNew, int versionId = 0, int parentId = 0)
        {
            var response = _supervisionReportService.GetSupervisionReportPage2(operationNumber, isNew, versionId, parentId);
            var viewModel = response.SupervisionReportPage2 ?? ViewModelInitializerFactory.InitializeSupervisionReportPage2ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSupervisionReportPage2();

            return viewModel;
        }

        private void SetViewBagSupervisionReportPage2()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.ESGItemType, "ItemTypeList" },
                { ConvergenceMasterDataTypeEnum.SafeguardPerfomance, "RatingList" },
                { ConvergenceMasterDataTypeEnum.ESGStatusAction, "StatusList" },
            });

            ViewBag.RatingList = ((List<SelectListItem>)ViewBag.RatingList).OrderBy(x => int.Parse(x.Value)).ToList();
        }

        private void SetViewBaSupervisionReportDashBoard()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.SafeguardPerfomance, "listRating" },
            });

            ViewBag.listRating = ((List<SelectListItem>)ViewBag.listRating).OrderBy(x => int.Parse(x.Value)).ToList();

            SetViewBagEnumMapping<ESGSupervisionReportRatingEnum>();
        }

        private void inicializeList(List<SelectListItem> list, string text, string value)
        {
            var item = new SelectListItem()
            {
                Value = value,
                Text = text
            };

            list.Add(item);
        }

        private List<ListItemViewModel> usersMock()
        {
            var response = new List<ListItemViewModel>();

            response.Add(
                new ListItemViewModel()
                {
                    Value = "1",
                    Text = "texto 1"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "2",
                    Text = "texto 2"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "3",
                    Text = "texto 3"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "4",
                    Text = "texto 4"
                });

            return response;
        }
        #endregion
    }
}