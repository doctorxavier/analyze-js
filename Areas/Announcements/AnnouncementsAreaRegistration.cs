using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Announcements
{
    public class AnnouncementsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Announcements";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Announcements_default",
                "Announcements/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}