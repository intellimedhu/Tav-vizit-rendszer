using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.TagHelpers
{
    [HtmlTargetElement("doki-net-member")]
    public class DokiNetMemberTagHelper : TagHelper
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelper _displayHelper;


        [HtmlAttributeName("view-model")]
        public EditDokiNetMemberViewModel ViewModel { get; set; }

        [HtmlAttributeName("prefix")]
        public string Prefix { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public DokiNetMemberTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelperFactory)
        {
            _shapeFactory = shapeFactory;
            _displayHelper = displayHelperFactory;
        }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var shape = await _shapeFactory.CreateAsync("DokiNetMemberPicker", new { ViewModel, Prefix });
            context.Items.Add(typeof(IShape), shape);
            await output.GetChildContentAsync();
            output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
            output.TagName = null;
        }
    }
}
