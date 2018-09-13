using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using IDB.MW.Application.PACI.Contracts;
using IDB.MW.Application.PACI.Message;
using IDB.MW.Application.PACI.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Paci;
using IDB.Presentation.MVC4.Areas.PACI.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.PACI.Controllers
{
    public partial class QuestionnaireController : BaseController
    {
        private IPaciService _paciQuestionnaire;
        private IQuestionnaireModuleService _questionnaireModule;
        private IPaciScreenSecurityService _securityService;

        public QuestionnaireController(IPaciService paciQuestionnaire,
                                       IQuestionnaireModuleService questionnaireModule,
                                       IPaciScreenSecurityService securityService)
        {
            _paciQuestionnaire = paciQuestionnaire;
            _questionnaireModule = questionnaireModule;
            _securityService = securityService;
        }

        [Authorize]
        public virtual ActionResult Index(string operationNumber, int? institutionId = null, int? idQuestionnaire = null, int? tabModuleId = null, string pageNumber = null)
        {
            UnlockQuestionnaire(operationNumber);

            var response = _questionnaireModule.GetQuestionnaireByModule(new QuestionnaireModuleRequest()
            {
                TabModuleId = tabModuleId,
                InstitutionId = institutionId.Value,
                PaciId = idQuestionnaire.Value,
                OperationNumber = operationNumber
            });

            QuestionnaireViewModel model = new QuestionnaireViewModel()
            {
                InstitutionId = institutionId.Value,
                OperationNumber = operationNumber,
                IsEdit = false,
                Modules = response.IsValid ? response.Modules : new List<ModuleQuestionnaireViewModel>()
            };

            if (!response.IsValid)
            {
                model.ErrorMessage = response.ErrorMessage;
            }
            else
            {
                var selectedModule = response.Modules.FirstOrDefault();
                int selectedModuleId;

                if (selectedModule != null)
                {
                    selectedModuleId = selectedModule.ModuleId;
                    pageNumber = selectedModule.PageNumber;
                }
                else
                {
                    selectedModuleId = 0;
                }

                model.ModulesTabs = new ModuleDataTabsViewModel();
                model.ModulesTabs = response.ModuleTabs;
                model.ModulesTabs.IsEdit = false;
                model.ModulesTabs.QuestionnaireId = idQuestionnaire.Value;
                model.ModulesTabs.InstitutionId = institutionId.Value;
                model.ModulesTabs.OperationNumber = operationNumber;

                model.QuestionnaireId = idQuestionnaire.Value;
                model.InstitutionName = response.ModuleTabs.InstitutionName;
                model.PaciStage = response.ModuleTabs.PaciStage;
                model.ModuleTabId = selectedModuleId.ToString();
                model.DisplayModule = model.Modules.FirstOrDefault(m => m.ModuleId == selectedModuleId);
            }

            string PageNameRead = SecurityAttributes.UIPA + pageNumber + SecurityAttributes.READ;

            model.Security = new SecurityViewModel
            {
                PageName = PageNameRead,
                Security = GetFieldsSecurity(PageNameRead, model.OperationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
            };

            return PartialView(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,EA PACI Write,Admin PACI Write,HR PACI Write,Legal PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write")]
        public virtual ActionResult Edit(QuestionnaireViewModel model)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var lockQuestionnaire = LockQuestionnaire(model.OperationNumber);

            if (!lockQuestionnaire.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, lockQuestionnaire.ErrorMessage);
            }

            string PageNameEdit = SecurityAttributes.UIPA + model.DisplayModule.PageNumber + SecurityAttributes.EDIT;

            model.IsEdit = true;
            model.ModulesTabs.IsEdit = true;
            model.Security = new SecurityViewModel
            {
                PageName = PageNameEdit,
                Security = GetFieldsSecurity(PageNameEdit, model.OperationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
            };

            return PartialView("~/Areas/PACI/Views/Partials/QuestionnaireModuleEdit.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,EA PACI Write,Admin PACI Write,HR PACI Write,Legal PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write")]
        public virtual ActionResult Save(QuestionnaireViewModel model)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel
                {
                    ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var response = _questionnaireModule.SaveQuestionnaire(new QuestionnaireModuleSaveRequest()
            {
                ModuleTabId = model.QuestionnaireId.ToString(),
                OperationNumber = model.OperationNumber,
                Module = model.DisplayModule
            });

            if (!response.IsValid)
            {
                string PageNameEdit = SecurityAttributes.UIPA + model.DisplayModule.PageNumber + SecurityAttributes.EDIT;
                model.IsEdit = true;
                model.Security = new SecurityViewModel
                {
                    PageName = PageNameEdit,
                    Security = GetFieldsSecurity(PageNameEdit, model.OperationNumber, IDBContext.Current.Permissions, model.QuestionnaireId)
                .SecuredFields
                .ToList()
                };

                model.ErrorMessage = response.ErrorMessage;
                return View("~/Areas/PACI/Views/Questionnaire/Index.cshtml", model);
            }

            model.IsEdit = false;
            return RedirectToAction("index", new { operationNumber = model.OperationNumber, institutionId = model.InstitutionId, idQuestionnaire = model.QuestionnaireId, tabModuleId = model.ModuleTabId });
        }

        [Authorize]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,EA PACI Write,Admin PACI Write,HR PACI Write,Legal PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write")]
        public virtual ActionResult Cancel(QuestionnaireViewModel model)
        {
            return RedirectToAction("index", new { operationNumber = model.OperationNumber, institutionId = model.InstitutionId, idQuestionnaire = model.QuestionnaireId, tabModuleId = model.ModuleTabId });
        }

        [Authorize]
        [HttpPost]
        public virtual ActionResult Inactive()
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,EA PACI Write,Admin PACI Write,HR PACI Write,Legal PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write")]
        public virtual ActionResult NewNote(QuestionnaireViewModel model, string operationNumber, int paciConditionId, int questionId)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var userName = IDBContext.Current.UserName;
            var newComment = new PaciCommentViewModel
            {
                QuestionId = questionId,
                UserComments = new List<CommentViewModel>
                {
                    new CommentViewModel
                    {
                        Created = DateTime.Now,
                        CreatedBy = userName
                    }
                }
            };

            model
                .DisplayModule
                .Conditions
                .FirstOrDefault(a => a.PaciConditionId == paciConditionId)
                .Questions
                .FirstOrDefault(b => b.QuestionId == questionId)
                .Comments.Add(newComment);

            return PartialView("~/Areas/PACI/Views/Controls/NewNoteQuestionnaire.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        public virtual ActionResult DeleteNote(QuestionnaireViewModel model, int questionId, int commentId)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var response = _questionnaireModule.DeleteNoteQuestionnaire(new QuestionnaireModuleDeleteNoteRequest
            {
                QuestionId = questionId,
                CommentId = commentId,
                Module = model.DisplayModule
            });

            if (!response.IsValid)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, model.ErrorMessage);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public virtual ActionResult DeleteDocument(QuestionnaireViewModel model, int questionId, int documentId)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var response = _questionnaireModule.DeleteDocumentQuestionnaire(new QuestionnaireModuleDeleteDocumentRequest
            {
                QuestionId = questionId,
                DocumentId = documentId,
                Module = model.DisplayModule
            });

            if (!response.IsValid)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, model.ErrorMessage);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "TL PACI Write,OA PACI Write,Legal PACI Write,EA PACI Write,Admin PACI Write,HR PACI Write,Legal PACI Write,Procurement Management PACI Write,Financial Management PACI Write,ESG PACI Write")]
        public virtual ActionResult AddDocument(QuestionnaireViewModel model, int paciConditionId, int questionId, string documentNumber, string documentName)
        {
            if (model == null)
            {
                model = new QuestionnaireViewModel();
                model.ErrorMessage = Localization.GetText(Literals.QUESTIONNAIRE_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var userName = IDBContext.Current.UserName;
            var newDocument = new PaciDocumentViewModel
            {
                QuestionId = questionId,
                Created = DateTime.Now,
                CreatedBy = userName,
                DocumentDescription = string.Empty,
                DocumentName = documentName,
                DocumentReference = documentNumber,
                IsNewDocument = true
            };

            model
                .DisplayModule
                .Conditions
                .FirstOrDefault(a => a.PaciConditionId == paciConditionId)
                .Questions
                .FirstOrDefault(b => b.QuestionId == questionId)
                .Documents.Add(newDocument);

            return PartialView("~/Areas/PACI/Views/Controls/AddDocument.cshtml", model);
        }

        [Authorize]
        public virtual FileResult DownloadQuestionnaireExportFile(int paciId, string formatType, string operationNumber)
        {
            var response = _questionnaireModule.GetQuestionnaireReport(new QuestionnaireReportRequest
            {
                PaciQuestionnaireId = paciId,
                FormatType = formatType,
                OperationNumber = operationNumber
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = string.Format(Literals.QUESTIONNAIRE_EXPORT_FILE_NAME_FORMAT, response.ExecutingAgencyCode);

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
            var result = _securityService.GetAllComponentSecurity(new SecurityRequest()
            {
                OperationNumber = operationNumber,
                PageName = pageName,
                Permissions = permissions,
                PaciId = paciId
            });

            return result;
        }

        private ResponseBase LockQuestionnaire(string operationNumber)
        {
            var response = new ResponseBase { IsValid = true };
            var lockRegister = LockRegister(operationNumber, Request.Url.AbsoluteUri);
            response = (ResponseBase)lockRegister.Data;

            return response;
        }

        private ResponseBase UnlockQuestionnaire(string operationNumber)
        {
            var response = new ResponseBase { IsValid = true };
            var unlockRegister = ForceUnlockRegister(operationNumber, Request.Path);
            response = unlockRegister;

            return response;
        }
    }
}