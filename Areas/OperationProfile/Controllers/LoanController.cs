using System.Web.Mvc;

using IDB.MW.Domain.Models.OperationProfile.FinancingData;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.EsgValueData;

namespace IDB.Presentation.MVC4.Areas.OperationProfile.Controllers
{
    public partial class LoanController : BaseController
    {
        private ILoanOperationDataService _clientLoan = null;
        
        // GET: /OperationProfile/EnvironmentalAndSocialImpactaData/
        private IEsgValueDataModelRepository _client = null;
        public IEsgValueDataModelRepository client
        {
            get { return _client; }
            set { _client = value; }
        }

        public ILoanOperationDataService ClientLoan
        {
            get { return _clientLoan; }
            set { _clientLoan = value; }
        }

        private IConvergenceMasterDataModelRepository _masterData = null;
        public virtual IConvergenceMasterDataModelRepository masterData
        {
            get { return _masterData; }
            set { _masterData = value; }
        }

        public virtual ActionResult Details(string loanNumber, string operationNumber, bool displayAsGroup) 
        {
            OperationModel operationModel = client.GetParentOperation(new OperationModel() { OperationId = -1, OperationNumber = operationNumber.Trim() });
            ViewData["userId"] = IDBContext.Current.UserName;
            ViewData["operationNumber"] = operationModel.OperationNumber;

            ViewData["loanNumber"] = loanNumber;
            ViewData["displayAsGroup"] = displayAsGroup; 
            LoanExpenseCategoryModel loanExpenseModel = ClientLoan.GetDetailLoanData(IDBContext.Current.UserName, operationNumber, loanNumber);
            loanExpenseModel.Loan.LoanStage = masterData.GetMasterDataNameByCode("LOAN_STATUS", loanExpenseModel.Loan.LoanStage, IDBContext.Current.CurrentLanguage);

            ViewBag.LoanStatusList = masterData.GetMasterDataModels("LOAN_STATUS"); //LOAN_STATUS
            var PartialEligibility = ClientLoan.GetMinElegibilityDateByLoans(operationNumber);
            ViewBag.PartialEligibility = string.Empty;
            if (PartialEligibility.HasValue)
            {
                ViewBag.PartialEligibility = string.Format("{0:dd MMM yyyy}", PartialEligibility.Value);
            }

            return View(loanExpenseModel);
        }

        public virtual ActionResult ExpenseCategories(string loanNumber, string operationNumber)
        {
            LoanExpenseCategoryModel model = ClientLoan.GetDetailLoanData(IDBContext.Current.UserName, operationNumber, loanNumber);
            return Json(model.ExpenseCategories, JsonRequestBehavior.AllowGet);
        }
    }
}
