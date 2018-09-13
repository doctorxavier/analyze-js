using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.Architecture.Extensions;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Business.SupervisionPlan.Services.Administration;
using IDB.MW.Business.SupervisionPlan.Services.SupervisionPlan;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.SupervisionPlan;
using IDB.MW.Domain.Models.Architecture.Clauses;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Models.Supervision.SupervisionPlan;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.SupervisionPlan.Models;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.SupervisionPlanModule.Services;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Controllers
{
    public enum SupervisionPlanView
    {
        CriticalProducts = 0,
        Activities = 1,
        Budget = 2,
        Comments = 3
    }

    public partial class SupervisionPlanLegacyController : BaseController
    {
        #region fields
        private readonly ISPAdministrationService _spAdministrationService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;
        private readonly ISpNotificationService _notificationService;
        #endregion

        public SupervisionPlanLegacyController(ISPAdministrationService administrationService, ICatalogService catalogService, ISpNotificationService notificationService)
        {
            _spAdministrationService = administrationService;
            _catalogService = catalogService;
            _notificationService = notificationService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_spAdministrationService, _catalogService, _notificationService);
        }

        public ISupervisionPlanModelRepository SupervisionPlanRepository { get; set; }

        public IConvergenceMasterDataModelRepository MasterDataRepository { get; set; }

        public IRuleEvaluatorService RuleService
        {
            get { return _ruleService; }
            set { _ruleService = value; }
        }

        private IRuleEvaluatorService _ruleService = null;

        private IOperationModelRepository _operationModelRepositoryService = null;
        public virtual IOperationModelRepository OperationModelRepositoryService
        {
            get { return _operationModelRepositoryService; }
            set { _operationModelRepositoryService = value; }
        }

        IReportsGenericRepository _clientGenericRepositoty = null;
        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _clientGenericRepositoty; }
            set { _clientGenericRepositoty = value; }
        }

        public IK2ServiceProxy K2ServiceProxy { get; set; }

        public virtual ActionResult Summary(string operationNumber, int? year, SupervisionPlanView view = SupervisionPlanView.CriticalProducts)
        {
            ViewBag.operationNumber = operationNumber;
            ViewBag.EditMode = false;
            ViewBag.EditableView = view;
            if (!year.HasValue)
                year = DateTime.Now.Year;
            var supervisionPlans = PrepareSummaryView(null, operationNumber, year.Value);
            if (view == SupervisionPlanView.Budget)
                LocalizeAndCompleteBudgetMatrix(supervisionPlans);
            var spService = Globals.Resolve<ISupervisionPlanService>();
            ViewBag.ModalityType = spService.GetModalityCodeById(supervisionPlans[0].SupervisionPlanId);
            ViewBag.SourceList = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ListSource);
            ViewBag.ActivityType = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ActivityType);
            return View(supervisionPlans);
        }

        [HasPermission(Permissions = "Supervision Plan Write")]
        public virtual ActionResult SummaryEdit(string operationNumber, int year, SupervisionPlanView view, bool createNew = false)
        {
            //ViewBag.UploadFileError = ((Exception)Session["IndexDocumentRelationshipError"]).Message;
            ViewBag.EditMode = true;
            ViewBag.LocalizedStatusValue = Localization.GetText("Draft");
            ViewBag.EditableView = view;
            ViewBag.operationNumber = operationNumber;

            SupervisionPlanModel plan = null;
            if (createNew)
                plan = new SupervisionPlanModel
                {
                    Year = year,
                    OperationNumber = operationNumber
                };
            var supervisionPlans = PrepareSummaryView(plan, operationNumber, year);

            if (view == SupervisionPlanView.Budget)
                LocalizeAndCompleteBudgetMatrix(supervisionPlans);

            ViewBag.IsNewPlan = supervisionPlans.Find(p => p.Year == year).ActualVersionIsNew;
            ViewBag.DraftOrModified = supervisionPlans.Find(p => p.Year == year).ActualVersionIsInDraftOrModified;
            ViewBag.Approved = supervisionPlans.Find(p => p.Year == year).ActualVersionIsApproved;
            ViewBag.Modified = supervisionPlans.Find(p => p.Year == year).ActualVersionIsInModified;
            ViewBag.OperationId = supervisionPlans.Find(p => p.Year == year).OperationId;
            ViewBag.OperationNumber = supervisionPlans.Find(p => p.Year == year).OperationNumber;

            return View("Summary", supervisionPlans);
        }

        [HasPermission(Permissions = "Supervision Plan Write")]
        public virtual ActionResult DeleteVersion(int planVersionId, string operationNumber, int year)
        {
            if (planVersionId != -1)
                SupervisionPlanRepository.DeleteSupervisionPlanVersion(planVersionId);

            return RedirectToAction("Summary", new
            {
                operationNumber = operationNumber,
                year = year
            });
        }

        [HasPermission(Permissions = "Supervision Plan Write")]
        public virtual ActionResult ModifyPlan(int planVersionId, string operationNumber, int year, SupervisionPlanView view)
        {
            var plan = SupervisionPlanRepository.GetNewModifiedPlanVersion(planVersionId);

            ViewBag.EditMode = true;
            ViewBag.EditableView = view;
            var supervisionPlans = PrepareSummaryView(plan, operationNumber, year);

            if (view == SupervisionPlanView.Budget)
            {
                LocalizeAndCompleteBudgetMatrix(supervisionPlans);
            }

            ViewBag.IsNewPlan = supervisionPlans.Find(p => p.Year == year).ActualVersionIsNew;
            ViewBag.DraftOrModified = supervisionPlans.Find(p => p.Year == year).ActualVersionIsInDraftOrModified;
            ViewBag.Approved = supervisionPlans.Find(p => p.Year == year).ActualVersionIsApproved;
            ViewBag.Modified = supervisionPlans.Find(p => p.Year == year).ActualVersionIsInModified;
            ViewBag.OperationId = supervisionPlans.Find(p => p.Year == year).OperationId;
            ViewBag.OperationNumber = supervisionPlans.Find(p => p.Year == year).OperationNumber;
            ViewBag.PlanIsModified = true;
            return View("Summary", supervisionPlans);
        }

        //[HasPermission(Permissions = "Supervision Plan Write")]
        public virtual PartialViewResult CriticalProductsPartial(int id)
        {
            var spService = Globals.Resolve<ISupervisionPlanService>();

            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            ViewBag.ModalityType = spService.GetModalityCodeById(id);

            SetLocalizedValues();
            return PartialView(plans.Find(p => p.SupervisionPlanId == id).ActualVersion != null
                                    ? plans.Find(p => p.SupervisionPlanId == id).ActualVersion.CriticalProducts
                                    : new List<CriticalProductModel>());
        }

        public virtual ActionResult CriticalProductsSave(int supervisionPlanVersionId, string operationNumber, int year, List<CriticalProductModel> products, bool returnToEdit, bool isNew, SupervisionPlanView view, bool requestAfterSave)
        {
            var versionId = GetVersionIdToSave(supervisionPlanVersionId, operationNumber, year, isNew);

            if (supervisionPlanVersionId > 0 || isNew)
            {
                SupervisionPlanRepository.UpdateCriticalProducts(versionId, products);
            }

            return RedirectSelector(versionId, operationNumber, year, view, requestAfterSave);
        }

        //[HasPermission(Permissions = "Supervision Plan Write")]
        public virtual PartialViewResult ActivitiesPartial(int id)
        {
            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            SetLocalizedValues();
            var spService = Globals.Resolve<ISupervisionPlanService>();
            ViewBag.ModalityType = spService.GetModalityCodeById(id);
            ViewBag.SourceList = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ListSource);
            ViewBag.ActivityType = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ActivityType);

            var model = spService.AddCriticalProductModels(plans, id);
            
            return PartialView(model);
        }

        public virtual ActionResult ActivitiesSave(int supervisionPlanVersionId, string operationNumber, int year, List<ActivityModel> activities, bool returnToEdit, bool isNew, SupervisionPlanView view, bool requestAfterSave)
        {
            var versionId = GetVersionIdToSave(supervisionPlanVersionId, operationNumber, year, isNew);

            if (supervisionPlanVersionId > 0)
            {
                SupervisionPlanRepository.UpdateActivities(versionId, activities);
            }

            return RedirectSelector(versionId, operationNumber, year, view, requestAfterSave);
        }

        //[HasPermission(Permissions = "Supervision Plan Write")]
        public virtual PartialViewResult BudgetPartial(int id)
        {
            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            LocalizeAndCompleteBudgetMatrix(plans);
            return PartialView(plans.Find(p => p.SupervisionPlanId == id).ActualVersion != null
                                    ? plans.Find(p => p.SupervisionPlanId == id).ActualVersion.Costs
                                    : new List<CostModel>());
        }

        public virtual ActionResult BudgetSave(int supervisionPlanVersionId, string operationNumber, int year, List<CostModel> costs, bool returnToEdit, bool isNew, SupervisionPlanView view, bool requestAfterSave, bool IsModified = false)
        {
            var versionId = GetVersionIdToSave(supervisionPlanVersionId, operationNumber, year, isNew);

            if (supervisionPlanVersionId > 0)
            {
                SupervisionPlanRepository.UpdateCosts(versionId, costs, IsModified);
            }

            return RedirectSelector(versionId, operationNumber, year, view, requestAfterSave);
        }

        public virtual PartialViewResult CommentsPartial(int id)
        {
            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            return PartialView(plans.Find(p => p.SupervisionPlanId == id).ActualVersion != null
                                    ? plans.Find(p => p.SupervisionPlanId == id).ActualVersion.UserComments
                                    : new List<UserCommentModel>());
        }

        public virtual ActionResult CommentsSave(
            int supervisionPlanVersionId,
            string operationNumber,
            int year,
            SupervisionPlanVersionModel comments,
            SupervisionPlanView view = new SupervisionPlanView(),
            bool returnToEdit = false,
            bool isNew = false,
            bool requestAfterSave = true)
        {
            comments.UserComments.ForEach(c =>
                                {
                                    c.ModifiedBy = IDBContext.Current.UserName;
                                    c.CreatedBy = IDBContext.Current.UserName;
                                });

            var versionId = GetVersionIdToSave(supervisionPlanVersionId, operationNumber, year, isNew);

            SupervisionPlanRepository.UpdateCommentPlanVersion(versionId, comments.UserComments);

            if (requestAfterSave)
            {
                string additionalValidators = Request["validator_list_additional_list"];

                return RedirectToAction("CreatePlanVersionApprovalRequest", new
                {
                    planVersionId = versionId,
                    operationNumber = operationNumber,
                    year = year,
                    additionalValidators = additionalValidators
                });
            }
            else
            {
                return RedirectToAction("DetailsPlanVersionApprovalRequest", new
                {
                    planVersionId = versionId,
                    operationNumber = operationNumber,
                    year = year
                });
            }
        }

        [HasPermission(Permissions = "Supervision Plan Request")]
        public virtual ActionResult DetailsPlanVersionApprovalRequest(int planVersionId, string operationNumber, int year, MessageSendRequestCode state = 0, string messageK2 = null)
        {
            var model = SupervisionPlanRepository.GetPlanVersionModel(planVersionId);
            var stages = new List<string> { "SP_DRAFT", "SP_MOD" };
            ViewBag.IsInDraftOrModified = stages.Any(s => s.Equals(model.ValidationStage.Code, StringComparison.InvariantCultureIgnoreCase));
            ViewBag.OperationNumber = operationNumber;
            ViewBag.Year = year;

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(state, false, 2, messageK2);
                ViewData["message"] = message;
            }

            return View(model);
        }

        [HasPermission(Permissions = "Supervision Plan Request")]
        public virtual ActionResult EditPlanVersionApprovalRequest(int planVersionId, string operationNumber, int year)
        {
            var model = SupervisionPlanRepository.GetPlanVersionModel(planVersionId);

            var stages = new List<string> { "SP_DRAFT", "SP_MOD" };
            ViewBag.IsInDraftOrModified = stages.Any(s => s.Equals(model.ValidationStage.Code, StringComparison.InvariantCultureIgnoreCase));
            ViewBag.OperationNumber = operationNumber;
            ViewBag.Year = year;
            return View(model);
        }

        public virtual ActionResult SavePlanVersionApprovalRequest(
            int planVersionId,
            string operationNumber,
            int year,
            List<UserCommentModel> comments)
        {
            SupervisionPlanRepository
                .UpdateCommentPlanVersion(planVersionId, comments);
            return Content(string.Empty);
        }

        [HasPermission(Permissions = "Supervision Plan Request")]
        public virtual ActionResult CreatePlanVersionApprovalRequest(
            int planVersionId,
            string operationNumber,
            int year,
            string additionalValidators = "")
        {
            planVersionId = SupervisionPlanRepository.GetSupervisionPlanVersionIdToValidate(
                planVersionId, IDBContext.Current.UserName);

            var spWorkflow = new SupervisionPlanWorkflow
            {
                EntityId = planVersionId,
                FolioID = "0",
                OperationNumber = operationNumber,
                EntityType = K2CallType.SupervisionPlanVersion.GetStringValue(),
                UserName = IDBContext.Current.UserName,
                UserProfile = IDBContext.Current.FirstRole,
                UserProfileAdditional = additionalValidators
            };

            int operationId = ClientGenericRepository
                .GetOperationIDForOperationNumber(operationNumber);
            decimal currentAmount = OperationModelRepositoryService.CalculateCurrentAmount(
                operationId, 1);
            string validators = SupervisionPlanWorkflowManager.GetValidators(
                _ruleService, currentAmount);

            SupervisionPlanWorkflowManager.SetValidatorsToStartWorkflow(
                validators, additionalValidators, spWorkflow);

            var response = SupervisionPlanWorkflowManager.BeginK2Workflow(
                K2CallType.SupervisionPlanVersion,
                spWorkflow,
                K2CallType.SupervisionPlanVersion.GetStringValue(),
                K2ServiceProxy);

            int statusCode = 0;
            if (response == K2Response.StartWorkFlow_SP.GetStringValue())
            {
                statusCode = 700;
            }
            else if (response == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(response);
                statusCode = 555;
            }
            else
            {
                IDBContext.Current.ErrorMessage(response);
                statusCode = 600;
            }

            return RedirectToAction("DetailsPlanVersionApprovalRequest", new
            {
                planVersionId = planVersionId,
                operationNumber = operationNumber,
                year = year,
                state = statusCode,
                messageK2 = response
            });
        }

        public virtual PartialViewResult PriorPlanVersionPartial(int id)
        {
            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            ViewBag.SelectedYear = plans.Find(p => p.SupervisionPlanId == id).Year;
            var priorVersionModel = new PriorVersionModel();
            priorVersionModel.SupervisionPlanList = plans;
            priorVersionModel.SupervisionPlanVersionList =
                plans.Find(p => p.SupervisionPlanId == id).SupervisionPlanVersions.OrderByDescending(x => x.ValidationDate).ToList();
            return PartialView(priorVersionModel);
        }

        public virtual PartialViewResult WarningSupervisionPlanCreation()
        {
            return PartialView("~/Areas/SupervisionPlan/Views/Shared/_WarningSupervisionPlanCreation.cshtml");
        }

        public virtual PartialViewResult _SaveFirstOnTabChange()
        {
            return PartialView();
        }

        public virtual PartialViewResult _WarningOnDeleteVersion()
        {
            return PartialView();
        }

        public virtual PartialViewResult _ActionButtons(int id)
        {
            var plans = (List<SupervisionPlanModel>)Session["SupervisionPlan"];
            if (plans == null)
                plans = new List<SupervisionPlanModel> { SupervisionPlanRepository.GetById(id) };
            Session["SupervisionPlan"] = plans;
            var model = plans.Find(p => p.SupervisionPlanId == id);
            return PartialView(model);
        }

        public virtual ActionResult ExportSupervisionPlanReport(int SupervisionPlanId, int operation_id, string language, string ExportType)
        {
            string url = string.Empty;

            string reportServer = ConfigurationManager.AppSettings["ParamReportServer"];
            string report = ConfigurationManager.AppSettings["ParamForSupervisionPlan"].ToString();

            url += reportServer + report;

            url += "&SUPERVISION_PLAN_VERSION_ID=" + SupervisionPlanId + "&OPERATION_ID=" + operation_id + "&LANG=" + language + "&rs:Format=" + ExportType;

            return Content(url);
        }

        #region Private Methods

        private int GetVersionIdToSave(int supervisionPlanVersionId, string operationNumber, int year, bool isNew)
        {
            var planModel = isNew ? SupervisionPlanRepository.CreateSupervisionPlan(operationNumber, year) : null;
            var versionId = planModel != null ? planModel.ActualVersion.SupervisionPlanVersionId : supervisionPlanVersionId;

            if (!isNew && versionId == -1)
            {
                planModel = SupervisionPlanRepository.GetByOperationAndYear(operationNumber, year);
                var versionIdToModify = planModel.ActualVersion.SupervisionPlanVersionId;
                planModel = SupervisionPlanRepository.SaveNewModifiedVersion(versionIdToModify);
                versionId = planModel.ActualVersion.SupervisionPlanVersionId;
            }

            return versionId;
        }

        private List<SupervisionPlanModel> PrepareSummaryView(SupervisionPlanModel plan, string operationNumber, int year)
        {
            var availableYears = new List<int> { DateTime.Today.Year, DateTime.Today.Year + 1 };

            if (plan != null)
                availableYears.Remove(year);

            var supervisionPlans = SupervisionPlanRepository.GetByOperation(operationNumber);

            if (plan != null)
                supervisionPlans.Add(plan);

            //Orden anterior: supervisionPlans.Sort((s1, s2) => s1.Year.CompareTo(s2.Year));
            supervisionPlans = supervisionPlans.OrderByDescending(s => s.Year).ToList();

            foreach (SupervisionPlanModel spversionModel in supervisionPlans)
            {
                if (spversionModel.SupervisionPlanVersions != null)
                {
                    spversionModel.SupervisionPlanVersions = spversionModel.SupervisionPlanVersions.OrderByDescending(x => x.ValidationDate).ToList();
                }
            }

            if (supervisionPlans.All(s => s.Year != year) && supervisionPlans.Count > 0)
                year = supervisionPlans.First().Year;

            foreach (var sp in supervisionPlans)
            {
                if (!sp.ActualVersionIsNew)
                    sp.ActualVersionIsNew = sp.ActualVersion == null;

                sp.ActualVersionIsApproved = sp.ActualVersion != null &&
                                                  sp.ActualVersion.ValidationStage.Code.Equals("SP_APPR", StringComparison.InvariantCultureIgnoreCase);
                sp.ActualVersionEditable = !(sp.ActualVersion != null &&
                                             sp.ActualVersion.ValidationStage.Code.Contains("SP_REV"));
                sp.ActualVersionCanModify = sp.ActualVersion != null &&
                                            sp.ActualVersion.ValidationStage.Code.Equals("SP_APPR", StringComparison.InvariantCultureIgnoreCase);
                var stages = new List<string> { "SP_DRAFT", "SP_MOD" };
                sp.ActualVersionIsInDraftOrModified = sp.ActualVersion == null || stages.Any(s => s.Equals(sp.ActualVersion.ValidationStage.Code, StringComparison.InvariantCultureIgnoreCase));
                sp.ActualVersionIsInModified = sp.ActualVersion != null &&
                                                  sp.ActualVersion.ValidationStage.Code.Equals("SP_MOD", StringComparison.InvariantCultureIgnoreCase);
                sp.SupervisionPlanId = sp.ActualVersion == null ? -1 : sp.SupervisionPlanId;
            }

            SetLocalizedValues();

            ViewBag.OperationNumber = operationNumber;
            ViewBag.AvailableYears = availableYears;
            ViewBag.SelectedYear = year;
            if (supervisionPlans.Count > 0)
            {
                ViewBag.OperationId = supervisionPlans.Last().OperationId;
                SetLocalizedModel(supervisionPlans);
                Session["SupervisionPlan"] = supervisionPlans;
            }

            return supervisionPlans;
        }

        private ActionResult RedirectSelector(int planVersionId, string operationNumber, int year, SupervisionPlanView view, bool requestAfterSave)
        {
            if (requestAfterSave)
            {
                return RedirectToAction("DetailsPlanVersionApprovalRequest", new
                {
                    planVersionId = planVersionId,
                    operationNumber = operationNumber,
                    year = year
                });
            }
            else
            {
                return RedirectToAction("Summary", new
                {
                    operationNumber = operationNumber,
                    year = year,
                    view = view
                });
            }
        }

        private void LocalizeAndCompleteBudgetMatrix(IEnumerable<SupervisionPlanModel> supervisionPlans)
        {
            var bCategory = MasterDataRepository.GetMasterDataModels("BUDGET_CATEGORY");
            var bFundingSource = MasterDataRepository.GetMasterDataModels("FUNDING_SOURCE");

            ViewBag.LocalizedBudgetMatrixRows = bCategory.ToDictionary(c => c.ConvergenceMasterDataId, c => BusinessLogic.MasterDataGetLocaleName(c));
            ViewBag.LocalizedBudgetMatrixCols = bFundingSource.ToDictionary(fs => fs.ConvergenceMasterDataId, fs => BusinessLogic.MasterDataGetLocaleName(fs));
            ViewBag.EmptyCosts = (from md2 in bCategory
                                  from md1 in bFundingSource
                                  select new CostModel
                                  {
                                      BudgetCategory = md2,
                                      BudgetCategoryId = md2.ConvergenceMasterDataId,
                                      FundingSource = md1,
                                      FundingSourceId = md1.ConvergenceMasterDataId,
                                      Budget = 0,
                                      CostId = -1,
                                  }).ToList();

            foreach (var sp in supervisionPlans.Where(p => p.ActualVersion != null))
            {
                var costs = (from md2 in bCategory
                             from md1 in bFundingSource
                             let cost = sp.ActualVersion.Costs.Find(c => c.BudgetCategoryId == md2.ConvergenceMasterDataId && c.FundingSourceId == md1.ConvergenceMasterDataId)
                             select new CostModel
                             {
                                 BudgetCategory = md2,
                                 BudgetCategoryId = md2.ConvergenceMasterDataId,
                                 FundingSource = md1,
                                 FundingSourceId = md1.ConvergenceMasterDataId,
                                 Budget = cost != null ? cost.Budget : 0,
                                 CostId = cost != null ? cost.CostId : -1,
                             }).ToList();
                sp.ActualVersion.Costs = costs;
            }
        }

        private void SetLocalizedValues()
        {
            var dQuarter = MasterDataRepository.GetMasterDataModels("QUARTER");
            var dActivities = MasterDataRepository.GetMasterDataModels("MAIN_ACTIVITY");

            ViewBag.LocalizedQuarterValue = dQuarter.ToDictionary(d => d.ConvergenceMasterDataId, d => BusinessLogic.MasterDataGetLocaleName(d));
            ViewBag.LocalizedMainActivities = dActivities.ToDictionary(d => d.ConvergenceMasterDataId, d => BusinessLogic.MasterDataGetLocaleName(d));
        }

        private void SetLocalizedModel(IEnumerable<SupervisionPlanModel> supervisionPlan)
        {
            foreach (var sp in supervisionPlan.Where(s => s.ActualVersion != null))
            {
                sp.ActualVersion.ValidationStage.Name = BusinessLogic.MasterDataGetLocaleName(sp.ActualVersion.ValidationStage);
                foreach (var spv in sp.SupervisionPlanVersions)
                {
                    spv.ValidationStage.Name = BusinessLogic.MasterDataGetLocaleName(spv.ValidationStage);
                }
            }
        }

        #endregion
    }
}
