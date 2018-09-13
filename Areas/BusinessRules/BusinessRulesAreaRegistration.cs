using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BusinessRules
{
    public class BusinessRulesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BusinessRules";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BusinessRules_default",
                "BusinessRules/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional, controller = "BusinessRules" });
        }
    }
}