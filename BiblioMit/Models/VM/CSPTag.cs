namespace BiblioMit.Models.VM
{
    public static class CSPTag
    {
        public static HashSet<string> BaseUri { get; } = new HashSet<string>();
        public static bool BlockAllMixedContent { get; set; } = true;
        public static HashSet<string> DefaultSrc { get; } = new HashSet<string>();
        public static HashSet<string> ConnectSrc { get; } = new HashSet<string>();
        public static HashSet<string> ImgSrc { get; } = new HashSet<string>();
        public static HashSet<string> FontSrc { get; } = new HashSet<string>();
        public static HashSet<string> ObjectSrc { get; } = new HashSet<string>();
        public static HashSet<string> ScriptSrc { get; } = new HashSet<string>();
        public static HashSet<string> ScriptSrcElem { get; } = new HashSet<string>();
        public static HashSet<string> StyleSrc { get; } = new HashSet<string>();
        public static HashSet<string> StyleSrcElem { get; } = new HashSet<string>();
        public static HashSet<string> FrameSrc { get; } = new HashSet<string>();
        public static bool UpgradeInsecureRequests { get; set; } = true;
        public static HashSet<string> AccessControlUrls { get; } = new HashSet<string>();
        public static string GetString(HostString baseUrl)
        {
#if DEBUG
            ScriptSrcElem.Add($"localhost:*");
            ConnectSrc.Add($"localhost:*");
            ConnectSrc.Add($"wss://localhost:*");
            ConnectSrc.Add($"ws://localhost:*");
            ConnectSrc.Add($"https://dc.services.visualstudio.com/");
            ScriptSrcElem.Add("'sha256-963QZmPvTsPUE3uwDlRCl3mQq0qQZQXE3XI9lYpIIVg='");
#endif
            StyleSrcElem.Add("'unsafe-inline'");
            //string noport = baseUrl.Value.Split(":")[0];
            if (StyleSrcElem.Contains("'unsafe-inline'"))
                StyleSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
            if (ScriptSrcElem.Contains("'unsafe-inline'"))
                ScriptSrcElem.RemoveWhere(s => s.StartsWith("'nonce", StringComparison.Ordinal) || s.StartsWith("'sha", StringComparison.Ordinal));
            return $"base-uri 'self' {string.Join(" ", BaseUri)} ; " +
                $"block-all-mixed-content;" +
                $"default-src 'self' {string.Join(" ", DefaultSrc)} ; " +
                $"connect-src 'self' ws://{baseUrl} https://fonts.googleapis.com/ https://fonts.gstatic.com/ https://www.google-analytics.com/ https://www.facebook.com/ https://web.facebook.com/ {string.Join(" ", ConnectSrc)} ; " +
                $"frame-src 'self' https://www.facebook.com/ https://web.facebook.com/ {string.Join(" ", FrameSrc)} ; " +
                $"img-src data: blob: 'self' {string.Join(" ", ImgSrc)} ; " +
                $"object-src 'none' {string.Join(" ", ObjectSrc)} ; " +
                $"script-src 'self' {string.Join(" ", ScriptSrc)} ; " +
                $"script-src-elem 'self' https://connect.facebook.net/ https://fonts.googleapis.com/ {string.Join(" ", ScriptSrcElem)} ; " +
                $"style-src 'self' {string.Join(" ", StyleSrc)} ; " +
                $"style-src-elem 'self' https://fonts.googleapis.com/ {string.Join(" ", StyleSrcElem)} ; " +
                $"font-src 'self' data: https://fonts.gstatic.com/ https://fonts.googleapis.com/ {string.Join(" ", FontSrc)} ; " +
                $"upgrade-insecure-requests;";
        }
        public static string GetAccessControlString()
        {
            return string.Join(" ", AccessControlUrls);
        }
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
