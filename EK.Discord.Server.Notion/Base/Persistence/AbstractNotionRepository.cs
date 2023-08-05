using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EK.Discord.Common.Base.Component.Persistence;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

public class NotionRepository<TEntity> : IRepository
    where TEntity : class, IEntity<Guid>, new() {

    // TODO Verify db model here
//    private readonly Dictionary<string, Property>? _properties;
    protected Guid TableId { get; }
    protected INotionClient NotionClient { get; }

    protected IPageParentInput DbPage { get; }

    public NotionRepository(INotionClient notionClient) {
        NotionClient = notionClient;
        TableAttribute? tableAtt = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        // TODO ArgCheck
        this.TableId = Guid.Parse(tableAtt!.Name);

//        Database db = NotionClient.Databases
//                                   .RetrieveAsync(TableId.ToString())
//                                   .Result;
        DbPage = new DatabaseParentInput() { DatabaseId = TableId.ToString() };
    }


    public TEntity Create(TEntity newEntry) {
        return NotionClient.Pages
                           .CreateAsync(new PagesCreateParameters() {
                               Parent = DbPage,
                               Properties = newEntry.Serialize(),
                           })
                           .Result
                           .Deserialize<TEntity>();
    }

    public IEnumerable<TEntity> Request(DatabasesQueryParameters? query = null) {
        query ??= new();
        return NotionClient
               .Databases
               .QueryAsync(TableId.ToString(), query)
               .Result
               .Results
               .Select(o => o.Deserialize<TEntity>())
               .ToList();
    }

    public TEntity Request(Guid entityId) {
        return NotionClient.Pages
                           .RetrieveAsync(entityId.ToString())
                           .Result
                           .Deserialize<TEntity>();
    }

    public TEntity Update(TEntity value) {
        UpdatePageContent(value);
        return NotionClient.Pages
                           .UpdateAsync(value.Id.ToString(),
                                        new PagesUpdateParameters() { Properties = value.Serialize() }
                           )
                           .Result
                           .Deserialize<TEntity>();
    }

    public void UpdatePageContent(TEntity value) {
//        var content =
//            NotionClient.Blocks
//                        .RetrieveChildrenAsync(value.Id.ToString(), new ())
//                        .Result
//                        .Results;
//        int i = 0;
    }

    // TODO Test
    public void Delete(TEntity value) {
        NotionClient.Pages
                           .UpdateAsync(value.Id.ToString(),
                                        new PagesUpdateParameters() { Archived = true }
                           )
                           .Wait();
    }
    
    public TEntity CreateOrUpdate(TEntity value) {
        if (value.Id == Guid.Empty) {
            return Create(value);
        } else {
            return Update(value);
        }
    }
}