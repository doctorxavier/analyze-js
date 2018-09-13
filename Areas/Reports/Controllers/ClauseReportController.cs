using System.Configuration;
using System;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.ClauseReport;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class ClauseReportController : BaseController
    {
        #region Repositories

        IReportsGenericRepository _ClientGenericRepositoty = null;
        public IReportsGenericRepository ClientGenericRepositoty
        {
            get { return _ClientGenericRepositoty; }
            set { _ClientGenericRepositoty = value; }
        } 

        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult Index(string OperationNumber)
        {
            ClauseSearchReport ModelClauseReport = new ClauseSearchReport();
            ModelClauseReport.OperationID = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);
            var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseReport.OperationID).ToList();
            var ListClauseStatus = ClientGenericRepositoty.GetClauseStatus(Lang).OrderBy(x => x.Name).ToList();
            var ClauseCategory = ClientGenericRepositoty.GetClauseCategories(Lang, OperationNumber).OrderBy(x => x.Name).ToList();
            var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
            var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();

            ViewBag.ListContractNumber = new MultiSelectList(ContractNumber, "ContractId", "ContractNumber");
            ViewBag.ListClauseStatus = new MultiSelectList(ListClauseStatus, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseCategory = new MultiSelectList(ClauseCategory, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseType = new MultiSelectList(ClauseType, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseLocation = new MultiSelectList(ClauseLocation, "ConvergenceMasterDataId", "Name");

            return View(ModelClauseReport);
        }

        public virtual ActionResult ClauseReportCreate(ClauseSearchReport ModelClauseReport)
        {
            string URLClauseReport = string.Empty;
            
            string Header = ReportBuilder.GetReportHeader();

            URLClauseReport += ReportBuilder.GetReportPreffix("ParamForClauseReport");

            if (Lang != null)
            {
                URLClauseReport += "&LANG=" + Lang;
            }
            else
            {
                URLClauseReport += "&LANG=EN";
            }

            string ClauseUrl = string.Empty;
            
            if (ModelClauseReport.OperationID == 0)
            {   
                return Content(Localization.GetText("Invalid or nonexistent operation number"));
            }
            else
            {
                ClauseUrl += "&OPERATION=" + ModelClauseReport.OperationID; 
            }

            ClauseUrl += "&OnlyDeleted=" + ModelClauseReport.deletedClausesOnly; 

            if (ModelClauseReport.ClauseNumber != null)
            {
                if (ModelClauseReport.ClauseNumber.Trim().Length > 0)
                {
                    ClauseUrl += "&ClauseNumber=" + ModelClauseReport.ClauseNumber.Trim();   
                }
            }

            if (ModelClauseReport.ContractNumber.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ContractNumber.Count; x++)
                {
                    ClauseUrl += "&Contract=" + ModelClauseReport.ContractNumber[x];
                }
            }
            else
            {
                var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseReport.OperationID).ToList();

                foreach (var itemContracNumber in ContractNumber)
                {
                    ClauseUrl += "&Contract=" + itemContracNumber.ContractId;
                }
            }

            if (ModelClauseReport.ClauseCategory.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseCategory.Count; x++)
                {
                    ClauseUrl += "&ClauseCategory=" + ModelClauseReport.ClauseCategory[x];
                }
            }
            else
            {   
                var ClauseCategory = ClientGenericRepositoty.GetClauseCategory(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseCategory in ClauseCategory)
                {
                    ClauseUrl += "&ClauseCategory=" + itemClauseCategory.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseLocation.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseLocation.Count; x++)
                {
                    ClauseUrl += "&ClauseLocation=" + ModelClauseReport.ClauseLocation[x];
                }
            }
            else
            {   
                var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();
                
                foreach (var itemClauseLocation in ClauseLocation)
                {
                    ClauseUrl += "&ClauseLocation=" + itemClauseLocation.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseStatus.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseStatus.Count; x++)
                {
                    ClauseUrl += "&ClauseStatus=" + ModelClauseReport.ClauseStatus[x];
                }
            }
            else
            {
                var ListClauseStatus = ClientGenericRepositoty.GetClauseStatus(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseStatus in ListClauseStatus)
                {
                    ClauseUrl += "&ClauseStatus=" + itemClauseStatus.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseType.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseType.Count; x++)
                {
                    ClauseUrl += "&ClauseType=" + ModelClauseReport.ClauseType[x];
                }
            }
            else
            {
                var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseType in ClauseType)
                {
                    ClauseUrl += "&ClauseType=" + itemClauseType.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.CurrentExpirationDateFrom != null)
            {
                DateTime DateFrom = new DateTime();
                DateFrom = Convert.ToDateTime(ModelClauseReport.CurrentExpirationDateFrom);
                ClauseUrl += "&ExpDateFrom=" + DateFrom.ToString("MM/dd/yyyy");
            }

            if (ModelClauseReport.CurrentExpirationDateTo != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseReport.CurrentExpirationDateTo);
                ClauseUrl += "&ExpDateto=" + DateTo.ToString("MM/dd/yyyy"); 
            }

            return Content(URLClauseReport + ClauseUrl + Header);
        }

        public virtual ActionResult ExportClauseReport(ClauseSearchReport ModelClauseReport)
        {
            string URLClauseReport = string.Empty;

            URLClauseReport += ReportBuilder.GetReportPreffix("ParamForClauseReport");

            if (Lang != null)
            {
                URLClauseReport += "&LANG=" + Lang;
            }
            else
            {
                URLClauseReport += "&LANG=EN";
            }

            string ClauseUrl = string.Empty;

            if (ModelClauseReport.OperationID == 0)
            {
                return Content(Localization.GetText("Invalid or nonexistent operation number"));
            }
            else
            {
                ClauseUrl += "&OPERATION=" + ModelClauseReport.OperationID;
            }

            ClauseUrl += "&OnlyDeleted=" + ModelClauseReport.deletedClausesOnly;

            if (ModelClauseReport.ClauseNumber != null)
            {
                if (ModelClauseReport.ClauseNumber.Trim().Length > 0)
                {
                    ClauseUrl += "&ClauseNumber=" + ModelClauseReport.ClauseNumber.Trim();
                }
            }

            if (ModelClauseReport.ContractNumber.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ContractNumber.Count; x++)
                {
                    ClauseUrl += "&Contract=" + ModelClauseReport.ContractNumber[x];
                }
            }
            else
            {
                var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseReport.OperationID).ToList();   
                foreach (var itemContracNumber in ContractNumber)
                {
                    ClauseUrl += "&Contract=" + itemContracNumber.ContractId;
                }
            }

            if (ModelClauseReport.ClauseCategory.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseCategory.Count; x++)
                {
                    ClauseUrl += "&ClauseCategory=" + ModelClauseReport.ClauseCategory[x];
                }
            }
            else
            {
                var ClauseCategory = ClientGenericRepositoty.GetClauseCategory(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseCategory in ClauseCategory)
                {
                    ClauseUrl += "&ClauseCategory=" + itemClauseCategory.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseLocation.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseLocation.Count; x++)
                {
                    ClauseUrl += "&ClauseLocation=" + ModelClauseReport.ClauseLocation[x];
                }
            }
            else
            {
                var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();

                foreach (var itemClauseLocation in ClauseLocation)
                {
                    ClauseUrl += "&ClauseLocation=" + itemClauseLocation.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseStatus.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseStatus.Count; x++)
                {
                    ClauseUrl += "&ClauseStatus=" + ModelClauseReport.ClauseStatus[x];
                }
            }
            else
            {
                var ListClauseStatus = ClientGenericRepositoty.GetClauseStatus(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseStatus in ListClauseStatus)
                {
                    ClauseUrl += "&ClauseStatus=" + itemClauseStatus.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseType.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseType.Count; x++)
                {
                    ClauseUrl += "&ClauseType=" + ModelClauseReport.ClauseType[x];
                }
            }
            else
            {
                var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseType in ClauseType)
                {
                    ClauseUrl += "&ClauseType=" + itemClauseType.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.CurrentExpirationDateFrom != null)
            {
                DateTime DateFrom = new DateTime();
                DateFrom = Convert.ToDateTime(ModelClauseReport.CurrentExpirationDateFrom);
                ClauseUrl += "&ExpDateFrom=" + DateFrom.ToString("dd/MM/yyyy");
            }

            if (ModelClauseReport.CurrentExpirationDateTo != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseReport.CurrentExpirationDateTo);
                ClauseUrl += "&ExpDateto=" + DateTo.ToString("dd/MM/yyyy");
            }

            ClauseUrl += "&rs:Format=" + ModelClauseReport.ExportType;

            return Content(URLClauseReport + ClauseUrl);
        }

        public virtual ContentResult ReturnMessageError(string Message)
        {
            return Content(Message); 
        }
    }
}
