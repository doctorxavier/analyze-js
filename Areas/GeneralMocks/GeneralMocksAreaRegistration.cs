using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Mock
{
    public class GeneralMocksAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "GeneralMocks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "GeneralMocks_default",
               "{operationNumber}/GeneralMocks/{controller}/{action}",
               new { action = "Index" });
        }
    }
}