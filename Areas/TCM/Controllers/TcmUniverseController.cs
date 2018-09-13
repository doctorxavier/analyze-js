using System.Web.Mvc;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class TcmUniverseController : MVC4.Controllers.ConfluenceController
    {
        #region Fields

        private readonly ITcmUniverseService _itcmUniverseService;
        #endregion

        #region Constructor

        public TcmUniverseController(ITcmUniverseService tcmUniverseService)
        {
            _itcmUniverseService = tcmUniverseService;
        }

        #endregion

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult RedirectOutcome(string operationNumber)
        {
            var code = _itcmUniverseService.GetCode(operationNumber);

            switch (code)
            {
                case TCMGlobalValues.CYCLE_TCM:
                    return RedirectToAction(
                        "Index",
                        "Outcomes",
                        new
                        {
                            area = "TCM",
                            operationNumber
                        });

                case TCMGlobalValues.CYCLE_SG:
                    return RedirectToAction(
                        "Index",
                        "Outcomes",
                        new
                        {
                            area = "ResultsMatrix",
                            operationNumber
                        });
            }

            return RedirectToAction("Index", "TcmUniverse");
        }

        public virtual ActionResult RedirectOutput(string operationNumber)
        {
            var code = _itcmUniverseService.GetCode(operationNumber);

            switch (code)
            {
                case TCMGlobalValues.CYCLE_TCM:
                    return RedirectToAction(
                        "Index",
                        "Components",
                        new
                        {
                            area = "TCM",
                            operationNumber
                        });
                case TCMGlobalValues.CYCLE_SG:
                    return RedirectToAction(
                        "IndexPhysicalProgress",
                        "Outputs",
                        new
                        {
                            area = "ResultsMatrix",
                            operationNumber
                        });
            }

            return RedirectToAction("Index", "TcmUniverse");  
        }

        public virtual ActionResult RedirectMatrixChange(string operationNumber)
        {
            var code = _itcmUniverseService.GetCode(operationNumber);

            switch (code)
            {
                case TCMGlobalValues.CYCLE_TCM:
                    return RedirectToAction(
                        "Index",
                        "MatrixChanges",
                        new
                        {
                            area = "TCM",
                            operationNumber
                        });
                case TCMGlobalValues.CYCLE_SG:
                    return RedirectToAction(
                        "Index",
                        "ChangeMatrix",
                        new
                        {
                            area = "ResultsMatrix",
                            operationNumber
                        });
            }

            return RedirectToAction("Index", "TcmUniverse");
        }

        public virtual ActionResult RedirectFindingRecommendation(string operationNumber)
        {
            var code = _itcmUniverseService.GetCode(operationNumber);

            switch (code)
            {
                case TCMGlobalValues.CYCLE_TCM:
                    return RedirectToAction(
                        "Index",
                        "FindingRecommendation",
                        new
                        {
                            area = "TCM",
                            operationNumber
                        });
                case TCMGlobalValues.CYCLE_SG:
                    return RedirectToAction(
                        "Index",
                        "FindingRecommendation",
                        new
                        {
                            area = "FindingRecomendations",
                            operationNumber
                        });
            }

            return RedirectToAction("Index", "TcmUniverse");
        }
    }
}