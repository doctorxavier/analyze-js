using System;

using IDB.MW.Application.MrBlueModule.ViewModels.DynamicQuestionnaire;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models.Fake
{
    [Serializable]
    public class QuestionnarieDemo1ViewModel
    {
        public int Id { get; set; }

        public DynamicQuestionnaireViewModel ViewModel { get; set; }
    }
}