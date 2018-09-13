using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.SGP.Mappers
{
    public static class SelectListItemMapper
    {
        public static SelectListItem ConvertToSelectListItems(this MasterDataViewModel item, string language = null, bool code = false)
        {
            if (language == null)
            {
                language = Localization.CurrentLanguage;
            }

            var result = new SelectListItem()
            {
                Value = item.MasterId.ToString(),
                Text = code ? item.Code : item.GetLocalizedName(language)
            };
            return result;
        }

        public static List<SelectListItem> ConvertToSelectListItems(this IEnumerable<MasterDataViewModel> list, bool code = false)
        {
            var language = Localization.CurrentLanguage;
            var result = list.Select(x => x.ConvertToSelectListItems(language, code));
            return result.ToList();
        }

        public static SelectListItem ConvertCodeToSelectListItems(this MasterDataViewModel item)
        {
            var result = new SelectListItem()
            {
                Value = item.MasterId.ToString(),
                Text = item.Code
            };
            return result;
        }
        
        public static List<SelectListItem> ConvertCodeToSelectListItems(this IEnumerable<MasterDataViewModel> list)
        {
            var result = list.Select(x => x.ConvertCodeToSelectListItems());
            return result.ToList();
        }

        public static SelectListItem ConvertToSelectListItem(this ListItemViewModel item)
        {
            var result = new SelectListItem()
            {
                Value = item.Value.ToString(),
                Text = item.Text,
                Selected = item.Select
            };
            return result;
        }

        public static List<SelectListItem> ConvertToSelectListItems(this IEnumerable<ListItemViewModel> list)
        {
            var result = list.Select(x => x.ConvertToSelectListItem());
            return result.ToList();
        }
    }
}