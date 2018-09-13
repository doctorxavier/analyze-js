using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.MasterDataService;
using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.AdministrationModule.ViewModels.MasterData;
using IDB.MW.Application.AttributesModule.Messages.AttributesService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.AttributesModule.Services.ConvergencePermissionServices;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Attribute
{
    public class ViewModelMapperHelper
    {
        private readonly IAttributesManagementService _attributesEngineService;
        private readonly ICatalogService _catalogService;
        private readonly IMasterDataService _masterDataService;
        private readonly IConvergencePermissionService _convergencePermissionService;
        private readonly IBreRuleService _breRuleService;
        private dynamic _viewBag;

        public ViewModelMapperHelper(
            dynamic viewBag,
            IAttributesManagementService attributesEngineService,
            ICatalogService catalogService,
            IMasterDataService masterDataService,
            IConvergencePermissionService convergencePermissionService,
            IBreRuleService breRuleService)
        {
            _viewBag = viewBag;
            _attributesEngineService = attributesEngineService;
            _catalogService = catalogService;
            _masterDataService = masterDataService;
            _convergencePermissionService = convergencePermissionService;
            _breRuleService = breRuleService;
        }

        public GetAttributeEditResponse GetAttributeEdit(int attributeId)
        {
            var response = _attributesEngineService.GetAttributeEdit(attributeId);
            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public List<SelectListItem> GetColumnSize(int minValue, int maxValue)
        {
            var list = new List<SelectListItem>();
            for (var i = minValue; i <= maxValue; i++)
            {
                list.Add(new SelectListItem
                {
                    Text = i + string.Empty,
                    Value = i + string.Empty
                });
            }

            return list;
        }

        public List<SelectListItem> GetLenghtList()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "160", Text = "160 Characters" });
            list.Add(new SelectListItem { Value = "200", Text = "200 Characters" });
            list.Add(new SelectListItem { Value = "500", Text = "500 Characters" });
            list.Add(new SelectListItem { Value = "1000", Text = "1000 Characters" });

            return list;
        }

        public GetListAttributeResponse SearchAttributeListResponse(int searchAttributeId, int searchOperationType, int searchRelationType, int searchSection, bool searchExpirationDate)
        {
            var response = _attributesEngineService.GetAttributeList(searchAttributeId, searchOperationType, searchRelationType, searchSection, searchExpirationDate);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetSelectListItemResponse GetAttributeList(string name)
        {
            var response = new GetSelectListItemResponse();

            var attributeRepository = _attributesEngineService.GetAttributeNameList(name);

            if (attributeRepository.GetListItemAttribute != null && attributeRepository.GetListItemAttribute.Count > 0)
            {
                response.ListResponse = attributeRepository.GetListItemAttribute.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public List<TableMasterDataTypeViewModel> GetMasterDataTypeList(string name)
        {
            var response = new GetListItemMasterDataTypeResponse();

            var masterDataRepository = _masterDataService.GetListItemMasterDataType(name);

            if (masterDataRepository.GetListItemMasterDataType != null &&
                masterDataRepository.GetListItemMasterDataType.Count > 0)
            {
                response.GetListItemMasterDataType =
                    masterDataRepository.GetListItemMasterDataType.Select(o => new TableMasterDataTypeViewModel
                    {
                        MasterTypeId = o.MasterTypeId,
                        Type = o.Type
                    }).ToList();
            }

            return response.GetListItemMasterDataType;
        }

        public GetSelectListItemResponse GetMasterTypes(string name)
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

        public List<SelectListItem> GetConvergencePermissionsList()
        {
            var list = new List<SelectListItem>();
            var listRepository = _convergencePermissionService.GetConvergencePermissions();

            if (listRepository != null && listRepository.ConvergencePermissionViewModel != null &&
                listRepository.ConvergencePermissionViewModel.Count > 0)
            {
                list = listRepository.ConvergencePermissionViewModel.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.ConvergencePermissionId.ToString(),
                    Text = o.Name
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetBreRuleList()
        {
            var list = new List<SelectListItem>();
            var listRepository = _breRuleService.GetBreRule();

            if (listRepository != null && listRepository.BreRuleView != null &&
                listRepository.BreRuleView.Count > 0)
            {
                list = listRepository.BreRuleView.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.CodeRule.ToString(),
                    Text = o.NameRule,
                }).ToList();
            }

            return list;
        }
    }
}