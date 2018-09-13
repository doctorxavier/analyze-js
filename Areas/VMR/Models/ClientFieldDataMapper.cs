using System.Linq;
using System;
using System.Collections.Generic;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Application.VMRModule.ViewModels;
using IDB.MW.Business.VMRModule.DTOs.AdditionalFieldsModels;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using Microsoft.Ajax.Utilities;
using IDB.MW.Domain.Values.Ver;

namespace IDB.Presentation.MVC4.Areas.VMR.Models
{
    public static class ClientFieldDataMapper
    {
        public static void UpdateParticipantViewModel(this VmrParticipantViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var participantsTypeList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("participantType"))
                .ToList();
            var participantsRoleList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("role"))
                .ToList();
            var participantsUsernameList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("username")).ToList();
            var participantsRowList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("participantRowId")).ToList();
            var isRequiredList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("isRequired")).ToList();
            var participantsOrganizationalUnitList =
               clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("organizationalUnitId")).ToList();
            var accessLevelList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("accessLevelId")).ToList();

            viewModel.Participants = (from participantType in participantsTypeList
                                      join role in participantsRoleList
                                          on participantsTypeList.IndexOf(participantType)
                                          equals participantsRoleList.IndexOf(role)
                                      join username in participantsUsernameList
                                          on participantsTypeList.IndexOf(participantType)
                                          equals participantsUsernameList.IndexOf(username)
                                      join participantRowId in participantsRowList
                                          on participantsTypeList.IndexOf(participantType)
                                          equals participantsRowList.IndexOf(participantRowId)
                                      join isRequired in isRequiredList
                                          on participantsTypeList.IndexOf(participantType)
                                          equals isRequiredList.IndexOf(isRequired)
                                      join organizationalUnit in participantsOrganizationalUnitList
                                          on participantsTypeList.IndexOf(participantType)
                                          equals participantsOrganizationalUnitList.IndexOf(organizationalUnit)
                                      join accessLevel in accessLevelList on participantsTypeList.IndexOf(participantType) equals
                                          accessLevelList.IndexOf(accessLevel)
                                      select new VmrParticipantRowViewModel
                                      {
                                          ParticipantTypeId = int.Parse(participantType.Value),
                                          Role = string.IsNullOrEmpty(role.Value) ? (int?)null : int.Parse(role.Value),
                                          Username = username.Value,
                                          RowId = int.Parse(participantRowId.Value),
                                          IsRequired = bool.Parse(isRequired.Value),
                                          OrganizationalUnitId = string.IsNullOrEmpty(organizationalUnit.Value) ? (int?)null : int.Parse(organizationalUnit.Value),
                                          AccessLevelId = int.Parse(accessLevel.Value)
                                      }).ToList();
        }

        public static void UpdateDocumentViewModel(this VerDocumentViewModel viewModel, 
                                                        ClientFieldData[] clientFieldData)
        {
            var documentsForDelete = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                                      o.Name.Equals(
                                          VerGlobalValues.DOCUMENTS_FOR_DELETE));

            viewModel.DocumentForDelete = documentsForDelete != null &&
                                          string.IsNullOrEmpty(documentsForDelete.Value) == false
                ? documentsForDelete.Value.Split('|').Select(int.Parse).ToList()
                : new List<int>();

            var documentsRowId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.VER_DOCUMENT_ID))
                .ToList();
            var documentsId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_ID))
                .ToList();
            var documentsNumber = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_NUMBER))
                .ToList();
            var documentsName = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_NAME))
                .ToList();
            var documentsType = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_TYPE))
                .ToList();
            var documentUrl = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.SHP_URL_DOCUMENT))
                .ToList();
            var documentIsPrimary = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_PRIMARY))
                .ToList();
            var documentsIsDocPackage = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.PACKAGE_DOC))
                .ToList();
            var documentsIsGenerated = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_GENERATED))
                .ToList();
            var documentIsRequired = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_REQUIRED))
                .ToList();
            var documentIsVer = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_VER))
                .ToList();
            var isOtherList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_OTHER))
                .ToList();
            var isVersionHistoryList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_VERSION_HISTORY))
                .ToList();
            var isDocumentPackageList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.IS_DOCUMENT_PACKAGE))
                .ToList();
            var isNewDocumentList = clientFieldData
               .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                           o.Name.Equals(VerGlobalValues.IS_NEW_DOCUMENT))
               .ToList();
            var documentNameTempList = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(VerGlobalValues.DOCUMENT_NAME_TEMP))
                .ToList();

            viewModel.Documents = (from verDocumentId in documentsRowId
                join docId in documentsId
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsId.IndexOf(docId)
                join docNumber in documentsNumber
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsNumber.IndexOf(docNumber)
                join docName in documentsName
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsName.IndexOf(docName)
                join docType in documentsType
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsType.IndexOf(docType)
                join shpUrl in documentUrl
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentUrl.IndexOf(shpUrl)
                join isPrimary in documentIsPrimary
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentIsPrimary.IndexOf(isPrimary)
                join isDocument in documentsIsDocPackage
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsIsDocPackage.IndexOf(isDocument)
                join isGenerated in documentsIsGenerated
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentsIsGenerated.IndexOf(isGenerated)
                join isRequired in documentIsRequired
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentIsRequired.IndexOf(isRequired)
                join isVer in documentIsVer
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentIsVer.IndexOf(isVer)
                join isOther in isOtherList
                    on documentsRowId.IndexOf(verDocumentId)
                    equals isOtherList.IndexOf(isOther)
                join isVersionHistory in isVersionHistoryList
                    on documentsRowId.IndexOf(verDocumentId)
                    equals isVersionHistoryList.IndexOf(isVersionHistory)
                join isDocumentPackage in isDocumentPackageList
                    on documentsRowId.IndexOf(verDocumentId)
                    equals isDocumentPackageList.IndexOf(isDocumentPackage)
                join isNewDocument in isNewDocumentList
                    on documentsRowId.IndexOf(verDocumentId)
                    equals isNewDocumentList.IndexOf(isNewDocument)
                join documentNameTemp in documentNameTempList
                    on documentsRowId.IndexOf(verDocumentId)
                    equals documentNameTempList.IndexOf(documentNameTemp)
                select new VerDocumentRowViewModel
                {
                    VerDocumentId = Convert.ToInt32(verDocumentId.Value),
                    DocumentNumber = docNumber.Value,
                    DocumentType = int.Parse(docType.Value),
                    DocumentName = docName.Value,
                    DocumentId = string.IsNullOrEmpty(docId.Value) ? (int?)null : Convert.ToInt32(docId.Value),
                    IsPrimary = Convert.ToBoolean(isPrimary.Value),
                    ShpUrlDocument = shpUrl.Value,
                    IncInPackage = Convert.ToBoolean(isDocument.Value),
                    IsGenerated = Convert.ToBoolean(isGenerated.Value),
                    IsRequired = Convert.ToBoolean(isRequired.Value),
                    IsVer = isVer.Value != null && bool.Parse(isVer.Value),
                    IsOther = isOther.Value != null && bool.Parse(isOther.Value),
                    IsVersionHistory = Convert.ToBoolean(isVersionHistory.Value),
                    IsDocumentPackage = isDocumentPackage.Value != null && bool.Parse(isDocumentPackage.Value),
                    IsNewDocument = isNewDocument.Value != null && bool.Parse(isNewDocument.Value),
                    DocumentNameTemp = documentNameTemp.Value
                }).ToList();
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
    }
}
