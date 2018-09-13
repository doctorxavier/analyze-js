using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Notifications
{
    public class NotificationsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Notifications";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Notifications_default",
                "Notifications/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
