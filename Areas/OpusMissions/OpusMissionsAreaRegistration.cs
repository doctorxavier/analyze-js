using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.OpusMissions
{
    public class OpusMissionsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OpusMissions";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "OpusMissions_default",
                "{operationNumber}/Missions/{controller}/{action}",
                new { action = "Missions" });

            context.MapRoute(
                "OpusMissions_Reports",
                "OpusMissions/{controller}/{action}",
                new
                {
                    controller = "OpusMissionsReport",
                    action = "OpusMissionsMonitoringReport"
                });
        }
    }
}