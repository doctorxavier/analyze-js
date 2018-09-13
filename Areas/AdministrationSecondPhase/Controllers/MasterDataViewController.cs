using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.AdministrationModule.ViewModels.MasterData;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.MasterData;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class MasterDataViewController : BaseController
    {
        #region fields
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IMasterDataService _masterDataService;
        #endregion

        public MasterDataViewController(ICatalogService catalogService, IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
            _viewModelMapperHelper = new ViewModelMapperHelper(catalogService, _masterDataService);
        }

        // GET: AdministrationSecondPhase/MasterData/Search
        public virtual ActionResult SearchFilter(string filter)
        {
            var model = _viewModelMapperHelper.GetMasterDataTypeListByFilter(filter) ??
                        new List<TableMasterDataTypeViewModel>();
            return PartialView("Partial/DataTables/TableMasterData", model);
        }

        public virtual ActionResult Search()
        {
            var model = _viewModelMapperHelper.GetMasterDataTypeList(null);
            return View(model);
        }

        public virtual JsonResult GetMasterTypes(string filter)
        {
            var response = _viewModelMapperHelper.GetMasterTypes(filter);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult GetMasterDatas(string filter)
        {
            var response = _viewModelMapperHelper.GetMasterDatasFil(filter);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult TableMasterData()
        {
            return View();
        }

        //GET: AdministrationSecondPhase/MasterData/Management
        public virtual ActionResult Management(int id = 0)
        {
            var model = new TableMasterDataTypeViewModel
            {
                Table = new List<MasterDataManagementTableModelView>(),
                Type = id > 0 ? "qwe" : string.Empty,
                MasterTypeId = id
            };
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.DateNow = DateTime.Now;
            return View("Management", model);
        }

        public virtual ActionResult ManagementContent(int id = 0)
        {
            var model = new TableMasterDataTypeViewModel
            {
                Table = new List<MasterDataManagementTableModelView>(),
                Type = id > 0 ? "qwe" : string.Empty,
                MasterTypeId = id
            };
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/managementPartial", model);
        }

        public virtual ActionResult ManagementFilter(string filter)
        {
            var model = new TableMasterDataTypeViewModel
            {
                Table = new List<MasterDataManagementTableModelView>(),
                Type = Convert.ToInt32(filter) > 0 ? "qwe" : string.Empty,
                MasterTypeId = Convert.ToInt32(filter)
            };
            model = _viewModelMapperHelper.GetMasterDataByType(filter);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View("Management", model);
        }

        //GET: AdministrationSecondPhase/MasterData/TableManagement
        public virtual ActionResult TableManagement()
        {
            return View("Partial/DataTables/TableManagement");
        }

        public virtual ActionResult GetRowManagement(string operationNumber)
        {
            return PartialView("Partial/DataTables/RowManagement");
        }

        public virtual ActionResult AddMasterDataView(string masterDataTypeId, int row = 0)
        {
            ViewBag.lastRow = row + 1;
            ViewBag.MasterDataTypeId = masterDataTypeId;
            return PartialView("Partial/DataTables/RowManagement");
        }

        public virtual ActionResult SearchMasterDataFilter(string filter, string masterType)
        {
            var model = _viewModelMapperHelper.GetMasterDataListByFilter(filter, masterType) ??
                         new List<MasterDataManagementTableModelView>();

            return PartialView("Partial/DataTables/TableManagement", model);
        }

        public bool ExistMasterDataType(string masterType)
        {
            return _masterDataService.ExistMasterType(masterType);
        }
    }
}
