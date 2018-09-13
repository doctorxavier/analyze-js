using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.DEM
{
    public class DEM_AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "DEM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "DEM_default",
                "{operationNumber}/DEM/{controller}/{action}",
                new { action = "Dem" });
        }
    }
}