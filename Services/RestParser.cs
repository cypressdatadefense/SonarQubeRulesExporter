namespace SonarQubeRulesExporter.Services;

using System.Text.Json;
using SonarQubeRulesExporter.Models;

public static class RestParser
{
    private const int PageSize = 500;
    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<List<Rule>> GetRulesAsync(string host)
    {
        host = host.TrimEnd('/');
        var first = await FetchPageAsync(host, 1);
        var rules = new List<Rule>(first.Rules);

        if (first.Total > PageSize)
        {
            int maxPage = first.Total / PageSize + 2;
            for (int page = 2; page < maxPage; page++)
            {
                var resp = await FetchPageAsync(host, page);
                rules.AddRange(resp.Rules);
            }
        }

        return rules;
    }

    private static async Task<RulesResponse> FetchPageAsync(string host, int page)
    {
        var url = $"{host}/api/rules/search?f=isTemplate,name,lang,langName,severity,status,sysTags,tags,templateKey&ps={PageSize}&s=name&p={page}";
        var json = await Http.GetStringAsync(url);
        return JsonSerializer.Deserialize<RulesResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Empty response from SonarQube API.");
    }
}
