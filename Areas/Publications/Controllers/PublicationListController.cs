using System.Web.Mvc;

using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Publications.Controllers
{
    public partial class PublicationListController : BaseController
    {
        [AllowAnonymous]
        public virtual ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public virtual ActionResult Workspace()
        {
            return View();
        }
    }
}