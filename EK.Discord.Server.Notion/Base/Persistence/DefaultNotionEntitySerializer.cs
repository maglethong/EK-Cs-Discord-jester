using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

/// <inheritdoc cref="INotionEntitySerializer{TEntity}"/>
public class DefaultNotionEntitySerializer<TEntity> : INotionEntitySerializer<TEntity>
    // TODO make compatible with Entity with non-empty constructors
    where TEntity : IEntity, new() {

    /// <inheritdoc/>
    public IDictionary<string, PropertyValue> Serialize(TEntity entity) {
        return typeof(TEntity)
               .GetProperties()
               .Select(o => new {
                       Attribute = o.GetCustomAttribute<ColumnAttribute>(),
                       Property = o,
                   }
               )
               .Where(o => o.Attribute != null)
               .Select(o => new {
                       Attribute = o.Attribute!,
                       o.Property,
                       IsPageContent = o.Attribute!.GetType().IsAssignableTo(typeof(PageContentAttribute)),
                   }
               )
               .Where(o => !o.IsPageContent)
               .Select(o => new {
                       o.Property,
                       NotionColumnName = o.Attribute.Name!,
                       NotionColumnType = (o.Attribute.TypeName != null) ? Enum.Parse<PropertyValueType>(o.Attribute.TypeName!) : (PropertyValueType?) null,
                       NotionColumnValue = o.Property.GetValue(entity),
                   }
               )
               .Where(o => o.NotionColumnType.HasValue)
               .Select(o => new {
                   Name = o.NotionColumnName,
                   Value = Serialize(o.NotionColumnValue, o.NotionColumnType!.Value),
               })
               .ToDictionary(o => o.Name,
                             o=> o.Value);
    }

    /// <inheritdoc/>
    public TEntity Deserialize(Guid pageId, IDictionary<string, PropertyValue> notionTableLine) {
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
                    Attribute = o.Attribute!,
                    o.Property,
                    IsPageContent = o.Attribute!.GetType().IsAssignableTo(typeof(PageContentAttribute)),
                }
            )
            .Where(o => !o.IsPageContent)
            .Select(o => new {
                    o.Property,
                    NotionColumnName = o.Attribute.Name!,
                    NotionColumnType = (o.Attribute.TypeName != null) ? Enum.Parse<PropertyValueType>(o.Attribute.TypeName!) : (PropertyValueType?) null,
                    NotionColumnValue = notionTableLine[o.Attribute.Name!],
                }
            )
            .Where(o => o.NotionColumnType.HasValue)
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
                object? value = Deserialize(o.NotionColumnValue);
                o.Property.SetValue(ret, value);
                return 1;
            })
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            // TODO make foreach extension
            .ToList();

        typeof(TEntity)
            .GetProperties()
            .Select(o => new {
                    Attribute = o.GetCustomAttribute<KeyAttribute>(),
                    Property = o,
                }
            )
            .Where(o => o.Attribute != null)
            .Select(o => new {
                    o.Property,
                    TypesMatch = o.Property.PropertyType == typeof(Guid),
                }
            )
            .Where(o => o.TypesMatch)
            .Select(o => {
                    o.Property.SetValue(ret, pageId);
                    return 1;
                }
            )
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            .SingleOrDefault();
        
        return ret;
    }

    private static PropertyValue Serialize(object? value, PropertyValueType type) {
        return type switch {
            PropertyValueType.Title => new TitlePropertyValue() {
                Title = (value?.ToString() ?? string.Empty)
                        .Split("\n")
                        .Select(o => new RichTextText() { Text = new Text() { Content = o } })
                        .Cast<RichTextBase>()
                        .ToList()
            },
            PropertyValueType.RichText => new RichTextPropertyValue () {
                RichText = (value?.ToString() ?? string.Empty)
                           .Split("\n")
                           .Select(o => new RichTextText() { Text = new Text() { Content = o } })
                           .Cast<RichTextBase>()
                           .ToList()
            },
            PropertyValueType.Select => new SelectPropertyValue () {
                Select = new SelectOption() {
                    Name = value?.ToString() ?? string.Empty
                }
            },
            PropertyValueType.Url => new UrlPropertyValue() {
                Url = value?.ToString() ?? string.Empty
            },
            PropertyValueType.MultiSelect => new MultiSelectPropertyValue() {
                MultiSelect = (value as IEnumerable)?
                              .Cast<object?>()
                              .Select(o => new SelectOption() { Name = o?.ToString() ?? string.Empty })
                              .ToList() ?? new List<SelectOption>(),
            },
            _ => throw new NotImplementedException($"Not implemented for Type {type}")
        };
    }

    private static object? Deserialize(PropertyValue value) {
        return value.Type switch {
            PropertyValueType.Title => ((TitlePropertyValue) value).Title
                                                                   .Select(o=>o.PlainText)
                                                                   .Aggregate((a, b) => $"{a}\n{b}"),
            PropertyValueType.RichText => ((RichTextPropertyValue) value).RichText
                                                                         .Select(o=>o.PlainText)
                                                                         .Aggregate((a, b) => $"{a}\n{b}"),
            PropertyValueType.Select => ((SelectPropertyValue) value).Select?.Name,
            PropertyValueType.Url => ((UrlPropertyValue) value).Url,
            PropertyValueType.MultiSelect => ((MultiSelectPropertyValue) value).MultiSelect
                                                                               .Select(o => o.Name)
                                                                               .ToList(),
            _ => throw new NotImplementedException($"Not implemented for Type {value.Type}")
        };
    }
}