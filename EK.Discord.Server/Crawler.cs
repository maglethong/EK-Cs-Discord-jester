﻿using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EK.Discord.Server.Notion.Base.Persistence;
using HtmlAgilityPack;
using Notion.Client;
using ReverseMarkdown;

namespace EK.Discord.Server;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Horrible Syntax")]
[SuppressMessage("ReSharper", "ReplaceWithSingleCallToSingleOrDefault")]
public class Crawler {

    private NotionRepository<SpellTo> Repo { get; }
    protected INotionClient NotionClient { get; }

    public Crawler(IServiceProvider services) {
        NotionClient = services.GetService<INotionClient>()!;
        Repo = new NotionRepository<SpellTo>(NotionClient);
    }

    public void Run() {
        IDictionary<string, SpellTo> existing = Repo.Request()
                                                    .ToDictionary(o => o.Name.Trim().ToLowerInvariant(), o=>o);
        var spells = GetAllSpells()
                     .Select(o => new {
                         oldVal = existing.TryGetValue(o.Name.Trim().ToLowerInvariant(), out SpellTo? value) ? value : null,
                         newVal = o,
                     })
                     .Select(o => Merge(o.oldVal, o.newVal))
                     .Where(o => o != null)
                     .Cast<SpellTo>()
                     .Select(o => Repo.CreateOrUpdate(o!))
                     // TODO Move this logic to CreateOrUpdate
                     // TODO Add Custom Attributes for Notion on Entity
                     // TODO Create attribute for page content as Markdown
                     .Select(o => {
                         var blocks = o.Description
                          .Split("\n")
                          .Where(k => !string.IsNullOrWhiteSpace(k))
                          .Select(k => new ParagraphBlock() {
                                      Paragraph = new() {
                                          RichText = k
                                                  .Split("\n")
                                                  // TODO Parse Markdown to Notion blocks (Need to convert and set properties)
                                                  .Select(p => new RichTextText() { Text = new Text() { Content = p } })
                                                  .Cast<RichTextBase>()
                                                  .ToList()
                                      }
                                  }
                          )
                          .ToList();
                         
                         var updated = NotionClient.Blocks
                                     .AppendChildrenAsync(o.Id.ToString(), new () {
                                         Children = blocks
                                     })
                                     .Result
                                     .Results;
                         
                         return o;
                     })
                     .Select(o => o)
                     .ToList();
    }

    private static SpellTo? Merge(SpellTo? oldVal, SpellTo newVal) {
        if (oldVal == null) {
            return newVal;
        }
        List<string> changed = typeof(SpellTo)
                               .GetProperties()
                               .Select(o => new {
                                       Attribute = o.GetCustomAttribute<ColumnAttribute>(),
                                       Property = o,
                                   }
                               )
                               .Where(o => o.Attribute != null)
                               .Select(o => new {
                                   o.Property,
                                   OldPropValue = o.Property.GetValue(oldVal),
                                   NewPropValue = o.Property.GetValue(newVal),
                               })
                               .Where(o => !AreEqual(o.OldPropValue, o.NewPropValue))
                               .Select(o => {
                                   o.Property.SetValue(oldVal, o.NewPropValue);
                                   return o.Property.Name;
                               })
                               .ToList();


        return changed.Any() ? oldVal : null;
    }
    
    private static bool AreEqual(object? a, object? b) {
        if (a == null && b == null) {
            return true;
        }
        if (a == null || b == null) {
            return false;
        }
        if (a.Equals(b)) {
            return true;
        }
        if (a.GetType().IsAssignableTo(typeof(IEnumerable)) && b.GetType().IsAssignableTo(typeof(IEnumerable))) {
            IEnumerable<object?> ae = ((IEnumerable) a).Cast<object?>().ToList();
            IEnumerable<object?> be = ((IEnumerable) b).Cast<object?>().ToList();
            return ae.All(o => be.Any(k => AreEqual(k, o))) && 
                   be.All(o => ae.Any(k => AreEqual(k, o)));
        }
        return false;
    }

    private const string baseUrl = "http://dnd5e.wikidot.com";

    private Converter MarkdownConverter { get;  } = new Converter();

    public IEnumerable<SpellTo> GetAllSpells() {
        using HttpClient http = new();
        string s = http.GetStringAsync($"{baseUrl}/spells").Result;
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(s);

        Random random = new Random();
        return doc.GetElementbyId("wiki-tab-0-9")
                  // Find table
                  .ParentNode
                  // List all table tab contents
                  .Elements("div")
//                  .AsParallel()
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
                  // Process Spell Link
                  .Select(o => ProcessSpellPage(o));
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
            spell.Level = "Cantrip";
        } else {
            spell.School = split[1];
            spell.Level = levelAndSchool.Substring(0, 3);
        }

        spell.School = spell.School.Substring(0, 1).ToUpperInvariant() + spell.School.Substring(1).ToLowerInvariant();

        spell.Source = pageContentElement
                       .Elements("p")
                       .First()
                       .InnerText
                       .Replace("Source: ", string.Empty);

        HtmlNode headerElement = pageContentElement
                                 .Elements("p")
                                 .Skip(2)
                                 .First();

        split = headerElement.InnerHtml
                             .Replace("</strong>", "")
                             .Replace("<br>", "")
                             .Replace("\n", " ")
                             .Split("<strong>")
                             .Skip(1)
                             .ToArray();
        spell.CastTime = split[0].Split(": ")[1].Trim();
        spell.Range = split[1].Split(": ")[1].Trim();
        spell.Components = split[2].Split(": ")[1]
                                   .Split(", ")
                                   .Select(o => o.Trim())
                                   .ToList();
        spell.Duration = split[3].Split(": ")[1].Replace("up to ", "").Trim();

        int i = spell.Components.FindIndex(o => o.StartsWith("M"));
        if (i >= 0) {
            spell.MaterialComponent = split[2]
                                      .Substring(split[2].IndexOf("(", StringComparison.InvariantCultureIgnoreCase))
                                      .Replace("(", "")
                                      .Replace(")", "");
            spell.Components[i] = "M";
            spell.Components.RemoveRange(i +1, spell.Components.Count -i -1);
        }

        if (spell.Duration.Contains("Concentration", StringComparison.InvariantCultureIgnoreCase)) {
            spell.Duration = spell.Duration
                                  .Replace("Concentration, ", "")
                                  .Replace("Concentration", "");
            spell.Components.Add("C");
        }

        if (levelAndSchool.Contains("Ritual", StringComparison.InvariantCultureIgnoreCase)) {
            spell.Components.Add("R");
        }
        
        if (spell.MaterialComponent.Contains("gp")) {
            spell.Components.Add("GP");
        }

        if (spell.CastTime.Contains("Raction", StringComparison.InvariantCultureIgnoreCase)) {
            split = spell.CastTime.Split(", ");
            spell.CastTime = "Raction";
            spell.ReactionTime = split[1];
        } else if (spell.CastTime.Contains("Bonus Action", StringComparison.InvariantCultureIgnoreCase)) {
            spell.CastTime = "Bonus Action";
        } else if (spell.CastTime.Contains("Action", StringComparison.InvariantCultureIgnoreCase)) {
            spell.CastTime = "Bonus Action";
        } else {
            spell.CastTime = spell.CastTime.ToLowerInvariant();
        }

        spell.SpellList = pageContentElement
                          .Elements("p")
                          .Last()
                          .InnerText
                          .Replace("Spell Lists. ", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                          .Split(", ")
                          .Select(o => o.Replace("(Optional)", "").Trim())
                          .ToList();

        spell.Description = MarkdownConverter.Convert(pageContentElement.InnerHtml);
        spell.HtmlDescription = pageContentElement.InnerHtml;
        spell.WikiLink = uri;
        return spell;
    }

}

// TODO use schema to select the notion client
//[Table("e1ba980b87eb4e87aec5da9d1e7f7195")] // Actual   
[Table("c147d58e5b5d4cbd85d6780f5884ce0d")] // Test
public class SpellTo : IGuidEntity {

    public override string ToString() {
        return $"({Level}) {Name} [" + string.Join(", ", Components) + "]";
    }

    [Key]
    public Guid Id { get; set; }

    [Column("Name", TypeName = nameof(PropertyValueType.Title))]
    public string Name { get; set; } = string.Empty;

    [Column("Level", TypeName = nameof(PropertyValueType.Select))]
    public string Level { get; set; } = string.Empty;

    [Column("Casting Time", TypeName = nameof(PropertyValueType.Select))]
    public string CastTime { get; set; } = string.Empty;

    [Column("Range/Area", TypeName = nameof(PropertyValueType.Select))]
    public string Range { get; set; } = string.Empty;

    [Column("Components", TypeName = nameof(PropertyValueType.MultiSelect))]
    public List<string> Components { get; set; } = new();

    [Column("School", TypeName = nameof(PropertyValueType.Select))]
    public string School { get; set; } = string.Empty;

    [Column("Duration", TypeName = nameof(PropertyValueType.Select))]
    public string Duration { get; set; } = string.Empty;

    [Column("Source", TypeName = nameof(PropertyValueType.RichText))]
    public string Source { get; set; } = string.Empty;

    [Column("Spell List", TypeName = nameof(PropertyValueType.MultiSelect))]
    public List<string> SpellList { get; set; } = new();

    [Column("Description", TypeName = nameof(PropertyValueType.RichText))]
    public string Description { get; set; } = string.Empty;

//    [Column("HtmlDescription", TypeName = nameof(PropertyValueType.RichText))]
    public string HtmlDescription { get; set; } = string.Empty;

    [Column("Wiki Link", TypeName = nameof(PropertyValueType.Url))]
    public string WikiLink { get; set; } = string.Empty;

    [Column("MaterialComponent", TypeName = nameof(PropertyValueType.RichText))]
    public string MaterialComponent { get; set; } = string.Empty;

//    [Column("ReactionTime", TypeName = nameof(PropertyValueType.RichText))]
    public string ReactionTime { get; set; } = string.Empty;

}