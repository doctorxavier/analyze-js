using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Domain.Models.FindingRecomendations;
using IDB.MW.Domain.Models.Architecture.FindingAndRecomendations;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.FindingAndRecomendations;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.FindingRecomendations.Controllers
{
    public partial class FindingRecommendationController : BaseController
    {
        #region Repositorios

        private IResultsMatrixModelRepository _ClientResultMatrix = null;
        public virtual IResultsMatrixModelRepository ClientResultMatrix
        {
            get { return _ClientResultMatrix; }
            set { _ClientResultMatrix = value; }
        }

        private IOperationModelRepository _ClientOperation = null;
        public virtual IOperationModelRepository ClientOperation
        {
            get { return _ClientOperation; }
            set { _ClientOperation = value; }
        }

        private IDelayAchievementModelRepository _ClientDelayArchievementRepository = null;
        public IDelayAchievementModelRepository ClientDelayArchievementRepository
        {
            get { return _ClientDelayArchievementRepository; }
            set { _ClientDelayArchievementRepository = value; }
        }

        private IClauseExpiredModelRepository _ClientClauseExpiredModelRepository = null;
        public IClauseExpiredModelRepository ClientClauseExpiredModelRepository
        {
            get { return _ClientClauseExpiredModelRepository; }
            set { _ClientClauseExpiredModelRepository = value; }
        }

        private IFindingRecommendationModelRepository _ClientFindingAndRecomendationsModelRepository = null;
        public IFindingRecommendationModelRepository findingAndRecommendationsRepository
        {
            get { return _ClientFindingAndRecomendationsModelRepository; }
            set { _ClientFindingAndRecomendationsModelRepository = value; }
        }

        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;
        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        private IReportsGenericRepository _reportsRepository = null;
        public IReportsGenericRepository ReportsRepository
        {
            get { return _reportsRepository; }
            set { _reportsRepository = value; }
        }
        #endregion

        #region Contractual Clauses

        public virtual ActionResult Index(string operationNumber, int State = 0) //DETAILS: /ExpiredClauses
        {
            var Model = ClientClauseExpiredModelRepository.GetClausesExpired(operationNumber, Language.EN);
            Model.OperationNumber = operationNumber;

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingClauses(State, false, 2);
                ViewData["message"] = message;
            }

            return RedirectToAction("DetailsDelays", "FindingRecommendation", new { area = "FindingRecomendations", operationNumber = operationNumber });
        }

        [HasPermission(Permissions = "Findings Write")]
        public virtual ActionResult ExpiredClausesEdit(string operationNumber) // GET: /ExpiredClauses/Edit/
        {
            var model = ClientClauseExpiredModelRepository.GetClausesExpiredEdit(operationNumber, Language.EN);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult ExpiredClausesEdit(ExpiredClausesEditModel resultModel) //POST: /ExpiredClauses/Edit
        {
            bool operationUpdate = ClientClauseExpiredModelRepository.SaveClauseExpired(resultModel);

            if (operationUpdate)
            {
                return RedirectToAction("Index", "FindingRecommendation", new { area = "FindingRecomendations", operationNumber = resultModel.OperationNumber, State = 600 });
            }
            else
            {
                return RedirectToAction("Index", "FindingRecommendation", new { area = "FindingRecomendations", operationNumber = resultModel.OperationNumber, State = 601 });
            }
        }

        #endregion

        #region ResultMatrixChanges

        public virtual ActionResult DetailsMatrixChanges(string operationNumber, int State = 0)
        {
            var Model = ClientResultMatrix.GetChangesResultMatrix(operationNumber, Language.EN);

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingMatrixChange(State, false, 2);
                ViewData["message"] = message;
            }

            return View(Model);
        }

        [HasPermission(Permissions = "Findings Write")]
        public virtual ActionResult MatrixChangesEdit(string operationNumber)
        {
            var Model = ClientResultMatrix.GetChangesResultMatrixEdit(operationNumber, Language.EN);
            return View(Model);
        }

        [HttpPost]
        public virtual ActionResult MatrixChangesEdit(MatrixChangesEditModel resultModel)
        {
            bool operationUpdate = ClientResultMatrix.SaveChangesResultMatrix(resultModel);

            if (operationUpdate)
            {
                return RedirectToAction("DetailsMatrixChanges", "FindingRecommendation", new { area = "FindingRecomendations", operationNumber = resultModel.OperationNumber, State = 602 });
            }
            else
            {
                return RedirectToAction("DetailsMatrixChanges", "FindingRecommendation", new { area = "FindingRecomendations", operationNumber = resultModel.OperationNumber, State = 603 });
            }
        }

        #endregion

        #region Delays

        public virtual ActionResult DetailsDelays(string operationNumber, int State = 0, string Type = "0", string Name = "Name", string Solved = "null", string cycleIdList = null) //DETAILS: /Delays
        {
            if (cycleIdList == "null")
            {
                cycleIdList = null;
            }

            var ModelDelaysArchitecture = ClientDelayArchievementRepository.GetDelays(operationNumber, Language.EN, Type, Name, Solved, cycleIdList);

            var ListCycles = ClientDelayArchievementRepository.GetCycles(Language.EN, operationNumber);

            ViewBag.IsCurrentCycleTableOne = true;
            ViewBag.IsCurrentCycleTableTwo = true;

            foreach (var itemAchievementDelays in ModelDelaysArchitecture.AchievementDelays)
            {
                itemAchievementDelays.StateIsSolved = itemAchievementDelays.IsSolved == true ? Localization.GetText("Yes") : Localization.GetText("No");
                ViewBag.IsCurrentCycleTableOne = itemAchievementDelays.IsCurrentCycle;
            }

            foreach (var itemOtherDelays in ModelDelaysArchitecture.OtherDelays)
            {
                itemOtherDelays.StateIsSolved = itemOtherDelays.IsSolved == true ? Localization.GetText("Yes") : Localization.GetText("No");
                ViewBag.IsCurrentCycleTableTwo = itemOtherDelays.IsCurrentCycle;
            }

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingDelays(State, false, 2);
                ViewData["message"] = message;
            }

            /*FILTER ELEMENTS*/
            var ListDelaysType = ClientDelayArchievementRepository.GetTypesDelay(Language.EN);
            Dictionary<int, string> ListOutputs = new Dictionary<int, string>();
            Dictionary<int, string> ListOutcomes = new Dictionary<int, string>();
            ListOutputs.Add(0, Localization.GetText("Name"));
            ListOutcomes.Add(0, Localization.GetText("Name"));
            var ResultOP = ClientDelayArchievementRepository.GetListOutputs(operationNumber);
            var ResultOC = ClientDelayArchievementRepository.GetListOutComes(operationNumber);
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

            var ListIsSolved = new Dictionary<string, string>();
            Dictionary<int, string> ListOutputsss = new Dictionary<int, string>();
            ListDelaysType.Reverse();
            ListDelaysType.Add(new MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel()
            {
                ParentConvergenceMasterDataId = -1, Name = @Localization.GetText("Type of delay")
            });
            ListDelaysType.Reverse();
            ListIsSolved.Add("null", Localization.GetText("Is the issue solved?"));
            ListIsSolved.Add(true.ToString(), Localization.GetText("Yes"));
            ListIsSolved.Add(false.ToString(), Localization.GetText("No"));
            ListCycles.Reverse();
            ListCycles.RemoveAt(0);
            ListCycles.Reverse();
            ListCycles.Add(new FilterItemModel { ItemId = int.MaxValue, Name = Localization.GetText("All Cycles") });
            ListCycles.Add(new FilterItemModel { ItemId = 0, Name = Localization.GetText("Current Cycle") });
            ListCycles.Reverse();
            ViewBag.ListDelaysType = new SelectList(ListDelaysType, "ConvergenceMasterDataID", "Name");
            ViewBag.ListIsSolved = new SelectList(ListIsSolved, "key", "value");
            ViewBag.ListOutputs = new SelectList(ListOutputs, "value", "value");
            ViewBag.ListOutcomes = new SelectList(ListOutcomes, "value", "value");
            ViewBag.ListCycles = new SelectList(ListCycles, "ItemId", "Name");

            /*FILTER ELEMENTS*/
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                ReportsRepository, PCRWorkflowStatusRepository);
            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            return View(ModelDelaysArchitecture);
        }

        [HasPermission(Permissions = "Findings Write")]
        public virtual ActionResult DelaysEdit(string operationNumber) // GET: /Delays/Edit/
        {
            var model = ClientDelayArchievementRepository.LoadEditDelays(operationNumber, Language.EN);

            var ListDelaysType = ClientDelayArchievementRepository.GetTypesDelay(Language.EN);
            var ListOutputs = ClientDelayArchievementRepository.GetListOutputs(operationNumber);
            var ListOutcomes = ClientDelayArchievementRepository.GetListOutComes(operationNumber);
            var ListCycles = ClientDelayArchievementRepository.GetCycles(Language.EN, operationNumber);
            var ListNoNameDelays = new Dictionary<int, string>();
            var ListIsSolved = new Dictionary<bool, string>();

            ListIsSolved.Add(true, Localization.GetText("Yes"));
            ListIsSolved.Add(false, Localization.GetText("No"));

            ListNoNameDelays.Add(0, "No data fount");

            if (ListDelaysType.Count > 0)
            {
                ViewBag.NoData = true;
            }

            ViewBag.ListDelaysType = new SelectList(ListDelaysType, "ConvergenceMasterDataID", "Name");
            ViewBag.ListIsSolved = new SelectList(ListIsSolved, "key", "value");
            ViewBag.ListNoNameDelays = new SelectList(ListNoNameDelays, "key", "value");
            ViewBag.ListOutputs = new SelectList(ListOutputs, "value", "value");
            ViewBag.ListOutcomes = new SelectList(ListOutcomes, "value", "value");
            ViewBag.ListCycles = new SelectList(ListCycles, "ItemId", "Name");

            return View(model);
        }
        
        /*** NEW METHODS (the rest can be removed) ****/
        [HttpPost]
        public virtual ActionResult SaveFindingAndRecommendations(
            FindingRecommendationHeaderModel model)
        {
            string response = findingAndRecommendationsRepository
                .SaveFindingAndRecommendations(model.OperationNumber, model.FindingRecommendations);

            return new JsonResult
            {
                Data = new
                {
                    isValid = string.IsNullOrEmpty(response),
                    message = response
                }
            };
        }

        public virtual ActionResult GetFindingsAndRecommendations(string operationNumber)
        {
            bool isEditable;
            var findingsAndRecommendations =
                findingAndRecommendationsRepository.GetFindingsAndRecommendations(
                    operationNumber,
                    IDBContext.Current.CurrentLanguage,
                    out isEditable);

            // AJAX calls don't include the operation number in the HttpRequeest.RouteData
            // so we need to setup the operation number forced
            IDBContext.Current.Operation = operationNumber;

            FindingRecommendationHeaderModel model = new FindingRecommendationHeaderModel
            {
                FindingRecommendations = findingsAndRecommendations,
                IsEditable = isEditable,
                IsCurrentPcrTaskLessThanFive = PCRHelpers.IsCurrentTaskLessThanFive(
                    ReportsRepository, PCRWorkflowStatusRepository),
                HasFindingsWritePermission = IDBContext.Current.HasPermission(
                    Permission.FINDINGS_WIRTE),
                HasRMAdministratorRole = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR),
                OperationNumber = operationNumber
            };

            return new JsonResult
            {
                Data = model
            };
        }

        public virtual ActionResult GetCycles(string operationNumber)
        {
            var cycles = findingAndRecommendationsRepository
                .GetCycles(IDBContext.Current.CurrentLanguage, operationNumber);

            return new JsonResult
            {
                Data = cycles
            };
        }

        public virtual ActionResult GetDimensions()
        {
            var dimensions = findingAndRecommendationsRepository
                .GetDFAndSGDimensions(IDBContext.Current.CurrentLanguage);

            return new JsonResult
            {
                Data = dimensions
            };
        }

        public virtual ActionResult GetCategories()
        {
            var categories = findingAndRecommendationsRepository
                .GetDFAndSGCategories(IDBContext.Current.CurrentLanguage);

            return new JsonResult
            {
                Data = categories
            };
        }

        public virtual ActionResult GetStages()
        {
            var stages = findingAndRecommendationsRepository
                .GetStages(IDBContext.Current.CurrentLanguage);

            return new JsonResult
            {
                Data = stages
            };
        }

        public virtual JsonResult GetOutputsDelays(string operationNumber)
        {
            var outputsDelays = ClientDelayArchievementRepository
                .GetListOutputs(operationNumber);

            var data = outputsDelays.Select(x => new 
            {
                ConvergenceMasterDataId = x.Key,
                NameEn = x.Value
            });

            return new JsonResult()
            {
                Data = data
            };
        }

        public virtual JsonResult GetOutcomesDelays(string operationNumber)
        {
            var outcomesDelays = ClientDelayArchievementRepository
                .GetListOutComes(operationNumber);

            var data = outcomesDelays.Select(x => new 
            {
                ConvergenceMasterDataId = x.Key,
                NameEn = x.Value
            });
            return new JsonResult()
            {
                Data = data
            };
        }

        public virtual JsonResult GetTypeDelays()
        {
            var typesDelays = ClientDelayArchievementRepository
                .GetTypesDelay(IDBContext.Current.CurrentLanguage);

            return new JsonResult()
            {
                Data = typesDelays
            };
        }

        /*** NEW METHODS (the rest can be removed) ****/

        [HttpPost()]
        public virtual ActionResult DelaysEdit(DelaysEditModel modelDelayModel, int[] deletedDelaysAchievement, int[] deletedDelaysOther)
        {
            bool StateRequest = true;
            bool StateSaveUpdate = true;
            bool StateDeleteAchievement = true;
            bool StateDeleteOther = true;

            var NewModelOther = modelDelayModel.OtherDelays.Where(x => x.DelayName != null);
            modelDelayModel.OtherDelays = NewModelOther.ToList();

            if (!ClientDelayArchievementRepository.SaveDelays(modelDelayModel))
            {
                StateRequest = false;
                StateSaveUpdate = false;
            }

            if (deletedDelaysAchievement != null)
            {
                if (deletedDelaysAchievement.Count() > 0)
                {
                    if (!ClientDelayArchievementRepository.DeleteAchievement(deletedDelaysAchievement))
                    {
                        StateRequest = false;
                        StateDeleteAchievement = false;
                    }
                }
            }

            if (deletedDelaysOther != null)
            {
                if (deletedDelaysOther.Count() > 0)
                {
                    if (!ClientDelayArchievementRepository.DeleteOtherDelay(deletedDelaysOther))
                    {
                        StateRequest = false;
                        StateDeleteOther = false;
                    }
                }
            }

            if (StateRequest == false)
            {
                if (StateDeleteAchievement == false && StateSaveUpdate == false && StateDeleteOther == false)
                {
                    return RedirectToAction("DetailsDelays", new { operationNumber = modelDelayModel.OperationNumber, State = 605 });
                }
                else if (StateSaveUpdate == false)
                {
                    return RedirectToAction("DetailsDelays", new { operationNumber = modelDelayModel.OperationNumber, State = 605 });
                }
                else if (StateDeleteAchievement == false)
                {
                    return RedirectToAction("DetailsDelays", new { operationNumber = modelDelayModel.OperationNumber, State = 605 });
                }
                else if (StateDeleteOther == false)
                {
                    return RedirectToAction("DetailsDelays", new { operationNumber = modelDelayModel.OperationNumber, State = 605 });
                }
            }

            return RedirectToAction("DetailsDelays", new { operationNumber = modelDelayModel.OperationNumber, State = 604 });
        }

        public virtual JsonResult LoadOutputsDelays(DelaysEditModel ModelDelay)
        {
            var ListOutputsDelays = ClientDelayArchievementRepository.GetListOutputs(ModelDelay.OperationNumber).ToList();
            return new JsonResult() { Data = ListOutputsDelays };
        }

        public virtual JsonResult LoadOutcomesDelays(DelaysEditModel ModelDelay)
        {
            var ListOutComesDelays = ClientDelayArchievementRepository.GetListOutComes(ModelDelay.OperationNumber).ToList();
            return new JsonResult() { Data = ListOutComesDelays };
        }

        public virtual JsonResult LoadTypeDelays(DelaysEditModel ModelDelay)
        {
            var ListTypeDelays = ClientDelayArchievementRepository.GetTypesDelay(Language.EN).ToList();
            return new JsonResult() { Data = ListTypeDelays };
        }

        public virtual JsonResult DeleteAchivement(int DeleteAchivementID)
        {
            int[] AchivementDelete = { DeleteAchivementID };
            if (ClientDelayArchievementRepository.DeleteAchievement(AchivementDelete))
            {
                return new JsonResult() { Data = "success" };
            }

            return new JsonResult() { Data = "error" };
        }

        public virtual JsonResult DeleteOther(int DeleteOtherID)
        {
            int[] OtherDelete = { DeleteOtherID };
            if (ClientDelayArchievementRepository.DeleteOtherDelay(OtherDelete))
            {
                return new JsonResult() { Data = "success" };
            }

            return new JsonResult() { Data = "error" };
        }

        #endregion

        #region Risk

        public virtual ActionResult DetailsRiskManagement(string operationNumber, int State = 0) //DETAILS: /RiskManagement
        {
            var model = ClientResultMatrix.GetRiskFindings(operationNumber);

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingRisk(State, false, 2);
                ViewData["message"] = message;
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult RiskManagementEdit(RiskManagementEditModel model) //POST: /RiskManagement/Edit
        {
            if (ClientResultMatrix.SaveRiskDescription(model))
            {
                return RedirectToAction("DetailsRiskManagement", new { operationNumber = model.OperationNumber, State = 606 });
            }
            else
            {
                return RedirectToAction("DetailsRiskManagement", new { operationNumber = model.OperationNumber, State = 607 });
            }
        }

        #endregion

        #region Overall Project Management

        public virtual ActionResult DetailsFindingRecommendation(
            string operationNumber,
            int State = 0,
            string Dimensions = "0",
            string Categories = "0",
            string Stages = "0",
            string cycleIdList = null)
        {
            if (cycleIdList == "null")
            {
                cycleIdList = null;
            }

            var Model = findingAndRecommendationsRepository.GetFindingAdittional(operationNumber, Language.EN, Dimensions, Categories, Stages, cycleIdList);

            ViewBag.IsCurrentCycleTable = true;

            foreach (var AllValues in Model.FindingRecommendations)
            {
                ViewBag.IsCurrentCycleTable = AllValues.IsCurrentCycle;
            }

            var ListDimensions = findingAndRecommendationsRepository.GetDimensions(Language.EN);
            var ListCategories = findingAndRecommendationsRepository.GetCategories(Language.EN);
            var ListStages = findingAndRecommendationsRepository.GetStages(Language.EN);
            var ListCycles = findingAndRecommendationsRepository.GetCycles(Language.EN, operationNumber);

            ListDimensions.Reverse();
            ListDimensions.Add(new MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel()
            {
                ConvergenceMasterDataId = 0, Name = Localization.GetText("Dimensions")
            });

            ListDimensions.Reverse();
            ListCategories.Reverse();
            ListCategories.Add(new MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel()
            {
                ConvergenceMasterDataId = 0, Name = Localization.GetText("Categories")
            });

            ListCategories.Reverse();
            ListStages.Reverse();
            ListStages.Add(new MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel()
            {
                ConvergenceMasterDataId = 0, Name = Localization.GetText("Stages")
            });

            ListStages.Reverse();
            ListCycles.Reverse();
            ListCycles.RemoveAt(0);
            ListCycles.Reverse();
            ListCycles.Add(new FilterItemModel
            {
                ItemId = int.MaxValue, Name = Localization.GetText("All Cycles")
            });

            ListCycles.Add(new FilterItemModel
            {
                ItemId = 0, Name = Localization.GetText("Current Cycle")
            });

            ListCycles.Reverse();

            string[,] CategoriesDimensions = new string[2, ListDimensions.Count];
            int count = 0;
            foreach (var data_ in ListDimensions)
            {
                if (data_.ConvergenceMasterDataId == 0)
                {
                    CategoriesDimensions[0, count] = data_.ConvergenceMasterDataId.ToString();
                    CategoriesDimensions[1, count] += "<option value='" + data_.ConvergenceMasterDataId + "' selected='selected'>" + Localization.GetText("Categories") + "</option>";
                }
                else
                {
                    CategoriesDimensions[1, count] += "<option value='0' selected='selected'>" + Localization.GetText("Categories") + "</option>";
                    var ListCategoriesFilter = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, data_.ConvergenceMasterDataId).ToList();

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

            if (State != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageFindingOverallProjectManagement(State, false, 2);
                ViewData["message"] = message;
            }

            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
               ReportsRepository, PCRWorkflowStatusRepository);
            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            return View(Model);
        }

        [HasPermission(Permissions = "Findings Write")]
        public virtual ActionResult FindingRecommendationEdit(string operationNumber)
        {
            var Model = findingAndRecommendationsRepository.GetFindingAdittionalEdit(operationNumber, Language.EN);

            var ListStages = findingAndRecommendationsRepository.GetStages(Language.EN);
            var ListDimensions = findingAndRecommendationsRepository.GetDimensions(Language.EN);

            var ListCategoriesTecSecDim = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("TEC-SEC-DIM"));
            var ListCategoriesOrgDim = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("ORG-DIM"));
            var ListCategoriesFidDim = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("FID-DIM"));
            var ListCategoriesDimProcAct = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("DIM-PROC-ACT"));
            var ListCategoriesDimLegPol = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("DIM-LEG-POL"));
            var ListCategoriesOther = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, findingAndRecommendationsRepository.GetDimensionIdByCode("OTHER"));

            ViewBag.ListStages = new SelectList(ListStages, "ConvergenceMasterDataID", "Name");
            ViewBag.ListDimensions = new SelectList(ListDimensions, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesTecSecDim = new SelectList(ListCategoriesTecSecDim, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesOrgDim = new SelectList(ListCategoriesOrgDim, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesFidDim = new SelectList(ListCategoriesFidDim, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesDimProcAct = new SelectList(ListCategoriesDimProcAct, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesDimLegPol = new SelectList(ListCategoriesDimLegPol, "ConvergenceMasterDataID", "Name");
            ViewBag.ListCategoriesOther = new SelectList(ListCategoriesOther, "ConvergenceMasterDataID", "Name");
            return View(Model);
        }

        [HttpPost]
        public virtual ActionResult FindingRecommendationEdit(FindingRecommendationHeaderModel ModelFindingAndRecomendations, int[] FindingsDeleted) //POST: /FindingRecomendation/Edit
        {
            bool StatusRequest = true;

            if (!findingAndRecommendationsRepository.SaveFindings(ModelFindingAndRecomendations))
            {
                StatusRequest = false;
            }

            if (FindingsDeleted != null)
            {
                if (!findingAndRecommendationsRepository.DeleteFindings(FindingsDeleted))
                {
                    StatusRequest = false;
                }
            }

            if (!StatusRequest)
            {
                return RedirectToAction("DetailsFindingRecommendation", new { operationNumber = ModelFindingAndRecomendations.OperationNumber, State = 609 });
            }
            else
            {
                return RedirectToAction("DetailsFindingRecommendation", new { operationNumber = ModelFindingAndRecomendations.OperationNumber, State = 608 });
            }
        }

        public virtual JsonResult DeleteAdittional(int DeleteAdittionalID)
        {
            int[] sasa = { DeleteAdittionalID };
            if (findingAndRecommendationsRepository.DeleteFindings(sasa))
            {
                return new JsonResult() { Data = "ok" };
            }

            return new JsonResult() { Data = "error" };
        }

        public virtual JsonResult CategoryFilter(int ValueSelected)
        {
            var ListCategoriesFilter = findingAndRecommendationsRepository.GetCategoriesFilter(Language.EN, ValueSelected).ToList();
            return new JsonResult() { Data = ListCategoriesFilter };
        }

        [HttpGet]
        public virtual ActionResult Notification()
        {
            return PartialView("Notification");
        }

        #endregion
    }
}