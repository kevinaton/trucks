using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DispatcherWeb.Web.Utils
{
    [HtmlTargetElement("a", Attributes = "asp-conditional")]
    public class ConditionalAnchorTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            bool showAnchor = false;
            string attributeStringValue = context.AllAttributes["asp-conditional"]?.Value.ToString();

            if (!String.IsNullOrWhiteSpace(attributeStringValue))
            {
                bool attributeBoolValue = false;
                bool.TryParse(attributeStringValue, out attributeBoolValue);
                if (attributeBoolValue)
                {
                    showAnchor = true;
                }
            }

            if (!showAnchor)
            {
                output.SuppressOutput();

                var childContent = await output.GetChildContentAsync();
                output.Content.SetHtmlContent(childContent);
            }
            else
            {
                output.Attributes.Remove(output.Attributes["asp-conditional"]);
            }

        }
    }
}
