using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Azure;
using Azure.Data.Tables;
using EK.Discord.Common.Base.Component.Api;
using EK.Discord.Common.Base.Component.Common;
using EK.Discord.Common.Base.Component.Persistence;

namespace EK.Discord.Server.Azure.Persistence;

/// <remarks>
///     Implementation of <see cref="IDataAccessObject{TEntity, Guid}"/> for Accessing Data from <see cref="TableServiceClient"/> Tables.
/// </remarks>
public class SinglePartitionAzureTableStorageDao<TEntity> : AbstractComponentPartBase, IDataAccessObject<TEntity, Guid>
    where TEntity : class, IEntity<Guid>, new() {

    private TableClient Client { get; }
    public string TableName { get; }
    private const string CONST_PARTITION_KEY = "";


    public SinglePartitionAzureTableStorageDao(IServiceProvider serviceProvider, TableServiceClient client) : base(serviceProvider) {
        TableName = typeof(TEntity)
                    .GetCustomAttribute<TableAttribute>()
                    ?
                    .Name
                    ?? typeof(TEntity).Name;
        Client = client.GetTableClient(TableName);
    }

    /// <inheritdoc cref="IDataAccessObject{TEntity, Guid}.Create"/>
    public TEntity Create(TEntity obj) {
        Client.AddEntity(Convert(obj));
        return Read(obj.Id);
    }

    /// <inheritdoc cref="IDataAccessObject{TEntity, Guid}.Read"/>
    public TEntity Read(Guid id) {
        Response<TableEntity>? response = Client.GetEntity<TableEntity>(CONST_PARTITION_KEY, id.ToString());
        return Convert(response.Value) ?? throw new Exception("TBD"); // TODO
    }

    /// <inheritdoc cref="IDataAccessObject{TEntity, Guid}.ReadAll"/>
    public IEnumerable<TEntity> ReadAll() {
        return Client
               .Query<TableEntity>()
               .AsPages()
               .SelectMany(o => o.Values)
               .Select(Convert)
               .Cast<TEntity>();
    }

    /// <inheritdoc cref="IDataAccessObject{TEntity, Guid}.Update"/>
    public TEntity Update(TEntity obj) {
        Client.UpdateEntity(Convert(obj), ETag.All, TableUpdateMode.Replace);
        return Read(obj.Id);
    }

    /// <inheritdoc cref="IDataAccessObject{TEntity, Guid}.Delete"/>
    public void Delete(Guid id) {
        Client.DeleteEntity(CONST_PARTITION_KEY, id.ToString(), ETag.All);
    }

    protected virtual ITableEntity? Convert(TEntity? obj) {
        if (obj == null) {
            return null;
        }
        TableEntity ret = new(CONST_PARTITION_KEY, obj.Id.ToString());
        ret.Add("Key", (object) "Value"); // TODO => Add all properties
        return ret;
    }

    protected virtual TEntity? Convert(ITableEntity? obj) {
        return new TEntity(); // TODO => Add all properties
    }

}