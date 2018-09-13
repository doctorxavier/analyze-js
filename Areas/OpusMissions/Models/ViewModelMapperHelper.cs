using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Core.ViewModels.Common;
using IDB.MW.Application.MissionsModule.ViewModels;
using IDB.MW.Application.MissionsModule.ViewModels.Workflows;
using IDB.MW.Application.OpusMissionsModule.Enums;
using IDB.MW.Application.OpusMissionsModule.Messages.Mission.MissionService;
using IDB.MW.Application.OpusMissionsModule.Messages.ReportsService;
using IDB.MW.Application.OpusMissionsModule.Services.MissionService;
using IDB.MW.Application.OpusMissionsModule.Services.ReportsService;
using IDB.MW.Application.OpusMissionsModule.ViewModels.Report;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models.Reports;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Models
{
    public class ViewModelMapperHelper
    {
        private const string AllValuesSelector = "-1";
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;
        private readonly IMissionReportService _missionReportService;

        private readonly dynamic _viewBag;
        private readonly IMissionService _missionService;

        public ViewModelMapperHelper(
            dynamic viewBag,
            ICatalogService catalogService,
            IAuthorizationService authorizationService,
            IMissionService missionService,
            IMissionReportService missionReportService)
        {
            _viewBag = viewBag;
            _catalogService = catalogService;
            _authorizationService = authorizationService;
            _missionService = missionService;
            _missionReportService = missionReportService;
        }

        public List<SelectListItem> GetCategories()
        {
            return _viewBag.Categories = (
                from scoreCategory in _catalogService.GetPCRScoreCategories().ScoreCategoryViewModels
                select new SelectListItem
                {
                    Text = scoreCategory.Description,
                    Value = scoreCategory.Id
                }).ToList();
        }

        public void GetMonitoringReport()
        {
            var masterData = _catalogService.GetMasterDataListByTypeCode(
                false,
                MonitoringReportType.Country,
                MonitoringReportType.SectorDepartment,
                MonitoringReportType.Status,
                MonitoringReportType.Type);

            if (!masterData.IsValid || masterData.MasterDataCollection == null)
            {
                _viewBag.StatusList = new List<SelectListItem>();
                _viewBag.TypeList = new List<SelectListItem>();
                Logger.GetLogger().WriteDebug("ViewModelMapperHelper", masterData.ErrorMessage);
                _viewBag.ErrorMessage = masterData.ErrorMessage;
                _viewBag.CountryList = new List<SelectListItem>();
                _viewBag.CountryDepartmentList = new List<SelectListItem>();
                _viewBag.SectorDepartmentList = new List<SelectListItem>();
                _viewBag.DivisionList = new List<SelectListItem>();
            }
            else
            {
                var sectorDepartment = masterData.MasterDataCollection
                     .Where(m => m.TypeName == MonitoringReportType.SectorDepartment).ToList();
                var TypeList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.Type,
                    IDBContext.Current.CurrentLanguage);

                _viewBag.TypeList = TypeList;

                var StatusList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.Status,
                    IDBContext.Current.CurrentLanguage);

                _viewBag.StatusList = StatusList;

                _viewBag.CountryList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.Country,
                    IDBContext.Current.CurrentLanguage);

                var countryDepartments = _catalogService
                    .GetCountryDepartments(masterData.MasterDataCollection
                    .Where(m => m.TypeName == MonitoringReportType.Country).ToList());

                if (countryDepartments.IsValid)
                    _viewBag.CountryDepartmentList = GetListItemsCountryDepartments(
                        countryDepartments.CountryDepartments,
                        IDBContext.Current.CurrentLanguage);

                var sector = _catalogService.GetSectorDepartments(sectorDepartment);

                if (sector.IsValid)
                    _viewBag.SectorDepartmentList = GetListItemsCode(sector.SectorDepartments);

                var division = _catalogService.GetDivision(sectorDepartment);

                if (division.IsValid)
                    _viewBag.DivisionList = GetListItemsCode(division.Division);
            }
        }

        public FilterOptionsReportViewModel SetFilterOptions(
            string monitoringStatus,
            string type,
            string sectorDepartment,
            string division,
            string countryDepartment,
            string country,
            bool allMonitoringStatus,
            bool allCountryDepartment,
            bool allCountry,
            bool AllType,
            bool allSectorDepartment,
            bool allDivision,
            string operationNumberMission,
            string missionMembersMission,
            string startDateMission,
            string endDateMission)
        {
            _viewBag.monitoringStatus = monitoringStatus;
            _viewBag.allMonitoringStatus = allMonitoringStatus;
            _viewBag.type = type;
            _viewBag.AllType = AllType;
            _viewBag.sectorDepartment = sectorDepartment;
            _viewBag.allSectorDepartment = allSectorDepartment;
            _viewBag.division = division;
            _viewBag.allDivision = allDivision;
            _viewBag.countryDepartment = countryDepartment;
            _viewBag.allCountryDepartment = allCountryDepartment;
            _viewBag.country = country;
            _viewBag.allCountry = allCountry;
            _viewBag.operationNumberMission = operationNumberMission;
            _viewBag.missionMembersMission = missionMembersMission;
            _viewBag.startDateMission = startDateMission;
            _viewBag.endDateMission = endDateMission;

            if (monitoringStatus == AllValuesSelector || type == AllValuesSelector ||
                countryDepartment == AllValuesSelector || country == AllValuesSelector ||
                sectorDepartment == AllValuesSelector || division == AllValuesSelector)
            {
                GetMonitoringReport();

                if (monitoringStatus == AllValuesSelector)
                    monitoringStatus = string.Join(",", ((List<SelectListItem>)_viewBag.StatusList)
                        .Select(i => i.Value));

                if (type == AllValuesSelector)
                    type = string.Join(",", ((List<SelectListItem>)_viewBag.TypeList)
                        .Select(i => i.Value));

                if (countryDepartment == AllValuesSelector)
                    countryDepartment =
                        string.Join(",", ((List<SelectListItem>)_viewBag.CountryDepartmentList)
                            .Select(i => i.Value));

                if (country == AllValuesSelector)
                    country = string.Join(",", ((List<SelectListItem>)_viewBag.CountryList)
                        .Select(i => i.Value));

                if (sectorDepartment == AllValuesSelector)
                    sectorDepartment =
                        string.Join(",", ((List<SelectListItem>)_viewBag.SectorDepartmentList)
                            .Select(i => i.Value));

                if (division == AllValuesSelector)
                    division = string.Join(",", ((List<SelectListItem>)_viewBag.DivisionList)
                        .Select(i => i.Value));
            }

            return new FilterOptionsReportViewModel
            {
                SectorDepartment = sectorDepartment.Split(',').ToList(),
                Division = division.Split(',').ToList(),
                CountryDepartment = countryDepartment.Split(',').ToList(),
                Country = country.Split(',').ToList(),
                Status = monitoringStatus.Split(',').ToList(),
                types = type.Split(',').ToList(),
                AllCountryDepartment = allCountryDepartment,
                AllCountries = allCountry,
                AllSectorDepartment = allSectorDepartment,
                AllDivision = allDivision,
                AllStatus = allMonitoringStatus,
                AllTypes = AllType,
                operationNumber = operationNumberMission,
                missionMembers = missionMembersMission,
                startDate = startDateMission,
                endDate = endDateMission
            };
        }

        public DownloadReportResponse FileReport(
            string Status,
            string type,
            string sectorDepartment,
            string division,
            string countryDepartment,
            string country,
            bool allStatus,
            bool allCountryDepartment,
            bool allCountries,
            bool allType,
            bool allSectorDepartment,
            bool allDivision,
            string operationNumber,
            string missionMembers,
            string startDate,
            string endDate,
            OutputFormatEnum fileFormat)
        {
            var paramsReport = SetFilterOptions(
                Status,
                type,
                sectorDepartment,
                division,
                countryDepartment,
                country,
                allStatus,
                allCountryDepartment,
                allCountries,
                allType,
                allSectorDepartment,
                allDivision,
                operationNumber,
                missionMembers,
                startDate,
                endDate);

            return _missionReportService.DownloadReport(paramsReport, fileFormat);
        }

        public MissionsReportHeadViewModel GetResultHeadReport(SubmitReportResponse submitReportResponse)
        {
            return new MissionsReportHeadViewModel
            {
                Types = submitReportResponse.missionReportResult.TypesHead,
                Status = submitReportResponse.missionReportResult.MissionStatusHead,
                Countries = submitReportResponse.missionReportResult.CountryHead,
                CountryDepartments = submitReportResponse.missionReportResult.CountryDepartmentHead,
                SectorDepartments = submitReportResponse.missionReportResult.SectorDepartmentHead,
                Divisions = submitReportResponse.missionReportResult.DivisionHead,
                OperationNumber = submitReportResponse.missionReportResult.operationNumberHead,
                MissionMembers = submitReportResponse.missionReportResult.missionMembersHead,
                StartDate = (submitReportResponse.missionReportResult.startDateHead != null) ?
                    submitReportResponse.missionReportResult.startDateHead.Replace("/", " ") : string.Empty,
                EndDate = (submitReportResponse.missionReportResult.endDateHead != null) ?
                    submitReportResponse.missionReportResult.endDateHead.Replace("/", " ") : string.Empty,
                Missions = submitReportResponse.missionReportResult.DivisionLists
            };
        }

        public List<MissionViewModel> GetListMissionAll(string operationNumber)
        {
            var response = _missionService.GetMissionsByOperation(operationNumber);
            return response.MissionViewModel;
        }

        public GetCompleteMissionResponse GetMissionbyId(int missionId)
        {
            var response = new GetCompleteMissionResponse();
            response = _missionService.GetMissionbyId(missionId);
            return response;
        }

        public List<MissionViewModel> GetMissionFilterSearch(
            string operationNumber,
            string status,
            string type,
            string countryDepartment,
            string country,
            string startDateMission,
            string endDateMission)
        {
            var response = _missionService.GetMissionsByFilter(
                operationNumber, status, type, countryDepartment, country, startDateMission, endDateMission);
            return response.MissionViewModel;
        }

        public List<MissionsWorkflowDocumentsViewModels> GetWorkFlowMissionDocuments(int missionId)
        {
            string descriptionTor = "TOR v";
            var viewDocuments = _missionService.GetDocuments(missionId).MissionDocuments;
            var listDocument = new List<MissionsWorkflowDocumentsViewModels>();
            var listDocumentTor = new List<MissionsWorkflowDocumentsViewModels>();

            foreach (var item in viewDocuments)
            {
                var missionDocument = new MissionsWorkflowDocumentsViewModels()
                {
                    //DocNumber = Convert.ToInt32(item.Document.DocumentReference),
                    DocumentNumber = item.Document.DocumentReference,
                    Date = item.Document.Created.Value,
                    Description = item.Document.Description,
                    User = item.Document.CreatedBy
                };
                if ((missionDocument.Description.Length >= 5) && missionDocument.Description.Substring(0, 5).Equals(descriptionTor))
                {
                    listDocumentTor.Add(missionDocument);
                }
            }

            if (listDocumentTor.Count > 0)
            {
                IEnumerable<MissionsWorkflowDocumentsViewModels> listDocuments = listDocumentTor.OrderByDescending(x => x.DocNumber);
                foreach (var item in listDocuments)
                {
                    listDocument.Add(item);
                }
            }

            foreach (var item in viewDocuments)
            {
                var missionDocument = new MissionsWorkflowDocumentsViewModels()
                {
                    //DocNumber = Convert.ToInt32(item.Document.DocumentReference),
                    DocumentNumber = item.Document.DocumentReference,
                    Date = item.Document.Created.Value,
                    Description = item.Document.Description,
                    User = item.Document.CreatedBy
                };
                if ((missionDocument.Description.Length <= 5) || (missionDocument.Description.Substring(0, 5) != descriptionTor))
                {
                    listDocument.Add(missionDocument);
                }
            }

            return listDocument;
        }

        public List<DocumentViewModel> GetMissionDocuments(int missionId)
        {
            var viewDocuments = _missionService.GetDocuments(missionId).MissionDocuments;
            var listDocument = new List<DocumentViewModel>();

            foreach (var item in viewDocuments)
            {
                var missionDocument = new DocumentViewModel()
                {
                    DocNumber = item.Document.DocumentReference,
                    Date = item.Document.Created.Value,
                    Description = item.Document.Description,
                    User = item.Document.CreatedBy
                };

                listDocument.Add(missionDocument);
            }

            return listDocument;
        }

        public int GetIdForESGType(string operationNumber)
        {
            var result = _catalogService.GetConvergenceMasterDataIdByCode("MISSION_TYPE_ESG", "MISSION_TYPE");
            return result != null ? result.Id : 0;
        }

        public bool IsExecutionPhaseOperation(string operationNumber)
        {
            return _missionService.IsExecutionPhaseByOperation(operationNumber);
        }

        public int GetFieldCurrentActivity(string operationNumber)
        {
            var response = _missionService.GetActualCurrentActivity(operationNumber);
            return response.NumberActual;
        }

        public bool IsLastEmptyState(List<ValidatorViewModel> list)
        {
            bool response = false;
            int countApproved = 0;

            foreach (var item in list)
            {
                if (item.Status.Equals("Approved"))
                {
                    countApproved++;
                }
            }

            response = ((list.Count() - 1) == countApproved) ? true : false;
            return response;
        }

        public string GetLastDocumentTOR(int missionId)
        {
            string result = string.Empty;
            var response = _missionService.GetDocumentTOR(missionId);
            if (response.IsValid)
            {
                result = response.DocumentNumber;
            }

            return result;
        }

        public string GetStatusDrafMasterData(string type, string codStatus)
        {
            string status = string.Empty;
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                status = listRepository.MasterDataCollection.Where(a => a.Code.Equals(codStatus)).Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList().FirstOrDefault().Text.ToString();
            }

            return status;
        }

        public List<SelectListItem> GetMissionTypeListFilteredNoFase(string operationNumber)
        {
            var list = new List<SelectListItem>();
            var listRepository = _missionService.GetTypeMissionsNoFase(operationNumber);

            if (listRepository != null && listRepository.Count > 0)
            {
                list = listRepository.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public Document GetDocumentData(string documentNumber)
        {
            var document = _missionService.GetDocumentData(documentNumber);
            return document;
        }

        public List<SelectListItem> GetMissionTypeListFiltered(string operationNumber)
        {
            var list = new List<SelectListItem>();
            var listRepository = _missionService.GetTypeMissions(operationNumber);

            if (listRepository != null && listRepository.Count > 0)
            {
                list = listRepository.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public bool IsOperationType(string operationNumber, string code)
        {
            var OperationTypeResponse = _missionService.GetIsOperationType(operationNumber, code);
            return OperationTypeResponse.IsOperationType;
        }

        public List<SelectListItem> GetMembersMission(string operationNumber)
        {
            var list = new List<SelectListItem>();
            var listMembers = _missionService.GetMissionMember(operationNumber);
            if (listMembers != null && listMembers.Any())
            {
                foreach (var obj in listMembers)
                {
                    list.Add(new SelectListItem
                    {
                        Selected = false,
                        Value = obj.NameId.ToString(),
                        Text = obj.Name,
                    });
                }
            }

            return list;
        }

        public SelectListItem GetOrganizationalUnitForCode(string code)
        {
            var result = new SelectListItem();
            var OrgUnit = _missionService.GetOrganizationalUnitForCode(code).FirstOrDefault();
            if (OrgUnit != null)
            {
                result.Selected = false;
                result.Value = OrgUnit.MasterId.ToString();
                result.Text = MvcHelpers.GetItemName(OrgUnit, Localization.CurrentLanguage);
            }

            return result;
        }

        //TODO Delete after Refactor
        public List<SelectListItem> GetOrganizationalUnitHeader(string operationNumber)
        {
            var list = new List<SelectListItem>();
            var listMembers = _missionService.GetMissionMember(operationNumber);
            if (listMembers != null && listMembers.Any())
            {
                foreach (var obj in listMembers)
                {
                    list.Add(new SelectListItem
                    {
                        Selected = false,
                        Value = obj.OrdanizationalUnitId.ToString(),
                        Text = obj.OrdanizationalUnit
                    });
                }
            }

            return list;
        }

        public void GetMemberRole()
        {
            var masterData = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: MissionsTypes.MissionRoles);

            _viewBag.MemberRole = GetListItems(
                masterData.MasterDataCollection,
                MissionsTypes.MissionRoles,
                IDBContext.Current.CurrentLanguage);
        }

        public GetRoleResponse GetTeamRole(string OperationTeamDataId, string operationNumber)
        {
            var Response = new GetRoleResponse();
            Response = _missionService.GetTeamRole(OperationTeamDataId, operationNumber);

            return Response;
        }

        public List<SelectListItem> GetListMasterData(int type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeId(masterTypeIds: type);

            if (listRepository != null && listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public void GetCountryAndDepartment()
        {
            var masterData = _catalogService.GetMasterDataListByTypeCode(false, MonitoringReportType.Country, MonitoringReportType.SectorDepartment);
            _viewBag.CountryList = new List<SelectListItem>();
            _viewBag.CountryDepartmentList = new List<SelectListItem>();

            //Country 
            _viewBag.CountryList = GetListItems(masterData.MasterDataCollection, MonitoringReportType.Country, IDBContext.Current.CurrentLanguage);

            //Country Departments
            var countryDepartments = _catalogService.GetCountryDepartments(masterData.MasterDataCollection.Where(m => m.TypeName == MonitoringReportType.Country).ToList());
            if (countryDepartments.IsValid)
            {
                _viewBag.CountryDepartmentList = GetListItemsCountryDepartments(countryDepartments.CountryDepartments, IDBContext.Current.CurrentLanguage);
            }
        }

        public GetSelectListItemResponse GetListInstitution(string search)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();

            var listRepository = _missionService.GetListInstitution(search);

            if (listRepository != null && listRepository.Any())
            {
                if (listRepository.Any())
                {
                    response.ListResponse = listRepository.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.Institution.ToString(),
                        Text = o.Nm.ToString(),
                    }).ToList();
                }
            }

            return response;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> RemoveUsedRole(List<ValidatorViewModel> validators, List<SelectListItem> roles)
        {
            foreach (var validator in validators)
            {
                roles.RemoveAll(x => x.Text == validator.Role);
            }

            return roles;
        }

        public List<SelectListItem> GetValidatorsListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<ValidatorViewModel> FilterMandatory(
            List<ValidatorViewModel> Validators,
            string MissionType,
            bool isHaiti,
            string TaskTypeCode,
            string operationType)
        {
            switch (TaskTypeCode)
            {
                case "WF-MIS-VPC":

                    if (MissionType == "Economic")
                    {
                        Validators.RemoveAll(v => v.Role.Equals("VPC Manager") && v.Mandatory == true);
                        break;
                    }

                    Validators.RemoveAll(v => v.Role.Equals("Regional Economic Advisor") && v.Mandatory == true);
                    break;

                case "WF-MIS-ESG":

                    if (isHaiti)
                    {
                        Validators.RemoveAll(v => v.Role.Equals("Representative") && v.Mandatory == true);
                        break;
                    }

                    Validators.RemoveAll(v => v.Role.Equals("Chief of Operations") && v.Mandatory == true);
                    break;

                case "WF-MIS-VPS":
                    if (operationType == OperationType.CIP || operationType == OperationType.ESW)
                    {
                        Validators.RemoveAll(v => v.Role.Equals("Representative") && v.Mandatory == true);
                    }

                    break;
            }

            return Validators.GroupBy(v => v.Role).Select(g => g.LastOrDefault()).ToList();
        }

        public bool DeleteMissionCascade(int missionId)
        {
            var result = _missionService.DeleteMissionCascade(missionId);
            if (result.IsValid == true)
            {
                return true;
            }

            return false;
        }

        public List<SelectListItem> GetActivities(string operationNumber, int year)
        {
            var list = new List<SelectListItem>();
            var listRepository = _missionService.GetActivitiesByOperationAndYear(operationNumber, year);

            if (listRepository.Activity != null && listRepository.Activity.Count > 0)
            {
                list = listRepository.Activity.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.ActivityId.ToString(),
                    Text = o.Description
                }).ToList();
            }

            return list;
        }

        public string GetTypeSupervisionPlan(string operationNumber)
        {
            return _missionService.GetTypeSupervisionPlan(operationNumber);
        }

        public bool IsSpDetailed(string operationNumber, int year)
        {
            var response = _missionService.IsSpDetailed(operationNumber, year);
            return response;
        }

        public string GetMissionTypeName(int? missionTypeId)
        {
            if (missionTypeId == null)
            {
                return string.Empty;
            }

            var data = _catalogService.GetConvergenceMasterDataById(missionTypeId.Value);

            if (data.IsValid)
            {
                return data.Model.GetNameLanguage(Localization.CurrentLanguage);
            }

            return string.Empty;
        }

        #region Private Methods
        private static string GetSelectedItemText(
            IEnumerable<MultiSelectListItem> items,
            bool includeParent,
            bool ignoreFirstLevel)
        {
            var textBuffer = new StringBuilder();
            MultiSelectListItem previousParent = null;
            var orderedList = items.OrderBy(s => s.Parent != null &&
                ((ignoreFirstLevel && !s.Parent.IsFirstLevel) || !ignoreFirstLevel) ?
                    s.Parent.Text.Substring(0, 4) + ": " :
                    string.Empty + s.Text.Substring(0, 4))
                .ToList();
            var firstTime = true;

            foreach (var item in orderedList)
            {
                item.Selected = true;

                var text = string.Empty;

                if ((includeParent && item.Parent != null) &&
                    ((ignoreFirstLevel && !item.Parent.IsFirstLevel) || !ignoreFirstLevel))
                {
                    if (previousParent != item.Parent)
                    {
                        if (!firstTime)
                        {
                            text = "\n";
                        }

                        firstTime = false;

                        previousParent = item.Parent;
                        text = text + item.Parent.Text + ": ";
                    }

                    text += item.Text;
                }
                else
                {
                    text = item.Text;
                }

                textBuffer.AppendFormat("{0}, ", text);
            }

            if (textBuffer.Length > 0)
            {
                textBuffer.Length = textBuffer.Length - 2;
            }

            return textBuffer.ToString();
        }

        private static List<SelectListItem> GetListItems(
            IEnumerable<MasterDataViewModel> masterData,
            string type,
            string language)
        {
            if (type == MonitoringReportType.Country)
                masterData = masterData.Where(x => x.Code != CountryCode.UND && x.Code != CountryCode.IDB
                    && (!x.ExpirationDate.HasValue || x.ExpirationDate.Value > DateTime.Today)
                    && x.TypeName == type);
            var listItem = (from item in masterData
                            where item.TypeName == type
                            select new SelectListItem
                            {
                                Selected = false,
                                Text = MvcHelpers.GetItemName(item, language),
                                Value = item.MasterId.ToString(),
                                Group = new SelectListGroup
                                {
                                    Name = item.ParentMasterId.ToString()
                                }
                            }).OrderBy(o => o.Text).ToList();

            return listItem;
        }

        private static List<SelectListItem> GetListItemsCode(List<MasterDataViewModel> masterData)
        {
            var listItem = (from item in masterData
                            select new SelectListItem
                            {
                                Selected = false,
                                Text = item.Code,
                                Value = item.MasterId.ToString(),
                                Group = new SelectListGroup
                                {
                                    Name = item.ParentMasterId.ToString()
                                }
                            }).OrderBy(o => o.Text).ToList();

            return listItem;
        }

        private static List<SelectListItem> GetListItemsCountryDepartments(List<ConvergenceMasterData> countryDepartments, string language)
        {
            var listItem = (from item in countryDepartments
                            select new SelectListItem
                            {
                                Selected = false,
                                Text = MvcHelpers.GetItemName(item, language),
                                Value = item.ConvergenceMasterDataId.ToString()
                            }).OrderBy(o => o.Text).ToList();

            return listItem;
        }
        #endregion
    }
}