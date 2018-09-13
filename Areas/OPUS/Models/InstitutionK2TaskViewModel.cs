using System;
using System.Collections.Generic;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.DomainModel.Entities.Core.K2;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    [Serializable]
    public class InstitutionK2TaskViewModel
    {
        public string Instructions { get; set; }

        public string UserName { get; set; }

        public string OperationNumber { get; set; }

        public string UserRoleInstitution { get; set; }

        public List<string> UserRole { get; set; }

        public bool CommentsWritePermission { get; set; }

        public List<UserCommentViewModel> UserComments { get; set; }

        public TaskDataModel TaskDataModel { get; set; }

        public List<ValidatorViewModel> Validators { get; set; }
    }
}