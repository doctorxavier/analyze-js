using System;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.MasterDataService;
using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.AdministrationModule.ViewModels.MasterData;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.MasterData;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class MasterDataSaveController : MVC4.Controllers.ConfluenceController
    {
        private const string UrlMasterDataView = "/AdministrationSecondPhase/MasterData";

        private readonly IMasterDataService _masterdataService;

        #region Contructors
        public MasterDataSaveController(IMasterDataService masterdataService)
        {
            _masterdataService = masterdataService;
        }

        #endregion

        public virtual JsonResult MasterDataEditSave()
        {
            int nTables = 0;
            CreateMasterDataResponse response = new CreateMasterDataResponse();
            try
            {
                var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
                var viewModel = PageSerializationHelper.DeserializeObject<TableMasterDataTypeViewModel>(jsonDataRequest.SerializedData);

                viewModel.UpdateAllMasterDataViewModel(jsonDataRequest.ClientFieldData);

                var userName = IDBContext.Current.UserName;
                var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlMasterDataView, nTables.ToString(), userName);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    response = new CreateMasterDataResponse
                    {
                        IsValid = false,
                        ErrorMessage = errorMessage
                    };
                }
                else
                {
                    response = _masterdataService.CreateMasterData(viewModel);
                    
                    if (response.IsValid)
                    {
                        SynchronizationHelper.TryReleaseLock(UrlMasterDataView, nTables.ToString(), userName);
                    }
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.Message;
            }

            return Json(response);
        }
    }
}