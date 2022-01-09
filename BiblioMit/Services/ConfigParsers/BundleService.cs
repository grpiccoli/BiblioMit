using BiblioMit.Extensions;
using BiblioMit.Models.VM;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BiblioMit.Services
{
    public static class Bundler
    {
        private static Collection<BundleConfig>? Bundles { get; set; }
        public static Collection<BundleConfig>? LoadJson()
        {
            using StreamReader r = new("bundleconfig.json");
            string json = r.ReadToEnd();
            Bundles = JsonSerializer.Deserialize<Collection<BundleConfig>>(json, JsonCase.Camel);
            return Bundles;
        }
        public static IEnumerable<BundleConfig> GetBundles(string lib)
        {
            if (Bundles == null) return Enumerable.Empty<BundleConfig>();
            return Bundles.Where(m => m.OutputFileName.Contains($"/{lib}.", StringComparison.InvariantCulture));
        }
    }
}
