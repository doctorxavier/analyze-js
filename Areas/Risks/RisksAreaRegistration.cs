using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Risks
{
    public class RisksAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Risks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Risks_operation",
                "{operation}/Risks/{controller}/{action}",
                defaults: new { action = "Details" });
            context.MapRoute(
               "Risks_operation_route",
               "{mainOperationNumber}/Risks/{controller}/{action}",
               defaults: new { action = "Details" });

            context.MapRoute(
               "Risks_newRoute_operation",
               "{mainOperationNumber}/Risks/{controller}/{action}/{id}",
               defaults: new { controller = "OperationRisk", action = "Details", id = UrlParameter.Optional });

            context.MapRoute(
              "Risks_newRoute",
              "Risks/{controller}/{action}/{id}",
              defaults: new { controller = "OperationRisk", action = "Details", id = UrlParameter.Optional });

            context.MapRoute(
                "Risks_default",
                "Risks/{controller}/{action}",
                defaults: new { action = "Details" });
        }
    }
}