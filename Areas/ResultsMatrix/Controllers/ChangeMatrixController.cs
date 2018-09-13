using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;

using IDB.Architecture.Cache;
using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.ChangesMatrix;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.ChangesMatrix;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Domain.Session;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Values;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class ChangeMatrixController : BaseController
    {
        private readonly IDEMLockModulesService _demLockModulesService;
        private readonly IOperationRepository _operationRepository;

        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheLongTime"].ToString());
        
        private IResultsMatrixChangesModelRepository _clientResultsMatrixChanges = null;
        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;

        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        private IReportsGenericRepository _ClientGenericRepository = null;
        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _ClientGenericRepository; }
            set { _ClientGenericRepository = value; }
        }

        public ChangeMatrixController(IResultsMatrixChangesModelRepository clientResultsMatrixChanges,
        IOperationRepository operationRepository,
        IDEMLockModulesService demLockModulesService)
        {
            _clientResultsMatrixChanges = clientResultsMatrixChanges;
            _operationRepository = operationRepository;
            _demLockModulesService = demLockModulesService;
        }

        [ExceptionHandling]
        public virtual ActionResult Index(
            string operationNumber,
            string cycleIds = null,
            string sectionIds = null,
            string changeIds = null,
            string subChangeIds = null,
            bool editView = false)
        {
            var operation = _operationRepository.GetOne(o => o.OperationNumber == operationNumber, o => o.ResultsMatrices);

            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
            ClientGenericRepository, PCRWorkflowStatusRepository, operation.OperationId);

            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            ViewBag.ModelIndicators = new LinkPredefinesIndicatorViewModel { Name = string.Empty };
            ResultsMatrixModel searchResults = null;

            if (string.IsNullOrEmpty(cycleIds))
            {
                searchResults = _clientResultsMatrixChanges.GetResultsMatrixModel(new OperationModel()
                {
                    OperationNumber = operationNumber,
                    AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR)
                }, operation);

                if (editView)
                {
                    return View("Edit", searchResults);
                }

                searchResults.DEMLockReviewProcessData = _demLockModulesService
                  .BuildLockReviewProcessDEMDataModel(operationNumber);

                return View(searchResults);
            }

            List<int> selectedCycles = new List<int>();
            selectedCycles =
                cycleIds.Split(',').Select(x => int.Parse(x)).ToList<int>();
            List<int> selectedSections = new List<int>();
            List<FilterItemModel> typesBySection = new List<FilterItemModel>();
            if (!string.IsNullOrEmpty(sectionIds))
            {
                selectedSections =
                    sectionIds.Split(',').Select(x => int.Parse(x)).ToList<int>();
                typesBySection = _clientResultsMatrixChanges
                    .GetMatrixChangeTypesBySectionId(
                        selectedSections,
                        IDBContext.Current.CurrentLanguage).ToList();
            }

            List<int> selectedChanges = new List<int>();
            List<FilterItemModel> subTypesByChange = new List<FilterItemModel>();
            if (!string.IsNullOrEmpty(changeIds))
            {
                selectedChanges =
                    changeIds.Split(',').Select(x => int.Parse(x)).ToList<int>();
                subTypesByChange = _clientResultsMatrixChanges
                    .GetMatrixChangeSubTypesByChangeTypeId(
                        selectedChanges,
                        IDBContext.Current.CurrentLanguage).ToList();
            }

            List<int> selectedSubChanges = new List<int>();
            if (!string.IsNullOrEmpty(subChangeIds))
            {
                selectedSubChanges =
                    subChangeIds.Split(',').Select(x => int.Parse(x)).ToList<int>();
            }

            searchResults = _clientResultsMatrixChanges.Search(new ResultsMatrixModel()
            {
                OperationNumber = operationNumber,
                PmrCycleIds = selectedCycles,
                SectionIds = selectedSections,
                TypeOfChangeIds = selectedChanges,
                SubTypeOfChangeIds = selectedSubChanges,
                AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR)
            }, operation);
            searchResults.PmrCycleIds = selectedCycles;
            searchResults.SectionIds = selectedSections;
            searchResults.TypeOfChangeIds = selectedChanges;
            searchResults.SubTypeOfChangeIds = selectedSubChanges;
            searchResults.TypeOfChanges = typesBySection;
            searchResults.SubtypeOfChanges = subTypesByChange;
            if (editView)
            {
                return View("Edit", searchResults);
            }

            searchResults.DEMLockReviewProcessData = _demLockModulesService
                  .BuildLockReviewProcessDEMDataModel(operationNumber);

            return View(searchResults);
        }

        // GET: /ResultsMatrix/ChangeMatrix/Edit
        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult Edit(
            string operationNumber,
            string cycleIds = null,
            string sectionIds = null,
            string changeIds = null,
            string subChangeIds = null)
        {
            return RedirectToAction(
                "Index",
                "ChangeMatrix",
                new
                {
                    area = "ResultsMatrix",
                    OperationNumber = operationNumber,
                    cycleIds = cycleIds,
                    sectionIds = sectionIds,
                    changeIds = changeIds,
                    subChangeIds = subChangeIds,
                    editView = true
                });
        }

        // GET: /ResultsMatrix/ChangeMatrix/Update
        [ExceptionHandling]
        [HttpPost]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult Update(ResultsMatrixModel resultModel)
        {
            _clientResultsMatrixChanges.Save(resultModel);
            return RedirectToAction("Index", "ChangeMatrix", new { operationNumber = IDBContext.Current.Operation });
        }

        // GET: /ResultsMatrix/ChangeMatrix/Delete
        [HttpGet]
        public virtual ActionResult Delete(int matrixChangeId)
        {
            if (IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                var matrixModel = _clientResultsMatrixChanges.FindMatrixChangeModel(matrixChangeId);
                _clientResultsMatrixChanges.Delete(matrixModel);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        // GET: /ResultsMatrix/ChangeMatrix/DeleteWarning
        [HttpGet]
        public virtual ActionResult DeleteWarning(int matrixChangeId)
        {
            ViewData["matrixChangeId"] = matrixChangeId;

            return PartialView();
        }

        [HttpPost]
        public virtual JsonResult GetMatrixChangeTypesBySectionId(IList<int> sectionIds)
        {
            var changeTypes = _clientResultsMatrixChanges
                .GetMatrixChangeTypesBySectionId(sectionIds, IDBContext.Current.CurrentLanguage);

            return Json(changeTypes);
        }

        [HttpPost]
        public virtual JsonResult GetMatrixChangeSubTypesByChangeTypeId(IList<int> changeTypeIds)
        {
            var changeSubTypes = _clientResultsMatrixChanges.GetMatrixChangeSubTypesByChangeTypeId(
                changeTypeIds,
                IDBContext.Current.CurrentLanguage);

            return Json(changeSubTypes);
        }

        [HttpGet]
        public virtual ActionResult Notification()
        {
            return PartialView("Notification");
        }
    }
}
