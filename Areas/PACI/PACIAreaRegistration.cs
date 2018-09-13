using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.PACI
{
    public class PACIAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PACI";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PACI_default",
                "PACI/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}