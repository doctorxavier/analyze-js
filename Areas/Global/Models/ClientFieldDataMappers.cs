using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.GlobalModule.ViewModels;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateWorkFlowViewModel(this WorkflowViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel == null)
            {
                viewModel = new WorkflowViewModel();
            }

            //Additional Validators
            var additionalValidators = clientFieldData.Where(o => o.Name.Equals("newUserProfile"));

            if (viewModel.Validators == null)
            {
                viewModel.Validators = new List<ValidatorViewModel>();
            }

            for (int i = 0; i < additionalValidators.Count(); i++)
            {
                var validator = new ValidatorViewModel();
                validator.Role = additionalValidators.ElementAt(i).Value;
                validator.Status = "Pending";
                validator.Mandatory = false;
                viewModel.Validators.Add(validator);
            }

            var deleteValidators = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteValidators"));

            if (deleteValidators != null)
            {
                string[] deleteV = deleteValidators.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (viewModel.DeleteValidators == null)
                {
                    viewModel.DeleteValidators = new List<int>();
                }

                foreach (string s in deleteV)
                {
                    viewModel.DeleteValidators.Add(Convert.ToInt32(s));
                    viewModel.Validators.RemoveAll(d => d.Order.Equals(Convert.ToInt32(s)));
                }
            }

            //Comments
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<UserCommentViewModel>();
            }

            var editComments = clientFieldData.Where(o => o.Name.Equals("textComment"));
            var editCommentsId = clientFieldData.Where(o => o.Name.Equals("commentId"));

            if (editComments != null && editCommentsId != null)
            {
                for (int i = 0; i < editComments.Count(); i++)
                {
                    var index = viewModel.UserComments.FindIndex(x => x.CommentId == Convert.ToInt32(editCommentsId.ElementAt(i).Value));
                    if (!viewModel.UserComments.ElementAt(index).Comment.Equals(editComments.ElementAt(i).Value.Trim()))
                    {
                        viewModel.UserComments.ElementAt(index).Comment = editComments.ElementAt(i).Value.Trim();
                    }
                }
            }

            var newComments = clientFieldData.Where(o => o.Name.Equals("newComment"));

            if (newComments != null)
            {
                for (int i = 0; i < newComments.Count(); i++)
                {
                    var comment = new UserCommentViewModel();
                    comment.Comment = newComments.ElementAt(i).Value.Trim();

                    viewModel.UserComments.Add(comment);
                }
            }

            var deleteComments = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteComments"));

            if (deleteComments != null)
            {
                string[] deleteC = deleteComments.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (viewModel.DeleteComments == null)
                {
                    viewModel.DeleteComments = new List<int>();
                }

                foreach (string s in deleteC)
                {
                    viewModel.DeleteComments.Add(Convert.ToInt32(s));
                    viewModel.UserComments.RemoveAll(d => d.CommentId.Equals(Convert.ToInt32(s)));
                }
            }

            var contractNumber = clientFieldData.FirstOrDefault(o => o.Name.Equals("contractNumber"));

            if (contractNumber != null)
            {
                viewModel.ContractNumber = contractNumber.Value;
            }

            // Documents
            //viewModel.UpdateMissionWorkflowDocuments(clientFieldData);
        }
    }
}