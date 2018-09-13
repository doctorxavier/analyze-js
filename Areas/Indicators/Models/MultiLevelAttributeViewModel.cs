using System.Collections.Generic;

using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.Presentation.MVC4.Areas.Indicators.Enums;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class MultiLevelAttributeViewModel
    {
        public MultiLevelAttributeViewModel()
        {
            Levels = new List<AttributeLevelViewModel>();
        }

        public MultiLevelAttributeViewModel(List<AttributeLevelViewModel> levels)
        {
            Levels = levels;
        }

        public string Name { get; set; }
        public string TitleKey { get; set; }
        public string LevelKey { get; set; }
        public string CategoryKey { get; set; }
        public string NewLevelButtonKey { get; set; }
        public string NewCategoryButtonKey { get; set; }
        public bool SubTitle { get; set; }
        public bool ShowNotificationDelete { get; set; }
        public LanguageTypeEnum LanguageRequired { get; set; }

        public List<AttributeLevelViewModel> Levels { get; set; }

        public MultiLevelAttributeViewModel SetName(string name)
        {
            Name = name;
            return this;
        }

        public MultiLevelAttributeViewModel SetTitleKey(string key)
        {
            TitleKey = key;
            return this;
        }

        public MultiLevelAttributeViewModel SetLevelKey(string key)
        {
            LevelKey = key;
            return this;
        }

        public MultiLevelAttributeViewModel SetCategoryKey(string key)
        {
            CategoryKey = key;
            return this;
        }

        public MultiLevelAttributeViewModel SetNewLevelButtonKey(string key)
        {
            NewLevelButtonKey = key;
            return this;
        }

        public MultiLevelAttributeViewModel SetNewCategoryButtonKey(string key)
        {
            NewCategoryButtonKey = key;
            return this;
        }

        public MultiLevelAttributeViewModel SetSubTitle(bool type)
        {
            SubTitle = type;
            return this;
        }

        public MultiLevelAttributeViewModel SetLanguageRequired(LanguageTypeEnum language)
        {
            LanguageRequired |= language;
            return this;
        }

        public MultiLevelAttributeViewModel SetShowNotificationDelete(bool showNotificationDelete = true)
        {
            ShowNotificationDelete = showNotificationDelete;
            return this;
        }
    }
}