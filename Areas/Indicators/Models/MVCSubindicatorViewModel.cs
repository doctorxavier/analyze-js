using System.Collections.Generic;

using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class MVCSubindicatorViewModel
    {
        public MVCSubindicatorViewModel()
        {
            Subindicators = new List<SubindicatorViewModel>();
        }

        public MVCSubindicatorViewModel(List<SubindicatorViewModel> subindicators)
        {
            Subindicators = subindicators;
        }

        public string Name { get; set; }
        public string TitleKey { get; set; }
        public string DescriptionKey { get; set; }
        public string NewButtonKey { get; set; }
        public List<SubindicatorViewModel> Subindicators { get; set; }
        public bool SubTitle { get; set; }
        public bool LanguageEnRequired { get; set; }
        public bool LanguageEsRequired { get; set; }
        public bool LanguageFrRequired { get; set; }
        public bool LanguagePtRequired { get; set; }

        public MVCSubindicatorViewModel SetName(string name)
        {
            Name = name;
            return this;
        }

        public MVCSubindicatorViewModel SetTitleKey(string key)
        {
            TitleKey = key;
            return this;
        }

        public MVCSubindicatorViewModel SetNewButtonKey(string key)
        {
            NewButtonKey = key;
            return this;
        }

        public MVCSubindicatorViewModel SetDescriptionKey(string key)
        {
            DescriptionKey = key;
            return this;
        }

        public MVCSubindicatorViewModel SetSubTitle(bool type)
        {
            SubTitle = type;
            return this;
        }

        public MVCSubindicatorViewModel SetLanguageEnRequired(bool required)
        {
            LanguageEnRequired = required;
            return this;
        }

        public MVCSubindicatorViewModel SetLanguageEsRequired(bool required)
        {
            LanguageEsRequired = required;
            return this;
        }

        public MVCSubindicatorViewModel SetLanguageFrRequired(bool required)
        {
            LanguageFrRequired = required;
            return this;
        }

        public MVCSubindicatorViewModel SetLanguagePtRequired(bool required)
        {
            LanguagePtRequired = required;
            return this;
        }
    }
}