using Ganss.Xss;
using Markdig;

namespace DocumentAnnotation.Services
{
    /// <summary>
    /// Renders user-written annotation Markdown to HTML that is safe to emit with Html.Raw. Markdig renders it (raw HTML
    /// in the Markdown is allowed, as on the client), then the result is passed through an allow-list HTML sanitiser:
    /// no scripts, event handlers, iframes, forms or styles, and only http, https, mailto and tel (or relative) URLs
    /// (so javascript: and data: links and images are dropped).
    /// The client (app/js/Markdown.ts: marked + DOMPurify) is configured to produce the same result, so a note looks the
    /// same before and after a reload.
    /// </summary>
    public static class MarkdownRenderer
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .Build();

        private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

        // HtmlSanitizer instances are not documented as safe for concurrent use
        private static readonly object SanitizerLock = new object();

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedSchemes.Clear();
            foreach (var scheme in new[] {"http", "https", "mailto", "tel"})
            {
                sanitizer.AllowedSchemes.Add(scheme);
            }

            // Not useful in a note, and they allow UI spoofing (fake login forms, restyling the page). The client's
            // DOMPurify config (app/js/Markdown.ts) forbids the same things.
            foreach (var tag in new[] {"form", "input", "button", "select", "textarea", "option", "label", "style"})
            {
                sanitizer.AllowedTags.Remove(tag);
            }

            foreach (var attribute in new[] {"style", "target"})
            {
                sanitizer.AllowedAttributes.Remove(attribute);
            }

            return sanitizer;
        }

        public static string ToSafeHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var html = Markdown.ToHtml(markdown, Pipeline);
            lock (SanitizerLock)
            {
                return Sanitizer.Sanitize(html);
            }
        }
    }
}
