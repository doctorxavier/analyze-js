using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.VER
{
    public class VERAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VER";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "VER_default",
                "{operationNumber}/VER/{controller}/{action}",
               new { action = "Index" });
            context.MapRoute(
                "VER_MyDocuments",
                "VER/{controller}/{action}",
                new { controller = "View", action = "Index" });
        }
    }
}