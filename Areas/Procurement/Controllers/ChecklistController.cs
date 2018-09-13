using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.BEOProcurementModule.ViewModels.FirmProcurement;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Controllers
{
    public partial class ChecklistController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        private const string TAB_NAME_IDENTIFICATION = "#linktabIdentification";
        private const string TAB_NAME_PREPARATION = "#linktabPreparation";
        private const string TAB_NAME_EVALUATION = "#linktabEvaluation";
        private const string TAB_NAME_NEGOTIATION = "#linktabNegotiation";
        private const string TAB_NAME_EXECUTION = "#linktabExecution";
        #endregion

        #region Fields
        #endregion

        #region Contructors
        public ChecklistController()
        {
        }
        #endregion

        #region Action Methods
        public virtual ActionResult Index()
        {
            var model = GetMockCheckList();
            return View(model);
        }
        #endregion

        #region Private Methods

        private List<ChecklistRowViewModel> GetMockCheckList()
        {
            var model = new List<ChecklistRowViewModel>();

            for (int i = 0; i < 10; i++)
            {
                model.Add(new ChecklistRowViewModel()
                {
                    ChecklistStageModalityItemId = i,
                    ChecklistItem = string.Format("ChecklistItem {0}", i),
                    MandatoryType = "Tipo1",
                    Completed = i % 4 == 0,
                    IsAuto = (i % 3) - 1 <= 0,
                    AutoCompleteText = (i % 3) - 1 <= 0 ? string.Format("AutoCompleteText {0}", i) : null,
                    DateCompleted = i % 4 == 0 ? DateTime.Today : (DateTime?)null,
                    User = "user"
                });
            }

            return model;
        }

        #endregion
    }
}
