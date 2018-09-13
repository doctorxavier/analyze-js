using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Business.SupervisionPlan.Services.Administration;
using IDB.MW.Business.SupervisionPlan.ViewModels.Administration;
using IDB.MW.Business.SupervisionPlan.Messages;
using IDB.Architecture.Services;
using IDB.MW.Application.SupervisionPlanModule.Services;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class ViewModelMapperHelper
    {
        #region
        private readonly ICatalogService _catalogService;
        private readonly ISPAdministrationService _spAdministrationService;
        private readonly ISpNotificationService _spNotificationService;
        #endregion

        public ViewModelMapperHelper(ISPAdministrationService spAdministrationService, ICatalogService catalogService, ISpNotificationService spNotificationService)
        {
            _catalogService = catalogService;
            _spAdministrationService = spAdministrationService;
            _spNotificationService = spNotificationService;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
                }).ToList();
            }

            return list;
        }

        public GetSupervisionPlanModalityResponse GetSupervisionPlanModality(string idOperation)
        {
            var service = _spAdministrationService.GetSupervisionPlanModality(idOperation);
            return service;
        }

        public bool SaveSupervisionPlanModality(SPSaveViewModel dataSave, string role)
        {
            var response = false;
            var listSP = new List<TableSupervisionPlanModalityViewModel>();
            var supervisionPlan = new TableSupervisionPlanModalityViewModel();
            try
            {
                response = true;
                _spAdministrationService.SaveSupervisionPlanModality(new TableSupervisionPlanModalityViewModel
                {
                    Role = role,
                    OperationId = _spAdministrationService.GetOperationIdByOperationNumber(dataSave.OperationNumber),
                    ModalityId = Convert.ToInt32(dataSave.Value)
                });
            }
            catch (Exception)
            {
                response = false;
                throw;
            }

            return response;
        }

        public bool DeleteSupervisionPlanModality(string operationNumber)
        {
            var response = _spAdministrationService.DeleteSupervisionPlanModality(operationNumber).IsValid;

            return response;
        }

        public UpdateSupervisionPlanModalityResponse UpdateSupervisionPlanModality(SPSaveViewModel dataSave, string role)
        {
            var response = new UpdateSupervisionPlanModalityResponse();
            try
            {
                var operationId = _spAdministrationService.GetOperationIdByOperationNumber(dataSave.OperationNumber);
                response = _spAdministrationService.UpdateSupervisionPlanModality(new TableSupervisionPlanModalityViewModel
                {
                    OperationId = operationId,
                    Role = role,
                    ModalityId = Convert.ToInt32(dataSave.Value)
                });
            }
            catch (Exception)
            {
                response.IsValid = false;
                throw;
            }

            return response;
        }

        public bool CreateSupervisionPlanStatus(string operationNumber, int currentPeriod, string role)
        {      
            return _spAdministrationService.CreateSupervisionPlanStatus(operationNumber, currentPeriod, role);
        }

        public bool CreateSupervisionPlanStatusMass(List<string> listSPSaveVM, int currentPeriod, string role)
        {
            return _spAdministrationService.CreateSupervisionPlanStatusMassive(listSPSaveVM, currentPeriod, role);
        }

        public bool UpdateSupervisionPlanStatus(string operationNumber, int currentPeriod, string role)
        {
            var response = false;
            _spAdministrationService.UpdateSupervisionPlanStatus(operationNumber, currentPeriod, role);
            return response;
        }

        public int CreateNewSupervisionPlan(string operationNumber, int currentYear)
        {
            var response = _spAdministrationService.CreateNewSupervisionPlan(operationNumber, currentYear);
            return response;
        }

        public List<int> CreateNewSupervisionPlanMass(List<string> operationNumber, int currentYear)
        {
            var response = _spAdministrationService.CreateNewSupervisionPlanMassive(operationNumber, currentYear);
            return response;
        }

        public int CreateSupervisionPlanNewVersion(int supervisionPlanId, int modalityId)
        {
            var response = _spAdministrationService.CreateSupervisionPlanNewVersion(supervisionPlanId, modalityId);
            return response;
        }

        public bool CopyCriticalProducts(int supervisionPlanVersionIdLast, int supervisionPlanVersionId, string userName)
        {
            return _spAdministrationService.CopyCriticalProducts(supervisionPlanVersionIdLast, supervisionPlanVersionId, userName);
        }
        
        public int GetSupervisionPlanVersionModalityLast(int SupervisionPlanId)
        {
            return _spAdministrationService.GetSupervisionPlanVersionModalityLast(SupervisionPlanId);
        }

        public int GetSupervisionPlanVersionIdLast(int SupervisionPlanId)
        {
            return _spAdministrationService.GetSupervisionPlanVersionIdLast(SupervisionPlanId);
        }

        public Dictionary<int, string> GetItemsCodeMasterData(string type)
        {
            Dictionary<int, string> list = null;
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection
                    .Select(o => new { o.MasterId, o.Code })
                    .ToDictionary(o => o.MasterId, o => o.Code);
                list.Add(0, string.Empty);
            }

            return list;
        }

        public BaseResponse SendMessageTeamLeader(List<string> operationNumber, bool isUpdate = false)
        {
            return _spNotificationService.SendMessageTeamLeader(operationNumber, isUpdate);
        }

        public BaseResponse SendMessageCountryRepresentative(List<string> operationNumber, bool isCreation = true)
        {
            return _spNotificationService.SendMessageCountryRepresentative(operationNumber, isCreation);
        }

        public BaseResponse SendMessageChiefOperations(List<string> operationNumber, bool isCreation = true)
        {
            return _spNotificationService.SendMessageChiefOperations(operationNumber, isCreation);
        }

        public int GetSupervisionPlanId(string operationNumber, int currentYear)
        {
            return _spAdministrationService.GetSupervisionPlanId(operationNumber, currentYear);
        }

        public List<SelectListItem> GetInputBool()
        {
            var yesNoChoices = new List<SelectListItem>
            {
                new SelectListItem { Text = "Yes", Value = "1" },
                new SelectListItem { Text = "No", Value = "0" }
            };

            return yesNoChoices;
        }

        public BaseResponse SendMessageChiefOperationsBySecurityGroup(List<string> securityGroup, bool isCreation = true)
        {
            return _spNotificationService.SendMessageChiefOperationsBySecurityGroup(securityGroup, isCreation);
        }

        public BaseResponse SendMessageCountryRepresentativeBySecurityGroup(List<string> securityGroups, bool isCreation = true)
        {
            return _spNotificationService.SendMessageCountryRepresentativeBySecurityGroup(securityGroups, isCreation);
        }

        public List<SelectListItem> GetUserRoleList()
        {
            var userRoleList = new List<SelectListItem>
            {
                new SelectListItem { Text = SpGlobalValues.ChiefofOperations, Value = SpGlobalValues.ChiefofOperations },
                new SelectListItem { Text = "Country Representative", Value = SpGlobalValues.CountryRepresentative }
            };

            return userRoleList;
        }
    }
}