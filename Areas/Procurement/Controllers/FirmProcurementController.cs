using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.BEOProcurementModule.Enums;
using IDB.MW.Application.BEOProcurementModule.Services.FirmProcurement;
using IDB.MW.Application.BEOProcurementModule.ViewModels.FirmProcurement;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.ProcurementModule.ViewModels.FirmProcurement;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.BaseClasses;
using IDB.MW.Infrastructure.Helpers;
using IDB.Presentation.MVC4.Areas.BEOProcurement.Mappers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Controllers
{
    public partial class FirmProcurementController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        public const string TAB_NAME_IDENTIFICATION = "#linktabIdentification";
        public const string TAB_NAME_PREPARATION = "#linktabPreparation";
        public const string TAB_NAME_EVALUATION = "#linktabEvaluation";
        public const string TAB_NAME_NEGOTIATION = "#linktabNegotiation";
        public const string TAB_NAME_EXECUTION = "#linktabExecution";

        private static string SYNC_PROCUREMENT_LIST = "/Procurement/ProcurementList";
        private static string SYNC_PROCUREMENT = "/Procurement/Procurement";
        #endregion

        #region Fields

        private readonly IFirmProcurementService _firmProcurementService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Contructors
        public FirmProcurementController(IEnumMappingService enumMappingService,
            IFirmProcurementService firmProcurementService,
            ICatalogService catalogService)
        {
            _enumMappingService = enumMappingService;
            _firmProcurementService = firmProcurementService;
            _catalogService = catalogService;
        }
        #endregion

        #region Action Methods
        #region Procurement List
        public virtual ActionResult ProcurementList(string operationNumber, string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var models = GetProcurementList(operationNumber);
            return View(models);
        }

        public virtual ActionResult CancelProcurementList(string operationNumber)
        {
            SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT_LIST, operationNumber, IDBContext.Current.UserLoginName);

            return RedirectToAction("ProcurementList");
        }

        public virtual ActionResult EditProcurementList(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT_LIST, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("ProcurementList", new { errorMessage = errorMessage });
            }

            var models = GetProcurementList(operationNumber);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(models);
            return View(models);
        }

        public virtual FileResult ProcurementListExportToPDF(string operationNumber)
        {
            var response = _firmProcurementService.ExportProcurementListToPDF(operationNumber);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "ProcurementList.pdf");
        }

        public virtual FileResult FinalFirmScoringExportToPDF(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.FinalFirmScoringExportToPDF(procurementId);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "FinalFirmScoring.pdf");
        }

        public virtual ActionResult SearchFrameworkProcuremet(string operationNumber, string filter)
        {
            var response = _firmProcurementService.GetFrameworkProcurementList(filter);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult SaveProcurementList(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT_LIST, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementViewModel>(jsonDataRequest.SerializedData);

            model.UpdateProcurementList(jsonDataRequest.ClientFieldData, _enumMappingService);

            var response = _firmProcurementService.SaveProcurementList(operationNumber, model);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT_LIST, operationNumber, IDBContext.Current.UserLoginName);
                var url = Url.Action("ProcurementList", "FirmProcurement", new { area = "Procurement" });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }
        #endregion

        #region Procurement
        public virtual JsonResult FindOperationsAmount(string filter)
        {
            var response = _firmProcurementService.SearchOperationAmounts(filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult FindApprovalNumbers(string filter)
        {
            var response = _firmProcurementService.SearchApprovalNumbers(filter.Trim());

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult Read(string operationNumber, int procurementId, string tabName = null, bool isReadOnly = false, string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            ViewBag.ActiveTab = tabName;
            ViewBag.IsReadOnly = isReadOnly;
            var model = GetReadData(operationNumber, procurementId);
            return View(model);
        }

        public virtual ActionResult CancelProcurementDetail(string operationNumber, int procurementId, string tabName)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            return RedirectToAction("Read", new { procurementId = procurementId, tabName = tabName });
        }

        public virtual ActionResult EditIdentification(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new
                {
                    procurementId = procurementId,
                    tabName = TAB_NAME_IDENTIFICATION,
                    errorMessage = errorMessage,
                });
            }

            ViewBag.ActiveTab = TAB_NAME_IDENTIFICATION;
            var model = new FirmProcurementViewModel();
            model = GetEditIdentificationData(operationNumber, procurementId);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model.Identification);

            return View("Edit", model);
        }

        public virtual JsonResult SaveIdentification(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            bool isClose = Request.Params["isClose"] == "true";

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<IdentificationViewModel>(jsonDataRequest.SerializedData);

            model.UpdateIdentification(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SaveIdentificationData(procurementId, model, isClose);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (response.IsValid)
            {
                if (isClose)
                {
                    //If we save properly and we are closing identification stage send a notification.            
                    _firmProcurementService.SendNotificationCloseIdentification(procurementId);
                }

                _firmProcurementService.SendNotificationSaveCloseIdeNotification(procurementId, response.NewFundIds);
            }

            var tab = TAB_NAME_IDENTIFICATION;
            if (isClose)
            {
                tab = TAB_NAME_PREPARATION;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("Read", "FirmProcurement", new { area = "Procurement", procurementId = procurementId, tabName = tab });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual ActionResult EditPreparation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new
                {
                    procurementId = procurementId,
                    tabName = TAB_NAME_PREPARATION,
                    errorMessage = errorMessage,
                });
            }

            ViewBag.ActiveTab = TAB_NAME_PREPARATION;
            var model = new FirmProcurementViewModel();
            model = GetEditPreparationData(operationNumber, procurementId);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model.Preparation);

            return View("Edit", model);
        }

        public virtual JsonResult SavePreparation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            bool isClose = Request.Params["isClose"] == "true";

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<PreparationViewModel>(jsonDataRequest.SerializedData);

            model.UpdatePreparation(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SavePreparationData(procurementId, model, isClose);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            var tab = TAB_NAME_PREPARATION;
            if (isClose)
            {
                tab = TAB_NAME_EVALUATION;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("Read", "FirmProcurement", new { area = "Procurement", procurementId = procurementId, tabName = tab });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual ActionResult EditEvaluation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new
                {
                    procurementId = procurementId,
                    tabName = TAB_NAME_EVALUATION,
                    errorMessage = errorMessage,
                });
            }

            ViewBag.ActiveTab = TAB_NAME_EVALUATION;
            var model = new FirmProcurementViewModel();
            model = GetEditEvaluationData(operationNumber, procurementId);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.Modality = model.Header.Modality;
            ViewBag.ModalityId = model.Header.ModalityId;

            return View("Edit", model);
        }

        public virtual JsonResult SaveEvaluation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            bool isClose = Request.Params["isClose"] == "true";

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<FirmProcurementViewModel>(jsonDataRequest.SerializedData);

            model.UpdateEvaluation(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SaveEvaluationData(procurementId, model, isClose);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            var tab = TAB_NAME_EVALUATION;
            if (isClose)
            {
                tab = TAB_NAME_NEGOTIATION;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("Read", "FirmProcurement", new { area = "Procurement", procurementId = procurementId, tabName = tab });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual ActionResult EditNegotiation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new
                {
                    procurementId = procurementId,
                    tabName = TAB_NAME_NEGOTIATION,
                    errorMessage = errorMessage,
                });
            }

            ViewBag.ActiveTab = TAB_NAME_NEGOTIATION;
            var model = new FirmProcurementViewModel();
            model = GetEditNegotiationData(operationNumber, procurementId);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model.Negotiation);

            ViewBag.Modality = model.Header.Modality;
            ViewBag.ModalityId = model.Header.ModalityId;

            return View("Edit", model);
        }

        public virtual JsonResult GetInfoByLMSNumber(string cmoNumber, string approvalNumber)
        {
            var response = _firmProcurementService.GetInfoByLMSNumber(cmoNumber, approvalNumber);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult GetInfoByPONumber(string poNumber, string opNumber)
        {
            var response = _firmProcurementService.GetInfoByPONumber(poNumber, opNumber);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult CheckDatesFirm(int offerId, string startDate, string endDate, string signDate, bool checkSignDate)
        {
            var response = _firmProcurementService.CheckDatesFirm(offerId, startDate, endDate, signDate, checkSignDate);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult SaveNegotiation(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            bool isClose = Request.Params["isClose"] == "true";

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<NegotiationViewModel>(jsonDataRequest.SerializedData);

            model.UpdateNegotiation(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SaveNegotiationData(procurementId, model, isClose);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.RedirectMessage = response.RedirectMessage;

            if (response.IsValid)
            {
                _firmProcurementService.SendNotificationSaveCloseIdeNotification(procurementId, response.NewFundIds);
            }

            var tab = TAB_NAME_NEGOTIATION;
            if (isClose)
            {
                tab = TAB_NAME_EXECUTION;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("Read", "FirmProcurement", new { area = "Procurement", procurementId = procurementId, tabName = tab });
                responseView.UrlRedirect = url;
                if (model.Modality == ProcurementModalityEnum.Framework.GetEnumCode() && isClose)
                {
                    if (string.IsNullOrWhiteSpace(responseView.RedirectMessage))
                    {
                        responseView.RedirectMessage = Localization.GetText("TCP.FirmProcurement.Negotiation.Framework.Available");
                    }
                    else
                    {
                        responseView.RedirectMessage = responseView.RedirectMessage + "<br /><br />" + Localization.GetText("TCP.FirmProcurement.Negotiation.Framework.Available");
                    }

                    responseView.RedirectTitle = Localization.GetText("Common.Information");
                }
            }

            return Json(responseView);
        }

        public virtual ActionResult EditExecution(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new
                {
                    procurementId = procurementId,
                    tabName = TAB_NAME_EXECUTION,
                    errorMessage = errorMessage,
                });
            }

            ViewBag.ActiveTab = TAB_NAME_EXECUTION;
            var model = new FirmProcurementViewModel();
            model = GetEditExecutionData(operationNumber, procurementId);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model);

            return View("Edit", model);
        }

        public virtual JsonResult SaveExecution(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            bool isClose = Request.Params["isClose"] == "true";

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<FirmProcurementViewModel>(jsonDataRequest.SerializedData);

            model.UpdateExecution(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SaveExecutionData(procurementId, model, isClose);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.RedirectMessage = response.RedirectMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("Read", "FirmProcurement", new { area = "Procurement", procurementId = procurementId, tabName = TAB_NAME_EXECUTION });

                if (isClose)
                {
                    url = Url.Action("ProcurementList", "FirmProcurement", new { area = "Procurement" });
                }

                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult CheckModificationEndDate(int offerId, string endDate)
        {
            var response = _firmProcurementService.CheckModificationEndDate(offerId, endDate);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult SaveCancelReason(string operationNumber, int procurementId)
        {
            var identifier = string.Format("{0}-proc-{1}", operationNumber, procurementId);
            var responseView = new SaveResponse() { IsValid = true };

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<CancelProcurementViewModel>(jsonDataRequest.SerializedData);

            if (model == null)
            {
                model = new CancelProcurementViewModel();
            }

            model.UpdateCancelReason(jsonDataRequest.ClientFieldData);

            var response = _firmProcurementService.SaveCancelReason(procurementId, model);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(SYNC_PROCUREMENT, identifier, IDBContext.Current.UserLoginName);
                var url = Url.Action("ProcurementList", "FirmProcurement", new { area = "Procurement" });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual bool CanDeleteFirm(int beoOfferId)
        {
            var canDelete = _firmProcurementService.CanDeleteFirm(beoOfferId);
            return canDelete;
        }

        public virtual JsonResult FindFirms(string filter)
        {
            var response = _firmProcurementService.SearchFirms(filter);

            return Json(response);
        }

        public virtual JsonResult HasActiveAssociatedTasks(int procurementId)
        {
            var response = _firmProcurementService.HasActiveAssociatedTasks(procurementId);
            return Json(response);
        }

        #endregion

        #region Popup
        public virtual ActionResult PopUpTest(string operationNumber)
        {
            return View(new List<SelectListItem>());
        }

        public virtual ActionResult GetProcurementsInformation(int outputId)
        {
            var contentResponse = new ReloadHtmlContentResponse();

            var response = _firmProcurementService.GetProcurementsByOutput(outputId);
            contentResponse.IsValid = response.IsValid;
            contentResponse.Message = response.ErrorMessage;
            SetViewBagProcurementInformation();
            var html = this.RenderRazorViewToString("BasicProcurementList", response.ProcurementsByOutput);

            contentResponse.ContentToReplace.Add("[data-name=\"prucurements-info-modal\"]", html);

            return Json(contentResponse);
        }

        public virtual FileResult AssociatedTasksExportToPDF(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.AssociatedTasksExportToPDF(procurementId);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "AssociatedTasks.pdf");
        }

        public virtual FileResult ContractModificationsExportToPDF(string operationNumber, int procurementId, int offerId)
        {
            var response = _firmProcurementService.ContractModificationsExportToPDF(procurementId, offerId);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "ContractModifications.pdf");
        }

        public virtual FileResult FirmEvaluationExportToPDF(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.FirmEvaluationExportToPDF(procurementId);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "FirmEvaluation.pdf");
        }

        private void SetViewBagProcurementInformation()
        {
            var collectionStage = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementStageEnum>();
            var stages = collectionStage.ConvertToSelectListItems();
            ViewBag.StageList = stages;

            var collectionStatus = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementStatusEnum>();
            var status = collectionStatus.ConvertToSelectListItems();
            ViewBag.StatusList = status;
        }
        #endregion
        #endregion

        #region Private Methods
        #region Procurement List
        private ProcurementViewModel GetProcurementList(string operationNumber)
        {
            var response = _firmProcurementService.GetProcurementList(operationNumber);

            SetViewBagErrorMessageInvalidResponse(response);

            var model = response.Procurement;
            SetViewBagProcurementList(operationNumber);
            return model;
        }

        private void SetViewBagProcurementList(string operationNumber)
        {
            ViewBag.OperationNumber = operationNumber;

            var collectionStage = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementStageEnum>();
            var stages = collectionStage.ConvertToSelectListItems();
            ViewBag.StageList = stages;

            var collectionStatus = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementStatusEnum>();
            var status = collectionStatus.ConvertToSelectListItems();
            ViewBag.StatusList = status;

            var collectionModality = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementModalityEnum>();
            var modality = collectionModality.ConvertToSelectListItems();
            ViewBag.ModalityList = modality;

            var cmdCollection = new CustomEnumDictionary<MasterDataViewModel>();
            cmdCollection.AddRange(collectionStage);
            cmdCollection.AddRange(collectionStatus);
            cmdCollection.AddRange(collectionModality);
            ViewBag.CMDCollection = cmdCollection;
        }
        #endregion

        #region Procurement
        private FirmProcurementViewModel GetReadData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetFirmProcurementTabs(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);

            if (response.IsValid)
            {
                SetViewBagRead(operationNumber, procurementId, response.FirmProcurement);
            }

            return response.FirmProcurement;
        }

        private void SetViewBagCommon(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            List<SelectListItem> items = null;

            var collectionCancelReason = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementCancelReasonEnum>();
            items = collectionCancelReason.ConvertToSelectListItems();
            ViewBag.CancelReasonList = items;

            //var responseStage = _firmProcurementService.GetStagesOrdered();
            var responseStage = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementStageEnum>();
            var stages = responseStage.ConvertToSelectListItems();
            ViewBag.StageList = stages;

            var collectionProcurementModality = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementModalityEnum>();

            var cmdCollection = new CustomEnumDictionary<MasterDataViewModel>();
            cmdCollection.AddRange(collectionCancelReason);
            cmdCollection.AddRange(collectionProcurementModality);
            ViewBag.CMDCollection = cmdCollection;

            ViewBag.CurrentUser = IDB.MW.Domain.Session.IDBContext.Current.UserName;

            ViewBag.Negotiation = _enumMappingService.GetMasterData(ProcurementStageEnum.Negotiation).Code;

            ViewBag.Initial = true;
        }

        private void SetViewBagRead(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);

            GetFirmNationalityList();
            GetFirmList(procurementId);
            GetScoringStatusList();
            GetModificationList();
            GetCurrencyList();
            GetStatusScoring();
            ViewBag.ReadMode = true;
            ViewBag.Modality = model.Header.Modality;
            ViewBag.ModalityId = model.Header.ModalityId;
        }

        private FirmProcurementViewModel GetEditIdentificationData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetIdentificationData(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewEditIdentification(operationNumber, procurementId, response.Identification);

            return response.Identification;
        }

        private void SetViewEditIdentification(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);

            GetFirmList(procurementId);
            GetFirmNationalityList();
        }

        private FirmProcurementViewModel GetEditPreparationData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetPreparationData(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewEditPreparation(operationNumber, procurementId, response.Preparation);

            return response.Preparation;
        }

        private void SetViewEditPreparation(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);
        }

        private FirmProcurementViewModel GetEditEvaluationData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetEvaluationData(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewEditEvaluation(operationNumber, procurementId, response.Evaluation);

            return response.Evaluation;
        }

        private void SetViewEditEvaluation(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);

            GetFirmList(procurementId);
            GetScoringStatusList();
            GetCurrencyList();
            GetStatusScoring();
        }

        private FirmProcurementViewModel GetEditNegotiationData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetNegotiationData(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewEditNegotiation(operationNumber, procurementId, response.Negotiation);

            return response.Negotiation;
        }

        private void SetViewEditNegotiation(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);

            GetFirmList(procurementId, model);
            GetCurrencyList();
        }

        private FirmProcurementViewModel GetEditExecutionData(string operationNumber, int procurementId)
        {
            var response = _firmProcurementService.GetExecutionData(procurementId);

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewEditExecution(operationNumber, procurementId, response.Execution);

            return response.Execution;
        }

        private void SetViewEditExecution(string operationNumber, int procurementId, FirmProcurementViewModel model)
        {
            SetViewBagCommon(operationNumber, procurementId, model);

            ViewBag.ReadMode = false;
            GetFirmList(procurementId);
            GetModificationList();
        }
        #endregion

        #region Common
        private bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            ViewBag.IsValid = response.IsValid;
            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    ViewBag.ErrorMessage = HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                }
            }

            return response.IsValid;
        }
        #endregion

        #region viewBag
        private void GetFirmList(int procurementId, FirmProcurementViewModel model = null)
        {
            var offers = _firmProcurementService.GetOffersList(procurementId);

            ViewBag.FirmList = offers.Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();

            ViewBag.FirmListShortListed = offers.Where(t => t.IsShortlisted)
            .Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();

            ViewBag.FirmListWinner = offers.Where(t => t.IsWinner)
            .Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();

            ViewBag.FirmListWinners = offers.Where(t => t.IsWinner);

            var shortFirmBankResponse = offers.Where(t => t.IsShortlisted)
            .Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();
            shortFirmBankResponse.Add(new SelectListItem()
            {
                Text = Localization.GetText("TCP.FirmProcurement.Combo.ClarificationNameOption"),
                Value = "-1",
            });

            ViewBag.FirmListShortListedWithBankResponse = shortFirmBankResponse;

            var consolidateBankResponseWinner = offers.Where(t => t.IsWinner)
            .Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();

            if ((model != null) && (model.Modality == ProcurementModalityEnum.Framework.GetEnumCode()))
            {
                if (consolidateBankResponseWinner.Count() > 1)
                {
                    consolidateBankResponseWinner.Add(new SelectListItem()
                    {
                        Text = Localization.GetText("TCP.FirmProcurement.Combo.NegotiationMinutes"),
                        Value = "-1",
                    });
                }
            }

            ViewBag.FirmWinnersMinutes = consolidateBankResponseWinner;

            var shortFirmIDBComunication = offers.Select(t => new SelectListItem()
            {
                Text = t.FirmName,
                Value = t.OfferId.ToString()
            }).ToList();
            shortFirmIDBComunication.Add(new SelectListItem()
            {
                Text = Localization.GetText("TCP.FirmProcurement.Combo.IDBComunicationOption"),
                Value = "-1"
            });

            ViewBag.FirmListWithIDBComunication = shortFirmIDBComunication;
        }

        private void GetFirmNationalityList()
        {
            ViewBag.FirmNationalityList = _catalogService.GetListMasterData(MasterType.PROCUREMENT_COUNTRY, false).OrderBy(x => x.Text).ToList();
        }

        private void GetScoringStatusList()
        {
            ViewBag.ScoringStatusList = _catalogService.GetListMasterData(MasterType.SCORING_STATUS);
        }

        private void GetModificationList()
        {
            var collectionModificationCauses = _enumMappingService.GetMappingCodeCollectionMasterData<ProcurementModificationCauseEnum>();
            var collectionModificationFWCauses = RemoveAmount(collectionModificationCauses);

            ViewBag.ModificationList = collectionModificationCauses.ConvertToSelectListItems().ConvertToMultiDropDownItems();
            ViewBag.ModificationFWList = collectionModificationFWCauses.ConvertToSelectListItems().ConvertToMultiDropDownItems();
            ViewBag.Amount = _enumMappingService.GetMappingCode(ProcurementModificationCauseEnum.Amount);
            ViewBag.Date = _enumMappingService.GetMappingCode(ProcurementModificationCauseEnum.Dates);
            ViewBag.Cancellation = _enumMappingService.GetMappingCode(ProcurementModificationCauseEnum.Cancellation);

            var cmdCollection = new CustomEnumDictionary<MasterDataViewModel>();
            cmdCollection.AddRange(collectionModificationCauses);
            ViewBag.ModificationCausesEnumValues = cmdCollection;
        }

        private void GetCurrencyList()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            var currencyDolar = _enumMappingService.GetMasterData(CurrencyCodeEnum.DOL);
            var currencyEuro = _enumMappingService.GetMasterData(CurrencyCodeEnum.EUR);

            var orderListCurrency = _catalogService.GetListMasterData(MasterType.CURRENCY, onlyCode: true, orderByCodeAsc: true);
            var euroCurrencyItem = orderListCurrency.FirstOrDefault(x => x.Text == currencyEuro.Code);
            var dolaCurrencyItem = orderListCurrency.FirstOrDefault(x => x.Text == currencyDolar.Code);

            orderListCurrency.Remove(euroCurrencyItem);
            orderListCurrency.Remove(dolaCurrencyItem);

            orderListCurrency.Insert(0, new SelectListItem() { Text = "-----------------------", Value = "separator-1", Disabled = true });
            orderListCurrency.Insert(0, new SelectListItem() { Text = currencyDolar.Code, Value = currencyDolar.MasterDataId.ToString() });
            orderListCurrency.Insert(0, new SelectListItem() { Text = currencyEuro.Code, Value = currencyEuro.MasterDataId.ToString() });

            ViewBag.CurrencyList = orderListCurrency;
        }

        private void GetStatusScoring()
        {
            ViewBag.discualified = _enumMappingService.GetMappingCode(ProcurementScoringStatusEnum.Disqualified);
            ViewBag.evaluated = _enumMappingService.GetMappingCode(ProcurementScoringStatusEnum.Evaluated);
            ViewBag.nonResponsive = _enumMappingService.GetMappingCode(ProcurementScoringStatusEnum.NonResponsive);
        }

        private IEnumerable<MasterDataViewModel> RemoveAmount(IEnumerable<MasterDataViewModel> list)
        {
            var codeAmount = ProcurementModificationCauseEnum.Amount.GetEnumCode();
            return list.Where(x => x.Code != codeAmount);
        }
        #endregion viewBag
        #endregion
    }
}
