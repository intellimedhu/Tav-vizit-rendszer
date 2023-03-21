using IntelliMed.DokiNetIntegration.Constants;
using IntelliMed.DokiNetIntegration.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Drivers
{
    public class DokiNetMemberDisplayDriver : DisplayDriver<User>
    {
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;


        public IHtmlLocalizer T { get; set; }


        public DokiNetMemberDisplayDriver(
            IDokiNetService dokiNetService,
            ILogger<DokiNetMemberDisplayDriver> logger,
            IHtmlLocalizer<DokiNetMemberDisplayDriver> htmlLocalizer)
        {
            _dokiNetService = dokiNetService;
            _logger = logger;

            T = htmlLocalizer;
        }


        public override IDisplayResult Display(User user)
            => Initialize<SummaryAdminUserViewModel>("DokiNetMemberButtons", model => model.User = user)
                .Location("SummaryAdmin", "Actions:2");

        public override IDisplayResult Edit(User user)
            => Initialize<EditDokiNetMemberViewModel>("DokiNetMember_Edit", viewModel =>
            {
                var member = user.As<DokiNetMember>();

                viewModel.UpdateViewModel(member);
                viewModel.Prefix = Prefix;
            })
            .Location("Content:1")
            .OnGroup(GroupIds.DokiNetMemberEditor);

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            if (context.GroupId == GroupIds.DokiNetMemberEditor)
            {
                var model = new EditDokiNetMemberViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                    DokiNetMember member = null;
                    if (model.MemberRightId.HasValue)
                    {
                        member = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(model.MemberRightId.Value);
                    }

                    user.UpdateUsersDokiNetMemberData(member);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + model.MemberRightId.Value);
                    context.Updater.ModelState.AddModelError(
                        string.Empty,
                        T["Hiba történt a doki.NET-el történő kapcsolat során."].Value);
                    context.Updater.ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return Edit(user);
        }
    }
}
