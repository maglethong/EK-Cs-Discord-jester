namespace EK.Discord.Server.Notion.Base.Persistence;

public interface IEntity {

}

public interface IEntity<out TKey> : IEntity {
    public TKey Id { get; }
}

public interface IGuidEntity : IEntity<Guid> {
    
}