using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.ESW_CIPModule.Enums;
using IDB.MW.Application.ESW_CIPModule.Helpers;
using IDB.MW.Application.ESW_CIPModule.Services;
using IDB.MW.Application.ESW_CIPModule.ViewModels;
using IDB.MW.Application.ESW_CIPModule.Enums.K2IntegrationEnums;
using IDB.Presentation.MVC4.Areas.ESW_CIP.Models;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.ESW_CIPModule.Services.K2Integration;
using IDB.MW.Application.ESW_CIPModule.Services.CommentsTaskService;
using IDB.MW.Application.ESW_CIPModule.ViewModels.K2Workflow;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Entities;
using IDB.MW.Application.ESW_CIPModule.Services.DocumentsTaskService;

namespace IDB.Presentation.MVC4.Areas.ESW_CIP.Controllers
{
    public partial class WorkflowK2Controller : BaseController
    {
        #region Fields
        private const string ACCEPT_WORKFLOW = "AcceptWorkflow";
        private const string REJECT_WORKFLOW = "RejectWorkflow";
        private const string ECO_TL_SUBMITTED = "ECO_TL_SUBMITTED";
        private const string ECO_FUN_COOR_ACCEPTED = "ECO_FUN_COOR_ACCEPTED";
        private const string ECO_DRAFT = "ECO_DRAFT";
        private readonly IESWCIPService _eswcipService;
        private readonly IOperationRepository _operationRepository;
        private readonly IOperationTeamDataRepository _operationTeamDataRepository;        
        private readonly IK2DataService _k2DataService;
        private readonly IK2IntegrationService _k2IntegrationService;
        private ICommentsTaskService _commentsTaskService;
        private IDocumentsTaskService _documentsTaskService;
       #endregion

        #region Contructors
        public WorkflowK2Controller(
            IK2DataService k2DataService,
            IK2IntegrationService k2IntegrationService,
            ICommentsTaskService commentsTaskService,
            IDocumentsTaskService documentsTaskService,
            IESWCIPService eswcipService,
            IOperationRepository operationRepository,
            IOperationTeamDataRepository operationTeamDataRepository)
        {
            _k2DataService = k2DataService;
            _k2IntegrationService = k2IntegrationService;
            _commentsTaskService = commentsTaskService;
            _documentsTaskService = documentsTaskService;
            _eswcipService = eswcipService;
            _operationRepository = operationRepository;
            _operationTeamDataRepository = operationTeamDataRepository;
        }
        #endregion

        public virtual ActionResult ECOWorkflow(string operationNumber, TaskDataModel model)
        {
            var pmodel = _eswcipService.GetProposal(model.Parameters[K2ECOHelpers.TagProposalId].ToString(), model.Parameters[K2ECOHelpers.TagProposalYear].ToString(), model.Parameters[K2ECOHelpers.TagProposalType].ToString());
            var operation = _operationRepository.GetOne(o => o.OperationNumber.Equals(operationNumber));
            var userData = _operationTeamDataRepository.GetOne(o => o.OperationId.Equals(operation.OperationId) && o.UserName.Equals(IDBContext.Current.UserName));
            string userDataRole = string.Empty;
            string userDataOrganizationalUnit = string.Empty;

            if (userData == null)
            {
                userDataRole = string.Empty;
                userDataOrganizationalUnit = string.Empty;
            }
            else
            {
                userDataRole = userData.UserRole == null ? string.Empty : LoadNameInProperLanguage(userData.UserRole);
                userDataOrganizationalUnit = userData.OrganizationalUnit == null ? string.Empty : LoadNameInProperLanguage(userData.OrganizationalUnit);
            }

            var modelTaskView = new WorkflowViewModels
            {
                ProposalId = pmodel.Proposal.ProposalId,
                OperationModality = pmodel.Proposal.ProposalType,
                UserName = IDBContext.Current.UserName,
                Role = model.UserProfile,
                OrganizationalUnit = userDataOrganizationalUnit,
                Instructions = _k2IntegrationService.getInstructions(model.TaskTypeCode,
                pmodel.Proposal.ProposalType,
                !model.TaskTypeCode.Equals(K2ECOHelpers.WF_ECO_001_T1) ? model.Parameters[K2ECOHelpers.TagRole].ToString() : string.Empty),

                UserComments = pmodel.Proposal.Comments != null ? pmodel.Proposal.Comments : new List<CommentsViewModel>(),
                TaskDataModel = model,
                Documents = pmodel.Proposal.DocumentProposalTasks,
                StudiesCommittee = pmodel.Proposal.StudiesCommittee
            };

            ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(modelTaskView);
            return View(modelTaskView);
        }

        public virtual JsonResult SaveWorkFlowECO(string operationNumber)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<WorkflowViewModels>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateCreationWorkFlowViewModel(jsonDataRequest.ClientFieldData);

                var workFlow = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));
                var studiesCommitee = jsonDataRequest.ClientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("studiesComitte"));

                var parameters = new Dictionary<string, object>();
                var sharepoint = ConfigurationManager.AppSettings["BasePath"];

                var linktask = string.Format(
                    "{0}/operation/{1}/Pages/Default?idTask=nro", sharepoint, operationNumber);

                string userName = string.IsNullOrEmpty(viewModel.UserName) ? IDBContext.Current.UserName : viewModel.UserName;
                Logger.GetLogger().WriteDebug("SaveController", "Method: SaveWorkflowECO : UserName: " + userName);

                parameters.Add(K2ECOHelpers.TaskUserName, userName);
                parameters.Add(K2ECOHelpers.ProposalType, viewModel.OperationModality);
                parameters.Add(K2ECOHelpers.LinkTask, linktask);

                int proposalId = Convert.ToInt32(viewModel.TaskDataModel.Parameters[K2ECOHelpers.TagProposalId]);

                int entityId = Convert.ToInt32(proposalId);

                response = _commentsTaskService.SaveCommentWorkFlowECO(viewModel, operationNumber);
                response = _documentsTaskService.SaveDocumentWorkFlowECO(viewModel, operationNumber);

                switch (workFlow.Value)
                {
                    case "SubmitWorkflow":

                        response.IsValid = _k2IntegrationService.StartAdvanceWorkflowECO(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            operationNumber,
                            K2ECOHelpers.EntityType,
                            entityId,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.AcceptWorkflow,
                            viewModel.TaskDataModel.TaskId);

                        if (response.IsValid)
                        {
                            if (viewModel.TaskDataModel.TaskTypeCode.Equals(K2ECOHelpers.WF_ECO_001_T6))
                            {
                                string roleReturned = viewModel.TaskDataModel.Parameters[K2ECOHelpers.TagRole].ToString();
                                switch (roleReturned)
                                {
                                    case "VPS Knowledge Work Coordinator":
                                        response = viewModel.OperationModality.Equals("CIP") ? _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsValidation, viewModel.OperationModality) : _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsReview, viewModel.OperationModality);
                                        break;

                                    case "VPS Final Validation":
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsValidation, viewModel.OperationModality);
                                        break;

                                    case "VPS Department Reviewer":
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.departmentReview, viewModel.OperationModality);
                                        break;

                                    case "VPS Studies Committee":
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.studiees, viewModel.OperationModality);
                                        break;
                                }
                            }
                            else
                                response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.departmentReview, viewModel.OperationModality);
                        }

                        break;

                    case "ValidateWorkflow":
                        bool? isStudiee = null;
                        if (viewModel.TaskDataModel.TaskTypeCode.Equals(K2ECOHelpers.WF_ECO_001_T3)
                            && viewModel.OperationModality.Equals("ESW")
                            && studiesCommitee != null)
                            isStudiee = Convert.ToBoolean(studiesCommitee.Value);

                        response.IsValid = _k2IntegrationService.StartAdvanceWorkflowECO(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            operationNumber,
                            K2ECOHelpers.EntityType,
                            entityId,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.AcceptWorkflow,
                            viewModel.TaskDataModel.TaskId,
                            isStudiee);

                        if (response.IsValid)
                        {
                            switch (viewModel.TaskDataModel.TaskTypeCode)
                            {
                                case K2ECOHelpers.WF_ECO_001_T2:
                                    if (viewModel.OperationModality.Equals(ESWCIPEnums.TYPE_ESW))
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsReview, viewModel.OperationModality);
                                    else
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsValidation, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T3:

                                    if (isStudiee.HasValue)
                                        if (isStudiee.Value.Equals(true))
                                            response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.studiees, viewModel.OperationModality, isStudiee.Value);
                                        else
                                            response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.validated, viewModel.OperationModality, isStudiee.Value);
                                    else
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.validated, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T4:
                                    response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsValidation, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T5:
                                    response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.validated, viewModel.OperationModality);
                                    break;
                            }
                        }

                        break;

                    case "ReturnWorkflow":
                        bool? isStud = null;
                        if (viewModel.TaskDataModel.TaskTypeCode.Equals(K2ECOHelpers.WF_ECO_001_T3)
                            && viewModel.OperationModality.Equals("ESW")
                            && studiesCommitee != null)
                            isStud = Convert.ToBoolean(studiesCommitee.Value);

                        response.IsValid = _k2IntegrationService.StartAdvanceWorkflowECO(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            operationNumber,
                            K2ECOHelpers.EntityType,
                            entityId,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.RejectWorkflow,
                            viewModel.TaskDataModel.TaskId,
                            isStud);

                        if (response.IsValid)
                            switch (viewModel.TaskDataModel.TaskTypeCode)
                            {
                                case K2ECOHelpers.WF_ECO_001_T2:
                                    response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.departmentReturned, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T3:
                                    if (isStud.HasValue)
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsReturned, viewModel.OperationModality, isStud.Value);
                                    else
                                        response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsReturned, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T4:
                                    response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.committeeReturned, viewModel.OperationModality);
                                    break;
                                case K2ECOHelpers.WF_ECO_001_T5:
                                    response = _eswcipService.ChangeStatus(proposalId, ESWCIPEnums.vpsReturned, viewModel.OperationModality);
                                    break;
                            }

                        break;
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual JsonResult IsReadyforSubmition(string proposalId)
        {
            ResponseBase response = new ResponseBase();
            response.IsValid = _eswcipService.GetIsReady(Convert.ToInt32(proposalId));
            return Json(response);
        }

        public virtual JsonResult DeleteDocument(string operationNumber, string documentNumber)
        {
            var response = _documentsTaskService.DeleteDocument(documentNumber);

            return Json(response);
        }

        private string LoadNameInProperLanguage(ConvergenceMasterData entity)
        {
            return ConvergenceMasterDataHelper.GetNameLanguage(entity, IDBContext.Current.CurrentLanguage.ToUpper());
        }
    }
}