using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EK.Discord.Common.Base.Component.Persistence;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

public abstract class AbstractNotionRepository<TEntity> : IRepository
    where TEntity : class, IEntity, new() {

    // TODO Verify db model here
//    private readonly Dictionary<string, Property>? _properties;
    protected Guid TableId { get; }
    protected INotionClient NotionClient { get; }

    protected IPageParentInput DbPage { get; }

    public AbstractNotionRepository(INotionClient notionClient) {
        NotionClient = notionClient;
        TableAttribute? tableAtt = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        // TODO ArgCheck
        this.TableId = Guid.Parse(tableAtt!.Name);

//        Database db = NotionClient.Databases
//                                   .RetrieveAsync(TableId.ToString())
//                                   .Result;
        DbPage = new DatabaseParentInput() { DatabaseId = TableId.ToString() };
    }


    protected IEnumerable<TEntity> RunQuery(DatabasesQueryParameters query) {
        return NotionClient
               .Databases
               .QueryAsync(TableId.ToString(), query)
               .Result
               .Results
               .Select(o => o.Deserialize<TEntity>())
               .ToList();
    }

    protected TEntity Create(TEntity newEntry) {
        return NotionClient.Pages
                    .CreateAsync(new PagesCreateParameters() {
                        Parent = DbPage,
                        Properties = newEntry.Serialize(),
                    })
                    .Result
                    .Deserialize<TEntity>();
    }

}