using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Visits
{
    public class VisitsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Visits";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Visits_operation",
                "{operation}/Visits/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "Visits_default",
                "Visits/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
