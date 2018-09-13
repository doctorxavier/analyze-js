using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outcomes;
using IDB.MW.Domain.Contracts.Specifications;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Outcomes;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Models.OutcomeIndicator;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class OutcomeIndicatorController : BaseController
    {
        private IResultsMatrixModelRepository _clientResultsMatrix = null;

        public virtual IResultsMatrixModelRepository ClientResultsMatrix
        {
            get { return _clientResultsMatrix; }
            set { _clientResultsMatrix = value; }
        }

        private IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions.IConvergenceMasterDataModelRepository _ClientConvergenceMasterData = null;
        
        public IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions.IConvergenceMasterDataModelRepository ClientConvergenceMasterData
        {
            get { return _ClientConvergenceMasterData; }
            set { _ClientConvergenceMasterData = value; }
        } 
        
        public virtual ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public virtual ActionResult Delete(CustomDeleteOutcomeIndicatorModel deletedModel)
        {
            int code = 0;
            var resultsMatrix = ClientResultsMatrix.FindOneModel(
                new ResultsMatrixSpecification { ResultsMatrixId = deletedModel.ResultsMatrixId });

            try
            {
                OutcomeIndicatorModel indicatorModel = ClientResultsMatrix.FindOneOutcomeIndicatorModel(
                    new OutcomeIndicatorSpecification { OutcomeIndicatorId = deletedModel.ImpactId });

                if (indicatorModel != null)
                {
                    ClientResultsMatrix.DeleteOutcomeIndicator(
                        new ResultsMatrixModel
                        {
                            ResultsMatrixId = deletedModel.ResultsMatrixId,
                            IsThirdInterval = deletedModel.IsThirdInterval,
                            AccessedByAdministrator = deletedModel.AccessedByAdministrator,
                            Interval = new IntervalModel { IntervalId = deletedModel.IntervalId }
                        },
                        indicatorModel,
                        IDBContext.Current.UserName);

                    code = 502;
                }
            }
            catch (Exception)
            {
                code = 498;
            }

            return RedirectToAction(
                "Edit",
                "Outcomes",
                new
                {
                    operationId = resultsMatrix.OperationId,
                    resultsMatrixId = resultsMatrix.ResultsMatrixId,
                    code = code
                });
        }

        [HttpGet]
        public virtual ActionResult DeleteIndicatorWarning(int resultsMatrixId, int impactIndicatorId, int intervalId)
        {
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            var resultModel = ClientResultsMatrix.GetLightResultsMatrixModel(new ResultsMatrixModel() { ResultsMatrixId = resultsMatrixId, AccessedByAdministrator = accessedByAdministrator });

            CustomDeleteOutcomeIndicatorModel deletedModel = new CustomDeleteOutcomeIndicatorModel()
            {
                AccessedByAdministrator = accessedByAdministrator,
                ImpactId = impactIndicatorId,
                IsThirdInterval = resultModel.IsThirdInterval,
                ResultsMatrixId = resultModel.ResultsMatrixId,
                IntervalId = resultModel.Interval.IntervalId
            };

            ViewData["defaulDeleteIndicatorMessage"] = Localization.GetText("This action cannot be undone, are you sure you wish to continue?");

            if (resultModel.Interval.IntervalId == ResultsMatrixCodes.ThirdInterval || (deletedModel.AccessedByAdministrator && resultModel.IsThirdInterval))
            {
                ViewData["thirdIntervalDeleteIndicatorMessage"] = Localization.GetText("This change will be registered on the 'Changes to the Matrix' section and you will be required to explain it. Would you like to proceed?");
            }
            else
            {
                ViewData["thirdIntervalDeleteIndicatorMessage"] = string.Empty;
            }

            return PartialView(deletedModel);
        }    

        [HttpGet]
        public virtual ActionResult Reassign(int resultsMatrixId, int impactId, int impactIndicatorId)
        {           
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            var lightResultModel = ClientResultsMatrix.GetLightResultsMatrixModel(new ResultsMatrixModel() { ResultsMatrixId = resultsMatrixId, AccessedByAdministrator = accessedByAdministrator });

            OutcomeModel currentImpact = ClientResultsMatrix.FindOneOutcomeModel(new OutcomeSpecification() { OutcomeId = impactId });

            IDB.MW.Domain.Models.Architecture.ResultMatrix.Outcomes.ResultsMatrixModel resultModel = ClientResultsMatrix.FindOneModel(new ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            List<OutcomeModel> outcomes = resultModel.Outcomes;
            
            outcomes.RemoveAll(i => i.OutcomeId == impactId);

            CustomReassignOutcomeIndicatorModel reassignModel = new CustomReassignOutcomeIndicatorModel()
            {
                Statement = currentImpact.Statement,
                Definition = currentImpact.OutcomeIndicators.Where(ii => ii.OutcomeIndicatorId == impactIndicatorId).SingleOrDefault<OutcomeIndicatorModel>().Definition,
                OutcomeIndicatorId = impactIndicatorId,
                ResultsMatrixId = lightResultModel.ResultsMatrixId,
                AccessedByAdministrator = accessedByAdministrator,
                IsThirdInterval = lightResultModel.IsThirdInterval,
                ImpactId = impactId,
                Outcomes = outcomes,
                IntervalId = lightResultModel.Interval.IntervalId
            };

            return PartialView(reassignModel);
        }

        [HttpPost]
        public virtual ActionResult ReassignIndicator(CustomReassignOutcomeIndicatorModel reassignModel)
        {
            int code = 0;

            try
            {
                OutcomeIndicatorModel indicatorModel = ClientResultsMatrix.FindOneOutcomeIndicatorModel(
                    new OutcomeIndicatorSpecification { OutcomeIndicatorId = reassignModel.OutcomeIndicatorId });

                indicatorModel.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                indicatorModel.IsThirdInterval = reassignModel.IsThirdInterval;
                indicatorModel.IntervalId = reassignModel.IntervalId;

                OutcomeModel currentImpactModel = ClientResultsMatrix.FindOneOutcomeModel(
                    new OutcomeSpecification { OutcomeId = indicatorModel.OutcomeId });
                OutcomeModel newImpactModel = ClientResultsMatrix.FindOneOutcomeModel(
                    new OutcomeSpecification { OutcomeId = reassignModel.ImpactId });

                if (indicatorModel != null & currentImpactModel != null && newImpactModel != null)
                {
                    ClientResultsMatrix.ReassignIndicator(
                        currentImpactModel, newImpactModel, indicatorModel, IDBContext.Current.UserName);

                    code = 503;
                }
            }
            catch (Exception)
            {
                code = 497;
            }

            var resultsMatrix = ClientResultsMatrix.FindOneModel(
                new ResultsMatrixSpecification { ResultsMatrixId = reassignModel.ResultsMatrixId });

            return RedirectToAction(
                "Edit",
                "Outcomes",
                new
                {
                    operationId = resultsMatrix.OperationId,
                    resultsMatrixId = resultsMatrix.ResultsMatrixId,
                    code = code
                });
        }

        public virtual ActionResult Detail(int resultsMatrixId, int impactId, int impactIndicatorId)
        {
            OutcomeModel impactModel = ClientResultsMatrix.FindOneOutcomeModel(new OutcomeSpecification() { OutcomeId = impactId });

            var impactIndicatorModel = impactModel.OutcomeIndicators.Where(ii => ii.OutcomeIndicatorId == impactIndicatorId).SingleOrDefault<OutcomeIndicatorModel>();

            var resultModel = ClientResultsMatrix.FindOneModel(new ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            impactModel.OutcomeIndicators.Clear();

            impactModel.OutcomeIndicators.Add(impactIndicatorModel);

            ViewData["operationNumber"] = resultModel.Operation.OperationNumber;
            
            var StateDraftId = ClientConvergenceMasterData.GetMasterDataId("VALIDATION_STAGE", "PMI_DRAFT");
            var isDraft = false;

            if (resultModel.ValidationStageId == StateDraftId)
            {
                isDraft = true;
            }

            ViewBag.isDraft = isDraft;
            ViewBag.isEditable = ClientResultsMatrix.GetLightResultsMatrixModel(new ResultsMatrixModel() { ResultsMatrixId = resultModel.ResultsMatrixId }).isEditable;

            return View(impactModel);
        }

        [HttpGet]
        public virtual ActionResult DeleteYearWarning(int resultsMatrixId, int impactId, int year)
        {
            ViewData["resultsMatrixId"] = resultsMatrixId;
            ViewData["impactId"] = impactId;
            ViewData["year"] = year;
            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult DeleteYear(int resultsMatrixId, int impactId, int year)
        {
            int code = 0;

            var resultsMatrix = ClientResultsMatrix.FindOneModel(new ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            try
            {
                ClientResultsMatrix.DeleteYear(resultsMatrix, new OutcomeIndicatorYearPlanModel() { Year = year });
                code = 505;
            }
            catch (Exception)
            {
                code = 495;
                throw;
            }

            return RedirectToAction("Edit", "Outcomes", new { operationId = resultsMatrix.OperationId, resultsMatrixId = resultsMatrixId, code });
        }

        [HttpGet]
        public virtual ActionResult DeleteDisaggregationWarning(int resultsMatrixId, int impactDisaggregationId)
        {
            ViewData["resultsMatrixId"] = resultsMatrixId;
            ViewData["impactDisaggregationId"] = impactDisaggregationId;
            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult DeleteDisaggregation(int resultsMatrixId, int impactDisaggregationId)
        {
            int code = 0;

            try
            {
                OutcomeDisaggregationModel indicatorModel = ClientResultsMatrix.FindOneOutcomeDisaggregationModel(new OutcomeDisaggregationSpecification() { OutcomeDisaggregationId = impactDisaggregationId });

                if (indicatorModel != null)
                {
                    ClientResultsMatrix.DeleteDissagregation(indicatorModel);
                    code = 504;
                }
                else
                {
                    code = 496;
                }
            }
            catch (Exception)
            {
                code = 496;
            }

            var resultsMatrix = ClientResultsMatrix.FindOneModel(new ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            return RedirectToAction("Edit", "Outcomes", new { operationId = resultsMatrix.OperationId, resultsMatrixId = resultsMatrixId, code = code });
        }

        [ExceptionHandling]
        [HttpGet]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditDetail(int impactIndicatorId)
        {
            OutcomeIndicatorModel indicatorModel = ClientResultsMatrix.FindOneOutcomeIndicatorModel(new OutcomeIndicatorSpecification() { OutcomeIndicatorId = impactIndicatorId });

            OutcomeModel impactModel = ClientResultsMatrix.FindOneOutcomeModel(new OutcomeSpecification() { OutcomeId = indicatorModel.OutcomeId });

            bool isLinked = (indicatorModel.PredefinedIndicator == null) ? false : true;

            var operationNumber = ClientResultsMatrix.FindOneModel(new ResultsMatrixSpecification() { ResultsMatrixId = impactModel.ResultsMatrixId }).Operation.OperationNumber;

            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            var resultsMatrix = ClientResultsMatrix.GetResultsMatrixModel(new OperationModel() { OperationNumber = operationNumber, AccessedByAdministrator = accessedByAdministrator });

            CustomEditOutcomeIndicatorModel editImpactIndicatoModel = new CustomEditOutcomeIndicatorModel
            {
                impact = impactModel,
                impactIndicator = indicatorModel,
                IsLinkedToPredefinedIndicator = isLinked,
                IntervalId = resultsMatrix.Interval.IntervalId,
                AccessedByAdministrator = accessedByAdministrator,
                IsThirdInterval = resultsMatrix.IsThirdInterval,
                OperationNumber = operationNumber,
                IsTcmModule = true
            };

            return View(editImpactIndicatoModel);
        }

        [ExceptionHandling]
        [HttpPost]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult UpdateDetail(CustomEditOutcomeIndicatorModel updatedModel)
        {
            var indicatorModel = ClientResultsMatrix.FindOneOutcomeIndicatorModel(
                new OutcomeIndicatorSpecification
                {
                    OutcomeIndicatorId = updatedModel.impactIndicator.OutcomeIndicatorId
                });
            var impactModel = ClientResultsMatrix.FindOneOutcomeModel(
                new OutcomeSpecification { OutcomeId = indicatorModel.OutcomeId });

            updatedModel.impactIndicator.AccessedByAdministrator = 
                IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            updatedModel.impactIndicator.IsThirdInterval = updatedModel.IsThirdInterval;
            updatedModel.impactIndicator.IntervalId = updatedModel.IntervalId;

            ClientResultsMatrix.UpdateImpactIndicator(
                updatedModel.impactIndicator, IDBContext.Current.UserName);

            return RedirectToAction(
                "Detail",
                "OutcomeIndicator",
                new
                {
                    resultsMatrixId = impactModel.ResultsMatrixId,
                    impactId = impactModel.OutcomeId,
                    impactIndicatorId = updatedModel.impactIndicator.OutcomeIndicatorId
                });
        }

        [HttpGet]
        public virtual ActionResult SaveChanges()
        {
            return PartialView();
        }
    }
}