using System.Data;
using System.Reflection;

namespace BiblioMit.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable SetColumnsOrder(this DataTable table, params string[] columnNames)
        {
            int columnIndex = 0;
            foreach (var columnName in columnNames)
            {
                table.Columns[columnName]?.SetOrdinal(columnIndex);
                columnIndex++;
            }
            return table;
        }
        public static DataTable SetColumnsNames(this DataTable table, params string[] columnNames)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].ColumnName = columnNames[i];
            }

            return table;
        }
        public static Type? GetEnumType(string enumName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = assembly.GetType(enumName);
                if (type == null)
                {
                    continue;
                }
                else if (type.IsEnum)
                {
                    return type;
                }
            }
            return null;
        }

    }
}
