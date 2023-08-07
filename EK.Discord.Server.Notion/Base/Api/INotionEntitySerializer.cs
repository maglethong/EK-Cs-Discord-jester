using EK.Discord.Common.Base.Component.Persistence;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base.Api;

/// <summary>
///     Interface for Serializing <see cref="IEntity"/> from and to a <see cref="INotionClient"/> Table.
/// </summary>
public interface INotionEntitySerializer<TEntity>
    where TEntity : IEntity {

    /// <summary>
    ///     Serialize the <see cref="IEntity"/> into a format ready for transmitting to the <see cref="INotionClient"/>.
    /// </summary>
    /// <param name="entity"> The <see cref="IEntity"/> to serialize. </param>
    /// <returns> The serialized <see cref="IEntity"/> as mapped values of the column's property name vs the values of the property. </returns>
    public IDictionary<string, PropertyValue> Serialize(TEntity entity);

    /// <summary>
    ///     Deserialize the <see cref="IEntity"/> from a format received for from the <see cref="INotionClient"/>.
    /// </summary>
    /// <param name="pageId"> The Id of the <see cref="Page"/>. Usually from <see cref="Page"/>.<see cref="Page.Id"/>. </param>
    /// <param name="notionTableLine"> The serialized <see cref="IEntity"/> as it is retrieved from <see cref="INotionClient"/>. </param>
    /// <returns> <see cref="IEntity"/> filled with the values received from the <see cref="INotionClient"/>. </returns>
    public TEntity Deserialize(Guid pageId, IDictionary<string, PropertyValue> notionTableLine);

    /// <inheritdoc cref="Deserialize(Guid, IDictionary{string, PropertyValue})"/>
    /// <param name="page"> <see cref="Page"/> object received from requests to <see cref="INotionClient"/>. </param>
    public TEntity Deserialize(Page page) {
        return Deserialize(Guid.Parse(page.Id), page.Properties);
    }

}