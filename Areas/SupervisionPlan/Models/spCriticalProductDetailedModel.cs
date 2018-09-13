using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class spCriticalProductDetailedModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string PlannedDate { get; set; }
        public string Compliance { get; set; }
        public string CompletionDate { get; set; }
        public List<SpOutputsModel> Outputs { get; set; }
        public string Risks { get; set; }
        public string QuarterName { get; set; }
    }
}