using IDB.MW.Application.VERModule.Messages.Request;
using IDB.MW.Application.VERModule.Services.MyDocuments;
using IDB.MW.Application.VERModule.ViewModels.MyDocument;

namespace IDB.Presentation.MVC4.Areas.VER.Models
{
    public class MyDocumentsModelMapperHelper
    {
        #region Fiedls
        private readonly dynamic _viewBag;
        private readonly IMyDocumentsService _myDocumentsService;
        #endregion

        #region Constructor
        public MyDocumentsModelMapperHelper(
            dynamic viewBag,
            IMyDocumentsService myDocumentsService)
        {
            _myDocumentsService = myDocumentsService;
            _viewBag = viewBag;
        }
        #endregion

        #region Methods
        public MyDocumetsViewModel GetMyDocuments(string operationNumber)
        {
            var modelResponse = _myDocumentsService.GetMyDocuments(new MyDocumentsRequest
            {
                OperationNumber = operationNumber
            });

            if (modelResponse.IsValid == false)
            {
                _viewBag.ErrorMessage = modelResponse.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return modelResponse.Ver;
        }

        #endregion
    }
}