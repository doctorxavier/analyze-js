using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.AdministrationModule.Messages.InstitutionService;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Language;
using IDB.MW.Application.OPUSModule.ViewModels.Institutions;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Institution
{
    public class ViewModelMapperHelper
    {
        private readonly dynamic _viewBag;
        private readonly IInstitutionService _institutionService;
        private readonly ICatalogService _catalogService;

        public ViewModelMapperHelper(dynamic viewBag, IInstitutionService institutionService, ICatalogService catalogService)
        {
            // TODO: Complete member initialization
            _viewBag = viewBag;
            _institutionService = institutionService;
            _catalogService = catalogService;
        }

        public GetInstitutionEditResponse GetInstitutionEdit(int institutionId)
        {
            var response = _institutionService.GetInstitutionEdit(institutionId);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetListInstitutionResponse SearchInstitutionListResponse(int searchInstitutionNameId, string searchInstitutionAcronymName, int searchInstitutionCountryId, int searchInstitutionStatusId, int searchInstitutionTypeId, bool searchInstitutionOperationAssigned, string userName)
        {
            var response = _institutionService.GetInstitutionList(searchInstitutionNameId, searchInstitutionAcronymName, searchInstitutionCountryId, searchInstitutionStatusId, searchInstitutionTypeId, searchInstitutionOperationAssigned, userName);

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response;
        }

        public GetSelectListItemResponse GetInstitutionList(string name)
        {
            var response = new GetSelectListItemResponse();

            var institutionRepository = _institutionService.GetInstitutionNameList(name);

            if (institutionRepository.GetListItemInstitution != null && institutionRepository.GetListItemInstitution.Count > 0)
            {
                response.ListResponse = institutionRepository.GetListItemInstitution.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public string GetInstitutionStatus(int institutionId)
        {
            return _institutionService.GetInstitutionStatus(institutionId);
        }

        public GetSelectListItemResponse GetInstitutionAcronymList(string term, string code)
        {
            var response = new GetSelectListItemResponse()
            {
                IsValid = true
            };
            try
            {
                var institutionRepository = _institutionService.GetInstitutionAcronymList(term, code);

                if (institutionRepository.GetListItemInstitution != null && institutionRepository.GetListItemInstitution.Count > 0)
                {
                    response.ListResponse = institutionRepository.GetListItemInstitution.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.Value,
                        Text = o.Text
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        public List<SelectListItem> GetListMasterData(string type, bool isEditMode = false)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService
                .GetMasterDataListByTypeCode(hideExpired: isEditMode, typeCodes: type);

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

        public List<SelectListItem> GetCodeListMasterDataCode(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Code,
                    Text = o.Code
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetListMasterDataStage(string type, string stage)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null && listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Where(x => x.Code.IndexOf(stage, StringComparison.OrdinalIgnoreCase) >= 0).Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<InstitutionsWorkflowDocumentsViewModels> GetWorkFlowInstitutionDocuments(int institutionId)
        {
            var viewDocuments = _institutionService.GetDocuments(institutionId).InstitutionsDocuments;

            return viewDocuments.Select(item => new InstitutionsWorkflowDocumentsViewModels()
            {
                DocNumber = item.Document.DocumentReference,
                Date = item.Document.Created.Value,
                Description = item.Document.Description,
                User = item.Document.CreatedBy,
                documentId = item.DocumentId
            }).ToList();
        }

        #region fields
        #endregion
    }
}