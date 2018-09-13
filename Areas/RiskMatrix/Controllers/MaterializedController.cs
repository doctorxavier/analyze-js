using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.RiskMatrix.Contracts;
using IDB.MW.Application.RiskMatrix.Messages;
using IDB.MW.Application.RiskMatrix.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.RiskMatrix;
using IDB.Presentation.MVC4.Areas.RiskMatrix.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Security;
using IDB.Presentation.MVCExtensions;
using IDB.Architecture.Language;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix.Controllers
{
    public partial class MaterializedController : IDB.Presentation.MVC4.Controllers.BaseController
    {
        private IRiskMaterializedService _materializedService;
        private IRiskMatrixScreenSecurityService _riskMatrixSecreenScurity;
        public MaterializedController(IRiskMaterializedService materializedService, IRiskMatrixScreenSecurityService riskMatrixSecreenScurity)
        {
            _materializedService = materializedService;
            _riskMatrixSecreenScurity = riskMatrixSecreenScurity;
        }

        [Authorize]
        public virtual ActionResult Index(string operationNumber)
        {
            ForceUnlockRegister(operationNumber, Request.Path);

            var resultResponse = _materializedService.GetAllMaterialized(new RiskMaterializedRequest()
            {
                OperationNumber = operationNumber
            });

            RiskMaterializedViewModel model = new RiskMaterializedViewModel()
            {
                OperationNumber = operationNumber,
                IsEdit = false
            };

            if (!resultResponse.IsValid)
            {
                model.ErrorMessage = resultResponse.ErrorMessage;
            }
            else
            {
                model = GetDataCatalog(model);
                model.HeaderInfo.PmrCycle = resultResponse.PmrCycle;
                model.HeaderInfo.EsgClassfication = resultResponse.EsgClassification;
                model.HeaderInfo.PmrValidationStage = resultResponse.PmrValidationStage;
                model.HeaderInfo.ExecutingAgencies = resultResponse.ExecutingAgencies;
                model.HeaderInfo.LastUpdatedRisk = resultResponse.LastUpdatedRisk;
                model.HeaderInfo.PmrValidatedClassification = resultResponse.PmrValidatedClassification;
                model.HeaderInfo.SafeguardPerformance = resultResponse.SafeguardPerformance;
                model.MaterializedRisks = resultResponse.MaterializedRisks;
            }

            string pageName = "UI-RM-002-READ";
            model.ScreenSecurity = new ScreenSecurityViewModel
            {
                PageName = pageName,
                Security = GetFieldsSecurity(pageName, model.OperationNumber, IDBContext.Current.Permissions)
               .SecuredFields
               .ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult Edit(RiskMaterializedViewModel model)
        {
            if (model == null)
            {
                model = new RiskMaterializedViewModel
                {
                    ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var lockRegister = LockRegister(model.OperationNumber, Request.Url.AbsoluteUri);
            var resultLock = (ResponseBase)lockRegister.Data;
            if (!resultLock.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, resultLock.ErrorMessage);
            }

            model.IsEdit = true;
            model = GetDataCatalog(model);
            string pageName = "UI-RM-002-EDIT";
            model.ScreenSecurity = new ScreenSecurityViewModel
            {
                PageName = pageName,
                Security = GetFieldsSecurity(pageName, model.OperationNumber, IDBContext.Current.Permissions)
               .SecuredFields
               .ToList()
            };

            return PartialView("~/Areas/RiskMatrix/Views/Materialized/Index.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult Cancel(RiskMaterializedViewModel model)
        {
            return RedirectToAction("index", new { operationNumber = model.OperationNumber });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult Save(RiskMaterializedViewModel model)
        {
            if (model == null)
            {
                model = new RiskMaterializedViewModel
                {
                    ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var request = new RiskMaterializedSaveRequest
            {
                OperationNumber = model.OperationNumber,
                RiskMaterialized = model.MaterializedRisks
            };

            var result = request.ValidateRequest();
            if (!request.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.FirstOrDefault().ErrorMessage);
            }

            var response = _materializedService.SaveMaterialized(request);

            if (!response.IsValid)
            {
                model.IsEdit = true;
                model.ErrorMessage = response.ErrorMessage;
                string pageName = "UI-RM-002-EDIT";
                model.ScreenSecurity = new ScreenSecurityViewModel
                {
                    PageName = pageName,
                    Security = GetFieldsSecurity(pageName, model.OperationNumber, IDBContext.Current.Permissions)
                   .SecuredFields
                   .ToList()
                };

                return View("~/Areas/RiskMatrix/Views/Materialized/Index.cshtml", model);
            }

            return RedirectToAction("Index", new { operationNumber = model.OperationNumber });
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult NewIssue(RiskMaterializedViewModel model)
        {
            if (model == null)
            {
                model = new RiskMaterializedViewModel
                {
                    ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST)
                };

                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var newIssue = model.MaterializedRisks.LastOrDefault();
            model = GetDataCatalog(model);
            model.MaterializedRisks.Add(new MaterializedViewModel()
            {
                Actions = string.Empty,
                Category = string.Empty,
                Causes = false,
                CurrentCompletationDate = null,
                Description = string.Empty,
                ExpectedCompletationDate = null,
                FindingsRecommendations = false,
                MaterializedCode = newIssue != null ? newIssue.MaterializedCode + 1 : default(int) + 1,
                MaterializedId = default(int),
                OperationImpact = string.Empty,
                Question = string.Empty,
                RiskId = null,
                StageId = null,
                StageName = string.Empty,
                FindingRecommendationId = default(int),
                FindingRecommendationVM = new FindingRecommendationViewModel()
                {
                    CategoriesRelated = model.MaterializedCategories.Select(r => new FindingRecommendationCategoryViewModel()
                    {
                        CategoryId = r.MasterDataId,
                        ParentId = r.MasterDataParentId,
                        CategoryName = r.NameEn,
                        Selected = false
                    }).ToList(),
                    FindingRecommendationId = default(int),
                }
            });

            return PartialView("~/Areas/RiskMatrix/Views/Controls/NewIssue.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DeleteMaterialized(MaterializedViewModel model, int materializedId)
        {
            var response = _materializedService.DeleteMaterialized(new MaterializedDeleteRequest()
            {
                MaterializedId = materializedId
            });

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DownloadRiskMaterializedExportFile(string operationNumber, string formatType)
        {
            var response = _materializedService.GetRiskMaterializedReport(new RiskMaterializedReportRequest
            {
                OperationNumber = operationNumber,
                FormatType = formatType
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = string.Format(Literals.RISKS_MATERIALIZED_EXPORT_FILE_NAME_FORMAT, operationNumber);

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

        private RiskMaterializedViewModel GetDataCatalog(RiskMaterializedViewModel model)
        {
            var response = _materializedService.GetStageData();
            model.Stages = response.IsValid ? response.Stages : new List<ListItemViewModel>();
            model.MaterializedCategories = response.MaterializedCategories;
            model.MaterializedCategoryTitles = response.MaterializedCategoriesTitle;
            return model;
        }

        private RiskMatrixSecurityResponse GetFieldsSecurity(string pageName, string operationNumber, List<string> permissions)
        {
            var result = _riskMatrixSecreenScurity.GetAllComponentSecurity(new RiskMatrixSecurityRequest()
            {
                OperationNumber = operationNumber,
                PageName = pageName,
                Permissions = permissions
            });

            return result;
        }
    }
}