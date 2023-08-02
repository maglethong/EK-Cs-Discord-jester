using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

public static class NotionTableEntryDeserializerExtension {

    public static IDictionary<string, PropertyValue> Serialize<TEntity>(this TEntity entity) 
    where TEntity : IEntity {
        
        return typeof(TEntity)
            .GetProperties()
            .Select(o => new {
                    Attribute = o.GetCustomAttribute<ColumnAttribute>(),
                    Property = o,
                }
            )
            .Where(o => o.Attribute != null)
            .Select(o => new {
                    o.Property,
                    NotionColumnName = o.Attribute!.Name!,
                    NotionColumnType = Enum.Parse<PropertyValueType>(o.Attribute!.TypeName!),
                    NotionColumnValue = o.Property.GetValue(entity),
                }
            )
            .Select(o => new {
                Name = o.NotionColumnName,
                Value = o.NotionColumnValue.Serialize(o.NotionColumnType),
            })
            .ToDictionary(o => o.Name,
                          o=> o.Value);
    }

    private static PropertyValue Serialize(this object? value, PropertyValueType type) {
        return type switch {
            PropertyValueType.Title => new TitlePropertyValue() {
                Title = new [] {
                    new RichTextText() {
                        Text = new Text(){ Content = value?.ToString() ?? string.Empty }
                    } as RichTextBase
                }.ToList()
            },
            _ => throw new NotImplementedException($"Not implemented for Type {type}")
        };
    }

    private static object? Deserialize(this PropertyValue value) {
        return value.Type switch {
            PropertyValueType.Title => ((TitlePropertyValue) value).Title[0].PlainText,
            _ => throw new NotImplementedException($"Not implemented for Type {value.Type}")
        };
    }
    
    public static TEntity Deserialize<TEntity>(this Page notionTableLine)
        // TODO make compatible with Entity with non-empty constructors
        where TEntity : IEntity, new() {
        TEntity ret = new TEntity();

        typeof(TEntity)
            .GetProperties()
            .Select(o => new {
                    Attribute = o.GetCustomAttribute<ColumnAttribute>(),
                    Property = o,
                }
            )
            .Where(o => o.Attribute != null)
            .Select(o => new {
                    o.Property,
                    NotionColumnName = o.Attribute!.Name!,
                    NotionColumnType = Enum.Parse<PropertyValueType>(o.Attribute!.TypeName!),
                    NotionColumnValue = notionTableLine.Properties[o.Attribute.Name!],
                    // TODO store TitlePropertyValue in case it is a page and we want to read it too.
                }
            )
            .Select(o => new {
                    o.Property,
                    o.NotionColumnName,
                    o.NotionColumnType,
                    o.NotionColumnValue,
                    TypesMatch = o.NotionColumnValue.Type == o.NotionColumnType
                }
            )
            .Where(o => o.TypesMatch)
            .Select(o => {
                object? value = o.NotionColumnValue.Deserialize();
                o.Property.SetValue(ret, value);
                return 1;
            })
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // TODO make foreach extension
            .ToList();

        return ret;
    }
}