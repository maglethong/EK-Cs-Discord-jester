using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Api;

/// <summary>
///     Interface for Serializing from and to a <see cref="INotionClient"/> <see cref="Page"/>'s content.
/// <p/>
///     Values are serialized to and from the <see cref="INotionClient"/>'s representation in <see cref="IBlock"/>.
/// <p/>
///     An <see cref="IBlock"/> represents a single entry block in a notion page.
/// </summary>
public interface INotionPageContentSerializer<TEntity> {

    /// <summary>
    ///     Serialize the desired Type <see cref="TEntity"/> to a
    /// </summary>
    /// <param name="entity"> The source object to serialize </param>
    /// <returns> The serialized <see cref="TEntity"/> <see cref="INotionClient"/> <see cref="IBlock"/>. </returns>
    public IEnumerable<IBlock> Serialize(TEntity entity);

    /// <summary>
    ///     Deserialize the notion representation in <see cref="IBlock"/>'s to the desired representation in <see cref="TEntity"/>
    /// </summary>
    /// <param name="pageId"> The Id of the <see cref="Page"/>. Usually from <see cref="Page"/>.<see cref="Page.Id"/>. </param>
    /// <param name="raw"> the input as received from <see cref="INotionClient"/> </param>
    /// <returns> The deserialized result </returns>
    public TEntity Deserialize(Guid pageId, IEnumerable<IBlock> raw);

//    /// <inheritdoc cref="Deserialize(Guid, IEnumerable{IBlock})"/>
//    /// <param name="page"> <see cref="Page"/> object received from requests to <see cref="INotionClient"/>. </param>
//    public TEntity Deserialize(Page page) {
//        return Deserialize(Guid.Parse(page.Id), page.Properties);

}