using System;
using System.Threading.Tasks;
using System.Web.Mvc;

using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.Architecture.Logging;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class ComponentCNDController : BaseController
    {
        private readonly IComponentCNDService _componentCNDService;
        private readonly IOperationDataService _operationDataService;

        public ComponentCNDController(
            IComponentCNDService componentServ,
            IOperationDataService opDataServ)
        {
            _componentCNDService = componentServ;
            _operationDataService = opDataServ;
        }

        [HttpPost]
        public virtual async Task<ActionResult> GetComponentCND(string operationNumber, int id)
        {
            var modelResponse = await _componentCNDService.LoadComponentInfoAsync(
                operationNumber, id);

            if (!modelResponse.IsValid)
            {
                return new JsonResult { Data = null };
            }

            var contactData = _operationDataService
                .GetUsersByNameOrPCMail(modelResponse.Model.Contact);

            var specialistData = _operationDataService
                .GetUsersByNameOrPCMail(modelResponse.Model.Specialist);

            bool isEditable =
                IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_WRITE) ||
                IDBContext.Current.HasPermission(Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);

            var fullModel = new ComponentCNDFullViewModel
            {
                ComponentModel = modelResponse.Model,
                IsEditable = isEditable,
                ContactData = contactData,
                SpecialistData = specialistData
            };

            return new JsonResult { Data = fullModel };
        }

        [HttpPost]
        public virtual async Task<ActionResult> GetAllBeneficiaries(string operationNumber)
        {
            try
            {
                var allBeneficiariesResponse = await _componentCNDService
                    .LoadBeneficiariesAsync(operationNumber);

                if (!allBeneficiariesResponse.IsValid)
                {
                    return new JsonResult { Data = null };
                }

                return new JsonResult { Data = allBeneficiariesResponse.Models };
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "ComponentCNDController", "Error when loading beneficiaries", e);

                return new JsonResult { Data = null };
            }
        }

        [HttpPost]
        public virtual ActionResult IsCNDOperation(string operationNumber)
        {
            try
            {
                var response = _componentCNDService.IsCNDOperation(operationNumber);

                if (!response.IsValid)
                    return new JsonResult { Data = false };

                return new JsonResult { Data = response.HasCondition };
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "ComponentCNDController", "Error checking CND logic", e);

                return new JsonResult { Data = false };
            }
        }

        public virtual ActionResult GetUsers(string filter)
        {
            var response = _operationDataService.GetUsersByNameOrPCMail(filter);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public virtual ActionResult SaveComponentCND(ComponentCNDViewModel model)
        {
            var response = new ResponseBase();

            try
            {
                ValidateViewModel(model);

                response = _componentCNDService.SaveComponent(model);
            }
            catch (ArgumentException e)
            {
                Logger.GetLogger().WriteError(
                    "ComponentCNDController", "Error validating the model", e);

                response.IsValid = false;
                response.ErrorMessage = e.Message;
            }

            return new JsonResult { Data = response };
        }

        void ValidateViewModel(ComponentCNDViewModel model)
        {
            if ((model.ComponentId > 0 && model.Id == 0) ||
                (model.ComponentId == 0 && model.Id > 0))
                throw new ArgumentException("Data inconsistency when creating / updating component");

            if (model.ResultsMatrixId <= 0)
                throw new ArgumentException("Component not related to a results matrix");

            if (model.Statement == null)
                throw new ArgumentException("Statement must be specified");

            if (model.OrderNumber == null || model.OrderNumber <= 0)
                throw new ArgumentException("Order number must be specified");

            if (model.ExpectedResults == null)
                throw new ArgumentException("Expected results must be specified");

            const int MAX_EXPECTED_RESULTS_LENGTH = 500;
            if (model.ExpectedResults.Length > MAX_EXPECTED_RESULTS_LENGTH)
                throw new ArgumentException("Expected results too long");

            if (model.Contact == null)
                throw new ArgumentException("Contact must be specified");

            if (model.Specialist == null)
                throw new ArgumentException("Specialist must be specified");

            if (model.StartDate == DateTime.MinValue)
                throw new ArgumentException("Start date must be specified");

            if (model.EndDate == DateTime.MinValue)
                throw new ArgumentException("End date must be specified");
        }
    }
}