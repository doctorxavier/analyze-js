using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Microsoft.Ajax.Utilities;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.AttributesModule.Messages.AttributesService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.MW.Application.Core.Messages;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.OPUSModule.Mappers.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Messages.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Messages.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Messages.ClimateChangeService;
using IDB.MW.Application.OPUSModule.Messages.CreationFormService;
using IDB.MW.Application.OPUSModule.Messages.CrossCreationOperationService;
using IDB.MW.Application.OPUSModule.Messages.DeliverableService;
using IDB.MW.Application.OPUSModule.Messages.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;
using IDB.MW.Application.OPUSModule.Services.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Services.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.CrossCreationOperationService;
using IDB.MW.Application.OPUSModule.Services.DeliverableService;
using IDB.MW.Application.OPUSModule.Services.DocumentService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Application.OPUSModule.ViewModels.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.ViewModels.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.ViewModels.ClimateChangeService;
using IDB.MW.Application.OPUSModule.ViewModels.CofinancingService;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.MW.Application.OPUSModule.ViewModels.CreationFormService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.ViewModels.OperationDataService;
using IDB.MW.Application.Reformulation.ViewModels;
using IDB.MW.Business.Core.OPUSService.DTOs;
using IDB.MW.Business.OPUSModule.Contracts;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Data.Optima.Context;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MW.Application.Helpers;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    public class ViewModelMapperHelper
    {
        const string APPROVAL_AUTH = "ApprovalAuth";
        const string APPROVAL_PROCESS = "ApprovalProcess";
        const string JUSTIFICATION = "Justification";
        private const char SEPARATOR = '-';

        private readonly dynamic _viewBag;
        private readonly IAuthorizationService _authorizationService;
        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly ICreationFormService _creationFormService;
        private readonly IOperationDataService _operationDataService;
        private readonly ICalculationOperationYearService _calculationOperationYearService;
        private readonly ICalculationEffortDaysService _calculationEffortDaysService;
        private readonly IDocumentService _documentService;
        private readonly ICatalogService _catalogService;
        private readonly IVerifyCountryService _verifyCountryService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IFinancialDataBussinesService _getFinancingTypeListService;
        private readonly IDeliverableService _deliverableService;
        private readonly IApprovalOperationService _approvalOperationService;
        private readonly ICrossCreationOperationService _crossCreationOperationService;
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly IRelationshipService _relationshipService;
        private readonly IApprovalIncreasesRevampService _approvalIncreasesRevampService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;

        public ViewModelMapperHelper(dynamic viewBag,
            IAuthorizationService authorizationService,
            IFinancialDataPreparationService financialDataPreparationService,
            ICreationFormService creationFormService,
            IOperationDataService operationDataService,
            ICatalogService catalogService,
            ICalculationOperationYearService calculationOperationYearService,
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
            IApprovalIncreasesRevampService approvalIncreasesRevampService)
        {
            _viewBag = viewBag;
            _authorizationService = authorizationService;
            _financialDataPreparationService = financialDataPreparationService;
            _creationFormService = creationFormService;
            _operationDataService = operationDataService;
            _catalogService = catalogService;
            _calculationOperationYearService = calculationOperationYearService;
            _calculationEffortDaysService = calculationEffortDaysService;
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
        }

        public FinancialDataPreparationResponse GetFinancialDataPreparation(
            string operationNumber, ReformulationViewModel reformulation)
        {
            var response = _financialDataPreparationService
                .GetFinancialDataPreparation(operationNumber, reformulation);

            if (response.FinancialDataPreparation == null)
            {
                response.FinancialDataPreparation = new FinancialDataPreparationViewModel()
                {
                    CoFinancing = new CoFinancingViewModel
                    {
                        CofinancingResourcesDetail =
                            new List<CofinancingResourcesDetailViewModel>()
                    },
                    CoFinancingInKind = false,
                    CounterpartFinancing = new CounterpartFinancingViewModel(),
                    ExpectedIDB = new List<ExpectedIDBViewModel>(),
                    IsRetroactiveExpense = false,
                    AvailablePsgDonors = new List<FundDonorDtfViewModel>(),
                    UnavailablePsgDonors = new List<FundDonorDtfViewModel>()
                };
            }

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public List<ListItemViewModel> GetFinancingCodeTypeList(int operationType,
            string operationNumber,
            dynamic iAttributesEngineService,
            bool isFromCreation = false)
        {
            var financingTypeList = new List<ListItemViewModel>();

            var financingType = _getFinancingTypeListService
                .GetFinancingTypeList(operationType,
                    operationNumber,
                    _attributesEngineService,
                    isFromCreation: isFromCreation).FinancingTypeCodeList;

            if (financingType != null && financingType.Count > 0)
            {
                financingTypeList = financingType.Select(o => new ListItemViewModel
                {
                    Select = false,
                    Value = o.ConvergenceMasterDataId.ToString(),
                    Text = o.Code + @" - " + MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).ToList();
            }

            return financingTypeList;
        }

        public List<SelectListItem> GetFinancingCodeTypeListViewBag(int operationType,
            string operationNumber,
            dynamic iAttributesEngineService,
            bool isFromCreation)
        {
            var financingTypeList = new List<SelectListItem>();
            var financingType = _getFinancingTypeListService.GetFinancingTypeList(operationType,
                operationNumber,
                _attributesEngineService,
                isFromCreation: isFromCreation)
                .FinancingTypeCodeList;

            if (financingType.HasAny())
            {
                financingTypeList = financingType.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.ConvergenceMasterDataId.ToString(),
                    Text = o.Code + @" - " + MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).ToList();
            }

            return financingTypeList;
        }

        public List<ListItemViewModel> GetFundList(
            bool gsmPermission,
            int operationType,
            int country,
            bool isFromCreation = false,
            bool isMifOpuOperation = false)
        {
            var fundList = new List<ListItemViewModel>();
            var fund = _financialDataPreparationService.GetFundList(
                gsmPermission,
                operationType,
                country,
                isFromCreation,
                isMifOpuOperation);

            if (fund.FundList.HasAny())
            {
                fundList = fund.FundList.Select(o => new ListItemViewModel
                {
                    Select = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return fundList;
        }

        public List<SelectListItem> GetFundListViewBag(
            bool gsmPermission,
            int operationType,
            int country,
            bool isFromCreation,
            bool isMifOpuOperation = false)
        {
            var fundList = new List<SelectListItem>();
            var fund = _financialDataPreparationService.GetFundList(
                gsmPermission,
                operationType,
                country,
                isFromCreation: isFromCreation,
                isMifOpuOperation: isMifOpuOperation);

            if (fund.FundList.HasAny())
            {
                fundList = fund.FundList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return fundList;
        }

        public List<MultiSelectListItemViewModel> FundTBD(bool gsmPermission, int operationType, int country)
        {
            var fundList = new List<MultiSelectListItemViewModel>();
            var fund = _financialDataPreparationService
                .FundTBD(gsmPermission, operationType, country);

            if (fund.FundList.HasAny())
            {
                fundList = fund.FundList.Select(o => new MultiSelectListItemViewModel
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return fundList;
        }

        public List<MultiSelectListItem> FundTBDViewBag(bool gsmPermission, int operationType, int country)
        {
            var fundList = new List<MultiSelectListItem>();
            var fund = _financialDataPreparationService.FundTBD(gsmPermission, operationType, country);

            if (fund.FundList.HasAny())
            {
                fundList = fund.FundList.Select(o => new MultiSelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return fundList;
        }

        public List<ListItemViewModel> GetExecutingAgency(string operationnumber)
        {
            var institutionList = new List<ListItemViewModel>();
            var executingAgencyList = _financialDataPreparationService.GetExecutingAgency(operationnumber).ExecutingAgencyList;

            if (executingAgencyList != null && executingAgencyList.Count > 0)
            {
                institutionList = executingAgencyList.Select(o => new ListItemViewModel
                {
                    Select = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return institutionList;
        }

        public RowExpectedIDBViewModel GetRowExpectedIDBViewModel(
            string operationNumber,
            bool isPsgOperation,
            bool gsmPermission,
            int operationTypeId,
            int countryId)
        {
            var viewModel = _financialDataPreparationService.GetRowExpectedIDBViewModel(
                operationNumber,
                isPsgOperation,
                gsmPermission,
                operationTypeId,
                countryId);

            return viewModel;
        }

        public CurrencyResponse GetCurrency(int fundId)
        {
            return _approvalIncreasesRevampService
                .GetFundOperationCurrency(new CurrencyRequest
                {
                    FundId = fundId
                });
        }

        public List<ListItemViewModel> GetFinancingTypes(string operationNumber, int fundId)
        {
            var request = new FinancingTypeListRequest
            {
                OperationNumber = operationNumber,
                FundId = fundId
            };

            var response = _approvalIncreasesRevampService.GetFinancingTypes(request);

            if (!response.IsValid)
            {
                return new List<ListItemViewModel>();
            }

            return ApprovalIncreasesRevampMappers
                .ConvertMasterDataToFinancingTypeList(response.FinancingTypeCodeList,
                    response.FinancingTypeSelected);
        }

        public IncreaseFundsResponse GetFunds(string operationNumber,
            bool newFunds,
            bool isRequestIncrease)
        {
            var request = new IncreaseFundsRequest
            {
                OperationNumber = operationNumber,
                NewFunds = newFunds,
                IsRequestIncrease = isRequestIncrease
            };

            return _approvalIncreasesRevampService.GetRequestIncreaseFunds(request);
        }

        #region Deliverable Module

        public GetDeliverableResponse GetDeliverable(string operationNumber)
        {
            var response = _deliverableService.GetDeliverable(operationNumber);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public List<SelectListItem> GetCountryRelatedCodeList(string code, int idCountry, bool isRegional)
        {
            var list = new List<SelectListItem>();

            if (isRegional)
            {
                var listRepository = _deliverableService.GetCountryRelatedList(code, idCountry);

                if (listRepository != null && listRepository.CountryList.Count > 0)
                {
                    list = listRepository.CountryList.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.Value.ToString(),
                        Text = o.Text.ToString()
                    }).ToList();
                }
            }
            else
            {
                var defaultCountry = new SelectListItem
                {
                    Text = code,
                    Value = idCountry.ToString()
                };
                list.Add(defaultCountry);
            }

            return list;
        }

        #endregion

        #region Creation & Registration
        public GetCreationFormResponse GetCreationForm(string operationNumber, Operation operation = null, string operationTypeCode = null, GetMasterDataListByTypeIdResponse listRepository = null)
        {
            var response = _creationFormService.GetCreationForm(operationNumber, operation, operationTypeCode, listRepository);

            if (response.CreationForm == null)
            {
                response.CreationForm = new CreationFormViewModel()
                {
                    BasicData = new BasicDataViewModel
                    {
                        RelatedOperations = new List<RelatedOperationRowViewModel>(),
                        Relationships = new List<RelationsViewModel>(),
                    },
                    ExpectedIDB = new List<ExpectedIDBViewModel>(),
                    ResponsibilityData = new ResponsibilityDataViewModel
                    {
                        AssociatedInstitutions = new List<AssociatedInstitutionsRowViewModel>(),
                        OperationTeams = new List<OperationTeamRowViewModel>(),
                        OrganizationalUnit = new List<OrganizationalUnitRowViewModel>(),
                        CountryRelated = new List<AssociatedCountriesRowViewModel>()
                    },
                    Attributes = new FormAttributeOperationCollectionViewModel(),
                    IsPsgOperation = false,
                    AvailablePsgDonors = new List<FundDonorDtfViewModel>(),
                };
            }

            LoadReferencesIds(response.CreationForm);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetFormAttributeResponse GetAttributteByTypeOperation(int operationType, string operationNumber)
        {
            var response = _creationFormService.GetAttributesByOperationType(operationType, operationNumber);
            return response;
        }

        //Temporal Method
        public List<SelectListItem> GetListOperationType(string type, GetMasterDataListByTypeIdResponse masterDataListByTypeCode)
        {
            var list = new List<SelectListItem>();

            var listAux = new List<MasterDataViewModel>();
            masterDataListByTypeCode.MasterDataCollection.CopyItemsTo(listAux);
            var listRepository =
                listAux.Where(
                    o => o.TypeName == type &&
                        (o.ExpirationDate == null || o.ExpirationDate > DateTime.Now)).ToList();
            if (listRepository.Any())
            {
                list = listRepository.DistinctBy(x => x.Code).Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = o.Code + @" - " + MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).ToList();
            }

            return list;
        }

        public int GetOperationTypeCode(string operationType)
        {
            int result = 0;

            var listRepository =
                _convergenceMasterDataRepository.GetByCriteria(
                    o =>
                        o.ConvergenceMasterType.Type == MasterType.OPERATION_TYPE &&
                        (o.ExpirationDate == null || o.ExpirationDate > DateTime.Now)).ToList();

            if (listRepository.Any())
            {
                ConvergenceMasterData convergenceMasterData = listRepository.DistinctBy(x => x.Code).FirstOrDefault(x => x.Code.Equals(operationType));
                if (convergenceMasterData != null)
                    result = convergenceMasterData.ConvergenceMasterDataId;
            }

            return result;
        }

        public List<SelectListItem> GetDivisionList(bool withCode = true, bool onlyCode = false)
        {
            //ToDo: This method with all optional parameters is incorrect. Fix.
            var list = new List<SelectListItem>();
            var listRepository = _crossCreationOperationService.GetDivisions();

            if (listRepository.IsValid && listRepository.Divisions.Any())
            {
                list = listRepository.Divisions.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value.ToString(),
                    Text = onlyCode ? o.AdditionalData :
                        withCode ? o.AdditionalData + " - " + o.Text : o.Text
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> AddDivisionExpired(List<SelectListItem> divisions, List<int> orgUnits)
        {
            foreach (var orgUnit in orgUnits)
            {
                var isIn = divisions.Any(o => o.Value == orgUnit.ToString());
                if (!isIn)
                {
                    var divisionToAdd =
                        _convergenceMasterDataRepository.GetOne(
                            o => o.ConvergenceMasterDataId == orgUnit);
                    divisions.Add(new SelectListItem
                    {
                        Selected = false,
                        Value = divisionToAdd.ConvergenceMasterDataId.ToString(),
                        Text = divisionToAdd.Code + " - " +
                        divisionToAdd.GetNameLanguage(IDBContext.Current.CurrentLanguage)
                    });
                }
            }

            return divisions;
        }

        public DivisionsResponse GetDivisionsFilter(
            string filter,
            bool onlyCode = false,
            bool onlyName = false,
            string excludeCode = null,
            IList<string> includeGroup = null,
            IList<string> includeDivision = null)
        {
            var response = new DivisionsResponse { IsValid = false };

            var listRepository = _crossCreationOperationService
                .GetFilteredDivisions(filter, excludeCode, includeGroup, includeDivision);

            if (listRepository.IsValid && listRepository.Divisions.Any())
            {
                response.Divisions = listRepository.Divisions.Select(o =>
                    new ListItemViewModel
                    {
                        Value = o.Value,
                        Text = onlyCode ? o.AdditionalData :
                            onlyName ? o.Text : o.AdditionalData + " - " + o.Text
                    }).ToList();
            }

            response.IsValid = true;
            return response;
        }

        public OrganizationalUnitsResponse GetOrganizationalUnitsFilter(
            string filter,
            string lendingType,
            string opType,
            string orgUnit,
            string guaranteedBy,
            int actualDivision = 0,
            string nsgCategorization = null)
        {
            var response = new OrganizationalUnitsResponse
            {
                IsValid = false,
                OrganizationalUnits = new List<ListItemViewModel>()
            };

            var excludeCodeUnit = string.Empty;
            int idLendingType = 0;
            var includeDivision = new List<string>();
            var includeGroup = new List<string>();

            if (!string.IsNullOrEmpty(lendingType) &&
                int.TryParse(lendingType, out idLendingType) &&
                idLendingType > 0)
            {
                var responseCode = _catalogService
                    .GetConvergenceMasterCodeByIdResponse(idLendingType, true);

                if (responseCode.IsValid && responseCode.Code.Equals(AttributeValue.SG))
                    excludeCodeUnit = OrgUnitCode.MIF;
            }

            var validGroups = GetValidGroups(guaranteedBy, nsgCategorization, opType, orgUnit);

            if (validGroups != null)
                includeGroup.AddRange(validGroups);

            var organizationalUnitCode = GetCode(orgUnit);
            var operationCode = GetCode(opType);
            var categorizationCode = GetCode(nsgCategorization);

            if (organizationalUnitCode == OrgUnitRoleCode.ORG_RESPONSIBLE &&
                operationCode == OperationType.LON &&
                categorizationCode == AttributeValue.TYPE_INSTRUMENT_IAIC_TC)
            {
                includeDivision.Add(OrgUnitCode.IIC);
                excludeCodeUnit = OrgUnitCode.ALL_CODES;
            }

            var responseDivisions = new DivisionsResponse();

            if (organizationalUnitCode == OrgUnitRoleCode.ORG_UDR)
            {
                responseDivisions = _crossCreationOperationService.GetAllUDROrganizations();
            }
            else
            {
                responseDivisions = GetDivisionsFilter(
                    filter: filter,
                    excludeCode: excludeCodeUnit,
                    includeGroup: includeGroup,
                    includeDivision: includeDivision);
            }

            if (!responseDivisions.IsValid)
            {
                response.ErrorMessage = responseDivisions.ErrorMessage;
                return response;
            }

            if (responseDivisions.Divisions.HasAny())
                response.OrganizationalUnits = responseDivisions.Divisions;

            if (actualDivision != 0)
            {
                var divisionInList = response.OrganizationalUnits
                    .Any(o => o.Value == actualDivision.ToString());

                if (!divisionInList)
                {
                    var divisionToAdd =
                        _convergenceMasterDataRepository.GetOne(
                            o => o.ConvergenceMasterDataId == actualDivision);

                    var code = divisionToAdd.Code + " - " +
                        divisionToAdd.GetNameLanguage(IDBContext.Current.CurrentLanguage);

                    if (code.Contains(filter) ||
                        code.ToLower().Contains(filter) ||
                        code.ToUpper().Contains(filter))
                    {
                        response.OrganizationalUnits.Add(new ListItemViewModel()
                        {
                            Text = code,
                            Value = divisionToAdd.ConvergenceMasterDataId.ToString()
                        });
                    }
                }
            }

            response.IsValid = true;

            return response;
        }

        public int GetMasterDataCode(string type, string code)
        {
            int result = 0;
            var listRepository = _catalogService.GetMasterDataListByTypeCode(hideExpired: true, typeCodes: type);

            if (listRepository.IsValid && listRepository.MasterDataCollection.Any())
            {
                var listMasterData = listRepository.MasterDataCollection;

                MasterDataViewModel masterDataViewModel = listMasterData.DistinctBy(x => x.Code).FirstOrDefault(o => o.Code.Equals(code));
                if (masterDataViewModel != null)
                    result =
                        masterDataViewModel.MasterId;
            }

            return result;
        }

        public List<SelectListItem> GetListCountryDeparmet(string operationNumber)
        {
            var countryDepartment = CatalogServiceHelper.GetCountryDepartment(_catalogService);

            return SelectListItemHelpers.BuildSelectItemList(
                countryDepartment,
                o => o.Code + " - " + o.GetLocalizedName(),
                o => o.MasterId.ToString())
                .ToList();
        }

        public List<SelectListItem> GetListRelationshipRoleList(int relationshipType, string type, string relationshipRoleItself, List<OPUSCheckOperation> tableArray, int relationshipRoleMainOp = 0, int opType = 0, bool findIsCon = false, string deleteOpInView = "")
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository.IsValid &&
                listRepository.MasterDataCollection.Any())
            {
                var relationshipList = string.IsNullOrEmpty(IDBContext.Current.Operation) ||
                    IDBContext.Current.IsOperationGlobal() ?
                        new List<OperationRelated>() :
                        _crossCreationOperationService.GetRelationShip(
                            null, IDBContext.Current.Operation);
                var getRelationRole =
                    _relationshipService.GetRelationRoleItems(relationshipType, tableArray, relationshipList, relationshipRoleMainOp, opType, findIsCon, deleteOpInView)
                        .RelationRoleList;

                list = getRelationRole.Select(role => listRepository.MasterDataCollection.Find(x => x.Code.ToString().Contains(role))).Select(relationship => new SelectListItem
                {
                    Selected = false,
                    Value = relationship.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(relationship, Localization.CurrentLanguage)
                }).ToList();
            }

            return list;
        }

        //Temporal
        public GetSelectListItemResponse GetListMasterDataWithExpired(string type, string search, bool withCode = false)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();
            var listRepository =
               _convergenceMasterDataRepository.GetByCriteria(
                   o =>
                       o.ConvergenceMasterType.Type == type &&
                       (o.ExpirationDate == null || o.ExpirationDate > DateTime.Now)).ToList();

            if (listRepository.Any())
            {
                if (withCode)
                {
                    listRepository =
                        listRepository.Where(x => x.NameEn.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameEs.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameFr.ToLower().Contains(search.ToLower()) ||
                                                                       x.NamePt.ToLower().Contains(search.ToLower()) ||
                                                                       x.Code.ToLower().Contains(search.ToLower()))
                            .ToList();
                }
                else
                {
                    listRepository = listRepository.DistinctBy(o => o.Code)
                        .Where(x => x.NameEn.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameEs.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameFr.ToLower().Contains(search.ToLower()) ||
                                                                       x.NamePt.ToLower().Contains(search.ToLower()))
                            .ToList();
                }

                if (listRepository.Any())
                {
                    response.ListResponse = listRepository.DistinctBy(o => o.Code).Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.ConvergenceMasterDataId.ToString(),
                        Text = (withCode ? o.Code + " - " : string.Empty) + MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                    }).ToList();
                }
            }

            return response;
        }

        public GetSelectListItemResponse GetListMasterDataFilter(string type, string search, bool withCode = false, bool onlyCode = false)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse
            {
                ListResponse = new List<SelectListItem>()
            };
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);
            if (listRepository.IsValid && listRepository.MasterDataCollection.Any())
            {
                List<MasterDataViewModel> filter;
                if (onlyCode)
                {
                    filter =
                        listRepository.MasterDataCollection.DistinctBy(o => o.Code)
                            .Where(x => x.Code.ToLower().Contains(search.ToLower())).ToList();
                }
                else if (withCode)
                {
                    filter =
                        listRepository.MasterDataCollection.DistinctBy(o => o.Code).Where(x => x.NameEn.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameEs.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameFr.ToLower().Contains(search.ToLower()) ||
                                                                       x.NamePt.ToLower().Contains(search.ToLower()) ||
                                                                       x.Code.ToLower().Contains(search.ToLower()))
                            .ToList();
                }
                else
                {
                    filter = listRepository.MasterDataCollection.DistinctBy(o => o.Code)
                        .Where(x => x.NameEn.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameEs.ToLower().Contains(search.ToLower()) ||
                                                                       x.NameFr.ToLower().Contains(search.ToLower()) ||
                                                                       x.NamePt.ToLower().Contains(search.ToLower()))
                            .ToList();
                }

                if (filter.Any())
                {
                    response.ListResponse = filter.DistinctBy(o => o.Code).Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.MasterId.ToString(),
                        Text = onlyCode ? o.Code : (withCode ? o.Code + " - " : string.Empty) + MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                    }).ToList();
                }
            }

            return response;
        }

        public List<SelectListItem> GetListOperationYear(int startYear = 0)
        {
            var operationYearListItem = new List<SelectListItem>();
            var operatinYearService = _creationFormService.GetOperationYearList(startYear);

            if (operatinYearService.OperationYear != null && operatinYearService.OperationYear.Count > 0)
            {
                operationYearListItem = operatinYearService.OperationYear.OrderByDescending(x => x.Value).Select(o => new SelectListItem
                {
                    Selected = o.Select,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return operationYearListItem;
        }

        public List<SelectListItem> GetInstitutionList()
        {
            var institutionList = new List<SelectListItem>();
            var institutionRepository = _crossCreationOperationService.GetInstitutionList();

            if (institutionRepository.InstitutionList != null && institutionRepository.InstitutionList.Count > 0)
            {
                institutionList = institutionRepository.InstitutionList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return institutionList;
        }

        public List<SelectListItem> GetInstitutionFilteredList(IList<int> institutionIdList)
        {
            var institutionList = new List<SelectListItem>();
            var institutionRepository = _crossCreationOperationService.GetInstitutionListFilteredById(institutionIdList);

            if (institutionRepository.InstitutionList != null && institutionRepository.InstitutionList.Count > 0)
            {
                institutionList = institutionRepository.InstitutionList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return institutionList;
        }

        public List<SelectListItem> GetInstitutionList(
            string nameAcronym, int type, int countryId, bool hideExpired = false)
        {
            var institutionList = new List<SelectListItem>();
            var institutionRepository = _crossCreationOperationService
                .GetInstitutionList(nameAcronym, type, countryId, hideExpired);

            if (institutionRepository.InstitutionList != null && institutionRepository.InstitutionList.Count > 0)
            {
                institutionList = institutionRepository.InstitutionList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return institutionList;
        }

        public SearchRelatedOperationResponse SearchRelatedOperationResponse(
            int? relationshipType,
            string relatedOperationFilter,
            string operationNumber,
            int opType,
            string xml,
            bool isOpData,
            int opCountry,
            IList<int> countries,
            int opOrganizationalUnit,
            int supportType)
        {
            var response = _crossCreationOperationService.SearchRelatedOperation(
                relationshipType,
                relatedOperationFilter,
                operationNumber,
                opType,
                xml,
                isOpData,
                opCountry,
                countries,
                opOrganizationalUnit,
                supportType);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetOperationBasicDataResponse GetBasicData(
            string operationNumber, bool hasRelationsView = false, Operation operation = null, GetAttributesBasicResponse getAttributesBasicResponse = null, GetMasterDataListByTypeIdResponse listRepository = null)
        {
            var response = _operationDataService.GetOperationBasicData(operationNumber, hasRelationsView, null, listRepository);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;

                return response;
            }

            response.BasicData.Attributes = GetAttributesBasic(operationNumber).Attributes.Attributes;

            return response;
        }

        public GetResponsabilityDataResponse GetResponsabilityData(
            string operationNumber,
            bool searchDataToLoadTab)
        {
            var response = _operationDataService.GetOperationResponsabilityData(operationNumber,
                                                                                searchDataToLoadTab);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetStrategicAlignmentResponse GetStrategicAlignment(string operationNumber)
        {
            var response = _operationDataService.GetStrategicAlignment(operationNumber);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetClimateChangeResponse GetClimateChange(string operationNumber)
        {
            var response = _operationDataService.GetClimateChange(operationNumber);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public CreateCCESResponse CreateOrUpdateCCES(ClimateDataViewModel modelData, List<ClimateTableViewModel> modelTable, string operationNumber)
        {
            var response = new CreateCCESResponse();
            var climateChangeHistory = new ClimateChangeHistoryViewModel();
            var ClimateChangeRowList = new List<ClimateChangeRowViewModel>();
            climateChangeHistory.TrackedByESG = modelData.TrackedByESG;
            climateChangeHistory.TrackedByCCS = modelData.TrackedByCCS;
            if (modelTable != null)
            {
                foreach (var item in modelTable)
                {
                    var ClimateChangeRow = new ClimateChangeRowViewModel
                    {
                        ClimateChangeEnviromentalSustainabilityId = item.CCESId,
                        EnvironmentId = item.EnvironmentId,
                        DisasterId = item.DisasterRiskManagmentId,
                        ClimateChangeMitigationSubId = item.ClimateChangeMitigationSubId,
                        ClimateChangeAdaptationSubId = item.ClimateChangeAdaptationSubId,
                        SocialSustainabilitySubId = item.SocialSustainabilitySubId,
                        IdbAmountPercent = item.OriginalIDBAmount,
                        Justification = item.Justification,
                        JustificationDate = item.JustificationDate,
                        JustificationUser = item.JustificationUser
                    };
                    ClimateChangeRowList.Add(ClimateChangeRow);
                }
            }

            response = _operationDataService.CreateOrUpdateCCES(climateChangeHistory, ClimateChangeRowList, operationNumber);
            return response;
        }

        public GetClimateChangeResponse GetClimateChangeNewRow(string operationNumber)
        {
            var climateChangeMitigationList = _catalogService.GetListMasterDataCollection(MasterType.CLIMATE_CHANGE_MITIGATION, parentId: null).ToList();
            var mitigationNone = climateChangeMitigationList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            climateChangeMitigationList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);

            var climateChangeAdaptionList = _catalogService.GetListMasterDataCollection(MasterType.CLIMATE_CHANGE_ADAPTATION, parentId: null).ToList();
            var adaptationNone = climateChangeAdaptionList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            climateChangeAdaptionList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);

            var climateChangeEnvironmentList = _catalogService.GetListMasterDataCollection(MasterType.CLIMATE_CHANGE_ENVIRONMENT, parentId: null).ToList();
            var environmentNone = climateChangeEnvironmentList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            climateChangeEnvironmentList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);

            var climateChangeDisasterList = _catalogService.GetListMasterDataCollection(MasterType.CLIMATE_CHANGE_DISASTER, parentId: null).ToList();
            var disasterNone = climateChangeDisasterList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            climateChangeDisasterList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);

            var climateChangeSocialList = _catalogService.GetListMasterDataCollection(MasterType.CLIMATE_CHANGE_SOCIAL, parentId: null).ToList();
            var socialNone = climateChangeSocialList.Where(x => x.NameEn == OPUSGlobalValues.CCES_NONE).FirstOrDefault();
            climateChangeSocialList.RemoveAll(x => x.NameEn == OPUSGlobalValues.CCES_NONE);

            var response = new GetClimateChangeResponse()
            {
                ClimateChangeRow = new MW.Application.OPUSModule.ViewModels.ClimateChangeService.ClimateChangeRowViewModel()
                {
                    ClimateChangeMitigationList = climateChangeMitigationList.Select(o => new ListItemViewModel
                    {
                        Value = o.MasterId.ToString(),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                    }).OrderBy(x => x.Text).ToList(),
                    ClimateChangeAdaptationCategoryList = climateChangeAdaptionList.Select(o => new ListItemViewModel
                    {
                        Value = o.MasterId.ToString(),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                    }).OrderBy(x => x.Text).ToList(),
                    EnvironmentList = climateChangeEnvironmentList.Select(o => new ListItemViewModel
                    {
                        Value = o.MasterId.ToString(),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                    }).OrderBy(x => x.Text).ToList(),
                    DisasterRiskManagementList = climateChangeDisasterList.Select(o => new ListItemViewModel
                    {
                        Value = o.MasterId.ToString(),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                    }).OrderBy(x => x.Text).ToList(),
                    ClimateChangeSocialCategoryList = climateChangeSocialList.Select(o => new ListItemViewModel
                    {
                        Value = o.MasterId.ToString(),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                    }).OrderBy(x => x.Text).ToList()
                }
            };

            if (mitigationNone != null)
            {
                response.ClimateChangeRow.ClimateChangeMitigationList.Add(new ListItemViewModel
                {
                    Value = mitigationNone.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(mitigationNone, Localization.CurrentLanguage)
                });
            }

            if (adaptationNone != null)
            {
                response.ClimateChangeRow.ClimateChangeAdaptationCategoryList.Add(new ListItemViewModel
                {
                    Value = adaptationNone.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(adaptationNone, Localization.CurrentLanguage)
                });
            }

            if (environmentNone != null)
            {
                response.ClimateChangeRow.EnvironmentList.Add(new ListItemViewModel
                {
                    Value = environmentNone.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(environmentNone, Localization.CurrentLanguage)
                });
            }

            if (disasterNone != null)
            {
                response.ClimateChangeRow.DisasterRiskManagementList.Add(new ListItemViewModel
                {
                    Value = disasterNone.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(disasterNone, Localization.CurrentLanguage)
                });
            }

            if (socialNone != null)
            {
                response.ClimateChangeRow.ClimateChangeSocialCategoryList.Add(new ListItemViewModel
                {
                    Value = socialNone.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(socialNone, Localization.CurrentLanguage)
                });
            }

            response.IsValid = true;
            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            response.ClimateChangeRow.CanEditEnvironment =
                _operationDataService.CanEditEnvironmental(operationNumber);

            return response;
        }

        public RemoveCCESResponse RemoveCCES(int rowId)
        {
            var response = new RemoveCCESResponse();
            response = _operationDataService.RemoveCCES(rowId);
            return response;
        }

        public GetAttributesResponse GetAttributes(string operationNumber)
        {
            var response = _operationDataService.GetAttributesResponse(operationNumber);
            _viewBag.FormAttributes = response.Attributes.FormAttributes;
            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetAttributesBasicResponse GetAttributesBasic(
            string operationNumber, 
            GetAttributesBasicResponse getAttributesBasicResponse = null,
            string operationType = null,
            bool hasAbstractLock = false)
        {
            if (getAttributesBasicResponse == null)
            {
                getAttributesBasicResponse = _operationDataService
                    .GetAttributesBasicResponse(operationNumber, operationType, hasAbstractLock);
            }

            _viewBag.FormBasicAttributes = getAttributesBasicResponse.Attributes.FormAttributes;
            if (!getAttributesBasicResponse.IsValid)
            {
                _viewBag.ErrorMessage = getAttributesBasicResponse.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return getAttributesBasicResponse;
        }

        public OperationDataViewModel GetOperationByOperationNumber(string operationNumber, int csOperationModalityCop)
        {
            var operationDataViewModel = new OperationDataViewModel();

            using (OptimaContainer ctx = new OptimaContainer())
            {
                {
                    operationDataViewModel = (
                        from op in ctx.Operation
                        where op.OperationNumber == operationNumber
                        select new OperationDataViewModel
                        {
                            OperationId = op.OperationId,
                            OperationNumber = op.OperationNumber,
                            IsPaperModality = (
                                from f in ctx.FormAttributeOperation
                                where f.OperationId == op.OperationId &&
                                    f.ListValueID == csOperationModalityCop
                                select f.OperationId).Any(),
                            OperationTypeId = op.OperationData.OperationTypeId,
                            Institutions = op.InstitutionRelated
                                .Where(relIns => relIns.ExpirationDate == null ||
                                    relIns.ExpirationDate > DateTime.Now)
                                .Select(relIns =>
                                    new AssociatedInstitution
                                    {
                                        AssociatedInstitutionId = relIns.InstitutionId,
                                        AssociatedInstitutionRoleId = relIns.RoleId
                                    }).ToList()
                        }).First();

                    return operationDataViewModel;
                }
            }
        }

        public OperationDataViewModel GetOperationData(
            string operationNumber = null, bool hasRelationsView = false, Operation operation = null, int csOperationModalityCop = -1, GetMasterDataListByTypeIdResponse listRepository = null)
        {
            var operationDataViewModelAux = new OperationDataViewModel();
            if (operation != null)
            {
                operationNumber = operation.OperationNumber;
            }

            operationDataViewModelAux = GetOperationByOperationNumber(operationNumber, csOperationModalityCop);

            var operationDataViewModel = new OperationDataViewModel
            {
                BasicData = new BasicDataViewModel
                {
                    ExpectedIdbAmount = new List<ExpectedIDBViewModel>(),
                },
                ResponsabilityData = new ResponsabilityDataViewModel(),
                Attributes = new FormAttributeOperationCollectionViewModel()
            };

            operationDataViewModel.OperationId = operationDataViewModelAux.OperationId;
            operationDataViewModel.OperationNumber = operationDataViewModelAux.OperationNumber;
            operationDataViewModel.OperationTypeId = operationDataViewModelAux.OperationTypeId;
            operationDataViewModel.IsPaperModality = operationDataViewModelAux.IsPaperModality;
            operationDataViewModel.Institutions = operationDataViewModelAux.Institutions;

            LoadReferencesIds(operationDataViewModel);

            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Going to Get Basic Data ");
            var basicDataResponse = GetBasicData(operationNumber, hasRelationsView, operation, operationDataViewModel.GetAttributesBasicResponse, listRepository);
            if (basicDataResponse.IsValid && basicDataResponse.BasicData != null)
            {
                operationDataViewModel.BasicData = basicDataResponse.BasicData;
                if (basicDataResponse.BasicData.CountryCode != null && !basicDataResponse.BasicData.IsRegional)
                {
                    basicDataResponse.BasicData.CountryGroup =
                        _verifyCountryService.GetCountryGroup(basicDataResponse.BasicData.CountryCode).countryGroup;
                }
            }

            operationDataViewModel.GetAttributesBasicResponse = _operationDataService
                .GetAttributesBasicResponse(
                    operationNumber,
                    operationDataViewModel.BasicData.OperationTypeCode,
                    operationDataViewModel.BasicData.HasAbstractLock);
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Finished  Getting Basic Data ");
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Going to Get Responsability Data ");
            var responsabilityResponse = GetResponsabilityData(operationNumber, false);
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Finished  Responsability  Data ");

            if (responsabilityResponse.IsValid)
            {
                operationDataViewModel.ResponsabilityData = responsabilityResponse.ResponsabilityData;
            }

            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Going to IsTabVisibleStrategicAlignment ");
            var strategicAlignmentReponse = IsTabVisibleStrategicAlignment(operationNumber, operation);
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Finished  IsTabVisibleStrategicAlignment ");
            if (strategicAlignmentReponse.IsValid)
            {
                operationDataViewModel.StrategicAlignment = strategicAlignmentReponse.StrategicAlignment;
            }

            return operationDataViewModel;
        }

        public ClimateChangeViewModel GetClimateChangeData(string operationNumber)
        {
            ClimateChangeViewModel climateChangeViewModel = null;
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Going to Get ClimateChange Data ");
            var climateChangeResponse = GetClimateChange(operationNumber);
            Logger.GetLogger().WriteDebug("ViewModelHelper", "OperationData - Method: GetOperationData - Finished  ClimateChange  Data ");
            if (climateChangeResponse.IsValid)
            {
                climateChangeViewModel = climateChangeResponse.ClimateChange;
            }

            return climateChangeViewModel;
        }

        public List<SelectListItem> GetResponsibleUnits(string operationNumber)
        {
            var list = new List<SelectListItem>();
            var listRepository = _operationDataService.GetOperationResponsabilityData(operationNumber, true);

            if (listRepository != null && listRepository.ResponsabilityData.ResponsibleUnits.Count > 0)
            {
                list = listRepository.ResponsabilityData.ResponsibleUnits.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.OrganizationalUnit.ToString(),
                    Text = o.OrganizationalUnitText.ToString()
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetSelectListItemClimateChange(List<ListItemViewModel> climateChangeList)
        {
            var list = climateChangeList.Select(o => new SelectListItem { Selected = false, Value = o.Value, Text = o.Text }).ToList();
            return list;
        }

        public List<SelectListItem> GetSubSectorList(int? idSector, string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository.IsValid && listRepository.MasterDataCollection.Any())
            {
                list = listRepository.MasterDataCollection.Where(x => x.ParentMasterId == idSector).Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = o.NameEn
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetYear(BasicDataViewModel model)
        {
            var listRepository = _calculationOperationYearService.GetOperationYear(model.OperationTypeCode, model.ApprovalDate, model.OperationYear.GetValueOrDefault());

            List<SelectListItem> list = listRepository.Years.Select(o => new SelectListItem
            {
                Selected = false,
                Value = o.ToString(),
                Text = o.ToString()
            }).ToList();

            return list;
        }

        public List<MultiSelectListItem> GetYearOpr(List<int> yearSelected)
        {
            var listRepository = _calculationOperationYearService.GetOperationYear(null, null);

            List<MultiSelectListItem> list = listRepository.Years.Select(o => new MultiSelectListItem
            {
                Selected = yearSelected.Any(x => x.Equals(o)),
                Value = o.ToString(),
                Text = o.ToString()
            }).ToList();

            if (list.Any())
            {
                _viewBag.TextOprYear = GetSelectedItemText(list);
            }

            return list;
        }

        public GetAdditionalDataUserResponse GetAdditionalDataOperationTeam(
            string operationNumber, string userName, string organizationCode, bool hideExpired = false)
        {
            return _crossCreationOperationService
                .GetAdditionalDataOperationTeam(userName, operationNumber, organizationCode, hideExpired);
        }

        public List<decimal> GetCalculationEffortDays(int effort)
        {
            var list = new List<decimal>();
            list.Add(_calculationEffortDaysService.GetEffortDays(effort).Contribution);
            list.Add(_calculationEffortDaysService.GetEffortDaysPercent(effort).ContributionPercent);
            return list;
        }

        public List<SelectListItem> GetCountryGroupList()
        {
            var countryGroupList = new List<SelectListItem>();
            var countryGroupListRepository = _verifyCountryService.GetCountryGroupList();

            if (countryGroupListRepository.CountryGroupList != null && countryGroupListRepository.CountryGroupList.Count > 0)
            {
                countryGroupList = countryGroupListRepository.CountryGroupList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.ToString(),
                    Text = o.ToString()
                }).ToList();
            }

            return countryGroupList;
        }

        public GetDocumentNumberResponse GetOperationDocument(string documentNumber)
        {
            return _documentService.GetDocumentNumber(documentNumber);
        }

        public string GetExpiredMbfItem(string description)
        {
            if (!description.Contains(SEPARATOR))
                return string.Empty;

            var code = description.Split(SEPARATOR)[0];

            var expired = _convergenceMasterDataRepository
                .GetOne(o => o.Code == code && o.ExpirationDate <= DateTime.Now);

            if (expired == null)
                return string.Empty;

            return expired.ConvergenceMasterDataId.ToString();
        }

        public GetStrategicAlignmentResponse IsTabVisibleStrategicAlignment(
            string operationNumber,
            Operation operation)
        {
            var response = _operationDataService.IsTabVisibleStrategicAlignment(operation);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }
        #endregion

        #region Approval

        public void HasApprovalWritePermissions()
        {
            _viewBag.ApprovalWrite = IDBContext.Current.HasPermission("Approval Write");
        }

        public void HasApprovalGcmWritePermissions()
        {
            _viewBag.ApprovalGcmWrite = IDBContext.Current.HasPermission("GCM Write");
            Logger.GetLogger().WriteDebug("ViewModelMapperHelper", "Method: HasApprovalGcmWritePermissions? " + _viewBag.ApprovalGcmWrite);
        }

        public void HasApprovalTLWritePermissions()
        {
            _viewBag.ApprovalTLWrite = IDBContext.Current.HasPermission("Approval TL write");
            Logger.GetLogger().WriteDebug("ViewModelMapperHelper", "Method: HasApprovalTLWritePermissions? " + _viewBag.ApprovalGcmWrite);
        }

        public void SetViewBagApprovalOperation()
        {
            string[] listTypes = { "CURRENCY" };
            var currencies = _catalogService
                .GetMasterDataListByTypeCode(typeCodes: listTypes)
                .MasterDataCollection
                .OrderBy(x => x.NameEn);

            var fundCurrencyList = currencies.Select(currency => new SelectListItem
            {
                Value = currency.MasterId.ToString(),
                Text = currency.NameEn
            }).ToList();

            _viewBag.FundCurrencyList = fundCurrencyList;
        }

        public Dictionary<string, string> AssigneeIdbAmount(string idbamount)
        {
            Dictionary<string, string> idbamounts = new Dictionary<string, string>();

            var pairs = idbamount.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var values = pair.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                idbamounts.Add(values[0], values[1]);
            }

            return idbamounts;
        }

        public Dictionary<string, string> AssigneeExecutor(string executor)
        {
            Dictionary<string, string> executors = new Dictionary<string, string>();
            var pairs = executor.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var values = pair.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                executors.Add(values[0], values[1]);
            }

            return executors;
        }

        public Dictionary<string, string> CalculateCurrency(string currency)
        {
            Dictionary<string, string> currencies = new Dictionary<string, string>();
            var pairs = currency.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var values = pair.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                currencies.Add(values[0], values[1]);
            }

            return currencies;
        }

        public int? ValidateSuffix(string suffix)
        {
            int? validatedSuffix = null;

            if (!string.IsNullOrEmpty(suffix))
            {
                validatedSuffix = Convert.ToInt32(suffix);
            }

            return validatedSuffix;
        }

        public GetSelectListItemResponse GetApprovalNumberList(string search)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();

            var listRepository = _approvalOperationService.GetListApprovalNumber(search);

            if (listRepository != null && listRepository.Any())
            {
                if (listRepository.Any())
                {
                    response.ListResponse = listRepository.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.ApprovalNumber.ToString(),
                        Text = o.ApprovalNumber.ToString(),
                    }).ToList();
                }
            }

            return response;
        }

        public GetAttributesApprovalResponse GetAttributesApproval(
            string operationNumber, ReformulationViewModel reformulation)
        {
            List<string> listCode = new List<string>();

            listCode.Add(APPROVAL_AUTH);
            listCode.Add(APPROVAL_PROCESS);
            listCode.Add(JUSTIFICATION);

            return _approvalOperationService
            .GetAttributesClassificationData(operationNumber, listCode, reformulation);
        }

        public RequestIncreaseDataResponse GetRequestIncreaseData(bool isFromApproval,
            string operationNumber)
        {
            var request = new RequestIncreaseDataRequest
            {
                IsFromApproval = isFromApproval,
                OperationNumber = operationNumber
            };

            return _approvalIncreasesRevampService.GetRequestIncreaseModal(request);
        }

        public ExpectedIDBResponse GetExpectedIDB(int fundOperationId)
        {
            var request = new ExpectedIDBRequest
            {
                FundOperationId = fundOperationId
            };

            return _approvalIncreasesRevampService.GetExpectedIDB(request);
        }

        public RequestIncreaseDataResponse GetChangeRequestData(bool isFromApproval,
            string operationNumber)
        {
            var request = new RequestIncreaseDataRequest
            {
                IsFromApproval = isFromApproval,
                OperationNumber = operationNumber
            };

            return _approvalIncreasesRevampService.GetChangeIncrease(request);
        }

        public ApprovalIncreaseFundingLineResponse GetApprovalFundingLine(
            int fundOperationIncreaseId,
            int fundOperationId,
            int rowNumber)
        {
            var request = new ApprovalIncreaseFundingLineRequest
            {
                FundOperationIncreaseId = fundOperationIncreaseId,
                FundOperationId = fundOperationId,
                RowNumber = rowNumber
            };

            return _approvalIncreasesRevampService.GetNewTransactionLine(request);
        }

        public ApprovalOperationDocumentResponse NewDocumentApproval(
            string operationNumber,
            string documentNumber,
            string documentName,
            bool isNameEditable)
        {
            var request = new ApprovalOperationDocumentRequest
            {
                OperationNumber = operationNumber
            };

            var response = _approvalOperationService.GetDocumentAdditionalInfo(request);
            if (response.IsValid)
            {
                var newDocumentDetail = new List<ApprovalDocumentDetailViewModel>
                {
                    new ApprovalDocumentDetailViewModel
                    {
                        DocNumber = documentNumber,
                        Description = Path.GetFileNameWithoutExtension(documentName),
                        Date = DateTime.Now.Date,
                        User = IDBContext.Current.UserName,
                        IsEditableDescription = isNameEditable
                    }
                };

                response.Documents.Documents = newDocumentDetail;
            }

            return response;
        }

        public int[] ToIncreasesArray(string increases)
        {
            var increasesList = new List<int>();
            var strIncreasesArray = increases.Split(new[] { '-' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in strIncreasesArray)
            {
                int increaseId = 0;
                if (int.TryParse(item, out increaseId))
                {
                    increasesList.Add(increaseId);
                }
            }

            return increasesList.ToArray();
        }

        public MasterDataNameResponse GetMasterDataNameByCode(string code,
            string type,
            string language)
        {
            return _approvalIncreasesRevampService
                .GetMasterDataNameByCode(new MasterDataNameRequest
                {
                    Code = code,
                    Type = type,
                    Language = language
                });
        }

        public IncreaseRowButtonsViewModel GetIncreaseActionButtons(string operationNumber,
            int fundOperationIncreaseId)
        {
            var actionButtonsResponse = _approvalIncreasesRevampService
                .GetIncreaseActionButtons(new ActionButtonsRequest
                {
                    OperationNumber = operationNumber,
                    FundOperationIncreaseId = fundOperationIncreaseId
                });

            var model = new IncreaseRowButtonsViewModel
            {
                HasApproveIncreasePermission = actionButtonsResponse.HasPermission,
                ValidationStageCode = actionButtonsResponse.ValidationStageCode
            };

            return model;
        }

        public ExecutingAgenciesValidationResponse GetExecutingAgenciesValidation(
            string operationNumber, string executingAgenciesIds)
        {
            return _approvalIncreasesRevampService
                .GetExecutingAgenciesValidation(new ExecutingAgenciesValidationRequest
                {
                    OperationNumber = operationNumber,
                    ExecutingAgenciesIdSelectedList = executingAgenciesIds
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList()
                });
        }

        public void LoadReferencesIds(OpusReferenceValuesViewModel referenceIdsVM)
        {
            var catalogValues = _catalogService.GetMasterDataListByTypeCode(
                false,
                MasterType.YES_NO_OPTIONS,
                MasterType.SECTOR,
                MasterType.SUBSECTOR,
                MasterType.FACILITY_TYPE,
                MasterType.RELATIONSHIP_TYPE,
                MasterType.OPERATION_TYPE,
                MasterType.INSTITUTION_ROLE,
                MasterType.CONMODALITY,
                MasterType.ORG_UNIT_ROLE,
                MasterType.LENDING_TYPE,
                MasterType.TYPE_INSTRUMENTS_FINANCED,
                MasterType.CON_OPERATION_NSG_INSTRUMENTS,
                MasterType.PRG_MODALITY);

            if (!catalogValues.IsValid)
            {
                var except = new ApplicationException(catalogValues.ErrorMessage);
                Logger.GetLogger().WriteError(
                    "ViewModelMapperHelper - LoadReferencesIds",
                    "Error getting reference IDs from Catalog Service",
                    except);

                throw except;
            }

            referenceIdsVM.FinancingYesId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.YES_NO_OPTIONS &&
                    md.Code == AttributeValue.YES)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.SectorOtherId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.SECTOR &&
                    md.Code == OPUSGlobalValues.OT)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.SectorMultipleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.SECTOR &&
                    md.Code == SectorSubsectorCode.MULTISECTOR)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.SubSectorMultipleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.SUBSECTOR &&
                    md.Code == SectorSubsectorCode.MULTISUBSECTOR)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.FacilityTypeSectorialCclipId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.FACILITY_TYPE &&
                    md.Code == AttributeValue.CCLIP_SECTORIAL)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.FacilityTypeMultisectICclipId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.FACILITY_TYPE &&
                    md.Code == AttributeValue.CCLIP_MULTISECTORIAL_I)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.FacilityTypeMultisectIICclipId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.FACILITY_TYPE &&
                    md.Code == AttributeValue.CCLIP_MULTISECTORIAL_II)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.RelationshipTypeCclipSId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.RELATIONSHIP_TYPE &&
                    md.Code == RelationTypeCode.CCLIPS)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.RelationshipTypeCclipRId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.RELATIONSHIP_TYPE &&
                    md.Code == RelationTypeCode.CCLIPR)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ConOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.CON)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ExecutingAgencyRoleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.INSTITUTION_ROLE &&
                    md.Code == InstitutionRoleCode.INST_EX_AGENCY)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ConModalityCreditLineId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.CONMODALITY &&
                    md.Code == AttributeValue.CCL)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ResponsibleRoleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.ORG_UNIT_ROLE &&
                    md.Code == OrgUnitRoleCode.ORG_RESPONSIBLE)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.DisbursRespUnitRoleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.ORG_UNIT_ROLE &&
                    md.Code == OrgUnitRoleCode.ORG_UDR)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.CreditLineExecutingAgencyRoleId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.INSTITUTION_ROLE &&
                    md.Code == InstitutionRoleCode.INST_CRED_LINE_EX_AGENCY)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.LonOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.LON)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.TcpOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                        md.Code == OperationType.TCP)
                    .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.IgrOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.IGR)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.EquOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.EQU)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.GuaOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.GUA)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ConOperatNsgCategSepId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.CON_OPERATION_NSG_INSTRUMENTS &&
                    md.Code == AttributeValue.SEP)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.ConOperatNsgCategMifId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.CON_OPERATION_NSG_INSTRUMENTS &&
                    md.Code == AttributeValue.MIF)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.MifOpuOperatNsgCategSmpId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.TYPE_INSTRUMENTS_FINANCED &&
                    md.Code == AttributeValue.TYPE_INSTRUMENT_SMP)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.MifOpuOperatNsgCategMifId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.TYPE_INSTRUMENTS_FINANCED &&
                    md.Code == AttributeValue.TYPE_INSTRUMENT_MIF_TC)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.LendingTypeNsgId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.LENDING_TYPE &&
                    md.Code == AttributeValue.NSG)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();
            referenceIdsVM.UsIdbInstitutionId = _operationDataService.GetUSIDBInstitutionId();
            referenceIdsVM.UsIICInstitutionId = _operationDataService.GetUSIICInstitutionId();
            referenceIdsVM.OrgUnitIICId = _operationDataService.GetIICOrgUnitId();
            referenceIdsVM.PrgOperationId = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.OPERATION_TYPE &&
                    md.Code == OperationType.PRG)
                .Select(md => md.MasterId).DefaultIfEmpty(0).FirstOrDefault();

            var pppUnavailablePrgModalities = new List<string>
            {
                AttributeValue.CPR,
                AttributeValue.COP,
                AttributeValue.MSA,
                AttributeValue.CDC,
                AttributeValue.NTS,
                AttributeValue.SPR
            };
            referenceIdsVM.PppUnavailablePrgModalityIds = catalogValues.MasterDataCollection
                .Where(md => md.TypeName == MasterType.PRG_MODALITY &&
                    pppUnavailablePrgModalities.Contains(md.Code))
                .Select(md => md.MasterId.ToString()).DefaultIfEmpty(string.Empty)
                .Aggregate((aggr, next) => aggr + ", " + next);
        }
        #endregion

        #region Private Methods
        private static string GetSelectedItemText(IEnumerable<MultiSelectListItem> itemsList)
        {
            var textBuffer = new StringBuilder();

            foreach (var item in itemsList)
            {
                if (item.Selected)
                {
                    textBuffer.AppendFormat("{0}, ", item.Text);
                }
            }

            if (textBuffer.Length > 0)
            {
                textBuffer.Length = textBuffer.Length - 2;
            }

            return textBuffer.ToString();
        }

        private string[] GetValidGroups(string guaranteedBy, string nsgCategorization, string opType, string orgUnit)
        {
            var guaranteedCode = GetCode(guaranteedBy);
            var categorizationCode = GetCode(nsgCategorization);
            var operationCode = GetCode(opType);
            var organizationalUnitCode = GetCode(orgUnit);

            if (organizationalUnitCode != OrgUnitRoleCode.ORG_RESPONSIBLE)
                return null;

            if (operationCode == OperationType.LON &&
               guaranteedCode == AttributeValue.SG)
            {
                return new[] { DivisionOpusGroup.DIVISION, DivisionOpusGroup.VPC };
            }

            var mifOpuOperations = new[]
            {
                OperationType.LON,
                OperationType.TCP,
                OperationType.IGR,
                OperationType.EQU,
                OperationType.GUA
            };

            var mifOpuOperNsgCategorizations = new[]
            {
                AttributeValue.TYPE_INSTRUMENT_SMP,
                AttributeValue.TYPE_INSTRUMENT_MIF_TC
            };
            var conOperNsgCategorizations = new[] { AttributeValue.SEP, AttributeValue.MIF };

            if (_creationFormService.ValidateMifOpuOperation(
                operationCode,
                guaranteedCode,
                categorizationCode))
            {
                return new[] { DivisionOpusGroup.MIF };
            }

            if (operationCode == OperationType.PRG)
            {
                return new[]
                    {
                        DivisionOpusGroup.VPC,
                        DivisionOpusGroup.COUNTRY_DEPARTMENT,
                        DivisionOpusGroup.COUNTRY
                    };
            }

            return null;
        }

        private string GetCode(string param)
        {
            if (string.IsNullOrEmpty(param))
                return string.Empty;

            var masterData = _catalogService
                .GetConvergenceMasterCodeByIdResponse(int.Parse(param));

            return masterData.IsValid ? masterData.Code : string.Empty;
        }
        #endregion
    }
}