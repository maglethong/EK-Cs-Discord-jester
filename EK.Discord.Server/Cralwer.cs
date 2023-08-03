using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using EK.Discord.Server.Notion.Base.Persistence;
using HtmlAgilityPack;
using Notion.Client;

namespace EK.Discord.Server;

public class Cralwer {

    private readonly IServiceProvider _services;

    public Cralwer(IServiceProvider services) {
        this._services = services;
    }

    public void Run() {
        var client = _services.GetService<INotionClient>()!;
//        new TestNotionRepo(client).Create(new TestTo() {
//            Skill = "TEST"
//        });
        var all = new TestNotionRepo(client).GetAll();

        int i = 0;
    }

    private const string baseUrl = "http://dnd5e.wikidot.com";

    public void GetAllSpells() {
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
            spell.Level = split[1];
        } else {
            spell.School = split[1];
            spell.Level = levelAndSchool.Substring(0, 3);
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
        spell.Components = split[2].Split(": ")[1].Split(", ").ToList();
        spell.Duration = split[3].Split(": ")[1];

        spell.SpellList = pageContentElement
                          .Elements("p")
                          .Last()
                          .InnerText
                          .Replace("Spell Lists. ", "")
                          .Split(", ")
                          .ToList();

        spell.Description = pageContentElement.InnerText;

        return spell;
    }

}

public class TestNotionRepo : AbstractNotionRepository<SpellTo> {

    public TestNotionRepo(INotionClient notionClient) : base(notionClient) {
    }

    public IReadOnlyList<SpellTo> GetAll() => RunQuery(new DatabasesQueryParameters()).ToList();
    public void Create(SpellTo newValue) => base.Create(newValue);

}

// TODO use schema to select the notion client
[Table("e1ba980b87eb4e87aec5da9d1e7f7195")]
public class SpellTo : IEntity {

    public override string ToString() {
        return $"({Level}) {Name}";
    }

    [Column("Name", TypeName = nameof(PropertyValueType.Title))]
    public string Name { get; set; } = "";

    [Column("Level", TypeName = nameof(PropertyValueType.Select))]
    public string Level { get; set; } = "";

    [Column("Casting Time", TypeName = nameof(PropertyValueType.Select))]
    public string CastTime { get; set; } = "";

    [Column("Range/Area", TypeName = nameof(PropertyValueType.Select))]
    public string Range { get; set; } = "";

    [Column("Components", TypeName = nameof(PropertyValueType.MultiSelect))]
    public List<string> Components { get; set; } = new();

    [Column("School", TypeName = nameof(PropertyValueType.Select))]
    public string School { get; set; } = "";

    [Column("Duration", TypeName = nameof(PropertyValueType.Select))]
    public string Duration { get; set; } = "";

    // TODO
    public string Source { get; set; } = "";

    [Column("Spell List", TypeName = nameof(PropertyValueType.MultiSelect))]
    public List<string> SpellList { get; set; } = new();
    
    // Page
    public string Description { get; set; } = "";

}