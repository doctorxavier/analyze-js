using System.Collections.Generic;

using IDB.Presentation.MVC4.Areas.Visualization.Enums;

namespace IDB.Presentation.MVC4.Areas.Visualization.Models
{
    public class SendToMapK2DataModelTCM
    {
        public SendToMapSourceEnum sourceScreen { get; set; }

        public int? visualProjectVersionId { get; set; }

        public IEnumerable<int> visualOutputVersionIds { get; set; }

        public string SerialNro { get; set; }
    }
}
