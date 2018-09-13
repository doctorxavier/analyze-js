using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using IDB.Architecture.Diagnostics;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Domain.Attributes;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.ClausesAndContracts.Services.RulesEngine;
using IDB.MW.Application.ClausesAndContractsModule.Enums;
using IDB.MW.Application.ClausesAndContractsModule.Services.Clauses;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.NoK2Workflows.Enums;
using IDB.MW.Application.NoK2Workflows.Models;
using IDB.MW.Application.NoK2Workflows.Services;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Architecture.Clauses;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Clauses;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.GenericWorkflow.Enums;
using IDB.MW.GenericWorkflow.Helpers;
using IDB.MW.GenericWorkflow.Messages;
using IDB.MW.GenericWorkflow.Services;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Architecture.Clauses;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.K2Service;
using IDB.Presentation.MVC4.Areas.Clauses.Helpers;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.Clauses.Controllers
{
    public partial class ClauseController : BaseController
    {
        private readonly IClauseBusinessLogicRuleService _businessRuleService;
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly ICatalogService _catalogService;
        private readonly IClauseIndividualReturnedService _clauseIndividualReturnedService;
        private readonly INoK2WorkflowService _noK2WorkflowsService;
        private readonly IOperationRepository _operationRepository;
        private readonly IWorkflowManagerService _workflowManager;
        private CommonDocument DoccumentObject = new CommonDocument();
        private IClauseService _clauseService;
        private IGlobalModelRepository _Global = null;

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

        #region WCF Services
        public virtual GlobalModelRepository GlobalModelRepositoryClient { get; set; }
        public virtual IK2ServiceProxy K2ServiceProxy { get; set; }

        private IConvergenceMasterDataModelRepository _ClientMasterDataRepository = null;
        public virtual IConvergenceMasterDataModelRepository MasterDataRep
        {
            get { return _ClientMasterDataRepository; }
            set { _ClientMasterDataRepository = value; }
        }

        public virtual IGlobalModelRepository Global
        {
            get { return _Global; }
            set { _Global = value; }
        }

        private IOperationModelRepository _operation = null;
        public virtual IOperationModelRepository ClauseRepository
        {
            get { return _operation; }
            set { _operation = value; }
        }

        private IDB.MW.Domain.Contracts.DomainServices.ILoanOperationDataService _clientLoan = null;
        public virtual IDB.MW.Domain.Contracts.DomainServices.ILoanOperationDataService ClientLoan
        {
            get { return _clientLoan; }
            set { _clientLoan = value; }
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

        private IConvergenceMasterDataModelRepository _ClientMasterDataModelRepository = null;
        public virtual IConvergenceMasterDataModelRepository ClientMasterDataModelRepository
        {
            get { return _ClientMasterDataModelRepository; }
            set { _ClientMasterDataModelRepository = value; }
        }
        #endregion

        #region Variables

        string Lang = "EN";

        #endregion

        public ClauseController(
            IClauseBusinessLogicRuleService businessRuleService,
            IClauseService clauseService,
            IAttributesEngineService attributesEngineService,
            ICatalogService catalogService,
            IClauseIndividualReturnedService clauseIndividualReturnedService,
            IOperationRepository operationRepository,
            IWorkflowManagerService workflowManager,
            INoK2WorkflowService noK2WorkflowsService)
        {
            _businessRuleService = businessRuleService;
            _clauseService = clauseService;
            _attributesEngineService = attributesEngineService;
            _catalogService = catalogService;
            _clauseIndividualReturnedService = clauseIndividualReturnedService;
            _noK2WorkflowsService = noK2WorkflowsService;
            _operationRepository = operationRepository;
            _workflowManager = workflowManager;
        }

        #region CLAUSES CRUD

        #region CREATE A NEW CLAUSE SECTION

        [ExceptionHandling]
        public virtual ActionResult Create(int operationId, int contractId, string mainOperationNumber)
        {
            var contractModel = ClauseRepository.GetContractById(contractId);

            if (contractModel == null || !contractModel.Clauses.HasAny())
                return RedirectFailsCreation(mainOperationNumber);

            var operationModel = ClauseRepository.GetContractByOperation(
                operationId, contractId, mainOperationNumber);

            var canCreateClauseResponse = _clauseService.CanCreateNewClause(
                contractModel.Clauses, contractModel.ContractId, mainOperationNumber);

            if (!canCreateClauseResponse.IsValid)
                return RedirectFailsCreation(mainOperationNumber);

            if (canCreateClauseResponse.HasCondition)
            {
                AttributeBankForTC(operationId, mainOperationNumber);

                return RedirectToAction("CreateNewClause", new
                {
                    operationId = operationId,
                    contractId = contractId,
                    mainOperationNumber = mainOperationNumber
                });
            }

            if (_clauseService.CanCreateLastDClause(contractModel, operationModel))
            {
                return RedirectToAction("CreateLastDisbursement", new
                {
                    operationId = operationId,
                    contractId = contractId,
                    mainOperationNumber = mainOperationNumber
                });
            }

            return RedirectFailsCreation(mainOperationNumber);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = Permission.LAST_DISBURSEMENT_CLAUSE_WRITE)]
        public virtual ActionResult CreateLastDisbursement(
            int operationId,
            int contractId,
            string mainOperationNumber)
        {
            var clauseCategoryQuery = new List<ConvergenceMasterDataModel>();

            var operationModel = ClauseRepository
                .GetContractByOperation(operationId, contractId, mainOperationNumber);

            var codeMasterData = MasterDataRep
                .GetMasterDataModelById(operationModel.Contracts[0].LmsTypeId.Value);

            var currentAmountContract = ClauseRepository
                .CurrentAmountContract(operationModel.Contracts[0].ContractId);

            var clauseLocationQuery = MasterDataRep.GetMasterDataModels("CLAUSE_LOCATION");
            ViewBag.ListClauseLocation = clauseLocationQuery;
            var clauseLocationList = new List<SelectListItem>();

            foreach (var clauseLocation in clauseLocationQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseLocation.ConvergenceMasterDataId.ToString();
                item.Text = clauseLocation.NameEn;
                clauseLocationList.Add(item);
            }

            operationModel.ListClauseLocation2 = clauseLocationList;
            var language = Language.EN;
            operationModel.ClauseType = MasterDataRep.GetMasterDataNameByCode("CLAUSE_TYPE", "FINANCIA", language);
            operationModel.ClauseStatus = MasterDataRep.GetMasterDataNameByCode("CLAUSE_STATUS", "CL_DRAFT", language);

            var response = _clauseService.GetClauseCategory(
                operationModel.OperationNumber, currentAmountContract, ClauseConstants.LAST_DISBURMENT);

            if (!response.IsValid)
                return View(operationModel);

            operationModel.ClauseCategory = response.Model.NameEn;

            operationModel.ListContractStatus = MasterDataRep.GetMasterDataModels("CONTRACT_CLAUSE_STATUS");
            ViewData["ClauseStatusFulFil"] = MasterDataRep.GetMasterDataId("CLAUSE_STATUS", "CL_FULFIL");
            int categoryId = GetCategoryId(operationId, contractId, mainOperationNumber);

            var modelClause = new ClauseModel()
            {
                ClauseId = -1,
                Frequency = 0,
                IsSpecial = false,
                ExpirationFrom = null,
                ExpirationFromDependencyId = 0,
                FromShift = 0,
                ExpirationTo = operationModel.Contracts[0].CurrentDisbursementExpirationDate.HasValue ?
                    operationModel.Contracts[0].CurrentDisbursementExpirationDate : DateTime.UtcNow,
                ExpirationToDependencyId = 0,
                ToShift = 0,
                ClauseTypeId = MasterDataRep.GetMasterDataId("CLAUSE_TYPE", "FINANCIA"),
                LocationId = clauseLocationQuery.FirstOrDefault().ConvergenceMasterDataId,
                CategoryId = categoryId,
                ContractRelationId = MasterDataRep.GetMasterDataId("CONTRACT_RELATION", "ONLY"),
                ClauseIndividuals = new List<ClauseIndividualModel>()
                {
                    new ClauseIndividualModel()
                    {
                        ClauseIndividualId = -1,
                        StatusId = MasterDataRep.GetMasterDataId("CLAUSE_STATUS", "CL_DRAFT"),
                        Suffix = string.Empty,
                        UpdateLMSStatus = 0,
                        IsDeleted = false,
                        OriginalExpirationDate = operationModel.Contracts[0].CurrentDisbursementExpirationDate.HasValue ?
                            operationModel.Contracts[0].CurrentDisbursementExpirationDate.Value : DateTime.UtcNow,
                        CurrentExpirationDate = operationModel.Contracts[0].CurrentDisbursementExpirationDate.HasValue ?
                            operationModel.Contracts[0].CurrentDisbursementExpirationDate.Value : DateTime.UtcNow,
                        SubmissionDate = operationModel.Contracts[0].CurrentDisbursementExpirationDate.HasValue ?
                            operationModel.Contracts[0].CurrentDisbursementExpirationDate.Value : DateTime.UtcNow,
                        ValidateDate = null,
                    }
                }
            };

            operationModel.Contracts[0].Clauses.Add(modelClause);

            if (operationModel.Contracts[0].Clauses.Count == 2)
            {
                operationModel.Contracts[0].Clauses.RemoveAt(0);
            }

            if (operationModel.Contracts[0].Clauses.Count == 3)
            {
                operationModel.Contracts[0].Clauses.RemoveAt(0);
                operationModel.Contracts[0].Clauses.RemoveAt(0);
            }

            operationModel.ListTrench = _clauseService.GetTrenchForOperationAttribute(operationModel, true);
            operationModel.IsSpecialOperationPBLPBP = _clauseService
                .IsSpecialOperationPBLPBP(operationModel.mainOperationNumber);

            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = Permission.LAST_DISBURSEMENT_CLAUSE_WRITE)]
        public virtual ActionResult CreateLastDisbursement(OperationRelatedModel model)
        {
            Tuple<int, int> tuple = Tuple.Create(0, 0);

            if (model != null)
            {
                tuple = ClauseRepository.SaveContract(model);
            }

            if (model != null && model.IncludeFinancialStatements)
            {
                _clauseService.InsertMilestoneForAFSClause();

                return RedirectToAction("CreateNewClause",
                    new
                    {
                        operationId = model.OperationId,
                        contractId = model.Contracts[0].ContractId,
                        mainOperationNumber = model.mainOperationNumber,
                        includeFinancialStatements = model.IncludeFinancialStatements
                    });
            }

            if (tuple.Item1 > 0 && tuple.Item2 > 0 && model != null)
            {
                AttributeBankForTC(model.OperationId, model.mainOperationNumber);
                return RedirectToAction("Details",
                    new
                    {
                        operationId = model.OperationId,
                        contractId = model.Contracts[0].ContractId,
                        clauseId = tuple.Item1,
                        clauseIndividualId = tuple.Item2,
                        mainOperationNumber = model.mainOperationNumber
                    });
            }

            return RedirectToAction("Index",
                "Contracts",
                new
                {
                    area = "Clauses",
                    operationNumber = model != null ? model.mainOperationNumber : string.Empty
                });
        }

        public virtual JsonResult EditDropdownForClauseType(int categoryId)
        {
            var selectItems = new List<SelectListItem>();

            var categoryTypesResponse = _clauseService
                .GetClauseTypesFromCategory(IDBContext.Current.Operation, categoryId);

            if (!categoryTypesResponse.IsValid)
                return Json(selectItems, JsonRequestBehavior.AllowGet);

            categoryTypesResponse.Models.ForEach(t =>
                selectItems.Add(new SelectListItem
                {
                    Value = t.ConvergenceMasterDataId.ToString(),
                    Text = t.NameEn
                }));

            selectItems.First().Selected = true;

            return Json(selectItems, JsonRequestBehavior.AllowGet);
        }

        [HasPermission(Permissions = Permission.CLAUSES_WRITE)]
        public virtual ActionResult CreateNewClause(
            int operationId,
            int contractId,
            string mainOperationNumber,
            int? clauseTemplateId,
            bool? includeFinancialStatements)
        {
            var operationModel = ClauseRepository
                .GetContractByOperation(operationId, contractId, mainOperationNumber);

            operationModel.IncludeFinancialStatements = includeFinancialStatements ?? false;
            operationModel = _clauseService
                .GetOperationRelatedModelForCreate(operationModel, operationId, clauseTemplateId);

            ViewData["ExpirationFromDependencyId"] =
                operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId;
            ViewData["ExpirationToDependencyId"] =
                operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId;
            ViewData["FromShift"] = operationModel.Contracts[0].Clauses[0].FromShift;
            ViewData["ToShift"] = operationModel.Contracts[0].Clauses[0].ToShift;
            ViewData["ClauseId"] = operationModel.Contracts[0].Clauses[0].ClauseId;

            ViewData["OtherP_421"] = MasterDataRep.GetMasterDataId(
                ClauseConstants.CLAUSE_CATEGORY_421,
                ClauseConstants.CATEGORY_OTHERP);

            var contractRelationQuery = MasterDataRep.GetMasterDataModels("CONTRACT_RELATION");
            AttributeBankForTC(operationId, mainOperationNumber);

            if (ViewBag.TypeBank && ViewBag.isTCOperation)
            {
                contractRelationQuery.RemoveAll(x => x.Code.Equals("LINKED"));
            }
            else
            {
                contractRelationQuery.RemoveAll(x => x.Code.Equals("LINKEDC"));
            }

            ViewData["ContractRelationQuery"] = ContractRelationConvertToList(contractRelationQuery);

            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = Permission.CLAUSES_WRITE)]
        public virtual ActionResult CreateNewClause(OperationRelatedModel model)
        {
            Tuple<int, int> tuple = Tuple.Create(0, 0);

            if (model != null)
            {
                if (!model.Contracts[0].Clauses[0].isFromDependencyDate)
                    model.Contracts[0].Clauses[0].ExpirationFromDependencyId = null;

                if (!model.Contracts[0].Clauses[0].isToDependencyDate)
                    model.Contracts[0].Clauses[0].ExpirationToDependencyId = null;

                tuple = ClauseRepository.SaveContract(model);

                var isCreatingAFS = _clauseService.IsCreatingAnAFSClause(
                    model.Contracts[0].Clauses[0].CategoryId, model.Contracts[0].Clauses[0].ClauseTypeId);

                if (model.IncludeFinancialStatements || isCreatingAFS.IsValid)
                    _clauseService.InsertMilestoneForAFSClause();

                if (tuple.Item1 > 0 && tuple.Item2 > 0)
                {
                    AttributeBankForTC(model.OperationId, model.mainOperationNumber);
                    return RedirectToAction("Details", new
                    {
                        operationId = model.OperationId,
                        contractId = model.Contracts[0].ContractId,
                        clauseId = tuple.Item1,
                        clauseIndividualId = tuple.Item2,
                        mainOperationNumber = model.mainOperationNumber
                    });
                }
            }

            return RedirectToAction(
                "Index",
                "Contracts",
                new
                {
                    area = "Clauses",
                    operationNumber = model != null ? model.mainOperationNumber : string.Empty
                });
        }

        public virtual ActionResult CreateClauseFromTemplate(
            int operationId,
            int contractId,
            string mainOperationNumber,
            int? filterByCountry,
            bool? includeFinancialStatements)
        {
            ViewBag.operationId = operationId;
            ViewBag.mainOperationNumber = mainOperationNumber;
            ViewBag.contractId = contractId;

            ViewBag.clauseTemplateId = 0;
            int countryId = CountryHelper.Get().GetCountry(operationId);
            var countriesQuery = MasterDataRep.GetMasterDataModels("COUNTRY").OrderBy(x => x.NameEn).ToList();
            var countriesList = new List<SelectListItem>();
            countriesList.Add(new SelectListItem() { Value = "0", Text = "Select Country" });
            foreach (var country in countriesQuery)
            {
                var item = new SelectListItem();
                item.Value = country.ConvergenceMasterDataId.ToString();
                item.Text = Localization.GetText(country.NameEn);
                item.Selected = countryId == country.ConvergenceMasterDataId ? true : false;
                countriesList.Add(item);
            }

            ViewBag.ListCountries = countriesList;
            ViewBag.countryId = countryId;
            ViewBag.CountrySearch = 0;
            List<ClauseTemplateModel> modelTemplate;

            if (filterByCountry != null && filterByCountry > 0)
            {
                ViewBag.CountrySearch = (int)filterByCountry;
                modelTemplate = ClauseRepository.GetAllClauseTemplatesByCountry((int)filterByCountry);
            }
            else if (filterByCountry == 0)
                modelTemplate = ClauseRepository.GetAllClauseTemplates();
            else
                modelTemplate = ClauseRepository.GetAllClauseTemplatesByOperationCountryId(countryId);

            modelTemplate.First().IncludeFinancialStatements = includeFinancialStatements ?? false;

            return PartialView(modelTemplate);
        }
        #endregion

        public virtual ActionResult DetailsClauseById(string operationNumber, int clauseIndividualId)
        {
            var clauseIndividualModel = ClauseRepository.GetClauseIndividualsById(clauseIndividualId);
            var clauseModel = ClauseRepository.GetClauseById(clauseIndividualModel.ClauseId);
            var contractModel = _clauseService.GetContract(operationNumber, clauseModel.ClauseId);
            return RedirectToAction("Details",
                 new
                 {
                     operationId = contractModel.Model.OperationId,
                     contractId = contractModel.Model.ContractId,
                     clauseId = clauseModel.ClauseId,
                     clauseIndividualId = clauseIndividualModel.ClauseIndividualId,
                     mainOperationNumber = operationNumber,
                     requestNameType = string.Empty
                 });
        }

        public virtual ActionResult Details(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            string requestNameType)
        {
            AttributeBankForTC(operationId, mainOperationNumber);

            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, false);

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CLAUSE_FINAL_STATUS",
                "VALIDATION_STAGE"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            var listClauseStatusList = masterDataDetail["CLAUSE_STATUS"];

            int categoryId = operationModel.Contracts[0].Clauses[0].CategoryId;
            var categoryList = GetCategoriesByClauses(operationModel);

            if (IDBContext.Current.HasPermission(Permission.CLAUSES_WRITE)
                || IDBContext.Current.HasPermission(Permission.LAST_DISBURSEMENT_CLAUSE_WRITE))
            {
                int statusId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].StatusId;

                List<string> listCode = new List<string>() { "CL_DRAFT", "CL_TR", "CL_TR_W" };
                var status = listClauseStatusList.FirstOrDefault(x => x.ConvergenceMasterDataId == statusId);
                var category = categoryList.FirstOrDefault(x => x.ConvergenceMasterDataId == categoryId);

                if (status != null && listCode.Contains(status.Code)
                    && category != null && category.Code != ClauseConstants.LAST_DISBURMENT)
                {
                    if (category.Code == "CCP" && status.Code == "CL_TR")
                    {
                        return RedirectToAction("Edit",
                         new
                         {
                             operationId = operationId,
                             contractId = contractId,
                             clauseId = clauseId,
                             clauseIndividualId = clauseIndividualId,
                             mainOperationNumber = mainOperationNumber,
                             requestNameType = requestNameType
                         });
                    }
                }
            }

            ViewBag.mainOperationNumber = operationModel.mainOperationNumber;

            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            ViewBag.ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            ViewBag.ListClauseStatus = listClauseStatusList;
            ViewBag.ListClauseCategory = categoryList;
            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];
            ViewBag.ClauseCategory421Id = MasterDataRep.GetMasterDataId(
                MasterType.CLAUSE_CATEGORY_421, MasterData.LASTD);
            ViewBag.ClauseCategory420Id = MasterDataRep.GetMasterDataId(
                MasterType.CLAUSE_CATEGORY_420, MasterData.LASTD);

            bool lastDisbursementDependency = false;
            if (operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0 && !lastDisbursementDependency)
            {
                var expirationFromDependency = MasterDataRep.GetMasterDataCode(
                    (int)operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId);
                if (expirationFromDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            if (operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0)
            {
                var expirationToDependency = MasterDataRep.GetMasterDataCode(
                    (int)operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId);
                if (expirationToDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            ViewBag.Dependency = lastDisbursementDependency;
            List<string> RelationList = new List<string>()
            {
                "onlyThis", "Linked", "All"
            };
            ViewBag.RelationsWithContracts = new SelectList(RelationList);
            ViewBag.FinalStatusList = masterDataDetail["CLAUSE_FINAL_STATUS"];

            var extensions = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension;

            extensions.Sort((x, y) =>
            {
                var xDate = x.ValidateDate == null ? DateTime.MaxValue : x.ValidateDate.Value;
                var yDate = y.ValidateDate == null ? DateTime.MaxValue : y.ValidateDate.Value;
                return xDate.CompareTo(yDate);
            });

            DateTime currentExpiration;

            currentExpiration = operationModel
                .Contracts.First()
                .Clauses.First()
                .ClauseIndividuals.First()
                .OriginalExpirationDate;

            int StateAppr = masterDataDetail["VALIDATION_STAGE"]
                              .FirstOrDefault(x => x.Code == ClauseConstants.APPROVED_CLAUSE_STATUS)
                              .ConvergenceMasterDataId;

            foreach (var extension in extensions)
            {
                if (extension.ValidationStageId != StateAppr)
                {
                    continue;
                }

                if (extension.IdbRequest == null)
                {
                    currentExpiration = Convert.ToDateTime(extension.RequestExtensionDate);
                }
                else
                {
                    currentExpiration = currentExpiration.AddMonths((int)extension.IdbRequest);
                }

                extension.RequestExtensionDate = currentExpiration;
            }

            operationModel.HasMilestonTypeTrenchOperation =
                HasMilestonTypeTrenchClauseOperation(operationModel);

            operationModel.IsSpecialOperationPBLPBP = _clauseService.IsSpecialOperationPBLPBP(operationModel.mainOperationNumber);

            operationModel.NameTrench = ClauseRepository.GetTrenchName(clauseId, Localization.CurrentLanguage);
            operationModel.HasDocument = ClauseRepository.ClauseHasDocument(clauseId);

            var clauseIndividual = operationModel.Contracts[0]
                .Clauses[0].ClauseIndividuals[0];

            operationModel.IsVisibleButtonClauseIndividualReturned =
                ValidationVisibleForClauseReturned(clauseIndividual);

            operationModel.IsTLOnlyApproverClauseFinalStatusValidation =
                IsTLOnlyApproverClauseFinalStatusValidation(clauseIndividual);

            operationModel.HasClauseIndividualReturnedHistory =
                HasClauseIndividualReturnedHistory(clauseIndividual);

            operationModel.GetAllClauseIndividualReturned = GetAllClauseIndividualReturned(clauseIndividual);

            operationModel.HasPermissionClauseIndividualReturned = IDBContext.Current
                .HasPermission(Permission.CLAUSE_TRACK_RETURN);

            operationModel.IsSignatureOrRatificationOrLegalReport =
                IsSignatureOrRatificationOrLegalReport(clauseIndividual);

            return View(operationModel);
        }

        #region UPDATE A CLAUSE/ REQUEST VALIDATION FOR A CLAUSE

        public virtual ActionResult Edit(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            if (string.IsNullOrEmpty(mainOperationNumber))
            {
                mainOperationNumber = IDBContext.Current.Operation;
            }

            var clauseCategory = ClauseRepository.GetClauseCategoryById(clauseId);
            var clauseStatus = ClauseRepository.GetClauseStatusById(clauseIndividualId);

            if (clauseCategory == ClauseConstants.LAST_DISBURMENT)
            {
                switch (clauseStatus)
                {
                    case "CL_DRAFT":
                        return RedirectToAction("EditLastDisbursementClauseDraft", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber });
                    case "CL_TR":
                        return RedirectToAction("EditLastDisbursementClauseTrack", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber, operationNumber = mainOperationNumber });
                    default:
                        return HttpNotFound();
                }
            }
            else
            {
                switch (clauseStatus)
                {
                    case "CL_DRAFT":
                        return RedirectToAction("EditClauseDraft", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber });
                    case "CL_TR":
                        return RedirectToAction("EditClauseTrack", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber });
                    case "CL_TR_W":
                        return RedirectToAction("EditClauseTrackWithDraw", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber });
                    default:
                        return HttpNotFound();
                }
            }
        }

        #region Change Clause Status

        [HasPermission(Permissions = Permission.CLAUSES_WRITE)]
        public virtual ActionResult EditClauseToTrack(
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            string locatetop = "")
        {
            var clauseStatusTrackId = MasterDataRep
                .GetMasterDataId(MasterType.CLAUSE_STATUS, ClauseConstants.TR_CLAUSE_STATUS);
            var clauseIndividualModels = ClauseRepository.GetClauseIndividualsByClauseId(clauseId);
            var ListValidStatesToChangeStateClausdeIndividual = new List<int>()
            {
                MasterDataRep.GetMasterDataId(
                    MasterType.CLAUSE_STATUS,
                    ClauseConstants.DRAFT_CLAUSE_STATUS)
            };

            foreach (var itemClauseIndividual in clauseIndividualModels)
            {
                if (ListValidStatesToChangeStateClausdeIndividual
                        .Any(validStatus => validStatus == itemClauseIndividual.StatusId) ||
                    itemClauseIndividual.ClauseIndividualId == clauseIndividualId)
                {
                    itemClauseIndividual.StatusId = clauseStatusTrackId;

                    ClauseRepository.SaveOrUpdateClauseIndividual(
                        itemClauseIndividual,
                        IDBContext.Current.UserName);

                    new CMBusiness().InsertTrenchMilestone(clauseId);
                }
            }

            return RedirectToAction("Index",
                "Contracts",
                new
                {
                    area = "Clauses",
                    operationNumber = mainOperationNumber,
                    locatetop = locatetop
                });
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseToTrackWithdraw(int clauseIndividualId, string mainOperationNumber, string locatetop = "")
        {
            var clauseStatusTrackId = MasterDataRep.GetMasterDataId("CLAUSE_STATUS", "CL_TR_W");
            var clauseIndividualModel = ClauseRepository.GetClauseIndividualsById(clauseIndividualId);
            clauseIndividualModel.StatusId = clauseStatusTrackId;
            ClauseRepository.SaveOrUpdateClauseIndividual(clauseIndividualModel, IDBContext.Current.UserName);
            return RedirectToAction("Index", "Contracts", new { area = "Clauses", operationNumber = mainOperationNumber, locatetop = locatetop });
        }

        [HasPermission(Permissions = "Last Disbursement Clause Write")]
        public virtual ActionResult ConfirmChangedClauseStatusToFulfilment(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, false);
            return PartialView(operationModel);
        }

        [HasPermission(Permissions = Permission.CLAUSES_WRITE)]
        public virtual ActionResult EditClauseToFulfilment(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber)
        {
            var clauseStatusFulfilmentId = MasterDataRep
                .GetMasterDataId(ClauseConstants.CLAUSE_STATUS, ClauseConstants.FULFILLED_FINAL_STATUS);
            var ClauseValidationStageAprobId = MasterDataRep
                .GetMasterDataId(ClauseConstants.VALIDATION_STAGE, ClauseConstants.APPROVED_CLAUSE_STATUS);
            var clauseStatusFinalFulfilmentId = MasterDataRep
                .GetMasterDataId(ClauseConstants.CLAUSE_FINAL_STATUS, ClauseConstants.FULFILLED_FINAL_STATUS);

            var clauseIndividualModel = ClauseRepository.GetClauseIndividualsById(clauseIndividualId);
            clauseIndividualModel.StatusId = clauseStatusFulfilmentId;
            clauseIndividualModel.FinalStatusId = clauseStatusFinalFulfilmentId;
            clauseIndividualModel.ValidationStageId = ClauseValidationStageAprobId;
            ClauseRepository.SaveOrUpdateClauseIndividual(
                clauseIndividualModel, IDBContext.Current.UserName);

            _clauseService.CompleteMilestonesForAFSClause(
                ClauseRepository.GetClauseCategoryById(clauseId),
                clauseIndividualModel.ValidateDate,
                clauseIndividualModel.SubmissionDate);

            AttributeBankForTC(operationId, mainOperationNumber);
            return RedirectToAction("Details",
                new
                {
                    operationId = operationId,
                    contractId = contractId,
                    clauseId = clauseId,
                    clauseIndividualId = clauseIndividualId,
                    mainOperationNumber = mainOperationNumber
                });
        }

        #endregion

        #region Last Disbursement Section

        [HasPermission(Permissions = "Last Disbursement Clause Write")]
        public virtual ActionResult EditLastDisbursementClauseDraft(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, true);

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_FINAL_STATUS"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            var clauseLocationQuery = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListClauseLocation = clauseLocationQuery;
            var clauseLocationList = new List<SelectListItem>();
            foreach (var clauseLocation in clauseLocationQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseLocation.ConvergenceMasterDataId.ToString();
                item.Text = clauseLocation.NameEn;
                clauseLocationList.Add(item);
            }

            ViewBag.ListClauseLocation2 = clauseLocationList;
            var language = Language.EN;
            ViewBag.ClauseType = MasterDataRep.GetMasterDataNameById(operationModel.Contracts[0].Clauses[0].ClauseTypeId, language);
            ViewBag.ClauseStatus = MasterDataRep.GetMasterDataNameById(operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].StatusId, language);
            ViewBag.Category = MasterDataRep.GetMasterDataNameById(operationModel.Contracts[0].Clauses[0].CategoryId, language);
            ViewBag.FinalStatusList = masterDataDetail["CLAUSE_FINAL_STATUS"];

            operationModel.ListTrench = _clauseService.GetTrenchForOperationAttribute(operationModel, true);
            operationModel.TrenchClauseId = ClauseRepository.GetCmdTrenchId(clauseId);
            operationModel.IsSpecialOperationPBLPBP = _clauseService.IsSpecialOperationPBLPBP(operationModel.mainOperationNumber);
            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = "Last Disbursement Clause Write")]
        public virtual ActionResult EditLastDisbursementClauseDraftPost(OperationRelatedModel model)
        {
            var modelClause = ClauseRepository.GetClauseById(model.Contracts[0].Clauses[0].ClauseId);
            modelClause.ClauseNumber = model.Contracts[0].Clauses[0].ClauseNumber;
            modelClause.ClauseIndividuals[0].SubmissionDate = model.Contracts[0].Clauses[0].ClauseIndividuals[0].SubmissionDate;

            modelClause.LocationId = model.Contracts[0].Clauses[0].LocationId;
            modelClause.IsSpecial = model.Contracts[0].Clauses[0].IsSpecial;
            modelClause.Description = model.Contracts[0].Clauses[0].Description;
            ClauseRepository.SaveClauseModel(modelClause, IDBContext.Current.UserName);
            AttributeBankForTC(model.OperationId, model.mainOperationNumber);
            return RedirectToAction("Details", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = model.Contracts[0].Clauses[0].ClauseId, clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        [HasPermission(Permissions = "Last Disbursement Clause Write")]
        public virtual ActionResult EditLastDisbursementClauseTrack(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, true);

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CLAUSE_FINAL_STATUS",
                "VALIDATION_STAGE"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            ViewBag.mainOperationNumber = operationModel.mainOperationNumber;
            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            ViewBag.ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            ViewBag.ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];

            ViewBag.ListClauseCategory = GetCategoriesByClauses(operationModel);
            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];
            var finalStatusQuery = masterDataDetail["CLAUSE_FINAL_STATUS"];
            var finalStatusList = new List<SelectListItem>();
            foreach (var finalStatus in finalStatusQuery)
            {
                var item = new SelectListItem();
                item.Value = finalStatus.ConvergenceMasterDataId.ToString();
                item.Text = finalStatus.NameEn;
                finalStatusList.Add(item);
            }

            ViewBag.FinalStatusList = finalStatusList;

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            var extensions = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension;

            extensions.Sort((x, y) =>
            {
                var xDate = x.ValidateDate == null ? DateTime.MaxValue : x.ValidateDate.Value;
                var yDate = y.ValidateDate == null ? DateTime.MaxValue : y.ValidateDate.Value;
                return xDate.CompareTo(yDate);
            });

            DateTime currentExpiration;

            currentExpiration = operationModel
                .Contracts.First()
                .Clauses.First()
                .ClauseIndividuals.First()
                .OriginalExpirationDate;

            int StateAppr = masterDataDetail["VALIDATION_STAGE"]
                     .FirstOrDefault(x => x.Code == ClauseConstants.APPROVED_CLAUSE_STATUS)
                     .ConvergenceMasterDataId;

            foreach (var extension in extensions)
            {
                if (extension.ValidationStageId != StateAppr)
                {
                    continue;
                }

                if (extension.IdbRequest == null)
                {
                    currentExpiration = Convert.ToDateTime(extension.RequestExtensionDate);
                }
                else
                {
                    currentExpiration = currentExpiration.AddMonths((int)extension.IdbRequest);
                }

                extension.RequestExtensionDate = currentExpiration;
            }

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveLastDisbursementClause")]
        [HasPermission(Permissions = "Last Disbursement Clause Write")]
        public virtual ActionResult EditLastDisbursementClauseTrackPost(OperationRelatedModel model)
        {
            var modelClause = ClauseRepository.GetClauseById(model.Contracts[0].Clauses[0].ClauseId);
            foreach (var clauseIndividualModel in modelClause.ClauseIndividuals)
            {
                if (clauseIndividualModel.ClauseIndividualId == model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId)
                {
                    clauseIndividualModel.SubmissionDate = model.Contracts[0].Clauses[0].ClauseIndividuals[0].SubmissionDate;
                    clauseIndividualModel.UserComments.Clear();
                    clauseIndividualModel.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);
                }
            }

            var modelWithIds = ClauseRepository.SaveClauseModel(modelClause, IDBContext.Current.UserName);
            AttributeBankForTC(model.OperationId, model.mainOperationNumber);
            return RedirectToAction("Details", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = modelWithIds.ClauseId, clauseIndividualId = modelWithIds.ClauseIndividuals.LastOrDefault().ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequestLastDisbursementClause")]
        public virtual ActionResult CreateLastDisbursementClauseRequest(OperationRelatedModel model)
        {
            var modelClause = ClauseRepository.GetClauseById(model.Contracts[0].Clauses[0].ClauseId);
            foreach (var clauseIndividualModel in modelClause.ClauseIndividuals)
            {
                if (clauseIndividualModel.ClauseIndividualId == model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId)
                {
                    clauseIndividualModel.CurrentExpirationDate = model.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate.HasValue ? model.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate : DateTime.Now;
                    clauseIndividualModel.UserComments.Clear();
                    clauseIndividualModel.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);
                }
            }

            var modelWithIds = ClauseRepository.SaveClauseModel(modelClause, IDBContext.Current.UserName);

            return RedirectToAction("DetailsClauseApprovalRequest", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = model.Contracts[0].Clauses[0].ClauseId, clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        #endregion

        #region Other Clause Category

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseDraft(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, true);

            var isIgrOperation = _clauseService.IsIgrOperation(operationModel.OperationId);
            var hasMoreThanOneContract = _clauseService.HasMoreThanOneContract(operationModel.OperationId);

            ViewBag.mainOperationNumber = operationModel.mainOperationNumber;

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CONTRACT_RELATION",
                "EXPIRATION_TO_DEPENDENCY"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            var clauseLocationQuery = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListClauseLocation = clauseLocationQuery;
            var clauseLocationList = new List<SelectListItem>();
            foreach (var clauseLocation in clauseLocationQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseLocation.ConvergenceMasterDataId.ToString();
                item.Text = clauseLocation.NameEn;
                clauseLocationList.Add(item);
            }

            ViewBag.ListClauseLocation2 = clauseLocationList;
            var clauseTypeQuery = masterDataDetail["CLAUSE_TYPE"];
            ViewBag.ListClauseType = clauseTypeQuery;
            var clauseTypeList = new List<SelectListItem>();
            foreach (var clauseType in clauseTypeQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseType.ConvergenceMasterDataId.ToString();
                item.Text = clauseType.NameEn;
                clauseTypeList.Add(item);
            }

            ViewBag.ListClauseType2 = clauseTypeList;

            var clauseCategoryQuery = GetCategoriesByClauses(operationModel);
            ViewBag.ListClauseCategory = clauseCategoryQuery;
            var clauseCategoryList = new List<SelectListItem>();
            foreach (var clauseCategory in clauseCategoryQuery)
            {
                if (clauseCategory.Code != "SIGN" && clauseCategory.Code != "RAT" && clauseCategory.Code != ClauseConstants.LAST_DISBURMENT)
                {
                    var item = new SelectListItem();
                    item.Value = clauseCategory.ConvergenceMasterDataId.ToString();
                    item.Text = clauseCategory.NameEn;
                    clauseCategoryList.Add(item);
                }
            }

            ViewBag.ListClauseCategory2 = clauseCategoryList;

            Func<string, bool> IsOperationTCP =
                x => OperationTypeHelper.GetOperationTypes(x).Contains(OperationType.TCP);

            if (IsOperationTCP(mainOperationNumber))
            {
                ViewBag.ListClauseCategory2 = _clauseService.CheckCategoriesForTcpExecutingByBank(
                    operationModel.Contracts[0],
                    clauseCategoryList);
            }

            ViewBag.ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];
            var contractRelationQuery = masterDataDetail["CONTRACT_RELATION"];

            var contractRelationList = new List<SelectListItem>();

            AttributeBankForTC(operationId, mainOperationNumber);

            if (ViewBag.TypeBank && ViewBag.isTCOperation)
            {
                contractRelationQuery.RemoveAll(x => x.Code.Equals("LINKED"));
            }
            else
            {
                contractRelationQuery.RemoveAll(x => x.Code.Equals("LINKEDC"));
            }

            foreach (var contractRelation in contractRelationQuery)
            {
                var item = new SelectListItem();
                item.Value = contractRelation.ConvergenceMasterDataId.ToString();
                item.Text = Localization.GetText(contractRelation.NameEn);
                item.Disabled = isIgrOperation && hasMoreThanOneContract &&
                    contractRelation.Code == "LINKED";
                contractRelationList.Add(item);
            }

            ViewBag.RelationsWithContracts = contractRelationList;
            var expirationToDependencyList = masterDataDetail["EXPIRATION_TO_DEPENDENCY"];
            var dateForDependencyQuery = expirationToDependencyList;
            var dateForDependencyList = new List<SelectListItem>();
            foreach (var dateForDependency in dateForDependencyQuery)
            {
                var item = new SelectListItem();
                item.Value = dateForDependency.ConvergenceMasterDataId.ToString();
                item.Text = Localization.GetText(dateForDependency.NameEn);
                dateForDependencyList.Add(item);
            }

            ViewBag.dateForDependencyList = dateForDependencyList;

            var dateForDependencyQueryforValidation = expirationToDependencyList;
            var dateForDependencyListforValidation = new List<SelectListItem>();

            foreach (var dateForDependencyforValidation in dateForDependencyQueryforValidation)
            {
                var item = new SelectListItem();
                item.Value = dateForDependencyforValidation.ConvergenceMasterDataId.ToString();
                item.Text = dateForDependencyforValidation.Code;
                dateForDependencyListforValidation.Add(item);
            }

            ViewData["dateForDependencyListforValidation"] = dateForDependencyListforValidation;

            var dependencyDirectionList = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "true",
                    Selected = true,
                    Text = "After"
                },
                new SelectListItem()
                {
                    Value = "false",
                    Text = "Before"
                }
            };

            ViewBag.dependencyDirectionList = dependencyDirectionList;

            ViewData["dependencyDirectionListValidation"] = dependencyDirectionList;

            ViewData["ExpirationFromDependencyId"] = operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId;
            ViewData["ExpirationToDependencyId"] = operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId;
            ViewData["FromShift"] = operationModel.Contracts[0].Clauses[0].FromShift;
            ViewData["ToShift"] = operationModel.Contracts[0].Clauses[0].ToShift;
            ViewData["ClauseId"] = operationModel.Contracts[0].Clauses[0].ClauseId;

            operationModel.Contracts[0].Clauses[0].isFromDependencyDate = operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId != null ? true : false;
            operationModel.Contracts[0].Clauses[0].isToDependencyDate = operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId != null ? true : false;
            operationModel.Contracts[0].Clauses[0].FromShift = operationModel.Contracts[0].Clauses[0].FromShift != null ? (int)operationModel.Contracts[0].Clauses[0].FromShift : 0;
            operationModel.Contracts[0].Clauses[0].ToShift = operationModel.Contracts[0].Clauses[0].ToShift != null ? (int)operationModel.Contracts[0].Clauses[0].ToShift : 0;

            operationModel.ListTrench = _clauseService.GetTrenchForOperationAttribute(operationModel, false);
            operationModel.TrenchClauseId = ClauseRepository.GetCmdTrenchId(clauseId);
            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseDraftPost(OperationRelatedModel model)
        {
            if (model != null)
            {
                if (!model.Contracts[0].Clauses[0].isFromDependencyDate || model.Contracts[0].Clauses[0].ExpirationFrom.HasValue)
                {
                    model.Contracts[0].Clauses[0].ExpirationFromDependencyId = null;
                    model.Contracts[0].Clauses[0].FromShift = null;
                }

                if (!model.Contracts[0].Clauses[0].isToDependencyDate || model.Contracts[0].Clauses[0].ExpirationTo.HasValue)
                {
                    model.Contracts[0].Clauses[0].ToShift = null;
                    model.Contracts[0].Clauses[0].ExpirationToDependencyId = null;
                }

                var tuple = ClauseRepository.SaveContract(model);

                if (tuple.Item1 > 0 && tuple.Item2 > 0)
                {
                    AttributeBankForTC(model.OperationId, model.mainOperationNumber);
                    return RedirectToAction("Details", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = tuple.Item1, clauseIndividualId = tuple.Item2, mainOperationNumber = model.mainOperationNumber });
                }
            }

            return RedirectToAction("Index", "Contracts", new { area = "Clauses", operationNumber = string.Empty });
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseTrack(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, true);

            operationModel.WorkflowInProgress = Localization.GetText("IDB.Clause.K2.Text.Message.CheckCL003Validation");

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CLAUSE_FINAL_STATUS",
                "VALIDATION_STAGE"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            ViewBag.mainOperationNumber = operationModel.mainOperationNumber;

            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            ViewBag.ListClauseLocation = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListClauseType = masterDataDetail["CLAUSE_TYPE"];
            ViewBag.ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];
            ViewBag.ListClauseCategory = GetCategoriesByClauses(operationModel);
            ViewBag.ListValidationStage = masterDataDetail["VALIDATION_STAGE"];
            ViewBag.FinalStatusList = masterDataDetail["CLAUSE_FINAL_STATUS"];

            bool lastDisbursementDependency = false;
            if (operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0
                && !lastDisbursementDependency)
            {
                var expirationFromDependency = MasterDataRep.GetMasterDataCode((int)operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId);
                if (expirationFromDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            if (operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0)
            {
                var expirationToDependency = MasterDataRep.GetMasterDataCode((int)operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId);
                if (expirationToDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            ViewBag.Dependency = lastDisbursementDependency;

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.WarningMessage = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            var extensions = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension;

            extensions.Sort((x, y) =>
            {
                var xDate = x.ValidateDate == null ? DateTime.MaxValue : x.ValidateDate.Value;
                var yDate = y.ValidateDate == null ? DateTime.MaxValue : y.ValidateDate.Value;
                return xDate.CompareTo(yDate);
            });

            DateTime currentExpiration;

            currentExpiration = operationModel
                .Contracts.First()
                .Clauses.First()
                .ClauseIndividuals.First()
                .OriginalExpirationDate;

            int StateAppr = masterDataDetail["VALIDATION_STAGE"]
                     .FirstOrDefault(x => x.Code == ClauseConstants.APPROVED_CLAUSE_STATUS)
                     .ConvergenceMasterDataId;

            foreach (var extension in extensions)
            {
                if (extension.ValidationStageId != StateAppr)
                {
                    continue;
                }

                if (extension.IdbRequest == null)
                {
                    currentExpiration = Convert.ToDateTime(extension.RequestExtensionDate);
                }
                else
                {
                    currentExpiration = currentExpiration.AddMonths((int)extension.IdbRequest);
                }

                extension.RequestExtensionDate = currentExpiration;
            }

            operationModel.HasMilestonTypeTrenchOperation =
               HasMilestonTypeTrenchClauseOperation(operationModel);

            operationModel.IsSpecialOperationPBLPBP = _clauseService.IsSpecialOperationPBLPBP(operationModel.mainOperationNumber);

            operationModel.NameTrench = ClauseRepository.GetTrenchName(clauseId, Localization.CurrentLanguage);
            operationModel.HasDocument = ClauseRepository.ClauseHasDocument(clauseId);

            return View(operationModel);
        }

        public virtual ActionResult SaveFinalStatusAndRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            int finalStatus)
        {
            var modelClauseIndividual = ClauseRepository.GetClauseIndividualsById(clauseIndividualId);
            modelClauseIndividual.FinalStatusId = finalStatus;
            ClauseRepository.SaveOrUpdateClauseIndividual(
                modelClauseIndividual, IDBContext.Current.UserName);

            _clauseService.CompleteMilestonesForAFSClause(
                ClauseRepository.GetClauseCategoryById(clauseId),
                modelClauseIndividual.ValidateDate,
                modelClauseIndividual.SubmissionDate);

            return RedirectToAction("DetailsClauseApprovalRequest",
                new
                {
                    operationId = operationId,
                    contractId = contractId,
                    clauseId = clauseId,
                    clauseIndividualId = clauseIndividualId,
                    mainOperationNumber = mainOperationNumber,
                    requestNameType = ClauseRepository
                        .GetClauseFinalStatusRequestTranslationById(finalStatus)
                });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveClause")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClausePost(OperationRelatedModel model)
        {
            model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId = null;
            SaveClauseIndividual(model);
            AttributeBankForTC(model.OperationId, model.mainOperationNumber);
            return RedirectToAction("Details", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = model.Contracts[0].Clauses[0].ClauseId, clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        private void SaveClauseIndividual(OperationRelatedModel model)
        {
            var modelClauseIndividual = ClauseRepository.GetClauseIndividualsById(model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId);
            modelClauseIndividual.SubmissionDate = model.Contracts[0].Clauses[0].ClauseIndividuals[0].SubmissionDate;
            if (model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId.HasValue)
                modelClauseIndividual.FinalStatusId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId;
            modelClauseIndividual.UserComments.Clear();
            modelClauseIndividual.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);
            ClauseRepository.SaveOrUpdateClauseIndividual(modelClauseIndividual, IDBContext.Current.UserName);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save&RequestClauseFulfilled")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult CreateClauseRequestFulfilled(OperationRelatedModel model)
        {
            model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId = MasterDataRep
                .GetMasterDataId("CLAUSE_FINAL_STATUS", "CL_FULFIL");

            SaveClauseIndividual(model);

            return RedirectToAction("DetailsClauseApprovalRequest", new
            {
                operationId = model.OperationId,
                contractId = model.Contracts[0].ContractId,
                clauseId = model.Contracts[0].Clauses[0].ClauseId,
                clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseIndividualId,
                mainOperationNumber = model.mainOperationNumber,
                requestNameType = Localization.GetText("CL.FINALSTATUS.START.K2SCREEN.FULFILLMENT")
            });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save&RequestClauseUnfulfilled")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult CreateClauseRequestUnfulfilled(OperationRelatedModel model)
        {
            model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId = MasterDataRep
                .GetMasterDataId("CLAUSE_FINAL_STATUS", "CL_UNFUL");

            SaveClauseIndividual(model);

            return RedirectToAction("DetailsClauseApprovalRequest", new
            {
                operationId = model.OperationId,
                contractId = model.Contracts[0].ContractId,
                clauseId = model.Contracts[0].Clauses[0].ClauseId,
                clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseIndividualId,
                mainOperationNumber = model.mainOperationNumber,
                requestNameType = Localization.GetText("CL.FINALSTATUS.START.K2SCREEN.UNFULFILLMENT")
            });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save&RequestClauseWaived")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult CreateClauseRequestWaived(OperationRelatedModel model)
        {
            model.Contracts[0].Clauses[0].ClauseIndividuals[0].FinalStatusId = MasterDataRep
                .GetMasterDataId("CLAUSE_FINAL_STATUS", "CL_WAIV");

            SaveClauseIndividual(model);

            return RedirectToAction("DetailsClauseApprovalRequest", new
            {
                operationId = model.OperationId,
                contractId = model.Contracts[0].ContractId,
                clauseId = model.Contracts[0].Clauses[0].ClauseId,
                clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseIndividualId,
                mainOperationNumber = model.mainOperationNumber,
                requestNameType = Localization.GetText("CL.FINALSTATUS.START.K2SCREEN.WAIVED")
            });
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseTrackWithDraw(int operationId, int contractId, int clauseId, int clauseIndividualId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, true);

            ViewBag.mainOperationNumber = operationModel.mainOperationNumber;

            string[] masterDataItems = new string[]
            {
                "CONTRACT_CLAUSE_STATUS",
                "CLAUSE_LOCATION",
                "CLAUSE_TYPE",
                "CLAUSE_STATUS",
                "CLAUSE_FINAL_STATUS"
            };
            var masterDataDetail = ClientMasterDataModelRepository.GetMasterDataModels(masterDataItems);

            var clauseLocationQuery = masterDataDetail["CLAUSE_LOCATION"];
            ViewBag.ListContractStatus = masterDataDetail["CONTRACT_CLAUSE_STATUS"];
            ViewBag.ListClauseLocation = clauseLocationQuery;
            var clauseLocationList = new List<SelectListItem>();
            foreach (var clauseLocation in clauseLocationQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseLocation.ConvergenceMasterDataId.ToString();
                item.Text = clauseLocation.NameEn;
                clauseLocationList.Add(item);
            }

            ViewBag.ListClauseLocation2 = clauseLocationList;
            var clauseTypeQuery = masterDataDetail["CLAUSE_TYPE"];
            ViewBag.ListClauseType = clauseTypeQuery;
            var clauseTypeList = new List<SelectListItem>();
            foreach (var clauseType in clauseTypeQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseType.ConvergenceMasterDataId.ToString();
                item.Text = clauseType.NameEn;
                clauseTypeList.Add(item);
            }

            ViewBag.ListClauseType2 = clauseTypeList;
            ViewBag.ListClauseStatus = masterDataDetail["CLAUSE_STATUS"];

            var clauseCategoryQuery = GetCategoriesByClauses(operationModel);
            ViewBag.ListClauseCategory = clauseCategoryQuery;

            var clauseCategoryList = new List<SelectListItem>();
            foreach (var clauseCategory in clauseCategoryQuery)
            {
                if (clauseCategory.Code != "SIGN" && clauseCategory.Code != "RAT" && clauseCategory.Code != ClauseConstants.LAST_DISBURMENT)
                {
                    var item = new SelectListItem();
                    item.Value = clauseCategory.ConvergenceMasterDataId.ToString();
                    item.Text = clauseCategory.NameEn;
                    clauseCategoryList.Add(item);
                }
            }

            ViewBag.ListClauseCategory2 = clauseCategoryList;

            Func<string, bool> IsOperationTCP =
                x => OperationTypeHelper.GetOperationTypes(x).Contains(OperationType.TCP);

            if (IsOperationTCP(mainOperationNumber))
            {
                ViewBag.ListClauseCategory2 = _clauseService.CheckCategoriesForTcpExecutingByBank(
                    operationModel.Contracts[0],
                    clauseCategoryList);
            }

            ViewBag.FinalStatusList = masterDataDetail["CLAUSE_FINAL_STATUS"];

            bool lastDisbursementDependency = false;
            if (operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0
                && !lastDisbursementDependency)
            {
                var expirationFromDependency = MasterDataRep.GetMasterDataCode((int)operationModel.Contracts[0].Clauses[0].ExpirationFromDependencyId);
                if (expirationFromDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            if (operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId != null
                && operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId > 0)
            {
                var expirationToDependency = MasterDataRep.GetMasterDataCode((int)operationModel.Contracts[0].Clauses[0].ExpirationToDependencyId);
                if (expirationToDependency == "LASTCD")
                    lastDisbursementDependency = true;
            }

            ViewBag.Dependency = lastDisbursementDependency;

            #endregion

            return View(operationModel);
        }

        [HttpPost]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseTrackWithDrawPost(OperationRelatedModel model)
        {
            var modelClause = ClauseRepository.GetClauseById(model.Contracts[0].Clauses[0].ClauseId);
            var clauseIndividualStatus = MasterDataRep.GetMasterDataCode(model.Contracts[0].Clauses[0].ClauseIndividuals[0].StatusId);
            modelClause.ClauseNumber = model.Contracts[0].Clauses[0].ClauseNumber;
            modelClause.Description = model.Contracts[0].Clauses[0].Description;
            modelClause.CategoryId = model.Contracts[0].Clauses[0].CategoryId;
            modelClause.ClauseTypeId = model.Contracts[0].Clauses[0].ClauseTypeId;
            modelClause.LocationId = model.Contracts[0].Clauses[0].LocationId;
            modelClause.IsSpecial = model.Contracts[0].Clauses[0].IsSpecial;
            foreach (var clauseIndividualModel in modelClause.ClauseIndividuals)
            {
                if (clauseIndividualStatus == "CL_TR_W")
                {
                    foreach (var eachClauseIndividual in model.Contracts[0].Clauses[0].ClauseIndividuals)
                    {
                        if (clauseIndividualModel.ClauseIndividualId == eachClauseIndividual.ClauseIndividualId)
                        {
                            clauseIndividualModel.Suffix = eachClauseIndividual.Suffix;
                        }
                    }
                }
                else if (clauseIndividualModel.ClauseIndividualId == model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId)
                {
                    clauseIndividualModel.CurrentExpirationDate = model.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate.HasValue ? model.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate : DateTime.Now;
                    break;
                }
            }

            var modelWithIds = ClauseRepository.SaveClauseModel(modelClause, IDBContext.Current.UserName);
            AttributeBankForTC(model.OperationId, model.mainOperationNumber);
            return RedirectToAction("Details", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = model.Contracts[0].Clauses[0].ClauseId, clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        #endregion

        #region Clause Approval Request Section

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult DetailsClauseApprovalRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            string requestNameType,
            MessageSendRequestCode state = 0,
            string messageK2 = null)
        {
            ViewBag.languages = MasterDataRep.GetMasterDataModels("Language");
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, false);
            ViewBag.ListValidationStage = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE");

            ViewBag.CategoryName = MasterDataRep.GetMasterDataNameById(
                operationModel.Contracts[0].Clauses[0].CategoryId, "EN");

            OperationRelatedModel ModelOperation = operationModel;
            ViewBag.OperationRelatedModel = ModelOperation;

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses).SelectMany(
                x => x.ClauseIndividuals).SelectMany(x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;
            foreach (var item in listDocuments)
            {
                if (item == "FULLFILLMENT LETTER")
                {
                    FlagLetter = true;
                }
            }

            ViewBag.Letter = FlagLetter;

            var ListLanguages = ClauseRepository.GetLanguages(Lang);
            ViewBag.ListLanguages = new SelectList(ListLanguages, "convergenceMasterDataId", "Name");

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(state, false, 2, messageK2);
                ViewData["message"] = message;
            }

            int? validationStageId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ValidationStageId;
            var validationStage = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE");
            bool isDraft = validationStage.Any(val => val.ConvergenceMasterDataId == validationStageId
                && ((val.Code == "CL_REJ") || (val.Code == "CL_DRAFT")));

            if (IDBContext.Current.HasPermission(Permission.CLAUSES_WRITE)
                && isDraft
                && state == 0)
            {
                return RedirectToAction("EditClauseApprovalRequest",
                       new
                       {
                           operationId = operationId,
                           contractId = contractId,
                           clauseId = clauseId,
                           clauseIndividualId = clauseIndividualId,
                           mainOperationNumber = mainOperationNumber,
                           requestNameType = requestNameType
                       });
            }

            return View(operationModel);
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseApprovalRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            string requestNameType)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, false);

            operationModel.RequestTypeName = requestNameType;

            OperationRelatedModel ModelOperation = operationModel;
            ViewBag.OperationRelatedModel = ModelOperation;

            var ListLanguages = ClauseRepository.GetLanguages(Lang);
            ViewBag.ListLanguages =
                new SelectList(ListLanguages, "convergenceMasterDataId", "Name");

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses)
                .SelectMany(x => x.ClauseIndividuals)
                .SelectMany(x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;

            foreach (var item in listDocuments)
            {
                if (item == "FULLFILLMENT LETTER")
                    FlagLetter = true;
            }

            ViewBag.Letter = FlagLetter;

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError =
                    ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveClauseRequest")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult EditClauseApprovalRequest(OperationRelatedModel model)
        {
            var modelClauseIndividual = ClauseRepository.GetClauseIndividualsById(model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId);
            modelClauseIndividual.UserComments.Clear();

            modelClauseIndividual.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);
            ClauseRepository.SaveOrUpdateClauseIndividual(modelClauseIndividual, IDBContext.Current.UserName);
            return RedirectToAction("DetailsClauseApprovalRequest", new { operationId = model.OperationId, contractId = model.Contracts[0].ContractId, clauseId = model.Contracts[0].Clauses[0].ClauseId, clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId, mainOperationNumber = model.mainOperationNumber });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SendClauseRequest")]
        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult CreateClauseApprovalRequest(OperationRelatedModel model)
        {
            UpdateFulFillComments(model);

            if (model == null)
            {
                throw new ArgumentException("Parameter cannot be null, CreateClauseApprovalRequest method");
            }

            var modelUpdated = ClauseRepository.GetClauseIndividualsById(
                                model.Contracts[0]
                                .Clauses[0].ClauseIndividuals[0]
                                .ClauseIndividualId);

            TempData["USERCOMMENTSLIST"] = modelUpdated.UserComments;

            return RedirectToAction("CreateClauseApprovalValidationRequest", new
            {
                operationId = model.OperationId,
                contractId = model.Contracts[0].ContractId,
                clauseId = model.Contracts[0].Clauses[0].ClauseId,
                clauseIndividualId =
                    model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId,
                mainOperationNumber = model.mainOperationNumber,
                additionalValidators = Request["validator_list_additional_list"],
                multiplePriorClauseIndividualIds = model.MultiplePriorClauseIndividualIds
            });
        }

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult CreateClauseApprovalValidationRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber,
            string additionalValidators,
            string multiplePriorClauseIndividualIds)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, mainOperationNumber, false);
            operationModel.User = IDBContext.Current.UserName;

            var documentNumber = ClauseRepository.GetIDBDocNumberLetter(
                "FULLFILLMENT LETTER", clauseIndividualId);
            var commentsUserList = TempData["USERCOMMENTSLIST"];
            var callInstance = AutoMapper.Mapper
                .Map<OperationRelatedModel, OperationRelatedModelWorkflow>(operationModel);
            callInstance.UserName = IDBContext.Current.UserName;
            callInstance.UserProfile = IDBContext.Current.FirstRole;
            callInstance.DocNumber = documentNumber;
            callInstance.UserComments = (IList<UserCommentModel>)commentsUserList;

            string extensionId = string.Empty;

            try
            {
                var clauseIndividuals = operationModel
                    .Contracts.First()
                    .Clauses.First()
                    .ClauseIndividuals.First();

                if (clauseIndividuals.ClauseExtension.Any())
                {
                    extensionId = clauseIndividuals
                        .ClauseExtension.First()
                        .ClauseExtensionId.ToString();
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "ClauseController", "Error in CreateClauseApprovalValidationRequest method", e);
            }

            ClauseBusinessLogic logic =
                new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            string userProfileValidators = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "4",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber ?? operationModel.OperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .ClauseIndividualId,
                extensionId,
                currentAmount);
            callInstance.UserProfileValidators = userProfileValidators;

            new ClauseBusinessLogic(_businessRuleService, _clauseService)
                .SetValidatorsToStartWorkflow(
                    userProfileValidators, additionalValidators, callInstance);

            var response = string.Empty;

            if (!string.IsNullOrEmpty(multiplePriorClauseIndividualIds))
            {
                var noK2MultiDto = new NoK2MultipleEntitiesDTO
                {
                    OperationNumber = IDBContext.Current.Operation,
                    CurrentUserName = IDBContext.Current.UserName,
                    MultipleEntityType = NoK2MultipleEntitiesEnum.MultipleClauseIndividual,
                    MainEntityId = clauseIndividualId,
                    AdditionalEntitiesIds = multiplePriorClauseIndividualIds
                        .Split(',').Select(x => int.Parse(x)).ToList(),
                    MandatoryValidators = callInstance.UserProfileValidators,
                    AdditionalValidators = callInstance.UserProfileAdditional
                };

                var noK2ServiceResponse = _noK2WorkflowsService.StartNoK2Workflow(noK2MultiDto);

                return HandleK2Response(
                    noK2ServiceResponse,
                    mainOperationNumber,
                    operationId,
                    contractId,
                    clauseId,
                    clauseIndividualId,
                    clauseExtensionId: null);
            }

            bool isOtherClauseCategory = ClauseRepository.IsOtherClauseCategory(clauseId);

            if (callInstance.UserProfileValidators == Role.TEAM_LEADER &&
                string.IsNullOrEmpty(callInstance.UserProfileAdditional) &&
                IDBContext.Current.HasRole(Role.TEAM_LEADER) &&
                isOtherClauseCategory)
            {
                response = NoK2BeginWorkflow(
                    clauseIndividualId, ClauseConstants.ClauseTypeIndividual);
            }
            else
            {
                try
                {
                    callInstance.EntityType = ClauseConstants.ClauseTypeIndividual;
                    callInstance.WorkflowType = WorkflowTypes.CLAUSE_FINAL_STATUS_VALIDATION;
                    var responseWorklow = BeginGenericWorkflow(callInstance);

                    if (responseWorklow.IsValid)
                    {
                        response = K2Response.StartWorkFlow_CL.GetStringValue();
                        ClauseContractHelper.UpdateValidationStage(
                            EntityToWorkflow.ENTITY_CLAUSE_INDIVUDUAL,
                            clauseIndividualId,
                            AgreementsAndConditionsConstants.CONDITION_STATUS_REV);
                    }
                    else
                    {
                        response = responseWorklow.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().WriteError(
                        "ClauseController",
                        "Error in CreateClauseApprovalValidationRequest method",
                        ex);
                    response = ex.Message;
                }
            }

            return HandleWorkflowResponse(
                response,
                mainOperationNumber,
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                clauseExtensionId: null);
        }

        #endregion

        #endregion

        [HasPermission(Permissions = "Clauses Write")]
        public virtual ActionResult Delete(string mainOperationNumber, int clauseId, int clauseIndividualId)
        {
            var modelClauseIndividual = ClauseRepository.GetClauseIndividualsById(clauseIndividualId);
            var clauseStatus = MasterDataRep.GetMasterDataCode(modelClauseIndividual.StatusId);

            switch (clauseStatus)
            {
                case "CL_DRAFT":
                    var modelClause = ClauseRepository.GetClauseById(clauseId);
                    foreach (var eachClauseIndividual in modelClause.ClauseIndividuals)
                    {
                        var eachClauseIndividualToDeleted = ClauseRepository.GetClauseIndividualsById(eachClauseIndividual.ClauseIndividualId);
                        eachClauseIndividualToDeleted.IsDeleted = true;
                        ClauseRepository.SaveOrUpdateClauseIndividual(eachClauseIndividualToDeleted, IDBContext.Current.UserName);
                    }

                    break;
                case "CL_TR_W":
                    modelClauseIndividual.IsDeleted = true;
                    ClauseRepository.SaveOrUpdateClauseIndividual(modelClauseIndividual, IDBContext.Current.UserName);
                    break;
            }

            return RedirectToAction("Index", "Contracts", new { area = "Clauses", operationNumber = mainOperationNumber });
        }

        public virtual ActionResult ConfirmChangedFulfillmentDate()
        {
            return PartialView();
        }

        #region EXTENSION CRUD
        #region CREATE A NEW EXTENSION / REQUEST VALIDATION FOR EXTENSION

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult CreateExtension(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseContractByOperation(
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                mainOperationNumber,
                false);

            operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension =
                new List<ClauseExtensionModel>();

            operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension
                .Add(new ClauseExtensionModel()
                {
                    RequestedBy = string.Empty,
                    Recommendation = string.Empty,
                    CommunicationNumber = string.Empty,
                    Justification = string.Empty,
                    Description = string.Empty,
                    ValidateDate = null,
                    ValidationStageId = MasterDataRep.GetMasterDataId("VALIDATION_STAGE", "CL_DRAFT")
                });

            var language = Language.EN;

            ViewBag.ValidationStageDraft = MasterDataRep
                .GetMasterDataNameByCode("VALIDATION_STAGE", "CL_DRAFT", language);

            var CurrentExpirationDate = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                .CurrentExpirationDate.HasValue ?
                (DateTime)operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate :
                DateTime.Now;

            ViewBag.CurrentExpirationDate = CurrentExpirationDate;
            ViewBag.CurrentExpirationDateRequest = operationModel.Contracts.First().Clauses.First()
                .ClauseIndividuals.First().ClauseExtension.First().RequestExtensionDate == null ?
                CurrentExpirationDate :
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                .ClauseExtension.First().RequestExtensionDate;

            var TempIDBRequest = operationModel.Contracts[0].Clauses[0]
                .ClauseIndividuals[0].ClauseExtension[0].IdbRequest;
            var IdbRequest = TempIDBRequest == null ? 0 : Convert.ToInt32(TempIDBRequest);
            ViewBag.IdbRequest = IdbRequest;

            ViewBag.RequestedDate = CurrentExpirationDate.AddMonths(IdbRequest);

            if (operationModel.IsSignatureDeadline)
            {
                var specialMonths = Convert.ToInt32(System.Configuration
                    .ConfigurationManager.AppSettings["ExtensionClauseIsSpecialMonths"]);
                operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].IdbRequest = specialMonths;
                operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].ExecutorRequest = specialMonths;
                operationModel.IsSignatureDeadline = true;
            }

            ViewBag.ListContractStatus = MasterDataRep.GetMasterDataModels("CONTRACT_CLAUSE_STATUS");
            ViewBag.Category = MasterDataRep
                .GetMasterDataCode(operationModel.Contracts[0].Clauses[0].CategoryId);

            var opTypes = OperationTypeHelper.GetOperationTypes(operationId);
            decimal currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            ViewBag.Normative = _clauseService
                .GetNormative(operationModel.OperationNumber, currentAmount);

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CreateExtension")]
        public virtual ActionResult CreateExtension(OperationRelatedModel model)
        {
            if (model.Contracts.First().Clauses.First().ClauseIndividuals.First().CurrentExpirationDate == null)
            {
                model.Contracts.First().Clauses.First().ClauseIndividuals.First().CurrentExpirationDate = Convert.ToDateTime(Request["CurrentExpirationDate"]);
            }

            ClauseRepository.SaveOrUpdateExtension(model);
            AttributeBankForTC(model.OperationId, model.mainOperationNumber);

            return RedirectToAction("Details",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    clauseId = model.Contracts[0].Clauses[0].ClauseId,
                    clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .ClauseIndividualId,
                    mainOperationNumber = model.mainOperationNumber
                });
        }

        [HttpPost]
        public virtual ActionResult CreateExtensionSpecial(OperationRelatedModel model)
        {
            return CreateExtensionRequest(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save&RequestExtension")]
        public virtual ActionResult CreateExtensionRequest(OperationRelatedModel model)
        {
            var auxiliarModel = ClauseRepository.SaveOrUpdateExtension(model);

            return RedirectToAction(
                "DetailsExtensionApprovalRequest",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    clauseId = model.Contracts[0].Clauses[0].ClauseId,
                    clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId,
                    clauseExtensionId =
                        model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId > 0 ?
                            model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId :
                            auxiliarModel.ClauseExtensionId,
                    mainOperationNumber = model.mainOperationNumber,
                    IsSpecialExtensionCheck = model.IsSpecialExtensionCheck
                });
        }

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult DetailsExtensionApprovalRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int clauseExtensionId,
            string mainOperationNumber,
            MessageSendRequestCode state = 0,
            string messageK2 = "",
            string stateExtension = "",
            bool? IsSpecialExtensionCheck = null)
        {
            Logger.GetLogger().WriteMessage("MFH - ClauseController", string.Format(
                "'DetailsExtensionApprovalRequest' method: starting Request, curren User= {0}, current Operation= {1}",
                IDBContext.Current.UserName,
                IDBContext.Current.Operation));

            ViewBag.ExtencionLetter = clauseExtensionId;
            var operationModel = ClauseRepository.GetClauseExtensionContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, clauseExtensionId, mainOperationNumber);

            operationModel.IsSpecialExtensionCheck = IsSpecialExtensionCheck;
            ViewBag.ListValidationStage = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE");
            ViewBag.stateExtension = stateExtension;
            OperationRelatedModel ModelOperation = operationModel;

            var category = MasterDataRep.GetMasterDataCode(
                operationModel.Contracts[0].Clauses[0].CategoryId);
            ViewBag.OperationRelatedModel = ModelOperation;

            string extensionId = string.Empty;

            try
            {
                extensionId = string.IsNullOrEmpty(operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .ClauseExtension.First().ClauseExtensionId.ToString()) ? string.Empty :
                    operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .ClauseExtension.First().ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);

            string userProfileValidators = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "3",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .ClauseIndividualId,
                extensionId,
                currentAmount);

            var List_ = userProfileValidators.Split('|');

            var CurrentRol = IDBContext.Current.Roles;
            ViewBag.SendRequest = true;

            if (List_ != null && category == ClauseConstants.LAST_DISBURMENT)
            {
                for (int i = 0; i < List_.Length; i++)
                {
                    var ContainsRole = CurrentRol.Contains(List_[i]);

                    if (ContainsRole == true)
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            var ListLanguages = ClauseRepository.GetLanguages(Lang);
            ViewBag.ListLanguages = new SelectList(ListLanguages, "convergenceMasterDataId", "Name");

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses).SelectMany(
                x => x.ClauseIndividuals).SelectMany(x => x.ClauseExtension).SelectMany(
                x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;
            foreach (var item in listDocuments)
            {
                if (item == "EXTENSION LETTER")
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

            int? validationStageId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                .ClauseExtension[0].ValidationStageId;
            var validationStage = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE");
            bool notExistsValStageCL_REV = validationStage.Any(
                val => val.ConvergenceMasterDataId == validationStageId && !val.Code.StartsWith("CL_REV"));

            if (notExistsValStageCL_REV)
            {
                return RedirectToAction(
                    "EditExtensionApprovalRequest",
                    "Clause",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        clauseId = clauseId,
                        clauseIndividualId = clauseIndividualId,
                        clauseExtensionId = clauseExtensionId,
                        mainOperationNumber = mainOperationNumber,
                        IsSpecialExtensionCheck = operationModel.IsSpecialExtensionCheck
                    });
            }

            return View(operationModel);
        }

        public virtual JsonResult DetermineLASTDExtensionMandatoryValidator(
            OperationRelatedModel operationModel)
        {
            var clauseExtensionModel = ClauseRepository.SaveOrUpdateExtension(operationModel);

            operationModel = ClauseRepository
                .GetClauseExtensionContractByOperation(
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .ClauseIndividualId,
                clauseExtensionModel.ClauseExtensionId,
                _operationRepository
                    .GetByCriteria(a => a.OperationId == operationModel.OperationId)
                    .Select(a => a.OperationNumber).First());

            try
            {
                string mandatoryValidator;
                string popupMessage;

                var logic = new ClauseBusinessLogic(_businessRuleService, _clauseService);

                logic.GetValidatorAndPopupMessage(
                    RuleService,
                    operationModel,
                    ClauseBusinessLogic.CLAUSES_EXTENSION_NUMBER,
                    operationModel.OperationId,
                    operationModel.Contracts.First().ContractId,
                    operationModel.mainOperationNumber,
                    operationModel.Contracts.First().Clauses.First().ClauseId,
                    operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                        .ClauseIndividualId,
                    clauseExtensionModel.ClauseExtensionId.ToString(),
                    ClauseRepository.CalculateCurrentAmount(operationModel.OperationId, 1),
                    out mandatoryValidator,
                    out popupMessage);

                return Json(new
                {
                    IsValid = !mandatoryValidator.Contains("Error"),
                    ErrorMessage = mandatoryValidator,
                    IsValidPopup = string.IsNullOrEmpty(popupMessage),
                    PopupMsg = popupMessage,
                    Data = new
                    {
                        Validators = mandatoryValidator,
                        ContractId = operationModel.Contracts.First().ContractId,
                        ClauseId = operationModel.Contracts.First().Clauses.First().ClauseId,
                        ClauseIndividualId = operationModel.Contracts.First().Clauses.First()
                            .ClauseIndividuals.First().ClauseIndividualId,
                        ClauseExtensionId = clauseExtensionModel.ClauseExtensionId.ToString()
                    }
                },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                string nullString = null;

                return Json(
                new
                {
                    IsValid = false,
                    ErrorMessage = e.Message,
                    IsValidPopup = false,
                    PopupMsg = nullString,
                    Data = new
                    {
                        Validators = nullString,
                        ContractId = operationModel.Contracts.First().ContractId,
                        ClauseId = operationModel.Contracts.First().Clauses.First().ClauseId,
                        ClauseIndividualId = operationModel.Contracts.First().Clauses.First()
                            .ClauseIndividuals.First().ClauseIndividualId,
                        ClauseExtensionId = clauseExtensionModel.ClauseExtensionId.ToString()
                    }
                },
                JsonRequestBehavior.AllowGet);
            }
        }

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult EditExtensionApprovalRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int clauseExtensionId,
            string mainOperationNumber,
            bool? IsSpecialExtensionCheck = null)
        {
            Logger.GetLogger().WriteMessage("MFH - ClauseController", string.Format(
                "'EditExtensionApprovalRequest' method: starting Request, curren User= {0}, current Operation= {1}",
                IDBContext.Current.UserName,
                IDBContext.Current.Operation));

            ViewBag.ExtencionLetter = clauseExtensionId;

            var operationModel = ClauseRepository.GetClauseExtensionContractByOperation(
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                clauseExtensionId,
                mainOperationNumber);
            operationModel.IsSpecialExtensionCheck = IsSpecialExtensionCheck;

            OperationRelatedModel ModelOperation = operationModel;

            ViewBag.OperationRelatedModel = ModelOperation;

            string extensionId = string.Empty;

            try
            {
                extensionId = string.IsNullOrEmpty(
                    operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First()
                        .ClauseExtension.First().ClauseExtensionId.ToString()) ?
                            string.Empty :
                            operationModel.Contracts.First().Clauses.First()
                                .ClauseIndividuals.First().ClauseExtension.First()
                                .ClauseExtensionId.ToString();
            }
            catch
            {
                extensionId = string.Empty;
            }

            ClauseBusinessLogic logic =
                new ClauseBusinessLogic(_businessRuleService, _clauseService);
            var currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);

            string userProfileValidators = logic.GetUserProfileValidators(
                RuleService,
                operationModel,
                "3",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First()
                    .ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);
            var List_ = userProfileValidators.Split('|');
            var CurrentRol = IDBContext.Current.Roles;

            ViewBag.SendRequest = true;

            if (List_ != null && ModelOperation.CategoryClause == ClauseConstants.LAST_DISBURMENT)
            {
                for (int i = 0; i < List_.Length; i++)
                {
                    var ContainsRole = CurrentRol.Contains(List_[i]);

                    if (ContainsRole == true)
                    {
                        ViewBag.SendRequest = false;
                        break;
                    }
                }
            }

            var ListLanguages = ClauseRepository.GetLanguages(Lang);
            ViewBag.ListLanguages =
                new SelectList(ListLanguages, "convergenceMasterDataId", "Name");

            var listDocuments = operationModel.Contracts.SelectMany(x => x.Clauses)
                .SelectMany(x => x.ClauseIndividuals)
                .SelectMany(x => x.Documents).Select(x => x.Description).ToList();
            bool FlagLetter = false;

            foreach (var item in listDocuments)
            {
                if (item == "EXTENSION LETTER")
                    FlagLetter = true;
            }

            ViewBag.Letter = FlagLetter;

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError =
                    ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            Logger.GetLogger().WriteMessage(
                "MFH - ClauseController",
                "'EditExtensionApprovalRequest' method: finishing Request, sending model to view");

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveExtensionRequest")]
        public virtual ActionResult EditExtensionApprovalRequest(OperationRelatedModel operationModel)
        {
            UpdateExtensionComments(operationModel);
            return RedirectToAction("DetailsExtensionApprovalRequest",
                new
                {
                    operationId = operationModel.OperationId,
                    contractId = operationModel.Contracts[0].ContractId,
                    clauseId = operationModel.Contracts[0].Clauses[0].ClauseId,
                    clauseIndividualId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId,
                    clauseExtensionId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId,
                    mainOperationNumber = operationModel.mainOperationNumber
                });
        }

        [HttpPost]
        public virtual ActionResult CreateExtensionApprovalRequest(OperationRelatedModel model)
        {
            UpdateExtensionComments(model);

            if (model == null)
            {
                throw new ArgumentException("Parameter cannot be null,CreateExtensionApprovalRequest method");
            }

            var modelUpdated = ClauseRepository
                .GetExtensionById(model.Contracts[0].Clauses[0]
                .ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId);
            TempData["USERCOMMENTSLIST"] = modelUpdated.UserComments;
            return RedirectToAction("CreateExtensionApprovalValidationRequest",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    clauseId = model.Contracts[0].Clauses[0].ClauseId,
                    clauseIndividualId = model.Contracts[0].Clauses[0]
                        .ClauseIndividuals[0].ClauseIndividualId,
                    clauseExtensionId = model.Contracts[0].Clauses[0]
                        .ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId,
                    mainOperationNumber = model.mainOperationNumber,
                    additionalValidators = Request["validator_list_additional_list"],
                    multiplePriorClauseIndividualIds = model.MultiplePriorClauseIndividualIds
                });
        }

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult CreateExtensionApprovalValidationRequest(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int clauseExtensionId,
            string mainOperationNumber,
            string additionalValidators,
            string multiplePriorClauseIndividualIds)
        {
            var operationModel = ClauseRepository
                .GetClauseExtensionContractByOperation(
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                clauseExtensionId,
                mainOperationNumber);

            var commentsUserList = TempData["USERCOMMENTSLIST"];

            var documentNumber = ClauseRepository
                .GetIDBDocNumberLetter("EXTENSION LETTER", clauseExtensionId);

            var callInstance = AutoMapper.Mapper
                .Map<OperationRelatedModel, OperationRelatedModelWorkflow>(operationModel);

            callInstance.UserName = IDBContext.Current.UserName;
            callInstance.UserProfile = IDBContext.Current.FirstRole;
            callInstance.DocNumber = documentNumber;
            callInstance.UserProfileAdditional = additionalValidators;
            callInstance.UserComments = (IList<UserCommentModel>)commentsUserList;

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
                "3",
                operationModel.OperationId,
                operationModel.Contracts.First().ContractId,
                operationModel.mainOperationNumber,
                operationModel.Contracts.First().Clauses.First().ClauseId,
                operationModel.Contracts.First().Clauses.First().ClauseIndividuals.First().ClauseIndividualId,
                extensionId,
                currentAmount);
            callInstance.UserProfileValidators = UserProfileValidators_;

            new ClauseBusinessLogic(_businessRuleService, _clauseService)
                .SetValidatorsToStartWorkflow(UserProfileValidators_, additionalValidators, callInstance);

            var response = string.Empty;

            if (!string.IsNullOrEmpty(multiplePriorClauseIndividualIds))
            {
                var additionalExtensionIds = _noK2WorkflowsService.CreateAdditionalClauseExtensions(
                    clauseExtensionId,
                    multiplePriorClauseIndividualIds.Split(',').Select(x => int.Parse(x)),
                    IDBContext.Current.UserName);

                var noK2MultiDto = new NoK2MultipleEntitiesDTO
                {
                    OperationNumber = IDBContext.Current.Operation,
                    CurrentUserName = IDBContext.Current.UserName,
                    MultipleEntityType = NoK2MultipleEntitiesEnum.MultipleClauseExtension,
                    MainEntityId = clauseExtensionId,
                    AdditionalEntitiesIds = additionalExtensionIds,
                    MandatoryValidators = callInstance.UserProfileValidators,
                    AdditionalValidators = callInstance.UserProfileAdditional
                };

                var noK2ServiceResponse = _noK2WorkflowsService.StartNoK2Workflow(noK2MultiDto);

                return HandleK2Response(
                    noK2ServiceResponse,
                    mainOperationNumber,
                    operationId,
                    contractId,
                    clauseId,
                    clauseIndividualId,
                    clauseExtensionId: null);
            }

            if (callInstance.UserProfileValidators == Role.TEAM_LEADER &&
                string.IsNullOrEmpty(callInstance.UserProfileAdditional) &&
                IDBContext.Current.HasRole(Role.TEAM_LEADER))
            {
                response = NoK2BeginWorkflow(
                    clauseExtensionId, ClauseConstants.ClauseTypeExtension);
            }
            else
            {
                new CommonDocument().Log(
                    LogType.Debug,
                    "K2 EXECUTION",
                    string.Format("CLAUSE EXTENSION-BEFORE OPERATIONID:{0}",
                        IDBContext.Current.Operation), callInstance);

                try
                {
                    callInstance.EntityType = ClauseConstants.ClauseTypeExtension;
                    callInstance.WorkflowType = WorkflowTypes.CLAUSE_EXTENSION_APPROVAL;

                    var responseWorklow = BeginGenericWorkflow(callInstance);
                    if (responseWorklow.IsValid)
                    {
                        response = K2Response.StartWorkFlow_CL.GetStringValue();
                        ClauseContractHelper.UpdateValidationStage(
                            EntityToWorkflow.ENTITY_CLAUSE_EXTENSION,
                            clauseExtensionId,
                            AgreementsAndConditionsConstants.CONDITION_STATUS_REV);
                    }
                    else
                    {
                        response = responseWorklow.ErrorMessage;
                    }
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().WriteError(
                        "ClauseController",
                        "Error in CreateExtensionApprovalValidationRequest method",
                        ex);

                    throw;
                }
            }

            return HandleWorkflowResponse(
                response,
                mainOperationNumber,
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                clauseExtensionId);
        }
        #endregion

        public virtual ActionResult DetailsExtensionById(string operationNumber, int clauseExtensionId)
        {
            var clauseExtensionModel = ClauseRepository.GetExtensionById(clauseExtensionId);
            var clauseIndividualModel = ClauseRepository.GetClauseIndividualsById(clauseExtensionModel.ClauseIndividualId);
            var clauseModel = ClauseRepository.GetClauseById(clauseIndividualModel.ClauseId);
            var contractModel = _clauseService.GetContract(operationNumber, clauseModel.ClauseId);

            return RedirectToAction("DetailsExtension",
                 new
                 {
                     operationId = contractModel.Model.OperationId,
                     contractId = contractModel.Model.ContractId,
                     clauseId = clauseModel.ClauseId,
                     clauseIndividualId = clauseIndividualModel.ClauseIndividualId,
                     clauseExtensionId = clauseExtensionId,
                     mainOperationNumber = operationNumber
                 });
        }

        public virtual ActionResult DetailsExtension(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int clauseExtensionId,
            string mainOperationNumber,
            string requestDate)
        {
            var operationModel = ClauseRepository.GetClauseExtensionContractByOperation(
                operationId, contractId, clauseId, clauseIndividualId, clauseExtensionId, mainOperationNumber);

            int? validationStageId = operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                .ClauseExtension[0].ValidationStageId;

            var validationStage = string.Empty;
            if (validationStageId != null
                && validationStageId > 0)
            {
                var language = Language.EN;
                validationStage = MasterDataRep.GetMasterDataNameById((int)validationStageId, language);
            }

            ViewBag.ValidationStage = validationStage;
            ViewBag.ListValidationStage = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE");
            ViewBag.ListContractStatus = MasterDataRep.GetMasterDataModels("CONTRACT_CLAUSE_STATUS");

            if (requestDate == null)
            {
                if (operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].RequestExtensionDate.HasValue)
                {
                    requestDate = Convert.ToDateTime(operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .ClauseExtension[0].RequestExtensionDate).ToString("dd MMM yyyy");
                }
                else
                {
                    requestDate = Convert.ToDateTime(operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .CurrentExpirationDate).AddMonths((int)(operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .ClauseExtension[0].IdbRequest == null ? 0 : operationModel.Contracts[0].Clauses[0]
                        .ClauseIndividuals[0].ClauseExtension[0].IdbRequest)).ToString("dd MMM yyyy");
                }
            }

            ViewBag.RequestDate = requestDate;

            if (IDBContext.Current.HasPermission(Permission.EXTENSION_WRITE))
            {
                var validationStageList = MasterDataRep.GetMasterDataModels("VALIDATION_STAGE")
                    .FirstOrDefault(x => x.ConvergenceMasterDataId == validationStageId);

                if (validationStageList != null && !validationStageList.Code.StartsWith("CL_REV")
                    && ViewBag.ValidationStage != "APPROVED")
                {
                    return RedirectToAction(
                        "EditExtension",
                        "Clause",
                        new
                        {
                            area = "Clauses",
                            operationId = operationModel.OperationId,
                            contractId = operationModel.Contracts[0].ContractId,
                            clauseId = operationModel.Contracts[0].Clauses[0].ClauseId,
                            clauseIndividualId = operationModel.Contracts[0].Clauses[0]
                            .ClauseIndividuals[0].ClauseIndividualId,
                            clauseExtensionId = operationModel.Contracts[0].Clauses[0]
                            .ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId,
                            mainOperationNumber = operationModel.mainOperationNumber
                        });
                }
            }

            return View(operationModel);
        }

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult EditExtension(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int clauseExtensionId,
            string mainOperationNumber)
        {
            var operationModel = ClauseRepository.GetClauseExtensionContractByOperation(
                operationId,
                contractId,
                clauseId,
                clauseIndividualId,
                clauseExtensionId,
                mainOperationNumber);

            int? valStage = operationModel
                .Contracts[0]
                .Clauses[0]
                .ClauseIndividuals[0]
                .ClauseExtension[0]
                .ValidationStageId;

            ViewBag.ValidationStage = string.Empty;

            if (valStage != null && valStage > 0)
            {
                ViewBag.ValidationStage = MasterDataRep
                    .GetMasterDataNameById(valStage.Value, IDBContext.Current.CurrentLanguage);
            }

            var currentExtensionRequestDate = operationModel
                .Contracts[0]
                .Clauses[0]
                .ClauseIndividuals[0]
                .ClauseExtension[0]
                .RequestExtensionDate;

            currentExtensionRequestDate = currentExtensionRequestDate ??
                operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0].CurrentExpirationDate;
            ViewBag.CurrentExpirationDate = currentExtensionRequestDate.Value;

            var idbRequest = (int)(operationModel
                .Contracts[0]
                .Clauses[0]
                .ClauseIndividuals[0]
                .ClauseExtension[0]
                .IdbRequest ?? 0);

            ViewBag.IdbRequest = idbRequest;
            ViewBag.RequestedDate = currentExtensionRequestDate.Value.AddMonths(idbRequest);
            ViewBag.ListContractStatus = MasterDataRep
                .GetMasterDataModels("CONTRACT_CLAUSE_STATUS");
            ViewBag.Category = MasterDataRep
                .GetMasterDataCode(operationModel.Contracts[0].Clauses[0].CategoryId);

            var opTypes = OperationTypeHelper.GetOperationTypes(operationId);
            decimal currentAmount = ClauseRepository.CalculateCurrentAmount(operationId, 1);
            ViewBag.Normative = _clauseService.GetNormative(mainOperationNumber, currentAmount);

            ViewBag.ListClauseCategory = GetCategoriesByClauses(operationModel);

            if (operationModel.IsSignatureDeadline)
            {
                var specialMonths = Convert.ToInt32(System.Configuration
                    .ConfigurationManager.AppSettings["ExtensionClauseIsSpecialMonths"]);
                operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].IdbRequest = specialMonths;
                operationModel.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].ExecutorRequest = specialMonths;
                operationModel.IsSignatureDeadline = true;
            }

            return View(operationModel);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "EditExtension")]
        public virtual ActionResult EditExtension(OperationRelatedModel model)
        {
            if (model.Contracts.First()
                .Clauses.First().ClauseIndividuals.First().CurrentExpirationDate == null)
            {
                model.Contracts.First().Clauses.First().ClauseIndividuals.First()
                    .CurrentExpirationDate = Convert.ToDateTime(Request["CurrentExpirationDate"]);
            }

            model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                .ClauseExtension[0] = ClauseRepository.SaveOrUpdateExtension(model);

            return RedirectToAction("DetailsExtension",
                new
                {
                    operationId = model.OperationId,
                    contractId = model.Contracts[0].ContractId,
                    clauseId = model.Contracts[0].Clauses[0].ClauseId,
                    clauseIndividualId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .ClauseIndividualId,
                    clauseExtensionId = model.Contracts[0].Clauses[0].ClauseIndividuals[0]
                        .ClauseExtension[0].ClauseExtensionId,
                    mainOperationNumber = model.mainOperationNumber
                });
        }

        [HasPermission(Permissions = "Extension Write")]
        public virtual ActionResult DeleteExtension(int operationId, int contractId, int clauseId, int clauseIndividualId, int clauseExtensionId, string mainOperationNumber)
        {
            ClauseRepository.DeleteExtensionById(clauseExtensionId);
            return RedirectToAction("Edit", new { operationId = operationId, contractId = contractId, clauseId = clauseId, clauseIndividualId = clauseIndividualId, mainOperationNumber = mainOperationNumber });
        }

        public virtual ActionResult SaveClauseIndividualReturned(
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            string mainOperationNumber)
        {
            var response = _clauseIndividualReturnedService
                .UpdateGeneralForClauseReturned(mainOperationNumber, clauseIndividualId);

            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return RedirectToAction("Details",
                new
                {
                    operationId = operationId,
                    contractId = contractId,
                    clauseId = clauseId,
                    clauseIndividualId = clauseIndividualId,
                    mainOperationNumber = mainOperationNumber
                });
        }
        #endregion

        #region LETTER GENERATION

        private void CleanDirtyString(string response)
        {
            response = response.Replace((char)13, ' ');
            response = response.Replace("'", string.Empty);
            response = response.Replace('"', ' ');
            response = response.Replace('.', '_');
            response = response.Replace("\n", string.Empty).Replace("\r", string.Empty);
            response = response.Replace(@"\\", string.Empty);
            response = response.Replace(@";", string.Empty);
            response = response.Replace(@"=\", string.Empty);
            response = response.Replace(@"<", string.Empty);
            response = response.Replace(@">", string.Empty);
        }

        #endregion

        #region Modal Windows for Notification/UserValidation

        [ExceptionHandling]
        public virtual ActionResult DeleteItem(int operationId, int contractId, int clauseId, int clauseIndividualId, int extensionId, int RevolvingFund, string entityRelated, int year, int entityRegisterId, int visualProjectId, int visualProjectVersionId, int documentId, int userCommentId, string itemToDelete, string mainOperationNumber)
        {
            ViewBag.operationId = operationId;
            ViewBag.contractId = contractId;
            ViewBag.clauseId = clauseId;
            ViewBag.clauseIndividualId = clauseIndividualId;
            ViewBag.extensionId = extensionId;
            ViewBag.RevolvingFund = RevolvingFund;
            ViewBag.entityRelated = entityRelated;
            ViewBag.entityRegisterId = entityRegisterId;
            ViewBag.visualProjectId = visualProjectId;
            ViewBag.year = year;
            ViewBag.visualProjectVersionId = visualProjectVersionId;
            ViewBag.documentId = documentId;
            ViewBag.userCommentId = userCommentId;
            ViewBag.itemtodelete = itemToDelete;
            ViewBag.MainOperationNumber = mainOperationNumber;

            return PartialView();
        }

        [ExceptionHandling]
        public virtual ActionResult TLAnulationRequestAction()
        {
            return PartialView();
        }
        #endregion

        #region Types categories
        public int GetCategoryId(int operationId, int contractId, string mainOperationNumber)
        {
            var operationModel = ClauseRepository
                .GetContractByOperation(operationId, contractId, mainOperationNumber);

            var codeMasterData = MasterDataRep
                .GetMasterDataModelById(operationModel.Contracts[0].LmsTypeId != null ?
                operationModel.Contracts[0].LmsTypeId.Value :
                -1);

            var currentAmountContract = ClauseRepository
                .CurrentAmountContract(operationModel.Contracts[0].ContractId);

            var response = _clauseService.GetClauseCategory(
                mainOperationNumber, currentAmountContract, ClauseConstants.LAST_DISBURMENT);

            if (!response.IsValid)
                return -1;

            return response.Model.ConvergenceMasterDataId;
        }

        public IList<ConvergenceMasterDataModel> GetCategoriesByClauses(OperationRelatedModel operationModel)
        {
            if (!operationModel.Contracts.HasAny())
            {
                return new List<ConvergenceMasterDataModel>();
            }

            var currentAmountContract = ClauseRepository
                .CurrentAmountContract(operationModel.Contracts[0].ContractId);

            return ClauseRepository
                .GetClauseCategories(operationModel.OperationId, currentAmountContract);
        }

        #endregion

        private string GetApp_Id(string Extension)
        {
            string App_Id = string.Empty;

            switch (Extension)
            {
                case ".pdf":
                    App_Id = "ACROBAT";
                    break;
                case ".xls":
                    App_Id = "MS EXCEL";
                    break;
                case ".xlsx":
                    App_Id = "MS EXCEL";
                    break;
                case ".pst":
                    App_Id = "MS OUTLOOK";
                    break;
                case ".ppt":
                    App_Id = "MS POWERPOINT";
                    break;
                case ".pptx":
                    App_Id = "MS POWERPOINT";
                    break;
                case ".mpp":
                    App_Id = "MS PROJECT";
                    break;
                case ".pub":
                    App_Id = "MS PUBLISHER";
                    break;
                case ".vsd":
                    App_Id = "VISIO";
                    break;
                case ".vss":
                    App_Id = "VISIO";
                    break;
                case ".vst":
                    App_Id = "VISIO";
                    break;
                case ".vdx":
                    App_Id = "VISIO";
                    break;
                case ".vsx":
                    App_Id = "VISIO";
                    break;
                case ".vtx":
                    App_Id = "VISIO";
                    break;
                case ".doc":
                    App_Id = "MS WORD";
                    break;
                case ".docx":
                    App_Id = "MS WORD";
                    break;
                case ".wpd":
                    App_Id = "WORDPERFECT";
                    break;
                case ".msg":
                    App_Id = "MS OUTLOOK";
                    break;
                default:
                    App_Id = "MS WORD";
                    break;
            }

            return App_Id;
        }

        RedirectToRouteResult HandleK2Response(
    string response,
    string opNumber,
    int operationId,
    int contractId,
    int clauseId,
    int clauseIndividualId,
    int? clauseExtensionId)
        {
            if (response == K2Response.StartWorkFlow_CL.GetStringValue())
            {
                return RedirectToAction(
                    "Index",
                    "Contracts",
                    new
                    {
                        area = "Clauses",
                        operationNumber = opNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 700,
                        messageK2 = response
                    });
            }

            if (response == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(response);
                return RedirectToAction(
                    "DetailsClauseApprovalRequest",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        clauseId = clauseId,
                        clauseIndividualId = clauseIndividualId,
                        mainOperationNumber = opNumber,
                        state = 555,
                        messageK2 = response
                    });
            }

            if (response == K2Response.NOK2_OK.GetStringValue())
            {
                return RedirectToAction(
                    "Index",
                    "Contracts",
                    new
                    {
                        area = "Clauses",
                        operationNumber = opNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 666,
                        messageK2 = response
                    });
            }

            IDBContext.Current.ErrorMessage(response);

            if (clauseExtensionId.HasValue)
            {
                return RedirectToAction("DetailsExtensionApprovalRequest", new
                {
                    operationId = operationId,
                    contractId = contractId,
                    clauseId = clauseId,
                    clauseIndividualId = clauseIndividualId,
                    clauseExtensionId = clauseExtensionId,
                    mainOperationNumber = opNumber,
                    state = 600,
                    messageK2 = response
                });
            }

            return RedirectToAction("DetailsClauseApprovalRequest", new
            {
                operationId = operationId,
                contractId = contractId,
                clauseId = clauseId,
                clauseIndividualId = clauseIndividualId,
                mainOperationNumber = opNumber,
                state = 600,
                messageK2 = response
            });
        }

        string NoK2BeginWorkflow(int entityId, string clauseType)
        {
            var noK2ServiceProxy = new NoK2ServiceProxy();

            var noK2Model = new NoK2Model
            {
                EntityId = entityId,
                UserName = IDBContext.Current.UserName,
                OperationNumber = IDBContext.Current.Operation
            };

            switch (clauseType)
            {
                case ClauseConstants.ClauseTypeExtension:
                    noK2ServiceProxy.BeginWorkflow(K2CallType.ClauseExtension, noK2Model);
                    break;
                case ClauseConstants.ClauseTypeIndividual:
                    noK2ServiceProxy.BeginWorkflow(K2CallType.ClauseIndividual, noK2Model);
                    break;
                default:
                    break;
            }

            return K2Response.NOK2_OK.GetStringValue();
        }

        RedirectToRouteResult HandleWorkflowResponse(
            string response,
            string opNumber,
            int operationId,
            int contractId,
            int clauseId,
            int clauseIndividualId,
            int? clauseExtensionId)
        {
            if (response == K2Response.StartWorkFlow_CL.GetStringValue())
            {
                return RedirectToAction(
                    "Index",
                    "Contracts",
                    new
                    {
                        area = "Clauses",
                        operationNumber = opNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 700,
                        messageK2 = response
                    });
            }

            if (response == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(response);
                return RedirectToAction(
                    "DetailsClauseApprovalRequest",
                    new
                    {
                        operationId = operationId,
                        contractId = contractId,
                        clauseId = clauseId,
                        clauseIndividualId = clauseIndividualId,
                        mainOperationNumber = opNumber,
                        state = 555,
                        messageK2 = response
                    });
            }

            if (response == K2Response.NOK2_OK.GetStringValue())
            {
                return RedirectToAction(
                    "Index",
                    "Contracts",
                    new
                    {
                        area = "Clauses",
                        operationNumber = opNumber,
                        clauseTypeId = string.Empty,
                        clauseStatusId = string.Empty,
                        clauseCategoryId = string.Empty,
                        clauseNumber = string.Empty,
                        isSpecial = string.Empty,
                        expirationDateFrom = string.Empty,
                        expirationDateTo = string.Empty,
                        state = 666,
                        messageK2 = response
                    });
            }

            IDBContext.Current.ErrorMessage(response);

            if (clauseExtensionId.HasValue)
            {
                return RedirectToAction("DetailsExtensionApprovalRequest", new
                {
                    operationId = operationId,
                    contractId = contractId,
                    clauseId = clauseId,
                    clauseIndividualId = clauseIndividualId,
                    clauseExtensionId = clauseExtensionId,
                    mainOperationNumber = opNumber,
                    state = 600,
                    messageK2 = response
                });
            }

            return RedirectToAction("DetailsClauseApprovalRequest", new
            {
                operationId = operationId,
                contractId = contractId,
                clauseId = clauseId,
                clauseIndividualId = clauseIndividualId,
                mainOperationNumber = opNumber,
                state = 600,
                messageK2 = response
            });
        }

        private bool HasMilestonTypeTrenchClauseOperation(OperationRelatedModel operationModel)
        {
            return ClauseRepository.HasMilestonTypeTrenchOperation(
                operationModel.OperationId,
                operationModel.Contracts[0].Clauses[0].ClauseId);
        }

        private bool AttributeBankForTC(int OperationId, string OperationNumber)
        {
            var opTypes = OperationTypeHelper.GetOperationTypes(OperationId).First();
            this.ViewBag.isTCOperation = opTypes == OperationType.TCP;
            this.ViewBag.TypeBank = false;

            if (opTypes == OperationType.TCP)
            {
                var attributesExecuteBy = _attributesEngineService.GetAttributeValueByCode(OperationNumber,
                        AttributeCode.EXECUTED_BY);
                if (attributesExecuteBy.IsValid && attributesExecuteBy.Value != null)
                {
                    var convergenceMasterCodeByIdResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(
                        int.Parse(attributesExecuteBy.FormAttribute.InitialValue));
                    this.ViewBag.TypeBank = convergenceMasterCodeByIdResponse.Code == AttributeValue.BANK;
                }
            }

            return ViewBag.TypeBank;
        }

        private IList<SelectListItem> ContractRelationConvertToList(List<ConvergenceMasterDataModel> contractRelationQuery)
        {
            var contractRelationQueryList = new List<SelectListItem>();
            foreach (var clauseLocation in contractRelationQuery)
            {
                var item = new SelectListItem();
                item.Value = clauseLocation.ConvergenceMasterDataId.ToString();
                item.Text = clauseLocation.NameEn;
                contractRelationQueryList.Add(item);
            }

            return contractRelationQueryList;
        }

        private RedirectToRouteResult RedirectFailsCreation(string operationNumber)
        {
            var messageStatus = MessageNotificationCodes.LastDisbursmentFail;

            return RedirectToAction("Index",
                "Contracts",
                new
                {
                    area = "Clauses",
                    operationNumber = operationNumber,
                    messageStatus = messageStatus
                });
        }

        private bool ValidationVisibleForClauseReturned(ClauseIndividualModel clauseIndividualModel)
        {
            var response = _clauseIndividualReturnedService
                .IsVisibleButtonClauseIndividualReturned(clauseIndividualModel);

            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return response.HasCondition;
        }

        private bool IsTLOnlyApproverClauseFinalStatusValidation(ClauseIndividualModel clauseIndividualModel)
        {
            var response = _clauseIndividualReturnedService
               .IsTLOnlyApproverClauseFinalStatusValidation(clauseIndividualModel);

            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return response.HasCondition;
        }

        private bool HasClauseIndividualReturnedHistory(ClauseIndividualModel clauseIndividualModel)
        {
            var response = _clauseIndividualReturnedService
               .HasClauseIndividualReturnedHistory(clauseIndividualModel);

            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return response.HasCondition;
        }

        private IList<ClauseIndividualReturnedViewModel> GetAllClauseIndividualReturned(ClauseIndividualModel clauseIndividual)
        {
            var response = _clauseIndividualReturnedService.GetAllClauseIndividualReturned(clauseIndividual);
            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return response.Models.ToList();
        }

        bool IsSignatureOrRatificationOrLegalReport(
            ClauseIndividualModel clauseIndividualModel)
        {
            var response = _clauseIndividualReturnedService
               .IsSignatureOrRatificationOrLegalReport(clauseIndividualModel);

            if (!response.IsValid)
            {
                throw new ApplicationException(response.ErrorMessage);
            }

            return response.HasCondition;
        }

        private ResponseBase BeginGenericWorkflow(OperationRelatedModelWorkflow callInstance)
        {
            var workflowRequest = new WorkflowCreationRequest
            {
                Parameters = new Dictionary<string, string>()
            };

            workflowRequest.CreateUser = ClauseHelper.GetUserName(callInstance);
            workflowRequest.CreateProfile = callInstance.UserProfile;
            workflowRequest.EntityType = callInstance.EntityType;
            workflowRequest.OperationNumber = callInstance.MainOperationNumber;
            workflowRequest.WorkflowType = callInstance.WorkflowType;
            workflowRequest.ContractNumber = callInstance.Contracts.First().ContractNumber;

            if (callInstance.UserComments.HasAny())
            {
                workflowRequest.UserCommentIds = callInstance
                    .UserComments.Select(x => x.UserCommentId).ToList();
            }

            if (callInstance.UserProfileAdditional != null)
            {
                workflowRequest.AdditionalValidators = callInstance
                    .UserProfileAdditional.Split(new char[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
            }

            workflowRequest.MandatoryValidators = callInstance
                .UserProfileValidators.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            var codeList = ValidatorHelpers
                .ConvertProfileToCode(workflowRequest.MandatoryValidators, workflowRequest.WorkflowType);
            if (codeList.Any())
            {
                workflowRequest.CodeValidators = codeList;
                workflowRequest.MandatoryValidators = null;
            }

            var firstClause = callInstance.Contracts.First().Clauses.First();
            var clauseNumber = firstClause.ClauseNumber;

            if (workflowRequest.EntityType == ClauseConstants.ClauseTypeExtension)
            {
                SetParametersForExtension(workflowRequest, firstClause, clauseNumber);
            }

            if (workflowRequest.EntityType == ClauseConstants.ClauseTypeIndividual)
            {
                SetParametersForClause(workflowRequest, firstClause, clauseNumber);
            }

            var response = _workflowManager.InitiateWorkflow(workflowRequest);

            return response;
        }

        private void SetParametersForExtension(
            WorkflowCreationRequest workflowRequest,
             ClauseModel clause,
            string clauseNumber)
        {
            var clauseIndividual = clause.ClauseIndividuals.First();
            var clauseExtension = clauseIndividual.ClauseExtension.First();
            workflowRequest.EntityId = clauseExtension.ClauseExtensionId;
            workflowRequest.Parameters.Add("clause-number", clauseNumber);
            workflowRequest.Parameters.Add("current-date",
            clauseIndividual.CurrentExpirationDate.Value.ToString("dd MMM yyyy"));
            workflowRequest.Parameters.Add("idb-request",
                clauseExtension.IdbRequest.ToString());
            if (clauseExtension.RequestExtensionDate.HasValue)
            {
                workflowRequest.Parameters.Add(GeneralClauseConstants.EXTENSION_DATE_VARIABLE,
                clauseExtension.RequestExtensionDate.Value.ToString("dd MMM yyyy"));
            }
            else
            {
                workflowRequest.Parameters.Add(GeneralClauseConstants.EXTENSION_DATE_VARIABLE,
                    Convert.ToDateTime(clauseIndividual
                    .CurrentExpirationDate).AddMonths((int)(clauseExtension
                    .IdbRequest == null ? 0 : clauseExtension.IdbRequest))
                    .ToString("dd MMM yyyy"));
            }

            workflowRequest.Parameters.Add("clause-description",
                clause.Description);
            workflowRequest.Parameters.Add(GeneralClauseConstants.CLAUSE_INDIVIDUAL_DATE,
                clauseIndividual.OriginalExpirationDate.ToString("dd MMM yyyy"));
        }

        private void SetParametersForClause(
            WorkflowCreationRequest workflowRequest,
            ClauseModel clause,
            string clauseNumber)
        {
            string finalStatusDesc = string.Empty;
            var clauseIndividual = clause.ClauseIndividuals.First();

            if (clauseIndividual == null)
            {
                return;
            }

            if (clauseIndividual.FinalStatusId.HasValue)
            {
                var masterData = _catalogService.GetConvergenceMasterDataById(
                    clauseIndividual.FinalStatusId.Value);
                finalStatusDesc = masterData.IsValid ? masterData.Model.NameEn : string.Empty;
            }

            workflowRequest.EntityId = clauseIndividual.ClauseIndividualId;
            workflowRequest.Parameters.Add("clause-number", clauseNumber);
            workflowRequest.Parameters.Add("clause-description", clause.Description);
            workflowRequest.Parameters.Add("final-status", finalStatusDesc);
            workflowRequest.Parameters.Add("request", string.Empty);
        }

        private void UpdateExtensionComments(OperationRelatedModel model)
        {
            var modelClauseIndividualExtension = ClauseRepository.GetExtensionById(model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId);
            modelClauseIndividualExtension.UserComments.Clear();

            foreach (var Comments in model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments.Where(x => x.UserCommentId == -1))
            {
                Comments.ModifiedBy = IDBContext.Current.UserName;
            }

            modelClauseIndividualExtension.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseExtension[0].UserComments);
            ClauseRepository.SaveOrUpdateExtensionModel(modelClauseIndividualExtension, IDBContext.Current.UserName);

            model.Contracts[0].UpdateLMSStatus = 0;
        }

        private void UpdateFulFillComments(OperationRelatedModel model)
        {
            var modelClauseIndividual = ClauseRepository.GetClauseIndividualsById(model.Contracts[0].Clauses[0].ClauseIndividuals[0].ClauseIndividualId);
            modelClauseIndividual.UserComments.Clear();

            foreach (var Comments in model.Contracts[0].Clauses[0]
                .ClauseIndividuals[0].UserComments.Where(x => x.UserCommentId == -1))
            {
                Comments.ModifiedBy = IDBContext.Current.UserName;
            }

            modelClauseIndividual.UserComments.AddRange(model.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);
            ClauseRepository.SaveOrUpdateClauseIndividual(modelClauseIndividual, IDBContext.Current.UserName);

            model.Contracts[0].UpdateLMSStatus = 0;
        }
    }
}