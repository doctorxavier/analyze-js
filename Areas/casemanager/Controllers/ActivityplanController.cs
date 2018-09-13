using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Newtonsoft.Json;

using IDB.Architecture;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.OPUSModule.Services.CofinancingService;
using IDB.MW.Application.Reformulation.Services;
using IDB.MW.Business.DEMModule.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Dem;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.casemanager.Controllers
{
    public partial class ActivityplanController : BaseController
    {
        public virtual ActionResult Index()
        {
            var cmBusiness = new CMBusiness(IDBContext.Current.Operation);
            if (cmBusiness.Context.HasActivityPlan &&
                cmBusiness.Context.ActivityPlan.CaseTemplateId.HasValue)
            {
                return RedirectToAction("ActivityItems");
            }
            else
            {
                return RedirectToAction("SelectTemplate");
            }
        }

        public virtual ActionResult ActivityItems()
        {
            return View();
        }

        public virtual ActionResult SelectTemplate()
        {
            ViewBag.templates = JsonConvert.SerializeObject(
                new CMBusiness(IDBContext.Current.Operation)
                .GetAvailableTemplates().Select(tpl => new
                {
                    Name = tpl.Name,
                    Id = tpl.Id
                }));
            return View();
        }

        [HasPermission(Permissions = "case write")]
        public virtual ActionResult CreateActivityPlan(int selectedTemplate)
        {
            var cmb = CMBusiness.Get(IDBContext.Current.Operation);

            var items = cmb.Context.ActivityPlanItems;

            int activityPlanId = cmb.Context.ActiveActivityPlan.Id;

            var cofinancingService = Globals.Resolve<ICofinancingService>();

            var cofinCounterpartFundDetailIds = 
                GetCofinCounterpartFundDetail(cofinancingService, activityPlanId);

            if (cofinCounterpartFundDetailIds.Any())
            {
                cofinancingService.UpdateActivityPlan(cofinCounterpartFundDetailIds, null);
            }

            cmb.DeleteCurrentActivityPlan();

            int newActivityPlanId = cmb.CreateActivityPlan(selectedTemplate, items, true).Id;

            if (cofinCounterpartFundDetailIds.Any())
            {
                cofinancingService
                    .UpdateActivityPlan(cofinCounterpartFundDetailIds, newActivityPlanId);
            }

            cmb = CMBusiness.Get(IDBContext.Current.Operation);

            if (cmb.Context.LastCompletedActivityItem != null)
                cmb.UpdateActivityItem(cmb.Context.LastCompletedActivityItem, true);

            UpdateActivityPlanCompAppr(cmb);

            UpdateReferencesToNewActivityPlan(
                cmb.Context.ActiveActivityPlan.OperationId,
                newActivityPlanId,
                activityPlanId);

            return Content("ok");
        }

        private void UpdateActivityPlanCompAppr(CMBusiness cmb)
        {
            if (CheckActivityPlanCode(cmb.Context.ActivityPlan.CaseTemplate.Code))
            {
                var demBusinessService = Globals.Resolve<IDEMService>();
                var isDemRequired = demBusinessService.IsDEMRequired(IDBContext.Current.Operation);
                if (isDemRequired)
                {
                    if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.DEMV))
                    {
                        cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.DEMV, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                    }
                }

                var operationFund = cmb.Context.Operation.FundOperation;

                if (operationFund != null && operationFund.Any())
                {
                    if (operationFund.Any(q => q.Fund.FundCode == FundCode.SCX || q.Fund.FundCode == FundCode.CTF))
                    {
                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.CGCM))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.CGCM, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.FASIP))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.FASIP, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.VBFC))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.VBFC, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.CERT))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.CERT, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }
                    }

                    if (operationFund.Any(q => q.Fund.FundCode == FundCode.CHC || q.Fund.FundCode == FundCode.KIF))
                    {
                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.CGCM))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.CGCM, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.SPPD))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.SPPD, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.FAS))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.FAS, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.VBFC))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.VBFC, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }

                        if (!cmb.Context.ActivityPlan.ActivityItems.Any(q => q.Code == CMConstants.DefaultActivityItems.CERT))
                        {
                            cmb.InsertSubTemplate(CMConstants.DefaultActivityItems.CERT, CMConstants.ActivityItemStatus.NotStarted.MasterId);
                        }
                    }
                }
            }
        }

        private bool CheckActivityPlanCode(string code)
        {
            bool isActivityPlanValid = false;

            switch (code)
            {
                case DemGlobalValues.COMP_APPR:
                    isActivityPlanValid = true;
                    break;

                case DemGlobalValues.COMP_PBP:
                    isActivityPlanValid = true;
                    break;

                case DemGlobalValues.PARTIAL_SG:
                    isActivityPlanValid = true;
                    break;

                case DemGlobalValues.PARTIAL_PBP:
                    isActivityPlanValid = true;
                    break;
            }

            return isActivityPlanValid;
        }

        void UpdateReferencesToNewActivityPlan(
            int operationId,
            int idNewActivityPlan,
            int activityPlanId)
        {
            var reformulationService = Globals.Resolve<IReformulationService>();

            reformulationService
                .UpdateActivityPlan(operationId, idNewActivityPlan, activityPlanId);
        }

        private List<int> GetCofinCounterpartFundDetail(
            ICofinancingService cofinancingService, int activityPlanId)
        {
            var cofinCounterpartFundDetail =
                cofinancingService.GetCounterpartFinancingDetail(activityPlanId);

            if (cofinCounterpartFundDetail.IsValid &&
                cofinCounterpartFundDetail.CounterpartFinancingDetails.Any())
            {
                return cofinCounterpartFundDetail
                    .CounterpartFinancingDetails.Select(coun => coun.Id).ToList();
            }

            return new List<int>();
        }
    }
}
