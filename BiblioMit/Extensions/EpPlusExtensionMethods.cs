using OfficeOpenXml;

namespace BiblioMit.Extensions
{
    public static class EpPlusExtensionMethods
    {
        public static int? GetColumnByName(this ExcelWorksheet ws, string columnName) =>
            ws.Cells["1:1"].FirstOrDefault(c => c.Value != null && ((string)c.Value).Equals(columnName, StringComparison.OrdinalIgnoreCase))?.Start.Column;
        public static int? GetColumnByNames(this ExcelWorksheet ws, IEnumerable<string> columnNames) =>
            ws.Cells["1:1"]
                .FirstOrDefault(c => c.Value != null && columnNames
                .Contains(((string)c.Value).CleanCell()))?.Start.Column;
        public static int GetColumnByNames(this IList<string> headers, IEnumerable<string> columnNames)
        {
            foreach (string name in columnNames)
            {
                int index = headers.IndexOf(name);
                if (index != -1)
                {
                    return index;
                }
            }
            return -1;
        }
        public static int? GetRowByValue(this ExcelWorksheet ws, char col, string columnName) =>
            ws.Cells[$"{col}:{col}"].FirstOrDefault(c => c.Value != null && ((string)c.Value).Equals(columnName, StringComparison.OrdinalIgnoreCase))?.Start.Row;
    }
}
