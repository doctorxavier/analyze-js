using System;
using System.Collections.Generic;

using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Common;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.MatrixChanges;
using IDB.MW.Domain.Models.Security;

namespace IDB.Presentation.MVC4.Areas.TCM.Models
{
    [Serializable]
    public class MatrixChangesViewModel
    {
        public bool IsRecipientExecutedTCs { get; set; }

        public int ResultsMatrixId { get; set; }

        public int CurrentTCReportingPeriod { get; set; }

        public bool IsEditable { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public OperationInfoViewModel OperationInfo { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public MatrixChangesFiltersViewModel Filters { get; set; }

        public List<MatrixChangesItem> MatrixChangesList { get; set; }

        public List<FieldAccessModel> FieldAccessList { get; set; }

        public bool IsCndOperation { get; set; }
    }
}