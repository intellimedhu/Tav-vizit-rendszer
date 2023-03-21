using Microsoft.AspNetCore.Mvc.Localization;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class BarButton
    {
        public string Text { get; set; }

        public string Action { get; set; }

        public string Classes { get; set; }

        public string IconClasses { get; set; }

        /// <summary>
        /// Button or link
        /// </summary>
        public bool IsButton { get; set; }


        public static BarButton Cancel(IHtmlLocalizer t, string link)
            => new BarButton()
            {
                Action = link,
                Text = t["Vissza"].Value,
                Classes = "btn btn-lg btn-secondary",
                IconClasses = "fas fa-chevron-left"
            };

        public static BarButton Submit(IHtmlLocalizer t)
            => new BarButton()
            {
                Action = "submit",
                IsButton = true,
                Text = t["Mentés"].Value,
                Classes = "btn btn-lg btn-primary",
                IconClasses = "fas fa-save"
            };
    }
}
