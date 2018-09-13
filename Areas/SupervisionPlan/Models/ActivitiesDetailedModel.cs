using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class ActivitiesDetailedModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string PlaneedDate { get; set; }
        public string Source { get; set; }
        public string Cost { get; set; }
        public string CompletitionDate { get; set; }
        public string Criticals { get; set; }
        public List<DocumentModel> Document { get; set; }
    }
}