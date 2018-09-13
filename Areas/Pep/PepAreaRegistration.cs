using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Pep
{
    public class PepAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Pep";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Pep_default",
                "{operationNumber}/Pep/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}