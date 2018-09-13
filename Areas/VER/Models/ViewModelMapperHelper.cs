using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.VERModule.Messages.Request;
using IDB.MW.Application.VERModule.Services;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.VERModule.Services.Permissions;
using IDB.MW.Application.VERModule.Services.Tasks;
using IDB.MW.Domain.Models.Security;
using IDB.MW.Domain.Values.Ver;
using IDB.MW.DomainModel.Contracts.Repositories.Core;

namespace IDB.Presentation.MVC4.Areas.VER.Models
{
    public class ViewModelMapperHelper
    {
        #region fields

        private const string MAXCONCURRENCETIMEINMINUTES = "MaxConcurrenceTimeInMinutes";
        
        private readonly object _lockingDelete = new object();
        private readonly object _lockingResource = new object();
        private readonly int _maxConcurrenceTimeInMinutes;
        private readonly IVerService _verService;
        private readonly ICatalogService _catalogService;
        private readonly IVerPermissionsService _verPermissionsService;
        private readonly IVerTasksService _verTaskService;
        private readonly IConcurrenceRepository _concurrenceRepository;
        private readonly IAdUsersRepository _adUsersRepository;
        private dynamic _viewBag;
        #endregion

        public ViewModelMapperHelper(dynamic viewBag,
            IVerService verService,
            ICatalogService catalogService,
            IVerTasksService verTaskService,
            IVerPermissionsService verPermissionsService,
            IConcurrenceRepository concurrenceRepository,
            IAdUsersRepository adUsersRepository)
        {
            _verService = verService;
            _viewBag = viewBag;
            _catalogService = catalogService;
            _verPermissionsService = verPermissionsService;
            _verTaskService = verTaskService;
            _concurrenceRepository = concurrenceRepository;
            _adUsersRepository = adUsersRepository;
            _maxConcurrenceTimeInMinutes = int.Parse(ConfigurationManager.AppSettings[MAXCONCURRENCETIMEINMINUTES]);
        }

        public VerViewModel GetVerGeneral(int instanceId)
        {
            var instanceData = _verService.GetVerGeneral(instanceId);

            var model = instanceData.Ver;
            var security = instanceData.Security;

            GetParticipantList(false);
            GetDocumentList();
            GetTaskList(instanceId);
            GetHistoryList();
            EditableScreen(model.IsLockedScreen);
            SecurityFirstLoad(security);

            _viewBag.SerializedParticipantViewModel = PageSerializationHelper.SerializeObject(model.Participant);
            _viewBag.SerializedDocumentViewModel = PageSerializationHelper.SerializeObject(model.Document);
            _viewBag.SerializedTaskViewModel = PageSerializationHelper.SerializeObject(model.Task);
            _viewBag.SerializedPolicyWaiverViewModel = PageSerializationHelper.SerializeObject(model.PolicyWaiver);

            return model;
        }

        public void GetParticipantList(bool excludeOtherParticipantType)
        {
            _viewBag.ParticipantsTypeList = excludeOtherParticipantType
                ? _catalogService.GetListMasterData(
                    MasterType.VER_PARTICIPANT_TYPE,
                    excludeByCode: new List<string>
                    {
                        VerParticipantTypeCode.EvpOwner,
                        VerParticipantTypeCode.Task
                    })
                : _catalogService.GetListMasterData(MasterType.VER_PARTICIPANT_TYPE);
            _viewBag.OrganizationUnit = OrgUnitList();
            _viewBag.RoleList = RoleList();
            _viewBag.RoleListTable = RoleListTable();
            GetPagination();
            _viewBag.Office365PermissionsList = _catalogService.GetListMasterData(MasterType.OFFICE_365_PERMISSION);
        }

        public IList<SelectListItem> OrgUnitList()
        {
            var list = new List<SelectListItem>();

                list.AddRange(_catalogService
                    .GetListMasterData(
                        MasterType.ORGANIZATION_UNIT, 
                        false, 
                        null, 
                        null,
                        false, 
                        true)
                    .Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }));

                list.AddRange(_catalogService
                    .GetListMasterData(
                        MasterType.COUNTRY, 
                        false,
                        null,
                        null,
                        false,
                        true)
                    .Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }));

                list.AddRange(_catalogService
                        .GetListMasterData(
                        MasterType.VMR_MEETING_TYPE,
                        false,
                        null, 
                        null,
                        false,
                        true)
                    .Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }));

            return list;
        }

        public void GetDocumentList()
        {
            _viewBag.DocumentTypeList = _catalogService.GetListMasterData(MasterType.VER_DOCUMENT_TYPE);

            GetPagination();
        }

        public void GetHistoryList()
        {
            GetPagination();
        }

        public void GetTaskList(int instanceId)
        {
            _viewBag.ActivityTypeList =
                _verTaskService.GetActitityTaskList()
                    .ListActivity.Select(o => new SelectListItem { Value = o.Value.ToString(), Text = o.Text })
                    .ToList();
            GetPagination();

            var roleListRequest = new TaskRoleListRequest
            {
                InstanceId = instanceId
            };

            _viewBag.RoleListTask = _verTaskService.GetRoleListTask(roleListRequest).RoleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text,
                }).ToList();

            _viewBag.RoleListTaskClosed = _verTaskService.GetRoleListTaskClosed(roleListRequest).RoleList.Select(o => new SelectListItem
            {
                Selected = false,
                Value = o.Value,
                Text = o.Text,
            }).ToList();
        }

        public void EditableScreen(bool isEditable)
        {
            _viewBag.EditableScreen = isEditable;
        }

        public void SecurityFirstLoad(IList<FieldAccessModel> security)
        {
            _viewBag.SecurityBasicData = security.Where(o => o.Page == VerSecurityValues.SECURITY_VER_BASIC_DATA_PAGE).ToList();
            _viewBag.SecurityParticipant = security.Where(o => o.Page == VerSecurityValues.SECURITY_VER_PARTICIPANT_PAGE).ToList();
            _viewBag.SecurityDocument = security.Where(o => o.Page == VerSecurityValues.SECURITY_VER_DOCUMENT_PAGE).ToList();
            _viewBag.SecurityTask = security.Where(o => o.Page == VerSecurityValues.SECURITY_VER_TASK_PAGE).ToList();
        }

        public void SecurityBasicData(IList<FieldAccessModel> security)
        {
            _viewBag.SecurityBasicData = security;
        }

        public void SecurityParticipant(IList<FieldAccessModel> security)
        {
            _viewBag.SecurityParticipant = security;
        }

        public void SecurityDocument(IList<FieldAccessModel> security)
        {
            _viewBag.SecurityDocument = security;
        }

        public void SecurityTask(IList<FieldAccessModel> security)
        {
            _viewBag.SecurityTask = security;
        }

        public IList<SelectListItem> RoleList()
        {
            var list = new List<SelectListItem>();
            var roleList = _verPermissionsService.GetRoleList();
            var convergencerRole = Globals.Resolve<IConvergenceRoleRepository>();

            if (roleList.IsValid && roleList.RoleList.Any())
            {
                list = roleList.RoleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text,
                }).ToList();
            }

            if (list.Any())
            {
                if (!list.Exists(x => x.Text == MemberRoleCode.EXTERNAL_CONTRACTOR))
                {
                    var external = convergencerRole.GetOne(x => x.Name == MemberRoleCode.EXTERNAL_CONTRACTOR);

                    if (external != null)
                    {
                        list.Add(new SelectListItem
                        {
                            Selected = false,
                            Text = external.Name,
                            Value = external.ConvergenceRoleId.ToString()
                        });
                    }
                }
            }

            return list;
        }

        public IList<SelectListItem> RoleListTable()
        {
            var list = new List<SelectListItem>();
            var roleList = _verPermissionsService.GetRoleListTable();

            if (roleList.IsValid && roleList.RoleList.Any())
            {
                list = roleList.RoleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text,
                }).ToList();
            }

            return list;
        }

        public void GetPagination()
        {
            var resultsPerPage = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = string.Format("{0}" + " " + Localization.GetText("GLOBAL.RESUTLS.PER.PAGE"), "5"),
                    Value = "5"
                },
                new SelectListItem
                {
                    Text = string.Format("{0}" + " " + Localization.GetText("GLOBAL.RESUTLS.PER.PAGE"), "10"),
                    Value = "10"
                },
                new SelectListItem
                {
                    Text = string.Format("{0}" + " " + Localization.GetText("VER.GLOBAL.Results"), "All"),
                    Value = "999999"
                }
            };

            _viewBag.ResultsPerPageList = resultsPerPage;
        }

        public bool IsLockConcurrence(List<string> lstTabsConcurrence, string operationNumber, string userName)
        {
            bool isLockConcurrence = false;
            foreach (var resource in lstTabsConcurrence)
            {
                lock (_lockingResource)
                {
                    var concurrence = _concurrenceRepository
                        .GetOne(x => resource.Equals(x.Url) && x.Query.Equals(operationNumber));
                    bool notExistConcurrence = concurrence == null ||
                        (DateTime.Now > concurrence.LastUpdate.AddMinutes(_maxConcurrenceTimeInMinutes) &&
                         !concurrence.User.Equals(userName, StringComparison.InvariantCultureIgnoreCase)) ||
                        concurrence.User.Equals(userName, StringComparison.InvariantCultureIgnoreCase);
                    if (!notExistConcurrence)
                    {
                        isLockConcurrence = true;
                    }
                }
            }

            return isLockConcurrence;
        }

        private string SanitizeFileName(string documentName)
        {
            string charsToRemove = new string(Path.GetInvalidFileNameChars()) +
                                   new string(Path.GetInvalidPathChars());
            string pattern = string.Format("[{0}]", Regex.Escape(charsToRemove));
            return Regex.Replace(documentName, pattern, "-");
        }
    }
}