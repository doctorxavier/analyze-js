using System.Configuration;
using System.Text;
using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.OPUSModule.Services.FundOperationService;
using IDB.MW.Application.TCM.Services.FindingAndRecommendationService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.FindingAndRecommendation;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.FindingAndRecomendations;
using IDB.Architecture.Language;
using IDB.MW.Domain.Models.Architecture.FindingAndRecomendations;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.FindingRecomendations;
using IDB.Architecture.Logging;
using IDB.MW.Domain.Models.Security;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Domain.EntityHelpers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class FindingRecommendationController : BaseController
    {
        #region Constants
        public const string OUTCOME_DELAY = "OUTCDEL";
        public const string OUTPUT_DELAY = "OUTPDEL";
        public const string PAGE_PARTNER_CONSUL = "UI-FR-004Partial-TabPartnerConsultancies";
        public const string PAGE_STORIES_FIELD = "UI-FR-005Partial-TabStoriesFromTheField";
        public const string PAGE_INNOVATION = "UI-FR-003Partial-TabSustainabilityInnovation";

        public const string PAGES_TCM =
           "UI-FR-002Partial-TabProgress," +
           "UI-FR-003Partial-TabSustainabilityInnovation," +
           "UI-FR-004Partial-TabPartnerConsultancies," +
           "UI-FR-005Partial-TabStoriesFromTheField," +
           "UI-FR-006Partial-TabProjectManagement," +
           "UI-COM-004Partial-TabMappingProgress";
        #endregion

        #region Fields
        private readonly ICatalogService _catalogService;
        private readonly IFindingRecommendationService _findingRecomendationService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly IDelayAchievementModelRepository _delayAchievementRepository;
        private readonly IFindingRecommendationModelRepository _overallProjectManagementRepository;
        private readonly IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;
        private readonly IReportsGenericRepository _reportsGenericRepository;
        private readonly IDocumentManagementService _docManagementService;
        private readonly IFundOperationService _fundOperationService;
        #endregion

        #region Constructors
        public FindingRecommendationController(ICatalogService catalogService,
            IFindingRecommendationService findingRecomendationService,
            ITcmUniverseService tcmUniverseService,
            IDelayAchievementModelRepository delayAchievementRepository,
            IFindingRecommendationModelRepository OverallProjectManagementRepository,
            IPCRWorkflowStatusRepository pcrWorkflowStatusRepository,
            IReportsGenericRepository reportsGenericRepository,
            IDocumentManagementService docManagementService,
            IFundOperationService fundOperationService)
        {
            _catalogService = catalogService;
            _findingRecomendationService = findingRecomendationService;

            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService, _findingRecomendationService);
            _tcmUniverseService = tcmUniverseService;
            _delayAchievementRepository = delayAchievementRepository;
            _overallProjectManagementRepository = OverallProjectManagementRepository;
            _pcrWorkflowStatusRepository = pcrWorkflowStatusRepository;
            _reportsGenericRepository = reportsGenericRepository;
            _fundOperationService = fundOperationService;

            _docManagementService = docManagementService;
        }
        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            string lang = IDBContext.Current.CurrentLanguage.ToUpper();
            string textYes = Localization.GetText("Yes");
            string textNo = Localization.GetText("No");

            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid)
            {
                ViewBag.WarningMessage = rspnse.ErrorMessage;
                FindingRecommendationViewModel noMatter = null;
                return View(noMatter);
            }

            SetPermissionFindingsRecommendations();
            var model = _findingRecomendationService.GetFindingRecommendation(
                rspnse.OperationNumber, lang).GetFindingRecommendation;
            model.DelaysDetailsModel = new DelaysDetailsModel();
            model.DelaysDetailsModel.DelaysDetails = new System.Collections.Generic.List<DelaysDetails>();

            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);

            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteDebug("FindingRecommendationController", "Method: Index - cycloOperation: " + cycloOperation + ")");
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);
            ViewBag.TypePartner = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEPARTNER);
            ViewBag.ConsultanciesType = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPECONSULTANCIES);
            ViewBag.ConsultanciesNationality = _viewModelMapperHelper.GetListNationality(TCMGlobalValues.NATIONALITY);
            ViewBag.QualityAssesment = _catalogService.GetListMasterData(TCMGlobalValues.QUALITYASSESSMENT, orderByCodeAsc: true);
            ViewBag.TypeInnovation = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEINNOVATION);
            ViewBag.TypeStory = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPESTORY);
            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(rspnse.OperationNumber);

            int State = 0;

            var ResultOP = _delayAchievementRepository.GetListOutputs(operationNumber);
            var ResultOC = _delayAchievementRepository.GetListOutComes(operationNumber);

            Dictionary<int, string> ListOutputs = ResultOP.Where(data => data.Key != 0).ToDictionary(data => data.Key, data => data.Value);
            Dictionary<int, string> ListOutcomes = ResultOC.Where(data => data.Key != 0).ToDictionary(data => data.Key, data => data.Value);

            SetOverallProjectManagement(rspnse.OperationNumber, lang);
            model.DelaysAchievement = _delayAchievementRepository.GetDelays(rspnse.OperationNumber, lang, "0", "Name", "null", null);

            var ListCycles = _delayAchievementRepository.GetCycles(lang, rspnse.OperationNumber);
            ViewBag.IsCurrentCycleTableOne = true;
            ViewBag.IsCurrentCycleTableTwo = true;
            SetDelaysInAchievement(operationNumber, lang, ListCycles);

            var ListDelaysType = _delayAchievementRepository.GetTypesDelay(lang);
            ListDelaysType.Reverse();
            var ListItemDelaysType = new List<SelectListItem>();
            foreach (var item in ListDelaysType)
            {
                ListItemDelaysType.Add(new SelectListItem
                {
                    Value = item.ConvergenceMasterDataId.ToString(),
                    Text = item.Name
                });
            }

            string nameOutputDelay = ListDelaysType.FirstOrDefault(q => q.Code == OUTPUT_DELAY).Name;
            string nameOutcomeDelay = ListDelaysType.FirstOrDefault(q => q.Code == OUTCOME_DELAY).Name;

            ListCycles.Reverse();
            if (ListCycles.Count > 0)
            {
                ListCycles.RemoveAt(0);
                ListCycles.Reverse();

                ListCycles.Reverse();
            }

            var ListItemPMRCyclesEdit = new Dictionary<string, string>();
            foreach (var item in ListCycles)
            {
                ListItemPMRCyclesEdit.Add(item.ItemId.ToString(), item.Name);
            }

            foreach (var itemAchievementDelays in model.DelaysAchievement.AchievementDelays)
            {
                itemAchievementDelays.StateIsSolved = (itemAchievementDelays.IsSolved == true) ? textYes : textNo;
                ViewBag.IsCurrentCycleTableOne = itemAchievementDelays.IsCurrentCycle;
                itemAchievementDelays.ResultElements = new Dictionary<string, string>();
                itemAchievementDelays.CycleElements = ListItemPMRCyclesEdit;
                if (itemAchievementDelays.DelayType == nameOutcomeDelay)
                {
                    foreach (var item in ListOutcomes)
                    {
                        itemAchievementDelays.ResultElements.Add(item.Key.ToString(), item.Value);

                        if (itemAchievementDelays.ResultElementName == item.Value)
                        {
                            itemAchievementDelays.ResultElementId = item.Key.ToString();
                        }
                    }
                }
                else
                {
                    foreach (var item in ListOutputs)
                    {
                        itemAchievementDelays.ResultElements.Add(item.Key.ToString(), item.Value);

                        if (itemAchievementDelays.ResultElementName == item.Value)
                        {
                            itemAchievementDelays.ResultElementId = item.Key.ToString();
                        }
                    }
                }
            }

            foreach (var itemOtherDelays in model.DelaysAchievement.OtherDelays)
            {
                itemOtherDelays.StateIsSolved = (itemOtherDelays.IsSolved == true) ? textYes : textNo;
                ViewBag.IsCurrentCycleTableTwo = itemOtherDelays.IsCurrentCycle;
            }

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingDelays(State, false, 2);
                ViewData["message"] = message;
            }

            var ListItemsOutPuts = new List<SelectListItem>();
            foreach (var item in ListOutputs)
            {
                ListItemsOutPuts.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutputDelay).Value.ToString()
                    }
                });
            }

            var ListItems = new List<SelectListItem>();
            var ListItemsOutComes = new List<SelectListItem>();
            foreach (var item in ListOutcomes)
            {
                ListItemsOutComes.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutcomeDelay).Value.ToString()
                    }
                });
            }

            foreach (var items in ListItemsOutComes)
            {
                ListItems.Add(items);
            }

            foreach (var items2 in ListItemsOutPuts)
            {
                ListItems.Add(items2);
            }

            model.OverallProjectManagement = _overallProjectManagementRepository.GetFindingAdittional(operationNumber, lang, "0", "0", "0", null);
            model.OverallProjectManagement.FieldAccessList = model.FieldAccessList;
            model.Progress.FieldAccessList = model.FieldAccessList;
            model.SustainabilityAndInnovation.Innovation.ForEach(o => o.FieldAccessList = model.FieldAccessList);
            model.ParterAndConsultancies.FieldAccessList = model.FieldAccessList;
            model.StoriesFromTheField.FieldAccessList = model.FieldAccessList;
            model.ProjectManagement.FieldAccessList = model.FieldAccessList;
            model.OperationNumber = operationNumber;
            ViewBag.IsCurrentCycleTable = true;
            foreach (var AllValues in model.OverallProjectManagement.FindingRecommendations)
            {
                ViewBag.IsCurrentCycleTable = AllValues.IsCurrentCycle;
            }

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingOverallProjectManagement(State, false, 2);
                ViewData["message"] = message;
            }

            ViewBag.ListOutputs = ListItemsOutPuts;
            ViewBag.ListOutcomes = ListItemsOutComes;
            ViewBag.ListItems = ListItems;
            ViewBag.ListDelaysType = ListItemDelaysType;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.ParterAndConsultancies);
            ViewBag.SerializedProgress = PageSerializationHelper.SerializeObject(model.Progress);
            ViewBag.SerializableProjectManagement = PageSerializationHelper.SerializeObject(model.ProjectManagement);
            ViewBag.SerializedSustainabilityAndInnovationViewModel = PageSerializationHelper.SerializeObject(model.SustainabilityAndInnovation);
            ViewBag.SerializedStories = PageSerializationHelper.SerializeObject(model.StoriesFromTheField);
            ViewBag.SerializedDelaysAchievement = PageSerializationHelper.SerializeObject(model.DelaysAchievement);
            ViewBag.SerializedOverallProjectManagement = PageSerializationHelper.SerializeObject(model.OverallProjectManagement);
            ViewBag.IsEditable = model.OverallProjectManagement.IsEditable;

            var fundsData = _fundOperationService.GetFundsDataByOperation(operationNumber);
            var language = Language.EN;

            if (fundsData.Models.Any())
            {
                language = fundsData.Models.First().Language;
            }

            switch (language)
            {
                case Language.EN:
                    model.FieldsFillLanguage = "Common.Languaje.English";
                    break;

                case Language.ES:
                    model.FieldsFillLanguage = "Common.Languaje.Spanish";
                    break;

                case Language.PT:
                    model.FieldsFillLanguage = "Common.Languaje.Portuguese";
                    break;

                case Language.FR:
                    model.FieldsFillLanguage = "Common.Languaje.French";
                    break;
            }

            return View(model);
        }

        #region project Management

        public virtual ActionResult SearchFilterResult(string operationNumber, string tcReportingPeriod, string stage, string dimension)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = new ProjectManagementModel();
            SetPermissionFindingsRecommendations();
            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(rspnse.OperationNumber);
            model = _findingRecomendationService.GetTableRowProjectManagement(rspnse.OperationNumber, tcReportingPeriod, stage, dimension);
            if (model.TableProjectManagement.Count > 0)
            {
                var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
                model.TableProjectManagement.ForEach(o => o.FieldAccessList = fieldAccessList);
            }

            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);
            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(rspnse.OperationNumber);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);

            ViewBag.SerializableProjectManagement = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partial/ProjectManagementView", model);
        }

        public virtual ActionResult SearchFilterAllResult(string operationNumber, string tcReportingPeriod)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = new ProjectManagementModel();
            LockRegister(operationNumber, TCMGlobalValues.URL_PROJECT_MANAGEMENT);

            SetPermissionFindingsRecommendations();
            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(rspnse.OperationNumber);
            model = _findingRecomendationService.GetTableRowProjectManagement(rspnse.OperationNumber, tcReportingPeriod, "0", "0");
            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);
            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(rspnse.OperationNumber);
            ViewBag.SerializableProjectManagement = PageSerializationHelper.SerializeObject(model);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);

            return PartialView("Partial/ProjectManagementView", model);
        }

        public virtual ActionResult GetRowFindingRecommendation(string operationNumber)
        {
            SetPermissionFindingsRecommendations();
            var model = new ProjectManagementModel();
            var tableProjectManagement = new List<ProjectManagementTableModel>();
            var projectManagementTableModel = new ProjectManagementTableModel();
            projectManagementTableModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            tableProjectManagement.Add(projectManagementTableModel);
            model.TableProjectManagement = tableProjectManagement;
            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);
            ViewBag.SerializableProjectManagement = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/DataTables/ProjectManagementRowView", model.TableProjectManagement);
        }

        public virtual ActionResult GetRowDelays(string operationNumber)
        {
            SetPermissionFindingsRecommendations();
            var model = new AchievementDelayModel();

            string lang = IDBContext.Current.CurrentLanguage.ToUpper();
            var ListCycles = _delayAchievementRepository.GetCycles(lang, operationNumber);
            SetDelaysInAchievement(operationNumber, lang, ListCycles);

            Dictionary<int, string> ListOutputs = new Dictionary<int, string>();
            Dictionary<int, string> ListOutcomes = new Dictionary<int, string>();

            var ResultOP = _delayAchievementRepository.GetListOutputs(operationNumber);
            var ResultOC = _delayAchievementRepository.GetListOutComes(operationNumber);

            foreach (var data in ResultOP)
            {
                if (data.Key != 0)
                {
                    ListOutputs.Add(data.Key, data.Value);
                }
            }

            foreach (var data in ResultOC)
            {
                if (data.Key != 0)
                {
                    ListOutcomes.Add(data.Key, data.Value);
                }
            }

            var ListDelaysType = _delayAchievementRepository.GetTypesDelay(lang);
            ListDelaysType.Reverse();
            var ListItemDelaysType = new List<SelectListItem>();
            foreach (var item in ListDelaysType)
            {
                ListItemDelaysType.Add(new SelectListItem
                {
                    Value = item.ConvergenceMasterDataId.ToString(),
                    Text = item.Name
                });
            }

            string nameOutputDelay = ListDelaysType.FirstOrDefault(q => q.Code == OUTPUT_DELAY).Name;
            string nameOutcomeDelay = ListDelaysType.FirstOrDefault(q => q.Code == OUTCOME_DELAY).Name;

            var ListItemsOutPuts = new List<SelectListItem>();
            foreach (var item in ListOutputs)
            {
                ListItemsOutPuts.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutcomeDelay).Value.ToString()
                    }
                });
            }

            var ListItems = new List<SelectListItem>();
            var ListItemsOutComes = new List<SelectListItem>();
            foreach (var item in ListOutcomes)
            {
                ListItemsOutComes.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutcomeDelay).Value.ToString()
                    }
                });
            }

            foreach (var items in ListItemsOutComes)
            {
                ListItems.Add(items);
            }

            foreach (var items2 in ListItemsOutPuts)
            {
                ListItems.Add(items2);
            }

            ViewBag.ListOutputs = ListItemsOutPuts;
            ViewBag.ListOutcomes = ListItemsOutComes;
            ViewBag.ListDelaysType = ListItemDelaysType;
            ViewBag.ListItems = ListItems;

            ViewBag.SerializedDelaysAchievement = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/DataTables/DelayDetailsRow", model);
        }

        #endregion

        #region Progress
        public virtual ActionResult ProgressResult(string operationNumber, string tcReportingPeriod)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = new ProgressModel();
            SetPermissionFindingsRecommendations();
            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(operationNumber);
            model = _findingRecomendationService.GetFindingRecomendationProgress(
                operationNumber, tcReportingPeriod, Localization.CurrentLanguage);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);

            ViewBag.SerializedProgress = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/IndexProgressView", model);
        }

        public virtual ActionResult AddDocument(string documentNumber)
        {
            SetPermissionFindingsRecommendations();
            var user = _findingRecomendationService.GetUserDocument(documentNumber);
            DateTime? date = _findingRecomendationService.GetDateDocument(documentNumber);
            var description = string.Empty;
            var model = new DocumentsTableModel
            {
                User = user,
                Date = date,
                IdbDocNumber = documentNumber,
                Description = description
            };
            return PartialView("Partial/DataTables/RowDocumentView", model);
        }

        public virtual ActionResult DownloadDocument(string documentNumber)
        {
            return Redirect(DownloadDocumentHelper.GetDocumentLink(documentNumber));
        }

        /// <summary>
        /// DownloadDocument using IDBDocsProxyService
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual FileResult DownloadDocument(DownloadRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                var downloadResponse = _docManagementService.Download(request);

                if (!downloadResponse.IsValid)
                {
                    return File(downloadResponse.Document.File, MimeMapping.GetMimeMapping(downloadResponse.Document.FileName), downloadResponse.Document.FileName);
                }
            }

            return null;
        }

        #endregion

        #region PartnerAndConsultances

        public virtual ActionResult PartnerAndConsultaciesContent(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = new PartnersAndConsultanciesModel();
            SetPermissionFindingsRecommendations();
            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            ViewBag.TypePartner = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEPARTNER);
            ViewBag.ConsultanciesType = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPECONSULTANCIES);
            ViewBag.ConsultanciesNationality = _viewModelMapperHelper.GetListNationality(TCMGlobalValues.NATIONALITY);
            ViewBag.QualityAssesment = _catalogService.GetListMasterData(TCMGlobalValues.QUALITYASSESSMENT, orderByCodeAsc: true);
            model = _findingRecomendationService.GetFinPartnerConsultances(rspnse.OperationNumber);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/PartnersAndConsiltanciesView", model);
        }

        public virtual ActionResult GetRowPartner()
        {
            SetPermissionFindingsRecommendations();
            var model = new PartnersAndConsultanciesModel();
            ViewBag.TypePartner = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEPARTNER);
            var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(IDBContext.Current.Operation, PAGES_TCM);
            return PartialView("Partial/DataTables/PartnerRow", fieldAccessList);
        }

        public virtual ActionResult GetRowConsultancies()
        {
            SetPermissionFindingsRecommendations();
            var model = new PartnersAndConsultanciesModel();
            ViewBag.ConsultanciesType = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPECONSULTANCIES);
            ViewBag.ConsultanciesNationality = _viewModelMapperHelper.GetListNationality(TCMGlobalValues.NATIONALITY);
            ViewBag.QualityAssesment = _catalogService.GetListMasterData(TCMGlobalValues.QUALITYASSESSMENT, orderByCodeAsc: true);
            var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(IDBContext.Current.Operation, PAGE_PARTNER_CONSUL);
            return PartialView("Partial/DataTables/ConsultancyRow", fieldAccessList);
        }

        #endregion

        public virtual JsonResult ToBlockByChangeInView(string operationNumber, string url)
        {
            var response = new ResponseBase { IsValid = true };
            var errorMessage = SynchronizationHelper.AccessToResources(
               "edit",
               url,
               operationNumber,
               IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual FileResult DownloadReportConsultancies(
          string operationNumber,
          string formatType)
        {
            var response = _findingRecomendationService.GetConsultanciesReport(
                operationNumber,
                formatType);

            if (!response.IsValid)
                return null;

            return File(response.File, response.Application, response.FileName);
        }

        public virtual JsonResult UnlockChangeInView(string operationNumber, string url)
        {
            var response = new ResponseBase { IsValid = true };
            SynchronizationHelper.TryReleaseLock(url, operationNumber, IDBContext.Current.UserLoginName);
            return Json(response);
        }

        #region SustainabilityAndInnovation

        public virtual ActionResult SustainabilityAndInnovationContent(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = new SustainabilityAndInnovationModel();
            SetPermissionFindingsRecommendations();

            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);

            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (!editValidation && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            model = _findingRecomendationService.GetSustainabilityAndInnovation(rspnse.OperationNumber);
            var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            model.Innovation.ForEach(o => o.FieldAccessList = fieldAccessList);
            ViewBag.TypeInnovation = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEINNOVATION);
            ViewBag.SerializedSustainabilityAndInnovationViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/SustainabilityInnovationView", model);
        }

        public virtual ActionResult GetRowInnovation()
        {
            SetPermissionFindingsRecommendations();
            var model = new SustainabilityAndInnovationModel();
            ViewBag.TypeInnovation = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPEINNOVATION);
            var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(IDBContext.Current.Operation, PAGE_INNOVATION);
            return PartialView("Partial/DataTables/InnovationRow", fieldAccessList);
        }

        #endregion

        #region StoriesFromTheField

        public virtual ActionResult StoriesFromTheFieldContent(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = new StoriesFromTheFieldModel();
            SetPermissionFindingsRecommendations();

            var cycloOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            ViewBag.CodeReportingCycle = cycloOperation;
            var editValidation = _findingRecomendationService.OperationEditValidation(rspnse.OperationNumber);
            var hasPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
            var teamLeader = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            if (editValidation && ((hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)) || teamLeader))
            {
                ViewBag.IsFREditable = true;
            }
            else if (editValidation == false && (hasPermission && cycloOperation.Equals(TCMGlobalValues.CYCLE_TCM)))
            {
                ViewBag.IsFREditable = true;
            }
            else
            {
                ViewBag.IsFREditable = false;
            }

            model = _findingRecomendationService.GetStoriesFromTheField(rspnse.OperationNumber);
            ViewBag.SerializedStories = PageSerializationHelper.SerializeObject(model);
            ViewBag.TypeStory = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPESTORY);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            return PartialView("Partial/StoriesFromTheFieldView", model);
        }

        public virtual ActionResult GetRowSotiesFromTheField()
        {
            SetPermissionFindingsRecommendations();
            var model = new StoriesFromTheFieldModel();
            ViewBag.TypeStory = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPESTORY);
            var fieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(IDBContext.Current.Operation, PAGE_STORIES_FIELD);
            return PartialView("Partial/DataTables/StoriesFromFieldRow", fieldAccessList);
        }
        #endregion

        #region OverallProjectManagement

        public virtual ActionResult GetRowOverallProjectManagement(List<FieldAccessModel> fieldAccessList = null)
        {
            string language = IDBContext.Current.CurrentLanguage.ToUpper();

            //ListStages
            SetPermissionFindingsRecommendations();
            var ListStages = _overallProjectManagementRepository.GetStages(language)
                .OrderBy(x => x.Name);
            ViewBag.ListStages = new SelectList(ListStages, "ConvergenceMasterDataID", "Name");

            //ListDimensions
            var ListDimensions = _overallProjectManagementRepository.GetDimensions(language);
            ListDimensions.Reverse();
            ListDimensions.Reverse();
            string[,] CategoriesDimensions = new string[2, ListDimensions.Count];
            int count = 0;
            foreach (var data_ in ListDimensions)
            {
                if (data_.ConvergenceMasterDataId == 0)
                {
                    CategoriesDimensions[0, count] = data_.ConvergenceMasterDataId.ToString();
                }
                else
                {
                    var ListCategoriesFilter = _overallProjectManagementRepository.GetCategoriesFilter(language, data_.ConvergenceMasterDataId);

                    foreach (var data__ in ListCategoriesFilter)
                    {
                        CategoriesDimensions[0, count] = data_.ConvergenceMasterDataId.ToString();
                        CategoriesDimensions[1, count] += "<option value='" + data__.ConvergenceMasterDataId + "' >" + data__.Name + "</option>";
                    }
                }

                count++;
            }

            ViewBag.ListDimensions = new SelectList(ListDimensions, "ConvergenceMasterDataID", "Name");

            var ListCategories = _overallProjectManagementRepository.GetCategories(language);
            ListCategories.Reverse();
            ListCategories.Reverse();
            ViewBag.ListCategories = new SelectList(ListCategories, "ConvergenceMasterDataID", "Name");

            var ListItemsCategory = new List<SelectListItem>();
            foreach (var item in ListCategories)
            {
                ListItemsCategory.Add(new SelectListItem
                {
                    Value = item.ConvergenceMasterDataId.ToString(),
                    Text = item.Name,
                    Group = new SelectListGroup
                    {
                        Name = item.ParentConvergenceMasterDataId.ToString()
                    }
                });
            }

            ViewBag.ListItemsCategory = ListItemsCategory;

            ViewBag.LastUpdate = DateTime.Now;
            return PartialView("Partial/DataTables/OverallProjectRow", fieldAccessList);
        }

        public virtual ActionResult OverallProjectManagementContent(string operationNumber)
        {
            var model = new FindingRecommendationHeaderModel();
            SetPermissionFindingsRecommendations();
            ViewBag.IsFREditable = _findingRecomendationService.OperationEditValidation(operationNumber);

            string lang = IDBContext.Current.CurrentLanguage.ToUpper();
            string textYes = Localization.GetText("Yes");
            string textNo = Localization.GetText("No");
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);

            model = _overallProjectManagementRepository.GetFindingAdittional(operationNumber, lang, "0", "0", "0", null);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            ViewBag.IsCurrentCycleTable = true;

            foreach (var AllValues in model.FindingRecommendations)
            {
                ViewBag.IsCurrentCycleTable = AllValues.IsCurrentCycle;
            }

            SetOverallProjectManagement(operationNumber, lang);
            ViewBag.IsEditable = model.IsEditable;
            ViewBag.SerializedOverallProjectManagement = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/OverallProjectManagementView", model);
        }

        public virtual ActionResult SearchFilterResultOverall(string operationNumber, string tcReportingPeriod, string stage, string dimension, string category)
        {
            var modelOverallProjectManagement = new FindingRecommendationHeaderModel();
            if (tcReportingPeriod == "null" || tcReportingPeriod == string.Empty)
            {
                tcReportingPeriod = null;
            }

            stage = stage.Equals(string.Empty) ? "0" : stage;
            dimension = dimension.Equals(string.Empty) ? "0" : dimension;
            category = category.Equals(string.Empty) ? "0" : category;

            SetPermissionFindingsRecommendations();
            ViewBag.IsFREditable = _findingRecomendationService.OperationEditValidation(operationNumber);
            ViewBag.ReportingPeriod = _viewModelMapperHelper.GetPmrList(operationNumber);
            ViewBag.Stage = _catalogService.GetListMasterData(TCMGlobalValues.STAGELIST, orderByCodeAsc: true);
            ViewBag.Dimension = _catalogService.GetListMasterData(TCMGlobalValues.TYPE, orderByCodeAsc: true);

            modelOverallProjectManagement = _overallProjectManagementRepository.GetFindingAdittional(operationNumber,
                                                                                IDBContext.Current.CurrentLanguage.ToUpper(),
                                                                                dimension,
                                                                                category,
                                                                                stage,
                                                                                tcReportingPeriod);
            modelOverallProjectManagement.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            ViewBag.IsCurrentCycleTable = true;
            foreach (var finding in modelOverallProjectManagement.FindingRecommendations)
            {
                ViewBag.IsCurrentCycleTable = finding.IsCurrentCycle;
            }

            SetOverallProjectManagement(operationNumber, IDBContext.Current.CurrentLanguage.ToUpper());
            ViewBag.IsEditable = modelOverallProjectManagement.IsEditable;
            ViewBag.SerializedOverallProjectManagement = PageSerializationHelper.SerializeObject(modelOverallProjectManagement);
            return PartialView("Partial/OverallProjectManagementView", modelOverallProjectManagement);
        }

        #endregion

        #region DealysDetail
        public virtual ActionResult DelaysDetailsResult(string operationNumber, string typeDelay, string otherType, string name, string otherName, string stateSolved, string flag, string cycleList)
        {
            string lang = IDBContext.Current.CurrentLanguage.ToUpper();

            if (flag == "delay")
            {
                stateSolved = !string.IsNullOrEmpty(stateSolved) ? stateSolved + "|0" : "null|0";
            }
            else
            {
                stateSolved = !string.IsNullOrEmpty(stateSolved) ? stateSolved + "|1" : "null|1";
            }

            if (string.IsNullOrEmpty(cycleList))
            {
                cycleList = null;
            }

            typeDelay = !string.IsNullOrEmpty(typeDelay) ? typeDelay : "0";
            otherType = !string.IsNullOrEmpty(otherType) ? otherType : "0";
            name = !string.IsNullOrEmpty(name) ? name : "Name";
            otherName = !string.IsNullOrEmpty(otherName) ? otherName : "Name";

            var model = _delayAchievementRepository.GetDelaysAndOther(operationNumber, lang, typeDelay, otherType, name, otherName, stateSolved, cycleList);
            var ListCycles = _delayAchievementRepository.GetCycles(lang, operationNumber);

            Dictionary<int, string> ListOutputs = new Dictionary<int, string>();
            Dictionary<int, string> ListOutcomes = new Dictionary<int, string>();

            var ResultOP = _delayAchievementRepository.GetListOutputs(operationNumber);
            var ResultOC = _delayAchievementRepository.GetListOutComes(operationNumber);

            foreach (var data in ResultOP)
            {
                if (data.Key != 0)
                {
                    ListOutputs.Add(data.Key, data.Value);
                }
            }

            foreach (var data in ResultOC)
            {
                if (data.Key != 0)
                {
                    ListOutcomes.Add(data.Key, data.Value);
                }
            }

            var ListDelaysType = _delayAchievementRepository.GetTypesDelay(lang);
            ListDelaysType.Reverse();
            var ListItemDelaysType = new List<SelectListItem>();
            foreach (var item in ListDelaysType)
            {
                ListItemDelaysType.Add(new SelectListItem
                {
                    Value = item.ConvergenceMasterDataId.ToString(),
                    Text = item.Name
                });
            }

            string nameOutputDelay = ListDelaysType.FirstOrDefault(q => q.Code == "OUTPDEL").Name;
            string nameOutcomeDelay = ListDelaysType.FirstOrDefault(q => q.Code == "OUTCDEL").Name;

            SetDelaysInAchievement(operationNumber, lang, ListCycles);
            foreach (var itemAchievementDelays in model.AchievementDelays)
            {
                itemAchievementDelays.StateIsSolved = (itemAchievementDelays.IsSolved == true) ? "Yes" : "No";
                ViewBag.IsCurrentCycleTable = itemAchievementDelays.IsCurrentCycle;
                itemAchievementDelays.ResultElements = new Dictionary<string, string>();

                if (itemAchievementDelays.DelayType == nameOutcomeDelay)
                {
                    foreach (var item in ListOutcomes)
                    {
                        itemAchievementDelays.ResultElements.Add(item.Key.ToString(), item.Value);

                        if (itemAchievementDelays.ResultElementName == item.Value)
                        {
                            itemAchievementDelays.ResultElementId = item.Key.ToString();
                        }
                    }
                }
                else
                {
                    foreach (var item in ListOutputs)
                    {
                        itemAchievementDelays.ResultElements.Add(item.Key.ToString(), item.Value);

                        if (itemAchievementDelays.ResultElementName == item.Value)
                        {
                            itemAchievementDelays.ResultElementId = item.Key.ToString();
                        }
                    }
                }
            }

            foreach (var itemOtherDelays in model.OtherDelays)
            {
                itemOtherDelays.StateIsSolved = (itemOtherDelays.IsSolved == true) ? "Yes" : "No";
                ViewBag.IsCurrentCycleTable = itemOtherDelays.IsCurrentCycle;
            }

            SetPermissionFindingsRecommendations();

            SetOverallProjectManagement(operationNumber, lang);

            var ListItemsOutPuts = new List<SelectListItem>();
            foreach (var item in ListOutputs)
            {
                ListItemsOutPuts.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutputDelay).Value.ToString()
                    }
                });
            }

            var ListItems = new List<SelectListItem>();
            var ListItemsOutComes = new List<SelectListItem>();
            foreach (var item in ListOutcomes)
            {
                ListItemsOutComes.Add(new SelectListItem
                {
                    Value = item.Key.ToString(),
                    Text = item.Value,
                    Group = new SelectListGroup
                    {
                        Name = ListItemDelaysType.Find(q => q.Text == nameOutcomeDelay).Value.ToString()
                    }
                });
            }

            foreach (var items in ListItemsOutComes)
            {
                ListItems.Add(items);
            }

            foreach (var items2 in ListItemsOutPuts)
            {
                ListItems.Add(items2);
            }

            ViewBag.ListOutputs = ListItemsOutPuts;
            ViewBag.ListOutcomes = ListItemsOutComes;
            ViewBag.ListDelaysType = ListItemDelaysType;
            ViewBag.ListItems = ListItems;
            ViewBag.IsFREditable = _findingRecomendationService.OperationEditValidation(operationNumber);
            ViewBag.IsEditable = model.IsEditable;

            ViewBag.SerializedDelays = PageSerializationHelper.SerializeObject(model);
            ViewBag.SerializedDelaysAchievement = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/IndexDelaysDetails", model);
        }

        public virtual ActionResult GetRowOtherDelay(string operationNumber)
        {
            SetPermissionFindingsRecommendations();
            var model = new OtherDelayModel();
            string lang = IDBContext.Current.CurrentLanguage.ToUpper();
            var ListCycles = _delayAchievementRepository.GetCycles(lang, operationNumber);
            SetDelaysInAchievement(operationNumber, lang, ListCycles);
            ViewBag.TypeStory = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.TYPESTORY);
            ViewBag.PrincipalActor = _viewModelMapperHelper.GetListMasterData(TCMGlobalValues.PRINCIPALACTOR);
            return PartialView("Partial/DataTables/OtherDelaysRow", model);
        }

        public virtual ActionResult DelaysResult(string operationNumber, string typeDelay, string name, string stateSolved)
        {
            var model = new ProgressModel();
            SetPermissionFindingsRecommendations();

            ViewBag.CodeReportingCycle = _tcmUniverseService.GetCode(operationNumber);
            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGES_TCM);
            ViewBag.SerializedProgress = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/IndexProgressView", model);
        }

        #endregion

        public virtual ActionResult GetCategories(string operationNumber)
        {
            return PartialView("~/Areas/TCM/Views/FindingRecommendation/Partial/CategoriesModal.cshtml");
        }

        private void SetPermissionFindingsRecommendations()
        {
            ViewBag.Partner = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Consultainces = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Stories = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Progress = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Sustainability = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Innovation = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.Project = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.OverallProject = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.DelaysAnchievement = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
            ViewBag.AdminPermission = IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);
        }

        private void SetDelaysInAchievement(string operationNumber, string lang, List<FilterItemModel> ListCycles)
        {
            var ListIsSolved = new List<SelectListItem>();
            Dictionary<int, string> ListOutputsss = new Dictionary<int, string>();

            ListIsSolved.Add(new SelectListItem { Value = true.ToString(), Text = Localization.GetText("Yes") });
            ListIsSolved.Add(new SelectListItem { Value = false.ToString(), Text = Localization.GetText("No") });

            ListCycles.Reverse();
            ListCycles.RemoveAt(0);
            ListCycles.Reverse();

            ListCycles.Add(new FilterItemModel { ItemId = int.MaxValue, Name = Localization.GetText("All Cycles") });
            ListCycles.Add(new FilterItemModel { ItemId = 0, Name = Localization.GetText("Current Cycle") });
            ListCycles.Reverse();

            var ListItemPMRCycles = new List<MultiSelectListItem>();
            foreach (var item in ListCycles)
            {
                ListItemPMRCycles.Add(new MultiSelectListItem
                {
                    Value = item.ItemId.ToString(),
                    Text = item.Name
                });
            }

            var ListItemPMRCyclesEdit = new List<SelectListItem>();
            foreach (var item in ListCycles)
            {
                ListItemPMRCyclesEdit.Add(new SelectListItem
                {
                    Value = item.ItemId.ToString(),
                    Text = item.Name
                });
            }

            ViewBag.ListCyclesMulti = ListItemPMRCycles;
            ViewBag.ListCyclesEdit = ListItemPMRCyclesEdit;
            ViewBag.ListIsSolved = ListIsSolved;
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(_reportsGenericRepository, _pcrWorkflowStatusRepository);
            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
        }

        private void SetOverallProjectManagement(string operationNumber, string lang)
        {
            var ListDimensions = _overallProjectManagementRepository.GetDimensions(lang);
            var ListCategories = _overallProjectManagementRepository.GetCategories(lang);
            var ListStages = _overallProjectManagementRepository.GetStages(lang).OrderBy(x => x.Name);
            var ListCycles = _overallProjectManagementRepository.GetCycles(lang, operationNumber);

            ListDimensions.Reverse();
            ListDimensions.Reverse();
            ListCategories.Reverse();
            ListCategories.Reverse();
            ListCycles.Reverse();
            ListCycles.RemoveAt(0);
            ListCycles.Reverse();
            ListCycles.Add(new FilterItemModel { ItemId = int.MaxValue, Name = Localization.GetText("All Cycles") });
            ListCycles.Add(new FilterItemModel { ItemId = 0, Name = Localization.GetText("Current Cycle") });
            ListCycles.Reverse();

            string[,] CategoriesDimensions = new string[2, ListDimensions.Count];
            int count = 0;
            foreach (var data_ in ListDimensions)
            {
                if (data_.ConvergenceMasterDataId == 0)
                {
                    CategoriesDimensions[0, count] = data_.ConvergenceMasterDataId.ToString();
                }
                else
                {
                    var ListCategoriesFilter = _overallProjectManagementRepository.GetCategoriesFilter(lang, data_.ConvergenceMasterDataId);

                    foreach (var data__ in ListCategoriesFilter)
                    {
                        CategoriesDimensions[0, count] = data_.ConvergenceMasterDataId.ToString();
                        CategoriesDimensions[1, count] += "<option value='" + data__.ConvergenceMasterDataId + "' >" + data__.Name + "</option>";
                    }
                }

                count++;
            }

            ViewBag.ListDimensions = new SelectList(ListDimensions, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategories = new SelectList(ListCategories, "ConvergenceMasterDataID", "Name");
            ViewBag.ListStages = new SelectList(ListStages, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCycles = new SelectList(ListCycles, "ItemId", "Name");
            ViewBag.CategoriesDimensions = CategoriesDimensions;

            var listMulti = new List<MultiSelectListItem>();
            foreach (var item in ListCycles)
            {
                var multi = new MultiSelectListItem()
                {
                    Selected = false,
                    Value = item.ItemId.ToString(),
                    Text = item.Name.ToString()
                };
                listMulti.Add(multi);
            }

            ViewBag.ListPmrCycle = listMulti;
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(_reportsGenericRepository, _pcrWorkflowStatusRepository);
            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            var ListItemsCategory = new List<SelectListItem>();
            foreach (var item in ListCategories)
            {
                ListItemsCategory.Add(new SelectListItem
                {
                    Value = item.ConvergenceMasterDataId.ToString(),
                    Text = item.Name,
                    Group = new SelectListGroup
                    {
                        Name = item.ParentConvergenceMasterDataId.ToString()
                    }
                });
            }

            ViewBag.ListItemsCategory = ListItemsCategory;
            ViewBag.SelectStages = string.Empty;
            ViewBag.SelectDimensions = string.Empty;
            ViewBag.SelectCategories = string.Empty;
        }
    }
}