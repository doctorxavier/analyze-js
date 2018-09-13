using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.VMR
{
    public class VMRAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VMR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "VMR_default",
                "{operationNumber}/VMR/{controller}/{action}",
               new { action = "Index" });

            context.MapRoute(
                "VMR_MyMeetings",
                "VMR/{controller}/{action}",
                new { controller = "View", action = "Index" });
        }
    }
}