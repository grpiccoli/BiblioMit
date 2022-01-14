using BiblioMit.Extensions;
using BiblioMit.Models.VM;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BiblioMit.Services
{
    public class SourcesModel
    {
        public SourcesModel(LibManLibrary l, string file)
        {
            string url;
            switch (l.Provider)
            {
                case "cdnjs":
                    l.Library = l.Library.Replace("@", "/");
                    url = "cdnjs.cloudflare.com/ajax/libs";
                    WgetArgs = $"https://{url}/{l.Library}/{file} -O {l.Destination}/{file}";
                    break;
                case "unpkg":
                    url = "unpkg.com";
                    WgetArgs = $"https://{url}/{l.Library}/{file} -O {l.Destination}/{file}";
                    break;
                default:
                    WgetArgs = $"https://unpkg.com/{l.Library}/{file} -O {l.Destination}/{file}";
                    url = "cdn.jsdelivr.net/npm";
                    break;
            }
            Fallback = $"{l.Destination}/{file}";
            //https required for this
            Href = $"https://{url}/{l.Library}/{file}";
            Extension = Path.GetExtension(file).TrimStart('.');
            Hash = Extensions.Hash.Get512Async(new Uri(Href)).Result;
            Preload = !l.Library.StartsWith("nanogallery2");
            LibType = Extension switch
            {
                "js" => LibType.jsRemote,
                "css" => LibType.cssRemote,
                _ => Preload ? LibType.fontRemotePreload : LibType.fontRemote
            };
        }
        public SourcesModel(BundleConfig bundle)
        {
            Href = bundle.OutputFileName.Replace("wwwroot", "~");
            Extension = Path.GetExtension(Href).TrimStart('.');
            LibType = Extension switch
            {
                "js" => LibType.jsLocal,
                "css" => LibType.cssLocal,
                _ => LibType.fontLocal
            };
        }
        public SourcesModel(Compile compile)
        {
            Href = compile.OutputFile.Replace("wwwroot", "~");
            Extension = Path.GetExtension(Href).TrimStart('.');
            LibType = LibType.cssLocal;
        }
        public string? WgetArgs { get; set; }
        public string Href { get; set; }
        public string? Hash { get; set; }
        public string? Fallback { get; set; }
        public string? Extension { get; set; }
        public bool Preload { get; set; } = true;
        public LibType LibType { get; set; }
    }
    public enum LibType
    {
        cssLocal,
        cssRemote,
        jsLocal,
        jsRemote,
        fontLocal,
        fontRemote,
        fontRemotePreload
    }
    public static class Libman
    {
        private const string _latestVersion = "1.0";
        private const string _fileName = "libman.json";
        private const string _bundler = "bundleconfig.json";
        private const string _compiler = "compilerconfig.json";
        private static SortedDictionary<string, HashSet<SourcesModel>> Libs { get; set; } = new();
        public static void LoadJson()
        {
            using StreamReader r = new(_fileName);
            string json = r.ReadToEnd();
            Libs? libs = JsonSerializer.Deserialize<Libs>(json, JsonCase.Camel);
            if (libs == null || libs.Libraries == null) throw new FileLoadException($"Unable to read file {_fileName}");
            Libs = new SortedDictionary<string, HashSet<SourcesModel>>(libs.Libraries.ToDictionary(
                a => Regex.Replace(a.Library, @"@[^@]+$", ""),
                a => new HashSet<SourcesModel>(a.Files.Where(f => !f.EndsWith("gif") && !f.EndsWith("png")).Select(f => new SourcesModel(a, f)))
                ));
            PlatformID os = Environment.OSVersion.Platform;
            if (PlatformID.Win32NT != os && libs.Version == _latestVersion)
            {
                foreach (HashSet<SourcesModel> lib in Libs.Values)
                {
                    foreach (SourcesModel file in lib)
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "wget",
                                Arguments = file.WgetArgs,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            }
                        };
                        var s = string.Empty;
                        var e = string.Empty;
                        process.OutputDataReceived += (sender, data) => s += data.Data;
                        process.ErrorDataReceived += (sender, data) => e += data.Data;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        process.Close();
                    }
                }
            }
            //bundler
            using StreamReader b = new(_bundler);
            json = b.ReadToEnd();
            IEnumerable<BundleConfig>? bundles = JsonSerializer.Deserialize<IEnumerable<BundleConfig>>(json, JsonCase.Camel);
            if (bundles != null)
            {
                foreach (BundleConfig bundle in bundles)
                {
                    string key = Regex.Replace(bundle.OutputFileName, @"^wwwroot/.*/(.*)(.min)?.(css|js|woff2|woff|ttf)$", "$1").Replace(".min", "");
                    if (!Libs.ContainsKey(key))
                        Libs[key] = new HashSet<SourcesModel>();
                    Libs[key].Add(new SourcesModel(bundle));
                }
            }
            using StreamReader c = new(_compiler);
            string jsonC = c.ReadToEnd();
            IEnumerable<Compile>? compiles = JsonSerializer.Deserialize<IEnumerable<Compile>>(jsonC, JsonCase.Camel);
            if (compiles != null)
            {
                foreach (Compile compile in compiles)
                {
                    string key = Regex.Replace(compile.OutputFile, @"^wwwroot/.*/(.*).css$", "$1");
#if !DEBUG
                    .Replace(".css", ".min.css")
#endif
                        ;
                    if (!Libs.ContainsKey(key))
                        Libs[key] = new HashSet<SourcesModel>();
                    Libs[key].Add(new SourcesModel(compile));
                }
            }
        }
#if DEBUG
        public static HashSet<SourcesModel> GetLibs(string lib) =>
            Libs[lib];
#else
        public static HashSet<SourcesModel>? GetLibs(string lib) =>
            Libs.ContainsKey(lib) ? Libs[lib] : null;
#endif
    }
}
