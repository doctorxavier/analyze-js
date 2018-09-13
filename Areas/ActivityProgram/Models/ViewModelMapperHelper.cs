using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.ActivityProgram.Services;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Models
{
    public class ViewModelMapperHelper
    {
        #region fields
        private readonly IActivityProgramServices _AapServices;
        #endregion

        #region Constructor
        public ViewModelMapperHelper(
            IActivityProgramServices AapServices)
        {
            _AapServices = AapServices;
        }
        #endregion

        public List<SelectListItem> ConvertToSelectListItem(IList<ListItemViewModel> listResponse)
        {
            var list =
                listResponse.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            return list;
        }

        public List<SelectListItem> GetDisplayedOptions()
        {
            var selectableDisplayed = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Display 20",
                    Value = "20"
                },
                new SelectListItem
                {
                    Text = "Display 50",
                    Value = "50"
                },
                new SelectListItem
                {
                    Text = "Display All",
                    Value = "All"
                }
            };
            return selectableDisplayed;
        }
    }
}