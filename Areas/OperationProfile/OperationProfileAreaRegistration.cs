using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.OperationProfile
{
    public class OperationProfileAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "OperationProfile";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "OperationProfile_operation",
                "{operation}/OperationProfile/{controller}/{action}",
                defaults: new { action = "Index" });

            context.MapRoute(
                "OperationProfile_default",
                "OperationProfile/{controller}/{action}",
                defaults: new { action = "Index" });
        }
    }
}
