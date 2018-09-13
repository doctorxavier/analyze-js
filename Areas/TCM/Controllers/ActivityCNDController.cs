using System;
using System.Web.Mvc;

using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Architecture.Logging;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class ActivityCNDController : BaseController
    {
        private readonly IActivityCNDService _activityCndService;

        public ActivityCNDController(IActivityCNDService activCndService)
        {
            _activityCndService = activCndService;
        }

        [HttpPost]
        public virtual ActionResult GetThemes()
        {
            var response = _activityCndService.LoadThemes();

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult GetOperations(string operationNumber)
        {
            string lang = IDBContext.Current.CurrentLanguage;
            var response = _activityCndService.LoadOperations(operationNumber, lang);

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult GetUnitsOfMeasurement()
        {
            var response = _activityCndService.LoadUnitsOfMeasurement();

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult GetSupportTypesWithActivityGroups()
        {
            var response = _activityCndService.LoadSupportTypesWithActivityGroups();

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult LoadActivityDetail(string operationNumber, int outputId)
        {
            var response = _activityCndService.LoadCNDActivityWithOutput(operationNumber, outputId);

            response.Model.IsEditable =
                IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_WRITE) ||
                IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult DeleteActivity(int outputId)
        {
            var response = _activityCndService.DeleteCndActivity(outputId);

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult SaveActivity(ActivityCNDViewModel model)
        {
            var response = new ResponseBase();
            try
            {
                ValidateViewModel(model);

                response = _activityCndService.SaveCNDActivity(model, IDBContext.Current.Operation);
            }
            catch (ArgumentException e)
            {
                Logger.GetLogger().WriteError(
                    "ActivityCNDController", "Error validating the model", e);

                response.IsValid = false;
                response.ErrorMessage = e.Message;
            }

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public virtual ActionResult GetOperationMbf(int operationDataId, string relatedOperationNumber)
        {
            var response = _activityCndService.LoadOperationMbf(operationDataId, relatedOperationNumber);

            return new JsonResult { Data = response };
        }

        void ValidateViewModel(ActivityCNDViewModel model)
        {
            if ((model.OutputId > 0 && model.Id == 0) ||
                (model.OutputId == 0 && model.Id > 0))
                throw new ArgumentException("Data inconsistency when creating / updating an activity");

            if (model.Amount <= 0)
                throw new ArgumentException("The amount must be positive");

            if (model.NumberOfUnits <= 0)
                throw new ArgumentException("The number of units must be a positive integer");

            if (model.RelatedOperation == null)
                throw new ArgumentException("There must be a related operation");

            if (model.Theme == null)
                throw new ArgumentException("There must be a theme");

            if (model.SupportType == null)
                throw new ArgumentException("No support type selected");

            if (model.ActivityGroup == null)
                throw new ArgumentException("No activity greoup selected");
        }
    }
}