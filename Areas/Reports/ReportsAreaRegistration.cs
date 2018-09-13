using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Reports
{
    public class ReportsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Reports";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Reports_operation",
                "{operation}/Reports/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "Reports_default",
                "Reports/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "Visualization_report",
                "Reports/{controller}/{action}/{id}",
                new { controller = "Visualization", action = "VisualizationReport", id = UrlParameter.Optional });
        }
    }
}