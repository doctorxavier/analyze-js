using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

using IDB.MW.Application.Core.ViewModels;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Models;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers
{
    public static class MultiDropDownMapper
    {
        #region Mappers

        public static MultiDropDownItem ConvertToMultiDropDownItem(this MasterDataViewModel model)
        {
            var item = new MultiDropDownItem()
            {
                Value = model.MasterId.ToString(),
                Text = model.GetLocalizedName()
            };
            return item;
        }

        public static List<MultiDropDownItem> ConvertToMultiDropDownItems(this List<MasterDataViewModel> models)
        {
            return models.Select(x => x.ConvertToMultiDropDownItem()).ToList();
        }

        public static MultiDropDownItem ConvertToMultiDropDownItem(this SelectListItem model)
        {
            var result = new MultiDropDownItem()
            {
                Value = model.Value,
                Text = model.Text
            };
            return result;
        }

        public static List<MultiDropDownItem> ConvertToMultiDropDownItems(this List<SelectListItem> models)
        {
            return models.Select(x => x.ConvertToMultiDropDownItem()).ToList();
        }
        #endregion
    }
}