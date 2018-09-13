using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Overview
{
    public class OverviewAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Overview"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Overview_operation",
                "{operationNumber}/Overview/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "Overview_default",
                "Overview/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}