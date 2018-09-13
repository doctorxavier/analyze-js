using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Styles
{
    public class StylesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Styles";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Styles_default",
                "Styles/{controller}/{action}",
                new { controller = "View", action = "Index" });
        }
    }
}
