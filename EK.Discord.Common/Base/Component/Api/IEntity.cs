namespace EK.Discord.Common.Base.Component.Persistence;

public interface IEntity {
    public object Id { get; }

}

public interface IEntity<out TKey> : IEntity
    where TKey : notnull {

    public new TKey Id { get; }
    object IEntity.Id => Id;

}

public interface IGuidEntity : IEntity<Guid> {
    
}