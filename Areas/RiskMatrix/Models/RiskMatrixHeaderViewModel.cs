using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix.Models
{
    public class RiskMatrixHeaderViewModel
    {
        public RiskMatrixHeaderViewModel()
        {
            ExecutingAgencies = new List<string>();
        }

        public string PmrCycle { get; set; }
        public string EsgClassfication { get; set; }
        public string PmrValidationStage { get; set; }
        public IList<string> ExecutingAgencies { get; set; }
        public DateTime? LastUpdatedRisk { get; set; }
        public string PmrValidatedClassification { get; set; }
        public string SafeguardPerformance { get; set; }
    }
}