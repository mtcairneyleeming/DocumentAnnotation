using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DocumentAnnotation.Extensions
{
    public static class WebpackHtmlHelperExtensions
    {
        /// <summary>
        /// Return the generated partial view that contains the script tags for the Webpack bundle
        /// indicated by <see cref="bundleName"/>.
        /// </summary>
        /// <param name="helper">The HTML helper instance.</param>
        /// <param name="bundleName">The Webpack bundle name.</param>
        /// <returns>The partial content.</returns>
        public static Task<IHtmlContent> WebpackScriptsAsync(this IHtmlHelper helper, string bundleName)
        {
            return helper.PartialAsync("Partials/Generated/_Gen_" + bundleName + "_Scripts");
        }

        /// <summary>
        /// Return the generated partial view that contains the style (link) tags for the Webpack bundle
        /// indicated by <see cref="bundleName"/>.
        /// </summary>
        /// <param name="helper">The HTML helper instance.</param>
        /// <param name="bundleName">The Webpack bundle name.</param>
        /// <returns>The partial content.</returns>
        public static Task<IHtmlContent> WebpackStylesAsync(this IHtmlHelper helper, string bundleName)
        {
            return helper.PartialAsync("Partials/Generated/_Gen_" + bundleName + "_Styles");
        }

        /// <summary>
        /// Emits a script tag with type application/json that Webpack scripts can then load to
        /// retrieve data from the page.
        /// </summary>
        /// <param name="helper">The HTML helper instance.</param>
        /// <param name="id">The ID that webpack will load the data block by.</param>
        /// <param name="data">The data object that will be serialized to JSON.</param>
        /// <returns></returns>
        public static IHtmlContent JsonDataBlock(this IHtmlHelper helper, string id, object data)
        {
            return new HtmlFormattableString(
                "<script type=\"application/json\" id=\"{0}\">{1}</script>",
                id,
                new HtmlString(JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                })));
        }
     
    }
}