using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.GenericCasesService.Interfaces;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Application.AdministrationModule.ViewModels.GenericCase;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class GenericCasesViewController : BaseController
    {
        private readonly IGenericCasesService _genericCasesService;

        public GenericCasesViewController(IGenericCasesService genericCasesService)
        {
            _genericCasesService = genericCasesService;
        }

        public virtual ActionResult ApprovalIndex()
        {
            return View(_genericCasesService.GetApprovalViewModel().Model);
        }

        public virtual ActionResult ClausesIndex()
        {
            return View(_genericCasesService.GetClausesViewModel().Model);
        }

        [HttpGet]
        public virtual JsonResult GetContractsAndClauses(int operationId)
        {
            return Json(
                _genericCasesService.GetClausesFieldsByOperationId(operationId),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual JsonResult GetFundsOperation(int operationId)
        {
            return Json(
                _genericCasesService.GetFundsOperation(operationId),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual JsonResult GetOperationNumbers(string filter)
        {
            return Json(
                _genericCasesService.GetOperationsNumber(filter),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual JsonResult ExecuteApproval(ApprovalParametersViewModel parameters)
        {
            return Json(
                _genericCasesService.ExecuteApproval(parameters),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual JsonResult ExecuteClause(ClauseParametersViewModel parameters)
        {
            return Json(
                _genericCasesService.ExecuteClause(parameters),
                JsonRequestBehavior.AllowGet);
        }
    }
}