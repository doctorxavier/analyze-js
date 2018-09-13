using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.PACI.Contracts;
using IDB.MW.Application.PACI.Message;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Paci;
using IDB.Presentation.MVC4.Areas.PACI.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.PACI.Controllers
{
    public partial class ReportController : BaseController
    {
        private IAnalysisService _analysisService;
        private IAssessmentService _assessmentService;
        private IPaciScreenSecurityService _securityService;
        private ICatalogService _catalogService;
        private IPaciService _paciService;

        public ReportController(IAnalysisService analysisService,
            IPaciScreenSecurityService securityService,
            IAssessmentService assessmentService,
            ICatalogService catalogService,
            IPaciService paciService)
        {
            _analysisService = analysisService;
            _securityService = securityService;
            _assessmentService = assessmentService;
            _catalogService = catalogService;
            _paciService = paciService;
        }

        [Authorize]
        public virtual ActionResult Index(string operationNumber, int? institutionId = null, int? idQuestionnaire = null)
        {
            ForceUnlockRegister(operationNumber, Request.Path);
            var analysisResponse = _analysisService.GetAllModulesAnalysis(new AnalysisRequest
            {
                InstitutionId = institutionId.Value,
                PaciId = idQuestionnaire.Value,
                OperationNumber = operationNumber
            });

            ReportViewModel model = new ReportViewModel()
            {
                InstitutionId = institutionId.Value,
                ModuleTabId = "tabReport",
                IsEdit = false,
                OperationNumber = operationNumber,
                TabId = "tabReport"
            };

            if (!analysisResponse.IsValid)
            {
                model.ErrorMessage = analysisResponse.ErrorMessage;
            }
            else
            {
                model.OperationNumber = operationNumber;
                model.PaciStage = analysisResponse.PaciQuestionnaire.QuestionnaireStage;
                model.Modules = analysisResponse.PaciQuestionnaire.Modules;
                model.InstitutionName = analysisResponse.InstitutionName;
                model.InstitutionId = analysisResponse.PaciQuestionnaire.InstitutionId;
                model.OperationIsComplete = analysisResponse.OperationIsComplete;
                model.QuestionnaireId = idQuestionnaire.Value;
                model.IsAllEvaluated = analysisResponse.IsAllEvaluated;
                model.CanEvaluate = analysisResponse.PaciQuestionnaire.CanEvaluate;
                model.PaciIsComplete = analysisResponse.PaciIsComplete;
            }

            model.Security = new SecurityViewModel()
            {
                PageName = SecurityAttributes.UIPA008READ,
                Security = GetFieldsSecurity(SecurityAttributes.UIPA008READ, operationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,HR PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write,Admin PACI Write")]
        public virtual ActionResult Edit(ReportViewModel model)
        {
            if (model == null)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var lockReport = LockReport(model.OperationNumber);

            if (!lockReport.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, lockReport.ErrorMessage);
            }

            model.IsEdit = true;
            model.PageName = new List<string>();

            foreach (var items in model.Modules.Select(x => x.PageNumber))
            {
                model.PageName.Add(SecurityAttributes.UIPA008 + items + SecurityAttributes.EDIT);
            }

            model.Security = new SecurityViewModel()
            {
                PageName = SecurityAttributes.UIPA008EDIT,
                Security = GetFieldsSecurity(SecurityAttributes.UIPA008, model.OperationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
            };

            return PartialView("~/Areas/PACI/Views/Partials/ReportEdit.cshtml", model);
        }

        [Authorize]
        public virtual ActionResult Cancel(ReportViewModel model)
        {
            return RedirectToAction("index", new
            {
                operationNumber = model.OperationNumber,
                institutionId = model.InstitutionId,
                idQuestionnaire = model.QuestionnaireId,
                paciStage = model.PaciStage,
                paciIsComplete = model.PaciIsComplete
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,HR PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write,Admin PACI Write")]
        public virtual ActionResult Save(ReportViewModel model)
        {
            if (model == null)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var result = _analysisService.SaveAnalysis(new AnalysisSaveRequest()
            {
                InstitutionId = model.InstitutionId,
                OperationNumber = model.OperationNumber,
                PaciId = model.QuestionnaireId,
                Modules = model.Modules.ToList()
            });

            if (!result.IsValid)
            {
                model.PageName = new List<string>();

                foreach (var items in model.Modules.Select(x => x.PageNumber))
                {
                    model.PageName.Add(SecurityAttributes.UIPA008 + items + SecurityAttributes.EDIT);
                }

                model.IsEdit = true;
                model.Security = new SecurityViewModel()
                {
                    PageName = SecurityAttributes.UIPA008EDIT,
                    Security = GetFieldsSecurity(SecurityAttributes.UIPA008, model.OperationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
                };

                model.ErrorMessage = result.ErrorMessage;
                return View("~/Areas/PACI/Views/Report/Index.cshtml", model);
            }

            model.IsEdit = false;
            return RedirectToAction("index",
                new { operationNumber = model.OperationNumber, institutionId = model.InstitutionId, idQuestionnaire = model.QuestionnaireId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EvaluateModule(ReportViewModel model, int moduleId)
        {
            if (model == null || model.Modules == null)
            {
                model = new ReportViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var resultResponse = _assessmentService.Evaluate(new AssessmentRequest()
            {
                ModuleId = moduleId,
                QuestionnaireId = model.QuestionnaireId
            });

            if (!resultResponse.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, resultResponse.ErrorMessage);
            }

            var module = model.Modules.FirstOrDefault(m => m.ModuleId == moduleId);
            if (module != null)
            {
                module.Assessments = resultResponse.Assessments;
                model.IsAllEvaluated = resultResponse.IsAllEvaluated;
            }

            model.SelectedAssessment = new MoveAssessmentViewModel()
            {
                AssessmentId = default(int),
                ModuleId = moduleId
            };

            return PartialView("~/Areas/PACI/Views/Controls/ReportAssessmentTable.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangeAssessmentType(ReportViewModel model, string newType, int moduleId, int assessmentId)
        {
            if (model == null || model.Modules == null)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var masterData = _catalogService.GetConvergenceMasterDataIdByCode(newType, Literals.TYPE_EVALUATION_TYPE);
            if (!masterData.IsValid)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var assess = model.Modules.SelectMany(m => m.Assessments.Where(a => a.AssessmentId == assessmentId)).FirstOrDefault();
            if (assess != null)
            {
                assess.EvaluationType = masterData.Id;
                assess.EvaluationCode = newType;
            }

            model.SelectedAssessment = new MoveAssessmentViewModel()
            {
                AssessmentId = assessmentId,
                ModuleId = moduleId
            };

            return PartialView("~/Areas/PACI/Views/Controls/AssessmentRow.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,Admin PACI Write")]
        public virtual ActionResult CompletePaci(ReportViewModel model)
        {
            if (model == null)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var resultResponse = _paciService.CompletePaci(new PaciChangeStatusRequest()
            {
                PaciId = model.QuestionnaireId
            });

            return Json(resultResponse, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,Admin PACI Write")]
        public virtual ActionResult ReviewPaci(ReportViewModel model)
        {
            if (model == null)
            {
                model = new ReportViewModel()
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var resultResponse = _paciService.ReviewPaci(new PaciChangeStatusRequest()
            {
                PaciId = model.QuestionnaireId
            });

            return Json(resultResponse, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public virtual FileResult DownloadPaciReportFile(int paciId, string formatType, string operationNumber)
        {
            var response = _analysisService.GetPaciReport(new ReportRequest
            {
                PaciQuestionnaireId = paciId,
                FormatType = formatType,
                OperationNumber = operationNumber
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = string.Format(Literals.PACI_REPORT_FILE_NAME_FORMAT, response.ExecutingAgencyCode);

            string application;
            switch (formatType)
            {
                case Literals.EXCEL:
                    application = MimeTypeMap.GetMimeType(Literals.EXCEL);
                    fileName = fileName + Literals.DOTEXCEL;
                    break;
                case Literals.WORD:
                    application = MimeTypeMap.GetMimeType(Literals.WORD);
                    fileName = fileName + Literals.DOTDOC;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(Literals.PDF);
                    fileName = fileName + Literals.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        private SecurityResponse GetFieldsSecurity(string pageName, string operationNumber, List<string> permissions, int paciId)
        {
            var result = _securityService.GetAllReportSecurity(new SecurityReportRequest()
            {
                OperationNumber = operationNumber,
                PageName = pageName,
                Permissions = permissions,
                PaciId = paciId
            });

            return result;
        }

        private ResponseBase LockReport(string operationNumber)
        {
            var response = new ResponseBase { IsValid = true };
            var lockRegister = LockRegister(operationNumber, Request.Url.AbsoluteUri);
            response = (ResponseBase)lockRegister.Data;

            return response;
        }
    }
}