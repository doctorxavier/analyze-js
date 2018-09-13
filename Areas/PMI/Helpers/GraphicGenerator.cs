using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataExecutionService;

namespace IDB.Presentation.MVC4.Areas.PMI.Helpers
{
    public static class GraphicGenerator
    {
        public static AccumulateDisbursementData LoadAccumalteDisbursementsGraphData(
            List<HistoricAcumulated> historyAccumulated,
            PMIOperation pmiOperation,
            string operationNumber,
            bool prevVersion)
        {
            if (pmiOperation == null || pmiOperation.BenchmarkCurve == null)
            {
                return null;
            }

            List<ListPMICurveModel> pmi;
            List<HistoricAcumulated> history;

            if (!prevVersion)
            {
                pmi = pmiOperation.BenchmarkCurve;
                history = pmiOperation.PersistedAccumulatedDisbursement
                    .Select(x =>
                        new HistoricAcumulated()
                        {
                            ACUMULATED = (float)x.Value,
                            MONTHS = x.Key,
                            YEAR =
                                pmiOperation.ApprovalDate != null
                                    ? ((DateTime)pmiOperation.ApprovalDate).AddMonths(x.Key).Year
                                    : 0
                        }).ToList();

                    return new AccumulateDisbursementData
                    {
                        IDB = pmi
                            .Select(c => new object[]
                            {
                                c.PeriodNumber,
                                c.Idb_Value,
                            })
                            .ToList(),
                        Country = pmi
                            .Select(c => new object[]
                            {
                                c.PeriodNumber,
                                c.AverageValue,
                            })
                            .ToList(),
                        Alert = pmi
                            .Select(c => new object[]
                            {
                                c.PeriodNumber,
                                c.UpperLimit,
                            })
                            .ToList(),
                        Problem = pmi
                            .Select(c => new object[]
                            {
                                c.PeriodNumber,
                                c.ProblemLevel,
                            })
                            .ToList(),
                        Percentage = history.Select(c => new object[]
                        {
                            c.MONTHS,
                            c.ACUMULATED,
                            c.YEAR
                        }).ToList(),
                    };
            }

            bool sw = true;
            pmi = pmiOperation.BenchmarkCurve;
            int yearLimit = pmiOperation.GetYearLimitForIndicators;
            history = historyAccumulated;
            var TotalAccumulatedDisbursements = pmiOperation.AccumulatedDisbursement;

            if (pmiOperation.OperationIsClosed)
            {
                if (yearLimit < DateTime.Now.Year)
                {
                    sw = false;
                }

                history.RemoveAll(item => item.YEAR > yearLimit);
            }

            var result = new AccumulateDisbursementData
            {
                IDB = pmi
                    .Select(c => new object[]
                    {
                        c.PeriodNumber,
                        c.Idb_Value
                    })
                    .ToList(),
                Country = pmi
                    .Select(c => new object[]
                    {
                        c.PeriodNumber,
                        c.AverageValue,
                    })
                    .ToList(),
                Alert = pmi
                    .Select(c => new object[]
                    {
                        c.PeriodNumber,
                        c.UpperLimit
                    })
                    .ToList(),
                Problem = pmi
                    .Select(c => new object[]
                    {
                        c.PeriodNumber,
                        c.ProblemLevel,
                    })
                    .ToList(),
                Percentage = history.Select(c => new object[]
                {
                    c.MONTHS,
                    c.ACUMULATED,
                    c.YEAR
                }).ToList(),
            };

            if (pmiOperation.OperationIsClosed && sw)
            {
                result.Percentage.Add(new object[]
                {
                    pmiOperation.DisbursedMonths,
                    TotalAccumulatedDisbursements,
                    DateTime.Now.Year
                });
            }
            else
            {
                int CountHistory = history.Select(item => item.YEAR).Count();
                if (CountHistory == 0)
                {
                    result.Percentage.Add(new object[]
                    {
                        pmiOperation.DisbursedMonths,
                        TotalAccumulatedDisbursements,
                        yearLimit
                    });
                }
                else
                {
                    int yearMaxOfHistory = history.Select(item => item.YEAR).Max();
                    if (yearMaxOfHistory < yearLimit)
                    {
                        result.Percentage.Add(new object[]
                        {
                            pmiOperation.DisbursedMonths,
                            TotalAccumulatedDisbursements,
                            yearLimit
                        });
                    }
                }
            }

            return result;
        }
    }
}