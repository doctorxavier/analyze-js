using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Cache;
using IDB.MW.Domain.EntityHelpers;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;

namespace IDB.Presentation.MVC4.Areas.PMI.Controllers
{
    public partial class StageTwoController : BaseController
    {
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheMediumTime"].ToString());

        public virtual Common.Logging.ILog Logger { get; set; }

        #region wcf services repositories

        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private ICacheManagement _cacheData = null;

        #endregion
        public StageTwoController(IPMIDetailsModelRepository clientPMIDetails, ICacheManagement cacheData)
        {
            _clientPMIDetails = clientPMIDetails;
            _cacheData = cacheData;
        }

        public void LoadAnnualGraphData(string operationNumber, PMIOperation pmiOperation)
        {
            int firstDisbursement = pmiOperation.YearOfDisbursement;
            var rawData = _clientPMIDetails.GetIndexPerformance(operationNumber, pmiOperation).OrderBy(c => c.Year);
            if (firstDisbursement >= 2012)
            {
                var transformed =
                    new
                    {
                        SPI =
                            rawData.Select(
                                c =>
                                    new object[] 
                                    {
                                        c.Year, c.SchedulePerformance == -1 ? null : (decimal?)c.SchedulePerformance
                                    })
                                .ToList(),
                        CPI =
                            rawData.Select(
                                c =>
                                    new object[] 
                                    {
                                        c.Year, c.CostPerformance == -1 ? null : (decimal?)c.CostPerformance
                                    })
                                .ToList(),
                        SPIa =
                            rawData.Select(
                                c =>
                                    new object[]
                                    {
                                        c.Year,
                                        c.AnnualSchedulePerformance == -1
                                            ? null
                                            : (decimal?)c.AnnualSchedulePerformance
                                    }).ToList(),
                        CPIa =
                            rawData.Select(
                                c =>
                                    new object[] 
                                    {
                                        c.Year, c.AnnualCostPerformance == -1 ? null : (decimal?)c.AnnualCostPerformance
                                    })
                                .ToList(),
                        Raw = rawData,
                        Years = rawData.Select(c => c.Year).Distinct().OrderBy(c => c),
                        firstDisbursement = 1
                    };
                ViewBag.AnnualGraphData = transformed;
            }
            else
            {
                var transformed =
                    new
                    {
                        SPIa =
                            rawData.Select(
                                c =>
                                    new object[]
                                    {
                                        c.Year,
                                        c.AnnualSchedulePerformance == -1
                                            ? null
                                            : (decimal?)c.AnnualSchedulePerformance
                                    }).ToList(),
                        CPIa =
                            rawData.Select(
                                c =>
                                    new object[] 
                                    {
                                        c.Year, c.AnnualCostPerformance == -1 ? null : (decimal?)c.AnnualCostPerformance
                                    })
                                .ToList(),
                        Raw = rawData,
                        Years = rawData.Select(c => c.Year).Distinct().OrderBy(c => c),
                        firstDisbursement = 2
                    };
                ViewBag.AnnualGraphData = transformed;
            }
        }

        public void LoadPercentageProgress(PMIOperation pmiOperation, string operationNumber)
        {
            var milestoneOrOutput = pmiOperation.MilestoneOrOutput;

            var transformed = new
            {
                CostSpent = pmiOperation.PercentageCostSpent,
                TimeElapsed = pmiOperation.PercentageOfTimeElapsed,
                Average = pmiOperation.AverageOutput,
                Weighted = pmiOperation.WeightedAverage,
                ListOutputs = milestoneOrOutput.Select(
                    x => new
                    {
                        x.Item3, x.Item2
                    }).ToList()
            };
            ViewBag.PercentageProgress = transformed;
        }

        public void LoadPercentageProgressAnnual(PMIOperation pmiOperation, string operationNumber)
        {
            var MilestoneOrOutputAnnual = pmiOperation.MilestoneOrOutputAnnual;

            var transformed = new
            {
                CostSpentAnnual = pmiOperation.PercentageCostSpentAnnual,
                TimeElapsedAnnual = pmiOperation.PercentageOfTimeElapsedAnnual,
                AverageAnnual = pmiOperation.AverageOutputAnnual,
                WeightedAnnual = pmiOperation.WeightedAverageAnnual,
                ListOutputsAnnual = MilestoneOrOutputAnnual.Select(
                    x => new
                    {
                        x.Item3, x.Item2
                    }).ToList()
            };
            ViewBag.PercentageProgressAnnual = transformed;
        }

        public virtual ActionResult Index(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);

            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            bool haveData = pmiOperation.HaveDataOutputsAndComponent;

            if (!haveData)
            {
                return ShowWarning(operationNumber, 1);
            }

            bool haveDataOyp = pmiOperation.HaveDataOutputYearPlan;

            //if not have data in OutputYearPlan
            if (!haveDataOyp)
            {
                return ShowWarning(operationNumber, 2);
            }
            else
            {
                ViewBag.Expected = pmiOperation.ExpectedExecutionDuration;
                ViewBag.Enviromental = pmiOperation.Enviromental;
                ViewBag.OperationNUmber = operationNumber;
                ViewBag.Tab = "S2G";
            }

            LoadAnnualGraphData(operationNumber, pmiOperation);
            LoadPeriodGraphData(pmiOperation, operationNumber);
            var historicalAccumulated = _clientPMIDetails.GetHistoricalAccumulated(operationNumber);
            ViewBag.AccumalteDisbursementsGraphData = GraphicGenerator
                .LoadAccumalteDisbursementsGraphData(
                    historicalAccumulated, pmiOperation, operationNumber, calculateInMemory);
            LoadPercentageProgress(pmiOperation, operationNumber);
            LoadPercentageProgressAnnual(pmiOperation, operationNumber);

            //TODO: The entire model should be sent to the view instead of using the viewbag
            return View(isLive);
        }

        public virtual ActionResult WarningMessage(string operationNumber, int MessageNumber)
        {
            ViewBag.OperationNumber = operationNumber;
            ViewBag.NumberMessage = MessageNumber;
            return PartialView();
        }

        public virtual ActionResult IndexSummaryTable(string operationNumber, bool isLive = false)
        {
            ViewBag.OperationNUmber = operationNumber;
            ViewBag.Tab = "S2T";

            return View(isLive);
        }

        [OutputCache(Duration = 60, VaryByParam = "*")]
        public virtual ActionResult RenderSummaryTable(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            bool hasData = pmiOperation.HaveDataOutputsAndComponent;

            if (!hasData)
            {
                return View("_WarningMessage", 1);
            }

            bool hasDataOutputYearPlan = pmiOperation.HaveDataOutputYearPlan;

            if (!hasDataOutputYearPlan)
            {
                return View("_WarningMessage", 2);
            }

            float yearLimitAccumulated = 0f;
            int yearLimit = pmiOperation.GetYearLimitForIndicators;
            int monthCalculated = pmiOperation.DisbursedMonths;

            var pmiValues = _clientPMIDetails
                .GetCurrentPMIDetailsModel(operationNumber, pmiOperation, isLive);
            pmiValues.IsLiveMode = isLive;

            if (pmiOperation.OperationIsClosed && (yearLimit < DateTime.Now.Year))
            {
                //If the operation is closed, the accumulated refers to the closing year
                List<HistoricAcumulated> history = _clientPMIDetails
                    .GetHistoricalAccumulated(operationNumber);

                yearLimitAccumulated = history
                    .Where(hx => hx.YEAR == yearLimit)
                    .Select(hx => hx.ACUMULATED)
                    .FirstOrDefault();

                pmiValues.AfterEligibility.Accumulated_disbursements =
                    (decimal)yearLimitAccumulated;
            }

            //The accumulated disbursements are from the closing year
            List<ListPMICurveModel> curveModelValues = pmiValues.AfterEligibility.IntervalsList;

            ViewBag.UpperLimit = curveModelValues
                .Where(x => x.PeriodNumber == monthCalculated)
                .Select(x => x.UpperLimit)
                .FirstOrDefault();

            ViewBag.Problem = curveModelValues
                .Where(x => x.PeriodNumber == monthCalculated)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault();

            int firstDisbursement = pmiOperation.YearOfDisbursement;

            ViewBag.Schedule_Performance = -1;
            ViewBag.Cost_Performance = -1;
            const int YEAR_CPI_SPI_APPLIABLE = 2012;
            if (firstDisbursement >= YEAR_CPI_SPI_APPLIABLE)
            {
                ViewBag.Schedule_Performance_ExactValue = pmiOperation.LastSchedulePerformance;
                ViewBag.Schedule_Performance_Truncated =
                    decimal.Round(pmiOperation.LastSchedulePerformance, 2);

                ViewBag.Cost_Performance_ExactValue = pmiOperation.LastCostPerformance;
                ViewBag.Cost_Performance_Truncated =
                    decimal.Round(pmiOperation.LastCostPerformance, 2);
            }

            ViewBag.Schedule_Performance_Annual_ExactValue =
                pmiOperation.LastSchedulePerformanceAnnual;
            ViewBag.Schedule_Performance_Annual_Truncated =
                decimal.Round(pmiOperation.LastSchedulePerformanceAnnual, 2);

            ViewBag.Cost_Performance_Annual_ExactValue = pmiOperation.LastCostPerformanceAnnual;
            ViewBag.Cost_Performance_Annual_Truncated =
                decimal.Round(pmiOperation.LastCostPerformanceAnnual, 2);

            pmiValues.Averageweighted = decimal.Round(pmiOperation.WeightedAverage, 2);
            pmiValues.AverageweightedAnnual = decimal.Round(pmiOperation.WeightedAverageAnnual, 2);

            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);

            ViewBag.ExpectedDuration = Convert.ToDecimal(
                string.Format("{0:0.##}", pmiOperation.ExpectedExecutionDuration));

            ViewBag.Disbursementshort = Convert.ToString(
                decimal.Round(pmiValues.AfterEligibility.Accumulated_disbursements, 2));

            return View(pmiValues);
        }

        private ActionResult ShowWarning(string operationNumber, int messageNumber)
        {
            ViewBag.OperationNumber = operationNumber;
            ViewBag.NumberMessage = messageNumber;
            return View("WarningMessage");
        }

        private void LoadPeriodGraphData(PMIOperation pmiOperation, string operationNumber)
        {
            //render second graphic
            var rawData = _clientPMIDetails
                .GetValuePerformance(0, operationNumber, pmiOperation)
                .OrderBy(c => c.Year);

            var transformed = new
            {
                Actual = rawData.Select(c => new object[]
                    {
                        c.Year, c.Actual == -1 ? null : (decimal?)c.Actual
                    }),
                Planned = rawData.Select(c => new object[]
                    {
                        c.Year, c.Planned == -1 ? null : (decimal?)c.Planned
                    }),
                Earned = rawData.Select(c => new object[]
                    {
                        c.Year, c.Earned == -1 ? null : (decimal?)c.Earned
                    }),
                AnnualPlanned = rawData.Select(c => new object[]
                    {
                        c.Year, c.AnnualPlanned == -1 ? null : (decimal?)c.AnnualPlanned
                    }),
                AnnualEarned = rawData.Select(c => new object[]
                    {
                        c.Year, c.AnnualEarned == -1 ? null : (decimal?)c.AnnualEarned
                    }),
                Raw = rawData,
                Years = rawData.Select(c => c.Year).Distinct().OrderBy(c => c)
            };
            ViewBag.PeriodGraphData = transformed;
        }
    }
}
