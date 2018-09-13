using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.Search;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Models.Search;
using IDB.Presentation.MVC4.Areas.Search.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Models.Search;
using IDB.MW.Domain.Contracts.DomainServices;

namespace IDB.Presentation.MVC4.Areas.Search.Controllers
{
    public partial class SearchController : BaseController
    {
        // GET: /Search/Search/
        private IConvergenceMasterDataModelRepository _MasterDataModel = null;

        public virtual IConvergenceMasterDataModelRepository MasterDataModel
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

        public enum SelectedFilter
        {
            none = 0,
            contractAndFinantial,
            environmentalAndRisk,
            resultsMatrix,
            supervision,
            visualization
        }

        public virtual ActionResult Index(StatisticsFilterModel filter = null)
        {
            filter = filter ?? new StatisticsFilterModel();

            ViewBag.UserName = filter.UserName;

            ViewBag.Filter = filter;

            ViewBag.CountryDepartmentList = SearchModel.GetCountryDeparmentList();

            ViewBag.SectorDepartmentList = SearchModel.GetSectorDeparmentList();

            ViewBag.DivisionList = SearchModel.GetDivisionBySectorDeparment(null);

            ViewBag.CountryList = SearchModel.GetCountryByCountryDeparment(null);

            ViewBag.OperationTypeList = MasterDataModel.GetMasterDataModels("OPERATION TYPE");
            ((List<ConvergenceMasterDataModel>)ViewBag.OperationTypeList).Insert(0, this.CreateSelectDataModel("Operation Type"));

            ViewBag.OverallStageList = MasterDataModel.GetMasterDataModels("OVERALL_STAGE");
            ((List<ConvergenceMasterDataModel>)ViewBag.OverallStageList).Insert(0, this.CreateSelectDataModel("Overall Stage"));

            var FundList = MasterDataModel.GetMasterDataModels("FUND");
            ViewBag.FundList = MasterDataModel.GetMasterDataModels("FUND");
            ((List<ConvergenceMasterDataModel>)ViewBag.FundList).Insert(0, this.CreateSelectDataModel("Fund"));

            ViewBag.SelectedFilter = SelectedFilter.none;

            var model = new SearchQueryModel();
            model.UserName = filter.UserName;
            model.ClauseStatus = filter.ClauseStatus;
            return View(model);
        }

        public virtual ActionResult ContractAndFinantial()
        {
            ViewBag.SelectedFilter = SelectedFilter.contractAndFinantial;
            return View();
        }

        public virtual ActionResult EnvironmentalAndRisk()
        {
            ViewBag.SelectedFilter = SelectedFilter.environmentalAndRisk;
            return View();
        }

        public virtual ActionResult ResultsMatrix()
        {
            ViewBag.SelectedFilter = SelectedFilter.resultsMatrix;

            ViewBag.ResultsMatrixFlags = this.GetResultsMatrixFlags();
            ((List<ConvergenceMasterDataModel>)ViewBag.ResultsMatrixFlags).Insert(0, this.CreateSelectDataModel("Flag"));

            ViewBag.ResultsMatrixPMRRequired = this.GetYesNoOptions();
            ((List<ConvergenceMasterDataModel>)ViewBag.ResultsMatrixPMRRequired).Insert(0, this.CreateSelectDataModel("PMR Required"));

            ViewBag.ResultsMatrixValidationStage = MasterDataModel.GetMasterDataModels("VALIDATION_STAGE");
            ViewBag.ResultsMatrixValidationStage = ((List<ConvergenceMasterDataModel>)ViewBag.ResultsMatrixValidationStage).Where(model => model.Code.StartsWith("PMI")).ToList();
            ((List<ConvergenceMasterDataModel>)ViewBag.ResultsMatrixValidationStage).Insert(0, this.CreateSelectDataModel("Validation Stage"));

            return View();
        }

        public virtual ActionResult Supervision()
        {
            ViewBag.SelectedFilter = SelectedFilter.supervision;
            return View();
        }

        public virtual ActionResult Visualization()
        {
            ViewBag.SelectedFilter = SelectedFilter.visualization;
            return View();
        }

        public virtual ActionResult Country(string value)
        {
            ViewBag.CountryList = SearchModel.GetCountryByCountryDeparment(value);
            return View();
        }

        public virtual ActionResult Division(string value)
        {
            ViewBag.DivisionList = SearchModel.GetDivisionBySectorDeparment(value);
            return View();
        }

        public virtual ActionResult QueryResult(
            string user_name,
            string keyword,
            string deparmentSectorCode,
            string divisionCode,
            string countryDeparmentCode,
            string countryCode,
            string operationTypeCode,
            string overallStageCode,
            string fundCode,
            string teamMember,
            string approvalYear,
            string dateFrom,
            string dateTo)
        {
            DateTime? _dateFrom = null;
            DateTime? _dateTo = null;
            try
            {
                if (!string.IsNullOrEmpty(dateFrom))
                {
                    _dateFrom = Convert.ToDateTime(dateFrom);
                }

                if (!string.IsNullOrEmpty(dateTo))
                {
                    _dateTo = Convert.ToDateTime(dateTo);
                }
            }
            catch (Exception)
            {
                _dateFrom = null;
                _dateTo = null;
            }

            ViewBag.SharePointAddress = Globals.GetSetting("BasePath");

            try
            {
                List<ResultModel> resultModelList = SearchModel.QueryResult(
                    user_name,
                    keyword,
                    deparmentSectorCode,
                    divisionCode,
                    countryDeparmentCode,
                    countryCode,
                    operationTypeCode,
                    overallStageCode,
                    fundCode,
                    teamMember,
                    approvalYear,
                    _dateFrom,
                    _dateTo);

                return View(resultModelList);
            }
            catch (Exception)
            {
                return PartialView("ErrorResult");
            }
        }

        [HttpPost]
        public virtual JsonResult GetSearchResults(CustomFilterModel searchFilter = null)
        {
            DateTime? _dateFrom = null;
            DateTime? _dateTo = null;
            try
            {
                if (!string.IsNullOrEmpty(searchFilter.dateFrom))
                {
                    _dateFrom = Convert.ToDateTime(searchFilter.dateFrom);
                }

                if (!string.IsNullOrEmpty(searchFilter.dateTo))
                {
                    _dateTo = Convert.ToDateTime(searchFilter.dateTo);
                }
            }
            catch (Exception)
            {
                _dateFrom = null;
                _dateTo = null;
            }

            ViewBag.SharePointAddress = Globals.GetSetting("BasePath");

            try
            {
                List<ResultModel> resultModelList = SearchModel.QueryResult(
                    searchFilter.user_name,
                    searchFilter.keyword,
                    searchFilter.deparmentSectorCode,
                    searchFilter.divisionCode,
                    searchFilter.countryDeparmentCode,
                    searchFilter.countryCode,
                    searchFilter.operationTypeCode,
                    searchFilter.overallStageCode,
                    searchFilter.fundCode,
                    searchFilter.teamMember,
                    searchFilter.approvalYear,
                    _dateFrom,
                    _dateTo);

                return Json(resultModelList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ConvergenceMasterDataModel CreateSelectDataModel(string section)
        {
            var objConvergenceMasterDataModel = new ConvergenceMasterDataModel();
            objConvergenceMasterDataModel.Code = string.Empty;
            objConvergenceMasterDataModel.NameEn = "Select " + section;
            objConvergenceMasterDataModel.NameEs = "Seleccione " + section;
            objConvergenceMasterDataModel.NameFr = "Votre choix " + section;
            objConvergenceMasterDataModel.NamePt = "Seleccione " + section;
            return objConvergenceMasterDataModel;
        }

        private List<ConvergenceMasterDataModel> GetResultsMatrixFlags()
        {
            var resultsMatrixFlagsList = new List<ConvergenceMasterDataModel>();

            var objConvergenceMasterDataModel = new ConvergenceMasterDataModel();
            objConvergenceMasterDataModel.Code = "Pro-Gender";
            objConvergenceMasterDataModel.NameEn = "Pro-Gender";
            objConvergenceMasterDataModel.NameEs = "Pro-Gender";
            objConvergenceMasterDataModel.NameFr = "Pro-Gender";
            objConvergenceMasterDataModel.NamePt = "Pro-Gender";
            resultsMatrixFlagsList.Add(objConvergenceMasterDataModel);

            var objConvergenceMasterDataModel1 = new ConvergenceMasterDataModel();
            objConvergenceMasterDataModel1.Code = "Pro-Etnicity";
            objConvergenceMasterDataModel1.NameEn = "Pro-Etnicity";
            objConvergenceMasterDataModel1.NameEs = "Pro-Etnicity";
            objConvergenceMasterDataModel1.NameFr = "Pro-Etnicity";
            objConvergenceMasterDataModel1.NamePt = "Pro-Etnicity";
            resultsMatrixFlagsList.Add(objConvergenceMasterDataModel1);

            return resultsMatrixFlagsList;
        }

        private List<ConvergenceMasterDataModel> GetYesNoOptions()
        {
            var resultsMatrixFlagsList = new List<ConvergenceMasterDataModel>();

            var objConvergenceMasterDataModel = new ConvergenceMasterDataModel();
            objConvergenceMasterDataModel.Code = "true";
            objConvergenceMasterDataModel.NameEn = "Yes";
            objConvergenceMasterDataModel.NameEs = "Si";
            objConvergenceMasterDataModel.NameFr = "Oui";
            objConvergenceMasterDataModel.NamePt = "Sim";
            resultsMatrixFlagsList.Add(objConvergenceMasterDataModel);

            var objConvergenceMasterDataModel1 = new ConvergenceMasterDataModel();
            objConvergenceMasterDataModel1.Code = "false";
            objConvergenceMasterDataModel1.NameEn = "No";
            objConvergenceMasterDataModel1.NameEs = "No";
            objConvergenceMasterDataModel1.NameFr = "Aucun";
            objConvergenceMasterDataModel1.NamePt = "Nao";
            resultsMatrixFlagsList.Add(objConvergenceMasterDataModel1);

            return resultsMatrixFlagsList;
        }
    }
}
