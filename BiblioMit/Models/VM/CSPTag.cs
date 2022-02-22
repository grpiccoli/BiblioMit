using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Linq.Expressions;
using UAParser;

namespace BiblioMit.Models.VM
{
    public static class CSPTag
    {
        public static HashSet<string> BaseUri { get; } = new HashSet<string>
        {
            "'self'"
        };
        public static bool BlockAllMixedContent { get; set; } = true;
        public static HashSet<string> DefaultSrc { get; } = new HashSet<string>
        {
            "'self'"
        };
        public static HashSet<string> ConnectSrc { get; } = new HashSet<string>
        {
            "'self'", "wss://bibliomit:*", "ws://bibliomit:*",
            "https://fonts.googleapis.com/", "https://fonts.gstatic.com/",
            "https://www.facebook.com/", "https://web.facebook.com/",
            "https://www.google-analytics.com/"
        };
        public static HashSet<string> FrameSrc { get; } = new HashSet<string>
        {
            "'self'",
            "https://www.facebook.com/", "https://web.facebook.com/"
        };
        public static HashSet<string> ImgSrc { get; } = new HashSet<string>
        {
            "data:","blob:","'self'"
        };
        public static HashSet<string> ObjectSrc { get; } = new HashSet<string>
        {
            "'none'"
        };
        public static HashSet<string> ScriptSrc { get; } = new HashSet<string>
        {
            "'self'"
        };
        public static HashSet<string> ScriptSrcElem { get; } = new HashSet<string>
        {
            "'self'",
            "https://connect.facebook.net/","https://fonts.googleapis.com/",
            "'sha256-963QZmPvTsPUE3uwDlRCl3mQq0qQZQXE3XI9lYpIIVg='"
        };
        public static HashSet<string> StyleSrc { get; } = new HashSet<string>
        {
            "'self'", "'unsafe-inline'"
        };
        public static HashSet<string> StyleSrcElem { get; } = new HashSet<string>
        {
            "'self'","https://fonts.googleapis.com/", "'unsafe-inline'"
        };
        public static HashSet<string> FontSrc { get; } = new HashSet<string>
        {
            "'self'","data:","https://fonts.gstatic.com/","https://fonts.googleapis.com/"
        };
        public static bool UpgradeInsecureRequests { get; set; } = true;
        public static HashSet<string> AccessControlUrls { get; } = new HashSet<string>
        {
            "https://web.facebook.com"
        };
        public static string GetString(HttpRequest request)
        {
            HostString baseUrl = request.Host;
            StringValues userAgent = request.Headers[HeaderNames.UserAgent];
            Parser uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);
#if DEBUG
            ScriptSrcElem.Add($"localhost:*");
            ConnectSrc.Add($"localhost:*");
            ConnectSrc.Add($"wss://localhost:*");
            ConnectSrc.Add($"ws://localhost:*");
            ConnectSrc.Add($"https://dc.services.visualstudio.com/");
#endif
            char separator = ' ';
            //string noport = baseUrl.Value.Split(":")[0];
            StyleSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
            StyleSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));

            string family = c.UA.Family.ToUpperInvariant();
            string scriptSrc = string.Empty;
            string styleSrc = string.Empty;
            if (family.Contains("SAFARI") || family.Contains("FIREFOX"))
            {
                //ScriptSrcElem.Remove("script-src-elem");
                //StyleSrcElem.Remove("style-src-elem");
                ScriptSrc.UnionWith(ScriptSrcElem);
                StyleSrc.UnionWith(StyleSrcElem);
                if (ScriptSrc.Contains("'unsafe-inline'"))
                {
                    ScriptSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                scriptSrc = string.Join(separator, ScriptSrc.Prepend("script-src"));
                styleSrc = string.Join(separator, StyleSrc.Prepend("style-src"));
            }
            else
            {
                if (ScriptSrcElem.Contains("'unsafe-inline'"))
                {
                    ScriptSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                if (ScriptSrc.Contains("'unsafe-inline'"))
                {
                    ScriptSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                scriptSrc = string.Join($";{separator}",
                    string.Join(separator, ScriptSrc.Prepend("script-src")),
                string.Join(separator, ScriptSrcElem.Prepend("script-src-elem")));
                styleSrc = string.Join($";{separator}",
                    string.Join(separator, StyleSrc.Prepend("style-src")),
                string.Join(separator, StyleSrcElem.Prepend("style-src-elem")));
            }

            ConnectSrc.Add($"ws://{baseUrl}");

            return string.Join($";{separator}",
                string.Join(separator, BaseUri.Prepend("base-uri")),
                BlockAllMixedContent ? "block-all-mixed-content" : null,
                string.Join(separator, DefaultSrc.Prepend("default-src")),
                string.Join(separator, ConnectSrc.Prepend("connect-src")),
                string.Join(separator, FrameSrc.Prepend("frame-src")),
                string.Join(separator, ImgSrc.Prepend("img-src")),
                string.Join(separator, ObjectSrc.Prepend("object-src")),
                scriptSrc,
                styleSrc,
                string.Join(separator, FontSrc.Prepend("font-src")),
                UpgradeInsecureRequests ? "upgrade-insecure-requests" : null);
        }
        public static string GetAccessControlString() => string.Join(" ", AccessControlUrls);
        public static void Clear()
        {
            BaseUri.Clear();
            DefaultSrc.Clear();
            ConnectSrc.Clear();
            FrameSrc.Clear();
            ImgSrc.Clear();
            ObjectSrc.Clear();
            ScriptSrc.Clear();
            ScriptSrcElem.Clear();
            StyleSrc.Clear();
            StyleSrcElem.Clear();
            FontSrc.Clear();
        }
    }
}
