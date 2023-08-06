using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

/// <inheritdoc cref="INotionPageContentSerializer{TEntity}"/>
public class DefaultNotionPageContentSerializer<TEntity> : INotionPageContentSerializer<TEntity> 
    where TEntity : class, IEntity<Guid>, new() {

    private PropertyInfo GetProperty() {
        return typeof(TEntity)
               .GetProperties()
               .Select(o => new {
                       Attribute = o.GetCustomAttribute<FullPageMarkdownContentAttribute>(),
                       Property = o,
                   }
               )
               .Where(o => o.Attribute != null)
               .Select(o => o.Property)
               .Single();
    }

    private PropertyInfo GetKeyProperty() {
        return typeof(TEntity)
               .GetProperties()
               .Select(o => new {
                       Attribute = o.GetCustomAttribute<KeyAttribute>(),
                       Property = o,
                   }
               )
               .Where(o => o.Attribute != null)
               .Select(o => o.Property)
               .Single();
    }
    
    /// <inheritdoc/>
    public IEnumerable<IBlock> Serialize(TEntity entity) {
        string source = GetProperty().GetValue(entity)!.ToString()!;
        return source
               .Split("\n")
               .Where(o => !string.IsNullOrWhiteSpace(o))
               .Select(SerializeBlock);
    }

    /// <inheritdoc/>
    public TEntity Deserialize(Guid pageId, IEnumerable<IBlock> raw) {
        TEntity ret = new();
        GetKeyProperty().SetValue(ret, pageId);
        GetProperty().SetValue(ret, string.Join("\n", raw.Select(DeserializeBlock)));
        return ret;
    }

    private static string DeserializeBlock(IBlock value) {
        return value.Type switch {
            BlockType.Paragraph => ((ParagraphBlock) value).Paragraph
                                                           .RichText
                                                           .Select(o => o.PlainText)
                                                           .Aggregate((a, b) => $"{a}\n{b}"),
            _ => throw new NotImplementedException($"Not implemented for Type {value.Type}")
        };
    }

    private IBlock SerializeBlock(string s) {
        return new ParagraphBlock() {
            Paragraph = new() {
                RichText = s
                           .Split("\n")
                           // TODO Parse Markdown to Notion blocks (Need to convert and set properties)
                           .Select(p => new RichTextText() { Text = new Text() { Content = p } })
                           .Cast<RichTextBase>()
                           .ToList()
            }
        };
    }
}