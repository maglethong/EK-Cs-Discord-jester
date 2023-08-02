using System.Diagnostics.CodeAnalysis;
using HtmlAgilityPack;

namespace EK.Discord.Server;

public class Cralwer {

    private readonly IServiceProvider _services;

    public Cralwer(IServiceProvider services) {
        this._services = services;
    }

    private const string baseUrl = "http://dnd5e.wikidot.com";

    public void Run() {
        using HttpClient http = new();
        string s = http.GetStringAsync($"{baseUrl}/spells").Result;
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(s);

        Random random = new Random();
        List<SpellTo> spells =
            doc.GetElementbyId("wiki-tab-0-9")
               // Find table
               .ParentNode
               // List all table tab contents
               .Elements("div")
               .AsParallel()
               .Select(o => o.Element("div"))
               .Select(o => o.Element("table"))
               // Flatten Table Body
               .SelectMany(o => o.Elements("tr"))
               // Ignore Table header
               .Where(o => o.Element("th") == null)
               // Extract link
               .SelectMany(o => o.Elements("td"))
               .Select(o => o.Element("a"))
               .Where(o => o != null)
               .Select(o => o.Attributes.FirstOrDefault())
               .Where(o => o != null)
               .Select(o => o!.Value)
               .Select(o => $"{baseUrl}{o}")
               .OrderBy(order => random.Next())
               // Process Spell Link
               .Select(o => ProcessSpellPage(o))
               .Take(10)
               .ToList();

        int i = 0;
    }


    [SuppressMessage("ReSharper", "ReplaceWithSingleCallToSingle")]
    private SpellTo ProcessSpellPage(string uri) {
        using HttpClient http = new();
        string s = http.GetStringAsync(uri).Result;
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(s);

        SpellTo spell = new();

        spell.Name = doc.DocumentNode
                        .Descendants("div")
                        .Where(o => o.Attributes["class"] != null)
                        .Where(o => o.Attributes["class"].Value.Contains("page-title"))
                        .Single()
                        .InnerText;

        HtmlNode pageContentElement = doc.GetElementbyId("page-content")!;

        string levelAndSchool = pageContentElement
                                .Elements("p")
                                .Skip(1)
                                .First()
                                .InnerText;
        string[] split = levelAndSchool.Split(" ");
        if (levelAndSchool.Contains("Cantrip", StringComparison.CurrentCultureIgnoreCase)) {
            spell.School = split[0];
            spell.Level = 0;
        } else {
            spell.School = split[1];
            spell.Level = int.Parse(levelAndSchool.Substring(0, 1));
        }

        spell.Source = pageContentElement
                       .Elements("p")
                       .First()
                       .InnerText
                       .Replace("Source: ", "");

        HtmlNode headerElement = pageContentElement
                                 .Elements("p")
                                 .Skip(2)
                                 .First();

        split = headerElement.InnerText.Split("\n"); 
        spell.CastTime = split[0].Split(": ")[1];
        spell.Range = split[1].Split(": ")[1];
        spell.Components = split[2].Split(": ")[1];
        spell.Duration = split[3].Split(": ")[1];

        spell.SpellList = pageContentElement
                          .Elements("p")
                          .Last()
                          .InnerText
                          .Replace("Spell Lists. ", "")
                          .Split(", ");

        spell.Description = pageContentElement.InnerText;

        return spell;
    }

}

public class SpellTo {

    public override string ToString() {
        return $"({Level}) {Name}";
    }

    public int Level { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Source { get; set; } = "";
    public string School { get; set; } = "";
    public string[] SpellList { get; set; } = Array.Empty<string>();
    public string Description { get; set; } = "";

    public string CastTime { get; set; } = "";
    public string Range { get; set; } = "";
    public string Components { get; set; } = "";
    public string Duration { get; set; } = "";

}