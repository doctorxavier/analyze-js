using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.SupervisionPlanModule.Services;
using IDB.MW.Business.SupervisionPlan.Services.Administration;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;
using IDB.MW.Business.SupervisionPlan.Services.SupervisionPlan;
using IDB.MW.Business.SupervisionPlan.ViewModels.SupervisionPlan;
using IDB.MW.Domain.Models.Supervision.SupervisionPlan;
using IDB.Presentation.MVC4.Areas.SupervisionPlan.Models;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Domain.Session;
using IDB.Architecture.Security.Messages.UserIdentity;
using IDB.Architecture.Security;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Controllers
{
    public partial class SupervisionPlanController : BaseController
    {
        #region fields

        private readonly ISupervisionPlanService _supervisionPlanService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;

        #endregion

        public SupervisionPlanController(
            ISupervisionPlanService supervisionPlanService,
            ICatalogService catalogService,
            ISPAdministrationService spAdministrationService,
            ISpNotificationService notificationService)
        {
            _supervisionPlanService = supervisionPlanService;
            _catalogService = catalogService;
            _viewModelMapperHelper = new ViewModelMapperHelper(
                spAdministrationService,
                catalogService,
                notificationService);
        }

        public virtual ActionResult Summary(string operationNumber, int? year, int? tab, bool? isInTask)
        {
            var model = _supervisionPlanService.GetSummary(operationNumber, year ?? 0);

            var yearSp = 0;

            if (model.SummaryModel.SupervisionPlan != null)
            {
                yearSp = model.SummaryModel.SupervisionPlan.Year;
            }

            DataCommon(operationNumber, yearSp, tab);

            if (!model.IsValid)
            {
                ViewBag.ErrorMessage = model.ErrorMessage;
            }

            ViewBag.isInTask = isInTask ?? false;

            if (!string.IsNullOrEmpty(model.OperationParent))
            {
                return RedirectToAction(
                    "OperationParent",
                    new { opNumber = model.OperationParent });
            }

            return View(model.SummaryModel);
        }

        public virtual ActionResult OperationParent(string opNumber)
        {
            return View(model: opNumber);
        }

        public virtual ActionResult GetSummary(string operationNumber, int supervisionPlanVersionId)
        {
            var year = _supervisionPlanService.GetYearSummary(supervisionPlanVersionId);
            return RedirectToAction("Summary", new { operationNumber, year, isInTask = true });
        }

        public virtual ActionResult SummaryEdit(string operationNumber, int year, int tab)
        {
            var model = _supervisionPlanService.GetSummary(operationNumber, year);

            ViewBag.EditMode = true;
            DataCommon(operationNumber, year, tab);

            ViewBag.IdAdmMission =
                _catalogService.GetConvergenceMasterDataIdByCode(
                    SpGlobalValues.AdmMission,
                    SpGlobalValues.ActivityType).Id;

            var stage = model.SummaryModel.SupervisionPlan.ActualVersion.ValidationStage;

            ViewBag.NoEditCompletitionDate = stage.Code.Equals(SpGlobalValues.SpDraft) ||
                                             stage.Code.Equals(SpGlobalValues.SpRej);

            if (!model.IsValid)
            {
                ViewBag.ErrorMessage = model.ErrorMessage;
            }

            return View(model.SummaryModel);
        }

        public virtual PartialViewResult PriorPlanVersionPartial(int supervisioPlanId)
        {
            var response = _supervisionPlanService.GetSupervisonPlanVersion(supervisioPlanId);
            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            var model = response.SupervisionPlanVersion;

            return PartialView("Partial/PriorPlanVersionPartial", model);
        }

        public virtual PartialViewResult ValidateNewCriticalPartial()
        {
            return PartialView("Partial/Popup/MsgValidateNewCriticalProductPopup");
        }

        public virtual ActionResult ExportSupervisionPlanReport(
            int supervisionPlanId,
            int operationId,
            string language,
            string exportType,
            string modality)
        {
            var reportServer = ConfigurationManager.AppSettings["ParamReportServer"];
            var report = modality == SpGlobalValues.DetailedId
                ? ConfigurationManager.AppSettings["ParamForSupervisionPlanDet"]
                : ConfigurationManager.AppSettings["ParamForSupervisionPlan"];

            var url = reportServer + report;

            url += "&SUPERVISION_PLAN_VERSION_ID=" + supervisionPlanId +
                "&OPERATION_ID=" + operationId +
                "&LANG=" + language +
                "&rs:Format=" + exportType;

            return Json(url);
        }

        public virtual JsonResult SaveCriticalProducSimplified(
            IList<CriticalProductModel> dataSave,
            int spVersionId)
        {
            var response = _supervisionPlanService.CriticalProductSimplifiedSave(
                dataSave,
                spVersionId);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HasPermission(Permissions = "Supervision Plan Request")]
        public virtual ActionResult DetailsPlanVersionApprovalRequest(
            int planVersionId,
            string operationNumber,
            int year,
            MessageSendRequestCode state = 0,
            string messageK2 = null)
        {
            var model = _supervisionPlanService.SupervisonPlanVersion(planVersionId);
            if (!model.IsValid)
            {
                ViewBag.ErrorMessage = model.ErrorMessage;
            }

            var stages = new List<string> { "SP_DRAFT", "SP_MOD" };
            ViewBag.IsInDraftOrModified = stages
                .Any(s =>
                    s.Equals(
                        model.SupervisionPlanVersion.ValidationStage.Code,
                        StringComparison.InvariantCultureIgnoreCase));
            ViewBag.OperationNumber = operationNumber;
            ViewBag.Year = year;

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(
                    state,
                    false,
                    2,
                    messageK2);
                ViewData["message"] = message;
            }

            return View(model.SupervisionPlanVersion);
        }

        public virtual JsonResult ActivitiesSaveDetailed(
            string operationNumber,
            int year,
            IList<ActivitiesDetailedModel> activities,
            string delactivities)
        {
            var activityModelList = new List<ActivityModel>();
            if (activities != null)
            {
                activityModelList = activities.Select(activity => new ActivityModel
                {
                    ActivityId = Convert.ToInt32(activity.Id),
                    MainActivityId = Convert.ToInt32(activity.Type),
                    Description = activity.Description,
                    PlannedDate = ConvertDate(activity.PlaneedDate),
                    FundingSourceId = ConvertInt(activity.Source),
                    Cost = Convert.ToDecimal(activity.Cost),
                    CompletionDate = ConvertDate(activity.CompletitionDate),
                    CriticalProduct = (activity.Criticals != null) ?
                        activity.Criticals.Split(',').Select(critical => new CriticalProductModel
                        {
                            CriticalProductId = Convert.ToInt32(critical)
                        }).ToList()
                            : null,
                    ActivityDocuments = ActivtyDocument(activity.Document, activity.Id)
                }).ToList();
            }

            var response = _supervisionPlanService.UpdateActivitiesDetailed(
                operationNumber, year, activityModelList, delactivities);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult DeleteDocumentsActivities(string documents)
        {
            if (documents.Length > 0)
            {
                var response = _supervisionPlanService.RemoveDocuments(documents);

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public virtual PartialViewResult GetDocumentInfo(string documentReference)
        {
            var response = _supervisionPlanService.GetDocumentInfo(documentReference);

            return PartialView("Partial/EditMode/ActivityDocumentRow", response);
        }

        public virtual JsonResult ActivitiesSave(
            int supervisionPlanVersionId,
            IList<Models.ActivitiesModel> activities,
            string delactivities)
        {
            var activityModelList = new List<ActivityModel>();
            if (activities != null)
            {
                activityModelList = activities.Select(activity => new ActivityModel
                {
                    ActivityId = Convert.ToInt32(activity.Id),
                    MainActivityId = Convert.ToInt32(activity.Type),
                    Planned = Convert.ToInt32(activity.Planeed),
                    Actual = Convert.ToInt32(activity.Actual),
                    ActivityDocuments = ActivtyDocument(activity.Document, activity.Id)
                }).ToList();
            }

            var response = _supervisionPlanService.UpdateActivities(
                supervisionPlanVersionId, activityModelList, delactivities);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetRowCriticalSimplified()
        {
            var model = new CriticalProductModel();
            ViewBag.Quarter = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.Quarter);
            ViewBagCompilance();
            ViewBag.Completed = _viewModelMapperHelper.GetInputBool();
            return PartialView("Partial/EditMode/CriticalSimplifiedRow", model);
        }

        public virtual ActionResult GetRowActivityDetailed()
        {
            ViewBag.SourceList = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.ListSource);
            ViewBag.ActivityType = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.ActivityType);
            return PartialView("Partial/EditMode/ActivityDetailedRow");
        }

        public virtual ActionResult GetRowActivitySimplified()
        {
            ViewBag.ActivityType = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.ActivityType);
            return PartialView("Partial/EditMode/ActivitySimplifiedRow");
        }

        public virtual PartialViewResult OutputsTable(string operationNumber)
        {
            var response = _supervisionPlanService.GetOutputs(operationNumber);
            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            return PartialView("Partial/Popup/OutputsTable", response.Outputs);
        }

        public virtual PartialViewResult RisksTable(string operationNumber)
        {
            var response = _supervisionPlanService.GetRisks(operationNumber);
            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            return PartialView("Partial/Popup/RisksTable", response.Risks);
        }

        public virtual PartialViewResult CriticalTable(string operationNumber, int year)
        {
            var response = _supervisionPlanService.GetCritical(operationNumber, year);
            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            return PartialView("Partial/Popup/CriticalProductsTable", response.Critical);
        }

        public virtual JsonResult SaveCriticalDetailed(
            int spVersionId, List<spCriticalProductDetailedModel> dataSave, string delcritical)
        {
            var criticalModelList = new List<CriticalProductDetailedModel>();
            if (dataSave != null)
            {
                foreach (var critical in dataSave)
                {
                    var newCritical = new CriticalProductDetailedModel
                    {
                        CriticalProductId = Convert.ToInt32(critical.Id),
                        Description = critical.Description,
                        PlannedDate = ConvertDate(critical.PlannedDate),
                        DetailedComplianceId = ConvertInt(critical.Compliance),
                        CompletionDate = ConvertDate(critical.CompletionDate),
                        QuarterName = critical.QuarterName
                    };

                    if (critical.Outputs != null)
                    {
                        var outputs = new List<OutputModel>();
                        foreach (var output in critical.Outputs)
                        {
                            var milestone = new List<MilestonesModel>();

                            if (!string.IsNullOrEmpty(output.Milestones))
                            {
                                foreach (
                                    var milestoneOutput in output.Milestones.Substring(
                                        0, output.Milestones.Length - 1).Split(','))
                                {
                                    milestone.Add(new MilestonesModel
                                    {
                                        MilestoneId = Convert.ToInt32(milestoneOutput)
                                    });
                                }
                            }

                            outputs.Add(new OutputModel
                            {
                                Id = Convert.ToInt32(output.Output),
                                Milestones = milestone
                            });
                        }

                        newCritical.Outputs = outputs;
                    }

                    if (!string.IsNullOrEmpty(critical.Risks))
                    {
                        var risks = new List<RiskModel>();
                        foreach (var risk in critical.Risks.Substring(
                            0, critical.Risks.Length - 1).Split(','))
                        {
                            risks.Add(new RiskModel
                            {
                                Id = Convert.ToInt32(risk)
                            });
                        }

                        newCritical.Risks = risks;
                    }

                    criticalModelList.Add(newCritical);
                }
            }

            var result = _supervisionPlanService.SaveCriticalDetailed(
                spVersionId,
                criticalModelList,
                delcritical);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetRowCriticalDetailed()
        {
            var model = new CriticalProductModel();
            ViewBag.Quarter = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.Quarter);
            ViewBagCompilance();
            ViewBag.Completed = _viewModelMapperHelper.GetInputBool();
            ViewBag.SpCompletedNew = _catalogService.GetConvergenceMasterDataIdByCode(
                SpGlobalValues.SpCompleted,
                SpGlobalValues.DetailedCompliance);
            return PartialView("Partial/EditMode/CriticalDetailedRow", model);
        }

        public virtual JsonResult SaveBudget(int spVersionId, List<CostModel> cost)
        {
            var response = _supervisionPlanService.SaveSupervisionPlanBudget(spVersionId, cost);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult UnlinkMission(string missionNumber)
        {
            var response = _supervisionPlanService.UnlinkMission(missionNumber);
            return Json(response);
        }

        public virtual JsonResult CreateNewSupervisionPlan(string operationNumber)
        {
            var response = _supervisionPlanService.CreateSimplifiedSupervisionPlan(operationNumber);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private DateTime? ConvertDate(string date)
        {
            DateTime? result = null;
            if (!string.IsNullOrEmpty(date))
            {
                result = DateTime.ParseExact(date, "dd MMM yyyy", CultureInfo.InvariantCulture);
            }

            return result;
        }

        private int? ConvertInt(string value)
        {
            int? result = null;
            if (!string.IsNullOrEmpty(value))
            {
                result = Convert.ToInt32(value);
            }

            return result;
        }

        private List<ActivityDocumentModel> ActivtyDocument(
            List<DocumentModel> document,
            string activityId)
        {
            var result = new List<ActivityDocumentModel>();

            if (document != null)
            {
                result = document.Select(doc => new ActivityDocumentModel
                {
                    ActivityId = Convert.ToInt32(activityId),
                    DocumentId = Convert.ToInt32(doc.Id),
                    Document = new MW.Domain.Models.Global.DocumentModel
                    {
                        DocumentId = Convert.ToInt32(doc.Id),
                        Description = doc.Description,
                        DocumentReference = doc.DocNumber
                    }
                }).ToList();
            }

            return result;
        }

        private void DataCommon(string operationNumber, int year, int? tab)
        {
            ViewBag.Loader = true;
            ViewBag.Year = year;
            ViewBag.LoadCollapse = year > 0;
            ViewBag.Tab = tab ?? 1;
            ViewBag.OperationNumber = operationNumber;
            ViewBag.SourceList = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.ListSource);
            ViewBag.ActivityType = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.ActivityType);
            ViewBag.Quarter = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.Quarter);

            ViewBagCompilance();
        }

        private void ViewBagCompilance()
        {
            var spCompleted = _catalogService.GetConvergenceMasterDataIdByCode(
                SpGlobalValues.SpCompleted,
                SpGlobalValues.DetailedCompliance);
            ViewBag.SpCompleted = spCompleted;
            var spCompletedUp = _catalogService.GetConvergenceMasterDataIdByCode(
                SpGlobalValues.SpCompletedUp,
                SpGlobalValues.DetailedCompliance);
            ViewBag.Completed = _viewModelMapperHelper.GetInputBool();

            var compilanceElements = _viewModelMapperHelper.GetListMasterData(
                SpGlobalValues.DetailedCompliance);

            var compilance100 = compilanceElements.Find(
                x => x.Value.Equals(spCompleted.Id.ToString()));
            compilanceElements.RemoveAt(
                compilanceElements.FindIndex(
                    x => x.Value.Equals(spCompleted.Id.ToString())));

            var compilanceUp100 = compilanceElements.Find(
                x => x.Value.Equals(spCompletedUp.Id.ToString()));
            compilanceElements.RemoveAt(
                compilanceElements.FindIndex(x => x.Value.Equals(spCompletedUp.Id.ToString())));

            var newCompilanceElements = compilanceElements.OrderBy(x => x.Text).ToList();

            newCompilanceElements.Insert(newCompilanceElements.Count, compilanceUp100);
            newCompilanceElements.Insert(newCompilanceElements.Count, compilance100);

            ViewBag.Compliance = newCompilanceElements;
        }
    }
}