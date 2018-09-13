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
    public partial class StageThreeController : BaseController
    {
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheMediumTime"].ToString());

        public virtual Common.Logging.ILog Logger { get; set; }

        #region wcf services repositories

        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private ICacheManagement _cacheData = null;

        #endregion

        public StageThreeController(IPMIDetailsModelRepository clientPMIDetails, ICacheManagement cacheData)
        {
            _clientPMIDetails = clientPMIDetails;
            _cacheData = cacheData;
        }

        public void LoadPeriodGraphData(PMIOperation pmiOperation, string operationNumber)
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
                        c.Year, c.AnnualPlanned == -1 ?
                            null : (decimal?)c.AnnualPlanned
                    }),
                AnnualEarned = rawData.Select(c => new object[]
                    {
                        c.Year, c.AnnualEarned == -1 ? 
                            null : (decimal?)c.AnnualEarned
                    }),
                Raw = rawData, 
                Years = rawData.Select(c => c.Year).Distinct().OrderBy(c => c)
            };
            ViewBag.PeriodGraphData = transformed;
        }

        public void LoadAnnualGraphData(string operationNumber, PMIOperation pmiOperation)
        {
            int firstDisbursement = pmiOperation.YearOfDisbursement;

            var rawData = _clientPMIDetails
                .GetIndexPerformance(operationNumber, pmiOperation)
                .OrderBy(c => c.Year);

            if (firstDisbursement >= 2012)
            {
                var transformed = new
                {
                    SPI = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.SchedulePerformance == -1 ?
                                null : (decimal?)c.SchedulePerformance
                        }).ToList(),
                    CPI = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.CostPerformance == -1 ?
                                null : (decimal?)c.CostPerformance
                        }).ToList(),
                    SPIa = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.AnnualSchedulePerformance == -1 ?
                                null : (decimal?)c.AnnualSchedulePerformance
                        }).ToList(),
                    CPIa = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.AnnualCostPerformance == -1 ?
                                null : (decimal?)c.AnnualCostPerformance
                        }).ToList(),
                    Raw = rawData,
                    Years = rawData.Select(c => c.Year).Distinct().OrderBy(c => c),
                    firstDisbursement = 1
                };
                ViewBag.AnnualGraphData = transformed;
            }
            else
            {
                var transformed = new
                {
                    SPIa = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.AnnualSchedulePerformance == -1 ? 
                                null : (decimal?)c.AnnualSchedulePerformance
                        }).ToList(),
                    CPIa = rawData.Select(c => 
                        new object[]
                        {
                            c.Year, c.AnnualCostPerformance == -1 ? 
                                null : (decimal?)c.AnnualCostPerformance
                        }).ToList(),
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
                ListOutputs = milestoneOrOutput.Select(x => new { x.Item3, x.Item2 }).ToList()
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
                ListOutputsAnnual = MilestoneOrOutputAnnual
                .Select(x => new { x.Item3, x.Item2 })
                .ToList()
            };
            ViewBag.PercentageProgressAnnual = transformed;
        }

        public void LoadDISB95(PMIOperation pmiOperation, string operationNumber, bool isLive)
        {
            var pmi = _clientPMIDetails.GetCurrentPMIEscalarBenchmark(operationNumber, pmiOperation, isLive);
            var transformed = new
            {
                MaxLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterDISB95)
                .Select(x => x.MaxLevel)
                .FirstOrDefault(),
                AlertLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterDISB95)
                .Select(x => x.AlertLevel)
                .FirstOrDefault(),
                ProblemLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterDISB95)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault(),
                Disbursement95 = pmi.Disbur95,
            };
            ViewBag.Disb95 = transformed;
        }

        public virtual ActionResult Index(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            //check if have data in Output and Component
            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);

            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            IDBContext.Current.ContextPMIOperation = pmiOperation;
            bool HaveData = pmiOperation.HaveDataOutputsAndComponent;

            if (!HaveData)
            {
                return ShowWarning(operationNumber, 1);
            }

            bool HaveDataOYP = pmiOperation.HaveDataOutputYearPlan;

            if (!HaveDataOYP)
            {
                return ShowWarning(operationNumber, 2);
            }
            else
            {
                ViewBag.Expected = pmiOperation.ExpectedExecutionDuration;
                ViewBag.Enviromental = pmiOperation.Enviromental;

                ViewBag.OperationNUmber = operationNumber;
                ViewData["OperationNumber"] = operationNumber;

                ViewBag.Tab = "S3G";
            }

            LoadAnnualGraphData(operationNumber, pmiOperation);
            LoadPeriodGraphData(pmiOperation, operationNumber);
            var historicalAccumulated = _clientPMIDetails.GetHistoricalAccumulated(operationNumber);
            ViewBag.AccumalteDisbursementsGraphData = GraphicGenerator
                .LoadAccumalteDisbursementsGraphData(
                    historicalAccumulated, pmiOperation, operationNumber, calculateInMemory);
            LoadPercentageProgress(pmiOperation, operationNumber);
            LoadPercentageProgressAnnual(pmiOperation, operationNumber);
            LoadDISB95(pmiOperation, operationNumber, isLive);

            //TODO: The entire model should be sent to the view instead of using the viewbag
            return View(isLive);
        }

        public virtual ActionResult WarningMessage(string operationNumber, int MessageNumber)
        {
            ViewBag.OperationNumber = operationNumber;
            ViewBag.NumberMessage = MessageNumber;
            return PartialView();
        }

        public virtual ActionResult IndexSummaryTable(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            var pmi = _clientPMIDetails
                .GetCurrentPMIEscalarBenchmark(operationNumber, pmiOperation, isLive);
            pmi.IsLiveMode = isLive;

            ViewBag.OperationNUmber = operationNumber;
            ViewBag.Tab = "S3T";

            return View(pmi);
        }

        [OutputCache(Duration = 60, VaryByParam = "*")]
        public virtual ActionResult RenderSummaryTable(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(cacheName,
                _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                _timeCachingVal);

            bool OperationIsClosed = pmiOperation.OperationIsClosed;
            bool sw = false;
            var AccDis = 0f;
            int AccMonth = 0;
            int yearLimit = pmiOperation.GetYearLimitForIndicators;
            var history = _clientPMIDetails.GetHistoricalAccumulated(operationNumber);

            if (OperationIsClosed) 
            {
                //IF THE OPERATION IS CLOSED, THE ACCUMULATED IS OF CLOSING YEAR
                if (yearLimit < DateTime.Now.Year)
                {
                    sw = true;
                }
                else
                {
                    sw = false;
                }
            }

            if (OperationIsClosed && sw) 
            {
                //IF THE OPERATION IS CLOSED, THE CLOSING YEAR IS "YEAR LIMIT"
                AccDis = history.Where(hx => hx.YEAR == yearLimit).Select(hx => hx.ACUMULATED).FirstOrDefault();
                AccMonth = history.Where(hx => hx.YEAR == yearLimit).Select(hx => hx.MONTHS).FirstOrDefault();
            }
            else
            {
                sw = false;
            }

            bool HaveData = pmiOperation.HaveDataOutputsAndComponent;

            if (!HaveData)
            {
                return View("_WarningMessage", 1);
            }

            bool HaveDataOYP = pmiOperation.HaveDataOutputYearPlan;

            //if not have data in OutputYearPlan
            if (!HaveDataOYP)
            {
                return View("_WarningMessage", 2);
            }

            var MIValues = _clientPMIDetails.GetCurrentPMIDetailsModel(operationNumber, pmiOperation, isLive);
            MIValues.IsLiveMode = isLive;

            var PmiEscalar = _clientPMIDetails.GetCurrentPMIEscalarBenchmark(operationNumber, pmiOperation, isLive);

            decimal SchedulePerformanceAnnualValue = pmiOperation.LastSchedulePerformanceAnnual;
            decimal SchedulePerformanceValue = pmiOperation.LastSchedulePerformance;
            decimal CostPerformanceValue = pmiOperation.LastCostPerformance;
            decimal CostPerformanceAnnualValue = pmiOperation.LastCostPerformanceAnnual;
            int disbursedMonths = pmiOperation.DisbursedMonths;

            ////////THE ACCUMULATED DISBURSEMENTS IS OF CLOSING YEAR
            if (sw == true)
            {
                MIValues.AfterEligibility.Accumulated_disbursements = (decimal)AccDis;
            }

            var intervals = MIValues.AfterEligibility.IntervalsList;

            ViewBag.UpperLimit = intervals
                .Where(x => x.PeriodNumber == disbursedMonths)
                .Select(x => x.UpperLimit)
                .FirstOrDefault();

            ViewBag.Problem = intervals
                .Where(x => x.PeriodNumber == disbursedMonths)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault();

            int firstDisbursement = pmiOperation.YearOfDisbursement;

            if (firstDisbursement >= 2012)
            {
                ViewBag.Schedule_Performance_Calcule = SchedulePerformanceValue == -1 ? " " :
                    Convert.ToString(SchedulePerformanceValue);
                ViewBag.Schedule_Performance = SchedulePerformanceValue == -1 ? " " :
                    Convert.ToString(decimal.Round(SchedulePerformanceValue, 2));

                ViewBag.Cost_Performance_Calcule = CostPerformanceValue == -1 ? " " :
                    Convert.ToString(CostPerformanceValue);
                ViewBag.Cost_Performance = CostPerformanceValue == -1 ? " " :
                    Convert.ToString(decimal.Round(CostPerformanceValue, 2));
            }
            else
            {
                ViewBag.Schedule_Performance = " ";

                ViewBag.Cost_Performance = " ";
            }

            ViewBag.Schedule_Performance_Annual_Calcule = SchedulePerformanceAnnualValue == -1 ? " " :
                Convert.ToString(SchedulePerformanceAnnualValue);
            ViewBag.Schedule_Performance_Annual = SchedulePerformanceAnnualValue == -1 ? " " :
                Convert.ToString(decimal.Round(SchedulePerformanceAnnualValue, 2));
            ViewBag.Cost_Performance_Annual_Calcule = CostPerformanceAnnualValue == -1 ? " " :
                Convert.ToString(CostPerformanceAnnualValue);
            ViewBag.Cost_Performance_Annual = CostPerformanceAnnualValue == -1 ? " " :
                Convert.ToString(decimal.Round(CostPerformanceAnnualValue, 2));

            MIValues.Maxlevel = PmiEscalar.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == PmiEscalar.MasterDISB95)
                .Select(x => x.MaxLevel)
                .FirstOrDefault();
            MIValues.Alert = PmiEscalar.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == PmiEscalar.MasterDISB95)
                .Select(x => x.AlertLevel)
                .FirstOrDefault();
            MIValues.Problem = Convert.ToDecimal(PmiEscalar.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == PmiEscalar.MasterDISB95)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault());
            MIValues.Disbur95 = PmiEscalar.Disbur95;

            ViewBag.ExpectedDuration = pmiOperation.ExpectedExecutionDuration != null ?
                Convert.ToDecimal(string.Format("{0:0.##}", pmiOperation.ExpectedExecutionDuration.Value)) :
                (decimal?)null;

            ViewBag.Disbursementshort = Convert.ToString(decimal.Round(MIValues.AfterEligibility.Accumulated_disbursements, 2));

            MIValues.Averageweighted = pmiOperation.WeightedAverage;

            MIValues.AverageweightedAnnual = pmiOperation.WeightedAverageAnnual;

            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);

            return View(MIValues);
        }

        private ActionResult ShowWarning(string operationNumber, int messageNumber)
        {
            ViewBag.OperationNumber = operationNumber;
            ViewBag.NumberMessage = messageNumber;
            return View("WarningMessage");
        }
    }
}
