using EK.Discord.Common.Base.Component.Common;

namespace EK.Discord.Common.Base.Component.Persistence;

/// <summary>
///     Interface for defining a DataAccessObject for a known <see cref="IEntity"/> type.
/// </summary>
/// <inheritdoc cref="IRepository"/>
/// <typeparam name="TEntity"> The concrete Type of the <see cref="IEntity"/>. </typeparam>
/// <typeparam name="TKey"> The primary key used for identifying the <see cref="IEntity"/>. </typeparam>
public interface IDataAccessObject<TEntity, in TKey> : IComponentPart
    where TEntity : class, IEntity<TKey>
    where TKey: struct {
    public TEntity Create(TEntity obj);
    public TEntity Read(TKey id);
    public IEnumerable<TEntity> ReadAll();
    public TEntity Update(TEntity obj);
    public void Delete(TKey id);
}