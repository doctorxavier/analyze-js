using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Enums.VisualOutputsMapping;
using IDB.MW.Application.AdministrationModule.Services.SearchService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.MapModule.Enums;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.VisualOutputsMapping;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Helpers
{
    public static class VisualOutputsMappingHelper
    {
        public static IList<SelectListItem> GetNumberOfResultsPerPage()
        {
            string[] _pageSize = VisualOutputsMappingEnum.FilterResultsPerPage;

            var numberPage = new List<SelectListItem>();

            for (int i = 0; i < _pageSize.Count(); i++)
            {
                numberPage.Add(
                    new SelectListItem()
                    {
                        Value = _pageSize[i],
                        Text = string.Format(
                            "{0} {1}",
                            Localization.GetText("OPUS.Search.PageSize"),
                            Convert.ToInt32(_pageSize[i]))
                    });
            }

            return numberPage;
        }

        public static IList<SelectListItem> GetDivisionsList(string sectorId)
        {
            var divisionsList = new List<SelectListItem>();

            var _searchService = Globals.Resolve<ISearchService>();

            var response = _searchService.GetDivisionOpus();

            response.DivisionOpus = response
                                        .DivisionOpus
                                        .Where(q => q.SectorDepartmentId != null)
                                        .ToList();

            if (!string.IsNullOrEmpty(sectorId))
            {
                var sector = Convert.ToInt32(sectorId);
                response.DivisionOpus = response
                                            .DivisionOpus
                                            .Where(q => q.SectorDepartmentId.Value == sector)
                                            .ToList();
            }

            if (response.IsValid)
            {
                divisionsList = response.DivisionOpus
                    .Select(division => new SelectListItem
                    {
                        Value = division.DivisionId.ToString(),
                        Text = division.DivisionCode + " - " + division.DivisionEn,
                        Group = new SelectListGroup() { Name = division.SectorDepartmentId.ToString() }
                    })
                    .ToList();
            }

            return divisionsList;
        }

        public static IList<MultiSelectListItem> GetYearsList()
        {
            var years = Enumerable.Range(DateTime.Now.Year - 10, 20);

            var operationYearsList = years.Select(year => new MultiSelectListItem
            {
                Text = year.ToString(),
                Value = year.ToString(),
            }).ToList();

            return operationYearsList;
        }

        public static IList<SelectListItem> GetCountryList()
        {
            return GetMasterDataListByType(SearchValues.COUNTRY);
        }

        public static IList<SelectListItem> GetOperationTypes()
        {
            var opertionTypes = GetMasterDataListByType(SearchValues.OPERATION_TYPE, true);

            return opertionTypes
                .Where(ot => VODocumentEnum.OperationTypes.Contains(ot.Value))
                .ToList();
        }

        public static IList<SelectListItem> GetDepartmentSectorList()
        {
            var _searchService = Globals.Resolve<ISearchService>();

            var sectorOpusWithExpired = _searchService.GetSectorOpusWithExpired();
            var sectorOpusList = new List<SelectListItem>();

            if (sectorOpusWithExpired.IsValid)
            {
                sectorOpusList = sectorOpusWithExpired.DivisionOpusWithExpired.Where(x =>
                            (VisualOutputsMappingDepartmentsCode.Departments
                            .Contains(x.SectorDepartmentCode) ||
                            (x.IsExpired && x.DivisionCode.Equals(x.SectorDepartmentCode))) &&
                            x.SectorDepartmentId == null)
                            .Select(c =>
                                new SelectListItem
                                {
                                    Text = c.SectorDepartmentCode,
                                    Value = c.DivisionId.ToString()
                                })
                            .OrderBy(x => x.Text).ToList();
            }

            return sectorOpusList;
        }

        public static string GetDocName()
        {
            string userName = IDBContext.Current.UserName;
            DateTime date = DateTime.Now;
            string name = VODocumentEnum.NAME;
            string underscore = VODocumentEnum.UNDERSCORE_SEPARATOR;
            string documentExtension = VODocumentEnum.DOCUMENT_EXTENSION;
            string documentName = userName + underscore + date + underscore + name + documentExtension;

            return documentName;
        }

        public static string GetLogOfChangesFileName()
        {
            string userName = IDBContext.Current.UserName;
            DateTime date = DateTime.Now;
            string name = VODocumentEnum.LOG_NAME;
            string underscore = VODocumentEnum.UNDERSCORE_SEPARATOR;
            string documentExtension = VODocumentEnum.LOG_EXTENSION;
            string documentName = userName + underscore + date + underscore + name + documentExtension;

            return documentName;
        }

        public static string GetMapLayersFileName(string layer)
        {
            string userName = IDBContext.Current.UserName;
            DateTime date = DateTime.Now;
            string name = string.Empty;
            switch (layer)
            {
                case "VOLayer":
                    name = VODocumentEnum.MAPLAYERS_NAME_VO;
                    break;
                case "ServicesLayer":
                    name = VODocumentEnum.MAPLAYERS_NAME_SERVICE;
                    break;
                case "MilestoneLayer":
                    name = VODocumentEnum.MAPLAYERS_NAME_MILESTONE;
                    break;
            }

            string underscore = VODocumentEnum.UNDERSCORE_SEPARATOR;
            string documentExtension = VODocumentEnum.MAPLAYERS_EXTENSION;
            string documentName = userName + underscore + date + underscore + name + documentExtension;

            return documentName;
        }

        private static IList<SelectListItem> GetMasterDataListByType(
            string masterDataType, bool includeCode = false)
        {
            var _catalogService = Globals.Resolve<ICatalogService>();

            return _catalogService
                .GetMasterDataListByTypeCode(true, masterDataType)
                .MasterDataCollection.Select(masterData => new SelectListItem
                {
                    Text = includeCode 
                        ? masterData.Code + " - " + masterData.GetLocalizedName(Localization.CurrentLanguage)
                        : masterData.GetLocalizedName(Localization.CurrentLanguage),
                    Value = masterData.Code
                }).ToList();
        }
    }
}