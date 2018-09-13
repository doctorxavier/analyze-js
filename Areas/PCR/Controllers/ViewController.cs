using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.PCRModule.Enums;
using IDB.MW.Application.PCRModule.Services.ChecklistService;
using IDB.MW.Application.PCRModule.Services.FollowUpService;
using IDB.MW.Application.PCRModule.Services.GuidelineService;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.PCR.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.PCR.Controllers
{
    public partial class ViewController : BaseController
    {
        #region Contructors

        public ViewController(
            IPCRChecklistService pcrChecklistService,
            ICatalogService catalogService,
            IPCRGuidelineService prcGuidelineService,
            IPCRFollowUpService pcrFollowUpService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _pcrChecklistService = pcrChecklistService;
            _pcrGuidelineService = prcGuidelineService;
            _pcrFollowUpService = pcrFollowUpService;
            _catalogService = catalogService;

            _viewModelMapperHelper = new ViewModelMapperHelper(
                ViewBag,
                _pcrChecklistService,
                _catalogService,
                _authorizationService,
                _pcrFollowUpService);
        }

        #endregion

        #region Constants

        private const string UrlSummary = "/PCR/Summary";
        private const string UrlEffectiveness = "/PCR/PCRChecklist/Effectiveness";
        private const string UrlGeneral = "/PCR/PCRChecklist/General";
        private const string UrlValidation = "/PCR/PCRChecklist/Validation";
        private const string UrlFollowUp = "/PCR/UrlFollowUp";

        #endregion

        #region Fields

        private readonly IAuthorizationService _authorizationService;
        private readonly IPCRChecklistService _pcrChecklistService;
        private readonly IPCRGuidelineService _pcrGuidelineService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IPCRFollowUpService _pcrFollowUpService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Views

        public virtual ActionResult PCRHeader(string operationNumber, string title)
        {
            var header = _viewModelMapperHelper.GetHeaderViewModel(operationNumber);
            ViewBag.Title = title;
            return PartialView("Partials/PCRHeader", header);
        }

        public virtual ActionResult PCRSummary(string operationNumber)
        {
            ViewBag.Header = _viewModelMapperHelper.GetHeaderViewModel(operationNumber);
            var model = _viewModelMapperHelper.GetSummaryViewModel(operationNumber);
            var checklistModel = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            ViewBag.Verify = _pcrFollowUpService
                .GetVerifyContent(
                    model.Summary,
                    checklistModel.PCREffectivenessViewModel,
                    checklistModel.PCRGeneralViewModel)
                        .FollowUpVerifyViewModel;

            return View(model.Summary);
        }

        public virtual ActionResult PCRChecklist(
            string operationNumber,
            string errorMessages,
            int selectedTab = 0)
        {
            var summaryViewModel = _viewModelMapperHelper
                .GetSummaryViewModel(operationNumber).Summary;
            var model = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);

            ViewBag.Categories = _viewModelMapperHelper.GetCategories();
            ViewBag.Header = _viewModelMapperHelper.GetHeaderViewModel(operationNumber);

            IList<string> opTypes = model.PCRGeneralViewModel.OperationType;
            if (opTypes == null)
                opTypes = OperationTypeHelper.GetOperationTypes(operationNumber);

            ViewBag.OperationType = opTypes;
            ViewBag.PCRId = model.PCRId;
            ViewBag.SelectedTab = selectedTab;
            ViewBag.Verify = _pcrFollowUpService.GetVerifyContent(
                summaryViewModel,
                model.PCREffectivenessViewModel,
                model.PCRGeneralViewModel)
                    .FollowUpVerifyViewModel;
            ViewBag.Permission = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                true);

            model.RedoResponse = _pcrChecklistService.GetRedoData(
                model.PCRValidationsViewModel.OperationId, operationNumber);
            ViewBag.LastPCRRedo = model.RedoResponse.LastPCRForRedo;
            ViewBag.IsActiveRedo = model.RedoResponse.IsValid;
            model.OperationNumber = operationNumber;
            if (!string.IsNullOrWhiteSpace(errorMessages))
            {
                ViewBag.ErrorMessage = HttpUtility
                    .HtmlEncode(HttpUtility.UrlDecode(errorMessages).ProcessStringForView());
                ViewBag.LockScreen = false;
                ViewBag.SelectedTab = 2;
            }

            ViewBag.LockScreenWorkflowValidation = false;

            return View(model);
        }

        public virtual ActionResult PCRFollowUp(string operationNumber)
        {
            var methodologyModel = _pcrFollowUpService
                .GetOldMethodologyModel(operationNumber, false);

            var statuses = _catalogService
                .GetMasterDataListByTypeCode(true, MasterType.OLD_METHO_STATUS);

            if (!statuses.IsValid)
            {
                throw new ApplicationException("PCR Statuses could not be retrieved");
            }

            var oldMethodologyStatusTranslations = new Dictionary<string, string>();

            foreach (var status in statuses.MasterDataCollection)
            {
                oldMethodologyStatusTranslations
                    .Add(status.Code, status.GetLocalizedName(Localization.CurrentLanguage));
            }

            ViewBag.OldMethodologyStatusTranslations = oldMethodologyStatusTranslations;

            return View("PCRFollowUpOldMethodology", methodologyModel.Model);
        }

        [HttpPost]
        public virtual JsonResult PCRFollowUpOldMethodology(string operationNumber)
        {
            var response = _pcrFollowUpService.GetOldMethodologyModel(operationNumber, true);

            return Json(new
            {
                IsValid = response.IsValid,
                ErrorMessage = response.ErrorMessage,
                Data = this.RenderRazorViewToString(
                    "Partials/PCRFollowUpOldMethodologyControl", response.Model),
            });
        }

        public virtual ActionResult PCRFollowUpHeader(string operationNumber)
        {
            return PartialView(
                "Partials/PCRFollowUpHeader",
                _viewModelMapperHelper.GetFollowUpHeader(operationNumber));
        }

        public virtual FileResult DownloadChecklistSummary(string operationNumber)
        {
            var response = _pcrChecklistService.DownloadSummaryReport(operationNumber);

            return !response.IsValid ? null : File(response.File, "application/pdf");
        }

        public virtual FileResult DownloadStageReport(int pcrId)
        {
            var response = _pcrChecklistService.DownloadStageReport(pcrId);

            return !response.IsValid ? null : File(response.File, "application/vnd.ms-excel");
        }

        public virtual FileResult DownloadPCRDoc(int pcrId)
        {
            var response = _pcrChecklistService.DownloadStageReport(pcrId);

            return !response.IsValid ? null : File(response.File, "application/vnd.ms-excel");
        }

        #endregion

        #region Partials

        public virtual ActionResult PCRSummaryStrategicContribution(
            string operationNumber,
            string accessType)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                UrlSummary,
                operationNumber,
                IDBContext.Current.UserName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            var editResponse = _pcrChecklistService.EditPCR(operationNumber);
            if (!editResponse.IsValid)
            {
                return Json(new
                {
                    editResponse.ErrorMessage
                });
            }

            var checklistModel = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            var responseSummary = _viewModelMapperHelper.GetSummaryViewModel(operationNumber);

            if (!responseSummary.IsValid)
            {
                return Json(new
                {
                    responseSummary.ErrorMessage
                });
            }

            ViewBag.Verify = _pcrFollowUpService.GetVerifyContent(
                responseSummary.Summary,
                checklistModel.PCREffectivenessViewModel,
                checklistModel.PCRGeneralViewModel)
                    .FollowUpVerifyViewModel;
            ViewBag.SerializedViewModel = PageSerializationHelper
                .SerializeObject(responseSummary.Summary);

            return PartialView("Partials/PCRSummary", responseSummary.Summary);
        }

        public virtual ActionResult PCREffectiveness(string operationNumber, string accessType)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                UrlEffectiveness,
                operationNumber,
                IDBContext.Current.UserName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            if (accessType == SynchronizationHelper.EDIT_MODE)
            {
                var editResponse = _pcrChecklistService.EditPCR(operationNumber);
                if (!editResponse.IsValid)
                {
                    return Json(new
                    {
                        editResponse.ErrorMessage
                    });
                }
            }

            var model = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            errorMessage = (string)ViewBag.ErrorMessage;

            if (!string.IsNullOrWhiteSpace(errorMessage) || (model.PCRId == int.MinValue))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            ViewBag.Permission = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/PCREffectiveness", model.PCREffectivenessViewModel);
        }

        public virtual ActionResult PCRGeneral(string operationNumber, string accessType)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                UrlGeneral,
                operationNumber,
                IDBContext.Current.UserName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            var editResponse = _pcrChecklistService.EditPCR(operationNumber);

            if (!editResponse.IsValid)
            {
                return Json(new
                {
                    editResponse.ErrorMessage
                });
            }

            var model = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            errorMessage = (string)ViewBag.ErrorMessage;

            if (!string.IsNullOrWhiteSpace(errorMessage) || (model.PCRId == int.MinValue))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            ViewBag.Categories = _viewModelMapperHelper.GetCategories();
            ViewBag.OperationType = model.PCRGeneralViewModel.OperationType;
            ViewBag.Permission = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/PCRGeneral", model.PCRGeneralViewModel);
        }

        public virtual ActionResult PCRValidation(string operationNumber, string accessType)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                UrlValidation,
                operationNumber,
                IDBContext.Current.UserName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            var editResponse = _pcrChecklistService.EditPCR(operationNumber);
            if (!editResponse.IsValid)
            {
                return Json(new
                {
                    editResponse.ErrorMessage
                });
            }

            var model = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            errorMessage = (string)ViewBag.ErrorMessage;

            if (!string.IsNullOrWhiteSpace(errorMessage) || (model.PCRId == int.MinValue))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            ViewBag.Permission = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/PCRValidation", model.PCRValidationsViewModel);
        }

        public virtual ActionResult PCREffectivenessGuidelines(string operationNumber)
        {
            var model = _pcrGuidelineService.GetEffectivenessGuidelines();
            ViewBag.SPDRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRSPDLeaderWrite,
                true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/Modals/PCREffectivenessGuidelines", model.EffectivenessGuidelines);
        }

        public virtual ActionResult PCRGeneralGuidelines(string operationNumber)
        {
            var model = _pcrGuidelineService.GetGeneralGuidelines();
            ViewBag.SPDRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRSPDLeaderWrite,
                true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Modals/PCRGeneralGuidelines", model.GeneralGuidelines);
        }

        public virtual ActionResult PCRFollowUpTaskList(string operationNumber, string accessType)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                UrlFollowUp,
                operationNumber,
                IDBContext.Current.UserName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new
                {
                    ErrorMessage = errorMessage
                });
            }

            ViewBag.SPDRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRSPDLeaderWrite,
                true);
            ViewBag.IsModeEdit = accessType.Equals("edit") ? true : false;

            ViewBag.Documents = _viewModelMapperHelper
                .AddOptionsDocumentsDropDown(IDBContext.Current.CurrentLanguage);
            ViewBag.UserName = IDBContext.Current.UserName;

            var model = _viewModelMapperHelper.GetFollowUp(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/PCRFollowUpTaskList", model);
        }

        public virtual ActionResult PCRFollowUpVerify(string operationNumber)
        {
            var checklistModel = _viewModelMapperHelper.GetChecklistViewModel(operationNumber);
            var responseSummary = _viewModelMapperHelper.GetSummaryViewModel(operationNumber);

            return PartialView(
                "Partials/Modals/PCRFollowUpVerify",
                _pcrFollowUpService.GetVerifyContent(
                    responseSummary.Summary,
                    checklistModel.PCREffectivenessViewModel,
                    checklistModel.PCRGeneralViewModel)
                        .FollowUpVerifyViewModel);
        }

        public virtual ActionResult ShowToSharePointHelp(string caller)
        {
            var url = Globals.GetSetting("BasePath", null);

            var lan = IDBContext.Current.CurrentLanguage;

            string pages = string.Empty;

            string response = string.Empty;

            switch (lan)
            {
                case Language.EN:
                    lan = "en-us";
                    pages = "Pages";
                    break;
                case Language.ES:
                    lan = "es-es";
                    pages = "Paginas";
                    break;
                case Language.FR:
                    lan = "fr-fr";
                    pages = "Pages";
                    break;
                case Language.PT:
                    lan = "pt-br";
                    pages = "Paginas";
                    break;
            }

            switch (caller)
            {
                case "general":
                    response = url + "/sites/knowledge/" + lan + "/" + pages + "/Guidelines-for-General.aspx";
                    break;
                case "effectiveness":
                    response = url + "/sites/knowledge/" + lan + "/" + pages + "/Guidelines-for-Effectiveness.aspx";
                    break;
                case "checklist":
                    response = url + "/sites/knowledge/" + lan + "/" + pages + "/PCR-Checklist-Guidelines.aspx";
                    break;
            }

            return Content(response);
        }

        #endregion

        #region AJAX

        public virtual JsonResult InitializePCRWorkflow(string operationNumber)
        {
            var response = _pcrFollowUpService.InitializePCRWorkflow(
                operationNumber,
                PCRInitializationTypeEnum.Forced);

            if (response.IsValid)
            {
                response.ErrorMessage = "Operation completed successfully.";
            }

            return Json(response);
        }

        public virtual JsonResult AddNewDocumment(string documentNumber)
        {
            var document = new
            {
                DocumentNumber = documentNumber,
                Date = FormatHelper.Format(DateTime.Now.Date),
                User = IDBContext.Current.UserName,
                Url = IDB.MW.Domain.EntityHelpers.DownloadDocumentHelper.GetDocumentLink(
                    documentNumber)
            };

            var result = Json(new { data = document }, JsonRequestBehavior.AllowGet);

            return result;
        }

        #endregion
    }
}