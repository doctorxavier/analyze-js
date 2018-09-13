using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class TaskAndRequestController : MVC4.Controllers.ConfluenceController
    {
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult SustainabilityPartialViewResult()
        {
            return PartialView();
        }
    }
}