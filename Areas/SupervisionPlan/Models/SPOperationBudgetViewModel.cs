using System.Collections.Generic;
using System.Web.Mvc;
using IDB.MW.Business.SupervisionPlan.DTOs;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class SPOperationBudgetViewModel
    {
        public SPOperationBudgetDTO SpOperationBudgetDto { get; set; }
        public SPTotalValuesDTO SpTotalValuesDto { get; set; }
        public List<SelectListItem> Years { get; set; }
        public string SelectedYear { get; set; }
        public string SupervisionPlanDashboardRoute { get; set; }
    }
}