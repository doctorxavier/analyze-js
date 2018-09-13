using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.OPUS
{
    public class OPUSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OPUS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "OPUS_default",
                "{operationNumber}/OPUS/{controller}/{action}",
                new { action = "EnvironmentalSocialData" });

            context.MapRoute(
                "OPUS_creation",
                "OPUS/View/{action}",
                new { controller = "View", action = "CreateCreationForm" });
        }
    }
}