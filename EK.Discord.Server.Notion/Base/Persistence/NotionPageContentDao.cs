using System.Diagnostics.CodeAnalysis;
using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Horrible Syntax")]
public class NotionPageContentDao<TEntity> : IDataAccessObject<TEntity, Guid>
    where TEntity : class, IEntity<Guid>, new() {

    protected INotionClient NotionClient { get; }
    protected INotionPageContentSerializer<TEntity> Serializer { get; }

    public NotionPageContentDao(INotionClient notionClient, INotionPageContentSerializer<TEntity> serializer) {
        NotionClient = notionClient;
        Serializer = serializer;
    }

    public TEntity Create(TEntity obj) {
        Page page = NotionClient.Pages
                                .CreateAsync(new PagesCreateParameters() { Children = Serializer.Serialize(obj).ToList() })
                                .Result;

        return Read(Guid.Parse(page.Id));
    }

    public TEntity Read(Guid id) {
        IList<IBlock> existingBlocks =
            NotionClient.Blocks
                        .RetrieveChildrenAsync(id.ToString(),
                                               new BlocksRetrieveChildrenParameters() {
                                                   // TODO PaginatedList result has property HasNext (meaning we should fetch next result if this happens =P
                                                   PageSize = 5000
                                               }
                        )
                        .Result
                        .Results!;
        return Serializer.Deserialize(id, existingBlocks);
    }

    public IEnumerable<TEntity> ReadAll() {
        throw new NotSupportedException("Request would result in to long query. Operation Disabled.");
    }

    public TEntity Update(TEntity obj) {
        NotionClient.Blocks
                    .RetrieveChildrenAsync(obj.Id.ToString(),
                                           new BlocksRetrieveChildrenParameters() {
                                               // TODO PaginatedList result has property HasNext (meaning we should fetch next result if this happens =P
                                               PageSize = 5000
                                           }
                    )
                    .Result
                    .Results!
                    .Select(o => o.Id)
                    .Select(NotionClient.Blocks.DeleteAsync)
                    .Select(o => {
                            o.Wait();
                            return 1;
                        }
                    )
                    .ToArray();

        IList<IBlock> serialized = Serializer.Serialize(obj).ToList();
        List<IBlock>? created = NotionClient.Blocks
                                            .AppendChildrenAsync(obj.Id.ToString(), new() { Children = serialized })
                                            .Result
                                            .Results!;
        TEntity ret = Serializer.Deserialize(obj.Id, created);


        return ret;
    }

    public void Delete(Guid id) {
        NotionClient.Blocks
                    .DeleteAsync(id.ToString())
                    .Wait();
    }

}