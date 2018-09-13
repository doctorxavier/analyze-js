using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Publications
{
    public class PublicationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "publications";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "publication_operation_route",
                "{operation}/publications/{controller}/{action}",
                defaults: new { action = "Index" });

            context.MapRoute(
                "publication_global_route",
                "publications/{controller}/{action}",
                defaults: new { action = "Index" });
        }
    }
}