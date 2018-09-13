using System.Collections.Generic;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public class DelegatingTaskSearchModel
    {
        public int TaskId { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string UserRole { get; set; }

        public string OperationNumber { get; set; }

        public string Country { get; set; }

        public string Division { get; set; }

        public string SharepointSelectedGroup { get; set; }

        public string SharepointSelectedGroupName { get; set; }

        public List<ConvergenceMasterDataModel> Countries { get; set; }

        public List<ConvergenceMasterDataModel> Roles { get; set; }

        public List<ConvergenceMasterDataModel> Divisions { get; set; }

        public List<SharepointGroupsModel> SharepointGroups { get; set; }

        public int SearchOption { get; set; }
    }
}