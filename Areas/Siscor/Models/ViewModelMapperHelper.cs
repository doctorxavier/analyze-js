using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.SiscorModule.ViewModels;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;
using IDB.MW.Domain.Contracts.ModelRepositories.Signature;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.SiscorModule.Services;
using IDB.MW.Application.SiscorModule.Helpers;
using IDB.Architecture.Logging;
using IDB.MW.DomainModel.Contracts.Repositories.SiscorModule;
using IDB.MW.Infrastructure.Data.Repositories.SiscorModule;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.IOCManager;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.Siscor.Models
{
    public class ViewModelMapperHelper
    {
        private readonly ICatalogService _catalogService;
        private readonly ISignatureModelRepository _signatureModelRepository;
        private readonly IProxySiscorService _proxySiscorService;
        private readonly ICorrespondenceService _correspondenceService;

        public ViewModelMapperHelper(ICatalogService catalogService,
            ISignatureModelRepository signatureModelRepository,
            IProxySiscorService proxySiscorService,
            ICorrespondenceService correspondenceService)
        {
            _catalogService = catalogService;
            _signatureModelRepository = signatureModelRepository;
            _proxySiscorService = proxySiscorService;
            _correspondenceService = correspondenceService;
        }

        public List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Code,
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        public List<SelectListItem> GetContracts(string operation)
        {
            var contracts = new List<SelectListItem>();

            var contractsPrev = SiscorHelper.GetContractsByOperationNumber(operation);

            foreach (var contract in contractsPrev)
            {
                var item = new SelectListItem
                {
                    Selected = false,
                    Value = contract.ContractId.ToString(),
                    Text = contract.ContractNumber,
                };

                contracts.Add(item);
            }

            return contracts;
        }

        public List<SiscorNumberViewModel> GetSiscorNumberList(string[] selects)
        {
            List<SiscorNumberViewModel> siscorNumberList = new List<SiscorNumberViewModel>();

            foreach (string siscorNumber in selects)
            {
                SiscorNumberViewModel sn = new SiscorNumberViewModel();
                sn.Year = Convert.ToInt32(siscorNumber.Substring(siscorNumber.Length - 4, 4));
                string withoutYearAndBar = siscorNumber.Remove(siscorNumber.Length - 5, 5);

                char[] separador = new char[1]
                {
                    '-'
                };

                string[] parts = withoutYearAndBar.Split(separador);
                sn.DocType = parts[0];
                sn.Division = parts[1];
                sn.Number = Convert.ToInt32(parts[2]);
                siscorNumberList.Add(sn);
            }

            return siscorNumberList;
        }

        public List<SelectListItem> GetListTemplate(string countryCode)
        {
            var list = new List<SelectListItem>();
            var listTemplate = _proxySiscorService.GetTemplatesByCountryOperation(countryCode);

            if (listTemplate != null && listTemplate.Templates.Count > 0)
            {
                list = listTemplate.Templates.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.TemplateNumber,
                    Text = o.TemplateName,
                    Group = new SelectListGroup
                    {
                        Name = o.CountryCode
                    }
                }).ToList();
            }

            foreach (var s in list)
            {
                SiscorHelper.SetTransactionPrefix("ViewModelmapperHelper - GetListTemplate");
                Logger.GetLogger().WriteDebug(SiscorHelper.TransactionPrefix,
                    string.Format(@" list.CountryCode : {0} ", s.Group.Name));
            }

            return list;
        }

        public GetUserNameListResponse GetSignBy(string filter)
        {
            var response = new GetUserNameListResponse();
            var list = new List<ListItemViewModel>();

            try
            {
                var listUser = _signatureModelRepository.GetSignatures(filter, string.Empty);

                foreach (var u in listUser)
                {
                    list.Add(new ListItemViewModel
                    {
                        Text = u.UserId,
                        Value = u.UserSignatureId.ToString(),
                    });
                }

                response.ListResponse = list;
                response.IsValid = true;
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.Message;
                throw;
            }

            return response;
        }

        public List<SiscorNumberViewModel> GetSiscorNumbersInputs(string siscorNumberOutput)
        {
            ISiscorCorrespondenceRepository siscorCorrespondenceRepository = new SiscorCorrespondenceRepository();
            var siscorNumber = SiscorHelper.ConvertStringToSiscorNumber(siscorNumberOutput);

            var siscorCorrespondenceOutput = siscorCorrespondenceRepository.GetOne(x => x.SiscorDocumentType.Code == siscorNumber.DocType
                   && x.SiscorNumber == siscorNumber.Number && x.SiscorDivision == siscorNumber.Division && x.SiscorYear == siscorNumber.Year);

            var siscorCorrespondenceInputs = siscorCorrespondenceRepository.GetByCriteria(x => x.OutputSiscorCorrespondenceId == siscorCorrespondenceOutput.SiscorCorrespondenceId);

            var siscornumbers = new List<SiscorNumberViewModel>();

            siscornumbers.Add(siscorNumber);
            foreach (var item in siscorCorrespondenceInputs)
            {
                var siscorNumberI = SiscorHelper.GetSiscorNumber(_catalogService, item.SiscorDocumentType.Code, item.SiscorDivision, item.SiscorNumber, item.SiscorYear);
                siscornumbers.Add(siscorNumberI);
            }

            return siscornumbers;
        }

        public TemplateViewModel GetTemplateTextByCode(string templateCode, string countryCode)
        {
            var templateResponse = new TemplateViewModel();
            var list = new List<SelectListItem>();
            list = GetListTemplate(countryCode);

            foreach (var item in list)
            {
                if (templateCode == item.Value)
                {
                    templateResponse.TemplateName = item.Text;
                }
            }

            return templateResponse;
        }

        public List<SelectListItem> ListTemplate(List<SelectListItem> GetListTemplate)
        {
            var list = new List<SelectListItem>();

            foreach (var template in GetListTemplate)
            {
                var item = new SelectListItem
                {
                    Selected = false,
                    Value = template.Value,
                    Text = template.Text
                };

                list.Add(item);
            }

            return list;
        }

        public GetSelectListItemResponse GetOperationNumberList(string search)
        {
            GetSelectListItemResponse response = new GetSelectListItemResponse();
            response.ListResponse = new List<SelectListItem>();
            IOperationRepository operationRepository = IOCManagerFactory.Current.Resolve<IOperationRepository>();
            var ListOperations = new List<MW.Domain.Entities.Operation>();

            if (!string.IsNullOrEmpty(search))
            {
                ListOperations = operationRepository.GetByCriteria(o => o.OperationNumber.Contains(search)).ToList();
            } 

            if (ListOperations != null && ListOperations.Any())
            {
                if (ListOperations.Any())
                {
                    response.IsValid = true;
                    response.ListResponse = ListOperations.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = o.OperationId.ToString(),
                        Text = o.OperationNumber,
                    }).ToList();
                }
            }

            return response;
        }

        public List<SelectListItem> GetCountryOrRelated(string operationNumber, string lang, string type)
        {
            var responseCountries = _correspondenceService.GetRegionalCountry(operationNumber, lang);
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(typeCodes: type);
            if (!string.IsNullOrWhiteSpace(operationNumber))
            {
                var operation = SiscorHelper.GetOperation(operationNumber);
                if (operation.CountryRelated.Count > 0)
                {
                    var rolAdminId = _catalogService.GetConvergenceMasterDataIdByCode("CTY_ADMIN", "CTY_ROLE").Id;
                    var countryAdmin = operation.CountryRelated.Where(c => c.RoleId == rolAdminId).FirstOrDefault();
                    list = responseCountries.RegionalCountries.Select(o => new SelectListItem
                    {
                        Selected = o.Code == countryAdmin.Country.Code,
                        Value = _correspondenceService.GetCountryCode(o.Code),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                    }).ToList();
                }
                else
                {
                    list = responseCountries.RegionalCountries.Select(o => new SelectListItem
                    {
                        Selected = false,
                        Value = _correspondenceService.GetCountryCode(o.Code),
                        Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                    }).ToList();
                }
            }

            return list;
        }
    }
}