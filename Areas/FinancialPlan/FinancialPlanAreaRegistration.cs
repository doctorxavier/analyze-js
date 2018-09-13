using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.FinancialPlan
{
    public class FinancialPlanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FinancialPlan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FinancialPlan_default",
                "{operationNumber}/FinancialPlan/{controller}/{action}",
                new { action = "Index" });
        }
    }
}