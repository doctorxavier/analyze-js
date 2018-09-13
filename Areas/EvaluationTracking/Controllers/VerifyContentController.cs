using System.Web.Mvc;

using IDB.Domain.Attributes;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Contracts.ModelRepositories.EvaluationTracking;

namespace IDB.Presentation.MVC4.Areas.EvaluationTracking.Controllers
{
    public partial class VerifyContentController : BaseController
    {
        private IEvaluationTrackingModelRepository _clientEvaluationTracking = null;
        public IEvaluationTrackingModelRepository ClientEvaluationTracking
        {
            get { return _clientEvaluationTracking; }
            set { _clientEvaluationTracking = value; }
        }

        [OutputCache(Duration = 60, VaryByParam = "*")]
        [ExceptionHandling]
        public virtual ActionResult VerifyContent(int resultsMatrixId, string verifyContent) 
        {
            var verifyResult = ClientEvaluationTracking.GetVerifyContentData(resultsMatrixId, verifyContent);

            return PartialView(verifyResult);
        }

        public virtual ActionResult VerifyContentSave(int resultsMatrixId, string verifyContent, string type)
        {
            var verifyResult = ClientEvaluationTracking.GetVerifyContentDataSave(resultsMatrixId, verifyContent, type);

            return PartialView("~/Areas/EvaluationTracking/Views/VerifyContent/VerifyContent.cshtml", verifyResult);
        }
    }
}
