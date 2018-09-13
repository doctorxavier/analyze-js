namespace IDB.Presentation.MVC4.Areas.Global
{
    using System.Web.Mvc;

    public class GTDAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Global";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Global_operation",
                "{operation}/Global/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "Global_default",
                "Global/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "global-security",
                "Global/security/{action}",
                new { controller = "security" });

            context.MapRoute(
                "WorkflowNew",
                "{operation}/Global/Workflows/{action}/{workflowTypeCode}/{entityTypeId}/{entityId}",
                new
                {
                    action = "Index",
                    workflowTypeCode = UrlParameter.Optional,
                    entityType = UrlParameter.Optional,
                    entityId = UrlParameter.Optional
                });
        }
    }
}
