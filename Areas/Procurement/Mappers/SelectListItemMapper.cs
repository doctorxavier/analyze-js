using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.ViewModels;
using IDB.Presentation.MVC4.Areas.BEOProcurement.Components;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Mappers
{
    public static class SelectListItemMapper
    {
        public static SelectListItem ConvertToSelectListItems(this MasterDataViewModel item, string language = null)
        {
            if (language == null)
            {
                language = Localization.CurrentLanguage;
            }

            var result = new SelectListItem()
            {
                Value = item.MasterId.ToString(),
                Text = item.GetLocalizedName(language)
            };
            return result;
        }

        public static List<SelectListItem> ConvertToSelectListItems(this IEnumerable<MasterDataViewModel> list)
        {
            var result = new List<SelectListItem>();
            var language = Localization.CurrentLanguage;

            if (list != null)
            {
                result = list.Select(x => x.ConvertToSelectListItems(language)).ToList();
            }
            
            return result.ToList();
        }

        public static MultiDropDownItem ConvertToMultiDropDownItem(this SelectListItem item)
        {
            var result = new MultiDropDownItem()
            {
                Text = item.Text,
                Value = item.Value
            };

            return result;
        }

        public static List<MultiDropDownItem> ConvertToMultiDropDownItems(this IEnumerable<SelectListItem> items)
        {
            var result = items.Select(x => x.ConvertToMultiDropDownItem());
            return result.ToList();
        }
    }
}