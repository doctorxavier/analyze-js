using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public class ViewModelMapperHelper
    {
        private readonly ICatalogService _catalogService;

        public ViewModelMapperHelper(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetListMasterData(int type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeId(masterTypeIds: type);

            if (listRepository != null && listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> RemoveUsedRole(List<ValidatorViewModel> validators, List<SelectListItem> roles)
        {
            foreach (var validator in validators)
            {
                roles.RemoveAll(x => x.Text == validator.Role);
            }

            return roles;
        }
    }
}