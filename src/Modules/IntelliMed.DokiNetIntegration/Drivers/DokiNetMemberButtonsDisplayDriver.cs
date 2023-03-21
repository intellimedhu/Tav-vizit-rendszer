using IntelliMed.DokiNetIntegration.Constants;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Drivers
{
    public class DokiNetMemberButtonsDisplayDriver : DisplayDriver<User>
    {
        public override IDisplayResult Edit(User model)
            => Dynamic("DokiNetMemberSaveButtons_Edit")
            .Location("Actions")
            .OnGroup(GroupIds.DokiNetMemberEditor);

        public override Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
            => Task.FromResult(Edit(user));
    }
}
