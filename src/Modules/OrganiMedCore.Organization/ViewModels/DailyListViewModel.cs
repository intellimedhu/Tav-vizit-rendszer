using System.Collections.Generic;

namespace OrganiMedCore.Organization.ViewModels
{
    public class DailyListViewModel
    {
        public List<DailyListItemViewModel> DailyListItems { get; set; }


        public DailyListViewModel()
        {
            DailyListItems = new List<DailyListItemViewModel>();
        }
    }
}
