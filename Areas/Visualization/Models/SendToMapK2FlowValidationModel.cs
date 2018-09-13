using System;
using IDB.Presentation.MVC4.Areas.Visualization.Enums;

namespace IDB.Presentation.MVC4.Areas.Visualization.Models
{
    /// <summary>
    /// Represents visual data objects being validated for a K2 Flow in order to Approve/Reject an External Map publishing
    /// </summary>
    public class SendToMapK2FlowValidationModel
    {
        public SendToMapK2OperTypeEnum k2OperationType { get; set; }

        public int visualDataVersionId { get; set; }

        public string voComment { get; set; }
    }
}