using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    public class ClimateTableViewModel
    {
        public int CCESId { get; set; }

        public decimal OriginalIDBAmount { get; set; }

        public int ClimateChangeMitigationSubId { get; set; }

        public int ClimateChangeAdaptationSubId { get; set; }

        public int EnvironmentId { get; set; }

        public int DisasterRiskManagmentId { get; set; }

        public int SocialSustainabilitySubId { get; set; }

        public string JustificationUser { get; set; }

        public DateTime JustificationDate { get; set; }

        public string Justification { get; set; }
    }
}