using System.Collections.Generic;

using IDB.MW.Application.CountryStrategyModule.ViewModels.UseCountry;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Models
{
    public class CSStytemRecordTableViewModel
    {
        public CSStytemRecordTableViewModel(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<CSStytemRecordRowViewModel> CSStytemRecordRowViewModels { get; set; }

        public CSStytemRecordTableViewModel SetCSStytemRecordRowViewModels(List<CSStytemRecordRowViewModel> cSStytemRecordRowViewModels)
        {
            CSStytemRecordRowViewModels = cSStytemRecordRowViewModels;
            return this;
        }
    }
}