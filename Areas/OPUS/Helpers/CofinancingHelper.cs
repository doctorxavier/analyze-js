using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.OPUS.Helpers
{
    public static class CofinancingHelper
    {
        public static List<SelectListItem> GetMasterCodeSources(
            string masterType, 
            bool hideExpired = false)
        {
            var service = Globals.Resolve<IFinancialDataPreparationService>();
            var currentDate = DateTime.Now;

            return service.GetMasterCodeCof(masterType, hideExpired).MasterDataCodes
                .Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Code,
                Group = new SelectListGroup() { Name = x.Type },
                Disabled = !(x.ExpirationDate == null || x.ExpirationDate > currentDate)
            }).ToList();
        }

        public static List<SelectListItem> GetFundingSources(string fundingSourceCode = "")
        {
            var fundingSourcesList = new List<SelectListItem>();

            fundingSourcesList.AddRange(
                GetMasterCodeSources(OPUSGlobalValues.COFIN_FUNDING_SOURCE_CASH));
            fundingSourcesList.AddRange(
                GetMasterCodeSources(OPUSGlobalValues.COFIN_FUNDING_SOURCE_KIND));
            fundingSourcesList.AddRange(
                GetMasterCodeSources(OPUSGlobalValues.COFIN_FUNDING_SOURCE_BOTH));

            if (fundingSourceCode != string.Empty)
            {
                fundingSourcesList = CheckForExpiredElements(
                    fundingSourceCode, 
                    fundingSourcesList);
            }

            fundingSourcesList = OtherFieldProcess(fundingSourcesList);

            return fundingSourcesList; 
        }

        public static string GetDescriptionByCode(string code, List<SelectListItem> list)
        {
            return list.FirstOrDefault(x => x.Value == code).Text ?? string.Empty;
        }

        public static bool IsDescriptionRequired(
            string typeCode, 
            string fundingSourceCode)
        {
            bool isDescriptionFieldRequired = false;

            var listOfInKindCodes = new List<string>()
            {
                OPUSGlobalValues.COFIN_COFINANCING_IN_KIND_CODE,
                OPUSGlobalValues.COFIN_COUNTER_PART_IN_KIND_CODE
            };

            if (fundingSourceCode == OPUSGlobalValues.COFIN_OTHER)
            {
                isDescriptionFieldRequired = true;

                return isDescriptionFieldRequired;
            }

            isDescriptionFieldRequired = listOfInKindCodes.Contains(typeCode);

            return isDescriptionFieldRequired;
        }

        public static List<SelectListItem> GetModalities(string modalityCode = "")
        {
            var modalities = new List<SelectListItem>();

            modalities = LoadElementsListWithoutExpired(
                OPUSGlobalValues.COFIN_MODALITIES, 
                modalityCode);

            return modalities;
        }

        public static List<SelectListItem> GetCashOrInKindList(string cashOrInKindCode = "")
        {
            var cashOrInKindList = new List<SelectListItem>();

            cashOrInKindList = LoadElementsListWithoutExpired(
                OPUSGlobalValues.COFIN_CASH_KIND,
                cashOrInKindCode);

            return cashOrInKindList;
        }

        public static List<SelectListItem> GetCounterPartTypes(string counterPartTypeCode = "")
        {
            var counterPartTypes = new List<SelectListItem>();

            counterPartTypes = LoadElementsListWithoutExpired(
                OPUSGlobalValues.COFIN_COUNTERPART_TYPES, 
                counterPartTypeCode);

            return counterPartTypes;
        }

        public static string ConvertDateTimeToString(DateTime? date)
        {
            return date != null ? date.Value.ToString("d MMM yyyy") : string.Empty;
        }

        public static List<SelectListItem> GetStatusList(string statusCode = "")
        {
            var statusList = new List<SelectListItem>();

            statusList = LoadElementsListWithoutExpired(
                OPUSGlobalValues.COFIN_STATUS,
                statusCode);

            return statusList;
        }

        private static List<SelectListItem> LoadElementsListWithoutExpired(
            string masterDataType, 
            string elementCode = "")
        {
            var elementsList = new List<SelectListItem>();

            elementsList = GetMasterCodeSources(masterDataType);

            if (elementCode != string.Empty)
            {
                elementsList = CheckForExpiredElements(elementCode, elementsList);
            }

            return elementsList;
        }

        private static List<SelectListItem> CheckForExpiredElements(
            string code, 
            List<SelectListItem> elementsList)
        {
            bool isCodeOnTheListOfNotExpired = elementsList.Where(b =>
                b.Value == code &&
                b.Disabled == true)
                .Any();

            if (isCodeOnTheListOfNotExpired)
            {
                var elementToBeAddedAsExpired = elementsList.Where(
                    m => m.Value == code).First();

                elementsList.RemoveAll(m => m.Value == code);

                elementsList.Add(new SelectListItem()
                {
                    Text = elementToBeAddedAsExpired.Text + " (Expired)",
                    Value = elementToBeAddedAsExpired.Value
                });
            }

            return elementsList;
        }

        private static List<SelectListItem> OtherFieldProcess(
            List<SelectListItem> fundingSourcesList)
        {
            var otherPSList = fundingSourcesList.Where(f => 
                f.Value == OPUSGlobalValues.COFIN_OTHER)
                .ToList();

            fundingSourcesList.RemoveAll(f => f.Value == OPUSGlobalValues.COFIN_OTHER);
            fundingSourcesList = fundingSourcesList.OrderBy(f => f.Text).ToList();
            fundingSourcesList.AddRange(otherPSList);

            return fundingSourcesList;
        }
    }
}
