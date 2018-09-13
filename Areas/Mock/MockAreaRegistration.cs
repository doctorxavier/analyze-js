using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Mock
{
    public class MockAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Mock";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "Mock_default",
               "{operationNumber}/Mock/{controller}/{action}",
               new { action = "Index" });
        }
    }
}
