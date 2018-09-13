using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.ESW_CIPModule.Enums;
using IDB.MW.Application.ESW_CIPModule.Enums.K2IntegrationEnums;
using IDB.MW.Application.ESW_CIPModule.Helpers;
using IDB.MW.Application.ESW_CIPModule.Messages;
using IDB.MW.Application.ESW_CIPModule.Services;
using IDB.MW.Application.ESW_CIPModule.Services.K2Integration;
using IDB.MW.Application.ESW_CIPModule.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.ESW_CIPModule;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.ESW_CIP.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.ESW_CIP.Controllers
{
    public partial class SaveController : MVC4.Controllers.ConfluenceController
    {
        private const string UrlAnnualAllocation = "/ESW_CIP/AnnualAllocation";
        private const string UrlProposalOperation = "/ESW_CIP/ProposalOperation";
        private const string CHANGE_STATUS_FIELD = "changeStatus";
        private const string SUBMIT_WORKFLOW = "SubmitWorkflow";

        private readonly IESWCIPService _eswcipService;
        private readonly IProposalOperationRepository _proposalOperationRepository;
        private readonly IWorkflowInstanceEntityRepository _workflowInstanceEntityRepository;
        private readonly IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;
        private readonly IK2IntegrationService _k2IntegrationService;

        public SaveController(
            IESWCIPService eswcipService, 
            IProposalOperationRepository proposalOperationRepository, 
            IK2IntegrationService k2IntegrationService, 
            IWorkflowInstanceEntityRepository workflowInstanceEntityRepository, 
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository)
        {
            _eswcipService = eswcipService;
            _k2IntegrationService = k2IntegrationService;
            _proposalOperationRepository = proposalOperationRepository;
            _workflowInstanceEntityRepository = workflowInstanceEntityRepository;
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
        }

        public virtual JsonResult AnnualAllocation(string operationNumber)
        {
            SaveAnnualAllocationResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper
                .DeserializeObject<AnnualAllocationContainerViewModel>(jsonDataRequest.SerializedData);

            var userName = IDBContext.Current.UserName;

            viewModel.UpdateAnnualAllocationViewModel(jsonDataRequest.ClientFieldData);

            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE, 
                UrlAnnualAllocation, 
                operationNumber, 
                userName);

            viewModel.AnnualAllocations = viewModel.AnnualAllocations.Where(x => x.HasChanges).ToList();

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveAnnualAllocationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _eswcipService.SaveAnnualAllocations(operationNumber, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlAnnualAllocation, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult ProposalOperation(string operationNumber)
        {
            SaveProposalResponse response;

            ResponseBase responseK = new ResponseBase();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper
                .DeserializeObject<ProposalViewModel>(jsonDataRequest.SerializedData);

            var userName = IDBContext.Current.UserName;

            viewModel.UpdateProposalOperationViewModel(jsonDataRequest.ClientFieldData);

            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE, 
                UrlProposalOperation, 
                operationNumber, 
                userName);

            var workFlow = jsonDataRequest.ClientFieldData
                .FirstOrDefault(o => o.Name.Equals(CHANGE_STATUS_FIELD));

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveProposalResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _eswcipService.SaveProposalOperations(operationNumber, viewModel);

                switch (workFlow.Value)
                {
                    case SUBMIT_WORKFLOW:

                        if (viewModel.IsReadyForSubmission)
                        {
                            var parameters = new Dictionary<string, object>();
                            int entityId = viewModel.ProposalId;
                            var annualAllocationToBeSaved = _proposalOperationRepository
                                .GetOne(x => x.ProposalOperationId == viewModel.ProposalId);
                            var entityWorkflow = _workflowInstanceEntityRepository
                                .GetOne(x => x.EntityId == entityId && 
                                    x.EntityType == K2ECOHelpers.EntityType);
                            int workflow_instance = entityWorkflow.WorkflowInstanceId;
                            var entityTask = _workflowInstanceTaskRepository
                                .GetByCriteria(x => x.WorkflowInstanceId == workflow_instance && 
                                    x.Status == string.Empty)
                                .ToList();
                            int taskID = entityTask != null ? 
                                entityTask.Last().WorkflowInstanceTaskId : 0;
                            string folio = entityWorkflow.WorkflowInstance.FolioId;
                            var sharepoint = ConfigurationManager.AppSettings["BasePath"];

                            var linktask = string.Format(
                                "{0}/operation/{1}/Pages/Default?idTask=nro",
                                sharepoint,
                                operationNumber);

                            parameters.Add(K2ECOHelpers.TaskUserName, userName);
                            parameters.Add(K2ECOHelpers.ProposalType, viewModel.ProposalType);
                            parameters.Add(K2ECOHelpers.LinkOpera, linktask);

                            responseK.IsValid = _k2IntegrationService.StartAdvanceWorkflowECO(
                                K2ECOHelpers.WorkflowTypeECO,
                                folio,
                                operationNumber,
                                K2ECOHelpers.EntityType,
                                entityId,
                                K2ECOHelpers.WF_ECO_001_T1,
                                parameters,
                                K2IntegrationEnumerator.GeneralActions.AcceptWorkflow,
                                taskID);

                            if (responseK.IsValid)
                            {
                                responseK = _eswcipService.ChangeStatus(
                                    entityId, 
                                    ESWCIPEnums.departmentReview, 
                                    viewModel.ProposalType);
                            }
                        }

                        break;
                }

                if (response.IsValid && responseK.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        UrlProposalOperation, 
                        operationNumber, 
                        userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult DeleteDocument(string operationNumber, string documentNumber)
        {
            var response = _eswcipService.DeleteDocument(documentNumber);

            return Json(response);
        }

        public virtual JsonResult DeleteAnnualAllocation(string operationNumber, string annualAllocationID)
        {
            var response = _eswcipService.DeleteAnnualAllocation(annualAllocationID);

            return Json(response);
        }

        public virtual JsonResult DiscardAndDeleteProposal(int proposalId)
        {
            var response = _eswcipService.DiscardAndDeleteProposal(proposalId);

            return Json(response);
        }
    }
}