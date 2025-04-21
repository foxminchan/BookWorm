using System.Text.Json;
using Azure.Data.Tables;

namespace BookWorm.Notification.Infrastructure.Table;

public sealed class TableService(TableServiceClient client) : ITableService
{
    private readonly string _tableName = nameof(Notification).ToLower();

    public async Task<Guid> UpsertAsync<T>(
        T entity,
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var tableClient = client.GetTableClient(_tableName);

        await tableClient.CreateIfNotExistsAsync(cancellationToken);

        var idProperty = typeof(T).GetProperty("Id");

        var entityId = idProperty?.GetValue(entity) is Guid id ? id : Guid.CreateVersion7();

        var tableEntity = new TableEntity(partitionKey, entityId.ToString())
        {
            { nameof(T).ToLower(), JsonSerializer.Serialize(entity) },
        };

        await tableClient.UpsertEntityAsync(tableEntity, cancellationToken: cancellationToken);

        return entityId;
    }

    public async Task<IEnumerable<T>> ListAsync<T>(
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var tableClient = client.GetTableClient(_tableName);

        var tableEntity = tableClient.QueryAsync<TableEntity>(
            TableClient.CreateQueryFilter($"{nameof(TableEntity.PartitionKey)} eq {partitionKey}"),
            cancellationToken: cancellationToken
        );

        var entities = new List<T>();

        await foreach (var entity in tableEntity)
        {
            var json = entity[nameof(T).ToLower()].ToString();

            if (json is null)
            {
                continue;
            }

            var item = JsonSerializer.Deserialize<T>(json);
            if (item is not null)
            {
                entities.Add(item);
            }
        }

        return entities;
    }

    public async Task DeleteAsync(
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default
    )
    {
        var tableClient = client.GetTableClient(_tableName);

        await tableClient.DeleteEntityAsync(
            partitionKey,
            rowKey,
            cancellationToken: cancellationToken
        );
    }
}
