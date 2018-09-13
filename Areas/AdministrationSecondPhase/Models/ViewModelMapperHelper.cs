using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.DelegationService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models
{
    public class ViewModelMapperHelper
    {
        #region fields
        private readonly ICatalogService _catalogService;
        private readonly IDelegationService _delegationService;
        #endregion

        public ViewModelMapperHelper(ICatalogService catalogService,
            IDelegationService delegationService)
        {
            _catalogService = catalogService;
            _delegationService = delegationService;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(typeCodes: type);

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
    }
}