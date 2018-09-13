using System.Web.Mvc;
using IDB.Architecture.Language;
using IDB.MW.Application.CPDModule.Enums;
using IDB.MW.Application.CPDModule.Services.Document;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.SecurityService.Contracts;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
{
    public partial class DocumentCPDController : BaseController
    {
        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IOperationRepository _operationRepository;
        private readonly ICPDDocumentService _cPDDocumentService;
        #endregion

        #region Contructors
        public DocumentCPDController(
            IOperationRepository operationRepository,
            IAuthorizationService authorizationService,
            ICPDDocumentService cPDDocumentService)
            : base(authorizationService)
        {
            _operationRepository = operationRepository;
            _authorizationService = authorizationService;
            _cPDDocumentService = cPDDocumentService;
        }
        #endregion
        // GET: CPD/Document
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult ExportDocCPD(string operationNumber)
        {
            var response =
                _cPDDocumentService.GetCountryProgramDocument(operationNumber);

            return base.File(response.Document, "application/msword", GetDocName(operationNumber));
        }

        private string GetDocName(string operationNumber)
        {
            string documentNameEN = CPDDocumentConstants.DOCUMENT_EN;
            string documentNameES = CPDDocumentConstants.DOCUMENT_ES;
            string documentExtension = CPDDocumentConstants.DOCUMENT_EXTENSION;
            string dashSeparator = CPDDocumentConstants.DASH_SEPARATOR;
            var document = IDBContext.Current.CurrentLanguage == Language.ES
                ? (operationNumber + dashSeparator + documentNameES + documentExtension)
                : (operationNumber + dashSeparator + documentNameEN + documentExtension);

            return document;
        }
    }
}