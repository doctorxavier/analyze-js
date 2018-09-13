using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using IDB.Domain.Attributes;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.TCM.Messages.ResultsMatrix;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.MatrixChanges;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class MatrixChangesController : BaseController
    {
        #region StaticFields

        public static string PAGE_CHART = "UI-MC-001-MatrixChange";

        #endregion

        #region Fields

        private readonly ICatalogService _catalogService;
        private readonly IMatrixChangesService _matrixChangesService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ITcmUniverseService _tcmUniverseService;
       private readonly IDocumentManagementService _docManagementService;
       
        #endregion

       public MatrixChangesController(
           IMatrixChangesService matrixChangesService,
           ICatalogService catalogService, 
           ITcmUniverseService tcmUniverseService, 
           IDocumentManagementService docManagementService)
        {
            _catalogService = catalogService;
            _matrixChangesService = matrixChangesService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService, null);
            _tcmUniverseService = tcmUniverseService;
           _docManagementService = docManagementService;
        }

        [ExceptionHandling]
        public virtual ActionResult Index(string operationNumber)
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            SetPermissionMatrixChanges();

            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);

            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var request = new GetMatrixChangesRequest
            {
                OperationNumber = rspnse.OperationNumber,
                TCReportingPeriod = null,
                Section = null,
                ChangeType = null,
                ChangeSubType = null
            };

            var response = _matrixChangesService.GetMatrixChanges(request);

            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            var viewModel = _viewModelMapperHelper.ConvertMatrixChangesViewModel(response.MatrixChangesDTO);
            if (viewModel.MatrixChangesList == null || viewModel.MatrixChangesList.Count == 0)
            {
                viewModel.MatrixChangesList = new List<MatrixChangesItem>();
            }

            viewModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;

            return View(viewModel);
        }

        public virtual ActionResult CancelAndSave(string operationNumber)
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SetPermissionMatrixChanges();
            var request = new GetMatrixChangesRequest
            {
                OperationNumber = rspnse.OperationNumber,
                TCReportingPeriod = null,
                Section = null,
                ChangeType = null,
                ChangeSubType = null
            };
            var response = _matrixChangesService.GetMatrixChanges(request);

            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            var viewModel = _viewModelMapperHelper.ConvertMatrixChangesViewModel(response.MatrixChangesDTO);

            if (viewModel.MatrixChangesList == null || viewModel.MatrixChangesList.Count == 0)
            {
                viewModel.MatrixChangesList = new List<MatrixChangesItem>();
            }

            viewModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;

            return PartialView("Partial/Table", viewModel);
        }

        #region Filters

        public virtual ActionResult FilterMatrixChanges(string operationNumber, int tcReportingPeriod, int? section, int? changeType, int? changeSubType)
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SetPermissionMatrixChanges();
            var request = new GetMatrixChangesRequest
            {
                OperationNumber = rspnse.OperationNumber,
                TCReportingPeriod = tcReportingPeriod,
                Section = section,
                ChangeType = changeType,
                ChangeSubType = changeSubType
            };
            var response = new GetMatrixChangesResponse();
            response = _matrixChangesService.FilterMatrixChanges(request);

            if (!response.IsValid) throw new Exception(response.ErrorMessage);

            var viewModel = new MatrixChangesViewModel
            {
                IsRecipientExecutedTCs = response.MatrixChangesDTO.IsRecipientExecutedTCs,
                MatrixChangesList = response.MatrixChangesDTO.MatrixChanges
            };

            viewModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;
            return PartialView("Partial/Table", viewModel);
        }

        public virtual JsonResult GetChangesTypes(string sectionId)
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            var changesTypes = _matrixChangesService.GetMatrixChangesType(int.Parse(sectionId));
            var response = _viewModelMapperHelper.GetMatrixChangesTypeFilter(changesTypes);

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;

            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual JsonResult GetChangesSubTypes(string typeId)
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            var changesSubTypes = _matrixChangesService.GetMatrixChangesSubType(int.Parse(typeId));
            var response = _viewModelMapperHelper.GetMatrixChangesSubTypeFilter(changesSubTypes);

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;

            return Json(new { IsValid = response.Any(), Data = response });
        }

        #endregion

        public virtual ActionResult DownloadDocument(string documentNumber)
        {
            return Redirect(DownloadDocumentHelper.GetDocumentLink(documentNumber));
        }

       public virtual FileResult DownloadDocument(DownloadRequest request)
       {
           TimeSpan stop;
           TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

           if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
           {
               var downloadResponse = _docManagementService.Download(request);
                
                if (!downloadResponse.IsValid)
                {
                    return File(downloadResponse.Document.File, MimeMapping.GetMimeMapping(downloadResponse.Document.FileName), downloadResponse.Document.FileName);
                }             
           }

           stop = new TimeSpan(DateTime.Now.Ticks);
           var resultado = stop.Subtract(start).Seconds;
           return null;
       }

        public virtual JsonResult Save()
        {
            TimeSpan stop;
            TimeSpan start = new TimeSpan(DateTime.Now.Ticks);

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper.DeserializeObject<List<MatrixChangesItem>>(jsonDataRequest.SerializedData);

            var serializedModifiedGrid = jsonDataRequest.ClientFieldData.SingleOrDefault(o => o.Name == "hiddenGridModel").Value;

            var serializer = new JavaScriptSerializer();

            var modifiedGrid = serializer.Deserialize<MatrixChangesDTO>(serializedModifiedGrid);

            var modifiedRows = modifiedGrid.MatrixChanges;

            foreach (var item in viewModel)
            {
                var modifiedItem = modifiedRows.FirstOrDefault(c => c.MatrixChangeId == item.MatrixChangeId);

                if (modifiedItem == null)
                {
                    item.ExpirationDate = DateTime.Now;
                    modifiedRows.Add(item);
                }
                else
                {
                    if (modifiedItem.Reasons == item.Reasons && modifiedItem.JustificationDocumentNumber == item.JustificationDocumentNumber
                        && modifiedItem.IsFundamental == item.IsFundamental
                        && modifiedItem.AgreementDate == item.AgreementDate)
                        modifiedRows.Remove(modifiedItem);
                }
            }

            var response = new SaveMatrixChangesResponse { IsValid = true };

            if (modifiedRows.Any())
            {
                var updateRequest = new SaveMatrixChangesRequest { MatrixChangesDTO = new MatrixChangesDTO { MatrixChanges = modifiedRows } };

                response = _matrixChangesService.UpdateMatrixChanges(updateRequest);

                if (!response.IsValid) throw new Exception(response.ErrorMessage);
            }

            stop = new TimeSpan(DateTime.Now.Ticks);
            var resultado = stop.Subtract(start).Seconds;

            return Json(response);
        }

        private void SetPermissionMatrixChanges()
        {
            ViewBag.Edit = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);
        }
    }
}