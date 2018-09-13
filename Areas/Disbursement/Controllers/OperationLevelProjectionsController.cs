using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
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
    public partial class OperationLevelProjectionsController : BaseController
    {
        #region fields
        private readonly IDisbursementService _disbursementService;
        private readonly IDisbursementProjectionEligibilityService _eligibilityService;
        #endregion

        #region contructors
        public OperationLevelProjectionsController(
            IDisbursementService disbursementService,
            IDisbursementProjectionEligibilityService eligibilityService)
        {
            _disbursementService = disbursementService;
            _eligibilityService = eligibilityService;
        }
        #endregion

        public virtual ActionResult Index(
            string operationNumber,
            string group = DisbursementValues.SELECTED,
            bool IsFromSharePoint = true)
        {
            var prjRsp = _eligibilityService.CheckIfOperationIsProjectable(
                new DisbursementEligibilityRequest() { OperationNumber = operationNumber });
            if (prjRsp != null && !prjRsp.IsValid)
            {
                var contract = prjRsp.EligibleContracts.HasAny()
                    && prjRsp.EligibleContracts.Count == decimal.One
                    ? prjRsp.EligibleContracts.FirstOrDefault() : null;
                return View("NonProjectableScreen", new NoProjectableViewModel
                {
                    ContractNumber = contract != null ? contract.ContractNumber : string.Empty,
                    OperationNumber = operationNumber,
                    IsAllNotProjectable = true
                });
            }

            var isExecution = _disbursementService.IsInExecution(operationNumber).IsValid;
            if (isExecution && IsFromSharePoint)
            {
                ProjectLoanResponse operationLoanInfo = _disbursementService.GetLoansInfo(
                    new GenericOperationNumberRequest { OperationNumber = operationNumber });
                if (operationLoanInfo.IsValid
                    && operationLoanInfo.ProjectLoanModels.Count == DisbursementValues.ONE_NUMB)
                {
                    var loanData = operationLoanInfo.ProjectLoanModels.FirstOrDefault();
                    return RedirectToAction(DisbursementValues.EDIT,
                    new RouteValueDictionary(new
                    {
                        area = DisbursementValues.DISBURSEMENT,
                        controller = DisbursementValues.LOAN_LEVEL_PROJECTIONS,
                        action = DisbursementValues.EDIT,
                        operationNumber = operationNumber,
                        loanNumber = loanData.LoanTCNumber,
                        fundCode = loanData.FundCode,
                        loanTcdNum = loanData.LoanTCDNum,
                        loanProjectNumber = loanData.ProjNum,
                        loanCurrencyCode = loanData.CurrCode
                    }));
                }
                else
                {
                    return RedirectToAction(DisbursementValues.INDEX,
                    new RouteValueDictionary(new
                    {
                        area = DisbursementValues.DISBURSEMENT,
                        controller = DisbursementValues.LOAN_LEVEL_PROJECTIONS,
                        action = DisbursementValues.INDEX,
                        operationNumber = operationNumber
                    }));
                }
            }

            string pcMail = IDBContext.Current.UserName;
            var model = _disbursementService.GetProjectionMatrixWithCache(null,
                pcMail,
                operationNumber,
                group,
                isExecution);
            model.OperationNumber = operationNumber;
            model.Download = true;
            model.Group = group;
            model.ExchangeRate = decimal.One;
            model.IsExecution = isExecution;
            var currentYear = DateTime.Now.Year;
            if (model.CurrentProjectionMonth <= DisbursementValues.ZERO_NUMB)
            {
                model.CurrentProjectionMonth = model.ProjectedMonths != null
                    && model.ProjectedMonths.Any()
                    ? model.ProjectedMonths.Where(o => o.Year == currentYear).Min(z => z.Month)
                    : DisbursementValues.ZERO_NUMB;
            }

            var oldProjection = _disbursementService.GetMonthlyProyections(operationNumber,
                model.CurrentProjectionMonth,
                true,
                model.IsExecution,
                model.LoanNumbers,
                model.ExchangeRates);

            var request = new GenericOperationNumberRequest();
            request.OperationNumber = operationNumber;
            if (model.IsExecution)
            {
                model.Projects = _disbursementService.GetReferenceInformation(model.OperationNumber,
                    model,
                    null,
                    model.ExchangeRates,
                    currentMonthOldProjection: oldProjection
                        .Where(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year)
                        .FirstOrDefault().Value);
                var response = _disbursementService.GetGovernmentBudget(request);
                model.GovernmentBudget = response.Value;
            }

            model.OldProjection = oldProjection;
            model.Total = _disbursementService.TotalOfProjections(
                oldProjection, model.ProjectedYears, model.Balance, model.CurrentProjectionMonth);
            FillProjectedYearsList(model);
            var edwDataResponse = _disbursementService
                .GetActualAgreedCompleteProjections(operationNumber, model.LoanNumbers);
            model.EdwData = edwDataResponse.IsValid
                ? edwDataResponse.Actual : new List<ActualAgreedModel>();
            var otherFinancingSourcesResponse = _disbursementService
                .GetOtherFinancingSources(request);
            if (otherFinancingSourcesResponse.IsValid)
            {
                model.OtherFinancingSources = otherFinancingSourcesResponse.Value;
            }
            else
            {
                model.OtherFinancingSources = 0m;
            }

            if (model.HasError)
            {
                MessageHandler.SetMessage(Localization.GetText(
                    DisbursementValues.WARNING_UNHANDLED_ERROR), DisbursementValues.ERROR);
            }

            return View(model);
        }

        public virtual ActionResult UpdateReferenceInfo(
            string year)
        {
            var model = new ProjectedLoanModel();

            return View(model);
        }

        public virtual ActionResult GetGraphData(Models.GraphModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                var result = new Dictionary<string, List<decimal?[]>>();
                string operationNumber = graphData.OperationNumber;
                string group = graphData.Group;
                string pcMail = IDBContext.Current.UserName;
                var currentExpiration = graphData.CurrentDisbursementExpirationDate;

                var projectionMatrix = new OperationLevelProjectionsViewModel()
                {
                    ProjectedMonths = graphData.ProjectedMonths != null
                   ? graphData.ProjectedMonths.ConvertToModel()
                   : new List<MW.Domain.Models.Disbursement.ProjectedMonthYearModel>()
                };

                _disbursementService.CalculateGraphPoints(
                    result,
                    projectionMatrix,
                    operationNumber,
                    pcMail,
                    group == DisbursementValues.GROUPED,
                    currentExpiration);

                ajaxResult[DisbursementValues.ACTION] =
                    DisbursementValues.CALL_FUNCTION_PARAMETERED;
                ajaxResult[DisbursementValues.CALL_FUNCTION_FN] =
                    DisbursementValues.DISBURSEMENT_SHOW_GRAPH;
                ajaxResult[DisbursementValues.CALL_FUNCTION_PARAMETERS] =
                    string.Format(
                        DisbursementValues.STRING_FORMAT,
                        new JavaScriptSerializer().Serialize(result));
            }
            catch (Exception e)
            {
                ajaxResult[DisbursementValues.ACTION] = DisbursementValues.SHOW_ERROR;
                ajaxResult[DisbursementValues.ERROR_TEXT] = e.Message;
                Logger.GetLogger()
                    .WriteMessage(
                        DisbursementValues.OPERATION_LEVEL_PROJECTIONS_CONTROLLER,
                        e.Message + e.StackTrace);
            }

            return Content(new JavaScriptSerializer().Serialize(ajaxResult));
        }

        public virtual ActionResult GetGraphYearlyProyections(Models.GraphViewModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                var newGraph = new MW.Domain.Models.Disbursement.GraphViewModel
                {
                    ActualAgreed = graphData.ActualAgreed,
                    CurrentProjectionMonth = graphData.CurrentProjectionMonth,
                    ExchangeRate = graphData.ExchangeRate,
                    ExchangeRates = graphData.ExchangeRates != null
                    ? graphData.ExchangeRates.Where(x => x != null).ToList()
                    : new List<ExchangeRateModel>(),
                    IsExecution = graphData.IsExecution,
                    LoanNumber = graphData.LoanNumber,
                    LoanNumbers = graphData.LoanNumbers,
                    ProjectedYears = graphData.ProjectedYears.ConvertToModel()
                };
                var result = _disbursementService.SetYearlyProjections2(newGraph);

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

        public virtual JsonResult CalculatePercent(
            decimal balance,
            decimal totalYear)
        {
            return Json(
                _disbursementService.CalculatePercent(balance, totalYear),
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetGraphMonthlyProyections(Models.GraphViewModel graphData)
        {
            var ajaxResult = new Dictionary<string, string>();

            try
            {
                var newGraph = new MW.Domain.Models.Disbursement.GraphViewModel
                {
                    ActualAgreed = graphData.ActualAgreed,
                    CurrentProjectionMonth = graphData.CurrentProjectionMonth,
                    ExchangeRate = graphData.ExchangeRate,
                    ExchangeRates = graphData.ExchangeRates != null
                    ? graphData.ExchangeRates.Where(x => x != null).ToList()
                    : new List<ExchangeRateModel>(),
                    IsExecution = graphData.IsExecution,
                    LoanNumber = graphData.LoanNumber,
                    LoanNumbers = graphData.LoanNumbers,
                    ProjectedYears = graphData.ProjectedYears.ConvertToModel()
                };
                var result = _disbursementService.SetMonthlyProjections2(newGraph);

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

        [AcceptVerbs(HttpVerbs.Post)]
        [HasPermission(Permissions = Permission.DISBURSEMENT_PROJECTIONS_WRITE)]
        public virtual ActionResult Edit(string group, string operationNumber)
        {
            var result = new Dictionary<string, string>();
            try
            {
                string pcMail = IDBContext.Current.UserName;
                var sendedFields = Request.Params.AllKeys
                    .Where(exp => exp.EndsWith(DisbursementValues.END_VALUE));

                var sumOfFields = decimal.Zero;
                var cancellations =
                    decimal.Parse(Request.Params[DisbursementValues.DISBURSEMENTCANCELLATIONS]);
                sendedFields.ForEach(key => sumOfFields += decimal.Parse(Request.Params[key]));
                var actualDisbursementProjection = decimal.Parse(
                    Request.Params[DisbursementValues.DISBURSEMENT_CURRENTMONTH]);
                var balance = decimal.Parse(
                    Request.Params[DisbursementValues.DISBURSEMENT_BALANCE]);

                if ((sumOfFields - actualDisbursementProjection + cancellations - balance) != 0)
                {
                    result.Add(DisbursementValues.ACTION, DisbursementValues.SHOW_ERROR);
                    result.Add(
                        DisbursementValues.ERROR_TEXT,
                        Localization.GetText(DisbursementValues.MSJ_ERROR_UNDISBURSED_BALANCE));
                    return Json(result);
                }

                var expirationDate = DateTime.Parse(
                    Request.Params[DisbursementValues.DISBURSEMENT_CURRENTEXPDATE]);
                var response = _disbursementService
                    .GetProjections(sendedFields, Request, expirationDate);
                if (response.HaveMsg)
                {
                    MessageHandler.SetMessage(
                        Localization.GetText(DisbursementValues.MSJ_ERROR_EXPIRATION_DATE),
                        DisbursementValues.WARNING);
                }

                var callResult =
                    _disbursementService.SaveProjectionMatrixOperationLevel(
                        pcMail,
                        operationNumber,
                        cancellations,
                        response.Projections,
                        group);

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

        public virtual FileResult DownloadReport(
            string operationNumber,
            string format = "",
            string group = DisbursementValues.SELECTED)
        {
            string pcMail = IDBContext.Current.UserName;

            OperationLevelProjectionsViewModel model;

            string cacheName = string.Format(
                DisbursementValues.DISBURSEMENT_CACHE_SKELETON,
                pcMail,
                operationNumber,
                group);

            var isInExecution = _disbursementService.IsInExecution(operationNumber);
            model = _disbursementService.GetProjectionMatrixWithCache(
                null,
                pcMail,
                operationNumber,
                group,
                isInExecution.IsValid);

            var oldProjection = _disbursementService.GetMonthlyProyections(
            operationNumber,
            model.CurrentProjectionMonth,
            true,
            model.IsExecution,
            model.LoanNumbers,
            model.ExchangeRates);
            model.OldProjection = oldProjection != null
                ? oldProjection : new List<ActualsFromEdwViewModel>();

            var exportModel = _disbursementService.FormatOperationLevelModelToExport(model);

            try
            {
                DownloadDisbursementReportResponse response =
                    _disbursementService.DownloadDisbursementProjectionsReport(exportModel, format);
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

                    var month = months.ProjectedMonths.FirstOrDefault(m => m.Month == item.Month);
                    if (month != null)
                    {
                        month.ProjectedAmount = decimal.MinValue;
                    }
                }

                IProjectionViewModel resultModel = MappProjectionViewModelModel(projectionsModel);
                htmlStringView = Helpers.ViewHelper
                    .ViewStringify("~/Areas/Disbursement/Views/Shared/_GridProjectForm.cshtml",
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

            var referenceInformationRequest = new ReferenceInformationRequest()
            {
                OperationNumber = model.OperationNumber,
                Model = modelExchanged,
                SelectedYear = model.SelectedYear,
                LoanNumber = model.LoanNumber,
                IsExecution = model.IsExecution,
                ExchangeRates = model.ExchangeRates,
                CurrentProjectionMonth = model.CurrentProjectionMonth
            };

            var referenceInformationResponse = _disbursementService
                .GetReferenceInformationByYear(referenceInformationRequest);
            modelExchanged.Projects = referenceInformationResponse.ReferenceInfo;

            FillProjectedYearsList(modelExchanged);
            return PartialView("_GridReferenceInformation", (IProjectionViewModel)modelExchanged);
        }

        public virtual ActionResult NonProjectionScreen(
            string operationNumber, DisbursementEligibilityResponse projectableOperationResponse)
        {
            var contract = projectableOperationResponse != null
                && projectableOperationResponse.EligibleContracts != null
                ? projectableOperationResponse.EligibleContracts.FirstOrDefault()
                : null;
            return View("NonProjectableScreen", new NoProjectableViewModel
            {
                ContractNumber = contract != null ? contract.ContractNumber : string.Empty,
                OperationNumber = operationNumber,
                IsAllNotProjectable = true
            });
        }

        private IProjectionViewModel MappProjectionViewModelModel(
            ConvertCurrencyViewModel projectionsModel)
        {
            OperationLevelProjectionsViewModel model = new OperationLevelProjectionsViewModel()
            {
                Balance = projectionsModel.Balance,
                Cancellations = projectionsModel.Cancellations,
                CurrentDisbursementExpirationDate =
                projectionsModel.CurrentDisbursementExpirationDate,
                CurrentProjectionMonth = projectionsModel.CurrentProjectionMonth,
                Disbursement = default(decimal),
                IsExecution = projectionsModel.IsExecution,
                IsOperationLevel = false,
                OldProjection = projectionsModel.OldProjection,
                OtherFinancingSources = projectionsModel.OtherFinancingSources,
                PCMailId = string.Empty,
                ProjectedMonths = null,
                ProjectedYears = projectionsModel.ProjectedYears,
                ProjectedYearsList = null,
                Projects = null,
                RemainingBalance = projectionsModel.RemainingBalance,
                SumofProjections = default(decimal),
                Total = projectionsModel.Total,
                TotalProjectionFinancialPlan = default(decimal),
                UndisbursedBalance = default(decimal)
            };

            return (IProjectionViewModel)model;
        }

        private void FillProjectedYearsList(IProjectionViewModel model)
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
            if (appDateResponse != null &&
                appDateResponse.ApprovalMilestoneDate.HasValue)
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
    }
}