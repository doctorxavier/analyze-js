using System.Net;
using System.Web.Mvc;

using IDB.Domain.Attributes;
using IDB.MW.Application.TCM.Messages.ResultsMatrix;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
 
namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class VerifyContentController : BaseController
    {
        private readonly IVerifyContentService _verifyContentService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;

        public VerifyContentController(IVerifyContentService verifyContentService)
        {
            _verifyContentService = verifyContentService;
            _viewModelMapperHelper = new ViewModelMapperHelper(null, null);
        }

        [ExceptionHandling]
        public virtual ActionResult VerifyContent(int resultsMatrixId)
        {
            var response = _verifyContentService
                .GetVerifyContents(new GetVerifyContentRequest { ResultsMatrixId = resultsMatrixId }, null);

            if (!response.IsValid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.ErrorMessage);

            var viewModel = _viewModelMapperHelper.ConvertVerifyContentViewModel(response.VerifyContentModel);

            return PartialView(viewModel);
        }
    }
}
