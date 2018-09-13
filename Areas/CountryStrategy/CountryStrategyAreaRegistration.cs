using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy
{
    public class CountryStrategyAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CountryStrategy";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CountryStrategy_default",
                "{operationNumber}/CountryStrategy/{controller}/{action}",
                new { action = "Index" });

            context.MapRoute("country_operation_routing",
                           "CountryStrategy/{controller}/{action}");
        }
    }
}