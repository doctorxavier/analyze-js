using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.MW.Application.PMR.Services.Interfaces;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class EOPRecalculationController : Controller
    {
        private readonly IEOPUpdaterDataService _eopUpdaterDataService;

        public EOPRecalculationController(IEOPUpdaterDataService eopUpdaterDataService)
        {
            _eopUpdaterDataService = eopUpdaterDataService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual JsonResult EOPRecalculation()
        {
            Stopwatch stp = new Stopwatch();

            stp.Start();

            var data = _eopUpdaterDataService.UpdateEOPCalculation();

            stp.Stop();

            Logger.GetLogger().WriteDebug("Timer PMR Eop", "Total updated on:" + stp.Elapsed);

            return new JsonResult
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}