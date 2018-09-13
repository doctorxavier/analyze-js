using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.MrBlue
{
    public class MrBlueAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MrBlue";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
                context.MapRoute(
                "MrBlue_default",
                "{operationNumber}/MrBlue/{controller}/{action}",
                new { action = "GeneralInformation" });
                context.MapRoute(
                "MrBlue_report_Custom",
                "MrBlue/Report/{action}",
                new { controller = "Report", action = "ReportsCustomReport" });
                context.MapRoute(
                "MrBlue_report_Safeguard",
                "MrBlue/Report/{action}",
                new { controller = "Report", action = "ReportsSafeguardReport" });
                context.MapRoute(
                "MrBlue_report_ESGReport",
                "MrBlue/Report/{action}",
                new { controller = "Report", action = "ReportsESGReport" });
                context.MapRoute(
                "MrBlue_report_ESGDownloads",
                "MrBlue/Report/{action}",
                new { controller = "Report", action = "ReportsESGDownloads" });
                context.MapRoute(
                "MrBlue_SupervisionReport",
                "MrBlue/SupervisionReport/{action}",
                new { controller = "SupervisionReport", action = "FindUserAD" });
                context.MapRoute(
                "MrBlue_report_ChangeTracking",
                "MrBlue/Report/{action}",
                new { controller = "Report", action = "ReportsChangeTracking" });
                context.MapRoute(
                "MrBlue_disclosure_document",
                "MrBlue/DisclosureDocument/{action}",
                new { controller = "DisclosureDocument", action = "Index" });
        }
    }
}
