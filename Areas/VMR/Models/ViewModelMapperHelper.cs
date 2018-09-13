using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Application.VMRModule.Messages.Request;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.VMRModule.Services;
using IDB.MW.Application.VMRModule.ViewModels;
using IDB.MW.Application.VMRModule.Services.GenericService;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Security;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Ver;
using IDB.MW.Domain.Values.Vmr;
using IDB.MW.Application.VMRModule.Messages.Response;
using IDB.MW.Application.VMRModule.Services.Comment;
using IDB.MW.Business.VMRModule.Contracts;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Business.VMRModule.Messages.Request;
using IDB.Architecture.Security;
using IDB.MW.Application.Core.ViewModels;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.VMR.Models
{
    public class ViewModelMapperHelper
    {
        #region fields
        private readonly IVmrService _vmrService;
        private readonly ICatalogService _catalogService;
        private readonly IVmrGenericService _vmrGenericService;
        private readonly ICommentService _commentService;
        private readonly ICommentRulesService _commentRulesService;
        private dynamic _viewBag;
        #endregion

        public ViewModelMapperHelper(
            dynamic viewBag,
            IVmrService vmrService,
            ICatalogService catalogService,
            IVmrGenericService vmrGenericService,
            ICommentService commentService,
            ICommentRulesService commentRulesService)
        {
            _vmrService = vmrService;
            _viewBag = viewBag;
            _catalogService = catalogService;
            _vmrGenericService = vmrGenericService;
            _commentService = commentService;
            _commentRulesService = commentRulesService;
        }

        public VmrViewModel GetVmrGeneral(int instanceId, string derivedBy)
        {
            var derivedByClient = IDBContext.Current.GetClientPreference(VmrGlobalValues.ON_BEHALF_OF_USERNAME);

            if (!string.IsNullOrEmpty(derivedByClient) && string.IsNullOrEmpty(derivedBy))
            {
                IDBContext.Current
                    .AddClientPreference(VmrGlobalValues.ON_BEHALF_OF_USERNAME, string.Empty);
            }

            if (string.IsNullOrEmpty(derivedBy))
            {
                _vmrGenericService.VerifyAccess(instanceId);
            }

            var vmrGeneralRequest = new VmrGeneralRequest
            {
                InstanceId = instanceId,
                DerivedBy = !string.IsNullOrEmpty(derivedBy)
            };

            var instanceData = _vmrService.GetVmrGeneral(vmrGeneralRequest);

            var model = instanceData.Vmr;
            var security = instanceData.Security;

            GetParticipantList();
            GetDocumentList();
            SecurityFirstLoad(security);
            IsEditableScreen(model.IsEditableScreen);

            _viewBag.FolderShp = model.BasicData.FolderShp;
            _viewBag.OperationNumber = model.OperationNumber;
            _viewBag.Site = model.BasicData.SiteShp;

            _viewBag.SerializedBasicDataViewModel = PageSerializationHelper.SerializeObject(model.BasicData);
            _viewBag.SerializedParticipantViewModel = PageSerializationHelper.SerializeObject(model.Participants);
            _viewBag.SerializedDocumentViewModel = PageSerializationHelper.SerializeObject(model.Documents);

            return model;
        }

        public VerDocumentRowViewModel GetDataNewDocument(
            int instanceId,
            string documentNumber,
            string documentName,
            string docWebUrl,
            string documentNameTemp)
        {
            var requestInstance = new VmrInstanceRequest
            {
                InstanceId = instanceId
            };
            var vmrInstance = _vmrGenericService.GetInstance(requestInstance).Instance;
            var loadSecurityRequest = new VmrLoadSecurityRequest
            {
                Instance = vmrInstance,
                Pages = VmrSecurityValues.SECURITY_VMR_DOCUMENT_PAGE
            };

            var documentSecurity = _vmrGenericService.LoadSecurity(loadSecurityRequest).SecurityList;
            SecurityDocument(documentSecurity.ToList());

            var document = new VerDocumentRowViewModel
            {
                SLno = 0,
                DocumentType = _catalogService.GetConvergenceMasterDataIdByCode(
                    VerDocumentTypeCode.OTHER, MasterType.VER_DOCUMENT_TYPE).Id,
                DocumentName = documentName,
                PackageVersion = vmrInstance.VerInstance.Version.GetNameLanguage(
                    Localization.CurrentLanguage),
                LastUpdate = DateTime.Now,
                UserName = UserIdentityManager
                .SearchFullNameByUserName(
                    new GetUsersRequest { UserName = IDBContext.Current.UserName })
                    .FullName,
                IsPrimary = false,
                IsRequired = false,
                DocumentNumber = documentNumber,
                ShpUrlDocument = docWebUrl,
                IsVer = false,
                LastUpdateFormat = string.Format("{0:dd MMM yyyy}", DateTime.Now),
                IsVersionHistory = false,
                DocumentNameTemp = documentNameTemp,
                IsNewDocument = true
            };

            return document;
        }

        public void SecurityFirstLoad(List<FieldAccessModel> security)
        {
            _viewBag.SecurityBasicData = security.Where(o => o.Page == VmrSecurityValues.SECURITY_VMR_BASIC_DATA_PAGE).ToList();
            _viewBag.SecurityParticipant = security.Where(o => o.Page == VmrSecurityValues.SECURITY_VMR_PARTICIPANT_PAGE).ToList();
            _viewBag.SecurityDocument = security.Where(o => o.Page == VmrSecurityValues.SECURITY_VMR_DOCUMENT_PAGE).ToList();
        }

        public void SecurityBasicData(List<FieldAccessModel> securityTab)
        {
            _viewBag.SecurityBasicData = securityTab;
        }

        public void SecurityParticipant(List<FieldAccessModel> securityTab)
        {
            _viewBag.SecurityParticipant = securityTab;
        }

        public void SecurityDocument(IList<FieldAccessModel> securityTab)
        {
            _viewBag.SecurityDocument = securityTab;
        }

        public List<SelectListItem> RoleList()
        {
            var list = new List<SelectListItem>();
            var roleList = _vmrService.GetRoleList();
            if (roleList.IsValid && roleList.RoleList.Any())
            {
                list = roleList.RoleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> RoleListTable()
        {
            var list = new List<SelectListItem>();
            var roleList = _vmrService.GetRoleListTable();
            if (roleList.IsValid && roleList.RoleList.Any())
            {
                list = roleList.RoleList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> OrgUnitList()
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

        public void GetParticipantList()
        {
            _viewBag.ParticipantsTypeList = _catalogService.GetListMasterData(MasterType.VMR_PARTICIPANT_TYPE);
            _viewBag.OrganizationUnit = OrgUnitList();
            _viewBag.RoleList = RoleList();
            _viewBag.RoleListTable = RoleListTable();
            _viewBag.Office365PermissionsList = _catalogService.GetListMasterData(MasterType.OFFICE_365_PERMISSION);
            GetPagination();
        }

        public void GetDocumentList()
        {
            _viewBag.DocumentTypeList = _catalogService.GetListMasterData(MasterType.VER_DOCUMENT_TYPE);

            GetPagination();
        }

        public void IsEditableScreen(bool isEditable)
        {
            _viewBag.EditableScreen = isEditable;
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

        public List<ListItemViewModel> GetTopicList(int verPackageType)
        {
            var topicList = _catalogService.GetListMasterData(MasterType.TOPIC_DOCUMENT_LIST, byParent: verPackageType)
                .Select(p => new ListItemViewModel
                {
                    Value = p.Value,
                    Text = p.Text
                })
                .OrderBy(o => o.Value)
                .ToList();
            return topicList;
        }

        public void GetMyWorkingDocumentsViewModel()
        {
            _viewBag.FilterType = _catalogService.GetListMasterData(MasterType.VER_DOCUMENT_TYPE);
            _viewBag.FilterCountry = _catalogService.GetListMasterData(MasterType.COUNTRY);
        }

        public void GetMyMeetingsViewModel()
        {
            GetPagination();
            _viewBag.FilterType = _catalogService.GetListMasterData(MasterType.VMR_MEETING_TYPE);
        }

        public CommentsResponse FilterComment(int instanceId,
            FilterCommentViewModel filter = null)
        {
            var response = new CommentsResponse
            {
                CommentList = new List<CommentViewModel>()
            };

            filter = filter ?? new FilterCommentViewModel();
            var request = new CommentsRequest
            {
                InstanceId = instanceId,
                CommentTypeCode = filter.CommentTypeCode,
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo,
                OrderDesc = filter.OrderBy,
                OrganizationalUnitId = filter.OrganizationalUnitId,
                TopicCommentId = filter.TopicCommentId,
                UserName = filter.UserName,
                MyComments = filter.MyComments,
                IsCheckPublic = filter.IsCheckPublic,
                IsOpcAgreement = filter.IsOpcAgreement
            };

            var filterResponse = _commentService.FilterComments(request);

            if (filterResponse.IsValid)
            {
                response = filterResponse;
            }

            return response;
        }

        public CommentsResponse SaveComment(VmrSaveCommentViewModel model)
        {
            var modelSave = new SaveCommentRequest
            {
                InstanceId = model.InstanceId,
                Model = model
            };

            return _commentService.SaveComment(modelSave);
        }

        public TransferCommentResponse TransferComment(int commentId, int instanceId, bool isCheckTransfer)
        {
            var request = new TransferCommentRequest
            {
                CommentId = commentId,
                InstanceId = instanceId,
                IsCheckTransfer = isCheckTransfer
            };

            return _commentService.TransferComment(request);
        }

        public CommentViewModel GetNewLevel(int instanceId, int parentCommentId, int treeLevel)
        {
            var request = new CommentTreeRequest
            {
                InstanceId = instanceId,
                CommentId = parentCommentId,
                EditLevel = treeLevel
            };

            return _commentService.GetNewLevel(request).Comment;
        }

        public CommentViewModel GetCommentTree(int commentId, 
            int instanceId, 
            int level,
            bool isExpand, 
            bool isOpcAgreement)
        {
            CommentTreeRequest request = new CommentTreeRequest
            {
                CommentId = commentId,
                EditLevel = level,
                InstanceId = instanceId,
                IsExpand = isExpand,
                IsOpcAgreement = isOpcAgreement
            };

            var response = _commentService.GetCommentTree(request);
            return response.Comment;
        }

        public CommentsResponse DeleteComment(int commentId, int instanceId)
        {
            CommentTreeRequest request = new CommentTreeRequest
            {
                CommentId = commentId,
                InstanceId = instanceId
            };

            return _commentService.DeleteComment(request);
        }

        public ResponseBase ValidateGenerateMinute(ValidateMinuteRequest request)
        {
            return _commentService.ValidateGenerateMinute(request);
        }

        public List<ListItemViewModel> GetTypeComment(int instanceId)
        {
            List<ListItemViewModel> result = new List<ListItemViewModel>();
            CommentTypeFilterRequest request = new CommentTypeFilterRequest
            {
                InstanceId = instanceId
            };

            var response = _commentRulesService.GetCommentTypeFilter(request);
            if (response.IsValid)
            {
                result.AddRange(response.CommentType.Select(item => new ListItemViewModel
                {
                    Value = item.Key, Text = item.Value
                }));
            }

            return result;
        }

        public CommentsResponse GetFiltersComment(int instanceId, bool isOpcAgreement, FilterCommentViewModel filter)
        {
            CommentsRequest request = new CommentsRequest
            {
                InstanceId = instanceId,
                IsOpcAgreement = isOpcAgreement,
                TopicCommentId = filter.TopicCommentId,
                CommentTypeId = filter.CommentTypeId,
                MyComments = filter.MyComments,
                OrganizationalUnitId = filter.OrganizationalUnitId,
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo,
                UserName = filter.UserName
            };

            return _commentService.GetFiltersComment(request);
        }

        public ResponseBase MakePublicAnswer(ValidateMinuteRequest request)
        {
            return _commentService.MakePublicAnswer(request);
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