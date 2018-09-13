using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CPDModule.Services.ProductProfile;
using IDB.MW.Application.CPDModule.ViewModels.ProductProfile;
using IDB.MW.Application.OPUSModule.Services.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Services.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.CrossCreationOperationService;
using IDB.MW.Application.OPUSModule.Services.DeliverableService;
using IDB.MW.Application.OPUSModule.Services.DocumentService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Business.OPUSModule.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.CPD.Mappers;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Values.CPD;
using IDB.MW.Application.CPDModule.Services.NSGIntegrationCPD;
using IDB.Architecture.Logging;
using IDB.Architecture.Cache;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
{
    public partial class ProductProfileController : BaseController
    {
        #region Constants
        public const string RelationshipCodeParent = "PCPD_PARENT";
        public const string RelationshipCodeChild = "PCPD_CHILD";
        private const string RelationshipTypeItself = "SELFCONTAINER";

        public static string TAB_NAME_BASICDATA
        {
            get { return "#linktabBasicData"; }
        }

        public static string TAB_NAME_RESPONSIBILITYDATA
        {
            get { return "#linktabResponsibilityData"; }
        }

        public static string READ_ACTION
        {
            get { return "Read"; }
        }

        public static string EDIT_ACTION
        {
            get { return "Edit"; }
        }

        public static string PP_CONTROLLER
        {
            get { return "ProductProfile"; }
        }

        public static string PARAM0
        {
            get { return "param0"; }
        }

        public static string PARAM1
        {
            get { return "param1"; }
        }

        public static string PARAM2
        {
            get { return "param2"; }
        }

        public static string OPUS_EDIT
        {
            get { return "edit"; }
        }

        public static string URL_OPERATION_BASIC_DATA
        {
            get { return "/CPD/ProductProfile/BasicData"; }
        }

        public static string URL_OPERATION_RESPONSABILITY_DATA
        {
            get { return "/CPD/ProductProfile/Responsability"; }
        }
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        private readonly IProductProfileService _productProfileService;
        private readonly IConvergenceRoleRepository _convergenceRoleRepository;
        private readonly IOperationDataService _operationDataService;

        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly ICreationFormService _creationFormService;
        private readonly ICalculationOperationYearService _calculationOperationYearService;
        private readonly IDocumentService _documentService;
        private readonly IVerifyCountryService _verifyCountryService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IFinancialDataBussinesService _getFinancingTypeListService;
        private readonly IDeliverableService _deliverableService;
        private readonly IApprovalOperationService _approvalOperationService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;
        private readonly ICrossCreationOperationService _crossCreationOperationService;
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly IRelationshipService _relationshipService;
        private readonly IApprovalIncreasesRevampService _approvalIncreasesRevampService;

        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ISecurityModelRepository _securityModelRepository;
        private readonly ICacheManagement _cacheManagement;
        #endregion

        #region Constructors
        public ProductProfileController(
            IAuthorizationService authorizationService,
            ICatalogService catalogService,
            IProductProfileService productProfileService,
            IEnumMappingService enumMappingService,
            IConvergenceRoleRepository convergenceRoleRepository,
            ICheckOperationTypeService checkOperationTypeService,
            IFinancialDataPreparationService financialDataPreparationService,
            ICreationFormService creationFormService,
            ICalculationEffortDaysService calculationEffortDaysService,
            IDocumentService documentService,
            IVerifyCountryService verifyCountryService,
            IFinancialDataExecutionService financialDataExecutionService,
            IFinancialDataBussinesService getFinancingTypeListService,
            IDeliverableService deliverableService,
            IApprovalOperationService approvalOperationService,
            IConvergenceMasterDataRepository convergenceMasterDataRepository,
            ICrossCreationOperationService crossCreationOperationService,
            IAttributesEngineService attributesEngineService,
            IRelationshipService relationshipService,
            ICalculationOperationYearService calculationOperationYearService,
            IOperationDataService operationDataService,
            ISecurityModelRepository securityModelRepository,
            IApprovalIncreasesRevampService approvalIncreasesRevampService,
            ICacheManagement cacheManagement)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _catalogService = catalogService;
            _cacheService = CacheStorageFactory.Current;
            _productProfileService = productProfileService;
            _convergenceRoleRepository = convergenceRoleRepository;
            _operationDataService = operationDataService;
            _calculationOperationYearService = calculationOperationYearService;

            _financialDataPreparationService = financialDataPreparationService;
            _creationFormService = creationFormService;
            _documentService = documentService;
            _verifyCountryService = verifyCountryService;
            _financialDataExecutionService = financialDataExecutionService;
            _getFinancingTypeListService = getFinancingTypeListService;
            _deliverableService = deliverableService;
            _approvalOperationService = approvalOperationService;
            _convergenceMasterDataRepository = convergenceMasterDataRepository;
            _crossCreationOperationService = crossCreationOperationService;
            _attributesEngineService = attributesEngineService;
            _relationshipService = relationshipService;
            _approvalIncreasesRevampService = approvalIncreasesRevampService;
            _securityModelRepository = securityModelRepository;
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
            _cacheManagement = cacheManagement;
        }
        #endregion

        #region Action Methods
        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName ?? TAB_NAME_BASICDATA;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetProductData(operationNumber);
            return View(READ_ACTION, model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName ?? TAB_NAME_BASICDATA;

            if (tabName == TAB_NAME_BASICDATA || tabName == null)
            {
                SynchronizationHelper.TryReleaseLock(URL_OPERATION_BASIC_DATA, operationNumber, IDBContext.Current.UserLoginName);
            }
            else
            {
                SynchronizationHelper.TryReleaseLock(URL_OPERATION_RESPONSABILITY_DATA, operationNumber, IDBContext.Current.UserLoginName);
            }

            var model = GetProductData(operationNumber);
            return View(READ_ACTION, model);
        }

        public virtual ActionResult EditResponsibilityData(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, URL_OPERATION_RESPONSABILITY_DATA, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_NAME_RESPONSIBILITYDATA, errorMessage = errorMessage });
            }

            ViewBag.ActiveTab = TAB_NAME_RESPONSIBILITYDATA;
            var model = GetResponsibilityData(operationNumber);
            ViewBag.SerializedResponsibilityDataViewModel = PageSerializationHelper.SerializeObject(model);
            SetViewBagEditResponsabilityData(operationNumber, model);
            return View(EDIT_ACTION, new ProductDataViewModel() { ResponsibilityData = model });
        }

        public virtual ActionResult EditBasicData(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, URL_OPERATION_BASIC_DATA, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_NAME_BASICDATA, errorMessage = errorMessage });
            }

            ViewBag.ActiveTab = TAB_NAME_BASICDATA;
            var model = GetBasicData(operationNumber);
            model.BasicData.SerializedModel = PageSerializationHelper.SerializeObject(model.BasicData);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model);
            SetViewBagBasicData(model);
            return View(EDIT_ACTION, new ProductDataViewModel() { BasicData = model });
        }

        public virtual JsonResult SaveResponsibilityData(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ResponsibilityDataViewModel>(jsonDataRequest.SerializedData);
            model.UpdateProductDataResponsibilityData(jsonDataRequest.ClientFieldData);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(OPUS_EDIT, URL_OPERATION_RESPONSABILITY_DATA, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var response = _productProfileService.SaveResponsibilityData(operationNumber, model);
                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(URL_OPERATION_RESPONSABILITY_DATA, operationNumber, userName);
                var url = Url.Action(READ_ACTION, PP_CONTROLLER, new { area = AttributeValue.CPD, tabName = TAB_NAME_RESPONSIBILITYDATA });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveBasicData(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<BasicDataViewModel>(jsonDataRequest.SerializedData);
            model.UpdateProductDataBasicData(jsonDataRequest.ClientFieldData);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(OPUS_EDIT, URL_OPERATION_BASIC_DATA, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var response = _productProfileService.SaveBasicData(operationNumber, model);
                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(URL_OPERATION_BASIC_DATA, operationNumber, userName);
                var url = Url.Action(READ_ACTION, PP_CONTROLLER, new { area = AttributeValue.CPD, tabName = TAB_NAME_BASICDATA });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult GetUsersList(string operationNumber, string filter)
        {
            var response = _productProfileService.SearchUser(operationNumber, filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult GetOrganizationalUnits(string filter, string lendingType, int actualDivision = 0)
        {
            var operationType = Request.QueryString[PARAM0];
            var guaranteedBy = Request.QueryString[PARAM1];
            var orgUnit = Request.QueryString[PARAM2];

            var response = _viewModelMapperHelper.GetOrganizationalUnitsFilter(
                filter, lendingType, operationType, orgUnit, guaranteedBy, actualDivision);

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

        public virtual ActionResult GetRowRelatedOperation(string operationNumber)
        {
            var excludeRelationshipType = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipType = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_TYPE, true, excludeRelationshipType);
            var excludeRelationshipRole = new List<string> { RelationshipTypeItself };
            ViewBag.RelationshipRole = _catalogService.GetListMasterData(
                MasterType.RELATIONSHIP_ROLE, excludeByCode: excludeRelationshipRole);

            var basicDataModel = _cacheManagement
                .Get<MW.Application.OPUSModule.ViewModels.OperationDataService.BasicDataViewModel>(
                "modelBasicDataOR");

            _cacheManagement.Remove("modelBasicDataOR",
                System.Web.Caching.CacheItemRemovedReason.Removed);

            MW.Application.OPUSModule.ViewModels.OperationDataService.BasicDataViewModel model = basicDataModel;

            model = _productProfileService.GetRelatedOperation(model).BasicData;

            model.SerializedModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("EditPartial/_TableRelations", model);
        }

        #endregion

        #region Private Methods
        private ProductDataViewModel GetProductData(string operationNumber)
        {
            var model = new ProductDataViewModel();
            var response = _productProfileService.GetProductData(operationNumber);
            model.BasicData = response.BasicData;
            model.ResponsibilityData = response.ResponsibilityData;
            model.IsOperationClosed = response.IsOperationClosed;
            SetViewBagErrorMessageInvalidResponse(response);

            model.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                                       IDBContext.Current.Operation,
                                       AttributeCPD.PAGE_NAME_BASIC_DATA +
                                        '|' + AttributeCPD.PAGE_NAME_RESPONSABILITY_DATA,
                                       IDBContext.Current.Permissions,
                                       0,
                                       0)
                                       .ToList();

            return model;
        }

        private ResponsibilityDataViewModel GetResponsibilityData(string operationNumber)
        {
            var response = _productProfileService.GetResponsibilityData(operationNumber);
            SetViewBagErrorMessageInvalidResponse(response);
            return response.ResponsibilityData;
        }

        private BasicDataViewModel GetBasicData(string operationNumber)
        {
            var response = _productProfileService.GetBasicData(operationNumber);
            SetViewBagErrorMessageInvalidResponse(response);
            return response.BasicData;
        }

        private void SetViewBagBasicData(BasicDataViewModel model)
        {
            var response = _calculationOperationYearService.GetOperationYear(model.ProductType, null, model.ProductYear ?? 0);
            ViewBag.Years = new List<SelectListItem>();
            if (response.IsValid)
            {
                ViewBag.Years = response.Years.Select(x => new SelectListItem()
                {
                    Text = x.ToString(),
                    Value = x.ToString(),
                }).ToList();
            }
        }

        private void SetViewBagEditResponsabilityData(string operationNumber, ResponsibilityDataViewModel model)
        {
            SetViewBagEditModel(operationNumber);
            ViewBag.ReponseRoleId = _catalogService.GetConvergenceMasterDataIdByCode(OrgUnitRoleCode.ORG_RESPONSIBLE, MasterType.ORG_UNIT_ROLE);
            ViewBag.ResponsibleUnitsRoleList = _catalogService.GetListMasterData(MasterType.ORG_UNIT_ROLE);
            ViewBag.ResponsibleUnitsOrganizationalUnitList = _viewModelMapperHelper.GetDivisionList();
        }

        private void SetViewBagEditModel(string operationNumber)
        {
            var includeValues = new List<string> { MemberRoleCode.FIDUCIARY_SPECIALIST, MemberRoleCode.PROCUREMENT_FIDUCIARY_SPECIALIST, MemberRoleCode.TEAM_LEADER, MemberRoleCode.VPC_TEAM_MEMBER };
            ViewBag.TeamLeaderRoleId = _catalogService.GetConvergenceMasterDataIdByCode(MemberRoleCode.TEAM_LEADER, MasterType.MEMBER_ROLE);
            ViewBag.OperationTeamsRoleList = _catalogService.GetListMasterData(MasterType.MEMBER_ROLE, includeByCode: includeValues);
        }
        #endregion
    }
}