using System;
using System.Linq;
using System.Collections.Generic;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Business.VMRModule.DTOs;
using IDB.MW.Domain.Values.Ver;
using IDB.MW.Business.VMRModule.DTOs.AdditionalFieldsModels;
using Microsoft.Ajax.Utilities;

namespace IDB.Presentation.MVC4.Areas.VER.Models
{
    public static class ClientFieldDataMapper
    {
        public static void UpdateParticipantViewModel(this VerParticipantViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var participantsTypeList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PARTICIPANT_TYPE))
                .ToList();
            var participantsRoleList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PARTICIPANT_ROLE))
                .ToList();
            var participantsusernameList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PARTICIPANT_USERNAME))
                .ToList();
            var participantsEmailList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PARTICIPANT_EMAIL))
                .ToList();
            var participantsOfficeList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.OFFICE_PERMISSIONS))
                .ToList();
            var participantsRowList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PARTICIPANT_ROW_ID))
                .ToList();
            var participantIsRequiredList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_REQUIRED))
                .ToList();
            var participantsOrganizationalUnitList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.ORGANIZATIONAL_UNIT_ID))
                .ToList();
            var participantsAccessLevelList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.ACCESS_LEVEL_ID))
                .ToList();

            viewModel.Participants = (from participantType in participantsTypeList
                join role in participantsRoleList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsRoleList.IndexOf(role)
                join username in participantsusernameList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsusernameList.IndexOf(username)
                join email in participantsEmailList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsEmailList.IndexOf(email)
                join office365Permissions in participantsOfficeList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsOfficeList.IndexOf(office365Permissions)
                join participantRorwId in participantsRowList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsRowList.IndexOf(participantRorwId)
                join participantRequired in participantIsRequiredList
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantIsRequiredList.IndexOf(participantRequired)
                join participantsOrganizationalUnit in participantsOrganizationalUnitList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsOrganizationalUnitList.IndexOf(participantsOrganizationalUnit)
                join participantsAccessLevel in participantsAccessLevelList 
                    on participantsTypeList.IndexOf(participantType) 
                    equals participantsAccessLevelList.IndexOf(participantsAccessLevel)
                select new VerParticipantRowViewModel
                    {
                        ParticipantTypeId = int.Parse(participantType.Value),
                        Role = string.IsNullOrEmpty(role.Value) 
                        ? (int?)null 
                        : int.Parse(role.Value),
                        UserName = username.Value,
                        Email = string.IsNullOrEmpty(email.Value) ? null : email.Value,
                        Office365 = string.IsNullOrEmpty(office365Permissions.Value) 
                        ? 0
                        : Convert.ToInt32(office365Permissions.Value),
                        RowId = int.Parse(participantRorwId.Value),
                        IsRequired = bool.Parse(participantRequired.Value),
                        OrganizationalUnitId = string.IsNullOrEmpty(participantsOrganizationalUnit.Value) 
                        ? (int?)null 
                        : int.Parse(participantsOrganizationalUnit.Value),
                        AccessLevelId = int.Parse(participantsAccessLevel.Value),
                    }).ToList();
        }

        public static void UpdateDocumentViewModel(this VerDocumentViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var documentsForDelete = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                                      o.Name.Equals(
                                          VerGlobalValues.DOCUMENTS_FOR_DELETE));

            viewModel.DocumentForDelete = documentsForDelete != null &&
                                          string.IsNullOrEmpty(documentsForDelete.Value) == false
                ? documentsForDelete.Value.Split('|').Select(int.Parse).ToList()
                : new List<int>();

            var updatedCheckClearance = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(VerGlobalValues.UPDATED_CHECK_CLEARANCE))
                .ToList();
            var docToSubmitTranslationList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(VerGlobalValues.SUBMIT_TRANSLATION))
                .ToList();
            var documentsRowId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.VER_DOCUMENT_ID))
                .ToList();
            var documentsId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.DOCUMENT_ID))
                .ToList();
            var documentsNumber = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.DOCUMENT_NUMBER))
                .ToList();
            var documentsName = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.DOCUMENT_NAME))
                .ToList();
            var documentIsPrimary = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.IS_PRIMARY))
                .ToList();
            var documentsIsDocPackage = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.PACKAGE_DOC))
                .ToList();
            var documentsIsGenerated = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.IS_GENERATED))
                .ToList();
            var documentIsRequired = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.IS_REQUIRED))
                .ToList();
            var documentPrimaryUrl =
                clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.SHP_URL_DOCUMENT))
                .ToList();
            var documentTypeId =
                clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.DOCUMENT_TYPE))
                .ToList();
            var documentIsVer = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.IS_VER))
                .ToList();
            var documentIsPublish = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.PUBLISH_DOC))
                .ToList();
            var isVisibleChkPublishList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(VerGlobalValues.IS_VISIBLE_CHK_PUBLISH))
                .ToList();
            var isOtherList = clientFieldData
               .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                   o.Name.Equals(VerGlobalValues.IS_OTHER))
               .ToList();
            var checkClearancePdf = clientFieldData
               .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                   o.Name.Equals(VerGlobalValues.IS_CHECK_CLEARANCE_PDF))
                .ToList();
            var isVersionHistoryList = clientFieldData
               .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_VERSION_HISTORY))
               .ToList();
            var isNewDocumentList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                            o.Name.Equals(VerGlobalValues.IS_NEW_DOCUMENT))
                .ToList();
            var documentNameTempList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_NAME_TEMP))
                .ToList();

            viewModel.Documents = 
                (from verDocumentId in documentsRowId
                    join docId in documentsId
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentsId.IndexOf(docId)
                    join docNumber in documentsNumber
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentsNumber.IndexOf(docNumber)
                    join docName in documentsName
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentsName.IndexOf(docName)
                    join isPrimary in documentIsPrimary
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentIsPrimary.IndexOf(isPrimary)
                    join isDocument in documentsIsDocPackage
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentsIsDocPackage.IndexOf(isDocument)
                    join isGeneratedAnnex in documentsIsGenerated
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentsIsGenerated.IndexOf(isGeneratedAnnex)
                    join isRequired in documentIsRequired
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentIsRequired.IndexOf(isRequired)
                    join shpUrl in documentPrimaryUrl
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentPrimaryUrl.IndexOf(shpUrl)
                    join documentType in documentTypeId
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentTypeId.IndexOf(documentType)
                    join documentVer in documentIsVer
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentIsVer.IndexOf(documentVer)
                    join isPublish in documentIsPublish
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentIsPublish.IndexOf(isPublish)
                    join docToSubmitTranslate in docToSubmitTranslationList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals docToSubmitTranslationList.IndexOf(docToSubmitTranslate)
                    join updatedCheckForClearance in updatedCheckClearance
                        on documentsRowId.IndexOf(verDocumentId)
                        equals updatedCheckClearance.IndexOf(updatedCheckForClearance)
                    join isVisibleChkPublish in isVisibleChkPublishList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals isVisibleChkPublishList.IndexOf(isVisibleChkPublish)
                    join isOther in isOtherList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals isOtherList.IndexOf(isOther)
                    join isVersionHistory in isVersionHistoryList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals isVersionHistoryList.IndexOf(isVersionHistory)
                    join checkClearancePdfItem in checkClearancePdf
                        on documentsRowId.IndexOf(verDocumentId)
                        equals checkClearancePdf.IndexOf(checkClearancePdfItem)
                    join isNewDocument in isNewDocumentList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals isNewDocumentList.IndexOf(isNewDocument)
                     join documentNameTemp in documentNameTempList
                        on documentsRowId.IndexOf(verDocumentId)
                        equals documentNameTempList.IndexOf(documentNameTemp)
                 select new VerDocumentRowViewModel
                    {
                        VerDocumentId = int.Parse(verDocumentId.Value),
                        DocumentNumber = docNumber.Value,
                        DocumentName = docName.Value,
                        DocumentId = string.IsNullOrEmpty(docId.Value) ? (int?)null : int.Parse(docId.Value),
                        IsPrimary = Convert.ToBoolean(isPrimary.Value),
                        IncInPackage = Convert.ToBoolean(isDocument.Value),
                        IsGenerated = Convert.ToBoolean(isGeneratedAnnex.Value),
                        IsRequired = Convert.ToBoolean(isRequired.Value),
                        ShpUrlDocument = shpUrl.Value,
                        DocumentType = int.Parse(documentType.Value),
                        IsVer = documentVer.Value != null && bool.Parse(documentVer.Value),
                        IncInPublish = Convert.ToBoolean(isPublish.Value),
                        IsSubmittedForTranslation = Convert.ToBoolean(docToSubmitTranslate.Value),
                        UpdatedCheckForClearance = Convert.ToBoolean(updatedCheckForClearance.Value),
                        IsVisibleChkPublish = Convert.ToBoolean(isVisibleChkPublish.Value),
                        IsOther = Convert.ToBoolean(isOther.Value),
                        IsVersionHistory = Convert.ToBoolean(isVersionHistory.Value),
                        IsCheckClearancePdf = Convert.ToBoolean(checkClearancePdfItem.Value),
                        IsNewDocument = Convert.ToBoolean(isNewDocument.Value),
                        DocumentNameTemp = documentNameTemp.Value
                 }).ToList();
        }

        public static void UpdateTaskViewModel(this TaskTableViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var tasksId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("IdRowTask"))
                .ToList();
            var tasksTypeActivity = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("activityType"))
                .ToList();
            var tasksTypeParticipant = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("roleTasks"))
                .ToList();
            var tasksNameParticipants = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("CurrentRowParticipant"))
                .ToList();
            var tasksDueDate = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("dueDate"))
                .ToList();
            var tasksNotify = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("chkNotify"))
                .ToList();
            var tasksInstrunctions = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("txtInstrunctions"))
                .ToList();
            var newTaskParticipantIds = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewTaskParticipantId"))
                .ToList();
            var taskStatus = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("hddStatus"))
                .ToList();
            var external = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("IsExternal"))
                .ToList();

            viewModel.Tasks = (from typeActivity in tasksTypeActivity
                join taskTypeParticipant in tasksTypeParticipant
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksTypeParticipant.IndexOf(taskTypeParticipant)
                join taskNameParticipants in tasksNameParticipants
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksNameParticipants.IndexOf(taskNameParticipants)
                join taskDueDate in tasksDueDate
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksDueDate.IndexOf(taskDueDate)
                join taskId in tasksId
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksId.IndexOf(taskId)
                join taskInstrunction in tasksInstrunctions
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksInstrunctions.IndexOf(taskInstrunction)
                join taskNotify in tasksNotify
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals tasksNotify.IndexOf(taskNotify)
                join newTaskParticipantId in newTaskParticipantIds
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals newTaskParticipantIds.IndexOf(newTaskParticipantId)
                join newtaskStatus in taskStatus
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals taskStatus.IndexOf(newtaskStatus)
                join isExternal in external
                    on tasksTypeActivity.IndexOf(typeActivity)
                    equals external.IndexOf(isExternal)
                select new TaskTableRowViewModel
                {
                    TaskId = int.Parse(taskId.Value),
                    ActivityType = int.Parse(typeActivity.Value),
                    RoleAndOrgUnitId = taskTypeParticipant.Value,
                    ParticipantsNames = ParseClientFieldDataToViewModel(taskNameParticipants),
                    Instructions = taskInstrunction.Value,
                    DueDate =
                        string.IsNullOrEmpty(taskDueDate.Value) ? (DateTime?)null : DateTime.Parse(taskDueDate.Value),
                    Notify = bool.Parse(taskNotify.Value),
                    NewTaskParticipantId = newTaskParticipantId.Value,
                    Status = newtaskStatus.Value,
                    IsExternal = Convert.ToBoolean(isExternal.Value)
                }).ToList();

            viewModel.Tasks = viewModel.Tasks.CheckNewRowsTaksList();
        }

        public static List<TaskTableRowViewModel> CheckNewRowsTaksList(this List<TaskTableRowViewModel> tasks)
        {
            var listTaskNewRows = tasks.Where(o => o.NewTaskParticipantId != "0").ToList();

            if (listTaskNewRows.Any())
            {
                tasks = tasks.Where(o => o.NewTaskParticipantId == "0").ToList();

                foreach (var newTask in listTaskNewRows)
                {
                    var newTaskIds = newTask.NewTaskParticipantId.Split(',').ToList();
                    tasks.AddRange(newTaskIds.Select(taskid => new TaskTableRowViewModel
                    {
                        TaskId = int.Parse(taskid),
                        ActivityType = newTask.ActivityType,
                        Description = newTask.Description,
                        RoleAndOrgUnitId = newTask.RoleAndOrgUnitId,
                        ParticipantsNames = newTask.ParticipantsNames,
                        Instructions = newTask.Instructions,
                        DueDate = newTask.DueDate,
                        Notify = newTask.Notify,
                        ParticipantsComment = newTask.ParticipantsComment,
                        NewTaskParticipantId = taskid
                    }));
                }
            }

            return tasks;
        }

        public static void UpdateVerTaskViewModel(this TaskViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<UserCommentViewModel>();
            }

            var editComments = clientFieldData.Where(o => o.Name.Equals("textComment"));
            var editCommentsId = clientFieldData.Where(o => o.Name.Equals("commentId"));

            viewModel.UserComments = MapperEditComments(viewModel.UserComments, editComments, editCommentsId);

            var newComments = clientFieldData.Where(o => o.Name.Equals("newComment"));

            viewModel.UserComments = MapperNewComment(viewModel.UserComments, newComments);

            var deleteComments = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteComments"));

            if (deleteComments != null)
            {
                string[] deleteC = deleteComments.Value.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (viewModel.DeleteComments == null)
                {
                    viewModel.DeleteComments = new List<int>();
                }

                foreach (string s in deleteC)
                {
                    viewModel.DeleteComments.Add(Convert.ToInt32(s));
                    viewModel.UserComments.RemoveAll(d => d.CommentId.Equals(Convert.ToInt32(s)));
                }
            }
        }

        public static List<VerUserViewModel> ParseClientFieldDataToViewModel(ClientFieldData dataClient)
        {
            var participantNames = new List<VerUserViewModel>();

            string[] idParticipants = dataClient.Value == null ? new[] { string.Empty } : dataClient.Value.Split(',');

            foreach (var participant in idParticipants)
            {
                int participantId;
                var isNumber = int.TryParse(participant, out participantId);
                var verUserViewModel = new VerUserViewModel
                {
                    IdUserName = isNumber ? participantId.ToString() : null, 
                    Username = isNumber ? null : participant
                };
                participantNames.Add(verUserViewModel);
            }

            return participantNames;
        }

        public static PolicyWaiverViewModel UpdatePolicyWaiverViewModel(
            this PolicyWaiverViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var groupClientField = clientFieldData
                .Where(o => string.IsNullOrEmpty(o.Id) == false)
                .GroupBy(x => x.Id)
                .ToList();

            var policyForDeleted = clientFieldData.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_FOR_DELETE);
            var requestPolicy = clientFieldData.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_REQUEST_POLICY);
            var sendPolicy = clientFieldData.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_SEND_RESPONSE_POLICY);
            var opcType = clientFieldData.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_OPC_TYPE);

            viewModel.IsRequested = requestPolicy != null && requestPolicy.Value == "true";
            viewModel.IsResponded = sendPolicy != null && sendPolicy.Value == "true";
            int opcTypeId;
            if (opcType != null && int.TryParse(opcType.Value, out opcTypeId))
            {
                viewModel.OpcTypeId = opcTypeId;
            }
            
            if (policyForDeleted != null && string.IsNullOrEmpty(policyForDeleted.Value) == false)
            {
                var policyForDeleteList = policyForDeleted.Value.Split('|').ToList();
                viewModel.PolicyWaiverList.ForEach(
                    o => o.IsDeleted =
                        policyForDeleteList.Any(x => x == o.OperationPolicyWaiverId.ToString()));
            }

            foreach (var group in groupClientField)
            {
                var register =
                    viewModel.PolicyWaiverList
                        .FirstOrDefault(o => o.OperationPolicyWaiverId.ToString() == group.Key);

                var fieldList = group.ToList();

                if (register != null)
                {
                    ProcessRegisterPolicyWaiver(register, fieldList, viewModel.IsRequested, viewModel.IsResponded);
                }
                else
                {
                    ProcessNewPolicyWaiver(viewModel, fieldList, viewModel.IsRequested);
                }
            }

            return viewModel;
        }

        public static IList<AdditionalFieldsSaveModel> SetAdditionalFieldForm(
            this ClientFieldData[] clientData)
        {
            var additionalFieldsList = new List<AdditionalFieldsSaveModel>();

            if (clientData != null && clientData.Any())
            {
                additionalFieldsList = clientData.DistinctBy(o => o.Name)
                    .Where(x =>
                      !string.IsNullOrWhiteSpace(x.Id) &&
                      !string.IsNullOrWhiteSpace(x.Name) &&
                      !string.IsNullOrWhiteSpace(x.Value))
                    .Select(o => new AdditionalFieldsSaveModel
                    {
                        Name = o.Name,
                        Value = o.Value,
                        Type = o.Id
                    }).ToList();
            }

            return additionalFieldsList;
        }

        #region Private

        private static List<UserCommentViewModel> MapperEditComments(List<UserCommentViewModel> userComments, IEnumerable<ClientFieldData> editComments, IEnumerable<ClientFieldData> editCommentsId)
        {
            if (editComments != null && editCommentsId != null)
            {
                for (int i = 0; i < editComments.Count(); i++)
                {
                    var index = userComments.FindIndex(x => x.CommentId == Convert.ToInt32(editCommentsId.ElementAt(i).Value));

                    if (!userComments.ElementAt(index).Comment.Equals(editComments.ElementAt(i).Value.Trim()))
                    {
                        userComments.ElementAt(index).Comment = editComments.ElementAt(i).Value.Trim();
                    }
                }
            }

            return userComments;
        }

        private static List<UserCommentViewModel> MapperNewComment(List<UserCommentViewModel> userComments, IEnumerable<ClientFieldData> newComments)
        {
            if (newComments != null)
            {
                for (int i = 0; i < newComments.Count(); i++)
                {
                    var comment = new UserCommentViewModel();
                    comment.Comment = newComments.ElementAt(i).Value.Trim();

                    userComments.Add(comment);
                }
            }

            return userComments;
        }

        private static void ProcessNewPolicyWaiver(
            PolicyWaiverViewModel viewModel,
            IList<ClientFieldData> fieldList,
            bool isRequestPolicy)
        {
            var category = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_CATEGORY);
            var policy = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER);
            var policyJustification = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_JUSTIFICATION);
            var status = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_STATUS);

            var newRegister = new PolicyWaiverRowViewModel
            {
                IsAdded = true
            };

            if (category != null)
            {
                newRegister.PolicyWaiverCategory = new FieldModel
                {
                    IntValue = Convert.ToInt32(category.Value),
                    IsEdited = true
                };
            }

            if (policy != null)
            {
                newRegister.PolicyWaiver = new FieldModel
                {
                    TextValue = policy.Value,
                    IsEdited = true
                };
            }

            if (policyJustification != null)
            {
                newRegister.PolicyWaiverJustification = new FieldModel
                {
                    TextValue = policyJustification.Value,
                    IsEdited = true
                };
            }

            if (status != null)
            {
                newRegister.PolicyWaiverStatus = new FieldModel
                {
                    IntValue = Convert.ToInt32(status.Value),
                    IsEdited = true
                };
            }

            if (isRequestPolicy)
            {
                newRegister.IsRequested = true;
            }

            viewModel.PolicyWaiverList.Add(newRegister);
        }

        private static void ProcessRegisterPolicyWaiver(
            PolicyWaiverRowViewModel register,
            IList<ClientFieldData> fieldList,
            bool isRequestPolicy, 
            bool isSendPolicy)
        {
            var category = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_CATEGORY);
            var policy = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER);
            var policyJustification = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_JUSTIFICATION);
            var policyResponse = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_RESPONSE);
            var policyStatus = fieldList.FirstOrDefault(o =>
                o.Name == VerSecurityValues.POLICY_WAIVER_STATUS);

            if (register.PolicyWaiverCategory.IsReadOnly == false &&
                category != null &&
                register.PolicyWaiverCategory.IntValue != Convert.ToInt32(category.Value))
            {
                register.IsEdited = true;
                register.PolicyWaiverCategory.IsEdited = true;
                register.PolicyWaiverCategory.IntValue = Convert.ToInt32(category.Value);
            }

            if (register.PolicyWaiver.IsReadOnly == false &&
                policy != null &&
                register.PolicyWaiver.TextValue != policy.Value)
            {
                register.IsEdited = true;
                register.PolicyWaiver.IsEdited = true;
                register.PolicyWaiver.TextValue = policy.Value;
            }

            if (register.PolicyWaiverJustification.IsReadOnly == false &&
                policyJustification != null &&
                register.PolicyWaiverJustification.TextValue != policyJustification.Value)
            {
                register.IsEdited = true;
                register.PolicyWaiverJustification.IsEdited = true;
                register.PolicyWaiverJustification.TextValue = policyJustification.Value;
            }

            if (register.PolicyWaiverResponse.IsReadOnly == false &&
                policyResponse != null &&
                register.PolicyWaiverResponse.TextValue != policyResponse.Value)
            {
                register.IsEdited = true;
                register.PolicyWaiverResponse.IsEdited = true;
                register.PolicyWaiverResponse.TextValue = policyResponse.Value;
            }

            if (register.PolicyWaiverStatus.IsReadOnly == false &&
                policyStatus != null &&
                register.PolicyWaiverStatus.IntValue != Convert.ToInt32(policyStatus.Value))
            {
                register.IsEdited = true;
                register.PolicyWaiverStatus.IsEdited = true;
                register.PolicyWaiverStatus.IntValue = Convert.ToInt32(policyStatus.Value);
            }

            if (register.IsRequested == false && isRequestPolicy)
            {
                register.IsRequested = true;
                register.IsEdited = true;
            }

            if (register.IsResponded == false && isSendPolicy)
            {
                register.PolicyWaiverResponseDate = DateTime.Now;
                register.IsResponded = true;
                register.IsEdited = true;
            }
        }
        #endregion
    }
}
