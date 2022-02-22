using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using UAParser;

namespace BiblioMit.Models.VM
{
    public static class CSPTag
    {
        public static HashSet<string> BaseUri { get; } = new HashSet<string>
        {
            "base-uri", "'self'"
        };
        public static bool BlockAllMixedContent { get; set; } = true;
        public static HashSet<string> DefaultSrc { get; } = new HashSet<string>
        {
            "default-src", "'self'"
        };
        public static HashSet<string> ConnectSrc { get; } = new HashSet<string>
        {
            "connect-src", "'self'", "wss://bibliomit:*", "ws://bibliomit:*",
            "https://fonts.googleapis.com/", "https://fonts.gstatic.com/",
            "https://www.facebook.com/", "https://web.facebook.com/",
            "https://www.google-analytics.com/"
        };
        public static HashSet<string> FrameSrc { get; } = new HashSet<string>
        {
            "frame-src", "'self'",
            "https://www.facebook.com/", "https://web.facebook.com/"
        };
        public static HashSet<string> ImgSrc { get; } = new HashSet<string>
        {
            "img-src","data:","blob:","'self'"
        };
        public static HashSet<string> ObjectSrc { get; } = new HashSet<string>
        {
            "object-src","'none'"
        };
        public static HashSet<string> ScriptSrc { get; } = new HashSet<string>
        {
            "script-src","'self'"
        };
        public static HashSet<string> ScriptSrcElem { get; } = new HashSet<string>
        {
            "'self'",
            "https://connect.facebook.net/","https://fonts.googleapis.com/",
            "'sha256-963QZmPvTsPUE3uwDlRCl3mQq0qQZQXE3XI9lYpIIVg='"
        };
        public static HashSet<string> StyleSrc { get; } = new HashSet<string>
        {
            "style-src","'self'", "'unsafe-inline'"
        };
        public static HashSet<string> StyleSrcElem { get; } = new HashSet<string>
        {
            "'self'","https://fonts.googleapis.com/", "'unsafe-inline'"
        };
        public static HashSet<string> FontSrc { get; } = new HashSet<string>
        {
            "font-src","'self'","data:","https://fonts.gstatic.com/","https://fonts.googleapis.com/"
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
                ScriptSrc.UnionWith(ScriptSrcElem);
                StyleSrc.UnionWith(StyleSrcElem);
                if (ScriptSrc.Contains("'unsafe-inline'"))
                {
                    ScriptSrc.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
                }
                scriptSrc = string.Join(separator, "script-src", "'self'",
                    "https://connect.facebook.net/", "https://fonts.googleapis.com/",
                    ScriptSrc);
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
                    string.Join(separator, "script-src", "'self'", ScriptSrc),
                string.Join(separator, "script-src-elem", "'self'",
                "https://connect.facebook.net/", "https://fonts.googleapis.com/",
                ScriptSrcElem));
            }

            return string.Join($";{separator}",
                string.Join(separator, BaseUri),
                BlockAllMixedContent ? "block-all-mixed-content" : null,
                string.Join(separator, DefaultSrc),
                string.Join(separator, $"ws://{baseUrl}", 
                ConnectSrc),
                string.Join(separator, FrameSrc),
                string.Join(separator, ImgSrc),
                string.Join(separator, ObjectSrc),
                string.Join(separator, ScriptSrc),
                string.Join(separator, "script-src-elem", ScriptSrcElem),
                string.Join(separator, StyleSrc),
                string.Join(separator, "style-src-elem", StyleSrcElem),
                string.Join(separator, FontSrc),
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
