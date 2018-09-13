using System.Collections.Generic;

using IDB.MW.Domain.Models.Common;

namespace IDB.Presentation.MVC4.Areas.Administration.Models.PredefinedIndicators
{
    public class DefaultSearchModel
    {
        public List<SimpleI18Resource> IndicatorTypes { get; set; }

        public List<SimpleI18Resource> PriorityAreas { get; set; }

        public List<SimpleI18Resource> ResultFrameworks { get; set; }

        public string IndicatorNumber { get; set; }

        public string IndicatorName { get; set; }

        public string UnitOfMeasure { get; set; }

        public bool DisplayExpired { get; set; }

        public int CurrentIndicatorid { get; set; }

        public string Module { get; set; }
    }
}