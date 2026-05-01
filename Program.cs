using SonarQubeRulesExporter.Models;
using SonarQubeRulesExporter.Services;

var asJson = args.Contains("--json");
var positional = args.Where(a => a != "--json").ToArray();

if (positional.Length == 0 || string.IsNullOrWhiteSpace(positional[0]))
{
    Console.Error.WriteLine("Usage: SonarQubeRulesExporter <sonarqube-url> [profile-name[,profile-name...]] [--json]");
    return 1;
}

var url = positional[0];
var profileNames = positional.Length > 1 && !string.IsNullOrWhiteSpace(positional[1])
    ? positional[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : [];

List<Rule> rules;
string baseName;

if (profileNames.Length == 0)
{
    Console.WriteLine($"Fetching rules from {url}...");
    rules = await RestParser.GetRulesAsync(url);
    baseName = "RULE_SET";
}
else
{
    Console.WriteLine($"Fetching quality profiles from {url}...");
    var profiles = await RestParser.GetQualityProfilesAsync(url, profileNames);

    var matchedNames = new HashSet<string>(profiles.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
    foreach (var requested in profileNames.Where(n => !matchedNames.Contains(n)))
        Console.Error.WriteLine($"Warning: no quality profile found with name '{requested}'.");

    if (profiles.Count == 0)
    {
        Console.Error.WriteLine("No matching quality profiles found.");
        return 1;
    }

    var byKey = new Dictionary<string, Rule>();
    foreach (var profile in profiles)
    {
        Console.WriteLine($"Fetching rules for profile '{profile.Name}' ({profile.LanguageName})...");
        var profileRules = await RestParser.GetRulesForProfileAsync(url, profile.Key);
        foreach (var rule in profileRules)
            byKey[rule.Key] = rule;
    }

    rules = byKey.Values.ToList();
    baseName = profileNames.Length == 1
        ? $"RULE_SET_{Sanitize(profileNames[0])}"
        : "RULE_SET_PROFILES";
}

Console.WriteLine($"Fetched {rules.Count} rules.");

var grouped = rules
    .OrderBy(r => r.Lang)
    .GroupBy(r => r.Lang)
    .ToDictionary(g => g.Key, g => g.ToList());

var fileName = asJson ? $"{baseName}.json" : $"{baseName}.xlsx";

if (asJson)
    JsonService.Export(grouped, fileName);
else
    ExcelService.Export(grouped, fileName);

Console.WriteLine($"Generated File - {fileName}");
return 0;

static string Sanitize(string name)
{
    var invalid = Path.GetInvalidFileNameChars();
    return new string(name.Select(c => invalid.Contains(c) || c == ' ' ? '_' : c).ToArray());
}
