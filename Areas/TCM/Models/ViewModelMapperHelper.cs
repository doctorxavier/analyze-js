using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.TCM.Services.FindingAndRecommendationService;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Common;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.MatrixChanges;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.VerifyContent;
using IDB.MW.Business.TCM.DTOs;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Models.Security;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.TCM.Models
{
    public class ViewModelMapperHelper
    {
        #region

        private readonly IFindingRecommendationService _findingRecomendationService;
        private readonly ICatalogService _catalogService;
        private readonly ISecurityModelRepository _securityModelRepository;

        #endregion

        public ViewModelMapperHelper(ICatalogService catalogService, IFindingRecommendationService findingRecomendationService)
        {
            _findingRecomendationService = findingRecomendationService;
            _catalogService = catalogService;
            if (_securityModelRepository == null)
            {
                _securityModelRepository = new SecurityModelRepository();
            }
        }

        public List<SelectListItem> GetPmrList(string operationNumber)
        {
            var pmrList = new List<SelectListItem>();
            var pmr = _findingRecomendationService.GetPmr(operationNumber);

            if (pmr.PmrCycleList != null && pmr.PmrCycleList.Count > 0)
            {
                pmrList = pmr.PmrCycleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return pmrList;
        }

        public List<FieldAccessModel> GetFieldAccessModelList(string operationNumber, string pageChart)
        {
            var FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                pageChart,
                IDBContext.Current.Permissions,
                0,
                0).ToList();
            return FieldAccessList;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetListNationality(string type)
        {
            var types = new string[] { type, TCMGlobalValues.NATIONALITY_ALL };
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, types);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Where(o => o.Code != "UND" && o.Code != "AC" && o.NameEn != "HQ" && o.NameEn != "IDB" && o.NameEn != "HEADQUARTERS WASH. D.C.").Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).Distinct().ToList();
            }

            return list;
        }

        public List<SelectListItem> GetListMaster(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Code.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).OrderByDescending(o => o.Text).ToList();
            }

            return list;
        }

        #region MatrixChanges

        public MatrixChangesViewModel ConvertMatrixChangesViewModel(MatrixChangesDTO matrixChangesDTO)
        {
            var viewModel = new MatrixChangesViewModel();
            if (matrixChangesDTO != null)
            {
                var filterPeriod = matrixChangesDTO.FilterTCReportingPeriod.Select(i => new SelectListItem
                {
                    Selected = false,
                    Value = i.Item1.ToString(),
                    Text = i.Item2
                }).ToList();

                var filterSections = GetSelectListItemList(matrixChangesDTO.FilterSection);

                var filters = new MatrixChangesFiltersViewModel
                {
                    CurrentTCReportingPeriod = matrixChangesDTO.CurrentTCReportingPeriod,
                    FilterTCReportingPeriodList = filterPeriod,
                    FilterSectionList = filterSections
                };

                viewModel = new MatrixChangesViewModel
                {
                    IsRecipientExecutedTCs = matrixChangesDTO.IsRecipientExecutedTCs,
                    ResultsMatrixId = matrixChangesDTO.ResultsMatrixId,
                    CurrentTCReportingPeriod = matrixChangesDTO.CurrentTCReportingPeriod,
                    OperationInfo = ConvertOperationInfoDtoFromViewModel(matrixChangesDTO.OperationInfo),
                    MatrixChangesList = matrixChangesDTO.MatrixChanges,
                    Filters = filters,
                    IsEditable = matrixChangesDTO.IsEditable,
                    IsCndOperation = matrixChangesDTO.IsCndOperation
                };
            }

            return viewModel;
        }

        public List<SelectListItem> GetMatrixChangesTypeFilter(List<MasterDataViewModel> typeList)
        {
            return GetSelectListItemList(typeList);
        }

        public List<SelectListItem> GetMatrixChangesSubTypeFilter(List<MasterDataViewModel> subTypeList)
        {
            return GetSelectListItemList(subTypeList);
        }

        #endregion

        public VerifyContentViewModel ConvertVerifyContentViewModel(VerifyContentModel verifyContentModel)
        {
            var viewModel = new VerifyContentViewModel { Verifications = verifyContentModel.Verifications };

            return viewModel;
        }

        public List<SelectListItem> GetSelectListItemCategories(List<ListItemViewModel> listResponse)
        {
            var list = listResponse.Select(o => new SelectListItem { Selected = false, Value = o.Value, Text = o.Text }).ToList();
            return list;
        }

        #region Private Methods

        private OperationInfoViewModel ConvertOperationInfoDtoFromViewModel(OperationInfoDTO operationInfoDto)
        {
            var operationInfoViewModel = new OperationInfoViewModel
            {
                OperationNumber = operationInfoDto.OperationNumber,
                ReportingPeriod = operationInfoDto.ReportingPeriod,
                ValidationStatus = operationInfoDto.ValidationStatus,
                LastUpdate = operationInfoDto.LastUpdate,
                SubmittedByTeamLeader = operationInfoDto.SubmittedByTeamLeader,
                AcceptedByFundCoordinator = operationInfoDto.AcceptedByFundCoordinator,
                OperationObjective = operationInfoDto.OperationObjective,
                DaysRemaning = operationInfoDto.DaysRemaning
            };

            return operationInfoViewModel;
        }

        private List<SelectListItem> GetSelectListItemList(IEnumerable<MasterDataViewModel> viewModelList)
        {
            var list = viewModelList.Select(i => new SelectListItem
            {
                Selected = false,
                Value = i.MasterId.ToString(),
                Text = i.GetLocalizedName()
            }).ToList();

            return list;
        }

        #endregion
    }
}