using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.Contracts.ModelRepositories.Visit;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Visit;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVCExtensions;
using IDB.Architecture.Logging;

namespace IDB.Presentation.MVC4.Areas.Visits.Controllers
{
    public partial class VisitsController : MVC4.Controllers.ConfluenceController
    {
        public enum VisitsPlanView
        {
            Visit = 0,
        }

        #region wcf services repositories
        public IConvergenceMasterDataModelRepository MasterDataRepository { get; set; }
        public IVisitModelRepository VisitRepository { get; set; }

        private IBasicDataModelRepository _clientBasicData = null;
        public virtual IBasicDataModelRepository clientBasicData
        {
            get { return _clientBasicData; }
            set { _clientBasicData = value; }
        }
        #endregion

        public virtual PartialViewResult VisitsPlanSummary(string operationNumber, int? year, MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage, VisitsPlanView view = VisitsPlanView.Visit)
        {
            ViewBag.EditMode = false;
            ViewBag.EditableView = view;
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var availableYears = new List<int> { DateTime.Today.Year, DateTime.Today.Year + 1 };
            var visitPlans = PrepareSummaryView(null, operationNumber, year.Value);
            ViewBag.OperationNumber = operationNumber;
            SetLocalizedValues();
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            return PartialView(visitPlans);
        }

        public virtual ActionResult PlanCreateSumary(string operationNumber, int year, VisitsPlanView view = VisitsPlanView.Visit)
        {
            var availableYears = new List<int> { DateTime.Today.Year, DateTime.Today.Year + 1 };
            ViewBag.operationNumber = operationNumber;
            ViewBag.EditMode = false;
            ViewBag.EditableView = view;
            SetLocalizedValues();

            if (availableYears[0] == year || availableYears[1] == year)
            {
                return RedirectToAction("SelectYear", new { operationNumber = operationNumber, year = year });
            }
            else
            {
                return HttpNotFound();
            }
        }

        public virtual PartialViewResult _SaveFirstOnTabChange()
        {
            return PartialView();
        }

        public virtual PartialViewResult _ActionButtons(int id)
        {
            var plans = (List<VisitPlanModel>)ViewBag.VisitPlan;
            if (plans == null)
            {
                plans = new List<VisitPlanModel> { VisitRepository.GetById(id) };
                ViewBag.VisitPlan = plans;
            }

            return PartialView(plans.Find(p => p.VisitPlanId == id));
        }

        public virtual PartialViewResult _WarningOnDeleteVersion(int planVersionId)
        {
            ViewBag.planVersionId = planVersionId;
            return PartialView();
        }

        public virtual PartialViewResult _WarningOnDeleteVisits()
        {
            return PartialView();
        }

        [HasPermission(Permissions = "Visit Plan Write")]
        public virtual ActionResult DeleteVersion(int? planVersionId, int year, string operationNumber = "")
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            if ((planVersionId == null) || (operationNumber == string.Empty) || (year == 0) || (planVersionId == 0))
            {
                messageStatus = MessageNotificationCodes.DeleteDataFail;
                operationNumber = IDBContext.Current.Operation;
                year = DateTime.Now.Year;
            }
            else
            {
                ViewBag.planVersionId = planVersionId;
                messageStatus = MessageNotificationCodes.SaveDataSucessfully;
                if (planVersionId != -1)
                {
                    VisitRepository.DeleteVisitPlan((int)planVersionId);
                }

                if (year == DateTime.Now.Year)
                {
                    year++;
                }
                else
                {
                    year--;
                }
            }

            return RedirectToAction("VisitsPlanSummary", new
            {
                operationNumber = operationNumber,
                year = year,
                messageStatus = messageStatus
            });
        }

        public virtual ActionResult SavePlan(string operationNumber, string years)
        {
            ViewBag.operationNumber = operationNumber;
            ViewBag.EditMode = true;
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            if (!VisitRepository.CreateVisitPlan(operationNumber, Convert.ToInt32(years)))
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            int yearsConverted = Convert.ToInt32(years);

            return RedirectToAction("VisitsPlanSummary", new
            {
                operationNumber = operationNumber,
                year = yearsConverted,
                messageStatus = messageStatus
            });
        }

        public virtual PartialViewResult SelectYear(string operationNumber, int year, VisitsPlanView view = VisitsPlanView.Visit)
        {
            var visitPlans = PrepareSummaryView(null, operationNumber, year);
            int Yearadd = DateTime.Today.Year + 1;
            List<SelectListItem> availableYears = new List<SelectListItem>();
            availableYears.Add(new SelectListItem { Selected = true, Text = Localization.GetText("Select year"), Value = "0" });
            foreach (var plan in visitPlans)
            {
                if (plan.Year == Yearadd)
                {
                    Yearadd = Yearadd - 1;
                }

                availableYears.Add(new SelectListItem { Text = Convert.ToString(Yearadd), Value = Convert.ToString(Yearadd), Selected = false });
            }

            if (visitPlans.Count == 0)
            {
                availableYears.Add(new SelectListItem { Text = Convert.ToString(DateTime.Now.Year), Value = Convert.ToString(DateTime.Now.Year), Selected = false });
                availableYears.Add(new SelectListItem { Text = Convert.ToString(DateTime.Now.Year + 1), Value = Convert.ToString(DateTime.Now.Year + 1), Selected = false });
            }

            ViewBag.years = availableYears;
            ViewBag.OperationNumber = operationNumber;

            return PartialView();
        }

        public virtual PartialViewResult IndexVisitsPartial(int id)
        {
            ViewBag.EditMode = true;
            SetLocalizedValues();
            VisitPlanModel planModel = new VisitPlanModel()
            {
                Visits = VisitRepository.GetVisitModel(id),
                VisitPlanId = id,
            };

            return PartialView(planModel);
        }

        [HasPermission(Permissions = "Visit Plan Write")]
        public virtual ActionResult Edit(string operationNumber, int year, VisitsPlanView view = VisitsPlanView.Visit, bool createNew = false)
        {
            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
            {
                ViewBag.UploadFileError = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;
            }

            ViewBag.EditMode = true;
            ViewBag.EditableView = view;
            ViewBag.operationNumber = operationNumber;
            var visitsPlans = PrepareSummaryView(null, operationNumber, year);
            VisitPlanModel plan = null;

            if (createNew)
            {
                plan = new VisitPlanModel
                {
                    Year = year,
                    OperationNumber = operationNumber
                };
            }

            if (year == DateTime.Now.Year)
            {
                visitsPlans = PrepareSummaryView(plan, operationNumber, year);
                ViewBag.OperationId = visitsPlans.Find(p => p.Year == year).OperationId;
                ViewBag.OperationNumber = visitsPlans.Find(p => p.Year == year).OperationNumber;
            }
            else if (year == DateTime.Now.Year + 1)
            {
                year = DateTime.Now.Year + 1;
                visitsPlans = PrepareSummaryView(plan, operationNumber, year);
            }

            return View("VisitsPlanSummary", visitsPlans);
        }

        public virtual ActionResult DeleteDocumentByVisit(int dicId)
        {
            bool deleted = false;
            int year = DateTime.Now.Year;
            var operation = IDBContext.Current.Operation;

            try
            {
                deleted = VisitRepository.DeleteDocumentVisit(dicId);

                return RedirectToAction("Edit", new { operationNumber = operation, year = year, view = "Visit", createNew = false });
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("VisitsController", "Error in DeleteDocumentByVisit method", e);
            }

            return RedirectToAction("Edit", new { operationNumber = operation, year = year, view = "Visit", createNew = false });
        }

        [HasPermission(Permissions = "Visit Plan Write")]
        public virtual ActionResult DeleteVisitComponent(int visitId, string operationNumber, int year)
        {
            bool deleted = false;

            try
            {
                deleted = VisitRepository.DeleteVisit(visitId);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("VisitsController", "Error in DeleteVisitComponent method", e);
            }

            return RedirectToAction("Edit", new { operationNumber = operationNumber, year = year, view = "Visit", createNew = false });
        }

        [HasPermission(Permissions = "Visit Plan Write")]
        public virtual ActionResult EditVisits(int visitPlanId)
        {
            ViewBag.EditMode = true;
            SetLocalizedValues();
            VisitPlanModel planModel = new VisitPlanModel()
            {
                VisitPlanId = visitPlanId,
                Visits = VisitRepository.GetVisitModel(visitPlanId),
            };
            return PartialView(planModel);
        }

        public virtual ActionResult Save1(int versionPlanId, VisitPlanModel planModel, string operationNumber)
        {
            // Set default notification message
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            // Get duplicated visit types count
            var duplicatedVisitTypes = planModel.Visits.GroupBy(vt => vt.TypeVisitId).Where(vt => vt.Count() > 1).ToList();

            // Check if there is more than one visit of the same type
            if (duplicatedVisitTypes.Count() > 0)
            {
                messageStatus = MessageNotificationCodes.DuplicateVisitType;
            }
            else
            {
                VisitRepository.SaveVisit(versionPlanId, planModel);
            }

            return RedirectToAction("VisitsPlanSummary", new
            {
                operationNumber = operationNumber,
                year = planModel.Year,
                messageStatus = messageStatus
            });
        }

        private void SetLocalizedValues()
        {
            var dVisits = MasterDataRepository.GetMasterDataModels("VISIT");
            var temp = dVisits.ToDictionary(d => d.ConvergenceMasterDataId, d => BusinessLogic.MasterDataGetLocaleName(d));
            ViewBag.LocalizedMainVisits = dVisits.ToDictionary(d => d.ConvergenceMasterDataId, d => BusinessLogic.MasterDataGetLocaleName(d));
        }

        private List<VisitPlanModel> PrepareSummaryView(VisitPlanModel plan, string operationNumber, int year)
        {
            var availableYears = new List<int> { DateTime.Today.Year, DateTime.Today.Year + 1 };

            var visitPlans = VisitRepository.GetByOperationAndYear(operationNumber, availableYears);
            if (plan != null)
            {
                visitPlans.Add(plan);
            }

            visitPlans.Sort((s1, s2) => s1.Year.CompareTo(s2.Year));

            if (visitPlans.All(s => s.Year != year))
            {
                year = availableYears.First();
            }

            foreach (var v in visitPlans)
            {
                v.ActualVersionEditable = true;
                v.ActualVersionIsApproved = false;
                v.ActualVersionCanModify = false;
                v.ActualVersionIsInDraftOrModified = true;
                v.ActualVersionIsInModified = true;
            }

            SetLocalizedValues();

            ViewBag.OperationNumber = operationNumber;
            ViewBag.AvailableYears = availableYears;

            ViewBag.SelectedYear = year;
            var ModelOperation = _clientBasicData.OperationGet(operationNumber);
            ViewBag.OperationId = ModelOperation.OperationId;
            ViewBag.VisitPlan = visitPlans;
            return visitPlans;
        }
    }
}
