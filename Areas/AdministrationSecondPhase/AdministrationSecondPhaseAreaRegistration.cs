using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase
{
    public class AdministrationSecondPhaseAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AdministrationSecondPhase";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "AdministrationSecondPhase_default",
                "AdministrationSecondPhase/{controller}/{action}",
                new { action = "Delegation" });

            context.MapRoute("AdministrationSecondPhase_Search",
                    "AdministrationSecondPhase/{controller}/{action}");
        }
    }
}