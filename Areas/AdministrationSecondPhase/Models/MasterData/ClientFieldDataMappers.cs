using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.AdministrationModule.ViewModels.MasterData;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.MasterData
{
    public static class ClientFieldDataMappers
    {
        #region Constants

        private const string ROLE_ID = "RoleId";
        private const string COMPONENT_AMOUNT = "ComponentAmount";
        private const string COMPONENT_ID = "ComponentId";
        private const string FUND_AMOUNT = "FundAmount";
        private const string FUND_ID = "FundId";
        private const string ID = "Id";
        private const string SUSTAINABILITY_TYPE_ID = "SustainabilityTypeId";

        #endregion

        #region Mappers

        #region Master Data Save
        public static void UpdateMasterDataViewModel(this MasterDataManagementTableModelView viewModel, ClientFieldData[] clientFieldData)
        {
            var masterDataId = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("managementId"));
            if (masterDataId != null)
            {
                viewModel.MasterDataId = Convert.ToInt32(masterDataId.Value);
            }

            var masterDataType = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("MasterDataType"));
            if (masterDataType != null)
            {
                viewModel.Type = masterDataType.Value;
            }

            var code = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("managementCode"));
            if (code != null)
            {
                viewModel.Code = code.Value;
            }

            var effectiveDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("managementEffectiveDate"));
            if (effectiveDate != null)
            {
                viewModel.EffectiveDate = Convert.ToDateTime(effectiveDate.Value);
            }

            var expirationDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("managementExpirationDate"));
            if (expirationDate != null)
            {
                viewModel.ExpirationDate = Convert.ToDateTime(expirationDate.Value);
            }

            var parentMasterData = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("masterdataParent"));
            if (parentMasterData != null)
            {
                viewModel.ParentMasterData = Convert.ToInt32(parentMasterData.Value);
            }

            var nameEn = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("masterdataEnglishName"));
            if (nameEn != null)
            {
                viewModel.EnglishName = nameEn.Value.Trim();
            }

            var nameEs = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("masterdataSpanishName"));
            if (nameEs != null)
            {
                viewModel.SpanishName = nameEs.Value.Trim();
            }

            viewModel.FrenchName = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("masterdataFrenchName")).Value;

            viewModel.PortugueseName = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("masterdataPortugueseName")).Value;
        }
        #endregion
        #region All Master Data Save
        public static void UpdateAllMasterDataViewModel(this TableMasterDataTypeViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var masterTypeId = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("MasterTypeId"));
            if (masterTypeId != null)
            {
                viewModel.MasterTypeId = Convert.ToInt32(masterTypeId.Value);
            }

            var masterType = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("MasterDataType"));
            if (masterType != null)
            {
                viewModel.Type = masterType.Value;
            }

            var effectiveDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("EffectiveDate"));

            viewModel.EffectiveDate = effectiveDate != null && effectiveDate.Value != string.Empty ? Convert.ToDateTime(effectiveDate.Value) : (DateTime?)null;

            var expirationDate = clientFieldData.FirstOrDefault(o => o.Name.Equals("ExpirationDate"));

            viewModel.ExpirationDate = expirationDate != null && expirationDate.Value != string.Empty ? Convert.ToDateTime(expirationDate.Value) : (DateTime?)null;

            List<MasterDataManagementTableModelView> lista = new List<MasterDataManagementTableModelView>();

            var managementIdList = clientFieldData.Where(o => o.Name.Equals("managementId")).ToList();
            var managementCodeList = clientFieldData.Where(o => o.Name.Equals("tableManagementCodeValue")).ToList();
            managementCodeList.AddRange(clientFieldData.Where(o => o.Name.Equals("tableManagementCode")).ToList());
            var managementEffectiveDateList = clientFieldData.Where(o => o.Name.Equals("tableManagementEffectiveDate")).ToList();
            var managementExpirationDateList = clientFieldData.Where(o => o.Name.Equals("tableManagementExpirationDate")).ToList();
            var masterdataParentList = clientFieldData.Where(o => o.Name.Equals("tableMasterdataParent")).ToList();
            var masterdataEnglishNameList = clientFieldData.Where(o => o.Name.Equals("masterdataEnglishName")).ToList();
            var masterdataSpanishNameList = clientFieldData.Where(o => o.Name.Equals("masterdataSpanishName")).ToList();
            var masterdataFrenchNameList = clientFieldData.Where(o => o.Name.Equals("masterdataFrenchName")).ToList();
            var masterdataPortugueseNameList = clientFieldData.Where(o => o.Name.Equals("masterdataPortugueseName")).ToList();

            viewModel.Table = (from managementId in managementIdList
                               join managementCode in managementCodeList on managementIdList.IndexOf(managementId) equals managementCodeList.IndexOf(managementCode)
                               join managementEffectiveDate in managementEffectiveDateList on managementIdList.IndexOf(managementId) equals managementEffectiveDateList.IndexOf(managementEffectiveDate)
                               join managementExpirationDate in managementExpirationDateList on managementIdList.IndexOf(managementId) equals managementExpirationDateList.IndexOf(managementExpirationDate)
                               join masterdataParent in masterdataParentList on managementIdList.IndexOf(managementId) equals masterdataParentList.IndexOf(masterdataParent)
                               join masterdataEnglishName in masterdataEnglishNameList on managementIdList.IndexOf(managementId) equals masterdataEnglishNameList.IndexOf(masterdataEnglishName)
                               join masterdataSpanishName in masterdataSpanishNameList on managementIdList.IndexOf(managementId) equals masterdataSpanishNameList.IndexOf(masterdataSpanishName)
                               join masterdataFrenchName in masterdataFrenchNameList on managementIdList.IndexOf(managementId) equals masterdataFrenchNameList.IndexOf(masterdataFrenchName)
                               join masterdataPortugueseName in masterdataPortugueseNameList on managementIdList.IndexOf(managementId) equals masterdataPortugueseNameList.IndexOf(masterdataPortugueseName)
                               select new MasterDataManagementTableModelView
                               {
                                   MasterDataId = int.Parse(managementId.Value),
                                   Code = managementCode.Value,
                                   EffectiveDate = managementEffectiveDate.Value != string.Empty ?
                                       Convert.ToDateTime(managementEffectiveDate.Value) : (DateTime?)null,
                                   ExpirationDate = managementExpirationDate.Value != string.Empty ?
                                       Convert.ToDateTime(managementExpirationDate.Value) : (DateTime?)null,
                                   ParentMasterData = masterdataParent.Value != "0" && !string.IsNullOrEmpty(masterdataParent.Value) ?
                                       int.Parse(masterdataParent.Value) : (int?)null,
                                   EnglishName = masterdataEnglishName.Value,
                                   SpanishName = masterdataSpanishName.Value,

                                   //TODO:Verificar esto!!!
                                   FrenchName = string.IsNullOrEmpty(masterdataFrenchName.Value) || masterdataFrenchName.Value == " " ?
                                       "-" : masterdataFrenchName.Value,
                                   PortugueseName = string.IsNullOrEmpty(masterdataPortugueseName.Value) || masterdataPortugueseName.Value == " " ?
                                       "-" : masterdataPortugueseName.Value,
                                   MasterDataTypeId = viewModel.MasterTypeId
                               }).ToList();
        }
        #endregion
        #endregion
    }
}