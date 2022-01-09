using BiblioMit.Extensions;
using BiblioMit.Models.VM;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BiblioMit.Services
{
    public static class WebCompiler
    {
        private static Collection<Compile>? Compiles { get; set; }
        public static Collection<Compile>? LoadJson()
        {
            using StreamReader r = new("compilerconfig.json");
            string json = r.ReadToEnd();
            Compiles = JsonSerializer.Deserialize<Collection<Compile>>(json, JsonCase.Camel);
            return Compiles;
        }
        public static IEnumerable<Compile>? GetBundles(string lib)
        {
            return Compiles?.Where(m => m.OutputFile.Contains($"/{lib}.", StringComparison.InvariantCulture));
        }
    }
}
