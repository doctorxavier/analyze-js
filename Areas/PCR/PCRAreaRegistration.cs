using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.PCR
{
    public class PCRAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PCR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PCR_default",
                "{operationNumber}/PCR/{controller}/{action}",
                new
                {
                    action = "PCRSummary"
                });

            context.MapRoute(
                "PCR_Reports",
                "PCR/{controller}/{action}",
                new
                {
                    controller = "PCRReport",
                    action = "PCRMonitoringReport"
                });
        }
    }
}
