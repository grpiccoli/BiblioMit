using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace BiblioMit.Extensions
{
    public static class TableFilter
    {
        public static IQueryable<TSource> Pre<TSource>(this IEnumerable<TSource> contexto)
        {
            IQueryable<TSource> pre = contexto as IQueryable<TSource> ?? Enumerable.Empty<TSource>().AsQueryable();
            return pre;
        }
        public static PropertyDescriptor? FilterSort<TSource>(this IEnumerable<TSource> contexto, string srt = "Id")
        {
            if (contexto is null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            if (string.IsNullOrWhiteSpace(srt)) srt = "Id";
            PropertyDescriptor? sort = TypeDescriptor.GetProperties(typeof(TSource)).Find(srt, false);
            return sort;
        }
        public static ViewDataDictionary ViewData<TSource>(this IEnumerable<TSource> contexto,
            IQueryable<TSource> pre, int pg = 1, int rpp = 20, string srt = "Id",
            bool asc = true, string[]? val = null)
        {
            Dictionary<string, List<string>> Filters = new() { };

            if (val != null && val.Length > 0)
            {
                foreach (var filter in val)
                {
                    string[] method = filter.Split(':').Take(2).ToArray();
                    Filters[method[0]] = method[1].Split(',').ToList();
                }

                foreach (var filter in Filters)
                {
                    PropertyDescriptor? prop = TypeDescriptor.GetProperties(typeof(TSource)).Find(filter.Key, false);
                    if (prop != null)
                        pre = prop.PropertyType == typeof(DateTime) ?
                            pre.Where(x => filter.Value
                            .Contains(string.Format(CultureInfo.InvariantCulture, "{0:dd-MM-yyyy}", prop.GetValue(x)))) :
                            pre.Where(x =>
                                filter.Value
                                .Contains(prop.GetValue(x)));
                }
            }

            var ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            { };
            ViewData["last"] = (int)Math.Ceiling((double)((decimal)contexto.Count() / rpp));
            ViewData["srt"] = srt;
            ViewData["val"] = val;
            ViewData["Filters"] = Filters;
            ViewData["asc"] = asc;
            ViewData["pg"] = pg;
            ViewData["rpp"] = rpp;

            return ViewData;
        }

        public static IDictionary<string, List<string>> Filters<TSource>(this IEnumerable<TSource> contexto, IQueryable<TSource> pre, string? val = null)
        {
            if (contexto is null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            Dictionary<string, List<string>> Filters = new() { };

            if (!string.IsNullOrEmpty(val))
            {
                var filters = val.Split(';');
                foreach (var filter in filters)
                {
                    var method = filter.Split(':').Take(2).ToArray();
                    Filters[method[0]] = method[1].Split(',').ToList();
                }

                foreach (var filter in Filters)
                {
                    PropertyDescriptor? prop = TypeDescriptor.GetProperties(typeof(TSource)).Find(filter.Key, false);
                    if (prop != null)
                        pre = prop.PropertyType == typeof(DateTime) ?
                            pre.Where(x => filter.Value
                            .Contains(string.Format(CultureInfo.InvariantCulture, "{0:dd-MM-yyyy}", prop.GetValue(x)))) :
                            pre.Where(x => filter.Value
                            .Contains(prop.GetValue(x)));
                }
            }
            return Filters;
        }

        public static Collection<object> Variables(
            int? pg = 1, int? rpp = 20, string srt = "Id",
            bool? asc = true)
        {
            if (pg == null) pg = 1;
            if (rpp == null) rpp = 20;
            if (string.IsNullOrEmpty(srt)) srt = "Date";
            if (asc == null) asc = false;

            bool _asc = asc.Value;

            return new Collection<object> { pg, rpp, srt, _asc };
        }
    }
}
