using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrganiMedCore.Manager.Constants;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.Drivers
{
    public class EVisitUserButtonsDisplayDriver : DisplayDriver<User>
    {
        public override IDisplayResult Edit(User user)
        {
            return Dynamic("EVisitUserSaveButtons_Edit").Location("Actions").OnGroup(GroupIds.EVisitUserEditor);
        }

        public override Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            return Task.FromResult(Edit(user));
        }
    }
}
