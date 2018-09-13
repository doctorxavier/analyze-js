using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.MediaGallery
{
    public class MediaGalleryAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MediaGallery";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                 "MediaGallery_default",
                "{operationNumber}/MediaGallery/{controller}/{action}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}