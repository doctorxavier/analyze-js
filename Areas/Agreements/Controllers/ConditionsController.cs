using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Agreements.Models;
using IDB.MW.Application.Agreements.Services;
using IDB.MW.Application.Agreements.ViewModel;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.Agreements.Controllers
{
    public partial class ConditionsController : BaseController
    {
        private readonly IAgreementAndConditionService _agreementAndConditionService;

        ////As for R6.7, this is unused due to client's specific request, should be used in some point until 7.1
        ////CON-5722 - PSG operations, use of K2 Workflows for Conditions disabled for R6.7
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class MultipleButtonAttribute : ActionNameSelectorAttribute
        {
            public string Name { get; set; }
            public string Argument { get; set; }

            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                var isValidName = false;
                var keyValue = string.Format("{0}:{1}", Name, Argument);
                var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

                if (value != null)
                {
                    controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                    isValidName = true;
                }

                return isValidName;
            }
        }

        public ConditionsController(
            IAgreementAndConditionService agreementAndConditionService)
        {
            _agreementAndConditionService = agreementAndConditionService;
        }

        public virtual ActionResult Details(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId)
        {
            var model = _agreementAndConditionService.GetDetailsConditionsByOperation(
                operationId, agreementId, conditionId, conditionIndividualId);

            model.DetailsCondition.FinalStatus = SelectListItemHelpers.BuildSelectItemList(
                model.DetailsCondition.FinalStatusModel,
                o => o.NameEn,
                o => o.MasterId.ToString());

            ////As for R6.7, this is temporary due to client's specific request, should be removed in some point until 7.1
            ////CON-5722 - PSG operations, use of K2 Workflows for Conditions disabled for R6.7
            ViewBag.FulfillmentConfirmationMessage =
                Localization.GetText("PSG.FinalStatus.Fulfillment.Confirmation.Message");

            return View(model.DetailsCondition);
        }

        public virtual ActionResult Edit(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            string mainOperationNumber)
        {
            var route = _agreementAndConditionService.GetEditingPath(
                operationId, agreementId, conditionId, conditionIndividualId, mainOperationNumber);

            if (string.IsNullOrEmpty(route))
                return Content(Localization.GetText("TC.SWMeetingService.ErrorMessage"));

            return Redirect(route);
        }

        public virtual ActionResult EditConditionStatusDraft(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            string mainOperationNumber)
        {
            var result = _agreementAndConditionService.GetCondition(conditionId);

            if (!result.IsValid)
                return Content(result.ErrorMessage);

            var newConditionModel = _agreementAndConditionService.GetDataForNewCondition();
            var dropdown = ConditionalViewModelMapAndBuild(
                newConditionModel, mainOperationNumber, agreementId);

            result.Model.Categories = dropdown.Categories;
            result.Model.DatesForDependencyNames = dropdown.DatesForDependencyNames;
            result.Model.Type = dropdown.Type;
            result.Model.Directions = dropdown.Directions;

            return View(result.Model);
        }

        [HttpPost]
        public virtual JsonResult EditConditionDraftPost(NewConditionViewModel model)
        {
            var response = _agreementAndConditionService.CreateNewCondition(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = response.IsValid ?
                    Url.Action("Details",
                        "Conditions",
                        new
                        {
                            area = "Agreements",
                            operationId = response.Model.OperationId,
                            agreementId = model.AgreementId,
                            conditionId = response.Model.ConditionId,
                            conditionIndividualId = response.Model.ConditionIndividualId
                        }) :
                    Url.Action("Index",
                        "Agreements",
                        new
                        {
                            area = "Agreements",
                            operationNumber = IDBContext.Current.Operation
                        })
            });
        }

        [HttpGet]
        public virtual ActionResult CreateNewCondition(string operationNumber, int agreementId)
        {
            NewConditionModel newConditionModel = _agreementAndConditionService.GetDataForNewCondition();

            return View(ConditionalViewModelMapAndBuild(
                newConditionModel, operationNumber, agreementId));
        }

        [HttpPost]
        public virtual JsonResult CreateNewCondition(NewConditionViewModel model)
        {
            var response = _agreementAndConditionService.CreateNewCondition(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index",
                    "Agreements",
                    new
                    {
                        area = "Agreements",
                        operationNumber = IDBContext.Current.Operation
                    })
            });
        }

        public virtual JsonResult EditConditionToTrack(
            int conditionId, int conditionIndividualId, string mainOperationNumber)
        {
            var response = _agreementAndConditionService
                .SendConditionToTrack(conditionId, conditionIndividualId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index",
                    "Agreements",
                    new
                    {
                        area = "Agreements",
                        operationNumber = mainOperationNumber
                    })
            });
        }

        public virtual JsonResult EditConditionToTrackWithdraw(
            int conditionIndividualId, string mainOperationNumber)
        {
            var response = _agreementAndConditionService
                .SendConditionToTrackWithdraw(conditionIndividualId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index",
                    "Agreements",
                    new
                    {
                        area = "Agreements",
                        operationNumber = mainOperationNumber
                    })
            });
        }

        public virtual JsonResult Delete(
            string operationNumber, int conditionId, int conditionIndividualId)
        {
            var response = _agreementAndConditionService
                .DeleteCondition(conditionId, conditionIndividualId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index",
                    "Agreements",
                    new
                    {
                        area = "Agreements",
                        operationNumber = operationNumber
                    })
            });
        }

        [HttpPost]
        public virtual JsonResult DeleteExtension(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            int conditionExtensionId)
        {
            var response = _agreementAndConditionService.DeleteExtension(conditionExtensionId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Details",
                    "Conditions",
                    new
                    {
                        area = "Agreements",
                        operationId = operationId,
                        agreementId = agreementId,
                        conditionId = conditionId,
                        conditionIndividualId = conditionIndividualId
                    })
            });
        }

        public virtual ActionResult EditConditionStatusTrack(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            string mainOperationNumber)
        {
            var model = _agreementAndConditionService.GetDetailsConditionsByOperation(
                operationId, agreementId, conditionId, conditionIndividualId);

            if (!model.IsValid)
                return Content(model.ErrorMessage);

            model.DetailsCondition.IsModeEdit = true;

            return View(model.DetailsCondition);
        }

        [HttpPost]
        public virtual JsonResult EditConditionStatusTrackPost(DetailsConditionViewModel model)
        {
            var response = _agreementAndConditionService.SaveConditionIndividual(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = response.IsValid ?
                    Url.Action("Details",
                        "Conditions",
                        new
                        {
                            area = "Agreements",
                            operationId = model.OperationId,
                            agreementId = model.Agreement.AgreementId,
                            conditionId = model.ConditionId,
                            conditionIndividualId = model.ConditionIndividuals[0].ConditionIndividualId
                        }) :
                    Url.Action("Index",
                        "Agreements",
                        new
                        {
                            area = "Agreements",
                            operationNumber = IDBContext.Current.Operation
                        })
            });
        }

        public virtual ActionResult EditConditionStatusTrackWithDraw(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            string mainOperationNumber)
        {
            var model = _agreementAndConditionService.GetDetailsConditionsByOperation(
                operationId, agreementId, conditionId, conditionIndividualId);

            if (!model.IsValid)
                return Content(model.ErrorMessage);

            var newConditionModel = _agreementAndConditionService.GetDataForNewCondition();

            model.DetailsCondition.CategoryItems = SelectListItemHelpers.BuildSelectItemList(
                newConditionModel.Categories,
                o => o.GetLocalizedName(),
                o => o.MasterId.ToString());

            model.DetailsCondition.TypeItems = SelectListItemHelpers.BuildSelectItemList(
                newConditionModel.Type,
                o => o.GetLocalizedName(),
                o => o.MasterId.ToString());

            return View(model.DetailsCondition);
        }

        [HttpPost]
        public virtual JsonResult EditConditionTrackWithDrawPost(DetailsConditionViewModel model)
        {
            var response = _agreementAndConditionService.UpdateConditionTrackWithDraw(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = response.IsValid ?
                    Url.Action("Details",
                        "Conditions",
                        new
                        {
                            area = "Agreements",
                            operationId = model.OperationId,
                            agreementId = model.Agreement.AgreementId,
                            conditionId = model.ConditionId,
                            conditionIndividualId = model.ConditionIndividuals[0].ConditionIndividualId
                        }) :
                    Url.Action("Index",
                        "Agreements",
                        new
                        {
                            area = "Agreements",
                            operationNumber = IDBContext.Current.Operation
                        })
            });
        }

        public virtual ActionResult CreateExtension(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            string mainOperationNumber)
        {
            var response = _agreementAndConditionService
                .GetExtensionModel(agreementId, conditionId, conditionIndividualId, 0, false);

            if (!response.IsValid)
                return Content(response.ErrorMessage);

            return View(response.Model);
        }

        [HttpPost]
        public virtual ActionResult CreateExtension(ConditionsViewModel model)
        {
            var response = _agreementAndConditionService.SaveExtension(model);

            if (!response.IsValid)
                return Content(response.ErrorMessage);

            return RedirectToAction("Details",
                new
                {
                    operationId = model.OperationId,
                    agreementId = model.AgreementId,
                    conditionId = model.ConditionId,
                    conditionIndividualId = model.ConditionIndividuals[0].ConditionIndividualId
                });
        }

        public virtual ActionResult EditExtension(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            int conditionExtensionId,
            string mainOperationNumber)
        {
            var response = _agreementAndConditionService.GetExtensionModel(
                agreementId, conditionId, conditionIndividualId, conditionExtensionId, true);

            if (!response.IsValid)
                return Content(response.ErrorMessage);

            return View("CreateExtension", response.Model);
        }

        [HttpPost]
        public virtual JsonResult DeleteUserComment(DetailsConditionViewModel model)
        {
            var response = _agreementAndConditionService.SaveConditionIndividual(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Index",
                    "Agreements",
                    new
                    {
                        area = "Agreements",
                        operationNumber = IDBContext.Current.Operation
                    })
            });
        }

        public virtual ActionResult ConditionStatusValidation(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            int finalStatusId)
        {
            var response = _agreementAndConditionService.ChangeConditionIndividualStatusGCM(
                conditionIndividualId, finalStatusId, null);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                Route = Url.Action("Details",
                    new
                    {
                        operationId = operationId,
                        agreementId = agreementId,
                        conditionId = conditionId,
                        conditionIndividualId = conditionIndividualId
                    })
            });
        }

        public virtual ActionResult ConditionFinalStatusValidationRequest(
            int operationId,
            int agreementId,
            int conditionId,
            int conditionIndividualId,
            int finalStatusId,
            DateTime? submissionDate,
            string workflowEntityType,
            int taskId = 0,
            bool isRequest = true)
        {
            var condition = _agreementAndConditionService.GetCondition(conditionId);

            if (condition.Model.CategoryCode == AgreementsAndConditionsConstants.DONOR_CONDITION_CATEGORY_GCM ||
                condition.Model.CategoryCode == AgreementsAndConditionsConstants.DONOR_CONDITION_CATEGORY_TEAM ||
                condition.Model.CategoryCode == AgreementsAndConditionsConstants.DONOR_CONDITION_CATEGORY_CLOSE)
            {
                var response = _agreementAndConditionService.ChangeConditionIndividualStatusGCM(
                    conditionIndividualId, finalStatusId, submissionDate);

                return Json(new
                {
                    response.IsValid,
                    response.ErrorMessage,
                    NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                    NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                    Route = Url.Action("Details",
                    new
                    {
                        operationId = operationId,
                        agreementId = agreementId,
                        conditionId = conditionId,
                        conditionIndividualId = conditionIndividualId
                    })
                });
            }

            var modelConditionIndividual = _agreementAndConditionService
                .ConditionFinalStatusValidationRequest(
                    conditionIndividualId,
                    finalStatusId,
                    workflowEntityType,
                    IDBContext.Current.Roles,
                    IDBContext.Current.IsLocalEnvironment,
                    IDBContext.Current.UserName,
                    IDBContext.Current.Operation,
                    taskId,
                    isRequest);

            if (!modelConditionIndividual.IsValid)
                throw new ArgumentException(modelConditionIndividual.ErrorMessage);

            ViewBag.IsRequiredValidator =
                modelConditionIndividual.Model.ConditionValidatorsViewModel.IsRequiredValidator;

            ViewBag.Validators =
                modelConditionIndividual.Model.ConditionValidatorsViewModel.ValidatorResults;

            ViewBag.CurrentRoles =
                modelConditionIndividual.Model.ConditionValidatorsViewModel.CurrentRoles;

            ViewBag.Editable =
                modelConditionIndividual.Model.ConditionValidatorsViewModel.IsEditable;

            ViewBag.AvailableRoles =
                modelConditionIndividual.Model.ConditionValidatorsViewModel.AvailableRoles;

            if ((IDBContext.Current.HasPermission(Permission.CONDITIONS_WRITE) &&
                modelConditionIndividual.Model.IsDraft) || true)
            {
                return View("ConditionFinalStatusValidationRequestEdit", modelConditionIndividual.Model);
            }

            return View("ConditionFinalStatusValidationRequestDetails", modelConditionIndividual.Model);
        }

        ////As for R6.7, this is unused due to client's specific request, should be used in some point until 7.1
        ////CON-5722 - PSG operations, use of K2 Workflows for Conditions disabled for R6.7
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveConditionRequest")]
        public virtual ActionResult SaveConditionRequest(
            ConditionFinalStatusValidationRequestViewModel model)
        {
            var conditionModel = _agreementAndConditionService.GetCondition(model.ConditionId);
            conditionModel.Model.ConditionIndividuals[0].UserComments.Clear();

            conditionModel.Model.ConditionIndividuals[0].UserComments.AddRange(
                model.ConditionIndividualComments);

            ////TODO MFH: save documents and comments
            ////_agreementAndConditionService.SaveConditionIndividual(conditionModel.Model.ConditionIndividuals[0]);
            return RedirectToAction("Details",
                new
                {
                    operationId = model.OperationId,
                    agreementId = model.AgreementId,
                    conditionId = model.ConditionId,
                    conditionIndividualId = model.ConditionIndividualId
                });
        }

        ////As for R6.7, this is unused due to client's specific request, should be used in some point until 7.1
        ////CON-5722 - PSG operations, use of K2 Workflows for Conditions disabled for R6.7
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveSendConditionRequest")]
        public virtual ActionResult SaveSendConditionRequest(
            ConditionFinalStatusValidationRequestViewModel model)
        {
            var response = _agreementAndConditionService.LaunchConditionK2Workflow(
                model.ConditionIndividualId,
                AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_INDIVIVDUAL,
                IDBContext.Current.UserName,
                IDBContext.Current.FirstRole,
                Request["validator_list_additional_list"],
                model.FinalStatusId);

            ////TODO MFH: save documents and comments
            if (response.IsValid)
                return RedirectToAction("Details",
                new
                {
                    operationId = model.OperationId,
                    agreementId = model.AgreementId,
                    conditionId = model.ConditionId,
                    conditionIndividualId = model.ConditionIndividualId
                });
            else
                throw new ApplicationException(response.ErrorMessage);
        }

        [HttpPost]
        public virtual JsonResult GetDataDocument(string entityRelated, string documentNumber)
        {
            var response = _agreementAndConditionService.GetDataDocument(entityRelated, documentNumber);

            return Json(new
            {
                document = response.Model,
                response.IsValid,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message"),
                DeleteMessage = Localization.GetText("OP.MS.MissionCompleteView.Message.DeleteDocument")
            });
        }

        public virtual JsonResult UpdateExpirationDate(DateTime date, int conditionIndividualId)
        {
            var response = _agreementAndConditionService
                .SaveExpirationDate(date, conditionIndividualId);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                NotificationType = response.IsValid ?
                    NotificationTypeCode.Success :
                    NotificationTypeCode.Error,
                NotificationMessage = response.IsValid ?
                    Localization.GetText("TC.FundingInformation.Save.Message") :
                    Localization.GetText("COMMON.ApplicationMappingException.Message")
            });
        }

        IList<SelectListItem> BuildDependencyDirections()
        {
            return new List<SelectListItem>()
                {
                    new SelectListItem()
                    {
                        Value = "true",
                        Selected = true,
                        Text = Localization.GetText("After")
                    },
                    new SelectListItem()
                    {
                        Value = "false",
                        Text = Localization.GetText("Before")
                    }
                };
        }

        NewConditionViewModel ConditionalViewModelMapAndBuild(
            NewConditionModel newConditionModel,
            string operationNumber,
            int agreementId)
        {
            return new NewConditionViewModel
            {
                Categories = SelectListItemHelpers
                    .BuildCMDSelectListItem(newConditionModel.Categories),
                DatesForDependencyNames = SelectListItemHelpers
                    .BuildCMDSelectListItem(newConditionModel.DatesForDependencyNames),
                Type = SelectListItemHelpers
                    .BuildCMDSelectListItem(newConditionModel.Type),
                Directions = BuildDependencyDirections(),
                OperationNumber = operationNumber,
                AgreementId = agreementId
            };
        }
    }
}