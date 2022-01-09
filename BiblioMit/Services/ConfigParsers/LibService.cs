using BiblioMit.Extensions;
using BiblioMit.Models.VM;
using System.Diagnostics;
using System.Text.Json;

namespace BiblioMit.Services
{
    public static class Libman
    {
#if !DEBUG
        private const string _latestVersion = "1.0";
#endif
        private const string _fileName = "libman.json";
        private static Libs Libs { get; set; } = new Libs();
        public static Libs LoadJson()
        {
            using StreamReader r = new(_fileName);
            string json = r.ReadToEnd();
            Libs? libs = JsonSerializer.Deserialize<Libs>(json, JsonCase.Camel);
            if (libs == null) throw new FileLoadException($"Unable to read file {_fileName}");
            Libs = libs;
#if !DEBUG
            var os = Environment.OSVersion.Platform.ToString();
            if ("Win32NT" != os && Libs.Version == _latestVersion)
            {
                foreach (var lib in Libs.Libraries)
                {
                    //unpkg as default!!!
                    if (string.IsNullOrWhiteSpace(lib.Provider))
                    {
                        var prefix = $"https://unpkg.com/{lib.Library}/";
                        foreach (var file in lib.Files)
                        {
                            using var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "wget",
                                    Arguments = $"{prefix}/{file} -O {lib.Destination}/{file}",
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
            }
#endif
            return Libs;
        }

        public static LibManLibrary GetLibs(string lib)
        {
            var libs = Libs.Libraries.Single(m => 
            m.Library.Contains($"/{lib}@", StringComparison.Ordinal) 
            || m.Library.StartsWith($"{lib}@", StringComparison.Ordinal) || m.Library.StartsWith($"{lib}/", StringComparison.Ordinal));
            return libs;
        }
    }
}
