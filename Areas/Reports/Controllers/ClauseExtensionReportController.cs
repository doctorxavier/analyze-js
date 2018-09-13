using System;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.ClauseReport;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Values;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class ClauseExtensionReportController : BaseController
    {
        #region Repositories
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly ICatalogService _catalogService;

        IReportsGenericRepository _ClientGenericRepositoty = null;
        public IReportsGenericRepository ClientGenericRepositoty
        {
            get { return _ClientGenericRepositoty; }
            set { _ClientGenericRepositoty = value; }
        }

        public ClauseExtensionReportController(IAttributesEngineService attributesEngineService,
            ICatalogService catalogService)
        {
            _attributesEngineService = attributesEngineService;
            _catalogService = catalogService;
        }

        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult Index(string OperationNumber)
        {
            ClauseReport ModelClauseReport = new ClauseReport();
            ModelClauseReport.OperationID = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);

            var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseReport.OperationID).ToList();
            var ListClauseStatus = ClientGenericRepositoty.GetClauseStatus(Lang).OrderBy(x => x.Name).ToList();
            var ClauseCategory = ClientGenericRepositoty.GetClauseCategory(Lang).OrderBy(x => x.Name).ToList();
            var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
            var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();

            ViewBag.ListContractNumber = new MultiSelectList(ContractNumber, "ContractId", "ContractNumber");
            ViewBag.ListClauseStatus = new MultiSelectList(ListClauseStatus, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseCategory = new MultiSelectList(ClauseCategory, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseType = new MultiSelectList(ClauseType, "ConvergenceMasterDataId", "Name");
            ViewBag.ClauseLocation = new MultiSelectList(ClauseLocation, "ConvergenceMasterDataId", "Name");

            var opTypes = OperationTypeHelper.GetOperationTypes(ModelClauseReport.OperationID).First();
            ViewBag.isTCOperation = opTypes == OperationType.TCP;
            ViewBag.TypeBank = false;

            if (opTypes == OperationType.TCP)
            {
                var attributesExecuteBy = _attributesEngineService.GetAttributeValueByCode(OperationNumber, AttributeCode.EXECUTED_BY);
                if (attributesExecuteBy.IsValid && attributesExecuteBy.Value != null)
                {
                    var convergenceMasterCodeByIdResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(int.Parse(attributesExecuteBy.FormAttribute.InitialValue));
                    ViewBag.TypeBank = convergenceMasterCodeByIdResponse.Code == AttributeValue.BANK;
                }
            }

            return View(ModelClauseReport);
        }

        [HttpPost]
        public virtual ActionResult Index(ClauseReport ModelClauseReport)
        {
            string URLClauseHistoryReport = string.Empty;
            
            string Header = ReportBuilder.GetReportHeader();

            URLClauseHistoryReport += ReportBuilder.GetReportPreffix("ParamForClauseExtensionReport");

            if (Lang != null)
            {
                URLClauseHistoryReport += "&LANG=" + Lang;
            }
            else
            {
                URLClauseHistoryReport += "&LANG=EN";
            }

            string ClauseHistoryUrl = string.Empty;

            if (ModelClauseReport.OperationID == 0)
            {
                return Content(Localization.GetText("Invalid or nonexistent operation number"));
            }
            else
            {
                ClauseHistoryUrl += "&OPERATION=" + ModelClauseReport.OperationID;
            }

            if (ModelClauseReport.ClauseNumber != null)
            {
                if (ModelClauseReport.ClauseNumber.Trim().Length > 0)
                {
                    ClauseHistoryUrl += "&ClauseNumber=" + ModelClauseReport.ClauseNumber.Trim();
                }
            }

            if (ModelClauseReport.ContractNumber.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ContractNumber.Count; x++)
                {
                    ClauseHistoryUrl += "&Contract=" + ModelClauseReport.ContractNumber[x];
                }
            }
            else
            {
                var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseReport.OperationID).ToList();

                foreach (var itemContracNumber in ContractNumber)
                {
                    ClauseHistoryUrl += "&Contract=" + itemContracNumber.ContractId;
                }
            }

            if (ModelClauseReport.ClauseCategory.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseCategory.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseCategory=" + ModelClauseReport.ClauseCategory[x];
                }
            }
            else
            {
                var ClauseCategory = ClientGenericRepositoty.GetClauseCategory(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseCategory in ClauseCategory)
                {
                    ClauseHistoryUrl += "&ClauseCategory=" + itemClauseCategory.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseLocation.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseLocation.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseLocation=" + ModelClauseReport.ClauseLocation[x];
                }
            }
            else
            {
                var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();

                foreach (var itemClauseLocation in ClauseLocation)
                {
                    ClauseHistoryUrl += "&ClauseLocation=" + itemClauseLocation.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.ClauseType.Count > 0)
            {
                for (int x = 0; x < ModelClauseReport.ClauseType.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseType=" + ModelClauseReport.ClauseType[x];
                }
            }
            else
            {
                var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseType in ClauseType)
                {
                    ClauseHistoryUrl += "&ClauseType=" + itemClauseType.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseReport.CurrentExpirationDate != null)
            {
                DateTime DateFrom = new DateTime();
                DateFrom = Convert.ToDateTime(ModelClauseReport.CurrentExpirationDate);
                ClauseHistoryUrl += "&ExpDateFrom=" + DateFrom.ToString("MM/dd/yyyy");
            }

            if (ModelClauseReport.LastStatusChangeDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseReport.LastStatusChangeDate);
                ClauseHistoryUrl += "&ExpDateto=" + DateTo.ToString("MM/dd/yyyy");
            }

            if (ModelClauseReport.OriginalExpirationDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseReport.OriginalExpirationDate);
                ClauseHistoryUrl += "&AprDateFrom=" + DateTo.ToString("MM/dd/yyyy");
            }

            if (ModelClauseReport.SubmissionDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseReport.SubmissionDate);
                ClauseHistoryUrl += "&AprDateto=" + DateTo.ToString("MM/dd/yyyy");
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(ModelClauseReport.OperationID).First();
            ViewBag.isTCOperation = opTypes == OperationType.TCP;
            ViewBag.TypeBank = false;

            if (opTypes == OperationType.TCP)
            {
                var operationNumber = ClientGenericRepositoty.GetOperationNumberForOperationID(ModelClauseReport.OperationID);
                var attributesExecuteBy = _attributesEngineService.GetAttributeValueByCode(operationNumber, AttributeCode.EXECUTED_BY);
                if (attributesExecuteBy.IsValid && attributesExecuteBy.Value != null)
                {
                    var convergenceMasterCodeByIdResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(int.Parse(attributesExecuteBy.FormAttribute.InitialValue));
                    ViewBag.TypeBank = convergenceMasterCodeByIdResponse.Code == AttributeValue.BANK;
                }
            }

            if (!string.IsNullOrEmpty(ModelClauseReport.ExportType))
            {
                ClauseHistoryUrl += ViewBag.TypeBank;
                ClauseHistoryUrl += "&rs:Format=" + ModelClauseReport.ExportType;
            }

            return Content(URLClauseHistoryReport + ClauseHistoryUrl + Header);
        }

        public virtual ActionResult ExportClauseHistoryReport(ClauseReport ModelClauseHistoryReport)
        {
            string URLClauseReport = string.Empty;

            URLClauseReport += ReportBuilder.GetReportPreffix("ParamForClauseExtensionReport");

            if (Lang != null)
            {
                URLClauseReport += "&LANG=" + Lang;
            }
            else
            {
                URLClauseReport += "&LANG=EN";
            }

            string ClauseHistoryUrl = string.Empty;

            if (ModelClauseHistoryReport.OperationID == 0)
            {
                return Content(Localization.GetText("Invalid or nonexistent operation number"));
            }
            else
            {
                ClauseHistoryUrl += "&OPERATION=" + ModelClauseHistoryReport.OperationID;
            }

            if (ModelClauseHistoryReport.ClauseNumber != null)
            {
                if (ModelClauseHistoryReport.ClauseNumber.Trim().Length > 0)
                {
                    ClauseHistoryUrl += "&ClauseNumber=" + ModelClauseHistoryReport.ClauseNumber.Trim();
                }
            }

            if (ModelClauseHistoryReport.ContractNumber.Count > 0)
            {
                for (int x = 0; x < ModelClauseHistoryReport.ContractNumber.Count; x++)
                {
                    ClauseHistoryUrl += "&Contract=" + ModelClauseHistoryReport.ContractNumber[x];
                }
            }
            else
            {
                var ContractNumber = ClientGenericRepositoty.GetContractNumberForOperationId(ModelClauseHistoryReport.OperationID).ToList();

                foreach (var itemContracNumber in ContractNumber)
                {
                    ClauseHistoryUrl += "&Contract=" + itemContracNumber.ContractId;
                }
            }

            if (ModelClauseHistoryReport.ClauseCategory.Count > 0)
            {
                for (int x = 0; x < ModelClauseHistoryReport.ClauseCategory.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseCategory=" + ModelClauseHistoryReport.ClauseCategory[x];
                }
            }
            else
            {
                var ClauseCategory = ClientGenericRepositoty.GetClauseCategory(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseCategory in ClauseCategory)
                {
                    ClauseHistoryUrl += "&ClauseCategory=" + itemClauseCategory.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseHistoryReport.ClauseLocation.Count > 0)
            {
                for (int x = 0; x < ModelClauseHistoryReport.ClauseLocation.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseLocation=" + ModelClauseHistoryReport.ClauseLocation[x];
                }
            }
            else
            {
                var ClauseLocation = ClientGenericRepositoty.GetClauseLocation(Lang).OrderBy(x => x.Name).ToList();

                foreach (var itemClauseLocation in ClauseLocation)
                {
                    ClauseHistoryUrl += "&ClauseLocation=" + itemClauseLocation.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseHistoryReport.ClauseType.Count > 0)
            {
                for (int x = 0; x < ModelClauseHistoryReport.ClauseType.Count; x++)
                {
                    ClauseHistoryUrl += "&ClauseType=" + ModelClauseHistoryReport.ClauseType[x];
                }
            }
            else
            {
                var ClauseType = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemClauseType in ClauseType)
                {
                    ClauseHistoryUrl += "&ClauseType=" + itemClauseType.ConvergenceMasterDataId;
                }
            }

            if (ModelClauseHistoryReport.CurrentExpirationDate != null)
            {
                DateTime DateFrom = new DateTime();
                DateFrom = Convert.ToDateTime(ModelClauseHistoryReport.SubmissionDate);
                ClauseHistoryUrl += "&ExpDateFrom=" + DateFrom.ToString("MM/dd/yyyy");
            }

            if (ModelClauseHistoryReport.LastStatusChangeDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseHistoryReport.LastStatusChangeDate);
                ClauseHistoryUrl += "&ExpDateto=" + DateTo.ToString("MM/dd/yyyy");
            }

            if (ModelClauseHistoryReport.OriginalExpirationDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseHistoryReport.OriginalExpirationDate);
                ClauseHistoryUrl += "&AprDateFrom=" + DateTo.ToString("MM/dd/yyyy");
            }

            if (ModelClauseHistoryReport.SubmissionDate != null)
            {
                DateTime DateTo = new DateTime();
                DateTo = Convert.ToDateTime(ModelClauseHistoryReport.CurrentExpirationDate);
                ClauseHistoryUrl += "&AprDateto=" + DateTo.ToString("MM/dd/yyyy");
            }

            var opTypes = OperationTypeHelper.GetOperationTypes(ModelClauseHistoryReport.OperationID).First();
            var isTCOperation = opTypes == OperationType.TCP;
            var TypeBank = false;

            if (opTypes == OperationType.TCP)
            {
                var operationNumber = ClientGenericRepositoty.GetOperationNumberForOperationID(ModelClauseHistoryReport.OperationID);
                var attributesExecuteBy = _attributesEngineService.GetAttributeValueByCode(operationNumber, AttributeCode.EXECUTED_BY);
                if (attributesExecuteBy.IsValid && attributesExecuteBy.Value != null)
                {
                    var convergenceMasterCodeByIdResponse = _catalogService.GetConvergenceMasterCodeByIdResponse(int.Parse(attributesExecuteBy.FormAttribute.InitialValue));
                    TypeBank = convergenceMasterCodeByIdResponse.Code == AttributeValue.BANK;
                }
            }

            ClauseHistoryUrl += "&rs:TypeBank=" + TypeBank;

            ClauseHistoryUrl += "&rs:Format=" + ModelClauseHistoryReport.ExportType;

            return Content(URLClauseReport + ClauseHistoryUrl);
        }
    }
}
