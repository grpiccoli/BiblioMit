using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BiblioMit.Extensions;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace BiblioMit.Services
{
    public class TableToExcelService : ITableToExcel
    {
        private readonly IStringLocalizer<ImportService> _localizer;
        public TableToExcelService(IStringLocalizer<ImportService> localizer)
        {
            _localizer = localizer;
            Matrix = new();
        }
        private int maxRow;
        private ExcelWorksheet? sheet;
        private Dictionary<(int, int), string> Matrix { get; set; }
        private int RowIndex;
        private int ColumnIndex;
        public async Task<ExcelPackage> ProcessAsync(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return await ProcessAsync(stream).ConfigureAwait(false);
        }
        public async Task<ExcelPackage> ProcessAsync(Stream html)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new();
            sheet = excel.Workbook.Worksheets.Add("sheet1");
            HtmlParser parser = new();
            IHtmlDocument document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);
            IEnumerable<IElement> elements = document.All.Where(e => (e.LocalName == "tr" && !e.InnerHtml.Contains("<tr", StringComparison.InvariantCultureIgnoreCase))
            || e.LocalName == "br");
            foreach (IElement e in elements)
            {
                ProcessRows(e);
            }

            return excel;
        }
        private void ProcessRows(IElement row)
        {
            if (sheet == null)
            {
                return;
            }

            int rowIndex = 1;
            int colIndex;
            if (maxRow > 0)
            {
                rowIndex = maxRow;
            }

            if (string.IsNullOrWhiteSpace(row.InnerHtml))
            {
                colIndex = 1;
                sheet.Cells[rowIndex, colIndex].Value = string.Empty;
                ++rowIndex;
                if (rowIndex > maxRow)
                {
                    maxRow = rowIndex;
                }

                return;
            }

            colIndex = 1;
            foreach (IElement td in row.QuerySelectorAll("td"))
            {
                sheet.Cells[rowIndex, colIndex].Value = Regex.Replace(td.TextContent, @"\r\n|\r|\n", "").Trim();
                ++colIndex;
            }
            ++rowIndex;
            if (rowIndex > maxRow)
            {
                maxRow = rowIndex;
            }
        }
        public async Task<Dictionary<(int, int), string>> HtmlTable2Matrix(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            return await HtmlTable2Matrix(stream).ConfigureAwait(false);
        }
        public async Task<Dictionary<(int, int), string>> HtmlTable2Matrix(Stream html)
        {
            Matrix = new();
            RowIndex = 1;
            HtmlParser parser = new();
            IHtmlDocument document = await parser.ParseDocumentAsync(html).ConfigureAwait(false);
            IEnumerable<IElement> elements = document.All.Where(e => (e.LocalName.Equals("tr", StringComparison.Ordinal)
            && !e.InnerHtml.Contains("<tr", StringComparison.Ordinal))
            || e.LocalName.Equals("br", StringComparison.Ordinal));
            if (!elements.Any())
            {
                throw new FormatException(_localizer["El archivo ingresado no contiene registros ni tablas de ningún tipo"]);
            }

            foreach (IElement e in elements)
            {
                ProcessRowsString(e);
            }

            return Matrix;
        }
        private void ProcessRowsString(IElement row)
        {
            ColumnIndex = 1;
            IHtmlCollection<IElement> tds = row.QuerySelectorAll("td");
            foreach (IElement td in tds)
            {
                string content = td.TextContent.CleanCell();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    Matrix.Add(
                        (ColumnIndex, RowIndex), content
                    );
                }

                ++ColumnIndex;
            }
            ++RowIndex;
        }
    }
}
