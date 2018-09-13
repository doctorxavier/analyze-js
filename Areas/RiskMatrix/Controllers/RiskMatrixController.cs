using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

using IDB.MW.Application.RiskMatrix.Contracts;
using IDB.MW.Application.RiskMatrix.Messages;
using IDB.MW.Application.RiskMatrix.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.RiskMatrix;
using IDB.Presentation.MVC4.Areas.RisksMatrix.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Security;
using IDB.Presentation.MVCExtensions;
using IDB.Architecture.Language;
using IDB.MW.Infrastructure.ApplicationBase.Messages;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix.Controllers
{
    public partial class RiskMatrixController : IDB.Presentation.MVC4.Controllers.BaseController
    {
        private const int CONSTONE = 1;
        private IRiskMatrixService _riskMastrixService;
        private IRiskMatrixScreenSecurityService _riskMatrixSecreenScurity;

        public RiskMatrixController(IRiskMatrixService riskMastrixService, IRiskMatrixScreenSecurityService riskMatrixSecreenScurity)
        {
            _riskMastrixService = riskMastrixService;
            _riskMatrixSecreenScurity = riskMatrixSecreenScurity;
        }

        [Authorize]
        public virtual ActionResult Index(string operationNumber, string message = "")
        {
            ForceUnlockRegister(operationNumber, Request.Path);
            var resultResponse = _riskMastrixService.GetAllOperationRisk(new MW.Application.RiskMatrix.Messages.RiskMatrixRequest()
            {
                OperationNumber = operationNumber
            });

            RiskMatrixViewModel model = new RiskMatrixViewModel();

            if (!resultResponse.IsValid)
            {
                model.ErrorMessage = resultResponse.ErrorMessage;
            }
            else
            {
                model.IsEditableRiskMatrix = resultResponse.IsEditableRiskMatrix;
                model.OperationRiskId = resultResponse.OperationRiskId;
                model.IdInactiveStatus = resultResponse.IdInactiveStatus;
                model.IsEdit = false;
                model.OperationNumber = operationNumber;
                model.Documents = resultResponse.Documents;
                model.Risks = resultResponse.Risks;
                model.UserComments = resultResponse.UserComments;
                model.HeaderInfo.PmrCycle = resultResponse.PmrCycle;
                model.HeaderInfo.EsgClassfication = resultResponse.EsgClassification;
                model.HeaderInfo.PmrValidationStage = resultResponse.PmrValidationStage;
                model.HeaderInfo.ExecutingAgencies = resultResponse.ExecutingAgencies;
                model.HeaderInfo.LastUpdatedRisk = resultResponse.LastUpdatedRisk;
                model.HeaderInfo.PmrValidatedClassification = resultResponse.PmrValidatedClassification;
                model.HeaderInfo.SafeguardPerformance = resultResponse.SafeguardPerformance;
                model.StrAccept = resultResponse.StrAccept;
                model.IdAcceptStatus = resultResponse.IdAcceptStatus;
                model.IdMaterialized = resultResponse.IdMaterialized;
                model.IdCompletedStatus = resultResponse.IdCompletedStatus;
                model.CodeStatusHigh = resultResponse.CodeStatusHigh;
                model.CodeStatusMediumHigh = resultResponse.CodeStatusMediumHigh;
                model.CodeStatusMediumLow = resultResponse.CodeStatusMediumLow;
                model.CodeStatusLow = resultResponse.CodeStatusLow;
                model.CodeStatusMedium = resultResponse.CodeStatusMedium;
                model.Message = message;
            }

            string pageName = "UI-RM-001-READ";
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
        [HasPermission(Permissions = "Risks Write,RMG Write")]
        public virtual ActionResult Edit(RiskMatrixViewModel model)
        {
            if (model == null)
            {
                model = new RiskMatrixViewModel();
                model.IsEdit = false;
                model.ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var lockRegister = LockRegister(model.OperationNumber, Request.Url.AbsoluteUri);
            var resultLock = (ResponseBase)lockRegister.Data;
            if (!resultLock.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, resultLock.ErrorMessage);
            }

            model = GetSelectItems(model);
            model.IsEdit = true;
            string pageName = "UI-RM-001-EDIT";
            model.ScreenSecurity = new ScreenSecurityViewModel
            {
                PageName = pageName,
                Security = GetFieldsSecurity(pageName, model.OperationNumber, IDBContext.Current.Permissions)
               .SecuredFields
               .ToList()
            };

            return PartialView("~/Areas/RiskMatrix/Views/RiskMatrix/Index.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write,RMG Write")]
        public virtual ActionResult Cancel(RiskMatrixViewModel model)
        {
            return RedirectToAction("index", new { operationNumber = model.OperationNumber });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions = "Risks Write,RMG Write")]
        public virtual ActionResult Save(RiskMatrixViewModel model, string operationNumber)
        {
            if (model == null)
            {
                model = new RiskMatrixViewModel();
                model.IsEdit = false;
                model.ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var requestSave = new RiskMatrixSaveRequest
            {
                OperationNumber = operationNumber,
                Documents = model.Documents,
                UserComments = model.UserComments,
                Risks = model.Risks,
                OperationRiskId = model.OperationRiskId
            };

            var response = _riskMastrixService.SaveRiskMatrix(requestSave);

            if (!response.IsValid)
            {
                model.IsEdit = true;
                model.ErrorMessage = response.ErrorMessage;
                HttpStatusCodeResult httpStatusCode;
                string pageName = "UI-RM-001-EDIT";
                model.ScreenSecurity = new ScreenSecurityViewModel
                {
                    PageName = pageName,
                    Security = GetFieldsSecurity(pageName, model.OperationNumber, IDBContext.Current.Permissions)
                   .SecuredFields
                   .ToList()
                };

                if (model.ErrorMessage.Contains(Localization.GetText(Literals.RISK_UNMATERIALIZED_FINDINGRECOMMENDATION)))
                {
                    httpStatusCode = new HttpStatusCodeResult(HttpStatusCode.NotAcceptable, model.ErrorMessage);
                }
                else
                {
                    httpStatusCode = new HttpStatusCodeResult(HttpStatusCode.BadRequest, model.ErrorMessage);
                }

                return httpStatusCode;
            }

            return RedirectToAction("index", new { operationNumber = operationNumber, message = response.ErrorMessage });
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write,RMG Write")]
        public virtual ActionResult NewNote(RiskMatrixViewModel model)
        {
            if (model == null)
            {
                model = new RiskMatrixViewModel();
                model.IsEdit = false;
                model.ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            model.UserComments.Add(new MW.Application.RiskMatrix.ViewModels.UserCommentViewModel()
            {
                Created = DateTime.Now,
                CreatedBy = IDBContext.Current.UserName,
                Text = string.Empty
            });

            return PartialView("~/Areas/RiskMatrix/Views/Controls/NewNote.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult AddNewRowActivity(RiskMatrixViewModel model, string riskMatrixId, string operationNumber, int activityId)
        {
            int riskMatrixIdParsed = default(int);
            model = GetSelectItems(model);

            RiskActivityViewModel activity = new RiskActivityViewModel
            {
                ActivityId = default(int),
                ActivityCode = default(int),
                ActivityDescription = string.Empty,
                CurrentCompletationDate = null,
                TriggerEventOrMilestone = null
            };

            if (activityId != default(int))
            {
                GetActivityByIdRequest requestApp = new GetActivityByIdRequest
                {
                    ActivityId = activityId
                };

                var activityFromModel = GetRiskAcitivyFromModel(model, activityId);
                var resultResponse = new GetActivityByIdResponse();

                if (activityFromModel == null)
                {
                    resultResponse = _riskMastrixService.GetActivitybyId(requestApp);

                    if (!resultResponse.IsValid)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.NoContent, resultResponse.ErrorMessage);
                    }

                    activity = CloneActivity(activity, resultResponse.ItemActivity);
                }
                else
                {
                    activity = CloneActivity(activity, activityFromModel);
                    activity.ManagementStrategy = LogicManagementStrategy(activity.ManagementStrategyValues);
                }
            }

            if (!int.TryParse(riskMatrixId, out riskMatrixIdParsed))
            {
                var code = model.Risks
                    .Where(x => x.CodeGuid == riskMatrixId)
                    .Select(a => a.ResponsePlanActivity)
                    .LastOrDefault()
                    .LastOrDefault();

                if (activity.ActivityCode == default(int))
                {
                    activity.ActivityCode = code != null ? code.ActivityCode + CONSTONE : default(int) + CONSTONE;
                }

                model.SelectedRisk = riskMatrixId;
                model.CodeGuid = riskMatrixId;
                model.Risks.Where(x => x.CodeGuid == riskMatrixId)
                    .Select(s => s.ResponsePlanActivity)
                    .FirstOrDefault()
                    .Add(activity);
            }
            else
            {
                var code = model.Risks
                    .Where(x => x.RiskId == riskMatrixIdParsed)
                    .Select(a => a.ResponsePlanActivity)
                    .LastOrDefault()
                    .LastOrDefault();

                if (activity.ActivityCode == default(int))
                {
                    activity.ActivityCode = code != null ? code.ActivityCode + CONSTONE : default(int) + CONSTONE;
                }

                model.SelectedRisk = riskMatrixIdParsed.ToString();
                model.Risks.Where(x => x.RiskId == riskMatrixIdParsed)
                    .Select(s => s.ResponsePlanActivity)
                    .FirstOrDefault()
                    .Add(activity);
            }

            return PartialView("~/Areas/RiskMatrix/Views/Controls/NewActivity.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult AddNewRowRisk(RiskMatrixViewModel model, string operationNumber)
        {
            model = GetSelectItems(model);
            var listCode = model.Risks.Where(r => r.OperationNumberForRisk == operationNumber).OrderBy(a => a.RiskCode).ToList();
            int lastRiskCode = default(int);

            if (listCode != null && listCode.Count > 0)
            {
                lastRiskCode = listCode.LastOrDefault().RiskCode;
            }

            RiskViewModel risk = new RiskViewModel
            {
                RiskId = 0,
                RiskStatus = new RiskStatusViewModel { StatusCode = string.Empty },
                Probability = new RiskMeasureViewModel { Value = default(int), Description = string.Empty, RiskMeasureId = default(int) },
                Impact = new RiskMeasureViewModel { Description = string.Empty, RiskMeasureId = default(int), Value = default(int) },
                TypeValue = string.Empty,
                SourceRiskId = default(int),
                RiskTypeImpact = new List<RiskTypeImpactViewModel>(),
                Description = string.Empty,
                OperationNumberForRisk = model.OperationNumber,
                OperationRisksId = model.OperationRiskId,
                RiskCode = lastRiskCode + CONSTONE
            };

            model.Risks.Add(risk);

            return PartialView("~/Areas/RiskMatrix/Views/Controls/NewRisk.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult AddDocument(RiskMatrixViewModel model, string documentNumber)
        {
            if (model == null)
            {
                model = new RiskMatrixViewModel();
                model.IsEdit = false;
                model.ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            model.Documents.Add(new DocumentViewModel()
            {
                Created = DateTime.Now,
                CreatedBy = IDBContext.Current.UserLoginName,
                Description = string.Empty,
                DocumentReference = documentNumber
            });

            return PartialView("~/Areas/RiskMatrix/Views/Controls/NewDocument.cshtml", model);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DeleteDocument(RiskMatrixViewModel model, int documentId)
        {
            if (model == null)
            {
                model = new RiskMatrixViewModel();
                model.IsEdit = false;
                model.ErrorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, model.ErrorMessage);
            }

            var resultResponse = _riskMastrixService.DeleteDocumentReference(new DeleteReferenceRequest()
            {
                ItemId = documentId,
                OperationNumber = model.OperationNumber,
                OperationRiskId = model.OperationRiskId
            });

            return Json(resultResponse, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult RecalculateRiskLevel(string ValProbability, string ValImpact, string OperationNumber)
        {
            RecalculateRiskLevelRequest request = new RecalculateRiskLevelRequest
            {
                OperationNumber = OperationNumber,
                ValImpact = ValImpact,
                ValProbability = ValProbability
            };

            var resp = _riskMastrixService.RecalculateRiskLevel(request);

            if (!resp.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, resp.ErrorMessage);
            }

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DeleteRisk(int RiskId, string operationNumber)
        {
            var response = _riskMastrixService.DeleteRisk(new DeleteRiskRequest()
            {
                RiskId = RiskId,
                OperationNumber = operationNumber
            });

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DeleteActivity(int ActivityId, string operationNumber, int RiskId)
        {
            var response = _riskMastrixService.DeleteActivity(new DeleteActivityRequest()
            {
                ActivityId = ActivityId,
                OperationNumber = operationNumber,
                RiskId = RiskId
            });

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult DownloadRiskMatrixExportFile(string operationNumber, string formatType)
        {
            var response = _riskMastrixService.GetRiskMatrixReport(new RiskMatrixReportRequest
            {
                OperationNumber = operationNumber,
                FormatType = formatType
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = string.Format(Literals.RISKS_MATRIX_EXPORT_FILE_NAME_FORMAT, operationNumber);

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

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult GetActivitiesByRisk(RiskMatrixViewModel model, string riskId, string operationNumber)
        {
            if (model == null)
            {
                string errorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, errorMessage);
            }

            int RiskIdInt = default(int);

            IList<RiskActivityViewModel> responseActivities = null;

            if (!int.TryParse(riskId, out RiskIdInt))
            {
                responseActivities = model.Risks
                    .Where(x => x.CodeGuid == riskId)
                    .SelectMany(rp => rp.ResponsePlanActivity)
                    .Where(rpa => rpa.ActivityId != default(int))
                    .ToList();
            }
            else
            {
                responseActivities = model.Risks
                    .Where(x => x.RiskId == RiskIdInt)
                    .SelectMany(rp => rp.ResponsePlanActivity)
                    .Where(rpa => rpa.ActivityId != default(int))
                    .ToList();
            }

            GetAllRiskActivitiesRequest request = new GetAllRiskActivitiesRequest
            {
                OperationNumber = operationNumber
            };

            var resultResponse = _riskMastrixService.GetAllRiskActivities(request);
            if (!resultResponse.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, resultResponse.ErrorMessage);
            }

            var allRiskActivities = resultResponse.Activities.Where(a => !responseActivities.Any(x => x.ActivityId.ToString() == a.Value)).ToList();

            return PartialView("~/Areas/RiskMatrix/Views/Modals/AddActivityModalContent.cshtml", allRiskActivities);
        }

        [Authorize]
        [HttpPost]
        [HasPermission(Permissions = "Risks Write,RMG Write")]
        public virtual ActionResult DeleteUserComment(RiskMatrixViewModel model, int userCommentId)
        {
            if (model == null)
            {
                string errorMessage = Localization.GetText(Literals.RISK_MATRIX_ERROR_LOAD_REQUEST);
                return new HttpStatusCodeResult(HttpStatusCode.NoContent, errorMessage);
            }

            var resultResponse = _riskMastrixService.DeleteUserComment(new DeleteReferenceRequest()
            {
                ItemId = userCommentId,
                OperationNumber = model.OperationNumber,
                OperationRiskId = model.OperationRiskId
            });

            return Json(resultResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult DownloadRiskProjectProfile(string operationNumber, string formatType, string language)
        {
            var response = _riskMastrixService.GetProjectProfileReport(new RiskReportRequest
            {
                OperationNumber = operationNumber,
                FormatType = formatType,
                Language = language
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = operationNumber;

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
                case Literals.WORDX:
                    application = MimeTypeMap.GetMimeType(Literals.WORDX);
                    fileName = fileName + Literals.DOTDOCX;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(Literals.PDF);
                    fileName = fileName + Literals.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual ActionResult DownloadRiskPODReport(string operationNumber, string formatType, string language)
        {
            var response = _riskMastrixService.GetPODReport(new RiskReportRequest
            {
                OperationNumber = operationNumber,
                FormatType = formatType,
                Language = language
            });

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = operationNumber;

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
                case Literals.WORDX:
                    application = MimeTypeMap.GetMimeType(Literals.WORDX);
                    fileName = fileName + Literals.DOTDOCX;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(Literals.PDF);
                    fileName = fileName + Literals.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        private RiskMatrixViewModel GetSelectItems(RiskMatrixViewModel model)
        {
            var resultResponse = _riskMastrixService.GetMasterDataCatalog(new MasterDataRequest
            {
                OperationNumber = model.OperationNumber
            });

            if (!resultResponse.IsValid)
            {
                return model;
            }

            /*mapp elements to list*/
            model.ActivityStatus = resultResponse.ActivityStatus;
            model.RiskStatus = resultResponse.RiskStatus;
            model.RiskProbability = resultResponse.RiskProbability;
            model.RiskImpact = resultResponse.RiskImpact;
            model.RiskType = resultResponse.RiskType;
            model.RiskProbabilityFactor = resultResponse.RiskProbabilityFactor;
            model.ImpactScope = resultResponse.TypeImpact;
            model.RiskManagementStrategy = resultResponse.RiskManagementStrategy;
            model.RiskFundingSource = resultResponse.RiskFundingSource;

            model.IdInactiveStatus = resultResponse.IdInactiveStatus;
            model.IdAcceptStatus = resultResponse.IdAcceptStatus;
            model.OutPuts = resultResponse.Outputs;
            model.OutComes = resultResponse.Outcomes;

            return model;
        }

        private RiskActivityViewModel CloneActivity(RiskActivityViewModel activity, RiskActivityViewModel activityToClone)
        {
            decimal budget = default(decimal);

            if (activityToClone.BudgetStr != null)
            {
                budget = decimal.Parse(activityToClone.BudgetStr.Replace(",", string.Empty));
            }

            activity.ActivityDescription = activityToClone.ActivityDescription;
            activity.ManagementStrategy = activityToClone.ManagementStrategy;
            activity.ActvityStatusId = activityToClone.ActvityStatusId;
            activity.TriggerEventOrMilestone = activityToClone.TriggerEventOrMilestone;
            activity.CurrentCompletationDate = activityToClone.CurrentCompletationDate;
            activity.BudgetStr = activityToClone.BudgetStr;
            activity.Budget = budget;
            activity.FundingSourceId = activityToClone.FundingSourceId;
            activity.Responsible = activityToClone.Responsible;
            activity.Institution = activityToClone.Institution;
            activity.Justification = activityToClone.Justification;
            activity.ActivityCode = activityToClone.ActivityCode;
            activity.ActivityId = activityToClone.ActivityId;
            activity.ManagementStrategyValues = activityToClone.ManagementStrategyValues;

            return activity;
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

        private RiskActivityViewModel GetRiskAcitivyFromModel(RiskMatrixViewModel model, int ActivityId)
        {
            RiskActivityViewModel activity = null;

            activity = model.Risks
                        .SelectMany(s => s.ResponsePlanActivity)
                        .Where(a => a.ActivityId == ActivityId)
                        .FirstOrDefault();

            return activity;
        }

        private IList<ActivityManagementStrategyViewModel> LogicManagementStrategy(string[] managementStrategyValues)
        {
            IList<ActivityManagementStrategyViewModel> modelList = new List<ActivityManagementStrategyViewModel>();

            if (managementStrategyValues != null)
            {
                foreach (var item in managementStrategyValues)
                {
                    int num;
                    int.TryParse(item, out num);
                    modelList.Add(new ActivityManagementStrategyViewModel
                    {
                        ManagementStrategyId = num
                    });
                }
            }

            return modelList;
        }
    }
}