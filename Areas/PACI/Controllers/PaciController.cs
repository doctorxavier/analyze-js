using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.PACI.Contracts;
using IDB.MW.Application.PACI.Message;
using IDB.MW.Application.PACI.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Paci;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Areas.PACI.Models;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.PACI.Controllers
{
    public partial class PaciController : BaseController
    {
        private readonly ICatalogService _catalogService;
        private readonly IPaciScreenSecurityService _securityService;
        private readonly IPaciService _paciService;

        public PaciController(IPaciService paciService,
            ICatalogService catalogService,
            IPaciScreenSecurityService securityService)
        {
            _paciService = paciService;
            _catalogService = catalogService;
            _securityService = securityService;
        }

        [Authorize]
        public virtual ActionResult Index(string operationNumber, string errorMessage = null)
        {
            ForceUnlockRegister(operationNumber, Request.Path.Replace(Literals.ROUTE_PACI, string.Empty));

            var response = _paciService.GetAllQuestionnaire(new PaciRequest()
            {
                OperationNumber = operationNumber
            });

            PaciViewModel model = new PaciViewModel()
            {
                IsEdit = false,
                OperationNumber = operationNumber
            };

            if (response.IsValid)
            {
                model.AddNewPaciIsVisible = response.AddNewPaciIsVisible;
                model.OperationId = response.OperationId;
                model.InstitutionQuestionnaires = response.InstitutionQuestionnaires
                    ?? new List<InstitutionQuestionnaireViewModel>();
                model.ErrorMessage = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : string.Empty;
            }
            else
            {
                model.ErrorMessage = response.ErrorMessage;
                model.InstitutionQuestionnaires = new List<InstitutionQuestionnaireViewModel>();
            }

            model.Security = new SecurityViewModel()
            {
                PageName = SecurityAttributes.UIPA001READ,
                Security = GetFieldsSecurity(SecurityAttributes.UIPA001READ, operationNumber, IDBContext.Current.Permissions).SecuredFields.ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult Edit(PaciViewModel model, string operationNumber)
        {
            model.Security = new SecurityViewModel()
            {
                PageName = SecurityAttributes.UIPA001EDIT,
                Security = GetFieldsSecurity(SecurityAttributes.UIPA001EDIT, model.OperationNumber, IDBContext.Current.Permissions).SecuredFields.ToList()
            };

            model.IsEdit = true;
            return PartialView("~/Areas/PACI/Views/Partials/PaciEdit.cshtml", model);
        }

        [Authorize]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult Cancel(PaciViewModel model, string operationNumber)
        {
            return RedirectToAction("Index", new { operationNumber = model.OperationNumber });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult Save(PaciViewModel model, string operationNumber)
        {
            var newPaciQuestionnaire = model.InstitutionQuestionnaires
                .SelectMany(p => p.OperationQuestionnaire)
                .Where(x => x.PaciQuestionnaireId == default(int))
                .ToList();

            if (!newPaciQuestionnaire.HasAny())
            {
                return RedirectToAction("Index", new { operationNumber = operationNumber });
            }

            var response = _paciService.SavePaci(new PaciSaveRequest
            {
                OperationNumber = operationNumber,
                OperationQuestionnaire = newPaciQuestionnaire
            });

            if (!response.IsValid)
            {
                var security = new SecurityViewModel()
                {
                    PageName = SecurityAttributes.UIPA001EDIT,
                    Security = GetFieldsSecurity(SecurityAttributes.UIPA001EDIT, operationNumber, IDBContext.Current.Permissions).SecuredFields.ToList()
                };

                model.IsEdit = true;
                model.Security = security;
                model.ErrorMessage = response.ErrorMessage;
                return View("~/Areas/PACI/Views/Paci/Index.cshtml", model);
            }

            return RedirectToAction("Index", new { operationNumber = operationNumber });
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult NewPaci(PaciViewModel model, int institutionId, string operationNumber)
        {
            model.SelectedInstitution = institutionId;

            var questionnaireStage = _catalogService.GetMasterDataListByTypeCode(typeCodes: Literals.QUESTIONNAIRE_STAGE_TYPE);

            if (!questionnaireStage.IsValid)
            {
                model.ErrorMessage = questionnaireStage.ErrorMessage;
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var questionnaireStageDraft = questionnaireStage
                .MasterDataCollection
                .FirstOrDefault(mdc => mdc.Code == QuestionnaireStageCodeEnum.Draft.GetStringValue());

            if (questionnaireStageDraft == null)
            {
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_NOT_FOUND_DRAFT_ERROR);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var newOperationQuestionnaire = new OperationQuestionnaireViewModel
            {
                PaciQuestionnaireId = default(int),
                QuestionnaireStage = questionnaireStageDraft.GetLocalizedName(Localization.CurrentLanguage),
                QuestionnaireStageCode = questionnaireStageDraft.Code,
                OperationNumber = operationNumber,
                InstitutionId = institutionId,
                QuestionnaireStageId = questionnaireStageDraft.MasterId
            };

            model.InstitutionQuestionnaires.Where(s => s.InstitutionId == institutionId)
                .Select(x => x.OperationQuestionnaire)
                .FirstOrDefault()
                .Add(newOperationQuestionnaire);

            return PartialView("~/Areas/PACI/Views/Controls/NewPaci.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult DeletePaci(PaciViewModel model, int paciQuestionnaireId)
        {
            var response = _paciService.DeleteQuestionnaire(new PaciQuestionnaireDeleteRequest
            {
                PaciQuestionnaireId = paciQuestionnaireId
            });

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write")]
        public virtual ActionResult ClonePaci(PaciViewModel model, int institutionId, int paciQuestionnaireId, int operationId, string operationNumber)
        {
            var response = _paciService.CloneQuestionnaire(new PaciQuestionnaireCloneRequest
            {
                OperationId = operationId,
                PaciQuestionnaireId = paciQuestionnaireId,
                InstitutionId = institutionId
            });

            if (!response.IsValid)
            {
                model.ErrorMessage = response.ErrorMessage;
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            OperationQuestionnaireViewModel clonedOperationQuestionnaire = model.InstitutionQuestionnaires
                .Where(n => n.InstitutionId == institutionId)
                .Select(q => q.OperationQuestionnaire)
                .FirstOrDefault(o => o.Any(p => p.PaciQuestionnaireId == paciQuestionnaireId))
                .FirstOrDefault();

            clonedOperationQuestionnaire.PaciQuestionnaireId = response.PaciQuestionnaireId;
            clonedOperationQuestionnaire.QuestionnaireStage = response.QuestionnaireStage;
            clonedOperationQuestionnaire.QuestionnaireStageId = response.QuestionnaireStageId;
            clonedOperationQuestionnaire.QuestionnaireStageCode = response.QuestionnaireStageCode;
            clonedOperationQuestionnaire.OperationNumber = operationNumber;
            clonedOperationQuestionnaire.InstitutionId = institutionId;
            clonedOperationQuestionnaire.OperationId = operationId;
            clonedOperationQuestionnaire.QuestionnaireIsClonable = default(bool);
            model.SelectedInstitution = institutionId;
            model.InstitutionQuestionnaires.Where(s => s.InstitutionId == institutionId)
                .Select(x => x.OperationQuestionnaire)
                .FirstOrDefault()
                .Add(clonedOperationQuestionnaire);

            return PartialView("~/Areas/PACI/Views/Controls/ClonePaci.cshtml", model);
        }

        private SecurityResponse GetFieldsSecurity(string pageName, string operationNumber, List<string> permissions, int? paciId = null)
        {
            var result = _securityService.GetAllComponentSecurity(new SecurityRequest()
            {
                OperationNumber = operationNumber,
                PageName = pageName,
                Permissions = permissions
            });

            return result;
        }
    }
}