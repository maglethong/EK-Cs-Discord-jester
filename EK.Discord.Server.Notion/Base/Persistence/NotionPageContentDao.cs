using EK.Discord.Common.Base.Component.Persistence;
using EK.Discord.Server.Notion.Base.Api;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Persistence;

/// <remarks>
///     Implementation of <see cref="IDataAccessObject{TEntity, Guid}"/> for Accessing Data from <see cref="INotionClient"/> Pages.
/// </remarks>
public class NotionPageContentDao<TEntity> : IDataAccessObject<TEntity, Guid>
    where TEntity : class, IEntity<Guid>, new() {

    protected INotionClient NotionClient { get; }
    protected INotionPageContentSerializer<TEntity> Serializer { get; }

    /// <summary> Constructor </summary>
    public NotionPageContentDao(INotionClient notionClient, INotionPageContentSerializer<TEntity> serializer) {
        NotionClient = notionClient;
        Serializer = serializer;
    }

    /// <inheritdoc/>
    public TEntity Create(TEntity obj) {
        Page page = NotionClient.Pages
                                .CreateAsync(new PagesCreateParameters() { Children = Serializer.Serialize(obj).ToList() })
                                .Result;

        return Read(Guid.Parse(page.Id));
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public IEnumerable<TEntity> ReadAll() {
        throw new NotSupportedException("Request would result in to long query. Operation Disabled.");
    }

    /// <inheritdoc/>
    public TEntity Update(TEntity obj) {
        IList<IBlock> existing = NotionClient.Blocks
                                 .RetrieveChildrenAsync(obj.Id.ToString(),
                                                        new BlocksRetrieveChildrenParameters() {
                                                            // TODO PaginatedList result has property HasNext (meaning we should fetch next result if this happens =P
                                                            PageSize = 5000
                                                        }
                                 )
                                 .Result
                                 .Results!;

        IList<IBlock> serialized = Serializer.Serialize(obj).ToList();

        existing
            .Select(o => o.Id)
            .Select(NotionClient.Blocks.DeleteAsync)
            .Select(o => {
                    o.Wait();
                    return 1;
                }
            )
            // TODO make foreach extension
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            .ToArray();
        
        List<IBlock>? created = NotionClient.Blocks
                                            .AppendChildrenAsync(obj.Id.ToString(), new() { Children = serialized })
                                            .Result
                                            .Results!;
        TEntity ret = Serializer.Deserialize(obj.Id, created);

        return ret;
    }

    /// <inheritdoc/>
    public void Delete(Guid id) {
        NotionClient.Blocks
                    .DeleteAsync(id.ToString())
                    .Wait();
    }

}