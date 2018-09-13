using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.Models.OperationProfile.FinancingData;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.OperationProfile.Controllers
{
    //[Interceptor(typeof(IExceptionHandlingAspect))]
    public partial class FinancingDataController : BaseController
    {
        private ILoanOperationDataService _clientLoan = null;

        public ILoanOperationDataService ClientLoan
        {
            get { return _clientLoan; }
            set { _clientLoan = value; }
        }

        private IBasicDataModelRepository _clientBasicData = null;
        public virtual IBasicDataModelRepository clientBasicData
        {
            get { return _clientBasicData; }
            set { _clientBasicData = value; }
        }

        private IConvergenceMasterDataModelRepository _masterData = null;
        public virtual IConvergenceMasterDataModelRepository masterData
        {
            get { return _masterData; }
            set { _masterData = value; }
        }

        [ExceptionHandling]
        public virtual ActionResult Index(string OperationNumber, bool displayAsGroup = false)
        {
            FinancingDataLoanModel financingModel = null;

            Session["displayAsGroup"] = displayAsGroup;
            ViewData["userId"] = IDBContext.Current.UserName;
            ViewData["operationNumber"] = OperationNumber;
            ViewData["displayAsGroup"] = displayAsGroup.ToString().ToLower();
            financingModel = ClientLoan.GetFinancingData(IDBContext.Current.UserName, OperationNumber, displayAsGroup);

            if (financingModel == null)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessageOPFinancingData(499, false, 1);

                // Pass message to the view
                ViewData["message"] = message;
            }
            else
            {
                foreach (var itemLoan in financingModel.Loans)
                {
                    itemLoan.LoanStage = masterData.GetMasterDataNameByCode("LOAN_STATUS", itemLoan.LoanStage, IDBContext.Current.CurrentLanguage);
                }
            }

            ViewBag.LoanStatusList = masterData.GetMasterDataModels("LOAN_STATUS"); //LOAN_STATUS
            return View(financingModel);
        }

        public virtual ActionResult GetOperationloans(string userId, string operationNumber, bool displayAsGroup)
        {
            Session["displayAsGroup"] = displayAsGroup;
            var financingModel = ClientLoan.GetFinancingData(userId, operationNumber, displayAsGroup);
            return Json(financingModel, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Loans(string operationNumber, bool displayAsGroup, string userId)
        {
            var financingModel = ClientLoan.GetFinancingData(userId, operationNumber, displayAsGroup);
            return Json(financingModel.Loans, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult LoanDetail(string operationNumber, string loanNumber)
        {
            return RedirectToAction("Details", "Loan", new { loanNumber = loanNumber, operationNumber = operationNumber, displayAsGroup = false });
        }

        public virtual ActionResult DownloadReport(string OperationNumber, bool displayAsGroup = false, string format = "")
        {
            if ((format == "pdf") || (format == "xls"))
            {
                var financingModel = ClientLoan.GetFinancingData(IDBContext.Current.UserName, OperationNumber, true);
                if (financingModel != null)
                {
                    foreach (var itemLoan in financingModel.Loans)
                    {
                        itemLoan.LoanStage = masterData.GetMasterDataNameByCode("LOAN_STATUS", itemLoan.LoanStage, IDBContext.Current.CurrentLanguage);
                    }

                    var ReportConfiguration = new ReportConfiguration<LoanModel>() { Name = IDBContext.Current.Operation + "_" + Localization.GetText("FinancingData"), Format = format, Data = financingModel.Loans, Columns = new string[] { "OperationNumber", "LoanNumber", "ExecutingAgency", "LoanStage", "CurrentIDB", "DisbursedPercentage", "Balance" }, OperationNumber = OperationNumber };
                    string Result = new ReportBuilder().GenerateReport(ReportConfiguration);

                    string filename = ReportConfiguration.Name + "." + format;
                    Response.AppendHeader("Content-Disposition", "inline;filename=" + filename);

                    if (format == "pdf")
                    {
                        Response.ContentType = "application/vnd.pdf";
                    }
                    else
                    {
                        Response.ContentType = "application/vnd.ms-excel";
                        Response.Write(Result);
                    }

                    Response.End();
                }
            }

            return View();
        }
    }
}