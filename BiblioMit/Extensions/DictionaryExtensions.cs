namespace BiblioMit.Extensions
{
    public static class DictionaryExtensions
    {
        public static (int, int) GetKeyFromValueContains(this Dictionary<(int, int), string> dic, string q) =>
            dic.FirstOrDefault(y => y.Value.Contains(q, StringComparison.Ordinal)).Key;
        public static (int, int) GetKeyFromHeader(this Dictionary<(int, int), string> dic, string q) =>
            GetFromHeader(dic, q).Key;
        public static string? GetValueFromHeaderHorizontal(
            this Dictionary<(int, int), string> dic, string q, int columns = 1) =>
            dic.GetValueFromHeader(q, columns);
        public static string? GetValueFromHeader(
            this Dictionary<(int, int), string> dic, string q, int columns = 0, int rows = 0)
        {
            (int, int) cell = GetKeyFromHeader(dic, q);
            (int, int) key = (cell.Item1 + columns, cell.Item2 + rows);
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }

            return null;
        }
        public static KeyValuePair<(int, int), string> GetFromHeader(
            this Dictionary<(int, int), string> dic, string q) =>
            dic.FirstOrDefault(y => y.Value.Equals(q, StringComparison.Ordinal));
        public static (int, int) SearchHeaders(this Dictionary<(int, int), string> dic, IEnumerable<string> headers)
        {
            if (headers != null)
            {
                foreach (var reg in headers)
                {
                    (int, int) np = dic.GetFromHeader(reg).Key;
                    if (np != (0, 0))
                    {
                        return np;
                    }
                }
            }

            return (0, 0);
        }
    }
}
