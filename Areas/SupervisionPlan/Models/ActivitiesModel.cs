using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class ActivitiesModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Planeed { get; set; }
        public string Actual { get; set; }
        public List<DocumentModel> Document { get; set; }
    }
}