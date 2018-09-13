using System.Collections.Generic;

using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class SimpleLevelAttributeViewModel
    {
        public SimpleLevelAttributeViewModel()
        {
            Categories = new List<AttributeCategoryViewModel>();
        }

        public SimpleLevelAttributeViewModel(List<AttributeCategoryViewModel> categories)
        {
            Categories = categories;
        }

        public string Name { get; set; }
        public string TitleKey { get; set; }
        public string NewButtonKey { get; set; }
        public string ExpReg { get; set; }
        public List<AttributeCategoryViewModel> Categories { get; set; }
        public bool ShowNotificationDelete { get; set; }
        public bool ShowOrderColumn { get; set; }
        public bool LanguageEnRequired { get; set; }
        public bool LanguageEsRequired { get; set; }
        public bool LanguageFrRequired { get; set; }
        public bool LanguagePtRequired { get; set; }

        public SimpleLevelAttributeViewModel SetName(string name)
        {
            Name = name;
            return this;
        }

        public SimpleLevelAttributeViewModel SetTitleKey(string key)
        {
            TitleKey = key;
            return this;
        }

        public SimpleLevelAttributeViewModel SetExpReg(string expReg)
        {
            ExpReg = expReg;
            return this;
        }

        public SimpleLevelAttributeViewModel SetNewButtonKey(string key)
        {
            NewButtonKey = key;
            return this;
        }

        public SimpleLevelAttributeViewModel SetLanguageEnRequired(bool required)
        {
            LanguageEnRequired = required;
            return this;
        }

        public SimpleLevelAttributeViewModel SetLanguageEsRequired(bool required)
        {
            LanguageEsRequired = required;
            return this;
        }

        public SimpleLevelAttributeViewModel SetLanguageFrRequired(bool required)
        {
            LanguageFrRequired = required;
            return this;
        }

        public SimpleLevelAttributeViewModel SetLanguagePtRequired(bool required)
        {
            LanguagePtRequired = required;
            return this;
        }

        public SimpleLevelAttributeViewModel SetShowNotificationDelete(bool showNotificationDelete = true)
        {
            ShowNotificationDelete = showNotificationDelete;
            return this;
        }

        public SimpleLevelAttributeViewModel SetShowOrderColumn(bool showOrderColumn = true)
        {
            ShowOrderColumn = showOrderColumn;
            return this;
        }
    }
}