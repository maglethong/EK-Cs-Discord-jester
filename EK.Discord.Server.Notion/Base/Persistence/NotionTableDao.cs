using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EK.Discord.Common.Base.Component.Api;
using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

/// <remarks>
///     Implementation of <see cref="IDataAccessObject{TEntity, Guid}"/> for Accessing Data from <see cref="INotionClient"/> Tables.
/// </remarks>
public class NotionTableDao<TEntity> : IDataAccessObject<TEntity, Guid>
    where TEntity : class, IEntity<Guid>, new() {

    // TODO Verify db model here
//    private readonly Dictionary<string, Property>? _properties;
    protected Guid TableId { get; }
    protected INotionClient NotionClient { get; }
    protected INotionEntitySerializer<TEntity> Serializer { get; }
    protected IPageParentInput DbPage { get; }
    protected PropertyInfo? PageContentProperty { get; }

    /// <summary> Constructor </summary>
    public NotionTableDao(INotionClient notionClient, INotionEntitySerializer<TEntity> serializer) {
        NotionClient = notionClient;
        Serializer = serializer;
        TableAttribute? tableAtt = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        PageContentProperty = typeof(TEntity)
                              .GetProperties()
                              .Select(o => new {
                                      Property = o,
                                      Attribute = o.GetCustomAttribute<PageContentAttribute>(),
                                  }
                              )
                              .Where(o => o.Attribute != null)
                              .Select(o => o.Property)
                              .SingleOrDefault();
        // TODO ArgCheck
        this.TableId = Guid.Parse(tableAtt!.Name);

//        Database db = NotionClient.Databases
//                                   .RetrieveAsync(TableId.ToString())
//                                   .Result;
        DbPage = new DatabaseParentInput() { DatabaseId = TableId.ToString() };
    }

    /// <inheritdoc/>
    public TEntity Create(TEntity newEntry) {
        IDictionary<string, PropertyValue> serialized = Serializer.Serialize(newEntry);
        Page page = NotionClient.Pages
                                .CreateAsync(new PagesCreateParameters() {
                                        Parent = DbPage,
                                        Properties = serialized,
                                    }
                                )
                                .Result;

        return Serializer.Deserialize(page);
    }

    /// <inheritdoc/>
    public TEntity Read(Guid entityId) {
        Page page = NotionClient.Pages
                                .RetrieveAsync(entityId.ToString())
                                .Result;
        return Serializer.Deserialize(page);
    }

    /// <inheritdoc/>
    public IEnumerable<TEntity> ReadAll() {
        return NotionClient
               .Databases
               .QueryAsync(TableId.ToString(), new() {
                   // TODO PaginatedList result has property HasNext (meaning we should fetch next result if this happens =P
                   PageSize = 5000
               })
               .Result
               .Results
               .Select(o => Serializer.Deserialize(o))
               .ToList();
    }

    /// <inheritdoc/>
    public TEntity Update(TEntity value) {
        Page page = NotionClient.Pages
                                .UpdateAsync(value.Id.ToString(),
                                             new PagesUpdateParameters() { Properties = Serializer.Serialize(value) }
                                )
                                .Result;
        return Serializer.Deserialize(page);
    }

    // TODO Test
    /// <inheritdoc/>
    public void Delete(Guid id) {
        NotionClient.Pages
                    .UpdateAsync(id.ToString(),
                                 new PagesUpdateParameters() { Archived = true }
                    )
                    .Wait();
    }

}