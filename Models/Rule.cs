namespace SonarQubeRulesExporter.Models;

public class Rule
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Severity { get; set; } = "";
    public bool IsTemplate { get; set; }
    public string[] Tags { get; set; } = [];
    public string[] SysTags { get; set; } = [];
    public string Lang { get; set; } = "";
    public string LangName { get; set; } = "";
    public string Type { get; set; } = "";
}

public class RulesResponse
{
    public int Total { get; set; }
    public List<Rule> Rules { get; set; } = [];
}

public class QualityProfile
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Language { get; set; } = "";
    public string LanguageName { get; set; } = "";
}

public class QualityProfilesResponse
{
    public List<QualityProfile> Profiles { get; set; } = [];
}
