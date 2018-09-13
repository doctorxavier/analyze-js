using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.MasterDataService;
using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.AdministrationModule.ViewModels.MasterData;
using IDB.MW.Application.Core.Messages;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.MasterData
{
    public class ViewModelMapperHelper
    {
        #region fields
        private readonly ICatalogService _catalogService;
        private readonly IMasterDataService _masterDataService;
        #endregion

        public ViewModelMapperHelper(ICatalogService catalogService, IMasterDataService masterDataService)
        {
            _catalogService = catalogService;
            _masterDataService = masterDataService;
        }

        public static void FillDefaultOverallStage(
            GetMasterDataListByTypeIdResponse masterDataRepository,
            IList<MultiSelectListItem> overallStageList)
        {
            var defaultOverallStages = masterDataRepository.MasterDataCollection
                .Where(x =>
                    x.NameEn == SearchValues.OVERALLSTAGE_ORDER_1 ||
                    x.NameEn.StartsWith(SearchValues.OVERALLSTAGE_ORDER_2));

            foreach (var overall in defaultOverallStages)
            {
                overallStageList.Add(new MultiSelectListItem
                {
                    Value = overall.MasterId.ToString(),
                    Text = overall.NameEn
                });
            }

            overallStageList.Add(new MultiSelectListItem
            {
                Text = SearchValues.OVERALL_STAGE_PLACEHOLDER
            });
        }

        public List<TableMasterDataTypeViewModel> GetMasterDataTypeList(string name)
        {
            var response = new GetListItemMasterDataTypeResponse();

            var masterDataRepository = _masterDataService.GetListItemMasterDataType(name);

            if (masterDataRepository.GetListItemMasterDataType != null && masterDataRepository.GetListItemMasterDataType.Count > 0)
            {
                var listitemmasterDataType = masterDataRepository.GetListItemMasterDataType.Select(o => new TableMasterDataTypeViewModel
                {
                    MasterTypeId = o.MasterTypeId,
                    MasterDataType = o.MasterDataType,
                    EffectiveDate = o.EffectiveDate,
                    ExpirationDate = o.ExpirationDate,
                    Type = o.Type
                }).ToList();

                response.GetListItemMasterDataType = listitemmasterDataType.OrderBy(c => c.Type).ToList();
            }

            return response.GetListItemMasterDataType;
        }

        public List<TableMasterDataTypeViewModel> GetMasterDataTypeListByFilter(string filter)
        {
            var response = new GetListItemMasterDataTypeResponse();

            var masterDataRepository = _masterDataService.GetListItemMasterDataTypeByFilter(filter);

            if (masterDataRepository.GetListItemMasterDataType != null && masterDataRepository.GetListItemMasterDataType.Count > 0)
            {
                response.GetListItemMasterDataType = masterDataRepository.GetListItemMasterDataType.Select(o => new TableMasterDataTypeViewModel
                {
                    MasterTypeId = o.MasterTypeId,
                    MasterDataType = o.MasterDataType,
                    EffectiveDate = o.EffectiveDate,
                    ExpirationDate = o.ExpirationDate,
                    Type = o.Type
                }).OrderBy(c => c.Type).ToList();
            }

            return response.GetListItemMasterDataType;
        }

        public List<MasterDataManagementTableModelView> GetMasterDataListByFilter(string filter, string masterType)
        {
            var response = new GetListMasterDataManagementResponse();
            int masterTypeId;
            int.TryParse(masterType, out masterTypeId);

            var masterDataRepository = _masterDataService.GetListMasterDataManagement(filter);

            if (masterDataRepository.GetListMasterDataManagement != null && masterDataRepository.GetListMasterDataManagement.Count > 0)
            {
                response.GetListMasterDataManagement = masterDataRepository.GetListMasterDataManagement.Select(o => new MasterDataManagementTableModelView
                {
                    MasterDataId = o.MasterDataId,
                    MasterDataTypeId = o.MasterDataTypeId,
                    EffectiveDate = o.EffectiveDate,
                    ExpirationDate = o.ExpirationDate,
                    EnglishName = o.EnglishName,
                    SpanishName = o.SpanishName,
                    FrenchName = o.FrenchName,
                    PortugueseName = o.PortugueseName,
                    ParentMasterData = o.ParentMasterData,
                    Code = o.Code
                }).ToList().Where(o => o.MasterDataTypeId.Equals(masterTypeId)).ToList();
            }

            return response.GetListMasterDataManagement;
        }

        public List<MasterDataManagementTableModelView> GetMasterDataListByFilter(string filter)
        {
            var response = new GetListMasterDataManagementResponse();

            var masterDataRepository = _masterDataService.GetListMasterDataManagement(filter);

            if (masterDataRepository.GetListMasterDataManagement != null && masterDataRepository.GetListMasterDataManagement.Count > 0)
            {
                response.GetListMasterDataManagement = masterDataRepository.GetListMasterDataManagement.Select(o => new MasterDataManagementTableModelView
                {
                    MasterDataId = o.MasterDataId,
                    MasterDataTypeId = o.MasterDataTypeId,
                    EffectiveDate = o.EffectiveDate,
                    ExpirationDate = o.ExpirationDate,
                    EnglishName = o.EnglishName,
                    SpanishName = o.SpanishName,
                    FrenchName = o.FrenchName,
                    PortugueseName = o.PortugueseName,
                    ParentMasterData = o.ParentMasterData,
                    Code = o.Code
                }).ToList();
            }

            return response.GetListMasterDataManagement;
        }

        public List<MasterDataManagementTableModelView> GetMasterDataListFilter(string filter)
        {
            var response = new GetListMasterDataManagementResponse();

            var masterDataRepository = _masterDataService.GetListItemMasterDataByFilter(filter);

            if (masterDataRepository.GetListMasterDataManagement != null && masterDataRepository.GetListMasterDataManagement.Count > 0)
            {
                response.GetListMasterDataManagement = masterDataRepository.GetListMasterDataManagement.Select(o => new MasterDataManagementTableModelView
                {
                    MasterDataId = o.MasterDataId,
                    MasterDataTypeId = o.MasterDataTypeId,
                    EffectiveDate = o.EffectiveDate,
                    ExpirationDate = o.ExpirationDate,
                    EnglishName = o.EnglishName,
                    SpanishName = o.SpanishName,
                    FrenchName = o.FrenchName,
                    PortugueseName = o.PortugueseName,
                    ParentMasterData = o.ParentMasterData,
                    Code = o.Code
                }).ToList();
            }

            return response.GetListMasterDataManagement;
        }

        public GetSelectListItemResponse GetMasterTypes(string name, bool full = false)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();
            var masterTypes = GetMasterDataTypeList(name);
            if (masterTypes != null && masterTypes.Count > 0)
            {
                response.ListResponse = masterTypes.Select(o => new SelectListItem
                {
                    Selected = false,
                    Text = o.Type,
                    Value = o.MasterTypeId.ToString()
                }).ToList();
            }

            return response;
        }

        public GetSelectListItemResponse GetMasterDatas(string name, string masterTypeId, string masterDataActualId)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();
            var masterDatasAux = GetMasterDataListByFilter(name);
            if (masterDatasAux != null)
            {
                var masterDatas = masterDatasAux.Where(o => o.MasterDataTypeId.ToString().Equals(masterTypeId) && !o.MasterDataId.ToString().Equals(masterDataActualId)).ToList();
                if (masterDatas != null && masterDatas.Count > 0)
                {
                    response.ListResponse = masterDatas.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Text = o.Code,
                        Value = o.MasterDataId.ToString()
                    }).ToList();
                }
            }

            return response;
        }

        public GetSelectListItemResponse GetMasterDatasFil(string name, bool full = false)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();
            var masterDatas = GetMasterDataListFilter(name);
            if (masterDatas != null && masterDatas.Count > 0)
            {
                response.ListResponse = masterDatas.Select(o => new SelectListItem
                {
                    Selected = false,
                    Text = o.Code,
                    Value = o.MasterDataId.ToString()
                }).ToList();
            }

            return response;
        }

        public TableMasterDataTypeViewModel GetMasterDataByType(string masterTypeId)
        {
            var response = new GetMasterDataByTypeIdResponse();
            var masterDataRepository = _masterDataService.GetMasterDataByTypeId(masterTypeId);

            if (masterDataRepository.GetMasterDataByTypeId != null)
            {
                response.GetMasterDataByTypeId = masterDataRepository.GetMasterDataByTypeId;
            }

            return response.GetMasterDataByTypeId;
        }
    }
}