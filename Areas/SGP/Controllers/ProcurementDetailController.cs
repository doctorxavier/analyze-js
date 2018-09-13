using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

using IDB.MW.Application.Core.Container;
using IDB.MW.Application.SGPModule.Constants;
using IDB.MW.Application.SGPModule.Services.ProcurementDetail;
using IDB.MW.Application.SGPModule.ViewModels.ProcurementDetail;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.SGP.Mappers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Session;
using IDB.Architecture.Language;
using IDB.MW.Application.GlobalModule.Messages.WorkflowsService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.SGPModule.Enums;
using IDB.MW.Application.SGPModule.Services.AscUpdateStatus;

namespace IDB.Presentation.MVC4.Areas.SGP.Controllers
{
    public partial class ProcurementDetailController : Controller
    {
        #region Constants
        private static string SGP_PROCUREMENT_DETAIL_URL = "/SGP/ProcurementDetails/Edit/";
        #endregion

        #region Fields

        private readonly IProcurementDetailService _procurementDetailsService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAscUpdateStatusService _ascUpdateStatusService;
        #endregion

        #region Constructors

        public ProcurementDetailController(IProcurementDetailService procurementDetailService,
                                           IAscUpdateStatusService ascUpdateStatusService,
                                           IEnumMappingService enumMappingService)
        {
            _procurementDetailsService = procurementDetailService;
            _enumMappingService = enumMappingService;
            _ascUpdateStatusService = ascUpdateStatusService;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, int procurementId = 0, string tabName = null, int? contractId = null, int? packageId = null, string errorMessage = null)
        {
            bool isEditMode = false;
            var model = GetReadData(procurementId, isEditMode, contractId, packageId);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                model.ViewContainer["IsValid"] = false;
                model.ViewContainer["ErrorMessage"] = errorMessage;
            }

            model.ViewContainer["ActiveTab"] = tabName ?? ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL;
            return View(model);
        }

        public virtual ActionResult Cancel(string operationNumber, int procurementId = 0, string tabName = null, int? contractId = null, int? packageId = null)
        {
            SynchronizationHelper.TryReleaseLock(SGP_PROCUREMENT_DETAIL_URL + procurementId, operationNumber, IDBContext.Current.UserLoginName);

            bool isEditMode = false;
            var model = GetReadData(procurementId, isEditMode, contractId, packageId);
            model.ViewContainer["ActiveTab"] = tabName ?? ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_LEVEL;
            return View("Read", model);
        }

        public virtual ActionResult Edit(string operationNumber, int procurementId = 0, string tabName = null, int? contractId = null, int? packageId = null)
        {
            ProcurementDetailViewModel model = new ProcurementDetailViewModel();

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { operationNumber = operationNumber, procurementId, tabName = tabName, errorMessage = errorMessage });
            }

            switch (tabName)
            {
                case ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL:
                    model = GetDetailsData(procurementId);
                    break;
                case ProcurementDetailNavigation.TAB_NAME_CHECKLIST:
                    model = GetChecklistData(procurementId);
                    break;
                case ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS:
                    model = GetBindingDocData(procurementId, packageId);
                    break;
                case ProcurementDetailNavigation.TAB_NAME_PARTICIPANTS:
                    model = GetParticipantsData(procurementId);
                    break;
                case ProcurementDetailNavigation.TAB_NAME_LOTS:
                    model = GetLotsData(procurementId);
                    break;
                case ProcurementDetailNavigation.TAB_NAME_CONTRACTS:
                    model = GetContractsData(procurementId, contractId);
                    break;
            }

            model.ViewContainer["ActiveTab"] = tabName ?? ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL;
            return View(model);
        }

        public virtual JsonResult SaveCancelProcess(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<CancelProcessViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel = viewModel.UpdateCancelProcess(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveCancelProcess(procurementId, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveDeclareInegibility(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<DeclareIneligibilityViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel = viewModel.UpdateDeclareInegibility(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveDeclareInegibility(procurementId, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveDetails(int procurementId = 0)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ProcurementDetailViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateProcurementDetailsViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveProcurementDetail(procurementId, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", procurementId = procurementId, tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveParticipant(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementDetailViewModel>(jsonDataRequest.SerializedData);

            model.UpdateParticipant(jsonDataRequest.ClientFieldData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveProcurementParticipant(procurementId, model);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PARTICIPANTS, procurementId = procurementId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveContract(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementDetailViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;
            int? contractSelectedId = null;

            model.UpdateContract(formData);

            if (model.ContractsTab.ContractDetails != null)
            {
                contractSelectedId = model.ContractsTab.ContractDetails.ContractId;
            }

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveProcurementContracts(procurementId, model);

            if (response.IsValid)
            {
                var submitMode = formData.First(x => x.Name == "submitMode").Value;

                if (submitMode == ProcurementDetailNavigation.SUBMIT_MODE_FIRM)
                {
                    response = _procurementDetailsService.FirmProcurementContract(procurementId, model);
                }

                if (submitMode == ProcurementDetailNavigation.SUBMIT_MODE_TERMINATION)
                {
                    response = _procurementDetailsService.FirmProcurementContract(procurementId, model);
                }

                if (submitMode == ProcurementDetailNavigation.SUBMIT_AMENDMENT_MODE_FIRM)
                {
                    response = _procurementDetailsService.ConfirmAmendmentContract(procurementId, model);
                }

                if (submitMode == ProcurementDetailNavigation.SUBMIT_AMENDMENT_MODE_REQUEST)
                {
                    response = _procurementDetailsService.RequestAmendmentContract(procurementId, model);
                }
            }

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = contractSelectedId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveChecklist(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementDetailViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;
            model.UpdateChecklist(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveProcurementChecklist(procurementId, model);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CHECKLIST, procurementId = procurementId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveBiddingDocs(int procurementId)
        {
            var responseView = new SaveResponse() { IsValid = true };
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementDetailViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;
            int? packageId = null;

            model.UpdateBiddingDocuments(formData);

            if (model.BiddingDocTab.PackageSelected != null)
            {
                packageId = model.BiddingDocTab.PackageSelected.PackageId;
            }

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_DETAIL_URL + procurementId, IDBContext.Current.Operation, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementDetailsService.SaveProcurementBiddingDoc(procurementId, model);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = packageId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual ActionResult InitBiddingWorkflow(string operationNumber, int procurementId, string entityType, int entityId)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-{1}-{2}", ProcurementDetailNavigation.WORKFLOW_TYPE_BIDDING, arrayWorkflowInfo[1], DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("ChangeStatusInitialBidding", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = arrayWorkflowInfo[1] }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = arrayWorkflowInfo[1] }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult ChangeStatusInitialBidding(string operationNumber, int procurementId = 0, string tabName = null, int? packageId = null)
        {
            var statusUnderReview = _enumMappingService.GetMappingCode(BiddingPackageStatusEnum.BiddingUnder);
            var statusAmendmentUnderReview = _enumMappingService.GetMappingCode(BiddingPackageStatusEnum.BiddingAmedment);
            var statusUnderReviewDoc = _enumMappingService.GetMappingCode(NonObjectionStatusEnum.UnderReview);
            var statusPendingPublicationDoc = _enumMappingService.GetMappingCode(NonObjectionStatusEnum.NonPendPublication);
            var response = _procurementDetailsService.ChangeStatusPackageBidding(packageId, statusUnderReview, statusUnderReviewDoc, statusAmendmentUnderReview, statusPendingPublicationDoc);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = packageId, errorMessage = response.ErrorMessage });
        }

        public virtual ActionResult InitTerminatedConctractWorkflow(string operationNumber, int procurementId, string entityType, int entityId)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-{1}-{2}", ProcurementDetailNavigation.WORKFLOW_TYPE_CONTRACT_TERMINATED, arrayWorkflowInfo[1], DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("ChangeStatusInitialTerminatedContract", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = arrayWorkflowInfo[1] }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = arrayWorkflowInfo[1] }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult ChangeStatusInitialTerminatedContract(string operationNumber, int procurementId = 0, string tabName = null, int? contractId = null)
        {
            var response = _procurementDetailsService.ChangeStatusTerminatedContract(contractId);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = contractId, errorMessage = response.ErrorMessage });
        }

        public virtual ActionResult InitContractAmendmentWorkflow(string operationNumber, int procurementId, string entityType, int entityId)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-{1}-{2}", ProcurementDetailNavigation.WORKFLOW_TYPE_CONTRACT_AMENDMENT, arrayWorkflowInfo[1], DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("ChangeStatusInitialContractAmendment", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = arrayWorkflowInfo[1] }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = arrayWorkflowInfo[1] }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult ChangeStatusInitialContractAmendment(string operationNumber, int procurementId = 0, string tabName = null, int? contractId = null)
        {
            var response = _procurementDetailsService.ChangeStatusAmendmentContract(contractId);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_CONTRACTS, procurementId = procurementId, contractId = contractId, errorMessage = response.ErrorMessage });
        }

        public virtual ActionResult InitCancelWorkflow(string operationNumber, int procurementId, string entityType, int entityId, string reason)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-0-{1}", ProcurementDetailNavigation.WORKFLOW_TYPE_CANCEL, DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("SaveCancelInitialCancelWF", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId, reason = reason }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult InitRequestForPublishing(string operationNumber, int procurementId, string entityType, int entityId, int publishtypeId)
        {
            var arrayWorkflowInfo = entityType.Split(';');
            var workflowCode = GetPublishType(publishtypeId);

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-{1}-{2}", workflowCode, arrayWorkflowInfo[1], DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("ChangeStatusRequestForPublishing", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = arrayWorkflowInfo[1] }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = arrayWorkflowInfo[1] }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult ChangeStatusRequestForPublishing(string operationNumber, int procurementId = 0, string tabName = null, int? packageId = null)
        {
            var response = _procurementDetailsService.ChangeStatusPublishing(packageId);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = packageId, errorMessage = response.ErrorMessage });
        }

        public virtual ActionResult SaveCancelInitialCancelWF(string operationNumber, int procurementId = 0, string tabName = null, string reason = null)
        {
            var viewModel = new CancelProcessViewModel() { Reason = reason != null ? reason : string.Empty };
            var response = _procurementDetailsService.SaveCancelProcess(procurementId, viewModel);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId, errorMessage = response.ErrorMessage });
        }

        public virtual ActionResult InitDeclareInegibilityWorkflow(string operationNumber, int procurementId, string entityType, int entityId, string workflowCode, int reasonType, string reason)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-0-{1}", workflowCode, DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("SaveDeclareInitialDeclareWF", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId, reasonType = reasonType, reason = reason }),
                ReturnURLCancel = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult SaveDeclareInitialDeclareWF(string operationNumber, int procurementId = 0, string tabName = null, int reasonType = 0, string reason = null)
        {
            var viewModel = new DeclareIneligibilityViewModel() { Reason = reason != null ? reason : string.Empty, IneligibilityReasonTypeId = reasonType };
            var response = _procurementDetailsService.SaveDeclareInegibility(procurementId, viewModel);

            return RedirectToAction("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_PROCUREMENT_DETAIL, procurementId = procurementId, errorMessage = response.ErrorMessage });
        }
        #endregion

        #region Calls Ajax

        public virtual ActionResult GetContractDetailsRead(int procurementId, int contractId)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;

            var response = _procurementDetailsService.GetContractDetails(procurementId, contractId);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            html = this.RenderRazorViewToString("ReadPartial/Contracts/ContractDetailsPartial", response.Model);

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual ActionResult GetContractDetailsEdit(int procurementId, int contractId)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;

            var response = _procurementDetailsService.GetContractDetails(procurementId, contractId);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            html = this.RenderRazorViewToString("EditPartial/Contracts/ContractDetailsPartial", response.Model);

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual JsonResult GetBidderIsCurrentUsed(int bidderId = 0)
        {
            var response = _procurementDetailsService.GetBidderIsCurrentUsed(bidderId);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetPackageDetail(int procurementId, int bidPackageConfProcId, bool isEditMode = false)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;
            var response = _procurementDetailsService.GetPackageDetail(procurementId, bidPackageConfProcId);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (isEditMode)
            {
                html = this.RenderRazorViewToString("EditPartial/BiddingDocuments/BiddingDocTablePartial", response.Model);
            }
            else
            {
                if (response.Model.CanModifyPackage)
                {
                    html = this.RenderRazorViewToString("ReadPartial/BiddingDocuments/BiddingDocPartial", response.Model);
                }
                else
                {
                    html = this.RenderRazorViewToString("ReadPartial/BiddingDocuments/BiddingDocReadPartial", response.Model);
                }
            }

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual JsonResult FindBiddersNameNationality(string filter)
        {
            var response = _procurementDetailsService.SearchBiddersNameNationality(filter);

            return new JsonResult { Data = response.Model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult FindParticipantsNameNationality(string filter)
        {
            var response = _procurementDetailsService.SearchParticipantsNameNationality(filter);

            return new JsonResult { Data = response.Model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult GetLocation(string literal)
        {
            var response = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(literal))
            {
                if (!response.ContainsKey(literal))
                {
                    response.Add(literal, Localization.GetText(literal));
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ChangeStatusDoc(int procurementId, int SGPBidPackageConfProcId)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;
            var response = _procurementDetailsService.PublishBidPackage(SGPBidPackageConfProcId);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = SGPBidPackageConfProcId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult AddResponseBankDoc(int procurementId, int sgpBidPackageConfProcId, string documenNumber, string descriptionDoc)
        {
            var responseView = new SaveResponse { IsValid = true };
            var documentLink = GetDocumentLink(documenNumber);
            var response = _procurementDetailsService.AddResponseBankDoc(procurementId, sgpBidPackageConfProcId, documentLink, documenNumber, descriptionDoc);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("Read", "ProcurementDetail", new { area = "SGP", tabName = ProcurementDetailNavigation.TAB_NAME_BINDING_DOCUMENTS, procurementId = procurementId, packageId = sgpBidPackageConfProcId });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult AddResponseBankContractDoc(int procurementId, int contractId, string documenNumber, string descriptionDoc)
        {
            var responseView = new SaveResponse { IsValid = true };
            var documentLink = GetDocumentLink(documenNumber);
            var html = string.Empty;

            var response = _procurementDetailsService.AddResponseBankContractDoc(procurementId, contractId, documentLink, documenNumber, descriptionDoc);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            html = this.RenderRazorViewToString("ReadPartial/Contracts/ContractDetailsPartial", response.Model);

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual ActionResult CalculateUSD(decimal amount, int currency)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;
            var response = _procurementDetailsService.CalculateUSD(currency, amount);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            return Json(response);
        }

        /*** DEBUG AscUpdateStatus **/

        public virtual ActionResult ChangeStatusExecution()
        {
            _ascUpdateStatusService.ChangeStatusExecution(DateTime.Today);
            return Json(string.Empty);
        }

        public virtual ActionResult ChangeStatusFinished()
        {
            _ascUpdateStatusService.ChangeStatusFinished(DateTime.Today);
            return Json(string.Empty);
        }

        public virtual ActionResult ChangeContractStatus()
        {
            _ascUpdateStatusService.ChangeContractStatus(DateTime.Today);
            return Json(string.Empty);
        }

        #endregion

        #region Private Methods

        private ProcurementDetailViewModel GetReadData(int procurementId, bool isEditMode = false, int? contractId = null, int? packageId = null)
        {
            var response = _procurementDetailsService.GetProcurementTabs(procurementId, contractId, packageId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetDetailsData(int procurementId)
        {
            var response = _procurementDetailsService.GetProcurementDetail(procurementId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetChecklistData(int procurementId)
        {
            var response = _procurementDetailsService.GetProcurementCheckList(procurementId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetBindingDocData(int procurementId, int? packageId = null)
        {
            var response = _procurementDetailsService.GetProcurementBiddingDoc(procurementId, packageId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetParticipantsData(int procurementId)
        {
            var response = _procurementDetailsService.GetProcurementParticipant(procurementId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetLotsData(int procurementId)
        {
            var response = _procurementDetailsService.GetProcurementLots(procurementId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementDetailViewModel GetContractsData(int procurementId, int? contractId = null)
        {
            var response = _procurementDetailsService.GetProcurementContracts(procurementId, contractId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private GenericContainer SetViewBagErrorMessageInvalidResponse(ResponseBase response, GenericContainer container)
        {
            if (container == null)
            {
                container = new GenericContainer();
            }

            container.Add("IsValid", response.IsValid);

            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    var message = HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                    container.Add("ErrorMessage", message);
                }
            }

            return container;
        }

        private string GetDocumentLink(string documenNumber)
        {
            return MW.Domain.EntityHelpers.DownloadDocumentHelper.GetDocumentLink(documenNumber);
        }

        private string GetPublishType(int publishtypeId)
        {
            var result = string.Empty;

            var workflowSPNId = _enumMappingService.GetMappingCode(WorkflowTypeEnum.WorkflowSPNPublish);
            var workflowEOIId = _enumMappingService.GetMappingCode(WorkflowTypeEnum.WorkflowEOIPublish);
            var workflowNOAId = _enumMappingService.GetMappingCode(WorkflowTypeEnum.WorkflowNOAPublish);

            if (publishtypeId == workflowSPNId)
            {
                result = ProcurementDetailNavigation.WORKFLOW_TYPE_SPN_PUBLISHING;
            }
            else if (publishtypeId == workflowEOIId)
            {
                result = ProcurementDetailNavigation.WORKFLOW_TYPE_EOI_PUBLISHING;
            }
            else if (publishtypeId == workflowNOAId)
            {
                result = ProcurementDetailNavigation.WORKFLOW_TYPE_NOA_PUBLISHING;
            }

            return result;
        }
        #endregion
    }
}
