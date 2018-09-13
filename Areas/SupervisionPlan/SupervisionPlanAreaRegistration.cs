using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan
{
    public class SupervisionPlanAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SupervisionPlan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {            
            context.MapRoute("SupervisionPlan_default", "{operationNumber}/SupervisionPlan/{controller}/{action}", new { controller = "SupervisionPlan", action = "Summary" });

            context.MapRoute("SupervisionPlan_administration", "SupervisionPlan/AdministrationSp/{action}", new { controller = "AdministrationSp", action = "Index" });
            context.MapRoute(
                "SupervisionPlan_route",
                "{mainOperationNumber}/SupervisionPlan/SupervisionPlan/{action}",
                new { action = "Index", Controller = "SupervisionPlan" });
        }
    }
}