using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;

using Newtonsoft.Json;

using IDB.Architecture;
using IDB.Architecture.Cache;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.Components;
using IDB.MW.Application.Core.Mappers;
using IDB.MW.Application.Core.Messages;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Core.ViewModels.FormRendering;
using IDB.MW.Application.CPDModule.Services.ProductProfile;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;
using IDB.MW.Application.OPUSModule.Enums;
using IDB.MW.Application.OPUSModule.Mappers.OperationDataService;
using IDB.MW.Application.OPUSModule.Messages.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Messages.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Messages.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Services.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Services.CheckFundAvailabilityService;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.CrossCreationOperationService;
using IDB.MW.Application.OPUSModule.Services.DeliverableService;
using IDB.MW.Application.OPUSModule.Services.DocumentService;
using IDB.MW.Application.OPUSModule.Services.EnvironmentalSocialDataService;
using IDB.MW.Application.OPUSModule.Services.ExchangeRateService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.OPUSModule.Services.OperationDataBusinessRulesService;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Application.OPUSModule.ViewModels.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.ViewModels.CheckFundAvailabilityService;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.MW.Application.OPUSModule.ViewModels.CreationFormService;
using IDB.MW.Application.OPUSModule.ViewModels.DeliverableService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.ViewModels.Institutions;
using IDB.MW.Application.OPUSModule.ViewModels.OperationDataService;
using IDB.MW.Application.OverviewModule.Enums;
using IDB.MW.Application.Reformulation.Services;
using IDB.MW.Application.Reformulation.ViewModels;
using IDB.MW.Application.TCAbstractModule.Services.TCAbstractService;
using IDB.MW.Business.Core.OPUSService.DTOs;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.OPUSModule.Contracts;
using IDB.MW.Business.OPUSModule.Services;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.CPD;
using IDB.MW.Domain.Values.Dem;
using IDB.MW.DomainModel.Contracts.Repositories.CaseManager;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.OPUSModule;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Clauses;
using IDB.MW.Infrastructure.Data.Optima.Utilities;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.Global.Controllers;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Business.OPUSModule.Business;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.OPUS.Controllers
{
    public partial class ViewController : BaseController
    {
        private const string RelationshipTypeItself = "SELFCONTAINER";
        private const string RelationshipRoleItself = "INDIVIDUAL";
        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_ENVIRONMENTAL_SOCIAL_DATA = "/OPUS/EnvironmentalSocialData";
        private const string UrlApprovalOperation = "/OPUS/ApprovalOperation";
        private const string MODALITY = "MODALITY";
        private const int TO_PERCENT = 100;
        private const string TRUE = "Y";
        private const int DEFAULT_VALUE = 0;
        private const string OPER_TYPE = "param0";
        private const string LENDING = "param1";
        private const string ORG_UNIT = "param2";
        private const string ORG_UNIT_OD = "param0";
        private const string NSG_CATEGORIZATION = "param3";
        private const string NSG_CATEGORIZATION_FOR_CON = "param4";
        private const string PMI_OPERATION = "PMI-OPERATION";
        private const string FDE_RESPONSE = "FDE-RESPONSE";
        private const string ORG_RESPONSIBLE = "ORG_RESPONSIBLE";
        private const string ORGANIZATIONAL_UNIT = "OrganizationalUnit";
        private const string COUNTRY = "country";
        private const string SUPPORTTYPE = "SupportType";
        private const string LIST_COUNTRY = "listCountry";
        private const string OPERATION_TYPE = "operationType";

        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationManager _authorizationManager;
        private readonly IApprovalOperationService _approvalOperationService;
        private readonly IEnvironmentalSocialDataService _environmentalSocialDataService;
        private readonly ICreationFormService _creationFormService;
        private readonly IInstitutionService _institutionService;
        private readonly IOperationDataService _operationDataService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly ICatalogService _catalogService;
        private readonly IOperationTeamDataService _operationTeamDataService;
        private readonly IDivisionOpusRepository _divisionOpusRepository;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IVerifyCountryService _verifyCountryService;
        private readonly ICalculationOperationYearService _calculationOperationYearService;
        private readonly ICacheManagement _cacheData;
        private readonly ICheckOperationAvailabilityService _checkOperationAvailabilityService;
        private readonly ICheckIsRegionalPermissionService _checkIsRegionalPermissionService;
        private readonly ICrossCreationOperationService _crossCreationOperationService;
        private readonly ICheckOperationTypeService _checkOperationTypeService;
        private readonly IRelationshipService _relationshipService;
        private readonly ICheckFundAvailabilityService _checkFundAvailabilityService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IK2DataService _k2DataService;
        private readonly IFinancialDataBussinesService _getFinancingTypeListService;
        private readonly IDeliverableService _deliverableService;
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly ICheckRequiredService _checkRequiredService;
        private readonly ICheckVisibilityService _checkVisibilityService;
        private readonly ICheckAnnualAllocationYear _checkAnnualAllocationYear;
        private readonly IOpusCalculationService _opusCalculationService;
        private readonly IOperationRepository _operationRepository;
        private readonly IOperationSubnationalLevelRepository _operationSubnationalLevelRepository;
        private readonly ISecurityModelRepository _securityModelRepository;
        private readonly IDocumentManagementService _docManagementService;
        private readonly IOperationTeamDataApprovedRepository _operationTeamDataApprovedRepository;
        private readonly IOrganizationalUnitRelatedApprovedRepository _organizationalUnitRelatedApprovedRepository;
        private readonly IInstitutionRelatedApprovedRepository _institutionRelatedApprovedRepository;
        private readonly ICountryRelatedApprovedRepository _countryRelatedApprovedRepository;
        private readonly IReformulationService _reformulationService;
        private readonly IOperationHistoryService _operationHistoryService;
        private readonly IFundOperationRepository _fundOperationRepository;
        private readonly IApprovalIncreasesRevampService _approvalIncreasesRevampService;
        private readonly IDEMLockModulesService _demLockModulesService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;
        private readonly IProductProfileService _productProfileService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly AdministrationSecondPhase.Models.Institution.ViewModelMapperHelper _viewInstitutionModelMapperHelper;
        private readonly IOperationDataBusinessRulesService _operationDataBusinessRulesService;
        private readonly IAdUsersRepository _adUsersRepository;
        private readonly ITCAbstractService _tcAbstractService;
        private readonly IComponentCNDService _componentCNDService;
        private ICacheStorageService _cacheStorageService;

        public virtual IPMIDetailsModelRepository PMIModelRepository { get; set; }

        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheVeryLongTime"]
                                         .ToString());

        #region Contructors
        public ViewController(
            IApprovalOperationService approvalOperationService,
            ICreationFormService creationFormService,
            IInstitutionService institutionService,
            IOperationDataService operationDataService,
            IFinancialDataExecutionService financialDataExecutionService,
            IEnvironmentalSocialDataService environmentalSocialDataService,
            IFinancialDataPreparationService financialDataPreparationService,
            ICatalogService catalogService,
            IDivisionOpusRepository divisionOpusRepository,
            IEnumMappingService enumMappingService,
            IVerifyCountryService verifyCountryService,
            ICalculationOperationYearService calculationOperationYearService,
            ICalculationEffortDaysService calculationEffortDaysService,
            IDocumentService documentService,
            ICheckOperationAvailabilityService checkOperationAvailabilityService,
            ICheckIsRegionalPermissionService checkIsRegionalPermissionService,
            ICrossCreationOperationService crossCreationOperationService,
            ICheckOperationTypeService checkOperationTypeService,
            ICheckRequiredService checkRequieredService,
            IRelationshipService relationshipService,
            IOperationTeamDataService operationTeamDataService,
            ICheckFundAvailabilityService checkFundAvailabilityService,
            IExchangeRateService exchangeRateService,
            IK2DataService k2DataService,
            IFinancialDataBussinesService getFinancingTypeListService,
            IDeliverableService deliverableService,
            IOperationRepository operationRepository,
            IOperationSubnationalLevelRepository operationSubnationalLevelRepository,
            ICountryRelatedApprovedRepository countryRelatedApprovedRepository,
            IOperationTeamDataApprovedRepository operationTeamDataApprovedRepository,
            IOrganizationalUnitRelatedApprovedRepository organizationalUnitRelatedApprovedRepository,
            IInstitutionRelatedApprovedRepository institutionRelatedApprovedRepository,
            IConvergenceMasterDataRepository convergenceMasterDataRepository,
            IAttributesEngineService attributesEngineService,
            ICheckVisibilityService checkVisibilityService,
            ICheckAnnualAllocationYear checkAnnualAllocationYear,
            IOpusCalculationService opusCalculationService,
            IAuthorizationManager authorizationManager,
            IDocumentManagementService docManagementService,
            ISecurityModelRepository securityModelRepository,
            IReformulationService reformulationService,
            IOperationHistoryService operationHistoryService,
            IFundOperationRepository fundOperationRepository,
            IDEMLockModulesService demLockModulesService,
            ICacheManagement cacheData,
            IApprovalIncreasesRevampService approvalIncreasesRevampService,
            IProductProfileService productProfileService,
            IOperationDataBusinessRulesService operationDataBusinessRulesService,
            IAdUsersRepository adUsersRepository,
            ITCAbstractService tcAbstractService,
            IComponentCNDService componentCNDService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _authorizationManager = authorizationManager;
            _approvalOperationService = approvalOperationService;
            _creationFormService = creationFormService;
            _institutionService = institutionService;
            _operationDataService = operationDataService;
            _financialDataExecutionService = financialDataExecutionService;
            _financialDataPreparationService = financialDataPreparationService;
            _environmentalSocialDataService = environmentalSocialDataService;
            _catalogService = catalogService;
            _cacheData = cacheData;
            _divisionOpusRepository = divisionOpusRepository;
            _cacheStorageService = CacheStorageFactory.Current;
            _calculationOperationYearService = calculationOperationYearService;
            _checkOperationAvailabilityService = checkOperationAvailabilityService;
            _enumMappingService = enumMappingService;
            _checkIsRegionalPermissionService = checkIsRegionalPermissionService;
            _crossCreationOperationService = crossCreationOperationService;
            _verifyCountryService = verifyCountryService;
            _checkOperationTypeService = checkOperationTypeService;
            _getFinancingTypeListService = getFinancingTypeListService;
            _checkRequiredService = checkRequieredService;
            _relationshipService = relationshipService;
            _operationTeamDataService = operationTeamDataService;
            _checkFundAvailabilityService = checkFundAvailabilityService;
            _exchangeRateService = exchangeRateService;
            _k2DataService = k2DataService;
            _deliverableService = deliverableService;
            _convergenceMasterDataRepository = convergenceMasterDataRepository;
            _attributesEngineService = attributesEngineService;
            _checkVisibilityService = checkVisibilityService;
            _checkAnnualAllocationYear = checkAnnualAllocationYear;
            _opusCalculationService = opusCalculationService;
            _operationRepository = operationRepository;
            _operationSubnationalLevelRepository = operationSubnationalLevelRepository;
            _securityModelRepository = securityModelRepository;
            _countryRelatedApprovedRepository = countryRelatedApprovedRepository;
            _institutionRelatedApprovedRepository = institutionRelatedApprovedRepository;
            _operationTeamDataApprovedRepository = operationTeamDataApprovedRepository;
            _demLockModulesService = demLockModulesService;
            _organizationalUnitRelatedApprovedRepository = organizationalUnitRelatedApprovedRepository;
            _securityModelRepository = securityModelRepository;
            _docManagementService = docManagementService;
            _reformulationService = reformulationService;
            _approvalIncreasesRevampService = approvalIncreasesRevampService;
            _operationHistoryService = operationHistoryService;
            _fundOperationRepository = fundOperationRepository;
            _productProfileService = productProfileService;
            _operationDataBusinessRulesService = operationDataBusinessRulesService;

            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag,
                _authorizationService,
                _financialDataPreparationService,
                _creationFormService,
                _operationDataService,
                _catalogService,
                _calculationOperationYearService,
                calculationEffortDaysService,
                documentService,
                _verifyCountryService,
                _financialDataExecutionService,
                _getFinancingTypeListService,
                _deliverableService,
                _approvalOperationService,
                _convergenceMasterDataRepository,
                _crossCreationOperationService,
                _attributesEngineService,
                _relationshipService,
                _approvalIncreasesRevampService);

            _viewInstitutionModelMapperHelper =
                new AdministrationSecondPhase.Models.Institution.ViewModelMapperHelper(
                    ViewBag, _institutionService, _catalogService);
            _adUsersRepository = adUsersRepository;
            _tcAbstractService = tcAbstractService;
            _componentCNDService = componentCNDService;
        }

        #endregion

        #region SubnationalLevel

        public virtual ActionResult GetSubnationalLevel(string operationNumber)
        {
            var response = _viewModelMapperHelper.GetCreationForm(operationNumber);
            SetPermisionSubnationalLevel();
            ViewBag.IsDraft = response.CreationForm.IsDraft;
            ViewBag.HasPermissionNotDraft = false;
            if (IDBContext.Current.HasPermission(Permission.VPC_WRITE_PERMISSION) == true || IDBContext.Current.HasPermission(Permission.VPS_MANAGER_WRITE) == true || IDBContext.Current.HasPermission(Permission.GCM_WRITE) == true)
            {
                ViewBag.HasPermissionNotDraft = true;
            }

            if (operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER && operationNumber != null)
            {
                ViewBag.Creation = response.CreationForm.IsCreation;
            }
            else
            {
                ViewBag.Creation = true;
            }

            if (operationNumber == OPUSGlobalValues.NEW_OPERATION_NUMBER || operationNumber == null)
            {
                return PartialView("Partials/CreationForm/_SubnationalLevel");
            }

            var operId = _operationRepository.GetOne(o => o.OperationNumber == operationNumber).OperationId;
            var subnationalLevelOperation =
                _operationSubnationalLevelRepository.GetByCriteria(o => o.OperationId == operId);

            var model = new List<SubnationalLevelViewModel>();

            if (subnationalLevelOperation.Any())
            {
                foreach (var fundSubnationalLevel in subnationalLevelOperation)
                {
                    model.Add(new SubnationalLevelViewModel()
                    {
                        CountryRegion = fundSubnationalLevel.CountryRegion,
                        AdminDistrict = fundSubnationalLevel.Admindistrict,
                        Locality = fundSubnationalLevel.Locality
                    });
                }
            }

            return PartialView("Partials/CreationForm/_SubnationalLevel", model);
        }

        public virtual ActionResult GetRowSubnationalLevel(string countryRegion, string adminDistrict, string locality, string operationNumber)
        {
            ViewBag.CountryRegion = countryRegion;
            ViewBag.AdminDistrict = adminDistrict;
            ViewBag.Locality = locality;

            return PartialView("Partials/CreationForm/DataTables/RowSubnationalLevel");
        }

        public virtual bool SaveRowSubnationalLevel(List<SubnationalLevelViewModel> newResultSubnational, List<SubnationalLevelViewModel> deleteSubnationalItems, string operationNumber)
        {
            var isSaved = _crossCreationOperationService.SaveSubnationalLevel(operationNumber, newResultSubnational, deleteSubnationalItems);

            return isSaved.IsValid;
        }

        public virtual bool DeleteSubnationalLevel(string operationNumber)
        {
            if (operationNumber == null || operationNumber == OPUSGlobalValues.NEW_OPERATION_NUMBER)
            {
                return false;
            }

            var operationId = _operationRepository
                .GetOne(o => o.OperationNumber == operationNumber).OperationId;
            var existSubnational =
                _operationSubnationalLevelRepository.GetByCriteria(o => o.OperationId == operationId);

            var model = new List<SubnationalLevelViewModel>();
            if (existSubnational != null && existSubnational.Any())
            {
                foreach (var subnationalLevel in existSubnational)
                {
                    model.Add(new SubnationalLevelViewModel()
                    {
                        OperationId = operationId,
                        CountryRegion = subnationalLevel.CountryRegion,
                        AdminDistrict = subnationalLevel.Admindistrict,
                        Locality = subnationalLevel.Locality
                    });
                }
            }

            var isDelete = _crossCreationOperationService.DeleteSubnationalLevel(operationNumber, model);
            return isDelete.IsValid;
        }

        #endregion

        #region ApprovalOperation
        public virtual ActionResult ApprovalOperation(string operationNumber)
        {
            var reformulations = _reformulationService.GetFormulations(operationNumber, false);
            ICollection<ApprovalOperationViewModel> modelFinal =
                new Collection<ApprovalOperationViewModel>();

            foreach (var item in reformulations.Models)
            {
                ApprovalOperationViewModel approvalModel = BuildApprovalOperationViewModel(
                    operationNumber, item);
                modelFinal.Add(approvalModel);
            }

            if (!reformulations.Models.HasAny())
            {
                var reformulation = new ReformulationViewModel { IsCurrent = true };
                ApprovalOperationViewModel approvalModel = BuildApprovalOperationViewModel(
                    operationNumber, reformulation);

                modelFinal.Add(approvalModel);
            }

            var modelCurrent = modelFinal.Single(x => x.IsCurrent);
            ViewBag.operationNumber = operationNumber;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelCurrent);

            return View(modelFinal);
        }

        public virtual ActionResult ApprovalOperationPartial(string operationNumber)
        {
            var reformulations = _reformulationService.GetFormulations(operationNumber, false);
            if (!reformulations.Models.HasAny())
            {
                reformulations.Models.Add(new ReformulationViewModel { IsCurrent = true });
            }

            var reformulationCurrent = reformulations.Models.Single(x => x.IsCurrent);
            var model = GetApprovalOperation(operationNumber, reformulationCurrent);

            var attributes = _viewModelMapperHelper.GetAttributesApproval(operationNumber, reformulationCurrent);
            model.AttributesApproval = attributes.AttributesApproval;
            model.FormAttributes = attributes.FormAttributes;

            ViewBag.operationNumber = operationNumber;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/ApprovalOperation/ApprovalOperationPartial", model);
        }

        public virtual ActionResult GetMailTemplate(string operationNumber)
        {
            var response = _approvalOperationService.GetMailData(operationNumber);
            return Json(response);
        }

        public virtual FileResult DownloadSGOperationProfileSummary(
            string operationNumber,
            string formatType,
            int? activityPlanId,
            bool isCurrent)
        {
            var reformulation = new ReformulationViewModel
            {
                IsCurrent = isCurrent,
                ActivityPlanId = activityPlanId
            };

            var response = _approvalOperationService.DownloadOperationSummaryReport(
                operationNumber, formatType, reformulation);

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";
            application = (formatType == "pdf") ? (application + formatType) : (application + "vnd.ms-excel");

            return File(response.File, application);
        }

        public virtual ActionResult AddNewDocument(string operationNumber,
            string documentNumber,
            string documentName,
            bool isEditableName)
        {
            var documentsResponse = _viewModelMapperHelper.NewDocumentApproval(
                operationNumber,
                documentNumber,
                documentName,
                isEditableName);

            return PartialView(
               "Partials/ApprovalOperation/ApprovalDocumentRow",
               documentsResponse.Documents);
        }

        public virtual JsonResult GetIncreaseSequences(
            bool hasDynamicApprovalNumber, string approvalNumber)
        {
            var response = _approvalOperationService.GetIncreaseSequences(
                hasDynamicApprovalNumber, approvalNumber);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage,
                    IncreasesSequenceNumbers = response.Models,
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public virtual JsonResult GetApprovalDate(int increaseId)
        {
            var response = _approvalOperationService.GetApprovalDateIncrease(increaseId);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage,
                    ApprovalDate = response.Data,
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public virtual JsonResult GenerateApprovalNumber(string operationNumber,
            int financingId,
            string suffix,
            string approvalNumberFromSearchTool,
            bool isIncrease)
        {
            var response = _approvalOperationService
                .GenerateApprovalNumber(new GenerateApprovalNumberRequest
                {
                    OperationNumber = operationNumber,
                    FinancingId = financingId,
                    Suffix = _viewModelMapperHelper.ValidateSuffix(suffix),
                    ApprovalNumberFromSearchTool = approvalNumberFromSearchTool,
                    IsIncrease = isIncrease
                });

            if (!response.IsValid)
            {
                var InvalidResult = Json(new
                {
                    isValid = response.IsValid,
                    errorMessage = response.ErrorMessage
                }, JsonRequestBehavior.AllowGet);

                return InvalidResult;
            }

            var ValidResult = Json(new
            {
                financingId = financingId,
                approvalNumber = response.GenerateApprovalNumberViewModel.ApprovalNumber,
                resolutionNumber = response.GenerateApprovalNumberViewModel.ResolutionNumber,
                documentNumber = response.GenerateApprovalNumberViewModel.DocumentNumber,
                documentDate = response.GenerateApprovalNumberViewModel.DocumentDate,
                numberAndDescription = response
                    .GenerateApprovalNumberViewModel
                    .NumberAndDescription,
                isValid = response.IsValid
            },
            JsonRequestBehavior.AllowGet);

            return ValidResult;
        }

        public virtual JsonResult RevokeApproval(int financingId, string actualStatus, string operationNumber)
        {
            var result = Json(new
            {
            }, JsonRequestBehavior.AllowGet);

            var model = _approvalOperationService.RevokeApproval(financingId, actualStatus).RevokeViewModel;
            result.Data = new
            {
                financingId = model.FinancigId,
                status = model.Status,
                isValid = true
            };

            return result;
        }

        public virtual JsonResult RegisterApproval(string operationNumber)
        {
            var response = _approvalOperationService.RegisterApproval(operationNumber);

            return Json(new
            {
                data = new
                {
                    isValid = response.IsValid,
                    isValidSAP = response.IsValidSap,
                    errorMessage = response.ErrorMessage,
                },
            });
        }

        public virtual JsonResult DownloadApprovalRegistryReport(
            string operationNumber,
            string formatType,
            int? activityPlanId,
            bool isCurrent)
        {
            var reformulation = new ReformulationViewModel
            {
                IsCurrent = isCurrent,
                ActivityPlanId = activityPlanId
            };

            var response = _approvalOperationService.DownloadApprovalRegistryReport(
                operationNumber, "PDF", reformulation);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CheckFundAvailability(string operationNumber)
        {
            var response = _approvalOperationService.CheckFundAvailability(operationNumber);

            return Json(
                new
                {
                    data = new
                    {
                        isValid = response.IsValid,
                        message = response.Message,
                        errorMessage = response.ErrorMessage
                    }
                },
                               JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult RegisterApprovalPreValidate(string operationNumber)
        {
            var response = _approvalOperationService
                .PreRegisterApprovalValidation(operationNumber);

            return Json(new
            {
                data = new { isValid = response.IsValid, errorMessage = response.ErrorMessage },
            });
        }

        public virtual JsonResult GetApprovalNumber(string filter)
        {
            var response = _viewModelMapperHelper.GetApprovalNumberList(filter);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult AccessToApprovalResource()
        {
            var response = new ResponseBase();
            string userName = IDBContext.Current.UserName;
            string operationNumber = IDBContext.Current.Operation;

            var syncErrorMessage = SynchronizationHelper.AccessToResources("edit", UrlApprovalOperation, operationNumber, userName);
            if (!string.IsNullOrWhiteSpace(syncErrorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = syncErrorMessage;
            }
            else
            {
                response.IsValid = true;
            }

            return Json(response);
        }

        public virtual JsonResult FreeApprovalResource()
        {
            var response = new ResponseBase();
            response.IsValid = true;
            string userName = IDBContext.Current.UserName;
            string operationNumber = IDBContext.Current.Operation;
            try
            {
                SynchronizationHelper.TryReleaseLock(UrlApprovalOperation, operationNumber, userName);
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }

        public virtual JsonResult GetApprovalProcessValueAutomaticallyCalculated(
            string operationNumber)
        {
            var response = _attributesEngineService.GetApprovalAutomaticValue(operationNumber);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual PartialViewResult NewTransactionModal(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetRequestIncreaseData(true, operationNumber);

            return PartialView(
                "Partials/ApprovalOperation/NewTransactionModal",
                model.RequestIncreaseData);
        }

        public virtual ActionResult GetDocumentApproval(string operationNumber)
        {
            var requestDocument = new ApprovalOperationDocumentRequest
            {
                OperationNumber = operationNumber
            };

            var documentResult = _approvalOperationService.GetApprovalOperationDocument(
                requestDocument);

            if (documentResult.IsValid)
            {
                documentResult.Documents.ApprovalTLWrite =
                IDBContext.Current.HasPermission("Approval TL write");
                documentResult.Documents.ApprovalWrite =
                    IDBContext.Current.HasPermission("Approval Write");

                return PartialView(
                    "Partials/ApprovalOperation/ApprovalDocuments", documentResult.Documents);
            }
            else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        IsValid = documentResult.IsValid,
                        ErrorMessage = documentResult.ErrorMessage
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public virtual ActionResult GetRemarksApproval(string operationNumber)
        {
            var requestRemark = new ApprovalOperationRemarkRequest
            {
                OperationNumber = operationNumber
            };

            var responseRemark = _approvalOperationService.GetApprovalRemark(requestRemark);
            if (responseRemark.IsValid)
            {
                return PartialView(
                    "Partials/ApprovalOperation/ApprovalOperationComments",
                    responseRemark.Comments);
            }
            else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        IsValid = responseRemark.IsValid,
                        ErrorMessage = responseRemark.ErrorMessage
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public virtual ActionResult GetApprovalFundingLine(int fundOperationIncreaseId,
            int fundOperationId,
            int rowNumber)
        {
            var response = _viewModelMapperHelper
                .GetApprovalFundingLine(fundOperationIncreaseId, fundOperationId, rowNumber);

            if (response.IsValid)
            {
                return PartialView(
                    "Partials/ApprovalOperation/ApprovalFundingLinePartial",
                    response.FundingLine);
            }
            else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        IsValid = response.IsValid,
                        ErrorMessage = response.ErrorMessage
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public virtual ActionResult ButtonsIncreaseApproval(string operationNumber,
            int fundOperationIncreaseId)
        {
            var masterDataResponse = _viewModelMapperHelper
                .GetMasterDataNameByCode(
                    OPUSGlobalValues.REVOKED_STATUS_CODE,
                    MasterType.VALIDATION_STAGE,
                    Localization.CurrentLanguage);

            if (!masterDataResponse.IsValid)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        ErrorMessage = masterDataResponse.ErrorMessage
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var model = _viewModelMapperHelper.GetIncreaseActionButtons(operationNumber,
                fundOperationIncreaseId);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = true,
                    HtmlButtons = this.RenderRazorViewToString(
                        "Partials/ApprovalOperation/IncreaseRowActionButtons", model),
                    newStatus = masterDataResponse.Name
                }
            };
        }

        public virtual JsonResult OperationAutoApproval(
            string operationNumber,
            int? activityPanId,
            bool isCurrent)
        {
            var response = _approvalIncreasesRevampService.AutomatedApproval(
                new AutomatedApprovalRequest
                {
                    OperationNumber = operationNumber,
                    IsIncrease = false,
                    FundOperationIncreaseId = 0,
                    Reformulation = new ReformulationViewModel
                    {
                        IsCurrent = isCurrent,
                        ActivityPlanId = activityPanId
                    }
                });

            return new JsonResult
            {
                Data = new
                {
                    isValid = response.IsValid,
                    isValidSAP = response.IsValidSap,
                    isValidRegistry = response.IsValidRegistry,
                    errorMessage = response.ErrorMessage
                },
            };
        }

        #endregion

        #region Deliverable Module
        public virtual ActionResult Deliverable(string operationNumber)
        {
            var model = GetDeliverableViewModel(operationNumber);

            return View(model);
        }

        public virtual ActionResult DeliverableContent(string operationNumber)
        {
            var model = GetDeliverableViewModel(operationNumber);

            return PartialView("Partials/Deliverable/DeliverablePartial", model);
        }

        public virtual ActionResult GetRowDeliverable(
            string operationNumber,
            string countryCode,
            int countryId,
            bool isRegional,
            int responsible,
            int year)
        {
            GetListsDeliverable(countryCode, countryId, isRegional, year);
            ViewBag.OperationResponsible = responsible;
            ViewBag.OperationCountry = countryId;

            return PartialView("Partials/Deliverable/DataTables/DataRow/DeliverableRow");
        }

        public virtual JsonResult GetListResponsibleDeliverable(string filter)
        {
            var response = _viewModelMapperHelper.GetDivisionsFilter(filter, true);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage,
                    ListResponse = response.Divisions
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual FileResult DownloadDeliverable(string operationNumber, string formatType)
        {
            var response = _deliverableService.GetDeliverableReport(operationNumber, formatType);

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";
            application = (formatType == "pdf") ? (application + formatType) : (application + "vnd.ms-excel");

            return File(response.File, application);
        }

        #endregion

        #region CreationForm

        public virtual ActionResult CreateCreationForm(string operationNumber)
        {
            var countryBK = _convergenceMasterDataRepository.GetOne(
                o => o.ConvergenceMasterType.Type == MasterType.COUNTRY &&
                o.Code == CountryCode.BK);

            ViewBag.countryBkEn = countryBK.NameEn;
            ViewBag.countryBkEs = countryBK.NameEs;
            ViewBag.countryBkFr = countryBK.NameFr;
            ViewBag.countryBkPt = countryBK.NamePt;

            if (!string.IsNullOrEmpty(operationNumber))
            {
                ViewBag.SGId = _catalogService.GetConvergenceMasterDataIdByCode(
                    AttributeValue.SG, MasterType.LENDING_TYPE).Id;
                ViewBag.CreationOperation = false;
                return RedirectToAction("CreationForm");
            }

            ViewBag.CreationOperation = true;
            ViewBag.Creation = true;
            ViewBag.OtherSectorId = _catalogService.GetConvergenceMasterDataIdByCode(
                OPUSGlobalValues.OT, MasterType.SECTOR).Id;
            ViewBag.SgId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.SG, MasterType.LENDING_TYPE).Id;
            ViewBag.ActionPlanCndId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.ACTION_PLAN_CND, AttributeCode.CTY_PROGRAM_CLASSIFICATION).Id;

            return View("CreationForm", DataCreationForm(OPUSGlobalValues.NEW_OPERATION_NUMBER));
        }

        public virtual ActionResult CreateCreationFormContent()
        {
            ViewBag.CreationOperation = true;
            return PartialView("Partials/CreationForm/CreationFormPartial", DataCreationForm(OPUSGlobalValues.NEW_OPERATION_NUMBER));
        }

        public virtual ActionResult CreationForm(string operationNumber)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);

            if (string.IsNullOrEmpty(operationNumber))
                return RedirectToAction("CreateCreationForm");

            ViewBag.CreationOperation = false;
            ViewBag.fixLoad = true;

            ViewBag.OtherSectorId = _catalogService.GetConvergenceMasterDataIdByCode(
                OPUSGlobalValues.OT, MasterType.SECTOR).Id;
            ViewBag.SgId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.SG, MasterType.LENDING_TYPE).Id;
            ViewBag.ActionPlanCndId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.ACTION_PLAN_CND, AttributeCode.CTY_PROGRAM_CLASSIFICATION).Id;

            return View(DataCreationForm(operationNumber));
        }

        public virtual ActionResult CreationFormContent(string operationNumber)
        {
            ViewBag.CreationOperation = false;
            return PartialView("Partials/CreationForm/CreationFormPartial", DataCreationForm(operationNumber));
        }

        public virtual CreationFormViewModel DataCreationForm(string operationNumber)
        {
            var operation = _operationRepository.GetOne(
                o => o.OperationNumber == operationNumber,
                o => o.OperationData.OperationType,
                o => o.OperationData.OperationStage,
                o => o.OperationData.Country,
                o => o.OperationData.SectorSubsector);

            string[] masterTypeList = new string[]
            {
                MasterType.BUDGET_CODE,
                MasterType.CATEGORY,
                MasterType.CONMODALITY,
                MasterType.COUNTRY,
                MasterType.COUNTRY_ROLE,
                MasterType.EXECUTED_BY,
                MasterType.IMPACT_CATEGORY,
                MasterType.INSTITUTION_ROLE,
                MasterType.LENDING_TYPE,
                MasterType.MBF,
                MasterType.MEMBER_ROLE,
                MasterType.OPERATION_TYPE,
                MasterType.ORGANIZATION_UNIT,
                MasterType.ORG_UNIT_ROLE,
                MasterType.RELATIONSHIP_ROLE,
                MasterType.RELATIONSHIP_TYPE,
                MasterType.SECTOR,
                MasterType.SUBSECTOR,
                MasterType.TC_TAXONOMY
            };

            var masterDataListByTypeCode = _catalogService
                .GetMasterDataListByTypeCode(typeCodes: masterTypeList);

            var response = _viewModelMapperHelper
                .GetCreationForm(operationNumber, operation, null, masterDataListByTypeCode);

            SetPermisionCreationForm(ViewBag.CreationOperation);

            var model = response.CreationForm;

            ViewBag.formAttributes = response.FormAttributes;
            ViewBag.IsDraft = response.CreationForm.IsDraft;
            ViewBag.HasPermissionNotDraft = false;

            if (IDBContext.Current.HasPermission(Permission.VPC_WRITE_PERMISSION) == true ||
                IDBContext.Current.HasPermission(Permission.VPS_MANAGER_WRITE) == true ||
                IDBContext.Current.HasPermission(Permission.GCM_WRITE) == true)
            {
                ViewBag.HasPermissionNotDraft = true;
            }

            if (operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER && operationNumber != null)
            {
                ViewBag.Creation = response.CreationForm.IsCreation;
                ViewBag.HasSubnational = _operationSubnationalLevelRepository
                    .Any(o => o.OperationId == operation.OperationId);
            }
            else
            {
                ViewBag.HasSubnational = false;
            }

            ViewBag.SubnationalLevel = response.CreationForm.SubnationalLevel;

            var excludeRelationshipType = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE,
                true,
                excludeRelationshipType,
                listRepository: masterDataListByTypeCode);

            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE,
                excludeByCode: excludeRelationshipRole,
                listRepository: masterDataListByTypeCode);

            ViewBag.OperationTypeList = _viewModelMapperHelper
                .GetListOperationType(MasterType.OPERATION_TYPE, masterDataListByTypeCode);

            ViewBag.OperationTypeTCP = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.TCP))
                .First()
                .MasterId;

            ViewBag.OperationTypeIGR = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.IGR))
                .First()
                .MasterId;

            ViewBag.CountryBK = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.COUNTRY && o.Code.Equals(CountryCode.BK))
                .First()
                .MasterId;

            var excludeValuesCountry = new List<string>
            {
                CountryCode.IDB, CountryCode.UND
            };

            var country = _verifyCountryService.GetCountry();
            ViewBag.CountryList = country.Disabled ?
                _catalogService.GetListMasterData(
                    MasterType.COUNTRY,
                    includeByCode: new List<string> { country.CountryCode },
                    listRepository: masterDataListByTypeCode) :
                _catalogService.GetListMasterData(
                    MasterType.COUNTRY,
                    excludeByCode: excludeValuesCountry);

            ViewBag.CountryDisabled = country.Disabled;
            ViewBag.CategoryList = _catalogService
                .GetListMasterData(MasterType.CATEGORY, listRepository: masterDataListByTypeCode);
            ViewBag.ImpactCategoryList = _catalogService.GetListMasterData(
                MasterType.IMPACT_CATEGORY,
                listRepository: masterDataListByTypeCode);
            ViewBag.OperationYearList = _viewModelMapperHelper.GetListOperationYear();
            ViewBag.BudgetCodeList = _catalogService
                .GetListMasterData(MasterType.BUDGET_CODE, listRepository: masterDataListByTypeCode);
            ViewBag.ViewApprovalDate = model.BasicData.ApprovalDate != null;

            var mbf = _crossCreationOperationService
                .GetMbfCode(model.BasicData.OperationType, operationNumber);
            model.BasicData.MbfIsDisabled = !mbf.IsEditable;

            model.BasicData.MbfCodes = mbf.MbfCodesList;

            var value = _viewModelMapperHelper
                .GetExpiredMbfItem(model.BasicData.MbfDescription ?? string.Empty);

            if (!string.IsNullOrEmpty(value))
                model.BasicData.MbfCodes.Add(new ListItemViewModel
                {
                    Text = model.BasicData.MbfDescription,
                    Value = value
                });

            ViewBag.IsRegionalPermission = _checkIsRegionalPermissionService
                .GetPermission(model.BasicData.IsRegional)
                .permission;

            ViewBag.CountryDepartmentList = _viewModelMapperHelper
                .GetListCountryDeparmet(operationNumber);

            var excludeValuesSector = new List<string> { CountryCode.UND };
            ViewBag.Sector = _catalogService.GetListMasterData(
                MasterType.SECTOR,
                excludeByCode: excludeValuesSector,
                listRepository: masterDataListByTypeCode);

            ViewBag.SubSector = _catalogService.GetListMasterData(MasterType.SUBSECTOR);

            ViewBag.SubSector = _catalogService
                .GetListMasterData(MasterType.SUBSECTOR, listRepository: masterDataListByTypeCode);

            ViewBag.SubSector = _catalogService.GetListMasterData(MasterType.SUBSECTOR);

            var excludeValues = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                MemberRoleCode.PCR_AUTOR,
                MemberRoleCode.ESG_REVIEWER,
                MemberRoleCode.CREATOR_ROLE
            };

            if (model.ExistTeamLeaderNotEditable)
            {
                excludeValues.Add(MemberRoleCode.TEAM_LEADER);
                excludeValues.Add(MemberRoleCode.ALTERNATIVE_TEAM_LEADER);
            }

            if (model.ExistOperationAnalistNotEditable)
            {
                excludeValues.Add(MemberRoleCode.OPERATIONAL_ANALIST);
            }

            var includeValuesForEsg = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
            };

            var excludeValuesForESG =
                new List<string>
                {
                    MemberRoleCode.CREATOR_ROLE,
                    MemberRoleCode.ESG_REVIEWER
                };

            var includeValuesForSpdPcr = new List<string>
            {
                MemberRoleCode.PCR_AUTOR,
            };

            var excludeValueEsgeRev = new List<string> { MemberRoleCode.ESG_REVIEWER };

            if (!(bool)ViewBag.SPDDEMResponsibilityWritePermission)
            {
                excludeValues.AddRange(new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST
                });
                excludeValuesForESG.AddRange(new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST
                });
                excludeValueEsgeRev.AddRange(new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST
                });
            }

            ViewBag.OperationTeamsRoleList = _catalogService
                .GetListMasterData(MasterType.MEMBER_ROLE, excludeByCode: excludeValues);

            ViewBag.OperationTeamsRoleEsgList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForEsg,
                excludeByCode: excludeValuesForESG,
                listRepository: masterDataListByTypeCode);

            ViewBag.OperationTeamsRoleSpdPcrList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForSpdPcr,
                excludeByCode: excludeValueEsgeRev,
                listRepository: masterDataListByTypeCode);

            var excludeValuesRoleOrg = new List<string>();

            if (_checkOperationTypeService
                    .IsOprOperation(
                        model.BasicData.OperationType,
                        model.BasicData.OperationTypeCode)
                    .Permission)
            {
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.ORG_UDR);
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.CO_RESPONSIBLE);
            }
            else if (_checkOperationTypeService
                        .IsEswOperation(
                            model.BasicData.OperationType,
                            model.BasicData.OperationTypeCode)
                        .Permission)
            {
                excludeValuesRoleOrg = new List<string> { OrgUnitRoleCode.ORG_UDR };
            }
            else if (_checkOperationTypeService
                        .IsCipOperation(
                            model.BasicData.OperationType,
                            model.BasicData.OperationTypeCode)
                        .Permission)
            {
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.ORG_UDR);
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(model.OperationNumber);
            if (!UDRBusinessLogic.MustIncludeUDRAsRole(opTypes))
            {
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.ORG_UDR);
            }

            ViewBag.RoleList = _catalogService.GetListMasterData(
                MasterType.ORG_UNIT_ROLE,
                excludeByCode: excludeValuesRoleOrg,
                listRepository: masterDataListByTypeCode);

            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE,
                listRepository: masterDataListByTypeCode);

            ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionFilteredList(
                model.ResponsibilityData.AssociatedInstitutions.Select(x => x.Institutions).ToList());

            var excludeCountries = new List<string>
            {
                CountryCode.BK,
                CountryCode.RG,
                CountryCode.IDB,
                CountryCode.UND
            };

            ViewBag.Countries = _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY,
                excludeByCode: excludeCountries);

            ViewBag.RoleCoutryAdmin = model.ResponsibilityData.CountryRelatedRoleAdminId;
            ViewBag.CoutryAdminId = model.ResponsibilityData.CountryAdminId;
            ViewBag.BorrowerId = model.BasicData.BorrowerId;
            ViewBag.ExecutorId = model.BasicData.ExecutorId;
            ViewBag.LoanId = model.BasicData.OperationTypeLoadnId;

            var divisions = _viewModelMapperHelper.GetDivisionList();

            if (operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER)
            {
                divisions = _viewModelMapperHelper.AddDivisionExpired(
                    divisions,
                    model.ResponsibilityData.OrganizationalUnit.Select(
                        o => o.OrganizationalUnit).ToList());
            }

            ViewBag.OrganizationalUnitList = divisions;

            ViewBag.AssociatedCountriesRoleList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ROLE,
                listRepository: masterDataListByTypeCode);

            ViewBag.IsRegional = _checkIsRegionalPermissionService
                .GetPermission(model.BasicData.IsRegional)
                .permission;

            ViewBag.HQOperation = true;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.financingTypeList = _viewModelMapperHelper.GetFinancingCodeTypeListViewBag(
                model.BasicData.OperationType, operationNumber, _attributesEngineService, true);

            bool isFromCreationForTCP = opTypes.Contains(OperationType.TCP);
            bool matchRuleGCMPermission = isFromCreationForTCP ?
                IDBContext.Current.HasPermission(Permission.GCM_WRITE) :
                true;

            ViewBag.fundList = _viewModelMapperHelper.GetFundListViewBag(
                gsmPermission: matchRuleGCMPermission,
                operationType: model.BasicData.OperationType,
                country: model.BasicData.CountryId.GetValueOrDefault(),
                isFromCreation: isFromCreationForTCP,
                isMifOpuOperation: _creationFormService.IsMifOpuOperation(operation));

            ViewBag.fundTDB = _viewModelMapperHelper.FundTBDViewBag(
                ViewBag.GCMWritePermission,
                model.BasicData.OperationType,
                model.BasicData.CountryId.GetValueOrDefault());
            ViewBag.IsFundTBD = model.ExpectedIDB.Any(o => o.IsTbdFund);
            ViewBag.WriteFinancing = ViewBag.CreationOperation ||
                IsEditableFinancing(model.BasicData.OperationType);

            ViewBag.CountryRelatedRoleAdmin = model.ResponsibilityData.CountryRelatedRoleAdminId;
            ViewBag.CountryRelatedRoleParticipant = model.ResponsibilityData
                .CountryRelatedRoleParticipantId;
            ViewBag.IsEsgList = false;
            ViewBag.IsEsgPermission = false;

            ViewBag.ModelSerialize = string.Empty;

            var responseVisibility = _checkVisibilityService.GetVisibility(
                operationTypeId: model.BasicData.OperationType,
                operationNumber: operationNumber,
                attributeService: _attributesEngineService);

            ViewBag.Visibility = responseVisibility;

            model.BasicData.CategoryAccess.IsVisible = responseVisibility.Category;

            ViewBag.SGId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.LENDING_TYPE &&
                    o.Code.Equals(AttributeValue.SG))
                .First()
                .MasterId;

            ViewBag.OpType = string.Empty;

            ViewBag.CONId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.CON))
                .First()
                .MasterId;

            ViewBag.CIPId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.CIP))
                .First()
                .MasterId;

            ViewBag.ESWId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.ESW))
                .First()
                .MasterId;

            if (model.ResponsibilityData.OperationTeams != null)
            {
                model.ResponsibilityData.OperationTeams
                    .RemoveAll(o => o.RoleName == Role.ESG_REVIEWER);
            }

            model.BasicData.SerializedModel = PageSerializationHelper
                .SerializeObject(model.BasicData);

            model.ResponsibilityData.OperationTeamsRoleList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    excludeByCode: GetExcludedTeamRoles(
                        model.ExistTeamLeaderNotEditable,
                        model.ExistOperationAnalistNotEditable,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission,
                        false));

            model.ResponsibilityData.OperationTeamsRoleEsgList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesEsg(false),
                    excludeByCode: GetExcludedTeamRolesEsg(
                        true,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            model.ResponsibilityData.OperationTeamsRoleSpdPcrList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesSpdPcr(),
                    excludeByCode: GetExcludedTeamRolesSpdPcr(
                        true,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            if (model.ResponsibilityData.OperationTeams.Any(otr => otr.IsRoleExpired))
                model.ResponsibilityData.OperationExpiredTeamsRoles = _catalogService
                    .GetListItemMasterData(
                        type: MasterType.MEMBER_ROLE,
                        hideExpired: false,
                        includeById: model.ResponsibilityData.OperationTeams
                            .Where(otr => otr.IsRoleExpired && otr.Role.HasValue)
                            .Select(otr => otr.Role.Value).ToList());

            model.BasicData.CategoryAccess.IsRequired = IsCategoryMandatory(
                response.CreationForm.BasicData.OperationType,
                response.FormAttributes,
                SumExpectedIDBAmounts(response.CreationForm.ExpectedIDB));

            return model;
        }

        public virtual ActionResult GetAttributtes(int operationType, string operationNumber)
        {
            var viewModel = _creationFormService.GetCreationForm(
                operationNumber ?? OPUSGlobalValues.NEW_OPERATION_NUMBER);

            var attribute = _viewModelMapperHelper.GetAttributteByTypeOperation(
                operationType, operationNumber);
            viewModel.CreationForm.Attributes = attribute.OperationAttributesData;

            _viewModelMapperHelper.LoadReferencesIds(viewModel.CreationForm);

            ViewBag.ModelSerialize = PageSerializationHelper.SerializeObject(viewModel.CreationForm);

            return PartialView("Partials/CreationForm/CreationFormAttributesPartial", attribute.FormData);
        }

        public virtual ActionResult SearchRelatedOperation(
            string operationNumber,
            int? relationshipType,
            int? relationshipRole,
            string relatedOperationFilter,
            List<OPUSCheckOperation> tableArray,
            int opType,
            ClientFieldData[] clientFieldData,
            bool isOpData,
            string deleteOpInView = "")
        {
            var xml = clientFieldData.ConvertToClienDataXML();

            int opOrganizationlUnit = 0;
            int opCountry = 0;

            opOrganizationlUnit = GetClientDataForOrganizationalUnit(
                clientFieldData,
                isOpData,
                operationNumber);

            opCountry = GetClientDataForCountry(
                clientFieldData,
                isOpData,
                operationNumber);

            var countries = isOpData ?
                GetClientDataForCountryRelated(operationNumber) :
                GetClientDataForCountryRelated(clientFieldData, opCountry);

            var supportType = GetClientDataForSupportingType(clientFieldData);

            var response = _viewModelMapperHelper.SearchRelatedOperationResponse(
                relationshipType,
                relatedOperationFilter,
                operationNumber,
                opType,
                xml,
                isOpData,
                opCountry,
                countries,
                opOrganizationlUnit,
                supportType);

            if (tableArray == null || tableArray[0] == null)
            {
                tableArray = new List<OPUSCheckOperation>();
            }

            ViewBag.RelationshipRole = _viewModelMapperHelper.GetListRelationshipRoleList(
                relationshipType.GetValueOrDefault(),
                MasterType.RELATIONSHIP_ROLE,
                RelationshipRoleItself,
                tableArray,
                relationshipRole
                .GetValueOrDefault(),
                opType,
                response.FindIsCon,
                deleteOpInView);

            return PartialView("_NewRelatedOperationResultSearch", response.Operations);
        }
        
        public virtual ActionResult GetRowRelatedOperation(string operationNumber)
        {
            var excludeRelationshipType = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE, true, excludeRelationshipType);
            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE, excludeByCode: excludeRelationshipRole);

            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);

            var model = SetNewRelatedValues(operation);
            model.SerializedModel = PageSerializationHelper.SerializeObject(model);

            var cpdMD = _convergenceMasterDataRepository.GetOne(o => o.Code == AttributeValue.CPD && o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper());
            bool isCON = OperationTypeHelper.GetOperationTypes(operation.OperationId).Contains(OperationType.CON);
            bool isCPD = operation.FormAttributeOperation.Any(x => x.ListValueID == cpdMD.ConvergenceMasterDataId);

            if (isCON && isCPD)
            {
                CacheDataToCPD(model);

                return RedirectToAction("GetRowRelatedOperation", "ProductProfile", new { Area = "CPD" });
            }

            return PartialView("_TableRelations", model);
        }

        public virtual ActionResult GetRowRelatedOperationCreation(string operationNumber)
        {
            var newRelatedOperation = JsonConvert
                .DeserializeObject<RelatedOperationRowViewModel>(Request.Form[0]);

            if (newRelatedOperation.OperationRelatedId == 0)
            {
                newRelatedOperation = GetDataModelRelationshipItself(newRelatedOperation);
            }

            newRelatedOperation.RelationshipNumber = _creationFormService
                .GetRelationshipNumber(newRelatedOperation.RelationshipTypeId)
                .RelationshipNumber;

            var excludeRelationshipType = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE, true, excludeRelationshipType);
            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE, excludeByCode: excludeRelationshipRole);

            newRelatedOperation.RowId = Convert.ToInt32(DateTime.Now.ToString("ddhhmmss"));

            return PartialView("_RowOperationRelated", newRelatedOperation);
        }

        public virtual JsonResult GetRelationshipNumber(
            string operationNumber, int relationshipTypeId)
        {
            var relationshipNumber = _relationshipService.GetRelationshipNumber(
                relationshipTypeId, 0, 0);

            return new JsonResult
            {
                Data = relationshipNumber,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual RelatedOperationRowViewModel GetDataModelRelationshipItself(
            RelatedOperationRowViewModel op)
        {
            return _crossCreationOperationService.GetDataOperation(op);
        }

        public virtual JsonResult CheckCountry(int countryId)
        {
            var response = _verifyCountryService.GetCountryRequiered(countryId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult IsSectionVisibleCreation(int operationType, int attribute)
        {
            var response = _checkVisibilityService.GetVisibility(
                operationType,
                valueAttribute: attribute,
                attributeService: _attributesEngineService);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult IsRequiredCategory(
            int operationTypeId,
            int attributeValueId,
            int? linkedAttribute)
        {
            var response = _checkRequiredService
                .CheckRequiredCategory(operationTypeId, attributeValueId, linkedAttribute);

            return Json(response);
        }

        public virtual JsonResult CheckCategoryByFundsAmount(
            int operationTypeId,
            decimal totalAmount)
        {
            var response = _checkRequiredService
                .CheckRequiredCategory(operationTypeId, totalAmount);

            return Json(response);
        }

        public virtual JsonResult GetCountries(int operationTypeId)
        {
            var response = _verifyCountryService.GetCountry(operationTypeId);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetCountryDepartmentList(string operationNumber)
        {
            return Json(_viewModelMapperHelper.GetListCountryDeparmet(operationNumber));
        }

        public virtual JsonResult GetOrgRole(int operationTypeId)
        {
            var orgRoles = GetRolesOrgUnit(operationTypeId);

            return Json(orgRoles);
        }

        public virtual JsonResult GetInstitutionRole(int operationType)
        {
            var includeValuesRoleInstitution = new List<string>();
            if (_checkOperationTypeService.IsTcOperation(operationType).Permission)
            {
                includeValuesRoleInstitution = new List<string>
                {
                    InstitutionRoleCode.INST_EX_AGENCY
                };
            }

            return Json(_catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE, includeByCode: includeValuesRoleInstitution));
        }

        public virtual ActionResult GetRowOrganizationalUnit(int operationTypeId)
        {
            ViewBag.RoleList = GetRolesOrgUnit(operationTypeId);

            ViewBag.OrganizationalUnitList = _viewModelMapperHelper.GetDivisionList();

            return PartialView("Partials/CreationForm/DataTables/RowOrganizationalUnit");
        }

        public virtual ActionResult GetRowCountryRelated(
            string operationNumber,
            bool isSelectCountryHQByDefault,
            bool isSelectCountriesWithoutHQByDefault)
        {
            var creationFormViewModel = new CreationFormViewModel();

            var idRegional = _catalogService.GetConvergenceMasterDataIdByCode(
                CountryCode.RG, MasterType.COUNTRY, true).Id;

            creationFormViewModel.AssociatedCountriesRoleList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ROLE);

            creationFormViewModel.SelectedCountries = string.Empty;

            GetConvergenceMasterDataResponse countryRoleCatalogServiceResult = null;
            GetConvergenceMasterDataResponse countriesCatalogServiceResult = null;

            int hqId = 0;

            if (isSelectCountriesWithoutHQByDefault || isSelectCountryHQByDefault)
            {
                countryRoleCatalogServiceResult = _catalogService
                    .GetConvergenceMasterData(MasterType.COUNTRY_ROLE);

                if (!countryRoleCatalogServiceResult.IsValid)
                    return Json(countryRoleCatalogServiceResult);

                countriesCatalogServiceResult = _catalogService
                    .GetConvergenceMasterData(MasterType.COUNTRY);

                if (!countriesCatalogServiceResult.IsValid)
                    return Json(countriesCatalogServiceResult);

                var hqCountryResult = _catalogService
                    .GetConvergenceMasterDataIdByCode(CountryCode.HQ, MasterType.COUNTRY);

                if (!hqCountryResult.IsValid)
                    return Json(hqCountryResult);

                hqId = hqCountryResult.Id;
            }

            if (isSelectCountryHQByDefault)
            {
                creationFormViewModel.SelectedCountries = Convert.ToString(hqId);

                var countryRoleAdmin = countryRoleCatalogServiceResult
                    .MasterDatas
                    .Where(a => a.Code == CountryRoleCode.ADMIN)
                    .Select(a => new SelectListItem
                    {
                        Text = MvcHelpers.GetItemName(a, Localization.CurrentLanguage),
                        Value = a.ConvergenceMasterDataId.ToString()
                    }).ToList();

                creationFormViewModel.SelectedAssociatedCountriesRoleList =
                    countryRoleAdmin.First().Value;
            }

            if (isSelectCountriesWithoutHQByDefault)
            {
                creationFormViewModel.SelectedCountries = string.Join(
                    ",", countriesCatalogServiceResult
                            .MasterDatas.Where(o => o.ConvergenceMasterDataId != hqId)
                            .Select(a => a.ConvergenceMasterDataId));

                var countryRoleAdmin = countryRoleCatalogServiceResult
                    .MasterDatas
                    .Where(a => a.Code == CountryRoleCode.PARTICIPANT)
                    .Select(a => new SelectListItem
                    {
                        Text = MvcHelpers.GetItemName(a, Localization.CurrentLanguage),
                        Value = a.ConvergenceMasterDataId.ToString()
                    }).ToList();

                creationFormViewModel.SelectedAssociatedCountriesRoleList =
                    countryRoleAdmin.First().Value;
            }

            var excludeCountries = new List<string>
            {
                CountryCode.BK, CountryCode.RG, CountryCode.IDB, CountryCode.UND
            };

            ViewBag.Countries = _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY, excludeByCode: excludeCountries).OrderBy(o => o.Text);

            return PartialView(
                "Partials/CreationForm/DataTables/RowCountryRelated", creationFormViewModel);
        }

        public virtual JsonResult GetCountryRelatedCountriesByRole(int roleId, bool isSGLON)
        {
            var excludeCountries = _verifyCountryService
                .FilterCountryAssociatedCountry(roleId, isSGLON)
                .FilterCountryList;

            return Json(_catalogService
                .GetListMasterData(MasterType.COUNTRY, excludeByCode: excludeCountries)
                .OrderBy(o => o.Text));
        }

        public virtual JsonResult GetEsgPermission(string orgUnit, int roleId)
        {
            var rolNameEn = _convergenceMasterDataRepository.GetOne(
                o => o.ConvergenceMasterDataId == roleId).NameEn;

            var response = _crossCreationOperationService.GetValidEsg(orgUnit, rolNameEn);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetRowInstitution(string role, string institution)
        {
            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE);
            ViewBag.AssociatedInstitutionsList = new List<SelectListItem>();
            ViewBag.RoleSelect = role;
            ViewBag.Institution = institution;
            return PartialView("Partials/CreationForm/DataTables/RowInstitution");
        }

        public virtual ActionResult GetRowInstitutionByRole(int roleSelected)
        {
            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE);
            ViewBag.RoleSelect = roleSelected;
            return PartialView("Partials/CreationForm/DataTables/RowInstitution");
        }

        public virtual ActionResult GetNewInstitution()
        {
            ViewBag.CountryList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ASSOCIATED);
            ViewBag.InstitutionList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_TYPE);
            return PartialView("_newInstitution");
        }

        public virtual JsonResult CheckAnnualAllocationYear(int conModality, int operationYear)
        {
            var response = new ResponseBase()
            {
                IsValid = false
            };

            response.IsValid = _checkAnnualAllocationYear.ifExistAA(conModality, operationYear).IsValid;
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetRowOperationTeamData(
            string operationNumber,
            bool tLorAtLnotEditable,
            bool operationAnalistNotEditable)
        {
            SetPermisionCreationForm(operationNumber == null);

            var countPermissions = 0;
            var codeRoleTeam = OPUSGlobalValues.ALL_ROLE;
            var excludeValues = new List<string>();
            var includeValues = new List<string>();

            if (ViewBag.CreationWritePermission)
            {
                excludeValues = new List<string>
                {
                    MemberRoleCode.ESG_TEAM_MEMBER,
                    MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                    MemberRoleCode.PCR_AUTOR,
                    MemberRoleCode.SPD_SPECIALIST,
                    MemberRoleCode.ESG_REVIEWER,
                    MemberRoleCode.CREATOR_ROLE
                };

                if (tLorAtLnotEditable)
                {
                    excludeValues.Add(MemberRoleCode.TEAM_LEADER);
                    excludeValues.Add(MemberRoleCode.ALTERNATIVE_TEAM_LEADER);
                }

                if (operationAnalistNotEditable)
                {
                    excludeValues.Add(MemberRoleCode.OPERATIONAL_ANALIST);
                }

                countPermissions++;
            }

            if (ViewBag.EsgSpecialistAssignmentWritePermission)
            {
                includeValues.AddRange(
                    new List<string>
                    {
                        MemberRoleCode.ESG_TEAM_MEMBER,
                        MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER
                    });
                codeRoleTeam = OPUSGlobalValues.ESG_ROLE;
                countPermissions++;
            }

            if (ViewBag.SPDPCRWritePermission)
            {
                includeValues.AddRange(new List<string>
                {
                    MemberRoleCode.PCR_AUTOR
                });
                codeRoleTeam = OPUSGlobalValues.PCR_ROLE;
                countPermissions++;
            }

            if (ViewBag.SPDDEMResponsibilityWritePermission)
            {
                includeValues.AddRange(new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST
                });
                codeRoleTeam = OPUSGlobalValues.SPD_ROLE;
                countPermissions++;
            }

            if (countPermissions > 1)
            {
                codeRoleTeam = OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE;
            }

            excludeValues.Add(MemberRoleCode.ESG_REVIEWER);

            var opTeamData = new List<OperationTeamRowViewModel>();

            if (!string.IsNullOrEmpty(operationNumber))
            {
                var operId = _operationRepository.GetOne(o => o.OperationNumber == operationNumber).OperationId;
                bool isPRG = OperationTypeHelper.GetOperationTypes(operId).Contains(OperationType.PRG);

                if (!isPRG)
                {
                    excludeValues.Add(MemberRoleCode.VPC_COUNTRY_ECONOMIST);
                    excludeValues.Add(MemberRoleCode.VPC_TEAM_MEMBER);
                }

                var responsabilityResponse =
                    _viewModelMapperHelper.GetResponsabilityData(operationNumber, true);

                if (responsabilityResponse.IsValid)
                {
                    opTeamData = responsabilityResponse.ResponsabilityData.OperationTeams;
                }
            }

            var teamList =
                _catalogService.GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: includeValues,
                    excludeByCode: excludeValues);

            ViewBag.RoleTypeCode = codeRoleTeam;

            return PartialView("Partials/OperationData/DataTables/RowOperationTeam", teamList);
        }

        public virtual JsonResult GetUsersList(string filter)
        {
            var response = _operationDataService.GetUsersByNameOrPCMail(filter);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual JsonResult CheckBusinessPermisionSave(int operationType)
        {
            var response = _checkRequiredService.RequiredFieldCreationResponse(operationType);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual JsonResult ValidateWorkflows(
            int guaranteedById, int countryId, int operationTypeId)
        {
            var response = _creationFormService.ValidateWorkflow(
                guaranteedById, countryId, operationTypeId);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual JsonResult RequestAuthorizationDraftOperation(string operationNumber)
        {
            return Json(_creationFormService.CheckFundAmountsByOperationType(operationNumber));
        }

        #endregion

        #region Operations Data
        public virtual ActionResult OperationData(string operationNumber)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);

            var operation = _operationRepository.GetOne(o => o.OperationNumber == operationNumber);

            var csOperationModalityCop = _enumMappingService
                .GetMappingCode(CSOperationModalityEnum.CountryStrategyPaper);

            string[] masterTypeList = new string[]
            {
                MasterType.CATEGORY,
                MasterType.COUNTRY,
                MasterType.INSTITUTION_ROLE,
                MasterType.MBF,
                MasterType.MEMBER_ROLE,
                MasterType.LENDING_TYPE,
                MasterType.OPERATION_TYPE,
                MasterType.ORG_UNIT_ROLE,
                MasterType.RELATIONSHIP_ROLE,
                MasterType.RELATIONSHIP_TYPE,
                MasterType.TC_TAXONOMY,
                MasterType.RELATIONSHIP_TYPE,
                MasterType.SECTOR
            };

            var masterDataListByTypeCode = _catalogService
                .GetMasterDataListByTypeCode(typeCodes: masterTypeList);

            var model = _viewModelMapperHelper.GetOperationData(
                operationNumber,
                true,
                operation,
                csOperationModalityCop,
                masterDataListByTypeCode);

            var tcAbstractEditResponse = _tcAbstractService.AreFieldsUnlocked(model.OperationId);

            model.IsEditableBasedOnTcAbstractState = tcAbstractEditResponse.IsValid ?
                tcAbstractEditResponse.HasCondition : false;

            IList<string> OperationTypesList = OperationTypeHelper
                .GetOperationTypes(operation.OperationId);

            bool isPRG = OperationTypesList.Contains(OperationType.PRG);

            if (isPRG && model.IsPaperModality)
                return RedirectToAction("Read", "ProductProfile", new { area = "CountryStrategy" });

            string[] codeList = new string[]
            {
                OperationType.CON,
                OperationType.CIP,
                OperationType.ESW
            };

            var CMDModalityIdModalityAttribute = _attributesEngineService.GetAttributeValueByCode(
                operationNumber,
                AttributeCode.CON_MODALITY,
                false,
                null,
                OperationTypesList);

            int CMDModalityId = CMDModalityIdModalityAttribute.IsValid &&
                CMDModalityIdModalityAttribute.Value != null &&
                CMDModalityIdModalityAttribute.Value.ToString() != string.Empty ?
                    Convert.ToInt32(CMDModalityIdModalityAttribute.Value.ToString()) :
                    0;

            var convergenceMasterDataList = _convergenceMasterDataRepository
                .GetByCriteria(o =>
                    (o.ConvergenceMasterType.Type == MasterType.OPERATION_TYPE &&
                        codeList.Contains(o.Code)) ||
                    o.ConvergenceMasterDataId == CMDModalityId,
                     o => o.ConvergenceMasterType)
                 .ToList();

            var CMDModality = convergenceMasterDataList
                .Where(x => x.ConvergenceMasterDataId == CMDModalityId)
                .FirstOrDefault();

            string modality = CMDModality != null &&
                CMDModality.ShortCode != null ? CMDModality.ShortCode : string.Empty;

            var operationTypeNum = _enumMappingService
                .GetMappedEnum<OperationTypeEnum>(model.OperationTypeId);

            if (operationTypeNum == OperationTypeEnum.ContainerType && modality == AttributeValue.CPD)
                return RedirectToAction(
                    AttributeCPD.ACTION_NAME,
                    AttributeCPD.CONTROLLER_NAME,
                    new { area = AttributeValue.CPD });

            SetPermissionOperationData();

            var excludeValuesRoleOrg = new List<string>();

            bool isOprOperation = _checkOperationTypeService.IsOprOperation(
                    model.BasicData.OperationType,
                    model.BasicData.OperationTypeCode)
                .Permission;

            if (isOprOperation)
            {
                excludeValuesRoleOrg = new List<string>
                {
                    OrgUnitRoleCode.ORG_UDR,
                    OrgUnitRoleCode.CO_RESPONSIBLE
                };
            }
            else if (_checkOperationTypeService
                .IsEswOperation(model.BasicData.OperationType, model.BasicData.OperationTypeCode)
                .Permission)
            {
                excludeValuesRoleOrg = new List<string> { OrgUnitRoleCode.ORG_UDR };
            }
            else if (_checkOperationTypeService
                .IsCipOperation(model.BasicData.OperationType, model.BasicData.OperationTypeCode)
                .Permission)
            {
                excludeValuesRoleOrg = new List<string> { OrgUnitRoleCode.ORG_UDR };
            }

            ViewBag.ResponsibleUnitsRoleList = _catalogService.GetListMasterData(
                MasterType.ORG_UNIT_ROLE,
                excludeByCode: excludeValuesRoleOrg,
                listRepository: masterDataListByTypeCode);

            var divisions = _viewModelMapperHelper.GetDivisionList();

            List<SelectListItem> divisionsResponsibleList = new List<SelectListItem>(divisions);

            if (operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER)
            {
                divisions = _viewModelMapperHelper.AddDivisionExpired(
                    divisions,
                    model.ResponsabilityData.ResponsibleUnits
                        .Select(o => o.OrganizationalUnit).ToList());
            }

            ViewBag.ResponsibleUnitsOrganizationalUnitList = divisions;

            var excludeRelationshipType = new List<string>
            {
                RelationshipTypeItself
            };

            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE,
                true,
                excludeRelationshipType,
                listRepository: masterDataListByTypeCode);

            var excludeRelationshipRole = new List<string>
            {
                RelationshipTypeItself
            };

            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE,
                excludeByCode: excludeRelationshipRole,
                listRepository: masterDataListByTypeCode);

            var excludeValues = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                MemberRoleCode.PCR_AUTOR,
                MemberRoleCode.ESG_REVIEWER,
                MemberRoleCode.CREATOR_ROLE,
                MemberRoleCode.VPC_COUNTRY_ECONOMIST,
                MemberRoleCode.VPC_TEAM_MEMBER
            };

            if (model.ResponsabilityData.ExistTeamLeaderNotEditable)
            {
                excludeValues.Add(MemberRoleCode.TEAM_LEADER);
                excludeValues.Add(MemberRoleCode.ALTERNATIVE_TEAM_LEADER);
            }

            if (model.ResponsabilityData.ExistOperationAnalistNotEditable)
            {
                excludeValues.Add(MemberRoleCode.OPERATIONAL_ANALIST);
            }

            ViewBag.OperationTeamsRoleList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE, excludeByCode: excludeValues);

            var includeValuesForEsg = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                MemberRoleCode.ESG_REVIEWER
            };

            var excludeValuesForESG = new List<string>
            {
                    MemberRoleCode.CREATOR_ROLE,
                    MemberRoleCode.ESG_REVIEWER
            };

            ViewBag.OperationTeamsRoleEsgList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForEsg,
                excludeByCode: excludeValuesForESG);

            var includeValuesForSpdPcr = new List<string> { MemberRoleCode.PCR_AUTOR };

            var excludeValueEsgeRev = new List<string> { MemberRoleCode.ESG_REVIEWER };

            ViewBag.OperationTeamsRoleSpdPcrList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForSpdPcr,
                excludeByCode: excludeValueEsgeRev);

            ViewBag.OperationTeamsRoleList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                excludeByCode: excludeValues,
                listRepository: masterDataListByTypeCode);

            ViewBag.OperationTeamsRoleEsgList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForEsg,
                excludeByCode: excludeValuesForESG,
                listRepository: masterDataListByTypeCode);

            ViewBag.OperationTeamsRoleSpdPcrList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForSpdPcr,
                excludeByCode: excludeValueEsgeRev,
                listRepository: masterDataListByTypeCode);

            var includeValuesForSpdDemCoordinator = new List<string>
            {
                MemberRoleCode.SPD_SPECIALIST
            };

            ViewBag.OperationTeamsRoleDemSpdSpecialistLists = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                includeByCode: includeValuesForSpdDemCoordinator);

            ViewBag.AssociatedCountriesRoleList = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE,
                listRepository: masterDataListByTypeCode);

            var excludeCountries = new List<string>
            {
                CountryCode.BK,
                CountryCode.RG,
                CountryCode.IDB,
                CountryCode.UND
            };

            ViewBag.Countries = _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY,
                excludeByCode: excludeCountries,
                listRepository: masterDataListByTypeCode);

            ViewBag.RoleCoutryAdmin = model.ResponsabilityData.CountryRelatedRoleAdminId;

            ViewBag.CoutryAdminId = model.ResponsabilityData.CountryAdminId;

            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE,
                listRepository: masterDataListByTypeCode);

            ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionFilteredList(
                model.ResponsabilityData.AssociatedInstitutions
                    .Select(x => x.Institutions)
                    .ToList());

            ViewBag.OperationTypeList = _catalogService.GetListMasterData(
                MasterType.OPERATION_TYPE,
                true,
                listRepository: masterDataListByTypeCode);

            ViewBag.CategoryList = _catalogService.GetListMasterData(
                MasterType.CATEGORY,
                listRepository: masterDataListByTypeCode);

            var excludeValuesSector = new List<string> { CountryCode.UND };

            ViewBag.OperationSectorList = _catalogService.GetListMasterData(
                MasterType.SECTOR,
                excludeByCode: excludeValuesSector,
                listRepository: masterDataListByTypeCode);

            ViewBag.OperationSubsectorList = _viewModelMapperHelper
                .GetSubSectorList(model.BasicData.Sector, MasterType.SUBSECTOR);

            var mbf = _crossCreationOperationService
                .GetMbfCode(model.BasicData.OperationType, operationNumber);

            ViewBag.MbfCodeList = mbf.MbfCodesList;
            model.BasicData.MbfCodes = ViewBag.MbfCodeList;

            ViewBag.CountryList = _catalogService.GetListMasterData(
                MasterType.COUNTRY,
                listRepository: masterDataListByTypeCode);

            ViewBag.CountryDepartmentList = model.BasicData.IsRegional ?
                _viewModelMapperHelper.GetListCountryDeparmet(operationNumber) :
                null;

            ViewBag.Years = _viewModelMapperHelper.GetYear(model.BasicData);

            ViewBag.ResponsibleList = divisionsResponsibleList;

            ViewBag.IsRegionalPermission = _checkIsRegionalPermissionService
                .GetPermission(model.BasicData.IsRegional)
                .permission;

            ViewBag.CountryGroupList = ViewBag.IsRegionalPermission ?
                _viewModelMapperHelper.GetCountryGroupList() :
                new List<SelectListItem>();

            ViewBag.OPRPermision = _checkOperationTypeService
                .IsEditableOpr(operation.OperationId)
                .Permission;

            ViewBag.IsRequiredOPR = !isOprOperation;
            ViewBag.BorrowerId = model.BasicData.BorrowerId;
            ViewBag.ExecutorId = model.BasicData.ExecutorId;
            ViewBag.LoanId = model.BasicData.OperationTypeLoadnId;
            ViewBag.OrganizationalUnitResponsibleUnit = model.ResponsabilityData
                .OrganizationalUnitResponsibleUnit;
            ViewBag.RegionalId = model.ResponsabilityData.RegionalId;
            ViewBag.CountryRelatedRoleAdmin = model.ResponsabilityData.CountryRelatedRoleAdminId;
            ViewBag.CountryRelatedRoleParticipant = model.ResponsabilityData
                .CountryRelatedRoleParticipantId;

            var formAttributes = _viewModelMapperHelper
                .GetAttributesBasic(operationNumber, model.GetAttributesBasicResponse)
                .Attributes
                .FormAttributes;

            ViewBag.FormBasicAttributes = formAttributes;

            model.BasicData.SerializedModel = PageSerializationHelper
                .SerializeObject(model.BasicData);

            var responseVisibility = _checkVisibilityService.GetVisibility(
                operationTypeId: model.BasicData.OperationType,
                operationNumber: operationNumber,
                attributeService: _attributesEngineService,
                operationTypeCode: model.BasicData.OperationTypeCode);

            responseVisibility.Opr = _checkOperationTypeService
                .IsEditableOpr(model.ResponsabilityData.OperationId)
                .Permission;

            ViewBag.Visibility = responseVisibility;

            model.BasicData.CategoryAccess.IsVisible = responseVisibility.Category;

            ViewBag.IsESGPermission = true;

            var cmBusiness = CMBusiness.Get(operationNumber);

            var activityItemAppr = cmBusiness.GetActivityItem(CMConstants.DefaultActivityItems.APPR);

            model.StrategicAlignment.StrategicAlignmentWrite = IDBContext.Current
                .HasPermission(Permission.STRATEGIC_ALIGNMENT_WRITE) &&
                (activityItemAppr == null || !activityItemAppr.IsCompleted());

            ViewBag.HasSubnational = _operationSubnationalLevelRepository
                .Any(o => o.OperationId == operation.OperationId);

            ViewBag.SGId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.LENDING_TYPE &&
                    o.Code.Equals(AttributeValue.SG))
                .First()
                .MasterId;

            ViewBag.OpType = string.Empty;

            ViewBag.CONId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.CON))
                .First()
                .MasterId;

            ViewBag.CIPId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.CIP))
                .First()
                .MasterId;

            ViewBag.ESWId = masterDataListByTypeCode.MasterDataCollection
                .Where(o => o.TypeName == MasterType.OPERATION_TYPE &&
                    o.Code.Equals(OperationType.ESW))
                .First()
                .MasterId;

            ViewBag.OtherSectorId = _catalogService.GetConvergenceMasterDataIdByCode(
                OPUSGlobalValues.OT, MasterType.SECTOR).Id;

            ViewBag.NSGId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.NSG, MasterType.LENDING_TYPE).Id;

            var isCndOperation = _componentCNDService.IsCNDOperation(operationNumber);

            if (!isCndOperation.IsValid)
                throw new Exception("Some error ocurred when checking CND operation");

            model.IsCndOperation = isCndOperation.HasCondition;

            if (model.ResponsabilityData.OperationTeams != null)
            {
                model.ResponsabilityData.OperationTeams
                    .RemoveAll(o => o.RoleName == Role.ESG_REVIEWER);
            }

            ViewBag.OperationNumber = operationNumber;

            string UserName = IDBContext.Current.UserName;

            var roleListModal = new List<string>();

            model.DelegatorMessage = _securityModelRepository
                .GetDelegatorMessages(roleListModal, operationNumber, UserName);

            if (model.DelegatorMessage)
            {
                IDBContext.Current.ExpireAndReloadSecurityForOperation(operationNumber);
            }

            var cmb = Globals.Resolve<ICMBusiness>().SetOperation(operation: operation);

            ViewBag.IsApproved = cmb.IsOperationInExecution();

            SetSecurityBasicData();

            model.BasicData.CategoryAccess.IsRequired = IsCategoryMandatory(
                model.BasicData.OperationType,
                formAttributes,
                CalculateExpectedIDBAmounts(operation.OperationId));

            var spdSpecialistConvergenceMasterData = _convergenceMasterDataRepository
                .GetOne(o => o.Code == DemGlobalValues.SPD_SPECIALIST_MASTER_DATA_CODE &&
                     o.ConvergenceMasterType.Type == MasterType.MEMBER_ROLE);

            if (spdSpecialistConvergenceMasterData != null)
            {
                ViewBag.SPDSpecialistConvergenceMasterDataId = spdSpecialistConvergenceMasterData
                    .ConvergenceMasterDataId;
            }
            else
            {
                ViewBag.SPDSpecialistConvergenceMasterDataId = 0;
            }

            model.IsCCESAllowed = OPUSGlobalValues.CCES_OP_LIST_ALLOWED
                .Contains(model.BasicData.OperationTypeCode);

            model.IsClassificationDataAllowed =
                !OperationTypesList.Contains(OperationType.EFC) ||
                IDBContext.Current.HasPermission(Permission.CLASSIFICATION_DATA_TAB_ACCESS);

            model.ResponsabilityData.OperationTeamsRoleList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    excludeByCode: GetExcludedTeamRoles(
                        model.ResponsabilityData.ExistTeamLeaderNotEditable,
                        model.ResponsabilityData.ExistOperationAnalistNotEditable,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission,
                        true));

            model.ResponsabilityData.OperationTeamsRoleEsgList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesEsg(true),
                    excludeByCode: GetExcludedTeamRolesEsg(
                        true,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            model.ResponsabilityData.OperationTeamsRoleSpdPcrList = _catalogService
                .GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesSpdPcr(),
                    excludeByCode: GetExcludedTeamRolesSpdPcr(
                        true,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            if (model.ResponsabilityData.OperationTeams.Any(otr => otr.IsRoleExpired))
                model.ResponsabilityData.OperationExpiredTeamsRoles = _catalogService
                    .GetListItemMasterData(
                        type: MasterType.MEMBER_ROLE,
                        hideExpired: false,
                        includeById: model.ResponsabilityData.OperationTeams
                            .Where(otr => otr.IsRoleExpired && otr.Role.HasValue)
                            .Select(otr => otr.Role.Value)
                            .ToList());

            model.StrategicAlignment.DEMLockReviewProcessData = _demLockModulesService
                .BuildLockReviewProcessDEMDataSerializable(operationNumber);

            return View(model);
        }

        public virtual ActionResult OperationDataBasicDataContent(string operationNumber)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);

            var model = _viewModelMapperHelper.GetBasicData(operationNumber, true);

            SetPermissionOperationData();

            var excludeRelationshipType = new List<string> { RelationshipTypeItself };

            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE,
                true,
                excludeRelationshipType);

            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };

            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE,
                excludeByCode: excludeRelationshipRole);

            ViewBag.OperationTypeList = _catalogService
                .GetListMasterData(MasterType.OPERATION_TYPE, true);

            ViewBag.OperationSubTypeList = _catalogService
                .GetListMasterData(MasterType.OPERATION_SUBTYPE);

            var excludeValuesSector = new List<string> { CountryCode.UND };

            ViewBag.OperationSectorList = _catalogService
                .GetListMasterData(MasterType.SECTOR, excludeByCode: excludeValuesSector);

            ViewBag.OperationSubsectorList = _viewModelMapperHelper
                .GetSubSectorList(model.BasicData.Sector, MasterType.SUBSECTOR);

            ViewBag.CategoryList = _catalogService.GetListMasterData(MasterType.CATEGORY);

            ViewBag.BudgetCodeList = _catalogService.GetListMasterData(MasterType.BUDGET_CODE);

            model.BasicData.MbfCodes = _catalogService
                .GetListItemMasterData(MasterType.MBF, true, excludeByCode: null);

            ViewBag.CountryList = _catalogService.GetListMasterData(MasterType.COUNTRY);

            ViewBag.CountryCodeList = _viewModelMapperHelper.GetCountryRelatedCodeList(
                model.BasicData.CountryCode,
                Convert.ToInt32(model.BasicData.CountryId),
                model.BasicData.IsRegional);

            ViewBag.CountryDepartmentList = model.BasicData.IsRegional ?
                _viewModelMapperHelper.GetListCountryDeparmet(operationNumber) : null;

            ViewBag.Years = _viewModelMapperHelper.GetYear(model.BasicData);

            ViewBag.ResponsibleList = _viewModelMapperHelper.GetDivisionList();

            ViewBag.IsRegionalPermission = _checkIsRegionalPermissionService
                .GetPermission(model.BasicData.IsRegional)
                .permission;

            ViewBag.CountryGroupList = ViewBag.IsRegionalPermission ?
                _viewModelMapperHelper.GetCountryGroupList() :
                new List<SelectListItem>();

            ViewBag.OPRPermision = _checkOperationTypeService
                .IsEditableOpr(model.OperationId)
                .Permission;

            ViewBag.IsRequiredOPR = !_checkOperationTypeService
                .IsOprOperation(model.BasicData.OperationType)
                .Permission;

            ViewBag.BorrowerId = model.BasicData.BorrowerId;
            ViewBag.ExecutorId = model.BasicData.ExecutorId;
            ViewBag.LoanId = model.BasicData.OperationTypeLoadnId;

            var dataAttributes = _viewModelMapperHelper
               .GetAttributesBasic(
                    operationNumber,
                    operationType: model.BasicData.OperationTypeCode,
                    hasAbstractLock: model.BasicData.HasAbstractLock)
               .Attributes
               .FormAttributes;

            ViewBag.FormBasicAttributes = dataAttributes;

            model.BasicData.SerializedModel = PageSerializationHelper
                .SerializeObject(model.BasicData);

            var responseVisibility = _checkVisibilityService.GetVisibility(
                operationTypeId: model.BasicData.OperationType,
                operationNumber: operationNumber,
                attributeService: _attributesEngineService);

            responseVisibility.Opr = _checkOperationTypeService
                .IsEditableOpr(model.OperationId)
                .Permission;

            ViewBag.Visibility = responseVisibility;

            model.BasicData.CategoryAccess.IsVisible = responseVisibility.Category;

            ViewBag.HasSubnational = _operationSubnationalLevelRepository
                .Any(o => o.OperationId == model.BasicData.OperationType);

            SetSecurityBasicData();

            model.BasicData.CategoryAccess.IsRequired = IsCategoryMandatory(
                model.BasicData.OperationType,
                dataAttributes,
                CalculateExpectedIDBAmounts(model.OperationId));

            return PartialView(
                "Partials/OperationData/OperationDataBasicDataPartial",
                model.BasicData);
        }

        public virtual ActionResult ClimateChangeDataContent(string operationNumber)
        {
            return PartialView("Partials/OperationData/ClimateChangeParcialRead",
                _viewModelMapperHelper.GetClimateChangeData(operationNumber));
        }

        public virtual ActionResult OperationResponsabilityContent(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetResponsabilityData(operationNumber, true);
            SetPermissionOperationData();

            bool isPRG = OperationTypeHelper.GetOperationTypes(
                model.ResponsabilityData.OperationId).Contains(OperationType.PRG);

            var excludeValuesRoleOrg = new List<string>();
            if (_checkOperationTypeService.IsOprOperation(
                model.ResponsabilityData.OperationType).Permission)
            {
                excludeValuesRoleOrg = new List<string>
                {
                    OrgUnitRoleCode.ORG_UDR,
                    OrgUnitRoleCode.CO_RESPONSIBLE
                };
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(
                model.ResponsabilityData.OperationId);
            if (!UDRBusinessLogic.MustIncludeUDRAsRole(opTypes))
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.ORG_UDR);

            ViewBag.ResponsibleUnitsRoleList = _catalogService
                .GetListMasterData(MasterType.ORG_UNIT_ROLE, excludeByCode: excludeValuesRoleOrg);

            var divisions = _viewModelMapperHelper.GetDivisionList();
            List<SelectListItem> divisionsList = new List<SelectListItem>(divisions);

            if (model.ResponsabilityData.OperationTeams != null)
            {
                model.ResponsabilityData.OperationTeams
                    .RemoveAll(o => o.RoleName == Role.ESG_REVIEWER);
            }

            if (operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER)
            {
                divisions = _viewModelMapperHelper.AddDivisionExpired(divisions,
                    model.ResponsabilityData.ResponsibleUnits
                    .Select(o => o.OrganizationalUnit).ToList());
            }

            ViewBag.ResponsibleUnitsOrganizationalUnitList = divisions;

            ViewBag.CollaboratingUnitsOrganizationalUnitList = divisionsList;

            var includeValuesForSpdDemCoordinator = new List<string>
            {
                MemberRoleCode.SPD_SPECIALIST
            };

            ViewBag.OperationTeamsRoleDemSpdSpecialistLists = _catalogService.GetListMasterData(
                MasterType.MEMBER_ROLE, includeByCode: includeValuesForSpdDemCoordinator);

            ViewBag.AssociatedCountriesRoleList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ROLE);
            var excludeCountries = new List<string>
            {
                CountryCode.BK,
                CountryCode.RG,
                CountryCode.IDB,
                CountryCode.UND
            };
            ViewBag.Countries =
                _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY, excludeByCode: excludeCountries);
            ViewBag.RoleCoutryAdmin = model.ResponsabilityData.CountryRelatedRoleAdminId;
            ViewBag.CoutryAdminId = model.ResponsabilityData.CountryAdminId;

            ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionList();
            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE);
            ViewBag.AssociatedInstitutionsInstitutionsList =
                _viewModelMapperHelper.GetInstitutionList();

            ViewBag.IsRegionalPermission = _checkIsRegionalPermissionService.GetPermission(
                model.ResponsabilityData.IsRegional).permission;

            ViewBag.OrganizationalUnitResponsibleUnit = model.ResponsabilityData
                .OrganizationalUnitResponsibleUnit;
            ViewBag.RegionalId = model.ResponsabilityData.RegionalId;
            ViewBag.CountryRelatedRoleAdmin = model.ResponsabilityData
                .CountryRelatedRoleAdminId;
            ViewBag.CountryRelatedRoleParticipant = model.ResponsabilityData
                .CountryRelatedRoleParticipantId;

            ViewBag.SerializedViewModelResponsibilityData =
                PageSerializationHelper.SerializeObject(model.ResponsabilityData);

            ViewBag.Visibility = _checkVisibilityService.GetVisibility(
                model.ResponsabilityData.OperationType, operationNumber);

            ViewBag.Visibility.Opr = _checkOperationTypeService.IsEditableOpr(
                model.ResponsabilityData.OperationId).Permission;

            var operationId = _operationRepository.GetOne(o => o.OperationNumber == operationNumber)
                .OperationId;
            var cmb = Globals.Resolve<ICMBusiness>().SetOperation(operationId);
            ViewBag.IsApproved = cmb.IsOperationInExecution();

            model.ResponsabilityData.OperationTeamsRoleList =
                _catalogService.GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    excludeByCode: GetExcludedTeamRoles(
                        model.ResponsabilityData.ExistTeamLeaderNotEditable,
                        model.ResponsabilityData.ExistOperationAnalistNotEditable,
                        !(bool)ViewBag.SPDDEMResponsibilityWritePermission,
                        false));

            model.ResponsabilityData.OperationTeamsRoleEsgList =
                _catalogService.GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesEsg(false),
                    excludeByCode: GetExcludedTeamRolesEsg(isPRG,
                    !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            model.ResponsabilityData.OperationTeamsRoleSpdPcrList =
                _catalogService.GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: GetIncludedTeamRolesSpdPcr(),
                    excludeByCode: GetExcludedTeamRolesSpdPcr(isPRG,
                    !(bool)ViewBag.SPDDEMResponsibilityWritePermission));

            SetSecurityTabResponsibilityData();

            var spdSpecialistConvergenceMasterData = _convergenceMasterDataRepository.GetOne(
                o => o.Code == DemGlobalValues.SPD_SPECIALIST_MASTER_DATA_CODE
                    && o.ConvergenceMasterType.Type == MasterType.MEMBER_ROLE);

            if (spdSpecialistConvergenceMasterData != null)
            {
                ViewBag.SPDSpecialistConvergenceMasterDataId =
                    spdSpecialistConvergenceMasterData.ConvergenceMasterDataId;
            }
            else
            {
                ViewBag.SPDSpecialistConvergenceMasterDataId = 0;
            }

            if (model.ResponsabilityData.OperationTeams.Any(otr => otr.IsRoleExpired))
                model.ResponsabilityData.OperationExpiredTeamsRoles =
                    _catalogService.GetListItemMasterData(
                        type: MasterType.MEMBER_ROLE,
                        hideExpired: false,
                        includeById: model.ResponsabilityData.OperationTeams
                            .Where(otr => otr.IsRoleExpired && otr.Role.HasValue)
                            .Select(otr => otr.Role.Value).ToList());

            var catalogData = _catalogService.GetConvergenceMasterDataIdByCode(
                OPUSGlobalValues.CREATOR_ROLE_CODE,
                MasterType.MEMBER_ROLE);

            if (!catalogData.IsValid)
            {
                Logger.GetLogger().WriteMessage(
                    "CreationFormMappers - ConvertToCreationFormViewModel",
                    "Fail to get creator role Id");

                throw new Exception("Catalog Service - Can't get creator role Id");
            }

            model.ResponsabilityData.OperationTeams.RemoveAll(o => o.Role == catalogData.Id);

            return PartialView(
                "Partials/OperationData/OperationDataResponsibilityDataPartial",
                model.ResponsabilityData);
        }

        public virtual ActionResult ApprovedOperationResponsabilityContent(string operationNumber)
        {
            ICollection<ResponsabilityDataApprovedViewModel> modelFinal =
                new Collection<ResponsabilityDataApprovedViewModel>();

            var reformulation = _reformulationService.GetFormulations(operationNumber, false);

            foreach (var item in reformulation.Models)
            {
                var operation = _operationHistoryService.GetHistory(item, operationNumber);

                var countryRelated = _countryRelatedApprovedRepository
                        .GetByCriteria(
                            _operationHistoryService
                                .BuildCountryRelatedApprovedExpr(operation, item.ActivityPlanId))
                        .ToList();
                var operationTeamRelated = _operationTeamDataApprovedRepository
                        .GetByCriteria(
                            _operationHistoryService
                                .BuildOperationTeamDataApprovedExpr(operation, item.ActivityPlanId))
                        .ToList();
                var organizationUnitRelated = _organizationalUnitRelatedApprovedRepository
                        .GetByCriteria(
                            _operationHistoryService
                                .BuildOrganizationalUnitRelatedApprovedExpr(operation, item.ActivityPlanId))
                        .ToList();
                var institutionRelated = _institutionRelatedApprovedRepository
                        .GetByCriteria(
                            _operationHistoryService
                                .BuildInstitutionRelatedApprovedExpr(operation, item.ActivityPlanId))
                        .ToList();

                ResponsabilityDataApprovedViewModel responsabilityDataApprovedViewModel =
                    operation.ConvertToResponsabilityDataApprovedViewModel(
                        organizationUnitRelated,
                        operationTeamRelated,
                        institutionRelated,
                        countryRelated,
                         _catalogService,
                        _operationTeamDataService,
                            _divisionOpusRepository,
                            item);

                modelFinal.Add(responsabilityDataApprovedViewModel);
            }

            var modelCurrent = modelFinal.Single(x => x.IsCurrent);
            SetPermissionOperationData();

            var excludeValuesRoleOrg = new List<string>();
            if (_checkOperationTypeService
                .IsOprOperation(modelCurrent.OperationType)
                .Permission)
            {
                excludeValuesRoleOrg = new List<string>
                {
                    OrgUnitRoleCode.ORG_UDR,
                    OrgUnitRoleCode.CO_RESPONSIBLE
                };
            }

            ViewBag.ResponsibleUnitsRoleList = _catalogService
                .GetListMasterData(MasterType.ORG_UNIT_ROLE, excludeByCode: excludeValuesRoleOrg);

            var divisions = _viewModelMapperHelper.GetDivisionList();

            divisions = _viewModelMapperHelper.AddDivisionExpired(
                divisions,
                modelCurrent
                    .ResponsibleUnits
                    .Select(o => o.OrganizationalUnit)
                    .ToList());

            ViewBag.ResponsibleUnitsOrganizationalUnitList = divisions;
            ViewBag.Visibility = _checkVisibilityService.GetVisibility(
                modelCurrent.OperationType,
                operationNumber);
            ViewBag.Visibility.Opr = _checkOperationTypeService
                .IsEditableOpr(modelCurrent.OperationId)
                .Permission;
            ViewBag.SerializedViewModelResponsibilityData =
               PageSerializationHelper.SerializeObject(modelCurrent);
            ViewBag.IsRegionalPermission = _checkIsRegionalPermissionService
                .GetPermission(modelCurrent.IsRegional)
                .permission;
            ViewBag.IsApproved = false;
            ViewBag.AssociatedCountriesRoleList = _catalogService
                .GetListMasterData(MasterType.COUNTRY_ROLE);

            var excludeCountries = new List<string>
            {
                CountryCode.BK,
                CountryCode.RG,
                CountryCode.IDB,
                CountryCode.UND
            };
            ViewBag.Countries = _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY,
                excludeByCode: excludeCountries);
            ViewBag.RoleCoutryAdmin = modelCurrent.CountryRelatedRoleAdminId;
            ViewBag.CoutryAdminId = modelCurrent.CountryAdminId;
            ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionList();
            ViewBag.AssociatedInstitutionsRoleList = _catalogService
                .GetListMasterData(MasterType.INSTITUTION_ROLE);
            SetSecurityTabResponsibilityData();

            return
                PartialView(
                    "Partials/OperationData/ApprovedResponsibilityDataPartial", modelFinal);
        }

        public virtual ActionResult OperationClassificationDataContent(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetAttributes(operationNumber);

            SetPermissionOperationData();

            ViewBag.SerializedViewModelAttributtes =
                PageSerializationHelper.SerializeObject(model.Attributes.Attributes);

            SetSecurityTabClassificationData();

            return
                PartialView("Partials/OperationData/ClassificationDataPartial", model.Attributes.FormAttributes);
        }

        public virtual ActionResult StrategicAlignmentDataContent(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetStrategicAlignment(operationNumber);

            SetPermissionOperationData();

            ViewBag.SerializedViewModelSA = PageSerializationHelper
                .SerializeObject(model.StrategicAlignment);

            var resultMatrixResponse = _operationDataService
                .GetResultMatrix(operationNumber);

            if (resultMatrixResponse.IsValid)
            {
                var resultMatrix = resultMatrixResponse.Model;
                SetViewbagStrategicAlignment(
                    operationNumber, resultMatrix, model.StrategicAlignment);

                ViewBag.StrategicAlignmentPermission = true;
                ViewBag.IsOperationEswType = model.StrategicAlignment.IsOperationEswType;

                model.StrategicAlignment.ResultMatrixId =
                    resultMatrix.ResultsMatrixId;

                return PartialView(
                    "Partials/OperationData/StrategicAlignmentPartial",
                    model.StrategicAlignment);
            }

            return PartialView(
                "Partials/OperationData/StrategicAlignmentPartialError",
                resultMatrixResponse);
        }

        public virtual ActionResult ClimateChangeContent(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetClimateChange(operationNumber);

            SetPermissionOperationData();

            if (model.ClimateChange != null &&
                (model.ClimateChange.ClimateChangePermission.CCSEWritePermission
                || model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission))
            {
                model.ClimateChange.ClimateChangePermission.CCSEWrite =
                    ClimateChangeControl(operationNumber,
                    model.ClimateChange.ClimateChangePermission.CCSEWritePermission,
                    model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission);
            }

            ViewBag.SerializedViewModelSA = PageSerializationHelper
                .SerializeObject(model.ClimateChange);

            return PartialView("Partials/OperationData/ClimateChangeParcialRead",
                model.ClimateChange);
        }

        public virtual JsonResult FillSapCodeSpecialProgram(int idFund)
        {
            var response = _financialDataPreparationService.GetAvailabilityData(idFund);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult FillSubSector(int? idSector)
        {
            return
                Json(_viewModelMapperHelper.GetSubSectorList(
                idSector, MasterType.SUBSECTOR), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CalculationEffortDays(int effort)
        {
            return Json(_viewModelMapperHelper.GetCalculationEffortDays(effort), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetRowResponsibleUnit(string operationNumber, int operationType)
        {
            var excludeValuesRoleOrg = new List<string>();

            if (_checkOperationTypeService.IsOprOperation(operationType).Permission)
            {
                excludeValuesRoleOrg = new List<string>
                {
                    OrgUnitRoleCode.ORG_UDR,
                    OrgUnitRoleCode.CO_RESPONSIBLE
                };
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(operationNumber);
            if (!UDRBusinessLogic.MustIncludeUDRAsRole(opTypes))
                excludeValuesRoleOrg.Add(OrgUnitRoleCode.ORG_UDR);

            ViewBag.ResponsibleUnitsRoleList = _catalogService.GetListMasterData(
                MasterType.ORG_UNIT_ROLE, excludeByCode: excludeValuesRoleOrg);

            ViewBag.ResponsibleUnitsOrganizationalUnitList = _viewModelMapperHelper.GetDivisionList();

            return PartialView("Partials/OperationData/DataTables/RowResponsibleUnit");
        }

        public virtual ActionResult GetClimateChange(string operationNumber, int operationType)
        {
            var excludeValuesRoleOrg = new List<string>();
            if (_checkOperationTypeService.IsOprOperation(operationType).Permission)
            {
                excludeValuesRoleOrg = new List<string>
                {
                    OrgUnitRoleCode.ORG_UDR,
                    OrgUnitRoleCode.CO_RESPONSIBLE
                };
            }

            ViewBag.ResponsibleUnitsRoleList = _catalogService.GetListMasterData(
                MasterType.ORG_UNIT_ROLE, excludeByCode: excludeValuesRoleOrg);
            ViewBag.ResponsibleUnitsOrganizationalUnitList = _viewModelMapperHelper.GetDivisionList();

            return PartialView("Partials/OperationData/DataTables/RowClimateChange");
        }

        public virtual JsonResult GetRelationshipRole(
            int relationshipType,
            List<OPUSCheckOperation> tableArray = null,
            int opType = 0,
            string deleteOpInView = "")
        {
            if (tableArray == null || tableArray[0] == null)
            {
                tableArray = new List<OPUSCheckOperation>();
            }

            var response = _viewModelMapperHelper.GetListRelationshipRoleList(
                relationshipType,
                MasterType.RELATIONSHIP_ROLE,
                RelationshipRoleItself,
                tableArray,
                opType: opType,
                deleteOpInView: deleteOpInView);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual JsonResult GetOrganizationalUnits(
            string filter, string lendingType, int actualDivision = 0, string origin = "")
        {
            var operationTypeId = string.Empty;
            var operationTypeCode = string.Empty;
            string guaranteedById = string.Empty;
            string nsgCategorizationId = string.Empty;

            var orgUnit = Request.QueryString[origin == OPUSGlobalValues.OPERATION_DATA ?
                ORG_UNIT_OD : ORG_UNIT];

            if (origin == OPUSGlobalValues.OPERATION_DATA)
            {
                var operation = _operationRepository
                    .GetOne(o => o.OperationNumber == IDBContext.Current.Operation);

                if (operation != null)
                {
                    operationTypeId = operation.OperationData.OperationTypeId.ToString();

                    var lending = operation.FormAttributeOperation
                        .FirstOrDefault(o => o.FormAttribute.Code == AttributeCode.LENDING_TYPE);

                    guaranteedById = lending != null ?
                        lending.ListValueID.ToString() : string.Empty;

                    var instruments = operation.FormAttributeOperation
                        .FirstOrDefault(o => o.FormAttribute.Code == AttributeCode.INSTRUMENTS);

                    nsgCategorizationId = instruments != null ?
                        instruments.ListValueID.ToString() : string.Empty;
                }
            }
            else
            {
                operationTypeId = Request.QueryString[OPER_TYPE];
                guaranteedById = Request.QueryString[LENDING];

                operationTypeCode = _catalogService.GetConvergenceMasterCodeByIdResponse(
                    operationTypeId.ConvertToInt()).Code;

                nsgCategorizationId = operationTypeCode == OperationType.CON ?
                    Request.QueryString[NSG_CATEGORIZATION_FOR_CON] :
                    Request.QueryString[NSG_CATEGORIZATION];
            }

            var response = _viewModelMapperHelper.GetOrganizationalUnitsFilter(
                filter,
                lendingType,
                operationTypeId,
                orgUnit,
                guaranteedById,
                actualDivision,
                nsgCategorizationId);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage,
                    ListResponse = response.OrganizationalUnits
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetRowOperationTeam(
            string operationNumber,
            bool tLorAtLnotEditable,
            bool operationAnalistNotEditable)
        {
            SetPermissionOperationData();

            var countPermissions = 0;
            var codeRoleTeam = OPUSGlobalValues.ALL_ROLE;
            var excludeValues = new List<string>();
            var includeValues = new List<string>();

            if (ViewBag.ResponsabilityDataPermission)
            {
                excludeValues = new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST,
                    MemberRoleCode.SPD_DEM_COORDINATOR,
                    MemberRoleCode.CREATOR_ROLE
                };

                if (tLorAtLnotEditable)
                {
                    excludeValues.Add(MemberRoleCode.TEAM_LEADER);
                    excludeValues.Add(MemberRoleCode.ALTERNATIVE_TEAM_LEADER);
                }

                if (operationAnalistNotEditable)
                {
                    excludeValues.Add(MemberRoleCode.OPERATIONAL_ANALIST);
                }

                countPermissions++;
            }

            if (ViewBag.EsgSpecialistAssignmentWritePermission)
            {
                includeValues.AddRange(new List<string>
                {
                    MemberRoleCode.ESG_TEAM_MEMBER,
                    MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                });
                codeRoleTeam = OPUSGlobalValues.ESG_ROLE;
                countPermissions++;
            }

            if (ViewBag.SPDPCRWritePermission)
            {
                includeValues.AddRange(new List<string>
                {
                    MemberRoleCode.PCR_AUTOR
                });
                codeRoleTeam = OPUSGlobalValues.PCR_ROLE;
                countPermissions++;
            }

            if (ViewBag.SPDDEMResponsibilityWritePermission)
            {
                includeValues.AddRange(new List<string>
                {
                    MemberRoleCode.SPD_SPECIALIST
                });
                codeRoleTeam = OPUSGlobalValues.SPD_ROLE;
                countPermissions++;
            }

            if (!string.IsNullOrEmpty(operationNumber))
            {
                var operId = _operationRepository.GetOne(o => o.OperationNumber == operationNumber).OperationId;
                bool isPRG = OperationTypeHelper.GetOperationTypes(operId).Contains(OperationType.PRG);
                if (!isPRG)
                {
                    excludeValues.Add(MemberRoleCode.VPC_COUNTRY_ECONOMIST);
                    excludeValues.Add(MemberRoleCode.VPC_TEAM_MEMBER);
                }

                var responsabilityResponse =
                    _viewModelMapperHelper.GetResponsabilityData(operationNumber, true);
            }

            if (countPermissions > 1)
            {
                codeRoleTeam = OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE;
            }

            var teamList =
                _catalogService.GetListItemMasterData(
                    type: MasterType.MEMBER_ROLE,
                    includeByCode: includeValues,
                    excludeByCode: excludeValues);

            ViewBag.RoleTypeCode = codeRoleTeam;

            return PartialView("Partials/OperationData/DataTables/RowOperationTeam", teamList);
        }

        public virtual ActionResult GetRowAssociatedCountries(string operationNumber)
        {
            ViewBag.AssociatedCountriesRoleList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ROLE);
            var excludeCountries =
                new List<string>
                {
                    CountryCode.BK,
                    CountryCode.RG,
                    CountryCode.IDB,
                    CountryCode.UND
                };
            ViewBag.Countries =
                _catalogService.GetListMultiMasterData(
                MasterType.COUNTRY, excludeByCode: excludeCountries);
            return PartialView("Partials/OperationData/DataTables/RowAssociatedCountries");
        }

        public virtual ActionResult GetRowAssociatedInstitution(
            string operationNumber,
            string role,
            string institution)
        {
            ViewBag.AssociatedInstitutionCountryCodeList = _catalogService.GetListMasterData(
                MasterType.COUNTRY_ASSOCIATED, true);
            ViewBag.AssociatedInstitutionType = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_TYPE);
            ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                MasterType.INSTITUTION_ROLE);
            ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionList();
            ViewBag.RoleSelect = role;
            ViewBag.Institution = institution;
            return PartialView("Partials/OperationData/DataTables/RowAssociatedInstitution");
        }

        public virtual ActionResult GetRowUserComment(string operationNumber)
        {
            return PartialView("Partials/OperationData/DataTables/RowUserComment");
        }

        public virtual JsonResult CheckOperationAvailability(
            int relationshipType,
            int relationshipRole,
            int relatedOperationId,
            string deleteOpInView,
            int currentSectorId,
            IList<OPUSCheckOperation> tableArray = null,
            int relationshipRoleThisOp = 0,
            IList<AssociatedInstitution> institutions = null)
        {
            if (tableArray == null || tableArray[0] == null)
                tableArray = new List<OPUSCheckOperation>();

            var response = _checkOperationAvailabilityService.CheckOperationAvailability(
                relationshipType,
                relationshipRole,
                relationshipRoleThisOp,
                relatedOperationId,
                currentSectorId,
                tableArray,
                deleteOpInView,
                institutions);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult SearchAssociatedInstitution(
            string nameAcronym, int type = 0, int countryId = 0, bool hideExpired = false)
        {
            return Json(
                _viewModelMapperHelper.GetInstitutionList(nameAcronym, type, countryId, hideExpired),
                JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetAdditionalDataOperationTeam(
            string operationNumber, string userName, string organization, bool hideExpired = false)
        {
            return Json(
                _viewModelMapperHelper.GetAdditionalDataOperationTeam(
                    operationNumber, userName, organization, hideExpired),
                JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult VerifyOperationTeamUser(string userName, List<string> tableArray)
        {
            return Json(_operationTeamDataService.CheckUserOperationTeam(userName, tableArray));
        }

        public virtual JsonResult GetCountryDeptByCountry(int countryId)
        {
            return Json(_crossCreationOperationService.GetCountryDept(countryId));
        }

        public virtual JsonResult GetMbfCodeExecution(
            string operationNumber,
            int? operationType,
            int? intrumentsAttr,
            int? lendingTypeAttr,
            int? capitalProjModalityAttr,
            int? conModalityAttr,
            int? facilityTypeAttr,
            int? tCTaxononyAttr,
            int? prgModalityAttr,
            int? iopModalityAttr,
            string code)
        {
            var mbf = _crossCreationOperationService.GetMbfCodeExecution(
                operationType,
                operationNumber,
                intrumentsAttr,
                lendingTypeAttr,
                capitalProjModalityAttr,
                conModalityAttr,
                facilityTypeAttr,
                tCTaxononyAttr,
                prgModalityAttr,
                iopModalityAttr);

            var data = mbf.MbfCodesSelectListItem;

            var value = string.Empty;
            if (data.Count() != 1 && !string.IsNullOrEmpty(code))
            {
                value = _viewModelMapperHelper.GetExpiredMbfItem(code);

                if (!string.IsNullOrEmpty(value))
                {
                    data.Add(new SelectListItem
                    {
                        Text = code,
                        Value = value
                    });
                }
            }

            return
                Json(new
                {
                    Data = data.OrderBy(o => o.Text).Distinct(),
                    Editable = mbf.IsEditable,
                    IsValid = mbf.IsValid,
                    IdSelected = value,
                    NameSelected = code
                }, JsonRequestBehavior.AllowGet);
        }

        public virtual FileResult OperationTeamExcel(string operationNumber)
        {
            var response = _operationDataService.GetOperationTeamReport(operationNumber);

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/vnd.ms-excel";

            return File(response.File, application);
        }

        public virtual JsonResult ValidateAttrCon(string conModality, string facilityType)
        {
            var rrId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeCode.RAPID_RESPONSE &&
                    o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper())
                .ConvergenceMasterDataId;
            var ffgId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeValue.FFG &&
                    o.ConvergenceMasterType.Type == AttributeCode.FACILITY_TYPE.ToUpper())
                .ConvergenceMasterDataId;
            var cpdId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeValue.CPD &&
                    o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper())
                .ConvergenceMasterDataId;
            var oprId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeValue.OPR &&
                    o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper())
                .ConvergenceMasterDataId;
            var mifpId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeValue.MIFP &&
                    o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper())
                .ConvergenceMasterDataId;
            var ccfId = _convergenceMasterDataRepository
                .GetOne(o => o.Code == AttributeValue.CCF &&
                    o.ConvergenceMasterType.Type == AttributeCode.FACILITY_TYPE.ToUpper())
                .ConvergenceMasterDataId;
            var isVisible = conModality.Equals(rrId.ToString()) ||
                conModality.Equals(cpdId.ToString()) ||
                conModality.Equals(oprId.ToString()) ||
                facilityType.Equals(ffgId.ToString()) ||
                facilityType.Equals(ccfId.ToString()) ||
                conModality.Equals(mifpId.ToString());

            return Json(new { IsVisible = isVisible }, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ValidateRole(int role)
        {
            string[] relationTypeCode = { RelationTypeCode.PFM, RelationTypeCode.PBP };
            var roles = _convergenceMasterDataRepository
                .GetByCriteria(o => o.Code.Contains(RelationRoleCode.FIRST) &&
                    relationTypeCode.Contains(o.ParentConvergenceMasterData.Code));

            bool isDisabled = roles.Any(x => x.ConvergenceMasterDataId == role);

            return Json(isDisabled, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult VerifyStaffMember(string userName)
        {
            return new JsonResult
            {
                Data = _operationTeamDataService.IsUserTypeStaffMember(userName),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region FinancialDataExecution

        public virtual ActionResult FinancialDataExecution(string operationNumber)
        {
            var modelFinal = GetFinancialDataExecution(operationNumber);

            var model = modelFinal.Single(x => x.IsCurrent);
            SetPermissionFinancialData();

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(modelFinal);
        }

        public virtual ActionResult FinancialDataExecutionReloadContract(
            string operationNumber, string approvalNumber, int? activityPlanId = null)
        {
            var model = GetContractLevelOperation(operationNumber, approvalNumber, activityPlanId);

            return PartialView("Partials/FinancialDataExecution/FDEContractLevelOperationTabPartial", model);
        }

        public virtual ActionResult FinancialDataExecutionReload(string operationNumber)
        {
            var modelFinal = GetFinancialDataExecution(operationNumber);
            var model = modelFinal.Single(x => x.IsCurrent);
            var isOperationigc = _checkOperationTypeService.IsIgrOperation(
                _convergenceMasterDataRepository.GetOne(x => x.Code.Equals(model.OperationType)).ConvergenceMasterDataId);
            ViewBag.OperationTypeValid = model.FundIncreases.OperationTypeValid || isOperationigc.Permission;
            return PartialView("Partials/FinancialDataExecution/FDEIncreasesPartial", model.FundIncreases);
        }

        public virtual ActionResult GetRowIncreases(string operationNumber, FormCollection collection)
        {
            var operation = _viewModelMapperHelper.GetOperationData(operationNumber);
            var IsValid = false;
            if (operation.BasicData != null)
            {
                IsValid = _checkOperationTypeService.IsEditableOpr(operation.Attributes.OperationId).Permission ||
                    _checkOperationTypeService.IsIgrOperation(operation.BasicData.OperationType).Permission;
            }

            ViewBag.OperationTypeValid = IsValid;
            ViewBag.clase = collection.Get("clase");
            var data = collection.Get("data");
            ViewBag.data = data.Substring(0, data.Length - 1).Split(',')
                .Select(p => new SelectListItem { Text = p, Value = p }).ToList();

            return PartialView(
                "Partials/FinancialDataExecution/DateTables/FinancialDataIncreaseRow");
        }

        public virtual JsonResult AmountUS()
        {
            return Json(string.Empty);
        }

        public virtual ActionResult ButtonsIncrease(bool isSent)
        {
            ViewBag.SendIncrease = isSent;
            return PartialView(
                "Partials/FinancialDataExecution/DateTables/FinancialDataIncreaseButtons");
        }

        #endregion

        #region FinancialDataPreparation

        public virtual ActionResult FinancialDataPreparation(string operationNumber)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);

             var modelFinal = GetFinancialDataPreparation(operationNumber);

            if (!modelFinal.HasAny())
            {
                return View(modelFinal);
            }

            var model = modelFinal.Single(x => x.IsCurrent);

            foreach (var mo in modelFinal)
            {
                mo.Permissions = SetPermissionPrepFinancialData();

                mo.FinancingTypeList = _viewModelMapperHelper
                    .GetFinancingCodeTypeList(model.OperationTypeId,
                    operationNumber,
                    _attributesEngineService);

                bool gsmPermission = IDBContext.Current.HasPermission(Permission.GCM_WRITE);

                mo.FundList = _viewModelMapperHelper
                    .GetFundList(true, model.OperationTypeId, model.CountryId);
                mo.ExecutingAgenciesList = _viewModelMapperHelper
                    .GetExecutingAgency(operationNumber);
                mo.FundTDB = _viewModelMapperHelper
                    .FundTBD(gsmPermission, model.OperationTypeId, model.CountryId);
                mo.IsFundTBD = model.ExpectedIDB.Any(o => o.IsTbdFund);
                mo.SerializedViewModel = PageSerializationHelper
                    .SerializeObject(model);
                mo.SerializedExpectedViewModel = PageSerializationHelper
                    .SerializeObject(model.ExpectedIDB);
            }

            ViewBag.KindPermission = model.IsInKind;

            return View(modelFinal);
        }

        public virtual ActionResult FinancialDataPreparationReload(
            string operationNumber, int? activityPlanId)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);

            var fullModel = GetFinancialDataPreparation(operationNumber);

            if (!fullModel.HasAny())
            {
                return View(fullModel);
            }

            var targetModel = fullModel.Single(x => x.ActivityPlanId == activityPlanId);

            targetModel.Permissions = SetPermissionPrepFinancialData();

            targetModel.FinancingTypeList = _viewModelMapperHelper
                .GetFinancingCodeTypeList(targetModel.OperationTypeId,
                    operationNumber,
                    _attributesEngineService);

            var gsmPermission = IDBContext.Current.HasPermission(Permission.GCM_WRITE);

            targetModel.FundList = _viewModelMapperHelper
                .GetFundList(gsmPermission,
                    targetModel.OperationTypeId,
                    targetModel.CountryId);
            targetModel.ExecutingAgenciesList = _viewModelMapperHelper
                .GetExecutingAgency(operationNumber);
            targetModel.FundTDB = _viewModelMapperHelper
                .FundTBD(gsmPermission,
                    targetModel.OperationTypeId,
                    targetModel.CountryId);
            targetModel.IsFundTBD = targetModel
                .ExpectedIDB
                .Any(o => o.IsTbdFund);

            targetModel.SerializedViewModel = PageSerializationHelper.SerializeObject(targetModel);
            ViewBag.KindPermission = targetModel.IsInKind;

            targetModel.SerializedExpectedViewModel = PageSerializationHelper
                .SerializeObject(targetModel.ExpectedIDB);

            return PartialView(
                "Partials/FinancialDataPreparation/FinancialDataPreparationContent",
                targetModel);
        }

        public virtual ActionResult GetRowExpectedIDB(int operationType,
            int country,
            string operationNumber,
            int attribute = 0,
            bool isPsgOperation = false,
            bool isFromPreparationFinancialData = false,
            bool isMifOpuOperation = false)
        {
            var model = new ExpectedIDBViewModel();
            var isFromCreation = !isFromPreparationFinancialData;
            var operTypeCode = _convergenceMasterDataRepository
                .GetOne(x => x.ConvergenceMasterDataId.Equals(operationType)).Code;

            model.FinancingTypeList = _viewModelMapperHelper.GetFinancingCodeTypeList(
                operationType, operationNumber, _attributesEngineService, isFromCreation);

            model.TipoTabla = Request["tp"];

            bool gsmPermission = IDBContext.Current.HasPermission(Permission.GCM_WRITE);

            model.FundList = _viewModelMapperHelper.GetFundList(
                gsmPermission, operationType, country, isFromCreation, isMifOpuOperation);

            model.FundTDB = _viewModelMapperHelper.FundTBD(true, operationType, country);
            model.ExecutingAgencies = _viewModelMapperHelper.GetExecutingAgency(operationNumber);

            var psgInformation = _viewModelMapperHelper.GetRowExpectedIDBViewModel(
                operationNumber, isPsgOperation, gsmPermission, operationType, country);
            model.IsPsgOperation = psgInformation.IsPsgOperation;
            model.AvailablePsgDonors = psgInformation.AvailablePsgDonors;
            model.IsFromPreparationFinancialData = isFromPreparationFinancialData;

            return PartialView("Partials/FinancialDataPreparation/DataTables/RowExpectedIDB", model);
        }

        public virtual ActionResult GetRowsBld(string operationNumber,
            decimal amount,
            int countryId = 0,
            string name = "")
        {
            ViewBag.tipoTabla = Request["tp"];
            var partialName = "Partials/FinancialDataPreparation/DataTables/RowTemplateBLD";
            var result = _opusCalculationService.CalculatePercentExpectedIdb(amount,
                operationNumber,
                countryId);

            var approvalDate = !string.IsNullOrEmpty(operationNumber) ?
                    CMBusiness.Get(operationNumber).Context.ActivityPlanItems
                        .Where(o => o.Code == OPUSGlobalValues.APPR_CODE) :
                    null;

            var fundOperationYear =
                approvalDate != null && approvalDate.Any() ?
                    approvalDate.FirstOrDefault().StartDate != null ?
                        approvalDate.FirstOrDefault().StartDate.Value :
                        DateTime.Now :
                    DateTime.Now;
            var model =
                new List<FinancialDataPreparationoBLDModels>
                {
                    FSOorCOC(fundOperationYear, amount, result.FSOValue, result.COCValue),
                    new FinancialDataPreparationoBLDModels
                    {
                        Amount = amount * result.ORCValue,
                        Nombre = FundCode.ORC
                    }
                };

            if (name == OPUSGlobalValues.CREATION)
            {
                partialName = "Partials/CreationForm/DataTables/RowTemplateBLD";
            }

            return PartialView(partialName, model);
        }

        public virtual ActionResult GetRowCounterPartFinancing()
        {
            return PartialView(
                "Partials/FinancialDataPreparation/DataTables/RowCounterPartFinancing");
        }

        public virtual ActionResult GetRowComplementaryResource()
        {
            return PartialView(
                "Partials/FinancialDataPreparation/DataTables/RowComplementaryResource");
        }

        public virtual JsonResult CheckIsFundTbd(
            string fundCode, decimal amount, string operationType)
        {
            var isBld =
                fundCode == FundCode.TBD &&
                    (operationType == OperationType.LON ||
                        (operationType == OperationType.IGR &&
                            amount >= ClauseBusinessLogicCalculator.CURRENT_AMOUNT_THRESOLD));

            return Json(isBld, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CheckBld(string fundCode)
        {
            var isBld = fundCode == FundCode.BLD;
            return Json(isBld, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ExchangeRate(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var clientFieldData = jsonDataRequest.ClientFieldData;

            DateTime fromDate = DateTime
                .Parse(clientFieldData.Where(o => o.Name.Equals("fromDate")).First().Value);
            DateTime ToDate = DateTime
                .Parse(clientFieldData.Where(o => o.Name.Equals("toDate")).First().Value);
            string currency = clientFieldData
                .Where(o => o.Name.Equals("currency")).First().Value;
            decimal amount = decimal
                .Parse(clientFieldData.Where(o => o.Name.Equals("amount")).First().Value);

            var result = _financialDataExecutionService
                .ExangeRateValidation(fromDate, ToDate, amount, currency);

            if (!result.IsValid)
            {
                return Json(new { result.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ChkFundAvailabilityConvertoUSD(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var clientFieldData = jsonDataRequest.ClientFieldData;
            var fundAmountList = clientFieldData.Where(o => o.Name.Equals("Amount")).ToList();
            var sapCodeList = clientFieldData.Where(o => o.Name.Equals("SapCode")).ToList();
            var ocSpecialProgramList = clientFieldData
                .Where(o => o.Name.Equals("OCSpecialProgram")).ToList();
            var efectiveDate = clientFieldData.Where(o => o.Name.Equals("EfectiveDate")).ToList();
            var currency = clientFieldData.Where(o => o.Name.Equals("Currency")).ToList();
            var fundIdList = clientFieldData.Where(o => o.Name.Equals("FundID")).ToList();

            List<RequestCheckFundAvailability> chkList = (
                from amount in fundAmountList
                join sapCode in sapCodeList on fundAmountList.IndexOf(amount)
                    equals sapCodeList.IndexOf(sapCode)
                join specialProgram in ocSpecialProgramList on fundAmountList.IndexOf(amount)
                    equals ocSpecialProgramList.IndexOf(specialProgram)
                join fundId in fundIdList on fundAmountList.IndexOf(amount)
                    equals fundIdList.IndexOf(fundId)
                select new RequestCheckFundAvailability
                {
                    SapCode = sapCode.Value != "0" ? sapCode.Value : string.Empty,
                    OCSpecialProgram = specialProgram.Value != "0" ?
                        specialProgram.Value : string.Empty,
                    IsOCSpecialProgram = specialProgram.Value != "0" &&
                        !string.IsNullOrEmpty(specialProgram.Value) ? true : false,
                    Amount = decimal.Parse(amount.Value),
                    FundId = fundId.Value
                }).ToList();

            List<FundAvailabilityViewModel> response = new List<FundAvailabilityViewModel>();

            chkList = chkList.GroupBy(x => x.FundId).Select(y => new RequestCheckFundAvailability
            {
                SapCode = y.First().SapCode,
                OCSpecialProgram = y.First().SapCode,
                IsOCSpecialProgram = y.First().IsOCSpecialProgram,
                Amount = decimal
                    .Parse(y.Sum(z => z.Amount).ToString(CultureInfo.InvariantCulture)),
                FundId = y.First().FundId
            }).ToList();

            foreach (var chkItem in chkList)
            {
                var chk = _checkFundAvailabilityService.CheckFundAvailability(chkItem);
                if (chk.IsValid)
                {
                    response.Add(chk.CheckFundAvailabilityViewModel);
                }
                else
                {
                    return Json(new
                    {
                        chk.ErrorMessage
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ChkFundAvailability(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var clientFieldData = jsonDataRequest.ClientFieldData;
            var fundAmountList = clientFieldData.Where(o => o.Name.Equals("Amount")).ToList();
            var sapCodeList = clientFieldData.Where(o => o.Name.Equals("SapCode")).ToList();
            var ocSpecialProgramList = clientFieldData.Where(o =>
                o.Name.Equals("OCSpecialProgram")).ToList();
            var fundIdList = clientFieldData.Where(o => o.Name.Equals("FundID")).ToList();

            List<RequestCheckFundAvailability> chkList = (
                from amount in fundAmountList
                join sapCode in sapCodeList
                    on fundAmountList.IndexOf(amount) equals sapCodeList.IndexOf(sapCode)
                join specialProgram in ocSpecialProgramList
                    on fundAmountList.IndexOf(amount)
                    equals ocSpecialProgramList.IndexOf(specialProgram)
                join fundId in fundIdList on fundAmountList.IndexOf(amount)
                    equals fundIdList.IndexOf(fundId)
                select new RequestCheckFundAvailability
                {
                    SapCode = sapCode.Value != "0" ? sapCode.Value : string.Empty,
                    OCSpecialProgram = specialProgram.Value != "0" ?
                        specialProgram.Value : string.Empty,
                    IsOCSpecialProgram = specialProgram.Value != "0" &&
                        !string.IsNullOrEmpty(specialProgram.Value) ? true : false,
                    Amount = decimal.Parse(amount.Value),
                    FundId = fundId.Value
                })
                .ToList();

            List<FundAvailabilityViewModel> response = new List<FundAvailabilityViewModel>();

            foreach (var chkItem in chkList)
            {
                var responseChk = _checkFundAvailabilityService.CheckFundAvailability(chkItem);

                if (responseChk.IsValid)
                {
                    response.Add(responseChk.CheckFundAvailabilityViewModel);
                }
                else
                {
                    return Json(new
                    {
                        responseChk.ErrorMessage
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual PartialViewResult RequestIncreaseModal(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetRequestIncreaseData(false, operationNumber);

            return PartialView(
                "Partials/FinancialDataPreparation/DataTables/RequestIncreaseModal",
                model.RequestIncreaseData);
        }

        public virtual JsonResult GetCurrency(int fundId)
        {
            var response = _viewModelMapperHelper.GetCurrency(fundId);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetUsEquivalentAmount(decimal amount, string currencyCode)
        {
            var actualDate = DateTime.Now.Date;
            var response = _exchangeRateService
                .GetExchangedRateAmount(actualDate, actualDate, amount, currencyCode);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetFinancingTypes(string operationNumber, int fundId)
        {
            var model = _viewModelMapperHelper.GetFinancingTypes(operationNumber, fundId);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = true,
                    FinancingTypes = model
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual PartialViewResult GetExpectedIDB(int fundOperationId)
        {
            var response = _viewModelMapperHelper.GetExpectedIDB(fundOperationId);

            return PartialView("Partials/FinancialDataPreparation/DataTables/RowExpectedIDB",
                response.ExpectedIDB);
        }

        public virtual JsonResult GetFunds(string operationNumber, bool newFunds, bool isRequestIncrease)
        {
            var response = _viewModelMapperHelper
                .GetFunds(operationNumber, newFunds, isRequestIncrease);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual PartialViewResult ChangeRequestIncreaseModal(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetChangeRequestData(false, operationNumber);

            return PartialView(
                "Partials/FinancialDataPreparation/DataTables/RequestIncreaseModal",
                model.RequestIncreaseData);
        }

        public virtual JsonResult GetExecutingAgenciesValidation(string operationNumber,
            string executingAgenciesIds)
        {
            var response = _viewModelMapperHelper
                .GetExecutingAgenciesValidation(operationNumber, executingAgenciesIds);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        #endregion

        #region Institutions

        public virtual ActionResult InstitutionsWorkflow(
            string operationNumber, TaskDataModel model)
        {
            var validatorsResponse = _k2DataService.GetValidators(model.Code, null);
            var commentsResponse = _k2DataService.GetComments(model.TaskId);
            int institutionId = Convert.ToInt32(model.Parameters["Institution_ID"]);

            string institutionStatus = _viewInstitutionModelMapperHelper
                .GetInstitutionStatus(institutionId);

            if (institutionStatus == null)
            {
                var errorModel = new InstitutionsWorkflowViewModels
                {
                    ErrorMessage = Localization.GetText("Institution.Deleted")
                };

                ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(errorModel);
                return View(errorModel);
            }

            var modelTaskView = new InstitutionsWorkflowViewModels
            {
                institutionId = Convert.ToInt32(model.Parameters["Institution_ID"]),
                institutionStatus = institutionStatus,
                UserComments = commentsResponse.IsValid ? commentsResponse.Comments : null,
                Instructions = Localization.GetText("OP.IN.K2.ApproveTaskWorkflow.Instructions"),
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
                Validators = validatorsResponse.Validators,
                Documents = _viewInstitutionModelMapperHelper
                    .GetWorkFlowInstitutionDocuments(institutionId)
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);
            return View(modelTaskView);
        }

        #endregion

        #region Mock Methods

        public virtual JsonResult ChangeRol(string operationNumber, ActionEnum rol)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>()
                {
                    RoleEnum.PCRProjectTeamLeader
                },
                UserName = IDBContext.Current.UserName
            };

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);

            if (!auth.ActionList.Any(x => x == rol))
            {
                auth.ActionList.Add(rol);
            }

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult RemovePermission()
        {
            var authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = false, ErrorMessage = "No permission data to remove." });
            }

            _cacheStorageService.Remove("AuthorizationInfo");
            authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = true, ErrorMessage = "Permission data removed correctly." });
            }

            return Json(new { IsValid = false, ErrorMessage = "Permission data could not be removed." });
        }
        #endregion

        #region Examples

        public virtual JsonResult GetParentDropdown()
        {
            var listDropDown = new List<SelectListItem>();
            for (int i = 0; i < 10; i++)
            {
                listDropDown.Add(
                    new SelectListItem { Text = string.Format("Text {0}", i), Value = i.ToString() });
            }

            return Json(listDropDown, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetChildDropdown(string parentId, FormCollection form)
        {
            var listDropDown = new List<SelectListItem>();
            for (int i = 0; i < 10; i++)
            {
                listDropDown.Add(
                    new SelectListItem { Text = string.Format("Text {0} {1}", parentId, i), Value = i.ToString() });
            }

            return Json(listDropDown, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public virtual ActionResult GetPublicationCode(int categoryId)
        {
            var result = _deliverableService.GetPublicationTypeCode(categoryId);
            return Json(result);
        }

        public virtual ActionResult ViewPage1()
        {
            return View();
        }

        public virtual ActionResult GetPartialView(
            string operationNumber,
            string partial,
            object model = null,
            int opType = 0,
            int country = 0,
            int facilityTypeAttr = 0,
            int lendingTypeAttr = 0,
            int instrumentAttr = 0,
            int lonModalityAttr = 0,
            int sgInstrumentAttr = 0,
            int ddoAttr = 0,
            int conModalityAttr = 0,
            int financingContainer = 0,
            bool hasMecResponsible = false,
            int tcTaxonomyAttr = 0,
            int suportTypeAttr = 0,
            bool isOperationData = false,
            int nsgInstrument = 0,
            ClientFieldData[] clientFieldData = null)
        {
            if (partial.Equals(OPUSGlobalValues.NEW_RELATED_OPERATION_PARTIAL))
            {
                var xml = clientFieldData.ConvertToClienDataXML();

                var opOrganizationlUnit = 0;

                opOrganizationlUnit = GetClientDataForOrganizationalUnit(
                    clientFieldData,
                    isOperationData,
                    operationNumber);

                country = GetClientDataForCountry(
                    clientFieldData,
                    isOperationData,
                    operationNumber);

                var relationTypeFilter = _crossCreationOperationService.GetRelationshipTypeCodes(
                    operationNumber,
                    opType,
                    xml,
                    isOperationData,
                    country,
                    opOrganizationlUnit);
                ///until findout what is causing sp to return null

                if (relationTypeFilter == null)
                {
                    relationTypeFilter = new MW.Application.OPUSModule.Messages.CrossCreationOperationService.RelationTypeResponse();
                }

                relationTypeFilter.RelationTypeCodes = _productProfileService.CheckFilterOperationCPD(operationNumber, relationTypeFilter.RelationTypeCodes);

                var relationshipTypes = _catalogService.GetListMasterData(
                    MasterType.RELATIONSHIP_TYPE,
                    true,
                    includeByCode: relationTypeFilter.RelationTypeCodes);

                    if ((relationTypeFilter.RelationTypeCodes == null || 
                        !relationTypeFilter.RelationTypeCodes.Contains(RelationTypeCode.OPS)) &&
                        relationshipTypes.HasAny(rt => rt.Text.StartsWith(RelationTypeCode.OPS)))
                    {
                        var item = relationshipTypes.First(
                            rt => rt.Text.StartsWith(RelationTypeCode.OPS));

                        relationshipTypes.Remove(item);
                    }

                    ViewBag.RelationshipType = ControlRelationshipTypes(
                    clientFieldData,
                    isOperationData,
                    relationshipTypes,
                    operationNumber);

                ViewBag.RelationshipRole = new List<SelectListItem>();
            }

            if (partial.Equals(OPUSGlobalValues.NEW_INSTITUTION_PARTIAL))
            {
                var institutionRolesToInclude = new List<string>();
                var institutionRolesToExclude = new List<string>();
                var operTypeResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(opType);
                var operTypeCode = opType != 0 && operTypeResponse.IsValid ?
                    operTypeResponse.Code : string.Empty;
                var operationTypes = OperationTypeHelper.GetOperationTypes(operationNumber);
                bool isTcpOper =
                    operationTypes.Contains(OperationType.TCP) || operTypeCode == OperationType.TCP;
                bool isConOper =
                    operationTypes.Contains(OperationType.CON) || operTypeCode == OperationType.CON;

                if (isTcpOper)
                    institutionRolesToInclude.Add(InstitutionRoleCode.INST_EX_AGENCY);

                if (!isConOper)
                    institutionRolesToExclude.Add(InstitutionRoleCode.INST_CRED_LINE_EX_AGENCY);

                ViewBag.SelectedRole = isTcpOper ?
                    _catalogService.GetConvergenceMasterDataIdByCode(
                        InstitutionRoleCode.INST_EX_AGENCY,
                        MasterType.INSTITUTION_ROLE).Id.ToString() :
                    string.Empty;
                ViewBag.Country = _crossCreationOperationService.GetCountryAllByCountry(country).Value;
                ViewBag.AssociatedInstitutionsRoleList = _catalogService.GetListMasterData(
                    type: MasterType.INSTITUTION_ROLE,
                    includeByCode: institutionRolesToInclude,
                    excludeByCode: institutionRolesToExclude);
                ViewBag.AssociatedInstitutionCountryCodeList = _catalogService.GetListMasterData(
                    MasterType.COUNTRY_ASSOCIATED);
                ViewBag.AssociatedInstitutionType = _catalogService.GetListMasterData(
                    MasterType.INSTITUTION_TYPE);
                ViewBag.AssociatedInstitutionsList = _viewModelMapperHelper.GetInstitutionList();
                ViewBag.InstitutionRoleExecutingAgencyId =
                    _catalogService.GetConvergenceMasterDataIdByCode(
                        InstitutionRoleCode.INST_EX_AGENCY, MasterType.INSTITUTION_ROLE).Id;
            }

            if (model != null)
            {
                return PartialView(partial);
            }

            return PartialView(partial, model);
        }

        public virtual ActionResult DeleteRelatedOperation(string operationNumber, int rowId, int relationship)
        {
            var excludeRelationshipType = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE, true, excludeRelationshipType);
            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE, excludeByCode: excludeRelationshipRole);

            var viewModel = SetDeletedRelationsViewModel(rowId, relationship);

            viewModel.SerializedModel = PageSerializationHelper.SerializeObject(viewModel);

            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);

            if (operation != null)
            {
                var cpdMD = _convergenceMasterDataRepository.GetOne(o => o.Code == AttributeValue.CPD && o.ConvergenceMasterType.Type == AttributeCode.CON_MODALITY.ToUpper());
                bool isCON = OperationTypeHelper.GetOperationTypes(operation.OperationId).Contains(OperationType.CON);
                bool isCPD = operation.FormAttributeOperation.Any(x => x.ListValueID == cpdMD.ConvergenceMasterDataId);

                if (isCON && isCPD)
                {
                    CacheDataToCPD(viewModel);

                    return RedirectToAction("GetRowRelatedOperation", "ProductProfile", new { Area = "CPD" });
                }
            }

            return PartialView("_TableRelations", viewModel);
        }

        public virtual ActionResult RedirectToClimateChangeParcialEdit(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetClimateChange(operationNumber);
            var CCSEWrite = false;

            if (model.ClimateChange != null && (model.ClimateChange.ClimateChangePermission.CCSEWritePermission
                || model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission))
            {
                CCSEWrite = ClimateChangeControl(operationNumber,
                    model.ClimateChange.ClimateChangePermission.CCSEWritePermission,
                    model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission);

                model.ClimateChange.ClimateChangePermission.CCSEWrite = CCSEWrite;
            }

            return PartialView("Partials/OperationData/ClimateChangeParcialEdit", model.ClimateChange);
        }

        public virtual ActionResult RedirectToClimateChangeParcialRead(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetClimateChange(operationNumber);
            var CCSEWrite = false;

            if (model.ClimateChange != null && (model.ClimateChange.ClimateChangePermission.CCSEWritePermission
                || model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission))
            {
                CCSEWrite = ClimateChangeControl(operationNumber,
                    model.ClimateChange.ClimateChangePermission.CCSEWritePermission,
                    model.ClimateChange.ClimateChangePermission.ConvergenceReadPermission);

                model.ClimateChange.ClimateChangePermission.CCSEWrite = CCSEWrite;
            }

            return PartialView("Partials/OperationData/ClimateChangeParcialRead", model.ClimateChange);
        }

        public virtual ActionResult GetRowClimateChange(string operationNumber)
        {
            var model = _viewModelMapperHelper.GetClimateChangeNewRow(operationNumber);
            return PartialView("Partials/OperationData/DataTables/RowClimateChange", model.ClimateChangeRow);
        }

        public virtual ActionResult SaveClimate(ClimateDataViewModel modelData, List<ClimateTableViewModel> modelTable, string operationNumber)
        {
            var response = _viewModelMapperHelper.CreateOrUpdateCCES(modelData, modelTable, operationNumber);
            return Json(response);
        }

        public virtual ActionResult RemoveCCES(int rowId)
        {
            var response = _viewModelMapperHelper.RemoveCCES(rowId);
            return Json(response);
        }

        public virtual bool ClimateChangeControl(string operationNumber, bool cCSEWritePermission, bool convergenceReadPermission)
        {
            var valid = false;

            var permissionList = _authorizationManager.GetRoles(IDBContext.Current.UserLoginName, operationNumber);
            var IsESGEspecialistGlobalPermission = permissionList.Any(o => o.Contains(Role.ESG_SPECIALIST_GLOBAL));
            var IsESGAdminPermission = permissionList.Any(o => o.Contains(Role.ESG_ADMIN));
            var IsESGChiefPermission = permissionList.Any(o => o.Contains(Role.ESG_CHIEF));
            var IsESGGropuLeaderPermission = permissionList.Any(o => o.Contains(Role.ESG_OPERATIONAL_GROUP_LEADER));
            var IsCCESEspecialistPermission = permissionList.Any(o => o.Contains(Role.CLIMATE_CHANGE_SPECIALIST));

            if ((IsESGEspecialistGlobalPermission || IsESGAdminPermission || IsESGChiefPermission || IsESGGropuLeaderPermission
                || IsCCESEspecialistPermission) &&
                (cCSEWritePermission || convergenceReadPermission))
            {
                valid = true;
            }

            return valid;
        }

        public virtual JsonResult LoadDropdownChilds(int parentId)
        {
            var filteredClimateChangeList = _convergenceMasterDataRepository.GetByCriteria(x => x.ParentConvergenceMasterDataId == parentId).ToList();
            var none = filteredClimateChangeList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            filteredClimateChangeList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);
            var finalList = filteredClimateChangeList.Select(o => new SelectListItem
            {
                Text = o.GetNameLanguage(Localization.CurrentLanguage),
                Value = o.ConvergenceMasterDataId.ToString()
            }).OrderBy(x => x.Text).ToList();
            if (none != null)
            {
                finalList.Add(new SelectListItem
                {
                    Text = none.GetNameLanguage(Localization.CurrentLanguage),
                    Value = none.ConvergenceMasterDataId.ToString()
                });
            }

            return Json(finalList, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetAttributeBusinessRules(
            int? operationTypeId, int containerModalityId, int facilityTypeId)
        {
            var response = _operationDataBusinessRulesService.FacilityIdentifierCode(
                operationTypeId, containerModalityId, facilityTypeId);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CheckFacilityTypeRule(int? facilityTypeId)
        {
            var response = _operationDataBusinessRulesService
                .FacilitityTypeRules(facilityTypeId ?? 0);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CheckRmIndicatorRelationsToSAClassifications(
            string operationNumber,
            ResultsMatrixElementTypesEnum elementType,
            int rmElementId)
        {
            var saveUrl = Url.Action(
                "UpdateRMIndicatorRelationsToSAClassifications", "Save", new { area = "OPUS" });

            var response = _operationDataService.CheckRmIndicatorRelationsToSAClassifications(
                operationNumber, elementType, rmElementId, saveUrl);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CheckIndicatorSAClassificationDelete(
            StrategicAlignmentIndicatorsViewModel saIndicatorsViewModel)
        {
            var response = _operationDataService
                .CheckRMIndicatorBeforeDeleteSAClassifications(saIndicatorsViewModel);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetInstitutionsForEFCOperation()
        {
            return Json(
                _crossCreationOperationService.GetInstitutionsForEFCOperation(),
                JsonRequestBehavior.AllowGet);
        }

        internal IList<string> GetExcludedTeamRoles(
            bool existTeamLeaderNotEditable,
            bool existOperationAnalistNotEditable,
            bool excludeSpdSpecialist,
            bool excludeVpcRoles)
        {
            var excludeValues = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER,
                MemberRoleCode.PCR_AUTOR,
                MemberRoleCode.ESG_REVIEWER,
                MemberRoleCode.CREATOR_ROLE
            };

            if (excludeVpcRoles)
            {
                excludeValues.Add(MemberRoleCode.VPC_COUNTRY_ECONOMIST);
                excludeValues.Add(MemberRoleCode.VPC_TEAM_MEMBER);
            }

            if (existTeamLeaderNotEditable)
            {
                excludeValues.Add(MemberRoleCode.TEAM_LEADER);
                excludeValues.Add(MemberRoleCode.ALTERNATIVE_TEAM_LEADER);
            }

            if (existOperationAnalistNotEditable)
            {
                excludeValues.Add(MemberRoleCode.OPERATIONAL_ANALIST);
            }

            if (excludeSpdSpecialist)
            {
                excludeValues.Add(MemberRoleCode.SPD_SPECIALIST);
            }

            return excludeValues;
        }

        internal int GetValueClientDataGeneric(ClientFieldData[] clientFieldData, string valueName)
        {
            int temp = 0;

            var temporal = string.Concat(clientFieldData
                .Where(x => x.Name == valueName).DefaultIfEmpty(new ClientFieldData())
                .Select(y => y.Value).DefaultIfEmpty());

            if (int.TryParse(temporal, out temp))
                return temp;

            return temp;
        }

        internal IList<int> GetListValueClientDataGeneric(
            ClientFieldData[] clientFieldData,
            string valueName)
        {
            var clientData = clientFieldData
                .Where(x => x.Name == valueName).DefaultIfEmpty(new ClientFieldData())
                .Select(y => y.Value)
                .DefaultIfEmpty();

            var data = string.Join(",", clientData);

            string[] stringSeparators = new string[] { "," };
            string[] result = data.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            return result.Select(int.Parse).ToArray();
        }

        internal IList<string> GetIncludedTeamRolesEsg(bool includeEsgReviewer)
        {
            var includeValuesEsg = new List<string>
            {
                MemberRoleCode.ESG_TEAM_MEMBER,
                MemberRoleCode.ESG_PRIMARY_TEAM_MEMBER
            };

            if (includeEsgReviewer)
                includeValuesEsg.Add(MemberRoleCode.ESG_REVIEWER);

            return includeValuesEsg;
        }

        internal IList<string> GetExcludedTeamRolesEsg(bool isPrg, bool excludeSpdSpecialist)
        {
            var excludeValuesEsg = new List<string>
            {
                MemberRoleCode.CREATOR_ROLE,
                MemberRoleCode.ESG_REVIEWER
            };

            if (!isPrg)
            {
                excludeValuesEsg.Add(MemberRoleCode.VPC_COUNTRY_ECONOMIST);
                excludeValuesEsg.Add(MemberRoleCode.VPC_TEAM_MEMBER);
            }

            if (excludeSpdSpecialist)
            {
                excludeValuesEsg.Add(MemberRoleCode.SPD_SPECIALIST);
            }

            return excludeValuesEsg;
        }

        internal IList<string> GetIncludedTeamRolesSpdPcr()
        {
            return new List<string> { MemberRoleCode.PCR_AUTOR };
        }

        internal IList<string> GetExcludedTeamRolesSpdPcr(bool isPrg, bool excludeSpdSpecialist)
        {
            var excludeValuesSpdPcr = new List<string> { MemberRoleCode.ESG_REVIEWER };

            if (!isPrg)
            {
                excludeValuesSpdPcr.Add(MemberRoleCode.VPC_COUNTRY_ECONOMIST);
                excludeValuesSpdPcr.Add(MemberRoleCode.VPC_TEAM_MEMBER);
            }

            if (excludeSpdSpecialist)
            {
                excludeValuesSpdPcr.Add(MemberRoleCode.SPD_SPECIALIST);
            }

            return excludeValuesSpdPcr;
        }

        internal decimal SumExpectedIDBAmounts(IEnumerable<ExpectedIDBViewModel> amounts)
        {
            if (amounts == null)
                return decimal.Zero;

            return amounts.Select(a => a.Amount).DefaultIfEmpty(decimal.Zero).Sum();
        }

        internal bool IsCategoryMandatory(
            int operationType,
            FormDataViewModel attributesData,
            decimal totalAmount)
        {
            var attributes = new Dictionary<string, object>();

            if (attributesData != null && attributesData.Fields.HasAny())
                attributes = attributesData.Fields.ToDictionary(f => f.Name, f => f.InitialValue);

            var responseCategory = _checkRequiredService.CheckRequiredCategory(
                operationType,
                attributes,
                totalAmount);

            if (!responseCategory.IsValid)
            {
                var exc = new ApplicationException(responseCategory.ErrorMessage);

                Logger.GetLogger()
                    .WriteError("OPUSViewController-DataCreationForm", exc.Message, exc);

                throw exc;
            }

            return responseCategory.NullableValue.HasValue && responseCategory.NullableValue.Value;
        }

        decimal CalculateExpectedIDBAmounts(int operationId)
        {
            var calculator = new AccountingCalculator(operationId);

            return calculator.CalculateFundAmount();
        }

        private DeliverableViewModel GetDeliverableViewModel(string operationNumber)
        {
            GlobalCommonLogic.SetLastOperation(operationNumber);
            var model = _viewModelMapperHelper.GetDeliverable(operationNumber).Deliverable;
            SetPermissionDeliverable();
            var lowestYear = GetDeliverableLowestYear(model);

            GetListsDeliverable(
                model.CountryCode,
                model.CountryId,
                model.IsRegional,
                lowestYear,
                model.DeliverableData);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            model.HasPermissionDeliverableWrite = IDBContext.Current
                .HasPermission(Permission.DELIVERABLES_WRITE);

            return model;
        }

        private int GetDeliverableLowestYear(DeliverableViewModel model)
        {
            return model.DeliverableData.HasAny() ?
                model.DeliverableData.Min(x => x.YearPlanned - 1) :
                DateTime.UtcNow.Year - 1;
        }

        private bool IsEditableFinancing(int operationType)
        {
            if (!_checkOperationTypeService.IsLoanOperation(operationType).Permission &&
                ViewBag.CreationWritePermission)
            {
                return true;
            }

            if (!_checkOperationTypeService.IsTcOperation(operationType).Permission &&
                ViewBag.GCMWritePermission)
            {
                return true;
            }

            var isUserCreator = (bool)ViewBag.Creation;

            if (ViewBag.CreationWritePermission || isUserCreator)
            {
                return true;
            }

            return false;
        }

        private ApprovalOperationViewModel GetApprovalOperation(
            string operationNumber, ReformulationViewModel reformulation)
        {
            var response = _approvalOperationService.GetApprovalOperations(operationNumber, reformulation);
            response.ApprovalOperation.IsGCMAdmin = IDBContext.Current.HasRole(Role.GCM_ADMIN);
            response.ApprovalOperation.ApprovalGcmWrite =
                IDBContext.Current.HasPermission("GCM Write");
            response.ApprovalOperation.ApprovalTLWrite =
                IDBContext.Current.HasPermission("Approval TL write");
            response.ApprovalOperation.ApprovalWrite =
                IDBContext.Current.HasPermission("Approval Write");
            response.ApprovalOperation.Documents.ApprovalTLWrite =
                 IDBContext.Current.HasPermission("Approval TL write");
            response.ApprovalOperation.Documents.ApprovalWrite =
               IDBContext.Current.HasPermission("Approval Write");

            var viewModel = response.ApprovalOperation;

            FillApprovalModelWithReformulation(viewModel, reformulation);

            SetViewBagErrorMessageInvalidResponse(response);

            _viewModelMapperHelper.SetViewBagApprovalOperation();

            GlobalCommonLogic.SetLastOperation(operationNumber);

            return viewModel;
        }

        private OperationViewModel GetContractLevelOperation(
            string operationNumber, string approvalNumber, int? activityPlanId = null)
        {
            var reformulationViewModels = _reformulationService.GetFormulations(operationNumber, false);

            var reformulation = reformulationViewModels.Models
                .First(x => x.ActivityPlanId == activityPlanId);

            var response = _financialDataExecutionService.GetFinancialDataExecution(
                operationNumber, Localization.CurrentLanguage, reformulation);

            if (!response.IsValid)
            {
                return new OperationViewModel();
            }

            return response.FinancialData.ContractLevel.Operations
                .First(a => a.ContractData.ContractId == approvalNumber);
        }

        private void FillApprovalModelWithReformulation(
            ApprovalOperationViewModel viewModel,
            ReformulationViewModel reformulation)
        {
            if (reformulation == null)
            {
                return;
            }

            viewModel.IsCurrent = reformulation.IsCurrent;
            viewModel.Name = reformulation.Name;
            viewModel.ActivityPlanId = reformulation.ActivityPlanId;
            viewModel.ReformulationApprovalDate = reformulation.ReformulationApprovalDate;
        }

        private void GetListsDeliverable(
            string countryCode,
            int countryId,
            bool isRegional,
            int year,
            List<DeliverableRowViewModel> deliverables = null)
        {
            ViewBag.DeliverableList = _catalogService.GetListMasterData(
                MasterType.DELIVERABLE_TYPE);

            List<SelectListItem> countriesCodes = _viewModelMapperHelper.GetCountryRelatedCodeList(
                countryCode, countryId, isRegional);

            if (deliverables != null)
            {
                foreach (var deliverable in deliverables)
                {
                    var masterDatas = _catalogService.GetListMasterDataCollection(
                        type: MasterType.DELIVERABLE_TYPE,
                        hideExpired: true,
                        includeById: null);

                    deliverable.DeliverableCategories = masterDatas;

                    if (!deliverable.DeliverableCategories.Select(x => x.MasterId).ContainsAny(deliverable.Type))
                    {
                        var categoryExpired = _catalogService.GetConvergenceMasterDataById(deliverable.Type);

                        deliverable.DeliverableCategories.Add(categoryExpired.Model.ConvertToMasterDataViewModel());
                    }

                    deliverable.DeliverableCountries = new List<MasterDataViewModel>();

                    if (!countriesCodes.Select(x => x.Value).ContainsAny(deliverable.Country.ToString()))
                    {
                        var countryDeliverable = _catalogService.GetConvergenceMasterDataById(deliverable.Country);

                        deliverable.DeliverableCountries.Add(countryDeliverable.Model.ConvertToMasterDataViewModel());
                    }

                    foreach (var country in countriesCodes)
                    {
                        var countryMasterDataCollection = _catalogService.GetConvergenceMasterDataById(country.Value.ConvertToInt());

                        deliverable.DeliverableCountries.Add(countryMasterDataCollection.Model.ConvertToMasterDataViewModel());
                    }
                }
            }

            ViewBag.CountryCodeList = _viewModelMapperHelper.GetCountryRelatedCodeList(
                countryCode, countryId, isRegional);

            ViewBag.ResponsibleList = _viewModelMapperHelper.GetDivisionList(onlyCode: true);
            ViewBag.DeliverableYearList = _viewModelMapperHelper.GetListOperationYear(year);
            ViewBag.LanguageList = _catalogService.GetListMasterData(
                MasterType.LANGUAGE);
        }

        private ICollection<FinancialDataViewModel> GetFinancialDataExecution(
            string operationNumber)
        {
            var modelFinal = new Collection<FinancialDataViewModel>();
            FinancialDataViewModel viewModel = new FinancialDataViewModel();

            ResponseBase responseBase = new ResponseBase();

            try
            {
                var historicalAccumulated = PMIModelRepository
                    .GetHistoricalAccumulated(operationNumber);

                var reformulationViewModels = _reformulationService
                    .GetFormulations(operationNumber, false);
                foreach (var reformulationModel in reformulationViewModels.Models)
                {
                    var response = BuildFinancialDataResponse(
                        operationNumber, reformulationModel, historicalAccumulated);

                    if (!response.IsValid)
                    {
                        responseBase.IsValid = false;
                        responseBase.ErrorMessage = response.ErrorMessage;
                        break;
                    }

                    modelFinal.Add(response.FinancialData);
                }

                if (!reformulationViewModels.Models.HasAny())
                {
                    var reformulationModel = new ReformulationViewModel { IsCurrent = true };

                    var response = BuildFinancialDataResponse(
                        operationNumber, reformulationModel, historicalAccumulated);

                    if (!response.IsValid)
                    {
                        responseBase.IsValid = false;
                        responseBase.ErrorMessage = response.ErrorMessage;

                        return modelFinal;
                    }

                    modelFinal.Add(response.FinancialData);
                }

                viewModel = modelFinal.SingleOrDefault(x => x.IsCurrent);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "Financial Data Execution - Presentation", "Error processing model" + e.Message, e);
                responseBase.IsValid = false;
                responseBase.ErrorMessage = e.Message;
            }

            SetViewBagErrorMessageInvalidResponse(responseBase);

            if (viewModel.FundIncreases.CurrentFundTotal == null)
            {
                viewModel.FundIncreases.CurrentFundTotal = 0;
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);
            ViewBag.OperationTypeValid = viewModel.FundIncreases.HasPermission;
            GlobalCommonLogic.SetLastOperation(operationNumber);

            return modelFinal;
        }

        private GetFinancialDataExecutionResponse BuildFinancialDataResponse(
            string operationNumber,
            ReformulationViewModel reformulationModel,
            List<HistoricAcumulated> historicalAccumulated)
        {
            var pmiOperation = (PMIOperation)TempData["operationContext"] ??
                PMIModelRepository.LoadOperationContext(
                    operationNumber,
                    false,
                    false,
                    reformulationModel.ActivityPlanId);

            var response = _financialDataExecutionService.GetFinancialDataExecution(
                operationNumber, Localization.CurrentLanguage, reformulationModel);

            response.FinancialData.AccumulateDisbursementsGraphData = GraphicGenerator
                .LoadAccumalteDisbursementsGraphData(
                    historicalAccumulated, pmiOperation, operationNumber, false);

            FillFinancialDataExcutionWithReformulation(
                response.FinancialData, reformulationModel);

            return response;
        }

        private void FillFinancialDataExcutionWithReformulation(
            FinancialDataViewModel financialData, ReformulationViewModel reformulationModel)
        {
            if (reformulationModel == null)
            {
                return;
            }

            financialData.IsCurrent = reformulationModel.IsCurrent;
            financialData.Name = reformulationModel.Name;
            financialData.ReformulationApprovalDate = reformulationModel.ReformulationApprovalDate;
            financialData.ActivityPlanId = reformulationModel.ActivityPlanId;
        }

        private ICollection<FinancialDataPreparationViewModel> GetFinancialDataPreparation(
            string operationNumber)
        {
            var modelFinal = new Collection<FinancialDataPreparationViewModel>();

            var reformulationViewModels = _reformulationService.GetFormulations(operationNumber, true);
            if (!reformulationViewModels.IsValid)
            {
                return modelFinal;
            }

            foreach (var reformulationModel in reformulationViewModels.Models)
            {
                var financialDataResponse = _viewModelMapperHelper
                    .GetFinancialDataPreparation(operationNumber, reformulationModel);
                if (!financialDataResponse.IsValid)
                {
                    break;
                }

                modelFinal.Add(financialDataResponse.FinancialDataPreparation);
            }

            if (!reformulationViewModels.Models.HasAny())
            {
                var reformulation = new ReformulationViewModel { IsCurrent = true };
                var financialDataResponse = _viewModelMapperHelper
                    .GetFinancialDataPreparation(operationNumber, reformulation);

                modelFinal.Add(financialDataResponse.FinancialDataPreparation);
            }

            string[] masterTypeList = new string[]
            {
                MasterType.CATEGORY
            };

            var masterDataListByTypeCode = _catalogService
                .GetMasterDataListByTypeCode(typeCodes: masterTypeList);

            if (!masterDataListByTypeCode.IsValid)
            {
                Logger.GetLogger().WriteMessage(
                    "OPUSViewController - ConvertToCreationFormViewModel",
                    "Fail to get pipeline categories");

                throw new NullReferenceException("Catalog Service - Can't get pipeline categories");
            }

            modelFinal.Single(x => x.IsCurrent).PipelineCategories =
                masterDataListByTypeCode.MasterDataCollection;

            return modelFinal;
        }

        private ApprovalOperationViewModel BuildApprovalOperationViewModel(
            string operationNumber, ReformulationViewModel reformulation)
        {
            var approvalModel = GetApprovalOperation(operationNumber, reformulation);
            var attributes = _viewModelMapperHelper
                .GetAttributesApproval(operationNumber, reformulation);
            approvalModel.AttributesApproval = attributes.AttributesApproval;
            approvalModel.FormAttributes = attributes.FormAttributes;
            return approvalModel;
        }

        private void SetPermisionSubnationalLevel()
        {
            ViewBag.CreationWritePermission = IDBContext.Current.HasPermission(Permission.CREATION_WRITE);
            ViewBag.CreationRegisterOperationPermission = IDBContext.Current.HasPermission(
                Permission.REGISTER_OPERATION_WRITE);
            ViewBag.RelatedOperationsWritePermission = IDBContext.Current.HasPermission(
                Permission.RELATED_OPERATIONS_WRITE);
            ViewBag.AssociatedInstitutionsWritePermission = IDBContext.Current.HasPermission(
                Permission.ASSOCIATED_INSTITUTIONS_WRITE);
            ViewBag.CreationandRegistrationValidationWorkflowPermission = IDBContext.Current.HasPermission(
                Permission.CREATION_AND_REGISTRATION_VALIDATION_WORKFLOW);
            ViewBag.GCMWritePermission = IDBContext.Current.HasPermission(Permission.GCM_WRITE);
            ViewBag.EsgSpecialistAssignmentWritePermission = IDBContext.Current.HasPermission(
                Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE);
            ViewBag.SPDDEMWritePermission = IDBContext.Current.HasPermission(Permission.SPD_DEM_WRITE);
            ViewBag.SPDPCRWritePermission = IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE);
        }

        private void SetPermisionCreationForm(bool isFirstLoad)
        {
            if (isFirstLoad)
            {
                ViewBag.CreationWritePermission = true;
                ViewBag.CreationRegisterOperationPermission = IDBContext.Current.HasPermission(
                    Permission.REGISTER_OPERATION_WRITE);
                ViewBag.RelatedOperationsWritePermission = true;
                ViewBag.AssociatedInstitutionsWritePermission = true;
                ViewBag.CreationandRegistrationValidationWorkflowPermission = true;
                ViewBag.GCMWritePermission = true;
                ViewBag.EsgSpecialistAssignmentWritePermission = true;
                ViewBag.SPDDEMWritePermission = true;
                ViewBag.SPDPCRWritePermission = true;
            }
            else
            {
                ViewBag.CreationWritePermission = IDBContext.Current.HasPermission(Permission.CREATION_WRITE);
                ViewBag.CreationRegisterOperationPermission = IDBContext.Current.HasPermission(
                    Permission.REGISTER_OPERATION_WRITE);
                ViewBag.RelatedOperationsWritePermission = IDBContext.Current.HasPermission(
                    Permission.RELATED_OPERATIONS_WRITE);
                ViewBag.AssociatedInstitutionsWritePermission = IDBContext.Current.HasPermission(
                    Permission.ASSOCIATED_INSTITUTIONS_WRITE);
                ViewBag.CreationandRegistrationValidationWorkflowPermission = IDBContext.Current.HasPermission(
                    Permission.CREATION_AND_REGISTRATION_VALIDATION_WORKFLOW);
                ViewBag.GCMWritePermission = IDBContext.Current.HasPermission(Permission.GCM_WRITE);
                ViewBag.EsgSpecialistAssignmentWritePermission = IDBContext.Current.HasPermission(
                    Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE);
                ViewBag.SPDDEMWritePermission = IDBContext.Current.HasPermission(Permission.SPD_DEM_WRITE);
                ViewBag.SPDPCRWritePermission = IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE);
            }

            ViewBag.SPDDEMResponsibilityWritePermission = IDBContext.Current.HasPermission(Permission.SPD_DEM_RESPONSIBILITY_WRITE);
        }

        private PermissionViewModel SetPermissionPrepFinancialData()
        {
            PermissionViewModel model = new PermissionViewModel();

            model.FinancialWritePermision = IDBContext.Current.HasPermission(
                Permission.FINANCIAL_DATA_WRITE);
            model.IncreaseWritePermission = IDBContext.Current.HasPermission(
                Permission.INCREASES_WRITE);
            model.GCMWritePermission = IDBContext.Current.HasPermission(
                Permission.GCM_WRITE);
            model.LocalCounterpartPermission = IDBContext.Current.HasPermission(
                Permission.LOCAL_COUNTERPART_WRITE);
            model.FinancialDataTLEAWritePermission = IDBContext.Current.HasPermission(
                Permission.FINANCIAL_DATA_TL_EA_WRITE);
            model.FinancialDataORPAdminPermission = IDBContext.Current.HasPermission(
                Permission.FINANCIAL_DATA_ORP_ADMIN_COFIN__WRITE);
            model.PSGWriteFinancialDataPermission = IDBContext.Current.HasPermission(
                Permission.PSG_WRITE_FINANCIAL_DATA);

            return model;
        }

        private void SetPermissionFinancialData()
        {
            ViewBag.FinancialWritePermision = IDBContext.Current.HasPermission(
                Permission.FINANCIAL_DATA_WRITE);
            ViewBag.IncreaseWritePermission = IDBContext.Current.HasPermission(
                Permission.INCREASES_WRITE);
            ViewBag.GCMWritePermission = IDBContext.Current.HasPermission(
                Permission.GCM_WRITE);
            ViewBag.LocalCounterpartPermission = IDBContext.Current.HasPermission(
                Permission.LOCAL_COUNTERPART_WRITE);
        }

        private void SetPermissionDeliverable()
        {
            ViewBag.DeliverablePermission = IDBContext.Current.HasPermission(
                Permission.DELIVERABLES_WRITE);
            ViewBag.PublicationPermission = IDBContext.Current.HasPermission(
                Permission.PUBLICATION_CREATION_WRITE);
        }

        private void SetPermissionOperationData()
        {
            ViewBag.BasicDataPermission =
                IDBContext.Current.HasPermission(Permission.BASIC_DATA_WRITE);

            ViewBag.ResponsabilityDataPermission =
                IDBContext.Current.HasPermission(Permission.RESPONSIBILITY_DATA_WRITE);

            ViewBag.EsgSpecialistAssignmentWritePermission =
                IDBContext.Current.HasPermission(Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE);

            ViewBag.RelatedOperationsWritePermission =
                IDBContext.Current.HasPermission(Permission.RELATED_OPERATIONS_WRITE);

            ViewBag.ClassificationDataPermission =
                IDBContext.Current.HasPermission(Permission.CLASSIFICATION_DATA_WRITE);

            ViewBag.LendingProgramPermission =
                IDBContext.Current.HasPermission(Permission.LENDING_PROGRAM_WRITE);

            ViewBag.SPDDEMWritePermission =
                IDBContext.Current.HasPermission(Permission.SPD_DEM_WRITE);

            ViewBag.SPDPCRWritePermission =
                IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE);

            ViewBag.GCMWritePermission =
                IDBContext.Current.HasPermission(Permission.GCM_WRITE);

            ViewBag.VPCWritePermission =
                IDBContext.Current.HasPermission(Permission.VPC_WRITE_PERMISSION);

            ViewBag.VPSManagerWrite =
                IDBContext.Current.HasPermission(Permission.VPS_MANAGER_WRITE);

            ViewBag.StrategicAlignmentWrite =
                IDBContext.Current.HasPermission(Permission.STRATEGIC_ALIGNMENT_WRITE);

            ViewBag.SPDDEMResponsibilityWritePermission =
                IDBContext.Current.HasPermission(Permission.SPD_DEM_RESPONSIBILITY_WRITE);
        }

        private bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    ViewBag.ErrorMessage = HttpUtility.HtmlEncode(urlDecode.Replace(
                        Environment.NewLine, " "));
                }
            }

            return response.IsValid;
        }

        private void SetViewbagStrategicAlignment(
            string operationNumber,
            IDB.MW.Domain.Entities.ResultsMatrix resultsMatrix,
            StrategicAlignmentViewModel model)
        {
            var impactIndicators = new List<MultiDropDownItem>();
            var impactsResponse = _operationDataService.GetResultMatrixImpacts(
                operationNumber, resultsMatrix, false);

            if (impactsResponse.IsValid)
            {
                impactsResponse.Impacts.ForEach(x =>
                {
                    impactIndicators.Add(new MultiDropDownItem()
                    {
                        Text = x.Text,
                        Value = x.Value
                    });
                });

                ViewBag.ImpactIndicators = impactIndicators;
                ViewBag.ResultMatrixId = resultsMatrix.ResultsMatrixId;
            }

            var outcomeIndicators = new List<MultiDropDownItem>();
            var outcomesResponse = _operationDataService.GetResultMatrixOutcomes(
                operationNumber, resultsMatrix, false);

            if (outcomesResponse.IsValid)
            {
                outcomesResponse.Outcomes.ForEach(x =>
                {
                    outcomeIndicators.Add(new MultiDropDownItem()
                    {
                        Text = x.Text,
                        Value = x.Value
                    });
                });

                ViewBag.OutcomeIndicators = outcomeIndicators;
            }

            var outputs = new List<MultiDropDownItem>();
            var outputsResponse = _operationDataService.GetResultMatrixOutputs(
                operationNumber, resultsMatrix, false);

            if (outputsResponse.IsValid)
            {
                outputsResponse.Outputs.ForEach(x =>
                {
                    outputs.Add(new MultiDropDownItem()
                    {
                        Text = x.Text,
                        Value = x.Value
                    });
                });

                ViewBag.Outputs = outputs;
            }

            ViewBag.StrategicAlignmentPermission = true;

            var objectivesList = new List<MultiDropDownItem>();
            var notAlignedList = new List<SelectListItem>();

            if (!model.IsOperationEswType)
            {
                var objectivesResponse = _operationDataService.GetCountryStrategyObjectivesList(operationNumber);
                if (objectivesResponse.IsValid)
                {
                    objectivesList = objectivesResponse.Objectives.Select(x => new MultiDropDownItem()
                    {
                        Value = x.Value,
                        Text = x.Text
                    }).ToList();
                }

                if (objectivesResponse.HaveResultMatrix)
                {
                    var StrategyObjectivescurrentApprovedType =
                        _catalogService.GetListMasterData(MasterType.STRATEGY_OBJECTIVES_CURRENT_APPROVED);

                    foreach (var data in StrategyObjectivescurrentApprovedType)
                    {
                    var aux = new SelectListItem()
                        {
                            Value = string.Format("SOCA-{0}", data.Value),
                            Text = data.Text
                        };
                        notAlignedList.Add(aux);
                    }
                }
            }

            ViewBag.CountryStrategyObjectivesList = objectivesList;
            ViewBag.CountryStrategyNotAligned = notAlignedList;
        }

        private BasicDataViewModel SetNewRelatedValues(Operation operationParent)
        {
            var actualViewModel = PageSerializationHelper
                .DeserializeObject<BasicDataViewModel>(Request.Form[1]);

            var newRelatedOperation = JsonConvert
                .DeserializeObject<RelatedOperationRowViewModel>(Request.Form[0]);

            if (newRelatedOperation.OperationRelatedId == 0)
            {
                newRelatedOperation = GetDataModelRelationshipItself(newRelatedOperation);
            }

            var opId = _operationRepository
                .GetOne(o => o.OperationNumber == IDBContext.Current.Operation).OperationId;

            newRelatedOperation.RelationshipRole = _convergenceMasterDataRepository
                .GetOne(o => o.ConvergenceMasterDataId == newRelatedOperation.RelationshipRoleId)
                .GetNameLanguage(Localization.CurrentLanguage);

            var operation = _operationRepository
                .GetOne(o => o.OperationNumber == newRelatedOperation.Number);

            var approvals = _fundOperationRepository
                .GetByCriteria(_operationHistoryService.BuildFundOperationExpr(operation))
                .Select(o => o.ApprovalNumber);

            newRelatedOperation.ApprovalNumber = string.Join(",", approvals);

            if (newRelatedOperation.OperationTypeCode == OperationType.TCP)
            {
                var admExpenses = operation.FormAttributeOperation
                    .Where(o => o.FormAttribute.Code == AttributeCode.ADMINISTRATIVE_EXPENSES);

                if (admExpenses != null &&
                    admExpenses.Any(o => (o.ListValue != null &&
                        o.ListValue.Code == AttributeValue.TRUE_ES) ||
                        o.NumberValue == AttributeValue.ADMINISTRATIVE_EXPENSES))
                    newRelatedOperation.Modality = OPUSGlobalValues.MODALITY_ADM_EXPENSES;
                else
                    newRelatedOperation.Modality = OPUSGlobalValues.MODALITY_NOT_ADM_EXPENSES;
            }
            else
            {
                var modalities = operation.FormAttributeOperation
                .Where(o => o.FormAttribute.Code.ToUpper().Contains(MODALITY));

                newRelatedOperation.Modality = modalities.Count() > 0 && modalities.First().ListValue != null ?
                    modalities.First().ListValue.Code : string.Empty;
            }

            newRelatedOperation.DisbursedPercent = 0;

            if (operation.Contracts != null)
            {
                var contract = operation.Contracts.Where(o => o.FinProcesing == TRUE);
                var currentAmount = contract.Sum(o => o.CurrentAprovedAmount);
                var disbursedAmount = currentAmount - contract.Sum(o => o.UndisbursedAmount);

                if (currentAmount != 0)
                {
                    newRelatedOperation.DisbursedPercent =
                       (disbursedAmount / currentAmount) * TO_PERCENT;
                    newRelatedOperation.DisbursedAmount = disbursedAmount;
                    newRelatedOperation.ApprovedAmount = currentAmount;
                }
            }

            newRelatedOperation.RowId = Convert.ToInt32(DateTime.Now.ToString("ddhhmmss"));

            var operationPipeline = operationParent.CPDPipelineOperations.SingleOrDefault(x => x.OperationNumber == newRelatedOperation.Number);
            if (operationPipeline != null)
            {
                newRelatedOperation.CPDAmount = operationPipeline.Amount;
            }

            actualViewModel.Relationships = _operationDataService.GetDraftRelatedOperations(
                actualViewModel.Relationships,
                newRelatedOperation);

            return actualViewModel;
        }

        private BasicDataViewModel SetDeletedRelationsViewModel(int rowId, int relationship)
        {
            var viewModel = PageSerializationHelper
                .DeserializeObject<BasicDataViewModel>(Request.Form[2]);

            var operation = _operationRepository
                .GetOne(o => o.OperationNumber == IDBContext.Current.Operation);

            var related = viewModel.Relationships
                .First(o => o.RelationshipId == relationship);

            var deleted = related.RelatedOperations.First(o => o.RowId == rowId);

            if (operation.OperationId == deleted.OperationRelatedId)
            {
                viewModel.Relationships.Remove(related);
                return viewModel;
            }

            var roleCode = _catalogService
                .GetConvergenceMasterCodeByIdResponse(deleted.RelationshipRoleId).Code;

            if (roleCode.Contains(RelationRoleCode.PARENT))
            {
                viewModel.Relationships.Remove(related);
                return viewModel;
            }

            related.RelatedOperations.Remove(deleted);

            if (related.RelatedOperations.Count() < 2)
            {
                viewModel.Relationships.Remove(related);
                return viewModel;
            }

            var notParent = related.RelatedOperations
                .Where(o => !o.RelationshipRoleCode.ToUpper().Contains(RelationRoleCode.PARENT));

            related.TotalAmmount = notParent.Sum(o => o.TotalAmount);

            var totalDisbursed = notParent.Sum(o => o.DisbursedAmount);
            var approved = notParent.Sum(o => o.ApprovedAmount);

            related.TotalBalance = related.HasParent ?
                related.RelatedOperations.First().TotalAmount - approved :
                DEFAULT_VALUE;

            var index = viewModel.Relationships
                .FindIndex(o => o.RelationshipId == relationship);

            viewModel.Relationships[index] = related;

            viewModel.Relationships[index].TotalRelated--;

            return viewModel;
        }

        void SetSecurityBasicData()
        {
            var pageNames = OPUSGlobalValues.PAGE_NAME_RELATED_OPERATION_MODAL +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_BASIC_DATA_HEADER +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_BASIC_DATA_OPERATION_NAME +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_BASIC_DATA_RELATED_OPERATION;

            ViewBag.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                    IDBContext.Current.Operation,
                    pageNames,
                    IDBContext.Current.Permissions,
                    0,
                    0)
                .ToList();
        }

        void SetSecurityTabClassificationData()
        {
            var pageNames = OPUSGlobalValues.PAGE_NAME_TAB_CLASSIFICATION_DATA;

            ViewBag.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                    IDBContext.Current.Operation,
                    pageNames,
                    IDBContext.Current.Permissions,
                    0,
                    0)
                .ToList();
        }

        void SetSecurityTabResponsibilityData()
        {
            var pageNames = OPUSGlobalValues.PAGE_NAME_TAB_RESP_DATA_ASSO_INSTITUTIONS +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_RESP_DATA_RESPONSIBI_UNIT +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_RESP_DATA_OPERATION_TEAM +
                '|' + OPUSGlobalValues.PAGE_NAME_TAB_RESP_DATA_ASSOC_COUNTRIES;

            ViewBag.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                    IDBContext.Current.Operation,
                    pageNames,
                    IDBContext.Current.Permissions,
                    0,
                    0)
                .ToList();
        }

        private FinancialDataPreparationoBLDModels FSOorCOC(
            DateTime fundOperationYear,
            decimal amount,
            decimal fsoValue,
            decimal cocValue)
        {
            var bldModel = new FinancialDataPreparationoBLDModels();
            var dateLimit = DateTime.Parse(
                ConfigurationServiceFactory.Current.GetApplicationSettings().FSOCOCChangeDate);

            if (fundOperationYear < dateLimit)
            {
                return new FinancialDataPreparationoBLDModels
                {
                    Amount = amount * fsoValue,
                    Nombre = FundCode.FSO
                };
            }

            return new FinancialDataPreparationoBLDModels
            {
                Amount = amount * cocValue,
                Nombre = FundCode.COC
            };
        }

        private int GetClientDataForOrganizationalUnit(
            ClientFieldData[] clientFieldData,
            bool isOperationData,
            string operationNumber)
        {
            if (isOperationData)
            {
                OperationHelper operation = new OperationHelper(operationNumber);

                if (operation.Entity == null)
                {
                    return 0;
                }

                return operation.Entity.OrganizationalUnitRelated
                    .Where(or => or.Role.Code == ORG_RESPONSIBLE &&
                        (or.ExpirationDate >= DateTime.Today || !or.ExpirationDate.HasValue))
                    .Select(id => id.OrganizationUnitId).FirstOrDefault();
            }

            return GetValueClientDataGeneric(clientFieldData, ORGANIZATIONAL_UNIT);
        }

        private int GetClientDataForCountry(
            ClientFieldData[] clientFieldData,
            bool isOperationData,
            string operationNumber)
        {
            if (isOperationData)
            {
                OperationHelper operation = new OperationHelper(operationNumber);

                if (operation.Entity == null)
                {
                    return 0;
                }

                return CountryHelper.Get().GetCountry(operation.Entity.OperationId);
            }

            return GetValueClientDataGeneric(clientFieldData, COUNTRY);
        }

        private IList<int> GetClientDataForCountryRelated(string operationNumber)
        {
            OperationHelper operation = new OperationHelper(operationNumber);

            if (operation.Entity == null)
            {
                return new List<int>();
            }

            return CountryHelper.Get().GetRelatedCountries(operation.Entity.OperationId);
        }

        private IList<int> GetClientDataForCountryRelated(
            ClientFieldData[] clientFieldData,
            int operationCountryId)
        {
            var countriesRelated = GetListValueClientDataGeneric(clientFieldData, LIST_COUNTRY);

            if (countriesRelated.Count == 0 &&
                !CountryHelper.Get().IsRegionalCountry(operationCountryId))
            {
                return new List<int> { operationCountryId };
            }

            return countriesRelated;
        }

        private int GetClientDataForSupportingType(ClientFieldData[] clientFieldData)
        {
            return GetValueClientDataGeneric(clientFieldData, SUPPORTTYPE);
        }

        private IList<SelectListItem> ControlRelationshipTypes(
            ClientFieldData[] clientFieldData,
            bool isOperationData,
            IList<SelectListItem> relationshipTypes,
            string operationNumber)
        {
            int lendingType = GetValueClientDataGeneric(clientFieldData, AttributeCode.LENDING_TYPE);
            int sgInstruments = GetValueClientDataGeneric(clientFieldData, AttributeCode.SG_INSTRUMENTS);
            int lonModality = GetValueClientDataGeneric(clientFieldData, AttributeCode.LON_MODALITY);
            int operationType = !isOperationData ?
                GetValueClientDataGeneric(clientFieldData, OPERATION_TYPE) : 0;

            var responseRelationshipTypes = _operationDataService.GetRelationshipTypes(
                lendingType,
                sgInstruments,
                lonModality,
                operationType,
                isOperationData,
                relationshipTypes,
                operationNumber);

            if (!responseRelationshipTypes.IsValid)
            {
                return relationshipTypes;
            }

            return responseRelationshipTypes.Models.ToList();
        }

        private IList<SelectListItem> GetRolesOrgUnit(int operationTypeId)
        {
            var excludedValues = new List<string>();
            var operationTypeResponse = _catalogService
                .GetConvergenceMasterDataById(operationTypeId);

            if (operationTypeResponse.IsValid &&
                operationTypeResponse.Model != null)
            {
                var opTypeList = new List<string>
                {
                    operationTypeResponse.Model.Code
                };
                if (!UDRBusinessLogic.MustIncludeUDRAsRole(opTypeList))
                    excludedValues.Add(OrgUnitRoleCode.ORG_UDR);
                if (operationTypeResponse.Model.Code == OperationType.PRG)
                    excludedValues.Add(OrgUnitRoleCode.CO_RESPONSIBLE);
            }

            return _catalogService.GetListMasterData(
                MasterType.ORG_UNIT_ROLE, excludeByCode: excludedValues);
        }

        private void CacheDataToCPD(BasicDataViewModel model)
        {
            _cacheData.Add<BasicDataViewModel>(
                        "modelBasicDataOR",
                        model,
                        int.Parse(ConfigurationManager.AppSettings.Get("CacheMediumTime")));
        }
    }
}