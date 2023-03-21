using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrganiMedCore.Navigation.Models;
using OrganiMedCore.Navigation.Services;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Helpers;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.DiabetesCare.Services
{
    public class PatientProfileMenuItemProvider : IMenuItemProvider
    {
        public string MenuId => MenuIds.PatientProfileAdditonalNavigation;

        public IStringLocalizer T { get; set; }


        public PatientProfileMenuItemProvider(IStringLocalizer<PatientProfileMenuItemProvider> localizer)
        {
            T = localizer;
        }


        public Task BuildMenuAsync(Menu builder, object additionalData = null)
        {
            if (additionalData is PatientProfileNavHelper navHelper)
            {
                builder
                    .Add(new MenuItem()
                    {
                        IsAspRouted = true,
                        Condition = true,
                        //Permission = Permissions.
                        RouteValueDictionary = new RouteValueDictionary()
                        {
                            { "area", "OrganiMedCore.Organization.DiabetesCare" },
                            { "controller", "Patients" },
                            { "action", "Index" },
                            { "eVisitPatientProfileId", navHelper.EVisitPatientProfileId },
                        },
                        Text = T["AGP"],
                        Disabled = navHelper.IsNew,
                        IsActive = navHelper.ActiveNavigation == "Agp"
                    });
            }

            return Task.CompletedTask;
        }
    }
}
