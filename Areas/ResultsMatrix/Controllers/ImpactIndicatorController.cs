using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.Architecture.Language;
using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Impacts;
using IDB.MW.Domain.Contracts.Specifications;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Impacts;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Models.ImpactIndicator;
using IDB.Presentation.MVCExtensions;
using MasterDefinition = IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class ImpactIndicatorController : BaseController
    {
        private IResultsMatrixModelRepository _clientResultsMatrix = null;
        private ICacheManagement _cacheData = null;
        private string _impactsIndicatorCacheName = string.Empty;
        private MasterDefinition.IConvergenceMasterDataModelRepository _ClientConvergenceMasterData = null;

        public ImpactIndicatorController(ICacheManagement cacheData)
        {
            _cacheData = cacheData;
            _impactsIndicatorCacheName = string.Format(
                CacheNames.IMPACTS, IDBContext.Current.Operation);
        }

        public IResultsMatrixModelRepository ClientResultsMatrix
        {
            get { return _clientResultsMatrix; }
            set { _clientResultsMatrix = value; }
        }

        public MasterDefinition.IConvergenceMasterDataModelRepository ClientConvergenceMasterData
        {
            get { return _ClientConvergenceMasterData; }
            set { _ClientConvergenceMasterData = value; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual ActionResult Delete(
            int resultsMatrixId,
            int impactIndicatorId,
            int intervalId,
            bool accessedByAdmin,
            bool isThirInterval)
        {
            int code = 0;
            var resultsMatrix = ClientResultsMatrix.FindOneModel(
                new ResultsMatrixSpecification { ResultsMatrixId = resultsMatrixId });

            try
            {
                ImpactIndicatorModel indicatorModel = ClientResultsMatrix.FindOneImpactIndicatorModel(
                    new ImpactIndicatorSpecification() { ImpactIndicatorId = impactIndicatorId });

                if (indicatorModel != null)
                {
                    IntervalModel interval = new IntervalModel() { IntervalId = intervalId };
                    ClientResultsMatrix.DeleteImpactIndicator(
                        new ResultsMatrixModel()
                        {
                            ResultsMatrixId = resultsMatrixId,
                            IsThirdInterval = isThirInterval,
                            AccessedByAdministrator = accessedByAdmin,
                            Interval = interval
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
                "Impacts",
                new
                {
                    operationId = resultsMatrix.OperationId,
                    resultsMatrixId = resultsMatrixId,
                    code = code
                });
        }

        [HttpGet]
        public virtual ActionResult DeleteIndicatorWarning(
            string order, 
            string definition, 
            int resultsMatrixId, 
            int impactIndicatorId, 
            int intervalId, 
            bool accessedByAdmin,
            bool isThirInterval)              
        {
            ViewData["intervalId"] = intervalId;
            ViewData["resultsMatrixId"] = resultsMatrixId;
            ViewData["impactId"] = impactIndicatorId;

            ViewData["accessedByAdmin"] = accessedByAdmin;
            ViewData["isThirInterval"] = isThirInterval;
            ViewData["isValidated"] = ClientResultsMatrix.IsValidate(impactIndicatorId);

            ViewData["IndicatorName"] = string.Format("{0} {1}", order, definition);
          
            ViewData["defaulDeleteIndicatorMessage"] = Localization.GetText("TCM.DO.DeleteOutcomeStatement.UndoneActionMessage");

            if (intervalId == ResultsMatrixCodes.ThirdInterval || (accessedByAdmin && isThirInterval))
            {
                ViewData["thirdIntervalDeleteIndicatorMessage"] = Localization.GetText("TCM.RCMW.RegisterChangesMany.TextMessage");
            }
            else
            {
                ViewData["thirdIntervalDeleteIndicatorMessage"] = string.Empty;
            }

            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult Reassign(int resultsMatrixId, int impactId, int impactIndicatorId, int intervalId, bool accessedByAdmin, bool isThirInterval)
        {
            ResultsMatrixModel resultModel = ClientResultsMatrix.FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            // Get Interval 
            var interval = ClientResultsMatrix.GetResultsMatrixModel(new OperationModel() { OperationNumber = resultModel.Operation.OperationNumber }).Interval;

            // Get all impacts associated to the current ResultsMatrix
            List<ImpactModel> impacts = resultModel.Impacts;

            // Set statement data
            ImpactModel currentImpact = impacts.Where(i => i.ImpactId == impactId).SingleOrDefault<ImpactModel>();

            // Remove current impact
            impacts.RemoveAll(i => i.ImpactId == impactId);

            ViewData["Statement"] = currentImpact.Statement;

            ViewData["Definition"] = currentImpact.ImpactIndicators.Where(ii => ii.ImpactIndicatorId == impactIndicatorId).SingleOrDefault<ImpactIndicatorModel>().Definition;

            ViewData["impactIndicatorId"] = impactIndicatorId;

            ViewData["resultsMatrixId"] = resultsMatrixId;

            ViewData["thirdIntervalReassignIndicatorMessage"] = Localization.GetText("TCM.RCMW.RegisterChangesMany.TextMessage");

            ViewData["accessedByAdmin"] = accessedByAdmin;

            ViewData["isThirInterval"] = isThirInterval;

            ViewData["intervalCategory"] = intervalId;

            resultModel.Impacts = null;

            // Set the imapcts to set in listbox
            resultModel.Impacts = impacts;

            return PartialView(impacts);
        }

        [HttpGet]
        public virtual ActionResult ReassignIndicator(
            int resultsMatrixId,
            int impactId,
            int impactIndicatorId,
            int intervalId,
            bool accessedByAdmin,
            bool isThirInterval)
        {
            int code = 0;

            try
            {
                ImpactIndicatorModel indicatorModel = ClientResultsMatrix.FindOneImpactIndicatorModel(
                    new ImpactIndicatorSpecification() { ImpactIndicatorId = impactIndicatorId });

                ImpactModel currentImpactModel = ClientResultsMatrix.FindOneImpactModel(
                    new ImpactSpecification() { ImpactId = indicatorModel.ImpactId });

                ImpactModel newImpactModel = ClientResultsMatrix.FindOneImpactModel(
                    new ImpactSpecification() { ImpactId = impactId });

                if (indicatorModel != null && currentImpactModel != null && newImpactModel != null)
                {
                    IntervalModel interval = new IntervalModel() { IntervalId = intervalId };
                    ClientResultsMatrix.ReassignIndicator(
                        new ResultsMatrixModel()
                        {
                            ResultsMatrixId = resultsMatrixId,
                            AccessedByAdministrator = accessedByAdmin,
                            IsThirdInterval = isThirInterval,
                            Interval = interval
                        }, 
                        currentImpactModel, 
                        newImpactModel, 
                        indicatorModel,
                        IDBContext.Current.UserName);
                    code = 503;
                }
            }
            catch (Exception)
            {
                code = 497;
            }

            var resultsMatrix = ClientResultsMatrix.FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });
            _cacheData.Remove(_impactsIndicatorCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);

            return RedirectToAction("Edit", "Impacts", new { operationId = resultsMatrix.OperationId, resultsMatrixId = resultsMatrixId, code = code });
        }

        public virtual ActionResult Detail(int resultsMatrixId, int impactId, int impactIndicatorId)
        {
            var impactModel = ClientResultsMatrix.FindOneImpactModel(new ImpactSpecification() { ImpactId = impactId });

            var impactIndicatorModel = impactModel.ImpactIndicators.Where(ii => ii.ImpactIndicatorId == impactIndicatorId).SingleOrDefault<ImpactIndicatorModel>();

            impactModel.ImpactIndicators.Clear();

            impactModel.ImpactIndicators.Add(impactIndicatorModel);

            var ResultMatrixModel = ClientResultsMatrix.FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification() { ResultsMatrixId = impactModel.ResultsMatrixId });

            ViewData["operationNumber"] = ResultMatrixModel.Operation.OperationNumber;

            var StateDraftId = ClientConvergenceMasterData.GetMasterDataId("VALIDATION_STAGE", "PMI_DRAFT");
            var isDraft = false;

            if (ResultMatrixModel.ValidationStageId == StateDraftId)
            {
                isDraft = true;
            }

            ViewBag.isDraft = isDraft;
            ViewBag.isEditable = ClientResultsMatrix.GetLightResultsMatrixModel(new ResultsMatrixModel() { ResultsMatrixId = ResultMatrixModel.ResultsMatrixId }).isEditable;

            return View(impactModel);
        }

        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult Edit(int impactId)
        {
            var impactModel = ClientResultsMatrix.FindOneImpactModel(new ImpactSpecification() { ImpactId = impactId });

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

            var resultsMatrix = ClientResultsMatrix
                .FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification()
                {
                    ResultsMatrixId = resultsMatrixId
                });

            try
            {
                ClientResultsMatrix.DeleteYear(
                    resultsMatrix,
                    new ImpactIndicatorYearPlanModel() { Year = year });

                code = 505;
            }
            catch (Exception)
            {
                code = 495;
                throw;
            }

            bool isAjaxRequest = IDBContext.Current.ContextRequestContext
                .HttpContext.Request.IsAjaxRequest();

            if (isAjaxRequest)
            {
                return Json(new
                {
                    IsValid = true,
                    Redirect = Url.Action("Edit", 
                    "Impacts", 
                    new
                    {
                        operationId = resultsMatrix.OperationId,
                        resultsMatrixId = resultsMatrixId,
                        code
                    })
                }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Edit", 
                "Impacts", 
                new
            {
                operationId = resultsMatrix.OperationId,
                resultsMatrixId = resultsMatrixId,
                code
            });
        }

        [HttpGet]
        public virtual ActionResult DeleteDisaggregation(int resultsMatrixId, int impactDisaggregationId)
        {
            // Define Ajax response
            int code = 0;

            try
            {
                // Retrieve ImpactIndicator
                ImpactDisaggregationModel indicatorModel = ClientResultsMatrix.FindOneImpactDisaggregationModel(new ImpactDisaggregationSpecification() { ImpactDisaggregationId = impactDisaggregationId });

                if (indicatorModel != null /*&& elementTypeModel != null && matrixChangeTypeModel != null*/)
                {
                    bool deleted = ClientResultsMatrix.DeleteDisaggregation(indicatorModel);

                    ImpactDisaggregationModel deletedModel = ClientResultsMatrix.FindOneImpactDisaggregationModel(new ImpactDisaggregationSpecification() { ImpactDisaggregationId = impactDisaggregationId });

                    if (deleted == true && deletedModel == null)
                    {
                        code = 504;
                    }
                    else
                    {
                        code = 496;
                    }
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

            var resultsMatrix = ClientResultsMatrix.FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification() { ResultsMatrixId = resultsMatrixId });

            return RedirectToAction("Edit", "Impacts", new { operationId = resultsMatrix.OperationId, resultsMatrixId = resultsMatrixId, code = code });
        }

        [ExceptionHandling]
        [HttpGet]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditDetail(int impactIndicatorId)
        {
            ImpactIndicatorModel indicatorModel = ClientResultsMatrix.FindOneImpactIndicatorModel(new ImpactIndicatorSpecification() { ImpactIndicatorId = impactIndicatorId });

            ImpactModel impactModel = ClientResultsMatrix.FindOneImpactModel(new ImpactSpecification() { ImpactId = indicatorModel.ImpactId });

            bool isLinked = (indicatorModel.PredefinedIndicator == null) ? false : true;

            // Start third interval validation
            var operationNumber = ClientResultsMatrix.FindOneModel(new IDB.MW.Domain.Contracts.Specifications.ResultsMatrixSpecification() { ResultsMatrixId = impactModel.ResultsMatrixId }).Operation.OperationNumber;

            // Check if the administrator is accessing
            var accessedByAdministrator = true;

            var resultsMatrix = ClientResultsMatrix.GetResultsMatrixModel(new OperationModel() { OperationNumber = operationNumber, AccessedByAdministrator = accessedByAdministrator });

            // End third interval validation
            CustomEditImpactIndicatoModel editImpactIndicatoModel = new CustomEditImpactIndicatoModel
            {
                impact = impactModel,
                impactIndicator = indicatorModel,
                IsLinkedToPredefinedIndicator = isLinked,
                IntervalId = resultsMatrix.Interval.IntervalId,
                AccessedByAdministrator = accessedByAdministrator,
                IsThirdInterval = resultsMatrix.IsThirdInterval,
                OperationNumber = operationNumber
            };

            return View(editImpactIndicatoModel);
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult UpdateDetail(CustomEditImpactIndicatoModel updatedModel)
        {
            var indicatorModel = ClientResultsMatrix.FindOneImpactIndicatorModel(
                new ImpactIndicatorSpecification()
                {
                    ImpactIndicatorId = updatedModel.impactIndicator.ImpactIndicatorId
                });
            var impactModel = ClientResultsMatrix.FindOneImpactModel(
                new ImpactSpecification()
                {
                    ImpactId = indicatorModel.ImpactId
                });

            updatedModel.impactIndicator.IntervalId = updatedModel.IntervalId;
            updatedModel.impactIndicator.AccessedByAdministrator = updatedModel.AccessedByAdministrator;
            updatedModel.impactIndicator.IsThirdInterval = updatedModel.IsThirdInterval;
            ClientResultsMatrix.UpdateImpactIndicator(
                updatedModel.impactIndicator, IDBContext.Current.UserName);
            _cacheData.Remove(
                _impactsIndicatorCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

            return RedirectToAction(
                "Detail",
                "ImpactIndicator",
                new
                {
                    resultsMatrixId = impactModel.ResultsMatrixId,
                    impactId = impactModel.ImpactId,
                    impactIndicatorId = updatedModel.impactIndicator.ImpactIndicatorId
                });
        }

        [HttpGet]
        public virtual ActionResult SaveChanges()
        {
            return PartialView();
        }
    }
}