using System.Collections.Generic;

using IDB.MW.Domain.Models.ResultMatrix;
using IDB.MW.Domain.Models.ResultMatrix.PredefinedIndicator;

namespace IDB.Presentation.MVC4.Areas.Administration.Models.PredefinedIndicators
{
    public class CustomSearchPredIndicatorModel
    {
        public PredefinedIndicatorFilter Filter { get; set; }

        public List<PredefinedIndicatorModel> Indicators { get; set; }
    }
}