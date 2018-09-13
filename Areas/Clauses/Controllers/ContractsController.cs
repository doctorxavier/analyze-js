using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Diagnostics;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Domain.Attributes;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.ClausesAndContracts.Services.RulesEngine;
using IDB.MW.Application.ClausesAndContractsModule.Services.Clauses;
using IDB.MW.Application.ClausesAndContractsModule.Services.Contracts.Interfaces;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Reformulation.Services;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Architecture.Clauses;
using IDB.MW.Domain.Models.Architecture.Clauses.Messages;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Clauses;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Clauses;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVCExtensions;
using IDB.MW.GenericWorkflow.Enums;
using IDB.MW.GenericWorkflow.Messages;
using IDB.MW.GenericWorkflow.Services;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Architecture.Clauses;
using IDB.MW.Application.ClausesAndContractsModule.Enums;
using IDB.MW.Domain.Entities;
using IDB.MW.GenericWorkflow.Helpers;

namespace IDB.Presentation.MVC4.Areas.Clauses.Controllers
{
    public partial class ContractsController : BaseController
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class MultipleButtonAttribute : ActionNameSelectorAttribute
        {
            public string Name { get; set; }
            public string Argument { get; set; }

            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                var isValidName = false;
                var keyValue = string.Format("{0}:{1}", Name, Argument);
                var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

                if (value != null)
                {
                    controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                    isValidName = true;
                }

                return isValidName;
            }
        }

        public virtual GlobalModelRepository GlobalModelRepositoryClient { get; set; }
        public virtual IK2ServiceProxy K2ServiceProxy { get; set; }
        private IOperationModelRepository _operation = null;
        public virtual IOperationModelRepository ClauseRepository
        {
            get { return _operation; }
            set { _operation = value; }
        }

        private IConvergenceMasterDataModelRepository _ClientMasterDataModelRepository = null;
        public virtual IConvergenceMasterDataModelRepository ClientMasterDataModelRepository
        {
            get { return _ClientMasterDataModelRepository; }
            set { _ClientMasterDataModelRepository = value; }
        }

        private IDB.MW.Domain.Contracts.DomainServices.ILoanOperationDataService _clientLoan = null;
        public virtual IDB.MW.Domain.Contracts.DomainServices.ILoanOperationDataService ClientLoan
        {
            get { return _clientLoan; }
            set { _clientLoan = value; }
        }

        private IGlobalModelRepository _Global = null;
        public virtual IGlobalModelRepository Global
        {
            get { return _Global; }
            set { _Global = value; }
        }

        private IRuleEvaluatorService _RuleService = null;
        public virtual IRuleEvaluatorService RuleService
        {
            get { return _RuleService; }
            set { _RuleService = value; }
        }

        private IBasicDataModelRepository _operationData = null;
        public virtual IBasicDataModelRepository OperationData
        {
            get { return _operationData; }
            set { _operationData = value; }
        }

        private IConvergenceMasterDataModelRepository _ClientMasterDataRepository = null;
        public virtual IConvergenceMasterDataModelRepository ClientMasterDataRepository
        {
            get { return _ClientMasterDataRepository; }
            set { _ClientMasterDataRepository = value; }
        }

        private readonly IAttributesEngineService _attributesEngineService;

        private readonly IClauseBusinessLogicRuleService _businessRuleService;
        private readonly IRevolvingFundOperationModelRepositoryService _rFOperationModelRepositoryService;
        private readonly IReformulationService _reformulationService;
        private readonly ICatalogService _catalogService;
        private readonly IClauseService _clauseService;
        private readonly IWorkflowManagerService _workflowManager;

        public ContractsController(IClauseBusinessLogicRuleService businessRuleService,
            IRevolvingFundOperationModelRepositoryService rFOperationModelRepositoryService,
            IReformulationService reformulationService,
            IAttributesEngineService attributesEngineService,
            IClauseService clauseService,
            ICatalogService catalogService,
            IWorkflowManagerService workflowManager)
        {
            _businessRuleService = businessRuleService;
            _rFOperationModelRepositoryService = rFOperationModelRepositoryService;
            _reformulationService = reformulationService;
            _attributesEngineService = attributesEngineService;
            _catalogService = catalogService;
            _clauseService = clauseService;
            _workflowManager = workflowManager;
        }

        // GET: /Clauses/Contracts/
        [ExceptionHandling]
        public virtual ActionResult Index(
            string operationNumber,
            int? clauseTypeId,
            int? clauseStatusId,
            int? clauseCategoryId,
            string clauseNumber,
            bool? isSpecial,
            DateTime? expirationDateFrom,
            DateTime? expirationDateTo,
            MessageSendRequestCode state = 0,
            string messageK2 = "",
            string locatetop = "",
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            ViewBag.LastDisbursment = false;
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["LastDisbursment"] = message;
                ViewBag.LastDisbursment = true;
            }

            var operations = ClauseRepository.GetRelatedContractsByOperation(
                                       operationNumber,
                                       clauseTypeId,
                                       clauseStatusId,
                                       clauseCategoryId,
                                       clauseNumber,
                                       isSpecial,
                                       expirationDateFrom,
                                       expirationDateTo,
                                       IDBContext.Current.CurrentLanguage);

            ReformulationContractName(operations);

            ViewBag.clauseTypeId = clauseTypeId;
            ViewBag.clauseStatusId = clauseStatusId;
            ViewBag.clauseCategoryId = clauseCategoryId;
            ViewBag.clauseNumber = clauseNumber;
            ViewBag.isSpecial = isSpecial;
            ViewBag.expirationDateFrom = expirationDateFrom;
            ViewBag.expirationDateTo = expirationDateTo;

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CLAUSE_FINAL_STATUS",
                "VALIDATION_STAGE",
                "LOAN_STATUS"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);
            this.ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            this.ViewBag.ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            this.ViewBag.ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            this.ViewBag.ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];
            this.ViewBag.ListClauseFinalStatus = masterDataDetail["CLAUSE_FINAL_STATUS"];
            this.ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];
            ViewBag.ListLoanStatus = masterDataDetail["LOAN_STATUS"];

            ViewBag.ListClauseCategory = GetAllCategories(operations);

            List<Tuple<int, string>> OverallStagesOperations = new List<Tuple<int, string>>();
            this.ViewBag.TypeBank = false;

            for (int i = 0; i < operations.Count; i++)
            {
                int Operation = operations[i].OperationId;
                var OverallStages = ClauseRepository.getOperationOverallStages(operations[i].mainOperationNumber, "EN");
                var DescripcionOverallStages = OverallStages.OverallStage;
                this.ViewBag.isTCOperation = operations[i].OperationType == OperationType.TCP;

                if (operations[i].OperationType == OperationType.TCP)
                {
                    var attributesExecuteBy = _attributesEngineService.GetAttributeValueByCode(operationNumber, AttributeCode.EXECUTED_BY);
                    if (attributesExecuteBy.IsValid && attributesExecuteBy.Value != null)
                    {
                        var convergenceMasterCodeByIdResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(
                    int.Parse(attributesExecuteBy.FormAttribute.InitialValue));

                        this.ViewBag.TypeBank = convergenceMasterCodeByIdResponse.Code == AttributeValue.BANK;
                    }
                }

                OverallStagesOperations.Add(Tuple.Create(Operation, DescripcionOverallStages));
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(operationNumber);
            decimal currentAmount = ClauseRepository.CalculateCurrentAmount(operations.First().OperationId, 1);
            operations.First().Normative = _clauseService
                .GetNormative(operations.First().OperationNumber, currentAmount).GetStringValue();

            ViewBag.ListOverallStagesOperations = OverallStagesOperations;

            ViewBag.locatetop = locatetop;
            if (locatetop.Split('|').Length < 2)
            {
                ViewBag.locatetop = ViewBag.locatetop + "|NO|NO";
            }

            if (operations != null)
            {
                if (state != 0)
                {
                    MessageConfiguration message = MessageHandler.SetMessageSendRequest(state, false, 2, messageK2);
                    ViewData["message"] = message;
                }

                return View(operations);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditContractToTrack(int contractId, string mainOperationNumber)
        {
            var contractModel = ClauseRepository.GetContractById(contractId);
            contractModel.ContractClauseStatId = ClientMasterDataModelRepository.GetMasterDataId("CONTRACT_CLAUSE_STATUS", "TRACK");
            var clauseStatusTrackId = ClientMasterDataModelRepository.GetMasterDataId("CLAUSE_STATUS", "CL_TR");
            List<int> ListValidStatesToChangeStateClausdeIndividual = new List<int>();
            ListValidStatesToChangeStateClausdeIndividual.Add(ClientMasterDataModelRepository.GetMasterDataId("CLAUSE_STATUS", "CL_DRAFT"));

            foreach (var itemClause in contractModel.Clauses)
            {
                foreach (var itemClauseIndividual in itemClause.ClauseIndividuals)
                {
                    if (ListValidStatesToChangeStateClausdeIndividual.Any(validStatus => validStatus == itemClauseIndividual.StatusId))
                    {
                        itemClauseIndividual.StatusId = clauseStatusTrackId;
                    }
                }
            }

            ClauseRepository.SaveContractStatus(contractModel, IDBContext.Current.UserName);

            return RedirectToAction("Index", new { operationNumber = mainOperationNumber });
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditContractToRecord(int contractId, string mainOperationNumber)
        {
            var contractModel = ClauseRepository.GetContractById(contractId);
            contractModel.ContractClauseStatId = ClientMasterDataModelRepository.GetMasterDataId("CONTRACT_CLAUSE_STATUS", "RECORD");
            ClauseRepository.SaveContractStatus(contractModel, IDBContext.Current.UserName);

            return RedirectToAction("Index", new { operationNumber = mainOperationNumber });
        }

        public virtual ActionResult DetailsEligibilityById(string operationNumber, int contractId)
        {
            var contract = _clauseService.GetContractById(contractId).Model;

            return RedirectToAction("DetailsEligibilityRequest", new
            {
                operationId = contract.OperationId,
                contractId = contractId,
                mainOperationNumber = contract.Operation.OperationNumber
            });
        }

        public virtual ActionResult DetailsEligibilityRequestEdit(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);

            bool canBeRequest = true;
            bool showRequest = true;

            string[] masterDataItems = new string[]
            {
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "VALIDATION_STAGE"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            int financialPriorConditionId = masterDataDetail["CLAUSE_TYPE"]
                    .FirstOrDefault(x => x.Code == "FINANCIA-PC").ConvergenceMasterDataId;
            int sectorialPriorConditionId = masterDataDetail["CLAUSE_TYPE"]
                                .FirstOrDefault(x => x.Code == "SECTOR-PC").ConvergenceMasterDataId;
            int LegalReportConditionId = masterDataDetail["CLAUSE_TYPE"]
                                .FirstOrDefault(x => x.Code == "LEGALREP").ConvergenceMasterDataId;
            var ListClauseCategory = GetAllCategories(new List<OperationRelatedModel> { operationModel });
            var ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            var ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            var ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];
            var ListValidationStage = masterDataDetail["VALIDATION_STAGE"];

            var validationStageId = operationModel.Contracts[0].ValidationStageId;
            if (validationStageId != null)
            {
                this.ViewBag.ContractStatus = ClientMasterDataModelRepository.GetMasterDataNameById((int)validationStageId, Globals.NeutralLanguage.ToUpper());
            }

            var ListClauseFilter = operationModel.Contracts[0].Clauses
                .Where(itemClause =>
                    itemClause.ClauseTypeId == financialPriorConditionId ||
                    itemClause.ClauseTypeId == sectorialPriorConditionId ||
                    itemClause.ClauseTypeId == LegalReportConditionId).ToList();

            if (validationStageId != null && validationStageId > 0)
            {
                int finalValidationStage = masterDataDetail["VALIDATION_STAGE"]
                              .FirstOrDefault(x => x.Code == ClauseConstants.APPROVED_CLAUSE_STATUS)
                              .ConvergenceMasterDataId;
                if (finalValidationStage == (int)validationStageId)
                {
                    showRequest = false;
                }
            }

            canBeRequest = ClauseRepository.ValidateFinalStatusForClausesPriorCondition(ListClauseFilter);

            ViewBag.canberequest = canBeRequest;
            ViewBag.showrequest = showRequest;

            foreach (var itemClause in ListClauseFilter)
            {
                foreach (var itemClauseIndividual in itemClause.ClauseIndividuals)
                {
                    var StatusComponent = ListClauseStatus.First(Stat => Stat.ConvergenceMasterDataId == itemClauseIndividual.StatusId);
                    var StageComponent = ListValidationStage.First(Stage => Stage.ConvergenceMasterDataId == itemClauseIndividual.ValidationStageId);
                    string StatusClause = string.Empty;

                    if (StageComponent.Code == "CL_REV" || StageComponent.Code == "CL_REV_LMS" || StageComponent.Code == "CL_REV_EXT")
                    {
                        StatusClause = StageComponent.NameEn;
                    }
                    else
                    {
                        StatusClause = StatusComponent.NameEn;
                    }

                    operationModel.ListHibridClauseClauseIndividual.Add(new HibridClauseClauseIndividual
                    {
                        ClauseNumber = itemClause.ClauseNumber,
                        Description = itemClause.Description,
                        Category = Localization.GetText(ListClauseCategory.First(cat => cat.ConvergenceMasterDataId == itemClause.CategoryId).NameEn),
                        Type = Localization.GetText(ListClauseType.First(type => type.ConvergenceMasterDataId == itemClause.ClauseTypeId).NameEn),
                        Special = itemClause.IsSpecial ? Localization.GetText("Yes") : Localization.GetText("No"),
                        Location = Localization.GetText(ListClauseLocation.First(loc => loc.ConvergenceMasterDataId == itemClause.LocationId).NameEn),
                        Expiration = itemClauseIndividual.CurrentExpirationDate != null ? Convert.ToDateTime(itemClauseIndividual.CurrentExpirationDate).ToString("dd MMM yyyy") : string.Empty,
                        Clauses = itemClauseIndividual.SubmissionDate != null ? Convert.ToDateTime(itemClauseIndividual.SubmissionDate).ToString("dd MMM yyyy") : string.Empty,
                        Approval = itemClauseIndividual.ValidateDate,
                        Status = Localization.GetText(StatusClause)
                    });
                }
            }

            string extensionId = string.Empty;

            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString()) ? string.Empty : operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "2",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);

            var roles = UserProfileValidators_.Split('|');

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];

            operationModel.ListHibridClauseClauseIndividual = operationModel.ListHibridClauseClauseIndividual.OrderByDescending(or => or.Approval).ToList();

            return View(operationModel);
        }

        public virtual ActionResult DetailsEligibilityRequest(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);

            bool canBeRequest = true;
            bool showRequest = true;

            string[] masterDataItems = new string[]
            {
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "VALIDATION_STAGE"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            int financialPriorConditionId = masterDataDetail["CLAUSE_TYPE"]
                                .FirstOrDefault(x => x.Code == "FINANCIA-PC").ConvergenceMasterDataId;
            int sectorialPriorConditionId = masterDataDetail["CLAUSE_TYPE"]
                                .FirstOrDefault(x => x.Code == "SECTOR-PC").ConvergenceMasterDataId;
            int LegalReportConditionId = masterDataDetail["CLAUSE_TYPE"]
                                .FirstOrDefault(x => x.Code == "LEGALREP").ConvergenceMasterDataId;
            var ListClauseCategory = GetAllCategories(new List<OperationRelatedModel> { operationModel });
            var ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            var ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            var ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];
            var ListValidationStage = masterDataDetail["VALIDATION_STAGE"];

            var validationStageId = operationModel.Contracts[0].ValidationStageId;
            if (validationStageId != null)
            {
                this.ViewBag.ContractStatus = ClientMasterDataModelRepository.GetMasterDataNameById((int)validationStageId, Globals.NeutralLanguage.ToUpper());
            }

            var ListClauseFilter = operationModel.Contracts[0].Clauses
                .Where(itemClause =>
                    itemClause.ClauseTypeId == financialPriorConditionId ||
                    itemClause.ClauseTypeId == sectorialPriorConditionId ||
                    itemClause.ClauseTypeId == LegalReportConditionId).ToList();

            if (validationStageId != null && validationStageId > 0)
            {
                int finalValidationStage = masterDataDetail["VALIDATION_STAGE"]
                              .FirstOrDefault(x => x.Code == ClauseConstants.APPROVED_CLAUSE_STATUS)
                              .ConvergenceMasterDataId;
                if (finalValidationStage == (int)validationStageId)
                {
                    showRequest = false;
                }
            }

            canBeRequest = ClauseRepository.ValidateFinalStatusForClausesPriorCondition(ListClauseFilter);

            ViewBag.canberequest = canBeRequest;
            ViewBag.showrequest = showRequest;

            foreach (var itemClause in ListClauseFilter)
            {
                foreach (var itemClauseIndividual in itemClause.ClauseIndividuals)
                {
                    var StatusComponent = ListClauseStatus.First(Stat => Stat.ConvergenceMasterDataId == itemClauseIndividual.StatusId);
                    var StageComponent = ListValidationStage.First(Stage => Stage.ConvergenceMasterDataId == itemClauseIndividual.ValidationStageId);
                    string StatusClause = string.Empty;

                    if (StageComponent.Code == "CL_REV" || StageComponent.Code == "CL_REV_LMS" || StageComponent.Code == "CL_REV_EXT")
                    {
                        StatusClause = StageComponent.NameEn;
                    }
                    else
                    {
                        StatusClause = StatusComponent.NameEn;
                    }

                    operationModel.ListHibridClauseClauseIndividual.Add(new HibridClauseClauseIndividual
                    {
                        ClauseNumber = itemClause.ClauseNumber,
                        Description = itemClause.Description,
                        Category = Localization.GetText(ListClauseCategory.First(cat => cat.ConvergenceMasterDataId == itemClause.CategoryId).NameEn),
                        Type = Localization.GetText(ListClauseType.First(type => type.ConvergenceMasterDataId == itemClause.ClauseTypeId).NameEn),
                        Special = itemClause.IsSpecial ? Localization.GetText("Yes") : Localization.GetText("No"),
                        Location = Localization.GetText(ListClauseLocation.First(loc => loc.ConvergenceMasterDataId == itemClause.LocationId).NameEn),
                        Expiration = itemClauseIndividual.CurrentExpirationDate != null ? Convert.ToDateTime(itemClauseIndividual.CurrentExpirationDate).ToString("dd MMM yyyy") : string.Empty,
                        Clauses = itemClauseIndividual.SubmissionDate != null ? Convert.ToDateTime(itemClauseIndividual.SubmissionDate).ToString("dd MMM yyyy") : string.Empty,
                        Approval = itemClauseIndividual.ValidateDate,
                        Status = Localization.GetText(StatusClause)
                    });
                }
            }

            string extensionId;
            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString()) ? string.Empty : operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "2",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);

            var roles = UserProfileValidators_.Split('|');

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];

            operationModel.ListHibridClauseClauseIndividual = operationModel.ListHibridClauseClauseIndividual.OrderByDescending(or => or.Approval).ToList();

            return View(operationModel);
        }

        public virtual ActionResult DetailsEligibilityApprovalRequest(
            int operationId,
            int contractId,
            string mainOperationNumber,
            MessageSendRequestCode state = 0,
            string messageK2 = null)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);

            OperationRelatedModel ModelOperation = operationModel;
            ViewBag.OperationRelatedModel = ModelOperation;

            string extensionId;

            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString()) ? string.Empty : operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "2",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);

            var roles = UserProfileValidators_.Split('|');

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            var ListLanguages = ClauseRepository.GetLanguages(Language.EN);
            ViewBag.ListLanguages = new SelectList(ListLanguages, "convergenceMasterDataId", "Name");
            ViewBag.ListValidationStage = ClientMasterDataModelRepository.GetMasterDataModels("VALIDATION_STAGE");

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses).SelectMany(x => x.ClauseIndividuals).SelectMany(x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;
            foreach (var item in listDocuments)
            {
                if (item == "ELEGIBILITY LETTER")
                {
                    FlagLetter = true;
                }
            }

            ViewBag.Letter = FlagLetter;

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(state, false, 2, messageK2);
                ViewData["message"] = message;
            }

            ViewBag.ListValidationStage = ClientMasterDataModelRepository.GetMasterDataModels("VALIDATION_STAGE");
            return View(operationModel);
        }

        [HasPermission(Permissions = "Eligibility Write")]
        public virtual ActionResult EditEligibilityApprovalRequest(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);

            OperationRelatedModel ModelOperation = operationModel;
            ViewBag.OperationRelatedModel = ModelOperation;

            string extensionId;
            try
            {
                extensionId =
                    string.IsNullOrEmpty(
                        operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString())
                        ? string.Empty
                        : operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "2",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);
            var roles = UserProfileValidators_.Split('|');

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            var ListLanguages = ClauseRepository.GetLanguages(Language.EN);
            ViewBag.ListLanguages = new SelectList(ListLanguages, "convergenceMasterDataId", "Name");

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses).SelectMany(x => x.ClauseIndividuals).SelectMany(x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;
            foreach (var item in listDocuments)
            {
                if (item == "ELEGIBILITY LETTER")
                {
                    FlagLetter = true;
                }
            }

            ViewBag.Letter = FlagLetter;

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveEligibilityRequest")]
        [HasPermission(Permissions = "Eligibility Write")]
        public virtual ActionResult EditEligibilityApprovalRequest(OperationRelatedModel model)
        {
            UpdateContractModel(model);

            if (model == null)
            {
                throw new ArgumentNullException("Parameter cannot be null, EditEligibilityApprovalRequest method");
            }

            return RedirectToAction("DetailsEligibilityApprovalRequest",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    mainOperationNumber = model.mainOperationNumber
                });
        }

        [HttpPost]
        [HasPermission(Permissions = "Eligibility Write")]
        public virtual ActionResult CreateEligibilityApprovalRequest(OperationRelatedModel model)
        {
            UpdateContractModel(model);

            if (model == null)
            {
                throw new ArgumentException("Parameter cannot be null, CreateEligibilityApprovalRequest method");
            }

            var modelUpdated = ClauseRepository.GetContractById(model.Contracts[0].ContractId);
            TempData["USERCOMMENTSLIST"] = modelUpdated.UserComments;
            return RedirectToAction("CreateEligibilityApprovalValidationRequest", new
            {
                operationId = model.OperationId,
                contractId = model.Contracts[0].ContractId,
                mainOperationNumber = model.mainOperationNumber,
                additionalValidators = Request["validator_list_additional_list"]
            });
        }

        [HasPermission(Permissions = "Eligibility Write")]
        public virtual ActionResult CreateEligibilityApprovalValidationRequest(
            int operationId,
            int contractId,
            string mainOperationNumber,
            string additionalValidators = "")
        {
            var operationModel = ClauseRepository
                .GetContractByOperation(
                operationId,
                contractId,
                mainOperationNumber);

            var commentsUserList = TempData["USERCOMMENTSLIST"];

            var documentNumber = ClauseRepository
                .GetIDBDocNumberLetter("ELEGIBILITY LETTER", contractId);

            var callInstance = AutoMapper.Mapper.Map<OperationRelatedModel, OperationRelatedModelWorkflow>(operationModel);

            //desde el workflow una vez se valida la informacion debe guardar los datos respectivos
            callInstance.UserName = IDBContext.Current.UserName;
            callInstance.UserProfile = IDBContext.Current.FirstRole;
            callInstance.DocNumber = documentNumber;
            callInstance.UserProfileAdditional = additionalValidators;
            callInstance.UserComments = (IList<UserCommentModel>)commentsUserList;

            string extensionId;
            try
            {
                extensionId =
                    string.IsNullOrEmpty(
                        operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString())
                        ? string.Empty
                        : operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
               operationModel,
               "2",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);

            callInstance.UserProfileValidators = UserProfileValidators_;

            new ClauseBusinessLogic(_businessRuleService, _clauseService)
                .SetValidatorsToStartWorkflow(
                    UserProfileValidators_, additionalValidators, callInstance);

            Logger.GetLogger().WriteMessage("MFH - ContractsController", string.Format(
               "Eligibility 'CreateEligibilityApprovalValidationRequest' method: going to invoke " +
               "K2ServiceProxy with parameters:" + Environment.NewLine +
               "callInstance.Classification= {0}," + Environment.NewLine +
               "callInstance.DocNumber= {1}," + Environment.NewLine +
               "callInstance.ElegibilityDateField= {2}," + Environment.NewLine +
               "callInstance.FolioID= {3}," + Environment.NewLine +
               "callInstance.MainOperationNumber= {4}," + Environment.NewLine +
               "callInstance.PmrValidationStage= {5}," + Environment.NewLine +
               "callInstance.Status= {6}," + Environment.NewLine +
               "callInstance.TransactionId= {7}," + Environment.NewLine +
               "callInstance.UserName= {8}," + Environment.NewLine +
               "callInstance.UserProfile= {9}," + Environment.NewLine +
               "callInstance.UserProfileAdditional= {10}," + Environment.NewLine +
               "callInstance.UserProfileValidators= {11}",
               callInstance.Classification == null ? "null" : callInstance.Classification,
               callInstance.DocNumber == null ? "null" : callInstance.DocNumber,
               callInstance.ElegibilityDateField == null ? "null" : callInstance.ElegibilityDateField.ToString(),
               callInstance.FolioID == null ? "null" : callInstance.FolioID,
               callInstance.MainOperationNumber == null ? "null" : callInstance.MainOperationNumber,
               callInstance.PmrValidationStage == null ? "null" : callInstance.PmrValidationStage,
               callInstance.Status == null ? "null" : callInstance.Status,
               callInstance.TransactionId.ToString(),
               callInstance.UserName == null ? "null" : callInstance.UserName,
               callInstance.UserProfile == null ? "null" : callInstance.UserProfile,
               callInstance.UserProfileAdditional == null ? "null" : callInstance.UserProfileAdditional,
               callInstance.UserProfileValidators == null ? "null" : callInstance.UserProfileValidators));

            //TODO: analyze the posibility of taking out this old database logging.
            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
               LogType.Debug,
               "K2 EXECUTION",
               string.Format("ELIGIBILITY-BEFORE OPERATIONID:{0}",
                    IDBContext.Current.Operation), callInstance);
            var wasRequested = string.Empty;
            try
            {
                var response = InitialElegibilityWorkflow(callInstance);
                wasRequested = response.ErrorMessage;
                if (response.IsValid)
                {
                    ClauseContractHelper.UpdateValidationStage(
                        EntityToWorkflow.ENTITY_CONTRACT,
                        contractId,
                        AgreementsAndConditionsConstants.CONDITION_STATUS_REV);
                }
            }
            catch (Exception ex)
            {
                //TODO: analyze the posibility of taking out this old database logging.
                new CommonDocument().Log(
                  LogType.Debug,
                  "K2 EXECUTION ERROR",
                  "ELIGIBILITY-ERROR",
                  ex);
                throw;
            }

            //TODO: analyze the posibility of taking out this old database logging.
            new CommonDocument().Log(
               LogType.Debug,
               "K2 EXECUTION",
               string.Format("ELIGIBILITY-AFTER OPERATIONID:{0} K2RESPONSE:{1}",
                   IDBContext.Current.Operation,
                   wasRequested),
               callInstance);

            if (wasRequested == K2Response.StartWorkFlow_CL.GetStringValue())
            {
                return RedirectToAction("Index",
                    new
                    {
                        operationNumber = mainOperationNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 700,
                        messageK2 = wasRequested
                    });
            }
            else if (wasRequested == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(wasRequested);
                return RedirectToAction("DetailsEligibilityApprovalRequest",
                   new
                   {
                       operationId = operationId,
                       contractId = contractId,
                       mainOperationNumber = mainOperationNumber,
                       state = 555,
                       messageK2 = wasRequested
                   });
            }
            else
            {
                IDBContext.Current.ErrorMessage(wasRequested);
                return RedirectToAction("DetailsEligibilityApprovalRequest",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        mainOperationNumber = mainOperationNumber,
                        state = 600,
                        messageK2 = wasRequested
                    });
            }
        }

        public virtual ActionResult DetailsRevolvingFundById(int revolvingFundId)
        {
            var revolvingFund = _clauseService.GetRevolvingFund(revolvingFundId).Model;

            return RedirectToAction("DetailsRevolvingFund", new
            {
                operationId = revolvingFund.Contract.OperationId,
                contractId = revolvingFund.ContractId,
                mainOperationNumber = revolvingFund.Contract.Operation.OperationNumber
            });
        }

        public virtual ActionResult DetailsRevolvingFund(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);
            var revolvingFundModel = ClauseRepository.GetRevolvingFundsByContractId(operationModel.Contracts[0].ContractId);

            string[] masterDataItems = new string[] { "VALIDATION_STAGE" };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            var approvalStatusId = masterDataDetail["VALIDATION_STAGE"]
                    .FirstOrDefault(x => x.Code == ClauseConstants.REVOLVING_FUND_APPROVED_STATUS)
                    .ConvergenceMasterDataId;

            var revolvingFundModelApproved = new List<RevolvingFundModel>();
            for (var eachRevolvingFund = 0; eachRevolvingFund < revolvingFundModel.Count; eachRevolvingFund++)
            {
                if (revolvingFundModel[eachRevolvingFund].ValidationStageId == approvalStatusId)
                {
                    revolvingFundModelApproved.Add(revolvingFundModel[eachRevolvingFund]);
                }
                else if (eachRevolvingFund == revolvingFundModel.Count - 1)
                    revolvingFundModelApproved.Add(revolvingFundModel[eachRevolvingFund]);
            }

            operationModel.Contracts[0].RevolvingFund.AddRange(revolvingFundModelApproved);
            this.ViewBag.ApprovedRevolvingFundId = approvalStatusId;
            bool canBeRequest = true;
            var language = Language.EN;
            if (operationModel.Contracts[0].RevolvingFund.Count > 0)
            {
                this.ViewBag.LastRevolvingFundStatus = ClientMasterDataModelRepository.GetMasterDataNameById((int)revolvingFundModel[revolvingFundModel.Count - 1].ValidationStageId, language);
            }
            else
            {
                canBeRequest = false;
                ViewBag.LastRevolvingFundStatus = ClientMasterDataModelRepository
                    .GetMasterDataNameByCode(
                    "VALIDATION_STAGE",
                    ClauseConstants.REVOLVING_FUND_DRAFT_STATUS,
                    language);
            }

            ViewBag.canberequest = canBeRequest;
            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];

            return View(operationModel);
        }

        [HttpGet]
        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult EditRevolvingFund(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);
            var revolvingFundModel = ClauseRepository.GetRevolvingFundsByContractId(operationModel.Contracts[0].ContractId);
            ViewBag.CountRevolvingFund = revolvingFundModel.Count;

            string[] masterDataItems = new string[] { "VALIDATION_STAGE" };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            var approvalStatusId = masterDataDetail["VALIDATION_STAGE"]
                    .FirstOrDefault(x => x.Code == ClauseConstants.REVOLVING_FUND_APPROVED_STATUS)
                    .ConvergenceMasterDataId;

            var revolvingFundModelApproved = new List<RevolvingFundModel>();
            for (var eachRevolvingFund = 0; eachRevolvingFund < revolvingFundModel.Count; eachRevolvingFund++)
            {
                if (revolvingFundModel[eachRevolvingFund].ValidationStageId == approvalStatusId)
                {
                    revolvingFundModelApproved.Add(revolvingFundModel[eachRevolvingFund]);
                    if (eachRevolvingFund == revolvingFundModel.Count - 1)
                    {
                        revolvingFundModelApproved.Add(new RevolvingFundModel()
                        {
                            ContractId = operationModel.Contracts[0].ContractId,
                            PercentageValue = 0,
                            UpdateLMSStatus = 0,
                            ValidateDate = null,
                            ValidationStageId = masterDataDetail["VALIDATION_STAGE"]
                                        .FirstOrDefault(x => x.Code == ClauseConstants.REVOLVING_FUND_DRAFT_STATUS)
                                        .ConvergenceMasterDataId
                        });
                    }
                }
                else if (eachRevolvingFund == revolvingFundModel.Count - 1)
                    revolvingFundModelApproved.Add(revolvingFundModel[eachRevolvingFund]);
            }

            operationModel.Contracts[0].RevolvingFund.AddRange(revolvingFundModelApproved);

            this.ViewBag.ApprovedRevolvingFundId = approvalStatusId;
            var language = Language.EN;
            if (operationModel.Contracts[0].RevolvingFund.Count > 0)
            {
                this.ViewBag.LastRevolvingFundStatus = ClientMasterDataModelRepository.GetMasterDataNameById((int)operationModel.Contracts[0].RevolvingFund[operationModel.Contracts[0].RevolvingFund.Count - 1].ValidationStageId, language);
            }
            else
            {
                operationModel.Contracts[0].RevolvingFund.Add(new RevolvingFundModel()
                {
                    ContractId = operationModel.Contracts[0].ContractId,
                    PercentageValue = 0,
                    UpdateLMSStatus = 0,
                    ValidateDate = null,
                    ValidationStageId = masterDataDetail["VALIDATION_STAGE"]
                                        .FirstOrDefault(x => x.Code == ClauseConstants.REVOLVING_FUND_DRAFT_STATUS)
                                        .ConvergenceMasterDataId
                });

                ViewBag.LastRevolvingFundStatus = ClientMasterDataModelRepository
                    .GetMasterDataNameByCode(
                    "VALIDATION_STAGE",
                    ClauseConstants.REVOLVING_FUND_DRAFT_STATUS,
                    language);
            }

            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = "Revolving Fund Write")]
        [MultipleButton(Name = "action", Argument = "SaveRevolvingFund")]
        public virtual ActionResult EditRevolvingFund(OperationRelatedModel model)
        {
            var validationStageId = ClientMasterDataModelRepository
                .GetMasterDataId("VALIDATION_STAGE",
                ClauseConstants.REVOLVING_FUND_DRAFT_STATUS);

            var RevolvingFunds = ClauseRepository.GetRevolvingFundsByContractId(model.Contracts[0].ContractId).OrderBy(x => x.RevolvingFundId).ToList();
            var RevolvingFundLastId = -1;
            var LastRevolvingFundState = string.Empty;
            RevolvingFundModel RevolvingFundCurrent = null;
            if (RevolvingFunds.Count > 0)
            {
                RevolvingFundLastId = RevolvingFunds[RevolvingFunds.Count - 1].RevolvingFundId;
                LastRevolvingFundState = ClientMasterDataModelRepository.GetMasterDataCode(Convert.ToInt32(RevolvingFunds[RevolvingFunds.Count - 1].ValidationStageId));
                RevolvingFundCurrent = RevolvingFunds[RevolvingFunds.Count - 1];
            }

            var RevolvingFundId = -1;

            if (LastRevolvingFundState == ClauseConstants.REVOLVING_FUND_DRAFT_STATUS)
            {
                RevolvingFundId = RevolvingFundLastId;
            }

            ClauseRepository.SaveOrUpdateRevolvingFund(new RevolvingFundModel()
            {
                ContractId = model.Contracts[0].ContractId,
                PercentageValue = model.Contracts[0].RevolvingFund[model.Contracts[0].RevolvingFund.Count - 1].PercentageValue,
                RevolvingFundId = RevolvingFundId,
                UpdateLMSStatus = 0,
                ValidateDate = null,
                ValidationStageId = validationStageId,
                Documents = GetDocumentsRevolvingFund(RevolvingFundCurrent)
            }, IDBContext.Current.UserName);
            return RedirectToAction("DetailsRevolvingFund", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, mainOperationNumber = model.mainOperationNumber });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveRevolvingFund&Request")]
        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult CreateRevolvingFundRequest(OperationRelatedModel model)
        {
            var validationStageId = ClientMasterDataModelRepository
                .GetMasterDataId("VALIDATION_STAGE", ClauseConstants.REVOLVING_FUND_DRAFT_STATUS);

            var RevolvingFunds = ClauseRepository.GetRevolvingFundsByContractId(model.Contracts[0].ContractId).OrderBy(x => x.RevolvingFundId).ToList();
            var RevolvingFundLastId = -1;
            var LastRevolvingFundState = string.Empty;
            RevolvingFundModel RevolvingFundCurrent = null;
            if (RevolvingFunds.Count > 0)
            {
                RevolvingFundLastId = RevolvingFunds[RevolvingFunds.Count - 1].RevolvingFundId;
                LastRevolvingFundState = ClientMasterDataModelRepository.GetMasterDataCode(Convert.ToInt32(RevolvingFunds[RevolvingFunds.Count - 1].ValidationStageId));
                RevolvingFundCurrent = RevolvingFunds[RevolvingFunds.Count - 1];
            }

            var RevolvingFundId = -1;

            if (LastRevolvingFundState == ClauseConstants.REVOLVING_FUND_DRAFT_STATUS)
            {
                RevolvingFundId = RevolvingFundLastId;
            }

            var revolvingFundSaved = ClauseRepository.SaveOrUpdateRevolvingFund(
                new RevolvingFundModel()
                {
                    ContractId = model.Contracts[0].ContractId,
                    PercentageValue = model.Contracts[0].RevolvingFund[model.Contracts[0].RevolvingFund.Count - 1].PercentageValue,
                    RevolvingFundId = RevolvingFundId,
                    UpdateLMSStatus = 0,
                    ValidateDate = null,
                    ValidationStageId = validationStageId,
                    Documents = GetDocumentsRevolvingFund(RevolvingFundCurrent)
                },
            IDBContext.Current.UserName);

            return RedirectToAction("DetailsRevolvingFundApprovalRequest", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, revolvingFundId = revolvingFundSaved.RevolvingFundId, mainOperationNumber = model.mainOperationNumber });
        }

        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult DetailsRevolvingFundApprovalRequest(
            int operationId,
            int contractId,
            int revolvingFundId,
            string mainOperationNumber,
            MessageSendRequestCode state = 0,
            string messageK2 = null)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);
            var revolvingFundModel = ClauseRepository.GetRevolvingFundById(revolvingFundId);
            operationModel.Contracts[0].RevolvingFund.Add(revolvingFundModel);
            ViewBag.ListValidationStage = ClientMasterDataModelRepository.GetMasterDataModels("VALIDATION_STAGE");

            OperationRelatedModel ModelOperation = operationModel;

            string extensionId;
            try
            {
                extensionId =
                    string.IsNullOrEmpty(
                        operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString())
                        ? string.Empty
                        : operationModel.Contracts.First()
                            .Clauses.First()
                            .ClauseIndividuals.First()
                            .ClauseExtension.First()
                            .ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "1",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);

            var roles = UserProfileValidators_.Split('|');

            ViewBag.OperationRelatedModel = ModelOperation;

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(state, false, 2, messageK2);
                ViewData["message"] = message;
            }

            return View(operationModel);
        }

        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult EditRevolvingFundApprovalRequest(int operationId, int contractId, int revolvingFundId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetContractByOperation(operationId, contractId, mainOperationNumber);
            var revolvingFundModel = ClauseRepository.GetRevolvingFundById(revolvingFundId);
            operationModel.Contracts[0].RevolvingFund.Add(revolvingFundModel);

            OperationRelatedModel ModelOperation = operationModel;
            ViewBag.OperationRelatedModel = ModelOperation;

            string extensionId;
            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString()) ? string.Empty : operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseExtension.First().ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "1",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);
            var roles = UserProfileValidators_.Split('|');

            ViewBag.SendRequest = true;

            if (roles != null)
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (IDBContext.Current.HasRole(roles[i]))
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveRevolvingFundRequest")]
        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult EditRevolvingFundApprovalRequest(OperationRelatedModel model)
        {
            var modelRevolvingFund = ClauseRepository.GetRevolvingFundById(model.Contracts[0].RevolvingFund[0].RevolvingFundId);
            modelRevolvingFund.UserComments.Clear();

            foreach (var Comments in model.Contracts[0].RevolvingFund[0].UserComments)
            {
                if (Comments.UserCommentId == -1)
                {
                    Comments.ModifiedBy = IDBContext.Current.UserName;
                }
            }

            modelRevolvingFund.UserComments.AddRange(model.Contracts[0].RevolvingFund[0].UserComments);
            ClauseRepository.SaveOrUpdateRevolvingFund(modelRevolvingFund, IDBContext.Current.UserName);

            return RedirectToAction("DetailsRevolvingFundApprovalRequest", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, revolvingFundId = model.Contracts[0].RevolvingFund[0].RevolvingFundId, mainOperationNumber = model.mainOperationNumber });
        }

        [HttpPost]
        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult CreateRevolvingFundApprovalRequest(OperationRelatedModel model)
        {
            EditRevolvingFundApprovalRequest(model);

            if (model == null)
            {
                throw new ArgumentException("Parameter cannot be null, CreateRevolvingFundApprovalRequest method");
            }

            var modelRevolvingFund = ClauseRepository
                .GetRevolvingFundById(model.Contracts[0]
                .RevolvingFund[0]
                .RevolvingFundId);

            TempData["USERCOMMENTSLIST"] = modelRevolvingFund.UserComments;

            return RedirectToAction("CreateRevolvingFundApprovalValidationRequest",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    revolvingFundId = model.Contracts[0].RevolvingFund[0].RevolvingFundId,
                    mainOperationNumber = model.mainOperationNumber,
                    additionalValidators = Request["validator_list_additional_list"]
                });
        }

        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult CreateRevolvingFundApprovalValidationRequest(
            int operationId,
            int contractId,
            int revolvingFundId,
            string mainOperationNumber,
            string additionalValidators = "")
        {
            var commentsUserList = TempData["USERCOMMENTSLIST"];

            var operationModel = ClauseRepository
                .GetContractByOperation(
                operationId,
                contractId,
                mainOperationNumber);
            var revolvingFundModel = ClauseRepository.GetRevolvingFundById(revolvingFundId);
            operationModel.Contracts[0].RevolvingFund.Clear();
            operationModel.Contracts[0].RevolvingFund.Add(revolvingFundModel);

            var callInstance = AutoMapper.Mapper.Map<OperationRelatedModel, OperationRelatedModelWorkflow>(operationModel);

            callInstance.UserName = IDBContext.Current.UserName;
            callInstance.UserProfile = IDBContext.Current.FirstRole;
            callInstance.UserProfileAdditional = additionalValidators;
            callInstance.UserComments = (IList<UserCommentModel>)commentsUserList;

            string extensionId = string.Empty;
            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First()
                    .Clauses.First()
                    .ClauseIndividuals.First()
                    .ClauseExtension.First()
                    .ClauseExtensionId.ToString()) ? string.Empty : operationModel.Contracts.First()
                    .Clauses.First()
                    .ClauseIndividuals.First()
                    .ClauseExtension.First()
                    .ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string UserProfileValidators_ = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "1",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);
            callInstance.UserProfileValidators = UserProfileValidators_;

            new ClauseBusinessLogic(_businessRuleService, _clauseService)
                .SetValidatorsToStartWorkflow(
                    UserProfileValidators_, additionalValidators, callInstance);

            //add a log before the call to K2
            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
               LogType.Debug,
               "K2 EXECUTION",
               string.Format("REVOLVING FOUND-BEFORE OPERATIONID:{0}",
               IDBContext.Current.Operation), callInstance);
            var wasRequested = string.Empty;
            try
            {
                var response = InitialResolvingFundWorkflow(callInstance);
                wasRequested = response.ErrorMessage;
                if (response.IsValid)
                {
                    ClauseContractHelper.UpdateValidationStage(
                        EntityToWorkflow.ENTITY_REVOLVING_FUND,
                        revolvingFundId,
                        AgreementsAndConditionsConstants.CONDITION_STATUS_REV_RF);
                }
            }
            catch (Exception ex)
            {
                //when k2 raise an error log this error an raise the error
                new CommonDocument().Log(
                  LogType.Debug,
                  "K2 EXECUTION ERROR",
                  "REVOLVING FOUND-ERROR",
                  ex);
                throw;
            }

            //add a log entry with the k2 response
            new CommonDocument().Log(
               LogType.Debug,
               "K2 EXECUTION",
               string.Format("REVOLVING FOUND-AFTER OPERATIONID:{0} K2RESPONSE:{1}",
                   IDBContext.Current.Operation,
                   wasRequested),
               callInstance);

            if (wasRequested == K2Response.StartWorkFlow_CL.GetStringValue())
            {
                return RedirectToAction("Index",
                    new
                    {
                        operationNumber = mainOperationNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 700,
                        messageK2 = wasRequested
                    });
            }
            else if (wasRequested == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(wasRequested);
                return RedirectToAction("DetailsRevolvingFundApprovalRequest",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        revolvingFundId = revolvingFundId,
                        mainOperationNumber = mainOperationNumber,
                        state = 555,
                        messageK2 = wasRequested
                    });
            }
            else
            {
                IDBContext.Current.ErrorMessage(wasRequested);
                return RedirectToAction("DetailsRevolvingFundApprovalRequest",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        revolvingFundId = revolvingFundId,
                        mainOperationNumber = mainOperationNumber,
                        state = 600,
                        messageK2 = wasRequested
                    });
            }
        }

        [ExceptionHandling]
        public virtual ActionResult IndexWarningMessage(string message)
        {
            this.ViewBag.Message = message;
            return PartialView();
        }

        [ExceptionHandling]
        public virtual ActionResult IndexWarningMessageSendToTrack(int contractId, string mainOperationNumber)
        {
            var contractModel = ClauseRepository.GetContractById(contractId);

            var currentAmount = ClauseRepository.CurrentAmountContract(contractId);

            var response = _clauseService.GetClauseCategory(
                mainOperationNumber, currentAmount, ClauseConstants.LAST_DISBURMENT);

            if (!response.IsValid)
                return PartialView(contractModel);

            bool canSendToTrack = false;
            if (contractModel.Clauses
                .Where(x => x.CategoryId == response.Model.ConvergenceMasterDataId)
                .Any())
            {
                canSendToTrack = true;
            }

            ViewBag.cansendtotrack = canSendToTrack;
            ViewBag.MainOperationNumber = mainOperationNumber;

            return PartialView(contractModel);
        }

        [ExceptionHandling]
        public virtual ActionResult IndexWarningMessageSendToRecord(int contractId, string mainOperationNumber)
        {
            this.ViewBag.cansendtorecord = ClauseRepository.ValidateSendToRecord(contractId);
            ViewBag.ContractId = contractId;
            ViewBag.MainOperationNumber = mainOperationNumber;
            return PartialView();
        }

        [ExceptionHandling]
        public virtual ActionResult TLAnulationRequestAction()
        {
            return PartialView();
        }

        [HttpPost]
        [HasPermission(Permissions = "Revolving Fund Write")]
        public virtual ActionResult RevolvingFundWarningPercent(
            int contractId, decimal revolvingFundPercent)
        {
            int draftId = ClientMasterDataRepository.GetMasterDataId(MasterType.VALIDATION_STAGE,
                ClauseConstants.DRAFT_CLAUSE_STATUS);

            var revolvingFundWarningPercent = new RevolvingFundWarningPercent();

            revolvingFundWarningPercent =
                ClauseBusinessLogicCalculator.MapRevolvingFundTotalPercent(contractId, revolvingFundPercent);

            return Json(revolvingFundWarningPercent);
        }

        public virtual FileResult DownloadReport(string OperationNumber, string format)
        {
            if (format.EqualsAny(AapGlobalValues.PDF, AapGlobalValues.EXCEL))
            {
                var operations = ClauseRepository.GetRelatedContractsByOperation(
                    OperationNumber,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    IDBContext.Current.CurrentLanguage);

                List<Tuple<int, string>> OverallStagesOperations = new List<Tuple<int, string>>();

                for (int i = 0; i < operations.Count; i++)
                {
                    int operationId = operations[i].OperationId;
                    var OverallStages = ClauseRepository.getOperationOverallStages(
                        operations[i].mainOperationNumber, Language.EN);

                    if (OverallStages != null)
                    {
                        var DescripcionOverallStages = OverallStages.OverallStage;

                        OverallStagesOperations.Add(
                            Tuple.Create(operationId, DescripcionOverallStages));
                    }
                }

                string[] masterDataItems = new string[]
                {
                    MasterType.CLAUSE_LOCATION,
                    MasterType.CLAUSE_TYPE,
                    MasterType.CLAUSE_STATUS,
                    MasterType.VALIDATION_STAGE
                };

                var masterDataDetail = ClientMasterDataModelRepository
                    .GetMasterDataModels(masterDataItems);

                var availablelocation = masterDataDetail[MasterType.CLAUSE_LOCATION];
                var availableType = masterDataDetail[MasterType.CLAUSE_TYPE];
                var availableStatus = masterDataDetail[MasterType.CLAUSE_STATUS];
                var clauseCategories = GetAllCategories(operations);
                var availableStage = masterDataDetail[MasterType.VALIDATION_STAGE];

                ReportBuilder RB = new ReportBuilder();

                var file = RB.BuildContractsAndClauses(
                    operations,
                    availablelocation,
                    availableType,
                    availableStatus,
                    clauseCategories,
                    availableStage,
                    format);

                string name = IDBContext.Current.Operation + "_" +
                    Localization.GetText("ContractsAndClauses");

                string filename = name + "." + format;

                if (format == AapGlobalValues.PDF)
                {
                    return File(file, DisbursementValues.CONTENT_TYPE_PDF, filename);
                }

                return File(file, DisbursementValues.CONTENT_TYPE_EXCEL, filename);
            }

            return null;
        }

        IList<ConvergenceMasterDataModel> GetAllCategories(
            List<OperationRelatedModel> operations)
        {
            var categories = new List<ConvergenceMasterDataModel>();

            categories.AddRange(ClientMasterDataRepository
                .GetMasterDataModels(ClauseConstants.CLAUSE_CATEGORY_420));

            categories.AddRange(ClientMasterDataRepository
                .GetMasterDataModels(ClauseConstants.CLAUSE_CATEGORY_421));

            categories.AddRange(ClientMasterDataRepository
                .GetMasterDataModels(ClauseConstants.CLAUSE_CATEGORY_422));

            return categories;
        }

        private List<DocumentModel> GetDocumentsRevolvingFund(
            RevolvingFundModel revolvingFund)
        {
            if (revolvingFund == null)
            {
                return new List<DocumentModel>();
            }

            return revolvingFund.Documents;
        }

        private void ReformulationContractName(List<OperationRelatedModel> operations)
        {
            if (!operations.Any(x => x.IsReformulated.IsTrue()))
                return;

            foreach (var operation in operations)
            {
                foreach (var contract in operation.Contracts)
                {
                    int activityPlanId = contract.ActivityPlanId.HasValue ?
                        contract.ActivityPlanId.Value :
                        default(int);
                    try
                    {
                        StringResponse response = _reformulationService
                            .GetReformulationName(activityPlanId);

                        contract.ReformulationName = response.Data;
                    }
                    catch (Exception ex)
                    {
                        contract.ReformulationName = ex.Message;
                    }
                }
            }
        }

        private ResponseBase InitialElegibilityWorkflow(OperationRelatedModelWorkflow workflowModel)
        {
            var workflowRequest = new WorkflowCreationRequest
            {
                Parameters = new Dictionary<string, string>()
            };

            var contract = workflowModel.Contracts.First();
            workflowRequest.CreateUser = workflowModel.UserName;
            workflowRequest.CreateProfile = workflowModel.UserProfile;
            workflowRequest.EntityId = contract.ContractId;
            workflowRequest.EntityType = "Contract";
            workflowRequest.OperationNumber = workflowModel.MainOperationNumber;
            workflowRequest.WorkflowType = WorkflowTypes.ELIGIBILITY_DATE_APPROVAL;
            workflowRequest.ContractNumber = contract.ContractNumber;
            workflowRequest.AdditionalValidators = workflowModel
                .UserProfileAdditional.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            workflowRequest.Parameters.Add(GeneralClauseConstants.CONTRACT_NUMBER_VARIABLE,
                contract.ContractNumber);
            workflowRequest.Parameters.Add(GeneralClauseConstants.ELEGIBILITY_DATE,
                workflowModel.ElegibilityDateField.ToString());
            
            workflowRequest.MandatoryValidators = workflowModel
                .UserProfileValidators.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (workflowModel.UserComments.HasAny())
            {
                workflowRequest.UserCommentIds = workflowModel
                    .UserComments.Select(x => x.UserCommentId).ToList();
            }

            workflowRequest.MandatoryValidators = workflowModel
                .UserProfileValidators.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            var codeList = ValidatorHelpers
                .ConvertProfileToCode(workflowRequest.MandatoryValidators, workflowRequest.WorkflowType);
            if (codeList.Any())
            {
                workflowRequest.CodeValidators = codeList;
                workflowRequest.MandatoryValidators = null;
            }

            var response = _workflowManager.InitiateWorkflow(workflowRequest);

            if (!response.IsValid)
            {
                return response;
            }

            response.ErrorMessage = K2Response.StartWorkFlow_CL.GetStringValue();
            return response;
        }

        private ResponseBase InitialResolvingFundWorkflow(OperationRelatedModelWorkflow workflowModel)
        {
            var workflowRequest = new WorkflowCreationRequest
            {
                Parameters = new Dictionary<string, string>()
            };

            var contract = workflowModel.Contracts.First();
            var newRevolvingFund = contract.RevolvingFund
                .OrderByDescending(x => x.RevolvingFundId).FirstOrDefault();

            workflowRequest.CreateUser = workflowModel.UserName;
            workflowRequest.CreateProfile = workflowModel.UserProfile;
            workflowRequest.EntityId = newRevolvingFund.RevolvingFundId;
            workflowRequest.EntityType = "RevolvingFund";
            workflowRequest.OperationNumber = workflowModel.MainOperationNumber;
            workflowRequest.WorkflowType = WorkflowTypes.VALIDATION_REVOLVING_FUND;
            workflowRequest.ContractNumber = contract.ContractNumber;

            workflowRequest.Parameters.Add(GeneralClauseConstants.CONTRACT_NUMBER_VARIABLE,
                contract.ContractNumber);

            var currentRevolvingFundPercentage = _clauseService.GetCurrentRevolvingFundPercentage(
                    contract.ContractId,
                    newRevolvingFund.RevolvingFundId);

            workflowRequest.Parameters.Add(GeneralClauseConstants.NEW_RF_VARIABLE,
                newRevolvingFund.PercentageValue.ToString());

            workflowRequest.Parameters.Add(GeneralClauseConstants.CURRENT_RF_VARIABLE,
                currentRevolvingFundPercentage.ToString());

            if (workflowModel.UserComments.HasAny())
            {
                workflowRequest.UserCommentIds = workflowModel
                    .UserComments.Select(x => x.UserCommentId).ToList();
            }

            if (workflowModel.UserProfileAdditional != null)
            {
                workflowRequest.AdditionalValidators = workflowModel
                    .UserProfileAdditional.Split(new char[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
            }

            workflowRequest.MandatoryValidators = workflowModel
                .UserProfileValidators.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            var codeList = ValidatorHelpers
                .ConvertProfileToCode(workflowRequest.MandatoryValidators, workflowRequest.WorkflowType);
            if (codeList.Any())
            {
                workflowRequest.CodeValidators = codeList;
                workflowRequest.MandatoryValidators = null;
            }

            var response = _workflowManager.InitiateWorkflow(workflowRequest);

            if (!response.IsValid)
            {
                return response;
            }

            response.ErrorMessage = K2Response.StartWorkFlow_CL.GetStringValue();
            return response;
        }

        private void UpdateContractModel(OperationRelatedModel model)
        {
            var modelEligibility = ClauseRepository.GetContractById(model.Contracts[0].ContractId);
            modelEligibility.UserComments.Clear();

            foreach (var Comments in model.Contracts[0].UserComments)
            {
                if (Comments.UserCommentId == -1)
                {
                    Comments.ModifiedBy = IDBContext.Current.UserName;
                }
            }

            modelEligibility.UserComments.AddRange(model.Contracts[0].UserComments);
            ClauseRepository.SaveContractStatus(modelEligibility, IDBContext.Current.UserName);
        }
    }
}