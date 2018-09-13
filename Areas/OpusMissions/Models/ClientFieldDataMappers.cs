using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Core.ViewModels.Common;
using IDB.MW.Application.MissionsModule.ViewModels;
using IDB.MW.Application.MissionsModule.ViewModels.Workflows;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateMissionDocuments(this MissionViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var hiddenMissionIdEdit = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenMissionIdDocument"));

            if (hiddenMissionIdEdit != null)
            {
                viewModel.MissionId = Convert.ToInt32(hiddenMissionIdEdit.Value);
            }

            var documentDescription = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentDescription"));

            var documentNumber = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentNumber"));

            var documentIndex = 0;

            foreach (var document in documentDescription)
            {
                var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;

                viewModel.Documents.Add(
                    new DocumentViewModel
                    {
                        Description = document.Value,
                        DocNumber = documentNumberValue
                    });

                documentIndex++;
            }
        }

        public static void UpdateMissionWorkflowDocuments(this MissionsWorkflowViewModels viewModel, ClientFieldData[] clientFieldData)
        {
            var documentDescription = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentDescription"));

            var documentNumber = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentNumber"));

            var documentIndex = 0;

            foreach (var document in documentDescription)
            {
                var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;

                viewModel.Documents.Add(
                    new MissionsWorkflowDocumentsViewModels
                    {
                        Description = document.Value,
                        DocNumber = Convert.ToInt32(documentNumberValue)
                    });

                documentIndex++;
            }
        }

        public static void UpdateMissionViewModel(this MissionViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel == null)
            {
                viewModel = new MissionViewModel();
            }

            viewModel.UpdateMissionDocuments(clientFieldData);

            var hiddenOrgUnitOne = clientFieldData.FirstOrDefault(o => o.Name.Equals("orgUnitOneBool"));

            if (hiddenOrgUnitOne != null && hiddenOrgUnitOne.Value.Equals("true"))
            {
                var hiddenOrgUnitValue = clientFieldData.FirstOrDefault(o => o.Name.Equals("orgUnitOneValue"));
                viewModel.OrganizationalUnitId = Convert.ToInt32(hiddenOrgUnitValue.Value);
            }
            else 
            {
                var organizationalUnit = clientFieldData.FirstOrDefault(o => o.Name.Equals("organizationalUnit"));
                if (organizationalUnit != null)
                {
                    viewModel.OrganizationalUnitId = Convert.ToInt32(organizationalUnit.Value.ToString());
                }
            }

            var hiddenIsNeededNewApproval = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenSelectNeededNewApproval"));

            if (hiddenIsNeededNewApproval != null)
            {
                viewModel.IsNeededNewApproval = Convert.ToBoolean(hiddenIsNeededNewApproval.Value);
            }

            var hiddenIsNewMission = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenIsNewMission"));

            if (hiddenIsNewMission != null)
            {
                viewModel.IsNewMission = Convert.ToBoolean(hiddenIsNewMission.Value);
            }

            var hiddenSelectUpdateActual = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenSelectUpdateActual"));

            if (hiddenSelectUpdateActual != null)
            {
                viewModel.updateFieldActualActivity = Convert.ToBoolean(hiddenSelectUpdateActual.Value);
            }

            var hiddenMissionIdEdit = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenMissionIdEdit"));

            if (hiddenMissionIdEdit != null)
            {
                viewModel.MissionId = Convert.ToInt32(hiddenMissionIdEdit.Value);
            }

            var type = clientFieldData.FirstOrDefault(o => o.Name.Equals("Type"));

            if (type != null)
            {
                viewModel.TypeId = Convert.ToInt32(type.Value);
            }

            var validatorCountry = clientFieldData.FirstOrDefault(o => o.Name.Equals("validatorCountry"));

            if (validatorCountry != null)
            {
                viewModel.CountryId = Convert.ToInt32(validatorCountry.Value);
            }

            var startDate = clientFieldData.FirstOrDefault(o => o.Name.Equals("dtStartDate"));

            if (startDate != null)
            {
                viewModel.StartDate = Convert.ToDateTime(startDate.Value.ToString());
            }

            var endDate = clientFieldData.FirstOrDefault(o => o.Name.Equals("dtEndDate"));

            if (endDate != null)
            {
                viewModel.EndDate = Convert.ToDateTime(endDate.Value.ToString());
            }

            if (viewModel.MissionId != 0)
            { 
                // if missions equal zero then is a nuew mission, the status is draft and then setted in SaveMission Method
                var status = clientFieldData.FirstOrDefault(o => o.Name.Equals("status"));

                if (status != null)
                {
                    viewModel.Status = status.Value.ToString();
                }
            }

            var country = clientFieldData.FirstOrDefault(o => o.Name.Equals("country"));

            if (country != null)
            {
                viewModel.CountryId = Convert.ToInt32(country.Value.ToString());
            }            

            //DetermineType(viewModel, clientFieldData);
            var objetives = clientFieldData.FirstOrDefault(o => o.Name.Equals("objetives"));

            if (objetives != null)
            {
                viewModel.Objective = objetives.Value.ToString().Trim();
            }

            var activities = clientFieldData.FirstOrDefault(o => o.Name.Equals("activities"));

            if (activities != null)
            {
                viewModel.Activities = activities.Value.ToString().Trim();
            }

            var membersOutside = clientFieldData.FirstOrDefault(o => o.Name.Equals("membersOutside"));

            if (membersOutside != null)
            {
                viewModel.MembersOutside = membersOutside.Value.ToString().Trim();
            }

            if (viewModel.IsNewMission)
            {
                //New
                //Mission Team
                if (viewModel.MissionLeader == null)
                {
                    viewModel.MissionLeader = new List<MissionTeamMemberViewModel>();
                }

                var membersTeam = clientFieldData.Where(o => o.Name.Equals("newTeamName"));
                if (membersTeam.Count() > 0)
                {
                    var membersRole = clientFieldData.Where(o => o.Name.Equals("newMissionRole"));
                    for (int i = 0; i < membersTeam.Count(); i++)
                    {
                        var teamMember = new MissionTeamMemberViewModel
                        {
                            NameId = Convert.ToInt32(membersTeam.ElementAt(i).Value),
                            MissionRoleId = Convert.ToInt32(membersRole.ElementAt(i).Value),
                            isExpire = false
                        };
                        viewModel.MissionLeader.Add(teamMember);
                    }
                }

                //mission contact
                if (viewModel.Contact == null)
                {
                    viewModel.Contact = new List<MissionContactViewModel>();
                }

                var contactName = clientFieldData.Where(o => o.Name.Equals("newCantactName"));
                if (contactName.Count() > 0)
                {
                    var institutionName = clientFieldData.Where(o => o.Name.Equals("newInstitutionName"));
                    var contactInformation = clientFieldData.Where(o => o.Name.Equals("newCantactInformation"));
                    for (int i = 0; i < contactName.Count(); i++)
                    {
                        if (contactName.ElementAt(i).Value != " " || institutionName.ElementAt(i).Value != " ")
                        {
                            var contact = new MissionContactViewModel
                            {
                                ContactName = (contactName.ElementAt(i).Value != " ") ? contactName.ElementAt(i).Value : null,
                                InstitutionId = (institutionName.ElementAt(i).Value != " ") ? Convert.ToInt32(institutionName.ElementAt(i).Value) : 0,
                                ContactInformation = (contactInformation.ElementAt(i).Value != " ") ? contactInformation.ElementAt(i).Value : null
                            };
                            viewModel.Contact.Add(contact);
                        }
                    }
                }
            }
            else
            { 
                //Edit
                //Mission member
                //New
                if (viewModel.MissionLeader == null)
                {
                    viewModel.MissionLeader = new List<MissionTeamMemberViewModel>();
                }

                var membersTeamNew = clientFieldData.Where(o => o.Name.Equals("newTeamName"));
                if (membersTeamNew.Count() > 0)
                {
                    var membersRoleNew = clientFieldData.Where(o => o.Name.Equals("newMissionRole"));
                    for (int i = 0; i < membersTeamNew.Count(); i++)
                    {
                        var teamMember = new MissionTeamMemberViewModel
                        {
                            NameId = Convert.ToInt32(membersTeamNew.ElementAt(i).Value),
                            MissionRoleId = Convert.ToInt32(membersRoleNew.ElementAt(i).Value),
                            isExpire = false
                        };
                        viewModel.MissionLeader.Add(teamMember);
                    }
                }

                //Update
                if (viewModel.EditMissionMember == null)
                {
                    viewModel.EditMissionMember = new List<MissionTeamMemberViewModel>();
                }

                var membersTeamIdEdit = clientFieldData.Where(o => o.Name.Equals("editTeamMemberId"));
                if (membersTeamIdEdit.Count() > 0)
                {
                    var membersTeamEdit = clientFieldData.Where(o => o.Name.Equals("editTeamName"));
                    var membersRoleEdit = clientFieldData.Where(o => o.Name.Equals("editMissionRole"));
                    var memberIsExpireEdit = clientFieldData.Where(o => o.Name.Equals("editIsExpire"));

                    for (int i = 0; i < membersTeamIdEdit.Count(); i++)
                    {
                        var teamMember = new MissionTeamMemberViewModel
                        {
                            MissionTeamMemberId = Convert.ToInt32(membersTeamIdEdit.ElementAt(i).Value),
                            NameId = Convert.ToInt32(membersTeamEdit.ElementAt(i).Value),
                            MissionRoleId = Convert.ToInt32(membersRoleEdit.ElementAt(i).Value),
                            isExpire = Convert.ToBoolean(memberIsExpireEdit.ElementAt(i).Value)
                        };
                        viewModel.EditMissionMember.Add(teamMember);
                    }
                }

                //Delete
                var deleteMembers = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteMissionTeam"));
                if (deleteMembers != null)
                {
                    string[] deleteMem = deleteMembers.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    if (viewModel.DeleteMissionMember == null)
                    {
                        viewModel.DeleteMissionMember = new List<int>();
                    }

                    foreach (string member in deleteMem)
                    {
                        viewModel.DeleteMissionMember.Add(Convert.ToInt32(member));
                    }
                }

                //Contact
                //New
                if (viewModel.Contact == null)
                {
                    viewModel.Contact = new List<MissionContactViewModel>();
                }

                var contactNameNew = clientFieldData.Where(o => o.Name.Equals("newCantactName"));
                if (contactNameNew.Count() > 0)
                {
                    var institutionNameNew = clientFieldData.Where(o => o.Name.Equals("newInstitutionName"));
                    var contactInformationNew = clientFieldData.Where(o => o.Name.Equals("newCantactInformation"));
                    for (int i = 0; i < contactNameNew.Count(); i++)
                    {
                        if (contactNameNew.ElementAt(i).Value != " " || institutionNameNew.ElementAt(i).Value != " ")
                        {
                            var contact = new MissionContactViewModel
                            {
                                ContactName = (contactNameNew.ElementAt(i).Value != " ") ? contactNameNew.ElementAt(i).Value : null,
                                InstitutionId = (institutionNameNew.ElementAt(i).Value != " ") ? Convert.ToInt32(institutionNameNew.ElementAt(i).Value) : 0,
                                ContactInformation = (contactInformationNew.ElementAt(i).Value != " ") ? contactInformationNew.ElementAt(i).Value : null
                            };
                            viewModel.Contact.Add(contact);
                        }
                    }
                }

                //Update
                if (viewModel.EditContact == null)
                {
                    viewModel.EditContact = new List<MissionContactViewModel>();
                }

                var contactIdEdit = clientFieldData.Where(o => o.Name.Equals("editContactId"));
                if (contactIdEdit.Count() > 0)
                {
                    var contactNameEdit = clientFieldData.Where(o => o.Name.Equals("contactName"));
                    var institutionNameEdit = clientFieldData.Where(o => o.Name.Equals("SearchInstitutionName"));
                    var contactInformationEdit = clientFieldData.Where(o => o.Name.Equals("contactInformation"));
                    for (int i = 0; i < contactIdEdit.Count(); i++)
                    {
                        if (contactNameEdit.ElementAt(i).Value != " " || institutionNameEdit.ElementAt(i).Value != " ")
                        {
                            var contact = new MissionContactViewModel
                            {
                                MissionContactId = Convert.ToInt32(contactIdEdit.ElementAt(i).Value),
                                ContactName = (contactNameEdit.ElementAt(i).Value != " ") ? contactNameEdit.ElementAt(i).Value : null,
                                InstitutionId = (institutionNameEdit.ElementAt(i).Value != " ") ? Convert.ToInt32(institutionNameEdit.ElementAt(i).Value) : 0,
                                ContactInformation = (contactInformationEdit.ElementAt(i).Value != " ") ? contactInformationEdit.ElementAt(i).Value : null
                            };
                            viewModel.EditContact.Add(contact);
                        }
                        else
                        {
                            var idContact = Convert.ToInt32(contactIdEdit.ElementAt(i).Value);
                            if (contactNameEdit.ElementAt(i).Value == " " && institutionNameEdit.ElementAt(i).Value == " " && idContact != 0)
                            {
                                if (viewModel.DeleteContacts == null)
                                {
                                    viewModel.DeleteContacts = new List<int>();
                                }

                                viewModel.DeleteContacts.Add(idContact);
                            }
                        }
                    }
                }

                //Delete
                var deleteContact = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteContacts"));
                if (deleteContact != null)
                {
                    string[] deleteC = deleteContact.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    if (viewModel.DeleteContacts == null)
                    {
                        viewModel.DeleteContacts = new List<int>();
                    }

                    foreach (string con in deleteC)
                    {
                        viewModel.DeleteContacts.Add(Convert.ToInt32(con));
                    }
                }
            }

            var isSaveOnly = clientFieldData.FirstOrDefault(o => o.Name.Equals("hiddenIsSaveOnly"));

            if (isSaveOnly != null)
            {
                bool result;
                bool.TryParse(isSaveOnly.Value, out result);

                viewModel.isSaveOnly = result;
            }
        }

        public static void UpdateMissionWorkFlowViewModel(this MissionsWorkflowViewModels viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel == null)
            {
                viewModel = new MissionsWorkflowViewModels();
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

            // Documents
            viewModel.UpdateMissionWorkflowDocuments(clientFieldData);
        }

        private static void DetermineType(MissionViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            //Verify if Type is ESG
            var ESGtype = clientFieldData.FirstOrDefault(o => o.Name.Equals("organizationalUnitText"));

            if (ESGtype != null)
            {
                if (ESGtype.Value.ToString() == "VPS/ESG")
                {
                    // if ESG get id from hidden
                    var typeESGIdHidden = clientFieldData.FirstOrDefault(o => o.Name.Equals("typeESGIdHidden"));
                    if (typeESGIdHidden != null)
                    {
                        viewModel.OrganizationalUnitId = Convert.ToInt32(typeESGIdHidden.Value.ToString());
                        return;
                    }
                }
            }

            //determine from where Type is is getted
            var hasOnlyOneOrgUnit = clientFieldData.FirstOrDefault(o => o.Name.Equals("hasOnlyOneOrgUnit"));
            ClientFieldData organizationalUnit = null;
            if (hasOnlyOneOrgUnit != null)
            {
                if (Convert.ToBoolean(hasOnlyOneOrgUnit.Value))  
                {
                    //if type is in texbox, get id from hidden
                    organizationalUnit = clientFieldData.FirstOrDefault(o => o.Name.Equals("organizationalUnitIdHidden"));
                }
                else
                {
                    // get the value from combobox
                    organizationalUnit = clientFieldData.FirstOrDefault(o => o.Name.Equals("organizationalUnit"));
                }

                if (organizationalUnit != null)
                {
                    viewModel.OrganizationalUnitId = Convert.ToInt32(organizationalUnit.Value.ToString());
                }
            }
        }
    }
}