using System;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Agreements.Services;
using IDB.MW.Application.Agreements.ViewModel;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.Agreements.Controllers
{
    public partial class AgreementsController : BaseController
    {
        private readonly IAgreementAndConditionService _agreementAndConditionService;

        public AgreementsController(
            IAgreementAndConditionService agreementAndConditionService)
        {
            _agreementAndConditionService = agreementAndConditionService;
        }

        public virtual ActionResult Index(AgreementsAndConditionViewModel model)
        {
            var response = _agreementAndConditionService.GetAgreementsAndConditions(model);

            return View(response.Model);
        }

        public virtual JsonResult EditAgreementToTrack(int agreementId, string operationNumber)
        {
            var response = _agreementAndConditionService.SendAgreementToTrack(agreementId);

            return BuildReturnMessage(response);
        }

        public virtual JsonResult EditAgreementToRecord(int agreementId, string operationNumber)
        {
            var response = _agreementAndConditionService.SendAgreementToRecord(agreementId);

            return BuildReturnMessage(response);
        }

        public virtual JsonResult UpdateDueDate(DateTime date, int agreementId, string operationNumber)
        {
            var response = _agreementAndConditionService
                .SaveDueDate(date, agreementId, operationNumber);

            return BuildReturnMessage(response);
        }

        private JsonResult BuildReturnMessage(ResponseBase response)
        {
            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index", new { operationNumber = IDBContext.Current.Operation })
            });
        }
    }
}