using System.Web.Mvc;
using System.Linq;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.SiscorModule.Enums;
using IDB.MW.Application.SiscorModule.Helpers;
using IDB.MW.Application.SiscorModule.Messages;
using IDB.MW.Application.SiscorModule.Services;
using IDB.MW.Application.SiscorModule.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.Presentation.MVC4.Areas.Siscor.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Siscor.Controllers
{
    public partial class CorrespondenceViewController : BaseController
    {
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;
        private readonly IProxySiscorService _proxySiscorService;
        private readonly ICorrespondenceService _correspondenceService;
        private readonly IOperationRepository _operationRepository;

        public CorrespondenceViewController(
            ICatalogService catalogService,
            ICorrespondenceService correspondenceService,
            IProxySiscorService proxySiscorService,
            IOperationRepository operationRepository)
        {
            _catalogService = catalogService;
            _correspondenceService = correspondenceService;
            _proxySiscorService = proxySiscorService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService, null, _proxySiscorService, _correspondenceService);
            _operationRepository = operationRepository;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            SiscorCorrespondenceViewModel siscorCorrespondenceViewModel = new SiscorCorrespondenceViewModel();
            CorrespondenceSearchViewModel correspondenceSearchViewModel = new CorrespondenceSearchViewModel();
            CorrespondenceSearchResponse searchResponse = new CorrespondenceSearchResponse();
            siscorCorrespondenceViewModel.CorrespondenceGridViewModel = new CorrespondenceGridViewModel();

            if (!string.IsNullOrWhiteSpace(operationNumber))
                correspondenceSearchViewModel.OperationNumber = operationNumber;

            searchResponse = _correspondenceService.SearchFunction(correspondenceSearchViewModel, operationNumber);

            siscorCorrespondenceViewModel.CorrespondenceGridViewModel.Correspondences = searchResponse.LsCorrespondenceViewModel;
            siscorCorrespondenceViewModel.CorrespondenceGridViewModel.OperationNumber = operationNumber;
            siscorCorrespondenceViewModel.DocumentType = _viewModelMapperHelper.GetListMasterData(SiscorEnums.SISCOR_TYPE_DOCUMENT);
            var LsBusinessAreas = _viewModelMapperHelper.GetListMasterData(SiscorEnums.BUSINESS_AREAS);
            siscorCorrespondenceViewModel.LsBusinessAreas = LsBusinessAreas.Where(x =>
                x.Value != "BA_SIGNATURES" &&
                x.Value != "BA_DELEGATIONS" &&
                x.Value != "BA_TFAS" &&
                x.Value != "BA_PROJECT_SPECIFIC").ToList();
            siscorCorrespondenceViewModel.CurrentUser = SiscorHelper.CheckUserFormat(IDBContext.Current.UserName);

            return View(siscorCorrespondenceViewModel);
        }

        public virtual ActionResult Search(CorrespondenceSearchViewModel correspondenceSearchViewModel, string operationNumber)
        {
            CorrespondenceGridViewModel siscorCorrespondenceViewModel = new CorrespondenceGridViewModel();
            CorrespondenceSearchResponse searchResponse = new CorrespondenceSearchResponse();
            correspondenceSearchViewModel.CurrentUser = IDBContext.Current.UserName;
            searchResponse = _correspondenceService.SearchFunction(correspondenceSearchViewModel, operationNumber);
            siscorCorrespondenceViewModel.OperationNumber = IDBContext.Current.Operation;
            siscorCorrespondenceViewModel.Correspondences = searchResponse.LsCorrespondenceViewModel;

            return Json(new JsonResult
            {
                Data = new
                {
                    partial = this.RenderRazorViewToString("Partials/DataTables/Grid", siscorCorrespondenceViewModel),
                    IsValid = searchResponse.IsValid,
                    ErrorMessage = searchResponse.ErrorMessage
                }
            });
        }
    }
}