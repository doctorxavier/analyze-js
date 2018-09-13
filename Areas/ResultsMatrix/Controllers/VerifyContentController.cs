using System.Web.Mvc;

using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.ModelRepositories.VerifyContent;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Common;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class VerifyContentController : BaseController
    {
        private IVerifyContentModelRepositoryService _clientVerifyContent = null;

        public virtual IVerifyContentModelRepositoryService ClientVerifyContent
        {
            get { return _clientVerifyContent; }
            set { _clientVerifyContent = value; }
        }

        [OutputCache(Duration = 60, VaryByParam = "*")]
        [ExceptionHandling]
        public virtual ActionResult VerifyContent(int resultsMatrixId, int intervalId, int cycleId)
        {
            var verifyResult = ClientVerifyContent.VerifyContent(
                new ContentVerifyModel
                {
                    ResultsMatrixId = resultsMatrixId,
                    IntervalId = intervalId,
                    CycleId = cycleId
                },
                IDBContext.Current.CurrentLanguage);

            return PartialView(verifyResult);
        }
    }
}
