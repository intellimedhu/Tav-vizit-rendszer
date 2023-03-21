using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;

namespace OrganiMedCore.DiabetesCareCenter.Core.Helpers
{
    public class CenterProfileReviewCheckResult
    {
        public ContentItem ContentItem { get; set; }

        public IActionResult Action { get; set; }

        public string CurrentRole { get; set; }
    }
}
