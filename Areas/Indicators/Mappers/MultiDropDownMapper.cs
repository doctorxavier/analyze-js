using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.Presentation.MVC4.Areas.Indicators.Models;

namespace IDB.Presentation.MVC4.Areas.Indicators.Mappers
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

        public static MultiDropDownItem ConvertToMultiDropDownItem(this AttributeLevelViewModel model)
        {
            var item = new MultiDropDownItem()
            {
                Value = model.LevelId.ToString(),
                Text = model.GetLocalizedName()
            };
            item.Childrens = model.Categories.ConvertToMultiDropDownItems();
            return item;
        }

        public static List<MultiDropDownItem> ConvertToMultiDropDownItems(this List<AttributeLevelViewModel> models)
        {
            return models.Where(x => x.IsEnabled).Select(x => x.ConvertToMultiDropDownItem()).ToList();
        }

        public static MultiDropDownItem ConvertToMultiDropDownItem(this AttributeCategoryViewModel model)
        {
            var item = new MultiDropDownItem()
            {
                Value = model.CategoryId.ToString(),
                Text = model.GetLocalizedName()
            };
            return item;
        }

        public static List<MultiDropDownItem> ConvertToMultiDropDownItems(this List<AttributeCategoryViewModel> models)
        {
            return models.Where(x => x.IsEnabled).Select(x => x.ConvertToMultiDropDownItem()).ToList();
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