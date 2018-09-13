using System;
using System.Collections.Generic;
using System.Linq;

using IDB.Architecture;
using IDB.Architecture.BusinessRules;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outputs;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Supervision.PMI;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Business
{
    public class ResultsMatrixBusiness : BusinessContext
    {
        public ResultsMatrixModel Current;
        public IResultsMatrixModelRepository OutputsRepository { get; set; }
        public OperationBusiness OperationContext;

        public ResultsMatrixBusiness()
            : base()
        {
            OutputsRepository = Globals.Resolve<IResultsMatrixModelRepository>();
        }

        public void Load(OperationBusiness operation)
        {
            OperationContext = operation;
        }

        protected override void OnLoad()
        {
        }
    }

    public class ResultsMatrixRules : IRulesContainer
    {
        [BusinessRule(
            ruleKey: "ResultsMatrix.Outputs.Load",
            description: "Load all outputs information for current operation",
            Type = RuleType.ProductionRule)]
        public void rmoulo(RuleContext rule)
        {
            var context = rule.TContext<ResultsMatrixBusiness>();
            context.Current = context
                .OutputsRepository
                .ResultsMatrixGet(context.OperationContext.OperationNumber);
            
            var outputs = context.Current.Components
                .SelectMany(rm => rm.Outputs);
            
            foreach (var output in outputs)
            {
                output.OutputYearPlans.Sort((x, y) =>
                    x.Year.CompareTo(y.Year));
            }
        }
    }

    public class ResultsMatrixBusinessRulesPhysicalProgress
    {
        IEnumerable<IResultsMatrixAnnualPlan> _model;

        public ResultsMatrixBusinessRulesPhysicalProgress(IEnumerable<IResultsMatrixAnnualPlan> model)
        {
            _model = model;
        }

        public int GetReferenceLastYearOutputProduct()
        {
           int yearCount = _model.Where(
               x => x.Year != ResultsMatrixCodes.EndOfProjectYear &&
                   x.AnnualPlan != null).Count();
           int year = 0;

           if (yearCount > 0)
           {
              year = _model.Where(
                   x => x.Year != ResultsMatrixCodes.EndOfProjectYear &&
                   x.AnnualPlan != null).Max(x => x.Year);
           }
           else if (yearCount == 0)
               year = DateTime.Now.Year;
            else
                year = _model.Where(x => x.Year != ResultsMatrixCodes.EndOfProjectYear)
                    .Min(x => x.Year);

           return year;
        }

        public int GetReferenceYear(
            int referenceLastYearFinancialOutputProduct, int pmrYear)
        {
            return referenceLastYearFinancialOutputProduct < pmrYear ?
                referenceLastYearFinancialOutputProduct :
                pmrYear;
        }

        public int GetPmrYear(PMIOperationContext context = null)
        {
            if (context == null)
                context = new PMIOperationContext(null);

            return context.GetPMRReferenceYearForEOPCalculation();
        }

        public decimal? GetOriginalPlanSum()
        {
            return _model.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Sum(x => x.OriginalPlan);
        }

        public decimal? GetAnnualPlanSum(int referenceYear)
        {
            var resultMatrixAnnualPlanList = _model
                .Where(x => x.Year != ResultsMatrixCodes.EndOfProjectYear);

            if (resultMatrixAnnualPlanList.Any(x => x.AnnualPlan.HasValue))
            {
                return resultMatrixAnnualPlanList.Where(
                    x => x.Year < referenceYear)
                    .Sum(x => x.ActualValue) + resultMatrixAnnualPlanList.Where(
                    x => x.Year >= referenceYear)
                    .Sum(x => x.AnnualPlan);
            }

            return 0;
        }

        public decimal? GetActualValueSum()
        {
            return _model.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Sum(x => x.ActualValue);
        }
    }

    public class ResultsMatrixBusinessRulesFinancialProgress
    {
        IEnumerable<IResultsMatrixAnnualPlan> _modelComponents, _modelOtherCosts;

        public ResultsMatrixBusinessRulesFinancialProgress(
            IEnumerable<IResultsMatrixAnnualPlan> modelComponents,
            IEnumerable<IResultsMatrixAnnualPlan> modelOtherCosts)
        {
            _modelComponents = modelComponents;
            _modelOtherCosts = modelOtherCosts;
        }

        public decimal? GetCostOriginalSum()
        {
            return _modelComponents.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Select(
                x => x.OriginalPlan).DefaultIfEmpty(0.0m).Sum(x => x) + _modelOtherCosts.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Select(
                x => x.OriginalPlan).DefaultIfEmpty(0.0m).Sum(
                x => x);
        }

        public int GetReferenceLastYearOutputProduct()
        {
            int yearCount = _modelComponents.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear && 
                    x.AnnualPlan != null).Count();
            int year = 0;

            if (yearCount > 0)
            {
                year = _modelComponents.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear &&
                    x.AnnualPlan != null).Max(x => x.Year);
            }
            else if (yearCount == 0)
                year = DateTime.Now.Year;
            else
                year = _modelComponents.Where(x => x.Year != ResultsMatrixCodes.EndOfProjectYear)
                    .Min(x => x.Year);

            return year;
        }

        public int GetReferenceYear(
            int referenceLastYearFinancialOutputProduct, int pmrYear)
        {
            return referenceLastYearFinancialOutputProduct < pmrYear ?
                referenceLastYearFinancialOutputProduct :
                pmrYear;
        }

        public int GetPmrYear(PMIOperationContext context = null)
        {
            if (context == null)
                context = new PMIOperationContext(null);

            return context.GetPMRReferenceYearForEOPCalculation();
        }

        public decimal? GetCostAnnualSum(int referenceYearFinancial)
        {
            return _modelComponents.Where( 
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear && x.Year < referenceYearFinancial).Select(x => x.ActualValue).DefaultIfEmpty(0.0m).Sum(x => x) +
                _modelComponents.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear && x.Year >= referenceYearFinancial).Select(x => x.AnnualPlan).DefaultIfEmpty(0.0m).Sum(x => x) + 
                _modelOtherCosts.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear && x.Year < referenceYearFinancial).Select(x => x.ActualValue).DefaultIfEmpty(0.0m).Sum(x => x) +
                _modelOtherCosts.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear  && x.Year >= referenceYearFinancial).Select(x => x.AnnualPlan).DefaultIfEmpty(0.0m).Sum(x => x);
        }

        public decimal? GetTotalCostActualSum()
        {
            return _modelComponents.Where(
                x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Select(x => x.ActualValue).DefaultIfEmpty(0.0m).Sum(x => x) +
                _modelOtherCosts.Where(x => x.Year != ResultsMatrixCodes.EndOfProjectYear).Select(x => x.ActualValue).DefaultIfEmpty(0.0m).Sum(x => x);
        }
    }
}