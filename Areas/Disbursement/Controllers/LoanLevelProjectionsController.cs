using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using Newtonsoft.Json;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Architecture.Extensions;
using IDB.MW.Application.Disbursement;
using IDB.MW.Application.Disbursement.Messages;
using IDB.MW.Domain.Models.Disbursement;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVCExtensions;
using IDB.Presentation.MVC4.Models;
using IDB.Presentation.MVC4.Areas.Disbursement.Helpers;
using IDB.MW.Application.AdministrationModule.Messages.DisbursementService;

namespace IDB.Presentation.MVC4.Areas.Disbursement.Controllers
{
    public partial class LoanLevelProjectionsController : BaseController
    {
        private readonly IDisbursementService _disbursementService;
        private readonly IDisbursementProjectionEligibilityService _eligibilityService;

        #region contructors
        public LoanLevelProjectionsController(
            IDisbursementService disbursementService,
            IDisbursementProjectionEligibilityService eligibilityService)
        {
            _disbursementService = disbursementService;
            _eligibilityService = eligibilityService;
        }
        #endregion

        #region view methods
        public virtual ActionResult Index(
            string operationNumber,
            string group = DisbursementValues.SELECTED)
        {
            string pcMail = IDBContext.Current.UserName;
            var model = new LoanLevelProjectionsViewModel()
            {
                Loans = new List<ProjectedLoanModel>()
            };

            model.Download = true;

            try
            {
                model.Loans = _disbursementService.GetLoansModel(pcMail, operationNumber, group);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    DisbursementValues.LOAN_LEVEL_PROJECTIONS_CONTROLLER,
                    DisbursementValues.MSJ_ERROR_GET_LOAN_MODEL,
                    e);
                MessageHandler.SetMessage(
                    Localization.GetText(DisbursementValues.WARNING_UNHANDLED_ERROR),
                    DisbursementValues.ERROR);

                model.Download = false;
            }

            model.Group = group;

            if (model.Loans.Count == decimal.One)
            {
                return RedirectToAction(DisbursementValues.EDIT, new
                {
                    operationNumber = operationNumber,
                    loanNumber = model.Loans[0].LoanNumber,
                    fundCode = model.Loans[0].FundCode,
                    loanTcdNum = model.Loans[0].TCDNumber,
                    loanProjectNumber = model.Loans[0].ProjectNumber,
                    loanCurrencyCode = model.Loans[0].CurrTypeCode
                });
            }

            model.OperationNumber = operationNumber;

            return View(model);
        }

        public virtual ActionResult UpdateReferenceInfo(int year,
            string operationNumber,
            string loanNumber,
            string fundCode,
            string loanTcdNum,
            string loanProjectNumber,
            string loanCurrencyCode)
        {
            int currentYear = DateTime.Now.Year;
            if (year > 0)
            {
                currentYear = Convert.ToInt16(year);
            }

            string pcMail = IDBContext.Current.UserName;
            var model = new ProjectedLoanModel();
            model = _disbursementService.GetProjectionMatrixWithCache(
                null,
                operationNumber,
                pcMail,
                loanNumber,
                fundCode,
                loanTcdNum,
                loanProjectNumber,
                loanCurrencyCode);

            if (model.CurrentProjectionMonth <= DisbursementValues.ZERO_NUMB)
            {
                model.CurrentProjectionMonth =
                    model.ProjectedMonths != null && model.ProjectedMonths.Any()
                    ? model.ProjectedMonths.Where(o => o.Year == currentYear).Min(z => z.Month)
                    : DisbursementValues.ZERO_NUMB;
            }

            model.OperationNumber = operationNumber;
            model.LoanTCDNumber = loanTcdNum;
            model.FundCode = fundCode;
            model.LoanProjectNumber = loanProjectNumber;

            model.Projects = _disbursementService
                .GetReferenceInformation(model.OperationNumber, model, model.LoanNumber);
            var loanNumberToArray = new string[] { model.LoanNumber };
            var oldProjection = _disbursementService.GetMonthlyProyections(
                operationNumber,
                model.CurrentProjectionMonth,
                false,
                model.IsExecution,
                loanNumberToArray);

            model.LoanNumber = loanNumber;
            model.FundCode = fundCode;
            model.LoanTCDNumber = loanTcdNum;
            model.LoanProjectNumber = loanProjectNumber;
            model.LoanCurrencyCode = loanCurrencyCode;
            model.Download = true;
            model.OldProjection = oldProjection.Any()
                ? oldProjection : new List<ActualsFromEdwViewModel>();
            model.Total = _disbursementService.TotalOfProjections(
                oldProjection,
                model.ProjectedYears,
                model.Balance,
                model.CurrentProjectionMonth);
            model.ExchangeRate = decimal.One;

            FillProjectedYearsList(model);

            return PartialView("_GridReferenceInformation", model);
        }

        public virtual ActionResult Edit(
            string operationNumber,
            string loanNumber,
            string fundCode,
            string loanTcdNum,
            string loanProjectNumber,
            string loanCurrencyCode)
        {
            var model = new ProjectedLoanModel();

            try
            {
                var projectableOperationResponse = _eligibilityService
                .CheckIfContractIsProjectable(new DisbursementEligibilityRequest()
                {
                    OperationNumber = operationNumber,
                    ContractNumber = loanNumber
                });

                if (!projectableOperationResponse.IsValid)
                {
                    return View("NonProjectableScreen", new NoProjectableViewModel
                    {
                        OperationNumber = operationNumber,
                        ContractNumber = loanNumber,
                        IsAllNotProjectable = false
                    });
                }

                string pcMail = IDBContext.Current.UserName;
                int currentYear = DateTime.Now.Year;
                model = _disbursementService.GetProjectionMatrixWithCache(
                    null,
                    operationNumber,
                    pcMail,
                    loanNumber,
                    fundCode,
                    loanTcdNum,
                    loanProjectNumber,
                    loanCurrencyCode);

                if (model.CurrentProjectionMonth <= DisbursementValues.ZERO_NUMB)
                {
                    model.CurrentProjectionMonth =
                        model.ProjectedMonths != null && model.ProjectedMonths.Any()
                        ? model.ProjectedMonths.Where(o => o.Year == currentYear).Min(z => z.Month)
                        : DisbursementValues.ZERO_NUMB;
                }

                model.OperationNumber = operationNumber;
                model.LoanTCDNumber = loanTcdNum;
                model.FundCode = fundCode;
                model.LoanProjectNumber = loanProjectNumber;
                model.LoanNumber = loanNumber;
                model.LoanCurrencyCode = loanCurrencyCode;
                model.Download = true;
                var loanNumberToArray = new string[] { model.LoanNumber };
                var oldProjection = _disbursementService.GetMonthlyProyections(
                    operationNumber,
                    model.CurrentProjectionMonth,
                    false,
                    model.IsExecution,
                    loanNumberToArray);
                model.OldProjection = oldProjection.Any()
                    ? oldProjection : new List<ActualsFromEdwViewModel>();
                var currMonthOldProjection = model.OldProjection
                     .HasAny(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year) ?
                      model.OldProjection
                         .Where(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year)
                         .FirstOrDefault().Value : decimal.Zero;
                model.Projects = _disbursementService
                                   .GetReferenceInformation(model.OperationNumber,
                                   model,
                                   model.LoanNumber,
                                  currentMonthOldProjection: currMonthOldProjection);
                model.Total = _disbursementService.TotalOfProjections(
                    oldProjection,
                    model.ProjectedYears,
                    model.Balance,
                    model.CurrentProjectionMonth);
                FillProjectedYearsList(model);
                model.ExchangeRate = decimal.One;
                model.CurrCode = string.IsNullOrEmpty(model.CurrCode)
                    ? DisbursementValues.USD : model.CurrCode;
                var edwDataResponse = _disbursementService.GetActualAgreedCompleteProjections(
                    operationNumber, new string[] { loanNumber });
                model.EdwData = edwDataResponse.IsValid
                    ? edwDataResponse.Actual
                    : new List<ActualAgreedModel>();
                GenericOperationNumberRequest request = new GenericOperationNumberRequest();
                request.OperationNumber = operationNumber;
                model.RemainingBalance = model.RemainingBalance + (model.OldProjection
                    .Where(o => o.Month == model.CurrentProjectionMonth
                        && o.Year == DateTime.Now.Year)
                    .Select(o => o.Value).FirstOrDefault() / 1000);
                var otherFinancingSourcesResponse = _disbursementService
                    .GetOtherFinancingSources(request);

                if (otherFinancingSourcesResponse.IsValid)
                {
                    model.OtherFinancingSources = otherFinancingSourcesResponse.Value;
                }

                GenericLoanNumberRequest loanNumberRequest = new GenericLoanNumberRequest
                { LoanNumber = loanNumber, OperationNumber = operationNumber };
                var minimumAmountToBeJustifiedResponse = _disbursementService
                    .GetMinimumAmountToBeJustified(loanNumberRequest);
                if (minimumAmountToBeJustifiedResponse.IsValid)
                {
                    model.MinimumAmountPending = minimumAmountToBeJustifiedResponse.Value;
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    DisbursementValues.LOAN_LEVEL_PROJECTIONS_CONTROLLER,
                    DisbursementValues.MSJ_ERROR_GET_LOAN_MODEL,
                    ex);
                MessageHandler.SetMessage(
                    Localization.GetText(DisbursementValues.WARNING_UNHANDLED_ERROR),
                    DisbursementValues.ERROR);
                model.HasServiceError = true;
            }

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HasPermission(Permissions = Permission.DISBURSEMENT_PROJECTIONS_WRITE)]
        public virtual ActionResult Edit(
            string operationNumber,
            string loanNumber,
            string fundCode,
            string loanTcdNum,
            string loanProjectNumber,
            string group,
            string loanCurrencyCode)
        {
            var result = new Dictionary<string, string>();

            try
            {
                var sendedFields = Request.Params.AllKeys
                    .Where(exp => exp.EndsWith(DisbursementValues.END_VALUE));
                var sumOfFields = decimal.Zero;
                var cancellations =
                    decimal.Parse(Request.Params[DisbursementValues.DISBURSEMENTCANCELLATIONS]);
                var actualDisbursementProjection =
                    decimal.Parse(Request.Params[DisbursementValues.DISBURSEMENT_CURRENTMONTH]);
                var balance = decimal.Parse(
                    Request.Params[DisbursementValues.DISBURSEMENT_BALANCE]);
                sendedFields.ForEach(key => sumOfFields += decimal.Parse(Request.Params[key]));

                if (((sumOfFields - actualDisbursementProjection + cancellations) - balance) != 0)
                {
                    result.Add(DisbursementValues.ACTION, DisbursementValues.SHOW_ERROR);
                    result.Add(
                        DisbursementValues.ERROR_TEXT,
                        Localization.GetText("Disbursement.LoanLevelProj.Msg.ErrorSumValidation"));
                    return Json(result);
                }

                var projections = new List<ProjectionMatrixRequestModel>();
                foreach (var field in sendedFields)
                {
                    var year = field.Split(
                        DisbursementValues.UNDERLINE_CHAR)[1]
                        .Split(DisbursementValues.HYPHEN_CHAR)[0];
                    var month = field.Split(DisbursementValues.UNDERLINE_CHAR)[0];
                    var amount = Request.Params[field];
                    projections.Add(new ProjectionMatrixRequestModel()
                    {
                        Year = year,
                        Amount = amount,
                        Month = month
                    });
                }

                var callResult = _disbursementService.SaveProjectionMatrixLoanLevel(
                    string.Empty,
                    loanNumber,
                    loanTcdNum,
                    cancellations,
                    projections,
                    operationNumber);

                var messageType =
                    callResult.Contains(Localization.GetText(DisbursementValues.SAVE_MESSAGE)) ?
                        DisbursementValues.SUCCESS :
                        DisbursementValues.ERROR;

                MessageHandler.SetMessage(callResult, messageType);

                result.Add(DisbursementValues.ACTION, DisbursementValues.CALL_FUNCTION);
                result.Add(
                    DisbursementValues.CALL_FUNCTION_FN,
                    DisbursementValues.DISBURSEMENT_REDIRECT_SAVE);
            }
            catch (Exception e)
            {
                result[DisbursementValues.ACTION] = DisbursementValues.SHOW_ERROR;
                result[DisbursementValues.ERROR_TEXT] = e.Message;
            }

            return Content(new JavaScriptSerializer().Serialize(result));
        }

        public virtual ActionResult GetGraphData(Models.GraphModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                DateTime currentExpirationDate = graphData.CurrentDisbursementExpirationDate;
                string pcMail = IDBContext.Current.UserName;
                var result = new Dictionary<string, List<decimal?[]>>();

                var projectionMatrix = new ProjectedLoanModel()
                {
                    ProjectedMonths =
                    new List<MW.Domain.Models.Disbursement.ProjectedMonthYearModel>(),
                    AllLoans = new OperationLevelProjectionsViewModel()
                    {
                        ProjectedMonths =
                        new List<MW.Domain.Models.Disbursement.ProjectedMonthYearModel>()
                    }
                };

                if (graphData.ProjectedMonths != null)
                {
                    projectionMatrix.ProjectedMonths = graphData.ProjectedMonths.ConvertToModel();
                }

                if (graphData.AllLoansProjectedMonths != null)
                {
                    projectionMatrix.AllLoans.ProjectedMonths =
                        graphData.AllLoansProjectedMonths.ConvertToModel();
                }

                _disbursementService.CalculateGraphPoints(
                    result,
                    projectionMatrix,
                    graphData.OperationNumber,
                    pcMail,
                    false,
                    currentExpirationDate,
                    projectionMatrix.AllLoans);

                ajaxResult[DisbursementValues.ACTION] =
                    DisbursementValues.CALL_FUNCTION_PARAMETERED;
                ajaxResult[DisbursementValues.CALL_FUNCTION_FN] =
                    DisbursementValues.DISBURSEMENT_SHOW_GRAPH;
                ajaxResult[DisbursementValues.CALL_FUNCTION_PARAMETERS] =
                    string.Format("[{0}]", new JavaScriptSerializer().Serialize(result));
            }
            catch (Exception e)
            {
                ajaxResult[DisbursementValues.ACTION] = DisbursementValues.SHOW_ERROR;
                ajaxResult[DisbursementValues.ERROR_TEXT] = e.Message;
            }

            return Content(new JavaScriptSerializer().Serialize(ajaxResult));
        }

        public virtual ActionResult GetGraphYearlyProyections(Models.GraphViewModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                var result = new Dictionary<string, List<decimal?>>();
                string pcMail = IDBContext.Current.UserName;
                int currentYear = DateTime.Now.Year;
                var projectedMonths = graphData.ProjectedYears.ConvertToModel()
                    .Where(x => x.Year == currentYear)
                  .SelectMany(x => x.ProjectedMonths).ToList();
                projectedMonths.ForEach(x =>
                {
                    if (x.ProjectedAmount == decimal.MinValue)
                    {
                        x.ProjectedAmount = DisbursementValues.ZERO_NUMB;
                    }
                    else
                    {
                        x.ProjectedAmount *= graphData.ExchangeRate;
                    }
                });

                var currentProjection = graphData.CurrentProjectionMonth;
                var response = _disbursementService
                    .ConvertEdwDataToActualProjectionModel(
                    graphData.ActualAgreed,
                    graphData.ExchangeRate);

                result[DisbursementValues.ACTUAL_ACUMULATED] = new List<decimal?>();
                result[DisbursementValues.CURRENT_ACUMULATED] = new List<decimal?>();
                result[DisbursementValues.ACCUMULATED_PROJECTION] = new List<decimal?>();
                for (int i = 1; i <= 12; i++)
                {
                    result[DisbursementValues.ACTUAL_ACUMULATED].Add(
                        response.Where(x =>
                            x.ACCEOM_DT.Month <= i && x.ACCEOM_DT.Year == currentYear &&
                            x.LOAN_NUM.Equals(graphData.LoanNumber))
                        .Sum(x => x.AGREED_PROJ_AMNT) / DisbursementValues.ONE_THOUSAND_NUMB);

                    var tempCurrentAccumulated = projectedMonths.Where(y => y.Month <= i)
                        .Sum(x => x.ProjectedAmount);

                    if (tempCurrentAccumulated == decimal.Zero && i < currentProjection)
                    {
                        result[DisbursementValues.ACCUMULATED_PROJECTION].Add(
                            response.Where(x =>
                                x.ACCEOM_DT.Month <= i && x.ACCEOM_DT.Year == currentYear &&
                                x.LOAN_NUM.Equals(graphData.LoanNumber))
                            .Sum(x => x.DISB_AMNT_USEQ) / DisbursementValues.ONE_THOUSAND_NUMB);
                        result[DisbursementValues.CURRENT_ACUMULATED].Add(null);
                    }
                    else
                    {
                        result[DisbursementValues.ACCUMULATED_PROJECTION].Add(null);
                        result[DisbursementValues.CURRENT_ACUMULATED].Add(tempCurrentAccumulated);
                    }
                }

                ajaxResult[DisbursementValues.ACTION] =
                    DisbursementValues.CALL_FUNCTION_PARAMETERED;
                ajaxResult[DisbursementValues.CALL_FUNCTION_FN] =
                    DisbursementValues.DISBURSEMENT_SHOW_YEARLYGRAPH;
                ajaxResult[DisbursementValues.CALL_FUNCTION_PARAMETERS] =
                    string.Format(
                        DisbursementValues.STRING_FORMAT,
                        new JavaScriptSerializer().Serialize(result));
            }
            catch (Exception e)
            {
                ajaxResult[DisbursementValues.ACTION] = DisbursementValues.SHOW_ERROR;
                ajaxResult[DisbursementValues.ERROR_TEXT] = e.Message;
            }

            return Content(new JavaScriptSerializer().Serialize(ajaxResult));
        }

        public virtual ActionResult GetGraphMonthlyProyections(Models.GraphViewModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                var result = new Dictionary<string, List<decimal?>>();
                int currentYear = DateTime.Now.Year;

                var projectedMonths = graphData.ProjectedYears.ConvertToModel()
                    .Where(x => x.Year == currentYear).SelectMany(x => x.ProjectedMonths).ToList();
                projectedMonths.ForEach(x =>
                {
                    if (x.ProjectedAmount == decimal.MinValue)
                    {
                        x.ProjectedAmount = DisbursementValues.ZERO_NUMB;
                    }
                    else
                    {
                        x.ProjectedAmount *= graphData.ExchangeRate;
                    }
                });

                var currentProjection = graphData.CurrentProjectionMonth;
                var response = _disbursementService.ConvertEdwDataToActualProjectionModel(
                    graphData.ActualAgreed,
                    graphData.ExchangeRate);

                result[DisbursementValues.AGREED_PROJECTIONS] = new List<decimal?>();
                result[DisbursementValues.CURRENT_PROJECTIONS] = new List<decimal?>();
                result[DisbursementValues.ACTUAL_DISBURSEMENTS] = new List<decimal?>();

                for (int i = 1; i <= 12; i++)
                {
                    result[DisbursementValues.AGREED_PROJECTIONS].Add(
                        response.Where(x =>
                            x.ACCEOM_DT.Month == i && x.ACCEOM_DT.Year == currentYear &&
                            x.LOAN_NUM.Equals(graphData.LoanNumber))
                        .Sum(x => x.AGREED_PROJ_AMNT) / DisbursementValues.ONE_THOUSAND_NUMB);
                    result[DisbursementValues.CURRENT_PROJECTIONS].Add(
                         projectedMonths.Where(y => y.Month == i).Sum(x => x.ProjectedAmount));
                    result[DisbursementValues.ACTUAL_DISBURSEMENTS].Add(
                        response.Where(x =>
                            x.ACCEOM_DT.Month == i && x.ACCEOM_DT.Year == currentYear &&
                            x.LOAN_NUM.Equals(graphData.LoanNumber))
                        .Sum(x => x.DISB_AMNT_USEQ) / DisbursementValues.ONE_THOUSAND_NUMB);
                }

                ajaxResult[DisbursementValues.ACTION] =
                    DisbursementValues.CALL_FUNCTION_PARAMETERED;
                ajaxResult[DisbursementValues.CALL_FUNCTION_FN] =
                    DisbursementValues.DISBURSEMENT_SHOW_MONTHLYGRAPH;
                ajaxResult[DisbursementValues.CALL_FUNCTION_PARAMETERS] =
                    string.Format(
                        DisbursementValues.STRING_FORMAT,
                        new JavaScriptSerializer().Serialize(result));
            }
            catch (Exception e)
            {
                ajaxResult[DisbursementValues.ACTION] = DisbursementValues.SHOW_ERROR;
                ajaxResult[DisbursementValues.ERROR_TEXT] = e.Message;
            }

            return Content(new JavaScriptSerializer().Serialize(ajaxResult));
        }

        public virtual FileResult DownloadReport(
            string operationNumber,
            string format = "",
            string group = DisbursementValues.SELECTED)
        {
            var response = new DisbursementFileExportResponse();
            try
            {
                response = _disbursementService.ExportContractLevel(
                    operationNumber, format, group);
                if (!response.IsValid)
                {
                    throw new Exception(DisbursementValues.MSJ_ERROR_EXPORT
                        + DisbursementValues.UNDERLINE_STRING + response.ErrorMessage);
                }

                string fileName = IDBContext.Current.Operation
                    + DisbursementValues.UNDERLINE_STRING
                    + Localization.GetText(DisbursementValues.DISBURSEMENT_PROJECTIONS)
                    + Localization.GetText(DisbursementValues.LOAN_LEVEL);
                string application;
                switch (format)
                {
                    case DisbursementValues.EXCEL:
                        application = MimeTypeMap.GetMimeType(DisbursementValues.EXCEL);
                        fileName = fileName + DisbursementValues.DOT_STRING +
                            DisbursementValues.EXCEL;
                        break;
                    default:
                        application = MimeTypeMap.GetMimeType(DisbursementValues.PDF);
                        fileName = fileName + DisbursementValues.DOT_STRING +
                            DisbursementValues.PDF;
                        break;
                }

                return File(response.File, application, fileName);
            }
            catch (Exception)
            {
                MessageHandler.SetMessage(
                    Localization.GetText(DisbursementValues.WARNING_UNHANDLED_ERROR),
                    DisbursementValues.ERROR);
            }

            return null;
        }

        public virtual FileResult DownloadReportByLonNumber(
            string operationNumber,
            string loanNumber,
            string fundCode,
            string loanTcdNum,
            string loanProjectNumber,
            string loanCurrencyCode,
            string format = "",
            string group = DisbursementValues.SELECTED)
        {
            if ((format == DisbursementValues.PDF) || (format == DisbursementValues.EXCEL))
            {
                string pcMail = IDBContext.Current.UserName;

                OperationLevelProjectionsViewModel model;
                var projectionMatrix = _disbursementService.GetProjectionMatrixWithCache(
                    null,
                    operationNumber,
                    pcMail,
                    loanNumber,
                    fundCode,
                    loanTcdNum,
                    loanProjectNumber,
                    loanCurrencyCode);

                var oldProjection = _disbursementService.GetMonthlyProyections(
                    operationNumber,
                    projectionMatrix.CurrentProjectionMonth,
                    false,
                    projectionMatrix.IsExecution,
                    new string[] { loanNumber });

                model =
                    new OperationLevelProjectionsViewModel
                    {
                        ProjectedMonths = projectionMatrix.ProjectedMonths,
                        ProjectedYears = projectionMatrix.ProjectedYears,
                        OldProjection = oldProjection,
                        CurrentProjectionMonth = projectionMatrix.CurrentProjectionMonth,
                        Balance = projectionMatrix.Balance
                    };

                var exportModel = _disbursementService.FormatOperationLevelModelToExport(model);

                try
                {
                    DownloadDisbursementReportResponse response = _disbursementService
                        .DownloadDisbursementProjectionsReport(exportModel, format);
                    if (!response.IsValid)
                    {
                        throw new Exception(DisbursementValues.MSJ_ERROR_EXPORT
                            + DisbursementValues.UNDERLINE_STRING + response.ErrorMessage);
                    }

                    string fileName = IDBContext.Current.Operation
                        + DisbursementValues.UNDERLINE_STRING
                        + Localization.GetText(DisbursementValues.DISBURSEMENT_PROJECTIONS)
                        + Localization.GetText(DisbursementValues.LOAN_LEVEL);
                    string application;
                    switch (format)
                    {
                        case DisbursementValues.EXCEL:
                            application = MimeTypeMap.GetMimeType(DisbursementValues.EXCEL);
                            fileName = fileName + DisbursementValues.DOT_STRING +
                                DisbursementValues.EXCEL;
                            break;
                        default:
                            application = MimeTypeMap.GetMimeType(DisbursementValues.PDF);
                            fileName = fileName + DisbursementValues.DOT_STRING +
                                DisbursementValues.PDF;
                            break;
                    }

                    return File(response.File, application, fileName);
                }
                catch (Exception)
                {
                    MessageHandler.SetMessage(
                        Localization.GetText(DisbursementValues.WARNING_UNHANDLED_ERROR),
                        DisbursementValues.ERROR);
                }
            }

            return null;
        }

        public virtual ActionResult GroupingSection(ConvertCurrencyViewModel model)
        {
            if (model == null)
            {
                model = new ConvertCurrencyViewModel();
            }

            var modelExchanged = _disbursementService.ExchangeHeaderLoanLevelSection(model);
            return PartialView("Partials/GroupingSection", modelExchanged);
        }

        public virtual ActionResult GridProjections(ConvertCurrencyViewModel model)
        {
            if (model == null)
            {
                model = new ConvertCurrencyViewModel();
            }

            var modelExchanged = _disbursementService.ExchangeGridProjections(model);

            return PartialView("_GridProjections", (IProjectionViewModel)modelExchanged);
        }

        public virtual ActionResult ReferenceInformation(ConvertCurrencyViewModel model)
        {
            if (model == null)
            {
                model = new ConvertCurrencyViewModel();
            }

            var modelExchanged = new ProjectedLoanModel();
            modelExchanged.ProjectedYears = model.ProjectedYears;
            modelExchanged.ProjectedMonths = model.ProjectedMonths;
            modelExchanged.OperationNumber = model.OperationNumber;
            modelExchanged.CurrentProjectionMonth = model.CurrentProjectionMonth;

            var referenceInformationRequest = new ReferenceInformationRequest()
            {
                OperationNumber = model.OperationNumber,
                Model = modelExchanged,
                SelectedYear = model.SelectedYear,
                LoanNumber = model.LoanNumber,
                ExchangeRate = model.ExchangeRate,
                CurrCode = model.CurrCode,
                IsExecution = model.IsExecution,
                CurrentProjectionMonth = model.CurrentProjectionMonth
            };

            var referenceInformationResponse = _disbursementService
                .GetReferenceInformationByYear(referenceInformationRequest);

            modelExchanged.Projects = referenceInformationResponse.ReferenceInfo;

            ConvertCurrencyViewModel modelExchangedCurrency =
                CreateNewConvertCurrencyViewModel(modelExchanged, model);

            var projects = _disbursementService
                .ExchangeReferenceInformation(modelExchangedCurrency);
            modelExchanged.Projects = projects;
            FillProjectedYearsList(modelExchanged);
            return PartialView("_GridReferenceInformation", modelExchanged);
        }

        public virtual JsonResult ConvertCurrency(string currency)
        {
            var response = _disbursementService.GetExchangeRate(currency);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult RetrieveFinancialData(
            string operationNumber,
            ConvertCurrencyViewModel projectionsModel)
        {
            if (projectionsModel == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    DisbursementValues.INVALID_REQUEST);
            }

            var financialResponse = _disbursementService
                .GetFinancialPlan(new FinancialPlanProjectionRequest()
                {
                    ApprovedCurrency = projectionsModel.CurrCode,
                    ContractNumber = projectionsModel.LoanNumber,
                    IsInExecution = projectionsModel.IsExecution,
                    OperationNumber = projectionsModel.OperationNumber,
                    ProjectionsModel = projectionsModel
                });

            string htmlStringView = string.Empty;
            if (financialResponse.IsValid)
            {
                projectionsModel.ProjectedYears = financialResponse.Projections;
                var outOfRangeProjections = projectionsModel.CurrentProjectionRequest
                    .Where(x => x.IsOutofRange);
                foreach (var item in outOfRangeProjections)
                {
                    var months = projectionsModel.ProjectedYears
                        .FirstOrDefault(y => y.Year == item.Year);
                    if (months == null)
                    {
                        continue;
                    }

                    var month = months.ProjectedMonths
                        .FirstOrDefault(m => m.Month == item.Month);
                    if (month != null)
                    {
                        month.ProjectedAmount = decimal.MinValue;
                    }
                }

                IProjectionViewModel resultModel = MappProjectionViewModelModel(projectionsModel);
                htmlStringView = Helpers.ViewHelper.ViewStringify(
                    "~/Areas/Disbursement/Views/Shared/_GridProjectForm.cshtml",
                    resultModel,
                    this);
            }

            return Json(
                new
                {
                    FinancialData = htmlStringView,
                    MessageType = financialResponse.MessageType,
                    IsValid = financialResponse.IsValid
                },
                JsonRequestBehavior.AllowGet);
        }

        private IProjectionViewModel MappProjectionViewModelModel(
            ConvertCurrencyViewModel projectionsModel)
        {
            LoanLevelProjectionsEditViewModel loanModel = new LoanLevelProjectionsEditViewModel()
            {
                Balance = projectionsModel.Balance,
                Cancellations = projectionsModel.Cancellations,
                CurrentDisbursementeExpirationDate = DateTime.Now,
                CurrentDisbursementExpirationDate =
                projectionsModel.CurrentDisbursementExpirationDate,
                OriginalExpirationDate = projectionsModel.OriginalExpirationDate,
                CurrentProjectionMonth = projectionsModel.CurrentProjectionMonth,
                Disbursement = default(decimal),
                GroupOperations = false,
                IsExecution = projectionsModel.IsExecution,
                IsOperationLevel = false,
                OldProjection = projectionsModel.OldProjection,
                OtherFinancingSources = projectionsModel.OtherFinancingSources,
                PCMailId = string.Empty,
                ProjectedLoans = null,
                ProjectedMonths = null,
                ProjectedYears = projectionsModel.ProjectedYears,
                ProjectedYearsList = null,
                Projects = null,
                RemainingBalance = projectionsModel.RemainingBalance,
                SumofProjections = default(decimal),
                Total = projectionsModel.Total,
                TotalProjectionFinancialPlan = default(decimal),
                UndisbursedBalance = default(decimal),
                UserId = default(int),
                CurrCode = projectionsModel.CurrCode
            };

            return (IProjectionViewModel)loanModel;
        }

        private void FillProjectedYearsList(ProjectedLoanModel model)
        {
            var request = new GenericOperationNumberRequest()
            {
                OperationNumber = model.OperationNumber
            };
            var operationId = _disbursementService.GetOperationId(request);
            ApprovalMilestoneDateResponse appDateResponse = new ApprovalMilestoneDateResponse();
            if (operationId.IsValid)
            {
                appDateResponse = _disbursementService
                    .GetApprovalMilestone(Convert.ToInt32(operationId.Value));
            }

            model.ProjectedYearsList = new List<int>();
            if (appDateResponse != null
                && appDateResponse.ApprovalMilestoneDate.HasValue)
            {
                int approbalYear = appDateResponse.ApprovalMilestoneDate.Value.Year;
                for (int i = DateTime.Now.Year; i >= approbalYear; i--)
                {
                    model.ProjectedYearsList.Add(i);
                }
            }
            else
            {
                model.ProjectedYearsList.Add(DateTime.Now.Year);
            }
        }

        private ConvertCurrencyViewModel CreateNewConvertCurrencyViewModel(
            ProjectedLoanModel modelExchanged,
            ConvertCurrencyViewModel originalModel)
        {
            if (originalModel != null)
            {
                ConvertCurrencyViewModel model = new ConvertCurrencyViewModel()
                {
                    HasOperationLevelProjection = originalModel.HasOperationLevelProjection,
                    Balance = originalModel.Balance,
                    RemainingBalance = originalModel.RemainingBalance,
                    IsExecution = originalModel.IsExecution,
                    EditMode = originalModel.EditMode,
                    Cancellations = originalModel.Cancellations,
                    CurrCode = originalModel.CurrCode,
                    PartialEligibilityDate = originalModel.PartialEligibilityDate,
                    TotalEligibilityDate = originalModel.TotalEligibilityDate,
                    OriginalExpirationDate = originalModel.OriginalExpirationDate,
                    CurrentDisbursementExpirationDate =
                    originalModel.CurrentDisbursementExpirationDate,
                    CummulativeExtensions = originalModel.CummulativeExtensions,
                    OperationNumber = originalModel.OperationNumber,
                    LoanNumber = originalModel.LoanNumber,
                    ProjectedYears = originalModel.ProjectedYears,
                    ProjectedMonths = originalModel.ProjectedMonths,
                    SelectedYear = originalModel.SelectedYear,
                    ExchangeRate = originalModel.ExchangeRate,

                    ActualAccumDisbCurrentAccumProj =
                    modelExchanged.Projects[DisbursementValues.LITERAL_ACTUAL],
                    AccumProjAgreedAccumProjPerc =
                    modelExchanged.Projects[DisbursementValues.LITERAL_ACTUAL_AGREED],
                    AccumProjAgreedAccumProj =
                    modelExchanged.Projects[DisbursementValues.LITERAL_ACTUAL_AGREED_ACCUM],
                    AgreedAccumProj =
                    modelExchanged.Projects[DisbursementValues.LITERAL_AGREED_ACCUM],
                    AgreedProjection =
                    modelExchanged.Projects[DisbursementValues.LITERAL_AGREED_PROJECTION],
                };

                return model;
            }
            else
            {
                return new ConvertCurrencyViewModel();
            }
        }
        #endregion
    }
}