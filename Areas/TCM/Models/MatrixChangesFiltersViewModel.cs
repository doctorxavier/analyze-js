using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.TCM.Models
{
    [Serializable]
    public class MatrixChangesFiltersViewModel
    {
        public List<SelectListItem> FilterTCReportingPeriodList { get; set; }

        public int CurrentTCReportingPeriod { get; set; }

        public List<SelectListItem> FilterSectionList { get; set; }
    }
}