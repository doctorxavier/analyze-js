using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Services.DisclosureESGDocuments;
using IDB.MW.Application.MrBlueModule.ViewModels.DisclosureESGDocuments;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.SecurityService.Contracts;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class DisclosureDocumentController : BaseController
    {
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisclosureESGDocumentsService _disclosureESGDocumentsService;
        private readonly IDocumentManagementService _documentManagementService;

        public DisclosureDocumentController(
            IAuthorizationService authorizationService,
            IEnumMappingService enumMappingService,
            IDisclosureESGDocumentsService disclosureESGDocumentsService,
            IDocumentManagementService documentManagementService)
            : base(authorizationService, enumMappingService)
        {
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _disclosureESGDocumentsService = disclosureESGDocumentsService;
            _documentManagementService = documentManagementService;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            return View();
        }

        public virtual ActionResult Edit(string operationNumber)
        {
            return View();
        }

        [HttpPost]
        public virtual JsonResult Load(string operationNumber)
        {
            var data = _disclosureESGDocumentsService.Load(IDBContext.Current.Operation);

            return Json(data);
        }

        [HttpPost]
        public virtual JsonResult Save(List<ESGDisclosureDocumentViewModel> models)
        {
            var response = _disclosureESGDocumentsService.Save(models.First(x => x.IsActive));

            _disclosureESGDocumentsService.SaveDisclosureComments(models);

            return Json(new
            {
                response.IsValid,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                DeleteMessage = Localization.GetText("OP.MS.MissionCompleteView.Message.DeleteDocument")
            });
        }

        [HttpPost]
        public virtual JsonResult SaveAndSend(List<ESGDisclosureDocumentViewModel> models)
        {
            var response = _disclosureESGDocumentsService
                .SaveAndSend(models.First(x => x.IsActive));

            _disclosureESGDocumentsService.SaveDisclosureComments(models);

            return Json(new
            {
                response.IsValid,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationEmail = response.Data,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                DeleteMessage = Localization.GetText("OP.MS.MissionCompleteView.Message.DeleteDocument")
            });
        }

        [HttpPost]
        public virtual JsonResult CloseMilestone(List<ESGDisclosureDocumentViewModel> models)
        {
            var response = _disclosureESGDocumentsService
                .CloseMilestone(models.First(x => x.IsActive));

            _disclosureESGDocumentsService.SaveDisclosureComments(models);

            return Json(new
            {
                response.IsValid,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationEmail = response.Data,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                DeleteMessage = Localization.GetText("OP.MS.MissionCompleteView.Message.DeleteDocument")
            });
        }

        [HttpPost]
        public virtual JsonResult CanCloseMilestone(List<ESGDisclosureDocumentViewModel> models)
        {
            var disclosureActive = models.First(x => x.IsActive);

            var response = _disclosureESGDocumentsService
                .CanCloseMilestone(disclosureActive.ESGDisclosureDocumentId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage
            });
        }

        [HttpPost]
        public virtual JsonResult GetDataDocument(string documentNumber, string documentName, string url)
        {
            var response = _disclosureESGDocumentsService
                .GetDataDocument(documentNumber, documentName, url);

            return Json(new
            {
                document = response.Model,
                response.IsValid,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                DeleteMessage = Localization.GetText("OP.MS.MissionCompleteView.Message.DeleteDocument")
            });
        }

        [HttpPost]
        public virtual JsonResult CheckAndUpdatePublicationDate(DocumentESGDisclosureViewModel models)
        {
            var response = _documentManagementService
                .CheckAndUpdatePublicationDate(models.EzShare, IDBContext.Current.CurrentLanguage);

            return Json(response);
        }
    }
}