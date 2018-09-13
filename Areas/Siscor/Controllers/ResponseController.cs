using System;
using System.Collections.Generic;
using System.Web.Mvc;
using IDB.Architecture.Services;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.SiscorModule.Enums;
using IDB.MW.Application.SiscorModule.Helpers;
using IDB.MW.Application.SiscorModule.Services;
using IDB.MW.Application.SiscorModule.ViewModels;
using IDB.MW.Domain.Contracts.ModelRepositories.Signature;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.OPUSModule;
using IDB.MW.DomainModel.Contracts.Repositories.SiscorModule;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.Siscor.Models;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Siscor.Controllers
{
    public partial class ResponseController : BaseController
    {
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;
        private readonly ISignatureModelRepository _signatureModelRepository;
        private readonly IProxySiscorService _proxySiscorService;
        private readonly ISiscorCorrespondenceRepository _siscorCorrespondenceRepository;
        private readonly IDocumentSiscorCorrespondenceRepository _documentSiscorCorrespondenceRepository;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IOperationDataRepository _operationDataRepository;
        private readonly IOperationRepository _operationRepository;

        public ResponseController(ICatalogService catalogService,
            ISignatureModelRepository signatureModelRepository,
            IProxySiscorService proxySiscorService,
            ISiscorCorrespondenceRepository siscorCorrespondenceRepository,
            IDocumentSiscorCorrespondenceRepository documentSiscorCorrespondenceRepository,
            ICorrespondenceService correspondenceService,
            IOperationDataRepository operationDataRepository,
            IOperationRepository operationRepository)
        {
            _catalogService = catalogService;
            _signatureModelRepository = signatureModelRepository;
            _proxySiscorService = proxySiscorService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService, _signatureModelRepository, _proxySiscorService, _correspondenceService);
            _siscorCorrespondenceRepository = siscorCorrespondenceRepository;
            _documentSiscorCorrespondenceRepository = documentSiscorCorrespondenceRepository;
            _correspondenceService = correspondenceService;
            _operationDataRepository = operationDataRepository;
            _operationRepository = operationRepository;
        }

        public virtual ActionResult Index(string operationNumber, string siscorNumber)
        {
            var siscorResponseViewModel = new SiscorResponseViewModel();
            var response = new BaseResponse();
            var correspondence = _correspondenceService.GetSiscorCorrespondence(siscorNumber);
            var correspondenceViewModel = _correspondenceService.GetInputsRelated(correspondence, siscorResponseViewModel);

            try
            {
                List<string> lsDocumentsAttached = new List<string>();

                siscorResponseViewModel.GridCorrespondences.OperationNumber = correspondence.Operation.OperationNumber;

                _correspondenceService.LoadDocumentPrincipalAttach(correspondence, siscorResponseViewModel);
                _correspondenceService.LoadInformationResponse(correspondence, siscorResponseViewModel);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.Message;
            }

            return View(siscorResponseViewModel);
        }

        public virtual FileResult DownloadDocument(string documentId, string opDownNumber, string businessAreaCode)
        {
            var opNumber = !string.IsNullOrEmpty(IDBContext.Current.Operation) ? IDBContext.Current.Operation : opDownNumber;
            var userName = SiscorHelper.CheckUserFormat(IDBContext.Current.UserName);
            var response = _correspondenceService.DownloadApi365Document(opNumber, userName, businessAreaCode, documentId);

            return !response.IsValid ? null : File(response.File, System.Net.Mime.MediaTypeNames.Application.Octet, response.FileName);
        }

        public virtual ActionResult CheckIfDocumentIsAvailable(string documentId, string opDownNumberF, string businessAreaCode)
        {
            var opNumber = !string.IsNullOrEmpty(IDBContext.Current.Operation) ? IDBContext.Current.Operation : opDownNumberF;
            var userName = SiscorHelper.CheckUserFormat(IDBContext.Current.UserName);
            var responseDocument = _correspondenceService.DownloadApi365Document(opNumber, userName, businessAreaCode, documentId);

            var response = new ResponseBase()
            {
                IsValid = responseDocument.IsValid,
                ErrorMessage = responseDocument.ErrorMessage
            };

            return Json(response);
        }

        #region Private Methods
        private string LoadSiscorDocumentModule(string businessAreaCode)
        {
            string documentModule = SiscorEnums.MOD_SISCOR_INBOX;
            var libraryByBusinessArea = _correspondenceService.GetLibraryByBusinessArea(businessAreaCode);
            var moduleByLibrary = _correspondenceService.GetModuleByBLibrary(businessAreaCode);
            documentModule = moduleByLibrary;
            return documentModule;
        }
        #endregion
    }
}