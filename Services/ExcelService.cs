namespace SonarQubeRulesExporter.Services;

using ClosedXML.Excel;
using SonarQubeRulesExporter.Models;

public static class ExcelService
{
    private static readonly XLColor HeaderBg = XLColor.FromArgb(32, 55, 100);
    private static readonly string[] Headers = ["#", "Language", "Rule Key", "Rule", "Issue Severity", "Issue Type", "Issue Category Tags/SysTags"];

    public static void Export(Dictionary<string, List<Rule>> rulesMap, string fileName)
    {
        using var workbook = new XLWorkbook();

        foreach (var (lang, rules) in rulesMap)
        {
            var sheet = workbook.Worksheets.Add(lang);
            WriteHeader(sheet);
            WriteData(sheet, rules);
            for (int col = 1; col <= Headers.Length; col++)
                sheet.Column(col).AdjustToContents();
        }

        workbook.SaveAs(fileName);
    }

    private static void WriteHeader(IXLWorksheet sheet)
    {
        for (int i = 0; i < Headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = Headers[i];
            cell.Style.Fill.BackgroundColor = HeaderBg;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 12;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
    }

    private static void WriteData(IXLWorksheet sheet, List<Rule> rules)
    {
        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            int row = i + 2;

            sheet.Cell(row, 1).Value = i + 1;
            sheet.Cell(row, 2).Value = rule.LangName;
            sheet.Cell(row, 3).Value = rule.Key;
            sheet.Cell(row, 4).Value = rule.Name;
            sheet.Cell(row, 5).Value = rule.Severity;
            sheet.Cell(row, 6).Value = rule.Type;
            sheet.Cell(row, 7).Value = string.Join(", ", rule.SysTags);

            for (int col = 1; col <= Headers.Length; col++)
            {
                var cell = sheet.Cell(row, col);
                cell.Style.Fill.BackgroundColor = XLColor.White;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.Black;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
        }
    }
}
