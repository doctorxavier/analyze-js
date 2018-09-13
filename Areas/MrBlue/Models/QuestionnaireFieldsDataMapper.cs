using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.MrBlueModule.ViewModels.DynamicQuestionnaire;
using IDB.MW.Infrastructure.Helpers;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models
{
    public static class QuestionnaireFieldsDataMapper
    {
        public static List<GivenAnswerViewModel> ProcessAnswers(this List<DynamicQuestionnaireViewModel> questionnaireViewModel, ClientFieldData[] clientFieldData)
        {
            List<GivenAnswerViewModel> givenAnswerList = new List<GivenAnswerViewModel>();

            foreach (var questionnaire in questionnaireViewModel)
            {
                var dynamicGroupIdList = questionnaire.QuestionGroups
                    .Traverse(x => x.QuestionGroups)
                    .Select(x => x.GroupId.ToString());
                var clientFieldDataFiltered = clientFieldData.Where(x => x.ExtraData.Any()
                    && x.ExtraData.ContainsKey("data-persist-questiongroupid")
                    && dynamicGroupIdList.Contains(x.ExtraData["data-persist-questiongroupid"])).ToArray();
                givenAnswerList.AddRange(questionnaire.ProcessAnswers(clientFieldDataFiltered));
            }

            return givenAnswerList;
        }

        private static List<GivenAnswerViewModel> ProcessAnswers(this DynamicQuestionnaireViewModel questionnaireViewModel, ClientFieldData[] clientFieldData)
        {
            var response = new List<GivenAnswerViewModel>();
            var dinamycForm = clientFieldData.Where(x => x.ExtraData.Any(y => y.Key == "data-persist-questionid") && !x.ExtraData.Any(y => y.Key == "data-persist-ignore"));
            var groupForm = dinamycForm.GroupBy(x => x.ExtraData["data-persist-questionid"]);
            foreach (var group in groupForm)
            {
                string value = string.Empty;
                if (group.Count() == 1)
                {
                    var item = group.ElementAt(0);

                    var answerId = int.Parse(item.ExtraData.Single(x => x.Key == "data-persist-answerid").Value);
                    if (answerId == 0)
                    {
                        value = string.IsNullOrEmpty(item.Value) ? null : item.Value;
                    }
                    else
                    {
                        value = answerId.ToString();
                    }
                }
                else
                {
                    var selectedElements = group.Where(x => x.Value.ToUpper() == "TRUE").Select(x => x.ExtraData.Single(y => y.Key == "data-persist-answerid").Value);
                    value = string.Join(",", selectedElements);
                }

                var answer = new GivenAnswerViewModel()
                {
                    QuestionId = int.Parse(group.Key),
                    VersionId = questionnaireViewModel.VersionId,
                    AnswerValue = value
                };
                response.Add(answer);
            }

            return response;
        }
    }
}