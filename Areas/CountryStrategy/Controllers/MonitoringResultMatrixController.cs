using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Application.CountryStrategyModule.Services.ResultMatrix;
using IDB.MW.Application.CountryStrategyModule.ViewModels.ResultMatrix;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.CountryStrategyModule.ViewModels.MonitoringResultMatrix;
using IDB.MW.Application.CountryStrategyModule.Services.MonitoringResultMatrix;
using IDB.MW.Application.CountryStrategyModule.Messages.MonitoringResultMatrix;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class MonitoringResultMatrixController : BaseController
    {
        #region Constants
        private static string CS_MONITORING_URL = "/CountryStrategy/MonitoringResultMatrix/Monitoring";
        private static string CHART_MONITORING_R_MATRIX = "UI-CS-008-ObjectiveDetails";
        #endregion

        #region Fields
        private readonly IMonitoringCSResultMatrixService _monitoringService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ISecurityModelRepository _securityModelRepository;
        #endregion

        #region Contructors

        public MonitoringResultMatrixController(
            IMonitoringCSResultMatrixService monitoringService,
            IAuthorizationService authorizationService,
            IEnumMappingService enumMappingService,
            ISecurityModelRepository securityModelRepository)
            : base(authorizationService)
        {
            _monitoringService = monitoringService;
            _enumMappingService = enumMappingService;
            _securityModelRepository = securityModelRepository;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, int csObjectiveId = 0, bool isAligned = true, string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetCSObjectiveDetailViewModel(operationNumber,
                csObjectiveId,
                isAligned,
                CHART_MONITORING_R_MATRIX);
            SetViewBag(csObjectiveId, isAligned);
            return View("Read", model);
        }

        public virtual ActionResult Cancel(string operationNumber, int csObjectiveId = 0, bool isAligned = true)
        {
            SynchronizationHelper.TryReleaseLock(CS_MONITORING_URL, operationNumber, IDBContext.Current.UserLoginName);

            var model = GetCSObjectiveDetailViewModel(operationNumber, csObjectiveId, isAligned, CHART_MONITORING_R_MATRIX);
            SetViewBag(csObjectiveId, isAligned);
            return View("Read", model);
        }

        public virtual ActionResult Edit(string operationNumber, int csObjectiveId = 0, bool isAligned = true)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_MONITORING_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { errorMessage = errorMessage });
            }

            var model = GetCSObjectiveDetailViewModel(operationNumber, csObjectiveId, isAligned, CHART_MONITORING_R_MATRIX);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            SetViewBag(csObjectiveId, isAligned);
            return View("Edit", model);
        }

        public virtual ActionResult Save(string operationNumber)
        {
            SaveCSObjectiveDetailResponse response = new SaveCSObjectiveDetailResponse() { IsValid = true };
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<CSObjectiveDetailViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateUseCountrySystemViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_MONITORING_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            response = _monitoringService.SaveCSObjectiveDetail(operationNumber, viewModel);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.UrlRedirect = Url.Action("Read", "MonitoringResultMatrix", new { area = "CountryStrategy", csObjectiveId = viewModel.ObjectiveId, isAligned = viewModel.IsAligned });

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_MONITORING_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "MonitoringResultMatrix", new { area = "CountryStrategy", csObjectiveId = viewModel.ObjectiveId, isAligned = viewModel.IsAligned });
            }

            return Json(responseView);
        }

        public virtual JsonResult GetOperationMonitored(string operationNumber, int operationId, int csObjectiveId, bool isAligned = true)
        {
            var response = _monitoringService.GetOperationMonitored(operationNumber, operationId, csObjectiveId, isAligned);
            if (response.IsValid)
            {
                SetViewBag(csObjectiveId, isAligned);
                var html = this.RenderRazorViewToString("EditPartial/renderPartials/OperationPartial", response.OperationViewModel);
                response.RenderResponse = html;
            }

            return Json(response);
        }

        public virtual JsonResult FindOperation(string operationNumber, string filter)
        {
            var response = _monitoringService.FindOperations(operationNumber, filter.ToUpper());
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion

        #region Private Methods

        private CSObjectiveDetailViewModel GetCSObjectiveDetailViewModel(string operationNumber, int csObjectiveId, bool isAligned, string pageChart = null)
        {
            CSObjectiveDetailViewModel model = null;
            try
            {
                var response = _monitoringService.GetMonitoringDetail(operationNumber, csObjectiveId, isAligned);
                SetViewBagErrorMessageInvalidResponse(response);
               
                if (pageChart != null)
                {
                    var fieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                        IDBContext.Current.Operation,
                        pageChart,
                        IDBContext.Current.Permissions,
                        0,
                        0).ToList();

                    response.CSObjectiveDetail.FieldAccessList = fieldAccessList;
                }

                model = response.CSObjectiveDetail;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                ViewBag.IsValid = false;
            }

            return model;
        }

        private void SetViewBag(int csObjectiveId, bool isAligned)
        {
            ViewBag.CSObjectiveId = csObjectiveId;
            ViewBag.IsAligned = isAligned;

            ViewBag.Equality = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.Equality).ToString();
            ViewBag.ClimateChange = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.ClimateChange).ToString();
            ViewBag.FullEconomic = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.FullEconomic).ToString();
            ViewBag.HighProductivity = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.HighProductivity).ToString();
            ViewBag.InstCapacity = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.InstCapacity).ToString();
            ViewBag.SocialInclusion = _enumMappingService.GetMappingCode<CSImageAssociatedEnum>(CSImageAssociatedEnum.SocialInclusion).ToString();
        }

        #endregion
    }
}