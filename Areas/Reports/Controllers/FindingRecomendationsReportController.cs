using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.FindingRecomendationsReport;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.FindingAndRecomendations;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.Search;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Models.Search;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class FindingRecomendationsReportController : BaseController
    {
        #region Repositories

        IReportsGenericRepository _ClientGenericRepository = null;
        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _ClientGenericRepository; }
            set { _ClientGenericRepository = value; }
        }

        private IPmrCycleModelRepository _ClientPMRForCycleModel = null;
        public IPmrCycleModelRepository ClientPMRForCycleModel
        {
            get { return _ClientPMRForCycleModel; }
            set { _ClientPMRForCycleModel = value; }
        }

        IDelayAchievementModelRepository _ClientDelayAchievementModelRepository = null;
        public IDelayAchievementModelRepository ClientDelayAchievementModelRepository
        {
            get { return _ClientDelayAchievementModelRepository; }
            set { _ClientDelayAchievementModelRepository = value; }
        }

        IFindingRecommendationModelRepository _ClientFindingRecommendationModelRepository = null;
        public IFindingRecommendationModelRepository ClientFindingRecommendationModelRepository
        {
            get { return _ClientFindingRecommendationModelRepository; }
            set { _ClientFindingRecommendationModelRepository = value; }
        }

        private IConvergenceMasterDataModelRepository _MasterDataModel = null;
        public IConvergenceMasterDataModelRepository ClientMasterDataModelRepository
        {
            get { return _MasterDataModel; }
            set { _MasterDataModel = value; }
        }

        private ISearchModelRepository _SearchModel = null;

        public virtual ISearchModelRepository SearchModel
        {
            get { return _SearchModel; }
            set { _SearchModel = value; }
        }

        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult Index()
        {
            FindingRecomendationsReportModel fRReportModel = new FindingRecomendationsReportModel();
            var ListPMRCycles = ClientPMRForCycleModel.GetAllCyclesSeptemberAndMarchCycles(Lang).OrderByDescending(x => x.PmrCycleId).ToList();
            var ListContry = ClientGenericRepository.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListContryDepartment = ClientGenericRepository.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
            var ListSectorDepartment = ClientGenericRepository.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
            var ListDivision = ClientGenericRepository.GetDivisions(Lang).OrderBy(x => x.Name).ToList();
            var ListDelayTypes = _ClientDelayAchievementModelRepository.GetTypesDelay(Lang).OrderBy(x => x.Name).ToList();
            var ListClassification = ClientMasterDataModelRepository.GetMasterDataModels("PMI_CLASSIFICATION").OrderBy(x => x.Name).ToList();
            var ListDraftClassification = ClientMasterDataModelRepository.GetMasterDataModels("PMI_CLASSIFICATION").OrderBy(x => x.Name).ToList();
            ListDraftClassification[0].ConvergenceMasterDataId = 1;
            ListDraftClassification[1].ConvergenceMasterDataId = 2;
            ListDraftClassification[2].ConvergenceMasterDataId = 3;
            ListDraftClassification[3].ConvergenceMasterDataId = 4;

            ViewBag.ListPMRCycle = new SelectList(ListPMRCycles, "PMRCycleId", "PmrCycleName");
            ViewBag.ListContry = new MultiSelectList(ListContry, "ConvergenceMasterDataId", "Name");
            ViewBag.ListContryDepartment = new MultiSelectList(ListContryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDelayTyeps = new MultiSelectList(ListDelayTypes, "ConvergenceMasterDataId", "Name");
            ViewBag.ListClassification = new MultiSelectList(ListClassification, "ConvergenceMasterDataId", "NameEn");
            ViewBag.ListDraftClassification = new MultiSelectList(ListDraftClassification, "ConvergenceMasterDataId", "NameEn");

            return View();
        }

        public virtual ActionResult IndexReport()
        {
            FindingRecomendationsReportModel fRReportModel = new FindingRecomendationsReportModel();
            var ListPMRCycles = ClientPMRForCycleModel.GetAllCyclesSeptemberAndMarchCycles(Lang).OrderByDescending(x => x.PmrCycleId).ToList();
            var ListContry = ClientGenericRepository.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListContryDepartment = ClientGenericRepository.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
            var ListSectorDepartment = ClientGenericRepository.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
            var ListDivision = ClientGenericRepository.GetDivisions(Lang).OrderBy(x => x.Name).ToList();
            var ListStage = _ClientFindingRecommendationModelRepository.GetStages(Lang).OrderBy(x => x.Name).ToList();
            var ListClassification = ClientMasterDataModelRepository.GetMasterDataModels("PMI_CLASSIFICATION").OrderBy(x => x.Name).ToList();
            var ListDimesion = _ClientFindingRecommendationModelRepository.GetDimensions(Lang).OrderBy(x => x.Name).ToList();            
            var contIndices = 1;
            foreach (var itemDimension in ListDimesion)
            {
                itemDimension.Name = contIndices.ToString() + ". " + itemDimension.Name;
                itemDimension.ConvergenceMasterDataId = Convert.ToInt32(itemDimension.ConvergenceMasterDataId.ToString() + contIndices.ToString());
                contIndices++;
            }

            var ListCategories = _ClientFindingRecommendationModelRepository.GetCategories(Lang).OrderBy(x => x.Name).ToList();
            var ListDraftClassification = ClientMasterDataModelRepository.GetMasterDataModels("PMI_CLASSIFICATION").OrderBy(x => x.Name).ToList();           

            ListDraftClassification[0].ConvergenceMasterDataId = 1;
            ListDraftClassification[1].ConvergenceMasterDataId = 2;
            ListDraftClassification[2].ConvergenceMasterDataId = 3;
            ListDraftClassification[3].ConvergenceMasterDataId = 4;

            ViewBag.ListPMRCycle = new SelectList(ListPMRCycles, "PMRCycleId", "PmrCycleName");
            ViewBag.ListContry = new MultiSelectList(ListContry, "ConvergenceMasterDataId", "Name");
            ViewBag.ListContryDepartment = new MultiSelectList(ListContryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "ConvergenceMasterDataId", "Name");
            ViewBag.ListStage = new MultiSelectList(ListStage, "ConvergenceMasterDataId", "Name");
            ViewBag.ListClassification = new MultiSelectList(ListClassification, "ConvergenceMasterDataId", "NameEn");
            ViewBag.ListDimesion = new MultiSelectList(ListDimesion, "ConvergenceMasterDataId", "Name");
            ViewBag.ListCategories = new MultiSelectList(ListCategories, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDraftClassification = new MultiSelectList(ListDraftClassification, "ConvergenceMasterDataId", "NameEn");

            return View();
        }

        public virtual JsonResult FRAggregateFilterCountrys(FindingRecomendationsReportModel ModelFRReport)
        {
            var ListCountrys = ClientGenericRepository.GetCountriesFilter(Lang, ModelFRReport.CountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult FRAggregateFilterDivision(FindingRecomendationsReportModel ModelFRReport)
        {
            var ListDivision = ClientGenericRepository.GetDivisionFilter(Lang, ModelFRReport.SectorDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        public virtual JsonResult FRAggregateFilterCategory(FindingRecomendationsReportModel ModelFRReport)
        {
            var ListCategories = _ClientFindingRecommendationModelRepository.GetCategoriesFilterWithList(Lang, ModelFRReport.Dimension).OrderBy(x => x.Name).ToList();            
            return new JsonResult() { Data = ListCategories };
        }

        public virtual ActionResult CreateReportFAndR(FindingRecomendationsReportModel ModelFRReport)
        {
            string URLFRReport = string.Empty;
            string Header = ReportBuilder.GetReportHeader();

            URLFRReport += ReportBuilder.GetReportPreffix("ParamForFAndRReport");

            int operationID = 0;

            if (ModelFRReport.OperationNumber != null)
            {
                operationID = ClientGenericRepository.GetOperationIDForOperationNumber(ModelFRReport.OperationNumber);
            }
            else
            {
                operationID = -1;
            }

            if (Lang != null)
            {
                URLFRReport += "&LANG=" + Lang;
            }
            else
            {
                URLFRReport += "&LANG=EN";
            }

            if (operationID != -1)
            {
                if (operationID == 0)
                {
                    return Content(Localization.GetText("Invalid or nonexistent operation number"));
                }
                else
                {
                    URLFRReport += "&OPERATION_ID=" + operationID;
                }
            }
            else
            {
                URLFRReport += "&OPERATION_ID=-1";
            }

            if (ModelFRReport.PMRCycleId != 0)
            {
                URLFRReport += "&PMR_CYCLE_ID=" + ModelFRReport.PMRCycleId;
            }
            else
            {
                URLFRReport += "&PMR_CYCLE_ID=-1";
            }

            if (ModelFRReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDeparment in ModelFRReport.CountryDepartment)
                {
                    URLFRReport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDeparment;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelFRReport.Country)
                {
                    URLFRReport += "&COUNTRY_ID=" + itemCountry;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_ID=-1";
            }

            if (ModelFRReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelFRReport.SectorDepartment)
                {
                    URLFRReport += "&SECTOR_DEPARTMENT_ID=" + itemSectorDepartment;
                }
            }
            else
            {
                URLFRReport += "&SECTOR_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelFRReport.Division)
                {
                    URLFRReport += "&DIVISION_ID=" + itemDivision;
                }
            }
            else
            {
                URLFRReport += "&DIVISION_ID=-1";
            }

            if (ModelFRReport.Stage.Count > 0)
            {
                foreach (var itemStage in ModelFRReport.Stage)
                {
                    URLFRReport += "&PMI_STAGE_ID=" + itemStage;
                }
            }
            else
            {
                URLFRReport += "&PMI_STAGE_ID=-1";
            }

            if (ModelFRReport.Dimension.Count > 0)
            {
                /* CON-1570 */
                string itemToStr = string.Empty;                
                var codItem = 0;

                foreach (var itemDimension in ModelFRReport.Dimension)
                {
                    itemToStr = itemDimension.ToString();
                    codItem = Convert.ToInt32(itemToStr.Substring(0, itemToStr.Length - 1));

                    URLFRReport += "&DIMENSION_ID=" + codItem;
                }
            }
            else
            {
                URLFRReport += "&DIMENSION_ID=-1";
            }

            if (ModelFRReport.Categories.Count > 0)
            {
                foreach (var itemCategories in ModelFRReport.Categories)
                {
                    URLFRReport += "&CATEGORY_ID=" + itemCategories;
                }
            }
            else
            {
                URLFRReport += "&CATEGORY_ID=-1";
            }

            if (ModelFRReport.DraftClassification.Count > 0)
            {
                foreach (var itemDraftClassification in ModelFRReport.DraftClassification)
                {
                    URLFRReport += "&DRAFT_CLASS_ID=" + itemDraftClassification;
                }
            }
            else
            {
                URLFRReport += "&DRAFT_CLASS_ID=-1";
            }

            if (ModelFRReport.ValidatedClassification.Count > 0)
            {
                foreach (var itemValidatedClassification in ModelFRReport.ValidatedClassification)
                {
                    URLFRReport += "&VALUE_CLASS_ID=" + itemValidatedClassification;
                }
            }
            else
            {
                URLFRReport += "&VALUE_CLASS_ID=-1";
            }

            return Content(URLFRReport + Header);
        }

        public virtual ActionResult ExportReportFAndR(FindingRecomendationsReportModel ModelFRReport)
        {
            string URLFRReport = string.Empty;

            URLFRReport += ReportBuilder.GetReportPreffix("ParamForFAndRReport");

            int operationID = 0;

            if (ModelFRReport.OperationNumber != null)
            {
                operationID = ClientGenericRepository.GetOperationIDForOperationNumber(ModelFRReport.OperationNumber);
            }
            else
            {
                operationID = -1;
            }

            if (Lang != null)
            {
                URLFRReport += "&LANG=" + Lang;
            }
            else
            {
                URLFRReport += "&LANG=EN";
            }

            if (operationID != -1)
            {
                if (operationID == 0)
                {
                    return Content(Localization.GetText("Invalid or nonexistent operation number"));
                }
                else
                {
                    URLFRReport += "&OPERATION_ID=" + operationID;
                }
            }
            else
            {
                URLFRReport += "&OPERATION_ID=-1";
            }

            if (ModelFRReport.PMRCycleId != 0)
            {
                URLFRReport += "&PMR_CYCLE_ID=" + ModelFRReport.PMRCycleId;
            }
            else
            {
                URLFRReport += "&PMR_CYCLE_ID=-1";
            }

            if (ModelFRReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDeparment in ModelFRReport.CountryDepartment)
                {
                    URLFRReport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDeparment;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelFRReport.Country)
                {
                    URLFRReport += "&COUNTRY_ID=" + itemCountry;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_ID=-1";
            }

            if (ModelFRReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelFRReport.SectorDepartment)
                {
                    URLFRReport += "&SECTOR_DEPARTMENT_ID=" + itemSectorDepartment;
                }
            }
            else
            {
                URLFRReport += "&SECTOR_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelFRReport.Division)
                {
                    URLFRReport += "&DIVISION_ID=" + itemDivision;
                }
            }
            else
            {
                URLFRReport += "&DIVISION_ID=-1";
            }

            if (ModelFRReport.Stage.Count > 0)
            {
                foreach (var itemStage in ModelFRReport.Stage)
                {
                    URLFRReport += "&PMI_STAGE_ID=" + itemStage;
                }
            }
            else
            {
                URLFRReport += "&PMI_STAGE_ID=-1";
            }

            if (ModelFRReport.Dimension.Count > 0)
            {
                /* CON-1570 */
                string itemToStr = string.Empty;
                var codItem = 0;

                foreach (var itemDimension in ModelFRReport.Dimension)
                {
                    itemToStr = itemDimension.ToString();
                    codItem = Convert.ToInt32(itemToStr.Substring(0, itemToStr.Length - 1));
                    URLFRReport += "&DIMENSION_ID=" + codItem;
                }
            }
            else
            {
                URLFRReport += "&DIMENSION_ID=-1";
            }

            if (ModelFRReport.Categories.Count > 0)
            {
                foreach (var itemCategories in ModelFRReport.Categories)
                {
                    URLFRReport += "&CATEGORY_ID=" + itemCategories;
                }
            }
            else
            {
                URLFRReport += "&CATEGORY_ID=-1";
            }

            if (ModelFRReport.DraftClassification.Count > 0)
            {
                foreach (var itemDraftClassification in ModelFRReport.DraftClassification)
                {
                    URLFRReport += "&DRAFT_CLASS_ID=" + itemDraftClassification;
                }
            }
            else
            {
                URLFRReport += "&DRAFT_CLASS_ID=-1";
            }

            if (ModelFRReport.ValidatedClassification.Count > 0)
            {
                foreach (var itemValidatedClassification in ModelFRReport.ValidatedClassification)
                {
                    URLFRReport += "&VALUE_CLASS_ID=" + itemValidatedClassification;
                }
            }
            else
            {
                URLFRReport += "&VALUE_CLASS_ID=-1";
            }

            URLFRReport += "&rs:Format=" + ModelFRReport.ExportType;
            return Content(URLFRReport);
        }

        public virtual ActionResult CreateFRReport(FindingRecomendationsReportModel ModelFRReport)
        {
            string URLFRReport = string.Empty;
            string Header = ReportBuilder.GetReportHeader();

            URLFRReport += ReportBuilder.GetReportPreffix("ParamForArchivementDelayReport");
            int operationID = 0;

            if (ModelFRReport.OperationNumber != null)
            {
                operationID = ClientGenericRepository.GetOperationIDForOperationNumber(ModelFRReport.OperationNumber);
            }
            else
            {
                operationID = -1;
            }

            if (Lang != null)
            {
                URLFRReport += "&LANG=" + Lang;
            }
            else
            {
                URLFRReport += "&LANG=EN";
            }

            if (operationID != -1)
            {
                if (operationID == 0)
                {
                    return Content(Localization.GetText("Invalid or nonexistent operation number"));
                }
                else
                {
                    URLFRReport += "&OPERATION_ID=" + operationID;
                }
            }
            else
            {
                URLFRReport += "&OPERATION_ID=-1";
            }

            if (ModelFRReport.PMRCycleId != 0)
            {
                URLFRReport += "&PMR_CYCLE_ID=" + ModelFRReport.PMRCycleId;
            }
            else
            {
                URLFRReport += "&PMR_CYCLE_ID=-1";
            }

            if (ModelFRReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDeparment in ModelFRReport.CountryDepartment)
                {
                    URLFRReport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDeparment;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelFRReport.Country)
                {
                    URLFRReport += "&COUNTRY_ID=" + itemCountry;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_ID=-1";
            }

            if (ModelFRReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelFRReport.SectorDepartment)
                {
                    URLFRReport += "&SECTOR_DEPARTMENT_ID=" + itemSectorDepartment;
                }
            }
            else
            {
                URLFRReport += "&SECTOR_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelFRReport.Division)
                {
                    URLFRReport += "&DIVISION_ID=" + itemDivision;
                }
            }
            else
            {
                URLFRReport += "&DIVISION_ID=-1";
            }

            if (ModelFRReport.DelayType.Count > 0)
            {
                foreach (var itemDelayType in ModelFRReport.DelayType)
                {
                    URLFRReport += "&DELAY_TYPE_ID=" + itemDelayType;
                }
            }
            else
            {
                URLFRReport += "&DELAY_TYPE_ID=-1";
            }

            URLFRReport += "&ISSUE_SOLVED=" + ModelFRReport.IsIssueSolved;

            if (ModelFRReport.DraftClassification.Count > 0)
            {
                foreach (var itemDraftClassification in ModelFRReport.DraftClassification)
                {
                    URLFRReport += "&DRAFT_CLASS_ID=" + itemDraftClassification;
                }
            }
            else
            {
                URLFRReport += "&DRAFT_CLASS_ID=-1";
            }

            if (ModelFRReport.ValidatedClassification.Count > 0)
            {
                foreach (var itemValidatedClassification in ModelFRReport.ValidatedClassification)
                {
                    URLFRReport += "&VALUE_CLASS_ID=" + itemValidatedClassification;
                }
            }
            else
            {
                URLFRReport += "&VALUE_CLASS_ID=-1";
            }

            return Content(URLFRReport + Header);
        }

        public virtual ActionResult ExportFRReport(FindingRecomendationsReportModel ModelFRReport)
        {
            string URLFRReport = string.Empty;

            URLFRReport += ReportBuilder.GetReportPreffix("ParamForArchivementDelayReport");

            int operationID = 0;

            if (ModelFRReport.OperationNumber != null)
            {
                operationID = ClientGenericRepository.GetOperationIDForOperationNumber(ModelFRReport.OperationNumber);
            }
            else
            {
                operationID = -1;
            }

            if (Lang != null)
            {
                URLFRReport += "&LANG=" + Lang;
            }
            else
            {
                URLFRReport += "&LANG=EN";
            }

            if (operationID != -1)
            {
                if (operationID == 0)
                {
                    return Content(Localization.GetText("Invalid or nonexistent operation number"));
                }
                else
                {
                    URLFRReport += "&OPERATION_ID=" + operationID;
                }
            }
            else
            {
                URLFRReport += "&OPERATION_ID=-1";
            }

            if (ModelFRReport.PMRCycleId != 0)
            {
                URLFRReport += "&PMR_CYCLE_ID=" + ModelFRReport.PMRCycleId;
            }
            else
            {
                URLFRReport += "&PMR_CYCLE_ID=-1";
            }

            if (ModelFRReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDeparment in ModelFRReport.CountryDepartment)
                {
                    URLFRReport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDeparment;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelFRReport.Country)
                {
                    URLFRReport += "&COUNTRY_ID=" + itemCountry;
                }
            }
            else
            {
                URLFRReport += "&COUNTRY_ID=-1";
            }

            if (ModelFRReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelFRReport.SectorDepartment)
                {
                    URLFRReport += "&SECTOR_DEPARTMENT_ID=" + itemSectorDepartment;
                }
            }
            else
            {
                URLFRReport += "&SECTOR_DEPARTMENT_ID=-1";
            }

            if (ModelFRReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelFRReport.Division)
                {
                    URLFRReport += "&DIVISION_ID=" + itemDivision;
                }
            }
            else
            {
                URLFRReport += "&DIVISION_ID=-1";
            }

            if (ModelFRReport.DelayType.Count > 0)
            {
                foreach (var itemDelayType in ModelFRReport.DelayType)
                {
                    URLFRReport += "&DELAY_TYPE_ID=" + itemDelayType;
                }
            }
            else
            {
                URLFRReport += "&DELAY_TYPE_ID=-1";
            }

            URLFRReport += "&ISSUE_SOLVED=" + ModelFRReport.IsIssueSolved;

            if (ModelFRReport.DraftClassification.Count > 0)
            {
                foreach (var itemDraftClassification in ModelFRReport.DraftClassification)
                {
                    URLFRReport += "&DRAFT_CLASS_ID=" + itemDraftClassification;
                }
            }
            else
            {
                URLFRReport += "&DRAFT_CLASS_ID=-1";
            }

            if (ModelFRReport.ValidatedClassification.Count > 0)
            {
                foreach (var itemValidatedClassification in ModelFRReport.ValidatedClassification)
                {
                    URLFRReport += "&VALUE_CLASS_ID=" + itemValidatedClassification;
                }
            }
            else
            {
                URLFRReport += "&VALUE_CLASS_ID=-1";
            }

            URLFRReport += "&rs:Format=" + ModelFRReport.ExportType;
            return Content(URLFRReport);
        }

        public virtual JsonResult GetSearchResults(string numberOperation)
        {
            List<ResultModel> resultModelList;
            try
            {
                resultModelList = SearchModel.QueryResult(
                    null, numberOperation, null, null, null, null, null, null, null, null, null, null, null);
            }
            catch (Exception)
            {
                throw;
            }

            var jsonResult = Json(resultModelList, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}
