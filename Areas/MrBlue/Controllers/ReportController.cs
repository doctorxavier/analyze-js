using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.Messages.Reports;
using IDB.MW.Application.MrBlueModule.Services.Reports;
using IDB.MW.Application.MrBlueModule.ViewModels.Reports;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class ReportController : BaseController
    {
        #region Constants

        private const int _aproval = 6468;
        private const int _notApproval = 6469;
        private const string _approved = "Approved";
        private const string _notApproved = "Not Approved";
        private const string _REGIONAL = "REGIONAL";
        #endregion

        #region Fields

        private readonly IReportingService _reportingService;

        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Constructors

        public ReportController(
            IReportingService reportingService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService)
            : base(authorizationService, enumMappingService)
        {
            _reportingService = reportingService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _catalogService = catalogService;
        }

        #endregion

        #region View

        //ReportsCustomReport
        public virtual ActionResult ReportsCustomReport(string operationNumber)
        {
            var model = GetReportsCustomReportViewModel(operationNumber);

            //SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission, ActionEnum.ESGSpecialistWritePermission);
            SetViewBagRoles(operationNumber);

            return View(model);
        }

        public virtual ReportsCustomReportViewModel GetReportsCustomReportViewModel(string operationNumber)
        {
            var customReportViewModel = new ReportsCustomReportViewModel();
            SetViewBagReportsCustomReport();
            return customReportViewModel;
        }

        public virtual FileResult CustomReportDownload(string operationNumber, ReportsCustomReportViewModel viewmodel, OutputFormatEnum formatType)
        {
            var columnDelimiter = Request.Form["ColumnDelimiter"];
            if (columnDelimiter == "T")
            {
                columnDelimiter = "\t";
            }

            bool regional;
            viewmodel.Country = IsRegional(viewmodel.Country, out regional);

            var response = _reportingService.ExportCustomReportToFile(viewmodel, formatType, columnDelimiter, regional);
            Logger.GetLogger().WriteDebug("Termino Customreport", response.IsValid.ToString() + response.ErrorMessage);
            if (!response.IsValid)
            {
                return null;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            if (formatType == OutputFormatEnum.Excel)
            {
                return File(response.File, FileContentTypeEnum.Csv.GetEnumDescription(), "Custom_Report_" + MiFecha + ".csv");
            }
            else if (formatType == OutputFormatEnum.PDF)
            {
                return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), "Custom_Report_" + MiFecha + ".pdf");
            }
            else
            {
                return null;
            }
        }

        //ReportsSafeguardReport
        public virtual ActionResult ReportsSafeguardReport(string operationNumber)
        {
            var model = GetReportsSafeguardReportViewModel(operationNumber);

            //SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission, ActionEnum.ESGSpecialistWritePermission);
            SetViewBagRoles(operationNumber);

            return View(model);
        }

        public virtual ReportsSafeguardReportViewModel GetReportsSafeguardReportViewModel(string operationNumber)
        {
            var safeguardReportViewModel = new ReportsSafeguardReportViewModel();
            SetViewBagReportsSafeguardReport();
            return safeguardReportViewModel;
        }

        public virtual FileResult SafeguardReportDownload(string operationNumber, ReportsSafeguardReportViewModel viewmodel, OutputFormatEnum formatType)
        {
            var idYears = Request.Form["Year"];
            var columnDelimiter = Request.Form["ColumnDelimiter"];
            if (columnDelimiter == "T")
            {
                columnDelimiter = "\t";
            }

            var response = _reportingService.ExportSafeguardReportToFile(operationNumber, idYears, formatType, columnDelimiter, viewmodel.Status);

            if (!response.IsValid)
            {
                return null;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            if (formatType == OutputFormatEnum.Excel)
            {
                return File(response.File, FileContentTypeEnum.Csv.GetEnumDescription(), "Implementation_Of_Safeguards_" + MiFecha + ".csv");
            }

            if (formatType == OutputFormatEnum.PDF)
            {
                return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), "Implementation_Of_Safeguards_" + MiFecha + ".pdf");
            }

            return null;
        }

        public virtual FileResult ChangeTrackingReportDownload(string operationNumber, ReportsChangeTrackingReportViewModel viewmodel, OutputFormatEnum formatType)
        {
            var columnDelimiter = Request.Form["ColumnDelimiter"];
            if (columnDelimiter == "T")
            {
                columnDelimiter = "\t";
            }

            var response = _reportingService.ExportChangeTrackingToFile(viewmodel, formatType, columnDelimiter);

            if (!response.IsValid)
            {
                ViewBag.ErrorMessage = response.ErrorMessage;
                return null;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            if (formatType == OutputFormatEnum.Excel)
            {
                return File(response.File, FileContentTypeEnum.Csv.GetEnumDescription(), "Change_Tracking_" + MiFecha + ".csv");
            }

            if (formatType == OutputFormatEnum.PDF)
            {
                return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), "Change_Tracking_" + MiFecha + ".pdf");
            }

            return null;
        }

        //ReportsESGReport
        public virtual ActionResult ReportsESGReport(string operationNumber)
        {
            var model = GetReportsESGReportViewModel(operationNumber);
            SetViewBagRoles(operationNumber);

            return View(model);
        }

        public virtual ReportsESGReportViewModel GetReportsESGReportViewModel(string operationNumber)
        {
            var esgReportViewModel = new ReportsESGReportViewModel();
            SetViewBagReportsESGReport();
            return esgReportViewModel;
        }

        public virtual FileResult ESGReportDownload(string operationNumber, ReportsESGReportViewModel viewmodel, OutputFormatEnum formatType)
        {
            bool regional;
            var columnDelimiter = Request.Form["ColumnDelimiter"];
            if (columnDelimiter == "T")
            {
                columnDelimiter = "\t";
            }

            viewmodel.Country = IsRegional(viewmodel.Country, out regional);
            var response = _reportingService.ExportESGReportToFile(viewmodel, OutputFormatEnum.Excel, columnDelimiter, regional);
            if (!response.IsValid)
            {
                return null;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            if (formatType == OutputFormatEnum.Excel)
            {
                return File(response.File, FileContentTypeEnum.Csv.GetEnumDescription(), "ESG_Report_" + MiFecha + ".csv");
            }
            else if (formatType == OutputFormatEnum.PDF)
            {
                return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), "ESG_Report_" + MiFecha + ".pdf");
            }
            else
            {
                return null;
            }
        }

        //ReportsESGDownloads
        public virtual ActionResult ReportsESGDownloads(string operationNumber)
        {
            var model = GetReportsESGDownloadsViewModel(operationNumber);
            SetViewBagRoles(operationNumber);

            return View(model);
        }

        public virtual FileResult ReportsESGDownloadFile(int reportId)
        {
            var response = new ReportFileResponse();
            var columnDelimiter = Request.Form["ColumnDelimiter"];
            var operationStatus = Request.Form["operationStatus"];
            int status = 0;
            int.TryParse(operationStatus, out status);
            if (columnDelimiter == "T")
            {
                columnDelimiter = "\t";
            }

            var nameDocument = "Report";
            switch (reportId)
            {
                case 0: nameDocument = "All_Components_Indicator_";
                    break;
                case 1: nameDocument = "Policy_Directives_";
                    break;
                case 2: nameDocument = "Operations_by_Document_";
                    break;
                case 3: nameDocument = "Operation_Photos_";
                    break;
                case 4: nameDocument = "Implementation_Safeguards_";
                    break;
                case 5: nameDocument = "Lending_Report_";
                    break;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            response = _reportingService.ESGDownloadExcelFile(reportId, columnDelimiter, status);
            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, FileContentTypeEnum.Csv.GetEnumDescription(), nameDocument + MiFecha + ".csv");
        }

        public virtual ReportsDownloadsReportViewModel GetReportsESGDownloadsViewModel(string operationNumber)
        {
            var downloadsReportViewModel = new ReportsDownloadsReportViewModel();
            SetViewBagReportsESGDownloads();
            return downloadsReportViewModel;
        }

        public virtual FileResult ESGDownloadsDownload(string operationNumber, ReportsDownloadsReportViewModel viewmodel, OutputFormatEnum formatType)
        {
            var response = _reportingService.ExportDownloadsReportToFile(viewmodel, formatType);

            if (!response.IsValid)
            {
                return null;
            }

            var OnDate = System.DateTime.Now.ToString("yyyyMMdd");
            var OnHour = System.DateTime.Now.ToString("HHmm");
            var MiFecha = OnDate + "_" + OnHour;

            return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), "High_Risk_Report_" + MiFecha + ".pdf");
        }

        //ReportsChangeTrackingReport
        public virtual ActionResult ReportsChangeTracking()
        {
            SetViewBagChangeTrackingReport();
            return View();
        }

        public virtual JsonResult UserAD(string filter)
        {
            UserADResponse response = new UserADResponse();

            response = _reportingService.GetUserAdList(filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion

        #region Private Methods

        private void SetViewBagReportsCustomReport()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.Country, "CountryList" },
                { ConvergenceMasterDataTypeEnum.ORGANIZATION_UNIT, "listDivision" },
                { ConvergenceMasterDataTypeEnum.CountryDepartment, "CountryDepartmentList" },
                { ConvergenceMasterDataTypeEnum.OperationStatus, "OperationStatusList" },
                { ConvergenceMasterDataTypeEnum.OperationType, "OperationTypeList" }, 
                { ConvergenceMasterDataTypeEnum.SafeguardPerfomance, "SafeguardPerfomanceList" },
                { ConvergenceMasterDataTypeEnum.EsgGroup, "EsgGroupList" },
                { ConvergenceMasterDataTypeEnum.DisasterRiskCategory, "DisasterRiskCategoryList" },
                { ConvergenceMasterDataTypeEnum.EnvironmentalCategory, "EnvironmentalCategoryList" },
                { ConvergenceMasterDataTypeEnum.Department, "listDepartment" },
                { ConvergenceMasterDataTypeEnum.HighRisk, "listHighRiskRating" }
            });

            ColumnDilimiter();
            Years();
        }

        private void SetViewBagReportsSafeguardReport()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.OperationStatus, "OperationStatusList" },
            });
            Years();
            ColumnDilimiter();
        }

        private void SetViewBagReportsESGReport()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.Country, "CountryList" },
                { ConvergenceMasterDataTypeEnum.ORGANIZATION_UNIT, "DivisionList" },
                { ConvergenceMasterDataTypeEnum.CountryDepartment, "CountryDepartmentList" },
                { ConvergenceMasterDataTypeEnum.OperationStatus, "OperationStatusList" },
                { ConvergenceMasterDataTypeEnum.DisasterRiskCategory, "DisasterRiskCategoryList" },
                { ConvergenceMasterDataTypeEnum.Department, "listDepartment" },
                { ConvergenceMasterDataTypeEnum.EnvironmentalCategory, "EnvironmentalCategoryList" },
                { ConvergenceMasterDataTypeEnum.SafeguardPerfomance, "SafeguardPerfomanceList" },
                { ConvergenceMasterDataTypeEnum.AprovalCategory, "listApprovalStatus" }
            });
            ColumnDilimiter();
            List<SelectListItem> listAproval = ViewBag.listApprovalStatus;
            foreach (var aproval in listAproval)
            {
                if (aproval.Value.Equals(_aproval.ToString()))
                {
                    aproval.Text = _approved;
                }
                else if (aproval.Value.Equals(_notApproval.ToString()))
                {
                    aproval.Text = _notApproved;
                }
            }

            ViewBag.listApprovalStatus = listAproval;
        }

        private void SetViewBagReportsESGDownloads()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.OperationStatus, "OperationStatusList" }
            });
            ColumnDilimiter();
        }

        private void SetViewBagReportMock()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.CountryFP, "CountryList" },
            });

            //Country Department
            var listCountryDepartment = new List<SelectListItem>();
            inicializeList(listCountryDepartment, "Andean Group", "1");
            inicializeList(listCountryDepartment, "Caribean Group", "2");
            inicializeList(listCountryDepartment, "Country Departament - Haiti", "3");
            inicializeList(listCountryDepartment, "Isthmus & DR", "4");
            inicializeList(listCountryDepartment, "Southern Cone", "5");

            //Department
            var listDepartment = new List<SelectListItem>();
            inicializeList(listDepartment, "IFD", "1");
            inicializeList(listDepartment, "INE", "2");
            inicializeList(listDepartment, "INT", "3");
            inicializeList(listDepartment, "SCL", "4");

            //Division
            var listDivision = new List<SelectListItem>();
            inicializeList(listDivision, "IFD/CMF", "1");
            inicializeList(listDivision, "IFD/CTI", "2");
            inicializeList(listDivision, "IFD/FMM", "3");
            inicializeList(listDivision, "IFD/ICS", "4");

            //Status
            var listStatus = new List<SelectListItem>();
            inicializeList(listStatus, "Active", "1");
            inicializeList(listStatus, "Cancelled", "2");
            inicializeList(listStatus, "On-Hold", "3");
            inicializeList(listStatus, "Closed", "4");
            inicializeList(listStatus, "Contact Negotiation", "5");

            //ApprovalStatus
            var listApprovalStatus = new List<SelectListItem>();
            inicializeList(listApprovalStatus, "Approved", "6468");
            inicializeList(listApprovalStatus, "Not Approved", "6469");

            //ApprovalStatus
            var listType = new List<SelectListItem>();
            inicializeList(listType, "Activities", "1");
            inicializeList(listType, "Capital Projects", "2");
            inicializeList(listType, "Corporate Input Product", "3");
            inicializeList(listType, "Deliverables", "4");
            inicializeList(listType, "Loan Operation", "5");

            //Year
            var listYear = new List<SelectListItem>();
            inicializeList(listYear, "2016", "11");
            inicializeList(listYear, "2015", "1");
            inicializeList(listYear, "2014", "2");
            inicializeList(listYear, "2013", "3");
            inicializeList(listYear, "2012", "4");
            inicializeList(listYear, "2011", "5");
            inicializeList(listYear, "2010", "6");
            inicializeList(listYear, "2009", "7");
            inicializeList(listYear, "2008", "8");
            inicializeList(listYear, "2007", "9");
            inicializeList(listYear, "2006", "10");

            //Environmental and Social Category
            var listEnviromentalSocialCategory = new List<SelectListItem>();
            inicializeList(listEnviromentalSocialCategory, "A", "1");
            inicializeList(listEnviromentalSocialCategory, "B", "2");
            inicializeList(listEnviromentalSocialCategory, "C", "3");
            inicializeList(listEnviromentalSocialCategory, "B.13", "4");
            inicializeList(listEnviromentalSocialCategory, "A(R)", "5");

            //Safeguard Performance
            var listSafeguarPerfom = new List<SelectListItem>();
            inicializeList(listSafeguarPerfom, "Satisfactory", "1");
            inicializeList(listSafeguarPerfom, "Partially Satisfactory", "2");
            inicializeList(listSafeguarPerfom, "Partially Unsatisfactory", "3");
            inicializeList(listSafeguarPerfom, "Unsatisfactory", "4");

            //Disaster Risk Category
            var listDisasterRiskCategory = new List<SelectListItem>();
            inicializeList(listDisasterRiskCategory, "Low", "1");
            inicializeList(listDisasterRiskCategory, "Moderate", "2");
            inicializeList(listDisasterRiskCategory, "High", "3");

            //ESG Operational Group
            var listESGOperationalGroup = new List<SelectListItem>();
            inicializeList(listESGOperationalGroup, "Urban", "1");
            inicializeList(listESGOperationalGroup, "Transport", "2");
            inicializeList(listESGOperationalGroup, "FIs", "3");
            inicializeList(listESGOperationalGroup, "Energy/Extractives", "4");
            inicializeList(listESGOperationalGroup, "Social", "5");

            //High Risk Rating
            var listHighRiskRating = new List<SelectListItem>();
            inicializeList(listHighRiskRating, "High Risk", "1");
            inicializeList(listHighRiskRating, "Not Higth Risk", "2");

            //Policies Triggered
            var listPoliciesTriggered = new List<SelectListItem>();
            inicializeList(listPoliciesTriggered, "Yes", "Yes");
            inicializeList(listPoliciesTriggered, "No", "No");
            inicializeList(listPoliciesTriggered, "Undefined", "Undefined");

            //Column Delimiter
            var columnDelimiterList = new List<SelectListItem>();
            inicializeList(columnDelimiterList, "Comma", ",");
            inicializeList(columnDelimiterList, "Semicolon", ";");
            inicializeList(columnDelimiterList, "Tab", "T");

            ViewBag.listCountryDepartment = listCountryDepartment;
            ViewBag.listDepartment = listDepartment;
            ViewBag.listDivision = listDivision;
            ViewBag.listStatus = listStatus;
            ViewBag.listApprovalStatus = listApprovalStatus;
            ViewBag.listType = listType;
            ViewBag.listYear = listYear;
            ViewBag.listEnviromentalSocialCategory = listEnviromentalSocialCategory;
            ViewBag.listSafeguarPerfom = listSafeguarPerfom;
            ViewBag.listDisasterRiskCategory = listDisasterRiskCategory;
            ViewBag.listESGOperationalGroup = listESGOperationalGroup;
            ViewBag.listHighRiskRating = listHighRiskRating;
            ViewBag.listPoliciesTriggered = listPoliciesTriggered;
            ViewBag.ColumnDelimiter = columnDelimiterList;
        }

        private void inicializeList(List<SelectListItem> list, string text, string value)
        {
            var item = new SelectListItem()
            {
                Value = value,
                Text = text
            };

            list.Add(item);
        }

        private void SetViewBagChangeTrackingReport()
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
               { ConvergenceMasterDataTypeEnum.OperationStatus, "OperationStatusList" }
            });
            var reports = _reportingService.GetTypeReportTracking();
            var list = reports.ReportType.Select(report => new SelectListItem()
             {
                 Text = report.NameEn,
                 Value = report.ConvergenceMasterDataId.ToString()
             }).ToList();

            ViewBag.ReportTypeList = list;
            var modificationTypeList = new List<SelectListItem>();

            modificationTypeList.Add(
                new SelectListItem()
                {
                    Text = "Complete",
                    Value = "complete"
                });
            modificationTypeList.Add(
                new SelectListItem()
                {
                    Text = "Modify",
                    Value = "modify"
                });
            modificationTypeList.Add(
                new SelectListItem()
                {
                    Text = "Add",
                    Value = "insert"
                });

            modificationTypeList.Add(
                new SelectListItem()
                {
                    Text = "Remove",
                    Value = "remove"
                });

            ViewBag.ModificationTypeList = modificationTypeList;
            ColumnDilimiter();
        }

        private void ColumnDilimiter()
        {
            var columnDelimiterList = new List<SelectListItem>();
            inicializeList(columnDelimiterList, "Comma", ",");
            inicializeList(columnDelimiterList, "Semicolon", ";");
            inicializeList(columnDelimiterList, "Tab", "T");
            ViewBag.ColumnDelimiter = columnDelimiterList;

            var listPoliciesTriggered = new List<SelectListItem>();
            inicializeList(listPoliciesTriggered, "Yes", "Yes");
            inicializeList(listPoliciesTriggered, "No", "No");
            inicializeList(listPoliciesTriggered, "{Not Set}", "{Not Set}");
            ViewBag.listPoliciesTriggered = listPoliciesTriggered;
        }

        private void Years()
        {
            int maxYear = _catalogService.GetMaxYear();
            var listYear = new List<SelectListItem>();
            var contador = 11;
            for (var i = maxYear; i > maxYear - 11; i--)
            {
                inicializeList(listYear, i.ToString(), i.ToString());
                contador--;
            }

            ViewBag.listYear = listYear;
        }

        private string[] IsRegional(string[] country, out bool regional)
        {
            regional = false;
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.Country, "CountryList" }
            });
            string elementRegional = string.Empty;
            if (country != null)
            {
                foreach (string t in country)
                {
                    List<SelectListItem> listCountry = ViewBag.CountryList;
                    var countryFind = listCountry.Find(r => r.Value == t).Text;
                    if (countryFind != null && countryFind.ToUpper() == _REGIONAL)
                    {
                        elementRegional = t;
                        regional = true;
                    }
                }

                if (elementRegional.Length > 0)
                {
                    country = country.Where(c => c != elementRegional).ToArray();
                    if (country.Length == 0)
                    {
                        country = null;
                    }
                }
            }

            return country;
        }
        #endregion
    }
}