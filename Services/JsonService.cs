namespace SonarQubeRulesExporter.Services;

using System.Text.Json;
using SonarQubeRulesExporter.Models;

public static class JsonService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Export(Dictionary<string, List<Rule>> rulesMap, string fileName)
    {
        var json = JsonSerializer.Serialize(rulesMap, Options);
        File.WriteAllText(fileName, json);
    }
}
