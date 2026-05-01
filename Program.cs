using SonarQubeRulesExporter.Services;

if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Usage: SonarQubeRulesExporter <sonarqube-url>");
    return 1;
}

var url = args[0];
Console.WriteLine($"Fetching rules from {url}...");

var rules = await RestParser.GetRulesAsync(url);
Console.WriteLine($"Fetched {rules.Count} rules.");

var grouped = rules
    .OrderBy(r => r.Lang)
    .GroupBy(r => r.Lang)
    .ToDictionary(g => g.Key, g => g.ToList());

ExcelService.Export(grouped, "RULE_SET.xlsx");
Console.WriteLine("Generated File - RULE_SET.xlsx");
return 0;
