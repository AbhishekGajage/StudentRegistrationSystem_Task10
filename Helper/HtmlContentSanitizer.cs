// Requires the NuGet package "HtmlSanitizer" (by mganss). Install via:
//   Install-Package HtmlSanitizer
// NOTE on the using directive below: older package versions (< 7.0) use the
// namespace "Ganss.XSS"; versions 7.0+ renamed it to "Ganss.Xss". If this
// doesn't compile, just flip the casing on the line below to match whichever
// version NuGet installed.
using Ganss.Xss;

/// <summary>
/// Whitelists exactly the tags/attributes/CSS properties TinyMCE's configured
/// toolbar can actually produce, and strips everything else -- &lt;script&gt;,
/// event handler attributes (onclick, onerror, ...), javascript: URLs, iframes,
/// forms, etc. This is the real security boundary for requirement #10 (XSS
/// prevention): the editor's own client-side restrictions are just UX and can be
/// bypassed by anyone who POSTs directly to the save endpoint, so this must run
/// server-side on every save, not just rely on what the browser sent.
/// </summary>
public static class HtmlContentSanitizer
{
    public static string Sanitize(string html)
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "span", "div",
            "strong", "b", "em", "i", "u", "s", "strike", "sub", "sup",
            "ul", "ol", "li",
            "a", "img",
            "table", "thead", "tbody", "tr", "td", "th",
            "hr", "h1", "h2", "h3", "h4", "h5", "h6",
            "blockquote", "pre", "code"
        })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[]
        {
            "href", "src", "alt", "title", "style",
            "width", "height", "align", "target", "rel",
            "colspan", "rowspan", "class"
        })
        {
            sanitizer.AllowedAttributes.Add(attr);
        }

        sanitizer.AllowedCssProperties.Clear();
        foreach (var css in new[]
        {
            "color", "background-color", "text-align",
            "font-weight", "font-style", "text-decoration",
            "font-size", "font-family", "line-height",
            "margin", "margin-left", "margin-right",
            "padding", "width", "height", "border", "border-collapse"
        })
        {
            sanitizer.AllowedCssProperties.Add(css);
        }

        // Blocks "javascript:", "data:", and anything else besides genuine links/images.
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        return sanitizer.Sanitize(html);
    }
}
