namespace SonarQubeRulesExporter.Services;

using System.Text.Json;
using SonarQubeRulesExporter.Models;

public static class RestParser
{
    private const int PageSize = 500;
    private const string RuleFields = "isTemplate,name,lang,langName,severity,status,sysTags,tags,templateKey";
    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static Task<List<Rule>> GetRulesAsync(string host)
        => FetchAllRulesAsync(NormalizeHost(host), extraQuery: null);

    public static async Task<List<QualityProfile>> GetQualityProfilesAsync(string host, IEnumerable<string> names)
    {
        host = NormalizeHost(host);
        var url = $"{host}/api/qualityprofiles/search";
        var json = await Http.GetStringAsync(url);
        var resp = JsonSerializer.Deserialize<QualityProfilesResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Empty response from SonarQube API.");

        var wanted = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        return resp.Profiles.Where(p => wanted.Contains(p.Name)).ToList();
    }

    public static Task<List<Rule>> GetRulesForProfileAsync(string host, string profileKey)
        => FetchAllRulesAsync(NormalizeHost(host), extraQuery: $"&activation=true&qprofile={Uri.EscapeDataString(profileKey)}");

    private static async Task<List<Rule>> FetchAllRulesAsync(string host, string? extraQuery)
    {
        var first = await FetchPageAsync(host, 1, extraQuery);
        var rules = new List<Rule>(first.Rules);

        if (first.Total > PageSize)
        {
            int maxPage = first.Total / PageSize + 2;
            for (int page = 2; page < maxPage; page++)
            {
                var resp = await FetchPageAsync(host, page, extraQuery);
                rules.AddRange(resp.Rules);
            }
        }

        return rules;
    }

    private static async Task<RulesResponse> FetchPageAsync(string host, int page, string? extraQuery)
    {
        var url = $"{host}/api/rules/search?f={RuleFields}&ps={PageSize}&s=name&p={page}{extraQuery}";
        var json = await Http.GetStringAsync(url);
        return JsonSerializer.Deserialize<RulesResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Empty response from SonarQube API.");
    }

    private static string NormalizeHost(string host) => host.TrimEnd('/');
}
