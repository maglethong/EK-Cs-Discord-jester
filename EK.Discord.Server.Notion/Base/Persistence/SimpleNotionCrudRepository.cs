using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

public class SimpleNotionCrudRepository<TEntity> : IDataAccessObject<TEntity, Guid>
    where TEntity : class, IEntity<Guid>, new() {

    // TODO Verify db model here
//    private readonly Dictionary<string, Property>? _properties;
    protected Guid TableId { get; }
    protected INotionClient NotionClient { get; }
    protected INotionEntitySerializer<TEntity> Serializer { get; }
    protected IPageParentInput DbPage { get; }

    public SimpleNotionCrudRepository(INotionClient notionClient, INotionEntitySerializer<TEntity> serializer) {
        NotionClient = notionClient;
        Serializer = serializer;
        TableAttribute? tableAtt = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        // TODO ArgCheck
        this.TableId = Guid.Parse(tableAtt!.Name);

//        Database db = NotionClient.Databases
//                                   .RetrieveAsync(TableId.ToString())
//                                   .Result;
        DbPage = new DatabaseParentInput() { DatabaseId = TableId.ToString() };
    }


    public TEntity Create(TEntity newEntry) {
        Page page = NotionClient.Pages
                                .CreateAsync(new PagesCreateParameters() {
                                        Parent = DbPage,
                                        Properties = Serializer.Serialize(newEntry),
                                    }
                                )
                                .Result;

        return Serializer.Deserialize(page);
    }

    public TEntity Read(Guid entityId) {
        Page page = NotionClient.Pages
                                .RetrieveAsync(entityId.ToString())
                                .Result;
        return Serializer.Deserialize(page);
    }

    public IEnumerable<TEntity> ReadAll() {
        return NotionClient
               .Databases
               .QueryAsync(TableId.ToString(), new())
               .Result
               .Results
               .Select(o => Serializer.Deserialize(o))
               .ToList();
    }

    public TEntity Update(TEntity value) {
        UpdatePageContent(value);
        Page page = NotionClient.Pages
                                .UpdateAsync(value.Id.ToString(),
                                             new PagesUpdateParameters() { Properties = Serializer.Serialize(value) }
                                )
                                .Result;
        return Serializer.Deserialize(page);
    }

    public void UpdatePageContent(TEntity value) {
//        var content =
//            NotionClient.Blocks
//                        .RetrieveChildrenAsync(value.Id.ToString(), new ())
//                        .Result
//                        .Results;
//        int i = 0;
    }

    public TEntity RequestPageContent(TEntity value) {
        Page page = NotionClient.Pages
                                .RetrieveAsync(value.Id.ToString())
                                .Result;
        return Serializer.Deserialize(page);
    }

    // TODO Test
    public void Delete(Guid id) {
        NotionClient.Pages
                    .UpdateAsync(id.ToString(),
                                 new PagesUpdateParameters() { Archived = true }
                    )
                    .Wait();
    }

}